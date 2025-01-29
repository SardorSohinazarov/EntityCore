using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TestApi004.DataTransferObjects;
using TestApi001.Entities;

namespace Services.Products
{
    public interface IProductsService
    {
        Task<ProductViewModel> AddAsync(ProductCreationDto entity);
        Task<List<ProductViewModel>> GetAllAsync();
        Task<ProductViewModel> GetByIdAsync(int id);
        Task<ProductViewModel> UpdateAsync(int id, ProductModificationDto entity);
        Task<ProductViewModel> DeleteAsync(int id);
    }
}