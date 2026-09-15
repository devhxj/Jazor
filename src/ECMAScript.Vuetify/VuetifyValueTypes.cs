using System.Collections;
using System.Runtime.CompilerServices;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 警告框类型。
/// Vuetify alert type.
/// </summary>
[String]
public enum VuetifyAlertType
{
    /// <summary>
    /// 成功状态样式；上游取值为 “success”。
    /// </summary>
    [Description("@#success")]
    Success,

    /// <summary>
    /// 信息提示样式；上游取值为 “info”。
    /// </summary>
    [Description("@#info")]
    Info,

    /// <summary>
    /// 警告状态样式；上游取值为 “warning”。
    /// </summary>
    [Description("@#warning")]
    Warning,

    /// <summary>
    /// 错误状态样式；上游取值为 “error”。
    /// </summary>
    [Description("@#error")]
    Error
}

/// <summary>
/// Alert 彩色边框所在的逻辑边缘。
/// </summary>
[String]
public enum VuetifyAlertBorderSide
{
    /// <summary>
    /// 顶部；上游取值为 “top”。
    /// </summary>
    [Description("@#top")]
    Top,

    /// <summary>
    /// 逻辑结束端，方向随 RTL 布局变化；上游取值为 “end”。
    /// </summary>
    [Description("@#end")]
    End,

    /// <summary>
    /// 底部；上游取值为 “bottom”。
    /// </summary>
    [Description("@#bottom")]
    Bottom,

    /// <summary>
    /// 逻辑起始端，方向随 RTL 布局变化；上游取值为 “start”。
    /// </summary>
    [Description("@#start")]
    Start
}

/// <summary>
/// 用于 VAlert.Border 的参数类型。
/// Adds a colored border to the component.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyAlertBorderValue(bool, VuetifyAlertBorderSide)
{
    /// <summary>
    /// 读取当前值的 bool 分支；不属于该分支时返回 null。
    /// </summary>
    public bool? AsBool
        => Value is bool value ? value : default(bool?);

    /// <summary>
    /// 读取当前值的 VuetifyAlertBorderSide 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyAlertBorderSide? AsSide
        => Value is VuetifyAlertBorderSide value ? value : default(VuetifyAlertBorderSide?);

    /// <summary>
    /// 将 bool 值转换为 VuetifyAlertBorderValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyAlertBorderValue(bool value)
        => new(value);

    /// <summary>
    /// 将 VuetifyAlertBorderSide 值转换为 VuetifyAlertBorderValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyAlertBorderValue(VuetifyAlertBorderSide value)
        => new(value);
}


/// <summary>
/// 用于 VAlert.Icon 的参数类型。
/// Apply a specific icon using the [v-icon](/components/icons/) component.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyAlertIconValue(
    string,
    Symbol,
    VueProps)
{
    /// <summary>
    /// 读取当前值的 string 分支；不属于该分支时返回 null。
    /// </summary>
    public string? AsString
        => Value is string value ? value : default(string?);

    /// <summary>
    /// 读取当前值的 Symbol 分支；不属于该分支时返回 null。
    /// </summary>
    public Symbol? AsSymbol
        => Value is Symbol value ? value : default(Symbol?);

    /// <summary>
    /// 读取当前值的 VueProps 分支；不属于该分支时返回 null。
    /// </summary>
    public VueProps? AsProps
        => Value is VueProps value ? value : default(VueProps?);

    /// <summary>
    /// 显式关闭 Alert 图标，传给组件的值为 false。
    /// </summary>
    [ECMAScriptInline("false")]
    public extern static VuetifyAlertIconValue None();

    /// <summary>
    /// 将 string 值转换为 VuetifyAlertIconValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyAlertIconValue(string value)
        => new(value);

    /// <summary>
    /// 将 Symbol 值转换为 VuetifyAlertIconValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyAlertIconValue(Symbol value)
        => new(value);

    /// <summary>
    /// 将 VueProps 值转换为 VuetifyAlertIconValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyAlertIconValue(VueProps value)
        => new(value);

    /// <summary>
    /// 将 VueDictionary 值转换为 VuetifyAlertIconValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyAlertIconValue(VueDictionary value)
        => new(value);
}


/// <summary>
/// 用于 VAlert.Density、VAppBar.Density、VAvatar.Density、VBanner.Density、VBottomNavigation.Density、VBtn.Density、VBtnGroup.Density、VBtnToggle.Density、VCard.Density、VChip.Density、VDataTable.Density、VFab.Density、VFileInput.Density、VFileUpload.Density、VInput.Density、VInputComponentBase.Density、VList.Density、VListItem.Density、VNumberInput.Density、VOtpInput.Density、VRadio.Density、VRadioGroup.Density、VRangeSlider.Density、VRating.Density、VSelectionControl.Density、VSelectionControlComponentBase.Density、VSelectionControlGroup.Density、VSelectLikeComponentBase.Density、VSlider.Density、VTable.Density、VTimeline.Density、VTreeview.Density 的可选取值。
/// Adjusts the vertical height used by the component.
/// Adjusts the vertical height of the table rows.
/// 输入框的紧凑程度。
/// Density/compactness of the input field.
/// 组件的紧凑程度。
/// The density/compactness of the component.
/// </summary>
[String]
public enum VuetifyDensity
{
    /// <summary>
    /// 默认配置；上游取值为 “default”。
    /// </summary>
    [Description("@#default")]
    Default,

    /// <summary>
    /// 舒适的间距密度；上游取值为 “comfortable”。
    /// </summary>
    [Description("@#comfortable")]
    Comfortable,

    /// <summary>
    /// 紧凑的间距密度；上游取值为 “compact”。
    /// </summary>
    [Description("@#compact")]
    Compact
}

/// <summary>
/// 用于 VAlert.Variant、VAvatar.Variant、VBtn.Variant、VBtnGroup.Variant、VBtnToggle.Variant、VCard.Variant、VChip.Variant、VChipGroup.Variant、VFab.Variant、VIconBtn.Variant、VIconBtn.ActiveVariant、VIconBtn.BaseVariant、VList.Variant、VListItem.Variant、VSnackbar.Variant、VSnackbarQueue.Variant、VToolbarItems.Variant、VTreeview.Variant 的可选取值。
/// Applies a distinct style to the component.
/// When active is a boolean, this variant is used when active is true.
/// When active is a boolean, this variant is used when active is false.
/// </summary>
[String]
public enum VuetifyVariant
{
    /// <summary>
    /// 带阴影的凸起样式；上游取值为 “elevated”。
    /// </summary>
    [Description("@#elevated")]
    Elevated,

    /// <summary>
    /// 平面样式；上游取值为 “flat”。
    /// </summary>
    [Description("@#flat")]
    Flat,

    /// <summary>
    /// 轮廓边框样式；上游取值为 “outlined”。
    /// </summary>
    [Description("@#outlined")]
    Outlined,

    /// <summary>
    /// 文本样式；上游取值为 “text”。
    /// </summary>
    [Description("@#text")]
    Text,

    /// <summary>
    /// 带色调背景的样式；上游取值为 “tonal”。
    /// </summary>
    [Description("@#tonal")]
    Tonal,

    /// <summary>
    /// 无强调装饰的样式；上游取值为 “plain”。
    /// </summary>
    [Description("@#plain")]
    Plain
}

/// <summary>
/// 用于 VField.Variant、VFileInput.Variant、VInputComponentBase.Variant、VNumberInput.Variant、VOtpInput.Variant、VSelectLikeComponentBase.Variant 的可选取值。
/// Applies a distinct style to the component.
/// 输入框的外观变体。
/// Visual variant of the input field.
/// 输入框的外观变体。
/// The visual variant of the input field.
/// </summary>
[String]
public enum VuetifyFieldVariant
{
    /// <summary>
    /// 下划线样式；上游取值为 “underlined”。
    /// </summary>
    [Description("@#underlined")]
    Underlined,

    /// <summary>
    /// 轮廓边框样式；上游取值为 “outlined”。
    /// </summary>
    [Description("@#outlined")]
    Outlined,

    /// <summary>
    /// 背景填充样式；上游取值为 “filled”。
    /// </summary>
    [Description("@#filled")]
    Filled,

    /// <summary>
    /// 独立输入框样式；上游取值为 “solo”。
    /// </summary>
    [Description("@#solo")]
    Solo,

    /// <summary>
    /// 反色独立输入框样式；上游取值为 “solo-inverted”。
    /// </summary>
    [Description("@#solo-inverted")]
    SoloInverted,

    /// <summary>
    /// 带填充的独立输入框样式；上游取值为 “solo-filled”。
    /// </summary>
    [Description("@#solo-filled")]
    SoloFilled,

    /// <summary>
    /// 无强调装饰的样式；上游取值为 “plain”。
    /// </summary>
    [Description("@#plain")]
    Plain
}

/// <summary>
/// 用于 VOtpInput.Type 的可选取值。
/// Supported types: `text`, `password`, `number`.
/// </summary>
[String]
public enum VuetifyInputType
{
    /// <summary>
    /// 原生颜色输入；上游取值为 “color”。
    /// </summary>
    [Description("@#color")]
    Color,

    /// <summary>
    /// 日期选择；上游取值为 “date”。
    /// </summary>
    [Description("@#date")]
    Date,

    /// <summary>
    /// 不带时区的本地日期时间输入；上游取值为 “datetime-local”。
    /// </summary>
    [Description("@#datetime-local")]
    DatetimeLocal,

    /// <summary>
    /// 电子邮件地址输入；上游取值为 “email”。
    /// </summary>
    [Description("@#email")]
    Email,

    /// <summary>
    /// 按月显示；上游取值为 “month”。
    /// </summary>
    [Description("@#month")]
    Month,

    /// <summary>
    /// 按数值处理；上游取值为 “number”。
    /// </summary>
    [Description("@#number")]
    Number,

    /// <summary>
    /// 隐藏字符的密码输入；上游取值为 “password”。
    /// </summary>
    [Description("@#password")]
    Password,

    /// <summary>
    /// 搜索文本输入；上游取值为 “search”。
    /// </summary>
    [Description("@#search")]
    Search,

    /// <summary>
    /// 电话号码输入；上游取值为 “tel”。
    /// </summary>
    [Description("@#tel")]
    Tel,

    /// <summary>
    /// 文本样式；上游取值为 “text”。
    /// </summary>
    [Description("@#text")]
    Text,

    /// <summary>
    /// 时间选择；上游取值为 “time”。
    /// </summary>
    [Description("@#time")]
    Time,

    /// <summary>
    /// URL 输入；上游取值为 “url”。
    /// </summary>
    [Description("@#url")]
    Url,

    /// <summary>
    /// 按周显示；上游取值为 “week”。
    /// </summary>
    [Description("@#week")]
    Week
}

/// <summary>
/// 自动选择第一个候选项的匹配策略。
/// </summary>
[String]
public enum VuetifyAutoSelectFirstMode
{
    /// <summary>
    /// 只有输入值与候选项精确匹配时自动选中首项；上游取值为 “exact”。
    /// </summary>
    [Description("@#exact")]
    Exact
}

/// <summary>
/// 用于 VNumberInput.ControlVariant 的可选取值。
/// Controls layout of the stepper buttons.
/// </summary>
[String]
public enum VuetifyNumberInputControlVariant
{
    /// <summary>
    /// 默认配置；上游取值为 “default”。
    /// </summary>
    [Description("@#default")]
    Default,

    /// <summary>
    /// 增减按钮上下堆叠；上游取值为 “stacked”。
    /// </summary>
    [Description("@#stacked")]
    Stacked,

    /// <summary>
    /// 增减按钮分列输入框两侧；上游取值为 “split”。
    /// </summary>
    [Description("@#split")]
    Split,

    /// <summary>
    /// 隐藏；上游取值为 “hidden”。
    /// </summary>
    [Description("@#hidden")]
    Hidden
}

/// <summary>
/// 格式化文件大小时采用的单位换算基数。
/// </summary>
public enum VuetifyFileSizeBase
{
    /// <summary>
    /// 按 1000 换算文件大小单位。
    /// </summary>
    Decimal = 1000,
    /// <summary>
    /// 按 1024 换算文件大小单位。
    /// </summary>
    Binary = 1024
}

/// <summary>
/// 控制是否始终显示对应辅助区域的字符串取值。
/// </summary>
[String]
public enum VuetifyAlwaysMode
{
    /// <summary>
    /// 始终启用或显示；上游取值为 “always”。
    /// </summary>
    [Description("@#always")]
    Always
}

/// <summary>
/// 用于 VRangeSlider.Direction、VSlider.Direction 的可选取值。
/// Changes the direction of the input.
/// </summary>
[String]
public enum VuetifySliderDirection
{
    /// <summary>
    /// 水平方向；上游取值为 “horizontal”。
    /// </summary>
    [Description("@#horizontal")]
    Horizontal,

    /// <summary>
    /// 垂直方向；上游取值为 “vertical”。
    /// </summary>
    [Description("@#vertical")]
    Vertical
}

/// <summary>
/// 用于 VBottomNavigation.Mode 的可选取值。
/// Changes the orientation and active state styling of the component.
/// </summary>
[String]
public enum VuetifyBottomNavigationMode
{
    /// <summary>
    /// 水平方向；上游取值为 “horizontal”。
    /// </summary>
    [Description("@#horizontal")]
    Horizontal,

    /// <summary>
    /// 选中导航项时突出显示其标签；上游取值为 “shift”。
    /// </summary>
    [Description("@#shift")]
    Shift
}

/// <summary>
/// 用于 VBottomSheet.ScrollStrategy、VDialog.ScrollStrategy、VMenu.ScrollStrategy、VOverlay.ScrollStrategy、VSpeedDial.ScrollStrategy 的可选取值。
/// Strategy used when the component is activate and user scrolls.
/// </summary>
[String]
public enum VuetifyScrollStrategy
{
    /// <summary>
    /// 阻止滚动；上游取值为 “block”。
    /// </summary>
    [Description("@#block")]
    Block,

    /// <summary>
    /// 关闭；上游取值为 “close”。
    /// </summary>
    [Description("@#close")]
    Close,

    /// <summary>
    /// 不启用当前选项；上游取值为 “none”。
    /// </summary>
    [Description("@#none")]
    None,

    /// <summary>
    /// 重新计算浮层位置；上游取值为 “reposition”。
    /// </summary>
    [Description("@#reposition")]
    Reposition
}

/// <summary>
/// 用于 VDialog.LocationStrategy、VOverlay.LocationStrategy、VSnackbar.LocationStrategy、VSnackbarQueue.LocationStrategy、VSpeedDial.LocationStrategy 的可选取值。
/// Sets how the overlay content is positioned. Defaults to `static`, which centers content in its container. Use `connected` to attach to an activator element, or `viewport` to position relative to the browser viewport.
/// </summary>
[String]
public enum VuetifyLocationStrategy
{
    /// <summary>
    /// 使用静态定位；上游取值为 “static”。
    /// </summary>
    [Description("@#static")]
    Static,

    /// <summary>
    /// 将浮层位置连接到指定目标并处理边界；上游取值为 “connected”。
    /// </summary>
    [Description("@#connected")]
    Connected
}

/// <summary>
/// C# 联合参数，允许 VuetifyStyleValue[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取相应分支。
/// </summary>
[ECMAScript]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyStyleValuesCollectionBuilder), nameof(VuetifyStyleValuesCollectionBuilder.Create))]
public readonly union VuetifyStyleValues(VuetifyStyleValue[]) : IEnumerable<VuetifyStyleValue>
{
    /// <summary>
    /// 读取当前值的 VuetifyStyleValue[] 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyStyleValue[]? AsArray
        => Value is VuetifyStyleValue[] value ? value : default(VuetifyStyleValue[]?);

    /// <summary>
    /// 将 VuetifyStyleValue[] 值转换为 VuetifyStyleValues，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyStyleValues(VuetifyStyleValue[] values)
        => new(values);

    /// <summary>
    /// 将 string[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyStyleValues(string[] values)
        => new(Array.ConvertAll(values, static value => (VuetifyStyleValue)value));

    /// <summary>
    /// 将 VueProps[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyStyleValues(VueProps[] values)
        => new(Array.ConvertAll(values, static value => (VuetifyStyleValue)value));

    /// <summary>
    /// 将 VueDictionary[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyStyleValues(VueDictionary[] values)
        => new(Array.ConvertAll(values, static value => (VuetifyStyleValue)value));

    IEnumerator<VuetifyStyleValue> IEnumerable<VuetifyStyleValue>.GetEnumerator()
        => ((IEnumerable<VuetifyStyleValue>)(AsArray ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VuetifyStyleValue>)this).GetEnumerator();
}


/// <summary>
/// 供 C# 集合表达式调用的构建器；应用可直接使用 [item1, item2] 语法构造对应集合。
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyStyleValuesCollectionBuilder
{
    /// <summary>
    /// 为 C# 集合表达式创建有序集合；复制传入的元素，不保留临时 Span。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    public static VuetifyStyleValues Create(ReadOnlySpan<VuetifyStyleValue> values)
        => values.ToArray();
}

/// <summary>
/// 用于 VAlert.CssStyle、VAvatar.CssStyle、VBadge.CssStyle、VBottomNavigation.CssStyle、VChip.CssStyle、VCol.CssStyle、VContainer.CssStyle、VDialog.CssStyle、VForm.CssStyle、VIcon.CssStyle、VImg.CssStyle、VProgressCircular.CssStyle、VProgressLinear.CssStyle、VRow.CssStyle、VSheet.CssStyle、VSnackbar.CssStyle、VSpacer.CssStyle、VStepperVertical.CssStyle、VToolbar.CssStyle、VToolbarItems.CssStyle、VToolbarTitle.CssStyle、VTreeview.CssStyle 的参数类型。
/// 自定义行内样式。
/// Custom inline style(s).
/// 应用的内联样式。
/// The inline style to apply.
/// 组件的内联样式。
/// Inline style for the component.
/// 应用于表单根元素的行内样式。
/// Inline styles applied to the form root element.
/// 应用于图片根元素的行内样式。
/// Inline styles applied to the image root element.
/// 应用于根元素的内联样式。
/// Inline styles applied to the root element.
/// 内联样式。
/// Inline styles.
/// 应用于组件的内联样式。
/// Inline styles applied to the component.
/// 行内样式。
/// Inline style.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyStyleValue(
    string,
    VueProps,
    VuetifyStyleValues)
{
    /// <summary>
    /// 读取当前值的 string 分支；不属于该分支时返回 null。
    /// </summary>
    public string? AsString
        => Value is string value ? value : default(string?);

    /// <summary>
    /// 读取当前值的 VueProps 分支；不属于该分支时返回 null。
    /// </summary>
    public VueProps? AsProps
        => Value is VueProps value ? value : default(VueProps?);

    /// <summary>
    /// 读取当前值的 VuetifyStyleValues 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyStyleValues? AsValues
        => Value is VuetifyStyleValues value ? value : default(VuetifyStyleValues?);

    /// <summary>
    /// 将 string 值转换为 VuetifyStyleValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyStyleValue(string value)
        => new(value);

    /// <summary>
    /// 将 VueProps 值转换为 VuetifyStyleValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyStyleValue(VueProps value)
        => new(value);

    /// <summary>
    /// 将 VueDictionary 值转换为 VuetifyStyleValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyStyleValue(VueDictionary value)
        => new(value);

    /// <summary>
    /// 将 VuetifyStyleValues 值转换为 VuetifyStyleValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyStyleValue(VuetifyStyleValues value)
        => new(value);

    /// <summary>
    /// 将 VuetifyStyleValue[] 值转换为 VuetifyStyleValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyStyleValue(VuetifyStyleValue[] value)
        => new((VuetifyStyleValues)value);

    /// <summary>
    /// 将 string[] 值转换为 VuetifyStyleValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyStyleValue(string[] value)
        => new((VuetifyStyleValues)value);

    /// <summary>
    /// 将 VueProps[] 值转换为 VuetifyStyleValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyStyleValue(VueProps[] value)
        => new((VuetifyStyleValues)value);

    /// <summary>
    /// 将 VueDictionary[] 值转换为 VuetifyStyleValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyStyleValue(VueDictionary[] value)
        => new((VuetifyStyleValues)value);
}


/// <summary>
/// 用于 VDialog.Attach、VOverlay.Attach、VSnackbar.Attach、VSnackbarQueue.Attach、VSpeedDial.Attach 的参数类型。
/// Specifies which DOM element the overlay content should teleport to. Can be a direct element reference, querySelector string, or `true` to disable teleporting. Uses `body` by default.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyAttachTarget(
    bool,
    string,
    Element)
{
    /// <summary>
    /// 读取当前值的 bool 分支；不属于该分支时返回 null。
    /// </summary>
    public bool? AsBool
        => Value is bool value ? value : default(bool?);

    /// <summary>
    /// 读取当前值的 string 分支；不属于该分支时返回 null。
    /// </summary>
    public string? AsSelector
        => Value is string value ? value : default(string?);

    /// <summary>
    /// 读取当前值的 Element 分支；不属于该分支时返回 null。
    /// </summary>
    public Element? AsElement
        => Value is Element value ? value : default(Element?);

    /// <summary>
    /// 将 bool 值转换为 VuetifyAttachTarget，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyAttachTarget(bool value)
        => new(value);

    /// <summary>
    /// 将 string 值转换为 VuetifyAttachTarget，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyAttachTarget(string value)
        => new(value);

    /// <summary>
    /// 将 Element 值转换为 VuetifyAttachTarget，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyAttachTarget(Element value)
        => new(value);
}


/// <summary>
/// 用于 VAlert.Location、VBadge.Location、VBanner.Location、VBtn.Location、VColorPicker.Location、VDateInput.Location、VDatePicker.Location、VDialog.Location、VFab.Location、VFileUpload.Location、VMenu.Location、VMenu.Origin、VOverlay.Location、VOverlay.Origin、VPicker.Location、VProgressLinear.Location、VSheet.Location、VSnackbar.Location、VSnackbarQueue.Location、VSpeedDial.Location、VStepper.Location、VTimePicker.Location、VTooltip.Location、VTooltip.Origin 的可选取值。
/// Specifies the component's location. Can combine by using a space separated string.
/// Specifies the date picker's location. Can combine by using a space separated string.
/// Specifies the anchor point for positioning the component, using directional cues to align it either horizontally, vertically, or both..
/// The location of the fab relative to the layout. Only works when using **app**.
/// 组件的对齐位置。
/// Location alignment of the component.
/// Sets the anchor point on the overlay content that aligns to the `location` anchor on the target. `auto` uses the opposing side of `location`; `overlap` uses the same anchor, causing the overlay to cover the target. Also sets the CSS `transform-origin` for enter/leave transitions.
/// </summary>
[String]
public enum VuetifyLocation
{
    /// <summary>
    /// 顶部；上游取值为 “top”。
    /// </summary>
    [Description("@#top")]
    Top,

    /// <summary>
    /// 底部；上游取值为 “bottom”。
    /// </summary>
    [Description("@#bottom")]
    Bottom,

    /// <summary>
    /// 逻辑起始端，方向随 RTL 布局变化；上游取值为 “start”。
    /// </summary>
    [Description("@#start")]
    Start,

    /// <summary>
    /// 逻辑结束端，方向随 RTL 布局变化；上游取值为 “end”。
    /// </summary>
    [Description("@#end")]
    End,

    /// <summary>
    /// 左侧；上游取值为 “left”。
    /// </summary>
    [Description("@#left")]
    Left,

    /// <summary>
    /// 右侧；上游取值为 “right”。
    /// </summary>
    [Description("@#right")]
    Right,

    /// <summary>
    /// 居中；上游取值为 “center”。
    /// </summary>
    [Description("@#center")]
    Center,

    /// <summary>
    /// 水平和垂直均居中；上游取值为 “center center”。
    /// </summary>
    [Description("@#center center")]
    CenterCenter,

    /// <summary>
    /// 上方并对齐逻辑起始端；上游取值为 “top start”。
    /// </summary>
    [Description("@#top start")]
    TopStart,

    /// <summary>
    /// 上方居中；上游取值为 “top center”。
    /// </summary>
    [Description("@#top center")]
    TopCenter,

    /// <summary>
    /// 上方并对齐逻辑结束端；上游取值为 “top end”。
    /// </summary>
    [Description("@#top end")]
    TopEnd,

    /// <summary>
    /// 下方并对齐逻辑起始端；上游取值为 “bottom start”。
    /// </summary>
    [Description("@#bottom start")]
    BottomStart,

    /// <summary>
    /// 下方居中；上游取值为 “bottom center”。
    /// </summary>
    [Description("@#bottom center")]
    BottomCenter,

    /// <summary>
    /// 下方并对齐逻辑结束端；上游取值为 “bottom end”。
    /// </summary>
    [Description("@#bottom end")]
    BottomEnd,

    /// <summary>
    /// 逻辑起始侧并顶部对齐；上游取值为 “start top”。
    /// </summary>
    [Description("@#start top")]
    StartTop,

    /// <summary>
    /// 逻辑起始侧并居中；上游取值为 “start center”。
    /// </summary>
    [Description("@#start center")]
    StartCenter,

    /// <summary>
    /// 逻辑起始侧并底部对齐；上游取值为 “start bottom”。
    /// </summary>
    [Description("@#start bottom")]
    StartBottom,

    /// <summary>
    /// 逻辑结束侧并顶部对齐；上游取值为 “end top”。
    /// </summary>
    [Description("@#end top")]
    EndTop,

    /// <summary>
    /// 逻辑结束侧并居中；上游取值为 “end center”。
    /// </summary>
    [Description("@#end center")]
    EndCenter,

    /// <summary>
    /// 逻辑结束侧并底部对齐；上游取值为 “end bottom”。
    /// </summary>
    [Description("@#end bottom")]
    EndBottom,

    /// <summary>
    /// 左侧顶部对齐；上游取值为 “left top”。
    /// </summary>
    [Description("@#left top")]
    LeftTop,

    /// <summary>
    /// 左侧居中；上游取值为 “left center”。
    /// </summary>
    [Description("@#left center")]
    LeftCenter,

    /// <summary>
    /// 左侧底部对齐；上游取值为 “left bottom”。
    /// </summary>
    [Description("@#left bottom")]
    LeftBottom,

    /// <summary>
    /// 右侧顶部对齐；上游取值为 “right top”。
    /// </summary>
    [Description("@#right top")]
    RightTop,

    /// <summary>
    /// 右侧居中；上游取值为 “right center”。
    /// </summary>
    [Description("@#right center")]
    RightCenter,

    /// <summary>
    /// 右侧底部对齐；上游取值为 “right bottom”。
    /// </summary>
    [Description("@#right bottom")]
    RightBottom
}

/// <summary>
/// 用于 VAppBar.Location 的可选取值。
/// Aligns the component towards the top or bottom.
/// </summary>
[String]
public enum VuetifyAppBarLocation
{
    /// <summary>
    /// 顶部；上游取值为 “top”。
    /// </summary>
    [Description("@#top")]
    Top,

    /// <summary>
    /// 底部；上游取值为 “bottom”。
    /// </summary>
    [Description("@#bottom")]
    Bottom
}

/// <summary>
/// 用于 VBanner.Lines 的可选取值。
/// The amount of visible lines of text before it truncates.
/// </summary>
[String]
public enum VuetifyListLineMode
{
    /// <summary>
    /// 单行内容布局；上游取值为 “one”。
    /// </summary>
    [Description("@#one")]
    One,

    /// <summary>
    /// 双行内容布局；上游取值为 “two”。
    /// </summary>
    [Description("@#two")]
    Two,

    /// <summary>
    /// 三行内容布局；上游取值为 “three”。
    /// </summary>
    [Description("@#three")]
    Three
}

/// <summary>
/// 用于 VList.Lines、VListItem.Lines、VTreeview.Lines 的参数类型。
/// Designates a **minimum-height** for all children `v-list-item` components. This prop uses [line-clamp](https://developer.mozilla.org/en-US/docs/Web/CSS/-webkit-line-clamp) and is not supported in all browsers.
/// The line declaration specifies the minimum height of the item and can also be controlled from v-list with the same prop.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyListLines(bool, VuetifyListLineMode)
{
    /// <summary>
    /// 读取当前值的 bool 分支；不属于该分支时返回 null。
    /// </summary>
    public bool? AsBool
        => Value is bool value ? value : default(bool?);

    /// <summary>
    /// 读取当前值的 VuetifyListLineMode 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyListLineMode? AsMode
        => Value is VuetifyListLineMode value ? value : default(VuetifyListLineMode?);

    /// <summary>
    /// 将 bool 值转换为 VuetifyListLines，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyListLines(bool value)
        => new(value);

    /// <summary>
    /// 将 VuetifyListLineMode 值转换为 VuetifyListLines，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyListLines(VuetifyListLineMode value)
        => new(value);
}


/// <summary>
/// 用于 VBtn.Ripple、VChip.Ripple、VFab.Ripple、VListItem.Ripple、VRating.Ripple、VSelectionControl.Ripple、VSelectionControlGroup.Ripple、VStepperVertical.Ripple 的参数类型。
/// Applies the [v-ripple](/directives/ripple) directive.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyRippleValue(bool, VueProps)
{
    /// <summary>
    /// 读取当前值的 bool 分支；不属于该分支时返回 null。
    /// </summary>
    public bool? AsBool
        => Value is bool value ? value : default(bool?);

    /// <summary>
    /// 读取当前值的 VueProps 分支；不属于该分支时返回 null。
    /// </summary>
    public VueProps? AsOptions
        => Value is VueProps value ? value : default(VueProps?);

    /// <summary>
    /// 将 bool 值转换为 VuetifyRippleValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyRippleValue(bool value)
        => new(value);

    /// <summary>
    /// 将 VueProps 值转换为 VuetifyRippleValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyRippleValue(VueProps value)
        => new(value);
}


/// <summary>
/// 用于 VNavigationDrawer.Location 的可选取值。
/// Controls the edge of the screen the drawer is attached to.
/// </summary>
[String]
public enum VuetifyNavigationDrawerLocation
{
    /// <summary>
    /// 逻辑起始端，方向随 RTL 布局变化；上游取值为 “start”。
    /// </summary>
    [Description("@#start")]
    Start,

    /// <summary>
    /// 逻辑结束端，方向随 RTL 布局变化；上游取值为 “end”。
    /// </summary>
    [Description("@#end")]
    End,

    /// <summary>
    /// 左侧；上游取值为 “left”。
    /// </summary>
    [Description("@#left")]
    Left,

    /// <summary>
    /// 右侧；上游取值为 “right”。
    /// </summary>
    [Description("@#right")]
    Right,

    /// <summary>
    /// 顶部；上游取值为 “top”。
    /// </summary>
    [Description("@#top")]
    Top,

    /// <summary>
    /// 底部；上游取值为 “bottom”。
    /// </summary>
    [Description("@#bottom")]
    Bottom
}

/// <summary>
/// 用于 VBottomSheet.Scrim、VDialog.Scrim、VFileUpload.Scrim、VNavigationDrawer.Scrim、VOverlay.Scrim、VSpeedDial.Scrim 的参数类型。
/// Accepts true/false to enable background, and string to define color.
/// Determines whether an overlay is used when hovering over the component with files. Accepts true/false to enable background, and string to define color.
/// Determines whether an overlay is used when a **temporary** drawer is open. Accepts true/false to enable background, and string to define color.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyScrimValue(bool, string)
{
    /// <summary>
    /// 读取当前值的 bool 分支；不属于该分支时返回 null。
    /// </summary>
    public bool? AsBool
        => Value is bool value ? value : default(bool?);

    /// <summary>
    /// 读取当前值的 string 分支；不属于该分支时返回 null。
    /// </summary>
    public string? AsString
        => Value is string value ? value : default(string?);

    /// <summary>
    /// 将 bool 值转换为 VuetifyScrimValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyScrimValue(bool value)
        => new(value);

    /// <summary>
    /// 将 string 值转换为 VuetifyScrimValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyScrimValue(string value)
        => new(value);
}


/// <summary>
/// 用于 VBadge.Transition、VBottomSheet.Transition、VCounter.Transition、VDataIterator.Transition、VDialog.Transition、VFab.Transition、VImg.Transition、VLazy.Transition、VMenu.Transition、VMessages.Transition、VOverlay.Transition、VSnackbar.Transition、VSnackbarQueue.Transition、VSpeedDial.Transition、VTooltip.Transition 的参数类型。
/// Sets the component transition. Can be one of the [built in](/styles/transitions/) or custom transition.
/// The transition to use when switching from `lazy-src` to `src`. Can be one of the [built in](/styles/transitions/) or custom transition.
/// Sets the component transition. Can be one of the [built in](/styles/transitions/) or custom transition. Supports special location-aware mode with **slide-auto** and **scroll-auto**
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyTransitionValue(
    bool,
    string,
    VueTransitionProps)
{
    /// <summary>
    /// 读取当前值的 bool 分支；不属于该分支时返回 null。
    /// </summary>
    public bool? AsBool
        => Value is bool value ? value : default(bool?);

    /// <summary>
    /// 读取当前值的 string 分支；不属于该分支时返回 null。
    /// </summary>
    public string? AsString
        => Value is string value ? value : default(string?);

    /// <summary>
    /// 读取当前值的 VueTransitionProps 分支；不属于该分支时返回 null。
    /// </summary>
    public VueTransitionProps? AsProps
        => Value is VueTransitionProps value ? value : default(VueTransitionProps?);

    /// <summary>
    /// 将 bool 值转换为 VuetifyTransitionValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyTransitionValue(bool value)
        => new(value);

    /// <summary>
    /// 将 string 值转换为 VuetifyTransitionValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyTransitionValue(string value)
        => new(value);

    /// <summary>
    /// 将 VueTransitionProps 值转换为 VuetifyTransitionValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyTransitionValue(VueTransitionProps value)
        => new(value);
}


/// <summary>
/// 控制输入控件的提示和校验消息区域是否自动隐藏。
/// </summary>
[String]
public enum VuetifyHideDetailsMode
{
    /// <summary>
    /// 自动选择；上游取值为 “auto”。
    /// </summary>
    [Description("@#auto")]
    Auto
}

/// <summary>
/// 用于 VFileInput.HideDetails、VInput.HideDetails、VInputComponentBase.HideDetails、VNumberInput.HideDetails、VRadioGroup.HideDetails、VSelectionControlComponentBase.HideDetails、VSelectLikeComponentBase.HideDetails 的参数类型。
/// Hides hint and validation errors. When set to `auto` messages will be rendered only if there's a message (hint, error message, counter value etc) to display.
/// 是否隐藏输入框的详情区域。
/// Whether to hide the details area of the input field.
/// 是否隐藏提示详细信息。
/// Whether to hide the details/hints section.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyHideDetailsValue(bool, VuetifyHideDetailsMode)
{
    /// <summary>
    /// 读取当前值的 bool 分支；不属于该分支时返回 null。
    /// </summary>
    public bool? AsBool
        => Value is bool value ? value : default(bool?);

    /// <summary>
    /// 读取当前值的 VuetifyHideDetailsMode 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyHideDetailsMode? AsMode
        => Value is VuetifyHideDetailsMode value ? value : default(VuetifyHideDetailsMode?);

    /// <summary>
    /// 将 bool 值转换为 VuetifyHideDetailsValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyHideDetailsValue(bool value)
        => new(value);

    /// <summary>
    /// 将 VuetifyHideDetailsMode 值转换为 VuetifyHideDetailsValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyHideDetailsValue(VuetifyHideDetailsMode value)
        => new(value);
}


/// <summary>
/// 用于 VFileInput.Messages、VInput.Messages、VInput.ErrorMessages、VInputComponentBase.ErrorMessages、VInputComponentBase.Messages、VMessages.Messages、VNumberInput.Messages、VRadioGroup.Messages、VRating.ItemLabels、VSelectionControlComponentBase.ErrorMessages、VSelectionControlComponentBase.Messages、VSelectLikeComponentBase.ErrorMessages、VSelectLikeComponentBase.Messages、VValidation.ErrorMessages 的参数类型。
/// Displays a list of messages or a single message if using a string.
/// Puts the input in an error state and passes through custom error messages. Will be combined with any validations that occur from the **rules** prop. This field will not trigger validation.
/// 输入框的错误消息。
/// Error messages for the input field.
/// 输入框的提示消息。
/// Hint messages for the input field.
/// Array of labels to display next to each item..
/// 错误状态下显示的消息。
/// Messages displayed in the error state.
/// 显示的提示消息。
/// The hint messages to display.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyMessagesValue(string, string[])
{
    /// <summary>
    /// 读取当前值的 string 分支；不属于该分支时返回 null。
    /// </summary>
    public string? AsString
        => Value is string value ? value : default(string?);

    /// <summary>
    /// 读取当前值的 string[] 分支；不属于该分支时返回 null。
    /// </summary>
    public string[]? AsStrings
        => Value is string[] value ? value : default(string[]?);

    /// <summary>
    /// 将 string 值转换为 VuetifyMessagesValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyMessagesValue(string value)
        => new(value);

    /// <summary>
    /// 将 string[] 值转换为 VuetifyMessagesValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyMessagesValue(string[] value)
        => new(value);
}


/// <summary>
/// 用于 VAutocomplete.AutoSelectFirst、VCombobox.AutoSelectFirst 的参数类型。
/// When searching, will always highlight the first option and select it on blur. `exact` will only highlight and select exact matches.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyAutoSelectFirstValue(bool, VuetifyAutoSelectFirstMode)
{
    /// <summary>
    /// 读取当前值的 bool 分支；不属于该分支时返回 null。
    /// </summary>
    public bool? AsBool
        => Value is bool value ? value : default(bool?);

    /// <summary>
    /// 读取当前值的 VuetifyAutoSelectFirstMode 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyAutoSelectFirstMode? AsMode
        => Value is VuetifyAutoSelectFirstMode value ? value : default(VuetifyAutoSelectFirstMode?);

    /// <summary>
    /// 将 bool 值转换为 VuetifyAutoSelectFirstValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyAutoSelectFirstValue(bool value)
        => new(value);

    /// <summary>
    /// 将 VuetifyAutoSelectFirstMode 值转换为 VuetifyAutoSelectFirstValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyAutoSelectFirstValue(VuetifyAutoSelectFirstMode value)
        => new(value);
}


/// <summary>
/// 用于 VFileInput.ShowSize 的参数类型。
/// Sets the displayed size of selected file(s). When using **true** will default to _1000_ displaying (**kB, MB, GB**) while _1024_ will display (**KiB, MiB, GiB**).
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyFileShowSizeValue(bool, VuetifyFileSizeBase)
{
    /// <summary>
    /// 读取当前值的 bool 分支；不属于该分支时返回 null。
    /// </summary>
    public bool? AsBool
        => Value is bool value ? value : default(bool?);

    /// <summary>
    /// 读取当前值的 VuetifyFileSizeBase 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyFileSizeBase? AsBase
        => Value is VuetifyFileSizeBase value ? value : default(VuetifyFileSizeBase?);

    /// <summary>
    /// 将 bool 值转换为 VuetifyFileShowSizeValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyFileShowSizeValue(bool value)
        => new(value);

    /// <summary>
    /// 将 VuetifyFileSizeBase 值转换为 VuetifyFileShowSizeValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyFileShowSizeValue(VuetifyFileSizeBase value)
        => new(value);
}


/// <summary>
/// 用于 VRangeSlider.ThumbLabel、VRangeSlider.ShowTicks、VSlider.ThumbLabel、VSlider.ShowTicks 的参数类型。
/// Show thumb label. If `true` it shows label when using slider. If set to `'always'` it always shows label. Use `'hover'` to show label when hovering over the thumb.
/// Show track ticks. If `true` it shows ticks when using slider. If set to `'always'` it always shows ticks.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyBooleanAlwaysValue(bool, VuetifyAlwaysMode)
{
    /// <summary>
    /// 读取当前值的 bool 分支；不属于该分支时返回 null。
    /// </summary>
    public bool? AsBool
        => Value is bool value ? value : default(bool?);

    /// <summary>
    /// 读取当前值的 VuetifyAlwaysMode 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyAlwaysMode? AsMode
        => Value is VuetifyAlwaysMode value ? value : default(VuetifyAlwaysMode?);

    /// <summary>
    /// 将 bool 值转换为 VuetifyBooleanAlwaysValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyBooleanAlwaysValue(bool value)
        => new(value);

    /// <summary>
    /// 将 VuetifyAlwaysMode 值转换为 VuetifyBooleanAlwaysValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyBooleanAlwaysValue(VuetifyAlwaysMode value)
        => new(value);
}


/// <summary>
/// 用于 VBtn.Loading、VCarousel.Progress、VDataTable.Loading、VDefaultsProvider.Root、VFab.Loading、VField.Loading、VOtpInput.Loading、VSnackbar.Timer、VSnackbarQueue.Timer、VSnackbarQueue.Closable、VSwitch.Loading、VTabsWindowItem.Transition、VTabsWindowItem.ReverseTransition 的参数类型。
/// Displays linear progress bar. Can either be a String which specifies which color is applied to the progress bar (any material color or theme color - **primary**, **secondary**, **success**, **info**, **warning**, **error**) or a Boolean which uses the component **color** (set by color prop - if it's supported by the component) or the primary color.
/// Displays a carousel progress bar. Requires the **cycle** prop and **interval**.
/// Displays `loading` slot if set to `true`
/// Force current defaults to match the application root defaults.
/// Display a progress bar that counts down until the snackbar closes. Use `bottom` to change the default placement.
/// Adds a dismiss button that closes the active snackbar.
/// Displays circular progress bar. Can either be a String which specifies which color is applied to the progress bar (any material color or theme color - primary, secondary, success, info, warning, error) or a Boolean which uses the component color (set by color prop - if it's supported by the component) or the primary color.
/// The transition used when the component progressing through items. Can be one of the [built in](/styles/transitions/) or custom transition.
/// Sets the reverse transition.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyBooleanStringValue(bool, string)
{
    /// <summary>
    /// 读取当前值的 bool 分支；不属于该分支时返回 null。
    /// </summary>
    public bool? AsBool
        => Value is bool value ? value : default(bool?);

    /// <summary>
    /// 读取当前值的 string 分支；不属于该分支时返回 null。
    /// </summary>
    public string? AsString
        => Value is string value ? value : default(string?);

    /// <summary>
    /// 将 bool 值转换为 VuetifyBooleanStringValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyBooleanStringValue(bool value)
        => new(value);

    /// <summary>
    /// 将 string 值转换为 VuetifyBooleanStringValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyBooleanStringValue(string value)
        => new(value);
}


/// <summary>
/// 用于 VInputComponentBase.Counter 的参数类型。
/// 是否显示字符计数器。
/// Whether to show a character counter.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyCounterValue(
    bool,
    Number,
    string)
{
    /// <summary>
    /// 读取当前值的 bool 分支；不属于该分支时返回 null。
    /// </summary>
    public bool? AsBool
        => Value is bool value ? value : default(bool?);

    /// <summary>
    /// 读取当前值的 Number 分支；不属于该分支时返回 null。
    /// </summary>
    public Number? AsNumber
        => Value is Number value ? value : default(Number?);

    /// <summary>
    /// 读取当前值的 string 分支；不属于该分支时返回 null。
    /// </summary>
    public string? AsString
        => Value is string value ? value : default(string?);

    /// <summary>
    /// 将 bool 值转换为 VuetifyCounterValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyCounterValue(bool value)
        => new(value);

    /// <summary>
    /// 将 Number 值转换为 VuetifyCounterValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyCounterValue(Number value)
        => new(value);

    /// <summary>
    /// 将 string 值转换为 VuetifyCounterValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyCounterValue(string value)
        => new(value);

    /// <summary>
    /// 将 byte 值转换为 VuetifyCounterValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyCounterValue(byte value)
        => new((Number)value);

    /// <summary>
    /// 将 sbyte 值转换为 VuetifyCounterValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyCounterValue(sbyte value)
        => new((Number)value);

    /// <summary>
    /// 将 short 值转换为 VuetifyCounterValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyCounterValue(short value)
        => new((Number)value);

    /// <summary>
    /// 将 ushort 值转换为 VuetifyCounterValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyCounterValue(ushort value)
        => new((Number)value);

    /// <summary>
    /// 将 int 值转换为 VuetifyCounterValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyCounterValue(int value)
        => new((Number)value);

    /// <summary>
    /// 将 uint 值转换为 VuetifyCounterValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyCounterValue(uint value)
        => new((Number)value);

    /// <summary>
    /// 将 float 值转换为 VuetifyCounterValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyCounterValue(float value)
        => new((Number)value);

    /// <summary>
    /// 将 double 值转换为 VuetifyCounterValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyCounterValue(double value)
        => new((Number)value);

    /// <summary>
    /// 将 decimal 值转换为 VuetifyCounterValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyCounterValue(decimal value)
        => new((Number)value);
}


/// <summary>
/// 用于 VBanner.Text、VBtn.Text、VCard.Title、VCard.Subtitle、VCard.Text、VCardItem.Subtitle、VCardItem.Title、VChip.Text、VExpansionPanel.Text、VExpansionPanel.Title、VFab.Text、VListItem.Subtitle、VListItem.Title 的参数类型。
/// Specify content text for the component.
/// Specify a title text for the component.
/// Specify a subtitle text for the component.
/// Generates a `v-list-item-title` component with the supplied value. Note that this overrides the native [`title`](https://developer.mozilla.org/en-US/docs/Web/HTML/Global_attributes/title) attribute, that must be set with `v-bind:title.attr` instead.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyTextValue(
    string,
    Number,
    bool)
{
    /// <summary>
    /// 读取当前值的 string 分支；不属于该分支时返回 null。
    /// </summary>
    public string? AsString
        => Value is string value ? value : default(string?);

    /// <summary>
    /// 读取当前值的 Number 分支；不属于该分支时返回 null。
    /// </summary>
    public Number? AsNumber
        => Value is Number value ? value : default(Number?);

    /// <summary>
    /// 读取当前值的 bool 分支；不属于该分支时返回 null。
    /// </summary>
    public bool? AsBool
        => Value is bool value ? value : default(bool?);

    /// <summary>
    /// 将 string 值转换为 VuetifyTextValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyTextValue(string value)
        => new(value);

    /// <summary>
    /// 将 Number 值转换为 VuetifyTextValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyTextValue(Number value)
        => new(value);

    /// <summary>
    /// 将 bool 值转换为 VuetifyTextValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyTextValue(bool value)
        => new(value);

    /// <summary>
    /// 将 byte 值转换为 VuetifyTextValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyTextValue(byte value)
        => new((Number)value);

    /// <summary>
    /// 将 sbyte 值转换为 VuetifyTextValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyTextValue(sbyte value)
        => new((Number)value);

    /// <summary>
    /// 将 short 值转换为 VuetifyTextValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyTextValue(short value)
        => new((Number)value);

    /// <summary>
    /// 将 ushort 值转换为 VuetifyTextValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyTextValue(ushort value)
        => new((Number)value);

    /// <summary>
    /// 将 int 值转换为 VuetifyTextValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyTextValue(int value)
        => new((Number)value);

    /// <summary>
    /// 将 uint 值转换为 VuetifyTextValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyTextValue(uint value)
        => new((Number)value);

    /// <summary>
    /// 将 float 值转换为 VuetifyTextValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyTextValue(float value)
        => new((Number)value);

    /// <summary>
    /// 将 double 值转换为 VuetifyTextValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyTextValue(double value)
        => new((Number)value);

    /// <summary>
    /// 将 decimal 值转换为 VuetifyTextValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyTextValue(decimal value)
        => new((Number)value);
}


/// <summary>
/// 用于 VAlert.Rounded、VAvatar.Rounded、VBadge.Rounded、VBanner.Rounded、VBottomNavigation.Rounded、VBtn.Rounded、VBtnGroup.Rounded、VBtnToggle.Rounded、VCard.Rounded、VChip.Rounded、VColorPicker.Rounded、VDatePicker.Rounded、VExpansionPanel.Rounded、VFab.Rounded、VField.Rounded、VFileUpload.Rounded、VFooter.Rounded、VIconBtn.Rounded、VImg.Rounded、VInputComponentBase.Rounded、VList.Rounded、VListItem.Rounded、VPicker.Rounded、VProgressLinear.Rounded、VSheet.Rounded、VSnackbar.Rounded、VSnackbarQueue.Rounded、VStepper.Rounded、VStepperVertical.Rounded、VSystemBar.Rounded、VTimePicker.Rounded、VToolbar.Rounded、VTreeview.Rounded 的参数类型。
/// Designates the **border-radius** applied to the component. You can use the predefined sizes **0**, **xs**, **sm**, **md**, **lg**, **xl**, **pill**, **circle**, and **shaped**, pass `true` for the component default. Since v4.1 you can also provide any valid CSS value (e.g. `8px`, `50%`, `1em`) or number (converted to `px`). Find more information on available border radius classes on the [Border Radius page](/styles/border-radius).
/// Round edge buttons.
/// 组件的圆角样式。
/// Border radius style of the component.
/// 输入框的圆角样式。
/// Border radius style of the input field.
/// Applies a border radius to the first and last panel. Since v4.1.0 accepts array of two values to customize inner radius.
/// Provides an alternative active style for `v-treeview` node. Only visible when `activatable` is `true` and should not be used in conjunction with the `shaped` prop.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyRoundedValue(
    bool,
    Number,
    string)
{
    /// <summary>
    /// 读取当前值的 bool 分支；不属于该分支时返回 null。
    /// </summary>
    public bool? AsBool
        => Value is bool value ? value : default(bool?);

    /// <summary>
    /// 读取当前值的 Number 分支；不属于该分支时返回 null。
    /// </summary>
    public Number? AsNumber
        => Value is Number value ? value : default(Number?);

    /// <summary>
    /// 读取当前值的 string 分支；不属于该分支时返回 null。
    /// </summary>
    public string? AsString
        => Value is string value ? value : default(string?);

    /// <summary>
    /// 将 bool 值转换为 VuetifyRoundedValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyRoundedValue(bool value)
        => new(value);

    /// <summary>
    /// 将 Number 值转换为 VuetifyRoundedValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyRoundedValue(Number value)
        => new(value);

    /// <summary>
    /// 将 string 值转换为 VuetifyRoundedValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyRoundedValue(string value)
        => new(value);

    /// <summary>
    /// 将 byte 值转换为 VuetifyRoundedValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyRoundedValue(byte value)
        => new((Number)value);

    /// <summary>
    /// 将 sbyte 值转换为 VuetifyRoundedValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyRoundedValue(sbyte value)
        => new((Number)value);

    /// <summary>
    /// 将 short 值转换为 VuetifyRoundedValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyRoundedValue(short value)
        => new((Number)value);

    /// <summary>
    /// 将 ushort 值转换为 VuetifyRoundedValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyRoundedValue(ushort value)
        => new((Number)value);

    /// <summary>
    /// 将 int 值转换为 VuetifyRoundedValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyRoundedValue(int value)
        => new((Number)value);

    /// <summary>
    /// 将 uint 值转换为 VuetifyRoundedValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyRoundedValue(uint value)
        => new((Number)value);

    /// <summary>
    /// 将 float 值转换为 VuetifyRoundedValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyRoundedValue(float value)
        => new((Number)value);

    /// <summary>
    /// 将 double 值转换为 VuetifyRoundedValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyRoundedValue(double value)
        => new((Number)value);

    /// <summary>
    /// 将 decimal 值转换为 VuetifyRoundedValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyRoundedValue(decimal value)
        => new((Number)value);
}


/// <summary>
/// 环形进度条不确定进度动画的特殊模式。
/// </summary>
[String]
public enum VuetifyProgressCircularIndeterminateMode
{
    /// <summary>
    /// 不确定进度动画中禁用收缩效果；上游取值为 “disable-shrink”。
    /// </summary>
    [Description("@#disable-shrink")]
    DisableShrink
}

/// <summary>
/// 用于 VProgressCircular.Indeterminate 的参数类型。
/// Constantly animates, use when loading progress is unknown. If set to the string `'disable-shrink'` it will use a simpler animation that does not run on the main thread.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyProgressCircularIndeterminateValue(bool, VuetifyProgressCircularIndeterminateMode)
{
    /// <summary>
    /// 读取当前值的 bool 分支；不属于该分支时返回 null。
    /// </summary>
    public bool? AsBool
        => Value is bool value ? value : default(bool?);

    /// <summary>
    /// 读取当前值的 VuetifyProgressCircularIndeterminateMode 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyProgressCircularIndeterminateMode? AsMode
        => Value is VuetifyProgressCircularIndeterminateMode value ? value : default(VuetifyProgressCircularIndeterminateMode?);

    /// <summary>
    /// 将 bool 值转换为 VuetifyProgressCircularIndeterminateValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyProgressCircularIndeterminateValue(bool value)
        => new(value);

    /// <summary>
    /// 将 VuetifyProgressCircularIndeterminateMode 值转换为 VuetifyProgressCircularIndeterminateValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyProgressCircularIndeterminateValue(VuetifyProgressCircularIndeterminateMode value)
        => new(value);
}


/// <summary>
/// 用于 VFileInput.ModelValue、VFileInput.ModelValueChanged、VFileUpload.ModelValue 的参数类型。
/// The v-model value of the component. If component supports the **multiple** prop, this defaults to an empty array.
/// 文件输入绑定值变化时的回调。
/// Callback when the file input value changes.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyFileModelValue(FileRef, FileRef[])
{
    /// <summary>
    /// 读取当前值的 FileRef 分支；不属于该分支时返回 null。
    /// </summary>
    public FileRef? AsFile
        => Value is FileRef value ? value : default(FileRef?);

    /// <summary>
    /// 读取当前值的 FileRef[] 分支；不属于该分支时返回 null。
    /// </summary>
    public FileRef[]? AsFiles
        => Value is FileRef[] value ? value : default(FileRef[]?);

    /// <summary>
    /// 将 FileRef 值转换为 VuetifyFileModelValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyFileModelValue(FileRef value)
        => new(value);

    /// <summary>
    /// 将 FileRef[] 值转换为 VuetifyFileModelValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyFileModelValue(FileRef[] value)
        => new(value);
}


/// <summary>
/// 用于 VRangeSlider.ModelValue、VRangeSlider.ModelValueChanged 的参数类型。
/// The v-model value of the component. If component supports the **multiple** prop, this defaults to an empty array.
/// 范围值变更时触发的回调。
/// Callback invoked when the range value changes.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyRangeSliderModelValue(Number[], string[])
{
    /// <summary>
    /// 读取当前值的 Number[] 分支；不属于该分支时返回 null。
    /// </summary>
    public Number[]? AsArray
        => Value is Number[] value ? value : default(Number[]?);

    /// <summary>
    /// 读取当前值的 string[] 分支；不属于该分支时返回 null。
    /// </summary>
    public string[]? AsStrings
        => Value as string[];

    /// <summary>
    /// 将 Number[] 值转换为 VuetifyRangeSliderModelValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyRangeSliderModelValue(Number[] values)
        => new(values);

    /// <summary>
    /// 将 double[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyRangeSliderModelValue(double[] values)
        => new(Array.ConvertAll(values, static value => (Number)value));

    /// <summary>
    /// 将 int[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyRangeSliderModelValue(int[] values)
        => new(Array.ConvertAll(values, static value => (Number)value));
}


/// <summary>
/// 用于 VForm.ValidateOn、VInput.ValidateOn、VInputComponentBase.ValidateOn、VSelectionControlComponentBase.ValidateOn、VSelectLikeComponentBase.ValidateOn、VValidation.ValidateOn 的可选取值。
/// Changes the events in which validation occurs.
/// Change what type of event triggers validation to run.
/// 输入验证的触发时机。
/// When to trigger input validation.
/// 验证触发时机。
/// When to trigger validation.
/// </summary>
[String]
public enum VuetifyValidateOn
{
    /// <summary>
    /// 输入事件触发校验；上游取值为 “input”。
    /// </summary>
    [Description("@#input")]
    Input,

    /// <summary>
    /// 失去焦点时触发校验；上游取值为 “blur”。
    /// </summary>
    [Description("@#blur")]
    Blur,

    /// <summary>
    /// 提交表单时触发校验；上游取值为 “submit”。
    /// </summary>
    [Description("@#submit")]
    Submit,

    /// <summary>
    /// 已有无效输入时，在输入事件触发校验；上游取值为 “invalid-input”。
    /// </summary>
    [Description("@#invalid-input")]
    InvalidInput,

    /// <summary>
    /// 延迟执行初始校验；上游取值为 “lazy”。
    /// </summary>
    [Description("@#lazy")]
    Lazy,

    /// <summary>
    /// 提前执行校验；上游取值为 “eager”。
    /// </summary>
    [Description("@#eager")]
    Eager,

    /// <summary>
    /// 输入事件触发校验；延迟执行初始校验；上游取值为 “input lazy”。
    /// </summary>
    [Description("@#input lazy")]
    InputLazy,

    /// <summary>
    /// 输入事件触发校验；提前执行校验；上游取值为 “input eager”。
    /// </summary>
    [Description("@#input eager")]
    InputEager,

    /// <summary>
    /// 失去焦点时触发校验；延迟执行初始校验；上游取值为 “blur lazy”。
    /// </summary>
    [Description("@#blur lazy")]
    BlurLazy,

    /// <summary>
    /// 失去焦点时触发校验；提前执行校验；上游取值为 “blur eager”。
    /// </summary>
    [Description("@#blur eager")]
    BlurEager,

    /// <summary>
    /// 提交表单时触发校验；延迟执行初始校验；上游取值为 “submit lazy”。
    /// </summary>
    [Description("@#submit lazy")]
    SubmitLazy,

    /// <summary>
    /// 提交表单时触发校验；提前执行校验；上游取值为 “submit eager”。
    /// </summary>
    [Description("@#submit eager")]
    SubmitEager,

    /// <summary>
    /// 已有无效输入时，在输入事件触发校验；延迟执行初始校验；上游取值为 “invalid-input lazy”。
    /// </summary>
    [Description("@#invalid-input lazy")]
    InvalidInputLazy,

    /// <summary>
    /// 已有无效输入时，在输入事件触发校验；提前执行校验；上游取值为 “invalid-input eager”。
    /// </summary>
    [Description("@#invalid-input eager")]
    InvalidInputEager,

    /// <summary>
    /// 延迟执行初始校验；输入事件触发校验；上游取值为 “lazy input”。
    /// </summary>
    [Description("@#lazy input")]
    LazyInput,

    /// <summary>
    /// 提前执行校验；输入事件触发校验；上游取值为 “eager input”。
    /// </summary>
    [Description("@#eager input")]
    EagerInput,

    /// <summary>
    /// 延迟执行初始校验；失去焦点时触发校验；上游取值为 “lazy blur”。
    /// </summary>
    [Description("@#lazy blur")]
    LazyBlur,

    /// <summary>
    /// 提前执行校验；失去焦点时触发校验；上游取值为 “eager blur”。
    /// </summary>
    [Description("@#eager blur")]
    EagerBlur,

    /// <summary>
    /// 延迟执行初始校验；提交表单时触发校验；上游取值为 “lazy submit”。
    /// </summary>
    [Description("@#lazy submit")]
    LazySubmit,

    /// <summary>
    /// 提前执行校验；提交表单时触发校验；上游取值为 “eager submit”。
    /// </summary>
    [Description("@#eager submit")]
    EagerSubmit,

    /// <summary>
    /// 延迟执行初始校验；已有无效输入时，在输入事件触发校验；上游取值为 “lazy invalid-input”。
    /// </summary>
    [Description("@#lazy invalid-input")]
    LazyInvalidInput,

    /// <summary>
    /// 提前执行校验；已有无效输入时，在输入事件触发校验；上游取值为 “eager invalid-input”。
    /// </summary>
    [Description("@#eager invalid-input")]
    EagerInvalidInput
}

/// <summary>
/// 用于 VCarousel.Direction、VChipGroup.Direction、VInfiniteScroll.Direction、VInput.Direction、VSlideGroup.Direction、VTabsWindow.Direction、VWindow.Direction 的可选取值。
/// The transition direction when changing windows.
/// Switch between horizontal and vertical modes.
/// Specifies if scroller is **vertical** or **horizontal**.
/// Changes the direction of the input.
/// </summary>
[String]
public enum VuetifyInputDirection
{
    /// <summary>
    /// 水平方向；上游取值为 “horizontal”。
    /// </summary>
    [Description("@#horizontal")]
    Horizontal,

    /// <summary>
    /// 垂直方向；上游取值为 “vertical”。
    /// </summary>
    [Description("@#vertical")]
    Vertical
}

/// <summary>
/// 用于 VInput.Disabled、VInput.Readonly、VSelectionControl.Disabled、VSelectionControl.Readonly、VSelectionControl.Multiple、VSelectionControlGroup.Disabled、VSelectionControlGroup.Readonly、VSelectionControlGroup.Multiple、VValidation.Disabled、VValidation.Readonly 的参数类型。
/// Removes the ability to click or target the component.
/// Puts input in readonly state.
/// Changes select to multiple. Accepts array for value.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyNullableBoolean(bool)
{
    /// <summary>
    /// 读取当前值的 bool 分支；不属于该分支时返回 null。
    /// </summary>
    public bool? AsBool
        => Value is bool value ? value : default(bool?);

    /// <summary>
    /// 构造显式的 JavaScript null 分支，用于向宿主 API 传入空值。
    /// </summary>
    [ECMAScriptInline("null")]
    public extern static VuetifyNullableBoolean Null();

    /// <summary>
    /// 将 bool 值转换为 VuetifyNullableBoolean，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyNullableBoolean(bool value)
        => new(value);
}


/// <summary>
/// 用于 VField.IconColor、VInput.IconColor 的参数类型。
/// Sets the color of the prepend/append icons.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyIconColorValue(bool, string)
{
    /// <summary>
    /// 读取当前值的 bool 分支；不属于该分支时返回 null。
    /// </summary>
    public bool? AsBool
        => Value is bool value ? value : default(bool?);

    /// <summary>
    /// 读取当前值的 string 分支；不属于该分支时返回 null。
    /// </summary>
    public string? AsString
        => Value is string value ? value : default(string?);

    /// <summary>
    /// 将 bool 值转换为 VuetifyIconColorValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyIconColorValue(bool value)
        => new(value);

    /// <summary>
    /// 将 string 值转换为 VuetifyIconColorValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyIconColorValue(string value)
        => new(value);
}


/// <summary>
/// C# 联合参数，允许 bool, string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取相应分支。
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyValidationResult(bool, string)
{
    /// <summary>
    /// 读取当前值的 bool 分支；不属于该分支时返回 null。
    /// </summary>
    public bool? AsBool
        => Value is bool value ? value : default(bool?);

    /// <summary>
    /// 读取当前值的 string 分支；不属于该分支时返回 null。
    /// </summary>
    public string? AsString
        => Value is string value ? value : default(string?);

    /// <summary>
    /// 将 bool 值转换为 VuetifyValidationResult，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyValidationResult(bool value)
        => new(value);

    /// <summary>
    /// 将 string 值转换为 VuetifyValidationResult，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyValidationResult(string value)
        => new(value);
}


/// <summary>
/// 用于 VuetifyValidationRule.AsResolver 的回调签名。
/// 读取当前值的 VuetifyValidationRuleResolver 分支；不属于该分支时返回 null。
/// </summary>
/// <param name="value">要传入的值，保持其声明的类型和数据。</param>
public delegate VuetifyValidationResult VuetifyValidationRuleResolver(VueValue? value);

/// <summary>
/// 用于 VuetifyValidationRule.AsAsyncResolver 的回调签名。
/// 读取当前值的 VuetifyAsyncValidationRuleResolver 分支；不属于该分支时返回 null。
/// </summary>
/// <param name="value">要传入的值，保持其声明的类型和数据。</param>
public delegate IPromise<VuetifyValidationResult> VuetifyAsyncValidationRuleResolver(VueValue? value);

/// <summary>
/// 用于 VInput.Rules、VValidation.Rules 的参数类型。
/// Accepts a mixed array of types `function`, `boolean` and `string`. Functions pass an input value as an argument and must return either `true` / `false` or a `string` containing an error message. The input field will enter an error state if a function returns (or any value in the array contains) `false` or is a `string`.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyValidationRule(
    VuetifyValidationResult,
    VuetifyValidationRuleResolver,
    IPromise<VuetifyValidationResult>,
    VuetifyAsyncValidationRuleResolver)
{
    /// <summary>
    /// 读取当前值的 VuetifyValidationResult 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyValidationResult? AsResult
        => Value is VuetifyValidationResult value ? value : default(VuetifyValidationResult?);

    /// <summary>
    /// 读取当前值的 VuetifyValidationRuleResolver 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyValidationRuleResolver? AsResolver
        => Value is VuetifyValidationRuleResolver value ? value : default(VuetifyValidationRuleResolver?);

    /// <summary>
    /// 读取当前值的 IPromise&lt;VuetifyValidationResult&gt; 分支；不属于该分支时返回 null。
    /// </summary>
    public IPromise<VuetifyValidationResult>? AsPromise
        => Value is IPromise<VuetifyValidationResult> value ? value : default(IPromise<VuetifyValidationResult>?);

    /// <summary>
    /// 读取当前值的 VuetifyAsyncValidationRuleResolver 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyAsyncValidationRuleResolver? AsAsyncResolver
        => Value is VuetifyAsyncValidationRuleResolver value ? value : default(VuetifyAsyncValidationRuleResolver?);

    /// <summary>
    /// 将 VuetifyValidationResult 值转换为 VuetifyValidationRule，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyValidationRule(VuetifyValidationResult value)
        => new(value);

    /// <summary>
    /// 将 bool 值转换为 VuetifyValidationRule，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyValidationRule(bool value)
        => new((VuetifyValidationResult)value);

    /// <summary>
    /// 将 string 值转换为 VuetifyValidationRule，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyValidationRule(string value)
        => new((VuetifyValidationResult)value);

    /// <summary>
    /// 将 VuetifyValidationRuleResolver 值转换为 VuetifyValidationRule，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyValidationRule(VuetifyValidationRuleResolver value)
        => new(value);

    /// <summary>
    /// 将 VuetifyAsyncValidationRuleResolver 值转换为 VuetifyValidationRule，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyValidationRule(VuetifyAsyncValidationRuleResolver value)
        => new(value);
}


/// <summary>
/// 用于 VAlert.Position、VBanner.Position、VBtn.Position、VColorPicker.Position、VDatePicker.Position、VFab.Position、VFileUpload.Position、VPicker.Position、VSheet.Position、VSnackbar.Position、VSnackbarQueue.Position、VStepper.Position、VTimePicker.Position 的可选取值。
/// Sets the position for the component.
/// 组件的位置。
/// Position of the component.
/// </summary>
[String]
public enum VuetifyPosition
{
    /// <summary>
    /// 使用静态定位；上游取值为 “static”。
    /// </summary>
    [Description("@#static")]
    Static,

    /// <summary>
    /// 相对于元素正常布局位置进行偏移；上游取值为 “relative”。
    /// </summary>
    [Description("@#relative")]
    Relative,

    /// <summary>
    /// 使用固定定位；上游取值为 “fixed”。
    /// </summary>
    [Description("@#fixed")]
    Fixed,

    /// <summary>
    /// 使用绝对定位；上游取值为 “absolute”。
    /// </summary>
    [Description("@#absolute")]
    Absolute,

    /// <summary>
    /// 在滚动时保持吸附；上游取值为 “sticky”。
    /// </summary>
    [Description("@#sticky")]
    Sticky
}

/// <summary>
/// 用于 VBanner.Mobile、VChipGroup.Mobile、VDateInput.Mobile、VSlideGroup.Mobile、VStepper.Mobile、VStepperVertical.Mobile 的参数类型。
/// Applies the mobile banner styles.
/// Determines the display mode of the component. If true, the component will be displayed in mobile mode. If false, the component will be displayed in desktop mode. If null, will be based on the current mobile-breakpoint
/// Forces the stepper into a mobile state, removing labels from stepper items.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyMobileValue(bool)
{
    /// <summary>
    /// 读取当前值的 bool 分支；不属于该分支时返回 null。
    /// </summary>
    public bool? AsBool
        => Value is bool value ? value : default(bool?);

    /// <summary>
    /// 交由 Vuetify 的显示断点规则决定是否采用移动布局；传入 null。
    /// </summary>
    [ECMAScriptInline("null")]
    public extern static VuetifyMobileValue Auto();

    /// <summary>
    /// 将 bool 值转换为 VuetifyMobileValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyMobileValue(bool value)
        => new(value);
}


/// <summary>
/// 用于 VAvatar.Border、VBanner.Border、VBottomNavigation.Border、VBtn.Border、VBtnGroup.Border、VBtnToggle.Border、VChip.Border、VColorPicker.Border、VDatePicker.Border、VFab.Border、VFileUpload.Border、VFooter.Border、VIconBtn.Border、VPicker.Border、VSheet.Border、VStepper.Border、VTimePicker.Border、VToolbar.Border、VTreeview.Border 的参数类型。
/// Applies utility border classes to the component. To use it, you need to omit the `border-` prefix, (for example use `border-sm` as `border=&quot;sm&quot;`).  Find a list of the built-in border classes on the [borders page](/styles/borders).
/// 组件的边框样式。
/// Border style of the component.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyBorderValue(
    bool,
    Number,
    string)
{
    /// <summary>
    /// 读取当前值的 bool 分支；不属于该分支时返回 null。
    /// </summary>
    public bool? AsBool
        => Value is bool value ? value : default(bool?);

    /// <summary>
    /// 读取当前值的 Number 分支；不属于该分支时返回 null。
    /// </summary>
    public Number? AsNumber
        => Value is Number value ? value : default(Number?);

    /// <summary>
    /// 读取当前值的 string 分支；不属于该分支时返回 null。
    /// </summary>
    public string? AsString
        => Value is string value ? value : default(string?);

    /// <summary>
    /// 将 bool 值转换为 VuetifyBorderValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyBorderValue(bool value)
        => new(value);

    /// <summary>
    /// 将 Number 值转换为 VuetifyBorderValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyBorderValue(Number value)
        => new(value);

    /// <summary>
    /// 将 string 值转换为 VuetifyBorderValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyBorderValue(string value)
        => new(value);

    /// <summary>
    /// 将 byte 值转换为 VuetifyBorderValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyBorderValue(byte value)
        => new((Number)value);

    /// <summary>
    /// 将 sbyte 值转换为 VuetifyBorderValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyBorderValue(sbyte value)
        => new((Number)value);

    /// <summary>
    /// 将 short 值转换为 VuetifyBorderValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyBorderValue(short value)
        => new((Number)value);

    /// <summary>
    /// 将 ushort 值转换为 VuetifyBorderValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyBorderValue(ushort value)
        => new((Number)value);

    /// <summary>
    /// 将 int 值转换为 VuetifyBorderValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyBorderValue(int value)
        => new((Number)value);

    /// <summary>
    /// 将 uint 值转换为 VuetifyBorderValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyBorderValue(uint value)
        => new((Number)value);

    /// <summary>
    /// 将 float 值转换为 VuetifyBorderValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyBorderValue(float value)
        => new((Number)value);

    /// <summary>
    /// 将 double 值转换为 VuetifyBorderValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyBorderValue(double value)
        => new((Number)value);

    /// <summary>
    /// 将 decimal 值转换为 VuetifyBorderValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyBorderValue(decimal value)
        => new((Number)value);
}


/// <summary>
/// 用于 VAlert.CloseIcon、VAvatar.Icon、VBadge.Icon、VBanner.Icon、VBtn.Icon、VCalendar.NextIcon、VCalendar.PrevIcon、VCarousel.DelimiterIcon、VCarousel.NextIcon、VCarousel.PrevIcon、VChip.AppendIcon、VChip.CloseIcon、VChip.FilterIcon、VChip.PrependIcon、VDatePicker.NextIcon、VDatePicker.PrevIcon、VDatePicker.ModeIcon、VEmptyState.Icon、VFab.Icon、VField.AppendInnerIcon、VField.ClearIcon、VField.PrependInnerIcon、VFileUpload.Icon、VIcon.Icon、VIconBtn.ActiveIcon、VIconBtn.Icon、VInput.PrependIcon、VInput.AppendIcon、VRating.EmptyIcon、VRating.FullIcon、VRating.HalfIcon、VSelectionControl.FalseIcon、VSelectionControl.TrueIcon、VSelectionControlComponentBase.FalseIcon、VSelectionControlComponentBase.TrueIcon、VSelectionControlComponentBase.IndeterminateIcon、VSelectionControlGroup.FalseIcon、VSelectionControlGroup.TrueIcon、VSlideGroup.NextIcon、VSlideGroup.PrevIcon、VStepper.CompleteIcon、VStepper.EditIcon、VStepper.ErrorIcon、VStepperVertical.CollapseIcon、VStepperVertical.ExpandIcon、VStepperVertical.CompleteIcon、VStepperVertical.EditIcon、VStepperVertical.ErrorIcon、VTreeview.CollapseIcon、VTreeview.ExpandIcon、VTreeview.IndeterminateIcon、VTreeview.FalseIcon、VTreeview.TrueIcon、VWindow.NextIcon、VWindow.PrevIcon 的参数类型。
/// Change the default icon used for **closable** alerts.
/// Apply a specific icon using the [v-icon](/components/icons/) component.
/// Apply a specific icon using the [v-icon](/components/icons/) component. The button will become _round_.
/// 下一页图标。
/// Next page icon.
/// 上一页图标。
/// Previous page icon.
/// Sets icon for carousel delimiter.
/// The displayed icon for forcing pagination to the next item.
/// The displayed icon for forcing pagination to the previous item.
/// Creates a [v-icon](/api/v-icon/) component after default content in the **append** slot.
/// Change the default icon used for **close** chips.
/// Change the default icon used for **filter** chips.
/// Creates a [v-icon](/api/v-icon/) component in the **prepend** slot before default content.
/// Sets the icon for next month/year button.
/// Sets the icon for previous month/year button.
/// Icon displayed next to the current month and year, toggles year selection when clicked.
/// Creates a [v-icon](/api/v-icon/) component in the **append-inner** slot.
/// The icon used when the **clearable** prop is set to true.
/// Creates a [v-icon](/api/v-icon/) component in the **prepend-inner** slot.
/// When active is a boolean, this icon is used when active is true.
/// Prepends an icon to the component, uses the same syntax as `v-icon`.
/// The icon displayed when empty.
/// The icon displayed when full.
/// 半选时显示的图标。
/// The icon displayed for half-filled items.
/// The icon used when inactive.
/// The icon used when active.
/// 未选中状态下显示的图标。
/// The icon displayed when unchecked.
/// 选中状态下显示的图标。
/// The icon displayed when checked.
/// 不确定状态下显示的图标。
/// The icon displayed when indeterminate.
/// The appended slot when arrows are shown.
/// The prepended slot when arrows are shown.
/// Icon to display when step is marked as completed.
/// Icon to display when step is editable.
/// Icon to display when step has an error.
/// Icon used when the expansion panel is in a collapsable state.
/// Icon used when the expansion panel is in a expandable state.
/// Icon to display when the list item is expanded.
/// Icon used to indicate that a node can be expanded.
/// Icon used when node is in an indeterminate state. Only visible when `selectable` is `true`.
/// Icon used for the &quot;next&quot; button if `show-arrows` is `true`.
/// Icon used for the &quot;prev&quot; button if `show-arrows` is `true`.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyIconValue(
    bool,
    string,
    Symbol,
    VueProps)
{
    /// <summary>
    /// 读取当前值的 bool 分支；不属于该分支时返回 null。
    /// </summary>
    public bool? AsBool
        => Value is bool value ? value : default(bool?);

    /// <summary>
    /// 读取当前值的 string 分支；不属于该分支时返回 null。
    /// </summary>
    public string? AsString
        => Value is string value ? value : default(string?);

    /// <summary>
    /// 读取当前值的 Symbol 分支；不属于该分支时返回 null。
    /// </summary>
    public Symbol? AsSymbol
        => Value is Symbol value ? value : default(Symbol?);

    /// <summary>
    /// 读取当前值的 VueProps 分支；不属于该分支时返回 null。
    /// </summary>
    public VueProps? AsComponent
        => Value is VueProps value ? value : default(VueProps?);

    /// <summary>
    /// 将 bool 值转换为 VuetifyIconValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyIconValue(bool value)
        => new(value);

    /// <summary>
    /// 将 string 值转换为 VuetifyIconValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyIconValue(string value)
        => new(value);

    /// <summary>
    /// 将 Symbol 值转换为 VuetifyIconValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyIconValue(Symbol value)
        => new(value);

    /// <summary>
    /// 将 VueProps 值转换为 VuetifyIconValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyIconValue(VueProps value)
        => new(value);

    /// <summary>
    /// 将 VueDictionary 值转换为 VuetifyIconValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyIconValue(VueDictionary value)
        => new(value);
}


/// <summary>
/// 用于 VuetifyCounterValueSource.AsResolver 的回调签名。
/// 读取当前值的 VuetifyCounterValueResolver 分支；不属于该分支时返回 null。
/// </summary>
/// <param name="value">要传入的值，保持其声明的类型和数据。</param>
public delegate Number VuetifyCounterValueResolver(string? value);

/// <summary>
/// 用于 VInputComponentBase.CounterValue 的参数类型。
/// 字符计数器的自定义值来源。
/// Custom value source for the character counter.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyCounterValueSource(Number, VuetifyCounterValueResolver)
{
    /// <summary>
    /// 读取当前值的 Number 分支；不属于该分支时返回 null。
    /// </summary>
    public Number? AsNumber
        => Value is Number value ? value : default(Number?);

    /// <summary>
    /// 读取当前值的 VuetifyCounterValueResolver 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyCounterValueResolver? AsResolver
        => Value is VuetifyCounterValueResolver value ? value : default(VuetifyCounterValueResolver?);

    /// <summary>
    /// 将 Number 值转换为 VuetifyCounterValueSource，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyCounterValueSource(Number value)
        => new(value);

    /// <summary>
    /// 将 VuetifyCounterValueResolver 值转换为 VuetifyCounterValueSource，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyCounterValueSource(VuetifyCounterValueResolver value)
        => new(value);

    /// <summary>
    /// 将 byte 值转换为 VuetifyCounterValueSource，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyCounterValueSource(byte value)
        => new((Number)value);

    /// <summary>
    /// 将 sbyte 值转换为 VuetifyCounterValueSource，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyCounterValueSource(sbyte value)
        => new((Number)value);

    /// <summary>
    /// 将 short 值转换为 VuetifyCounterValueSource，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyCounterValueSource(short value)
        => new((Number)value);

    /// <summary>
    /// 将 ushort 值转换为 VuetifyCounterValueSource，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyCounterValueSource(ushort value)
        => new((Number)value);

    /// <summary>
    /// 将 int 值转换为 VuetifyCounterValueSource，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyCounterValueSource(int value)
        => new((Number)value);

    /// <summary>
    /// 将 uint 值转换为 VuetifyCounterValueSource，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyCounterValueSource(uint value)
        => new((Number)value);

    /// <summary>
    /// 将 float 值转换为 VuetifyCounterValueSource，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyCounterValueSource(float value)
        => new((Number)value);

    /// <summary>
    /// 将 double 值转换为 VuetifyCounterValueSource，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyCounterValueSource(double value)
        => new((Number)value);

    /// <summary>
    /// 将 decimal 值转换为 VuetifyCounterValueSource，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyCounterValueSource(decimal value)
        => new((Number)value);
}


/// <summary>
/// 用于 VInputComponentBase.ModelModifiers 的参数类型。
/// 文本输入的模型修饰符。
/// Model modifiers for text input.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VuetifyTextModelModifiers : VueProps
{
    /// <summary>
    /// 更新字符串模型前移除首尾空白。
    /// </summary>
    [Description("@#trim")]
    public bool? Trim { get; init; }

    /// <summary>
    /// 将可解析的输入文本转换为数值。
    /// </summary>
    [Description("@#number")]
    public bool? Number { get; init; }

    /// <summary>
    /// 使用 change 而非 input 更新原生输入模型。
    /// </summary>
    [Description("@#lazy")]
    public bool? Lazy { get; init; }
}
