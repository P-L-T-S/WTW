using Wtw.Domain.Results;

namespace Wtw.Domain.Clientes;

public static class ClienteErros
{
    public static Erro EmailInvalido => new("Email.Criar",
        $"Informe um e-mail válido com no máximo {Email.TamanhoMaximo} caracteres.")
    {
        Tipo = TipoErro.Validacao
    };
}
