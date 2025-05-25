using System;
using TestApiNet8.Domain.Entities;
using System.Collections.Generic;

namespace DataTransferObjects.Teachers;

public class TeacherViewModel
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public ICollection<Student> Students { get; set; }
}
