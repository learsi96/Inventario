using Inventario.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace Inventario.Infrastructure.Archivos;

public sealed class ArchivosOptions
{
    public const string Seccion = "Archivos";

    /// <summary>Carpeta donde se guardan los archivos en desarrollo.</summary>
    public string RutaBase { get; set; } = "archivos-dev";
}

/// <summary>Almacén de archivos en el sistema de ficheros local (desarrollo).</summary>
public sealed class AlmacenArchivosLocal : IAlmacenArchivos
{
    private readonly string _rutaBase;

    public AlmacenArchivosLocal(IOptions<ArchivosOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _rutaBase = Path.GetFullPath(options.Value.RutaBase);
        Directory.CreateDirectory(_rutaBase);
    }

    public async Task<string> GuardarAsync(Stream contenido, string extension, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(contenido);
        var nombre = $"{Guid.NewGuid():N}{extension}";
        var ruta = Path.Combine(_rutaBase, nombre);

        await using var destino = File.Create(ruta);
        await contenido.CopyToAsync(destino, ct);

        return nombre;
    }

    public Task<ArchivoAlmacenado?> ObtenerAsync(string nombre, CancellationToken ct)
    {
        var ruta = RutaSegura(nombre);
        if (ruta is null || !File.Exists(ruta))
        {
            return Task.FromResult<ArchivoAlmacenado?>(null);
        }

        Stream contenido = File.OpenRead(ruta);
        return Task.FromResult<ArchivoAlmacenado?>(
            new ArchivoAlmacenado(contenido, TipoContenido(ruta)));
    }

    public Task EliminarAsync(string nombre, CancellationToken ct)
    {
        var ruta = RutaSegura(nombre);
        if (ruta is not null && File.Exists(ruta))
        {
            File.Delete(ruta);
        }
        return Task.CompletedTask;
    }

    private string? RutaSegura(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre) || nombre.Contains("..", StringComparison.Ordinal))
        {
            return null;
        }
        var ruta = Path.GetFullPath(Path.Combine(_rutaBase, nombre));
        return ruta.StartsWith(_rutaBase, StringComparison.Ordinal) ? ruta : null;
    }

    private static string TipoContenido(string ruta) => Path.GetExtension(ruta).ToLowerInvariant() switch
    {
        ".png" => "image/png",
        ".webp" => "image/webp",
        ".gif" => "image/gif",
        _ => "image/jpeg",
    };
}
