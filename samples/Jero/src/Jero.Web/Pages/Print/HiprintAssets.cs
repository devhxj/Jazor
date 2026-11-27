using Jero.Web.Infrastructure.Api.Print;
using System.ComponentModel;

namespace Jero.Web.Pages.Print;

/// <summary>
/// Hiprint 按需导入返回的模块命名空间。
/// </summary>
[ECMAScript]
public sealed class HiprintExports
{
#pragma warning disable CS0626 // JavaScript owns these accessors.
    [Description("@#default")]
    public extern HiprintRuntime Default { get; }
#pragma warning restore CS0626

    [Description("@#disAutoConnect")]
    public extern void DisableAutoConnect();
}

[ECMAScript]
public sealed class HiprintRuntime
{
#pragma warning disable CS0626 // JavaScript owns these accessors.
    [Description("@#hiprint")]
    public extern HiprintApi Hiprint { get; }
#pragma warning restore CS0626
}

/// <summary>Hiprint members used by Jero's template designer.</summary>
[ECMAScript]
public sealed class HiprintApi
{
#pragma warning disable CS0626 // JavaScript owns these accessors.
    public extern object PrintElementTypeGroup { get; }

    public extern HiprintPrintElementTypeManager PrintElementTypeManager { get; }

    public extern object PrintTemplate { get; }
#pragma warning restore CS0626

    [Description("@#init")]
    public extern void Init(HiprintInitOptions options);
}

[ECMAScript]
public sealed class HiprintPrintElementTypeManager
{
    [Description("@#build")]
    public extern void Build(string selector, string providerName);
}

[ECMAScript]
public sealed class HiprintProviderContext
{
    [Description("@#removePrintElementTypes")]
    public extern void RemovePrintElementTypes(string providerName);

    [Description("@#addPrintElementTypes")]
    public extern void AddPrintElementTypes(string providerName, HiprintElementTypeGroup[] groups);
}

[ECMAScript]
public sealed class HiprintElementTypeGroup;

[ECMAScript]
public sealed class HiprintPrintTemplate
{
#pragma warning disable CS0626 // JavaScript owns these accessors.
    [Description("@#editingPanel")]
    public extern HiprintEditingPanel? EditingPanel { get; }
#pragma warning restore CS0626

    [Description("@#design")]
    public extern void Design(string selector);

    [Description("@#update")]
    public extern void Update(object template);

    [Description("@#getJson")]
    public extern object? GetJson();

    [Description("@#setPaper")]
    public extern void SetPaper(double width, double height);

    [Description("@#zoom")]
    public extern void Zoom(double scale);

    [Description("@#rotatePaper")]
    public extern void RotatePaper();

    [Description("@#setElsAlign")]
    public extern void SetElementsAlign(string alignment);

    [Description("@#clear")]
    public extern void Clear();

    [Description("@#getHtml")]
    public extern object? GetHtml(object printData);

    [Description("@#getPrinterList")]
    public extern HiprintPrinter[] GetPrinterList();

    [Description("@#print")]
    public extern void Print(object printData, HiprintBrowserPrintOptions options, HiprintPrintCallbacks callbacks);

    [Description("@#print2")]
    public extern void PrintClient(object printData, object printOptions);

    [Description("@#on")]
    public extern void On(string eventName, Action<object?> handler);

    [Description("@#toPdf")]
    public extern void ToPdf(object printData, string fileName);
}

[ECMAScript]
public sealed class HiprintEditingPanel
{
#pragma warning disable CS0626 // JavaScript owns these accessors.
    [Description("@#scale")]
    public extern double Scale { get; }
#pragma warning restore CS0626
}

[ECMAScript]
public sealed class HiprintPrinter
{
#pragma warning disable CS0626 // JavaScript owns these accessors.
    [Description("@#name")]
    public extern string Name { get; }
#pragma warning restore CS0626
}

public sealed record HiprintInitOptions
{
    [ECMAScriptName("host")]
    public string? Host { get; init; }
    [ECMAScriptName("providers")]
    public HiprintProviderFactory[] Providers { get; init; } = [];
}

public sealed record HiprintProviderFactory
{
    [ECMAScriptName("addElementTypes")]
    public Action<HiprintProviderContext> AddElementTypes { get; init; } = default!;
}

public sealed record HiprintPrintTemplateOptions
{
    [ECMAScriptName("template")]
    public object Template { get; init; } = default!;
    [ECMAScriptName("settingContainer")]
    public string SettingContainer { get; init; } = string.Empty;
    [ECMAScriptName("paginationContainer")]
    public string PaginationContainer { get; init; } = string.Empty;
    [ECMAScriptName("fontList")]
    public HiprintFontOption[] FontList { get; init; } = [];
}

public sealed record HiprintFontOption
{
    [ECMAScriptName("title")]
    public string Title { get; init; } = string.Empty;
    [ECMAScriptName("value")]
    public string Value { get; init; } = string.Empty;
}

public sealed record HiprintBrowserPrintOptions;

public sealed record HiprintPrintCallbacks
{
    [ECMAScriptName("callback")]
    public Action Callback { get; init; } = default!;
}
