using System.Globalization;
using Inventario.Application.Abstractions;
using Inventario.Application.Conteos;
using Inventario.Application.Existencias;
using Inventario.Application.Movimientos;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Application.Reportes;

public sealed class ReportesService(
    IAppDbContext db,
    ExistenciasService existencias,
    MovimientosService movimientos,
    ConteosService conteos,
    IGeneradorReporte generador)
{
    private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-MX");

    public async Task<ArchivoReporte> ExistenciasAsync(
        FiltroExistencias filtro, FormatoReporte formato, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(filtro);
        var pagina = await existencias.ListarAsync(filtro with { Pagina = 1, Tamano = 100 }, ct);
        var todas = new List<ExistenciaListaDto>(pagina.Items);
        var totalPaginas = (int)Math.Ceiling(pagina.Total / 100.0);
        for (var p = 2; p <= totalPaginas; p++)
        {
            todas.AddRange((await existencias.ListarAsync(filtro with { Pagina = p, Tamano = 100 }, ct)).Items);
        }

        var reporte = new ReporteTabular(
            "Existencias",
            await SubtituloSucursalAsync(filtro.SucursalId, ct),
            [
                new("SKU"), new("Artículo"), new("Sucursal"),
                new("Cantidad", true), new("Unidad"),
                new("Costo prom.", true), new("Valor", true), new("Mínimo", true),
            ],
            todas.Select(e => (IReadOnlyList<string>)
                [
                    e.Sku, e.ArticuloNombre, e.SucursalNombre,
                    N(e.Cantidad), e.UnidadCodigo,
                    N(e.CostoPromedio), N(e.Valor), N(e.Minimo),
                ]).ToList(),
            ["", "", "Total", "", "", "", N(todas.Sum(e => e.Valor)), ""]);

        return generador.Generar(reporte, formato);
    }

    public async Task<ArchivoReporte> ValorizacionAsync(
        Guid? sucursalId, FormatoReporte formato, CancellationToken ct)
    {
        var v = await existencias.ValorizacionAsync(sucursalId, ct);

        var reporte = new ReporteTabular(
            "Valorización del inventario",
            await SubtituloSucursalAsync(sucursalId, ct),
            [new("Sucursal"), new("Artículos", true), new("Unidades", true), new("Valor", true)],
            v.PorSucursal.Select(s => (IReadOnlyList<string>)
                [s.SucursalNombre, s.Articulos.ToString(Cultura), N(s.Unidades), N(s.Valor)]).ToList(),
            ["Total", v.ArticulosConExistencia.ToString(Cultura), N(v.UnidadesTotal), N(v.ValorTotal)]);

        return generador.Generar(reporte, formato);
    }

    public async Task<ArchivoReporte> KardexAsync(
        Guid articuloId, Guid? sucursalId, FormatoReporte formato, CancellationToken ct)
    {
        var lineas = await movimientos.KardexAsync(articuloId, sucursalId, null, null, ct);
        var articulo = await db.Articulos.AsNoTracking()
            .Where(a => a.Id == articuloId)
            .Select(a => new { a.Sku, a.Nombre })
            .FirstOrDefaultAsync(ct)
            ?? throw new Common.NoEncontradoException("Artículo no encontrado.");

        var reporte = new ReporteTabular(
            $"Kardex — {articulo.Sku} {articulo.Nombre}",
            await SubtituloSucursalAsync(sucursalId, ct),
            [
                new("Folio"), new("Fecha"), new("Tipo"), new("Motivo"),
                new("Cantidad", true), new("Costo unit.", true),
                new("Existencia", true), new("Costo prom.", true),
            ],
            lineas.Select(k => (IReadOnlyList<string>)
                [
                    $"#{k.Folio}",
                    k.Fecha.ToLocalTime().ToString("g", Cultura),
                    k.Tipo.ToString(),
                    k.Motivo ?? "",
                    N(k.Cantidad), N(k.CostoUnitario),
                    N(k.CantidadResultante), N(k.CostoPromedioResultante),
                ]).ToList());

        return generador.Generar(reporte, formato);
    }

    public async Task<ArchivoReporte> DiferenciasConteoAsync(
        Guid conteoId, FormatoReporte formato, CancellationToken ct)
    {
        var c = await conteos.ObtenerAsync(conteoId, ct);

        var reporte = new ReporteTabular(
            $"Diferencias de conteo #{c.Folio}",
            $"{c.SucursalNombre} · {c.CategoriaNombre ?? "Todo el inventario"} · {c.Estado}",
            [
                new("SKU"), new("Artículo"),
                new("Sistema", true), new("Contado", true), new("Diferencia", true),
            ],
            c.Detalles
                .Where(d => d.CantidadContada is not null)
                .Select(d => (IReadOnlyList<string>)
                    [
                        d.ArticuloSku, d.ArticuloNombre,
                        N(d.CantidadSistema), N(d.CantidadContada!.Value), N(d.Diferencia),
                    ]).ToList(),
            ["", "Diferencia neta", "", "", N(c.DiferenciaNeta)]);

        return generador.Generar(reporte, formato);
    }

    private async Task<string?> SubtituloSucursalAsync(Guid? sucursalId, CancellationToken ct)
    {
        if (sucursalId is not { } id)
        {
            return "Todas las sucursales";
        }
        var nombre = await db.Sucursales.AsNoTracking()
            .Where(s => s.Id == id).Select(s => s.Nombre).FirstOrDefaultAsync(ct);
        return nombre is null ? null : $"Sucursal: {nombre}";
    }

    private static string N(decimal valor) => valor.ToString("N2", Cultura);
}
