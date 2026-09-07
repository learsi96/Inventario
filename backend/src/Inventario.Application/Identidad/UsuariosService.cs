using Inventario.Application.Abstractions;
using Inventario.Application.Common;
using Inventario.Domain.Identidad;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Application.Identidad;

/// <summary>Gestión de los usuarios de un partner (solo Administrador).</summary>
public sealed class UsuariosService(IAppDbContext db, ICurrentUser currentUser, IPasswordHasher hasher)
{
    private const int LongitudMinimaContrasena = 8;

    public async Task<IReadOnlyList<UsuarioDto>> ListarAsync(CancellationToken ct)
    {
        var usuarios = await db.Usuarios
            .AsNoTracking()
            .Include(u => u.Sucursales)
            .OrderBy(u => u.NombreCompleto)
            .ToListAsync(ct);

        return usuarios.Select(Proyectar).ToList();
    }

    public async Task<UsuarioDto> ObtenerAsync(Guid id, CancellationToken ct)
    {
        var usuario = await db.Usuarios
            .AsNoTracking()
            .Include(u => u.Sucursales)
            .FirstOrDefaultAsync(u => u.Id == id, ct)
            ?? throw new NoEncontradoException("Usuario no encontrado.");

        return Proyectar(usuario);
    }

    public async Task<UsuarioDto> CrearAsync(CrearUsuarioRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        var email = NormalizarEmail(request.Email);
        ValidarNombre(request.NombreCompleto);
        ValidarContrasena(request.Contrasena);

        if (await db.Usuarios.IgnoreQueryFilters().AnyAsync(u => u.Email == email, ct))
        {
            throw new ConflictoException($"Ya existe un usuario con el email '{email}'.");
        }

        var usuario = new Usuario
        {
            Email = email,
            NombreCompleto = request.NombreCompleto.Trim(),
            Rol = request.Rol,
            HashContrasena = hasher.Hash(request.Contrasena),
            Activo = true,
        };
        await AsignarSucursalesAsync(usuario, request.SucursalIds, ct);

        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync(ct);

        return await ObtenerAsync(usuario.Id, ct);
    }

    public async Task<UsuarioDto> ActualizarAsync(Guid id, ActualizarUsuarioRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidarNombre(request.NombreCompleto);

        var usuario = await db.Usuarios
            .Include(u => u.Sucursales)
            .FirstOrDefaultAsync(u => u.Id == id, ct)
            ?? throw new NoEncontradoException("Usuario no encontrado.");

        var esUsuarioActual = usuario.Id == currentUser.UsuarioId;
        if (esUsuarioActual && !request.Activo)
        {
            throw new ValidacionException("No puedes desactivar tu propia cuenta.");
        }
        if (esUsuarioActual && request.Rol != RolUsuario.Administrador)
        {
            throw new ValidacionException("No puedes quitarte a ti mismo el rol de Administrador.");
        }

        usuario.NombreCompleto = request.NombreCompleto.Trim();
        usuario.Rol = request.Rol;
        usuario.Activo = request.Activo;

        db.UsuariosSucursales.RemoveRange(usuario.Sucursales);
        usuario.Sucursales.Clear();
        await AsignarSucursalesAsync(usuario, request.SucursalIds, ct);

        await db.SaveChangesAsync(ct);
        return await ObtenerAsync(usuario.Id, ct);
    }

    public async Task CambiarActivacionAsync(Guid id, bool activo, CancellationToken ct)
    {
        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == id, ct)
            ?? throw new NoEncontradoException("Usuario no encontrado.");

        if (usuario.Id == currentUser.UsuarioId && !activo)
        {
            throw new ValidacionException("No puedes desactivar tu propia cuenta.");
        }

        usuario.Activo = activo;
        await db.SaveChangesAsync(ct);
    }

    public async Task ResetContrasenaAsync(Guid id, string contrasena, CancellationToken ct)
    {
        ValidarContrasena(contrasena);

        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == id, ct)
            ?? throw new NoEncontradoException("Usuario no encontrado.");

        usuario.HashContrasena = hasher.Hash(contrasena);
        await db.SaveChangesAsync(ct);
    }

    private async Task AsignarSucursalesAsync(
        Usuario usuario, IReadOnlyList<Guid>? sucursalIds, CancellationToken ct)
    {
        if (sucursalIds is null || sucursalIds.Count == 0)
        {
            return;
        }

        var ids = sucursalIds.Distinct().ToList();
        var validas = await db.Sucursales
            .Where(s => ids.Contains(s.Id))
            .Select(s => s.Id)
            .ToListAsync(ct);

        if (validas.Count != ids.Count)
        {
            throw new NoEncontradoException("Una o más sucursales no existen.");
        }

        foreach (var sucursalId in validas)
        {
            usuario.Sucursales.Add(new UsuarioSucursal
            {
                TenantId = usuario.TenantId,
                SucursalId = sucursalId,
            });
        }
    }

    private static string NormalizarEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@', StringComparison.Ordinal))
        {
            throw new ValidacionException("El email no es válido.");
        }
        return email.Trim().ToLowerInvariant();
    }

    private static void ValidarNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ValidacionException("El nombre completo es obligatorio.");
        }
    }

    private static void ValidarContrasena(string contrasena)
    {
        if (string.IsNullOrWhiteSpace(contrasena) || contrasena.Length < LongitudMinimaContrasena)
        {
            throw new ValidacionException(
                $"La contraseña debe tener al menos {LongitudMinimaContrasena} caracteres.");
        }
    }

    private UsuarioDto Proyectar(Usuario u) => new(
        u.Id,
        u.Email,
        u.NombreCompleto,
        u.Rol,
        u.Activo,
        u.Sucursales.Select(s => s.SucursalId).ToList(),
        u.Id == currentUser.UsuarioId);
}
