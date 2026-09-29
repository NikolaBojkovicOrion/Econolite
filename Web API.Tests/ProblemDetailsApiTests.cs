using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Econolite_API.Tests;

public sealed class ProblemDetailsApiTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient client;

    public ProblemDetailsApiTests(ApiFactory factory)
    {
        client = factory.CreateClient();
    }

    [Fact]
    public async Task Invalid_login_returns_problem_details_with_trace_id()
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = JsonContent.Create(new { Email = "", Password = "" }),
        };
        request.Headers.Add("X-Correlation-ID", "test-correlation-123");

        var response = await client.SendAsync(request);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("test-correlation-123", response.Headers.GetValues("X-Correlation-ID").Single());
        Assert.True(body.TryGetProperty("traceId", out _));
        Assert.True(body.TryGetProperty("errors", out _));
    }

    [Fact]
    public async Task Unauthorized_request_returns_correlation_id()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/intersections");
        request.Headers.Add("X-Correlation-ID", "anonymous-request-456");

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("anonymous-request-456", response.Headers.GetValues("X-Correlation-ID").Single());
    }
}
