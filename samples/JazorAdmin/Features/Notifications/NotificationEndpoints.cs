// Aggregates recent operational notifications for the header bell.
// 聚合头部通知铃铛的近期运营通知；当前来源为最近 7 天内失败的 Quartz 任务执行。
using JazorAdmin.Data;
using Microsoft.EntityFrameworkCore;

namespace JazorAdmin.Features.Notifications;

public sealed record NotificationView(
    string Id,
    string Source,
    string Title,
    string Status,
    string StartedAt,
    string? Message);

public static class NotificationEndpoints
{
    private const int ListLimit = 20;
    private static readonly TimeSpan Window = TimeSpan.FromDays(7);

    public static IEndpointRouteBuilder MapNotificationEndpoints(this IEndpointRouteBuilder app)
    {
        var notifications = app.MapGroup("/api/notifications")
            .WithTags("Notifications")
            .RequireAuthorization();

        notifications.MapGet("/", ListAsync);
        return app;
    }

    private static async Task<IResult> ListAsync(
        AdminDbContext database,
        CancellationToken cancellationToken)
    {
        var cutoff = DateTime.UtcNow.Add(-Window);
        var failedRuns = await database.ScheduleRuns
            .AsNoTracking()
            .Where(run => run.Status == "failed" && run.StartedAtUtc >= cutoff)
            .OrderByDescending(run => run.StartedAtUtc)
            .ThenByDescending(run => run.Id)
            .Take(ListLimit)
            .Select(run => new
            {
                run.Id,
                run.Status,
                run.StartedAt,
                run.Message,
                Title = run.Schedule.Name
            })
            .ToArrayAsync(cancellationToken);

        // The indexed UTC mirror is only a SQLite transport concern. Response timestamps retain the
        // original DateTimeOffset value and its offset for the client contract.
        // 带索引 UTC 镜像只服务 SQLite 查询；响应仍返回原始 DateTimeOffset 与 offset，保持客户端契约。
        var views = failedRuns
            .Select(run => new NotificationView(
                run.Id.ToString(),
                "schedule",
                run.Title,
                run.Status,
                run.StartedAt.ToString("O"),
                run.Message))
            .ToArray();

        return Results.Ok(views);
    }
}
