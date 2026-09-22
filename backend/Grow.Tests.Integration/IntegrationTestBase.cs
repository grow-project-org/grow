using Grow.Domain.Plants;
using Grow.WebApi.Endpoints;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;

namespace Grow.Tests.Integration;

public abstract class IntegrationTestBase
{
    protected HttpClient client = null!;
    protected GrowApiFactory factory = null!;
    protected Guid currentUserId;

    private readonly List<HttpClient> additionalClients = [];

    [SetUp]
    public async Task BaseSetUp()
    {
        this.factory = new GrowApiFactory();
        this.client = this.CreateHttpsClient();
        this.currentUserId = await this.RegisterAndLoginAsync(this.client, $"user-{Guid.NewGuid()}@example.com");
    }

    [TearDown]
    public void BaseTearDown()
    {
        this.client.Dispose();
        foreach (var additionalClient in this.additionalClients)
        {
            additionalClient.Dispose();
        }

        this.factory.Dispose();
    }

    protected HttpClient CreateHttpsClient()
        => this.factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

    protected async Task<Guid> RegisterAndLoginAsync(HttpClient httpClient, string email, string username = "test_username")
    {
        var registerRequest = new CreateUserRequest(email, username);
        var registerResponse = await httpClient.PostAsJsonAsync("/api/users/register", registerRequest);
        _ = registerResponse.EnsureSuccessStatusCode();

        var loginResponse = await httpClient.PostAsJsonAsync("/api/users/login", new LoginRequest { Email = email, Password = "irrelevant" });
        _ = loginResponse.EnsureSuccessStatusCode();

        await using var db = await this.factory.CreateDbContextAsync();
        var user = await db.Users.FirstAsync(u => u.Email == email);
        return user.Id;
    }

    protected async Task<(HttpClient Client, Guid UserId)> CreateOtherUserClientAsync()
    {
        var otherClient = this.CreateHttpsClient();
        this.additionalClients.Add(otherClient);

        var otherUserId = await this.RegisterAndLoginAsync(otherClient, $"user-{Guid.NewGuid()}@example.com");
        return (otherClient, otherUserId);
    }

    protected async Task<Guid> CreatePlantAsync(string? customId = null, Guid? specieId = null)
    {
        var request = new CreatePlantRequest(customId ?? $"plant-{Guid.NewGuid()}", specieId ?? await this.CreateSpecieAsync());
        var response = await this.client.PostAsJsonAsync("/api/plants", request);
        _ = response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<CreatePlantResponse>();
        return result!.CreatedPlantId;
    }

    protected async Task<Guid> CreatePlantGroupAsync(string? name = null, GroupType type = GroupType.WorkGroup)
    {
        var request = new CreatePlantGroupRequest(name ?? $"group-{Guid.NewGuid()}", type);
        var response = await this.client.PostAsJsonAsync("/api/plant-groups", request);
        _ = response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<CreatePlantGroupResponse>();
        return result!.CreatedPlantGroupId;
    }

    protected async Task<Guid> CreateSpecieAsync(string? name = null)
    {
        var request = new CreateSpecieRequest(name ?? $"specie-{Guid.NewGuid()}");
        var response = await this.client.PostAsJsonAsync("/api/species", request);
        _ = response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<CreateSpecieResponse>();
        return result!.SpecieId;
    }
}
