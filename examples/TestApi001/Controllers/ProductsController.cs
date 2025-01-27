using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Services;
using TestApi001.Entities;

namespace Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductsService _productsService;
        public ProductsController(IProductsService iProductsService)
        {
            _productsService = iProductsService;
        }

        [HttpPost]
        public async Task<IActionResult> AddAsync(Product entity)
        {
            return await _productsService.AddAsync(entity);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            return await _productsService.GetAllAsync();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            return await _productsService.GetByIdAsync(id);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, Product entity)
        {
            return await _productsService.UpdateAsync(id, entity);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            return await _productsService.DeleteAsync(id);
        }
    }
}