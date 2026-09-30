using Wtw.Domain.Produtos.Eventos;
using Wtw.Domain.Tests.Comum;

namespace Wtw.Domain.Tests.Produtos;

public sealed class ProdutoTests
{
    [Fact]
    public void Criar_DeveDispararProdutoCriadoDomainEvent()
    {
        var produto = ProdutoTeste.Criar();

        produto.Eventos.OfType<ProdutoCriadoDomainEvent>().Single().ProdutoId.ShouldBe(produto.Id);
    }

    [Fact]
    public void BaixarEstoque_QuantidadeDisponivel_DeveReduzirOEstoque()
    {
        var produto = ProdutoTeste.Criar(estoque: 10);

        var resultado = produto.BaixarEstoque(4);

        resultado.Success.ShouldBeTrue();
        produto.Estoque.Valor.ShouldBe(6);
    }

    [Fact]
    public void BaixarEstoque_TodoOEstoque_DeveZerar()
    {
        var produto = ProdutoTeste.Criar(estoque: 3);

        produto.BaixarEstoque(3).Success.ShouldBeTrue();
        produto.Estoque.Valor.ShouldBe(0);
    }

    [Fact]
    public void BaixarEstoque_AcimaDoDisponivel_DeveRetornarFalhaSemAlterarEstoque()
    {
        var produto = ProdutoTeste.Criar(estoque: 3);

        var resultado = produto.BaixarEstoque(4);

        resultado.IsFailure.ShouldBeTrue();
        resultado.Erro!.Mensagem.ShouldContain("Estoque insuficiente");
        produto.Estoque.Valor.ShouldBe(3);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void BaixarEstoque_QuantidadeNaoPositiva_DeveRetornarFalha(int quantidade)
    {
        ProdutoTeste.Criar().BaixarEstoque(quantidade).IsFailure.ShouldBeTrue();
    }

    [Fact]
    public void ReporEstoque_DeveAumentarOEstoque()
    {
        var produto = ProdutoTeste.Criar(estoque: 3);

        produto.ReporEstoque(2).Success.ShouldBeTrue();
        produto.Estoque.Valor.ShouldBe(5);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ReporEstoque_QuantidadeNaoPositiva_DeveRetornarFalha(int quantidade)
    {
        ProdutoTeste.Criar().ReporEstoque(quantidade).IsFailure.ShouldBeTrue();
    }
}
