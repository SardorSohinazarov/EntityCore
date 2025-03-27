namespace DataTransferObjects.Teachers;

public class TeacherCreationDto
{
	public int UserId { get; set; }
	public List<int> StudentsIds { get; set; }
}
