using Microsoft.EntityFrameworkCore;
using TestApiNet8.Domain.Entities;

namespace TestApiWithNet8
{
    public class TestApiNet8Db : DbContext
    {
        public TestApiNet8Db()
            => Database.Migrate();

        public DbSet<Student> Students { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Test> Tests { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=TestApiNet8Db;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Teacher-Student ko‘p-ko‘p bog‘lanish
            modelBuilder.Entity<Student>()
                .HasMany(s => s.Teachers)
                .WithMany(t => t.Students)
                .UsingEntity<Dictionary<string, object>>(
                    "StudentTeacher",
                    j => j
                        .HasOne<Teacher>()
                        .WithMany()
                        .HasForeignKey("TeachersId")
                        .OnDelete(DeleteBehavior.Restrict),  // Cascade delete o‘rniga restrict
                    j => j
                        .HasOne<Student>()
                        .WithMany()
                        .HasForeignKey("StudentsId")
                        .OnDelete(DeleteBehavior.Restrict)); // Cascade delete o‘rniga restrict

            // Category va Product o‘rtasidagi bog‘lanish
            modelBuilder.Entity<Category>()
                .HasMany(c => c.ChildCategories)
                .WithOne(c => c.ParentCategory)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);

            modelBuilder.Entity<Category>()
                .HasMany(c => c.Products)
                .WithOne(p => p.Category)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
