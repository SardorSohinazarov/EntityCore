using TestApiNet8.Domain.Entities;

namespace DataTransferObjects.Students;

public class StudentViewModel
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public List<Teacher> Teachers { get; set; }
}
