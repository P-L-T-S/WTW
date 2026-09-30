using FluentValidation;
using Wtw.Domain.Pedidos;

namespace Wtw.Application.Pedidos.CriarPedido;

internal sealed class CriarPedidoCommandValidator : AbstractValidator<CriarPedidoCommand>
{
    public CriarPedidoCommandValidator()
    {
        RuleFor(command => command.ClienteId)
            .NotEmpty()
            .WithMessage(PedidoErros.ClienteObrigatorio.Mensagem);

        RuleFor(command => command.Itens)
            .NotEmpty()
            .WithMessage(PedidoErros.SemItens.Mensagem);

        RuleForEach(command => command.Itens).ChildRules(item =>
        {
            item.RuleFor(i => i.ProdutoId)
                .NotEmpty()
                .WithMessage("Informe o produto do item.");

            item.RuleFor(i => i.Quantidade)
                .GreaterThan(0)
                .WithMessage(PedidoErros.QuantidadeInvalida.Mensagem);
        });
    }
}
