using Jero.Web.App.Http;
using static ECMAScript.Global;

namespace Jero.Web.App.ApprovalFlow;

/// <summary>审批流程页面的同源 HTTP 边界，所有雪花 ID 均保持 JSON string。</summary>
[ECMAScriptModule("components/approval-flow-client.js")]
public static class ApprovalFlowClient
{
    public static IPromise<ApiOutcome> Page(ApprovalFlowPageInput input)
        => BrowserApiClient.Post("/api/approvalFlow/page", input);

    public static IPromise<ApiOutcome> Detail(string id)
        => BrowserApiClient.Get("/api/approvalFlow/detail?id=" + EncodeURIComponent(id));

    public static IPromise<ApiOutcome> Add(ApprovalFlowAddInput input)
        => BrowserApiClient.Post("/api/approvalFlow/add", input);

    public static IPromise<ApiOutcome> Update(ApprovalFlowUpdateInput input)
        => BrowserApiClient.Post("/api/approvalFlow/update", input);

    public static IPromise<ApiOutcome> Delete(string id)
        => BrowserApiClient.Post("/api/approvalFlow/delete", new ApprovalFlowIdInput { id = id });

    public static IPromise<ApiOutcome> FormRoutes()
        => BrowserApiClient.Get("/api/approvalFlow/formRoutes");

    public static IPromise<ApiOutcome> Databases()
        => BrowserApiClient.Get("/api/sysCodeGen/databaseList");

    public static IPromise<ApiOutcome> Tables(string configId)
        => BrowserApiClient.Get("/api/sysCodeGen/tableList/" + EncodeURIComponent(configId));

    public static ApprovalFlowView[] ReadItems(object? data)
    {
        var values = (data is null ? null : Reflect.Get(data, "items")) as object?[] ?? [];
        var result = new ApprovalFlowView[values.Length];
        for (var index = 0; index < values.Length; index++)
            result[index] = ReadItem(values[index]!);
        return result;
    }

    public static int ReadTotal(object? data)
        => data is null ? 0 : ReadInt(data, "total", 0);

    public static ApprovalFlowView? ReadDetail(object? data)
        => data is null ? null : ReadItem(data);

    public static string[] ReadStrings(object? data)
    {
        var values = data as object?[] ?? [];
        var result = new string[values.Length];
        for (var index = 0; index < values.Length; index++)
            result[index] = values[index] is null ? string.Empty : StringValue(values[index]!);
        return result;
    }

    public static ApprovalDatabaseOption[] ReadDatabases(object? data)
    {
        var values = data as object?[] ?? [];
        var result = new ApprovalDatabaseOption[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index]!;
            result[index] = new ApprovalDatabaseOption
            {
                configId = ReadString(value, "configId"),
                dbNickName = ReadString(value, "dbNickName")
            };
        }
        return result;
    }

    public static ApprovalTableOption[] ReadTables(object? data)
    {
        var values = data as object?[] ?? [];
        var result = new ApprovalTableOption[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index]!;
            result[index] = new ApprovalTableOption
            {
                tableName = ReadString(value, "tableName"),
                tableComment = ReadString(value, "tableComment"),
                entityName = ReadString(value, "entityName")
            };
        }
        return result;
    }

    private static ApprovalFlowView ReadItem(object value)
        => new()
        {
            id = ReadString(value, "id"),
            code = ReadString(value, "code"),
            name = ReadString(value, "name"),
            formJson = ReadString(value, "formJson"),
            flowJson = ReadString(value, "flowJson"),
            status = ReadInt(value, "status", -1),
            remark = ReadString(value, "remark"),
            isDelete = ReadBool(value, "isDelete"),
            createTime = ReadString(value, "createTime"),
            createUserId = ReadString(value, "createUserId"),
            createUserName = ReadString(value, "createUserName"),
            createOrgId = ReadString(value, "createOrgId"),
            createOrgName = ReadString(value, "createOrgName"),
            updateTime = ReadString(value, "updateTime"),
            updateUserId = ReadString(value, "updateUserId"),
            updateUserName = ReadString(value, "updateUserName"),
            updateOrgId = ReadString(value, "updateOrgId"),
            updateOrgName = ReadString(value, "updateOrgName")
        };

    private static string ReadString(object data, string key)
    {
        var value = Reflect.Get(data, key);
        if (value is null)
            return string.Empty;
        var result = StringValue(value);
        return result == "null" || result == "undefined" ? string.Empty : result;
    }

    private static int ReadInt(object data, string key, int fallback)
    {
        var value = Reflect.Get(data, key);
        return value is null ? fallback : (int)NumberValue(value);
    }

    private static bool ReadBool(object data, string key)
    {
        var value = Reflect.Get(data, key);
        return value is not null && BooleanValue(value);
    }
}

public sealed record ApprovalFlowPageInput
{
    public int page { get; init; } = 1;
    public int pageSize { get; init; } = 50;
    public string keyword { get; init; } = string.Empty;
    public string code { get; init; } = string.Empty;
    public string name { get; init; } = string.Empty;
    public string remark { get; init; } = string.Empty;
}

public sealed record ApprovalFlowAddInput
{
    public string? code { get; init; }
    public string name { get; init; } = string.Empty;
    public string? formJson { get; init; }
    public string? flowJson { get; init; }
    public string? remark { get; init; }
    public bool isDelete { get; init; }
}

/// <summary>
/// 更新以 detail 完整快照为底稿，只投影后端 UpdateApprovalFlowInput 明确接收的字段。
/// </summary>
public sealed record ApprovalFlowUpdateInput
{
    public string id { get; init; } = string.Empty;
    public string? code { get; init; }
    public string name { get; init; } = string.Empty;
    public string? formJson { get; init; }
    public string? flowJson { get; init; }
    public string? remark { get; init; }
    public bool isDelete { get; init; }
    public string? createTime { get; init; }
    public string? createUserId { get; init; }
    public string? createUserName { get; init; }
    public string? createOrgId { get; init; }
    public string? createOrgName { get; init; }
    public string? updateTime { get; init; }
    public string? updateUserId { get; init; }
    public string? updateUserName { get; init; }
}

public sealed record ApprovalFlowIdInput
{
    public string id { get; init; } = string.Empty;
}

public sealed record ApprovalFlowView : VueProps
{
    public string id { get; init; } = string.Empty;
    public string code { get; init; } = string.Empty;
    public string name { get; init; } = string.Empty;
    public string formJson { get; init; } = string.Empty;
    public string flowJson { get; init; } = string.Empty;
    public int status { get; init; } = -1;
    public string remark { get; init; } = string.Empty;
    public bool isDelete { get; init; }
    public string createTime { get; init; } = string.Empty;
    public string createUserId { get; init; } = string.Empty;
    public string createUserName { get; init; } = string.Empty;
    public string createOrgId { get; init; } = string.Empty;
    public string createOrgName { get; init; } = string.Empty;
    public string updateTime { get; init; } = string.Empty;
    public string updateUserId { get; init; } = string.Empty;
    public string updateUserName { get; init; } = string.Empty;
    public string updateOrgId { get; init; } = string.Empty;
    public string updateOrgName { get; init; } = string.Empty;
}

public sealed record ApprovalDatabaseOption : VueProps
{
    public string configId { get; init; } = string.Empty;
    public string dbNickName { get; init; } = string.Empty;
}

public sealed record ApprovalTableOption : VueProps
{
    public string tableName { get; init; } = string.Empty;
    public string tableComment { get; init; } = string.Empty;
    public string entityName { get; init; } = string.Empty;
}
