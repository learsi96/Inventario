using Inventario.Application.Abstractions;
using Inventario.Application.Common;
using Inventario.Domain.Existencias;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Application.Existencias;

public sealed class UbicacionesService(IAppDbContext db)
{
    public async Task<IReadOnlyList<UbicacionDto>> ListarAsync(
        Guid? sucursalId, bool incluirInactivas, CancellationToken ct)
    {
        var query = db.Ubicaciones.AsNoTracking();
        if (sucursalId is { } id)
        {
            query = query.Where(u => u.SucursalId == id);
        }
        if (!incluirInactivas)
        {
            query = query.Where(u => u.Activa);
        }

        return await query
            .OrderBy(u => u.Codigo)
            .Select(u => new UbicacionDto(u.Id, u.SucursalId, u.Codigo, u.Descripcion, u.Activa))
            .ToListAsync(ct);
    }

    public async Task<UbicacionDto> CrearAsync(CrearUbicacionRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var codigo = Normalizar(request.Codigo);

        if (!await db.Sucursales.AnyAsync(s => s.Id == request.SucursalId, ct))
        {
            throw new NoEncontradoException("La sucursal indicada no existe.");
        }
        await ValidarCodigoUnicoAsync(request.SucursalId, codigo, null, ct);

        var ubicacion = new Ubicacion
        {
            SucursalId = request.SucursalId,
            Codigo = codigo,
            Descripcion = Limpiar(request.Descripcion),
            Activa = true,
        };
        db.Ubicaciones.Add(ubicacion);
        await db.SaveChangesAsync(ct);

        return new UbicacionDto(
            ubicacion.Id, ubicacion.SucursalId, ubicacion.Codigo, ubicacion.Descripcion, ubicacion.Activa);
    }

    public async Task<UbicacionDto> ActualizarAsync(
        Guid id, ActualizarUbicacionRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var codigo = Normalizar(request.Codigo);

        var ubicacion = await db.Ubicaciones.FirstOrDefaultAsync(u => u.Id == id, ct)
            ?? throw new NoEncontradoException("Ubicación no encontrada.");

        await ValidarCodigoUnicoAsync(ubicacion.SucursalId, codigo, id, ct);

        ubicacion.Codigo = codigo;
        ubicacion.Descripcion = Limpiar(request.Descripcion);
        ubicacion.Activa = request.Activa;
        await db.SaveChangesAsync(ct);

        return new UbicacionDto(
            ubicacion.Id, ubicacion.SucursalId, ubicacion.Codigo, ubicacion.Descripcion, ubicacion.Activa);
    }

    private async Task ValidarCodigoUnicoAsync(
        Guid sucursalId, string codigo, Guid? idActual, CancellationToken ct)
    {
        if (await db.Ubicaciones.AnyAsync(
            u => u.SucursalId == sucursalId && u.Codigo == codigo && u.Id != idActual, ct))
        {
            throw new ConflictoException($"Ya existe la ubicación '{codigo}' en esa sucursal.");
        }
    }

    private static string Normalizar(string codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new ValidacionException("El código de la ubicación es obligatorio.");
        }
        return codigo.Trim().ToUpperInvariant();
    }

    private static string? Limpiar(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
