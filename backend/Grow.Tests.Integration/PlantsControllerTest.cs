using Grow.Domain;
using Grow.Infrastructure.Database;
using Grow.WebApi.Endpoints;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(IDbContextFactory<DatabaseContext>));

                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    var contextDescriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DatabaseContext));

                    if (contextDescriptor != null)
                    {
                        services.Remove(contextDescriptor);
                    }

                    services.AddDbContextFactory<DatabaseContext>(options =>
                    {
                        options.UseInMemoryDatabase("GrowTestDb");
                    });

                    services.AddScoped<IDatabaseContext>(sp =>
                        sp.GetRequiredService<DatabaseContext>());
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
        var newPlantCommand = new CreatePlantRequest($"monstera-{Guid.NewGuid()}", Guid.NewGuid());

        var response = await this._client.PostAsJsonAsync("/api/plants", newPlantCommand);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var result = await response.Content.ReadFromJsonAsync<CreatePlantResponse>();

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.CreatedPlantId, Is.Not.EqualTo(Guid.Empty));
    }
}