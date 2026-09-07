using Inventario.Application.Abstractions;
using Inventario.Application.Common;
using Inventario.Application.Identidad;
using Inventario.Domain.Identidad;
using Inventario.Domain.Sucursales;
using Inventario.Infrastructure.Identidad;
using Inventario.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Inventario.UnitTests.Identidad;

public sealed class UsuariosServiceTests : IDisposable
{
    private readonly SqliteConnection _conexion;
    private readonly DbContextOptions<AppDbContext> _opciones;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _adminId = Guid.NewGuid();
    private readonly Guid _sucursalId = Guid.NewGuid();
    private readonly PasswordHasherAdapter _hasher = new();

    public UsuariosServiceTests()
    {
        _conexion = new SqliteConnection("DataSource=:memory:");
        _conexion.Open();
        _opciones = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_conexion).Options;

        using var ctx = Crear(_adminId);
        ctx.Database.EnsureCreated();
        ctx.Sucursales.Add(new Sucursal
        {
            Id = _sucursalId,
            TenantId = _tenant,
            Nombre = "Matriz",
            Codigo = "MATRIZ",
        });
        ctx.Usuarios.Add(new Usuario
        {
            Id = _adminId,
            TenantId = _tenant,
            Email = "admin@demo.com",
            NombreCompleto = "Admin",
            HashContrasena = _hasher.Hash("Demo1234!"),
            Rol = RolUsuario.Administrador,
            Activo = true,
        });
        ctx.SaveChanges();
    }

    [Fact]
    public async Task Crea_un_usuario_con_sucursales_asignadas()
    {
        var dto = await Servicio(_adminId).CrearAsync(
            new CrearUsuarioRequest(
                "vendedor@demo.com", "Vendedor", RolUsuario.Vendedor, "Clave1234", [_sucursalId]),
            default);

        dto.Email.ShouldBe("vendedor@demo.com");
        dto.SucursalIds.ShouldHaveSingleItem().ShouldBe(_sucursalId);
    }

    [Fact]
    public async Task No_permite_email_duplicado()
    {
        await Should.ThrowAsync<ConflictoException>(() => Servicio(_adminId).CrearAsync(
            new CrearUsuarioRequest(
                "admin@demo.com", "Otro", RolUsuario.Consulta, "Clave1234", null),
            default));
    }

    [Fact]
    public async Task Rechaza_contrasenas_cortas()
    {
        await Should.ThrowAsync<ValidacionException>(() => Servicio(_adminId).CrearAsync(
            new CrearUsuarioRequest("x@demo.com", "X", RolUsuario.Consulta, "corta", null),
            default));
    }

    [Fact]
    public async Task No_permite_que_el_admin_se_desactive_a_si_mismo()
    {
        await Should.ThrowAsync<ValidacionException>(
            () => Servicio(_adminId).CambiarActivacionAsync(_adminId, false, default));
    }

    [Fact]
    public async Task No_permite_que_el_admin_se_quite_su_propio_rol()
    {
        await Should.ThrowAsync<ValidacionException>(() => Servicio(_adminId).ActualizarAsync(
            _adminId,
            new ActualizarUsuarioRequest("Admin", RolUsuario.Consulta, true, null),
            default));
    }

    private UsuariosService Servicio(Guid usuarioActual) =>
        new(Crear(usuarioActual), new CurrentUserFake(_tenant, usuarioActual), _hasher);

    private AppDbContext Crear(Guid usuarioActual) =>
        new(_opciones, new CurrentUserFake(_tenant, usuarioActual));

    public void Dispose() => _conexion.Dispose();

    private sealed class CurrentUserFake(Guid tenant, Guid usuario) : ICurrentUser
    {
        public Guid? UsuarioId => usuario;

        public Guid? TenantId => tenant;

        public RolUsuario? Rol => RolUsuario.Administrador;

        public bool EsSuperadmin => false;

        public bool EstaAutenticado => true;
    }
}
