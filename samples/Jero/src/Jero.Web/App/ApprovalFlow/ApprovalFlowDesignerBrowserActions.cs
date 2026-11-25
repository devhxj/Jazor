using static ECMAScript.Global;

namespace Jero.Web.App.ApprovalFlow;

/// <summary>
/// LogicFlow 与 DOM 生命周期边界。页面只保存可响应状态；实例、事件和画布操作均留在 typed ESM 模块内。
/// </summary>
[ECMAScriptModule("components/approval-flow-designer-browser-actions.js")]
public static class ApprovalFlowDesignerBrowserActions
{
    public const string CanvasId = "jero-approval-flow-canvas";

    public static ApprovalFlowDesignerSession Create(
        object graph,
        Action<ApprovalFlowSelection> selectionChanged,
        Action<bool, bool> historyChanged,
        Action graphChanged)
    {
        var container = Document.GetElementById(CanvasId) as HTMLElement;
        if (container is null)
            throw new Exception("流程设计器画布尚未挂载");

        container.ReplaceChildren([]);
        var flow = (ApprovalLogicFlowInstance)Reflect.Construct(
            ApprovalLogicFlowCore.Constructor,
            new object?[]
            {
                new ApprovalLogicFlowOptions
                {
                    container = container,
                    autoExpand = true,
                    metaKeyMultipleSelected = true,
                    snapline = true,
                    grid = new ApprovalLogicFlowGrid { size = 10, type = "dot" },
                    keyboard = new ApprovalLogicFlowKeyboard { enabled = true },
                    plugins =
                    [
                        ApprovalLogicFlowExtensions.BpmnElement,
                        ApprovalLogicFlowExtensions.DndPanel,
                        ApprovalLogicFlowExtensions.MiniMap,
                        ApprovalLogicFlowExtensions.SelectionSelect,
                        ApprovalLogicFlowExtensions.Snapshot
                    ]
                }
            });

        flow.SetTheme(new ApprovalLogicFlowTheme());
        flow.SetPatternItems(CreatePalette());
        BindEvents(flow, selectionChanged, historyChanged, graphChanged);
        flow.Render(graph);
        flow.FitView(80, 160);
        return new ApprovalFlowDesignerSession { Flow = flow };
    }

    public static void Destroy(ApprovalFlowDesignerSession? session)
    {
        session?.Flow.Destroy();
        var container = Document.GetElementById(CanvasId);
        container?.ReplaceChildren([]);
    }

    public static object GetGraph(ApprovalFlowDesignerSession session)
        => session.Flow.GetGraphRawData();

    public static ApprovalFlowSelection ReadSelection(ApprovalFlowDesignerSession session, string id)
        => ReadSelection(session.Flow, id);

    public static void UpdateText(ApprovalFlowDesignerSession session, string id, string value)
        => session.Flow.UpdateText(id, value.Trim());

    public static void DeleteElement(ApprovalFlowDesignerSession session, string id)
        => session.Flow.DeleteElement(id);

    public static void Focus(ApprovalFlowDesignerSession session, string id)
        => session.Flow.FocusOn(id);

    public static void EditText(ApprovalFlowDesignerSession session, string id)
        => session.Flow.EditText(id);

    public static void Fit(ApprovalFlowDesignerSession session)
        => session.Flow.FitView(80, 160);

    public static void Center(ApprovalFlowDesignerSession session)
        => session.Flow.TranslateCenter();

    public static void ZoomIn(ApprovalFlowDesignerSession session)
        => session.Flow.Zoom(true);

    public static void ZoomOut(ApprovalFlowDesignerSession session)
        => session.Flow.Zoom(false);

    public static void ResetZoom(ApprovalFlowDesignerSession session)
        => session.Flow.ResetZoom();

    public static void Undo(ApprovalFlowDesignerSession session)
        => session.Flow.Undo();

    public static void Redo(ApprovalFlowDesignerSession session)
        => session.Flow.Redo();

    public static void Clear(ApprovalFlowDesignerSession session)
        => session.Flow.ClearData();

    public static void Render(ApprovalFlowDesignerSession session, object graph)
        => session.Flow.Render(graph);

    public static void SetSelectionMode(ApprovalFlowDesignerSession session, bool enabled)
    {
        var selection = session.Flow.Extension.SelectionSelect;
        if (enabled)
            selection?.Open();
        else
            selection?.Close();
    }

    private static void BindEvents(
        ApprovalLogicFlowInstance flow,
        Action<ApprovalFlowSelection> selectionChanged,
        Action<bool, bool> historyChanged,
        Action graphChanged)
    {
        flow.On("blank:click", _ => selectionChanged(new ApprovalFlowSelection()));
        flow.On("node:click", payload => SelectFromEvent(flow, payload, selectionChanged));
        flow.On("edge:click", payload => SelectFromEvent(flow, payload, selectionChanged));
        flow.On("graph:rendered", _ => graphChanged());
        flow.On("graph:updated", _ => graphChanged());
        flow.On("text:update", _ => graphChanged());
        flow.On("history:change", payload =>
        {
            var data = payload is null ? null : Reflect.Get(payload, "data");
            historyChanged(ReadBool(data, "undoAble"), ReadBool(data, "redoAble"));
        });
    }

    private static void SelectFromEvent(
        ApprovalLogicFlowInstance flow,
        object? payload,
        Action<ApprovalFlowSelection> selectionChanged)
    {
        var data = payload is null ? null : Reflect.Get(payload, "data");
        var id = ReadString(data, "id");
        selectionChanged(id.Length == 0 ? new ApprovalFlowSelection() : ReadSelection(flow, id));
    }

    private static ApprovalFlowSelection ReadSelection(ApprovalLogicFlowInstance flow, string id)
    {
        var data = flow.GetDataById(id);
        if (data is null)
            return new ApprovalFlowSelection();
        var source = ReadString(data, "sourceNodeId");
        var target = ReadString(data, "targetNodeId");
        return new ApprovalFlowSelection
        {
            id = id,
            type = ReadString(data, "type"),
            kind = source.Length > 0 ? "edge" : "node",
            text = ReadText(Reflect.Get(data, "text")),
            x = ReadString(data, "x"),
            y = ReadString(data, "y"),
            sourceNodeId = source,
            targetNodeId = target
        };
    }

    private static ApprovalLogicFlowPatternItem[] CreatePalette()
        =>
        [
            Pattern("bpmn:startEvent", "开始", "开始", "circle", "E8F5E9", "2E7D32"),
            Pattern("bpmn:userTask", "审批", "审批节点", "rect", "E3F2FD", "1565C0"),
            Pattern("bpmn:serviceTask", "系统", "系统节点", "rect", "FFF3E0", "EF6C00"),
            Pattern("bpmn:exclusiveGateway", "网关", "分支网关", "diamond", "FFF8E1", "F9A825"),
            Pattern("bpmn:endEvent", "结束", "结束", "circle", "FFEBEE", "C62828")
        ];

    private static ApprovalLogicFlowPatternItem Pattern(
        string type,
        string label,
        string text,
        string shape,
        string fill,
        string stroke)
    {
        var body = shape == "diamond"
            ? "<path d='M24 4L44 24L24 44L4 24Z' fill='%23" + fill + "' stroke='%23" + stroke + "' stroke-width='4'/>"
            : shape == "rect"
                ? "<rect x='5' y='9' width='54' height='30' rx='9' fill='%23" + fill + "' stroke='%23" + stroke + "' stroke-width='4'/>"
                : "<circle cx='24' cy='24' r='18' fill='%23" + fill + "' stroke='%23" + stroke + "' stroke-width='4'/>";
        return new ApprovalLogicFlowPatternItem
        {
            type = type,
            label = label,
            text = text,
            icon = "data:image/svg+xml;charset=UTF-8,<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 64 48'>" + body + "</svg>"
        };
    }

    private static string ReadText(object? value)
    {
        if (value is null)
            return string.Empty;
        if (TypeOf(value) == "string")
            return StringValue(value);
        return ReadString(value, "value");
    }

    private static string ReadString(object? value, string key)
    {
        if (value is null)
            return string.Empty;
        var raw = Reflect.Get(value, key);
        return raw is null ? string.Empty : StringValue(raw);
    }

    private static bool ReadBool(object? value, string key)
    {
        if (value is null)
            return false;
        var raw = Reflect.Get(value, key);
        return raw is not null && BooleanValue(raw);
    }
}

public sealed record ApprovalFlowDesignerSession
{
    public ApprovalLogicFlowInstance Flow { get; init; } = default!;
}

public sealed record ApprovalFlowSelection
{
    public string id { get; init; } = string.Empty;
    public string type { get; init; } = string.Empty;
    public string kind { get; init; } = string.Empty;
    public string text { get; init; } = string.Empty;
    public string x { get; init; } = string.Empty;
    public string y { get; init; } = string.Empty;
    public string sourceNodeId { get; init; } = string.Empty;
    public string targetNodeId { get; init; } = string.Empty;
}
