using mediaAPI.Models;
using mediaAPI.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mediaAPI
{
    public class TestAppConfiguration
    {
        public static void ConfigureServices(WebApplicationBuilder builder)
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddScoped<IDbConnection>(x => new NpgsqlConnection(connectionString));
            builder.Services.AddScoped<IPostRepository, PostRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
        }

        public static void Configure(WebApplication app)
        {
            
            // Route for creating a new post
            app.MapPost("/post", async ([FromServices] IPostRepository postRepository, [FromServices] IUserRepository userRepository, [FromBody] Post post) =>
            {
                // Validate the required fields
                if (string.IsNullOrWhiteSpace(post.Title) || string.IsNullOrWhiteSpace(post.Body))
                {
                    return Results.BadRequest("Title and body are required.");
                }

                // Check for a valid author
                var author = await userRepository.GetUserByIdAsync(post.Author);
                if (author == null) return Results.BadRequest("Error: Such author does not exist.");

                // Insert the post into the posts table
                var Id = await postRepository.CreatePostAsync(post);
                return Results.Created($"/posts/{Id}", post);
            });

            // Route for a user to follow another user
            app.MapPost("/user/{followerId}/subscribes/{followeeId}", async ([FromServices] IUserRepository userRepository, int followerId, int followeeId) =>
            {
                // Query the users table to get the follower and followee
                var follower = await userRepository.GetUserByIdAsync(followerId);
                var followee = await userRepository.GetUserByIdAsync(followeeId);

                // Check if both the follower and followee exist
                if (follower == null || followee == null) return Results.BadRequest("Error: Follower or followee does not exist.");

                // Insert the relationship into the followers table
                await userRepository.SubscribeAsync(followerId, followeeId);
                return Results.Ok();
            });

            // Route for a user to like a post
            app.MapPost("/user/{userId}/likes/{postId}", async ([FromServices] IPostRepository postRepository, [FromServices] IUserRepository userRepository, int userId, int postId) =>
            {
                // Query the users and posts tables to get the user and post
                var user = await userRepository.GetUserByIdAsync(userId);
                var post = await postRepository.GetPostByIdAsync(postId);

                // Check if both the user and post exist
                if (user == null || post == null) return Results.BadRequest("Error: User or post does not exist.");

                // Increment the likes count for the post
                await postRepository.LikePostAsync(postId);
                return Results.Ok("The post has been liked successfully.");
            });

            // Route for getting a specific post by its ID
            app.MapGet("/post/{id}", async ([FromServices] IPostRepository postRepository, int id) =>
            {
                // Query the post with the given ID
                var result = await postRepository.GetPostByIdAsync(id);

                // Return the post if found, or a NotFound result if not found
                if (result == null) return Results.NotFound();
                return Results.Ok(result);
            });

            // Route for getting all posts
            app.MapGet("/posts", async ([FromServices] IPostRepository postRepository) =>
            {
                // Query all the posts
                var result = await postRepository.GetAllPostsAsync();

                // Return the posts if any found, or a NotFound result if no posts found
                if (!result.Any()) return Results.NotFound();
                return Results.Ok(result);
            });
        }
    }
}
