using Wtw.Application.Abstractions.Messaging;
using Wtw.Application.Comum;
using Wtw.Domain;
using Wtw.Domain.Pedidos;
using Wtw.Domain.Results;

namespace Wtw.Application.Pedidos.Comum;

public interface IAlterarStatusPedidoCommand : ICommand
{
    Guid PedidoId { get; }
}

internal abstract class AlterarStatusPedidoCommandHandler<TCommand>(
    IPedidoRepository pedidos,
    IUnitOfWork unitOfWork) : ICommandHandler<TCommand>
    where TCommand : IAlterarStatusPedidoCommand
{
    public async Task<Result> Handle(TCommand command, CancellationToken cancellationToken)
    {
        var pedido = await pedidos.ObterPorIdAsync(command.PedidoId, cancellationToken);

        if (pedido is null)
        {
            return Result.Fail(ApplicationErros.NaoEncontrado(nameof(Pedido), command.PedidoId));
        }

        var transicao = AlterarStatus(pedido);

        if (transicao.IsFailure)
        {
            return transicao;
        }

        return await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    protected abstract Result AlterarStatus(Pedido pedido);
}
