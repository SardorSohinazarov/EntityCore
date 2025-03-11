using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Common.Paginations.Models;
using Common.Paginations.Extensions;
using TestApiNet8.Application.DataTransferObjects.Students;
using TestApiWithNet8.Entities;

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