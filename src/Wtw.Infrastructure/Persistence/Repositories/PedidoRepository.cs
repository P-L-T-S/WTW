using Microsoft.EntityFrameworkCore;
using Wtw.Domain.Pedidos;

namespace Wtw.Infrastructure.Persistence.Repositories;

internal sealed class PedidoRepository(WtwDbContext context) : IPedidoRepository
{
    public Task<Pedido?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Pedidos
            .Include(pedido => pedido.Itens)
            .FirstOrDefaultAsync(pedido => pedido.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Pedido>> ListarAsync(StatusPedido? status = null,
        CancellationToken cancellationToken = default)
    {
        var consulta = context.Pedidos
            .AsNoTracking()
            .Include(pedido => pedido.Itens)
            .AsQueryable();

        if (status is not null)
        {
            consulta = consulta.Where(pedido => pedido.Status == status);
        }

        return await consulta
            .OrderByDescending(pedido => pedido.CriadoEmUtc)
            .ToListAsync(cancellationToken);
    }

    public void Adicionar(Pedido pedido) => context.Pedidos.Add(pedido);
}
