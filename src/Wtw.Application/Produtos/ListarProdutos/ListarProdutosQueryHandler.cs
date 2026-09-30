using Microsoft.Extensions.Logging;
using Wtw.Application.Abstractions.Messaging;
using Wtw.Application.Produtos.Dtos;
using Wtw.Domain.Produtos;
using Wtw.Domain.Results;

namespace Wtw.Application.Produtos.ListarProdutos;

internal sealed class ListarProdutosQueryHandler(IProdutoRepository produtos)
    : IQueryHandler<ListarProdutosQuery, IReadOnlyList<ProdutoDto>>
{
    public async Task<Result<IReadOnlyList<ProdutoDto>>> Handle(ListarProdutosQuery query,
        CancellationToken cancellationToken)
    {
        var lista = await produtos.ListarAsync(cancellationToken);

        return Result<IReadOnlyList<ProdutoDto>>.Ok(lista.Select(produto => produto.ToDto()).ToList());
    }
}