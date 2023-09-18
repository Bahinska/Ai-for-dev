using mediaAPI.Models;
using mediaAPI.Repositories;
using Microsoft.Extensions.DependencyInjection;
using mediaAPI;
using Microsoft.AspNetCore.Diagnostics;
using Npgsql;
using System.Data;
using System.Text.Json;

// Create and configure the application
var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddScoped<IDbConnection>(x => new NpgsqlConnection(connectionString));
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

TestAppConfiguration.ConfigureServices(builder);
var app = builder.Build();

TestAppConfiguration.Configure(app);

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
            var exception = exceptionHandlerPathFeature?.Error;

            var result = JsonSerializer.Serialize(new { error = exception?.Message });
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(result);
        });
    });
}

app.Run();