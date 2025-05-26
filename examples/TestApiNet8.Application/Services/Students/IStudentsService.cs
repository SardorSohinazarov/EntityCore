using Common.Paginations.Models;
using DataTransferObjects.Students;

namespace Services.Students
{
    public interface IStudentsService
    {
        Task<StudentViewModel> AddAsync(StudentCreationDto entity);
        Task<List<StudentViewModel>> GetAllAsync();
        Task<List<StudentViewModel>> FilterAsync(PaginationOptions filter);
        Task<StudentViewModel> GetByIdAsync(int id);
        Task<StudentViewModel> UpdateAsync(int id, StudentModificationDto entity);
        Task<StudentViewModel> DeleteAsync(int id);
    }
}