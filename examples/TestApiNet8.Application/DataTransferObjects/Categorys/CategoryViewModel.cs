using System;
using TestApiNet8.Domain.Entities;
using System.Collections.Generic;

namespace DataTransferObjects.Categorys;

public class CategoryViewModel
{
    public string Name { get; set; }
    public int? ParentCategoryId { get; set; }
    public Category ParentCategory { get; set; }
    public ICollection<Category> ChildCategories { get; set; }
    public ICollection<Product> Products { get; set; }
    public int Id { get; set; }
}
