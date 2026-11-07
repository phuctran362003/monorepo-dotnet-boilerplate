using MediatR;
using {{Namespace}}.Application.Products.Dtos;

namespace {{Namespace}}.Application.Products.Commands;

public sealed record CreateProductCommand(string Name, decimal Price) : IRequest<ProductDto>;
