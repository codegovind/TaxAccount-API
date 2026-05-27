using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TaxAccount.Authorization;
using TaxAccount.DTOs;
using TaxAccount.Services;

namespace TaxAccount.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductService productService, ILogger<ProductsController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        [HttpGet]
        [HasPermission("products.view")]
        public async Task<IActionResult> Get()
        {
            var products = await _productService.GetAllAsync();
            return Ok(products);
        }

        [HttpGet("{id}")]
        [HasPermission("products.view")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            return Ok(product);
        }

        [HttpPost]
        [HasPermission("products.create")]
        public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                _logger.LogWarning("Product validation failed: {Errors}", string.Join(", ", errors));
                return BadRequest(new { message = "Validation failed", errors });
            }

            try
            {
                var product = await _productService.CreateAsync(dto);
                _logger.LogInformation("Product created successfully: {ProductId} - {ProductName}", product.Id, product.Name);
                return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return StatusCode(500, new { message = "Error creating product", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [HasPermission("products.edit")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
        {
            await _productService.UpdateAsync(id, dto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [HasPermission("products.delete")]
        public async Task<IActionResult> Delete(int id)
        {
            await _productService.DeleteAsync(id);
            return NoContent();
        }
    }
}