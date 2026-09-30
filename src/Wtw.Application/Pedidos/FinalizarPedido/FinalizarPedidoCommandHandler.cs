using Wtw.Application.Pedidos.Comum;
using Wtw.Domain;
using Wtw.Domain.Pedidos;
using Wtw.Domain.Results;

namespace Wtw.Application.Pedidos.FinalizarPedido;

internal sealed class FinalizarPedidoCommandHandler(IPedidoRepository pedidos, IUnitOfWork unitOfWork)
    : AlterarStatusPedidoCommandHandler<FinalizarPedidoCommand>(pedidos, unitOfWork)
{
    protected override Result AlterarStatus(Pedido pedido) => pedido.Finalizar();
}
