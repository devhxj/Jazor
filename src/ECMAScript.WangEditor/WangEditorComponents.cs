using System.Collections.Generic;
using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;
using static ECMAScript.Vue;

namespace ECMAScript;

/// <summary>
/// Razor authoring proxy for WangEditor's <c>Editor</c> component.
/// 富文本编辑器组件的 Razor 创作代理；<c>ModelValue</c> 承载 HTML 内容并支持双向更新。
/// </summary>
[ECMAScript("@wangeditor/editor-for-vue", Transform.Component, "Editor")]
public sealed class WangEditorComponent : ComponentBase, IVueComponent
{
    /// <summary>
    /// 编辑器当前 HTML 内容；与组件双向更新。
    /// The editor's current HTML content; bound two-way with the component.
    /// </summary>
    [Parameter]
    public string? ModelValue { get; set; }

    /// <summary>
    /// 内容变化时回调（用于 <c>@bind-ModelValue</c> 语义）。
    /// Invoked when the content changes (used with the <c>@bind-ModelValue</c> pattern).
    /// </summary>
    [Parameter]
    public EventCallback<string> ModelValueChanged { get; set; }

    /// <summary>
    /// 编辑器模式。
    /// The editor mode.
    /// </summary>
    [Parameter]
    public WangEditorMode? Mode { get; set; }

    /// <summary>
    /// 初始 HTML 内容。
    /// The initial HTML content.
    /// </summary>
    [Parameter]
    public string? DefaultHtml { get; set; }

    /// <summary>
    /// 编辑器配置（占位文本与生命周期回调）。
    /// The editor configuration (placeholder text and lifecycle callbacks).
    /// </summary>
    [Parameter]
    public WangEditorConfig? DefaultConfig { get; set; }

    /// <summary>
    /// 传给 Editor 的其他宿主属性；键使用 Vue/HTML 实际属性名。
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
}

/// <summary>
/// Razor authoring proxy for WangEditor's <c>Toolbar</c> component.
/// 富文本编辑器工具栏组件的 Razor 创作代理；需要与 <see cref="WangEditorComponent"/> 的实例关联。
/// </summary>
[ECMAScript("@wangeditor/editor-for-vue", Transform.Component, "Toolbar")]
public sealed class WangEditorToolbar : ComponentBase, IVueComponent
{
    /// <summary>
    /// 与工具栏关联的编辑器实例。
    /// The editor instance this toolbar is associated with.
    /// </summary>
    [Parameter]
    public WangEditorInstance? Editor { get; set; }

    /// <summary>
    /// 工具栏模式。
    /// The toolbar mode.
    /// </summary>
    [Parameter]
    public WangEditorMode? Mode { get; set; }

    /// <summary>
    /// 工具栏配置。
    /// The toolbar configuration.
    /// </summary>
    [Parameter]
    public WangEditorToolbarConfig? DefaultConfig { get; set; }

    /// <summary>
    /// 传给 Toolbar 的其他宿主属性；键使用 Vue/HTML 实际属性名。
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
}
