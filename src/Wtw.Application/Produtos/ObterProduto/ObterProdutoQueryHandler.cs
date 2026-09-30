using Wtw.Application.Abstractions.Messaging;
using Wtw.Application.Comum;
using Wtw.Application.Produtos.Dtos;
using Wtw.Domain.Produtos;
using Wtw.Domain.Results;

namespace Wtw.Application.Produtos.ObterProduto;

internal sealed class ObterProdutoQueryHandler(IProdutoRepository produtos)
    : IQueryHandler<ObterProdutoQuery, ProdutoDto>
{
    public async Task<Result<ProdutoDto>> Handle(ObterProdutoQuery query, CancellationToken cancellationToken)
    {
        var produto = await produtos.ObterPorIdAsync(query.Id, cancellationToken);

        return produto is null
            ? Result<ProdutoDto>.Fail(ApplicationErros.NaoEncontrado(nameof(Produto), query.Id))
            : Result<ProdutoDto>.Ok(produto.ToDto());
    }
}
