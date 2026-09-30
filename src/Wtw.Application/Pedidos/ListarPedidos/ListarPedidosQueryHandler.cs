using Wtw.Application.Abstractions.Messaging;
using Wtw.Application.Pedidos.Dtos;
using Wtw.Domain.Clientes;
using Wtw.Domain.Pedidos;
using Wtw.Domain.Results;

namespace Wtw.Application.Pedidos.ListarPedidos;

internal sealed class ListarPedidosQueryHandler(IPedidoRepository pedidos, IClienteRepository clientes)
    : IQueryHandler<ListarPedidosQuery, IReadOnlyList<PedidoResumoDto>>
{
    public async Task<Result<IReadOnlyList<PedidoResumoDto>>> Handle(ListarPedidosQuery query,
        CancellationToken cancellationToken)
    {
        var lista = await pedidos.ListarAsync(query.Status, cancellationToken);

        var clientesDosPedidos = await clientes.ObterPorIdsAsync(lista.Select(pedido => pedido.ClienteId), cancellationToken);
        var nomes = clientesDosPedidos.ToDictionary(cliente => cliente.Id, cliente => cliente.Nome.Valor);

        var resumos = lista
            .Select(pedido => pedido.ToResumoDto(nomes.GetValueOrDefault(pedido.ClienteId, string.Empty)))
            .ToList();

        return Result<IReadOnlyList<PedidoResumoDto>>.Ok(resumos);
    }
}
