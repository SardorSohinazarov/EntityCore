using Microsoft.AspNetCore.Mvc;
using Services.Products;
using TestApi004.DataTransferObjects;
using TestApi001.Entities;

namespace Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductsService _productsService;
        public ProductsController(IProductsService productsService)
        {
            _productsService = productsService;
        }

        [HttpPost]
        public async Task<IActionResult> AddAsync(ProductCreationDto productCreationDto)
        {
            return Ok(await _productsService.AddAsync(productCreationDto));
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
        public async Task<IActionResult> UpdateAsync(int id, ProductModificationDto productModificationDto)
        {
            return Ok(await _productsService.UpdateAsync(id, productModificationDto));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            return Ok(await _productsService.DeleteAsync(id));
        }
    }
}