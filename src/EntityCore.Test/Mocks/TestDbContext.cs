using EntityCore.Test.Entities;
using Microsoft.EntityFrameworkCore;

namespace EntityCore.Test.Mocks
{
    public class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
        {
        }

        public DbSet<SimpleEntity> SimpleEntities { get; set; }
        public DbSet<ComplexEntity> ComplexEntities { get; set; }
        public DbSet<RelatedEntity> RelatedEntities { get; set; }
    }
}
