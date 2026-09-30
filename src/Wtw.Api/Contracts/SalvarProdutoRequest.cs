namespace Wtw.Api.Contracts;

public sealed record SalvarProdutoRequest(string Nome, string? Descricao, decimal Preco, int QuantidadeEstoque);
