using Wtw.Application.Abstractions.Messaging;
using Wtw.Application.Clientes.Dtos;
using Wtw.Domain.Clientes;
using Wtw.Domain.Results;

namespace Wtw.Application.Clientes.ListarClientes;

internal sealed class ListarClientesQueryHandler(IClienteRepository clientes)
    : IQueryHandler<ListarClientesQuery, IReadOnlyList<ClienteDto>>
{
    public async Task<Result<IReadOnlyList<ClienteDto>>> Handle(ListarClientesQuery query,
        CancellationToken cancellationToken)
    {
        var lista = await clientes.ListarAsync(cancellationToken);

        return Result<IReadOnlyList<ClienteDto>>.Ok(lista.Select(cliente => cliente.ToDto()).ToList());
    }
}
