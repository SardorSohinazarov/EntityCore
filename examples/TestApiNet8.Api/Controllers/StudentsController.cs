using Microsoft.AspNetCore.Mvc;
using Services.Students;
using Common.Paginations.Models;
using Common;
using DataTransferObjects.Students;
using TestApiNet8.Domain.Entities;

namespace Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentsService _studentsService;
        public StudentsController(IStudentsService studentsService)
        {
            _studentsService = studentsService;
        }

        [HttpPost]
        public async Task<Result<StudentViewModel>> AddAsync(StudentCreationDto studentCreationDto)
        {
            return Result<StudentViewModel>.Success(await _studentsService.AddAsync(studentCreationDto));
        }

        [HttpGet]
        public async Task<Result<List<StudentViewModel>>> GetAllAsync()
        {
            return Result<List<StudentViewModel>>.Success(await _studentsService.GetAllAsync());
        }

        [HttpPost("filter")]
        public async Task<Result<List<StudentViewModel>>> FilterAsync(PaginationOptions filter)
        {
            return Result<List<StudentViewModel>>.Success(await _studentsService.FilterAsync(filter));
        }

        [HttpGet("{id}")]
        public async Task<Result<StudentViewModel>> GetByIdAsync(int id)
        {
            return Result<StudentViewModel>.Success(await _studentsService.GetByIdAsync(id));
        }

        [HttpPut("{id}")]
        public async Task<Result<StudentViewModel>> UpdateAsync(int id, StudentModificationDto studentModificationDto)
        {
            return Result<StudentViewModel>.Success(await _studentsService.UpdateAsync(id, studentModificationDto));
        }

        [HttpDelete("{id}")]
        public async Task<Result<StudentViewModel>> DeleteAsync(int id)
        {
            return Result<StudentViewModel>.Success(await _studentsService.DeleteAsync(id));
        }
    }
}