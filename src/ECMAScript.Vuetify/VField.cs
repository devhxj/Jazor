using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 字段创作代理，用于组合自定义输入外观。
/// Vuetify field authoring proxy for composing custom input chrome.
/// </summary>
[VueLibraryComponent("vuetify/components", "VField")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(ModelValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
[VueLibraryEmit(nameof(FocusedChanged), VueEmitKind.ModelUpdate, Name = "update:focused")]
[VueLibraryEmit(nameof(ClearClick), VueEmitKind.LibrarySpecific, Name = "click:clear")]
[VueLibraryEmit(nameof(AppendInnerClick), VueEmitKind.LibrarySpecific, Name = "click:appendInner")]
[VueLibraryEmit(nameof(PrependInnerClick), VueEmitKind.LibrarySpecific, Name = "click:prependInner")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
[VueLibrarySlot(nameof(PrependInner), Name = "prepend-inner")]
[VueLibrarySlot(nameof(AppendInner), Name = "append-inner")]
[VueLibrarySlot(nameof(Clear), Name = "clear")]
[VueLibrarySlot(nameof(LabelContent), Name = "label")]
[VueLibrarySlot(nameof(Loader), Name = "loader")]
public sealed class VField : ComponentBase, IVueLibraryComponent
{
    /// <summary>
    /// 字段的 HTML id 属性。
    /// HTML id attribute of the field.
    /// </summary>
    [Parameter]
    public string? Id { get; set; }

    /// <summary>
    /// 组件使用的主题名称。
    /// Theme name applied to the component.
    /// </summary>
    [Parameter]
    public string? Theme { get; set; }

    /// <summary>
    /// 字段的圆角样式。
    /// Border radius style of the field.
    /// </summary>
    [Parameter]
    public VuetifyRoundedValue? Rounded { get; set; }

    /// <summary>
    /// 是否移除圆角。
    /// Whether to remove border radius.
    /// </summary>
    [Parameter]
    public bool Tile { get; set; }

    /// <summary>
    /// 是否显示加载状态指示器。
    /// Whether to show a loading indicator.
    /// </summary>
    [Parameter]
    public VuetifyBooleanStringValue? Loading { get; set; }

    /// <summary>
    /// 字段内部右侧追加的图标。
    /// Icon appended to the inner right side of the field.
    /// </summary>
    [Parameter]
    public VuetifyIconValue? AppendInnerIcon { get; set; }

    /// <summary>
    /// 字段的背景颜色。
    /// Background color of the field.
    /// </summary>
    [Parameter]
    public string? BgColor { get; set; }

    /// <summary>
    /// 是否显示清除按钮。
    /// Whether to show a clear button.
    /// </summary>
    [Parameter]
    public bool Clearable { get; set; }

    /// <summary>
    /// 清除按钮使用的图标。
    /// Icon used for the clear button.
    /// </summary>
    [Parameter]
    public VuetifyIconValue? ClearIcon { get; set; }

    /// <summary>
    /// 字段是否处于激活状态。
    /// Whether the field is in an active state.
    /// </summary>
    [Parameter]
    public bool Active { get; set; }

    /// <summary>
    /// 是否将前缀/后缀图标垂直居中。
    /// Whether to vertically center affix icons.
    /// </summary>
    [Parameter]
    public bool? CenterAffix { get; set; }

    /// <summary>
    /// 字段的主题颜色。
    /// Theme color of the field.
    /// </summary>
    [Parameter]
    public string? Color { get; set; }

    /// <summary>
    /// 字段的基础颜色。
    /// Base color of the field.
    /// </summary>
    [Parameter]
    public string? BaseColor { get; set; }

    /// <summary>
    /// 字段值是否已被修改。
    /// Whether the field value has been modified.
    /// </summary>
    [Parameter]
    public bool Dirty { get; set; }

    /// <summary>
    /// 是否禁用字段。
    /// Whether to disable the field.
    /// </summary>
    [Parameter]
    public bool? Disabled { get; set; }

    /// <summary>
    /// 是否显示聚焦时的发光效果。
    /// Whether to show a glow effect on focus.
    /// </summary>
    [Parameter]
    public bool Glow { get; set; }

    /// <summary>
    /// 是否将字段置于错误状态。
    /// Whether to put the field in an error state.
    /// </summary>
    [Parameter]
    public bool Error { get; set; }

    /// <summary>
    /// 是否移除字段的阴影边框。
    /// Whether to remove the field shadow border.
    /// </summary>
    [Parameter]
    public bool Flat { get; set; }

    /// <summary>
    /// 字段图标的颜色。
    /// Color applied to icons within the field.
    /// </summary>
    [Parameter]
    public VuetifyIconColorValue? IconColor { get; set; }

    /// <summary>
    /// 字段的标签文本。
    /// Label text of the field.
    /// </summary>
    [Parameter]
    public string? Label { get; set; }

    /// <summary>
    /// 是否始终显示清除按钮。
    /// Whether to always show the clear button.
    /// </summary>
    [Parameter]
    public bool PersistentClear { get; set; }

    /// <summary>
    /// 字段内部左侧前置的图标。
    /// Icon prepended to the inner left side of the field.
    /// </summary>
    [Parameter]
    public VuetifyIconValue? PrependInnerIcon { get; set; }

    /// <summary>
    /// 是否反转字段的输入方向。
    /// Whether to reverse the field input direction.
    /// </summary>
    [Parameter]
    public bool Reverse { get; set; }

    /// <summary>
    /// 是否使用单行模式，标签不浮动。
    /// Whether to use single-line mode where the label does not float.
    /// </summary>
    [Parameter]
    public bool SingleLine { get; set; }

    /// <summary>
    /// 字段的视觉变体样式。
    /// Visual variant style of the field.
    /// </summary>
    [Parameter]
    public VuetifyFieldVariant? Variant { get; set; }

    /// <summary>
    /// 字段是否处于聚焦状态。
    /// Whether the field is focused.
    /// </summary>
    [Parameter]
    public bool Focused { get; set; }

    /// <summary>
    /// 当 Focused 状态变化时触发的事件回调。
    /// Event callback fired when the Focused state changes.
    /// </summary>
    [Parameter]
    public EventCallback<bool> FocusedChanged { get; set; }

    /// <summary>
    /// 字段的绑定模型值。
    /// Bound model value of the field.
    /// </summary>
    [Parameter]
    public VueValue? ModelValue { get; set; }

    /// <summary>
    /// 当 ModelValue 变化时触发的事件回调。
    /// Event callback fired when ModelValue changes.
    /// </summary>
    [Parameter]
    public EventCallback<VueValue?> ModelValueChanged { get; set; }

    /// <summary>
    /// 点击清除按钮时触发的事件回调。
    /// Event callback fired when the clear icon is clicked.
    /// </summary>
    [Parameter]
    public EventCallback<MouseEvent> ClearClick { get; set; }

    /// <summary>
    /// 点击内部右侧追加图标时触发的事件回调。
    /// Event callback fired when the append-inner icon is clicked.
    /// </summary>
    [Parameter]
    public EventCallback<MouseEvent> AppendInnerClick { get; set; }

    /// <summary>
    /// 点击内部左侧前置图标时触发的事件回调。
    /// Event callback fired when the prepend-inner icon is clicked.
    /// </summary>
    [Parameter]
    public EventCallback<MouseEvent> PrependInnerClick { get; set; }

    /// <summary>
    /// 附加到根元素的自定义属性。
    /// Additional custom attributes applied to the root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 字段的默认子内容插槽。
    /// Default child content slot of the field.
    /// </summary>
    [Parameter]
    public RenderFragment<VFieldSlotContext>? ChildContent { get; set; }

    /// <summary>
    /// 字段内部左侧前置内容的自定义插槽。
    /// Custom slot for content prepended to the inner left side.
    /// </summary>
    [Parameter]
    public RenderFragment<VFieldSlotContext>? PrependInner { get; set; }

    /// <summary>
    /// 字段内部右侧追加内容的自定义插槽。
    /// Custom slot for content appended to the inner right side.
    /// </summary>
    [Parameter]
    public RenderFragment<VFieldSlotContext>? AppendInner { get; set; }

    /// <summary>
    /// 清除按钮的自定义内容插槽。
    /// Custom content slot for the clear button.
    /// </summary>
    [Parameter]
    public RenderFragment<VFieldSlotContext>? Clear { get; set; }

    /// <summary>
    /// 标签的自定义内容插槽。
    /// Custom content slot for the field label.
    /// </summary>
    [Parameter]
    public RenderFragment<VFieldLabelSlotContext>? LabelContent { get; set; }

    /// <summary>
    /// 加载指示器的自定义内容插槽。
    /// Custom content slot for the loader indicator.
    /// </summary>
    [Parameter]
    public RenderFragment<VuetifyLoaderSlotContext>? Loader { get; set; }
}
