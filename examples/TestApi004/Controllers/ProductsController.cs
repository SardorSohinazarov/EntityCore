using Microsoft.AspNetCore.Mvc;
using Services.Products;
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
            return Ok(await _productsService.AddAsync(entity));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            return Ok(await _productsService.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            return Ok(await _productsService.GetByIdAsync(id));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, Product entity)
        {
            return Ok(await _productsService.UpdateAsync(id, entity));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            return Ok(await _productsService.DeleteAsync(id));
        }
    }
}