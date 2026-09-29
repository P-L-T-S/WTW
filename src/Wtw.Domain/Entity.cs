namespace Wtw.Domain;

public abstract class Entity
{
    protected Entity()
    {
        Id = Guid.CreateVersion7();
    }

    public Guid Id { get; protected init; }

    public override bool Equals(object? obj) =>
        obj is Entity outra && outra.GetType() == GetType() && outra.Id == Id;

    public override int GetHashCode() => Id.GetHashCode();
}
