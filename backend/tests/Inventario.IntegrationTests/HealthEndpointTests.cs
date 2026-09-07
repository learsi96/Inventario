using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Shouldly;

namespace Inventario.IntegrationTests;

/// <summary>
/// Verifica que la API arranca de extremo a extremo y responde al health check.
/// No requiere base de datos: <c>AddInfrastructure</c> solo registra el
/// <c>DbContext</c>, no abre conexión, y <c>/health</c> no consulta datos.
/// </summary>
public class HealthEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public HealthEndpointTests(WebApplicationFactory<Program> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        _factory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting(
                "ConnectionStrings:Default",
                "Server=localhost;Database=Fake;Trusted_Connection=True;TrustServerCertificate=True"));
    }

    [Fact]
    public async Task Health_responde_200_OK()
    {
        using var client = _factory.CreateClient();

        using var response = await client.GetAsync(new Uri("/health", UriKind.Relative));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
