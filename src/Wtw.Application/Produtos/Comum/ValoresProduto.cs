using Wtw.Domain.Comum;
using Wtw.Domain.Produtos;
using Wtw.Domain.Results;

namespace Wtw.Application.Produtos.Comum;

internal sealed record ValoresProduto(Nome Nome, Descricao Descricao, Preco Preco, Estoque Estoque)
{
    public static Result<ValoresProduto> Criar(IDadosProduto dados)
    {
        var nome = Nome.Criar(dados.Nome);
        var descricao = Descricao.Criar(dados.Descricao);
        var preco = Preco.Criar(dados.Preco);
        var estoque = Estoque.Criar(dados.QuantidadeEstoque);

        Result[] resultados = [nome, descricao, preco, estoque];
        var falha = resultados.FirstOrDefault(resultado => resultado.IsFailure);

        return falha is null
            ? Result<ValoresProduto>.Ok(new ValoresProduto(nome.Data, descricao.Data, preco.Data, estoque.Data))
            : Result<ValoresProduto>.Fail(falha.Erro!);
    }
}
