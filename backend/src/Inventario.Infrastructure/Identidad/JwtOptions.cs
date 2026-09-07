using System.ComponentModel.DataAnnotations;

namespace Inventario.Infrastructure.Identidad;

/// <summary>Configuración de emisión y validación de tokens JWT (sección <c>Jwt</c>).</summary>
public sealed class JwtOptions
{
    public const string Seccion = "Jwt";

    [Required]
    public string Issuer { get; set; } = "inventario";

    [Required]
    public string Audience { get; set; } = "inventario";

    /// <summary>Clave simétrica de firma (HMAC-SHA256). Mínimo 32 caracteres.</summary>
    [Required]
    [MinLength(32)]
    public string ClaveFirma { get; set; } = string.Empty;

    [Range(5, 1440)]
    public int MinutosVigencia { get; set; } = 60;
}
