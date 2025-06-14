using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Common.Paginations.Models;
using Common.Paginations.Extensions;
using Common.ServiceAttribute;
using Common;
using DataTransferObjects.Tests;
using TestApiWithNet8;
using TestApiNet8.Domain.Entities;

namespace Services.Tests
{
    [ScopedService]
    public class TestsService : ITestsService
    {
        private readonly TestApiNet8Db _testApiNet8Db;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContext;
        public TestsService(TestApiNet8Db testApiNet8Db, IMapper mapper, IHttpContextAccessor httpContext)
        {
            _testApiNet8Db = testApiNet8Db;
            _mapper = mapper;
            _httpContext = httpContext;
        }

        public async Task<TestViewModel> AddAsync(TestCreationDto testCreationDto)
        {
            var entity = _mapper.Map<Test>(testCreationDto);
            var entry = await _testApiNet8Db.Set<Test>().AddAsync(entity);
            await _testApiNet8Db.SaveChangesAsync();
            return _mapper.Map<TestViewModel>(entry.Entity);
        }

        public async Task<List<TestViewModel>> GetAllAsync()
        {
            var entities = await _testApiNet8Db.Set<Test>().ToListAsync();
            return _mapper.Map<List<TestViewModel>>(entities);
        }

        public async Task<ListResult<TestViewModel>> FilterAsync(PaginationOptions filter)
        {
            var httpContext = _httpContext.HttpContext;
            var paginatedResult = await _testApiNet8Db.Set<Test>().ApplyPaginationAsync(filter);
            var Tests = _mapper.Map<List<TestViewModel>>(paginatedResult.paginatedList);
            return new ListResult<TestViewModel>(paginatedResult.paginationMetadata, Tests);
        }

        public async Task<TestViewModel> GetByIdAsync(Guid id)
        {
            var entity = await _testApiNet8Db.Set<Test>().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                throw new InvalidOperationException($"Test with Id {id} not found.");
            return _mapper.Map<TestViewModel>(entity);
        }

        public async Task<TestViewModel> UpdateAsync(Guid id, TestModificationDto testModificationDto)
        {
            var entity = await _testApiNet8Db.Set<Test>().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                throw new InvalidOperationException($"Test with {id} not found.");
            _mapper.Map(testModificationDto, entity);
            var entry = _testApiNet8Db.Set<Test>().Update(entity);
            await _testApiNet8Db.SaveChangesAsync();
            return _mapper.Map<TestViewModel>(entry.Entity);
        }

        public async Task<TestViewModel> DeleteAsync(Guid id)
        {
            var entity = await _testApiNet8Db.Set<Test>().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                throw new InvalidOperationException($"Test with {id} not found.");
            var entry = _testApiNet8Db.Set<Test>().Remove(entity);
            await _testApiNet8Db.SaveChangesAsync();
            return _mapper.Map<TestViewModel>(entry.Entity);
        }
    }

    /// <summary>
    /// AutoMapper mapping profile for Test entity.
    /// </summary>
    public class TestMappingProfile : Profile
    {
        public TestMappingProfile()
        {
            CreateMap<Test, TestViewModel>();
            CreateMap<TestCreationDto, Test>();
            CreateMap<TestModificationDto, Test>();
        }
    }
}