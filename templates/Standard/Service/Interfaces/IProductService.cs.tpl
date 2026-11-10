using {{SolutionName}}.Service.DTOs;

namespace {{SolutionName}}.Service.Interfaces;

public interface IProductService
{
    Task<List<ProductDto>> GetProductsAsync();

    Task<ProductDto> GetProductAsync(Guid id);

    Task<ProductDto> CreateProductAsync(CreateProductRequestDto dto);
}
