using System.ComponentModel.DataAnnotations;

namespace TestApiNet8.Domain
{
    public class Entity<T>
    {
        public T Id { get; set; }
    }

    public class Entity : Entity<long>
    {
    }
}
