using System.Text;
using Inventario.Api.Common;
using Inventario.Api.Endpoints;
using Inventario.Api.Seguridad;
using Inventario.Application;
using Inventario.Application.Abstractions;
using Inventario.Infrastructure;
using Inventario.Infrastructure.Identidad;
using Inventario.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();

builder.Services.Configure<SuperadminOptions>(
    builder.Configuration.GetSection(SuperadminOptions.Seccion));

var jwt = builder.Configuration.GetSection(JwtOptions.Seccion).Get<JwtOptions>()
    ?? throw new InvalidOperationException("Falta la sección de configuración 'Jwt'.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.ClaveFirma)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = "sub",
            RoleClaimType = "role",
        };
    });

builder.Services.AddAuthorizationBuilder().AddPoliticasInventario();

const string CorsClientesWeb = "clientes-web";
var origenesPermitidos = builder.Configuration
    .GetSection("Cors:OrigenesPermitidos").Get<string[]>() ?? [];
builder.Services.AddCors(options => options.AddPolicy(CorsClientesWeb, policy =>
{
    if (origenesPermitidos.Length > 0)
    {
        policy.WithOrigins(origenesPermitidos).AllowAnyHeader().AllowAnyMethod();
    }
}));

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(
        new System.Text.Json.Serialization.JsonStringEnumConverter()));

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ManejadorExcepciones>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(SwaggerConfig.Configurar);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseCors(CorsClientesWeb);
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new HealthResponse("ok", "Inventario.Api")))
    .WithName("Health")
    .WithTags("Diagnóstico")
    .AllowAnonymous();

app.MapAuthEndpoints();
app.MapUsuariosEndpoints();
app.MapSucursalesEndpoints();
app.MapPartnersEndpoints();
app.MapCatalogoEndpoints();
app.MapArticulosEndpoints();

if (app.Environment.IsDevelopment())
{
    try
    {
        await SeedData.InicializarAsync(app.Services);
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(
            ex,
            "No se pudo aplicar migraciones/seed al arrancar (¿SQL Server disponible?). "
            + "La API arranca igualmente; los endpoints con base de datos fallarán hasta resolverlo.");
    }
}

app.Run();

internal sealed record HealthResponse(string Status, string Service);

/// <summary>Punto de entrada expuesto para las pruebas de integración (WebApplicationFactory).</summary>
public partial class Program;
