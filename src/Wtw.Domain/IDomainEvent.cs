using MediatR;

namespace Wtw.Domain;

public interface IDomainEvent: INotification
{
    Guid Id { get;  }
    DateTime ProcessadoUtc { get;  }
}