using Wtw.Domain.Pedidos;
using Wtw.Domain.Pedidos.Eventos;
using Wtw.Domain.Results;
using Wtw.Domain.Tests.Comum;

namespace Wtw.Domain.Tests.Pedidos;

public sealed class PedidoStatusTests
{
    public enum Transicao
    {
        Confirmar,
        Cancelar,
        Finalizar
    }

    [Theory]
    [InlineData(StatusPedido.Criado, Transicao.Confirmar, true, StatusPedido.Confirmado)]
    [InlineData(StatusPedido.Criado, Transicao.Cancelar, true, StatusPedido.Cancelado)]
    [InlineData(StatusPedido.Criado, Transicao.Finalizar, false, StatusPedido.Criado)]
    [InlineData(StatusPedido.Confirmado, Transicao.Confirmar, false, StatusPedido.Confirmado)]
    [InlineData(StatusPedido.Confirmado, Transicao.Cancelar, true, StatusPedido.Cancelado)]
    [InlineData(StatusPedido.Confirmado, Transicao.Finalizar, true, StatusPedido.Finalizado)]
    [InlineData(StatusPedido.Cancelado, Transicao.Confirmar, false, StatusPedido.Cancelado)]
    [InlineData(StatusPedido.Cancelado, Transicao.Cancelar, false, StatusPedido.Cancelado)]
    [InlineData(StatusPedido.Cancelado, Transicao.Finalizar, false, StatusPedido.Cancelado)]
    [InlineData(StatusPedido.Finalizado, Transicao.Confirmar, false, StatusPedido.Finalizado)]
    [InlineData(StatusPedido.Finalizado, Transicao.Cancelar, false, StatusPedido.Finalizado)]
    [InlineData(StatusPedido.Finalizado, Transicao.Finalizar, false, StatusPedido.Finalizado)]
    public void Transicoes_DevemSeguirAMatrizDeStatus_RC01(
        StatusPedido statusInicial, Transicao transicao, bool permitida, StatusPedido statusFinal)
    {
        var pedido = PedidoTeste.NoStatus(statusInicial);

        var resultado = Aplicar(pedido, transicao);

        resultado.Success.ShouldBe(permitida);
        pedido.Status.ShouldBe(statusFinal);

        if (!permitida)
        {
            resultado.Erro!.Tipo.ShouldBe(TipoErro.RegraNegocio);
        }
    }

    [Fact]
    public void Confirmar_DeveDispararEventoComOsItensDoPedido_RN03()
    {
        var webcam = ProdutoTeste.Criar("Webcam", estoque: 10);
        var teclado = ProdutoTeste.Criar("Teclado", estoque: 10);
        var pedido = PedidoTeste.Criar((webcam, 2), (teclado, 1));

        pedido.Confirmar();

        var evento = pedido.Eventos.OfType<PedidoConfirmadoDomainEvent>().Single();
        evento.PedidoId.ShouldBe(pedido.Id);
        evento.Itens.ShouldBe([new ItemMovimentado(webcam.Id, 2), new ItemMovimentado(teclado.Id, 1)],
            ignoreOrder: true);
    }

    [Fact]
    public void Confirmar_NaoDeveAlterarOEstoqueDoProduto_RN03()
    {
        var produto = ProdutoTeste.Criar(estoque: 3);
        var pedido = PedidoTeste.Criar((produto, 2));

        pedido.Confirmar();

        produto.Estoque.Valor.ShouldBe(3);
    }

    [Fact]
    public void Cancelar_PedidoConfirmado_DeveDispararEventoParaRestaurarEstoque_RN04()
    {
        var produto = ProdutoTeste.Criar(estoque: 10);
        var pedido = PedidoTeste.Criar((produto, 2));
        pedido.Confirmar();
        pedido.LimparEventos();

        pedido.Cancelar();

        var evento = pedido.Eventos.OfType<PedidoCanceladoDomainEvent>().Single();
        evento.PedidoId.ShouldBe(pedido.Id);
        evento.Itens.ShouldBe([new ItemMovimentado(produto.Id, 2)]);
    }

    [Fact]
    public void Cancelar_PedidoCriado_NaoDeveDispararEvento_RN04()
    {
        var pedido = PedidoTeste.Criar();

        pedido.Cancelar();

        pedido.Status.ShouldBe(StatusPedido.Cancelado);
        pedido.Eventos.ShouldBeEmpty();
    }

    [Fact]
    public void Cancelar_PedidoFinalizado_DeveRetornarMensagemEspecifica_RN05()
    {
        var pedido = PedidoTeste.NoStatus(StatusPedido.Finalizado);

        var resultado = pedido.Cancelar();

        resultado.Erro!.Mensagem.ShouldBe(PedidoErros.FinalizadoNaoPodeSerCancelado.Mensagem);
        pedido.Eventos.ShouldBeEmpty();
    }

    [Fact]
    public void TransicaoRejeitada_NaoDeveDispararEventos()
    {
        var pedido = PedidoTeste.NoStatus(StatusPedido.Confirmado);

        pedido.Confirmar();

        pedido.Eventos.ShouldBeEmpty();
    }

    private static Result Aplicar(Pedido pedido, Transicao transicao) => transicao switch
    {
        Transicao.Confirmar => pedido.Confirmar(),
        Transicao.Cancelar => pedido.Cancelar(),
        _ => pedido.Finalizar()
    };
}
