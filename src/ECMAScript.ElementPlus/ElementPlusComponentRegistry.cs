#nullable enable

namespace ECMAScript.ElementPlus;

/// <summary>
/// Registry of generated Element Plus components.
/// </summary>
[ECMAScript]
[Description("@#ElComponentRegistry")]
public sealed record ElComponentRegistry : VueComponentRegistry
{
    /// <summary>按名称注册 <see cref="ElAffix"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>Fix the element to a specific visible area.</remarks>
    [Description("@#ElAffix")]
    public IElementPlusComponent? ElAffix { get; init; }

    /// <summary>按名称注册 <see cref="ElAlert"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>Displays important alert messages.</remarks>
    [Description("@#ElAlert")]
    public IElementPlusComponent? ElAlert { get; init; }

    /// <summary>按名称注册 <see cref="ElAnchor"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>Through the anchor point, you can quickly find the position of the information content on the current page.</remarks>
    [Description("@#ElAnchor")]
    public IElementPlusComponent? ElAnchor { get; init; }

    /// <summary>按名称注册 <see cref="ElAnchorLink"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>el-anchor-link</remarks>
    [Description("@#ElAnchorLink")]
    public IElementPlusComponent? ElAnchorLink { get; init; }

    /// <summary>按名称注册 <see cref="ElAside"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>el-aside</remarks>
    [Description("@#ElAside")]
    public IElementPlusComponent? ElAside { get; init; }

    /// <summary>按名称注册 <see cref="ElAutoResizer"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>ElAutoResizer</remarks>
    [Description("@#ElAutoResizer")]
    public IElementPlusComponent? ElAutoResizer { get; init; }

    /// <summary>按名称注册 <see cref="ElAutocomplete"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>Get some recommended tips based on the current input.</remarks>
    [Description("@#ElAutocomplete")]
    public IElementPlusComponent? ElAutocomplete { get; init; }

    /// <summary>按名称注册 <see cref="ElAvatar"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>Avatars can be used to represent people or objects. It supports images, Icons, or characters.</remarks>
    [Description("@#ElAvatar")]
    public IElementPlusComponent? ElAvatar { get; init; }

    /// <summary>按名称注册 <see cref="ElAvatarGroup"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>el-avatar-group</remarks>
    [Description("@#ElAvatarGroup")]
    public IElementPlusComponent? ElAvatarGroup { get; init; }

    /// <summary>按名称注册 <see cref="ElBacktop"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>A button to back to top.</remarks>
    [Description("@#ElBacktop")]
    public IElementPlusComponent? ElBacktop { get; init; }

    /// <summary>按名称注册 <see cref="ElBadge"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>A number or status mark on buttons and icons.</remarks>
    [Description("@#ElBadge")]
    public IElementPlusComponent? ElBadge { get; init; }

    /// <summary>按名称注册 <see cref="ElBreadcrumb"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>Displays the location of the current page, making it easier to browser back.</remarks>
    [Description("@#ElBreadcrumb")]
    public IElementPlusComponent? ElBreadcrumb { get; init; }

    /// <summary>按名称注册 <see cref="ElBreadcrumbItem"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>el-breadcrumb-item</remarks>
    [Description("@#ElBreadcrumbItem")]
    public IElementPlusComponent? ElBreadcrumbItem { get; init; }

    /// <summary>按名称注册 <see cref="ElButton"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>Commonly used button.</remarks>
    [Description("@#ElButton")]
    public IElementPlusComponent? ElButton { get; init; }

    /// <summary>按名称注册 <see cref="ElButtonGroup"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>el-button-group</remarks>
    [Description("@#ElButtonGroup")]
    public IElementPlusComponent? ElButtonGroup { get; init; }

    /// <summary>按名称注册 <see cref="ElCalendar"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>Display date.</remarks>
    [Description("@#ElCalendar")]
    public IElementPlusComponent? ElCalendar { get; init; }

    /// <summary>按名称注册 <see cref="ElCard"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>Integrate information in a card container.</remarks>
    [Description("@#ElCard")]
    public IElementPlusComponent? ElCard { get; init; }

    /// <summary>按名称注册 <see cref="ElCarousel"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>Loop a series of images or texts in a limited space</remarks>
    [Description("@#ElCarousel")]
    public IElementPlusComponent? ElCarousel { get; init; }

    /// <summary>按名称注册 <see cref="ElCarouselItem"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>el-carousel-item</remarks>
    [Description("@#ElCarouselItem")]
    public IElementPlusComponent? ElCarouselItem { get; init; }

    /// <summary>按名称注册 <see cref="ElCascader"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>If the options have a clear hierarchical structure, Cascader can be used to view and select them.</remarks>
    [Description("@#ElCascader")]
    public IElementPlusComponent? ElCascader { get; init; }

    /// <summary>按名称注册 <see cref="ElCascaderPanel"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>el-cascader-panel</remarks>
    [Description("@#ElCascaderPanel")]
    public IElementPlusComponent? ElCascaderPanel { get; init; }

    /// <summary>按名称注册 <see cref="ElCheckTag"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>el-check-tag</remarks>
    [Description("@#ElCheckTag")]
    public IElementPlusComponent? ElCheckTag { get; init; }

    /// <summary>按名称注册 <see cref="ElCheckbox"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>A group of options for multiple choices.</remarks>
    [Description("@#ElCheckbox")]
    public IElementPlusComponent? ElCheckbox { get; init; }

    /// <summary>按名称注册 <see cref="ElCheckboxButton"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>el-checkbox-button</remarks>
    [Description("@#ElCheckboxButton")]
    public IElementPlusComponent? ElCheckboxButton { get; init; }

    /// <summary>按名称注册 <see cref="ElCheckboxGroup"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>el-checkbox-group</remarks>
    [Description("@#ElCheckboxGroup")]
    public IElementPlusComponent? ElCheckboxGroup { get; init; }

    /// <summary>按名称注册 <see cref="ElCol"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>el-col</remarks>
    [Description("@#ElCol")]
    public IElementPlusComponent? ElCol { get; init; }

    /// <summary>按名称注册 <see cref="ElCollapse"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>Use Collapse to store contents.</remarks>
    [Description("@#ElCollapse")]
    public IElementPlusComponent? ElCollapse { get; init; }

    /// <summary>按名称注册 <see cref="ElCollapseItem"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>el-collapse-item</remarks>
    [Description("@#ElCollapseItem")]
    public IElementPlusComponent? ElCollapseItem { get; init; }

    /// <summary>按名称注册 <see cref="ElCollapseTransition"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>ElCollapseTransition</remarks>
    [Description("@#ElCollapseTransition")]
    public IElementPlusComponent? ElCollapseTransition { get; init; }

    /// <summary>按名称注册 <see cref="ElColorPicker"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>ColorPicker is a color selector supporting multiple color formats.</remarks>
    [Description("@#ElColorPicker")]
    public IElementPlusComponent? ElColorPicker { get; init; }

    /// <summary>按名称注册 <see cref="ElColorPickerPanel"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>`ColorPickerPanel` is the core component of `ColorPicker`.</remarks>
    [Description("@#ElColorPickerPanel")]
    public IElementPlusComponent? ElColorPickerPanel { get; init; }

    /// <summary>按名称注册 <see cref="ElConfigProvider"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>Config Provider is used for providing global configurations, which enables your entire application to access these configurations everywhere.</remarks>
    [Description("@#ElConfigProvider")]
    public IElementPlusComponent? ElConfigProvider { get; init; }

    /// <summary>按名称注册 <see cref="ElContainer"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>Container components for scaffolding basic structure of the page:</remarks>
    [Description("@#ElContainer")]
    public IElementPlusComponent? ElContainer { get; init; }

    /// <summary>按名称注册 <see cref="ElCountdown"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>:::demo Countdown component, support to add other components control countdown.</remarks>
    [Description("@#ElCountdown")]
    public IElementPlusComponent? ElCountdown { get; init; }

    /// <summary>按名称注册 <see cref="ElDatePicker"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>Use Date Picker for date input.</remarks>
    [Description("@#ElDatePicker")]
    public IElementPlusComponent? ElDatePicker { get; init; }

    /// <summary>按名称注册 <see cref="ElDatePickerPanel"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>`DatePickerPanel` is the core component of `DatePicker`.</remarks>
    [Description("@#ElDatePickerPanel")]
    public IElementPlusComponent? ElDatePickerPanel { get; init; }

    /// <summary>按名称注册 <see cref="ElDescriptions"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>Display multiple fields in list form.</remarks>
    [Description("@#ElDescriptions")]
    public IElementPlusComponent? ElDescriptions { get; init; }

    /// <summary>按名称注册 <see cref="ElDescriptionsItem"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>el-descriptions-item</remarks>
    [Description("@#ElDescriptionsItem")]
    public IElementPlusComponent? ElDescriptionsItem { get; init; }

    /// <summary>按名称注册 <see cref="ElDialog"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>Informs users while preserving the current page state.</remarks>
    [Description("@#ElDialog")]
    public IElementPlusComponent? ElDialog { get; init; }

    /// <summary>按名称注册 <see cref="ElDivider"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>The dividing line that separates the content.</remarks>
    [Description("@#ElDivider")]
    public IElementPlusComponent? ElDivider { get; init; }

    /// <summary>按名称注册 <see cref="ElDrawer"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>Sometimes, `Dialog` does not always satisfy our requirements, let's say you have a massive form, or you need space to display something like `terms &amp; conditions`, `Drawer` has almost identical API with `Dialog`, but it introduces different user experience.</remarks>
    [Description("@#ElDrawer")]
    public IElementPlusComponent? ElDrawer { get; init; }

    /// <summary>按名称注册 <see cref="ElDropdown"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>Toggleable menu for displaying lists of links and actions.</remarks>
    [Description("@#ElDropdown")]
    public IElementPlusComponent? ElDropdown { get; init; }

    /// <summary>按名称注册 <see cref="ElDropdownItem"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>el-dropdown-item</remarks>
    [Description("@#ElDropdownItem")]
    public IElementPlusComponent? ElDropdownItem { get; init; }

    /// <summary>按名称注册 <see cref="ElDropdownMenu"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>el-dropdown-menu</remarks>
    [Description("@#ElDropdownMenu")]
    public IElementPlusComponent? ElDropdownMenu { get; init; }

    /// <summary>按名称注册 <see cref="ElEmpty"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>Placeholder hints for empty states.</remarks>
    [Description("@#ElEmpty")]
    public IElementPlusComponent? ElEmpty { get; init; }

    /// <summary>按名称注册 <see cref="ElFooter"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>el-footer</remarks>
    [Description("@#ElFooter")]
    public IElementPlusComponent? ElFooter { get; init; }

    /// <summary>按名称注册 <see cref="ElForm"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>Form consists of `input`, `radio`, `select`, `checkbox` and so on. With form, you can collect, verify and submit data.</remarks>
    [Description("@#ElForm")]
    public IElementPlusComponent? ElForm { get; init; }

    /// <summary>按名称注册 <see cref="ElFormItem"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>el-form-item</remarks>
    [Description("@#ElFormItem")]
    public IElementPlusComponent? ElFormItem { get; init; }

    /// <summary>按名称注册 <see cref="ElHeader"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>el-header</remarks>
    [Description("@#ElHeader")]
    public IElementPlusComponent? ElHeader { get; init; }

    /// <summary>按名称注册 <see cref="ElIcon"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>Element Plus provides a set of common icons.</remarks>
    [Description("@#ElIcon")]
    public IElementPlusComponent? ElIcon { get; init; }

    /// <summary>按名称注册 <see cref="ElImage"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>Besides the native features of img, support lazy load, custom placeholder and load failure, etc.</remarks>
    [Description("@#ElImage")]
    public IElementPlusComponent? ElImage { get; init; }

    /// <summary>按名称注册 <see cref="ElImageViewer"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>el-image-viewer</remarks>
    [Description("@#ElImageViewer")]
    public IElementPlusComponent? ElImageViewer { get; init; }

    /// <summary>按名称注册 <see cref="ElInput"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>Input data using mouse or keyboard.</remarks>
    [Description("@#ElInput")]
    public IElementPlusComponent? ElInput { get; init; }

    /// <summary>按名称注册 <see cref="ElInputNumber"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>Input numerical values with a customizable range.</remarks>
    [Description("@#ElInputNumber")]
    public IElementPlusComponent? ElInputNumber { get; init; }

    /// <summary>按名称注册 <see cref="ElInputOtp"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>Used to enter a one-time password</remarks>
    [Description("@#ElInputOtp")]
    public IElementPlusComponent? ElInputOtp { get; init; }

    /// <summary>按名称注册 <see cref="ElInputTag"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>The InputTag component allows users to add content as tags.</remarks>
    [Description("@#ElInputTag")]
    public IElementPlusComponent? ElInputTag { get; init; }

    /// <summary>按名称注册 <see cref="ElLink"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>Text hyperlink</remarks>
    [Description("@#ElLink")]
    public IElementPlusComponent? ElLink { get; init; }

    /// <summary>按名称注册 <see cref="ElMain"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>el-main</remarks>
    [Description("@#ElMain")]
    public IElementPlusComponent? ElMain { get; init; }

    /// <summary>按名称注册 <see cref="ElMention"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>Used to mention someone or something in an input.</remarks>
    [Description("@#ElMention")]
    public IElementPlusComponent? ElMention { get; init; }

    /// <summary>按名称注册 <see cref="ElMenu"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>Menu that provides navigation for your website.</remarks>
    [Description("@#ElMenu")]
    public IElementPlusComponent? ElMenu { get; init; }

    /// <summary>按名称注册 <see cref="ElMenuItem"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>el-menu-item</remarks>
    [Description("@#ElMenuItem")]
    public IElementPlusComponent? ElMenuItem { get; init; }

    /// <summary>按名称注册 <see cref="ElMenuItemGroup"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>el-menu-item-group</remarks>
    [Description("@#ElMenuItemGroup")]
    public IElementPlusComponent? ElMenuItemGroup { get; init; }

    /// <summary>按名称注册 <see cref="ElOption"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>el-option</remarks>
    [Description("@#ElOption")]
    public IElementPlusComponent? ElOption { get; init; }

    /// <summary>按名称注册 <see cref="ElOptionGroup"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>el-option-group</remarks>
    [Description("@#ElOptionGroup")]
    public IElementPlusComponent? ElOptionGroup { get; init; }

    /// <summary>按名称注册 <see cref="ElPageHeader"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>If path of the page is simple, it is recommended to use PageHeader instead of the Breadcrumb.</remarks>
    [Description("@#ElPageHeader")]
    public IElementPlusComponent? ElPageHeader { get; init; }

    /// <summary>按名称注册 <see cref="ElPagination"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>If you have too much data to display in one page, use pagination.</remarks>
    [Description("@#ElPagination")]
    public IElementPlusComponent? ElPagination { get; init; }

    /// <summary>按名称注册 <see cref="ElPopconfirm"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>A simple confirmation dialog of an element click action.</remarks>
    [Description("@#ElPopconfirm")]
    public IElementPlusComponent? ElPopconfirm { get; init; }

    /// <summary>按名称注册 <see cref="ElPopover"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>el-popover</remarks>
    [Description("@#ElPopover")]
    public IElementPlusComponent? ElPopover { get; init; }

    /// <summary>按名称注册 <see cref="ElPopper"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>ElPopper</remarks>
    [Description("@#ElPopper")]
    public IElementPlusComponent? ElPopper { get; init; }

    /// <summary>按名称注册 <see cref="ElProgress"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>Progress is used to show the progress of current operation, and inform the user the current status.</remarks>
    [Description("@#ElProgress")]
    public IElementPlusComponent? ElProgress { get; init; }

    /// <summary>按名称注册 <see cref="ElRadio"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>Single selection among multiple options.</remarks>
    [Description("@#ElRadio")]
    public IElementPlusComponent? ElRadio { get; init; }

    /// <summary>按名称注册 <see cref="ElRadioButton"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>el-radio-button</remarks>
    [Description("@#ElRadioButton")]
    public IElementPlusComponent? ElRadioButton { get; init; }

    /// <summary>按名称注册 <see cref="ElRadioGroup"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>el-radio-group</remarks>
    [Description("@#ElRadioGroup")]
    public IElementPlusComponent? ElRadioGroup { get; init; }

    /// <summary>按名称注册 <see cref="ElRate"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>Used for rating</remarks>
    [Description("@#ElRate")]
    public IElementPlusComponent? ElRate { get; init; }

    /// <summary>按名称注册 <see cref="ElResult"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>Used to give feedback on the result of user's operation or access exception.</remarks>
    [Description("@#ElResult")]
    public IElementPlusComponent? ElResult { get; init; }

    /// <summary>按名称注册 <see cref="ElRow"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>el-row</remarks>
    [Description("@#ElRow")]
    public IElementPlusComponent? ElRow { get; init; }

    /// <summary>按名称注册 <see cref="ElScrollbar"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>Used to replace the browser's native scrollbar.</remarks>
    [Description("@#ElScrollbar")]
    public IElementPlusComponent? ElScrollbar { get; init; }

    /// <summary>按名称注册 <see cref="ElSegmented"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>Display multiple options and allow users to select a single option.</remarks>
    [Description("@#ElSegmented")]
    public IElementPlusComponent? ElSegmented { get; init; }

    /// <summary>按名称注册 <see cref="ElSelect"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>When there are plenty of options, use a drop-down menu to display and select desired ones.</remarks>
    [Description("@#ElSelect")]
    public IElementPlusComponent? ElSelect { get; init; }

    /// <summary>按名称注册 <see cref="ElSkeleton"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>When loading data, and you need a rich experience for visual and interactions for your end users, you can choose `skeleton`.</remarks>
    [Description("@#ElSkeleton")]
    public IElementPlusComponent? ElSkeleton { get; init; }

    /// <summary>按名称注册 <see cref="ElSkeletonItem"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>el-skeleton-item</remarks>
    [Description("@#ElSkeletonItem")]
    public IElementPlusComponent? ElSkeletonItem { get; init; }

    /// <summary>按名称注册 <see cref="ElSlider"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>Drag the slider within a fixed range.</remarks>
    [Description("@#ElSlider")]
    public IElementPlusComponent? ElSlider { get; init; }

    /// <summary>按名称注册 <see cref="ElSpace"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>Even though we have [Divider]</remarks>
    [Description("@#ElSpace")]
    public IElementPlusComponent? ElSpace { get; init; }

    /// <summary>按名称注册 <see cref="ElSplitter"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>el-splitter</remarks>
    [Description("@#ElSplitter")]
    public IElementPlusComponent? ElSplitter { get; init; }

    /// <summary>按名称注册 <see cref="ElSplitterPanel"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>el-splitter-panel</remarks>
    [Description("@#ElSplitterPanel")]
    public IElementPlusComponent? ElSplitterPanel { get; init; }

    /// <summary>按名称注册 <see cref="ElStatistic"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>Display statistics.</remarks>
    [Description("@#ElStatistic")]
    public IElementPlusComponent? ElStatistic { get; init; }

    /// <summary>按名称注册 <see cref="ElStep"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>el-step</remarks>
    [Description("@#ElStep")]
    public IElementPlusComponent? ElStep { get; init; }

    /// <summary>按名称注册 <see cref="ElSteps"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>Guide the user to complete tasks in accordance with the process. Its steps can be set according to the actual application scenario and the number of the steps can't be less than 2.</remarks>
    [Description("@#ElSteps")]
    public IElementPlusComponent? ElSteps { get; init; }

    /// <summary>按名称注册 <see cref="ElSubMenu"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>el-sub-menu</remarks>
    [Description("@#ElSubMenu")]
    public IElementPlusComponent? ElSubMenu { get; init; }

    /// <summary>按名称注册 <see cref="ElSwitch"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>Switch is used for switching between two opposing states.</remarks>
    [Description("@#ElSwitch")]
    public IElementPlusComponent? ElSwitch { get; init; }

    /// <summary>按名称注册 <see cref="ElTabPane"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>el-tab-pane</remarks>
    [Description("@#ElTabPane")]
    public IElementPlusComponent? ElTabPane { get; init; }

    /// <summary>按名称注册 <see cref="ElTable"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>Display multiple data with similar format. You can sort, filter, compare your data in a table.</remarks>
    [Description("@#ElTable")]
    public IElementPlusComponent? ElTable { get; init; }

    /// <summary>按名称注册 <see cref="ElTableColumn"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>el-table-column</remarks>
    [Description("@#ElTableColumn")]
    public IElementPlusComponent? ElTableColumn { get; init; }

    /// <summary>按名称注册 <see cref="ElTableV2"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>el-table-v2</remarks>
    [Description("@#ElTableV2")]
    public IElementPlusComponent? ElTableV2 { get; init; }

    /// <summary>按名称注册 <see cref="ElTabs"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>Divide data collections which are related yet belong to different types.</remarks>
    [Description("@#ElTabs")]
    public IElementPlusComponent? ElTabs { get; init; }

    /// <summary>按名称注册 <see cref="ElTag"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>Used for marking and selection.</remarks>
    [Description("@#ElTag")]
    public IElementPlusComponent? ElTag { get; init; }

    /// <summary>按名称注册 <see cref="ElText"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>Used for text.</remarks>
    [Description("@#ElText")]
    public IElementPlusComponent? ElText { get; init; }

    /// <summary>按名称注册 <see cref="ElTimePicker"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>Use Time Picker for time input.</remarks>
    [Description("@#ElTimePicker")]
    public IElementPlusComponent? ElTimePicker { get; init; }

    /// <summary>按名称注册 <see cref="ElTimeSelect"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>Use Time Select for time input.</remarks>
    [Description("@#ElTimeSelect")]
    public IElementPlusComponent? ElTimeSelect { get; init; }

    /// <summary>按名称注册 <see cref="ElTimeline"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>Visually display timeline.</remarks>
    [Description("@#ElTimeline")]
    public IElementPlusComponent? ElTimeline { get; init; }

    /// <summary>按名称注册 <see cref="ElTimelineItem"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>el-timeline-item</remarks>
    [Description("@#ElTimelineItem")]
    public IElementPlusComponent? ElTimelineItem { get; init; }

    /// <summary>按名称注册 <see cref="ElTooltip"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>Display prompt information for mouse hover.</remarks>
    [Description("@#ElTooltip")]
    public IElementPlusComponent? ElTooltip { get; init; }

    /// <summary>按名称注册 <see cref="ElTour"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>A popup component for guiding users through a product. Use when you want to guide users through a product.</remarks>
    [Description("@#ElTour")]
    public IElementPlusComponent? ElTour { get; init; }

    /// <summary>按名称注册 <see cref="ElTourStep"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>el-tour-step</remarks>
    [Description("@#ElTourStep")]
    public IElementPlusComponent? ElTourStep { get; init; }

    /// <summary>按名称注册 <see cref="ElTransfer"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>el-transfer</remarks>
    [Description("@#ElTransfer")]
    public IElementPlusComponent? ElTransfer { get; init; }

    /// <summary>按名称注册 <see cref="ElTree"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>Display a set of data with hierarchies.</remarks>
    [Description("@#ElTree")]
    public IElementPlusComponent? ElTree { get; init; }

    /// <summary>按名称注册 <see cref="ElTreeSelect"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>ElTreeSelect</remarks>
    [Description("@#ElTreeSelect")]
    public IElementPlusComponent? ElTreeSelect { get; init; }

    /// <summary>按名称注册 <see cref="ElTreeV2"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>el-tree-v2</remarks>
    [Description("@#ElTreeV2")]
    public IElementPlusComponent? ElTreeV2 { get; init; }

    /// <summary>按名称注册 <see cref="ElUpload"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>Upload files by clicking or drag-and-drop.</remarks>
    [Description("@#ElUpload")]
    public IElementPlusComponent? ElUpload { get; init; }

    /// <summary>按名称注册 <see cref="ElVirtualizedSelect"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>:::tip</remarks>
    [Description("@#ElVirtualizedSelect")]
    public IElementPlusComponent? ElVirtualizedSelect { get; init; }

    /// <summary>按名称注册 <see cref="ElWatermark"/> 组件，供 Vue 应用解析。</summary>
    /// <remarks>Add specific text or patterns to the page.</remarks>
    [Description("@#ElWatermark")]
    public IElementPlusComponent? ElWatermark { get; init; }

}
