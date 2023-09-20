using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using bookstoreAPI.Controllers;
using bookstoreAPI.Models;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

public class AuthorsControllerTests : IDisposable
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthorsControllerTests()
    {
        _factory = new TestWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _factory.Dispose();
        _client.Dispose();
    }

    [Fact]
    public async Task GetAuthors_Returns_SuccessStatusCode()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/authors");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    // Test for GET: api/Authors
    [Fact]
    public async Task GetAuthors_Returns_SuccessStatusCodeAndCorrectContentType()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/authors");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType.ToString());
    }

    // Test for POST and GET: api/Authors
    [Fact]
    public async Task PostAuthorAndGetAuthorById_Returns_SuccessStatusCodeAndAuthor()
    {
        // Arrange
        string newAuthorJson = "{\"name\": \"New Test Author\"}";
        var postRequest = new HttpRequestMessage(HttpMethod.Post, "/api/authors")
        {
            Content = new StringContent(newAuthorJson, Encoding.UTF8, "application/json")
        };

        // Act
        var postResponse = await _client.SendAsync(postRequest);

        // Assert (POST)
        postResponse.EnsureSuccessStatusCode();
        Assert.Equal("application/json; charset=utf-8", postResponse.Content.Headers.ContentType.ToString());

        // Get the author's ID from the POST response
        var createdAuthor = await postResponse.Content.ReadFromJsonAsync<Author>();
        var createdAuthorId = createdAuthor.Id;

        // Second request (GET with author ID)
        var getRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/authors/{createdAuthorId}");

        // Act (GET)
        var getResponse = await _client.SendAsync(getRequest);

        // Assert (GET)
        Assert.Equal("application/json; charset=utf-8", getResponse.Content.Headers.ContentType.ToString());

        var fetchedAuthor = await getResponse.Content.ReadFromJsonAsync<Author>();
        Assert.Equal(createdAuthor.Id, fetchedAuthor.Id);
        Assert.Equal("New Test Author", fetchedAuthor.Name);
    }

    // Test for PUT: api/Authors/{id}
    [Fact]
    public async Task PutAuthor_Returns_SuccessStatusCodeAndUpdatesAuthor()
    {
        // Get an existing author
        var existingAuthor = await _getOrCreateTestAuthor();
        // Arrange for the PUT request
        string updatedAuthorJson = JsonSerializer.Serialize(new { existingAuthor.Id, Name = "Updated Test Author" });
        var putRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/authors/{existingAuthor.Id}")
        {
            Content = new StringContent(updatedAuthorJson, Encoding.UTF8, "application/json")
        };

        // Act
        HttpResponseMessage putResponse = await _client.SendAsync(putRequest);

        // Assert (PUT)
        putResponse.EnsureSuccessStatusCode();

        // Get the updated author
        var getRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/authors/{existingAuthor.Id}");
        var getResponse = await _client.SendAsync(getRequest);
        var updatedAuthor = await getResponse.Content.ReadFromJsonAsync<Author>();

        // Assert (GET)
        Assert.Equal(existingAuthor.Id, updatedAuthor.Id);
        Assert.Equal("Updated Test Author", updatedAuthor.Name);
    }

    // Test for DELETE: api/Authors/{id}
    [Fact]
    public async Task DeleteAuthor_Returns_SuccessStatusCodeAndDeletesAuthor()
    {
        // Get an existing author
        var existingAuthor = await _getOrCreateTestAuthor();

        // Act (DELETE)
        var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/authors/{existingAuthor.Id}");
        HttpResponseMessage deleteResponse = await _client.SendAsync(deleteRequest);

        // Assert (DELETE)
        deleteResponse.EnsureSuccessStatusCode();

        // Act (GET)
        var getRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/authors/{existingAuthor.Id}");
        var getResponse = await _client.SendAsync(getRequest);

        // Assert (GET)
        Assert.True(getResponse.StatusCode == System.Net.HttpStatusCode.NotFound);
    }

    // Utility method to create a test author entry
    private async Task<Author> _getOrCreateTestAuthor()
    {
        var getRequest = new HttpRequestMessage(HttpMethod.Get, "/api/authors");
        var getResponse = await _client.SendAsync(getRequest);
        var authors = await getResponse.Content.ReadFromJsonAsync<IEnumerable<Author>>();

        // If no authors found, create one
        if (!authors.Any())
        {
            var postRequest = new HttpRequestMessage(HttpMethod.Post, "/api/authors")
            {
                Content = new StringContent("{\"name\": \"Test Author\"}", Encoding.UTF8, "application/json")
            };
            var postResponse = await _client.SendAsync(postRequest);
            var createdAuthor = await postResponse.Content.ReadFromJsonAsync<Author>();
            return createdAuthor;
        }
        else
        {
            return authors.First();
        }
    }
}