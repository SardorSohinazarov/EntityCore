namespace TestApiNet8.Domain.Entities
{
    public class Product : Entity
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }

        public User Creator { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }
    }
}
