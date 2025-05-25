namespace TestApiNet8.Domain.Entities
{
    public class Teacher
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }

        public List<Student> Students { get; set; }
    }
}
