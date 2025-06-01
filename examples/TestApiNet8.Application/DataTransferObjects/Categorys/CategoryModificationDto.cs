namespace DataTransferObjects.Categorys;

public class CategoryModificationDto
{
	public string Name { get; set; }
	public int? ParentCategoryId { get; set; }
	public ICollection<int> ChildCategoriesIds { get; set; }
	public ICollection<long> ProductsIds { get; set; }
}
