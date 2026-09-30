using Wtw.Application.Abstractions.Messaging;
using Wtw.Application.Produtos.Dtos;

namespace Wtw.Application.Produtos.ListarProdutos;

public sealed record ListarProdutosQuery : IQuery<IReadOnlyList<ProdutoDto>>;
