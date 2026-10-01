using Wtw.Domain.Comum;
using Wtw.Domain.Pedidos;
using Wtw.Domain.Produtos;
using Wtw.Domain.Tests.Comum;

namespace Wtw.Domain.Tests.Pedidos;

public sealed class PedidoItensTests
{
    [Fact]
    public void Criar_SemItens_DeveRetornarFalha_RN01()
    {
        var resultado = Pedido.Criar(PedidoTeste.ClienteId, []);

        resultado.IsFailure.ShouldBeTrue();
        resultado.Erro!.Mensagem.ShouldBe(PedidoErros.SemItens.Mensagem);
    }

    [Fact]
    public void Criar_ComListaNula_DeveRetornarFalha_RN01()
    {
        var resultado = Pedido.Criar(PedidoTeste.ClienteId, null);
        resultado.IsFailure.ShouldBeTrue();
        resultado.Erro.Mensagem.ShouldBe(PedidoErros.SemItens.Mensagem);
    }

    [Fact]
    public void Criar_SemCliente_DeveRetornarFalha()
    {
        var resultado = Pedido.Criar(Guid.Empty, [(ProdutoTeste.Criar(), 1)]);
        resultado.IsFailure.ShouldBeTrue();
        resultado.Erro.Mensagem.ShouldBe(PedidoErros.ClienteObrigatorio.Mensagem);
    }

    [Fact]
    public void Criar_ComItensValidos_DeveIniciarComoCriadoESemEventos()
    {
        var pedido = PedidoTeste.Criar((ProdutoTeste.Criar(), 2));

        pedido.Status.ShouldBe(StatusPedido.Criado);
        pedido.ClienteId.ShouldBe(PedidoTeste.ClienteId);
        pedido.Itens.Count.ShouldBe(1);
        pedido.Eventos.ShouldBeEmpty();
    }

    [Fact]
    public void Criar_ItemAcimaDoEstoque_DeveRetornarFalha_RN02()
    {
        var produto = ProdutoTeste.Criar(estoque: 3);

        var resultado = Pedido.Criar(PedidoTeste.ClienteId, [(produto, 4)]);

        resultado.IsFailure.ShouldBeTrue();
        resultado.Erro.Mensagem.ShouldBe(PedidoErros.EstoqueInsuficiente(produto, 4).Mensagem);
    }

    [Fact]
    public void Criar_ItemComEstoqueExato_DeveSerPermitido_RN02()
    {
        Pedido.Criar(PedidoTeste.ClienteId, [(ProdutoTeste.Criar(estoque: 3), 3)]).Success.ShouldBeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AdicionarItem_QuantidadeNaoPositiva_DeveRetornarFalha(int quantidade)
    {
        var pedido = PedidoTeste.Criar();

        var resultado = pedido.AdicionarItem(ProdutoTeste.Criar("Mouse"), quantidade);
        resultado.IsFailure.ShouldBeTrue();
        resultado.Erro.Mensagem.ShouldBe(PedidoErros.QuantidadeInvalida.Mensagem);
    }

    [Fact]
    public void AdicionarItem_ProdutoRepetido_DeveSomarAQuantidade_RC04()
    {
        var produto = ProdutoTeste.Criar(estoque: 5);
        var pedido = PedidoTeste.Criar((produto, 2));

        var resultado = pedido.AdicionarItem(produto, 2);

        resultado.Success.ShouldBeTrue();
        pedido.Itens.Single().Quantidade.ShouldBe(4);
    }

    [Fact]
    public void AdicionarItem_SomaAcimaDoEstoque_DeveRetornarFalhaSemAlterarItem_RC04()
    {
        var produto = ProdutoTeste.Criar(estoque: 3);
        var pedido = PedidoTeste.Criar((produto, 2));

        var resultado = pedido.AdicionarItem(produto, 2);

        resultado.IsFailure.ShouldBeTrue();
        resultado.Erro!.Mensagem.ShouldBe(PedidoErros.EstoqueInsuficiente(produto, 4).Mensagem);
        pedido.Itens.Single().Quantidade.ShouldBe(2);
    }

    [Fact]
    public void Criar_ProdutoRepetidoNaMesmaLista_DeveAgruparEmUmItem_RC04()
    {
        var produto = ProdutoTeste.Criar(estoque: 5);

        var pedido = PedidoTeste.Criar((produto, 2), (produto, 3));

        pedido.Itens.Single().Quantidade.ShouldBe(5);
    }

    [Fact]
    public void Item_DeveGuardarNomeEPrecoDoMomentoDaInclusao_RC05()
    {
        var produto = ProdutoTeste.Criar("Webcam HD", preco: 159m, estoque: 10);
        var pedido = PedidoTeste.Criar((produto, 2));

        produto.Atualizar(Nome.Criar("Webcam 4K").Data, produto.Descricao, Preco.Criar(299m).Data, produto.Estoque);

        var item = pedido.Itens.Single();
        item.NomeProduto.ShouldBe("Webcam HD");
        item.PrecoUnitario.ShouldBe(159m);
        item.Subtotal.ShouldBe(318m);
    }

    [Fact]
    public void Item_DeveReferenciarPedidoEProdutoPorId()
    {
        var produto = ProdutoTeste.Criar();
        var pedido = PedidoTeste.Criar((produto, 1));

        var item = pedido.Itens.Single();
        item.PedidoId.ShouldBe(pedido.Id);
        item.ProdutoId.ShouldBe(produto.Id);
    }

    [Fact]
    public void ValorTotal_DeveSerASomaDosSubtotais()
    {
        var produto1 = ProdutoTeste.Criar("Webcam", preco: 159m, estoque: 10);
        var produto2 = ProdutoTeste.Criar("Teclado", preco: 349.90m, estoque: 10);

        var pedido = PedidoTeste.Criar(
            (produto1, 2), (produto2, 1)
        );

        pedido.ValorTotal.ShouldBe(667.90m);
    }

    [Fact]
    public void AlterarQuantidadeItem_DentroDoEstoque_DeveSubstituirAQuantidade()
    {
        var produto = ProdutoTeste.Criar(estoque: 10);
        var pedido = PedidoTeste.Criar((produto, 2));

        pedido.AlterarQuantidadeItem(produto, 7).Success.ShouldBeTrue();
        pedido.Itens.Single().Quantidade.ShouldBe(7);
    }

    [Fact]
    public void AlterarQuantidadeItem_AcimaDoEstoque_DeveRetornarFalha()
    {
        var produto = ProdutoTeste.Criar(estoque: 3);
        var pedido = PedidoTeste.Criar((produto, 1));

        pedido.AlterarQuantidadeItem(produto, 4).IsFailure.ShouldBeTrue();
        pedido.Itens.Single().Quantidade.ShouldBe(1);
    }

    [Fact]
    public void AlterarQuantidadeItem_ProdutoForaDoPedido_DeveRetornarFalha()
    {
        var pedido = PedidoTeste.Criar();

        pedido.AlterarQuantidadeItem(ProdutoTeste.Criar("Mouse"), 1).IsFailure.ShouldBeTrue();
    }

    [Fact]
    public void RemoverItem_ComMaisDeUmItem_DeveRemover()
    {
        var mouse = ProdutoTeste.Criar("Mouse");
        var pedido = PedidoTeste.Criar((ProdutoTeste.Criar(), 1), (mouse, 1));

        pedido.RemoverItem(mouse.Id).Success.ShouldBeTrue();
        pedido.Itens.Count.ShouldBe(1);
    }

    [Fact]
    public void RemoverItem_UltimoItem_DeveRetornarFalha_RC08()
    {
        var produto = ProdutoTeste.Criar();
        var pedido = PedidoTeste.Criar((produto, 1));

        var resultado = pedido.RemoverItem(produto.Id);

        resultado.IsFailure.ShouldBeTrue();
        resultado.Erro!.Mensagem.ShouldBe(PedidoErros.UltimoItem.Mensagem);
        pedido.Itens.Count.ShouldBe(1);
    }

    [Theory]
    [InlineData(StatusPedido.Confirmado)]
    [InlineData(StatusPedido.Cancelado)]
    [InlineData(StatusPedido.Finalizado)]
    public void AlterarItens_ForaDoStatusCriado_DeveRetornarFalha_RC02_RN05(StatusPedido status)
    {
        var pedido = PedidoTeste.NoStatus(status);
        var itemExistente = pedido.Itens.Single();
        var novoProduto = ProdutoTeste.Criar("Mouse", estoque: 10);

        pedido.AdicionarItem(novoProduto, 1).IsFailure.ShouldBeTrue();
        pedido.RemoverItem(itemExistente.ProdutoId).IsFailure.ShouldBeTrue();
        pedido.Itens.Count.ShouldBe(1);
    }

    [Fact]
    public void AlterarItens_ComSucesso_DeveRegistrarDataDeAtualizacao()
    {
        var pedido = PedidoTeste.Criar((ProdutoTeste.Criar(estoque: 10), 1));
        pedido.AtualizadoEmUtc.ShouldBeNull();

        pedido.AdicionarItem(ProdutoTeste.Criar("Mouse"), 1);

        pedido.AtualizadoEmUtc.ShouldNotBeNull();
    }
}