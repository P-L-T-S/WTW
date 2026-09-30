using Wtw.Domain;
using Wtw.Domain.Results;

namespace Wtw.Application.Tests.Fakes;

internal sealed class UnitOfWorkFalso(Result? resultado = null) : IUnitOfWork
{
    public int Chamadas { get; private set; }

    public Task<Result> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        Chamadas++;
        return Task.FromResult(resultado ?? Result.Ok());
    }
}
