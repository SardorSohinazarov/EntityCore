namespace DataTransferObjects.Answers;

public class AnswerModificationDto
{
	public string Text { get; set; }
	public bool IsCorrect { get; set; }
	public Guid QuestionId { get; set; }
}
