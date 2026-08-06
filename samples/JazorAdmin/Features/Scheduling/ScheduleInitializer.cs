using JazorAdmin.Data;
using Microsoft.EntityFrameworkCore;

namespace JazorAdmin.Features.Scheduling;

public sealed class ScheduleInitializer(
    IServiceScopeFactory scopeFactory,
    ScheduleService schedules) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<JazorAdminDbContext>();
        foreach (var item in ScheduleCatalog.Items)
        {
            if (await database.Schedules.FindAsync([item.Key], cancellationToken) is not null)
                continue;

            database.Schedules.Add(new Schedule
            {
                Key = item.Key,
                Name = item.Name,
                Description = item.Description,
                Cron = item.Cron,
                Enabled = item.Enabled
            });
        }

        if (database.ChangeTracker.HasChanges())
            await database.SaveChangesAsync(cancellationToken);
        await schedules.SyncAllAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
