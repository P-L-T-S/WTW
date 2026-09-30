using Wtw.Application.Pedidos.Comum;

namespace Wtw.Application.Pedidos.CancelarPedido;

public sealed record CancelarPedidoCommand(Guid PedidoId) : IAlterarStatusPedidoCommand;
