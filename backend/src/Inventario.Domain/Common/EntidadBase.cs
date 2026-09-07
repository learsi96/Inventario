namespace Inventario.Domain.Common;

/// <summary>Raíz común de las entidades persistidas: identidad y marcas de tiempo.</summary>
public abstract class EntidadBase
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public DateTimeOffset CreadoEn { get; set; }

    public DateTimeOffset? ActualizadoEn { get; set; }
}
