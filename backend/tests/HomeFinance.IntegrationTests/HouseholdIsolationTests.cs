using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace HomeFinance.IntegrationTests;

[Collection(PostgresTestCollection.Name)]
public sealed class HouseholdIsolationTests(PostgresFixture fixture)
{
    [Fact]
    public async Task ListingReturnsOnlyTheAuthenticatedOwnersHouseholds()
    {
        using var host = fixture.CreateFactory();
        var client = host.CreateClient();
        var ownerToken = await RegisterAndLoginAsync(client, $"{Guid.NewGuid():N}@example.test");
        var otherToken = await RegisterAndLoginAsync(client, $"{Guid.NewGuid():N}@example.test");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ownerToken);
        var create = await client.PostAsJsonAsync("/api/households", new { name = "Privado" });
        create.EnsureSuccessStatusCode();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", otherToken);
        var response = await client.GetFromJsonAsync<JsonElement[]>("/api/households");

        Assert.Empty(response!);
    }

    private static async Task<string> RegisterAndLoginAsync(HttpClient client, string email)
    {
        var registration = await client.PostAsJsonAsync(
            "/api/auth/register",
            new { email, password = "Password123!" });
        registration.EnsureSuccessStatusCode();
        var login = await client.PostAsJsonAsync("/api/auth/login", new { email, password = "Password123!" });
        login.EnsureSuccessStatusCode();
        using var body = JsonDocument.Parse(await login.Content.ReadAsStringAsync());
        return body.RootElement.GetProperty("accessToken").GetString()!;
    }
}
