namespace EntityCore.Test.Entities
{
    public class SimpleEntity : Entity<Guid>
    {
        public string Name { get; set; }
        public int Value { get; set; }
        public bool IsActive { get; set; }
    }
}
