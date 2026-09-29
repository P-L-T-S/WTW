namespace Wtw.Domain;

public class DomainEvent : IDomainEvent
{
    public DomainEvent()
    {
        Id = Guid.NewGuid();
        ProcessadoUtc = DateTime.UtcNow;
    }

    public DomainEvent(Guid id, DateTime processadoUtc)
    {
        Id = id;
        ProcessadoUtc = processadoUtc;
    }

    public Guid Id { get; }
    public DateTime ProcessadoUtc { get; }
}