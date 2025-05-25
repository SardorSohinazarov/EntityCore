namespace DataTransferObjects.Teachers;

public class TeacherModificationDto
{
	public int UserId { get; set; }
	public List<int> StudentsIds { get; set; }
}
