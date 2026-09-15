namespace ECMAScript.ElementPlus;

// Defines Element Plus value domains, callback payloads, and erased union authoring contracts.
// 定义 Element Plus 值域、回调载荷与擦除 union；仅分支继承重叠时保留 tagged fallback。

/// <summary>
/// 用于 ElButton.Size、ElButtonGroup.Size、ElCascader.Size、ElCheckbox.Size、ElCheckboxGroup.Size、ElColorPicker.Size、ElConfigProvider.Size、ElInput.Size、ElInputNumber.Size、ElInputOtp.Size、ElInputTag.Size、ElPagination.Size、ElRadio.Size、ElRate.Size、ElSegmented.Size、ElTag.Size、ElText.Size、ElTimePicker.Size、ElTimeSelect.Size、ElInstallOptions.Size、ElButtonProps.Size 的可选取值。
/// button size
/// control the size of buttons in this button-group
/// size of input
/// size of the Checkbox
/// size of checkbox
/// size of ColorPicker
/// global component size
/// size of Input, works when `type` is not 'textarea'
/// size of the component
/// The size of the OTP fields
/// input box size
/// pagination size
/// size of the Radio
/// size of Rate
/// size of component
/// size of Tag
/// text size
/// size of Input
/// </summary>
[String]
public enum ElComponentSize
{
    /// <summary>
    /// 大号尺寸；上游取值为 “large”。
    /// </summary>
    [Description("@#large")]
    Large,

    /// <summary>
    /// 默认配置；上游取值为 “default”。
    /// </summary>
    [Description("@#default")]
    Default,

    /// <summary>
    /// 小号尺寸；上游取值为 “small”。
    /// </summary>
    [Description("@#small")]
    Small
}

/// <summary>
/// 用于 ElAlert.Effect、ElAvatarGroup.Effect、ElDropdown.Effect、ElPopover.Effect、ElSelect.Effect、ElTooltip.Effect、ElTreeSelect.Effect、ElVirtualizedSelect.Effect 的可选取值。
/// theme style.
/// tooltip theme, built-in theme: `dark` / `light`
/// Tooltip theme, built-in theme: `dark` / `light`
/// </summary>
[String]
public enum ElPopperEffect
{
    /// <summary>
    /// 深色外观；上游取值为 “dark”。
    /// </summary>
    [Description("@#dark")]
    Dark,

    /// <summary>
    /// 浅色外观；上游取值为 “light”。
    /// </summary>
    [Description("@#light")]
    Light
}

/// <summary>
/// 用于 ElAvatarGroup.Placement、ElDropdown.Placement、ElPopover.Placement、ElSelect.Placement、ElTooltip.Placement、ElTreeSelect.Placement、ElVirtualizedSelect.Placement 的可选取值。
/// placement of tooltip
/// placement of pop menu
/// popover placement
/// position of dropdown
/// Position of Tooltip
/// </summary>
[String]
public enum ElPopperPlacement
{
    /// <summary>
    /// 顶部；上游取值为 “top”。
    /// </summary>
    [Description("@#top")]
    Top,

    /// <summary>
    /// 上方并对齐逻辑起始端；上游取值为 “top-start”。
    /// </summary>
    [Description("@#top-start")]
    TopStart,

    /// <summary>
    /// 上方并对齐逻辑结束端；上游取值为 “top-end”。
    /// </summary>
    [Description("@#top-end")]
    TopEnd,

    /// <summary>
    /// 底部；上游取值为 “bottom”。
    /// </summary>
    [Description("@#bottom")]
    Bottom,

    /// <summary>
    /// 位于下方并对齐起始端；上游取值为 “bottom-start”。
    /// </summary>
    [Description("@#bottom-start")]
    BottomStart,

    /// <summary>
    /// 位于下方并对齐结束端；上游取值为 “bottom-end”。
    /// </summary>
    [Description("@#bottom-end")]
    BottomEnd,

    /// <summary>
    /// 左侧；上游取值为 “left”。
    /// </summary>
    [Description("@#left")]
    Left,

    /// <summary>
    /// 位于左侧并对齐起始端；上游取值为 “left-start”。
    /// </summary>
    [Description("@#left-start")]
    LeftStart,

    /// <summary>
    /// 位于左侧并对齐结束端；上游取值为 “left-end”。
    /// </summary>
    [Description("@#left-end")]
    LeftEnd,

    /// <summary>
    /// 右侧；上游取值为 “right”。
    /// </summary>
    [Description("@#right")]
    Right,

    /// <summary>
    /// 位于右侧并对齐起始端；上游取值为 “right-start”。
    /// </summary>
    [Description("@#right-start")]
    RightStart,

    /// <summary>
    /// 位于右侧并对齐结束端；上游取值为 “right-end”。
    /// </summary>
    [Description("@#right-end")]
    RightEnd,

    /// <summary>
    /// 自动选择；上游取值为 “auto”。
    /// </summary>
    [Description("@#auto")]
    Auto,

    /// <summary>
    /// 自动选择方向并对齐起始端；上游取值为 “auto-start”。
    /// </summary>
    [Description("@#auto-start")]
    AutoStart,

    /// <summary>
    /// 自动选择方向并对齐结束端；上游取值为 “auto-end”。
    /// </summary>
    [Description("@#auto-end")]
    AutoEnd
}

/// <summary>
/// 用于 ElTabs.TabPosition 的可选取值。
/// position of tabs
/// </summary>
[String]
public enum ElPopperPlacementSide
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
    /// 左侧；上游取值为 “left”。
    /// </summary>
    [Description("@#left")]
    Left,

    /// <summary>
    /// 右侧；上游取值为 “right”。
    /// </summary>
    [Description("@#right")]
    Right
}

/// <summary>
/// 用于 ElCard.Shadow、ElCardConfig.Shadow 的可选取值。
/// when to show card shadows
/// </summary>
[String]
public enum ElCardShadow
{
    /// <summary>
    /// 始终显示卡片阴影；上游取值为 “always”。
    /// </summary>
    [Description("@#always")]
    Always,

    /// <summary>
    /// 仅在悬停时显示卡片阴影；上游取值为 “hover”。
    /// </summary>
    [Description("@#hover")]
    Hover,

    /// <summary>
    /// 不显示卡片阴影；上游取值为 “never”。
    /// </summary>
    [Description("@#never")]
    Never
}

/// <summary>
/// 上传文件的生命周期状态：待上传、上传中、成功或失败。
/// </summary>
[String]
public enum ElUploadStatus
{
    /// <summary>
    /// 等待上传；上游取值为 “ready”。
    /// </summary>
    [Description("@#ready")]
    Ready,

    /// <summary>
    /// 正在上传；上游取值为 “uploading”。
    /// </summary>
    [Description("@#uploading")]
    Uploading,

    /// <summary>
    /// 成功状态样式；上游取值为 “success”。
    /// </summary>
    [Description("@#success")]
    Success,

    /// <summary>
    /// 上传失败；上游取值为 “fail”。
    /// </summary>
    [Description("@#fail")]
    Fail
}

/// <summary>
/// 用于 ElCarousel.Trigger、ElMenu.MenuTrigger 的可选取值。
/// how indicators are triggered
/// how sub-menus are triggered, only works when `mode` is 'horizontal'
/// </summary>
[String]
public enum ElHoverClickTrigger
{
    /// <summary>
    /// 指针悬停触发；上游取值为 “hover”。
    /// </summary>
    [Description("@#hover")]
    Hover,

    /// <summary>
    /// 单击触发；上游取值为 “click”。
    /// </summary>
    [Description("@#click")]
    Click
}

/// <summary>
/// 用于 ElImage.Crossorigin、ElUpload.Crossorigin 的可选取值。
/// native attribute [crossorigin](https://developer.mozilla.org/en-US/docs/Web/HTML/Attributes/crossorigin).
/// </summary>
[String]
public enum ElCrossorigin
{
    /// <summary>
    /// 传入空字符串，使用组件对此属性的默认或空值行为；上游取值为 “”。
    /// </summary>
    [Description("@#")]
    Empty,

    /// <summary>
    /// 匿名跨源资源请求，同源情况下保留凭据；上游取值为 “anonymous”。
    /// </summary>
    [Description("@#anonymous")]
    Anonymous,

    /// <summary>
    /// 跨源资源请求携带凭据；上游取值为 “use-credentials”。
    /// </summary>
    [Description("@#use-credentials")]
    UseCredentials
}

/// <summary>
/// 用于 ElUpload.ListType 的可选取值。
/// type of file list.
/// </summary>
[String]
public enum ElUploadListType
{
    /// <summary>
    /// 使用文本文件列表展示上传项；上游取值为 “text”。
    /// </summary>
    [Description("@#text")]
    Text,

    /// <summary>
    /// 图片列表；上游取值为 “picture”。
    /// </summary>
    [Description("@#picture")]
    Picture,

    /// <summary>
    /// 图片卡片列表；上游取值为 “picture-card”。
    /// </summary>
    [Description("@#picture-card")]
    PictureCard
}

/// <summary>
/// 用于 ElImage.Fit 的可选取值。
/// indicate how the image should be resized to fit its container, same as [object-fit](https://developer.mozilla.org/en-US/docs/Web/CSS/object-fit).
/// </summary>
[String]
public enum ElImageFitType
{
    /// <summary>
    /// 传入空字符串，使用组件对此属性的默认或空值行为；上游取值为 “”。
    /// </summary>
    [Description("@#")]
    Empty,

    /// <summary>
    /// 保持宽高比并完整显示图片；上游取值为 “contain”。
    /// </summary>
    [Description("@#contain")]
    Contain,

    /// <summary>
    /// 保持宽高比填满容器，必要时裁剪；上游取值为 “cover”。
    /// </summary>
    [Description("@#cover")]
    Cover,

    /// <summary>
    /// 拉伸内容填满容器；上游取值为 “fill”。
    /// </summary>
    [Description("@#fill")]
    Fill,

    /// <summary>
    /// 保持图片原始尺寸，不进行适配缩放；上游取值为 “none”。
    /// </summary>
    [Description("@#none")]
    None,

    /// <summary>
    /// 在原始尺寸与 contain 中选择较小尺寸；上游取值为 “scale-down”。
    /// </summary>
    [Description("@#scale-down")]
    ScaleDown
}

/// <summary>
/// 用于 ElImage.Loading 的可选取值。
/// Indicates how the browser should load the image, same as [native](https://developer.mozilla.org/en-US/docs/Web/HTML/Element/img#attr-loading).
/// </summary>
[String]
public enum ElImageLoadingType
{
    /// <summary>
    /// 立即加载；上游取值为 “eager”。
    /// </summary>
    [Description("@#eager")]
    Eager,

    /// <summary>
    /// 延迟到图片接近视口时加载；上游取值为 “lazy”。
    /// </summary>
    [Description("@#lazy")]
    Lazy
}

/// <summary>
/// 用于 ElAvatar.Shape、ElAvatarGroup.Shape 的可选取值。
/// avatar shape.
/// control the shape of avatars in this avatar-group
/// </summary>
[String]
public enum ElAvatarShape
{
    /// <summary>
    /// 圆形；上游取值为 “circle”。
    /// </summary>
    [Description("@#circle")]
    Circle,

    /// <summary>
    /// 正方形；上游取值为 “square”。
    /// </summary>
    [Description("@#square")]
    Square
}

/// <summary>
/// 用于 ElButton.Type、ElButtonGroup.Type、ElDropdown.Type、ElButtonConfig.Type、ElButtonProps.Type 的可选取值。
/// button type, when setting `color`, the latter prevails
/// control the type of buttons in this button-group
/// menu button type, refer to `Button` Component, only works when `split-button` is true
/// </summary>
[String]
public enum ElButtonType
{
    /// <summary>
    /// 传入空字符串，使用组件对此属性的默认或空值行为；上游取值为 “”。
    /// </summary>
    [Description("@#")]
    Empty,

    /// <summary>
    /// 默认配置；上游取值为 “default”。
    /// </summary>
    [Description("@#default")]
    Default,

    /// <summary>
    /// 主要操作的主题色；上游取值为 “primary”。
    /// </summary>
    [Description("@#primary")]
    Primary,

    /// <summary>
    /// 成功状态样式；上游取值为 “success”。
    /// </summary>
    [Description("@#success")]
    Success,

    /// <summary>
    /// 警告状态样式；上游取值为 “warning”。
    /// </summary>
    [Description("@#warning")]
    Warning,

    /// <summary>
    /// 信息提示样式；上游取值为 “info”。
    /// </summary>
    [Description("@#info")]
    Info,

    /// <summary>
    /// 危险操作样式；上游取值为 “danger”。
    /// </summary>
    [Description("@#danger")]
    Danger,

    /// <summary>
    /// 文本样式；上游取值为 “text”。
    /// </summary>
    [Description("@#text")]
    Text
}

/// <summary>
/// 用于 ElButton.NativeType、ElButtonProps.NativeType 的可选取值。
/// same as native button's `type`
/// </summary>
[String]
public enum ElButtonNativeType
{
    /// <summary>
    /// 普通按钮，不触发表单提交；上游取值为 “button”。
    /// </summary>
    [Description("@#button")]
    Button,

    /// <summary>
    /// 提交表单；上游取值为 “submit”。
    /// </summary>
    [Description("@#submit")]
    Submit,

    /// <summary>
    /// 重置表单；上游取值为 “reset”。
    /// </summary>
    [Description("@#reset")]
    Reset
}

/// <summary>
/// 用于 ElButtonGroup.Direction、ElCarousel.Direction、ElContainer.Direction、ElDescriptions.Direction、ElDivider.Direction、ElSegmented.Direction、ElSpace.Direction、ElSteps.Direction 的可选取值。
/// display direction
/// layout direction for child elements
/// direction of list
/// Set divider's direction
/// Placement direction
/// </summary>
[String]
public enum ElDirection
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
/// 用于 ElAffix.Position、ElTimelineItem.Placement 的可选取值。
/// position of affix
/// position of timestamp
/// </summary>
[String]
public enum ElTopBottomPlacement
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
/// 用于 ElCarousel.Type 的可选取值。
/// type of the Carousel
/// </summary>
[String]
public enum ElCarouselType
{
    /// <summary>
    /// 传入空字符串，使用组件对此属性的默认或空值行为；上游取值为 “”。
    /// </summary>
    [Description("@#")]
    Empty,

    /// <summary>
    /// 卡片样式；上游取值为 “card”。
    /// </summary>
    [Description("@#card")]
    Card
}

/// <summary>
/// 用于 ElText.Type、ElTimelineItem.Type 的可选取值。
/// text type
/// node type
/// </summary>
[String]
public enum ElSemanticType
{
    /// <summary>
    /// 传入空字符串，使用组件对此属性的默认或空值行为；上游取值为 “”。
    /// </summary>
    [Description("@#")]
    Empty,

    /// <summary>
    /// 主要操作的主题色；上游取值为 “primary”。
    /// </summary>
    [Description("@#primary")]
    Primary,

    /// <summary>
    /// 成功状态样式；上游取值为 “success”。
    /// </summary>
    [Description("@#success")]
    Success,

    /// <summary>
    /// 警告状态样式；上游取值为 “warning”。
    /// </summary>
    [Description("@#warning")]
    Warning,

    /// <summary>
    /// 信息提示样式；上游取值为 “info”。
    /// </summary>
    [Description("@#info")]
    Info,

    /// <summary>
    /// 危险操作样式；上游取值为 “danger”。
    /// </summary>
    [Description("@#danger")]
    Danger
}

/// <summary>
/// 用于 ElTimeline.Mode 的可选取值。
/// relative position of timeline and content
/// </summary>
[String]
public enum ElTimelineMode
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
    /// 时间线内容在两侧交替显示；上游取值为 “alternate”。
    /// </summary>
    [Description("@#alternate")]
    Alternate,

    /// <summary>
    /// 以相反起始侧交替显示时间线内容；上游取值为 “alternate-reverse”。
    /// </summary>
    [Description("@#alternate-reverse")]
    AlternateReverse
}

/// <summary>
/// 用于 ElCalendar.ControllerType 的可选取值。
/// type of the controller for Calendar header
/// </summary>
[String]
public enum ElCalendarControllerType
{
    /// <summary>
    /// 使用按钮切换日历月份；上游取值为 “button”。
    /// </summary>
    [Description("@#button")]
    Button,

    /// <summary>
    /// 使用下拉选择器切换日历年月；上游取值为 “select”。
    /// </summary>
    [Description("@#select")]
    Select
}

/// <summary>
/// 用于 ElCollapse.ExpandIconPosition 的可选取值。
/// set expand icon position
/// </summary>
[String]
public enum ElCollapseIconPosition
{
    /// <summary>
    /// 左侧；上游取值为 “left”。
    /// </summary>
    [Description("@#left")]
    Left,

    /// <summary>
    /// 右侧；上游取值为 “right”。
    /// </summary>
    [Description("@#right")]
    Right
}

/// <summary>
/// 用于 ElDivider.ContentPosition 的可选取值。
/// The position of the customized content on the divider line
/// </summary>
[String]
public enum ElContentPosition
{
    /// <summary>
    /// 左侧；上游取值为 “left”。
    /// </summary>
    [Description("@#left")]
    Left,

    /// <summary>
    /// 居中；上游取值为 “center”。
    /// </summary>
    [Description("@#center")]
    Center,

    /// <summary>
    /// 右侧；上游取值为 “right”。
    /// </summary>
    [Description("@#right")]
    Right
}

/// <summary>
/// 用于 ElFormItem.ValidateStatus 的可选取值。
/// Validation state of formItem.
/// </summary>
[String]
public enum ElFormItemValidateStatus
{
    /// <summary>
    /// 传入空字符串，使用组件对此属性的默认或空值行为；上游取值为 “”。
    /// </summary>
    [Description("@#")]
    Empty,

    /// <summary>
    /// 错误状态样式；上游取值为 “error”。
    /// </summary>
    [Description("@#error")]
    Error,

    /// <summary>
    /// 正在进行校验；上游取值为 “validating”。
    /// </summary>
    [Description("@#validating")]
    Validating,

    /// <summary>
    /// 成功状态样式；上游取值为 “success”。
    /// </summary>
    [Description("@#success")]
    Success
}

/// <summary>
/// 用于 ElProgress.Type 的可选取值。
/// the type of progress bar
/// </summary>
[String]
public enum ElProgressType
{
    /// <summary>
    /// 线条形式；上游取值为 “line”。
    /// </summary>
    [Description("@#line")]
    Line,

    /// <summary>
    /// 使用完整环形进度条；上游取值为 “circle”。
    /// </summary>
    [Description("@#circle")]
    Circle,

    /// <summary>
    /// 仪表盘形式；上游取值为 “dashboard”。
    /// </summary>
    [Description("@#dashboard")]
    Dashboard
}

/// <summary>
/// 用于 ElProgress.Status 的可选取值。
/// the current status of progress bar
/// </summary>
[String]
public enum ElProgressStatus
{
    /// <summary>
    /// 传入空字符串，使用组件对此属性的默认或空值行为；上游取值为 “”。
    /// </summary>
    [Description("@#")]
    Empty,

    /// <summary>
    /// 成功状态样式；上游取值为 “success”。
    /// </summary>
    [Description("@#success")]
    Success,

    /// <summary>
    /// 异常状态；上游取值为 “exception”。
    /// </summary>
    [Description("@#exception")]
    Exception,

    /// <summary>
    /// 警告状态样式；上游取值为 “warning”。
    /// </summary>
    [Description("@#warning")]
    Warning
}

/// <summary>
/// 用于 ElStep.Status、ElSteps.ProcessStatus、ElSteps.FinishStatus 的可选取值。
/// current status. It will be automatically set by Steps if not configured.
/// status of current step
/// status of end step
/// </summary>
[String]
public enum ElStepStatus
{
    /// <summary>
    /// 传入空字符串，使用组件对此属性的默认或空值行为；上游取值为 “”。
    /// </summary>
    [Description("@#")]
    Empty,

    /// <summary>
    /// 尚未开始的步骤；上游取值为 “wait”。
    /// </summary>
    [Description("@#wait")]
    Wait,

    /// <summary>
    /// 正在进行的步骤；上游取值为 “process”。
    /// </summary>
    [Description("@#process")]
    Process,

    /// <summary>
    /// 已完成的步骤；上游取值为 “finish”。
    /// </summary>
    [Description("@#finish")]
    Finish,

    /// <summary>
    /// 错误状态样式；上游取值为 “error”。
    /// </summary>
    [Description("@#error")]
    Error,

    /// <summary>
    /// 成功状态样式；上游取值为 “success”。
    /// </summary>
    [Description("@#success")]
    Success
}

/// <summary>
/// 用于 ElTabs.Type 的可选取值。
/// type of Tab
/// </summary>
[String]
public enum ElTabsType
{
    /// <summary>
    /// 传入空字符串，使用组件对此属性的默认或空值行为；上游取值为 “”。
    /// </summary>
    [Description("@#")]
    Empty,

    /// <summary>
    /// 卡片样式；上游取值为 “card”。
    /// </summary>
    [Description("@#card")]
    Card,

    /// <summary>
    /// 带边框的卡片样式；上游取值为 “border-card”。
    /// </summary>
    [Description("@#border-card")]
    BorderCard
}

/// <summary>
/// 用于 ElSelect.TagType、ElTag.Type、ElTreeSelect.TagType、ElVirtualizedSelect.TagType 的可选取值。
/// tag type
/// type of Tag
/// </summary>
[String]
public enum ElTagType
{
    /// <summary>
    /// 主要操作的主题色；上游取值为 “primary”。
    /// </summary>
    [Description("@#primary")]
    Primary,

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
    /// 危险操作样式；上游取值为 “danger”。
    /// </summary>
    [Description("@#danger")]
    Danger
}

/// <summary>
/// 用于 ElSelect.TagEffect、ElTag.Effect、ElTreeSelect.TagEffect、ElVirtualizedSelect.TagEffect 的可选取值。
/// tag effect
/// theme of Tag
/// </summary>
[String]
public enum ElTagEffect
{
    /// <summary>
    /// 深色外观；上游取值为 “dark”。
    /// </summary>
    [Description("@#dark")]
    Dark,

    /// <summary>
    /// 浅色外观；上游取值为 “light”。
    /// </summary>
    [Description("@#light")]
    Light,

    /// <summary>
    /// 无强调装饰的样式；上游取值为 “plain”。
    /// </summary>
    [Description("@#plain")]
    Plain
}

/// <summary>
/// 用于 ElLinkConfig.Type 的可选取值。
/// type
/// </summary>
[String]
public enum ElLinkType
{
    /// <summary>
    /// 默认配置；上游取值为 “default”。
    /// </summary>
    [Description("@#default")]
    Default,

    /// <summary>
    /// 主要操作的主题色；上游取值为 “primary”。
    /// </summary>
    [Description("@#primary")]
    Primary,

    /// <summary>
    /// 成功状态样式；上游取值为 “success”。
    /// </summary>
    [Description("@#success")]
    Success,

    /// <summary>
    /// 警告状态样式；上游取值为 “warning”。
    /// </summary>
    [Description("@#warning")]
    Warning,

    /// <summary>
    /// 信息提示样式；上游取值为 “info”。
    /// </summary>
    [Description("@#info")]
    Info,

    /// <summary>
    /// 危险操作样式；上游取值为 “danger”。
    /// </summary>
    [Description("@#danger")]
    Danger
}

/// <summary>
/// 以 CSS 属性名为键的样式对象，用于 Element Plus 的内联样式参数。
/// </summary>
[ECMAScript]
[Description("@#Styles")]
public sealed record ElStyles : VueDictionary<VueStringNumberValue>
{
}

/// <summary>
/// C# 联合参数，允许 bool, VueProps。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取相应分支。
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union ElDirectiveValue(bool, VueProps)
{
    /// <summary>
    /// 读取当前值的 bool 分支；不属于该分支时返回 null。
    /// </summary>
    public bool? AsBool => Value is bool value ? value : default(bool?);

    /// <summary>
    /// 读取当前值的 VueProps 分支；不属于该分支时返回 null。
    /// </summary>
    public VueProps? AsProps => Value as VueProps;

    /// <summary>
    /// 将 VueDictionary 值转换为 ElDirectiveValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator ElDirectiveValue(VueDictionary value) => (VueProps)value;
}

/// <summary>
/// Loading 服务的遮罩配置，包括目标区域、图标、提示文字和滚动锁定。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record ElLoadingOptions : VueProps
{
    /// <summary>
    /// the DOM node Loading needs to cover. Accepts a DOM object or a string. If it's a string, it will be passed to `document.querySelector` to get the corresponding DOM node
    /// Default: document.body
    /// </summary>
    [Description("@#target")]
    public VueTeleportTarget? Target { get; init; }

    /// <summary>
    /// same as the `body` modifier of `v-loading`
    /// Default: false
    /// </summary>
    [Description("@#body")]
    public bool? Body { get; init; }

    /// <summary>
    /// same as the `lock` modifier of `v-loading`
    /// Default: false
    /// </summary>
    [Description("@#lock")]
    public bool? Lock { get; init; }

    /// <summary>
    /// loading text that displays under the spinner
    /// </summary>
    [Description("@#text")]
    public string? Text { get; init; }

    /// <summary>
    /// class name of the custom spinner
    /// </summary>
    [Description("@#spinner")]
    public string? Spinner { get; init; }

    /// <summary>
    /// custom SVG element to override the default loading spinner
    /// </summary>
    [Description("@#svg")]
    public string? Svg { get; init; }

    /// <summary>
    /// sets the viewBox attribute for loading svg element
    /// </summary>
    [Description("@#svgViewBox")]
    public string? SvgViewBox { get; init; }

    /// <summary>
    /// background color of the mask
    /// </summary>
    [Description("@#background")]
    public string? Background { get; init; }

    /// <summary>
    /// custom class name for loading
    /// </summary>
    [Description("@#customClass")]
    public string? CustomClass { get; init; }

    /// <summary>
    /// same as the `fullscreen` modifier of `v-loading`
    /// Default: true
    /// </summary>
    [Description("@#fullscreen")]
    public bool? Fullscreen { get; init; }
}

/// <summary>
/// 用于 ElConfigProvider.Button 的参数类型。
/// button related configuration, [see the following table](#button-attribute)
/// </summary>
[ECMAScript]
[Description("@#ButtonConfigContext")]
public sealed record ElButtonConfig : VueProps
{
    /// <summary>
    /// automatically insert a space between two chinese characters(this will only take effect when the text length is 2 and all characters are in Chinese.)
    /// </summary>
    [Description("@#autoInsertSpace")]
    public bool? AutoInsertSpace { get; init; }

    /// <summary>
    /// button type, when setting `color`, the latter prevails
    /// </summary>
    [Description("@#type")]
    public ElButtonType? Type { get; init; }

    /// <summary>
    /// determine whether it's a plain button
    /// </summary>
    [Description("@#plain")]
    public bool? Plain { get; init; }

    /// <summary>
    /// determine whether it's a text button
    /// </summary>
    [Description("@#text")]
    public bool? Text { get; init; }

    /// <summary>
    /// determine whether it's a round button
    /// </summary>
    [Description("@#round")]
    public bool? Round { get; init; }

    /// <summary>
    /// determine whether it's a dashed button
    /// </summary>
    [Description("@#dashed")]
    public bool? Dashed { get; init; }
}

/// <summary>
/// ConfigProvider 作用域内 Card 组件的默认配置。
/// </summary>
[ECMAScript]
[Description("@#CardConfigContext")]
public sealed record ElCardConfig : VueProps
{
    /// <summary>
    /// when to show card shadows
    /// </summary>
    [Description("@#shadow")]
    public ElCardShadow? Shadow { get; init; }
}

/// <summary>
/// 用于 ElMention.Options 的参数类型。
/// mention options list
/// </summary>
[ECMAScript]
[Description("@#MentionOption")]
public sealed record ElMentionOption : VueDictionary
{
    /// <summary>
    /// 选择此项时写入模型的值。
    /// </summary>
    [Description("@#value")]
    public string? Value { get; init; }

    /// <summary>
    /// 当前选项或单元格显示的标签文本。
    /// </summary>
    [Description("@#label")]
    public string? Label { get; init; }

    /// <summary>
    /// 是否禁止选择此项。
    /// </summary>
    [Description("@#disabled")]
    public bool? Disabled { get; init; }
}

/// <summary>
/// 用于 ElConfigProvider.Dialog 的参数类型。
/// dialog related configuration, [see the following table](#dialog-attribute)
/// </summary>
[ECMAScript]
[Description("@#DialogConfigContext")]
public sealed record ElDialogConfig : VueProps
{
    /// <summary>
    /// whether to align the dialog both horizontally and vertically
    /// </summary>
    [Description("@#alignCenter")]
    public bool? AlignCenter { get; init; }

    /// <summary>
    /// enable dragging feature for Dialog
    /// </summary>
    [Description("@#draggable")]
    public bool? Draggable { get; init; }

    /// <summary>
    /// draggable Dialog can overflow the viewport
    /// </summary>
    [Description("@#overflow")]
    public bool? Overflow { get; init; }

    /// <summary>
    /// custom transition configuration for dialog animation. Can be a string (transition name) or an object with Vue transition props
    /// </summary>
    [Description("@#transition")]
    public VueTransitionValue? Transition { get; init; }
}

/// <summary>
/// 用于 ElConfigProvider.Link 的参数类型。
/// link related configuration, [see the following table](#link-attribute)
/// </summary>
[ECMAScript]
[Description("@#LinkConfigContext")]
public sealed record ElLinkConfig : VueProps
{
    /// <summary>
    /// when underlines should appear
    /// </summary>
    [Description("@#underline")]
    public VueBooleanStringValue? Underline { get; init; }

    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public ElLinkType? Type { get; init; }
}

/// <summary>
/// 用于 ElConfigProvider.Message 的参数类型。
/// message related configuration, [see the following table](#message-attribute)
/// </summary>
[ECMAScript]
[Description("@#MessageConfigContext")]
public sealed record ElMessageConfig : VueProps
{
    /// <summary>
    /// 同时显示的 Message 最大数量。
    /// </summary>
    [Description("@#max")]
    public Number? Max { get; init; }

    /// <summary>
    /// 合并内容相同的消息并显示重复次数。
    /// </summary>
    [Description("@#grouping")]
    public bool? Grouping { get; init; }

    /// <summary>
    /// 消息自动关闭前等待的毫秒数；0 表示不自动关闭。
    /// </summary>
    [Description("@#duration")]
    public Number? Duration { get; init; }

    /// <summary>
    /// 消息距视口顶部的像素偏移。
    /// </summary>
    [Description("@#offset")]
    public Number? Offset { get; init; }
}

/// <summary>
/// 用于 ElConfigProvider.Table 的参数类型。
/// table related configuration, [see the following table](#table-attribute)
/// </summary>
[ECMAScript]
[Description("@#TableConfigContext")]
public sealed record ElTableConfig : VueProps
{
    /// <summary>
    /// whether to hide extra content and show them in a tooltip when hovering on the cell.It will affect all the table columns, refer to table [tooltip-options](#table-attributes)
    /// </summary>
    [Description("@#showOverflowTooltip")]
    public ElTableOverflowTooltipValue? ShowOverflowTooltip { get; init; }

    /// <summary>
    /// the `effect` of the overflow tooltip
    /// </summary>
    [Description("@#tooltipEffect")]
    public string? TooltipEffect { get; init; }

    /// <summary>
    /// the options for the overflow tooltip, [see the following tooltip component](tooltip.html#attributes)
    /// </summary>
    [Description("@#tooltipOptions")]
    public ElTableOverflowTooltipOptions? TooltipOptions { get; init; }

    /// <summary>
    /// customize tooltip content when using `show-overflow-tooltip`
    /// </summary>
    [Description("@#tooltipFormatter")]
    public ElTableTooltipFormatter? TooltipFormatter { get; init; }
}

/// <summary>
/// 可嵌套的本地化文本映射，键对应语言包中的消息路径。
/// </summary>
[ECMAScript]
[Description("@#TranslatePair")]
public sealed record ElTranslatePair : VueDictionary<ElTranslateValue>
{
}

/// <summary>
/// C# 联合参数，允许 string, string[], ElTranslatePair。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取相应分支。
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union ElTranslateValue(string, string[], ElTranslatePair)
{
    /// <summary>
    /// 读取当前值的 string 分支；不属于该分支时返回 null。
    /// </summary>
    public string? AsString => Value as string;

    /// <summary>
    /// 读取当前值的 string[] 分支；不属于该分支时返回 null。
    /// </summary>
    public string[]? AsStrings => Value as string[];

    /// <summary>
    /// 读取当前值的 ElTranslatePair 分支；不属于该分支时返回 null。
    /// </summary>
    public ElTranslatePair? AsPair => Value as ElTranslatePair;
}

/// <summary>
/// 用于 ElConfigProvider.Locale 的参数类型。
/// Locale Object
/// </summary>
[ECMAScript]
[Description("@#Language")]
public sealed record ElLanguage : VueProps
{
    /// <summary>
    /// 语言包名称或 locale 标识。
    /// </summary>
    [Description("@#name")]
    public string? Name { get; init; }

    /// <summary>
    /// Element Plus 组件使用的本地化文本映射。
    /// </summary>
    [Description("@#el")]
    public ElTranslatePair? El { get; init; }
}

/// <summary>
/// 用于 ElCascader.ValueOnClear、ElColorPicker.ValueOnClear、ElConfigProvider.ValueOnClear、ElDatePicker.ValueOnClear、ElInputNumber.ValueOnClear、ElSelect.ValueOnClear、ElTimePicker.ValueOnClear、ElTimeSelect.ValueOnClear、ElTreeSelect.ValueOnClear、ElVirtualizedSelect.ValueOnClear 的参数类型。
/// clear return value, [see config-provider](./config-provider.md#empty-values-configurations)
/// global clear return value
/// value should be set when input box is cleared
/// </summary>
[ECMAScript]
[Description("@#ValueOnClear")]
public readonly union ElValueOnClearValue(bool, double, string, ElValueOnClearCallback)
{
    /// <summary>
    /// 读取当前值的 bool 分支；不属于该分支时返回 null。
    /// </summary>
    public bool? AsBool => Value is bool value ? value : default(bool?);

    /// <summary>
    /// 读取当前值的 double 分支；不属于该分支时返回 null。
    /// </summary>
    public double? AsNumber => Value is double value ? value : default(double?);

    /// <summary>
    /// 读取当前值的 string 分支；不属于该分支时返回 null。
    /// </summary>
    public string? AsString => Value as string;

    /// <summary>
    /// 读取当前值的 ElValueOnClearCallback 分支；不属于该分支时返回 null。
    /// </summary>
    public ElValueOnClearCallback? AsCallback => Value as ElValueOnClearCallback;

    /// <summary>
    /// 构造显式的 JavaScript null 分支，用于向宿主 API 传入空值。
    /// </summary>
    [ECMAScriptInline("null")]
    public extern static ElValueOnClearValue Null();
}

/// <summary>
/// 以字符串键控制启用状态的映射，常用于 CSS 类名条件。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record ElStringBooleanMap : VueDictionary<bool>
{
}

/// <summary>
/// C# 联合参数，允许 string, ElStringBooleanMap。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取相应分支。
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union ElStringBooleanClassValue(string, ElStringBooleanMap)
{
    /// <summary>
    /// 读取当前值的 string 分支；不属于该分支时返回 null。
    /// </summary>
    public string? AsString => Value as string;

    /// <summary>
    /// 读取当前值的 ElStringBooleanMap 分支；不属于该分支时返回 null。
    /// </summary>
    public ElStringBooleanMap? AsMap => Value as ElStringBooleanMap;
}

/// <summary>
/// 自动补全候选项的原始数据对象；显示字段由 value-key 指定。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record ElAutocompleteSuggestionItem : VueDictionary
{
}

/// <summary>
/// 容器尺寸变化回调的数据，包含最新可用宽度与高度。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record ElAutoResizerResizeContext : VueProps
{
    /// <summary>
    /// 容器可用高度，单位为像素。
    /// </summary>
    [Description("@#height")]
    public Number? Height { get; init; }

    /// <summary>
    /// 当前列或可用区域的宽度，单位为像素。
    /// </summary>
    [Description("@#width")]
    public Number? Width { get; init; }
}

/// <summary>
/// 容器尺寸变化后接收最新宽度和高度，用于同步子组件的布局。
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate void ElAutoResizerResizeCallback(ElAutoResizerResizeContext context);

/// <summary>
/// 将查询完成的候选项数组交回自动补全组件，以更新建议列表。
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate void ElAutocompleteFetchSuggestionsCallback(ElAutocompleteSuggestionItem[] data);

/// <summary>
/// 用于 ElAutocompleteFetchSuggestionsValue.AsAsyncCallback 的回调签名。
/// 读取当前值的 ElAutocompleteFetchSuggestionsAsyncCallback 分支；不属于该分支时返回 null。
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate IPromise<ElAutocompleteSuggestionItem[]?> ElAutocompleteFetchSuggestionsAsyncCallback(
    string queryString,
    ElAutocompleteFetchSuggestionsCallback callback);

/// <summary>
/// 用于 ElAutocompleteFetchSuggestionsValue.AsCallback 的回调签名。
/// 读取当前值的 ElAutocompleteFetchSuggestionsCallbackOnly 分支；不属于该分支时返回 null。
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate void ElAutocompleteFetchSuggestionsCallbackOnly(string queryString, ElAutocompleteFetchSuggestionsCallback callback);

/// <summary>
/// 用于 ElAutocomplete.FetchSuggestions 的参数类型。
/// a method to fetch input suggestions. When suggestions are ready, invoke `callback(data:[])` to return them to Autocomplete
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union ElAutocompleteFetchSuggestionsValue(
    ElAutocompleteSuggestionItem[],
    ElAutocompleteFetchSuggestionsCallbackOnly,
    ElAutocompleteFetchSuggestionsAsyncCallback)
{
    /// <summary>
    /// 读取当前值的 ElAutocompleteSuggestionItem[] 分支；不属于该分支时返回 null。
    /// </summary>
    public ElAutocompleteSuggestionItem[]? AsSuggestions => Value as ElAutocompleteSuggestionItem[];

    /// <summary>
    /// 读取当前值的 ElAutocompleteFetchSuggestionsCallbackOnly 分支；不属于该分支时返回 null。
    /// </summary>
    public ElAutocompleteFetchSuggestionsCallbackOnly? AsCallback => Value as ElAutocompleteFetchSuggestionsCallbackOnly;

    /// <summary>
    /// 读取当前值的 ElAutocompleteFetchSuggestionsAsyncCallback 分支；不属于该分支时返回 null。
    /// </summary>
    public ElAutocompleteFetchSuggestionsAsyncCallback? AsAsyncCallback => Value as ElAutocompleteFetchSuggestionsAsyncCallback;
}

/// <summary>
/// 日历日期单元格的上下文，包含日期、月份类别和选择状态。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record ElCalendarDateCellContext : VueProps
{
    /// <summary>
    /// 当前日历单元格对应的 JavaScript Date。
    /// </summary>
    [Description("@#date")]
    public Date? Date { get; init; }

    /// <summary>
    /// 日期所属月份类别，如 prev-month、current-month、next-month。
    /// </summary>
    [Description("@#type")]
    public string? Type { get; init; }

    /// <summary>
    /// 当前日历单元格的日期字符串。
    /// </summary>
    [Description("@#day")]
    public string? Day { get; init; }

    /// <summary>
    /// 当前日期是否被选中。
    /// </summary>
    [Description("@#isSelected")]
    public bool? IsSelected { get; init; }
}

/// <summary>
/// 用于 ElCalendar.Formatter 的回调签名。
/// format label when `controller-type` is 'select'
/// </summary>
/// <param name="value">要传入的值，保持其声明的类型和数据。</param>
/// <param name="type">正在格式化的日期部分，决定返回日历标题中的年份或月份文本。</param>
[ECMAScript]
[Description("@#")]
public delegate VueStringNumberValue ElCalendarFormatterCallback(Number value, string type);

/// <summary>
/// 同步返回是否允许继续当前操作；返回 false 可阻止对应操作。
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate bool ElAsyncBooleanCallback();

/// <summary>
/// 异步决定是否继续当前操作；Promise 完成后的布尔值交给对应组件处理。
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate IPromise<bool?> ElAsyncBooleanPromiseCallback();

/// <summary>
/// C# 联合参数，允许 bool, IPromise&lt;bool?&gt;。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取相应分支。
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union ElAsyncBooleanResult(bool, IPromise<bool?>)
{
    /// <summary>
    /// 读取当前值的 bool 分支；不属于该分支时返回 null。
    /// </summary>
    public bool? AsBool => Value is bool value ? value : default(bool?);

    /// <summary>
    /// 读取当前值的 IPromise&lt;bool?&gt; 分支；不属于该分支时返回 null。
    /// </summary>
    public IPromise<bool?>? AsPromise => Value as IPromise<bool?>;
}

/// <summary>
/// 结束关闭前拦截并执行对话框关闭；cancel 为 true 时保留对话框。
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate void ElDialogDoneCallback(bool? cancel = null);

/// <summary>
/// 用于 ElDialog.BeforeClose、ElDrawer.BeforeClose 的回调签名。
/// callback before Dialog closes, and it will prevent Dialog from closing, use done to close the dialog
/// If set, closing procedure will be halted
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate void ElDialogBeforeCloseCallback(ElDialogDoneCallback done);

/// <summary>
/// 用于 ElInput.Formatter、ElInputNumber.Formatter 的回调签名。
/// specifies the format of the value presented input.(only works when `type` is 'text')
/// specifies the format of the value presented in the input
/// </summary>
/// <param name="value">要传入的值，保持其声明的类型和数据。</param>
[ECMAScript]
[Description("@#")]
public delegate string ElInputFormatter(string value);

/// <summary>
/// 用于 ElInput.Parser、ElInputNumber.Parser 的回调签名。
/// specifies the value extracted from formatter input.(only works when `type` is 'text')
/// specifies the value extracted from the formatted input
/// </summary>
/// <param name="value">要传入的值，保持其声明的类型和数据。</param>
[ECMAScript]
[Description("@#")]
public delegate string ElInputParser(string value);

/// <summary>
/// 用于 ElInput.CountGraphemes 的回调签名。
/// custom function to count graphemes; when set, native `maxlength`/`minlength` constraints are bypassed. Component uses `Intl.Segmenter` (Chrome 87+, Firefox 125+, Safari 14.1+) for proper grapheme clustering; older browsers fall back to `Array.from()` for code-point iteration
/// </summary>
/// <param name="value">要传入的值，保持其声明的类型和数据。</param>
[ECMAScript]
[Description("@#")]
public delegate Number ElInputCountGraphemes(string value);

/// <summary>
/// 用于 ElInputOtp.Validator 的回调签名。
/// Custom validator function
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate bool ElInputOtpValidator(string @char, Number index);

/// <summary>
/// 用于 ElInputOtpSeparatorValue.AsRenderer 的回调签名。
/// 读取当前值的 ElInputOtpSeparatorRenderer 分支；不属于该分支时返回 null。
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate VueStringNumberVNodeValue ElInputOtpSeparatorRenderer(Number index);

/// <summary>
/// 用于 ElInputOtp.Separator 的参数类型。
/// The separator between OTP fields
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union ElInputOtpSeparatorValue(string, IVNode, ElInputOtpSeparatorRenderer)
{
    /// <summary>
    /// 读取当前值的 string 分支；不属于该分支时返回 null。
    /// </summary>
    public string? AsString => Value as string;

    /// <summary>
    /// 读取当前值的 IVNode 分支；不属于该分支时返回 null。
    /// </summary>
    public IVNode? AsVNode => Value as IVNode;

    /// <summary>
    /// 读取当前值的 ElInputOtpSeparatorRenderer 分支；不属于该分支时返回 null。
    /// </summary>
    public ElInputOtpSeparatorRenderer? AsRenderer => Value as ElInputOtpSeparatorRenderer;
}

/// <summary>
/// 用于 ElMentionFilterOptionValue.AsCallback 的回调签名。
/// 读取当前值的 ElMentionFilterOption 分支；不属于该分支时返回 null。
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate bool ElMentionFilterOption(string pattern, ElMentionOption option);

/// <summary>
/// 用于 ElMention.FilterOption 的参数类型。
/// customize filter option logic
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union ElMentionFilterOptionValue(bool, ElMentionFilterOption)
{
    /// <summary>
    /// 读取当前值的 bool 分支；不属于该分支时返回 null。
    /// </summary>
    public bool? AsBool => Value is bool value ? value : default(bool?);

    /// <summary>
    /// 读取当前值的 ElMentionFilterOption 分支；不属于该分支时返回 null。
    /// </summary>
    public ElMentionFilterOption? AsCallback => Value as ElMentionFilterOption;
}

/// <summary>
/// 用于 ElMention.CheckIsWhole 的回调签名。
/// when backspace is pressed to delete, check if the mention is a whole
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate bool ElMentionCheckIsWhole(string pattern, string prefix);

/// <summary>
/// 进度颜色区间的上限和颜色，供 Progress 根据 percentage 选择颜色。
/// </summary>
[ECMAScript]
[Description("@#ProgressColor")]
public sealed record ElProgressColorStop : VueProps
{
    /// <summary>
    /// 该进度区间使用的 CSS 颜色值。
    /// </summary>
    [Description("@#color")]
    public string? Color { get; init; }

    /// <summary>
    /// 进度百分比，以 0 到 100 表示。
    /// </summary>
    [Description("@#percentage")]
    public Number? Percentage { get; init; }
}

/// <summary>
/// 用于 ElProgressColorValue.AsCallback 的回调签名。
/// 读取当前值的 ElProgressColorCallback 分支；不属于该分支时返回 null。
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate string ElProgressColorCallback(Number percentage);

/// <summary>
/// 用于 ElProgress.Color 的参数类型。
/// background color of progress bar. Overrides `status` prop
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union ElProgressColorValue(
    string,
    ElProgressColorStop[],
    ElProgressColorCallback)
{
    /// <summary>
    /// 读取当前值的 string 分支；不属于该分支时返回 null。
    /// </summary>
    public string? AsString => Value as string;

    /// <summary>
    /// 读取当前值的 ElProgressColorStop[] 分支；不属于该分支时返回 null。
    /// </summary>
    public ElProgressColorStop[]? AsStops => Value as ElProgressColorStop[];

    /// <summary>
    /// 读取当前值的 ElProgressColorCallback 分支；不属于该分支时返回 null。
    /// </summary>
    public ElProgressColorCallback? AsCallback => Value as ElProgressColorCallback;
}

/// <summary>
/// 用于 ElProgress.Format 的回调签名。
/// custom text format
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate string ElProgressFormatCallback(Number percentage);

/// <summary>
/// 用于 ElCascader.FilterMethod 的回调签名。
/// customize search logic, the first parameter is `node`, the second is `keyword`, and need return a boolean value indicating whether it hits.
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate bool ElCascaderFilterMethod(VueDictionary node, string keyword);

/// <summary>
/// 筛选前执行异步处理；Promise 被拒绝时阻止本次筛选。
/// </summary>
/// <param name="value">要传入的值，保持其声明的类型和数据。</param>
[ECMAScript]
[Description("@#")]
public delegate IPromise<VueValue?> ElCascaderBeforeFilterAsyncCallback(string value);

/// <summary>
/// 筛选前检查输入文本；返回 false 时阻止本次筛选。
/// </summary>
/// <param name="value">要传入的值，保持其声明的类型和数据。</param>
[ECMAScript]
[Description("@#")]
public delegate bool ElCascaderBeforeFilterSyncCallback(string value);

/// <summary>
/// C# 联合参数，允许 bool, IPromise&lt;VueValue?&gt;。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取相应分支。
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union ElCascaderBeforeFilterResult(bool, IPromise<VueValue?>)
{
    /// <summary>
    /// 读取当前值的 bool 分支；不属于该分支时返回 null。
    /// </summary>
    public bool? AsBool => Value is bool value ? value : default(bool?);

    /// <summary>
    /// 读取当前值的 IPromise&lt;VueValue?&gt; 分支；不属于该分支时返回 null。
    /// </summary>
    public IPromise<VueValue?>? AsPromise => Value as IPromise<VueValue?>;
}

/// <summary>
/// 用于 ElCascader.BeforeFilter 的回调签名。
/// hook function before filtering with the value to be filtered as its parameter. If `false` is returned or a `Promise` is returned and then is rejected, filtering will be aborted
/// </summary>
/// <param name="value">要传入的值，保持其声明的类型和数据。</param>
[ECMAScript]
[Description("@#")]
public delegate ElCascaderBeforeFilterResult ElCascaderBeforeFilterCallback(string value);

/// <summary>
/// 展开或收起前异步确认；返回 false 或拒绝 Promise 可阻止切换。
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate IPromise<bool?> ElCollapseBeforeCollapseAsyncCallback(VueStringNumberValue name);

/// <summary>
/// 展开或收起前同步确认；返回 false 可阻止切换。
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate bool ElCollapseBeforeCollapseSyncCallback(VueStringNumberValue name);

/// <summary>
/// C# 联合参数，允许 bool, IPromise&lt;bool?&gt;。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取相应分支。
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union ElCollapseBeforeCollapseResult(bool, IPromise<bool?>)
{
    /// <summary>
    /// 读取当前值的 bool 分支；不属于该分支时返回 null。
    /// </summary>
    public bool? AsBool => Value is bool value ? value : default(bool?);

    /// <summary>
    /// 读取当前值的 IPromise&lt;bool?&gt; 分支；不属于该分支时返回 null。
    /// </summary>
    public IPromise<bool?>? AsPromise => Value as IPromise<bool?>;
}

/// <summary>
/// 用于 ElCollapse.BeforeCollapse 的回调签名。
/// before-collapse hook before the collapse state changes. If `false` is returned or a `Promise` is returned and then is rejected, will stop collapsing
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate ElCollapseBeforeCollapseResult ElCollapseBeforeCollapseCallback(VueStringNumberValue name);

/// <summary>
/// 用于 ElDatePicker.CellClassName、ElDatePickerPanel.CellClassName 的回调签名。
/// set custom className
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate string ElDateLikeCellClassName(Date date);

/// <summary>
/// 用于 ElDatePicker.DisabledDate、ElDatePickerPanel.DisabledDate 的回调签名。
/// a function determining if a date is disabled with that date as its parameter. Should return a Boolean
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate bool ElDateLikeDisabledDate(Date date);

/// <summary>
/// 用于 ElValueOnClearValue.AsCallback 的回调签名。
/// 读取当前值的 ElValueOnClearCallback 分支；不属于该分支时返回 null。
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate void ElValueOnClearCallback();

/// <summary>
/// 用于 ElTable.TooltipOptions、ElTableConfig.TooltipOptions 的参数类型。
/// the options for the overflow tooltip, [see the following tooltip component](tooltip.html#attributes)
/// </summary>
[ECMAScript]
[Description("@#TableOverflowTooltipOptions")]
public sealed record ElTableOverflowTooltipOptions : VueProps
{
    /// <summary>
    /// 提示浮层的挂载目标元素或选择器。
    /// </summary>
    [Description("@#appendTo")]
    public VueTeleportTarget? AppendTo { get; init; }

    /// <summary>
    /// 提示浮层的明暗主题。
    /// </summary>
    [Description("@#effect")]
    public string? Effect { get; init; }

    /// <summary>
    /// 是否允许指针进入提示浮层。
    /// </summary>
    [Description("@#enterable")]
    public bool? Enterable { get; init; }

    /// <summary>
    /// 指针离开后延迟隐藏的时间，单位为毫秒。
    /// </summary>
    [Description("@#hideAfter")]
    public Number? HideAfter { get; init; }

    /// <summary>
    /// 提示浮层与目标元素的像素间距。
    /// </summary>
    [Description("@#offset")]
    public Number? Offset { get; init; }

    /// <summary>
    /// 提示浮层相对于触发元素的位置。
    /// </summary>
    [Description("@#placement")]
    public string? Placement { get; init; }

    /// <summary>
    /// 附加到提示浮层的 CSS 类名。
    /// </summary>
    [Description("@#popperClass")]
    public string? PopperClass { get; init; }

    /// <summary>
    /// 传给 Popper 定位引擎的附加配置。
    /// </summary>
    [Description("@#popperOptions")]
    public VueDictionary? PopperOptions { get; init; }

    /// <summary>
    /// 触发后延迟显示的时间，单位为毫秒。
    /// </summary>
    [Description("@#showAfter")]
    public Number? ShowAfter { get; init; }

    /// <summary>
    /// 是否显示提示浮层的指向箭头。
    /// </summary>
    [Description("@#showArrow")]
    public bool? ShowArrow { get; init; }

    /// <summary>
    /// 提示浮层使用的 Vue 过渡名称。
    /// </summary>
    [Description("@#transition")]
    public string? Transition { get; init; }
}

/// <summary>
/// 用于 ElTable.ShowOverflowTooltip、ElTableColumn.ShowOverflowTooltip、ElTableConfig.ShowOverflowTooltip 的参数类型。
/// whether to hide extra content and show them in a tooltip when hovering on the cell.It will affect all the table columns, refer to table [tooltip-options](#table-attributes)
/// whether to hide extra content and show them in a tooltip when hovering on the cell
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union ElTableOverflowTooltipValue(bool, ElTableOverflowTooltipOptions)
{
    /// <summary>
    /// 读取当前值的 bool 分支；不属于该分支时返回 null。
    /// </summary>
    public bool? AsBool => Value is bool value ? value : default(bool?);

    /// <summary>
    /// 读取当前值的 ElTableOverflowTooltipOptions 分支；不属于该分支时返回 null。
    /// </summary>
    public ElTableOverflowTooltipOptions? AsOptions => Value as ElTableOverflowTooltipOptions;
}

/// <summary>
/// 多行输入框自动高度的最少、最多行数限制。
/// </summary>
[ECMAScript]
[Description("@#InputAutoSizeOptions")]
public sealed record ElInputAutoSizeOptions : VueProps
{
    /// <summary>
    /// 自动调整文本框高度时允许的最少行数。
    /// </summary>
    [Description("@#minRows")]
    public Number? MinRows { get; init; }

    /// <summary>
    /// 自动调整文本框高度时允许的最多行数。
    /// </summary>
    [Description("@#maxRows")]
    public Number? MaxRows { get; init; }
}

/// <summary>
/// 用于 ElInput.Autosize 的参数类型。
/// whether textarea has an adaptive height, only works when `type` is 'textarea'. Can accept an object, e.g. `{ minRows: 2, maxRows: 6 }`
/// </summary>
[ECMAScript]
[Description("@#InputAutoSize")]
public readonly union ElInputAutoSize(bool, ElInputAutoSizeOptions)
{
    /// <summary>
    /// 读取当前值的 bool 分支；不属于该分支时返回 null。
    /// </summary>
    public bool? AsBool => Value is bool value ? value : default(bool?);

    /// <summary>
    /// 读取当前值的 ElInputAutoSizeOptions 分支；不属于该分支时返回 null。
    /// </summary>
    public ElInputAutoSizeOptions? AsOptions => Value as ElInputAutoSizeOptions;
}

/// <summary>
/// 栅格列在特定响应式断点下使用的跨度、偏移和左右移动配置。
/// </summary>
[ECMAScript]
[Description("@#ColSizeObject")]
public sealed record ElColSizeProps : VueProps
{
    /// <summary>
    /// number of column the grid spans
    /// </summary>
    [Description("@#span")]
    public Number? Span { get; init; }

    /// <summary>
    /// number of spacing on the left side of the grid
    /// </summary>
    [Description("@#offset")]
    public Number? Offset { get; init; }

    /// <summary>
    /// number of columns that grid moves to the left
    /// </summary>
    [Description("@#pull")]
    public Number? Pull { get; init; }

    /// <summary>
    /// number of columns that grid moves to the right
    /// </summary>
    [Description("@#push")]
    public Number? Push { get; init; }
}

/// <summary>
/// 用于 ElCol.Xs、ElCol.Sm、ElCol.Md、ElCol.Lg、ElCol.Xl 的参数类型。
/// `&lt;768px` Responsive columns or column props object
/// `≥768px` Responsive columns or column props object
/// `≥992px` Responsive columns or column props object
/// `≥1200px` Responsive columns or column props object
/// `≥1920px` Responsive columns or column props object
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union ElColSizeValue(double, ElColSizeProps)
{
    /// <summary>
    /// 读取当前值的 double 分支；不属于该分支时返回 null。
    /// </summary>
    public double? AsNumber => Value is double value ? value : default(double?);

    /// <summary>
    /// 读取当前值的 ElColSizeProps 分支；不属于该分支时返回 null。
    /// </summary>
    public ElColSizeProps? AsProps => Value as ElColSizeProps;
}

/// <summary>
/// 用于 ElSpace.Size 的参数类型。
/// Spacing size
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union ElSpaceSizeValue(ElComponentSize, Number, VueNumberPair)
{
    /// <summary>
    /// 读取当前值的 ElComponentSize 分支；不属于该分支时返回 null。
    /// </summary>
    public ElComponentSize? AsComponentSize
        => Value is ElComponentSize value ? value : default(ElComponentSize?);

    /// <summary>
    /// 读取当前值的 Number 分支；不属于该分支时返回 null。
    /// </summary>
    public Number? AsNumber => Value is Number value ? value : default(Number?);

    /// <summary>
    /// 读取当前值的 VueNumberPair 分支；不属于该分支时返回 null。
    /// </summary>
    public VueNumberPair? AsPair => Value is VueNumberPair value ? value : default(VueNumberPair?);

    /// <summary>
    /// 将 double 值转换为 ElSpaceSizeValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator ElSpaceSizeValue(double value)
        => new((Number)value);
}

/// <summary>
/// 节流渲染的初始状态和前后延迟配置。
/// </summary>
[ECMAScript]
[Description("@#ThrottleRender")]
public sealed record ElThrottleRenderOptions : VueProps
{
    /// <summary>
    /// 节流周期开始阶段使用的延迟时间，单位为毫秒。
    /// </summary>
    [Description("@#leading")]
    public Number? Leading { get; init; }

    /// <summary>
    /// 节流周期结束阶段使用的延迟时间，单位为毫秒。
    /// </summary>
    [Description("@#trailing")]
    public Number? Trailing { get; init; }

    /// <summary>
    /// 节流渲染开始前使用的初始布尔状态。
    /// </summary>
    [Description("@#initVal")]
    public bool? InitVal { get; init; }
}

/// <summary>
/// 用于 ElSkeleton.Throttle 的参数类型。
/// rendering delay in milliseconds. Numbers represent delayed display, and can also be set to delay hide, for example `{ leading: 500, trailing: 500 }`. When needing to control the initial value of loading, you can set `{ initVal: true }`
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union ElThrottleValue(Number, ElThrottleRenderOptions)
{
    /// <summary>
    /// 读取当前值的 Number 分支；不属于该分支时返回 null。
    /// </summary>
    public Number? AsNumber => Value is Number value ? value : default(Number?);

    /// <summary>
    /// 读取当前值的 ElThrottleRenderOptions 分支；不属于该分支时返回 null。
    /// </summary>
    public ElThrottleRenderOptions? AsOptions => Value as ElThrottleRenderOptions;

    /// <summary>
    /// 将 double 值转换为 ElThrottleValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator ElThrottleValue(double value)
        => new((Number)value);
}

/// <summary>
/// 用于 ElTableColumn.SortOrders 的可选取值。
/// the order of the sorting strategies used when sorting the data, works when `sortable` is `true`. Accepts an array, as the user clicks on the header, the column is sorted in order of the elements in the array
/// </summary>
[String]
public enum ElTableSortOrder
{
    /// <summary>
    /// 按升序排序；上游取值为 “ascending”。
    /// </summary>
    [Description("@#ascending")]
    Ascending,

    /// <summary>
    /// 按降序排序；上游取值为 “descending”。
    /// </summary>
    [Description("@#descending")]
    Descending
}

/// <summary>
/// 用于 ElTable.DefaultSort 的参数类型。
/// set the default sort column and order. property `prop` is used to set default sort column, property `order` is used to set default sort order
/// </summary>
[ECMAScript]
[Description("@#Sort")]
public sealed record ElTableSort : VueProps
{
    /// <summary>
    /// 用于读取行数据字段的属性名称或路径。
    /// </summary>
    [Description("@#prop")]
    public string? Prop { get; init; }

    /// <summary>
    /// 当前排序方向；空值表示不指定排序方向。
    /// </summary>
    [Description("@#order")]
    public ElTableSortOrder? Order { get; init; }

    /// <summary>
    /// 标识本次排序是否来自初始化阶段。
    /// </summary>
    [Description("@#init")]
    public VueValue? Init { get; init; }

    /// <summary>
    /// 是否静默应用排序状态。
    /// </summary>
    [Description("@#silent")]
    public VueValue? Silent { get; init; }
}

/// <summary>
/// 用于 ElTable.TreeProps 的参数类型。
/// configuration for rendering nested data
/// </summary>
[ECMAScript]
[Description("@#TreeProps")]
public sealed record ElTableTreeProps : VueProps
{
    /// <summary>
    /// 行数据中表示存在子节点的字段名称。
    /// </summary>
    [Description("@#hasChildren")]
    public string? HasChildren { get; init; }

    /// <summary>
    /// 源数据中保存子节点数组的字段名称。
    /// </summary>
    [Description("@#children")]
    public string? Children { get; init; }

    /// <summary>
    /// 父子行的选择状态是否互不关联。
    /// </summary>
    [Description("@#checkStrictly")]
    public bool? CheckStrictly { get; init; }
}

/// <summary>
/// 用于 ElTableColumn.Filters 的参数类型。
/// an array of data filtering options. For each element in this array, `text` and `value` are required
/// </summary>
[ECMAScript]
[Description("@#Filter")]
public sealed record ElTableFilterItem : VueProps
{
    /// <summary>
    /// 筛选选项的显示文本。
    /// </summary>
    [Description("@#text")]
    public string? Text { get; init; }

    /// <summary>
    /// 选择此项时写入模型的值。
    /// </summary>
    [Description("@#value")]
    public string? Value { get; init; }
}

/// <summary>
/// 扩展浏览器 File 的上传原始文件，带有组件分配的 uid。
/// </summary>
[ECMAScript]
[Description("@#UploadRawFile")]
public sealed record ElUploadRawFile : VueProps
{
    /// <summary>
    /// 上传文件的唯一标识，用于关联列表状态与请求回调。
    /// </summary>
    [Description("@#uid")]
    public Number Uid { get; init; } = default!;

    /// <summary>
    /// 原始上传项是否表示目录。
    /// </summary>
    [Description("@#isDirectory")]
    public bool? IsDirectory { get; init; }
}

/// <summary>
/// 用于 ElUpload.FileList 的参数类型。
/// default uploaded files.
/// </summary>
[ECMAScript]
[Description("@#UploadUserFile")]
public sealed record ElUploadUserFile : VueProps
{
    /// <summary>
    /// 上传文件的名称。
    /// </summary>
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 进度百分比，以 0 到 100 表示。
    /// </summary>
    [Description("@#percentage")]
    public Number? Percentage { get; init; }

    /// <summary>
    /// 当前上传文件的生命周期状态。
    /// </summary>
    [Description("@#status")]
    public ElUploadStatus? Status { get; init; }

    /// <summary>
    /// 原始文件大小，单位为字节。
    /// </summary>
    [Description("@#size")]
    public Number? Size { get; init; }

    /// <summary>
    /// 上传请求返回的响应数据。
    /// </summary>
    [Description("@#response")]
    public VueValue? Response { get; init; }

    /// <summary>
    /// 上传文件的唯一标识，用于关联列表状态与请求回调。
    /// </summary>
    [Description("@#uid")]
    public Number? Uid { get; init; }

    /// <summary>
    /// 已上传文件的访问或预览 URL。
    /// </summary>
    [Description("@#url")]
    public string? Url { get; init; }

    /// <summary>
    /// 当前上传项对应的原始浏览器文件对象。
    /// </summary>
    [Description("@#raw")]
    public ElUploadRawFile? Raw { get; init; }
}

/// <summary>
/// 上传列表中的文件状态，包含进度、响应、预览地址和原始文件。
/// </summary>
[ECMAScript]
[Description("@#UploadFile")]
public sealed record ElUploadFile : VueProps
{
    /// <summary>
    /// 上传文件的名称。
    /// </summary>
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 进度百分比，以 0 到 100 表示。
    /// </summary>
    [Description("@#percentage")]
    public Number? Percentage { get; init; }

    /// <summary>
    /// 当前上传文件的生命周期状态。
    /// </summary>
    [Description("@#status")]
    public ElUploadStatus Status { get; init; } = default!;

    /// <summary>
    /// 原始文件大小，单位为字节。
    /// </summary>
    [Description("@#size")]
    public Number? Size { get; init; }

    /// <summary>
    /// 上传请求返回的响应数据。
    /// </summary>
    [Description("@#response")]
    public VueValue? Response { get; init; }

    /// <summary>
    /// 上传文件的唯一标识，用于关联列表状态与请求回调。
    /// </summary>
    [Description("@#uid")]
    public Number Uid { get; init; } = default!;

    /// <summary>
    /// 已上传文件的访问或预览 URL。
    /// </summary>
    [Description("@#url")]
    public string? Url { get; init; }

    /// <summary>
    /// 当前上传项对应的原始浏览器文件对象。
    /// </summary>
    [Description("@#raw")]
    public ElUploadRawFile? Raw { get; init; }
}

/// <summary>
/// 工具提示的触发方式，可通过悬停、焦点、单击或上下文菜单触发。
/// </summary>
[String]
public enum ElTooltipTriggerType
{
    /// <summary>
    /// 指针悬停触发；上游取值为 “hover”。
    /// </summary>
    [Description("@#hover")]
    Hover,

    /// <summary>
    /// 获得焦点触发；上游取值为 “focus”。
    /// </summary>
    [Description("@#focus")]
    Focus,

    /// <summary>
    /// 单击触发；上游取值为 “click”。
    /// </summary>
    [Description("@#click")]
    Click,

    /// <summary>
    /// 上下文菜单事件触发，通常为右键单击；上游取值为 “contextmenu”。
    /// </summary>
    [Description("@#contextmenu")]
    Contextmenu
}

/// <summary>
/// 下拉菜单的触发方式：悬停、单击或上下文菜单。
/// </summary>
[String]
public enum ElDropdownTriggerType
{
    /// <summary>
    /// 单击触发；上游取值为 “click”。
    /// </summary>
    [Description("@#click")]
    Click,

    /// <summary>
    /// 指针悬停触发；上游取值为 “hover”。
    /// </summary>
    [Description("@#hover")]
    Hover,

    /// <summary>
    /// 上下文菜单事件触发，通常为右键单击；上游取值为 “contextmenu”。
    /// </summary>
    [Description("@#contextmenu")]
    Contextmenu
}

/// <summary>
/// 用于 ElDropdown.Trigger 的参数类型。
/// how to trigger
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union ElDropdownTriggerValue(ElDropdownTriggerType, ElDropdownTriggerType[])
{
    /// <summary>
    /// 读取当前值的 ElDropdownTriggerType 分支；不属于该分支时返回 null。
    /// </summary>
    public ElDropdownTriggerType? AsSingle
        => Value is ElDropdownTriggerType value ? value : default(ElDropdownTriggerType?);

    /// <summary>
    /// 读取当前值的 ElDropdownTriggerType[] 分支；不属于该分支时返回 null。
    /// </summary>
    public ElDropdownTriggerType[]? AsMultiple => Value as ElDropdownTriggerType[];
}

/// <summary>
/// 用于 ElPopover.Trigger、ElTooltip.Trigger 的参数类型。
/// how the popover is triggered, not valid in controlled mode
/// How should the tooltip be triggered (to show), not valid in controlled mode
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union ElTooltipTriggerValue(ElTooltipTriggerType, ElTooltipTriggerType[])
{
    /// <summary>
    /// 读取当前值的 ElTooltipTriggerType 分支；不属于该分支时返回 null。
    /// </summary>
    public ElTooltipTriggerType? AsSingle
        => Value is ElTooltipTriggerType value ? value : default(ElTooltipTriggerType?);

    /// <summary>
    /// 读取当前值的 ElTooltipTriggerType[] 分支；不属于该分支时返回 null。
    /// </summary>
    public ElTooltipTriggerType[]? AsMultiple => Value as ElTooltipTriggerType[];
}

/// <summary>
/// 用于 ElSelect.TagTooltip、ElVirtualizedSelect.TagTooltip 的参数类型。
/// configuration object for the collapse-tags tooltip. To use this, `collapse-tags` and `collapse-tags-tooltip` must be true
/// </summary>
[ECMAScript]
[Description("@#TagTooltipProps")]
public sealed record ElTagTooltipProps : VueProps
{
    /// <summary>
    /// Which element the tooltip CONTENT appends to
    /// </summary>
    [Description("@#appendTo")]
    public VueTeleportTarget? AppendTo { get; init; }

    /// <summary>
    /// Position of Tooltip
    /// </summary>
    [Description("@#placement")]
    public ElPopperPlacement? Placement { get; init; }

    /// <summary>
    /// List of possible positions for Tooltip [popper.js](https://popper.js.org/docs/v2/modifiers/flip/#fallbackplacements)
    /// </summary>
    [Description("@#fallbackPlacements")]
    public ElPopperPlacement[]? FallbackPlacements { get; init; }

    /// <summary>
    /// Tooltip theme, built-in theme: `dark` / `light`
    /// </summary>
    [Description("@#effect")]
    public ElPopperEffect? Effect { get; init; }

    /// <summary>
    /// Custom class name for Tooltip's popper
    /// </summary>
    [Description("@#popperClass")]
    public string? PopperClass { get; init; }

    /// <summary>
    /// Custom style for Tooltip's popper
    /// </summary>
    [Description("@#popperStyle")]
    public VueStyleValue? PopperStyle { get; init; }

    /// <summary>
    /// Animation name
    /// </summary>
    [Description("@#transition")]
    public string? Transition { get; init; }

    /// <summary>
    /// Whether tooltip content is teleported, if `true` it will be teleported to where `append-to` sets
    /// </summary>
    [Description("@#teleported")]
    public bool? Teleported { get; init; }

    /// <summary>
    /// [popper.js](https://popper.js.org/docs/v2/) parameters
    /// </summary>
    [Description("@#popperOptions")]
    public VueDictionary? PopperOptions { get; init; }

    /// <summary>
    /// Delay of appearance, in millisecond, not valid in controlled mode
    /// </summary>
    [Description("@#showAfter")]
    public Number? ShowAfter { get; init; }

    /// <summary>
    /// Delay of disappear, in millisecond, not valid in controlled mode
    /// </summary>
    [Description("@#hideAfter")]
    public Number? HideAfter { get; init; }

    /// <summary>
    /// Timeout in milliseconds to hide tooltip, not valid in controlled mode
    /// </summary>
    [Description("@#autoClose")]
    public Number? AutoClose { get; init; }

    /// <summary>
    /// Offset of the Tooltip
    /// </summary>
    [Description("@#offset")]
    public Number? Offset { get; init; }
}

/// <summary>
/// 用于 ElDropdown.ButtonProps 的参数类型。
/// props for the button component, refer to [Button Attributes](./button.html#button-attributes)
/// </summary>
[ECMAScript]
[Description("@#ButtonProps")]
public sealed record ElButtonProps : VueProps
{
    /// <summary>
    /// button size
    /// </summary>
    [Description("@#size")]
    public ElComponentSize? Size { get; init; }

    /// <summary>
    /// disable the button
    /// </summary>
    [Description("@#disabled")]
    public bool? Disabled { get; init; }

    /// <summary>
    /// button type, when setting `color`, the latter prevails
    /// </summary>
    [Description("@#type")]
    public ElButtonType? Type { get; init; }

    /// <summary>
    /// icon component
    /// </summary>
    [Description("@#icon")]
    public VueStringComponentValue? Icon { get; init; }

    /// <summary>
    /// same as native button's `type`
    /// </summary>
    [Description("@#nativeType")]
    public ElButtonNativeType? NativeType { get; init; }

    /// <summary>
    /// determine whether it's loading
    /// </summary>
    [Description("@#loading")]
    public bool? Loading { get; init; }

    /// <summary>
    /// customize loading icon component
    /// </summary>
    [Description("@#loadingIcon")]
    public VueStringComponentValue? LoadingIcon { get; init; }

    /// <summary>
    /// determine whether it's a plain button
    /// </summary>
    [Description("@#plain")]
    public bool? Plain { get; init; }

    /// <summary>
    /// determine whether it's a text button
    /// </summary>
    [Description("@#text")]
    public bool? Text { get; init; }

    /// <summary>
    /// determine whether it's a link button
    /// </summary>
    [Description("@#link")]
    public bool? Link { get; init; }

    /// <summary>
    /// determine whether the text button background color is always on
    /// </summary>
    [Description("@#bg")]
    public bool? Bg { get; init; }

    /// <summary>
    /// same as native button's `autofocus`
    /// </summary>
    [Description("@#autofocus")]
    public bool? Autofocus { get; init; }

    /// <summary>
    /// determine whether it's a round button
    /// </summary>
    [Description("@#round")]
    public bool? Round { get; init; }

    /// <summary>
    /// determine whether it's a circle button
    /// </summary>
    [Description("@#circle")]
    public bool? Circle { get; init; }

    /// <summary>
    /// determine whether it's a dashed button
    /// </summary>
    [Description("@#dashed")]
    public bool? Dashed { get; init; }

    /// <summary>
    /// custom button color, automatically calculate `hover` and `active` color. Works with `link`/`text` buttons since
    /// </summary>
    [Description("@#color")]
    public string? Color { get; init; }

    /// <summary>
    /// dark mode, which automatically converts `color` to dark mode colors
    /// </summary>
    [Description("@#dark")]
    public bool? Dark { get; init; }

    /// <summary>
    /// automatically insert a space between two chinese characters(this will only take effect when the text length is 2 and all characters are in Chinese.)
    /// </summary>
    [Description("@#autoInsertSpace")]
    public bool? AutoInsertSpace { get; init; }

    /// <summary>
    /// custom element tag
    /// </summary>
    [Description("@#tag")]
    public VueStringComponentValue? Tag { get; init; }
}

/// <summary>
/// 用于 ElTransfer.Data 的参数类型。
/// data source
/// </summary>
[ECMAScript]
[Description("@#TransferDataItem")]
public sealed record ElTransferDataItem : VueDictionary
{
}

/// <summary>
/// 用于 ElTransfer.TargetOrder 的可选取值。
/// order strategy for elements in the target list. If set to `original`, the elements will keep the same order as the data source. If set to `push`, the newly added elements will be pushed to the bottom. If set to `unshift`, the newly added elements will be inserted on the top
/// </summary>
[String]
public enum ElTransferTargetOrder
{
    /// <summary>
    /// 保持源数据中的顺序；上游取值为 “original”。
    /// </summary>
    [Description("@#original")]
    Original,

    /// <summary>
    /// 将新选择项追加到末尾；上游取值为 “push”。
    /// </summary>
    [Description("@#push")]
    Push,

    /// <summary>
    /// 将新选择项添加到开头；上游取值为 “unshift”。
    /// </summary>
    [Description("@#unshift")]
    Unshift
}

/// <summary>
/// 用于 ElTransfer.Format 的参数类型。
/// texts for checking status in list header
/// </summary>
[ECMAScript]
[Description("@#TransferFormat")]
public sealed record ElTransferFormat : VueProps
{
    /// <summary>
    /// 未选中条目时显示的统计文本格式，可包含总数占位符。
    /// </summary>
    [Description("@#noChecked")]
    public string? NoChecked { get; init; }

    /// <summary>
    /// 存在选中条目时显示的统计文本格式，可包含选中数和总数占位符。
    /// </summary>
    [Description("@#hasChecked")]
    public string? HasChecked { get; init; }
}

/// <summary>
/// 用于 ElTransfer.Props 的参数类型。
/// prop aliases for data source
/// </summary>
[ECMAScript]
[Description("@#TransferPropsAlias")]
public sealed record ElTransferPropsAlias : VueProps
{
    /// <summary>
    /// 选项对象中用于显示标签的字段名称。
    /// </summary>
    [Description("@#label")]
    public string? Label { get; init; }

    /// <summary>
    /// 源数据中用于条目唯一标识的字段名称。
    /// </summary>
    [Description("@#key")]
    public string? Key { get; init; }

    /// <summary>
    /// 选项对象中用于判断禁用状态的字段名称。
    /// </summary>
    [Description("@#disabled")]
    public string? Disabled { get; init; }
}

/// <summary>
/// 用于 ElTransfer.Titles、ElTransfer.ButtonTexts 的参数类型。
/// custom list titles
/// custom button texts
/// </summary>
[ECMAScript]
[Union]
[Description("@#")]
[CollectionBuilder(typeof(ElTransferTextPairCollectionBuilder), nameof(ElTransferTextPairCollectionBuilder.Create))]
public readonly struct ElTransferTextPair : IUnion, IEnumerable<string>
{
    private readonly string[]? _values;

    /// <summary>
    /// 使用恰好两个字符串构造穿梭框左右列表的文本配置；数组长度不为 2 时抛出参数异常。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    public ElTransferTextPair(string[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        if (values.Length != 2)
            throw new ArgumentException("Element Plus transfer text pairs require exactly two items.", nameof(values));

        _values = values;
    }

    /// <summary>
    /// 读取当前值的 string[] 分支；不属于该分支时返回 null。
    /// </summary>
    public string[]? AsValues => _values;

    /// <summary>
    /// 值对中的第一段文本，通常用于左侧列表。
    /// </summary>
    public string? First => _values is { Length: > 0 } values ? values[0] : null;

    /// <summary>
    /// 值对中的第二段文本，通常用于右侧列表。
    /// </summary>
    public string? Second => _values is { Length: > 1 } values ? values[1] : null;

    /// <summary>
    /// 读取当前联合分支保存的原始值，供宿主 API 传递；不会转换到其他分支。
    /// </summary>
    public object? Value => _values;

    /// <summary>
    /// 将 string[] 值转换为 ElTransferTextPair，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator ElTransferTextPair(string[] values)
        => new(values);

    IEnumerator<string> IEnumerable<string>.GetEnumerator()
        => ((IEnumerable<string>)(_values ?? Array.Empty<string>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<string>)this).GetEnumerator();
}

/// <summary>
/// 供 C# 集合表达式调用的构建器；应用可直接使用 [item1, item2] 语法构造对应集合。
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class ElTransferTextPairCollectionBuilder
{
    /// <summary>
    /// 为 C# 集合表达式创建有序集合；复制传入的元素，不保留临时 Span。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    public static ElTransferTextPair Create(ReadOnlySpan<string> values)
        => values.ToArray();
}

/// <summary>
/// 用于 ElSelect.Props、ElTreeSelect.Props、ElVirtualizedSelect.Props 的参数类型。
/// configuration options
/// configuration options, see the following table
/// </summary>
[ECMAScript]
[Description("@#SelectPropsAlias")]
public sealed record ElSelectPropsAlias : VueProps
{
    /// <summary>
    /// 选项对象中用于显示标签的字段名称。
    /// </summary>
    [Description("@#label")]
    public string? Label { get; init; }

    /// <summary>
    /// 选项对象中用于模型绑定的值字段名称。
    /// </summary>
    [Description("@#value")]
    public string? Value { get; init; }

    /// <summary>
    /// 选项对象中用于判断禁用状态的字段名称。
    /// </summary>
    [Description("@#disabled")]
    public string? Disabled { get; init; }

    /// <summary>
    /// 选项对象中嵌套选项数组的字段名称。
    /// </summary>
    [Description("@#options")]
    public string? Options { get; init; }
}

/// <summary>
/// 用于 ElCheckboxGroup.Props 的参数类型。
/// configuration options
/// </summary>
[ECMAScript]
[Description("@#CheckboxOptionProps")]
public sealed record ElCheckboxOptionPropsAlias : VueProps
{
    /// <summary>
    /// 选项对象中用于模型绑定的值字段名称。
    /// </summary>
    [Description("@#value")]
    public string? Value { get; init; }

    /// <summary>
    /// 选项对象中用于显示标签的字段名称。
    /// </summary>
    [Description("@#label")]
    public string? Label { get; init; }

    /// <summary>
    /// 选项对象中用于判断禁用状态的字段名称。
    /// </summary>
    [Description("@#disabled")]
    public string? Disabled { get; init; }
}

/// <summary>
/// 用于 ElMention.Props 的参数类型。
/// configuration options
/// </summary>
[ECMAScript]
[Description("@#MentionOptionProps")]
public sealed record ElMentionOptionPropsAlias : VueProps
{
    /// <summary>
    /// 选项对象中用于模型绑定的值字段名称。
    /// </summary>
    [Description("@#value")]
    public string? Value { get; init; }

    /// <summary>
    /// 选项对象中用于显示标签的字段名称。
    /// </summary>
    [Description("@#label")]
    public string? Label { get; init; }

    /// <summary>
    /// 选项对象中用于判断禁用状态的字段名称。
    /// </summary>
    [Description("@#disabled")]
    public string? Disabled { get; init; }
}

/// <summary>
/// 用于 ElRadioGroup.Props 的参数类型。
/// configuration options
/// </summary>
[ECMAScript]
[Description("@#radioOptionProp")]
public sealed record ElRadioOptionPropsAlias : VueProps
{
    /// <summary>
    /// 选项对象中用于模型绑定的值字段名称。
    /// </summary>
    [Description("@#value")]
    public string? Value { get; init; }

    /// <summary>
    /// 选项对象中用于显示标签的字段名称。
    /// </summary>
    [Description("@#label")]
    public string? Label { get; init; }

    /// <summary>
    /// 选项对象中用于判断禁用状态的字段名称。
    /// </summary>
    [Description("@#disabled")]
    public string? Disabled { get; init; }
}

/// <summary>
/// 用于 ElSegmented.Props 的参数类型。
/// configuration options, see the following table
/// </summary>
[ECMAScript]
[Description("@#SegmentedPropsAlias")]
public sealed record ElSegmentedPropsAlias : VueProps
{
    /// <summary>
    /// 选项对象中用于显示标签的字段名称。
    /// </summary>
    [Description("@#label")]
    public string? Label { get; init; }

    /// <summary>
    /// 选项对象中用于模型绑定的值字段名称。
    /// </summary>
    [Description("@#value")]
    public string? Value { get; init; }

    /// <summary>
    /// 选项对象中用于判断禁用状态的字段名称。
    /// </summary>
    [Description("@#disabled")]
    public string? Disabled { get; init; }
}

/// <summary>
/// 用于 ElTree.Props、ElTreeV2.Props 的参数类型。
/// configuration options, see the following table
/// </summary>
[ECMAScript]
[Description("@#TreeOptionProps")]
public sealed record ElTreeOptionProps : VueProps
{
    /// <summary>
    /// 源数据中保存子节点数组的字段名称。
    /// </summary>
    [Description("@#children")]
    public string? Children { get; init; }

    /// <summary>
    /// 节点标签的字段名称或生成标签的函数。
    /// </summary>
    [Description("@#label")]
    public VueValue? Label { get; init; }

    /// <summary>
    /// 节点禁用状态的字段名称或判定函数。
    /// </summary>
    [Description("@#disabled")]
    public VueValue? Disabled { get; init; }

    /// <summary>
    /// 指定节点是否为叶子节点的字段或判定函数，供懒加载树使用。
    /// </summary>
    [Description("@#isLeaf")]
    public VueValue? IsLeaf { get; init; }

    /// <summary>
    /// 附加到组件或单元格的 CSS 类名。
    /// </summary>
    [Description("@#class")]
    public ElTreeOptionClassCallback? CssClass { get; init; }
}

/// <summary>
/// 用于 ElCascader.Props、ElCascaderPanel.Props 的参数类型。
/// configuration options, see the following `CascaderProps` table.
/// </summary>
[ECMAScript]
[Description("@#CascaderProps")]
public sealed record ElCascaderProps : VueProps
{
    /// <summary>
    /// trigger mode of expanding options
    /// Default: click
    /// </summary>
    [Description("@#expandTrigger")]
    public string? ExpandTrigger { get; init; }

    /// <summary>
    /// whether multiple selection is enabled
    /// Default: false
    /// </summary>
    [Description("@#multiple")]
    public bool? Multiple { get; init; }

    /// <summary>
    /// whether checked state of a node not affects its parent and child nodes
    /// Default: false
    /// </summary>
    [Description("@#checkStrictly")]
    public bool? CheckStrictly { get; init; }

    /// <summary>
    /// when checked nodes change, whether to emit an array of node's path, if false, only emit the value of node.
    /// Default: true
    /// </summary>
    [Description("@#emitPath")]
    public bool? EmitPath { get; init; }

    /// <summary>
    /// whether to dynamic load child nodes, use with `lazyload` attribute
    /// Default: false
    /// </summary>
    [Description("@#lazy")]
    public bool? Lazy { get; init; }

    /// <summary>
    /// method for loading child nodes data, only works when `lazy` is true. The reject parameter is supported after version ^(2.11.5).
    /// </summary>
    [Description("@#lazyLoad")]
    public ElCascaderLazyLoadCallback? LazyLoad { get; init; }

    /// <summary>
    /// specify which key of node object is used as the node's value
    /// Default: value
    /// </summary>
    [Description("@#value")]
    public string? Value { get; init; }

    /// <summary>
    /// specify which key of node object is used as the node's label
    /// Default: label
    /// </summary>
    [Description("@#label")]
    public string? Label { get; init; }

    /// <summary>
    /// specify which key of node object is used as the node's children
    /// Default: children
    /// </summary>
    [Description("@#children")]
    public string? Children { get; init; }

    /// <summary>
    /// whether Cascader is disabled
    /// </summary>
    [Description("@#disabled")]
    public VueValue? Disabled { get; init; }

    /// <summary>
    /// specify which key of node object is used as the node's leaf field
    /// Default: leaf
    /// </summary>
    [Description("@#leaf")]
    public VueValue? Leaf { get; init; }

    /// <summary>
    /// hover threshold of expanding options
    /// Default: 500
    /// </summary>
    [Description("@#hoverThreshold")]
    public Number? HoverThreshold { get; init; }

    /// <summary>
    /// whether to check or uncheck node when clicking on the node
    /// Default: false
    /// </summary>
    [Description("@#checkOnClickNode")]
    public bool? CheckOnClickNode { get; init; }

    /// <summary>
    /// whether to check or uncheck node when clicking on leaf node (last children).
    /// Default: true
    /// </summary>
    [Description("@#checkOnClickLeaf")]
    public bool? CheckOnClickLeaf { get; init; }

    /// <summary>
    /// whether to show the radio or checkbox prefix
    /// Default: true
    /// </summary>
    [Description("@#showPrefix")]
    public bool? ShowPrefix { get; init; }
}

/// <summary>
/// 表单字段的 async-validator 规则；通过字段名与 FormItem 关联。
/// </summary>
[ECMAScript]
[Description("@#FormItemRule")]
public sealed record ElFormItemRule : VueDictionary
{
    /// <summary>
    /// How the validator is triggered.
    /// </summary>
    [Description("@#trigger")]
    public VueStringOrStringsValue? Trigger { get; init; }
}

/// <summary>
/// 用于 ElFormItem.Rules 的参数类型。
/// Validation rules of form, see the [following table](#formitemrule), more advanced usage at [async-validator](https://github.com/yiminghe/async-validator).
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union ElFormItemRules(
    ElFormItemRule,
    ElFormItemRule[]) : IEnumerable<ElFormItemRule>
{
    /// <summary>
    /// 读取当前值的 ElFormItemRule 分支；不属于该分支时返回 null。
    /// </summary>
    public ElFormItemRule? AsSingle
        => Value as ElFormItemRule;

    /// <summary>
    /// 读取当前值的 ElFormItemRule[] 分支；不属于该分支时返回 null。
    /// </summary>
    public ElFormItemRule[]? AsMultiple => Value as ElFormItemRule[];

    IEnumerator<ElFormItemRule> IEnumerable<ElFormItemRule>.GetEnumerator()
        => ((IEnumerable<ElFormItemRule>)(AsMultiple ?? Array.Empty<ElFormItemRule>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<ElFormItemRule>)this).GetEnumerator();
}

/// <summary>
/// C# 联合参数，允许 ElFormItemRules, ElFormRules。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取相应分支。
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union ElFormRuleValue(ElFormItemRules, ElFormRules)
{
    /// <summary>
    /// 读取当前值的 ElFormItemRules 分支；不属于该分支时返回 null。
    /// </summary>
    public ElFormItemRules? AsItemRules
        => Value is ElFormItemRules value ? value : default(ElFormItemRules?);

    /// <summary>
    /// 读取当前值的 ElFormRules 分支；不属于该分支时返回 null。
    /// </summary>
    public ElFormRules? AsNestedRules => Value as ElFormRules;
}

/// <summary>
/// 用于 ElForm.Rules 的参数类型。
/// Validation rules of form.
/// </summary>
[ECMAScript]
[Description("@#FormRules")]
public sealed record ElFormRules : VueDictionary<ElFormRuleValue>
{
}

/// <summary>
/// 按评分阈值配置显示颜色的映射。
/// </summary>
[ECMAScript]
[Description("@#RateColorMap")]
public sealed record ElRateColorMap : VueDictionary<string>
{
}

/// <summary>
/// 用于 ElRate.Colors 的参数类型。
/// colors for icons. If array, it should have 3 elements, each of which corresponds with a score level, else if object, the key should be threshold value between two levels, and the value should be corresponding color
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union ElRateColorsValue(
    string[],
    ElRateColorMap) : IEnumerable<string>
{
    /// <summary>
    /// 读取当前值的 string[] 分支；不属于该分支时返回 null。
    /// </summary>
    public string[]? AsArray => Value as string[];

    /// <summary>
    /// 读取当前值的 ElRateColorMap 分支；不属于该分支时返回 null。
    /// </summary>
    public ElRateColorMap? AsMap => Value as ElRateColorMap;

    IEnumerator<string> IEnumerable<string>.GetEnumerator()
        => ((IEnumerable<string>)(AsArray ?? Array.Empty<string>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<string>)this).GetEnumerator();
}

/// <summary>
/// 按评分阈值配置评分图标的映射。
/// </summary>
[ECMAScript]
[Description("@#RateIconMap")]
public sealed record ElRateIconMap : VueDictionary<VueStringComponentValue>
{
}

/// <summary>
/// 用于 ElRate.Icons 的参数类型。
/// icon components. If array, it should have 3 elements, each of which corresponds with a score level, else if object, the key should be threshold value between two levels, and the value should be corresponding icon component
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union ElRateIconsValue(
    VueStringComponentValue[],
    ElRateIconMap) : IEnumerable<VueStringComponentValue>
{
    /// <summary>
    /// 读取当前值的 VueStringComponentValue[] 分支；不属于该分支时返回 null。
    /// </summary>
    public VueStringComponentValue[]? AsArray => Value as VueStringComponentValue[];

    /// <summary>
    /// 读取当前值的 ElRateIconMap 分支；不属于该分支时返回 null。
    /// </summary>
    public ElRateIconMap? AsMap => Value as ElRateIconMap;

    /// <summary>
    /// 将 string[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator ElRateIconsValue(string[] values)
        => new(Array.ConvertAll(values, static value => (VueStringComponentValue)value));

    /// <summary>
    /// 将 IVueComponent[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator ElRateIconsValue(IVueComponent[] values)
        => new(Array.ConvertAll(values, static value => (VueStringComponentValue)value));

    IEnumerator<VueStringComponentValue> IEnumerable<VueStringComponentValue>.GetEnumerator()
        => ((IEnumerable<VueStringComponentValue>)(AsArray ?? Array.Empty<VueStringComponentValue>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VueStringComponentValue>)this).GetEnumerator();
}

/// <summary>
/// 滑块某个数值位置的标记标签及样式。
/// </summary>
[ECMAScript]
[Description("@#SliderMarker")]
public sealed record ElSliderMarker : VueProps
{
    /// <summary>
    /// 附加到当前渲染目标的内联样式。
    /// </summary>
    [Description("@#style")]
    public VueStyleValue? Style { get; init; }

    /// <summary>
    /// 当前选项或单元格显示的标签文本。
    /// </summary>
    [Description("@#label")]
    public VueValue? Label { get; init; }
}

/// <summary>
/// C# 联合参数，允许 string, ElSliderMarker。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取相应分支。
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union ElSliderMarkValue(string, ElSliderMarker)
{
    /// <summary>
    /// 读取当前值的 string 分支；不属于该分支时返回 null。
    /// </summary>
    public string? AsString => Value as string;

    /// <summary>
    /// 读取当前值的 ElSliderMarker 分支；不属于该分支时返回 null。
    /// </summary>
    public ElSliderMarker? AsMarker => Value as ElSliderMarker;
}

/// <summary>
/// 用于 ElSlider.Marks 的参数类型。
/// marks, type of key must be `number` and must in closed interval `[min, max]`, each mark can custom style
/// </summary>
[ECMAScript]
[Description("@#SliderMarks")]
public sealed record ElSliderMarks : VueDictionary<ElSliderMarkValue>
{
}

/// <summary>
/// 虚拟表格排序方向：升序或降序。
/// </summary>
[String]
public enum ElTableV2SortOrder
{
    /// <summary>
    /// 按升序排序；上游取值为 “asc”。
    /// </summary>
    [Description("@#asc")]
    Asc,

    /// <summary>
    /// 按降序排序；上游取值为 “desc”。
    /// </summary>
    [Description("@#desc")]
    Desc
}

/// <summary>
/// 虚拟表格单元格的水平对齐方式。
/// </summary>
[String]
public enum ElTableV2Alignment
{
    /// <summary>
    /// 左侧；上游取值为 “left”。
    /// </summary>
    [Description("@#left")]
    Left,

    /// <summary>
    /// 居中；上游取值为 “center”。
    /// </summary>
    [Description("@#center")]
    Center,

    /// <summary>
    /// 右侧；上游取值为 “right”。
    /// </summary>
    [Description("@#right")]
    Right
}

/// <summary>
/// 虚拟表格固定列所在的左右边缘。
/// </summary>
[String]
public enum ElTableV2FixedDirection
{
    /// <summary>
    /// 左侧；上游取值为 “left”。
    /// </summary>
    [Description("@#left")]
    Left,

    /// <summary>
    /// 右侧；上游取值为 “right”。
    /// </summary>
    [Description("@#right")]
    Right
}

/// <summary>
/// 虚拟表格动态 CSS 类名回调的行、列和单元格数据。
/// </summary>
[ECMAScript]
[Description("@#TableV2ClassContext")]
public sealed record ElTableV2ClassContext : VueProps
{
    /// <summary>
    /// 表格当前的列配置，顺序与渲染列一致。
    /// </summary>
    [Description("@#columns")]
    public ElTableV2Column[]? Columns { get; init; }

    /// <summary>
    /// 当前单元格对应的列配置。
    /// </summary>
    [Description("@#column")]
    public ElTableV2Column? Column { get; init; }

    /// <summary>
    /// 当前列在列集合中的索引，从 0 开始。
    /// </summary>
    [Description("@#columnIndex")]
    public Number? ColumnIndex { get; init; }

    /// <summary>
    /// 当前表头行的索引，从 0 开始。
    /// </summary>
    [Description("@#headerIndex")]
    public Number? HeaderIndex { get; init; }

    /// <summary>
    /// 当前单元格从行数据中提取的值。
    /// </summary>
    [Description("@#cellData")]
    public VueValue? CellData { get; init; }

    /// <summary>
    /// 当前行的原始业务数据。
    /// </summary>
    [Description("@#rowData")]
    public VueValue? RowData { get; init; }

    /// <summary>
    /// 当前行在数据集合中的索引，从 0 开始。
    /// </summary>
    [Description("@#rowIndex")]
    public Number? RowIndex { get; init; }
}

/// <summary>
/// 用于 ElTableV2ClassValue.AsGetter 的回调签名。
/// 读取当前值的 ElTableV2ClassGetter 分支；不属于该分支时返回 null。
/// </summary>
[ECMAScript]
[Description("@#ClassGetter")]
public delegate string ElTableV2ClassGetter(ElTableV2ClassContext context);

/// <summary>
/// 用于 ElTableV2.HeaderClass、ElTableV2.RowClass 的参数类型。
/// Customized class name passed to header wrapper
/// Customized class name passed to row wrapper
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union ElTableV2ClassValue(string, ElTableV2ClassGetter)
{
    /// <summary>
    /// 读取当前值的 string 分支；不属于该分支时返回 null。
    /// </summary>
    public string? AsString => Value as string;

    /// <summary>
    /// 读取当前值的 ElTableV2ClassGetter 分支；不属于该分支时返回 null。
    /// </summary>
    public ElTableV2ClassGetter? AsGetter => Value as ElTableV2ClassGetter;
}

/// <summary>
/// 虚拟表格动态属性回调的当前行列上下文。
/// </summary>
[ECMAScript]
[Description("@#TableV2DynamicPropsContext")]
public sealed record ElTableV2DynamicPropsContext : VueProps
{
    /// <summary>
    /// 表格当前的列配置，顺序与渲染列一致。
    /// </summary>
    [Description("@#columns")]
    public ElTableV2Column[]? Columns { get; init; }

    /// <summary>
    /// 当前单元格对应的列配置。
    /// </summary>
    [Description("@#column")]
    public ElTableV2Column? Column { get; init; }

    /// <summary>
    /// 当前列在列集合中的索引，从 0 开始。
    /// </summary>
    [Description("@#columnIndex")]
    public Number? ColumnIndex { get; init; }

    /// <summary>
    /// 当前表头行的索引，从 0 开始。
    /// </summary>
    [Description("@#headerIndex")]
    public Number? HeaderIndex { get; init; }

    /// <summary>
    /// 当前单元格从行数据中提取的值。
    /// </summary>
    [Description("@#cellData")]
    public VueValue? CellData { get; init; }

    /// <summary>
    /// 当前行的原始业务数据。
    /// </summary>
    [Description("@#rowData")]
    public VueValue? RowData { get; init; }

    /// <summary>
    /// 当前行在数据集合中的索引，从 0 开始。
    /// </summary>
    [Description("@#rowIndex")]
    public Number? RowIndex { get; init; }
}

/// <summary>
/// 用于 ElTableV2DynamicPropsValue.AsGetter 的回调签名。
/// 读取当前值的 ElTableV2DynamicPropsGetter 分支；不属于该分支时返回 null。
/// </summary>
[ECMAScript]
[Description("@#DynamicPropsGetter")]
public delegate VueDictionary ElTableV2DynamicPropsGetter(ElTableV2DynamicPropsContext context);

/// <summary>
/// 用于 ElTableV2.HeaderProps、ElTableV2.HeaderCellProps、ElTableV2.RowProps、ElTableV2.CellProps 的参数类型。
/// Customized props name passed to header component
/// Customized props name passed to header cell component
/// Customized props name passed to row component
/// extra props passed to each cell (except header cells)
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union ElTableV2DynamicPropsValue(VueDictionary, ElTableV2DynamicPropsGetter)
{
    /// <summary>
    /// 读取当前值的 VueDictionary 分支；不属于该分支时返回 null。
    /// </summary>
    public VueDictionary? AsObject => Value as VueDictionary;

    /// <summary>
    /// 读取当前值的 ElTableV2DynamicPropsGetter 分支；不属于该分支时返回 null。
    /// </summary>
    public ElTableV2DynamicPropsGetter? AsGetter => Value as ElTableV2DynamicPropsGetter;
}

/// <summary>
/// 用于 ElTableV2.RowKey、ElTableV2.ExpandedRowKeys、ElTableV2.DefaultExpandedRowKeys 的参数类型。
/// The key of each row, if not provided, will be the index of the row
/// An array of keys for expanded rows, can be used with `v-model`
/// An array of keys for default expanded rows, **NON REACTIVE**
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union ElTableV2KeyValue(VueKey)
{
    /// <summary>
    /// 读取当前值的 VueKey 分支；不属于该分支时返回 null。
    /// </summary>
    public VueKey? AsKey => Value as VueKey;

    /// <summary>
    /// 将 string 值转换为 ElTableV2KeyValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator ElTableV2KeyValue(string value)
        => new((VueKey)value);

    /// <summary>
    /// 将 Symbol 值转换为 ElTableV2KeyValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator ElTableV2KeyValue(Symbol value)
        => new((VueKey)value);

    /// <summary>
    /// 将 Number 值转换为 ElTableV2KeyValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator ElTableV2KeyValue(Number value)
        => new((VueKey)value);

    /// <summary>
    /// 将 byte 值转换为 ElTableV2KeyValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator ElTableV2KeyValue(byte value)
        => new((VueKey)value);

    /// <summary>
    /// 将 sbyte 值转换为 ElTableV2KeyValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator ElTableV2KeyValue(sbyte value)
        => new((VueKey)value);

    /// <summary>
    /// 将 short 值转换为 ElTableV2KeyValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator ElTableV2KeyValue(short value)
        => new((VueKey)value);

    /// <summary>
    /// 将 ushort 值转换为 ElTableV2KeyValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator ElTableV2KeyValue(ushort value)
        => new((VueKey)value);

    /// <summary>
    /// 将 int 值转换为 ElTableV2KeyValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator ElTableV2KeyValue(int value)
        => new((VueKey)value);

    /// <summary>
    /// 将 uint 值转换为 ElTableV2KeyValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator ElTableV2KeyValue(uint value)
        => new((VueKey)value);

    /// <summary>
    /// 将 long 值转换为 ElTableV2KeyValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator ElTableV2KeyValue(long value)
        => new((VueKey)value);

    /// <summary>
    /// 将 ulong 值转换为 ElTableV2KeyValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator ElTableV2KeyValue(ulong value)
        => new((VueKey)value);

    /// <summary>
    /// 将 float 值转换为 ElTableV2KeyValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator ElTableV2KeyValue(float value)
        => new((VueKey)value);

    /// <summary>
    /// 将 double 值转换为 ElTableV2KeyValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator ElTableV2KeyValue(double value)
        => new((VueKey)value);

    /// <summary>
    /// 将 decimal 值转换为 ElTableV2KeyValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator ElTableV2KeyValue(decimal value)
        => new((VueKey)value);
}

/// <summary>
/// 用于 ElTableV2.SortBy 的参数类型。
/// Sort indicator
/// </summary>
[ECMAScript]
[Description("@#SortBy")]
public sealed record ElTableV2SortBy : VueProps
{
    /// <summary>
    /// 此列或排序字段的唯一键。
    /// </summary>
    [Description("@#key")]
    public ElTableV2KeyValue? Key { get; init; }

    /// <summary>
    /// 当前排序方向；空值表示不指定排序方向。
    /// </summary>
    [Description("@#order")]
    public ElTableV2SortOrder? Order { get; init; }
}

/// <summary>
/// 用于 ElTableV2.SortState 的参数类型。
/// Multiple sort indicator
/// </summary>
[ECMAScript]
[Description("@#SortState")]
public sealed record ElTableV2SortState : VueDictionary<ElTableV2SortOrder>
{
}

/// <summary>
/// 用于 ElTableV2.Columns 的参数类型。
/// An array of column definitions.
/// </summary>
[ECMAScript]
[Description("@#Column")]
public sealed record ElTableV2Column : VueDictionary
{
    /// <summary>
    /// 列单元格内容的对齐方向。
    /// </summary>
    [Description("@#align")]
    public ElTableV2Alignment? Align { get; init; }

    /// <summary>
    /// 附加到组件或单元格的 CSS 类名。
    /// </summary>
    [Description("@#class")]
    public ElTableV2ClassValue? CssClass { get; init; }

    /// <summary>
    /// 此列或排序字段的唯一键。
    /// </summary>
    [Description("@#key")]
    public ElTableV2KeyValue? Key { get; init; }

    /// <summary>
    /// 从行数据提取此列单元格值的字段键。
    /// </summary>
    [Description("@#dataKey")]
    public ElTableV2KeyValue? DataKey { get; init; }

    /// <summary>
    /// 是否固定此列，或指定固定在左侧或右侧。
    /// </summary>
    [Description("@#fixed")]
    public ElTableV2FixedValue? Fixed { get; init; }

    /// <summary>
    /// 列标题显示的文本。
    /// </summary>
    [Description("@#title")]
    public string? Title { get; init; }

    /// <summary>
    /// 是否隐藏此列。
    /// </summary>
    [Description("@#hidden")]
    public bool? Hidden { get; init; }

    /// <summary>
    /// 表头单元格的 CSS 类名或根据上下文生成类名的回调。
    /// </summary>
    [Description("@#headerClass")]
    public ElTableV2ClassValue? HeaderClass { get; init; }

    /// <summary>
    /// 列宽允许的最大像素值。
    /// </summary>
    [Description("@#maxWidth")]
    public Number? MaxWidth { get; init; }

    /// <summary>
    /// 列宽允许的最小像素值。
    /// </summary>
    [Description("@#minWidth")]
    public Number? MinWidth { get; init; }

    /// <summary>
    /// 附加到当前渲染目标的内联样式。
    /// </summary>
    [Description("@#style")]
    public VueStyleValue? Style { get; init; }

    /// <summary>
    /// 是否允许用户按此列排序。
    /// </summary>
    [Description("@#sortable")]
    public bool? Sortable { get; init; }

    /// <summary>
    /// 当前列或可用区域的宽度，单位为像素。
    /// </summary>
    [Description("@#width")]
    public Number? Width { get; init; }

    /// <summary>
    /// 分配额外可用宽度时该列的增长比例。
    /// </summary>
    [Description("@#flexGrow")]
    public Number? FlexGrow { get; init; }

    /// <summary>
    /// 可用宽度不足时该列的收缩比例。
    /// </summary>
    [Description("@#flexShrink")]
    public Number? FlexShrink { get; init; }

    /// <summary>
    /// 自定义当前列数据单元格的渲染函数。
    /// </summary>
    [Description("@#cellRenderer")]
    public ElTableV2CellRenderer? CellRenderer { get; init; }

    /// <summary>
    /// 自定义当前列表头单元格的渲染函数。
    /// </summary>
    [Description("@#headerCellRenderer")]
    public ElTableV2HeaderCellRenderer? HeaderCellRenderer { get; init; }
}

/// <summary>
/// C# 联合参数，允许 bool, ElTableV2FixedDirection。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取相应分支。
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union ElTableV2FixedValue(bool, ElTableV2FixedDirection)
{
    /// <summary>
    /// 读取当前值的 bool 分支；不属于该分支时返回 null。
    /// </summary>
    public bool? AsBool => Value is bool value ? value : default(bool?);

    /// <summary>
    /// 读取当前值的 ElTableV2FixedDirection 分支；不属于该分支时返回 null。
    /// </summary>
    public ElTableV2FixedDirection? AsDirection
        => Value is ElTableV2FixedDirection value ? value : default(ElTableV2FixedDirection?);
}

/// <summary>
/// 用于 ElTableV2.Data、ElTableV2.FixedData 的参数类型。
/// An array of data to be rendered in the table.
/// Data for rendering rows above the main content and below the header
/// </summary>
[ECMAScript]
[Description("@#TableData")]
public sealed record ElTableV2DataItem : VueDictionary
{
}

/// <summary>
/// 用于 ElTableV2.HeaderHeight 的参数类型。
/// The height of the header is set by `height`. If given an array, it renders header rows equal to its length
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union ElTableV2HeaderHeightValue(double, Number[]) : IEnumerable<Number>
{
    /// <summary>
    /// 读取当前值的 double 分支；不属于该分支时返回 null。
    /// </summary>
    public double? AsNumber => Value is double value ? value : default(double?);

    /// <summary>
    /// 读取当前值的 Number[] 分支；不属于该分支时返回 null。
    /// </summary>
    public Number[]? AsNumbers => Value is Number[] values ? values : default(Number[]?);

    /// <summary>
    /// 将 double[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator ElTableV2HeaderHeightValue(double[] values)
        => new(Array.ConvertAll(values, static value => (Number)value));

    IEnumerator<Number> IEnumerable<Number>.GetEnumerator()
        => ((IEnumerable<Number>)(AsNumbers ?? Array.Empty<Number>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<Number>)this).GetEnumerator();
}

/// <summary>
/// 虚拟表格自定义单元格取值函数的行列上下文。
/// </summary>
[ECMAScript]
[Description("@#TableV2DataGetterContext")]
public sealed record ElTableV2DataGetterContext : VueProps
{
    /// <summary>
    /// 表格当前的列配置，顺序与渲染列一致。
    /// </summary>
    [Description("@#columns")]
    public ElTableV2Column[]? Columns { get; init; }

    /// <summary>
    /// 当前单元格对应的列配置。
    /// </summary>
    [Description("@#column")]
    public ElTableV2Column? Column { get; init; }

    /// <summary>
    /// 当前列在列集合中的索引，从 0 开始。
    /// </summary>
    [Description("@#columnIndex")]
    public Number? ColumnIndex { get; init; }

    /// <summary>
    /// 当前行的原始业务数据。
    /// </summary>
    [Description("@#rowData")]
    public VueValue? RowData { get; init; }

    /// <summary>
    /// 当前行在数据集合中的索引，从 0 开始。
    /// </summary>
    [Description("@#rowIndex")]
    public Number? RowIndex { get; init; }
}

/// <summary>
/// 用于 ElTableV2.DataGetter 的回调签名。
/// A method to customize data fetch from the data source.
/// </summary>
[ECMAScript]
[Description("@#DataGetter")]
public delegate VueValue ElTableV2DataGetter(ElTableV2DataGetterContext context);

/// <summary>
/// 虚拟表格单元格渲染函数的上下文，包含已提取值和原始行数据。
/// </summary>
[ECMAScript]
[Description("@#TableV2CellRendererContext")]
public sealed record ElTableV2CellRendererContext : VueProps
{
    /// <summary>
    /// 表格当前的列配置，顺序与渲染列一致。
    /// </summary>
    [Description("@#columns")]
    public ElTableV2Column[]? Columns { get; init; }

    /// <summary>
    /// 当前单元格对应的列配置。
    /// </summary>
    [Description("@#column")]
    public ElTableV2Column? Column { get; init; }

    /// <summary>
    /// 当前列在列集合中的索引，从 0 开始。
    /// </summary>
    [Description("@#columnIndex")]
    public Number? ColumnIndex { get; init; }

    /// <summary>
    /// 当前单元格从行数据中提取的值。
    /// </summary>
    [Description("@#cellData")]
    public VueValue? CellData { get; init; }

    /// <summary>
    /// 当前行的原始业务数据。
    /// </summary>
    [Description("@#rowData")]
    public VueValue? RowData { get; init; }

    /// <summary>
    /// 当前行在数据集合中的索引，从 0 开始。
    /// </summary>
    [Description("@#rowIndex")]
    public Number? RowIndex { get; init; }
}

/// <summary>
/// 虚拟表格表头单元格渲染函数的当前列和表头位置。
/// </summary>
[ECMAScript]
[Description("@#TableV2HeaderCellRendererContext")]
public sealed record ElTableV2HeaderCellRendererContext : VueProps
{
    /// <summary>
    /// 表格当前的列配置，顺序与渲染列一致。
    /// </summary>
    [Description("@#columns")]
    public ElTableV2Column[]? Columns { get; init; }

    /// <summary>
    /// 当前单元格对应的列配置。
    /// </summary>
    [Description("@#column")]
    public ElTableV2Column? Column { get; init; }

    /// <summary>
    /// 当前列在列集合中的索引，从 0 开始。
    /// </summary>
    [Description("@#columnIndex")]
    public Number? ColumnIndex { get; init; }

    /// <summary>
    /// 当前表头行的索引，从 0 开始。
    /// </summary>
    [Description("@#headerIndex")]
    public Number? HeaderIndex { get; init; }
}

/// <summary>
/// 根据当前单元格上下文返回用于显示的 Vue VNode。
/// </summary>
[ECMAScript]
[Description("@#CellRenderer")]
public delegate IVNode ElTableV2CellRenderer(ElTableV2CellRendererContext context);

/// <summary>
/// 根据表头上下文返回自定义表头的 Vue VNode。
/// </summary>
[ECMAScript]
[Description("@#HeaderCellRenderer")]
public delegate IVNode ElTableV2HeaderCellRenderer(ElTableV2HeaderCellRendererContext context);

/// <summary>
/// 虚拟表格行事件携带的原生事件、行数据和行标识。
/// </summary>
[ECMAScript]
[Description("@#TableV2RowEventHandlerContext")]
public sealed record ElTableV2RowEventHandlerContext : VueProps
{
    /// <summary>
    /// 当前行的唯一标识。
    /// </summary>
    [Description("@#rowKey")]
    public ElTableV2KeyValue? RowKey { get; init; }

    /// <summary>
    /// 触发当前行回调的原生 DOM 事件。
    /// </summary>
    [Description("@#event")]
    public EventRef? Event { get; init; }

    /// <summary>
    /// 当前行的原始业务数据。
    /// </summary>
    [Description("@#rowData")]
    public VueValue? RowData { get; init; }

    /// <summary>
    /// 当前行在数据集合中的索引，从 0 开始。
    /// </summary>
    [Description("@#rowIndex")]
    public Number? RowIndex { get; init; }
}

/// <summary>
/// 处理虚拟表格的行级鼠标事件；上下文包含原生事件及行数据。
/// </summary>
[ECMAScript]
[Description("@#RowEventHandler")]
public delegate void ElTableV2RowEventHandler(ElTableV2RowEventHandlerContext context);

/// <summary>
/// 用于 ElTableV2.RowEventHandlers 的参数类型。
/// A collection of handlers attached to each row
/// </summary>
[ECMAScript]
[Description("@#RowEventHandlers")]
public sealed record ElTableV2RowEventHandlers : VueProps
{
    /// <summary>
    /// 单击当前行时调用。
    /// </summary>
    [Description("@#onClick")]
    public ElTableV2RowEventHandler? OnClick { get; init; }

    /// <summary>
    /// 在当前行触发上下文菜单时调用，通常来自右键单击。
    /// </summary>
    [Description("@#onContextmenu")]
    public ElTableV2RowEventHandler? OnContextmenu { get; init; }

    /// <summary>
    /// 双击当前行时调用。
    /// </summary>
    [Description("@#onDblclick")]
    public ElTableV2RowEventHandler? OnDblclick { get; init; }

    /// <summary>
    /// 指针进入当前行时调用。
    /// </summary>
    [Description("@#onMouseenter")]
    public ElTableV2RowEventHandler? OnMouseenter { get; init; }

    /// <summary>
    /// 指针离开当前行时调用。
    /// </summary>
    [Description("@#onMouseleave")]
    public ElTableV2RowEventHandler? OnMouseleave { get; init; }
}

/// <summary>
/// C# 联合参数，允许 IVNode, IVNode[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取相应分支。
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union ElTransferRenderContentResult(IVNode, IVNode[])
{
    /// <summary>
    /// 读取当前值的 IVNode 分支；不属于该分支时返回 null。
    /// </summary>
    public IVNode? AsSingle => Value as IVNode;

    /// <summary>
    /// 读取当前值的 IVNode[] 分支；不属于该分支时返回 null。
    /// </summary>
    public IVNode[]? AsMultiple => Value as IVNode[];
}

/// <summary>
/// 用于 ElTransfer.FilterMethod 的回调签名。
/// custom filter method
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate bool ElTransferFilterMethod(string query, ElTransferDataItem item);

/// <summary>
/// 用于 ElTransfer.RenderContent 的回调签名。
/// custom render function for data items
/// </summary>
[ECMAScript]
[Description("@#renderContent")]
public delegate ElTransferRenderContentResult ElTransferRenderContent(VueRenderHost h, ElTransferDataItem option);

/// <summary>
/// Tree 内部节点投影，用于拖放、筛选和懒加载回调；原始业务数据使用 ElTreeNodeData。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record ElTreeNode : VueDictionary
{
}

/// <summary>
/// Tree 的原始节点数据对象，字段名称可通过 props 配置。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record ElTreeNodeData : VueDictionary
{
}

/// <summary>
/// Tree 自定义节点渲染的上下文，包含节点、原始数据及组件内部状态。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record ElTreeRenderContentContext : VueProps
{
    /// <summary>
    /// 树组件为当前数据项创建的内部节点。
    /// </summary>
    [Description("@#node")]
    public ElTreeNode? Node { get; init; }

    /// <summary>
    /// 当前树节点对应的原始业务数据。
    /// </summary>
    [Description("@#data")]
    public ElTreeNodeData? Data { get; init; }

    /// <summary>
    /// 组件内部维护的数据状态对象，供当前渲染回调读取。
    /// </summary>
    [Description("@#store")]
    public VueDictionary? Store { get; init; }

    /// <summary>
    /// 当前渲染回调所属的组件实例。
    /// </summary>
    [Description("@#_self")]
    public VueValue? Self { get; init; }
}

/// <summary>
/// 树节点拖放指示的位置描述。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record ElTreeDropIndicator : VueProps
{
    /// <summary>
    /// 拖放位置类别，用于区分节点前、节点内和节点后。
    /// </summary>
    [Description("@#type")]
    public string? Type { get; init; }
}

/// <summary>
/// 根据原始数据和内部节点返回自定义 CSS 类名或类名开关映射。
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate ElStringBooleanClassValue ElTreeOptionClassCallback(ElTreeNodeData data, ElTreeNode node);

/// <summary>
/// 提交懒加载得到的子节点数据，并结束本次级联节点加载。
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate void ElCascaderLazyResolveCallback(VueDictionary[]? dataList = null);

/// <summary>
/// 通知级联选择器本次懒加载失败并停止加载状态。
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate void ElCascaderLazyRejectCallback();

/// <summary>
/// 用于 ElCascaderProps.LazyLoad 的回调签名。
/// method for loading child nodes data, only works when `lazy` is true. The reject parameter is supported after version ^(2.11.5).
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate void ElCascaderLazyLoadCallback(
    VueDictionary node,
    ElCascaderLazyResolveCallback resolve,
    ElCascaderLazyRejectCallback reject);

/// <summary>
/// 用于 ElTree.RenderContent、ElTreeSelect.RenderContent 的回调签名。
/// render function for tree node
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate ElTransferRenderContentResult ElTreeRenderContentCallback(VueRenderHost h, ElTreeRenderContentContext context);

/// <summary>
/// 向树组件提交懒加载得到的子节点数组。
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate void ElTreeResolveChildrenCallback(ElTreeNodeData[] data);

/// <summary>
/// 结束当前树节点的加载状态，可用于本次加载未成功的情形。
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate void ElTreeStopLoadingCallback();

/// <summary>
/// 用于 ElTree.Load、ElTreeSelect.Load 的回调签名。
/// method for loading subtree data, only works when `lazy` is true
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate void ElTreeLoadCallback(
    ElTreeNode rootNode,
    ElTreeResolveChildrenCallback loadedCallback,
    ElTreeStopLoadingCallback stopLoading);

/// <summary>
/// 用于 ElTree.FilterNodeMethod、ElTreeSelect.FilterNodeMethod 的回调签名。
/// this function will be executed on each node when use filter method. if return `false`, tree node will be hidden.
/// </summary>
/// <param name="value">要传入的值，保持其声明的类型和数据。</param>
/// <param name="data">当前树节点的原始业务数据。</param>
/// <param name="child">当前节点实例，可读取层级、父子关系和展开状态。</param>
[ECMAScript]
[Description("@#")]
public delegate bool ElTreeFilterNodeMethod(VueValue? value, ElTreeNodeData data, ElTreeNode child);

/// <summary>
/// 用于 ElTree.AllowDrag、ElTreeSelect.AllowDrag 的回调签名。
/// this function will be executed before dragging a node. If `false` is returned, the node can not be dragged
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate bool ElTreeAllowDragCallback(ElTreeNode node);

/// <summary>
/// 用于 ElTree.AllowDrop、ElTreeSelect.AllowDrop 的回调签名。
/// this function will be executed before the dragging node is dropped. If `false` is returned, the dragging node can not be dropped at the target node. `type` has three possible values: 'prev' (inserting the dragging node before the target node), 'inner' (inserting the dragging node to the target node) and 'next' (inserting the dragging node after the target node)
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate bool ElTreeAllowDropCallback(
    ElTreeNode draggingNode,
    ElTreeNode dropNode,
    string type);

/// <summary>
/// 用于 ElTreeV2.FilterMethod 的回调签名。
/// this function will be executed on each node when use filter method. if return `false`, tree node will be hidden.
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate bool ElTreeV2FilterMethod(string query, ElTreeNodeData data, ElTreeNode node);

/// <summary>
/// 用于 ElSelect.FilterMethod、ElSelect.RemoteMethod、ElTreeSelect.FilterMethod、ElTreeSelect.RemoteMethod、ElVirtualizedSelect.FilterMethod、ElVirtualizedSelect.RemoteMethod 的回调签名。
/// custom filter method, the first parameter is the current input value. To use this, `filterable` must be true
/// function that gets called when the input value changes. Its parameter is the current input value. To use this, `filterable` must be true
/// custom filter method, the first parameter is the current input value. To use this, `filterable` must be true method
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate void ElSelectQueryCallback(string query);

/// <summary>
/// 用于 ElSlider.FormatTooltip 的回调签名。
/// format to display tooltip value
/// </summary>
/// <param name="value">要传入的值，保持其声明的类型和数据。</param>
[ECMAScript]
[Description("@#")]
public delegate VueStringNumberValue ElSliderFormatTooltipCallback(Number value);

/// <summary>
/// 用于 ElSlider.FormatValueText 的回调签名。
/// format to display the `aria-valuenow` attribute for screen readers
/// </summary>
/// <param name="value">要传入的值，保持其声明的类型和数据。</param>
[ECMAScript]
[Description("@#")]
public delegate string ElSliderFormatValueTextCallback(Number value);

/// <summary>
/// 用于 ElSwitch.BeforeChange 的回调签名。
/// before-change hook before the switch state changes. If `false` is returned or a `Promise` is returned and then is rejected, will stop switching
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate ElAsyncBooleanResult ElSwitchBeforeChangeCallback();

/// <summary>
/// 标签页切换前异步确认；拒绝 Promise 或返回 false 时阻止离开当前标签页。
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate IPromise<bool?> ElTabsBeforeLeaveAsyncCallback(VueStringNumberValue? newName, VueStringNumberValue? oldName);

/// <summary>
/// 标签页切换前同步确认；返回 false 时阻止切换。
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate bool? ElTabsBeforeLeaveSyncCallback(VueStringNumberValue? newName, VueStringNumberValue? oldName);

/// <summary>
/// C# 联合参数，允许 bool, IPromise&lt;bool?&gt;。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取相应分支。
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union ElTabsBeforeLeaveResult(bool, IPromise<bool?>)
{
    /// <summary>
    /// 读取当前值的 bool 分支；不属于该分支时返回 null。
    /// </summary>
    public bool? AsBool => Value is bool value ? value : default(bool?);

    /// <summary>
    /// 读取当前值的 IPromise&lt;bool?&gt; 分支；不属于该分支时返回 null。
    /// </summary>
    public IPromise<bool?>? AsPromise => Value as IPromise<bool?>;
}

/// <summary>
/// 用于 ElTabs.BeforeLeave 的回调签名。
/// hook function before switching tab. If `false` is returned or a `Promise` is returned and then is rejected, switching will be prevented
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate ElTabsBeforeLeaveResult? ElTabsBeforeLeaveCallback(
    VueStringNumberValue? newName,
    VueStringNumberValue? oldName);

/// <summary>
/// 带 percent 百分比字段的上传进度事件。
/// </summary>
[ECMAScript]
[Description("@#UploadProgressEvent")]
public sealed record ElUploadProgressEvent : VueProps
{
    /// <summary>
    /// 本次上传请求已经完成的百分比。
    /// </summary>
    [Description("@#percent")]
    public Number? Percent { get; init; }
}

/// <summary>
/// 上传请求错误，携带错误名称、消息及可用的请求错误字段。
/// </summary>
[ECMAScript]
[Description("@#UploadError")]
public sealed record ElUploadAjaxError : VueProps
{
    /// <summary>
    /// 上传错误的类型名称。
    /// </summary>
    [Description("@#name")]
    public string? Name { get; init; }

    /// <summary>
    /// 上传请求失败的错误消息。
    /// </summary>
    [Description("@#message")]
    public string? Message { get; init; }
}

/// <summary>
/// 上传时附加的表单字段映射。
/// </summary>
[ECMAScript]
[Description("@#UploadRequestData")]
public sealed record ElUploadRequestData : VueDictionary<VueValue>
{
}

/// <summary>
/// C# 联合参数，允许 Headers, VueDictionary。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取相应分支。
/// </summary>
[ECMAScript]
[Description("@#UploadRequestHeaders")]
public readonly union ElUploadRequestHeaders(Headers, VueDictionary)
{
    /// <summary>
    /// 读取当前值的 Headers 分支；不属于该分支时返回 null。
    /// </summary>
    public Headers? AsHeaders => Value as Headers;

    /// <summary>
    /// 读取当前值的 VueDictionary 分支；不属于该分支时返回 null。
    /// </summary>
    public VueDictionary? AsDictionary => Value as VueDictionary;
}

/// <summary>
/// 自定义上传请求的完整参数，包含文件、URL、请求头和状态回调。
/// </summary>
[ECMAScript]
[Description("@#UploadRequestOptions")]
public sealed record ElUploadRequestOptions : VueProps
{
    /// <summary>
    /// 上传请求发送到的目标 URL。
    /// </summary>
    [Description("@#action")]
    public string Action { get; init; } = string.Empty;

    /// <summary>
    /// 上传请求的 HTTP 方法，例如 POST。
    /// </summary>
    [Description("@#method")]
    public string Method { get; init; } = string.Empty;

    /// <summary>
    /// 与文件一同提交的附加表单字段。
    /// </summary>
    [Description("@#data")]
    public ElUploadRequestData Data { get; init; } = new();

    /// <summary>
    /// 表单上传请求中承载文件的字段名称。
    /// </summary>
    [Description("@#filename")]
    public string Filename { get; init; } = string.Empty;

    /// <summary>
    /// 本次请求上传的原始文件。
    /// </summary>
    [Description("@#file")]
    public ElUploadRawFile File { get; init; } = default!;

    /// <summary>
    /// 附加到上传请求的 HTTP 请求头。
    /// </summary>
    [Description("@#headers")]
    public ElUploadRequestHeaders? Headers { get; init; }

    /// <summary>
    /// 请求失败时调用，用于同步组件中的失败状态。
    /// </summary>
    [Description("@#onError")]
    public ElUploadRequestOnErrorCallback? OnError { get; init; }

    /// <summary>
    /// 上传进度变化时调用，用于同步进度显示。
    /// </summary>
    [Description("@#onProgress")]
    public ElUploadRequestOnProgressCallback? OnProgress { get; init; }

    /// <summary>
    /// 请求成功时调用，将响应数据传回组件。
    /// </summary>
    [Description("@#onSuccess")]
    public ElUploadRequestOnSuccessCallback? OnSuccess { get; init; }

    /// <summary>
    /// 是否让上传请求携带跨源凭据。
    /// </summary>
    [Description("@#withCredentials")]
    public bool? WithCredentials { get; init; }
}

/// <summary>
/// 向上传组件报告请求失败，以更新列表中的错误状态。
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate void ElUploadRequestOnErrorCallback(ElUploadAjaxError error);

/// <summary>
/// 向上传组件报告最新上传进度。
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate void ElUploadRequestOnProgressCallback(ElUploadProgressEvent @event);

/// <summary>
/// 向上传组件提交请求成功的响应数据。
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate void ElUploadRequestOnSuccessCallback(VueValue? response);

/// <summary>
/// C# 联合参数，允许 XMLHttpRequest, IPromise&lt;VueValue?&gt;。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取相应分支。
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union ElUploadRequestResult(XMLHttpRequest, IPromise<VueValue?>)
{
    /// <summary>
    /// 读取当前值的 XMLHttpRequest 分支；不属于该分支时返回 null。
    /// </summary>
    public XMLHttpRequest? AsRequest => Value as XMLHttpRequest;

    /// <summary>
    /// 读取当前值的 IPromise&lt;VueValue?&gt; 分支；不属于该分支时返回 null。
    /// </summary>
    public IPromise<VueValue?>? AsPromise => Value as IPromise<VueValue?>;
}

/// <summary>
/// 用于 ElUpload.HttpRequest 的回调签名。
/// override default xhr behavior, allowing you to implement your own upload-file's request.
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate ElUploadRequestResult ElUploadRequestCallback(ElUploadRequestOptions options);

/// <summary>
/// 上传文件附带的数据字段，可由同步或异步数据工厂产生。
/// </summary>
[ECMAScript]
[Description("@#UploadData")]
public sealed record ElUploadData : VueDictionary<VueValue>
{
}

/// <summary>
/// 用于 ElUploadDataValue.AsAsyncFactory 的回调签名。
/// 读取当前值的 ElUploadDataPromiseFactory 分支；不属于该分支时返回 null。
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate IPromise<ElUploadData> ElUploadDataPromiseFactory(ElUploadRawFile rawFile);

/// <summary>
/// 用于 ElUploadDataValue.AsFactory 的回调签名。
/// 读取当前值的 ElUploadDataFactory 分支；不属于该分支时返回 null。
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate ElUploadData ElUploadDataFactory(ElUploadRawFile rawFile);

/// <summary>
/// 用于 ElUpload.Data 的参数类型。
/// additions options of request. support `Awaitable` data and `Function` since v2.3.13.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union ElUploadDataValue(
    ElUploadData,
    IPromise<ElUploadData>,
    ElUploadDataFactory,
    ElUploadDataPromiseFactory)
{
    /// <summary>
    /// 读取当前值的 ElUploadData 分支；不属于该分支时返回 null。
    /// </summary>
    public ElUploadData? AsData => Value as ElUploadData;

    /// <summary>
    /// 读取当前值的 IPromise&lt;ElUploadData&gt; 分支；不属于该分支时返回 null。
    /// </summary>
    public IPromise<ElUploadData>? AsPromise => Value as IPromise<ElUploadData>;

    /// <summary>
    /// 读取当前值的 ElUploadDataFactory 分支；不属于该分支时返回 null。
    /// </summary>
    public ElUploadDataFactory? AsFactory => Value as ElUploadDataFactory;

    /// <summary>
    /// 读取当前值的 ElUploadDataPromiseFactory 分支；不属于该分支时返回 null。
    /// </summary>
    public ElUploadDataPromiseFactory? AsAsyncFactory => Value as ElUploadDataPromiseFactory;
}

/// <summary>
/// 上传前钩子的结果；false 阻止上传，File/Blob 替换文件，Promise 用于异步准备。
/// </summary>
[ECMAScript]
[Union]
[Description("@#")]
public readonly struct ElUploadBeforeUploadResult : IUnion
{
    // File derives from Blob, so Value-based native-union projections cannot preserve the authored branch.
    // File 继承 Blob；这里必须保留显式 tag，确保 AsFile 与 AsBlob 不会同时命中。
    private readonly byte _kind;
    private readonly bool? _bool;
    private readonly FileRef? _file;
    private readonly Blob? _blob;
    private readonly IPromise<VueValue?>? _promise;

    /// <summary>
    /// 保存上传前钩子的结果分支；布尔值控制是否继续，文件或 Blob 替换待上传内容，Promise 表示异步准备。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    public ElUploadBeforeUploadResult(bool value)
    {
        _kind = 1;
        _bool = value;
        _file = default;
        _blob = default;
        _promise = default;
    }

    /// <summary>
    /// 保存上传前钩子的结果分支；布尔值控制是否继续，文件或 Blob 替换待上传内容，Promise 表示异步准备。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    public ElUploadBeforeUploadResult(FileRef value)
    {
        _kind = 2;
        _bool = default;
        _file = value;
        _blob = default;
        _promise = default;
    }

    /// <summary>
    /// 保存上传前钩子的结果分支；布尔值控制是否继续，文件或 Blob 替换待上传内容，Promise 表示异步准备。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    public ElUploadBeforeUploadResult(Blob value)
    {
        _kind = 3;
        _bool = default;
        _file = default;
        _blob = value;
        _promise = default;
    }

    // C# forbids user-defined conversions with an interface source, so promise authoring uses new(...).
    // C# 禁止以接口作为用户定义转换源；promise 分支通过 new(...) 显式构造。
    /// <summary>
    /// 保存上传前钩子的结果分支；布尔值控制是否继续，文件或 Blob 替换待上传内容，Promise 表示异步准备。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    public ElUploadBeforeUploadResult(IPromise<VueValue?> value)
    {
        _kind = 4;
        _bool = default;
        _file = default;
        _blob = default;
        _promise = value;
    }

    /// <summary>
    /// 读取当前值的 bool 分支；不属于该分支时返回 null。
    /// </summary>
    public bool? AsBool => _kind == 1 ? _bool : default;

    /// <summary>
    /// 读取当前值的 FileRef 分支；不属于该分支时返回 null。
    /// </summary>
    public FileRef? AsFile => _kind == 2 ? _file : default;

    /// <summary>
    /// 读取当前值的 Blob 分支；不属于该分支时返回 null。
    /// </summary>
    public Blob? AsBlob => _kind == 3 ? _blob : default;

    /// <summary>
    /// 读取当前值的 IPromise&lt;VueValue?&gt; 分支；不属于该分支时返回 null。
    /// </summary>
    public IPromise<VueValue?>? AsPromise => _kind == 4 ? _promise : default;

    /// <summary>
    /// 读取当前联合分支保存的原始值，供宿主 API 传递；不会转换到其他分支。
    /// </summary>
    public object? Value => _kind switch
    {
        1 => AsBool,
        2 => AsFile,
        3 => AsBlob,
        4 => AsPromise,
        _ => default
    };

    /// <summary>
    /// 将 bool 值转换为 ElUploadBeforeUploadResult，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator ElUploadBeforeUploadResult(bool value)
        => new(value);

    /// <summary>
    /// 将 FileRef 值转换为 ElUploadBeforeUploadResult，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator ElUploadBeforeUploadResult(FileRef value)
        => new(value);

    /// <summary>
    /// 将 Blob 值转换为 ElUploadBeforeUploadResult，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator ElUploadBeforeUploadResult(Blob value)
        => new(value);

}

/// <summary>
/// 用于 ElUpload.BeforeUpload 的回调签名。
/// hook function before uploading with the file to be uploaded as its parameter. If `false` is returned or a `Promise` is returned and then is rejected, uploading will be aborted.
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate ElUploadBeforeUploadResult? ElUploadBeforeUploadCallback(ElUploadRawFile rawFile);

/// <summary>
/// 删除上传项前同步确认；返回 false 时阻止移除。
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate bool ElUploadBeforeRemoveSyncCallback(ElUploadFile uploadFile, ElUploadFile[] uploadFiles);

/// <summary>
/// 删除上传项前异步确认；返回 false 或拒绝 Promise 时阻止移除。
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate IPromise<bool?> ElUploadBeforeRemoveAsyncCallback(ElUploadFile uploadFile, ElUploadFile[] uploadFiles);

/// <summary>
/// C# 联合参数，允许 bool, IPromise&lt;bool?&gt;。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取相应分支。
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union ElUploadBeforeRemoveResult(bool, IPromise<bool?>)
{
    /// <summary>
    /// 读取当前值的 bool 分支；不属于该分支时返回 null。
    /// </summary>
    public bool? AsBool => Value is bool value ? value : default(bool?);

    /// <summary>
    /// 读取当前值的 IPromise&lt;bool?&gt; 分支；不属于该分支时返回 null。
    /// </summary>
    public IPromise<bool?>? AsPromise => Value as IPromise<bool?>;
}

/// <summary>
/// 用于 ElUpload.BeforeRemove 的回调签名。
/// hook function before removing a file with the file and file list as its parameters. If `false` is returned or a `Promise` is returned and then is rejected, removing will be aborted.
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate ElUploadBeforeRemoveResult ElUploadBeforeRemoveCallback(ElUploadFile uploadFile, ElUploadFile[] uploadFiles);

/// <summary>
/// 用于 ElUpload.OnPreview 的回调签名。
/// hook function when clicking the uploaded files.
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate void ElUploadPreviewCallback(ElUploadFile uploadFile);

/// <summary>
/// 用于 ElUpload.OnRemove、ElUpload.OnChange 的回调签名。
/// hook function when files are removed.
/// hook function when select file or upload file success or upload file fail.
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate void ElUploadFileListCallback(ElUploadFile uploadFile, ElUploadFile[] uploadFiles);

/// <summary>
/// 用于 ElUpload.OnSuccess 的回调签名。
/// hook function when uploaded successfully.
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate void ElUploadSuccessCallback(VueValue? response, ElUploadFile uploadFile, ElUploadFile[] uploadFiles);

/// <summary>
/// 用于 ElUpload.OnProgress 的回调签名。
/// hook function when some progress occurs.
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate void ElUploadProgressCallback(ElUploadProgressEvent @event, ElUploadFile uploadFile, ElUploadFile[] uploadFiles);

/// <summary>
/// 用于 ElUpload.OnError 的回调签名。
/// hook function when some errors occurs.
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate void ElUploadErrorCallback(Error error, ElUploadFile uploadFile, ElUploadFile[] uploadFiles);

/// <summary>
/// 用于 ElUpload.OnExceed 的回调签名。
/// hook function when limit is exceeded.
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate void ElUploadExceedCallback(FileRef[] files, ElUploadUserFile[] uploadFiles);

/// <summary>
/// 用于 ElTimePicker.DisabledHours 的回调签名。
/// To specify the array of hours that cannot be selected
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate Number[] ElTimePickerDisabledHoursCallback(string role, Dayjs? comparingDate = null);

/// <summary>
/// 用于 ElTimePicker.DisabledMinutes 的回调签名。
/// To specify the array of minutes that cannot be selected
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate Number[] ElTimePickerDisabledMinutesCallback(Number hour, string role, Dayjs? comparingDate = null);

/// <summary>
/// 用于 ElTimePicker.DisabledSeconds 的回调签名。
/// To specify the array of seconds that cannot be selected
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate Number[] ElTimePickerDisabledSecondsCallback(Number hour, Number minute, string role, Dayjs? comparingDate = null);

/// <summary>
/// 表格溢出提示格式化回调的行、列与单元格值。
/// </summary>
[ECMAScript]
[Description("@#TableOverflowTooltipData")]
public sealed record ElTableTooltipFormatterContext : VueProps
{
    /// <summary>
    /// 当前行的原始数据对象。
    /// </summary>
    [Description("@#row")]
    public VueDictionary? Row { get; init; }

    /// <summary>
    /// 当前单元格对应的列配置。
    /// </summary>
    [Description("@#column")]
    public ElTableColumnContext? Column { get; init; }

    /// <summary>
    /// 当前单元格要格式化或显示在提示中的值。
    /// </summary>
    [Description("@#cellValue")]
    public VueValue? CellValue { get; init; }
}

/// <summary>
/// 用于 ElTable.TooltipFormatter、ElTableColumn.TooltipFormatter、ElTableConfig.TooltipFormatter 的回调签名。
/// customize tooltip content when using `show-overflow-tooltip`
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate VueStringNumberVNodeValue ElTableTooltipFormatter(ElTableTooltipFormatterContext data);

/// <summary>
/// 表格列在渲染和格式化回调中暴露的列配置。
/// </summary>
[ECMAScript]
[Description("@#TableColumnCtx")]
public sealed record ElTableColumnContext : VueProps
{
    /// <summary>
    /// 当前列的内部唯一标识。
    /// </summary>
    [Description("@#id")]
    public string? Id { get; init; }

    /// <summary>
    /// 当前选项或单元格显示的标签文本。
    /// </summary>
    [Description("@#label")]
    public string? Label { get; init; }

    /// <summary>
    /// 当前列绑定的行数据字段。
    /// </summary>
    [Description("@#property")]
    public string? Property { get; init; }

    /// <summary>
    /// 用于读取行数据字段的属性名称或路径。
    /// </summary>
    [Description("@#prop")]
    public string? Prop { get; init; }

    /// <summary>
    /// 调用方为此列指定的唯一业务键。
    /// </summary>
    [Description("@#columnKey")]
    public string? ColumnKey { get; init; }

    /// <summary>
    /// 表格列类型，如 selection、index 或 expand。
    /// </summary>
    [Description("@#type")]
    public string? Type { get; init; }
}

/// <summary>
/// 表格行样式或类名回调的行数据和行索引。
/// </summary>
[ECMAScript]
[Description("@#TableRowContext")]
public sealed record ElTableRowContext : VueProps
{
    /// <summary>
    /// 当前行的原始数据对象。
    /// </summary>
    [Description("@#row")]
    public VueDictionary? Row { get; init; }

    /// <summary>
    /// 当前行在数据集合中的索引，从 0 开始。
    /// </summary>
    [Description("@#rowIndex")]
    public Number? RowIndex { get; init; }
}

/// <summary>
/// 表格单元格回调的行数据、列配置及行列索引。
/// </summary>
[ECMAScript]
[Description("@#TableCellContext")]
public sealed record ElTableCellContext : VueProps
{
    /// <summary>
    /// 当前行的原始数据对象。
    /// </summary>
    [Description("@#row")]
    public VueDictionary? Row { get; init; }

    /// <summary>
    /// 当前行在数据集合中的索引，从 0 开始。
    /// </summary>
    [Description("@#rowIndex")]
    public Number? RowIndex { get; init; }

    /// <summary>
    /// 当前单元格对应的列配置。
    /// </summary>
    [Description("@#column")]
    public ElTableColumnContext? Column { get; init; }

    /// <summary>
    /// 当前列在列集合中的索引，从 0 开始。
    /// </summary>
    [Description("@#columnIndex")]
    public Number? ColumnIndex { get; init; }
}

/// <summary>
/// 表格汇总函数的全部列配置及参与汇总的行数据。
/// </summary>
[ECMAScript]
[Description("@#SummaryMethodContext")]
public sealed record ElTableSummaryMethodContext : VueProps
{
    /// <summary>
    /// 表格当前的列配置，顺序与渲染列一致。
    /// </summary>
    [Description("@#columns")]
    public ElTableColumnContext[]? Columns { get; init; }

    /// <summary>
    /// 参与汇总计算的表格行数据。
    /// </summary>
    [Description("@#data")]
    public VueDictionary[]? Data { get; init; }
}

/// <summary>
/// C# 联合参数，允许 Number[], ElTableSpanMethodCoordinates。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取相应分支。
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union ElTableSpanMethodResult(Number[], ElTableSpanMethodCoordinates)
{
    /// <summary>
    /// 读取当前值的 Number[] 分支；不属于该分支时返回 null。
    /// </summary>
    public Number[]? AsPair => Value as Number[];

    /// <summary>
    /// 读取当前值的 ElTableSpanMethodCoordinates 分支；不属于该分支时返回 null。
    /// </summary>
    public ElTableSpanMethodCoordinates? AsCoordinates => Value as ElTableSpanMethodCoordinates;
}

/// <summary>
/// 单元格合并函数返回的 rowspan 与 colspan。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record ElTableSpanMethodCoordinates : VueProps
{
    /// <summary>
    /// 当前单元格纵向跨越的行数；0 可用于隐藏被合并的单元格。
    /// </summary>
    [Description("@#rowspan")]
    public Number? Rowspan { get; init; }

    /// <summary>
    /// 当前单元格横向跨越的列数；0 可用于隐藏被合并的单元格。
    /// </summary>
    [Description("@#colspan")]
    public Number? Colspan { get; init; }
}

/// <summary>
/// 用于 ElTableRowClassNameValue.AsCallback 的回调签名。
/// 读取当前值的 ElTableRowClassNameCallback 分支；不属于该分支时返回 null。
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate string ElTableRowClassNameCallback(ElTableRowContext context);

/// <summary>
/// 用于 ElTableRowStyleValue.AsCallback 的回调签名。
/// 读取当前值的 ElTableRowStyleCallback 分支；不属于该分支时返回 null。
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate VueStyleValue ElTableRowStyleCallback(ElTableRowContext context);

/// <summary>
/// 用于 ElTableCellClassNameValue.AsCallback 的回调签名。
/// 读取当前值的 ElTableCellClassNameCallback 分支；不属于该分支时返回 null。
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate string ElTableCellClassNameCallback(ElTableCellContext context);

/// <summary>
/// 用于 ElTableCellStyleValue.AsCallback 的回调签名。
/// 读取当前值的 ElTableCellStyleCallback 分支；不属于该分支时返回 null。
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate VueStyleValue ElTableCellStyleCallback(ElTableCellContext context);

/// <summary>
/// 用于 ElTable.RowClassName、ElTable.HeaderRowClassName 的参数类型。
/// function that returns custom class names for a row, or a string assigning class names for every row
/// function that returns custom class names for a row in table header, or a string assigning class names for every row in table header
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union ElTableRowClassNameValue(string, ElTableRowClassNameCallback)
{
    /// <summary>
    /// 读取当前值的 string 分支；不属于该分支时返回 null。
    /// </summary>
    public string? AsString => Value as string;

    /// <summary>
    /// 读取当前值的 ElTableRowClassNameCallback 分支；不属于该分支时返回 null。
    /// </summary>
    public ElTableRowClassNameCallback? AsCallback => Value as ElTableRowClassNameCallback;
}

/// <summary>
/// 用于 ElTable.RowStyle、ElTable.HeaderRowStyle 的参数类型。
/// function that returns custom style for a row, or an object assigning custom style for every row
/// function that returns custom style for a row in table header, or an object assigning custom style for every row in table header
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union ElTableRowStyleValue(VueStyleValue, ElTableRowStyleCallback)
{
    /// <summary>
    /// 读取当前值的 VueStyleValue 分支；不属于该分支时返回 null。
    /// </summary>
    public VueStyleValue? AsStyle => Value is VueStyleValue value ? value : default(VueStyleValue?);

    /// <summary>
    /// 读取当前值的 ElTableRowStyleCallback 分支；不属于该分支时返回 null。
    /// </summary>
    public ElTableRowStyleCallback? AsCallback => Value as ElTableRowStyleCallback;
}

/// <summary>
/// 用于 ElTable.CellClassName、ElTable.HeaderCellClassName 的参数类型。
/// function that returns custom class names for a cell, or a string assigning class names for every cell
/// function that returns custom class names for a cell in table header, or a string assigning class names for every cell in table header
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union ElTableCellClassNameValue(string, ElTableCellClassNameCallback)
{
    /// <summary>
    /// 读取当前值的 string 分支；不属于该分支时返回 null。
    /// </summary>
    public string? AsString => Value as string;

    /// <summary>
    /// 读取当前值的 ElTableCellClassNameCallback 分支；不属于该分支时返回 null。
    /// </summary>
    public ElTableCellClassNameCallback? AsCallback => Value as ElTableCellClassNameCallback;
}

/// <summary>
/// 用于 ElTable.CellStyle、ElTable.HeaderCellStyle 的参数类型。
/// function that returns custom style for a cell, or an object assigning custom style for every cell
/// function that returns custom style for a cell in table header, or an object assigning custom style for every cell in table header
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union ElTableCellStyleValue(VueStyleValue, ElTableCellStyleCallback)
{
    /// <summary>
    /// 读取当前值的 VueStyleValue 分支；不属于该分支时返回 null。
    /// </summary>
    public VueStyleValue? AsStyle => Value is VueStyleValue value ? value : default(VueStyleValue?);

    /// <summary>
    /// 读取当前值的 ElTableCellStyleCallback 分支；不属于该分支时返回 null。
    /// </summary>
    public ElTableCellStyleCallback? AsCallback => Value as ElTableCellStyleCallback;
}

/// <summary>
/// 用于 ElTableRowKeyValue.AsCallback 的回调签名。
/// 读取当前值的 ElTableRowKeyCallback 分支；不属于该分支时返回 null。
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate string ElTableRowKeyCallback(VueDictionary row);

/// <summary>
/// 用于 ElTable.RowKey 的参数类型。
/// key of row data, used for optimizing rendering. Required if `reserve-selection` is on or display tree data. When its type is String, multi-level access is supported, e.g. `user.info.id`, but `user.info[0].id` is not supported, in which case `Function` should be used
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union ElTableRowKeyValue(string, ElTableRowKeyCallback)
{
    /// <summary>
    /// 读取当前值的 string 分支；不属于该分支时返回 null。
    /// </summary>
    public string? AsString => Value as string;

    /// <summary>
    /// 读取当前值的 ElTableRowKeyCallback 分支；不属于该分支时返回 null。
    /// </summary>
    public ElTableRowKeyCallback? AsCallback => Value as ElTableRowKeyCallback;
}

/// <summary>
/// 用于 ElTable.SummaryMethod 的回调签名。
/// custom summary method
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate VueStringNumberVNodeValue[] ElTableSummaryMethodCallback(ElTableSummaryMethodContext context);

/// <summary>
/// 用于 ElTable.SpanMethod 的回调签名。
/// method that returns rowspan and colspan
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate ElTableSpanMethodResult? ElTableSpanMethodCallback(ElTableCellContext context);

/// <summary>
/// 树形表格懒加载节点的展开、加载和缩进状态。
/// </summary>
[ECMAScript]
[Description("@#TableTreeNode")]
public sealed record ElTableTreeNode : VueProps
{
    /// <summary>
    /// 当前树形行是否已展开。
    /// </summary>
    [Description("@#expanded")]
    public bool? Expanded { get; init; }

    /// <summary>
    /// 当前树形行是否正在懒加载子节点。
    /// </summary>
    [Description("@#loading")]
    public bool? Loading { get; init; }

    /// <summary>
    /// 当前树形行的缩进量。
    /// </summary>
    [Description("@#indent")]
    public Number? Indent { get; init; }

    /// <summary>
    /// 当前树形行的层级。
    /// </summary>
    [Description("@#level")]
    public Number? Level { get; init; }
}

/// <summary>
/// 将异步读取的子行数据交给表格，并完成本次懒加载。
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate void ElTableResolveChildrenCallback(VueDictionary[] data);

/// <summary>
/// 用于 ElTable.Load 的回调签名。
/// method for loading child row data, only works when `lazy` is true
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate void ElTableLoadCallback(VueDictionary row, ElTableTreeNode treeNode, ElTableResolveChildrenCallback resolve);

/// <summary>
/// 用于 ElTable.RowExpandable 的回调签名。
/// enable expandable rows, works when the table has a column type=&quot;expand&quot;
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate bool ElTableRowExpandableCallback(VueDictionary row, Number index);

/// <summary>
/// 表格自定义表头函数的列配置、位置及组件状态。
/// </summary>
[ECMAScript]
[Description("@#TableColumnHeaderContext")]
public sealed record ElTableColumnHeaderContext : VueProps
{
    /// <summary>
    /// 当前单元格对应的列配置。
    /// </summary>
    [Description("@#column")]
    public ElTableColumnContext? Column { get; init; }

    /// <summary>
    /// 当前列的索引，从 0 开始。
    /// </summary>
    [Description("@#$index")]
    public Number? Index { get; init; }

    /// <summary>
    /// 组件内部维护的数据状态对象，供当前渲染回调读取。
    /// </summary>
    [Description("@#store")]
    public VueDictionary? Store { get; init; }

    /// <summary>
    /// 当前渲染回调所属的组件实例。
    /// </summary>
    [Description("@#_self")]
    public VueValue? Self { get; init; }
}

/// <summary>
/// 用于 ElTableColumnIndexValue.AsCallback 的回调签名。
/// 读取当前值的 ElTableColumnIndexCallback 分支；不属于该分支时返回 null。
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate Number ElTableColumnIndexCallback(Number index);

/// <summary>
/// 用于 ElTableColumn.Index 的参数类型。
/// customize indices for each row, works on columns with `type=index`
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union ElTableColumnIndexValue(Number, ElTableColumnIndexCallback)
{
    /// <summary>
    /// 读取当前值的 Number 分支；不属于该分支时返回 null。
    /// </summary>
    public Number? AsNumber => Value is Number value ? value : default(Number?);

    /// <summary>
    /// 读取当前值的 ElTableColumnIndexCallback 分支；不属于该分支时返回 null。
    /// </summary>
    public ElTableColumnIndexCallback? AsCallback => Value as ElTableColumnIndexCallback;
}

/// <summary>
/// 用于 ElTableColumn.RenderHeader 的回调签名。
/// render function for table header of this column
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate IVNode ElTableColumnRenderHeaderCallback(ElTableColumnHeaderContext context);

/// <summary>
/// 用于 ElTableColumn.SortMethod 的回调签名。
/// sorting method, works when `sortable` is `true`. Should return a number, just like Array.sort
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate Number ElTableColumnSortMethodCallback(VueDictionary left, VueDictionary right);

/// <summary>
/// 用于 ElTableColumnSortByValue.AsCallback 的回调签名。
/// 读取当前值的 ElTableColumnSortByCallback 分支；不属于该分支时返回 null。
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate string ElTableColumnSortByCallback(VueDictionary row, Number index, VueDictionary[]? array = null);

/// <summary>
/// 用于 ElTableColumn.SortBy 的参数类型。
/// specify which property to sort by, works when `sortable` is `true` and `sort-method` is `undefined`. If set to an Array, the column will sequentially sort by the next property if the previous one is equal
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union ElTableColumnSortByValue(
    string,
    string[],
    ElTableColumnSortByCallback)
{
    /// <summary>
    /// 读取当前值的 string 分支；不属于该分支时返回 null。
    /// </summary>
    public string? AsString => Value as string;

    /// <summary>
    /// 读取当前值的 string[] 分支；不属于该分支时返回 null。
    /// </summary>
    public string[]? AsStrings => Value as string[];

    /// <summary>
    /// 读取当前值的 ElTableColumnSortByCallback 分支；不属于该分支时返回 null。
    /// </summary>
    public ElTableColumnSortByCallback? AsCallback => Value as ElTableColumnSortByCallback;
}

/// <summary>
/// 用于 ElTableColumn.Formatter 的回调签名。
/// function that formats cell content
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate VueStringNumberVNodeValue ElTableColumnFormatterCallback(
    VueDictionary row,
    ElTableColumnContext column,
    VueValue? cellValue,
    Number index);

/// <summary>
/// 用于 ElTableColumn.Selectable 的回调签名。
/// function that determines if a certain row can be selected, works when `type` is 'selection'
/// </summary>
[ECMAScript]
[Description("@#")]
public delegate bool ElTableColumnSelectableCallback(VueDictionary row, Number index);

/// <summary>
/// 用于 ElTableColumn.FilterMethod 的回调签名。
/// data filtering method. If `filter-multiple` is on, this method will be called multiple times for each row, and a row will display if one of the calls returns `true`
/// </summary>
/// <param name="value">要传入的值，保持其声明的类型和数据。</param>
/// <param name="row">当前待判断的行数据。</param>
/// <param name="column">配置此过滤器的表格列上下文。</param>
[ECMAScript]
[Description("@#")]
public delegate void ElTableColumnFilterMethodCallback(string value, VueDictionary row, ElTableColumnContext column);

/// <summary>
/// 虚拟选择器的选项数据；支持由用户输入创建的候选项。
/// </summary>
[ECMAScript]
[Description("@#SelectV2Option")]
public sealed record ElSelectV2Option : VueDictionary
{
    /// <summary>
    /// 此选项是否由允许创建选项的输入操作产生。
    /// </summary>
    [Description("@#created")]
    public bool? Created { get; init; }
}

/// <summary>
/// 虚拟选择器的选项分组，包含组标签和子选项。
/// </summary>
[ECMAScript]
[Description("@#OptionGroup")]
public sealed record ElSelectV2OptionGroup : VueDictionary
{
}

/// <summary>
/// 用于 ElVirtualizedSelect.Options 的参数类型。
/// data of the options, the key of `value` and `label` can be customize by `props`
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union ElSelectV2OptionValue(
    ElSelectV2Option,
    ElSelectV2OptionGroup)
{
    /// <summary>
    /// 读取当前值的 ElSelectV2Option 分支；不属于该分支时返回 null。
    /// </summary>
    public ElSelectV2Option? AsOption => Value as ElSelectV2Option;

    /// <summary>
    /// 读取当前值的 ElSelectV2OptionGroup 分支；不属于该分支时返回 null。
    /// </summary>
    public ElSelectV2OptionGroup? AsGroup => Value as ElSelectV2OptionGroup;
}

/// <summary>
/// 用于 ElVirtualizedSelect.ModelValue、ElVirtualizedSelect.ModelValueChanged 的参数类型。
/// binding value
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union ElSelectV2ModelValue(VueValue, VueValue[]) : IEnumerable<VueValue>
{
    /// <summary>
    /// 读取当前值的 VueValue 分支；不属于该分支时返回 null。
    /// </summary>
    public VueValue? AsSingle => Value as VueValue;

    /// <summary>
    /// 读取当前值的 VueValue[] 分支；不属于该分支时返回 null。
    /// </summary>
    public VueValue[]? AsMultiple => Value as VueValue[];

    IEnumerator<VueValue> IEnumerable<VueValue>.GetEnumerator()
        => ((IEnumerable<VueValue>)(AsMultiple ?? Array.Empty<VueValue>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VueValue>)this).GetEnumerator();
}
