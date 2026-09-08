using Inventario.Application.Reportes;
using Inventario.Infrastructure.Reportes;
using Shouldly;

namespace Inventario.UnitTests.Reportes;

public sealed class GeneradorReporteTests
{
    private static readonly ReporteTabular Reporte = new(
        "Existencias",
        "Sucursal: Matriz",
        [new("SKU"), new("Cantidad", true)],
        [["ART-1", "10.00"], ["ART-2", "5.00"]],
        ["Total", "15.00"]);

    [Fact]
    public void Excel_genera_un_archivo_xlsx_no_vacio()
    {
        var archivo = new GeneradorReporte().Generar(Reporte, FormatoReporte.Xlsx);

        archivo.NombreArchivo.ShouldEndWith(".xlsx");
        archivo.Contenido.Length.ShouldBeGreaterThan(500);
        // Firma de un ZIP (los .xlsx lo son).
        archivo.Contenido[0].ShouldBe((byte)'P');
        archivo.Contenido[1].ShouldBe((byte)'K');
    }

    [Fact]
    public void Pdf_genera_un_archivo_pdf_no_vacio()
    {
        var archivo = new GeneradorReporte().Generar(Reporte, FormatoReporte.Pdf);

        archivo.NombreArchivo.ShouldEndWith(".pdf");
        archivo.ContentType.ShouldBe("application/pdf");
        System.Text.Encoding.ASCII.GetString(archivo.Contenido, 0, 5).ShouldBe("%PDF-");
    }
}
