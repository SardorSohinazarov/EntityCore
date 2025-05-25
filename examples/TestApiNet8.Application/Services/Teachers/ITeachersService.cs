using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Common.Paginations.Models;
using Common.Paginations.Extensions;
using DataTransferObjects.Teachers;
using TestApiNet8.Domain.Entities;

namespace Services.Teachers
{
    public interface ITeachersService
    {
        Task<TeacherViewModel> AddAsync(TeacherCreationDto entity);
        Task<List<TeacherViewModel>> GetAllAsync();
        Task<List<TeacherViewModel>> FilterAsync(PaginationOptions filter);
        Task<TeacherViewModel> GetByIdAsync(int id);
        Task<TeacherViewModel> UpdateAsync(int id, TeacherModificationDto entity);
        Task<TeacherViewModel> DeleteAsync(int id);
    }
}