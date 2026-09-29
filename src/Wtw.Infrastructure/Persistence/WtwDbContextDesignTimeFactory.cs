using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Wtw.Infrastructure.Persistence;

/// <summary>
/// Usada apenas pelas ferramentas <c>dotnet ef</c>, para gerar migrations e scripts sem depender da API.
/// A connection string pode ser sobrescrita pela variável de ambiente <c>WTW_CONNECTION_STRING</c>.
/// </summary>
internal sealed class WtwDbContextDesignTimeFactory : IDesignTimeDbContextFactory<WtwDbContext>
{
    private const string ConnectionStringPadrao =
        "Server=localhost,1433;Database=WtwPedidos;User Id=sa;Password=Wtw@Dev2026!;TrustServerCertificate=True";

    public WtwDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("WTW_CONNECTION_STRING") ?? ConnectionStringPadrao;

        var options = new DbContextOptionsBuilder<WtwDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new WtwDbContext(options);
    }
}
