using MediatR;
using {{Namespace}}.Application.Products.Dtos;

namespace {{Namespace}}.Application.Products.Queries;

public sealed class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    private readonly IProductService _productService;

    public GetProductByIdQueryHandler(IProductService productService) => _productService = productService;

    public Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken) =>
        _productService.GetByIdAsync(request.Id, cancellationToken);
}
