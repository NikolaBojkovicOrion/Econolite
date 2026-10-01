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
    public async Task Get_intersections_returns_seeded_page_over_http()
    {
        var client = await CreateAuthenticatedClient();
        var response = await client.GetAsync("/api/intersections");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await response.Content.ReadFromJsonAsync<IntersectionPageResponse>();

        Assert.NotNull(page);
        Assert.NotEmpty(page.Items);
        Assert.Equal(1, page.PageNumber);
        Assert.Equal(10, page.PageSize);
        Assert.True(page.TotalCount >= page.Items.Count);
        Assert.Equal(8, page.Summary.TotalCount);
    }

    [Fact]
    public async Task Get_intersections_rejects_invalid_page_number()
    {
        var client = await CreateAuthenticatedClient();
        var response = await client.GetAsync("/api/intersections?pageNumber=0");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
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
