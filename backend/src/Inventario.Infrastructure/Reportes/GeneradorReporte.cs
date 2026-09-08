using ClosedXML.Excel;
using Inventario.Application.Abstractions;
using Inventario.Application.Reportes;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Inventario.Infrastructure.Reportes;

public sealed class GeneradorReporte : IGeneradorReporte
{
    static GeneradorReporte()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public ArchivoReporte Generar(ReporteTabular reporte, FormatoReporte formato)
    {
        ArgumentNullException.ThrowIfNull(reporte);
        var nombre = Sanitizar(reporte.Titulo);

        return formato switch
        {
            FormatoReporte.Xlsx => new ArchivoReporte(
                Excel(reporte),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"{nombre}.xlsx"),
            FormatoReporte.Pdf => new ArchivoReporte(Pdf(reporte), "application/pdf", $"{nombre}.pdf"),
            _ => throw new ArgumentOutOfRangeException(nameof(formato)),
        };
    }

    private static byte[] Excel(ReporteTabular reporte)
    {
        using var libro = new XLWorkbook();
        var hoja = libro.AddWorksheet("Reporte");

        var fila = 1;
        hoja.Cell(fila, 1).Value = reporte.Titulo;
        hoja.Cell(fila, 1).Style.Font.Bold = true;
        hoja.Cell(fila, 1).Style.Font.FontSize = 14;
        fila++;

        if (reporte.Subtitulo is { } subtitulo)
        {
            hoja.Cell(fila, 1).Value = subtitulo;
            fila++;
        }
        fila++;

        var encabezado = fila;
        for (var c = 0; c < reporte.Columnas.Count; c++)
        {
            var celda = hoja.Cell(encabezado, c + 1);
            celda.Value = reporte.Columnas[c].Titulo;
            celda.Style.Font.Bold = true;
            celda.Style.Fill.BackgroundColor = XLColor.LightGray;
        }
        fila++;

        foreach (var renglon in reporte.Filas)
        {
            for (var c = 0; c < renglon.Count; c++)
            {
                hoja.Cell(fila, c + 1).Value = renglon[c];
            }
            fila++;
        }

        if (reporte.TotalesFila is { } totales)
        {
            for (var c = 0; c < totales.Count; c++)
            {
                var celda = hoja.Cell(fila, c + 1);
                celda.Value = totales[c];
                celda.Style.Font.Bold = true;
            }
        }

        hoja.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        libro.SaveAs(ms);
        return ms.ToArray();
    }

    private static byte[] Pdf(ReporteTabular reporte)
    {
        var documento = Document.Create(contenedor =>
        {
            contenedor.Page(pagina =>
            {
                pagina.Size(PageSizes.A4.Landscape());
                pagina.Margin(30);
                pagina.DefaultTextStyle(x => x.FontSize(9));

                pagina.Header().Column(col =>
                {
                    col.Item().Text(reporte.Titulo).FontSize(15).Bold();
                    if (reporte.Subtitulo is { } subtitulo)
                    {
                        col.Item().Text(subtitulo).FontColor(Colors.Grey.Darken1);
                    }
                    col.Item().Text($"Generado: {DateTimeOffset.Now:g}").FontSize(8)
                        .FontColor(Colors.Grey.Medium);
                });

                pagina.Content().PaddingVertical(10).Table(tabla =>
                {
                    tabla.ColumnsDefinition(def =>
                    {
                        foreach (var _ in reporte.Columnas)
                        {
                            def.RelativeColumn();
                        }
                    });

                    tabla.Header(h =>
                    {
                        foreach (var columna in reporte.Columnas)
                        {
                            h.Cell().Background(Colors.Grey.Lighten2).Padding(4)
                                .Text(columna.Titulo).Bold();
                        }
                    });

                    foreach (var renglon in reporte.Filas)
                    {
                        for (var c = 0; c < renglon.Count; c++)
                        {
                            var alineacion = c < reporte.Columnas.Count && reporte.Columnas[c].Numerica;
                            var celda = tabla.Cell().BorderBottom(0.5f)
                                .BorderColor(Colors.Grey.Lighten2).Padding(4);
                            (alineacion ? celda.AlignRight() : celda).Text(renglon[c]);
                        }
                    }

                    if (reporte.TotalesFila is { } totales)
                    {
                        foreach (var valor in totales)
                        {
                            tabla.Cell().Padding(4).Text(valor).Bold();
                        }
                    }
                });

                pagina.Footer().AlignCenter().Text(x =>
                {
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });
            });
        });

        return documento.GeneratePdf();
    }

    private static string Sanitizar(string titulo)
    {
        var limpio = string.Concat(titulo.Split(Path.GetInvalidFileNameChars()));
        return limpio.Replace(' ', '-').Replace('—', '-').Trim('-');
    }
}
