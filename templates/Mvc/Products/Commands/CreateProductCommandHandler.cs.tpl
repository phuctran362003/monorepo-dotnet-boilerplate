using MediatR;
using {{Namespace}}.Services;
using {{Namespace}}.ViewModels;

namespace {{Namespace}}.Products.Commands;

public sealed class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductViewModel>
{
    private readonly IProductService _productService;

    public CreateProductCommandHandler(IProductService productService) => _productService = productService;

    public Task<ProductViewModel> Handle(CreateProductCommand request, CancellationToken cancellationToken) =>
        _productService.CreateAsync(
            new CreateProductViewModel { Name = request.Name, Price = request.Price },
            cancellationToken);
}
