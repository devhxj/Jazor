#!/usr/bin/env dotnet run
#:project ../../src/ECMAScript.Vue3/ECMAScript.Vue3.csproj

#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
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
        ["Delegate"] = typeof(Delegate),
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
        ["VueNumberOrNumbersValue"] = typeof(VueNumberOrNumbersValue),
        ["VueStringNumberObjectValue"] = typeof(VueStringNumberObjectValue),
        ["VueBooleanStringNumberObjectValue"] = typeof(VueBooleanStringNumberObjectValue),
        ["VueStringOrStringsValue"] = typeof(VueStringOrStringsValue),
        ["VueStringRegExpValue"] = typeof(VueStringRegExpValue),
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
        ["RegExp"] = typeof(ECMAScript.RegExp)
    };

    private static readonly Dictionary<(string TagName, string RuntimeName), ExplicitGeneratedType> ExplicitPropTypeOverrides = new()
    {
        [("el-config-provider", "locale")] = ExplicitGeneratedType.Reference("ElementPlusLanguage"),
        [("el-config-provider", "experimentalFeatures")] = ExplicitGeneratedType.Reference("VueProps"),
        [("el-config-provider", "button")] = ExplicitGeneratedType.Reference("ElementPlusButtonConfig"),
        [("el-config-provider", "card")] = ExplicitGeneratedType.Reference("ElementPlusCardConfig"),
        [("el-config-provider", "dialog")] = ExplicitGeneratedType.Reference("ElementPlusDialogConfig"),
        [("el-config-provider", "link")] = ExplicitGeneratedType.Reference("ElementPlusLinkConfig"),
        [("el-config-provider", "message")] = ExplicitGeneratedType.Reference("ElementPlusMessageConfig"),
        [("el-config-provider", "emptyValues")] = ExplicitGeneratedType.Reference("VueValue[]"),
        [("el-config-provider", "valueOnClear")] = ExplicitGeneratedType.Value("ElementPlusValueOnClearValue"),
        [("el-config-provider", "table")] = ExplicitGeneratedType.Reference("ElementPlusTableConfig"),
        [("el-breadcrumb-item", "to")] = ExplicitGeneratedType.Value("RouteLocationRaw"),
        [("el-calendar", "modelValue")] = ExplicitGeneratedType.Reference("Date"),
        [("el-calendar", "range")] = ExplicitGeneratedType.Value("VueDatePair"),
        [("el-card", "shadow")] = ExplicitGeneratedType.Value("ElementPlusCardShadow"),
        [("el-col", "xs")] = ExplicitGeneratedType.Value("ElementPlusColSizeValue"),
        [("el-col", "sm")] = ExplicitGeneratedType.Value("ElementPlusColSizeValue"),
        [("el-col", "md")] = ExplicitGeneratedType.Value("ElementPlusColSizeValue"),
        [("el-col", "lg")] = ExplicitGeneratedType.Value("ElementPlusColSizeValue"),
        [("el-col", "xl")] = ExplicitGeneratedType.Value("ElementPlusColSizeValue"),
        [("el-color-picker-panel", "hueSliderClass")] = ExplicitGeneratedType.Value("VueClassValue"),
        [("el-color-picker-panel", "hueSliderStyle")] = ExplicitGeneratedType.Value("VueStyleValue"),
        [("el-dialog", "transition")] = ExplicitGeneratedType.Value("VueTransitionValue"),
        [("el-dropdown", "trigger")] = ExplicitGeneratedType.Value("ElementPlusDropdownTriggerValue"),
        [("el-dropdown", "buttonProps")] = ExplicitGeneratedType.Reference("ElementPlusButtonProps"),
        [("el-pagination", "pagerCount")] = ExplicitGeneratedType.Value("Number"),
        [("el-progress", "percentage")] = ExplicitGeneratedType.Value("Number"),
        [("el-table", "tooltipEffect")] = ExplicitGeneratedType.Reference("string"),
        [("el-table", "showOverflowTooltip")] = ExplicitGeneratedType.Value("ElementPlusTableOverflowTooltipValue"),
        [("el-table", "tooltipOptions")] = ExplicitGeneratedType.Reference("ElementPlusTableOverflowTooltipOptions"),
        [("el-table", "defaultSort")] = ExplicitGeneratedType.Reference("ElementPlusTableSort"),
        [("el-table", "treeProps")] = ExplicitGeneratedType.Reference("ElementPlusTableTreeProps"),
        [("el-table-column", "showOverflowTooltip")] = ExplicitGeneratedType.Value("ElementPlusTableOverflowTooltipValue"),
        [("el-table-column", "sortOrders")] = ExplicitGeneratedType.Reference("ElementPlusTableSortOrder?[]"),
        [("el-table-column", "filters")] = ExplicitGeneratedType.Reference("ElementPlusTableFilterItem[]"),
        [("el-form", "scrollIntoViewOptions")] = ExplicitGeneratedType.Value("ScrollIntoViewArg"),
        [("el-cascader", "emptyValues")] = ExplicitGeneratedType.Reference("VueValue[]"),
        [("el-cascader", "valueOnClear")] = ExplicitGeneratedType.Value("ElementPlusValueOnClearValue"),
        [("el-cascader", "fitInputWidth")] = ExplicitGeneratedType.Value("VueBooleanNumberValue"),
        [("el-cascader", "fallbackPlacements")] = ExplicitGeneratedType.Reference("string[]"),
        [("el-color-picker", "emptyValues")] = ExplicitGeneratedType.Reference("VueValue[]"),
        [("el-color-picker", "valueOnClear")] = ExplicitGeneratedType.Value("ElementPlusValueOnClearValue"),
        [("el-date-picker", "emptyValues")] = ExplicitGeneratedType.Reference("VueValue[]"),
        [("el-date-picker", "valueOnClear")] = ExplicitGeneratedType.Value("ElementPlusValueOnClearValue"),
        [("el-date-picker", "format")] = ExplicitGeneratedType.Reference("string"),
        [("el-date-picker", "valueFormat")] = ExplicitGeneratedType.Reference("string"),
        [("el-date-picker", "dateFormat")] = ExplicitGeneratedType.Reference("string"),
        [("el-date-picker", "timeFormat")] = ExplicitGeneratedType.Reference("string"),
        [("el-date-picker", "popperOptions")] = ExplicitGeneratedType.Reference("VueDictionary"),
        [("el-date-picker", "fallbackPlacements")] = ExplicitGeneratedType.Reference("string[]"),
        [("el-date-picker", "placement")] = ExplicitGeneratedType.Reference("string"),
        [("el-date-picker", "defaultValue")] = ExplicitGeneratedType.Value("VueDateSingleOrRangeValue"),
        [("el-date-picker", "defaultTime")] = ExplicitGeneratedType.Value("VueDateSingleOrRangeValue"),
        [("el-date-picker", "id")] = ExplicitGeneratedType.Value("VueStringSingleOrRangeValue"),
        [("el-date-picker", "name")] = ExplicitGeneratedType.Value("VueStringSingleOrRangeValue"),
        [("el-date-picker-panel", "defaultValue")] = ExplicitGeneratedType.Value("VueDateSingleOrRangeValue"),
        [("el-date-picker-panel", "defaultTime")] = ExplicitGeneratedType.Value("VueDateSingleOrRangeValue"),
        [("el-date-picker-panel", "valueFormat")] = ExplicitGeneratedType.Reference("string"),
        [("el-date-picker-panel", "dateFormat")] = ExplicitGeneratedType.Reference("string"),
        [("el-date-picker-panel", "timeFormat")] = ExplicitGeneratedType.Reference("string"),
        [("el-divider", "borderStyle")] = ExplicitGeneratedType.Reference("string"),
        [("el-image", "scrollContainer")] = ExplicitGeneratedType.Value("VueStringHtmlElementValue"),
        [("el-input", "autosize")] = ExplicitGeneratedType.Value("ElementPlusInputAutoSize"),
        [("el-input", "max")] = ExplicitGeneratedType.Value("VueStringNumberValue"),
        [("el-input", "min")] = ExplicitGeneratedType.Value("VueStringNumberValue"),
        [("el-input", "step")] = ExplicitGeneratedType.Value("VueStringNumberValue"),
        [("el-input", "inputStyle")] = ExplicitGeneratedType.Value("VueStyleValue"),
        [("el-input-tag", "delimiter")] = ExplicitGeneratedType.Value("VueStringRegExpValue"),
        [("el-input-number", "valueOnClear")] = ExplicitGeneratedType.Value("ElementPlusValueOnClearValue"),
        [("el-menu-item", "route")] = ExplicitGeneratedType.Value("RouteLocationRaw"),
        [("el-popover", "visible")] = ExplicitGeneratedType.Value("bool"),
        [("el-popover", "trigger")] = ExplicitGeneratedType.Value("ElementPlusTooltipTriggerValue"),
        [("el-popover", "triggerKeys")] = ExplicitGeneratedType.Reference("string[]"),
        [("el-select", "emptyValues")] = ExplicitGeneratedType.Reference("VueValue[]"),
        [("el-select", "valueOnClear")] = ExplicitGeneratedType.Value("ElementPlusValueOnClearValue"),
        [("el-time-picker", "emptyValues")] = ExplicitGeneratedType.Reference("VueValue[]"),
        [("el-time-picker", "valueOnClear")] = ExplicitGeneratedType.Value("ElementPlusValueOnClearValue"),
        [("el-time-picker", "format")] = ExplicitGeneratedType.Reference("string"),
        [("el-time-picker", "valueFormat")] = ExplicitGeneratedType.Reference("string"),
        [("el-time-picker", "dateFormat")] = ExplicitGeneratedType.Reference("string"),
        [("el-time-picker", "timeFormat")] = ExplicitGeneratedType.Reference("string"),
        [("el-time-picker", "popperOptions")] = ExplicitGeneratedType.Reference("VueDictionary"),
        [("el-time-picker", "fallbackPlacements")] = ExplicitGeneratedType.Reference("string[]"),
        [("el-time-picker", "placement")] = ExplicitGeneratedType.Reference("string"),
        [("el-time-picker", "defaultValue")] = ExplicitGeneratedType.Value("VueDateSingleOrRangeValue"),
        [("el-time-picker", "defaultTime")] = ExplicitGeneratedType.Value("VueDateSingleOrRangeValue"),
        [("el-time-picker", "id")] = ExplicitGeneratedType.Value("VueStringSingleOrRangeValue"),
        [("el-time-picker", "name")] = ExplicitGeneratedType.Value("VueStringSingleOrRangeValue"),
        [("el-time-select", "emptyValues")] = ExplicitGeneratedType.Reference("VueValue[]"),
        [("el-time-select", "valueOnClear")] = ExplicitGeneratedType.Value("ElementPlusValueOnClearValue"),
        [("el-select-v2", "emptyValues")] = ExplicitGeneratedType.Reference("VueValue[]"),
        [("el-select-v2", "valueOnClear")] = ExplicitGeneratedType.Value("ElementPlusValueOnClearValue"),
        [("el-select-v2", "fitInputWidth")] = ExplicitGeneratedType.Value("VueBooleanNumberValue"),
        [("el-select-v2", "tagTooltip")] = ExplicitGeneratedType.Reference("ElementPlusTagTooltipProps"),
        [("el-select-v2", "props")] = ExplicitGeneratedType.Reference("ElementPlusSelectPropsAlias"),
        [("el-segmented", "size")] = ExplicitGeneratedType.Value("ElementPlusComponentSize"),
        [("el-slider", "modelValue")] = ExplicitGeneratedType.Value("VueNumberOrNumbersValue"),
        [("el-scrollbar", "wrapStyle")] = ExplicitGeneratedType.Value("VueStyleValue"),
        [("el-scrollbar", "viewStyle")] = ExplicitGeneratedType.Value("VueStyleValue"),
        [("el-skeleton", "throttle")] = ExplicitGeneratedType.Value("ElementPlusThrottleValue"),
        [("el-select", "props")] = ExplicitGeneratedType.Reference("ElementPlusSelectPropsAlias"),
        [("el-select", "fallbackPlacements")] = ExplicitGeneratedType.Reference("string[]"),
        [("el-checkbox-group", "props")] = ExplicitGeneratedType.Reference("ElementPlusCheckboxOptionPropsAlias"),
        [("el-checkbox-group", "modelValue")] = ExplicitGeneratedType.Reference("VueStringNumberValue[]"),
        [("el-checkbox", "value")] = ExplicitGeneratedType.Value("VueBooleanStringNumberObjectValue"),
        [("el-checkbox", "label")] = ExplicitGeneratedType.Value("VueBooleanStringNumberObjectValue"),
        [("el-checkbox-button", "value")] = ExplicitGeneratedType.Value("VueBooleanStringNumberObjectValue"),
        [("el-checkbox-button", "label")] = ExplicitGeneratedType.Value("VueBooleanStringNumberObjectValue"),
        [("el-mention", "props")] = ExplicitGeneratedType.Reference("ElementPlusMentionOptionPropsAlias"),
        [("el-mention", "options")] = ExplicitGeneratedType.Reference("ElementPlusMentionOption[]"),
        [("el-mention", "prefix")] = ExplicitGeneratedType.Value("VueStringOrStringsValue"),
        [("el-mention", "popperOptions")] = ExplicitGeneratedType.Reference("VueDictionary"),
        [("el-radio-group", "props")] = ExplicitGeneratedType.Reference("ElementPlusRadioOptionPropsAlias"),
        [("el-input-number", "modelValue")] = ExplicitGeneratedType.Value("Number"),
        [("el-segmented", "props")] = ExplicitGeneratedType.Reference("ElementPlusSegmentedPropsAlias"),
        [("el-space", "alignment")] = ExplicitGeneratedType.Reference("string"),
        [("el-space", "spacer")] = ExplicitGeneratedType.Value("VueStringNumberVNodeValue"),
        [("el-space", "size")] = ExplicitGeneratedType.Value("ElementPlusSpaceSizeValue"),
        [("el-tooltip", "fallbackPlacements")] = ExplicitGeneratedType.Reference("string[]"),
        [("el-tooltip", "trigger")] = ExplicitGeneratedType.Value("ElementPlusTooltipTriggerValue"),
        [("el-tooltip", "triggerKeys")] = ExplicitGeneratedType.Reference("string[]"),
        [("el-transfer", "modelValue")] = ExplicitGeneratedType.Reference("VueStringNumberValue[]"),
        [("el-dropdown-item", "command")] = ExplicitGeneratedType.Value("VueStringNumberObjectValue"),
        [("el-transfer", "data")] = ExplicitGeneratedType.Reference("ElementPlusTransferDataItem[]"),
        [("el-transfer", "targetOrder")] = ExplicitGeneratedType.Value("ElementPlusTransferTargetOrder"),
        [("el-transfer", "titles")] = ExplicitGeneratedType.Reference("ElementPlusTransferTextPair"),
        [("el-transfer", "buttonTexts")] = ExplicitGeneratedType.Reference("ElementPlusTransferTextPair"),
        [("el-transfer", "format")] = ExplicitGeneratedType.Reference("ElementPlusTransferFormat"),
        [("el-transfer", "props")] = ExplicitGeneratedType.Reference("ElementPlusTransferPropsAlias"),
        [("el-transfer", "leftDefaultChecked")] = ExplicitGeneratedType.Reference("VueStringNumberValue[]"),
        [("el-transfer", "rightDefaultChecked")] = ExplicitGeneratedType.Reference("VueStringNumberValue[]"),
        [("el-tree", "props")] = ExplicitGeneratedType.Reference("ElementPlusTreeOptionProps"),
        [("el-tree", "defaultExpandedKeys")] = ExplicitGeneratedType.Reference("VueStringNumberValue[]"),
        [("el-tree", "defaultCheckedKeys")] = ExplicitGeneratedType.Reference("VueStringNumberValue[]"),
        [("el-tree-select", "fallbackPlacements")] = ExplicitGeneratedType.Reference("string[]"),
        [("el-tree-select", "defaultExpandedKeys")] = ExplicitGeneratedType.Reference("VueStringNumberValue[]"),
        [("el-tree-select", "defaultCheckedKeys")] = ExplicitGeneratedType.Reference("VueStringNumberValue[]"),
        [("el-tree-v2", "props")] = ExplicitGeneratedType.Reference("ElementPlusTreeOptionProps"),
        [("el-tree-v2", "defaultExpandedKeys")] = ExplicitGeneratedType.Reference("VueStringNumberValue[]"),
        [("el-tree-v2", "defaultCheckedKeys")] = ExplicitGeneratedType.Reference("VueStringNumberValue[]"),
        [("el-upload", "headers")] = ExplicitGeneratedType.Value("VueHeadersValue"),
        [("el-upload", "fileList")] = ExplicitGeneratedType.Reference("ElementPlusUploadUserFile[]"),
        [("el-cascader", "props")] = ExplicitGeneratedType.Reference("ElementPlusCascaderProps"),
        [("el-cascader-panel", "props")] = ExplicitGeneratedType.Reference("ElementPlusCascaderProps"),
        [("el-virtualized-select", "emptyValues")] = ExplicitGeneratedType.Reference("VueValue[]"),
        [("el-virtualized-select", "valueOnClear")] = ExplicitGeneratedType.Value("ElementPlusValueOnClearValue"),
        [("el-virtualized-select", "props")] = ExplicitGeneratedType.Reference("ElementPlusSelectPropsAlias"),
        [("el-virtualized-select", "fallbackPlacements")] = ExplicitGeneratedType.Reference("string[]"),
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
        builder.AppendLine("public static class ElementPlusComponents");
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
        builder.AppendLine("[Description(\"@#ElementPlusComponentRegistry\")]");
        builder.AppendLine("public sealed record ElementPlusComponentRegistry : VueComponentRegistry");
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
            builder.AppendLine("[VueLibraryStyle(\"element-plus/dist/index.css\")]");
            builder.AppendLine("[VueLibraryPluginRequirement(\"element-plus\")]");
            builder.AppendLine("[VueProp(nameof(CssClass), Name = \"class\")]");
            builder.AppendLine("[VueProp(nameof(CssStyle), Name = \"style\")]");

            foreach (var prop in component.Props.Where(static prop => !prop.IsSkipped))
            {
                builder.AppendLine(RenderVuePropAttribute(prop));
            }

            foreach (var slot in component.Slots)
            {
                if (slot.IsDefault)
                {
                    builder.AppendLine("[VueSlot(nameof(ChildContent), IsDefault = true)]");
                }
                else
                {
                    builder.AppendLine($"[VueSlot(nameof({slot.PropertyName}), Name = \"{slot.RuntimeName}\")]");
                }
            }

            foreach (var emit in component.Emits)
            {
                builder.AppendLine(RenderVueEmitAttribute(emit));
            }

            builder.AppendLine($"public sealed class {component.ClassName} : {(component.HasDefaultSlot ? "ElementPlusContentComponentBase" : "ElementPlusComponentBase")}");
            builder.AppendLine("{");

            foreach (var prop in component.Props.Where(static prop => !prop.IsSkipped))
            {
                builder.AppendLine("    [Parameter]");
                builder.AppendLine($"    public {prop.Type.SourceText} {prop.PropertyName} {{ get; set; }}");
                builder.AppendLine();
            }

            foreach (var slot in component.Slots.Where(static slot => !slot.IsDefault))
            {
                builder.AppendLine("    [Parameter]");
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
        builder.AppendLine("public static class ElementPlusDirectives");
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
        builder.AppendLine("[Description(\"@#ElementPlusDirectiveRegistry\")]");
        builder.AppendLine("public sealed record ElementPlusDirectiveRegistry : VueDirectiveRegistry");
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

    private static string RenderVuePropAttribute(ElementPlusPropMetadata prop)
    {
        var arguments = new List<string>
        {
            $"nameof({prop.PropertyName})"
        };

        if (prop.AcceptsBinding)
        {
            arguments.Add("VuePropKind.Model");
        }

        var namedArguments = new List<string>
        {
            $"Name = \"{prop.RuntimeName}\""
        };

        if (prop.Required)
        {
            namedArguments.Add("Required = true");
        }

        if (prop.AcceptsBinding)
        {
            namedArguments.Add("AcceptsBinding = true");
        }

        return $"[VueProp({string.Join(", ", arguments.Concat(namedArguments))})]";
    }

    private static string RenderVueEmitAttribute(ElementPlusEmitMetadata emit)
    {
        if (emit.IsModelUpdate)
        {
            return $"[VueLibraryEmit(nameof({emit.PropertyName}), VueEmitKind.ModelUpdate, Name = \"{emit.RuntimeName}\", PayloadTypeName = \"{EscapeCSharpString(emit.PayloadTypeRuntimeName ?? emit.PayloadTypeSourceText ?? "void")}\")]";
        }

        return $"[VueLibraryEmit(nameof({emit.PropertyName}), Name = \"{emit.RuntimeName}\")]";
    }

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
                CreateSupplementalProp("onResize", "OnResize", GeneratedType.Reference("Delegate?"))
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
                    CreateSupplementalProp("tagTooltip", "TagTooltip", GeneratedType.Reference("ElementPlusTagTooltipProps?"))
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
            return GeneratedType.Reference("Delegate?");

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
                return GeneratedType.Value("ElementPlusComponentSize").AsOptional(prop.Required);

            if (HasExactStringLiteralSet(tokens, "dark", "light"))
                return GeneratedType.Value("ElementPlusPopperEffect").AsOptional(prop.Required);

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
            return GeneratedType.Reference("Delegate?").AsOptional(required);

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
            "InputAutoSize" => GeneratedType.Value("ElementPlusInputAutoSize").AsOptional(required),
            "TagTooltipProps" => GeneratedType.Reference("ElementPlusTagTooltipProps").AsOptional(required),
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
                "ElInfiniteScroll" => new ElementPlusDirectiveMetadata(exportName, "InfiniteScroll", "ElementPlusDirective"),
                "ElLoading" => new ElementPlusDirectiveMetadata("ElLoadingDirective", "Loading", "VueDirective<ElementPlusDirectiveValue>"),
                _ => new ElementPlusDirectiveMetadata(exportName, exportName, "ElementPlusDirective")
            };
        }
    }
}
