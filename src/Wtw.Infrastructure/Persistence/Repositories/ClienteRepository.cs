using Microsoft.EntityFrameworkCore;
using Wtw.Domain.Clientes;

namespace Wtw.Infrastructure.Persistence.Repositories;

internal sealed class ClienteRepository(WtwDbContext context) : IClienteRepository
{
    public Task<bool> ExisteAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Clientes.AnyAsync(cliente => cliente.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Cliente>> ObterPorIdsAsync(IEnumerable<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        var idsDistintos = ids.Distinct().ToList();

        return await context.Clientes
            .AsNoTracking()
            .Where(cliente => idsDistintos.Contains(cliente.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Cliente>> ListarAsync(CancellationToken cancellationToken = default) =>
        await context.Clientes
            .AsNoTracking()
            .OrderBy(cliente => cliente.Nome)
            .ToListAsync(cancellationToken);
}
