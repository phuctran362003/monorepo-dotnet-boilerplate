using MediatR;
using {{Namespace}}.Products.Dtos;

namespace {{Namespace}}.Products.Commands;

public sealed record CreateProductCommand(string Name, decimal Price) : IRequest<ProductDto>;
