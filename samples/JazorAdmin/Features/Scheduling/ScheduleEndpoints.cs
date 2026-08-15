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
            .RequireAuthorization(PolicyKeys.PlatformAdministrator);

        schedules.MapGet("/", ListAsync);
        schedules.MapGet("/{key}/runs", ListRunsAsync);
        schedules.MapPut("/{key}", UpdateAsync);
        schedules.MapPost("/{key}/run", TriggerAsync);
        return app;
    }

    private static async Task<IResult> ListAsync(
        AdminDbContext database,
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
        AdminDbContext database,
        CancellationToken cancellationToken)
    {
        if (await database.Schedules.FindAsync([key], cancellationToken) is null)
            return Results.NotFound();

        var items = await database.ScheduleRuns
            .AsNoTracking()
            .Where(run => run.ScheduleKey == key)
            .OrderByDescending(run => run.StartedAtUtc)
            .ThenByDescending(run => run.Id)
            .Take(100)
            .ToArrayAsync(cancellationToken);

        // StartedAt remains the API timestamp. StartedAtUtc is its normalized query key, so SQLite can
        // select the true latest 100 rows without materializing an unbounded task history.
        // StartedAt 仍是 API 的原始时间；StartedAtUtc 是归一化查询键，SQLite 可直接选出真正最新的
        // 100 条，避免把无界任务历史全部物化到应用进程。
        var runs = items
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
        AdminDbContext database,
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
        AdminDbContext database,
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
