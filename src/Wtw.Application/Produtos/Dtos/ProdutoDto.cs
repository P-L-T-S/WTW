using Wtw.Domain.Produtos;

namespace Wtw.Application.Produtos.Dtos;

public sealed record ProdutoDto(Guid Id, string Nome, string Descricao, decimal Preco, int QuantidadeEstoque);

internal static class ProdutoMapeamento
{
    public static ProdutoDto ToDto(this Produto produto) =>
        new(produto.Id, produto.Nome.Valor, produto.Descricao.Valor, produto.Preco.Valor, produto.Estoque.Valor);
}
