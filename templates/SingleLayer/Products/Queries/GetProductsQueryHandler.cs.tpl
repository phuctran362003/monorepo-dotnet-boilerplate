using MediatR;
using {{Namespace}}.Products.Dtos;

namespace {{Namespace}}.Products.Queries;

public sealed class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, IReadOnlyList<ProductDto>>
{
    private readonly IProductService _productService;

    public GetProductsQueryHandler(IProductService productService) => _productService = productService;

    public Task<IReadOnlyList<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken) =>
        _productService.GetAllAsync(cancellationToken);
}
