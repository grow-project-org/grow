using Grow.WebApi.Endpoints;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;

namespace Grow.Tests.Integration;

[TestFixture]
public class PlantsControllerTest
{
    private HttpClient _client;
    private WebApplicationFactory<Program> _factory;

    [SetUp]
    public void SetUp()
    {
        this._factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                });
            });

        this._client = this._factory.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        this._client.Dispose();
        this._factory.Dispose();
    }

    [Test]
    public async Task Post_Plants_ShouldReturnPlantId()
    {
        var newPlantCommand = new CreatePlantRequest("monstera-01", Guid.NewGuid());

        var response = await this._client.PostAsJsonAsync("/api/plants", newPlantCommand);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var result = await response.Content.ReadFromJsonAsync<CreatePlantResponse>();

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.CreatedPlantId, Is.Not.EqualTo(Guid.Empty));
    }
}
