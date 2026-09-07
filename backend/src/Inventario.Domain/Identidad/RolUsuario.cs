namespace Inventario.Domain.Identidad;

/// <summary>Roles del sistema dentro de un partner. Conjunto fijo en el MVP1.</summary>
public enum RolUsuario
{
    /// <summary>Acceso total dentro del partner: configuración, usuarios, todas las sucursales.</summary>
    Administrador = 1,

    /// <summary>Movimientos, transferencias y conteos en sus sucursales asignadas.</summary>
    EncargadoAlmacen = 2,

    /// <summary>Salida de inventario y consulta en su sucursal.</summary>
    Vendedor = 3,

    /// <summary>Solo lectura: existencias, fichas y reportes.</summary>
    Consulta = 4,
}
