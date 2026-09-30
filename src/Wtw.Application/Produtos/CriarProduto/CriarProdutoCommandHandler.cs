using FluentValidation;
using Wtw.Application.Abstractions.Messaging;
using Wtw.Application.Comum;
using Wtw.Application.Produtos.Comum;
using Wtw.Domain;
using Wtw.Domain.Produtos;
using Wtw.Domain.Results;

namespace Wtw.Application.Produtos.CriarProduto;

internal sealed class CriarProdutoCommandHandler(
    IProdutoRepository produtos,
    IUnitOfWork unitOfWork,
    IValidator<CriarProdutoCommand> validator) : ICommandHandler<CriarProdutoCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CriarProdutoCommand command, CancellationToken cancellationToken)
    {
        var validacao = await validator.ValidarAsync(command, cancellationToken);

        if (validacao.IsFailure)
        {
            return Result<Guid>.Fail(validacao.Erro!);
        }

        var valores = ValoresProduto.Criar(command);

        if (valores.IsFailure)
        {
            return Result<Guid>.Fail(valores.Erro!);
        }

        var (nome, descricao, preco, estoque) = valores.Data!;
        var produto = Produto.Criar(nome, descricao, preco, estoque);

        produtos.Adicionar(produto);

        var persistencia = await unitOfWork.SaveChangesAsync(cancellationToken);

        return persistencia.IsFailure
            ? Result<Guid>.Fail(persistencia.Erro!)
            : Result<Guid>.Ok(produto.Id);
    }
}
