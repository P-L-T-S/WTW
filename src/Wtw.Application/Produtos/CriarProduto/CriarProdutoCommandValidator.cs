using FluentValidation;
using Wtw.Application.Produtos.Comum;

namespace Wtw.Application.Produtos.CriarProduto;

internal sealed class CriarProdutoCommandValidator : AbstractValidator<CriarProdutoCommand>
{
    public CriarProdutoCommandValidator()
    {
        Include(new DadosProdutoValidator());
    }
}
