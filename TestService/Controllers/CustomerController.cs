using Microsoft.AspNetCore.Mvc;
using TestService.Application.Common;
using TestService.Application.Dtos;
using TestService.Application.Interfaces;

namespace TestService.Web.Controllers;

/// <summary>客户管理 Controller。</summary>
[ApiController]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly ICurrentUser _currentUser;

    public CustomerController(ICustomerService customerService, ICurrentUser currentUser)
    {
        _customerService = customerService;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ApiResponse<PagedResult<CustomerDto>>> Query([FromQuery] CustomerQuery query, CancellationToken cancellationToken)
    {
        var data = await _customerService.QueryAsync(query, cancellationToken);
        return ApiResponse<PagedResult<CustomerDto>>.Ok(data);
    }

    [HttpGet("{id:guid}")]
    public async Task<ApiResponse<CustomerDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var data = await _customerService.GetByIdAsync(id, cancellationToken);
        return data is null
            ? ApiResponse<CustomerDto>.Fail("客户不存在", 404)
            : ApiResponse<CustomerDto>.Ok(data);
    }

    [HttpPost]
    public async Task<ApiResponse<Guid>> Create([FromBody] CustomerCreateRequest request, CancellationToken cancellationToken)
    {
        var id = await _customerService.CreateAsync(request, _currentUser.Username ?? "system", cancellationToken);
        return ApiResponse<Guid>.Ok(id, "创建成功");
    }

    [HttpPut("{id:guid}")]
    public async Task<ApiResponse> Update(Guid id, [FromBody] CustomerUpdateRequest request, CancellationToken cancellationToken)
    {
        await _customerService.UpdateAsync(id, request, _currentUser.Username ?? "system", cancellationToken);
        return ApiResponse.Ok("更新成功");
    }

    [HttpDelete("{id:guid}")]
    public async Task<ApiResponse> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _customerService.DeleteAsync(id, _currentUser.Username ?? "system", cancellationToken);
        return ApiResponse.Ok("删除成功");
    }
}
