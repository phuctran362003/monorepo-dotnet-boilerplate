using MediatR;
using {{Namespace}}.Application.Products.Dtos;

namespace {{Namespace}}.Application.Products.Queries;

public sealed record GetProductsQuery : IRequest<IReadOnlyList<ProductDto>>;
