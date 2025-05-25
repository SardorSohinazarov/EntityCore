using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Common.Paginations.Models;
using Common.Paginations.Extensions;
using DataTransferObjects.Students;
using TestApiNet8.Domain.Entities;

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