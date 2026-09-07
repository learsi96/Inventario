using Inventario.Domain.Common;
using Inventario.Domain.Sucursales;
using Inventario.Domain.Tenancy;

namespace Inventario.Domain.Identidad;

/// <summary>Usuario que pertenece a un partner.</summary>
public class Usuario : EntidadBase, ITenantEntity
{
    public Guid TenantId { get; set; }

    public required string Email { get; set; }

    public required string NombreCompleto { get; set; }

    /// <summary>Hash de la contraseña (formato de <c>PasswordHasher</c> de ASP.NET).</summary>
    public required string HashContrasena { get; set; }

    public RolUsuario Rol { get; set; } = RolUsuario.Consulta;

    public bool Activo { get; set; } = true;

    /// <summary>Sucursales a las que el usuario tiene acceso.</summary>
    public ICollection<UsuarioSucursal> Sucursales { get; } = [];
}

/// <summary>Asignación de un usuario a una sucursal (relación N:M).</summary>
public class UsuarioSucursal : ITenantEntity
{
    public Guid TenantId { get; set; }

    public Guid UsuarioId { get; set; }

    public Usuario Usuario { get; set; } = null!;

    public Guid SucursalId { get; set; }

    public Sucursal Sucursal { get; set; } = null!;
}
