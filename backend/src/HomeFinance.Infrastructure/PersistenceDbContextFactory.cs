using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HomeFinance.Infrastructure;

public sealed class PersistenceDbContextFactory : IDesignTimeDbContextFactory<PersistenceDbContext>
{
    public PersistenceDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("HOMEFINANCE_CONNECTION_STRING");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Defina HOMEFINANCE_CONNECTION_STRING para ejecutar herramientas de EF Core.");
        }

        var options = new DbContextOptionsBuilder<PersistenceDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new PersistenceDbContext(options);
    }
}
