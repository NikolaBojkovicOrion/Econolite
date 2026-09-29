using System.Net;
using System.Net.Http.Json;
using Econolite_API.Modules.Identity.Application.Contracts;

namespace Econolite_API.Tests;

public sealed class AuthenticationApiTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient client;

    public AuthenticationApiTests(ApiFactory factory)
    {
        client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_returns_operator_jwt_for_valid_credentials()
    {
        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest("operator@econolite.local", "Operator123!"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var login = await response.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(login);
        Assert.False(string.IsNullOrWhiteSpace(login.AccessToken));
        Assert.Contains("Operator", login.User.Roles);
    }

    [Fact]
    public async Task Login_rejects_invalid_credentials()
    {
        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest("operator@econolite.local", "wrong-password"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Traffic_endpoint_requires_authentication()
    {
        var response = await client.GetAsync("/api/intersections");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}