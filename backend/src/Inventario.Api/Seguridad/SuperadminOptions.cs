namespace Inventario.Api.Seguridad;

/// <summary>
/// Credenciales del superadministrador de la plataforma (sección <c>Superadmin</c>).
/// Es un acceso de configuración, sin usuario en base de datos; sirve para dar de
/// alta los primeros partners. En producción va en Key Vault.
/// </summary>
public sealed class SuperadminOptions
{
    public const string Seccion = "Superadmin";

    public string Email { get; set; } = string.Empty;

    public string Contrasena { get; set; } = string.Empty;
}
