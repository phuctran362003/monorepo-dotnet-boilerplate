using {{DomainNamespace}}.Entities;
using {{SolutionName}}.Service.DTOs;

namespace {{SolutionName}}.Service.Mappers;

public static class ProductMapper
{
    public static ProductDto ToProductDto(Product entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Price = entity.Price,
        Description = entity.Description,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };
}
