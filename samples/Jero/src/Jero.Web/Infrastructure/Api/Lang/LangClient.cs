using Jero.Web.Infrastructure.Http;
using static ECMAScript.Global;

namespace Jero.Web.Infrastructure.Api.Lang;

/// <summary>维护语言配置，读取书写方向和一周起始日选项。</summary>
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
            Name = input.Name,
            Code = input.Code,
            IsoCode = input.IsoCode,
            UrlCode = input.UrlCode,
            Direction = input.Direction,
            DateFormat = input.DateFormat,
            TimeFormat = input.TimeFormat,
            WeekStart = input.WeekStart,
            Grouping = input.Grouping,
            DecimalPoint = input.DecimalPoint,
            ThousandsSep = input.ThousandsSep,
            Active = input.Active
        });

    public static IPromise<ApiOutcome> Update(LangSaveInput input)
        => BrowserApiClient.Post("/api/sysLang/update", input);

    public static IPromise<ApiOutcome> Delete(string id)
        => BrowserApiClient.Post("/api/sysLang/delete", new LangIdInput { Id = id });

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
                Value = value,
                Label = label.Length == 0 ? "选项" + value : label,
                TagType = ReadString(item, "tagType")
            };
            target++;
        }
        return result;
    }

    private static LangView ReadItem(object value)
        => new()
        {
            Id = ReadString(value, "id"),
            Name = ReadString(value, "name"),
            Code = ReadString(value, "code"),
            IsoCode = ReadString(value, "isoCode"),
            UrlCode = ReadString(value, "urlCode"),
            Direction = ReadInt(value, "direction", 1),
            DateFormat = ReadString(value, "dateFormat"),
            TimeFormat = ReadString(value, "timeFormat"),
            WeekStart = ReadInt(value, "weekStart", 7),
            Grouping = ReadString(value, "grouping"),
            DecimalPoint = ReadString(value, "decimalPoint"),
            ThousandsSep = ReadString(value, "thousandsSep"),
            ThousandsSepDisplay = ReadDisplayString(value, "thousandsSep"),
            Active = ReadBool(value, "active"),
            CreateTime = ReadString(value, "createTime"),
            CreateUserId = ReadString(value, "createUserId"),
            CreateUserName = ReadString(value, "createUserName"),
            UpdateTime = ReadString(value, "updateTime"),
            UpdateUserId = ReadString(value, "updateUserId"),
            UpdateUserName = ReadString(value, "updateUserName")
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
    [ECMAScriptName("page")]
    public int Page { get; init; } = 1;
    [ECMAScriptName("pageSize")]
    public int PageSize { get; init; } = 20;
    [ECMAScriptName("field")]
    public string Field { get; init; } = "active";
    [ECMAScriptName("order")]
    public string Order { get; init; } = "descending";
    [ECMAScriptName("descStr")]
    public string DescStr { get; init; } = "descending";
    [ECMAScriptName("keyword")]
    public string Keyword { get; init; } = string.Empty;
    [ECMAScriptName("name")]
    public string Name { get; init; } = string.Empty;
    [ECMAScriptName("code")]
    public string Code { get; init; } = string.Empty;
    [ECMAScriptName("isoCode")]
    public string IsoCode { get; init; } = string.Empty;
    [ECMAScriptName("urlCode")]
    public string UrlCode { get; init; } = string.Empty;
    [ECMAScriptName("active")]
    public bool? Active { get; init; }
}

public sealed record LangSaveInput
{
    [ECMAScriptName("id")]
    public string Id { get; init; } = string.Empty;
    [ECMAScriptName("name")]
    public string Name { get; init; } = string.Empty;
    [ECMAScriptName("code")]
    public string Code { get; init; } = string.Empty;
    [ECMAScriptName("isoCode")]
    public string IsoCode { get; init; } = string.Empty;
    [ECMAScriptName("urlCode")]
    public string UrlCode { get; init; } = string.Empty;
    [ECMAScriptName("direction")]
    public int Direction { get; init; } = 1;
    [ECMAScriptName("dateFormat")]
    public string DateFormat { get; init; } = string.Empty;
    [ECMAScriptName("timeFormat")]
    public string TimeFormat { get; init; } = string.Empty;
    [ECMAScriptName("weekStart")]
    public int WeekStart { get; init; } = 7;
    [ECMAScriptName("grouping")]
    public string Grouping { get; init; } = string.Empty;
    [ECMAScriptName("decimalPoint")]
    public string DecimalPoint { get; init; } = string.Empty;
    [ECMAScriptName("thousandsSep")]
    public string? ThousandsSep { get; init; }
    [ECMAScriptName("active")]
    public bool Active { get; init; }
}

public sealed record LangAddInput
{
    [ECMAScriptName("name")]
    public string Name { get; init; } = string.Empty;
    [ECMAScriptName("code")]
    public string Code { get; init; } = string.Empty;
    [ECMAScriptName("isoCode")]
    public string IsoCode { get; init; } = string.Empty;
    [ECMAScriptName("urlCode")]
    public string UrlCode { get; init; } = string.Empty;
    [ECMAScriptName("direction")]
    public int Direction { get; init; } = 1;
    [ECMAScriptName("dateFormat")]
    public string DateFormat { get; init; } = string.Empty;
    [ECMAScriptName("timeFormat")]
    public string TimeFormat { get; init; } = string.Empty;
    [ECMAScriptName("weekStart")]
    public int WeekStart { get; init; } = 7;
    [ECMAScriptName("grouping")]
    public string Grouping { get; init; } = string.Empty;
    [ECMAScriptName("decimalPoint")]
    public string DecimalPoint { get; init; } = string.Empty;
    [ECMAScriptName("thousandsSep")]
    public string? ThousandsSep { get; init; }
    [ECMAScriptName("active")]
    public bool Active { get; init; }
}

public sealed record LangIdInput
{
    [ECMAScriptName("id")]
    public string Id { get; init; } = string.Empty;
}

public sealed record LangView : VueProps
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string IsoCode { get; init; } = string.Empty;
    public string UrlCode { get; init; } = string.Empty;
    public int Direction { get; init; }
    public string DateFormat { get; init; } = string.Empty;
    public string TimeFormat { get; init; } = string.Empty;
    public int WeekStart { get; init; }
    public string Grouping { get; init; } = string.Empty;
    public string DecimalPoint { get; init; } = string.Empty;
    public string ThousandsSep { get; init; } = string.Empty;
    public string ThousandsSepDisplay { get; init; } = "-";
    public bool Active { get; init; }
    public string CreateTime { get; init; } = string.Empty;
    public string CreateUserId { get; init; } = string.Empty;
    public string CreateUserName { get; init; } = string.Empty;
    public string UpdateTime { get; init; } = string.Empty;
    public string UpdateUserId { get; init; } = string.Empty;
    public string UpdateUserName { get; init; } = string.Empty;
}

public sealed record LangEnumOption : VueProps
{
    public int Value { get; init; }
    public string Label { get; init; } = string.Empty;
    public string TagType { get; init; } = string.Empty;
}
