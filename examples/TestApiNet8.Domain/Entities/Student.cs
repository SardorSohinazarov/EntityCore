namespace TestApiNet8.Domain.Entities
{
    public class Student
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }

        public List<Teacher> Teachers { get; set; }
    }
}
