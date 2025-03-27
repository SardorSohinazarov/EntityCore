using TestApiNet8.Domain.Entities;
using TestApiWithNet8.Entities;

namespace DataTransferObjects.Teachers;

public class TeacherViewModel
{
	public int Id { get; set; }
	public int UserId { get; set; }
	public User User { get; set; }
	public List<Student> Students { get; set; }
}
