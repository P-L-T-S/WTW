using Wtw.Application.Pedidos.Comum;

namespace Wtw.Application.Pedidos.ConfirmarPedido;

public sealed record ConfirmarPedidoCommand(Guid PedidoId) : IAlterarStatusPedidoCommand;
