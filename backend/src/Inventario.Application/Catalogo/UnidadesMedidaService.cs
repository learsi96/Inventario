using Inventario.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Application.Catalogo;

/// <summary>Consulta del catálogo global de unidades de medida.</summary>
public sealed class UnidadesMedidaService(IAppDbContext db)
{
    public async Task<IReadOnlyList<UnidadMedidaDto>> ListarAsync(CancellationToken ct) =>
        await db.UnidadesMedida
            .AsNoTracking()
            .Where(u => u.Activa)
            .OrderBy(u => u.Codigo)
            .Select(u => new UnidadMedidaDto(u.Id, u.Codigo, u.Nombre, u.Activa))
            .ToListAsync(ct);
}
