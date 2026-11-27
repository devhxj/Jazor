using System.ComponentModel;

namespace Jero.Web.Infrastructure.Browser;

/// <summary>审批流程设计器使用的 LogicFlow 默认导出及核心样式。</summary>
[ECMAScript("@logicflow/core")]
[Style("@logicflow/core/dist/index.css")]
internal static class ApprovalLogicFlowCore
{
#pragma warning disable CS0626 // 由 JavaScript 提供这些属性的实现。
    [Description("@#default")]
    public extern static object Constructor { get; }
#pragma warning restore CS0626
}

/// <summary>审批流程设计器注册的 LogicFlow 扩展及其样式。</summary>
[ECMAScript("@logicflow/extension")]
[Style("@logicflow/extension/lib/style/index.css")]
internal static class ApprovalLogicFlowExtensions
{
#pragma warning disable CS0626 // JavaScript owns these accessors.
    [Description("@#BpmnElement")]
    public extern static ApprovalLogicFlowPlugin BpmnElement { get; }

    [Description("@#DndPanel")]
    public extern static ApprovalLogicFlowPlugin DndPanel { get; }

    [Description("@#MiniMap")]
    public extern static ApprovalLogicFlowPlugin MiniMap { get; }

    [Description("@#SelectionSelect")]
    public extern static ApprovalLogicFlowPlugin SelectionSelect { get; }

    [Description("@#Snapshot")]
    public extern static ApprovalLogicFlowPlugin Snapshot { get; }
#pragma warning restore CS0626
}

[ECMAScript]
public sealed class ApprovalLogicFlowPlugin;

/// <summary>审批流程画布实例，用于渲染图、处理交互和读取原始图数据。</summary>
[ECMAScript]
public sealed class ApprovalLogicFlowInstance
{
#pragma warning disable CS0626 // JavaScript owns these accessors.
    [Description("@#extension")]
    public extern ApprovalLogicFlowExtension Extension { get; }
#pragma warning restore CS0626

    [Description("@#render")]
    public extern void Render(object graph);

    /// <summary>销毁画布实例，供设计器卸载时释放资源。</summary>
    [Description("@#destroy")]
    public extern void Destroy();

    [Description("@#setTheme")]
    public extern void SetTheme(ApprovalLogicFlowTheme theme);

    [Description("@#setPatternItems")]
    public extern void SetPatternItems(ApprovalLogicFlowPatternItem[] items);

    [Description("@#on")]
    public extern void On(string eventName, Action<object?> handler);

    [Description("@#getGraphRawData")]
    public extern object GetGraphRawData();

    [Description("@#getDataById")]
    public extern object? GetDataById(string id);

    [Description("@#updateText")]
    public extern void UpdateText(string id, string value);

    [Description("@#deleteElement")]
    public extern void DeleteElement(string id);

    [Description("@#focusOn")]
    public extern void FocusOn(string id);

    [Description("@#editText")]
    public extern void EditText(string id);

    [Description("@#fitView")]
    public extern void FitView(double verticalOffset, double horizontalOffset);

    [Description("@#translateCenter")]
    public extern void TranslateCenter();

    [Description("@#zoom")]
    public extern void Zoom(bool zoomIn);

    [Description("@#resetZoom")]
    public extern void ResetZoom();

    [Description("@#undo")]
    public extern void Undo();

    [Description("@#redo")]
    public extern void Redo();

    [Description("@#clearData")]
    public extern void ClearData();
}

[ECMAScript]
public sealed class ApprovalLogicFlowExtension
{
#pragma warning disable CS0626 // JavaScript owns these accessors.
    [Description("@#selectionSelect")]
    public extern ApprovalLogicFlowSelectionSelect? SelectionSelect { get; }
#pragma warning restore CS0626
}

[ECMAScript]
public sealed class ApprovalLogicFlowSelectionSelect
{
    [Description("@#openSelectionSelect")]
    public extern void Open();

    [Description("@#closeSelectionSelect")]
    public extern void Close();
}

public sealed record ApprovalLogicFlowOptions
{
    [ECMAScriptName("container")]
    public HTMLElement Container { get; init; } = default!;
    [ECMAScriptName("autoExpand")]
    public bool AutoExpand { get; init; }
    [ECMAScriptName("metaKeyMultipleSelected")]
    public bool MetaKeyMultipleSelected { get; init; }
    [ECMAScriptName("snapline")]
    public bool Snapline { get; init; }
    [ECMAScriptName("grid")]
    public ApprovalLogicFlowGrid Grid { get; init; } = new();
    [ECMAScriptName("keyboard")]
    public ApprovalLogicFlowKeyboard Keyboard { get; init; } = new();
    [ECMAScriptName("plugins")]
    public ApprovalLogicFlowPlugin[] Plugins { get; init; } = [];
}

public sealed record ApprovalLogicFlowGrid
{
    [ECMAScriptName("size")]
    public double Size { get; init; } = 10;
    [ECMAScriptName("type")]
    public string Type { get; init; } = "dot";
}

public sealed record ApprovalLogicFlowKeyboard
{
    [ECMAScriptName("enabled")]
    public bool Enabled { get; init; } = true;
}

public sealed record ApprovalLogicFlowTheme
{
    [ECMAScriptName("snapline")]
    public ApprovalLogicFlowSnapline Snapline { get; init; } = new();
}

public sealed record ApprovalLogicFlowSnapline
{
    [ECMAScriptName("stroke")]
    public string Stroke { get; init; } = "#2563EB";
    [ECMAScriptName("strokeWidth")]
    public double StrokeWidth { get; init; } = 1;
}

public sealed record ApprovalLogicFlowPatternItem
{
    [ECMAScriptName("type")]
    public string Type { get; init; } = string.Empty;
    [ECMAScriptName("text")]
    public string Text { get; init; } = string.Empty;
    [ECMAScriptName("label")]
    public string Label { get; init; } = string.Empty;
    [ECMAScriptName("icon")]
    public string Icon { get; init; } = string.Empty;
}
