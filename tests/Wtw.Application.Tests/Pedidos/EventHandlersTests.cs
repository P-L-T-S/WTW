using Wtw.Application.Comum;
using Wtw.Application.Pedidos.EventHandlers;
using Wtw.Application.Tests.Fakes;
using Wtw.Domain.Pedidos.Eventos;

namespace Wtw.Application.Tests.Pedidos;

public sealed class EventHandlersTests
{
    [Fact]
    public async Task BaixarEstoque_DeveReduzirOEstoqueDeCadaProdutoDoEvento()
    {
        var webcam = Dados.Produto("Webcam", estoque: 3);
        var teclado = Dados.Produto("Teclado", estoque: 10);
        var handler = new BaixarEstoqueAoConfirmarPedidoHandler(new ProdutoRepositoryEmMemoria(webcam, teclado));

        await handler.Handle(
            new PedidoConfirmadoDomainEvent(Guid.CreateVersion7(),
                [new ItemMovimentado(webcam.Id, 2), new ItemMovimentado(teclado.Id, 4)]),
            TestContext.Current.CancellationToken);

        webcam.Estoque.Valor.ShouldBe(1);
        teclado.Estoque.Valor.ShouldBe(6);
    }

    [Fact]
    public async Task BaixarEstoque_Insuficiente_DeveLancarFalhaEventoDominioComOErroDoDominio()
    {
        var webcam = Dados.Produto("Webcam", estoque: 1);
        var handler = new BaixarEstoqueAoConfirmarPedidoHandler(new ProdutoRepositoryEmMemoria(webcam));

        var excecao = await Should.ThrowAsync<FalhaEventoDominioException>(() => handler.Handle(
            new PedidoConfirmadoDomainEvent(Guid.CreateVersion7(), [new ItemMovimentado(webcam.Id, 2)]),
            TestContext.Current.CancellationToken));

        excecao.Erro.Metodo.ShouldBe("Produto.BaixarEstoque");
        excecao.Erro.Mensagem.ShouldBe("Estoque insuficiente para o produto 'Webcam'. Disponível: 1, solicitado: 2.");
        webcam.Estoque.Valor.ShouldBe(1);
    }

    [Fact]
    public async Task BaixarEstoque_ProdutoInexistente_DeveLancarInvalidOperation()
    {
        var handler = new BaixarEstoqueAoConfirmarPedidoHandler(new ProdutoRepositoryEmMemoria());

        await Should.ThrowAsync<InvalidOperationException>(() => handler.Handle(
            new PedidoConfirmadoDomainEvent(Guid.CreateVersion7(), [new ItemMovimentado(Guid.CreateVersion7(), 1)]),
            TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ReporEstoque_DeveAumentarOEstoqueDeCadaProdutoDoEvento()
    {
        var webcam = Dados.Produto("Webcam", estoque: 0);
        var handler = new ReporEstoqueAoCancelarPedidoHandler(new ProdutoRepositoryEmMemoria(webcam));

        await handler.Handle(
            new PedidoCanceladoDomainEvent(Guid.CreateVersion7(), [new ItemMovimentado(webcam.Id, 2)]),
            TestContext.Current.CancellationToken);

        webcam.Estoque.Valor.ShouldBe(2);
    }
}
