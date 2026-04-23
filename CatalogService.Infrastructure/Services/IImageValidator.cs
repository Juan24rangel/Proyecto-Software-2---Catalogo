namespace CatalogService.Infrastructure.Services
{
    /// <summary>
    /// Interfaz para validación de URLs de imágenes (HU-08)
    /// </summary>
    public interface IImageValidator
    {
        /// <summary>
        /// Valida una URL de imagen
        /// </summary>
        /// <param name="imageUrl">URL a validar</param>
        /// <returns>true si es válida, false si no</returns>
        bool ValidateImageUrl(string imageUrl);

        /// <summary>
        /// Valida una lista de URLs de imágenes
        /// </summary>
        /// <param name="imageUrls">Lista de URLs</param>
        /// <param name="minImages">Mínimo de imágenes (default 1)</param>
        /// <param name="maxImages">Máximo de imágenes (default 5)</param>
        /// <returns>Tupla (isValid, errorMessage)</returns>
        (bool isValid, string? errorMessage) ValidateImageUrls(
            List<string> imageUrls,
            int minImages = 1,
            int maxImages = 5
        );
    }
}
