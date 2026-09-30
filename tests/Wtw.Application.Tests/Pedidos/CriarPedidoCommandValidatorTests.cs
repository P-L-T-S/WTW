using Wtw.Application.Pedidos.CriarPedido;
using Wtw.Domain.Pedidos;

namespace Wtw.Application.Tests.Pedidos;

public sealed class CriarPedidoCommandValidatorTests
{
    private readonly CriarPedidoCommandValidator _validator = new();

    [Fact]
    public void Validar_ItemSemProdutoEComQuantidadeZero_DeveApontarOCampoDoItem()
    {
        var resultado = _validator.Validate(new CriarPedidoCommand(
            Guid.CreateVersion7(),
            [new ItemPedidoCommand(Guid.CreateVersion7(), 1), new ItemPedidoCommand(Guid.Empty, 0)]));

        resultado.Errors.Select(erro => (erro.PropertyName, erro.ErrorMessage)).ShouldBe(
        [
            ("Itens[1].ProdutoId", "Informe o produto do item."),
            ("Itens[1].Quantidade", PedidoErros.QuantidadeInvalida.Mensagem)
        ], ignoreOrder: true);
    }

    [Fact]
    public void Validar_ProdutoRepetido_DeveSerAceito_RC04()
    {
        var produtoId = Guid.CreateVersion7();

        var resultado = _validator.Validate(new CriarPedidoCommand(
            Guid.CreateVersion7(), [new ItemPedidoCommand(produtoId, 1), new ItemPedidoCommand(produtoId, 2)]));

        resultado.IsValid.ShouldBeTrue();
    }
}
