namespace TestApiNet8.Domain.Entities
{
    public class Category : Entity<int>
    {
        public string Name { get; set; }
        public int? ParentCategoryId { get; set; }
        public Category? ParentCategory { get; set; }
        public ICollection<Category> ChildCategories { get; set; } = new List<Category>();
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
