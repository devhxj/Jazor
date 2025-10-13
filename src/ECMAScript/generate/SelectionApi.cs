namespace ECMAScript;

/// <summary>
/// GetComposedRangesOptions
/// </summary>
[ECMAScript]
[Description("@#GetComposedRangesOptions")]
public record GetComposedRangesOptions(
    [property: Description("@#shadowRoots")]ShadowRoot[]? ShadowRoots = default);

/// <summary>
/// Selection
/// </summary>
[ECMAScript]
[Description("@#Selection")]
public class Selection
{
    /// <summary>
    /// anchorNode
    /// </summary>
    [Description("@#anchorNode")]
    public extern Node? AnchorNode { get; }

    /// <summary>
    /// anchorOffset
    /// </summary>
    [Description("@#anchorOffset")]
    public extern uint AnchorOffset { get; }

    /// <summary>
    /// focusNode
    /// </summary>
    [Description("@#focusNode")]
    public extern Node? FocusNode { get; }

    /// <summary>
    /// focusOffset
    /// </summary>
    [Description("@#focusOffset")]
    public extern uint FocusOffset { get; }

    /// <summary>
    /// isCollapsed
    /// </summary>
    [Description("@#isCollapsed")]
    public extern bool IsCollapsed { get; }

    /// <summary>
    /// rangeCount
    /// </summary>
    [Description("@#rangeCount")]
    public extern uint RangeCount { get; }

    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern string Type { get; }

    /// <summary>
    /// direction
    /// </summary>
    [Description("@#direction")]
    public extern string Direction { get; }

    /// <summary>
    /// getRangeAt
    /// </summary>
    /// <param name="index">index</param>
    [Description("@#getRangeAt")]
    public extern Range GetRangeAt(uint index);

    /// <summary>
    /// addRange
    /// </summary>
    /// <param name="range">range</param>
    [Description("@#addRange")]
    public extern void AddRange(Range range);

    /// <summary>
    /// removeRange
    /// </summary>
    /// <param name="range">range</param>
    [Description("@#removeRange")]
    public extern void RemoveRange(Range range);

    /// <summary>
    /// removeAllRanges
    /// </summary>
    [Description("@#removeAllRanges")]
    public extern void RemoveAllRanges();

    /// <summary>
    /// empty
    /// </summary>
    [Description("@#empty")]
    public extern void Empty();

    /// <summary>
    /// getComposedRanges
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#getComposedRanges")]
    public extern StaticRange[] GetComposedRanges(GetComposedRangesOptions? options = default);

    /// <summary>
    /// collapse
    /// </summary>
    /// <param name="node">node</param>
    /// <param name="offset">offset</param>
    [Description("@#collapse")]
    public extern void Collapse(Node? node, uint offset = 0);

    /// <summary>
    /// setPosition
    /// </summary>
    /// <param name="node">node</param>
    /// <param name="offset">offset</param>
    [Description("@#setPosition")]
    public extern void SetPosition(Node? node, uint offset = 0);

    /// <summary>
    /// collapseToStart
    /// </summary>
    [Description("@#collapseToStart")]
    public extern void CollapseToStart();

    /// <summary>
    /// collapseToEnd
    /// </summary>
    [Description("@#collapseToEnd")]
    public extern void CollapseToEnd();

    /// <summary>
    /// extend
    /// </summary>
    /// <param name="node">node</param>
    /// <param name="offset">offset</param>
    [Description("@#extend")]
    public extern void Extend(Node node, uint offset = 0);

    /// <summary>
    /// setBaseAndExtent
    /// </summary>
    /// <param name="anchorNode">anchorNode</param>
    /// <param name="anchorOffset">anchorOffset</param>
    /// <param name="focusNode">focusNode</param>
    /// <param name="focusOffset">focusOffset</param>
    [Description("@#setBaseAndExtent")]
    public extern void SetBaseAndExtent(Node anchorNode, uint anchorOffset, Node focusNode, uint focusOffset);

    /// <summary>
    /// selectAllChildren
    /// </summary>
    /// <param name="node">node</param>
    [Description("@#selectAllChildren")]
    public extern void SelectAllChildren(Node node);

    /// <summary>
    /// modify
    /// </summary>
    /// <param name="alter">alter</param>
    /// <param name="direction">direction</param>
    /// <param name="granularity">granularity</param>
    [Description("@#modify")]
    public extern void Modify(string alter, string direction, string granularity);

    /// <summary>
    /// deleteFromDocument
    /// </summary>
    [Description("@#deleteFromDocument")]
    public extern void DeleteFromDocument();

    /// <summary>
    /// containsNode
    /// </summary>
    /// <param name="node">node</param>
    /// <param name="allowPartialContainment">allowPartialContainment</param>
    [Description("@#containsNode")]
    public extern bool ContainsNode(Node node, bool allowPartialContainment = false);
}