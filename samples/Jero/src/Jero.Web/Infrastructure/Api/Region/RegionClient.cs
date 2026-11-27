using Jero.Web.Infrastructure.Http;
using static ECMAScript.Global;

namespace Jero.Web.Infrastructure.Api.Region;

/// <summary>查询和维护行政区划，并触发区划同步。</summary>
[ECMAScriptModule("components/region-client.js")]
public static class RegionClient
{
    public static IPromise<ApiOutcome> List(string id)
        => BrowserApiClient.Get("/api/sysRegion/list?Id=" + EncodeURIComponent(id));

    public static IPromise<ApiOutcome> Page(RegionPageInput input)
        => BrowserApiClient.Post("/api/sysRegion/page", input);

    public static IPromise<ApiOutcome> Add(RegionSaveInput input)
        => BrowserApiClient.Post("/api/sysRegion/add", new RegionAddInput
        {
            Pid = input.Pid,
            Name = input.Name,
            ShortName = null,
            MergerName = null,
            Code = input.Code,
            ZipCode = null,
            CityCode = Optional(input.CityCode),
            Level = 0,
            PinYin = null,
            Lng = 0,
            Lat = 0,
            OrderNo = input.OrderNo,
            Remark = Optional(input.Remark)
        });

    public static IPromise<ApiOutcome> Update(RegionView original, RegionSaveInput input)
        => BrowserApiClient.Post("/api/sysRegion/update", new RegionUpdateInput
        {
            // UpdateRegion 将请求映射回 SysRegion。保留原行的隐藏字段，避免编辑时覆盖同步数据。
            Id = original.Id,
            Pid = original.Pid,
            Name = input.Name,
            ShortName = Optional(original.ShortName),
            MergerName = Optional(original.MergerName),
            Code = input.Code,
            ZipCode = Optional(original.ZipCode),
            CityCode = Optional(input.CityCode),
            Level = original.Level,
            PinYin = Optional(original.PinYin),
            Lng = original.Lng,
            Lat = original.Lat,
            OrderNo = input.OrderNo,
            Remark = Optional(input.Remark)
        });

    public static IPromise<ApiOutcome> Delete(string id)
        => BrowserApiClient.Post("/api/sysRegion/delete", new RegionIdInput { Id = id });

    public static IPromise<ApiOutcome> Sync()
        => BrowserApiClient.Post("/api/sysRegion/sync");

    public static RegionView[] ReadList(object? data)
        => ReadRegions(data);

    public static RegionView[] ReadItems(object? data)
    {
        var values = data is null ? null : Reflect.Get(data, "items");
        return ReadRegions(values);
    }

    public static int ReadTotal(object? data)
        => data is null ? 0 : ReadInt(data, "total", 0);

    public static RegionView[] ReadRegions(object? data)
    {
        var values = data as object?[] ?? [];
        var result = new RegionView[values.Length];
        for (var index = 0; index < values.Length; index++)
            result[index] = ReadRegion(values[index]!);
        return result;
    }

    private static RegionView ReadRegion(object value)
        => new()
        {
            // 读响应时将雪花 ID 保存为字符串，避免让 JavaScript 数值承载超出安全整数范围的标识。
            Id = ReadString(value, "id"),
            Pid = ReadString(value, "pid"),
            Name = ReadString(value, "name"),
            ShortName = ReadString(value, "shortName"),
            MergerName = ReadString(value, "mergerName"),
            Code = ReadString(value, "code"),
            ZipCode = ReadString(value, "zipCode"),
            CityCode = ReadString(value, "cityCode"),
            Level = ReadInt(value, "level", 0),
            PinYin = ReadString(value, "pinYin"),
            Lng = ReadDouble(value, "lng"),
            Lat = ReadDouble(value, "lat"),
            OrderNo = ReadInt(value, "orderNo", 100),
            Remark = ReadString(value, "remark"),
            Children = ReadRegions(Reflect.Get(value, "children"))
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

    private static double ReadDouble(object data, string key)
    {
        var value = Reflect.Get(data, key);
        return value is null ? 0 : (double)NumberValue(value);
    }

    private static string? Optional(string value)
        => value.Length == 0 ? null : value;
}

public sealed record RegionPageInput
{
    [ECMAScriptName("page")]
    public int Page { get; init; } = 1;
    [ECMAScriptName("pageSize")]
    public int PageSize { get; init; } = 50;
    [ECMAScriptName("pid")]
    public string Pid { get; init; } = "0";
    [ECMAScriptName("name")]
    public string Name { get; init; } = string.Empty;
    [ECMAScriptName("code")]
    public string Code { get; init; } = string.Empty;
}

public sealed record RegionSaveInput
{
    public string Pid { get; init; } = "0";
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string CityCode { get; init; } = string.Empty;
    public int OrderNo { get; init; } = 100;
    public string Remark { get; init; } = string.Empty;
}

public sealed record RegionAddInput
{
    [ECMAScriptName("pid")]
    public string Pid { get; init; } = "0";
    [ECMAScriptName("name")]
    public string Name { get; init; } = string.Empty;
    [ECMAScriptName("shortName")]
    public string? ShortName { get; init; }
    [ECMAScriptName("mergerName")]
    public string? MergerName { get; init; }
    [ECMAScriptName("code")]
    public string Code { get; init; } = string.Empty;
    [ECMAScriptName("zipCode")]
    public string? ZipCode { get; init; }
    [ECMAScriptName("cityCode")]
    public string? CityCode { get; init; }
    [ECMAScriptName("level")]
    public int Level { get; init; }
    [ECMAScriptName("pinYin")]
    public string? PinYin { get; init; }
    [ECMAScriptName("lng")]
    public double Lng { get; init; }
    [ECMAScriptName("lat")]
    public double Lat { get; init; }
    [ECMAScriptName("orderNo")]
    public int OrderNo { get; init; } = 100;
    [ECMAScriptName("remark")]
    public string? Remark { get; init; }
}

public sealed record RegionUpdateInput
{
    [ECMAScriptName("id")]
    public string Id { get; init; } = string.Empty;
    [ECMAScriptName("pid")]
    public string Pid { get; init; } = "0";
    [ECMAScriptName("name")]
    public string Name { get; init; } = string.Empty;
    [ECMAScriptName("shortName")]
    public string? ShortName { get; init; }
    [ECMAScriptName("mergerName")]
    public string? MergerName { get; init; }
    [ECMAScriptName("code")]
    public string Code { get; init; } = string.Empty;
    [ECMAScriptName("zipCode")]
    public string? ZipCode { get; init; }
    [ECMAScriptName("cityCode")]
    public string? CityCode { get; init; }
    [ECMAScriptName("level")]
    public int Level { get; init; }
    [ECMAScriptName("pinYin")]
    public string? PinYin { get; init; }
    [ECMAScriptName("lng")]
    public double Lng { get; init; }
    [ECMAScriptName("lat")]
    public double Lat { get; init; }
    [ECMAScriptName("orderNo")]
    public int OrderNo { get; init; } = 100;
    [ECMAScriptName("remark")]
    public string? Remark { get; init; }
}

public sealed record RegionIdInput
{
    [ECMAScriptName("id")]
    public string Id { get; init; } = string.Empty;
}

public sealed record RegionView : VueProps
{
    public string Id { get; init; } = string.Empty;
    public string Pid { get; init; } = "0";
    public string Name { get; init; } = string.Empty;
    public string ShortName { get; init; } = string.Empty;
    public string MergerName { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string ZipCode { get; init; } = string.Empty;
    public string CityCode { get; init; } = string.Empty;
    public int Level { get; init; }
    public string PinYin { get; init; } = string.Empty;
    public double Lng { get; init; }
    public double Lat { get; init; }
    public int OrderNo { get; init; } = 100;
    public string Remark { get; init; } = string.Empty;
    public RegionView[] Children { get; init; } = [];
}
