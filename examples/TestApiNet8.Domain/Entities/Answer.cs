namespace TestApiNet8.Domain.Entities
{
    public class Answer : Entity<Guid>
    {
        public string Text { get; set; }
        public bool IsCorrect { get; set; }
        public Guid QuestionId { get; set; }
        public Question Question { get; set; }
    }
}
