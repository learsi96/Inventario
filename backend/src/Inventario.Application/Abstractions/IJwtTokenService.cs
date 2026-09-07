using Inventario.Domain.Identidad;

namespace Inventario.Application.Abstractions;

/// <summary>Emisión de tokens JWT de acceso.</summary>
public interface IJwtTokenService
{
    TokenEmitido Emitir(Usuario usuario);

    TokenEmitido EmitirSuperadmin(string email);
}

/// <summary>Token generado junto con su fecha de expiración (UTC).</summary>
public sealed record TokenEmitido(string AccessToken, DateTimeOffset ExpiraEn);
