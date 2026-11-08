using HomeFinance.Api;
using HomeFinance.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace HomeFinance.IntegrationTests;

[CollectionDefinition("PostgreSQL integration tests")]
public sealed class PostgresTestCollection : ICollectionFixture<PostgresFixture>
{
    public const string Name = "PostgreSQL integration tests";
}

public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("homefinance_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public string ConnectionString => _postgres.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        await using var factory = CreateFactory();
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PersistenceDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    public Task DisposeAsync() => _postgres.DisposeAsync().AsTask();

    public WebApplicationFactory<Program> CreateFactory(string? connectionString = null) =>
        new TestApiFactory(connectionString ?? ConnectionString);

    private sealed class TestApiFactory(string connectionString) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:HomeFinance"] = connectionString,
                    ["Authentication:SigningKey"] = TestSigningKey,
                    ["Authentication:Issuer"] = "HomeFinance.Tests",
                    ["Authentication:Audience"] = "HomeFinance.Tests",
                    ["Authentication:TokenLifetimeMinutes"] = "10"
                });
            });
        }
    }

    public const string TestSigningKey = "integration-test-key-that-is-at-least-32-bytes";
}
