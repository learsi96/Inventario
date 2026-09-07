using Inventario.Application.Abstractions;
using Inventario.Application.Common;
using Inventario.Domain.Existencias;
using Inventario.Domain.Movimientos;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Application.Movimientos;

/// <summary>
/// Aplica renglones de movimiento a las existencias: mantiene la cantidad y el
/// costo promedio ponderado, y anota el resultado en cada renglón para el kardex.
/// Lo usan los ajustes (Hito 3), los movimientos (Hito 4) y las transferencias
/// (Hito 5). Ver <c>docs/decisions/0006-modelo-de-existencias.md</c>.
/// </summary>
public sealed class MotorExistencias(IAppDbContext db)
{
    /// <summary>
    /// Aplica un renglón a la existencia de (artículo, sucursal). <paramref name="cantidad"/>
    /// positiva entra, negativa sale. En entradas se requiere <paramref name="costoUnitario"/>.
    /// Devuelve el renglón ya con los valores resultantes; el llamador lo añade al movimiento.
    /// </summary>
    public async Task<MovimientoRenglon> AplicarAsync(
        Guid articuloId,
        Guid sucursalId,
        decimal cantidad,
        decimal? costoUnitario,
        CancellationToken ct,
        bool permitirNegativo = false)
    {
        if (cantidad == 0)
        {
            throw new ValidacionException("La cantidad del renglón no puede ser cero.");
        }

        var existencia = await ObtenerOCrearAsync(articuloId, sucursalId, ct);

        decimal costoRenglon;
        if (cantidad > 0)
        {
            costoRenglon = costoUnitario
                ?? throw new ValidacionException("Las entradas requieren costo unitario.");
            if (costoRenglon < 0)
            {
                throw new ValidacionException("El costo unitario no puede ser negativo.");
            }

            var cantidadTotal = existencia.Cantidad + cantidad;
            existencia.CostoPromedio = cantidadTotal == 0
                ? costoRenglon
                : ((existencia.Cantidad * existencia.CostoPromedio) + (cantidad * costoRenglon)) / cantidadTotal;
            existencia.Cantidad = cantidadTotal;
        }
        else
        {
            var resultante = existencia.Cantidad + cantidad;
            if (resultante < 0 && !permitirNegativo)
            {
                throw new ValidacionException(
                    $"Existencia insuficiente: hay {existencia.Cantidad}, se intentan sacar {-cantidad}.");
            }
            costoRenglon = existencia.CostoPromedio;
            existencia.Cantidad = resultante;
        }

        return new MovimientoRenglon
        {
            TenantId = existencia.TenantId,
            ArticuloId = articuloId,
            Cantidad = cantidad,
            CostoUnitario = costoRenglon,
            CantidadResultante = existencia.Cantidad,
            CostoPromedioResultante = existencia.CostoPromedio,
        };
    }

    /// <summary>Fija la existencia de (artículo, sucursal) a valores absolutos (ajuste/carga inicial).</summary>
    public async Task<(MovimientoRenglon Renglon, Existencia Existencia)> FijarAsync(
        Guid articuloId, Guid sucursalId, decimal cantidad, decimal costoPromedio, CancellationToken ct)
    {
        if (cantidad < 0)
        {
            throw new ValidacionException("La cantidad no puede ser negativa.");
        }
        if (costoPromedio < 0)
        {
            throw new ValidacionException("El costo no puede ser negativo.");
        }

        var existencia = await ObtenerOCrearAsync(articuloId, sucursalId, ct);
        var delta = cantidad - existencia.Cantidad;

        existencia.Cantidad = cantidad;
        existencia.CostoPromedio = costoPromedio;

        var renglon = new MovimientoRenglon
        {
            TenantId = existencia.TenantId,
            ArticuloId = articuloId,
            Cantidad = delta,
            CostoUnitario = costoPromedio,
            CantidadResultante = cantidad,
            CostoPromedioResultante = costoPromedio,
        };
        return (renglon, existencia);
    }

    public async Task<Existencia> ObtenerOCrearAsync(Guid articuloId, Guid sucursalId, CancellationToken ct)
    {
        // Primero lo ya rastreado (varios renglones del mismo movimiento sobre el mismo artículo).
        var existencia = db.Existencias.Local
            .FirstOrDefault(e => e.ArticuloId == articuloId && e.SucursalId == sucursalId)
            ?? await db.Existencias
                .FirstOrDefaultAsync(e => e.ArticuloId == articuloId && e.SucursalId == sucursalId, ct);

        if (existencia is null)
        {
            if (!await db.Articulos.AnyAsync(a => a.Id == articuloId, ct))
            {
                throw new NoEncontradoException("El artículo indicado no existe.");
            }
            if (!await db.Sucursales.AnyAsync(s => s.Id == sucursalId, ct))
            {
                throw new NoEncontradoException("La sucursal indicada no existe.");
            }

            existencia = new Existencia { ArticuloId = articuloId, SucursalId = sucursalId };
            db.Existencias.Add(existencia);
        }

        return existencia;
    }
}
