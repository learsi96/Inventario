namespace Inventario.Application.Abstractions;

/// <summary>Hash y verificación de contraseñas.</summary>
public interface IPasswordHasher
{
    string Hash(string contrasena);

    bool Verificar(string hash, string contrasena);
}
