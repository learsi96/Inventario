using Inventario.Application.Abstractions;
using Inventario.Application.Common;
using Inventario.Domain.Movimientos;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Application.Movimientos;

public sealed class MovimientosService(
    IAppDbContext db,
    ICurrentUser currentUser,
    MotorExistencias motor,
    FoliosService folios)
{
    private const int TamanoMaximo = 100;

    public async Task<MovimientoDto> RegistrarEntradaAsync(RegistrarEntradaRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidarRenglones(request.Renglones);
        await ValidarSucursalAsync(request.SucursalId, ct);

        var movimiento = await NuevoMovimientoAsync(
            TipoMovimiento.Entrada, request.SucursalId, request.Motivo, request.Referencia, ct);

        foreach (var renglon in request.Renglones)
        {
            if (renglon.Cantidad <= 0)
            {
                throw new ValidacionException("Las cantidades de la entrada deben ser positivas.");
            }
            var aplicado = await motor.AplicarAsync(
                renglon.ArticuloId, request.SucursalId, renglon.Cantidad, renglon.CostoUnitario, ct);
            movimiento.Renglones.Add(aplicado);
        }

        db.Movimientos.Add(movimiento);
        await db.SaveChangesAsync(ct);
        return await ObtenerAsync(movimiento.Id, ct);
    }

    public async Task<MovimientoDto> RegistrarSalidaAsync(RegistrarSalidaRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidarRenglones(request.Renglones);
        await ValidarSucursalAsync(request.SucursalId, ct);

        if (request.Tipo is not (TipoMovimiento.Salida or TipoMovimiento.Merma))
        {
            throw new ValidacionException("Tipo de salida no válido (usa Salida o Merma).");
        }
        if (string.IsNullOrWhiteSpace(request.Motivo))
        {
            throw new ValidacionException("El motivo es obligatorio.");
        }

        var movimiento = await NuevoMovimientoAsync(
            request.Tipo, request.SucursalId, request.Motivo, null, ct);

        foreach (var renglon in request.Renglones)
        {
            if (renglon.Cantidad <= 0)
            {
                throw new ValidacionException("Las cantidades de la salida deben ser positivas.");
            }
            var aplicado = await motor.AplicarAsync(
                renglon.ArticuloId, request.SucursalId, -renglon.Cantidad, null, ct);
            movimiento.Renglones.Add(aplicado);
        }

        db.Movimientos.Add(movimiento);
        await db.SaveChangesAsync(ct);
        return await ObtenerAsync(movimiento.Id, ct);
    }

    public async Task<ResultadoPaginado<MovimientoListaDto>> ListarAsync(
        FiltroMovimientos filtro, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(filtro);
        var pagina = Math.Max(1, filtro.Pagina);
        var tamano = Math.Clamp(filtro.Tamano, 1, TamanoMaximo);

        var query = db.Movimientos.AsNoTracking();

        if (filtro.SucursalId is { } sucursalId)
        {
            query = query.Where(m => m.SucursalId == sucursalId || m.SucursalDestinoId == sucursalId);
        }
        if (filtro.Tipo is { } tipo)
        {
            query = query.Where(m => m.Tipo == tipo);
        }
        if (filtro.Desde is { } desde)
        {
            query = query.Where(m => m.Fecha >= desde);
        }
        if (filtro.Hasta is { } hasta)
        {
            query = query.Where(m => m.Fecha <= hasta);
        }
        if (!string.IsNullOrWhiteSpace(filtro.Texto))
        {
            var t = filtro.Texto.Trim();
            query = query.Where(m => m.Referencia!.Contains(t) || m.Motivo!.Contains(t));
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(m => m.Folio)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .Select(m => new MovimientoListaDto(
                m.Id,
                m.Folio,
                m.Tipo,
                m.Fecha,
                m.Sucursal.Nombre,
                m.Motivo,
                m.Referencia,
                m.UsuarioNombre,
                m.Renglones.Count,
                m.Renglones.Sum(r => r.Cantidad * r.CostoUnitario)))
            .ToListAsync(ct);

        return new ResultadoPaginado<MovimientoListaDto>(items, total, pagina, tamano);
    }

    public async Task<MovimientoDto> ObtenerAsync(Guid id, CancellationToken ct)
    {
        var m = await db.Movimientos
            .AsNoTracking()
            .Include(x => x.Sucursal)
            .Include(x => x.SucursalDestino)
            .Include(x => x.Renglones).ThenInclude(r => r.Articulo)
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new NoEncontradoException("Movimiento no encontrado.");

        return new MovimientoDto(
            m.Id, m.Folio, m.Tipo, m.Fecha, m.SucursalId, m.Sucursal.Nombre,
            m.SucursalDestinoId, m.SucursalDestino?.Nombre, m.EstadoTransferencia,
            m.Motivo, m.Referencia, m.UsuarioNombre,
            m.Renglones
                .OrderBy(r => r.Articulo.Nombre)
                .Select(r => new RenglonMovimientoDto(
                    r.ArticuloId, r.Articulo.Sku, r.Articulo.Nombre,
                    r.Cantidad, r.CostoUnitario, r.CantidadResultante, r.CostoPromedioResultante))
                .ToList());
    }

    public async Task<IReadOnlyList<KardexRenglonDto>> KardexAsync(
        Guid articuloId, Guid? sucursalId, DateTimeOffset? desde, DateTimeOffset? hasta, CancellationToken ct)
    {
        var query = db.MovimientoRenglones.AsNoTracking().Where(r => r.ArticuloId == articuloId);

        if (sucursalId is { } id)
        {
            query = query.Where(r => r.Movimiento.SucursalId == id);
        }
        if (desde is { } d)
        {
            query = query.Where(r => r.Movimiento.Fecha >= d);
        }
        if (hasta is { } h)
        {
            query = query.Where(r => r.Movimiento.Fecha <= h);
        }

        return await query
            .OrderBy(r => r.Movimiento.Folio)
            .Select(r => new KardexRenglonDto(
                r.MovimientoId,
                r.Movimiento.Folio,
                r.Movimiento.Tipo,
                r.Movimiento.Fecha,
                r.Movimiento.Sucursal.Nombre,
                r.Movimiento.Motivo,
                r.Cantidad,
                r.CostoUnitario,
                r.CantidadResultante,
                r.CostoPromedioResultante))
            .ToListAsync(ct);
    }

    internal async Task<Movimiento> NuevoMovimientoAsync(
        TipoMovimiento tipo, Guid sucursalId, string? motivo, string? referencia, CancellationToken ct) =>
        new()
        {
            Folio = await folios.SiguienteMovimientoAsync(ct),
            Tipo = tipo,
            Fecha = DateTimeOffset.UtcNow,
            SucursalId = sucursalId,
            Motivo = string.IsNullOrWhiteSpace(motivo) ? null : motivo.Trim(),
            Referencia = string.IsNullOrWhiteSpace(referencia) ? null : referencia.Trim(),
            UsuarioId = currentUser.UsuarioId ?? Guid.Empty,
            UsuarioNombre = await NombreUsuarioAsync(ct),
        };

    private static void ValidarRenglones<T>(IReadOnlyList<T>? renglones)
    {
        if (renglones is null || renglones.Count == 0)
        {
            throw new ValidacionException("El movimiento debe tener al menos un renglón.");
        }
    }

    private async Task ValidarSucursalAsync(Guid sucursalId, CancellationToken ct)
    {
        if (!await db.Sucursales.AnyAsync(s => s.Id == sucursalId, ct))
        {
            throw new NoEncontradoException("La sucursal indicada no existe.");
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
