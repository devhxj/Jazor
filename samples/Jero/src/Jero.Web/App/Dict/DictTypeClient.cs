using Jero.Web.App.Http;
using static ECMAScript.Global;

namespace Jero.Web.App.Dict;

/// <summary>
/// Typed endpoint client for the complete dictionary management feature.
/// IDs are intentionally represented as strings at the browser boundary: Jazor lowers C#
/// <c>long</c> to JavaScript BigInt, which cannot be serialized by JSON.stringify.
/// </summary>
[ECMAScriptModule("components/dict-client.js")]
public static class DictTypeClient
{
    public static IPromise<ApiOutcome> Page(PageDictTypeInput input)
        => BrowserApiClient.Post("/api/sysDictType/page", input);

    public static IPromise<ApiOutcome> Add(DictTypeSaveInput input)
        => BrowserApiClient.Post("/api/sysDictType/add", new DictTypeAddInput
        {
            name = input.name,
            code = input.code,
            orderNo = input.orderNo,
            remark = input.remark,
            status = input.status,
            sysFlag = input.sysFlag,
            isTenant = input.isTenant
        });

    public static IPromise<ApiOutcome> Update(DictTypeSaveInput input)
        => BrowserApiClient.Post("/api/sysDictType/update", input);

    public static IPromise<ApiOutcome> Delete(string id)
        => BrowserApiClient.Post("/api/sysDictType/delete", new IdInput { id = id });

    public static IPromise<ApiOutcome> PageData(PageDictDataInput input)
        => BrowserApiClient.Post("/api/sysDictData/page", input);

    public static IPromise<ApiOutcome> AddData(DictDataSaveInput input)
        => BrowserApiClient.Post("/api/sysDictData/add", new DictDataAddInput
        {
            dictTypeId = input.dictTypeId,
            label = input.label,
            value = input.value,
            code = input.code,
            name = input.name,
            tagType = input.tagType,
            styleSetting = input.styleSetting,
            classSetting = input.classSetting,
            orderNo = input.orderNo,
            remark = input.remark,
            extData = input.extData,
            status = input.status
        });

    public static IPromise<ApiOutcome> UpdateData(DictDataSaveInput input)
        => BrowserApiClient.Post("/api/sysDictData/update", input);

    public static IPromise<ApiOutcome> DeleteData(string id)
        => BrowserApiClient.Post("/api/sysDictData/delete", new IdInput { id = id });

    public static DictTypeView[] ReadTypeList(object? data)
    {
        var values = ReadItems(data);
        var rows = new DictTypeView[values.Length];
        for (var index = 0; index < values.Length; index++)
            rows[index] = ReadType(values[index]!);
        return rows;
    }

    public static DictDataView[] ReadDataList(object? data)
    {
        var values = ReadItems(data);
        var rows = new DictDataView[values.Length];
        for (var index = 0; index < values.Length; index++)
            rows[index] = ReadData(values[index]!);
        return rows;
    }

    public static int ReadTotal(object? data)
        => data is null ? 0 : (int)NumberValue(Reflect.Get(data, "total"));

    private static object?[] ReadItems(object? data)
        => (data is null ? null : Reflect.Get(data, "items")) as object?[] ?? [];

    private static DictTypeView ReadType(object value)
        => new()
        {
            id = ReadString(value, "id"),
            name = ReadString(value, "name"),
            code = ReadString(value, "code"),
            orderNo = ReadInt(value, "orderNo", 100),
            remark = ReadOptionalString(value, "remark"),
            status = ReadInt(value, "status", 1),
            sysFlag = ReadInt(value, "sysFlag", 2),
            isTenant = ReadInt(value, "isTenant", 2)
        };

    private static DictDataView ReadData(object value)
        => new()
        {
            id = ReadString(value, "id"),
            dictTypeId = ReadString(value, "dictTypeId"),
            label = ReadString(value, "label"),
            value = ReadString(value, "value"),
            code = ReadOptionalString(value, "code"),
            name = ReadOptionalString(value, "name"),
            tagType = ReadOptionalString(value, "tagType"),
            styleSetting = ReadOptionalString(value, "styleSetting"),
            classSetting = ReadOptionalString(value, "classSetting"),
            orderNo = ReadInt(value, "orderNo", 100),
            remark = ReadOptionalString(value, "remark"),
            extData = ReadOptionalString(value, "extData"),
            status = ReadInt(value, "status", 1)
        };

    private static string ReadString(object data, string key)
    {
        var value = Reflect.Get(data, key);
        return value is null ? string.Empty : StringValue(value);
    }

    private static string? ReadOptionalString(object data, string key)
    {
        var value = Reflect.Get(data, key);
        return value is null ? null : StringValue(value);
    }

    private static int ReadInt(object data, string key, int fallback)
    {
        var value = Reflect.Get(data, key);
        return value is null ? fallback : (int)NumberValue(value);
    }
}

public sealed record PageDictTypeInput
{
    public int page { get; init; } = 1;
    public int pageSize { get; init; } = 50;
    public string field { get; init; } = "orderNo";
    public string order { get; init; } = "ascending";
    public string descStr { get; init; } = "descending";
    public string name { get; init; } = string.Empty;
    public string code { get; init; } = string.Empty;
}

public sealed record PageDictDataInput
{
    public int page { get; init; } = 1;
    public int pageSize { get; init; } = 50;
    public string field { get; init; } = "orderNo";
    public string order { get; init; } = "ascending";
    public string descStr { get; init; } = "descending";
    public string dictTypeId { get; init; } = string.Empty;
    public string label { get; init; } = string.Empty;
    public string code { get; init; } = string.Empty;
}

public sealed record DictTypeSaveInput
{
    public string id { get; init; } = string.Empty;
    public string name { get; init; } = string.Empty;
    public string code { get; init; } = string.Empty;
    public int orderNo { get; init; } = 100;
    public string remark { get; init; } = string.Empty;
    public int status { get; init; } = 1;
    public int sysFlag { get; init; } = 2;
    public int isTenant { get; init; } = 2;
}

public sealed record DictTypeAddInput
{
    public string name { get; init; } = string.Empty;
    public string code { get; init; } = string.Empty;
    public int orderNo { get; init; } = 100;
    public string remark { get; init; } = string.Empty;
    public int status { get; init; } = 1;
    public int sysFlag { get; init; } = 2;
    public int isTenant { get; init; } = 2;
}

public sealed record DictDataSaveInput
{
    public string id { get; init; } = string.Empty;
    public string dictTypeId { get; init; } = string.Empty;
    public string label { get; init; } = string.Empty;
    public string value { get; init; } = string.Empty;
    public string code { get; init; } = string.Empty;
    public string name { get; init; } = string.Empty;
    public string tagType { get; init; } = "primary";
    public string styleSetting { get; init; } = string.Empty;
    public string classSetting { get; init; } = string.Empty;
    public int orderNo { get; init; } = 100;
    public string remark { get; init; } = string.Empty;
    public string extData { get; init; } = string.Empty;
    public int status { get; init; } = 1;
}

public sealed record DictDataAddInput
{
    public string dictTypeId { get; init; } = string.Empty;
    public string label { get; init; } = string.Empty;
    public string value { get; init; } = string.Empty;
    public string code { get; init; } = string.Empty;
    public string name { get; init; } = string.Empty;
    public string tagType { get; init; } = "primary";
    public string styleSetting { get; init; } = string.Empty;
    public string classSetting { get; init; } = string.Empty;
    public int orderNo { get; init; } = 100;
    public string remark { get; init; } = string.Empty;
    public string extData { get; init; } = string.Empty;
    public int status { get; init; } = 1;
}

public sealed record IdInput
{
    public string id { get; init; } = string.Empty;
}

public sealed partial record DictTypeView : VueProps;

public sealed partial record DictDataView : VueProps;
