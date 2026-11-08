using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HomeFinance.Api;
using HomeFinance.Domain.Entities;
using HomeFinance.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HomeFinance.IntegrationTests;

[Collection(PostgresTestCollection.Name)]
public sealed class AuthPersistenceTests(PostgresFixture fixture)
{
    [Fact]
    public async Task Register_DuplicateNormalizedEmailAndLogin_ArePersistentAcrossHostRestart()
    {
        var email = $"{Guid.NewGuid():N}@example.test";
        const string password = "Password123!";
        using (var firstHost = fixture.CreateFactory())
        {
            var client = firstHost.CreateClient();
            var registration = await client.PostAsJsonAsync("/api/auth/register", new { email, password });
            Assert.Equal(HttpStatusCode.OK, registration.StatusCode);
            var registrationBody = await registration.Content.ReadAsStringAsync();
            Assert.DoesNotContain(password, registrationBody, StringComparison.Ordinal);

            var duplicate = await client.PostAsJsonAsync(
                "/api/auth/register",
                new { email = $" {email.ToUpperInvariant()} ", password = "Different123!" });
            Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);

            using var scope = firstHost.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<PersistenceDbContext>();
            var storedUser = await dbContext.Users.SingleAsync(user => user.NormalizedEmail == email);
            Assert.DoesNotContain(password, storedUser.PasswordHash, StringComparison.Ordinal);
        }

        using var restartedHost = fixture.CreateFactory();
        var restartedClient = restartedHost.CreateClient();
        var login = await restartedClient.PostAsJsonAsync(
            "/api/auth/login",
            new { email = email.ToUpperInvariant(), password });

        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        using var loginBody = JsonDocument.Parse(await login.Content.ReadAsStringAsync());
        Assert.False(string.IsNullOrWhiteSpace(loginBody.RootElement.GetProperty("accessToken").GetString()));
    }
}
