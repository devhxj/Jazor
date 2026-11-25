using Jero.Web.App.Http;
using static ECMAScript.Global;

namespace Jero.Web.App.Lang;

/// <summary>Typed browser boundary for language management.</summary>
[ECMAScriptModule("components/lang-client.js")]
public static class LangClient
{
    public static IPromise<ApiOutcome> Page(LangPageInput input)
        => BrowserApiClient.Post("/api/sysLang/page", input);

    public static IPromise<ApiOutcome> DirectionOptions()
        => BrowserApiClient.Get("/api/sysDictType/dataList?code=DirectionEnum");

    public static IPromise<ApiOutcome> WeekOptions()
        => BrowserApiClient.Get("/api/sysDictType/dataList?code=WeekEnum");

    public static IPromise<ApiOutcome> Add(LangSaveInput input)
        => BrowserApiClient.Post("/api/sysLang/add", new LangAddInput
        {
            name = input.name,
            code = input.code,
            isoCode = input.isoCode,
            urlCode = input.urlCode,
            direction = input.direction,
            dateFormat = input.dateFormat,
            timeFormat = input.timeFormat,
            weekStart = input.weekStart,
            grouping = input.grouping,
            decimalPoint = input.decimalPoint,
            thousandsSep = input.thousandsSep,
            active = input.active
        });

    public static IPromise<ApiOutcome> Update(LangSaveInput input)
        => BrowserApiClient.Post("/api/sysLang/update", input);

    public static IPromise<ApiOutcome> Delete(string id)
        => BrowserApiClient.Post("/api/sysLang/delete", new LangIdInput { id = id });

    public static LangView[] ReadItems(object? data)
    {
        var values = (data is null ? null : Reflect.Get(data, "items")) as object?[] ?? [];
        var result = new LangView[values.Length];
        for (var index = 0; index < values.Length; index++)
            result[index] = ReadItem(values[index]!);
        return result;
    }

    public static int ReadTotal(object? data)
        => data is null ? 0 : ReadInt(data, "total", 0);

    public static LangEnumOption[] ReadEnumOptions(object? data)
    {
        var values = data as object?[] ?? [];
        var validCount = 0;
        for (var index = 0; index < values.Length; index++)
            if (ReadInt(values[index]!, "value", 0) > 0)
                validCount++;

        var result = new LangEnumOption[validCount];
        var target = 0;
        for (var index = 0; index < values.Length; index++)
        {
            var item = values[index]!;
            var value = ReadInt(item, "value", 0);
            if (value <= 0)
                continue;

            var label = ReadString(item, "label");
            if (label.Length == 0)
                label = ReadString(item, "name");

            result[target] = new LangEnumOption
            {
                value = value,
                label = label.Length == 0 ? "选项" + value : label,
                tagType = ReadString(item, "tagType")
            };
            target++;
        }
        return result;
    }

    private static LangView ReadItem(object value)
        => new()
        {
            id = ReadString(value, "id"),
            name = ReadString(value, "name"),
            code = ReadString(value, "code"),
            isoCode = ReadString(value, "isoCode"),
            urlCode = ReadString(value, "urlCode"),
            direction = ReadInt(value, "direction", 1),
            dateFormat = ReadString(value, "dateFormat"),
            timeFormat = ReadString(value, "timeFormat"),
            weekStart = ReadInt(value, "weekStart", 7),
            grouping = ReadString(value, "grouping"),
            decimalPoint = ReadString(value, "decimalPoint"),
            thousandsSep = ReadString(value, "thousandsSep"),
            thousandsSepDisplay = ReadDisplayString(value, "thousandsSep"),
            active = ReadBool(value, "active"),
            createTime = ReadString(value, "createTime"),
            createUserId = ReadString(value, "createUserId"),
            createUserName = ReadString(value, "createUserName"),
            updateTime = ReadString(value, "updateTime"),
            updateUserId = ReadString(value, "updateUserId"),
            updateUserName = ReadString(value, "updateUserName")
        };

    private static string ReadString(object data, string key)
    {
        var value = Reflect.Get(data, key);
        if (value is null)
            return string.Empty;

        var result = StringValue(value);
        return result == "null" || result == "undefined" ? string.Empty : result;
    }

    private static string ReadDisplayString(object data, string key)
    {
        var value = ReadString(data, key);
        return value.Length == 0 ? "-" : value;
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

public sealed record LangPageInput
{
    public int page { get; init; } = 1;
    public int pageSize { get; init; } = 20;
    public string field { get; init; } = "active";
    public string order { get; init; } = "descending";
    public string descStr { get; init; } = "descending";
    public string keyword { get; init; } = string.Empty;
    public string name { get; init; } = string.Empty;
    public string code { get; init; } = string.Empty;
    public string isoCode { get; init; } = string.Empty;
    public string urlCode { get; init; } = string.Empty;
    public bool? active { get; init; }
}

public sealed record LangSaveInput
{
    public string id { get; init; } = string.Empty;
    public string name { get; init; } = string.Empty;
    public string code { get; init; } = string.Empty;
    public string isoCode { get; init; } = string.Empty;
    public string urlCode { get; init; } = string.Empty;
    public int direction { get; init; } = 1;
    public string dateFormat { get; init; } = string.Empty;
    public string timeFormat { get; init; } = string.Empty;
    public int weekStart { get; init; } = 7;
    public string grouping { get; init; } = string.Empty;
    public string decimalPoint { get; init; } = string.Empty;
    public string? thousandsSep { get; init; }
    public bool active { get; init; }
}

public sealed record LangAddInput
{
    public string name { get; init; } = string.Empty;
    public string code { get; init; } = string.Empty;
    public string isoCode { get; init; } = string.Empty;
    public string urlCode { get; init; } = string.Empty;
    public int direction { get; init; } = 1;
    public string dateFormat { get; init; } = string.Empty;
    public string timeFormat { get; init; } = string.Empty;
    public int weekStart { get; init; } = 7;
    public string grouping { get; init; } = string.Empty;
    public string decimalPoint { get; init; } = string.Empty;
    public string? thousandsSep { get; init; }
    public bool active { get; init; }
}

public sealed record LangIdInput
{
    public string id { get; init; } = string.Empty;
}

public sealed record LangView : VueProps
{
    public string id { get; init; } = string.Empty;
    public string name { get; init; } = string.Empty;
    public string code { get; init; } = string.Empty;
    public string isoCode { get; init; } = string.Empty;
    public string urlCode { get; init; } = string.Empty;
    public int direction { get; init; }
    public string dateFormat { get; init; } = string.Empty;
    public string timeFormat { get; init; } = string.Empty;
    public int weekStart { get; init; }
    public string grouping { get; init; } = string.Empty;
    public string decimalPoint { get; init; } = string.Empty;
    public string thousandsSep { get; init; } = string.Empty;
    public string thousandsSepDisplay { get; init; } = "-";
    public bool active { get; init; }
    public string createTime { get; init; } = string.Empty;
    public string createUserId { get; init; } = string.Empty;
    public string createUserName { get; init; } = string.Empty;
    public string updateTime { get; init; } = string.Empty;
    public string updateUserId { get; init; } = string.Empty;
    public string updateUserName { get; init; } = string.Empty;
}

public sealed record LangEnumOption : VueProps
{
    public int value { get; init; }
    public string label { get; init; } = string.Empty;
    public string tagType { get; init; } = string.Empty;
}
