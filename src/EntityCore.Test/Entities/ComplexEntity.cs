namespace EntityCore.Test.Entities
{
    public class ComplexEntity : Entity, IDeleteable, IAuditable
    {
        public string MainProperty { get; set; }

        // Single navigation property
        public long? OptionalRelatedId { get; set; } // Foreign key for OptionalRelated
        public RelatedEntity? OptionalRelated { get; set; }

        public long RequiredRelatedId { get; set; } // Foreign key for RequiredRelated
        public RelatedEntity RequiredRelated { get; set; } = null!;


        // Collection navigation property
        public ICollection<RelatedEntity> RelatedCollection { get; set; } = new List<RelatedEntity>();
        public List<SimpleEntity> SimpleItems { get; set; } = new List<SimpleEntity>();

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
    }
}
