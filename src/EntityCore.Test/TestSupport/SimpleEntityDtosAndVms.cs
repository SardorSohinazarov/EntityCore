namespace EntityCore.Test.TestSupport
{
    // --- SimpleEntity DTOs ---
    public class SimpleEntityCreationDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; } // Added to match SimpleEntity
    }

    public class SimpleEntityModificationDto
    {
        public int Id { get; set; } // Assuming Id is int based on SimpleEntity.cs
        public string? Name { get; set; }
        public string? Description { get; set; } // Added to match SimpleEntity
    }

    // --- SimpleEntity ViewModel ---
    public class SimpleEntityViewModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public DateTimeOffset CreatedDate { get; set; } // Added to match SimpleEntity
    }
}
