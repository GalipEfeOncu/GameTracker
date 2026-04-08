using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace GameTracker.Api.Tests;

/// <summary>
/// Connection string bilinçli olarak boşaltıldığında DB çağrıları 503 ve makine-okur kod döner.
/// </summary>
public class DatabaseNotConfiguredTests : IClassFixture<NoDatabaseWebApplicationFactory>
{
    private readonly HttpClient _client;

    public DatabaseNotConfiguredTests(NoDatabaseWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WhenConnectionStringEmpty_Returns503_WithErrorCode()
    {
        var res = await _client.PostAsJsonAsync(
            "/api/User/register",
            new
            {
                email = "db-test@example.com",
                username = "dbtestuser",
                password = "longenough"
            });

        Assert.Equal(HttpStatusCode.ServiceUnavailable, res.StatusCode);
        var body = await res.Content.ReadAsStringAsync();
        Assert.Contains("database_unconfigured", body, StringComparison.Ordinal);
    }
}
