using mediaAPI.Models;
using mediaAPI.Repositories;
using mediaAPI.Tests;
using Moq;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Http;

public class MediaAPITests : IClassFixture<MediaApiFactory>
{
    private readonly MediaApiFactory _factory;

    public MediaAPITests(MediaApiFactory factory)
    {
        _factory = factory;
    }

    [Theory]
    [InlineData("Test title", "Test body", 1, HttpStatusCode.Created)]
    [InlineData("", "Test body", 1, HttpStatusCode.BadRequest)]
    [InlineData("Test title", "", 1, HttpStatusCode.BadRequest)]
    [InlineData("Test title", "Test body", -1, HttpStatusCode.BadRequest)]
    public async Task Test_PostRoute(string title, string body, int author, HttpStatusCode expectedStatusCode)
    {
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(_factory.ConfigureServices);
        }).CreateClient();

        var response = await client.PostAsJsonAsync("/post", new Post { Title = title, Body = body, Author = author });

        // Check the results
        Assert.Equal(expectedStatusCode, response.StatusCode);
    }

    [Theory]
    [InlineData(1, HttpStatusCode.OK)]
    [InlineData(2, HttpStatusCode.NotFound)]
    public async Task Test_GetPostById(int postId, HttpStatusCode expectedStatusCode)
    {
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(_factory.ConfigureServices);
        }).CreateClient();

        var response = await client.GetAsync($"/post/{postId}");

        Assert.Equal(expectedStatusCode, response.StatusCode);

        if (response.IsSuccessStatusCode)
        {
            var post = await response.Content.ReadFromJsonAsync<Post>();

            Assert.NotNull(post);
            Assert.Equal(1, post.Author);
            Assert.Equal("Test title", post.Title);
        }
    }

    [Theory]
    [InlineData(1, 1, HttpStatusCode.OK)]
    [InlineData(1, 2, HttpStatusCode.BadRequest)]
    public async Task Test_LikePost(int userId, int postId, HttpStatusCode expectedStatusCode)
    {
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(_factory.ConfigureServicesScenarioForLikes);
        }).CreateClient();

        var response = await client.PostAsync($"/user/{userId}/likes/{postId}", null);

        // Check if the like operation returned the expected status code
        Assert.Equal(expectedStatusCode, response.StatusCode);
    }

    [Theory]
    [InlineData(1, 2, HttpStatusCode.OK)]
    [InlineData(1, 3, HttpStatusCode.BadRequest)]
    [InlineData(4, 2, HttpStatusCode.BadRequest)]
    public async Task Test_SubscribeUser(int followerId, int followeeId, HttpStatusCode expectedStatusCode)
    { 
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(_factory.ConfigureServices);
        }).CreateClient();

        var response = await client.PostAsync($"/user/{followerId}/subscribes/{followeeId}", null);

        Assert.Equal(expectedStatusCode, response.StatusCode);
    }

    [Fact]
    public async Task Test_GetAllPostsPass()
    {
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(_factory.ConfigureServices);
        }).CreateClient();

        var response = await client.GetAsync("/posts");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    }
    [Fact]
    public async Task Test_GetAllPostsFail()
    {
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(_factory.ConfigureServicesScenarioEmptyPostRepo);
        }).CreateClient();

        var response = await client.GetAsync("/posts");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

    }
}