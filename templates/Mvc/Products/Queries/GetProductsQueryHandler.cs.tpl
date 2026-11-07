using MediatR;
using {{Namespace}}.Services;
using {{Namespace}}.ViewModels;

namespace {{Namespace}}.Products.Queries;

public sealed class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, IReadOnlyList<ProductViewModel>>
{
    private readonly IProductService _productService;

    public GetProductsQueryHandler(IProductService productService) => _productService = productService;

    public Task<IReadOnlyList<ProductViewModel>> Handle(
        GetProductsQuery request,
        CancellationToken cancellationToken) =>
        _productService.GetAllAsync(cancellationToken);
}
