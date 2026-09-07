using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Inventario.Api.Common;

/// <summary>Configuración de Swagger/OpenAPI, con el esquema Bearer para probar endpoints protegidos.</summary>
public static class SwaggerConfig
{
    public static void Configurar(SwaggerGenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Inventario API",
            Version = "v1",
            Description = "Sistema de inventario multi-partner.",
        });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Pega el token devuelto por /api/auth/login.",
        });

        options.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer")] = [],
        });
    }
}
