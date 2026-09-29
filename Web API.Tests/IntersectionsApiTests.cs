using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Econolite_API.Modules.Identity.Application.Contracts;
using Econolite_API.Modules.Intersections.Application.Contracts;

namespace Econolite_API.Tests;

public sealed class IntersectionsApiTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory factory;

    public IntersectionsApiTests(ApiFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task Get_intersections_returns_seeded_data_over_http()
    {
        var client = await CreateAuthenticatedClient();
        var response = await client.GetAsync("/api/intersections");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var intersections = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<IntersectionResponse>>();

        Assert.NotNull(intersections);
        Assert.NotEmpty(intersections);
    }

    [Fact]
    public async Task Get_unknown_intersection_returns_not_found_over_http()
    {
        var client = await CreateAuthenticatedClient();
        var response = await client.GetAsync("/api/intersections/9999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<HttpClient> CreateAuthenticatedClient()
    {
        var client = factory.CreateClient();
        var loginResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest("operator@econolite.local", "Operator123!"));
        loginResponse.EnsureSuccessStatusCode();
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login!.AccessToken);
        return client;
    }
}
