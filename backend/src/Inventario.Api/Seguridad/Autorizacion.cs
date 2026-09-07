using Inventario.Domain.Identidad;
using Microsoft.AspNetCore.Authorization;

namespace Inventario.Api.Seguridad;

/// <summary>Nombres de políticas y roles usados en los endpoints.</summary>
public static class Politicas
{
    public const string Superadmin = "Superadmin";

    /// <summary>Administrador del partner.</summary>
    public const string AdminPartner = "AdminPartner";

    /// <summary>Administrador o Encargado de almacén: opera inventario (movimientos, conteos).</summary>
    public const string OperadorAlmacen = "OperadorAlmacen";

    /// <summary>Cualquier usuario autenticado de un partner (excluye superadmin sin tenant).</summary>
    public const string UsuarioPartner = "UsuarioPartner";
}

public static class AutorizacionSetup
{
    public static AuthorizationBuilder AddPoliticasInventario(this AuthorizationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder
            .AddPolicy(Politicas.Superadmin, p => p.RequireClaim("superadmin", "true"))
            .AddPolicy(Politicas.AdminPartner, p => p.RequireClaim("role", nameof(RolUsuario.Administrador)))
            .AddPolicy(Politicas.OperadorAlmacen, p => p.RequireClaim(
                "role", nameof(RolUsuario.Administrador), nameof(RolUsuario.EncargadoAlmacen)))
            .AddPolicy(Politicas.UsuarioPartner, p => p.RequireClaim("tenant"));
    }
}
