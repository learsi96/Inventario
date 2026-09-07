using System.Security.Cryptography;
using System.Text;
using Inventario.Application.Abstractions;
using Inventario.Domain.Identidad;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Inventario.Infrastructure.Identidad;

/// <summary>Emite tokens JWT firmados con HMAC-SHA256.</summary>
public sealed class JwtTokenService(IOptions<JwtOptions> options) : IJwtTokenService
{
    private readonly JwtOptions _options = options.Value;

    public TokenEmitido Emitir(Usuario usuario)
    {
        ArgumentNullException.ThrowIfNull(usuario);

        return Crear(new Dictionary<string, object>
        {
            [JwtRegisteredClaimNames.Sub] = usuario.Id.ToString(),
            [JwtRegisteredClaimNames.Email] = usuario.Email,
            [JwtRegisteredClaimNames.Name] = usuario.NombreCompleto,
            ["tenant"] = usuario.TenantId.ToString(),
            ["role"] = usuario.Rol.ToString(),
        });
    }

    public TokenEmitido EmitirSuperadmin(string email)
    {
        return Crear(new Dictionary<string, object>
        {
            [JwtRegisteredClaimNames.Sub] = email,
            [JwtRegisteredClaimNames.Email] = email,
            ["role"] = "Superadmin",
            ["superadmin"] = "true",
        });
    }

    private TokenEmitido Crear(IDictionary<string, object> claims)
    {
        var expira = DateTimeOffset.UtcNow.AddMinutes(_options.MinutosVigencia);
        var clave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.ClaveFirma));

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            IssuedAt = DateTime.UtcNow,
            Expires = expira.UtcDateTime,
            Claims = claims,
            SigningCredentials = new SigningCredentials(clave, SecurityAlgorithms.HmacSha256),
        };

        var token = new JsonWebTokenHandler().CreateToken(descriptor);
        return new TokenEmitido(token, expira);
    }

    /// <summary>Genera una clave de firma aleatoria (para semilla de configuración en dev).</summary>
    public static string GenerarClaveAleatoria() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
}
