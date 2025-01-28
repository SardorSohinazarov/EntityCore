using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TestApi001;
using TestApi001.Entities;

namespace Services
{
    public class ProductsService : IProductsService
    {
        private readonly TestApiDbContext _testApiDbContext;
        public ProductsService(TestApiDbContext testApiDbContext)
        {
            _testApiDbContext = testApiDbContext;
        }

        public async Task<Product> AddAsync(Product entity)
        {
            var entry = await _testApiDbContext.Set<Product>().AddAsync(entity);
            await _testApiDbContext.SaveChangesAsync();
            return entry.Entity;
        }

        public async Task<List<Product>> GetAllAsync()
        {
            return await _testApiDbContext.Set<Product>().ToListAsync();
        }

        public async Task<Product> GetByIdAsync(int id)
        {
            var entity = await _testApiDbContext.Set<Product>().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                throw new InvalidOperationException($"Product with Id {id} not found.");
            return entity;
        }

        public async Task<Product> UpdateAsync(int id, Product entity)
        {
            var entry = _testApiDbContext.Set<Product>().Update(entity);
            await _testApiDbContext.SaveChangesAsync();
            return entry.Entity;
        }

        public async Task<Product> DeleteAsync(int id)
        {
            var entity = await _testApiDbContext.Set<Product>().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                throw new InvalidOperationException($"Product with {id} not found.");
            var entry = _testApiDbContext.Set<Product>().Remove(entity);
            await _testApiDbContext.SaveChangesAsync();
            return entry.Entity;
        }
    }
}