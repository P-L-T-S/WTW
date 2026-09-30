using Wtw.Application.Pedidos.Comum;

namespace Wtw.Application.Pedidos.FinalizarPedido;

public sealed record FinalizarPedidoCommand(Guid PedidoId) : IAlterarStatusPedidoCommand;
