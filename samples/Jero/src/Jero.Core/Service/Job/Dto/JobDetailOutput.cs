namespace Jero.Core.Service;

public class JobDetailOutput
{
    /// <summary>
    /// 作业信息
    /// </summary>
    public SysJobDetail JobDetail { get; set; } = new();

    /// <summary>
    /// 作业运行状态
    /// </summary>
    public string JobStatus { get; set; } = JobRuntimeStatusResolver.Unknown;

    /// <summary>
    /// 作业运行状态文本
    /// </summary>
    public string JobStatusText { get; set; } = "未知";

    /// <summary>
    /// 触发器集合
    /// </summary>
    public List<SysJobTrigger> JobTriggers { get; set; } = [];
}

public class JobDashboardOutput
{
    public int TotalJobs { get; set; }

    public int TotalTriggers { get; set; }

    public int RunningTriggerCount { get; set; }

    public int PausedTriggerCount { get; set; }

    public int AbnormalTriggerCount { get; set; }

    public int ClusterWorkingCount { get; set; }

    public int ClusterWaitingCount { get; set; }

    public List<JobDashboardGroupOutput> GroupRows { get; set; } = [];

    public List<SysJobCluster> ClusterRows { get; set; } = [];

    public List<JobDetailOutput> RecentlyUpdatedJobs { get; set; } = [];
}

public class JobDashboardGroupOutput
{
    public string GroupName { get; set; } = "default";

    public int JobCount { get; set; }

    public int TriggerCount { get; set; }

    public int RunningTriggerCount { get; set; }
}
