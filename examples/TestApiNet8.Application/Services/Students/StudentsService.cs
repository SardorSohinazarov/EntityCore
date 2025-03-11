using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Common.Paginations.Models;
using Common.Paginations.Extensions;
using TestApiNet8.Application.DataTransferObjects.Students;
using TestApiWithNet8;
using TestApiWithNet8.Entities;

namespace Services.Students
{
    public class StudentsService : IStudentsService
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContext;
        public StudentsService(ApplicationDbContext applicationDbContext, IMapper mapper, IHttpContextAccessor httpContext)
        {
            _applicationDbContext = applicationDbContext;
            _mapper = mapper;
            _httpContext = httpContext;
        }

        public async Task<StudentViewModel> AddAsync(StudentCreationDto studentCreationDto)
        {
            var entity = _mapper.Map<Student>(studentCreationDto);
            var entry = await _applicationDbContext.Set<Student>().AddAsync(entity);
            await _applicationDbContext.SaveChangesAsync();
            return _mapper.Map<StudentViewModel>(entry.Entity);
        }

        public async Task<List<StudentViewModel>> GetAllAsync()
        {
            var entities = await _applicationDbContext.Set<Student>().ToListAsync();
            return _mapper.Map<List<StudentViewModel>>(entities);
        }

        public async Task<List<StudentViewModel>> FilterAsync(PaginationOptions filter)
        {
            var httpContext = _httpContext.HttpContext;
            var entities = await _applicationDbContext.Set<Student>().ApplyPagination(filter, httpContext).ToListAsync();
            return _mapper.Map<List<StudentViewModel>>(entities);
        }

        public async Task<StudentViewModel> GetByIdAsync(int id)
        {
            var entity = await _applicationDbContext.Set<Student>().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                throw new InvalidOperationException($"Student with Id {id} not found.");
            return _mapper.Map<StudentViewModel>(entity);
        }

        public async Task<StudentViewModel> UpdateAsync(int id, StudentModificationDto studentModificationDto)
        {
            var entity = await _applicationDbContext.Set<Student>().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                throw new InvalidOperationException($"Student with {id} not found.");
            _mapper.Map(studentModificationDto, entity);
            var entry = _applicationDbContext.Set<Student>().Update(entity);
            await _applicationDbContext.SaveChangesAsync();
            return _mapper.Map<StudentViewModel>(entry.Entity);
        }

        public async Task<StudentViewModel> DeleteAsync(int id)
        {
            var entity = await _applicationDbContext.Set<Student>().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                throw new InvalidOperationException($"Student with {id} not found.");
            var entry = _applicationDbContext.Set<Student>().Remove(entity);
            await _applicationDbContext.SaveChangesAsync();
            return _mapper.Map<StudentViewModel>(entry.Entity);
        }
    }

    /// <summary>
    /// AutoMapper mapping profile for Student entity.
    /// </summary>
    public class StudentMappingProfile : Profile
    {
        public StudentMappingProfile()
        {
            CreateMap<Student, StudentViewModel>();
            CreateMap<StudentCreationDto, Student>();
            CreateMap<StudentModificationDto, Student>();
        }
    }
}