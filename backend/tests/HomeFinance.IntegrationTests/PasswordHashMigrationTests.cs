using System.Security.Cryptography;
using System.Text;
using HomeFinance.Application.Services;
using HomeFinance.Domain.Entities;
using HomeFinance.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;

namespace HomeFinance.IntegrationTests;

public sealed class PasswordHashMigrationTests
{
    [Fact]
    public async Task Login_ReplacesValidLegacySha256HashWithAdaptiveHash()
    {
        const string password = "LegacyPassword123!";
        var legacyHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password)));
        var user = new User
        {
            Email = "legacy@example.test",
            NormalizedEmail = "legacy@example.test",
            PasswordHash = legacyHash
        };
        var repository = new InMemoryUserRepository();
        await repository.AddAsync(user);
        var hasher = new PasswordHasher<User>();
        var service = new AuthService(repository, hasher);

        var result = await service.LoginAsync(user.Email, password);

        Assert.True(result.Success);
        Assert.NotEqual(legacyHash, user.PasswordHash);
        Assert.Equal(PasswordVerificationResult.Success,
            hasher.VerifyHashedPassword(user, user.PasswordHash, password));
    }

    [Fact]
    public async Task Login_DoesNotReplaceLegacyHashWhenPasswordIsInvalid()
    {
        const string password = "LegacyPassword123!";
        var legacyHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password)));
        var user = new User
        {
            Email = "legacy@example.test",
            NormalizedEmail = "legacy@example.test",
            PasswordHash = legacyHash
        };
        var repository = new InMemoryUserRepository();
        await repository.AddAsync(user);
        var service = new AuthService(repository, new PasswordHasher<User>());

        var result = await service.LoginAsync(user.Email, "WrongPassword123!");

        Assert.False(result.Success);
        Assert.Equal(legacyHash, user.PasswordHash);
    }
}
