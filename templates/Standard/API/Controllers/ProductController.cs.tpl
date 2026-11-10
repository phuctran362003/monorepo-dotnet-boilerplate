using {{SolutionName}}.Service.DTOs;
using {{SolutionName}}.Service.Interfaces;
using {{SolutionName}}.Service.Utils;
using Microsoft.AspNetCore.Mvc;

namespace {{ApiNamespace}}.Controllers;

[Route("api/products")]
[ApiController]
public class ProductController : ControllerBase
{
    private readonly IProductService _service;

    public ProductController(IProductService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResult<List<ProductDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducts()
    {
        try
        {
            var result = await _service.GetProductsAsync();
            return Ok(ApiResult<List<ProductDto>>.Success(result, "200", "Lấy danh sách sản phẩm thành công."));
        }
        catch (Exception ex)
        {
            var statusCode = ExceptionUtils.ExtractStatusCode(ex);
            var errorResponse = ExceptionUtils.CreateErrorResponse<List<ProductDto>>(ex);
            return StatusCode(statusCode, errorResponse);
        }
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResult<ProductDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProduct(Guid id)
    {
        try
        {
            var result = await _service.GetProductAsync(id);
            return Ok(ApiResult<ProductDto>.Success(result, "200", "Lấy sản phẩm thành công."));
        }
        catch (Exception ex)
        {
            var statusCode = ExceptionUtils.ExtractStatusCode(ex);
            var errorResponse = ExceptionUtils.CreateErrorResponse<ProductDto>(ex);
            return StatusCode(statusCode, errorResponse);
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResult<ProductDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequestDto dto)
    {
        try
        {
            var result = await _service.CreateProductAsync(dto);
            return Ok(ApiResult<ProductDto>.Success(result, "200", "Tạo sản phẩm thành công."));
        }
        catch (Exception ex)
        {
            var statusCode = ExceptionUtils.ExtractStatusCode(ex);
            var errorResponse = ExceptionUtils.CreateErrorResponse<ProductDto>(ex);
            return StatusCode(statusCode, errorResponse);
        }
    }
}
