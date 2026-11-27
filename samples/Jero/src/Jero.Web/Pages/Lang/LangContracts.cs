namespace Jero.Web.Pages.Lang;

public sealed record LangNumberOption(int Value, string Label);

public sealed record LangTableRow(
    string Id,
    [property: ECMAScriptName("name")] string Name,
    [property: ECMAScriptName("code")] string Code,
    [property: ECMAScriptName("isoCode")] string IsoCode,
    [property: ECMAScriptName("urlCode")] string UrlCode,
    int Direction,
    string DirectionLabel,
    [property: ECMAScriptName("dateFormat")] string DateFormat,
    [property: ECMAScriptName("timeFormat")] string TimeFormat,
    string WeekLabel,
    [property: ECMAScriptName("grouping")] string Grouping,
    [property: ECMAScriptName("decimalPoint")] string DecimalPoint,
    string ThousandsSepDisplay,
    [property: ECMAScriptName("active")] bool Active);
