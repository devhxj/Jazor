namespace Jero.Web.App.Lang;

public sealed record LangNumberOption(int value, string label);

public sealed record LangTableRow(
    string id, string name, string code, string isoCode, string urlCode,
    int direction, string directionLabel, string dateFormat, string timeFormat,
    string weekLabel, string grouping, string decimalPoint, string thousandsSepDisplay, bool active);
