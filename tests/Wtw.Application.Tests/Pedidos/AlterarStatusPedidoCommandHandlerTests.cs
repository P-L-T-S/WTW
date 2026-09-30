using Wtw.Application.Pedidos.CancelarPedido;
using Wtw.Application.Pedidos.ConfirmarPedido;
using Wtw.Application.Pedidos.FinalizarPedido;
using Wtw.Application.Tests.Fakes;
using Wtw.Domain.Pedidos;
using Wtw.Domain.Pedidos.Eventos;
using Wtw.Domain.Results;

namespace Wtw.Application.Tests.Pedidos;

public sealed class AlterarStatusPedidoCommandHandlerTests
{
    private readonly Pedido _pedido = Dados.Pedido(Dados.Cliente(), (Dados.Produto(estoque: 10), 2));
    private readonly UnitOfWorkFalso _unitOfWork = new();
    private readonly PedidoRepositoryEmMemoria _pedidos;

    public AlterarStatusPedidoCommandHandlerTests()
    {
        _pedidos = new PedidoRepositoryEmMemoria(_pedido);
    }

    [Fact]
    public async Task Confirmar_PedidoCriado_DeveConfirmarDispararEventoESalvar()
    {
        var resultado = await new ConfirmarPedidoCommandHandler(_pedidos, _unitOfWork)
            .Handle(new ConfirmarPedidoCommand(_pedido.Id), TestContext.Current.CancellationToken);

        resultado.Success.ShouldBeTrue();
        _pedido.Status.ShouldBe(StatusPedido.Confirmado);
        _pedido.Eventos.OfType<PedidoConfirmadoDomainEvent>().ShouldHaveSingleItem();
        _unitOfWork.Chamadas.ShouldBe(1);
    }

    [Fact]
    public async Task Confirmar_PedidoInexistente_DeveRetornarNaoEncontradoSemSalvar()
    {
        var resultado = await new ConfirmarPedidoCommandHandler(_pedidos, _unitOfWork)
            .Handle(new ConfirmarPedidoCommand(Guid.CreateVersion7()), TestContext.Current.CancellationToken);

        resultado.Erro!.Tipo.ShouldBe(TipoErro.NaoEncontrado);
        _unitOfWork.Chamadas.ShouldBe(0);
    }

    [Fact]
    public async Task Confirmar_PedidoJaConfirmado_DeveRetornarRegraDeNegocioSemSalvar()
    {
        _pedido.Confirmar();

        var resultado = await new ConfirmarPedidoCommandHandler(_pedidos, _unitOfWork)
            .Handle(new ConfirmarPedidoCommand(_pedido.Id), TestContext.Current.CancellationToken);

        resultado.Erro!.Tipo.ShouldBe(TipoErro.RegraNegocio);
        _unitOfWork.Chamadas.ShouldBe(0);
    }

    [Fact]
    public async Task Confirmar_FalhaNoHandlerDoEvento_DevePropagarOErroDoUnitOfWork()
    {
        var estoqueInsuficiente = new Erro("Produto.BaixarEstoque", "Estoque insuficiente");
        var unitOfWork = new UnitOfWorkFalso(Result.Fail(estoqueInsuficiente));

        var resultado = await new ConfirmarPedidoCommandHandler(_pedidos, unitOfWork)
            .Handle(new ConfirmarPedidoCommand(_pedido.Id), TestContext.Current.CancellationToken);

        resultado.Erro.ShouldBeSameAs(estoqueInsuficiente);
    }

    [Fact]
    public async Task Cancelar_PedidoConfirmado_DeveDispararEventoDeReposicao()
    {
        _pedido.Confirmar();
        _pedido.LimparEventos();

        var resultado = await new CancelarPedidoCommandHandler(_pedidos, _unitOfWork)
            .Handle(new CancelarPedidoCommand(_pedido.Id), TestContext.Current.CancellationToken);

        resultado.Success.ShouldBeTrue();
        _pedido.Status.ShouldBe(StatusPedido.Cancelado);
        _pedido.Eventos.OfType<PedidoCanceladoDomainEvent>().ShouldHaveSingleItem();
    }

    [Fact]
    public async Task Finalizar_PedidoCriado_DeveRetornarRegraDeNegocioSemSalvar()
    {
        var resultado = await new FinalizarPedidoCommandHandler(_pedidos, _unitOfWork)
            .Handle(new FinalizarPedidoCommand(_pedido.Id), TestContext.Current.CancellationToken);

        resultado.Erro!.Metodo.ShouldBe("Pedido.Finalizar");
        _pedido.Status.ShouldBe(StatusPedido.Criado);
        _unitOfWork.Chamadas.ShouldBe(0);
    }
}
