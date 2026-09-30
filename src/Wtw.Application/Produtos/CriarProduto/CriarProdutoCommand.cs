using Wtw.Application.Abstractions.Messaging;
using Wtw.Application.Produtos.Comum;

namespace Wtw.Application.Produtos.CriarProduto;

public sealed record CriarProdutoCommand(string Nome, string? Descricao, decimal Preco, int QuantidadeEstoque)
    : ICommand<Guid>, IDadosProduto;
