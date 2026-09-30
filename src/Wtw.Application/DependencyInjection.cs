using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Wtw.Application.Abstractions.Behaviors;

namespace Wtw.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(configuracao =>
        {
            configuracao.RegisterServicesFromAssembly(assembly);
            configuracao.AddOpenBehavior(typeof(LoggingBehavior<,>));
        });
        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

        return services;
    }
}
