using FluentValidation;
using Wtw.Domain.Comum;
using Wtw.Domain.Produtos;

namespace Wtw.Application.Produtos.Comum;

internal sealed class DadosProdutoValidator : AbstractValidator<IDadosProduto>
{
    public DadosProdutoValidator()
    {
        RuleFor(dados => dados.Nome)
            .Must(nome => nome?.Trim().Length is >= Nome.TamanhoMinimo and <= Nome.TamanhoMaximo)
            .WithMessage(ComumErros.NomeInvalido.Mensagem);

        RuleFor(dados => dados.Descricao)
            .MaximumLength(Descricao.TamanhoMaximo)
            .WithMessage(ProdutoErros.DescricaoMuitoLonga.Mensagem);

        RuleFor(dados => dados.Preco)
            .GreaterThan(0)
            .WithMessage(ProdutoErros.PrecoInvalido.Mensagem);

        RuleFor(dados => dados.QuantidadeEstoque)
            .GreaterThanOrEqualTo(0)
            .WithMessage(ProdutoErros.EstoqueNegativo.Mensagem);
    }
}
