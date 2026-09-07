using Inventario.Application.Abstractions;
using Inventario.Application.Common;
using Inventario.Domain.Catalogo;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Application.Catalogo;

public sealed class ArticulosService(
    IAppDbContext db,
    ICurrentUser currentUser,
    CategoriasService categorias)
{
    private const int TamanoMaximo = 100;

    public async Task<ResultadoPaginado<ArticuloListaDto>> ListarAsync(
        FiltroArticulos filtro, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(filtro);

        var pagina = Math.Max(1, filtro.Pagina);
        var tamano = Math.Clamp(filtro.Tamano, 1, TamanoMaximo);

        var query = db.Articulos.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filtro.Texto))
        {
            var t = filtro.Texto.Trim();
            query = query.Where(a =>
                a.Sku.Contains(t)
                || a.Nombre.Contains(t)
                || a.Marca!.Contains(t)
                || a.NumeroParteOem!.Contains(t)
                || a.CodigoBarras!.Contains(t)
                || a.CodigosAlternos.Any(c => c.Codigo.Contains(t)));
        }

        if (filtro.CategoriaId is { } categoriaId)
        {
            query = query.Where(a => a.CategoriaId == categoriaId);
        }

        if (filtro.Estado is { } estado)
        {
            query = query.Where(a => a.Estado == estado);
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(a => a.Nombre)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .Select(a => new ArticuloListaDto(
                a.Id,
                a.Sku,
                a.Nombre,
                a.Marca,
                a.Categoria.Nombre,
                a.UnidadMedida.Codigo,
                a.PrecioVenta,
                a.Estado,
                a.ImagenNombre != null))
            .ToListAsync(ct);

        return new ResultadoPaginado<ArticuloListaDto>(items, total, pagina, tamano);
    }

    public async Task<ArticuloDto> ObtenerAsync(Guid id, CancellationToken ct)
    {
        var articulo = await db.Articulos
            .AsNoTracking()
            .Include(a => a.Categoria)
            .Include(a => a.UnidadMedida)
            .Include(a => a.CodigosAlternos)
            .FirstOrDefaultAsync(a => a.Id == id, ct)
            ?? throw new NoEncontradoException("Artículo no encontrado.");

        return Proyectar(articulo);
    }

    public async Task<ArticuloDto> CrearAsync(CrearArticuloRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        Validar(request.Nombre, request.Costo, request.PrecioVenta, request.IvaPorcentaje);

        var categoriaId = await ResolverCategoriaAsync(request.CategoriaId, ct);
        await ValidarUnidadAsync(request.UnidadMedidaId, ct);

        var sku = string.IsNullOrWhiteSpace(request.Sku)
            ? await GenerarSkuAsync(ct)
            : request.Sku.Trim().ToUpperInvariant();
        await ValidarSkuUnicoAsync(sku, null, ct);
        await ValidarCodigoBarrasUnicoAsync(request.CodigoBarras, null, ct);

        var articulo = new Articulo
        {
            Sku = sku,
            CodigoBarras = Limpiar(request.CodigoBarras),
            Nombre = request.Nombre.Trim(),
            Descripcion = Limpiar(request.Descripcion),
            Marca = Limpiar(request.Marca),
            NumeroParteOem = Limpiar(request.NumeroParteOem),
            CategoriaId = categoriaId,
            UnidadMedidaId = request.UnidadMedidaId,
            Costo = request.Costo,
            PrecioVenta = request.PrecioVenta,
            IvaPorcentaje = request.IvaPorcentaje ?? 16m,
            Estado = EstadoArticulo.Activo,
        };
        AplicarCodigosAlternos(articulo, request.CodigosAlternos);

        db.Articulos.Add(articulo);
        await db.SaveChangesAsync(ct);

        return await ObtenerAsync(articulo.Id, ct);
    }

    public async Task<ArticuloDto> ActualizarAsync(
        Guid id, ActualizarArticuloRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        Validar(request.Nombre, request.Costo, request.PrecioVenta, request.IvaPorcentaje);

        var articulo = await db.Articulos
            .Include(a => a.CodigosAlternos)
            .FirstOrDefaultAsync(a => a.Id == id, ct)
            ?? throw new NoEncontradoException("Artículo no encontrado.");

        var sku = string.IsNullOrWhiteSpace(request.Sku)
            ? articulo.Sku
            : request.Sku.Trim().ToUpperInvariant();
        await ValidarSkuUnicoAsync(sku, id, ct);
        await ValidarCodigoBarrasUnicoAsync(request.CodigoBarras, id, ct);

        var categoriaId = await ResolverCategoriaAsync(request.CategoriaId, ct);
        await ValidarUnidadAsync(request.UnidadMedidaId, ct);

        articulo.Sku = sku;
        articulo.CodigoBarras = Limpiar(request.CodigoBarras);
        articulo.Nombre = request.Nombre.Trim();
        articulo.Descripcion = Limpiar(request.Descripcion);
        articulo.Marca = Limpiar(request.Marca);
        articulo.NumeroParteOem = Limpiar(request.NumeroParteOem);
        articulo.CategoriaId = categoriaId;
        articulo.UnidadMedidaId = request.UnidadMedidaId;
        articulo.Costo = request.Costo;
        articulo.PrecioVenta = request.PrecioVenta;
        articulo.IvaPorcentaje = request.IvaPorcentaje ?? 16m;

        db.CodigosAlternos.RemoveRange(articulo.CodigosAlternos);
        AplicarCodigosAlternos(articulo, request.CodigosAlternos);

        await db.SaveChangesAsync(ct);
        return await ObtenerAsync(articulo.Id, ct);
    }

    public async Task<ArticuloDto> CambiarEstadoAsync(Guid id, EstadoArticulo estado, CancellationToken ct)
    {
        var articulo = await db.Articulos.FirstOrDefaultAsync(a => a.Id == id, ct)
            ?? throw new NoEncontradoException("Artículo no encontrado.");

        articulo.Estado = estado;
        await db.SaveChangesAsync(ct);
        return await ObtenerAsync(articulo.Id, ct);
    }

    private static void Validar(string nombre, decimal costo, decimal precio, decimal? iva)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ValidacionException("El nombre es obligatorio.");
        }
        if (costo < 0 || precio < 0)
        {
            throw new ValidacionException("El costo y el precio no pueden ser negativos.");
        }
        if (iva is < 0 or > 100)
        {
            throw new ValidacionException("El IVA debe estar entre 0 y 100.");
        }
    }

    private static string? Limpiar(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

    private async Task<Guid> ResolverCategoriaAsync(Guid? categoriaId, CancellationToken ct)
    {
        if (categoriaId is null)
        {
            return await categorias.IdCategoriaOtrosAsync(ct);
        }
        if (!await db.Categorias.AnyAsync(c => c.Id == categoriaId, ct))
        {
            throw new NoEncontradoException("La categoría indicada no existe.");
        }
        return categoriaId.Value;
    }

    private async Task ValidarUnidadAsync(Guid unidadId, CancellationToken ct)
    {
        if (!await db.UnidadesMedida.AnyAsync(u => u.Id == unidadId && u.Activa, ct))
        {
            throw new NoEncontradoException("La unidad de medida indicada no existe.");
        }
    }

    private async Task<string> GenerarSkuAsync(CancellationToken ct)
    {
        var tenant = await db.Tenants.FirstAsync(t => t.Id == currentUser.TenantId, ct);
        tenant.FolioArticulos++;
        return $"ART-{tenant.FolioArticulos:D6}";
    }

    private async Task ValidarSkuUnicoAsync(string sku, Guid? idActual, CancellationToken ct)
    {
        if (await db.Articulos.AnyAsync(a => a.Sku == sku && a.Id != idActual, ct))
        {
            throw new ConflictoException($"Ya existe un artículo con el SKU '{sku}'.");
        }
    }

    private async Task ValidarCodigoBarrasUnicoAsync(string? codigo, Guid? idActual, CancellationToken ct)
    {
        var limpio = Limpiar(codigo);
        if (limpio is null)
        {
            return;
        }
        if (await db.Articulos.AnyAsync(a => a.CodigoBarras == limpio && a.Id != idActual, ct))
        {
            throw new ConflictoException($"Ya existe un artículo con el código de barras '{limpio}'.");
        }
    }

    private static void AplicarCodigosAlternos(
        Articulo articulo, IReadOnlyList<CodigoAlternoDto>? codigos)
    {
        if (codigos is null)
        {
            return;
        }
        foreach (var c in codigos.Where(c => !string.IsNullOrWhiteSpace(c.Codigo)))
        {
            articulo.CodigosAlternos.Add(new CodigoAlterno
            {
                TenantId = articulo.TenantId,
                Codigo = c.Codigo.Trim(),
                Tipo = c.Tipo,
            });
        }
    }

    private static ArticuloDto Proyectar(Articulo a) => new(
        a.Id,
        a.Sku,
        a.CodigoBarras,
        a.Nombre,
        a.Descripcion,
        a.Marca,
        a.NumeroParteOem,
        a.CategoriaId,
        a.Categoria.Nombre,
        a.UnidadMedidaId,
        a.UnidadMedida.Codigo,
        a.Costo,
        a.PrecioVenta,
        a.IvaPorcentaje,
        a.Estado,
        a.ImagenNombre != null,
        a.CodigosAlternos.Select(c => new CodigoAlternoDto(c.Codigo, c.Tipo)).ToList(),
        a.CreadoEn);
}
