using System.Net;
using System.Text.Json;

namespace Grow.Tests.Integration;

[TestFixture]
public class HealthCheckTests : IntegrationTestBase
{
    [Test]
    public async Task Get_Health_ShouldReturnHealthyStatus()
    {
        var response = await this.client.GetAsync("/hc");
        var body = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(body);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(document.RootElement.GetProperty("status").GetString(), Is.EqualTo("Healthy"));
        }
    }
}
