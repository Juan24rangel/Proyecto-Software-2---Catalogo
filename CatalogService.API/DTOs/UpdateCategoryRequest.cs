namespace CatalogService.API.DTOs
{
    /// <summary>
    /// Request DTO para actualizar una categoría (HU-07)
    /// </summary>
    public class UpdateCategoryRequest
    {
        public string? Name { get; set; }
        public string? Slug { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? ParentId { get; set; }
    }
}
