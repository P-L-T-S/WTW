using Wtw.Domain.Results;

namespace Wtw.Application.Comum;

public sealed class ErroValidacao : Erro
{
    public ErroValidacao(IReadOnlyDictionary<string, string[]> campos)
        : base("Validacao", "Um ou mais campos são inválidos.")
    {
        Campos = campos;
        Tipo = TipoErro.Validacao;
    }

    public IReadOnlyDictionary<string, string[]> Campos { get; }
}
