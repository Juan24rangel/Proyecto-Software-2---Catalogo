using CatalogService.Domain.Entities;
using CatalogService.Domain.Interfaces;
using CatalogService.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace CatalogService.Infrastructure.Repositories
{
    /// <summary>
    /// Repositorio MongoDB para Categorías (HU-07)
    /// </summary>
    public class CategoryRepository : ICategoryRepository
    {
        private readonly IMongoCollection<Category> _categories;

        public CategoryRepository(IOptions<MongoDbSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            var database = client.GetDatabase(settings.Value.DatabaseName);
            _categories = database.GetCollection<Category>(
                settings.Value.CategoriesCollectionName ?? "categories"
            );

            // Crear índices
            var slugIndex = Builders<Category>.IndexKeys.Ascending(c => c.Slug);
            _categories.Indexes.CreateOne(
                new CreateIndexModel<Category>(slugIndex, new CreateIndexOptions { Unique = true })
            );

            var parentIdIndex = Builders<Category>.IndexKeys.Ascending(c => c.ParentId);
            _categories.Indexes.CreateOne(
                new CreateIndexModel<Category>(parentIdIndex)
            );
        }

        public async Task<Category?> GetByIdAsync(string id)
        {
            return await _categories
                .Find(c => c.Id == id && c.IsActive)
                .FirstOrDefaultAsync();
        }

        public async Task<Category?> GetBySlugAsync(string slug)
        {
            return await _categories
                .Find(c => c.Slug == slug && c.IsActive)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Category>> GetAllAsync(int page = 1, int pageSize = 10)
        {
            return await _categories
                .Find(c => c.IsActive)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetRootCategoriesAsync()
        {
            return await _categories
                .Find(c => c.IsActive && c.ParentId == null)
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetSubCategoriesAsync(string parentId)
        {
            return await _categories
                .Find(c => c.IsActive && c.ParentId == parentId)
                .ToListAsync();
        }

        public async Task CreateAsync(Category category)
        {
            category.CreatedAt = DateTime.UtcNow;
            category.UpdatedAt = DateTime.UtcNow;
            await _categories.InsertOneAsync(category);
        }

        public async Task UpdateAsync(Category category)
        {
            category.UpdatedAt = DateTime.UtcNow;
            await _categories.ReplaceOneAsync(c => c.Id == category.Id, category);
        }

        public async Task DeleteAsync(string id)
        {
            // Borrado lógico
            var update = Builders<Category>.Update
                .Set(c => c.IsActive, false)
                .Set(c => c.UpdatedAt, DateTime.UtcNow);
            await _categories.UpdateOneAsync(c => c.Id == id, update);
        }

        public async Task<bool> SlugExistsAsync(string slug, string? excludeId = null)
        {
            var filter = Builders<Category>.Filter.Eq(c => c.Slug, slug) & 
                         Builders<Category>.Filter.Eq(c => c.IsActive, true);

            if (!string.IsNullOrWhiteSpace(excludeId))
            {
                filter = filter & Builders<Category>.Filter.Ne(c => c.Id, excludeId);
            }

            var count = await _categories.CountDocumentsAsync(filter);
            return count > 0;
        }
    }
}
