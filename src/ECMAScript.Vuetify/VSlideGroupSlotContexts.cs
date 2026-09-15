namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify VSlideGroup 默认、上一步和下一步插槽所暴露的作用域插槽上下文。
/// Scoped slot context exposed by Vuetify VSlideGroup default, prev, and next slots.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VSlideGroupSlotContext
{
    /// <summary>
    /// 移动到组中的下一个可选项。
    /// </summary>
    [Description("@#next")]
    public Action? Next { get; init; }

    /// <summary>
    /// 移动到组中的上一个可选项。
    /// </summary>
    [Description("@#prev")]
    public Action? Prev { get; init; }

    /// <summary>
    /// 根据传入的目标和布尔值更新选择状态。
    /// </summary>
    [Description("@#select")]
    public VuetifySlideGroupSelectCallback? Select { get; init; }

    /// <summary>
    /// 检查指定条目是否已选中的回调。
    /// </summary>
    [Description("@#isSelected")]
    public VuetifySlideGroupIsSelectedCallback? IsSelected { get; init; }
}

/// <summary>
/// 用于 VSlideGroupSlotContext.Select 的回调签名。
/// 根据传入的目标和布尔值更新选择状态。
/// </summary>
/// <param name="value">要传入的值，保持其声明的类型和数据。</param>
/// <param name="id">由分组组件注册的项目标识，用于定位要更新选中状态的项目。</param>
public delegate void VuetifySlideGroupSelectCallback(string id, bool value);

/// <summary>
/// 用于 VSlideGroupSlotContext.IsSelected 的回调签名。
/// 检查指定条目是否已选中的回调。
/// </summary>
public delegate bool VuetifySlideGroupIsSelectedCallback(string id);