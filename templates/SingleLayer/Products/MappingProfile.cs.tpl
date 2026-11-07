using AutoMapper;
using {{Namespace}}.Models;
using {{Namespace}}.Products.Dtos;

namespace {{Namespace}}.Products;

/// <summary>AutoMapper profile for the sample Product slice.</summary>
public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Product, ProductDto>();
    }
}
