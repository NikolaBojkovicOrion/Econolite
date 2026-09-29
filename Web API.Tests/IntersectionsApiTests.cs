using System.Net;
using System.Net.Http.Json;
using Econolite_API.Modules.Intersections.Application.Contracts;

namespace Econolite_API.Tests;

public sealed class IntersectionsApiTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient client;

    public IntersectionsApiTests(ApiFactory factory)
    {
        client = factory.CreateClient();
    }

    [Fact]
    public async Task Get_intersections_returns_seeded_data_over_http()
    {
        var response = await client.GetAsync("/api/intersections");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var intersections = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<IntersectionResponse>>();

        Assert.NotNull(intersections);
        Assert.NotEmpty(intersections);
    }

    [Fact]
    public async Task Get_unknown_intersection_returns_not_found_over_http()
    {
        var response = await client.GetAsync("/api/intersections/9999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
