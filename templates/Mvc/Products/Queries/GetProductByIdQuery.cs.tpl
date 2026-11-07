using MediatR;
using {{Namespace}}.ViewModels;

namespace {{Namespace}}.Products.Queries;

public sealed record GetProductByIdQuery(Guid Id) : IRequest<ProductViewModel?>;
