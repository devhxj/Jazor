


using Jero.Plugin.ApprovalFlow.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
namespace Jero.Plugin.ApprovalFlow;

/// <summary>
/// 扩展审批流中间件
/// </summary>
public static class ApprovalFlowMiddlewareExtensions
{
    /// <summary>
    /// 使用审批流
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseApprovalFlow(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ApprovalFlowMiddleware>();
    }
}

/// <summary>
/// 审批流中间件
/// </summary>
public class ApprovalFlowMiddleware
{
    private readonly RequestDelegate _next;
    private readonly SysApprovalService _sysApprovalService;

    public ApprovalFlowMiddleware(RequestDelegate next)
    {
        _next = next;
        _sysApprovalService = App.GetRequiredService<SysApprovalService>();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _sysApprovalService.MatchApproval(context);

        // 调用下一个中间件
        await _next(context);
    }
}