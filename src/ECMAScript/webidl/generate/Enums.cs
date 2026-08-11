namespace ECMAScript;

/// <summary>
/// &quot;serviceworker&quot; is omitted from RequestDestination as it cannot be observed from JavaScript. Implementations will still need to support it as a destination. &quot;websocket&quot; and &quot;webtransport&quot; are omitted from RequestMode as they cannot be used or observed from JavaScript.
/// </summary>
/// <remarks>
/// <see href="https://fetch.spec.whatwg.org/#requestdestination">Fetch Standard: 5.4 Request class</see>
/// </remarks>
[Description("@#RequestDestination")]
[ECMAScript]
[String]
public enum RequestDestination
{
    [Description("@#")]
    Empty = 0,

    /// <summary>
    /// <see href="https://fetch.spec.whatwg.org/#dom-requestdestination-audio">Fetch Standard: 5.4 Request class</see>
    /// </summary>
    [Description("@#audio")]
    Audio = 1,

    /// <summary>
    /// A request&apos;s destination is script-like if it is &quot;audioworklet&quot;, &quot;paintworklet&quot;, &quot;script&quot;, &quot;serviceworker&quot;, &quot;sharedworker&quot;, or &quot;worker&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://fetch.spec.whatwg.org/#dom-requestdestination-audioworklet">Fetch Standard: 5.4 Request class</see>
    /// </remarks>
    [Description("@#audioworklet")]
    Audioworklet = 2,

    /// <summary>
    /// A non-subresource request is a request whose destination is &quot;document&quot;, &quot;embed&quot;, &quot;frame&quot;, &quot;iframe&quot;, &quot;object&quot;, &quot;report&quot;, &quot;serviceworker&quot;, &quot;sharedworker&quot;, or &quot;worker&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://fetch.spec.whatwg.org/#dom-requestdestination-document">Fetch Standard: 5.4 Request class</see>
    /// </remarks>
    [Description("@#document")]
    Document = 3,

    /// <summary>
    /// A non-subresource request is a request whose destination is &quot;document&quot;, &quot;embed&quot;, &quot;frame&quot;, &quot;iframe&quot;, &quot;object&quot;, &quot;report&quot;, &quot;serviceworker&quot;, &quot;sharedworker&quot;, or &quot;worker&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://fetch.spec.whatwg.org/#dom-requestdestination-embed">Fetch Standard: 5.4 Request class</see>
    /// </remarks>
    [Description("@#embed")]
    Embed = 4,

    /// <summary>
    /// A subresource request is a request whose destination is &quot;audio&quot;, &quot;audioworklet&quot;, &quot;font&quot;, &quot;image&quot;, &quot;json&quot;, &quot;manifest&quot;, &quot;paintworklet&quot;, &quot;script&quot;, &quot;style&quot;, &quot;text&quot;, &quot;track&quot;, &quot;video&quot;, &quot;xslt&quot;, or the empty string.
    /// </summary>
    /// <remarks>
    /// <see href="https://fetch.spec.whatwg.org/#dom-requestdestination-font">Fetch Standard: 5.4 Request class</see>
    /// </remarks>
    [Description("@#font")]
    Font = 5,

    /// <summary>
    /// A non-subresource request is a request whose destination is &quot;document&quot;, &quot;embed&quot;, &quot;frame&quot;, &quot;iframe&quot;, &quot;object&quot;, &quot;report&quot;, &quot;serviceworker&quot;, &quot;sharedworker&quot;, or &quot;worker&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://fetch.spec.whatwg.org/#dom-requestdestination-frame">Fetch Standard: 5.4 Request class</see>
    /// </remarks>
    [Description("@#frame")]
    Frame = 6,

    /// <summary>
    /// A non-subresource request is a request whose destination is &quot;document&quot;, &quot;embed&quot;, &quot;frame&quot;, &quot;iframe&quot;, &quot;object&quot;, &quot;report&quot;, &quot;serviceworker&quot;, &quot;sharedworker&quot;, or &quot;worker&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://fetch.spec.whatwg.org/#dom-requestdestination-iframe">Fetch Standard: 5.4 Request class</see>
    /// </remarks>
    [Description("@#iframe")]
    Iframe = 7,

    /// <summary>
    /// A subresource request is a request whose destination is &quot;audio&quot;, &quot;audioworklet&quot;, &quot;font&quot;, &quot;image&quot;, &quot;json&quot;, &quot;manifest&quot;, &quot;paintworklet&quot;, &quot;script&quot;, &quot;style&quot;, &quot;text&quot;, &quot;track&quot;, &quot;video&quot;, &quot;xslt&quot;, or the empty string.
    /// </summary>
    /// <remarks>
    /// <see href="https://fetch.spec.whatwg.org/#dom-requestdestination-image">Fetch Standard: 5.4 Request class</see>
    /// </remarks>
    [Description("@#image")]
    Image = 8,

    /// <summary>
    /// A subresource request is a request whose destination is &quot;audio&quot;, &quot;audioworklet&quot;, &quot;font&quot;, &quot;image&quot;, &quot;json&quot;, &quot;manifest&quot;, &quot;paintworklet&quot;, &quot;script&quot;, &quot;style&quot;, &quot;text&quot;, &quot;track&quot;, &quot;video&quot;, &quot;xslt&quot;, or the empty string.
    /// </summary>
    /// <remarks>
    /// <see href="https://fetch.spec.whatwg.org/#dom-requestdestination-json">Fetch Standard: 5.4 Request class</see>
    /// </remarks>
    [Description("@#json")]
    Json = 9,

    /// <summary>
    /// A subresource request is a request whose destination is &quot;audio&quot;, &quot;audioworklet&quot;, &quot;font&quot;, &quot;image&quot;, &quot;json&quot;, &quot;manifest&quot;, &quot;paintworklet&quot;, &quot;script&quot;, &quot;style&quot;, &quot;text&quot;, &quot;track&quot;, &quot;video&quot;, &quot;xslt&quot;, or the empty string.
    /// </summary>
    /// <remarks>
    /// <see href="https://fetch.spec.whatwg.org/#dom-requestdestination-manifest">Fetch Standard: 5.4 Request class</see>
    /// </remarks>
    [Description("@#manifest")]
    Manifest = 10,

    /// <summary>
    /// A non-subresource request is a request whose destination is &quot;document&quot;, &quot;embed&quot;, &quot;frame&quot;, &quot;iframe&quot;, &quot;object&quot;, &quot;report&quot;, &quot;serviceworker&quot;, &quot;sharedworker&quot;, or &quot;worker&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://fetch.spec.whatwg.org/#dom-requestdestination-object">Fetch Standard: 5.4 Request class</see>
    /// </remarks>
    [Description("@#object")]
    Object = 11,

    /// <summary>
    /// A request&apos;s destination is script-like if it is &quot;audioworklet&quot;, &quot;paintworklet&quot;, &quot;script&quot;, &quot;serviceworker&quot;, &quot;sharedworker&quot;, or &quot;worker&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://fetch.spec.whatwg.org/#dom-requestdestination-paintworklet">Fetch Standard: 5.4 Request class</see>
    /// </remarks>
    [Description("@#paintworklet")]
    Paintworklet = 12,

    /// <summary>
    /// A non-subresource request is a request whose destination is &quot;document&quot;, &quot;embed&quot;, &quot;frame&quot;, &quot;iframe&quot;, &quot;object&quot;, &quot;report&quot;, &quot;serviceworker&quot;, &quot;sharedworker&quot;, or &quot;worker&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://fetch.spec.whatwg.org/#dom-requestdestination-report">Fetch Standard: 5.4 Request class</see>
    /// </remarks>
    [Description("@#report")]
    Report = 13,

    /// <summary>
    /// A request&apos;s destination is script-like if it is &quot;audioworklet&quot;, &quot;paintworklet&quot;, &quot;script&quot;, &quot;serviceworker&quot;, &quot;sharedworker&quot;, or &quot;worker&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://fetch.spec.whatwg.org/#dom-requestdestination-script">Fetch Standard: 5.4 Request class</see>
    /// </remarks>
    [Description("@#script")]
    Script = 14,

    /// <summary>
    /// A request&apos;s destination is script-like if it is &quot;audioworklet&quot;, &quot;paintworklet&quot;, &quot;script&quot;, &quot;serviceworker&quot;, &quot;sharedworker&quot;, or &quot;worker&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://fetch.spec.whatwg.org/#dom-requestdestination-sharedworker">Fetch Standard: 5.4 Request class</see>
    /// </remarks>
    [Description("@#sharedworker")]
    Sharedworker = 15,

    /// <summary>
    /// A subresource request is a request whose destination is &quot;audio&quot;, &quot;audioworklet&quot;, &quot;font&quot;, &quot;image&quot;, &quot;json&quot;, &quot;manifest&quot;, &quot;paintworklet&quot;, &quot;script&quot;, &quot;style&quot;, &quot;text&quot;, &quot;track&quot;, &quot;video&quot;, &quot;xslt&quot;, or the empty string.
    /// </summary>
    /// <remarks>
    /// <see href="https://fetch.spec.whatwg.org/#dom-requestdestination-style">Fetch Standard: 5.4 Request class</see>
    /// </remarks>
    [Description("@#style")]
    Style = 16,

    /// <summary>
    /// A subresource request is a request whose destination is &quot;audio&quot;, &quot;audioworklet&quot;, &quot;font&quot;, &quot;image&quot;, &quot;json&quot;, &quot;manifest&quot;, &quot;paintworklet&quot;, &quot;script&quot;, &quot;style&quot;, &quot;text&quot;, &quot;track&quot;, &quot;video&quot;, &quot;xslt&quot;, or the empty string.
    /// </summary>
    /// <remarks>
    /// <see href="https://fetch.spec.whatwg.org/#dom-requestdestination-text">Fetch Standard: 5.4 Request class</see>
    /// </remarks>
    [Description("@#text")]
    Text = 17,

    /// <summary>
    /// A subresource request is a request whose destination is &quot;audio&quot;, &quot;audioworklet&quot;, &quot;font&quot;, &quot;image&quot;, &quot;json&quot;, &quot;manifest&quot;, &quot;paintworklet&quot;, &quot;script&quot;, &quot;style&quot;, &quot;text&quot;, &quot;track&quot;, &quot;video&quot;, &quot;xslt&quot;, or the empty string.
    /// </summary>
    /// <remarks>
    /// <see href="https://fetch.spec.whatwg.org/#dom-requestdestination-track">Fetch Standard: 5.4 Request class</see>
    /// </remarks>
    [Description("@#track")]
    Track = 18,

    /// <summary>
    /// A subresource request is a request whose destination is &quot;audio&quot;, &quot;audioworklet&quot;, &quot;font&quot;, &quot;image&quot;, &quot;json&quot;, &quot;manifest&quot;, &quot;paintworklet&quot;, &quot;script&quot;, &quot;style&quot;, &quot;text&quot;, &quot;track&quot;, &quot;video&quot;, &quot;xslt&quot;, or the empty string.
    /// </summary>
    /// <remarks>
    /// <see href="https://fetch.spec.whatwg.org/#dom-requestdestination-video">Fetch Standard: 5.4 Request class</see>
    /// </remarks>
    [Description("@#video")]
    Video = 19,

    /// <summary>
    /// <see href="https://fetch.spec.whatwg.org/#dom-requestdestination-worker">Fetch Standard: 5.4 Request class</see>
    /// </summary>
    [Description("@#worker")]
    Worker = 20,

    /// <summary>
    /// Algorithms that use script-like should also consider &quot;xslt&quot; as that too can cause script execution. It is not included in the list as it is not always relevant and might require different behavior.
    /// </summary>
    /// <remarks>
    /// <see href="https://fetch.spec.whatwg.org/#dom-requestdestination-xslt">Fetch Standard: 5.4 Request class</see>
    /// </remarks>
    [Description("@#xslt")]
    Xslt = 21
}

/// <summary>
/// &quot;serviceworker&quot; is omitted from RequestDestination as it cannot be observed from JavaScript. Implementations will still need to support it as a destination. &quot;websocket&quot; and &quot;webtransport&quot; are omitted from RequestMode as they cannot be used or observed from JavaScript.
/// </summary>
/// <remarks>
/// <see href="https://fetch.spec.whatwg.org/#requestmode">Fetch Standard: 5.4 Request class</see>
/// </remarks>
[Description("@#RequestMode")]
[ECMAScript]
[String]
public enum RequestMode
{
    /// <summary>
    /// A request has an associated mode, which is &quot;same-origin&quot;, &quot;cors&quot;, &quot;no-cors&quot;, &quot;navigate&quot;, &quot;websocket&quot;, or &quot;webtransport&quot;. Unless stated otherwise, it is &quot;no-cors&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://fetch.spec.whatwg.org/#dom-requestmode-navigate">Fetch Standard: 5.4 Request class</see>
    /// </remarks>
    [Description("@#navigate")]
    Navigate = 0,

    /// <summary>
    /// A request has an associated mode, which is &quot;same-origin&quot;, &quot;cors&quot;, &quot;no-cors&quot;, &quot;navigate&quot;, &quot;websocket&quot;, or &quot;webtransport&quot;. Unless stated otherwise, it is &quot;no-cors&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://fetch.spec.whatwg.org/#dom-requestmode-same-origin">Fetch Standard: 5.4 Request class</see>
    /// </remarks>
    [Description("@#same-origin")]
    SameOrigin = 1,

    /// <summary>
    /// A request has an associated mode, which is &quot;same-origin&quot;, &quot;cors&quot;, &quot;no-cors&quot;, &quot;navigate&quot;, &quot;websocket&quot;, or &quot;webtransport&quot;. Unless stated otherwise, it is &quot;no-cors&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://fetch.spec.whatwg.org/#dom-requestmode-no-cors">Fetch Standard: 5.4 Request class</see>
    /// </remarks>
    [Description("@#no-cors")]
    NoCors = 2,

    /// <summary>
    /// A request has an associated mode, which is &quot;same-origin&quot;, &quot;cors&quot;, &quot;no-cors&quot;, &quot;navigate&quot;, &quot;websocket&quot;, or &quot;webtransport&quot;. Unless stated otherwise, it is &quot;no-cors&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://fetch.spec.whatwg.org/#dom-requestmode-cors">Fetch Standard: 5.4 Request class</see>
    /// </remarks>
    [Description("@#cors")]
    Cors = 3
}

/// <summary>
/// <see href="https://compression.spec.whatwg.org/#enumdef-compressionformat">Compression Standard: 4 Interface CompressionStream</see>
/// </summary>
[Description("@#CompressionFormat")]
[ECMAScript]
[String]
public enum CompressionFormat
{
    /// <summary>
    /// <see href="https://compression.spec.whatwg.org/#dom-compressionformat-brotli">Compression Standard: 4 Interface CompressionStream</see>
    /// </summary>
    [Description("@#brotli")]
    Brotli = 0,

    /// <summary>
    /// <see href="https://compression.spec.whatwg.org/#dom-compressionformat-deflate">Compression Standard: 4 Interface CompressionStream</see>
    /// </summary>
    [Description("@#deflate")]
    Deflate = 1,

    /// <summary>
    /// <see href="https://compression.spec.whatwg.org/#dom-compressionformat-deflate-raw">Compression Standard: 4 Interface CompressionStream</see>
    /// </summary>
    [Description("@#deflate-raw")]
    DeflateRaw = 2,

    /// <summary>
    /// <see href="https://compression.spec.whatwg.org/#dom-compressionformat-gzip">Compression Standard: 4 Interface CompressionStream</see>
    /// </summary>
    [Description("@#gzip")]
    Gzip = 3
}

/// <summary>
/// <see href="https://cookiestore.spec.whatwg.org/#enumdef-cookiesamesite">Cookie Store API Standard: 3 The CookieStore interface</see>
/// </summary>
[Description("@#CookieSameSite")]
[ECMAScript]
[String]
public enum CookieSameSite
{
    /// <summary>
    /// <see href="https://cookiestore.spec.whatwg.org/#dom-cookiesamesite-strict">Cookie Store API Standard: 3 The CookieStore interface</see>
    /// </summary>
    [Description("@#strict")]
    Strict = 0,

    /// <summary>
    /// <see href="https://cookiestore.spec.whatwg.org/#dom-cookiesamesite-lax">Cookie Store API Standard: 3 The CookieStore interface</see>
    /// </summary>
    [Description("@#lax")]
    Lax = 1,

    /// <summary>
    /// <see href="https://cookiestore.spec.whatwg.org/#dom-cookiesamesite-none">Cookie Store API Standard: 3 The CookieStore interface</see>
    /// </summary>
    [Description("@#none")]
    None = 2
}

/// <summary>
/// <see href="https://dom.spec.whatwg.org/#enumdef-shadowrootmode">DOM Standard: 4.8 Interface ShadowRoot</see>
/// </summary>
[Description("@#ShadowRootMode")]
[ECMAScript]
[String]
public enum ShadowRootMode
{
    /// <summary>
    /// If open is true and shadow&apos;s mode is not &quot;open&quot;, then return null.
    /// </summary>
    /// <remarks>
    /// <see href="https://dom.spec.whatwg.org/#dom-shadowrootmode-open">DOM Standard: 4.8 Interface ShadowRoot</see>
    /// </remarks>
    [Description("@#open")]
    Open = 0,

    /// <summary>
    /// Returns the invocation target objects of event&apos;s path (objects on which listeners will be invoked), except for any nodes in shadow trees of which the shadow root&apos;s mode is &quot;closed&quot; that are not reachable from event&apos;s currentTarget.
    /// </summary>
    /// <remarks>
    /// <see href="https://dom.spec.whatwg.org/#dom-shadowrootmode-closed">DOM Standard: 4.8 Interface ShadowRoot</see>
    /// </remarks>
    [Description("@#closed")]
    Closed = 1
}

/// <summary>
/// <see href="https://dom.spec.whatwg.org/#enumdef-slotassignmentmode">DOM Standard: 4.8 Interface ShadowRoot</see>
/// </summary>
[Description("@#SlotAssignmentMode")]
[ECMAScript]
[String]
public enum SlotAssignmentMode
{
    /// <summary>
    /// <see href="https://dom.spec.whatwg.org/#dom-slotassignmentmode-manual">DOM Standard: 4.8 Interface ShadowRoot</see>
    /// </summary>
    [Description("@#manual")]
    Manual = 0,

    /// <summary>
    /// <see href="https://dom.spec.whatwg.org/#dom-slotassignmentmode-named">DOM Standard: 4.8 Interface ShadowRoot</see>
    /// </summary>
    [Description("@#named")]
    Named = 1
}

/// <summary>
/// <see href="https://drafts.csswg.org/css-font-loading-3/#enumdef-fontfaceloadstatus">CSS Font Loading Module Level 3: 2 The FontFace Interface</see>
/// </summary>
[Description("@#FontFaceLoadStatus")]
[ECMAScript]
[String]
public enum FontFaceLoadStatus
{
    /// <summary>
    /// <see href="https://drafts.csswg.org/css-font-loading-3/#dom-fontfaceloadstatus-unloaded">CSS Font Loading Module Level 3: 2 The FontFace Interface</see>
    /// </summary>
    [Description("@#unloaded")]
    Unloaded = 0,

    /// <summary>
    /// <see href="https://drafts.csswg.org/css-font-loading-3/#dom-fontfaceloadstatus-loading">CSS Font Loading Module Level 3: 2 The FontFace Interface</see>
    /// </summary>
    [Description("@#loading")]
    Loading = 1,

    /// <summary>
    /// <see href="https://drafts.csswg.org/css-font-loading-3/#dom-fontfaceloadstatus-loaded">CSS Font Loading Module Level 3: 2 The FontFace Interface</see>
    /// </summary>
    [Description("@#loaded")]
    Loaded = 2,

    /// <summary>
    /// <see href="https://drafts.csswg.org/css-font-loading-3/#dom-fontfaceloadstatus-error">CSS Font Loading Module Level 3: 2 The FontFace Interface</see>
    /// </summary>
    [Description("@#error")]
    Error = 3
}

/// <summary>
/// <see href="https://drafts.csswg.org/css-font-loading-3/#enumdef-fontfacesetloadstatus">CSS Font Loading Module Level 3: 3 The FontFaceSet Interface</see>
/// </summary>
[Description("@#FontFaceSetLoadStatus")]
[ECMAScript]
[String]
public enum FontFaceSetLoadStatus
{
    /// <summary>
    /// <see href="https://drafts.csswg.org/css-font-loading-3/#dom-fontfacesetloadstatus-loading">CSS Font Loading Module Level 3: 3 The FontFaceSet Interface</see>
    /// </summary>
    [Description("@#loading")]
    Loading = 0,

    /// <summary>
    /// <see href="https://drafts.csswg.org/css-font-loading-3/#dom-fontfacesetloadstatus-loaded">CSS Font Loading Module Level 3: 3 The FontFaceSet Interface</see>
    /// </summary>
    [Description("@#loaded")]
    Loaded = 1
}

/// <summary>
/// <see href="https://drafts.csswg.org/css-spatial-nav-1/#enumdef-focusableareasearchmode">CSS Spatial Navigation Module Level 1: 5.2 Low level APIs</see>
/// </summary>
[Description("@#FocusableAreaSearchMode")]
[ECMAScript]
[String]
public enum FocusableAreaSearchMode
{
    /// <summary>
    /// <see href="https://drafts.csswg.org/css-spatial-nav-1/#dom-focusableareasearchmode-visible">CSS Spatial Navigation Module Level 1: 5.2 Low level APIs</see>
    /// </summary>
    [Description("@#visible")]
    Visible = 0,

    /// <summary>
    /// <see href="https://drafts.csswg.org/css-spatial-nav-1/#dom-focusableareasearchmode-all">CSS Spatial Navigation Module Level 1: 5.2 Low level APIs</see>
    /// </summary>
    [Description("@#all")]
    All = 1
}

/// <summary>
/// <see href="https://drafts.csswg.org/css-spatial-nav-1/#enumdef-spatialnavigationdirection">CSS Spatial Navigation Module Level 1: 5.1 Triggering Navigation Programmatically</see>
/// </summary>
[Description("@#SpatialNavigationDirection")]
[ECMAScript]
[String]
public enum SpatialNavigationDirection
{
    /// <summary>
    /// <see href="https://drafts.csswg.org/css-spatial-nav-1/#dom-spatialnavigationdirection-up">CSS Spatial Navigation Module Level 1: 5.1 Triggering Navigation Programmatically</see>
    /// </summary>
    [Description("@#up")]
    Up = 0,

    /// <summary>
    /// <see href="https://drafts.csswg.org/css-spatial-nav-1/#dom-spatialnavigationdirection-down">CSS Spatial Navigation Module Level 1: 5.1 Triggering Navigation Programmatically</see>
    /// </summary>
    [Description("@#down")]
    Down = 1,

    /// <summary>
    /// <see href="https://drafts.csswg.org/css-spatial-nav-1/#dom-spatialnavigationdirection-left">CSS Spatial Navigation Module Level 1: 5.1 Triggering Navigation Programmatically</see>
    /// </summary>
    [Description("@#left")]
    Left = 2,

    /// <summary>
    /// <see href="https://drafts.csswg.org/css-spatial-nav-1/#dom-spatialnavigationdirection-right">CSS Spatial Navigation Module Level 1: 5.1 Triggering Navigation Programmatically</see>
    /// </summary>
    [Description("@#right")]
    Right = 3
}

/// <summary>
/// <see href="https://drafts.csswg.org/cssom-view-1/#enumdef-cssboxtype">CSSOM View Module Level 1: 11.1 The GeometryUtils Interface</see>
/// </summary>
[Description("@#CSSBoxType")]
[ECMAScript]
[String]
public enum CSSBoxType
{
    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-view-1/#dom-cssboxtype-margin">CSSOM View Module Level 1: 11.1 The GeometryUtils Interface</see>
    /// </summary>
    [Description("@#margin")]
    Margin = 0,

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-view-1/#dom-cssboxtype-border">CSSOM View Module Level 1: 11.1 The GeometryUtils Interface</see>
    /// </summary>
    [Description("@#border")]
    Border = 1,

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-view-1/#dom-cssboxtype-padding">CSSOM View Module Level 1: 11.1 The GeometryUtils Interface</see>
    /// </summary>
    [Description("@#padding")]
    Padding = 2,

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-view-1/#dom-cssboxtype-content">CSSOM View Module Level 1: 11.1 The GeometryUtils Interface</see>
    /// </summary>
    [Description("@#content")]
    Content = 3
}

/// <summary>
/// <see href="https://drafts.csswg.org/cssom-view-1/#enumdef-scrollbehavior">CSSOM View Module Level 1: 4 Extensions to the Window Interface</see>
/// </summary>
[Description("@#ScrollBehavior")]
[ECMAScript]
[String]
public enum ScrollBehavior
{
    /// <summary>
    /// behavior is &quot;auto&quot; and element is not null and its computed value of the &apos;scroll-behavior&apos; property is &apos;&apos;scroll-behavior/smooth&apos;&apos;, or
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/cssom-view-1/#dom-scrollbehavior-auto">CSSOM View Module Level 1: 4 Extensions to the Window Interface</see>
    /// </remarks>
    [Description("@#auto")]
    Auto = 0,

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-view-1/#dom-scrollbehavior-instant">CSSOM View Module Level 1: 4 Extensions to the Window Interface</see>
    /// </summary>
    [Description("@#instant")]
    Instant = 1,

    /// <summary>
    /// behavior is &quot;auto&quot; and element is not null and its computed value of the &apos;scroll-behavior&apos; property is &apos;&apos;scroll-behavior/smooth&apos;&apos;, or
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/cssom-view-1/#dom-scrollbehavior-smooth">CSSOM View Module Level 1: 4 Extensions to the Window Interface</see>
    /// </remarks>
    [Description("@#smooth")]
    Smooth = 2
}

/// <summary>
/// <see href="https://drafts.csswg.org/cssom-view-1/#enumdef-scrollintoviewcontainer">CSSOM View Module Level 1: 6 Extensions to the Element Interface</see>
/// </summary>
[Description("@#ScrollIntoViewContainer")]
[ECMAScript]
[String]
public enum ScrollIntoViewContainer
{
    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-view-1/#dom-scrollintoviewcontainer-all">CSSOM View Module Level 1: 6 Extensions to the Element Interface</see>
    /// </summary>
    [Description("@#all")]
    All = 0,

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-view-1/#dom-scrollintoviewcontainer-nearest">CSSOM View Module Level 1: 6 Extensions to the Element Interface</see>
    /// </summary>
    [Description("@#nearest")]
    Nearest = 1
}

/// <summary>
/// <see href="https://drafts.csswg.org/cssom-view-1/#enumdef-scrolllogicalposition">CSSOM View Module Level 1: 6 Extensions to the Element Interface</see>
/// </summary>
[Description("@#ScrollLogicalPosition")]
[ECMAScript]
[String]
public enum ScrollLogicalPosition
{
    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-view-1/#dom-scrolllogicalposition-start">CSSOM View Module Level 1: 6 Extensions to the Element Interface</see>
    /// </summary>
    [Description("@#start")]
    Start = 0,

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-view-1/#dom-scrolllogicalposition-center">CSSOM View Module Level 1: 6 Extensions to the Element Interface</see>
    /// </summary>
    [Description("@#center")]
    Center = 1,

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-view-1/#dom-scrolllogicalposition-end">CSSOM View Module Level 1: 6 Extensions to the Element Interface</see>
    /// </summary>
    [Description("@#end")]
    End = 2,

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-view-1/#dom-scrolllogicalposition-nearest">CSSOM View Module Level 1: 6 Extensions to the Element Interface</see>
    /// </summary>
    [Description("@#nearest")]
    Nearest = 3
}

/// <summary>
/// <see href="https://drafts.csswg.org/pointer-animations-1/#enumdef-pointeraxis">Pointer-driven Animations Module Level 1: 2.3.2 The PointerTimeline Interface</see>
/// </summary>
[Description("@#PointerAxis")]
[ECMAScript]
[String]
public enum PointerAxis
{
    /// <summary>
    /// <see href="https://drafts.csswg.org/pointer-animations-1/#dom-pointeraxis-block">Pointer-driven Animations Module Level 1: 2.3.2 The PointerTimeline Interface</see>
    /// </summary>
    [Description("@#block")]
    Block = 0,

    /// <summary>
    /// <see href="https://drafts.csswg.org/pointer-animations-1/#dom-pointeraxis-inline">Pointer-driven Animations Module Level 1: 2.3.2 The PointerTimeline Interface</see>
    /// </summary>
    [Description("@#inline")]
    Inline = 1,

    /// <summary>
    /// <see href="https://drafts.csswg.org/pointer-animations-1/#dom-pointeraxis-x">Pointer-driven Animations Module Level 1: 2.3.2 The PointerTimeline Interface</see>
    /// </summary>
    [Description("@#x")]
    X = 2,

    /// <summary>
    /// <see href="https://drafts.csswg.org/pointer-animations-1/#dom-pointeraxis-y">Pointer-driven Animations Module Level 1: 2.3.2 The PointerTimeline Interface</see>
    /// </summary>
    [Description("@#y")]
    Y = 3
}

/// <summary>
/// <see href="https://drafts.csswg.org/resize-observer-1/#enumdef-resizeobserverboxoptions">Resize Observer Module Level 1: 2.1 ResizeObserver interface</see>
/// </summary>
[Description("@#ResizeObserverBoxOptions")]
[ECMAScript]
[String]
public enum ResizeObserverBoxOptions
{
    /// <summary>
    /// <see href="https://drafts.csswg.org/resize-observer-1/#dom-resizeobserverboxoptions-border-box">Resize Observer Module Level 1: 2.1 ResizeObserver interface</see>
    /// </summary>
    [Description("@#border-box")]
    BorderBox = 0,

    /// <summary>
    /// <see href="https://drafts.csswg.org/resize-observer-1/#dom-resizeobserverboxoptions-content-box">Resize Observer Module Level 1: 2.1 ResizeObserver interface</see>
    /// </summary>
    [Description("@#content-box")]
    ContentBox = 1,

    /// <summary>
    /// <see href="https://drafts.csswg.org/resize-observer-1/#dom-resizeobserverboxoptions-device-pixel-content-box">Resize Observer Module Level 1: 2.1 ResizeObserver interface</see>
    /// </summary>
    [Description("@#device-pixel-content-box")]
    DevicePixelContentBox = 2
}

/// <summary>
/// <see href="https://drafts.csswg.org/scroll-animations-1/#enumdef-scrollaxis">Scroll-driven Animations Module Level 1: 2.2.2 The ScrollTimeline Interface</see>
/// </summary>
[Description("@#ScrollAxis")]
[ECMAScript]
[String]
public enum ScrollAxis
{
    /// <summary>
    /// <see href="https://drafts.csswg.org/scroll-animations-1/#dom-scrollaxis-block">Scroll-driven Animations Module Level 1: 2.2.2 The ScrollTimeline Interface</see>
    /// </summary>
    [Description("@#block")]
    Block = 0,

    /// <summary>
    /// <see href="https://drafts.csswg.org/scroll-animations-1/#dom-scrollaxis-inline">Scroll-driven Animations Module Level 1: 2.2.2 The ScrollTimeline Interface</see>
    /// </summary>
    [Description("@#inline")]
    Inline = 1,

    /// <summary>
    /// <see href="https://drafts.csswg.org/scroll-animations-1/#dom-scrollaxis-x">Scroll-driven Animations Module Level 1: 2.2.2 The ScrollTimeline Interface</see>
    /// </summary>
    [Description("@#x")]
    X = 2,

    /// <summary>
    /// <see href="https://drafts.csswg.org/scroll-animations-1/#dom-scrollaxis-y">Scroll-driven Animations Module Level 1: 2.2.2 The ScrollTimeline Interface</see>
    /// </summary>
    [Description("@#y")]
    Y = 3
}

/// <summary>
/// <see href="https://drafts.csswg.org/web-animations-1/#enumdef-animationplaystate">Web Animations Module Level 1: 6.4.1 The AnimationPlayState enumeration</see>
/// </summary>
[Description("@#AnimationPlayState")]
[ECMAScript]
[String]
public enum AnimationPlayState
{
    /// <summary>
    /// <see href="https://drafts.csswg.org/web-animations-1/#dom-animationplaystate-idle">Web Animations Module Level 1: 6.4.1 The AnimationPlayState enumeration</see>
    /// </summary>
    [Description("@#idle")]
    Idle = 0,

    /// <summary>
    /// <see href="https://drafts.csswg.org/web-animations-1/#dom-animationplaystate-running">Web Animations Module Level 1: 6.4.1 The AnimationPlayState enumeration</see>
    /// </summary>
    [Description("@#running")]
    Running = 1,

    /// <summary>
    /// <see href="https://drafts.csswg.org/web-animations-1/#dom-animationplaystate-paused">Web Animations Module Level 1: 6.4.1 The AnimationPlayState enumeration</see>
    /// </summary>
    [Description("@#paused")]
    Paused = 2,

    /// <summary>
    /// <see href="https://drafts.csswg.org/web-animations-1/#dom-animationplaystate-finished">Web Animations Module Level 1: 6.4.1 The AnimationPlayState enumeration</see>
    /// </summary>
    [Description("@#finished")]
    Finished = 3
}

/// <summary>
/// <see href="https://drafts.csswg.org/web-animations-1/#enumdef-animationreplacestate">Web Animations Module Level 1: 6.4.2 The AnimationReplaceState enumeration</see>
/// </summary>
[Description("@#AnimationReplaceState")]
[ECMAScript]
[String]
public enum AnimationReplaceState
{
    /// <summary>
    /// <see href="https://drafts.csswg.org/web-animations-1/#dom-animationreplacestate-active">Web Animations Module Level 1: 6.4.2 The AnimationReplaceState enumeration</see>
    /// </summary>
    [Description("@#active")]
    Active = 0,

    /// <summary>
    /// <see href="https://drafts.csswg.org/web-animations-1/#dom-animationreplacestate-removed">Web Animations Module Level 1: 6.4.2 The AnimationReplaceState enumeration</see>
    /// </summary>
    [Description("@#removed")]
    Removed = 1,

    /// <summary>
    /// <see href="https://drafts.csswg.org/web-animations-1/#dom-animationreplacestate-persisted">Web Animations Module Level 1: 6.4.2 The AnimationReplaceState enumeration</see>
    /// </summary>
    [Description("@#persisted")]
    Persisted = 2
}

/// <summary>
/// <see href="https://drafts.csswg.org/web-animations-1/#enumdef-compositeoperation">Web Animations Module Level 1: 6.7 The CompositeOperation and CompositeOperationOrAuto enumerations</see>
/// </summary>
[Description("@#CompositeOperation")]
[ECMAScript]
[String]
public enum CompositeOperation
{
    /// <summary>
    /// <see href="https://drafts.csswg.org/web-animations-1/#dom-compositeoperation-replace">Web Animations Module Level 1: 6.7 The CompositeOperation and CompositeOperationOrAuto enumerations</see>
    /// </summary>
    [Description("@#replace")]
    Replace = 0,

    /// <summary>
    /// <see href="https://drafts.csswg.org/web-animations-1/#dom-compositeoperation-add">Web Animations Module Level 1: 6.7 The CompositeOperation and CompositeOperationOrAuto enumerations</see>
    /// </summary>
    [Description("@#add")]
    Add = 1,

    /// <summary>
    /// <see href="https://drafts.csswg.org/web-animations-1/#dom-compositeoperation-accumulate">Web Animations Module Level 1: 6.7 The CompositeOperation and CompositeOperationOrAuto enumerations</see>
    /// </summary>
    [Description("@#accumulate")]
    Accumulate = 2
}

/// <summary>
/// <see href="https://drafts.csswg.org/web-animations-1/#enumdef-compositeoperationorauto">Web Animations Module Level 1: 6.7 The CompositeOperation and CompositeOperationOrAuto enumerations</see>
/// </summary>
[Description("@#CompositeOperationOrAuto")]
[ECMAScript]
[String]
public enum CompositeOperationOrAuto
{
    /// <summary>
    /// <see href="https://drafts.csswg.org/web-animations-1/#dom-compositeoperation-replace">Web Animations Module Level 1: 6.7 The CompositeOperation and CompositeOperationOrAuto enumerations</see>
    /// </summary>
    [Description("@#replace")]
    Replace = 0,

    /// <summary>
    /// <see href="https://drafts.csswg.org/web-animations-1/#dom-compositeoperation-add">Web Animations Module Level 1: 6.7 The CompositeOperation and CompositeOperationOrAuto enumerations</see>
    /// </summary>
    [Description("@#add")]
    Add = 1,

    /// <summary>
    /// <see href="https://drafts.csswg.org/web-animations-1/#dom-compositeoperation-accumulate">Web Animations Module Level 1: 6.7 The CompositeOperation and CompositeOperationOrAuto enumerations</see>
    /// </summary>
    [Description("@#accumulate")]
    Accumulate = 2,

    /// <summary>
    /// <see href="https://drafts.csswg.org/web-animations-1/#dom-compositeoperationorauto-auto">Web Animations Module Level 1: 6.7 The CompositeOperation and CompositeOperationOrAuto enumerations</see>
    /// </summary>
    [Description("@#auto")]
    Auto = 3
}

/// <summary>
/// <see href="https://drafts.csswg.org/web-animations-1/#enumdef-fillmode">Web Animations Module Level 1: 6.5.2 The FillMode enumeration</see>
/// </summary>
[Description("@#FillMode")]
[ECMAScript]
[String]
public enum FillMode
{
    /// <summary>
    /// <see href="https://drafts.csswg.org/web-animations-1/#dom-fillmode-none">Web Animations Module Level 1: 6.5.2 The FillMode enumeration</see>
    /// </summary>
    [Description("@#none")]
    None = 0,

    /// <summary>
    /// <see href="https://drafts.csswg.org/web-animations-1/#dom-fillmode-forwards">Web Animations Module Level 1: 6.5.2 The FillMode enumeration</see>
    /// </summary>
    [Description("@#forwards")]
    Forwards = 1,

    /// <summary>
    /// <see href="https://drafts.csswg.org/web-animations-1/#dom-fillmode-backwards">Web Animations Module Level 1: 6.5.2 The FillMode enumeration</see>
    /// </summary>
    [Description("@#backwards")]
    Backwards = 2,

    /// <summary>
    /// <see href="https://drafts.csswg.org/web-animations-1/#dom-fillmode-both">Web Animations Module Level 1: 6.5.2 The FillMode enumeration</see>
    /// </summary>
    [Description("@#both")]
    Both = 3,

    /// <summary>
    /// <see href="https://drafts.csswg.org/web-animations-1/#dom-fillmode-auto">Web Animations Module Level 1: 6.5.2 The FillMode enumeration</see>
    /// </summary>
    [Description("@#auto")]
    Auto = 4
}

/// <summary>
/// <see href="https://drafts.csswg.org/web-animations-1/#enumdef-playbackdirection">Web Animations Module Level 1: 6.5.3 The PlaybackDirection enumeration</see>
/// </summary>
[Description("@#PlaybackDirection")]
[ECMAScript]
[String]
public enum PlaybackDirection
{
    /// <summary>
    /// <see href="https://drafts.csswg.org/web-animations-1/#dom-playbackdirection-normal">Web Animations Module Level 1: 6.5.3 The PlaybackDirection enumeration</see>
    /// </summary>
    [Description("@#normal")]
    Normal = 0,

    /// <summary>
    /// <see href="https://drafts.csswg.org/web-animations-1/#dom-playbackdirection-reverse">Web Animations Module Level 1: 6.5.3 The PlaybackDirection enumeration</see>
    /// </summary>
    [Description("@#reverse")]
    Reverse = 1,

    /// <summary>
    /// <see href="https://drafts.csswg.org/web-animations-1/#dom-playbackdirection-alternate">Web Animations Module Level 1: 6.5.3 The PlaybackDirection enumeration</see>
    /// </summary>
    [Description("@#alternate")]
    Alternate = 2,

    /// <summary>
    /// <see href="https://drafts.csswg.org/web-animations-1/#dom-playbackdirection-alternate-reverse">Web Animations Module Level 1: 6.5.3 The PlaybackDirection enumeration</see>
    /// </summary>
    [Description("@#alternate-reverse")]
    AlternateReverse = 3
}

/// <summary>
/// <see href="https://drafts.csswg.org/web-animations-2/#enumdef-animationtriggerbehavior">Web Animations Module Level 2: 4.18 The AnimationTriggerBehavior enumeration</see>
/// </summary>
[Description("@#AnimationTriggerBehavior")]
[ECMAScript]
[String]
public enum AnimationTriggerBehavior
{
    /// <summary>
    /// &apos;&apos;animation-trigger-behavior/once&apos;&apos;
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/web-animations-2/#dom-animationtriggerbehavior-once">Web Animations Module Level 2: 4.18 The AnimationTriggerBehavior enumeration</see>
    /// </remarks>
    [Description("@#once")]
    Once = 0,

    /// <summary>
    /// &apos;&apos;animation-trigger-behavior/repeat&apos;&apos;
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/web-animations-2/#dom-animationtriggerbehavior-repeat">Web Animations Module Level 2: 4.18 The AnimationTriggerBehavior enumeration</see>
    /// </remarks>
    [Description("@#repeat")]
    Repeat = 1,

    /// <summary>
    /// &apos;&apos;animation-trigger-behavior/alternate&apos;&apos;
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/web-animations-2/#dom-animationtriggerbehavior-alternate">Web Animations Module Level 2: 4.18 The AnimationTriggerBehavior enumeration</see>
    /// </remarks>
    [Description("@#alternate")]
    Alternate = 2,

    /// <summary>
    /// &apos;&apos;animation-trigger-behavior/state&apos;&apos;
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/web-animations-2/#dom-animationtriggerbehavior-state">Web Animations Module Level 2: 4.18 The AnimationTriggerBehavior enumeration</see>
    /// </remarks>
    [Description("@#state")]
    State = 3
}

/// <summary>
/// <see href="https://drafts.csswg.org/web-animations-2/#enumdef-iterationcompositeoperation">Web Animations Module Level 2: 4.11 The IterationCompositeOperation enumeration</see>
/// </summary>
[Description("@#IterationCompositeOperation")]
[ECMAScript]
[String]
public enum IterationCompositeOperation
{
    /// <summary>
    /// <see href="https://drafts.csswg.org/web-animations-2/#dom-iterationcompositeoperation-replace">Web Animations Module Level 2: 4.11 The IterationCompositeOperation enumeration</see>
    /// </summary>
    [Description("@#replace")]
    Replace = 0,

    /// <summary>
    /// <see href="https://drafts.csswg.org/web-animations-2/#dom-iterationcompositeoperation-accumulate">Web Animations Module Level 2: 4.11 The IterationCompositeOperation enumeration</see>
    /// </summary>
    [Description("@#accumulate")]
    Accumulate = 1
}

/// <summary>
/// <see href="https://fetch.spec.whatwg.org/#enumdef-requestduplex">Fetch Standard: 5.4 Request class</see>
/// </summary>
[Description("@#RequestDuplex")]
[ECMAScript]
[String]
public enum RequestDuplex
{
    /// <summary>
    /// <see href="https://fetch.spec.whatwg.org/#dom-requestduplex-half">Fetch Standard: 5.4 Request class</see>
    /// </summary>
    [Description("@#half")]
    Half = 0
}

/// <summary>
/// <see href="https://fetch.spec.whatwg.org/#enumdef-requestpriority">Fetch Standard: 5.4 Request class</see>
/// </summary>
[Description("@#RequestPriority")]
[ECMAScript]
[String]
public enum RequestPriority
{
    /// <summary>
    /// A request has an associated priority, which is &quot;high&quot;, &quot;low&quot;, or &quot;auto&quot;. Unless stated otherwise it is &quot;auto&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://fetch.spec.whatwg.org/#dom-requestpriority-high">Fetch Standard: 5.4 Request class</see>
    /// </remarks>
    [Description("@#high")]
    High = 0,

    /// <summary>
    /// A request has an associated priority, which is &quot;high&quot;, &quot;low&quot;, or &quot;auto&quot;. Unless stated otherwise it is &quot;auto&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://fetch.spec.whatwg.org/#dom-requestpriority-low">Fetch Standard: 5.4 Request class</see>
    /// </remarks>
    [Description("@#low")]
    Low = 1,

    /// <summary>
    /// A request has an associated priority, which is &quot;high&quot;, &quot;low&quot;, or &quot;auto&quot;. Unless stated otherwise it is &quot;auto&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://fetch.spec.whatwg.org/#dom-requestpriority-auto">Fetch Standard: 5.4 Request class</see>
    /// </remarks>
    [Description("@#auto")]
    Auto = 2
}

/// <summary>
/// <see href="https://fetch.spec.whatwg.org/#requestcache">Fetch Standard: 5.4 Request class</see>
/// </summary>
[Description("@#RequestCache")]
[ECMAScript]
[String]
public enum RequestCache
{
    /// <summary>
    /// A request has an associated cache mode, which is &quot;default&quot;, &quot;no-store&quot;, &quot;reload&quot;, &quot;no-cache&quot;, &quot;force-cache&quot;, or &quot;only-if-cached&quot;. Unless stated otherwise, it is &quot;default&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://fetch.spec.whatwg.org/#dom-requestcache-default">Fetch Standard: 5.4 Request class</see>
    /// </remarks>
    [Description("@#default")]
    Default = 0,

    /// <summary>
    /// A request has an associated cache mode, which is &quot;default&quot;, &quot;no-store&quot;, &quot;reload&quot;, &quot;no-cache&quot;, &quot;force-cache&quot;, or &quot;only-if-cached&quot;. Unless stated otherwise, it is &quot;default&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://fetch.spec.whatwg.org/#dom-requestcache-no-store">Fetch Standard: 5.4 Request class</see>
    /// </remarks>
    [Description("@#no-store")]
    NoStore = 1,

    /// <summary>
    /// A request has an associated cache mode, which is &quot;default&quot;, &quot;no-store&quot;, &quot;reload&quot;, &quot;no-cache&quot;, &quot;force-cache&quot;, or &quot;only-if-cached&quot;. Unless stated otherwise, it is &quot;default&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://fetch.spec.whatwg.org/#dom-requestcache-reload">Fetch Standard: 5.4 Request class</see>
    /// </remarks>
    [Description("@#reload")]
    Reload = 2,

    /// <summary>
    /// A request has an associated cache mode, which is &quot;default&quot;, &quot;no-store&quot;, &quot;reload&quot;, &quot;no-cache&quot;, &quot;force-cache&quot;, or &quot;only-if-cached&quot;. Unless stated otherwise, it is &quot;default&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://fetch.spec.whatwg.org/#dom-requestcache-no-cache">Fetch Standard: 5.4 Request class</see>
    /// </remarks>
    [Description("@#no-cache")]
    NoCache = 3,

    /// <summary>
    /// A request has an associated cache mode, which is &quot;default&quot;, &quot;no-store&quot;, &quot;reload&quot;, &quot;no-cache&quot;, &quot;force-cache&quot;, or &quot;only-if-cached&quot;. Unless stated otherwise, it is &quot;default&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://fetch.spec.whatwg.org/#dom-requestcache-force-cache">Fetch Standard: 5.4 Request class</see>
    /// </remarks>
    [Description("@#force-cache")]
    ForceCache = 4,

    /// <summary>
    /// A request has an associated cache mode, which is &quot;default&quot;, &quot;no-store&quot;, &quot;reload&quot;, &quot;no-cache&quot;, &quot;force-cache&quot;, or &quot;only-if-cached&quot;. Unless stated otherwise, it is &quot;default&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://fetch.spec.whatwg.org/#dom-requestcache-only-if-cached">Fetch Standard: 5.4 Request class</see>
    /// </remarks>
    [Description("@#only-if-cached")]
    OnlyIfCached = 5
}

/// <summary>
/// <see href="https://fetch.spec.whatwg.org/#requestcredentials">Fetch Standard: 5.4 Request class</see>
/// </summary>
[Description("@#RequestCredentials")]
[ECMAScript]
[String]
public enum RequestCredentials
{
    /// <summary>
    /// A request has an associated credentials mode, which is &quot;omit&quot;, &quot;same-origin&quot;, or &quot;include&quot;. Unless stated otherwise, it is &quot;same-origin&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://fetch.spec.whatwg.org/#dom-requestcredentials-omit">Fetch Standard: 5.4 Request class</see>
    /// </remarks>
    [Description("@#omit")]
    Omit = 0,

    /// <summary>
    /// A request has an associated credentials mode, which is &quot;omit&quot;, &quot;same-origin&quot;, or &quot;include&quot;. Unless stated otherwise, it is &quot;same-origin&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://fetch.spec.whatwg.org/#dom-requestcredentials-same-origin">Fetch Standard: 5.4 Request class</see>
    /// </remarks>
    [Description("@#same-origin")]
    SameOrigin = 1,

    /// <summary>
    /// A request has an associated credentials mode, which is &quot;omit&quot;, &quot;same-origin&quot;, or &quot;include&quot;. Unless stated otherwise, it is &quot;same-origin&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://fetch.spec.whatwg.org/#dom-requestcredentials-include">Fetch Standard: 5.4 Request class</see>
    /// </remarks>
    [Description("@#include")]
    Include = 2
}

/// <summary>
/// <see href="https://fetch.spec.whatwg.org/#requestredirect">Fetch Standard: 5.4 Request class</see>
/// </summary>
[Description("@#RequestRedirect")]
[ECMAScript]
[String]
public enum RequestRedirect
{
    /// <summary>
    /// Fetch uses any response in the HTTP cache matching the request, not paying attention to staleness. If there was no response, it returns a network error. (Can only be used when request&apos;s mode is &quot;same-origin&quot;. Any cached redirects will be followed assuming request&apos;s redirect mode is &quot;follow&quot; and the redirects do not violate request&apos;s mode.)
    /// </summary>
    /// <remarks>
    /// <see href="https://fetch.spec.whatwg.org/#dom-requestredirect-follow">Fetch Standard: 5.4 Request class</see>
    /// </remarks>
    [Description("@#follow")]
    Follow = 0,

    /// <summary>
    /// Fetch uses any response in the HTTP cache matching the request, not paying attention to staleness. If there was no response, it returns a network error. (Can only be used when request&apos;s mode is &quot;same-origin&quot;. Any cached redirects will be followed assuming request&apos;s redirect mode is &quot;follow&quot; and the redirects do not violate request&apos;s mode.)
    /// </summary>
    /// <remarks>
    /// <see href="https://fetch.spec.whatwg.org/#dom-requestredirect-error">Fetch Standard: 5.4 Request class</see>
    /// </remarks>
    [Description("@#error")]
    Error = 1,

    /// <summary>
    /// A request has an associated redirect mode, which is &quot;follow&quot;, &quot;error&quot;, or &quot;manual&quot;. Unless stated otherwise, it is &quot;follow&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://fetch.spec.whatwg.org/#dom-requestredirect-manual">Fetch Standard: 5.4 Request class</see>
    /// </remarks>
    [Description("@#manual")]
    Manual = 2
}

/// <summary>
/// <see href="https://fetch.spec.whatwg.org/#responsetype">Fetch Standard: 5.5 Response class</see>
/// </summary>
[Description("@#ResponseType")]
[ECMAScript]
[String]
public enum ResponseType
{
    /// <summary>
    /// A response has an associated type which is &quot;basic&quot;, &quot;cors&quot;, &quot;default&quot;, &quot;error&quot;, &quot;opaque&quot;, or &quot;opaqueredirect&quot;. Unless stated otherwise, it is &quot;default&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://fetch.spec.whatwg.org/#dom-responsetype-basic">Fetch Standard: 5.5 Response class</see>
    /// </remarks>
    [Description("@#basic")]
    Basic = 0,

    /// <summary>
    /// A response has an associated type which is &quot;basic&quot;, &quot;cors&quot;, &quot;default&quot;, &quot;error&quot;, &quot;opaque&quot;, or &quot;opaqueredirect&quot;. Unless stated otherwise, it is &quot;default&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://fetch.spec.whatwg.org/#dom-responsetype-cors">Fetch Standard: 5.5 Response class</see>
    /// </remarks>
    [Description("@#cors")]
    Cors = 1,

    /// <summary>
    /// A response has an associated type which is &quot;basic&quot;, &quot;cors&quot;, &quot;default&quot;, &quot;error&quot;, &quot;opaque&quot;, or &quot;opaqueredirect&quot;. Unless stated otherwise, it is &quot;default&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://fetch.spec.whatwg.org/#dom-responsetype-default">Fetch Standard: 5.5 Response class</see>
    /// </remarks>
    [Description("@#default")]
    Default = 2,

    /// <summary>
    /// A response has an associated type which is &quot;basic&quot;, &quot;cors&quot;, &quot;default&quot;, &quot;error&quot;, &quot;opaque&quot;, or &quot;opaqueredirect&quot;. Unless stated otherwise, it is &quot;default&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://fetch.spec.whatwg.org/#dom-responsetype-error">Fetch Standard: 5.5 Response class</see>
    /// </remarks>
    [Description("@#error")]
    Error = 3,

    /// <summary>
    /// A response has an associated type which is &quot;basic&quot;, &quot;cors&quot;, &quot;default&quot;, &quot;error&quot;, &quot;opaque&quot;, or &quot;opaqueredirect&quot;. Unless stated otherwise, it is &quot;default&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://fetch.spec.whatwg.org/#dom-responsetype-opaque">Fetch Standard: 5.5 Response class</see>
    /// </remarks>
    [Description("@#opaque")]
    Opaque = 4,

    /// <summary>
    /// A response has an associated type which is &quot;basic&quot;, &quot;cors&quot;, &quot;default&quot;, &quot;error&quot;, &quot;opaque&quot;, or &quot;opaqueredirect&quot;. Unless stated otherwise, it is &quot;default&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://fetch.spec.whatwg.org/#dom-responsetype-opaqueredirect">Fetch Standard: 5.5 Response class</see>
    /// </remarks>
    [Description("@#opaqueredirect")]
    Opaqueredirect = 5
}

/// <summary>
/// <see href="https://fs.spec.whatwg.org/#enumdef-filesystemhandlekind">File System Standard: 2.2 The FileSystemHandle interface</see>
/// </summary>
[Description("@#FileSystemHandleKind")]
[ECMAScript]
[String]
public enum FileSystemHandleKind
{
    /// <summary>
    /// <see href="https://fs.spec.whatwg.org/#dom-filesystemhandlekind-file">File System Standard: 2.2 The FileSystemHandle interface</see>
    /// </summary>
    [Description("@#file")]
    File = 0,

    /// <summary>
    /// <see href="https://fs.spec.whatwg.org/#dom-filesystemhandlekind-directory">File System Standard: 2.2 The FileSystemHandle interface</see>
    /// </summary>
    [Description("@#directory")]
    Directory = 1
}

/// <summary>
/// <see href="https://fs.spec.whatwg.org/#enumdef-writecommandtype">File System Standard: 2.5 The FileSystemWritableFileStream interface</see>
/// </summary>
[Description("@#WriteCommandType")]
[ECMAScript]
[String]
public enum WriteCommandType
{
    /// <summary>
    /// <see href="https://fs.spec.whatwg.org/#dom-writecommandtype-write">File System Standard: 2.5 The FileSystemWritableFileStream interface</see>
    /// </summary>
    /// <example>
    /// <code>await stream.write({ type: &quot;write&quot;, data: data })</code>
    /// </example>
    [Description("@#write")]
    Write = 0,

    /// <summary>
    /// <see href="https://fs.spec.whatwg.org/#dom-writecommandtype-seek">File System Standard: 2.5 The FileSystemWritableFileStream interface</see>
    /// </summary>
    /// <example>
    /// <code>await stream.write({ type: &quot;seek&quot;, position: position })</code>
    /// </example>
    [Description("@#seek")]
    Seek = 1,

    /// <summary>
    /// <see href="https://fs.spec.whatwg.org/#dom-writecommandtype-truncate">File System Standard: 2.5 The FileSystemWritableFileStream interface</see>
    /// </summary>
    /// <example>
    /// <code>await stream.write({ type: &quot;truncate&quot;, size: size })</code>
    /// </example>
    [Description("@#truncate")]
    Truncate = 2
}

/// <summary>
/// <see href="https://fullscreen.spec.whatwg.org/#enumdef-fullscreenkeyboardlock">Fullscreen API Standard: 3 API</see>
/// </summary>
[Description("@#FullscreenKeyboardLock")]
[ECMAScript]
[String]
public enum FullscreenKeyboardLock
{
    /// <summary>
    /// <see href="https://fullscreen.spec.whatwg.org/#dom-fullscreenkeyboardlock-browser">Fullscreen API Standard: 3 API</see>
    /// </summary>
    [Description("@#browser")]
    Browser = 0,

    /// <summary>
    /// <see href="https://fullscreen.spec.whatwg.org/#dom-fullscreenkeyboardlock-none">Fullscreen API Standard: 3 API</see>
    /// </summary>
    [Description("@#none")]
    None = 1
}

/// <summary>
/// <see href="https://fullscreen.spec.whatwg.org/#enumdef-fullscreennavigationui">Fullscreen API Standard: 3 API</see>
/// </summary>
[Description("@#FullscreenNavigationUI")]
[ECMAScript]
[String]
public enum FullscreenNavigationUI
{
    /// <summary>
    /// <see href="https://fullscreen.spec.whatwg.org/#dom-fullscreennavigationui-auto">Fullscreen API Standard: 3 API</see>
    /// </summary>
    [Description("@#auto")]
    Auto = 0,

    /// <summary>
    /// <see href="https://fullscreen.spec.whatwg.org/#dom-fullscreennavigationui-show">Fullscreen API Standard: 3 API</see>
    /// </summary>
    [Description("@#show")]
    Show = 1,

    /// <summary>
    /// <see href="https://fullscreen.spec.whatwg.org/#dom-fullscreennavigationui-hide">Fullscreen API Standard: 3 API</see>
    /// </summary>
    [Description("@#hide")]
    Hide = 2
}

/// <summary>
/// <see href="https://html.spec.whatwg.org/multipage/canvas.html#canvascolortype">HTML Standard: 4.12.5.1 The 2D rendering context</see>
/// </summary>
[Description("@#CanvasColorType")]
[ECMAScript]
[String]
public enum CanvasColorType
{
    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-canvascolortype-unorm8">HTML Standard: 4.12.5.1 The 2D rendering context</see>
    /// </summary>
    [Description("@#unorm8")]
    Unorm8 = 0,

    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-canvascolortype-float16">HTML Standard: 4.12.5.1 The 2D rendering context</see>
    /// </summary>
    [Description("@#float16")]
    Float16 = 1
}

/// <summary>
/// <see href="https://html.spec.whatwg.org/multipage/canvas.html#canvasdirection">HTML Standard: 4.12.5.1 The 2D rendering context</see>
/// </summary>
[Description("@#CanvasDirection")]
[ECMAScript]
[String]
public enum CanvasDirection
{
    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-direction-ltr">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </summary>
    [Description("@#ltr")]
    Ltr = 0,

    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-direction-rtl">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </summary>
    [Description("@#rtl")]
    Rtl = 1,

    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-direction-inherit">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </summary>
    [Description("@#inherit")]
    Inherit = 2
}

/// <summary>
/// <see href="https://html.spec.whatwg.org/multipage/canvas.html#canvasfillrule">HTML Standard: 4.12.5.1 The 2D rendering context</see>
/// </summary>
[Description("@#CanvasFillRule")]
[ECMAScript]
[String]
public enum CanvasFillRule
{
    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-fillrule-nonzero">HTML Standard: 4.12.5.1 The 2D rendering context</see>
    /// </summary>
    [Description("@#nonzero")]
    Nonzero = 0,

    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-fillrule-evenodd">HTML Standard: 4.12.5.1 The 2D rendering context</see>
    /// </summary>
    [Description("@#evenodd")]
    Evenodd = 1
}

/// <summary>
/// <see href="https://html.spec.whatwg.org/multipage/canvas.html#canvasfontkerning">HTML Standard: 4.12.5.1 The 2D rendering context</see>
/// </summary>
[Description("@#CanvasFontKerning")]
[ECMAScript]
[String]
public enum CanvasFontKerning
{
    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-fontkerning-auto">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </summary>
    [Description("@#auto")]
    Auto = 0,

    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-fontkerning-normal">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </summary>
    [Description("@#normal")]
    Normal = 1,

    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-fontkerning-none">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </summary>
    [Description("@#none")]
    None = 2
}

/// <summary>
/// <see href="https://html.spec.whatwg.org/multipage/canvas.html#canvasfontstretch">HTML Standard: 4.12.5.1 The 2D rendering context</see>
/// </summary>
[Description("@#CanvasFontStretch")]
[ECMAScript]
[String]
public enum CanvasFontStretch
{
    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-fontstretch-ultra-condensed">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </summary>
    [Description("@#ultra-condensed")]
    UltraCondensed = 0,

    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-fontstretch-extra-condensed">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </summary>
    [Description("@#extra-condensed")]
    ExtraCondensed = 1,

    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-fontstretch-condensed">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </summary>
    [Description("@#condensed")]
    Condensed = 2,

    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-fontstretch-semi-condensed">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </summary>
    [Description("@#semi-condensed")]
    SemiCondensed = 3,

    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-fontstretch-normal">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </summary>
    [Description("@#normal")]
    Normal = 4,

    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-fontstretch-semi-expanded">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </summary>
    [Description("@#semi-expanded")]
    SemiExpanded = 5,

    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-fontstretch-expanded">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </summary>
    [Description("@#expanded")]
    Expanded = 6,

    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-fontstretch-extra-expanded">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </summary>
    [Description("@#extra-expanded")]
    ExtraExpanded = 7,

    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-fontstretch-ultra-expanded">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </summary>
    [Description("@#ultra-expanded")]
    UltraExpanded = 8
}

/// <summary>
/// <see href="https://html.spec.whatwg.org/multipage/canvas.html#canvasfontvariantcaps">HTML Standard: 4.12.5.1 The 2D rendering context</see>
/// </summary>
[Description("@#CanvasFontVariantCaps")]
[ECMAScript]
[String]
public enum CanvasFontVariantCaps
{
    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-fontvariantcaps-normal">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </summary>
    [Description("@#normal")]
    Normal = 0,

    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-fontvariantcaps-small-caps">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </summary>
    [Description("@#small-caps")]
    SmallCaps = 1,

    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-fontvariantcaps-all-small-caps">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </summary>
    [Description("@#all-small-caps")]
    AllSmallCaps = 2,

    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-fontvariantcaps-petite-caps">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </summary>
    [Description("@#petite-caps")]
    PetiteCaps = 3,

    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-fontvariantcaps-all-petite-caps">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </summary>
    [Description("@#all-petite-caps")]
    AllPetiteCaps = 4,

    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-fontvariantcaps-unicase">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </summary>
    [Description("@#unicase")]
    Unicase = 5,

    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-fontvariantcaps-titling-caps">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </summary>
    [Description("@#titling-caps")]
    TitlingCaps = 6
}

/// <summary>
/// <see href="https://html.spec.whatwg.org/multipage/canvas.html#canvaslinecap">HTML Standard: 4.12.5.1 The 2D rendering context</see>
/// </summary>
[Description("@#CanvasLineCap")]
[ECMAScript]
[String]
public enum CanvasLineCap
{
    [Description("@#butt")]
    Butt = 0,

    [Description("@#round")]
    Round = 1,

    [Description("@#square")]
    Square = 2
}

/// <summary>
/// <see href="https://html.spec.whatwg.org/multipage/canvas.html#canvaslinejoin">HTML Standard: 4.12.5.1 The 2D rendering context</see>
/// </summary>
[Description("@#CanvasLineJoin")]
[ECMAScript]
[String]
public enum CanvasLineJoin
{
    [Description("@#round")]
    Round = 0,

    [Description("@#bevel")]
    Bevel = 1,

    [Description("@#miter")]
    Miter = 2
}

/// <summary>
/// <see href="https://html.spec.whatwg.org/multipage/canvas.html#canvastextalign">HTML Standard: 4.12.5.1 The 2D rendering context</see>
/// </summary>
[Description("@#CanvasTextAlign")]
[ECMAScript]
[String]
public enum CanvasTextAlign
{
    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-textalign-start">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </summary>
    [Description("@#start")]
    Start = 0,

    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-textalign-end">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </summary>
    [Description("@#end")]
    End = 1,

    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-textalign-left">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </summary>
    [Description("@#left")]
    Left = 2,

    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-textalign-right">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </summary>
    [Description("@#right")]
    Right = 3,

    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-textalign-center">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </summary>
    [Description("@#center")]
    Center = 4
}

/// <summary>
/// <see href="https://html.spec.whatwg.org/multipage/canvas.html#canvastextbaseline">HTML Standard: 4.12.5.1 The 2D rendering context</see>
/// </summary>
[Description("@#CanvasTextBaseline")]
[ECMAScript]
[String]
public enum CanvasTextBaseline
{
    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-textbaseline-top">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </summary>
    [Description("@#top")]
    Top = 0,

    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-textbaseline-hanging">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </summary>
    [Description("@#hanging")]
    Hanging = 1,

    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-textbaseline-middle">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </summary>
    [Description("@#middle")]
    Middle = 2,

    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-textbaseline-alphabetic">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </summary>
    [Description("@#alphabetic")]
    Alphabetic = 3,

    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-textbaseline-ideographic">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </summary>
    [Description("@#ideographic")]
    Ideographic = 4,

    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-textbaseline-bottom">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </summary>
    [Description("@#bottom")]
    Bottom = 5
}

/// <summary>
/// <see href="https://html.spec.whatwg.org/multipage/canvas.html#canvastextrendering">HTML Standard: 4.12.5.1 The 2D rendering context</see>
/// </summary>
[Description("@#CanvasTextRendering")]
[ECMAScript]
[String]
public enum CanvasTextRendering
{
    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-textrendering-auto">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </summary>
    [Description("@#auto")]
    Auto = 0,

    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-textrendering-optimizespeed">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </summary>
    [Description("@#optimizeSpeed")]
    OptimizeSpeed = 1,

    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-textrendering-optimizelegibility">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </summary>
    [Description("@#optimizeLegibility")]
    OptimizeLegibility = 2,

    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-textrendering-geometricprecision">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </summary>
    [Description("@#geometricPrecision")]
    GeometricPrecision = 3
}

/// <summary>
/// <see href="https://html.spec.whatwg.org/multipage/canvas.html#imagesmoothingquality">HTML Standard: 4.12.5.1 The 2D rendering context</see>
/// </summary>
[Description("@#ImageSmoothingQuality")]
[ECMAScript]
[String]
public enum ImageSmoothingQuality
{
    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-imagesmoothingquality-low">HTML Standard: 4.12.5.1 The 2D rendering context</see>
    /// </summary>
    [Description("@#low")]
    Low = 0,

    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-imagesmoothingquality-medium">HTML Standard: 4.12.5.1 The 2D rendering context</see>
    /// </summary>
    [Description("@#medium")]
    Medium = 1,

    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-imagesmoothingquality-high">HTML Standard: 4.12.5.1 The 2D rendering context</see>
    /// </summary>
    [Description("@#high")]
    High = 2
}

/// <summary>
/// <see href="https://html.spec.whatwg.org/multipage/canvas.html#offscreenrenderingcontextid">HTML Standard: 4.12.5.3 The OffscreenCanvas interface</see>
/// </summary>
[Description("@#OffscreenRenderingContextId")]
[ECMAScript]
[String]
public enum OffscreenRenderingContextId
{
    [Description("@#2d")]
    _2d = 0,

    [Description("@#bitmaprenderer")]
    Bitmaprenderer = 1,

    [Description("@#webgl")]
    Webgl = 2,

    [Description("@#webgl2")]
    Webgl2 = 3,

    [Description("@#webgpu")]
    Webgpu = 4
}

/// <summary>
/// <see href="https://html.spec.whatwg.org/multipage/canvas.html#predefinedcolorspace">HTML Standard: 4.12.5.4 Color spaces and color space conversion</see>
/// </summary>
[Description("@#PredefinedColorSpace")]
[ECMAScript]
[String]
public enum PredefinedColorSpace
{
    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-predefinedcolorspace-srgb">HTML Standard: 4.12.5.4 Color spaces and color space conversion</see>
    /// </summary>
    [Description("@#srgb")]
    Srgb = 0,

    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-predefinedcolorspace-srgb-linear">HTML Standard: 4.12.5.4 Color spaces and color space conversion</see>
    /// </summary>
    [Description("@#srgb-linear")]
    SrgbLinear = 1,

    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-predefinedcolorspace-display-p3">HTML Standard: 4.12.5.4 Color spaces and color space conversion</see>
    /// </summary>
    [Description("@#display-p3")]
    DisplayP3 = 2,

    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-predefinedcolorspace-display-p3-linear">HTML Standard: 4.12.5.4 Color spaces and color space conversion</see>
    /// </summary>
    [Description("@#display-p3-linear")]
    DisplayP3Linear = 3
}

/// <summary>
/// <see href="https://html.spec.whatwg.org/multipage/dom.html#documentreadystate">HTML Standard: 3.1.1 The Document object</see>
/// </summary>
[Description("@#DocumentReadyState")]
[ECMAScript]
[String]
public enum DocumentReadyState
{
    [Description("@#loading")]
    Loading = 0,

    [Description("@#interactive")]
    Interactive = 1,

    [Description("@#complete")]
    Complete = 2
}

/// <summary>
/// <see href="https://html.spec.whatwg.org/multipage/dom.html#documentvisibilitystate">HTML Standard: 3.1.1 The Document object</see>
/// </summary>
[Description("@#DocumentVisibilityState")]
[ECMAScript]
[String]
public enum DocumentVisibilityState
{
    [Description("@#visible")]
    Visible = 0,

    [Description("@#hidden")]
    Hidden = 1
}

/// <summary>
/// <see href="https://html.spec.whatwg.org/multipage/dynamic-markup-insertion.html#domparsersupportedtype">HTML Standard: 8.5.1 The DOMParser interface</see>
/// </summary>
[Description("@#DOMParserSupportedType")]
[ECMAScript]
[String]
public enum DOMParserSupportedType
{
    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/dynamic-markup-insertion.html#dom-domparsersupportedtype-texthtml">HTML Standard: 8.5.1 The DOMParser interface</see>
    /// </summary>
    [Description("@#text/html")]
    TextHtml = 0,

    [Description("@#text/xml")]
    TextXml = 1,

    [Description("@#application/xml")]
    ApplicationXml = 2,

    [Description("@#application/xhtml\u002Bxml")]
    ApplicationXhtmlXml = 3,

    [Description("@#image/svg\u002Bxml")]
    ImageSvgXml = 4
}

/// <summary>
/// <see href="https://html.spec.whatwg.org/multipage/dynamic-markup-insertion.html#sanitizerpresets">HTML Standard: 8.5 DOM parsing and serialization APIs</see>
/// </summary>
[Description("@#SanitizerPresets")]
[ECMAScript]
[String]
public enum SanitizerPresets
{
    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/dynamic-markup-insertion.html#dom-sanitizerpresets-default">HTML Standard: 8.6.3.1 Configuration invariants</see>
    /// </summary>
    [Description("@#default")]
    Default = 0
}

/// <summary>
/// <see href="https://html.spec.whatwg.org/multipage/form-control-infrastructure.html#selectionmode">HTML Standard: 4.10.20 APIs for the text control selections</see>
/// </summary>
[Description("@#SelectionMode")]
[ECMAScript]
[String]
public enum SelectionMode
{
    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/form-control-infrastructure.html#dom-selectionmode-select">HTML Standard: 4.10.20 APIs for the text control selections</see>
    /// </summary>
    [Description("@#select")]
    Select = 0,

    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/form-control-infrastructure.html#dom-selectionmode-start">HTML Standard: 4.10.20 APIs for the text control selections</see>
    /// </summary>
    [Description("@#start")]
    Start = 1,

    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/form-control-infrastructure.html#dom-selectionmode-end">HTML Standard: 4.10.20 APIs for the text control selections</see>
    /// </summary>
    [Description("@#end")]
    End = 2,

    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/form-control-infrastructure.html#dom-selectionmode-preserve">HTML Standard: 4.10.20 APIs for the text control selections</see>
    /// </summary>
    [Description("@#preserve")]
    Preserve = 3
}

/// <summary>
/// <see href="https://html.spec.whatwg.org/multipage/imagebitmap-and-animations.html#colorspaceconversion">HTML Standard: 8.11.2 The ImageBitmap interface</see>
/// </summary>
[Description("@#ColorSpaceConversion")]
[ECMAScript]
[String]
public enum ColorSpaceConversion
{
    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/imagebitmap-and-animations.html#dom-colorspaceconversion-none">HTML Standard: 8.11.2 The ImageBitmap interface</see>
    /// </summary>
    [Description("@#none")]
    None = 0,

    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/imagebitmap-and-animations.html#dom-colorspaceconversion-default">HTML Standard: 8.11.2 The ImageBitmap interface</see>
    /// </summary>
    [Description("@#default")]
    Default = 1
}

/// <summary>
/// <see href="https://html.spec.whatwg.org/multipage/imagebitmap-and-animations.html#imagedatapixelformat">HTML Standard: 8.11.1 The ImageData interface</see>
/// </summary>
[Description("@#ImageDataPixelFormat")]
[ECMAScript]
[String]
public enum ImageDataPixelFormat
{
    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/imagebitmap-and-animations.html#dom-imagedatapixelformat-rgba-unorm8">HTML Standard: 8.11.1 The ImageData interface</see>
    /// </summary>
    [Description("@#rgba-unorm8")]
    RgbaUnorm8 = 0,

    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/imagebitmap-and-animations.html#dom-imagedatapixelformat-rgba-float16">HTML Standard: 8.11.1 The ImageData interface</see>
    /// </summary>
    [Description("@#rgba-float16")]
    RgbaFloat16 = 1
}

/// <summary>
/// <see href="https://html.spec.whatwg.org/multipage/imagebitmap-and-animations.html#imageorientation">HTML Standard: 8.11.2 The ImageBitmap interface</see>
/// </summary>
[Description("@#ImageOrientation")]
[ECMAScript]
[String]
public enum ImageOrientation
{
    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/imagebitmap-and-animations.html#dom-imageorientation-from-image">HTML Standard: 8.11.2 The ImageBitmap interface</see>
    /// </summary>
    [Description("@#from-image")]
    FromImage = 0,

    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/imagebitmap-and-animations.html#dom-imageorientation-flipy">HTML Standard: 8.11.2 The ImageBitmap interface</see>
    /// </summary>
    [Description("@#flipY")]
    FlipY = 1
}

/// <summary>
/// <see href="https://html.spec.whatwg.org/multipage/imagebitmap-and-animations.html#premultiplyalpha">HTML Standard: 8.11.2 The ImageBitmap interface</see>
/// </summary>
[Description("@#PremultiplyAlpha")]
[ECMAScript]
[String]
public enum PremultiplyAlpha
{
    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/imagebitmap-and-animations.html#dom-premultiplyalpha-none">HTML Standard: 8.11.2 The ImageBitmap interface</see>
    /// </summary>
    [Description("@#none")]
    None = 0,

    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/imagebitmap-and-animations.html#dom-premultiplyalpha-premultiply">HTML Standard: 8.11.2 The ImageBitmap interface</see>
    /// </summary>
    [Description("@#premultiply")]
    Premultiply = 1,

    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/imagebitmap-and-animations.html#dom-premultiplyalpha-default">HTML Standard: 8.11.2 The ImageBitmap interface</see>
    /// </summary>
    [Description("@#default")]
    Default = 2
}

/// <summary>
/// <see href="https://html.spec.whatwg.org/multipage/imagebitmap-and-animations.html#resizequality">HTML Standard: 8.11.2 The ImageBitmap interface</see>
/// </summary>
[Description("@#ResizeQuality")]
[ECMAScript]
[String]
public enum ResizeQuality
{
    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/imagebitmap-and-animations.html#dom-resizequality-pixelated">HTML Standard: 8.11.2 The ImageBitmap interface</see>
    /// </summary>
    [Description("@#pixelated")]
    Pixelated = 0,

    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/imagebitmap-and-animations.html#dom-resizequality-low">HTML Standard: 8.11.2 The ImageBitmap interface</see>
    /// </summary>
    [Description("@#low")]
    Low = 1,

    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/imagebitmap-and-animations.html#dom-resizequality-medium">HTML Standard: 8.11.2 The ImageBitmap interface</see>
    /// </summary>
    [Description("@#medium")]
    Medium = 2,

    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/imagebitmap-and-animations.html#dom-resizequality-high">HTML Standard: 8.11.2 The ImageBitmap interface</see>
    /// </summary>
    [Description("@#high")]
    High = 3
}

/// <summary>
/// <see href="https://html.spec.whatwg.org/multipage/media.html#canplaytyperesult">HTML Standard: 4.8.11 Media elements</see>
/// </summary>
[Description("@#CanPlayTypeResult")]
[ECMAScript]
[String]
public enum CanPlayTypeResult
{
    [Description("@#")]
    Empty = 0,

    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/media.html#dom-canplaytyperesult-maybe">HTML Standard: 4.8.11.3 MIME types</see>
    /// </summary>
    [Description("@#maybe")]
    Maybe = 1,

    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/media.html#dom-canplaytyperesult-probably">HTML Standard: 4.8.11.3 MIME types</see>
    /// </summary>
    [Description("@#probably")]
    Probably = 2
}

/// <summary>
/// <see href="https://html.spec.whatwg.org/multipage/media.html#texttrackkind">HTML Standard: 4.8.11.11.5 Text track API</see>
/// </summary>
[Description("@#TextTrackKind")]
[ECMAScript]
[String]
public enum TextTrackKind
{
    [Description("@#subtitles")]
    Subtitles = 0,

    [Description("@#captions")]
    Captions = 1,

    [Description("@#descriptions")]
    Descriptions = 2,

    [Description("@#chapters")]
    Chapters = 3,

    [Description("@#metadata")]
    Metadata = 4
}

/// <summary>
/// <see href="https://html.spec.whatwg.org/multipage/media.html#texttrackmode">HTML Standard: 4.8.11.11.5 Text track API</see>
/// </summary>
[Description("@#TextTrackMode")]
[ECMAScript]
[String]
public enum TextTrackMode
{
    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/media.html#dom-texttrack-disabled">HTML Standard: 4.8.11.11.5 Text track API</see>
    /// </summary>
    [Description("@#disabled")]
    Disabled = 0,

    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/media.html#dom-texttrack-hidden">HTML Standard: 4.8.11.11.5 Text track API</see>
    /// </summary>
    [Description("@#hidden")]
    Hidden = 1,

    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/media.html#dom-texttrack-showing">HTML Standard: 4.8.11.11.5 Text track API</see>
    /// </summary>
    [Description("@#showing")]
    Showing = 2
}

/// <summary>
/// <see href="https://html.spec.whatwg.org/multipage/nav-history-apis.html#navigationfocusreset">HTML Standard: 7.2.6.10.1 The NavigateEvent interface</see>
/// </summary>
[Description("@#NavigationFocusReset")]
[ECMAScript]
[String]
public enum NavigationFocusReset
{
    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/nav-history-apis.html#dom-navigationfocusreset-after-transition">HTML Standard: 7.2.6.10.1 The NavigateEvent interface</see>
    /// </summary>
    [Description("@#after-transition")]
    AfterTransition = 0,

    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/nav-history-apis.html#dom-navigationfocusreset-manual">HTML Standard: 7.2.6.10.1 The NavigateEvent interface</see>
    /// </summary>
    [Description("@#manual")]
    Manual = 1
}

/// <summary>
/// <see href="https://html.spec.whatwg.org/multipage/nav-history-apis.html#navigationhistorybehavior">HTML Standard: 7.2.6.2 The Navigation interface</see>
/// </summary>
[Description("@#NavigationHistoryBehavior")]
[ECMAScript]
[String]
public enum NavigationHistoryBehavior
{
    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/browsing-the-web.html#navigationhistorybehavior-auto">HTML Standard: 7.4.2.1 Supporting concepts</see>
    /// </summary>
    [Description("@#auto")]
    Auto = 0,

    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/browsing-the-web.html#navigationhistorybehavior-push">HTML Standard: 7.4.2.1 Supporting concepts</see>
    /// </summary>
    [Description("@#push")]
    Push = 1,

    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/browsing-the-web.html#navigationhistorybehavior-replace">HTML Standard: 7.4.2.1 Supporting concepts</see>
    /// </summary>
    [Description("@#replace")]
    Replace = 2
}

/// <summary>
/// <see href="https://html.spec.whatwg.org/multipage/nav-history-apis.html#navigationscrollbehavior">HTML Standard: 7.2.6.10.1 The NavigateEvent interface</see>
/// </summary>
[Description("@#NavigationScrollBehavior")]
[ECMAScript]
[String]
public enum NavigationScrollBehavior
{
    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/nav-history-apis.html#dom-navigationscrollbehavior-after-transition">HTML Standard: 7.2.6.10.1 The NavigateEvent interface</see>
    /// </summary>
    [Description("@#after-transition")]
    AfterTransition = 0,

    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/nav-history-apis.html#dom-navigationscrollbehavior-manual">HTML Standard: 7.2.6.10.1 The NavigateEvent interface</see>
    /// </summary>
    [Description("@#manual")]
    Manual = 1
}

/// <summary>
/// <see href="https://html.spec.whatwg.org/multipage/nav-history-apis.html#navigationtype">HTML Standard: 7.2.6.3 Core infrastructure</see>
/// </summary>
[Description("@#NavigationType")]
[ECMAScript]
[String]
public enum NavigationType
{
    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/nav-history-apis.html#dom-navigationtype-push">HTML Standard: 7.2.6.3 Core infrastructure</see>
    /// </summary>
    [Description("@#push")]
    Push = 0,

    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/nav-history-apis.html#dom-navigationtype-replace">HTML Standard: 7.2.6.3 Core infrastructure</see>
    /// </summary>
    [Description("@#replace")]
    Replace = 1,

    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/nav-history-apis.html#dom-navigationtype-reload">HTML Standard: 7.2.6.3 Core infrastructure</see>
    /// </summary>
    [Description("@#reload")]
    Reload = 2,

    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/nav-history-apis.html#dom-navigationtype-traverse">HTML Standard: 7.2.6.3 Core infrastructure</see>
    /// </summary>
    [Description("@#traverse")]
    Traverse = 3
}

/// <summary>
/// <see href="https://html.spec.whatwg.org/multipage/nav-history-apis.html#scrollrestoration">HTML Standard: 7.2.5 The History interface</see>
/// </summary>
[Description("@#ScrollRestoration")]
[ECMAScript]
[String]
public enum ScrollRestoration
{
    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/browsing-the-web.html#dom-scrollrestoration-auto">HTML Standard: 7.4.1.1 Session history entries</see>
    /// </summary>
    [Description("@#auto")]
    Auto = 0,

    /// <summary>
    /// <see href="https://html.spec.whatwg.org/multipage/browsing-the-web.html#dom-scrollrestoration-manual">HTML Standard: 7.4.1.1 Session history entries</see>
    /// </summary>
    [Description("@#manual")]
    Manual = 1
}

/// <summary>
/// <see href="https://html.spec.whatwg.org/multipage/workers.html#workertype">HTML Standard: 10.2.6.3 Dedicated workers and the Worker interface</see>
/// </summary>
[Description("@#WorkerType")]
[ECMAScript]
[String]
public enum WorkerType
{
    [Description("@#classic")]
    Classic = 0,

    [Description("@#module")]
    Module = 1
}

/// <summary>
/// <see href="https://immersive-web.github.io/body-tracking/#enumdef-xrbodyjoint">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
/// </summary>
[Description("@#XRBodyJoint")]
[ECMAScript]
[String]
public enum XRBodyJoint
{
    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-hips">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#hips")]
    Hips = 0,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-spine-lower">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#spine-lower")]
    SpineLower = 1,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-spine-middle">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#spine-middle")]
    SpineMiddle = 2,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-spine-upper">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#spine-upper")]
    SpineUpper = 3,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-chest">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#chest")]
    Chest = 4,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-neck">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#neck")]
    Neck = 5,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-head">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#head")]
    Head = 6,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-shoulder">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#left-shoulder")]
    LeftShoulder = 7,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-scapula">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#left-scapula")]
    LeftScapula = 8,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-arm-upper">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#left-arm-upper")]
    LeftArmUpper = 9,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-arm-lower">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#left-arm-lower")]
    LeftArmLower = 10,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-hand-wrist-twist">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#left-hand-wrist-twist")]
    LeftHandWristTwist = 11,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-shoulder">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#right-shoulder")]
    RightShoulder = 12,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-scapula">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#right-scapula")]
    RightScapula = 13,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-arm-upper">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#right-arm-upper")]
    RightArmUpper = 14,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-arm-lower">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#right-arm-lower")]
    RightArmLower = 15,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-hand-wrist-twist">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#right-hand-wrist-twist")]
    RightHandWristTwist = 16,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-hand-palm">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#left-hand-palm")]
    LeftHandPalm = 17,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-hand-wrist">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#left-hand-wrist")]
    LeftHandWrist = 18,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-hand-thumb-metacarpal">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#left-hand-thumb-metacarpal")]
    LeftHandThumbMetacarpal = 19,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-hand-thumb-phalanx-proximal">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#left-hand-thumb-phalanx-proximal")]
    LeftHandThumbPhalanxProximal = 20,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-hand-thumb-phalanx-distal">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#left-hand-thumb-phalanx-distal")]
    LeftHandThumbPhalanxDistal = 21,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-hand-thumb-tip">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#left-hand-thumb-tip")]
    LeftHandThumbTip = 22,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-hand-index-metacarpal">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#left-hand-index-metacarpal")]
    LeftHandIndexMetacarpal = 23,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-hand-index-phalanx-proximal">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#left-hand-index-phalanx-proximal")]
    LeftHandIndexPhalanxProximal = 24,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-hand-index-phalanx-intermediate">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#left-hand-index-phalanx-intermediate")]
    LeftHandIndexPhalanxIntermediate = 25,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-hand-index-phalanx-distal">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#left-hand-index-phalanx-distal")]
    LeftHandIndexPhalanxDistal = 26,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-hand-index-tip">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#left-hand-index-tip")]
    LeftHandIndexTip = 27,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-hand-middle-phalanx-metacarpal">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#left-hand-middle-phalanx-metacarpal")]
    LeftHandMiddlePhalanxMetacarpal = 28,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-hand-middle-phalanx-proximal">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#left-hand-middle-phalanx-proximal")]
    LeftHandMiddlePhalanxProximal = 29,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-hand-middle-phalanx-intermediate">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#left-hand-middle-phalanx-intermediate")]
    LeftHandMiddlePhalanxIntermediate = 30,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-hand-middle-phalanx-distal">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#left-hand-middle-phalanx-distal")]
    LeftHandMiddlePhalanxDistal = 31,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-hand-middle-tip">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#left-hand-middle-tip")]
    LeftHandMiddleTip = 32,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-hand-ring-metacarpal">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#left-hand-ring-metacarpal")]
    LeftHandRingMetacarpal = 33,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-hand-ring-phalanx-proximal">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#left-hand-ring-phalanx-proximal")]
    LeftHandRingPhalanxProximal = 34,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-hand-ring-phalanx-intermediate">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#left-hand-ring-phalanx-intermediate")]
    LeftHandRingPhalanxIntermediate = 35,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-hand-ring-phalanx-distal">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#left-hand-ring-phalanx-distal")]
    LeftHandRingPhalanxDistal = 36,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-hand-ring-tip">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#left-hand-ring-tip")]
    LeftHandRingTip = 37,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-hand-little-metacarpal">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#left-hand-little-metacarpal")]
    LeftHandLittleMetacarpal = 38,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-hand-little-phalanx-proximal">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#left-hand-little-phalanx-proximal")]
    LeftHandLittlePhalanxProximal = 39,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-hand-little-phalanx-intermediate">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#left-hand-little-phalanx-intermediate")]
    LeftHandLittlePhalanxIntermediate = 40,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-hand-little-phalanx-distal">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#left-hand-little-phalanx-distal")]
    LeftHandLittlePhalanxDistal = 41,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-hand-little-tip">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#left-hand-little-tip")]
    LeftHandLittleTip = 42,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-hand-palm">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#right-hand-palm")]
    RightHandPalm = 43,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-hand-wrist">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#right-hand-wrist")]
    RightHandWrist = 44,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-hand-thumb-metacarpal">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#right-hand-thumb-metacarpal")]
    RightHandThumbMetacarpal = 45,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-hand-thumb-phalanx-proximal">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#right-hand-thumb-phalanx-proximal")]
    RightHandThumbPhalanxProximal = 46,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-hand-thumb-phalanx-distal">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#right-hand-thumb-phalanx-distal")]
    RightHandThumbPhalanxDistal = 47,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-hand-thumb-tip">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#right-hand-thumb-tip")]
    RightHandThumbTip = 48,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-hand-index-metacarpal">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#right-hand-index-metacarpal")]
    RightHandIndexMetacarpal = 49,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-hand-index-phalanx-proximal">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#right-hand-index-phalanx-proximal")]
    RightHandIndexPhalanxProximal = 50,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-hand-index-phalanx-intermediate">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#right-hand-index-phalanx-intermediate")]
    RightHandIndexPhalanxIntermediate = 51,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-hand-index-phalanx-distal">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#right-hand-index-phalanx-distal")]
    RightHandIndexPhalanxDistal = 52,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-hand-index-tip">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#right-hand-index-tip")]
    RightHandIndexTip = 53,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-hand-middle-metacarpal">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#right-hand-middle-metacarpal")]
    RightHandMiddleMetacarpal = 54,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-hand-middle-phalanx-proximal">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#right-hand-middle-phalanx-proximal")]
    RightHandMiddlePhalanxProximal = 55,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-hand-middle-phalanx-intermediate">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#right-hand-middle-phalanx-intermediate")]
    RightHandMiddlePhalanxIntermediate = 56,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-hand-middle-phalanx-distal">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#right-hand-middle-phalanx-distal")]
    RightHandMiddlePhalanxDistal = 57,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-hand-middle-tip">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#right-hand-middle-tip")]
    RightHandMiddleTip = 58,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-hand-ring-metacarpal">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#right-hand-ring-metacarpal")]
    RightHandRingMetacarpal = 59,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-hand-ring-phalanx-proximal">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#right-hand-ring-phalanx-proximal")]
    RightHandRingPhalanxProximal = 60,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-hand-ring-phalanx-intermediate">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#right-hand-ring-phalanx-intermediate")]
    RightHandRingPhalanxIntermediate = 61,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-hand-ring-phalanx-distal">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#right-hand-ring-phalanx-distal")]
    RightHandRingPhalanxDistal = 62,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-hand-ring-tip">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#right-hand-ring-tip")]
    RightHandRingTip = 63,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-hand-little-metacarpal">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#right-hand-little-metacarpal")]
    RightHandLittleMetacarpal = 64,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-hand-little-phalanx-proximal">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#right-hand-little-phalanx-proximal")]
    RightHandLittlePhalanxProximal = 65,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-hand-little-phalanx-intermediate">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#right-hand-little-phalanx-intermediate")]
    RightHandLittlePhalanxIntermediate = 66,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-hand-little-phalanx-distal">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#right-hand-little-phalanx-distal")]
    RightHandLittlePhalanxDistal = 67,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-hand-little-tip">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#right-hand-little-tip")]
    RightHandLittleTip = 68,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-upper-leg">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#left-upper-leg")]
    LeftUpperLeg = 69,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-lower-leg">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#left-lower-leg")]
    LeftLowerLeg = 70,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-foot-ankle-twist">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#left-foot-ankle-twist")]
    LeftFootAnkleTwist = 71,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-foot-ankle">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#left-foot-ankle")]
    LeftFootAnkle = 72,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-foot-subtalar">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#left-foot-subtalar")]
    LeftFootSubtalar = 73,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-foot-transverse">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#left-foot-transverse")]
    LeftFootTransverse = 74,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-foot-ball">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#left-foot-ball")]
    LeftFootBall = 75,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-upper-leg">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#right-upper-leg")]
    RightUpperLeg = 76,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-lower-leg">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#right-lower-leg")]
    RightLowerLeg = 77,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-foot-ankle-twist">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#right-foot-ankle-twist")]
    RightFootAnkleTwist = 78,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-foot-ankle">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#right-foot-ankle")]
    RightFootAnkle = 79,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-foot-subtalar">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#right-foot-subtalar")]
    RightFootSubtalar = 80,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-foot-transverse">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#right-foot-transverse")]
    RightFootTransverse = 81,

    /// <summary>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-foot-ball">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </summary>
    [Description("@#right-foot-ball")]
    RightFootBall = 82
}

/// <summary>
/// <see href="https://immersive-web.github.io/depth-sensing/#enumdef-xrdepthdataformat">WebXR Depth Sensing Module: 2.2 Intended depth type, data usage, and data formats</see>
/// </summary>
[Description("@#XRDepthDataFormat")]
[ECMAScript]
[String]
public enum XRDepthDataFormat
{
    /// <summary>
    /// <see href="https://immersive-web.github.io/depth-sensing/#dom-xrdepthdataformat-luminance-alpha">WebXR Depth Sensing Module: 2.2 Intended depth type, data usage, and data formats</see>
    /// </summary>
    [Description("@#luminance-alpha")]
    LuminanceAlpha = 0,

    /// <summary>
    /// <see href="https://immersive-web.github.io/depth-sensing/#dom-xrdepthdataformat-float32">WebXR Depth Sensing Module: 2.2 Intended depth type, data usage, and data formats</see>
    /// </summary>
    [Description("@#float32")]
    Float32 = 1,

    /// <summary>
    /// <see href="https://immersive-web.github.io/depth-sensing/#dom-xrdepthdataformat-unsigned-short">WebXR Depth Sensing Module: 2.2 Intended depth type, data usage, and data formats</see>
    /// </summary>
    [Description("@#unsigned-short")]
    UnsignedShort = 2
}

/// <summary>
/// <see href="https://immersive-web.github.io/depth-sensing/#enumdef-xrdepthtype">WebXR Depth Sensing Module: 2.2 Intended depth type, data usage, and data formats</see>
/// </summary>
[Description("@#XRDepthType")]
[ECMAScript]
[String]
public enum XRDepthType
{
    /// <summary>
    /// <see href="https://immersive-web.github.io/depth-sensing/#dom-xrdepthtype-raw">WebXR Depth Sensing Module: 2.2 Intended depth type, data usage, and data formats</see>
    /// </summary>
    [Description("@#raw")]
    Raw = 0,

    /// <summary>
    /// <see href="https://immersive-web.github.io/depth-sensing/#dom-xrdepthtype-smooth">WebXR Depth Sensing Module: 2.2 Intended depth type, data usage, and data formats</see>
    /// </summary>
    [Description("@#smooth")]
    Smooth = 1
}

/// <summary>
/// <see href="https://immersive-web.github.io/depth-sensing/#enumdef-xrdepthusage">WebXR Depth Sensing Module: 2.2 Intended depth type, data usage, and data formats</see>
/// </summary>
[Description("@#XRDepthUsage")]
[ECMAScript]
[String]
public enum XRDepthUsage
{
    /// <summary>
    /// <see href="https://immersive-web.github.io/depth-sensing/#dom-xrdepthusage-cpu-optimized">WebXR Depth Sensing Module: 2.2 Intended depth type, data usage, and data formats</see>
    /// </summary>
    [Description("@#cpu-optimized")]
    CpuOptimized = 0,

    /// <summary>
    /// <see href="https://immersive-web.github.io/depth-sensing/#dom-xrdepthusage-gpu-optimized">WebXR Depth Sensing Module: 2.2 Intended depth type, data usage, and data formats</see>
    /// </summary>
    [Description("@#gpu-optimized")]
    GpuOptimized = 1
}

/// <summary>
/// <see href="https://immersive-web.github.io/dom-overlays/#enumdef-xrdomoverlaytype">WebXR DOM Overlays Module: WebXR DOM Overlays Module</see>
/// </summary>
[Description("@#XRDOMOverlayType")]
[ECMAScript]
[String]
public enum XRDOMOverlayType
{
    /// <summary>
    /// <see href="https://immersive-web.github.io/dom-overlays/#dom-xrdomoverlaytype-screen">WebXR DOM Overlays Module: WebXR DOM Overlays Module</see>
    /// </summary>
    [Description("@#screen")]
    Screen = 0,

    /// <summary>
    /// <see href="https://immersive-web.github.io/dom-overlays/#dom-xrdomoverlaytype-floating">WebXR DOM Overlays Module: WebXR DOM Overlays Module</see>
    /// </summary>
    [Description("@#floating")]
    Floating = 1,

    /// <summary>
    /// <see href="https://immersive-web.github.io/dom-overlays/#dom-xrdomoverlaytype-head-locked">WebXR DOM Overlays Module: WebXR DOM Overlays Module</see>
    /// </summary>
    [Description("@#head-locked")]
    HeadLocked = 2
}

/// <summary>
/// <see href="https://immersive-web.github.io/hit-test/#enumdef-xrhittesttrackabletype">WebXR Hit Test Module: WebXR Hit Test Module</see>
/// </summary>
[Description("@#XRHitTestTrackableType")]
[ECMAScript]
[String]
public enum XRHitTestTrackableType
{
    /// <summary>
    /// If nativeEntityType contains type that corresponds to &quot;point&quot;
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/hit-test/#dom-xrhittesttrackabletype-point">WebXR Hit Test Module: WebXR Hit Test Module</see>
    /// </remarks>
    [Description("@#point")]
    Point = 0,

    /// <summary>
    /// Else, if nativeEntityType contains type that corresponds to &quot;plane&quot;
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/hit-test/#dom-xrhittesttrackabletype-plane">WebXR Hit Test Module: WebXR Hit Test Module</see>
    /// </remarks>
    [Description("@#plane")]
    Plane = 1,

    /// <summary>
    /// Else, if nativeEntityType contains type that corresponds to &quot;mesh&quot;
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/hit-test/#dom-xrhittesttrackabletype-mesh">WebXR Hit Test Module: WebXR Hit Test Module</see>
    /// </remarks>
    [Description("@#mesh")]
    Mesh = 2
}

/// <summary>
/// <see href="https://immersive-web.github.io/layers/#enumdef-xrlayerlayout">WebXR Layers API Level 1: 3.2 XRLayerLayout</see>
/// </summary>
[Description("@#XRLayerLayout")]
[ECMAScript]
[String]
public enum XRLayerLayout
{
    /// <summary>
    /// Initialize layer&apos;s ignoreDepthValues to true 1. Initialize layer&apos;s fixedFoveation to 0. 1. let layout be the result of determine the layout attributedetermining the layout attribute with init&apos;s textureType, context| and &quot;default&quot;. 1. Let maximum scalefactor be the result of determine the maximum scalefactordetermining the maximum scalefactor with session, context and layout|. 1. If scaleFactor is larger than maximum scalefactor, set scaleFactor to maximum scalefactor. 1. Initialize layer&apos;s layout to layout. 1. Initialize layer&apos;s needsRedraw to true. 1....
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/layers/#dom-xrlayerlayout-default">WebXR Layers API Level 1: 3.2 XRLayerLayout</see>
    /// </remarks>
    [Description("@#default")]
    Default = 0,

    /// <summary>
    /// <see href="https://immersive-web.github.io/layers/#dom-xrlayerlayout-mono">WebXR Layers API Level 1: 3.2 XRLayerLayout</see>
    /// </summary>
    [Description("@#mono")]
    Mono = 1,

    /// <summary>
    /// Return array and abort these steps. 1. If the session&apos;s viewviews in the list of views don&apos;t all have the same recommended WebGL color texture resolution excluding the secondary viewsecondary views, throw a NotSupportedError and abort these steps. 1. If layer&apos;s layout is stereo-left-right, initialize array with 1 new instance of opaque texture in the relevant realm of context created as a textureType texture using context , textureFormat, numViews multiplied by width and height. 1....
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/layers/#dom-xrlayerlayout-stereo">WebXR Layers API Level 1: 3.2 XRLayerLayout</see>
    /// </remarks>
    [Description("@#stereo")]
    Stereo = 2,

    /// <summary>
    /// Return array and abort these steps. 1. If the session&apos;s viewviews in the list of views don&apos;t all have the same recommended WebGL color texture resolution excluding the secondary viewsecondary views, throw a NotSupportedError and abort these steps. 1. If layer&apos;s layout is stereo-left-right, initialize array with 1 new instance of opaque texture in the relevant realm of context created as a textureType texture using context , textureFormat, numViews multiplied by width and height. 1....
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/layers/#dom-xrlayerlayout-stereo-left-right">WebXR Layers API Level 1: 3.2 XRLayerLayout</see>
    /// </remarks>
    [Description("@#stereo-left-right")]
    StereoLeftRight = 3,

    /// <summary>
    /// Return array and abort these steps. 1. If layer&apos;s layout is stereo-left-right, initialize array with 1 new instance of an opaque texture in the relevant realm of context created as a textureType texture using context and init&apos;s colorFormat, mipLevels, double of viewPixelWidth and viewPixelHeight values. 1. If layer&apos;s layout is stereo-top-bottom, initialize array with 1 new instance of an opaque texture in the relevant realm of context created as a textureType texture using context and init&apos;s colorFormat, mipLevels, viewPixelWidth and double of viewPixelHeight values. 1. return array.
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/layers/#dom-xrlayerlayout-stereo-top-bottom">WebXR Layers API Level 1: 3.2 XRLayerLayout</see>
    /// </remarks>
    [Description("@#stereo-top-bottom")]
    StereoTopBottom = 4
}

/// <summary>
/// <see href="https://immersive-web.github.io/layers/#enumdef-xrlayerquality">WebXR Layers API Level 1: 3.3 XRLayerQuality</see>
/// </summary>
[Description("@#XRLayerQuality")]
[ECMAScript]
[String]
public enum XRLayerQuality
{
    /// <summary>
    /// <see href="https://immersive-web.github.io/layers/#dom-xrlayerquality-default">WebXR Layers API Level 1: 3.3 XRLayerQuality</see>
    /// </summary>
    [Description("@#default")]
    Default = 0,

    /// <summary>
    /// <see href="https://immersive-web.github.io/layers/#dom-xrlayerquality-text-optimized">WebXR Layers API Level 1: 3.3 XRLayerQuality</see>
    /// </summary>
    [Description("@#text-optimized")]
    TextOptimized = 1,

    /// <summary>
    /// <see href="https://immersive-web.github.io/layers/#dom-xrlayerquality-graphics-optimized">WebXR Layers API Level 1: 3.3 XRLayerQuality</see>
    /// </summary>
    [Description("@#graphics-optimized")]
    GraphicsOptimized = 2
}

/// <summary>
/// <see href="https://immersive-web.github.io/layers/#enumdef-xrtexturetype">WebXR Layers API Level 1: 5.3 XRTextureType</see>
/// </summary>
[Description("@#XRTextureType")]
[ECMAScript]
[String]
public enum XRTextureType
{
    /// <summary>
    /// <see href="https://immersive-web.github.io/layers/#dom-xrtexturetype-texture">WebXR Layers API Level 1: 5.3 XRTextureType</see>
    /// </summary>
    [Description("@#texture")]
    Texture = 0,

    /// <summary>
    /// <see href="https://immersive-web.github.io/layers/#dom-xrtexturetype-texture-array">WebXR Layers API Level 1: 5.3 XRTextureType</see>
    /// </summary>
    [Description("@#texture-array")]
    TextureArray = 1
}

/// <summary>
/// <see href="https://immersive-web.github.io/lighting-estimation/#enumdef-xrreflectionformat">WebXR Lighting Estimation API Level 1: 2.2 XRReflectionFormat</see>
/// </summary>
[Description("@#XRReflectionFormat")]
[ECMAScript]
[String]
public enum XRReflectionFormat
{
    /// <summary>
    /// <see href="https://immersive-web.github.io/lighting-estimation/#dom-xrreflectionformat-srgba8">WebXR Lighting Estimation API Level 1: 2.2 XRReflectionFormat</see>
    /// </summary>
    [Description("@#srgba8")]
    Srgba8 = 0,

    /// <summary>
    /// <see href="https://immersive-web.github.io/lighting-estimation/#dom-xrreflectionformat-rgba16f">WebXR Lighting Estimation API Level 1: 2.2 XRReflectionFormat</see>
    /// </summary>
    [Description("@#rgba16f")]
    Rgba16f = 1
}

/// <summary>
/// <see href="https://immersive-web.github.io/plane-detection/#enumdef-xrplaneorientation">WebXR Plane Detection Module: 3.1 XRPlaneOrientation</see>
/// </summary>
[Description("@#XRPlaneOrientation")]
[ECMAScript]
[String]
public enum XRPlaneOrientation
{
    /// <summary>
    /// <see href="https://immersive-web.github.io/plane-detection/#dom-xrplaneorientation-horizontal">WebXR Plane Detection Module: 3.1 XRPlaneOrientation</see>
    /// </summary>
    [Description("@#horizontal")]
    Horizontal = 0,

    /// <summary>
    /// <see href="https://immersive-web.github.io/plane-detection/#dom-xrplaneorientation-vertical">WebXR Plane Detection Module: 3.1 XRPlaneOrientation</see>
    /// </summary>
    [Description("@#vertical")]
    Vertical = 1
}

/// <summary>
/// <see href="https://immersive-web.github.io/webxr-ar-module/#enumdef-xrenvironmentblendmode">WebXR Augmented Reality Module - Level 1: 2.2 XREnvironmentBlendMode</see>
/// </summary>
[Description("@#XREnvironmentBlendMode")]
[ECMAScript]
[String]
public enum XREnvironmentBlendMode
{
    /// <summary>
    /// <see href="https://immersive-web.github.io/webxr-ar-module/#dom-xrenvironmentblendmode-opaque">WebXR Augmented Reality Module - Level 1: 2.2 XREnvironmentBlendMode</see>
    /// </summary>
    [Description("@#opaque")]
    Opaque = 0,

    /// <summary>
    /// <see href="https://immersive-web.github.io/webxr-ar-module/#dom-xrenvironmentblendmode-alpha-blend">WebXR Augmented Reality Module - Level 1: 2.2 XREnvironmentBlendMode</see>
    /// </summary>
    [Description("@#alpha-blend")]
    AlphaBlend = 1,

    /// <summary>
    /// <see href="https://immersive-web.github.io/webxr-ar-module/#dom-xrenvironmentblendmode-additive">WebXR Augmented Reality Module - Level 1: 2.2 XREnvironmentBlendMode</see>
    /// </summary>
    [Description("@#additive")]
    Additive = 2
}

/// <summary>
/// <see href="https://immersive-web.github.io/webxr-ar-module/#enumdef-xrinteractionmode">WebXR Augmented Reality Module - Level 1: 2.3 XRInteractionMode</see>
/// </summary>
[Description("@#XRInteractionMode")]
[ECMAScript]
[String]
public enum XRInteractionMode
{
    /// <summary>
    /// <see href="https://immersive-web.github.io/webxr-ar-module/#dom-xrinteractionmode-screen-space">WebXR Augmented Reality Module - Level 1: 2.3 XRInteractionMode</see>
    /// </summary>
    [Description("@#screen-space")]
    ScreenSpace = 0,

    /// <summary>
    /// <see href="https://immersive-web.github.io/webxr-ar-module/#dom-xrinteractionmode-world-space">WebXR Augmented Reality Module - Level 1: 2.3 XRInteractionMode</see>
    /// </summary>
    [Description("@#world-space")]
    WorldSpace = 1
}

/// <summary>
/// <see href="https://immersive-web.github.io/webxr-hand-input/#enumdef-xrhandjoint">WebXR Hand Input Module - Level 1: 3.3 XRHand</see>
/// </summary>
[Description("@#XRHandJoint")]
[ECMAScript]
[String]
public enum XRHandJoint
{
    /// <summary>
    /// <see href="https://immersive-web.github.io/webxr-hand-input/#dom-xrhandjoint-wrist">WebXR Hand Input Module - Level 1: WebXR Hand Input Module - Level 1</see>
    /// </summary>
    [Description("@#wrist")]
    Wrist = 0,

    /// <summary>
    /// <see href="https://immersive-web.github.io/webxr-hand-input/#dom-xrhandjoint-thumb-metacarpal">WebXR Hand Input Module - Level 1: WebXR Hand Input Module - Level 1</see>
    /// </summary>
    [Description("@#thumb-metacarpal")]
    ThumbMetacarpal = 1,

    /// <summary>
    /// <see href="https://immersive-web.github.io/webxr-hand-input/#dom-xrhandjoint-thumb-phalanx-proximal">WebXR Hand Input Module - Level 1: WebXR Hand Input Module - Level 1</see>
    /// </summary>
    [Description("@#thumb-phalanx-proximal")]
    ThumbPhalanxProximal = 2,

    /// <summary>
    /// <see href="https://immersive-web.github.io/webxr-hand-input/#dom-xrhandjoint-thumb-phalanx-distal">WebXR Hand Input Module - Level 1: WebXR Hand Input Module - Level 1</see>
    /// </summary>
    [Description("@#thumb-phalanx-distal")]
    ThumbPhalanxDistal = 3,

    /// <summary>
    /// <see href="https://immersive-web.github.io/webxr-hand-input/#dom-xrhandjoint-thumb-tip">WebXR Hand Input Module - Level 1: WebXR Hand Input Module - Level 1</see>
    /// </summary>
    [Description("@#thumb-tip")]
    ThumbTip = 4,

    /// <summary>
    /// <see href="https://immersive-web.github.io/webxr-hand-input/#dom-xrhandjoint-index-finger-metacarpal">WebXR Hand Input Module - Level 1: WebXR Hand Input Module - Level 1</see>
    /// </summary>
    [Description("@#index-finger-metacarpal")]
    IndexFingerMetacarpal = 5,

    /// <summary>
    /// <see href="https://immersive-web.github.io/webxr-hand-input/#dom-xrhandjoint-index-finger-phalanx-proximal">WebXR Hand Input Module - Level 1: WebXR Hand Input Module - Level 1</see>
    /// </summary>
    [Description("@#index-finger-phalanx-proximal")]
    IndexFingerPhalanxProximal = 6,

    /// <summary>
    /// <see href="https://immersive-web.github.io/webxr-hand-input/#dom-xrhandjoint-index-finger-phalanx-intermediate">WebXR Hand Input Module - Level 1: WebXR Hand Input Module - Level 1</see>
    /// </summary>
    [Description("@#index-finger-phalanx-intermediate")]
    IndexFingerPhalanxIntermediate = 7,

    /// <summary>
    /// <see href="https://immersive-web.github.io/webxr-hand-input/#dom-xrhandjoint-index-finger-phalanx-distal">WebXR Hand Input Module - Level 1: WebXR Hand Input Module - Level 1</see>
    /// </summary>
    [Description("@#index-finger-phalanx-distal")]
    IndexFingerPhalanxDistal = 8,

    /// <summary>
    /// <see href="https://immersive-web.github.io/webxr-hand-input/#dom-xrhandjoint-index-finger-tip">WebXR Hand Input Module - Level 1: WebXR Hand Input Module - Level 1</see>
    /// </summary>
    [Description("@#index-finger-tip")]
    IndexFingerTip = 9,

    /// <summary>
    /// <see href="https://immersive-web.github.io/webxr-hand-input/#dom-xrhandjoint-middle-finger-metacarpal">WebXR Hand Input Module - Level 1: WebXR Hand Input Module - Level 1</see>
    /// </summary>
    [Description("@#middle-finger-metacarpal")]
    MiddleFingerMetacarpal = 10,

    /// <summary>
    /// <see href="https://immersive-web.github.io/webxr-hand-input/#dom-xrhandjoint-middle-finger-phalanx-proximal">WebXR Hand Input Module - Level 1: WebXR Hand Input Module - Level 1</see>
    /// </summary>
    [Description("@#middle-finger-phalanx-proximal")]
    MiddleFingerPhalanxProximal = 11,

    /// <summary>
    /// <see href="https://immersive-web.github.io/webxr-hand-input/#dom-xrhandjoint-middle-finger-phalanx-intermediate">WebXR Hand Input Module - Level 1: WebXR Hand Input Module - Level 1</see>
    /// </summary>
    [Description("@#middle-finger-phalanx-intermediate")]
    MiddleFingerPhalanxIntermediate = 12,

    /// <summary>
    /// <see href="https://immersive-web.github.io/webxr-hand-input/#dom-xrhandjoint-middle-finger-phalanx-distal">WebXR Hand Input Module - Level 1: WebXR Hand Input Module - Level 1</see>
    /// </summary>
    [Description("@#middle-finger-phalanx-distal")]
    MiddleFingerPhalanxDistal = 13,

    /// <summary>
    /// <see href="https://immersive-web.github.io/webxr-hand-input/#dom-xrhandjoint-middle-finger-tip">WebXR Hand Input Module - Level 1: WebXR Hand Input Module - Level 1</see>
    /// </summary>
    [Description("@#middle-finger-tip")]
    MiddleFingerTip = 14,

    /// <summary>
    /// <see href="https://immersive-web.github.io/webxr-hand-input/#dom-xrhandjoint-ring-finger-metacarpal">WebXR Hand Input Module - Level 1: WebXR Hand Input Module - Level 1</see>
    /// </summary>
    [Description("@#ring-finger-metacarpal")]
    RingFingerMetacarpal = 15,

    /// <summary>
    /// <see href="https://immersive-web.github.io/webxr-hand-input/#dom-xrhandjoint-ring-finger-phalanx-proximal">WebXR Hand Input Module - Level 1: WebXR Hand Input Module - Level 1</see>
    /// </summary>
    [Description("@#ring-finger-phalanx-proximal")]
    RingFingerPhalanxProximal = 16,

    /// <summary>
    /// <see href="https://immersive-web.github.io/webxr-hand-input/#dom-xrhandjoint-ring-finger-phalanx-intermediate">WebXR Hand Input Module - Level 1: WebXR Hand Input Module - Level 1</see>
    /// </summary>
    [Description("@#ring-finger-phalanx-intermediate")]
    RingFingerPhalanxIntermediate = 17,

    /// <summary>
    /// <see href="https://immersive-web.github.io/webxr-hand-input/#dom-xrhandjoint-ring-finger-phalanx-distal">WebXR Hand Input Module - Level 1: WebXR Hand Input Module - Level 1</see>
    /// </summary>
    [Description("@#ring-finger-phalanx-distal")]
    RingFingerPhalanxDistal = 18,

    /// <summary>
    /// <see href="https://immersive-web.github.io/webxr-hand-input/#dom-xrhandjoint-ring-finger-tip">WebXR Hand Input Module - Level 1: WebXR Hand Input Module - Level 1</see>
    /// </summary>
    [Description("@#ring-finger-tip")]
    RingFingerTip = 19,

    /// <summary>
    /// <see href="https://immersive-web.github.io/webxr-hand-input/#dom-xrhandjoint-pinky-finger-metacarpal">WebXR Hand Input Module - Level 1: WebXR Hand Input Module - Level 1</see>
    /// </summary>
    [Description("@#pinky-finger-metacarpal")]
    PinkyFingerMetacarpal = 20,

    /// <summary>
    /// <see href="https://immersive-web.github.io/webxr-hand-input/#dom-xrhandjoint-pinky-finger-phalanx-proximal">WebXR Hand Input Module - Level 1: WebXR Hand Input Module - Level 1</see>
    /// </summary>
    [Description("@#pinky-finger-phalanx-proximal")]
    PinkyFingerPhalanxProximal = 21,

    /// <summary>
    /// <see href="https://immersive-web.github.io/webxr-hand-input/#dom-xrhandjoint-pinky-finger-phalanx-intermediate">WebXR Hand Input Module - Level 1: WebXR Hand Input Module - Level 1</see>
    /// </summary>
    [Description("@#pinky-finger-phalanx-intermediate")]
    PinkyFingerPhalanxIntermediate = 22,

    /// <summary>
    /// <see href="https://immersive-web.github.io/webxr-hand-input/#dom-xrhandjoint-pinky-finger-phalanx-distal">WebXR Hand Input Module - Level 1: WebXR Hand Input Module - Level 1</see>
    /// </summary>
    [Description("@#pinky-finger-phalanx-distal")]
    PinkyFingerPhalanxDistal = 23,

    /// <summary>
    /// <see href="https://immersive-web.github.io/webxr-hand-input/#dom-xrhandjoint-pinky-finger-tip">WebXR Hand Input Module - Level 1: WebXR Hand Input Module - Level 1</see>
    /// </summary>
    [Description("@#pinky-finger-tip")]
    PinkyFingerTip = 24
}

/// <summary>
/// <see href="https://immersive-web.github.io/webxr/#enumdef-xreye">WebXR Device API: 7.2 XRView</see>
/// </summary>
[Description("@#XREye")]
[ECMAScript]
[String]
public enum XREye
{
    /// <summary>
    /// <see href="https://immersive-web.github.io/webxr/#dom-xreye-none">WebXR Device API: 7.2 XRView</see>
    /// </summary>
    [Description("@#none")]
    None = 0,

    /// <summary>
    /// <see href="https://immersive-web.github.io/webxr/#dom-xreye-left">WebXR Device API: 7.2 XRView</see>
    /// </summary>
    [Description("@#left")]
    Left = 1,

    /// <summary>
    /// <see href="https://immersive-web.github.io/webxr/#dom-xreye-right">WebXR Device API: 7.2 XRView</see>
    /// </summary>
    [Description("@#right")]
    Right = 2
}

/// <summary>
/// <see href="https://immersive-web.github.io/webxr/#enumdef-xrhandedness">WebXR Device API: 10.1 XRInputSource</see>
/// </summary>
[Description("@#XRHandedness")]
[ECMAScript]
[String]
public enum XRHandedness
{
    /// <summary>
    /// <see href="https://immersive-web.github.io/webxr/#dom-xrhandedness-none">WebXR Device API: 10.1 XRInputSource</see>
    /// </summary>
    [Description("@#none")]
    None = 0,

    /// <summary>
    /// <see href="https://immersive-web.github.io/webxr/#dom-xrhandedness-left">WebXR Device API: 10.1 XRInputSource</see>
    /// </summary>
    [Description("@#left")]
    Left = 1,

    /// <summary>
    /// <see href="https://immersive-web.github.io/webxr/#dom-xrhandedness-right">WebXR Device API: 10.1 XRInputSource</see>
    /// </summary>
    [Description("@#right")]
    Right = 2
}

/// <summary>
/// <see href="https://immersive-web.github.io/webxr/#enumdef-xrreferencespacetype">WebXR Device API: 6.2 XRReferenceSpace</see>
/// </summary>
[Description("@#XRReferenceSpaceType")]
[ECMAScript]
[String]
public enum XRReferenceSpaceType
{
    /// <summary>
    /// <see href="https://immersive-web.github.io/webxr/#dom-xrreferencespacetype-viewer">WebXR Device API: 6.2 XRReferenceSpace</see>
    /// </summary>
    [Description("@#viewer")]
    Viewer = 0,

    /// <summary>
    /// <see href="https://immersive-web.github.io/webxr/#dom-xrreferencespacetype-local">WebXR Device API: 6.2 XRReferenceSpace</see>
    /// </summary>
    [Description("@#local")]
    Local = 1,

    /// <summary>
    /// <see href="https://immersive-web.github.io/webxr/#dom-xrreferencespacetype-local-floor">WebXR Device API: 6.2 XRReferenceSpace</see>
    /// </summary>
    [Description("@#local-floor")]
    LocalFloor = 2,

    /// <summary>
    /// <see href="https://immersive-web.github.io/webxr/#dom-xrreferencespacetype-bounded-floor">WebXR Device API: 6.2 XRReferenceSpace</see>
    /// </summary>
    [Description("@#bounded-floor")]
    BoundedFloor = 3,

    /// <summary>
    /// <see href="https://immersive-web.github.io/webxr/#dom-xrreferencespacetype-unbounded">WebXR Device API: 6.2 XRReferenceSpace</see>
    /// </summary>
    [Description("@#unbounded")]
    Unbounded = 4
}

/// <summary>
/// <see href="https://immersive-web.github.io/webxr/#enumdef-xrsessionmode">WebXR Device API: 3.3 XRSessionMode</see>
/// </summary>
[Description("@#XRSessionMode")]
[ECMAScript]
[String]
public enum XRSessionMode
{
    /// <summary>
    /// <see href="https://immersive-web.github.io/webxr/#dom-xrsessionmode-inline">WebXR Device API: 3.3 XRSessionMode</see>
    /// </summary>
    [Description("@#inline")]
    Inline = 0,

    /// <summary>
    /// <see href="https://immersive-web.github.io/webxr/#dom-xrsessionmode-immersive-vr">WebXR Device API: 3.3 XRSessionMode</see>
    /// </summary>
    [Description("@#immersive-vr")]
    ImmersiveVr = 1,

    /// <summary>
    /// <see href="https://immersive-web.github.io/webxr/#dom-xrsessionmode-immersive-ar">WebXR Device API: 3.3 XRSessionMode</see>
    /// </summary>
    [Description("@#immersive-ar")]
    ImmersiveAr = 2
}

/// <summary>
/// <see href="https://immersive-web.github.io/webxr/#enumdef-xrtargetraymode">WebXR Device API: 10.1 XRInputSource</see>
/// </summary>
[Description("@#XRTargetRayMode")]
[ECMAScript]
[String]
public enum XRTargetRayMode
{
    /// <summary>
    /// <see href="https://immersive-web.github.io/webxr/#dom-xrtargetraymode-gaze">WebXR Device API: 10.1 XRInputSource</see>
    /// </summary>
    [Description("@#gaze")]
    Gaze = 0,

    /// <summary>
    /// <see href="https://immersive-web.github.io/webxr/#dom-xrtargetraymode-tracked-pointer">WebXR Device API: 10.1 XRInputSource</see>
    /// </summary>
    [Description("@#tracked-pointer")]
    TrackedPointer = 1,

    /// <summary>
    /// <see href="https://immersive-web.github.io/webxr/#dom-xrtargetraymode-screen">WebXR Device API: 10.1 XRInputSource</see>
    /// </summary>
    [Description("@#screen")]
    Screen = 2,

    /// <summary>
    /// <see href="https://immersive-web.github.io/webxr/#dom-xrtargetraymode-transient-pointer">WebXR Device API: 10.1 XRInputSource</see>
    /// </summary>
    [Description("@#transient-pointer")]
    TransientPointer = 3
}

/// <summary>
/// <see href="https://immersive-web.github.io/webxr/#enumdef-xrvisibilitystate">WebXR Device API: 4.1 XRSession</see>
/// </summary>
[Description("@#XRVisibilityState")]
[ECMAScript]
[String]
public enum XRVisibilityState
{
    /// <summary>
    /// <see href="https://immersive-web.github.io/webxr/#dom-xrvisibilitystate-visible">WebXR Device API: 4.1 XRSession</see>
    /// </summary>
    [Description("@#visible")]
    Visible = 0,

    /// <summary>
    /// <see href="https://immersive-web.github.io/webxr/#dom-xrvisibilitystate-visible-blurred">WebXR Device API: 4.1 XRSession</see>
    /// </summary>
    [Description("@#visible-blurred")]
    VisibleBlurred = 1,

    /// <summary>
    /// <see href="https://immersive-web.github.io/webxr/#dom-xrvisibilitystate-hidden">WebXR Device API: 4.1 XRSession</see>
    /// </summary>
    [Description("@#hidden")]
    Hidden = 2
}

/// <summary>
/// <see href="https://notifications.spec.whatwg.org/#enumdef-notificationdirection">Notifications API Standard: 3 API</see>
/// </summary>
[Description("@#NotificationDirection")]
[ECMAScript]
[String]
public enum NotificationDirection
{
    /// <summary>
    /// User agents are expected to honor the Unicode semantics of the text of a notification&apos;s title, body, and the title of each of its actions. Each is expected to be treated as an independent set of one or more bidirectional algorithm paragraphs when displayed, as defined by the bidirectional algorithm&apos;s rules P1, P2, and P3, including, for instance, supporting the paragraph-breaking behavior of U+000A LINE FEED (LF) characters. For each paragraph of the title, body and the title of each of the actions, the notification&apos;s direction provides the higher-level override of rules P2 and P3 if it has a value other than &quot;auto&quot;. !BIDI
    /// </summary>
    /// <remarks>
    /// <see href="https://notifications.spec.whatwg.org/#dom-notificationdirection-auto">Notifications API Standard: 3 API</see>
    /// </remarks>
    [Description("@#auto")]
    Auto = 0,

    /// <summary>
    /// <see href="https://notifications.spec.whatwg.org/#dom-notificationdirection-ltr">Notifications API Standard: 3 API</see>
    /// </summary>
    [Description("@#ltr")]
    Ltr = 1,

    /// <summary>
    /// <see href="https://notifications.spec.whatwg.org/#dom-notificationdirection-rtl">Notifications API Standard: 3 API</see>
    /// </summary>
    [Description("@#rtl")]
    Rtl = 2
}

/// <summary>
/// <see href="https://notifications.spec.whatwg.org/#enumdef-notificationpermission">Notifications API Standard: 3 API</see>
/// </summary>
[Description("@#NotificationPermission")]
[ECMAScript]
[String]
public enum NotificationPermission
{
    /// <summary>
    /// <see href="https://notifications.spec.whatwg.org/#dom-notificationpermission-default">Notifications API Standard: 3 API</see>
    /// </summary>
    [Description("@#default")]
    Default = 0,

    /// <summary>
    /// <see href="https://notifications.spec.whatwg.org/#dom-notificationpermission-denied">Notifications API Standard: 3 API</see>
    /// </summary>
    [Description("@#denied")]
    Denied = 1,

    /// <summary>
    /// <see href="https://notifications.spec.whatwg.org/#dom-notificationpermission-granted">Notifications API Standard: 3 API</see>
    /// </summary>
    [Description("@#granted")]
    Granted = 2
}

/// <summary>
/// <see href="https://privacycg.github.io/saa-non-cookie-storage/#enumdef-samesitecookiestype">Extending Storage Access API (SAA) to non-cookie storage: 2.3.9 Shared Workers</see>
/// </summary>
[Description("@#SameSiteCookiesType")]
[ECMAScript]
[String]
public enum SameSiteCookiesType
{
    /// <summary>
    /// <see href="https://privacycg.github.io/saa-non-cookie-storage/#dom-samesitecookiestype-all">Extending Storage Access API (SAA) to non-cookie storage: 2.3.9 Shared Workers</see>
    /// </summary>
    [Description("@#all")]
    All = 0,

    /// <summary>
    /// <see href="https://privacycg.github.io/saa-non-cookie-storage/#dom-samesitecookiestype-none">Extending Storage Access API (SAA) to non-cookie storage: 2.3.9 Shared Workers</see>
    /// </summary>
    [Description("@#none")]
    None = 1
}

/// <summary>
/// <see href="https://streams.spec.whatwg.org/#enumdef-readablestreamreadermode">Streams Standard: 4.2.1 Interface definition</see>
/// </summary>
[Description("@#ReadableStreamReaderMode")]
[ECMAScript]
[String]
public enum ReadableStreamReaderMode
{
    /// <summary>
    /// This is equivalent to calling stream.getReader({ mode: &quot;byob&quot; }).
    /// </summary>
    /// <remarks>
    /// <see href="https://streams.spec.whatwg.org/#dom-readablestreamreadermode-byob">Streams Standard: 4.2.1 Interface definition</see>
    /// </remarks>
    /// <example>
    /// <code>reader = stream.getReader({ mode: &quot;byob&quot; })</code>
    /// </example>
    [Description("@#byob")]
    Byob = 0
}

/// <summary>
/// <see href="https://streams.spec.whatwg.org/#enumdef-readablestreamtype">Streams Standard: 4.2.3 The underlying source API</see>
/// </summary>
[Description("@#ReadableStreamType")]
[ECMAScript]
[String]
public enum ReadableStreamType
{
    /// <summary>
    /// Can be set to &quot;bytes&quot; to signal that the constructed ReadableStream is a readable byte stream. This ensures that the resulting ReadableStream will successfully be able to vend BYOB readers via its getReader() method. It also affects the controller argument passed to the start() and pull() methods; see below.
    /// </summary>
    /// <remarks>
    /// <see href="https://streams.spec.whatwg.org/#dom-readablestreamtype-bytes">Streams Standard: 4.2.3 The underlying source API</see>
    /// </remarks>
    [Description("@#bytes")]
    Bytes = 0
}

/// <summary>
/// <see href="https://w3c-fedid.github.io/FedCM/#enumdef-identitycredentialrequestoptionscontext">Federated Credential Management API: 2.4 The CredentialRequestOptions</see>
/// </summary>
[Description("@#IdentityCredentialRequestOptionsContext")]
[ECMAScript]
[String]
public enum IdentityCredentialRequestOptionsContext
{
    /// <summary>
    /// <see href="https://w3c-fedid.github.io/FedCM/#dom-identitycredentialrequestoptionscontext-signin">Federated Credential Management API: 2.4 The CredentialRequestOptions</see>
    /// </summary>
    [Description("@#signin")]
    Signin = 0,

    /// <summary>
    /// <see href="https://w3c-fedid.github.io/FedCM/#dom-identitycredentialrequestoptionscontext-signup">Federated Credential Management API: 2.4 The CredentialRequestOptions</see>
    /// </summary>
    [Description("@#signup")]
    Signup = 1,

    /// <summary>
    /// <see href="https://w3c-fedid.github.io/FedCM/#dom-identitycredentialrequestoptionscontext-use">Federated Credential Management API: 2.4 The CredentialRequestOptions</see>
    /// </summary>
    [Description("@#use")]
    Use = 2,

    /// <summary>
    /// <see href="https://w3c-fedid.github.io/FedCM/#dom-identitycredentialrequestoptionscontext-continue">Federated Credential Management API: 2.4 The CredentialRequestOptions</see>
    /// </summary>
    [Description("@#continue")]
    Continue = 3
}

/// <summary>
/// <see href="https://w3c-fedid.github.io/FedCM/#enumdef-identitycredentialrequestoptionsmode">Federated Credential Management API: 2.4 The CredentialRequestOptions</see>
/// </summary>
[Description("@#IdentityCredentialRequestOptionsMode")]
[ECMAScript]
[String]
public enum IdentityCredentialRequestOptionsMode
{
    /// <summary>
    /// <see href="https://w3c-fedid.github.io/FedCM/#dom-identitycredentialrequestoptionsmode-active">Federated Credential Management API: 2.4 The CredentialRequestOptions</see>
    /// </summary>
    [Description("@#active")]
    Active = 0,

    /// <summary>
    /// <see href="https://w3c-fedid.github.io/FedCM/#dom-identitycredentialrequestoptionsmode-passive">Federated Credential Management API: 2.4 The CredentialRequestOptions</see>
    /// </summary>
    [Description("@#passive")]
    Passive = 1
}

/// <summary>
/// <see href="https://w3c-fedid.github.io/login-status/#enumdef-loginstatus">Login Status API: 5 JavaScript API</see>
/// </summary>
[Description("@#LoginStatus")]
[ECMAScript]
[String]
public enum LoginStatus
{
    /// <summary>
    /// <see href="https://w3c-fedid.github.io/login-status/#dom-loginstatus-logged-in">Login Status API: Login Status API</see>
    /// </summary>
    [Description("@#logged-in")]
    LoggedIn = 0,

    /// <summary>
    /// <see href="https://w3c-fedid.github.io/login-status/#dom-loginstatus-logged-out">Login Status API: Login Status API</see>
    /// </summary>
    [Description("@#logged-out")]
    LoggedOut = 1
}

/// <summary>
/// <see href="https://w3c.github.io/FileAPI/#enumdef-endingtype">File API: 3 The Blob Interface and Binary Data</see>
/// </summary>
[Description("@#EndingType")]
[ECMAScript]
[String]
public enum EndingType
{
    /// <summary>
    /// <see href="https://w3c.github.io/FileAPI/#dom-endingtype-transparent">File API: 3 The Blob Interface and Binary Data</see>
    /// </summary>
    [Description("@#transparent")]
    Transparent = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/FileAPI/#dom-endingtype-native">File API: 3 The Blob Interface and Binary Data</see>
    /// </summary>
    [Description("@#native")]
    Native = 1
}

/// <summary>
/// <see href="https://w3c.github.io/IndexedDB/#enumdef-idbcursordirection">Indexed Database API 3.0: 4.9 The IDBCursor interface</see>
/// </summary>
[Description("@#IDBCursorDirection")]
[ECMAScript]
[String]
public enum IDBCursorDirection
{
    /// <summary>
    /// <see href="https://w3c.github.io/IndexedDB/#dom-idbcursordirection-next">Indexed Database API 3.0: 4.9 The IDBCursor interface</see>
    /// </summary>
    [Description("@#next")]
    Next = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/IndexedDB/#dom-idbcursordirection-nextunique">Indexed Database API 3.0: 4.9 The IDBCursor interface</see>
    /// </summary>
    [Description("@#nextunique")]
    Nextunique = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/IndexedDB/#dom-idbcursordirection-prev">Indexed Database API 3.0: 4.9 The IDBCursor interface</see>
    /// </summary>
    [Description("@#prev")]
    Prev = 2,

    /// <summary>
    /// <see href="https://w3c.github.io/IndexedDB/#dom-idbcursordirection-prevunique">Indexed Database API 3.0: 4.9 The IDBCursor interface</see>
    /// </summary>
    [Description("@#prevunique")]
    Prevunique = 3
}

/// <summary>
/// <see href="https://w3c.github.io/IndexedDB/#enumdef-idbrequestreadystate">Indexed Database API 3.0: 4.1 The IDBRequest interface</see>
/// </summary>
[Description("@#IDBRequestReadyState")]
[ECMAScript]
[String]
public enum IDBRequestReadyState
{
    /// <summary>
    /// <see href="https://w3c.github.io/IndexedDB/#dom-idbrequestreadystate-pending">Indexed Database API 3.0: 4.1 The IDBRequest interface</see>
    /// </summary>
    [Description("@#pending")]
    Pending = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/IndexedDB/#dom-idbrequestreadystate-done">Indexed Database API 3.0: 4.1 The IDBRequest interface</see>
    /// </summary>
    [Description("@#done")]
    Done = 1
}

/// <summary>
/// <see href="https://w3c.github.io/IndexedDB/#enumdef-idbtransactiondurability">Indexed Database API 3.0: 4.4 The IDBDatabase interface</see>
/// </summary>
[Description("@#IDBTransactionDurability")]
[ECMAScript]
[String]
public enum IDBTransactionDurability
{
    /// <summary>
    /// <see href="https://w3c.github.io/IndexedDB/#dom-idbtransactiondurability-default">Indexed Database API 3.0: 4.4 The IDBDatabase interface</see>
    /// </summary>
    [Description("@#default")]
    Default = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/IndexedDB/#dom-idbtransactiondurability-strict">Indexed Database API 3.0: 4.4 The IDBDatabase interface</see>
    /// </summary>
    [Description("@#strict")]
    Strict = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/IndexedDB/#dom-idbtransactiondurability-relaxed">Indexed Database API 3.0: 4.4 The IDBDatabase interface</see>
    /// </summary>
    [Description("@#relaxed")]
    Relaxed = 2
}

/// <summary>
/// <see href="https://w3c.github.io/IndexedDB/#enumdef-idbtransactionmode">Indexed Database API 3.0: 4.10 The IDBTransaction interface</see>
/// </summary>
[Description("@#IDBTransactionMode")]
[ECMAScript]
[String]
public enum IDBTransactionMode
{
    /// <summary>
    /// <see href="https://w3c.github.io/IndexedDB/#dom-idbtransactionmode-readonly">Indexed Database API 3.0: 4.10 The IDBTransaction interface</see>
    /// </summary>
    [Description("@#readonly")]
    Readonly = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/IndexedDB/#dom-idbtransactionmode-readwrite">Indexed Database API 3.0: 4.10 The IDBTransaction interface</see>
    /// </summary>
    [Description("@#readwrite")]
    Readwrite = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/IndexedDB/#dom-idbtransactionmode-versionchange">Indexed Database API 3.0: 4.10 The IDBTransaction interface</see>
    /// </summary>
    [Description("@#versionchange")]
    Versionchange = 2
}

/// <summary>
/// <see href="https://w3c.github.io/ServiceWorker/#enumdef-clienttype">Service Workers Nightly: 4.3 Clients</see>
/// </summary>
[Description("@#ClientType")]
[ECMAScript]
[String]
public enum ClientType
{
    /// <summary>
    /// <see href="https://w3c.github.io/ServiceWorker/#dom-clienttype-window">Service Workers Nightly: 4.3 Clients</see>
    /// </summary>
    [Description("@#window")]
    Window = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/ServiceWorker/#dom-clienttype-worker">Service Workers Nightly: 4.3 Clients</see>
    /// </summary>
    [Description("@#worker")]
    Worker = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/ServiceWorker/#dom-clienttype-sharedworker">Service Workers Nightly: 4.3 Clients</see>
    /// </summary>
    [Description("@#sharedworker")]
    Sharedworker = 2,

    /// <summary>
    /// <see href="https://w3c.github.io/ServiceWorker/#dom-clienttype-all">Service Workers Nightly: 4.3 Clients</see>
    /// </summary>
    [Description("@#all")]
    All = 3
}

/// <summary>
/// <see href="https://w3c.github.io/ServiceWorker/#enumdef-frametype">Service Workers Nightly: 4.2 Client</see>
/// </summary>
[Description("@#FrameType")]
[ECMAScript]
[String]
public enum FrameType
{
    /// <summary>
    /// <see href="https://w3c.github.io/ServiceWorker/#dom-frametype-auxiliary">Service Workers Nightly: 4.2 Client</see>
    /// </summary>
    [Description("@#auxiliary")]
    Auxiliary = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/ServiceWorker/#dom-frametype-top-level">Service Workers Nightly: 4.2 Client</see>
    /// </summary>
    [Description("@#top-level")]
    TopLevel = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/ServiceWorker/#dom-frametype-nested">Service Workers Nightly: 4.2 Client</see>
    /// </summary>
    [Description("@#nested")]
    Nested = 2,

    /// <summary>
    /// <see href="https://w3c.github.io/ServiceWorker/#dom-frametype-none">Service Workers Nightly: 4.2 Client</see>
    /// </summary>
    [Description("@#none")]
    None = 3
}

/// <summary>
/// <see href="https://w3c.github.io/ServiceWorker/#enumdef-routersourceenum">Service Workers Nightly: 4.5 InstallEvent</see>
/// </summary>
[Description("@#RouterSourceEnum")]
[ECMAScript]
[String]
public enum RouterSourceEnum
{
    /// <summary>
    /// <see href="https://w3c.github.io/ServiceWorker/#dom-routersourceenum-cache">Service Workers Nightly: 4.5 InstallEvent</see>
    /// </summary>
    [Description("@#cache")]
    Cache = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/ServiceWorker/#dom-routersourceenum-fetch-event">Service Workers Nightly: 4.5 InstallEvent</see>
    /// </summary>
    [Description("@#fetch-event")]
    FetchEvent = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/ServiceWorker/#dom-routersourceenum-network">Service Workers Nightly: 4.5 InstallEvent</see>
    /// </summary>
    [Description("@#network")]
    Network = 2,

    /// <summary>
    /// <see href="https://w3c.github.io/ServiceWorker/#dom-routersourceenum-race-network-and-fetch-handler">Service Workers Nightly: 4.5 InstallEvent</see>
    /// </summary>
    [Description("@#race-network-and-fetch-handler")]
    RaceNetworkAndFetchHandler = 3
}

/// <summary>
/// <see href="https://w3c.github.io/ServiceWorker/#enumdef-runningstatus">Service Workers Nightly: 4.5 InstallEvent</see>
/// </summary>
[Description("@#RunningStatus")]
[ECMAScript]
[String]
public enum RunningStatus
{
    /// <summary>
    /// <see href="https://w3c.github.io/ServiceWorker/#dom-runningstatus-running">Service Workers Nightly: 4.5 InstallEvent</see>
    /// </summary>
    [Description("@#running")]
    Running = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/ServiceWorker/#dom-runningstatus-not-running">Service Workers Nightly: 4.5 InstallEvent</see>
    /// </summary>
    [Description("@#not-running")]
    NotRunning = 1
}

/// <summary>
/// <see href="https://w3c.github.io/ServiceWorker/#enumdef-serviceworkerstate">Service Workers Nightly: 3.1 ServiceWorker</see>
/// </summary>
[Description("@#ServiceWorkerState")]
[ECMAScript]
[String]
public enum ServiceWorkerState
{
    /// <summary>
    /// <see href="https://w3c.github.io/ServiceWorker/#dom-serviceworkerstate-parsed">Service Workers Nightly: 3.1 ServiceWorker</see>
    /// </summary>
    [Description("@#parsed")]
    Parsed = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/ServiceWorker/#dom-serviceworkerstate-installing">Service Workers Nightly: 3.1 ServiceWorker</see>
    /// </summary>
    [Description("@#installing")]
    Installing = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/ServiceWorker/#dom-serviceworkerstate-installed">Service Workers Nightly: 3.1 ServiceWorker</see>
    /// </summary>
    [Description("@#installed")]
    Installed = 2,

    /// <summary>
    /// <see href="https://w3c.github.io/ServiceWorker/#dom-serviceworkerstate-activating">Service Workers Nightly: 3.1 ServiceWorker</see>
    /// </summary>
    [Description("@#activating")]
    Activating = 3,

    /// <summary>
    /// <see href="https://w3c.github.io/ServiceWorker/#dom-serviceworkerstate-activated">Service Workers Nightly: 3.1 ServiceWorker</see>
    /// </summary>
    [Description("@#activated")]
    Activated = 4,

    /// <summary>
    /// <see href="https://w3c.github.io/ServiceWorker/#dom-serviceworkerstate-redundant">Service Workers Nightly: 3.1 ServiceWorker</see>
    /// </summary>
    [Description("@#redundant")]
    Redundant = 5
}

/// <summary>
/// <see href="https://w3c.github.io/ServiceWorker/#enumdef-serviceworkerupdateviacache">Service Workers Nightly: 3.2 ServiceWorkerRegistration</see>
/// </summary>
[Description("@#ServiceWorkerUpdateViaCache")]
[ECMAScript]
[String]
public enum ServiceWorkerUpdateViaCache
{
    /// <summary>
    /// <see href="https://w3c.github.io/ServiceWorker/#dom-serviceworkerupdateviacache-imports">Service Workers Nightly: 3.2 ServiceWorkerRegistration</see>
    /// </summary>
    [Description("@#imports")]
    Imports = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/ServiceWorker/#dom-serviceworkerupdateviacache-all">Service Workers Nightly: 3.2 ServiceWorkerRegistration</see>
    /// </summary>
    [Description("@#all")]
    All = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/ServiceWorker/#dom-serviceworkerupdateviacache-none">Service Workers Nightly: 3.2 ServiceWorkerRegistration</see>
    /// </summary>
    [Description("@#none")]
    None = 2
}

/// <summary>
/// <see href="https://w3c.github.io/accelerometer/#enumdef-accelerometerlocalcoordinatesystem">Accelerometer: 7.1 The Accelerometer Interface</see>
/// </summary>
[Description("@#AccelerometerLocalCoordinateSystem")]
[ECMAScript]
[String]
public enum AccelerometerLocalCoordinateSystem
{
    /// <summary>
    /// <see href="https://w3c.github.io/accelerometer/#dom-accelerometerlocalcoordinatesystem-device">Accelerometer: 7.1 The Accelerometer Interface</see>
    /// </summary>
    [Description("@#device")]
    Device = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/accelerometer/#dom-accelerometerlocalcoordinatesystem-screen">Accelerometer: 7.1 The Accelerometer Interface</see>
    /// </summary>
    [Description("@#screen")]
    Screen = 1
}

/// <summary>
/// <see href="https://w3c.github.io/aria/#dom-arianotifypriority">Accessible Rich Internet Applications (WAI-ARIA) 1.3: 10.2 Interface Mixin ARIANotifyMixin</see>
/// </summary>
[Description("@#AriaNotifyPriority")]
[ECMAScript]
[String]
public enum AriaNotifyPriority
{
    /// <summary>
    /// <see href="https://w3c.github.io/aria/#dom-arianotifypriority-normal">Accessible Rich Internet Applications (WAI-ARIA) 1.3: 10.2 Interface Mixin ARIANotifyMixin</see>
    /// </summary>
    [Description("@#normal")]
    Normal = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/aria/#dom-arianotifypriority-high">Accessible Rich Internet Applications (WAI-ARIA) 1.3: 10.2 Interface Mixin ARIANotifyMixin</see>
    /// </summary>
    [Description("@#high")]
    High = 1
}

/// <summary>
/// <see href="https://w3c.github.io/attribution/#enumdef-attributionaggregationprotocol">Attribution Level 1: 3.3 Finding a Supported Aggregation Service</see>
/// </summary>
[Description("@#AttributionAggregationProtocol")]
[ECMAScript]
[String]
public enum AttributionAggregationProtocol
{
    /// <summary>
    /// The URL for &quot;dap-18-histogram&quot; is expected to identify the DAP Leader role. Implementations need to obtain HPKE configuration for both Aggregators statically; see #unconfigured. The HPKE configuration must not be fetched on demand, as the time that takes will leak information to callers of measureConversion().
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/attribution/#dom-attributionaggregationprotocol-dap-18-histogram">Attribution Level 1: 3.3 Finding a Supported Aggregation Service</see>
    /// </remarks>
    [Description("@#dap-18-histogram")]
    Dap18Histogram = 0
}

/// <summary>
/// <see href="https://w3c.github.io/audio-session/#enumdef-audiosessionstate">Audio Session: 3.2 Audio session states</see>
/// </summary>
[Description("@#AudioSessionState")]
[ECMAScript]
[String]
public enum AudioSessionState
{
    /// <summary>
    /// <see href="https://w3c.github.io/audio-session/#dom-audiosessionstate-inactive">Audio Session: 3.2 Audio session states</see>
    /// </summary>
    [Description("@#inactive")]
    Inactive = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/audio-session/#dom-audiosessionstate-active">Audio Session: 3.2 Audio session states</see>
    /// </summary>
    [Description("@#active")]
    Active = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/audio-session/#dom-audiosessionstate-interrupted">Audio Session: 3.2 Audio session states</see>
    /// </summary>
    [Description("@#interrupted")]
    Interrupted = 2
}

/// <summary>
/// <see href="https://w3c.github.io/audio-session/#enumdef-audiosessiontype">Audio Session: 3.1 Audio session types</see>
/// </summary>
[Description("@#AudioSessionType")]
[ECMAScript]
[String]
public enum AudioSessionType
{
    /// <summary>
    /// Auto lets the user agent choose the best audio session type according the use of audio by the web page. This is the default type of AudioSession.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/audio-session/#dom-audiosessiontype-auto">Audio Session: 3.1 Audio session types</see>
    /// </remarks>
    [Description("@#auto")]
    Auto = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/audio-session/#dom-audiosessiontype-playback">Audio Session: 3.1 Audio session types</see>
    /// </summary>
    [Description("@#playback")]
    Playback = 1,

    /// <summary>
    /// transient-solo
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/audio-session/#dom-audiosessiontype-transient">Audio Session: 3.1 Audio session types</see>
    /// </remarks>
    [Description("@#transient")]
    Transient = 2,

    /// <summary>
    /// transient-solo
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/audio-session/#dom-audiosessiontype-transient-solo">Audio Session: 3.1 Audio session types</see>
    /// </remarks>
    [Description("@#transient-solo")]
    TransientSolo = 3,

    /// <summary>
    /// <see href="https://w3c.github.io/audio-session/#dom-audiosessiontype-ambient">Audio Session: 3.1 Audio session types</see>
    /// </summary>
    [Description("@#ambient")]
    Ambient = 4,

    /// <summary>
    /// play-and-record
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/audio-session/#dom-audiosessiontype-play-and-record">Audio Session: 3.1 Audio session types</see>
    /// </remarks>
    [Description("@#play-and-record")]
    PlayAndRecord = 5
}

/// <summary>
/// <see href="https://w3c.github.io/autoplay/#enumdef-autoplaypolicy">Autoplay Policy Detection: 2.1 Autoplay Policy Enum</see>
/// </summary>
[Description("@#AutoplayPolicy")]
[ECMAScript]
[String]
public enum AutoplayPolicy
{
    /// <summary>
    /// <see href="https://w3c.github.io/autoplay/#dom-autoplaypolicy-allowed">Autoplay Policy Detection: 2.1 Autoplay Policy Enum</see>
    /// </summary>
    [Description("@#allowed")]
    Allowed = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/autoplay/#dom-autoplaypolicy-allowed-muted">Autoplay Policy Detection: 2.1 Autoplay Policy Enum</see>
    /// </summary>
    [Description("@#allowed-muted")]
    AllowedMuted = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/autoplay/#dom-autoplaypolicy-disallowed">Autoplay Policy Detection: 2.1 Autoplay Policy Enum</see>
    /// </summary>
    [Description("@#disallowed")]
    Disallowed = 2
}

/// <summary>
/// <see href="https://w3c.github.io/autoplay/#enumdef-autoplaypolicymediatype">Autoplay Policy Detection: 2.2 The Autoplay Detection Methods</see>
/// </summary>
[Description("@#AutoplayPolicyMediaType")]
[ECMAScript]
[String]
public enum AutoplayPolicyMediaType
{
    /// <summary>
    /// If type is mediaelement, return a result that represents the current status for HTMLMediaElement and its extensions, such as HTMLVideoElement and HTMLAudioElement, which exist in the document contained in the Window object associated with the queried Navigator object.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/autoplay/#dom-autoplaypolicymediatype-mediaelement">Autoplay Policy Detection: 2.2 The Autoplay Detection Methods</see>
    /// </remarks>
    [Description("@#mediaelement")]
    Mediaelement = 0,

    /// <summary>
    /// If type is audiocontext, return a result that represents the current status for AudioContext, which exist in the document contained in the Window object associated with the queried Navigator object.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/autoplay/#dom-autoplaypolicymediatype-audiocontext">Autoplay Policy Detection: 2.2 The Autoplay Detection Methods</see>
    /// </remarks>
    [Description("@#audiocontext")]
    Audiocontext = 1
}

/// <summary>
/// <see href="https://w3c.github.io/clipboard-apis/#enumdef-presentationstyle">Clipboard API and events: 7.2 ClipboardItem Interface</see>
/// </summary>
[Description("@#PresentationStyle")]
[ECMAScript]
[String]
public enum PresentationStyle
{
    /// <summary>
    /// <see href="https://w3c.github.io/clipboard-apis/#dom-presentationstyle-unspecified">Clipboard API and events: 7.2 ClipboardItem Interface</see>
    /// </summary>
    [Description("@#unspecified")]
    Unspecified = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/clipboard-apis/#dom-presentationstyle-inline">Clipboard API and events: 7.2 ClipboardItem Interface</see>
    /// </summary>
    [Description("@#inline")]
    Inline = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/clipboard-apis/#dom-presentationstyle-attachment">Clipboard API and events: 7.2 ClipboardItem Interface</see>
    /// </summary>
    [Description("@#attachment")]
    Attachment = 2
}

/// <summary>
/// <see href="https://w3c.github.io/compute-pressure/#dom-pressuresource">Compute Pressure Level 1: 3.2 Pressure sources</see>
/// </summary>
[Description("@#PressureSource")]
[ECMAScript]
[String]
public enum PressureSource
{
    /// <summary>
    /// &quot;cpu&quot; represents the average pressure of the central processing unit across all its cores.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/compute-pressure/#dom-pressuresource-cpu">Compute Pressure Level 1: 3.2 Pressure sources</see>
    /// </remarks>
    [Description("@#cpu")]
    Cpu = 0
}

/// <summary>
/// <see href="https://w3c.github.io/compute-pressure/#dom-pressurestate">Compute Pressure Level 1: 8 Pressure States</see>
/// </summary>
[Description("@#PressureState")]
[ECMAScript]
[String]
public enum PressureState
{
    /// <summary>
    /// &quot;nominal&quot;: The conditions of the target device are at an acceptable level with no noticeable adverse effects on the user.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/compute-pressure/#dom-pressurestate-nominal">Compute Pressure Level 1: 8 Pressure States</see>
    /// </remarks>
    [Description("@#nominal")]
    Nominal = 0,

    /// <summary>
    /// &quot;fair&quot;: Target device pressure, temperature and/or energy usage are slightly elevated, potentially resulting in reduced battery-life, as well as fans (or systems with fans) becoming active and audible. Apart from that the target device is running flawlessly and can take on additional work.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/compute-pressure/#dom-pressurestate-fair">Compute Pressure Level 1: 8 Pressure States</see>
    /// </remarks>
    [Description("@#fair")]
    Fair = 1,

    /// <summary>
    /// &quot;serious&quot;: Target device pressure, temperature and/or energy usage is consistently highly elevated. The system may be throttling as a countermeasure to reduce thermals.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/compute-pressure/#dom-pressurestate-serious">Compute Pressure Level 1: 8 Pressure States</see>
    /// </remarks>
    [Description("@#serious")]
    Serious = 2,

    /// <summary>
    /// &quot;critical&quot;: The temperature of the target device or system is significantly elevated and it requires cooling down to avoid any potential issues.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/compute-pressure/#dom-pressurestate-critical">Compute Pressure Level 1: 8 Pressure States</see>
    /// </remarks>
    [Description("@#critical")]
    Critical = 3
}

/// <summary>
/// <see href="https://w3c.github.io/contact-picker/#enumdef-contactproperty">Contact Picker API: 6.2 ContactProperty</see>
/// </summary>
[Description("@#ContactProperty")]
[ECMAScript]
[String]
public enum ContactProperty
{
    /// <summary>
    /// <see href="https://w3c.github.io/contact-picker/#dom-contactproperty-address">Contact Picker API: Contact Picker API</see>
    /// </summary>
    [Description("@#address")]
    Address = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/contact-picker/#dom-contactproperty-email">Contact Picker API: Contact Picker API</see>
    /// </summary>
    [Description("@#email")]
    Email = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/contact-picker/#dom-contactproperty-icon">Contact Picker API: Contact Picker API</see>
    /// </summary>
    [Description("@#icon")]
    Icon = 2,

    /// <summary>
    /// <see href="https://w3c.github.io/contact-picker/#dom-contactproperty-name">Contact Picker API: Contact Picker API</see>
    /// </summary>
    [Description("@#name")]
    Name = 3,

    /// <summary>
    /// <see href="https://w3c.github.io/contact-picker/#dom-contactproperty-tel">Contact Picker API: Contact Picker API</see>
    /// </summary>
    [Description("@#tel")]
    Tel = 4
}

/// <summary>
/// <see href="https://w3c.github.io/encrypted-media/#dom-mediakeysessionclosedreason">Encrypted Media Extensions: 6 MediaKeySession Interface</see>
/// </summary>
[Description("@#MediaKeySessionClosedReason")]
[ECMAScript]
[String]
public enum MediaKeySessionClosedReason
{
    /// <summary>
    /// If cdm has become unavailable for any other reason, queue a task to run the CDM Unavailable algorithm with reason &quot;internal-error&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/encrypted-media/#dom-mediakeysessionclosedreason-internal-error">Encrypted Media Extensions: 6 MediaKeySession Interface</see>
    /// </remarks>
    [Description("@#internal-error")]
    InternalError = 0,

    /// <summary>
    /// The returned promise is resolved when the request has been processed, and the closed attribute promise is resolved with &quot;closed-by-application&quot; when the session is closed.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/encrypted-media/#dom-mediakeysessionclosedreason-closed-by-application">Encrypted Media Extensions: 6 MediaKeySession Interface</see>
    /// </remarks>
    [Description("@#closed-by-application")]
    ClosedByApplication = 1,

    /// <summary>
    /// Run the Session Closed algorithm on this object with reason &quot;release-acknowledged&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/encrypted-media/#dom-mediakeysessionclosedreason-release-acknowledged">Encrypted Media Extensions: 6 MediaKeySession Interface</see>
    /// </remarks>
    [Description("@#release-acknowledged")]
    ReleaseAcknowledged = 2,

    /// <summary>
    /// If cdm has become unavailable due to a hardware context reset, queue a task to run the CDM Unavailable algorithm with reason &quot;hardware-context-reset&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/encrypted-media/#dom-mediakeysessionclosedreason-hardware-context-reset">Encrypted Media Extensions: 6 MediaKeySession Interface</see>
    /// </remarks>
    [Description("@#hardware-context-reset")]
    HardwareContextReset = 3,

    /// <summary>
    /// <see href="https://w3c.github.io/encrypted-media/#dom-mediakeysessionclosedreason-resource-evicted">Encrypted Media Extensions: 6 MediaKeySession Interface</see>
    /// </summary>
    [Description("@#resource-evicted")]
    ResourceEvicted = 4
}

/// <summary>
/// <see href="https://w3c.github.io/encrypted-media/#dom-mediakeysessiontype">Encrypted Media Extensions: 5 MediaKeys Interface</see>
/// </summary>
[Description("@#MediaKeySessionType")]
[ECMAScript]
[String]
public enum MediaKeySessionType
{
    /// <summary>
    /// Let session types be [ &quot;temporary&quot; ].
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/encrypted-media/#dom-mediakeysessiontype-temporary">Encrypted Media Extensions: 5 MediaKeys Interface</see>
    /// </remarks>
    [Description("@#temporary")]
    Temporary = 0,

    /// <summary>
    /// &quot;persistent-license&quot;
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/encrypted-media/#dom-mediakeysessiontype-persistent-license">Encrypted Media Extensions: 5 MediaKeys Interface</see>
    /// </remarks>
    [Description("@#persistent-license")]
    PersistentLicense = 1
}

/// <summary>
/// <see href="https://w3c.github.io/encrypted-media/#dom-mediakeysrequirement">Encrypted Media Extensions: 3.3 MediaKeySystemConfiguration dictionary</see>
/// </summary>
[Description("@#MediaKeysRequirement")]
[ECMAScript]
[String]
public enum MediaKeysRequirement
{
    /// <summary>
    /// distinctiveIdentifier controls whether Distinctive Permanent Identifiers may be used. Specifically, Distinctive Permanent Identifiers may only be used when the value of the distinctiveIdentifier member of the MediaKeySystemAccess used to create the MediaKeys object is &quot;required&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/encrypted-media/#dom-mediakeysrequirement-required">Encrypted Media Extensions: 3.3 MediaKeySystemConfiguration dictionary</see>
    /// </remarks>
    [Description("@#required")]
    Required = 0,

    /// <summary>
    /// If distinctive identifier requirement is &quot;optional&quot; and Distinctive Identifiers are not allowed according to restrictions, set distinctive identifier requirement to &quot;not-allowed&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/encrypted-media/#dom-mediakeysrequirement-optional">Encrypted Media Extensions: 3.3 MediaKeySystemConfiguration dictionary</see>
    /// </remarks>
    [Description("@#optional")]
    Optional = 1,

    /// <summary>
    /// If distinctive identifier requirement is &quot;optional&quot; and Distinctive Identifiers are not allowed according to restrictions, set distinctive identifier requirement to &quot;not-allowed&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/encrypted-media/#dom-mediakeysrequirement-not-allowed">Encrypted Media Extensions: 3.3 MediaKeySystemConfiguration dictionary</see>
    /// </remarks>
    [Description("@#not-allowed")]
    NotAllowed = 2
}

/// <summary>
/// <see href="https://w3c.github.io/encrypted-media/#dom-mediakeystatus">Encrypted Media Extensions: 6.3 MediaKeyStatusMap Interface</see>
/// </summary>
[Description("@#MediaKeyStatus")]
[ECMAScript]
[String]
public enum MediaKeyStatus
{
    /// <summary>
    /// Add &quot;usable-in-future&quot; to MediaKeyStatus for keys that are not yet usable for decryption.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/encrypted-media/#dom-mediakeystatus-usable">Encrypted Media Extensions: 6.3 MediaKeyStatusMap Interface</see>
    /// </remarks>
    [Description("@#usable")]
    Usable = 0,

    /// <summary>
    /// The map entries and their values may be updated whenever the event loop spins. The map will never be inconsistent or partially updated, but it may change between accesses if the event loop spins in between the accesses. Key IDs may be added as the result of a load() or update() call. Key IDs may be removed as the result of a update() call that removes knowledge of existing keys (or replaces the existing set of keys with a new set). Key IDs are not removed if they became unusable, such as due to expiration. Instead, such keys MUST be given an appropriate status, such as &quot;expired&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/encrypted-media/#dom-mediakeystatus-expired">Encrypted Media Extensions: 6.3 MediaKeyStatusMap Interface</see>
    /// </remarks>
    [Description("@#expired")]
    Expired = 1,

    /// <summary>
    /// Run the Update Key Statuses algorithm on the session, providing all key ID(s) in the session along with the &quot;released&quot; MediaKeyStatus value for each.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/encrypted-media/#dom-mediakeystatus-released">Encrypted Media Extensions: 6.3 MediaKeyStatusMap Interface</see>
    /// </remarks>
    [Description("@#released")]
    Released = 2,

    /// <summary>
    /// If the CDM would block presentation of decrypted media data for the dictionary member, then resolve promise with &quot;output-restricted&quot; and abort these steps.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/encrypted-media/#dom-mediakeystatus-output-restricted">Encrypted Media Extensions: 6.3 MediaKeyStatusMap Interface</see>
    /// </remarks>
    [Description("@#output-restricted")]
    OutputRestricted = 3,

    /// <summary>
    /// For example, if a key has output requirements that cannot currently be met, the key&apos;s status should be &quot;output-downscaled&quot; or &quot;output-restricted&quot;, as appropriate, regardless of whether that key has been or is currently needed to decrypt media data.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/encrypted-media/#dom-mediakeystatus-output-downscaled">Encrypted Media Extensions: 6.3 MediaKeyStatusMap Interface</see>
    /// </remarks>
    [Description("@#output-downscaled")]
    OutputDownscaled = 4,

    /// <summary>
    /// Add &quot;usable-in-future&quot; to MediaKeyStatus for keys that are not yet usable for decryption.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/encrypted-media/#dom-mediakeystatus-usable-in-future">Encrypted Media Extensions: 6.3 MediaKeyStatusMap Interface</see>
    /// </remarks>
    [Description("@#usable-in-future")]
    UsableInFuture = 5,

    /// <summary>
    /// Should additional processing be necessary to determine with certainty the status of a key, use &quot;status-pending&quot;. Once the additional processing for one or more keys has completed, run the Update Key Statuses algorithm again with the actual status(es).
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/encrypted-media/#dom-mediakeystatus-status-pending">Encrypted Media Extensions: 6.3 MediaKeyStatusMap Interface</see>
    /// </remarks>
    [Description("@#status-pending")]
    StatusPending = 6,

    /// <summary>
    /// <see href="https://w3c.github.io/encrypted-media/#dom-mediakeystatus-internal-error">Encrypted Media Extensions: 6.3 MediaKeyStatusMap Interface</see>
    /// </summary>
    [Description("@#internal-error")]
    InternalError = 7
}

/// <summary>
/// <see href="https://w3c.github.io/gamepad/#dom-gamepadhapticeffecttype">Gamepad: 10. GamepadHapticEffectType Enum</see>
/// </summary>
[Description("@#GamepadHapticEffectType")]
[ECMAScript]
[String]
public enum GamepadHapticEffectType
{
    /// <summary>
    /// &quot;dual-rumble&quot;
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/gamepad/#dom-gamepadhapticeffecttype-dual-rumble">Gamepad: 10. GamepadHapticEffectType Enum</see>
    /// </remarks>
    [Description("@#dual-rumble")]
    DualRumble = 0,

    /// <summary>
    /// &quot;trigger-rumble&quot;
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/gamepad/#dom-gamepadhapticeffecttype-trigger-rumble">Gamepad: 10. GamepadHapticEffectType Enum</see>
    /// </remarks>
    [Description("@#trigger-rumble")]
    TriggerRumble = 1
}

/// <summary>
/// <see href="https://w3c.github.io/gamepad/#dom-gamepadhapticsresult">Gamepad: 9 GamepadHapticsResult Enum</see>
/// </summary>
[Description("@#GamepadHapticsResult")]
[ECMAScript]
[String]
public enum GamepadHapticsResult
{
    /// <summary>
    /// Resolve this.playingEffectPromise with &quot;complete&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/gamepad/#dom-gamepadhapticsresult-complete">Gamepad: 9 GamepadHapticsResult Enum</see>
    /// </remarks>
    [Description("@#complete")]
    Complete = 0,

    /// <summary>
    /// Queue a global task on the gamepad task source with the relevant global object of this to resolve effectPromise with &quot;preempted&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/gamepad/#dom-gamepadhapticsresult-preempted">Gamepad: 9 GamepadHapticsResult Enum</see>
    /// </remarks>
    [Description("@#preempted")]
    Preempted = 1
}

/// <summary>
/// <see href="https://w3c.github.io/gyroscope/#enumdef-gyroscopelocalcoordinatesystem">Gyroscope: 7.1 The Gyroscope Interface</see>
/// </summary>
[Description("@#GyroscopeLocalCoordinateSystem")]
[ECMAScript]
[String]
public enum GyroscopeLocalCoordinateSystem
{
    /// <summary>
    /// <see href="https://w3c.github.io/gyroscope/#dom-gyroscopelocalcoordinatesystem-device">Gyroscope: 7.1 The Gyroscope Interface</see>
    /// </summary>
    [Description("@#device")]
    Device = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/gyroscope/#dom-gyroscopelocalcoordinatesystem-screen">Gyroscope: 7.1 The Gyroscope Interface</see>
    /// </summary>
    [Description("@#screen")]
    Screen = 1
}

/// <summary>
/// <see href="https://w3c.github.io/long-animation-frames/#enumdef-scriptinvokertype">Long Animation Frames API: 2.2 PerformanceScriptTiming interface</see>
/// </summary>
[Description("@#ScriptInvokerType")]
[ECMAScript]
[String]
public enum ScriptInvokerType
{
    /// <summary>
    /// <see href="https://w3c.github.io/long-animation-frames/#dom-scriptinvokertype-classic-script">Long Animation Frames API: 2.2 PerformanceScriptTiming interface</see>
    /// </summary>
    [Description("@#classic-script")]
    ClassicScript = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/long-animation-frames/#dom-scriptinvokertype-module-script">Long Animation Frames API: 2.2 PerformanceScriptTiming interface</see>
    /// </summary>
    [Description("@#module-script")]
    ModuleScript = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/long-animation-frames/#dom-scriptinvokertype-event-listener">Long Animation Frames API: 2.2 PerformanceScriptTiming interface</see>
    /// </summary>
    [Description("@#event-listener")]
    EventListener = 2,

    /// <summary>
    /// <see href="https://w3c.github.io/long-animation-frames/#dom-scriptinvokertype-user-callback">Long Animation Frames API: 2.2 PerformanceScriptTiming interface</see>
    /// </summary>
    [Description("@#user-callback")]
    UserCallback = 3,

    /// <summary>
    /// <see href="https://w3c.github.io/long-animation-frames/#dom-scriptinvokertype-resolve-promise">Long Animation Frames API: 2.2 PerformanceScriptTiming interface</see>
    /// </summary>
    [Description("@#resolve-promise")]
    ResolvePromise = 4,

    /// <summary>
    /// <see href="https://w3c.github.io/long-animation-frames/#dom-scriptinvokertype-reject-promise">Long Animation Frames API: 2.2 PerformanceScriptTiming interface</see>
    /// </summary>
    [Description("@#reject-promise")]
    RejectPromise = 5
}

/// <summary>
/// <see href="https://w3c.github.io/long-animation-frames/#enumdef-scriptwindowattribution">Long Animation Frames API: 2.2 PerformanceScriptTiming interface</see>
/// </summary>
[Description("@#ScriptWindowAttribution")]
[ECMAScript]
[String]
public enum ScriptWindowAttribution
{
    /// <summary>
    /// <see href="https://w3c.github.io/long-animation-frames/#dom-scriptwindowattribution-self">Long Animation Frames API: 2.2 PerformanceScriptTiming interface</see>
    /// </summary>
    [Description("@#self")]
    Self = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/long-animation-frames/#dom-scriptwindowattribution-descendant">Long Animation Frames API: 2.2 PerformanceScriptTiming interface</see>
    /// </summary>
    [Description("@#descendant")]
    Descendant = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/long-animation-frames/#dom-scriptwindowattribution-ancestor">Long Animation Frames API: 2.2 PerformanceScriptTiming interface</see>
    /// </summary>
    [Description("@#ancestor")]
    Ancestor = 2,

    /// <summary>
    /// <see href="https://w3c.github.io/long-animation-frames/#dom-scriptwindowattribution-same-page">Long Animation Frames API: 2.2 PerformanceScriptTiming interface</see>
    /// </summary>
    [Description("@#same-page")]
    SamePage = 3,

    /// <summary>
    /// <see href="https://w3c.github.io/long-animation-frames/#dom-scriptwindowattribution-other">Long Animation Frames API: 2.2 PerformanceScriptTiming interface</see>
    /// </summary>
    [Description("@#other")]
    Other = 4
}

/// <summary>
/// <see href="https://w3c.github.io/magnetometer/#enumdef-magnetometerlocalcoordinatesystem">Magnetometer: 6.1 The Magnetometer Interface</see>
/// </summary>
[Description("@#MagnetometerLocalCoordinateSystem")]
[ECMAScript]
[String]
public enum MagnetometerLocalCoordinateSystem
{
    /// <summary>
    /// <see href="https://w3c.github.io/magnetometer/#dom-magnetometerlocalcoordinatesystem-device">Magnetometer: 6.1 The Magnetometer Interface</see>
    /// </summary>
    [Description("@#device")]
    Device = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/magnetometer/#dom-magnetometerlocalcoordinatesystem-screen">Magnetometer: 6.1 The Magnetometer Interface</see>
    /// </summary>
    [Description("@#screen")]
    Screen = 1
}

/// <summary>
/// <see href="https://w3c.github.io/media-capabilities/#enumdef-colorgamut">Media Capabilities: 2.1.6 ColorGamut</see>
/// </summary>
[Description("@#ColorGamut")]
[ECMAScript]
[String]
public enum ColorGamut
{
    /// <summary>
    /// srgb, representing the !sRGB color gamut.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/media-capabilities/#dom-colorgamut-srgb">Media Capabilities: 2.1.6 ColorGamut</see>
    /// </remarks>
    [Description("@#srgb")]
    Srgb = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/media-capabilities/#dom-colorgamut-p3">Media Capabilities: 2.1.6 ColorGamut</see>
    /// </summary>
    [Description("@#p3")]
    P3 = 1,

    /// <summary>
    /// rec2020, representing the ITU-R Recommendation BT.2020 color gamut. This color gamut includes the p3 gamut.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/media-capabilities/#dom-colorgamut-rec2020">Media Capabilities: 2.1.6 ColorGamut</see>
    /// </remarks>
    [Description("@#rec2020")]
    Rec2020 = 2
}

/// <summary>
/// <see href="https://w3c.github.io/mediacapture-automation/#dom-mockcapturepromptresult">Media Capture Automation: 3.1.1 Set capture prompt result</see>
/// </summary>
[Description("@#MockCapturePromptResult")]
[ECMAScript]
[String]
public enum MockCapturePromptResult
{
    /// <summary>
    /// <see href="https://w3c.github.io/mediacapture-automation/#dom-mockcapturepromptresult-granted">Media Capture Automation: 3.1.1 Set capture prompt result</see>
    /// </summary>
    [Description("@#granted")]
    Granted = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/mediacapture-automation/#dom-mockcapturepromptresult-denied">Media Capture Automation: 3.1.1 Set capture prompt result</see>
    /// </summary>
    [Description("@#denied")]
    Denied = 1
}

/// <summary>
/// <see href="https://w3c.github.io/mediacapture-handle/actions/#dom-captureaction">The Capture-Handle Actions Mechanism: 3.1.1 Registering and responding to capture actions</see>
/// </summary>
[Description("@#CaptureAction")]
[ECMAScript]
[String]
public enum CaptureAction
{
    /// <summary>
    /// <see href="https://w3c.github.io/mediacapture-handle/actions/#dom-captureaction-next">The Capture-Handle Actions Mechanism: 3.1.1 Registering and responding to capture actions</see>
    /// </summary>
    [Description("@#next")]
    Next = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/mediacapture-handle/actions/#dom-captureaction-previous">The Capture-Handle Actions Mechanism: 3.1.1 Registering and responding to capture actions</see>
    /// </summary>
    [Description("@#previous")]
    Previous = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/mediacapture-handle/actions/#dom-captureaction-first">The Capture-Handle Actions Mechanism: 3.1.1 Registering and responding to capture actions</see>
    /// </summary>
    [Description("@#first")]
    First = 2,

    /// <summary>
    /// <see href="https://w3c.github.io/mediacapture-handle/actions/#dom-captureaction-last">The Capture-Handle Actions Mechanism: 3.1.1 Registering and responding to capture actions</see>
    /// </summary>
    [Description("@#last")]
    Last = 3
}

/// <summary>
/// <see href="https://w3c.github.io/mediacapture-main/#dom-mediadevicekind">Media Capture and Streams: 9.3 Device Info</see>
/// </summary>
[Description("@#MediaDeviceKind")]
[ECMAScript]
[String]
public enum MediaDeviceKind
{
    /// <summary>
    /// <see href="https://w3c.github.io/mediacapture-main/#idl-def-MediaDeviceKind.audioinput">Media Capture and Streams: 9.3 Device Info</see>
    /// </summary>
    [Description("@#audioinput")]
    Audioinput = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/mediacapture-main/#idl-def-MediaDeviceKind.audiooutput">Media Capture and Streams: 9.3 Device Info</see>
    /// </summary>
    [Description("@#audiooutput")]
    Audiooutput = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/mediacapture-main/#idl-def-MediaDeviceKind.videoinput">Media Capture and Streams: 9.3 Device Info</see>
    /// </summary>
    [Description("@#videoinput")]
    Videoinput = 2
}

/// <summary>
/// <see href="https://w3c.github.io/mediacapture-main/#dom-videofacingmodeenum">Media Capture and Streams: 4.3.8 Constrainable Properties</see>
/// </summary>
[Description("@#VideoFacingModeEnum")]
[ECMAScript]
[String]
public enum VideoFacingModeEnum
{
    /// <summary>
    /// A camera can report multiple facing modes. For example, in a high-end telepresence solution with several cameras facing the user, a camera to the left of the user can report both &quot;left&quot; and &quot;user&quot;. See facingMode for additional details.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-main/#dom-videofacingmodeenum-user">Media Capture and Streams: 4.3.8 Constrainable Properties</see>
    /// </remarks>
    [Description("@#user")]
    User = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/mediacapture-main/#dom-videofacingmodeenum-environment">Media Capture and Streams: 4.3.8 Constrainable Properties</see>
    /// </summary>
    [Description("@#environment")]
    Environment = 1,

    /// <summary>
    /// A camera can report multiple facing modes. For example, in a high-end telepresence solution with several cameras facing the user, a camera to the left of the user can report both &quot;left&quot; and &quot;user&quot;. See facingMode for additional details.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-main/#dom-videofacingmodeenum-left">Media Capture and Streams: 4.3.8 Constrainable Properties</see>
    /// </remarks>
    [Description("@#left")]
    Left = 2,

    /// <summary>
    /// <see href="https://w3c.github.io/mediacapture-main/#dom-videofacingmodeenum-right">Media Capture and Streams: 4.3.8 Constrainable Properties</see>
    /// </summary>
    [Description("@#right")]
    Right = 3
}

/// <summary>
/// <see href="https://w3c.github.io/mediacapture-main/#dom-videoresizemodeenum">Media Capture and Streams: 4.3.8 Constrainable Properties</see>
/// </summary>
[Description("@#VideoResizeModeEnum")]
[ECMAScript]
[String]
public enum VideoResizeModeEnum
{
    /// <summary>
    /// For every settings dictionary with resizeMode set to &quot;none&quot;, the User Agent MUST include another otherwise identical settings dictionary with resizeMode set to &quot;crop-and-scale&quot;. Constraining around non-native modes is not supported.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-main/#idl-def-VideoResizeModeEnum.none">Media Capture and Streams: 4.3.8 Constrainable Properties</see>
    /// </remarks>
    [Description("@#none")]
    None = 0,

    /// <summary>
    /// For every settings dictionary with resizeMode set to &quot;none&quot;, the User Agent MUST include another otherwise identical settings dictionary with resizeMode set to &quot;crop-and-scale&quot;. Constraining around non-native modes is not supported.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-main/#idl-def-VideoResizeModeEnum.cropandscale">Media Capture and Streams: 4.3.8 Constrainable Properties</see>
    /// </remarks>
    [Description("@#crop-and-scale")]
    CropAndScale = 1
}

/// <summary>
/// <see href="https://w3c.github.io/mediacapture-record/#enumdef-recordingstate">MediaStream Recording: 2.7 RecordingState</see>
/// </summary>
[Description("@#RecordingState")]
[ECMAScript]
[String]
public enum RecordingState
{
    /// <summary>
    /// <see href="https://w3c.github.io/mediacapture-record/#dom-recordingstate-inactive">MediaStream Recording: 2.7.1 Values</see>
    /// </summary>
    [Description("@#inactive")]
    Inactive = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/mediacapture-record/#dom-recordingstate-recording">MediaStream Recording: 2.7.1 Values</see>
    /// </summary>
    [Description("@#recording")]
    Recording = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/mediacapture-record/#dom-recordingstate-paused">MediaStream Recording: 2.7.1 Values</see>
    /// </summary>
    [Description("@#paused")]
    Paused = 2
}

/// <summary>
/// <see href="https://w3c.github.io/mediacapture-screen-share/#dom-capturestartfocusbehavior">Screen Capture: 5.4.2 CaptureStartFocusBehavior</see>
/// </summary>
[Description("@#CaptureStartFocusBehavior")]
[ECMAScript]
[String]
public enum CaptureStartFocusBehavior
{
    /// <summary>
    /// If focusBehavior is &quot;focus-capturing-application&quot;, focus the display surface representing the capturing document.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-screen-share/#idl-def-CaptureStartFocusBehavior.focus-capturing-application">Screen Capture: 5.4.2 CaptureStartFocusBehavior</see>
    /// </remarks>
    [Description("@#focus-capturing-application")]
    FocusCapturingApplication = 0,

    /// <summary>
    /// If focusBehavior is &quot;focus-captured-surface&quot;, focus the display surface referred to by controller.Source.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-screen-share/#idl-def-CaptureStartFocusBehavior.focus-captured-surface">Screen Capture: 5.4.2 CaptureStartFocusBehavior</see>
    /// </remarks>
    [Description("@#focus-captured-surface")]
    FocusCapturedSurface = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/mediacapture-screen-share/#idl-def-CaptureStartFocusBehavior.no-focus-change">Screen Capture: 5.4.2 CaptureStartFocusBehavior</see>
    /// </summary>
    [Description("@#no-focus-change")]
    NoFocusChange = 2
}

/// <summary>
/// <see href="https://w3c.github.io/mediasession/#enumdef-mediasessionaction">Media Session: 5 The MediaSession interface</see>
/// </summary>
[Description("@#MediaSessionAction")]
[ECMAScript]
[String]
public enum MediaSessionAction
{
    /// <summary>
    /// In order to make play and pause actions work properly, the user agent SHOULD be able to determine if a /browsing context of the active media session is playing media or not, which is called the guessed playback state. The RECOMMENDED way for determining the guessed playback state is to monitor the media elements whose node document&apos;s Document/browsing context is the /browsing context. The /browsing context&apos;s guessed playback state is &quot;playing&quot; if any of them is media element/potentially playing and not media element/muted, and is &quot;paused&quot; otherwise. Other information SHOULD also be considered, such as WebAudio and plugins.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediasession/#dom-mediasessionaction-play">Media Session: 4.4 Actions</see>
    /// </remarks>
    [Description("@#play")]
    Play = 0,

    /// <summary>
    /// In order to make play and pause actions work properly, the user agent SHOULD be able to determine if a /browsing context of the active media session is playing media or not, which is called the guessed playback state. The RECOMMENDED way for determining the guessed playback state is to monitor the media elements whose node document&apos;s Document/browsing context is the /browsing context. The /browsing context&apos;s guessed playback state is &quot;playing&quot; if any of them is media element/potentially playing and not media element/muted, and is &quot;paused&quot; otherwise. Other information SHOULD also be considered, such as WebAudio and plugins.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediasession/#dom-mediasessionaction-pause">Media Session: 4.4 Actions</see>
    /// </remarks>
    [Description("@#pause")]
    Pause = 1,

    /// <summary>
    /// seekbackward: the action&apos;s intent is to move the playback time backward by a short period (eg. a few seconds).
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediasession/#dom-mediasessionaction-seekbackward">Media Session: 4.4 Actions</see>
    /// </remarks>
    [Description("@#seekbackward")]
    Seekbackward = 2,

    /// <summary>
    /// seekforward: the action&apos;s intent is to move the playback time forward by a short period (eg. a few seconds).
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediasession/#dom-mediasessionaction-seekforward">Media Session: 4.4 Actions</see>
    /// </remarks>
    [Description("@#seekforward")]
    Seekforward = 3,

    /// <summary>
    /// previoustrack: the action&apos;s intent is to either start the current playback from the beginning if the playback has a notion of beginning, or move to the previous item in the playlist if the playback has a notion of playlist.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediasession/#dom-mediasessionaction-previoustrack">Media Session: 4.4 Actions</see>
    /// </remarks>
    [Description("@#previoustrack")]
    Previoustrack = 4,

    /// <summary>
    /// nexttrack: the action&apos;s intent is to move to the playback to the next item in the playlist if the playback has a notion of playlist.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediasession/#dom-mediasessionaction-nexttrack">Media Session: 4.4 Actions</see>
    /// </remarks>
    [Description("@#nexttrack")]
    Nexttrack = 5,

    /// <summary>
    /// skipad: the action&apos;s intent is to skip the advertisement that is currently playing.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediasession/#dom-mediasessionaction-skipad">Media Session: 4.4 Actions</see>
    /// </remarks>
    [Description("@#skipad")]
    Skipad = 6,

    /// <summary>
    /// <see href="https://w3c.github.io/mediasession/#dom-mediasessionaction-stop">Media Session: 4.4 Actions</see>
    /// </summary>
    [Description("@#stop")]
    Stop = 7,

    /// <summary>
    /// seekto: the action&apos;s intent is to move the playback time to a specific time.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediasession/#dom-mediasessionaction-seekto">Media Session: 4.4 Actions</see>
    /// </remarks>
    [Description("@#seekto")]
    Seekto = 8,

    /// <summary>
    /// togglemicrophone: the action&apos;s intent is to mute or unmute the user&apos;s microphone.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediasession/#dom-mediasessionaction-togglemicrophone">Media Session: 4.4 Actions</see>
    /// </remarks>
    [Description("@#togglemicrophone")]
    Togglemicrophone = 9,

    /// <summary>
    /// togglecamera: the action&apos;s intent is to turn the user&apos;s active camera on or off.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediasession/#dom-mediasessionaction-togglecamera">Media Session: 4.4 Actions</see>
    /// </remarks>
    [Description("@#togglecamera")]
    Togglecamera = 10,

    /// <summary>
    /// togglescreenshare: the action&apos;s intent is to turn the user&apos;s active screenshare on or off.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediasession/#dom-mediasessionaction-togglescreenshare">Media Session: 4.4 Actions</see>
    /// </remarks>
    [Description("@#togglescreenshare")]
    Togglescreenshare = 11,

    /// <summary>
    /// hangup: the action&apos;s intent is to end a call.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediasession/#dom-mediasessionaction-hangup">Media Session: 4.4 Actions</see>
    /// </remarks>
    [Description("@#hangup")]
    Hangup = 12,

    /// <summary>
    /// previousslide: the action&apos;s intent is to go back to the previous slide when presenting slides.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediasession/#dom-mediasessionaction-previousslide">Media Session: 4.4 Actions</see>
    /// </remarks>
    [Description("@#previousslide")]
    Previousslide = 13,

    /// <summary>
    /// nextslide: the action&apos;s intent is to go to the next slide when presenting slides.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediasession/#dom-mediasessionaction-nextslide">Media Session: 4.4 Actions</see>
    /// </remarks>
    [Description("@#nextslide")]
    Nextslide = 14,

    /// <summary>
    /// enterpictureinpicture: the action&apos;s intent is to open the media session in a picture-in-picture window.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediasession/#dom-mediasessionaction-enterpictureinpicture">Media Session: 4.4 Actions</see>
    /// </remarks>
    [Description("@#enterpictureinpicture")]
    Enterpictureinpicture = 15,

    /// <summary>
    /// voiceactivity: the action&apos;s intent is to notify the web page that voice activity has been detected by the microphone.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediasession/#dom-mediasessionaction-voiceactivity">Media Session: 4.4 Actions</see>
    /// </remarks>
    [Description("@#voiceactivity")]
    Voiceactivity = 16
}

/// <summary>
/// <see href="https://w3c.github.io/mediasession/#enumdef-mediasessionenterpictureinpicturereason">Media Session: 5 The MediaSession interface</see>
/// </summary>
[Description("@#MediaSessionEnterPictureInPictureReason")]
[ECMAScript]
[String]
public enum MediaSessionEnterPictureInPictureReason
{
    /// <summary>
    /// other: the reason for entering picture-in-picture is not one of the existing enum values
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediasession/#dom-mediasessionenterpictureinpicturereason-other">Media Session: 10. The MediaSessionActionDetails dictionary</see>
    /// </remarks>
    [Description("@#other")]
    Other = 0,

    /// <summary>
    /// useraction: the user has taken an explicit action to enter picture-in-picture (e.g. clicking a picture-in-picture button in the user agent UI)
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediasession/#dom-mediasessionenterpictureinpicturereason-useraction">Media Session: 10. The MediaSessionActionDetails dictionary</see>
    /// </remarks>
    [Description("@#useraction")]
    Useraction = 1,

    /// <summary>
    /// contentoccluded: the user agent is requesting picture-in-picture because the page has become occluded. This can happen in various cases like tab switching or tab minimization.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediasession/#dom-mediasessionenterpictureinpicturereason-contentoccluded">Media Session: 10. The MediaSessionActionDetails dictionary</see>
    /// </remarks>
    [Description("@#contentoccluded")]
    Contentoccluded = 2
}

/// <summary>
/// <see href="https://w3c.github.io/mediasession/#enumdef-mediasessionplaybackstate">Media Session: 5 The MediaSession interface</see>
/// </summary>
[Description("@#MediaSessionPlaybackState")]
[ECMAScript]
[String]
public enum MediaSessionPlaybackState
{
    /// <summary>
    /// The playbackState attribute represents the declared playback state of the media session, by which the session declares whether its /browsing context is playing media or not. The initial value is none. On setting, the user agent MUST set the IDL attribute to the new value if it is a valid MediaSessionPlaybackState value. On getting, the user agent MUST return the last valid value that was set. The playbackState attribute is a hint for the user agent to determine whether the /browsing context is playing or paused.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediasession/#dom-mediasessionplaybackstate-none">Media Session: 5 The MediaSession interface</see>
    /// </remarks>
    [Description("@#none")]
    None = 0,

    /// <summary>
    /// In order to make play and pause actions work properly, the user agent SHOULD be able to determine if a /browsing context of the active media session is playing media or not, which is called the guessed playback state. The RECOMMENDED way for determining the guessed playback state is to monitor the media elements whose node document&apos;s Document/browsing context is the /browsing context. The /browsing context&apos;s guessed playback state is &quot;playing&quot; if any of them is media element/potentially playing and not media element/muted, and is &quot;paused&quot; otherwise. Other information SHOULD also be considered, such as WebAudio and plugins.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediasession/#dom-mediasessionplaybackstate-paused">Media Session: 5 The MediaSession interface</see>
    /// </remarks>
    [Description("@#paused")]
    Paused = 1,

    /// <summary>
    /// In order to make play and pause actions work properly, the user agent SHOULD be able to determine if a /browsing context of the active media session is playing media or not, which is called the guessed playback state. The RECOMMENDED way for determining the guessed playback state is to monitor the media elements whose node document&apos;s Document/browsing context is the /browsing context. The /browsing context&apos;s guessed playback state is &quot;playing&quot; if any of them is media element/potentially playing and not media element/muted, and is &quot;paused&quot; otherwise. Other information SHOULD also be considered, such as WebAudio and plugins.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediasession/#dom-mediasessionplaybackstate-playing">Media Session: 5 The MediaSession interface</see>
    /// </remarks>
    [Description("@#playing")]
    Playing = 2
}

/// <summary>
/// <see href="https://w3c.github.io/navigation-timing/#enumdef-navigationtimingtype">Navigation Timing Level 2: 3.3.1 The NavigationTimingType enum</see>
/// </summary>
[Description("@#NavigationTimingType")]
[ECMAScript]
[String]
public enum NavigationTimingType
{
    /// <summary>
    /// Client-side redirects, such as those using the Refresh pragma directive|Refresh pragma directive, are not considered redirect status|HTTP redirects by this spec. In those cases, the type attribute SHOULD return appropriate value, such as reload if reloading the current page, or navigate if navigating to a new URL.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/navigation-timing/#dom-navigationtimingtype-navigate">Navigation Timing Level 2: 3.3.1 The NavigationTimingType enum</see>
    /// </remarks>
    [Description("@#navigate")]
    Navigate = 0,

    /// <summary>
    /// Client-side redirects, such as those using the Refresh pragma directive|Refresh pragma directive, are not considered redirect status|HTTP redirects by this spec. In those cases, the type attribute SHOULD return appropriate value, such as reload if reloading the current page, or navigate if navigating to a new URL.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/navigation-timing/#dom-navigationtimingtype-reload">Navigation Timing Level 2: 3.3.1 The NavigationTimingType enum</see>
    /// </remarks>
    [Description("@#reload")]
    Reload = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/navigation-timing/#dom-navigationtimingtype-back_forward">Navigation Timing Level 2: 3.3.1 The NavigationTimingType enum</see>
    /// </summary>
    [Description("@#back_forward")]
    BackForward = 2
}

/// <summary>
/// <see href="https://w3c.github.io/orientation-sensor/#enumdef-orientationsensorlocalcoordinatesystem">Orientation Sensor: 6.1 The OrientationSensor Interface</see>
/// </summary>
[Description("@#OrientationSensorLocalCoordinateSystem")]
[ECMAScript]
[String]
public enum OrientationSensorLocalCoordinateSystem
{
    /// <summary>
    /// <see href="https://w3c.github.io/orientation-sensor/#dom-orientationsensorlocalcoordinatesystem-device">Orientation Sensor: 6.1 The OrientationSensor Interface</see>
    /// </summary>
    [Description("@#device")]
    Device = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/orientation-sensor/#dom-orientationsensorlocalcoordinatesystem-screen">Orientation Sensor: 6.1 The OrientationSensor Interface</see>
    /// </summary>
    [Description("@#screen")]
    Screen = 1
}

/// <summary>
/// <see href="https://w3c.github.io/payment-request/#dom-paymentcomplete">Payment Request API: 12. PaymentComplete enum</see>
/// </summary>
[Description("@#PaymentComplete")]
[ECMAScript]
[String]
public enum PaymentComplete
{
    /// <summary>
    /// <see href="https://w3c.github.io/payment-request/#dom-paymentcomplete-fail">Payment Request API: 12. PaymentComplete enum</see>
    /// </summary>
    [Description("@#fail")]
    Fail = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/payment-request/#dom-paymentcomplete-success">Payment Request API: 12. PaymentComplete enum</see>
    /// </summary>
    [Description("@#success")]
    Success = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/payment-request/#dom-paymentcomplete-unknown">Payment Request API: 12. PaymentComplete enum</see>
    /// </summary>
    [Description("@#unknown")]
    Unknown = 2
}

/// <summary>
/// <see href="https://w3c.github.io/presentation-api/#dom-presentationconnectionclosereason">Presentation API: 6.5.4 Interface PresentationConnectionCloseEvent</see>
/// </summary>
[Description("@#PresentationConnectionCloseReason")]
[ECMAScript]
[String]
public enum PresentationConnectionCloseReason
{
    /// <summary>
    /// If the next step fails, abort all remaining steps and close the presentation connection S with error as closeReason, and a human readable message describing the failure as closeMessage.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/presentation-api/#dom-presentationconnectionclosereason-error">Presentation API: 6.5.4 Interface PresentationConnectionCloseEvent</see>
    /// </remarks>
    [Description("@#error")]
    Error = 0,

    /// <summary>
    /// When the close method is called on a PresentationConnection S, the user agent MUST start closing the presentation connection S with closed as closeReason and an empty message as closeMessage.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/presentation-api/#dom-presentationconnectionclosereason-closed">Presentation API: 6.5.4 Interface PresentationConnectionCloseEvent</see>
    /// </remarks>
    [Description("@#closed")]
    Closed = 1,

    /// <summary>
    /// When a PresentationConnection object S is discarded (because the document owning it is navigating or is closed) while the presentation connection state of S is connecting or connected, the user agent MUST start closing the presentation connection S with wentaway as closeReason and an empty closeMessage.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/presentation-api/#dom-presentationconnectionclosereason-wentaway">Presentation API: 6.5.4 Interface PresentationConnectionCloseEvent</see>
    /// </remarks>
    [Description("@#wentaway")]
    Wentaway = 2
}

/// <summary>
/// <see href="https://w3c.github.io/presentation-api/#dom-presentationconnectionstate">Presentation API: 6.5 Interface PresentationConnection</see>
/// </summary>
[Description("@#PresentationConnectionState")]
[ECMAScript]
[String]
public enum PresentationConnectionState
{
    /// <summary>
    /// Set the presentation connection state of S to connecting.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/presentation-api/#dom-presentationconnectionstate-connecting">Presentation API: 6.5 Interface PresentationConnection</see>
    /// </remarks>
    [Description("@#connecting")]
    Connecting = 0,

    /// <summary>
    /// Since the the controlling page may connect to and disconnect from multiple presentations during its lifetime, it&apos;s helpful to keep track of the current PresentationConnection and its state. Messages can only be sent and received on connections in a connected state.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/presentation-api/#dom-presentationconnectionstate-connected">Presentation API: 6.5 Interface PresentationConnection</see>
    /// </remarks>
    [Description("@#connected")]
    Connected = 1,

    /// <summary>
    /// When a PresentationConnection object S is discarded (because the document owning it is navigating or is closed) while the presentation connection state of S is connecting or connected, the user agent MUST start closing the presentation connection S with wentaway as closeReason and an empty closeMessage.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/presentation-api/#dom-presentationconnectionstate-closed">Presentation API: 6.5 Interface PresentationConnection</see>
    /// </remarks>
    [Description("@#closed")]
    Closed = 2,

    /// <summary>
    /// Its presentation connection state is not terminated
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/presentation-api/#dom-presentationconnectionstate-terminated">Presentation API: 6.5 Interface PresentationConnection</see>
    /// </remarks>
    [Description("@#terminated")]
    Terminated = 3
}

/// <summary>
/// <see href="https://w3c.github.io/secure-payment-confirmation/#enumdef-securepaymentconfirmationavailability">Secure Payment Confirmation: 4.6 Checking if Secure Payment Confirmation is available</see>
/// </summary>
[Description("@#SecurePaymentConfirmationAvailability")]
[ECMAScript]
[String]
public enum SecurePaymentConfirmationAvailability
{
    /// <summary>
    /// <see href="https://w3c.github.io/secure-payment-confirmation/#dom-securepaymentconfirmationavailability-available">Secure Payment Confirmation: 4.6 Checking if Secure Payment Confirmation is available</see>
    /// </summary>
    [Description("@#available")]
    Available = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/secure-payment-confirmation/#dom-securepaymentconfirmationavailability-unavailable-unknown-reason">Secure Payment Confirmation: 4.6 Checking if Secure Payment Confirmation is available</see>
    /// </summary>
    [Description("@#unavailable-unknown-reason")]
    UnavailableUnknownReason = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/secure-payment-confirmation/#dom-securepaymentconfirmationavailability-unavailable-feature-not-enabled">Secure Payment Confirmation: 4.6 Checking if Secure Payment Confirmation is available</see>
    /// </summary>
    [Description("@#unavailable-feature-not-enabled")]
    UnavailableFeatureNotEnabled = 2,

    /// <summary>
    /// <see href="https://w3c.github.io/secure-payment-confirmation/#dom-securepaymentconfirmationavailability-unavailable-no-permission-policy">Secure Payment Confirmation: 4.6 Checking if Secure Payment Confirmation is available</see>
    /// </summary>
    [Description("@#unavailable-no-permission-policy")]
    UnavailableNoPermissionPolicy = 3,

    /// <summary>
    /// <see href="https://w3c.github.io/secure-payment-confirmation/#dom-securepaymentconfirmationavailability-unavailable-no-user-verifying-platform-authenticator">Secure Payment Confirmation: 4.6 Checking if Secure Payment Confirmation is available</see>
    /// </summary>
    [Description("@#unavailable-no-user-verifying-platform-authenticator")]
    UnavailableNoUserVerifyingPlatformAuthenticator = 4
}

/// <summary>
/// <see href="https://w3c.github.io/secure-payment-confirmation/#enumdef-securepaymentconfirmationcapability">Secure Payment Confirmation: 4.7.1 SecurePaymentConfirmationCapability Enumeration</see>
/// </summary>
[Description("@#SecurePaymentConfirmationCapability")]
[ECMAScript]
[String]
public enum SecurePaymentConfirmationCapability
{
    /// <summary>
    /// <see href="https://w3c.github.io/secure-payment-confirmation/#dom-securepaymentconfirmationcapability-browserboundkeyhardware">Secure Payment Confirmation: 4.7.1 SecurePaymentConfirmationCapability Enumeration</see>
    /// </summary>
    [Description("@#browserBoundKeyHardware")]
    BrowserBoundKeyHardware = 0
}

/// <summary>
/// <see href="https://w3c.github.io/web-locks/#enumdef-lockmode">Web Locks API: 3.2 LockManager class</see>
/// </summary>
[Description("@#LockMode")]
[ECMAScript]
[String]
public enum LockMode
{
    /// <summary>
    /// <see href="https://w3c.github.io/web-locks/#dom-lockmode-shared">Web Locks API: 3.2 LockManager class</see>
    /// </summary>
    [Description("@#shared")]
    Shared = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/web-locks/#dom-lockmode-exclusive">Web Locks API: 3.2 LockManager class</see>
    /// </summary>
    [Description("@#exclusive")]
    Exclusive = 1
}

/// <summary>
/// <see href="https://w3c.github.io/webappsec-credential-management/#enumdef-credentialmediationrequirement">Credential Management Level 1: 2.3.2 Mediation Requirements</see>
/// </summary>
[Description("@#CredentialMediationRequirement")]
[ECMAScript]
[String]
public enum CredentialMediationRequirement
{
    /// <summary>
    /// <see href="https://w3c.github.io/webappsec-credential-management/#dom-credentialmediationrequirement-silent">Credential Management Level 1: 2.3.2 Mediation Requirements</see>
    /// </summary>
    [Description("@#silent")]
    Silent = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webappsec-credential-management/#dom-credentialmediationrequirement-optional">Credential Management Level 1: 2.3.2 Mediation Requirements</see>
    /// </summary>
    [Description("@#optional")]
    Optional = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/webappsec-credential-management/#dom-credentialmediationrequirement-conditional">Credential Management Level 1: 2.3.2 Mediation Requirements</see>
    /// </summary>
    [Description("@#conditional")]
    Conditional = 2,

    /// <summary>
    /// <see href="https://w3c.github.io/webappsec-credential-management/#dom-credentialmediationrequirement-required">Credential Management Level 1: 2.3.2 Mediation Requirements</see>
    /// </summary>
    [Description("@#required")]
    Required = 3
}

/// <summary>
/// <see href="https://w3c.github.io/webappsec-credential-management/#enumdef-credentialuimode">Credential Management Level 1: 2.3.3 UI Mode</see>
/// </summary>
[Description("@#CredentialUiMode")]
[ECMAScript]
[String]
public enum CredentialUiMode
{
    /// <summary>
    /// <see href="https://w3c.github.io/webappsec-credential-management/#dom-credentialuimode-immediate">Credential Management Level 1: 2.3.3 UI Mode</see>
    /// </summary>
    [Description("@#immediate")]
    Immediate = 0
}

/// <summary>
/// <see href="https://w3c.github.io/webappsec-csp/#enumdef-securitypolicyviolationeventdisposition">Content Security Policy Level 3: 5.1 Violation DOM Events</see>
/// </summary>
[Description("@#SecurityPolicyViolationEventDisposition")]
[ECMAScript]
[String]
public enum SecurityPolicyViolationEventDisposition
{
    /// <summary>
    /// <see href="https://w3c.github.io/webappsec-csp/#dom-securitypolicyviolationeventdisposition-enforce">Content Security Policy Level 3: 5.1 Violation DOM Events</see>
    /// </summary>
    [Description("@#enforce")]
    Enforce = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webappsec-csp/#dom-securitypolicyviolationeventdisposition-report">Content Security Policy Level 3: 5.1 Violation DOM Events</see>
    /// </summary>
    [Description("@#report")]
    Report = 1
}

/// <summary>
/// <see href="https://w3c.github.io/webappsec-referrer-policy/#enumdef-referrerpolicy">Referrer Policy: 3 Referrer Policies</see>
/// </summary>
[Description("@#ReferrerPolicy")]
[ECMAScript]
[String]
public enum ReferrerPolicy
{
    [Description("@#")]
    Empty = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webappsec-referrer-policy/#dom-referrerpolicy-no-referrer">Referrer Policy: 3 Referrer Policies</see>
    /// </summary>
    [Description("@#no-referrer")]
    NoReferrer = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/webappsec-referrer-policy/#dom-referrerpolicy-no-referrer-when-downgrade">Referrer Policy: 3 Referrer Policies</see>
    /// </summary>
    [Description("@#no-referrer-when-downgrade")]
    NoReferrerWhenDowngrade = 2,

    /// <summary>
    /// <see href="https://w3c.github.io/webappsec-referrer-policy/#dom-referrerpolicy-same-origin">Referrer Policy: 3 Referrer Policies</see>
    /// </summary>
    [Description("@#same-origin")]
    SameOrigin = 3,

    /// <summary>
    /// <see href="https://w3c.github.io/webappsec-referrer-policy/#dom-referrerpolicy-origin">Referrer Policy: 3 Referrer Policies</see>
    /// </summary>
    [Description("@#origin")]
    Origin = 4,

    /// <summary>
    /// <see href="https://w3c.github.io/webappsec-referrer-policy/#dom-referrerpolicy-strict-origin">Referrer Policy: 3 Referrer Policies</see>
    /// </summary>
    [Description("@#strict-origin")]
    StrictOrigin = 5,

    /// <summary>
    /// <see href="https://w3c.github.io/webappsec-referrer-policy/#dom-referrerpolicy-origin-when-cross-origin">Referrer Policy: 3 Referrer Policies</see>
    /// </summary>
    [Description("@#origin-when-cross-origin")]
    OriginWhenCrossOrigin = 6,

    /// <summary>
    /// <see href="https://w3c.github.io/webappsec-referrer-policy/#dom-referrerpolicy-strict-origin-when-cross-origin">Referrer Policy: 3 Referrer Policies</see>
    /// </summary>
    [Description("@#strict-origin-when-cross-origin")]
    StrictOriginWhenCrossOrigin = 7,

    /// <summary>
    /// <see href="https://w3c.github.io/webappsec-referrer-policy/#dom-referrerpolicy-unsafe-url">Referrer Policy: 3 Referrer Policies</see>
    /// </summary>
    [Description("@#unsafe-url")]
    UnsafeUrl = 8
}

/// <summary>
/// <see href="https://w3c.github.io/webauthn/#enumdef-attestationconveyancepreference">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.4.7 Attestation Conveyance Preference Enumeration (enum AttestationConveyancePreference)</see>
/// </summary>
[Description("@#AttestationConveyancePreference")]
[ECMAScript]
[String]
public enum AttestationConveyancePreference
{
    /// <summary>
    /// <see href="https://w3c.github.io/webauthn/#dom-attestationconveyancepreference-none">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.4.7 Attestation Conveyance Preference Enumeration (enum AttestationConveyancePreference)</see>
    /// </summary>
    [Description("@#none")]
    None = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webauthn/#dom-attestationconveyancepreference-indirect">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.4.7 Attestation Conveyance Preference Enumeration (enum AttestationConveyancePreference)</see>
    /// </summary>
    [Description("@#indirect")]
    Indirect = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/webauthn/#dom-attestationconveyancepreference-direct">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.4.7 Attestation Conveyance Preference Enumeration (enum AttestationConveyancePreference)</see>
    /// </summary>
    [Description("@#direct")]
    Direct = 2,

    /// <summary>
    /// <see href="https://w3c.github.io/webauthn/#dom-attestationconveyancepreference-enterprise">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.4.7 Attestation Conveyance Preference Enumeration (enum AttestationConveyancePreference)</see>
    /// </summary>
    [Description("@#enterprise")]
    Enterprise = 3
}

/// <summary>
/// <see href="https://w3c.github.io/webauthn/#enumdef-authenticatorattachment">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.4.5 Authenticator Attachment Enumeration (enum AuthenticatorAttachment)</see>
/// </summary>
[Description("@#AuthenticatorAttachment")]
[ECMAScript]
[String]
public enum AuthenticatorAttachment
{
    /// <summary>
    /// <see href="https://w3c.github.io/webauthn/#dom-authenticatorattachment-platform">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.4.5 Authenticator Attachment Enumeration (enum AuthenticatorAttachment)</see>
    /// </summary>
    [Description("@#platform")]
    Platform = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webauthn/#dom-authenticatorattachment-cross-platform">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.4.5 Authenticator Attachment Enumeration (enum AuthenticatorAttachment)</see>
    /// </summary>
    [Description("@#cross-platform")]
    CrossPlatform = 1
}

/// <summary>
/// <see href="https://w3c.github.io/webauthn/#enumdef-authenticatortransport">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.8.4 Authenticator Transport Enumeration (enum AuthenticatorTransport)</see>
/// </summary>
[Description("@#AuthenticatorTransport")]
[ECMAScript]
[String]
public enum AuthenticatorTransport
{
    /// <summary>
    /// <see href="https://w3c.github.io/webauthn/#dom-authenticatortransport-usb">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.8.4 Authenticator Transport Enumeration (enum AuthenticatorTransport)</see>
    /// </summary>
    [Description("@#usb")]
    Usb = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webauthn/#dom-authenticatortransport-nfc">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.8.4 Authenticator Transport Enumeration (enum AuthenticatorTransport)</see>
    /// </summary>
    [Description("@#nfc")]
    Nfc = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/webauthn/#dom-authenticatortransport-ble">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.8.4 Authenticator Transport Enumeration (enum AuthenticatorTransport)</see>
    /// </summary>
    [Description("@#ble")]
    Ble = 2,

    /// <summary>
    /// <see href="https://w3c.github.io/webauthn/#dom-authenticatortransport-smart-card">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.8.4 Authenticator Transport Enumeration (enum AuthenticatorTransport)</see>
    /// </summary>
    [Description("@#smart-card")]
    SmartCard = 3,

    /// <summary>
    /// <see href="https://w3c.github.io/webauthn/#dom-authenticatortransport-hybrid">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.8.4 Authenticator Transport Enumeration (enum AuthenticatorTransport)</see>
    /// </summary>
    [Description("@#hybrid")]
    Hybrid = 4,

    /// <summary>
    /// <see href="https://w3c.github.io/webauthn/#dom-authenticatortransport-internal">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.8.4 Authenticator Transport Enumeration (enum AuthenticatorTransport)</see>
    /// </summary>
    [Description("@#internal")]
    Internal = 5
}

/// <summary>
/// <see href="https://w3c.github.io/webauthn/#enumdef-clientcapability">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.8.7 Client Capability Enumeration (enum ClientCapability)</see>
/// </summary>
[Description("@#ClientCapability")]
[ECMAScript]
[String]
public enum ClientCapability
{
    /// <summary>
    /// <see href="https://w3c.github.io/webauthn/#dom-clientcapability-conditionalcreate">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.8.7 Client Capability Enumeration (enum ClientCapability)</see>
    /// </summary>
    [Description("@#conditionalCreate")]
    ConditionalCreate = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webauthn/#dom-clientcapability-conditionalget">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.8.7 Client Capability Enumeration (enum ClientCapability)</see>
    /// </summary>
    [Description("@#conditionalGet")]
    ConditionalGet = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/webauthn/#dom-clientcapability-hybridtransport">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.8.7 Client Capability Enumeration (enum ClientCapability)</see>
    /// </summary>
    [Description("@#hybridTransport")]
    HybridTransport = 2,

    /// <summary>
    /// <see href="https://w3c.github.io/webauthn/#dom-clientcapability-passkeyplatformauthenticator">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.8.7 Client Capability Enumeration (enum ClientCapability)</see>
    /// </summary>
    [Description("@#passkeyPlatformAuthenticator")]
    PasskeyPlatformAuthenticator = 3,

    /// <summary>
    /// <see href="https://w3c.github.io/webauthn/#dom-clientcapability-userverifyingplatformauthenticator">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.8.7 Client Capability Enumeration (enum ClientCapability)</see>
    /// </summary>
    [Description("@#userVerifyingPlatformAuthenticator")]
    UserVerifyingPlatformAuthenticator = 4,

    /// <summary>
    /// <see href="https://w3c.github.io/webauthn/#dom-clientcapability-relatedorigins">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.8.7 Client Capability Enumeration (enum ClientCapability)</see>
    /// </summary>
    [Description("@#relatedOrigins")]
    RelatedOrigins = 5,

    /// <summary>
    /// <see href="https://w3c.github.io/webauthn/#dom-clientcapability-signalallacceptedcredentials">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.8.7 Client Capability Enumeration (enum ClientCapability)</see>
    /// </summary>
    [Description("@#signalAllAcceptedCredentials")]
    SignalAllAcceptedCredentials = 6,

    /// <summary>
    /// <see href="https://w3c.github.io/webauthn/#dom-clientcapability-signalcurrentuserdetails">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.8.7 Client Capability Enumeration (enum ClientCapability)</see>
    /// </summary>
    [Description("@#signalCurrentUserDetails")]
    SignalCurrentUserDetails = 7,

    /// <summary>
    /// <see href="https://w3c.github.io/webauthn/#dom-clientcapability-signalunknowncredential">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.8.7 Client Capability Enumeration (enum ClientCapability)</see>
    /// </summary>
    [Description("@#signalUnknownCredential")]
    SignalUnknownCredential = 8
}

/// <summary>
/// <see href="https://w3c.github.io/webauthn/#enumdef-largeblobsupport">Web Authentication: An API for accessing Public Key Credentials - Level 3: 10.1.5 Large blob storage extension (largeBlob)</see>
/// </summary>
[Description("@#LargeBlobSupport")]
[ECMAScript]
[String]
public enum LargeBlobSupport
{
    /// <summary>
    /// <see href="https://w3c.github.io/webauthn/#dom-largeblobsupport-required">Web Authentication: An API for accessing Public Key Credentials - Level 3: 10.1.5 Large blob storage extension (largeBlob)</see>
    /// </summary>
    [Description("@#required")]
    Required = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webauthn/#dom-largeblobsupport-preferred">Web Authentication: An API for accessing Public Key Credentials - Level 3: 10.1.5 Large blob storage extension (largeBlob)</see>
    /// </summary>
    [Description("@#preferred")]
    Preferred = 1
}

/// <summary>
/// <see href="https://w3c.github.io/webauthn/#enumdef-publickeycredentialhint">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.8.8 User-agent Hints Enumeration (enum PublicKeyCredentialHint)</see>
/// </summary>
[Description("@#PublicKeyCredentialHint")]
[ECMAScript]
[String]
public enum PublicKeyCredentialHint
{
    /// <summary>
    /// <see href="https://w3c.github.io/webauthn/#dom-publickeycredentialhint-security-key">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.8.8 User-agent Hints Enumeration (enum PublicKeyCredentialHint)</see>
    /// </summary>
    [Description("@#security-key")]
    SecurityKey = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webauthn/#dom-publickeycredentialhint-client-device">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.8.8 User-agent Hints Enumeration (enum PublicKeyCredentialHint)</see>
    /// </summary>
    [Description("@#client-device")]
    ClientDevice = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/webauthn/#dom-publickeycredentialhint-hybrid">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.8.8 User-agent Hints Enumeration (enum PublicKeyCredentialHint)</see>
    /// </summary>
    [Description("@#hybrid")]
    Hybrid = 2
}

/// <summary>
/// <see href="https://w3c.github.io/webauthn/#enumdef-publickeycredentialtype">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.8.2 Credential Type Enumeration (enum PublicKeyCredentialType)</see>
/// </summary>
[Description("@#PublicKeyCredentialType")]
[ECMAScript]
[String]
public enum PublicKeyCredentialType
{
    /// <summary>
    /// <see href="https://w3c.github.io/webauthn/#dom-publickeycredentialtype-public-key">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.8.2 Credential Type Enumeration (enum PublicKeyCredentialType)</see>
    /// </summary>
    [Description("@#public-key")]
    PublicKey = 0
}

/// <summary>
/// <see href="https://w3c.github.io/webauthn/#enumdef-residentkeyrequirement">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.4.6 Resident Key Requirement Enumeration (enum ResidentKeyRequirement)</see>
/// </summary>
[Description("@#ResidentKeyRequirement")]
[ECMAScript]
[String]
public enum ResidentKeyRequirement
{
    /// <summary>
    /// <see href="https://w3c.github.io/webauthn/#dom-residentkeyrequirement-discouraged">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.4.6 Resident Key Requirement Enumeration (enum ResidentKeyRequirement)</see>
    /// </summary>
    [Description("@#discouraged")]
    Discouraged = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webauthn/#dom-residentkeyrequirement-preferred">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.4.6 Resident Key Requirement Enumeration (enum ResidentKeyRequirement)</see>
    /// </summary>
    [Description("@#preferred")]
    Preferred = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/webauthn/#dom-residentkeyrequirement-required">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.4.6 Resident Key Requirement Enumeration (enum ResidentKeyRequirement)</see>
    /// </summary>
    [Description("@#required")]
    Required = 2
}

/// <summary>
/// <see href="https://w3c.github.io/webauthn/#enumdef-tokenbindingstatus">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.8.1 Client Data Used in WebAuthn Signatures (dictionary CollectedClientData)</see>
/// </summary>
[Description("@#TokenBindingStatus")]
[ECMAScript]
[String]
public enum TokenBindingStatus
{
    /// <summary>
    /// <see href="https://w3c.github.io/webauthn/#dom-tokenbindingstatus-present">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.8.1 Client Data Used in WebAuthn Signatures (dictionary CollectedClientData)</see>
    /// </summary>
    [Description("@#present")]
    Present = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webauthn/#dom-tokenbindingstatus-supported">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.8.1 Client Data Used in WebAuthn Signatures (dictionary CollectedClientData)</see>
    /// </summary>
    [Description("@#supported")]
    Supported = 1
}

/// <summary>
/// <see href="https://w3c.github.io/webauthn/#enumdef-userverificationrequirement">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.8.6 User Verification Requirement Enumeration (enum UserVerificationRequirement)</see>
/// </summary>
[Description("@#UserVerificationRequirement")]
[ECMAScript]
[String]
public enum UserVerificationRequirement
{
    /// <summary>
    /// <see href="https://w3c.github.io/webauthn/#dom-userverificationrequirement-required">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.8.6 User Verification Requirement Enumeration (enum UserVerificationRequirement)</see>
    /// </summary>
    [Description("@#required")]
    Required = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webauthn/#dom-userverificationrequirement-preferred">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.8.6 User Verification Requirement Enumeration (enum UserVerificationRequirement)</see>
    /// </summary>
    [Description("@#preferred")]
    Preferred = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/webauthn/#dom-userverificationrequirement-discouraged">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.8.6 User Verification Requirement Enumeration (enum UserVerificationRequirement)</see>
    /// </summary>
    [Description("@#discouraged")]
    Discouraged = 2
}

/// <summary>
/// <see href="https://w3c.github.io/webcodecs/#enumdef-alphaoption">WebCodecs: 7.10 Alpha Option</see>
/// </summary>
[Description("@#AlphaOption")]
[ECMAScript]
[String]
public enum AlphaOption
{
    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/#dom-alphaoption-keep">WebCodecs: 7.10 Alpha Option</see>
    /// </summary>
    [Description("@#keep")]
    Keep = 0,

    /// <summary>
    /// Whether the alpha component of the VideoFrame inputs SHOULD be kept or discarded prior to encoding. If alpha is equal to discard, alpha data is always discarded, regardless of a VideoFrame&apos;s format.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-alphaoption-discard">WebCodecs: 7.10 Alpha Option</see>
    /// </remarks>
    [Description("@#discard")]
    Discard = 1
}

/// <summary>
/// <see href="https://w3c.github.io/webcodecs/#enumdef-audiosampleformat">WebCodecs: 9.3 Audio Sample Format</see>
/// </summary>
[Description("@#AudioSampleFormat")]
[ECMAScript]
[String]
public enum AudioSampleFormat
{
    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/#dom-audiosampleformat-u8">WebCodecs: 9.3 Audio Sample Format</see>
    /// </summary>
    [Description("@#u8")]
    U8 = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/#dom-audiosampleformat-s16">WebCodecs: 9.3 Audio Sample Format</see>
    /// </summary>
    [Description("@#s16")]
    S16 = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/#dom-audiosampleformat-s32">WebCodecs: 9.3 Audio Sample Format</see>
    /// </summary>
    [Description("@#s32")]
    S32 = 2,

    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/#dom-audiosampleformat-f32">WebCodecs: 9.3 Audio Sample Format</see>
    /// </summary>
    [Description("@#f32")]
    F32 = 3,

    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/#dom-audiosampleformat-u8-planar">WebCodecs: 9.3 Audio Sample Format</see>
    /// </summary>
    [Description("@#u8-planar")]
    U8Planar = 4,

    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/#dom-audiosampleformat-s16-planar">WebCodecs: 9.3 Audio Sample Format</see>
    /// </summary>
    [Description("@#s16-planar")]
    S16Planar = 5,

    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/#dom-audiosampleformat-s32-planar">WebCodecs: 9.3 Audio Sample Format</see>
    /// </summary>
    [Description("@#s32-planar")]
    S32Planar = 6,

    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/#dom-audiosampleformat-f32-planar">WebCodecs: 9.3 Audio Sample Format</see>
    /// </summary>
    [Description("@#f32-planar")]
    F32Planar = 7
}

/// <summary>
/// <see href="https://w3c.github.io/webcodecs/#enumdef-codecstate">WebCodecs: 7.15 CodecState</see>
/// </summary>
[Description("@#CodecState")]
[ECMAScript]
[String]
public enum CodecState
{
    /// <summary>
    /// unconfigured
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-codecstate-unconfigured">WebCodecs: 7.15 CodecState</see>
    /// </remarks>
    [Description("@#unconfigured")]
    Unconfigured = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/#dom-codecstate-configured">WebCodecs: 7.15 CodecState</see>
    /// </summary>
    [Description("@#configured")]
    Configured = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/#dom-codecstate-closed">WebCodecs: 7.15 CodecState</see>
    /// </summary>
    [Description("@#closed")]
    Closed = 2
}

/// <summary>
/// <see href="https://w3c.github.io/webcodecs/#enumdef-videocolorprimaries">WebCodecs: 9.10 Video Color Primaries</see>
/// </summary>
[Description("@#VideoColorPrimaries")]
[ECMAScript]
[String]
public enum VideoColorPrimaries
{
    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videocolorprimaries-bt709">WebCodecs: 9.10 Video Color Primaries</see>
    /// </summary>
    [Description("@#bt709")]
    Bt709 = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videocolorprimaries-bt470bg">WebCodecs: 9.10 Video Color Primaries</see>
    /// </summary>
    [Description("@#bt470bg")]
    Bt470bg = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videocolorprimaries-smpte170m">WebCodecs: 9.10 Video Color Primaries</see>
    /// </summary>
    [Description("@#smpte170m")]
    Smpte170m = 2,

    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videocolorprimaries-bt2020">WebCodecs: 9.10 Video Color Primaries</see>
    /// </summary>
    [Description("@#bt2020")]
    Bt2020 = 3,

    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videocolorprimaries-smpte432">WebCodecs: 9.10 Video Color Primaries</see>
    /// </summary>
    [Description("@#smpte432")]
    Smpte432 = 4
}

/// <summary>
/// <see href="https://w3c.github.io/webcodecs/#enumdef-videomatrixcoefficients">WebCodecs: 9.12 Video Matrix Coefficients</see>
/// </summary>
[Description("@#VideoMatrixCoefficients")]
[ECMAScript]
[String]
public enum VideoMatrixCoefficients
{
    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videomatrixcoefficients-rgb">WebCodecs: 9.12 Video Matrix Coefficients</see>
    /// </summary>
    [Description("@#rgb")]
    Rgb = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videomatrixcoefficients-bt709">WebCodecs: 9.12 Video Matrix Coefficients</see>
    /// </summary>
    [Description("@#bt709")]
    Bt709 = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videomatrixcoefficients-bt470bg">WebCodecs: 9.12 Video Matrix Coefficients</see>
    /// </summary>
    [Description("@#bt470bg")]
    Bt470bg = 2,

    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videomatrixcoefficients-smpte170m">WebCodecs: 9.12 Video Matrix Coefficients</see>
    /// </summary>
    [Description("@#smpte170m")]
    Smpte170m = 3,

    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videomatrixcoefficients-bt2020-ncl">WebCodecs: 9.12 Video Matrix Coefficients</see>
    /// </summary>
    [Description("@#bt2020-ncl")]
    Bt2020Ncl = 4
}

/// <summary>
/// <see href="https://w3c.github.io/webcodecs/#enumdef-videopixelformat">WebCodecs: 9.8 Pixel Format</see>
/// </summary>
[Description("@#VideoPixelFormat")]
[ECMAScript]
[String]
public enum VideoPixelFormat
{
    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videopixelformat-i420">WebCodecs: 9.8 Pixel Format</see>
    /// </summary>
    [Description("@#I420")]
    I420 = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videopixelformat-i420p10">WebCodecs: 9.8 Pixel Format</see>
    /// </summary>
    [Description("@#I420P10")]
    I420P10 = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videopixelformat-i420p12">WebCodecs: 9.8 Pixel Format</see>
    /// </summary>
    [Description("@#I420P12")]
    I420P12 = 2,

    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videopixelformat-i420a">WebCodecs: 9.8 Pixel Format</see>
    /// </summary>
    [Description("@#I420A")]
    I420A = 3,

    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videopixelformat-i420ap10">WebCodecs: 9.8 Pixel Format</see>
    /// </summary>
    [Description("@#I420AP10")]
    I420AP10 = 4,

    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videopixelformat-i420ap12">WebCodecs: 9.8 Pixel Format</see>
    /// </summary>
    [Description("@#I420AP12")]
    I420AP12 = 5,

    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videopixelformat-i422">WebCodecs: 9.8 Pixel Format</see>
    /// </summary>
    [Description("@#I422")]
    I422 = 6,

    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videopixelformat-i422p10">WebCodecs: 9.8 Pixel Format</see>
    /// </summary>
    [Description("@#I422P10")]
    I422P10 = 7,

    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videopixelformat-i422p12">WebCodecs: 9.8 Pixel Format</see>
    /// </summary>
    [Description("@#I422P12")]
    I422P12 = 8,

    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videopixelformat-i422a">WebCodecs: 9.8 Pixel Format</see>
    /// </summary>
    [Description("@#I422A")]
    I422A = 9,

    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videopixelformat-i422ap10">WebCodecs: 9.8 Pixel Format</see>
    /// </summary>
    [Description("@#I422AP10")]
    I422AP10 = 10,

    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videopixelformat-i422ap12">WebCodecs: 9.8 Pixel Format</see>
    /// </summary>
    [Description("@#I422AP12")]
    I422AP12 = 11,

    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videopixelformat-i444">WebCodecs: 9.8 Pixel Format</see>
    /// </summary>
    [Description("@#I444")]
    I444 = 12,

    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videopixelformat-i444p10">WebCodecs: 9.8 Pixel Format</see>
    /// </summary>
    [Description("@#I444P10")]
    I444P10 = 13,

    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videopixelformat-i444p12">WebCodecs: 9.8 Pixel Format</see>
    /// </summary>
    [Description("@#I444P12")]
    I444P12 = 14,

    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videopixelformat-i444a">WebCodecs: 9.8 Pixel Format</see>
    /// </summary>
    [Description("@#I444A")]
    I444A = 15,

    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videopixelformat-i444ap10">WebCodecs: 9.8 Pixel Format</see>
    /// </summary>
    [Description("@#I444AP10")]
    I444AP10 = 16,

    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videopixelformat-i444ap12">WebCodecs: 9.8 Pixel Format</see>
    /// </summary>
    [Description("@#I444AP12")]
    I444AP12 = 17,

    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videopixelformat-nv12">WebCodecs: 9.8 Pixel Format</see>
    /// </summary>
    [Description("@#NV12")]
    NV12 = 18,

    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videopixelformat-rgba">WebCodecs: 9.8 Pixel Format</see>
    /// </summary>
    [Description("@#RGBA")]
    RGBA = 19,

    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videopixelformat-rgbx">WebCodecs: 9.8 Pixel Format</see>
    /// </summary>
    [Description("@#RGBX")]
    RGBX = 20,

    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videopixelformat-bgra">WebCodecs: 9.8 Pixel Format</see>
    /// </summary>
    [Description("@#BGRA")]
    BGRA = 21,

    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videopixelformat-bgrx">WebCodecs: 9.8 Pixel Format</see>
    /// </summary>
    [Description("@#BGRX")]
    BGRX = 22
}

/// <summary>
/// <see href="https://w3c.github.io/webcodecs/#enumdef-videotransfercharacteristics">WebCodecs: 9.11 Video Transfer Characteristics</see>
/// </summary>
[Description("@#VideoTransferCharacteristics")]
[ECMAScript]
[String]
public enum VideoTransferCharacteristics
{
    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videotransfercharacteristics-bt709">WebCodecs: 9.11 Video Transfer Characteristics</see>
    /// </summary>
    [Description("@#bt709")]
    Bt709 = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videotransfercharacteristics-smpte170m">WebCodecs: 9.11 Video Transfer Characteristics</see>
    /// </summary>
    [Description("@#smpte170m")]
    Smpte170m = 1,

    /// <summary>
    /// iec61966-2-1
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videotransfercharacteristics-iec61966-2-1">WebCodecs: 9.11 Video Transfer Characteristics</see>
    /// </remarks>
    [Description("@#iec61966-2-1")]
    Iec6196621 = 2,

    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videotransfercharacteristics-linear">WebCodecs: 9.11 Video Transfer Characteristics</see>
    /// </summary>
    [Description("@#linear")]
    Linear = 3,

    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videotransfercharacteristics-pq">WebCodecs: 9.11 Video Transfer Characteristics</see>
    /// </summary>
    [Description("@#pq")]
    Pq = 4,

    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videotransfercharacteristics-hlg">WebCodecs: 9.11 Video Transfer Characteristics</see>
    /// </summary>
    [Description("@#hlg")]
    Hlg = 5
}

/// <summary>
/// <see href="https://w3c.github.io/webcrypto/#dom-keytype">Web Cryptography API Level 2: 13.2 Key interface data types</see>
/// </summary>
[Description("@#KeyType")]
[ECMAScript]
[String]
public enum KeyType
{
    /// <summary>
    /// <see href="https://w3c.github.io/webcrypto/#dom-keytype-public">Web Cryptography API Level 2: 13.2 Key interface data types</see>
    /// </summary>
    [Description("@#public")]
    Public = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webcrypto/#dom-keytype-private">Web Cryptography API Level 2: 13.2 Key interface data types</see>
    /// </summary>
    [Description("@#private")]
    Private = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/webcrypto/#dom-keytype-secret">Web Cryptography API Level 2: 13.2 Key interface data types</see>
    /// </summary>
    [Description("@#secret")]
    Secret = 2
}

/// <summary>
/// <see href="https://w3c.github.io/webextensions/specification/#enumdef-executionworld">Web Extensions: 13.2.13 ExecutionWorld enum</see>
/// </summary>
[Description("@#ExecutionWorld")]
[ECMAScript]
[String]
public enum ExecutionWorld
{
    /// <summary>
    /// <see href="https://w3c.github.io/webextensions/specification/#dom-executionworld-isolated">Web Extensions: 13.2.13 ExecutionWorld enum</see>
    /// </summary>
    [Description("@#ISOLATED")]
    ISOLATED = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webextensions/specification/#dom-executionworld-main">Web Extensions: 13.2.13 ExecutionWorld enum</see>
    /// </summary>
    [Description("@#MAIN")]
    MAIN = 1
}

/// <summary>
/// <see href="https://w3c.github.io/webextensions/specification/#enumdef-runat">Web Extensions: 13.2.12 RunAt enum</see>
/// </summary>
[Description("@#RunAt")]
[ECMAScript]
[String]
public enum RunAt
{
    /// <summary>
    /// <see href="https://w3c.github.io/webextensions/specification/#dom-runat-document_start">Web Extensions: 13.2.12 RunAt enum</see>
    /// </summary>
    [Description("@#document_start")]
    DocumentStart = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webextensions/specification/#dom-runat-document_end">Web Extensions: 13.2.12 RunAt enum</see>
    /// </summary>
    [Description("@#document_end")]
    DocumentEnd = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/webextensions/specification/#dom-runat-document_idle">Web Extensions: 13.2.12 RunAt enum</see>
    /// </summary>
    [Description("@#document_idle")]
    DocumentIdle = 2
}

/// <summary>
/// <see href="https://w3c.github.io/webrtc-encoded-transform/#enumdef-rtcrtpscripttransformtype">WebRTC Encoded Transform: 5 RTCRtpScriptTransform interface</see>
/// </summary>
[Description("@#RTCRtpScriptTransformType")]
[ECMAScript]
[String]
public enum RTCRtpScriptTransformType
{
    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-encoded-transform/#dom-rtcrtpscripttransformtype-sframe">WebRTC Encoded Transform: 5 RTCRtpScriptTransform interface</see>
    /// </summary>
    [Description("@#sframe")]
    Sframe = 0
}

/// <summary>
/// <see href="https://w3c.github.io/webrtc-encoded-transform/#enumdef-sframeciphersuite">WebRTC Encoded Transform: 3 SFrame transforms</see>
/// </summary>
[Description("@#SFrameCipherSuite")]
[ECMAScript]
[String]
public enum SFrameCipherSuite
{
    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-encoded-transform/#dom-sframeciphersuite-aes_128_ctr_hmac_sha256_80">WebRTC Encoded Transform: 3 SFrame transforms</see>
    /// </summary>
    [Description("@#AES_128_CTR_HMAC_SHA256_80")]
    AES128CTRHMACSHA25680 = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-encoded-transform/#dom-sframeciphersuite-aes_128_ctr_hmac_sha256_64">WebRTC Encoded Transform: 3 SFrame transforms</see>
    /// </summary>
    [Description("@#AES_128_CTR_HMAC_SHA256_64")]
    AES128CTRHMACSHA25664 = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-encoded-transform/#dom-sframeciphersuite-aes_128_ctr_hmac_sha256_32">WebRTC Encoded Transform: 3 SFrame transforms</see>
    /// </summary>
    [Description("@#AES_128_CTR_HMAC_SHA256_32")]
    AES128CTRHMACSHA25632 = 2,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-encoded-transform/#dom-sframeciphersuite-aes_128_gcm_sha256_128">WebRTC Encoded Transform: 3 SFrame transforms</see>
    /// </summary>
    [Description("@#AES_128_GCM_SHA256_128")]
    AES128GCMSHA256128 = 3,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-encoded-transform/#dom-sframeciphersuite-aes_256_gcm_sha512_128">WebRTC Encoded Transform: 3 SFrame transforms</see>
    /// </summary>
    [Description("@#AES_256_GCM_SHA512_128")]
    AES256GCMSHA512128 = 4,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-encoded-transform/#dom-sframeciphersuite-aes_256_ctr_hmac_sha512_80">WebRTC Encoded Transform: 3 SFrame transforms</see>
    /// </summary>
    [Description("@#AES_256_CTR_HMAC_SHA512_80")]
    AES256CTRHMACSHA51280 = 5,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-encoded-transform/#dom-sframeciphersuite-aes_256_ctr_hmac_sha512_64">WebRTC Encoded Transform: 3 SFrame transforms</see>
    /// </summary>
    [Description("@#AES_256_CTR_HMAC_SHA512_64")]
    AES256CTRHMACSHA51264 = 6,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-encoded-transform/#dom-sframeciphersuite-aes_256_ctr_hmac_sha512_32">WebRTC Encoded Transform: 3 SFrame transforms</see>
    /// </summary>
    [Description("@#AES_256_CTR_HMAC_SHA512_32")]
    AES256CTRHMACSHA51232 = 7
}

/// <summary>
/// <see href="https://w3c.github.io/webrtc-encoded-transform/#enumdef-sframetransformerroreventtype">WebRTC Encoded Transform: 3 SFrame transforms</see>
/// </summary>
[Description("@#SFrameTransformErrorEventType")]
[ECMAScript]
[String]
public enum SFrameTransformErrorEventType
{
    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-encoded-transform/#dom-sframetransformerroreventtype-authentication">WebRTC Encoded Transform: 3 SFrame transforms</see>
    /// </summary>
    [Description("@#authentication")]
    Authentication = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-encoded-transform/#dom-sframetransformerroreventtype-keyid">WebRTC Encoded Transform: 3 SFrame transforms</see>
    /// </summary>
    [Description("@#keyID")]
    KeyID = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-encoded-transform/#dom-sframetransformerroreventtype-syntax">WebRTC Encoded Transform: 3 SFrame transforms</see>
    /// </summary>
    [Description("@#syntax")]
    Syntax = 2
}

/// <summary>
/// <see href="https://w3c.github.io/webrtc-encoded-transform/#enumdef-sframetype">WebRTC Encoded Transform: 3 SFrame transforms</see>
/// </summary>
[Description("@#SFrameType")]
[ECMAScript]
[String]
public enum SFrameType
{
    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-encoded-transform/#dom-sframetype-per-frame">WebRTC Encoded Transform: 3 SFrame transforms</see>
    /// </summary>
    [Description("@#per-frame")]
    PerFrame = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-encoded-transform/#dom-sframetype-per-packet">WebRTC Encoded Transform: 3 SFrame transforms</see>
    /// </summary>
    [Description("@#per-packet")]
    PerPacket = 1
}

/// <summary>
/// <see href="https://w3c.github.io/webrtc-identity/#dom-rtcerrordetailtypeidp">Identity for WebRTC 1.0: 10.3 RTCErrorDetailTypeIdp Enum</see>
/// </summary>
[Description("@#RTCErrorDetailTypeIdp")]
[ECMAScript]
[String]
public enum RTCErrorDetailTypeIdp
{
    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-identity/#dom-rtcerrordetailtypeidp-idp-bad-script-failure">Identity for WebRTC 1.0: 10.3 RTCErrorDetailTypeIdp Enum</see>
    /// </summary>
    [Description("@#idp-bad-script-failure")]
    IdpBadScriptFailure = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-identity/#dom-rtcerrordetailtypeidp-idp-execution-failure">Identity for WebRTC 1.0: 10.3 RTCErrorDetailTypeIdp Enum</see>
    /// </summary>
    [Description("@#idp-execution-failure")]
    IdpExecutionFailure = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-identity/#dom-rtcerrordetailtypeidp-idp-load-failure">Identity for WebRTC 1.0: 10.3 RTCErrorDetailTypeIdp Enum</see>
    /// </summary>
    [Description("@#idp-load-failure")]
    IdpLoadFailure = 2,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-identity/#dom-rtcerrordetailtypeidp-idp-need-login">Identity for WebRTC 1.0: 10.3 RTCErrorDetailTypeIdp Enum</see>
    /// </summary>
    [Description("@#idp-need-login")]
    IdpNeedLogin = 3,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-identity/#dom-rtcerrordetailtypeidp-idp-timeout">Identity for WebRTC 1.0: 10.3 RTCErrorDetailTypeIdp Enum</see>
    /// </summary>
    [Description("@#idp-timeout")]
    IdpTimeout = 4,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-identity/#dom-rtcerrordetailtypeidp-idp-tls-failure">Identity for WebRTC 1.0: 10.3 RTCErrorDetailTypeIdp Enum</see>
    /// </summary>
    [Description("@#idp-tls-failure")]
    IdpTlsFailure = 5,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-identity/#dom-rtcerrordetailtypeidp-idp-token-expired">Identity for WebRTC 1.0: 10.3 RTCErrorDetailTypeIdp Enum</see>
    /// </summary>
    [Description("@#idp-token-expired")]
    IdpTokenExpired = 6,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-identity/#dom-rtcerrordetailtypeidp-idp-token-invalid">Identity for WebRTC 1.0: 10.3 RTCErrorDetailTypeIdp Enum</see>
    /// </summary>
    [Description("@#idp-token-invalid")]
    IdpTokenInvalid = 7
}

/// <summary>
/// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcbundlepolicy">WebRTC: Real-Time Communication in Browsers: 4.2.4 RTCBundlePolicy Enum</see>
/// </summary>
[Description("@#RTCBundlePolicy")]
[ECMAScript]
[String]
public enum RTCBundlePolicy
{
    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcbundlepolicy-balanced">WebRTC: Real-Time Communication in Browsers: 4.2.4 RTCBundlePolicy Enum</see>
    /// </summary>
    [Description("@#balanced")]
    Balanced = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcbundlepolicy-max-compat">WebRTC: Real-Time Communication in Browsers: 4.2.4 RTCBundlePolicy Enum</see>
    /// </summary>
    [Description("@#max-compat")]
    MaxCompat = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcbundlepolicy-max-bundle">WebRTC: Real-Time Communication in Browsers: 4.2.4 RTCBundlePolicy Enum</see>
    /// </summary>
    [Description("@#max-bundle")]
    MaxBundle = 2
}

/// <summary>
/// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcdatachannelstate">WebRTC: Real-Time Communication in Browsers: 6.2 RTCDataChannel</see>
/// </summary>
[Description("@#RTCDataChannelState")]
[ECMAScript]
[String]
public enum RTCDataChannelState
{
    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcdatachannelstate-connecting">WebRTC: Real-Time Communication in Browsers: 6.2 RTCDataChannel</see>
    /// </summary>
    [Description("@#connecting")]
    Connecting = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcdatachannelstate-open">WebRTC: Real-Time Communication in Browsers: 6.2 RTCDataChannel</see>
    /// </summary>
    [Description("@#open")]
    Open = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcdatachannelstate-closing">WebRTC: Real-Time Communication in Browsers: 6.2 RTCDataChannel</see>
    /// </summary>
    [Description("@#closing")]
    Closing = 2,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcdatachannelstate-closed">WebRTC: Real-Time Communication in Browsers: 6.2 RTCDataChannel</see>
    /// </summary>
    [Description("@#closed")]
    Closed = 3
}

/// <summary>
/// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcdtlstransportstate">WebRTC: Real-Time Communication in Browsers: 5.5.1 RTCDtlsTransportState Enum</see>
/// </summary>
[Description("@#RTCDtlsTransportState")]
[ECMAScript]
[String]
public enum RTCDtlsTransportState
{
    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcdtlstransportstate-new">WebRTC: Real-Time Communication in Browsers: 5.5.1 RTCDtlsTransportState Enum</see>
    /// </summary>
    [Description("@#new")]
    New = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcdtlstransportstate-connecting">WebRTC: Real-Time Communication in Browsers: 5.5.1 RTCDtlsTransportState Enum</see>
    /// </summary>
    [Description("@#connecting")]
    Connecting = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcdtlstransportstate-connected">WebRTC: Real-Time Communication in Browsers: 5.5.1 RTCDtlsTransportState Enum</see>
    /// </summary>
    [Description("@#connected")]
    Connected = 2,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcdtlstransportstate-closed">WebRTC: Real-Time Communication in Browsers: 5.5.1 RTCDtlsTransportState Enum</see>
    /// </summary>
    [Description("@#closed")]
    Closed = 3,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcdtlstransportstate-failed">WebRTC: Real-Time Communication in Browsers: 5.5.1 RTCDtlsTransportState Enum</see>
    /// </summary>
    [Description("@#failed")]
    Failed = 4
}

/// <summary>
/// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcerrordetailtype">WebRTC: Real-Time Communication in Browsers: 11.2 RTCErrorDetailType Enum</see>
/// </summary>
[Description("@#RTCErrorDetailType")]
[ECMAScript]
[String]
public enum RTCErrorDetailType
{
    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcerrordetailtype-data-channel-failure">WebRTC: Real-Time Communication in Browsers: 11.2 RTCErrorDetailType Enum</see>
    /// </summary>
    [Description("@#data-channel-failure")]
    DataChannelFailure = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcerrordetailtype-dtls-failure">WebRTC: Real-Time Communication in Browsers: 11.2 RTCErrorDetailType Enum</see>
    /// </summary>
    [Description("@#dtls-failure")]
    DtlsFailure = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcerrordetailtype-fingerprint-failure">WebRTC: Real-Time Communication in Browsers: 11.2 RTCErrorDetailType Enum</see>
    /// </summary>
    [Description("@#fingerprint-failure")]
    FingerprintFailure = 2,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcerrordetailtype-sctp-failure">WebRTC: Real-Time Communication in Browsers: 11.2 RTCErrorDetailType Enum</see>
    /// </summary>
    [Description("@#sctp-failure")]
    SctpFailure = 3,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcerrordetailtype-sdp-syntax-error">WebRTC: Real-Time Communication in Browsers: 11.2 RTCErrorDetailType Enum</see>
    /// </summary>
    [Description("@#sdp-syntax-error")]
    SdpSyntaxError = 4,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcerrordetailtype-hardware-encoder-not-available">WebRTC: Real-Time Communication in Browsers: 11.2 RTCErrorDetailType Enum</see>
    /// </summary>
    [Description("@#hardware-encoder-not-available")]
    HardwareEncoderNotAvailable = 5,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcerrordetailtype-hardware-encoder-error">WebRTC: Real-Time Communication in Browsers: 11.2 RTCErrorDetailType Enum</see>
    /// </summary>
    [Description("@#hardware-encoder-error")]
    HardwareEncoderError = 6
}

/// <summary>
/// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicecandidatetype">WebRTC: Real-Time Communication in Browsers: 4.8.1.4 RTCIceCandidateType Enum</see>
/// </summary>
[Description("@#RTCIceCandidateType")]
[ECMAScript]
[String]
public enum RTCIceCandidateType
{
    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicecandidatetype-host">WebRTC: Real-Time Communication in Browsers: 4.8.1.4 RTCIceCandidateType Enum</see>
    /// </summary>
    [Description("@#host")]
    Host = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicecandidatetype-srflx">WebRTC: Real-Time Communication in Browsers: 4.8.1.4 RTCIceCandidateType Enum</see>
    /// </summary>
    [Description("@#srflx")]
    Srflx = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicecandidatetype-prflx">WebRTC: Real-Time Communication in Browsers: 4.8.1.4 RTCIceCandidateType Enum</see>
    /// </summary>
    [Description("@#prflx")]
    Prflx = 2,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicecandidatetype-relay">WebRTC: Real-Time Communication in Browsers: 4.8.1.4 RTCIceCandidateType Enum</see>
    /// </summary>
    [Description("@#relay")]
    Relay = 3
}

/// <summary>
/// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicecomponent">WebRTC: Real-Time Communication in Browsers: 5.6.6 RTCIceComponent Enum</see>
/// </summary>
[Description("@#RTCIceComponent")]
[ECMAScript]
[String]
public enum RTCIceComponent
{
    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicecomponent-rtp">WebRTC: Real-Time Communication in Browsers: 5.6.6 RTCIceComponent Enum</see>
    /// </summary>
    [Description("@#rtp")]
    Rtp = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicecomponent-rtcp">WebRTC: Real-Time Communication in Browsers: 5.6.6 RTCIceComponent Enum</see>
    /// </summary>
    [Description("@#rtcp")]
    Rtcp = 1
}

/// <summary>
/// <see href="https://w3c.github.io/webrtc-pc/#dom-rtciceconnectionstate">WebRTC: Real-Time Communication in Browsers: 4.3.4 RTCIceConnectionState Enum</see>
/// </summary>
[Description("@#RTCIceConnectionState")]
[ECMAScript]
[String]
public enum RTCIceConnectionState
{
    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtciceconnectionstate-closed">WebRTC: Real-Time Communication in Browsers: 4.3.4 RTCIceConnectionState Enum</see>
    /// </summary>
    [Description("@#closed")]
    Closed = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtciceconnectionstate-failed">WebRTC: Real-Time Communication in Browsers: 4.3.4 RTCIceConnectionState Enum</see>
    /// </summary>
    [Description("@#failed")]
    Failed = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtciceconnectionstate-disconnected">WebRTC: Real-Time Communication in Browsers: 4.3.4 RTCIceConnectionState Enum</see>
    /// </summary>
    [Description("@#disconnected")]
    Disconnected = 2,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtciceconnectionstate-new">WebRTC: Real-Time Communication in Browsers: 4.3.4 RTCIceConnectionState Enum</see>
    /// </summary>
    [Description("@#new")]
    New = 3,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtciceconnectionstate-checking">WebRTC: Real-Time Communication in Browsers: 4.3.4 RTCIceConnectionState Enum</see>
    /// </summary>
    [Description("@#checking")]
    Checking = 4,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtciceconnectionstate-completed">WebRTC: Real-Time Communication in Browsers: 4.3.4 RTCIceConnectionState Enum</see>
    /// </summary>
    [Description("@#completed")]
    Completed = 5,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtciceconnectionstate-connected">WebRTC: Real-Time Communication in Browsers: 4.3.4 RTCIceConnectionState Enum</see>
    /// </summary>
    [Description("@#connected")]
    Connected = 6
}

/// <summary>
/// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicegathererstate">WebRTC: Real-Time Communication in Browsers: 5.6.3 RTCIceGathererState Enum</see>
/// </summary>
[Description("@#RTCIceGathererState")]
[ECMAScript]
[String]
public enum RTCIceGathererState
{
    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicegathererstate-new">WebRTC: Real-Time Communication in Browsers: 5.6.3 RTCIceGathererState Enum</see>
    /// </summary>
    [Description("@#new")]
    New = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicegathererstate-gathering">WebRTC: Real-Time Communication in Browsers: 5.6.3 RTCIceGathererState Enum</see>
    /// </summary>
    [Description("@#gathering")]
    Gathering = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicegathererstate-complete">WebRTC: Real-Time Communication in Browsers: 5.6.3 RTCIceGathererState Enum</see>
    /// </summary>
    [Description("@#complete")]
    Complete = 2
}

/// <summary>
/// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicegatheringstate">WebRTC: Real-Time Communication in Browsers: 4.3.2 RTCIceGatheringState Enum</see>
/// </summary>
[Description("@#RTCIceGatheringState")]
[ECMAScript]
[String]
public enum RTCIceGatheringState
{
    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicegatheringstate-new">WebRTC: Real-Time Communication in Browsers: 4.3.2 RTCIceGatheringState Enum</see>
    /// </summary>
    [Description("@#new")]
    New = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicegatheringstate-gathering">WebRTC: Real-Time Communication in Browsers: 4.3.2 RTCIceGatheringState Enum</see>
    /// </summary>
    [Description("@#gathering")]
    Gathering = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicegatheringstate-complete">WebRTC: Real-Time Communication in Browsers: 4.3.2 RTCIceGatheringState Enum</see>
    /// </summary>
    [Description("@#complete")]
    Complete = 2
}

/// <summary>
/// <see href="https://w3c.github.io/webrtc-pc/#dom-rtciceprotocol">WebRTC: Real-Time Communication in Browsers: 4.8.1.2 RTCIceProtocol Enum</see>
/// </summary>
[Description("@#RTCIceProtocol")]
[ECMAScript]
[String]
public enum RTCIceProtocol
{
    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtciceprotocol-udp">WebRTC: Real-Time Communication in Browsers: 4.8.1.2 RTCIceProtocol Enum</see>
    /// </summary>
    [Description("@#udp")]
    Udp = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtciceprotocol-tcp">WebRTC: Real-Time Communication in Browsers: 4.8.1.2 RTCIceProtocol Enum</see>
    /// </summary>
    [Description("@#tcp")]
    Tcp = 1
}

/// <summary>
/// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicerole">WebRTC: Real-Time Communication in Browsers: 5.6.5 RTCIceRole Enum</see>
/// </summary>
[Description("@#RTCIceRole")]
[ECMAScript]
[String]
public enum RTCIceRole
{
    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicerole-unknown">WebRTC: Real-Time Communication in Browsers: 5.6.5 RTCIceRole Enum</see>
    /// </summary>
    [Description("@#unknown")]
    Unknown = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicerole-controlling">WebRTC: Real-Time Communication in Browsers: 5.6.5 RTCIceRole Enum</see>
    /// </summary>
    [Description("@#controlling")]
    Controlling = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicerole-controlled">WebRTC: Real-Time Communication in Browsers: 5.6.5 RTCIceRole Enum</see>
    /// </summary>
    [Description("@#controlled")]
    Controlled = 2
}

/// <summary>
/// <see href="https://w3c.github.io/webrtc-pc/#dom-rtciceservertransportprotocol">WebRTC: Real-Time Communication in Browsers: 4.8.1.5 RTCIceServerTransportProtocol Enum</see>
/// </summary>
[Description("@#RTCIceServerTransportProtocol")]
[ECMAScript]
[String]
public enum RTCIceServerTransportProtocol
{
    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtciceservertransportprotocol-udp">WebRTC: Real-Time Communication in Browsers: 4.8.1.5 RTCIceServerTransportProtocol Enum</see>
    /// </summary>
    [Description("@#udp")]
    Udp = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtciceservertransportprotocol-tcp">WebRTC: Real-Time Communication in Browsers: 4.8.1.5 RTCIceServerTransportProtocol Enum</see>
    /// </summary>
    [Description("@#tcp")]
    Tcp = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtciceservertransportprotocol-tls">WebRTC: Real-Time Communication in Browsers: 4.8.1.5 RTCIceServerTransportProtocol Enum</see>
    /// </summary>
    [Description("@#tls")]
    Tls = 2
}

/// <summary>
/// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicetcpcandidatetype">WebRTC: Real-Time Communication in Browsers: 4.8.1.3 RTCIceTcpCandidateType Enum</see>
/// </summary>
[Description("@#RTCIceTcpCandidateType")]
[ECMAScript]
[String]
public enum RTCIceTcpCandidateType
{
    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicetcpcandidatetype-active">WebRTC: Real-Time Communication in Browsers: 4.8.1.3 RTCIceTcpCandidateType Enum</see>
    /// </summary>
    [Description("@#active")]
    Active = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicetcpcandidatetype-passive">WebRTC: Real-Time Communication in Browsers: 4.8.1.3 RTCIceTcpCandidateType Enum</see>
    /// </summary>
    [Description("@#passive")]
    Passive = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicetcpcandidatetype-so">WebRTC: Real-Time Communication in Browsers: 4.8.1.3 RTCIceTcpCandidateType Enum</see>
    /// </summary>
    [Description("@#so")]
    So = 2
}

/// <summary>
/// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicetransportpolicy">WebRTC: Real-Time Communication in Browsers: 4.2.3 RTCIceTransportPolicy Enum</see>
/// </summary>
[Description("@#RTCIceTransportPolicy")]
[ECMAScript]
[String]
public enum RTCIceTransportPolicy
{
    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicetransportpolicy-relay">WebRTC: Real-Time Communication in Browsers: 4.2.3 RTCIceTransportPolicy Enum</see>
    /// </summary>
    [Description("@#relay")]
    Relay = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicetransportpolicy-all">WebRTC: Real-Time Communication in Browsers: 4.2.3 RTCIceTransportPolicy Enum</see>
    /// </summary>
    [Description("@#all")]
    All = 1
}

/// <summary>
/// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicetransportstate">WebRTC: Real-Time Communication in Browsers: 5.6.4 RTCIceTransportState Enum</see>
/// </summary>
[Description("@#RTCIceTransportState")]
[ECMAScript]
[String]
public enum RTCIceTransportState
{
    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicetransportstate-closed">WebRTC: Real-Time Communication in Browsers: 5.6.4 RTCIceTransportState Enum</see>
    /// </summary>
    [Description("@#closed")]
    Closed = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicetransportstate-failed">WebRTC: Real-Time Communication in Browsers: 5.6.4 RTCIceTransportState Enum</see>
    /// </summary>
    [Description("@#failed")]
    Failed = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicetransportstate-disconnected">WebRTC: Real-Time Communication in Browsers: 5.6.4 RTCIceTransportState Enum</see>
    /// </summary>
    [Description("@#disconnected")]
    Disconnected = 2,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicetransportstate-new">WebRTC: Real-Time Communication in Browsers: 5.6.4 RTCIceTransportState Enum</see>
    /// </summary>
    [Description("@#new")]
    New = 3,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicetransportstate-checking">WebRTC: Real-Time Communication in Browsers: 5.6.4 RTCIceTransportState Enum</see>
    /// </summary>
    [Description("@#checking")]
    Checking = 4,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicetransportstate-completed">WebRTC: Real-Time Communication in Browsers: 5.6.4 RTCIceTransportState Enum</see>
    /// </summary>
    [Description("@#completed")]
    Completed = 5,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicetransportstate-connected">WebRTC: Real-Time Communication in Browsers: 5.6.4 RTCIceTransportState Enum</see>
    /// </summary>
    [Description("@#connected")]
    Connected = 6
}

/// <summary>
/// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcpeerconnectionstate">WebRTC: Real-Time Communication in Browsers: 4.3.3 RTCPeerConnectionState Enum</see>
/// </summary>
[Description("@#RTCPeerConnectionState")]
[ECMAScript]
[String]
public enum RTCPeerConnectionState
{
    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcpeerconnectionstate-closed">WebRTC: Real-Time Communication in Browsers: 4.3.3 RTCPeerConnectionState Enum</see>
    /// </summary>
    [Description("@#closed")]
    Closed = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcpeerconnectionstate-failed">WebRTC: Real-Time Communication in Browsers: 4.3.3 RTCPeerConnectionState Enum</see>
    /// </summary>
    [Description("@#failed")]
    Failed = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcpeerconnectionstate-disconnected">WebRTC: Real-Time Communication in Browsers: 4.3.3 RTCPeerConnectionState Enum</see>
    /// </summary>
    [Description("@#disconnected")]
    Disconnected = 2,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcpeerconnectionstate-new">WebRTC: Real-Time Communication in Browsers: 4.3.3 RTCPeerConnectionState Enum</see>
    /// </summary>
    [Description("@#new")]
    New = 3,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcpeerconnectionstate-connecting">WebRTC: Real-Time Communication in Browsers: 4.3.3 RTCPeerConnectionState Enum</see>
    /// </summary>
    [Description("@#connecting")]
    Connecting = 4,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcpeerconnectionstate-connected">WebRTC: Real-Time Communication in Browsers: 4.3.3 RTCPeerConnectionState Enum</see>
    /// </summary>
    [Description("@#connected")]
    Connected = 5
}

/// <summary>
/// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcrtcpmuxpolicy">WebRTC: Real-Time Communication in Browsers: 4.2.5 RTCRtcpMuxPolicy Enum</see>
/// </summary>
[Description("@#RTCRtcpMuxPolicy")]
[ECMAScript]
[String]
public enum RTCRtcpMuxPolicy
{
    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcrtcpmuxpolicy-require">WebRTC: Real-Time Communication in Browsers: 4.2.5 RTCRtcpMuxPolicy Enum</see>
    /// </summary>
    [Description("@#require")]
    Require = 0
}

/// <summary>
/// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcrtptransceiverdirection">WebRTC: Real-Time Communication in Browsers: 5.1 RTCPeerConnection Interface Extensions</see>
/// </summary>
[Description("@#RTCRtpTransceiverDirection")]
[ECMAScript]
[String]
public enum RTCRtpTransceiverDirection
{
    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcrtptransceiverdirection-sendrecv">WebRTC: Real-Time Communication in Browsers: 5.1 RTCPeerConnection Interface Extensions</see>
    /// </summary>
    [Description("@#sendrecv")]
    Sendrecv = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcrtptransceiverdirection-sendonly">WebRTC: Real-Time Communication in Browsers: 5.1 RTCPeerConnection Interface Extensions</see>
    /// </summary>
    [Description("@#sendonly")]
    Sendonly = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcrtptransceiverdirection-recvonly">WebRTC: Real-Time Communication in Browsers: 5.1 RTCPeerConnection Interface Extensions</see>
    /// </summary>
    [Description("@#recvonly")]
    Recvonly = 2,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcrtptransceiverdirection-inactive">WebRTC: Real-Time Communication in Browsers: 5.1 RTCPeerConnection Interface Extensions</see>
    /// </summary>
    [Description("@#inactive")]
    Inactive = 3,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcrtptransceiverdirection-stopped">WebRTC: Real-Time Communication in Browsers: 5.1 RTCPeerConnection Interface Extensions</see>
    /// </summary>
    [Description("@#stopped")]
    Stopped = 4
}

/// <summary>
/// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcsctptransportstate">WebRTC: Real-Time Communication in Browsers: 6.1.2 RTCSctpTransportState Enum</see>
/// </summary>
[Description("@#RTCSctpTransportState")]
[ECMAScript]
[String]
public enum RTCSctpTransportState
{
    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#idl-def-RTCSctpTransportState.connecting">WebRTC: Real-Time Communication in Browsers: 6.1.2 RTCSctpTransportState Enum</see>
    /// </summary>
    [Description("@#connecting")]
    Connecting = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#idl-def-RTCSctpTransportState.connected">WebRTC: Real-Time Communication in Browsers: 6.1.2 RTCSctpTransportState Enum</see>
    /// </summary>
    [Description("@#connected")]
    Connected = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#idl-def-RTCSctpTransportState.closed">WebRTC: Real-Time Communication in Browsers: 6.1.2 RTCSctpTransportState Enum</see>
    /// </summary>
    [Description("@#closed")]
    Closed = 2
}

/// <summary>
/// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcsdptype">WebRTC: Real-Time Communication in Browsers: 4.6.1 RTCSdpType</see>
/// </summary>
[Description("@#RTCSdpType")]
[ECMAScript]
[String]
public enum RTCSdpType
{
    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcsdptype-offer">WebRTC: Real-Time Communication in Browsers: 4.6.1 RTCSdpType</see>
    /// </summary>
    [Description("@#offer")]
    Offer = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcsdptype-pranswer">WebRTC: Real-Time Communication in Browsers: 4.6.1 RTCSdpType</see>
    /// </summary>
    [Description("@#pranswer")]
    Pranswer = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcsdptype-answer">WebRTC: Real-Time Communication in Browsers: 4.6.1 RTCSdpType</see>
    /// </summary>
    [Description("@#answer")]
    Answer = 2,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcsdptype-rollback">WebRTC: Real-Time Communication in Browsers: 4.6.1 RTCSdpType</see>
    /// </summary>
    [Description("@#rollback")]
    Rollback = 3
}

/// <summary>
/// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcsignalingstate">WebRTC: Real-Time Communication in Browsers: 4.3.1 RTCSignalingState Enum</see>
/// </summary>
[Description("@#RTCSignalingState")]
[ECMAScript]
[String]
public enum RTCSignalingState
{
    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcsignalingstate-stable">WebRTC: Real-Time Communication in Browsers: 4.3.1 RTCSignalingState Enum</see>
    /// </summary>
    [Description("@#stable")]
    Stable = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcsignalingstate-have-local-offer">WebRTC: Real-Time Communication in Browsers: 4.3.1 RTCSignalingState Enum</see>
    /// </summary>
    [Description("@#have-local-offer")]
    HaveLocalOffer = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcsignalingstate-have-remote-offer">WebRTC: Real-Time Communication in Browsers: 4.3.1 RTCSignalingState Enum</see>
    /// </summary>
    [Description("@#have-remote-offer")]
    HaveRemoteOffer = 2,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcsignalingstate-have-local-pranswer">WebRTC: Real-Time Communication in Browsers: 4.3.1 RTCSignalingState Enum</see>
    /// </summary>
    [Description("@#have-local-pranswer")]
    HaveLocalPranswer = 3,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcsignalingstate-have-remote-pranswer">WebRTC: Real-Time Communication in Browsers: 4.3.1 RTCSignalingState Enum</see>
    /// </summary>
    [Description("@#have-remote-pranswer")]
    HaveRemotePranswer = 4,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcsignalingstate-closed">WebRTC: Real-Time Communication in Browsers: 4.3.1 RTCSignalingState Enum</see>
    /// </summary>
    [Description("@#closed")]
    Closed = 5
}

/// <summary>
/// <see href="https://w3c.github.io/webrtc-priority/#enumdef-rtcprioritytype">WebRTC Priority Control API: 3.1 RTCPriorityType Enum</see>
/// </summary>
[Description("@#RTCPriorityType")]
[ECMAScript]
[String]
public enum RTCPriorityType
{
    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-priority/#dom-rtcprioritytype-very-low">WebRTC Priority Control API: 3.1 RTCPriorityType Enum</see>
    /// </summary>
    [Description("@#very-low")]
    VeryLow = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-priority/#dom-rtcprioritytype-low">WebRTC Priority Control API: 3.1 RTCPriorityType Enum</see>
    /// </summary>
    [Description("@#low")]
    Low = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-priority/#dom-rtcprioritytype-medium">WebRTC Priority Control API: 3.1 RTCPriorityType Enum</see>
    /// </summary>
    [Description("@#medium")]
    Medium = 2,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-priority/#dom-rtcprioritytype-high">WebRTC Priority Control API: 3.1 RTCPriorityType Enum</see>
    /// </summary>
    [Description("@#high")]
    High = 3
}

/// <summary>
/// <see href="https://w3c.github.io/webtransport/#enumdef-webtransportcongestioncontrol">WebTransport: 6.9 Configuration</see>
/// </summary>
[Description("@#WebTransportCongestionControl")]
[ECMAScript]
[String]
public enum WebTransportCongestionControl
{
    /// <summary>
    /// <see href="https://w3c.github.io/webtransport/#dom-webtransportcongestioncontrol-default">WebTransport: 6.9 Configuration</see>
    /// </summary>
    [Description("@#default")]
    Default = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webtransport/#dom-webtransportcongestioncontrol-throughput">WebTransport: 6.9 Configuration</see>
    /// </summary>
    [Description("@#throughput")]
    Throughput = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/webtransport/#dom-webtransportcongestioncontrol-low-latency">WebTransport: 6.9 Configuration</see>
    /// </summary>
    [Description("@#low-latency")]
    LowLatency = 2
}

/// <summary>
/// <see href="https://w3c.github.io/webtransport/#enumdef-webtransporterrorsource">WebTransport: 12. WebTransportError Interface</see>
/// </summary>
[Description("@#WebTransportErrorSource")]
[ECMAScript]
[String]
public enum WebTransportErrorSource
{
    /// <summary>
    /// <see href="https://w3c.github.io/webtransport/#dom-webtransporterrorsource-stream">WebTransport: 12. WebTransportError Interface</see>
    /// </summary>
    [Description("@#stream")]
    Stream = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webtransport/#dom-webtransporterrorsource-session">WebTransport: 12. WebTransportError Interface</see>
    /// </summary>
    [Description("@#session")]
    Session = 1
}

/// <summary>
/// <see href="https://w3c.github.io/webtransport/#enumdef-webtransportreliabilitymode">WebTransport: 6 WebTransport Interface</see>
/// </summary>
[Description("@#WebTransportReliabilityMode")]
[ECMAScript]
[String]
public enum WebTransportReliabilityMode
{
    /// <summary>
    /// <see href="https://w3c.github.io/webtransport/#dom-webtransportreliabilitymode-pending">WebTransport: 6 WebTransport Interface</see>
    /// </summary>
    [Description("@#pending")]
    Pending = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webtransport/#dom-webtransportreliabilitymode-reliable-only">WebTransport: 6 WebTransport Interface</see>
    /// </summary>
    [Description("@#reliable-only")]
    ReliableOnly = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/webtransport/#dom-webtransportreliabilitymode-supports-unreliable">WebTransport: 6 WebTransport Interface</see>
    /// </summary>
    [Description("@#supports-unreliable")]
    SupportsUnreliable = 2
}

/// <summary>
/// <see href="https://w3c.github.io/webvtt/#enumdef-alignsetting">WebVTT: The Web Video Text Tracks Format: 9.1 The VTTCue interface</see>
/// </summary>
[Description("@#AlignSetting")]
[ECMAScript]
[String]
public enum AlignSetting
{
    /// <summary>
    /// <see href="https://w3c.github.io/webvtt/#dom-alignsetting-start">WebVTT: The Web Video Text Tracks Format: 9.1 The VTTCue interface</see>
    /// </summary>
    [Description("@#start")]
    Start = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webvtt/#dom-alignsetting-center">WebVTT: The Web Video Text Tracks Format: 9.1 The VTTCue interface</see>
    /// </summary>
    [Description("@#center")]
    Center = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/webvtt/#dom-alignsetting-end">WebVTT: The Web Video Text Tracks Format: 9.1 The VTTCue interface</see>
    /// </summary>
    [Description("@#end")]
    End = 2,

    /// <summary>
    /// <see href="https://w3c.github.io/webvtt/#dom-alignsetting-left">WebVTT: The Web Video Text Tracks Format: 9.1 The VTTCue interface</see>
    /// </summary>
    [Description("@#left")]
    Left = 3,

    /// <summary>
    /// <see href="https://w3c.github.io/webvtt/#dom-alignsetting-right">WebVTT: The Web Video Text Tracks Format: 9.1 The VTTCue interface</see>
    /// </summary>
    [Description("@#right")]
    Right = 4
}

/// <summary>
/// <see href="https://w3c.github.io/webvtt/#enumdef-autokeyword">WebVTT: The Web Video Text Tracks Format: 9.1 The VTTCue interface</see>
/// </summary>
[Description("@#AutoKeyword")]
[ECMAScript]
[String]
public enum AutoKeyword
{
    /// <summary>
    /// <see href="https://w3c.github.io/webvtt/#dom-autokeyword-auto">WebVTT: The Web Video Text Tracks Format: 9.1 The VTTCue interface</see>
    /// </summary>
    [Description("@#auto")]
    Auto = 0
}

/// <summary>
/// <see href="https://w3c.github.io/webvtt/#enumdef-directionsetting">WebVTT: The Web Video Text Tracks Format: 9.1 The VTTCue interface</see>
/// </summary>
[Description("@#DirectionSetting")]
[ECMAScript]
[String]
public enum DirectionSetting
{
    [Description("@#")]
    Empty = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webvtt/#dom-directionsetting-rl">WebVTT: The Web Video Text Tracks Format: 9.1 The VTTCue interface</see>
    /// </summary>
    [Description("@#rl")]
    Rl = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/webvtt/#dom-directionsetting-lr">WebVTT: The Web Video Text Tracks Format: 9.1 The VTTCue interface</see>
    /// </summary>
    [Description("@#lr")]
    Lr = 2
}

/// <summary>
/// <see href="https://w3c.github.io/webvtt/#enumdef-linealignsetting">WebVTT: The Web Video Text Tracks Format: 9.1 The VTTCue interface</see>
/// </summary>
[Description("@#LineAlignSetting")]
[ECMAScript]
[String]
public enum LineAlignSetting
{
    /// <summary>
    /// <see href="https://w3c.github.io/webvtt/#dom-linealignsetting-start">WebVTT: The Web Video Text Tracks Format: 9.1 The VTTCue interface</see>
    /// </summary>
    [Description("@#start")]
    Start = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webvtt/#dom-linealignsetting-center">WebVTT: The Web Video Text Tracks Format: 9.1 The VTTCue interface</see>
    /// </summary>
    [Description("@#center")]
    Center = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/webvtt/#dom-linealignsetting-end">WebVTT: The Web Video Text Tracks Format: 9.1 The VTTCue interface</see>
    /// </summary>
    [Description("@#end")]
    End = 2
}

/// <summary>
/// <see href="https://w3c.github.io/webvtt/#enumdef-positionalignsetting">WebVTT: The Web Video Text Tracks Format: 9.1 The VTTCue interface</see>
/// </summary>
[Description("@#PositionAlignSetting")]
[ECMAScript]
[String]
public enum PositionAlignSetting
{
    /// <summary>
    /// <see href="https://w3c.github.io/webvtt/#dom-positionalignsetting-line-left">WebVTT: The Web Video Text Tracks Format: 9.1 The VTTCue interface</see>
    /// </summary>
    [Description("@#line-left")]
    LineLeft = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webvtt/#dom-positionalignsetting-center">WebVTT: The Web Video Text Tracks Format: 9.1 The VTTCue interface</see>
    /// </summary>
    [Description("@#center")]
    Center = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/webvtt/#dom-positionalignsetting-line-right">WebVTT: The Web Video Text Tracks Format: 9.1 The VTTCue interface</see>
    /// </summary>
    [Description("@#line-right")]
    LineRight = 2,

    /// <summary>
    /// <see href="https://w3c.github.io/webvtt/#dom-positionalignsetting-auto">WebVTT: The Web Video Text Tracks Format: 9.1 The VTTCue interface</see>
    /// </summary>
    [Description("@#auto")]
    Auto = 3
}

/// <summary>
/// <see href="https://w3c.github.io/webvtt/#enumdef-scrollsetting">WebVTT: The Web Video Text Tracks Format: 9.2 The VTTRegion interface</see>
/// </summary>
[Description("@#ScrollSetting")]
[ECMAScript]
[String]
public enum ScrollSetting
{
    [Description("@#")]
    Empty = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webvtt/#dom-scrollsetting-up">WebVTT: The Web Video Text Tracks Format: 9.2 The VTTRegion interface</see>
    /// </summary>
    [Description("@#up")]
    Up = 1
}

/// <summary>
/// <see href="https://webaudio.github.io/web-audio-api/#enumdef-audiocontextlatencycategory">Web Audio API 1.1: 1.2 The AudioContext Interface</see>
/// </summary>
[Description("@#AudioContextLatencyCategory")]
[ECMAScript]
[String]
public enum AudioContextLatencyCategory
{
    /// <summary>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-audiocontextlatencycategory-balanced">Web Audio API 1.1: 1.2 The AudioContext Interface</see>
    /// </summary>
    [Description("@#balanced")]
    Balanced = 0,

    /// <summary>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-audiocontextlatencycategory-interactive">Web Audio API 1.1: 1.2 The AudioContext Interface</see>
    /// </summary>
    [Description("@#interactive")]
    Interactive = 1,

    /// <summary>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-audiocontextlatencycategory-playback">Web Audio API 1.1: 1.2 The AudioContext Interface</see>
    /// </summary>
    [Description("@#playback")]
    Playback = 2
}

/// <summary>
/// <see href="https://webaudio.github.io/web-audio-api/#enumdef-audiocontextrendersizecategory">Web Audio API 1.1: 1.1 The BaseAudioContext Interface</see>
/// </summary>
[Description("@#AudioContextRenderSizeCategory")]
[ECMAScript]
[String]
public enum AudioContextRenderSizeCategory
{
    /// <summary>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-audiocontextrendersizecategory-default">Web Audio API 1.1: 1.1 The BaseAudioContext Interface</see>
    /// </summary>
    [Description("@#default")]
    Default = 0,

    /// <summary>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-audiocontextrendersizecategory-hardware">Web Audio API 1.1: 1.1 The BaseAudioContext Interface</see>
    /// </summary>
    [Description("@#hardware")]
    Hardware = 1
}

/// <summary>
/// <see href="https://webaudio.github.io/web-audio-api/#enumdef-audiocontextstate">Web Audio API 1.1: 1.1 The BaseAudioContext Interface</see>
/// </summary>
[Description("@#AudioContextState")]
[ECMAScript]
[String]
public enum AudioContextState
{
    /// <summary>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-audiocontextstate-suspended">Web Audio API 1.1: 1.1 The BaseAudioContext Interface</see>
    /// </summary>
    [Description("@#suspended")]
    Suspended = 0,

    /// <summary>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-audiocontextstate-running">Web Audio API 1.1: 1.1 The BaseAudioContext Interface</see>
    /// </summary>
    [Description("@#running")]
    Running = 1,

    /// <summary>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-audiocontextstate-closed">Web Audio API 1.1: 1.1 The BaseAudioContext Interface</see>
    /// </summary>
    [Description("@#closed")]
    Closed = 2,

    /// <summary>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-audiocontextstate-interrupted">Web Audio API 1.1: 1.1 The BaseAudioContext Interface</see>
    /// </summary>
    [Description("@#interrupted")]
    Interrupted = 3
}

/// <summary>
/// <see href="https://webaudio.github.io/web-audio-api/#enumdef-audiosinktype">Web Audio API 1.1: 1.2 The AudioContext Interface</see>
/// </summary>
[Description("@#AudioSinkType")]
[ECMAScript]
[String]
public enum AudioSinkType
{
    /// <summary>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-audiosinktype-none">Web Audio API 1.1: 1.2 The AudioContext Interface</see>
    /// </summary>
    [Description("@#none")]
    None = 0
}

/// <summary>
/// <see href="https://webaudio.github.io/web-audio-api/#enumdef-automationrate">Web Audio API 1.1: 1.6 The AudioParam Interface</see>
/// </summary>
[Description("@#AutomationRate")]
[ECMAScript]
[String]
public enum AutomationRate
{
    /// <summary>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-automationrate-a-rate">Web Audio API 1.1: 1.6 The AudioParam Interface</see>
    /// </summary>
    [Description("@#a-rate")]
    ARate = 0,

    /// <summary>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-automationrate-k-rate">Web Audio API 1.1: 1.6 The AudioParam Interface</see>
    /// </summary>
    [Description("@#k-rate")]
    KRate = 1
}

/// <summary>
/// <see href="https://webaudio.github.io/web-audio-api/#enumdef-biquadfiltertype">Web Audio API 1.1: 1.13 The BiquadFilterNode Interface</see>
/// </summary>
[Description("@#BiquadFilterType")]
[ECMAScript]
[String]
public enum BiquadFilterType
{
    /// <summary>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-biquadfiltertype-lowpass">Web Audio API 1.1: 1.13 The BiquadFilterNode Interface</see>
    /// </summary>
    [Description("@#lowpass")]
    Lowpass = 0,

    /// <summary>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-biquadfiltertype-highpass">Web Audio API 1.1: 1.13 The BiquadFilterNode Interface</see>
    /// </summary>
    [Description("@#highpass")]
    Highpass = 1,

    /// <summary>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-biquadfiltertype-bandpass">Web Audio API 1.1: 1.13 The BiquadFilterNode Interface</see>
    /// </summary>
    [Description("@#bandpass")]
    Bandpass = 2,

    /// <summary>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-biquadfiltertype-lowshelf">Web Audio API 1.1: 1.13 The BiquadFilterNode Interface</see>
    /// </summary>
    [Description("@#lowshelf")]
    Lowshelf = 3,

    /// <summary>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-biquadfiltertype-highshelf">Web Audio API 1.1: 1.13 The BiquadFilterNode Interface</see>
    /// </summary>
    [Description("@#highshelf")]
    Highshelf = 4,

    /// <summary>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-biquadfiltertype-peaking">Web Audio API 1.1: 1.13 The BiquadFilterNode Interface</see>
    /// </summary>
    [Description("@#peaking")]
    Peaking = 5,

    /// <summary>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-biquadfiltertype-notch">Web Audio API 1.1: 1.13 The BiquadFilterNode Interface</see>
    /// </summary>
    [Description("@#notch")]
    Notch = 6,

    /// <summary>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-biquadfiltertype-allpass">Web Audio API 1.1: 1.13 The BiquadFilterNode Interface</see>
    /// </summary>
    [Description("@#allpass")]
    Allpass = 7
}

/// <summary>
/// <see href="https://webaudio.github.io/web-audio-api/#enumdef-channelcountmode">Web Audio API 1.1: 1.5.1 AudioNode Creation</see>
/// </summary>
[Description("@#ChannelCountMode")]
[ECMAScript]
[String]
public enum ChannelCountMode
{
    /// <summary>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-channelcountmode-max">Web Audio API 1.1: 1.5.1 AudioNode Creation</see>
    /// </summary>
    [Description("@#max")]
    Max = 0,

    /// <summary>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-channelcountmode-clamped-max">Web Audio API 1.1: 1.5.1 AudioNode Creation</see>
    /// </summary>
    [Description("@#clamped-max")]
    ClampedMax = 1,

    /// <summary>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-channelcountmode-explicit">Web Audio API 1.1: 1.5.1 AudioNode Creation</see>
    /// </summary>
    [Description("@#explicit")]
    Explicit = 2
}

/// <summary>
/// <see href="https://webaudio.github.io/web-audio-api/#enumdef-channelinterpretation">Web Audio API 1.1: 1.5.1 AudioNode Creation</see>
/// </summary>
[Description("@#ChannelInterpretation")]
[ECMAScript]
[String]
public enum ChannelInterpretation
{
    /// <summary>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-channelinterpretation-speakers">Web Audio API 1.1: 1.5.1 AudioNode Creation</see>
    /// </summary>
    [Description("@#speakers")]
    Speakers = 0,

    /// <summary>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-channelinterpretation-discrete">Web Audio API 1.1: 1.5.1 AudioNode Creation</see>
    /// </summary>
    [Description("@#discrete")]
    Discrete = 1
}

/// <summary>
/// <see href="https://webaudio.github.io/web-audio-api/#enumdef-distancemodeltype">Web Audio API 1.1: 1.27 The PannerNode Interface</see>
/// </summary>
[Description("@#DistanceModelType")]
[ECMAScript]
[String]
public enum DistanceModelType
{
    /// <summary>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-distancemodeltype-linear">Web Audio API 1.1: 1.27 The PannerNode Interface</see>
    /// </summary>
    [Description("@#linear")]
    Linear = 0,

    /// <summary>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-distancemodeltype-inverse">Web Audio API 1.1: 1.27 The PannerNode Interface</see>
    /// </summary>
    [Description("@#inverse")]
    Inverse = 1,

    /// <summary>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-distancemodeltype-exponential">Web Audio API 1.1: 1.27 The PannerNode Interface</see>
    /// </summary>
    [Description("@#exponential")]
    Exponential = 2
}

/// <summary>
/// <see href="https://webaudio.github.io/web-audio-api/#enumdef-oscillatortype">Web Audio API 1.1: 1.26 The OscillatorNode Interface</see>
/// </summary>
[Description("@#OscillatorType")]
[ECMAScript]
[String]
public enum OscillatorType
{
    /// <summary>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-oscillatortype-sine">Web Audio API 1.1: 1.26 The OscillatorNode Interface</see>
    /// </summary>
    [Description("@#sine")]
    Sine = 0,

    /// <summary>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-oscillatortype-square">Web Audio API 1.1: 1.26 The OscillatorNode Interface</see>
    /// </summary>
    [Description("@#square")]
    Square = 1,

    /// <summary>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-oscillatortype-sawtooth">Web Audio API 1.1: 1.26 The OscillatorNode Interface</see>
    /// </summary>
    [Description("@#sawtooth")]
    Sawtooth = 2,

    /// <summary>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-oscillatortype-triangle">Web Audio API 1.1: 1.26 The OscillatorNode Interface</see>
    /// </summary>
    [Description("@#triangle")]
    Triangle = 3,

    /// <summary>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-oscillatortype-custom">Web Audio API 1.1: 1.26 The OscillatorNode Interface</see>
    /// </summary>
    [Description("@#custom")]
    Custom = 4
}

/// <summary>
/// <see href="https://webaudio.github.io/web-audio-api/#enumdef-oversampletype">Web Audio API 1.1: 1.31 The WaveShaperNode Interface</see>
/// </summary>
[Description("@#OverSampleType")]
[ECMAScript]
[String]
public enum OverSampleType
{
    /// <summary>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-oversampletype-none">Web Audio API 1.1: 1.31 The WaveShaperNode Interface</see>
    /// </summary>
    [Description("@#none")]
    None = 0,

    /// <summary>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-oversampletype-2x">Web Audio API 1.1: 1.31 The WaveShaperNode Interface</see>
    /// </summary>
    [Description("@#2x")]
    _2x = 1,

    /// <summary>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-oversampletype-4x">Web Audio API 1.1: 1.31 The WaveShaperNode Interface</see>
    /// </summary>
    [Description("@#4x")]
    _4x = 2
}

/// <summary>
/// <see href="https://webaudio.github.io/web-audio-api/#enumdef-panningmodeltype">Web Audio API 1.1: 1.27 The PannerNode Interface</see>
/// </summary>
[Description("@#PanningModelType")]
[ECMAScript]
[String]
public enum PanningModelType
{
    /// <summary>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-panningmodeltype-equalpower">Web Audio API 1.1: 1.27 The PannerNode Interface</see>
    /// </summary>
    [Description("@#equalpower")]
    Equalpower = 0,

    /// <summary>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-panningmodeltype-hrtf">Web Audio API 1.1: 1.27 The PannerNode Interface</see>
    /// </summary>
    [Description("@#HRTF")]
    HRTF = 1
}

/// <summary>
/// <see href="https://webaudio.github.io/web-midi-api/#dom-midiportconnectionstate">Web MIDI API: 5.4.5 MIDIPortConnectionState Enum</see>
/// </summary>
[Description("@#MIDIPortConnectionState")]
[ECMAScript]
[String]
public enum MIDIPortConnectionState
{
    /// <summary>
    /// If open() is called on a port that is &quot;disconnected&quot;, the port&apos;s .connection will transition to &quot;pending&quot;, until the port becomes &quot;connected&quot; or all references to it are dropped.
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-midi-api/#dom-midiportconnectionstate-open">Web MIDI API: 5.4.5 MIDIPortConnectionState Enum</see>
    /// </remarks>
    [Description("@#open")]
    Open = 0,

    /// <summary>
    /// If the port is already closed (its .connection is &quot;closed&quot; - e.g. the port has not yet been implicitly or explicitly opened, or close() has already been called on this MIDIPort), jump to the step labeled closed below.
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-midi-api/#dom-midiportconnectionstate-closed">Web MIDI API: 5.4.5 MIDIPortConnectionState Enum</see>
    /// </remarks>
    [Description("@#closed")]
    Closed = 1,

    /// <summary>
    /// If open() is called on a port that is &quot;disconnected&quot;, the port&apos;s .connection will transition to &quot;pending&quot;, until the port becomes &quot;connected&quot; or all references to it are dropped.
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-midi-api/#dom-midiportconnectionstate-pending">Web MIDI API: 5.4.5 MIDIPortConnectionState Enum</see>
    /// </remarks>
    [Description("@#pending")]
    Pending = 2
}

/// <summary>
/// <see href="https://webaudio.github.io/web-midi-api/#dom-midiportdevicestate">Web MIDI API: 5.4.4 MIDIPortDeviceState Enum</see>
/// </summary>
[Description("@#MIDIPortDeviceState")]
[ECMAScript]
[String]
public enum MIDIPortDeviceState
{
    /// <summary>
    /// If open() is called on a port that is &quot;disconnected&quot;, the port&apos;s .connection will transition to &quot;pending&quot;, until the port becomes &quot;connected&quot; or all references to it are dropped.
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-midi-api/#dom-midiportdevicestate-disconnected">Web MIDI API: 5.4.4 MIDIPortDeviceState Enum</see>
    /// </remarks>
    [Description("@#disconnected")]
    Disconnected = 0,

    /// <summary>
    /// If the port device has a state of &quot;connected&quot;, when access to the port has been obtained (and the port is ready for input or output), the vended Promise is resolved.
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-midi-api/#dom-midiportdevicestate-connected">Web MIDI API: 5.4.4 MIDIPortDeviceState Enum</see>
    /// </remarks>
    [Description("@#connected")]
    Connected = 1
}

/// <summary>
/// <see href="https://webaudio.github.io/web-midi-api/#dom-midiporttype">Web MIDI API: 5.4.3 MIDIPortType Enum</see>
/// </summary>
[Description("@#MIDIPortType")]
[ECMAScript]
[String]
public enum MIDIPortType
{
    /// <summary>
    /// <see href="https://webaudio.github.io/web-midi-api/#dom-midiporttype-input">Web MIDI API: 5.4.3 MIDIPortType Enum</see>
    /// </summary>
    [Description("@#input")]
    Input = 0,

    /// <summary>
    /// <see href="https://webaudio.github.io/web-midi-api/#dom-midiporttype-output">Web MIDI API: 5.4.3 MIDIPortType Enum</see>
    /// </summary>
    [Description("@#output")]
    Output = 1
}

/// <summary>
/// <see href="https://webaudio.github.io/web-speech-api/#enumdef-availabilitystatus">Web Speech API: 4.1 The SpeechRecognition Interface</see>
/// </summary>
[Description("@#AvailabilityStatus")]
[ECMAScript]
[String]
public enum AvailabilityStatus
{
    /// <summary>
    /// &quot;unavailable&quot;
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-speech-api/#dom-availabilitystatus-unavailable">Web Speech API: 4.1.3 AvailabilityStatus Enum Values</see>
    /// </remarks>
    [Description("@#unavailable")]
    Unavailable = 0,

    /// <summary>
    /// &quot;downloadable&quot;
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-speech-api/#dom-availabilitystatus-downloadable">Web Speech API: 4.1.3 AvailabilityStatus Enum Values</see>
    /// </remarks>
    [Description("@#downloadable")]
    Downloadable = 1,

    /// <summary>
    /// &quot;downloading&quot;
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-speech-api/#dom-availabilitystatus-downloading">Web Speech API: 4.1.3 AvailabilityStatus Enum Values</see>
    /// </remarks>
    [Description("@#downloading")]
    Downloading = 2,

    /// <summary>
    /// The available method returns a Promise that resolves to a AvailabilityStatus indicating the recognition availability matching the SpeechRecognitionOptions argument, including the requested quality level. Access to this method is gated behind the policy-controlled feature &quot;on-device-speech-recognition&quot;, which has a policy-controlled feature/default allowlist of default allowlist/&apos;self&apos;.
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-speech-api/#dom-availabilitystatus-available">Web Speech API: 4.1.3 AvailabilityStatus Enum Values</see>
    /// </remarks>
    [Description("@#available")]
    Available = 3
}

/// <summary>
/// <see href="https://webaudio.github.io/web-speech-api/#enumdef-speechrecognitionerrorcode">Web Speech API: 4.1 The SpeechRecognition Interface</see>
/// </summary>
[Description("@#SpeechRecognitionErrorCode")]
[ECMAScript]
[String]
public enum SpeechRecognitionErrorCode
{
    /// <summary>
    /// <see href="https://webaudio.github.io/web-speech-api/#dom-speechrecognitionerrorcode-no-speech">Web Speech API: 4.1.6 SpeechRecognitionErrorEvent</see>
    /// </summary>
    [Description("@#no-speech")]
    NoSpeech = 0,

    /// <summary>
    /// <see href="https://webaudio.github.io/web-speech-api/#dom-speechrecognitionerrorcode-aborted">Web Speech API: 4.1.6 SpeechRecognitionErrorEvent</see>
    /// </summary>
    [Description("@#aborted")]
    Aborted = 1,

    /// <summary>
    /// &quot;audio-capture&quot;
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-speech-api/#dom-speechrecognitionerrorcode-audio-capture">Web Speech API: 4.1.6 SpeechRecognitionErrorEvent</see>
    /// </remarks>
    [Description("@#audio-capture")]
    AudioCapture = 2,

    /// <summary>
    /// <see href="https://webaudio.github.io/web-speech-api/#dom-speechrecognitionerrorcode-network">Web Speech API: 4.1.6 SpeechRecognitionErrorEvent</see>
    /// </summary>
    [Description("@#network")]
    Network = 3,

    /// <summary>
    /// &quot;not-allowed&quot;
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-speech-api/#dom-speechrecognitionerrorcode-not-allowed">Web Speech API: 4.1.6 SpeechRecognitionErrorEvent</see>
    /// </remarks>
    [Description("@#not-allowed")]
    NotAllowed = 4,

    /// <summary>
    /// &quot;service-not-allowed&quot;
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-speech-api/#dom-speechrecognitionerrorcode-service-not-allowed">Web Speech API: 4.1.6 SpeechRecognitionErrorEvent</see>
    /// </remarks>
    [Description("@#service-not-allowed")]
    ServiceNotAllowed = 5,

    /// <summary>
    /// &quot;language-not-supported&quot;
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-speech-api/#dom-speechrecognitionerrorcode-language-not-supported">Web Speech API: 4.1.6 SpeechRecognitionErrorEvent</see>
    /// </remarks>
    [Description("@#language-not-supported")]
    LanguageNotSupported = 6,

    /// <summary>
    /// &quot;phrases-not-supported&quot;
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-speech-api/#dom-speechrecognitionerrorcode-phrases-not-supported">Web Speech API: 4.1.6 SpeechRecognitionErrorEvent</see>
    /// </remarks>
    [Description("@#phrases-not-supported")]
    PhrasesNotSupported = 7
}

/// <summary>
/// <see href="https://webaudio.github.io/web-speech-api/#enumdef-speechrecognitionquality">Web Speech API: 4.1 The SpeechRecognition Interface</see>
/// </summary>
[Description("@#SpeechRecognitionQuality")]
[ECMAScript]
[String]
public enum SpeechRecognitionQuality
{
    /// <summary>
    /// <see href="https://webaudio.github.io/web-speech-api/#dom-speechrecognitionquality-command">Web Speech API: 4.1.4 SpeechRecognitionQuality Enum Values</see>
    /// </summary>
    [Description("@#command")]
    Command = 0,

    /// <summary>
    /// <see href="https://webaudio.github.io/web-speech-api/#dom-speechrecognitionquality-dictation">Web Speech API: 4.1.4 SpeechRecognitionQuality Enum Values</see>
    /// </summary>
    [Description("@#dictation")]
    Dictation = 1,

    /// <summary>
    /// &quot;conversation&quot;
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-speech-api/#dom-speechrecognitionquality-conversation">Web Speech API: 4.1.4 SpeechRecognitionQuality Enum Values</see>
    /// </remarks>
    [Description("@#conversation")]
    Conversation = 2
}

/// <summary>
/// <see href="https://webaudio.github.io/web-speech-api/#enumdef-speechsynthesiserrorcode">Web Speech API: 4.2 The SpeechSynthesis Interface</see>
/// </summary>
[Description("@#SpeechSynthesisErrorCode")]
[ECMAScript]
[String]
public enum SpeechSynthesisErrorCode
{
    /// <summary>
    /// <see href="https://webaudio.github.io/web-speech-api/#dom-speechsynthesiserrorcode-canceled">Web Speech API: 4.2.7 SpeechSynthesisErrorEvent Attributes</see>
    /// </summary>
    [Description("@#canceled")]
    Canceled = 0,

    /// <summary>
    /// &quot;interrupted&quot;
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-speech-api/#dom-speechsynthesiserrorcode-interrupted">Web Speech API: 4.2.7 SpeechSynthesisErrorEvent Attributes</see>
    /// </remarks>
    [Description("@#interrupted")]
    Interrupted = 1,

    /// <summary>
    /// &quot;audio-busy&quot;
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-speech-api/#dom-speechsynthesiserrorcode-audio-busy">Web Speech API: 4.2.7 SpeechSynthesisErrorEvent Attributes</see>
    /// </remarks>
    [Description("@#audio-busy")]
    AudioBusy = 2,

    /// <summary>
    /// &quot;audio-hardware&quot;
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-speech-api/#dom-speechsynthesiserrorcode-audio-hardware">Web Speech API: 4.2.7 SpeechSynthesisErrorEvent Attributes</see>
    /// </remarks>
    [Description("@#audio-hardware")]
    AudioHardware = 3,

    /// <summary>
    /// <see href="https://webaudio.github.io/web-speech-api/#dom-speechsynthesiserrorcode-network">Web Speech API: 4.2.7 SpeechSynthesisErrorEvent Attributes</see>
    /// </summary>
    [Description("@#network")]
    Network = 4,

    /// <summary>
    /// &quot;synthesis-unavailable&quot;
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-speech-api/#dom-speechsynthesiserrorcode-synthesis-unavailable">Web Speech API: 4.2.7 SpeechSynthesisErrorEvent Attributes</see>
    /// </remarks>
    [Description("@#synthesis-unavailable")]
    SynthesisUnavailable = 5,

    /// <summary>
    /// &quot;synthesis-failed&quot;
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-speech-api/#dom-speechsynthesiserrorcode-synthesis-failed">Web Speech API: 4.2.7 SpeechSynthesisErrorEvent Attributes</see>
    /// </remarks>
    [Description("@#synthesis-failed")]
    SynthesisFailed = 6,

    /// <summary>
    /// &quot;language-unavailable&quot;
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-speech-api/#dom-speechsynthesiserrorcode-language-unavailable">Web Speech API: 4.2.7 SpeechSynthesisErrorEvent Attributes</see>
    /// </remarks>
    [Description("@#language-unavailable")]
    LanguageUnavailable = 7,

    /// <summary>
    /// &quot;voice-unavailable&quot;
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-speech-api/#dom-speechsynthesiserrorcode-voice-unavailable">Web Speech API: 4.2.7 SpeechSynthesisErrorEvent Attributes</see>
    /// </remarks>
    [Description("@#voice-unavailable")]
    VoiceUnavailable = 8,

    /// <summary>
    /// &quot;text-too-long&quot;
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-speech-api/#dom-speechsynthesiserrorcode-text-too-long">Web Speech API: 4.2.7 SpeechSynthesisErrorEvent Attributes</see>
    /// </remarks>
    [Description("@#text-too-long")]
    TextTooLong = 9,

    /// <summary>
    /// &quot;invalid-argument&quot;
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-speech-api/#dom-speechsynthesiserrorcode-invalid-argument">Web Speech API: 4.2.7 SpeechSynthesisErrorEvent Attributes</see>
    /// </remarks>
    [Description("@#invalid-argument")]
    InvalidArgument = 10,

    /// <summary>
    /// &quot;not-allowed&quot;
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-speech-api/#dom-speechsynthesiserrorcode-not-allowed">Web Speech API: 4.2.7 SpeechSynthesisErrorEvent Attributes</see>
    /// </remarks>
    [Description("@#not-allowed")]
    NotAllowed = 11
}

/// <summary>
/// <see href="https://webmachinelearning.github.io/prompt-api/#enumdef-languagemodelmessagerole">Prompt API: 3 The API</see>
/// </summary>
[Description("@#LanguageModelMessageRole")]
[ECMAScript]
[String]
public enum LanguageModelMessageRole
{
    /// <summary>
    /// <see href="https://webmachinelearning.github.io/prompt-api/#dom-languagemodelmessagerole-system">Prompt API: 3 The API</see>
    /// </summary>
    [Description("@#system")]
    System = 0,

    /// <summary>
    /// <see href="https://webmachinelearning.github.io/prompt-api/#dom-languagemodelmessagerole-user">Prompt API: 3 The API</see>
    /// </summary>
    [Description("@#user")]
    User = 1,

    /// <summary>
    /// <see href="https://webmachinelearning.github.io/prompt-api/#dom-languagemodelmessagerole-assistant">Prompt API: 3 The API</see>
    /// </summary>
    [Description("@#assistant")]
    Assistant = 2
}

/// <summary>
/// <see href="https://webmachinelearning.github.io/prompt-api/#enumdef-languagemodelmessagetype">Prompt API: 3 The API</see>
/// </summary>
[Description("@#LanguageModelMessageType")]
[ECMAScript]
[String]
public enum LanguageModelMessageType
{
    /// <summary>
    /// <see href="https://webmachinelearning.github.io/prompt-api/#dom-languagemodelmessagetype-text">Prompt API: 3 The API</see>
    /// </summary>
    [Description("@#text")]
    Text = 0,

    /// <summary>
    /// <see href="https://webmachinelearning.github.io/prompt-api/#dom-languagemodelmessagetype-image">Prompt API: 3 The API</see>
    /// </summary>
    [Description("@#image")]
    Image = 1,

    /// <summary>
    /// <see href="https://webmachinelearning.github.io/prompt-api/#dom-languagemodelmessagetype-audio">Prompt API: 3 The API</see>
    /// </summary>
    [Description("@#audio")]
    Audio = 2,

    /// <summary>
    /// <see href="https://webmachinelearning.github.io/prompt-api/#dom-languagemodelmessagetype-tool-call">Prompt API: 3 The API</see>
    /// </summary>
    [Description("@#tool-call")]
    ToolCall = 3,

    /// <summary>
    /// <see href="https://webmachinelearning.github.io/prompt-api/#dom-languagemodelmessagetype-tool-response">Prompt API: 3 The API</see>
    /// </summary>
    [Description("@#tool-response")]
    ToolResponse = 4
}

/// <summary>
/// <see href="https://webmachinelearning.github.io/prompt-api/#enumdef-languagemodelsamplingmode">Prompt API: 3 The API</see>
/// </summary>
[Description("@#LanguageModelSamplingMode")]
[ECMAScript]
[String]
public enum LanguageModelSamplingMode
{
    /// <summary>
    /// <see href="https://webmachinelearning.github.io/prompt-api/#dom-languagemodelsamplingmode-most-predictable">Prompt API: 3 The API</see>
    /// </summary>
    [Description("@#most-predictable")]
    MostPredictable = 0,

    /// <summary>
    /// <see href="https://webmachinelearning.github.io/prompt-api/#dom-languagemodelsamplingmode-predictable">Prompt API: 3 The API</see>
    /// </summary>
    [Description("@#predictable")]
    Predictable = 1,

    /// <summary>
    /// <see href="https://webmachinelearning.github.io/prompt-api/#dom-languagemodelsamplingmode-balanced">Prompt API: 3 The API</see>
    /// </summary>
    [Description("@#balanced")]
    Balanced = 2,

    /// <summary>
    /// <see href="https://webmachinelearning.github.io/prompt-api/#dom-languagemodelsamplingmode-creative">Prompt API: 3 The API</see>
    /// </summary>
    [Description("@#creative")]
    Creative = 3,

    /// <summary>
    /// <see href="https://webmachinelearning.github.io/prompt-api/#dom-languagemodelsamplingmode-most-creative">Prompt API: 3 The API</see>
    /// </summary>
    [Description("@#most-creative")]
    MostCreative = 4
}

/// <summary>
/// <see href="https://webmachinelearning.github.io/webnn/#enumdef-mlconv2dfilteroperandlayout">Web Neural Network API: 8.9.10 conv2d</see>
/// </summary>
[Description("@#MLConv2dFilterOperandLayout")]
[ECMAScript]
[String]
public enum MLConv2dFilterOperandLayout
{
    /// <summary>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mlconv2dfilteroperandlayout-oihw">Web Neural Network API: 8.9.10 conv2d</see>
    /// </summary>
    [Description("@#oihw")]
    Oihw = 0,

    /// <summary>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mlconv2dfilteroperandlayout-hwio">Web Neural Network API: 8.9.10 conv2d</see>
    /// </summary>
    [Description("@#hwio")]
    Hwio = 1,

    /// <summary>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mlconv2dfilteroperandlayout-ohwi">Web Neural Network API: 8.9.10 conv2d</see>
    /// </summary>
    [Description("@#ohwi")]
    Ohwi = 2,

    /// <summary>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mlconv2dfilteroperandlayout-ihwo">Web Neural Network API: 8.9.10 conv2d</see>
    /// </summary>
    [Description("@#ihwo")]
    Ihwo = 3
}

/// <summary>
/// <see href="https://webmachinelearning.github.io/webnn/#enumdef-mlconvtranspose2dfilteroperandlayout">Web Neural Network API: 8.9.11 convTranspose2d</see>
/// </summary>
[Description("@#MLConvTranspose2dFilterOperandLayout")]
[ECMAScript]
[String]
public enum MLConvTranspose2dFilterOperandLayout
{
    /// <summary>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mlconvtranspose2dfilteroperandlayout-iohw">Web Neural Network API: 8.9.11 convTranspose2d</see>
    /// </summary>
    [Description("@#iohw")]
    Iohw = 0,

    /// <summary>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mlconvtranspose2dfilteroperandlayout-hwoi">Web Neural Network API: 8.9.11 convTranspose2d</see>
    /// </summary>
    [Description("@#hwoi")]
    Hwoi = 1,

    /// <summary>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mlconvtranspose2dfilteroperandlayout-ohwi">Web Neural Network API: 8.9.11 convTranspose2d</see>
    /// </summary>
    [Description("@#ohwi")]
    Ohwi = 2
}

/// <summary>
/// <see href="https://webmachinelearning.github.io/webnn/#enumdef-mlgruweightlayout">Web Neural Network API: 8.9.25 gru</see>
/// </summary>
[Description("@#MLGruWeightLayout")]
[ECMAScript]
[String]
public enum MLGruWeightLayout
{
    /// <summary>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mlgruweightlayout-zrn">Web Neural Network API: 8.9.25 gru</see>
    /// </summary>
    [Description("@#zrn")]
    Zrn = 0,

    /// <summary>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mlgruweightlayout-rzn">Web Neural Network API: 8.9.25 gru</see>
    /// </summary>
    [Description("@#rzn")]
    Rzn = 1
}

/// <summary>
/// <see href="https://webmachinelearning.github.io/webnn/#enumdef-mlinputoperandlayout">Web Neural Network API: 8.5 MLOperandDescriptor dictionary</see>
/// </summary>
[Description("@#MLInputOperandLayout")]
[ECMAScript]
[String]
public enum MLInputOperandLayout
{
    /// <summary>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mlinputoperandlayout-nchw">Web Neural Network API: 8.5 MLOperandDescriptor dictionary</see>
    /// </summary>
    [Description("@#nchw")]
    Nchw = 0,

    /// <summary>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mlinputoperandlayout-nhwc">Web Neural Network API: 8.5 MLOperandDescriptor dictionary</see>
    /// </summary>
    [Description("@#nhwc")]
    Nhwc = 1
}

/// <summary>
/// <see href="https://webmachinelearning.github.io/webnn/#enumdef-mlinterpolationmode">Web Neural Network API: 8.9.41 resample2d</see>
/// </summary>
[Description("@#MLInterpolationMode")]
[ECMAScript]
[String]
public enum MLInterpolationMode
{
    /// <summary>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mlinterpolationmode-nearest-neighbor">Web Neural Network API: 8.9.41 resample2d</see>
    /// </summary>
    [Description("@#nearest-neighbor")]
    NearestNeighbor = 0,

    /// <summary>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mlinterpolationmode-linear">Web Neural Network API: 8.9.41 resample2d</see>
    /// </summary>
    [Description("@#linear")]
    Linear = 1
}

/// <summary>
/// <see href="https://webmachinelearning.github.io/webnn/#enumdef-mllstmweightlayout">Web Neural Network API: 8.9.33 lstm</see>
/// </summary>
[Description("@#MLLstmWeightLayout")]
[ECMAScript]
[String]
public enum MLLstmWeightLayout
{
    /// <summary>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mllstmweightlayout-iofg">Web Neural Network API: 8.9.33 lstm</see>
    /// </summary>
    [Description("@#iofg")]
    Iofg = 0,

    /// <summary>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mllstmweightlayout-ifgo">Web Neural Network API: 8.9.33 lstm</see>
    /// </summary>
    [Description("@#ifgo")]
    Ifgo = 1
}

/// <summary>
/// <see href="https://webmachinelearning.github.io/webnn/#enumdef-mloperanddatatype">Web Neural Network API: 8.5 MLOperandDescriptor dictionary</see>
/// </summary>
[Description("@#MLOperandDataType")]
[ECMAScript]
[String]
public enum MLOperandDataType
{
    /// <summary>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mloperanddatatype-float32">Web Neural Network API: 8.5 MLOperandDescriptor dictionary</see>
    /// </summary>
    [Description("@#float32")]
    Float32 = 0,

    /// <summary>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mloperanddatatype-float16">Web Neural Network API: 8.5 MLOperandDescriptor dictionary</see>
    /// </summary>
    [Description("@#float16")]
    Float16 = 1,

    /// <summary>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mloperanddatatype-int32">Web Neural Network API: 8.5 MLOperandDescriptor dictionary</see>
    /// </summary>
    [Description("@#int32")]
    Int32 = 2,

    /// <summary>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mloperanddatatype-uint32">Web Neural Network API: 8.5 MLOperandDescriptor dictionary</see>
    /// </summary>
    [Description("@#uint32")]
    Uint32 = 3,

    /// <summary>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mloperanddatatype-int64">Web Neural Network API: 8.5 MLOperandDescriptor dictionary</see>
    /// </summary>
    [Description("@#int64")]
    Int64 = 4,

    /// <summary>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mloperanddatatype-uint64">Web Neural Network API: 8.5 MLOperandDescriptor dictionary</see>
    /// </summary>
    [Description("@#uint64")]
    Uint64 = 5,

    /// <summary>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mloperanddatatype-int8">Web Neural Network API: 8.5 MLOperandDescriptor dictionary</see>
    /// </summary>
    [Description("@#int8")]
    Int8 = 6,

    /// <summary>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mloperanddatatype-uint8">Web Neural Network API: 8.5 MLOperandDescriptor dictionary</see>
    /// </summary>
    [Description("@#uint8")]
    Uint8 = 7
}

/// <summary>
/// <see href="https://webmachinelearning.github.io/webnn/#enumdef-mlpaddingmode">Web Neural Network API: 8.9.36 pad</see>
/// </summary>
[Description("@#MLPaddingMode")]
[ECMAScript]
[String]
public enum MLPaddingMode
{
    /// <summary>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mlpaddingmode-constant">Web Neural Network API: 8.9.36 pad</see>
    /// </summary>
    [Description("@#constant")]
    Constant = 0,

    /// <summary>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mlpaddingmode-edge">Web Neural Network API: 8.9.36 pad</see>
    /// </summary>
    [Description("@#edge")]
    Edge = 1,

    /// <summary>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mlpaddingmode-reflection">Web Neural Network API: 8.9.36 pad</see>
    /// </summary>
    [Description("@#reflection")]
    Reflection = 2
}

/// <summary>
/// <see href="https://webmachinelearning.github.io/webnn/#enumdef-mlpowerpreference">Web Neural Network API: 8.2.1 MLContextOptions</see>
/// </summary>
[Description("@#MLPowerPreference")]
[ECMAScript]
[String]
public enum MLPowerPreference
{
    /// <summary>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mlpowerpreference-default">Web Neural Network API: 8.2.1 MLContextOptions</see>
    /// </summary>
    [Description("@#default")]
    Default = 0,

    /// <summary>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mlpowerpreference-high-performance">Web Neural Network API: 8.2.1 MLContextOptions</see>
    /// </summary>
    [Description("@#high-performance")]
    HighPerformance = 1,

    /// <summary>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mlpowerpreference-low-power">Web Neural Network API: 8.2.1 MLContextOptions</see>
    /// </summary>
    [Description("@#low-power")]
    LowPower = 2
}

/// <summary>
/// <see href="https://webmachinelearning.github.io/webnn/#enumdef-mlrecurrentnetworkdirection">Web Neural Network API: 8.9.25 gru</see>
/// </summary>
[Description("@#MLRecurrentNetworkDirection")]
[ECMAScript]
[String]
public enum MLRecurrentNetworkDirection
{
    /// <summary>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mlrecurrentnetworkdirection-forward">Web Neural Network API: 8.9.25 gru</see>
    /// </summary>
    [Description("@#forward")]
    Forward = 0,

    /// <summary>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mlrecurrentnetworkdirection-backward">Web Neural Network API: 8.9.25 gru</see>
    /// </summary>
    [Description("@#backward")]
    Backward = 1,

    /// <summary>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mlrecurrentnetworkdirection-both">Web Neural Network API: 8.9.25 gru</see>
    /// </summary>
    [Description("@#both")]
    Both = 2
}

/// <summary>
/// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#enumdef-performancepreference">Writing Assistance APIs: 2 The summarizer API</see>
/// </summary>
[Description("@#PerformancePreference")]
[ECMAScript]
[String]
public enum PerformancePreference
{
    /// <summary>
    /// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#dom-performancepreference-auto">Writing Assistance APIs: 2.4.3 Options</see>
    /// </summary>
    [Description("@#auto")]
    Auto = 0,

    /// <summary>
    /// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#dom-performancepreference-speed">Writing Assistance APIs: 2.4.3 Options</see>
    /// </summary>
    [Description("@#speed")]
    Speed = 1,

    /// <summary>
    /// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#dom-performancepreference-capability">Writing Assistance APIs: 2.4.3 Options</see>
    /// </summary>
    [Description("@#capability")]
    Capability = 2
}

/// <summary>
/// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#enumdef-rewriterformat">Writing Assistance APIs: 4 The rewriter API</see>
/// </summary>
[Description("@#RewriterFormat")]
[ECMAScript]
[String]
public enum RewriterFormat
{
    /// <summary>
    /// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#dom-rewriterformat-as-is">Writing Assistance APIs: 4.4.3 Options</see>
    /// </summary>
    [Description("@#as-is")]
    AsIs = 0,

    /// <summary>
    /// The rewriting should preserve the format of the original text. &quot;plain-text&quot;
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#dom-rewriterformat-plain-text">Writing Assistance APIs: 4.4.3 Options</see>
    /// </remarks>
    [Description("@#plain-text")]
    PlainText = 1,

    /// <summary>
    /// The rewriting should convert the text to plain text, removing any formatting or markup language that may be present in the original. &quot;markdown&quot;
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#dom-rewriterformat-markdown">Writing Assistance APIs: 4.4.3 Options</see>
    /// </remarks>
    [Description("@#markdown")]
    Markdown = 2
}

/// <summary>
/// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#enumdef-rewriterlength">Writing Assistance APIs: 4 The rewriter API</see>
/// </summary>
[Description("@#RewriterLength")]
[ECMAScript]
[String]
public enum RewriterLength
{
    /// <summary>
    /// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#dom-rewriterlength-as-is">Writing Assistance APIs: 4.4.3 Options</see>
    /// </summary>
    [Description("@#as-is")]
    AsIs = 0,

    /// <summary>
    /// The rewriting should aim to preserve the approximate length of the original text. &quot;shorter&quot;
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#dom-rewriterlength-shorter">Writing Assistance APIs: 4.4.3 Options</see>
    /// </remarks>
    [Description("@#shorter")]
    Shorter = 1,

    /// <summary>
    /// The rewriting should make the text more concise than the original, omitting or shortening as necessary such that the end result is shorter. &quot;longer&quot;
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#dom-rewriterlength-longer">Writing Assistance APIs: 4.4.3 Options</see>
    /// </remarks>
    [Description("@#longer")]
    Longer = 2
}

/// <summary>
/// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#enumdef-rewritertone">Writing Assistance APIs: 4 The rewriter API</see>
/// </summary>
[Description("@#RewriterTone")]
[ECMAScript]
[String]
public enum RewriterTone
{
    /// <summary>
    /// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#dom-rewritertone-as-is">Writing Assistance APIs: 4.4.3 Options</see>
    /// </summary>
    [Description("@#as-is")]
    AsIs = 0,

    /// <summary>
    /// The rewriting should preserve the tone of the original text. &quot;more-formal&quot;
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#dom-rewritertone-more-formal">Writing Assistance APIs: 4.4.3 Options</see>
    /// </remarks>
    [Description("@#more-formal")]
    MoreFormal = 1,

    /// <summary>
    /// The rewriting should make the text more formal than the original, using more precise terminology, avoiding contractions and slang, and employing a more professional tone suitable for academic, business, or official contexts. &quot;more-casual&quot;
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#dom-rewritertone-more-casual">Writing Assistance APIs: 4.4.3 Options</see>
    /// </remarks>
    [Description("@#more-casual")]
    MoreCasual = 2
}

/// <summary>
/// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#enumdef-summarizerformat">Writing Assistance APIs: 2 The summarizer API</see>
/// </summary>
[Description("@#SummarizerFormat")]
[ECMAScript]
[String]
public enum SummarizerFormat
{
    /// <summary>
    /// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#dom-summarizerformat-plain-text">Writing Assistance APIs: 2.4.3 Options</see>
    /// </summary>
    [Description("@#plain-text")]
    PlainText = 0,

    /// <summary>
    /// The summary should not contain any formatting or markup language. &quot;markdown&quot;
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#dom-summarizerformat-markdown">Writing Assistance APIs: 2.4.3 Options</see>
    /// </remarks>
    [Description("@#markdown")]
    Markdown = 1
}

/// <summary>
/// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#enumdef-summarizerlength">Writing Assistance APIs: 2 The summarizer API</see>
/// </summary>
[Description("@#SummarizerLength")]
[ECMAScript]
[String]
public enum SummarizerLength
{
    /// <summary>
    /// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#dom-summarizerlength-short">Writing Assistance APIs: 2.4.3 Options</see>
    /// </summary>
    [Description("@#short")]
    Short = 0,

    /// <summary>
    /// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#dom-summarizerlength-medium">Writing Assistance APIs: 2.4.3 Options</see>
    /// </summary>
    [Description("@#medium")]
    Medium = 1,

    /// <summary>
    /// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#dom-summarizerlength-long">Writing Assistance APIs: 2.4.3 Options</see>
    /// </summary>
    [Description("@#long")]
    Long = 2
}

/// <summary>
/// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#enumdef-summarizertype">Writing Assistance APIs: 2 The summarizer API</see>
/// </summary>
[Description("@#SummarizerType")]
[ECMAScript]
[String]
public enum SummarizerType
{
    /// <summary>
    /// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#dom-summarizertype-tldr">Writing Assistance APIs: 2.4.3 Options</see>
    /// </summary>
    [Description("@#tldr")]
    Tldr = 0,

    /// <summary>
    /// The summary should be short and to the point, providing a quick overview of the input, suitable for a busy reader. &quot;teaser&quot;
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#dom-summarizertype-teaser">Writing Assistance APIs: 2.4.3 Options</see>
    /// </remarks>
    [Description("@#teaser")]
    Teaser = 1,

    /// <summary>
    /// The summary should focus on the most interesting or intriguing parts of the input, designed to draw the reader in to read more. &quot;key-points&quot;
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#dom-summarizertype-key-points">Writing Assistance APIs: 2.4.3 Options</see>
    /// </remarks>
    [Description("@#key-points")]
    KeyPoints = 2,

    /// <summary>
    /// The summary should extract the most important points from the input, presented as a bulleted list. &quot;headline&quot;
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#dom-summarizertype-headline">Writing Assistance APIs: 2.4.3 Options</see>
    /// </remarks>
    [Description("@#headline")]
    Headline = 3
}

/// <summary>
/// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#enumdef-writerformat">Writing Assistance APIs: 3 The writer API</see>
/// </summary>
[Description("@#WriterFormat")]
[ECMAScript]
[String]
public enum WriterFormat
{
    /// <summary>
    /// The rewriting should preserve the format of the original text. &quot;plain-text&quot;
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#dom-writerformat-plain-text">Writing Assistance APIs: 3.4.3 Options</see>
    /// </remarks>
    [Description("@#plain-text")]
    PlainText = 0,

    /// <summary>
    /// The writing should not contain any formatting or markup language. &quot;markdown&quot;
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#dom-writerformat-markdown">Writing Assistance APIs: 3.4.3 Options</see>
    /// </remarks>
    [Description("@#markdown")]
    Markdown = 1
}

/// <summary>
/// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#enumdef-writerlength">Writing Assistance APIs: 3 The writer API</see>
/// </summary>
[Description("@#WriterLength")]
[ECMAScript]
[String]
public enum WriterLength
{
    /// <summary>
    /// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#dom-writerlength-short">Writing Assistance APIs: 3.4.3 Options</see>
    /// </summary>
    [Description("@#short")]
    Short = 0,

    /// <summary>
    /// The writing should be concise and to the point, using no more than 100 words. &quot;medium&quot;
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#dom-writerlength-medium">Writing Assistance APIs: 3.4.3 Options</see>
    /// </remarks>
    [Description("@#medium")]
    Medium = 1,

    /// <summary>
    /// The writing should be moderately detailed, using no more than 300 words. &quot;long&quot;
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#dom-writerlength-long">Writing Assistance APIs: 3.4.3 Options</see>
    /// </remarks>
    [Description("@#long")]
    Long = 2
}

/// <summary>
/// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#enumdef-writertone">Writing Assistance APIs: 3 The writer API</see>
/// </summary>
[Description("@#WriterTone")]
[ECMAScript]
[String]
public enum WriterTone
{
    /// <summary>
    /// The writing should use formal language, employing precise terminology, avoiding contractions and slang, and maintaining a professional tone suitable for academic, business, or official contexts. &quot;neutral&quot;
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#dom-writertone-formal">Writing Assistance APIs: 3.4.3 Options</see>
    /// </remarks>
    [Description("@#formal")]
    Formal = 0,

    /// <summary>
    /// The writing should use formal language, employing precise terminology, avoiding contractions and slang, and maintaining a professional tone suitable for academic, business, or official contexts. &quot;neutral&quot;
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#dom-writertone-neutral">Writing Assistance APIs: 3.4.3 Options</see>
    /// </remarks>
    [Description("@#neutral")]
    Neutral = 1,

    /// <summary>
    /// The writing should use a balanced, moderate tone that is neither overly formal nor casual, suitable for general audiences and informational contexts. &quot;casual&quot;
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#dom-writertone-casual">Writing Assistance APIs: 3.4.3 Options</see>
    /// </remarks>
    [Description("@#casual")]
    Casual = 2
}

/// <summary>
/// <see href="https://websockets.spec.whatwg.org/#enumdef-binarytype">WebSockets Standard: 3.1 Interface definition</see>
/// </summary>
[Description("@#BinaryType")]
[ECMAScript]
[String]
public enum BinaryType
{
    /// <summary>
    /// User agents can use the WebSocket/binary type as a hint for how to handle incoming binary data: if it is &quot;blob&quot;, it is safe to spool it to disk, and if it is &quot;arraybuffer&quot;, it is likely more efficient to keep the data in memory. Naturally, user agents are encouraged to use more subtle heuristics to decide whether to keep incoming data in memory or not, e.g. based on how big the data is or how common it is for a script to change the attribute at the last minute....
    /// </summary>
    /// <remarks>
    /// <see href="https://websockets.spec.whatwg.org/#dom-binarytype-blob">WebSockets Standard: 4 Feedback from the protocol</see>
    /// </remarks>
    /// <example>
    /// <code>&quot;blob&quot;</code>
    /// </example>
    [Description("@#blob")]
    Blob = 0,

    /// <summary>
    /// User agents can use the WebSocket/binary type as a hint for how to handle incoming binary data: if it is &quot;blob&quot;, it is safe to spool it to disk, and if it is &quot;arraybuffer&quot;, it is likely more efficient to keep the data in memory. Naturally, user agents are encouraged to use more subtle heuristics to decide whether to keep incoming data in memory or not, e.g. based on how big the data is or how common it is for a script to change the attribute at the last minute....
    /// </summary>
    /// <remarks>
    /// <see href="https://websockets.spec.whatwg.org/#dom-binarytype-arraybuffer">WebSockets Standard: 4 Feedback from the protocol</see>
    /// </remarks>
    /// <example>
    /// <code>&quot;arraybuffer&quot;</code>
    /// </example>
    [Description("@#arraybuffer")]
    Arraybuffer = 1
}

/// <summary>
/// <see href="https://wicg.github.io/PEPC/geolocation-element.html#enumdef-activationblockersmixinblockerreason">The HTML Geolocation Element: 3.1.2 Action Blockers, Blocker Reasons, and Blocker Lifetimes</see>
/// </summary>
[Description("@#ActivationBlockersMixinBlockerReason")]
[ECMAScript]
[String]
public enum ActivationBlockersMixinBlockerReason
{
    [Description("@#")]
    Empty = 0,

    /// <summary>
    /// <see href="https://wicg.github.io/PEPC/geolocation-element.html#dom-activationblockersmixinblockerreason-illegal_subframe">The HTML Geolocation Element: 3.1.2 Action Blockers, Blocker Reasons, and Blocker Lifetimes</see>
    /// </summary>
    [Description("@#illegal_subframe")]
    IllegalSubframe = 1,

    /// <summary>
    /// <see href="https://wicg.github.io/PEPC/geolocation-element.html#dom-activationblockersmixinblockerreason-unsuccessful_registration">The HTML Geolocation Element: 3.1.2 Action Blockers, Blocker Reasons, and Blocker Lifetimes</see>
    /// </summary>
    [Description("@#unsuccessful_registration")]
    UnsuccessfulRegistration = 2,

    /// <summary>
    /// <see href="https://wicg.github.io/PEPC/geolocation-element.html#dom-activationblockersmixinblockerreason-recently_attached">The HTML Geolocation Element: 3.1.2 Action Blockers, Blocker Reasons, and Blocker Lifetimes</see>
    /// </summary>
    [Description("@#recently_attached")]
    RecentlyAttached = 3,

    /// <summary>
    /// <see href="https://wicg.github.io/PEPC/geolocation-element.html#dom-activationblockersmixinblockerreason-intersection_changed">The HTML Geolocation Element: 3.1.2 Action Blockers, Blocker Reasons, and Blocker Lifetimes</see>
    /// </summary>
    [Description("@#intersection_changed")]
    IntersectionChanged = 4,

    /// <summary>
    /// <see href="https://wicg.github.io/PEPC/geolocation-element.html#dom-activationblockersmixinblockerreason-intersection_out_of_viewport_or_clipped">The HTML Geolocation Element: 3.1.2 Action Blockers, Blocker Reasons, and Blocker Lifetimes</see>
    /// </summary>
    [Description("@#intersection_out_of_viewport_or_clipped")]
    IntersectionOutOfViewportOrClipped = 5,

    /// <summary>
    /// <see href="https://wicg.github.io/PEPC/geolocation-element.html#dom-activationblockersmixinblockerreason-intersection_occluded_or_distorted">The HTML Geolocation Element: 3.1.2 Action Blockers, Blocker Reasons, and Blocker Lifetimes</see>
    /// </summary>
    [Description("@#intersection_occluded_or_distorted")]
    IntersectionOccludedOrDistorted = 6,

    /// <summary>
    /// <see href="https://wicg.github.io/PEPC/geolocation-element.html#dom-activationblockersmixinblockerreason-style_invalid">The HTML Geolocation Element: 3.1.2 Action Blockers, Blocker Reasons, and Blocker Lifetimes</see>
    /// </summary>
    [Description("@#style_invalid")]
    StyleInvalid = 7,

    /// <summary>
    /// <see href="https://wicg.github.io/PEPC/geolocation-element.html#dom-activationblockersmixinblockerreason-type_invalid">The HTML Geolocation Element: 3.1.2 Action Blockers, Blocker Reasons, and Blocker Lifetimes</see>
    /// </summary>
    [Description("@#type_invalid")]
    TypeInvalid = 8
}

/// <summary>
/// <see href="https://wicg.github.io/background-fetch/#enumdef-backgroundfetchfailurereason">Background Fetch: 6.4 BackgroundFetchRegistration</see>
/// </summary>
[Description("@#BackgroundFetchFailureReason")]
[ECMAScript]
[String]
public enum BackgroundFetchFailureReason
{
    [Description("@#")]
    Empty = 0,

    /// <summary>
    /// <see href="https://wicg.github.io/background-fetch/#dom-backgroundfetchfailurereason-aborted">Background Fetch: 6.4 BackgroundFetchRegistration</see>
    /// </summary>
    [Description("@#aborted")]
    Aborted = 1,

    /// <summary>
    /// <see href="https://wicg.github.io/background-fetch/#dom-backgroundfetchfailurereason-bad-status">Background Fetch: 6.4 BackgroundFetchRegistration</see>
    /// </summary>
    [Description("@#bad-status")]
    BadStatus = 2,

    /// <summary>
    /// <see href="https://wicg.github.io/background-fetch/#dom-backgroundfetchfailurereason-fetch-error">Background Fetch: 6.4 BackgroundFetchRegistration</see>
    /// </summary>
    [Description("@#fetch-error")]
    FetchError = 3,

    /// <summary>
    /// <see href="https://wicg.github.io/background-fetch/#dom-backgroundfetchfailurereason-quota-exceeded">Background Fetch: 6.4 BackgroundFetchRegistration</see>
    /// </summary>
    [Description("@#quota-exceeded")]
    QuotaExceeded = 4,

    /// <summary>
    /// <see href="https://wicg.github.io/background-fetch/#dom-backgroundfetchfailurereason-download-total-exceeded">Background Fetch: 6.4 BackgroundFetchRegistration</see>
    /// </summary>
    [Description("@#download-total-exceeded")]
    DownloadTotalExceeded = 5
}

/// <summary>
/// <see href="https://wicg.github.io/background-fetch/#enumdef-backgroundfetchresult">Background Fetch: 6.4 BackgroundFetchRegistration</see>
/// </summary>
[Description("@#BackgroundFetchResult")]
[ECMAScript]
[String]
public enum BackgroundFetchResult
{
    [Description("@#")]
    Empty = 0,

    /// <summary>
    /// <see href="https://wicg.github.io/background-fetch/#dom-backgroundfetchresult-success">Background Fetch: 6.4 BackgroundFetchRegistration</see>
    /// </summary>
    [Description("@#success")]
    Success = 1,

    /// <summary>
    /// <see href="https://wicg.github.io/background-fetch/#dom-backgroundfetchresult-failure">Background Fetch: 6.4 BackgroundFetchRegistration</see>
    /// </summary>
    [Description("@#failure")]
    Failure = 2
}

/// <summary>
/// <see href="https://wicg.github.io/connection-allowlists/#enumdef-connectionallowlistdisposition">Connection Allowlists: 3.3 Reporting</see>
/// </summary>
[Description("@#ConnectionAllowlistDisposition")]
[ECMAScript]
[String]
public enum ConnectionAllowlistDisposition
{
    /// <summary>
    /// <see href="https://wicg.github.io/connection-allowlists/#dom-connectionallowlistdisposition-enforce">Connection Allowlists: 3.3 Reporting</see>
    /// </summary>
    [Description("@#enforce")]
    Enforce = 0,

    /// <summary>
    /// <see href="https://wicg.github.io/connection-allowlists/#dom-connectionallowlistdisposition-report">Connection Allowlists: 3.3 Reporting</see>
    /// </summary>
    [Description("@#report")]
    Report = 1
}

/// <summary>
/// <see href="https://wicg.github.io/content-index/spec/#enumdef-contentcategory">Content Index: 5.3 ContentIndex</see>
/// </summary>
[Description("@#ContentCategory")]
[ECMAScript]
[String]
public enum ContentCategory
{
    [Description("@#")]
    Empty = 0,

    /// <summary>
    /// <see href="https://wicg.github.io/content-index/spec/#dom-contentcategory-homepage">Content Index: 5.3 ContentIndex</see>
    /// </summary>
    [Description("@#homepage")]
    Homepage = 1,

    /// <summary>
    /// <see href="https://wicg.github.io/content-index/spec/#dom-contentcategory-article">Content Index: 5.3 ContentIndex</see>
    /// </summary>
    [Description("@#article")]
    Article = 2,

    /// <summary>
    /// <see href="https://wicg.github.io/content-index/spec/#dom-contentcategory-video">Content Index: 5.3 ContentIndex</see>
    /// </summary>
    [Description("@#video")]
    Video = 3,

    /// <summary>
    /// <see href="https://wicg.github.io/content-index/spec/#dom-contentcategory-audio">Content Index: 5.3 ContentIndex</see>
    /// </summary>
    [Description("@#audio")]
    Audio = 4
}

/// <summary>
/// <see href="https://wicg.github.io/csp-next/scripting-policy.html#enumdef-scriptingpolicyviolationtype">Scripting Policy: 2.7 Reporting Violations</see>
/// </summary>
[Description("@#ScriptingPolicyViolationType")]
[ECMAScript]
[String]
public enum ScriptingPolicyViolationType
{
    /// <summary>
    /// <see href="https://wicg.github.io/csp-next/scripting-policy.html#dom-scriptingpolicyviolationtype-externalscript">Scripting Policy: 2.7 Reporting Violations</see>
    /// </summary>
    [Description("@#externalScript")]
    ExternalScript = 0,

    /// <summary>
    /// <see href="https://wicg.github.io/csp-next/scripting-policy.html#dom-scriptingpolicyviolationtype-inlinescript">Scripting Policy: 2.7 Reporting Violations</see>
    /// </summary>
    [Description("@#inlineScript")]
    InlineScript = 1,

    /// <summary>
    /// <see href="https://wicg.github.io/csp-next/scripting-policy.html#dom-scriptingpolicyviolationtype-inlineeventhandler">Scripting Policy: 2.7 Reporting Violations</see>
    /// </summary>
    [Description("@#inlineEventHandler")]
    InlineEventHandler = 2,

    /// <summary>
    /// <see href="https://wicg.github.io/csp-next/scripting-policy.html#dom-scriptingpolicyviolationtype-eval">Scripting Policy: 2.7 Reporting Violations</see>
    /// </summary>
    [Description("@#eval")]
    Eval = 3
}

/// <summary>
/// <see href="https://wicg.github.io/digital-goods/#enumdef-itemtype">Digital Goods API: 2.2 DigitalGoodsService interface</see>
/// </summary>
[Description("@#ItemType")]
[ECMAScript]
[String]
public enum ItemType
{
    /// <summary>
    /// <see href="https://wicg.github.io/digital-goods/#dom-itemtype-product">Digital Goods API: 2.2 DigitalGoodsService interface</see>
    /// </summary>
    [Description("@#product")]
    Product = 0,

    /// <summary>
    /// <see href="https://wicg.github.io/digital-goods/#dom-itemtype-subscription">Digital Goods API: 2.2 DigitalGoodsService interface</see>
    /// </summary>
    [Description("@#subscription")]
    Subscription = 1
}

/// <summary>
/// <see href="https://wicg.github.io/fenced-frame/#enumdef-fencereportingdestination">Fenced Frame: 2.4 The Fence interface</see>
/// </summary>
[Description("@#FenceReportingDestination")]
[ECMAScript]
[String]
public enum FenceReportingDestination
{
    /// <summary>
    /// <see href="https://wicg.github.io/fenced-frame/#dom-fencereportingdestination-buyer">Fenced Frame: 2.4 The Fence interface</see>
    /// </summary>
    [Description("@#buyer")]
    Buyer = 0,

    /// <summary>
    /// <see href="https://wicg.github.io/fenced-frame/#dom-fencereportingdestination-seller">Fenced Frame: 2.4 The Fence interface</see>
    /// </summary>
    [Description("@#seller")]
    Seller = 1,

    /// <summary>
    /// <see href="https://wicg.github.io/fenced-frame/#dom-fencereportingdestination-component-seller">Fenced Frame: 2.4 The Fence interface</see>
    /// </summary>
    [Description("@#component-seller")]
    ComponentSeller = 2,

    /// <summary>
    /// <see href="https://wicg.github.io/fenced-frame/#dom-fencereportingdestination-direct-seller">Fenced Frame: 2.4 The Fence interface</see>
    /// </summary>
    [Description("@#direct-seller")]
    DirectSeller = 3,

    /// <summary>
    /// <see href="https://wicg.github.io/fenced-frame/#dom-fencereportingdestination-shared-storage-select-url">Fenced Frame: 2.4 The Fence interface</see>
    /// </summary>
    [Description("@#shared-storage-select-url")]
    SharedStorageSelectUrl = 4
}

/// <summary>
/// <see href="https://wicg.github.io/fenced-frame/#enumdef-opaqueproperty">Fenced Frame: 2.3.5 The FencedFrameConfig interface</see>
/// </summary>
[Description("@#OpaqueProperty")]
[ECMAScript]
[String]
public enum OpaqueProperty
{
    /// <summary>
    /// <see href="https://wicg.github.io/fenced-frame/#dom-opaqueproperty-opaque">Fenced Frame: 2.3.5 The FencedFrameConfig interface</see>
    /// </summary>
    [Description("@#opaque")]
    Opaque = 0
}

/// <summary>
/// <see href="https://wicg.github.io/file-system-access/#enumdef-filesystempermissionmode">File System Access: 2.2 Permissions</see>
/// </summary>
[Description("@#FileSystemPermissionMode")]
[ECMAScript]
[String]
public enum FileSystemPermissionMode
{
    /// <summary>
    /// <see href="https://wicg.github.io/file-system-access/#dom-filesystempermissionmode-read">File System Access: 2.2 Permissions</see>
    /// </summary>
    /// <example>
    /// <code>status = await handle.queryPermission({ mode : &quot;read&quot; })</code>
    /// </example>
    [Description("@#read")]
    Read = 0,

    /// <summary>
    /// <see href="https://wicg.github.io/file-system-access/#dom-filesystempermissionmode-readwrite">File System Access: 2.2 Permissions</see>
    /// </summary>
    /// <example>
    /// <code>status = await handle.queryPermission({ mode : &quot;readwrite&quot; })</code>
    /// </example>
    [Description("@#readwrite")]
    Readwrite = 1
}

/// <summary>
/// <see href="https://wicg.github.io/file-system-access/#enumdef-wellknowndirectory">File System Access: 3.2.2 Starting Directory</see>
/// </summary>
[Description("@#WellKnownDirectory")]
[ECMAScript]
[String]
public enum WellKnownDirectory
{
    /// <summary>
    /// <see href="https://wicg.github.io/file-system-access/#dom-wellknowndirectory-desktop">File System Access: 3.2.2 Starting Directory</see>
    /// </summary>
    [Description("@#desktop")]
    Desktop = 0,

    /// <summary>
    /// <see href="https://wicg.github.io/file-system-access/#dom-wellknowndirectory-documents">File System Access: 3.2.2 Starting Directory</see>
    /// </summary>
    [Description("@#documents")]
    Documents = 1,

    /// <summary>
    /// <see href="https://wicg.github.io/file-system-access/#dom-wellknowndirectory-downloads">File System Access: 3.2.2 Starting Directory</see>
    /// </summary>
    [Description("@#downloads")]
    Downloads = 2,

    /// <summary>
    /// <see href="https://wicg.github.io/file-system-access/#dom-wellknowndirectory-music">File System Access: 3.2.2 Starting Directory</see>
    /// </summary>
    [Description("@#music")]
    Music = 3,

    /// <summary>
    /// <see href="https://wicg.github.io/file-system-access/#dom-wellknowndirectory-pictures">File System Access: 3.2.2 Starting Directory</see>
    /// </summary>
    [Description("@#pictures")]
    Pictures = 4,

    /// <summary>
    /// <see href="https://wicg.github.io/file-system-access/#dom-wellknowndirectory-videos">File System Access: 3.2.2 Starting Directory</see>
    /// </summary>
    [Description("@#videos")]
    Videos = 5
}

/// <summary>
/// <see href="https://wicg.github.io/idle-detection/#enumdef-screenidlestate">Idle Detection API: 2.1.2 The ScreenIdleState enum</see>
/// </summary>
[Description("@#ScreenIdleState")]
[ECMAScript]
[String]
public enum ScreenIdleState
{
    /// <summary>
    /// <see href="https://wicg.github.io/idle-detection/#dom-screenidlestate-locked">Idle Detection API: 2.1.2 The ScreenIdleState enum</see>
    /// </summary>
    [Description("@#locked")]
    Locked = 0,

    /// <summary>
    /// <see href="https://wicg.github.io/idle-detection/#dom-screenidlestate-unlocked">Idle Detection API: 2.1.2 The ScreenIdleState enum</see>
    /// </summary>
    [Description("@#unlocked")]
    Unlocked = 1
}

/// <summary>
/// <see href="https://wicg.github.io/idle-detection/#enumdef-useridlestate">Idle Detection API: 2.1.1 The UserIdleState enum</see>
/// </summary>
[Description("@#UserIdleState")]
[ECMAScript]
[String]
public enum UserIdleState
{
    /// <summary>
    /// <see href="https://wicg.github.io/idle-detection/#dom-useridlestate-active">Idle Detection API: 2.1.1 The UserIdleState enum</see>
    /// </summary>
    [Description("@#active")]
    Active = 0,

    /// <summary>
    /// <see href="https://wicg.github.io/idle-detection/#dom-useridlestate-idle">Idle Detection API: 2.1.1 The UserIdleState enum</see>
    /// </summary>
    [Description("@#idle")]
    Idle = 1
}

/// <summary>
/// <see href="https://wicg.github.io/local-network-access/#enumdef-ipaddressspace">Local Network Access: 2.1 IP Address Space</see>
/// </summary>
[Description("@#IPAddressSpace")]
[ECMAScript]
[String]
public enum IPAddressSpace
{
    /// <summary>
    /// <see href="https://wicg.github.io/local-network-access/#dom-ipaddressspace-public">Local Network Access: 2.1 IP Address Space</see>
    /// </summary>
    [Description("@#public")]
    Public = 0,

    /// <summary>
    /// Set request&apos;s targetAddressSpace to IP address space/local.
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/local-network-access/#dom-ipaddressspace-local">Local Network Access: 2.1 IP Address Space</see>
    /// </remarks>
    [Description("@#local")]
    Local = 1,

    /// <summary>
    /// <see href="https://wicg.github.io/local-network-access/#dom-ipaddressspace-loopback">Local Network Access: 2.1 IP Address Space</see>
    /// </summary>
    [Description("@#loopback")]
    Loopback = 2
}

/// <summary>
/// <see href="https://wicg.github.io/netinfo/#dom-connectiontype">Network Information API: 4.2 ConnectionType enum</see>
/// </summary>
[Description("@#ConnectionType")]
[ECMAScript]
[String]
public enum ConnectionType
{
    /// <summary>
    /// <see href="https://wicg.github.io/netinfo/#dom-connectiontype-bluetooth">Network Information API: 4.1 Underlying connection technology</see>
    /// </summary>
    [Description("@#bluetooth")]
    Bluetooth = 0,

    /// <summary>
    /// <see href="https://wicg.github.io/netinfo/#dom-connectiontype-cellular">Network Information API: 4.1 Underlying connection technology</see>
    /// </summary>
    [Description("@#cellular")]
    Cellular = 1,

    /// <summary>
    /// <see href="https://wicg.github.io/netinfo/#dom-connectiontype-ethernet">Network Information API: 4.1 Underlying connection technology</see>
    /// </summary>
    [Description("@#ethernet")]
    Ethernet = 2,

    /// <summary>
    /// <see href="https://wicg.github.io/netinfo/#dom-connectiontype-mixed">Network Information API: 4.1 Underlying connection technology</see>
    /// </summary>
    [Description("@#mixed")]
    Mixed = 3,

    /// <summary>
    /// <see href="https://wicg.github.io/netinfo/#dom-connectiontype-none">Network Information API: 4.1 Underlying connection technology</see>
    /// </summary>
    [Description("@#none")]
    None = 4,

    /// <summary>
    /// <see href="https://wicg.github.io/netinfo/#dom-connectiontype-other">Network Information API: 4.1 Underlying connection technology</see>
    /// </summary>
    [Description("@#other")]
    Other = 5,

    /// <summary>
    /// <see href="https://wicg.github.io/netinfo/#dom-connectiontype-unknown">Network Information API: 4.1 Underlying connection technology</see>
    /// </summary>
    [Description("@#unknown")]
    Unknown = 6,

    /// <summary>
    /// <see href="https://wicg.github.io/netinfo/#dom-connectiontype-wifi">Network Information API: 4.1 Underlying connection technology</see>
    /// </summary>
    [Description("@#wifi")]
    Wifi = 7,

    /// <summary>
    /// <see href="https://wicg.github.io/netinfo/#dom-connectiontype-wimax">Network Information API: 4.1 Underlying connection technology</see>
    /// </summary>
    [Description("@#wimax")]
    Wimax = 8
}

/// <summary>
/// <see href="https://wicg.github.io/netinfo/#dom-effectiveconnectiontype">Network Information API: 4.4 EffectiveConnectionType enum</see>
/// </summary>
[Description("@#EffectiveConnectionType")]
[ECMAScript]
[String]
public enum EffectiveConnectionType
{
    /// <summary>
    /// <see href="https://wicg.github.io/netinfo/#dom-effectiveconnectiontype-2g">Network Information API: 4.3 Effective connection types</see>
    /// </summary>
    [Description("@#2g")]
    _2g = 0,

    /// <summary>
    /// <see href="https://wicg.github.io/netinfo/#dom-effectiveconnectiontype-3g">Network Information API: 4.3 Effective connection types</see>
    /// </summary>
    [Description("@#3g")]
    _3g = 1,

    /// <summary>
    /// <see href="https://wicg.github.io/netinfo/#dom-effectiveconnectiontype-4g">Network Information API: 4.3 Effective connection types</see>
    /// </summary>
    [Description("@#4g")]
    _4g = 2,

    /// <summary>
    /// <see href="https://wicg.github.io/netinfo/#dom-effectiveconnectiontype-slow-2g">Network Information API: 4.3 Effective connection types</see>
    /// </summary>
    [Description("@#slow-2g")]
    Slow2g = 3
}

/// <summary>
/// <see href="https://wicg.github.io/page-lifecycle/#enumdef-clientlifecyclestate">Page Lifecycle: 5.3.1 Client</see>
/// </summary>
[Description("@#ClientLifecycleState")]
[ECMAScript]
[String]
public enum ClientLifecycleState
{
    /// <summary>
    /// <see href="https://wicg.github.io/page-lifecycle/#dom-clientlifecyclestate-active">Page Lifecycle: 5.3.1 Client</see>
    /// </summary>
    [Description("@#active")]
    Active = 0,

    /// <summary>
    /// <see href="https://wicg.github.io/page-lifecycle/#dom-clientlifecyclestate-frozen">Page Lifecycle: 5.3.1 Client</see>
    /// </summary>
    [Description("@#frozen")]
    Frozen = 1
}

/// <summary>
/// <see href="https://wicg.github.io/scheduling-apis/#enumdef-taskpriority">Prioritized Task Scheduling: 2.1 Task and Continuation Priorities</see>
/// </summary>
[Description("@#TaskPriority")]
[ECMAScript]
[String]
public enum TaskPriority
{
    /// <summary>
    /// <see href="https://wicg.github.io/scheduling-apis/#dom-taskpriority-user-blocking">Prioritized Task Scheduling: 2.1 Task and Continuation Priorities</see>
    /// </summary>
    [Description("@#user-blocking")]
    UserBlocking = 0,

    /// <summary>
    /// <see href="https://wicg.github.io/scheduling-apis/#dom-taskpriority-user-visible">Prioritized Task Scheduling: 2.1 Task and Continuation Priorities</see>
    /// </summary>
    [Description("@#user-visible")]
    UserVisible = 1,

    /// <summary>
    /// <see href="https://wicg.github.io/scheduling-apis/#dom-taskpriority-background">Prioritized Task Scheduling: 2.1 Task and Continuation Priorities</see>
    /// </summary>
    [Description("@#background")]
    Background = 2
}

/// <summary>
/// <see href="https://wicg.github.io/serial/#dom-flowcontroltype">Web Serial API: 4.4.1.2 FlowControlType enum</see>
/// </summary>
[Description("@#FlowControlType")]
[ECMAScript]
[String]
public enum FlowControlType
{
    /// <summary>
    /// <see href="https://wicg.github.io/serial/#dom-flowcontroltype-none">Web Serial API: 4.4.1.2 FlowControlType enum</see>
    /// </summary>
    [Description("@#none")]
    None = 0,

    /// <summary>
    /// <see href="https://wicg.github.io/serial/#dom-flowcontroltype-hardware">Web Serial API: 4.4.1.2 FlowControlType enum</see>
    /// </summary>
    [Description("@#hardware")]
    Hardware = 1
}

/// <summary>
/// <see href="https://wicg.github.io/serial/#dom-paritytype">Web Serial API: 4.4.1.1 ParityType enum</see>
/// </summary>
[Description("@#ParityType")]
[ECMAScript]
[String]
public enum ParityType
{
    /// <summary>
    /// <see href="https://wicg.github.io/serial/#dom-paritytype-none">Web Serial API: 4.4.1.1 ParityType enum</see>
    /// </summary>
    [Description("@#none")]
    None = 0,

    /// <summary>
    /// <see href="https://wicg.github.io/serial/#dom-paritytype-even">Web Serial API: 4.4.1.1 ParityType enum</see>
    /// </summary>
    [Description("@#even")]
    Even = 1,

    /// <summary>
    /// <see href="https://wicg.github.io/serial/#dom-paritytype-odd">Web Serial API: 4.4.1.1 ParityType enum</see>
    /// </summary>
    [Description("@#odd")]
    Odd = 2
}

/// <summary>
/// <see href="https://wicg.github.io/shape-detection-api/#enumdef-barcodeformat">Accelerated Shape Detection in Images: 2.3.3 BarcodeFormat</see>
/// </summary>
[Description("@#BarcodeFormat")]
[ECMAScript]
[String]
public enum BarcodeFormat
{
    /// <summary>
    /// <see href="https://wicg.github.io/shape-detection-api/#dom-barcodeformat-aztec">Accelerated Shape Detection in Images: 2.3.3 BarcodeFormat</see>
    /// </summary>
    [Description("@#aztec")]
    Aztec = 0,

    /// <summary>
    /// <see href="https://wicg.github.io/shape-detection-api/#dom-barcodeformat-code_128">Accelerated Shape Detection in Images: 2.3.3 BarcodeFormat</see>
    /// </summary>
    [Description("@#code_128")]
    Code128 = 1,

    /// <summary>
    /// <see href="https://wicg.github.io/shape-detection-api/#dom-barcodeformat-code_39">Accelerated Shape Detection in Images: 2.3.3 BarcodeFormat</see>
    /// </summary>
    [Description("@#code_39")]
    Code39 = 2,

    /// <summary>
    /// <see href="https://wicg.github.io/shape-detection-api/#dom-barcodeformat-code_93">Accelerated Shape Detection in Images: 2.3.3 BarcodeFormat</see>
    /// </summary>
    [Description("@#code_93")]
    Code93 = 3,

    /// <summary>
    /// <see href="https://wicg.github.io/shape-detection-api/#dom-barcodeformat-codabar">Accelerated Shape Detection in Images: 2.3.3 BarcodeFormat</see>
    /// </summary>
    [Description("@#codabar")]
    Codabar = 4,

    /// <summary>
    /// <see href="https://wicg.github.io/shape-detection-api/#dom-barcodeformat-data_matrix">Accelerated Shape Detection in Images: 2.3.3 BarcodeFormat</see>
    /// </summary>
    [Description("@#data_matrix")]
    DataMatrix = 5,

    /// <summary>
    /// <see href="https://wicg.github.io/shape-detection-api/#dom-barcodeformat-ean_13">Accelerated Shape Detection in Images: 2.3.3 BarcodeFormat</see>
    /// </summary>
    [Description("@#ean_13")]
    Ean13 = 6,

    /// <summary>
    /// <see href="https://wicg.github.io/shape-detection-api/#dom-barcodeformat-ean_8">Accelerated Shape Detection in Images: 2.3.3 BarcodeFormat</see>
    /// </summary>
    [Description("@#ean_8")]
    Ean8 = 7,

    /// <summary>
    /// <see href="https://wicg.github.io/shape-detection-api/#dom-barcodeformat-itf">Accelerated Shape Detection in Images: 2.3.3 BarcodeFormat</see>
    /// </summary>
    [Description("@#itf")]
    Itf = 8,

    /// <summary>
    /// <see href="https://wicg.github.io/shape-detection-api/#dom-barcodeformat-pdf417">Accelerated Shape Detection in Images: 2.3.3 BarcodeFormat</see>
    /// </summary>
    [Description("@#pdf417")]
    Pdf417 = 9,

    /// <summary>
    /// <see href="https://wicg.github.io/shape-detection-api/#dom-barcodeformat-qr_code">Accelerated Shape Detection in Images: 2.3.3 BarcodeFormat</see>
    /// </summary>
    [Description("@#qr_code")]
    QrCode = 10,

    /// <summary>
    /// <see href="https://wicg.github.io/shape-detection-api/#dom-barcodeformat-unknown">Accelerated Shape Detection in Images: 2.3.3 BarcodeFormat</see>
    /// </summary>
    [Description("@#unknown")]
    Unknown = 11,

    /// <summary>
    /// <see href="https://wicg.github.io/shape-detection-api/#dom-barcodeformat-upc_a">Accelerated Shape Detection in Images: 2.3.3 BarcodeFormat</see>
    /// </summary>
    [Description("@#upc_a")]
    UpcA = 12,

    /// <summary>
    /// <see href="https://wicg.github.io/shape-detection-api/#dom-barcodeformat-upc_e">Accelerated Shape Detection in Images: 2.3.3 BarcodeFormat</see>
    /// </summary>
    [Description("@#upc_e")]
    UpcE = 13
}

/// <summary>
/// <see href="https://wicg.github.io/shape-detection-api/#enumdef-landmarktype">Accelerated Shape Detection in Images: 2.2.2 DetectedFace</see>
/// </summary>
[Description("@#LandmarkType")]
[ECMAScript]
[String]
public enum LandmarkType
{
    /// <summary>
    /// <see href="https://wicg.github.io/shape-detection-api/#dom-landmarktype-mouth">Accelerated Shape Detection in Images: 2.2.2 DetectedFace</see>
    /// </summary>
    [Description("@#mouth")]
    Mouth = 0,

    /// <summary>
    /// <see href="https://wicg.github.io/shape-detection-api/#dom-landmarktype-eye">Accelerated Shape Detection in Images: 2.2.2 DetectedFace</see>
    /// </summary>
    [Description("@#eye")]
    Eye = 1,

    /// <summary>
    /// <see href="https://wicg.github.io/shape-detection-api/#dom-landmarktype-nose">Accelerated Shape Detection in Images: 2.2.2 DetectedFace</see>
    /// </summary>
    [Description("@#nose")]
    Nose = 2
}

/// <summary>
/// <see href="https://wicg.github.io/trust-token-api/#enumdef-operationtype">Private State Token API: Private State Token API</see>
/// </summary>
[Description("@#OperationType")]
[ECMAScript]
[String]
public enum OperationType
{
    /// <summary>
    /// <see href="https://wicg.github.io/trust-token-api/#dom-operationtype-token-request">Private State Token API: Private State Token API</see>
    /// </summary>
    [Description("@#token-request")]
    TokenRequest = 0,

    /// <summary>
    /// <see href="https://wicg.github.io/trust-token-api/#dom-operationtype-send-redemption-record">Private State Token API: Private State Token API</see>
    /// </summary>
    [Description("@#send-redemption-record")]
    SendRedemptionRecord = 1,

    /// <summary>
    /// <see href="https://wicg.github.io/trust-token-api/#dom-operationtype-token-redemption">Private State Token API: Private State Token API</see>
    /// </summary>
    [Description("@#token-redemption")]
    TokenRedemption = 2
}

/// <summary>
/// <see href="https://wicg.github.io/trust-token-api/#enumdef-refreshpolicy">Private State Token API: 6.1 Definitions</see>
/// </summary>
[Description("@#RefreshPolicy")]
[ECMAScript]
[String]
public enum RefreshPolicy
{
    /// <summary>
    /// <see href="https://wicg.github.io/trust-token-api/#dom-refreshpolicy-none">Private State Token API: Private State Token API</see>
    /// </summary>
    [Description("@#none")]
    None = 0,

    /// <summary>
    /// <see href="https://wicg.github.io/trust-token-api/#dom-refreshpolicy-refresh">Private State Token API: Private State Token API</see>
    /// </summary>
    [Description("@#refresh")]
    Refresh = 1
}

/// <summary>
/// <see href="https://wicg.github.io/trust-token-api/#enumdef-tokenversion">Private State Token API: Private State Token API</see>
/// </summary>
[Description("@#TokenVersion")]
[ECMAScript]
[String]
public enum TokenVersion
{
    /// <summary>
    /// <see href="https://wicg.github.io/trust-token-api/#dom-tokenversion-1">Private State Token API: Private State Token API</see>
    /// </summary>
    [Description("@#1")]
    _1 = 0
}

/// <summary>
/// <see href="https://wicg.github.io/web-otp/#enumdef-otpcredentialtransporttype">WebOTP API: 2.4 OTPCredentialTransportType</see>
/// </summary>
[Description("@#OTPCredentialTransportType")]
[ECMAScript]
[String]
public enum OTPCredentialTransportType
{
    /// <summary>
    /// <see href="https://wicg.github.io/web-otp/#dom-otpcredentialtransporttype-sms">WebOTP API: 2.4 OTPCredentialTransportType</see>
    /// </summary>
    [Description("@#sms")]
    Sms = 0
}

/// <summary>
/// <see href="https://wicg.github.io/webcrypto-modern-algos/#dom-keyformat">Modern Algorithms in the Web Cryptography API: 3.1.1 Key Formats</see>
/// </summary>
[Description("@#KeyFormat")]
[ECMAScript]
[String]
public enum KeyFormat
{
    /// <summary>
    /// For all existing asymmetric algorithms in webcrypto, &quot;raw-public&quot; acts as an alias of &quot;raw&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/webcrypto-modern-algos/#dom-keyformat-raw-public">Modern Algorithms in the Web Cryptography API: 3.1.1 Key Formats</see>
    /// </remarks>
    [Description("@#raw-public")]
    RawPublic = 0,

    /// <summary>
    /// <see href="https://wicg.github.io/webcrypto-modern-algos/#dom-keyformat-raw-private">Modern Algorithms in the Web Cryptography API: 3.1.1 Key Formats</see>
    /// </summary>
    [Description("@#raw-private")]
    RawPrivate = 1,

    /// <summary>
    /// <see href="https://wicg.github.io/webcrypto-modern-algos/#dom-keyformat-raw-seed">Modern Algorithms in the Web Cryptography API: 3.1.1 Key Formats</see>
    /// </summary>
    [Description("@#raw-seed")]
    RawSeed = 2,

    /// <summary>
    /// For all existing symmetric algorithms in webcrypto, &quot;raw-secret&quot; acts as an alias of &quot;raw&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/webcrypto-modern-algos/#dom-keyformat-raw-secret">Modern Algorithms in the Web Cryptography API: 3.1.1 Key Formats</see>
    /// </remarks>
    [Description("@#raw-secret")]
    RawSecret = 3,

    /// <summary>
    /// For all existing symmetric algorithms in webcrypto, &quot;raw-secret&quot; acts as an alias of &quot;raw&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/webcrypto-modern-algos/#dom-keyformat-raw">Modern Algorithms in the Web Cryptography API: 3.1.1 Key Formats</see>
    /// </remarks>
    [Description("@#raw")]
    Raw = 4,

    /// <summary>
    /// <see href="https://wicg.github.io/webcrypto-modern-algos/#dom-keyformat-spki">Modern Algorithms in the Web Cryptography API: 3.1.1 Key Formats</see>
    /// </summary>
    [Description("@#spki")]
    Spki = 5,

    /// <summary>
    /// <see href="https://wicg.github.io/webcrypto-modern-algos/#dom-keyformat-pkcs8">Modern Algorithms in the Web Cryptography API: 3.1.1 Key Formats</see>
    /// </summary>
    [Description("@#pkcs8")]
    Pkcs8 = 6,

    /// <summary>
    /// <see href="https://wicg.github.io/webcrypto-modern-algos/#dom-keyformat-jwk">Modern Algorithms in the Web Cryptography API: 3.1.1 Key Formats</see>
    /// </summary>
    [Description("@#jwk")]
    Jwk = 7
}

/// <summary>
/// <see href="https://wicg.github.io/webcrypto-modern-algos/#dom-keyusage">Modern Algorithms in the Web Cryptography API: 3.1.2 Key Usages</see>
/// </summary>
[Description("@#KeyUsage")]
[ECMAScript]
[String]
public enum KeyUsage
{
    /// <summary>
    /// <see href="https://wicg.github.io/webcrypto-modern-algos/#dom-keyusage-encrypt">Modern Algorithms in the Web Cryptography API: 3.1.2 Key Usages</see>
    /// </summary>
    [Description("@#encrypt")]
    Encrypt = 0,

    /// <summary>
    /// <see href="https://wicg.github.io/webcrypto-modern-algos/#dom-keyusage-decrypt">Modern Algorithms in the Web Cryptography API: 3.1.2 Key Usages</see>
    /// </summary>
    [Description("@#decrypt")]
    Decrypt = 1,

    /// <summary>
    /// <see href="https://wicg.github.io/webcrypto-modern-algos/#dom-keyusage-sign">Modern Algorithms in the Web Cryptography API: 3.1.2 Key Usages</see>
    /// </summary>
    [Description("@#sign")]
    Sign = 2,

    /// <summary>
    /// <see href="https://wicg.github.io/webcrypto-modern-algos/#dom-keyusage-verify">Modern Algorithms in the Web Cryptography API: 3.1.2 Key Usages</see>
    /// </summary>
    [Description("@#verify")]
    Verify = 3,

    /// <summary>
    /// <see href="https://wicg.github.io/webcrypto-modern-algos/#dom-keyusage-derivekey">Modern Algorithms in the Web Cryptography API: 3.1.2 Key Usages</see>
    /// </summary>
    [Description("@#deriveKey")]
    DeriveKey = 4,

    /// <summary>
    /// <see href="https://wicg.github.io/webcrypto-modern-algos/#dom-keyusage-derivebits">Modern Algorithms in the Web Cryptography API: 3.1.2 Key Usages</see>
    /// </summary>
    [Description("@#deriveBits")]
    DeriveBits = 5,

    /// <summary>
    /// <see href="https://wicg.github.io/webcrypto-modern-algos/#dom-keyusage-wrapkey">Modern Algorithms in the Web Cryptography API: 3.1.2 Key Usages</see>
    /// </summary>
    [Description("@#wrapKey")]
    WrapKey = 6,

    /// <summary>
    /// <see href="https://wicg.github.io/webcrypto-modern-algos/#dom-keyusage-unwrapkey">Modern Algorithms in the Web Cryptography API: 3.1.2 Key Usages</see>
    /// </summary>
    [Description("@#unwrapKey")]
    UnwrapKey = 7,

    /// <summary>
    /// <see href="https://wicg.github.io/webcrypto-modern-algos/#dom-keyusage-encapsulatekey">Modern Algorithms in the Web Cryptography API: 3.1.2 Key Usages</see>
    /// </summary>
    [Description("@#encapsulateKey")]
    EncapsulateKey = 8,

    /// <summary>
    /// <see href="https://wicg.github.io/webcrypto-modern-algos/#dom-keyusage-encapsulatebits">Modern Algorithms in the Web Cryptography API: 3.1.2 Key Usages</see>
    /// </summary>
    [Description("@#encapsulateBits")]
    EncapsulateBits = 9,

    /// <summary>
    /// <see href="https://wicg.github.io/webcrypto-modern-algos/#dom-keyusage-decapsulatekey">Modern Algorithms in the Web Cryptography API: 3.1.2 Key Usages</see>
    /// </summary>
    [Description("@#decapsulateKey")]
    DecapsulateKey = 10,

    /// <summary>
    /// <see href="https://wicg.github.io/webcrypto-modern-algos/#dom-keyusage-decapsulatebits">Modern Algorithms in the Web Cryptography API: 3.1.2 Key Usages</see>
    /// </summary>
    [Description("@#decapsulateBits")]
    DecapsulateBits = 11
}

/// <summary>
/// <see href="https://wicg.github.io/webusb/#enumdef-usbdirection">WebUSB API: 6.6 The USBEndpoint Interface</see>
/// </summary>
[Description("@#USBDirection")]
[ECMAScript]
[String]
public enum USBDirection
{
    /// <summary>
    /// <see href="https://wicg.github.io/webusb/#dom-usbdirection-in">WebUSB API: 6.6 The USBEndpoint Interface</see>
    /// </summary>
    [Description("@#in")]
    In = 0,

    /// <summary>
    /// <see href="https://wicg.github.io/webusb/#dom-usbdirection-out">WebUSB API: 6.6 The USBEndpoint Interface</see>
    /// </summary>
    [Description("@#out")]
    Out = 1
}

/// <summary>
/// <see href="https://wicg.github.io/webusb/#enumdef-usbendpointtype">WebUSB API: 6.6 The USBEndpoint Interface</see>
/// </summary>
[Description("@#USBEndpointType")]
[ECMAScript]
[String]
public enum USBEndpointType
{
    /// <summary>
    /// <see href="https://wicg.github.io/webusb/#dom-usbendpointtype-bulk">WebUSB API: 6.6 The USBEndpoint Interface</see>
    /// </summary>
    [Description("@#bulk")]
    Bulk = 0,

    /// <summary>
    /// <see href="https://wicg.github.io/webusb/#dom-usbendpointtype-interrupt">WebUSB API: 6.6 The USBEndpoint Interface</see>
    /// </summary>
    [Description("@#interrupt")]
    Interrupt = 1,

    /// <summary>
    /// <see href="https://wicg.github.io/webusb/#dom-usbendpointtype-isochronous">WebUSB API: 6.6 The USBEndpoint Interface</see>
    /// </summary>
    [Description("@#isochronous")]
    Isochronous = 2
}

/// <summary>
/// <see href="https://wicg.github.io/webusb/#enumdef-usbrecipient">WebUSB API: 6.2 The USBControlTransferParameters Dictionary</see>
/// </summary>
[Description("@#USBRecipient")]
[ECMAScript]
[String]
public enum USBRecipient
{
    /// <summary>
    /// <see href="https://wicg.github.io/webusb/#dom-usbrecipient-device">WebUSB API: 6.2 The USBControlTransferParameters Dictionary</see>
    /// </summary>
    [Description("@#device")]
    Device = 0,

    /// <summary>
    /// <see href="https://wicg.github.io/webusb/#dom-usbrecipient-interface">WebUSB API: 6.2 The USBControlTransferParameters Dictionary</see>
    /// </summary>
    [Description("@#interface")]
    Interface = 1,

    /// <summary>
    /// <see href="https://wicg.github.io/webusb/#dom-usbrecipient-endpoint">WebUSB API: 6.2 The USBControlTransferParameters Dictionary</see>
    /// </summary>
    [Description("@#endpoint")]
    Endpoint = 2,

    /// <summary>
    /// <see href="https://wicg.github.io/webusb/#dom-usbrecipient-other">WebUSB API: 6.2 The USBControlTransferParameters Dictionary</see>
    /// </summary>
    [Description("@#other")]
    Other = 3
}

/// <summary>
/// <see href="https://wicg.github.io/webusb/#enumdef-usbrequesttype">WebUSB API: 6.2 The USBControlTransferParameters Dictionary</see>
/// </summary>
[Description("@#USBRequestType")]
[ECMAScript]
[String]
public enum USBRequestType
{
    /// <summary>
    /// <see href="https://wicg.github.io/webusb/#dom-usbrequesttype-standard">WebUSB API: 6.2 The USBControlTransferParameters Dictionary</see>
    /// </summary>
    [Description("@#standard")]
    Standard = 0,

    /// <summary>
    /// <see href="https://wicg.github.io/webusb/#dom-usbrequesttype-class">WebUSB API: 6.2 The USBControlTransferParameters Dictionary</see>
    /// </summary>
    [Description("@#class")]
    Class = 1,

    /// <summary>
    /// <see href="https://wicg.github.io/webusb/#dom-usbrequesttype-vendor">WebUSB API: 6.2 The USBControlTransferParameters Dictionary</see>
    /// </summary>
    [Description("@#vendor")]
    Vendor = 2
}

/// <summary>
/// <see href="https://wicg.github.io/webusb/#enumdef-usbtransferstatus">WebUSB API: 6.1 The USBDevice Interface</see>
/// </summary>
[Description("@#USBTransferStatus")]
[ECMAScript]
[String]
public enum USBTransferStatus
{
    /// <summary>
    /// <see href="https://wicg.github.io/webusb/#dom-usbtransferstatus-ok">WebUSB API: 6.1 The USBDevice Interface</see>
    /// </summary>
    [Description("@#ok")]
    Ok = 0,

    /// <summary>
    /// <see href="https://wicg.github.io/webusb/#dom-usbtransferstatus-stall">WebUSB API: 6.1 The USBDevice Interface</see>
    /// </summary>
    [Description("@#stall")]
    Stall = 1,

    /// <summary>
    /// <see href="https://wicg.github.io/webusb/#dom-usbtransferstatus-babble">WebUSB API: 6.1 The USBDevice Interface</see>
    /// </summary>
    [Description("@#babble")]
    Babble = 2
}

/// <summary>
/// <see href="https://xhr.spec.whatwg.org/#xmlhttprequestresponsetype">XMLHttpRequest Standard: 3 Interface XMLHttpRequest</see>
/// </summary>
[Description("@#XMLHttpRequestResponseType")]
[ECMAScript]
[String]
public enum XMLHttpRequestResponseType
{
    [Description("@#")]
    Empty = 0,

    /// <summary>
    /// <see href="https://xhr.spec.whatwg.org/#dom-xmlhttprequestresponsetype-arraybuffer">XMLHttpRequest Standard: 3 Interface XMLHttpRequest</see>
    /// </summary>
    [Description("@#arraybuffer")]
    Arraybuffer = 1,

    /// <summary>
    /// <see href="https://xhr.spec.whatwg.org/#dom-xmlhttprequestresponsetype-blob">XMLHttpRequest Standard: 3 Interface XMLHttpRequest</see>
    /// </summary>
    [Description("@#blob")]
    Blob = 2,

    /// <summary>
    /// Throws an &quot;InvalidStateError!!exception&quot; DOMException if responseType is not the empty string or &quot;document&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://xhr.spec.whatwg.org/#dom-xmlhttprequestresponsetype-document">XMLHttpRequest Standard: 3 Interface XMLHttpRequest</see>
    /// </remarks>
    [Description("@#document")]
    Document = 3,

    /// <summary>
    /// <see href="https://xhr.spec.whatwg.org/#dom-xmlhttprequestresponsetype-json">XMLHttpRequest Standard: 3 Interface XMLHttpRequest</see>
    /// </summary>
    [Description("@#json")]
    Json = 4,

    /// <summary>
    /// Throws an &quot;InvalidStateError!!exception&quot; DOMException if responseType is not the empty string or &quot;text&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://xhr.spec.whatwg.org/#dom-xmlhttprequestresponsetype-text">XMLHttpRequest Standard: 3 Interface XMLHttpRequest</see>
    /// </remarks>
    [Description("@#text")]
    Text = 5
}

/// <summary>
/// A HIDUnitSystem enum value specifying the unit system for the unit definition, or &quot;none&quot; if the item has no unit definition.
/// </summary>
/// <remarks>
/// <see href="https://wicg.github.io/webhid/#dom-hidunitsystem">WebHID API: 13. HIDUnitSystem enum</see>
/// </remarks>
[Description("@#HIDUnitSystem")]
[ECMAScript]
[String]
public enum HIDUnitSystem
{
    /// <summary>
    /// A HIDUnitSystem enum value specifying the unit system for the unit definition, or &quot;none&quot; if the item has no unit definition.
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/webhid/#dom-hidunitsystem-none">WebHID API: 13. HIDUnitSystem enum</see>
    /// </remarks>
    [Description("@#none")]
    None = 0,

    /// <summary>
    /// 1, then set globalState[&quot;unitSystem&quot;] to &quot;si-linear&quot;
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/webhid/#dom-hidunitsystem-si-linear">WebHID API: 13. HIDUnitSystem enum</see>
    /// </remarks>
    [Description("@#si-linear")]
    SiLinear = 1,

    /// <summary>
    /// 2, then set globalState[&quot;unitSystem&quot;] to &quot;si-rotation&quot;
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/webhid/#dom-hidunitsystem-si-rotation">WebHID API: 13. HIDUnitSystem enum</see>
    /// </remarks>
    [Description("@#si-rotation")]
    SiRotation = 2,

    /// <summary>
    /// 3, then set globalState[&quot;unitSystem&quot;] to &quot;english-linear&quot;
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/webhid/#dom-hidunitsystem-english-linear">WebHID API: 13. HIDUnitSystem enum</see>
    /// </remarks>
    [Description("@#english-linear")]
    EnglishLinear = 3,

    /// <summary>
    /// 4, then set globalState[&quot;unitSystem&quot;] to &quot;english-rotation&quot;
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/webhid/#dom-hidunitsystem-english-rotation">WebHID API: 13. HIDUnitSystem enum</see>
    /// </remarks>
    [Description("@#english-rotation")]
    EnglishRotation = 4,

    /// <summary>
    /// -1, then set globalState[&quot;unitSystem&quot;] to &quot;vendor-defined&quot;
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/webhid/#dom-hidunitsystem-vendor-defined">WebHID API: 13. HIDUnitSystem enum</see>
    /// </remarks>
    [Description("@#vendor-defined")]
    VendorDefined = 5,

    /// <summary>
    /// Otherwise, set globalState[&quot;unitSystem&quot;] to &quot;reserved&quot;
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/webhid/#dom-hidunitsystem-reserved">WebHID API: 13. HIDUnitSystem enum</see>
    /// </remarks>
    [Description("@#reserved")]
    Reserved = 6
}

/// <summary>
/// A PaymentRequest&apos;s shippingType attribute is the type of shipping used to fulfill the transaction. Its value is either a PaymentShippingType enum value, or null if none is provided by the developer during PaymentRequest.PaymentRequest()|construction (see PaymentOptions&apos;s shippingType member).
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/payment-request/#dom-paymentshippingtype">Payment Request API: 8 PaymentShippingType enum</see>
/// </remarks>
[Description("@#PaymentShippingType")]
[ECMAScript]
[String]
public enum PaymentShippingType
{
    /// <summary>
    /// A PaymentRequest&apos;s shippingType attribute is the type of shipping used to fulfill the transaction. Its value is either a PaymentShippingType enum value, or null if none is provided by the developer during PaymentRequest.PaymentRequest()|construction (see PaymentOptions&apos;s shippingType member).
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/payment-request/#dom-paymentshippingtype-shipping">Payment Request API: 8 PaymentShippingType enum</see>
    /// </remarks>
    [Description("@#shipping")]
    Shipping = 0,

    /// <summary>
    /// A PaymentShippingType enum value. Some transactions require an physical address|address for delivery but the term &quot;shipping&quot; isn&apos;t appropriate. For example, &quot;pizza delivery&quot; not &quot;pizza shipping&quot; and &quot;laundry pickup&quot; not &quot;laundry shipping&quot;. If requestShipping is set to true, then the shippingType member can influence the way the user agent presents the user interface for gathering the shipping address.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/payment-request/#dom-paymentshippingtype-delivery">Payment Request API: 8 PaymentShippingType enum</see>
    /// </remarks>
    [Description("@#delivery")]
    Delivery = 1,

    /// <summary>
    /// A PaymentShippingType enum value. Some transactions require an physical address|address for delivery but the term &quot;shipping&quot; isn&apos;t appropriate. For example, &quot;pizza delivery&quot; not &quot;pizza shipping&quot; and &quot;laundry pickup&quot; not &quot;laundry shipping&quot;. If requestShipping is set to true, then the shippingType member can influence the way the user agent presents the user interface for gathering the shipping address.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/payment-request/#dom-paymentshippingtype-pickup">Payment Request API: 8 PaymentShippingType enum</see>
    /// </remarks>
    [Description("@#pickup")]
    Pickup = 2
}

/// <summary>
/// A PerformanceResourceTiming has an associated RenderBlockingStatusType render-blocking status.
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/resource-timing/#enumdef-renderblockingstatustype">Resource Timing: 3.3.1 RenderBlockingStatusType enum</see>
/// </remarks>
[Description("@#RenderBlockingStatusType")]
[ECMAScript]
[String]
public enum RenderBlockingStatusType
{
    /// <summary>
    /// A PerformanceResourceTiming has an associated RenderBlockingStatusType render-blocking status.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/resource-timing/#dom-renderblockingstatustype-blocking">Resource Timing: 3.3.1 RenderBlockingStatusType enum</see>
    /// </remarks>
    [Description("@#blocking")]
    Blocking = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/resource-timing/#dom-renderblockingstatustype-non-blocking">Resource Timing: 3.3.1 RenderBlockingStatusType enum</see>
    /// </summary>
    [Description("@#non-blocking")]
    NonBlocking = 1
}

/// <summary>
/// A list of HandwritingInputType enums describing how the drawing is made.
/// </summary>
/// <remarks>
/// <see href="https://wicg.github.io/handwriting-recognition/#enumdef-handwritinginputtype">Handwriting Recognition API: 3 Feature Query</see>
/// </remarks>
[Description("@#HandwritingInputType")]
[ECMAScript]
[String]
public enum HandwritingInputType
{
    /// <summary>
    /// <see href="https://wicg.github.io/handwriting-recognition/#dom-handwritinginputtype-mouse">Handwriting Recognition API: 3 Feature Query</see>
    /// </summary>
    [Description("@#mouse")]
    Mouse = 0,

    /// <summary>
    /// <see href="https://wicg.github.io/handwriting-recognition/#dom-handwritinginputtype-stylus">Handwriting Recognition API: 3 Feature Query</see>
    /// </summary>
    [Description("@#stylus")]
    Stylus = 1,

    /// <summary>
    /// <see href="https://wicg.github.io/handwriting-recognition/#dom-handwritinginputtype-touch">Handwriting Recognition API: 3 Feature Query</see>
    /// </summary>
    [Description("@#touch")]
    Touch = 2
}

/// <summary>
/// A list of HandwritingRecognitionType enums describing the type of text that is likely to be drawn.
/// </summary>
/// <remarks>
/// <see href="https://wicg.github.io/handwriting-recognition/#enumdef-handwritingrecognitiontype">Handwriting Recognition API: 3 Feature Query</see>
/// </remarks>
[Description("@#HandwritingRecognitionType")]
[ECMAScript]
[String]
public enum HandwritingRecognitionType
{
    /// <summary>
    /// A list of HandwritingRecognitionType enums describing the type of text that is likely to be drawn.
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/handwriting-recognition/#dom-handwritingrecognitiontype-text">Handwriting Recognition API: 3 Feature Query</see>
    /// </remarks>
    [Description("@#text")]
    Text = 0,

    /// <summary>
    /// <see href="https://wicg.github.io/handwriting-recognition/#dom-handwritingrecognitiontype-per-character">Handwriting Recognition API: 3 Feature Query</see>
    /// </summary>
    [Description("@#per-character")]
    PerCharacter = 1
}

/// <summary>
/// A sequence of supported white balance modes. Each string MUST be one of the members of MeteringMode.
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/mediacapture-image/#enumdef-meteringmode">MediaStream Image Capture: 11. MeteringMode</see>
/// </remarks>
[Description("@#MeteringMode")]
[ECMAScript]
[String]
public enum MeteringMode
{
    /// <summary>
    /// <see href="https://w3c.github.io/mediacapture-image/#dom-meteringmode-none">MediaStream Image Capture: 11.1 Values</see>
    /// </summary>
    [Description("@#none")]
    None = 0,

    /// <summary>
    /// Exposure is the amount of light that is allowed to fall on the photosensitive device. In auto-exposure modes (single-shot or continuous exposureMode), the exposure time and/or camera aperture are automatically adjusted by the implementation based on the subject of the photo. In manual exposureMode, these parameters are set to fixed absolute values.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-image/#dom-meteringmode-manual">MediaStream Image Capture: 11.1 Values</see>
    /// </remarks>
    [Description("@#manual")]
    Manual = 1,

    /// <summary>
    /// Current exposure compensation setting. A value of 0 EV is interpreted as no exposure compensation. This field is only significant if exposureMode is continuous or single-shot
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-image/#dom-meteringmode-single-shot">MediaStream Image Capture: 11.1 Values</see>
    /// </remarks>
    [Description("@#single-shot")]
    SingleShot = 2,

    /// <summary>
    /// Current exposure compensation setting. A value of 0 EV is interpreted as no exposure compensation. This field is only significant if exposureMode is continuous or single-shot
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-image/#dom-meteringmode-continuous">MediaStream Image Capture: 11.1 Values</see>
    /// </remarks>
    [Description("@#continuous")]
    Continuous = 3
}

/// <summary>
/// Add touchType, altitudeAngle, azimuthAngle (Safari iOS 10.3 extensions for stylus)
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/touch-events/#dom-touchtype">Touch Events - Level 2: 3 Touch Interface</see>
/// </remarks>
[Description("@#TouchType")]
[ECMAScript]
[String]
public enum TouchType
{
    /// <summary>
    /// <see href="https://w3c.github.io/touch-events/#dom-touchtype-direct">Touch Events - Level 2: 3 Touch Interface</see>
    /// </summary>
    [Description("@#direct")]
    Direct = 0,

    /// <summary>
    /// Add touchType, altitudeAngle, azimuthAngle (Safari iOS 10.3 extensions for stylus)
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/touch-events/#dom-touchtype-stylus">Touch Events - Level 2: 3 Touch Interface</see>
    /// </remarks>
    [Description("@#stylus")]
    Stylus = 1
}

/// <summary>
/// An enumeration, GamepadHand, that indicates which hand the controller is being held in or is most likely to be held in.
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/gamepad/extensions.html#dom-gamepadhand">Gamepad Extensions: 3 GamepadHand Enum</see>
/// </remarks>
[Description("@#GamepadHand")]
[ECMAScript]
[String]
public enum GamepadHand
{
    [Description("@#")]
    Empty = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/gamepad/extensions.html#dom-gamepadhand-left">Gamepad Extensions: 3 GamepadHand Enum</see>
    /// </summary>
    [Description("@#left")]
    Left = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/gamepad/extensions.html#dom-gamepadhand-right">Gamepad Extensions: 3 GamepadHand Enum</see>
    /// </summary>
    [Description("@#right")]
    Right = 2
}

/// <summary>
/// Assumes values from the CursorCaptureConstraint enumeration that determines if and when the cursor is included in the captured display surface.
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/mediacapture-screen-share/#dom-cursorcaptureconstraint">Screen Capture: 5.4.16 CursorCaptureConstraint</see>
/// </remarks>
[Description("@#CursorCaptureConstraint")]
[ECMAScript]
[String]
public enum CursorCaptureConstraint
{
    /// <summary>
    /// <see href="https://w3c.github.io/mediacapture-screen-share/#idl-def-CursorCaptureConstraint.never">Screen Capture: 5.4.16 CursorCaptureConstraint</see>
    /// </summary>
    [Description("@#never")]
    Never = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/mediacapture-screen-share/#idl-def-CursorCaptureConstraint.always">Screen Capture: 5.4.16 CursorCaptureConstraint</see>
    /// </summary>
    [Description("@#always")]
    Always = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/mediacapture-screen-share/#idl-def-CursorCaptureConstraint.motion">Screen Capture: 5.4.16 CursorCaptureConstraint</see>
    /// </summary>
    [Description("@#motion")]
    Motion = 2
}

/// <summary>
/// Configures encoding to use one of the rate control modes specified by VideoEncoderBitrateMode.
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webcodecs/#enumdef-videoencoderbitratemode">WebCodecs: 7.14 VideoEncoderBitrateMode</see>
/// </remarks>
[Description("@#VideoEncoderBitrateMode")]
[ECMAScript]
[String]
public enum VideoEncoderBitrateMode
{
    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videoencoderbitratemode-constant">WebCodecs: 7.14 VideoEncoderBitrateMode</see>
    /// </summary>
    [Description("@#constant")]
    Constant = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videoencoderbitratemode-variable">WebCodecs: 7.14 VideoEncoderBitrateMode</see>
    /// </summary>
    [Description("@#variable")]
    Variable = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videoencoderbitratemode-quantizer">WebCodecs: 7.14 VideoEncoderBitrateMode</see>
    /// </summary>
    [Description("@#quantizer")]
    Quantizer = 2
}

/// <summary>
/// Configures the format of output EncodedAudioChunks. See AacBitstreamFormat.
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webcodecs/aac_codec_registration.html#enumdef-aacbitstreamformat">AAC WebCodecs Registration: 5.2 AacBitstreamFormat</see>
/// </remarks>
[Description("@#AacBitstreamFormat")]
[ECMAScript]
[String]
public enum AacBitstreamFormat
{
    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/aac_codec_registration.html#dom-aacbitstreamformat-aac">AAC WebCodecs Registration: 5.2 AacBitstreamFormat</see>
    /// </summary>
    [Description("@#aac")]
    Aac = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/aac_codec_registration.html#dom-aacbitstreamformat-adts">AAC WebCodecs Registration: 5.2 AacBitstreamFormat</see>
    /// </summary>
    [Description("@#adts")]
    Adts = 1
}

/// <summary>
/// Configures the format of output EncodedAudioChunks. See OpusBitstreamFormat.
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webcodecs/opus_codec_registration.html#enumdef-opusbitstreamformat">Opus WebCodecs Registration: 5.2 OpusBitstreamFormat</see>
/// </remarks>
[Description("@#OpusBitstreamFormat")]
[ECMAScript]
[String]
public enum OpusBitstreamFormat
{
    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/opus_codec_registration.html#dom-opusbitstreamformat-opus">Opus WebCodecs Registration: 5.2 OpusBitstreamFormat</see>
    /// </summary>
    [Description("@#opus")]
    Opus = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/opus_codec_registration.html#dom-opusbitstreamformat-ogg">Opus WebCodecs Registration: 5.2 OpusBitstreamFormat</see>
    /// </summary>
    [Description("@#ogg")]
    Ogg = 1
}

/// <summary>
/// Configures the format of output EncodedVideoChunks. See AvcBitstreamFormat.
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webcodecs/avc_codec_registration.html#enumdef-avcbitstreamformat">AVC (H.264) WebCodecs Registration: 5.2 AvcBitstreamFormat</see>
/// </remarks>
[Description("@#AvcBitstreamFormat")]
[ECMAScript]
[String]
public enum AvcBitstreamFormat
{
    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/avc_codec_registration.html#dom-avcbitstreamformat-annexb">AVC (H.264) WebCodecs Registration: 5.2 AvcBitstreamFormat</see>
    /// </summary>
    [Description("@#annexb")]
    Annexb = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/avc_codec_registration.html#dom-avcbitstreamformat-avc">AVC (H.264) WebCodecs Registration: 5.2 AvcBitstreamFormat</see>
    /// </summary>
    [Description("@#avc")]
    Avc = 1
}

/// <summary>
/// Configures the format of output EncodedVideoChunks. See HevcBitstreamFormat.
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webcodecs/hevc_codec_registration.html#enumdef-hevcbitstreamformat">HEVC (H.265) WebCodecs Registration: 5.2 HevcBitstreamFormat</see>
/// </remarks>
[Description("@#HevcBitstreamFormat")]
[ECMAScript]
[String]
public enum HevcBitstreamFormat
{
    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/hevc_codec_registration.html#dom-hevcbitstreamformat-annexb">HEVC (H.265) WebCodecs Registration: 5.2 HevcBitstreamFormat</see>
    /// </summary>
    [Description("@#annexb")]
    Annexb = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/hevc_codec_registration.html#dom-hevcbitstreamformat-hevc">HEVC (H.265) WebCodecs Registration: 5.2 HevcBitstreamFormat</see>
    /// </summary>
    [Description("@#hevc")]
    Hevc = 1
}

/// <summary>
/// Encryption keys used for push message encryption are provided to a web application through the getKey() method or the serializer of PushSubscription. Each key is named using a value from the PushEncryptionKeyName enumeration.
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/push-api/#dom-pushencryptionkeyname">Push API: 8.1 PushEncryptionKeyName enumeration</see>
/// </remarks>
[Description("@#PushEncryptionKeyName")]
[ECMAScript]
[String]
public enum PushEncryptionKeyName
{
    /// <summary>
    /// <see href="https://w3c.github.io/push-api/#dom-pushencryptionkeyname-p256dh">Push API: 8.1 PushEncryptionKeyName enumeration</see>
    /// </summary>
    [Description("@#p256dh")]
    P256dh = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/push-api/#dom-pushencryptionkeyname-auth">Push API: 8.1 PushEncryptionKeyName enumeration</see>
    /// </summary>
    [Description("@#auth")]
    Auth = 1
}

/// <summary>
/// If an optional member is specified for a MediaDecodingType or MediaEncodingType to which it&apos;s not applicable, return false and abort these steps. See applicability rules in the member definitions below.
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/media-capabilities/#enumdef-mediadecodingtype">Media Capabilities: 2.1.2 MediaDecodingType</see>
/// </remarks>
[Description("@#MediaDecodingType")]
[ECMAScript]
[String]
public enum MediaDecodingType
{
    /// <summary>
    /// file is used to represent a configuration that is meant to be used for playback of media sources other than MediaSource as defined in media-source and RTCPeerConnection as defined in webrtc.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/media-capabilities/#dom-mediadecodingtype-file">Media Capabilities: 2.1.2 MediaDecodingType</see>
    /// </remarks>
    [Description("@#file")]
    File = 0,

    /// <summary>
    /// file is used to represent a configuration that is meant to be used for playback of media sources other than MediaSource as defined in media-source and RTCPeerConnection as defined in webrtc.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/media-capabilities/#dom-mediadecodingtype-media-source">Media Capabilities: 2.1.2 MediaDecodingType</see>
    /// </remarks>
    [Description("@#media-source")]
    MediaSource = 1,

    /// <summary>
    /// file is used to represent a configuration that is meant to be used for playback of media sources other than MediaSource as defined in media-source and RTCPeerConnection as defined in webrtc.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/media-capabilities/#dom-mediadecodingtype-webrtc">Media Capabilities: 2.1.2 MediaDecodingType</see>
    /// </remarks>
    [Description("@#webrtc")]
    Webrtc = 2
}

/// <summary>
/// If an optional member is specified for a MediaDecodingType or MediaEncodingType to which it&apos;s not applicable, return false and abort these steps. See applicability rules in the member definitions below.
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/media-capabilities/#enumdef-mediaencodingtype">Media Capabilities: 2.1.3 MediaEncodingType</see>
/// </remarks>
[Description("@#MediaEncodingType")]
[ECMAScript]
[String]
public enum MediaEncodingType
{
    /// <summary>
    /// record is used to represent a configuration for recording of media, e.g., using MediaRecorder as defined in mediastream-recording.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/media-capabilities/#dom-mediaencodingtype-record">Media Capabilities: 2.1.3 MediaEncodingType</see>
    /// </remarks>
    [Description("@#record")]
    Record = 0,

    /// <summary>
    /// webrtc is used to represent a configuration that is meant to be transmitted using RTCPeerConnection as defined in webrtc).
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/media-capabilities/#dom-mediaencodingtype-webrtc">Media Capabilities: 2.1.3 MediaEncodingType</see>
    /// </remarks>
    [Description("@#webrtc")]
    Webrtc = 1
}

/// <summary>
/// If present, the hdrMetadataType member represents that the video track includes the specified HDR metadata type, which the UA needs to be capable of interpreting for tone mapping the HDR content to a color volume and luminance of the output device. Valid inputs are defined by HdrMetadataType. hdrMetadataType is only applicable to MediaDecodingConfiguration for types media-source and file.
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/media-capabilities/#enumdef-hdrmetadatatype">Media Capabilities: 2.1.5 HdrMetadataType</see>
/// </remarks>
[Description("@#HdrMetadataType")]
[ECMAScript]
[String]
public enum HdrMetadataType
{
    /// <summary>
    /// smpteSt2086, representing the static metadata type defined by !SMPTE-ST-2086.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/media-capabilities/#dom-hdrmetadatatype-smptest2086">Media Capabilities: 2.1.5 HdrMetadataType</see>
    /// </remarks>
    [Description("@#smpteSt2086")]
    SmpteSt2086 = 0,

    /// <summary>
    /// smpteSt2094-10, representing the dynamic metadata type defined by !SMPTE-ST-2094.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/media-capabilities/#dom-hdrmetadatatype-smptest2094-10">Media Capabilities: 2.1.5 HdrMetadataType</see>
    /// </remarks>
    [Description("@#smpteSt2094-10")]
    SmpteSt209410 = 1,

    /// <summary>
    /// smpteSt2094-40, representing the dynamic metadata type defined by !SMPTE-ST-2094.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/media-capabilities/#dom-hdrmetadatatype-smptest2094-40">Media Capabilities: 2.1.5 HdrMetadataType</see>
    /// </remarks>
    [Description("@#smpteSt2094-40")]
    SmpteSt209440 = 2
}

/// <summary>
/// If present, the transferFunction member represents that the video track requires the specified transfer function to be understood by the UA. Transfer function describes the electro-optical algorithm supported by the rendering capabilities of a user agent, independent of the display, to map the source colors in the decoded media into the colors to be displayed. Valid inputs are defined by TransferFunction. transferFunction is only applicable to MediaDecodingConfiguration for types media-source and file.
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/media-capabilities/#enumdef-transferfunction">Media Capabilities: 2.1.7 TransferFunction</see>
/// </remarks>
[Description("@#TransferFunction")]
[ECMAScript]
[String]
public enum TransferFunction
{
    /// <summary>
    /// srgb, representing the transfer function defined by !sRGB.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/media-capabilities/#dom-transferfunction-srgb">Media Capabilities: 2.1.7 TransferFunction</see>
    /// </remarks>
    [Description("@#srgb")]
    Srgb = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/media-capabilities/#dom-transferfunction-pq">Media Capabilities: 2.1.7 TransferFunction</see>
    /// </summary>
    [Description("@#pq")]
    Pq = 1,

    /// <summary>
    /// hlg, representing the &quot;Hybrid Log Gamma&quot; transfer function defined by BT.2100.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/media-capabilities/#dom-transferfunction-hlg">Media Capabilities: 2.1.7 TransferFunction</see>
    /// </remarks>
    [Description("@#hlg")]
    Hlg = 2
}

/// <summary>
/// If protocolString does not equal any enumeration value in DigitalCredentialIssuanceProtocol, return failure.
/// </summary>
/// <remarks>
/// <see href="https://w3c-fedid.github.io/digital-credentials/#dom-digitalcredentialissuanceprotocol">Digital Credentials: 7.8.3 The DigitalCredentialIssuanceProtocol enumeration</see>
/// </remarks>
[Description("@#DigitalCredentialIssuanceProtocol")]
[ECMAScript]
[String]
public enum DigitalCredentialIssuanceProtocol
{
    /// <summary>
    /// <see href="https://w3c-fedid.github.io/digital-credentials/#dom-digitalcredentialissuanceprotocol-openid4vci-v1">Digital Credentials: 5 Protocols</see>
    /// </summary>
    [Description("@#openid4vci-v1")]
    Openid4vciV1 = 0
}

/// <summary>
/// If protocolString does not equal any enumeration value in DigitalCredentialPresentationProtocol, return failure.
/// </summary>
/// <remarks>
/// <see href="https://w3c-fedid.github.io/digital-credentials/#dom-digitalcredentialpresentationprotocol">Digital Credentials: 7.8.2 The DigitalCredentialPresentationProtocol enumeration</see>
/// </remarks>
[Description("@#DigitalCredentialPresentationProtocol")]
[ECMAScript]
[String]
public enum DigitalCredentialPresentationProtocol
{
    /// <summary>
    /// <see href="https://w3c-fedid.github.io/digital-credentials/#dom-digitalcredentialpresentationprotocol-openid4vp-v1-unsigned">Digital Credentials: 5 Protocols</see>
    /// </summary>
    [Description("@#openid4vp-v1-unsigned")]
    Openid4vpV1Unsigned = 0,

    /// <summary>
    /// <see href="https://w3c-fedid.github.io/digital-credentials/#dom-digitalcredentialpresentationprotocol-openid4vp-v1-signed">Digital Credentials: 5 Protocols</see>
    /// </summary>
    [Description("@#openid4vp-v1-signed")]
    Openid4vpV1Signed = 1,

    /// <summary>
    /// <see href="https://w3c-fedid.github.io/digital-credentials/#dom-digitalcredentialpresentationprotocol-openid4vp-v1-multisigned">Digital Credentials: 5 Protocols</see>
    /// </summary>
    [Description("@#openid4vp-v1-multisigned")]
    Openid4vpV1Multisigned = 2,

    /// <summary>
    /// <see href="https://w3c-fedid.github.io/digital-credentials/#dom-digitalcredentialpresentationprotocol-org-iso-mdoc">Digital Credentials: 5 Protocols</see>
    /// </summary>
    [Description("@#org-iso-mdoc")]
    OrgIsoMdoc = 3
}

/// <summary>
/// If recorder supports the BitrateMode specified by the value of options&apos; audioBitrateMode member, then initialize recorder&apos;s audioBitrateMode attribute to the value of options&apos; audioBitrateMode member, else initialize recorder&apos;s audioBitrateMode attribute to the value &quot;variable&quot;.
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/mediacapture-record/#enumdef-bitratemode">MediaStream Recording: 2.6 BitrateMode</see>
/// </remarks>
[Description("@#BitrateMode")]
[ECMAScript]
[String]
public enum BitrateMode
{
    /// <summary>
    /// <see href="https://w3c.github.io/mediacapture-record/#dom-bitratemode-constant">MediaStream Recording: 2.6.1 Values</see>
    /// </summary>
    [Description("@#constant")]
    Constant = 0,

    /// <summary>
    /// If recorder supports the BitrateMode specified by the value of options&apos; audioBitrateMode member, then initialize recorder&apos;s audioBitrateMode attribute to the value of options&apos; audioBitrateMode member, else initialize recorder&apos;s audioBitrateMode attribute to the value &quot;variable&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-record/#dom-bitratemode-variable">MediaStream Recording: 2.6.1 Values</see>
    /// </remarks>
    [Description("@#variable")]
    Variable = 1
}

/// <summary>
/// If the actual number of bytes necessary to download is 0, but the user agent is faking a download for the reasons described in #privacy (notably #privacy-language-availability), then set this number to an implementation-defined value that helps with the download faking.
/// </summary>
/// <remarks>
/// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#enumdef-availability">Writing Assistance APIs: 5.1 Common APIs</see>
/// </remarks>
[Description("@#Availability")]
[ECMAScript]
[String]
public enum Availability
{
    /// <summary>
    /// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#dom-availability-unavailable">Writing Assistance APIs: 5.1 Common APIs</see>
    /// </summary>
    [Description("@#unavailable")]
    Unavailable = 0,

    /// <summary>
    /// If document stops being Document/fully active, this loop does not terminate, and the user agent should not cancel the download, for the reasons explained in #privacy-availability-cancelation. It could pause the download, effectively meaning that the loop will never again have observable effects such as firing downloadprogress events. But even in such a case, future calls to getAvailability given options need to return &quot;downloading&quot; instead of &quot;downloadable&quot;, and the material downloaded so far needs to persist even across user agent restarts.
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#dom-availability-downloadable">Writing Assistance APIs: 5.1 Common APIs</see>
    /// </remarks>
    [Description("@#downloadable")]
    Downloadable = 1,

    /// <summary>
    /// If document stops being Document/fully active, this loop does not terminate, and the user agent should not cancel the download, for the reasons explained in #privacy-availability-cancelation. It could pause the download, effectively meaning that the loop will never again have observable effects such as firing downloadprogress events. But even in such a case, future calls to getAvailability given options need to return &quot;downloading&quot; instead of &quot;downloadable&quot;, and the material downloaded so far needs to persist even across user agent restarts.
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#dom-availability-downloading">Writing Assistance APIs: 5.1 Common APIs</see>
    /// </remarks>
    [Description("@#downloading")]
    Downloading = 2,

    /// <summary>
    /// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#dom-availability-available">Writing Assistance APIs: 5.1 Common APIs</see>
    /// </summary>
    [Description("@#available")]
    Available = 3
}

/// <summary>
/// If the source cannot do echo cancellation a single false MUST be the only element in the list. If the source can do echo cancellation, then true MUST be included in the list. If the script can control the feature, the list MUST include at least both true and false. Additionally, if the source allows controlling which audio sources will be cancelled, it must include any supported values from the EchoCancellationModeEnum enum. If true or false are included in the list, they must appear before any value from EchoCancellationModeEnum. See echoCancellation for additional details.
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/mediacapture-main/#dom-echocancellationmodeenum">Media Capture and Streams: 4.3.8 Constrainable Properties</see>
/// </remarks>
[Description("@#EchoCancellationModeEnum")]
[ECMAScript]
[String]
public enum EchoCancellationModeEnum
{
    /// <summary>
    /// <see href="https://w3c.github.io/mediacapture-main/#dom-echocancellationmodeenum-all">Media Capture and Streams: 4.3.8 Constrainable Properties</see>
    /// </summary>
    [Description("@#all")]
    All = 0,

    /// <summary>
    /// In addition to the values from EchoCancellationModeEnum, the echoCancellation constrainable property also accepts the values true and false. false means that no echo cancellation will take place. true means that the UA decides what audio will be removed from the signals recorded by the microphone. true MUST attempt to cancel at least as much as &quot;remote-only&quot; and SHOULD attempt to cancel as much as &quot;all&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-main/#dom-echocancellationmodeenum-remote-only">Media Capture and Streams: 4.3.8 Constrainable Properties</see>
    /// </remarks>
    [Description("@#remote-only")]
    RemoteOnly = 1
}

/// <summary>
/// In the API, the posture values are represented by the DevicePostureType enum values.
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/device-posture/#dom-deviceposturetype">Device Posture API: 4 The DevicePosture interface</see>
/// </remarks>
[Description("@#DevicePostureType")]
[ECMAScript]
[String]
public enum DevicePostureType
{
    /// <summary>
    /// The API exposes a high-level abstraction referred to as a posture that can be either &quot;continuous&quot; or &quot;folded&quot;. Devices that do not support different postures default to &quot;continuous&quot;. This means at most one bit of entropy is added to the fingerprint. At most, because revealing this one bit will require a significant, explicit physical action by the user to manipulate the physical posture of the device required to trigger a change.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/device-posture/#dom-deviceposturetype-continuous">Device Posture API: 4 The DevicePosture interface</see>
    /// </remarks>
    [Description("@#continuous")]
    Continuous = 0,

    /// <summary>
    /// The API exposes a high-level abstraction referred to as a posture that can be either &quot;continuous&quot; or &quot;folded&quot;. Devices that do not support different postures default to &quot;continuous&quot;. This means at most one bit of entropy is added to the fingerprint. At most, because revealing this one bit will require a significant, explicit physical action by the user to manipulate the physical posture of the device required to trigger a change.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/device-posture/#dom-deviceposturetype-folded">Device Posture API: 4 The DevicePosture interface</see>
    /// </remarks>
    [Description("@#folded")]
    Folded = 1
}

/// <summary>
/// In the API, the wake lock types are represented by the WakeLockType enum values.
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/screen-wake-lock/#dom-wakelocktype">Screen Wake Lock API: 10. The WakeLockType enum</see>
/// </remarks>
[Description("@#WakeLockType")]
[ECMAScript]
[String]
public enum WakeLockType
{
    /// <summary>
    /// Run release a wake lock with document, lock, and &quot;screen&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/screen-wake-lock/#dom-wakelocktype-screen">Screen Wake Lock API: 10. The WakeLockType enum</see>
    /// </remarks>
    [Description("@#screen")]
    Screen = 0
}

/// <summary>
/// Let message type be the appropriate MediaKeyMessageType for the message.
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/encrypted-media/#dom-mediakeymessagetype">Encrypted Media Extensions: 6.4 MediaKeyMessageEvent</see>
/// </remarks>
[Description("@#MediaKeyMessageType")]
[ECMAScript]
[String]
public enum MediaKeyMessageType
{
    /// <summary>
    /// Generates a license request based on the initData. A message of type &quot;license-request&quot; or &quot;individualization-request&quot; will always be queued if the algorithm succeeds and the promise is resolved.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/encrypted-media/#dom-mediakeymessagetype-license-request">Encrypted Media Extensions: 6.4 MediaKeyMessageEvent</see>
    /// </remarks>
    [Description("@#license-request")]
    LicenseRequest = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/encrypted-media/#dom-mediakeymessagetype-license-renewal">Encrypted Media Extensions: 6.4 MediaKeyMessageEvent</see>
    /// </summary>
    [Description("@#license-renewal")]
    LicenseRenewal = 1,

    /// <summary>
    /// Sessions of this type can only be created if the configuration associated with the MediaKeySystemAccess object that created this object has a persistentState value of &quot;required&quot;. The session MUST be loadable via its Session ID once update() is called successfully. A message of type &quot;license-release&quot; containing the record of license destruction will be generated when remove() is called until the record is acknowledged by a response passed to update().
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/encrypted-media/#dom-mediakeymessagetype-license-release">Encrypted Media Extensions: 6.4 MediaKeyMessageEvent</see>
    /// </remarks>
    [Description("@#license-release")]
    LicenseRelease = 2,

    /// <summary>
    /// Generates a license request based on the initData. A message of type &quot;license-request&quot; or &quot;individualization-request&quot; will always be queued if the algorithm succeeds and the promise is resolved.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/encrypted-media/#dom-mediakeymessagetype-individualization-request">Encrypted Media Extensions: 6.4 MediaKeyMessageEvent</see>
    /// </remarks>
    [Description("@#individualization-request")]
    IndividualizationRequest = 3
}

/// <summary>
/// Let underlying be this&apos;s underlying confidence value, a PerformanceTimingConfidenceValue.
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/navigation-timing/#enumdef-performancetimingconfidencevalue">Navigation Timing Level 2: 3.3.3 The PerformanceTimingConfidenceValue enum</see>
/// </remarks>
[Description("@#PerformanceTimingConfidenceValue")]
[ECMAScript]
[String]
public enum PerformanceTimingConfidenceValue
{
    /// <summary>
    /// If s equals 0, return high.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/navigation-timing/#dom-performancetimingconfidencevalue-high">Navigation Timing Level 2: 3.3.3 The PerformanceTimingConfidenceValue enum</see>
    /// </remarks>
    [Description("@#high")]
    High = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/navigation-timing/#dom-performancetimingconfidencevalue-low">Navigation Timing Level 2: 3.3.3 The PerformanceTimingConfidenceValue enum</see>
    /// </summary>
    [Description("@#low")]
    Low = 1
}

/// <summary>
/// Replace MLActivation with MLRecurrentNetworkActivation for a more specific type for recurrent network activations (#718)
/// </summary>
/// <remarks>
/// <see href="https://webmachinelearning.github.io/webnn/#enumdef-mlrecurrentnetworkactivation">Web Neural Network API: 8.9.25 gru</see>
/// </remarks>
[Description("@#MLRecurrentNetworkActivation")]
[ECMAScript]
[String]
public enum MLRecurrentNetworkActivation
{
    /// <summary>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mlrecurrentnetworkactivation-relu">Web Neural Network API: 8.9.25 gru</see>
    /// </summary>
    [Description("@#relu")]
    Relu = 0,

    /// <summary>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mlrecurrentnetworkactivation-sigmoid">Web Neural Network API: 8.9.25 gru</see>
    /// </summary>
    [Description("@#sigmoid")]
    Sigmoid = 1,

    /// <summary>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mlrecurrentnetworkactivation-tanh">Web Neural Network API: 8.9.25 gru</see>
    /// </summary>
    [Description("@#tanh")]
    Tanh = 2
}

/// <summary>
/// Run the end of stream algorithm with the error parameter set to error:EndOfStreamError.
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/media-source/#dom-endofstreamerror">Media Source Extensions™: 3 MediaSource interface</see>
/// </remarks>
[Description("@#EndOfStreamError")]
[ECMAScript]
[String]
public enum EndOfStreamError
{
    /// <summary>
    /// If error is set to &quot;&quot;network&quot;&quot;
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/media-source/#dom-endofstreamerror-network">Media Source Extensions™: 3 MediaSource interface</see>
    /// </remarks>
    [Description("@#network")]
    Network = 0,

    /// <summary>
    /// This algorithm gets called when the application signals the end of stream via an endOfStream() call or an algorithm needs to signal a decode error. This algorithm takes an error:EndOfStreamError parameter that indicates whether an error will be signalled.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/media-source/#dom-endofstreamerror-decode">Media Source Extensions™: 3 MediaSource interface</see>
    /// </remarks>
    [Description("@#decode")]
    Decode = 1
}

/// <summary>
/// Run these steps: 1. For each output in outputs: 1. Let chunkInit be an EncodedAudioChunkInit with the following keys: 1. Let data contain the encoded audio data from output. 2. Let type be the EncodedAudioChunkType of output. 3. Let timestamp be the timestamp from the AudioData associated with output. 4. Let duration be the duration from the AudioData associated with output. 2. Let chunk be a new EncodedAudioChunk constructed with chunkInit. 3. Let chunkMetadata be a new EncodedAudioChunkMetadata. 4. Let encoderConfig be the active encoder config. 5. Let outputConfig be a new AudioDecoderConfig that describes output....
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webcodecs/#enumdef-encodedaudiochunktype">WebCodecs: 8.1 EncodedAudioChunk Interface</see>
/// </remarks>
[Description("@#EncodedAudioChunkType")]
[ECMAScript]
[String]
public enum EncodedAudioChunkType
{
    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/#dom-encodedaudiochunktype-key">WebCodecs: 8.1 EncodedAudioChunk Interface</see>
    /// </summary>
    [Description("@#key")]
    Key = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/#dom-encodedaudiochunktype-delta">WebCodecs: 8.1 EncodedAudioChunk Interface</see>
    /// </summary>
    [Description("@#delta")]
    Delta = 1
}

/// <summary>
/// Run these steps: 1. For each output in outputs: 1. Let chunkInit be an EncodedVideoChunkInit with the following keys: 1. Let data contain the encoded video data from output. 2. Let type be the EncodedVideoChunkType of output. 3. Let timestamp be the timestamp from the VideoFrame associated with output. 4. Let duration be the duration from the VideoFrame associated with output. 2. Let chunk be a new EncodedVideoChunk constructed with chunkInit. 3. Let chunkMetadata be a new EncodedVideoChunkMetadata. 4. Let encoderConfig be the active encoder config. 5. Let outputConfig be a VideoDecoderConfig that describes output....
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webcodecs/#enumdef-encodedvideochunktype">WebCodecs: 8.2 EncodedVideoChunk Interface</see>
/// </remarks>
[Description("@#EncodedVideoChunkType")]
[ECMAScript]
[String]
public enum EncodedVideoChunkType
{
    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/#dom-encodedvideochunktype-key">WebCodecs: 8.2 EncodedVideoChunk Interface</see>
    /// </summary>
    [Description("@#key")]
    Key = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/#dom-encodedvideochunktype-delta">WebCodecs: 8.2 EncodedVideoChunk Interface</see>
    /// </summary>
    [Description("@#delta")]
    Delta = 1
}

/// <summary>
/// Simplify operand layout support of conv2d and pool2d operations, remove MLRoundingType from pool2d, simplify layout support (#770)
/// </summary>
/// <remarks>
/// <see href="https://webmachinelearning.github.io/webnn/#enumdef-mlroundingtype">Web Neural Network API: 8.9.37 Pooling operations</see>
/// </remarks>
[Description("@#MLRoundingType")]
[ECMAScript]
[String]
public enum MLRoundingType
{
    /// <summary>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mlroundingtype-floor">Web Neural Network API: 8.9.37 Pooling operations</see>
    /// </summary>
    [Description("@#floor")]
    Floor = 0,

    /// <summary>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mlroundingtype-ceil">Web Neural Network API: 8.9.37 Pooling operations</see>
    /// </summary>
    [Description("@#ceil")]
    Ceil = 1
}

/// <summary>
/// Specificies the encoder&apos;s intended application. See OpusApplication.
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webcodecs/opus_codec_registration.html#enumdef-opusapplication">Opus WebCodecs Registration: 5.4 OpusApplication</see>
/// </remarks>
[Description("@#OpusApplication")]
[ECMAScript]
[String]
public enum OpusApplication
{
    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/opus_codec_registration.html#dom-opusapplication-voip">Opus WebCodecs Registration: 5.4 OpusApplication</see>
    /// </summary>
    [Description("@#voip")]
    Voip = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/opus_codec_registration.html#dom-opusapplication-audio">Opus WebCodecs Registration: 5.4 OpusApplication</see>
    /// </summary>
    [Description("@#audio")]
    Audio = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/opus_codec_registration.html#dom-opusapplication-lowdelay">Opus WebCodecs Registration: 5.4 OpusApplication</see>
    /// </summary>
    [Description("@#lowdelay")]
    Lowdelay = 2
}

/// <summary>
/// Specificies the type of audio signal being encoded. See OpusSignal.
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webcodecs/opus_codec_registration.html#enumdef-opussignal">Opus WebCodecs Registration: 5.3 OpusSignal</see>
/// </remarks>
[Description("@#OpusSignal")]
[ECMAScript]
[String]
public enum OpusSignal
{
    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/opus_codec_registration.html#dom-opussignal-auto">Opus WebCodecs Registration: 5.3 OpusSignal</see>
    /// </summary>
    [Description("@#auto")]
    Auto = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/opus_codec_registration.html#dom-opussignal-music">Opus WebCodecs Registration: 5.3 OpusSignal</see>
    /// </summary>
    [Description("@#music")]
    Music = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/opus_codec_registration.html#dom-opussignal-voice">Opus WebCodecs Registration: 5.3 OpusSignal</see>
    /// </summary>
    [Description("@#voice")]
    Voice = 2
}

/// <summary>
/// The AppBannerPromptOutcome enum&apos;s values represent the outcomes from presenting an install prompt.
/// </summary>
/// <remarks>
/// <see href="https://wicg.github.io/manifest-incubations/#dom-appbannerpromptoutcome">Manifest Incubations: 16.1.3 AppBannerPromptOutcome enum</see>
/// </remarks>
[Description("@#AppBannerPromptOutcome")]
[ECMAScript]
[String]
public enum AppBannerPromptOutcome
{
    /// <summary>
    /// Show some user-agent-specific UI, asking the user whether to proceed with installing the app. The result of this choice is either &quot;accepted&quot; or &quot;dismissed&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/manifest-incubations/#dom-appbannerpromptoutcome-accepted">Manifest Incubations: 16.1.3 AppBannerPromptOutcome enum</see>
    /// </remarks>
    [Description("@#accepted")]
    Accepted = 0,

    /// <summary>
    /// Show some user-agent-specific UI, asking the user whether to proceed with installing the app. The result of this choice is either &quot;accepted&quot; or &quot;dismissed&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/manifest-incubations/#dom-appbannerpromptoutcome-dismissed">Manifest Incubations: 16.1.3 AppBannerPromptOutcome enum</see>
    /// </remarks>
    [Description("@#dismissed")]
    Dismissed = 1
}

/// <summary>
/// The MediaSource interface represents a source of media data for an HTMLMediaElement. It keeps track of the readyState for this source as well as a list of SourceBuffer objects that can be used to add media data to the presentation. MediaSource objects are created by the web application and then attached to an HTMLMediaElement. The application uses the SourceBuffer objects in sourceBuffers to add media data to this source. The HTMLMediaElement fetches this media data from the MediaSource object when it is needed during playback.
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/media-source/#dom-readystate">Media Source Extensions™: 3 MediaSource interface</see>
/// </remarks>
[Description("@#ReadyState")]
[ECMAScript]
[String]
public enum ReadyState
{
    /// <summary>
    /// Contains the list of SourceBuffer objects associated with this MediaSource. When MediaSource&apos;s readyState equals &quot;&quot;closed&quot;&quot; this list will be empty. Once readyState transitions to &quot;&quot;open&quot;&quot; SourceBuffer objects can be added to this list by using addSourceBuffer().
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/media-source/#dom-readystate-closed">Media Source Extensions™: 3 MediaSource interface</see>
    /// </remarks>
    [Description("@#closed")]
    Closed = 0,

    /// <summary>
    /// Contains the list of SourceBuffer objects associated with this MediaSource. When MediaSource&apos;s readyState equals &quot;&quot;closed&quot;&quot; this list will be empty. Once readyState transitions to &quot;&quot;open&quot;&quot; SourceBuffer objects can be added to this list by using addSourceBuffer().
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/media-source/#dom-readystate-open">Media Source Extensions™: 3 MediaSource interface</see>
    /// </remarks>
    [Description("@#open")]
    Open = 1,

    /// <summary>
    /// If the readyState attribute is &quot;&quot;ended&quot;&quot; and the new playback position is within a TimeRanges currently in HTMLMediaElement&apos;s buffered, then the seek operation must continue to completion here even if one or more currently selected or enabled track buffers&apos; largest range end timestamp is less than |new playback position|. This condition should only occur due to logic in buffered when readyState is &quot;&quot;ended&quot;&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/media-source/#dom-readystate-ended">Media Source Extensions™: 3 MediaSource interface</see>
    /// </remarks>
    [Description("@#ended")]
    Ended = 2
}

/// <summary>
/// The OrientationLockType enum represents the screen orientations to which a screen can be potentially locked.
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/screen-orientation/#dom-orientationlocktype">Screen Orientation: 6 OrientationLockType enum</see>
/// </remarks>
[Description("@#OrientationLockType")]
[ECMAScript]
[String]
public enum OrientationLockType
{
    /// <summary>
    /// <see href="https://w3c.github.io/screen-orientation/#dom-orientationlocktype-any">Screen Orientation: 6 OrientationLockType enum</see>
    /// </summary>
    [Description("@#any")]
    Any = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/screen-orientation/#dom-orientationlocktype-natural">Screen Orientation: 6 OrientationLockType enum</see>
    /// </summary>
    [Description("@#natural")]
    Natural = 1,

    /// <summary>
    /// To lock the screen orientation to an OrientationLockType orientation means that the screen can only be rotated by the user to a specific screen orientation - possibly at the exclusion of other orientations. The possible orientations to which the screen can be rotated is determined by the user agent, a user preference, the operating system&apos;s conventions, or the screen itself. For example, locking the orientation to landscape means that the screen can be rotated by the user to landscape-primary and maybe landscape-secondary if the system allows it, but won&apos;t change the orientation to portrait-secondary orientation.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/screen-orientation/#dom-orientationlocktype-landscape">Screen Orientation: 6 OrientationLockType enum</see>
    /// </remarks>
    [Description("@#landscape")]
    Landscape = 2,

    /// <summary>
    /// To lock the screen orientation to an OrientationLockType orientation means that the screen can only be rotated by the user to a specific screen orientation - possibly at the exclusion of other orientations. The possible orientations to which the screen can be rotated is determined by the user agent, a user preference, the operating system&apos;s conventions, or the screen itself. For example, locking the orientation to landscape means that the screen can be rotated by the user to landscape-primary and maybe landscape-secondary if the system allows it, but won&apos;t change the orientation to portrait-secondary orientation.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/screen-orientation/#dom-orientationlocktype-portrait">Screen Orientation: 6 OrientationLockType enum</see>
    /// </remarks>
    [Description("@#portrait")]
    Portrait = 3,

    /// <summary>
    /// <see href="https://w3c.github.io/screen-orientation/#dom-orientationlocktype-portrait-primary">Screen Orientation: 6 OrientationLockType enum</see>
    /// </summary>
    [Description("@#portrait-primary")]
    PortraitPrimary = 4,

    /// <summary>
    /// To lock the screen orientation to an OrientationLockType orientation means that the screen can only be rotated by the user to a specific screen orientation - possibly at the exclusion of other orientations. The possible orientations to which the screen can be rotated is determined by the user agent, a user preference, the operating system&apos;s conventions, or the screen itself. For example, locking the orientation to landscape means that the screen can be rotated by the user to landscape-primary and maybe landscape-secondary if the system allows it, but won&apos;t change the orientation to portrait-secondary orientation.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/screen-orientation/#dom-orientationlocktype-portrait-secondary">Screen Orientation: 6 OrientationLockType enum</see>
    /// </remarks>
    [Description("@#portrait-secondary")]
    PortraitSecondary = 5,

    /// <summary>
    /// To lock the screen orientation to an OrientationLockType orientation means that the screen can only be rotated by the user to a specific screen orientation - possibly at the exclusion of other orientations. The possible orientations to which the screen can be rotated is determined by the user agent, a user preference, the operating system&apos;s conventions, or the screen itself. For example, locking the orientation to landscape means that the screen can be rotated by the user to landscape-primary and maybe landscape-secondary if the system allows it, but won&apos;t change the orientation to portrait-secondary orientation.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/screen-orientation/#dom-orientationlocktype-landscape-primary">Screen Orientation: 6 OrientationLockType enum</see>
    /// </remarks>
    [Description("@#landscape-primary")]
    LandscapePrimary = 6,

    /// <summary>
    /// To lock the screen orientation to an OrientationLockType orientation means that the screen can only be rotated by the user to a specific screen orientation - possibly at the exclusion of other orientations. The possible orientations to which the screen can be rotated is determined by the user agent, a user preference, the operating system&apos;s conventions, or the screen itself. For example, locking the orientation to landscape means that the screen can be rotated by the user to landscape-primary and maybe landscape-secondary if the system allows it, but won&apos;t change the orientation to portrait-secondary orientation.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/screen-orientation/#dom-orientationlocktype-landscape-secondary">Screen Orientation: 6 OrientationLockType enum</see>
    /// </remarks>
    [Description("@#landscape-secondary")]
    LandscapeSecondary = 7
}

/// <summary>
/// The OrientationType enum values are used to represent the screen&apos;s Screen/current orientation type.
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/screen-orientation/#dom-orientationtype">Screen Orientation: 7 OrientationType enum</see>
/// </remarks>
[Description("@#OrientationType")]
[ECMAScript]
[String]
public enum OrientationType
{
    /// <summary>
    /// Restrict the possible return values of the type getter to &quot;portrait-primary&quot; or &quot;landscape-primary&quot;. The screen aspect ratio determines which is returned.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/screen-orientation/#dom-orientationtype-portrait-primary">Screen Orientation: 7 OrientationType enum</see>
    /// </remarks>
    [Description("@#portrait-primary")]
    PortraitPrimary = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/screen-orientation/#dom-orientationtype-portrait-secondary">Screen Orientation: 7 OrientationType enum</see>
    /// </summary>
    [Description("@#portrait-secondary")]
    PortraitSecondary = 1,

    /// <summary>
    /// Restrict the possible return values of the type getter to &quot;portrait-primary&quot; or &quot;landscape-primary&quot;. The screen aspect ratio determines which is returned.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/screen-orientation/#dom-orientationtype-landscape-primary">Screen Orientation: 7 OrientationType enum</see>
    /// </remarks>
    [Description("@#landscape-primary")]
    LandscapePrimary = 2,

    /// <summary>
    /// <see href="https://w3c.github.io/screen-orientation/#dom-orientationtype-landscape-secondary">Screen Orientation: 7 OrientationType enum</see>
    /// </summary>
    [Description("@#landscape-secondary")]
    LandscapeSecondary = 3
}

/// <summary>
/// The RemotePlaybackState enum represents possible connection states to a remote playback device.
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/remote-playback/#dom-remoteplaybackstate">Remote Playback API: 5.2 RemotePlayback interface</see>
/// </remarks>
[Description("@#RemotePlaybackState")]
[ECMAScript]
[String]
public enum RemotePlaybackState
{
    /// <summary>
    /// Set the state of the remote:RemotePlayback object to connecting.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/remote-playback/#dom-remoteplaybackstate-connecting">Remote Playback API: 5.2.3 The state attribute</see>
    /// </remarks>
    [Description("@#connecting")]
    Connecting = 0,

    /// <summary>
    /// connected means that the transition from local to remote playback has finished and all media commands now take effect on the remote playback state.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/remote-playback/#dom-remoteplaybackstate-connected">Remote Playback API: 5.2.3 The state attribute</see>
    /// </remarks>
    [Description("@#connected")]
    Connected = 1,

    /// <summary>
    /// disconnected means that the remote playback has not been initiated, has failed to initiate or has been stopped. All media commands will take effect on the local playback state. The remote playback can be initiated through a call to prompt().
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/remote-playback/#dom-remoteplaybackstate-disconnected">Remote Playback API: 5.2.3 The state attribute</see>
    /// </remarks>
    [Description("@#disconnected")]
    Disconnected = 2
}

/// <summary>
/// The expected frame rate in frames per second, if known. This value, along with the frame timestamp, SHOULD be used by the video encoder to calculate the optimal byte length for each encoded frame. Additionally, the value SHOULD be considered a target deadline for outputting encoding chunks when latencyMode is set to realtime.
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webcodecs/#enumdef-latencymode">WebCodecs: 7.11 Latency Mode</see>
/// </remarks>
[Description("@#LatencyMode")]
[ECMAScript]
[String]
public enum LatencyMode
{
    /// <summary>
    /// <see href="https://w3c.github.io/webcodecs/#dom-latencymode-quality">WebCodecs: 7.11 Latency Mode</see>
    /// </summary>
    [Description("@#quality")]
    Quality = 0,

    /// <summary>
    /// The expected frame rate in frames per second, if known. This value, along with the frame timestamp, SHOULD be used by the video encoder to calculate the optimal byte length for each encoded frame. Additionally, the value SHOULD be considered a target deadline for outputting encoding chunks when latencyMode is set to realtime.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-latencymode-realtime">WebCodecs: 7.11 Latency Mode</see>
    /// </remarks>
    [Description("@#realtime")]
    Realtime = 1
}

/// <summary>
/// The mapping in use for this device. If the user agent has knowledge of the layout of the device, then it SHOULD indicate that a mapping is in use by setting mapping to the corresponding GamepadMappingType value.
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/gamepad/#dom-gamepadmappingtype">Gamepad: 7 GamepadMappingType Enum</see>
/// </remarks>
[Description("@#GamepadMappingType")]
[ECMAScript]
[String]
public enum GamepadMappingType
{
    [Description("@#")]
    Empty = 0,

    /// <summary>
    /// If the button and axis layout of the gamepad device corresponds with the Standard Gamepad layout, then return &quot;standard&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/gamepad/#dom-gamepadmappingtype-standard">Gamepad: 7 GamepadMappingType Enum</see>
    /// </remarks>
    [Description("@#standard")]
    Standard = 1,

    /// <summary>
    /// The Gamepad&apos;s controls have been mapped to the &quot;xr-standard&quot; gamepad mapping. This mapping is reserved for use by the [webxr-gamepads-module-1]. Gamepad objects returned by getGamepads() MUST NOT report a mapping of &quot;xr-standard&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/gamepad/#dom-gamepadmappingtype-xr-standard">Gamepad: 7 GamepadMappingType Enum</see>
    /// </remarks>
    [Description("@#xr-standard")]
    XrStandard = 2
}

/// <summary>
/// The object MUST define a new value in the RTCStatsType enum, and MUST define the syntax of the stats object it returns either by reference to an existing sub-dictionary of RTCStats or by defining a new sub-dictionary of RTCStats.
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webrtc-stats/#dom-rtcstatstype">Identifiers for WebRTC&apos;s Statistics API: 7.1 RTCStatsType enum</see>
/// </remarks>
[Description("@#RTCStatsType")]
[ECMAScript]
[String]
public enum RTCStatsType
{
    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-stats/#dom-rtcstatstype-codec">Identifiers for WebRTC&apos;s Statistics API: 7.1 RTCStatsType enum</see>
    /// </summary>
    [Description("@#codec")]
    Codec = 0,

    /// <summary>
    /// For all subclasses of RTCRtpStreamStats, it MUST include ssrc and kind. When stats exist for both sides of a connection, in the form of an &quot;inbound-rtp&quot; / &quot;remote-outbound-rtp&quot; pair or an &quot;outbound-rtp&quot; / &quot;remote-inbound-rtp&quot; pair, the members remoteId and localId MUST also be present.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-stats/#dom-rtcstatstype-inbound-rtp">Identifiers for WebRTC&apos;s Statistics API: 7.1 RTCStatsType enum</see>
    /// </remarks>
    [Description("@#inbound-rtp")]
    InboundRtp = 1,

    /// <summary>
    /// For all subclasses of RTCRtpStreamStats, it MUST include ssrc and kind. When stats exist for both sides of a connection, in the form of an &quot;inbound-rtp&quot; / &quot;remote-outbound-rtp&quot; pair or an &quot;outbound-rtp&quot; / &quot;remote-inbound-rtp&quot; pair, the members remoteId and localId MUST also be present.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-stats/#dom-rtcstatstype-outbound-rtp">Identifiers for WebRTC&apos;s Statistics API: 7.1 RTCStatsType enum</see>
    /// </remarks>
    [Description("@#outbound-rtp")]
    OutboundRtp = 2,

    /// <summary>
    /// For all subclasses of RTCRtpStreamStats, it MUST include ssrc and kind. When stats exist for both sides of a connection, in the form of an &quot;inbound-rtp&quot; / &quot;remote-outbound-rtp&quot; pair or an &quot;outbound-rtp&quot; / &quot;remote-inbound-rtp&quot; pair, the members remoteId and localId MUST also be present.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-stats/#dom-rtcstatstype-remote-inbound-rtp">Identifiers for WebRTC&apos;s Statistics API: 7.1 RTCStatsType enum</see>
    /// </remarks>
    [Description("@#remote-inbound-rtp")]
    RemoteInboundRtp = 3,

    /// <summary>
    /// For all subclasses of RTCRtpStreamStats, it MUST include ssrc and kind. When stats exist for both sides of a connection, in the form of an &quot;inbound-rtp&quot; / &quot;remote-outbound-rtp&quot; pair or an &quot;outbound-rtp&quot; / &quot;remote-inbound-rtp&quot; pair, the members remoteId and localId MUST also be present.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-stats/#dom-rtcstatstype-remote-outbound-rtp">Identifiers for WebRTC&apos;s Statistics API: 7.1 RTCStatsType enum</see>
    /// </remarks>
    [Description("@#remote-outbound-rtp")]
    RemoteOutboundRtp = 4,

    /// <summary>
    /// Media source objects are of either subdictionary RTCAudioSourceStats or RTCVideoSourceStats. The type is the same (&quot;media-source&quot;) but kind is different (&quot;audio&quot; or &quot;video&quot;) depending on the kind of track.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-stats/#dom-rtcstatstype-media-source">Identifiers for WebRTC&apos;s Statistics API: 7.1 RTCStatsType enum</see>
    /// </remarks>
    [Description("@#media-source")]
    MediaSource = 5,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-stats/#dom-rtcstatstype-media-playout">Identifiers for WebRTC&apos;s Statistics API: 7.1 RTCStatsType enum</see>
    /// </summary>
    [Description("@#media-playout")]
    MediaPlayout = 6,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-stats/#dom-rtcstatstype-peer-connection">Identifiers for WebRTC&apos;s Statistics API: 7.1 RTCStatsType enum</see>
    /// </summary>
    [Description("@#peer-connection")]
    PeerConnection = 7,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-stats/#dom-rtcstatstype-data-channel">Identifiers for WebRTC&apos;s Statistics API: 7.1 RTCStatsType enum</see>
    /// </summary>
    [Description("@#data-channel")]
    DataChannel = 8,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-stats/#dom-rtcstatstype-transport">Identifiers for WebRTC&apos;s Statistics API: 7.1 RTCStatsType enum</see>
    /// </summary>
    [Description("@#transport")]
    Transport = 9,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-stats/#dom-rtcstatstype-candidate-pair">Identifiers for WebRTC&apos;s Statistics API: 7.1 RTCStatsType enum</see>
    /// </summary>
    [Description("@#candidate-pair")]
    CandidatePair = 10,

    /// <summary>
    /// The type member, of type RTCStatsType, indicates the type of the object that the RTCStats object represents. An object with a given type can have only one IDL dictionary type, but multiple type values may indicate the same IDL dictionary type; for example, &quot;local-candidate&quot; and &quot;remote-candidate&quot; both use the IDL dictionary type RTCIceCandidateStats.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-stats/#dom-rtcstatstype-local-candidate">Identifiers for WebRTC&apos;s Statistics API: 7.1 RTCStatsType enum</see>
    /// </remarks>
    [Description("@#local-candidate")]
    LocalCandidate = 11,

    /// <summary>
    /// The type member, of type RTCStatsType, indicates the type of the object that the RTCStats object represents. An object with a given type can have only one IDL dictionary type, but multiple type values may indicate the same IDL dictionary type; for example, &quot;local-candidate&quot; and &quot;remote-candidate&quot; both use the IDL dictionary type RTCIceCandidateStats.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-stats/#dom-rtcstatstype-remote-candidate">Identifiers for WebRTC&apos;s Statistics API: 7.1 RTCStatsType enum</see>
    /// </remarks>
    [Description("@#remote-candidate")]
    RemoteCandidate = 12,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-stats/#dom-rtcstatstype-certificate">Identifiers for WebRTC&apos;s Statistics API: 7.1 RTCStatsType enum</see>
    /// </summary>
    [Description("@#certificate")]
    Certificate = 13
}

/// <summary>
/// The proofread result should contain a list of corrections where each ProofreadCorrection, defined by its range from startIndex to endIndex, should describe the types of errors that are corrected according to the CorrectionType enumeration. &quot;false&quot;
/// </summary>
/// <remarks>
/// <see href="https://webmachinelearning.github.io/proofreader-api/#enumdef-correctiontype">Proofreader API: 3 The proofreader API</see>
/// </remarks>
[Description("@#CorrectionType")]
[ECMAScript]
[String]
public enum CorrectionType
{
    /// <summary>
    /// <see href="https://webmachinelearning.github.io/proofreader-api/#dom-correctiontype-spelling">Proofreader API: 3 The proofreader API</see>
    /// </summary>
    [Description("@#spelling")]
    Spelling = 0,

    /// <summary>
    /// <see href="https://webmachinelearning.github.io/proofreader-api/#dom-correctiontype-punctuation">Proofreader API: 3 The proofreader API</see>
    /// </summary>
    [Description("@#punctuation")]
    Punctuation = 1,

    /// <summary>
    /// <see href="https://webmachinelearning.github.io/proofreader-api/#dom-correctiontype-capitalization">Proofreader API: 3 The proofreader API</see>
    /// </summary>
    [Description("@#capitalization")]
    Capitalization = 2,

    /// <summary>
    /// <see href="https://webmachinelearning.github.io/proofreader-api/#dom-correctiontype-grammar">Proofreader API: 3 The proofreader API</see>
    /// </summary>
    [Description("@#grammar")]
    Grammar = 3
}

/// <summary>
/// The type of display surface that is being captured. This assumes values from the DisplayCaptureSurfaceType enumeration.
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/mediacapture-screen-share/#dom-displaycapturesurfacetype">Screen Capture: 5.4.15 DisplayCaptureSurfaceType</see>
/// </remarks>
[Description("@#DisplayCaptureSurfaceType")]
[ECMAScript]
[String]
public enum DisplayCaptureSurfaceType
{
    /// <summary>
    /// <see href="https://w3c.github.io/mediacapture-screen-share/#idl-def-DisplayCaptureSurfaceType.monitor">Screen Capture: 5.4.15 DisplayCaptureSurfaceType</see>
    /// </summary>
    [Description("@#monitor")]
    Monitor = 0,

    /// <summary>
    /// If this.DisplaySurfaceType is neither &quot;browser&quot; nor &quot;window&quot;, exception/throw an &quot;InvalidStateError&quot; DOMException.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-screen-share/#idl-def-DisplayCaptureSurfaceType.window">Screen Capture: 5.4.15 DisplayCaptureSurfaceType</see>
    /// </remarks>
    [Description("@#window")]
    Window = 1,

    /// <summary>
    /// If this.DisplaySurfaceType is neither &quot;browser&quot; nor &quot;window&quot;, exception/throw an &quot;InvalidStateError&quot; DOMException.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-screen-share/#idl-def-DisplayCaptureSurfaceType.browser">Screen Capture: 5.4.15 DisplayCaptureSurfaceType</see>
    /// </remarks>
    [Description("@#browser")]
    Browser = 2
}

/// <summary>
/// This method allows a Web-based payment handler to asynchronously declare its supported PaymentDelegation list.
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/web-based-payment-handler/#dom-paymentdelegation">Web-based Payment Handler API: 4.3 PaymentDelegation enum</see>
/// </remarks>
[Description("@#PaymentDelegation")]
[ECMAScript]
[String]
public enum PaymentDelegation
{
    /// <summary>
    /// <see href="https://w3c.github.io/web-based-payment-handler/#dom-paymentdelegation-shippingaddress">Web-based Payment Handler API: 4.3 PaymentDelegation enum</see>
    /// </summary>
    [Description("@#shippingAddress")]
    ShippingAddress = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/web-based-payment-handler/#dom-paymentdelegation-payername">Web-based Payment Handler API: 4.3 PaymentDelegation enum</see>
    /// </summary>
    [Description("@#payerName")]
    PayerName = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/web-based-payment-handler/#dom-paymentdelegation-payerphone">Web-based Payment Handler API: 4.3 PaymentDelegation enum</see>
    /// </summary>
    [Description("@#payerPhone")]
    PayerPhone = 2,

    /// <summary>
    /// <see href="https://w3c.github.io/web-based-payment-handler/#dom-paymentdelegation-payeremail">Web-based Payment Handler API: 4.3 PaymentDelegation enum</see>
    /// </summary>
    [Description("@#payerEmail")]
    PayerEmail = 3
}

/// <summary>
/// To get the current permission state, given a powerful feature/name name and an optional environment settings object settings, run the following steps. This algorithm returns a PermissionState enum value.
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/permissions/#dom-permissionstate">Permissions: 6.3 PermissionStatus interface</see>
/// </remarks>
[Description("@#PermissionState")]
[ECMAScript]
[String]
public enum PermissionState
{
    /// <summary>
    /// The &quot;granted&quot;, &quot;denied&quot;, and &quot;prompt&quot; enum values represent the concepts of permission/&quot;granted&quot;, permission/&quot;denied&quot;, and permission/&quot;prompt&quot; respectively.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/permissions/#dom-permissionstate-granted">Permissions: 6.3 PermissionStatus interface</see>
    /// </remarks>
    [Description("@#granted")]
    Granted = 0,

    /// <summary>
    /// The &quot;granted&quot;, &quot;denied&quot;, and &quot;prompt&quot; enum values represent the concepts of permission/&quot;granted&quot;, permission/&quot;denied&quot;, and permission/&quot;prompt&quot; respectively.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/permissions/#dom-permissionstate-denied">Permissions: 6.3 PermissionStatus interface</see>
    /// </remarks>
    [Description("@#denied")]
    Denied = 1,

    /// <summary>
    /// The &quot;granted&quot;, &quot;denied&quot;, and &quot;prompt&quot; enum values represent the concepts of permission/&quot;granted&quot;, permission/&quot;denied&quot;, and permission/&quot;prompt&quot; respectively.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/permissions/#dom-permissionstate-prompt">Permissions: 6.3 PermissionStatus interface</see>
    /// </remarks>
    [Description("@#prompt")]
    Prompt = 2
}

/// <summary>
/// audioSelection of type AudioSelectionPreferenceEnum
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/mediacapture-screen-share/#dom-audioselectionpreferenceenum">Screen Capture: 5.4.9 AudioSelectionPreferenceEnum</see>
/// </remarks>
[Description("@#AudioSelectionPreferenceEnum")]
[ECMAScript]
[String]
public enum AudioSelectionPreferenceEnum
{
    /// <summary>
    /// <see href="https://w3c.github.io/mediacapture-screen-share/#idl-def-AudioSelectionPreferenceEnum.preferred">Screen Capture: 5.4.9 AudioSelectionPreferenceEnum</see>
    /// </summary>
    [Description("@#preferred")]
    Preferred = 0
}

/// <summary>
/// degradationPreference of type RTCDegradationPreference.
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/mst-content-hint/#dom-rtcdegradationpreference">MediaStreamTrack Content Hints: 4.2 Degradation preference when encoding</see>
/// </remarks>
[Description("@#RTCDegradationPreference")]
[ECMAScript]
[String]
public enum RTCDegradationPreference
{
    /// <summary>
    /// For a video track with the attribute value &quot;motion&quot;, use &quot;maintain-framerate&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mst-content-hint/#dom-rtcdegradationpreference-maintain-framerate">MediaStreamTrack Content Hints: 4.2 Degradation preference when encoding</see>
    /// </remarks>
    [Description("@#maintain-framerate")]
    MaintainFramerate = 0,

    /// <summary>
    /// For a video track with the attribute value &quot;detail&quot;, use &quot;maintain-resolution&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mst-content-hint/#dom-rtcdegradationpreference-maintain-resolution">MediaStreamTrack Content Hints: 4.2 Degradation preference when encoding</see>
    /// </remarks>
    [Description("@#maintain-resolution")]
    MaintainResolution = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/mst-content-hint/#dom-rtcdegradationpreference-balanced">MediaStreamTrack Content Hints: 4.2 Degradation preference when encoding</see>
    /// </summary>
    [Description("@#balanced")]
    Balanced = 2,

    /// <summary>
    /// <see href="https://w3c.github.io/mst-content-hint/#dom-rtcdegradationpreference-maintain-framerate-and-resolution">MediaStreamTrack Content Hints: 4.2 Degradation preference when encoding</see>
    /// </summary>
    [Description("@#maintain-framerate-and-resolution")]
    MaintainFramerateAndResolution = 3
}

/// <summary>
/// dtlsRole of type RTCDtlsRole
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webrtc-stats/#dom-rtcdtlsrole">Identifiers for WebRTC&apos;s Statistics API: RTCDtlsRole enum</see>
/// </remarks>
[Description("@#RTCDtlsRole")]
[ECMAScript]
[String]
public enum RTCDtlsRole
{
    /// <summary>
    /// &quot;client&quot; or &quot;server&quot; depending on the DTLS role. &quot;unknown&quot; before the DTLS negotiation starts.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-stats/#dom-rtcdtlsrole-client">Identifiers for WebRTC&apos;s Statistics API: RTCDtlsRole enum</see>
    /// </remarks>
    [Description("@#client")]
    Client = 0,

    /// <summary>
    /// &quot;client&quot; or &quot;server&quot; depending on the DTLS role. &quot;unknown&quot; before the DTLS negotiation starts.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-stats/#dom-rtcdtlsrole-server">Identifiers for WebRTC&apos;s Statistics API: RTCDtlsRole enum</see>
    /// </remarks>
    [Description("@#server")]
    Server = 1,

    /// <summary>
    /// &quot;client&quot; or &quot;server&quot; depending on the DTLS role. &quot;unknown&quot; before the DTLS negotiation starts.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-stats/#dom-rtcdtlsrole-unknown">Identifiers for WebRTC&apos;s Statistics API: RTCDtlsRole enum</see>
    /// </remarks>
    [Description("@#unknown")]
    Unknown = 2
}

/// <summary>
/// fillLightMode
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/mediacapture-image/#enumdef-filllightmode">MediaStream Image Capture: 8 FillLightMode</see>
/// </remarks>
[Description("@#FillLightMode")]
[ECMAScript]
[String]
public enum FillLightMode
{
    /// <summary>
    /// <see href="https://w3c.github.io/mediacapture-image/#dom-filllightmode-auto">MediaStream Image Capture: 8.1 Values</see>
    /// </summary>
    [Description("@#auto")]
    Auto = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/mediacapture-image/#dom-filllightmode-off">MediaStream Image Capture: 8.1 Values</see>
    /// </summary>
    [Description("@#off")]
    Off = 1,

    /// <summary>
    /// This reflects the supported fill light mode (flash) settings, if any.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-image/#dom-filllightmode-flash">MediaStream Image Capture: 8.1 Values</see>
    /// </remarks>
    [Description("@#flash")]
    Flash = 2
}

/// <summary>
/// hardwareAcceleration
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webcodecs/#enumdef-hardwareacceleration">WebCodecs: 7.9 Hardware Acceleration</see>
/// </remarks>
[Description("@#HardwareAcceleration")]
[ECMAScript]
[String]
public enum HardwareAcceleration
{
    /// <summary>
    /// no-preference
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-hardwareacceleration-no-preference">WebCodecs: 7.9 Hardware Acceleration</see>
    /// </remarks>
    [Description("@#no-preference")]
    NoPreference = 0,

    /// <summary>
    /// prefer-hardware
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-hardwareacceleration-prefer-hardware">WebCodecs: 7.9 Hardware Acceleration</see>
    /// </remarks>
    [Description("@#prefer-hardware")]
    PreferHardware = 1,

    /// <summary>
    /// prefer-software
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-hardwareacceleration-prefer-software">WebCodecs: 7.9 Hardware Acceleration</see>
    /// </remarks>
    [Description("@#prefer-software")]
    PreferSoftware = 2
}

/// <summary>
/// mode of type AppendMode
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/media-source/#dom-appendmode">Media Source Extensions™: 5 SourceBuffer interface</see>
/// </remarks>
[Description("@#AppendMode")]
[ECMAScript]
[String]
public enum AppendMode
{
    /// <summary>
    /// If buffer&apos;s generate timestamps flag is true, set buffer&apos;s mode to &quot;sequence&quot;. Otherwise, set buffer&apos;s mode to &quot;segments&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/media-source/#dom-appendmode-segments">Media Source Extensions™: 5 SourceBuffer interface</see>
    /// </remarks>
    [Description("@#segments")]
    Segments = 0,

    /// <summary>
    /// If buffer&apos;s generate timestamps flag is true, set buffer&apos;s mode to &quot;sequence&quot;. Otherwise, set buffer&apos;s mode to &quot;segments&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/media-source/#dom-appendmode-sequence">Media Source Extensions™: 5 SourceBuffer interface</see>
    /// </remarks>
    [Description("@#sequence")]
    Sequence = 1
}

/// <summary>
/// monitorTypeSurfaces of type MonitorTypeSurfacesEnum
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/mediacapture-screen-share/#dom-monitortypesurfacesenum">Screen Capture: 5.4.8 MonitorTypeSurfacesEnum</see>
/// </remarks>
[Description("@#MonitorTypeSurfacesEnum")]
[ECMAScript]
[String]
public enum MonitorTypeSurfacesEnum
{
    /// <summary>
    /// <see href="https://w3c.github.io/mediacapture-screen-share/#idl-def-MonitorTypeSurfacesEnum.include">Screen Capture: 5.4.8 MonitorTypeSurfacesEnum</see>
    /// </summary>
    [Description("@#include")]
    Include = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/mediacapture-screen-share/#idl-def-MonitorTypeSurfacesEnum.exclude">Screen Capture: 5.4.8 MonitorTypeSurfacesEnum</see>
    /// </summary>
    [Description("@#exclude")]
    Exclude = 1
}

/// <summary>
/// qualityLimitationReason of type RTCQualityLimitationReason
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webrtc-stats/#dom-rtcqualitylimitationreason">Identifiers for WebRTC&apos;s Statistics API: 8.9 RTCQualityLimitationReason enum</see>
/// </remarks>
[Description("@#RTCQualityLimitationReason")]
[ECMAScript]
[String]
public enum RTCQualityLimitationReason
{
    /// <summary>
    /// MUST NOT map/exist for audio. The current reason for limiting the resolution and/or framerate, or &quot;none&quot; if not limited.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-stats/#dom-rtcqualitylimitationreason-none">Identifiers for WebRTC&apos;s Statistics API: 8.9 RTCQualityLimitationReason enum</see>
    /// </remarks>
    [Description("@#none")]
    None = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-stats/#dom-rtcqualitylimitationreason-cpu">Identifiers for WebRTC&apos;s Statistics API: 8.9 RTCQualityLimitationReason enum</see>
    /// </summary>
    [Description("@#cpu")]
    Cpu = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-stats/#dom-rtcqualitylimitationreason-bandwidth">Identifiers for WebRTC&apos;s Statistics API: 8.9 RTCQualityLimitationReason enum</see>
    /// </summary>
    [Description("@#bandwidth")]
    Bandwidth = 2,

    /// <summary>
    /// MUST NOT map/exist for audio. The number of times that the resolution has changed because we are quality limited (qualityLimitationReason has a value other than &quot;none&quot;). The counter is initially zero and increases when the resolution goes up or down. For example, if a 720p track is sent as 480p for some time and then recovers to 720p, qualityLimitationResolutionChanges will have the value 2.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-stats/#dom-rtcqualitylimitationreason-other">Identifiers for WebRTC&apos;s Statistics API: 8.9 RTCQualityLimitationReason enum</see>
    /// </remarks>
    [Description("@#other")]
    Other = 3
}

/// <summary>
/// readyState of type MediaStreamTrackState, readonly
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/mediacapture-main/#dom-mediastreamtrackstate">Media Capture and Streams: 4.3.3 Interface Definition</see>
/// </remarks>
[Description("@#MediaStreamTrackState")]
[ECMAScript]
[String]
public enum MediaStreamTrackState
{
    /// <summary>
    /// \ReadyState, initialized to &quot;live&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-main/#idl-def-MediaStreamTrackState.live">Media Capture and Streams: 4.3.3 Interface Definition</see>
    /// </remarks>
    [Description("@#live")]
    Live = 0,

    /// <summary>
    /// For each MediaStreamTrack object track whose relevant global object is globalObject, set track&apos;s ReadyState to &quot;ended&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-main/#idl-def-MediaStreamTrackState.ended">Media Capture and Streams: 4.3.3 Interface Definition</see>
    /// </remarks>
    [Description("@#ended")]
    Ended = 1
}

/// <summary>
/// redEyeReduction
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/mediacapture-image/#enumdef-redeyereduction">MediaStream Image Capture: 7 RedEyeReduction</see>
/// </remarks>
[Description("@#RedEyeReduction")]
[ECMAScript]
[String]
public enum RedEyeReduction
{
    /// <summary>
    /// <see href="https://w3c.github.io/mediacapture-image/#dom-redeyereduction-never">MediaStream Image Capture: 7.1 Values</see>
    /// </summary>
    [Description("@#never")]
    Never = 0,

    /// <summary>
    /// Red eye reduction is available in the device and it is always configured to true.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-image/#dom-redeyereduction-always">MediaStream Image Capture: 7.1 Values</see>
    /// </remarks>
    [Description("@#always")]
    Always = 1,

    /// <summary>
    /// controllable
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-image/#dom-redeyereduction-controllable">MediaStream Image Capture: 7.1 Values</see>
    /// </remarks>
    [Description("@#controllable")]
    Controllable = 2
}

/// <summary>
/// selfBrowserSurface of type SelfCapturePreferenceEnum
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/mediacapture-screen-share/#dom-selfcapturepreferenceenum">Screen Capture: 5.4.4 SelfCapturePreferenceEnum</see>
/// </remarks>
[Description("@#SelfCapturePreferenceEnum")]
[ECMAScript]
[String]
public enum SelfCapturePreferenceEnum
{
    /// <summary>
    /// <see href="https://w3c.github.io/mediacapture-screen-share/#idl-def-SelfCapturePreferenceEnum.include">Screen Capture: 5.4.4 SelfCapturePreferenceEnum</see>
    /// </summary>
    [Description("@#include")]
    Include = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/mediacapture-screen-share/#idl-def-SelfCapturePreferenceEnum.exclude">Screen Capture: 5.4.4 SelfCapturePreferenceEnum</see>
    /// </summary>
    [Description("@#exclude")]
    Exclude = 1
}

/// <summary>
/// state of type RTCStatsIceCandidatePairState
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webrtc-stats/#dom-rtcstatsicecandidatepairstate">Identifiers for WebRTC&apos;s Statistics API: 8.19.1 RTCStatsIceCandidatePairState enum</see>
/// </remarks>
[Description("@#RTCStatsIceCandidatePairState")]
[ECMAScript]
[String]
public enum RTCStatsIceCandidatePairState
{
    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-stats/#dom-rtcstatsicecandidatepairstate-frozen">Identifiers for WebRTC&apos;s Statistics API: 8.19.1 RTCStatsIceCandidatePairState enum</see>
    /// </summary>
    [Description("@#frozen")]
    Frozen = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-stats/#dom-rtcstatsicecandidatepairstate-waiting">Identifiers for WebRTC&apos;s Statistics API: 8.19.1 RTCStatsIceCandidatePairState enum</see>
    /// </summary>
    [Description("@#waiting")]
    Waiting = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-stats/#dom-rtcstatsicecandidatepairstate-in-progress">Identifiers for WebRTC&apos;s Statistics API: 8.19.1 RTCStatsIceCandidatePairState enum</see>
    /// </summary>
    [Description("@#in-progress")]
    InProgress = 2,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-stats/#dom-rtcstatsicecandidatepairstate-failed">Identifiers for WebRTC&apos;s Statistics API: 8.19.1 RTCStatsIceCandidatePairState enum</see>
    /// </summary>
    [Description("@#failed")]
    Failed = 3,

    /// <summary>
    /// <see href="https://w3c.github.io/webrtc-stats/#dom-rtcstatsicecandidatepairstate-succeeded">Identifiers for WebRTC&apos;s Statistics API: 8.19.1 RTCStatsIceCandidatePairState enum</see>
    /// </summary>
    [Description("@#succeeded")]
    Succeeded = 4
}

/// <summary>
/// surfaceSwitching of type SurfaceSwitchingPreferenceEnum
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/mediacapture-screen-share/#dom-surfaceswitchingpreferenceenum">Screen Capture: 5.4.7 SurfaceSwitchingPreferenceEnum</see>
/// </remarks>
[Description("@#SurfaceSwitchingPreferenceEnum")]
[ECMAScript]
[String]
public enum SurfaceSwitchingPreferenceEnum
{
    /// <summary>
    /// <see href="https://w3c.github.io/mediacapture-screen-share/#idl-def-SurfaceSwitchingPreferenceEnum.include">Screen Capture: 5.4.7 SurfaceSwitchingPreferenceEnum</see>
    /// </summary>
    [Description("@#include")]
    Include = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/mediacapture-screen-share/#idl-def-SurfaceSwitchingPreferenceEnum.exclude">Screen Capture: 5.4.7 SurfaceSwitchingPreferenceEnum</see>
    /// </summary>
    [Description("@#exclude")]
    Exclude = 1
}

/// <summary>
/// systemAudio of type SystemAudioPreferenceEnum
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/mediacapture-screen-share/#dom-systemaudiopreferenceenum">Screen Capture: 5.4.5 SystemAudioPreferenceEnum</see>
/// </remarks>
[Description("@#SystemAudioPreferenceEnum")]
[ECMAScript]
[String]
public enum SystemAudioPreferenceEnum
{
    /// <summary>
    /// <see href="https://w3c.github.io/mediacapture-screen-share/#idl-def-SystemAudioPreferenceEnum.include">Screen Capture: 5.4.5 SystemAudioPreferenceEnum</see>
    /// </summary>
    [Description("@#include")]
    Include = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/mediacapture-screen-share/#idl-def-SystemAudioPreferenceEnum.exclude">Screen Capture: 5.4.5 SystemAudioPreferenceEnum</see>
    /// </summary>
    [Description("@#exclude")]
    Exclude = 1
}

/// <summary>
/// underline style, a UnderlineStyle which is the preferred underline style of the decorated text range.
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/edit-context/#dom-underlinestyle">EditContext API: 4.2 TextFormatUpdateEvent</see>
/// </remarks>
[Description("@#UnderlineStyle")]
[ECMAScript]
[String]
public enum UnderlineStyle
{
    /// <summary>
    /// <see href="https://w3c.github.io/edit-context/#dom-underlinestyle-none">EditContext API: 4.2 TextFormatUpdateEvent</see>
    /// </summary>
    [Description("@#none")]
    None = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/edit-context/#dom-underlinestyle-solid">EditContext API: 4.2 TextFormatUpdateEvent</see>
    /// </summary>
    [Description("@#solid")]
    Solid = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/edit-context/#dom-underlinestyle-dotted">EditContext API: 4.2 TextFormatUpdateEvent</see>
    /// </summary>
    [Description("@#dotted")]
    Dotted = 2,

    /// <summary>
    /// <see href="https://w3c.github.io/edit-context/#dom-underlinestyle-dashed">EditContext API: 4.2 TextFormatUpdateEvent</see>
    /// </summary>
    [Description("@#dashed")]
    Dashed = 3,

    /// <summary>
    /// <see href="https://w3c.github.io/edit-context/#dom-underlinestyle-wavy">EditContext API: 4.2 TextFormatUpdateEvent</see>
    /// </summary>
    [Description("@#wavy")]
    Wavy = 4
}

/// <summary>
/// underline thickness, a UnderlineThickness which is the preferred underline thickness of the decorated text range.
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/edit-context/#dom-underlinethickness">EditContext API: 4.2 TextFormatUpdateEvent</see>
/// </remarks>
[Description("@#UnderlineThickness")]
[ECMAScript]
[String]
public enum UnderlineThickness
{
    /// <summary>
    /// <see href="https://w3c.github.io/edit-context/#dom-underlinethickness-none">EditContext API: 4.2 TextFormatUpdateEvent</see>
    /// </summary>
    [Description("@#none")]
    None = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/edit-context/#dom-underlinethickness-thin">EditContext API: 4.2 TextFormatUpdateEvent</see>
    /// </summary>
    [Description("@#thin")]
    Thin = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/edit-context/#dom-underlinethickness-thick">EditContext API: 4.2 TextFormatUpdateEvent</see>
    /// </summary>
    [Description("@#thick")]
    Thick = 2
}

/// <summary>
/// windowAudio of type WindowAudioPreferenceEnum
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/mediacapture-screen-share/#dom-windowaudiopreferenceenum">Screen Capture: 5.4.6 WindowAudioPreferenceEnum</see>
/// </remarks>
[Description("@#WindowAudioPreferenceEnum")]
[ECMAScript]
[String]
public enum WindowAudioPreferenceEnum
{
    /// <summary>
    /// <see href="https://w3c.github.io/mediacapture-screen-share/#idl-def-WindowAudioPreferenceEnum.system">Screen Capture: 5.4.6 WindowAudioPreferenceEnum</see>
    /// </summary>
    [Description("@#system")]
    System = 0,

    /// <summary>
    /// <see href="https://w3c.github.io/mediacapture-screen-share/#idl-def-WindowAudioPreferenceEnum.window">Screen Capture: 5.4.6 WindowAudioPreferenceEnum</see>
    /// </summary>
    [Description("@#window")]
    Window = 1,

    /// <summary>
    /// <see href="https://w3c.github.io/mediacapture-screen-share/#idl-def-WindowAudioPreferenceEnum.exclude">Screen Capture: 5.4.6 WindowAudioPreferenceEnum</see>
    /// </summary>
    [Description("@#exclude")]
    Exclude = 2
}

[Description("@#WebGLPowerPreference")]
[ECMAScript]
[String]
public enum WebGLPowerPreference
{
    [Description("@#default")]
    Default = 0,

    [Description("@#low-power")]
    LowPower = 1,

    [Description("@#high-performance")]
    HighPerformance = 2
}
