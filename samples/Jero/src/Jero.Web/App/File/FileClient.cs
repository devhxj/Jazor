using Jero.Web.App.Http;
using static ECMAScript.Global;

namespace Jero.Web.App.File;

/// <summary>Typed API boundary and JSON projection for system file management.</summary>
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
        => BrowserApiClient.Post("/api/sysFile/delete", new FileIdInput { id = id });

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
            id = ReadString(value, "id"),
            createTime = ReadString(value, "createTime"),
            updateTime = ReadNullableString(value, "updateTime"),
            createUserId = ReadNullableString(value, "createUserId"),
            createUserName = ReadNullableString(value, "createUserName"),
            updateUserId = ReadNullableString(value, "updateUserId"),
            updateUserName = ReadNullableString(value, "updateUserName"),
            orgId = ReadString(value, "orgId"),
            tenantId = ReadNullableString(value, "tenantId"),
            provider = ReadNullableString(value, "provider"),
            bucketName = ReadNullableString(value, "bucketName"),
            fileName = ReadNullableString(value, "fileName"),
            suffix = ReadNullableString(value, "suffix"),
            filePath = ReadNullableString(value, "filePath"),
            sizeKb = ReadString(value, "sizeKb"),
            sizeInfo = ReadNullableString(value, "sizeInfo"),
            url = ReadNullableString(value, "url"),
            fileMd5 = ReadNullableString(value, "fileMd5"),
            fileType = ReadNullableString(value, "fileType"),
            fileAlias = ReadNullableString(value, "fileAlias"),
            isPublic = ReadBool(value, "isPublic"),
            dataId = ReadNullableString(value, "dataId")
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
                label = label,
                fullPath = fullPath,
                depth = depth
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
    public int page { get; init; } = 1;
    public int pageSize { get; init; } = 50;
    public string fileName { get; init; } = string.Empty;
    public string filePath { get; init; } = string.Empty;
    public string? startTime { get; init; }
    public string? endTime { get; init; }
}

public sealed record FileIdInput
{
    public string id { get; init; } = string.Empty;
}

/// <summary>
/// Full SysFile update contract. The service updates the complete entity, so every storage and
/// audit field must round-trip even though the form exposes only editable metadata.
/// </summary>
public sealed record FileUpdateInput
{
    public string id { get; init; } = string.Empty;
    public string createTime { get; init; } = string.Empty;
    public string? updateTime { get; init; }
    public string? createUserId { get; init; }
    public string? createUserName { get; init; }
    public string? updateUserId { get; init; }
    public string? updateUserName { get; init; }
    public string orgId { get; init; } = string.Empty;
    public string? tenantId { get; init; }
    public string? provider { get; init; }
    public string? bucketName { get; init; }
    public string? fileName { get; init; }
    public string? suffix { get; init; }
    public string? filePath { get; init; }
    public string sizeKb { get; init; } = "0";
    public string? sizeInfo { get; init; }
    public string? url { get; init; }
    public string? fileMd5 { get; init; }
    public string? fileType { get; init; }
    public string? fileAlias { get; init; }
    public bool isPublic { get; init; }
    public string? dataId { get; init; }
}
