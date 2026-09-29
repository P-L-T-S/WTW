namespace Wtw.Domain.Pedidos.Eventos;

public sealed class PedidoConfirmadoDomainEvent(Guid pedidoId, IReadOnlyCollection<ItemMovimentado> itens) : DomainEvent
{
    public Guid PedidoId { get; } = pedidoId;
    public IReadOnlyCollection<ItemMovimentado> Itens { get; } = itens;
}
