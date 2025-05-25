
namespace DataTransferObjects.Teachers;

public class TeacherCreationDto
{
	public int UserId { get; set; }
	public ICollection<int> StudentsIds { get; set; }
}
