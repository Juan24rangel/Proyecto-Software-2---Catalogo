using CatalogService.API.GrpcServices;
using CatalogService.Domain.Interfaces;
using CatalogService.Infrastructure.Repositories;
using CatalogService.Infrastructure.Services;
using CatalogService.Infrastructure.Settings;

var builder = WebApplication.CreateBuilder(args);

// MongoDB
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings")
);
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

// Services
builder.Services.AddScoped<IImageValidator, ImageValidator>();

// gRPC (HU-09)
builder.Services.AddGrpc();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// gRPC Mapping
app.MapGrpcService<CatalogGrpcService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a gRPC client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.UseAuthorization();
app.MapControllers();
app.Run();