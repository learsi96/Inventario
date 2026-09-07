using Inventario.Application.Abstractions;
using Inventario.Application.Common;
using Inventario.Domain.Sucursales;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Application.Sucursales;

/// <summary>
/// Casos de uso de sucursales. El filtro por tenant lo aplica <c>AppDbContext</c>
/// de forma automática; aquí solo va la lógica de negocio.
/// </summary>
public sealed class SucursalesService(IAppDbContext db)
{
    public async Task<IReadOnlyList<SucursalDto>> ListarAsync(bool incluirInactivas, CancellationToken ct)
    {
        var query = db.Sucursales.AsNoTracking();
        if (!incluirInactivas)
        {
            query = query.Where(s => s.Activa);
        }

        return await query
            .OrderBy(s => s.Nombre)
            .Select(s => Proyectar(s))
            .ToListAsync(ct);
    }

    public async Task<SucursalDto> ObtenerAsync(Guid id, CancellationToken ct)
    {
        var sucursal = await db.Sucursales.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id, ct)
            ?? throw new NoEncontradoException("Sucursal no encontrada.");

        return Proyectar(sucursal);
    }

    public async Task<SucursalDto> CrearAsync(CrearSucursalRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var (nombre, codigo, direccion) = Normalizar(request.Nombre, request.Codigo, request.Direccion);

        if (await db.Sucursales.AnyAsync(s => s.Codigo == codigo, ct))
        {
            throw new ConflictoException($"Ya existe una sucursal con el código '{codigo}'.");
        }

        var sucursal = new Sucursal
        {
            Nombre = nombre,
            Codigo = codigo,
            Direccion = direccion,
            Activa = true,
        };

        db.Sucursales.Add(sucursal);
        await db.SaveChangesAsync(ct);

        return Proyectar(sucursal);
    }

    public async Task<SucursalDto> ActualizarAsync(Guid id, ActualizarSucursalRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var (nombre, codigo, direccion) = Normalizar(request.Nombre, request.Codigo, request.Direccion);

        var sucursal = await db.Sucursales.FirstOrDefaultAsync(s => s.Id == id, ct)
            ?? throw new NoEncontradoException("Sucursal no encontrada.");

        if (await db.Sucursales.AnyAsync(s => s.Codigo == codigo && s.Id != id, ct))
        {
            throw new ConflictoException($"Ya existe una sucursal con el código '{codigo}'.");
        }

        sucursal.Nombre = nombre;
        sucursal.Codigo = codigo;
        sucursal.Direccion = direccion;
        await db.SaveChangesAsync(ct);

        return Proyectar(sucursal);
    }

    public async Task<SucursalDto> CambiarActivacionAsync(Guid id, bool activa, CancellationToken ct)
    {
        var sucursal = await db.Sucursales.FirstOrDefaultAsync(s => s.Id == id, ct)
            ?? throw new NoEncontradoException("Sucursal no encontrada.");

        sucursal.Activa = activa;
        await db.SaveChangesAsync(ct);

        return Proyectar(sucursal);
    }

    private static (string Nombre, string Codigo, string? Direccion) Normalizar(
        string nombre, string codigo, string? direccion)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ValidacionException("El nombre es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new ValidacionException("El código es obligatorio.");
        }

        return (
            nombre.Trim(),
            codigo.Trim().ToUpperInvariant(),
            string.IsNullOrWhiteSpace(direccion) ? null : direccion.Trim());
    }

    private static SucursalDto Proyectar(Sucursal s) =>
        new(s.Id, s.Nombre, s.Codigo, s.Direccion, s.Activa, s.CreadoEn);
}
