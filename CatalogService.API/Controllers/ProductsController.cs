using CatalogService.API.DTOs;
using CatalogService.Domain.Entities;
using CatalogService.Domain.Interfaces;
using CatalogService.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.API.Controllers
{
    [ApiController]
    [Route("api/v1/catalog/products")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _repository;
        private readonly IImageValidator _imageValidator;

        public ProductsController(IProductRepository repository, IImageValidator imageValidator)
        {
            _repository = repository;
            _imageValidator = imageValidator;
        }

        // HU-01 — Listar productos
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                // HU-05 — Búsqueda por texto
                var searchResults = await _repository.SearchAsync(search, page, pageSize);
                return Ok(new
                {
                    page,
                    pageSize,
                    search,
                    results = searchResults
                });
            }

            // Listado normal
            var products = await _repository.GetAllAsync(page, pageSize);
            return Ok(new
            {
                page,
                pageSize,
                results = products
            });
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

        // HU-03 — Crear producto (POST)
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
        {
            // Validación básica
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new { message = "El nombre del producto es obligatorio." });

            if (request.Price <= 0)
                return BadRequest(new { message = "El precio debe ser mayor a 0." });

            if (request.Stock < 0)
                return BadRequest(new { message = "El stock no puede ser negativo." });

            // HU-08 — Validar imágenes
            if (request.Images != null && request.Images.Count > 0)
            {
                var (isValid, errorMessage) = _imageValidator.ValidateImageUrls(request.Images);
                if (!isValid)
                    return BadRequest(new { message = errorMessage });
            }

            // Crear objeto Product
            var product = new Product
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                Stock = request.Stock,
                CategoryId = request.CategoryId,
                Images = request.Images,
                Tags = request.Tags,
                CreatedBy = request.CreatedBy,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _repository.CreateAsync(product);

            return CreatedAtAction(
                nameof(GetById),
                new { id = product.Id },
                product
            );
        }

        // HU-04 — Actualizar producto (PUT - actualización completa)
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateProductRequest request)
        {
            var existingProduct = await _repository.GetByIdAsync(id);

            if (existingProduct == null)
                return NotFound(new { message = $"Producto con id '{id}' no encontrado." });

            // Actualizar campos proporcionados
            if (!string.IsNullOrWhiteSpace(request.Name))
                existingProduct.Name = request.Name;

            if (!string.IsNullOrWhiteSpace(request.Description))
                existingProduct.Description = request.Description;

            if (request.Price.HasValue && request.Price > 0)
                existingProduct.Price = request.Price.Value;

            if (request.Stock.HasValue && request.Stock >= 0)
                existingProduct.Stock = request.Stock.Value;

            if (!string.IsNullOrWhiteSpace(request.CategoryId))
                existingProduct.CategoryId = request.CategoryId;

            if (request.Images != null)
                existingProduct.Images = request.Images;

            if (request.Tags != null)
                existingProduct.Tags = request.Tags;

            existingProduct.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(existingProduct);

            return Ok(new { message = "Producto actualizado exitosamente.", product = existingProduct });
        }

        // HU-04 — Actualizar producto (PATCH - actualización parcial)
        [HttpPatch("{id}")]
        public async Task<IActionResult> PartialUpdate(string id, [FromBody] UpdateProductRequest request)
        {
            var existingProduct = await _repository.GetByIdAsync(id);

            if (existingProduct == null)
                return NotFound(new { message = $"Producto con id '{id}' no encontrado." });

            // Solo actualizar campos que fueron proporcionados
            if (!string.IsNullOrWhiteSpace(request.Name))
                existingProduct.Name = request.Name;

            if (!string.IsNullOrWhiteSpace(request.Description))
                existingProduct.Description = request.Description;

            if (request.Price.HasValue && request.Price > 0)
                existingProduct.Price = request.Price.Value;

            if (request.Stock.HasValue && request.Stock >= 0)
                existingProduct.Stock = request.Stock.Value;

            if (!string.IsNullOrWhiteSpace(request.CategoryId))
                existingProduct.CategoryId = request.CategoryId;

            if (request.Images != null)
                existingProduct.Images = request.Images;

            if (request.Tags != null)
                existingProduct.Tags = request.Tags;

            existingProduct.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(existingProduct);

            return Ok(new { message = "Producto actualizado parcialmente.", product = existingProduct });
        }

        // HU-06 — Eliminar producto (DELETE - borrado lógico)
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var existingProduct = await _repository.GetByIdAsync(id);

            if (existingProduct == null)
                return NotFound(new { message = $"Producto con id '{id}' no encontrado." });

            await _repository.DeleteAsync(id);

            return NoContent(); // 204 No Content
        }
    }
}