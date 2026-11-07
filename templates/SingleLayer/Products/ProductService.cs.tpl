{{#if Redis}}using {{Namespace}}.Caching;
{{/if}}using {{Namespace}}.Common;
using {{Namespace}}.Data;
using {{Namespace}}.Models;
using {{Namespace}}.Products.Dtos;
{{#if AutoMapper}}using AutoMapper;
{{/if}}
namespace {{Namespace}}.Products;

public sealed class ProductService : IProductService
{
    private const string ProductsCacheKey = "products:all";

    private readonly IProductRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
{{#if AutoMapper}}    private readonly IMapper _mapper;
{{/if}}{{#if Redis}}    private readonly ICacheService _cache;
{{/if}}
    public ProductService(
        IProductRepository repository,
        IUnitOfWork unitOfWork{{#if AutoMapper}},
        IMapper mapper{{/if}}{{#if Redis}},
        ICacheService cache{{/if}})
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
{{#if AutoMapper}}        _mapper = mapper;
{{/if}}{{#if Redis}}        _cache = cache;
{{/if}}    }

    public async Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
{{#if Redis}}        var cached = await _cache.GetAsync<IReadOnlyList<ProductDto>>(ProductsCacheKey, cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

{{/if}}        var products = await _repository.GetAllAsync(cancellationToken);
        var result = products.Select(Map).ToList();

{{#if Redis}}        await _cache.SetAsync(ProductsCacheKey, result, TimeSpan.FromMinutes(5), cancellationToken);
{{/if}}
        return result;
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _repository.GetByIdAsync(id, cancellationToken);
        return product is null ? null : Map(product);
    }

    public async Task<ProductDto> CreateAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var product = Product.Create(request.Name, request.Price);

        await _repository.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
{{#if Redis}}
        await _cache.RemoveAsync(ProductsCacheKey, cancellationToken);
{{/if}}
        return Map(product);
    }

    private ProductDto Map(Product product) =>
{{#if AutoMapper}}        _mapper.Map<ProductDto>(product);
{{/if}}{{#if !AutoMapper}}        ProductDto.FromEntity(product);
{{/if}}}
