namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify VItemGroup 默认插槽上下文。
/// Default slot context exposed by Vuetify VItemGroup.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VItemGroupDefaultSlotContext
{
    /// <summary>
    /// 检查指定条目是否已选中的回调。
    /// </summary>
    [Description("@#isSelected")]
    public VuetifyGroupIsSelectedCallback? IsSelected { get; init; }

    /// <summary>
    /// 根据传入的目标和布尔值更新选择状态。
    /// </summary>
    [Description("@#select")]
    public VuetifyGroupSelectCallback? Select { get; init; }

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
    /// 当前已选中条目的模型值或 id 集合。
    /// </summary>
    [Description("@#selected")]
    public VuetifyGroupModelValue[]? Selected { get; init; }
}

/// <summary>
/// 用于 VItemGroupDefaultSlotContext.IsSelected 的回调签名。
/// 检查指定条目是否已选中的回调。
/// </summary>
public delegate bool VuetifyGroupIsSelectedCallback(VuetifyGroupModelValue id);

/// <summary>
/// 用于 VItemGroupDefaultSlotContext.Select 的回调签名。
/// 根据传入的目标和布尔值更新选择状态。
/// </summary>
/// <param name="value">要传入的值，保持其声明的类型和数据。</param>
/// <param name="id">由分组组件注册的项目标识，用于定位要更新选中状态的项目。</param>
public delegate void VuetifyGroupSelectCallback(VuetifyGroupModelValue id, bool value);