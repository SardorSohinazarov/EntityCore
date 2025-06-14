namespace DataTransferObjects.Answers;

public class AnswerCreationDto
{
	public string Text { get; set; }
	public bool IsCorrect { get; set; }
	public Guid QuestionId { get; set; }
}
