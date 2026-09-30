using Wtw.Application.Abstractions.Messaging;
using Wtw.Application.Comum;
using Wtw.Application.Pedidos.Dtos;
using Wtw.Domain.Clientes;
using Wtw.Domain.Pedidos;
using Wtw.Domain.Results;

namespace Wtw.Application.Pedidos.ObterPedido;

internal sealed class ObterPedidoQueryHandler(IPedidoRepository pedidos, IClienteRepository clientes)
    : IQueryHandler<ObterPedidoQuery, PedidoDto>
{
    public async Task<Result<PedidoDto>> Handle(ObterPedidoQuery query, CancellationToken cancellationToken)
    {
        var pedido = await pedidos.ObterPorIdAsync(query.Id, cancellationToken);

        if (pedido is null)
        {
            return Result<PedidoDto>.Fail(ApplicationErros.NaoEncontrado(nameof(Pedido), query.Id));
        }

        var cliente = await clientes.ObterPorIdAsync(pedido.ClienteId, cancellationToken);

        return Result<PedidoDto>.Ok(pedido.ToDto(cliente?.Nome.Valor ?? string.Empty));
    }
}
