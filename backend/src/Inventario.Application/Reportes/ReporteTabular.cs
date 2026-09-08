namespace Inventario.Application.Reportes;

public sealed record ColumnaReporte(string Titulo, bool Numerica = false);

/// <summary>Representación neutral de un reporte tabular; los generadores la vuelcan a Excel o PDF.</summary>
public sealed record ReporteTabular(
    string Titulo,
    string? Subtitulo,
    IReadOnlyList<ColumnaReporte> Columnas,
    IReadOnlyList<IReadOnlyList<string>> Filas,
    IReadOnlyList<string>? TotalesFila = null);

public enum FormatoReporte
{
    Xlsx,
    Pdf,
}

public sealed record ArchivoReporte(byte[] Contenido, string ContentType, string NombreArchivo);
