using MediatR;
using {{ApplicationNamespace}}.Products.Dtos;

namespace {{ApplicationNamespace}}.Products.Queries;

public sealed record GetProductsQuery : IRequest<IReadOnlyList<ProductDto>>;
