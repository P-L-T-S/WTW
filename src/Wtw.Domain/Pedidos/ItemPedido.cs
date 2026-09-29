using Wtw.Domain.Produtos;

namespace Wtw.Domain.Pedidos;

public sealed class ItemPedido : Entity
{
    private ItemPedido()
    {
        NomeProduto = string.Empty;
    }

    internal ItemPedido(Guid pedidoId, Produto produto, int quantidade)
    {
        PedidoId = pedidoId;
        ProdutoId = produto.Id;
        NomeProduto = produto.Nome.Valor;
        PrecoUnitario = produto.Preco.Valor;
        Quantidade = quantidade;
    }

    public Guid PedidoId { get; private set; }
    public Guid ProdutoId { get; private set; }

    // Nome e preço são copiados no momento da inclusão para preservar o histórico do pedido.
    public string NomeProduto { get; private set; }
    public decimal PrecoUnitario { get; private set; }

    public int Quantidade { get; private set; }
    public decimal Subtotal => PrecoUnitario * Quantidade;

    internal void AlterarQuantidade(int quantidade) => Quantidade = quantidade;
}
