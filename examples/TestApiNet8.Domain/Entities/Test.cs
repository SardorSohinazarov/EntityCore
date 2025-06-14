namespace TestApiNet8.Domain.Entities
{
    public class Test : Entity<Guid>
    {
        public string Name { get; set; }
        public User Owner { get; set; }
        public int OwnerId { get; set; }
    }
}
