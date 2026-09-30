using Wtw.Domain.Clientes;
using Wtw.Domain.Comum;
using Wtw.Domain.Produtos;
using Wtw.Domain.Results;

namespace Wtw.Domain.Tests.ValueObjects;

public sealed class ValueObjectsTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("A")]
    [InlineData(" A ")]
    public void Nome_VazioOuCurto_DeveRetornarFalhaDeValidacao(string? valor)
    {
        var resultado = Nome.Criar(valor);

        resultado.IsFailure.ShouldBeTrue();
        resultado.Erro!.Tipo.ShouldBe(TipoErro.Validacao);
    }

    [Fact]
    public void Nome_AcimaDoLimite_DeveRetornarFalha()
    {
        Nome.Criar(new string('a', Nome.TamanhoMaximo + 1)).IsFailure.ShouldBeTrue();
    }

    [Fact]
    public void Nome_Valido_DeveSerNormalizadoSemEspacosNasPontas()
    {
        Nome.Criar("  Teclado mecânico  ").Data.Valor.ShouldBe("Teclado mecânico");
    }

    [Fact]
    public void Descricao_Nula_DeveVirarVazia()
    {
        Descricao.Criar(null).Data.Valor.ShouldBeEmpty();
    }

    [Fact]
    public void Descricao_AcimaDoLimite_DeveRetornarFalha()
    {
        Descricao.Criar(new string('a', Descricao.TamanhoMaximo + 1)).IsFailure.ShouldBeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    [InlineData(-10)]
    public void Preco_MenorOuIgualAZero_DeveRetornarFalha(decimal valor)
    {
        Preco.Criar(valor).IsFailure.ShouldBeTrue();
    }

    [Fact]
    public void Preco_Valido_DeveSerArredondadoParaDuasCasas()
    {
        Preco.Criar(29.999m).Data.Valor.ShouldBe(30.00m);
    }

    [Fact]
    public void Estoque_Negativo_DeveRetornarFalha()
    {
        Estoque.Criar(-1).IsFailure.ShouldBeTrue();
    }

    [Fact]
    public void Estoque_Zero_DeveSerValido()
    {
        Estoque.Criar(0).Data.Valor.ShouldBe(0);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("sem-arroba")]
    [InlineData("a@")]
    [InlineData("@dominio.com")]
    public void Email_Invalido_DeveRetornarFalha(string? valor)
    {
        Email.Criar(valor).IsFailure.ShouldBeTrue();
    }

    [Fact]
    public void Email_Valido_DeveSerNormalizadoEmMinusculas()
    {
        Email.Criar("  Ana.Souza@Exemplo.com ").Data.Valor.ShouldBe("ana.souza@exemplo.com");
    }

    [Fact]
    public void ValueObjects_ComMesmoValor_DevemSerIguais()
    {
        Nome.Criar("Webcam").Data.ShouldBe(Nome.Criar(" Webcam ").Data);
    }
}
