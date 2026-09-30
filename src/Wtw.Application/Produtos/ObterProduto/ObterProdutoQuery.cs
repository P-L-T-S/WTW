using Wtw.Application.Abstractions.Messaging;
using Wtw.Application.Produtos.Dtos;

namespace Wtw.Application.Produtos.ObterProduto;

public sealed record ObterProdutoQuery(Guid Id) : IQuery<ProdutoDto>;
