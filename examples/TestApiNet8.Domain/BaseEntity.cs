using System.ComponentModel.DataAnnotations;

namespace TestApiNet8.Domain
{
    public class BaseEntity
    {
        [Key]
        public long Id { get; set; }
    }
}
