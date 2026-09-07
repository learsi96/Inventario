using Inventario.Application.Abstractions;
using Inventario.Application.Common;
using Inventario.Application.Movimientos;
using Inventario.Domain.Existencias;
using Inventario.Domain.Movimientos;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Application.Existencias;

public sealed class ExistenciasService(
    IAppDbContext db,
    ICurrentUser currentUser,
    MotorExistencias motor,
    FoliosService folios)
{
    private const int TamanoMaximo = 100;

    public async Task<ResultadoPaginado<ExistenciaListaDto>> ListarAsync(
        FiltroExistencias filtro, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(filtro);
        var pagina = Math.Max(1, filtro.Pagina);
        var tamano = Math.Clamp(filtro.Tamano, 1, TamanoMaximo);

        var query = db.Existencias.AsNoTracking();

        if (filtro.SucursalId is { } sucursalId)
        {
            query = query.Where(e => e.SucursalId == sucursalId);
        }
        if (filtro.CategoriaId is { } categoriaId)
        {
            query = query.Where(e => e.Articulo.CategoriaId == categoriaId);
        }
        if (filtro.SoloBajoMinimo)
        {
            query = query.Where(e => e.Minimo > 0 && e.Cantidad <= e.Minimo);
        }
        if (!string.IsNullOrWhiteSpace(filtro.Texto))
        {
            var t = filtro.Texto.Trim();
            query = query.Where(e => e.Articulo.Sku.Contains(t) || e.Articulo.Nombre.Contains(t));
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(e => e.Articulo.Nombre)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .Select(e => new ExistenciaListaDto(
                e.ArticuloId,
                e.Articulo.Sku,
                e.Articulo.Nombre,
                e.SucursalId,
                e.Sucursal.Nombre,
                e.Cantidad,
                e.Articulo.UnidadMedida.Codigo,
                e.CostoPromedio,
                e.Cantidad * e.CostoPromedio,
                e.Minimo,
                e.Minimo > 0 && e.Cantidad <= e.Minimo))
            .ToListAsync(ct);

        return new ResultadoPaginado<ExistenciaListaDto>(items, total, pagina, tamano);
    }

    public async Task<ExistenciaDto> ObtenerAsync(Guid articuloId, Guid sucursalId, CancellationToken ct)
    {
        var e = await db.Existencias
            .AsNoTracking()
            .Include(x => x.Articulo)
            .Include(x => x.Sucursal)
            .Include(x => x.PorUbicacion).ThenInclude(pu => pu.Ubicacion)
            .FirstOrDefaultAsync(x => x.ArticuloId == articuloId && x.SucursalId == sucursalId, ct);

        if (e is null)
        {
            var articulo = await db.Articulos.AsNoTracking().FirstOrDefaultAsync(a => a.Id == articuloId, ct)
                ?? throw new NoEncontradoException("Artículo no encontrado.");
            var sucursal = await db.Sucursales.AsNoTracking().FirstOrDefaultAsync(s => s.Id == sucursalId, ct)
                ?? throw new NoEncontradoException("Sucursal no encontrada.");

            return new ExistenciaDto(
                articuloId, articulo.Sku, articulo.Nombre, sucursalId, sucursal.Nombre,
                0, 0, 0, 0, 0, 0, false, []);
        }

        return new ExistenciaDto(
            e.ArticuloId, e.Articulo.Sku, e.Articulo.Nombre, e.SucursalId, e.Sucursal.Nombre,
            e.Cantidad, e.CostoPromedio, e.Cantidad * e.CostoPromedio,
            e.Minimo, e.Maximo, e.PuntoReorden, e.Minimo > 0 && e.Cantidad <= e.Minimo,
            e.PorUbicacion
                .Select(pu => new ExistenciaUbicacionDto(pu.UbicacionId, pu.Ubicacion.Codigo, pu.Cantidad))
                .ToList());
    }

    public async Task<ExistenciaDto> AjustarAsync(AjusteExistenciaRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.Motivo))
        {
            throw new ValidacionException("El motivo del ajuste es obligatorio.");
        }

        var (renglon, _) = await motor.FijarAsync(
            request.ArticuloId, request.SucursalId, request.Cantidad, request.CostoPromedio, ct);

        var movimiento = new Movimiento
        {
            Folio = await folios.SiguienteMovimientoAsync(ct),
            Tipo = TipoMovimiento.AjusteInventario,
            Fecha = DateTimeOffset.UtcNow,
            SucursalId = request.SucursalId,
            Motivo = request.Motivo.Trim(),
            UsuarioId = currentUser.UsuarioId ?? Guid.Empty,
            UsuarioNombre = await NombreUsuarioAsync(ct),
        };
        movimiento.Renglones.Add(renglon);
        db.Movimientos.Add(movimiento);

        await db.SaveChangesAsync(ct);
        return await ObtenerAsync(request.ArticuloId, request.SucursalId, ct);
    }

    public async Task<ExistenciaDto> GuardarParametrosAsync(ParametrosReordenRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.Minimo < 0 || request.Maximo < 0 || request.PuntoReorden < 0)
        {
            throw new ValidacionException("Los parámetros no pueden ser negativos.");
        }
        if (request.Maximo > 0 && request.Maximo < request.Minimo)
        {
            throw new ValidacionException("El máximo no puede ser menor que el mínimo.");
        }

        var existencia = await motor.ObtenerOCrearAsync(request.ArticuloId, request.SucursalId, ct);
        existencia.Minimo = request.Minimo;
        existencia.Maximo = request.Maximo;
        existencia.PuntoReorden = request.PuntoReorden;

        await db.SaveChangesAsync(ct);
        return await ObtenerAsync(request.ArticuloId, request.SucursalId, ct);
    }

    public async Task<ExistenciaDto> AsignarUbicacionesAsync(
        AsignarUbicacionesRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        var existencia = await db.Existencias
            .FirstOrDefaultAsync(
                e => e.ArticuloId == request.ArticuloId && e.SucursalId == request.SucursalId, ct)
            ?? throw new NoEncontradoException("No hay existencia para ese artículo en esa sucursal.");

        var ubicacionesValidas = await db.Ubicaciones
            .Where(u => u.SucursalId == request.SucursalId)
            .Select(u => u.Id)
            .ToListAsync(ct);

        var nuevas = request.Asignaciones
            .Where(a => a.Cantidad > 0)
            .Select(a =>
            {
                if (!ubicacionesValidas.Contains(a.UbicacionId))
                {
                    throw new NoEncontradoException("Una de las ubicaciones no pertenece a la sucursal.");
                }
                return new ExistenciaUbicacion
                {
                    TenantId = existencia.TenantId,
                    ExistenciaId = existencia.Id,
                    UbicacionId = a.UbicacionId,
                    Cantidad = a.Cantidad,
                };
            })
            .ToList();

        await db.ExistenciasUbicacion
            .Where(pu => pu.ExistenciaId == existencia.Id)
            .ExecuteDeleteAsync(ct);

        db.ExistenciasUbicacion.AddRange(nuevas);
        await db.SaveChangesAsync(ct);
        return await ObtenerAsync(request.ArticuloId, request.SucursalId, ct);
    }

    public async Task<ValorizacionDto> ValorizacionAsync(Guid? sucursalId, CancellationToken ct)
    {
        var query = db.Existencias.AsNoTracking().Where(e => e.Cantidad != 0);
        if (sucursalId is { } id)
        {
            query = query.Where(e => e.SucursalId == id);
        }

        var agregados = await query
            .GroupBy(e => e.SucursalId)
            .Select(g => new
            {
                SucursalId = g.Key,
                Articulos = g.Count(),
                Unidades = g.Sum(e => e.Cantidad),
                Valor = g.Sum(e => e.Cantidad * e.CostoPromedio),
            })
            .ToListAsync(ct);

        var ids = agregados.Select(a => a.SucursalId).ToList();
        var nombres = await db.Sucursales.AsNoTracking()
            .Where(s => ids.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => s.Nombre, ct);

        var porSucursal = agregados
            .Select(a => new ValorizacionSucursalDto(
                a.SucursalId,
                nombres.GetValueOrDefault(a.SucursalId, "?"),
                a.Articulos,
                a.Unidades,
                a.Valor))
            .OrderBy(v => v.SucursalNombre)
            .ToList();

        return new ValorizacionDto(
            porSucursal.Sum(v => v.Valor),
            porSucursal.Sum(v => v.Unidades),
            porSucursal.Sum(v => v.Articulos),
            porSucursal);
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
