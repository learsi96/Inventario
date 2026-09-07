using Inventario.Domain.Common;

namespace Inventario.Domain.Partners;

/// <summary>
/// Partner: empresa cliente que usa el sistema de forma aislada. No implementa
/// <c>ITenantEntity</c> porque es la raíz de la jerarquía multi-tenant.
/// </summary>
public class Tenant : EntidadBase
{
    public required string Nombre { get; set; }

    /// <summary>Clave corta única del partner (p. ej. "REFACC-NORTE").</summary>
    public required string Codigo { get; set; }

    public string Moneda { get; set; } = "MXN";

    public decimal IvaPorcentaje { get; set; } = 16m;

    public string ZonaHoraria { get; set; } = "America/Mexico_City";

    public bool Activo { get; set; } = true;

    /// <summary>Folio del que se deriva el siguiente SKU autogenerado (ART-000001…).</summary>
    public int FolioArticulos { get; set; }

    /// <summary>Folio consecutivo de movimientos de inventario.</summary>
    public int FolioMovimientos { get; set; }
}
