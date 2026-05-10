using System.Reflection;
using ECMAScript.Vuetify;

namespace Jazor.RazorVue.Test;

internal static class VuetifyTestMetadata
{
    public static string[] RuntimeComponentExportNames { get; } =
        typeof(VuetifyComponents)
            .GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(static property => property.PropertyType == typeof(IVuetifyComponent))
            .Select(static property => property.Name)
            .OrderBy(static name => name, StringComparer.Ordinal)
            .ToArray();

    public static string[] RuntimeOnlyAuthoringComponentNames { get; } =
    [
        "VBanner",
        "VBottomNavigation",
        "VBottomSheet",
        "VBtnGroup",
        "VBtnToggle",
        "VCalendar",
        "VCardActions",
        "VCardItem",
        "VCardSubtitle",
        "VCarousel",
        "VChipGroup",
        "VCode",
        "VColorPicker",
        "VConfirmEdit",
        "VCounter",
        "VDataIterator",
        "VDatePicker",
        "VDefaultsProvider",
        "VEmptyState",
        "VExpansionPanel",
        "VFab",
        "VField",
        "VFooter",
        "VHotkey",
        "VHover",
        "VInfiniteScroll",
        "VInput",
        "VItemGroup",
        "VKbd",
        "VLabel",
        "VLayout",
        "VLazy",
        "VLocaleProvider",
        "VMessages",
        "VNoSsr",
        "VOverlay",
        "VParallax",
        "VRating",
        "VResponsive",
        "VSelectionControl",
        "VSelectionControlGroup",
        "VSkeletonLoader",
        "VSlideGroup",
        "VSnackbarQueue",
        "VSparkline",
        "VSpeedDial",
        "VStepper",
        "VSystemBar",
        "VTabsWindow",
        "VTabsWindowItem",
        "VTable",
        "VThemeProvider",
        "VTimeline",
        "VTimePicker",
        "VToolbarItems",
        "VTreeview",
        "VValidation",
        "VVirtualScroll",
        "VWindow"
    ];

    public static string[] StrongAuthoringComponentNames { get; } =
        RuntimeComponentExportNames
            .Except(RuntimeOnlyAuthoringComponentNames, StringComparer.Ordinal)
            .OrderBy(static name => name, StringComparer.Ordinal)
            .ToArray();
}
