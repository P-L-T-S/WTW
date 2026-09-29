using System.Net.Mail;
using Wtw.Domain.Results;

namespace Wtw.Domain.Clientes;

public readonly record struct Email
{
    public const int TamanhoMaximo = 200;

    private Email(string valor)
    {
        Valor = valor;
    }

    public string Valor { get; }

    public static Result<Email> Criar(string? valor)
    {
        var email = valor?.Trim().ToLowerInvariant();

        if (string.IsNullOrEmpty(email) || email.Length > TamanhoMaximo || !EhValido(email))
        {
            return Result<Email>.Fail(ClienteErros.EmailInvalido);
        }

        return Result<Email>.Ok(new Email(email));
    }

    private static bool EhValido(string email) =>
        MailAddress.TryCreate(email, out var endereco) && endereco.Address == email;

    public override string ToString() => Valor;
}
