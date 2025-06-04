using System;
using TestApiNet8.Domain.Entities;

namespace DataTransferObjects.Questions;

public class QuestionViewModel
{
    public string Text { get; set; }
    public User Owner { get; set; }
    public int OwnerId { get; set; }
    public Guid Id { get; set; }
}
