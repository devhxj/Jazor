using Jero.Web.Infrastructure.Api.Print;
using static ECMAScript.Global;

namespace Jero.Web.Pages.Print;

/// <summary>
/// 页面保存响应式状态。本模块持有 Hiprint 设计器实例，
/// 并管理依赖 DOM 的打印流程。
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
    private static HiprintApi _api = default!;

    public static HiprintDesignerSession CreateDesigner(HiprintRuntime runtime, int modeIndex, string host, string templateText)
    {
        // Init 会同步调用下面注册的 Provider。先保存当前 Hiprint 实例，回调才能用它创建打印元素组。
        var hiprint = runtime.Hiprint;
        _api = hiprint;
        var normalizedMode = PrintShared.NormalizeModeIndex(modeIndex) == 1 ? 1 : 0;
        var providerName = normalizedMode == 1 ? BProviderName : AProviderName;
        var provider = new HiprintProviderFactory
        {
            AddElementTypes = normalizedMode == 1 ? AddBProviderTypes : AddAProviderTypes
        };

        hiprint.Init(new HiprintInitOptions
        {
            Host = PrintShared.Optional(host),
            Providers = [provider]
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
                Template = templateData,
                SettingContainer = "#" + SettingContainerId,
                PaginationContainer = ".jero-print-pagination",
                FontList =
                [
                    new() { Title = "微软雅黑", Value = "Microsoft YaHei" },
                    new() { Title = "宋体", Value = "SimSun" },
                    new() { Title = "黑体", Value = "STHeitiSC-Light" },
                    new() { Title = "Arial", Value = "Arial" },
                    new() { Title = "华为楷体", Value = "STKaiti" },
                    new() { Title = "cursive", Value = "cursive" },
                    new() { Title = "Vector", Value = "Vector" }
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
        // Element Plus 可能先触发 opened，随后才能查询到 Teleport 移出的弹窗内容。
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
            new HiprintPrintCallbacks { Callback = completed });

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

        // 先注册完成回调，再发出打印请求，因为本地打印服务可能立即响应。
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
                new() { Tid = "aProviderModule.logo", Title = "Logo", Type = "image", Data = "/images/logo.png", Options = new() { Field = "imageUrl" } },
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
                new() { Tid = "bProviderModule.image", Title = "Logo", Type = "image", Data = "/images/logo.png" }
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
            _api.PrintElementTypeGroup,
            new object?[] { title, elements });

    private static HiprintElementDefinition Shape(string tid, string title, string type)
        => new() { Tid = tid, Title = title, Type = type };

    private static HiprintElementDefinition CustomText(string tid)
        => new()
        {
            Tid = tid,
            Title = "文本",
            Type = "text",
            Custom = true,
            CustomText = "自定义文本",
            Options = new() { TestData = "自定义文本", Width = 200 }
        };

    private static HiprintElementDefinition LongText(string tid)
        => new()
        {
            Tid = tid,
            Title = "长文本",
            Type = "longText",
            Options = new() { Field = "longText", TestData = "长文本分页/不分页测试", Width = 200 }
        };

    private static HiprintElementDefinition Heading(string tid, string title, double fontSize, string? data = null)
        => new()
        {
            Tid = tid,
            Title = title,
            Type = "text",
            Data = data ?? title,
            Options = new()
            {
                FontSize = fontSize,
                FontWeight = "700",
                Height = title == "单据类型" ? 16 : 17,
                HideTitle = true,
                TestData = data ?? title,
                TextAlign = "center"
            }
        };

    private static HiprintElementDefinition FieldText(string tid, string title, string data, string field)
        => new()
        {
            Tid = tid,
            Title = title,
            Type = "text",
            Data = data,
            Options = new()
            {
                Field = field,
                FontSize = 6.75,
                FontWeight = "700",
                Height = 16,
                TestData = data,
                TextAlign = "left",
                TextContentVerticalAlign = "middle"
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
            Tid = tid,
            Title = title,
            Type = "text",
            Data = data,
            Options = new()
            {
                Field = field,
                FontSize = 12,
                FontWeight = "700",
                Height = height,
                LineHeight = 18,
                TestData = "Z18012345678",
                TextAlign = "left",
                TextContentVerticalAlign = "middle",
                TextType = textType,
                Width = width
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
            fields[index] = new HiprintTableField { Field = columns[index].Field, Text = columns[index].Title };

        return new HiprintElementDefinition
        {
            Tid = tid,
            Title = title,
            Type = "table",
            Editable = true,
            ColumnAlignEditable = true,
            ColumnDisplayEditable = true,
            ColumnDisplayIndexEditable = true,
            ColumnResizable = true,
            ColumnTitleEditable = true,
            IsEnableContextMenu = true,
            IsEnableDeleteColumn = true,
            IsEnableDeleteRow = true,
            IsEnableEditField = true,
            IsEnableInsertColumn = true,
            IsEnableInsertRow = true,
            IsEnableMergeCell = true,
            Columns = [columns],
            Options = new HiprintElementOptions
            {
                Field = "table",
                Fields = fields,
                TableFooterRepeat = includeDetail ? null : "last",
                TableHeaderRepeat = includeDetail ? null : "first"
            }
        };
    }

    private static HiprintTableColumn Column(string field, string title, double width, bool? visible = null)
        => new() { Align = "center", Field = field, Title = title, Width = width, Checked = visible };

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
    [ECMAScriptName("tid")]
    public string Tid { get; init; } = string.Empty;
    [ECMAScriptName("title")]
    public string Title { get; init; } = string.Empty;
    [ECMAScriptName("type")]
    public string Type { get; init; } = string.Empty;
    [ECMAScriptName("data")]
    public string? Data { get; init; }
    [ECMAScriptName("custom")]
    public bool? Custom { get; init; }
    [ECMAScriptName("customText")]
    public string? CustomText { get; init; }
    [ECMAScriptName("editable")]
    public bool? Editable { get; init; }
    [ECMAScriptName("columnAlignEditable")]
    public bool? ColumnAlignEditable { get; init; }
    [ECMAScriptName("columnDisplayEditable")]
    public bool? ColumnDisplayEditable { get; init; }
    [ECMAScriptName("columnDisplayIndexEditable")]
    public bool? ColumnDisplayIndexEditable { get; init; }
    [ECMAScriptName("columnResizable")]
    public bool? ColumnResizable { get; init; }
    [ECMAScriptName("columnTitleEditable")]
    public bool? ColumnTitleEditable { get; init; }
    [ECMAScriptName("isEnableContextMenu")]
    public bool? IsEnableContextMenu { get; init; }
    [ECMAScriptName("isEnableDeleteColumn")]
    public bool? IsEnableDeleteColumn { get; init; }
    [ECMAScriptName("isEnableDeleteRow")]
    public bool? IsEnableDeleteRow { get; init; }
    [ECMAScriptName("isEnableEditField")]
    public bool? IsEnableEditField { get; init; }
    [ECMAScriptName("isEnableInsertColumn")]
    public bool? IsEnableInsertColumn { get; init; }
    [ECMAScriptName("isEnableInsertRow")]
    public bool? IsEnableInsertRow { get; init; }
    [ECMAScriptName("isEnableMergeCell")]
    public bool? IsEnableMergeCell { get; init; }
    [ECMAScriptName("columns")]
    public HiprintTableColumn[][]? Columns { get; init; }
    [ECMAScriptName("options")]
    public HiprintElementOptions? Options { get; init; }
}

public sealed record HiprintElementOptions
{
    [ECMAScriptName("field")]
    public string? Field { get; init; }
    [ECMAScriptName("fontSize")]
    public double? FontSize { get; init; }
    [ECMAScriptName("fontWeight")]
    public string? FontWeight { get; init; }
    [ECMAScriptName("height")]
    public double? Height { get; init; }
    [ECMAScriptName("hideTitle")]
    public bool? HideTitle { get; init; }
    [ECMAScriptName("lineHeight")]
    public double? LineHeight { get; init; }
    [ECMAScriptName("testData")]
    public string? TestData { get; init; }
    [ECMAScriptName("textAlign")]
    public string? TextAlign { get; init; }
    [ECMAScriptName("textContentVerticalAlign")]
    public string? TextContentVerticalAlign { get; init; }
    [ECMAScriptName("textType")]
    public string? TextType { get; init; }
    [ECMAScriptName("width")]
    public double? Width { get; init; }
    [ECMAScriptName("fields")]
    public HiprintTableField[]? Fields { get; init; }
    [ECMAScriptName("tableFooterRepeat")]
    public string? TableFooterRepeat { get; init; }
    [ECMAScriptName("tableHeaderRepeat")]
    public string? TableHeaderRepeat { get; init; }
}

public sealed record HiprintTableColumn
{
    [ECMAScriptName("align")]
    public string Align { get; init; } = "center";
    [ECMAScriptName("field")]
    public string Field { get; init; } = string.Empty;
    [ECMAScriptName("title")]
    public string Title { get; init; } = string.Empty;
    [ECMAScriptName("width")]
    public double Width { get; init; }
    [ECMAScriptName("checked")]
    public bool? Checked { get; init; }
}

public sealed record HiprintTableField
{
    [ECMAScriptName("field")]
    public string Field { get; init; } = string.Empty;
    [ECMAScriptName("text")]
    public string Text { get; init; } = string.Empty;
}
