using Wtw.Domain.Pedidos;
using Wtw.Domain.Produtos;

namespace Wtw.Domain.Tests.Comum;

internal static class PedidoTeste
{
    public static readonly Guid ClienteId = Guid.CreateVersion7();

    public static Pedido Criar(params (Produto Produto, int Quantidade)[] itens) =>
        Pedido.Criar(ClienteId, itens.Length > 0 ? itens : [(ProdutoTeste.Criar(), 1)]).Data!;

    public static Pedido NoStatus(StatusPedido status)
    {
        var pedido = Criar();

        switch (status)
        {
            case StatusPedido.Confirmado:
                pedido.Confirmar();
                break;
            case StatusPedido.Cancelado:
                pedido.Cancelar();
                break;
            case StatusPedido.Finalizado:
                pedido.Confirmar();
                pedido.Finalizar();
                break;
        }

        pedido.Status.ShouldBe(status);
        pedido.LimparEventos();

        return pedido;
    }
}
