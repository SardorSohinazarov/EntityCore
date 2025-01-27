using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TestApi002;
using TestApi002.Entities;

namespace Services
{
    public class ProductsService : IProductsService
    {
        private readonly AppDbContext _appDbContext;
        public ProductsService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<Product> AddAsync(Product entity)
        {
            var entry = await _appDbContext.Set<Product>().AddAsync(entity);
            await _appDbContext.SaveChangesAsync();
            return entry.Entity;
        }

        public async Task<List<Product>> GetAllAsync()
        {
            return await _appDbContext.Set<Product>().ToListAsync();
        }

        public async Task<Product> GetByIdAsync(int id)
        {
            var entity = await _appDbContext.Set<Product>().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                throw new InvalidOperationException($"Product with Id {id} not found.");
            return entity;
        }

        public async Task<Product> UpdateAsync(int id, Product entity)
        {
            var entry = _appDbContext.Set<Product>().Update(entity);
            await _appDbContext.SaveChangesAsync();
            return entry.Entity;
        }

        public async Task<Product> DeleteAsync(int id)
        {
            var entity = await _appDbContext.Set<Product>().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                throw new InvalidOperationException($"Product with {id} not found.");
            var entry = _appDbContext.Set<Product>().Remove(entity);
            await _appDbContext.SaveChangesAsync();
            return entry.Entity;
        }
    }
}