using FluentValidation;
using Wtw.Application.Produtos.Comum;

namespace Wtw.Application.Produtos.AtualizarProduto;

internal sealed class AtualizarProdutoCommandValidator : AbstractValidator<AtualizarProdutoCommand>
{
    public AtualizarProdutoCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty()
            .WithMessage("Informe o produto.");

        Include(new DadosProdutoValidator());
    }
}
