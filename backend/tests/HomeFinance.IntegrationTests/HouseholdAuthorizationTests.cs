using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace HomeFinance.IntegrationTests;

[Collection(PostgresTestCollection.Name)]
public sealed class HouseholdAuthorizationTests(PostgresFixture fixture)
{
    [Fact]
    public async Task SpoofedUserIdHeaderDoesNotGrantAccess()
    {
        var email = $"{Guid.NewGuid():N}@example.test";
        using var host = fixture.CreateFactory();
        var client = host.CreateClient();
        var registration = await client.PostAsJsonAsync("/api/auth/register", new { email, password = "Password123!" });
        registration.EnsureSuccessStatusCode();
        var login = await client.PostAsJsonAsync("/api/auth/login", new { email, password = "Password123!" });
        login.EnsureSuccessStatusCode();
        using var response = JsonDocument.Parse(await login.Content.ReadAsStringAsync());
        var accessToken = response.RootElement.GetProperty("accessToken").GetString();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        client.DefaultRequestHeaders.Add("X-User-Id", Guid.NewGuid().ToString());

        var create = await client.PostAsJsonAsync(
            "/api/households",
            new { name = "Hogar propio", userId = Guid.NewGuid().ToString() });
        Assert.Equal(HttpStatusCode.OK, create.StatusCode);

        using var created = JsonDocument.Parse(await create.Content.ReadAsStringAsync());
        var householdId = created.RootElement.GetProperty("household").GetProperty("id").GetGuid();
        var spoofedRead = await client.GetAsync($"/api/households/{householdId}/settings");
        Assert.Equal(HttpStatusCode.OK, spoofedRead.StatusCode);
    }
}
