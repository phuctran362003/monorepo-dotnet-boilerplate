using MediatR;
using {{ApplicationNamespace}}.Products.Dtos;

namespace {{ApplicationNamespace}}.Products.Commands;

public sealed class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IProductService _productService;

    public CreateProductCommandHandler(IProductService productService) => _productService = productService;

    public Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken) =>
        _productService.CreateAsync(new CreateProductRequest(request.Name, request.Price), cancellationToken);
}
