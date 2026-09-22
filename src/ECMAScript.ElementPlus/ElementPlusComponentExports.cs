#nullable enable

namespace ECMAScript.ElementPlus;

/// <summary>
/// Export surface for generated Element Plus components.
/// </summary>
[ECMAScript("element-plus/es/index.mjs")]
public static class ElComponents
{
    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElAffix"/>。</summary>
    /// <remarks>Fix the element to a specific visible area.</remarks>
    [ECMAScript("element-plus/es/components/affix/index.mjs")]
    [Style("element-plus/es/components/affix/style/css.mjs")]
    [ECMAScriptName("ElAffix")]
    public extern static IElementPlusComponent ElAffix { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElAlert"/>。</summary>
    /// <remarks>Displays important alert messages.</remarks>
    [ECMAScript("element-plus/es/components/alert/index.mjs")]
    [Style("element-plus/es/components/alert/style/css.mjs")]
    [ECMAScriptName("ElAlert")]
    public extern static IElementPlusComponent ElAlert { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElAnchor"/>。</summary>
    /// <remarks>Through the anchor point, you can quickly find the position of the information content on the current page.</remarks>
    [ECMAScript("element-plus/es/components/anchor/index.mjs")]
    [Style("element-plus/es/components/anchor/style/css.mjs")]
    [ECMAScriptName("ElAnchor")]
    public extern static IElementPlusComponent ElAnchor { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElAnchorLink"/>。</summary>
    /// <remarks>el-anchor-link</remarks>
    [ECMAScript("element-plus/es/components/anchor/index.mjs")]
    [Style("element-plus/es/components/anchor/style/css.mjs")]
    [ECMAScriptName("ElAnchorLink")]
    public extern static IElementPlusComponent ElAnchorLink { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElAside"/>。</summary>
    /// <remarks>el-aside</remarks>
    [ECMAScript("element-plus/es/components/container/index.mjs")]
    [Style("element-plus/es/components/container/style/css.mjs")]
    [ECMAScriptName("ElAside")]
    public extern static IElementPlusComponent ElAside { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElAutoResizer"/>。</summary>
    /// <remarks>ElAutoResizer</remarks>
    [ECMAScript("element-plus/es/components/table-v2/index.mjs")]
    [Style("element-plus/es/components/table-v2/style/css.mjs")]
    [ECMAScriptName("ElAutoResizer")]
    public extern static IElementPlusComponent ElAutoResizer { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElAutocomplete"/>。</summary>
    /// <remarks>Get some recommended tips based on the current input.</remarks>
    [ECMAScript("element-plus/es/components/autocomplete/index.mjs")]
    [Style("element-plus/es/components/autocomplete/style/css.mjs")]
    [ECMAScriptName("ElAutocomplete")]
    public extern static IElementPlusComponent ElAutocomplete { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElAvatar"/>。</summary>
    /// <remarks>Avatars can be used to represent people or objects. It supports images, Icons, or characters.</remarks>
    [ECMAScript("element-plus/es/components/avatar/index.mjs")]
    [Style("element-plus/es/components/avatar/style/css.mjs")]
    [ECMAScriptName("ElAvatar")]
    public extern static IElementPlusComponent ElAvatar { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElAvatarGroup"/>。</summary>
    /// <remarks>el-avatar-group</remarks>
    [ECMAScript("element-plus/es/components/avatar/index.mjs")]
    [Style("element-plus/es/components/avatar/style/css.mjs")]
    [ECMAScriptName("ElAvatarGroup")]
    public extern static IElementPlusComponent ElAvatarGroup { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElBacktop"/>。</summary>
    /// <remarks>A button to back to top.</remarks>
    [ECMAScript("element-plus/es/components/backtop/index.mjs")]
    [Style("element-plus/es/components/backtop/style/css.mjs")]
    [ECMAScriptName("ElBacktop")]
    public extern static IElementPlusComponent ElBacktop { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElBadge"/>。</summary>
    /// <remarks>A number or status mark on buttons and icons.</remarks>
    [ECMAScript("element-plus/es/components/badge/index.mjs")]
    [Style("element-plus/es/components/badge/style/css.mjs")]
    [ECMAScriptName("ElBadge")]
    public extern static IElementPlusComponent ElBadge { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElBreadcrumb"/>。</summary>
    /// <remarks>Displays the location of the current page, making it easier to browser back.</remarks>
    [ECMAScript("element-plus/es/components/breadcrumb/index.mjs")]
    [Style("element-plus/es/components/breadcrumb/style/css.mjs")]
    [ECMAScriptName("ElBreadcrumb")]
    public extern static IElementPlusComponent ElBreadcrumb { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElBreadcrumbItem"/>。</summary>
    /// <remarks>el-breadcrumb-item</remarks>
    [ECMAScript("element-plus/es/components/breadcrumb/index.mjs")]
    [Style("element-plus/es/components/breadcrumb/style/css.mjs")]
    [ECMAScriptName("ElBreadcrumbItem")]
    public extern static IElementPlusComponent ElBreadcrumbItem { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElButton"/>。</summary>
    /// <remarks>Commonly used button.</remarks>
    [ECMAScript("element-plus/es/components/button/index.mjs")]
    [Style("element-plus/es/components/button/style/css.mjs")]
    [ECMAScriptName("ElButton")]
    public extern static IElementPlusComponent ElButton { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElButtonGroup"/>。</summary>
    /// <remarks>el-button-group</remarks>
    [ECMAScript("element-plus/es/components/button/index.mjs")]
    [Style("element-plus/es/components/button/style/css.mjs")]
    [ECMAScriptName("ElButtonGroup")]
    public extern static IElementPlusComponent ElButtonGroup { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElCalendar"/>。</summary>
    /// <remarks>Display date.</remarks>
    [ECMAScript("element-plus/es/components/calendar/index.mjs")]
    [Style("element-plus/es/components/calendar/style/css.mjs")]
    [ECMAScriptName("ElCalendar")]
    public extern static IElementPlusComponent ElCalendar { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElCard"/>。</summary>
    /// <remarks>Integrate information in a card container.</remarks>
    [ECMAScript("element-plus/es/components/card/index.mjs")]
    [Style("element-plus/es/components/card/style/css.mjs")]
    [ECMAScriptName("ElCard")]
    public extern static IElementPlusComponent ElCard { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElCarousel"/>。</summary>
    /// <remarks>Loop a series of images or texts in a limited space</remarks>
    [ECMAScript("element-plus/es/components/carousel/index.mjs")]
    [Style("element-plus/es/components/carousel/style/css.mjs")]
    [ECMAScriptName("ElCarousel")]
    public extern static IElementPlusComponent ElCarousel { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElCarouselItem"/>。</summary>
    /// <remarks>el-carousel-item</remarks>
    [ECMAScript("element-plus/es/components/carousel/index.mjs")]
    [Style("element-plus/es/components/carousel/style/css.mjs")]
    [ECMAScriptName("ElCarouselItem")]
    public extern static IElementPlusComponent ElCarouselItem { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElCascader"/>。</summary>
    /// <remarks>If the options have a clear hierarchical structure, Cascader can be used to view and select them.</remarks>
    [ECMAScript("element-plus/es/components/cascader/index.mjs")]
    [Style("element-plus/es/components/cascader/style/css.mjs")]
    [ECMAScriptName("ElCascader")]
    public extern static IElementPlusComponent ElCascader { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElCascaderPanel"/>。</summary>
    /// <remarks>el-cascader-panel</remarks>
    [ECMAScript("element-plus/es/components/cascader-panel/index.mjs")]
    [Style("element-plus/es/components/cascader-panel/style/css.mjs")]
    [ECMAScriptName("ElCascaderPanel")]
    public extern static IElementPlusComponent ElCascaderPanel { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElCheckTag"/>。</summary>
    /// <remarks>el-check-tag</remarks>
    [ECMAScript("element-plus/es/components/check-tag/index.mjs")]
    [Style("element-plus/es/components/check-tag/style/css.mjs")]
    [ECMAScriptName("ElCheckTag")]
    public extern static IElementPlusComponent ElCheckTag { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElCheckbox"/>。</summary>
    /// <remarks>A group of options for multiple choices.</remarks>
    [ECMAScript("element-plus/es/components/checkbox/index.mjs")]
    [Style("element-plus/es/components/checkbox/style/css.mjs")]
    [ECMAScriptName("ElCheckbox")]
    public extern static IElementPlusComponent ElCheckbox { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElCheckboxButton"/>。</summary>
    /// <remarks>el-checkbox-button</remarks>
    [ECMAScript("element-plus/es/components/checkbox/index.mjs")]
    [Style("element-plus/es/components/checkbox/style/css.mjs")]
    [ECMAScriptName("ElCheckboxButton")]
    public extern static IElementPlusComponent ElCheckboxButton { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElCheckboxGroup"/>。</summary>
    /// <remarks>el-checkbox-group</remarks>
    [ECMAScript("element-plus/es/components/checkbox/index.mjs")]
    [Style("element-plus/es/components/checkbox/style/css.mjs")]
    [ECMAScriptName("ElCheckboxGroup")]
    public extern static IElementPlusComponent ElCheckboxGroup { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElCol"/>。</summary>
    /// <remarks>el-col</remarks>
    [ECMAScript("element-plus/es/components/col/index.mjs")]
    [Style("element-plus/es/components/col/style/css.mjs")]
    [ECMAScriptName("ElCol")]
    public extern static IElementPlusComponent ElCol { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElCollapse"/>。</summary>
    /// <remarks>Use Collapse to store contents.</remarks>
    [ECMAScript("element-plus/es/components/collapse/index.mjs")]
    [Style("element-plus/es/components/collapse/style/css.mjs")]
    [ECMAScriptName("ElCollapse")]
    public extern static IElementPlusComponent ElCollapse { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElCollapseItem"/>。</summary>
    /// <remarks>el-collapse-item</remarks>
    [ECMAScript("element-plus/es/components/collapse/index.mjs")]
    [Style("element-plus/es/components/collapse/style/css.mjs")]
    [ECMAScriptName("ElCollapseItem")]
    public extern static IElementPlusComponent ElCollapseItem { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElCollapseTransition"/>。</summary>
    /// <remarks>ElCollapseTransition</remarks>
    [ECMAScript("element-plus/es/components/collapse-transition/index.mjs")]
    [Style("element-plus/es/components/collapse-transition/style/css.mjs")]
    [ECMAScriptName("ElCollapseTransition")]
    public extern static IElementPlusComponent ElCollapseTransition { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElColorPicker"/>。</summary>
    /// <remarks>ColorPicker is a color selector supporting multiple color formats.</remarks>
    [ECMAScript("element-plus/es/components/color-picker/index.mjs")]
    [Style("element-plus/es/components/color-picker/style/css.mjs")]
    [ECMAScriptName("ElColorPicker")]
    public extern static IElementPlusComponent ElColorPicker { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElColorPickerPanel"/>。</summary>
    /// <remarks>`ColorPickerPanel` is the core component of `ColorPicker`.</remarks>
    [ECMAScript("element-plus/es/components/color-picker-panel/index.mjs")]
    [Style("element-plus/es/components/color-picker-panel/style/css.mjs")]
    [ECMAScriptName("ElColorPickerPanel")]
    public extern static IElementPlusComponent ElColorPickerPanel { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElConfigProvider"/>。</summary>
    /// <remarks>Config Provider is used for providing global configurations, which enables your entire application to access these configurations everywhere.</remarks>
    [ECMAScript("element-plus/es/components/config-provider/index.mjs")]
    [Style("element-plus/es/components/config-provider/style/css.mjs")]
    [ECMAScriptName("ElConfigProvider")]
    public extern static IElementPlusComponent ElConfigProvider { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElContainer"/>。</summary>
    /// <remarks>Container components for scaffolding basic structure of the page:</remarks>
    [ECMAScript("element-plus/es/components/container/index.mjs")]
    [Style("element-plus/es/components/container/style/css.mjs")]
    [ECMAScriptName("ElContainer")]
    public extern static IElementPlusComponent ElContainer { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElCountdown"/>。</summary>
    /// <remarks>:::demo Countdown component, support to add other components control countdown.</remarks>
    [ECMAScript("element-plus/es/components/countdown/index.mjs")]
    [Style("element-plus/es/components/countdown/style/css.mjs")]
    [ECMAScriptName("ElCountdown")]
    public extern static IElementPlusComponent ElCountdown { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElDatePicker"/>。</summary>
    /// <remarks>Use Date Picker for date input.</remarks>
    [ECMAScript("element-plus/es/components/date-picker/index.mjs")]
    [Style("element-plus/es/components/date-picker/style/css.mjs")]
    [ECMAScriptName("ElDatePicker")]
    public extern static IElementPlusComponent ElDatePicker { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElDatePickerPanel"/>。</summary>
    /// <remarks>`DatePickerPanel` is the core component of `DatePicker`.</remarks>
    [ECMAScript("element-plus/es/components/date-picker-panel/index.mjs")]
    [Style("element-plus/es/components/date-picker-panel/style/css.mjs")]
    [ECMAScriptName("ElDatePickerPanel")]
    public extern static IElementPlusComponent ElDatePickerPanel { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElDescriptions"/>。</summary>
    /// <remarks>Display multiple fields in list form.</remarks>
    [ECMAScript("element-plus/es/components/descriptions/index.mjs")]
    [Style("element-plus/es/components/descriptions/style/css.mjs")]
    [ECMAScriptName("ElDescriptions")]
    public extern static IElementPlusComponent ElDescriptions { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElDescriptionsItem"/>。</summary>
    /// <remarks>el-descriptions-item</remarks>
    [ECMAScript("element-plus/es/components/descriptions/index.mjs")]
    [Style("element-plus/es/components/descriptions/style/css.mjs")]
    [ECMAScriptName("ElDescriptionsItem")]
    public extern static IElementPlusComponent ElDescriptionsItem { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElDialog"/>。</summary>
    /// <remarks>Informs users while preserving the current page state.</remarks>
    [ECMAScript("element-plus/es/components/dialog/index.mjs")]
    [Style("element-plus/es/components/dialog/style/css.mjs")]
    [ECMAScriptName("ElDialog")]
    public extern static IElementPlusComponent ElDialog { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElDivider"/>。</summary>
    /// <remarks>The dividing line that separates the content.</remarks>
    [ECMAScript("element-plus/es/components/divider/index.mjs")]
    [Style("element-plus/es/components/divider/style/css.mjs")]
    [ECMAScriptName("ElDivider")]
    public extern static IElementPlusComponent ElDivider { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElDrawer"/>。</summary>
    /// <remarks>Sometimes, `Dialog` does not always satisfy our requirements, let's say you have a massive form, or you need space to display something like `terms &amp; conditions`, `Drawer` has almost identical API with `Dialog`, but it introduces different user experience.</remarks>
    [ECMAScript("element-plus/es/components/drawer/index.mjs")]
    [Style("element-plus/es/components/drawer/style/css.mjs")]
    [ECMAScriptName("ElDrawer")]
    public extern static IElementPlusComponent ElDrawer { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElDropdown"/>。</summary>
    /// <remarks>Toggleable menu for displaying lists of links and actions.</remarks>
    [ECMAScript("element-plus/es/components/dropdown/index.mjs")]
    [Style("element-plus/es/components/dropdown/style/css.mjs")]
    [ECMAScriptName("ElDropdown")]
    public extern static IElementPlusComponent ElDropdown { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElDropdownItem"/>。</summary>
    /// <remarks>el-dropdown-item</remarks>
    [ECMAScript("element-plus/es/components/dropdown/index.mjs")]
    [Style("element-plus/es/components/dropdown/style/css.mjs")]
    [ECMAScriptName("ElDropdownItem")]
    public extern static IElementPlusComponent ElDropdownItem { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElDropdownMenu"/>。</summary>
    /// <remarks>el-dropdown-menu</remarks>
    [ECMAScript("element-plus/es/components/dropdown/index.mjs")]
    [Style("element-plus/es/components/dropdown/style/css.mjs")]
    [ECMAScriptName("ElDropdownMenu")]
    public extern static IElementPlusComponent ElDropdownMenu { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElEmpty"/>。</summary>
    /// <remarks>Placeholder hints for empty states.</remarks>
    [ECMAScript("element-plus/es/components/empty/index.mjs")]
    [Style("element-plus/es/components/empty/style/css.mjs")]
    [ECMAScriptName("ElEmpty")]
    public extern static IElementPlusComponent ElEmpty { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElFooter"/>。</summary>
    /// <remarks>el-footer</remarks>
    [ECMAScript("element-plus/es/components/container/index.mjs")]
    [Style("element-plus/es/components/container/style/css.mjs")]
    [ECMAScriptName("ElFooter")]
    public extern static IElementPlusComponent ElFooter { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElForm"/>。</summary>
    /// <remarks>Form consists of `input`, `radio`, `select`, `checkbox` and so on. With form, you can collect, verify and submit data.</remarks>
    [ECMAScript("element-plus/es/components/form/index.mjs")]
    [Style("element-plus/es/components/form/style/css.mjs")]
    [ECMAScriptName("ElForm")]
    public extern static IElementPlusComponent ElForm { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElFormItem"/>。</summary>
    /// <remarks>el-form-item</remarks>
    [ECMAScript("element-plus/es/components/form/index.mjs")]
    [Style("element-plus/es/components/form/style/css.mjs")]
    [ECMAScriptName("ElFormItem")]
    public extern static IElementPlusComponent ElFormItem { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElHeader"/>。</summary>
    /// <remarks>el-header</remarks>
    [ECMAScript("element-plus/es/components/container/index.mjs")]
    [Style("element-plus/es/components/container/style/css.mjs")]
    [ECMAScriptName("ElHeader")]
    public extern static IElementPlusComponent ElHeader { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElIcon"/>。</summary>
    /// <remarks>Element Plus provides a set of common icons.</remarks>
    [ECMAScript("element-plus/es/components/icon/index.mjs")]
    [Style("element-plus/es/components/icon/style/css.mjs")]
    [ECMAScriptName("ElIcon")]
    public extern static IElementPlusComponent ElIcon { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElImage"/>。</summary>
    /// <remarks>Besides the native features of img, support lazy load, custom placeholder and load failure, etc.</remarks>
    [ECMAScript("element-plus/es/components/image/index.mjs")]
    [Style("element-plus/es/components/image/style/css.mjs")]
    [ECMAScriptName("ElImage")]
    public extern static IElementPlusComponent ElImage { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElImageViewer"/>。</summary>
    /// <remarks>el-image-viewer</remarks>
    [ECMAScript("element-plus/es/components/image-viewer/index.mjs")]
    [Style("element-plus/es/components/image-viewer/style/css.mjs")]
    [ECMAScriptName("ElImageViewer")]
    public extern static IElementPlusComponent ElImageViewer { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElInput"/>。</summary>
    /// <remarks>Input data using mouse or keyboard.</remarks>
    [ECMAScript("element-plus/es/components/input/index.mjs")]
    [Style("element-plus/es/components/input/style/css.mjs")]
    [ECMAScriptName("ElInput")]
    public extern static IElementPlusComponent ElInput { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElInputNumber"/>。</summary>
    /// <remarks>Input numerical values with a customizable range.</remarks>
    [ECMAScript("element-plus/es/components/input-number/index.mjs")]
    [Style("element-plus/es/components/input-number/style/css.mjs")]
    [ECMAScriptName("ElInputNumber")]
    public extern static IElementPlusComponent ElInputNumber { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElInputOtp"/>。</summary>
    /// <remarks>Used to enter a one-time password</remarks>
    [ECMAScript("element-plus/es/components/input-otp/index.mjs")]
    [Style("element-plus/es/components/input-otp/style/css.mjs")]
    [ECMAScriptName("ElInputOtp")]
    public extern static IElementPlusComponent ElInputOtp { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElInputTag"/>。</summary>
    /// <remarks>The InputTag component allows users to add content as tags.</remarks>
    [ECMAScript("element-plus/es/components/input-tag/index.mjs")]
    [Style("element-plus/es/components/input-tag/style/css.mjs")]
    [ECMAScriptName("ElInputTag")]
    public extern static IElementPlusComponent ElInputTag { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElLink"/>。</summary>
    /// <remarks>Text hyperlink</remarks>
    [ECMAScript("element-plus/es/components/link/index.mjs")]
    [Style("element-plus/es/components/link/style/css.mjs")]
    [ECMAScriptName("ElLink")]
    public extern static IElementPlusComponent ElLink { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElMain"/>。</summary>
    /// <remarks>el-main</remarks>
    [ECMAScript("element-plus/es/components/container/index.mjs")]
    [Style("element-plus/es/components/container/style/css.mjs")]
    [ECMAScriptName("ElMain")]
    public extern static IElementPlusComponent ElMain { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElMention"/>。</summary>
    /// <remarks>Used to mention someone or something in an input.</remarks>
    [ECMAScript("element-plus/es/components/mention/index.mjs")]
    [Style("element-plus/es/components/mention/style/css.mjs")]
    [ECMAScriptName("ElMention")]
    public extern static IElementPlusComponent ElMention { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElMenu"/>。</summary>
    /// <remarks>Menu that provides navigation for your website.</remarks>
    [ECMAScript("element-plus/es/components/menu/index.mjs")]
    [Style("element-plus/es/components/menu/style/css.mjs")]
    [ECMAScriptName("ElMenu")]
    public extern static IElementPlusComponent ElMenu { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElMenuItem"/>。</summary>
    /// <remarks>el-menu-item</remarks>
    [ECMAScript("element-plus/es/components/menu/index.mjs")]
    [Style("element-plus/es/components/menu/style/css.mjs")]
    [ECMAScriptName("ElMenuItem")]
    public extern static IElementPlusComponent ElMenuItem { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElMenuItemGroup"/>。</summary>
    /// <remarks>el-menu-item-group</remarks>
    [ECMAScript("element-plus/es/components/menu/index.mjs")]
    [Style("element-plus/es/components/menu/style/css.mjs")]
    [ECMAScriptName("ElMenuItemGroup")]
    public extern static IElementPlusComponent ElMenuItemGroup { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElOption"/>。</summary>
    /// <remarks>el-option</remarks>
    [ECMAScript("element-plus/es/components/select/index.mjs")]
    [Style("element-plus/es/components/select/style/css.mjs")]
    [ECMAScriptName("ElOption")]
    public extern static IElementPlusComponent ElOption { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElOptionGroup"/>。</summary>
    /// <remarks>el-option-group</remarks>
    [ECMAScript("element-plus/es/components/select/index.mjs")]
    [Style("element-plus/es/components/select/style/css.mjs")]
    [ECMAScriptName("ElOptionGroup")]
    public extern static IElementPlusComponent ElOptionGroup { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElPageHeader"/>。</summary>
    /// <remarks>If path of the page is simple, it is recommended to use PageHeader instead of the Breadcrumb.</remarks>
    [ECMAScript("element-plus/es/components/page-header/index.mjs")]
    [Style("element-plus/es/components/page-header/style/css.mjs")]
    [ECMAScriptName("ElPageHeader")]
    public extern static IElementPlusComponent ElPageHeader { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElPagination"/>。</summary>
    /// <remarks>If you have too much data to display in one page, use pagination.</remarks>
    [ECMAScript("element-plus/es/components/pagination/index.mjs")]
    [Style("element-plus/es/components/pagination/style/css.mjs")]
    [ECMAScriptName("ElPagination")]
    public extern static IElementPlusComponent ElPagination { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElPopconfirm"/>。</summary>
    /// <remarks>A simple confirmation dialog of an element click action.</remarks>
    [ECMAScript("element-plus/es/components/popconfirm/index.mjs")]
    [Style("element-plus/es/components/popconfirm/style/css.mjs")]
    [ECMAScriptName("ElPopconfirm")]
    public extern static IElementPlusComponent ElPopconfirm { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElPopover"/>。</summary>
    /// <remarks>el-popover</remarks>
    [ECMAScript("element-plus/es/components/popover/index.mjs")]
    [Style("element-plus/es/components/popover/style/css.mjs")]
    [ECMAScriptName("ElPopover")]
    public extern static IElementPlusComponent ElPopover { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElPopper"/>。</summary>
    /// <remarks>ElPopper</remarks>
    [ECMAScript("element-plus/es/components/popper/index.mjs")]
    [Style("element-plus/es/components/popper/style/css.mjs")]
    [ECMAScriptName("ElPopper")]
    public extern static IElementPlusComponent ElPopper { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElProgress"/>。</summary>
    /// <remarks>Progress is used to show the progress of current operation, and inform the user the current status.</remarks>
    [ECMAScript("element-plus/es/components/progress/index.mjs")]
    [Style("element-plus/es/components/progress/style/css.mjs")]
    [ECMAScriptName("ElProgress")]
    public extern static IElementPlusComponent ElProgress { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElRadio"/>。</summary>
    /// <remarks>Single selection among multiple options.</remarks>
    [ECMAScript("element-plus/es/components/radio/index.mjs")]
    [Style("element-plus/es/components/radio/style/css.mjs")]
    [ECMAScriptName("ElRadio")]
    public extern static IElementPlusComponent ElRadio { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElRadioButton"/>。</summary>
    /// <remarks>el-radio-button</remarks>
    [ECMAScript("element-plus/es/components/radio/index.mjs")]
    [Style("element-plus/es/components/radio/style/css.mjs")]
    [ECMAScriptName("ElRadioButton")]
    public extern static IElementPlusComponent ElRadioButton { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElRadioGroup"/>。</summary>
    /// <remarks>el-radio-group</remarks>
    [ECMAScript("element-plus/es/components/radio/index.mjs")]
    [Style("element-plus/es/components/radio/style/css.mjs")]
    [ECMAScriptName("ElRadioGroup")]
    public extern static IElementPlusComponent ElRadioGroup { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElRate"/>。</summary>
    /// <remarks>Used for rating</remarks>
    [ECMAScript("element-plus/es/components/rate/index.mjs")]
    [Style("element-plus/es/components/rate/style/css.mjs")]
    [ECMAScriptName("ElRate")]
    public extern static IElementPlusComponent ElRate { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElResult"/>。</summary>
    /// <remarks>Used to give feedback on the result of user's operation or access exception.</remarks>
    [ECMAScript("element-plus/es/components/result/index.mjs")]
    [Style("element-plus/es/components/result/style/css.mjs")]
    [ECMAScriptName("ElResult")]
    public extern static IElementPlusComponent ElResult { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElRow"/>。</summary>
    /// <remarks>el-row</remarks>
    [ECMAScript("element-plus/es/components/row/index.mjs")]
    [Style("element-plus/es/components/row/style/css.mjs")]
    [ECMAScriptName("ElRow")]
    public extern static IElementPlusComponent ElRow { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElScrollbar"/>。</summary>
    /// <remarks>Used to replace the browser's native scrollbar.</remarks>
    [ECMAScript("element-plus/es/components/scrollbar/index.mjs")]
    [Style("element-plus/es/components/scrollbar/style/css.mjs")]
    [ECMAScriptName("ElScrollbar")]
    public extern static IElementPlusComponent ElScrollbar { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElSegmented"/>。</summary>
    /// <remarks>Display multiple options and allow users to select a single option.</remarks>
    [ECMAScript("element-plus/es/components/segmented/index.mjs")]
    [Style("element-plus/es/components/segmented/style/css.mjs")]
    [ECMAScriptName("ElSegmented")]
    public extern static IElementPlusComponent ElSegmented { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElSelect"/>。</summary>
    /// <remarks>When there are plenty of options, use a drop-down menu to display and select desired ones.</remarks>
    [ECMAScript("element-plus/es/components/select/index.mjs")]
    [Style("element-plus/es/components/select/style/css.mjs")]
    [ECMAScriptName("ElSelect")]
    public extern static IElementPlusComponent ElSelect { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElSkeleton"/>。</summary>
    /// <remarks>When loading data, and you need a rich experience for visual and interactions for your end users, you can choose `skeleton`.</remarks>
    [ECMAScript("element-plus/es/components/skeleton/index.mjs")]
    [Style("element-plus/es/components/skeleton/style/css.mjs")]
    [ECMAScriptName("ElSkeleton")]
    public extern static IElementPlusComponent ElSkeleton { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElSkeletonItem"/>。</summary>
    /// <remarks>el-skeleton-item</remarks>
    [ECMAScript("element-plus/es/components/skeleton/index.mjs")]
    [Style("element-plus/es/components/skeleton/style/css.mjs")]
    [ECMAScriptName("ElSkeletonItem")]
    public extern static IElementPlusComponent ElSkeletonItem { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElSlider"/>。</summary>
    /// <remarks>Drag the slider within a fixed range.</remarks>
    [ECMAScript("element-plus/es/components/slider/index.mjs")]
    [Style("element-plus/es/components/slider/style/css.mjs")]
    [ECMAScriptName("ElSlider")]
    public extern static IElementPlusComponent ElSlider { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElSpace"/>。</summary>
    /// <remarks>Even though we have [Divider]</remarks>
    [ECMAScript("element-plus/es/components/space/index.mjs")]
    [Style("element-plus/es/components/space/style/css.mjs")]
    [ECMAScriptName("ElSpace")]
    public extern static IElementPlusComponent ElSpace { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElSplitter"/>。</summary>
    /// <remarks>el-splitter</remarks>
    [ECMAScript("element-plus/es/components/splitter/index.mjs")]
    [Style("element-plus/es/components/splitter/style/css.mjs")]
    [ECMAScriptName("ElSplitter")]
    public extern static IElementPlusComponent ElSplitter { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElSplitterPanel"/>。</summary>
    /// <remarks>el-splitter-panel</remarks>
    [ECMAScript("element-plus/es/components/splitter/index.mjs")]
    [Style("element-plus/es/components/splitter/style/css.mjs")]
    [ECMAScriptName("ElSplitterPanel")]
    public extern static IElementPlusComponent ElSplitterPanel { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElStatistic"/>。</summary>
    /// <remarks>Display statistics.</remarks>
    [ECMAScript("element-plus/es/components/statistic/index.mjs")]
    [Style("element-plus/es/components/statistic/style/css.mjs")]
    [ECMAScriptName("ElStatistic")]
    public extern static IElementPlusComponent ElStatistic { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElStep"/>。</summary>
    /// <remarks>el-step</remarks>
    [ECMAScript("element-plus/es/components/steps/index.mjs")]
    [Style("element-plus/es/components/steps/style/css.mjs")]
    [ECMAScriptName("ElStep")]
    public extern static IElementPlusComponent ElStep { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElSteps"/>。</summary>
    /// <remarks>Guide the user to complete tasks in accordance with the process. Its steps can be set according to the actual application scenario and the number of the steps can't be less than 2.</remarks>
    [ECMAScript("element-plus/es/components/steps/index.mjs")]
    [Style("element-plus/es/components/steps/style/css.mjs")]
    [ECMAScriptName("ElSteps")]
    public extern static IElementPlusComponent ElSteps { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElSubMenu"/>。</summary>
    /// <remarks>el-sub-menu</remarks>
    [ECMAScript("element-plus/es/components/menu/index.mjs")]
    [Style("element-plus/es/components/menu/style/css.mjs")]
    [ECMAScriptName("ElSubMenu")]
    public extern static IElementPlusComponent ElSubMenu { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElSwitch"/>。</summary>
    /// <remarks>Switch is used for switching between two opposing states.</remarks>
    [ECMAScript("element-plus/es/components/switch/index.mjs")]
    [Style("element-plus/es/components/switch/style/css.mjs")]
    [ECMAScriptName("ElSwitch")]
    public extern static IElementPlusComponent ElSwitch { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElTabPane"/>。</summary>
    /// <remarks>el-tab-pane</remarks>
    [ECMAScript("element-plus/es/components/tabs/index.mjs")]
    [Style("element-plus/es/components/tabs/style/css.mjs")]
    [ECMAScriptName("ElTabPane")]
    public extern static IElementPlusComponent ElTabPane { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElTable"/>。</summary>
    /// <remarks>Display multiple data with similar format. You can sort, filter, compare your data in a table.</remarks>
    [ECMAScript("element-plus/es/components/table/index.mjs")]
    [Style("element-plus/es/components/table/style/css.mjs")]
    [ECMAScriptName("ElTable")]
    public extern static IElementPlusComponent ElTable { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElTableColumn"/>。</summary>
    /// <remarks>el-table-column</remarks>
    [ECMAScript("element-plus/es/components/table/index.mjs")]
    [Style("element-plus/es/components/table/style/css.mjs")]
    [ECMAScriptName("ElTableColumn")]
    public extern static IElementPlusComponent ElTableColumn { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElTableV2"/>。</summary>
    /// <remarks>el-table-v2</remarks>
    [ECMAScript("element-plus/es/components/table-v2/index.mjs")]
    [Style("element-plus/es/components/table-v2/style/css.mjs")]
    [ECMAScriptName("ElTableV2")]
    public extern static IElementPlusComponent ElTableV2 { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElTabs"/>。</summary>
    /// <remarks>Divide data collections which are related yet belong to different types.</remarks>
    [ECMAScript("element-plus/es/components/tabs/index.mjs")]
    [Style("element-plus/es/components/tabs/style/css.mjs")]
    [ECMAScriptName("ElTabs")]
    public extern static IElementPlusComponent ElTabs { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElTag"/>。</summary>
    /// <remarks>Used for marking and selection.</remarks>
    [ECMAScript("element-plus/es/components/tag/index.mjs")]
    [Style("element-plus/es/components/tag/style/css.mjs")]
    [ECMAScriptName("ElTag")]
    public extern static IElementPlusComponent ElTag { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElText"/>。</summary>
    /// <remarks>Used for text.</remarks>
    [ECMAScript("element-plus/es/components/text/index.mjs")]
    [Style("element-plus/es/components/text/style/css.mjs")]
    [ECMAScriptName("ElText")]
    public extern static IElementPlusComponent ElText { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElTimePicker"/>。</summary>
    /// <remarks>Use Time Picker for time input.</remarks>
    [ECMAScript("element-plus/es/components/time-picker/index.mjs")]
    [Style("element-plus/es/components/time-picker/style/css.mjs")]
    [ECMAScriptName("ElTimePicker")]
    public extern static IElementPlusComponent ElTimePicker { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElTimeSelect"/>。</summary>
    /// <remarks>Use Time Select for time input.</remarks>
    [ECMAScript("element-plus/es/components/time-select/index.mjs")]
    [Style("element-plus/es/components/time-select/style/css.mjs")]
    [ECMAScriptName("ElTimeSelect")]
    public extern static IElementPlusComponent ElTimeSelect { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElTimeline"/>。</summary>
    /// <remarks>Visually display timeline.</remarks>
    [ECMAScript("element-plus/es/components/timeline/index.mjs")]
    [Style("element-plus/es/components/timeline/style/css.mjs")]
    [ECMAScriptName("ElTimeline")]
    public extern static IElementPlusComponent ElTimeline { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElTimelineItem"/>。</summary>
    /// <remarks>el-timeline-item</remarks>
    [ECMAScript("element-plus/es/components/timeline/index.mjs")]
    [Style("element-plus/es/components/timeline/style/css.mjs")]
    [ECMAScriptName("ElTimelineItem")]
    public extern static IElementPlusComponent ElTimelineItem { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElTooltip"/>。</summary>
    /// <remarks>Display prompt information for mouse hover.</remarks>
    [ECMAScript("element-plus/es/components/tooltip/index.mjs")]
    [Style("element-plus/es/components/tooltip/style/css.mjs")]
    [ECMAScriptName("ElTooltip")]
    public extern static IElementPlusComponent ElTooltip { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElTour"/>。</summary>
    /// <remarks>A popup component for guiding users through a product. Use when you want to guide users through a product.</remarks>
    [ECMAScript("element-plus/es/components/tour/index.mjs")]
    [Style("element-plus/es/components/tour/style/css.mjs")]
    [ECMAScriptName("ElTour")]
    public extern static IElementPlusComponent ElTour { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElTourStep"/>。</summary>
    /// <remarks>el-tour-step</remarks>
    [ECMAScript("element-plus/es/components/tour/index.mjs")]
    [Style("element-plus/es/components/tour/style/css.mjs")]
    [ECMAScriptName("ElTourStep")]
    public extern static IElementPlusComponent ElTourStep { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElTransfer"/>。</summary>
    /// <remarks>el-transfer</remarks>
    [ECMAScript("element-plus/es/components/transfer/index.mjs")]
    [Style("element-plus/es/components/transfer/style/css.mjs")]
    [ECMAScriptName("ElTransfer")]
    public extern static IElementPlusComponent ElTransfer { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElTree"/>。</summary>
    /// <remarks>Display a set of data with hierarchies.</remarks>
    [ECMAScript("element-plus/es/components/tree/index.mjs")]
    [Style("element-plus/es/components/tree/style/css.mjs")]
    [ECMAScriptName("ElTree")]
    public extern static IElementPlusComponent ElTree { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElTreeSelect"/>。</summary>
    /// <remarks>ElTreeSelect</remarks>
    [ECMAScript("element-plus/es/components/tree-select/index.mjs")]
    [Style("element-plus/es/components/tree-select/style/css.mjs")]
    [ECMAScriptName("ElTreeSelect")]
    public extern static IElementPlusComponent ElTreeSelect { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElTreeV2"/>。</summary>
    /// <remarks>el-tree-v2</remarks>
    [ECMAScript("element-plus/es/components/tree-v2/index.mjs")]
    [Style("element-plus/es/components/tree-v2/style/css.mjs")]
    [ECMAScriptName("ElTreeV2")]
    public extern static IElementPlusComponent ElTreeV2 { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElUpload"/>。</summary>
    /// <remarks>Upload files by clicking or drag-and-drop.</remarks>
    [ECMAScript("element-plus/es/components/upload/index.mjs")]
    [Style("element-plus/es/components/upload/style/css.mjs")]
    [ECMAScriptName("ElUpload")]
    public extern static IElementPlusComponent ElUpload { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElVirtualizedSelect"/>。</summary>
    /// <remarks>:::tip</remarks>
    [ECMAScript("element-plus/es/components/select-v2/index.mjs")]
    [Style("element-plus/es/components/select-v2/style/css.mjs")]
    [ECMAScriptName("ElSelectV2")]
    public extern static IElementPlusComponent ElVirtualizedSelect { get; }

    /// <summary>用于 render/h 调用的组件导出；组件参数见 <see cref="ElWatermark"/>。</summary>
    /// <remarks>Add specific text or patterns to the page.</remarks>
    [ECMAScript("element-plus/es/components/watermark/index.mjs")]
    [Style("element-plus/es/components/watermark/style/css.mjs")]
    [ECMAScriptName("ElWatermark")]
    public extern static IElementPlusComponent ElWatermark { get; }

}
