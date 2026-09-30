using Wtw.Application.Comum;
using Wtw.Application.Pedidos.CriarPedido;
using Wtw.Application.Tests.Fakes;
using Wtw.Domain.Clientes;
using Wtw.Domain.Pedidos;
using Wtw.Domain.Produtos;
using Wtw.Domain.Results;

namespace Wtw.Application.Tests.Pedidos;

public sealed class CriarPedidoCommandHandlerTests
{
    private readonly Cliente _cliente = Dados.Cliente();
    private readonly Produto _webcam = Dados.Produto("Webcam", preco: 159m, estoque: 3);
    private readonly PedidoRepositoryEmMemoria _pedidos = new();
    private readonly UnitOfWorkFalso _unitOfWork = new();

    private CriarPedidoCommandHandler CriarHandler(UnitOfWorkFalso? unitOfWork = null) => new(
        _pedidos,
        new ProdutoRepositoryEmMemoria(_webcam),
        new ClienteRepositoryEmMemoria(_cliente),
        unitOfWork ?? _unitOfWork,
        new CriarPedidoCommandValidator());

    [Fact]
    public async Task Handle_ComandoValido_DeveCriarPedidoESalvar()
    {
        var resultado = await CriarHandler().Handle(
            new CriarPedidoCommand(_cliente.Id, [new ItemPedidoCommand(_webcam.Id, 2)]),
            TestContext.Current.CancellationToken);

        resultado.Success.ShouldBeTrue();
        var pedido = _pedidos.Pedidos.Single();
        pedido.Id.ShouldBe(resultado.Data);
        pedido.Status.ShouldBe(StatusPedido.Criado);
        pedido.ValorTotal.ShouldBe(318m);
        _unitOfWork.Chamadas.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_SemClienteESemItens_DeveRetornarErroDeValidacaoPorCampo()
    {
        var resultado = await CriarHandler().Handle(
            new CriarPedidoCommand(Guid.Empty, []), TestContext.Current.CancellationToken);

        var erro = resultado.Erro.ShouldBeOfType<ErroValidacao>();
        erro.Campos.Keys.ShouldBe(["ClienteId", "Itens"], ignoreOrder: true);
        NadaFoiSalvo();
    }

    [Fact]
    public async Task Handle_ClienteInexistente_DeveRetornarNaoEncontrado()
    {
        var resultado = await CriarHandler().Handle(
            new CriarPedidoCommand(Guid.CreateVersion7(), [new ItemPedidoCommand(_webcam.Id, 1)]),
            TestContext.Current.CancellationToken);

        resultado.Erro!.Tipo.ShouldBe(TipoErro.NaoEncontrado);
        resultado.Erro.Metodo.ShouldBe("Cliente.NaoEncontrado");
        NadaFoiSalvo();
    }

    [Fact]
    public async Task Handle_ProdutoInexistente_DeveRetornarNaoEncontrado()
    {
        var resultado = await CriarHandler().Handle(
            new CriarPedidoCommand(_cliente.Id, [new ItemPedidoCommand(Guid.CreateVersion7(), 1)]),
            TestContext.Current.CancellationToken);

        resultado.Erro!.Tipo.ShouldBe(TipoErro.NaoEncontrado);
        resultado.Erro.Metodo.ShouldBe("Produto.NaoEncontrado");
        NadaFoiSalvo();
    }

    [Fact]
    public async Task Handle_QuantidadeAcimaDoEstoque_DeveRetornarRegraDeNegocio()
    {
        var resultado = await CriarHandler().Handle(
            new CriarPedidoCommand(_cliente.Id, [new ItemPedidoCommand(_webcam.Id, 4)]),
            TestContext.Current.CancellationToken);

        resultado.Erro!.Tipo.ShouldBe(TipoErro.RegraNegocio);
        NadaFoiSalvo();
    }

    [Fact]
    public async Task Handle_FalhaAoSalvar_DevePropagarOErroDoUnitOfWork()
    {
        var conflito = new Erro("UnitOfWork.SaveChanges", "Conflito") { Tipo = TipoErro.Conflito };

        var resultado = await CriarHandler(new UnitOfWorkFalso(Result.Fail(conflito))).Handle(
            new CriarPedidoCommand(_cliente.Id, [new ItemPedidoCommand(_webcam.Id, 1)]),
            TestContext.Current.CancellationToken);

        resultado.Erro.ShouldBeSameAs(conflito);
    }

    private void NadaFoiSalvo()
    {
        _pedidos.Pedidos.ShouldBeEmpty();
        _unitOfWork.Chamadas.ShouldBe(0);
    }
}
