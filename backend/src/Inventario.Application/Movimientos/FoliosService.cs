using Inventario.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Application.Movimientos;

/// <summary>Genera folios consecutivos por partner para artículos y movimientos.</summary>
public sealed class FoliosService(IAppDbContext db, ICurrentUser currentUser)
{
    public async Task<int> SiguienteMovimientoAsync(CancellationToken ct)
    {
        var tenant = await db.Tenants.FirstAsync(t => t.Id == currentUser.TenantId, ct);
        return ++tenant.FolioMovimientos;
    }
}
