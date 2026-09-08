using Inventario.Application.Abstractions;
using Inventario.Application.Common;
using Inventario.Application.Movimientos;
using Inventario.Domain.Conteos;
using Inventario.Domain.Movimientos;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Application.Conteos;

public sealed class ConteosService(
    IAppDbContext db,
    ICurrentUser currentUser,
    MotorExistencias motor,
    FoliosService folios)
{
    public async Task<ConteoDto> IniciarAsync(IniciarConteoRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!await db.Sucursales.AnyAsync(s => s.Id == request.SucursalId, ct))
        {
            throw new NoEncontradoException("La sucursal indicada no existe.");
        }
        if (request.CategoriaId is { } catId && !await db.Categorias.AnyAsync(c => c.Id == catId, ct))
        {
            throw new NoEncontradoException("La categoría indicada no existe.");
        }

        var abierto = await db.Conteos.AnyAsync(
            c => c.SucursalId == request.SucursalId && c.Estado == EstadoConteo.EnProgreso, ct);
        if (abierto)
        {
            throw new ConflictoException("Ya hay un conteo en progreso para esa sucursal.");
        }

        var existencias = db.Existencias.AsNoTracking().Where(e => e.SucursalId == request.SucursalId);
        if (request.CategoriaId is { } categoriaId)
        {
            existencias = existencias.Where(e => e.Articulo.CategoriaId == categoriaId);
        }

        var snapshot = await existencias
            .Select(e => new { e.ArticuloId, e.Cantidad })
            .ToListAsync(ct);

        var conteo = new Conteo
        {
            Folio = await folios.SiguienteMovimientoAsync(ct),
            SucursalId = request.SucursalId,
            CategoriaId = request.CategoriaId,
            Estado = EstadoConteo.EnProgreso,
            UsuarioId = currentUser.UsuarioId ?? Guid.Empty,
            UsuarioNombre = await NombreUsuarioAsync(ct),
        };

        foreach (var item in snapshot)
        {
            conteo.Detalles.Add(new ConteoDetalle
            {
                TenantId = conteo.TenantId,
                ArticuloId = item.ArticuloId,
                CantidadSistema = item.Cantidad,
            });
        }

        db.Conteos.Add(conteo);
        await db.SaveChangesAsync(ct);
        return await ObtenerAsync(conteo.Id, ct);
    }

    public async Task<ConteoDto> CapturarAsync(Guid id, CapturaConteoRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var conteo = await CargarAsync(id, ct);
        ExigirEnProgreso(conteo);

        var porArticulo = conteo.Detalles.ToDictionary(d => d.ArticuloId);
        foreach (var renglon in request.Renglones)
        {
            if (renglon.CantidadContada < 0)
            {
                throw new ValidacionException("La cantidad contada no puede ser negativa.");
            }
            if (porArticulo.TryGetValue(renglon.ArticuloId, out var detalle))
            {
                detalle.CantidadContada = renglon.CantidadContada;
            }
            else
            {
                // Artículo no incluido en el snapshot (existencia cero): se agrega.
                conteo.Detalles.Add(new ConteoDetalle
                {
                    TenantId = conteo.TenantId,
                    ArticuloId = renglon.ArticuloId,
                    CantidadSistema = 0,
                    CantidadContada = renglon.CantidadContada,
                });
            }
        }

        await db.SaveChangesAsync(ct);
        return await ObtenerAsync(id, ct);
    }

    public async Task<ConteoDto> ConciliarAsync(Guid id, CancellationToken ct)
    {
        var conteo = await CargarAsync(id, ct);
        ExigirEnProgreso(conteo);

        var contados = conteo.Detalles.Where(d => d.CantidadContada is not null).ToList();
        if (contados.Count == 0)
        {
            throw new ValidacionException("No hay renglones contados para conciliar.");
        }

        Movimiento? ajuste = null;

        foreach (var detalle in contados)
        {
            var existencia = await motor.ObtenerOCrearAsync(detalle.ArticuloId, conteo.SucursalId, ct);
            var contada = detalle.CantidadContada!.Value;
            if (existencia.Cantidad == contada)
            {
                continue;
            }

            ajuste ??= new Movimiento
            {
                Folio = await folios.SiguienteMovimientoAsync(ct),
                Tipo = TipoMovimiento.AjusteInventario,
                Fecha = DateTimeOffset.UtcNow,
                SucursalId = conteo.SucursalId,
                Motivo = $"Conciliación de conteo #{conteo.Folio}",
                UsuarioId = currentUser.UsuarioId ?? Guid.Empty,
                UsuarioNombre = conteo.UsuarioNombre,
            };

            var (renglon, _) = await motor.FijarAsync(
                detalle.ArticuloId, conteo.SucursalId, contada, existencia.CostoPromedio, ct);
            ajuste.Renglones.Add(renglon);
        }

        if (ajuste is not null)
        {
            db.Movimientos.Add(ajuste);
            conteo.MovimientoAjusteId = ajuste.Id;
        }

        conteo.Estado = EstadoConteo.Conciliado;
        conteo.ConciliadoEn = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        return await ObtenerAsync(id, ct);
    }

    public async Task<ConteoDto> CancelarAsync(Guid id, CancellationToken ct)
    {
        var conteo = await CargarAsync(id, ct);
        ExigirEnProgreso(conteo);
        conteo.Estado = EstadoConteo.Cancelado;
        await db.SaveChangesAsync(ct);
        return await ObtenerAsync(id, ct);
    }

    public async Task<IReadOnlyList<ConteoListaDto>> ListarAsync(
        Guid? sucursalId, EstadoConteo? estado, CancellationToken ct)
    {
        var query = db.Conteos.AsNoTracking();
        if (sucursalId is { } id)
        {
            query = query.Where(c => c.SucursalId == id);
        }
        if (estado is { } e)
        {
            query = query.Where(c => c.Estado == e);
        }

        return await query
            .OrderByDescending(c => c.Folio)
            .Select(c => new ConteoListaDto(
                c.Id,
                c.Folio,
                c.Estado,
                c.Sucursal.Nombre,
                c.Categoria != null ? c.Categoria.Nombre : null,
                c.CreadoEn,
                c.UsuarioNombre,
                c.Detalles.Count,
                c.Detalles.Count(d => d.CantidadContada != null)))
            .ToListAsync(ct);
    }

    public async Task<ConteoDto> ObtenerAsync(Guid id, CancellationToken ct)
    {
        var c = await db.Conteos
            .AsNoTracking()
            .Include(x => x.Sucursal)
            .Include(x => x.Categoria)
            .Include(x => x.Detalles).ThenInclude(d => d.Articulo)
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new NoEncontradoException("Conteo no encontrado.");

        var detalles = c.Detalles
            .OrderBy(d => d.Articulo.Nombre)
            .Select(d => new ConteoDetalleDto(
                d.ArticuloId,
                d.Articulo.Sku,
                d.Articulo.Nombre,
                d.CantidadSistema,
                d.CantidadContada,
                (d.CantidadContada ?? d.CantidadSistema) - d.CantidadSistema))
            .ToList();

        var contados = detalles.Count(d => d.CantidadContada is not null);
        var conDiferencia = detalles.Count(d => d.CantidadContada is not null && d.Diferencia != 0);
        var diferenciaNeta = detalles.Where(d => d.CantidadContada is not null).Sum(d => d.Diferencia);

        return new ConteoDto(
            c.Id, c.Folio, c.Estado, c.SucursalId, c.Sucursal.Nombre,
            c.CategoriaId, c.Categoria?.Nombre, c.CreadoEn, c.ConciliadoEn,
            c.UsuarioNombre, c.MovimientoAjusteId,
            contados, conDiferencia, diferenciaNeta, detalles);
    }

    private async Task<Conteo> CargarAsync(Guid id, CancellationToken ct) =>
        await db.Conteos.Include(c => c.Detalles).FirstOrDefaultAsync(c => c.Id == id, ct)
        ?? throw new NoEncontradoException("Conteo no encontrado.");

    private static void ExigirEnProgreso(Conteo conteo)
    {
        if (conteo.Estado != EstadoConteo.EnProgreso)
        {
            throw new ValidacionException($"El conteo está '{conteo.Estado}', no se puede modificar.");
        }
    }

    private async Task<string> NombreUsuarioAsync(CancellationToken ct)
    {
        var id = currentUser.UsuarioId;
        if (id is null)
        {
            return "sistema";
        }
        return await db.Usuarios.Where(u => u.Id == id).Select(u => u.NombreCompleto).FirstOrDefaultAsync(ct)
            ?? "desconocido";
    }
}
