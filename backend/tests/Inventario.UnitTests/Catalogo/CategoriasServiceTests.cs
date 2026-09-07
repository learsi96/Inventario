using Inventario.Application.Abstractions;
using Inventario.Application.Catalogo;
using Inventario.Application.Common;
using Inventario.Domain.Catalogo;
using Inventario.Domain.Identidad;
using Inventario.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Inventario.UnitTests.Catalogo;

public sealed class CategoriasServiceTests : IDisposable
{
    private readonly SqliteConnection _conexion;
    private readonly DbContextOptions<AppDbContext> _opciones;
    private readonly Guid _tenant = Guid.NewGuid();

    public CategoriasServiceTests()
    {
        _conexion = new SqliteConnection("DataSource=:memory:");
        _conexion.Open();
        _opciones = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_conexion).Options;

        using var ctx = Crear();
        ctx.Database.EnsureCreated();
        ctx.Categorias.Add(new Categoria
        {
            TenantId = _tenant,
            Nombre = "Otros",
            EsSistema = true,
            Activa = true,
        });
        ctx.SaveChanges();
    }

    [Fact]
    public async Task Crea_una_subcategoria_y_el_arbol_la_anida()
    {
        var servicio = new CategoriasService(Crear());

        var frenos = await servicio.CrearAsync(new CrearCategoriaRequest("Frenos", null), default);
        await servicio.CrearAsync(new CrearCategoriaRequest("Balatas", frenos.Id), default);

        var arbol = await servicio.ArbolAsync(false, default);
        var nodoFrenos = arbol.Single(c => c.Nombre == "Frenos");
        nodoFrenos.Subcategorias.ShouldHaveSingleItem().Nombre.ShouldBe("Balatas");
    }

    [Fact]
    public async Task No_permite_desactivar_la_categoria_de_sistema()
    {
        var db = Crear();
        var servicio = new CategoriasService(db);
        var otros = await db.Categorias.SingleAsync(c => c.EsSistema);

        await Should.ThrowAsync<ValidacionException>(
            () => servicio.CambiarActivacionAsync(otros.Id, false, default));
    }

    [Fact]
    public async Task No_permite_crear_un_ciclo_en_la_jerarquia()
    {
        var servicio = new CategoriasService(Crear());
        var a = await servicio.CrearAsync(new CrearCategoriaRequest("A", null), default);
        var b = await servicio.CrearAsync(new CrearCategoriaRequest("B", a.Id), default);

        await Should.ThrowAsync<ValidacionException>(
            () => servicio.ActualizarAsync(a.Id, new ActualizarCategoriaRequest("A", b.Id), default));
    }

    [Fact]
    public async Task No_permite_dos_categorias_con_el_mismo_nombre_en_el_mismo_nivel()
    {
        var servicio = new CategoriasService(Crear());
        await servicio.CrearAsync(new CrearCategoriaRequest("Frenos", null), default);

        await Should.ThrowAsync<ConflictoException>(
            () => servicio.CrearAsync(new CrearCategoriaRequest("Frenos", null), default));
    }

    private AppDbContext Crear() => new(_opciones, new CurrentUserFake(_tenant));

    public void Dispose() => _conexion.Dispose();

    private sealed class CurrentUserFake(Guid tenant) : ICurrentUser
    {
        public Guid? UsuarioId => Guid.NewGuid();

        public Guid? TenantId => tenant;

        public RolUsuario? Rol => RolUsuario.Administrador;

        public bool EsSuperadmin => false;

        public bool EstaAutenticado => true;
    }
}
