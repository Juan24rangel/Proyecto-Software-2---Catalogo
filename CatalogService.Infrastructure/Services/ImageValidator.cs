using System.Text.RegularExpressions;

namespace CatalogService.Infrastructure.Services
{
    /// <summary>
    /// Implementación del validador de imágenes (HU-08)
    /// </summary>
    public class ImageValidator : IImageValidator
    {
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg" };

        public bool ValidateImageUrl(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                return false;

            // Validar que sea una URL válida
            if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri))
                return false;

            // Validar que sea http o https
            if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                return false;

            // Validar extensión de archivo
            var extension = Path.GetExtension(uri.LocalPath).ToLower();
            if (!_allowedExtensions.Contains(extension))
                return false;

            return true;
        }

        public (bool isValid, string? errorMessage) ValidateImageUrls(
            List<string> imageUrls,
            int minImages = 1,
            int maxImages = 5)
        {
            if (imageUrls == null || imageUrls.Count == 0)
                return (false, $"Se requiere al menos {minImages} imagen(es).");

            if (imageUrls.Count < minImages)
                return (false, $"Se requieren al menos {minImages} imagen(es).");

            if (imageUrls.Count > maxImages)
                return (false, $"No puede haber más de {maxImages} imágenes.");

            // Validar cada URL
            foreach (var url in imageUrls)
            {
                if (!ValidateImageUrl(url))
                    return (false, $"La URL '{url}' no es válida o tiene extensión no permitida.");
            }

            return (true, null);
        }
    }
}
