using System;
using TestApiNet8.Domain.Entities;

namespace DataTransferObjects.Answers;

public class AnswerViewModel
{
    public string Text { get; set; }
    public bool IsCorrect { get; set; }
    public Guid QuestionId { get; set; }
    public Question Question { get; set; }
    public Guid Id { get; set; }
}
