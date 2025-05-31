using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Common.Paginations.Models;
using Common.Paginations.Extensions;
using DataTransferObjects.Users;
using TestApiNet8.Domain.Entities;
using TestApiNet8.Application.DataTransferObjects.Users;

namespace Services.Users
{
    public interface IUsersService
    {
        Task<UserViewModel> AddAsync(UserCreationDto entity);
        Task<List<UserViewModel>> GetAllAsync();
        Task<UserListViewModel> FilterAsync(PaginationOptions filter);
        Task<UserViewModel> GetByIdAsync(int id);
        Task<UserViewModel> UpdateAsync(int id, UserModificationDto entity);
        Task<UserViewModel> DeleteAsync(int id);
    }
}