using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Common.Paginations.Models;
using Common.Paginations.Extensions;
using Common.ServiceAttribute;
using DataTransferObjects.Teachers;
using TestApiWithNet8;
using TestApiNet8.Domain.Entities;

namespace Services.Teachers
{
    [ScopedService]
    public class TeachersService : ITeachersService
    {
        private readonly TestApiNet8Db _testApiNet8Db;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContext;
        public TeachersService(TestApiNet8Db testApiNet8Db, IMapper mapper, IHttpContextAccessor httpContext)
        {
            _testApiNet8Db = testApiNet8Db;
            _mapper = mapper;
            _httpContext = httpContext;
        }

        public async Task<TeacherViewModel> AddAsync(TeacherCreationDto teacherCreationDto)
        {
            var entity = _mapper.Map<Teacher>(teacherCreationDto);
            entity.Students = await _testApiNet8Db.Set<Student>().Where(x => teacherCreationDto.StudentsIds.Contains(x.Id)).ToListAsync();
            var entry = await _testApiNet8Db.Set<Teacher>().AddAsync(entity);
            await _testApiNet8Db.SaveChangesAsync();
            return _mapper.Map<TeacherViewModel>(entry.Entity);
        }

        public async Task<List<TeacherViewModel>> GetAllAsync()
        {
            var entities = await _testApiNet8Db.Set<Teacher>().ToListAsync();
            return _mapper.Map<List<TeacherViewModel>>(entities);
        }

        public async Task<List<TeacherViewModel>> FilterAsync(PaginationOptions filter)
        {
            var httpContext = _httpContext.HttpContext;
            var entities = await _testApiNet8Db.Set<Teacher>().ApplyPagination(filter, httpContext).ToListAsync();
            return _mapper.Map<List<TeacherViewModel>>(entities);
        }

        public async Task<TeacherViewModel> GetByIdAsync(int id)
        {
            var entity = await _testApiNet8Db.Set<Teacher>().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                throw new InvalidOperationException($"Teacher with Id {id} not found.");
            return _mapper.Map<TeacherViewModel>(entity);
        }

        public async Task<TeacherViewModel> UpdateAsync(int id, TeacherModificationDto teacherModificationDto)
        {
            var entity = await _testApiNet8Db.Set<Teacher>().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                throw new InvalidOperationException($"Teacher with {id} not found.");
            _mapper.Map(teacherModificationDto, entity);
            entity.Students = await _testApiNet8Db.Set<Student>().Where(x => teacherModificationDto.StudentsIds.Contains(x.Id)).ToListAsync();
            var entry = _testApiNet8Db.Set<Teacher>().Update(entity);
            await _testApiNet8Db.SaveChangesAsync();
            return _mapper.Map<TeacherViewModel>(entry.Entity);
        }

        public async Task<TeacherViewModel> DeleteAsync(int id)
        {
            var entity = await _testApiNet8Db.Set<Teacher>().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                throw new InvalidOperationException($"Teacher with {id} not found.");
            var entry = _testApiNet8Db.Set<Teacher>().Remove(entity);
            await _testApiNet8Db.SaveChangesAsync();
            return _mapper.Map<TeacherViewModel>(entry.Entity);
        }
    }

    /// <summary>
    /// AutoMapper mapping profile for Teacher entity.
    /// </summary>
    public class TeacherMappingProfile : Profile
    {
        public TeacherMappingProfile()
        {
            CreateMap<Teacher, TeacherViewModel>();
            CreateMap<TeacherCreationDto, Teacher>();
            CreateMap<TeacherModificationDto, Teacher>();
        }
    }
}