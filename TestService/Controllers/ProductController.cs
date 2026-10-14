using Microsoft.AspNetCore.Mvc;
using TestService.Application.Common;
using TestService.Application.Dtos;
using TestService.Application.Interfaces;

namespace TestService.Web.Controllers;

/// <summary>商品管理 Controller。</summary>
[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ICurrentUser _currentUser;

    public ProductController(IProductService productService, ICurrentUser currentUser)
    {
        _productService = productService;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ApiResponse<PagedResult<ProductDto>>> Query([FromQuery] ProductQuery query, CancellationToken cancellationToken)
    {
        var data = await _productService.QueryAsync(query, cancellationToken);
        return ApiResponse<PagedResult<ProductDto>>.Ok(data);
    }

    [HttpGet("{id:guid}")]
    public async Task<ApiResponse<ProductDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var data = await _productService.GetByIdAsync(id, cancellationToken);
        return data is null
            ? ApiResponse<ProductDto>.Fail("商品不存在", 404)
            : ApiResponse<ProductDto>.Ok(data);
    }

    [HttpPost]
    public async Task<ApiResponse<Guid>> Create([FromBody] ProductCreateRequest request, CancellationToken cancellationToken)
    {
        var id = await _productService.CreateAsync(request, _currentUser.Username ?? "system", cancellationToken);
        return ApiResponse<Guid>.Ok(id, "创建成功");
    }

    [HttpPut("{id:guid}")]
    public async Task<ApiResponse> Update(Guid id, [FromBody] ProductUpdateRequest request, CancellationToken cancellationToken)
    {
        await _productService.UpdateAsync(id, request, _currentUser.Username ?? "system", cancellationToken);
        return ApiResponse.Ok("更新成功");
    }

    [HttpDelete("{id:guid}")]
    public async Task<ApiResponse> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _productService.DeleteAsync(id, _currentUser.Username ?? "system", cancellationToken);
        return ApiResponse.Ok("删除成功");
    }
}
