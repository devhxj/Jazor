using static ECMAScript.Vue;

namespace Jero.Web.Application;

public sealed record JobClusterView : VueProps
{
    public required string Id { get; init; }
    public required string ClusterId { get; init; }
    [ECMAScriptName("description")]
    public required string Description { get; init; }
    [ECMAScriptName("status")]
    public required int Status { get; init; }
    [ECMAScriptName("updatedTime")]
    public required string UpdatedTime { get; init; }
}

public sealed record JobTriggerRecordView : VueProps
{
    public required string Id { get; init; }
    public required string JobId { get; init; }
    public required string TriggerId { get; init; }
    public string NumberOfRuns { get; init; } = "0";
    [ECMAScriptName("lastRunTime")]
    public string LastRunTime { get; init; } = string.Empty;
    [ECMAScriptName("nextRunTime")]
    public string NextRunTime { get; init; } = string.Empty;
    [ECMAScriptName("status")]
    public required int Status { get; init; }
    [ECMAScriptName("result")]
    public string Result { get; init; } = string.Empty;
    public required string ElapsedTime { get; init; }
    [ECMAScriptName("createdTime")]
    public required string CreatedTime { get; init; }
}

public sealed record JobView : VueProps
{
    public required string Id { get; init; }
    [ECMAScriptName("jobId")]
    public required string JobId { get; init; }
    public required string GroupName { get; init; }
    public string JobType { get; init; } = string.Empty;
    public string AssemblyName { get; init; } = string.Empty;
    [ECMAScriptName("description")]
    public required string Description { get; init; }
    [ECMAScriptName("concurrent")]
    public bool Concurrent { get; init; }
    public bool IncludeAnnotation { get; init; }
    [ECMAScriptName("properties")]
    public string Properties { get; init; } = string.Empty;
    [ECMAScriptName("updatedTime")]
    public required string UpdatedTime { get; init; }
    [ECMAScriptName("createType")]
    public required int CreateType { get; init; }
    public string ScriptCode { get; init; } = string.Empty;
    [ECMAScriptName("jobStatus")]
    public string JobStatus { get; init; } = string.Empty;
    [ECMAScriptName("jobStatusText")]
    public string JobStatusText { get; init; } = string.Empty;
    public JobTriggerView[] Triggers { get; init; } = [];
}

public sealed record JobDashboardGroupView : VueProps
{
    public required string GroupName { get; init; }
    public required int JobCount { get; init; }
    public required int TriggerCount { get; init; }
    public required int RunningTriggerCount { get; init; }
}

public sealed record JobTriggerView : VueProps
{
    public string Id { get; init; } = string.Empty;
    [ECMAScriptName("triggerId")]
    public string TriggerId { get; init; } = string.Empty;
    [ECMAScriptName("jobId")]
    public string JobId { get; init; } = string.Empty;
    [ECMAScriptName("triggerType")]
    public string TriggerType { get; init; } = string.Empty;
    public string AssemblyName { get; init; } = string.Empty;
    public string Args { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    [ECMAScriptName("status")]
    public int Status { get; init; }
    [ECMAScriptName("startTime")]
    public string StartTime { get; init; } = string.Empty;
    [ECMAScriptName("endTime")]
    public string EndTime { get; init; } = string.Empty;
    [ECMAScriptName("lastRunTime")]
    public string LastRunTime { get; init; } = string.Empty;
    [ECMAScriptName("nextRunTime")]
    public string NextRunTime { get; init; } = string.Empty;
    public string NumberOfRuns { get; init; } = "0";
    public string MaxNumberOfRuns { get; init; } = "0";
    public string NumberOfErrors { get; init; } = "0";
    public string MaxNumberOfErrors { get; init; } = "0";
    public int NumRetries { get; init; }
    public int RetryTimeout { get; init; }
    [ECMAScriptName("startNow")]
    public bool StartNow { get; init; }
    [ECMAScriptName("runOnStart")]
    public bool RunOnStart { get; init; }
    [ECMAScriptName("resetOnlyOnce")]
    public bool ResetOnlyOnce { get; init; }
    public string UpdatedTime { get; init; } = string.Empty;
}
