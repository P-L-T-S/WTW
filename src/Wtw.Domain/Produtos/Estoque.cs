using Wtw.Domain.Results;

namespace Wtw.Domain.Produtos;

public readonly record struct Estoque
{
    private Estoque(int valor)
    {
        Valor = valor;
    }

    public int Valor { get; }

    public static Result<Estoque> Criar(int valor)
    {
        if (valor < 0)
        {
            return Result<Estoque>.Fail(ProdutoErros.EstoqueNegativo);
        }

        return Result<Estoque>.Ok(new Estoque(valor));
    }

    public bool PossuiDisponivel(int quantidade) => quantidade <= Valor;

    public Estoque Baixar(int quantidade) => new(Valor - quantidade);

    public Estoque Repor(int quantidade) => new(Valor + quantidade);

    public override string ToString() => Valor.ToString();
}
