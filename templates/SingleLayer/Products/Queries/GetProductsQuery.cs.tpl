using MediatR;
using {{Namespace}}.Products.Dtos;

namespace {{Namespace}}.Products.Queries;

public sealed record GetProductsQuery : IRequest<IReadOnlyList<ProductDto>>;
