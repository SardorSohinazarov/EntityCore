namespace DataTransferObjects.Students;

public class StudentCreationDto
{
    public int UserId { get; set; }
    public List<int> TeachersIds { get; set; }
}
