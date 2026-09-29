using Wtw.Domain.Comum;
using Wtw.Domain.Produtos.Eventos;
using Wtw.Domain.Results;

namespace Wtw.Domain.Produtos;

public sealed class Produto : AggregateRoot
{
    // Construtor exigido pelo EF Core.
    private Produto()
    {
    }

    private Produto(Nome nome, Descricao descricao, Preco preco, Estoque estoque)
    {
        Nome = nome;
        Descricao = descricao;
        Preco = preco;
        Estoque = estoque;
    }

    public Nome Nome { get; private set; }
    public Descricao Descricao { get; private set; }
    public Preco Preco { get; private set; }
    public Estoque Estoque { get; private set; }

    public static Produto Criar(Nome nome, Descricao descricao, Preco preco, Estoque estoque)
    {
        var produto = new Produto(nome, descricao, preco, estoque);

        produto.Disparar(new ProdutoCriadoDomainEvent(produto.Id));

        return produto;
    }

    public void Atualizar(Nome nome, Descricao descricao, Preco preco, Estoque estoque)
    {
        Nome = nome;
        Descricao = descricao;
        Preco = preco;
        Estoque = estoque;
    }

    public bool PossuiEstoque(int quantidade) => Estoque.PossuiDisponivel(quantidade);

    public Result BaixarEstoque(int quantidade)
    {
        if (quantidade <= 0)
        {
            return Result.Fail(ProdutoErros.QuantidadeMovimentacaoInvalida);
        }

        if (!PossuiEstoque(quantidade))
        {
            return Result.Fail(ProdutoErros.EstoqueInsuficiente(this, quantidade));
        }

        Estoque = Estoque.Baixar(quantidade);

        return Result.Ok();
    }

    public Result ReporEstoque(int quantidade)
    {
        if (quantidade <= 0)
        {
            return Result.Fail(ProdutoErros.QuantidadeMovimentacaoInvalida);
        }

        Estoque = Estoque.Repor(quantidade);

        return Result.Ok();
    }
}
