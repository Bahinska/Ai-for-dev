using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using bookstoreAPI.Models;
using System.Net.Http.Json;
using System.Text;

public class GenresControllerTests : IDisposable
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public GenresControllerTests()
    {
        _factory = new TestWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _factory.Dispose();
        _client.Dispose();
    }

    // Test for GET: api/Genres
    [Fact]
    public async Task GetGenres_Returns_SuccessStatusCodeAndCorrectContentType()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/genres");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType.ToString());
    }

    // Test for POST and GET: api/Genres/{id}
    [Fact]
    public async Task PostGenreAndGetGenreById_Returns_SuccessStatusCodeAndGenre()
    {
        // Arrange
        string newGenreJson = "{\"name\": \"New Test Genre\"}";
        var postRequest = new HttpRequestMessage(HttpMethod.Post, "/api/genres")
        {
            Content = new StringContent(newGenreJson, Encoding.UTF8, "application/json")
        };

        // Act
        var postResponse = await _client.SendAsync(postRequest);

        // Assert (POST)
        postResponse.EnsureSuccessStatusCode();
        Assert.Equal("application/json; charset=utf-8", postResponse.Content.Headers.ContentType.ToString());

        // Get the genre's ID from the POST response
        var createdGenre = await postResponse.Content.ReadFromJsonAsync<Genre>();
        var createdGenreId = createdGenre.Id;

        // Second request (GET with Genre ID)
        var getRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/genres/{createdGenreId}");

        // Act (GET)
        var getResponse = await _client.SendAsync(getRequest);

        // Assert (GET)
        Assert.Equal("application/json; charset=utf-8", getResponse.Content.Headers.ContentType.ToString());

        var fetchedGenre = await getResponse.Content.ReadFromJsonAsync<Genre>();
        Assert.Equal(createdGenre.Id, fetchedGenre.Id);
        Assert.Equal("New Test Genre", fetchedGenre.Name);
    }

    // Test for PUT: api/Genres/{id}
    [Fact]
    public async Task PutGenre_Returns_SuccessStatusCodeAndUpdatesGenre()
    {
        // Get or create a test genre
        var existingGenre = await _getOrCreateTestGenre();

        // Arrange for the PUT request
        string updatedGenreJson = JsonSerializer.Serialize(new { existingGenre.Id, Name = "Updated Test Genre" });
        var putRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/genres/{existingGenre.Id}")
        {
            Content = new StringContent(updatedGenreJson, Encoding.UTF8, "application/json")
        };

        // Act
        HttpResponseMessage putResponse = await _client.SendAsync(putRequest);

        // Assert (PUT)
        putResponse.EnsureSuccessStatusCode();

        // Get the updated genre
        var getRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/genres/{existingGenre.Id}");
        var getResponse = await _client.SendAsync(getRequest);
        var updatedGenre = await getResponse.Content.ReadFromJsonAsync<Genre>();

        // Assert (GET)
        Assert.Equal(existingGenre.Id, updatedGenre.Id);
        Assert.Equal("Updated Test Genre", updatedGenre.Name);
    }

    // Test for DELETE: api/Genres/{id}
    [Fact]
    public async Task DeleteGenre_Returns_SuccessStatusCodeAndDeletesGenre()
    {
        // Get or create a test genre
        var existingGenre = await _getOrCreateTestGenre();

        // Act (DELETE)
        var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/genres/{existingGenre.Id}");
        HttpResponseMessage deleteResponse = await _client.SendAsync(deleteRequest);

        // Assert (DELETE)
        deleteResponse.EnsureSuccessStatusCode();

        // Act (GET)
        var getRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/genres/{existingGenre.Id}");
        var getResponse = await _client.SendAsync(getRequest);

        // Assert (GET)
        Assert.True(getResponse.StatusCode == System.Net.HttpStatusCode.NotFound);
    }

    // Utility method to create a test genre entry
    private async Task<Genre> _getOrCreateTestGenre()
    {
        var getRequest = new HttpRequestMessage(HttpMethod.Get, "/api/genres");
        var getResponse = await _client.SendAsync(getRequest);
        var genres = await getResponse.Content.ReadFromJsonAsync<IEnumerable<Genre>>();

        // If no genres found, create one
        if (!genres.Any())
        {
            var postRequest = new HttpRequestMessage(HttpMethod.Post, "/api/genres")
            {
                Content = new StringContent("{\"name\": \"Test Genre\"}", Encoding.UTF8, "application/json")
            };
            var postResponse = await _client.SendAsync(postRequest);
            var createdGenre = await postResponse.Content.ReadFromJsonAsync<Genre>();
            return createdGenre;
        }
        else
        {
            return genres.First();
        }
    }
}
