using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; // Required for potential future use, even if not directly for Key on FKs

namespace EntityCore.Tools.Tests.TestEntities
{
    public class MainTestEntity : BaseEntity
    {
        public string MainProperty { get; set; }

        // Single navigation property
        public Guid? OptionalRelatedId { get; set; } // Foreign key for OptionalRelated
        public RelatedEntity? OptionalRelated { get; set; }

        public Guid RequiredRelatedId { get; set; } // Foreign key for RequiredRelated
        public RelatedEntity RequiredRelated { get; set; } = null!;


        // Collection navigation property
        public ICollection<RelatedEntity> RelatedCollection { get; set; } = new List<RelatedEntity>();
        public List<SimpleEntity> SimpleItems { get; set; } = new List<SimpleEntity>();
    }
}
