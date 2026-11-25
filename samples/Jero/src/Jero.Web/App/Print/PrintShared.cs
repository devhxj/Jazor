using static ECMAScript.Global;

namespace Jero.Web.App.Print;

/// <summary>JSON and form normalization shared by save, preview, and provider-mode flows.</summary>
[ECMAScriptModule("components/print-shared.js")]
public static class PrintShared
{
    public const string DefaultPrintDataDemo = """
        {
          "barCode": "JERO-PRINT-001",
          "creater": "Jero Admin",
          "date": "2026-04-24",
          "director": "审批经理",
          "imageUrl": "",
          "longText": "这是打印模板设计器的预览测试数据，可直接修改为业务真实数据结构。",
          "name": "测试客户",
          "orderId": "PO20260424001",
          "printDate": "2026-04-24 10:00:00",
          "qrCode": "https://jero.local/print/demo",
          "signer": "仓库签收",
          "table": [
            {
              "DETAIL": "演示数据",
              "DJ": "6.80",
              "GG": "1*24g",
              "JE": "13.60",
              "NAME": "测试商品01",
              "SL": 2,
              "TM": "690000000001"
            },
            {
              "DETAIL": "演示数据",
              "DJ": "9.90",
              "GG": "500ml",
              "JE": "19.80",
              "NAME": "测试商品02",
              "SL": 2,
              "TM": "690000000002"
            }
          ],
          "tel": "13800000000"
        }
        """;

    public const string DefaultPrintParam = """
        {
          "color": false,
          "copies": 1,
          "printer": "",
          "title": ""
        }
        """;

    public static int NormalizeModeIndex(int value) => value < 0 ? 0 : value;

    public static string? Optional(string value)
    {
        var normalized = value.Trim();
        return normalized.Length == 0 ? null : normalized;
    }

    public static string FormatJsonText(string value)
    {
        var normalized = value.Trim();
        if (normalized.Length == 0)
            return string.Empty;

        try
        {
            return PrettyJson(JSON.Parse(normalized));
        }
        catch
        {
            return normalized;
        }
    }

    public static PrintJsonOutcome ValidateJsonObjectText(string value, string label)
    {
        var normalized = value.Trim();
        if (normalized.Length == 0)
            return new PrintJsonOutcome { Ok = true };

        try
        {
            var parsed = JSON.Parse(normalized);
            if (!IsObject(parsed))
                return new PrintJsonOutcome { Error = label + "必须是 JSON 对象" };
            return new PrintJsonOutcome { Ok = true, Value = PrettyJson(parsed) };
        }
        catch
        {
            return new PrintJsonOutcome { Error = label + "不是有效 JSON" };
        }
    }

    public static int ReadTemplateModeIndex(string template)
    {
        var normalized = template.Trim();
        if (normalized.Length == 0)
            return 0;

        try
        {
            var parsed = JSON.Parse(normalized);
            if (parsed is null)
                return 0;
            var panels = Reflect.Get(parsed, "panels") as object?[] ?? [];
            if (panels.Length == 0 || panels[0] is null)
                return 0;
            var index = Reflect.Get(panels[0]!, "index");
            return index is null ? 0 : NormalizeModeIndex((int)NumberValue(index));
        }
        catch
        {
            return 0;
        }
    }

    public static PrintJsonOutcome NormalizeTemplateJson(string template, int modeIndex)
    {
        try
        {
            var parsed = JSON.Parse(template.Trim());
            if (!IsObject(parsed))
                return new PrintJsonOutcome { Error = "打印模板必须是 JSON 对象" };
            var panels = Reflect.Get(parsed!, "panels") as object?[] ?? [];
            if (panels.Length == 0 || !IsObject(panels[0]))
                return new PrintJsonOutcome { Error = "打印模板缺少有效面板定义" };

            // Provider identity is persisted on the first panel by the legacy shared.ts contract.
            Reflect.Set(panels[0]!, "index", NormalizeModeIndex(modeIndex));
            return new PrintJsonOutcome { Ok = true, Value = PrettyJson(parsed) };
        }
        catch
        {
            return new PrintJsonOutcome { Error = "打印模板不是有效 JSON" };
        }
    }

    public static string NormalizePreviewData(string value)
    {
        var normalized = value.Trim();
        if (normalized.Length == 0)
            return DefaultPrintDataDemo;
        try
        {
            JSON.Parse(normalized);
            return normalized;
        }
        catch
        {
            return DefaultPrintDataDemo;
        }
    }

    private static bool IsObject(object? value)
        => value is not null && value is not object?[] && TypeOf(value) == "object";

    private static string PrettyJson(object? value)
        => JSON.Stringify(value!, (object?)null, (Number)2) ?? "{}";
}

public sealed record PrintProviderOption : VueProps
{
    public int value { get; init; }
    public string label { get; init; } = string.Empty;
}

public sealed record PrintJsonOutcome
{
    public bool Ok { get; init; }
    public string Value { get; init; } = string.Empty;
    public string? Error { get; init; }
}

public sealed record PrintPaperOption : VueProps
{
    public string type { get; init; } = string.Empty;
    public double width { get; init; }
    public double height { get; init; }
}
