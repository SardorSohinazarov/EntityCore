using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TestApi001.Entities;

namespace Services
{
    public interface IProductsService
    {
        Task<Product> AddAsync(Product entity);
        Task<List<Product>> GetAllAsync();
        Task<Product> GetByIdAsync(int id);
        Task<Product> UpdateAsync(int id, Product entity);
        Task<Product> DeleteAsync(int id);
    }
}