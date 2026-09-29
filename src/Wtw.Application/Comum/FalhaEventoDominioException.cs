using Wtw.Domain.Results;

namespace Wtw.Application.Comum;

/// <summary>
/// Lançada por um handler de evento de domínio para abortar a operação em andamento.
/// Handlers do MediatR retornam apenas <see cref="Task"/>, então esta é a forma de propagar o <see cref="Erro"/>
/// até o UnitOfWork, que o converte de volta em <see cref="Result"/>.
/// </summary>
public sealed class FalhaEventoDominioException(Erro erro) : Exception(erro.Mensagem)
{
    public Erro Erro { get; } = erro;
}
