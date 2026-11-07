using MediatR;
using {{Namespace}}.ViewModels;

namespace {{Namespace}}.Products.Commands;

public sealed record CreateProductCommand(string Name, decimal Price) : IRequest<ProductViewModel>;
