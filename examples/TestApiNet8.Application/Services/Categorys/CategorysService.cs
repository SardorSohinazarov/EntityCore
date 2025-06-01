using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Common.Paginations.Models;
using Common.Paginations.Extensions;
using Common.ServiceAttribute;
using Common;
using DataTransferObjects.Categorys;
using TestApiWithNet8;
using TestApiNet8.Domain.Entities;

namespace Services.Categorys
{
    [ScopedService]
    public class CategorysService : ICategorysService
    {
        private readonly TestApiNet8Db _testApiNet8Db;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContext;
        public CategorysService(TestApiNet8Db testApiNet8Db, IMapper mapper, IHttpContextAccessor httpContext)
        {
            _testApiNet8Db = testApiNet8Db;
            _mapper = mapper;
            _httpContext = httpContext;
        }

        public async Task<CategoryViewModel> AddAsync(CategoryCreationDto categoryCreationDto)
        {
            var entity = _mapper.Map<Category>(categoryCreationDto);
            entity.ChildCategories = await _testApiNet8Db.Set<Category>().Where(x => categoryCreationDto.ChildCategoriesIds.Contains(x.Id)).ToListAsync();
            entity.Products = await _testApiNet8Db.Set<Product>().Where(x => categoryCreationDto.ProductsIds.Contains(x.Id)).ToListAsync();
            var entry = await _testApiNet8Db.Set<Category>().AddAsync(entity);
            await _testApiNet8Db.SaveChangesAsync();
            return _mapper.Map<CategoryViewModel>(entry.Entity);
        }

        public async Task<List<CategoryViewModel>> GetAllAsync()
        {
            var entities = await _testApiNet8Db.Set<Category>().ToListAsync();
            return _mapper.Map<List<CategoryViewModel>>(entities);
        }

        public async Task<ListResult<CategoryViewModel>> FilterAsync(PaginationOptions filter)
        {
            var httpContext = _httpContext.HttpContext;
            var paginatedResult = await _testApiNet8Db.Set<Category>().ApplyPaginationAsync(filter);
            var Categorys = _mapper.Map<List<CategoryViewModel>>(paginatedResult.paginatedList);
            return new ListResult<CategoryViewModel>(paginatedResult.paginationMetadata, Categorys);
        }

        public async Task<CategoryViewModel> GetByIdAsync(int id)
        {
            var entity = await _testApiNet8Db.Set<Category>().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                throw new InvalidOperationException($"Category with Id {id} not found.");
            return _mapper.Map<CategoryViewModel>(entity);
        }

        public async Task<CategoryViewModel> UpdateAsync(int id, CategoryModificationDto categoryModificationDto)
        {
            var entity = await _testApiNet8Db.Set<Category>().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                throw new InvalidOperationException($"Category with {id} not found.");
            _mapper.Map(categoryModificationDto, entity);
            entity.ChildCategories = await _testApiNet8Db.Set<Category>().Where(x => categoryModificationDto.ChildCategoriesIds.Contains(x.Id)).ToListAsync();
            entity.Products = await _testApiNet8Db.Set<Product>().Where(x => categoryModificationDto.ProductsIds.Contains(x.Id)).ToListAsync();
            var entry = _testApiNet8Db.Set<Category>().Update(entity);
            await _testApiNet8Db.SaveChangesAsync();
            return _mapper.Map<CategoryViewModel>(entry.Entity);
        }

        public async Task<CategoryViewModel> DeleteAsync(int id)
        {
            var entity = await _testApiNet8Db.Set<Category>().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                throw new InvalidOperationException($"Category with {id} not found.");
            var entry = _testApiNet8Db.Set<Category>().Remove(entity);
            await _testApiNet8Db.SaveChangesAsync();
            return _mapper.Map<CategoryViewModel>(entry.Entity);
        }
    }

    /// <summary>
    /// AutoMapper mapping profile for Category entity.
    /// </summary>
    public class CategoryMappingProfile : Profile
    {
        public CategoryMappingProfile()
        {
            CreateMap<Category, CategoryViewModel>();
            CreateMap<CategoryCreationDto, Category>();
            CreateMap<CategoryModificationDto, Category>();
        }
    }
}