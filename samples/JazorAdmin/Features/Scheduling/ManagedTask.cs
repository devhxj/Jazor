// Defines the bounded task catalog. Schedules may change timing, never the code that is executed.
// 任务中心只调整已有任务的调度，不接受由管理页注入任意执行代码。
namespace JazorAdmin.Features.Scheduling;

public interface IManagedTask
{
    string Key { get; }

    Task<string> ExecuteAsync(CancellationToken cancellationToken);
}

public sealed record ScheduleSeed(string Key, string Name, string Description, string Cron, bool Enabled);

public static class ScheduleCatalog
{
    public static readonly ScheduleSeed[] Items =
    [
        new(
            "openid-prune",
            "Prune expired OpenID records",
            "Removes invalid OpenIddict tokens and detached authorizations older than 14 days.",
            "0 15 2 * * ?",
            true)
    ];
}
