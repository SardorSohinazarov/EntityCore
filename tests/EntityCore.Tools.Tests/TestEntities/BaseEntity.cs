using System;
using System.ComponentModel.DataAnnotations;

namespace EntityCore.Tools.Tests.TestEntities
{
    public class BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
