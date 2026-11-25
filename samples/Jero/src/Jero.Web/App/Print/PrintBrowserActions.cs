using static ECMAScript.Global;

namespace Jero.Web.App.Print;

/// <summary>
/// Typed Hiprint and DOM boundary. The page keeps ordinary reactive state while this module
/// owns the designer instance and the browser-only print lifecycle.
/// </summary>
[ECMAScriptModule("components/print-browser-actions.js")]
public static class PrintBrowserActions
{
    public const string ElementContainerId = "jero-print-elements";
    public const string TemplateContainerId = "jero-print-template";
    public const string SettingContainerId = "jero-print-settings";
    public const string PreviewContainerId = "jero-print-preview";

    private const string AProviderName = "aProviderModule";
    private const string BProviderName = "bProviderModule";
    private static HiprintApi api = default!;

    public static HiprintDesignerSession CreateDesigner(HiprintRuntime runtime, int modeIndex, string host, string templateText)
    {
        // Provider callbacks run synchronously during Init; keep native runtime identity
        // with its resource owner instead of placing it in reactive component fields.
        var hiprint = runtime.Hiprint;
        api = hiprint;
        var normalizedMode = PrintShared.NormalizeModeIndex(modeIndex) == 1 ? 1 : 0;
        var providerName = normalizedMode == 1 ? BProviderName : AProviderName;
        var provider = new HiprintProviderFactory
        {
            addElementTypes = normalizedMode == 1 ? AddBProviderTypes : AddAProviderTypes
        };

        hiprint.Init(new HiprintInitOptions
        {
            host = PrintShared.Optional(host),
            providers = [provider]
        });

        ClearContainer(ElementContainerId);
        ClearContainer(TemplateContainerId);
        ClearContainer(SettingContainerId);
        hiprint.PrintElementTypeManager.Build("#" + ElementContainerId, providerName);

        var templateData = ParseTemplateOrEmpty(templateText);
        var template = (HiprintPrintTemplate)Reflect.Construct(hiprint.PrintTemplate, new object?[]
        {
            new HiprintPrintTemplateOptions
            {
                template = templateData,
                settingContainer = "#" + SettingContainerId,
                paginationContainer = ".jero-print-pagination",
                fontList =
                [
                    new() { title = "微软雅黑", value = "Microsoft YaHei" },
                    new() { title = "宋体", value = "SimSun" },
                    new() { title = "黑体", value = "STHeitiSC-Light" },
                    new() { title = "Arial", value = "Arial" },
                    new() { title = "华为楷体", value = "STKaiti" },
                    new() { title = "cursive", value = "cursive" },
                    new() { title = "Vector", value = "Vector" }
                ]
            }
        });
        template.Design("#" + TemplateContainerId);

        var paper = ReadPaper(template.GetJson());
        if (paper.Width <= 0 || paper.Height <= 0)
        {
            paper = new HiprintPaperState { Width = 210, Height = 296.6 };
            template.SetPaper(paper.Width, paper.Height);
        }

        var scale = template.EditingPanel?.Scale ?? 1;
        return new HiprintDesignerSession
        {
            Template = template,
            Host = host.Trim(),
            ModeIndex = normalizedMode,
            PaperWidth = paper.Width,
            PaperHeight = paper.Height,
            Scale = ClampScale(scale)
        };
    }

    public static void ClearDesigner(HiprintDesignerSession? session)
    {
        session?.Template.Clear();
        ClearContainer(ElementContainerId);
        ClearContainer(TemplateContainerId);
        ClearContainer(SettingContainerId);
        ClearContainer(PreviewContainerId);
    }

    public static void SetPaper(HiprintDesignerSession session, double width, double height)
    {
        session.Template.SetPaper(width, height);
        session.PaperWidth = width;
        session.PaperHeight = height;
    }

    public static double SetScale(HiprintDesignerSession session, double scale)
    {
        var normalized = ClampScale(scale);
        session.Template.Zoom(normalized);
        session.Scale = normalized;
        return normalized;
    }

    public static void RotatePaper(HiprintDesignerSession session)
    {
        session.Template.RotatePaper();
        var width = session.PaperWidth;
        session.PaperWidth = session.PaperHeight;
        session.PaperHeight = width;
    }

    public static void Align(HiprintDesignerSession session, string alignment)
        => session.Template.SetElementsAlign(alignment);

    public static void ClearPaper(HiprintDesignerSession session)
        => session.Template.Clear();

    public static string GetTemplateJson(HiprintDesignerSession session, int modeIndex)
    {
        var template = session.Template.GetJson();
        if (template is null)
            return "{}";

        var panels = Reflect.Get(template, "panels") as object?[] ?? [];
        if (panels.Length > 0 && panels[0] is not null)
            Reflect.Set(panels[0]!, "index", PrintShared.NormalizeModeIndex(modeIndex));
        return JSON.Stringify(template, (object?)null, (Number)2) ?? "{}";
    }

    public static void RenderPreview(HiprintDesignerSession session, string printDataText)
    {
        // Element Plus may raise opened before the teleported dialog body is queryable.
        // 下一事件循环再写 DOM，避免预览容器尚未挂载时静默丢失本次渲染。
        Window.SetTimeout(
            (TimerHandler)(Delegate)(Action)(() => RenderPreviewNow(session, printDataText)),
            0,
            []);
    }

    private static void RenderPreviewNow(HiprintDesignerSession session, string printDataText)
    {
        ClearContainer(PreviewContainerId);
        var container = Document.GetElementById(PreviewContainerId);
        if (container is null)
            return;

        var rendered = session.Template.GetHtml(ParseJsonObject(printDataText));
        if (rendered is null)
            return;
        var candidate = Reflect.Get(rendered, "0") as Node ?? rendered as Node;
        if (candidate is not null)
            container.AppendChild(candidate);
    }

    public static void PrintInBrowser(HiprintDesignerSession session, string printDataText, Action completed)
        => session.Template.Print(
            ParseJsonObject(printDataText),
            new HiprintBrowserPrintOptions(),
            new HiprintPrintCallbacks { callback = completed });

    public static string PrintWithClient(
        HiprintDesignerSession session,
        string printDataText,
        string printParamText,
        Action completed,
        Action<string> failed)
    {
        var printOptions = ParseJsonObject(printParamText);
        var printerValue = Reflect.Get(printOptions, "printer");
        var printerName = printerValue is null ? string.Empty : StringValue(printerValue).Trim();
        if (printerName.Length > 0)
        {
            var printers = session.Template.GetPrinterList();
            var found = false;
            for (var index = 0; index < printers.Length; index++)
            {
                if (printers[index].Name == printerName)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
                return "未找到打印机：" + printerName;
        }

        // Register completion handlers before dispatch; a local print service may respond immediately.
        session.Template.On("printSuccess", _ => completed());
        session.Template.On("printError", value => failed(value is null ? "未知错误" : StringValue(value)));
        session.Template.PrintClient(ParseJsonObject(printDataText), printOptions);
        return string.Empty;
    }

    public static void ExportPdf(HiprintDesignerSession session, string printDataText)
        => session.Template.ToPdf(ParseJsonObject(printDataText), "打印模板");

    private static void AddAProviderTypes(HiprintProviderContext context)
    {
        context.RemovePrintElementTypes(AProviderName);
        context.AddPrintElementTypes(AProviderName,
        [
            Group("【公共组件】",
            [
                Text("aProviderModule.barcode", "条形码", "18012345678", "barCode", "barcode", 32, 0),
                Text("aProviderModule.qrcode", "二维码", "Z18012345678", "qrCode", "qrcode", 64, 64),
                Table("aProviderModule.table", "表格", false),
                CustomText("aProviderModule.customText"),
                LongText("aProviderModule.longText"),
                new() { tid = "aProviderModule.logo", title = "Logo", type = "image", data = "/images/logo.png", options = new() { field = "imageUrl" } },
                Shape("aProviderModule.hline", "横线", "hline"),
                Shape("aProviderModule.vline", "竖线", "vline"),
                Shape("aProviderModule.rect", "矩形", "rect"),
                Shape("aProviderModule.oval", "椭圆", "oval")
            ]),
            Group("【视图字段】",
            [
                FieldText("aProviderModule.creater", "制表人", "Jero Admin", "creater"),
                FieldText("aProviderModule.printDate", "打印时间", "2026-04-24 09:00", "printDate"),
                FieldText("aProviderModule.signer", "库管签字", "仓库签收", "signer"),
                FieldText("aProviderModule.director", "经理签字", "审批经理", "director")
            ])
        ]);
    }

    private static void AddBProviderTypes(HiprintProviderContext context)
    {
        context.RemovePrintElementTypes(BProviderName);
        context.AddPrintElementTypes(BProviderName,
        [
            Group("【常规】",
            [
                Heading("bProviderModule.header", "单据表头", 16.5),
                Heading("bProviderModule.type", "单据类型", 15),
                FieldText("bProviderModule.order", "订单编号", "PO20260424001", "orderId"),
                FieldText("bProviderModule.date", "业务日期", "2026-04-24", "date"),
                Text("bProviderModule.barcode", "条形码", "18012345678", string.Empty, "barcode", 32, 0),
                Text("bProviderModule.qrcode", "二维码", "Z18012345678", string.Empty, "qrcode", 64, 64),
                Heading("bProviderModule.platform", "平台名称", 16.5, "Jero 平台"),
                new() { tid = "bProviderModule.image", title = "Logo", type = "image", data = "/images/logo.png" }
            ]),
            Group("【客户】",
            [
                FieldText("bProviderModule.khname", "客户名称", "测试客户", "name"),
                FieldText("bProviderModule.tel", "客户电话", "13800000000", "tel")
            ]),
            Group("【表格/其他】",
            [
                Table("bProviderModule.table", "订单数据", true),
                CustomText("bProviderModule.customText"),
                LongText("bProviderModule.longText")
            ]),
            Group("【辅助】",
            [
                Shape("bProviderModule.hline", "横线", "hline"),
                Shape("bProviderModule.vline", "竖线", "vline"),
                Shape("bProviderModule.rect", "矩形", "rect"),
                Shape("bProviderModule.oval", "椭圆", "oval")
            ])
        ]);
    }

    private static HiprintElementTypeGroup Group(string title, HiprintElementDefinition[] elements)
        => (HiprintElementTypeGroup)Reflect.Construct(
            api.PrintElementTypeGroup,
            new object?[] { title, elements });

    private static HiprintElementDefinition Shape(string tid, string title, string type)
        => new() { tid = tid, title = title, type = type };

    private static HiprintElementDefinition CustomText(string tid)
        => new()
        {
            tid = tid,
            title = "文本",
            type = "text",
            custom = true,
            customText = "自定义文本",
            options = new() { testData = "自定义文本", width = 200 }
        };

    private static HiprintElementDefinition LongText(string tid)
        => new()
        {
            tid = tid,
            title = "长文本",
            type = "longText",
            options = new() { field = "longText", testData = "长文本分页/不分页测试", width = 200 }
        };

    private static HiprintElementDefinition Heading(string tid, string title, double fontSize, string? data = null)
        => new()
        {
            tid = tid,
            title = title,
            type = "text",
            data = data ?? title,
            options = new()
            {
                fontSize = fontSize,
                fontWeight = "700",
                height = title == "单据类型" ? 16 : 17,
                hideTitle = true,
                testData = data ?? title,
                textAlign = "center"
            }
        };

    private static HiprintElementDefinition FieldText(string tid, string title, string data, string field)
        => new()
        {
            tid = tid,
            title = title,
            type = "text",
            data = data,
            options = new()
            {
                field = field,
                fontSize = 6.75,
                fontWeight = "700",
                height = 16,
                testData = data,
                textAlign = "left",
                textContentVerticalAlign = "middle"
            }
        };

    private static HiprintElementDefinition Text(
        string tid,
        string title,
        string data,
        string field,
        string textType,
        double height,
        double width)
        => new()
        {
            tid = tid,
            title = title,
            type = "text",
            data = data,
            options = new()
            {
                field = field,
                fontSize = 12,
                fontWeight = "700",
                height = height,
                lineHeight = 18,
                testData = "Z18012345678",
                textAlign = "left",
                textContentVerticalAlign = "middle",
                textType = textType,
                width = width
            }
        };

    private static HiprintElementDefinition Table(string tid, string title, bool includeDetail)
    {
        var columns = includeDetail
            ? new HiprintTableColumn[]
            {
                Column("NAME", "名称", 100), Column("SL", "数量", 100), Column("TM", "条码", 100),
                Column("GG", "规格", 100), Column("DJ", "单价", 100), Column("JE", "金额", 100),
                Column("DETAIL", "备注", 100)
            }
            : new HiprintTableColumn[]
            {
                Column("NAME", "名称", 150), Column("SL", "数量", 80), Column("GG", "规格", 80, false),
                Column("TM", "条码", 100, false), Column("DJ", "单价", 100), Column("JE", "金额", 100, false)
            };

        var fields = new HiprintTableField[columns.Length];
        for (var index = 0; index < columns.Length; index++)
            fields[index] = new HiprintTableField { field = columns[index].field, text = columns[index].title };

        return new HiprintElementDefinition
        {
            tid = tid,
            title = title,
            type = "table",
            editable = true,
            columnAlignEditable = true,
            columnDisplayEditable = true,
            columnDisplayIndexEditable = true,
            columnResizable = true,
            columnTitleEditable = true,
            isEnableContextMenu = true,
            isEnableDeleteColumn = true,
            isEnableDeleteRow = true,
            isEnableEditField = true,
            isEnableInsertColumn = true,
            isEnableInsertRow = true,
            isEnableMergeCell = true,
            columns = [columns],
            options = new HiprintElementOptions
            {
                field = "table",
                fields = fields,
                tableFooterRepeat = includeDetail ? null : "last",
                tableHeaderRepeat = includeDetail ? null : "first"
            }
        };
    }

    private static HiprintTableColumn Column(string field, string title, double width, bool? visible = null)
        => new() { align = "center", field = field, title = title, width = width, @checked = visible };

    private static object ParseTemplateOrEmpty(string text)
    {
        var normalized = text.Trim();
        if (normalized.Length == 0)
            return JSON.Parse("{}")!;
        try
        {
            var value = JSON.Parse(normalized);
            return value is null || TypeOf(value) != "object" ? JSON.Parse("{}")! : value;
        }
        catch
        {
            return JSON.Parse("{}")!;
        }
    }

    private static object ParseJsonObject(string text)
    {
        var normalized = text.Trim();
        if (normalized.Length == 0)
            return JSON.Parse("{}")!;
        var value = JSON.Parse(normalized);
        return value is null || TypeOf(value) != "object" ? JSON.Parse("{}")! : value;
    }

    private static HiprintPaperState ReadPaper(object? template)
    {
        if (template is null)
            return new();
        var panels = Reflect.Get(template, "panels") as object?[] ?? [];
        if (panels.Length == 0 || panels[0] is null)
            return new();
        var width = Reflect.Get(panels[0]!, "width");
        var height = Reflect.Get(panels[0]!, "height");
        return new HiprintPaperState
        {
            Width = width is null ? 0 : NumberValue(width),
            Height = height is null ? 0 : NumberValue(height)
        };
    }

    private static void ClearContainer(string id)
    {
        var element = Document.GetElementById(id);
        if (element is not null)
            element.InnerHTML = string.Empty;
    }

    private static double ClampScale(double scale)
        => scale < 0.5 ? 0.5 : scale > 5 ? 5 : scale;
}

public sealed record HiprintDesignerSession
{
    public HiprintPrintTemplate Template { get; init; } = default!;
    public string Host { get; init; } = string.Empty;
    public int ModeIndex { get; init; }
    public double PaperWidth { get; set; }
    public double PaperHeight { get; set; }
    public double Scale { get; set; }
}

public sealed record HiprintPaperState
{
    public double Width { get; init; }
    public double Height { get; init; }
}

public sealed record HiprintElementDefinition
{
    public string tid { get; init; } = string.Empty;
    public string title { get; init; } = string.Empty;
    public string type { get; init; } = string.Empty;
    public string? data { get; init; }
    public bool? custom { get; init; }
    public string? customText { get; init; }
    public bool? editable { get; init; }
    public bool? columnAlignEditable { get; init; }
    public bool? columnDisplayEditable { get; init; }
    public bool? columnDisplayIndexEditable { get; init; }
    public bool? columnResizable { get; init; }
    public bool? columnTitleEditable { get; init; }
    public bool? isEnableContextMenu { get; init; }
    public bool? isEnableDeleteColumn { get; init; }
    public bool? isEnableDeleteRow { get; init; }
    public bool? isEnableEditField { get; init; }
    public bool? isEnableInsertColumn { get; init; }
    public bool? isEnableInsertRow { get; init; }
    public bool? isEnableMergeCell { get; init; }
    public HiprintTableColumn[][]? columns { get; init; }
    public HiprintElementOptions? options { get; init; }
}

public sealed record HiprintElementOptions
{
    public string? field { get; init; }
    public double? fontSize { get; init; }
    public string? fontWeight { get; init; }
    public double? height { get; init; }
    public bool? hideTitle { get; init; }
    public double? lineHeight { get; init; }
    public string? testData { get; init; }
    public string? textAlign { get; init; }
    public string? textContentVerticalAlign { get; init; }
    public string? textType { get; init; }
    public double? width { get; init; }
    public HiprintTableField[]? fields { get; init; }
    public string? tableFooterRepeat { get; init; }
    public string? tableHeaderRepeat { get; init; }
}

public sealed record HiprintTableColumn
{
    public string align { get; init; } = "center";
    public string field { get; init; } = string.Empty;
    public string title { get; init; } = string.Empty;
    public double width { get; init; }
    public bool? @checked { get; init; }
}

public sealed record HiprintTableField
{
    public string field { get; init; } = string.Empty;
    public string text { get; init; } = string.Empty;
}
