using AutoMapper;
using {{Namespace}}.Application.Products.Dtos;
using {{Namespace}}.Domain.Entities;

namespace {{Namespace}}.Application.Products;

/// <summary>
/// AutoMapper profile for the sample Product slice. Add one mapping per DTO
/// you expose; profiles are discovered from this assembly automatically.
/// </summary>
public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Product, ProductDto>()
            .ForCtorParam(nameof(ProductDto.Name), options => options.MapFrom(product => product.Name.Value))
            .ForCtorParam(nameof(ProductDto.Status), options => options.MapFrom(product => product.Status.ToString()));
    }
}
