using MediatR;
using Microsoft.EntityFrameworkCore;
using Wtw.Application.Comum;
using Wtw.Domain;
using Wtw.Domain.Results;

namespace Wtw.Infrastructure.Persistence;

internal sealed class UnitOfWork(WtwDbContext context, IPublisher publisher) : IUnitOfWork
{
    public async Task<Result> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Os handlers apenas alteram entidades rastreadas; um único SaveChanges grava tudo
            // na mesma transação, então uma falha em qualquer handler não deixa nada persistido.
            await PublicarEventosAsync(cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
        catch (FalhaEventoDominioException ex)
        {
            DescartarAlteracoes();
            return Result.Fail(ex.Erro);
        }
        catch (DbUpdateConcurrencyException)
        {
            DescartarAlteracoes();
            return Result.Fail(PersistenciaErros.ConflitoDeConcorrencia);
        }
    }

    private async Task PublicarEventosAsync(CancellationToken cancellationToken)
    {
        // Repete enquanto houver eventos, pois um handler pode alterar outro agregado que dispara novos eventos.
        while (true)
        {
            var agregados = context.ChangeTracker
                .Entries<AggregateRoot>()
                .Select(entrada => entrada.Entity)
                .Where(agregado => agregado.Eventos.Count > 0)
                .ToList();

            if (agregados.Count == 0)
            {
                return;
            }

            var eventos = agregados.SelectMany(agregado => agregado.Eventos).ToList();
            agregados.ForEach(agregado => agregado.LimparEventos());

            foreach (var evento in eventos)
            {
                await publisher.Publish(evento, cancellationToken);
            }
        }
    }

    private void DescartarAlteracoes() => context.ChangeTracker.Clear();
}
