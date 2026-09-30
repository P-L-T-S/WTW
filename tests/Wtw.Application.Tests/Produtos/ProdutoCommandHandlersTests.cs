using Wtw.Application.Comum;
using Wtw.Application.Produtos.AtualizarProduto;
using Wtw.Application.Produtos.CriarProduto;
using Wtw.Application.Tests.Fakes;
using Wtw.Domain.Comum;
using Wtw.Domain.Produtos;
using Wtw.Domain.Results;

namespace Wtw.Application.Tests.Produtos;

public sealed class ProdutoCommandHandlersTests
{
    private readonly ProdutoRepositoryEmMemoria _produtos = new();
    private readonly UnitOfWorkFalso _unitOfWork = new();

    [Fact]
    public async Task Criar_ComandoValido_DeveCadastrarComValoresNormalizados()
    {
        var handler = new CriarProdutoCommandHandler(_produtos, _unitOfWork, new CriarProdutoCommandValidator());

        var resultado = await handler.Handle(
            new CriarProdutoCommand("  Cabo HDMI  ", null, 29.999m, 10), TestContext.Current.CancellationToken);

        resultado.Success.ShouldBeTrue();
        var produto = _produtos.Produtos.Single();
        produto.Id.ShouldBe(resultado.Data);
        produto.Nome.Valor.ShouldBe("Cabo HDMI");
        produto.Descricao.Valor.ShouldBeEmpty();
        produto.Preco.Valor.ShouldBe(30m);
        produto.Estoque.Valor.ShouldBe(10);
        _unitOfWork.Chamadas.ShouldBe(1);
    }

    [Fact]
    public async Task Criar_ComandoInvalido_DeveReportarTodosOsCamposComAsMensagensDoDominio()
    {
        var handler = new CriarProdutoCommandHandler(_produtos, _unitOfWork, new CriarProdutoCommandValidator());

        var resultado = await handler.Handle(
            new CriarProdutoCommand(" a", new string('x', 501), 0, -1), TestContext.Current.CancellationToken);

        var erro = resultado.Erro.ShouldBeOfType<ErroValidacao>();
        erro.Campos["Nome"].ShouldBe([ComumErros.NomeInvalido.Mensagem]);
        erro.Campos["Descricao"].ShouldBe([ProdutoErros.DescricaoMuitoLonga.Mensagem]);
        erro.Campos["Preco"].ShouldBe([ProdutoErros.PrecoInvalido.Mensagem]);
        erro.Campos["QuantidadeEstoque"].ShouldBe([ProdutoErros.EstoqueNegativo.Mensagem]);
        _produtos.Produtos.ShouldBeEmpty();
        _unitOfWork.Chamadas.ShouldBe(0);
    }

    [Fact]
    public async Task Atualizar_ProdutoExistente_DeveAlterarOsDados()
    {
        var produto = Dados.Produto("Webcam", preco: 159m, estoque: 3);
        _produtos.Adicionar(produto);
        var handler = new AtualizarProdutoCommandHandler(_produtos, _unitOfWork, new AtualizarProdutoCommandValidator());

        var resultado = await handler.Handle(
            new AtualizarProdutoCommand(produto.Id, "Webcam 4K", "Nova versão", 299m, 8), TestContext.Current.CancellationToken);

        resultado.Success.ShouldBeTrue();
        produto.Nome.Valor.ShouldBe("Webcam 4K");
        produto.Preco.Valor.ShouldBe(299m);
        produto.Estoque.Valor.ShouldBe(8);
        _unitOfWork.Chamadas.ShouldBe(1);
    }

    [Fact]
    public async Task Atualizar_ProdutoInexistente_DeveRetornarNaoEncontrado()
    {
        var handler = new AtualizarProdutoCommandHandler(_produtos, _unitOfWork, new AtualizarProdutoCommandValidator());

        var resultado = await handler.Handle(
            new AtualizarProdutoCommand(Guid.CreateVersion7(), "Webcam", null, 10m, 1), TestContext.Current.CancellationToken);

        resultado.Erro!.Tipo.ShouldBe(TipoErro.NaoEncontrado);
        _unitOfWork.Chamadas.ShouldBe(0);
    }
}
