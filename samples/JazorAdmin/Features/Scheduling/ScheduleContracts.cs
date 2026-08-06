namespace JazorAdmin.Features.Scheduling;

public sealed record ScheduleUpdate(string Cron, bool Enabled);

public sealed record ScheduleView(
    string Key,
    string Name,
    string Description,
    string Cron,
    bool Enabled,
    string? NextRunAt,
    string? LastRunAt,
    string? LastStatus,
    string? LastMessage);

public sealed record ScheduleRunView(
    string Id,
    string Trigger,
    string Status,
    string StartedAt,
    string? FinishedAt,
    string? Message);
