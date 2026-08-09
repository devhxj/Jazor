// Adapts persisted schedule metadata to Quartz. Quartz owns trigger execution and Cron semantics.
// 将持久化调度元数据映射到 Quartz；触发与 Cron 语义完全交由 Quartz 管理。
using JazorAdmin.Data;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace JazorAdmin.Features.Scheduling;

public sealed class ScheduleService(
    ISchedulerFactory schedulerFactory,
    IServiceScopeFactory scopeFactory)
{
    private const string Group = "jazoradmin";

    public async Task SyncAllAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var schedules = await scope.ServiceProvider.GetRequiredService<AdminDbContext>()
            .Schedules
            .AsNoTracking()
            .ToArrayAsync(cancellationToken);
        foreach (var schedule in schedules)
            await SyncAsync(schedule, cancellationToken);
    }

    public async Task SyncAsync(Schedule schedule, CancellationToken cancellationToken)
    {
        var scheduler = await schedulerFactory.GetScheduler(cancellationToken);
        var jobKey = GetJobKey(schedule.Key);
        var triggerKey = GetTriggerKey(schedule.Key);
        var job = JobBuilder.Create<ManagedTaskJob>()
            .WithIdentity(jobKey)
            .StoreDurably()
            .UsingJobData(ManagedTaskJob.ScheduleKeyData, schedule.Key)
            .Build();
        await scheduler.AddJob(job, replace: true, storeNonDurableWhileAwaitingScheduling: true, cancellationToken);

        if (!schedule.Enabled)
        {
            if (await scheduler.CheckExists(triggerKey, cancellationToken))
                await scheduler.UnscheduleJob(triggerKey, cancellationToken);
            return;
        }

        var trigger = TriggerBuilder.Create()
            .WithIdentity(triggerKey)
            .ForJob(jobKey)
            .UsingJobData(ManagedTaskJob.TriggerData, "scheduled")
            .WithCronSchedule(
                schedule.Cron,
                cron => cron
                    .InTimeZone(TimeZoneInfo.Utc)
                    .WithMisfireHandlingInstructionDoNothing())
            .Build();
        if (await scheduler.CheckExists(triggerKey, cancellationToken))
            await scheduler.RescheduleJob(triggerKey, trigger, cancellationToken);
        else
            await scheduler.ScheduleJob(trigger, cancellationToken);
    }

    public async Task TriggerAsync(string key, CancellationToken cancellationToken)
    {
        var scheduler = await schedulerFactory.GetScheduler(cancellationToken);
        await scheduler.TriggerJob(
            GetJobKey(key),
            new JobDataMap { [ManagedTaskJob.TriggerData] = "manual" },
            cancellationToken);
    }

    public async Task<string?> GetNextRunAsync(string key, CancellationToken cancellationToken)
    {
        var scheduler = await schedulerFactory.GetScheduler(cancellationToken);
        var trigger = await scheduler.GetTrigger(GetTriggerKey(key), cancellationToken);
        return trigger?.GetNextFireTimeUtc()?.ToString("O");
    }

    public static JobKey GetJobKey(string key) => new(key, Group);

    private static TriggerKey GetTriggerKey(string key) => new(key, Group);
}
