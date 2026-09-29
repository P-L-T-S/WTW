using Wtw.Domain.Results;

namespace Wtw.Domain.Comum;

public static class ComumErros
{
    public static Erro NomeInvalido => new("Nome.Criar",
        $"O nome é obrigatório e deve ter entre {Nome.TamanhoMinimo} e {Nome.TamanhoMaximo} caracteres.")
    {
        Tipo = TipoErro.Validacao
    };
}
