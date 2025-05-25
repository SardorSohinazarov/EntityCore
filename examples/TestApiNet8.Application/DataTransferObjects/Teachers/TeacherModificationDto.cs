namespace DataTransferObjects.Teachers;

public class TeacherModificationDto
{
	public int UserId { get; set; }
	public ICollection<int> StudentsIds { get; set; }
}
