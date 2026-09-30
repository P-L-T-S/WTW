using FluentValidation;
using Wtw.Application.Abstractions.Messaging;
using Wtw.Application.Comum;
using Wtw.Application.Produtos.Comum;
using Wtw.Domain;
using Wtw.Domain.Produtos;
using Wtw.Domain.Results;

namespace Wtw.Application.Produtos.AtualizarProduto;

internal sealed class AtualizarProdutoCommandHandler(
    IProdutoRepository produtos,
    IUnitOfWork unitOfWork,
    IValidator<AtualizarProdutoCommand> validator) : ICommandHandler<AtualizarProdutoCommand>
{
    public async Task<Result> Handle(AtualizarProdutoCommand command, CancellationToken cancellationToken)
    {
        var validacao = await validator.ValidarAsync(command, cancellationToken);

        if (validacao.IsFailure)
        {
            return validacao;
        }

        var valores = ValoresProduto.Criar(command);

        if (valores.IsFailure)
        {
            return Result.Fail(valores.Erro!);
        }

        var produto = await produtos.ObterPorIdAsync(command.Id, cancellationToken);

        if (produto is null)
        {
            return Result.Fail(ApplicationErros.NaoEncontrado(nameof(Produto), command.Id));
        }

        var (nome, descricao, preco, estoque) = valores.Data!;
        produto.Atualizar(nome, descricao, preco, estoque);

        return await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
