using CatalogService.Domain.Entities;

namespace CatalogService.Domain.Interfaces
{
    /// <summary>
    /// Contrato para el repositorio de Categorías (HU-07)
    /// </summary>
    public interface ICategoryRepository
    {
        Task<Category?> GetByIdAsync(string id);
        Task<Category?> GetBySlugAsync(string slug);
        Task<IEnumerable<Category>> GetAllAsync(int page = 1, int pageSize = 10);
        Task<IEnumerable<Category>> GetRootCategoriesAsync();
        Task<IEnumerable<Category>> GetSubCategoriesAsync(string parentId);
        Task CreateAsync(Category category);
        Task UpdateAsync(Category category);
        Task DeleteAsync(string id);
        Task<bool> SlugExistsAsync(string slug, string? excludeId = null);
    }
}
