using Microsoft.AspNetCore.Mvc;
using Services.Teachers;
using Common.Paginations.Models;
using Common;
using DataTransferObjects.Teachers;
using TestApiNet8.Domain.Entities;

namespace Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeachersController : ControllerBase
    {
        private readonly ITeachersService _teachersService;
        public TeachersController(ITeachersService teachersService)
        {
            _teachersService = teachersService;
        }

        [HttpPost]
        public async Task<Result<TeacherViewModel>> AddAsync(TeacherCreationDto teacherCreationDto)
        {
            return Result<TeacherViewModel>.Success(await _teachersService.AddAsync(teacherCreationDto));
        }

        [HttpGet]
        public async Task<Result<List<TeacherViewModel>>> GetAllAsync()
        {
            return Result<List<TeacherViewModel>>.Success(await _teachersService.GetAllAsync());
        }

        [HttpPost("filter")]
        public async Task<Result<List<TeacherViewModel>>> FilterAsync(PaginationOptions filter)
        {
            return Result<List<TeacherViewModel>>.Success(await _teachersService.FilterAsync(filter));
        }

        [HttpGet("{id}")]
        public async Task<Result<TeacherViewModel>> GetByIdAsync(int id)
        {
            return Result<TeacherViewModel>.Success(await _teachersService.GetByIdAsync(id));
        }

        [HttpPut("{id}")]
        public async Task<Result<TeacherViewModel>> UpdateAsync(int id, TeacherModificationDto teacherModificationDto)
        {
            return Result<TeacherViewModel>.Success(await _teachersService.UpdateAsync(id, teacherModificationDto));
        }

        [HttpDelete("{id}")]
        public async Task<Result<TeacherViewModel>> DeleteAsync(int id)
        {
            return Result<TeacherViewModel>.Success(await _teachersService.DeleteAsync(id));
        }
    }
}