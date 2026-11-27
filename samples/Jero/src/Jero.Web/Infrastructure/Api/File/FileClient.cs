using Jero.Web.Application;
using Jero.Web.Infrastructure.Http;
using static ECMAScript.Global;

namespace Jero.Web.Infrastructure.Api.File;

/// <summary>查询文件和目录，并更新或删除文件记录。</summary>
[ECMAScriptModule("components/file-client.js")]
public static class FileClient
{
    public static IPromise<ApiOutcome> Page(FilePageInput input)
        => BrowserApiClient.Post("/api/sysFile/page", input);

    public static IPromise<ApiOutcome> Folders()
        => BrowserApiClient.Get("/api/sysFile/folder");

    public static IPromise<ApiOutcome> Update(FileUpdateInput input)
        => BrowserApiClient.Post("/api/sysFile/update", input);

    public static IPromise<ApiOutcome> Delete(string id)
        => BrowserApiClient.Post("/api/sysFile/delete", new FileIdInput { Id = id });

    public static FileView[] ReadItems(object? data)
    {
        var values = (data is null ? null : Reflect.Get(data, "items")) as object?[] ?? [];
        var result = new FileView[values.Length];
        for (var index = 0; index < values.Length; index++)
            result[index] = ReadFile(values[index]!);
        return result;
    }

    public static FileView? ReadUploadedFile(object? data)
        => data is null ? null : ReadFile(data);

    public static int ReadTotal(object? data)
        => data is null ? 0 : ReadInt(data, "total", 0);

    public static FileFolderView[] ReadFolders(object? data)
    {
        var roots = data as object?[] ?? [];
        var result = new FileFolderView[CountFolders(roots)];
        var index = 0;
        AppendFolders(roots, string.Empty, 0, result, ref index);
        return result;
    }

    private static FileView ReadFile(object value)
        => new()
        {
            Id = ReadString(value, "id"),
            CreateTime = ReadString(value, "createTime"),
            UpdateTime = ReadNullableString(value, "updateTime"),
            CreateUserId = ReadNullableString(value, "createUserId"),
            CreateUserName = ReadNullableString(value, "createUserName"),
            UpdateUserId = ReadNullableString(value, "updateUserId"),
            UpdateUserName = ReadNullableString(value, "updateUserName"),
            OrgId = ReadString(value, "orgId"),
            TenantId = ReadNullableString(value, "tenantId"),
            Provider = ReadNullableString(value, "provider"),
            BucketName = ReadNullableString(value, "bucketName"),
            FileName = ReadNullableString(value, "fileName"),
            Suffix = ReadNullableString(value, "suffix"),
            FilePath = ReadNullableString(value, "filePath"),
            SizeKb = ReadString(value, "sizeKb"),
            SizeInfo = ReadNullableString(value, "sizeInfo"),
            Url = ReadNullableString(value, "url"),
            FileMd5 = ReadNullableString(value, "fileMd5"),
            FileType = ReadNullableString(value, "fileType"),
            FileAlias = ReadNullableString(value, "fileAlias"),
            IsPublic = ReadBool(value, "isPublic"),
            DataId = ReadNullableString(value, "dataId")
        };

    private static int CountFolders(object?[] values)
    {
        var count = values.Length;
        for (var index = 0; index < values.Length; index++)
            count += CountFolders(ReadChildren(values[index]!));
        return count;
    }

    private static void AppendFolders(
        object?[] values,
        string parentPath,
        int depth,
        FileFolderView[] result,
        ref int resultIndex)
    {
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index]!;
            var label = NormalizeFolder(ReadString(value, "name"));
            if (label.Length == 0)
                label = "未命名目录";
            var fullPath = parentPath.Length == 0 ? label : parentPath + "/" + label;
            result[resultIndex] = new FileFolderView
            {
                Label = label,
                FullPath = fullPath,
                Depth = depth
            };
            resultIndex++;
            AppendFolders(ReadChildren(value), fullPath, depth + 1, result, ref resultIndex);
        }
    }

    private static object?[] ReadChildren(object value)
        => Reflect.Get(value, "children") as object?[] ?? [];

    private static string NormalizeFolder(string value)
    {
        var normalized = value.Trim().Replace("\\", "/");
        while (normalized.StartsWith('/'))
            normalized = normalized.Substring(1);
        while (normalized.EndsWith('/'))
            normalized = normalized.Substring(0, normalized.Length - 1);
        return normalized;
    }

    private static string ReadString(object data, string key)
        => ReadNullableString(data, key) ?? string.Empty;

    private static string? ReadNullableString(object data, string key)
    {
        var value = Reflect.Get(data, key);
        if (value is null)
            return null;
        var result = StringValue(value);
        return result == "null" || result == "undefined" ? null : result;
    }

    private static bool ReadBool(object data, string key)
    {
        var value = Reflect.Get(data, key);
        return value is not null && BooleanValue(value);
    }

    private static int ReadInt(object data, string key, int fallback)
    {
        var value = Reflect.Get(data, key);
        return value is null ? fallback : (int)NumberValue(value);
    }
}

public sealed record FilePageInput
{
    [ECMAScriptName("page")]
    public int Page { get; init; } = 1;
    [ECMAScriptName("pageSize")]
    public int PageSize { get; init; } = 50;
    [ECMAScriptName("fileName")]
    public string FileName { get; init; } = string.Empty;
    [ECMAScriptName("filePath")]
    public string FilePath { get; init; } = string.Empty;
    [ECMAScriptName("startTime")]
    public string? StartTime { get; init; }
    [ECMAScriptName("endTime")]
    public string? EndTime { get; init; }
}

public sealed record FileIdInput
{
    [ECMAScriptName("id")]
    public string Id { get; init; } = string.Empty;
}

/// <summary>
/// 更新接口覆盖完整的 SysFile 实体。表单只编辑部分元数据，提交时仍需带回存储和审计字段。
/// </summary>
public sealed record FileUpdateInput
{
    [ECMAScriptName("id")]
    public string Id { get; init; } = string.Empty;
    [ECMAScriptName("createTime")]
    public string CreateTime { get; init; } = string.Empty;
    [ECMAScriptName("updateTime")]
    public string? UpdateTime { get; init; }
    [ECMAScriptName("createUserId")]
    public string? CreateUserId { get; init; }
    [ECMAScriptName("createUserName")]
    public string? CreateUserName { get; init; }
    [ECMAScriptName("updateUserId")]
    public string? UpdateUserId { get; init; }
    [ECMAScriptName("updateUserName")]
    public string? UpdateUserName { get; init; }
    [ECMAScriptName("orgId")]
    public string OrgId { get; init; } = string.Empty;
    [ECMAScriptName("tenantId")]
    public string? TenantId { get; init; }
    [ECMAScriptName("provider")]
    public string? Provider { get; init; }
    [ECMAScriptName("bucketName")]
    public string? BucketName { get; init; }
    [ECMAScriptName("fileName")]
    public string? FileName { get; init; }
    [ECMAScriptName("suffix")]
    public string? Suffix { get; init; }
    [ECMAScriptName("filePath")]
    public string? FilePath { get; init; }
    [ECMAScriptName("sizeKb")]
    public string SizeKb { get; init; } = "0";
    [ECMAScriptName("sizeInfo")]
    public string? SizeInfo { get; init; }
    [ECMAScriptName("url")]
    public string? Url { get; init; }
    [ECMAScriptName("fileMd5")]
    public string? FileMd5 { get; init; }
    [ECMAScriptName("fileType")]
    public string? FileType { get; init; }
    [ECMAScriptName("fileAlias")]
    public string? FileAlias { get; init; }
    [ECMAScriptName("isPublic")]
    public bool IsPublic { get; init; }
    [ECMAScriptName("dataId")]
    public string? DataId { get; init; }
}
