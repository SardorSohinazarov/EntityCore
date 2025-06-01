using Common.Paginations.Models;
using Common;
using DataTransferObjects.Categorys;
using TestApiNet8.Domain.Entities;

namespace Services.Categorys
{
    public interface ICategorysService
    {
        Task<CategoryViewModel> AddAsync(CategoryCreationDto categoryCreationDto);
        Task<List<CategoryViewModel>> GetAllAsync();
        Task<ListResult<CategoryViewModel>> FilterAsync(PaginationOptions filter);
        Task<CategoryViewModel> GetByIdAsync(int id);
        Task<CategoryViewModel> UpdateAsync(int id, CategoryModificationDto categoryModificationDto);
        Task<CategoryViewModel> DeleteAsync(int id);
    }
}