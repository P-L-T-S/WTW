using Wtw.Domain.Clientes;

namespace Wtw.Application.Clientes.Dtos;

public sealed record ClienteDto(Guid Id, string Nome, string Email);

internal static class ClienteMapeamento
{
    public static ClienteDto ToDto(this Cliente cliente) => new(cliente.Id, cliente.Nome.Valor, cliente.Email.Valor);
}
