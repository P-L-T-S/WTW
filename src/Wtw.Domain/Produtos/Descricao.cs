using Wtw.Domain.Results;

namespace Wtw.Domain.Produtos;

public readonly record struct Descricao
{
    public const int TamanhoMaximo = 500;

    private Descricao(string valor)
    {
        Valor = valor;
    }

    public string Valor { get; }

    public static Result<Descricao> Criar(string? valor)
    {
        var descricao = valor?.Trim() ?? string.Empty;

        if (descricao.Length > TamanhoMaximo)
        {
            return Result<Descricao>.Fail(ProdutoErros.DescricaoMuitoLonga);
        }

        return Result<Descricao>.Ok(new Descricao(descricao));
    }

    public override string ToString() => Valor;
}
