using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopNest.Application.Common;
using ShopNest.Application.Dtos;
using ShopNest.Application.Features;

namespace ShopNest.API.Controllers;

[ApiController]
[Route("api/admin/logs")]
[Authorize(Roles = "Admin,SuperAdmin")]
public sealed class AdminLogsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminLogsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("audits")]
    public async Task<ActionResult<ApiResponse<PagedResult<AuditLogDto>>>> GetAudits([FromQuery] LogQueryRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAuditLogsQuery(request), cancellationToken);
        return Ok(ApiResponse<PagedResult<AuditLogDto>>.SuccessResult(result));
    }

    [HttpGet("activities")]
    public async Task<ActionResult<ApiResponse<PagedResult<ActivityLogDto>>>> GetActivities([FromQuery] LogQueryRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetActivityLogsQuery(request), cancellationToken);
        return Ok(ApiResponse<PagedResult<ActivityLogDto>>.SuccessResult(result));
    }

    [HttpGet("errors")]
    public async Task<ActionResult<ApiResponse<PagedResult<ErrorLogDto>>>> GetErrors([FromQuery] LogQueryRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetErrorLogsQuery(request), cancellationToken);
        return Ok(ApiResponse<PagedResult<ErrorLogDto>>.SuccessResult(result));
    }

    [HttpGet("logins")]
    public async Task<ActionResult<ApiResponse<PagedResult<LoginHistoryDto>>>> GetLogins([FromQuery] LogQueryRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetLoginHistoriesQuery(request), cancellationToken);
        return Ok(ApiResponse<PagedResult<LoginHistoryDto>>.SuccessResult(result));
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<ApiResponse<AuditDashboardSummaryDto>>> GetDashboard(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAuditDashboardSummaryQuery(), cancellationToken);
        return Ok(ApiResponse<AuditDashboardSummaryDto>.SuccessResult(result));
    }
}
