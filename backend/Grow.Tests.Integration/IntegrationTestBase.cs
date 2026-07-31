using Grow.WebApi.Endpoints;
using System.Net.Http.Json;

namespace Grow.Tests.Integration;

public abstract class IntegrationTestBase
{
    protected HttpClient client = null!;
    protected GrowApiFactory factory = null!;

    [SetUp]
    public void BaseSetUp()
    {
        this.factory = new GrowApiFactory();
        this.client = this.factory.CreateClient();
    }

    [TearDown]
    public void BaseTearDown()
    {
        this.client.Dispose();
        this.factory.Dispose();
    }

    protected async Task<Guid> CreatePlantAsync(string? customId = null, Guid? specieId = null)
    {
        var request = new CreatePlantRequest(customId ?? $"plant-{Guid.NewGuid()}", specieId ?? Guid.NewGuid());
        var response = await this.client.PostAsJsonAsync("/api/plants", request);
        _ = response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<CreatePlantResponse>();
        return result!.CreatedPlantId;
    }
}
