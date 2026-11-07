using AutoMapper;
using {{ApplicationNamespace}}.Products.Dtos;
using {{DomainNamespace}}.Entities;

namespace {{ApplicationNamespace}}.Products;

/// <summary>AutoMapper profile for the sample Product slice.</summary>
public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Product, ProductDto>();
    }
}
