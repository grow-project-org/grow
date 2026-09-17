using Grow.WebApi.Endpoints;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;

namespace Grow.Tests.Integration.Endpoints;

[TestFixture]
public class UsersTests : IntegrationTestBase
{
    private HttpClient CreateHttpsClient()
        => this.factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

    private async Task RegisterUserAsync(HttpClient httpClient, string email, string username = "test_username")
    {
        var request = new CreateUserRequest(email, username);
        var response = await httpClient.PostAsJsonAsync("/api/users/register", request);
        _ = response.EnsureSuccessStatusCode();
    }

    [Test]
    public async Task Register_WithValidData_ShouldReturnOk()
    {
        var request = new CreateUserRequest($"user-{Guid.NewGuid()}@example.com", "test_username");

        var response = await this.client.PostAsJsonAsync("/api/users/register", request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Register_WithValidData_ShouldPersistUserWithGivenEmailAndUsername()
    {
        var email = $"user-{Guid.NewGuid()}@example.com";
        var request = new CreateUserRequest(email, "test_username");

        _ = await this.client.PostAsJsonAsync("/api/users/register", request);

        await using var db = await this.factory.CreateDbContextAsync();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);

        Assert.That(user, Is.Not.Null);
        Assert.That(user!.Username, Is.EqualTo("test_username"));
    }

    [Test]
    public void Register_WithInvalidEmail_ShouldThrowArgumentException()
    {
        var request = new CreateUserRequest("invalid-email", "test_username");

        _ = Assert.CatchAsync<ArgumentException>(() => this.client.PostAsJsonAsync("/api/users/register", request));
    }

    [Test]
    public async Task Register_CalledTwiceForSameEmailWithinThrottleWindow_ShouldReturnTooManyRequestsOnSecondCall()
    {
        var request = new CreateUserRequest($"user-{Guid.NewGuid()}@example.com", "test_username");
        _ = await this.client.PostAsJsonAsync("/api/users/register", request);

        var response = await this.client.PostAsJsonAsync("/api/users/register", request);

        Assert.That(response.StatusCode, Is.EqualTo((HttpStatusCode)429));
    }

    [Test]
    public async Task Register_CalledForDifferentEmails_ShouldReturnOkForBoth()
    {
        var firstRequest = new CreateUserRequest($"user-{Guid.NewGuid()}@example.com", "test_username");
        var secondRequest = new CreateUserRequest($"user-{Guid.NewGuid()}@example.com", "test_username");

        var firstResponse = await this.client.PostAsJsonAsync("/api/users/register", firstRequest);
        var secondResponse = await this.client.PostAsJsonAsync("/api/users/register", secondRequest);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(firstResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(secondResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }
    }

    [Test]
    public async Task GetCsrf_ShouldReturnOkWithNonEmptyToken()
    {
        using var httpsClient = this.CreateHttpsClient();

        var response = await httpsClient.GetAsync("/api/users/csrf");
        var token = await response.Content.ReadAsStringAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(token, Is.Not.Empty);
        }
    }

    [Test]
    public async Task Login_WithRegisteredEmail_ShouldReturnOkAndSetAuthCookie()
    {
        using var httpsClient = this.CreateHttpsClient();
        var email = $"user-{Guid.NewGuid()}@example.com";
        await this.RegisterUserAsync(httpsClient, email);

        var response = await httpsClient.PostAsJsonAsync("/api/users/login", new LoginRequest { Email = email, Password = "irrelevant" });

        using (Assert.EnterMultipleScope())
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Headers.TryGetValues("Set-Cookie", out var cookies) && cookies.Any(c => c.StartsWith("__Host-Auth", StringComparison.Ordinal)), Is.True);
        }
    }

    [Test]
    public void Login_WithUnregisteredEmail_ShouldThrow()
    {
        using var httpsClient = this.CreateHttpsClient();
        var request = new LoginRequest { Email = $"missing-{Guid.NewGuid()}@example.com", Password = "irrelevant" };

        _ = Assert.CatchAsync(() => httpsClient.PostAsJsonAsync("/api/users/login", request));
    }

    [Test]
    public async Task Login_CalledTwiceForSameEmailWithinThrottleWindow_ShouldReturnTooManyRequestsOnSecondCall()
    {
        using var httpsClient = this.CreateHttpsClient();
        var email = $"user-{Guid.NewGuid()}@example.com";
        await this.RegisterUserAsync(httpsClient, email);
        var request = new LoginRequest { Email = email, Password = "irrelevant" };
        _ = await httpsClient.PostAsJsonAsync("/api/users/login", request);

        var response = await httpsClient.PostAsJsonAsync("/api/users/login", request);

        Assert.That(response.StatusCode, Is.EqualTo((HttpStatusCode)429));
    }

    [Test]
    public async Task GetMe_WhenLoggedIn_ShouldReturnUsername()
    {
        using var httpsClient = this.CreateHttpsClient();
        var email = $"user-{Guid.NewGuid()}@example.com";
        await this.RegisterUserAsync(httpsClient, email, "logged_in_user");
        _ = await httpsClient.PostAsJsonAsync("/api/users/login", new LoginRequest { Email = email, Password = "irrelevant" });

        var response = await httpsClient.PostAsync("/api/users/me", null);
        var result = await response.Content.ReadFromJsonAsync<MeResponse>();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result, Is.Not.Null);
        }

        Assert.That(result!.Username, Is.EqualTo("logged_in_user"));
    }

    [Test]
    public void GetMe_WhenNotLoggedIn_ShouldThrow()
    {
        using var httpsClient = this.CreateHttpsClient();

        _ = Assert.CatchAsync(() => httpsClient.PostAsync("/api/users/me", null));
    }
}
