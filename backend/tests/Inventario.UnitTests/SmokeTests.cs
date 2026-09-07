using Inventario.Domain.Tenancy;
using Shouldly;

namespace Inventario.UnitTests;

/// <summary>Pruebas mínimas que verifican que el proyecto de pruebas unitarias compila y ejecuta.</summary>
public class SmokeTests
{
    [Fact]
    public void El_contrato_de_multi_tenant_expone_TenantId()
    {
        typeof(ITenantEntity)
            .GetProperty(nameof(ITenantEntity.TenantId))
            .ShouldNotBeNull();
    }
}
