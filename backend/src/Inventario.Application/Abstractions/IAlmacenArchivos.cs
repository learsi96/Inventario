namespace Inventario.Application.Abstractions;

/// <summary>
/// Almacenamiento de archivos binarios (imágenes de artículos). La implementación
/// local escribe en una carpeta; en la nube (Hito 9) será Azure Blob Storage.
/// </summary>
public interface IAlmacenArchivos
{
    /// <summary>Guarda el contenido y devuelve el nombre con el que quedó almacenado.</summary>
    Task<string> GuardarAsync(Stream contenido, string extension, CancellationToken ct);

    Task<ArchivoAlmacenado?> ObtenerAsync(string nombre, CancellationToken ct);

    Task EliminarAsync(string nombre, CancellationToken ct);
}

public sealed record ArchivoAlmacenado(Stream Contenido, string ContentType);
