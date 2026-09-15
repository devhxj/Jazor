namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify VWindow show-arrows 接受的方向模式。
/// Direction mode accepted by Vuetify VWindow show-arrows.
/// </summary>
[String]
public enum VuetifyWindowShowArrowsMode
{
    /// <summary>
    /// 指针悬停触发；上游取值为 “hover”。
    /// </summary>
    [Description("@#hover")]
    Hover
}

/// <summary>
/// Vuetify VWindow show-arrows value, matching <c>boolean | "hover"</c>.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyWindowShowArrowsValue(bool, VuetifyWindowShowArrowsMode)
{
    /// <summary>
    /// 读取当前值的 bool 分支；不属于该分支时返回 null。
    /// </summary>
    public bool? AsBool => Value is bool value ? value : default(bool?);

    /// <summary>
    /// 读取当前值的 VuetifyWindowShowArrowsMode 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyWindowShowArrowsMode? AsMode
        => Value is VuetifyWindowShowArrowsMode value ? value : default(VuetifyWindowShowArrowsMode?);

    /// <summary>
    /// 将 bool 值转换为 VuetifyWindowShowArrowsValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyWindowShowArrowsValue(bool value)
        => new(value);

    /// <summary>
    /// 将 VuetifyWindowShowArrowsMode 值转换为 VuetifyWindowShowArrowsValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyWindowShowArrowsValue(VuetifyWindowShowArrowsMode value)
        => new(value);
}

/// <summary>
/// Vuetify touch directive payload used by VWindow.
/// </summary>
[ECMAScript]
[Description("@#")]
public record VuetifyTouchData
{
    /// <summary>
    /// 触摸开始时的横向坐标。
    /// </summary>
    [Description("@#touchstartX")]
    public Number TouchstartX { get; init; }

    /// <summary>
    /// 触摸开始时的纵向坐标。
    /// </summary>
    [Description("@#touchstartY")]
    public Number TouchstartY { get; init; }

    /// <summary>
    /// 最近一次触摸移动时的横向坐标。
    /// </summary>
    [Description("@#touchmoveX")]
    public Number TouchmoveX { get; init; }

    /// <summary>
    /// 最近一次触摸移动时的纵向坐标。
    /// </summary>
    [Description("@#touchmoveY")]
    public Number TouchmoveY { get; init; }

    /// <summary>
    /// 触摸结束时的横向坐标。
    /// </summary>
    [Description("@#touchendX")]
    public Number TouchendX { get; init; }

    /// <summary>
    /// 触摸结束时的纵向坐标。
    /// </summary>
    [Description("@#touchendY")]
    public Number TouchendY { get; init; }

    /// <summary>
    /// 本次触摸相对于起始位置的横向位移。
    /// </summary>
    [Description("@#offsetX")]
    public Number OffsetX { get; init; }

    /// <summary>
    /// 本次触摸相对于起始位置的纵向位移。
    /// </summary>
    [Description("@#offsetY")]
    public Number OffsetY { get; init; }
}

/// <summary>
/// Vuetify touch event wrapper carrying the original DOM touch event.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VuetifyTouchEventData : VuetifyTouchData
{
    /// <summary>
    /// 触发当前触摸回调的原生 TouchEvent。
    /// </summary>
    [Description("@#originalEvent")]
    public TouchEvent? OriginalEvent { get; init; }
}

/// <summary>
/// 用于 VuetifyTouchHandlers.Start、VuetifyTouchHandlers.End、VuetifyTouchHandlers.Move 的回调签名。
/// 触摸开始时调用的回调。
/// 触摸结束时调用的回调。
/// 触摸移动时调用的回调。
/// </summary>
public delegate void VuetifyTouchEventHandler(VuetifyTouchEventData eventData);

/// <summary>
/// 用于 VuetifyTouchHandlers.Left、VuetifyTouchHandlers.Right、VuetifyTouchHandlers.Up、VuetifyTouchHandlers.Down 的回调签名。
/// 识别出向左滑动时调用的回调。
/// 识别出向右滑动时调用的回调。
/// 识别出向上滑动时调用的回调。
/// 识别出向下滑动时调用的回调。
/// </summary>
public delegate void VuetifyTouchDirectionHandler(VuetifyTouchData touchData);

/// <summary>
/// Strongly typed Vuetify touch handler bag.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VuetifyTouchHandlers : VueProps
{
    /// <summary>
    /// 触摸开始时调用的回调。
    /// </summary>
    [Description("@#start")]
    public VuetifyTouchEventHandler? Start { get; init; }

    /// <summary>
    /// 触摸结束时调用的回调。
    /// </summary>
    [Description("@#end")]
    public VuetifyTouchEventHandler? End { get; init; }

    /// <summary>
    /// 触摸移动时调用的回调。
    /// </summary>
    [Description("@#move")]
    public VuetifyTouchEventHandler? Move { get; init; }

    /// <summary>
    /// 识别出向左滑动时调用的回调。
    /// </summary>
    [Description("@#left")]
    public VuetifyTouchDirectionHandler? Left { get; init; }

    /// <summary>
    /// 识别出向右滑动时调用的回调。
    /// </summary>
    [Description("@#right")]
    public VuetifyTouchDirectionHandler? Right { get; init; }

    /// <summary>
    /// 识别出向上滑动时调用的回调。
    /// </summary>
    [Description("@#up")]
    public VuetifyTouchDirectionHandler? Up { get; init; }

    /// <summary>
    /// 识别出向下滑动时调用的回调。
    /// </summary>
    [Description("@#down")]
    public VuetifyTouchDirectionHandler? Down { get; init; }
}

/// <summary>
/// Vuetify VWindow touch prop value, matching <c>boolean | TouchHandlers</c>.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyTouchValue(bool, VuetifyTouchHandlers)
{
    /// <summary>
    /// 读取当前值的 bool 分支；不属于该分支时返回 null。
    /// </summary>
    public bool? AsBool => Value is bool value ? value : default(bool?);

    /// <summary>
    /// 读取当前值的 VuetifyTouchHandlers 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyTouchHandlers? AsHandlers => Value as VuetifyTouchHandlers;

    /// <summary>
    /// 将 bool 值转换为 VuetifyTouchValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyTouchValue(bool value)
        => new(value);

    /// <summary>
    /// 将 VuetifyTouchHandlers 值转换为 VuetifyTouchValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyTouchValue(VuetifyTouchHandlers value)
        => new(value);
}

/// <summary>
/// Vuetify group item exposed through VWindow group slot context.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VuetifyWindowGroupItem
{
    /// <summary>
    /// 当前条目的标识，用于组件内部关联及状态更新。
    /// </summary>
    [Description("@#id")]
    public string? Id { get; init; }

    /// <summary>
    /// 此条目关联的模型值，用于渲染和更新回调。
    /// </summary>
    [Description("@#value")]
    public VueValue? Value { get; init; }

    /// <summary>
    /// 是否禁止此项的用户交互。
    /// </summary>
    [Description("@#disabled")]
    public bool? Disabled { get; init; }
}

/// <summary>
/// Vuetify group contract exposed by VWindow default/additional slots.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VuetifyWindowGroupProvide
{
    /// <summary>
    /// 根据传入的目标和布尔值更新选择状态。
    /// </summary>
    [Description("@#select")]
    public VuetifyWindowGroupSelectCallback? Select { get; init; }

    /// <summary>
    /// 当前已选中条目的模型值或 id 集合。 通过引用的 Value 读取最新状态。
    /// </summary>
    [Description("@#selected")]
    public IVueRef<string[]>? Selected { get; init; }

    /// <summary>
    /// 检查指定条目是否已选中的回调。
    /// </summary>
    [Description("@#isSelected")]
    public VuetifyWindowGroupIsSelectedCallback? IsSelected { get; init; }

    /// <summary>
    /// 移动到组中的上一个可选项。
    /// </summary>
    [Description("@#prev")]
    public Action? Prev { get; init; }

    /// <summary>
    /// 移动到组中的下一个可选项。
    /// </summary>
    [Description("@#next")]
    public Action? Next { get; init; }

    /// <summary>
    /// 条目被选中时使用的 CSS 类名。 通过引用的 Value 读取最新状态。
    /// </summary>
    [Description("@#selectedClass")]
    public IVueRef<string?>? SelectedClass { get; init; }

    /// <summary>
    /// 当前组件处理后的条目集合，顺序与当前渲染顺序一致。 通过引用的 Value 读取最新状态。
    /// </summary>
    [Description("@#items")]
    public VueComputedRef<VuetifyWindowGroupItem[]>? Items { get; init; }

    /// <summary>
    /// 是否禁止此项的用户交互。 通过引用的 Value 读取最新状态。
    /// </summary>
    [Description("@#disabled")]
    public IVueRef<bool?>? Disabled { get; init; }

    /// <summary>
    /// 根据模型值查询对应条目在组中的索引。
    /// </summary>
    [Description("@#getItemIndex")]
    public VuetifyWindowGroupItemIndexCallback? GetItemIndex { get; init; }
}

/// <summary>
/// 用于 VuetifyWindowGroupProvide.Select 的回调签名。
/// 根据传入的目标和布尔值更新选择状态。
/// </summary>
/// <param name="value">要传入的值，保持其声明的类型和数据。</param>
/// <param name="id">由分组组件注册的项目标识，用于定位要更新选中状态的项目。</param>
public delegate void VuetifyWindowGroupSelectCallback(string id, bool value);

/// <summary>
/// 用于 VuetifyWindowGroupProvide.IsSelected 的回调签名。
/// 检查指定条目是否已选中的回调。
/// </summary>
public delegate bool VuetifyWindowGroupIsSelectedCallback(string id);

/// <summary>
/// 用于 VuetifyWindowGroupProvide.GetItemIndex 的回调签名。
/// 根据模型值查询对应条目在组中的索引。
/// </summary>
/// <param name="value">要传入的值，保持其声明的类型和数据。</param>
public delegate Number VuetifyWindowGroupItemIndexCallback(VueValue? value);

/// <summary>
/// Default and additional slot context exposed by Vuetify VWindow.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VWindowSlotContext
{
    /// <summary>
    /// 窗口组件提供的分组状态与选择、导航操作。
    /// </summary>
    [Description("@#group")]
    public VuetifyWindowGroupProvide? Group { get; init; }
}

/// <summary>
/// Props object for VWindow prev/next arrow slots.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VWindowControlProps
{
    /// <summary>
    /// 当前状态所用的图标，支持 Vuetify 图标别名或相应图标值。
    /// </summary>
    [Description("@#icon")]
    public VuetifyIconValue? Icon { get; init; }

    /// <summary>
    /// 需要转发给渲染目标的 CSS 类名。
    /// </summary>
    [Description("@#class")]
    public string? Class { get; init; }

    /// <summary>
    /// 自定义渲染时应转发的点击处理函数，用于执行组件默认交互。
    /// </summary>
    [Description("@#onClick")]
    public Action? OnClick { get; init; }

    /// <summary>
    /// 用于辅助技术描述当前操作的 aria-label 文本。
    /// </summary>
    [Description("@#aria-label")]
    public string? AriaLabel { get; init; }
}

/// <summary>
/// Prev/next slot context exposed by Vuetify VWindow.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VWindowControlSlotContext
{
    /// <summary>
    /// 供自定义渲染转发给目标元素或组件的属性，包含上游提供的事件和可访问性绑定。
    /// </summary>
    [Description("@#props")]
    public VWindowControlProps? Props { get; init; }
}

/// <summary>
/// Payload emitted by Vuetify window item group:selected.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VuetifyGroupSelectedEvent
{
    /// <summary>
    /// 本次选择或激活操作要求的新状态。
    /// </summary>
    [Description("@#value")]
    public bool Value { get; init; }
}