using Wtw.Domain.Results;

namespace Wtw.Domain.Produtos;

public readonly record struct Preco
{
    private Preco(decimal valor)
    {
        Valor = valor;
    }

    public decimal Valor { get; }

    public static Result<Preco> Criar(decimal valor)
    {
        if (valor <= 0)
        {
            return Result<Preco>.Fail(ProdutoErros.PrecoInvalido);
        }

        return Result<Preco>.Ok(new Preco(decimal.Round(valor, 2, MidpointRounding.AwayFromZero)));
    }

    public override string ToString() => Valor.ToString("F2");
}
