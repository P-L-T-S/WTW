using Wtw.Application.Abstractions.Messaging;
using Wtw.Application.Produtos.Comum;

namespace Wtw.Application.Produtos.AtualizarProduto;

public sealed record AtualizarProdutoCommand(Guid Id, string Nome, string? Descricao, decimal Preco, int QuantidadeEstoque)
    : ICommand, IDadosProduto;
