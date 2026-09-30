namespace Wtw.Application.Produtos.Comum;

public interface IDadosProduto
{
    string Nome { get; }
    string? Descricao { get; }
    decimal Preco { get; }
    int QuantidadeEstoque { get; }
}
