namespace ECMAScript.ElementPlus;

// Defines Element Plus value domains, callback payloads, and erased union authoring contracts.
// 定义 Element Plus 值域、回调载荷与擦除 union；仅分支继承重叠时保留 tagged fallback。

[String]
public enum ElComponentSize
{
    [Description("@#large")]
    Large,

    [Description("@#default")]
    Default,

    [Description("@#small")]
    Small
}

[String]
public enum ElPopperEffect
{
    [Description("@#dark")]
    Dark,

    [Description("@#light")]
    Light
}

[String]
public enum ElPopperPlacement
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
public enum ElPopperPlacementSide
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
public enum ElCardShadow
{
    [Description("@#always")]
    Always,

    [Description("@#hover")]
    Hover,

    [Description("@#never")]
    Never
}

[String]
public enum ElUploadStatus
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
public enum ElHoverClickTrigger
{
    [Description("@#hover")]
    Hover,

    [Description("@#click")]
    Click
}

[String]
public enum ElCrossorigin
{
    [Description("@#")]
    Empty,

    [Description("@#anonymous")]
    Anonymous,

    [Description("@#use-credentials")]
    UseCredentials
}

[String]
public enum ElUploadListType
{
    [Description("@#text")]
    Text,

    [Description("@#picture")]
    Picture,

    [Description("@#picture-card")]
    PictureCard
}

[String]
public enum ElImageFitType
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
public enum ElImageLoadingType
{
    [Description("@#eager")]
    Eager,

    [Description("@#lazy")]
    Lazy
}

[String]
public enum ElAvatarShape
{
    [Description("@#circle")]
    Circle,

    [Description("@#square")]
    Square
}

[String]
public enum ElButtonType
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
public enum ElButtonNativeType
{
    [Description("@#button")]
    Button,

    [Description("@#submit")]
    Submit,

    [Description("@#reset")]
    Reset
}

[String]
public enum ElDirection
{
    [Description("@#horizontal")]
    Horizontal,

    [Description("@#vertical")]
    Vertical
}

[String]
public enum ElTopBottomPlacement
{
    [Description("@#top")]
    Top,

    [Description("@#bottom")]
    Bottom
}

[String]
public enum ElCarouselType
{
    [Description("@#")]
    Empty,

    [Description("@#card")]
    Card
}

[String]
public enum ElSemanticType
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
public enum ElTimelineMode
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
public enum ElCalendarControllerType
{
    [Description("@#button")]
    Button,

    [Description("@#select")]
    Select
}

[String]
public enum ElCollapseIconPosition
{
    [Description("@#left")]
    Left,

    [Description("@#right")]
    Right
}

[String]
public enum ElContentPosition
{
    [Description("@#left")]
    Left,

    [Description("@#center")]
    Center,

    [Description("@#right")]
    Right
}

[String]
public enum ElFormItemValidateStatus
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
public enum ElProgressType
{
    [Description("@#line")]
    Line,

    [Description("@#circle")]
    Circle,

    [Description("@#dashboard")]
    Dashboard
}

[String]
public enum ElProgressStatus
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
public enum ElStepStatus
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
public enum ElTabsType
{
    [Description("@#")]
    Empty,

    [Description("@#card")]
    Card,

    [Description("@#border-card")]
    BorderCard
}

[String]
public enum ElTagType
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
public enum ElTagEffect
{
    [Description("@#dark")]
    Dark,

    [Description("@#light")]
    Light,

    [Description("@#plain")]
    Plain
}

[String]
public enum ElLinkType
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
public sealed record ElStyles : VueDictionary<VueStringNumberValue>
{
}

[ECMAScript]
[Description("@#")]
public readonly union ElDirectiveValue(bool, VueProps)
{
    public bool? AsBool => Value is bool value ? value : default(bool?);

    public VueProps? AsProps => Value as VueProps;

    public static implicit operator ElDirectiveValue(VueDictionary value) => (VueProps)value;
}

[ECMAScript]
[Description("@#")]
public sealed record ElLoadingOptions : VueProps
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
public sealed record ElButtonConfig : VueProps
{
    [Description("@#autoInsertSpace")]
    public bool? AutoInsertSpace { get; init; }

    [Description("@#type")]
    public ElButtonType? Type { get; init; }

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
public sealed record ElCardConfig : VueProps
{
    [Description("@#shadow")]
    public ElCardShadow? Shadow { get; init; }
}

[ECMAScript]
[Description("@#MentionOption")]
public sealed record ElMentionOption : VueDictionary
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
public sealed record ElDialogConfig : VueProps
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
public sealed record ElLinkConfig : VueProps
{
    [Description("@#underline")]
    public VueBooleanStringValue? Underline { get; init; }

    [Description("@#type")]
    public ElLinkType? Type { get; init; }
}

[ECMAScript]
[Description("@#MessageConfigContext")]
public sealed record ElMessageConfig : VueProps
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
public sealed record ElTableConfig : VueProps
{
    [Description("@#showOverflowTooltip")]
    public ElTableOverflowTooltipValue? ShowOverflowTooltip { get; init; }

    [Description("@#tooltipEffect")]
    public string? TooltipEffect { get; init; }

    [Description("@#tooltipOptions")]
    public ElTableOverflowTooltipOptions? TooltipOptions { get; init; }

    [Description("@#tooltipFormatter")]
    public ElTableTooltipFormatter? TooltipFormatter { get; init; }
}

[ECMAScript]
[Description("@#TranslatePair")]
public sealed record ElTranslatePair : VueDictionary<ElTranslateValue>
{
}

[ECMAScript]
[Description("@#")]
public readonly union ElTranslateValue(string, string[], ElTranslatePair)
{
    public string? AsString => Value as string;

    public string[]? AsStrings => Value as string[];

    public ElTranslatePair? AsPair => Value as ElTranslatePair;
}

[ECMAScript]
[Description("@#Language")]
public sealed record ElLanguage : VueProps
{
    [Description("@#name")]
    public string? Name { get; init; }

    [Description("@#el")]
    public ElTranslatePair? El { get; init; }
}

[ECMAScript]
[Description("@#ValueOnClear")]
public readonly union ElValueOnClearValue(bool, double, string, ElValueOnClearCallback)
{
    public bool? AsBool => Value is bool value ? value : default(bool?);

    public double? AsNumber => Value is double value ? value : default(double?);

    public string? AsString => Value as string;

    public ElValueOnClearCallback? AsCallback => Value as ElValueOnClearCallback;

    [ECMAScriptInline("null")]
    public extern static ElValueOnClearValue Null();
}

[ECMAScript]
[Description("@#")]
public sealed record ElStringBooleanMap : VueDictionary<bool>
{
}

[ECMAScript]
[Description("@#")]
public readonly union ElStringBooleanClassValue(string, ElStringBooleanMap)
{
    public string? AsString => Value as string;

    public ElStringBooleanMap? AsMap => Value as ElStringBooleanMap;
}

[ECMAScript]
[Description("@#")]
public sealed record ElAutocompleteSuggestionItem : VueDictionary
{
}

[ECMAScript]
[Description("@#")]
public sealed record ElAutoResizerResizeContext : VueProps
{
    [Description("@#height")]
    public Number? Height { get; init; }

    [Description("@#width")]
    public Number? Width { get; init; }
}

[ECMAScript]
[Description("@#")]
public delegate void ElAutoResizerResizeCallback(ElAutoResizerResizeContext context);

[ECMAScript]
[Description("@#")]
public delegate void ElAutocompleteFetchSuggestionsCallback(ElAutocompleteSuggestionItem[] data);

[ECMAScript]
[Description("@#")]
public delegate IPromise<ElAutocompleteSuggestionItem[]?> ElAutocompleteFetchSuggestionsAsyncCallback(
    string queryString,
    ElAutocompleteFetchSuggestionsCallback callback);

[ECMAScript]
[Description("@#")]
public delegate void ElAutocompleteFetchSuggestionsCallbackOnly(string queryString, ElAutocompleteFetchSuggestionsCallback callback);

[ECMAScript]
[Description("@#")]
public readonly union ElAutocompleteFetchSuggestionsValue(
    ElAutocompleteSuggestionItem[],
    ElAutocompleteFetchSuggestionsCallbackOnly,
    ElAutocompleteFetchSuggestionsAsyncCallback)
{
    public ElAutocompleteSuggestionItem[]? AsSuggestions => Value as ElAutocompleteSuggestionItem[];

    public ElAutocompleteFetchSuggestionsCallbackOnly? AsCallback => Value as ElAutocompleteFetchSuggestionsCallbackOnly;

    public ElAutocompleteFetchSuggestionsAsyncCallback? AsAsyncCallback => Value as ElAutocompleteFetchSuggestionsAsyncCallback;
}

[ECMAScript]
[Description("@#")]
public sealed record ElCalendarDateCellContext : VueProps
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
public delegate VueStringNumberValue ElCalendarFormatterCallback(Number value, string type);

[ECMAScript]
[Description("@#")]
public delegate bool ElAsyncBooleanCallback();

[ECMAScript]
[Description("@#")]
public delegate IPromise<bool?> ElAsyncBooleanPromiseCallback();

[ECMAScript]
[Description("@#")]
public readonly union ElAsyncBooleanResult(bool, IPromise<bool?>)
{
    public bool? AsBool => Value is bool value ? value : default(bool?);

    public IPromise<bool?>? AsPromise => Value as IPromise<bool?>;
}

[ECMAScript]
[Description("@#")]
public delegate void ElDialogDoneCallback(bool? cancel = null);

[ECMAScript]
[Description("@#")]
public delegate void ElDialogBeforeCloseCallback(ElDialogDoneCallback done);

[ECMAScript]
[Description("@#")]
public delegate string ElInputFormatter(string value);

[ECMAScript]
[Description("@#")]
public delegate string ElInputParser(string value);

[ECMAScript]
[Description("@#")]
public delegate Number ElInputCountGraphemes(string value);

[ECMAScript]
[Description("@#")]
public delegate bool ElInputOtpValidator(string @char, Number index);

[ECMAScript]
[Description("@#")]
public delegate VueStringNumberVNodeValue ElInputOtpSeparatorRenderer(Number index);

[ECMAScript]
[Description("@#")]
public readonly union ElInputOtpSeparatorValue(string, IVNode, ElInputOtpSeparatorRenderer)
{
    public string? AsString => Value as string;

    public IVNode? AsVNode => Value as IVNode;

    public ElInputOtpSeparatorRenderer? AsRenderer => Value as ElInputOtpSeparatorRenderer;
}

[ECMAScript]
[Description("@#")]
public delegate bool ElMentionFilterOption(string pattern, ElMentionOption option);

[ECMAScript]
[Description("@#")]
public readonly union ElMentionFilterOptionValue(bool, ElMentionFilterOption)
{
    public bool? AsBool => Value is bool value ? value : default(bool?);

    public ElMentionFilterOption? AsCallback => Value as ElMentionFilterOption;
}

[ECMAScript]
[Description("@#")]
public delegate bool ElMentionCheckIsWhole(string pattern, string prefix);

[ECMAScript]
[Description("@#ProgressColor")]
public sealed record ElProgressColorStop : VueProps
{
    [Description("@#color")]
    public string? Color { get; init; }

    [Description("@#percentage")]
    public Number? Percentage { get; init; }
}

[ECMAScript]
[Description("@#")]
public delegate string ElProgressColorCallback(Number percentage);

[ECMAScript]
[Description("@#")]
public readonly union ElProgressColorValue(
    string,
    ElProgressColorStop[],
    ElProgressColorCallback)
{
    public string? AsString => Value as string;

    public ElProgressColorStop[]? AsStops => Value as ElProgressColorStop[];

    public ElProgressColorCallback? AsCallback => Value as ElProgressColorCallback;
}

[ECMAScript]
[Description("@#")]
public delegate string ElProgressFormatCallback(Number percentage);

[ECMAScript]
[Description("@#")]
public delegate bool ElCascaderFilterMethod(VueDictionary node, string keyword);

[ECMAScript]
[Description("@#")]
public delegate IPromise<VueValue?> ElCascaderBeforeFilterAsyncCallback(string value);

[ECMAScript]
[Description("@#")]
public delegate bool ElCascaderBeforeFilterSyncCallback(string value);

[ECMAScript]
[Description("@#")]
public readonly union ElCascaderBeforeFilterResult(bool, IPromise<VueValue?>)
{
    public bool? AsBool => Value is bool value ? value : default(bool?);

    public IPromise<VueValue?>? AsPromise => Value as IPromise<VueValue?>;
}

[ECMAScript]
[Description("@#")]
public delegate ElCascaderBeforeFilterResult ElCascaderBeforeFilterCallback(string value);

[ECMAScript]
[Description("@#")]
public delegate IPromise<bool?> ElCollapseBeforeCollapseAsyncCallback(VueStringNumberValue name);

[ECMAScript]
[Description("@#")]
public delegate bool ElCollapseBeforeCollapseSyncCallback(VueStringNumberValue name);

[ECMAScript]
[Description("@#")]
public readonly union ElCollapseBeforeCollapseResult(bool, IPromise<bool?>)
{
    public bool? AsBool => Value is bool value ? value : default(bool?);

    public IPromise<bool?>? AsPromise => Value as IPromise<bool?>;
}

[ECMAScript]
[Description("@#")]
public delegate ElCollapseBeforeCollapseResult ElCollapseBeforeCollapseCallback(VueStringNumberValue name);

[ECMAScript]
[Description("@#")]
public delegate string ElDateLikeCellClassName(Date date);

[ECMAScript]
[Description("@#")]
public delegate bool ElDateLikeDisabledDate(Date date);

[ECMAScript]
[Description("@#")]
public delegate void ElValueOnClearCallback();

[ECMAScript]
[Description("@#TableOverflowTooltipOptions")]
public sealed record ElTableOverflowTooltipOptions : VueProps
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
public readonly union ElTableOverflowTooltipValue(bool, ElTableOverflowTooltipOptions)
{
    public bool? AsBool => Value is bool value ? value : default(bool?);

    public ElTableOverflowTooltipOptions? AsOptions => Value as ElTableOverflowTooltipOptions;
}

[ECMAScript]
[Description("@#InputAutoSizeOptions")]
public sealed record ElInputAutoSizeOptions : VueProps
{
    [Description("@#minRows")]
    public Number? MinRows { get; init; }

    [Description("@#maxRows")]
    public Number? MaxRows { get; init; }
}

[ECMAScript]
[Description("@#InputAutoSize")]
public readonly union ElInputAutoSize(bool, ElInputAutoSizeOptions)
{
    public bool? AsBool => Value is bool value ? value : default(bool?);

    public ElInputAutoSizeOptions? AsOptions => Value as ElInputAutoSizeOptions;
}

[ECMAScript]
[Description("@#ColSizeObject")]
public sealed record ElColSizeProps : VueProps
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
public readonly union ElColSizeValue(double, ElColSizeProps)
{
    public double? AsNumber => Value is double value ? value : default(double?);

    public ElColSizeProps? AsProps => Value as ElColSizeProps;
}

[ECMAScript]
[Description("@#")]
public readonly union ElSpaceSizeValue(ElComponentSize, Number, VueNumberPair)
{
    public ElComponentSize? AsComponentSize
        => Value is ElComponentSize value ? value : default(ElComponentSize?);

    public Number? AsNumber => Value is Number value ? value : default(Number?);

    public VueNumberPair? AsPair => Value is VueNumberPair value ? value : default(VueNumberPair?);

    public static implicit operator ElSpaceSizeValue(double value)
        => new((Number)value);
}

[ECMAScript]
[Description("@#ThrottleRender")]
public sealed record ElThrottleRenderOptions : VueProps
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
public readonly union ElThrottleValue(Number, ElThrottleRenderOptions)
{
    public Number? AsNumber => Value is Number value ? value : default(Number?);

    public ElThrottleRenderOptions? AsOptions => Value as ElThrottleRenderOptions;

    public static implicit operator ElThrottleValue(double value)
        => new((Number)value);
}

[String]
public enum ElTableSortOrder
{
    [Description("@#ascending")]
    Ascending,

    [Description("@#descending")]
    Descending
}

[ECMAScript]
[Description("@#Sort")]
public sealed record ElTableSort : VueProps
{
    [Description("@#prop")]
    public string? Prop { get; init; }

    [Description("@#order")]
    public ElTableSortOrder? Order { get; init; }

    [Description("@#init")]
    public VueValue? Init { get; init; }

    [Description("@#silent")]
    public VueValue? Silent { get; init; }
}

[ECMAScript]
[Description("@#TreeProps")]
public sealed record ElTableTreeProps : VueProps
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
public sealed record ElTableFilterItem : VueProps
{
    [Description("@#text")]
    public string? Text { get; init; }

    [Description("@#value")]
    public string? Value { get; init; }
}

[ECMAScript]
[Description("@#UploadRawFile")]
public sealed record ElUploadRawFile : VueProps
{
    [Description("@#uid")]
    public Number Uid { get; init; } = default!;

    [Description("@#isDirectory")]
    public bool? IsDirectory { get; init; }
}

[ECMAScript]
[Description("@#UploadUserFile")]
public sealed record ElUploadUserFile : VueProps
{
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    [Description("@#percentage")]
    public Number? Percentage { get; init; }

    [Description("@#status")]
    public ElUploadStatus? Status { get; init; }

    [Description("@#size")]
    public Number? Size { get; init; }

    [Description("@#response")]
    public VueValue? Response { get; init; }

    [Description("@#uid")]
    public Number? Uid { get; init; }

    [Description("@#url")]
    public string? Url { get; init; }

    [Description("@#raw")]
    public ElUploadRawFile? Raw { get; init; }
}

[ECMAScript]
[Description("@#UploadFile")]
public sealed record ElUploadFile : VueProps
{
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    [Description("@#percentage")]
    public Number? Percentage { get; init; }

    [Description("@#status")]
    public ElUploadStatus Status { get; init; } = default!;

    [Description("@#size")]
    public Number? Size { get; init; }

    [Description("@#response")]
    public VueValue? Response { get; init; }

    [Description("@#uid")]
    public Number Uid { get; init; } = default!;

    [Description("@#url")]
    public string? Url { get; init; }

    [Description("@#raw")]
    public ElUploadRawFile? Raw { get; init; }
}

[String]
public enum ElTooltipTriggerType
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
public enum ElDropdownTriggerType
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
public readonly union ElDropdownTriggerValue(ElDropdownTriggerType, ElDropdownTriggerType[])
{
    public ElDropdownTriggerType? AsSingle
        => Value is ElDropdownTriggerType value ? value : default(ElDropdownTriggerType?);

    public ElDropdownTriggerType[]? AsMultiple => Value as ElDropdownTriggerType[];
}

[ECMAScript]
[Description("@#")]
public readonly union ElTooltipTriggerValue(ElTooltipTriggerType, ElTooltipTriggerType[])
{
    public ElTooltipTriggerType? AsSingle
        => Value is ElTooltipTriggerType value ? value : default(ElTooltipTriggerType?);

    public ElTooltipTriggerType[]? AsMultiple => Value as ElTooltipTriggerType[];
}

[ECMAScript]
[Description("@#TagTooltipProps")]
public sealed record ElTagTooltipProps : VueProps
{
    [Description("@#appendTo")]
    public VueTeleportTarget? AppendTo { get; init; }

    [Description("@#placement")]
    public ElPopperPlacement? Placement { get; init; }

    [Description("@#fallbackPlacements")]
    public ElPopperPlacement[]? FallbackPlacements { get; init; }

    [Description("@#effect")]
    public ElPopperEffect? Effect { get; init; }

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
public sealed record ElButtonProps : VueProps
{
    [Description("@#size")]
    public ElComponentSize? Size { get; init; }

    [Description("@#disabled")]
    public bool? Disabled { get; init; }

    [Description("@#type")]
    public ElButtonType? Type { get; init; }

    [Description("@#icon")]
    public VueStringComponentValue? Icon { get; init; }

    [Description("@#nativeType")]
    public ElButtonNativeType? NativeType { get; init; }

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
public sealed record ElTransferDataItem : VueDictionary
{
}

[String]
public enum ElTransferTargetOrder
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
public sealed record ElTransferFormat : VueProps
{
    [Description("@#noChecked")]
    public string? NoChecked { get; init; }

    [Description("@#hasChecked")]
    public string? HasChecked { get; init; }
}

[ECMAScript]
[Description("@#TransferPropsAlias")]
public sealed record ElTransferPropsAlias : VueProps
{
    [Description("@#label")]
    public string? Label { get; init; }

    [Description("@#key")]
    public string? Key { get; init; }

    [Description("@#disabled")]
    public string? Disabled { get; init; }
}

[ECMAScript]
[Union]
[Description("@#")]
[CollectionBuilder(typeof(ElTransferTextPairCollectionBuilder), nameof(ElTransferTextPairCollectionBuilder.Create))]
public readonly struct ElTransferTextPair : IUnion, IEnumerable<string>
{
    private readonly string[]? _values;

    public ElTransferTextPair(string[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        if (values.Length != 2)
            throw new ArgumentException("Element Plus transfer text pairs require exactly two items.", nameof(values));

        _values = values;
    }

    public string[]? AsValues => _values;

    public string? First => _values is { Length: > 0 } values ? values[0] : null;

    public string? Second => _values is { Length: > 1 } values ? values[1] : null;

    public object? Value => _values;

    public static implicit operator ElTransferTextPair(string[] values)
        => new(values);

    IEnumerator<string> IEnumerable<string>.GetEnumerator()
        => ((IEnumerable<string>)(_values ?? Array.Empty<string>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<string>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class ElTransferTextPairCollectionBuilder
{
    public static ElTransferTextPair Create(ReadOnlySpan<string> values)
        => values.ToArray();
}

[ECMAScript]
[Description("@#SelectPropsAlias")]
public sealed record ElSelectPropsAlias : VueProps
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
public sealed record ElCheckboxOptionPropsAlias : VueProps
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
public sealed record ElMentionOptionPropsAlias : VueProps
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
public sealed record ElRadioOptionPropsAlias : VueProps
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
public sealed record ElSegmentedPropsAlias : VueProps
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
public sealed record ElTreeOptionProps : VueProps
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
    public ElTreeOptionClassCallback? CssClass { get; init; }
}

[ECMAScript]
[Description("@#CascaderProps")]
public sealed record ElCascaderProps : VueProps
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
    public ElCascaderLazyLoadCallback? LazyLoad { get; init; }

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
public sealed record ElFormItemRule : VueDictionary
{
    [Description("@#trigger")]
    public VueStringOrStringsValue? Trigger { get; init; }
}

[ECMAScript]
[Description("@#")]
public readonly union ElFormItemRules(
    ElFormItemRule,
    ElFormItemRule[]) : IEnumerable<ElFormItemRule>
{
    public ElFormItemRule? AsSingle
        => Value as ElFormItemRule;

    public ElFormItemRule[]? AsMultiple => Value as ElFormItemRule[];

    IEnumerator<ElFormItemRule> IEnumerable<ElFormItemRule>.GetEnumerator()
        => ((IEnumerable<ElFormItemRule>)(AsMultiple ?? Array.Empty<ElFormItemRule>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<ElFormItemRule>)this).GetEnumerator();
}

[ECMAScript]
[Description("@#")]
public readonly union ElFormRuleValue(ElFormItemRules, ElFormRules)
{
    public ElFormItemRules? AsItemRules
        => Value is ElFormItemRules value ? value : default(ElFormItemRules?);

    public ElFormRules? AsNestedRules => Value as ElFormRules;
}

[ECMAScript]
[Description("@#FormRules")]
public sealed record ElFormRules : VueDictionary<ElFormRuleValue>
{
}

[ECMAScript]
[Description("@#RateColorMap")]
public sealed record ElRateColorMap : VueDictionary<string>
{
}

[ECMAScript]
[Description("@#")]
public readonly union ElRateColorsValue(
    string[],
    ElRateColorMap) : IEnumerable<string>
{
    public string[]? AsArray => Value as string[];

    public ElRateColorMap? AsMap => Value as ElRateColorMap;

    IEnumerator<string> IEnumerable<string>.GetEnumerator()
        => ((IEnumerable<string>)(AsArray ?? Array.Empty<string>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<string>)this).GetEnumerator();
}

[ECMAScript]
[Description("@#RateIconMap")]
public sealed record ElRateIconMap : VueDictionary<VueStringComponentValue>
{
}

[ECMAScript]
[Description("@#")]
public readonly union ElRateIconsValue(
    VueStringComponentValue[],
    ElRateIconMap) : IEnumerable<VueStringComponentValue>
{
    public VueStringComponentValue[]? AsArray => Value as VueStringComponentValue[];

    public ElRateIconMap? AsMap => Value as ElRateIconMap;

    public static implicit operator ElRateIconsValue(string[] values)
        => new(Array.ConvertAll(values, static value => (VueStringComponentValue)value));

    public static implicit operator ElRateIconsValue(IVueComponent[] values)
        => new(Array.ConvertAll(values, static value => (VueStringComponentValue)value));

    IEnumerator<VueStringComponentValue> IEnumerable<VueStringComponentValue>.GetEnumerator()
        => ((IEnumerable<VueStringComponentValue>)(AsArray ?? Array.Empty<VueStringComponentValue>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VueStringComponentValue>)this).GetEnumerator();
}

[ECMAScript]
[Description("@#SliderMarker")]
public sealed record ElSliderMarker : VueProps
{
    [Description("@#style")]
    public VueStyleValue? Style { get; init; }

    [Description("@#label")]
    public VueValue? Label { get; init; }
}

[ECMAScript]
[Description("@#")]
public readonly union ElSliderMarkValue(string, ElSliderMarker)
{
    public string? AsString => Value as string;

    public ElSliderMarker? AsMarker => Value as ElSliderMarker;
}

[ECMAScript]
[Description("@#SliderMarks")]
public sealed record ElSliderMarks : VueDictionary<ElSliderMarkValue>
{
}

[String]
public enum ElTableV2SortOrder
{
    [Description("@#asc")]
    Asc,

    [Description("@#desc")]
    Desc
}

[String]
public enum ElTableV2Alignment
{
    [Description("@#left")]
    Left,

    [Description("@#center")]
    Center,

    [Description("@#right")]
    Right
}

[String]
public enum ElTableV2FixedDirection
{
    [Description("@#left")]
    Left,

    [Description("@#right")]
    Right
}

[ECMAScript]
[Description("@#TableV2ClassContext")]
public sealed record ElTableV2ClassContext : VueProps
{
    [Description("@#columns")]
    public ElTableV2Column[]? Columns { get; init; }

    [Description("@#column")]
    public ElTableV2Column? Column { get; init; }

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
public delegate string ElTableV2ClassGetter(ElTableV2ClassContext context);

[ECMAScript]
[Description("@#")]
public readonly union ElTableV2ClassValue(string, ElTableV2ClassGetter)
{
    public string? AsString => Value as string;

    public ElTableV2ClassGetter? AsGetter => Value as ElTableV2ClassGetter;
}

[ECMAScript]
[Description("@#TableV2DynamicPropsContext")]
public sealed record ElTableV2DynamicPropsContext : VueProps
{
    [Description("@#columns")]
    public ElTableV2Column[]? Columns { get; init; }

    [Description("@#column")]
    public ElTableV2Column? Column { get; init; }

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
public delegate VueDictionary ElTableV2DynamicPropsGetter(ElTableV2DynamicPropsContext context);

[ECMAScript]
[Description("@#")]
public readonly union ElTableV2DynamicPropsValue(VueDictionary, ElTableV2DynamicPropsGetter)
{
    public VueDictionary? AsObject => Value as VueDictionary;

    public ElTableV2DynamicPropsGetter? AsGetter => Value as ElTableV2DynamicPropsGetter;
}

[ECMAScript]
[Description("@#")]
public readonly union ElTableV2KeyValue(VueKey)
{
    public VueKey? AsKey => Value as VueKey;

    public static implicit operator ElTableV2KeyValue(string value)
        => new((VueKey)value);

    public static implicit operator ElTableV2KeyValue(Symbol value)
        => new((VueKey)value);

    public static implicit operator ElTableV2KeyValue(Number value)
        => new((VueKey)value);

    public static implicit operator ElTableV2KeyValue(byte value)
        => new((VueKey)value);

    public static implicit operator ElTableV2KeyValue(sbyte value)
        => new((VueKey)value);

    public static implicit operator ElTableV2KeyValue(short value)
        => new((VueKey)value);

    public static implicit operator ElTableV2KeyValue(ushort value)
        => new((VueKey)value);

    public static implicit operator ElTableV2KeyValue(int value)
        => new((VueKey)value);

    public static implicit operator ElTableV2KeyValue(uint value)
        => new((VueKey)value);

    public static implicit operator ElTableV2KeyValue(long value)
        => new((VueKey)value);

    public static implicit operator ElTableV2KeyValue(ulong value)
        => new((VueKey)value);

    public static implicit operator ElTableV2KeyValue(float value)
        => new((VueKey)value);

    public static implicit operator ElTableV2KeyValue(double value)
        => new((VueKey)value);

    public static implicit operator ElTableV2KeyValue(decimal value)
        => new((VueKey)value);
}

[ECMAScript]
[Description("@#SortBy")]
public sealed record ElTableV2SortBy : VueProps
{
    [Description("@#key")]
    public ElTableV2KeyValue? Key { get; init; }

    [Description("@#order")]
    public ElTableV2SortOrder? Order { get; init; }
}

[ECMAScript]
[Description("@#SortState")]
public sealed record ElTableV2SortState : VueDictionary<ElTableV2SortOrder>
{
}

[ECMAScript]
[Description("@#Column")]
public sealed record ElTableV2Column : VueDictionary
{
    [Description("@#align")]
    public ElTableV2Alignment? Align { get; init; }

    [Description("@#class")]
    public ElTableV2ClassValue? CssClass { get; init; }

    [Description("@#key")]
    public ElTableV2KeyValue? Key { get; init; }

    [Description("@#dataKey")]
    public ElTableV2KeyValue? DataKey { get; init; }

    [Description("@#fixed")]
    public ElTableV2FixedValue? Fixed { get; init; }

    [Description("@#title")]
    public string? Title { get; init; }

    [Description("@#hidden")]
    public bool? Hidden { get; init; }

    [Description("@#headerClass")]
    public ElTableV2ClassValue? HeaderClass { get; init; }

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
    public ElTableV2CellRenderer? CellRenderer { get; init; }

    [Description("@#headerCellRenderer")]
    public ElTableV2HeaderCellRenderer? HeaderCellRenderer { get; init; }
}

[ECMAScript]
[Description("@#")]
public readonly union ElTableV2FixedValue(bool, ElTableV2FixedDirection)
{
    public bool? AsBool => Value is bool value ? value : default(bool?);

    public ElTableV2FixedDirection? AsDirection
        => Value is ElTableV2FixedDirection value ? value : default(ElTableV2FixedDirection?);
}

[ECMAScript]
[Description("@#TableData")]
public sealed record ElTableV2DataItem : VueDictionary
{
}

[ECMAScript]
[Description("@#")]
public readonly union ElTableV2HeaderHeightValue(double, Number[]) : IEnumerable<Number>
{
    public double? AsNumber => Value is double value ? value : default(double?);

    public Number[]? AsNumbers => Value is Number[] values ? values : default(Number[]?);

    public static implicit operator ElTableV2HeaderHeightValue(double[] values)
        => new(Array.ConvertAll(values, static value => (Number)value));

    IEnumerator<Number> IEnumerable<Number>.GetEnumerator()
        => ((IEnumerable<Number>)(AsNumbers ?? Array.Empty<Number>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<Number>)this).GetEnumerator();
}

[ECMAScript]
[Description("@#TableV2DataGetterContext")]
public sealed record ElTableV2DataGetterContext : VueProps
{
    [Description("@#columns")]
    public ElTableV2Column[]? Columns { get; init; }

    [Description("@#column")]
    public ElTableV2Column? Column { get; init; }

    [Description("@#columnIndex")]
    public Number? ColumnIndex { get; init; }

    [Description("@#rowData")]
    public VueValue? RowData { get; init; }

    [Description("@#rowIndex")]
    public Number? RowIndex { get; init; }
}

[ECMAScript]
[Description("@#DataGetter")]
public delegate VueValue ElTableV2DataGetter(ElTableV2DataGetterContext context);

[ECMAScript]
[Description("@#TableV2CellRendererContext")]
public sealed record ElTableV2CellRendererContext : VueProps
{
    [Description("@#columns")]
    public ElTableV2Column[]? Columns { get; init; }

    [Description("@#column")]
    public ElTableV2Column? Column { get; init; }

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
public sealed record ElTableV2HeaderCellRendererContext : VueProps
{
    [Description("@#columns")]
    public ElTableV2Column[]? Columns { get; init; }

    [Description("@#column")]
    public ElTableV2Column? Column { get; init; }

    [Description("@#columnIndex")]
    public Number? ColumnIndex { get; init; }

    [Description("@#headerIndex")]
    public Number? HeaderIndex { get; init; }
}

[ECMAScript]
[Description("@#CellRenderer")]
public delegate IVNode ElTableV2CellRenderer(ElTableV2CellRendererContext context);

[ECMAScript]
[Description("@#HeaderCellRenderer")]
public delegate IVNode ElTableV2HeaderCellRenderer(ElTableV2HeaderCellRendererContext context);

[ECMAScript]
[Description("@#TableV2RowEventHandlerContext")]
public sealed record ElTableV2RowEventHandlerContext : VueProps
{
    [Description("@#rowKey")]
    public ElTableV2KeyValue? RowKey { get; init; }

    [Description("@#event")]
    public JazorEvent? Event { get; init; }

    [Description("@#rowData")]
    public VueValue? RowData { get; init; }

    [Description("@#rowIndex")]
    public Number? RowIndex { get; init; }
}

[ECMAScript]
[Description("@#RowEventHandler")]
public delegate void ElTableV2RowEventHandler(ElTableV2RowEventHandlerContext context);

[ECMAScript]
[Description("@#RowEventHandlers")]
public sealed record ElTableV2RowEventHandlers : VueProps
{
    [Description("@#onClick")]
    public ElTableV2RowEventHandler? OnClick { get; init; }

    [Description("@#onContextmenu")]
    public ElTableV2RowEventHandler? OnContextmenu { get; init; }

    [Description("@#onDblclick")]
    public ElTableV2RowEventHandler? OnDblclick { get; init; }

    [Description("@#onMouseenter")]
    public ElTableV2RowEventHandler? OnMouseenter { get; init; }

    [Description("@#onMouseleave")]
    public ElTableV2RowEventHandler? OnMouseleave { get; init; }
}

[ECMAScript]
[Description("@#")]
public readonly union ElTransferRenderContentResult(IVNode, IVNode[])
{
    public IVNode? AsSingle => Value as IVNode;

    public IVNode[]? AsMultiple => Value as IVNode[];
}

[ECMAScript]
[Description("@#")]
public delegate bool ElTransferFilterMethod(string query, ElTransferDataItem item);

[ECMAScript]
[Description("@#renderContent")]
public delegate ElTransferRenderContentResult ElTransferRenderContent(VueRenderHost h, ElTransferDataItem option);

[ECMAScript]
[Description("@#")]
public sealed record ElTreeNode : VueDictionary
{
}

[ECMAScript]
[Description("@#")]
public sealed record ElTreeNodeData : VueDictionary
{
}

[ECMAScript]
[Description("@#")]
public sealed record ElTreeRenderContentContext : VueProps
{
    [Description("@#node")]
    public ElTreeNode? Node { get; init; }

    [Description("@#data")]
    public ElTreeNodeData? Data { get; init; }

    [Description("@#store")]
    public VueDictionary? Store { get; init; }

    [Description("@#_self")]
    public VueValue? Self { get; init; }
}

[ECMAScript]
[Description("@#")]
public sealed record ElTreeDropIndicator : VueProps
{
    [Description("@#type")]
    public string? Type { get; init; }
}

[ECMAScript]
[Description("@#")]
public delegate ElStringBooleanClassValue ElTreeOptionClassCallback(ElTreeNodeData data, ElTreeNode node);

[ECMAScript]
[Description("@#")]
public delegate void ElCascaderLazyResolveCallback(VueDictionary[]? dataList = null);

[ECMAScript]
[Description("@#")]
public delegate void ElCascaderLazyRejectCallback();

[ECMAScript]
[Description("@#")]
public delegate void ElCascaderLazyLoadCallback(
    VueDictionary node,
    ElCascaderLazyResolveCallback resolve,
    ElCascaderLazyRejectCallback reject);

[ECMAScript]
[Description("@#")]
public delegate ElTransferRenderContentResult ElTreeRenderContentCallback(VueRenderHost h, ElTreeRenderContentContext context);

[ECMAScript]
[Description("@#")]
public delegate void ElTreeResolveChildrenCallback(ElTreeNodeData[] data);

[ECMAScript]
[Description("@#")]
public delegate void ElTreeStopLoadingCallback();

[ECMAScript]
[Description("@#")]
public delegate void ElTreeLoadCallback(
    ElTreeNode rootNode,
    ElTreeResolveChildrenCallback loadedCallback,
    ElTreeStopLoadingCallback stopLoading);

[ECMAScript]
[Description("@#")]
public delegate bool ElTreeFilterNodeMethod(VueValue? value, ElTreeNodeData data, ElTreeNode child);

[ECMAScript]
[Description("@#")]
public delegate bool ElTreeAllowDragCallback(ElTreeNode node);

[ECMAScript]
[Description("@#")]
public delegate bool ElTreeAllowDropCallback(
    ElTreeNode draggingNode,
    ElTreeNode dropNode,
    string type);

[ECMAScript]
[Description("@#")]
public delegate bool ElTreeV2FilterMethod(string query, ElTreeNodeData data, ElTreeNode node);

[ECMAScript]
[Description("@#")]
public delegate void ElSelectQueryCallback(string query);

[ECMAScript]
[Description("@#")]
public delegate VueStringNumberValue ElSliderFormatTooltipCallback(Number value);

[ECMAScript]
[Description("@#")]
public delegate string ElSliderFormatValueTextCallback(Number value);

[ECMAScript]
[Description("@#")]
public delegate ElAsyncBooleanResult ElSwitchBeforeChangeCallback();

[ECMAScript]
[Description("@#")]
public delegate IPromise<bool?> ElTabsBeforeLeaveAsyncCallback(VueStringNumberValue? newName, VueStringNumberValue? oldName);

[ECMAScript]
[Description("@#")]
public delegate bool? ElTabsBeforeLeaveSyncCallback(VueStringNumberValue? newName, VueStringNumberValue? oldName);

[ECMAScript]
[Description("@#")]
public readonly union ElTabsBeforeLeaveResult(bool, IPromise<bool?>)
{
    public bool? AsBool => Value is bool value ? value : default(bool?);

    public IPromise<bool?>? AsPromise => Value as IPromise<bool?>;
}

[ECMAScript]
[Description("@#")]
public delegate ElTabsBeforeLeaveResult? ElTabsBeforeLeaveCallback(
    VueStringNumberValue? newName,
    VueStringNumberValue? oldName);

[ECMAScript]
[Description("@#UploadProgressEvent")]
public sealed record ElUploadProgressEvent : VueProps
{
    [Description("@#percent")]
    public Number? Percent { get; init; }
}

[ECMAScript]
[Description("@#UploadError")]
public sealed record ElUploadAjaxError : VueProps
{
    [Description("@#name")]
    public string? Name { get; init; }

    [Description("@#message")]
    public string? Message { get; init; }
}

[ECMAScript]
[Description("@#UploadRequestData")]
public sealed record ElUploadRequestData : VueDictionary<VueValue>
{
}

[ECMAScript]
[Description("@#UploadRequestHeaders")]
public readonly union ElUploadRequestHeaders(Headers, VueDictionary)
{
    public Headers? AsHeaders => Value as Headers;

    public VueDictionary? AsDictionary => Value as VueDictionary;
}

[ECMAScript]
[Description("@#UploadRequestOptions")]
public sealed record ElUploadRequestOptions : VueProps
{
    [Description("@#action")]
    public string Action { get; init; } = string.Empty;

    [Description("@#method")]
    public string Method { get; init; } = string.Empty;

    [Description("@#data")]
    public ElUploadRequestData Data { get; init; } = new();

    [Description("@#filename")]
    public string Filename { get; init; } = string.Empty;

    [Description("@#file")]
    public ElUploadRawFile File { get; init; } = default!;

    [Description("@#headers")]
    public ElUploadRequestHeaders? Headers { get; init; }

    [Description("@#onError")]
    public ElUploadRequestOnErrorCallback? OnError { get; init; }

    [Description("@#onProgress")]
    public ElUploadRequestOnProgressCallback? OnProgress { get; init; }

    [Description("@#onSuccess")]
    public ElUploadRequestOnSuccessCallback? OnSuccess { get; init; }

    [Description("@#withCredentials")]
    public bool? WithCredentials { get; init; }
}

[ECMAScript]
[Description("@#")]
public delegate void ElUploadRequestOnErrorCallback(ElUploadAjaxError error);

[ECMAScript]
[Description("@#")]
public delegate void ElUploadRequestOnProgressCallback(ElUploadProgressEvent @event);

[ECMAScript]
[Description("@#")]
public delegate void ElUploadRequestOnSuccessCallback(VueValue? response);

[ECMAScript]
[Description("@#")]
public readonly union ElUploadRequestResult(XMLHttpRequest, IPromise<VueValue?>)
{
    public XMLHttpRequest? AsRequest => Value as XMLHttpRequest;

    public IPromise<VueValue?>? AsPromise => Value as IPromise<VueValue?>;
}

[ECMAScript]
[Description("@#")]
public delegate ElUploadRequestResult ElUploadRequestCallback(ElUploadRequestOptions options);

[ECMAScript]
[Description("@#UploadData")]
public sealed record ElUploadData : VueDictionary<VueValue>
{
}

[ECMAScript]
[Description("@#")]
public delegate IPromise<ElUploadData> ElUploadDataPromiseFactory(ElUploadRawFile rawFile);

[ECMAScript]
[Description("@#")]
public delegate ElUploadData ElUploadDataFactory(ElUploadRawFile rawFile);

[ECMAScript]
[Description("@#")]
public readonly union ElUploadDataValue(
    ElUploadData,
    IPromise<ElUploadData>,
    ElUploadDataFactory,
    ElUploadDataPromiseFactory)
{
    public ElUploadData? AsData => Value as ElUploadData;

    public IPromise<ElUploadData>? AsPromise => Value as IPromise<ElUploadData>;

    public ElUploadDataFactory? AsFactory => Value as ElUploadDataFactory;

    public ElUploadDataPromiseFactory? AsAsyncFactory => Value as ElUploadDataPromiseFactory;
}

[ECMAScript]
[Union]
[Description("@#")]
public readonly struct ElUploadBeforeUploadResult : IUnion
{
    // File derives from Blob, so Value-based native-union projections cannot preserve the authored branch.
    // File 继承 Blob；这里必须保留显式 tag，确保 AsFile 与 AsBlob 不会同时命中。
    private readonly byte _kind;
    private readonly bool? _bool;
    private readonly JazorFile? _file;
    private readonly Blob? _blob;
    private readonly IPromise<VueValue?>? _promise;

    public ElUploadBeforeUploadResult(bool value)
    {
        _kind = 1;
        _bool = value;
        _file = default;
        _blob = default;
        _promise = default;
    }

    public ElUploadBeforeUploadResult(JazorFile value)
    {
        _kind = 2;
        _bool = default;
        _file = value;
        _blob = default;
        _promise = default;
    }

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
    public ElUploadBeforeUploadResult(IPromise<VueValue?> value)
    {
        _kind = 4;
        _bool = default;
        _file = default;
        _blob = default;
        _promise = value;
    }

    public bool? AsBool => _kind == 1 ? _bool : default;

    public JazorFile? AsFile => _kind == 2 ? _file : default;

    public Blob? AsBlob => _kind == 3 ? _blob : default;

    public IPromise<VueValue?>? AsPromise => _kind == 4 ? _promise : default;

    public object? Value => _kind switch
    {
        1 => AsBool,
        2 => AsFile,
        3 => AsBlob,
        4 => AsPromise,
        _ => default
    };

    public static implicit operator ElUploadBeforeUploadResult(bool value)
        => new(value);

    public static implicit operator ElUploadBeforeUploadResult(JazorFile value)
        => new(value);

    public static implicit operator ElUploadBeforeUploadResult(Blob value)
        => new(value);

}

[ECMAScript]
[Description("@#")]
public delegate ElUploadBeforeUploadResult? ElUploadBeforeUploadCallback(ElUploadRawFile rawFile);

[ECMAScript]
[Description("@#")]
public delegate bool ElUploadBeforeRemoveSyncCallback(ElUploadFile uploadFile, ElUploadFile[] uploadFiles);

[ECMAScript]
[Description("@#")]
public delegate IPromise<bool?> ElUploadBeforeRemoveAsyncCallback(ElUploadFile uploadFile, ElUploadFile[] uploadFiles);

[ECMAScript]
[Description("@#")]
public readonly union ElUploadBeforeRemoveResult(bool, IPromise<bool?>)
{
    public bool? AsBool => Value is bool value ? value : default(bool?);

    public IPromise<bool?>? AsPromise => Value as IPromise<bool?>;
}

[ECMAScript]
[Description("@#")]
public delegate ElUploadBeforeRemoveResult ElUploadBeforeRemoveCallback(ElUploadFile uploadFile, ElUploadFile[] uploadFiles);

[ECMAScript]
[Description("@#")]
public delegate void ElUploadPreviewCallback(ElUploadFile uploadFile);

[ECMAScript]
[Description("@#")]
public delegate void ElUploadFileListCallback(ElUploadFile uploadFile, ElUploadFile[] uploadFiles);

[ECMAScript]
[Description("@#")]
public delegate void ElUploadSuccessCallback(VueValue? response, ElUploadFile uploadFile, ElUploadFile[] uploadFiles);

[ECMAScript]
[Description("@#")]
public delegate void ElUploadProgressCallback(ElUploadProgressEvent @event, ElUploadFile uploadFile, ElUploadFile[] uploadFiles);

[ECMAScript]
[Description("@#")]
public delegate void ElUploadErrorCallback(Error error, ElUploadFile uploadFile, ElUploadFile[] uploadFiles);

[ECMAScript]
[Description("@#")]
public delegate void ElUploadExceedCallback(JazorFile[] files, ElUploadUserFile[] uploadFiles);

[ECMAScript]
[Description("@#")]
public delegate Number[] ElTimePickerDisabledHoursCallback(string role, Dayjs? comparingDate = null);

[ECMAScript]
[Description("@#")]
public delegate Number[] ElTimePickerDisabledMinutesCallback(Number hour, string role, Dayjs? comparingDate = null);

[ECMAScript]
[Description("@#")]
public delegate Number[] ElTimePickerDisabledSecondsCallback(Number hour, Number minute, string role, Dayjs? comparingDate = null);

[ECMAScript]
[Description("@#TableOverflowTooltipData")]
public sealed record ElTableTooltipFormatterContext : VueProps
{
    [Description("@#row")]
    public VueDictionary? Row { get; init; }

    [Description("@#column")]
    public ElTableColumnContext? Column { get; init; }

    [Description("@#cellValue")]
    public VueValue? CellValue { get; init; }
}

[ECMAScript]
[Description("@#")]
public delegate VueStringNumberVNodeValue ElTableTooltipFormatter(ElTableTooltipFormatterContext data);

[ECMAScript]
[Description("@#TableColumnCtx")]
public sealed record ElTableColumnContext : VueProps
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
public sealed record ElTableRowContext : VueProps
{
    [Description("@#row")]
    public VueDictionary? Row { get; init; }

    [Description("@#rowIndex")]
    public Number? RowIndex { get; init; }
}

[ECMAScript]
[Description("@#TableCellContext")]
public sealed record ElTableCellContext : VueProps
{
    [Description("@#row")]
    public VueDictionary? Row { get; init; }

    [Description("@#rowIndex")]
    public Number? RowIndex { get; init; }

    [Description("@#column")]
    public ElTableColumnContext? Column { get; init; }

    [Description("@#columnIndex")]
    public Number? ColumnIndex { get; init; }
}

[ECMAScript]
[Description("@#SummaryMethodContext")]
public sealed record ElTableSummaryMethodContext : VueProps
{
    [Description("@#columns")]
    public ElTableColumnContext[]? Columns { get; init; }

    [Description("@#data")]
    public VueDictionary[]? Data { get; init; }
}

[ECMAScript]
[Description("@#")]
public readonly union ElTableSpanMethodResult(Number[], ElTableSpanMethodCoordinates)
{
    public Number[]? AsPair => Value as Number[];

    public ElTableSpanMethodCoordinates? AsCoordinates => Value as ElTableSpanMethodCoordinates;
}

[ECMAScript]
[Description("@#")]
public sealed record ElTableSpanMethodCoordinates : VueProps
{
    [Description("@#rowspan")]
    public Number? Rowspan { get; init; }

    [Description("@#colspan")]
    public Number? Colspan { get; init; }
}

[ECMAScript]
[Description("@#")]
public delegate string ElTableRowClassNameCallback(ElTableRowContext context);

[ECMAScript]
[Description("@#")]
public delegate VueStyleValue ElTableRowStyleCallback(ElTableRowContext context);

[ECMAScript]
[Description("@#")]
public delegate string ElTableCellClassNameCallback(ElTableCellContext context);

[ECMAScript]
[Description("@#")]
public delegate VueStyleValue ElTableCellStyleCallback(ElTableCellContext context);

[ECMAScript]
[Description("@#")]
public readonly union ElTableRowClassNameValue(string, ElTableRowClassNameCallback)
{
    public string? AsString => Value as string;

    public ElTableRowClassNameCallback? AsCallback => Value as ElTableRowClassNameCallback;
}

[ECMAScript]
[Description("@#")]
public readonly union ElTableRowStyleValue(VueStyleValue, ElTableRowStyleCallback)
{
    public VueStyleValue? AsStyle => Value is VueStyleValue value ? value : default(VueStyleValue?);

    public ElTableRowStyleCallback? AsCallback => Value as ElTableRowStyleCallback;
}

[ECMAScript]
[Description("@#")]
public readonly union ElTableCellClassNameValue(string, ElTableCellClassNameCallback)
{
    public string? AsString => Value as string;

    public ElTableCellClassNameCallback? AsCallback => Value as ElTableCellClassNameCallback;
}

[ECMAScript]
[Description("@#")]
public readonly union ElTableCellStyleValue(VueStyleValue, ElTableCellStyleCallback)
{
    public VueStyleValue? AsStyle => Value is VueStyleValue value ? value : default(VueStyleValue?);

    public ElTableCellStyleCallback? AsCallback => Value as ElTableCellStyleCallback;
}

[ECMAScript]
[Description("@#")]
public delegate string ElTableRowKeyCallback(VueDictionary row);

[ECMAScript]
[Description("@#")]
public readonly union ElTableRowKeyValue(string, ElTableRowKeyCallback)
{
    public string? AsString => Value as string;

    public ElTableRowKeyCallback? AsCallback => Value as ElTableRowKeyCallback;
}

[ECMAScript]
[Description("@#")]
public delegate VueStringNumberVNodeValue[] ElTableSummaryMethodCallback(ElTableSummaryMethodContext context);

[ECMAScript]
[Description("@#")]
public delegate ElTableSpanMethodResult? ElTableSpanMethodCallback(ElTableCellContext context);

[ECMAScript]
[Description("@#TableTreeNode")]
public sealed record ElTableTreeNode : VueProps
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
public delegate void ElTableResolveChildrenCallback(VueDictionary[] data);

[ECMAScript]
[Description("@#")]
public delegate void ElTableLoadCallback(VueDictionary row, ElTableTreeNode treeNode, ElTableResolveChildrenCallback resolve);

[ECMAScript]
[Description("@#")]
public delegate bool ElTableRowExpandableCallback(VueDictionary row, Number index);

[ECMAScript]
[Description("@#TableColumnHeaderContext")]
public sealed record ElTableColumnHeaderContext : VueProps
{
    [Description("@#column")]
    public ElTableColumnContext? Column { get; init; }

    [Description("@#$index")]
    public Number? Index { get; init; }

    [Description("@#store")]
    public VueDictionary? Store { get; init; }

    [Description("@#_self")]
    public VueValue? Self { get; init; }
}

[ECMAScript]
[Description("@#")]
public delegate Number ElTableColumnIndexCallback(Number index);

[ECMAScript]
[Description("@#")]
public readonly union ElTableColumnIndexValue(Number, ElTableColumnIndexCallback)
{
    public Number? AsNumber => Value is Number value ? value : default(Number?);

    public ElTableColumnIndexCallback? AsCallback => Value as ElTableColumnIndexCallback;
}

[ECMAScript]
[Description("@#")]
public delegate IVNode ElTableColumnRenderHeaderCallback(ElTableColumnHeaderContext context);

[ECMAScript]
[Description("@#")]
public delegate Number ElTableColumnSortMethodCallback(VueDictionary left, VueDictionary right);

[ECMAScript]
[Description("@#")]
public delegate string ElTableColumnSortByCallback(VueDictionary row, Number index, VueDictionary[]? array = null);

[ECMAScript]
[Description("@#")]
public readonly union ElTableColumnSortByValue(
    string,
    string[],
    ElTableColumnSortByCallback)
{
    public string? AsString => Value as string;

    public string[]? AsStrings => Value as string[];

    public ElTableColumnSortByCallback? AsCallback => Value as ElTableColumnSortByCallback;
}

[ECMAScript]
[Description("@#")]
public delegate VueStringNumberVNodeValue ElTableColumnFormatterCallback(
    VueDictionary row,
    ElTableColumnContext column,
    VueValue? cellValue,
    Number index);

[ECMAScript]
[Description("@#")]
public delegate bool ElTableColumnSelectableCallback(VueDictionary row, Number index);

[ECMAScript]
[Description("@#")]
public delegate void ElTableColumnFilterMethodCallback(string value, VueDictionary row, ElTableColumnContext column);

[ECMAScript]
[Description("@#SelectV2Option")]
public sealed record ElSelectV2Option : VueDictionary
{
    [Description("@#created")]
    public bool? Created { get; init; }
}

[ECMAScript]
[Description("@#OptionGroup")]
public sealed record ElSelectV2OptionGroup : VueDictionary
{
}

[ECMAScript]
[Description("@#")]
public readonly union ElSelectV2OptionValue(
    ElSelectV2Option,
    ElSelectV2OptionGroup)
{
    public ElSelectV2Option? AsOption => Value as ElSelectV2Option;

    public ElSelectV2OptionGroup? AsGroup => Value as ElSelectV2OptionGroup;
}

[ECMAScript]
[Description("@#")]
public readonly union ElSelectV2ModelValue(VueValue, VueValue[]) : IEnumerable<VueValue>
{
    public VueValue? AsSingle => Value as VueValue;

    public VueValue[]? AsMultiple => Value as VueValue[];

    IEnumerator<VueValue> IEnumerable<VueValue>.GetEnumerator()
        => ((IEnumerable<VueValue>)(AsMultiple ?? Array.Empty<VueValue>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VueValue>)this).GetEnumerator();
}
