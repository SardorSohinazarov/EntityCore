namespace DataTransferObjects.Products;

public class ProductViewModel
{
	public string Name { get; set; }
	public decimal Price { get; set; }
	public int Quantity { get; set; }
	public DateTime CreatedAt { get; set; }
	public string CreatedBy { get; set; }
	public DateTime UpdatedAt { get; set; }
	public string UpdatedBy { get; set; }
	public long Id { get; set; }
}
