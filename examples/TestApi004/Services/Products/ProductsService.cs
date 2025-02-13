using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TestApi004.DataTransferObjects;
using TestApi001;
using TestApi001.Entities;

namespace Services.Products
{
    public class ProductsService : IProductsService
    {
        private readonly TestApiDbContext _testApiDbContext;
        private readonly IMapper _mapper;
        public ProductsService(TestApiDbContext testApiDbContext, IMapper mapper)
        {
            _testApiDbContext = testApiDbContext;
            _mapper = mapper;
        }

        public async Task<ProductViewModel> AddAsync(ProductCreationDto productCreationDto)
        {
            var entity = _mapper.Map<Product>(productCreationDto);
            var entry = await _testApiDbContext.Set<Product>().AddAsync(entity);
            await _testApiDbContext.SaveChangesAsync();
            return _mapper.Map<ProductViewModel>(entry.Entity);
        }

        public async Task<List<ProductViewModel>> GetAllAsync()
        {
            var entities = await _testApiDbContext.Set<Product>().ToListAsync();
            return _mapper.Map<List<ProductViewModel>>(entities);
        }

        public async Task<ProductViewModel> GetByIdAsync(int id)
        {
            var entity = await _testApiDbContext.Set<Product>().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                throw new InvalidOperationException($"Product with Id {id} not found.");
            return _mapper.Map<ProductViewModel>(entity);
        }

        public async Task<ProductViewModel> UpdateAsync(int id, ProductModificationDto productModificationDto)
        {
            var entity = await _testApiDbContext.Set<Product>().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                throw new InvalidOperationException($"Product with {id} not found.");
            _mapper.Map(productModificationDto, entity);
            var entry = _testApiDbContext.Set<Product>().Update(entity);
            await _testApiDbContext.SaveChangesAsync();
            return _mapper.Map<ProductViewModel>(entry.Entity);
        }

        public async Task<ProductViewModel> DeleteAsync(int id)
        {
            var entity = await _testApiDbContext.Set<Product>().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                throw new InvalidOperationException($"Product with {id} not found.");
            var entry = _testApiDbContext.Set<Product>().Remove(entity);
            await _testApiDbContext.SaveChangesAsync();
            return _mapper.Map<ProductViewModel>(entry.Entity);
        }
    }

    /// <summary>
    /// AutoMapper mapping profile for Product entity.
    /// </summary>
    public class ProductMappingProfile : Profile
    {
        public ProductMappingProfile()
        {
            CreateMap<Product, ProductViewModel>();
            CreateMap<ProductCreationDto, Product>();
            CreateMap<ProductModificationDto, Product>();
        }
    }
}