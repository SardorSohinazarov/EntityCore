using Common.Paginations.Models;
using Common;
using DataTransferObjects.Users;
using TestApiNet8.Domain.Entities;

namespace Services.Users
{
    public interface IUsersService
    {
        Task<UserViewModel> AddAsync(UserCreationDto userCreationDto);
        Task<List<UserViewModel>> GetAllAsync();
        Task<ListResult<UserViewModel>> FilterAsync(PaginationOptions filter);
        Task<UserViewModel> GetByIdAsync(int id);
        Task<UserViewModel> UpdateAsync(int id, UserModificationDto userModificationDto);
        Task<UserViewModel> DeleteAsync(int id);
    }
}