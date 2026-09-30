using Wtw.Application.Abstractions.Messaging;

namespace Wtw.Application.Pedidos.CriarPedido;

public sealed record CriarPedidoCommand(Guid ClienteId, IReadOnlyList<ItemPedidoCommand> Itens) : ICommand<Guid>;

public sealed record ItemPedidoCommand(Guid ProdutoId, int Quantidade);
