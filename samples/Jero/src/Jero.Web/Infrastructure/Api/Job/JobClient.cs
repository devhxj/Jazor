using Jero.Web.Application;
using Jero.Web.Infrastructure.Http;
using static ECMAScript.Global;

namespace Jero.Web.Infrastructure.Api.Job;

/// <summary>任务调度 API 与浏览器 JSON 边界。雪花 ID 始终读取和发送为 string。</summary>
[ECMAScriptModule("components/job-client.js")]
public static class JobClient
{
    public const string PeriodTrigger = "Furion.Schedule.PeriodTrigger";
    public const string CronTrigger = "Furion.Schedule.CronTrigger";

    public const string DefaultScript =
        "#region using\n\n" +
        "using Furion;\n" +
        "using Furion.Logging;\n" +
        "using Furion.RemoteRequest.Extensions;\n" +
        "using Furion.Schedule;\n" +
        "using Microsoft.Extensions.DependencyInjection;\n" +
        "using System;\n" +
        "using System.Threading;\n" +
        "using System.Threading.Tasks;\n\n" +
        "#endregion\n" +
        "namespace Jero.Core;\n\n" +
        "/// <summary>\n" +
        "/// 动态作业任务\n" +
        "/// </summary>\n" +
        "[JobDetail(\"your-job-id\")]\n" +
        "public class DynamicJob : IJob\n" +
        "{\n" +
        "    private readonly IServiceProvider _serviceProvider;\n\n" +
        "    public DynamicJob(IServiceProvider serviceProvider)\n" +
        "    {\n" +
        "        _serviceProvider = serviceProvider;\n" +
        "    }\n\n" +
        "    public async Task ExecuteAsync(JobExecutingContext context, CancellationToken stoppingToken)\n" +
        "    {\n" +
        "        using var serviceScope = _serviceProvider.CreateScope();\n" +
        "        await Task.CompletedTask;\n" +
        "    }\n" +
        "}\n";

    public static IPromise<ApiOutcome> Page(JobPageInput input)
        => BrowserApiClient.Post("/api/sysJob/pageJobDetail", input);

    public static IPromise<ApiOutcome> Groups()
        => BrowserApiClient.Post("/api/sysJob/listJobGroup", new JobEmptyInput());

    public static IPromise<ApiOutcome> Dashboard()
        => BrowserApiClient.Get("/api/sysJob/dashboard");

    public static IPromise<ApiOutcome> Clusters()
        => BrowserApiClient.Get("/api/sysJob/jobClusterList");

    public static IPromise<ApiOutcome> TriggerRecords(JobTriggerRecordPageInput input)
        => BrowserApiClient.Post("/api/sysJob/pageJobTriggerRecord", input);

    public static IPromise<ApiOutcome> AddDetail(JobDetailSaveInput input)
    {
        Reflect.DeleteProperty(input, "id");
        Reflect.DeleteProperty(input, "updatedTime");
        OmitEmptyDetailProperties(input);
        return BrowserApiClient.Post("/api/sysJob/addJobDetail", input);
    }

    public static IPromise<ApiOutcome> UpdateDetail(JobDetailSaveInput input)
    {
        OmitEmptyDetailProperties(input);
        return BrowserApiClient.Post("/api/sysJob/updateJobDetail", input);
    }

    public static IPromise<ApiOutcome> DeleteDetail(string jobId)
        => BrowserApiClient.Post("/api/sysJob/deleteJobDetail", new JobIdInput { JobId = jobId });

    public static IPromise<ApiOutcome> AddTrigger(JobTriggerSaveInput input)
    {
        Reflect.DeleteProperty(input, "id");
        OmitEmptyTriggerTimes(input);
        return BrowserApiClient.Post("/api/sysJob/addJobTrigger", input);
    }

    public static IPromise<ApiOutcome> UpdateTrigger(JobTriggerSaveInput input)
    {
        OmitEmptyTriggerTimes(input);
        return BrowserApiClient.Post("/api/sysJob/updateJobTrigger", input);
    }

    public static IPromise<ApiOutcome> DeleteTrigger(string jobId, string triggerId)
        => BrowserApiClient.Post("/api/sysJob/deleteJobTrigger", new JobTriggerIdInput
        {
            JobId = jobId,
            TriggerId = triggerId
        });

    public static IPromise<ApiOutcome> StartAll()
        => BrowserApiClient.Post("/api/sysJob/startAllJob");

    public static IPromise<ApiOutcome> PauseAll()
        => BrowserApiClient.Post("/api/sysJob/pauseAllJob");

    public static IPromise<ApiOutcome> CancelSleep()
        => BrowserApiClient.Post("/api/sysJob/cancelSleep");

    public static IPromise<ApiOutcome> PersistAll()
        => BrowserApiClient.Post("/api/sysJob/persistAll");

    public static IPromise<ApiOutcome> StartJob(string jobId)
        => BrowserApiClient.Post("/api/sysJob/startJob", new JobIdInput { JobId = jobId });

    public static IPromise<ApiOutcome> PauseJob(string jobId)
        => BrowserApiClient.Post("/api/sysJob/pauseJob", new JobIdInput { JobId = jobId });

    public static IPromise<ApiOutcome> CancelJob(string jobId)
        => BrowserApiClient.Post("/api/sysJob/cancelJob", new JobIdInput { JobId = jobId });

    public static IPromise<ApiOutcome> RunJob(string jobId)
        => BrowserApiClient.Post("/api/sysJob/runJob", new JobIdInput { JobId = jobId });

    public static IPromise<ApiOutcome> StartTrigger(string jobId, string triggerId)
        => BrowserApiClient.Post("/api/sysJob/startTrigger", new JobTriggerIdInput
        {
            JobId = jobId,
            TriggerId = triggerId
        });

    public static IPromise<ApiOutcome> PauseTrigger(string jobId, string triggerId)
        => BrowserApiClient.Post("/api/sysJob/pauseTrigger", new JobTriggerIdInput
        {
            JobId = jobId,
            TriggerId = triggerId
        });

    public static IPromise<ApiOutcome> ClearRecords()
        => BrowserApiClient.Post("/api/sysJob/clearJobTriggerRecord");

    public static JobView[] ReadJobs(object? data)
    {
        var values = ReadArray(data is null ? null : Reflect.Get(data, "items"));
        var result = new JobView[values.Length];
        for (var index = 0; index < values.Length; index++)
            result[index] = ReadJob(values[index]!);
        return result;
    }

    public static int ReadTotal(object? data)
        => data is null ? 0 : ReadInt(data, "total", 0);

    public static string[] ReadGroups(object? data)
    {
        var values = ReadArray(data);
        var result = new string[values.Length];
        for (var index = 0; index < values.Length; index++)
            result[index] = NormalizeString(values[index]);
        return result;
    }

    public static JobTriggerRecordView[] ReadRecords(object? data)
    {
        var values = ReadArray(data is null ? null : Reflect.Get(data, "items"));
        var result = new JobTriggerRecordView[values.Length];
        for (var index = 0; index < values.Length; index++)
            result[index] = ReadRecord(values[index]!);
        return result;
    }

    public static JobClusterView[] ReadClusters(object? data)
    {
        var values = ReadArray(data);
        var result = new JobClusterView[values.Length];
        for (var index = 0; index < values.Length; index++)
            result[index] = ReadCluster(values[index]!);
        return result;
    }

    public static JobDashboardView ReadDashboard(object? data)
    {
        if (data is null)
            return new JobDashboardView();

        var groups = ReadArray(Reflect.Get(data, "groupRows"));
        var groupRows = new JobDashboardGroupView[groups.Length];
        for (var index = 0; index < groups.Length; index++)
        {
            var value = groups[index]!;
            groupRows[index] = new JobDashboardGroupView
            {
                GroupName = ReadString(value, "groupName"),
                JobCount = ReadInt(value, "jobCount", 0),
                TriggerCount = ReadInt(value, "triggerCount", 0),
                RunningTriggerCount = ReadInt(value, "runningTriggerCount", 0)
            };
        }

        var clusters = ReadArray(Reflect.Get(data, "clusterRows"));
        var clusterRows = new JobClusterView[clusters.Length];
        for (var index = 0; index < clusters.Length; index++)
            clusterRows[index] = ReadCluster(clusters[index]!);

        var jobs = ReadArray(Reflect.Get(data, "recentlyUpdatedJobs"));
        var recentlyUpdatedJobs = new JobView[jobs.Length];
        for (var index = 0; index < jobs.Length; index++)
            recentlyUpdatedJobs[index] = ReadJob(jobs[index]!);

        return new JobDashboardView
        {
            TotalJobs = ReadInt(data, "totalJobs", 0),
            TotalTriggers = ReadInt(data, "totalTriggers", 0),
            RunningTriggerCount = ReadInt(data, "runningTriggerCount", 0),
            PausedTriggerCount = ReadInt(data, "pausedTriggerCount", 0),
            AbnormalTriggerCount = ReadInt(data, "abnormalTriggerCount", 0),
            ClusterWorkingCount = ReadInt(data, "clusterWorkingCount", 0),
            ClusterWaitingCount = ReadInt(data, "clusterWaitingCount", 0),
            GroupRows = groupRows,
            ClusterRows = clusterRows,
            RecentlyUpdatedJobs = recentlyUpdatedJobs
        };
    }

    public static string BuildHttpProperties(JobHttpDraft draft)
    {
        var inner = JSON.Stringify(new JobHttpPayload
        {
            Body = draft.Body.Trim(),
            ClientName = "HttpJob",
            EnsureSuccessStatusCode = true,
            HttpMethod = new JobHttpMethod { Method = NormalizeHttpMethod(draft.Method) },
            RequestUri = draft.RequestUri.Trim()
        }) ?? "{}";
        return JSON.Stringify(new JobHttpEnvelope { HttpJob = inner }) ?? "{}";
    }

    public static JobHttpDraft ParseHttpProperties(string properties)
    {
        if (properties.Trim().Length == 0)
            return new JobHttpDraft { Valid = true };

        try
        {
            var envelope = JSON.Parse(properties);
            if (envelope is null)
                return new JobHttpDraft();
            var raw = Reflect.Get(envelope, "HttpJob");
            if (raw is null)
                return new JobHttpDraft();

            var value = JSON.Parse(StringValue(raw));
            if (value is null)
                return new JobHttpDraft();
            var methodValue = Reflect.Get(value, "HttpMethod");
            return new JobHttpDraft
            {
                Valid = true,
                RequestUri = ReadString(value, "RequestUri"),
                Method = methodValue is null ? "GET" : NormalizeHttpMethod(ReadString(methodValue, "Method")),
                Body = ReadString(value, "Body")
            };
        }
        catch
        {
            return new JobHttpDraft();
        }
    }

    public static JobTriggerArgs ParseTriggerArgs(string triggerType, string args)
    {
        var normalized = args.Trim();
        if (triggerType == PeriodTrigger)
            return new JobTriggerArgs { Period = normalized.Length == 0 ? "1000" : normalized };
        if (triggerType != CronTrigger || normalized.Length == 0)
            return new JobTriggerArgs { Period = normalized.Length == 0 ? "1000" : normalized };

        var comma = normalized.LastIndexOf(",");
        var value = comma < 0 ? normalized : normalized.Substring(0, comma);
        try
        {
            var parsed = JSON.Parse(value);
            return new JobTriggerArgs { Cron = NormalizeString(parsed) };
        }
        catch
        {
            if (value.Length >= 2 && value[0] == '"' && value[value.Length - 1] == '"')
                value = value.Substring(1, value.Length - 2);
            return new JobTriggerArgs { Cron = value.Trim() };
        }
    }

    public static string BuildTriggerArgs(string triggerType, string period, string cron)
    {
        if (triggerType != CronTrigger)
            return period.Trim();

        var expression = cron.Trim();
        var format = expression.StartsWith("@") ? 0 : CronFormat(expression);
        return (JSON.Stringify(expression) ?? "\"\"") + "," + format;
    }

    private static JobView ReadJob(object value)
    {
        var detail = Reflect.Get(value, "jobDetail");
        if (detail is null)
            return new JobView { Id = string.Empty, JobId = string.Empty, GroupName = string.Empty, Description = string.Empty, UpdatedTime = string.Empty, CreateType = 0 };

        var triggers = ReadArray(Reflect.Get(value, "jobTriggers"));
        var triggerRows = new JobTriggerView[triggers.Length];
        for (var index = 0; index < triggers.Length; index++)
            triggerRows[index] = ReadTrigger(triggers[index]!);

        return new JobView
        {
            Id = ReadString(detail, "id"),
            JobId = ReadString(detail, "jobId"),
            GroupName = ReadString(detail, "groupName"),
            JobType = ReadString(detail, "jobType"),
            AssemblyName = ReadString(detail, "assemblyName"),
            Description = ReadString(detail, "description"),
            Concurrent = ReadBool(detail, "concurrent", true),
            IncludeAnnotation = ReadBool(detail, "includeAnnotation", false),
            Properties = ReadString(detail, "properties"),
            UpdatedTime = ReadString(detail, "updatedTime"),
            CreateType = ReadInt(detail, "createType", 0),
            ScriptCode = ReadString(detail, "scriptCode"),
            JobStatus = ReadString(value, "jobStatus"),
            JobStatusText = ReadString(value, "jobStatusText"),
            Triggers = triggerRows
        };
    }

    private static JobTriggerView ReadTrigger(object value)
        => new()
        {
            Id = ReadString(value, "id"),
            TriggerId = ReadString(value, "triggerId"),
            JobId = ReadString(value, "jobId"),
            TriggerType = ReadString(value, "triggerType"),
            AssemblyName = ReadString(value, "assemblyName"),
            Args = ReadString(value, "args"),
            Description = ReadString(value, "description"),
            Status = ReadInt(value, "status", 1),
            StartTime = ReadString(value, "startTime"),
            EndTime = ReadString(value, "endTime"),
            LastRunTime = ReadString(value, "lastRunTime"),
            NextRunTime = ReadString(value, "nextRunTime"),
            NumberOfRuns = ReadString(value, "numberOfRuns"),
            MaxNumberOfRuns = ReadString(value, "maxNumberOfRuns"),
            NumberOfErrors = ReadString(value, "numberOfErrors"),
            MaxNumberOfErrors = ReadString(value, "maxNumberOfErrors"),
            NumRetries = ReadInt(value, "numRetries", 0),
            RetryTimeout = ReadInt(value, "retryTimeout", 1000),
            StartNow = ReadBool(value, "startNow", true),
            RunOnStart = ReadBool(value, "runOnStart", false),
            ResetOnlyOnce = ReadBool(value, "resetOnlyOnce", true),
            UpdatedTime = ReadString(value, "updatedTime")
        };

    private static JobTriggerRecordView ReadRecord(object value)
        => new()
        {
            Id = ReadString(value, "id"),
            JobId = ReadString(value, "jobId"),
            TriggerId = ReadString(value, "triggerId"),
            NumberOfRuns = ReadString(value, "numberOfRuns"),
            LastRunTime = ReadString(value, "lastRunTime"),
            NextRunTime = ReadString(value, "nextRunTime"),
            Status = ReadInt(value, "status", 1),
            Result = ReadString(value, "result"),
            ElapsedTime = ReadString(value, "elapsedTime"),
            CreatedTime = ReadString(value, "createdTime")
        };

    private static JobClusterView ReadCluster(object value)
        => new()
        {
            Id = ReadString(value, "id"),
            ClusterId = ReadString(value, "clusterId"),
            Description = ReadString(value, "description"),
            Status = ReadInt(value, "status", 0),
            UpdatedTime = ReadString(value, "updatedTime")
        };

    private static void OmitEmptyDetailProperties(JobDetailSaveInput input)
    {
        if (input.Properties.Trim().Length == 0)
            Reflect.DeleteProperty(input, "properties");
    }

    private static void OmitEmptyTriggerTimes(JobTriggerSaveInput input)
    {
        if (string.IsNullOrWhiteSpace(input.StartTime)) Reflect.DeleteProperty(input, "startTime");
        if (string.IsNullOrWhiteSpace(input.EndTime)) Reflect.DeleteProperty(input, "endTime");
        if (string.IsNullOrWhiteSpace(input.LastRunTime)) Reflect.DeleteProperty(input, "lastRunTime");
        if (string.IsNullOrWhiteSpace(input.NextRunTime)) Reflect.DeleteProperty(input, "nextRunTime");
        if (string.IsNullOrWhiteSpace(input.UpdatedTime)) Reflect.DeleteProperty(input, "updatedTime");
    }

    private static string NormalizeHttpMethod(string value)
    {
        var normalized = value.Trim().ToUpper();
        return normalized == "POST" || normalized == "PUT" || normalized == "DELETE" ? normalized : "GET";
    }

    private static int CronFormat(string expression)
    {
        var segments = 0;
        var inside = false;
        for (var index = 0; index < expression.Length; index++)
        {
            var whitespace = char.IsWhiteSpace(expression[index]);
            if (!whitespace && !inside)
            {
                segments++;
                inside = true;
            }
            else if (whitespace)
            {
                inside = false;
            }
        }
        if (segments <= 5) return 0;
        return segments == 6 ? 2 : 3;
    }

    private static object?[] ReadArray(object? value)
        => value as object?[] ?? [];

    private static string ReadString(object data, string key)
    {
        var value = Reflect.Get(data, key);
        return NormalizeString(value);
    }

    private static string NormalizeString(object? value)
    {
        if (value is null)
            return string.Empty;
        var text = StringValue(value);
        return text == "null" || text == "undefined" ? string.Empty : text;
    }

    private static int ReadInt(object data, string key, int fallback)
    {
        var value = Reflect.Get(data, key);
        return value is null ? fallback : (int)NumberValue(value);
    }

    private static bool ReadBool(object data, string key, bool fallback)
    {
        var value = Reflect.Get(data, key);
        return value is null ? fallback : BooleanValue(value);
    }
}

public sealed record JobEmptyInput;

public sealed record JobPageInput
{
    [ECMAScriptName("page")]
    public int Page { get; init; } = 1;
    [ECMAScriptName("pageSize")]
    public int PageSize { get; init; } = 50;
    [ECMAScriptName("jobId")]
    public string JobId { get; init; } = string.Empty;
    [ECMAScriptName("groupName")]
    public string GroupName { get; init; } = string.Empty;
    [ECMAScriptName("description")]
    public string Description { get; init; } = string.Empty;
}

public sealed record JobTriggerRecordPageInput
{
    [ECMAScriptName("page")]
    public int Page { get; init; } = 1;
    [ECMAScriptName("pageSize")]
    public int PageSize { get; init; } = 50;
    [ECMAScriptName("jobId")]
    public string JobId { get; init; } = string.Empty;
    [ECMAScriptName("triggerId")]
    public string TriggerId { get; init; } = string.Empty;
}

public sealed record JobIdInput
{
    [ECMAScriptName("jobId")]
    public string JobId { get; init; } = string.Empty;
}

public sealed record JobTriggerIdInput
{
    [ECMAScriptName("jobId")]
    public string JobId { get; init; } = string.Empty;
    [ECMAScriptName("triggerId")]
    public string TriggerId { get; init; } = string.Empty;
}

public sealed record JobDetailSaveInput
{
    [ECMAScriptName("id")]
    public string Id { get; init; } = string.Empty;
    [ECMAScriptName("jobId")]
    public string JobId { get; init; } = string.Empty;
    [ECMAScriptName("groupName")]
    public string GroupName { get; init; } = "default";
    [ECMAScriptName("jobType")]
    public string JobType { get; init; } = string.Empty;
    [ECMAScriptName("assemblyName")]
    public string AssemblyName { get; init; } = string.Empty;
    [ECMAScriptName("description")]
    public string Description { get; init; } = string.Empty;
    [ECMAScriptName("concurrent")]
    public bool Concurrent { get; init; } = true;
    [ECMAScriptName("includeAnnotation")]
    public bool IncludeAnnotation { get; init; }
    [ECMAScriptName("properties")]
    public string Properties { get; init; } = string.Empty;
    [ECMAScriptName("updatedTime")]
    public string UpdatedTime { get; init; } = string.Empty;
    [ECMAScriptName("createType")]
    public int CreateType { get; init; }
    [ECMAScriptName("scriptCode")]
    public string ScriptCode { get; init; } = string.Empty;
}

public sealed record JobTriggerSaveInput
{
    [ECMAScriptName("id")]
    public string Id { get; init; } = string.Empty;
    [ECMAScriptName("triggerId")]
    public string TriggerId { get; init; } = string.Empty;
    [ECMAScriptName("jobId")]
    public string JobId { get; init; } = string.Empty;
    [ECMAScriptName("triggerType")]
    public string TriggerType { get; init; } = JobClient.PeriodTrigger;
    [ECMAScriptName("assemblyName")]
    public string AssemblyName { get; init; } = "Furion.Pure";
    [ECMAScriptName("args")]
    public string Args { get; init; } = "1000";
    [ECMAScriptName("description")]
    public string Description { get; init; } = string.Empty;
    [ECMAScriptName("status")]
    public int Status { get; init; } = 1;
    [ECMAScriptName("startTime")]
    public string? StartTime { get; init; }
    [ECMAScriptName("endTime")]
    public string? EndTime { get; init; }
    [ECMAScriptName("lastRunTime")]
    public string? LastRunTime { get; init; }
    [ECMAScriptName("nextRunTime")]
    public string? NextRunTime { get; init; }
    [ECMAScriptName("numberOfRuns")]
    public string NumberOfRuns { get; init; } = "0";
    [ECMAScriptName("maxNumberOfRuns")]
    public string MaxNumberOfRuns { get; init; } = "0";
    [ECMAScriptName("numberOfErrors")]
    public string NumberOfErrors { get; init; } = "0";
    [ECMAScriptName("maxNumberOfErrors")]
    public string MaxNumberOfErrors { get; init; } = "0";
    [ECMAScriptName("numRetries")]
    public int NumRetries { get; init; }
    [ECMAScriptName("retryTimeout")]
    public int RetryTimeout { get; init; } = 1000;
    [ECMAScriptName("startNow")]
    public bool StartNow { get; init; } = true;
    [ECMAScriptName("runOnStart")]
    public bool RunOnStart { get; init; }
    [ECMAScriptName("resetOnlyOnce")]
    public bool ResetOnlyOnce { get; init; } = true;
    [ECMAScriptName("updatedTime")]
    public string? UpdatedTime { get; init; }
}

public sealed record JobDashboardView
{
    public int TotalJobs { get; init; }
    public int TotalTriggers { get; init; }
    public int RunningTriggerCount { get; init; }
    public int PausedTriggerCount { get; init; }
    public int AbnormalTriggerCount { get; init; }
    public int ClusterWorkingCount { get; init; }
    public int ClusterWaitingCount { get; init; }
    public JobDashboardGroupView[] GroupRows { get; init; } = [];
    public JobClusterView[] ClusterRows { get; init; } = [];
    public JobView[] RecentlyUpdatedJobs { get; init; } = [];
}

public sealed record JobHttpDraft
{
    public bool Valid { get; init; }
    public string RequestUri { get; init; } = string.Empty;
    public string Method { get; init; } = "GET";
    public string Body { get; init; } = string.Empty;
}

public sealed record JobTriggerArgs
{
    public string Period { get; init; } = "1000";
    public string Cron { get; init; } = string.Empty;
}

public sealed record JobHttpEnvelope
{
    public string HttpJob { get; init; } = string.Empty;
}

public sealed record JobHttpPayload
{
    public string Body { get; init; } = string.Empty;
    public string ClientName { get; init; } = "HttpJob";
    public bool EnsureSuccessStatusCode { get; init; }
    public JobHttpMethod HttpMethod { get; init; } = new();
    public string RequestUri { get; init; } = string.Empty;
}

public sealed record JobHttpMethod
{
    public string Method { get; init; } = "GET";
}
