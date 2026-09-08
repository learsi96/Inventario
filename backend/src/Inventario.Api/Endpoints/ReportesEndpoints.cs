using Inventario.Api.Seguridad;
using Inventario.Application.Existencias;
using Inventario.Application.Reportes;

namespace Inventario.Api.Endpoints;

public static class ReportesEndpoints
{
    public static IEndpointRouteBuilder MapReportesEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/reportes")
            .WithTags("Reportes")
            .RequireAuthorization(Politicas.UsuarioPartner);

        grupo.MapGet("/existencias", async (
            string? formato,
            Guid? sucursalId,
            Guid? categoriaId,
            string? texto,
            bool? soloBajoMinimo,
            ReportesService servicio,
            CancellationToken ct) =>
        {
            var filtro = new FiltroExistencias(sucursalId, categoriaId, texto, soloBajoMinimo ?? false);
            var archivo = await servicio.ExistenciasAsync(filtro, Formato(formato), ct);
            return Descarga(archivo);
        })
        .WithSummary("Reporte de existencias (xlsx | pdf).");

        grupo.MapGet("/valorizacion", async (
            string? formato, Guid? sucursalId, ReportesService servicio, CancellationToken ct) =>
            Descarga(await servicio.ValorizacionAsync(sucursalId, Formato(formato), ct)))
            .WithSummary("Reporte de valorización (xlsx | pdf).");

        grupo.MapGet("/kardex/{articuloId:guid}", async (
            Guid articuloId, string? formato, Guid? sucursalId,
            ReportesService servicio, CancellationToken ct) =>
            Descarga(await servicio.KardexAsync(articuloId, sucursalId, Formato(formato), ct)))
            .WithSummary("Reporte de kardex de un artículo (xlsx | pdf).");

        grupo.MapGet("/conteos/{conteoId:guid}/diferencias", async (
            Guid conteoId, string? formato, ReportesService servicio, CancellationToken ct) =>
            Descarga(await servicio.DiferenciasConteoAsync(conteoId, Formato(formato), ct)))
            .WithSummary("Reporte de diferencias de un conteo (xlsx | pdf).");

        return app;
    }

    private static FormatoReporte Formato(string? valor) =>
        string.Equals(valor, "pdf", StringComparison.OrdinalIgnoreCase)
            ? FormatoReporte.Pdf
            : FormatoReporte.Xlsx;

    private static IResult Descarga(ArchivoReporte archivo) =>
        Results.File(archivo.Contenido, archivo.ContentType, archivo.NombreArchivo);
}
