namespace EntityCore.Test.Entities
{
    public class Entity : Entity<long>
    {
    }

    public class Entity<T>
    {
        public T Id { get; set; }
    }

    public interface IDeleteable
    {
        bool IsDeleted { get; set; }
        DateTime? DeletedAt { get; set; }
    }

    public interface IAuditable
    {
        DateTime CreatedAt { get; set; }
        string CreatedBy { get; set; }
        DateTime? UpdatedAt { get; set; }
        string UpdatedBy { get; set; }
    }
}
