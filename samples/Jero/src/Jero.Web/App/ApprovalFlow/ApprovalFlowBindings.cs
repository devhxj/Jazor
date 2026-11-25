using System.ComponentModel;

namespace Jero.Web.App.ApprovalFlow;

/// <summary>LogicFlow core default export and stylesheet.</summary>
[ECMAScript("@logicflow/core")]
[Style("@logicflow/core/dist/index.css")]
internal static class ApprovalLogicFlowCore
{
#pragma warning disable CS0626 // JavaScript owns these accessors.
    [Description("@#default")]
    public extern static object Constructor { get; }
#pragma warning restore CS0626
}

/// <summary>LogicFlow extension named exports used by the approval designer.</summary>
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

[ECMAScript]
public sealed class ApprovalLogicFlowInstance
{
#pragma warning disable CS0626 // JavaScript owns these accessors.
    [Description("@#extension")]
    public extern ApprovalLogicFlowExtension Extension { get; }
#pragma warning restore CS0626

    [Description("@#render")]
    public extern void Render(object graph);

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
    public HTMLElement container { get; init; } = default!;
    public bool autoExpand { get; init; }
    public bool metaKeyMultipleSelected { get; init; }
    public bool snapline { get; init; }
    public ApprovalLogicFlowGrid grid { get; init; } = new();
    public ApprovalLogicFlowKeyboard keyboard { get; init; } = new();
    public ApprovalLogicFlowPlugin[] plugins { get; init; } = [];
}

public sealed record ApprovalLogicFlowGrid
{
    public double size { get; init; } = 10;
    public string type { get; init; } = "dot";
}

public sealed record ApprovalLogicFlowKeyboard
{
    public bool enabled { get; init; } = true;
}

public sealed record ApprovalLogicFlowTheme
{
    public ApprovalLogicFlowSnapline snapline { get; init; } = new();
}

public sealed record ApprovalLogicFlowSnapline
{
    public string stroke { get; init; } = "#2563EB";
    public double strokeWidth { get; init; } = 1;
}

public sealed record ApprovalLogicFlowPatternItem
{
    public string type { get; init; } = string.Empty;
    public string text { get; init; } = string.Empty;
    public string label { get; init; } = string.Empty;
    public string icon { get; init; } = string.Empty;
}
