using {{DomainNamespace}}.Entities;
using {{SolutionName}}.Service.DTOs;
using {{SolutionName}}.Service.Interfaces;
using {{SolutionName}}.Service.Mappers;
using {{SolutionName}}.Service.Utils;

namespace {{SolutionName}}.Service.Services;

public class ProductService : IProductService
{
    private readonly ILoggerService _logger;
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(ILoggerService logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<ProductDto>> GetProductsAsync()
    {
        var products = await _unitOfWork.Products.GetAllAsync();
        return products.Select(ProductMapper.ToProductDto).ToList();
    }

    public async Task<ProductDto> GetProductAsync(Guid id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product is null)
        {
            throw ErrorHelper.NotFound(ErrorMessages.ProductNotFound);
        }

        return ProductMapper.ToProductDto(product);
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw ErrorHelper.BadRequest(ErrorMessages.ProductNameRequired);
        }

        var product = new Product
        {
            Name = dto.Name.Trim(),
            Price = dto.Price,
            Description = dto.Description
        };

        await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();
        _logger.Success($"Created product {product.Id}.");
        return ProductMapper.ToProductDto(product);
    }
}
