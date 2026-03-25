using CatalogService.Domain.Entities;
using CatalogService.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.API.Controllers
{
    [ApiController]
    [Route("api/v1/catalog/products")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _repository;

        public ProductsController(IProductRepository repository)
        {
            _repository = repository;
        }

        // HU-02 — Ver detalle de un producto
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var product = await _repository.GetByIdAsync(id);

            if (product == null)
                return NotFound(new { message = $"Producto con id '{id}' no encontrado." });

            return Ok(product);
        }

        // HU-01 — Listar productos
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var products = await _repository.GetAllAsync(page, pageSize);
            return Ok(products);
        }
    }
}