namespace CatalogService.API.DTOs
{
    /// <summary>
    /// Request DTO para crear una categoría (HU-07)
    /// </summary>
    public class CreateCategoryRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string? ParentId { get; set; } // Para subcategorías
    }
}
