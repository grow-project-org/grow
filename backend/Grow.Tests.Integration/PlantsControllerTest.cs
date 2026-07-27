using Grow.WebApi.Endpoints;
using System.Net;
using System.Net.Http.Json;

namespace Grow.Tests.Integration;

[TestFixture]
public class PlantsControllerTest : IntegrationTestBase
{
    [Test]
    public async Task Post_Plants_ShouldReturnPlantId()
    {
        var newPlantCommand = new CreatePlantRequest($"monstera-{Guid.NewGuid()}", Guid.NewGuid());

        var response = await this.client.PostAsJsonAsync("/api/plants", newPlantCommand);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var result = await response.Content.ReadFromJsonAsync<CreatePlantResponse>();

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.CreatedPlantId, Is.Not.EqualTo(Guid.Empty));
    }
}
