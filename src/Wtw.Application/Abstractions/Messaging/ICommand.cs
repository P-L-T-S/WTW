using MediatR;
using Wtw.Domain.Results;

namespace Wtw.Application.Abstractions.Messaging;

public interface ICommand : IRequest<Result>;

public interface ICommand<TResponse> : IRequest<Result<TResponse>>;
