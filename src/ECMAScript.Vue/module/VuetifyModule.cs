namespace ECMAScript.Vue.Vuetify;

[ECMAScriptModule("vuetify")]
public static class Vuetify
{
    [ECMAScriptName("createVuetify")]
    public extern static VuetifyPlugin CreateVuetify();

    [ECMAScriptName("createVuetify")]
    public extern static VuetifyPlugin CreateVuetify(VuetifyOptions options);
}

[ECMAScriptModule("vuetify")]
public sealed class VuetifyPlugin : VuePlugin
{
    private VuetifyPlugin()
    {
    }
}

[ECMAScriptModule]
[Description("@#VuetifyOptions")]
public sealed record VuetifyOptions : VuePluginOptions
{
    [Description("@#components")]
    public VuetifyComponentRegistry? Components { get; init; }

    [Description("@#directives")]
    public VuetifyDirectiveRegistry? Directives { get; init; }

    [Description("@#display")]
    public VuetifyDisplayOptions? Display { get; init; }

    [Description("@#theme")]
    public VuetifyThemeOptions? Theme { get; init; }

    [Description("@#icons")]
    public VuetifyIconOptions? Icons { get; init; }

    [Description("@#locale")]
    public VuetifyLocaleOptions? Locale { get; init; }

    [Description("@#date")]
    public VuetifyDateOptions? Date { get; init; }

    [Description("@#ssr")]
    public bool? Ssr { get; init; }
}

[ECMAScriptModule]
[Description("@#VuetifyThemeOptions")]
public sealed record VuetifyThemeOptions
{
    [Description("@#defaultTheme")]
    public string? DefaultTheme { get; init; }

    [Description("@#variations")]
    public VuetifyThemeVariationOptions? Variations { get; init; }
}

[ECMAScriptModule]
[Description("@#VuetifyThemeVariationOptions")]
public sealed record VuetifyThemeVariationOptions
{
    [Description("@#colors")]
    public string[]? Colors { get; init; }

    [Description("@#lighten")]
    public int? Lighten { get; init; }

    [Description("@#darken")]
    public int? Darken { get; init; }
}

[ECMAScriptModule]
[Description("@#VuetifyDisplayOptions")]
public sealed record VuetifyDisplayOptions
{
    [Description("@#mobileBreakpoint")]
    public Either<string, Number>? MobileBreakpoint { get; init; }

    [Description("@#thresholds")]
    public VuetifyDisplayThresholds? Thresholds { get; init; }
}

[ECMAScriptModule]
[Description("@#VuetifyDisplayThresholds")]
public sealed record VuetifyDisplayThresholds
{
    [Description("@#xs")]
    public int? Xs { get; init; }

    [Description("@#sm")]
    public int? Sm { get; init; }

    [Description("@#md")]
    public int? Md { get; init; }

    [Description("@#lg")]
    public int? Lg { get; init; }

    [Description("@#xl")]
    public int? Xl { get; init; }

    [Description("@#xxl")]
    public int? Xxl { get; init; }
}

[ECMAScriptModule]
[Description("@#VuetifyIconOptions")]
public sealed record VuetifyIconOptions
{
    [Description("@#defaultSet")]
    public string? DefaultSet { get; init; }
}

[ECMAScriptModule]
[Description("@#VuetifyLocaleOptions")]
public sealed record VuetifyLocaleOptions
{
    [Description("@#locale")]
    public string? Locale { get; init; }

    [Description("@#fallback")]
    public string? Fallback { get; init; }
}

[ECMAScriptModule]
[Description("@#VuetifyDateOptions")]
public sealed record VuetifyDateOptions
{
    [Description("@#locale")]
    public string? Locale { get; init; }
}

[ECMAScriptModule("vuetify/components")]
public abstract class VuetifyComponent : VueComponent
{
    protected VuetifyComponent()
    {
    }
}

[ECMAScriptModule("vuetify/directives")]
public abstract class VuetifyDirective : VueDirective
{
    protected VuetifyDirective()
    {
    }
}

[ECMAScriptModule]
[Description("@#VuetifyComponentRegistry")]
public sealed record VuetifyComponentRegistry : VueComponentRegistry
{
    [Description("@#VAlert")]
    public VAlert? VAlert { get; init; }

    [Description("@#VApp")]
    public VApp? VApp { get; init; }

    [Description("@#VAppBar")]
    public VAppBar? VAppBar { get; init; }

    [Description("@#VAutocomplete")]
    public VAutocomplete? VAutocomplete { get; init; }

    [Description("@#VAvatar")]
    public VAvatar? VAvatar { get; init; }

    [Description("@#VBadge")]
    public VBadge? VBadge { get; init; }

    [Description("@#VBanner")]
    public VBanner? VBanner { get; init; }

    [Description("@#VBottomNavigation")]
    public VBottomNavigation? VBottomNavigation { get; init; }

    [Description("@#VBottomSheet")]
    public VBottomSheet? VBottomSheet { get; init; }

    [Description("@#VBreadcrumbs")]
    public VBreadcrumbs? VBreadcrumbs { get; init; }

    [Description("@#VBtn")]
    public VBtn? VBtn { get; init; }

    [Description("@#VBtnGroup")]
    public VBtnGroup? VBtnGroup { get; init; }

    [Description("@#VBtnToggle")]
    public VBtnToggle? VBtnToggle { get; init; }

    [Description("@#VCalendar")]
    public VCalendar? VCalendar { get; init; }

    [Description("@#VCard")]
    public VCard? VCard { get; init; }

    [Description("@#VCardActions")]
    public VCardActions? VCardActions { get; init; }

    [Description("@#VCardItem")]
    public VCardItem? VCardItem { get; init; }

    [Description("@#VCardSubtitle")]
    public VCardSubtitle? VCardSubtitle { get; init; }

    [Description("@#VCardText")]
    public VCardText? VCardText { get; init; }

    [Description("@#VCardTitle")]
    public VCardTitle? VCardTitle { get; init; }

    [Description("@#VCarousel")]
    public VCarousel? VCarousel { get; init; }

    [Description("@#VCheckbox")]
    public VCheckbox? VCheckbox { get; init; }

    [Description("@#VChip")]
    public VChip? VChip { get; init; }

    [Description("@#VChipGroup")]
    public VChipGroup? VChipGroup { get; init; }

    [Description("@#VCode")]
    public VCode? VCode { get; init; }

    [Description("@#VCol")]
    public VCol? VCol { get; init; }

    [Description("@#VColorPicker")]
    public VColorPicker? VColorPicker { get; init; }

    [Description("@#VCombobox")]
    public VCombobox? VCombobox { get; init; }

    [Description("@#VConfirmEdit")]
    public VConfirmEdit? VConfirmEdit { get; init; }

    [Description("@#VContainer")]
    public VContainer? VContainer { get; init; }

    [Description("@#VCounter")]
    public VCounter? VCounter { get; init; }

    [Description("@#VDataIterator")]
    public VDataIterator? VDataIterator { get; init; }

    [Description("@#VDataTable")]
    public VDataTable? VDataTable { get; init; }

    [Description("@#VDatePicker")]
    public VDatePicker? VDatePicker { get; init; }

    [Description("@#VDefaultsProvider")]
    public VDefaultsProvider? VDefaultsProvider { get; init; }

    [Description("@#VDialog")]
    public VDialog? VDialog { get; init; }

    [Description("@#VDivider")]
    public VDivider? VDivider { get; init; }

    [Description("@#VEmptyState")]
    public VEmptyState? VEmptyState { get; init; }

    [Description("@#VExpansionPanel")]
    public VExpansionPanel? VExpansionPanel { get; init; }

    [Description("@#VFab")]
    public VFab? VFab { get; init; }

    [Description("@#VField")]
    public VField? VField { get; init; }

    [Description("@#VFileInput")]
    public VFileInput? VFileInput { get; init; }

    [Description("@#VFooter")]
    public VFooter? VFooter { get; init; }

    [Description("@#VForm")]
    public VForm? VForm { get; init; }

    [Description("@#VHotkey")]
    public VHotkey? VHotkey { get; init; }

    [Description("@#VHover")]
    public VHover? VHover { get; init; }

    [Description("@#VIcon")]
    public VIcon? VIcon { get; init; }

    [Description("@#VImg")]
    public VImg? VImg { get; init; }

    [Description("@#VInfiniteScroll")]
    public VInfiniteScroll? VInfiniteScroll { get; init; }

    [Description("@#VInput")]
    public VInput? VInput { get; init; }

    [Description("@#VItemGroup")]
    public VItemGroup? VItemGroup { get; init; }

    [Description("@#VKbd")]
    public VKbd? VKbd { get; init; }

    [Description("@#VLabel")]
    public VLabel? VLabel { get; init; }

    [Description("@#VLayout")]
    public VLayout? VLayout { get; init; }

    [Description("@#VLazy")]
    public VLazy? VLazy { get; init; }

    [Description("@#VList")]
    public VList? VList { get; init; }

    [Description("@#VListItem")]
    public VListItem? VListItem { get; init; }

    [Description("@#VLocaleProvider")]
    public VLocaleProvider? VLocaleProvider { get; init; }

    [Description("@#VMain")]
    public VMain? VMain { get; init; }

    [Description("@#VMenu")]
    public VMenu? VMenu { get; init; }

    [Description("@#VMessages")]
    public VMessages? VMessages { get; init; }

    [Description("@#VNavigationDrawer")]
    public VNavigationDrawer? VNavigationDrawer { get; init; }

    [Description("@#VNoSsr")]
    public VNoSsr? VNoSsr { get; init; }

    [Description("@#VNumberInput")]
    public VNumberInput? VNumberInput { get; init; }

    [Description("@#VOtpInput")]
    public VOtpInput? VOtpInput { get; init; }

    [Description("@#VOverlay")]
    public VOverlay? VOverlay { get; init; }

    [Description("@#VPagination")]
    public VPagination? VPagination { get; init; }

    [Description("@#VParallax")]
    public VParallax? VParallax { get; init; }

    [Description("@#VProgressCircular")]
    public VProgressCircular? VProgressCircular { get; init; }

    [Description("@#VProgressLinear")]
    public VProgressLinear? VProgressLinear { get; init; }

    [Description("@#VRadio")]
    public VRadio? VRadio { get; init; }

    [Description("@#VRadioGroup")]
    public VRadioGroup? VRadioGroup { get; init; }

    [Description("@#VRangeSlider")]
    public VRangeSlider? VRangeSlider { get; init; }

    [Description("@#VRating")]
    public VRating? VRating { get; init; }

    [Description("@#VResponsive")]
    public VResponsive? VResponsive { get; init; }

    [Description("@#VRow")]
    public VRow? VRow { get; init; }

    [Description("@#VSelect")]
    public VSelect? VSelect { get; init; }

    [Description("@#VSelectionControl")]
    public VSelectionControl? VSelectionControl { get; init; }

    [Description("@#VSelectionControlGroup")]
    public VSelectionControlGroup? VSelectionControlGroup { get; init; }

    [Description("@#VSheet")]
    public VSheet? VSheet { get; init; }

    [Description("@#VSkeletonLoader")]
    public VSkeletonLoader? VSkeletonLoader { get; init; }

    [Description("@#VSlideGroup")]
    public VSlideGroup? VSlideGroup { get; init; }

    [Description("@#VSlider")]
    public VSlider? VSlider { get; init; }

    [Description("@#VSnackbar")]
    public VSnackbar? VSnackbar { get; init; }

    [Description("@#VSnackbarQueue")]
    public VSnackbarQueue? VSnackbarQueue { get; init; }

    [Description("@#VSpacer")]
    public VSpacer? VSpacer { get; init; }

    [Description("@#VSparkline")]
    public VSparkline? VSparkline { get; init; }

    [Description("@#VSpeedDial")]
    public VSpeedDial? VSpeedDial { get; init; }

    [Description("@#VStepper")]
    public VStepper? VStepper { get; init; }

    [Description("@#VSwitch")]
    public VSwitch? VSwitch { get; init; }

    [Description("@#VSystemBar")]
    public VSystemBar? VSystemBar { get; init; }

    [Description("@#VTab")]
    public VTab? VTab { get; init; }

    [Description("@#VTable")]
    public VTable? VTable { get; init; }

    [Description("@#VTabs")]
    public VTabs? VTabs { get; init; }

    [Description("@#VTabsWindow")]
    public VTabsWindow? VTabsWindow { get; init; }

    [Description("@#VTabsWindowItem")]
    public VTabsWindowItem? VTabsWindowItem { get; init; }

    [Description("@#VTextarea")]
    public VTextarea? VTextarea { get; init; }

    [Description("@#VTextField")]
    public VTextField? VTextField { get; init; }

    [Description("@#VThemeProvider")]
    public VThemeProvider? VThemeProvider { get; init; }

    [Description("@#VTimeline")]
    public VTimeline? VTimeline { get; init; }

    [Description("@#VTimePicker")]
    public VTimePicker? VTimePicker { get; init; }

    [Description("@#VToolbar")]
    public VToolbar? VToolbar { get; init; }

    [Description("@#VToolbarItems")]
    public VToolbarItems? VToolbarItems { get; init; }

    [Description("@#VToolbarTitle")]
    public VToolbarTitle? VToolbarTitle { get; init; }

    [Description("@#VTooltip")]
    public VTooltip? VTooltip { get; init; }

    [Description("@#VTreeview")]
    public VTreeview? VTreeview { get; init; }

    [Description("@#VValidation")]
    public VValidation? VValidation { get; init; }

    [Description("@#VVirtualScroll")]
    public VVirtualScroll? VVirtualScroll { get; init; }

    [Description("@#VWindow")]
    public VWindow? VWindow { get; init; }
}

[ECMAScriptModule]
[Description("@#VuetifyDirectiveRegistry")]
public sealed record VuetifyDirectiveRegistry : VueDirectiveRegistry
{
    [Description("@#ClickOutside")]
    public ClickOutsideDirective? ClickOutside { get; init; }

    [Description("@#Intersect")]
    public IntersectDirective? Intersect { get; init; }

    [Description("@#Mutate")]
    public MutateDirective? Mutate { get; init; }

    [Description("@#Resize")]
    public ResizeDirective? Resize { get; init; }

    [Description("@#Ripple")]
    public RippleDirective? Ripple { get; init; }

    [Description("@#Scroll")]
    public ScrollDirective? Scroll { get; init; }

    [Description("@#Tooltip")]
    public TooltipDirective? Tooltip { get; init; }

    [Description("@#Touch")]
    public TouchDirective? Touch { get; init; }
}

[ECMAScriptModule("vuetify/components")]
public static class VuetifyComponents
{
    [ECMAScriptName("VApp")]
    public extern static VApp VApp { get; }

    [ECMAScriptName("VAppBar")]
    public extern static VAppBar VAppBar { get; }

    [ECMAScriptName("VAlert")]
    public extern static VAlert VAlert { get; }

    [ECMAScriptName("VAutocomplete")]
    public extern static VAutocomplete VAutocomplete { get; }

    [ECMAScriptName("VAvatar")]
    public extern static VAvatar VAvatar { get; }

    [ECMAScriptName("VBadge")]
    public extern static VBadge VBadge { get; }

    [ECMAScriptName("VBanner")]
    public extern static VBanner VBanner { get; }

    [ECMAScriptName("VBottomNavigation")]
    public extern static VBottomNavigation VBottomNavigation { get; }

    [ECMAScriptName("VBottomSheet")]
    public extern static VBottomSheet VBottomSheet { get; }

    [ECMAScriptName("VBreadcrumbs")]
    public extern static VBreadcrumbs VBreadcrumbs { get; }

    [ECMAScriptName("VBtn")]
    public extern static VBtn VBtn { get; }

    [ECMAScriptName("VBtnGroup")]
    public extern static VBtnGroup VBtnGroup { get; }

    [ECMAScriptName("VBtnToggle")]
    public extern static VBtnToggle VBtnToggle { get; }

    [ECMAScriptName("VCalendar")]
    public extern static VCalendar VCalendar { get; }

    [ECMAScriptName("VCard")]
    public extern static VCard VCard { get; }

    [ECMAScriptName("VCardActions")]
    public extern static VCardActions VCardActions { get; }

    [ECMAScriptName("VCardItem")]
    public extern static VCardItem VCardItem { get; }

    [ECMAScriptName("VCardSubtitle")]
    public extern static VCardSubtitle VCardSubtitle { get; }

    [ECMAScriptName("VCardText")]
    public extern static VCardText VCardText { get; }

    [ECMAScriptName("VCardTitle")]
    public extern static VCardTitle VCardTitle { get; }

    [ECMAScriptName("VCarousel")]
    public extern static VCarousel VCarousel { get; }

    [ECMAScriptName("VCheckbox")]
    public extern static VCheckbox VCheckbox { get; }

    [ECMAScriptName("VChip")]
    public extern static VChip VChip { get; }

    [ECMAScriptName("VChipGroup")]
    public extern static VChipGroup VChipGroup { get; }

    [ECMAScriptName("VCode")]
    public extern static VCode VCode { get; }

    [ECMAScriptName("VCol")]
    public extern static VCol VCol { get; }

    [ECMAScriptName("VColorPicker")]
    public extern static VColorPicker VColorPicker { get; }

    [ECMAScriptName("VCombobox")]
    public extern static VCombobox VCombobox { get; }

    [ECMAScriptName("VConfirmEdit")]
    public extern static VConfirmEdit VConfirmEdit { get; }

    [ECMAScriptName("VContainer")]
    public extern static VContainer VContainer { get; }

    [ECMAScriptName("VCounter")]
    public extern static VCounter VCounter { get; }

    [ECMAScriptName("VDataIterator")]
    public extern static VDataIterator VDataIterator { get; }

    [ECMAScriptName("VDataTable")]
    public extern static VDataTable VDataTable { get; }

    [ECMAScriptName("VDatePicker")]
    public extern static VDatePicker VDatePicker { get; }

    [ECMAScriptName("VDefaultsProvider")]
    public extern static VDefaultsProvider VDefaultsProvider { get; }

    [ECMAScriptName("VDialog")]
    public extern static VDialog VDialog { get; }

    [ECMAScriptName("VDivider")]
    public extern static VDivider VDivider { get; }

    [ECMAScriptName("VEmptyState")]
    public extern static VEmptyState VEmptyState { get; }

    [ECMAScriptName("VExpansionPanel")]
    public extern static VExpansionPanel VExpansionPanel { get; }

    [ECMAScriptName("VFab")]
    public extern static VFab VFab { get; }

    [ECMAScriptName("VField")]
    public extern static VField VField { get; }

    [ECMAScriptName("VFileInput")]
    public extern static VFileInput VFileInput { get; }

    [ECMAScriptName("VFooter")]
    public extern static VFooter VFooter { get; }

    [ECMAScriptName("VForm")]
    public extern static VForm VForm { get; }

    [ECMAScriptName("VHotkey")]
    public extern static VHotkey VHotkey { get; }

    [ECMAScriptName("VHover")]
    public extern static VHover VHover { get; }

    [ECMAScriptName("VIcon")]
    public extern static VIcon VIcon { get; }

    [ECMAScriptName("VImg")]
    public extern static VImg VImg { get; }

    [ECMAScriptName("VInfiniteScroll")]
    public extern static VInfiniteScroll VInfiniteScroll { get; }

    [ECMAScriptName("VInput")]
    public extern static VInput VInput { get; }

    [ECMAScriptName("VItemGroup")]
    public extern static VItemGroup VItemGroup { get; }

    [ECMAScriptName("VKbd")]
    public extern static VKbd VKbd { get; }

    [ECMAScriptName("VLabel")]
    public extern static VLabel VLabel { get; }

    [ECMAScriptName("VLayout")]
    public extern static VLayout VLayout { get; }

    [ECMAScriptName("VLazy")]
    public extern static VLazy VLazy { get; }

    [ECMAScriptName("VList")]
    public extern static VList VList { get; }

    [ECMAScriptName("VLocaleProvider")]
    public extern static VLocaleProvider VLocaleProvider { get; }

    [ECMAScriptName("VListItem")]
    public extern static VListItem VListItem { get; }

    [ECMAScriptName("VMain")]
    public extern static VMain VMain { get; }

    [ECMAScriptName("VMenu")]
    public extern static VMenu VMenu { get; }

    [ECMAScriptName("VMessages")]
    public extern static VMessages VMessages { get; }

    [ECMAScriptName("VNavigationDrawer")]
    public extern static VNavigationDrawer VNavigationDrawer { get; }

    [ECMAScriptName("VNoSsr")]
    public extern static VNoSsr VNoSsr { get; }

    [ECMAScriptName("VNumberInput")]
    public extern static VNumberInput VNumberInput { get; }

    [ECMAScriptName("VOtpInput")]
    public extern static VOtpInput VOtpInput { get; }

    [ECMAScriptName("VOverlay")]
    public extern static VOverlay VOverlay { get; }

    [ECMAScriptName("VPagination")]
    public extern static VPagination VPagination { get; }

    [ECMAScriptName("VParallax")]
    public extern static VParallax VParallax { get; }

    [ECMAScriptName("VProgressCircular")]
    public extern static VProgressCircular VProgressCircular { get; }

    [ECMAScriptName("VProgressLinear")]
    public extern static VProgressLinear VProgressLinear { get; }

    [ECMAScriptName("VRadio")]
    public extern static VRadio VRadio { get; }

    [ECMAScriptName("VRadioGroup")]
    public extern static VRadioGroup VRadioGroup { get; }

    [ECMAScriptName("VRangeSlider")]
    public extern static VRangeSlider VRangeSlider { get; }

    [ECMAScriptName("VRating")]
    public extern static VRating VRating { get; }

    [ECMAScriptName("VResponsive")]
    public extern static VResponsive VResponsive { get; }

    [ECMAScriptName("VRow")]
    public extern static VRow VRow { get; }

    [ECMAScriptName("VSelect")]
    public extern static VSelect VSelect { get; }

    [ECMAScriptName("VSelectionControl")]
    public extern static VSelectionControl VSelectionControl { get; }

    [ECMAScriptName("VSelectionControlGroup")]
    public extern static VSelectionControlGroup VSelectionControlGroup { get; }

    [ECMAScriptName("VSheet")]
    public extern static VSheet VSheet { get; }

    [ECMAScriptName("VSkeletonLoader")]
    public extern static VSkeletonLoader VSkeletonLoader { get; }

    [ECMAScriptName("VSlideGroup")]
    public extern static VSlideGroup VSlideGroup { get; }

    [ECMAScriptName("VSlider")]
    public extern static VSlider VSlider { get; }

    [ECMAScriptName("VSnackbar")]
    public extern static VSnackbar VSnackbar { get; }

    [ECMAScriptName("VSnackbarQueue")]
    public extern static VSnackbarQueue VSnackbarQueue { get; }

    [ECMAScriptName("VSpacer")]
    public extern static VSpacer VSpacer { get; }

    [ECMAScriptName("VSparkline")]
    public extern static VSparkline VSparkline { get; }

    [ECMAScriptName("VSpeedDial")]
    public extern static VSpeedDial VSpeedDial { get; }

    [ECMAScriptName("VStepper")]
    public extern static VStepper VStepper { get; }

    [ECMAScriptName("VSwitch")]
    public extern static VSwitch VSwitch { get; }

    [ECMAScriptName("VSystemBar")]
    public extern static VSystemBar VSystemBar { get; }

    [ECMAScriptName("VTab")]
    public extern static VTab VTab { get; }

    [ECMAScriptName("VTabs")]
    public extern static VTabs VTabs { get; }

    [ECMAScriptName("VTabsWindow")]
    public extern static VTabsWindow VTabsWindow { get; }

    [ECMAScriptName("VTabsWindowItem")]
    public extern static VTabsWindowItem VTabsWindowItem { get; }

    [ECMAScriptName("VTable")]
    public extern static VTable VTable { get; }

    [ECMAScriptName("VTextarea")]
    public extern static VTextarea VTextarea { get; }

    [ECMAScriptName("VTextField")]
    public extern static VTextField VTextField { get; }

    [ECMAScriptName("VThemeProvider")]
    public extern static VThemeProvider VThemeProvider { get; }

    [ECMAScriptName("VTimeline")]
    public extern static VTimeline VTimeline { get; }

    [ECMAScriptName("VTimePicker")]
    public extern static VTimePicker VTimePicker { get; }

    [ECMAScriptName("VToolbar")]
    public extern static VToolbar VToolbar { get; }

    [ECMAScriptName("VToolbarItems")]
    public extern static VToolbarItems VToolbarItems { get; }

    [ECMAScriptName("VToolbarTitle")]
    public extern static VToolbarTitle VToolbarTitle { get; }

    [ECMAScriptName("VTooltip")]
    public extern static VTooltip VTooltip { get; }

    [ECMAScriptName("VTreeview")]
    public extern static VTreeview VTreeview { get; }

    [ECMAScriptName("VValidation")]
    public extern static VValidation VValidation { get; }

    [ECMAScriptName("VVirtualScroll")]
    public extern static VVirtualScroll VVirtualScroll { get; }

    [ECMAScriptName("VWindow")]
    public extern static VWindow VWindow { get; }
}

[ECMAScriptModule("vuetify/directives")]
public static class VuetifyDirectives
{
    [ECMAScriptName("ClickOutside")]
    public extern static ClickOutsideDirective ClickOutside { get; }

    [ECMAScriptName("Intersect")]
    public extern static IntersectDirective Intersect { get; }

    [ECMAScriptName("Mutate")]
    public extern static MutateDirective Mutate { get; }

    [ECMAScriptName("Resize")]
    public extern static ResizeDirective Resize { get; }

    [ECMAScriptName("Ripple")]
    public extern static RippleDirective Ripple { get; }

    [ECMAScriptName("Scroll")]
    public extern static ScrollDirective Scroll { get; }

    [ECMAScriptName("Touch")]
    public extern static TouchDirective Touch { get; }

    [ECMAScriptName("Tooltip")]
    public extern static TooltipDirective Tooltip { get; }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VAlert")]
public sealed class VAlert : VuetifyComponent
{
    private VAlert()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VApp")]
public sealed class VApp : VuetifyComponent
{
    private VApp()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VAppBar")]
public sealed class VAppBar : VuetifyComponent
{
    private VAppBar()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VAutocomplete")]
public sealed class VAutocomplete : VuetifyComponent
{
    private VAutocomplete()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VAvatar")]
public sealed class VAvatar : VuetifyComponent
{
    private VAvatar()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VBadge")]
public sealed class VBadge : VuetifyComponent
{
    private VBadge()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VBanner")]
public sealed class VBanner : VuetifyComponent
{
    private VBanner()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VBottomNavigation")]
public sealed class VBottomNavigation : VuetifyComponent
{
    private VBottomNavigation()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VBottomSheet")]
public sealed class VBottomSheet : VuetifyComponent
{
    private VBottomSheet()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VBreadcrumbs")]
public sealed class VBreadcrumbs : VuetifyComponent
{
    private VBreadcrumbs()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VBtn")]
public sealed class VBtn : VuetifyComponent
{
    private VBtn()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VBtnGroup")]
public sealed class VBtnGroup : VuetifyComponent
{
    private VBtnGroup()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VBtnToggle")]
public sealed class VBtnToggle : VuetifyComponent
{
    private VBtnToggle()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VCalendar")]
public sealed class VCalendar : VuetifyComponent
{
    private VCalendar()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VCard")]
public sealed class VCard : VuetifyComponent
{
    private VCard()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VCardActions")]
public sealed class VCardActions : VuetifyComponent
{
    private VCardActions()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VCardItem")]
public sealed class VCardItem : VuetifyComponent
{
    private VCardItem()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VCardSubtitle")]
public sealed class VCardSubtitle : VuetifyComponent
{
    private VCardSubtitle()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VCardText")]
public sealed class VCardText : VuetifyComponent
{
    private VCardText()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VCardTitle")]
public sealed class VCardTitle : VuetifyComponent
{
    private VCardTitle()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VCarousel")]
public sealed class VCarousel : VuetifyComponent
{
    private VCarousel()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VCheckbox")]
public sealed class VCheckbox : VuetifyComponent
{
    private VCheckbox()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VChip")]
public sealed class VChip : VuetifyComponent
{
    private VChip()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VChipGroup")]
public sealed class VChipGroup : VuetifyComponent
{
    private VChipGroup()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VCode")]
public sealed class VCode : VuetifyComponent
{
    private VCode()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VCol")]
public sealed class VCol : VuetifyComponent
{
    private VCol()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VColorPicker")]
public sealed class VColorPicker : VuetifyComponent
{
    private VColorPicker()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VCombobox")]
public sealed class VCombobox : VuetifyComponent
{
    private VCombobox()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VConfirmEdit")]
public sealed class VConfirmEdit : VuetifyComponent
{
    private VConfirmEdit()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VContainer")]
public sealed class VContainer : VuetifyComponent
{
    private VContainer()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VCounter")]
public sealed class VCounter : VuetifyComponent
{
    private VCounter()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VDataIterator")]
public sealed class VDataIterator : VuetifyComponent
{
    private VDataIterator()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VDataTable")]
public sealed class VDataTable : VuetifyComponent
{
    private VDataTable()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VDatePicker")]
public sealed class VDatePicker : VuetifyComponent
{
    private VDatePicker()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VDefaultsProvider")]
public sealed class VDefaultsProvider : VuetifyComponent
{
    private VDefaultsProvider()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VDialog")]
public sealed class VDialog : VuetifyComponent
{
    private VDialog()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VDivider")]
public sealed class VDivider : VuetifyComponent
{
    private VDivider()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VEmptyState")]
public sealed class VEmptyState : VuetifyComponent
{
    private VEmptyState()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VExpansionPanel")]
public sealed class VExpansionPanel : VuetifyComponent
{
    private VExpansionPanel()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VFab")]
public sealed class VFab : VuetifyComponent
{
    private VFab()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VField")]
public sealed class VField : VuetifyComponent
{
    private VField()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VFileInput")]
public sealed class VFileInput : VuetifyComponent
{
    private VFileInput()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VFooter")]
public sealed class VFooter : VuetifyComponent
{
    private VFooter()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VForm")]
public sealed class VForm : VuetifyComponent
{
    private VForm()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VHotkey")]
public sealed class VHotkey : VuetifyComponent
{
    private VHotkey()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VHover")]
public sealed class VHover : VuetifyComponent
{
    private VHover()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VIcon")]
public sealed class VIcon : VuetifyComponent
{
    private VIcon()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VImg")]
public sealed class VImg : VuetifyComponent
{
    private VImg()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VInfiniteScroll")]
public sealed class VInfiniteScroll : VuetifyComponent
{
    private VInfiniteScroll()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VInput")]
public sealed class VInput : VuetifyComponent
{
    private VInput()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VItemGroup")]
public sealed class VItemGroup : VuetifyComponent
{
    private VItemGroup()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VKbd")]
public sealed class VKbd : VuetifyComponent
{
    private VKbd()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VLabel")]
public sealed class VLabel : VuetifyComponent
{
    private VLabel()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VLayout")]
public sealed class VLayout : VuetifyComponent
{
    private VLayout()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VLazy")]
public sealed class VLazy : VuetifyComponent
{
    private VLazy()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VList")]
public sealed class VList : VuetifyComponent
{
    private VList()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VListItem")]
public sealed class VListItem : VuetifyComponent
{
    private VListItem()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VLocaleProvider")]
public sealed class VLocaleProvider : VuetifyComponent
{
    private VLocaleProvider()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VMain")]
public sealed class VMain : VuetifyComponent
{
    private VMain()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VMenu")]
public sealed class VMenu : VuetifyComponent
{
    private VMenu()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VMessages")]
public sealed class VMessages : VuetifyComponent
{
    private VMessages()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VNavigationDrawer")]
public sealed class VNavigationDrawer : VuetifyComponent
{
    private VNavigationDrawer()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VNoSsr")]
public sealed class VNoSsr : VuetifyComponent
{
    private VNoSsr()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VNumberInput")]
public sealed class VNumberInput : VuetifyComponent
{
    private VNumberInput()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VOtpInput")]
public sealed class VOtpInput : VuetifyComponent
{
    private VOtpInput()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VOverlay")]
public sealed class VOverlay : VuetifyComponent
{
    private VOverlay()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VPagination")]
public sealed class VPagination : VuetifyComponent
{
    private VPagination()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VParallax")]
public sealed class VParallax : VuetifyComponent
{
    private VParallax()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VProgressCircular")]
public sealed class VProgressCircular : VuetifyComponent
{
    private VProgressCircular()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VProgressLinear")]
public sealed class VProgressLinear : VuetifyComponent
{
    private VProgressLinear()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VRadio")]
public sealed class VRadio : VuetifyComponent
{
    private VRadio()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VRadioGroup")]
public sealed class VRadioGroup : VuetifyComponent
{
    private VRadioGroup()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VRangeSlider")]
public sealed class VRangeSlider : VuetifyComponent
{
    private VRangeSlider()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VRating")]
public sealed class VRating : VuetifyComponent
{
    private VRating()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VResponsive")]
public sealed class VResponsive : VuetifyComponent
{
    private VResponsive()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VRow")]
public sealed class VRow : VuetifyComponent
{
    private VRow()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VSelect")]
public sealed class VSelect : VuetifyComponent
{
    private VSelect()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VSelectionControl")]
public sealed class VSelectionControl : VuetifyComponent
{
    private VSelectionControl()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VSelectionControlGroup")]
public sealed class VSelectionControlGroup : VuetifyComponent
{
    private VSelectionControlGroup()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VSheet")]
public sealed class VSheet : VuetifyComponent
{
    private VSheet()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VSkeletonLoader")]
public sealed class VSkeletonLoader : VuetifyComponent
{
    private VSkeletonLoader()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VSlideGroup")]
public sealed class VSlideGroup : VuetifyComponent
{
    private VSlideGroup()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VSlider")]
public sealed class VSlider : VuetifyComponent
{
    private VSlider()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VSnackbar")]
public sealed class VSnackbar : VuetifyComponent
{
    private VSnackbar()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VSnackbarQueue")]
public sealed class VSnackbarQueue : VuetifyComponent
{
    private VSnackbarQueue()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VSpacer")]
public sealed class VSpacer : VuetifyComponent
{
    private VSpacer()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VSparkline")]
public sealed class VSparkline : VuetifyComponent
{
    private VSparkline()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VSpeedDial")]
public sealed class VSpeedDial : VuetifyComponent
{
    private VSpeedDial()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VStepper")]
public sealed class VStepper : VuetifyComponent
{
    private VStepper()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VSwitch")]
public sealed class VSwitch : VuetifyComponent
{
    private VSwitch()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VSystemBar")]
public sealed class VSystemBar : VuetifyComponent
{
    private VSystemBar()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VTab")]
public sealed class VTab : VuetifyComponent
{
    private VTab()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VTable")]
public sealed class VTable : VuetifyComponent
{
    private VTable()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VTabs")]
public sealed class VTabs : VuetifyComponent
{
    private VTabs()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VTabsWindow")]
public sealed class VTabsWindow : VuetifyComponent
{
    private VTabsWindow()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VTabsWindowItem")]
public sealed class VTabsWindowItem : VuetifyComponent
{
    private VTabsWindowItem()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VTextarea")]
public sealed class VTextarea : VuetifyComponent
{
    private VTextarea()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VTextField")]
public sealed class VTextField : VuetifyComponent
{
    private VTextField()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VThemeProvider")]
public sealed class VThemeProvider : VuetifyComponent
{
    private VThemeProvider()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VTimeline")]
public sealed class VTimeline : VuetifyComponent
{
    private VTimeline()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VTimePicker")]
public sealed class VTimePicker : VuetifyComponent
{
    private VTimePicker()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VToolbar")]
public sealed class VToolbar : VuetifyComponent
{
    private VToolbar()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VToolbarItems")]
public sealed class VToolbarItems : VuetifyComponent
{
    private VToolbarItems()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VToolbarTitle")]
public sealed class VToolbarTitle : VuetifyComponent
{
    private VToolbarTitle()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VTooltip")]
public sealed class VTooltip : VuetifyComponent
{
    private VTooltip()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VTreeview")]
public sealed class VTreeview : VuetifyComponent
{
    private VTreeview()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VValidation")]
public sealed class VValidation : VuetifyComponent
{
    private VValidation()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VVirtualScroll")]
public sealed class VVirtualScroll : VuetifyComponent
{
    private VVirtualScroll()
    {
    }
}

[ECMAScriptModule("vuetify/components")]
[ECMAScriptName("VWindow")]
public sealed class VWindow : VuetifyComponent
{
    private VWindow()
    {
    }
}

[ECMAScriptModule("vuetify/directives")]
[ECMAScriptName("ClickOutside")]
public sealed class ClickOutsideDirective : VuetifyDirective
{
    private ClickOutsideDirective()
    {
    }
}

[ECMAScriptModule("vuetify/directives")]
[ECMAScriptName("Intersect")]
public sealed class IntersectDirective : VuetifyDirective
{
    private IntersectDirective()
    {
    }
}

[ECMAScriptModule("vuetify/directives")]
[ECMAScriptName("Mutate")]
public sealed class MutateDirective : VuetifyDirective
{
    private MutateDirective()
    {
    }
}

[ECMAScriptModule("vuetify/directives")]
[ECMAScriptName("Resize")]
public sealed class ResizeDirective : VuetifyDirective
{
    private ResizeDirective()
    {
    }
}

[ECMAScriptModule("vuetify/directives")]
[ECMAScriptName("Ripple")]
public sealed class RippleDirective : VuetifyDirective
{
    private RippleDirective()
    {
    }
}

[ECMAScriptModule("vuetify/directives")]
[ECMAScriptName("Scroll")]
public sealed class ScrollDirective : VuetifyDirective
{
    private ScrollDirective()
    {
    }
}

[ECMAScriptModule("vuetify/directives")]
[ECMAScriptName("Tooltip")]
public sealed class TooltipDirective : VuetifyDirective
{
    private TooltipDirective()
    {
    }
}

[ECMAScriptModule("vuetify/directives")]
[ECMAScriptName("Touch")]
public sealed class TouchDirective : VuetifyDirective
{
    private TouchDirective()
    {
    }
}
