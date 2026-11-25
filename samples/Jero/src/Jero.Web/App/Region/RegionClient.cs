using Jero.Web.App.Http;
using static ECMAScript.Global;

namespace Jero.Web.App.Region;

/// <summary>Typed browser boundary for administrative region management.</summary>
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
            pid = input.pid,
            name = input.name,
            shortName = null,
            mergerName = null,
            code = input.code,
            zipCode = null,
            cityCode = Optional(input.cityCode),
            level = 0,
            pinYin = null,
            lng = 0,
            lat = 0,
            orderNo = input.orderNo,
            remark = Optional(input.remark)
        });

    public static IPromise<ApiOutcome> Update(RegionView original, RegionSaveInput input)
        => BrowserApiClient.Post("/api/sysRegion/update", new RegionUpdateInput
        {
            // UpdateRegion adapts the request back to SysRegion. Preserve every hidden field
            // from the selected row so a five-field edit cannot erase the synced snapshot.
            id = original.id,
            pid = original.pid,
            name = input.name,
            shortName = Optional(original.shortName),
            mergerName = Optional(original.mergerName),
            code = input.code,
            zipCode = Optional(original.zipCode),
            cityCode = Optional(input.cityCode),
            level = original.level,
            pinYin = Optional(original.pinYin),
            lng = original.lng,
            lat = original.lat,
            orderNo = input.orderNo,
            remark = Optional(input.remark)
        });

    public static IPromise<ApiOutcome> Delete(string id)
        => BrowserApiClient.Post("/api/sysRegion/delete", new RegionIdInput { id = id });

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
            // Snowflake identifiers must become strings at the fetch boundary. Never store
            // them as JavaScript numbers, even when a serializer happens to emit a number.
            id = ReadString(value, "id"),
            pid = ReadString(value, "pid"),
            name = ReadString(value, "name"),
            shortName = ReadString(value, "shortName"),
            mergerName = ReadString(value, "mergerName"),
            code = ReadString(value, "code"),
            zipCode = ReadString(value, "zipCode"),
            cityCode = ReadString(value, "cityCode"),
            level = ReadInt(value, "level", 0),
            pinYin = ReadString(value, "pinYin"),
            lng = ReadDouble(value, "lng"),
            lat = ReadDouble(value, "lat"),
            orderNo = ReadInt(value, "orderNo", 100),
            remark = ReadString(value, "remark"),
            children = ReadRegions(Reflect.Get(value, "children"))
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
    public int page { get; init; } = 1;
    public int pageSize { get; init; } = 50;
    public string pid { get; init; } = "0";
    public string name { get; init; } = string.Empty;
    public string code { get; init; } = string.Empty;
}

public sealed record RegionSaveInput
{
    public string pid { get; init; } = "0";
    public string name { get; init; } = string.Empty;
    public string code { get; init; } = string.Empty;
    public string cityCode { get; init; } = string.Empty;
    public int orderNo { get; init; } = 100;
    public string remark { get; init; } = string.Empty;
}

public sealed record RegionAddInput
{
    public string pid { get; init; } = "0";
    public string name { get; init; } = string.Empty;
    public string? shortName { get; init; }
    public string? mergerName { get; init; }
    public string code { get; init; } = string.Empty;
    public string? zipCode { get; init; }
    public string? cityCode { get; init; }
    public int level { get; init; }
    public string? pinYin { get; init; }
    public double lng { get; init; }
    public double lat { get; init; }
    public int orderNo { get; init; } = 100;
    public string? remark { get; init; }
}

public sealed record RegionUpdateInput
{
    public string id { get; init; } = string.Empty;
    public string pid { get; init; } = "0";
    public string name { get; init; } = string.Empty;
    public string? shortName { get; init; }
    public string? mergerName { get; init; }
    public string code { get; init; } = string.Empty;
    public string? zipCode { get; init; }
    public string? cityCode { get; init; }
    public int level { get; init; }
    public string? pinYin { get; init; }
    public double lng { get; init; }
    public double lat { get; init; }
    public int orderNo { get; init; } = 100;
    public string? remark { get; init; }
}

public sealed record RegionIdInput
{
    public string id { get; init; } = string.Empty;
}

public sealed record RegionView : VueProps
{
    public string id { get; init; } = string.Empty;
    public string pid { get; init; } = "0";
    public string name { get; init; } = string.Empty;
    public string shortName { get; init; } = string.Empty;
    public string mergerName { get; init; } = string.Empty;
    public string code { get; init; } = string.Empty;
    public string zipCode { get; init; } = string.Empty;
    public string cityCode { get; init; } = string.Empty;
    public int level { get; init; }
    public string pinYin { get; init; } = string.Empty;
    public double lng { get; init; }
    public double lat { get; init; }
    public int orderNo { get; init; } = 100;
    public string remark { get; init; } = string.Empty;
    public RegionView[] children { get; init; } = [];
}

public sealed partial record RegionFlatView : VueProps;
