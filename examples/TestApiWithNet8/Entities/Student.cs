using Microsoft.EntityFrameworkCore;

namespace TestApiWithNet8.Entities
{
    public class Student
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Age { get; set; }
    }

    public class StudentViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Age { get; set; }
    }

    public class StudentCreationDto
    {
        public string Name { get; set; }
        public string Age { get; set; }
    }

    public class StudentModificationDto
    {
        public string Name { get; set; }
        public string Age { get; set; }
    }

    public class ApplicationDbContext : DbContext
    {
        public DbSet<Student> Students { get; set; }
    }
}
