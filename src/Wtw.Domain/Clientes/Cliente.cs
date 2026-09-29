using Wtw.Domain.Comum;

namespace Wtw.Domain.Clientes;

public sealed class Cliente : AggregateRoot
{
    // Construtor exigido pelo EF Core.
    private Cliente()
    {
    }

    private Cliente(Nome nome, Email email)
    {
        Nome = nome;
        Email = email;
    }

    public Nome Nome { get; private set; }
    public Email Email { get; private set; }

    public static Cliente Criar(Nome nome, Email email) => new(nome, email);
}
