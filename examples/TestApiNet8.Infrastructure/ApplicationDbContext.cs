using Microsoft.EntityFrameworkCore;
using TestApiWithNet8.Entities;

namespace TestApiWithNet8
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext() 
            => Database.Migrate();

        public DbSet<Student> Students { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=TestApi;");
        }
    }
}
