using Inventario.Application.Abstractions;
using Inventario.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Application.Identidad;

public sealed class AutenticacionService(
    IAppDbContext db,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwt,
    ICurrentUser currentUser)
{
    public async Task<TokenResponse> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Contrasena))
        {
            throw new ValidacionException("Email y contraseña son obligatorios.");
        }

        var email = request.Email.Trim().ToLowerInvariant();

        // IgnoreQueryFilters: el login ocurre sin tenant en el contexto todavía.
        var usuario = await db.Usuarios
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == email, ct);

        if (usuario is null || !passwordHasher.Verificar(usuario.HashContrasena, request.Contrasena))
        {
            throw new AutenticacionException("Credenciales inválidas.");
        }

        if (!usuario.Activo)
        {
            throw new AutenticacionException("El usuario está inactivo.");
        }

        var token = jwt.Emitir(usuario);
        return new TokenResponse(token.AccessToken, token.ExpiraEn);
    }

    public async Task<UsuarioActualDto> ObtenerActualAsync(CancellationToken ct)
    {
        var id = currentUser.UsuarioId
            ?? throw new AutenticacionException("Petición no autenticada.");

        var usuario = await db.Usuarios
            .Include(u => u.Sucursales)
            .FirstOrDefaultAsync(u => u.Id == id, ct)
            ?? throw new NoEncontradoException("El usuario ya no existe.");

        return new UsuarioActualDto(
            usuario.Id,
            usuario.Email,
            usuario.NombreCompleto,
            usuario.Rol,
            usuario.TenantId,
            usuario.Sucursales.Select(s => s.SucursalId).ToList());
    }
}
