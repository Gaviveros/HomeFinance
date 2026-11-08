using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using HomeFinance.Api;
using Microsoft.Extensions.DependencyInjection;

namespace HomeFinance.IntegrationTests;

[Collection(PostgresTestCollection.Name)]
public sealed class HouseholdPersistenceTests(PostgresFixture fixture)
{
    [Fact]
    public async Task HouseholdSettingsPersistAndRemainIsolatedToAuthenticatedOwner()
    {
        var ownerEmail = $"{Guid.NewGuid():N}@example.test";
        var otherEmail = $"{Guid.NewGuid():N}@example.test";
        Guid householdId;
        using (var firstHost = fixture.CreateFactory())
        {
            var client = firstHost.CreateClient();
            var ownerToken = await RegisterAndLoginAsync(client, ownerEmail);
            var otherToken = await RegisterAndLoginAsync(client, otherEmail);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ownerToken);
            var creation = await client.PostAsJsonAsync("/api/households", new { name = "Hogar persistente" });
            Assert.Equal(HttpStatusCode.OK, creation.StatusCode);
            using var created = JsonDocument.Parse(await creation.Content.ReadAsStringAsync());
            householdId = created.RootElement.GetProperty("household").GetProperty("id").GetGuid();
            Assert.Equal("ARS", created.RootElement.GetProperty("household").GetProperty("currency").GetString());
            Assert.Equal("America/Argentina/Cordoba",
                created.RootElement.GetProperty("household").GetProperty("timeZone").GetString());

            var update = await client.PutAsJsonAsync($"/api/households/{householdId}/settings", new
            {
                name = "Hogar actualizado",
                currency = "USD",
                timeZone = "America/New_York"
            });
            Assert.Equal(HttpStatusCode.OK, update.StatusCode);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", otherToken);
            var forbiddenRead = await client.GetAsync($"/api/households/{householdId}/settings");
            Assert.Equal(HttpStatusCode.NotFound, forbiddenRead.StatusCode);
            var forbiddenUpdate = await client.PutAsJsonAsync(
                $"/api/households/{householdId}/settings",
                new { name = "Intento ajeno" });
            Assert.Equal(HttpStatusCode.NotFound, forbiddenUpdate.StatusCode);
            var otherHouseholds = await client.GetFromJsonAsync<JsonElement[]>("/api/households");
            Assert.Empty(otherHouseholds!);
        }

        using var restartedHost = fixture.CreateFactory();
        var restartedClient = restartedHost.CreateClient();
        var ownerAccessToken = await RegisterAndLoginAsync(restartedClient, ownerEmail, register: false);
        restartedClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", ownerAccessToken);
        var persistedSettings = await restartedClient.GetFromJsonAsync<JsonElement>(
            $"/api/households/{householdId}/settings");
        Assert.Equal("Hogar actualizado", persistedSettings.GetProperty("name").GetString());
        Assert.Equal("USD", persistedSettings.GetProperty("currency").GetString());
    }

    [Fact]
    public async Task HouseholdEndpointsRequireAuthentication()
    {
        using var host = fixture.CreateFactory();
        var client = host.CreateClient();
        var response = await client.GetAsync("/api/households");
        var createResponse = await client.PostAsJsonAsync("/api/households", new { name = "No autorizado" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, createResponse.StatusCode);
    }

    private static async Task<string> RegisterAndLoginAsync(
        HttpClient client,
        string email,
        bool register = true)
    {
        if (register)
        {
            var registration = await client.PostAsJsonAsync(
                "/api/auth/register",
                new { email, password = "Password123!" });
            Assert.Equal(HttpStatusCode.OK, registration.StatusCode);
        }

        var login = await client.PostAsJsonAsync(
            "/api/auth/login",
            new { email, password = "Password123!" });
        login.EnsureSuccessStatusCode();
        using var response = JsonDocument.Parse(await login.Content.ReadAsStringAsync());
        return response.RootElement.GetProperty("accessToken").GetString()!;
    }

}
