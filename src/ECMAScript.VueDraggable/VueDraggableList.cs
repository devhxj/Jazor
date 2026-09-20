using System.Collections.Generic;
using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;
using static ECMAScript.Vue;

namespace ECMAScript;

/// <summary>
/// Razor authoring proxy for vue-draggable-plus's <c>VueDraggable</c> component.
/// 面向前端列表拖拽排序的 Razor 创作代理；列表与插槽项类型由泛型参数精确表达。
/// </summary>
/// <typeparam name="TItem">列表元素类型。The list item type.</typeparam>
[ECMAScriptName("VueDraggable")]
[ECMAScript("vue-draggable-plus")]
public sealed class VueDraggableList<TItem> : ComponentBase, IVueComponent
{
    /// <summary>
    /// 与容器绑定的列表；排序变化写回该引用。
    /// The list bound to the container; sort changes write back to this ref.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public IVueRef<TItem[]>? ModelValue { get; set; }

    /// <summary>
    /// 容器的根标签名。
    /// The root tag name of the container.
    /// </summary>
    [Parameter]
    public string? Tag { get; set; }

    /// <summary>
    /// 拖拽选项。
    /// The drag options.
    /// </summary>
    [Parameter]
    public VueDraggableOptions? Options { get; set; }

    /// <summary>
    /// 传给 VueDraggable 的其他宿主属性；键使用 Vue/HTML 实际属性名。
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 列表项内容；每项对应列表中的一个元素。
    /// The list-item content; each entry renders one element of the list.
    /// </summary>
    [Parameter]
    [ECMAScriptName("default")]
    public RenderFragment<TItem>? ChildContent { get; set; }
}
