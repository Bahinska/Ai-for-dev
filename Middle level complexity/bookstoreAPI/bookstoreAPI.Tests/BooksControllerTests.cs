using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using bookstoreAPI.Models;
using System.Net.Http.Json;

public class BooksControllerTests : IDisposable
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public BooksControllerTests()
    {
        _factory = new TestWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _factory.Dispose();
        _client.Dispose();
    }
    // Test for GET: api/Books with parameters
    [Theory]
    [InlineData("Title4", "Author4", "Genre4")]
    public async Task GetBooks_WithParameters_Returns_SuccessStatusCodeAndCorrectContentType(
        string title, string author, string genre)
    {
        // Arrange
        StringBuilder sb = new StringBuilder("/api/books");
        List<string> queryParams = new List<string>();

        if (!string.IsNullOrEmpty(title))
        {
            queryParams.Add($"title={Uri.EscapeDataString(title)}");
        }

        if (!string.IsNullOrEmpty(author))
        {
            queryParams.Add($"author={Uri.EscapeDataString(author)}");
        }

        if (!string.IsNullOrEmpty(genre))
        {
            queryParams.Add($"genre={Uri.EscapeDataString(genre)}");
        }

        if (queryParams.Any())
        {
            sb.Append("?").Append(string.Join("&", queryParams));
        }

        var request = new HttpRequestMessage(HttpMethod.Get, sb.ToString());

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType.ToString());
    }


    // Test for GET: api/Books/{id}
    [Fact]
    public async Task GetBookById_Returns_SuccessStatusCodeAndCorrectContentType()
    {
        // Set up a book for the test
        Book testBook = await _createTestBook();

        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/books/{testBook.Id}");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType.ToString());
    }

    // Test for POST and GET: api/Books/{id}
    [Fact]
    public async Task PostBookAndGetBookById_Returns_SuccessStatusCodeAndBook()
    {
        // Arrange the POST request
        Book newBook = new Book
        {
            Title = "New Test Book",
            Author = new Author { Name = "New Test Author" },
            Genre = new Genre { Name = "New Test Genre" }
        };

        string newBookJson = JsonSerializer.Serialize(newBook,
            new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull });

        var postRequest = new HttpRequestMessage(HttpMethod.Post, "/api/books")
        {
            Content = new StringContent(newBookJson, Encoding.UTF8, "application/json")
        };

        // Act (POST)
        HttpResponseMessage postResponse = await _client.SendAsync(postRequest);

        // Assert (POST)
        postResponse.EnsureSuccessStatusCode();
       

        // Get the book's ID from the POST response
        Book createdBook = await postResponse.Content.ReadFromJsonAsync<Book>();

        // Second request (GET with Book ID)
        var getRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/books/{createdBook.Id}");

        // Act (GET)
        HttpResponseMessage getResponse = await _client.SendAsync(getRequest);

        Book fetchedBook = await getResponse.Content.ReadFromJsonAsync<Book>();
        Assert.Equal(createdBook.Id, fetchedBook.Id);
        Assert.Equal(newBook.Title, fetchedBook.Title);
        Assert.Equal(newBook.Author.Name, fetchedBook.Author.Name);
        Assert.Equal(newBook.Genre.Name, fetchedBook.Genre.Name);
    }

    // Test for PUT: api/Books/{id}

    [Fact]
    public async Task PutBook_Returns_SuccessStatusCodeAndUpdatesBook()
    {
        // Get or create a test book
        Book existingBook = await _createTestBook();

        // Arrange for the PUT request
        Book updatedBook = new Book
        {
            Id = existingBook.Id,
            Title = "Updated Test Book",
            Author = existingBook.Author, // Keep the same Author and Genre
            Genre = existingBook.Genre
        };

        string updatedBookJson = JsonSerializer.Serialize(updatedBook,
            new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull });

        var putRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/books/{updatedBook.Id}")
        {
            Content = new StringContent(updatedBookJson, Encoding.UTF8, "application/json")
        };

        // Act
        HttpResponseMessage putResponse = await _client.SendAsync(putRequest);

        // Assert (PUT)
        putResponse.EnsureSuccessStatusCode();

        // Get the updated book
        var getRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/books/{updatedBook.Id}");
        var getResponse = await _client.SendAsync(getRequest);
        Book fetchedUpdatedBook = await getResponse.Content.ReadFromJsonAsync<Book>();

        // Assert (GET)
        Assert.Equal(existingBook.Id, fetchedUpdatedBook.Id);
        Assert.Equal("Updated Test Book", fetchedUpdatedBook.Title);
        Assert.Equal(existingBook.Author.Id, fetchedUpdatedBook.Author.Id);
        Assert.Equal(existingBook.Genre.Id, fetchedUpdatedBook.Genre.Id);
    }

    // Test for DELETE: api/Books/{id}
    [Fact]
    public async Task DeleteBook_Returns_SuccessStatusCodeAndDeletesBook()
    {
        // Get or create a test book
        Book existingBook = await _createTestBook();

        // Act (DELETE)
        var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/books/{existingBook.Id}");
        HttpResponseMessage deleteResponse = await _client.SendAsync(deleteRequest);

        // Assert (DELETE)
        deleteResponse.EnsureSuccessStatusCode();

        // Act (GET)
        var getRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/books/{existingBook.Id}");
        var getResponse = await _client.SendAsync(getRequest);

        // Assert (GET)
        Assert.True(getResponse.StatusCode == System.Net.HttpStatusCode.NotFound);
    }

    // Utility method to create a test book entry
    private async Task<Book> _createTestBook()
    {
        Book newBook = new Book
        {
            Title = "Test Book",
            Author = new Author { Name = "Test Author" },
            Genre = new Genre { Name = "Test Genre" }
        };

        string newBookJson = JsonSerializer.Serialize(newBook,
            new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull });

        var postRequest = new HttpRequestMessage(HttpMethod.Post, "/api/books")
        {
            Content = new StringContent(newBookJson, Encoding.UTF8, "application/json")
        };

        HttpResponseMessage postResponse = await _client.SendAsync(postRequest);
        Book createdBook = await postResponse.Content.ReadFromJsonAsync<Book>();

        return createdBook;
    }
    private async Task<Book> _getOrCreateTestBook()
    {
        var getRequest = new HttpRequestMessage(HttpMethod.Get, "/api/books");
        var getResponse = await _client.SendAsync(getRequest);
        var books = await getResponse.Content.ReadFromJsonAsync<List<Book>>(); // Change this line to use List<Book>

        // If no books found, create one
        if (!books.Any())
        {
            return await _createTestBook();
        }
        else
        {
            return books.First();
        }
    }
}