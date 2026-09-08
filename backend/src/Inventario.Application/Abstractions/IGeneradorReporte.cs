using Inventario.Application.Reportes;

namespace Inventario.Application.Abstractions;

/// <summary>Convierte un <see cref="ReporteTabular"/> en un archivo (Excel o PDF).</summary>
public interface IGeneradorReporte
{
    ArchivoReporte Generar(ReporteTabular reporte, FormatoReporte formato);
}
