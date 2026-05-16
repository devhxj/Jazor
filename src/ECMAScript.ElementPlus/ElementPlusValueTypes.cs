using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ECMAScript.ElementPlus;

[String]
public enum ElementPlusComponentSize
{
    [Description("@#large")]
    Large,

    [Description("@#default")]
    Default,

    [Description("@#small")]
    Small
}

[String]
public enum ElementPlusPopperEffect
{
    [Description("@#dark")]
    Dark,

    [Description("@#light")]
    Light
}

[String]
public enum ElementPlusCardShadow
{
    [Description("@#always")]
    Always,

    [Description("@#hover")]
    Hover,

    [Description("@#never")]
    Never
}

[String]
public enum ElementPlusUploadStatus
{
    [Description("@#ready")]
    Ready,

    [Description("@#uploading")]
    Uploading,

    [Description("@#success")]
    Success,

    [Description("@#fail")]
    Fail
}

[String]
public enum ElementPlusHoverClickTrigger
{
    [Description("@#hover")]
    Hover,

    [Description("@#click")]
    Click
}

[String]
public enum ElementPlusCrossorigin
{
    [Description("@#")]
    Empty,

    [Description("@#anonymous")]
    Anonymous,

    [Description("@#use-credentials")]
    UseCredentials
}

[String]
public enum ElementPlusUploadListType
{
    [Description("@#text")]
    Text,

    [Description("@#picture")]
    Picture,

    [Description("@#picture-card")]
    PictureCard
}

[String]
public enum ElementPlusImageFitType
{
    [Description("@#")]
    Empty,

    [Description("@#contain")]
    Contain,

    [Description("@#cover")]
    Cover,

    [Description("@#fill")]
    Fill,

    [Description("@#none")]
    None,

    [Description("@#scale-down")]
    ScaleDown
}

[String]
public enum ElementPlusImageLoadingType
{
    [Description("@#eager")]
    Eager,

    [Description("@#lazy")]
    Lazy
}

[ECMAScript]
[Description("@#Styles")]
public sealed record ElementPlusStyles : VueDictionary<VueStringNumberValue>
{
}

[ECMAScript]
[Description("@#")]
public readonly union ElementPlusDirectiveValue(bool, VueProps)
{
    public bool? AsBool => Value is bool value ? value : default(bool?);

    public VueProps? AsProps => Value as VueProps;

    public static implicit operator ElementPlusDirectiveValue(VueDictionary value) => (VueProps)value;
}

[ECMAScript]
[Description("@#")]
public sealed record ElementPlusLoadingOptions : VueProps
{
    [Description("@#target")]
    public VueTeleportTarget? Target { get; init; }

    [Description("@#body")]
    public bool? Body { get; init; }

    [Description("@#lock")]
    public bool? Lock { get; init; }

    [Description("@#text")]
    public string? Text { get; init; }

    [Description("@#spinner")]
    public string? Spinner { get; init; }

    [Description("@#svg")]
    public string? Svg { get; init; }

    [Description("@#svgViewBox")]
    public string? SvgViewBox { get; init; }

    [Description("@#background")]
    public string? Background { get; init; }

    [Description("@#customClass")]
    public string? CustomClass { get; init; }

    [Description("@#fullscreen")]
    public bool? Fullscreen { get; init; }
}

[ECMAScript]
[Description("@#ButtonConfigContext")]
public sealed record ElementPlusButtonConfig : VueProps
{
    [Description("@#autoInsertSpace")]
    public bool? AutoInsertSpace { get; init; }

    [Description("@#type")]
    public string? Type { get; init; }

    [Description("@#plain")]
    public bool? Plain { get; init; }

    [Description("@#text")]
    public bool? Text { get; init; }

    [Description("@#round")]
    public bool? Round { get; init; }

    [Description("@#dashed")]
    public bool? Dashed { get; init; }
}

[ECMAScript]
[Description("@#CardConfigContext")]
public sealed record ElementPlusCardConfig : VueProps
{
    [Description("@#shadow")]
    public ElementPlusCardShadow? Shadow { get; init; }
}

[ECMAScript]
[Description("@#MentionOption")]
public sealed record ElementPlusMentionOption : VueDictionary
{
    [Description("@#value")]
    public string? Value { get; init; }

    [Description("@#label")]
    public string? Label { get; init; }

    [Description("@#disabled")]
    public bool? Disabled { get; init; }
}

[ECMAScript]
[Description("@#DialogConfigContext")]
public sealed record ElementPlusDialogConfig : VueProps
{
    [Description("@#alignCenter")]
    public bool? AlignCenter { get; init; }

    [Description("@#draggable")]
    public bool? Draggable { get; init; }

    [Description("@#overflow")]
    public bool? Overflow { get; init; }

    [Description("@#transition")]
    public VueTransitionValue? Transition { get; init; }
}

[ECMAScript]
[Description("@#LinkConfigContext")]
public sealed record ElementPlusLinkConfig : VueProps
{
    [Description("@#underline")]
    public VueBooleanStringValue? Underline { get; init; }

    [Description("@#type")]
    public string? Type { get; init; }
}

[ECMAScript]
[Description("@#MessageConfigContext")]
public sealed record ElementPlusMessageConfig : VueProps
{
    [Description("@#max")]
    public Number? Max { get; init; }

    [Description("@#grouping")]
    public bool? Grouping { get; init; }

    [Description("@#duration")]
    public Number? Duration { get; init; }

    [Description("@#offset")]
    public Number? Offset { get; init; }
}

[ECMAScript]
[Description("@#TableConfigContext")]
public sealed record ElementPlusTableConfig : VueProps
{
    [Description("@#showOverflowTooltip")]
    public ElementPlusTableOverflowTooltipValue? ShowOverflowTooltip { get; init; }

    [Description("@#tooltipEffect")]
    public string? TooltipEffect { get; init; }

    [Description("@#tooltipOptions")]
    public ElementPlusTableOverflowTooltipOptions? TooltipOptions { get; init; }

    [Description("@#tooltipFormatter")]
    public Delegate? TooltipFormatter { get; init; }
}

[ECMAScript]
[Description("@#TranslatePair")]
public sealed record ElementPlusTranslatePair : VueDictionary<ElementPlusTranslateValue>
{
}

[ECMAScript]
[Description("@#")]
public readonly union ElementPlusTranslateValue(string, string[], ElementPlusTranslatePair)
{
    public string? AsString => Value as string;

    public string[]? AsStrings => Value as string[];

    public ElementPlusTranslatePair? AsPair => Value as ElementPlusTranslatePair;
}

[ECMAScript]
[Description("@#Language")]
public sealed record ElementPlusLanguage : VueProps
{
    [Description("@#name")]
    public string? Name { get; init; }

    [Description("@#el")]
    public ElementPlusTranslatePair? El { get; init; }
}

[ECMAScript]
[Description("@#ValueOnClear")]
public readonly union ElementPlusValueOnClearValue(bool, double, string, Delegate)
{
    public bool? AsBool => Value is bool value ? value : default(bool?);

    public double? AsNumber => Value is double value ? value : default(double?);

    public string? AsString => Value as string;

    public Delegate? AsDelegate => Value as Delegate;

    [ECMAScriptInline("null")]
    public extern static ElementPlusValueOnClearValue Null();
}

[ECMAScript]
[Description("@#TableOverflowTooltipOptions")]
public sealed record ElementPlusTableOverflowTooltipOptions : VueProps
{
    [Description("@#appendTo")]
    public VueTeleportTarget? AppendTo { get; init; }

    [Description("@#effect")]
    public string? Effect { get; init; }

    [Description("@#enterable")]
    public bool? Enterable { get; init; }

    [Description("@#hideAfter")]
    public Number? HideAfter { get; init; }

    [Description("@#offset")]
    public Number? Offset { get; init; }

    [Description("@#placement")]
    public string? Placement { get; init; }

    [Description("@#popperClass")]
    public string? PopperClass { get; init; }

    [Description("@#popperOptions")]
    public VueDictionary? PopperOptions { get; init; }

    [Description("@#showAfter")]
    public Number? ShowAfter { get; init; }

    [Description("@#showArrow")]
    public bool? ShowArrow { get; init; }

    [Description("@#transition")]
    public string? Transition { get; init; }
}

[ECMAScript]
[Description("@#")]
public readonly union ElementPlusTableOverflowTooltipValue(bool, ElementPlusTableOverflowTooltipOptions)
{
    public bool? AsBool => Value is bool value ? value : default(bool?);

    public ElementPlusTableOverflowTooltipOptions? AsOptions => Value as ElementPlusTableOverflowTooltipOptions;
}

[ECMAScript]
[Description("@#InputAutoSizeOptions")]
public sealed record ElementPlusInputAutoSizeOptions : VueProps
{
    [Description("@#minRows")]
    public Number? MinRows { get; init; }

    [Description("@#maxRows")]
    public Number? MaxRows { get; init; }
}

[ECMAScript]
[Description("@#InputAutoSize")]
public readonly union ElementPlusInputAutoSize(bool, ElementPlusInputAutoSizeOptions)
{
    public bool? AsBool => Value is bool value ? value : default(bool?);

    public ElementPlusInputAutoSizeOptions? AsOptions => Value as ElementPlusInputAutoSizeOptions;
}

[ECMAScript]
[Description("@#ColSizeObject")]
public sealed record ElementPlusColSizeProps : VueProps
{
    [Description("@#span")]
    public Number? Span { get; init; }

    [Description("@#offset")]
    public Number? Offset { get; init; }

    [Description("@#pull")]
    public Number? Pull { get; init; }

    [Description("@#push")]
    public Number? Push { get; init; }
}

[ECMAScript]
[Description("@#")]
public readonly union ElementPlusColSizeValue(double, ElementPlusColSizeProps)
{
    public double? AsNumber => Value is double value ? value : default(double?);

    public ElementPlusColSizeProps? AsProps => Value as ElementPlusColSizeProps;
}

[ECMAScript]
[Description("@#")]
public readonly union ElementPlusSpaceSizeValue(ElementPlusComponentSize, Number, VueNumberPair)
{
    public ElementPlusComponentSize? AsComponentSize
        => Value is ElementPlusComponentSize value ? value : default(ElementPlusComponentSize?);

    public Number? AsNumber => Value is Number value ? value : default(Number?);

    public VueNumberPair? AsPair => Value is VueNumberPair value ? value : default(VueNumberPair?);

    public static implicit operator ElementPlusSpaceSizeValue(double value)
        => new((Number)value);
}

[ECMAScript]
[Description("@#ThrottleRender")]
public sealed record ElementPlusThrottleRenderOptions : VueProps
{
    [Description("@#leading")]
    public Number? Leading { get; init; }

    [Description("@#trailing")]
    public Number? Trailing { get; init; }

    [Description("@#initVal")]
    public bool? InitVal { get; init; }
}

[ECMAScript]
[Description("@#")]
public readonly union ElementPlusThrottleValue(Number, ElementPlusThrottleRenderOptions)
{
    public Number? AsNumber => Value is Number value ? value : default(Number?);

    public ElementPlusThrottleRenderOptions? AsOptions => Value as ElementPlusThrottleRenderOptions;

    public static implicit operator ElementPlusThrottleValue(double value)
        => new((Number)value);
}

[String]
public enum ElementPlusTableSortOrder
{
    [Description("@#ascending")]
    Ascending,

    [Description("@#descending")]
    Descending
}

[ECMAScript]
[Description("@#Sort")]
public sealed record ElementPlusTableSort : VueProps
{
    [Description("@#prop")]
    public string? Prop { get; init; }

    [Description("@#order")]
    public ElementPlusTableSortOrder? Order { get; init; }

    [Description("@#init")]
    public VueValue? Init { get; init; }

    [Description("@#silent")]
    public VueValue? Silent { get; init; }
}

[ECMAScript]
[Description("@#TreeProps")]
public sealed record ElementPlusTableTreeProps : VueProps
{
    [Description("@#hasChildren")]
    public string? HasChildren { get; init; }

    [Description("@#children")]
    public string? Children { get; init; }

    [Description("@#checkStrictly")]
    public bool? CheckStrictly { get; init; }
}

[ECMAScript]
[Description("@#Filter")]
public sealed record ElementPlusTableFilterItem : VueProps
{
    [Description("@#text")]
    public string? Text { get; init; }

    [Description("@#value")]
    public string? Value { get; init; }
}

[ECMAScript]
[Description("@#UploadRawFile")]
public sealed record ElementPlusUploadRawFile : VueProps
{
    [Description("@#uid")]
    public Number Uid { get; init; } = default!;

    [Description("@#isDirectory")]
    public bool? IsDirectory { get; init; }
}

[ECMAScript]
[Description("@#UploadUserFile")]
public sealed record ElementPlusUploadUserFile : VueProps
{
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    [Description("@#percentage")]
    public Number? Percentage { get; init; }

    [Description("@#status")]
    public ElementPlusUploadStatus? Status { get; init; }

    [Description("@#size")]
    public Number? Size { get; init; }

    [Description("@#response")]
    public VueValue? Response { get; init; }

    [Description("@#uid")]
    public Number? Uid { get; init; }

    [Description("@#url")]
    public string? Url { get; init; }

    [Description("@#raw")]
    public ElementPlusUploadRawFile? Raw { get; init; }
}

[String]
public enum ElementPlusTooltipTriggerType
{
    [Description("@#hover")]
    Hover,

    [Description("@#focus")]
    Focus,

    [Description("@#click")]
    Click,

    [Description("@#contextmenu")]
    Contextmenu
}

[String]
public enum ElementPlusDropdownTriggerType
{
    [Description("@#click")]
    Click,

    [Description("@#hover")]
    Hover,

    [Description("@#contextmenu")]
    Contextmenu
}

[ECMAScript]
[Description("@#")]
public readonly union ElementPlusDropdownTriggerValue(ElementPlusDropdownTriggerType, ElementPlusDropdownTriggerType[])
{
    public ElementPlusDropdownTriggerType? AsSingle
        => Value is ElementPlusDropdownTriggerType value ? value : default(ElementPlusDropdownTriggerType?);

    public ElementPlusDropdownTriggerType[]? AsMultiple => Value as ElementPlusDropdownTriggerType[];
}

[ECMAScript]
[Description("@#")]
public readonly union ElementPlusTooltipTriggerValue(ElementPlusTooltipTriggerType, ElementPlusTooltipTriggerType[])
{
    public ElementPlusTooltipTriggerType? AsSingle
        => Value is ElementPlusTooltipTriggerType value ? value : default(ElementPlusTooltipTriggerType?);

    public ElementPlusTooltipTriggerType[]? AsMultiple => Value as ElementPlusTooltipTriggerType[];
}

[ECMAScript]
[Description("@#TagTooltipProps")]
public sealed record ElementPlusTagTooltipProps : VueProps
{
    [Description("@#appendTo")]
    public VueTeleportTarget? AppendTo { get; init; }

    [Description("@#placement")]
    public string? Placement { get; init; }

    [Description("@#fallbackPlacements")]
    public string[]? FallbackPlacements { get; init; }

    [Description("@#effect")]
    public ElementPlusPopperEffect? Effect { get; init; }

    [Description("@#popperClass")]
    public string? PopperClass { get; init; }

    [Description("@#popperStyle")]
    public VueStyleValue? PopperStyle { get; init; }

    [Description("@#transition")]
    public string? Transition { get; init; }

    [Description("@#teleported")]
    public bool? Teleported { get; init; }

    [Description("@#popperOptions")]
    public VueDictionary? PopperOptions { get; init; }

    [Description("@#showAfter")]
    public Number? ShowAfter { get; init; }

    [Description("@#hideAfter")]
    public Number? HideAfter { get; init; }

    [Description("@#autoClose")]
    public Number? AutoClose { get; init; }

    [Description("@#offset")]
    public Number? Offset { get; init; }
}

[ECMAScript]
[Description("@#ButtonProps")]
public sealed record ElementPlusButtonProps : VueProps
{
    [Description("@#size")]
    public ElementPlusComponentSize? Size { get; init; }

    [Description("@#disabled")]
    public bool? Disabled { get; init; }

    [Description("@#type")]
    public string? Type { get; init; }

    [Description("@#icon")]
    public VueStringComponentValue? Icon { get; init; }

    [Description("@#nativeType")]
    public string? NativeType { get; init; }

    [Description("@#loading")]
    public bool? Loading { get; init; }

    [Description("@#loadingIcon")]
    public VueStringComponentValue? LoadingIcon { get; init; }

    [Description("@#plain")]
    public bool? Plain { get; init; }

    [Description("@#text")]
    public bool? Text { get; init; }

    [Description("@#link")]
    public bool? Link { get; init; }

    [Description("@#bg")]
    public bool? Bg { get; init; }

    [Description("@#autofocus")]
    public bool? Autofocus { get; init; }

    [Description("@#round")]
    public bool? Round { get; init; }

    [Description("@#circle")]
    public bool? Circle { get; init; }

    [Description("@#dashed")]
    public bool? Dashed { get; init; }

    [Description("@#color")]
    public string? Color { get; init; }

    [Description("@#dark")]
    public bool? Dark { get; init; }

    [Description("@#autoInsertSpace")]
    public bool? AutoInsertSpace { get; init; }

    [Description("@#tag")]
    public VueStringComponentValue? Tag { get; init; }
}

[ECMAScript]
[Description("@#TransferDataItem")]
public sealed record ElementPlusTransferDataItem : VueDictionary
{
}

[String]
public enum ElementPlusTransferTargetOrder
{
    [Description("@#original")]
    Original,

    [Description("@#push")]
    Push,

    [Description("@#unshift")]
    Unshift
}

[ECMAScript]
[Description("@#TransferFormat")]
public sealed record ElementPlusTransferFormat : VueProps
{
    [Description("@#noChecked")]
    public string? NoChecked { get; init; }

    [Description("@#hasChecked")]
    public string? HasChecked { get; init; }
}

[ECMAScript]
[Description("@#TransferPropsAlias")]
public sealed record ElementPlusTransferPropsAlias : VueProps
{
    [Description("@#label")]
    public string? Label { get; init; }

    [Description("@#key")]
    public string? Key { get; init; }

    [Description("@#disabled")]
    public string? Disabled { get; init; }
}

[ECMAScript]
[Description("@#")]
[CollectionBuilder(typeof(ElementPlusTransferTextPairCollectionBuilder), nameof(ElementPlusTransferTextPairCollectionBuilder.Create))]
public readonly union ElementPlusTransferTextPair(string[]) : IEnumerable<string>
{
    public string[]? AsValues => Value as string[];

    IEnumerator<string> IEnumerable<string>.GetEnumerator()
        => ((IEnumerable<string>)(AsValues ?? Array.Empty<string>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<string>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class ElementPlusTransferTextPairCollectionBuilder
{
    public static ElementPlusTransferTextPair Create(ReadOnlySpan<string> values)
    {
        if (values.Length != 2)
            throw new ArgumentException("Element Plus transfer text pairs require exactly two items.", nameof(values));

        return values.ToArray();
    }
}

[ECMAScript]
[Description("@#SelectPropsAlias")]
public sealed record ElementPlusSelectPropsAlias : VueProps
{
    [Description("@#label")]
    public string? Label { get; init; }

    [Description("@#value")]
    public string? Value { get; init; }

    [Description("@#disabled")]
    public string? Disabled { get; init; }

    [Description("@#options")]
    public string? Options { get; init; }
}

[ECMAScript]
[Description("@#CheckboxOptionProps")]
public sealed record ElementPlusCheckboxOptionPropsAlias : VueProps
{
    [Description("@#value")]
    public string? Value { get; init; }

    [Description("@#label")]
    public string? Label { get; init; }

    [Description("@#disabled")]
    public string? Disabled { get; init; }
}

[ECMAScript]
[Description("@#MentionOptionProps")]
public sealed record ElementPlusMentionOptionPropsAlias : VueProps
{
    [Description("@#value")]
    public string? Value { get; init; }

    [Description("@#label")]
    public string? Label { get; init; }

    [Description("@#disabled")]
    public string? Disabled { get; init; }
}

[ECMAScript]
[Description("@#radioOptionProp")]
public sealed record ElementPlusRadioOptionPropsAlias : VueProps
{
    [Description("@#value")]
    public string? Value { get; init; }

    [Description("@#label")]
    public string? Label { get; init; }

    [Description("@#disabled")]
    public string? Disabled { get; init; }
}

[ECMAScript]
[Description("@#SegmentedPropsAlias")]
public sealed record ElementPlusSegmentedPropsAlias : VueProps
{
    [Description("@#label")]
    public string? Label { get; init; }

    [Description("@#value")]
    public string? Value { get; init; }

    [Description("@#disabled")]
    public string? Disabled { get; init; }
}

[ECMAScript]
[Description("@#TreeOptionProps")]
public sealed record ElementPlusTreeOptionProps : VueProps
{
    [Description("@#children")]
    public string? Children { get; init; }

    [Description("@#label")]
    public VueValue? Label { get; init; }

    [Description("@#disabled")]
    public VueValue? Disabled { get; init; }

    [Description("@#isLeaf")]
    public VueValue? IsLeaf { get; init; }

    [Description("@#class")]
    public Delegate? CssClass { get; init; }
}

[ECMAScript]
[Description("@#CascaderProps")]
public sealed record ElementPlusCascaderProps : VueProps
{
    [Description("@#expandTrigger")]
    public string? ExpandTrigger { get; init; }

    [Description("@#multiple")]
    public bool? Multiple { get; init; }

    [Description("@#checkStrictly")]
    public bool? CheckStrictly { get; init; }

    [Description("@#emitPath")]
    public bool? EmitPath { get; init; }

    [Description("@#lazy")]
    public bool? Lazy { get; init; }

    [Description("@#lazyLoad")]
    public Delegate? LazyLoad { get; init; }

    [Description("@#value")]
    public string? Value { get; init; }

    [Description("@#label")]
    public string? Label { get; init; }

    [Description("@#children")]
    public string? Children { get; init; }

    [Description("@#disabled")]
    public VueValue? Disabled { get; init; }

    [Description("@#leaf")]
    public VueValue? Leaf { get; init; }

    [Description("@#hoverThreshold")]
    public Number? HoverThreshold { get; init; }

    [Description("@#checkOnClickNode")]
    public bool? CheckOnClickNode { get; init; }

    [Description("@#checkOnClickLeaf")]
    public bool? CheckOnClickLeaf { get; init; }

    [Description("@#showPrefix")]
    public bool? ShowPrefix { get; init; }
}
