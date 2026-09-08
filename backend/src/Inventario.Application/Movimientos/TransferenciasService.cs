using Inventario.Application.Abstractions;
using Inventario.Application.Common;
using Inventario.Domain.Movimientos;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Application.Movimientos;

/// <summary>
/// Transferencias entre sucursales: solicitada → en tránsito → recibida.
/// - Solicitar: solo crea la solicitud, no mueve existencia.
/// - Enviar: aplica la salida en la sucursal origen (al costo promedio de origen).
/// - Recibir: aplica la entrada en destino por la cantidad recibida, al costo de la
///   transferencia. Si se recibe menos de lo enviado, la diferencia es una merma
///   implícita (origen ya la descontó, destino no la sumó).
/// - Cancelar: si estaba en tránsito, devuelve la existencia al origen.
/// </summary>
public sealed class TransferenciasService(
    IAppDbContext db,
    ICurrentUser currentUser,
    MotorExistencias motor,
    FoliosService folios)
{
    public async Task<TransferenciaDto> SolicitarAsync(
        SolicitarTransferenciaRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.SucursalOrigenId == request.SucursalDestinoId)
        {
            throw new ValidacionException("El origen y el destino deben ser distintos.");
        }
        if (request.Renglones is null || request.Renglones.Count == 0)
        {
            throw new ValidacionException("La transferencia debe tener al menos un artículo.");
        }
        await ValidarSucursalAsync(request.SucursalOrigenId, ct);
        await ValidarSucursalAsync(request.SucursalDestinoId, ct);

        var movimiento = new Movimiento
        {
            Folio = await folios.SiguienteMovimientoAsync(ct),
            Tipo = TipoMovimiento.TransferenciaSalida,
            Fecha = DateTimeOffset.UtcNow,
            SucursalId = request.SucursalOrigenId,
            SucursalDestinoId = request.SucursalDestinoId,
            EstadoTransferencia = EstadoTransferencia.Solicitada,
            Motivo = string.IsNullOrWhiteSpace(request.Motivo) ? null : request.Motivo.Trim(),
            UsuarioId = currentUser.UsuarioId ?? Guid.Empty,
            UsuarioNombre = await NombreUsuarioAsync(ct),
        };

        foreach (var renglon in request.Renglones)
        {
            if (renglon.Cantidad <= 0)
            {
                throw new ValidacionException("Las cantidades deben ser positivas.");
            }
            if (!await db.Articulos.AnyAsync(a => a.Id == renglon.ArticuloId, ct))
            {
                throw new NoEncontradoException("Uno de los artículos no existe.");
            }
            movimiento.Renglones.Add(new MovimientoRenglon
            {
                TenantId = movimiento.TenantId,
                ArticuloId = renglon.ArticuloId,
                Cantidad = renglon.Cantidad,
                CostoUnitario = 0,
            });
        }

        db.Movimientos.Add(movimiento);
        await db.SaveChangesAsync(ct);
        return await ObtenerAsync(movimiento.Id, ct);
    }

    public async Task<TransferenciaDto> EnviarAsync(Guid id, CancellationToken ct)
    {
        var transferencia = await CargarAsync(id, ct);
        Exigir(transferencia, EstadoTransferencia.Solicitada, "enviar");

        foreach (var renglon in transferencia.Renglones)
        {
            var aplicado = await motor.AplicarAsync(
                renglon.ArticuloId, transferencia.SucursalId, -renglon.Cantidad, null, ct);
            renglon.CostoUnitario = aplicado.CostoUnitario;
            renglon.CantidadResultante = aplicado.CantidadResultante;
            renglon.CostoPromedioResultante = aplicado.CostoPromedioResultante;
        }

        transferencia.EstadoTransferencia = EstadoTransferencia.EnTransito;
        await db.SaveChangesAsync(ct);
        return await ObtenerAsync(id, ct);
    }

    public async Task<TransferenciaDto> RecibirAsync(
        Guid id, RecibirTransferenciaRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var transferencia = await CargarAsync(id, ct);
        Exigir(transferencia, EstadoTransferencia.EnTransito, "recibir");

        var recibidas = request.Renglones.ToDictionary(r => r.ArticuloId, r => r.CantidadRecibida);

        var entrada = new Movimiento
        {
            Folio = await folios.SiguienteMovimientoAsync(ct),
            Tipo = TipoMovimiento.TransferenciaEntrada,
            Fecha = DateTimeOffset.UtcNow,
            SucursalId = transferencia.SucursalDestinoId!.Value,
            SucursalDestinoId = transferencia.SucursalId,
            EstadoTransferencia = EstadoTransferencia.Recibida,
            MovimientoRelacionadoId = transferencia.Id,
            Motivo = $"Recepción de transferencia #{transferencia.Folio}",
            UsuarioId = currentUser.UsuarioId ?? Guid.Empty,
            UsuarioNombre = await NombreUsuarioAsync(ct),
        };

        foreach (var renglon in transferencia.Renglones)
        {
            var cantidad = recibidas.TryGetValue(renglon.ArticuloId, out var r) ? r : renglon.Cantidad;
            if (cantidad < 0)
            {
                throw new ValidacionException("La cantidad recibida no puede ser negativa.");
            }
            if (cantidad > renglon.Cantidad)
            {
                throw new ValidacionException("No se puede recibir más de lo enviado.");
            }
            if (cantidad == 0)
            {
                continue;
            }

            var aplicado = await motor.AplicarAsync(
                renglon.ArticuloId, entrada.SucursalId, cantidad, renglon.CostoUnitario, ct);
            entrada.Renglones.Add(aplicado);
        }

        transferencia.EstadoTransferencia = EstadoTransferencia.Recibida;
        transferencia.MovimientoRelacionadoId = entrada.Id;
        db.Movimientos.Add(entrada);
        await db.SaveChangesAsync(ct);
        return await ObtenerAsync(id, ct);
    }

    public async Task<TransferenciaDto> CancelarAsync(Guid id, string? motivo, CancellationToken ct)
    {
        var transferencia = await CargarAsync(id, ct);

        if (transferencia.EstadoTransferencia is not
            (EstadoTransferencia.Solicitada or EstadoTransferencia.EnTransito))
        {
            throw new ValidacionException("Solo se puede cancelar una transferencia solicitada o en tránsito.");
        }

        if (transferencia.EstadoTransferencia == EstadoTransferencia.EnTransito)
        {
            // Devolver la existencia al origen.
            foreach (var renglon in transferencia.Renglones)
            {
                await motor.AplicarAsync(
                    renglon.ArticuloId, transferencia.SucursalId, renglon.Cantidad, renglon.CostoUnitario, ct);
            }
        }

        transferencia.EstadoTransferencia = EstadoTransferencia.Cancelada;
        if (!string.IsNullOrWhiteSpace(motivo))
        {
            transferencia.Motivo = $"{transferencia.Motivo} · Cancelada: {motivo.Trim()}".Trim(' ', '·');
        }
        await db.SaveChangesAsync(ct);
        return await ObtenerAsync(id, ct);
    }

    public async Task<ResultadoPaginado<TransferenciaListaDto>> ListarAsync(
        Guid? sucursalId, EstadoTransferencia? estado, int pagina, int tamano, CancellationToken ct)
    {
        pagina = Math.Max(1, pagina);
        tamano = Math.Clamp(tamano, 1, 100);

        var query = db.Movimientos.AsNoTracking()
            .Where(m => m.Tipo == TipoMovimiento.TransferenciaSalida);

        if (sucursalId is { } id)
        {
            query = query.Where(m => m.SucursalId == id || m.SucursalDestinoId == id);
        }
        if (estado is { } e)
        {
            query = query.Where(m => m.EstadoTransferencia == e);
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(m => m.Folio)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .Select(m => new TransferenciaListaDto(
                m.Id,
                m.Folio,
                m.EstadoTransferencia!.Value,
                m.Fecha,
                m.Sucursal.Nombre,
                m.SucursalDestino!.Nombre,
                m.Renglones.Count,
                m.UsuarioNombre))
            .ToListAsync(ct);

        return new ResultadoPaginado<TransferenciaListaDto>(items, total, pagina, tamano);
    }

    public async Task<TransferenciaDto> ObtenerAsync(Guid id, CancellationToken ct)
    {
        var m = await db.Movimientos
            .AsNoTracking()
            .Include(x => x.Sucursal)
            .Include(x => x.SucursalDestino)
            .Include(x => x.Renglones).ThenInclude(r => r.Articulo)
            .FirstOrDefaultAsync(x => x.Id == id && x.Tipo == TipoMovimiento.TransferenciaSalida, ct)
            ?? throw new NoEncontradoException("Transferencia no encontrada.");

        var recepcion = m.MovimientoRelacionadoId is null
            ? null
            : await db.Movimientos.AsNoTracking()
                .Include(x => x.Renglones)
                .FirstOrDefaultAsync(x => x.Id == m.MovimientoRelacionadoId, ct);

        var recibidasPorArticulo = recepcion?.Renglones
            .GroupBy(r => r.ArticuloId)
            .ToDictionary(g => g.Key, g => g.Sum(r => r.Cantidad));

        return new TransferenciaDto(
            m.Id, m.Folio, m.EstadoTransferencia!.Value, m.Fecha,
            m.SucursalId, m.Sucursal.Nombre,
            m.SucursalDestinoId!.Value, m.SucursalDestino!.Nombre,
            m.Motivo, m.UsuarioNombre, recepcion?.Id,
            m.Renglones
                .OrderBy(r => r.Articulo.Nombre)
                .Select(r => new RenglonTransferenciaDto(
                    r.ArticuloId, r.Articulo.Sku, r.Articulo.Nombre,
                    r.Cantidad,
                    recibidasPorArticulo?.GetValueOrDefault(r.ArticuloId),
                    r.CostoUnitario))
                .ToList());
    }

    private async Task<Movimiento> CargarAsync(Guid id, CancellationToken ct) =>
        await db.Movimientos
            .Include(m => m.Renglones)
            .FirstOrDefaultAsync(m => m.Id == id && m.Tipo == TipoMovimiento.TransferenciaSalida, ct)
            ?? throw new NoEncontradoException("Transferencia no encontrada.");

    private static void Exigir(Movimiento t, EstadoTransferencia esperado, string accion)
    {
        if (t.EstadoTransferencia != esperado)
        {
            throw new ValidacionException(
                $"Para {accion} la transferencia debe estar '{esperado}', y está '{t.EstadoTransferencia}'.");
        }
    }

    private async Task ValidarSucursalAsync(Guid sucursalId, CancellationToken ct)
    {
        if (!await db.Sucursales.AnyAsync(s => s.Id == sucursalId, ct))
        {
            throw new NoEncontradoException("Una de las sucursales no existe.");
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
