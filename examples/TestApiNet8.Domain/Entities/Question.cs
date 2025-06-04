namespace TestApiNet8.Domain.Entities
{
    public class Question : Entity<Guid>
    {
        public string Text { get; set; }
        public User Owner { get; set; }
        public int OwnerId { get; set; }
    }
}
