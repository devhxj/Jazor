using System.ComponentModel;
using ECMAScript;
using static ECMAScript.Vue;

namespace Jero.Web.Pages.Database;

// 包将默认导出声明为 VueElement 子类，这里按 Vue 组件及其公开实例使用。
// 底层 setJsonData 实际返回 Promise。
[ECMAScript("relation-graph/vue3")]
public static class DatabaseGraphPackage
{
#pragma warning disable CS0626 // JavaScript owns this accessor.
    [Description("@#default")]
    public static extern IVueComponent Component { get; }
#pragma warning restore CS0626
}

public sealed record DatabaseGraphProps([property: ECMAScriptName("options")] DatabaseGraphOptions Options) : VueProps;

public sealed record DatabaseGraphOptions(
    [property: ECMAScriptName("defaultJunctionPoint")] string DefaultJunctionPoint,
    [property: ECMAScriptName("allowShowMiniToolBar")] bool AllowShowMiniToolBar,
    [property: ECMAScriptName("allowShowZoomMenu")] bool AllowShowZoomMenu,
    [property: ECMAScriptName("allowShowRefreshButton")] bool AllowShowRefreshButton,
    [property: ECMAScriptName("allowShowDownloadButton")] bool AllowShowDownloadButton,
    [property: ECMAScriptName("allowShowFullscreenMenu")] bool AllowShowFullscreenMenu,
    [property: ECMAScriptName("moveToCenterWhenRefresh")] bool MoveToCenterWhenRefresh,
    [property: ECMAScriptName("zoomToFitWhenRefresh")] bool ZoomToFitWhenRefresh,
    [property: ECMAScriptName("useAnimationWhenRefresh")] bool UseAnimationWhenRefresh,
    [property: ECMAScriptName("disableNodeClickEffect")] bool DisableNodeClickEffect,
    [property: ECMAScriptName("disableLineClickEffect")] bool DisableLineClickEffect,
    [property: ECMAScriptName("minCanvasZoom")] int MinCanvasZoom,
    [property: ECMAScriptName("backgroundColor")] string BackgroundColor);

public sealed record DatabaseGraphNode(
    [property: ECMAScriptName("id")] string Id,
    [property: ECMAScriptName("text")] string Text,
    [property: ECMAScriptName("html")] string Html,
    [property: ECMAScriptName("x")] double X,
    [property: ECMAScriptName("y")] double Y,
    [property: ECMAScriptName("nodeShape")] int NodeShape);

public sealed record DatabaseGraphLine(
    [property: ECMAScriptName("id")] string Id,
    [property: ECMAScriptName("from")] string From,
    [property: ECMAScriptName("to")] string To,
    [property: ECMAScriptName("color")] string Color,
    [property: ECMAScriptName("text")] string Text,
    [property: ECMAScriptName("fromJunctionPoint")] string FromJunctionPoint,
    [property: ECMAScriptName("toJunctionPoint")] string ToJunctionPoint,
    [property: ECMAScriptName("lineShape")] int LineShape,
    [property: ECMAScriptName("lineWidth")] int LineWidth);

public sealed record DatabaseGraphData(
    [property: ECMAScriptName("nodes")] DatabaseGraphNode[] Nodes,
    [property: ECMAScriptName("lines")] DatabaseGraphLine[] Lines);

[ECMAScript]
public sealed class DatabaseGraphComponent
{
    [Description("@#getInstance")]
    public extern DatabaseGraphCore GetInstance();

    [Description("@#onGraphResize")]
    public extern void OnGraphResize();
}

[ECMAScript]
public sealed class DatabaseGraphCore
{
#pragma warning disable CS0626 // JavaScript owns this accessor.
    [Description("@#options")]
    public extern DatabaseGraphCanvasOptions Options { get; }
#pragma warning restore CS0626
    [Description("@#setJsonData")]
    public extern IPromise SetJsonData(DatabaseGraphData data, bool layout);
    [Description("@#addElementLines")]
    public extern void AddElementLines(DatabaseGraphLine[] lines);
    [Description("@#stopAutoLayout")]
    public extern void StopAutoLayout();
    [Description("@#clearGraph")]
    public extern void ClearGraph();
    [Description("@#moveToCenter")]
    public extern void MoveToCenter();
    [Description("@#zoomToFit")]
    public extern void ZoomToFit();
    [Description("@#setZoom")]
    public extern void SetZoom(double zoom);
}

[ECMAScript]
public sealed class DatabaseGraphCanvasOptions
{
#pragma warning disable CS0626 // JavaScript owns this accessor.
    [Description("@#canvasZoom"), ECMAScriptOptional]
    public extern double? CanvasZoom { get; }
#pragma warning restore CS0626
}
