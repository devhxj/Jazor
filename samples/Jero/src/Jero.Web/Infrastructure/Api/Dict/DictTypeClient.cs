using Jero.Web.Application;
using Jero.Web.Infrastructure.Http;
using static ECMAScript.Global;

namespace Jero.Web.Infrastructure.Api.Dict;

/// <summary>
/// 分别查询和维护字典类型与字典数据。标识符以字符串传给 JSON 序列化，避免转换为 JavaScript Number 时丢失精度。
/// </summary>
[ECMAScriptModule("components/dict-client.js")]
public static class DictTypeClient
{
    public static IPromise<ApiOutcome> Page(PageDictTypeInput input)
        => BrowserApiClient.Post("/api/sysDictType/page", input);

    public static IPromise<ApiOutcome> Add(DictTypeSaveInput input)
        => BrowserApiClient.Post("/api/sysDictType/add", new DictTypeAddInput
        {
            Name = input.Name,
            Code = input.Code,
            OrderNo = input.OrderNo,
            Remark = input.Remark,
            Status = input.Status,
            SysFlag = input.SysFlag,
            IsTenant = input.IsTenant
        });

    public static IPromise<ApiOutcome> Update(DictTypeSaveInput input)
        => BrowserApiClient.Post("/api/sysDictType/update", input);

    public static IPromise<ApiOutcome> Delete(string id)
        => BrowserApiClient.Post("/api/sysDictType/delete", new IdInput { Id = id });

    public static IPromise<ApiOutcome> PageData(PageDictDataInput input)
        => BrowserApiClient.Post("/api/sysDictData/page", input);

    public static IPromise<ApiOutcome> AddData(DictDataSaveInput input)
        => BrowserApiClient.Post("/api/sysDictData/add", new DictDataAddInput
        {
            DictTypeId = input.DictTypeId,
            Label = input.Label,
            Value = input.Value,
            Code = input.Code,
            Name = input.Name,
            TagType = input.TagType,
            StyleSetting = input.StyleSetting,
            ClassSetting = input.ClassSetting,
            OrderNo = input.OrderNo,
            Remark = input.Remark,
            ExtData = input.ExtData,
            Status = input.Status
        });

    public static IPromise<ApiOutcome> UpdateData(DictDataSaveInput input)
        => BrowserApiClient.Post("/api/sysDictData/update", input);

    public static IPromise<ApiOutcome> DeleteData(string id)
        => BrowserApiClient.Post("/api/sysDictData/delete", new IdInput { Id = id });

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
            Id = ReadString(value, "id"),
            Name = ReadString(value, "name"),
            Code = ReadString(value, "code"),
            OrderNo = ReadInt(value, "orderNo", 100),
            Remark = ReadOptionalString(value, "remark"),
            Status = ReadInt(value, "status", 1),
            SysFlag = ReadInt(value, "sysFlag", 2),
            IsTenant = ReadInt(value, "isTenant", 2)
        };

    private static DictDataView ReadData(object value)
        => new()
        {
            Id = ReadString(value, "id"),
            DictTypeId = ReadString(value, "dictTypeId"),
            Label = ReadString(value, "label"),
            Value = ReadString(value, "value"),
            Code = ReadOptionalString(value, "code"),
            Name = ReadOptionalString(value, "name"),
            TagType = ReadOptionalString(value, "tagType"),
            StyleSetting = ReadOptionalString(value, "styleSetting"),
            ClassSetting = ReadOptionalString(value, "classSetting"),
            OrderNo = ReadInt(value, "orderNo", 100),
            Remark = ReadOptionalString(value, "remark"),
            ExtData = ReadOptionalString(value, "extData"),
            Status = ReadInt(value, "status", 1)
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
    [ECMAScriptName("page")]
    public int Page { get; init; } = 1;
    [ECMAScriptName("pageSize")]
    public int PageSize { get; init; } = 50;
    [ECMAScriptName("field")]
    public string Field { get; init; } = "orderNo";
    [ECMAScriptName("order")]
    public string Order { get; init; } = "ascending";
    [ECMAScriptName("descStr")]
    public string DescStr { get; init; } = "descending";
    [ECMAScriptName("name")]
    public string Name { get; init; } = string.Empty;
    [ECMAScriptName("code")]
    public string Code { get; init; } = string.Empty;
}

public sealed record PageDictDataInput
{
    [ECMAScriptName("page")]
    public int Page { get; init; } = 1;
    [ECMAScriptName("pageSize")]
    public int PageSize { get; init; } = 50;
    [ECMAScriptName("field")]
    public string Field { get; init; } = "orderNo";
    [ECMAScriptName("order")]
    public string Order { get; init; } = "ascending";
    [ECMAScriptName("descStr")]
    public string DescStr { get; init; } = "descending";
    [ECMAScriptName("dictTypeId")]
    public string DictTypeId { get; init; } = string.Empty;
    [ECMAScriptName("label")]
    public string Label { get; init; } = string.Empty;
    [ECMAScriptName("code")]
    public string Code { get; init; } = string.Empty;
}

public sealed record DictTypeSaveInput
{
    [ECMAScriptName("id")]
    public string Id { get; init; } = string.Empty;
    [ECMAScriptName("name")]
    public string Name { get; init; } = string.Empty;
    [ECMAScriptName("code")]
    public string Code { get; init; } = string.Empty;
    [ECMAScriptName("orderNo")]
    public int OrderNo { get; init; } = 100;
    [ECMAScriptName("remark")]
    public string Remark { get; init; } = string.Empty;
    [ECMAScriptName("status")]
    public int Status { get; init; } = 1;
    [ECMAScriptName("sysFlag")]
    public int SysFlag { get; init; } = 2;
    [ECMAScriptName("isTenant")]
    public int IsTenant { get; init; } = 2;
}

public sealed record DictTypeAddInput
{
    [ECMAScriptName("name")]
    public string Name { get; init; } = string.Empty;
    [ECMAScriptName("code")]
    public string Code { get; init; } = string.Empty;
    [ECMAScriptName("orderNo")]
    public int OrderNo { get; init; } = 100;
    [ECMAScriptName("remark")]
    public string Remark { get; init; } = string.Empty;
    [ECMAScriptName("status")]
    public int Status { get; init; } = 1;
    [ECMAScriptName("sysFlag")]
    public int SysFlag { get; init; } = 2;
    [ECMAScriptName("isTenant")]
    public int IsTenant { get; init; } = 2;
}

public sealed record DictDataSaveInput
{
    [ECMAScriptName("id")]
    public string Id { get; init; } = string.Empty;
    [ECMAScriptName("dictTypeId")]
    public string DictTypeId { get; init; } = string.Empty;
    [ECMAScriptName("label")]
    public string Label { get; init; } = string.Empty;
    [ECMAScriptName("value")]
    public string Value { get; init; } = string.Empty;
    [ECMAScriptName("code")]
    public string Code { get; init; } = string.Empty;
    [ECMAScriptName("name")]
    public string Name { get; init; } = string.Empty;
    [ECMAScriptName("tagType")]
    public string TagType { get; init; } = "primary";
    [ECMAScriptName("styleSetting")]
    public string StyleSetting { get; init; } = string.Empty;
    [ECMAScriptName("classSetting")]
    public string ClassSetting { get; init; } = string.Empty;
    [ECMAScriptName("orderNo")]
    public int OrderNo { get; init; } = 100;
    [ECMAScriptName("remark")]
    public string Remark { get; init; } = string.Empty;
    [ECMAScriptName("extData")]
    public string ExtData { get; init; } = string.Empty;
    [ECMAScriptName("status")]
    public int Status { get; init; } = 1;
}

public sealed record DictDataAddInput
{
    [ECMAScriptName("dictTypeId")]
    public string DictTypeId { get; init; } = string.Empty;
    [ECMAScriptName("label")]
    public string Label { get; init; } = string.Empty;
    [ECMAScriptName("value")]
    public string Value { get; init; } = string.Empty;
    [ECMAScriptName("code")]
    public string Code { get; init; } = string.Empty;
    [ECMAScriptName("name")]
    public string Name { get; init; } = string.Empty;
    [ECMAScriptName("tagType")]
    public string TagType { get; init; } = "primary";
    [ECMAScriptName("styleSetting")]
    public string StyleSetting { get; init; } = string.Empty;
    [ECMAScriptName("classSetting")]
    public string ClassSetting { get; init; } = string.Empty;
    [ECMAScriptName("orderNo")]
    public int OrderNo { get; init; } = 100;
    [ECMAScriptName("remark")]
    public string Remark { get; init; } = string.Empty;
    [ECMAScriptName("extData")]
    public string ExtData { get; init; } = string.Empty;
    [ECMAScriptName("status")]
    public int Status { get; init; } = 1;
}

public sealed record IdInput
{
    [ECMAScriptName("id")]
    public string Id { get; init; } = string.Empty;
}
