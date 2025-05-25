namespace DataTransferObjects.Students;

public class StudentModificationDto
{
	public int UserId { get; set; }
	public List<int> TeachersIds { get; set; }
}
