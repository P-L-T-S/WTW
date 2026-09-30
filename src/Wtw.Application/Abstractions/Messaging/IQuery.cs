using MediatR;
using Wtw.Domain.Results;

namespace Wtw.Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
