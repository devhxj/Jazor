using Jero.Web.App.Http;
using static ECMAScript.Global;

namespace Jero.Web.App.Job;

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
        => BrowserApiClient.Post("/api/sysJob/deleteJobDetail", new JobIdInput { jobId = jobId });

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
            jobId = jobId,
            triggerId = triggerId
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
        => BrowserApiClient.Post("/api/sysJob/startJob", new JobIdInput { jobId = jobId });

    public static IPromise<ApiOutcome> PauseJob(string jobId)
        => BrowserApiClient.Post("/api/sysJob/pauseJob", new JobIdInput { jobId = jobId });

    public static IPromise<ApiOutcome> CancelJob(string jobId)
        => BrowserApiClient.Post("/api/sysJob/cancelJob", new JobIdInput { jobId = jobId });

    public static IPromise<ApiOutcome> RunJob(string jobId)
        => BrowserApiClient.Post("/api/sysJob/runJob", new JobIdInput { jobId = jobId });

    public static IPromise<ApiOutcome> StartTrigger(string jobId, string triggerId)
        => BrowserApiClient.Post("/api/sysJob/startTrigger", new JobTriggerIdInput
        {
            jobId = jobId,
            triggerId = triggerId
        });

    public static IPromise<ApiOutcome> PauseTrigger(string jobId, string triggerId)
        => BrowserApiClient.Post("/api/sysJob/pauseTrigger", new JobTriggerIdInput
        {
            jobId = jobId,
            triggerId = triggerId
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
                groupName = ReadString(value, "groupName"),
                jobCount = ReadInt(value, "jobCount", 0),
                triggerCount = ReadInt(value, "triggerCount", 0),
                runningTriggerCount = ReadInt(value, "runningTriggerCount", 0)
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
            totalJobs = ReadInt(data, "totalJobs", 0),
            totalTriggers = ReadInt(data, "totalTriggers", 0),
            runningTriggerCount = ReadInt(data, "runningTriggerCount", 0),
            pausedTriggerCount = ReadInt(data, "pausedTriggerCount", 0),
            abnormalTriggerCount = ReadInt(data, "abnormalTriggerCount", 0),
            clusterWorkingCount = ReadInt(data, "clusterWorkingCount", 0),
            clusterWaitingCount = ReadInt(data, "clusterWaitingCount", 0),
            groupRows = groupRows,
            clusterRows = clusterRows,
            recentlyUpdatedJobs = recentlyUpdatedJobs
        };
    }

    public static string BuildHttpProperties(JobHttpDraft draft)
    {
        var inner = JSON.Stringify(new JobHttpPayload
        {
            Body = draft.body.Trim(),
            ClientName = "HttpJob",
            EnsureSuccessStatusCode = true,
            HttpMethod = new JobHttpMethod { Method = NormalizeHttpMethod(draft.method) },
            RequestUri = draft.requestUri.Trim()
        }) ?? "{}";
        return JSON.Stringify(new JobHttpEnvelope { HttpJob = inner }) ?? "{}";
    }

    public static JobHttpDraft ParseHttpProperties(string properties)
    {
        if (properties.Trim().Length == 0)
            return new JobHttpDraft { valid = true };

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
                valid = true,
                requestUri = ReadString(value, "RequestUri"),
                method = methodValue is null ? "GET" : NormalizeHttpMethod(ReadString(methodValue, "Method")),
                body = ReadString(value, "Body")
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
            return new JobTriggerArgs { period = normalized.Length == 0 ? "1000" : normalized };
        if (triggerType != CronTrigger || normalized.Length == 0)
            return new JobTriggerArgs { period = normalized.Length == 0 ? "1000" : normalized };

        var comma = normalized.LastIndexOf(",");
        var value = comma < 0 ? normalized : normalized.Substring(0, comma);
        try
        {
            var parsed = JSON.Parse(value);
            return new JobTriggerArgs { cron = NormalizeString(parsed) };
        }
        catch
        {
            if (value.Length >= 2 && value[0] == '"' && value[value.Length - 1] == '"')
                value = value.Substring(1, value.Length - 2);
            return new JobTriggerArgs { cron = value.Trim() };
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
            return new JobView { id = string.Empty, jobId = string.Empty, groupName = string.Empty, description = string.Empty, updatedTime = string.Empty, createType = 0 };

        var triggers = ReadArray(Reflect.Get(value, "jobTriggers"));
        var triggerRows = new JobTriggerView[triggers.Length];
        for (var index = 0; index < triggers.Length; index++)
            triggerRows[index] = ReadTrigger(triggers[index]!);

        return new JobView
        {
            id = ReadString(detail, "id"),
            jobId = ReadString(detail, "jobId"),
            groupName = ReadString(detail, "groupName"),
            jobType = ReadString(detail, "jobType"),
            assemblyName = ReadString(detail, "assemblyName"),
            description = ReadString(detail, "description"),
            concurrent = ReadBool(detail, "concurrent", true),
            includeAnnotation = ReadBool(detail, "includeAnnotation", false),
            properties = ReadString(detail, "properties"),
            updatedTime = ReadString(detail, "updatedTime"),
            createType = ReadInt(detail, "createType", 0),
            scriptCode = ReadString(detail, "scriptCode"),
            jobStatus = ReadString(value, "jobStatus"),
            jobStatusText = ReadString(value, "jobStatusText"),
            triggers = triggerRows
        };
    }

    private static JobTriggerView ReadTrigger(object value)
        => new()
        {
            id = ReadString(value, "id"),
            triggerId = ReadString(value, "triggerId"),
            jobId = ReadString(value, "jobId"),
            triggerType = ReadString(value, "triggerType"),
            assemblyName = ReadString(value, "assemblyName"),
            args = ReadString(value, "args"),
            description = ReadString(value, "description"),
            status = ReadInt(value, "status", 1),
            startTime = ReadString(value, "startTime"),
            endTime = ReadString(value, "endTime"),
            lastRunTime = ReadString(value, "lastRunTime"),
            nextRunTime = ReadString(value, "nextRunTime"),
            numberOfRuns = ReadString(value, "numberOfRuns"),
            maxNumberOfRuns = ReadString(value, "maxNumberOfRuns"),
            numberOfErrors = ReadString(value, "numberOfErrors"),
            maxNumberOfErrors = ReadString(value, "maxNumberOfErrors"),
            numRetries = ReadInt(value, "numRetries", 0),
            retryTimeout = ReadInt(value, "retryTimeout", 1000),
            startNow = ReadBool(value, "startNow", true),
            runOnStart = ReadBool(value, "runOnStart", false),
            resetOnlyOnce = ReadBool(value, "resetOnlyOnce", true),
            updatedTime = ReadString(value, "updatedTime")
        };

    private static JobTriggerRecordView ReadRecord(object value)
        => new()
        {
            id = ReadString(value, "id"),
            jobId = ReadString(value, "jobId"),
            triggerId = ReadString(value, "triggerId"),
            numberOfRuns = ReadString(value, "numberOfRuns"),
            lastRunTime = ReadString(value, "lastRunTime"),
            nextRunTime = ReadString(value, "nextRunTime"),
            status = ReadInt(value, "status", 1),
            result = ReadString(value, "result"),
            elapsedTime = ReadString(value, "elapsedTime"),
            createdTime = ReadString(value, "createdTime")
        };

    private static JobClusterView ReadCluster(object value)
        => new()
        {
            id = ReadString(value, "id"),
            clusterId = ReadString(value, "clusterId"),
            description = ReadString(value, "description"),
            status = ReadInt(value, "status", 0),
            updatedTime = ReadString(value, "updatedTime")
        };

    private static void OmitEmptyDetailProperties(JobDetailSaveInput input)
    {
        if (input.properties.Trim().Length == 0)
            Reflect.DeleteProperty(input, "properties");
    }

    private static void OmitEmptyTriggerTimes(JobTriggerSaveInput input)
    {
        if (string.IsNullOrWhiteSpace(input.startTime)) Reflect.DeleteProperty(input, "startTime");
        if (string.IsNullOrWhiteSpace(input.endTime)) Reflect.DeleteProperty(input, "endTime");
        if (string.IsNullOrWhiteSpace(input.lastRunTime)) Reflect.DeleteProperty(input, "lastRunTime");
        if (string.IsNullOrWhiteSpace(input.nextRunTime)) Reflect.DeleteProperty(input, "nextRunTime");
        if (string.IsNullOrWhiteSpace(input.updatedTime)) Reflect.DeleteProperty(input, "updatedTime");
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
    public int page { get; init; } = 1;
    public int pageSize { get; init; } = 50;
    public string jobId { get; init; } = string.Empty;
    public string groupName { get; init; } = string.Empty;
    public string description { get; init; } = string.Empty;
}

public sealed record JobTriggerRecordPageInput
{
    public int page { get; init; } = 1;
    public int pageSize { get; init; } = 50;
    public string jobId { get; init; } = string.Empty;
    public string triggerId { get; init; } = string.Empty;
}

public sealed record JobIdInput
{
    public string jobId { get; init; } = string.Empty;
}

public sealed record JobTriggerIdInput
{
    public string jobId { get; init; } = string.Empty;
    public string triggerId { get; init; } = string.Empty;
}

public sealed record JobDetailSaveInput
{
    public string id { get; init; } = string.Empty;
    public string jobId { get; init; } = string.Empty;
    public string groupName { get; init; } = "default";
    public string jobType { get; init; } = string.Empty;
    public string assemblyName { get; init; } = string.Empty;
    public string description { get; init; } = string.Empty;
    public bool concurrent { get; init; } = true;
    public bool includeAnnotation { get; init; }
    public string properties { get; init; } = string.Empty;
    public string updatedTime { get; init; } = string.Empty;
    public int createType { get; init; }
    public string scriptCode { get; init; } = string.Empty;
}

public sealed record JobTriggerSaveInput
{
    public string id { get; init; } = string.Empty;
    public string triggerId { get; init; } = string.Empty;
    public string jobId { get; init; } = string.Empty;
    public string triggerType { get; init; } = JobClient.PeriodTrigger;
    public string assemblyName { get; init; } = "Furion.Pure";
    public string args { get; init; } = "1000";
    public string description { get; init; } = string.Empty;
    public int status { get; init; } = 1;
    public string? startTime { get; init; }
    public string? endTime { get; init; }
    public string? lastRunTime { get; init; }
    public string? nextRunTime { get; init; }
    public string numberOfRuns { get; init; } = "0";
    public string maxNumberOfRuns { get; init; } = "0";
    public string numberOfErrors { get; init; } = "0";
    public string maxNumberOfErrors { get; init; } = "0";
    public int numRetries { get; init; }
    public int retryTimeout { get; init; } = 1000;
    public bool startNow { get; init; } = true;
    public bool runOnStart { get; init; }
    public bool resetOnlyOnce { get; init; } = true;
    public string? updatedTime { get; init; }
}

public sealed record JobDashboardView
{
    public int totalJobs { get; init; }
    public int totalTriggers { get; init; }
    public int runningTriggerCount { get; init; }
    public int pausedTriggerCount { get; init; }
    public int abnormalTriggerCount { get; init; }
    public int clusterWorkingCount { get; init; }
    public int clusterWaitingCount { get; init; }
    public JobDashboardGroupView[] groupRows { get; init; } = [];
    public JobClusterView[] clusterRows { get; init; } = [];
    public JobView[] recentlyUpdatedJobs { get; init; } = [];
}

public sealed record JobHttpDraft
{
    public bool valid { get; init; }
    public string requestUri { get; init; } = string.Empty;
    public string method { get; init; } = "GET";
    public string body { get; init; } = string.Empty;
}

public sealed record JobTriggerArgs
{
    public string period { get; init; } = "1000";
    public string cron { get; init; } = string.Empty;
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

// The legacy typed table accepts VueProps. Jolt compiles the shared record declarations on their own.
public sealed partial record JobView : VueProps;
public sealed partial record JobTriggerView : VueProps;
public sealed partial record JobTriggerRecordView : VueProps;
public sealed partial record JobClusterView : VueProps;
public sealed partial record JobDashboardGroupView : VueProps;
