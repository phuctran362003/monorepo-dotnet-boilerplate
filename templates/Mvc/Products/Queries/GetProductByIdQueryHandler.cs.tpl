using MediatR;
using {{Namespace}}.Services;
using {{Namespace}}.ViewModels;

namespace {{Namespace}}.Products.Queries;

public sealed class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductViewModel?>
{
    private readonly IProductService _productService;

    public GetProductByIdQueryHandler(IProductService productService) => _productService = productService;

    public Task<ProductViewModel?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken) =>
        _productService.GetByIdAsync(request.Id, cancellationToken);
}
