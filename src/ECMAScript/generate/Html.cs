namespace ECMAScript;

/// <summary>
/// ShowPopoverOptions
/// </summary>
[ECMAScript]
[Description("@#ShowPopoverOptions")]
public record ShowPopoverOptions(
    [property: Description("@#source")]HTMLElement? Source = default);

/// <summary>
/// TogglePopoverOptions
/// </summary>
[ECMAScript]
[Description("@#TogglePopoverOptions")]
public record TogglePopoverOptions(
    [property: Description("@#force")]bool Force = default): ShowPopoverOptions;

/// <summary>
/// TrackEventInit
/// </summary>
[ECMAScript]
[Description("@#TrackEventInit")]
public record TrackEventInit(
    [property: Description("@#track")]Either<VideoTrack, AudioTrack, TextTrack>? Track = default): EventInit;

/// <summary>
/// SubmitEventInit
/// </summary>
[ECMAScript]
[Description("@#SubmitEventInit")]
public record SubmitEventInit(
    [property: Description("@#submitter")]HTMLElement? Submitter = null): EventInit;

/// <summary>
/// FormDataEventInit
/// </summary>
[ECMAScript]
[Description("@#FormDataEventInit")]
public record FormDataEventInit(
    [property: Description("@#formData")]FormData? FormData = default): EventInit;

/// <summary>
/// AssignedNodesOptions
/// </summary>
[ECMAScript]
[Description("@#AssignedNodesOptions")]
public record AssignedNodesOptions(
    [property: Description("@#flatten")]bool Flatten = false);

/// <summary>
/// CanvasRenderingContext2DSettings
/// </summary>
[ECMAScript]
[Description("@#CanvasRenderingContext2DSettings")]
public record CanvasRenderingContext2DSettings(
    [property: Description("@#alpha")]bool Alpha = true,
	[property: Description("@#desynchronized")]bool Desynchronized = false,
	[property: Description("@#colorSpace")]PredefinedColorSpace ColorSpace = PredefinedColorSpace.Srgb,
	[property: Description("@#colorType")]CanvasColorType ColorType = CanvasColorType.Unorm8,
	[property: Description("@#willReadFrequently")]bool WillReadFrequently = false);

/// <summary>
/// ImageBitmapRenderingContextSettings
/// </summary>
[ECMAScript]
[Description("@#ImageBitmapRenderingContextSettings")]
public record ImageBitmapRenderingContextSettings(
    [property: Description("@#alpha")]bool Alpha = true);

/// <summary>
/// ImageEncodeOptions
/// </summary>
[ECMAScript]
[Description("@#ImageEncodeOptions")]
public record ImageEncodeOptions(
    [property: Description("@#type")]string? Type = default,
	[property: Description("@#quality")]double Quality = default);

/// <summary>
/// ElementDefinitionOptions
/// </summary>
[ECMAScript]
[Description("@#ElementDefinitionOptions")]
public record ElementDefinitionOptions(
    [property: Description("@#extends")]string? Extends = default);

/// <summary>
/// ValidityStateFlags
/// </summary>
[ECMAScript]
[Description("@#ValidityStateFlags")]
public record ValidityStateFlags(
    [property: Description("@#valueMissing")]bool ValueMissing = false,
	[property: Description("@#typeMismatch")]bool TypeMismatch = false,
	[property: Description("@#patternMismatch")]bool PatternMismatch = false,
	[property: Description("@#tooLong")]bool TooLong = false,
	[property: Description("@#tooShort")]bool TooShort = false,
	[property: Description("@#rangeUnderflow")]bool RangeUnderflow = false,
	[property: Description("@#rangeOverflow")]bool RangeOverflow = false,
	[property: Description("@#stepMismatch")]bool StepMismatch = false,
	[property: Description("@#badInput")]bool BadInput = false,
	[property: Description("@#customError")]bool CustomError = false);

/// <summary>
/// ToggleEventInit
/// </summary>
[ECMAScript]
[Description("@#ToggleEventInit")]
public record ToggleEventInit(
    [property: Description("@#oldState")]string? OldState = default,
	[property: Description("@#newState")]string? NewState = default): EventInit;

/// <summary>
/// CommandEventInit
/// </summary>
[ECMAScript]
[Description("@#CommandEventInit")]
public record CommandEventInit(
    [property: Description("@#source")]Element? Source = null,
	[property: Description("@#command")]string? Command = default): EventInit;

/// <summary>
/// FocusOptions
/// </summary>
[ECMAScript]
[Description("@#FocusOptions")]
public record FocusOptions(
    [property: Description("@#preventScroll")]bool PreventScroll = false,
	[property: Description("@#focusVisible")]bool FocusVisible = default);

/// <summary>
/// CloseWatcherOptions
/// </summary>
[ECMAScript]
[Description("@#CloseWatcherOptions")]
public record CloseWatcherOptions(
    [property: Description("@#signal")]AbortSignal? Signal = default);

/// <summary>
/// DragEventInit
/// </summary>
[ECMAScript]
[Description("@#DragEventInit")]
public record DragEventInit(
    [property: Description("@#dataTransfer")]DataTransfer? DataTransfer = null): MouseEventInit;

/// <summary>
/// WindowPostMessageOptions
/// </summary>
[ECMAScript]
[Description("@#WindowPostMessageOptions")]
public record WindowPostMessageOptions(
    [property: Description("@#targetOrigin")]string? TargetOrigin = default): StructuredSerializeOptions;

/// <summary>
/// NavigationUpdateCurrentEntryOptions
/// </summary>
[ECMAScript]
[Description("@#NavigationUpdateCurrentEntryOptions")]
public record NavigationUpdateCurrentEntryOptions(
    [property: Description("@#state")]object? State = default);

/// <summary>
/// NavigationOptions
/// </summary>
[ECMAScript]
[Description("@#NavigationOptions")]
public record NavigationOptions(
    [property: Description("@#info")]object? Info = default);

/// <summary>
/// NavigationNavigateOptions
/// </summary>
[ECMAScript]
[Description("@#NavigationNavigateOptions")]
public record NavigationNavigateOptions(
    [property: Description("@#state")]object? State = default,
	[property: Description("@#history")]NavigationHistoryBehavior History = NavigationHistoryBehavior.Auto): NavigationOptions;

/// <summary>
/// NavigationReloadOptions
/// </summary>
[ECMAScript]
[Description("@#NavigationReloadOptions")]
public record NavigationReloadOptions(
    [property: Description("@#state")]object? State = default): NavigationOptions;

/// <summary>
/// NavigationResult
/// </summary>
[ECMAScript]
[Description("@#NavigationResult")]
public record NavigationResult(
    [property: Description("@#committed")]PromiseResult<NavigationHistoryEntry>? Committed = default,
	[property: Description("@#finished")]PromiseResult<NavigationHistoryEntry>? Finished = default);

/// <summary>
/// NavigateEventInit
/// </summary>
[ECMAScript]
[Description("@#NavigateEventInit")]
public record NavigateEventInit(
    [property: Description("@#navigationType")]NavigationType NavigationType = NavigationType.Push,
	[property: Description("@#destination")]NavigationDestination? Destination = default,
	[property: Description("@#canIntercept")]bool CanIntercept = false,
	[property: Description("@#userInitiated")]bool UserInitiated = false,
	[property: Description("@#hashChange")]bool HashChange = false,
	[property: Description("@#signal")]AbortSignal? Signal = default,
	[property: Description("@#formData")]FormData? FormData = null,
	[property: Description("@#downloadRequest")]string? DownloadRequest = null,
	[property: Description("@#info")]object? Info = default,
	[property: Description("@#hasUAVisualTransition")]bool HasUAVisualTransition = false,
	[property: Description("@#sourceElement")]Element? SourceElement = null): EventInit;

/// <summary>
/// NavigationInterceptOptions
/// </summary>
[ECMAScript]
[Description("@#NavigationInterceptOptions")]
public record NavigationInterceptOptions(
    [property: Description("@#handler")]NavigationInterceptHandler? Handler = default,
	[property: Description("@#focusReset")]NavigationFocusReset? FocusReset = default,
	[property: Description("@#scroll")]NavigationScrollBehavior? Scroll = default);

/// <summary>
/// NavigationCurrentEntryChangeEventInit
/// </summary>
[ECMAScript]
[Description("@#NavigationCurrentEntryChangeEventInit")]
public record NavigationCurrentEntryChangeEventInit(
    [property: Description("@#navigationType")]NavigationType? NavigationType = null,
	[property: Description("@#from")]NavigationHistoryEntry? From = default): EventInit;

/// <summary>
/// PopStateEventInit
/// </summary>
[ECMAScript]
[Description("@#PopStateEventInit")]
public record PopStateEventInit(
    [property: Description("@#state")]object? State = default,
	[property: Description("@#hasUAVisualTransition")]bool HasUAVisualTransition = false): EventInit;

/// <summary>
/// HashChangeEventInit
/// </summary>
[ECMAScript]
[Description("@#HashChangeEventInit")]
public record HashChangeEventInit(
    [property: Description("@#oldURL")]string? OldURL = default,
	[property: Description("@#newURL")]string? NewURL = default): EventInit;

/// <summary>
/// PageSwapEventInit
/// </summary>
[ECMAScript]
[Description("@#PageSwapEventInit")]
public record PageSwapEventInit(
    [property: Description("@#activation")]NavigationActivation? Activation = null,
	[property: Description("@#viewTransition")]ViewTransition? ViewTransition = null): EventInit;

/// <summary>
/// PageRevealEventInit
/// </summary>
[ECMAScript]
[Description("@#PageRevealEventInit")]
public record PageRevealEventInit(
    [property: Description("@#viewTransition")]ViewTransition? ViewTransition = null): EventInit;

/// <summary>
/// PageTransitionEventInit
/// </summary>
[ECMAScript]
[Description("@#PageTransitionEventInit")]
public record PageTransitionEventInit(
    [property: Description("@#persisted")]bool Persisted = false): EventInit;

/// <summary>
/// ErrorEventInit
/// </summary>
[ECMAScript]
[Description("@#ErrorEventInit")]
public record ErrorEventInit(
    [property: Description("@#message")]string? Message = default,
	[property: Description("@#filename")]string? Filename = default,
	[property: Description("@#lineno")]uint Lineno = 0,
	[property: Description("@#colno")]uint Colno = 0,
	[property: Description("@#error")]object? Error = default): EventInit;

/// <summary>
/// PromiseRejectionEventInit
/// </summary>
[ECMAScript]
[Description("@#PromiseRejectionEventInit")]
public record PromiseRejectionEventInit(
    [property: Description("@#promise")]object? Promise = default,
	[property: Description("@#reason")]object? Reason = default): EventInit;

/// <summary>
/// GetHTMLOptions
/// </summary>
[ECMAScript]
[Description("@#GetHTMLOptions")]
public record GetHTMLOptions(
    [property: Description("@#serializableShadowRoots")]bool SerializableShadowRoots = false,
	[property: Description("@#shadowRoots")]ShadowRoot[]? ShadowRoots = default);

/// <summary>
/// ImageDataSettings
/// </summary>
[ECMAScript]
[Description("@#ImageDataSettings")]
public record ImageDataSettings(
    [property: Description("@#colorSpace")]PredefinedColorSpace? ColorSpace = default,
	[property: Description("@#pixelFormat")]ImageDataPixelFormat PixelFormat = ImageDataPixelFormat.RgbaUnorm8);

/// <summary>
/// ImageBitmapOptions
/// </summary>
[ECMAScript]
[Description("@#ImageBitmapOptions")]
public record ImageBitmapOptions(
    [property: Description("@#imageOrientation")]ImageOrientation ImageOrientation = ImageOrientation.FromImage,
	[property: Description("@#premultiplyAlpha")]PremultiplyAlpha PremultiplyAlpha = PremultiplyAlpha.Default,
	[property: Description("@#colorSpaceConversion")]ColorSpaceConversion ColorSpaceConversion = ColorSpaceConversion.Default,
	[property: Description("@#resizeWidth")]uint ResizeWidth = default,
	[property: Description("@#resizeHeight")]uint ResizeHeight = default,
	[property: Description("@#resizeQuality")]ResizeQuality ResizeQuality = ResizeQuality.Low);

/// <summary>
/// MessageEventInit
/// </summary>
[ECMAScript]
[Description("@#MessageEventInit")]
public record MessageEventInit(
    [property: Description("@#data")]object? Data = default,
	[property: Description("@#origin")]string? Origin = default,
	[property: Description("@#lastEventId")]string? LastEventId = default,
	[property: Description("@#source")]MessageEventSource? Source = null,
	[property: Description("@#ports")]MessagePort[]? Ports = default): EventInit;

/// <summary>
/// EventSourceInit
/// </summary>
[ECMAScript]
[Description("@#EventSourceInit")]
public record EventSourceInit(
    [property: Description("@#withCredentials")]bool WithCredentials = false);

/// <summary>
/// StructuredSerializeOptions
/// </summary>
[ECMAScript]
[Description("@#StructuredSerializeOptions")]
public record StructuredSerializeOptions(
    [property: Description("@#transfer")]object[]? Transfer = default);

/// <summary>
/// WorkerOptions
/// </summary>
[ECMAScript]
[Description("@#WorkerOptions")]
public record WorkerOptions(
    [property: Description("@#type")]WorkerType Type = WorkerType.Classic,
	[property: Description("@#credentials")]RequestCredentials Credentials = RequestCredentials.SameOrigin,
	[property: Description("@#name")]string? Name = default);

/// <summary>
/// WorkletOptions
/// </summary>
[ECMAScript]
[Description("@#WorkletOptions")]
public record WorkletOptions(
    [property: Description("@#credentials")]RequestCredentials Credentials = RequestCredentials.SameOrigin);

/// <summary>
/// StorageEventInit
/// </summary>
[ECMAScript]
[Description("@#StorageEventInit")]
public record StorageEventInit(
    [property: Description("@#key")]string? Key = null,
	[property: Description("@#oldValue")]string? OldValue = null,
	[property: Description("@#newValue")]string? NewValue = null,
	[property: Description("@#url")]string? Url = default,
	[property: Description("@#storageArea")]Storage? StorageArea = null): EventInit;

/// <summary>
/// HTMLAllCollection
/// </summary>
[ECMAScript]
[Description("@#HTMLAllCollection")]
public class HTMLAllCollection
{
    /// <summary>
    /// length
    /// </summary>
    [Description("@#length")]
    public extern uint Length { get; }

    [Description("@#")]
    public extern Element this[uint index] { get; }

    /// <summary>
    /// namedItem
    /// </summary>
    /// <param name="name">name</param>
    [Description("@#namedItem")]
    public extern Either<HTMLCollection, Element>? NamedItem(string name);

    /// <summary>
    /// item
    /// </summary>
    /// <param name="nameOrIndex">nameOrIndex</param>
    [Description("@#item")]
    public extern Either<HTMLCollection, Element>? GetItem(string nameOrIndex);
}

/// <summary>
/// HTMLFormControlsCollection
/// </summary>
[ECMAScript]
[Description("@#HTMLFormControlsCollection")]
public class HTMLFormControlsCollection : HTMLCollection
{
    /// <summary>
    /// namedItem
    /// </summary>
    /// <param name="name">name</param>
    [Description("@#namedItem")]
    public extern new Either<RadioNodeList, Element>? NamedItem(string name);
}

/// <summary>
/// RadioNodeList
/// </summary>
[ECMAScript]
[Description("@#RadioNodeList")]
public class RadioNodeList : NodeList
{
    /// <summary>
    /// value
    /// </summary>
    [Description("@#value")]
    public extern string Value { get; set; }
}

/// <summary>
/// HTMLOptionsCollection
/// </summary>
[ECMAScript]
[Description("@#HTMLOptionsCollection")]
public class HTMLOptionsCollection : HTMLCollection
{
    [Description("@#")]
    public extern HTMLOptionElement? this[uint index] { set; }

    /// <summary>
    /// add
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="before">before</param>
    [Description("@#add")]
    public extern void Add(Either<HTMLOptionElement, HTMLOptGroupElement> element, Either<HTMLElement, int>? before);

    /// <summary>
    /// add
    /// </summary>
    /// <param name="element">element para</param>
    /// <param name="before">before</param>
    [Description("@#add")]
    public extern void Add(Either<HTMLOptionElement, HTMLOptGroupElement> element, HTMLElement before);

    /// <summary>
    /// add
    /// </summary>
    /// <param name="element">element para</param>
    /// <param name="before">before</param>
    [Description("@#add")]
    public extern void Add(Either<HTMLOptionElement, HTMLOptGroupElement> element, int before);

    /// <summary>
    /// remove
    /// </summary>
    /// <param name="index">index</param>
    [Description("@#remove")]
    public extern void Remove(int index);

    /// <summary>
    /// selectedIndex
    /// </summary>
    [Description("@#selectedIndex")]
    public extern int SelectedIndex { get; set; }
}

/// <summary>
/// DOMStringList
/// </summary>
[ECMAScript]
[Description("@#DOMStringList")]
public class DOMStringList
{
    /// <summary>
    /// length
    /// </summary>
    [Description("@#length")]
    public extern uint Length { get; }

    /// <summary>
    /// item
    /// </summary>
    /// <param name="index">index</param>
    [Description("@#item")]
    public extern string? GetItem(uint index);

    /// <summary>
    /// contains
    /// </summary>
    /// <param name="string">string</param>
    [Description("@#contains")]
    public extern bool Contains(string @string);
}

/// <summary>
/// HTMLUnknownElement
/// </summary>
[ECMAScript]
[Description("@#HTMLUnknownElement")]
public class HTMLUnknownElement : HTMLElement
{

}

/// <summary>
/// DOMStringMap
/// </summary>
[ECMAScript]
[Description("@#DOMStringMap")]
public class DOMStringMap
{
    [Description("@#")]
    public extern string this[string name] { get; set; }

    [Description("@#")]
    [Category("deleter")]
    public extern void Delete(string name);
}

/// <summary>
/// HTMLHtmlElement
/// </summary>
[ECMAScript]
[Description("@#HTMLHtmlElement")]
public partial class HTMLHtmlElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLHtmlElement();

    /// <summary>
    /// version
    /// </summary>
    [Description("@#version")]
    public extern string Version { get; set; }
}

/// <summary>
/// HTMLHeadElement
/// </summary>
[ECMAScript]
[Description("@#HTMLHeadElement")]
public class HTMLHeadElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLHeadElement();
}

/// <summary>
/// HTMLTitleElement
/// </summary>
[ECMAScript]
[Description("@#HTMLTitleElement")]
public class HTMLTitleElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLTitleElement();

    /// <summary>
    /// text
    /// </summary>
    [Description("@#text")]
    public extern string Text { get; set; }
}

/// <summary>
/// HTMLBaseElement
/// </summary>
[ECMAScript]
[Description("@#HTMLBaseElement")]
public class HTMLBaseElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLBaseElement();

    /// <summary>
    /// href
    /// </summary>
    [Description("@#href")]
    public extern string Href { get; set; }

    /// <summary>
    /// target
    /// </summary>
    [Description("@#target")]
    public extern string Target { get; set; }
}

/// <summary>
/// HTMLLinkElement
/// </summary>
[ECMAScript]
[Description("@#HTMLLinkElement")]
public partial class HTMLLinkElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLLinkElement();

    /// <summary>
    /// href
    /// </summary>
    [Description("@#href")]
    public extern string Href { get; set; }

    /// <summary>
    /// crossOrigin
    /// </summary>
    [Description("@#crossOrigin")]
    public extern string? CrossOrigin { get; set; }

    /// <summary>
    /// rel
    /// </summary>
    [Description("@#rel")]
    public extern string Rel { get; set; }

    /// <summary>
    /// as
    /// </summary>
    [Description("@#as")]
    public extern string As { get; set; }

    /// <summary>
    /// relList
    /// </summary>
    [Description("@#relList")]
    public extern List<string> RelList { get; }

    /// <summary>
    /// media
    /// </summary>
    [Description("@#media")]
    public extern string Media { get; set; }

    /// <summary>
    /// integrity
    /// </summary>
    [Description("@#integrity")]
    public extern string Integrity { get; set; }

    /// <summary>
    /// hreflang
    /// </summary>
    [Description("@#hreflang")]
    public extern string Hreflang { get; set; }

    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern string Type { get; set; }

    /// <summary>
    /// sizes
    /// </summary>
    [Description("@#sizes")]
    public extern List<string> Sizes { get; }

    /// <summary>
    /// imageSrcset
    /// </summary>
    [Description("@#imageSrcset")]
    public extern string ImageSrcset { get; set; }

    /// <summary>
    /// imageSizes
    /// </summary>
    [Description("@#imageSizes")]
    public extern string ImageSizes { get; set; }

    /// <summary>
    /// referrerPolicy
    /// </summary>
    [Description("@#referrerPolicy")]
    public extern string ReferrerPolicy { get; set; }

    /// <summary>
    /// blocking
    /// </summary>
    [Description("@#blocking")]
    public extern List<string> Blocking { get; }

    /// <summary>
    /// disabled
    /// </summary>
    [Description("@#disabled")]
    public extern bool Disabled { get; set; }

    /// <summary>
    /// fetchPriority
    /// </summary>
    [Description("@#fetchPriority")]
    public extern string FetchPriority { get; set; }

    /// <summary>
    /// charset
    /// </summary>
    [Description("@#charset")]
    public extern string Charset { get; set; }

    /// <summary>
    /// rev
    /// </summary>
    [Description("@#rev")]
    public extern string Rev { get; set; }

    /// <summary>
    /// target
    /// </summary>
    [Description("@#target")]
    public extern string Target { get; set; }

    #region mixin LinkStyle
    /// <summary>
    /// sheet
    /// </summary>
    [Description("@#sheet")]
    public extern CSSStyleSheet? Sheet { get; }
    #endregion
}

/// <summary>
/// HTMLMetaElement
/// </summary>
[ECMAScript]
[Description("@#HTMLMetaElement")]
public partial class HTMLMetaElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLMetaElement();

    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; set; }

    /// <summary>
    /// httpEquiv
    /// </summary>
    [Description("@#httpEquiv")]
    public extern string HttpEquiv { get; set; }

    /// <summary>
    /// content
    /// </summary>
    [Description("@#content")]
    public extern string Content { get; set; }

    /// <summary>
    /// media
    /// </summary>
    [Description("@#media")]
    public extern string Media { get; set; }

    /// <summary>
    /// scheme
    /// </summary>
    [Description("@#scheme")]
    public extern string Scheme { get; set; }
}

/// <summary>
/// HTMLStyleElement
/// </summary>
[ECMAScript]
[Description("@#HTMLStyleElement")]
public partial class HTMLStyleElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLStyleElement();

    /// <summary>
    /// disabled
    /// </summary>
    [Description("@#disabled")]
    public extern bool Disabled { get; set; }

    /// <summary>
    /// media
    /// </summary>
    [Description("@#media")]
    public extern string Media { get; set; }

    /// <summary>
    /// blocking
    /// </summary>
    [Description("@#blocking")]
    public extern List<string> Blocking { get; }

    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern string Type { get; set; }

    #region mixin LinkStyle
    /// <summary>
    /// sheet
    /// </summary>
    [Description("@#sheet")]
    public extern CSSStyleSheet? Sheet { get; }
    #endregion
}

/// <summary>
/// HTMLHeadingElement
/// </summary>
[ECMAScript]
[Description("@#HTMLHeadingElement")]
public partial class HTMLHeadingElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLHeadingElement();

    /// <summary>
    /// align
    /// </summary>
    [Description("@#align")]
    public extern string Align { get; set; }
}

/// <summary>
/// HTMLParagraphElement
/// </summary>
[ECMAScript]
[Description("@#HTMLParagraphElement")]
public partial class HTMLParagraphElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLParagraphElement();

    /// <summary>
    /// align
    /// </summary>
    [Description("@#align")]
    public extern string Align { get; set; }
}

/// <summary>
/// HTMLHRElement
/// </summary>
[ECMAScript]
[Description("@#HTMLHRElement")]
public partial class HTMLHRElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLHRElement();

    /// <summary>
    /// align
    /// </summary>
    [Description("@#align")]
    public extern string Align { get; set; }

    /// <summary>
    /// color
    /// </summary>
    [Description("@#color")]
    public extern string Color { get; set; }

    /// <summary>
    /// noShade
    /// </summary>
    [Description("@#noShade")]
    public extern bool NoShade { get; set; }

    /// <summary>
    /// size
    /// </summary>
    [Description("@#size")]
    public extern string Size { get; set; }

    /// <summary>
    /// width
    /// </summary>
    [Description("@#width")]
    public extern string Width { get; set; }
}

/// <summary>
/// HTMLPreElement
/// </summary>
[ECMAScript]
[Description("@#HTMLPreElement")]
public partial class HTMLPreElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLPreElement();

    /// <summary>
    /// width
    /// </summary>
    [Description("@#width")]
    public extern int Width { get; set; }
}

/// <summary>
/// HTMLQuoteElement
/// </summary>
[ECMAScript]
[Description("@#HTMLQuoteElement")]
public class HTMLQuoteElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLQuoteElement();

    /// <summary>
    /// cite
    /// </summary>
    [Description("@#cite")]
    public extern string Cite { get; set; }
}

/// <summary>
/// HTMLOListElement
/// </summary>
[ECMAScript]
[Description("@#HTMLOListElement")]
public partial class HTMLOListElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLOListElement();

    /// <summary>
    /// reversed
    /// </summary>
    [Description("@#reversed")]
    public extern bool Reversed { get; set; }

    /// <summary>
    /// start
    /// </summary>
    [Description("@#start")]
    public extern int Start { get; set; }

    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern string Type { get; set; }

    /// <summary>
    /// compact
    /// </summary>
    [Description("@#compact")]
    public extern bool Compact { get; set; }
}

/// <summary>
/// HTMLUListElement
/// </summary>
[ECMAScript]
[Description("@#HTMLUListElement")]
public partial class HTMLUListElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLUListElement();

    /// <summary>
    /// compact
    /// </summary>
    [Description("@#compact")]
    public extern bool Compact { get; set; }

    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern string Type { get; set; }
}

/// <summary>
/// HTMLMenuElement
/// </summary>
[ECMAScript]
[Description("@#HTMLMenuElement")]
public partial class HTMLMenuElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLMenuElement();

    /// <summary>
    /// compact
    /// </summary>
    [Description("@#compact")]
    public extern bool Compact { get; set; }
}

/// <summary>
/// HTMLLIElement
/// </summary>
[ECMAScript]
[Description("@#HTMLLIElement")]
public partial class HTMLLIElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLLIElement();

    /// <summary>
    /// value
    /// </summary>
    [Description("@#value")]
    public extern int Value { get; set; }

    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern string Type { get; set; }
}

/// <summary>
/// HTMLDListElement
/// </summary>
[ECMAScript]
[Description("@#HTMLDListElement")]
public partial class HTMLDListElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLDListElement();

    /// <summary>
    /// compact
    /// </summary>
    [Description("@#compact")]
    public extern bool Compact { get; set; }
}

/// <summary>
/// HTMLDivElement
/// </summary>
[ECMAScript]
[Description("@#HTMLDivElement")]
public partial class HTMLDivElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLDivElement();

    /// <summary>
    /// align
    /// </summary>
    [Description("@#align")]
    public extern string Align { get; set; }
}

/// <summary>
/// HTMLAnchorElement
/// </summary>
[ECMAScript]
[Description("@#HTMLAnchorElement")]
public partial class HTMLAnchorElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLAnchorElement();

    /// <summary>
    /// target
    /// </summary>
    [Description("@#target")]
    public extern string Target { get; set; }

    /// <summary>
    /// download
    /// </summary>
    [Description("@#download")]
    public extern string Download { get; set; }

    /// <summary>
    /// ping
    /// </summary>
    [Description("@#ping")]
    public extern string Ping { get; set; }

    /// <summary>
    /// rel
    /// </summary>
    [Description("@#rel")]
    public extern string Rel { get; set; }

    /// <summary>
    /// relList
    /// </summary>
    [Description("@#relList")]
    public extern List<string> RelList { get; }

    /// <summary>
    /// hreflang
    /// </summary>
    [Description("@#hreflang")]
    public extern string Hreflang { get; set; }

    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern string Type { get; set; }

    /// <summary>
    /// text
    /// </summary>
    [Description("@#text")]
    public extern string Text { get; set; }

    /// <summary>
    /// referrerPolicy
    /// </summary>
    [Description("@#referrerPolicy")]
    public extern string ReferrerPolicy { get; set; }

    /// <summary>
    /// coords
    /// </summary>
    [Description("@#coords")]
    public extern string Coords { get; set; }

    /// <summary>
    /// charset
    /// </summary>
    [Description("@#charset")]
    public extern string Charset { get; set; }

    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; set; }

    /// <summary>
    /// rev
    /// </summary>
    [Description("@#rev")]
    public extern string Rev { get; set; }

    /// <summary>
    /// shape
    /// </summary>
    [Description("@#shape")]
    public extern string Shape { get; set; }

    /// <summary>
    /// attributionSourceId
    /// </summary>
    [Description("@#attributionSourceId")]
    public extern uint AttributionSourceId { get; set; }

    #region mixin HTMLAttributionSrcElementUtils
    /// <summary>
    /// attributionSrc
    /// </summary>
    [Description("@#attributionSrc")]
    public extern string AttributionSrc { get; set; }
    #endregion

    #region mixin HTMLHyperlinkElementUtils
    /// <summary>
    /// href
    /// </summary>
    [Description("@#href")]
    public extern string Href { get; set; }

    /// <summary>
    /// origin
    /// </summary>
    [Description("@#origin")]
    public extern string Origin { get; }

    /// <summary>
    /// protocol
    /// </summary>
    [Description("@#protocol")]
    public extern string Protocol { get; set; }

    /// <summary>
    /// username
    /// </summary>
    [Description("@#username")]
    public extern string Username { get; set; }

    /// <summary>
    /// password
    /// </summary>
    [Description("@#password")]
    public extern string Password { get; set; }

    /// <summary>
    /// host
    /// </summary>
    [Description("@#host")]
    public extern string Host { get; set; }

    /// <summary>
    /// hostname
    /// </summary>
    [Description("@#hostname")]
    public extern string Hostname { get; set; }

    /// <summary>
    /// port
    /// </summary>
    [Description("@#port")]
    public extern string Port { get; set; }

    /// <summary>
    /// pathname
    /// </summary>
    [Description("@#pathname")]
    public extern string Pathname { get; set; }

    /// <summary>
    /// search
    /// </summary>
    [Description("@#search")]
    public extern string Search { get; set; }

    /// <summary>
    /// hash
    /// </summary>
    [Description("@#hash")]
    public extern string Hash { get; set; }
    #endregion
}

/// <summary>
/// HTMLDataElement
/// </summary>
[ECMAScript]
[Description("@#HTMLDataElement")]
public class HTMLDataElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLDataElement();

    /// <summary>
    /// value
    /// </summary>
    [Description("@#value")]
    public extern string Value { get; set; }
}

/// <summary>
/// HTMLTimeElement
/// </summary>
[ECMAScript]
[Description("@#HTMLTimeElement")]
public class HTMLTimeElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLTimeElement();

    /// <summary>
    /// dateTime
    /// </summary>
    [Description("@#dateTime")]
    public extern string DateTime { get; set; }
}

/// <summary>
/// HTMLSpanElement
/// </summary>
[ECMAScript]
[Description("@#HTMLSpanElement")]
public class HTMLSpanElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLSpanElement();
}

/// <summary>
/// HTMLBRElement
/// </summary>
[ECMAScript]
[Description("@#HTMLBRElement")]
public partial class HTMLBRElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLBRElement();

    /// <summary>
    /// clear
    /// </summary>
    [Description("@#clear")]
    public extern string Clear { get; set; }
}

/// <summary>
/// HTMLModElement
/// </summary>
[ECMAScript]
[Description("@#HTMLModElement")]
public class HTMLModElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLModElement();

    /// <summary>
    /// cite
    /// </summary>
    [Description("@#cite")]
    public extern string Cite { get; set; }

    /// <summary>
    /// dateTime
    /// </summary>
    [Description("@#dateTime")]
    public extern string DateTime { get; set; }
}

/// <summary>
/// HTMLPictureElement
/// </summary>
[ECMAScript]
[Description("@#HTMLPictureElement")]
public class HTMLPictureElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLPictureElement();
}

/// <summary>
/// HTMLSourceElement
/// </summary>
[ECMAScript]
[Description("@#HTMLSourceElement")]
public class HTMLSourceElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLSourceElement();

    /// <summary>
    /// src
    /// </summary>
    [Description("@#src")]
    public extern string Src { get; set; }

    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern string Type { get; set; }

    /// <summary>
    /// srcset
    /// </summary>
    [Description("@#srcset")]
    public extern string Srcset { get; set; }

    /// <summary>
    /// sizes
    /// </summary>
    [Description("@#sizes")]
    public extern string Sizes { get; set; }

    /// <summary>
    /// media
    /// </summary>
    [Description("@#media")]
    public extern string Media { get; set; }

    /// <summary>
    /// width
    /// </summary>
    [Description("@#width")]
    public extern uint Width { get; set; }

    /// <summary>
    /// height
    /// </summary>
    [Description("@#height")]
    public extern uint Height { get; set; }
}

/// <summary>
/// HTMLEmbedElement
/// </summary>
[ECMAScript]
[Description("@#HTMLEmbedElement")]
public partial class HTMLEmbedElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLEmbedElement();

    /// <summary>
    /// src
    /// </summary>
    [Description("@#src")]
    public extern string Src { get; set; }

    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern string Type { get; set; }

    /// <summary>
    /// width
    /// </summary>
    [Description("@#width")]
    public extern string Width { get; set; }

    /// <summary>
    /// height
    /// </summary>
    [Description("@#height")]
    public extern string Height { get; set; }

    /// <summary>
    /// getSVGDocument
    /// </summary>
    [Description("@#getSVGDocument")]
    public extern Document? GetSVGDocument();

    /// <summary>
    /// align
    /// </summary>
    [Description("@#align")]
    public extern string Align { get; set; }

    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; set; }
}

/// <summary>
/// HTMLObjectElement
/// </summary>
[ECMAScript]
[Description("@#HTMLObjectElement")]
public partial class HTMLObjectElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLObjectElement();

    /// <summary>
    /// data
    /// </summary>
    [Description("@#data")]
    public extern string Data { get; set; }

    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern string Type { get; set; }

    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; set; }

    /// <summary>
    /// form
    /// </summary>
    [Description("@#form")]
    public extern HTMLFormElement? Form { get; }

    /// <summary>
    /// width
    /// </summary>
    [Description("@#width")]
    public extern string Width { get; set; }

    /// <summary>
    /// height
    /// </summary>
    [Description("@#height")]
    public extern string Height { get; set; }

    /// <summary>
    /// contentDocument
    /// </summary>
    [Description("@#contentDocument")]
    public extern Document? ContentDocument { get; }

    /// <summary>
    /// contentWindow
    /// </summary>
    [Description("@#contentWindow")]
    public extern WindowProxy? ContentWindow { get; }

    /// <summary>
    /// getSVGDocument
    /// </summary>
    [Description("@#getSVGDocument")]
    public extern Document? GetSVGDocument();

    /// <summary>
    /// willValidate
    /// </summary>
    [Description("@#willValidate")]
    public extern bool WillValidate { get; }

    /// <summary>
    /// validity
    /// </summary>
    [Description("@#validity")]
    public extern ValidityState Validity { get; }

    /// <summary>
    /// validationMessage
    /// </summary>
    [Description("@#validationMessage")]
    public extern string ValidationMessage { get; }

    /// <summary>
    /// checkValidity
    /// </summary>
    [Description("@#checkValidity")]
    public extern bool CheckValidity();

    /// <summary>
    /// reportValidity
    /// </summary>
    [Description("@#reportValidity")]
    public extern bool ReportValidity();

    /// <summary>
    /// setCustomValidity
    /// </summary>
    /// <param name="error">error</param>
    [Description("@#setCustomValidity")]
    public extern void SetCustomValidity(string error);

    /// <summary>
    /// align
    /// </summary>
    [Description("@#align")]
    public extern string Align { get; set; }

    /// <summary>
    /// archive
    /// </summary>
    [Description("@#archive")]
    public extern string Archive { get; set; }

    /// <summary>
    /// code
    /// </summary>
    [Description("@#code")]
    public extern string Code { get; set; }

    /// <summary>
    /// declare
    /// </summary>
    [Description("@#declare")]
    public extern bool Declare { get; set; }

    /// <summary>
    /// hspace
    /// </summary>
    [Description("@#hspace")]
    public extern uint Hspace { get; set; }

    /// <summary>
    /// standby
    /// </summary>
    [Description("@#standby")]
    public extern string Standby { get; set; }

    /// <summary>
    /// vspace
    /// </summary>
    [Description("@#vspace")]
    public extern uint Vspace { get; set; }

    /// <summary>
    /// codeBase
    /// </summary>
    [Description("@#codeBase")]
    public extern string CodeBase { get; set; }

    /// <summary>
    /// codeType
    /// </summary>
    [Description("@#codeType")]
    public extern string CodeType { get; set; }

    /// <summary>
    /// useMap
    /// </summary>
    [Description("@#useMap")]
    public extern string UseMap { get; set; }

    /// <summary>
    /// border
    /// </summary>
    [Description("@#border")]
    public extern string Border { get; set; }
}

/// <summary>
/// HTMLVideoElement
/// </summary>
[ECMAScript]
[Description("@#HTMLVideoElement")]
public partial class HTMLVideoElement : HTMLMediaElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLVideoElement();

    /// <summary>
    /// width
    /// </summary>
    [Description("@#width")]
    public extern uint Width { get; set; }

    /// <summary>
    /// height
    /// </summary>
    [Description("@#height")]
    public extern uint Height { get; set; }

    /// <summary>
    /// videoWidth
    /// </summary>
    [Description("@#videoWidth")]
    public extern uint VideoWidth { get; }

    /// <summary>
    /// videoHeight
    /// </summary>
    [Description("@#videoHeight")]
    public extern uint VideoHeight { get; }

    /// <summary>
    /// poster
    /// </summary>
    [Description("@#poster")]
    public extern string Poster { get; set; }

    /// <summary>
    /// playsInline
    /// </summary>
    [Description("@#playsInline")]
    public extern bool PlaysInline { get; set; }

    /// <summary>
    /// getVideoPlaybackQuality
    /// </summary>
    [Description("@#getVideoPlaybackQuality")]
    public extern VideoPlaybackQuality GetVideoPlaybackQuality();

    /// <summary>
    /// requestPictureInPicture
    /// </summary>
    [Description("@#requestPictureInPicture")]
    public extern PromiseResult<PictureInPictureWindow> RequestPictureInPicture();

    /// <summary>
    /// onenterpictureinpicture
    /// </summary>
    [Description("@#onenterpictureinpicture")]
    public extern EventHandler Onenterpictureinpicture { get; set; }

    /// <summary>
    /// onleavepictureinpicture
    /// </summary>
    [Description("@#onleavepictureinpicture")]
    public extern EventHandler Onleavepictureinpicture { get; set; }

    /// <summary>
    /// disablePictureInPicture
    /// </summary>
    [Description("@#disablePictureInPicture")]
    public extern bool DisablePictureInPicture { get; set; }

    /// <summary>
    /// requestVideoFrameCallback
    /// </summary>
    /// <param name="callback">callback</param>
    [Description("@#requestVideoFrameCallback")]
    public extern uint RequestVideoFrameCallback(VideoFrameRequestCallback callback);

    /// <summary>
    /// cancelVideoFrameCallback
    /// </summary>
    /// <param name="handle">handle</param>
    [Description("@#cancelVideoFrameCallback")]
    public extern void CancelVideoFrameCallback(uint handle);
}

/// <summary>
/// HTMLAudioElement
/// </summary>
[ECMAScript]
[Description("@#HTMLAudioElement")]
public class HTMLAudioElement : HTMLMediaElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLAudioElement();
}

/// <summary>
/// HTMLTrackElement
/// </summary>
[ECMAScript]
[Description("@#HTMLTrackElement")]
public class HTMLTrackElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLTrackElement();

    /// <summary>
    /// kind
    /// </summary>
    [Description("@#kind")]
    public extern string Kind { get; set; }

    /// <summary>
    /// src
    /// </summary>
    [Description("@#src")]
    public extern string Src { get; set; }

    /// <summary>
    /// srclang
    /// </summary>
    [Description("@#srclang")]
    public extern string Srclang { get; set; }

    /// <summary>
    /// label
    /// </summary>
    [Description("@#label")]
    public extern string Label { get; set; }

    /// <summary>
    /// default
    /// </summary>
    [Description("@#default")]
    public extern bool Default { get; set; }

    /// <summary>
    /// NONE
    /// </summary>
    [Description("@#NONE")]
    public const ushort NONE = 0;

    /// <summary>
    /// LOADING
    /// </summary>
    [Description("@#LOADING")]
    public const ushort LOADING = 1;

    /// <summary>
    /// LOADED
    /// </summary>
    [Description("@#LOADED")]
    public const ushort LOADED = 2;

    /// <summary>
    /// ERROR
    /// </summary>
    [Description("@#ERROR")]
    public const ushort ERROR = 3;

    /// <summary>
    /// readyState
    /// </summary>
    [Description("@#readyState")]
    public extern ushort ReadyState { get; }

    /// <summary>
    /// track
    /// </summary>
    [Description("@#track")]
    public extern TextTrack Track { get; }
}

/// <summary>
/// MediaError
/// </summary>
[ECMAScript]
[Description("@#MediaError")]
public class MediaError
{
    /// <summary>
    /// MEDIA_ERR_ABORTED
    /// </summary>
    [Description("@#MEDIA_ERR_ABORTED")]
    public const ushort MEDIA_ERR_ABORTED = 1;

    /// <summary>
    /// MEDIA_ERR_NETWORK
    /// </summary>
    [Description("@#MEDIA_ERR_NETWORK")]
    public const ushort MEDIA_ERR_NETWORK = 2;

    /// <summary>
    /// MEDIA_ERR_DECODE
    /// </summary>
    [Description("@#MEDIA_ERR_DECODE")]
    public const ushort MEDIA_ERR_DECODE = 3;

    /// <summary>
    /// MEDIA_ERR_SRC_NOT_SUPPORTED
    /// </summary>
    [Description("@#MEDIA_ERR_SRC_NOT_SUPPORTED")]
    public const ushort MEDIA_ERR_SRC_NOT_SUPPORTED = 4;

    /// <summary>
    /// code
    /// </summary>
    [Description("@#code")]
    public extern ushort Code { get; }

    /// <summary>
    /// message
    /// </summary>
    [Description("@#message")]
    public extern string Message { get; }
}

/// <summary>
/// AudioTrackList
/// </summary>
[ECMAScript]
[Description("@#AudioTrackList")]
public class AudioTrackList : EventTarget
{
    /// <summary>
    /// length
    /// </summary>
    [Description("@#length")]
    public extern uint Length { get; }

    [Description("@#")]
    public extern AudioTrack this[uint index] { get; }

    /// <summary>
    /// getTrackById
    /// </summary>
    /// <param name="id">id</param>
    [Description("@#getTrackById")]
    public extern AudioTrack? GetTrackById(string id);

    /// <summary>
    /// onchange
    /// </summary>
    [Description("@#onchange")]
    public extern EventHandler Onchange { get; set; }

    /// <summary>
    /// onaddtrack
    /// </summary>
    [Description("@#onaddtrack")]
    public extern EventHandler Onaddtrack { get; set; }

    /// <summary>
    /// onremovetrack
    /// </summary>
    [Description("@#onremovetrack")]
    public extern EventHandler Onremovetrack { get; set; }
}

/// <summary>
/// AudioTrack
/// </summary>
[ECMAScript]
[Description("@#AudioTrack")]
public partial class AudioTrack
{
    /// <summary>
    /// id
    /// </summary>
    [Description("@#id")]
    public extern string Id { get; }

    /// <summary>
    /// kind
    /// </summary>
    [Description("@#kind")]
    public extern string Kind { get; }

    /// <summary>
    /// label
    /// </summary>
    [Description("@#label")]
    public extern string Label { get; }

    /// <summary>
    /// language
    /// </summary>
    [Description("@#language")]
    public extern string Language { get; }

    /// <summary>
    /// enabled
    /// </summary>
    [Description("@#enabled")]
    public extern bool Enabled { get; set; }

    /// <summary>
    /// sourceBuffer
    /// </summary>
    [Description("@#sourceBuffer")]
    public extern SourceBuffer? SourceBuffer { get; }
}

/// <summary>
/// VideoTrackList
/// </summary>
[ECMAScript]
[Description("@#VideoTrackList")]
public class VideoTrackList : EventTarget
{
    /// <summary>
    /// length
    /// </summary>
    [Description("@#length")]
    public extern uint Length { get; }

    [Description("@#")]
    public extern VideoTrack this[uint index] { get; }

    /// <summary>
    /// getTrackById
    /// </summary>
    /// <param name="id">id</param>
    [Description("@#getTrackById")]
    public extern VideoTrack? GetTrackById(string id);

    /// <summary>
    /// selectedIndex
    /// </summary>
    [Description("@#selectedIndex")]
    public extern int SelectedIndex { get; }

    /// <summary>
    /// onchange
    /// </summary>
    [Description("@#onchange")]
    public extern EventHandler Onchange { get; set; }

    /// <summary>
    /// onaddtrack
    /// </summary>
    [Description("@#onaddtrack")]
    public extern EventHandler Onaddtrack { get; set; }

    /// <summary>
    /// onremovetrack
    /// </summary>
    [Description("@#onremovetrack")]
    public extern EventHandler Onremovetrack { get; set; }
}

/// <summary>
/// VideoTrack
/// </summary>
[ECMAScript]
[Description("@#VideoTrack")]
public partial class VideoTrack
{
    /// <summary>
    /// id
    /// </summary>
    [Description("@#id")]
    public extern string Id { get; }

    /// <summary>
    /// kind
    /// </summary>
    [Description("@#kind")]
    public extern string Kind { get; }

    /// <summary>
    /// label
    /// </summary>
    [Description("@#label")]
    public extern string Label { get; }

    /// <summary>
    /// language
    /// </summary>
    [Description("@#language")]
    public extern string Language { get; }

    /// <summary>
    /// selected
    /// </summary>
    [Description("@#selected")]
    public extern bool Selected { get; set; }

    /// <summary>
    /// sourceBuffer
    /// </summary>
    [Description("@#sourceBuffer")]
    public extern SourceBuffer? SourceBuffer { get; }
}

/// <summary>
/// TextTrackList
/// </summary>
[ECMAScript]
[Description("@#TextTrackList")]
public class TextTrackList : EventTarget
{
    /// <summary>
    /// length
    /// </summary>
    [Description("@#length")]
    public extern uint Length { get; }

    [Description("@#")]
    public extern TextTrack this[uint index] { get; }

    /// <summary>
    /// getTrackById
    /// </summary>
    /// <param name="id">id</param>
    [Description("@#getTrackById")]
    public extern TextTrack? GetTrackById(string id);

    /// <summary>
    /// onchange
    /// </summary>
    [Description("@#onchange")]
    public extern EventHandler Onchange { get; set; }

    /// <summary>
    /// onaddtrack
    /// </summary>
    [Description("@#onaddtrack")]
    public extern EventHandler Onaddtrack { get; set; }

    /// <summary>
    /// onremovetrack
    /// </summary>
    [Description("@#onremovetrack")]
    public extern EventHandler Onremovetrack { get; set; }
}

/// <summary>
/// TextTrack
/// </summary>
[ECMAScript]
[Description("@#TextTrack")]
public partial class TextTrack : EventTarget
{
    /// <summary>
    /// kind
    /// </summary>
    [Description("@#kind")]
    public extern TextTrackKind Kind { get; }

    /// <summary>
    /// label
    /// </summary>
    [Description("@#label")]
    public extern string Label { get; }

    /// <summary>
    /// language
    /// </summary>
    [Description("@#language")]
    public extern string Language { get; }

    /// <summary>
    /// id
    /// </summary>
    [Description("@#id")]
    public extern string Id { get; }

    /// <summary>
    /// inBandMetadataTrackDispatchType
    /// </summary>
    [Description("@#inBandMetadataTrackDispatchType")]
    public extern string InBandMetadataTrackDispatchType { get; }

    /// <summary>
    /// mode
    /// </summary>
    [Description("@#mode")]
    public extern TextTrackMode Mode { get; set; }

    /// <summary>
    /// cues
    /// </summary>
    [Description("@#cues")]
    public extern TextTrackCueList? Cues { get; }

    /// <summary>
    /// activeCues
    /// </summary>
    [Description("@#activeCues")]
    public extern TextTrackCueList? ActiveCues { get; }

    /// <summary>
    /// addCue
    /// </summary>
    /// <param name="cue">cue</param>
    [Description("@#addCue")]
    public extern void AddCue(TextTrackCue cue);

    /// <summary>
    /// removeCue
    /// </summary>
    /// <param name="cue">cue</param>
    [Description("@#removeCue")]
    public extern void RemoveCue(TextTrackCue cue);

    /// <summary>
    /// oncuechange
    /// </summary>
    [Description("@#oncuechange")]
    public extern EventHandler Oncuechange { get; set; }

    /// <summary>
    /// sourceBuffer
    /// </summary>
    [Description("@#sourceBuffer")]
    public extern SourceBuffer? SourceBuffer { get; }
}

/// <summary>
/// TextTrackCueList
/// </summary>
[ECMAScript]
[Description("@#TextTrackCueList")]
public class TextTrackCueList
{
    /// <summary>
    /// length
    /// </summary>
    [Description("@#length")]
    public extern uint Length { get; }

    [Description("@#")]
    public extern TextTrackCue this[uint index] { get; }

    /// <summary>
    /// getCueById
    /// </summary>
    /// <param name="id">id</param>
    [Description("@#getCueById")]
    public extern TextTrackCue? GetCueById(string id);
}

/// <summary>
/// TextTrackCue
/// </summary>
[ECMAScript]
[Description("@#TextTrackCue")]
public class TextTrackCue : EventTarget
{
    /// <summary>
    /// track
    /// </summary>
    [Description("@#track")]
    public extern TextTrack? Track { get; }

    /// <summary>
    /// id
    /// </summary>
    [Description("@#id")]
    public extern string Id { get; set; }

    /// <summary>
    /// startTime
    /// </summary>
    [Description("@#startTime")]
    public extern double StartTime { get; set; }

    /// <summary>
    /// endTime
    /// </summary>
    [Description("@#endTime")]
    public extern double EndTime { get; set; }

    /// <summary>
    /// pauseOnExit
    /// </summary>
    [Description("@#pauseOnExit")]
    public extern bool PauseOnExit { get; set; }

    /// <summary>
    /// onenter
    /// </summary>
    [Description("@#onenter")]
    public extern EventHandler Onenter { get; set; }

    /// <summary>
    /// onexit
    /// </summary>
    [Description("@#onexit")]
    public extern EventHandler Onexit { get; set; }
}

/// <summary>
/// TimeRanges
/// </summary>
[ECMAScript]
[Description("@#TimeRanges")]
public class TimeRanges
{
    /// <summary>
    /// length
    /// </summary>
    [Description("@#length")]
    public extern uint Length { get; }

    /// <summary>
    /// start
    /// </summary>
    /// <param name="index">index</param>
    [Description("@#start")]
    public extern double Start(uint index);

    /// <summary>
    /// end
    /// </summary>
    /// <param name="index">index</param>
    [Description("@#end")]
    public extern double End(uint index);
}

/// <summary>
/// TrackEvent
/// </summary>
[ECMAScript]
[Description("@#TrackEvent")]
public class TrackEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern TrackEvent(string type, TrackEventInit eventInitDict);

    /// <summary>
    /// track
    /// </summary>
    [Description("@#track")]
    public extern Either<VideoTrack, AudioTrack, TextTrack>? Track { get; }
}

/// <summary>
/// HTMLMapElement
/// </summary>
[ECMAScript]
[Description("@#HTMLMapElement")]
public class HTMLMapElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLMapElement();

    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; set; }

    /// <summary>
    /// areas
    /// </summary>
    [Description("@#areas")]
    public extern HTMLCollection Areas { get; }
}

/// <summary>
/// HTMLAreaElement
/// </summary>
[ECMAScript]
[Description("@#HTMLAreaElement")]
public partial class HTMLAreaElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLAreaElement();

    /// <summary>
    /// alt
    /// </summary>
    [Description("@#alt")]
    public extern string Alt { get; set; }

    /// <summary>
    /// coords
    /// </summary>
    [Description("@#coords")]
    public extern string Coords { get; set; }

    /// <summary>
    /// shape
    /// </summary>
    [Description("@#shape")]
    public extern string Shape { get; set; }

    /// <summary>
    /// target
    /// </summary>
    [Description("@#target")]
    public extern string Target { get; set; }

    /// <summary>
    /// download
    /// </summary>
    [Description("@#download")]
    public extern string Download { get; set; }

    /// <summary>
    /// ping
    /// </summary>
    [Description("@#ping")]
    public extern string Ping { get; set; }

    /// <summary>
    /// rel
    /// </summary>
    [Description("@#rel")]
    public extern string Rel { get; set; }

    /// <summary>
    /// relList
    /// </summary>
    [Description("@#relList")]
    public extern List<string> RelList { get; }

    /// <summary>
    /// referrerPolicy
    /// </summary>
    [Description("@#referrerPolicy")]
    public extern string ReferrerPolicy { get; set; }

    /// <summary>
    /// noHref
    /// </summary>
    [Description("@#noHref")]
    public extern bool NoHref { get; set; }

    #region mixin HTMLAttributionSrcElementUtils
    /// <summary>
    /// attributionSrc
    /// </summary>
    [Description("@#attributionSrc")]
    public extern string AttributionSrc { get; set; }
    #endregion

    #region mixin HTMLHyperlinkElementUtils
    /// <summary>
    /// href
    /// </summary>
    [Description("@#href")]
    public extern string Href { get; set; }

    /// <summary>
    /// origin
    /// </summary>
    [Description("@#origin")]
    public extern string Origin { get; }

    /// <summary>
    /// protocol
    /// </summary>
    [Description("@#protocol")]
    public extern string Protocol { get; set; }

    /// <summary>
    /// username
    /// </summary>
    [Description("@#username")]
    public extern string Username { get; set; }

    /// <summary>
    /// password
    /// </summary>
    [Description("@#password")]
    public extern string Password { get; set; }

    /// <summary>
    /// host
    /// </summary>
    [Description("@#host")]
    public extern string Host { get; set; }

    /// <summary>
    /// hostname
    /// </summary>
    [Description("@#hostname")]
    public extern string Hostname { get; set; }

    /// <summary>
    /// port
    /// </summary>
    [Description("@#port")]
    public extern string Port { get; set; }

    /// <summary>
    /// pathname
    /// </summary>
    [Description("@#pathname")]
    public extern string Pathname { get; set; }

    /// <summary>
    /// search
    /// </summary>
    [Description("@#search")]
    public extern string Search { get; set; }

    /// <summary>
    /// hash
    /// </summary>
    [Description("@#hash")]
    public extern string Hash { get; set; }
    #endregion
}

/// <summary>
/// HTMLTableElement
/// </summary>
[ECMAScript]
[Description("@#HTMLTableElement")]
public partial class HTMLTableElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLTableElement();

    /// <summary>
    /// caption
    /// </summary>
    [Description("@#caption")]
    public extern HTMLTableCaptionElement? Caption { get; set; }

    /// <summary>
    /// createCaption
    /// </summary>
    [Description("@#createCaption")]
    public extern HTMLTableCaptionElement CreateCaption();

    /// <summary>
    /// deleteCaption
    /// </summary>
    [Description("@#deleteCaption")]
    public extern void DeleteCaption();

    /// <summary>
    /// tHead
    /// </summary>
    [Description("@#tHead")]
    public extern HTMLTableSectionElement? THead { get; set; }

    /// <summary>
    /// createTHead
    /// </summary>
    [Description("@#createTHead")]
    public extern HTMLTableSectionElement CreateTHead();

    /// <summary>
    /// deleteTHead
    /// </summary>
    [Description("@#deleteTHead")]
    public extern void DeleteTHead();

    /// <summary>
    /// tFoot
    /// </summary>
    [Description("@#tFoot")]
    public extern HTMLTableSectionElement? TFoot { get; set; }

    /// <summary>
    /// createTFoot
    /// </summary>
    [Description("@#createTFoot")]
    public extern HTMLTableSectionElement CreateTFoot();

    /// <summary>
    /// deleteTFoot
    /// </summary>
    [Description("@#deleteTFoot")]
    public extern void DeleteTFoot();

    /// <summary>
    /// tBodies
    /// </summary>
    [Description("@#tBodies")]
    public extern HTMLCollection TBodies { get; }

    /// <summary>
    /// createTBody
    /// </summary>
    [Description("@#createTBody")]
    public extern HTMLTableSectionElement CreateTBody();

    /// <summary>
    /// rows
    /// </summary>
    [Description("@#rows")]
    public extern HTMLCollection Rows { get; }

    /// <summary>
    /// insertRow
    /// </summary>
    /// <param name="index">index</param>
    [Description("@#insertRow")]
    public extern HTMLTableRowElement InsertRow(int index = -1);

    /// <summary>
    /// deleteRow
    /// </summary>
    /// <param name="index">index</param>
    [Description("@#deleteRow")]
    public extern void DeleteRow(int index);

    /// <summary>
    /// align
    /// </summary>
    [Description("@#align")]
    public extern string Align { get; set; }

    /// <summary>
    /// border
    /// </summary>
    [Description("@#border")]
    public extern string Border { get; set; }

    /// <summary>
    /// frame
    /// </summary>
    [Description("@#frame")]
    public extern string Frame { get; set; }

    /// <summary>
    /// rules
    /// </summary>
    [Description("@#rules")]
    public extern string Rules { get; set; }

    /// <summary>
    /// summary
    /// </summary>
    [Description("@#summary")]
    public extern string Summary { get; set; }

    /// <summary>
    /// width
    /// </summary>
    [Description("@#width")]
    public extern string Width { get; set; }

    /// <summary>
    /// bgColor
    /// </summary>
    [Description("@#bgColor")]
    public extern string BgColor { get; set; }

    /// <summary>
    /// cellPadding
    /// </summary>
    [Description("@#cellPadding")]
    public extern string CellPadding { get; set; }

    /// <summary>
    /// cellSpacing
    /// </summary>
    [Description("@#cellSpacing")]
    public extern string CellSpacing { get; set; }
}

/// <summary>
/// HTMLTableCaptionElement
/// </summary>
[ECMAScript]
[Description("@#HTMLTableCaptionElement")]
public partial class HTMLTableCaptionElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLTableCaptionElement();

    /// <summary>
    /// align
    /// </summary>
    [Description("@#align")]
    public extern string Align { get; set; }
}

/// <summary>
/// HTMLTableColElement
/// </summary>
[ECMAScript]
[Description("@#HTMLTableColElement")]
public partial class HTMLTableColElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLTableColElement();

    /// <summary>
    /// span
    /// </summary>
    [Description("@#span")]
    public extern uint Span { get; set; }

    /// <summary>
    /// align
    /// </summary>
    [Description("@#align")]
    public extern string Align { get; set; }

    /// <summary>
    /// ch
    /// </summary>
    [Description("@#ch")]
    public extern string Ch { get; set; }

    /// <summary>
    /// chOff
    /// </summary>
    [Description("@#chOff")]
    public extern string ChOff { get; set; }

    /// <summary>
    /// vAlign
    /// </summary>
    [Description("@#vAlign")]
    public extern string VAlign { get; set; }

    /// <summary>
    /// width
    /// </summary>
    [Description("@#width")]
    public extern string Width { get; set; }
}

/// <summary>
/// HTMLTableSectionElement
/// </summary>
[ECMAScript]
[Description("@#HTMLTableSectionElement")]
public partial class HTMLTableSectionElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLTableSectionElement();

    /// <summary>
    /// rows
    /// </summary>
    [Description("@#rows")]
    public extern HTMLCollection Rows { get; }

    /// <summary>
    /// insertRow
    /// </summary>
    /// <param name="index">index</param>
    [Description("@#insertRow")]
    public extern HTMLTableRowElement InsertRow(int index = -1);

    /// <summary>
    /// deleteRow
    /// </summary>
    /// <param name="index">index</param>
    [Description("@#deleteRow")]
    public extern void DeleteRow(int index);

    /// <summary>
    /// align
    /// </summary>
    [Description("@#align")]
    public extern string Align { get; set; }

    /// <summary>
    /// ch
    /// </summary>
    [Description("@#ch")]
    public extern string Ch { get; set; }

    /// <summary>
    /// chOff
    /// </summary>
    [Description("@#chOff")]
    public extern string ChOff { get; set; }

    /// <summary>
    /// vAlign
    /// </summary>
    [Description("@#vAlign")]
    public extern string VAlign { get; set; }
}

/// <summary>
/// HTMLTableRowElement
/// </summary>
[ECMAScript]
[Description("@#HTMLTableRowElement")]
public partial class HTMLTableRowElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLTableRowElement();

    /// <summary>
    /// rowIndex
    /// </summary>
    [Description("@#rowIndex")]
    public extern int RowIndex { get; }

    /// <summary>
    /// sectionRowIndex
    /// </summary>
    [Description("@#sectionRowIndex")]
    public extern int SectionRowIndex { get; }

    /// <summary>
    /// cells
    /// </summary>
    [Description("@#cells")]
    public extern HTMLCollection Cells { get; }

    /// <summary>
    /// insertCell
    /// </summary>
    /// <param name="index">index</param>
    [Description("@#insertCell")]
    public extern HTMLTableCellElement InsertCell(int index = -1);

    /// <summary>
    /// deleteCell
    /// </summary>
    /// <param name="index">index</param>
    [Description("@#deleteCell")]
    public extern void DeleteCell(int index);

    /// <summary>
    /// align
    /// </summary>
    [Description("@#align")]
    public extern string Align { get; set; }

    /// <summary>
    /// ch
    /// </summary>
    [Description("@#ch")]
    public extern string Ch { get; set; }

    /// <summary>
    /// chOff
    /// </summary>
    [Description("@#chOff")]
    public extern string ChOff { get; set; }

    /// <summary>
    /// vAlign
    /// </summary>
    [Description("@#vAlign")]
    public extern string VAlign { get; set; }

    /// <summary>
    /// bgColor
    /// </summary>
    [Description("@#bgColor")]
    public extern string BgColor { get; set; }
}

/// <summary>
/// HTMLTableCellElement
/// </summary>
[ECMAScript]
[Description("@#HTMLTableCellElement")]
public partial class HTMLTableCellElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLTableCellElement();

    /// <summary>
    /// colSpan
    /// </summary>
    [Description("@#colSpan")]
    public extern uint ColSpan { get; set; }

    /// <summary>
    /// rowSpan
    /// </summary>
    [Description("@#rowSpan")]
    public extern uint RowSpan { get; set; }

    /// <summary>
    /// headers
    /// </summary>
    [Description("@#headers")]
    public extern string Headers { get; set; }

    /// <summary>
    /// cellIndex
    /// </summary>
    [Description("@#cellIndex")]
    public extern int CellIndex { get; }

    /// <summary>
    /// scope
    /// </summary>
    [Description("@#scope")]
    public extern string Scope { get; set; }

    /// <summary>
    /// abbr
    /// </summary>
    [Description("@#abbr")]
    public extern string Abbr { get; set; }

    /// <summary>
    /// align
    /// </summary>
    [Description("@#align")]
    public extern string Align { get; set; }

    /// <summary>
    /// axis
    /// </summary>
    [Description("@#axis")]
    public extern string Axis { get; set; }

    /// <summary>
    /// height
    /// </summary>
    [Description("@#height")]
    public extern string Height { get; set; }

    /// <summary>
    /// width
    /// </summary>
    [Description("@#width")]
    public extern string Width { get; set; }

    /// <summary>
    /// ch
    /// </summary>
    [Description("@#ch")]
    public extern string Ch { get; set; }

    /// <summary>
    /// chOff
    /// </summary>
    [Description("@#chOff")]
    public extern string ChOff { get; set; }

    /// <summary>
    /// noWrap
    /// </summary>
    [Description("@#noWrap")]
    public extern bool NoWrap { get; set; }

    /// <summary>
    /// vAlign
    /// </summary>
    [Description("@#vAlign")]
    public extern string VAlign { get; set; }

    /// <summary>
    /// bgColor
    /// </summary>
    [Description("@#bgColor")]
    public extern string BgColor { get; set; }
}

/// <summary>
/// HTMLFormElement
/// </summary>
[ECMAScript]
[Description("@#HTMLFormElement")]
public class HTMLFormElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLFormElement();

    /// <summary>
    /// acceptCharset
    /// </summary>
    [Description("@#acceptCharset")]
    public extern string AcceptCharset { get; set; }

    /// <summary>
    /// action
    /// </summary>
    [Description("@#action")]
    public extern string Action { get; set; }

    /// <summary>
    /// autocomplete
    /// </summary>
    [Description("@#autocomplete")]
    public extern string Autocomplete { get; set; }

    /// <summary>
    /// enctype
    /// </summary>
    [Description("@#enctype")]
    public extern string Enctype { get; set; }

    /// <summary>
    /// encoding
    /// </summary>
    [Description("@#encoding")]
    public extern string Encoding { get; set; }

    /// <summary>
    /// method
    /// </summary>
    [Description("@#method")]
    public extern string Method { get; set; }

    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; set; }

    /// <summary>
    /// noValidate
    /// </summary>
    [Description("@#noValidate")]
    public extern bool NoValidate { get; set; }

    /// <summary>
    /// target
    /// </summary>
    [Description("@#target")]
    public extern string Target { get; set; }

    /// <summary>
    /// rel
    /// </summary>
    [Description("@#rel")]
    public extern string Rel { get; set; }

    /// <summary>
    /// relList
    /// </summary>
    [Description("@#relList")]
    public extern List<string> RelList { get; }

    /// <summary>
    /// elements
    /// </summary>
    [Description("@#elements")]
    public extern HTMLFormControlsCollection Elements { get; }

    /// <summary>
    /// length
    /// </summary>
    [Description("@#length")]
    public extern uint Length { get; }

    [Description("@#")]
    public extern Element this[uint index] { get; }

    [Description("@#")]
    public extern Either<RadioNodeList, Element> this[string name] { get; }

    /// <summary>
    /// submit
    /// </summary>
    [Description("@#submit")]
    public extern void Submit();

    /// <summary>
    /// requestSubmit
    /// </summary>
    /// <param name="submitter">submitter</param>
    [Description("@#requestSubmit")]
    public extern void RequestSubmit(HTMLElement? submitter = null);

    /// <summary>
    /// reset
    /// </summary>
    [Description("@#reset")]
    public extern void Reset();

    /// <summary>
    /// checkValidity
    /// </summary>
    [Description("@#checkValidity")]
    public extern bool CheckValidity();

    /// <summary>
    /// reportValidity
    /// </summary>
    [Description("@#reportValidity")]
    public extern bool ReportValidity();
}

/// <summary>
/// HTMLLabelElement
/// </summary>
[ECMAScript]
[Description("@#HTMLLabelElement")]
public class HTMLLabelElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLLabelElement();

    /// <summary>
    /// form
    /// </summary>
    [Description("@#form")]
    public extern HTMLFormElement? Form { get; }

    /// <summary>
    /// htmlFor
    /// </summary>
    [Description("@#htmlFor")]
    public extern string HtmlFor { get; set; }

    /// <summary>
    /// control
    /// </summary>
    [Description("@#control")]
    public extern HTMLElement? Control { get; }
}

/// <summary>
/// HTMLButtonElement
/// </summary>
[ECMAScript]
[Description("@#HTMLButtonElement")]
public class HTMLButtonElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLButtonElement();

    /// <summary>
    /// command
    /// </summary>
    [Description("@#command")]
    public extern string Command { get; set; }

    /// <summary>
    /// commandForElement
    /// </summary>
    [Description("@#commandForElement")]
    public extern Element? CommandForElement { get; set; }

    /// <summary>
    /// disabled
    /// </summary>
    [Description("@#disabled")]
    public extern bool Disabled { get; set; }

    /// <summary>
    /// form
    /// </summary>
    [Description("@#form")]
    public extern HTMLFormElement? Form { get; }

    /// <summary>
    /// formAction
    /// </summary>
    [Description("@#formAction")]
    public extern string FormAction { get; set; }

    /// <summary>
    /// formEnctype
    /// </summary>
    [Description("@#formEnctype")]
    public extern string FormEnctype { get; set; }

    /// <summary>
    /// formMethod
    /// </summary>
    [Description("@#formMethod")]
    public extern string FormMethod { get; set; }

    /// <summary>
    /// formNoValidate
    /// </summary>
    [Description("@#formNoValidate")]
    public extern bool FormNoValidate { get; set; }

    /// <summary>
    /// formTarget
    /// </summary>
    [Description("@#formTarget")]
    public extern string FormTarget { get; set; }

    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; set; }

    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern string Type { get; set; }

    /// <summary>
    /// value
    /// </summary>
    [Description("@#value")]
    public extern string Value { get; set; }

    /// <summary>
    /// willValidate
    /// </summary>
    [Description("@#willValidate")]
    public extern bool WillValidate { get; }

    /// <summary>
    /// validity
    /// </summary>
    [Description("@#validity")]
    public extern ValidityState Validity { get; }

    /// <summary>
    /// validationMessage
    /// </summary>
    [Description("@#validationMessage")]
    public extern string ValidationMessage { get; }

    /// <summary>
    /// checkValidity
    /// </summary>
    [Description("@#checkValidity")]
    public extern bool CheckValidity();

    /// <summary>
    /// reportValidity
    /// </summary>
    [Description("@#reportValidity")]
    public extern bool ReportValidity();

    /// <summary>
    /// setCustomValidity
    /// </summary>
    /// <param name="error">error</param>
    [Description("@#setCustomValidity")]
    public extern void SetCustomValidity(string error);

    /// <summary>
    /// labels
    /// </summary>
    [Description("@#labels")]
    public extern NodeList Labels { get; }

    #region mixin PopoverInvokerElement
    /// <summary>
    /// popoverTargetElement
    /// </summary>
    [Description("@#popoverTargetElement")]
    public extern Element? PopoverTargetElement { get; set; }

    /// <summary>
    /// popoverTargetAction
    /// </summary>
    [Description("@#popoverTargetAction")]
    public extern string PopoverTargetAction { get; set; }
    #endregion
}

/// <summary>
/// HTMLSelectElement
/// </summary>
[ECMAScript]
[Description("@#HTMLSelectElement")]
public class HTMLSelectElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLSelectElement();

    /// <summary>
    /// autocomplete
    /// </summary>
    [Description("@#autocomplete")]
    public extern string Autocomplete { get; set; }

    /// <summary>
    /// disabled
    /// </summary>
    [Description("@#disabled")]
    public extern bool Disabled { get; set; }

    /// <summary>
    /// form
    /// </summary>
    [Description("@#form")]
    public extern HTMLFormElement? Form { get; }

    /// <summary>
    /// multiple
    /// </summary>
    [Description("@#multiple")]
    public extern bool Multiple { get; set; }

    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; set; }

    /// <summary>
    /// required
    /// </summary>
    [Description("@#required")]
    public extern bool Required { get; set; }

    /// <summary>
    /// size
    /// </summary>
    [Description("@#size")]
    public extern uint Size { get; set; }

    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern string Type { get; }

    /// <summary>
    /// options
    /// </summary>
    [Description("@#options")]
    public extern HTMLOptionsCollection Options { get; }

    /// <summary>
    /// length
    /// </summary>
    [Description("@#length")]
    public extern uint Length { get; set; }

    /// <summary>
    /// item
    /// </summary>
    /// <param name="index">index</param>
    [Description("@#item")]
    public extern HTMLOptionElement? GetItem(uint index);

    /// <summary>
    /// namedItem
    /// </summary>
    /// <param name="name">name</param>
    [Description("@#namedItem")]
    public extern HTMLOptionElement? NamedItem(string name);

    /// <summary>
    /// add
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="before">before</param>
    [Description("@#add")]
    public extern void Add(Either<HTMLOptionElement, HTMLOptGroupElement> element, Either<HTMLElement, int>? before);

    /// <summary>
    /// add
    /// </summary>
    /// <param name="element">element para</param>
    /// <param name="before">before</param>
    [Description("@#add")]
    public extern void Add(Either<HTMLOptionElement, HTMLOptGroupElement> element, HTMLElement before);

    /// <summary>
    /// add
    /// </summary>
    /// <param name="element">element para</param>
    /// <param name="before">before</param>
    [Description("@#add")]
    public extern void Add(Either<HTMLOptionElement, HTMLOptGroupElement> element, int before);

    ///// <summary>
    ///// remove
    ///// </summary>
    //[Description("@#remove")]
    //public extern void Remove();

    /// <summary>
    /// remove
    /// </summary>
    /// <param name="index">index</param>
    [Description("@#remove")]
    public extern void Remove(int index);

    /// <summary>
    /// selectedOptions
    /// </summary>
    [Description("@#selectedOptions")]
    public extern HTMLCollection SelectedOptions { get; }

    /// <summary>
    /// selectedIndex
    /// </summary>
    [Description("@#selectedIndex")]
    public extern int SelectedIndex { get; set; }

    /// <summary>
    /// value
    /// </summary>
    [Description("@#value")]
    public extern string Value { get; set; }

    /// <summary>
    /// willValidate
    /// </summary>
    [Description("@#willValidate")]
    public extern bool WillValidate { get; }

    /// <summary>
    /// validity
    /// </summary>
    [Description("@#validity")]
    public extern ValidityState Validity { get; }

    /// <summary>
    /// validationMessage
    /// </summary>
    [Description("@#validationMessage")]
    public extern string ValidationMessage { get; }

    /// <summary>
    /// checkValidity
    /// </summary>
    [Description("@#checkValidity")]
    public extern bool CheckValidity();

    /// <summary>
    /// reportValidity
    /// </summary>
    [Description("@#reportValidity")]
    public extern bool ReportValidity();

    /// <summary>
    /// setCustomValidity
    /// </summary>
    /// <param name="error">error</param>
    [Description("@#setCustomValidity")]
    public extern void SetCustomValidity(string error);

    /// <summary>
    /// showPicker
    /// </summary>
    [Description("@#showPicker")]
    public extern void ShowPicker();

    /// <summary>
    /// labels
    /// </summary>
    [Description("@#labels")]
    public extern NodeList Labels { get; }
}

/// <summary>
/// HTMLDataListElement
/// </summary>
[ECMAScript]
[Description("@#HTMLDataListElement")]
public class HTMLDataListElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLDataListElement();

    /// <summary>
    /// options
    /// </summary>
    [Description("@#options")]
    public extern HTMLCollection Options { get; }
}

/// <summary>
/// HTMLOptGroupElement
/// </summary>
[ECMAScript]
[Description("@#HTMLOptGroupElement")]
public class HTMLOptGroupElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLOptGroupElement();

    /// <summary>
    /// disabled
    /// </summary>
    [Description("@#disabled")]
    public extern bool Disabled { get; set; }

    /// <summary>
    /// label
    /// </summary>
    [Description("@#label")]
    public extern string Label { get; set; }
}

/// <summary>
/// HTMLOptionElement
/// </summary>
[ECMAScript]
[Description("@#HTMLOptionElement")]
public class HTMLOptionElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLOptionElement();

    /// <summary>
    /// disabled
    /// </summary>
    [Description("@#disabled")]
    public extern bool Disabled { get; set; }

    /// <summary>
    /// form
    /// </summary>
    [Description("@#form")]
    public extern HTMLFormElement? Form { get; }

    /// <summary>
    /// label
    /// </summary>
    [Description("@#label")]
    public extern string Label { get; set; }

    /// <summary>
    /// defaultSelected
    /// </summary>
    [Description("@#defaultSelected")]
    public extern bool DefaultSelected { get; set; }

    /// <summary>
    /// selected
    /// </summary>
    [Description("@#selected")]
    public extern bool Selected { get; set; }

    /// <summary>
    /// value
    /// </summary>
    [Description("@#value")]
    public extern string Value { get; set; }

    /// <summary>
    /// text
    /// </summary>
    [Description("@#text")]
    public extern string Text { get; set; }

    /// <summary>
    /// index
    /// </summary>
    [Description("@#index")]
    public extern int Index { get; }
}

/// <summary>
/// HTMLTextAreaElement
/// </summary>
[ECMAScript]
[Description("@#HTMLTextAreaElement")]
public class HTMLTextAreaElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLTextAreaElement();

    /// <summary>
    /// autocomplete
    /// </summary>
    [Description("@#autocomplete")]
    public extern string Autocomplete { get; set; }

    /// <summary>
    /// cols
    /// </summary>
    [Description("@#cols")]
    public extern uint Cols { get; set; }

    /// <summary>
    /// dirName
    /// </summary>
    [Description("@#dirName")]
    public extern string DirName { get; set; }

    /// <summary>
    /// disabled
    /// </summary>
    [Description("@#disabled")]
    public extern bool Disabled { get; set; }

    /// <summary>
    /// form
    /// </summary>
    [Description("@#form")]
    public extern HTMLFormElement? Form { get; }

    /// <summary>
    /// maxLength
    /// </summary>
    [Description("@#maxLength")]
    public extern int MaxLength { get; set; }

    /// <summary>
    /// minLength
    /// </summary>
    [Description("@#minLength")]
    public extern int MinLength { get; set; }

    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; set; }

    /// <summary>
    /// placeholder
    /// </summary>
    [Description("@#placeholder")]
    public extern string Placeholder { get; set; }

    /// <summary>
    /// readOnly
    /// </summary>
    [Description("@#readOnly")]
    public extern bool ReadOnly { get; set; }

    /// <summary>
    /// required
    /// </summary>
    [Description("@#required")]
    public extern bool Required { get; set; }

    /// <summary>
    /// rows
    /// </summary>
    [Description("@#rows")]
    public extern uint Rows { get; set; }

    /// <summary>
    /// wrap
    /// </summary>
    [Description("@#wrap")]
    public extern string Wrap { get; set; }

    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern string Type { get; }

    /// <summary>
    /// defaultValue
    /// </summary>
    [Description("@#defaultValue")]
    public extern string DefaultValue { get; set; }

    /// <summary>
    /// value
    /// </summary>
    [Description("@#value")]
    public extern string Value { get; set; }

    /// <summary>
    /// textLength
    /// </summary>
    [Description("@#textLength")]
    public extern uint TextLength { get; }

    /// <summary>
    /// willValidate
    /// </summary>
    [Description("@#willValidate")]
    public extern bool WillValidate { get; }

    /// <summary>
    /// validity
    /// </summary>
    [Description("@#validity")]
    public extern ValidityState Validity { get; }

    /// <summary>
    /// validationMessage
    /// </summary>
    [Description("@#validationMessage")]
    public extern string ValidationMessage { get; }

    /// <summary>
    /// checkValidity
    /// </summary>
    [Description("@#checkValidity")]
    public extern bool CheckValidity();

    /// <summary>
    /// reportValidity
    /// </summary>
    [Description("@#reportValidity")]
    public extern bool ReportValidity();

    /// <summary>
    /// setCustomValidity
    /// </summary>
    /// <param name="error">error</param>
    [Description("@#setCustomValidity")]
    public extern void SetCustomValidity(string error);

    /// <summary>
    /// labels
    /// </summary>
    [Description("@#labels")]
    public extern NodeList Labels { get; }

    /// <summary>
    /// select
    /// </summary>
    [Description("@#select")]
    public extern void Select();

    /// <summary>
    /// selectionStart
    /// </summary>
    [Description("@#selectionStart")]
    public extern uint SelectionStart { get; set; }

    /// <summary>
    /// selectionEnd
    /// </summary>
    [Description("@#selectionEnd")]
    public extern uint SelectionEnd { get; set; }

    /// <summary>
    /// selectionDirection
    /// </summary>
    [Description("@#selectionDirection")]
    public extern string SelectionDirection { get; set; }

    /// <summary>
    /// setRangeText
    /// </summary>
    /// <param name="replacement">replacement</param>
    [Description("@#setRangeText")]
    public extern void SetRangeText(string replacement);

    /// <summary>
    /// setRangeText
    /// </summary>
    /// <param name="replacement">replacement</param>
    /// <param name="start">start</param>
    /// <param name="end">end</param>
    /// <param name="selectionMode">selectionMode</param>
    [Description("@#setRangeText")]
    public extern void SetRangeText(string replacement, uint start, uint end, SelectionMode selectionMode = SelectionMode.Preserve);

    /// <summary>
    /// setSelectionRange
    /// </summary>
    /// <param name="start">start</param>
    /// <param name="end">end</param>
    /// <param name="direction">direction</param>
    [Description("@#setSelectionRange")]
    public extern void SetSelectionRange(uint start, uint end, string direction);
}

/// <summary>
/// HTMLOutputElement
/// </summary>
[ECMAScript]
[Description("@#HTMLOutputElement")]
public class HTMLOutputElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLOutputElement();

    /// <summary>
    /// htmlFor
    /// </summary>
    [Description("@#htmlFor")]
    public extern List<string> HtmlFor { get; }

    /// <summary>
    /// form
    /// </summary>
    [Description("@#form")]
    public extern HTMLFormElement? Form { get; }

    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; set; }

    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern string Type { get; }

    /// <summary>
    /// defaultValue
    /// </summary>
    [Description("@#defaultValue")]
    public extern string DefaultValue { get; set; }

    /// <summary>
    /// value
    /// </summary>
    [Description("@#value")]
    public extern string Value { get; set; }

    /// <summary>
    /// willValidate
    /// </summary>
    [Description("@#willValidate")]
    public extern bool WillValidate { get; }

    /// <summary>
    /// validity
    /// </summary>
    [Description("@#validity")]
    public extern ValidityState Validity { get; }

    /// <summary>
    /// validationMessage
    /// </summary>
    [Description("@#validationMessage")]
    public extern string ValidationMessage { get; }

    /// <summary>
    /// checkValidity
    /// </summary>
    [Description("@#checkValidity")]
    public extern bool CheckValidity();

    /// <summary>
    /// reportValidity
    /// </summary>
    [Description("@#reportValidity")]
    public extern bool ReportValidity();

    /// <summary>
    /// setCustomValidity
    /// </summary>
    /// <param name="error">error</param>
    [Description("@#setCustomValidity")]
    public extern void SetCustomValidity(string error);

    /// <summary>
    /// labels
    /// </summary>
    [Description("@#labels")]
    public extern NodeList Labels { get; }
}

/// <summary>
/// HTMLProgressElement
/// </summary>
[ECMAScript]
[Description("@#HTMLProgressElement")]
public class HTMLProgressElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLProgressElement();

    /// <summary>
    /// value
    /// </summary>
    [Description("@#value")]
    public extern double Value { get; set; }

    /// <summary>
    /// max
    /// </summary>
    [Description("@#max")]
    public extern double Max { get; set; }

    /// <summary>
    /// position
    /// </summary>
    [Description("@#position")]
    public extern double Position { get; }

    /// <summary>
    /// labels
    /// </summary>
    [Description("@#labels")]
    public extern NodeList Labels { get; }
}

/// <summary>
/// HTMLMeterElement
/// </summary>
[ECMAScript]
[Description("@#HTMLMeterElement")]
public class HTMLMeterElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLMeterElement();

    /// <summary>
    /// value
    /// </summary>
    [Description("@#value")]
    public extern double Value { get; set; }

    /// <summary>
    /// min
    /// </summary>
    [Description("@#min")]
    public extern double Min { get; set; }

    /// <summary>
    /// max
    /// </summary>
    [Description("@#max")]
    public extern double Max { get; set; }

    /// <summary>
    /// low
    /// </summary>
    [Description("@#low")]
    public extern double Low { get; set; }

    /// <summary>
    /// high
    /// </summary>
    [Description("@#high")]
    public extern double High { get; set; }

    /// <summary>
    /// optimum
    /// </summary>
    [Description("@#optimum")]
    public extern double Optimum { get; set; }

    /// <summary>
    /// labels
    /// </summary>
    [Description("@#labels")]
    public extern NodeList Labels { get; }
}

/// <summary>
/// HTMLFieldSetElement
/// </summary>
[ECMAScript]
[Description("@#HTMLFieldSetElement")]
public class HTMLFieldSetElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLFieldSetElement();

    /// <summary>
    /// disabled
    /// </summary>
    [Description("@#disabled")]
    public extern bool Disabled { get; set; }

    /// <summary>
    /// form
    /// </summary>
    [Description("@#form")]
    public extern HTMLFormElement? Form { get; }

    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; set; }

    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern string Type { get; }

    /// <summary>
    /// elements
    /// </summary>
    [Description("@#elements")]
    public extern HTMLCollection Elements { get; }

    /// <summary>
    /// willValidate
    /// </summary>
    [Description("@#willValidate")]
    public extern bool WillValidate { get; }

    /// <summary>
    /// validity
    /// </summary>
    [Description("@#validity")]
    public extern ValidityState Validity { get; }

    /// <summary>
    /// validationMessage
    /// </summary>
    [Description("@#validationMessage")]
    public extern string ValidationMessage { get; }

    /// <summary>
    /// checkValidity
    /// </summary>
    [Description("@#checkValidity")]
    public extern bool CheckValidity();

    /// <summary>
    /// reportValidity
    /// </summary>
    [Description("@#reportValidity")]
    public extern bool ReportValidity();

    /// <summary>
    /// setCustomValidity
    /// </summary>
    /// <param name="error">error</param>
    [Description("@#setCustomValidity")]
    public extern void SetCustomValidity(string error);
}

/// <summary>
/// HTMLLegendElement
/// </summary>
[ECMAScript]
[Description("@#HTMLLegendElement")]
public partial class HTMLLegendElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLLegendElement();

    /// <summary>
    /// form
    /// </summary>
    [Description("@#form")]
    public extern HTMLFormElement? Form { get; }

    /// <summary>
    /// align
    /// </summary>
    [Description("@#align")]
    public extern string Align { get; set; }
}

/// <summary>
/// ValidityState
/// </summary>
[ECMAScript]
[Description("@#ValidityState")]
public class ValidityState
{
    /// <summary>
    /// valueMissing
    /// </summary>
    [Description("@#valueMissing")]
    public extern bool ValueMissing { get; }

    /// <summary>
    /// typeMismatch
    /// </summary>
    [Description("@#typeMismatch")]
    public extern bool TypeMismatch { get; }

    /// <summary>
    /// patternMismatch
    /// </summary>
    [Description("@#patternMismatch")]
    public extern bool PatternMismatch { get; }

    /// <summary>
    /// tooLong
    /// </summary>
    [Description("@#tooLong")]
    public extern bool TooLong { get; }

    /// <summary>
    /// tooShort
    /// </summary>
    [Description("@#tooShort")]
    public extern bool TooShort { get; }

    /// <summary>
    /// rangeUnderflow
    /// </summary>
    [Description("@#rangeUnderflow")]
    public extern bool RangeUnderflow { get; }

    /// <summary>
    /// rangeOverflow
    /// </summary>
    [Description("@#rangeOverflow")]
    public extern bool RangeOverflow { get; }

    /// <summary>
    /// stepMismatch
    /// </summary>
    [Description("@#stepMismatch")]
    public extern bool StepMismatch { get; }

    /// <summary>
    /// badInput
    /// </summary>
    [Description("@#badInput")]
    public extern bool BadInput { get; }

    /// <summary>
    /// customError
    /// </summary>
    [Description("@#customError")]
    public extern bool CustomError { get; }

    /// <summary>
    /// valid
    /// </summary>
    [Description("@#valid")]
    public extern bool Valid { get; }
}

/// <summary>
/// SubmitEvent
/// </summary>
[ECMAScript]
[Description("@#SubmitEvent")]
public class SubmitEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern SubmitEvent(string type, SubmitEventInit eventInitDict);

    /// <summary>
    /// submitter
    /// </summary>
    [Description("@#submitter")]
    public extern HTMLElement? Submitter { get; }
}

/// <summary>
/// FormDataEvent
/// </summary>
[ECMAScript]
[Description("@#FormDataEvent")]
public class FormDataEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern FormDataEvent(string type, FormDataEventInit eventInitDict);

    /// <summary>
    /// formData
    /// </summary>
    [Description("@#formData")]
    public extern FormData FormData { get; }
}

/// <summary>
/// HTMLDetailsElement
/// </summary>
[ECMAScript]
[Description("@#HTMLDetailsElement")]
public class HTMLDetailsElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLDetailsElement();

    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; set; }

    /// <summary>
    /// open
    /// </summary>
    [Description("@#open")]
    public extern bool Open { get; set; }
}

/// <summary>
/// HTMLDialogElement
/// </summary>
[ECMAScript]
[Description("@#HTMLDialogElement")]
public class HTMLDialogElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLDialogElement();

    /// <summary>
    /// open
    /// </summary>
    [Description("@#open")]
    public extern bool Open { get; set; }

    /// <summary>
    /// returnValue
    /// </summary>
    [Description("@#returnValue")]
    public extern string ReturnValue { get; set; }

    /// <summary>
    /// closedBy
    /// </summary>
    [Description("@#closedBy")]
    public extern string ClosedBy { get; set; }

    /// <summary>
    /// show
    /// </summary>
    [Description("@#show")]
    public extern void Show();

    /// <summary>
    /// showModal
    /// </summary>
    [Description("@#showModal")]
    public extern void ShowModal();

    /// <summary>
    /// close
    /// </summary>
    /// <param name="returnValue">returnValue</param>
    [Description("@#close")]
    public extern void Close(string returnValue);

    /// <summary>
    /// requestClose
    /// </summary>
    /// <param name="returnValue">returnValue</param>
    [Description("@#requestClose")]
    public extern void RequestClose(string returnValue);
}

/// <summary>
/// HTMLScriptElement
/// </summary>
[ECMAScript]
[Description("@#HTMLScriptElement")]
public partial class HTMLScriptElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLScriptElement();

    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern string Type { get; set; }

    /// <summary>
    /// src
    /// </summary>
    [Description("@#src")]
    public extern string Src { get; set; }

    /// <summary>
    /// noModule
    /// </summary>
    [Description("@#noModule")]
    public extern bool NoModule { get; set; }

    /// <summary>
    /// async
    /// </summary>
    [Description("@#async")]
    public extern bool Async { get; set; }

    /// <summary>
    /// defer
    /// </summary>
    [Description("@#defer")]
    public extern bool Defer { get; set; }

    /// <summary>
    /// blocking
    /// </summary>
    [Description("@#blocking")]
    public extern List<string> Blocking { get; }

    /// <summary>
    /// crossOrigin
    /// </summary>
    [Description("@#crossOrigin")]
    public extern string? CrossOrigin { get; set; }

    /// <summary>
    /// referrerPolicy
    /// </summary>
    [Description("@#referrerPolicy")]
    public extern string ReferrerPolicy { get; set; }

    /// <summary>
    /// integrity
    /// </summary>
    [Description("@#integrity")]
    public extern string Integrity { get; set; }

    /// <summary>
    /// fetchPriority
    /// </summary>
    [Description("@#fetchPriority")]
    public extern string FetchPriority { get; set; }

    /// <summary>
    /// text
    /// </summary>
    [Description("@#text")]
    public extern string Text { get; set; }

    /// <summary>
    /// supports
    /// </summary>
    /// <param name="type">type</param>
    [Description("@#supports")]
    public extern static bool Supports(string type);

    /// <summary>
    /// charset
    /// </summary>
    [Description("@#charset")]
    public extern string Charset { get; set; }

    /// <summary>
    /// event
    /// </summary>
    [Description("@#event")]
    public extern string Event { get; set; }

    /// <summary>
    /// htmlFor
    /// </summary>
    [Description("@#htmlFor")]
    public extern string HtmlFor { get; set; }

    #region mixin HTMLAttributionSrcElementUtils
    /// <summary>
    /// attributionSrc
    /// </summary>
    [Description("@#attributionSrc")]
    public extern string AttributionSrc { get; set; }
    #endregion
}

/// <summary>
/// HTMLTemplateElement
/// </summary>
[ECMAScript]
[Description("@#HTMLTemplateElement")]
public class HTMLTemplateElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLTemplateElement();

    /// <summary>
    /// content
    /// </summary>
    [Description("@#content")]
    public extern DocumentFragment Content { get; }

    /// <summary>
    /// shadowRootMode
    /// </summary>
    [Description("@#shadowRootMode")]
    public extern string ShadowRootMode { get; set; }

    /// <summary>
    /// shadowRootDelegatesFocus
    /// </summary>
    [Description("@#shadowRootDelegatesFocus")]
    public extern bool ShadowRootDelegatesFocus { get; set; }

    /// <summary>
    /// shadowRootClonable
    /// </summary>
    [Description("@#shadowRootClonable")]
    public extern bool ShadowRootClonable { get; set; }

    /// <summary>
    /// shadowRootSerializable
    /// </summary>
    [Description("@#shadowRootSerializable")]
    public extern bool ShadowRootSerializable { get; set; }

    /// <summary>
    /// shadowRootCustomElementRegistry
    /// </summary>
    [Description("@#shadowRootCustomElementRegistry")]
    public extern string ShadowRootCustomElementRegistry { get; set; }
}

/// <summary>
/// HTMLSlotElement
/// </summary>
[ECMAScript]
[Description("@#HTMLSlotElement")]
public class HTMLSlotElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLSlotElement();

    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; set; }

    /// <summary>
    /// assignedNodes
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#assignedNodes")]
    public extern Node[] AssignedNodes(AssignedNodesOptions? options = default);

    /// <summary>
    /// assignedElements
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#assignedElements")]
    public extern Element[] AssignedElements(AssignedNodesOptions? options = default);

    /// <summary>
    /// assign
    /// </summary>
    /// <param name="nodes">nodes</param>
    [Description("@#assign")]
    public extern void Assign(Either<Element, Text> nodes);

    /// <summary>
    /// assign
    /// </summary>
    /// <param name="nodes">nodes</param>
    [Description("@#assign")]
    public extern void Assign(Element nodes);

    /// <summary>
    /// assign
    /// </summary>
    /// <param name="nodes">nodes</param>
    [Description("@#assign")]
    public extern void Assign(Text nodes);
}

/// <summary>
/// HTMLCanvasElement
/// </summary>
[ECMAScript]
[Description("@#HTMLCanvasElement")]
public partial class HTMLCanvasElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLCanvasElement();

    /// <summary>
    /// width
    /// </summary>
    [Description("@#width")]
    public extern uint Width { get; set; }

    /// <summary>
    /// height
    /// </summary>
    [Description("@#height")]
    public extern uint Height { get; set; }

    /// <summary>
    /// getContext
    /// </summary>
    /// <param name="contextId">contextId</param>
    /// <param name="options">options</param>
    [Description("@#getContext")]
    public extern RenderingContext? GetContext(string contextId, object? options = default);

    /// <summary>
    /// toDataURL
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="quality">quality</param>
    [Description("@#toDataURL")]
    public extern string ToDataURL(string? type = default, object? quality = default);

    /// <summary>
    /// toBlob
    /// </summary>
    /// <param name="callback">callback</param>
    /// <param name="type">type</param>
    /// <param name="quality">quality</param>
    [Description("@#toBlob")]
    public extern void ToBlob(BlobCallback callback, string? type = default, object? quality = default);

    /// <summary>
    /// transferControlToOffscreen
    /// </summary>
    [Description("@#transferControlToOffscreen")]
    public extern OffscreenCanvas TransferControlToOffscreen();

    /// <summary>
    /// captureStream
    /// </summary>
    /// <param name="frameRequestRate">frameRequestRate</param>
    [Description("@#captureStream")]
    public extern MediaStream CaptureStream(double frameRequestRate);
}

/// <summary>
/// CanvasRenderingContext2D
/// </summary>
[ECMAScript]
[Description("@#CanvasRenderingContext2D")]
public class CanvasRenderingContext2D
{
    /// <summary>
    /// canvas
    /// </summary>
    [Description("@#canvas")]
    public extern HTMLCanvasElement Canvas { get; }

    #region mixin CanvasSettings
    /// <summary>
    /// getContextAttributes
    /// </summary>
    [Description("@#getContextAttributes")]
    public extern CanvasRenderingContext2DSettings GetContextAttributes();
    #endregion

    #region mixin CanvasState
    /// <summary>
    /// save
    /// </summary>
    [Description("@#save")]
    public extern void Save();

    /// <summary>
    /// restore
    /// </summary>
    [Description("@#restore")]
    public extern void Restore();

    /// <summary>
    /// reset
    /// </summary>
    [Description("@#reset")]
    public extern void Reset();

    /// <summary>
    /// isContextLost
    /// </summary>
    [Description("@#isContextLost")]
    public extern bool IsContextLost();
    #endregion

    #region mixin CanvasTransform
    /// <summary>
    /// scale
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    [Description("@#scale")]
    public extern void Scale(double x, double y);

    /// <summary>
    /// rotate
    /// </summary>
    /// <param name="angle">angle</param>
    [Description("@#rotate")]
    public extern void Rotate(double angle);

    /// <summary>
    /// translate
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    [Description("@#translate")]
    public extern void Translate(double x, double y);

    /// <summary>
    /// transform
    /// </summary>
    /// <param name="a">a</param>
    /// <param name="b">b</param>
    /// <param name="c">c</param>
    /// <param name="d">d</param>
    /// <param name="e">e</param>
    /// <param name="f">f</param>
    [Description("@#transform")]
    public extern void Transform(double a, double b, double c, double d, double e, double f);

    /// <summary>
    /// getTransform
    /// </summary>
    [Description("@#getTransform")]
    public extern DOMMatrix GetTransform();

    /// <summary>
    /// setTransform
    /// </summary>
    /// <param name="a">a</param>
    /// <param name="b">b</param>
    /// <param name="c">c</param>
    /// <param name="d">d</param>
    /// <param name="e">e</param>
    /// <param name="f">f</param>
    [Description("@#setTransform")]
    public extern void SetTransform(double a, double b, double c, double d, double e, double f);

    /// <summary>
    /// setTransform
    /// </summary>
    /// <param name="transform">transform</param>
    [Description("@#setTransform")]
    public extern void SetTransform(DOMMatrix2DInit? transform = default);

    /// <summary>
    /// resetTransform
    /// </summary>
    [Description("@#resetTransform")]
    public extern void ResetTransform();
    #endregion

    #region mixin CanvasCompositing
    /// <summary>
    /// globalAlpha
    /// </summary>
    [Description("@#globalAlpha")]
    public extern double GlobalAlpha { get; set; }

    /// <summary>
    /// globalCompositeOperation
    /// </summary>
    [Description("@#globalCompositeOperation")]
    public extern string GlobalCompositeOperation { get; set; }
    #endregion

    #region mixin CanvasImageSmoothing
    /// <summary>
    /// imageSmoothingEnabled
    /// </summary>
    [Description("@#imageSmoothingEnabled")]
    public extern bool ImageSmoothingEnabled { get; set; }

    /// <summary>
    /// imageSmoothingQuality
    /// </summary>
    [Description("@#imageSmoothingQuality")]
    public extern ImageSmoothingQuality ImageSmoothingQuality { get; set; }
    #endregion

    #region mixin CanvasFillStrokeStyles
    /// <summary>
    /// strokeStyle
    /// </summary>
    [Description("@#strokeStyle")]
    public extern Either<string, CanvasGradient, CanvasPattern> StrokeStyle { get; set; }

    /// <summary>
    /// fillStyle
    /// </summary>
    [Description("@#fillStyle")]
    public extern Either<string, CanvasGradient, CanvasPattern> FillStyle { get; set; }

    /// <summary>
    /// createLinearGradient
    /// </summary>
    /// <param name="x0">x0</param>
    /// <param name="y0">y0</param>
    /// <param name="x1">x1</param>
    /// <param name="y1">y1</param>
    [Description("@#createLinearGradient")]
    public extern CanvasGradient CreateLinearGradient(double x0, double y0, double x1, double y1);

    /// <summary>
    /// createRadialGradient
    /// </summary>
    /// <param name="x0">x0</param>
    /// <param name="y0">y0</param>
    /// <param name="r0">r0</param>
    /// <param name="x1">x1</param>
    /// <param name="y1">y1</param>
    /// <param name="r1">r1</param>
    [Description("@#createRadialGradient")]
    public extern CanvasGradient CreateRadialGradient(double x0, double y0, double r0, double x1, double y1, double r1);

    /// <summary>
    /// createConicGradient
    /// </summary>
    /// <param name="startAngle">startAngle</param>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    [Description("@#createConicGradient")]
    public extern CanvasGradient CreateConicGradient(double startAngle, double x, double y);

    /// <summary>
    /// createPattern
    /// </summary>
    /// <param name="image">image</param>
    /// <param name="repetition">repetition</param>
    [Description("@#createPattern")]
    public extern CanvasPattern? CreatePattern(CanvasImageSource image, string repetition);
    #endregion

    #region mixin CanvasShadowStyles
    /// <summary>
    /// shadowOffsetX
    /// </summary>
    [Description("@#shadowOffsetX")]
    public extern double ShadowOffsetX { get; set; }

    /// <summary>
    /// shadowOffsetY
    /// </summary>
    [Description("@#shadowOffsetY")]
    public extern double ShadowOffsetY { get; set; }

    /// <summary>
    /// shadowBlur
    /// </summary>
    [Description("@#shadowBlur")]
    public extern double ShadowBlur { get; set; }

    /// <summary>
    /// shadowColor
    /// </summary>
    [Description("@#shadowColor")]
    public extern string ShadowColor { get; set; }
    #endregion

    #region mixin CanvasFilters
    /// <summary>
    /// filter
    /// </summary>
    [Description("@#filter")]
    public extern string Filter { get; set; }
    #endregion

    #region mixin CanvasRect
    /// <summary>
    /// clearRect
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="w">w</param>
    /// <param name="h">h</param>
    [Description("@#clearRect")]
    public extern void ClearRect(double x, double y, double w, double h);

    /// <summary>
    /// fillRect
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="w">w</param>
    /// <param name="h">h</param>
    [Description("@#fillRect")]
    public extern void FillRect(double x, double y, double w, double h);

    /// <summary>
    /// strokeRect
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="w">w</param>
    /// <param name="h">h</param>
    [Description("@#strokeRect")]
    public extern void StrokeRect(double x, double y, double w, double h);
    #endregion

    #region mixin CanvasDrawPath
    /// <summary>
    /// beginPath
    /// </summary>
    [Description("@#beginPath")]
    public extern void BeginPath();

    /// <summary>
    /// fill
    /// </summary>
    /// <param name="fillRule">fillRule</param>
    [Description("@#fill")]
    public extern void Fill(CanvasFillRule fillRule = CanvasFillRule.Nonzero);

    /// <summary>
    /// fill
    /// </summary>
    /// <param name="path">path</param>
    /// <param name="fillRule">fillRule</param>
    [Description("@#fill")]
    public extern void Fill(Path2D path, CanvasFillRule fillRule = CanvasFillRule.Nonzero);

    /// <summary>
    /// stroke
    /// </summary>
    [Description("@#stroke")]
    public extern void Stroke();

    /// <summary>
    /// stroke
    /// </summary>
    /// <param name="path">path</param>
    [Description("@#stroke")]
    public extern void Stroke(Path2D path);

    /// <summary>
    /// clip
    /// </summary>
    /// <param name="fillRule">fillRule</param>
    [Description("@#clip")]
    public extern void Clip(CanvasFillRule fillRule = CanvasFillRule.Nonzero);

    /// <summary>
    /// clip
    /// </summary>
    /// <param name="path">path</param>
    /// <param name="fillRule">fillRule</param>
    [Description("@#clip")]
    public extern void Clip(Path2D path, CanvasFillRule fillRule = CanvasFillRule.Nonzero);

    /// <summary>
    /// isPointInPath
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="fillRule">fillRule</param>
    [Description("@#isPointInPath")]
    public extern bool IsPointInPath(double x, double y, CanvasFillRule fillRule = CanvasFillRule.Nonzero);

    /// <summary>
    /// isPointInPath
    /// </summary>
    /// <param name="path">path</param>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="fillRule">fillRule</param>
    [Description("@#isPointInPath")]
    public extern bool IsPointInPath(Path2D path, double x, double y, CanvasFillRule fillRule = CanvasFillRule.Nonzero);

    /// <summary>
    /// isPointInStroke
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    [Description("@#isPointInStroke")]
    public extern bool IsPointInStroke(double x, double y);

    /// <summary>
    /// isPointInStroke
    /// </summary>
    /// <param name="path">path</param>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    [Description("@#isPointInStroke")]
    public extern bool IsPointInStroke(Path2D path, double x, double y);
    #endregion

    #region mixin CanvasUserInterface
    /// <summary>
    /// drawFocusIfNeeded
    /// </summary>
    /// <param name="element">element</param>
    [Description("@#drawFocusIfNeeded")]
    public extern void DrawFocusIfNeeded(Element element);

    /// <summary>
    /// drawFocusIfNeeded
    /// </summary>
    /// <param name="path">path</param>
    /// <param name="element">element</param>
    [Description("@#drawFocusIfNeeded")]
    public extern void DrawFocusIfNeeded(Path2D path, Element element);
    #endregion

    #region mixin CanvasText
    /// <summary>
    /// fillText
    /// </summary>
    /// <param name="text">text</param>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="maxWidth">maxWidth</param>
    [Description("@#fillText")]
    public extern void FillText(string text, double x, double y, double maxWidth);

    /// <summary>
    /// strokeText
    /// </summary>
    /// <param name="text">text</param>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="maxWidth">maxWidth</param>
    [Description("@#strokeText")]
    public extern void StrokeText(string text, double x, double y, double maxWidth);

    /// <summary>
    /// measureText
    /// </summary>
    /// <param name="text">text</param>
    [Description("@#measureText")]
    public extern TextMetrics MeasureText(string text);
    #endregion

    #region mixin CanvasDrawImage
    /// <summary>
    /// drawImage
    /// </summary>
    /// <param name="image">image</param>
    /// <param name="dx">dx</param>
    /// <param name="dy">dy</param>
    [Description("@#drawImage")]
    public extern void DrawImage(CanvasImageSource image, double dx, double dy);

    /// <summary>
    /// drawImage
    /// </summary>
    /// <param name="image">image</param>
    /// <param name="dx">dx</param>
    /// <param name="dy">dy</param>
    /// <param name="dw">dw</param>
    /// <param name="dh">dh</param>
    [Description("@#drawImage")]
    public extern void DrawImage(CanvasImageSource image, double dx, double dy, double dw, double dh);

    /// <summary>
    /// drawImage
    /// </summary>
    /// <param name="image">image</param>
    /// <param name="sx">sx</param>
    /// <param name="sy">sy</param>
    /// <param name="sw">sw</param>
    /// <param name="sh">sh</param>
    /// <param name="dx">dx</param>
    /// <param name="dy">dy</param>
    /// <param name="dw">dw</param>
    /// <param name="dh">dh</param>
    [Description("@#drawImage")]
    public extern void DrawImage(CanvasImageSource image, double sx, double sy, double sw, double sh, double dx, double dy, double dw, double dh);
    #endregion

    #region mixin CanvasImageData
    /// <summary>
    /// createImageData
    /// </summary>
    /// <param name="sw">sw</param>
    /// <param name="sh">sh</param>
    /// <param name="settings">settings</param>
    [Description("@#createImageData")]
    public extern ImageData CreateImageData(int sw, int sh, ImageDataSettings? settings = default);

    /// <summary>
    /// createImageData
    /// </summary>
    /// <param name="imageData">imageData</param>
    [Description("@#createImageData")]
    public extern ImageData CreateImageData(ImageData imageData);

    /// <summary>
    /// getImageData
    /// </summary>
    /// <param name="sx">sx</param>
    /// <param name="sy">sy</param>
    /// <param name="sw">sw</param>
    /// <param name="sh">sh</param>
    /// <param name="settings">settings</param>
    [Description("@#getImageData")]
    public extern ImageData GetImageData(int sx, int sy, int sw, int sh, ImageDataSettings? settings = default);

    /// <summary>
    /// putImageData
    /// </summary>
    /// <param name="imageData">imageData</param>
    /// <param name="dx">dx</param>
    /// <param name="dy">dy</param>
    [Description("@#putImageData")]
    public extern void PutImageData(ImageData imageData, int dx, int dy);

    /// <summary>
    /// putImageData
    /// </summary>
    /// <param name="imageData">imageData</param>
    /// <param name="dx">dx</param>
    /// <param name="dy">dy</param>
    /// <param name="dirtyX">dirtyX</param>
    /// <param name="dirtyY">dirtyY</param>
    /// <param name="dirtyWidth">dirtyWidth</param>
    /// <param name="dirtyHeight">dirtyHeight</param>
    [Description("@#putImageData")]
    public extern void PutImageData(ImageData imageData, int dx, int dy, int dirtyX, int dirtyY, int dirtyWidth, int dirtyHeight);
    #endregion

    #region mixin CanvasPathDrawingStyles
    /// <summary>
    /// lineWidth
    /// </summary>
    [Description("@#lineWidth")]
    public extern double LineWidth { get; set; }

    /// <summary>
    /// lineCap
    /// </summary>
    [Description("@#lineCap")]
    public extern CanvasLineCap LineCap { get; set; }

    /// <summary>
    /// lineJoin
    /// </summary>
    [Description("@#lineJoin")]
    public extern CanvasLineJoin LineJoin { get; set; }

    /// <summary>
    /// miterLimit
    /// </summary>
    [Description("@#miterLimit")]
    public extern double MiterLimit { get; set; }

    /// <summary>
    /// setLineDash
    /// </summary>
    /// <param name="segments">segments</param>
    [Description("@#setLineDash")]
    public extern void SetLineDash(double[] segments);

    /// <summary>
    /// getLineDash
    /// </summary>
    [Description("@#getLineDash")]
    public extern double[] GetLineDash();

    /// <summary>
    /// lineDashOffset
    /// </summary>
    [Description("@#lineDashOffset")]
    public extern double LineDashOffset { get; set; }
    #endregion

    #region mixin CanvasTextDrawingStyles
    /// <summary>
    /// lang
    /// </summary>
    [Description("@#lang")]
    public extern string Lang { get; set; }

    /// <summary>
    /// font
    /// </summary>
    [Description("@#font")]
    public extern string Font { get; set; }

    /// <summary>
    /// textAlign
    /// </summary>
    [Description("@#textAlign")]
    public extern CanvasTextAlign TextAlign { get; set; }

    /// <summary>
    /// textBaseline
    /// </summary>
    [Description("@#textBaseline")]
    public extern CanvasTextBaseline TextBaseline { get; set; }

    /// <summary>
    /// direction
    /// </summary>
    [Description("@#direction")]
    public extern CanvasDirection Direction { get; set; }

    /// <summary>
    /// letterSpacing
    /// </summary>
    [Description("@#letterSpacing")]
    public extern string LetterSpacing { get; set; }

    /// <summary>
    /// fontKerning
    /// </summary>
    [Description("@#fontKerning")]
    public extern CanvasFontKerning FontKerning { get; set; }

    /// <summary>
    /// fontStretch
    /// </summary>
    [Description("@#fontStretch")]
    public extern CanvasFontStretch FontStretch { get; set; }

    /// <summary>
    /// fontVariantCaps
    /// </summary>
    [Description("@#fontVariantCaps")]
    public extern CanvasFontVariantCaps FontVariantCaps { get; set; }

    /// <summary>
    /// textRendering
    /// </summary>
    [Description("@#textRendering")]
    public extern CanvasTextRendering TextRendering { get; set; }

    /// <summary>
    /// wordSpacing
    /// </summary>
    [Description("@#wordSpacing")]
    public extern string WordSpacing { get; set; }
    #endregion

    #region mixin CanvasPath
    /// <summary>
    /// closePath
    /// </summary>
    [Description("@#closePath")]
    public extern void ClosePath();

    /// <summary>
    /// moveTo
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    [Description("@#moveTo")]
    public extern void MoveTo(double x, double y);

    /// <summary>
    /// lineTo
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    [Description("@#lineTo")]
    public extern void LineTo(double x, double y);

    /// <summary>
    /// quadraticCurveTo
    /// </summary>
    /// <param name="cpx">cpx</param>
    /// <param name="cpy">cpy</param>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    [Description("@#quadraticCurveTo")]
    public extern void QuadraticCurveTo(double cpx, double cpy, double x, double y);

    /// <summary>
    /// bezierCurveTo
    /// </summary>
    /// <param name="cp1x">cp1x</param>
    /// <param name="cp1y">cp1y</param>
    /// <param name="cp2x">cp2x</param>
    /// <param name="cp2y">cp2y</param>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    [Description("@#bezierCurveTo")]
    public extern void BezierCurveTo(double cp1x, double cp1y, double cp2x, double cp2y, double x, double y);

    /// <summary>
    /// arcTo
    /// </summary>
    /// <param name="x1">x1</param>
    /// <param name="y1">y1</param>
    /// <param name="x2">x2</param>
    /// <param name="y2">y2</param>
    /// <param name="radius">radius</param>
    [Description("@#arcTo")]
    public extern void ArcTo(double x1, double y1, double x2, double y2, double radius);

    /// <summary>
    /// rect
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="w">w</param>
    /// <param name="h">h</param>
    [Description("@#rect")]
    public extern void Rect(double x, double y, double w, double h);

    /// <summary>
    /// roundRect
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="w">w</param>
    /// <param name="h">h</param>
    /// <param name="radii">radii</param>
    [Description("@#roundRect")]
    public extern void RoundRect(double x, double y, double w, double h, Either<double, DOMPointInit, Either<double, DOMPointInit>[]> radii);

    /// <summary>
    /// roundRect
    /// </summary>
    /// <param name="x">x para</param>
    /// <param name="y">y para</param>
    /// <param name="w">w para</param>
    /// <param name="h">h para</param>
    /// <param name="radii">radii</param>
    [Description("@#roundRect")]
    public extern void RoundRect(double x, double y, double w, double h, double radii);

    /// <summary>
    /// roundRect
    /// </summary>
    /// <param name="x">x para</param>
    /// <param name="y">y para</param>
    /// <param name="w">w para</param>
    /// <param name="h">h para</param>
    /// <param name="radii">radii</param>
    [Description("@#roundRect")]
    public extern void RoundRect(double x, double y, double w, double h, DOMPointInit? radii = default);

    /// <summary>
    /// roundRect
    /// </summary>
    /// <param name="x">x para</param>
    /// <param name="y">y para</param>
    /// <param name="w">w para</param>
    /// <param name="h">h para</param>
    /// <param name="radii">radii</param>
    [Description("@#roundRect")]
    public extern void RoundRect(double x, double y, double w, double h, Either<double, DOMPointInit>[] radii);

    /// <summary>
    /// arc
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="radius">radius</param>
    /// <param name="startAngle">startAngle</param>
    /// <param name="endAngle">endAngle</param>
    /// <param name="counterclockwise">counterclockwise</param>
    [Description("@#arc")]
    public extern void Arc(double x, double y, double radius, double startAngle, double endAngle, bool counterclockwise = false);

    /// <summary>
    /// ellipse
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="radiusX">radiusX</param>
    /// <param name="radiusY">radiusY</param>
    /// <param name="rotation">rotation</param>
    /// <param name="startAngle">startAngle</param>
    /// <param name="endAngle">endAngle</param>
    /// <param name="counterclockwise">counterclockwise</param>
    [Description("@#ellipse")]
    public extern void Ellipse(double x, double y, double radiusX, double radiusY, double rotation, double startAngle, double endAngle, bool counterclockwise = false);
    #endregion
}

/// <summary>
/// CanvasGradient
/// </summary>
[ECMAScript]
[Description("@#CanvasGradient")]
public class CanvasGradient
{
    /// <summary>
    /// addColorStop
    /// </summary>
    /// <param name="offset">offset</param>
    /// <param name="color">color</param>
    [Description("@#addColorStop")]
    public extern void AddColorStop(double offset, string color);
}

/// <summary>
/// CanvasPattern
/// </summary>
[ECMAScript]
[Description("@#CanvasPattern")]
public class CanvasPattern
{
    /// <summary>
    /// setTransform
    /// </summary>
    /// <param name="transform">transform</param>
    [Description("@#setTransform")]
    public extern void SetTransform(DOMMatrix2DInit? transform = default);
}

/// <summary>
/// TextMetrics
/// </summary>
[ECMAScript]
[Description("@#TextMetrics")]
public class TextMetrics
{
    /// <summary>
    /// width
    /// </summary>
    [Description("@#width")]
    public extern double Width { get; }

    /// <summary>
    /// actualBoundingBoxLeft
    /// </summary>
    [Description("@#actualBoundingBoxLeft")]
    public extern double ActualBoundingBoxLeft { get; }

    /// <summary>
    /// actualBoundingBoxRight
    /// </summary>
    [Description("@#actualBoundingBoxRight")]
    public extern double ActualBoundingBoxRight { get; }

    /// <summary>
    /// fontBoundingBoxAscent
    /// </summary>
    [Description("@#fontBoundingBoxAscent")]
    public extern double FontBoundingBoxAscent { get; }

    /// <summary>
    /// fontBoundingBoxDescent
    /// </summary>
    [Description("@#fontBoundingBoxDescent")]
    public extern double FontBoundingBoxDescent { get; }

    /// <summary>
    /// actualBoundingBoxAscent
    /// </summary>
    [Description("@#actualBoundingBoxAscent")]
    public extern double ActualBoundingBoxAscent { get; }

    /// <summary>
    /// actualBoundingBoxDescent
    /// </summary>
    [Description("@#actualBoundingBoxDescent")]
    public extern double ActualBoundingBoxDescent { get; }

    /// <summary>
    /// emHeightAscent
    /// </summary>
    [Description("@#emHeightAscent")]
    public extern double EmHeightAscent { get; }

    /// <summary>
    /// emHeightDescent
    /// </summary>
    [Description("@#emHeightDescent")]
    public extern double EmHeightDescent { get; }

    /// <summary>
    /// hangingBaseline
    /// </summary>
    [Description("@#hangingBaseline")]
    public extern double HangingBaseline { get; }

    /// <summary>
    /// alphabeticBaseline
    /// </summary>
    [Description("@#alphabeticBaseline")]
    public extern double AlphabeticBaseline { get; }

    /// <summary>
    /// ideographicBaseline
    /// </summary>
    [Description("@#ideographicBaseline")]
    public extern double IdeographicBaseline { get; }
}

/// <summary>
/// Path2D
/// </summary>
[ECMAScript]
[Description("@#Path2D")]
public class Path2D
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="path">path</param>
    public extern Path2D(Either<Path2D, string> path);

    /// <summary>
    /// addPath
    /// </summary>
    /// <param name="path">path</param>
    /// <param name="transform">transform</param>
    [Description("@#addPath")]
    public extern void AddPath(Path2D path, DOMMatrix2DInit? transform = default);

    #region mixin CanvasPath
    /// <summary>
    /// closePath
    /// </summary>
    [Description("@#closePath")]
    public extern void ClosePath();

    /// <summary>
    /// moveTo
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    [Description("@#moveTo")]
    public extern void MoveTo(double x, double y);

    /// <summary>
    /// lineTo
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    [Description("@#lineTo")]
    public extern void LineTo(double x, double y);

    /// <summary>
    /// quadraticCurveTo
    /// </summary>
    /// <param name="cpx">cpx</param>
    /// <param name="cpy">cpy</param>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    [Description("@#quadraticCurveTo")]
    public extern void QuadraticCurveTo(double cpx, double cpy, double x, double y);

    /// <summary>
    /// bezierCurveTo
    /// </summary>
    /// <param name="cp1x">cp1x</param>
    /// <param name="cp1y">cp1y</param>
    /// <param name="cp2x">cp2x</param>
    /// <param name="cp2y">cp2y</param>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    [Description("@#bezierCurveTo")]
    public extern void BezierCurveTo(double cp1x, double cp1y, double cp2x, double cp2y, double x, double y);

    /// <summary>
    /// arcTo
    /// </summary>
    /// <param name="x1">x1</param>
    /// <param name="y1">y1</param>
    /// <param name="x2">x2</param>
    /// <param name="y2">y2</param>
    /// <param name="radius">radius</param>
    [Description("@#arcTo")]
    public extern void ArcTo(double x1, double y1, double x2, double y2, double radius);

    /// <summary>
    /// rect
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="w">w</param>
    /// <param name="h">h</param>
    [Description("@#rect")]
    public extern void Rect(double x, double y, double w, double h);

    /// <summary>
    /// roundRect
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="w">w</param>
    /// <param name="h">h</param>
    /// <param name="radii">radii</param>
    [Description("@#roundRect")]
    public extern void RoundRect(double x, double y, double w, double h, Either<double, DOMPointInit, Either<double, DOMPointInit>[]> radii);

    /// <summary>
    /// roundRect
    /// </summary>
    /// <param name="x">x para</param>
    /// <param name="y">y para</param>
    /// <param name="w">w para</param>
    /// <param name="h">h para</param>
    /// <param name="radii">radii</param>
    [Description("@#roundRect")]
    public extern void RoundRect(double x, double y, double w, double h, double radii);

    /// <summary>
    /// roundRect
    /// </summary>
    /// <param name="x">x para</param>
    /// <param name="y">y para</param>
    /// <param name="w">w para</param>
    /// <param name="h">h para</param>
    /// <param name="radii">radii</param>
    [Description("@#roundRect")]
    public extern void RoundRect(double x, double y, double w, double h, DOMPointInit? radii = default);

    /// <summary>
    /// roundRect
    /// </summary>
    /// <param name="x">x para</param>
    /// <param name="y">y para</param>
    /// <param name="w">w para</param>
    /// <param name="h">h para</param>
    /// <param name="radii">radii</param>
    [Description("@#roundRect")]
    public extern void RoundRect(double x, double y, double w, double h, Either<double, DOMPointInit>[] radii);

    /// <summary>
    /// arc
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="radius">radius</param>
    /// <param name="startAngle">startAngle</param>
    /// <param name="endAngle">endAngle</param>
    /// <param name="counterclockwise">counterclockwise</param>
    [Description("@#arc")]
    public extern void Arc(double x, double y, double radius, double startAngle, double endAngle, bool counterclockwise = false);

    /// <summary>
    /// ellipse
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="radiusX">radiusX</param>
    /// <param name="radiusY">radiusY</param>
    /// <param name="rotation">rotation</param>
    /// <param name="startAngle">startAngle</param>
    /// <param name="endAngle">endAngle</param>
    /// <param name="counterclockwise">counterclockwise</param>
    [Description("@#ellipse")]
    public extern void Ellipse(double x, double y, double radiusX, double radiusY, double rotation, double startAngle, double endAngle, bool counterclockwise = false);
    #endregion
}

/// <summary>
/// ImageBitmapRenderingContext
/// </summary>
[ECMAScript]
[Description("@#ImageBitmapRenderingContext")]
public class ImageBitmapRenderingContext
{
    /// <summary>
    /// canvas
    /// </summary>
    [Description("@#canvas")]
    public extern Either<HTMLCanvasElement, OffscreenCanvas> Canvas { get; }

    /// <summary>
    /// transferFromImageBitmap
    /// </summary>
    /// <param name="bitmap">bitmap</param>
    [Description("@#transferFromImageBitmap")]
    public extern void TransferFromImageBitmap(ImageBitmap? bitmap);
}

/// <summary>
/// OffscreenCanvas
/// </summary>
[ECMAScript]
[Description("@#OffscreenCanvas")]
public class OffscreenCanvas : EventTarget
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="width">width</param>
    /// <param name="height">height</param>
    public extern OffscreenCanvas(ulong width, ulong height);

    /// <summary>
    /// width
    /// </summary>
    [Description("@#width")]
    public extern ulong Width { get; set; }

    /// <summary>
    /// height
    /// </summary>
    [Description("@#height")]
    public extern ulong Height { get; set; }

    /// <summary>
    /// getContext
    /// </summary>
    /// <param name="contextId">contextId</param>
    /// <param name="options">options</param>
    [Description("@#getContext")]
    public extern OffscreenRenderingContext? GetContext(OffscreenRenderingContextId contextId, object? options = default);

    /// <summary>
    /// transferToImageBitmap
    /// </summary>
    [Description("@#transferToImageBitmap")]
    public extern ImageBitmap TransferToImageBitmap();

    /// <summary>
    /// convertToBlob
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#convertToBlob")]
    public extern PromiseResult<Blob> ConvertToBlob(ImageEncodeOptions? options = default);

    /// <summary>
    /// oncontextlost
    /// </summary>
    [Description("@#oncontextlost")]
    public extern EventHandler Oncontextlost { get; set; }

    /// <summary>
    /// oncontextrestored
    /// </summary>
    [Description("@#oncontextrestored")]
    public extern EventHandler Oncontextrestored { get; set; }
}

/// <summary>
/// OffscreenCanvasRenderingContext2D
/// </summary>
[ECMAScript]
[Description("@#OffscreenCanvasRenderingContext2D")]
public class OffscreenCanvasRenderingContext2D
{
    /// <summary>
    /// canvas
    /// </summary>
    [Description("@#canvas")]
    public extern OffscreenCanvas Canvas { get; }

    #region mixin CanvasSettings
    /// <summary>
    /// getContextAttributes
    /// </summary>
    [Description("@#getContextAttributes")]
    public extern CanvasRenderingContext2DSettings GetContextAttributes();
    #endregion

    #region mixin CanvasState
    /// <summary>
    /// save
    /// </summary>
    [Description("@#save")]
    public extern void Save();

    /// <summary>
    /// restore
    /// </summary>
    [Description("@#restore")]
    public extern void Restore();

    /// <summary>
    /// reset
    /// </summary>
    [Description("@#reset")]
    public extern void Reset();

    /// <summary>
    /// isContextLost
    /// </summary>
    [Description("@#isContextLost")]
    public extern bool IsContextLost();
    #endregion

    #region mixin CanvasTransform
    /// <summary>
    /// scale
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    [Description("@#scale")]
    public extern void Scale(double x, double y);

    /// <summary>
    /// rotate
    /// </summary>
    /// <param name="angle">angle</param>
    [Description("@#rotate")]
    public extern void Rotate(double angle);

    /// <summary>
    /// translate
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    [Description("@#translate")]
    public extern void Translate(double x, double y);

    /// <summary>
    /// transform
    /// </summary>
    /// <param name="a">a</param>
    /// <param name="b">b</param>
    /// <param name="c">c</param>
    /// <param name="d">d</param>
    /// <param name="e">e</param>
    /// <param name="f">f</param>
    [Description("@#transform")]
    public extern void Transform(double a, double b, double c, double d, double e, double f);

    /// <summary>
    /// getTransform
    /// </summary>
    [Description("@#getTransform")]
    public extern DOMMatrix GetTransform();

    /// <summary>
    /// setTransform
    /// </summary>
    /// <param name="a">a</param>
    /// <param name="b">b</param>
    /// <param name="c">c</param>
    /// <param name="d">d</param>
    /// <param name="e">e</param>
    /// <param name="f">f</param>
    [Description("@#setTransform")]
    public extern void SetTransform(double a, double b, double c, double d, double e, double f);

    /// <summary>
    /// setTransform
    /// </summary>
    /// <param name="transform">transform</param>
    [Description("@#setTransform")]
    public extern void SetTransform(DOMMatrix2DInit? transform = default);

    /// <summary>
    /// resetTransform
    /// </summary>
    [Description("@#resetTransform")]
    public extern void ResetTransform();
    #endregion

    #region mixin CanvasCompositing
    /// <summary>
    /// globalAlpha
    /// </summary>
    [Description("@#globalAlpha")]
    public extern double GlobalAlpha { get; set; }

    /// <summary>
    /// globalCompositeOperation
    /// </summary>
    [Description("@#globalCompositeOperation")]
    public extern string GlobalCompositeOperation { get; set; }
    #endregion

    #region mixin CanvasImageSmoothing
    /// <summary>
    /// imageSmoothingEnabled
    /// </summary>
    [Description("@#imageSmoothingEnabled")]
    public extern bool ImageSmoothingEnabled { get; set; }

    /// <summary>
    /// imageSmoothingQuality
    /// </summary>
    [Description("@#imageSmoothingQuality")]
    public extern ImageSmoothingQuality ImageSmoothingQuality { get; set; }
    #endregion

    #region mixin CanvasFillStrokeStyles
    /// <summary>
    /// strokeStyle
    /// </summary>
    [Description("@#strokeStyle")]
    public extern Either<string, CanvasGradient, CanvasPattern> StrokeStyle { get; set; }

    /// <summary>
    /// fillStyle
    /// </summary>
    [Description("@#fillStyle")]
    public extern Either<string, CanvasGradient, CanvasPattern> FillStyle { get; set; }

    /// <summary>
    /// createLinearGradient
    /// </summary>
    /// <param name="x0">x0</param>
    /// <param name="y0">y0</param>
    /// <param name="x1">x1</param>
    /// <param name="y1">y1</param>
    [Description("@#createLinearGradient")]
    public extern CanvasGradient CreateLinearGradient(double x0, double y0, double x1, double y1);

    /// <summary>
    /// createRadialGradient
    /// </summary>
    /// <param name="x0">x0</param>
    /// <param name="y0">y0</param>
    /// <param name="r0">r0</param>
    /// <param name="x1">x1</param>
    /// <param name="y1">y1</param>
    /// <param name="r1">r1</param>
    [Description("@#createRadialGradient")]
    public extern CanvasGradient CreateRadialGradient(double x0, double y0, double r0, double x1, double y1, double r1);

    /// <summary>
    /// createConicGradient
    /// </summary>
    /// <param name="startAngle">startAngle</param>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    [Description("@#createConicGradient")]
    public extern CanvasGradient CreateConicGradient(double startAngle, double x, double y);

    /// <summary>
    /// createPattern
    /// </summary>
    /// <param name="image">image</param>
    /// <param name="repetition">repetition</param>
    [Description("@#createPattern")]
    public extern CanvasPattern? CreatePattern(CanvasImageSource image, string repetition);
    #endregion

    #region mixin CanvasShadowStyles
    /// <summary>
    /// shadowOffsetX
    /// </summary>
    [Description("@#shadowOffsetX")]
    public extern double ShadowOffsetX { get; set; }

    /// <summary>
    /// shadowOffsetY
    /// </summary>
    [Description("@#shadowOffsetY")]
    public extern double ShadowOffsetY { get; set; }

    /// <summary>
    /// shadowBlur
    /// </summary>
    [Description("@#shadowBlur")]
    public extern double ShadowBlur { get; set; }

    /// <summary>
    /// shadowColor
    /// </summary>
    [Description("@#shadowColor")]
    public extern string ShadowColor { get; set; }
    #endregion

    #region mixin CanvasFilters
    /// <summary>
    /// filter
    /// </summary>
    [Description("@#filter")]
    public extern string Filter { get; set; }
    #endregion

    #region mixin CanvasRect
    /// <summary>
    /// clearRect
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="w">w</param>
    /// <param name="h">h</param>
    [Description("@#clearRect")]
    public extern void ClearRect(double x, double y, double w, double h);

    /// <summary>
    /// fillRect
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="w">w</param>
    /// <param name="h">h</param>
    [Description("@#fillRect")]
    public extern void FillRect(double x, double y, double w, double h);

    /// <summary>
    /// strokeRect
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="w">w</param>
    /// <param name="h">h</param>
    [Description("@#strokeRect")]
    public extern void StrokeRect(double x, double y, double w, double h);
    #endregion

    #region mixin CanvasDrawPath
    /// <summary>
    /// beginPath
    /// </summary>
    [Description("@#beginPath")]
    public extern void BeginPath();

    /// <summary>
    /// fill
    /// </summary>
    /// <param name="fillRule">fillRule</param>
    [Description("@#fill")]
    public extern void Fill(CanvasFillRule fillRule = CanvasFillRule.Nonzero);

    /// <summary>
    /// fill
    /// </summary>
    /// <param name="path">path</param>
    /// <param name="fillRule">fillRule</param>
    [Description("@#fill")]
    public extern void Fill(Path2D path, CanvasFillRule fillRule = CanvasFillRule.Nonzero);

    /// <summary>
    /// stroke
    /// </summary>
    [Description("@#stroke")]
    public extern void Stroke();

    /// <summary>
    /// stroke
    /// </summary>
    /// <param name="path">path</param>
    [Description("@#stroke")]
    public extern void Stroke(Path2D path);

    /// <summary>
    /// clip
    /// </summary>
    /// <param name="fillRule">fillRule</param>
    [Description("@#clip")]
    public extern void Clip(CanvasFillRule fillRule = CanvasFillRule.Nonzero);

    /// <summary>
    /// clip
    /// </summary>
    /// <param name="path">path</param>
    /// <param name="fillRule">fillRule</param>
    [Description("@#clip")]
    public extern void Clip(Path2D path, CanvasFillRule fillRule = CanvasFillRule.Nonzero);

    /// <summary>
    /// isPointInPath
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="fillRule">fillRule</param>
    [Description("@#isPointInPath")]
    public extern bool IsPointInPath(double x, double y, CanvasFillRule fillRule = CanvasFillRule.Nonzero);

    /// <summary>
    /// isPointInPath
    /// </summary>
    /// <param name="path">path</param>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="fillRule">fillRule</param>
    [Description("@#isPointInPath")]
    public extern bool IsPointInPath(Path2D path, double x, double y, CanvasFillRule fillRule = CanvasFillRule.Nonzero);

    /// <summary>
    /// isPointInStroke
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    [Description("@#isPointInStroke")]
    public extern bool IsPointInStroke(double x, double y);

    /// <summary>
    /// isPointInStroke
    /// </summary>
    /// <param name="path">path</param>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    [Description("@#isPointInStroke")]
    public extern bool IsPointInStroke(Path2D path, double x, double y);
    #endregion

    #region mixin CanvasText
    /// <summary>
    /// fillText
    /// </summary>
    /// <param name="text">text</param>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="maxWidth">maxWidth</param>
    [Description("@#fillText")]
    public extern void FillText(string text, double x, double y, double maxWidth);

    /// <summary>
    /// strokeText
    /// </summary>
    /// <param name="text">text</param>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="maxWidth">maxWidth</param>
    [Description("@#strokeText")]
    public extern void StrokeText(string text, double x, double y, double maxWidth);

    /// <summary>
    /// measureText
    /// </summary>
    /// <param name="text">text</param>
    [Description("@#measureText")]
    public extern TextMetrics MeasureText(string text);
    #endregion

    #region mixin CanvasDrawImage
    /// <summary>
    /// drawImage
    /// </summary>
    /// <param name="image">image</param>
    /// <param name="dx">dx</param>
    /// <param name="dy">dy</param>
    [Description("@#drawImage")]
    public extern void DrawImage(CanvasImageSource image, double dx, double dy);

    /// <summary>
    /// drawImage
    /// </summary>
    /// <param name="image">image</param>
    /// <param name="dx">dx</param>
    /// <param name="dy">dy</param>
    /// <param name="dw">dw</param>
    /// <param name="dh">dh</param>
    [Description("@#drawImage")]
    public extern void DrawImage(CanvasImageSource image, double dx, double dy, double dw, double dh);

    /// <summary>
    /// drawImage
    /// </summary>
    /// <param name="image">image</param>
    /// <param name="sx">sx</param>
    /// <param name="sy">sy</param>
    /// <param name="sw">sw</param>
    /// <param name="sh">sh</param>
    /// <param name="dx">dx</param>
    /// <param name="dy">dy</param>
    /// <param name="dw">dw</param>
    /// <param name="dh">dh</param>
    [Description("@#drawImage")]
    public extern void DrawImage(CanvasImageSource image, double sx, double sy, double sw, double sh, double dx, double dy, double dw, double dh);
    #endregion

    #region mixin CanvasImageData
    /// <summary>
    /// createImageData
    /// </summary>
    /// <param name="sw">sw</param>
    /// <param name="sh">sh</param>
    /// <param name="settings">settings</param>
    [Description("@#createImageData")]
    public extern ImageData CreateImageData(int sw, int sh, ImageDataSettings? settings = default);

    /// <summary>
    /// createImageData
    /// </summary>
    /// <param name="imageData">imageData</param>
    [Description("@#createImageData")]
    public extern ImageData CreateImageData(ImageData imageData);

    /// <summary>
    /// getImageData
    /// </summary>
    /// <param name="sx">sx</param>
    /// <param name="sy">sy</param>
    /// <param name="sw">sw</param>
    /// <param name="sh">sh</param>
    /// <param name="settings">settings</param>
    [Description("@#getImageData")]
    public extern ImageData GetImageData(int sx, int sy, int sw, int sh, ImageDataSettings? settings = default);

    /// <summary>
    /// putImageData
    /// </summary>
    /// <param name="imageData">imageData</param>
    /// <param name="dx">dx</param>
    /// <param name="dy">dy</param>
    [Description("@#putImageData")]
    public extern void PutImageData(ImageData imageData, int dx, int dy);

    /// <summary>
    /// putImageData
    /// </summary>
    /// <param name="imageData">imageData</param>
    /// <param name="dx">dx</param>
    /// <param name="dy">dy</param>
    /// <param name="dirtyX">dirtyX</param>
    /// <param name="dirtyY">dirtyY</param>
    /// <param name="dirtyWidth">dirtyWidth</param>
    /// <param name="dirtyHeight">dirtyHeight</param>
    [Description("@#putImageData")]
    public extern void PutImageData(ImageData imageData, int dx, int dy, int dirtyX, int dirtyY, int dirtyWidth, int dirtyHeight);
    #endregion

    #region mixin CanvasPathDrawingStyles
    /// <summary>
    /// lineWidth
    /// </summary>
    [Description("@#lineWidth")]
    public extern double LineWidth { get; set; }

    /// <summary>
    /// lineCap
    /// </summary>
    [Description("@#lineCap")]
    public extern CanvasLineCap LineCap { get; set; }

    /// <summary>
    /// lineJoin
    /// </summary>
    [Description("@#lineJoin")]
    public extern CanvasLineJoin LineJoin { get; set; }

    /// <summary>
    /// miterLimit
    /// </summary>
    [Description("@#miterLimit")]
    public extern double MiterLimit { get; set; }

    /// <summary>
    /// setLineDash
    /// </summary>
    /// <param name="segments">segments</param>
    [Description("@#setLineDash")]
    public extern void SetLineDash(double[] segments);

    /// <summary>
    /// getLineDash
    /// </summary>
    [Description("@#getLineDash")]
    public extern double[] GetLineDash();

    /// <summary>
    /// lineDashOffset
    /// </summary>
    [Description("@#lineDashOffset")]
    public extern double LineDashOffset { get; set; }
    #endregion

    #region mixin CanvasTextDrawingStyles
    /// <summary>
    /// lang
    /// </summary>
    [Description("@#lang")]
    public extern string Lang { get; set; }

    /// <summary>
    /// font
    /// </summary>
    [Description("@#font")]
    public extern string Font { get; set; }

    /// <summary>
    /// textAlign
    /// </summary>
    [Description("@#textAlign")]
    public extern CanvasTextAlign TextAlign { get; set; }

    /// <summary>
    /// textBaseline
    /// </summary>
    [Description("@#textBaseline")]
    public extern CanvasTextBaseline TextBaseline { get; set; }

    /// <summary>
    /// direction
    /// </summary>
    [Description("@#direction")]
    public extern CanvasDirection Direction { get; set; }

    /// <summary>
    /// letterSpacing
    /// </summary>
    [Description("@#letterSpacing")]
    public extern string LetterSpacing { get; set; }

    /// <summary>
    /// fontKerning
    /// </summary>
    [Description("@#fontKerning")]
    public extern CanvasFontKerning FontKerning { get; set; }

    /// <summary>
    /// fontStretch
    /// </summary>
    [Description("@#fontStretch")]
    public extern CanvasFontStretch FontStretch { get; set; }

    /// <summary>
    /// fontVariantCaps
    /// </summary>
    [Description("@#fontVariantCaps")]
    public extern CanvasFontVariantCaps FontVariantCaps { get; set; }

    /// <summary>
    /// textRendering
    /// </summary>
    [Description("@#textRendering")]
    public extern CanvasTextRendering TextRendering { get; set; }

    /// <summary>
    /// wordSpacing
    /// </summary>
    [Description("@#wordSpacing")]
    public extern string WordSpacing { get; set; }
    #endregion

    #region mixin CanvasPath
    /// <summary>
    /// closePath
    /// </summary>
    [Description("@#closePath")]
    public extern void ClosePath();

    /// <summary>
    /// moveTo
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    [Description("@#moveTo")]
    public extern void MoveTo(double x, double y);

    /// <summary>
    /// lineTo
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    [Description("@#lineTo")]
    public extern void LineTo(double x, double y);

    /// <summary>
    /// quadraticCurveTo
    /// </summary>
    /// <param name="cpx">cpx</param>
    /// <param name="cpy">cpy</param>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    [Description("@#quadraticCurveTo")]
    public extern void QuadraticCurveTo(double cpx, double cpy, double x, double y);

    /// <summary>
    /// bezierCurveTo
    /// </summary>
    /// <param name="cp1x">cp1x</param>
    /// <param name="cp1y">cp1y</param>
    /// <param name="cp2x">cp2x</param>
    /// <param name="cp2y">cp2y</param>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    [Description("@#bezierCurveTo")]
    public extern void BezierCurveTo(double cp1x, double cp1y, double cp2x, double cp2y, double x, double y);

    /// <summary>
    /// arcTo
    /// </summary>
    /// <param name="x1">x1</param>
    /// <param name="y1">y1</param>
    /// <param name="x2">x2</param>
    /// <param name="y2">y2</param>
    /// <param name="radius">radius</param>
    [Description("@#arcTo")]
    public extern void ArcTo(double x1, double y1, double x2, double y2, double radius);

    /// <summary>
    /// rect
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="w">w</param>
    /// <param name="h">h</param>
    [Description("@#rect")]
    public extern void Rect(double x, double y, double w, double h);

    /// <summary>
    /// roundRect
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="w">w</param>
    /// <param name="h">h</param>
    /// <param name="radii">radii</param>
    [Description("@#roundRect")]
    public extern void RoundRect(double x, double y, double w, double h, Either<double, DOMPointInit, Either<double, DOMPointInit>[]> radii);

    /// <summary>
    /// roundRect
    /// </summary>
    /// <param name="x">x para</param>
    /// <param name="y">y para</param>
    /// <param name="w">w para</param>
    /// <param name="h">h para</param>
    /// <param name="radii">radii</param>
    [Description("@#roundRect")]
    public extern void RoundRect(double x, double y, double w, double h, double radii);

    /// <summary>
    /// roundRect
    /// </summary>
    /// <param name="x">x para</param>
    /// <param name="y">y para</param>
    /// <param name="w">w para</param>
    /// <param name="h">h para</param>
    /// <param name="radii">radii</param>
    [Description("@#roundRect")]
    public extern void RoundRect(double x, double y, double w, double h, DOMPointInit? radii = default);

    /// <summary>
    /// roundRect
    /// </summary>
    /// <param name="x">x para</param>
    /// <param name="y">y para</param>
    /// <param name="w">w para</param>
    /// <param name="h">h para</param>
    /// <param name="radii">radii</param>
    [Description("@#roundRect")]
    public extern void RoundRect(double x, double y, double w, double h, Either<double, DOMPointInit>[] radii);

    /// <summary>
    /// arc
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="radius">radius</param>
    /// <param name="startAngle">startAngle</param>
    /// <param name="endAngle">endAngle</param>
    /// <param name="counterclockwise">counterclockwise</param>
    [Description("@#arc")]
    public extern void Arc(double x, double y, double radius, double startAngle, double endAngle, bool counterclockwise = false);

    /// <summary>
    /// ellipse
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="radiusX">radiusX</param>
    /// <param name="radiusY">radiusY</param>
    /// <param name="rotation">rotation</param>
    /// <param name="startAngle">startAngle</param>
    /// <param name="endAngle">endAngle</param>
    /// <param name="counterclockwise">counterclockwise</param>
    [Description("@#ellipse")]
    public extern void Ellipse(double x, double y, double radiusX, double radiusY, double rotation, double startAngle, double endAngle, bool counterclockwise = false);
    #endregion
}

/// <summary>
/// CustomElementRegistry
/// </summary>
[ECMAScript]
[Description("@#CustomElementRegistry")]
public class CustomElementRegistry
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern CustomElementRegistry();

    /// <summary>
    /// define
    /// </summary>
    /// <param name="name">name</param>
    /// <param name="constructor">constructor</param>
    /// <param name="options">options</param>
    [Description("@#define")]
    public extern void Define(string name, CustomElementConstructor constructor, ElementDefinitionOptions? options = default);

    /// <summary>
    /// get
    /// </summary>
    /// <param name="name">name</param>
    [Description("@#get")]
    public extern Either<CustomElementConstructor, object> Get(string name);

    /// <summary>
    /// getName
    /// </summary>
    /// <param name="constructor">constructor</param>
    [Description("@#getName")]
    public extern string? GetName(CustomElementConstructor constructor);

    /// <summary>
    /// whenDefined
    /// </summary>
    /// <param name="name">name</param>
    [Description("@#whenDefined")]
    public extern PromiseResult<CustomElementConstructor> WhenDefined(string name);

    /// <summary>
    /// upgrade
    /// </summary>
    /// <param name="root">root</param>
    [Description("@#upgrade")]
    public extern void Upgrade(Node root);

    /// <summary>
    /// initialize
    /// </summary>
    /// <param name="root">root</param>
    [Description("@#initialize")]
    public extern void Initialize(Node root);
}

/// <summary>
/// ElementInternals
/// </summary>
[ECMAScript]
[Description("@#ElementInternals")]
public class ElementInternals
{
    /// <summary>
    /// shadowRoot
    /// </summary>
    [Description("@#shadowRoot")]
    public extern ShadowRoot? ShadowRoot { get; }

    /// <summary>
    /// setFormValue
    /// </summary>
    /// <param name="value">value</param>
    /// <param name="state">state</param>
    [Description("@#setFormValue")]
    public extern void SetFormValue(Either<File, string, FormData>? value, Either<File, string, FormData>? state);

    /// <summary>
    /// setFormValue
    /// </summary>
    /// <param name="value">value para</param>
    /// <param name="state">state</param>
    [Description("@#setFormValue")]
    public extern void SetFormValue(Either<File, string, FormData>? value, File state);

    /// <summary>
    /// setFormValue
    /// </summary>
    /// <param name="value">value para</param>
    /// <param name="state">state</param>
    [Description("@#setFormValue")]
    public extern void SetFormValue(Either<File, string, FormData>? value, string state);

    /// <summary>
    /// setFormValue
    /// </summary>
    /// <param name="value">value para</param>
    /// <param name="state">state</param>
    [Description("@#setFormValue")]
    public extern void SetFormValue(Either<File, string, FormData>? value, FormData state);

    /// <summary>
    /// form
    /// </summary>
    [Description("@#form")]
    public extern HTMLFormElement? Form { get; }

    /// <summary>
    /// setValidity
    /// </summary>
    /// <param name="flags">flags</param>
    /// <param name="message">message</param>
    /// <param name="anchor">anchor</param>
    [Description("@#setValidity")]
    public extern void SetValidity(ValidityStateFlags? flags = default, string? message = default, HTMLElement? anchor = default);

    /// <summary>
    /// willValidate
    /// </summary>
    [Description("@#willValidate")]
    public extern bool WillValidate { get; }

    /// <summary>
    /// validity
    /// </summary>
    [Description("@#validity")]
    public extern ValidityState Validity { get; }

    /// <summary>
    /// validationMessage
    /// </summary>
    [Description("@#validationMessage")]
    public extern string ValidationMessage { get; }

    /// <summary>
    /// checkValidity
    /// </summary>
    [Description("@#checkValidity")]
    public extern bool CheckValidity();

    /// <summary>
    /// reportValidity
    /// </summary>
    [Description("@#reportValidity")]
    public extern bool ReportValidity();

    /// <summary>
    /// labels
    /// </summary>
    [Description("@#labels")]
    public extern NodeList Labels { get; }

    /// <summary>
    /// states
    /// </summary>
    [Description("@#states")]
    public extern CustomStateSet States { get; }

    #region mixin ARIAMixin
    /// <summary>
    /// role
    /// </summary>
    [Description("@#role")]
    public extern string? Role { get; set; }

    /// <summary>
    /// ariaActiveDescendantElement
    /// </summary>
    [Description("@#ariaActiveDescendantElement")]
    public extern Element? AriaActiveDescendantElement { get; set; }

    /// <summary>
    /// ariaAtomic
    /// </summary>
    [Description("@#ariaAtomic")]
    public extern string? AriaAtomic { get; set; }

    /// <summary>
    /// ariaAutoComplete
    /// </summary>
    [Description("@#ariaAutoComplete")]
    public extern string? AriaAutoComplete { get; set; }

    /// <summary>
    /// ariaBrailleLabel
    /// </summary>
    [Description("@#ariaBrailleLabel")]
    public extern string? AriaBrailleLabel { get; set; }

    /// <summary>
    /// ariaBrailleRoleDescription
    /// </summary>
    [Description("@#ariaBrailleRoleDescription")]
    public extern string? AriaBrailleRoleDescription { get; set; }

    /// <summary>
    /// ariaBusy
    /// </summary>
    [Description("@#ariaBusy")]
    public extern string? AriaBusy { get; set; }

    /// <summary>
    /// ariaChecked
    /// </summary>
    [Description("@#ariaChecked")]
    public extern string? AriaChecked { get; set; }

    /// <summary>
    /// ariaColCount
    /// </summary>
    [Description("@#ariaColCount")]
    public extern string? AriaColCount { get; set; }

    /// <summary>
    /// ariaColIndex
    /// </summary>
    [Description("@#ariaColIndex")]
    public extern string? AriaColIndex { get; set; }

    /// <summary>
    /// ariaColIndexText
    /// </summary>
    [Description("@#ariaColIndexText")]
    public extern string? AriaColIndexText { get; set; }

    /// <summary>
    /// ariaColSpan
    /// </summary>
    [Description("@#ariaColSpan")]
    public extern string? AriaColSpan { get; set; }

    /// <summary>
    /// ariaControlsElements
    /// </summary>
    [Description("@#ariaControlsElements")]
    public extern FrozenSet<Element>? AriaControlsElements { get; set; }

    /// <summary>
    /// ariaCurrent
    /// </summary>
    [Description("@#ariaCurrent")]
    public extern string? AriaCurrent { get; set; }

    /// <summary>
    /// ariaDescribedByElements
    /// </summary>
    [Description("@#ariaDescribedByElements")]
    public extern FrozenSet<Element>? AriaDescribedByElements { get; set; }

    /// <summary>
    /// ariaDescription
    /// </summary>
    [Description("@#ariaDescription")]
    public extern string? AriaDescription { get; set; }

    /// <summary>
    /// ariaDetailsElements
    /// </summary>
    [Description("@#ariaDetailsElements")]
    public extern FrozenSet<Element>? AriaDetailsElements { get; set; }

    /// <summary>
    /// ariaDisabled
    /// </summary>
    [Description("@#ariaDisabled")]
    public extern string? AriaDisabled { get; set; }

    /// <summary>
    /// ariaErrorMessageElements
    /// </summary>
    [Description("@#ariaErrorMessageElements")]
    public extern FrozenSet<Element>? AriaErrorMessageElements { get; set; }

    /// <summary>
    /// ariaExpanded
    /// </summary>
    [Description("@#ariaExpanded")]
    public extern string? AriaExpanded { get; set; }

    /// <summary>
    /// ariaFlowToElements
    /// </summary>
    [Description("@#ariaFlowToElements")]
    public extern FrozenSet<Element>? AriaFlowToElements { get; set; }

    /// <summary>
    /// ariaHasPopup
    /// </summary>
    [Description("@#ariaHasPopup")]
    public extern string? AriaHasPopup { get; set; }

    /// <summary>
    /// ariaHidden
    /// </summary>
    [Description("@#ariaHidden")]
    public extern string? AriaHidden { get; set; }

    /// <summary>
    /// ariaInvalid
    /// </summary>
    [Description("@#ariaInvalid")]
    public extern string? AriaInvalid { get; set; }

    /// <summary>
    /// ariaKeyShortcuts
    /// </summary>
    [Description("@#ariaKeyShortcuts")]
    public extern string? AriaKeyShortcuts { get; set; }

    /// <summary>
    /// ariaLabel
    /// </summary>
    [Description("@#ariaLabel")]
    public extern string? AriaLabel { get; set; }

    /// <summary>
    /// ariaLabelledByElements
    /// </summary>
    [Description("@#ariaLabelledByElements")]
    public extern FrozenSet<Element>? AriaLabelledByElements { get; set; }

    /// <summary>
    /// ariaLevel
    /// </summary>
    [Description("@#ariaLevel")]
    public extern string? AriaLevel { get; set; }

    /// <summary>
    /// ariaLive
    /// </summary>
    [Description("@#ariaLive")]
    public extern string? AriaLive { get; set; }

    /// <summary>
    /// ariaModal
    /// </summary>
    [Description("@#ariaModal")]
    public extern string? AriaModal { get; set; }

    /// <summary>
    /// ariaMultiLine
    /// </summary>
    [Description("@#ariaMultiLine")]
    public extern string? AriaMultiLine { get; set; }

    /// <summary>
    /// ariaMultiSelectable
    /// </summary>
    [Description("@#ariaMultiSelectable")]
    public extern string? AriaMultiSelectable { get; set; }

    /// <summary>
    /// ariaOrientation
    /// </summary>
    [Description("@#ariaOrientation")]
    public extern string? AriaOrientation { get; set; }

    /// <summary>
    /// ariaOwnsElements
    /// </summary>
    [Description("@#ariaOwnsElements")]
    public extern FrozenSet<Element>? AriaOwnsElements { get; set; }

    /// <summary>
    /// ariaPlaceholder
    /// </summary>
    [Description("@#ariaPlaceholder")]
    public extern string? AriaPlaceholder { get; set; }

    /// <summary>
    /// ariaPosInSet
    /// </summary>
    [Description("@#ariaPosInSet")]
    public extern string? AriaPosInSet { get; set; }

    /// <summary>
    /// ariaPressed
    /// </summary>
    [Description("@#ariaPressed")]
    public extern string? AriaPressed { get; set; }

    /// <summary>
    /// ariaReadOnly
    /// </summary>
    [Description("@#ariaReadOnly")]
    public extern string? AriaReadOnly { get; set; }

    /// <summary>
    /// ariaRelevant
    /// </summary>
    [Description("@#ariaRelevant")]
    public extern string? AriaRelevant { get; set; }

    /// <summary>
    /// ariaRequired
    /// </summary>
    [Description("@#ariaRequired")]
    public extern string? AriaRequired { get; set; }

    /// <summary>
    /// ariaRoleDescription
    /// </summary>
    [Description("@#ariaRoleDescription")]
    public extern string? AriaRoleDescription { get; set; }

    /// <summary>
    /// ariaRowCount
    /// </summary>
    [Description("@#ariaRowCount")]
    public extern string? AriaRowCount { get; set; }

    /// <summary>
    /// ariaRowIndex
    /// </summary>
    [Description("@#ariaRowIndex")]
    public extern string? AriaRowIndex { get; set; }

    /// <summary>
    /// ariaRowIndexText
    /// </summary>
    [Description("@#ariaRowIndexText")]
    public extern string? AriaRowIndexText { get; set; }

    /// <summary>
    /// ariaRowSpan
    /// </summary>
    [Description("@#ariaRowSpan")]
    public extern string? AriaRowSpan { get; set; }

    /// <summary>
    /// ariaSelected
    /// </summary>
    [Description("@#ariaSelected")]
    public extern string? AriaSelected { get; set; }

    /// <summary>
    /// ariaSetSize
    /// </summary>
    [Description("@#ariaSetSize")]
    public extern string? AriaSetSize { get; set; }

    /// <summary>
    /// ariaSort
    /// </summary>
    [Description("@#ariaSort")]
    public extern string? AriaSort { get; set; }

    /// <summary>
    /// ariaValueMax
    /// </summary>
    [Description("@#ariaValueMax")]
    public extern string? AriaValueMax { get; set; }

    /// <summary>
    /// ariaValueMin
    /// </summary>
    [Description("@#ariaValueMin")]
    public extern string? AriaValueMin { get; set; }

    /// <summary>
    /// ariaValueNow
    /// </summary>
    [Description("@#ariaValueNow")]
    public extern string? AriaValueNow { get; set; }

    /// <summary>
    /// ariaValueText
    /// </summary>
    [Description("@#ariaValueText")]
    public extern string? AriaValueText { get; set; }
    #endregion
}

/// <summary>
/// CustomStateSet
/// </summary>
[ECMAScript]
[Description("@#CustomStateSet")]
public class CustomStateSet : ISet<string>
{
    #region Set
    extern int ICollection<string>.Count { get; }
    extern bool ICollection<string>.IsReadOnly { get; }
    extern bool ISet<string>.Add(string item);
    extern void ICollection<string>.Clear();
    extern bool ICollection<string>.Contains(string item);
    extern void ICollection<string>.CopyTo(string[] array, int arrayIndex);
    extern void ISet<string>.ExceptWith(IEnumerable<string> other);
    extern IEnumerator<string> IEnumerable<string>.GetEnumerator();
    extern void ISet<string>.IntersectWith(IEnumerable<string> other);
    extern bool ISet<string>.IsProperSubsetOf(IEnumerable<string> other);
    extern bool ISet<string>.IsProperSupersetOf(IEnumerable<string> other);
    extern bool ISet<string>.IsSubsetOf(IEnumerable<string> other);
    extern bool ISet<string>.IsSupersetOf(IEnumerable<string> other);
    extern bool ISet<string>.Overlaps(IEnumerable<string> other);
    extern bool ICollection<string>.Remove(string item);
    extern bool ISet<string>.SetEquals(IEnumerable<string> other);
    extern void ISet<string>.SymmetricExceptWith(IEnumerable<string> other);
    extern void ISet<string>.UnionWith(IEnumerable<string> other);
    extern void ICollection<string>.Add(string item);
    extern IEnumerator IEnumerable.GetEnumerator();
    #endregion
}

/// <summary>
/// VisibilityStateEntry
/// </summary>
[ECMAScript]
[Description("@#VisibilityStateEntry")]
public class VisibilityStateEntry : PerformanceEntry
{
    /// <summary>
    /// duration
    /// </summary>
    [Description("@#duration")]
    public extern new uint Duration { get; }
}

/// <summary>
/// UserActivation
/// </summary>
[ECMAScript]
[Description("@#UserActivation")]
public class UserActivation
{
    /// <summary>
    /// hasBeenActive
    /// </summary>
    [Description("@#hasBeenActive")]
    public extern bool HasBeenActive { get; }

    /// <summary>
    /// isActive
    /// </summary>
    [Description("@#isActive")]
    public extern bool IsActive { get; }
}

/// <summary>
/// ToggleEvent
/// </summary>
[ECMAScript]
[Description("@#ToggleEvent")]
public class ToggleEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern ToggleEvent(string type, ToggleEventInit eventInitDict);

    /// <summary>
    /// oldState
    /// </summary>
    [Description("@#oldState")]
    public extern string OldState { get; }

    /// <summary>
    /// newState
    /// </summary>
    [Description("@#newState")]
    public extern string NewState { get; }

    /// <summary>
    /// source
    /// </summary>
    [Description("@#source")]
    public extern Element? Source { get; }
}

/// <summary>
/// CommandEvent
/// </summary>
[ECMAScript]
[Description("@#CommandEvent")]
public class CommandEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern CommandEvent(string type, CommandEventInit eventInitDict);

    /// <summary>
    /// source
    /// </summary>
    [Description("@#source")]
    public extern Element? Source { get; }

    /// <summary>
    /// command
    /// </summary>
    [Description("@#command")]
    public extern string Command { get; }
}

/// <summary>
/// CloseWatcher
/// </summary>
[ECMAScript]
[Description("@#CloseWatcher")]
public class CloseWatcher : EventTarget
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="options">options</param>
    public extern CloseWatcher(CloseWatcherOptions options);

    /// <summary>
    /// requestClose
    /// </summary>
    [Description("@#requestClose")]
    public extern void RequestClose();

    /// <summary>
    /// close
    /// </summary>
    [Description("@#close")]
    public extern void Close();

    /// <summary>
    /// destroy
    /// </summary>
    [Description("@#destroy")]
    public extern void Destroy();

    /// <summary>
    /// oncancel
    /// </summary>
    [Description("@#oncancel")]
    public extern EventHandler Oncancel { get; set; }

    /// <summary>
    /// onclose
    /// </summary>
    [Description("@#onclose")]
    public extern EventHandler Onclose { get; set; }
}

/// <summary>
/// DataTransfer
/// </summary>
[ECMAScript]
[Description("@#DataTransfer")]
public class DataTransfer
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern DataTransfer();

    /// <summary>
    /// dropEffect
    /// </summary>
    [Description("@#dropEffect")]
    public extern string DropEffect { get; set; }

    /// <summary>
    /// effectAllowed
    /// </summary>
    [Description("@#effectAllowed")]
    public extern string EffectAllowed { get; set; }

    /// <summary>
    /// items
    /// </summary>
    [Description("@#items")]
    public extern DataTransferItemList Items { get; }

    /// <summary>
    /// setDragImage
    /// </summary>
    /// <param name="image">image</param>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    [Description("@#setDragImage")]
    public extern void SetDragImage(Element image, int x, int y);

    /// <summary>
    /// types
    /// </summary>
    [Description("@#types")]
    public extern FrozenSet<string> Types { get; }

    /// <summary>
    /// getData
    /// </summary>
    /// <param name="format">format</param>
    [Description("@#getData")]
    public extern string GetData(string format);

    /// <summary>
    /// setData
    /// </summary>
    /// <param name="format">format</param>
    /// <param name="data">data</param>
    [Description("@#setData")]
    public extern void SetData(string format, string data);

    /// <summary>
    /// clearData
    /// </summary>
    /// <param name="format">format</param>
    [Description("@#clearData")]
    public extern void ClearData(string format);

    /// <summary>
    /// files
    /// </summary>
    [Description("@#files")]
    public extern FileList Files { get; }
}

/// <summary>
/// DataTransferItemList
/// </summary>
[ECMAScript]
[Description("@#DataTransferItemList")]
public class DataTransferItemList
{
    /// <summary>
    /// length
    /// </summary>
    [Description("@#length")]
    public extern uint Length { get; }

    [Description("@#")]
    public extern DataTransferItem this[uint index] { get; }

    /// <summary>
    /// add
    /// </summary>
    /// <param name="data">data</param>
    /// <param name="type">type</param>
    [Description("@#add")]
    public extern DataTransferItem? Add(string data, string type);

    /// <summary>
    /// add
    /// </summary>
    /// <param name="data">data</param>
    [Description("@#add")]
    public extern DataTransferItem? Add(File data);

    /// <summary>
    /// remove
    /// </summary>
    /// <param name="index">index</param>
    [Description("@#remove")]
    public extern void Remove(uint index);

    /// <summary>
    /// clear
    /// </summary>
    [Description("@#clear")]
    public extern void Clear();
}

/// <summary>
/// DragEvent
/// </summary>
[ECMAScript]
[Description("@#DragEvent")]
public class DragEvent(string type, MouseEventInit eventInitDict) : MouseEvent(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern DragEvent(string type, DragEventInit eventInitDict);

    /// <summary>
    /// dataTransfer
    /// </summary>
    [Description("@#dataTransfer")]
    public extern DataTransfer? DataTransfer { get; }
}

/// <summary>
/// BarProp
/// </summary>
[ECMAScript]
[Description("@#BarProp")]
public class BarProp
{
    /// <summary>
    /// visible
    /// </summary>
    [Description("@#visible")]
    public extern bool Visible { get; }
}

/// <summary>
/// Location
/// </summary>
[ECMAScript]
[Description("@#Location")]
public class Location
{
    /// <summary>
    /// href
    /// </summary>
    [Description("@#href")]
    public extern string Href { get; set; }

    /// <summary>
    /// origin
    /// </summary>
    [Description("@#origin")]
    public extern string Origin { get; }

    /// <summary>
    /// protocol
    /// </summary>
    [Description("@#protocol")]
    public extern string Protocol { get; set; }

    /// <summary>
    /// host
    /// </summary>
    [Description("@#host")]
    public extern string Host { get; set; }

    /// <summary>
    /// hostname
    /// </summary>
    [Description("@#hostname")]
    public extern string Hostname { get; set; }

    /// <summary>
    /// port
    /// </summary>
    [Description("@#port")]
    public extern string Port { get; set; }

    /// <summary>
    /// pathname
    /// </summary>
    [Description("@#pathname")]
    public extern string Pathname { get; set; }

    /// <summary>
    /// search
    /// </summary>
    [Description("@#search")]
    public extern string Search { get; set; }

    /// <summary>
    /// hash
    /// </summary>
    [Description("@#hash")]
    public extern string Hash { get; set; }

    /// <summary>
    /// assign
    /// </summary>
    /// <param name="url">url</param>
    [Description("@#assign")]
    public extern void Assign(string url);

    /// <summary>
    /// replace
    /// </summary>
    /// <param name="url">url</param>
    [Description("@#replace")]
    public extern void Replace(string url);

    /// <summary>
    /// reload
    /// </summary>
    [Description("@#reload")]
    public extern void Reload();

    /// <summary>
    /// ancestorOrigins
    /// </summary>
    [Description("@#ancestorOrigins")]
    public extern DOMStringList AncestorOrigins { get; }
}

/// <summary>
/// History
/// </summary>
[ECMAScript]
[Description("@#History")]
public class History
{
    /// <summary>
    /// length
    /// </summary>
    [Description("@#length")]
    public extern uint Length { get; }

    /// <summary>
    /// scrollRestoration
    /// </summary>
    [Description("@#scrollRestoration")]
    public extern ScrollRestoration ScrollRestoration { get; set; }

    /// <summary>
    /// state
    /// </summary>
    [Description("@#state")]
    public extern object State { get; }

    /// <summary>
    /// go
    /// </summary>
    /// <param name="delta">delta</param>
    [Description("@#go")]
    public extern void Go(int delta = 0);

    /// <summary>
    /// back
    /// </summary>
    [Description("@#back")]
    public extern void Back();

    /// <summary>
    /// forward
    /// </summary>
    [Description("@#forward")]
    public extern void Forward();

    /// <summary>
    /// pushState
    /// </summary>
    /// <param name="data">data</param>
    /// <param name="unused">unused</param>
    /// <param name="url">url</param>
    [Description("@#pushState")]
    public extern void PushState(object data, string unused, string? url = null);

    /// <summary>
    /// replaceState
    /// </summary>
    /// <param name="data">data</param>
    /// <param name="unused">unused</param>
    /// <param name="url">url</param>
    [Description("@#replaceState")]
    public extern void ReplaceState(object data, string unused, string? url = null);
}

/// <summary>
/// Navigation
/// </summary>
[ECMAScript]
[Description("@#Navigation")]
public class Navigation : EventTarget
{
    /// <summary>
    /// entries
    /// </summary>
    [Description("@#entries")]
    public extern NavigationHistoryEntry[] Entries();

    /// <summary>
    /// currentEntry
    /// </summary>
    [Description("@#currentEntry")]
    public extern NavigationHistoryEntry? CurrentEntry { get; }

    /// <summary>
    /// updateCurrentEntry
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#updateCurrentEntry")]
    public extern void UpdateCurrentEntry(NavigationUpdateCurrentEntryOptions options);

    /// <summary>
    /// transition
    /// </summary>
    [Description("@#transition")]
    public extern NavigationTransition? Transition { get; }

    /// <summary>
    /// activation
    /// </summary>
    [Description("@#activation")]
    public extern NavigationActivation? Activation { get; }

    /// <summary>
    /// canGoBack
    /// </summary>
    [Description("@#canGoBack")]
    public extern bool CanGoBack { get; }

    /// <summary>
    /// canGoForward
    /// </summary>
    [Description("@#canGoForward")]
    public extern bool CanGoForward { get; }

    /// <summary>
    /// navigate
    /// </summary>
    /// <param name="url">url</param>
    /// <param name="options">options</param>
    [Description("@#navigate")]
    public extern NavigationResult Navigate(string url, NavigationNavigateOptions? options = default);

    /// <summary>
    /// reload
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#reload")]
    public extern NavigationResult Reload(NavigationReloadOptions? options = default);

    /// <summary>
    /// traverseTo
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="options">options</param>
    [Description("@#traverseTo")]
    public extern NavigationResult TraverseTo(string key, NavigationOptions? options = default);

    /// <summary>
    /// back
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#back")]
    public extern NavigationResult Back(NavigationOptions? options = default);

    /// <summary>
    /// forward
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#forward")]
    public extern NavigationResult Forward(NavigationOptions? options = default);

    /// <summary>
    /// onnavigate
    /// </summary>
    [Description("@#onnavigate")]
    public extern EventHandler Onnavigate { get; set; }

    /// <summary>
    /// onnavigatesuccess
    /// </summary>
    [Description("@#onnavigatesuccess")]
    public extern EventHandler Onnavigatesuccess { get; set; }

    /// <summary>
    /// onnavigateerror
    /// </summary>
    [Description("@#onnavigateerror")]
    public extern EventHandler Onnavigateerror { get; set; }

    /// <summary>
    /// oncurrententrychange
    /// </summary>
    [Description("@#oncurrententrychange")]
    public extern EventHandler Oncurrententrychange { get; set; }
}

/// <summary>
/// NavigationHistoryEntry
/// </summary>
[ECMAScript]
[Description("@#NavigationHistoryEntry")]
public class NavigationHistoryEntry : EventTarget
{
    /// <summary>
    /// url
    /// </summary>
    [Description("@#url")]
    public extern string? Url { get; }

    /// <summary>
    /// key
    /// </summary>
    [Description("@#key")]
    public extern string Key { get; }

    /// <summary>
    /// id
    /// </summary>
    [Description("@#id")]
    public extern string Id { get; }

    /// <summary>
    /// index
    /// </summary>
    [Description("@#index")]
    public extern long Index { get; }

    /// <summary>
    /// sameDocument
    /// </summary>
    [Description("@#sameDocument")]
    public extern bool SameDocument { get; }

    /// <summary>
    /// getState
    /// </summary>
    [Description("@#getState")]
    public extern object GetState();

    /// <summary>
    /// ondispose
    /// </summary>
    [Description("@#ondispose")]
    public extern EventHandler Ondispose { get; set; }
}

/// <summary>
/// NavigationTransition
/// </summary>
[ECMAScript]
[Description("@#NavigationTransition")]
public class NavigationTransition
{
    /// <summary>
    /// navigationType
    /// </summary>
    [Description("@#navigationType")]
    public extern NavigationType NavigationType { get; }

    /// <summary>
    /// from
    /// </summary>
    [Description("@#from")]
    public extern NavigationHistoryEntry From { get; }

    /// <summary>
    /// finished
    /// </summary>
    [Description("@#finished")]
    public extern PromiseResult<object> Finished { get; }
}

/// <summary>
/// NavigationActivation
/// </summary>
[ECMAScript]
[Description("@#NavigationActivation")]
public class NavigationActivation
{
    /// <summary>
    /// from
    /// </summary>
    [Description("@#from")]
    public extern NavigationHistoryEntry? From { get; }

    /// <summary>
    /// entry
    /// </summary>
    [Description("@#entry")]
    public extern NavigationHistoryEntry Entry { get; }

    /// <summary>
    /// navigationType
    /// </summary>
    [Description("@#navigationType")]
    public extern NavigationType NavigationType { get; }
}

/// <summary>
/// NavigateEvent
/// </summary>
[ECMAScript]
[Description("@#NavigateEvent")]
public class NavigateEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern NavigateEvent(string type, NavigateEventInit eventInitDict);

    /// <summary>
    /// navigationType
    /// </summary>
    [Description("@#navigationType")]
    public extern NavigationType NavigationType { get; }

    /// <summary>
    /// destination
    /// </summary>
    [Description("@#destination")]
    public extern NavigationDestination Destination { get; }

    /// <summary>
    /// canIntercept
    /// </summary>
    [Description("@#canIntercept")]
    public extern bool CanIntercept { get; }

    /// <summary>
    /// userInitiated
    /// </summary>
    [Description("@#userInitiated")]
    public extern bool UserInitiated { get; }

    /// <summary>
    /// hashChange
    /// </summary>
    [Description("@#hashChange")]
    public extern bool HashChange { get; }

    /// <summary>
    /// signal
    /// </summary>
    [Description("@#signal")]
    public extern AbortSignal Signal { get; }

    /// <summary>
    /// formData
    /// </summary>
    [Description("@#formData")]
    public extern FormData? FormData { get; }

    /// <summary>
    /// downloadRequest
    /// </summary>
    [Description("@#downloadRequest")]
    public extern string? DownloadRequest { get; }

    /// <summary>
    /// info
    /// </summary>
    [Description("@#info")]
    public extern object Info { get; }

    /// <summary>
    /// hasUAVisualTransition
    /// </summary>
    [Description("@#hasUAVisualTransition")]
    public extern bool HasUAVisualTransition { get; }

    /// <summary>
    /// sourceElement
    /// </summary>
    [Description("@#sourceElement")]
    public extern Element? SourceElement { get; }

    /// <summary>
    /// intercept
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#intercept")]
    public extern void Intercept(NavigationInterceptOptions? options = default);

    /// <summary>
    /// scroll
    /// </summary>
    [Description("@#scroll")]
    public extern void Scroll();
}

/// <summary>
/// NavigationDestination
/// </summary>
[ECMAScript]
[Description("@#NavigationDestination")]
public class NavigationDestination
{
    /// <summary>
    /// url
    /// </summary>
    [Description("@#url")]
    public extern string Url { get; }

    /// <summary>
    /// key
    /// </summary>
    [Description("@#key")]
    public extern string Key { get; }

    /// <summary>
    /// id
    /// </summary>
    [Description("@#id")]
    public extern string Id { get; }

    /// <summary>
    /// index
    /// </summary>
    [Description("@#index")]
    public extern long Index { get; }

    /// <summary>
    /// sameDocument
    /// </summary>
    [Description("@#sameDocument")]
    public extern bool SameDocument { get; }

    /// <summary>
    /// getState
    /// </summary>
    [Description("@#getState")]
    public extern object GetState();
}

/// <summary>
/// NavigationCurrentEntryChangeEvent
/// </summary>
[ECMAScript]
[Description("@#NavigationCurrentEntryChangeEvent")]
public class NavigationCurrentEntryChangeEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern NavigationCurrentEntryChangeEvent(string type, NavigationCurrentEntryChangeEventInit eventInitDict);

    /// <summary>
    /// navigationType
    /// </summary>
    [Description("@#navigationType")]
    public extern NavigationType? NavigationType { get; }

    /// <summary>
    /// from
    /// </summary>
    [Description("@#from")]
    public extern NavigationHistoryEntry From { get; }
}

/// <summary>
/// PopStateEvent
/// </summary>
[ECMAScript]
[Description("@#PopStateEvent")]
public class PopStateEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern PopStateEvent(string type, PopStateEventInit eventInitDict);

    /// <summary>
    /// state
    /// </summary>
    [Description("@#state")]
    public extern object State { get; }

    /// <summary>
    /// hasUAVisualTransition
    /// </summary>
    [Description("@#hasUAVisualTransition")]
    public extern bool HasUAVisualTransition { get; }
}

/// <summary>
/// HashChangeEvent
/// </summary>
[ECMAScript]
[Description("@#HashChangeEvent")]
public class HashChangeEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern HashChangeEvent(string type, HashChangeEventInit eventInitDict);

    /// <summary>
    /// oldURL
    /// </summary>
    [Description("@#oldURL")]
    public extern string OldURL { get; }

    /// <summary>
    /// newURL
    /// </summary>
    [Description("@#newURL")]
    public extern string NewURL { get; }
}

/// <summary>
/// PageSwapEvent
/// </summary>
[ECMAScript]
[Description("@#PageSwapEvent")]
public class PageSwapEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern PageSwapEvent(string type, PageSwapEventInit eventInitDict);

    /// <summary>
    /// activation
    /// </summary>
    [Description("@#activation")]
    public extern NavigationActivation? Activation { get; }

    /// <summary>
    /// viewTransition
    /// </summary>
    [Description("@#viewTransition")]
    public extern ViewTransition? ViewTransition { get; }
}

/// <summary>
/// PageRevealEvent
/// </summary>
[ECMAScript]
[Description("@#PageRevealEvent")]
public class PageRevealEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern PageRevealEvent(string type, PageRevealEventInit eventInitDict);

    /// <summary>
    /// viewTransition
    /// </summary>
    [Description("@#viewTransition")]
    public extern ViewTransition? ViewTransition { get; }
}

/// <summary>
/// PageTransitionEvent
/// </summary>
[ECMAScript]
[Description("@#PageTransitionEvent")]
public class PageTransitionEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern PageTransitionEvent(string type, PageTransitionEventInit eventInitDict);

    /// <summary>
    /// persisted
    /// </summary>
    [Description("@#persisted")]
    public extern bool Persisted { get; }
}

/// <summary>
/// BeforeUnloadEvent
/// </summary>
[ECMAScript]
[Description("@#BeforeUnloadEvent")]
public class BeforeUnloadEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// returnValue
    /// </summary>
    [Description("@#returnValue")]
    public extern new string ReturnValue { get; set; }
}

/// <summary>
/// NotRestoredReasonDetails
/// </summary>
[ECMAScript]
[Description("@#NotRestoredReasonDetails")]
public class NotRestoredReasonDetails
{
    /// <summary>
    /// reason
    /// </summary>
    [Description("@#reason")]
    public extern string Reason { get; }

    /// <summary>
    /// toJSON
    /// </summary>
    [Description("@#toJSON")]
    public extern object ToJSON();
}

/// <summary>
/// NotRestoredReasons
/// </summary>
[ECMAScript]
[Description("@#NotRestoredReasons")]
public class NotRestoredReasons
{
    /// <summary>
    /// src
    /// </summary>
    [Description("@#src")]
    public extern string? Src { get; }

    /// <summary>
    /// id
    /// </summary>
    [Description("@#id")]
    public extern string? Id { get; }

    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string? Name { get; }

    /// <summary>
    /// url
    /// </summary>
    [Description("@#url")]
    public extern string? Url { get; }

    /// <summary>
    /// reasons
    /// </summary>
    [Description("@#reasons")]
    public extern FrozenSet<NotRestoredReasonDetails>? Reasons { get; }

    /// <summary>
    /// children
    /// </summary>
    [Description("@#children")]
    public extern FrozenSet<NotRestoredReasons>? Children { get; }

    /// <summary>
    /// toJSON
    /// </summary>
    [Description("@#toJSON")]
    public extern object ToJSON();
}

/// <summary>
/// ErrorEvent
/// </summary>
[ECMAScript]
[Description("@#ErrorEvent")]
public class ErrorEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern ErrorEvent(string type, ErrorEventInit eventInitDict);

    /// <summary>
    /// message
    /// </summary>
    [Description("@#message")]
    public extern string Message { get; }

    /// <summary>
    /// filename
    /// </summary>
    [Description("@#filename")]
    public extern string Filename { get; }

    /// <summary>
    /// lineno
    /// </summary>
    [Description("@#lineno")]
    public extern uint Lineno { get; }

    /// <summary>
    /// colno
    /// </summary>
    [Description("@#colno")]
    public extern uint Colno { get; }

    /// <summary>
    /// error
    /// </summary>
    [Description("@#error")]
    public extern object Error { get; }
}

/// <summary>
/// PromiseRejectionEvent
/// </summary>
[ECMAScript]
[Description("@#PromiseRejectionEvent")]
public class PromiseRejectionEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern PromiseRejectionEvent(string type, PromiseRejectionEventInit eventInitDict);

    /// <summary>
    /// promise
    /// </summary>
    [Description("@#promise")]
    public extern object Promise { get; }

    /// <summary>
    /// reason
    /// </summary>
    [Description("@#reason")]
    public extern object Reason { get; }
}

/// <summary>
/// DOMParser
/// </summary>
[ECMAScript]
[Description("@#DOMParser")]
public class DOMParser
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern DOMParser();

    /// <summary>
    /// parseFromString
    /// </summary>
    /// <param name="string">string</param>
    /// <param name="type">type</param>
    [Description("@#parseFromString")]
    public extern Document ParseFromString(Either<TrustedHTML, string> @string, DOMParserSupportedType type);
}

/// <summary>
/// XMLSerializer
/// </summary>
[ECMAScript]
[Description("@#XMLSerializer")]
public class XMLSerializer
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern XMLSerializer();

    /// <summary>
    /// serializeToString
    /// </summary>
    /// <param name="root">root</param>
    [Description("@#serializeToString")]
    public extern string SerializeToString(Node root);
}

/// <summary>
/// PluginArray
/// </summary>
[ECMAScript]
[Description("@#PluginArray")]
public class PluginArray
{
    /// <summary>
    /// refresh
    /// </summary>
    [Description("@#refresh")]
    public extern void Refresh();

    /// <summary>
    /// length
    /// </summary>
    [Description("@#length")]
    public extern uint Length { get; }

    /// <summary>
    /// item
    /// </summary>
    /// <param name="index">index</param>
    [Description("@#item")]
    public extern Plugin? GetItem(uint index);

    /// <summary>
    /// namedItem
    /// </summary>
    /// <param name="name">name</param>
    [Description("@#namedItem")]
    public extern Plugin? NamedItem(string name);
}

/// <summary>
/// MimeTypeArray
/// </summary>
[ECMAScript]
[Description("@#MimeTypeArray")]
public class MimeTypeArray
{
    /// <summary>
    /// length
    /// </summary>
    [Description("@#length")]
    public extern uint Length { get; }

    /// <summary>
    /// item
    /// </summary>
    /// <param name="index">index</param>
    [Description("@#item")]
    public extern MimeType? GetItem(uint index);

    /// <summary>
    /// namedItem
    /// </summary>
    /// <param name="name">name</param>
    [Description("@#namedItem")]
    public extern MimeType? NamedItem(string name);
}

/// <summary>
/// Plugin
/// </summary>
[ECMAScript]
[Description("@#Plugin")]
public class Plugin
{
    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; }

    /// <summary>
    /// description
    /// </summary>
    [Description("@#description")]
    public extern string Description { get; }

    /// <summary>
    /// filename
    /// </summary>
    [Description("@#filename")]
    public extern string Filename { get; }

    /// <summary>
    /// length
    /// </summary>
    [Description("@#length")]
    public extern uint Length { get; }

    /// <summary>
    /// item
    /// </summary>
    /// <param name="index">index</param>
    [Description("@#item")]
    public extern MimeType? GetItem(uint index);

    /// <summary>
    /// namedItem
    /// </summary>
    /// <param name="name">name</param>
    [Description("@#namedItem")]
    public extern MimeType? NamedItem(string name);
}

/// <summary>
/// MimeType
/// </summary>
[ECMAScript]
[Description("@#MimeType")]
public class MimeType
{
    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern string Type { get; }

    /// <summary>
    /// description
    /// </summary>
    [Description("@#description")]
    public extern string Description { get; }

    /// <summary>
    /// suffixes
    /// </summary>
    [Description("@#suffixes")]
    public extern string Suffixes { get; }

    /// <summary>
    /// enabledPlugin
    /// </summary>
    [Description("@#enabledPlugin")]
    public extern Plugin EnabledPlugin { get; }
}

/// <summary>
/// ImageData
/// </summary>
[ECMAScript]
[Description("@#ImageData")]
public class ImageData
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="sw">sw</param>
    /// <param name="sh">sh</param>
    /// <param name="settings">settings</param>
    public extern ImageData(uint sw, uint sh, ImageDataSettings settings);

    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="data">data</param>
    /// <param name="sw">sw</param>
    /// <param name="sh">sh</param>
    /// <param name="settings">settings</param>
    public extern ImageData(ImageDataArray data, uint sw, uint sh, ImageDataSettings settings);

    /// <summary>
    /// width
    /// </summary>
    [Description("@#width")]
    public extern uint Width { get; }

    /// <summary>
    /// height
    /// </summary>
    [Description("@#height")]
    public extern uint Height { get; }

    /// <summary>
    /// data
    /// </summary>
    [Description("@#data")]
    public extern ImageDataArray Data { get; }

    /// <summary>
    /// pixelFormat
    /// </summary>
    [Description("@#pixelFormat")]
    public extern ImageDataPixelFormat PixelFormat { get; }

    /// <summary>
    /// colorSpace
    /// </summary>
    [Description("@#colorSpace")]
    public extern PredefinedColorSpace ColorSpace { get; }
}

/// <summary>
/// ImageBitmap
/// </summary>
[ECMAScript]
[Description("@#ImageBitmap")]
public class ImageBitmap
{
    /// <summary>
    /// width
    /// </summary>
    [Description("@#width")]
    public extern uint Width { get; }

    /// <summary>
    /// height
    /// </summary>
    [Description("@#height")]
    public extern uint Height { get; }

    /// <summary>
    /// close
    /// </summary>
    [Description("@#close")]
    public extern void Close();
}

/// <summary>
/// MessageEvent
/// </summary>
[ECMAScript]
[Description("@#MessageEvent")]
public class MessageEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern MessageEvent(string type, MessageEventInit eventInitDict);

    /// <summary>
    /// data
    /// </summary>
    [Description("@#data")]
    public extern object Data { get; }

    /// <summary>
    /// origin
    /// </summary>
    [Description("@#origin")]
    public extern string Origin { get; }

    /// <summary>
    /// lastEventId
    /// </summary>
    [Description("@#lastEventId")]
    public extern string LastEventId { get; }

    /// <summary>
    /// source
    /// </summary>
    [Description("@#source")]
    public extern MessageEventSource? Source { get; }

    /// <summary>
    /// ports
    /// </summary>
    [Description("@#ports")]
    public extern FrozenSet<MessagePort> Ports { get; }

    /// <summary>
    /// initMessageEvent
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="bubbles">bubbles</param>
    /// <param name="cancelable">cancelable</param>
    /// <param name="data">data</param>
    /// <param name="origin">origin</param>
    /// <param name="lastEventId">lastEventId</param>
    /// <param name="source">source</param>
    /// <param name="ports">ports</param>
    [Description("@#initMessageEvent")]
    public extern void InitMessageEvent(string type, bool bubbles = false, bool cancelable = false, object? data = default, string? origin = default, string? lastEventId = default, MessageEventSource? source = null, MessagePort[]? ports = default);
}

/// <summary>
/// EventSource
/// </summary>
[ECMAScript]
[Description("@#EventSource")]
public class EventSource : EventTarget
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="url">url</param>
    /// <param name="eventSourceInitDict">eventSourceInitDict</param>
    public extern EventSource(string url, EventSourceInit eventSourceInitDict);

    /// <summary>
    /// url
    /// </summary>
    [Description("@#url")]
    public extern string Url { get; }

    /// <summary>
    /// withCredentials
    /// </summary>
    [Description("@#withCredentials")]
    public extern bool WithCredentials { get; }

    /// <summary>
    /// CONNECTING
    /// </summary>
    [Description("@#CONNECTING")]
    public const ushort CONNECTING = 0;

    /// <summary>
    /// OPEN
    /// </summary>
    [Description("@#OPEN")]
    public const ushort OPEN = 1;

    /// <summary>
    /// CLOSED
    /// </summary>
    [Description("@#CLOSED")]
    public const ushort CLOSED = 2;

    /// <summary>
    /// readyState
    /// </summary>
    [Description("@#readyState")]
    public extern ushort ReadyState { get; }

    /// <summary>
    /// onopen
    /// </summary>
    [Description("@#onopen")]
    public extern EventHandler Onopen { get; set; }

    /// <summary>
    /// onmessage
    /// </summary>
    [Description("@#onmessage")]
    public extern EventHandler Onmessage { get; set; }

    /// <summary>
    /// onerror
    /// </summary>
    [Description("@#onerror")]
    public extern EventHandler Onerror { get; set; }

    /// <summary>
    /// close
    /// </summary>
    [Description("@#close")]
    public extern void Close();
}

/// <summary>
/// MessageChannel
/// </summary>
[ECMAScript]
[Description("@#MessageChannel")]
public class MessageChannel
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern MessageChannel();

    /// <summary>
    /// port1
    /// </summary>
    [Description("@#port1")]
    public extern MessagePort Port1 { get; }

    /// <summary>
    /// port2
    /// </summary>
    [Description("@#port2")]
    public extern MessagePort Port2 { get; }
}

/// <summary>
/// MessagePort
/// </summary>
[ECMAScript]
[Description("@#MessagePort")]
public class MessagePort : EventTarget
{
    /// <summary>
    /// postMessage
    /// </summary>
    /// <param name="message">message</param>
    /// <param name="transfer">transfer</param>
    [Description("@#postMessage")]
    public extern void PostMessage(object message, object[] transfer);

    /// <summary>
    /// postMessage
    /// </summary>
    /// <param name="message">message</param>
    /// <param name="options">options</param>
    [Description("@#postMessage")]
    public extern void PostMessage(object message, StructuredSerializeOptions? options = default);

    /// <summary>
    /// start
    /// </summary>
    [Description("@#start")]
    public extern void Start();

    /// <summary>
    /// close
    /// </summary>
    [Description("@#close")]
    public extern void Close();

    /// <summary>
    /// onclose
    /// </summary>
    [Description("@#onclose")]
    public extern EventHandler Onclose { get; set; }

    #region mixin MessageEventTarget
    /// <summary>
    /// onmessage
    /// </summary>
    [Description("@#onmessage")]
    public extern EventHandler Onmessage { get; set; }

    /// <summary>
    /// onmessageerror
    /// </summary>
    [Description("@#onmessageerror")]
    public extern EventHandler Onmessageerror { get; set; }
    #endregion
}

/// <summary>
/// BroadcastChannel
/// </summary>
[ECMAScript]
[Description("@#BroadcastChannel")]
public class BroadcastChannel : EventTarget
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="name">name</param>
    public extern BroadcastChannel(string name);

    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; }

    /// <summary>
    /// postMessage
    /// </summary>
    /// <param name="message">message</param>
    [Description("@#postMessage")]
    public extern void PostMessage(object message);

    /// <summary>
    /// close
    /// </summary>
    [Description("@#close")]
    public extern void Close();

    /// <summary>
    /// onmessage
    /// </summary>
    [Description("@#onmessage")]
    public extern EventHandler Onmessage { get; set; }

    /// <summary>
    /// onmessageerror
    /// </summary>
    [Description("@#onmessageerror")]
    public extern EventHandler Onmessageerror { get; set; }
}

/// <summary>
/// WorkerGlobalScope
/// </summary>
[ECMAScript]
[Description("@#WorkerGlobalScope")]
public class WorkerGlobalScope : EventTarget
{
    /// <summary>
    /// self
    /// </summary>
    [Description("@#self")]
    public extern WorkerGlobalScope Self { get; }

    /// <summary>
    /// location
    /// </summary>
    [Description("@#location")]
    public extern WorkerLocation Location { get; }

    /// <summary>
    /// navigator
    /// </summary>
    [Description("@#navigator")]
    public extern WorkerNavigator Navigator { get; }

    /// <summary>
    /// importScripts
    /// </summary>
    /// <param name="urls">urls</param>
    [Description("@#importScripts")]
    public extern void ImportScripts(Either<TrustedScriptURL, string> urls);

    /// <summary>
    /// importScripts
    /// </summary>
    /// <param name="urls">urls</param>
    [Description("@#importScripts")]
    public extern void ImportScripts(TrustedScriptURL urls);

    /// <summary>
    /// importScripts
    /// </summary>
    /// <param name="urls">urls</param>
    [Description("@#importScripts")]
    public extern void ImportScripts(string urls);

    /// <summary>
    /// onerror
    /// </summary>
    [Description("@#onerror")]
    public extern OnErrorEventHandler Onerror { get; set; }

    /// <summary>
    /// onlanguagechange
    /// </summary>
    [Description("@#onlanguagechange")]
    public extern EventHandler Onlanguagechange { get; set; }

    /// <summary>
    /// onoffline
    /// </summary>
    [Description("@#onoffline")]
    public extern EventHandler Onoffline { get; set; }

    /// <summary>
    /// ononline
    /// </summary>
    [Description("@#ononline")]
    public extern EventHandler Ononline { get; set; }

    /// <summary>
    /// onrejectionhandled
    /// </summary>
    [Description("@#onrejectionhandled")]
    public extern EventHandler Onrejectionhandled { get; set; }

    /// <summary>
    /// onunhandledrejection
    /// </summary>
    [Description("@#onunhandledrejection")]
    public extern EventHandler Onunhandledrejection { get; set; }

    #region mixin FontFaceSource
    /// <summary>
    /// fonts
    /// </summary>
    [Description("@#fonts")]
    public extern FontFaceSet Fonts { get; }
    #endregion

    #region mixin WindowOrWorkerGlobalScope
    /// <summary>
    /// fetch
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="init">init</param>
    [Description("@#fetch")]
    public extern PromiseResult<Response> Fetch(RequestInfo input, RequestInit? init = default);

    /// <summary>
    /// performance
    /// </summary>
    [Description("@#performance")]
    public extern Performance Performance { get; }

    /// <summary>
    /// origin
    /// </summary>
    [Description("@#origin")]
    public extern string Origin { get; }

    /// <summary>
    /// isSecureContext
    /// </summary>
    [Description("@#isSecureContext")]
    public extern bool IsSecureContext { get; }

    /// <summary>
    /// crossOriginIsolated
    /// </summary>
    [Description("@#crossOriginIsolated")]
    public extern bool CrossOriginIsolated { get; }

    /// <summary>
    /// reportError
    /// </summary>
    /// <param name="e">e</param>
    [Description("@#reportError")]
    public extern void ReportError(object e);

    /// <summary>
    /// btoa
    /// </summary>
    /// <param name="data">data</param>
    [Description("@#btoa")]
    public extern string Btoa(string data);

    /// <summary>
    /// atob
    /// </summary>
    /// <param name="data">data</param>
    [Description("@#atob")]
    public extern byte[] Atob(string data);

    /// <summary>
    /// setTimeout
    /// </summary>
    /// <param name="handler">handler</param>
    /// <param name="timeout">timeout</param>
    /// <param name="arguments">arguments</param>
    [Description("@#setTimeout")]
    public extern int SetTimeout(TimerHandler handler, int timeout = 0, object? arguments = default);

    /// <summary>
    /// clearTimeout
    /// </summary>
    /// <param name="id">id</param>
    [Description("@#clearTimeout")]
    public extern void ClearTimeout(int id = 0);

    /// <summary>
    /// setInterval
    /// </summary>
    /// <param name="handler">handler</param>
    /// <param name="timeout">timeout</param>
    /// <param name="arguments">arguments</param>
    [Description("@#setInterval")]
    public extern int SetInterval(TimerHandler handler, int timeout = 0, object? arguments = default);

    /// <summary>
    /// clearInterval
    /// </summary>
    /// <param name="id">id</param>
    [Description("@#clearInterval")]
    public extern void ClearInterval(int id = 0);

    /// <summary>
    /// queueMicrotask
    /// </summary>
    /// <param name="callback">callback</param>
    [Description("@#queueMicrotask")]
    public extern void QueueMicrotask(Action callback);

    /// <summary>
    /// createImageBitmap
    /// </summary>
    /// <param name="image">image</param>
    /// <param name="options">options</param>
    [Description("@#createImageBitmap")]
    public extern PromiseResult<ImageBitmap> CreateImageBitmap(ImageBitmapSource image, ImageBitmapOptions? options = default);

    /// <summary>
    /// createImageBitmap
    /// </summary>
    /// <param name="image">image</param>
    /// <param name="sx">sx</param>
    /// <param name="sy">sy</param>
    /// <param name="sw">sw</param>
    /// <param name="sh">sh</param>
    /// <param name="options">options</param>
    [Description("@#createImageBitmap")]
    public extern PromiseResult<ImageBitmap> CreateImageBitmap(ImageBitmapSource image, int sx, int sy, int sw, int sh, ImageBitmapOptions? options = default);

    /// <summary>
    /// structuredClone
    /// </summary>
    /// <param name="value">value</param>
    /// <param name="options">options</param>
    [Description("@#structuredClone")]
    public extern object StructuredClone(object value, StructuredSerializeOptions? options = default);

    /// <summary>
    /// indexedDB
    /// </summary>
    [Description("@#indexedDB")]
    public extern IDBFactory IndexedDB { get; }

    /// <summary>
    /// scheduler
    /// </summary>
    [Description("@#scheduler")]
    public extern Scheduler Scheduler { get; }

    /// <summary>
    /// caches
    /// </summary>
    [Description("@#caches")]
    public extern CacheStorage Caches { get; }

    /// <summary>
    /// trustedTypes
    /// </summary>
    [Description("@#trustedTypes")]
    public extern TrustedTypePolicyFactory TrustedTypes { get; }

    /// <summary>
    /// crypto
    /// </summary>
    [Description("@#crypto")]
    public extern Crypto Crypto { get; }
    #endregion
}

/// <summary>
/// DedicatedWorkerGlobalScope
/// </summary>
[ECMAScript]
[Description("@#DedicatedWorkerGlobalScope")]
public partial class DedicatedWorkerGlobalScope : WorkerGlobalScope
{
    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; }

    /// <summary>
    /// postMessage
    /// </summary>
    /// <param name="message">message</param>
    /// <param name="transfer">transfer</param>
    [Description("@#postMessage")]
    public extern void PostMessage(object message, object[] transfer);

    /// <summary>
    /// postMessage
    /// </summary>
    /// <param name="message">message</param>
    /// <param name="options">options</param>
    [Description("@#postMessage")]
    public extern void PostMessage(object message, StructuredSerializeOptions? options = default);

    /// <summary>
    /// close
    /// </summary>
    [Description("@#close")]
    public extern void Close();

    /// <summary>
    /// onrtctransform
    /// </summary>
    [Description("@#onrtctransform")]
    public extern EventHandler Onrtctransform { get; set; }

    #region mixin AnimationFrameProvider
    /// <summary>
    /// requestAnimationFrame
    /// </summary>
    /// <param name="callback">callback</param>
    [Description("@#requestAnimationFrame")]
    public extern uint RequestAnimationFrame(FrameRequestCallback callback);

    /// <summary>
    /// cancelAnimationFrame
    /// </summary>
    /// <param name="handle">handle</param>
    [Description("@#cancelAnimationFrame")]
    public extern void CancelAnimationFrame(uint handle);
    #endregion

    #region mixin MessageEventTarget
    /// <summary>
    /// onmessage
    /// </summary>
    [Description("@#onmessage")]
    public extern EventHandler Onmessage { get; set; }

    /// <summary>
    /// onmessageerror
    /// </summary>
    [Description("@#onmessageerror")]
    public extern EventHandler Onmessageerror { get; set; }
    #endregion
}

/// <summary>
/// SharedWorkerGlobalScope
/// </summary>
[ECMAScript]
[Description("@#SharedWorkerGlobalScope")]
public class SharedWorkerGlobalScope : WorkerGlobalScope
{
    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; }

    /// <summary>
    /// close
    /// </summary>
    [Description("@#close")]
    public extern void Close();

    /// <summary>
    /// onconnect
    /// </summary>
    [Description("@#onconnect")]
    public extern EventHandler Onconnect { get; set; }
}

/// <summary>
/// Worker
/// </summary>
[ECMAScript]
[Description("@#Worker")]
public class Worker : EventTarget
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="scriptUrl">scriptURL</param>
    /// <param name="options">options</param>
    public extern Worker(Either<TrustedScriptURL, string> scriptUrl, WorkerOptions options);

    /// <summary>
    /// terminate
    /// </summary>
    [Description("@#terminate")]
    public extern void Terminate();

    /// <summary>
    /// postMessage
    /// </summary>
    /// <param name="message">message</param>
    /// <param name="transfer">transfer</param>
    [Description("@#postMessage")]
    public extern void PostMessage(object message, object[] transfer);

    /// <summary>
    /// postMessage
    /// </summary>
    /// <param name="message">message</param>
    /// <param name="options">options</param>
    [Description("@#postMessage")]
    public extern void PostMessage(object message, StructuredSerializeOptions? options = default);

    #region mixin AbstractWorker
    /// <summary>
    /// onerror
    /// </summary>
    [Description("@#onerror")]
    public extern EventHandler Onerror { get; set; }
    #endregion

    #region mixin MessageEventTarget
    /// <summary>
    /// onmessage
    /// </summary>
    [Description("@#onmessage")]
    public extern EventHandler Onmessage { get; set; }

    /// <summary>
    /// onmessageerror
    /// </summary>
    [Description("@#onmessageerror")]
    public extern EventHandler Onmessageerror { get; set; }
    #endregion
}

/// <summary>
/// SharedWorker
/// </summary>
[ECMAScript]
[Description("@#SharedWorker")]
public class SharedWorker : EventTarget
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="scriptUrl">scriptURL</param>
    /// <param name="options">options</param>
    public extern SharedWorker(Either<TrustedScriptURL, string> scriptUrl, Either<string, WorkerOptions> options);

    /// <summary>
    /// port
    /// </summary>
    [Description("@#port")]
    public extern MessagePort Port { get; }

    #region mixin AbstractWorker
    /// <summary>
    /// onerror
    /// </summary>
    [Description("@#onerror")]
    public extern EventHandler Onerror { get; set; }
    #endregion
}

/// <summary>
/// WorkerNavigator
/// </summary>
[ECMAScript]
[Description("@#WorkerNavigator")]
public partial class WorkerNavigator
{
    /// <summary>
    /// mediaCapabilities
    /// </summary>
    [Description("@#mediaCapabilities")]
    public extern MediaCapabilities MediaCapabilities { get; }

    /// <summary>
    /// permissions
    /// </summary>
    [Description("@#permissions")]
    public extern Permissions Permissions { get; }

    /// <summary>
    /// serial
    /// </summary>
    [Description("@#serial")]
    public extern Serial Serial { get; }

    /// <summary>
    /// serviceWorker
    /// </summary>
    [Description("@#serviceWorker")]
    public extern ServiceWorkerContainer ServiceWorker { get; }

    /// <summary>
    /// hid
    /// </summary>
    [Description("@#hid")]
    public extern HID Hid { get; }

    /// <summary>
    /// usb
    /// </summary>
    [Description("@#usb")]
    public extern USB Usb { get; }

    #region mixin NavigatorBadge
    /// <summary>
    /// setAppBadge
    /// </summary>
    /// <param name="contents">contents</param>
    [Description("@#setAppBadge")]
    public extern PromiseResult<object> SetAppBadge(ulong contents);

    /// <summary>
    /// clearAppBadge
    /// </summary>
    [Description("@#clearAppBadge")]
    public extern PromiseResult<object> ClearAppBadge();
    #endregion

    #region mixin NavigatorDeviceMemory
    /// <summary>
    /// deviceMemory
    /// </summary>
    [Description("@#deviceMemory")]
    public extern double DeviceMemory { get; }
    #endregion

    #region mixin GlobalPrivacyControl
    /// <summary>
    /// globalPrivacyControl
    /// </summary>
    [Description("@#globalPrivacyControl")]
    public extern bool GlobalPrivacyControl_ { get; }
    #endregion

    #region mixin NavigatorID
    /// <summary>
    /// appCodeName
    /// </summary>
    [Description("@#appCodeName")]
    public extern string AppCodeName { get; }

    /// <summary>
    /// appName
    /// </summary>
    [Description("@#appName")]
    public extern string AppName { get; }

    /// <summary>
    /// appVersion
    /// </summary>
    [Description("@#appVersion")]
    public extern string AppVersion { get; }

    /// <summary>
    /// platform
    /// </summary>
    [Description("@#platform")]
    public extern string Platform { get; }

    /// <summary>
    /// product
    /// </summary>
    [Description("@#product")]
    public extern string Product { get; }

    /// <summary>
    /// productSub
    /// </summary>
    [Description("@#productSub")]
    public extern string ProductSub { get; }

    /// <summary>
    /// userAgent
    /// </summary>
    [Description("@#userAgent")]
    public extern string UserAgent { get; }

    /// <summary>
    /// vendor
    /// </summary>
    [Description("@#vendor")]
    public extern string Vendor { get; }

    /// <summary>
    /// vendorSub
    /// </summary>
    [Description("@#vendorSub")]
    public extern string VendorSub { get; }

    /// <summary>
    /// taintEnabled
    /// </summary>
    [Description("@#taintEnabled")]
    public extern bool TaintEnabled();

    /// <summary>
    /// oscpu
    /// </summary>
    [Description("@#oscpu")]
    public extern string Oscpu { get; }
    #endregion

    #region mixin NavigatorLanguage
    /// <summary>
    /// language
    /// </summary>
    [Description("@#language")]
    public extern string Language { get; }

    /// <summary>
    /// languages
    /// </summary>
    [Description("@#languages")]
    public extern FrozenSet<string> Languages { get; }
    #endregion

    #region mixin NavigatorOnLine
    /// <summary>
    /// onLine
    /// </summary>
    [Description("@#onLine")]
    public extern bool OnLine { get; }
    #endregion

    #region mixin NavigatorConcurrentHardware
    /// <summary>
    /// hardwareConcurrency
    /// </summary>
    [Description("@#hardwareConcurrency")]
    public extern ulong HardwareConcurrency { get; }
    #endregion

    #region mixin NavigatorNetworkInformation
    /// <summary>
    /// connection
    /// </summary>
    [Description("@#connection")]
    public extern NetworkInformation Connection { get; }
    #endregion

    #region mixin NavigatorStorageBuckets
    /// <summary>
    /// storageBuckets
    /// </summary>
    [Description("@#storageBuckets")]
    public extern StorageBucketManager StorageBuckets { get; }
    #endregion

    #region mixin NavigatorStorage
    /// <summary>
    /// storage
    /// </summary>
    [Description("@#storage")]
    public extern StorageManager Storage { get; }
    #endregion

    #region mixin NavigatorUA
    /// <summary>
    /// userAgentData
    /// </summary>
    [Description("@#userAgentData")]
    public extern NavigatorUAData UserAgentData { get; }
    #endregion

    #region mixin NavigatorLocks
    /// <summary>
    /// locks
    /// </summary>
    [Description("@#locks")]
    public extern LockManager Locks { get; }
    #endregion

    #region mixin NavigatorGPU
    /// <summary>
    /// gpu
    /// </summary>
    [Description("@#gpu")]
    public extern GPU Gpu { get; }
    #endregion

    #region mixin NavigatorML
    /// <summary>
    /// ml
    /// </summary>
    [Description("@#ml")]
    public extern ML Ml { get; }
    #endregion
}

/// <summary>
/// WorkerLocation
/// </summary>
[ECMAScript]
[Description("@#WorkerLocation")]
public class WorkerLocation
{
    /// <summary>
    /// href
    /// </summary>
    [Description("@#href")]
    public extern string Href { get; }

    /// <summary>
    /// origin
    /// </summary>
    [Description("@#origin")]
    public extern string Origin { get; }

    /// <summary>
    /// protocol
    /// </summary>
    [Description("@#protocol")]
    public extern string Protocol { get; }

    /// <summary>
    /// host
    /// </summary>
    [Description("@#host")]
    public extern string Host { get; }

    /// <summary>
    /// hostname
    /// </summary>
    [Description("@#hostname")]
    public extern string Hostname { get; }

    /// <summary>
    /// port
    /// </summary>
    [Description("@#port")]
    public extern string Port { get; }

    /// <summary>
    /// pathname
    /// </summary>
    [Description("@#pathname")]
    public extern string Pathname { get; }

    /// <summary>
    /// search
    /// </summary>
    [Description("@#search")]
    public extern string Search { get; }

    /// <summary>
    /// hash
    /// </summary>
    [Description("@#hash")]
    public extern string Hash { get; }
}

/// <summary>
/// WorkletGlobalScope
/// </summary>
[ECMAScript]
[Description("@#WorkletGlobalScope")]
public abstract class WorkletGlobalScope
{

}

/// <summary>
/// Worklet
/// </summary>
[ECMAScript]
[Description("@#Worklet")]
public class Worklet
{
    /// <summary>
    /// addModule
    /// </summary>
    /// <param name="moduleUrl">moduleURL</param>
    /// <param name="options">options</param>
    [Description("@#addModule")]
    public extern PromiseResult<object> AddModule(string moduleUrl, WorkletOptions? options = default);
}

/// <summary>
/// Storage
/// </summary>
[ECMAScript]
[Description("@#Storage")]
public class Storage
{
    /// <summary>
    /// length
    /// </summary>
    [Description("@#length")]
    public extern uint Length { get; }

    /// <summary>
    /// key
    /// </summary>
    /// <param name="index">index</param>
    [Description("@#key")]
    public extern string? Key(uint index);

    /// <summary>
    /// getItem
    /// </summary>
    /// <param name="key">key</param>
    [Description("@#getItem")]
    public extern string? GetItem(string key);

    /// <summary>
    /// setItem
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="value">value</param>
    [Description("@#setItem")]
    public extern void SetItem(string key, string value);

    /// <summary>
    /// removeItem
    /// </summary>
    /// <param name="key">key</param>
    [Description("@#removeItem")]
    public extern void RemoveItem(string key);

    /// <summary>
    /// clear
    /// </summary>
    [Description("@#clear")]
    public extern void Clear();
}

/// <summary>
/// StorageEvent
/// </summary>
[ECMAScript]
[Description("@#StorageEvent")]
public class StorageEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern StorageEvent(string type, StorageEventInit eventInitDict);

    /// <summary>
    /// key
    /// </summary>
    [Description("@#key")]
    public extern string? Key { get; }

    /// <summary>
    /// oldValue
    /// </summary>
    [Description("@#oldValue")]
    public extern string? OldValue { get; }

    /// <summary>
    /// newValue
    /// </summary>
    [Description("@#newValue")]
    public extern string? NewValue { get; }

    /// <summary>
    /// url
    /// </summary>
    [Description("@#url")]
    public extern string Url { get; }

    /// <summary>
    /// storageArea
    /// </summary>
    [Description("@#storageArea")]
    public extern Storage? StorageArea { get; }

    /// <summary>
    /// initStorageEvent
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="bubbles">bubbles</param>
    /// <param name="cancelable">cancelable</param>
    /// <param name="key">key</param>
    /// <param name="oldValue">oldValue</param>
    /// <param name="newValue">newValue</param>
    /// <param name="url">url</param>
    /// <param name="storageArea">storageArea</param>
    [Description("@#initStorageEvent")]
    public extern void InitStorageEvent(string type, bool bubbles = false, bool cancelable = false, string? key = null, string? oldValue = null, string? newValue = null, string? url = default, Storage? storageArea = null);
}

/// <summary>
/// HTMLMarqueeElement
/// </summary>
[ECMAScript]
[Description("@#HTMLMarqueeElement")]
public class HTMLMarqueeElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLMarqueeElement();

    /// <summary>
    /// behavior
    /// </summary>
    [Description("@#behavior")]
    public extern string Behavior { get; set; }

    /// <summary>
    /// bgColor
    /// </summary>
    [Description("@#bgColor")]
    public extern string BgColor { get; set; }

    /// <summary>
    /// direction
    /// </summary>
    [Description("@#direction")]
    public extern string Direction { get; set; }

    /// <summary>
    /// height
    /// </summary>
    [Description("@#height")]
    public extern string Height { get; set; }

    /// <summary>
    /// hspace
    /// </summary>
    [Description("@#hspace")]
    public extern uint Hspace { get; set; }

    /// <summary>
    /// loop
    /// </summary>
    [Description("@#loop")]
    public extern int Loop { get; set; }

    /// <summary>
    /// scrollAmount
    /// </summary>
    [Description("@#scrollAmount")]
    public extern uint ScrollAmount { get; set; }

    /// <summary>
    /// scrollDelay
    /// </summary>
    [Description("@#scrollDelay")]
    public extern uint ScrollDelay { get; set; }

    /// <summary>
    /// trueSpeed
    /// </summary>
    [Description("@#trueSpeed")]
    public extern bool TrueSpeed { get; set; }

    /// <summary>
    /// vspace
    /// </summary>
    [Description("@#vspace")]
    public extern uint Vspace { get; set; }

    /// <summary>
    /// width
    /// </summary>
    [Description("@#width")]
    public extern string Width { get; set; }

    /// <summary>
    /// start
    /// </summary>
    [Description("@#start")]
    public extern void Start();

    /// <summary>
    /// stop
    /// </summary>
    [Description("@#stop")]
    public extern void Stop();
}

/// <summary>
/// HTMLFrameSetElement
/// </summary>
[ECMAScript]
[Description("@#HTMLFrameSetElement")]
public class HTMLFrameSetElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLFrameSetElement();

    /// <summary>
    /// cols
    /// </summary>
    [Description("@#cols")]
    public extern string Cols { get; set; }

    /// <summary>
    /// rows
    /// </summary>
    [Description("@#rows")]
    public extern string Rows { get; set; }

    #region mixin WindowEventHandlers
    /// <summary>
    /// ongamepadconnected
    /// </summary>
    [Description("@#ongamepadconnected")]
    public extern EventHandler Ongamepadconnected { get; set; }

    /// <summary>
    /// ongamepaddisconnected
    /// </summary>
    [Description("@#ongamepaddisconnected")]
    public extern EventHandler Ongamepaddisconnected { get; set; }

    /// <summary>
    /// onafterprint
    /// </summary>
    [Description("@#onafterprint")]
    public extern EventHandler Onafterprint { get; set; }

    /// <summary>
    /// onbeforeprint
    /// </summary>
    [Description("@#onbeforeprint")]
    public extern EventHandler Onbeforeprint { get; set; }

    /// <summary>
    /// onbeforeunload
    /// </summary>
    [Description("@#onbeforeunload")]
    public extern OnBeforeUnloadEventHandler Onbeforeunload { get; set; }

    /// <summary>
    /// onhashchange
    /// </summary>
    [Description("@#onhashchange")]
    public extern EventHandler Onhashchange { get; set; }

    /// <summary>
    /// onlanguagechange
    /// </summary>
    [Description("@#onlanguagechange")]
    public extern EventHandler Onlanguagechange { get; set; }

    /// <summary>
    /// onmessage
    /// </summary>
    [Description("@#onmessage")]
    public extern EventHandler Onmessage { get; set; }

    /// <summary>
    /// onmessageerror
    /// </summary>
    [Description("@#onmessageerror")]
    public extern EventHandler Onmessageerror { get; set; }

    /// <summary>
    /// onoffline
    /// </summary>
    [Description("@#onoffline")]
    public extern EventHandler Onoffline { get; set; }

    /// <summary>
    /// ononline
    /// </summary>
    [Description("@#ononline")]
    public extern EventHandler Ononline { get; set; }

    /// <summary>
    /// onpagehide
    /// </summary>
    [Description("@#onpagehide")]
    public extern EventHandler Onpagehide { get; set; }

    /// <summary>
    /// onpagereveal
    /// </summary>
    [Description("@#onpagereveal")]
    public extern EventHandler Onpagereveal { get; set; }

    /// <summary>
    /// onpageshow
    /// </summary>
    [Description("@#onpageshow")]
    public extern EventHandler Onpageshow { get; set; }

    /// <summary>
    /// onpageswap
    /// </summary>
    [Description("@#onpageswap")]
    public extern EventHandler Onpageswap { get; set; }

    /// <summary>
    /// onpopstate
    /// </summary>
    [Description("@#onpopstate")]
    public extern EventHandler Onpopstate { get; set; }

    /// <summary>
    /// onrejectionhandled
    /// </summary>
    [Description("@#onrejectionhandled")]
    public extern EventHandler Onrejectionhandled { get; set; }

    /// <summary>
    /// onstorage
    /// </summary>
    [Description("@#onstorage")]
    public extern EventHandler Onstorage { get; set; }

    /// <summary>
    /// onunhandledrejection
    /// </summary>
    [Description("@#onunhandledrejection")]
    public extern EventHandler Onunhandledrejection { get; set; }

    /// <summary>
    /// onunload
    /// </summary>
    [Description("@#onunload")]
    public extern EventHandler Onunload { get; set; }

    /// <summary>
    /// onportalactivate
    /// </summary>
    [Description("@#onportalactivate")]
    public extern EventHandler Onportalactivate { get; set; }
    #endregion
}

/// <summary>
/// HTMLFrameElement
/// </summary>
[ECMAScript]
[Description("@#HTMLFrameElement")]
public class HTMLFrameElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLFrameElement();

    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; set; }

    /// <summary>
    /// scrolling
    /// </summary>
    [Description("@#scrolling")]
    public extern string Scrolling { get; set; }

    /// <summary>
    /// src
    /// </summary>
    [Description("@#src")]
    public extern string Src { get; set; }

    /// <summary>
    /// frameBorder
    /// </summary>
    [Description("@#frameBorder")]
    public extern string FrameBorder { get; set; }

    /// <summary>
    /// longDesc
    /// </summary>
    [Description("@#longDesc")]
    public extern string LongDesc { get; set; }

    /// <summary>
    /// noResize
    /// </summary>
    [Description("@#noResize")]
    public extern bool NoResize { get; set; }

    /// <summary>
    /// contentDocument
    /// </summary>
    [Description("@#contentDocument")]
    public extern Document? ContentDocument { get; }

    /// <summary>
    /// contentWindow
    /// </summary>
    [Description("@#contentWindow")]
    public extern WindowProxy? ContentWindow { get; }

    /// <summary>
    /// marginHeight
    /// </summary>
    [Description("@#marginHeight")]
    public extern string MarginHeight { get; set; }

    /// <summary>
    /// marginWidth
    /// </summary>
    [Description("@#marginWidth")]
    public extern string MarginWidth { get; set; }
}

/// <summary>
/// HTMLDirectoryElement
/// </summary>
[ECMAScript]
[Description("@#HTMLDirectoryElement")]
public class HTMLDirectoryElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLDirectoryElement();

    /// <summary>
    /// compact
    /// </summary>
    [Description("@#compact")]
    public extern bool Compact { get; set; }
}

/// <summary>
/// HTMLFontElement
/// </summary>
[ECMAScript]
[Description("@#HTMLFontElement")]
public class HTMLFontElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLFontElement();

    /// <summary>
    /// color
    /// </summary>
    [Description("@#color")]
    public extern string Color { get; set; }

    /// <summary>
    /// face
    /// </summary>
    [Description("@#face")]
    public extern string Face { get; set; }

    /// <summary>
    /// size
    /// </summary>
    [Description("@#size")]
    public extern string Size { get; set; }
}

/// <summary>
/// HTMLParamElement
/// </summary>
[ECMAScript]
[Description("@#HTMLParamElement")]
public class HTMLParamElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLParamElement();

    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; set; }

    /// <summary>
    /// value
    /// </summary>
    [Description("@#value")]
    public extern string Value { get; set; }

    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern string Type { get; set; }

    /// <summary>
    /// valueType
    /// </summary>
    [Description("@#valueType")]
    public extern string ValueType { get; set; }
}

/// <summary>
/// External
/// </summary>
[ECMAScript]
[Description("@#External")]
public class External
{
    /// <summary>
    /// AddSearchProvider
    /// </summary>
    [Description("@#AddSearchProvider")]
    public extern void AddSearchProvider();

    /// <summary>
    /// IsSearchProviderInstalled
    /// </summary>
    [Description("@#IsSearchProviderInstalled")]
    public extern void IsSearchProviderInstalled();
}