using Microsoft.EntityFrameworkCore;
using AutoMapper;
using TestApi004.DataTransferObjects;
using TestApi001.Entities;

namespace Services.Products
{
    public interface IProductsService
    {
        Task<ProductViewModel> AddAsync(Product entity);
        Task<List<ProductViewModel>> GetAllAsync();
        Task<ProductViewModel> GetByIdAsync(int id);
        Task<ProductViewModel> UpdateAsync(int id, Product entity);
        Task<ProductViewModel> DeleteAsync(int id);
    }
}