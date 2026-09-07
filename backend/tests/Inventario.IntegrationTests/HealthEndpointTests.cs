using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Shouldly;

namespace Inventario.IntegrationTests;

/// <summary>
/// Verifica que la API arranca de extremo a extremo y responde al health check.
/// No requiere base de datos: nada se conecta al iniciar y <c>/health</c> no
/// consulta datos. El entorno es "Testing" para que no se ejecute el seed.
/// </summary>
public class HealthEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public HealthEndpointTests(WebApplicationFactory<Program> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting(
                "ConnectionStrings:Default",
                "Server=localhost;Database=Fake;Trusted_Connection=True;TrustServerCertificate=True");
            builder.UseSetting("Jwt:ClaveFirma", "clave-de-pruebas-con-longitud-suficiente-000");
        });
    }

    [Fact]
    public async Task Health_responde_200_OK()
    {
        using var client = _factory.CreateClient();

        using var response = await client.GetAsync(new Uri("/health", UriKind.Relative));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Endpoint_protegido_sin_token_responde_401()
    {
        using var client = _factory.CreateClient();

        using var response = await client.GetAsync(new Uri("/api/sucursales", UriKind.Relative));

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
