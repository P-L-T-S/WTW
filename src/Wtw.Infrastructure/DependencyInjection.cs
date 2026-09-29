using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wtw.Domain;
using Wtw.Domain.Clientes;
using Wtw.Domain.Pedidos;
using Wtw.Domain.Produtos;
using Wtw.Infrastructure.Persistence;
using Wtw.Infrastructure.Persistence.Repositories;

namespace Wtw.Infrastructure;

public static class DependencyInjection
{
    public const string NomeConnectionString = "Wtw";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(NomeConnectionString)
                               ?? throw new InvalidOperationException(
                                   $"Connection string '{NomeConnectionString}' não configurada.");

        services.AddDbContext<WtwDbContext>(options => options.UseSqlServer(connectionString));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IProdutoRepository, ProdutoRepository>();
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IPedidoRepository, PedidoRepository>();

        return services;
    }

    public static async Task AplicarMigrationsAsync(this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        await using var escopo = serviceProvider.CreateAsyncScope();
        var context = escopo.ServiceProvider.GetRequiredService<WtwDbContext>();

        await context.Database.MigrateAsync(cancellationToken);
    }
}
