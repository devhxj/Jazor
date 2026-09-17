using System;
using System.ComponentModel;
using ECMAScript.Contract;

namespace ECMAScript;

/// <summary>
/// 编辑器模式：<c>default</c> 或 <c>simple</c>；字符串值与 WangEditor <c>mode</c> 一致。
/// The editor mode: <c>default</c> or <c>simple</c>; values match the WangEditor <c>mode</c>.
/// </summary>
[String]
public enum WangEditorMode
{
    /// <summary>完整模式。The full-featured mode.</summary>
    [Description("@#default")]
    Default,

    /// <summary>简洁模式。The simplified mode.</summary>
    [Description("@#simple")]
    Simple
}

/// <summary>
/// 编辑器实例（不透明宿主值）；由 <c>createEditor()</c> 或 <c>Editor</c> 组件创建。
/// An editor instance (opaque host value) created by <c>createEditor()</c> or the <c>Editor</c> component.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class WangEditorInstance
{
    private WangEditorInstance()
    {
    }
}

/// <summary>
/// 工具栏实例（不透明宿主值）；由 <c>createToolbar()</c> 创建。
/// A toolbar instance (opaque host value) created by <c>createToolbar()</c>.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class WangEditorToolbarInstance
{
    private WangEditorToolbarInstance()
    {
    }
}

/// <summary>
/// 编辑器配置（slate 内容与菜单/上传等）；对应 <c>IEditorConfig</c> 的常用子集。
/// Editor configuration (slate content plus menus and upload); the frequently used subset of <c>IEditorConfig</c>.
/// </summary>
[ECMAScript]
[Description("@#")]
public record WangEditorConfig
{
    /// <summary>占位提示文本。The placeholder text.</summary>
    [Description("@#placeholder")]
    public string? Placeholder { get; init; }

    /// <summary>是否随输入实时返回 HTML。Whether HTML is emitted on every input.</summary>
    [Description("@#onChange")]
    public Action<WangEditorInstance>? OnChange { get; init; }

    /// <summary>编辑器创建完成后的回调。Callback invoked after the editor is created.</summary>
    [Description("@#onCreated")]
    public Action<WangEditorInstance>? OnCreated { get; init; }

    /// <summary>编辑器销毁时的回调。Callback invoked when the editor is destroyed.</summary>
    [Description("@#onDestroyed")]
    public Action<WangEditorInstance>? OnDestroyed { get; init; }

    /// <summary>获得焦点时的回调。Callback invoked when the editor gains focus.</summary>
    [Description("@#onFocus")]
    public Action<WangEditorInstance>? OnFocus { get; init; }

    /// <summary>失去焦点时的回调。Callback invoked when the editor loses focus.</summary>
    [Description("@#onBlur")]
    public Action<WangEditorInstance>? OnBlur { get; init; }
}

/// <summary>
/// 工具栏配置；对应 <c>IToolbarConfig</c> 的常用子集。
/// Toolbar configuration; the frequently used subset of <c>IToolbarConfig</c>.
/// </summary>
[ECMAScript]
[Description("@#")]
public record WangEditorToolbarConfig
{
    /// <summary>自定义工具栏键列表。The custom toolbar key list.</summary>
    [Description("@#toolbarKeys")]
    public string[]? ToolbarKeys { get; init; }

    /// <summary>是否排除默认工具栏键。Whether the default toolbar keys are excluded.</summary>
    [Description("@#excludeKeys")]
    public string[]? ExcludeKeys { get; init; }
}
