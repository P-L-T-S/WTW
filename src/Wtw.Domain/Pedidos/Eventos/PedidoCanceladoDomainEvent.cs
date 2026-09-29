namespace Wtw.Domain.Pedidos.Eventos;

/// <summary>
/// Disparado apenas quando um pedido já confirmado é cancelado, indicando que o estoque deve ser restaurado.
/// </summary>
public sealed class PedidoCanceladoDomainEvent(Guid pedidoId, IReadOnlyCollection<ItemMovimentado> itens) : DomainEvent
{
    public Guid PedidoId { get; } = pedidoId;
    public IReadOnlyCollection<ItemMovimentado> Itens { get; } = itens;
}
