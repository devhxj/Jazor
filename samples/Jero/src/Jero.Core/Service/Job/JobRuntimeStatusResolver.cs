namespace Jero.Core.Service;

public static class JobRuntimeStatusResolver
{
    public const string Abnormal = "abnormal";
    public const string NoTrigger = "noTrigger";
    public const string NotLoaded = "notLoaded";
    public const string NotStarted = "notStarted";
    public const string Paused = "paused";
    public const string Running = "running";
    public const string Started = "started";
    public const string Stopped = "stopped";
    public const string Unknown = "unknown";

    public static JobRuntimeStatus Resolve(bool isLoaded, IEnumerable<TriggerStatus> triggerStatuses)
    {
        if (!isLoaded) return new JobRuntimeStatus(NotLoaded, "未加载");

        var statuses = triggerStatuses.ToList();
        if (statuses.Count == 0) return new JobRuntimeStatus(NoTrigger, "无触发器");

        if (statuses.Any(u => u == TriggerStatus.Running)) return new JobRuntimeStatus(Running, "运行中");

        if (statuses.All(u => u == TriggerStatus.Pause)) return new JobRuntimeStatus(Paused, "已暂停");

        if (statuses.All(u => u == TriggerStatus.NotStart)) return new JobRuntimeStatus(NotStarted, "未启动");

        if (statuses.Any(IsAbnormalStatus)) return new JobRuntimeStatus(Abnormal, "异常");

        if (statuses.All(IsStoppedStatus)) return new JobRuntimeStatus(Stopped, "已停止");

        if (statuses.Any(IsStartedStatus)) return new JobRuntimeStatus(Started, "已启动");

        return new JobRuntimeStatus(Unknown, "未知");
    }

    private static bool IsAbnormalStatus(TriggerStatus status)
    {
        return status is TriggerStatus.ErrorToReady
            or TriggerStatus.Blocked
            or TriggerStatus.Panic
            or TriggerStatus.Overrun
            or TriggerStatus.Unknown
            or TriggerStatus.Unhandled;
    }

    private static bool IsStartedStatus(TriggerStatus status)
    {
        return status is TriggerStatus.Backlog
            or TriggerStatus.Ready;
    }

    private static bool IsStoppedStatus(TriggerStatus status)
    {
        return status is TriggerStatus.Archived or TriggerStatus.Unoccupied;
    }
}

public sealed class JobRuntimeStatus
{
    public JobRuntimeStatus(string value, string text)
    {
        Value = value;
        Text = text;
    }

    public string Text { get; }

    public string Value { get; }
}
