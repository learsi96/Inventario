using Inventario.Application.Abstractions;
using Microsoft.AspNetCore.Identity;

namespace Inventario.Infrastructure.Identidad;

/// <summary>
/// Adaptador sobre <see cref="PasswordHasher{TUser}"/> de ASP.NET (PBKDF2 con sal
/// e iteraciones). Se usa solo la primitiva de hash, no el resto de Identity.
/// </summary>
public sealed class PasswordHasherAdapter : IPasswordHasher
{
    private static readonly PasswordHasher<object> Hasher = new();
    private static readonly object Contexto = new();

    public string Hash(string contrasena) => Hasher.HashPassword(Contexto, contrasena);

    public bool Verificar(string hash, string contrasena) =>
        Hasher.VerifyHashedPassword(Contexto, hash, contrasena)
            is PasswordVerificationResult.Success
            or PasswordVerificationResult.SuccessRehashNeeded;
}
