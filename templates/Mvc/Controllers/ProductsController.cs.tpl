{{#if MediatR}}using MediatR;
using {{Namespace}}.Products.Commands;
using {{Namespace}}.Products.Queries;
{{/if}}{{#if !MediatR}}using {{Namespace}}.Services;
{{/if}}using {{Namespace}}.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace {{Namespace}}.Controllers;

public sealed class ProductsController : Controller
{
{{#if MediatR}}    private readonly ISender _sender;

    public ProductsController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken) =>
        View(await _sender.Send(new GetProductsQuery(), cancellationToken));

    [HttpGet]
    public IActionResult Create() => View(new CreateProductViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreateProductViewModel request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(request);
        }

        var product = await _sender.Send(
            new CreateProductCommand(request.Name, request.Price),
            cancellationToken);

        return RedirectToAction(nameof(Details), new { id = product.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var product = await _sender.Send(new GetProductByIdQuery(id), cancellationToken);
        return product is null ? NotFound() : View(product);
    }
{{/if}}{{#if !MediatR}}    private readonly IProductService _productService;

    public ProductsController(IProductService productService) => _productService = productService;

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken) =>
        View(await _productService.GetAllAsync(cancellationToken));

    [HttpGet]
    public IActionResult Create() => View(new CreateProductViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreateProductViewModel request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(request);
        }

        var product = await _productService.CreateAsync(request, cancellationToken);
        return RedirectToAction(nameof(Details), new { id = product.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var product = await _productService.GetByIdAsync(id, cancellationToken);
        return product is null ? NotFound() : View(product);
    }
{{/if}}}
