using MediatR;
using {{Namespace}}.Products.Dtos;

namespace {{Namespace}}.Products.Queries;

public sealed record GetProductByIdQuery(Guid Id) : IRequest<ProductDto?>;
