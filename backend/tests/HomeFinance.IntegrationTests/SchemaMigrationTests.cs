using HomeFinance.Domain.Entities;
using HomeFinance.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace HomeFinance.IntegrationTests;

[Collection(PostgresTestCollection.Name)]
public sealed class SchemaMigrationTests(PostgresFixture fixture)
{
    [Fact]
    public async Task MigrationCanBeAppliedToEmptyDatabaseAndPreservesDataOnReapplication()
    {
        var databaseName = $"homefinance_migration_{Guid.NewGuid():N}";
        var adminConnectionString = new NpgsqlConnectionStringBuilder(fixture.ConnectionString)
        {
            Database = "postgres",
            Pooling = false
        }.ConnectionString;

        await using (var adminConnection = new NpgsqlConnection(adminConnectionString))
        {
            await adminConnection.OpenAsync();
            await using var createDatabase = new NpgsqlCommand(
                $"CREATE DATABASE \"{databaseName}\"",
                adminConnection);
            await createDatabase.ExecuteNonQueryAsync();
        }

        var email = $"{Guid.NewGuid():N}@example.test";
        var testConnectionString = new NpgsqlConnectionStringBuilder(fixture.ConnectionString)
        {
            Database = databaseName,
            Pooling = false
        }.ConnectionString;

        try
        {
            await using var dbContext = new PersistenceDbContext(
                new DbContextOptionsBuilder<PersistenceDbContext>().UseNpgsql(testConnectionString).Options);
            Assert.Empty(await dbContext.Database.GetAppliedMigrationsAsync());
            await dbContext.Database.MigrateAsync();
            dbContext.Users.Add(new User
            {
                Email = email,
                NormalizedEmail = email,
                PasswordHash = "test-hash"
            });
            await dbContext.SaveChangesAsync();
            await dbContext.Database.MigrateAsync();
            Assert.Contains(
                await dbContext.Database.GetAppliedMigrationsAsync(),
                migration => migration.EndsWith("_InitialPersistence", StringComparison.Ordinal));

            Assert.Equal(1, await dbContext.Users.CountAsync(user => user.NormalizedEmail == email));

            await using var duplicateContext = new PersistenceDbContext(
                new DbContextOptionsBuilder<PersistenceDbContext>().UseNpgsql(testConnectionString).Options);
            duplicateContext.Users.Add(new User
            {
                Email = email.ToUpperInvariant(),
                NormalizedEmail = email,
                PasswordHash = "another-hash"
            });
            await Assert.ThrowsAsync<DbUpdateException>(() => duplicateContext.SaveChangesAsync());
        }
        finally
        {
            NpgsqlConnection.ClearAllPools();
            await using var adminConnection = new NpgsqlConnection(adminConnectionString);
            await adminConnection.OpenAsync();
            await using var dropDatabase = new NpgsqlCommand(
                $"DROP DATABASE \"{databaseName}\"",
                adminConnection);
            await dropDatabase.ExecuteNonQueryAsync();
        }
    }

    [Fact]
    public async Task HouseholdForeignKeyRejectsMissingOwner()
    {
        await using var dbContext = new PersistenceDbContext(
            new DbContextOptionsBuilder<PersistenceDbContext>().UseNpgsql(fixture.ConnectionString).Options);
        dbContext.Households.Add(new Household
        {
            Name = "Hogar sin titular",
            OwnerId = Guid.NewGuid()
        });

        await Assert.ThrowsAsync<DbUpdateException>(() => dbContext.SaveChangesAsync());
    }

    [Fact]
    public async Task ConcurrentDuplicateNormalizedEmailsAreRejectedByDatabase()
    {
        var normalizedEmail = $"{Guid.NewGuid():N}@example.test";
        var results = await Task.WhenAll(Enumerable.Range(0, 2).Select(async _ =>
        {
            await using var dbContext = new PersistenceDbContext(
                new DbContextOptionsBuilder<PersistenceDbContext>().UseNpgsql(fixture.ConnectionString).Options);
            dbContext.Users.Add(new User
            {
                Email = normalizedEmail,
                NormalizedEmail = normalizedEmail,
                PasswordHash = "test-hash"
            });

            try
            {
                await dbContext.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException)
            {
                return false;
            }
        }));

        Assert.Single(results, succeeded => succeeded);
        Assert.Single(results, succeeded => !succeeded);
    }
}
