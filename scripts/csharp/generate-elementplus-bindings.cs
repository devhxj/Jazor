#!/usr/bin/env dotnet run
#:project ../../src/ECMAScript.Vue3/ECMAScript.Vue3.csproj
#:project ../../src/ECMAScript.ElementPlus/ECMAScript.ElementPlus.csproj

#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using ECMAScript.ElementPlus;
using static ECMAScript.Vue3;

internal static class Program
{
    private const string CssClassPropertyName = "CssClass";
    private const string CssStylePropertyName = "CssStyle";
    private const string AdditionalAttributesPropertyName = "AdditionalAttributes";
    private const string ChildContentPropertyName = "ChildContent";

    private static readonly string[] ReservedPropertyNames =
    [
        CssClassPropertyName,
        CssStylePropertyName,
        AdditionalAttributesPropertyName,
        ChildContentPropertyName
    ];

    private static readonly Dictionary<string, string> RuntimeComponentExportOverrides = new(StringComparer.Ordinal)
    {
        ["ElVirtualizedSelect"] = "ElSelectV2"
    };

    private static readonly Dictionary<string, Type> RuntimeTypeMap = new(StringComparer.Ordinal)
    {
        ["bool"] = typeof(bool),
        ["byte"] = typeof(byte),
        ["sbyte"] = typeof(sbyte),
        ["short"] = typeof(short),
        ["ushort"] = typeof(ushort),
        ["int"] = typeof(int),
        ["uint"] = typeof(uint),
        ["long"] = typeof(long),
        ["ulong"] = typeof(ulong),
        ["float"] = typeof(float),
        ["double"] = typeof(double),
        ["decimal"] = typeof(decimal),
        ["string"] = typeof(string),
        ["object"] = typeof(object),
        ["Number"] = typeof(ECMAScript.Number),
        ["BigInt"] = typeof(ECMAScript.BigInt),
        ["Element"] = typeof(ECMAScript.Element),
        ["HTMLElement"] = typeof(ECMAScript.HTMLElement),
        ["Headers"] = typeof(ECMAScript.Headers),
        ["XMLHttpRequest"] = typeof(ECMAScript.XMLHttpRequest),
        ["File"] = typeof(ECMAScript.File),
        ["Blob"] = typeof(ECMAScript.Blob),
        ["Error"] = typeof(ECMAScript.Error),
        ["IVueComponent"] = typeof(IVueComponent),
        ["VueValue"] = typeof(VueValue),
        ["VueProps"] = typeof(VueProps),
        ["VueDictionary"] = typeof(VueDictionary),
        ["VueClassValue"] = typeof(VueClassValue),
        ["VueStyleValue"] = typeof(VueStyleValue),
        ["VueTeleportTarget"] = typeof(VueTeleportTarget),
        ["VueBooleanNumberValue"] = typeof(VueBooleanNumberValue),
        ["VueStringNumberValue"] = typeof(VueStringNumberValue),
        ["VueStringNumberDateValue"] = typeof(VueStringNumberDateValue),
        ["VueStringNumberArrayableValue"] = typeof(VueStringNumberArrayableValue),
        ["VueStringNumberDateArrayableValue"] = typeof(VueStringNumberDateArrayableValue),
        ["VueNumberOrNumbersValue"] = typeof(VueNumberOrNumbersValue),
        ["VueStringNumberObjectValue"] = typeof(VueStringNumberObjectValue),
        ["VueBooleanStringNumberObjectValue"] = typeof(VueBooleanStringNumberObjectValue),
        ["VueBooleanStringNumberObjectArrayableValue"] = typeof(VueBooleanStringNumberObjectArrayableValue),
        ["VueStringOrStringsValue"] = typeof(VueStringOrStringsValue),
        ["VueStringRegExpValue"] = typeof(VueStringRegExpValue),
        ["VueModelModifierBag"] = typeof(VueModelModifierBag),
        ["VueNumberPair"] = typeof(VueNumberPair),
        ["VueStringNumberVNodeValue"] = typeof(VueStringNumberVNodeValue),
        ["VueBooleanStringValue"] = typeof(VueBooleanStringValue),
        ["VueBooleanStringNumberValue"] = typeof(VueBooleanStringNumberValue),
        ["VueStringComponentValue"] = typeof(VueStringComponentValue),
        ["VueDatePair"] = typeof(VueDatePair),
        ["VueDateSingleOrRangeValue"] = typeof(VueDateSingleOrRangeValue),
        ["VueStringPair"] = typeof(VueStringPair),
        ["VueStringSingleOrRangeValue"] = typeof(VueStringSingleOrRangeValue),
        ["VueTransitionValue"] = typeof(VueTransitionValue),
        ["Date"] = typeof(ECMAScript.Date),
        ["RegExp"] = typeof(ECMAScript.RegExp),
        ["ElSelectV2ModelValue"] = typeof(ElSelectV2ModelValue),
        ["ElPopperPlacement"] = typeof(ElPopperPlacement)
    };

    private static readonly Dictionary<(string TagName, string RuntimeName), ExplicitGeneratedType> ExplicitPropTypeOverrides = new()
    {
        [("el-config-provider", "locale")] = ExplicitGeneratedType.Reference("ElLanguage"),
        [("el-config-provider", "experimentalFeatures")] = ExplicitGeneratedType.Reference("VueProps"),
        [("el-config-provider", "button")] = ExplicitGeneratedType.Reference("ElButtonConfig"),
        [("el-config-provider", "card")] = ExplicitGeneratedType.Reference("ElCardConfig"),
        [("el-config-provider", "dialog")] = ExplicitGeneratedType.Reference("ElDialogConfig"),
        [("el-config-provider", "link")] = ExplicitGeneratedType.Reference("ElLinkConfig"),
        [("el-config-provider", "message")] = ExplicitGeneratedType.Reference("ElMessageConfig"),
        [("el-config-provider", "emptyValues")] = ExplicitGeneratedType.Reference("VueValue[]"),
        [("el-config-provider", "valueOnClear")] = ExplicitGeneratedType.Value("ElValueOnClearValue"),
        [("el-config-provider", "table")] = ExplicitGeneratedType.Reference("ElTableConfig"),
        [("el-breadcrumb-item", "to")] = ExplicitGeneratedType.Value("RouteLocationRaw"),
        [("el-calendar", "modelValue")] = ExplicitGeneratedType.Reference("Date"),
        [("el-calendar", "range")] = ExplicitGeneratedType.Value("VueDatePair"),
        [("el-calendar", "controllerType")] = ExplicitGeneratedType.Value("ElCalendarControllerType"),
        [("el-card", "shadow")] = ExplicitGeneratedType.Value("ElCardShadow"),
        [("el-affix", "position")] = ExplicitGeneratedType.Value("ElTopBottomPlacement"),
        [("el-col", "xs")] = ExplicitGeneratedType.Value("ElColSizeValue"),
        [("el-col", "sm")] = ExplicitGeneratedType.Value("ElColSizeValue"),
        [("el-col", "md")] = ExplicitGeneratedType.Value("ElColSizeValue"),
        [("el-col", "lg")] = ExplicitGeneratedType.Value("ElColSizeValue"),
        [("el-col", "xl")] = ExplicitGeneratedType.Value("ElColSizeValue"),
        [("el-color-picker-panel", "hueSliderClass")] = ExplicitGeneratedType.Value("VueClassValue"),
        [("el-color-picker-panel", "hueSliderStyle")] = ExplicitGeneratedType.Value("VueStyleValue"),
        [("el-avatar", "shape")] = ExplicitGeneratedType.Value("ElAvatarShape"),
        [("el-avatar-group", "shape")] = ExplicitGeneratedType.Value("ElAvatarShape"),
        [("el-avatar-group", "effect")] = ExplicitGeneratedType.Value("ElPopperEffect"),
        [("el-avatar-group", "placement")] = ExplicitGeneratedType.Value("ElPopperPlacement"),
        [("el-button", "type")] = ExplicitGeneratedType.Value("ElButtonType"),
        [("el-button", "nativeType")] = ExplicitGeneratedType.Value("ElButtonNativeType"),
        [("el-button-group", "type")] = ExplicitGeneratedType.Value("ElButtonType"),
        [("el-button-group", "direction")] = ExplicitGeneratedType.Value("ElDirection"),
        [("el-collapse", "modelValue")] = ExplicitGeneratedType.Value("VueStringNumberArrayableValue"),
        [("el-collapse", "expandIconPosition")] = ExplicitGeneratedType.Value("ElCollapseIconPosition"),
        [("el-dialog", "transition")] = ExplicitGeneratedType.Value("VueTransitionValue"),
        [("el-dropdown", "trigger")] = ExplicitGeneratedType.Value("ElDropdownTriggerValue"),
        [("el-dropdown", "type")] = ExplicitGeneratedType.Value("ElButtonType"),
        [("el-dropdown", "placement")] = ExplicitGeneratedType.Value("ElPopperPlacement"),
        [("el-dropdown", "effect")] = ExplicitGeneratedType.Value("ElPopperEffect"),
        [("el-dropdown", "buttonProps")] = ExplicitGeneratedType.Reference("ElButtonProps"),
        [("el-carousel", "trigger")] = ExplicitGeneratedType.Value("ElHoverClickTrigger"),
        [("el-carousel", "type")] = ExplicitGeneratedType.Value("ElCarouselType"),
        [("el-carousel", "direction")] = ExplicitGeneratedType.Value("ElDirection"),
        [("el-container", "direction")] = ExplicitGeneratedType.Value("ElDirection"),
        [("el-descriptions", "direction")] = ExplicitGeneratedType.Value("ElDirection"),
        [("el-image", "fit")] = ExplicitGeneratedType.Value("ElImageFitType"),
        [("el-image", "loading")] = ExplicitGeneratedType.Value("ElImageLoadingType"),
        [("el-image", "crossorigin")] = ExplicitGeneratedType.Value("ElCrossorigin"),
        [("el-pagination", "pagerCount")] = ExplicitGeneratedType.Value("Number"),
        [("el-progress", "percentage")] = ExplicitGeneratedType.Value("Number"),
        [("el-table", "tooltipEffect")] = ExplicitGeneratedType.Reference("string"),
        [("el-table", "showOverflowTooltip")] = ExplicitGeneratedType.Value("ElTableOverflowTooltipValue"),
        [("el-table", "tooltipOptions")] = ExplicitGeneratedType.Reference("ElTableOverflowTooltipOptions"),
        [("el-table", "defaultSort")] = ExplicitGeneratedType.Reference("ElTableSort"),
        [("el-table", "treeProps")] = ExplicitGeneratedType.Reference("ElTableTreeProps"),
        [("el-table-column", "showOverflowTooltip")] = ExplicitGeneratedType.Value("ElTableOverflowTooltipValue"),
        [("el-table-column", "sortOrders")] = ExplicitGeneratedType.Reference("ElTableSortOrder?[]"),
        [("el-table-column", "filters")] = ExplicitGeneratedType.Reference("ElTableFilterItem[]"),
        [("el-form", "scrollIntoViewOptions")] = ExplicitGeneratedType.Value("ScrollIntoViewArg"),
        [("el-form", "rules")] = ExplicitGeneratedType.Reference("ElFormRules"),
        [("el-form-item", "prop")] = ExplicitGeneratedType.Value("VueStringOrStringsValue"),
        [("el-form-item", "rules")] = ExplicitGeneratedType.Value("ElFormItemRules"),
        [("el-form-item", "validateStatus")] = ExplicitGeneratedType.Value("ElFormItemValidateStatus"),
        [("el-cascader", "emptyValues")] = ExplicitGeneratedType.Reference("VueValue[]"),
        [("el-cascader", "valueOnClear")] = ExplicitGeneratedType.Value("ElValueOnClearValue"),
        [("el-cascader", "fitInputWidth")] = ExplicitGeneratedType.Value("VueBooleanNumberValue"),
        [("el-cascader", "fallbackPlacements")] = ExplicitGeneratedType.Reference("string[]"),
        [("el-cascader", "modelValue")] = ExplicitGeneratedType.Value("VueBooleanStringNumberObjectArrayableValue"),
        [("el-cascader-panel", "modelValue")] = ExplicitGeneratedType.Value("VueBooleanStringNumberObjectArrayableValue"),
        [("el-color-picker", "emptyValues")] = ExplicitGeneratedType.Reference("VueValue[]"),
        [("el-color-picker", "valueOnClear")] = ExplicitGeneratedType.Value("ElValueOnClearValue"),
        [("el-date-picker", "emptyValues")] = ExplicitGeneratedType.Reference("VueValue[]"),
        [("el-date-picker", "valueOnClear")] = ExplicitGeneratedType.Value("ElValueOnClearValue"),
        [("el-date-picker", "format")] = ExplicitGeneratedType.Reference("string"),
        [("el-date-picker", "valueFormat")] = ExplicitGeneratedType.Reference("string"),
        [("el-date-picker", "dateFormat")] = ExplicitGeneratedType.Reference("string"),
        [("el-date-picker", "timeFormat")] = ExplicitGeneratedType.Reference("string"),
        [("el-date-picker", "popperOptions")] = ExplicitGeneratedType.Reference("VueDictionary"),
        [("el-date-picker", "fallbackPlacements")] = ExplicitGeneratedType.Reference("string[]"),
        [("el-date-picker", "placement")] = ExplicitGeneratedType.Reference("string"),
        [("el-date-picker", "defaultValue")] = ExplicitGeneratedType.Value("VueDateSingleOrRangeValue"),
        [("el-date-picker", "defaultTime")] = ExplicitGeneratedType.Value("VueDateSingleOrRangeValue"),
        [("el-date-picker", "modelValue")] = ExplicitGeneratedType.Value("VueStringNumberDateArrayableValue"),
        [("el-date-picker", "id")] = ExplicitGeneratedType.Value("VueStringSingleOrRangeValue"),
        [("el-date-picker", "name")] = ExplicitGeneratedType.Value("VueStringSingleOrRangeValue"),
        [("el-date-picker-panel", "defaultValue")] = ExplicitGeneratedType.Value("VueDateSingleOrRangeValue"),
        [("el-date-picker-panel", "defaultTime")] = ExplicitGeneratedType.Value("VueDateSingleOrRangeValue"),
        [("el-date-picker-panel", "valueFormat")] = ExplicitGeneratedType.Reference("string"),
        [("el-date-picker-panel", "dateFormat")] = ExplicitGeneratedType.Reference("string"),
        [("el-date-picker-panel", "timeFormat")] = ExplicitGeneratedType.Reference("string"),
        [("el-date-picker-panel", "modelValue")] = ExplicitGeneratedType.Value("VueStringNumberDateArrayableValue"),
        [("el-divider", "direction")] = ExplicitGeneratedType.Value("ElDirection"),
        [("el-divider", "contentPosition")] = ExplicitGeneratedType.Value("ElContentPosition"),
        [("el-divider", "borderStyle")] = ExplicitGeneratedType.Reference("string"),
        [("el-image", "scrollContainer")] = ExplicitGeneratedType.Value("VueStringHtmlElementValue"),
        [("el-input", "autosize")] = ExplicitGeneratedType.Value("ElInputAutoSize"),
        [("el-input", "modelModifiers")] = ExplicitGeneratedType.Reference("VueModelModifierBag"),
        [("el-input", "max")] = ExplicitGeneratedType.Value("VueStringNumberValue"),
        [("el-input", "min")] = ExplicitGeneratedType.Value("VueStringNumberValue"),
        [("el-input", "step")] = ExplicitGeneratedType.Value("VueStringNumberValue"),
        [("el-input", "inputStyle")] = ExplicitGeneratedType.Value("VueStyleValue"),
        [("el-input-tag", "delimiter")] = ExplicitGeneratedType.Value("VueStringRegExpValue"),
        [("el-input-number", "valueOnClear")] = ExplicitGeneratedType.Value("ElValueOnClearValue"),
        [("el-menu-item", "route")] = ExplicitGeneratedType.Value("RouteLocationRaw"),
        [("el-menu", "menuTrigger")] = ExplicitGeneratedType.Value("ElHoverClickTrigger"),
        [("el-popover", "visible")] = ExplicitGeneratedType.Value("bool"),
        [("el-popover", "trigger")] = ExplicitGeneratedType.Value("ElTooltipTriggerValue"),
        [("el-popover", "triggerKeys")] = ExplicitGeneratedType.Reference("string[]"),
        [("el-popover", "effect")] = ExplicitGeneratedType.Value("ElPopperEffect"),
        [("el-popover", "placement")] = ExplicitGeneratedType.Value("ElPopperPlacement"),
        [("el-select", "emptyValues")] = ExplicitGeneratedType.Reference("VueValue[]"),
        [("el-select", "valueOnClear")] = ExplicitGeneratedType.Value("ElValueOnClearValue"),
        [("el-select", "effect")] = ExplicitGeneratedType.Value("ElPopperEffect"),
        [("el-select", "tagType")] = ExplicitGeneratedType.Value("ElTagType"),
        [("el-select", "tagEffect")] = ExplicitGeneratedType.Value("ElTagEffect"),
        [("el-select", "placement")] = ExplicitGeneratedType.Value("ElPopperPlacement"),
        [("el-time-picker", "emptyValues")] = ExplicitGeneratedType.Reference("VueValue[]"),
        [("el-time-picker", "valueOnClear")] = ExplicitGeneratedType.Value("ElValueOnClearValue"),
        [("el-time-picker", "format")] = ExplicitGeneratedType.Reference("string"),
        [("el-time-picker", "valueFormat")] = ExplicitGeneratedType.Reference("string"),
        [("el-time-picker", "dateFormat")] = ExplicitGeneratedType.Reference("string"),
        [("el-time-picker", "timeFormat")] = ExplicitGeneratedType.Reference("string"),
        [("el-time-picker", "popperOptions")] = ExplicitGeneratedType.Reference("VueDictionary"),
        [("el-time-picker", "fallbackPlacements")] = ExplicitGeneratedType.Reference("string[]"),
        [("el-time-picker", "placement")] = ExplicitGeneratedType.Reference("string"),
        [("el-time-picker", "defaultValue")] = ExplicitGeneratedType.Value("VueDateSingleOrRangeValue"),
        [("el-time-picker", "defaultTime")] = ExplicitGeneratedType.Value("VueDateSingleOrRangeValue"),
        [("el-time-picker", "modelValue")] = ExplicitGeneratedType.Value("VueStringNumberDateArrayableValue"),
        [("el-time-picker", "id")] = ExplicitGeneratedType.Value("VueStringSingleOrRangeValue"),
        [("el-time-picker", "name")] = ExplicitGeneratedType.Value("VueStringSingleOrRangeValue"),
        [("el-time-select", "emptyValues")] = ExplicitGeneratedType.Reference("VueValue[]"),
        [("el-time-select", "valueOnClear")] = ExplicitGeneratedType.Value("ElValueOnClearValue"),
        [("el-select-v2", "emptyValues")] = ExplicitGeneratedType.Reference("VueValue[]"),
        [("el-select-v2", "valueOnClear")] = ExplicitGeneratedType.Value("ElValueOnClearValue"),
        [("el-select-v2", "fitInputWidth")] = ExplicitGeneratedType.Value("VueBooleanNumberValue"),
        [("el-select-v2", "tagTooltip")] = ExplicitGeneratedType.Reference("ElTagTooltipProps"),
        [("el-select-v2", "props")] = ExplicitGeneratedType.Reference("ElSelectPropsAlias"),
        [("el-segmented", "size")] = ExplicitGeneratedType.Value("ElComponentSize"),
        [("el-slider", "modelValue")] = ExplicitGeneratedType.Value("VueNumberOrNumbersValue"),
        [("el-scrollbar", "wrapStyle")] = ExplicitGeneratedType.Value("VueStyleValue"),
        [("el-scrollbar", "viewStyle")] = ExplicitGeneratedType.Value("VueStyleValue"),
        [("el-skeleton", "throttle")] = ExplicitGeneratedType.Value("ElThrottleValue"),
        [("el-select", "props")] = ExplicitGeneratedType.Reference("ElSelectPropsAlias"),
        [("el-select", "fallbackPlacements")] = ExplicitGeneratedType.Reference("string[]"),
        [("el-select", "modelValue")] = ExplicitGeneratedType.Value("VueBooleanStringNumberObjectArrayableValue"),
        [("el-option", "value")] = ExplicitGeneratedType.Value("VueBooleanStringNumberObjectValue"),
        [("el-checkbox-group", "props")] = ExplicitGeneratedType.Reference("ElCheckboxOptionPropsAlias"),
        [("el-checkbox-group", "modelValue")] = ExplicitGeneratedType.Reference("VueStringNumberValue[]"),
        [("el-checkbox", "value")] = ExplicitGeneratedType.Value("VueBooleanStringNumberObjectValue"),
        [("el-checkbox", "label")] = ExplicitGeneratedType.Value("VueBooleanStringNumberObjectValue"),
        [("el-checkbox-button", "value")] = ExplicitGeneratedType.Value("VueBooleanStringNumberObjectValue"),
        [("el-checkbox-button", "label")] = ExplicitGeneratedType.Value("VueBooleanStringNumberObjectValue"),
        [("el-mention", "props")] = ExplicitGeneratedType.Reference("ElMentionOptionPropsAlias"),
        [("el-mention", "options")] = ExplicitGeneratedType.Reference("ElMentionOption[]"),
        [("el-mention", "prefix")] = ExplicitGeneratedType.Value("VueStringOrStringsValue"),
        [("el-mention", "popperOptions")] = ExplicitGeneratedType.Reference("VueDictionary"),
        [("el-radio-group", "props")] = ExplicitGeneratedType.Reference("ElRadioOptionPropsAlias"),
        [("el-input-number", "modelValue")] = ExplicitGeneratedType.Value("Number"),
        [("el-progress", "type")] = ExplicitGeneratedType.Value("ElProgressType"),
        [("el-progress", "status")] = ExplicitGeneratedType.Value("ElProgressStatus"),
        [("el-segmented", "direction")] = ExplicitGeneratedType.Value("ElDirection"),
        [("el-space", "direction")] = ExplicitGeneratedType.Value("ElDirection"),
        [("el-step", "status")] = ExplicitGeneratedType.Value("ElStepStatus"),
        [("el-steps", "direction")] = ExplicitGeneratedType.Value("ElDirection"),
        [("el-steps", "finishStatus")] = ExplicitGeneratedType.Value("ElStepStatus"),
        [("el-steps", "processStatus")] = ExplicitGeneratedType.Value("ElStepStatus"),
        [("el-text", "type")] = ExplicitGeneratedType.Value("ElSemanticType"),
        [("el-timeline", "mode")] = ExplicitGeneratedType.Value("ElTimelineMode"),
        [("el-timeline-item", "placement")] = ExplicitGeneratedType.Value("ElTopBottomPlacement"),
        [("el-timeline-item", "type")] = ExplicitGeneratedType.Value("ElSemanticType"),
        [("el-rate", "colors")] = ExplicitGeneratedType.Value("ElRateColorsValue"),
        [("el-rate", "icons")] = ExplicitGeneratedType.Value("ElRateIconsValue"),
        [("el-segmented", "props")] = ExplicitGeneratedType.Reference("ElSegmentedPropsAlias"),
        [("el-space", "alignment")] = ExplicitGeneratedType.Reference("string"),
        [("el-space", "spacer")] = ExplicitGeneratedType.Value("VueStringNumberVNodeValue"),
        [("el-space", "size")] = ExplicitGeneratedType.Value("ElSpaceSizeValue"),
        [("el-slider", "marks")] = ExplicitGeneratedType.Reference("ElSliderMarks"),
        [("el-tooltip", "fallbackPlacements")] = ExplicitGeneratedType.Reference("string[]"),
        [("el-tooltip", "trigger")] = ExplicitGeneratedType.Value("ElTooltipTriggerValue"),
        [("el-tooltip", "triggerKeys")] = ExplicitGeneratedType.Reference("string[]"),
        [("el-tooltip", "placement")] = ExplicitGeneratedType.Value("ElPopperPlacement"),
        [("el-tabs", "type")] = ExplicitGeneratedType.Value("ElTabsType"),
        [("el-tabs", "tabPosition")] = ExplicitGeneratedType.Value("ElPopperPlacementSide"),
        [("el-tag", "type")] = ExplicitGeneratedType.Value("ElTagType"),
        [("el-tag", "effect")] = ExplicitGeneratedType.Value("ElTagEffect"),
        [("el-transfer", "modelValue")] = ExplicitGeneratedType.Reference("VueStringNumberValue[]"),
        [("el-dropdown-item", "command")] = ExplicitGeneratedType.Value("VueStringNumberObjectValue"),
        [("el-transfer", "data")] = ExplicitGeneratedType.Reference("ElTransferDataItem[]"),
        [("el-transfer", "targetOrder")] = ExplicitGeneratedType.Value("ElTransferTargetOrder"),
        [("el-transfer", "titles")] = ExplicitGeneratedType.Reference("ElTransferTextPair"),
        [("el-transfer", "buttonTexts")] = ExplicitGeneratedType.Reference("ElTransferTextPair"),
        [("el-transfer", "format")] = ExplicitGeneratedType.Reference("ElTransferFormat"),
        [("el-transfer", "props")] = ExplicitGeneratedType.Reference("ElTransferPropsAlias"),
        [("el-transfer", "leftDefaultChecked")] = ExplicitGeneratedType.Reference("VueStringNumberValue[]"),
        [("el-transfer", "rightDefaultChecked")] = ExplicitGeneratedType.Reference("VueStringNumberValue[]"),
        [("el-transfer", "renderContent")] = ExplicitGeneratedType.Reference("ElTransferRenderContent"),
        [("el-tree", "props")] = ExplicitGeneratedType.Reference("ElTreeOptionProps"),
        [("el-tree", "defaultExpandedKeys")] = ExplicitGeneratedType.Reference("VueStringNumberValue[]"),
        [("el-tree", "defaultCheckedKeys")] = ExplicitGeneratedType.Reference("VueStringNumberValue[]"),
        [("el-tree-select", "fallbackPlacements")] = ExplicitGeneratedType.Reference("string[]"),
        [("el-tree-select", "defaultExpandedKeys")] = ExplicitGeneratedType.Reference("VueStringNumberValue[]"),
        [("el-tree-select", "defaultCheckedKeys")] = ExplicitGeneratedType.Reference("VueStringNumberValue[]"),
        [("el-tree-select", "modelValue")] = ExplicitGeneratedType.Value("VueBooleanStringNumberObjectArrayableValue"),
        [("el-tree-select", "effect")] = ExplicitGeneratedType.Value("ElPopperEffect"),
        [("el-tree-select", "tagType")] = ExplicitGeneratedType.Value("ElTagType"),
        [("el-tree-select", "tagEffect")] = ExplicitGeneratedType.Value("ElTagEffect"),
        [("el-tree-select", "placement")] = ExplicitGeneratedType.Value("ElPopperPlacement"),
        [("el-tree-v2", "props")] = ExplicitGeneratedType.Reference("ElTreeOptionProps"),
        [("el-tree-v2", "defaultExpandedKeys")] = ExplicitGeneratedType.Reference("VueStringNumberValue[]"),
        [("el-tree-v2", "defaultCheckedKeys")] = ExplicitGeneratedType.Reference("VueStringNumberValue[]"),
        [("el-upload", "headers")] = ExplicitGeneratedType.Value("VueHeadersValue"),
        [("el-upload", "crossorigin")] = ExplicitGeneratedType.Value("ElCrossorigin"),
        [("el-upload", "fileList")] = ExplicitGeneratedType.Reference("ElUploadUserFile[]"),
        [("el-upload", "listType")] = ExplicitGeneratedType.Value("ElUploadListType"),
        [("el-cascader", "props")] = ExplicitGeneratedType.Reference("ElCascaderProps"),
        [("el-cascader-panel", "props")] = ExplicitGeneratedType.Reference("ElCascaderProps"),
        [("el-virtualized-select", "emptyValues")] = ExplicitGeneratedType.Reference("VueValue[]"),
        [("el-table-v2", "headerClass")] = ExplicitGeneratedType.Value("ElTableV2ClassValue"),
        [("el-table-v2", "headerProps")] = ExplicitGeneratedType.Value("ElTableV2DynamicPropsValue"),
        [("el-table-v2", "headerCellProps")] = ExplicitGeneratedType.Value("ElTableV2DynamicPropsValue"),
        [("el-table-v2", "headerHeight")] = ExplicitGeneratedType.Value("ElTableV2HeaderHeightValue"),
        [("el-table-v2", "rowClass")] = ExplicitGeneratedType.Value("ElTableV2ClassValue"),
        [("el-table-v2", "rowKey")] = ExplicitGeneratedType.Value("ElTableV2KeyValue"),
        [("el-table-v2", "rowProps")] = ExplicitGeneratedType.Value("ElTableV2DynamicPropsValue"),
        [("el-table-v2", "rowEventHandlers")] = ExplicitGeneratedType.Reference("ElTableV2RowEventHandlers"),
        [("el-table-v2", "cellProps")] = ExplicitGeneratedType.Value("ElTableV2DynamicPropsValue"),
        [("el-table-v2", "columns")] = ExplicitGeneratedType.Reference("ElTableV2Column[]"),
        [("el-table-v2", "data")] = ExplicitGeneratedType.Reference("ElTableV2DataItem[]"),
        [("el-table-v2", "dataGetter")] = ExplicitGeneratedType.Reference("ElTableV2DataGetter"),
        [("el-table-v2", "fixedData")] = ExplicitGeneratedType.Reference("ElTableV2DataItem[]"),
        [("el-table-v2", "expandedRowKeys")] = ExplicitGeneratedType.Reference("ElTableV2KeyValue[]"),
        [("el-table-v2", "defaultExpandedRowKeys")] = ExplicitGeneratedType.Reference("ElTableV2KeyValue[]"),
        [("el-table-v2", "sortBy")] = ExplicitGeneratedType.Reference("ElTableV2SortBy"),
        [("el-table-v2", "sortState")] = ExplicitGeneratedType.Reference("ElTableV2SortState"),
        [("el-virtualized-select", "modelValue")] = ExplicitGeneratedType.Value("ElSelectV2ModelValue"),
        [("el-virtualized-select", "options")] = ExplicitGeneratedType.Reference("ElSelectV2OptionValue[]"),
        [("el-virtualized-select", "valueOnClear")] = ExplicitGeneratedType.Value("ElValueOnClearValue"),
        [("el-virtualized-select", "props")] = ExplicitGeneratedType.Reference("ElSelectPropsAlias"),
        [("el-virtualized-select", "effect")] = ExplicitGeneratedType.Value("ElPopperEffect"),
        [("el-virtualized-select", "placement")] = ExplicitGeneratedType.Value("ElPopperPlacement"),
        [("el-virtualized-select", "fallbackPlacements")] = ExplicitGeneratedType.Reference("string[]"),
        [("el-virtualized-select", "tagType")] = ExplicitGeneratedType.Value("ElTagType"),
        [("el-virtualized-select", "tagEffect")] = ExplicitGeneratedType.Value("ElTagEffect"),
        [("el-autocomplete", "fetchSuggestions")] = ExplicitGeneratedType.Value("ElAutocompleteFetchSuggestionsValue"),
        [("el-auto-resizer", "onResize")] = ExplicitGeneratedType.Reference("ElAutoResizerResizeCallback"),
        [("el-calendar", "formatter")] = ExplicitGeneratedType.Reference("ElCalendarFormatterCallback"),
        [("el-cascader", "filterMethod")] = ExplicitGeneratedType.Reference("ElCascaderFilterMethod"),
        [("el-cascader", "beforeFilter")] = ExplicitGeneratedType.Reference("ElCascaderBeforeFilterCallback"),
        [("el-collapse", "beforeCollapse")] = ExplicitGeneratedType.Reference("ElCollapseBeforeCollapseCallback"),
        [("el-date-picker", "disabledDate")] = ExplicitGeneratedType.Reference("ElDateLikeDisabledDate"),
        [("el-date-picker", "cellClassName")] = ExplicitGeneratedType.Reference("ElDateLikeCellClassName"),
        [("el-date-picker-panel", "disabledDate")] = ExplicitGeneratedType.Reference("ElDateLikeDisabledDate"),
        [("el-date-picker-panel", "cellClassName")] = ExplicitGeneratedType.Reference("ElDateLikeCellClassName"),
        [("el-dialog", "beforeClose")] = ExplicitGeneratedType.Reference("ElDialogBeforeCloseCallback"),
        [("el-drawer", "beforeClose")] = ExplicitGeneratedType.Reference("ElDialogBeforeCloseCallback"),
        [("el-input", "formatter")] = ExplicitGeneratedType.Reference("ElInputFormatter"),
        [("el-input", "parser")] = ExplicitGeneratedType.Reference("ElInputParser"),
        [("el-input", "countGraphemes")] = ExplicitGeneratedType.Reference("ElInputCountGraphemes"),
        [("el-input-number", "formatter")] = ExplicitGeneratedType.Reference("ElInputFormatter"),
        [("el-input-number", "parser")] = ExplicitGeneratedType.Reference("ElInputParser"),
        [("el-input-otp", "validator")] = ExplicitGeneratedType.Reference("ElInputOtpValidator"),
        [("el-input-otp", "separator")] = ExplicitGeneratedType.Value("ElInputOtpSeparatorValue"),
        [("el-mention", "filterOption")] = ExplicitGeneratedType.Value("ElMentionFilterOptionValue"),
        [("el-mention", "checkIsWhole")] = ExplicitGeneratedType.Reference("ElMentionCheckIsWhole"),
        [("el-progress", "color")] = ExplicitGeneratedType.Value("ElProgressColorValue"),
        [("el-progress", "format")] = ExplicitGeneratedType.Reference("ElProgressFormatCallback"),
        [("el-select", "filterMethod")] = ExplicitGeneratedType.Reference("ElSelectQueryCallback"),
        [("el-select", "remoteMethod")] = ExplicitGeneratedType.Reference("ElSelectQueryCallback"),
        [("el-slider", "formatTooltip")] = ExplicitGeneratedType.Reference("ElSliderFormatTooltipCallback"),
        [("el-slider", "formatValueText")] = ExplicitGeneratedType.Reference("ElSliderFormatValueTextCallback"),
        [("el-switch", "beforeChange")] = ExplicitGeneratedType.Reference("ElSwitchBeforeChangeCallback"),
        [("el-table", "rowClassName")] = ExplicitGeneratedType.Value("ElTableRowClassNameValue"),
        [("el-table", "rowStyle")] = ExplicitGeneratedType.Value("ElTableRowStyleValue"),
        [("el-table", "cellClassName")] = ExplicitGeneratedType.Value("ElTableCellClassNameValue"),
        [("el-table", "cellStyle")] = ExplicitGeneratedType.Value("ElTableCellStyleValue"),
        [("el-table", "headerRowClassName")] = ExplicitGeneratedType.Value("ElTableRowClassNameValue"),
        [("el-table", "headerRowStyle")] = ExplicitGeneratedType.Value("ElTableRowStyleValue"),
        [("el-table", "headerCellClassName")] = ExplicitGeneratedType.Value("ElTableCellClassNameValue"),
        [("el-table", "headerCellStyle")] = ExplicitGeneratedType.Value("ElTableCellStyleValue"),
        [("el-table", "rowKey")] = ExplicitGeneratedType.Value("ElTableRowKeyValue"),
        [("el-table", "summaryMethod")] = ExplicitGeneratedType.Reference("ElTableSummaryMethodCallback"),
        [("el-table", "spanMethod")] = ExplicitGeneratedType.Reference("ElTableSpanMethodCallback"),
        [("el-table", "load")] = ExplicitGeneratedType.Reference("ElTableLoadCallback"),
        [("el-table", "tooltipFormatter")] = ExplicitGeneratedType.Reference("ElTableTooltipFormatter"),
        [("el-table", "rowExpandable")] = ExplicitGeneratedType.Reference("ElTableRowExpandableCallback"),
        [("el-table-column", "index")] = ExplicitGeneratedType.Value("ElTableColumnIndexValue"),
        [("el-table-column", "renderHeader")] = ExplicitGeneratedType.Reference("ElTableColumnRenderHeaderCallback"),
        [("el-table-column", "sortMethod")] = ExplicitGeneratedType.Reference("ElTableColumnSortMethodCallback"),
        [("el-table-column", "sortBy")] = ExplicitGeneratedType.Value("ElTableColumnSortByValue"),
        [("el-table-column", "formatter")] = ExplicitGeneratedType.Reference("ElTableColumnFormatterCallback"),
        [("el-table-column", "selectable")] = ExplicitGeneratedType.Reference("ElTableColumnSelectableCallback"),
        [("el-table-column", "filterMethod")] = ExplicitGeneratedType.Reference("ElTableColumnFilterMethodCallback"),
        [("el-table-column", "tooltipFormatter")] = ExplicitGeneratedType.Reference("ElTableTooltipFormatter"),
        [("el-tabs", "beforeLeave")] = ExplicitGeneratedType.Reference("ElTabsBeforeLeaveCallback"),
        [("el-time-picker", "disabledHours")] = ExplicitGeneratedType.Reference("ElTimePickerDisabledHoursCallback"),
        [("el-time-picker", "disabledMinutes")] = ExplicitGeneratedType.Reference("ElTimePickerDisabledMinutesCallback"),
        [("el-time-picker", "disabledSeconds")] = ExplicitGeneratedType.Reference("ElTimePickerDisabledSecondsCallback"),
        [("el-transfer", "filterMethod")] = ExplicitGeneratedType.Reference("ElTransferFilterMethod"),
        [("el-tree", "renderContent")] = ExplicitGeneratedType.Reference("ElTreeRenderContentCallback"),
        [("el-tree", "load")] = ExplicitGeneratedType.Reference("ElTreeLoadCallback"),
        [("el-tree", "filterNodeMethod")] = ExplicitGeneratedType.Reference("ElTreeFilterNodeMethod"),
        [("el-tree", "allowDrag")] = ExplicitGeneratedType.Reference("ElTreeAllowDragCallback"),
        [("el-tree", "allowDrop")] = ExplicitGeneratedType.Reference("ElTreeAllowDropCallback"),
        [("el-tree-select", "filterMethod")] = ExplicitGeneratedType.Reference("ElSelectQueryCallback"),
        [("el-tree-select", "remoteMethod")] = ExplicitGeneratedType.Reference("ElSelectQueryCallback"),
        [("el-tree-select", "renderContent")] = ExplicitGeneratedType.Reference("ElTreeRenderContentCallback"),
        [("el-tree-select", "load")] = ExplicitGeneratedType.Reference("ElTreeLoadCallback"),
        [("el-tree-select", "filterNodeMethod")] = ExplicitGeneratedType.Reference("ElTreeFilterNodeMethod"),
        [("el-tree-select", "allowDrag")] = ExplicitGeneratedType.Reference("ElTreeAllowDragCallback"),
        [("el-tree-select", "allowDrop")] = ExplicitGeneratedType.Reference("ElTreeAllowDropCallback"),
        [("el-tree-v2", "filterMethod")] = ExplicitGeneratedType.Reference("ElTreeV2FilterMethod"),
        [("el-upload", "data")] = ExplicitGeneratedType.Value("ElUploadDataValue"),
        [("el-upload", "onPreview")] = ExplicitGeneratedType.Reference("ElUploadPreviewCallback"),
        [("el-upload", "onRemove")] = ExplicitGeneratedType.Reference("ElUploadFileListCallback"),
        [("el-upload", "onSuccess")] = ExplicitGeneratedType.Reference("ElUploadSuccessCallback"),
        [("el-upload", "onError")] = ExplicitGeneratedType.Reference("ElUploadErrorCallback"),
        [("el-upload", "onProgress")] = ExplicitGeneratedType.Reference("ElUploadProgressCallback"),
        [("el-upload", "onChange")] = ExplicitGeneratedType.Reference("ElUploadFileListCallback"),
        [("el-upload", "onExceed")] = ExplicitGeneratedType.Reference("ElUploadExceedCallback"),
        [("el-upload", "beforeUpload")] = ExplicitGeneratedType.Reference("ElUploadBeforeUploadCallback"),
        [("el-upload", "beforeRemove")] = ExplicitGeneratedType.Reference("ElUploadBeforeRemoveCallback"),
        [("el-upload", "httpRequest")] = ExplicitGeneratedType.Reference("ElUploadRequestCallback"),
        [("el-virtualized-select", "filterMethod")] = ExplicitGeneratedType.Reference("ElSelectQueryCallback"),
        [("el-virtualized-select", "remoteMethod")] = ExplicitGeneratedType.Reference("ElSelectQueryCallback"),
        [("el-watermark", "content")] = ExplicitGeneratedType.Value("VueStringOrStringsValue")
    };

    private static readonly Dictionary<string, RawPropMetadata[]> SupplementalPropsByTag = new(StringComparer.Ordinal)
    {
        ["el-config-provider"] =
        [
            new RawPropMetadata("a11y", null, "boolean", false),
            new RawPropMetadata("card", null, "object", false),
            new RawPropMetadata("keyboard-navigation", null, "boolean", false)
        ],
        ["el-date-picker"] =
        [
            new RawPropMetadata("dateFormat", null, "string", false),
            new RawPropMetadata("timeFormat", null, "string", false)
        ],
        ["el-time-picker"] =
        [
            new RawPropMetadata("defaultTime", null, "Date | [Date, Date]", false),
            new RawPropMetadata("dateFormat", null, "string", false),
            new RawPropMetadata("timeFormat", null, "string", false)
        ],
        ["el-virtualized-select"] =
        [
            new RawPropMetadata("tagTooltip", null, "TagTooltipProps", false)
        ],
        ["el-tree-select"] =
        [
            new RawPropMetadata("tagTooltip", null, "TagTooltipProps", false)
        ]
    };

    private static void Main()
    {
        var repositoryRoot = ResolveRepositoryRoot();
        var packageRoot = Path.Combine(repositoryRoot, "src", "ECMAScript.ElementPlus");
        var metadataRoot = Path.Combine(repositoryRoot, ".tmp", "elementplus-inspect", "package");

        var webTypesPath = Path.Combine(metadataRoot, "web-types.json");
        var attributesPath = Path.Combine(metadataRoot, "attributes.json");
        var componentBaselinePath = Path.Combine(metadataRoot, "es", "component.mjs");
        var eventConstantsPath = Path.Combine(metadataRoot, "es", "constants", "event.d.ts");

        EnsureFileExists(webTypesPath, "Element Plus web-types metadata");
        EnsureFileExists(attributesPath, "Element Plus attributes metadata");
        EnsureFileExists(componentBaselinePath, "Element Plus installable component baseline");
        EnsureFileExists(eventConstantsPath, "Element Plus event constants metadata");

        var attributeCatalog = ReadAttributeCatalog(attributesPath);
        var installableComponentExports = ReadInstallableComponentExports(componentBaselinePath);
        ValidateRuntimeComponentExportOverrides(installableComponentExports);
        var updateModelEventName = ReadUpdateModelEventName(eventConstantsPath);

        using var webTypes = JsonDocument.Parse(File.ReadAllText(webTypesPath));
        var html = webTypes.RootElement.GetProperty("contributions").GetProperty("html");

        var components = SupplementInstallableComponents(
            html.GetProperty("vue-components")
            .EnumerateArray()
            .Select(static element => RawComponentMetadata.FromJson(element))
            .Where(component => installableComponentExports.Contains(GetRuntimeComponentExportName(component.ExportName)))
            .GroupBy(static component => component.ExportName, StringComparer.Ordinal)
            .Select(group => ElementPlusComponentMetadata.Merge(
                group.ToArray(),
                attributeCatalog,
                updateModelEventName))
            .ToArray(),
            installableComponentExports,
            metadataRoot)
            .OrderBy(static component => component.ClassName, StringComparer.Ordinal)
            .ToArray();

        var directives = html.GetProperty("attributes")
            .EnumerateArray()
            .Select(static element => ElementPlusDirectiveMetadata.FromJson(element))
            .GroupBy(static directive => directive.ExportName, StringComparer.Ordinal)
            .Select(static group => group.First())
            .OrderBy(static directive => directive.ExportName, StringComparer.Ordinal)
            .ToArray();

        WriteFile(
            Path.Combine(packageRoot, "ElementPlusComponentExports.cs"),
            RenderComponentExports(components));

        WriteFile(
            Path.Combine(packageRoot, "ElementPlusComponentRegistry.cs"),
            RenderComponentRegistry(components));

        WriteFile(
            Path.Combine(packageRoot, "ElementPlus.Components.generated.cs"),
            RenderComponentDefinitions(components, webTypesPath, attributesPath, componentBaselinePath));

        WriteFile(
            Path.Combine(packageRoot, "ElementPlusDirectiveExports.cs"),
            RenderDirectiveExports(directives));

        WriteFile(
            Path.Combine(packageRoot, "ElementPlusDirectiveRegistry.cs"),
            RenderDirectiveRegistry(directives));

        Console.WriteLine($"Generated {components.Length} Element Plus components and {directives.Length} directives.");
    }

    private static string RenderComponentExports(ElementPlusComponentMetadata[] components)
    {
        var builder = new StringBuilder();
        builder.AppendLine("#nullable enable");
        builder.AppendLine();
        builder.AppendLine("namespace ECMAScript.ElementPlus;");
        builder.AppendLine();
        builder.AppendLine("/// <summary>");
        builder.AppendLine("/// Export surface for generated Element Plus components.");
        builder.AppendLine("/// </summary>");
        builder.AppendLine("[ECMAScript(\"element-plus\")]");
        builder.AppendLine("public static class ElComponents");
        builder.AppendLine("{");

        foreach (var component in components)
        {
            builder.AppendLine($"    [ECMAScriptName(\"{component.RuntimeExportName}\")]");
            builder.AppendLine($"    public extern static IElementPlusComponent {component.AuthoringName} {{ get; }}");
            builder.AppendLine();
        }

        builder.AppendLine("}");
        return builder.ToString();
    }

    private static string RenderComponentRegistry(ElementPlusComponentMetadata[] components)
    {
        var builder = new StringBuilder();
        builder.AppendLine("#nullable enable");
        builder.AppendLine();
        builder.AppendLine("namespace ECMAScript.ElementPlus;");
        builder.AppendLine();
        builder.AppendLine("/// <summary>");
        builder.AppendLine("/// Registry of generated Element Plus components.");
        builder.AppendLine("/// </summary>");
        builder.AppendLine("[ECMAScript]");
        builder.AppendLine("[Description(\"@#ElComponentRegistry\")]");
        builder.AppendLine("public sealed record ElComponentRegistry : VueComponentRegistry");
        builder.AppendLine("{");

        foreach (var component in components)
        {
            builder.AppendLine($"    [Description(\"@#{component.AuthoringName}\")]");
            builder.AppendLine($"    public IElementPlusComponent? {component.ClassName} {{ get; init; }}");
            builder.AppendLine();
        }

        builder.AppendLine("}");
        return builder.ToString();
    }

    private static string RenderComponentDefinitions(
        ElementPlusComponentMetadata[] components,
        string webTypesPath,
        string attributesPath,
        string componentsIndexPath)
    {
        var builder = new StringBuilder();
        builder.AppendLine("#nullable enable");
        builder.AppendLine();
        builder.AppendLine("using Microsoft.AspNetCore.Components;");
        builder.AppendLine();
        builder.AppendLine("namespace ECMAScript.ElementPlus;");
        builder.AppendLine();
        builder.AppendLine("// <auto-generated />");
        builder.AppendLine("// Generated from:");
        builder.AppendLine($"// - {GetRepositoryRelativePath(webTypesPath)}");
        builder.AppendLine($"// - {GetRepositoryRelativePath(attributesPath)}");
        builder.AppendLine($"// - {GetRepositoryRelativePath(componentsIndexPath)}");
        builder.AppendLine();

        foreach (var component in components)
        {
            builder.AppendLine("/// <summary>");
            builder.AppendLine($"/// {EscapeXml(component.Description)}");
            builder.AppendLine("/// </summary>");
            builder.AppendLine($"[VueLibraryComponent(\"element-plus\", \"{component.RuntimeExportName}\")]");

            foreach (var emit in component.Emits.Where(RequiresExplicitEmitName))
            {
                builder.AppendLine(RenderVueEmitAttribute(emit));
            }

            builder.AppendLine($"public sealed class {component.ClassName} : {(component.HasDefaultSlot ? "ElContentComponentBase" : "ElComponentBase")}");
            builder.AppendLine("{");

            foreach (var prop in component.Props.Where(static prop => !prop.IsSkipped))
            {
                builder.AppendLine("    [Parameter]");
                if (prop.Required)
                    builder.AppendLine("    [EditorRequired]");
                if (RequiresExplicitPropName(prop))
                    builder.AppendLine($"    [ECMAScriptName(\"{EscapeCSharpString(prop.RuntimeName)}\")]");
                builder.AppendLine($"    public {prop.Type.SourceText} {prop.PropertyName} {{ get; set; }}");
                builder.AppendLine();
            }

            foreach (var slot in component.Slots.Where(static slot => !slot.IsDefault))
            {
                builder.AppendLine("    [Parameter]");
                if (RequiresExplicitSlotName(slot))
                    builder.AppendLine($"    [ECMAScriptName(\"{EscapeCSharpString(slot.RuntimeName)}\")]");
                builder.AppendLine($"    public RenderFragment? {slot.PropertyName} {{ get; set; }}");
                builder.AppendLine();
            }

            foreach (var emit in component.Emits)
            {
                builder.AppendLine("    [Parameter]");
                builder.AppendLine($"    public {emit.CallbackTypeSourceText} {emit.PropertyName} {{ get; set; }}");
                builder.AppendLine();
            }

            builder.AppendLine("}");
            builder.AppendLine();
        }

        return builder.ToString();
    }

    private static string RenderDirectiveExports(ElementPlusDirectiveMetadata[] directives)
    {
        var builder = new StringBuilder();
        builder.AppendLine("#nullable enable");
        builder.AppendLine();
        builder.AppendLine("namespace ECMAScript.ElementPlus;");
        builder.AppendLine();
        builder.AppendLine("/// <summary>");
        builder.AppendLine("/// Export surface for Element Plus directives.");
        builder.AppendLine("/// </summary>");
        builder.AppendLine("[ECMAScript(\"element-plus\")]");
        builder.AppendLine("public static class ElDirectives");
        builder.AppendLine("{");

        foreach (var directive in directives)
        {
            builder.AppendLine($"    [ECMAScriptName(\"{directive.ExportName}\")]");
            builder.AppendLine($"    public extern static {directive.TypeName} {directive.PropertyName} {{ get; }}");
            builder.AppendLine();
        }

        builder.AppendLine("}");
        return builder.ToString();
    }

    private static string RenderDirectiveRegistry(ElementPlusDirectiveMetadata[] directives)
    {
        var builder = new StringBuilder();
        builder.AppendLine("#nullable enable");
        builder.AppendLine();
        builder.AppendLine("namespace ECMAScript.ElementPlus;");
        builder.AppendLine();
        builder.AppendLine("/// <summary>");
        builder.AppendLine("/// Registry of Element Plus directives.");
        builder.AppendLine("/// </summary>");
        builder.AppendLine("[ECMAScript]");
        builder.AppendLine("[Description(\"@#ElDirectiveRegistry\")]");
        builder.AppendLine("public sealed record ElDirectiveRegistry : VueDirectiveRegistry");
        builder.AppendLine("{");

        foreach (var directive in directives)
        {
            builder.AppendLine($"    [Description(\"@#{directive.PropertyName}\")]");
            builder.AppendLine($"    public {directive.TypeName}? {directive.PropertyName} {{ get; init; }}");
            builder.AppendLine();
        }

        builder.AppendLine("}");
        return builder.ToString();
    }

    private static bool RequiresExplicitPropName(ElementPlusPropMetadata prop)
        => !string.Equals(
            prop.RuntimeName,
            ToLowerCamelCase(prop.PropertyName),
            StringComparison.Ordinal);

    private static bool RequiresExplicitSlotName(ElementPlusSlotMetadata slot)
    {
        var conventionalName = slot.PropertyName.EndsWith("Content", StringComparison.Ordinal) &&
                               slot.PropertyName.Length > "Content".Length
            ? slot.PropertyName[..^"Content".Length]
            : slot.PropertyName;
        return !string.Equals(
            slot.RuntimeName,
            ToKebabCase(conventionalName),
            StringComparison.Ordinal);
    }

    private static bool RequiresExplicitEmitName(ElementPlusEmitMetadata emit)
    {
        if (emit.IsModelUpdate)
            return false;

        return emit.PropertyName.Length <= 2 ||
               !emit.PropertyName.StartsWith("On", StringComparison.Ordinal) ||
               !char.IsUpper(emit.PropertyName[2]) ||
               !string.Equals(
                   emit.RuntimeName,
                   ToKebabCase(emit.PropertyName[2..]),
                   StringComparison.Ordinal);
    }

    private static string ToKebabCase(string name)
    {
        var result = new StringBuilder(name.Length + 4);
        for (var index = 0; index < name.Length; index++)
        {
            var character = name[index];
            if (char.IsUpper(character))
            {
                var separatesWord = index > 0 &&
                    (char.IsLower(name[index - 1]) ||
                     char.IsDigit(name[index - 1]) ||
                     index + 1 < name.Length && char.IsLower(name[index + 1]));
                if (separatesWord)
                    result.Append('-');

                result.Append(char.ToLowerInvariant(character));
                continue;
            }

            result.Append(character);
        }

        return result.ToString();
    }

    private static string RenderVueEmitAttribute(ElementPlusEmitMetadata emit)
        => $"[VueLibraryEmit(nameof({emit.PropertyName}), Name = \"{emit.RuntimeName}\")]";

    private static ElementPlusAttributeCatalog ReadAttributeCatalog(string path)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var byTag = new Dictionary<string, TagAttributeMetadata>(StringComparer.Ordinal);

        foreach (var property in document.RootElement.EnumerateObject())
        {
            var separatorIndex = property.Name.IndexOf('/', StringComparison.Ordinal);
            if (separatorIndex <= 0 || separatorIndex >= property.Name.Length - 1)
                continue;

            var tagName = property.Name[..separatorIndex];
            var memberName = property.Name[(separatorIndex + 1)..];
            if (!byTag.TryGetValue(tagName, out var metadata))
            {
                metadata = new TagAttributeMetadata(tagName);
                byTag.Add(tagName, metadata);
            }

            var type = property.Value.TryGetProperty("type", out var typeElement) && typeElement.ValueKind == JsonValueKind.String
                ? typeElement.GetString()
                : null;
            var description = property.Value.TryGetProperty("description", out var descriptionElement) && descriptionElement.ValueKind == JsonValueKind.String
                ? descriptionElement.GetString()
                : null;

            if (string.Equals(type, "event", StringComparison.Ordinal))
            {
                metadata.AddEvent(new RawEventMetadata(memberName, description));
            }
            else
            {
                metadata.AddProp(new RawPropMetadata(
                    memberName,
                    description,
                    type,
                    false));
            }
        }

        return new ElementPlusAttributeCatalog(byTag);
    }

    private static HashSet<string> ReadInstallableComponentExports(string path)
    {
        var content = File.ReadAllText(path);
        var match = Regex.Match(
            content,
            @"var\s+component_default\s*=\s*\[(?<items>[\s\S]*?)\];",
            RegexOptions.CultureInvariant);
        if (!match.Success)
        {
            throw new InvalidOperationException(
                $"Could not locate the Element Plus installable component baseline in '{path}'.");
        }

        return Regex.Matches(match.Groups["items"].Value, @"\bEl[A-Z][A-Za-z0-9]*\b", RegexOptions.CultureInvariant)
            .Select(static match => match.Value)
            .Distinct(StringComparer.Ordinal)
            .ToHashSet(StringComparer.Ordinal);
    }

    private static void ValidateRuntimeComponentExportOverrides(HashSet<string> validComponentExports)
    {
        foreach (var overrideEntry in RuntimeComponentExportOverrides)
        {
            if (!validComponentExports.Contains(overrideEntry.Value))
            {
                throw new InvalidOperationException(
                    $"Element Plus runtime export override '{overrideEntry.Key}' -> '{overrideEntry.Value}' does not exist in the package export baseline.");
            }
        }
    }

    private static string ReadUpdateModelEventName(string path)
    {
        var content = File.ReadAllText(path);
        var match = Regex.Match(
            content,
            @"UPDATE_MODEL_EVENT\s*=\s*""(?<value>[^""]+)""",
            RegexOptions.CultureInvariant);

        return match.Success && match.Groups["value"].Value.Length > 0
            ? match.Groups["value"].Value
            : "update:modelValue";
    }

    private static void EnsureFileExists(string path, string description)
    {
        if (!File.Exists(path))
            throw new InvalidOperationException($"Missing {description}: {path}");
    }

    private static void WriteFile(string path, string content)
    {
        var normalized = content.Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace("\n", Environment.NewLine, StringComparison.Ordinal);
        File.WriteAllText(path, normalized, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    private static string ResolveRepositoryRoot()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Jazor.slnx")))
                return directory.FullName;

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not locate repository root from current directory.");
    }

    private static string GetRepositoryRelativePath(string path)
    {
        var repositoryRoot = ResolveRepositoryRoot();
        return Path.GetRelativePath(repositoryRoot, path).Replace('\\', '/');
    }

    private static string EscapeXml(string value)
        => value
            .Replace("&", "&amp;", StringComparison.Ordinal)
            .Replace("<", "&lt;", StringComparison.Ordinal)
            .Replace(">", "&gt;", StringComparison.Ordinal);

    private static string EscapeCSharpString(string value)
        => value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal);

    private static string ToPascalCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var sanitized = value.Trim();
        sanitized = sanitized.Replace("@", "At", StringComparison.Ordinal);
        sanitized = sanitized.Replace(":", " ", StringComparison.Ordinal);
        sanitized = sanitized.Replace("-", " ", StringComparison.Ordinal);
        sanitized = sanitized.Replace("_", " ", StringComparison.Ordinal);
        sanitized = sanitized.Replace("/", " ", StringComparison.Ordinal);
        sanitized = sanitized.Replace(".", " ", StringComparison.Ordinal);
        sanitized = sanitized.Replace("[", " ", StringComparison.Ordinal);
        sanitized = sanitized.Replace("]", " ", StringComparison.Ordinal);

        var parts = sanitized
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var builder = new StringBuilder();
        foreach (var part in parts)
        {
            if (part.Length == 0)
                continue;

            if (part.Length == 1)
            {
                builder.Append(char.ToUpperInvariant(part[0]));
                continue;
            }

            if (char.IsDigit(part[0]))
            {
                builder.Append('_').Append(part);
                continue;
            }

            builder.Append(char.ToUpperInvariant(part[0]));
            builder.Append(part[1..]);
        }

        return builder.ToString();
    }

    private static string ToLowerCamelCase(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return value.Length == 1
            ? char.ToLowerInvariant(value[0]).ToString()
            : char.ToLowerInvariant(value[0]) + value[1..];
    }

    private static string NormalizePropRuntimeName(string rawName)
    {
        var normalizedRawName = rawName.Trim();
        if (normalizedRawName.Length >= 2 &&
            normalizedRawName[0] == '[' &&
            normalizedRawName[^1] == ']')
        {
            normalizedRawName = normalizedRawName[1..^1].Trim();
        }

        if (string.Equals(rawName, "class", StringComparison.Ordinal) ||
            string.Equals(rawName, "style", StringComparison.Ordinal))
        {
            return normalizedRawName;
        }

        return ToLowerCamelCase(ToPascalCase(normalizedRawName));
    }

    private static string NormalizeEventRuntimeName(string rawName)
    {
        if (!rawName.StartsWith("update:", StringComparison.Ordinal))
            return rawName;

        var suffix = rawName["update:".Length..];
        return "update:" + NormalizePropRuntimeName(suffix);
    }

    private static bool IsBracketedPlaceholderName(string rawName)
    {
        var trimmed = rawName.Trim();
        return trimmed.Length >= 2 &&
               trimmed[0] == '[' &&
               trimmed[^1] == ']';
    }

    private static bool IsBracketedDocumentSectionName(string rawName)
    {
        if (!IsBracketedPlaceholderName(rawName))
            return false;

        var content = rawName.Trim()[1..^1].Trim();
        return content.Contains(' ', StringComparison.Ordinal) ||
               string.Equals(content, "tooltip", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(content, "image viewer slots", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(content, "input props", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(content, "input events", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(content, "input slots", StringComparison.OrdinalIgnoreCase);
    }

    private static string GetUpdateEventRuntimeName(string propRuntimeName, string updateModelEventName)
        => string.Equals(propRuntimeName, "modelValue", StringComparison.Ordinal)
            ? updateModelEventName
            : "update:" + propRuntimeName;

    private static bool IsCanonicalModelRuntimeName(string runtimeName)
        => string.Equals(runtimeName, "modelValue", StringComparison.Ordinal);

    private static string GetRuntimeComponentExportName(string authoringExportName)
        => RuntimeComponentExportOverrides.TryGetValue(authoringExportName, out var runtimeExportName)
            ? runtimeExportName
            : authoringExportName;

    private static ElementPlusComponentMetadata[] SupplementInstallableComponents(
        ElementPlusComponentMetadata[] components,
        HashSet<string> installableComponentExports,
        string metadataRoot)
    {
        var supplemented = components.ToList();
        var byRuntimeExport = supplemented.ToDictionary(
            static component => component.RuntimeExportName,
            StringComparer.Ordinal);
        var byAuthoringName = supplemented.ToDictionary(
            static component => component.AuthoringName,
            StringComparer.Ordinal);

        foreach (var runtimeExportName in installableComponentExports.OrderBy(static name => name, StringComparer.Ordinal))
        {
            if (byRuntimeExport.ContainsKey(runtimeExportName))
                continue;

            var component = BuildSupplementalInstallableComponent(runtimeExportName, byAuthoringName, metadataRoot);
            if (!byRuntimeExport.TryAdd(component.RuntimeExportName, component))
            {
                throw new InvalidOperationException(
                    $"Duplicate runtime export '{component.RuntimeExportName}' while supplementing Element Plus installable components.");
            }

            if (!byAuthoringName.TryAdd(component.AuthoringName, component))
            {
                throw new InvalidOperationException(
                    $"Duplicate authoring component '{component.AuthoringName}' while supplementing Element Plus installable components.");
            }

            supplemented.Add(component);
        }

        return supplemented.ToArray();
    }

    private static ElementPlusComponentMetadata BuildSupplementalInstallableComponent(
        string runtimeExportName,
        IReadOnlyDictionary<string, ElementPlusComponentMetadata> existingComponentsByAuthoringName,
        string metadataRoot)
        => runtimeExportName switch
        {
            "ElAutoResizer" => CreateAutoResizerComponentMetadata(metadataRoot),
            "ElCollapseTransition" => CreateCollapseTransitionComponentMetadata(metadataRoot),
            "ElPopper" => CreatePopperComponentMetadata(metadataRoot),
            "ElTreeSelect" => CreateTreeSelectComponentMetadata(existingComponentsByAuthoringName, metadataRoot),
            _ => throw new InvalidOperationException(
                $"Element Plus installable component '{runtimeExportName}' is not covered by web-types metadata and no supplemental metadata builder exists.")
        };

    private static ElementPlusComponentMetadata CreateAutoResizerComponentMetadata(string metadataRoot)
    {
        EnsureFileExists(
            Path.Combine(metadataRoot, "es", "components", "table-v2", "src", "auto-resizer.d.ts"),
            "Element Plus ElAutoResizer prop metadata");
        EnsureFileExists(
            Path.Combine(metadataRoot, "es", "components", "table-v2", "src", "components", "auto-resizer.mjs.map"),
            "Element Plus ElAutoResizer slot metadata");

        return CreateSupplementalComponentMetadata(
            tagName: "el-auto-resizer",
            authoringName: "ElAutoResizer",
            runtimeExportName: "ElAutoResizer",
            description: "ElAutoResizer",
            props:
            [
                CreateSupplementalProp("disableWidth", "DisableWidth", GeneratedType.Value("bool").AsOptional(required: false)),
                CreateSupplementalProp("disableHeight", "DisableHeight", GeneratedType.Value("bool").AsOptional(required: false)),
                CreateSupplementalProp("onResize", "OnResize", GeneratedType.Reference("ElAutoResizerResizeCallback").AsOptional(required: false))
            ],
            slots:
            [
                CreateSupplementalDefaultSlot()
            ],
            emits: []);
    }

    private static ElementPlusComponentMetadata CreateCollapseTransitionComponentMetadata(string metadataRoot)
    {
        EnsureFileExists(
            Path.Combine(metadataRoot, "es", "components", "collapse-transition", "src", "collapse-transition.vue.d.ts"),
            "Element Plus ElCollapseTransition slot metadata");

        return CreateSupplementalComponentMetadata(
            tagName: "el-collapse-transition",
            authoringName: "ElCollapseTransition",
            runtimeExportName: "ElCollapseTransition",
            description: "ElCollapseTransition",
            props: [],
            slots:
            [
                CreateSupplementalDefaultSlot()
            ],
            emits: []);
    }

    private static ElementPlusComponentMetadata CreatePopperComponentMetadata(string metadataRoot)
    {
        EnsureFileExists(
            Path.Combine(metadataRoot, "es", "components", "popper", "src", "popper.d.ts"),
            "Element Plus ElPopper prop metadata");
        EnsureFileExists(
            Path.Combine(metadataRoot, "es", "components", "popper", "src", "popper.vue.d.ts"),
            "Element Plus ElPopper slot metadata");

        return CreateSupplementalComponentMetadata(
            tagName: "el-popper",
            authoringName: "ElPopper",
            runtimeExportName: "ElPopper",
            description: "ElPopper",
            props:
            [
                CreateSupplementalProp("role", "Role", GeneratedType.Reference("string?"))
            ],
            slots:
            [
                CreateSupplementalDefaultSlot()
            ],
            emits: []);
    }

    private static ElementPlusComponentMetadata CreateTreeSelectComponentMetadata(
        IReadOnlyDictionary<string, ElementPlusComponentMetadata> existingComponentsByAuthoringName,
        string metadataRoot)
    {
        EnsureFileExists(
            Path.Combine(metadataRoot, "es", "components", "tree-select", "src", "tree-select.vue.d.ts"),
            "Element Plus ElTreeSelect prop metadata");
        EnsureFileExists(
            Path.Combine(metadataRoot, "es", "components", "tree-select", "src", "tree-select.vue_vue_type_script_lang.mjs.map"),
            "Element Plus ElTreeSelect composition metadata");
        EnsureFileExists(
            Path.Combine(metadataRoot, "es", "components", "tree-select", "src", "tree.mjs.map"),
            "Element Plus ElTreeSelect tree slot metadata");

        var select = GetRequiredSupplementalSourceComponent(existingComponentsByAuthoringName, "ElSelect");
        var tree = GetRequiredSupplementalSourceComponent(existingComponentsByAuthoringName, "ElTree");

        return CreateSupplementalComponentMetadata(
            tagName: "el-tree-select",
            authoringName: "ElTreeSelect",
            runtimeExportName: "ElTreeSelect",
            description: "ElTreeSelect",
            props: MergeSupplementalProps(
                [
                    CreateSupplementalProp("cacheData", "CacheData", GeneratedType.Reference("VueValue[]?")),
                    CreateSupplementalProp("tagTooltip", "TagTooltip", GeneratedType.Reference("ElTagTooltipProps?"))
                ],
                select.Props,
                tree.Props),
            slots: MergeSupplementalSlots(
                [
                    CreateSupplementalDefaultSlot()
                ],
                select.Slots.Where(static slot => !slot.IsDefault)),
            emits: MergeSupplementalEmits(select.Emits, tree.Emits));
    }

    private static ElementPlusComponentMetadata GetRequiredSupplementalSourceComponent(
        IReadOnlyDictionary<string, ElementPlusComponentMetadata> componentsByAuthoringName,
        string authoringName)
    {
        if (componentsByAuthoringName.TryGetValue(authoringName, out var component))
            return component;

        throw new InvalidOperationException(
            $"Element Plus supplemental component generation requires source component '{authoringName}', but it was not found.");
    }

    private static ElementPlusComponentMetadata CreateSupplementalComponentMetadata(
        string tagName,
        string authoringName,
        string runtimeExportName,
        string description,
        ElementPlusPropMetadata[] props,
        ElementPlusSlotMetadata[] slots,
        ElementPlusEmitMetadata[] emits)
        => new(
            tagName,
            authoringName,
            authoringName,
            runtimeExportName,
            description,
            props,
            slots,
            emits);

    private static ElementPlusPropMetadata CreateSupplementalProp(
        string runtimeName,
        string propertyName,
        GeneratedType type,
        bool required = false,
        bool acceptsBinding = false)
        => new(
            runtimeName,
            propertyName,
            type,
            required,
            IsSkipped: false,
            acceptsBinding);

    private static ElementPlusSlotMetadata CreateSupplementalDefaultSlot()
        => new("default", ChildContentPropertyName, IsDefault: true);

    private static ElementPlusSlotMetadata[] MergeSupplementalSlots(params IEnumerable<ElementPlusSlotMetadata>[] groups)
    {
        var merged = new List<ElementPlusSlotMetadata>();
        var indexes = new Dictionary<string, int>(StringComparer.Ordinal);

        foreach (var group in groups)
        {
            foreach (var slot in group)
            {
                if (!indexes.TryAdd(slot.RuntimeName, merged.Count))
                {
                    merged[indexes[slot.RuntimeName]] = slot;
                    continue;
                }

                merged.Add(slot);
            }
        }

        EnsureUniqueSupplementalPropertyNames(
            merged.Select(static slot => slot.PropertyName).Where(static name => !string.IsNullOrWhiteSpace(name)),
            "slot");
        return merged.ToArray();
    }

    private static ElementPlusEmitMetadata[] MergeSupplementalEmits(params IEnumerable<ElementPlusEmitMetadata>[] groups)
    {
        var merged = new List<ElementPlusEmitMetadata>();
        var indexes = new Dictionary<string, int>(StringComparer.Ordinal);

        foreach (var group in groups)
        {
            foreach (var emit in group)
            {
                if (!indexes.TryAdd(emit.RuntimeName, merged.Count))
                {
                    merged[indexes[emit.RuntimeName]] = emit;
                    continue;
                }

                merged.Add(emit);
            }
        }

        EnsureUniqueSupplementalPropertyNames(
            merged.Select(static emit => emit.PropertyName).Where(static name => !string.IsNullOrWhiteSpace(name)),
            "emit");
        return merged.ToArray();
    }

    private static ElementPlusPropMetadata[] MergeSupplementalProps(params IEnumerable<ElementPlusPropMetadata>[] groups)
    {
        var merged = new List<ElementPlusPropMetadata>();
        var indexes = new Dictionary<string, int>(StringComparer.Ordinal);

        foreach (var group in groups)
        {
            foreach (var prop in group)
            {
                if (!indexes.TryAdd(prop.RuntimeName, merged.Count))
                {
                    var current = merged[indexes[prop.RuntimeName]];
                    var preferred = current.IsSkipped && !prop.IsSkipped
                        ? prop
                        : !current.IsSkipped && prop.IsSkipped
                            ? current
                            : current.Type.SourceText == "VueValue?" && prop.Type.SourceText != "VueValue?"
                                ? prop
                                : current;

                    merged[indexes[prop.RuntimeName]] = preferred with
                    {
                        Required = current.Required || prop.Required,
                        IsSkipped = current.IsSkipped && prop.IsSkipped,
                        AcceptsBinding = current.AcceptsBinding || prop.AcceptsBinding
                    };
                    continue;
                }

                merged.Add(prop);
            }
        }

        EnsureUniqueSupplementalPropertyNames(
            merged.Where(static prop => !prop.IsSkipped)
                .Select(static prop => prop.PropertyName)
                .Where(static name => !string.IsNullOrWhiteSpace(name)),
            "prop");
        return merged.ToArray();
    }

    private static void EnsureUniqueSupplementalPropertyNames(
        IEnumerable<string> propertyNames,
        string memberKind)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var propertyName in propertyNames)
        {
            if (seen.Add(propertyName))
                continue;

            throw new InvalidOperationException(
                $"Duplicate Element Plus supplemental {memberKind} property name '{propertyName}'.");
        }
    }

    private static string GetUniquePropPropertyName(string basePropertyName, HashSet<string> occupiedNames)
    {
        var normalizedBaseName = string.IsNullOrWhiteSpace(basePropertyName)
            ? "Value"
            : basePropertyName;
        if (!occupiedNames.Contains(normalizedBaseName))
            return normalizedBaseName;

        var suffixed = normalizedBaseName.EndsWith("Value", StringComparison.Ordinal)
            ? normalizedBaseName
            : normalizedBaseName + "Value";
        if (!occupiedNames.Contains(suffixed))
            return suffixed;

        for (var suffix = 2; ; suffix++)
        {
            var candidate = suffixed + suffix;
            if (!occupiedNames.Contains(candidate))
                return candidate;
        }
    }

    private static string GetUniqueEmitPropertyName(string basePropertyName, HashSet<string> occupiedNames)
    {
        var normalizedBaseName = string.IsNullOrWhiteSpace(basePropertyName)
            ? "OnEvent"
            : basePropertyName;
        if (!occupiedNames.Contains(normalizedBaseName))
            return normalizedBaseName;

        var suffixed = normalizedBaseName.EndsWith("Event", StringComparison.Ordinal)
            ? normalizedBaseName
            : normalizedBaseName + "Event";
        if (!occupiedNames.Contains(suffixed))
            return suffixed;

        for (var suffix = 2; ; suffix++)
        {
            var candidate = suffixed + suffix;
            if (!occupiedNames.Contains(candidate))
                return candidate;
        }
    }

    private static string GetUniqueSlotPropertyName(string basePropertyName, HashSet<string> occupiedNames)
    {
        var normalizedBaseName = string.IsNullOrWhiteSpace(basePropertyName)
            ? "Slot"
            : basePropertyName;
        if (!occupiedNames.Contains(normalizedBaseName))
            return normalizedBaseName;

        var suffixed = normalizedBaseName.EndsWith("Slot", StringComparison.Ordinal)
            ? normalizedBaseName
            : normalizedBaseName + "Slot";
        if (!occupiedNames.Contains(suffixed))
            return suffixed;

        for (var suffix = 2; ; suffix++)
        {
            var candidate = suffixed + suffix;
            if (!occupiedNames.Contains(candidate))
                return candidate;
        }
    }

    private static string ToNormalEventPropertyName(string runtimeName)
        => "On" + ToPascalCase(runtimeName);

    private static GeneratedType ResolvePropType(string tagName, RawPropMetadata prop)
    {
        if (string.Equals(prop.RawName, "class", StringComparison.Ordinal) ||
            string.Equals(prop.RawName, "style", StringComparison.Ordinal))
        {
            return GeneratedType.Reference("VueValue?");
        }

        if (IsBracketedDocumentSectionName(prop.RawName) ||
            prop.RawName.Contains('/', StringComparison.Ordinal))
        {
            return GeneratedType.Reference("VueValue?");
        }

        if (ExplicitPropTypeOverrides.TryGetValue((tagName, prop.RuntimeName), out var overrideType))
        {
            return overrideType.ToGeneratedType(prop.Required);
        }

        var expression = NormalizeTypeExpression(prop.TypeExpression);
        if (string.IsNullOrWhiteSpace(expression))
            return GeneratedType.Reference("VueValue?");

        if (ContainsTopLevelArrow(expression))
            throw CreateUnsupportedFunctionPropTypeException(tagName, prop);

        var tokens = SplitTopLevel(expression, '|')
            .Select(NormalizeTypeToken)
            .Where(static token => !string.IsNullOrWhiteSpace(token))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (tokens.Length == 0)
            return GeneratedType.Reference("VueValue?");

        if (IsTeleportTarget(tokens))
            return GeneratedType.Value("VueTeleportTarget").AsOptional(prop.Required);

        if (IsStringComponent(tokens))
            return GeneratedType.Value("VueStringComponentValue").AsOptional(prop.Required);

        if (IsBooleanStringNumber(tokens))
            return GeneratedType.Value("VueBooleanStringNumberValue").AsOptional(prop.Required);

        if (IsBooleanString(tokens))
            return GeneratedType.Value("VueBooleanStringValue").AsOptional(prop.Required);

        if (IsBooleanNumber(tokens))
            return GeneratedType.Value("VueBooleanNumberValue").AsOptional(prop.Required);

        if (IsStringNumber(tokens))
            return GeneratedType.Value("VueStringNumberValue").AsOptional(prop.Required);

        if (TryResolveSingleOrRangeValue(tokens, out var singleOrRangeType))
            return singleOrRangeType.AsOptional(prop.Required);

        if (tokens.All(IsStringLikeToken))
        {
            if (HasExactStringLiteralSet(tokens, "large", "default", "small"))
                return GeneratedType.Value("ElComponentSize").AsOptional(prop.Required);

            if (HasExactStringLiteralSet(tokens, "dark", "light"))
                return GeneratedType.Value("ElPopperEffect").AsOptional(prop.Required);

            return GeneratedType.Reference("string?").AsOptional(prop.Required);
        }

        if (tokens.Any(IsObjectLikeToken))
        {
            if (tokens.Length == 2 && tokens.Any(static token => token == "string"))
            {
                if (prop.RuntimeName.Contains("class", StringComparison.OrdinalIgnoreCase))
                    return GeneratedType.Value("VueClassValue").AsOptional(prop.Required);

                if (prop.RuntimeName.Contains("style", StringComparison.OrdinalIgnoreCase))
                    return GeneratedType.Value("VueStyleValue").AsOptional(prop.Required);
            }

            if (tokens.Length == 1)
                return GeneratedType.Reference("VueDictionary?").AsOptional(prop.Required);

            return GeneratedType.Reference("VueValue?").AsOptional(prop.Required);
        }

        if (tokens.Length == 1)
            return MapSingleToken(tokens[0], prop.Required);

        if (tokens.Any(static token => token.Contains("Awaitable", StringComparison.Ordinal) ||
                                       token.Contains("Promise<", StringComparison.Ordinal) ||
                                       token.Contains("unknown", StringComparison.OrdinalIgnoreCase) ||
                                       token.Contains("any", StringComparison.OrdinalIgnoreCase)))
        {
            return GeneratedType.Reference("VueValue?").AsOptional(prop.Required);
        }

        return GeneratedType.Reference("VueValue?").AsOptional(prop.Required);
    }

    private static GeneratedType MapSingleToken(string token, bool required)
    {
        if (ContainsTopLevelArrow(token) || string.Equals(token, "Function", StringComparison.Ordinal))
            throw new InvalidOperationException(
                $"Element Plus function prop type token '{token}' requires an explicit override or named contract.");

        return token switch
        {
            "boolean" => GeneratedType.Value("bool").AsOptional(required),
            "number" => GeneratedType.Value("Number").AsOptional(required),
            "string" => GeneratedType.Reference("string?").AsOptional(required),
            "string[]" => GeneratedType.Reference("string[]?").AsOptional(required),
            "number[]" => GeneratedType.Reference("Number[]?").AsOptional(required),
            "VueProps" => GeneratedType.Reference("VueProps?").AsOptional(required),
            "VueDictionary" => GeneratedType.Reference("VueDictionary?").AsOptional(required),
            "CSSProperties" => GeneratedType.Value("VueStyleValue").AsOptional(required),
            "StyleValue" => GeneratedType.Value("VueStyleValue").AsOptional(required),
            "InputAutoSize" => GeneratedType.Value("ElInputAutoSize").AsOptional(required),
            "TagTooltipProps" => GeneratedType.Reference("ElTagTooltipProps").AsOptional(required),
            "Date" => GeneratedType.Reference("Date?").AsOptional(required),
            "RegExp" => GeneratedType.Reference("RegExp?").AsOptional(required),
            "CSSSelector" => GeneratedType.Reference("string?").AsOptional(required),
            "HTMLElement" => GeneratedType.Reference("HTMLElement?").AsOptional(required),
            "Element" => GeneratedType.Reference("Element?").AsOptional(required),
            "Component" => GeneratedType.Reference("IVueComponent?").AsOptional(required),
            "RouteLocationRaw" => GeneratedType.Value("RouteLocationRaw").AsOptional(required),
            "Headers" => GeneratedType.Reference("Headers?").AsOptional(required),
            "XMLHttpRequest" => GeneratedType.Reference("XMLHttpRequest?").AsOptional(required),
            "File" => GeneratedType.Reference("File?").AsOptional(required),
            "Blob" => GeneratedType.Reference("Blob?").AsOptional(required),
            "Error" => GeneratedType.Reference("Error?").AsOptional(required),
            "object" => GeneratedType.Reference("VueDictionary?").AsOptional(required),
            _ when token.StartsWith("Array<", StringComparison.Ordinal) => MapArrayToken(token, required),
            _ when token.EndsWith("[]", StringComparison.Ordinal) => MapArrayShorthandToken(token, required),
            _ when token.StartsWith("[", StringComparison.Ordinal) => MapTupleToken(token, required),
            _ when token.StartsWith("Record<", StringComparison.Ordinal) => GeneratedType.Reference("VueDictionary?").AsOptional(required),
            _ when token.Contains("Record<", StringComparison.Ordinal) => GeneratedType.Reference("VueDictionary?").AsOptional(required),
            _ when token.Contains("Awaitable", StringComparison.Ordinal) => GeneratedType.Reference("VueValue?").AsOptional(required),
            _ when token.Contains("Promise<", StringComparison.Ordinal) => GeneratedType.Reference("VueValue?").AsOptional(required),
            _ when token.Contains("unknown", StringComparison.OrdinalIgnoreCase) => GeneratedType.Reference("VueValue?").AsOptional(required),
            _ when token.Contains("any", StringComparison.OrdinalIgnoreCase) => GeneratedType.Reference("VueValue?").AsOptional(required),
            _ when IsStringLiteralToken(token) => GeneratedType.Reference("string?").AsOptional(required),
            _ => GeneratedType.Reference("VueValue?").AsOptional(required)
        };
    }

    private static GeneratedType MapArrayToken(string token, bool required)
    {
        var inner = token[6..^1];
        var parts = SplitTopLevel(inner, '|')
            .Select(NormalizeTypeToken)
            .Where(static part => !string.IsNullOrWhiteSpace(part))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (parts.Length == 1)
        {
            if (parts[0] == "string")
                return GeneratedType.Reference("string[]?").AsOptional(required);

            if (parts[0] == "number")
                return GeneratedType.Reference("Number[]?").AsOptional(required);
        }

        if (IsStringNumber(parts))
            return GeneratedType.Reference("VueValue[]?").AsOptional(required);

        if (parts.All(static part => part == "string"))
            return GeneratedType.Reference("string[]?").AsOptional(required);

        return GeneratedType.Reference("VueValue[]?").AsOptional(required);
    }

    private static GeneratedType MapArrayShorthandToken(string token, bool required)
    {
        if (string.Equals(token, "string[]", StringComparison.Ordinal))
            return GeneratedType.Reference("string[]?").AsOptional(required);

        if (string.Equals(token, "number[]", StringComparison.Ordinal))
            return GeneratedType.Reference("Number[]?").AsOptional(required);

        return GeneratedType.Reference("VueValue[]?").AsOptional(required);
    }

    private static GeneratedType MapTupleToken(string token, bool required)
    {
        if (string.Equals(token, "[Date, Date]", StringComparison.Ordinal))
            return GeneratedType.Value("VueDatePair").AsOptional(required);

        if (string.Equals(token, "[number, number]", StringComparison.Ordinal))
            return GeneratedType.Value("VueNumberPair").AsOptional(required);

        if (string.Equals(token, "[Font]", StringComparison.Ordinal))
            return GeneratedType.Reference("VueProps?").AsOptional(required);

        return GeneratedType.Reference("VueValue?").AsOptional(required);
    }

    private static string NormalizeTypeExpression(string? expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            return string.Empty;

        var normalized = expression.Trim();
        normalized = normalized.Replace("string|string[]", "string | string[]", StringComparison.Ordinal);
        normalized = normalized.Replace("string|number", "string | number", StringComparison.Ordinal);
        normalized = normalized.Replace("number|string", "number | string", StringComparison.Ordinal);
        normalized = normalized.Replace("boolean|string", "boolean | string", StringComparison.Ordinal);
        normalized = normalized.Replace("string|boolean", "string | boolean", StringComparison.Ordinal);
        normalized = normalized.Replace("boolean|string|number", "boolean | string | number", StringComparison.Ordinal);
        normalized = normalized.Replace("string|number|boolean", "string | number | boolean", StringComparison.Ordinal);
        normalized = normalized.Replace("objectrefer to  doc", "object", StringComparison.OrdinalIgnoreCase);
        normalized = Regex.Replace(normalized, @"\s+", " ");
        return normalized.Trim();
    }

    private static string NormalizeTypeToken(string token)
    {
        var normalized = NormalizeTypeExpression(token);
        if (normalized.Length == 0)
            return string.Empty;

        if (string.Equals(normalized, "Array", StringComparison.Ordinal))
            return "Array";

        if (string.Equals(normalized, "object", StringComparison.OrdinalIgnoreCase))
            return "object";

        if (string.Equals(normalized, "Function", StringComparison.Ordinal))
            return "Function";

        if (string.Equals(normalized, "CSSProperties", StringComparison.Ordinal))
            return "CSSProperties";

        if (string.Equals(normalized, "StyleValue", StringComparison.Ordinal))
            return "StyleValue";

        if (string.Equals(normalized, "Component", StringComparison.Ordinal))
            return "Component";

        if (string.Equals(normalized, "HTMLElement", StringComparison.Ordinal))
            return "HTMLElement";

        if (string.Equals(normalized, "Element", StringComparison.Ordinal))
            return "Element";

        if (string.Equals(normalized, "CSSSelector", StringComparison.Ordinal))
            return "CSSSelector";

        if (string.Equals(normalized, "Date", StringComparison.Ordinal))
            return "Date";

        if (string.Equals(normalized, "RegExp", StringComparison.Ordinal))
            return "RegExp";

        if (string.Equals(normalized, "RouteLocationRaw", StringComparison.Ordinal))
            return "RouteLocationRaw";

        if (string.Equals(normalized, "Headers", StringComparison.Ordinal))
            return "Headers";

        if (string.Equals(normalized, "XMLHttpRequest", StringComparison.Ordinal))
            return "XMLHttpRequest";

        if (string.Equals(normalized, "File", StringComparison.Ordinal))
            return "File";

        if (string.Equals(normalized, "Blob", StringComparison.Ordinal))
            return "Blob";

        if (string.Equals(normalized, "Error", StringComparison.Ordinal))
            return "Error";

        if (normalized.StartsWith("string see", StringComparison.OrdinalIgnoreCase))
            return "string";

        if (normalized.StartsWith("number see", StringComparison.OrdinalIgnoreCase))
            return "number";

        if (normalized.StartsWith("boolean see", StringComparison.OrdinalIgnoreCase))
            return "boolean";

        if (normalized.StartsWith("Record<", StringComparison.Ordinal) ||
            normalized.Contains("Record<", StringComparison.Ordinal) ||
            normalized.StartsWith("Array<", StringComparison.Ordinal) ||
            normalized.EndsWith("[]", StringComparison.Ordinal) ||
            normalized.StartsWith("[", StringComparison.Ordinal) ||
            normalized.Contains("Awaitable", StringComparison.Ordinal) ||
            normalized.Contains("Promise<", StringComparison.Ordinal) ||
            normalized.Contains("=>", StringComparison.Ordinal))
        {
            return normalized;
        }

        return normalized;
    }

    private static InvalidOperationException CreateUnsupportedFunctionPropTypeException(string tagName, RawPropMetadata prop)
        => new(
            $"Element Plus function prop '{tagName}.{prop.RuntimeName}' with type expression '{prop.TypeExpression ?? "<null>"}' requires an explicit override or named contract.");

    private static bool ContainsTopLevelArrow(string expression)
    {
        var depthAngle = 0;
        var depthParen = 0;
        var depthBracket = 0;
        var depthBrace = 0;
        var inSingleQuote = false;
        var inDoubleQuote = false;

        for (var index = 0; index < expression.Length - 1; index++)
        {
            var current = expression[index];
            var next = expression[index + 1];

            ToggleQuoteState(current, ref inSingleQuote, ref inDoubleQuote);
            if (inSingleQuote || inDoubleQuote)
                continue;

            UpdateDepth(current, ref depthAngle, ref depthParen, ref depthBracket, ref depthBrace);
            if (current == '=' &&
                next == '>' &&
                depthAngle == 0 &&
                depthParen == 0 &&
                depthBracket == 0 &&
                depthBrace == 0)
            {
                return true;
            }
        }

        return false;
    }

    private static string[] SplitTopLevel(string value, char separator)
    {
        if (string.IsNullOrWhiteSpace(value))
            return [];

        var result = new List<string>();
        var depthAngle = 0;
        var depthParen = 0;
        var depthBracket = 0;
        var depthBrace = 0;
        var inSingleQuote = false;
        var inDoubleQuote = false;
        var start = 0;

        for (var index = 0; index < value.Length; index++)
        {
            var current = value[index];
            ToggleQuoteState(current, ref inSingleQuote, ref inDoubleQuote);
            if (!inSingleQuote && !inDoubleQuote)
            {
                UpdateDepth(current, ref depthAngle, ref depthParen, ref depthBracket, ref depthBrace);
                if (current == separator &&
                    depthAngle == 0 &&
                    depthParen == 0 &&
                    depthBracket == 0 &&
                    depthBrace == 0)
                {
                    result.Add(value[start..index].Trim());
                    start = index + 1;
                }
            }
        }

        result.Add(value[start..].Trim());
        return result.ToArray();
    }

    private static void ToggleQuoteState(char current, ref bool inSingleQuote, ref bool inDoubleQuote)
    {
        if (current == '\'' && !inDoubleQuote)
        {
            inSingleQuote = !inSingleQuote;
            return;
        }

        if (current == '"' && !inSingleQuote)
        {
            inDoubleQuote = !inDoubleQuote;
        }
    }

    private static void UpdateDepth(
        char current,
        ref int depthAngle,
        ref int depthParen,
        ref int depthBracket,
        ref int depthBrace)
    {
        switch (current)
        {
            case '<':
                depthAngle++;
                break;
            case '>':
                if (depthAngle > 0)
                    depthAngle--;
                break;
            case '(':
                depthParen++;
                break;
            case ')':
                if (depthParen > 0)
                    depthParen--;
                break;
            case '[':
                depthBracket++;
                break;
            case ']':
                if (depthBracket > 0)
                    depthBracket--;
                break;
            case '{':
                depthBrace++;
                break;
            case '}':
                if (depthBrace > 0)
                    depthBrace--;
                break;
        }
    }

    private static bool IsTeleportTarget(string[] tokens)
        => tokens.All(static token => token is "CSSSelector" or "HTMLElement" or "Element") &&
           tokens.Length >= 1 &&
           tokens.Length <= 2;

    private static bool IsStringComponent(string[] tokens)
    {
        var filtered = tokens.Where(static token => token != "string" && !IsStringLiteralToken(token)).ToArray();
        return filtered.Length == 1 && filtered[0] == "Component";
    }

    private static bool IsBooleanStringNumber(string[] tokens)
        => tokens.Any(static token => token == "boolean") &&
           tokens.Any(IsNumberLikeToken) &&
           tokens.Any(IsStringLikeToken) &&
           tokens.All(static token => token == "boolean" || IsNumberLikeToken(token) || IsStringLikeToken(token));

    private static bool IsBooleanNumber(string[] tokens)
        => tokens.Any(static token => token == "boolean") &&
           tokens.Any(IsNumberLikeToken) &&
           tokens.All(static token => token == "boolean" || IsNumberLikeToken(token));

    private static bool IsBooleanString(string[] tokens)
        => tokens.Any(static token => token == "boolean") &&
           tokens.Any(IsStringLikeToken) &&
           tokens.All(static token => token == "boolean" || IsStringLikeToken(token));

    private static bool IsStringNumber(string[] tokens)
        => tokens.Any(IsNumberLikeToken) &&
           tokens.Any(IsStringLikeToken) &&
           tokens.All(static token => IsNumberLikeToken(token) || IsStringLikeToken(token));

    private static bool TryResolveSingleOrRangeValue(string[] tokens, out GeneratedType type)
    {
        if (HasExactTokenSet(tokens, "Date", "[Date, Date]"))
        {
            type = GeneratedType.Value("VueDateSingleOrRangeValue");
            return true;
        }

        if (HasExactTokenSet(tokens, "string", "[string, string]"))
        {
            type = GeneratedType.Value("VueStringSingleOrRangeValue");
            return true;
        }

        type = default!;
        return false;
    }

    private static bool HasExactTokenSet(string[] tokens, params string[] expected)
        => tokens.OrderBy(static token => token, StringComparer.Ordinal)
            .SequenceEqual(
                expected.OrderBy(static token => token, StringComparer.Ordinal),
                StringComparer.Ordinal);

    private static bool HasExactStringLiteralSet(string[] tokens, params string[] expected)
    {
        var values = tokens
            .Where(IsStringLikeToken)
            .Select(GetStringLiteralValueOrToken)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static item => item, StringComparer.Ordinal)
            .ToArray();
        var expectedValues = expected
            .OrderBy(static item => item, StringComparer.Ordinal)
            .ToArray();
        return values.SequenceEqual(expectedValues, StringComparer.Ordinal);
    }

    private static bool IsNumberLikeToken(string token)
        => token == "number";

    private static bool IsStringLikeToken(string token)
        => token == "string" || IsStringLiteralToken(token);

    private static bool IsStringLiteralToken(string token)
        => token.Length >= 2 &&
           token[0] == '\'' &&
           token[^1] == '\'';

    private static string GetStringLiteralValueOrToken(string token)
        => IsStringLiteralToken(token)
            ? token[1..^1]
            : token;

    private static bool IsObjectLikeToken(string token)
        => token == "object" ||
           token == "VueProps" ||
           token == "VueDictionary" ||
           token.StartsWith("Record<", StringComparison.Ordinal) ||
           token.Contains("Record<", StringComparison.Ordinal);

    private static bool TryResolveRuntimeType(string sourceText, out Type? type)
    {
        var normalized = sourceText.Trim();
        if (normalized.EndsWith("?", StringComparison.Ordinal))
        {
            var innerSource = normalized[..^1];
            if (TryResolveRuntimeType(innerSource, out var innerType) && innerType is not null)
            {
                if (innerType.IsValueType)
                {
                    type = typeof(Nullable<>).MakeGenericType(innerType);
                    return true;
                }

                type = innerType;
                return true;
            }

            type = null;
            return false;
        }

        if (normalized.EndsWith("[]", StringComparison.Ordinal))
        {
            var elementSource = normalized[..^2];
            if (TryResolveRuntimeType(elementSource, out var elementType) && elementType is not null)
            {
                type = elementType.MakeArrayType();
                return true;
            }

            type = null;
            return false;
        }

        if (RuntimeTypeMap.TryGetValue(normalized, out var resolvedType))
        {
            type = resolvedType;
            return true;
        }

        type = null;
        return false;
    }

    private static string? ResolveRuntimeTypeName(string sourceText)
    {
        if (TryResolveRuntimeType(sourceText, out var runtimeType) &&
            runtimeType?.FullName is { Length: > 0 } runtimeTypeName)
        {
            return runtimeTypeName;
        }

        return null;
    }

    private sealed class ElementPlusAttributeCatalog
    {
        private readonly Dictionary<string, TagAttributeMetadata> _byTag;

        public ElementPlusAttributeCatalog(Dictionary<string, TagAttributeMetadata> byTag)
        {
            _byTag = byTag;
        }

        public bool TryGetTag(string tagName, out TagAttributeMetadata metadata)
            => _byTag.TryGetValue(tagName, out metadata!);
    }

    private sealed class TagAttributeMetadata
    {
        private readonly Dictionary<string, RawPropMetadata> _propsByRuntimeName = new(StringComparer.Ordinal);
        private readonly Dictionary<string, RawEventMetadata> _eventsByRuntimeName = new(StringComparer.Ordinal);
        private readonly List<RawPropMetadata> _props = new();
        private readonly List<RawEventMetadata> _events = new();

        public TagAttributeMetadata(string tagName)
        {
            TagName = tagName;
        }

        public string TagName { get; }

        public IReadOnlyList<RawPropMetadata> Props => _props;

        public IReadOnlyList<RawEventMetadata> Events => _events;

        public void AddProp(RawPropMetadata prop)
        {
            var key = NormalizePropRuntimeName(prop.RawName);
            if (_propsByRuntimeName.ContainsKey(key))
                return;

            _propsByRuntimeName.Add(key, prop);
            _props.Add(prop);
        }

        public void AddEvent(RawEventMetadata emit)
        {
            var key = NormalizeEventRuntimeName(emit.RawName);
            if (_eventsByRuntimeName.ContainsKey(key))
                return;

            _eventsByRuntimeName.Add(key, emit);
            _events.Add(emit);
        }
    }

    private sealed record RawComponentMetadata(
        string TagName,
        string ExportName,
        string ClassName,
        string Description,
        IReadOnlyList<RawPropMetadata> Props,
        IReadOnlyList<RawSlotMetadata> Slots,
        IReadOnlyList<RawEventMetadata> Events)
    {
        public static RawComponentMetadata FromJson(JsonElement element)
        {
            var tagName = element.GetProperty("name").GetString()
                          ?? throw new InvalidOperationException("Element Plus component is missing tag name.");
            var exportName = element.TryGetProperty("source", out var source) &&
                             source.TryGetProperty("symbol", out var symbol) &&
                             symbol.ValueKind == JsonValueKind.String
                ? symbol.GetString()!
                : ToPascalCase(tagName);
            var description = element.TryGetProperty("description", out var descriptionElement) &&
                              descriptionElement.ValueKind == JsonValueKind.String
                ? descriptionElement.GetString() ?? tagName
                : tagName;

            var props = element.TryGetProperty("props", out var propsElement)
                ? propsElement.EnumerateArray().Select(ReadRawPropMetadata).ToArray()
                : [];
            var slots = element.TryGetProperty("slots", out var slotsElement)
                ? slotsElement.EnumerateArray().Select(ReadRawSlotMetadata).ToArray()
                : [];
            var events = element.TryGetProperty("js", out var jsElement) &&
                         jsElement.TryGetProperty("events", out var eventsElement)
                ? eventsElement.EnumerateArray().Select(ReadRawEventMetadata).ToArray()
                : [];

            return new RawComponentMetadata(
                tagName,
                exportName,
                exportName,
                description,
                props,
                slots,
                events);
        }

        private static RawPropMetadata ReadRawPropMetadata(JsonElement element)
        {
            var rawName = element.GetProperty("name").GetString()
                          ?? throw new InvalidOperationException("Element Plus prop is missing name.");
            var description = element.TryGetProperty("description", out var descriptionElement) &&
                              descriptionElement.ValueKind == JsonValueKind.String
                ? descriptionElement.GetString()
                : null;
            var typeExpression = element.TryGetProperty("type", out var typeElement)
                ? ReadTypeExpression(typeElement)
                : null;
            var required = element.TryGetProperty("required", out var requiredElement) &&
                           requiredElement.ValueKind == JsonValueKind.True;

            return new RawPropMetadata(rawName, description, typeExpression, required);
        }

        private static RawSlotMetadata ReadRawSlotMetadata(JsonElement element)
        {
            var rawName = element.GetProperty("name").GetString()
                          ?? throw new InvalidOperationException("Element Plus slot is missing name.");
            var description = element.TryGetProperty("description", out var descriptionElement) &&
                              descriptionElement.ValueKind == JsonValueKind.String
                ? descriptionElement.GetString()
                : null;
            return new RawSlotMetadata(rawName, description);
        }

        private static RawEventMetadata ReadRawEventMetadata(JsonElement element)
        {
            var rawName = element.GetProperty("name").GetString()
                          ?? throw new InvalidOperationException("Element Plus event is missing name.");
            var description = element.TryGetProperty("description", out var descriptionElement) &&
                              descriptionElement.ValueKind == JsonValueKind.String
                ? descriptionElement.GetString()
                : null;
            return new RawEventMetadata(rawName, description);
        }

        private static string? ReadTypeExpression(JsonElement typeElement)
        {
            if (typeElement.ValueKind == JsonValueKind.String)
                return typeElement.GetString();

            if (typeElement.ValueKind != JsonValueKind.Array || typeElement.GetArrayLength() == 0)
                return null;

            return string.Join(
                " | ",
                typeElement.EnumerateArray()
                    .Select(static item => item.ValueKind switch
                    {
                        JsonValueKind.String => item.GetString(),
                        JsonValueKind.Object when item.TryGetProperty("name", out var name) && name.ValueKind == JsonValueKind.String
                            => name.GetString(),
                        _ => null
                    })
                    .Where(static item => !string.IsNullOrWhiteSpace(item)));
        }
    }

    private sealed record RawPropMetadata(
        string RawName,
        string? Description,
        string? TypeExpression,
        bool Required)
    {
        public string RuntimeName => NormalizePropRuntimeName(RawName);
    }

    private sealed record RawSlotMetadata(
        string RawName,
        string? Description);

    private sealed record RawEventMetadata(
        string RawName,
        string? Description)
    {
        public string RuntimeName => NormalizeEventRuntimeName(RawName);
    }

    private sealed record GeneratedType(
        string SourceText,
        bool IsValueType)
    {
        public static GeneratedType Value(string sourceText)
            => new(sourceText, true);

        public static GeneratedType Reference(string sourceText)
            => new(sourceText, false);

        public GeneratedType AsOptional(bool required)
        {
            if (required || SourceText.EndsWith("?", StringComparison.Ordinal))
                return this;

            return this with { SourceText = SourceText + "?" };
        }
    }

    private sealed record ExplicitGeneratedType(
        string SourceText,
        bool IsValueType)
    {
        public static ExplicitGeneratedType Value(string sourceText)
            => new(sourceText, true);

        public static ExplicitGeneratedType Reference(string sourceText)
            => new(sourceText, false);

        public GeneratedType ToGeneratedType(bool required)
            => (IsValueType ? GeneratedType.Value(SourceText) : GeneratedType.Reference(SourceText))
                .AsOptional(required);
    }

    private sealed record ElementPlusPropMetadata(
        string RuntimeName,
        string PropertyName,
        GeneratedType Type,
        bool Required,
        bool IsSkipped,
        bool AcceptsBinding);

    private sealed record ElementPlusSlotMetadata(
        string RuntimeName,
        string PropertyName,
        bool IsDefault);

    private sealed record ElementPlusEmitMetadata(
        string RuntimeName,
        string PropertyName,
        bool IsModelUpdate,
        string? PayloadTypeSourceText,
        string? PayloadTypeRuntimeName)
    {
        public string CallbackTypeSourceText
            => PayloadTypeSourceText is null
                ? "EventCallback"
                : $"EventCallback<{PayloadTypeSourceText}>";
    }

    private sealed record ElementPlusComponentMetadata(
        string TagName,
        string ClassName,
        string AuthoringName,
        string RuntimeExportName,
        string Description,
        ElementPlusPropMetadata[] Props,
        ElementPlusSlotMetadata[] Slots,
        ElementPlusEmitMetadata[] Emits)
    {
        public bool HasDefaultSlot => Slots.Any(static slot => slot.IsDefault);

        public static ElementPlusComponentMetadata Merge(
            RawComponentMetadata[] components,
            ElementPlusAttributeCatalog attributeCatalog,
            string updateModelEventName)
        {
            if (components.Length == 0)
                throw new InvalidOperationException("Element Plus component merge received no items.");

            var first = components[0];
            var description = components
                .Select(static component => component.Description)
                .FirstOrDefault(static item => !string.IsNullOrWhiteSpace(item))
                ?? first.TagName;

            var rawProps = MergeProps(first.TagName, components, attributeCatalog);
            var rawEvents = MergeEvents(first.TagName, components, attributeCatalog);
            var rawSlots = MergeSlots(components);

            var updateEvents = rawEvents
                .Select(static item => item.RuntimeName)
                .Where(static item => item.StartsWith("update:", StringComparison.Ordinal))
                .ToHashSet(StringComparer.Ordinal);

            var propOccupiedNames = new HashSet<string>(ReservedPropertyNames, StringComparer.Ordinal);
            var props = new List<ElementPlusPropMetadata>(rawProps.Count);
            var bindablePropsByUpdateEvent = new Dictionary<string, ElementPlusPropMetadata>(StringComparer.Ordinal);

            foreach (var rawProp in rawProps)
            {
                var isSkipped = string.Equals(rawProp.RawName, "class", StringComparison.Ordinal) ||
                                string.Equals(rawProp.RawName, "style", StringComparison.Ordinal) ||
                                IsBracketedDocumentSectionName(rawProp.RawName) ||
                                rawProp.RawName.Contains('/', StringComparison.Ordinal);
                var propertyName = GetUniquePropPropertyName(ToPascalCase(rawProp.RawName), propOccupiedNames);
                if (!isSkipped)
                    propOccupiedNames.Add(propertyName);

                var type = ResolvePropType(first.TagName, rawProp);
                var updateEventRuntimeName = GetUpdateEventRuntimeName(rawProp.RuntimeName, updateModelEventName);
                var acceptsBinding = !isSkipped &&
                                     (updateEvents.Contains(updateEventRuntimeName) ||
                                      IsCanonicalModelRuntimeName(rawProp.RuntimeName));
                var prop = new ElementPlusPropMetadata(
                    rawProp.RuntimeName,
                    propertyName,
                    type,
                    rawProp.Required,
                    isSkipped,
                    acceptsBinding);
                props.Add(prop);

                if (acceptsBinding)
                    bindablePropsByUpdateEvent[updateEventRuntimeName] = prop;
            }

            var emitOccupiedNames = new HashSet<string>(propOccupiedNames, StringComparer.Ordinal);
            var emits = new List<ElementPlusEmitMetadata>(rawEvents.Count);
            var emittedModelUpdateRuntimeNames = new HashSet<string>(StringComparer.Ordinal);

            ElementPlusEmitMetadata CreateModelUpdateEmit(ElementPlusPropMetadata bindableProp)
            {
                var propertyName = bindableProp.PropertyName + "Changed";
                var uniquePropertyName = GetUniqueEmitPropertyName(propertyName, emitOccupiedNames);
                emitOccupiedNames.Add(uniquePropertyName);

                return new ElementPlusEmitMetadata(
                    GetUpdateEventRuntimeName(bindableProp.RuntimeName, updateModelEventName),
                    uniquePropertyName,
                    IsModelUpdate: true,
                    bindableProp.Type.SourceText,
                    ResolveRuntimeTypeName(bindableProp.Type.SourceText));
            }

            foreach (var rawEvent in rawEvents)
            {
                if (bindablePropsByUpdateEvent.TryGetValue(rawEvent.RuntimeName, out var bindableProp))
                {
                    emits.Add(CreateModelUpdateEmit(bindableProp));
                    emittedModelUpdateRuntimeNames.Add(rawEvent.RuntimeName);
                    continue;
                }

                var emitPropertyName = GetUniqueEmitPropertyName(
                    ToNormalEventPropertyName(rawEvent.RuntimeName),
                    emitOccupiedNames);
                emitOccupiedNames.Add(emitPropertyName);

                emits.Add(new ElementPlusEmitMetadata(
                    rawEvent.RuntimeName,
                    emitPropertyName,
                    IsModelUpdate: false,
                    PayloadTypeSourceText: null,
                    PayloadTypeRuntimeName: null));
            }

            foreach (var bindableProp in props.Where(static prop => prop.AcceptsBinding))
            {
                var updateEventRuntimeName = GetUpdateEventRuntimeName(bindableProp.RuntimeName, updateModelEventName);
                if (emittedModelUpdateRuntimeNames.Contains(updateEventRuntimeName))
                    continue;

                emits.Add(CreateModelUpdateEmit(bindableProp));
                emittedModelUpdateRuntimeNames.Add(updateEventRuntimeName);
            }

            var slots = ResolveSlotPropertyNames(rawSlots, propOccupiedNames, emitOccupiedNames);

            return new ElementPlusComponentMetadata(
                first.TagName,
                first.ClassName,
                first.ExportName,
                GetRuntimeComponentExportName(first.ExportName),
                description,
                props.ToArray(),
                slots,
                emits.ToArray());
        }

        private static List<RawPropMetadata> MergeProps(
            string tagName,
            RawComponentMetadata[] components,
            ElementPlusAttributeCatalog attributeCatalog)
        {
            var merged = new List<RawPropMetadata>();
            var indexes = new Dictionary<string, int>(StringComparer.Ordinal);

            foreach (var component in components)
            {
                foreach (var prop in component.Props)
                {
                    AddOrMergeProp(merged, indexes, prop);
                }
            }

            if (attributeCatalog.TryGetTag(tagName, out var attributeMetadata))
            {
                foreach (var prop in attributeMetadata.Props)
                {
                    AddOrMergeProp(merged, indexes, prop, preferIncomingType: true);
                }
            }

            if (SupplementalPropsByTag.TryGetValue(tagName, out var supplementalProps))
            {
                foreach (var prop in supplementalProps)
                {
                    AddOrMergeProp(merged, indexes, prop, preferIncomingType: true);
                }
            }

            return merged;
        }

        private static void AddOrMergeProp(
            List<RawPropMetadata> merged,
            Dictionary<string, int> indexes,
            RawPropMetadata incoming,
            bool preferIncomingType = false)
        {
            var key = incoming.RuntimeName;
            if (!indexes.TryGetValue(key, out var index))
            {
                indexes.Add(key, merged.Count);
                merged.Add(incoming);
                return;
            }

            var current = merged[index];
            var typeExpression = preferIncomingType && !string.IsNullOrWhiteSpace(incoming.TypeExpression)
                ? incoming.TypeExpression
                : string.IsNullOrWhiteSpace(current.TypeExpression)
                    ? incoming.TypeExpression
                    : current.TypeExpression;
            var description = !string.IsNullOrWhiteSpace(current.Description)
                ? current.Description
                : incoming.Description;
            var required = current.Required || incoming.Required;
            var rawName = IsBracketedPlaceholderName(current.RawName) && !IsBracketedPlaceholderName(incoming.RawName)
                ? incoming.RawName
                : current.RawName;

            merged[index] = current with
            {
                RawName = rawName,
                Description = description,
                TypeExpression = typeExpression,
                Required = required
            };
        }

        private static List<RawEventMetadata> MergeEvents(
            string tagName,
            RawComponentMetadata[] components,
            ElementPlusAttributeCatalog attributeCatalog)
        {
            var merged = new List<RawEventMetadata>();
            var seen = new HashSet<string>(StringComparer.Ordinal);

            foreach (var component in components)
            {
                foreach (var emit in component.Events)
                {
                    if (IsBracketedDocumentSectionName(emit.RawName))
                        continue;

                    if (seen.Add(emit.RuntimeName))
                        merged.Add(emit with { RawName = emit.RuntimeName });
                }
            }

            if (attributeCatalog.TryGetTag(tagName, out var attributeMetadata))
            {
                foreach (var emit in attributeMetadata.Events)
                {
                    if (IsBracketedDocumentSectionName(emit.RawName))
                        continue;

                    var runtimeName = emit.RuntimeName;
                    if (seen.Add(runtimeName))
                        merged.Add(emit with { RawName = runtimeName });
                }
            }

            return merged;
        }

        private static List<RawSlotMetadata> MergeSlots(RawComponentMetadata[] components)
        {
            var merged = new List<RawSlotMetadata>();
            var seen = new HashSet<string>(StringComparer.Ordinal);

            foreach (var component in components)
            {
                foreach (var slot in component.Slots)
                {
                    if (IsBracketedDocumentSectionName(slot.RawName))
                        continue;

                    if (seen.Add(slot.RawName))
                        merged.Add(slot);
                }
            }

            return merged;
        }

        private static ElementPlusSlotMetadata[] ResolveSlotPropertyNames(
            List<RawSlotMetadata> rawSlots,
            HashSet<string> propOccupiedNames,
            HashSet<string> emitOccupiedNames)
        {
            var occupiedNames = new HashSet<string>(ReservedPropertyNames, StringComparer.Ordinal);
            foreach (var name in propOccupiedNames)
                occupiedNames.Add(name);
            foreach (var name in emitOccupiedNames)
                occupiedNames.Add(name);

            var resolved = new List<ElementPlusSlotMetadata>(rawSlots.Count);
            foreach (var slot in rawSlots)
            {
                var isDefault = string.Equals(slot.RawName, "default", StringComparison.Ordinal);
                if (isDefault)
                {
                    resolved.Add(new ElementPlusSlotMetadata(slot.RawName, ChildContentPropertyName, true));
                    continue;
                }

                var propertyName = GetUniqueSlotPropertyName(ToPascalCase(slot.RawName), occupiedNames);
                occupiedNames.Add(propertyName);
                resolved.Add(new ElementPlusSlotMetadata(slot.RawName, propertyName, false));
            }

            return resolved.ToArray();
        }
    }

    private sealed record ElementPlusDirectiveMetadata(
        string ExportName,
        string PropertyName,
        string TypeName)
    {
        public static ElementPlusDirectiveMetadata FromJson(JsonElement element)
        {
            var exportName = element.TryGetProperty("source", out var source) &&
                             source.TryGetProperty("symbol", out var symbol) &&
                             symbol.ValueKind == JsonValueKind.String
                ? symbol.GetString()!
                : throw new InvalidOperationException("Element Plus directive is missing export symbol.");

            return exportName switch
            {
                "ElInfiniteScroll" => new ElementPlusDirectiveMetadata(exportName, "InfiniteScroll", "ElDirective"),
                "ElLoading" => new ElementPlusDirectiveMetadata("ElLoadingDirective", "Loading", "VueDirective<ElDirectiveValue>"),
                _ => new ElementPlusDirectiveMetadata(exportName, exportName, "ElDirective")
            };
        }
    }
}
