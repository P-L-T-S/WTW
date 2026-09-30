using Wtw.Application.Pedidos.Comum;
using Wtw.Domain;
using Wtw.Domain.Pedidos;
using Wtw.Domain.Results;

namespace Wtw.Application.Pedidos.CancelarPedido;

internal sealed class CancelarPedidoCommandHandler(IPedidoRepository pedidos, IUnitOfWork unitOfWork)
    : AlterarStatusPedidoCommandHandler<CancelarPedidoCommand>(pedidos, unitOfWork)
{
    protected override Result AlterarStatus(Pedido pedido) => pedido.Cancelar();
}
