using MediatR;
using {{Namespace}}.ViewModels;

namespace {{Namespace}}.Products.Queries;

public sealed record GetProductsQuery : IRequest<IReadOnlyList<ProductViewModel>>;
