using JazorAdmin.Data;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace JazorAdmin.Features.Scheduling;

[DisallowConcurrentExecution]
public sealed class ManagedTaskJob(
    IServiceScopeFactory scopeFactory,
    ILogger<ManagedTaskJob> logger) : IJob
{
    public const string ScheduleKeyData = "schedule-key";
    public const string TriggerData = "trigger";

    public async Task Execute(IJobExecutionContext context)
    {
        var scheduleKey = context.MergedJobDataMap.GetString(ScheduleKeyData);
        if (string.IsNullOrWhiteSpace(scheduleKey))
            return;

        await using var scope = scopeFactory.CreateAsyncScope();
        var services = scope.ServiceProvider;
        var database = services.GetRequiredService<JazorAdminDbContext>();
        var schedule = await database.Schedules.FindAsync([scheduleKey], context.CancellationToken);
        if (schedule is null)
            return;

        var run = new ScheduleRun
        {
            ScheduleKey = schedule.Key,
            Trigger = context.MergedJobDataMap.GetString(TriggerData) ?? "scheduled",
            Status = "running",
            StartedAt = DateTimeOffset.UtcNow
        };
        database.ScheduleRuns.Add(run);
        await database.SaveChangesAsync(context.CancellationToken);

        try
        {
            var task = services.GetServices<IManagedTask>()
                .SingleOrDefault(candidate => candidate.Key == schedule.Key);
            if (task is null)
                throw new InvalidOperationException($"No managed task is registered for '{schedule.Key}'.");

            run.Status = "succeeded";
            run.Message = await task.ExecuteAsync(context.CancellationToken);
            schedule.LastStatus = run.Status;
            schedule.LastMessage = run.Message;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Scheduled task {ScheduleKey} failed.", schedule.Key);
            run.Status = "failed";
            run.Message = exception.Message;
            schedule.LastStatus = run.Status;
            schedule.LastMessage = run.Message;
        }

        run.FinishedAt = DateTimeOffset.UtcNow;
        schedule.LastRunAt = run.FinishedAt;
        await database.SaveChangesAsync(context.CancellationToken);
    }
}
