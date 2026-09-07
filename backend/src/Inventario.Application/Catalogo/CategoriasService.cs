using Inventario.Application.Abstractions;
using Inventario.Application.Common;
using Inventario.Domain.Catalogo;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Application.Catalogo;

/// <summary>Casos de uso de categorías. Filtro por tenant automático vía <c>AppDbContext</c>.</summary>
public sealed class CategoriasService(IAppDbContext db)
{
    public const string NombreCategoriaSistema = "Otros";

    public async Task<IReadOnlyList<CategoriaNodoDto>> ArbolAsync(bool incluirInactivas, CancellationToken ct)
    {
        var query = db.Categorias.AsNoTracking();
        if (!incluirInactivas)
        {
            query = query.Where(c => c.Activa);
        }

        var todas = await query
            .OrderBy(c => c.Nombre)
            .Select(c => new CategoriaPlana(c.Id, c.Nombre, c.CategoriaPadreId, c.EsSistema, c.Activa))
            .ToListAsync(ct);

        var porPadre = todas.ToLookup(c => c.CategoriaPadreId);
        return Construir(null);

        IReadOnlyList<CategoriaNodoDto> Construir(Guid? padreId) =>
            porPadre[padreId]
                .Select(c => new CategoriaNodoDto(
                    c.Id, c.Nombre, c.CategoriaPadreId, c.EsSistema, c.Activa, Construir(c.Id)))
                .ToList();
    }

    public async Task<Guid> IdCategoriaOtrosAsync(CancellationToken ct)
    {
        var otros = await db.Categorias
            .FirstOrDefaultAsync(c => c.EsSistema && c.CategoriaPadreId == null, ct)
            ?? throw new NoEncontradoException(
                "El partner no tiene categoría 'Otros'. Contacta a soporte.");
        return otros.Id;
    }

    public async Task<CategoriaNodoDto> CrearAsync(CrearCategoriaRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var nombre = Normalizar(request.Nombre);
        await ValidarPadreAsync(request.CategoriaPadreId, null, ct);
        await ValidarNombreUnicoAsync(nombre, request.CategoriaPadreId, null, ct);

        var categoria = new Categoria
        {
            Nombre = nombre,
            CategoriaPadreId = request.CategoriaPadreId,
            EsSistema = false,
            Activa = true,
        };
        db.Categorias.Add(categoria);
        await db.SaveChangesAsync(ct);

        return new CategoriaNodoDto(
            categoria.Id, categoria.Nombre, categoria.CategoriaPadreId, false, true, []);
    }

    public async Task<CategoriaNodoDto> ActualizarAsync(
        Guid id, ActualizarCategoriaRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var nombre = Normalizar(request.Nombre);

        var categoria = await db.Categorias.FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new NoEncontradoException("Categoría no encontrada.");

        if (categoria.EsSistema && request.CategoriaPadreId is not null)
        {
            throw new ValidacionException("La categoría 'Otros' debe permanecer en la raíz.");
        }

        await ValidarPadreAsync(request.CategoriaPadreId, id, ct);
        await ValidarNombreUnicoAsync(nombre, request.CategoriaPadreId, id, ct);

        categoria.Nombre = nombre;
        categoria.CategoriaPadreId = request.CategoriaPadreId;
        await db.SaveChangesAsync(ct);

        return new CategoriaNodoDto(
            categoria.Id, categoria.Nombre, categoria.CategoriaPadreId, categoria.EsSistema, categoria.Activa, []);
    }

    public async Task CambiarActivacionAsync(Guid id, bool activa, CancellationToken ct)
    {
        var categoria = await db.Categorias.FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new NoEncontradoException("Categoría no encontrada.");

        if (categoria.EsSistema && !activa)
        {
            throw new ValidacionException("La categoría 'Otros' no se puede desactivar.");
        }

        categoria.Activa = activa;
        await db.SaveChangesAsync(ct);
    }

    private static string Normalizar(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ValidacionException("El nombre de la categoría es obligatorio.");
        }
        return nombre.Trim();
    }

    private async Task ValidarPadreAsync(Guid? padreId, Guid? idActual, CancellationToken ct)
    {
        if (padreId is null)
        {
            return;
        }

        if (padreId == idActual)
        {
            throw new ValidacionException("Una categoría no puede ser su propia padre.");
        }

        if (!await db.Categorias.AnyAsync(c => c.Id == padreId, ct))
        {
            throw new NoEncontradoException("La categoría padre no existe.");
        }

        // Evitar ciclos: subir por la cadena de padres desde `padreId`.
        var actual = padreId;
        var visitados = new HashSet<Guid>();
        while (actual is { } cursor && visitados.Add(cursor))
        {
            if (cursor == idActual)
            {
                throw new ValidacionException("El cambio crearía un ciclo en la jerarquía.");
            }
            actual = await db.Categorias
                .Where(c => c.Id == cursor)
                .Select(c => c.CategoriaPadreId)
                .FirstOrDefaultAsync(ct);
        }
    }

    private async Task ValidarNombreUnicoAsync(string nombre, Guid? padreId, Guid? idActual, CancellationToken ct)
    {
        // La colación por defecto de SQL Server es insensible a mayúsculas.
        var existe = await db.Categorias.AnyAsync(
            c => c.CategoriaPadreId == padreId && c.Id != idActual && c.Nombre == nombre,
            ct);
        if (existe)
        {
            throw new ConflictoException($"Ya existe una categoría '{nombre}' en ese nivel.");
        }
    }

    private sealed record CategoriaPlana(
        Guid Id, string Nombre, Guid? CategoriaPadreId, bool EsSistema, bool Activa);
}
