namespace Jero.Web.App.Job;

public sealed partial record JobClusterView
{
    public required string id { get; init; }
    public required string clusterId { get; init; }
    public required string description { get; init; }
    public required int status { get; init; }
    public required string updatedTime { get; init; }
}

public sealed partial record JobTriggerRecordView
{
    public required string id { get; init; }
    public required string jobId { get; init; }
    public required string triggerId { get; init; }
    public string numberOfRuns { get; init; } = "0";
    public string lastRunTime { get; init; } = string.Empty;
    public string nextRunTime { get; init; } = string.Empty;
    public required int status { get; init; }
    public string result { get; init; } = string.Empty;
    public required string elapsedTime { get; init; }
    public required string createdTime { get; init; }
}

public sealed partial record JobView
{
    public required string id { get; init; }
    public required string jobId { get; init; }
    public required string groupName { get; init; }
    public string jobType { get; init; } = string.Empty;
    public string assemblyName { get; init; } = string.Empty;
    public required string description { get; init; }
    public bool concurrent { get; init; }
    public bool includeAnnotation { get; init; }
    public string properties { get; init; } = string.Empty;
    public required string updatedTime { get; init; }
    public required int createType { get; init; }
    public string scriptCode { get; init; } = string.Empty;
    public string jobStatus { get; init; } = string.Empty;
    public string jobStatusText { get; init; } = string.Empty;
    public JobTriggerView[] triggers { get; init; } = [];
}

public sealed partial record JobDashboardGroupView
{
    public required string groupName { get; init; }
    public required int jobCount { get; init; }
    public required int triggerCount { get; init; }
    public required int runningTriggerCount { get; init; }
}

public sealed partial record JobTriggerView
{
    public string id { get; init; } = string.Empty;
    public string triggerId { get; init; } = string.Empty;
    public string jobId { get; init; } = string.Empty;
    public string triggerType { get; init; } = string.Empty;
    public string assemblyName { get; init; } = string.Empty;
    public string args { get; init; } = string.Empty;
    public string description { get; init; } = string.Empty;
    public int status { get; init; }
    public string startTime { get; init; } = string.Empty;
    public string endTime { get; init; } = string.Empty;
    public string lastRunTime { get; init; } = string.Empty;
    public string nextRunTime { get; init; } = string.Empty;
    public string numberOfRuns { get; init; } = "0";
    public string maxNumberOfRuns { get; init; } = "0";
    public string numberOfErrors { get; init; } = "0";
    public string maxNumberOfErrors { get; init; } = "0";
    public int numRetries { get; init; }
    public int retryTimeout { get; init; }
    public bool startNow { get; init; }
    public bool runOnStart { get; init; }
    public bool resetOnlyOnce { get; init; }
    public string updatedTime { get; init; } = string.Empty;
}
