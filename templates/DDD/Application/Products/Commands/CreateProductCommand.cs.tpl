using MediatR;
using {{ApplicationNamespace}}.Products.Dtos;

namespace {{ApplicationNamespace}}.Products.Commands;

public sealed record CreateProductCommand(string Name, decimal Price) : IRequest<ProductDto>;
