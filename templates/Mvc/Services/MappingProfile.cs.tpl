using AutoMapper;
using {{Namespace}}.Models;
using {{Namespace}}.ViewModels;

namespace {{Namespace}}.Services;

/// <summary>AutoMapper profile for the sample Product slice.</summary>
public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Product, ProductViewModel>();
    }
}
