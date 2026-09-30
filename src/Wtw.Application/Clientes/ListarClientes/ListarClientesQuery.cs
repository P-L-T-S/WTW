using Wtw.Application.Abstractions.Messaging;
using Wtw.Application.Clientes.Dtos;

namespace Wtw.Application.Clientes.ListarClientes;

public sealed record ListarClientesQuery : IQuery<IReadOnlyList<ClienteDto>>;
