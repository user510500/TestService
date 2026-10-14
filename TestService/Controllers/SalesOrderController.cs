using Microsoft.AspNetCore.Mvc;
using TestService.Application.Common;
using TestService.Application.Dtos;
using TestService.Application.Interfaces;

namespace TestService.Web.Controllers;

/// <summary>销售订单 Controller。</summary>
[ApiController]
[Route("api/[controller]")]
public class SalesOrderController : ControllerBase
{
    private readonly ISalesOrderService _salesOrderService;
    private readonly ICurrentUser _currentUser;

    public SalesOrderController(ISalesOrderService salesOrderService, ICurrentUser currentUser)
    {
        _salesOrderService = salesOrderService;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ApiResponse<PagedResult<SalesOrderDto>>> Query([FromQuery] SalesOrderQuery query, CancellationToken cancellationToken)
    {
        var data = await _salesOrderService.QueryAsync(query, cancellationToken);
        return ApiResponse<PagedResult<SalesOrderDto>>.Ok(data);
    }

    [HttpGet("{id:guid}")]
    public async Task<ApiResponse<SalesOrderDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var data = await _salesOrderService.GetByIdAsync(id, cancellationToken);
        return data is null
            ? ApiResponse<SalesOrderDto>.Fail("订单不存在", 404)
            : ApiResponse<SalesOrderDto>.Ok(data);
    }

    [HttpPost]
    public async Task<ApiResponse<Guid>> Create([FromBody] SalesOrderCreateRequest request, CancellationToken cancellationToken)
    {
        var id = await _salesOrderService.CreateAsync(request, _currentUser.Username ?? "system", cancellationToken);
        return ApiResponse<Guid>.Ok(id, "创建成功");
    }

    [HttpPost("{id:guid}/confirm")]
    public async Task<ApiResponse> Confirm(Guid id, CancellationToken cancellationToken)
    {
        await _salesOrderService.ConfirmAsync(id, _currentUser.Username ?? "system", cancellationToken);
        return ApiResponse.Ok("确认成功");
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<ApiResponse> Cancel(Guid id, CancellationToken cancellationToken)
    {
        await _salesOrderService.CancelAsync(id, _currentUser.Username ?? "system", cancellationToken);
        return ApiResponse.Ok("已取消");
    }

    [HttpDelete("{id:guid}")]
    public async Task<ApiResponse> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _salesOrderService.DeleteAsync(id, _currentUser.Username ?? "system", cancellationToken);
        return ApiResponse.Ok("删除成功");
    }
}
