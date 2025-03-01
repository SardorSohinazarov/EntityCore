using Microsoft.AspNetCore.Mvc;
using Services.Students;
using TestApiWithNet8.Entities;

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
        public async Task<IActionResult> AddAsync(StudentCreationDto studentCreationDto)
        {
            return Ok(await _studentsService.AddAsync(studentCreationDto));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            return Ok(await _studentsService.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            return Ok(await _studentsService.GetByIdAsync(id));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, StudentModificationDto studentModificationDto)
        {
            return Ok(await _studentsService.UpdateAsync(id, studentModificationDto));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            return Ok(await _studentsService.DeleteAsync(id));
        }
    }
}