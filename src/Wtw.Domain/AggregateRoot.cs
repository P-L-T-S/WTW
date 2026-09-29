namespace Wtw.Domain;

public abstract class AggregateRoot : Entity
{
    private readonly List<IDomainEvent> _eventos = [];
    public IReadOnlyCollection<IDomainEvent> Eventos => _eventos.AsReadOnly();

    public void LimparEventos() => _eventos.Clear();
    protected void Disparar(IDomainEvent evento) => _eventos.Add(evento);
}
