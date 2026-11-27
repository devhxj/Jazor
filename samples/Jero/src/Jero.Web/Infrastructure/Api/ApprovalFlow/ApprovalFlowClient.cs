using Jero.Web.Infrastructure.Http;
using static ECMAScript.Global;

namespace Jero.Web.Infrastructure.Api.ApprovalFlow;

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
        => BrowserApiClient.Post("/api/approvalFlow/delete", new ApprovalFlowIdInput { Id = id });

    public static IPromise<ApiOutcome> FormRoutes()
        => BrowserApiClient.Get("/api/approvalFlow/formRoutes");

    public static IPromise<ApiOutcome> Databases()
        => BrowserApiClient.Get("/api/sysDatabase/list");

    public static IPromise<ApiOutcome> Tables(string configId)
        => BrowserApiClient.Get("/api/sysDatabase/entityTables/" + EncodeURIComponent(configId));

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
                ConfigId = ReadString(value, "configId"),
                DbNickName = ReadString(value, "dbNickName")
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
                TableName = ReadString(value, "tableName"),
                TableComment = ReadString(value, "tableComment"),
                EntityName = ReadString(value, "entityName")
            };
        }
        return result;
    }

    private static ApprovalFlowView ReadItem(object value)
        => new()
        {
            Id = ReadString(value, "id"),
            Code = ReadString(value, "code"),
            Name = ReadString(value, "name"),
            FormJson = ReadString(value, "formJson"),
            FlowJson = ReadString(value, "flowJson"),
            Status = ReadInt(value, "status", -1),
            Remark = ReadString(value, "remark"),
            IsDelete = ReadBool(value, "isDelete"),
            CreateTime = ReadString(value, "createTime"),
            CreateUserId = ReadString(value, "createUserId"),
            CreateUserName = ReadString(value, "createUserName"),
            CreateOrgId = ReadString(value, "createOrgId"),
            CreateOrgName = ReadString(value, "createOrgName"),
            UpdateTime = ReadString(value, "updateTime"),
            UpdateUserId = ReadString(value, "updateUserId"),
            UpdateUserName = ReadString(value, "updateUserName"),
            UpdateOrgId = ReadString(value, "updateOrgId"),
            UpdateOrgName = ReadString(value, "updateOrgName")
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
    [ECMAScriptName("page")]
    public int Page { get; init; } = 1;
    [ECMAScriptName("pageSize")]
    public int PageSize { get; init; } = 50;
    [ECMAScriptName("keyword")]
    public string Keyword { get; init; } = string.Empty;
    [ECMAScriptName("code")]
    public string Code { get; init; } = string.Empty;
    [ECMAScriptName("name")]
    public string Name { get; init; } = string.Empty;
    [ECMAScriptName("remark")]
    public string Remark { get; init; } = string.Empty;
}

public sealed record ApprovalFlowAddInput
{
    [ECMAScriptName("code")]
    public string? Code { get; init; }
    [ECMAScriptName("name")]
    public string Name { get; init; } = string.Empty;
    [ECMAScriptName("formJson")]
    public string? FormJson { get; init; }
    [ECMAScriptName("flowJson")]
    public string? FlowJson { get; init; }
    [ECMAScriptName("remark")]
    public string? Remark { get; init; }
    [ECMAScriptName("isDelete")]
    public bool IsDelete { get; init; }
}

/// <summary>
/// 更新以 detail 完整快照为底稿，只投影后端 UpdateApprovalFlowInput 明确接收的字段。
/// </summary>
public sealed record ApprovalFlowUpdateInput
{
    [ECMAScriptName("id")]
    public string Id { get; init; } = string.Empty;
    [ECMAScriptName("code")]
    public string? Code { get; init; }
    [ECMAScriptName("name")]
    public string Name { get; init; } = string.Empty;
    [ECMAScriptName("formJson")]
    public string? FormJson { get; init; }
    [ECMAScriptName("flowJson")]
    public string? FlowJson { get; init; }
    [ECMAScriptName("remark")]
    public string? Remark { get; init; }
    [ECMAScriptName("isDelete")]
    public bool IsDelete { get; init; }
    [ECMAScriptName("createTime")]
    public string? CreateTime { get; init; }
    [ECMAScriptName("createUserId")]
    public string? CreateUserId { get; init; }
    [ECMAScriptName("createUserName")]
    public string? CreateUserName { get; init; }
    [ECMAScriptName("createOrgId")]
    public string? CreateOrgId { get; init; }
    [ECMAScriptName("createOrgName")]
    public string? CreateOrgName { get; init; }
    [ECMAScriptName("updateTime")]
    public string? UpdateTime { get; init; }
    [ECMAScriptName("updateUserId")]
    public string? UpdateUserId { get; init; }
    [ECMAScriptName("updateUserName")]
    public string? UpdateUserName { get; init; }
}

public sealed record ApprovalFlowIdInput
{
    [ECMAScriptName("id")]
    public string Id { get; init; } = string.Empty;
}

public sealed record ApprovalFlowView : VueProps
{
    public string Id { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string FormJson { get; init; } = string.Empty;
    public string FlowJson { get; init; } = string.Empty;
    public int Status { get; init; } = -1;
    public string Remark { get; init; } = string.Empty;
    public bool IsDelete { get; init; }
    public string CreateTime { get; init; } = string.Empty;
    public string CreateUserId { get; init; } = string.Empty;
    public string CreateUserName { get; init; } = string.Empty;
    public string CreateOrgId { get; init; } = string.Empty;
    public string CreateOrgName { get; init; } = string.Empty;
    public string UpdateTime { get; init; } = string.Empty;
    public string UpdateUserId { get; init; } = string.Empty;
    public string UpdateUserName { get; init; } = string.Empty;
    public string UpdateOrgId { get; init; } = string.Empty;
    public string UpdateOrgName { get; init; } = string.Empty;
}

public sealed record ApprovalDatabaseOption : VueProps
{
    public string ConfigId { get; init; } = string.Empty;
    public string DbNickName { get; init; } = string.Empty;
}

public sealed record ApprovalTableOption : VueProps
{
    public string TableName { get; init; } = string.Empty;
    public string TableComment { get; init; } = string.Empty;
    public string EntityName { get; init; } = string.Empty;
}
