using Microsoft.EntityFrameworkCore;
using TestApi001.Entities;

namespace TestApi001
{
    public class TestApiDbContext : DbContext
    {
        public TestApiDbContext(DbContextOptions<TestApiDbContext> options) : base(options)
        {
            Database.Migrate();
        }

        public DbSet<Product> Products { get; set; }
    }
}
