using FluentValidation;
using Wtw.Application.Abstractions.Messaging;
using Wtw.Application.Comum;
using Wtw.Domain;
using Wtw.Domain.Clientes;
using Wtw.Domain.Pedidos;
using Wtw.Domain.Produtos;
using Wtw.Domain.Results;

namespace Wtw.Application.Pedidos.CriarPedido;

internal sealed class CriarPedidoCommandHandler(
    IPedidoRepository pedidos,
    IProdutoRepository produtos,
    IClienteRepository clientes,
    IUnitOfWork unitOfWork,
    IValidator<CriarPedidoCommand> validator) : ICommandHandler<CriarPedidoCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CriarPedidoCommand command, CancellationToken cancellationToken)
    {
        var validacao = await validator.ValidarAsync(command, cancellationToken);

        if (validacao.IsFailure)
        {
            return Result<Guid>.Fail(validacao.Erro!);
        }

        var cliente = await clientes.ObterPorIdAsync(command.ClienteId, cancellationToken);

        if (cliente is null)
        {
            return Result<Guid>.Fail(ApplicationErros.NaoEncontrado(nameof(Cliente), command.ClienteId));
        }

        var itens = await ObterItensAsync(command.Itens, cancellationToken);

        if (itens.IsFailure)
        {
            return Result<Guid>.Fail(itens.Erro!);
        }

        var pedido = Pedido.Criar(cliente.Id, itens.Data!);

        if (pedido.IsFailure)
        {
            return Result<Guid>.Fail(pedido.Erro!);
        }

        pedidos.Adicionar(pedido.Data!);

        var persistencia = await unitOfWork.SaveChangesAsync(cancellationToken);

        return persistencia.IsFailure
            ? Result<Guid>.Fail(persistencia.Erro!)
            : Result<Guid>.Ok(pedido.Data!.Id);
    }

    private async Task<Result<IReadOnlyCollection<(Produto Produto, int Quantidade)>>> ObterItensAsync(
        IReadOnlyList<ItemPedidoCommand> itens, CancellationToken cancellationToken)
    {
        var encontrados = await produtos.ObterPorIdsAsync(itens.Select(item => item.ProdutoId), cancellationToken);
        var encontradosPorId = encontrados.ToDictionary(produto => produto.Id);

        var inexistente = itens.FirstOrDefault(item => !encontradosPorId.ContainsKey(item.ProdutoId));

        if (inexistente is not null)
        {
            return Result<IReadOnlyCollection<(Produto, int)>>.Fail(
                ApplicationErros.NaoEncontrado(nameof(Produto), inexistente.ProdutoId));
        }

        return Result<IReadOnlyCollection<(Produto, int)>>.Ok(
            itens.Select(item => (encontradosPorId[item.ProdutoId], item.Quantidade)).ToList());
    }
}
