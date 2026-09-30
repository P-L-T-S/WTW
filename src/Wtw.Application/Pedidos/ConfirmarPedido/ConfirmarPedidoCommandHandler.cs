using Wtw.Application.Pedidos.Comum;
using Wtw.Domain;
using Wtw.Domain.Pedidos;
using Wtw.Domain.Results;

namespace Wtw.Application.Pedidos.ConfirmarPedido;

internal sealed class ConfirmarPedidoCommandHandler(IPedidoRepository pedidos, IUnitOfWork unitOfWork)
    : AlterarStatusPedidoCommandHandler<ConfirmarPedidoCommand>(pedidos, unitOfWork)
{
    protected override Result AlterarStatus(Pedido pedido) => pedido.Confirmar();
}
