using MediatR;
using {{ApplicationNamespace}}.Products.Dtos;

namespace {{ApplicationNamespace}}.Products.Queries;

public sealed class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    private readonly IProductService _productService;

    public GetProductByIdQueryHandler(IProductService productService) => _productService = productService;

    public Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken) =>
        _productService.GetByIdAsync(request.Id, cancellationToken);
}
