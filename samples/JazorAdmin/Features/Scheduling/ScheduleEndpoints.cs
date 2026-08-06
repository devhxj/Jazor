using JazorAdmin.Authorization;
using JazorAdmin.Data;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace JazorAdmin.Features.Scheduling;

public static class ScheduleEndpoints
{
    public static IEndpointRouteBuilder MapScheduleEndpoints(this IEndpointRouteBuilder app)
    {
        var schedules = app.MapGroup("/api/schedules")
            .WithTags("Schedules")
            .RequireAuthorization(JazorAdminPolicies.PlatformAdministrator);

        schedules.MapGet("/", ListAsync);
        schedules.MapGet("/{key}/runs", ListRunsAsync);
        schedules.MapPut("/{key}", UpdateAsync);
        schedules.MapPost("/{key}/run", TriggerAsync);
        return app;
    }

    private static async Task<IResult> ListAsync(
        JazorAdminDbContext database,
        ScheduleService schedules,
        CancellationToken cancellationToken)
    {
        var items = await database.Schedules
            .AsNoTracking()
            .OrderBy(schedule => schedule.Name)
            .ToArrayAsync(cancellationToken);
        var views = new ScheduleView[items.Length];
        for (var index = 0; index < items.Length; index++)
            views[index] = await ToViewAsync(items[index], schedules, cancellationToken);
        return Results.Ok(views);
    }

    private static async Task<IResult> ListRunsAsync(
        string key,
        JazorAdminDbContext database,
        CancellationToken cancellationToken)
    {
        if (await database.Schedules.FindAsync([key], cancellationToken) is null)
            return Results.NotFound();

        var items = await database.ScheduleRuns
            .AsNoTracking()
            .Where(run => run.ScheduleKey == key)
            .ToArrayAsync(cancellationToken);

        // SQLite cannot translate ORDER BY DateTimeOffset. Keep the persisted UTC offset in the model and
        // order the bounded administration history after materialization instead of weakening the timestamp type.
        // SQLite 无法翻译 DateTimeOffset 的排序；保留模型中的 UTC 偏移时间，在物化后排序管理历史，
        // 不因测试/默认数据库限制而削弱时间类型。
        var runs = items
            .OrderByDescending(run => run.StartedAt)
            .Take(100)
            .Select(run => new ScheduleRunView(
                run.Id.ToString(),
                run.Trigger,
                run.Status,
                run.StartedAt.ToString("O"),
                run.FinishedAt == null ? null : run.FinishedAt.Value.ToString("O"),
                run.Message))
            .ToArray();
        return Results.Ok(runs);
    }

    private static async Task<IResult> UpdateAsync(
        string key,
        ScheduleUpdate request,
        JazorAdminDbContext database,
        ScheduleService schedules,
        CancellationToken cancellationToken)
    {
        var schedule = await database.Schedules.FindAsync([key], cancellationToken);
        if (schedule is null)
            return Results.NotFound();

        var cron = request.Cron?.Trim();
        if (string.IsNullOrEmpty(cron) || !CronExpression.IsValidExpression(cron))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["cron"] = ["Cron must be a valid Quartz expression."]
            });
        }

        schedule.Cron = cron;
        schedule.Enabled = request.Enabled;
        await database.SaveChangesAsync(cancellationToken);
        await schedules.SyncAsync(schedule, cancellationToken);
        return Results.Ok(await ToViewAsync(schedule, schedules, cancellationToken));
    }

    private static async Task<IResult> TriggerAsync(
        string key,
        JazorAdminDbContext database,
        ScheduleService schedules,
        CancellationToken cancellationToken)
    {
        if (await database.Schedules.FindAsync([key], cancellationToken) is null)
            return Results.NotFound();

        await schedules.TriggerAsync(key, cancellationToken);
        return Results.Accepted("/api/schedules/" + key + "/runs");
    }

    private static async Task<ScheduleView> ToViewAsync(
        Schedule schedule,
        ScheduleService schedules,
        CancellationToken cancellationToken)
        => new(
            schedule.Key,
            schedule.Name,
            schedule.Description,
            schedule.Cron,
            schedule.Enabled,
            await schedules.GetNextRunAsync(schedule.Key, cancellationToken),
            schedule.LastRunAt?.ToString("O"),
            schedule.LastStatus,
            schedule.LastMessage);
}
