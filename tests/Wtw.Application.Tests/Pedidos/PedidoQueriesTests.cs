using Wtw.Application.Pedidos.ListarPedidos;
using Wtw.Application.Pedidos.ObterPedido;
using Wtw.Application.Tests.Fakes;
using Wtw.Domain.Pedidos;
using Wtw.Domain.Results;

namespace Wtw.Application.Tests.Pedidos;

public sealed class PedidoQueriesTests
{
    [Fact]
    public async Task Listar_DeveMontarResumoComNomeDoClienteEFiltrarPorStatus()
    {
        var ana = Dados.Cliente("Ana Souza", "ana@exemplo.com");
        var bruno = Dados.Cliente("Bruno Lima", "bruno@exemplo.com");
        var produto = Dados.Produto(preco: 100m, estoque: 10);
        var pedidoAna = Dados.Pedido(ana, (produto, 2));
        var pedidoBruno = Dados.Pedido(bruno, (produto, 1));
        pedidoBruno.Confirmar();

        var handler = new ListarPedidosQueryHandler(
            new PedidoRepositoryEmMemoria(pedidoAna, pedidoBruno), new ClienteRepositoryEmMemoria(ana, bruno));

        var resultado = await handler.Handle(new ListarPedidosQuery(StatusPedido.Criado), TestContext.Current.CancellationToken);

        var resumo = resultado.Data!.ShouldHaveSingleItem();
        resumo.Id.ShouldBe(pedidoAna.Id);
        resumo.NomeCliente.ShouldBe("Ana Souza");
        resumo.QuantidadeItens.ShouldBe(1);
        resumo.ValorTotal.ShouldBe(200m);
    }

    [Fact]
    public async Task Obter_PedidoInexistente_DeveRetornarNaoEncontrado()
    {
        var handler = new ObterPedidoQueryHandler(new PedidoRepositoryEmMemoria(), new ClienteRepositoryEmMemoria());

        var resultado = await handler.Handle(new ObterPedidoQuery(Guid.CreateVersion7()), TestContext.Current.CancellationToken);

        resultado.Erro!.Tipo.ShouldBe(TipoErro.NaoEncontrado);
    }

    [Fact]
    public async Task Obter_DeveRetornarItensComPrecoDoMomentoDaInclusao()
    {
        var cliente = Dados.Cliente();
        var produto = Dados.Produto("Webcam", preco: 159m, estoque: 10);
        var pedido = Dados.Pedido(cliente, (produto, 2));
        var handler = new ObterPedidoQueryHandler(new PedidoRepositoryEmMemoria(pedido), new ClienteRepositoryEmMemoria(cliente));

        var resultado = await handler.Handle(new ObterPedidoQuery(pedido.Id), TestContext.Current.CancellationToken);

        var dto = resultado.Data!;
        dto.NomeCliente.ShouldBe("Ana Souza");
        dto.ValorTotal.ShouldBe(318m);
        var item = dto.Itens.ShouldHaveSingleItem();
        item.NomeProduto.ShouldBe("Webcam");
        item.PrecoUnitario.ShouldBe(159m);
        item.Subtotal.ShouldBe(318m);
    }
}
