using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Text.Json;
using WordFinder.Application.DTOs;
using WordFinder.Api;
using Xunit;

namespace WordFinder.Integration.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
    }
}

public class WordFinderIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public WordFinderIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task SearchWords_WithValidRequest_ShouldReturnFoundWords()
    {
        // Arrange
        var request = new WordSearchRequest
        {
            Matrix = new List<string>
            {
                "abcdc",
                "fgwio",
                "chill",
                "pqnsd",
                "uvdxy"
            },
            WordStream = new List<string> { "chill", "cold", "wind", "abc" }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/wordfinder/search", request);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<WordSearchResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(result);
        Assert.Contains("chill", result.FoundWords); // horizontal in row 2
        Assert.Contains("cold", result.FoundWords);  // vertical in column 4  
        Assert.Contains("wind", result.FoundWords);  // vertical in column 2
        Assert.Contains("abc", result.FoundWords);   // horizontal in row 0
        Assert.True(result.ProcessingTimeMs >= 0);
        Assert.Equal(4, result.TotalWordsSearched); // Unique words in stream
    }

    [Fact]
    public async Task SearchWords_WithEmptyMatrix_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new WordSearchRequest
        {
            Matrix = new List<string>(),
            WordStream = new List<string> { "test" }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/wordfinder/search", request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SearchWords_WithPerformanceMetrics_ShouldIncludeTimings()
    {
        // Arrange
        var request = new WordSearchRequest
        {
            Matrix = new List<string> { "test", "word", "find", "perf" },
            WordStream = new List<string> { "test", "word", "find" }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/wordfinder/search", request);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<WordSearchResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(result);
        Assert.True(result.ProcessingTimeMs >= 0);
        Assert.True(result.TotalWordsSearched > 0);
    }
}
