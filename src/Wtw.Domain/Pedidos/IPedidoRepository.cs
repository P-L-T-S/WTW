namespace Wtw.Domain.Pedidos;

public interface IPedidoRepository
{
    Task<Pedido?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Pedido>> ListarAsync(StatusPedido? status = null, CancellationToken cancellationToken = default);
    void Adicionar(Pedido pedido);
}
