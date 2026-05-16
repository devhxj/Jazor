using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
public enum ElementPlusPopperPlacement
{
    [Description("@#top")]
    Top,

    [Description("@#top-start")]
    TopStart,

    [Description("@#top-end")]
    TopEnd,

    [Description("@#bottom")]
    Bottom,

    [Description("@#bottom-start")]
    BottomStart,

    [Description("@#bottom-end")]
    BottomEnd,

    [Description("@#left")]
    Left,

    [Description("@#left-start")]
    LeftStart,

    [Description("@#left-end")]
    LeftEnd,

    [Description("@#right")]
    Right,

    [Description("@#right-start")]
    RightStart,

    [Description("@#right-end")]
    RightEnd,

    [Description("@#auto")]
    Auto,

    [Description("@#auto-start")]
    AutoStart,

    [Description("@#auto-end")]
    AutoEnd
}

[String]
public enum ElementPlusPopperPlacementSide
{
    [Description("@#top")]
    Top,

    [Description("@#bottom")]
    Bottom,

    [Description("@#left")]
    Left,

    [Description("@#right")]
    Right
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

[String]
public enum ElementPlusAvatarShape
{
    [Description("@#circle")]
    Circle,

    [Description("@#square")]
    Square
}

[String]
public enum ElementPlusButtonType
{
    [Description("@#")]
    Empty,

    [Description("@#default")]
    Default,

    [Description("@#primary")]
    Primary,

    [Description("@#success")]
    Success,

    [Description("@#warning")]
    Warning,

    [Description("@#info")]
    Info,

    [Description("@#danger")]
    Danger,

    [Description("@#text")]
    Text
}

[String]
public enum ElementPlusButtonNativeType
{
    [Description("@#button")]
    Button,

    [Description("@#submit")]
    Submit,

    [Description("@#reset")]
    Reset
}

[String]
public enum ElementPlusDirection
{
    [Description("@#horizontal")]
    Horizontal,

    [Description("@#vertical")]
    Vertical
}

[String]
public enum ElementPlusTopBottomPlacement
{
    [Description("@#top")]
    Top,

    [Description("@#bottom")]
    Bottom
}

[String]
public enum ElementPlusCarouselType
{
    [Description("@#")]
    Empty,

    [Description("@#card")]
    Card
}

[String]
public enum ElementPlusSemanticType
{
    [Description("@#")]
    Empty,

    [Description("@#primary")]
    Primary,

    [Description("@#success")]
    Success,

    [Description("@#warning")]
    Warning,

    [Description("@#info")]
    Info,

    [Description("@#danger")]
    Danger
}

[String]
public enum ElementPlusTimelineMode
{
    [Description("@#start")]
    Start,

    [Description("@#end")]
    End,

    [Description("@#alternate")]
    Alternate,

    [Description("@#alternate-reverse")]
    AlternateReverse
}

[String]
public enum ElementPlusCalendarControllerType
{
    [Description("@#button")]
    Button,

    [Description("@#select")]
    Select
}

[String]
public enum ElementPlusCollapseIconPosition
{
    [Description("@#left")]
    Left,

    [Description("@#right")]
    Right
}

[String]
public enum ElementPlusContentPosition
{
    [Description("@#left")]
    Left,

    [Description("@#center")]
    Center,

    [Description("@#right")]
    Right
}

[String]
public enum ElementPlusFormItemValidateStatus
{
    [Description("@#")]
    Empty,

    [Description("@#error")]
    Error,

    [Description("@#validating")]
    Validating,

    [Description("@#success")]
    Success
}

[String]
public enum ElementPlusProgressType
{
    [Description("@#line")]
    Line,

    [Description("@#circle")]
    Circle,

    [Description("@#dashboard")]
    Dashboard
}

[String]
public enum ElementPlusProgressStatus
{
    [Description("@#")]
    Empty,

    [Description("@#success")]
    Success,

    [Description("@#exception")]
    Exception,

    [Description("@#warning")]
    Warning
}

[String]
public enum ElementPlusStepStatus
{
    [Description("@#")]
    Empty,

    [Description("@#wait")]
    Wait,

    [Description("@#process")]
    Process,

    [Description("@#finish")]
    Finish,

    [Description("@#error")]
    Error,

    [Description("@#success")]
    Success
}

[String]
public enum ElementPlusTabsType
{
    [Description("@#")]
    Empty,

    [Description("@#card")]
    Card,

    [Description("@#border-card")]
    BorderCard
}

[String]
public enum ElementPlusTagType
{
    [Description("@#primary")]
    Primary,

    [Description("@#success")]
    Success,

    [Description("@#info")]
    Info,

    [Description("@#warning")]
    Warning,

    [Description("@#danger")]
    Danger
}

[String]
public enum ElementPlusTagEffect
{
    [Description("@#dark")]
    Dark,

    [Description("@#light")]
    Light,

    [Description("@#plain")]
    Plain
}

[String]
public enum ElementPlusLinkType
{
    [Description("@#default")]
    Default,

    [Description("@#primary")]
    Primary,

    [Description("@#success")]
    Success,

    [Description("@#warning")]
    Warning,

    [Description("@#info")]
    Info,

    [Description("@#danger")]
    Danger
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
    public ElementPlusButtonType? Type { get; init; }

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
    public ElementPlusLinkType? Type { get; init; }
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
    public ElementPlusTableTooltipFormatter? TooltipFormatter { get; init; }
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
public readonly union ElementPlusValueOnClearValue(bool, double, string, ElementPlusValueOnClearCallback)
{
    public bool? AsBool => Value is bool value ? value : default(bool?);

    public double? AsNumber => Value is double value ? value : default(double?);

    public string? AsString => Value as string;

    public ElementPlusValueOnClearCallback? AsCallback => Value as ElementPlusValueOnClearCallback;

    [ECMAScriptInline("null")]
    public extern static ElementPlusValueOnClearValue Null();
}

[ECMAScript]
[Description("@#")]
public sealed record ElementPlusStringBooleanMap : VueDictionary<bool>
{
}

[ECMAScript]
[Description("@#")]
public readonly union ElementPlusStringBooleanClassValue(string, ElementPlusStringBooleanMap)
{
    public string? AsString => Value as string;

    public ElementPlusStringBooleanMap? AsMap => Value as ElementPlusStringBooleanMap;
}

[ECMAScript]
[Description("@#")]
public sealed record ElementPlusAutocompleteSuggestionItem : VueDictionary
{
}

[ECMAScript]
[Description("@#")]
public sealed record ElementPlusAutoResizerResizeContext : VueProps
{
    [Description("@#height")]
    public Number? Height { get; init; }

    [Description("@#width")]
    public Number? Width { get; init; }
}

[ECMAScript]
[Description("@#")]
public delegate void ElementPlusAutoResizerResizeCallback(ElementPlusAutoResizerResizeContext context);

[ECMAScript]
[Description("@#")]
public delegate void ElementPlusAutocompleteFetchSuggestionsCallback(ElementPlusAutocompleteSuggestionItem[] data);

[ECMAScript]
[Description("@#")]
public delegate IPromise<ElementPlusAutocompleteSuggestionItem[]?> ElementPlusAutocompleteFetchSuggestionsAsyncCallback(
    string queryString,
    ElementPlusAutocompleteFetchSuggestionsCallback callback);

[ECMAScript]
[Description("@#")]
public delegate void ElementPlusAutocompleteFetchSuggestionsCallbackOnly(string queryString, ElementPlusAutocompleteFetchSuggestionsCallback callback);

[ECMAScript]
[Description("@#")]
public readonly union ElementPlusAutocompleteFetchSuggestionsValue(
    ElementPlusAutocompleteSuggestionItem[],
    ElementPlusAutocompleteFetchSuggestionsCallbackOnly,
    ElementPlusAutocompleteFetchSuggestionsAsyncCallback)
{
    public ElementPlusAutocompleteSuggestionItem[]? AsSuggestions => Value as ElementPlusAutocompleteSuggestionItem[];

    public ElementPlusAutocompleteFetchSuggestionsCallbackOnly? AsCallback => Value as ElementPlusAutocompleteFetchSuggestionsCallbackOnly;

    public ElementPlusAutocompleteFetchSuggestionsAsyncCallback? AsAsyncCallback => Value as ElementPlusAutocompleteFetchSuggestionsAsyncCallback;
}

[ECMAScript]
[Description("@#")]
public sealed record ElementPlusCalendarDateCellContext : VueProps
{
    [Description("@#date")]
    public Date? Date { get; init; }

    [Description("@#type")]
    public string? Type { get; init; }

    [Description("@#day")]
    public string? Day { get; init; }

    [Description("@#isSelected")]
    public bool? IsSelected { get; init; }
}

[ECMAScript]
[Description("@#")]
public delegate VueStringNumberValue ElementPlusCalendarFormatterCallback(Number value, string type);

[ECMAScript]
[Description("@#")]
public delegate bool ElementPlusAsyncBooleanCallback();

[ECMAScript]
[Description("@#")]
public delegate IPromise<bool?> ElementPlusAsyncBooleanPromiseCallback();

[ECMAScript]
[Description("@#")]
public readonly union ElementPlusAsyncBooleanResult(bool, IPromise<bool?>)
{
    public bool? AsBool => Value is bool value ? value : default(bool?);

    public IPromise<bool?>? AsPromise => Value as IPromise<bool?>;
}

[ECMAScript]
[Description("@#")]
public delegate void ElementPlusDialogDoneCallback(bool? cancel = null);

[ECMAScript]
[Description("@#")]
public delegate void ElementPlusDialogBeforeCloseCallback(ElementPlusDialogDoneCallback done);

[ECMAScript]
[Description("@#")]
public delegate string ElementPlusInputFormatter(string value);

[ECMAScript]
[Description("@#")]
public delegate string ElementPlusInputParser(string value);

[ECMAScript]
[Description("@#")]
public delegate Number ElementPlusInputCountGraphemes(string value);

[ECMAScript]
[Description("@#")]
public delegate bool ElementPlusInputOtpValidator(string @char, Number index);

[ECMAScript]
[Description("@#")]
public delegate VueStringNumberVNodeValue ElementPlusInputOtpSeparatorRenderer(Number index);

[ECMAScript]
[Description("@#")]
public readonly union ElementPlusInputOtpSeparatorValue(string, IVNode, ElementPlusInputOtpSeparatorRenderer)
{
    public string? AsString => Value as string;

    public IVNode? AsVNode => Value as IVNode;

    public ElementPlusInputOtpSeparatorRenderer? AsRenderer => Value as ElementPlusInputOtpSeparatorRenderer;
}

[ECMAScript]
[Description("@#")]
public delegate bool ElementPlusMentionFilterOption(string pattern, ElementPlusMentionOption option);

[ECMAScript]
[Description("@#")]
public readonly union ElementPlusMentionFilterOptionValue(bool, ElementPlusMentionFilterOption)
{
    public bool? AsBool => Value is bool value ? value : default(bool?);

    public ElementPlusMentionFilterOption? AsCallback => Value as ElementPlusMentionFilterOption;
}

[ECMAScript]
[Description("@#")]
public delegate bool ElementPlusMentionCheckIsWhole(string pattern, string prefix);

[ECMAScript]
[Description("@#ProgressColor")]
public sealed record ElementPlusProgressColorStop : VueProps
{
    [Description("@#color")]
    public string? Color { get; init; }

    [Description("@#percentage")]
    public Number? Percentage { get; init; }
}

[ECMAScript]
[Description("@#")]
public delegate string ElementPlusProgressColorCallback(Number percentage);

[ECMAScript]
[Description("@#")]
public readonly union ElementPlusProgressColorValue(
    string,
    ElementPlusProgressColorStop[],
    ElementPlusProgressColorCallback)
{
    public string? AsString => Value as string;

    public ElementPlusProgressColorStop[]? AsStops => Value as ElementPlusProgressColorStop[];

    public ElementPlusProgressColorCallback? AsCallback => Value as ElementPlusProgressColorCallback;
}

[ECMAScript]
[Description("@#")]
public delegate string ElementPlusProgressFormatCallback(Number percentage);

[ECMAScript]
[Description("@#")]
public delegate bool ElementPlusCascaderFilterMethod(VueDictionary node, string keyword);

[ECMAScript]
[Description("@#")]
public delegate IPromise<VueValue?> ElementPlusCascaderBeforeFilterAsyncCallback(string value);

[ECMAScript]
[Description("@#")]
public delegate bool ElementPlusCascaderBeforeFilterSyncCallback(string value);

[ECMAScript]
[Description("@#")]
public readonly union ElementPlusCascaderBeforeFilterResult(bool, IPromise<VueValue?>)
{
    public bool? AsBool => Value is bool value ? value : default(bool?);

    public IPromise<VueValue?>? AsPromise => Value as IPromise<VueValue?>;
}

[ECMAScript]
[Description("@#")]
public delegate ElementPlusCascaderBeforeFilterResult ElementPlusCascaderBeforeFilterCallback(string value);

[ECMAScript]
[Description("@#")]
public delegate IPromise<bool?> ElementPlusCollapseBeforeCollapseAsyncCallback(VueStringNumberValue name);

[ECMAScript]
[Description("@#")]
public delegate bool ElementPlusCollapseBeforeCollapseSyncCallback(VueStringNumberValue name);

[ECMAScript]
[Description("@#")]
public readonly union ElementPlusCollapseBeforeCollapseResult(bool, IPromise<bool?>)
{
    public bool? AsBool => Value is bool value ? value : default(bool?);

    public IPromise<bool?>? AsPromise => Value as IPromise<bool?>;
}

[ECMAScript]
[Description("@#")]
public delegate ElementPlusCollapseBeforeCollapseResult ElementPlusCollapseBeforeCollapseCallback(VueStringNumberValue name);

[ECMAScript]
[Description("@#")]
public delegate string ElementPlusDateLikeCellClassName(Date date);

[ECMAScript]
[Description("@#")]
public delegate bool ElementPlusDateLikeDisabledDate(Date date);

[ECMAScript]
[Description("@#")]
public delegate void ElementPlusValueOnClearCallback();

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

[ECMAScript]
[Description("@#UploadFile")]
public sealed record ElementPlusUploadFile : VueProps
{
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    [Description("@#percentage")]
    public Number? Percentage { get; init; }

    [Description("@#status")]
    public ElementPlusUploadStatus Status { get; init; } = default!;

    [Description("@#size")]
    public Number? Size { get; init; }

    [Description("@#response")]
    public VueValue? Response { get; init; }

    [Description("@#uid")]
    public Number Uid { get; init; } = default!;

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
    public ElementPlusPopperPlacement? Placement { get; init; }

    [Description("@#fallbackPlacements")]
    public ElementPlusPopperPlacement[]? FallbackPlacements { get; init; }

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
    public ElementPlusButtonType? Type { get; init; }

    [Description("@#icon")]
    public VueStringComponentValue? Icon { get; init; }

    [Description("@#nativeType")]
    public ElementPlusButtonNativeType? NativeType { get; init; }

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
    public ElementPlusTreeOptionClassCallback? CssClass { get; init; }
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
    public ElementPlusCascaderLazyLoadCallback? LazyLoad { get; init; }

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

[ECMAScript]
[Description("@#FormItemRule")]
public sealed record ElementPlusFormItemRule : VueDictionary
{
    [Description("@#trigger")]
    public VueStringOrStringsValue? Trigger { get; init; }
}

[ECMAScript]
[Description("@#")]
public readonly union ElementPlusFormItemRules(
    ElementPlusFormItemRule,
    ElementPlusFormItemRule[]) : IEnumerable<ElementPlusFormItemRule>
{
    public ElementPlusFormItemRule? AsSingle
        => Value as ElementPlusFormItemRule;

    public ElementPlusFormItemRule[]? AsMultiple => Value as ElementPlusFormItemRule[];

    IEnumerator<ElementPlusFormItemRule> IEnumerable<ElementPlusFormItemRule>.GetEnumerator()
        => ((IEnumerable<ElementPlusFormItemRule>)(AsMultiple ?? Array.Empty<ElementPlusFormItemRule>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<ElementPlusFormItemRule>)this).GetEnumerator();
}

[ECMAScript]
[Description("@#")]
public readonly union ElementPlusFormRuleValue(ElementPlusFormItemRules, ElementPlusFormRules)
{
    public ElementPlusFormItemRules? AsItemRules
        => Value is ElementPlusFormItemRules value ? value : default(ElementPlusFormItemRules?);

    public ElementPlusFormRules? AsNestedRules => Value as ElementPlusFormRules;
}

[ECMAScript]
[Description("@#FormRules")]
public sealed record ElementPlusFormRules : VueDictionary<ElementPlusFormRuleValue>
{
}

[ECMAScript]
[Description("@#RateColorMap")]
public sealed record ElementPlusRateColorMap : VueDictionary<string>
{
}

[ECMAScript]
[Description("@#")]
public readonly union ElementPlusRateColorsValue(
    string[],
    ElementPlusRateColorMap) : IEnumerable<string>
{
    public string[]? AsArray => Value as string[];

    public ElementPlusRateColorMap? AsMap => Value as ElementPlusRateColorMap;

    IEnumerator<string> IEnumerable<string>.GetEnumerator()
        => ((IEnumerable<string>)(AsArray ?? Array.Empty<string>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<string>)this).GetEnumerator();
}

[ECMAScript]
[Description("@#RateIconMap")]
public sealed record ElementPlusRateIconMap : VueDictionary<VueStringComponentValue>
{
}

[ECMAScript]
[Description("@#")]
public readonly union ElementPlusRateIconsValue(
    VueStringComponentValue[],
    ElementPlusRateIconMap) : IEnumerable<VueStringComponentValue>
{
    public VueStringComponentValue[]? AsArray => Value as VueStringComponentValue[];

    public ElementPlusRateIconMap? AsMap => Value as ElementPlusRateIconMap;

    public static implicit operator ElementPlusRateIconsValue(string[] values)
        => new(Array.ConvertAll(values, static value => (VueStringComponentValue)value));

    public static implicit operator ElementPlusRateIconsValue(IVueComponent[] values)
        => new(Array.ConvertAll(values, static value => (VueStringComponentValue)value));

    IEnumerator<VueStringComponentValue> IEnumerable<VueStringComponentValue>.GetEnumerator()
        => ((IEnumerable<VueStringComponentValue>)(AsArray ?? Array.Empty<VueStringComponentValue>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VueStringComponentValue>)this).GetEnumerator();
}

[ECMAScript]
[Description("@#SliderMarker")]
public sealed record ElementPlusSliderMarker : VueProps
{
    [Description("@#style")]
    public VueStyleValue? Style { get; init; }

    [Description("@#label")]
    public VueValue? Label { get; init; }
}

[ECMAScript]
[Description("@#")]
public readonly union ElementPlusSliderMarkValue(string, ElementPlusSliderMarker)
{
    public string? AsString => Value as string;

    public ElementPlusSliderMarker? AsMarker => Value as ElementPlusSliderMarker;
}

[ECMAScript]
[Description("@#SliderMarks")]
public sealed record ElementPlusSliderMarks : VueDictionary<ElementPlusSliderMarkValue>
{
}

[String]
public enum ElementPlusTableV2SortOrder
{
    [Description("@#asc")]
    Asc,

    [Description("@#desc")]
    Desc
}

[String]
public enum ElementPlusTableV2Alignment
{
    [Description("@#left")]
    Left,

    [Description("@#center")]
    Center,

    [Description("@#right")]
    Right
}

[String]
public enum ElementPlusTableV2FixedDirection
{
    [Description("@#left")]
    Left,

    [Description("@#right")]
    Right
}

[ECMAScript]
[Description("@#TableV2ClassContext")]
public sealed record ElementPlusTableV2ClassContext : VueProps
{
    [Description("@#columns")]
    public ElementPlusTableV2Column[]? Columns { get; init; }

    [Description("@#column")]
    public ElementPlusTableV2Column? Column { get; init; }

    [Description("@#columnIndex")]
    public Number? ColumnIndex { get; init; }

    [Description("@#headerIndex")]
    public Number? HeaderIndex { get; init; }

    [Description("@#cellData")]
    public VueValue? CellData { get; init; }

    [Description("@#rowData")]
    public VueValue? RowData { get; init; }

    [Description("@#rowIndex")]
    public Number? RowIndex { get; init; }
}

[ECMAScript]
[Description("@#ClassGetter")]
public delegate string ElementPlusTableV2ClassGetter(ElementPlusTableV2ClassContext context);

[ECMAScript]
[Description("@#")]
public readonly union ElementPlusTableV2ClassValue(string, ElementPlusTableV2ClassGetter)
{
    public string? AsString => Value as string;

    public ElementPlusTableV2ClassGetter? AsGetter => Value as ElementPlusTableV2ClassGetter;
}

[ECMAScript]
[Description("@#TableV2DynamicPropsContext")]
public sealed record ElementPlusTableV2DynamicPropsContext : VueProps
{
    [Description("@#columns")]
    public ElementPlusTableV2Column[]? Columns { get; init; }

    [Description("@#column")]
    public ElementPlusTableV2Column? Column { get; init; }

    [Description("@#columnIndex")]
    public Number? ColumnIndex { get; init; }

    [Description("@#headerIndex")]
    public Number? HeaderIndex { get; init; }

    [Description("@#cellData")]
    public VueValue? CellData { get; init; }

    [Description("@#rowData")]
    public VueValue? RowData { get; init; }

    [Description("@#rowIndex")]
    public Number? RowIndex { get; init; }
}

[ECMAScript]
[Description("@#DynamicPropsGetter")]
public delegate VueDictionary ElementPlusTableV2DynamicPropsGetter(ElementPlusTableV2DynamicPropsContext context);

[ECMAScript]
[Description("@#")]
public readonly union ElementPlusTableV2DynamicPropsValue(VueDictionary, ElementPlusTableV2DynamicPropsGetter)
{
    public VueDictionary? AsObject => Value as VueDictionary;

    public ElementPlusTableV2DynamicPropsGetter? AsGetter => Value as ElementPlusTableV2DynamicPropsGetter;
}

[ECMAScript]
[Description("@#")]
public readonly union ElementPlusTableV2KeyValue(VueKey)
{
    public VueKey? AsKey => Value as VueKey;

    public static implicit operator ElementPlusTableV2KeyValue(string value)
        => new((VueKey)value);

    public static implicit operator ElementPlusTableV2KeyValue(Symbol value)
        => new((VueKey)value);

    public static implicit operator ElementPlusTableV2KeyValue(Number value)
        => new((VueKey)value);

    public static implicit operator ElementPlusTableV2KeyValue(byte value)
        => new((VueKey)value);

    public static implicit operator ElementPlusTableV2KeyValue(sbyte value)
        => new((VueKey)value);

    public static implicit operator ElementPlusTableV2KeyValue(short value)
        => new((VueKey)value);

    public static implicit operator ElementPlusTableV2KeyValue(ushort value)
        => new((VueKey)value);

    public static implicit operator ElementPlusTableV2KeyValue(int value)
        => new((VueKey)value);

    public static implicit operator ElementPlusTableV2KeyValue(uint value)
        => new((VueKey)value);

    public static implicit operator ElementPlusTableV2KeyValue(long value)
        => new((VueKey)value);

    public static implicit operator ElementPlusTableV2KeyValue(ulong value)
        => new((VueKey)value);

    public static implicit operator ElementPlusTableV2KeyValue(float value)
        => new((VueKey)value);

    public static implicit operator ElementPlusTableV2KeyValue(double value)
        => new((VueKey)value);

    public static implicit operator ElementPlusTableV2KeyValue(decimal value)
        => new((VueKey)value);
}

[ECMAScript]
[Description("@#SortBy")]
public sealed record ElementPlusTableV2SortBy : VueProps
{
    [Description("@#key")]
    public ElementPlusTableV2KeyValue? Key { get; init; }

    [Description("@#order")]
    public ElementPlusTableV2SortOrder? Order { get; init; }
}

[ECMAScript]
[Description("@#SortState")]
public sealed record ElementPlusTableV2SortState : VueDictionary<ElementPlusTableV2SortOrder>
{
}

[ECMAScript]
[Description("@#Column")]
public sealed record ElementPlusTableV2Column : VueDictionary
{
    [Description("@#align")]
    public ElementPlusTableV2Alignment? Align { get; init; }

    [Description("@#class")]
    public ElementPlusTableV2ClassValue? CssClass { get; init; }

    [Description("@#key")]
    public ElementPlusTableV2KeyValue? Key { get; init; }

    [Description("@#dataKey")]
    public ElementPlusTableV2KeyValue? DataKey { get; init; }

    [Description("@#fixed")]
    public ElementPlusTableV2FixedValue? Fixed { get; init; }

    [Description("@#title")]
    public string? Title { get; init; }

    [Description("@#hidden")]
    public bool? Hidden { get; init; }

    [Description("@#headerClass")]
    public ElementPlusTableV2ClassValue? HeaderClass { get; init; }

    [Description("@#maxWidth")]
    public Number? MaxWidth { get; init; }

    [Description("@#minWidth")]
    public Number? MinWidth { get; init; }

    [Description("@#style")]
    public VueStyleValue? Style { get; init; }

    [Description("@#sortable")]
    public bool? Sortable { get; init; }

    [Description("@#width")]
    public Number? Width { get; init; }

    [Description("@#flexGrow")]
    public Number? FlexGrow { get; init; }

    [Description("@#flexShrink")]
    public Number? FlexShrink { get; init; }

    [Description("@#cellRenderer")]
    public ElementPlusTableV2CellRenderer? CellRenderer { get; init; }

    [Description("@#headerCellRenderer")]
    public ElementPlusTableV2HeaderCellRenderer? HeaderCellRenderer { get; init; }
}

[ECMAScript]
[Description("@#")]
public readonly union ElementPlusTableV2FixedValue(bool, ElementPlusTableV2FixedDirection)
{
    public bool? AsBool => Value is bool value ? value : default(bool?);

    public ElementPlusTableV2FixedDirection? AsDirection
        => Value is ElementPlusTableV2FixedDirection value ? value : default(ElementPlusTableV2FixedDirection?);
}

[ECMAScript]
[Description("@#TableData")]
public sealed record ElementPlusTableV2DataItem : VueDictionary
{
}

[ECMAScript]
[Description("@#")]
public readonly union ElementPlusTableV2HeaderHeightValue(double, Number[]) : IEnumerable<Number>
{
    public double? AsNumber => Value is double value ? value : default(double?);

    public Number[]? AsNumbers => Value is Number[] values ? values : default(Number[]?);

    public static implicit operator ElementPlusTableV2HeaderHeightValue(double[] values)
        => new(Array.ConvertAll(values, static value => (Number)value));

    IEnumerator<Number> IEnumerable<Number>.GetEnumerator()
        => ((IEnumerable<Number>)(AsNumbers ?? Array.Empty<Number>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<Number>)this).GetEnumerator();
}

[ECMAScript]
[Description("@#TableV2DataGetterContext")]
public sealed record ElementPlusTableV2DataGetterContext : VueProps
{
    [Description("@#columns")]
    public ElementPlusTableV2Column[]? Columns { get; init; }

    [Description("@#column")]
    public ElementPlusTableV2Column? Column { get; init; }

    [Description("@#columnIndex")]
    public Number? ColumnIndex { get; init; }

    [Description("@#rowData")]
    public VueValue? RowData { get; init; }

    [Description("@#rowIndex")]
    public Number? RowIndex { get; init; }
}

[ECMAScript]
[Description("@#DataGetter")]
public delegate VueValue ElementPlusTableV2DataGetter(ElementPlusTableV2DataGetterContext context);

[ECMAScript]
[Description("@#TableV2CellRendererContext")]
public sealed record ElementPlusTableV2CellRendererContext : VueProps
{
    [Description("@#columns")]
    public ElementPlusTableV2Column[]? Columns { get; init; }

    [Description("@#column")]
    public ElementPlusTableV2Column? Column { get; init; }

    [Description("@#columnIndex")]
    public Number? ColumnIndex { get; init; }

    [Description("@#cellData")]
    public VueValue? CellData { get; init; }

    [Description("@#rowData")]
    public VueValue? RowData { get; init; }

    [Description("@#rowIndex")]
    public Number? RowIndex { get; init; }
}

[ECMAScript]
[Description("@#TableV2HeaderCellRendererContext")]
public sealed record ElementPlusTableV2HeaderCellRendererContext : VueProps
{
    [Description("@#columns")]
    public ElementPlusTableV2Column[]? Columns { get; init; }

    [Description("@#column")]
    public ElementPlusTableV2Column? Column { get; init; }

    [Description("@#columnIndex")]
    public Number? ColumnIndex { get; init; }

    [Description("@#headerIndex")]
    public Number? HeaderIndex { get; init; }
}

[ECMAScript]
[Description("@#CellRenderer")]
public delegate IVNode ElementPlusTableV2CellRenderer(ElementPlusTableV2CellRendererContext context);

[ECMAScript]
[Description("@#HeaderCellRenderer")]
public delegate IVNode ElementPlusTableV2HeaderCellRenderer(ElementPlusTableV2HeaderCellRendererContext context);

[ECMAScript]
[Description("@#TableV2RowEventHandlerContext")]
public sealed record ElementPlusTableV2RowEventHandlerContext : VueProps
{
    [Description("@#rowKey")]
    public ElementPlusTableV2KeyValue? RowKey { get; init; }

    [Description("@#event")]
    public Event? Event { get; init; }

    [Description("@#rowData")]
    public VueValue? RowData { get; init; }

    [Description("@#rowIndex")]
    public Number? RowIndex { get; init; }
}

[ECMAScript]
[Description("@#RowEventHandler")]
public delegate void ElementPlusTableV2RowEventHandler(ElementPlusTableV2RowEventHandlerContext context);

[ECMAScript]
[Description("@#RowEventHandlers")]
public sealed record ElementPlusTableV2RowEventHandlers : VueProps
{
    [Description("@#onClick")]
    public ElementPlusTableV2RowEventHandler? OnClick { get; init; }

    [Description("@#onContextmenu")]
    public ElementPlusTableV2RowEventHandler? OnContextmenu { get; init; }

    [Description("@#onDblclick")]
    public ElementPlusTableV2RowEventHandler? OnDblclick { get; init; }

    [Description("@#onMouseenter")]
    public ElementPlusTableV2RowEventHandler? OnMouseenter { get; init; }

    [Description("@#onMouseleave")]
    public ElementPlusTableV2RowEventHandler? OnMouseleave { get; init; }
}

[ECMAScript]
[Description("@#")]
public readonly union ElementPlusTransferRenderContentResult(IVNode, IVNode[])
{
    public IVNode? AsSingle => Value as IVNode;

    public IVNode[]? AsMultiple => Value as IVNode[];
}

[ECMAScript]
[Description("@#")]
public delegate bool ElementPlusTransferFilterMethod(string query, ElementPlusTransferDataItem item);

[ECMAScript]
[Description("@#renderContent")]
public delegate ElementPlusTransferRenderContentResult ElementPlusTransferRenderContent(VueRenderHost h, ElementPlusTransferDataItem option);

[ECMAScript]
[Description("@#")]
public sealed record ElementPlusTreeNode : VueDictionary
{
}

[ECMAScript]
[Description("@#")]
public sealed record ElementPlusTreeNodeData : VueDictionary
{
}

[ECMAScript]
[Description("@#")]
public sealed record ElementPlusTreeRenderContentContext : VueProps
{
    [Description("@#node")]
    public ElementPlusTreeNode? Node { get; init; }

    [Description("@#data")]
    public ElementPlusTreeNodeData? Data { get; init; }

    [Description("@#store")]
    public VueDictionary? Store { get; init; }

    [Description("@#_self")]
    public VueValue? Self { get; init; }
}

[ECMAScript]
[Description("@#")]
public sealed record ElementPlusTreeDropIndicator : VueProps
{
    [Description("@#type")]
    public string? Type { get; init; }
}

[ECMAScript]
[Description("@#")]
public delegate ElementPlusStringBooleanClassValue ElementPlusTreeOptionClassCallback(ElementPlusTreeNodeData data, ElementPlusTreeNode node);

[ECMAScript]
[Description("@#")]
public delegate void ElementPlusCascaderLazyResolveCallback(VueDictionary[]? dataList = null);

[ECMAScript]
[Description("@#")]
public delegate void ElementPlusCascaderLazyRejectCallback();

[ECMAScript]
[Description("@#")]
public delegate void ElementPlusCascaderLazyLoadCallback(
    VueDictionary node,
    ElementPlusCascaderLazyResolveCallback resolve,
    ElementPlusCascaderLazyRejectCallback reject);

[ECMAScript]
[Description("@#")]
public delegate ElementPlusTransferRenderContentResult ElementPlusTreeRenderContentCallback(VueRenderHost h, ElementPlusTreeRenderContentContext context);

[ECMAScript]
[Description("@#")]
public delegate void ElementPlusTreeResolveChildrenCallback(ElementPlusTreeNodeData[] data);

[ECMAScript]
[Description("@#")]
public delegate void ElementPlusTreeStopLoadingCallback();

[ECMAScript]
[Description("@#")]
public delegate void ElementPlusTreeLoadCallback(
    ElementPlusTreeNode rootNode,
    ElementPlusTreeResolveChildrenCallback loadedCallback,
    ElementPlusTreeStopLoadingCallback stopLoading);

[ECMAScript]
[Description("@#")]
public delegate bool ElementPlusTreeFilterNodeMethod(VueValue? value, ElementPlusTreeNodeData data, ElementPlusTreeNode child);

[ECMAScript]
[Description("@#")]
public delegate bool ElementPlusTreeAllowDragCallback(ElementPlusTreeNode node);

[ECMAScript]
[Description("@#")]
public delegate bool ElementPlusTreeAllowDropCallback(
    ElementPlusTreeNode draggingNode,
    ElementPlusTreeNode dropNode,
    string type);

[ECMAScript]
[Description("@#")]
public delegate bool ElementPlusTreeV2FilterMethod(string query, ElementPlusTreeNodeData data, ElementPlusTreeNode node);

[ECMAScript]
[Description("@#")]
public delegate void ElementPlusSelectQueryCallback(string query);

[ECMAScript]
[Description("@#")]
public delegate VueStringNumberValue ElementPlusSliderFormatTooltipCallback(Number value);

[ECMAScript]
[Description("@#")]
public delegate string ElementPlusSliderFormatValueTextCallback(Number value);

[ECMAScript]
[Description("@#")]
public delegate ElementPlusAsyncBooleanResult ElementPlusSwitchBeforeChangeCallback();

[ECMAScript]
[Description("@#")]
public delegate IPromise<bool?> ElementPlusTabsBeforeLeaveAsyncCallback(VueStringNumberValue? newName, VueStringNumberValue? oldName);

[ECMAScript]
[Description("@#")]
public delegate bool? ElementPlusTabsBeforeLeaveSyncCallback(VueStringNumberValue? newName, VueStringNumberValue? oldName);

[ECMAScript]
[Description("@#")]
public readonly union ElementPlusTabsBeforeLeaveResult(bool, IPromise<bool?>)
{
    public bool? AsBool => Value is bool value ? value : default(bool?);

    public IPromise<bool?>? AsPromise => Value as IPromise<bool?>;
}

[ECMAScript]
[Description("@#")]
public delegate ElementPlusTabsBeforeLeaveResult? ElementPlusTabsBeforeLeaveCallback(
    VueStringNumberValue? newName,
    VueStringNumberValue? oldName);

[ECMAScript]
[Description("@#UploadProgressEvent")]
public sealed record ElementPlusUploadProgressEvent : VueProps
{
    [Description("@#percent")]
    public Number? Percent { get; init; }
}

[ECMAScript]
[Description("@#UploadError")]
public sealed record ElementPlusUploadAjaxError : VueProps
{
    [Description("@#name")]
    public string? Name { get; init; }

    [Description("@#message")]
    public string? Message { get; init; }
}

[ECMAScript]
[Description("@#UploadRequestData")]
public sealed record ElementPlusUploadRequestData : VueDictionary<VueValue>
{
}

[ECMAScript]
[Description("@#UploadRequestHeaders")]
public readonly union ElementPlusUploadRequestHeaders(Headers, VueDictionary)
{
    public Headers? AsHeaders => Value as Headers;

    public VueDictionary? AsDictionary => Value as VueDictionary;
}

[ECMAScript]
[Description("@#UploadRequestOptions")]
public sealed record ElementPlusUploadRequestOptions : VueProps
{
    [Description("@#action")]
    public string Action { get; init; } = string.Empty;

    [Description("@#method")]
    public string Method { get; init; } = string.Empty;

    [Description("@#data")]
    public ElementPlusUploadRequestData Data { get; init; } = new();

    [Description("@#filename")]
    public string Filename { get; init; } = string.Empty;

    [Description("@#file")]
    public ElementPlusUploadRawFile File { get; init; } = default!;

    [Description("@#headers")]
    public ElementPlusUploadRequestHeaders? Headers { get; init; }

    [Description("@#onError")]
    public ElementPlusUploadRequestOnErrorCallback? OnError { get; init; }

    [Description("@#onProgress")]
    public ElementPlusUploadRequestOnProgressCallback? OnProgress { get; init; }

    [Description("@#onSuccess")]
    public ElementPlusUploadRequestOnSuccessCallback? OnSuccess { get; init; }

    [Description("@#withCredentials")]
    public bool? WithCredentials { get; init; }
}

[ECMAScript]
[Description("@#")]
public delegate void ElementPlusUploadRequestOnErrorCallback(ElementPlusUploadAjaxError error);

[ECMAScript]
[Description("@#")]
public delegate void ElementPlusUploadRequestOnProgressCallback(ElementPlusUploadProgressEvent @event);

[ECMAScript]
[Description("@#")]
public delegate void ElementPlusUploadRequestOnSuccessCallback(VueValue? response);

[ECMAScript]
[Description("@#")]
public readonly union ElementPlusUploadRequestResult(XMLHttpRequest, IPromise<VueValue?>)
{
    public XMLHttpRequest? AsRequest => Value as XMLHttpRequest;

    public IPromise<VueValue?>? AsPromise => Value as IPromise<VueValue?>;
}

[ECMAScript]
[Description("@#")]
public delegate ElementPlusUploadRequestResult ElementPlusUploadRequestCallback(ElementPlusUploadRequestOptions options);

[ECMAScript]
[Description("@#UploadData")]
public sealed record ElementPlusUploadData : VueDictionary<VueValue>
{
}

[ECMAScript]
[Description("@#")]
public delegate IPromise<ElementPlusUploadData> ElementPlusUploadDataPromiseFactory(ElementPlusUploadRawFile rawFile);

[ECMAScript]
[Description("@#")]
public delegate ElementPlusUploadData ElementPlusUploadDataFactory(ElementPlusUploadRawFile rawFile);

[ECMAScript]
[Description("@#")]
public readonly union ElementPlusUploadDataValue(
    ElementPlusUploadData,
    IPromise<ElementPlusUploadData>,
    ElementPlusUploadDataFactory,
    ElementPlusUploadDataPromiseFactory)
{
    public ElementPlusUploadData? AsData => Value as ElementPlusUploadData;

    public IPromise<ElementPlusUploadData>? AsPromise => Value as IPromise<ElementPlusUploadData>;

    public ElementPlusUploadDataFactory? AsFactory => Value as ElementPlusUploadDataFactory;

    public ElementPlusUploadDataPromiseFactory? AsAsyncFactory => Value as ElementPlusUploadDataPromiseFactory;
}

[ECMAScript]
[Description("@#")]
public readonly union ElementPlusUploadBeforeUploadResult(bool, File, Blob, IPromise<VueValue?>)
{
    public bool? AsBool => Value is bool value ? value : default(bool?);

    public File? AsFile => Value as File;

    public Blob? AsBlob => Value as Blob;

    public IPromise<VueValue?>? AsPromise => Value as IPromise<VueValue?>;
}

[ECMAScript]
[Description("@#")]
public delegate ElementPlusUploadBeforeUploadResult? ElementPlusUploadBeforeUploadCallback(ElementPlusUploadRawFile rawFile);

[ECMAScript]
[Description("@#")]
public delegate bool ElementPlusUploadBeforeRemoveSyncCallback(ElementPlusUploadFile uploadFile, ElementPlusUploadFile[] uploadFiles);

[ECMAScript]
[Description("@#")]
public delegate IPromise<bool?> ElementPlusUploadBeforeRemoveAsyncCallback(ElementPlusUploadFile uploadFile, ElementPlusUploadFile[] uploadFiles);

[ECMAScript]
[Description("@#")]
public readonly union ElementPlusUploadBeforeRemoveResult(bool, IPromise<bool?>)
{
    public bool? AsBool => Value is bool value ? value : default(bool?);

    public IPromise<bool?>? AsPromise => Value as IPromise<bool?>;
}

[ECMAScript]
[Description("@#")]
public delegate ElementPlusUploadBeforeRemoveResult ElementPlusUploadBeforeRemoveCallback(ElementPlusUploadFile uploadFile, ElementPlusUploadFile[] uploadFiles);

[ECMAScript]
[Description("@#")]
public delegate void ElementPlusUploadPreviewCallback(ElementPlusUploadFile uploadFile);

[ECMAScript]
[Description("@#")]
public delegate void ElementPlusUploadFileListCallback(ElementPlusUploadFile uploadFile, ElementPlusUploadFile[] uploadFiles);

[ECMAScript]
[Description("@#")]
public delegate void ElementPlusUploadSuccessCallback(VueValue? response, ElementPlusUploadFile uploadFile, ElementPlusUploadFile[] uploadFiles);

[ECMAScript]
[Description("@#")]
public delegate void ElementPlusUploadProgressCallback(ElementPlusUploadProgressEvent @event, ElementPlusUploadFile uploadFile, ElementPlusUploadFile[] uploadFiles);

[ECMAScript]
[Description("@#")]
public delegate void ElementPlusUploadErrorCallback(Error error, ElementPlusUploadFile uploadFile, ElementPlusUploadFile[] uploadFiles);

[ECMAScript]
[Description("@#")]
public delegate void ElementPlusUploadExceedCallback(File[] files, ElementPlusUploadUserFile[] uploadFiles);

[ECMAScript]
[Description("@#")]
public delegate Number[] ElementPlusTimePickerDisabledHoursCallback(string role, Dayjs? comparingDate = null);

[ECMAScript]
[Description("@#")]
public delegate Number[] ElementPlusTimePickerDisabledMinutesCallback(Number hour, string role, Dayjs? comparingDate = null);

[ECMAScript]
[Description("@#")]
public delegate Number[] ElementPlusTimePickerDisabledSecondsCallback(Number hour, Number minute, string role, Dayjs? comparingDate = null);

[ECMAScript]
[Description("@#TableOverflowTooltipData")]
public sealed record ElementPlusTableTooltipFormatterContext : VueProps
{
    [Description("@#row")]
    public VueDictionary? Row { get; init; }

    [Description("@#column")]
    public ElementPlusTableColumnContext? Column { get; init; }

    [Description("@#cellValue")]
    public VueValue? CellValue { get; init; }
}

[ECMAScript]
[Description("@#")]
public delegate VueStringNumberVNodeValue ElementPlusTableTooltipFormatter(ElementPlusTableTooltipFormatterContext data);

[ECMAScript]
[Description("@#TableColumnCtx")]
public sealed record ElementPlusTableColumnContext : VueProps
{
    [Description("@#id")]
    public string? Id { get; init; }

    [Description("@#label")]
    public string? Label { get; init; }

    [Description("@#property")]
    public string? Property { get; init; }

    [Description("@#prop")]
    public string? Prop { get; init; }

    [Description("@#columnKey")]
    public string? ColumnKey { get; init; }

    [Description("@#type")]
    public string? Type { get; init; }
}

[ECMAScript]
[Description("@#TableRowContext")]
public sealed record ElementPlusTableRowContext : VueProps
{
    [Description("@#row")]
    public VueDictionary? Row { get; init; }

    [Description("@#rowIndex")]
    public Number? RowIndex { get; init; }
}

[ECMAScript]
[Description("@#TableCellContext")]
public sealed record ElementPlusTableCellContext : VueProps
{
    [Description("@#row")]
    public VueDictionary? Row { get; init; }

    [Description("@#rowIndex")]
    public Number? RowIndex { get; init; }

    [Description("@#column")]
    public ElementPlusTableColumnContext? Column { get; init; }

    [Description("@#columnIndex")]
    public Number? ColumnIndex { get; init; }
}

[ECMAScript]
[Description("@#SummaryMethodContext")]
public sealed record ElementPlusTableSummaryMethodContext : VueProps
{
    [Description("@#columns")]
    public ElementPlusTableColumnContext[]? Columns { get; init; }

    [Description("@#data")]
    public VueDictionary[]? Data { get; init; }
}

[ECMAScript]
[Description("@#")]
public readonly union ElementPlusTableSpanMethodResult(Number[], ElementPlusTableSpanMethodCoordinates)
{
    public Number[]? AsPair => Value as Number[];

    public ElementPlusTableSpanMethodCoordinates? AsCoordinates => Value as ElementPlusTableSpanMethodCoordinates;
}

[ECMAScript]
[Description("@#")]
public sealed record ElementPlusTableSpanMethodCoordinates : VueProps
{
    [Description("@#rowspan")]
    public Number? Rowspan { get; init; }

    [Description("@#colspan")]
    public Number? Colspan { get; init; }
}

[ECMAScript]
[Description("@#")]
public delegate string ElementPlusTableRowClassNameCallback(ElementPlusTableRowContext context);

[ECMAScript]
[Description("@#")]
public delegate VueStyleValue ElementPlusTableRowStyleCallback(ElementPlusTableRowContext context);

[ECMAScript]
[Description("@#")]
public delegate string ElementPlusTableCellClassNameCallback(ElementPlusTableCellContext context);

[ECMAScript]
[Description("@#")]
public delegate VueStyleValue ElementPlusTableCellStyleCallback(ElementPlusTableCellContext context);

[ECMAScript]
[Description("@#")]
public readonly union ElementPlusTableRowClassNameValue(string, ElementPlusTableRowClassNameCallback)
{
    public string? AsString => Value as string;

    public ElementPlusTableRowClassNameCallback? AsCallback => Value as ElementPlusTableRowClassNameCallback;
}

[ECMAScript]
[Description("@#")]
public readonly union ElementPlusTableRowStyleValue(VueStyleValue, ElementPlusTableRowStyleCallback)
{
    public VueStyleValue? AsStyle => Value is VueStyleValue value ? value : default(VueStyleValue?);

    public ElementPlusTableRowStyleCallback? AsCallback => Value as ElementPlusTableRowStyleCallback;
}

[ECMAScript]
[Description("@#")]
public readonly union ElementPlusTableCellClassNameValue(string, ElementPlusTableCellClassNameCallback)
{
    public string? AsString => Value as string;

    public ElementPlusTableCellClassNameCallback? AsCallback => Value as ElementPlusTableCellClassNameCallback;
}

[ECMAScript]
[Description("@#")]
public readonly union ElementPlusTableCellStyleValue(VueStyleValue, ElementPlusTableCellStyleCallback)
{
    public VueStyleValue? AsStyle => Value is VueStyleValue value ? value : default(VueStyleValue?);

    public ElementPlusTableCellStyleCallback? AsCallback => Value as ElementPlusTableCellStyleCallback;
}

[ECMAScript]
[Description("@#")]
public delegate string ElementPlusTableRowKeyCallback(VueDictionary row);

[ECMAScript]
[Description("@#")]
public readonly union ElementPlusTableRowKeyValue(string, ElementPlusTableRowKeyCallback)
{
    public string? AsString => Value as string;

    public ElementPlusTableRowKeyCallback? AsCallback => Value as ElementPlusTableRowKeyCallback;
}

[ECMAScript]
[Description("@#")]
public delegate VueStringNumberVNodeValue[] ElementPlusTableSummaryMethodCallback(ElementPlusTableSummaryMethodContext context);

[ECMAScript]
[Description("@#")]
public delegate ElementPlusTableSpanMethodResult? ElementPlusTableSpanMethodCallback(ElementPlusTableCellContext context);

[ECMAScript]
[Description("@#TableTreeNode")]
public sealed record ElementPlusTableTreeNode : VueProps
{
    [Description("@#expanded")]
    public bool? Expanded { get; init; }

    [Description("@#loading")]
    public bool? Loading { get; init; }

    [Description("@#indent")]
    public Number? Indent { get; init; }

    [Description("@#level")]
    public Number? Level { get; init; }
}

[ECMAScript]
[Description("@#")]
public delegate void ElementPlusTableResolveChildrenCallback(VueDictionary[] data);

[ECMAScript]
[Description("@#")]
public delegate void ElementPlusTableLoadCallback(VueDictionary row, ElementPlusTableTreeNode treeNode, ElementPlusTableResolveChildrenCallback resolve);

[ECMAScript]
[Description("@#")]
public delegate bool ElementPlusTableRowExpandableCallback(VueDictionary row, Number index);

[ECMAScript]
[Description("@#TableColumnHeaderContext")]
public sealed record ElementPlusTableColumnHeaderContext : VueProps
{
    [Description("@#column")]
    public ElementPlusTableColumnContext? Column { get; init; }

    [Description("@#$index")]
    public Number? Index { get; init; }

    [Description("@#store")]
    public VueDictionary? Store { get; init; }

    [Description("@#_self")]
    public VueValue? Self { get; init; }
}

[ECMAScript]
[Description("@#")]
public delegate Number ElementPlusTableColumnIndexCallback(Number index);

[ECMAScript]
[Description("@#")]
public readonly union ElementPlusTableColumnIndexValue(Number, ElementPlusTableColumnIndexCallback)
{
    public Number? AsNumber => Value is Number value ? value : default(Number?);

    public ElementPlusTableColumnIndexCallback? AsCallback => Value as ElementPlusTableColumnIndexCallback;
}

[ECMAScript]
[Description("@#")]
public delegate IVNode ElementPlusTableColumnRenderHeaderCallback(ElementPlusTableColumnHeaderContext context);

[ECMAScript]
[Description("@#")]
public delegate Number ElementPlusTableColumnSortMethodCallback(VueDictionary left, VueDictionary right);

[ECMAScript]
[Description("@#")]
public delegate string ElementPlusTableColumnSortByCallback(VueDictionary row, Number index, VueDictionary[]? array = null);

[ECMAScript]
[Description("@#")]
public readonly union ElementPlusTableColumnSortByValue(
    string,
    string[],
    ElementPlusTableColumnSortByCallback)
{
    public string? AsString => Value as string;

    public string[]? AsStrings => Value as string[];

    public ElementPlusTableColumnSortByCallback? AsCallback => Value as ElementPlusTableColumnSortByCallback;
}

[ECMAScript]
[Description("@#")]
public delegate VueStringNumberVNodeValue ElementPlusTableColumnFormatterCallback(
    VueDictionary row,
    ElementPlusTableColumnContext column,
    VueValue? cellValue,
    Number index);

[ECMAScript]
[Description("@#")]
public delegate bool ElementPlusTableColumnSelectableCallback(VueDictionary row, Number index);

[ECMAScript]
[Description("@#")]
public delegate void ElementPlusTableColumnFilterMethodCallback(string value, VueDictionary row, ElementPlusTableColumnContext column);

[ECMAScript]
[Description("@#SelectV2Option")]
public sealed record ElementPlusSelectV2Option : VueDictionary
{
    [Description("@#created")]
    public bool? Created { get; init; }
}

[ECMAScript]
[Description("@#OptionGroup")]
public sealed record ElementPlusSelectV2OptionGroup : VueDictionary
{
}

[ECMAScript]
[Description("@#")]
public readonly union ElementPlusSelectV2OptionValue(
    ElementPlusSelectV2Option,
    ElementPlusSelectV2OptionGroup)
{
    public ElementPlusSelectV2Option? AsOption => Value as ElementPlusSelectV2Option;

    public ElementPlusSelectV2OptionGroup? AsGroup => Value as ElementPlusSelectV2OptionGroup;
}

[ECMAScript]
[Description("@#")]
public readonly union ElementPlusSelectV2ModelValue(VueValue, VueValue[]) : IEnumerable<VueValue>
{
    public VueValue? AsSingle => Value as VueValue;

    public VueValue[]? AsMultiple => Value as VueValue[];

    IEnumerator<VueValue> IEnumerable<VueValue>.GetEnumerator()
        => ((IEnumerable<VueValue>)(AsMultiple ?? Array.Empty<VueValue>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VueValue>)this).GetEnumerator();
}
