using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;

namespace bookstoreAPI.Data
{
    public class BookstoreContextFactory : IDesignTimeDbContextFactory<BookstoreContext>
    {
        public BookstoreContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<BookstoreContext>();
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("BookstoreConnection"));

            return new BookstoreContext(optionsBuilder.Options);
        }
    }
}
