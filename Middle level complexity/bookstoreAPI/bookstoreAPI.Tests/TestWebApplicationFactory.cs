using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using bookstoreAPI.Models;

public class TestWebApplicationFactory : WebApplicationFactory<bookstoreAPI.Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<BookstoreContext>));
            services.Remove(descriptor);

            // Register an in-memory database for testing
            services.AddDbContext<BookstoreContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryTestDB");
            });
        });
    }
}