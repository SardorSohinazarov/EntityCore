using TestApiNet8.Domain.Entities;

namespace DataTransferObjects.Teachers;

public class TeacherViewModel
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public ICollection<Student> Students { get; set; }
}
