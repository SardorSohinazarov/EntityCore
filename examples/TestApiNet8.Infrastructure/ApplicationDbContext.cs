using Microsoft.EntityFrameworkCore;
using TestApiWithNet8.Entities;

namespace TestApiWithNet8
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Student> Students { get; set; }
    }
}
