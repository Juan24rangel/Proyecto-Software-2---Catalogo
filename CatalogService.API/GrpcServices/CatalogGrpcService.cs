using CatalogService.API.Protos;
using CatalogService.Domain.Interfaces;
using Grpc.Core;

namespace CatalogService.API.GrpcServices
{
    /// <summary>
    /// Servicio gRPC para el Microservicio de Catálogo (HU-09)
    /// Permite consultas de bajo latency desde otros microservicios (ej: Carrito)
    /// </summary>
    public class CatalogGrpcService : Catalog.CatalogBase
    {
        private readonly IProductRepository _productRepository;
        private readonly ILogger<CatalogGrpcService> _logger;

        public CatalogGrpcService(
            IProductRepository productRepository,
            ILogger<CatalogGrpcService> logger)
        {
            _productRepository = productRepository;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene un producto por ID
        /// Responde en <100ms (RNF-09)
        /// </summary>
        public override async Task<ProductResponse> GetProduct(
            GetProductRequest request,
            ServerCallContext context)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.ProductId))
                    throw new RpcException(
                        new Status(StatusCode.InvalidArgument, "ProductId es requerido"));

                var product = await _productRepository.GetByIdAsync(request.ProductId);

                if (product == null)
                    throw new RpcException(
                        new Status(StatusCode.NotFound, $"Producto {request.ProductId} no encontrado"));

                return MapToGrpcProduct(product);
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo producto {ProductId}", request.ProductId);
                throw new RpcException(
                    new Status(StatusCode.Internal, "Error interno del servidor"));
            }
        }

        /// <summary>
        /// Obtiene múltiples productos con paginación
        /// </summary>
        public override async Task<GetProductsResponse> GetProducts(
            GetProductsRequest request,
            ServerCallContext context)
        {
            try
            {
                var page = request.Page > 0 ? request.Page : 1;
                var pageSize = request.PageSize > 0 ? request.PageSize : 10;

                IEnumerable<Domain.Entities.Product> productsEnumerable;

                if (!string.IsNullOrWhiteSpace(request.Search))
                {
                    productsEnumerable = await _productRepository.SearchAsync(
                        request.Search, page, pageSize);
                }
                else
                {
                    productsEnumerable = await _productRepository.GetAllAsync(page, pageSize);
                }

                var products = productsEnumerable.ToList();

                var response = new GetProductsResponse
                {
                    Page = page,
                    PageSize = pageSize,
                    Total = products.Count
                };

                foreach (var product in products)
                {
                    response.Products.Add(MapToGrpcProduct(product));
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo productos");
                throw new RpcException(
                    new Status(StatusCode.Internal, "Error interno del servidor"));
            }
        }

        /// <summary>
        /// Obtiene múltiples productos por lista de IDs
        /// Optimizado para llamadas desde Carrito
        /// </summary>
        public override async Task<GetProductsResponse> GetProductsByIds(
            GetProductsByIdsRequest request,
            ServerCallContext context)
        {
            try
            {
                if (request.ProductIds.Count == 0)
                    throw new RpcException(
                        new Status(StatusCode.InvalidArgument, "ProductIds es requerido"));

                var response = new GetProductsResponse
                {
                    Page = 1,
                    PageSize = request.ProductIds.Count
                };

                foreach (var productId in request.ProductIds)
                {
                    var product = await _productRepository.GetByIdAsync(productId);
                    if (product != null)
                    {
                        response.Products.Add(MapToGrpcProduct(product));
                    }
                }

                response.Total = response.Products.Count;
                return response;
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo productos por IDs");
                throw new RpcException(
                    new Status(StatusCode.Internal, "Error interno del servidor"));
            }
        }

        /// <summary>
        /// Mapea un objeto Product de Domain a ProductResponse para gRPC
        /// </summary>
        private ProductResponse MapToGrpcProduct(Domain.Entities.Product product)
        {
            var response = new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = (double)product.Price,
                Stock = product.Stock,
                CategoryId = product.CategoryId,
                IsActive = product.IsActive,
                CreatedAt = product.CreatedAt.ToUniversalTime().ToString("O"),
                UpdatedAt = product.UpdatedAt.ToUniversalTime().ToString("O"),
                CreatedBy = product.CreatedBy
            };

            if (product.Images != null && product.Images.Count > 0)
            {
                foreach (var image in product.Images)
                {
                    if (!string.IsNullOrWhiteSpace(image))
                        response.Images.Add(image);
                }
            }

            if (product.Tags != null && product.Tags.Count > 0)
            {
                foreach (var tag in product.Tags)
                {
                    if (!string.IsNullOrWhiteSpace(tag))
                        response.Tags.Add(tag);
                }
            }

            return response;
        }
    }
}
