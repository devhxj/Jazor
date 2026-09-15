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
    /// <summary>
    /// JavaScript 字符串取值 “”；属于 RequestDestination 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://fetch.spec.whatwg.org/">Fetch Standard: RequestDestination.</see>
    /// </remarks>
[Description("@#")]
    Empty = 0,

    /// <summary>
    /// JavaScript 字符串取值 “audio”；属于 RequestDestination 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://fetch.spec.whatwg.org/#dom-requestdestination-audio">Fetch Standard: 5.4 Request class</see>
    /// </remarks>
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
    /// JavaScript 字符串取值 “worker”；属于 RequestDestination 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://fetch.spec.whatwg.org/#dom-requestdestination-worker">Fetch Standard: 5.4 Request class</see>
    /// </remarks>
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
/// WebIDL enum CompressionFormat。定义于 Compression Standard。
/// </summary>
/// <remarks>
/// <see href="https://compression.spec.whatwg.org/#enumdef-compressionformat">Compression Standard: 4 Interface CompressionStream</see>
/// </remarks>
[Description("@#CompressionFormat")]
[ECMAScript]
[String]
public enum CompressionFormat
{
    /// <summary>
    /// JavaScript 字符串取值 “brotli”；属于 CompressionFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://compression.spec.whatwg.org/#dom-compressionformat-brotli">Compression Standard: 4 Interface CompressionStream</see>
    /// </remarks>
    [Description("@#brotli")]
    Brotli = 0,

    /// <summary>
    /// JavaScript 字符串取值 “deflate”；属于 CompressionFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://compression.spec.whatwg.org/#dom-compressionformat-deflate">Compression Standard: 4 Interface CompressionStream</see>
    /// </remarks>
    [Description("@#deflate")]
    Deflate = 1,

    /// <summary>
    /// JavaScript 字符串取值 “deflate-raw”；属于 CompressionFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://compression.spec.whatwg.org/#dom-compressionformat-deflate-raw">Compression Standard: 4 Interface CompressionStream</see>
    /// </remarks>
    [Description("@#deflate-raw")]
    DeflateRaw = 2,

    /// <summary>
    /// JavaScript 字符串取值 “gzip”；属于 CompressionFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://compression.spec.whatwg.org/#dom-compressionformat-gzip">Compression Standard: 4 Interface CompressionStream</see>
    /// </remarks>
    [Description("@#gzip")]
    Gzip = 3
}

/// <summary>
/// WebIDL enum CookieSameSite。定义于 Cookie Store API Standard。
/// </summary>
/// <remarks>
/// <see href="https://cookiestore.spec.whatwg.org/#enumdef-cookiesamesite">Cookie Store API Standard: 3 The CookieStore interface</see>
/// </remarks>
[Description("@#CookieSameSite")]
[ECMAScript]
[String]
public enum CookieSameSite
{
    /// <summary>
    /// JavaScript 字符串取值 “strict”；属于 CookieSameSite 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://cookiestore.spec.whatwg.org/#dom-cookiesamesite-strict">Cookie Store API Standard: 3 The CookieStore interface</see>
    /// </remarks>
    [Description("@#strict")]
    Strict = 0,

    /// <summary>
    /// JavaScript 字符串取值 “lax”；属于 CookieSameSite 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://cookiestore.spec.whatwg.org/#dom-cookiesamesite-lax">Cookie Store API Standard: 3 The CookieStore interface</see>
    /// </remarks>
    [Description("@#lax")]
    Lax = 1,

    /// <summary>
    /// JavaScript 字符串取值 “none”；属于 CookieSameSite 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://cookiestore.spec.whatwg.org/#dom-cookiesamesite-none">Cookie Store API Standard: 3 The CookieStore interface</see>
    /// </remarks>
    [Description("@#none")]
    None = 2
}

/// <summary>
/// WebIDL enum ShadowRootMode。定义于 DOM Standard。
/// </summary>
/// <remarks>
/// <see href="https://dom.spec.whatwg.org/#enumdef-shadowrootmode">DOM Standard: 4.8 Interface ShadowRoot</see>
/// </remarks>
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
/// WebIDL enum SlotAssignmentMode。定义于 DOM Standard。
/// </summary>
/// <remarks>
/// <see href="https://dom.spec.whatwg.org/#enumdef-slotassignmentmode">DOM Standard: 4.8 Interface ShadowRoot</see>
/// </remarks>
[Description("@#SlotAssignmentMode")]
[ECMAScript]
[String]
public enum SlotAssignmentMode
{
    /// <summary>
    /// JavaScript 字符串取值 “manual”；属于 SlotAssignmentMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://dom.spec.whatwg.org/#dom-slotassignmentmode-manual">DOM Standard: 4.8 Interface ShadowRoot</see>
    /// </remarks>
    [Description("@#manual")]
    Manual = 0,

    /// <summary>
    /// JavaScript 字符串取值 “named”；属于 SlotAssignmentMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://dom.spec.whatwg.org/#dom-slotassignmentmode-named">DOM Standard: 4.8 Interface ShadowRoot</see>
    /// </remarks>
    [Description("@#named")]
    Named = 1
}

/// <summary>
/// WebIDL enum FontFaceLoadStatus。定义于 CSS Font Loading Module Level 3。
/// </summary>
/// <remarks>
/// <see href="https://drafts.csswg.org/css-font-loading-3/#enumdef-fontfaceloadstatus">CSS Font Loading Module Level 3: 2 The FontFace Interface</see>
/// </remarks>
[Description("@#FontFaceLoadStatus")]
[ECMAScript]
[String]
public enum FontFaceLoadStatus
{
    /// <summary>
    /// JavaScript 字符串取值 “unloaded”；属于 FontFaceLoadStatus 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/css-font-loading-3/#dom-fontfaceloadstatus-unloaded">CSS Font Loading Module Level 3: 2 The FontFace Interface</see>
    /// </remarks>
    [Description("@#unloaded")]
    Unloaded = 0,

    /// <summary>
    /// JavaScript 字符串取值 “loading”；属于 FontFaceLoadStatus 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/css-font-loading-3/#dom-fontfaceloadstatus-loading">CSS Font Loading Module Level 3: 2 The FontFace Interface</see>
    /// </remarks>
    [Description("@#loading")]
    Loading = 1,

    /// <summary>
    /// JavaScript 字符串取值 “loaded”；属于 FontFaceLoadStatus 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/css-font-loading-3/#dom-fontfaceloadstatus-loaded">CSS Font Loading Module Level 3: 2 The FontFace Interface</see>
    /// </remarks>
    [Description("@#loaded")]
    Loaded = 2,

    /// <summary>
    /// JavaScript 字符串取值 “error”；属于 FontFaceLoadStatus 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/css-font-loading-3/#dom-fontfaceloadstatus-error">CSS Font Loading Module Level 3: 2 The FontFace Interface</see>
    /// </remarks>
    [Description("@#error")]
    Error = 3
}

/// <summary>
/// WebIDL enum FontFaceSetLoadStatus。定义于 CSS Font Loading Module Level 3。
/// </summary>
/// <remarks>
/// <see href="https://drafts.csswg.org/css-font-loading-3/#enumdef-fontfacesetloadstatus">CSS Font Loading Module Level 3: 3 The FontFaceSet Interface</see>
/// </remarks>
[Description("@#FontFaceSetLoadStatus")]
[ECMAScript]
[String]
public enum FontFaceSetLoadStatus
{
    /// <summary>
    /// JavaScript 字符串取值 “loading”；属于 FontFaceSetLoadStatus 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/css-font-loading-3/#dom-fontfacesetloadstatus-loading">CSS Font Loading Module Level 3: 3 The FontFaceSet Interface</see>
    /// </remarks>
    [Description("@#loading")]
    Loading = 0,

    /// <summary>
    /// JavaScript 字符串取值 “loaded”；属于 FontFaceSetLoadStatus 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/css-font-loading-3/#dom-fontfacesetloadstatus-loaded">CSS Font Loading Module Level 3: 3 The FontFaceSet Interface</see>
    /// </remarks>
    [Description("@#loaded")]
    Loaded = 1
}

/// <summary>
/// WebIDL enum FocusableAreaSearchMode。定义于 CSS Spatial Navigation Module Level 1。
/// </summary>
/// <remarks>
/// <see href="https://drafts.csswg.org/css-spatial-nav-1/#enumdef-focusableareasearchmode">CSS Spatial Navigation Module Level 1: 5.2 Low level APIs</see>
/// </remarks>
[Description("@#FocusableAreaSearchMode")]
[ECMAScript]
[String]
public enum FocusableAreaSearchMode
{
    /// <summary>
    /// JavaScript 字符串取值 “visible”；属于 FocusableAreaSearchMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/css-spatial-nav-1/#dom-focusableareasearchmode-visible">CSS Spatial Navigation Module Level 1: 5.2 Low level APIs</see>
    /// </remarks>
    [Description("@#visible")]
    Visible = 0,

    /// <summary>
    /// JavaScript 字符串取值 “all”；属于 FocusableAreaSearchMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/css-spatial-nav-1/#dom-focusableareasearchmode-all">CSS Spatial Navigation Module Level 1: 5.2 Low level APIs</see>
    /// </remarks>
    [Description("@#all")]
    All = 1
}

/// <summary>
/// WebIDL enum SpatialNavigationDirection。定义于 CSS Spatial Navigation Module Level 1。
/// </summary>
/// <remarks>
/// <see href="https://drafts.csswg.org/css-spatial-nav-1/#enumdef-spatialnavigationdirection">CSS Spatial Navigation Module Level 1: 5.1 Triggering Navigation Programmatically</see>
/// </remarks>
[Description("@#SpatialNavigationDirection")]
[ECMAScript]
[String]
public enum SpatialNavigationDirection
{
    /// <summary>
    /// JavaScript 字符串取值 “up”；属于 SpatialNavigationDirection 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/css-spatial-nav-1/#dom-spatialnavigationdirection-up">CSS Spatial Navigation Module Level 1: 5.1 Triggering Navigation Programmatically</see>
    /// </remarks>
    [Description("@#up")]
    Up = 0,

    /// <summary>
    /// JavaScript 字符串取值 “down”；属于 SpatialNavigationDirection 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/css-spatial-nav-1/#dom-spatialnavigationdirection-down">CSS Spatial Navigation Module Level 1: 5.1 Triggering Navigation Programmatically</see>
    /// </remarks>
    [Description("@#down")]
    Down = 1,

    /// <summary>
    /// JavaScript 字符串取值 “left”；属于 SpatialNavigationDirection 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/css-spatial-nav-1/#dom-spatialnavigationdirection-left">CSS Spatial Navigation Module Level 1: 5.1 Triggering Navigation Programmatically</see>
    /// </remarks>
    [Description("@#left")]
    Left = 2,

    /// <summary>
    /// JavaScript 字符串取值 “right”；属于 SpatialNavigationDirection 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/css-spatial-nav-1/#dom-spatialnavigationdirection-right">CSS Spatial Navigation Module Level 1: 5.1 Triggering Navigation Programmatically</see>
    /// </remarks>
    [Description("@#right")]
    Right = 3
}

/// <summary>
/// WebIDL enum CSSBoxType。定义于 CSSOM View Module Level 1。
/// </summary>
/// <remarks>
/// <see href="https://drafts.csswg.org/cssom-view-1/#enumdef-cssboxtype">CSSOM View Module Level 1: 11.1 The GeometryUtils Interface</see>
/// </remarks>
[Description("@#CSSBoxType")]
[ECMAScript]
[String]
public enum CSSBoxType
{
    /// <summary>
    /// JavaScript 字符串取值 “margin”；属于 CSSBoxType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/cssom-view-1/#dom-cssboxtype-margin">CSSOM View Module Level 1: 11.1 The GeometryUtils Interface</see>
    /// </remarks>
    [Description("@#margin")]
    Margin = 0,

    /// <summary>
    /// JavaScript 字符串取值 “border”；属于 CSSBoxType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/cssom-view-1/#dom-cssboxtype-border">CSSOM View Module Level 1: 11.1 The GeometryUtils Interface</see>
    /// </remarks>
    [Description("@#border")]
    Border = 1,

    /// <summary>
    /// JavaScript 字符串取值 “padding”；属于 CSSBoxType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/cssom-view-1/#dom-cssboxtype-padding">CSSOM View Module Level 1: 11.1 The GeometryUtils Interface</see>
    /// </remarks>
    [Description("@#padding")]
    Padding = 2,

    /// <summary>
    /// JavaScript 字符串取值 “content”；属于 CSSBoxType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/cssom-view-1/#dom-cssboxtype-content">CSSOM View Module Level 1: 11.1 The GeometryUtils Interface</see>
    /// </remarks>
    [Description("@#content")]
    Content = 3
}

/// <summary>
/// WebIDL enum ScrollBehavior。定义于 CSSOM View Module Level 1。
/// </summary>
/// <remarks>
/// <see href="https://drafts.csswg.org/cssom-view-1/#enumdef-scrollbehavior">CSSOM View Module Level 1: 4 Extensions to the Window Interface</see>
/// </remarks>
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
    /// JavaScript 字符串取值 “instant”；属于 ScrollBehavior 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/cssom-view-1/#dom-scrollbehavior-instant">CSSOM View Module Level 1: 4 Extensions to the Window Interface</see>
    /// </remarks>
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
/// WebIDL enum ScrollIntoViewContainer。定义于 CSSOM View Module Level 1。
/// </summary>
/// <remarks>
/// <see href="https://drafts.csswg.org/cssom-view-1/#enumdef-scrollintoviewcontainer">CSSOM View Module Level 1: 6 Extensions to the Element Interface</see>
/// </remarks>
[Description("@#ScrollIntoViewContainer")]
[ECMAScript]
[String]
public enum ScrollIntoViewContainer
{
    /// <summary>
    /// JavaScript 字符串取值 “all”；属于 ScrollIntoViewContainer 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/cssom-view-1/#dom-scrollintoviewcontainer-all">CSSOM View Module Level 1: 6 Extensions to the Element Interface</see>
    /// </remarks>
    [Description("@#all")]
    All = 0,

    /// <summary>
    /// JavaScript 字符串取值 “nearest”；属于 ScrollIntoViewContainer 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/cssom-view-1/#dom-scrollintoviewcontainer-nearest">CSSOM View Module Level 1: 6 Extensions to the Element Interface</see>
    /// </remarks>
    [Description("@#nearest")]
    Nearest = 1
}

/// <summary>
/// WebIDL enum ScrollLogicalPosition。定义于 CSSOM View Module Level 1。
/// </summary>
/// <remarks>
/// <see href="https://drafts.csswg.org/cssom-view-1/#enumdef-scrolllogicalposition">CSSOM View Module Level 1: 6 Extensions to the Element Interface</see>
/// </remarks>
[Description("@#ScrollLogicalPosition")]
[ECMAScript]
[String]
public enum ScrollLogicalPosition
{
    /// <summary>
    /// JavaScript 字符串取值 “start”；属于 ScrollLogicalPosition 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/cssom-view-1/#dom-scrolllogicalposition-start">CSSOM View Module Level 1: 6 Extensions to the Element Interface</see>
    /// </remarks>
    [Description("@#start")]
    Start = 0,

    /// <summary>
    /// JavaScript 字符串取值 “center”；属于 ScrollLogicalPosition 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/cssom-view-1/#dom-scrolllogicalposition-center">CSSOM View Module Level 1: 6 Extensions to the Element Interface</see>
    /// </remarks>
    [Description("@#center")]
    Center = 1,

    /// <summary>
    /// JavaScript 字符串取值 “end”；属于 ScrollLogicalPosition 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/cssom-view-1/#dom-scrolllogicalposition-end">CSSOM View Module Level 1: 6 Extensions to the Element Interface</see>
    /// </remarks>
    [Description("@#end")]
    End = 2,

    /// <summary>
    /// JavaScript 字符串取值 “nearest”；属于 ScrollLogicalPosition 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/cssom-view-1/#dom-scrolllogicalposition-nearest">CSSOM View Module Level 1: 6 Extensions to the Element Interface</see>
    /// </remarks>
    [Description("@#nearest")]
    Nearest = 3
}

/// <summary>
/// WebIDL enum PointerAxis。定义于 Pointer-driven Animations Module Level 1。
/// </summary>
/// <remarks>
/// <see href="https://drafts.csswg.org/pointer-animations-1/#enumdef-pointeraxis">Pointer-driven Animations Module Level 1: 2.3.2 The PointerTimeline Interface</see>
/// </remarks>
[Description("@#PointerAxis")]
[ECMAScript]
[String]
public enum PointerAxis
{
    /// <summary>
    /// JavaScript 字符串取值 “block”；属于 PointerAxis 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/pointer-animations-1/#dom-pointeraxis-block">Pointer-driven Animations Module Level 1: 2.3.2 The PointerTimeline Interface</see>
    /// </remarks>
    [Description("@#block")]
    Block = 0,

    /// <summary>
    /// JavaScript 字符串取值 “inline”；属于 PointerAxis 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/pointer-animations-1/#dom-pointeraxis-inline">Pointer-driven Animations Module Level 1: 2.3.2 The PointerTimeline Interface</see>
    /// </remarks>
    [Description("@#inline")]
    Inline = 1,

    /// <summary>
    /// JavaScript 字符串取值 “x”；属于 PointerAxis 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/pointer-animations-1/#dom-pointeraxis-x">Pointer-driven Animations Module Level 1: 2.3.2 The PointerTimeline Interface</see>
    /// </remarks>
    [Description("@#x")]
    X = 2,

    /// <summary>
    /// JavaScript 字符串取值 “y”；属于 PointerAxis 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/pointer-animations-1/#dom-pointeraxis-y">Pointer-driven Animations Module Level 1: 2.3.2 The PointerTimeline Interface</see>
    /// </remarks>
    [Description("@#y")]
    Y = 3
}

/// <summary>
/// WebIDL enum ResizeObserverBoxOptions。定义于 Resize Observer Module Level 1。
/// </summary>
/// <remarks>
/// <see href="https://drafts.csswg.org/resize-observer-1/#enumdef-resizeobserverboxoptions">Resize Observer Module Level 1: 2.1 ResizeObserver interface</see>
/// </remarks>
[Description("@#ResizeObserverBoxOptions")]
[ECMAScript]
[String]
public enum ResizeObserverBoxOptions
{
    /// <summary>
    /// JavaScript 字符串取值 “border-box”；属于 ResizeObserverBoxOptions 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/resize-observer-1/#dom-resizeobserverboxoptions-border-box">Resize Observer Module Level 1: 2.1 ResizeObserver interface</see>
    /// </remarks>
    [Description("@#border-box")]
    BorderBox = 0,

    /// <summary>
    /// JavaScript 字符串取值 “content-box”；属于 ResizeObserverBoxOptions 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/resize-observer-1/#dom-resizeobserverboxoptions-content-box">Resize Observer Module Level 1: 2.1 ResizeObserver interface</see>
    /// </remarks>
    [Description("@#content-box")]
    ContentBox = 1,

    /// <summary>
    /// JavaScript 字符串取值 “device-pixel-content-box”；属于 ResizeObserverBoxOptions 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/resize-observer-1/#dom-resizeobserverboxoptions-device-pixel-content-box">Resize Observer Module Level 1: 2.1 ResizeObserver interface</see>
    /// </remarks>
    [Description("@#device-pixel-content-box")]
    DevicePixelContentBox = 2
}

/// <summary>
/// WebIDL enum ScrollAxis。定义于 Scroll-driven Animations Module Level 1。
/// </summary>
/// <remarks>
/// <see href="https://drafts.csswg.org/scroll-animations-1/#enumdef-scrollaxis">Scroll-driven Animations Module Level 1: 2.2.2 The ScrollTimeline Interface</see>
/// </remarks>
[Description("@#ScrollAxis")]
[ECMAScript]
[String]
public enum ScrollAxis
{
    /// <summary>
    /// JavaScript 字符串取值 “block”；属于 ScrollAxis 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/scroll-animations-1/#dom-scrollaxis-block">Scroll-driven Animations Module Level 1: 2.2.2 The ScrollTimeline Interface</see>
    /// </remarks>
    [Description("@#block")]
    Block = 0,

    /// <summary>
    /// JavaScript 字符串取值 “inline”；属于 ScrollAxis 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/scroll-animations-1/#dom-scrollaxis-inline">Scroll-driven Animations Module Level 1: 2.2.2 The ScrollTimeline Interface</see>
    /// </remarks>
    [Description("@#inline")]
    Inline = 1,

    /// <summary>
    /// JavaScript 字符串取值 “x”；属于 ScrollAxis 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/scroll-animations-1/#dom-scrollaxis-x">Scroll-driven Animations Module Level 1: 2.2.2 The ScrollTimeline Interface</see>
    /// </remarks>
    [Description("@#x")]
    X = 2,

    /// <summary>
    /// JavaScript 字符串取值 “y”；属于 ScrollAxis 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/scroll-animations-1/#dom-scrollaxis-y">Scroll-driven Animations Module Level 1: 2.2.2 The ScrollTimeline Interface</see>
    /// </remarks>
    [Description("@#y")]
    Y = 3
}

/// <summary>
/// WebIDL enum AnimationPlayState。定义于 Web Animations Module Level 1。
/// </summary>
/// <remarks>
/// <see href="https://drafts.csswg.org/web-animations-1/#enumdef-animationplaystate">Web Animations Module Level 1: 6.4.1 The AnimationPlayState enumeration</see>
/// </remarks>
[Description("@#AnimationPlayState")]
[ECMAScript]
[String]
public enum AnimationPlayState
{
    /// <summary>
    /// JavaScript 字符串取值 “idle”；属于 AnimationPlayState 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/web-animations-1/#dom-animationplaystate-idle">Web Animations Module Level 1: 6.4.1 The AnimationPlayState enumeration</see>
    /// </remarks>
    [Description("@#idle")]
    Idle = 0,

    /// <summary>
    /// JavaScript 字符串取值 “running”；属于 AnimationPlayState 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/web-animations-1/#dom-animationplaystate-running">Web Animations Module Level 1: 6.4.1 The AnimationPlayState enumeration</see>
    /// </remarks>
    [Description("@#running")]
    Running = 1,

    /// <summary>
    /// JavaScript 字符串取值 “paused”；属于 AnimationPlayState 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/web-animations-1/#dom-animationplaystate-paused">Web Animations Module Level 1: 6.4.1 The AnimationPlayState enumeration</see>
    /// </remarks>
    [Description("@#paused")]
    Paused = 2,

    /// <summary>
    /// JavaScript 字符串取值 “finished”；属于 AnimationPlayState 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/web-animations-1/#dom-animationplaystate-finished">Web Animations Module Level 1: 6.4.1 The AnimationPlayState enumeration</see>
    /// </remarks>
    [Description("@#finished")]
    Finished = 3
}

/// <summary>
/// WebIDL enum AnimationReplaceState。定义于 Web Animations Module Level 1。
/// </summary>
/// <remarks>
/// <see href="https://drafts.csswg.org/web-animations-1/#enumdef-animationreplacestate">Web Animations Module Level 1: 6.4.2 The AnimationReplaceState enumeration</see>
/// </remarks>
[Description("@#AnimationReplaceState")]
[ECMAScript]
[String]
public enum AnimationReplaceState
{
    /// <summary>
    /// JavaScript 字符串取值 “active”；属于 AnimationReplaceState 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/web-animations-1/#dom-animationreplacestate-active">Web Animations Module Level 1: 6.4.2 The AnimationReplaceState enumeration</see>
    /// </remarks>
    [Description("@#active")]
    Active = 0,

    /// <summary>
    /// JavaScript 字符串取值 “removed”；属于 AnimationReplaceState 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/web-animations-1/#dom-animationreplacestate-removed">Web Animations Module Level 1: 6.4.2 The AnimationReplaceState enumeration</see>
    /// </remarks>
    [Description("@#removed")]
    Removed = 1,

    /// <summary>
    /// JavaScript 字符串取值 “persisted”；属于 AnimationReplaceState 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/web-animations-1/#dom-animationreplacestate-persisted">Web Animations Module Level 1: 6.4.2 The AnimationReplaceState enumeration</see>
    /// </remarks>
    [Description("@#persisted")]
    Persisted = 2
}

/// <summary>
/// WebIDL enum CompositeOperation。定义于 Web Animations Module Level 1。
/// </summary>
/// <remarks>
/// <see href="https://drafts.csswg.org/web-animations-1/#enumdef-compositeoperation">Web Animations Module Level 1: 6.7 The CompositeOperation and CompositeOperationOrAuto enumerations</see>
/// </remarks>
[Description("@#CompositeOperation")]
[ECMAScript]
[String]
public enum CompositeOperation
{
    /// <summary>
    /// JavaScript 字符串取值 “replace”；属于 CompositeOperation 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/web-animations-1/#dom-compositeoperation-replace">Web Animations Module Level 1: 6.7 The CompositeOperation and CompositeOperationOrAuto enumerations</see>
    /// </remarks>
    [Description("@#replace")]
    Replace = 0,

    /// <summary>
    /// JavaScript 字符串取值 “add”；属于 CompositeOperation 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/web-animations-1/#dom-compositeoperation-add">Web Animations Module Level 1: 6.7 The CompositeOperation and CompositeOperationOrAuto enumerations</see>
    /// </remarks>
    [Description("@#add")]
    Add = 1,

    /// <summary>
    /// JavaScript 字符串取值 “accumulate”；属于 CompositeOperation 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/web-animations-1/#dom-compositeoperation-accumulate">Web Animations Module Level 1: 6.7 The CompositeOperation and CompositeOperationOrAuto enumerations</see>
    /// </remarks>
    [Description("@#accumulate")]
    Accumulate = 2
}

/// <summary>
/// WebIDL enum CompositeOperationOrAuto。定义于 Web Animations Module Level 1。
/// </summary>
/// <remarks>
/// <see href="https://drafts.csswg.org/web-animations-1/#enumdef-compositeoperationorauto">Web Animations Module Level 1: 6.7 The CompositeOperation and CompositeOperationOrAuto enumerations</see>
/// </remarks>
[Description("@#CompositeOperationOrAuto")]
[ECMAScript]
[String]
public enum CompositeOperationOrAuto
{
    /// <summary>
    /// JavaScript 字符串取值 “replace”；属于 CompositeOperationOrAuto 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/web-animations-1/#dom-compositeoperation-replace">Web Animations Module Level 1: 6.7 The CompositeOperation and CompositeOperationOrAuto enumerations</see>
    /// </remarks>
    [Description("@#replace")]
    Replace = 0,

    /// <summary>
    /// JavaScript 字符串取值 “add”；属于 CompositeOperationOrAuto 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/web-animations-1/#dom-compositeoperation-add">Web Animations Module Level 1: 6.7 The CompositeOperation and CompositeOperationOrAuto enumerations</see>
    /// </remarks>
    [Description("@#add")]
    Add = 1,

    /// <summary>
    /// JavaScript 字符串取值 “accumulate”；属于 CompositeOperationOrAuto 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/web-animations-1/#dom-compositeoperation-accumulate">Web Animations Module Level 1: 6.7 The CompositeOperation and CompositeOperationOrAuto enumerations</see>
    /// </remarks>
    [Description("@#accumulate")]
    Accumulate = 2,

    /// <summary>
    /// JavaScript 字符串取值 “auto”；属于 CompositeOperationOrAuto 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/web-animations-1/#dom-compositeoperationorauto-auto">Web Animations Module Level 1: 6.7 The CompositeOperation and CompositeOperationOrAuto enumerations</see>
    /// </remarks>
    [Description("@#auto")]
    Auto = 3
}

/// <summary>
/// WebIDL enum FillMode。定义于 Web Animations Module Level 1。
/// </summary>
/// <remarks>
/// <see href="https://drafts.csswg.org/web-animations-1/#enumdef-fillmode">Web Animations Module Level 1: 6.5.2 The FillMode enumeration</see>
/// </remarks>
[Description("@#FillMode")]
[ECMAScript]
[String]
public enum FillMode
{
    /// <summary>
    /// JavaScript 字符串取值 “none”；属于 FillMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/web-animations-1/#dom-fillmode-none">Web Animations Module Level 1: 6.5.2 The FillMode enumeration</see>
    /// </remarks>
    [Description("@#none")]
    None = 0,

    /// <summary>
    /// JavaScript 字符串取值 “forwards”；属于 FillMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/web-animations-1/#dom-fillmode-forwards">Web Animations Module Level 1: 6.5.2 The FillMode enumeration</see>
    /// </remarks>
    [Description("@#forwards")]
    Forwards = 1,

    /// <summary>
    /// JavaScript 字符串取值 “backwards”；属于 FillMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/web-animations-1/#dom-fillmode-backwards">Web Animations Module Level 1: 6.5.2 The FillMode enumeration</see>
    /// </remarks>
    [Description("@#backwards")]
    Backwards = 2,

    /// <summary>
    /// JavaScript 字符串取值 “both”；属于 FillMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/web-animations-1/#dom-fillmode-both">Web Animations Module Level 1: 6.5.2 The FillMode enumeration</see>
    /// </remarks>
    [Description("@#both")]
    Both = 3,

    /// <summary>
    /// JavaScript 字符串取值 “auto”；属于 FillMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/web-animations-1/#dom-fillmode-auto">Web Animations Module Level 1: 6.5.2 The FillMode enumeration</see>
    /// </remarks>
    [Description("@#auto")]
    Auto = 4
}

/// <summary>
/// WebIDL enum PlaybackDirection。定义于 Web Animations Module Level 1。
/// </summary>
/// <remarks>
/// <see href="https://drafts.csswg.org/web-animations-1/#enumdef-playbackdirection">Web Animations Module Level 1: 6.5.3 The PlaybackDirection enumeration</see>
/// </remarks>
[Description("@#PlaybackDirection")]
[ECMAScript]
[String]
public enum PlaybackDirection
{
    /// <summary>
    /// JavaScript 字符串取值 “normal”；属于 PlaybackDirection 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/web-animations-1/#dom-playbackdirection-normal">Web Animations Module Level 1: 6.5.3 The PlaybackDirection enumeration</see>
    /// </remarks>
    [Description("@#normal")]
    Normal = 0,

    /// <summary>
    /// JavaScript 字符串取值 “reverse”；属于 PlaybackDirection 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/web-animations-1/#dom-playbackdirection-reverse">Web Animations Module Level 1: 6.5.3 The PlaybackDirection enumeration</see>
    /// </remarks>
    [Description("@#reverse")]
    Reverse = 1,

    /// <summary>
    /// JavaScript 字符串取值 “alternate”；属于 PlaybackDirection 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/web-animations-1/#dom-playbackdirection-alternate">Web Animations Module Level 1: 6.5.3 The PlaybackDirection enumeration</see>
    /// </remarks>
    [Description("@#alternate")]
    Alternate = 2,

    /// <summary>
    /// JavaScript 字符串取值 “alternate-reverse”；属于 PlaybackDirection 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/web-animations-1/#dom-playbackdirection-alternate-reverse">Web Animations Module Level 1: 6.5.3 The PlaybackDirection enumeration</see>
    /// </remarks>
    [Description("@#alternate-reverse")]
    AlternateReverse = 3
}

/// <summary>
/// WebIDL enum IterationCompositeOperation。定义于 Web Animations Module Level 2。
/// </summary>
/// <remarks>
/// <see href="https://drafts.csswg.org/web-animations-2/#enumdef-iterationcompositeoperation">Web Animations Module Level 2: 4.11 The IterationCompositeOperation enumeration</see>
/// </remarks>
[Description("@#IterationCompositeOperation")]
[ECMAScript]
[String]
public enum IterationCompositeOperation
{
    /// <summary>
    /// JavaScript 字符串取值 “replace”；属于 IterationCompositeOperation 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/web-animations-2/#dom-iterationcompositeoperation-replace">Web Animations Module Level 2: 4.11 The IterationCompositeOperation enumeration</see>
    /// </remarks>
    [Description("@#replace")]
    Replace = 0,

    /// <summary>
    /// JavaScript 字符串取值 “accumulate”；属于 IterationCompositeOperation 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/web-animations-2/#dom-iterationcompositeoperation-accumulate">Web Animations Module Level 2: 4.11 The IterationCompositeOperation enumeration</see>
    /// </remarks>
    [Description("@#accumulate")]
    Accumulate = 1
}

/// <summary>
/// WebIDL enum RequestDuplex。定义于 Fetch Standard。
/// </summary>
/// <remarks>
/// <see href="https://fetch.spec.whatwg.org/#enumdef-requestduplex">Fetch Standard: 5.4 Request class</see>
/// </remarks>
[Description("@#RequestDuplex")]
[ECMAScript]
[String]
public enum RequestDuplex
{
    /// <summary>
    /// JavaScript 字符串取值 “half”；属于 RequestDuplex 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://fetch.spec.whatwg.org/#dom-requestduplex-half">Fetch Standard: 5.4 Request class</see>
    /// </remarks>
    [Description("@#half")]
    Half = 0
}

/// <summary>
/// WebIDL enum RequestPriority。定义于 Fetch Standard。
/// </summary>
/// <remarks>
/// <see href="https://fetch.spec.whatwg.org/#enumdef-requestpriority">Fetch Standard: 5.4 Request class</see>
/// </remarks>
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
/// WebIDL enum RequestCache。定义于 Fetch Standard。
/// </summary>
/// <remarks>
/// <see href="https://fetch.spec.whatwg.org/#requestcache">Fetch Standard: 5.4 Request class</see>
/// </remarks>
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
/// WebIDL enum RequestCredentials。定义于 Fetch Standard。
/// </summary>
/// <remarks>
/// <see href="https://fetch.spec.whatwg.org/#requestcredentials">Fetch Standard: 5.4 Request class</see>
/// </remarks>
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
/// WebIDL enum RequestRedirect。定义于 Fetch Standard。
/// </summary>
/// <remarks>
/// <see href="https://fetch.spec.whatwg.org/#requestredirect">Fetch Standard: 5.4 Request class</see>
/// </remarks>
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
/// WebIDL enum ResponseType。定义于 Fetch Standard。
/// </summary>
/// <remarks>
/// <see href="https://fetch.spec.whatwg.org/#responsetype">Fetch Standard: 5.5 Response class</see>
/// </remarks>
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
/// WebIDL enum FileSystemHandleKind。定义于 File System Standard。
/// </summary>
/// <remarks>
/// <see href="https://fs.spec.whatwg.org/#enumdef-filesystemhandlekind">File System Standard: 2.2 The FileSystemHandle interface</see>
/// </remarks>
[Description("@#FileSystemHandleKind")]
[ECMAScript]
[String]
public enum FileSystemHandleKind
{
    /// <summary>
    /// JavaScript 字符串取值 “file”；属于 FileSystemHandleKind 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://fs.spec.whatwg.org/#dom-filesystemhandlekind-file">File System Standard: 2.2 The FileSystemHandle interface</see>
    /// </remarks>
    [Description("@#file")]
    File = 0,

    /// <summary>
    /// JavaScript 字符串取值 “directory”；属于 FileSystemHandleKind 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://fs.spec.whatwg.org/#dom-filesystemhandlekind-directory">File System Standard: 2.2 The FileSystemHandle interface</see>
    /// </remarks>
    [Description("@#directory")]
    Directory = 1
}

/// <summary>
/// WebIDL enum WriteCommandType。定义于 File System Standard。
/// </summary>
/// <remarks>
/// <see href="https://fs.spec.whatwg.org/#enumdef-writecommandtype">File System Standard: 2.5 The FileSystemWritableFileStream interface</see>
/// </remarks>
[Description("@#WriteCommandType")]
[ECMAScript]
[String]
public enum WriteCommandType
{
    /// <summary>
    /// JavaScript 字符串取值 “write”；属于 WriteCommandType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://fs.spec.whatwg.org/#dom-writecommandtype-write">File System Standard: 2.5 The FileSystemWritableFileStream interface</see>
    /// </remarks>
    /// <example>
    /// <code>await stream.write({ type: &quot;write&quot;, data: data })</code>
    /// </example>
    [Description("@#write")]
    Write = 0,

    /// <summary>
    /// JavaScript 字符串取值 “seek”；属于 WriteCommandType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://fs.spec.whatwg.org/#dom-writecommandtype-seek">File System Standard: 2.5 The FileSystemWritableFileStream interface</see>
    /// </remarks>
    /// <example>
    /// <code>await stream.write({ type: &quot;seek&quot;, position: position })</code>
    /// </example>
    [Description("@#seek")]
    Seek = 1,

    /// <summary>
    /// JavaScript 字符串取值 “truncate”；属于 WriteCommandType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://fs.spec.whatwg.org/#dom-writecommandtype-truncate">File System Standard: 2.5 The FileSystemWritableFileStream interface</see>
    /// </remarks>
    /// <example>
    /// <code>await stream.write({ type: &quot;truncate&quot;, size: size })</code>
    /// </example>
    [Description("@#truncate")]
    Truncate = 2
}

/// <summary>
/// WebIDL enum FullscreenKeyboardLock。定义于 Fullscreen API Standard。
/// </summary>
/// <remarks>
/// <see href="https://fullscreen.spec.whatwg.org/#enumdef-fullscreenkeyboardlock">Fullscreen API Standard: 3 API</see>
/// </remarks>
[Description("@#FullscreenKeyboardLock")]
[ECMAScript]
[String]
public enum FullscreenKeyboardLock
{
    /// <summary>
    /// JavaScript 字符串取值 “browser”；属于 FullscreenKeyboardLock 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://fullscreen.spec.whatwg.org/#dom-fullscreenkeyboardlock-browser">Fullscreen API Standard: 3 API</see>
    /// </remarks>
    [Description("@#browser")]
    Browser = 0,

    /// <summary>
    /// JavaScript 字符串取值 “none”；属于 FullscreenKeyboardLock 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://fullscreen.spec.whatwg.org/#dom-fullscreenkeyboardlock-none">Fullscreen API Standard: 3 API</see>
    /// </remarks>
    [Description("@#none")]
    None = 1
}

/// <summary>
/// WebIDL enum FullscreenNavigationUI。定义于 Fullscreen API Standard。
/// </summary>
/// <remarks>
/// <see href="https://fullscreen.spec.whatwg.org/#enumdef-fullscreennavigationui">Fullscreen API Standard: 3 API</see>
/// </remarks>
[Description("@#FullscreenNavigationUI")]
[ECMAScript]
[String]
public enum FullscreenNavigationUI
{
    /// <summary>
    /// JavaScript 字符串取值 “auto”；属于 FullscreenNavigationUI 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://fullscreen.spec.whatwg.org/#dom-fullscreennavigationui-auto">Fullscreen API Standard: 3 API</see>
    /// </remarks>
    [Description("@#auto")]
    Auto = 0,

    /// <summary>
    /// JavaScript 字符串取值 “show”；属于 FullscreenNavigationUI 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://fullscreen.spec.whatwg.org/#dom-fullscreennavigationui-show">Fullscreen API Standard: 3 API</see>
    /// </remarks>
    [Description("@#show")]
    Show = 1,

    /// <summary>
    /// JavaScript 字符串取值 “hide”；属于 FullscreenNavigationUI 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://fullscreen.spec.whatwg.org/#dom-fullscreennavigationui-hide">Fullscreen API Standard: 3 API</see>
    /// </remarks>
    [Description("@#hide")]
    Hide = 2
}

/// <summary>
/// WebIDL enum CanvasDirection。定义于 HTML Standard。
/// </summary>
/// <remarks>
/// <see href="https://html.spec.whatwg.org/multipage/canvas.html#canvasdirection">HTML Standard: 4.12.5.1 The 2D rendering context</see>
/// </remarks>
[Description("@#CanvasDirection")]
[ECMAScript]
[String]
public enum CanvasDirection
{
    /// <summary>
    /// JavaScript 字符串取值 “ltr”；属于 CanvasDirection 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-direction-ltr">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </remarks>
    [Description("@#ltr")]
    Ltr = 0,

    /// <summary>
    /// JavaScript 字符串取值 “rtl”；属于 CanvasDirection 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-direction-rtl">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </remarks>
    [Description("@#rtl")]
    Rtl = 1,

    /// <summary>
    /// JavaScript 字符串取值 “inherit”；属于 CanvasDirection 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-direction-inherit">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </remarks>
    [Description("@#inherit")]
    Inherit = 2
}

/// <summary>
/// WebIDL enum CanvasFontKerning。定义于 HTML Standard。
/// </summary>
/// <remarks>
/// <see href="https://html.spec.whatwg.org/multipage/canvas.html#canvasfontkerning">HTML Standard: 4.12.5.1 The 2D rendering context</see>
/// </remarks>
[Description("@#CanvasFontKerning")]
[ECMAScript]
[String]
public enum CanvasFontKerning
{
    /// <summary>
    /// JavaScript 字符串取值 “auto”；属于 CanvasFontKerning 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-fontkerning-auto">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </remarks>
    [Description("@#auto")]
    Auto = 0,

    /// <summary>
    /// JavaScript 字符串取值 “normal”；属于 CanvasFontKerning 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-fontkerning-normal">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </remarks>
    [Description("@#normal")]
    Normal = 1,

    /// <summary>
    /// JavaScript 字符串取值 “none”；属于 CanvasFontKerning 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-fontkerning-none">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </remarks>
    [Description("@#none")]
    None = 2
}

/// <summary>
/// WebIDL enum CanvasFontStretch。定义于 HTML Standard。
/// </summary>
/// <remarks>
/// <see href="https://html.spec.whatwg.org/multipage/canvas.html#canvasfontstretch">HTML Standard: 4.12.5.1 The 2D rendering context</see>
/// </remarks>
[Description("@#CanvasFontStretch")]
[ECMAScript]
[String]
public enum CanvasFontStretch
{
    /// <summary>
    /// ultra-condensed
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-fontstretch-ultra-condensed">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </remarks>
    [Description("@#ultra-condensed")]
    UltraCondensed = 0,

    /// <summary>
    /// extra-condensed
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-fontstretch-extra-condensed">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </remarks>
    [Description("@#extra-condensed")]
    ExtraCondensed = 1,

    /// <summary>
    /// ultra-condensed
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-fontstretch-condensed">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </remarks>
    [Description("@#condensed")]
    Condensed = 2,

    /// <summary>
    /// semi-condensed
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-fontstretch-semi-condensed">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </remarks>
    [Description("@#semi-condensed")]
    SemiCondensed = 3,

    /// <summary>
    /// JavaScript 字符串取值 “normal”；属于 CanvasFontStretch 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-fontstretch-normal">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </remarks>
    [Description("@#normal")]
    Normal = 4,

    /// <summary>
    /// semi-expanded
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-fontstretch-semi-expanded">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </remarks>
    [Description("@#semi-expanded")]
    SemiExpanded = 5,

    /// <summary>
    /// semi-expanded
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-fontstretch-expanded">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </remarks>
    [Description("@#expanded")]
    Expanded = 6,

    /// <summary>
    /// extra-expanded
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-fontstretch-extra-expanded">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </remarks>
    [Description("@#extra-expanded")]
    ExtraExpanded = 7,

    /// <summary>
    /// ultra-expanded
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-fontstretch-ultra-expanded">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </remarks>
    [Description("@#ultra-expanded")]
    UltraExpanded = 8
}

/// <summary>
/// WebIDL enum CanvasFontVariantCaps。定义于 HTML Standard。
/// </summary>
/// <remarks>
/// <see href="https://html.spec.whatwg.org/multipage/canvas.html#canvasfontvariantcaps">HTML Standard: 4.12.5.1 The 2D rendering context</see>
/// </remarks>
[Description("@#CanvasFontVariantCaps")]
[ECMAScript]
[String]
public enum CanvasFontVariantCaps
{
    /// <summary>
    /// JavaScript 字符串取值 “normal”；属于 CanvasFontVariantCaps 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-fontvariantcaps-normal">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </remarks>
    [Description("@#normal")]
    Normal = 0,

    /// <summary>
    /// all-small-caps
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-fontvariantcaps-small-caps">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </remarks>
    [Description("@#small-caps")]
    SmallCaps = 1,

    /// <summary>
    /// all-small-caps
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-fontvariantcaps-all-small-caps">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </remarks>
    [Description("@#all-small-caps")]
    AllSmallCaps = 2,

    /// <summary>
    /// all-petite-caps
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-fontvariantcaps-petite-caps">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </remarks>
    [Description("@#petite-caps")]
    PetiteCaps = 3,

    /// <summary>
    /// all-petite-caps
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-fontvariantcaps-all-petite-caps">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </remarks>
    [Description("@#all-petite-caps")]
    AllPetiteCaps = 4,

    /// <summary>
    /// JavaScript 字符串取值 “unicase”；属于 CanvasFontVariantCaps 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-fontvariantcaps-unicase">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </remarks>
    [Description("@#unicase")]
    Unicase = 5,

    /// <summary>
    /// titling-caps
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-fontvariantcaps-titling-caps">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </remarks>
    [Description("@#titling-caps")]
    TitlingCaps = 6
}

/// <summary>
/// WebIDL enum CanvasLineCap。定义于 HTML Standard。
/// </summary>
/// <remarks>
/// <see href="https://html.spec.whatwg.org/multipage/canvas.html#canvaslinecap">HTML Standard: 4.12.5.1 The 2D rendering context</see>
/// </remarks>
[Description("@#CanvasLineCap")]
[ECMAScript]
[String]
public enum CanvasLineCap
{
    /// <summary>
    /// JavaScript 字符串取值 “butt”；属于 CanvasLineCap 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/">HTML Standard: CanvasLineCap.butt</see>
    /// </remarks>
[Description("@#butt")]
    Butt = 0,

    /// <summary>
    /// JavaScript 字符串取值 “round”；属于 CanvasLineCap 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/">HTML Standard: CanvasLineCap.round</see>
    /// </remarks>
[Description("@#round")]
    Round = 1,

    /// <summary>
    /// JavaScript 字符串取值 “square”；属于 CanvasLineCap 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/">HTML Standard: CanvasLineCap.square</see>
    /// </remarks>
[Description("@#square")]
    Square = 2
}

/// <summary>
/// WebIDL enum CanvasLineJoin。定义于 HTML Standard。
/// </summary>
/// <remarks>
/// <see href="https://html.spec.whatwg.org/multipage/canvas.html#canvaslinejoin">HTML Standard: 4.12.5.1 The 2D rendering context</see>
/// </remarks>
[Description("@#CanvasLineJoin")]
[ECMAScript]
[String]
public enum CanvasLineJoin
{
    /// <summary>
    /// JavaScript 字符串取值 “round”；属于 CanvasLineJoin 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/">HTML Standard: CanvasLineJoin.round</see>
    /// </remarks>
[Description("@#round")]
    Round = 0,

    /// <summary>
    /// JavaScript 字符串取值 “bevel”；属于 CanvasLineJoin 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/">HTML Standard: CanvasLineJoin.bevel</see>
    /// </remarks>
[Description("@#bevel")]
    Bevel = 1,

    /// <summary>
    /// JavaScript 字符串取值 “miter”；属于 CanvasLineJoin 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/">HTML Standard: CanvasLineJoin.miter</see>
    /// </remarks>
[Description("@#miter")]
    Miter = 2
}

/// <summary>
/// WebIDL enum CanvasTextAlign。定义于 HTML Standard。
/// </summary>
/// <remarks>
/// <see href="https://html.spec.whatwg.org/multipage/canvas.html#canvastextalign">HTML Standard: 4.12.5.1 The 2D rendering context</see>
/// </remarks>
[Description("@#CanvasTextAlign")]
[ECMAScript]
[String]
public enum CanvasTextAlign
{
    /// <summary>
    /// JavaScript 字符串取值 “start”；属于 CanvasTextAlign 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-textalign-start">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </remarks>
    [Description("@#start")]
    Start = 0,

    /// <summary>
    /// JavaScript 字符串取值 “end”；属于 CanvasTextAlign 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-textalign-end">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </remarks>
    [Description("@#end")]
    End = 1,

    /// <summary>
    /// JavaScript 字符串取值 “left”；属于 CanvasTextAlign 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-textalign-left">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </remarks>
    [Description("@#left")]
    Left = 2,

    /// <summary>
    /// JavaScript 字符串取值 “right”；属于 CanvasTextAlign 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-textalign-right">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </remarks>
    [Description("@#right")]
    Right = 3,

    /// <summary>
    /// JavaScript 字符串取值 “center”；属于 CanvasTextAlign 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-textalign-center">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </remarks>
    [Description("@#center")]
    Center = 4
}

/// <summary>
/// WebIDL enum CanvasTextBaseline。定义于 HTML Standard。
/// </summary>
/// <remarks>
/// <see href="https://html.spec.whatwg.org/multipage/canvas.html#canvastextbaseline">HTML Standard: 4.12.5.1 The 2D rendering context</see>
/// </remarks>
[Description("@#CanvasTextBaseline")]
[ECMAScript]
[String]
public enum CanvasTextBaseline
{
    /// <summary>
    /// JavaScript 字符串取值 “top”；属于 CanvasTextBaseline 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-textbaseline-top">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </remarks>
    [Description("@#top")]
    Top = 0,

    /// <summary>
    /// JavaScript 字符串取值 “hanging”；属于 CanvasTextBaseline 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-textbaseline-hanging">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </remarks>
    [Description("@#hanging")]
    Hanging = 1,

    /// <summary>
    /// JavaScript 字符串取值 “middle”；属于 CanvasTextBaseline 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-textbaseline-middle">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </remarks>
    [Description("@#middle")]
    Middle = 2,

    /// <summary>
    /// JavaScript 字符串取值 “alphabetic”；属于 CanvasTextBaseline 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-textbaseline-alphabetic">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </remarks>
    [Description("@#alphabetic")]
    Alphabetic = 3,

    /// <summary>
    /// JavaScript 字符串取值 “ideographic”；属于 CanvasTextBaseline 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-textbaseline-ideographic">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </remarks>
    [Description("@#ideographic")]
    Ideographic = 4,

    /// <summary>
    /// JavaScript 字符串取值 “bottom”；属于 CanvasTextBaseline 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-textbaseline-bottom">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </remarks>
    [Description("@#bottom")]
    Bottom = 5
}

/// <summary>
/// WebIDL enum CanvasTextRendering。定义于 HTML Standard。
/// </summary>
/// <remarks>
/// <see href="https://html.spec.whatwg.org/multipage/canvas.html#canvastextrendering">HTML Standard: 4.12.5.1 The 2D rendering context</see>
/// </remarks>
[Description("@#CanvasTextRendering")]
[ECMAScript]
[String]
public enum CanvasTextRendering
{
    /// <summary>
    /// JavaScript 字符串取值 “auto”；属于 CanvasTextRendering 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-textrendering-auto">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </remarks>
    [Description("@#auto")]
    Auto = 0,

    /// <summary>
    /// optimizeSpeed
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-textrendering-optimizespeed">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </remarks>
    [Description("@#optimizeSpeed")]
    OptimizeSpeed = 1,

    /// <summary>
    /// optimizeLegibility
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-textrendering-optimizelegibility">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </remarks>
    [Description("@#optimizeLegibility")]
    OptimizeLegibility = 2,

    /// <summary>
    /// geometricPrecision
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-textrendering-geometricprecision">HTML Standard: 4.12.5.1.5 Text styles</see>
    /// </remarks>
    [Description("@#geometricPrecision")]
    GeometricPrecision = 3
}

/// <summary>
/// WebIDL enum OffscreenRenderingContextId。定义于 HTML Standard。
/// </summary>
/// <remarks>
/// <see href="https://html.spec.whatwg.org/multipage/canvas.html#offscreenrenderingcontextid">HTML Standard: 4.12.5.3 The OffscreenCanvas interface</see>
/// </remarks>
[Description("@#OffscreenRenderingContextId")]
[ECMAScript]
[String]
public enum OffscreenRenderingContextId
{
    /// <summary>
    /// JavaScript 字符串取值 “2d”；属于 OffscreenRenderingContextId 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/">HTML Standard: OffscreenRenderingContextId.2d</see>
    /// </remarks>
[Description("@#2d")]
    _2d = 0,

    /// <summary>
    /// JavaScript 字符串取值 “bitmaprenderer”；属于 OffscreenRenderingContextId 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/">HTML Standard: OffscreenRenderingContextId.bitmaprenderer</see>
    /// </remarks>
[Description("@#bitmaprenderer")]
    Bitmaprenderer = 1,

    /// <summary>
    /// JavaScript 字符串取值 “webgl”；属于 OffscreenRenderingContextId 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/">HTML Standard: OffscreenRenderingContextId.webgl</see>
    /// </remarks>
[Description("@#webgl")]
    Webgl = 2,

    /// <summary>
    /// JavaScript 字符串取值 “webgl2”；属于 OffscreenRenderingContextId 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/">HTML Standard: OffscreenRenderingContextId.webgl2</see>
    /// </remarks>
[Description("@#webgl2")]
    Webgl2 = 3,

    /// <summary>
    /// JavaScript 字符串取值 “webgpu”；属于 OffscreenRenderingContextId 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/">HTML Standard: OffscreenRenderingContextId.webgpu</see>
    /// </remarks>
[Description("@#webgpu")]
    Webgpu = 4
}

/// <summary>
/// WebIDL enum DocumentReadyState。定义于 HTML Standard。
/// </summary>
/// <remarks>
/// <see href="https://html.spec.whatwg.org/multipage/dom.html#documentreadystate">HTML Standard: 3.1.1 The Document object</see>
/// </remarks>
[Description("@#DocumentReadyState")]
[ECMAScript]
[String]
public enum DocumentReadyState
{
    /// <summary>
    /// JavaScript 字符串取值 “loading”；属于 DocumentReadyState 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/">HTML Standard: DocumentReadyState.loading</see>
    /// </remarks>
[Description("@#loading")]
    Loading = 0,

    /// <summary>
    /// JavaScript 字符串取值 “interactive”；属于 DocumentReadyState 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/">HTML Standard: DocumentReadyState.interactive</see>
    /// </remarks>
[Description("@#interactive")]
    Interactive = 1,

    /// <summary>
    /// JavaScript 字符串取值 “complete”；属于 DocumentReadyState 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/">HTML Standard: DocumentReadyState.complete</see>
    /// </remarks>
[Description("@#complete")]
    Complete = 2
}

/// <summary>
/// WebIDL enum DocumentVisibilityState。定义于 HTML Standard。
/// </summary>
/// <remarks>
/// <see href="https://html.spec.whatwg.org/multipage/dom.html#documentvisibilitystate">HTML Standard: 3.1.1 The Document object</see>
/// </remarks>
[Description("@#DocumentVisibilityState")]
[ECMAScript]
[String]
public enum DocumentVisibilityState
{
    /// <summary>
    /// JavaScript 字符串取值 “visible”；属于 DocumentVisibilityState 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/">HTML Standard: DocumentVisibilityState.visible</see>
    /// </remarks>
[Description("@#visible")]
    Visible = 0,

    /// <summary>
    /// JavaScript 字符串取值 “hidden”；属于 DocumentVisibilityState 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/">HTML Standard: DocumentVisibilityState.hidden</see>
    /// </remarks>
[Description("@#hidden")]
    Hidden = 1
}

/// <summary>
/// WebIDL enum DOMParserSupportedType。定义于 HTML Standard。
/// </summary>
/// <remarks>
/// <see href="https://html.spec.whatwg.org/multipage/dynamic-markup-insertion.html#domparsersupportedtype">HTML Standard: 8.5.1 The DOMParser interface</see>
/// </remarks>
[Description("@#DOMParserSupportedType")]
[ECMAScript]
[String]
public enum DOMParserSupportedType
{
    /// <summary>
    /// JavaScript 字符串取值 “text/html”；属于 DOMParserSupportedType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/dynamic-markup-insertion.html#dom-domparsersupportedtype-texthtml">HTML Standard: 8.5.1 The DOMParser interface</see>
    /// </remarks>
    [Description("@#text/html")]
    TextHtml = 0,

    /// <summary>
    /// JavaScript 字符串取值 “text/xml”；属于 DOMParserSupportedType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/">HTML Standard: DOMParserSupportedType.text/xml</see>
    /// </remarks>
[Description("@#text/xml")]
    TextXml = 1,

    /// <summary>
    /// JavaScript 字符串取值 “application/xml”；属于 DOMParserSupportedType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/">HTML Standard: DOMParserSupportedType.application/xml</see>
    /// </remarks>
[Description("@#application/xml")]
    ApplicationXml = 2,

    /// <summary>
    /// JavaScript 字符串取值 “application/xhtml+xml”；属于 DOMParserSupportedType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/">HTML Standard: DOMParserSupportedType.application/xhtml+xml</see>
    /// </remarks>
[Description("@#application/xhtml\u002Bxml")]
    ApplicationXhtmlXml = 3,

    /// <summary>
    /// JavaScript 字符串取值 “image/svg+xml”；属于 DOMParserSupportedType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/">HTML Standard: DOMParserSupportedType.image/svg+xml</see>
    /// </remarks>
[Description("@#image/svg\u002Bxml")]
    ImageSvgXml = 4
}

/// <summary>
/// WebIDL enum SanitizerPresets。定义于 HTML Standard。
/// </summary>
/// <remarks>
/// <see href="https://html.spec.whatwg.org/multipage/dynamic-markup-insertion.html#sanitizerpresets">HTML Standard: 8.5 DOM parsing and serialization APIs</see>
/// </remarks>
[Description("@#SanitizerPresets")]
[ECMAScript]
[String]
public enum SanitizerPresets
{
    /// <summary>
    /// Assert: configuration is &quot;default&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/dynamic-markup-insertion.html#dom-sanitizerpresets-default">HTML Standard: 8.6.3.1 Configuration invariants</see>
    /// </remarks>
    [Description("@#default")]
    Default = 0
}

/// <summary>
/// WebIDL enum SelectionMode。定义于 HTML Standard。
/// </summary>
/// <remarks>
/// <see href="https://html.spec.whatwg.org/multipage/form-control-infrastructure.html#selectionmode">HTML Standard: 4.10.20 APIs for the text control selections</see>
/// </remarks>
[Description("@#SelectionMode")]
[ECMAScript]
[String]
public enum SelectionMode
{
    /// <summary>
    /// If the fourth argument&apos;s value is &quot;select&quot;
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/form-control-infrastructure.html#dom-selectionmode-select">HTML Standard: 4.10.20 APIs for the text control selections</see>
    /// </remarks>
    [Description("@#select")]
    Select = 0,

    /// <summary>
    /// If the fourth argument&apos;s value is &quot;start&quot;
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/form-control-infrastructure.html#dom-selectionmode-start">HTML Standard: 4.10.20 APIs for the text control selections</see>
    /// </remarks>
    [Description("@#start")]
    Start = 1,

    /// <summary>
    /// If the fourth argument&apos;s value is &quot;end&quot;
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/form-control-infrastructure.html#dom-selectionmode-end">HTML Standard: 4.10.20 APIs for the text control selections</see>
    /// </remarks>
    [Description("@#end")]
    End = 2,

    /// <summary>
    /// If the fourth argument&apos;s value is &quot;preserve&quot;
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/form-control-infrastructure.html#dom-selectionmode-preserve">HTML Standard: 4.10.20 APIs for the text control selections</see>
    /// </remarks>
    [Description("@#preserve")]
    Preserve = 3
}

/// <summary>
/// WebIDL enum ColorSpaceConversion。定义于 HTML Standard。
/// </summary>
/// <remarks>
/// <see href="https://html.spec.whatwg.org/multipage/imagebitmap-and-animations.html#colorspaceconversion">HTML Standard: 8.11.2 The ImageBitmap interface</see>
/// </remarks>
[Description("@#ColorSpaceConversion")]
[ECMAScript]
[String]
public enum ColorSpaceConversion
{
    /// <summary>
    /// If val is &quot;none&quot;, output must be decoded without performing any color space conversions. This means that the image decoding algorithm must ignore color profile metadata embedded in the source data as well as the display device color profile.
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/imagebitmap-and-animations.html#dom-colorspaceconversion-none">HTML Standard: 8.11.2 The ImageBitmap interface</see>
    /// </remarks>
    [Description("@#none")]
    None = 0,

    /// <summary>
    /// If val is &quot;default&quot;, the color space conversion behavior is implementation-specific, and should be chosen according to the default color space that the implementation uses for drawing images onto the canvas.
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/imagebitmap-and-animations.html#dom-colorspaceconversion-default">HTML Standard: 8.11.2 The ImageBitmap interface</see>
    /// </remarks>
    [Description("@#default")]
    Default = 1
}

/// <summary>
/// WebIDL enum ImageOrientation。定义于 HTML Standard。
/// </summary>
/// <remarks>
/// <see href="https://html.spec.whatwg.org/multipage/imagebitmap-and-animations.html#imageorientation">HTML Standard: 8.11.2 The ImageBitmap interface</see>
/// </remarks>
[Description("@#ImageOrientation")]
[ECMAScript]
[String]
public enum ImageOrientation
{
    /// <summary>
    /// There used to be a &quot;none&quot; enum value. It was renamed to &quot;from-image&quot;. In the future, &quot;none&quot; will be added back with a different meaning.
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/imagebitmap-and-animations.html#dom-imageorientation-from-image">HTML Standard: 8.11.2 The ImageBitmap interface</see>
    /// </remarks>
    [Description("@#from-image")]
    FromImage = 0,

    /// <summary>
    /// If the value of the imageOrientation member of options is &quot;flipY&quot;, output must be flipped vertically, disregarding any image orientation metadata of the source (such as EXIF metadata), if any. EXIF
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/imagebitmap-and-animations.html#dom-imageorientation-flipy">HTML Standard: 8.11.2 The ImageBitmap interface</see>
    /// </remarks>
    [Description("@#flipY")]
    FlipY = 1
}

/// <summary>
/// WebIDL enum CanPlayTypeResult。定义于 HTML Standard。
/// </summary>
/// <remarks>
/// <see href="https://html.spec.whatwg.org/multipage/media.html#canplaytyperesult">HTML Standard: 4.8.11 Media elements</see>
/// </remarks>
[Description("@#CanPlayTypeResult")]
[ECMAScript]
[String]
public enum CanPlayTypeResult
{
    /// <summary>
    /// JavaScript 字符串取值 “”；属于 CanPlayTypeResult 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/">HTML Standard: CanPlayTypeResult.</see>
    /// </remarks>
[Description("@#")]
    Empty = 0,

    /// <summary>
    /// The canPlayType(type) method must return the empty string if type is a type that the user agent knows it cannot render or is the type &quot;application/octet-stream&quot;; it must return &quot;probably&quot; if the user agent is confident that the type represents a media resource that it can render if used with this audio or video element; and it must return &quot;maybe&quot; otherwise. Implementers are encouraged to return &quot;maybe&quot; unless the type can be confidently established as being supported or not. Generally, a user agent should never return &quot;probably&quot; for a type that allows the codecs parameter if that parameter is not present.
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/media.html#dom-canplaytyperesult-maybe">HTML Standard: 4.8.11.3 MIME types</see>
    /// </remarks>
    [Description("@#maybe")]
    Maybe = 1,

    /// <summary>
    /// The canPlayType(type) method must return the empty string if type is a type that the user agent knows it cannot render or is the type &quot;application/octet-stream&quot;; it must return &quot;probably&quot; if the user agent is confident that the type represents a media resource that it can render if used with this audio or video element; and it must return &quot;maybe&quot; otherwise. Implementers are encouraged to return &quot;maybe&quot; unless the type can be confidently established as being supported or not. Generally, a user agent should never return &quot;probably&quot; for a type that allows the codecs parameter if that parameter is not present.
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/media.html#dom-canplaytyperesult-probably">HTML Standard: 4.8.11.3 MIME types</see>
    /// </remarks>
    [Description("@#probably")]
    Probably = 2
}

/// <summary>
/// WebIDL enum TextTrackKind。定义于 HTML Standard。
/// </summary>
/// <remarks>
/// <see href="https://html.spec.whatwg.org/multipage/media.html#texttrackkind">HTML Standard: 4.8.11.11.5 Text track API</see>
/// </remarks>
[Description("@#TextTrackKind")]
[ECMAScript]
[String]
public enum TextTrackKind
{
    /// <summary>
    /// JavaScript 字符串取值 “subtitles”；属于 TextTrackKind 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/">HTML Standard: TextTrackKind.subtitles</see>
    /// </remarks>
[Description("@#subtitles")]
    Subtitles = 0,

    /// <summary>
    /// JavaScript 字符串取值 “captions”；属于 TextTrackKind 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/">HTML Standard: TextTrackKind.captions</see>
    /// </remarks>
[Description("@#captions")]
    Captions = 1,

    /// <summary>
    /// JavaScript 字符串取值 “descriptions”；属于 TextTrackKind 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/">HTML Standard: TextTrackKind.descriptions</see>
    /// </remarks>
[Description("@#descriptions")]
    Descriptions = 2,

    /// <summary>
    /// JavaScript 字符串取值 “chapters”；属于 TextTrackKind 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/">HTML Standard: TextTrackKind.chapters</see>
    /// </remarks>
[Description("@#chapters")]
    Chapters = 3,

    /// <summary>
    /// JavaScript 字符串取值 “metadata”；属于 TextTrackKind 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/">HTML Standard: TextTrackKind.metadata</see>
    /// </remarks>
[Description("@#metadata")]
    Metadata = 4
}

/// <summary>
/// WebIDL enum TextTrackMode。定义于 HTML Standard。
/// </summary>
/// <remarks>
/// <see href="https://html.spec.whatwg.org/multipage/media.html#texttrackmode">HTML Standard: 4.8.11.11.5 Text track API</see>
/// </remarks>
[Description("@#TextTrackMode")]
[ECMAScript]
[String]
public enum TextTrackMode
{
    /// <summary>
    /// When the user agent is required to populate the list of pending text tracks of a media element, the user agent must add to the element&apos;s list of pending text tracks each text track in the element&apos;s list of text tracks whose text track mode is not disabled and whose text track readiness state is loading.
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/media.html#dom-texttrack-disabled">HTML Standard: 4.8.11.11.5 Text track API</see>
    /// </remarks>
    [Description("@#disabled")]
    Disabled = 0,

    /// <summary>
    /// If there are any text tracks in the media element&apos;s list of text tracks whose text track kind is chapters or metadata that correspond to track elements with a default attribute set whose text track mode is set to disabled, then set the text track mode of all such tracks to hidden.
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/media.html#dom-texttrack-hidden">HTML Standard: 4.8.11.11.5 Text track API</see>
    /// </remarks>
    [Description("@#hidden")]
    Hidden = 1,

    /// <summary>
    /// The user agent must synchronously unset this flag whenever the text track cue is removed from its text track&apos;s text track list of cues; whenever the text track itself is removed from its media element&apos;s list of text tracks or has its text track mode changed to disabled; and whenever the media element&apos;s readyState is changed back to HAVE_NOTHING. When the flag is unset in this way for one or more cues in text tracks that were showing prior to the relevant incident, the user agent must, after having unset the flag for all the affected cues, apply the rules for updating the text track rendering of those text tracks....
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/media.html#dom-texttrack-showing">HTML Standard: 4.8.11.11.5 Text track API</see>
    /// </remarks>
    [Description("@#showing")]
    Showing = 2
}

/// <summary>
/// WebIDL enum NavigationHistoryBehavior。定义于 HTML Standard。
/// </summary>
/// <remarks>
/// <see href="https://html.spec.whatwg.org/multipage/nav-history-apis.html#navigationhistorybehavior">HTML Standard: 7.2.6.2 The Navigation interface</see>
/// </remarks>
[Description("@#NavigationHistoryBehavior")]
[ECMAScript]
[String]
public enum NavigationHistoryBehavior
{
    /// <summary>
    /// JavaScript 字符串取值 “auto”；属于 NavigationHistoryBehavior 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/browsing-the-web.html#navigationhistorybehavior-auto">HTML Standard: 7.4.2.1 Supporting concepts</see>
    /// </remarks>
    [Description("@#auto")]
    Auto = 0,

    /// <summary>
    /// JavaScript 字符串取值 “push”；属于 NavigationHistoryBehavior 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/browsing-the-web.html#navigationhistorybehavior-push">HTML Standard: 7.4.2.1 Supporting concepts</see>
    /// </remarks>
    [Description("@#push")]
    Push = 1,

    /// <summary>
    /// After the refresh has come due (as defined below), if the user has not canceled the redirect and, if meta is given, document&apos;s active sandboxing flag set does not have the sandboxed automatic features browsing context flag set, then navigate document&apos;s node navigable to urlRecord using document, with historyHandling set to &quot;replace&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/browsing-the-web.html#navigationhistorybehavior-replace">HTML Standard: 7.4.2.1 Supporting concepts</see>
    /// </remarks>
    [Description("@#replace")]
    Replace = 2
}

/// <summary>
/// WebIDL enum NavigationType。定义于 HTML Standard。
/// </summary>
/// <remarks>
/// <see href="https://html.spec.whatwg.org/multipage/nav-history-apis.html#navigationtype">HTML Standard: 7.2.6.3 Core infrastructure</see>
/// </remarks>
[Description("@#NavigationType")]
[ECMAScript]
[String]
public enum NavigationType
{
    /// <summary>
    /// Let continue be the result of firing a push/replace/reload navigate event at navigation with navigationType set to historyHandling, isSameDocument set to true, destinationURL set to newURL, and classicHistoryAPIState set to serializedData.
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/nav-history-apis.html#dom-navigationtype-push">HTML Standard: 7.2.6.3 Core infrastructure</see>
    /// </remarks>
    [Description("@#push")]
    Push = 0,

    /// <summary>
    /// Let continue be the result of firing a push/replace/reload navigate event at navigation with navigationType set to historyHandling, isSameDocument set to true, destinationURL set to newURL, and classicHistoryAPIState set to serializedData.
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/nav-history-apis.html#dom-navigationtype-replace">HTML Standard: 7.2.6.3 Core infrastructure</see>
    /// </remarks>
    [Description("@#replace")]
    Replace = 1,

    /// <summary>
    /// Let continue be the result of firing a push/replace/reload navigate event at navigation with navigationType set to historyHandling, isSameDocument set to true, destinationURL set to newURL, and classicHistoryAPIState set to serializedData.
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/nav-history-apis.html#dom-navigationtype-reload">HTML Standard: 7.2.6.3 Core infrastructure</see>
    /// </remarks>
    [Description("@#reload")]
    Reload = 2,

    /// <summary>
    /// The cancelable property will be false for some &quot;traverse&quot; navigations, such as those taking place inside child navigables, those crossing to new origins, or when the user attempts to traverse again shortly after a previous call to preventDefault() prevented them from doing so.
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/nav-history-apis.html#dom-navigationtype-traverse">HTML Standard: 7.2.6.3 Core infrastructure</see>
    /// </remarks>
    [Description("@#traverse")]
    Traverse = 3
}

/// <summary>
/// WebIDL enum ScrollRestoration。定义于 HTML Standard。
/// </summary>
/// <remarks>
/// <see href="https://html.spec.whatwg.org/multipage/nav-history-apis.html#scrollrestoration">HTML Standard: 7.2.5 The History interface</see>
/// </remarks>
[Description("@#ScrollRestoration")]
[ECMAScript]
[String]
public enum ScrollRestoration
{
    /// <summary>
    /// scroll restoration mode, a scroll restoration mode, initially &quot;auto&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/browsing-the-web.html#dom-scrollrestoration-auto">HTML Standard: 7.4.1.1 Session history entries</see>
    /// </remarks>
    [Description("@#auto")]
    Auto = 0,

    /// <summary>
    /// By default, using this method will delay the browser&apos;s scroll restoration logic for &quot;traverse&quot; or &quot;reload&quot; navigations, or its scroll-reset/scroll-to-a-fragment logic for &quot;push&quot; or &quot;replace&quot; navigations, until any handlers&apos; returned promises settle. The scroll option can be set to &quot;manual&quot; to turn off any browser-driven scroll behavior entirely for this navigation, or scroll() can be called before the promise settles to trigger this behavior early.
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/browsing-the-web.html#dom-scrollrestoration-manual">HTML Standard: 7.4.1.1 Session history entries</see>
    /// </remarks>
    [Description("@#manual")]
    Manual = 1
}

/// <summary>
/// WebIDL enum XRBodyJoint。定义于 WebXR Body Tracking Module - Level 1。
/// </summary>
/// <remarks>
/// <see href="https://immersive-web.github.io/body-tracking/#enumdef-xrbodyjoint">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
/// </remarks>
[Description("@#XRBodyJoint")]
[ECMAScript]
[String]
public enum XRBodyJoint
{
    /// <summary>
    /// JavaScript 字符串取值 “hips”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-hips">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#hips")]
    Hips = 0,

    /// <summary>
    /// JavaScript 字符串取值 “spine-lower”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-spine-lower">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#spine-lower")]
    SpineLower = 1,

    /// <summary>
    /// JavaScript 字符串取值 “spine-middle”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-spine-middle">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#spine-middle")]
    SpineMiddle = 2,

    /// <summary>
    /// JavaScript 字符串取值 “spine-upper”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-spine-upper">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#spine-upper")]
    SpineUpper = 3,

    /// <summary>
    /// JavaScript 字符串取值 “chest”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-chest">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#chest")]
    Chest = 4,

    /// <summary>
    /// JavaScript 字符串取值 “neck”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-neck">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#neck")]
    Neck = 5,

    /// <summary>
    /// JavaScript 字符串取值 “head”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-head">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#head")]
    Head = 6,

    /// <summary>
    /// JavaScript 字符串取值 “left-shoulder”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-shoulder">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#left-shoulder")]
    LeftShoulder = 7,

    /// <summary>
    /// JavaScript 字符串取值 “left-scapula”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-scapula">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#left-scapula")]
    LeftScapula = 8,

    /// <summary>
    /// JavaScript 字符串取值 “left-arm-upper”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-arm-upper">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#left-arm-upper")]
    LeftArmUpper = 9,

    /// <summary>
    /// JavaScript 字符串取值 “left-arm-lower”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-arm-lower">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#left-arm-lower")]
    LeftArmLower = 10,

    /// <summary>
    /// JavaScript 字符串取值 “left-hand-wrist-twist”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-hand-wrist-twist">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#left-hand-wrist-twist")]
    LeftHandWristTwist = 11,

    /// <summary>
    /// JavaScript 字符串取值 “right-shoulder”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-shoulder">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#right-shoulder")]
    RightShoulder = 12,

    /// <summary>
    /// JavaScript 字符串取值 “right-scapula”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-scapula">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#right-scapula")]
    RightScapula = 13,

    /// <summary>
    /// JavaScript 字符串取值 “right-arm-upper”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-arm-upper">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#right-arm-upper")]
    RightArmUpper = 14,

    /// <summary>
    /// JavaScript 字符串取值 “right-arm-lower”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-arm-lower">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#right-arm-lower")]
    RightArmLower = 15,

    /// <summary>
    /// JavaScript 字符串取值 “right-hand-wrist-twist”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-hand-wrist-twist">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#right-hand-wrist-twist")]
    RightHandWristTwist = 16,

    /// <summary>
    /// JavaScript 字符串取值 “left-hand-palm”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-hand-palm">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#left-hand-palm")]
    LeftHandPalm = 17,

    /// <summary>
    /// JavaScript 字符串取值 “left-hand-wrist”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-hand-wrist">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#left-hand-wrist")]
    LeftHandWrist = 18,

    /// <summary>
    /// JavaScript 字符串取值 “left-hand-thumb-metacarpal”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-hand-thumb-metacarpal">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#left-hand-thumb-metacarpal")]
    LeftHandThumbMetacarpal = 19,

    /// <summary>
    /// JavaScript 字符串取值 “left-hand-thumb-phalanx-proximal”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-hand-thumb-phalanx-proximal">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#left-hand-thumb-phalanx-proximal")]
    LeftHandThumbPhalanxProximal = 20,

    /// <summary>
    /// JavaScript 字符串取值 “left-hand-thumb-phalanx-distal”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-hand-thumb-phalanx-distal">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#left-hand-thumb-phalanx-distal")]
    LeftHandThumbPhalanxDistal = 21,

    /// <summary>
    /// JavaScript 字符串取值 “left-hand-thumb-tip”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-hand-thumb-tip">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#left-hand-thumb-tip")]
    LeftHandThumbTip = 22,

    /// <summary>
    /// JavaScript 字符串取值 “left-hand-index-metacarpal”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-hand-index-metacarpal">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#left-hand-index-metacarpal")]
    LeftHandIndexMetacarpal = 23,

    /// <summary>
    /// JavaScript 字符串取值 “left-hand-index-phalanx-proximal”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-hand-index-phalanx-proximal">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#left-hand-index-phalanx-proximal")]
    LeftHandIndexPhalanxProximal = 24,

    /// <summary>
    /// JavaScript 字符串取值 “left-hand-index-phalanx-intermediate”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-hand-index-phalanx-intermediate">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#left-hand-index-phalanx-intermediate")]
    LeftHandIndexPhalanxIntermediate = 25,

    /// <summary>
    /// JavaScript 字符串取值 “left-hand-index-phalanx-distal”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-hand-index-phalanx-distal">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#left-hand-index-phalanx-distal")]
    LeftHandIndexPhalanxDistal = 26,

    /// <summary>
    /// JavaScript 字符串取值 “left-hand-index-tip”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-hand-index-tip">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#left-hand-index-tip")]
    LeftHandIndexTip = 27,

    /// <summary>
    /// JavaScript 字符串取值 “left-hand-middle-phalanx-metacarpal”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-hand-middle-phalanx-metacarpal">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#left-hand-middle-phalanx-metacarpal")]
    LeftHandMiddlePhalanxMetacarpal = 28,

    /// <summary>
    /// JavaScript 字符串取值 “left-hand-middle-phalanx-proximal”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-hand-middle-phalanx-proximal">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#left-hand-middle-phalanx-proximal")]
    LeftHandMiddlePhalanxProximal = 29,

    /// <summary>
    /// JavaScript 字符串取值 “left-hand-middle-phalanx-intermediate”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-hand-middle-phalanx-intermediate">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#left-hand-middle-phalanx-intermediate")]
    LeftHandMiddlePhalanxIntermediate = 30,

    /// <summary>
    /// JavaScript 字符串取值 “left-hand-middle-phalanx-distal”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-hand-middle-phalanx-distal">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#left-hand-middle-phalanx-distal")]
    LeftHandMiddlePhalanxDistal = 31,

    /// <summary>
    /// JavaScript 字符串取值 “left-hand-middle-tip”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-hand-middle-tip">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#left-hand-middle-tip")]
    LeftHandMiddleTip = 32,

    /// <summary>
    /// JavaScript 字符串取值 “left-hand-ring-metacarpal”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-hand-ring-metacarpal">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#left-hand-ring-metacarpal")]
    LeftHandRingMetacarpal = 33,

    /// <summary>
    /// JavaScript 字符串取值 “left-hand-ring-phalanx-proximal”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-hand-ring-phalanx-proximal">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#left-hand-ring-phalanx-proximal")]
    LeftHandRingPhalanxProximal = 34,

    /// <summary>
    /// JavaScript 字符串取值 “left-hand-ring-phalanx-intermediate”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-hand-ring-phalanx-intermediate">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#left-hand-ring-phalanx-intermediate")]
    LeftHandRingPhalanxIntermediate = 35,

    /// <summary>
    /// JavaScript 字符串取值 “left-hand-ring-phalanx-distal”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-hand-ring-phalanx-distal">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#left-hand-ring-phalanx-distal")]
    LeftHandRingPhalanxDistal = 36,

    /// <summary>
    /// JavaScript 字符串取值 “left-hand-ring-tip”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-hand-ring-tip">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#left-hand-ring-tip")]
    LeftHandRingTip = 37,

    /// <summary>
    /// JavaScript 字符串取值 “left-hand-little-metacarpal”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-hand-little-metacarpal">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#left-hand-little-metacarpal")]
    LeftHandLittleMetacarpal = 38,

    /// <summary>
    /// JavaScript 字符串取值 “left-hand-little-phalanx-proximal”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-hand-little-phalanx-proximal">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#left-hand-little-phalanx-proximal")]
    LeftHandLittlePhalanxProximal = 39,

    /// <summary>
    /// JavaScript 字符串取值 “left-hand-little-phalanx-intermediate”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-hand-little-phalanx-intermediate">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#left-hand-little-phalanx-intermediate")]
    LeftHandLittlePhalanxIntermediate = 40,

    /// <summary>
    /// JavaScript 字符串取值 “left-hand-little-phalanx-distal”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-hand-little-phalanx-distal">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#left-hand-little-phalanx-distal")]
    LeftHandLittlePhalanxDistal = 41,

    /// <summary>
    /// JavaScript 字符串取值 “left-hand-little-tip”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-hand-little-tip">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#left-hand-little-tip")]
    LeftHandLittleTip = 42,

    /// <summary>
    /// JavaScript 字符串取值 “right-hand-palm”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-hand-palm">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#right-hand-palm")]
    RightHandPalm = 43,

    /// <summary>
    /// JavaScript 字符串取值 “right-hand-wrist”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-hand-wrist">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#right-hand-wrist")]
    RightHandWrist = 44,

    /// <summary>
    /// JavaScript 字符串取值 “right-hand-thumb-metacarpal”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-hand-thumb-metacarpal">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#right-hand-thumb-metacarpal")]
    RightHandThumbMetacarpal = 45,

    /// <summary>
    /// JavaScript 字符串取值 “right-hand-thumb-phalanx-proximal”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-hand-thumb-phalanx-proximal">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#right-hand-thumb-phalanx-proximal")]
    RightHandThumbPhalanxProximal = 46,

    /// <summary>
    /// JavaScript 字符串取值 “right-hand-thumb-phalanx-distal”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-hand-thumb-phalanx-distal">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#right-hand-thumb-phalanx-distal")]
    RightHandThumbPhalanxDistal = 47,

    /// <summary>
    /// JavaScript 字符串取值 “right-hand-thumb-tip”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-hand-thumb-tip">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#right-hand-thumb-tip")]
    RightHandThumbTip = 48,

    /// <summary>
    /// JavaScript 字符串取值 “right-hand-index-metacarpal”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-hand-index-metacarpal">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#right-hand-index-metacarpal")]
    RightHandIndexMetacarpal = 49,

    /// <summary>
    /// JavaScript 字符串取值 “right-hand-index-phalanx-proximal”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-hand-index-phalanx-proximal">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#right-hand-index-phalanx-proximal")]
    RightHandIndexPhalanxProximal = 50,

    /// <summary>
    /// JavaScript 字符串取值 “right-hand-index-phalanx-intermediate”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-hand-index-phalanx-intermediate">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#right-hand-index-phalanx-intermediate")]
    RightHandIndexPhalanxIntermediate = 51,

    /// <summary>
    /// JavaScript 字符串取值 “right-hand-index-phalanx-distal”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-hand-index-phalanx-distal">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#right-hand-index-phalanx-distal")]
    RightHandIndexPhalanxDistal = 52,

    /// <summary>
    /// JavaScript 字符串取值 “right-hand-index-tip”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-hand-index-tip">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#right-hand-index-tip")]
    RightHandIndexTip = 53,

    /// <summary>
    /// JavaScript 字符串取值 “right-hand-middle-metacarpal”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-hand-middle-metacarpal">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#right-hand-middle-metacarpal")]
    RightHandMiddleMetacarpal = 54,

    /// <summary>
    /// JavaScript 字符串取值 “right-hand-middle-phalanx-proximal”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-hand-middle-phalanx-proximal">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#right-hand-middle-phalanx-proximal")]
    RightHandMiddlePhalanxProximal = 55,

    /// <summary>
    /// JavaScript 字符串取值 “right-hand-middle-phalanx-intermediate”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-hand-middle-phalanx-intermediate">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#right-hand-middle-phalanx-intermediate")]
    RightHandMiddlePhalanxIntermediate = 56,

    /// <summary>
    /// JavaScript 字符串取值 “right-hand-middle-phalanx-distal”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-hand-middle-phalanx-distal">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#right-hand-middle-phalanx-distal")]
    RightHandMiddlePhalanxDistal = 57,

    /// <summary>
    /// JavaScript 字符串取值 “right-hand-middle-tip”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-hand-middle-tip">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#right-hand-middle-tip")]
    RightHandMiddleTip = 58,

    /// <summary>
    /// JavaScript 字符串取值 “right-hand-ring-metacarpal”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-hand-ring-metacarpal">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#right-hand-ring-metacarpal")]
    RightHandRingMetacarpal = 59,

    /// <summary>
    /// JavaScript 字符串取值 “right-hand-ring-phalanx-proximal”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-hand-ring-phalanx-proximal">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#right-hand-ring-phalanx-proximal")]
    RightHandRingPhalanxProximal = 60,

    /// <summary>
    /// JavaScript 字符串取值 “right-hand-ring-phalanx-intermediate”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-hand-ring-phalanx-intermediate">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#right-hand-ring-phalanx-intermediate")]
    RightHandRingPhalanxIntermediate = 61,

    /// <summary>
    /// JavaScript 字符串取值 “right-hand-ring-phalanx-distal”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-hand-ring-phalanx-distal">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#right-hand-ring-phalanx-distal")]
    RightHandRingPhalanxDistal = 62,

    /// <summary>
    /// JavaScript 字符串取值 “right-hand-ring-tip”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-hand-ring-tip">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#right-hand-ring-tip")]
    RightHandRingTip = 63,

    /// <summary>
    /// JavaScript 字符串取值 “right-hand-little-metacarpal”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-hand-little-metacarpal">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#right-hand-little-metacarpal")]
    RightHandLittleMetacarpal = 64,

    /// <summary>
    /// JavaScript 字符串取值 “right-hand-little-phalanx-proximal”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-hand-little-phalanx-proximal">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#right-hand-little-phalanx-proximal")]
    RightHandLittlePhalanxProximal = 65,

    /// <summary>
    /// JavaScript 字符串取值 “right-hand-little-phalanx-intermediate”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-hand-little-phalanx-intermediate">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#right-hand-little-phalanx-intermediate")]
    RightHandLittlePhalanxIntermediate = 66,

    /// <summary>
    /// JavaScript 字符串取值 “right-hand-little-phalanx-distal”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-hand-little-phalanx-distal">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#right-hand-little-phalanx-distal")]
    RightHandLittlePhalanxDistal = 67,

    /// <summary>
    /// JavaScript 字符串取值 “right-hand-little-tip”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-hand-little-tip">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#right-hand-little-tip")]
    RightHandLittleTip = 68,

    /// <summary>
    /// JavaScript 字符串取值 “left-upper-leg”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-upper-leg">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#left-upper-leg")]
    LeftUpperLeg = 69,

    /// <summary>
    /// JavaScript 字符串取值 “left-lower-leg”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-lower-leg">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#left-lower-leg")]
    LeftLowerLeg = 70,

    /// <summary>
    /// JavaScript 字符串取值 “left-foot-ankle-twist”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-foot-ankle-twist">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#left-foot-ankle-twist")]
    LeftFootAnkleTwist = 71,

    /// <summary>
    /// JavaScript 字符串取值 “left-foot-ankle”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-foot-ankle">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#left-foot-ankle")]
    LeftFootAnkle = 72,

    /// <summary>
    /// JavaScript 字符串取值 “left-foot-subtalar”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-foot-subtalar">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#left-foot-subtalar")]
    LeftFootSubtalar = 73,

    /// <summary>
    /// JavaScript 字符串取值 “left-foot-transverse”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-foot-transverse">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#left-foot-transverse")]
    LeftFootTransverse = 74,

    /// <summary>
    /// JavaScript 字符串取值 “left-foot-ball”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-left-foot-ball">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#left-foot-ball")]
    LeftFootBall = 75,

    /// <summary>
    /// JavaScript 字符串取值 “right-upper-leg”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-upper-leg">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#right-upper-leg")]
    RightUpperLeg = 76,

    /// <summary>
    /// JavaScript 字符串取值 “right-lower-leg”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-lower-leg">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#right-lower-leg")]
    RightLowerLeg = 77,

    /// <summary>
    /// JavaScript 字符串取值 “right-foot-ankle-twist”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-foot-ankle-twist">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#right-foot-ankle-twist")]
    RightFootAnkleTwist = 78,

    /// <summary>
    /// JavaScript 字符串取值 “right-foot-ankle”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-foot-ankle">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#right-foot-ankle")]
    RightFootAnkle = 79,

    /// <summary>
    /// JavaScript 字符串取值 “right-foot-subtalar”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-foot-subtalar">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#right-foot-subtalar")]
    RightFootSubtalar = 80,

    /// <summary>
    /// JavaScript 字符串取值 “right-foot-transverse”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-foot-transverse">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#right-foot-transverse")]
    RightFootTransverse = 81,

    /// <summary>
    /// JavaScript 字符串取值 “right-foot-ball”；属于 XRBodyJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/body-tracking/#dom-xrbodyjoint-right-foot-ball">WebXR Body Tracking Module - Level 1: 2.3 XRBody</see>
    /// </remarks>
    [Description("@#right-foot-ball")]
    RightFootBall = 82
}

/// <summary>
/// WebIDL enum XRDepthDataFormat。定义于 WebXR Depth Sensing Module。
/// </summary>
/// <remarks>
/// <see href="https://immersive-web.github.io/depth-sensing/#enumdef-xrdepthdataformat">WebXR Depth Sensing Module: 2.2 Intended depth type, data usage, and data formats</see>
/// </remarks>
[Description("@#XRDepthDataFormat")]
[ECMAScript]
[String]
public enum XRDepthDataFormat
{
    /// <summary>
    /// JavaScript 字符串取值 “luminance-alpha”；属于 XRDepthDataFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/depth-sensing/#dom-xrdepthdataformat-luminance-alpha">WebXR Depth Sensing Module: 2.2 Intended depth type, data usage, and data formats</see>
    /// </remarks>
    [Description("@#luminance-alpha")]
    LuminanceAlpha = 0,

    /// <summary>
    /// JavaScript 字符串取值 “float32”；属于 XRDepthDataFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/depth-sensing/#dom-xrdepthdataformat-float32">WebXR Depth Sensing Module: 2.2 Intended depth type, data usage, and data formats</see>
    /// </remarks>
    [Description("@#float32")]
    Float32 = 1,

    /// <summary>
    /// JavaScript 字符串取值 “unsigned-short”；属于 XRDepthDataFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/depth-sensing/#dom-xrdepthdataformat-unsigned-short">WebXR Depth Sensing Module: 2.2 Intended depth type, data usage, and data formats</see>
    /// </remarks>
    [Description("@#unsigned-short")]
    UnsignedShort = 2
}

/// <summary>
/// WebIDL enum XRDepthType。定义于 WebXR Depth Sensing Module。
/// </summary>
/// <remarks>
/// <see href="https://immersive-web.github.io/depth-sensing/#enumdef-xrdepthtype">WebXR Depth Sensing Module: 2.2 Intended depth type, data usage, and data formats</see>
/// </remarks>
[Description("@#XRDepthType")]
[ECMAScript]
[String]
public enum XRDepthType
{
    /// <summary>
    /// JavaScript 字符串取值 “raw”；属于 XRDepthType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/depth-sensing/#dom-xrdepthtype-raw">WebXR Depth Sensing Module: 2.2 Intended depth type, data usage, and data formats</see>
    /// </remarks>
    [Description("@#raw")]
    Raw = 0,

    /// <summary>
    /// JavaScript 字符串取值 “smooth”；属于 XRDepthType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/depth-sensing/#dom-xrdepthtype-smooth">WebXR Depth Sensing Module: 2.2 Intended depth type, data usage, and data formats</see>
    /// </remarks>
    [Description("@#smooth")]
    Smooth = 1
}

/// <summary>
/// WebIDL enum XRDepthUsage。定义于 WebXR Depth Sensing Module。
/// </summary>
/// <remarks>
/// <see href="https://immersive-web.github.io/depth-sensing/#enumdef-xrdepthusage">WebXR Depth Sensing Module: 2.2 Intended depth type, data usage, and data formats</see>
/// </remarks>
[Description("@#XRDepthUsage")]
[ECMAScript]
[String]
public enum XRDepthUsage
{
    /// <summary>
    /// JavaScript 字符串取值 “cpu-optimized”；属于 XRDepthUsage 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/depth-sensing/#dom-xrdepthusage-cpu-optimized">WebXR Depth Sensing Module: 2.2 Intended depth type, data usage, and data formats</see>
    /// </remarks>
    [Description("@#cpu-optimized")]
    CpuOptimized = 0,

    /// <summary>
    /// JavaScript 字符串取值 “gpu-optimized”；属于 XRDepthUsage 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/depth-sensing/#dom-xrdepthusage-gpu-optimized">WebXR Depth Sensing Module: 2.2 Intended depth type, data usage, and data formats</see>
    /// </remarks>
    [Description("@#gpu-optimized")]
    GpuOptimized = 1
}

/// <summary>
/// WebIDL enum XRDOMOverlayType。定义于 WebXR DOM Overlays Module。
/// </summary>
/// <remarks>
/// <see href="https://immersive-web.github.io/dom-overlays/#enumdef-xrdomoverlaytype">WebXR DOM Overlays Module: WebXR DOM Overlays Module</see>
/// </remarks>
[Description("@#XRDOMOverlayType")]
[ECMAScript]
[String]
public enum XRDOMOverlayType
{
    /// <summary>
    /// JavaScript 字符串取值 “screen”；属于 XRDOMOverlayType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/dom-overlays/#dom-xrdomoverlaytype-screen">WebXR DOM Overlays Module: WebXR DOM Overlays Module</see>
    /// </remarks>
    [Description("@#screen")]
    Screen = 0,

    /// <summary>
    /// JavaScript 字符串取值 “floating”；属于 XRDOMOverlayType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/dom-overlays/#dom-xrdomoverlaytype-floating">WebXR DOM Overlays Module: WebXR DOM Overlays Module</see>
    /// </remarks>
    [Description("@#floating")]
    Floating = 1,

    /// <summary>
    /// JavaScript 字符串取值 “head-locked”；属于 XRDOMOverlayType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/dom-overlays/#dom-xrdomoverlaytype-head-locked">WebXR DOM Overlays Module: WebXR DOM Overlays Module</see>
    /// </remarks>
    [Description("@#head-locked")]
    HeadLocked = 2
}

/// <summary>
/// WebIDL enum XRHitTestTrackableType。定义于 WebXR Hit Test Module。
/// </summary>
/// <remarks>
/// <see href="https://immersive-web.github.io/hit-test/#enumdef-xrhittesttrackabletype">WebXR Hit Test Module: WebXR Hit Test Module</see>
/// </remarks>
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
/// WebIDL enum XRLayerLayout。定义于 WebXR Layers API Level 1。
/// </summary>
/// <remarks>
/// <see href="https://immersive-web.github.io/layers/#enumdef-xrlayerlayout">WebXR Layers API Level 1: 3.2 XRLayerLayout</see>
/// </remarks>
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
    /// JavaScript 字符串取值 “mono”；属于 XRLayerLayout 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/layers/#dom-xrlayerlayout-mono">WebXR Layers API Level 1: 3.2 XRLayerLayout</see>
    /// </remarks>
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
/// WebIDL enum XRLayerQuality。定义于 WebXR Layers API Level 1。
/// </summary>
/// <remarks>
/// <see href="https://immersive-web.github.io/layers/#enumdef-xrlayerquality">WebXR Layers API Level 1: 3.3 XRLayerQuality</see>
/// </remarks>
[Description("@#XRLayerQuality")]
[ECMAScript]
[String]
public enum XRLayerQuality
{
    /// <summary>
    /// JavaScript 字符串取值 “default”；属于 XRLayerQuality 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/layers/#dom-xrlayerquality-default">WebXR Layers API Level 1: 3.3 XRLayerQuality</see>
    /// </remarks>
    [Description("@#default")]
    Default = 0,

    /// <summary>
    /// JavaScript 字符串取值 “text-optimized”；属于 XRLayerQuality 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/layers/#dom-xrlayerquality-text-optimized">WebXR Layers API Level 1: 3.3 XRLayerQuality</see>
    /// </remarks>
    [Description("@#text-optimized")]
    TextOptimized = 1,

    /// <summary>
    /// JavaScript 字符串取值 “graphics-optimized”；属于 XRLayerQuality 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/layers/#dom-xrlayerquality-graphics-optimized">WebXR Layers API Level 1: 3.3 XRLayerQuality</see>
    /// </remarks>
    [Description("@#graphics-optimized")]
    GraphicsOptimized = 2
}

/// <summary>
/// WebIDL enum XRTextureType。定义于 WebXR Layers API Level 1。
/// </summary>
/// <remarks>
/// <see href="https://immersive-web.github.io/layers/#enumdef-xrtexturetype">WebXR Layers API Level 1: 5.3 XRTextureType</see>
/// </remarks>
[Description("@#XRTextureType")]
[ECMAScript]
[String]
public enum XRTextureType
{
    /// <summary>
    /// JavaScript 字符串取值 “texture”；属于 XRTextureType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/layers/#dom-xrtexturetype-texture">WebXR Layers API Level 1: 5.3 XRTextureType</see>
    /// </remarks>
    [Description("@#texture")]
    Texture = 0,

    /// <summary>
    /// JavaScript 字符串取值 “texture-array”；属于 XRTextureType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/layers/#dom-xrtexturetype-texture-array">WebXR Layers API Level 1: 5.3 XRTextureType</see>
    /// </remarks>
    [Description("@#texture-array")]
    TextureArray = 1
}

/// <summary>
/// WebIDL enum XRReflectionFormat。定义于 WebXR Lighting Estimation API Level 1。
/// </summary>
/// <remarks>
/// <see href="https://immersive-web.github.io/lighting-estimation/#enumdef-xrreflectionformat">WebXR Lighting Estimation API Level 1: 2.2 XRReflectionFormat</see>
/// </remarks>
[Description("@#XRReflectionFormat")]
[ECMAScript]
[String]
public enum XRReflectionFormat
{
    /// <summary>
    /// JavaScript 字符串取值 “srgba8”；属于 XRReflectionFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/lighting-estimation/#dom-xrreflectionformat-srgba8">WebXR Lighting Estimation API Level 1: 2.2 XRReflectionFormat</see>
    /// </remarks>
    [Description("@#srgba8")]
    Srgba8 = 0,

    /// <summary>
    /// JavaScript 字符串取值 “rgba16f”；属于 XRReflectionFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/lighting-estimation/#dom-xrreflectionformat-rgba16f">WebXR Lighting Estimation API Level 1: 2.2 XRReflectionFormat</see>
    /// </remarks>
    [Description("@#rgba16f")]
    Rgba16f = 1
}

/// <summary>
/// WebIDL enum XRPlaneOrientation。定义于 WebXR Plane Detection Module。
/// </summary>
/// <remarks>
/// <see href="https://immersive-web.github.io/plane-detection/#enumdef-xrplaneorientation">WebXR Plane Detection Module: 3.1 XRPlaneOrientation</see>
/// </remarks>
[Description("@#XRPlaneOrientation")]
[ECMAScript]
[String]
public enum XRPlaneOrientation
{
    /// <summary>
    /// JavaScript 字符串取值 “horizontal”；属于 XRPlaneOrientation 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/plane-detection/#dom-xrplaneorientation-horizontal">WebXR Plane Detection Module: 3.1 XRPlaneOrientation</see>
    /// </remarks>
    [Description("@#horizontal")]
    Horizontal = 0,

    /// <summary>
    /// JavaScript 字符串取值 “vertical”；属于 XRPlaneOrientation 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/plane-detection/#dom-xrplaneorientation-vertical">WebXR Plane Detection Module: 3.1 XRPlaneOrientation</see>
    /// </remarks>
    [Description("@#vertical")]
    Vertical = 1
}

/// <summary>
/// Experimental: This is an experimental technologyCheck the Browser compatibility table carefully before using this in production. Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. The XRSession interface&apos;s read-only environmentBlendMode property identifies if, and to what degree, the computer-generated imagery is overlaid atop the real world. This is used to differentiate between fully-immersive VR sessions and AR sessions which render over a pass-through image of the real world, possibly partially transparently.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/XRSession/environmentBlendMode">MDN Web Docs: XREnvironmentBlendMode</see>
/// </remarks>
[Description("@#XREnvironmentBlendMode")]
[ECMAScript]
[String]
public enum XREnvironmentBlendMode
{
    /// <summary>
    /// JavaScript 字符串取值 “opaque”；属于 XREnvironmentBlendMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/webxr-ar-module/#dom-xrenvironmentblendmode-opaque">WebXR Augmented Reality Module - Level 1: 2.2 XREnvironmentBlendMode</see>
    /// </remarks>
    [Description("@#opaque")]
    Opaque = 0,

    /// <summary>
    /// JavaScript 字符串取值 “alpha-blend”；属于 XREnvironmentBlendMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/webxr-ar-module/#dom-xrenvironmentblendmode-alpha-blend">WebXR Augmented Reality Module - Level 1: 2.2 XREnvironmentBlendMode</see>
    /// </remarks>
    [Description("@#alpha-blend")]
    AlphaBlend = 1,

    /// <summary>
    /// JavaScript 字符串取值 “additive”；属于 XREnvironmentBlendMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/webxr-ar-module/#dom-xrenvironmentblendmode-additive">WebXR Augmented Reality Module - Level 1: 2.2 XREnvironmentBlendMode</see>
    /// </remarks>
    [Description("@#additive")]
    Additive = 2
}

/// <summary>
/// WebIDL enum XRInteractionMode。定义于 WebXR Augmented Reality Module - Level 1。
/// </summary>
/// <remarks>
/// <see href="https://immersive-web.github.io/webxr-ar-module/#enumdef-xrinteractionmode">WebXR Augmented Reality Module - Level 1: 2.3 XRInteractionMode</see>
/// </remarks>
[Description("@#XRInteractionMode")]
[ECMAScript]
[String]
public enum XRInteractionMode
{
    /// <summary>
    /// JavaScript 字符串取值 “screen-space”；属于 XRInteractionMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/webxr-ar-module/#dom-xrinteractionmode-screen-space">WebXR Augmented Reality Module - Level 1: 2.3 XRInteractionMode</see>
    /// </remarks>
    [Description("@#screen-space")]
    ScreenSpace = 0,

    /// <summary>
    /// JavaScript 字符串取值 “world-space”；属于 XRInteractionMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/webxr-ar-module/#dom-xrinteractionmode-world-space">WebXR Augmented Reality Module - Level 1: 2.3 XRInteractionMode</see>
    /// </remarks>
    [Description("@#world-space")]
    WorldSpace = 1
}

/// <summary>
/// WebIDL enum XRHandJoint。定义于 WebXR Hand Input Module - Level 1。
/// </summary>
/// <remarks>
/// <see href="https://immersive-web.github.io/webxr-hand-input/#enumdef-xrhandjoint">WebXR Hand Input Module - Level 1: 3.3 XRHand</see>
/// </remarks>
[Description("@#XRHandJoint")]
[ECMAScript]
[String]
public enum XRHandJoint
{
    /// <summary>
    /// JavaScript 字符串取值 “wrist”；属于 XRHandJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/webxr-hand-input/#dom-xrhandjoint-wrist">WebXR Hand Input Module - Level 1: WebXR Hand Input Module - Level 1</see>
    /// </remarks>
    [Description("@#wrist")]
    Wrist = 0,

    /// <summary>
    /// JavaScript 字符串取值 “thumb-metacarpal”；属于 XRHandJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/webxr-hand-input/#dom-xrhandjoint-thumb-metacarpal">WebXR Hand Input Module - Level 1: WebXR Hand Input Module - Level 1</see>
    /// </remarks>
    [Description("@#thumb-metacarpal")]
    ThumbMetacarpal = 1,

    /// <summary>
    /// JavaScript 字符串取值 “thumb-phalanx-proximal”；属于 XRHandJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/webxr-hand-input/#dom-xrhandjoint-thumb-phalanx-proximal">WebXR Hand Input Module - Level 1: WebXR Hand Input Module - Level 1</see>
    /// </remarks>
    [Description("@#thumb-phalanx-proximal")]
    ThumbPhalanxProximal = 2,

    /// <summary>
    /// JavaScript 字符串取值 “thumb-phalanx-distal”；属于 XRHandJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/webxr-hand-input/#dom-xrhandjoint-thumb-phalanx-distal">WebXR Hand Input Module - Level 1: WebXR Hand Input Module - Level 1</see>
    /// </remarks>
    [Description("@#thumb-phalanx-distal")]
    ThumbPhalanxDistal = 3,

    /// <summary>
    /// JavaScript 字符串取值 “thumb-tip”；属于 XRHandJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/webxr-hand-input/#dom-xrhandjoint-thumb-tip">WebXR Hand Input Module - Level 1: WebXR Hand Input Module - Level 1</see>
    /// </remarks>
    [Description("@#thumb-tip")]
    ThumbTip = 4,

    /// <summary>
    /// JavaScript 字符串取值 “index-finger-metacarpal”；属于 XRHandJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/webxr-hand-input/#dom-xrhandjoint-index-finger-metacarpal">WebXR Hand Input Module - Level 1: WebXR Hand Input Module - Level 1</see>
    /// </remarks>
    [Description("@#index-finger-metacarpal")]
    IndexFingerMetacarpal = 5,

    /// <summary>
    /// JavaScript 字符串取值 “index-finger-phalanx-proximal”；属于 XRHandJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/webxr-hand-input/#dom-xrhandjoint-index-finger-phalanx-proximal">WebXR Hand Input Module - Level 1: WebXR Hand Input Module - Level 1</see>
    /// </remarks>
    [Description("@#index-finger-phalanx-proximal")]
    IndexFingerPhalanxProximal = 6,

    /// <summary>
    /// JavaScript 字符串取值 “index-finger-phalanx-intermediate”；属于 XRHandJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/webxr-hand-input/#dom-xrhandjoint-index-finger-phalanx-intermediate">WebXR Hand Input Module - Level 1: WebXR Hand Input Module - Level 1</see>
    /// </remarks>
    [Description("@#index-finger-phalanx-intermediate")]
    IndexFingerPhalanxIntermediate = 7,

    /// <summary>
    /// JavaScript 字符串取值 “index-finger-phalanx-distal”；属于 XRHandJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/webxr-hand-input/#dom-xrhandjoint-index-finger-phalanx-distal">WebXR Hand Input Module - Level 1: WebXR Hand Input Module - Level 1</see>
    /// </remarks>
    [Description("@#index-finger-phalanx-distal")]
    IndexFingerPhalanxDistal = 8,

    /// <summary>
    /// JavaScript 字符串取值 “index-finger-tip”；属于 XRHandJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/webxr-hand-input/#dom-xrhandjoint-index-finger-tip">WebXR Hand Input Module - Level 1: WebXR Hand Input Module - Level 1</see>
    /// </remarks>
    [Description("@#index-finger-tip")]
    IndexFingerTip = 9,

    /// <summary>
    /// JavaScript 字符串取值 “middle-finger-metacarpal”；属于 XRHandJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/webxr-hand-input/#dom-xrhandjoint-middle-finger-metacarpal">WebXR Hand Input Module - Level 1: WebXR Hand Input Module - Level 1</see>
    /// </remarks>
    [Description("@#middle-finger-metacarpal")]
    MiddleFingerMetacarpal = 10,

    /// <summary>
    /// JavaScript 字符串取值 “middle-finger-phalanx-proximal”；属于 XRHandJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/webxr-hand-input/#dom-xrhandjoint-middle-finger-phalanx-proximal">WebXR Hand Input Module - Level 1: WebXR Hand Input Module - Level 1</see>
    /// </remarks>
    [Description("@#middle-finger-phalanx-proximal")]
    MiddleFingerPhalanxProximal = 11,

    /// <summary>
    /// JavaScript 字符串取值 “middle-finger-phalanx-intermediate”；属于 XRHandJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/webxr-hand-input/#dom-xrhandjoint-middle-finger-phalanx-intermediate">WebXR Hand Input Module - Level 1: WebXR Hand Input Module - Level 1</see>
    /// </remarks>
    [Description("@#middle-finger-phalanx-intermediate")]
    MiddleFingerPhalanxIntermediate = 12,

    /// <summary>
    /// JavaScript 字符串取值 “middle-finger-phalanx-distal”；属于 XRHandJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/webxr-hand-input/#dom-xrhandjoint-middle-finger-phalanx-distal">WebXR Hand Input Module - Level 1: WebXR Hand Input Module - Level 1</see>
    /// </remarks>
    [Description("@#middle-finger-phalanx-distal")]
    MiddleFingerPhalanxDistal = 13,

    /// <summary>
    /// JavaScript 字符串取值 “middle-finger-tip”；属于 XRHandJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/webxr-hand-input/#dom-xrhandjoint-middle-finger-tip">WebXR Hand Input Module - Level 1: WebXR Hand Input Module - Level 1</see>
    /// </remarks>
    [Description("@#middle-finger-tip")]
    MiddleFingerTip = 14,

    /// <summary>
    /// JavaScript 字符串取值 “ring-finger-metacarpal”；属于 XRHandJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/webxr-hand-input/#dom-xrhandjoint-ring-finger-metacarpal">WebXR Hand Input Module - Level 1: WebXR Hand Input Module - Level 1</see>
    /// </remarks>
    [Description("@#ring-finger-metacarpal")]
    RingFingerMetacarpal = 15,

    /// <summary>
    /// JavaScript 字符串取值 “ring-finger-phalanx-proximal”；属于 XRHandJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/webxr-hand-input/#dom-xrhandjoint-ring-finger-phalanx-proximal">WebXR Hand Input Module - Level 1: WebXR Hand Input Module - Level 1</see>
    /// </remarks>
    [Description("@#ring-finger-phalanx-proximal")]
    RingFingerPhalanxProximal = 16,

    /// <summary>
    /// JavaScript 字符串取值 “ring-finger-phalanx-intermediate”；属于 XRHandJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/webxr-hand-input/#dom-xrhandjoint-ring-finger-phalanx-intermediate">WebXR Hand Input Module - Level 1: WebXR Hand Input Module - Level 1</see>
    /// </remarks>
    [Description("@#ring-finger-phalanx-intermediate")]
    RingFingerPhalanxIntermediate = 17,

    /// <summary>
    /// JavaScript 字符串取值 “ring-finger-phalanx-distal”；属于 XRHandJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/webxr-hand-input/#dom-xrhandjoint-ring-finger-phalanx-distal">WebXR Hand Input Module - Level 1: WebXR Hand Input Module - Level 1</see>
    /// </remarks>
    [Description("@#ring-finger-phalanx-distal")]
    RingFingerPhalanxDistal = 18,

    /// <summary>
    /// JavaScript 字符串取值 “ring-finger-tip”；属于 XRHandJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/webxr-hand-input/#dom-xrhandjoint-ring-finger-tip">WebXR Hand Input Module - Level 1: WebXR Hand Input Module - Level 1</see>
    /// </remarks>
    [Description("@#ring-finger-tip")]
    RingFingerTip = 19,

    /// <summary>
    /// JavaScript 字符串取值 “pinky-finger-metacarpal”；属于 XRHandJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/webxr-hand-input/#dom-xrhandjoint-pinky-finger-metacarpal">WebXR Hand Input Module - Level 1: WebXR Hand Input Module - Level 1</see>
    /// </remarks>
    [Description("@#pinky-finger-metacarpal")]
    PinkyFingerMetacarpal = 20,

    /// <summary>
    /// JavaScript 字符串取值 “pinky-finger-phalanx-proximal”；属于 XRHandJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/webxr-hand-input/#dom-xrhandjoint-pinky-finger-phalanx-proximal">WebXR Hand Input Module - Level 1: WebXR Hand Input Module - Level 1</see>
    /// </remarks>
    [Description("@#pinky-finger-phalanx-proximal")]
    PinkyFingerPhalanxProximal = 21,

    /// <summary>
    /// JavaScript 字符串取值 “pinky-finger-phalanx-intermediate”；属于 XRHandJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/webxr-hand-input/#dom-xrhandjoint-pinky-finger-phalanx-intermediate">WebXR Hand Input Module - Level 1: WebXR Hand Input Module - Level 1</see>
    /// </remarks>
    [Description("@#pinky-finger-phalanx-intermediate")]
    PinkyFingerPhalanxIntermediate = 22,

    /// <summary>
    /// JavaScript 字符串取值 “pinky-finger-phalanx-distal”；属于 XRHandJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/webxr-hand-input/#dom-xrhandjoint-pinky-finger-phalanx-distal">WebXR Hand Input Module - Level 1: WebXR Hand Input Module - Level 1</see>
    /// </remarks>
    [Description("@#pinky-finger-phalanx-distal")]
    PinkyFingerPhalanxDistal = 23,

    /// <summary>
    /// JavaScript 字符串取值 “pinky-finger-tip”；属于 XRHandJoint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/webxr-hand-input/#dom-xrhandjoint-pinky-finger-tip">WebXR Hand Input Module - Level 1: WebXR Hand Input Module - Level 1</see>
    /// </remarks>
    [Description("@#pinky-finger-tip")]
    PinkyFingerTip = 24
}

/// <summary>
/// Experimental: This is an experimental technologyCheck the Browser compatibility table carefully before using this in production. Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. The XRView interface&apos;s read-only eye property is a string indicating which eye&apos;s viewpoint the XRView represents: left or right. For views which represent neither eye, such as monoscopic views, this property&apos;s value is none.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/XRView/eye">MDN Web Docs: XREye</see>
/// </remarks>
[Description("@#XREye")]
[ECMAScript]
[String]
public enum XREye
{
    /// <summary>
    /// JavaScript 字符串取值 “none”；属于 XREye 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/webxr/#dom-xreye-none">WebXR Device API: 7.2 XRView</see>
    /// </remarks>
    [Description("@#none")]
    None = 0,

    /// <summary>
    /// JavaScript 字符串取值 “left”；属于 XREye 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/webxr/#dom-xreye-left">WebXR Device API: 7.2 XRView</see>
    /// </remarks>
    [Description("@#left")]
    Left = 1,

    /// <summary>
    /// JavaScript 字符串取值 “right”；属于 XREye 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/webxr/#dom-xreye-right">WebXR Device API: 7.2 XRView</see>
    /// </remarks>
    [Description("@#right")]
    Right = 2
}

/// <summary>
/// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. The read-only XRInputSource property handedness indicates which of the user&apos;s hands the WebXR input source is associated with, or if it&apos;s not associated with a hand at all.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/XRInputSource/handedness">MDN Web Docs: XRHandedness</see>
/// </remarks>
[Description("@#XRHandedness")]
[ECMAScript]
[String]
public enum XRHandedness
{
    /// <summary>
    /// JavaScript 字符串取值 “none”；属于 XRHandedness 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/webxr/#dom-xrhandedness-none">WebXR Device API: 10.1 XRInputSource</see>
    /// </remarks>
    [Description("@#none")]
    None = 0,

    /// <summary>
    /// JavaScript 字符串取值 “left”；属于 XRHandedness 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/webxr/#dom-xrhandedness-left">WebXR Device API: 10.1 XRInputSource</see>
    /// </remarks>
    [Description("@#left")]
    Left = 1,

    /// <summary>
    /// JavaScript 字符串取值 “right”；属于 XRHandedness 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/webxr/#dom-xrhandedness-right">WebXR Device API: 10.1 XRInputSource</see>
    /// </remarks>
    [Description("@#right")]
    Right = 2
}

/// <summary>
/// Experimental: This is an experimental technologyCheck the Browser compatibility table carefully before using this in production. Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. The requestReferenceSpace() method of the XRSession interface returns a Promise that resolves with an instance of either XRReferenceSpace or XRBoundedReferenceSpace as appropriate given the type of reference space requested.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/XRSession/requestReferenceSpace">MDN Web Docs: XRReferenceSpaceType</see>
/// </remarks>
[Description("@#XRReferenceSpaceType")]
[ECMAScript]
[String]
public enum XRReferenceSpaceType
{
    /// <summary>
    /// JavaScript 字符串取值 “viewer”；属于 XRReferenceSpaceType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/webxr/#dom-xrreferencespacetype-viewer">WebXR Device API: 6.2 XRReferenceSpace</see>
    /// </remarks>
    [Description("@#viewer")]
    Viewer = 0,

    /// <summary>
    /// JavaScript 字符串取值 “local”；属于 XRReferenceSpaceType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/webxr/#dom-xrreferencespacetype-local">WebXR Device API: 6.2 XRReferenceSpace</see>
    /// </remarks>
    [Description("@#local")]
    Local = 1,

    /// <summary>
    /// JavaScript 字符串取值 “local-floor”；属于 XRReferenceSpaceType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/webxr/#dom-xrreferencespacetype-local-floor">WebXR Device API: 6.2 XRReferenceSpace</see>
    /// </remarks>
    [Description("@#local-floor")]
    LocalFloor = 2,

    /// <summary>
    /// JavaScript 字符串取值 “bounded-floor”；属于 XRReferenceSpaceType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/webxr/#dom-xrreferencespacetype-bounded-floor">WebXR Device API: 6.2 XRReferenceSpace</see>
    /// </remarks>
    [Description("@#bounded-floor")]
    BoundedFloor = 3,

    /// <summary>
    /// JavaScript 字符串取值 “unbounded”；属于 XRReferenceSpaceType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/webxr/#dom-xrreferencespacetype-unbounded">WebXR Device API: 6.2 XRReferenceSpace</see>
    /// </remarks>
    [Description("@#unbounded")]
    Unbounded = 4
}

/// <summary>
/// Experimental: This is an experimental technologyCheck the Browser compatibility table carefully before using this in production. Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. The XRSystem interface&apos;s requestSession() method returns a Promise which resolves to an XRSession object through which you can manage the requested type of WebXR session. While only one immersive VR session can be active at a time, multiple inline sessions can be in progress at once.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/XRSystem/requestSession">MDN Web Docs: XRSessionMode</see>
/// </remarks>
[Description("@#XRSessionMode")]
[ECMAScript]
[String]
public enum XRSessionMode
{
    /// <summary>
    /// JavaScript 字符串取值 “inline”；属于 XRSessionMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/webxr/#dom-xrsessionmode-inline">WebXR Device API: 3.3 XRSessionMode</see>
    /// </remarks>
    [Description("@#inline")]
    Inline = 0,

    /// <summary>
    /// JavaScript 字符串取值 “immersive-vr”；属于 XRSessionMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/webxr/#dom-xrsessionmode-immersive-vr">WebXR Device API: 3.3 XRSessionMode</see>
    /// </remarks>
    [Description("@#immersive-vr")]
    ImmersiveVr = 1,

    /// <summary>
    /// JavaScript 字符串取值 “immersive-ar”；属于 XRSessionMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/webxr/#dom-xrsessionmode-immersive-ar">WebXR Device API: 3.3 XRSessionMode</see>
    /// </remarks>
    [Description("@#immersive-ar")]
    ImmersiveAr = 2
}

/// <summary>
/// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. The read-only XRInputSource property targetRayMode indicates the method by which the target ray for the input source should be generated and how it should be presented to the user. Typically a target ray is drawn from the source of the targeting system along the target ray in the direction in which the user is looking or pointing. The style of the ray is generally up to you, as is the method for indicating the endpoint of the ray. The targeted point or object might be indicated by drawing a shape or highlighting the targeted surface or object. A target ray emitted by a hand controller: The target ray can be anything from a simple line (ideally fading over distance) to an animated effect, such as the science-fiction &quot;phaser&quot; style shown in the screenshot above.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/XRInputSource/targetRayMode">MDN Web Docs: XRTargetRayMode</see>
/// </remarks>
[Description("@#XRTargetRayMode")]
[ECMAScript]
[String]
public enum XRTargetRayMode
{
    /// <summary>
    /// JavaScript 字符串取值 “gaze”；属于 XRTargetRayMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/webxr/#dom-xrtargetraymode-gaze">WebXR Device API: 10.1 XRInputSource</see>
    /// </remarks>
    [Description("@#gaze")]
    Gaze = 0,

    /// <summary>
    /// JavaScript 字符串取值 “tracked-pointer”；属于 XRTargetRayMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/webxr/#dom-xrtargetraymode-tracked-pointer">WebXR Device API: 10.1 XRInputSource</see>
    /// </remarks>
    [Description("@#tracked-pointer")]
    TrackedPointer = 1,

    /// <summary>
    /// JavaScript 字符串取值 “screen”；属于 XRTargetRayMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/webxr/#dom-xrtargetraymode-screen">WebXR Device API: 10.1 XRInputSource</see>
    /// </remarks>
    [Description("@#screen")]
    Screen = 2,

    /// <summary>
    /// JavaScript 字符串取值 “transient-pointer”；属于 XRTargetRayMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/webxr/#dom-xrtargetraymode-transient-pointer">WebXR Device API: 10.1 XRInputSource</see>
    /// </remarks>
    [Description("@#transient-pointer")]
    TransientPointer = 3
}

/// <summary>
/// Experimental: This is an experimental technologyCheck the Browser compatibility table carefully before using this in production. Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. The read-only visibilityState property of the XRSession interface is a string indicating whether the WebXR content is currently visible to the user, and if it is, whether it&apos;s the primary focus. Every time the visibility state changes, a visibilitychange event is fired on the XRSession object.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/XRSession/visibilityState">MDN Web Docs: XRVisibilityState</see>
/// </remarks>
[Description("@#XRVisibilityState")]
[ECMAScript]
[String]
public enum XRVisibilityState
{
    /// <summary>
    /// JavaScript 字符串取值 “visible”；属于 XRVisibilityState 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/webxr/#dom-xrvisibilitystate-visible">WebXR Device API: 4.1 XRSession</see>
    /// </remarks>
    [Description("@#visible")]
    Visible = 0,

    /// <summary>
    /// JavaScript 字符串取值 “visible-blurred”；属于 XRVisibilityState 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/webxr/#dom-xrvisibilitystate-visible-blurred">WebXR Device API: 4.1 XRSession</see>
    /// </remarks>
    [Description("@#visible-blurred")]
    VisibleBlurred = 1,

    /// <summary>
    /// JavaScript 字符串取值 “hidden”；属于 XRVisibilityState 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://immersive-web.github.io/webxr/#dom-xrvisibilitystate-hidden">WebXR Device API: 4.1 XRSession</see>
    /// </remarks>
    [Description("@#hidden")]
    Hidden = 2
}

/// <summary>
/// WebIDL enum NotificationDirection。定义于 Notifications API Standard。
/// </summary>
/// <remarks>
/// <see href="https://notifications.spec.whatwg.org/#enumdef-notificationdirection">Notifications API Standard: 3 API</see>
/// </remarks>
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
    /// JavaScript 字符串取值 “ltr”；属于 NotificationDirection 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://notifications.spec.whatwg.org/#dom-notificationdirection-ltr">Notifications API Standard: 3 API</see>
    /// </remarks>
    [Description("@#ltr")]
    Ltr = 1,

    /// <summary>
    /// JavaScript 字符串取值 “rtl”；属于 NotificationDirection 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://notifications.spec.whatwg.org/#dom-notificationdirection-rtl">Notifications API Standard: 3 API</see>
    /// </remarks>
    [Description("@#rtl")]
    Rtl = 2
}

/// <summary>
/// WebIDL enum NotificationPermission。定义于 Notifications API Standard。
/// </summary>
/// <remarks>
/// <see href="https://notifications.spec.whatwg.org/#enumdef-notificationpermission">Notifications API Standard: 3 API</see>
/// </remarks>
[Description("@#NotificationPermission")]
[ECMAScript]
[String]
public enum NotificationPermission
{
    /// <summary>
    /// JavaScript 字符串取值 “default”；属于 NotificationPermission 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://notifications.spec.whatwg.org/#dom-notificationpermission-default">Notifications API Standard: 3 API</see>
    /// </remarks>
    [Description("@#default")]
    Default = 0,

    /// <summary>
    /// JavaScript 字符串取值 “denied”；属于 NotificationPermission 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://notifications.spec.whatwg.org/#dom-notificationpermission-denied">Notifications API Standard: 3 API</see>
    /// </remarks>
    [Description("@#denied")]
    Denied = 1,

    /// <summary>
    /// JavaScript 字符串取值 “granted”；属于 NotificationPermission 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://notifications.spec.whatwg.org/#dom-notificationpermission-granted">Notifications API Standard: 3 API</see>
    /// </remarks>
    [Description("@#granted")]
    Granted = 2
}

/// <summary>
/// WebIDL enum SameSiteCookiesType。定义于 Extending Storage Access API (SAA) to non-cookie storage。
/// </summary>
/// <remarks>
/// <see href="https://privacycg.github.io/saa-non-cookie-storage/#enumdef-samesitecookiestype">Extending Storage Access API (SAA) to non-cookie storage: 2.3.9 Shared Workers</see>
/// </remarks>
[Description("@#SameSiteCookiesType")]
[ECMAScript]
[String]
public enum SameSiteCookiesType
{
    /// <summary>
    /// JavaScript 字符串取值 “all”；属于 SameSiteCookiesType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://privacycg.github.io/saa-non-cookie-storage/#dom-samesitecookiestype-all">Extending Storage Access API (SAA) to non-cookie storage: 2.3.9 Shared Workers</see>
    /// </remarks>
    [Description("@#all")]
    All = 0,

    /// <summary>
    /// JavaScript 字符串取值 “none”；属于 SameSiteCookiesType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://privacycg.github.io/saa-non-cookie-storage/#dom-samesitecookiestype-none">Extending Storage Access API (SAA) to non-cookie storage: 2.3.9 Shared Workers</see>
    /// </remarks>
    [Description("@#none")]
    None = 1
}

/// <summary>
/// WebIDL enum ReadableStreamReaderMode。定义于 Streams Standard。
/// </summary>
/// <remarks>
/// <see href="https://streams.spec.whatwg.org/#enumdef-readablestreamreadermode">Streams Standard: 4.2.1 Interface definition</see>
/// </remarks>
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
/// WebIDL enum ReadableStreamType。定义于 Streams Standard。
/// </summary>
/// <remarks>
/// <see href="https://streams.spec.whatwg.org/#enumdef-readablestreamtype">Streams Standard: 4.2.3 The underlying source API</see>
/// </remarks>
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
/// WebIDL enum IdentityCredentialRequestOptionsContext。定义于 Federated Credential Management API。
/// </summary>
/// <remarks>
/// <see href="https://w3c-fedid.github.io/FedCM/#enumdef-identitycredentialrequestoptionscontext">Federated Credential Management API: 2.4 The CredentialRequestOptions</see>
/// </remarks>
[Description("@#IdentityCredentialRequestOptionsContext")]
[ECMAScript]
[String]
public enum IdentityCredentialRequestOptionsContext
{
    /// <summary>
    /// JavaScript 字符串取值 “signin”；属于 IdentityCredentialRequestOptionsContext 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c-fedid.github.io/FedCM/#dom-identitycredentialrequestoptionscontext-signin">Federated Credential Management API: 2.4 The CredentialRequestOptions</see>
    /// </remarks>
    [Description("@#signin")]
    Signin = 0,

    /// <summary>
    /// JavaScript 字符串取值 “signup”；属于 IdentityCredentialRequestOptionsContext 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c-fedid.github.io/FedCM/#dom-identitycredentialrequestoptionscontext-signup">Federated Credential Management API: 2.4 The CredentialRequestOptions</see>
    /// </remarks>
    [Description("@#signup")]
    Signup = 1,

    /// <summary>
    /// JavaScript 字符串取值 “use”；属于 IdentityCredentialRequestOptionsContext 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c-fedid.github.io/FedCM/#dom-identitycredentialrequestoptionscontext-use">Federated Credential Management API: 2.4 The CredentialRequestOptions</see>
    /// </remarks>
    [Description("@#use")]
    Use = 2,

    /// <summary>
    /// JavaScript 字符串取值 “continue”；属于 IdentityCredentialRequestOptionsContext 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c-fedid.github.io/FedCM/#dom-identitycredentialrequestoptionscontext-continue">Federated Credential Management API: 2.4 The CredentialRequestOptions</see>
    /// </remarks>
    [Description("@#continue")]
    Continue = 3
}

/// <summary>
/// WebIDL enum IdentityCredentialRequestOptionsMode。定义于 Federated Credential Management API。
/// </summary>
/// <remarks>
/// <see href="https://w3c-fedid.github.io/FedCM/#enumdef-identitycredentialrequestoptionsmode">Federated Credential Management API: 2.4 The CredentialRequestOptions</see>
/// </remarks>
[Description("@#IdentityCredentialRequestOptionsMode")]
[ECMAScript]
[String]
public enum IdentityCredentialRequestOptionsMode
{
    /// <summary>
    /// JavaScript 字符串取值 “active”；属于 IdentityCredentialRequestOptionsMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c-fedid.github.io/FedCM/#dom-identitycredentialrequestoptionsmode-active">Federated Credential Management API: 2.4 The CredentialRequestOptions</see>
    /// </remarks>
    [Description("@#active")]
    Active = 0,

    /// <summary>
    /// JavaScript 字符串取值 “passive”；属于 IdentityCredentialRequestOptionsMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c-fedid.github.io/FedCM/#dom-identitycredentialrequestoptionsmode-passive">Federated Credential Management API: 2.4 The CredentialRequestOptions</see>
    /// </remarks>
    [Description("@#passive")]
    Passive = 1
}

/// <summary>
/// WebIDL enum LoginStatus。定义于 Login Status API。
/// </summary>
/// <remarks>
/// <see href="https://w3c-fedid.github.io/login-status/#enumdef-loginstatus">Login Status API: 5 JavaScript API</see>
/// </remarks>
[Description("@#LoginStatus")]
[ECMAScript]
[String]
public enum LoginStatus
{
    /// <summary>
    /// JavaScript 字符串取值 “logged-in”；属于 LoginStatus 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c-fedid.github.io/login-status/#dom-loginstatus-logged-in">Login Status API: Login Status API</see>
    /// </remarks>
    [Description("@#logged-in")]
    LoggedIn = 0,

    /// <summary>
    /// JavaScript 字符串取值 “logged-out”；属于 LoginStatus 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c-fedid.github.io/login-status/#dom-loginstatus-logged-out">Login Status API: Login Status API</see>
    /// </remarks>
    [Description("@#logged-out")]
    LoggedOut = 1
}

/// <summary>
/// WebIDL enum EndingType。定义于 File API。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/FileAPI/#enumdef-endingtype">File API: 3 The Blob Interface and Binary Data</see>
/// </remarks>
[Description("@#EndingType")]
[ECMAScript]
[String]
public enum EndingType
{
    /// <summary>
    /// JavaScript 字符串取值 “transparent”；属于 EndingType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/FileAPI/#dom-endingtype-transparent">File API: 3 The Blob Interface and Binary Data</see>
    /// </remarks>
    [Description("@#transparent")]
    Transparent = 0,

    /// <summary>
    /// JavaScript 字符串取值 “native”；属于 EndingType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/FileAPI/#dom-endingtype-native">File API: 3 The Blob Interface and Binary Data</see>
    /// </remarks>
    [Description("@#native")]
    Native = 1
}

/// <summary>
/// WebIDL enum IDBCursorDirection。定义于 Indexed Database API 3.0。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/IndexedDB/#enumdef-idbcursordirection">Indexed Database API 3.0: 4.9 The IDBCursor interface</see>
/// </remarks>
[Description("@#IDBCursorDirection")]
[ECMAScript]
[String]
public enum IDBCursorDirection
{
    /// <summary>
    /// JavaScript 字符串取值 “next”；属于 IDBCursorDirection 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/IndexedDB/#dom-idbcursordirection-next">Indexed Database API 3.0: 4.9 The IDBCursor interface</see>
    /// </remarks>
    [Description("@#next")]
    Next = 0,

    /// <summary>
    /// JavaScript 字符串取值 “nextunique”；属于 IDBCursorDirection 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/IndexedDB/#dom-idbcursordirection-nextunique">Indexed Database API 3.0: 4.9 The IDBCursor interface</see>
    /// </remarks>
    [Description("@#nextunique")]
    Nextunique = 1,

    /// <summary>
    /// JavaScript 字符串取值 “prev”；属于 IDBCursorDirection 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/IndexedDB/#dom-idbcursordirection-prev">Indexed Database API 3.0: 4.9 The IDBCursor interface</see>
    /// </remarks>
    [Description("@#prev")]
    Prev = 2,

    /// <summary>
    /// JavaScript 字符串取值 “prevunique”；属于 IDBCursorDirection 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/IndexedDB/#dom-idbcursordirection-prevunique">Indexed Database API 3.0: 4.9 The IDBCursor interface</see>
    /// </remarks>
    [Description("@#prevunique")]
    Prevunique = 3
}

/// <summary>
/// WebIDL enum IDBRequestReadyState。定义于 Indexed Database API 3.0。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/IndexedDB/#enumdef-idbrequestreadystate">Indexed Database API 3.0: 4.1 The IDBRequest interface</see>
/// </remarks>
[Description("@#IDBRequestReadyState")]
[ECMAScript]
[String]
public enum IDBRequestReadyState
{
    /// <summary>
    /// JavaScript 字符串取值 “pending”；属于 IDBRequestReadyState 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/IndexedDB/#dom-idbrequestreadystate-pending">Indexed Database API 3.0: 4.1 The IDBRequest interface</see>
    /// </remarks>
    [Description("@#pending")]
    Pending = 0,

    /// <summary>
    /// JavaScript 字符串取值 “done”；属于 IDBRequestReadyState 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/IndexedDB/#dom-idbrequestreadystate-done">Indexed Database API 3.0: 4.1 The IDBRequest interface</see>
    /// </remarks>
    [Description("@#done")]
    Done = 1
}

/// <summary>
/// WebIDL enum IDBTransactionDurability。定义于 Indexed Database API 3.0。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/IndexedDB/#enumdef-idbtransactiondurability">Indexed Database API 3.0: 4.4 The IDBDatabase interface</see>
/// </remarks>
[Description("@#IDBTransactionDurability")]
[ECMAScript]
[String]
public enum IDBTransactionDurability
{
    /// <summary>
    /// JavaScript 字符串取值 “default”；属于 IDBTransactionDurability 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/IndexedDB/#dom-idbtransactiondurability-default">Indexed Database API 3.0: 4.4 The IDBDatabase interface</see>
    /// </remarks>
    [Description("@#default")]
    Default = 0,

    /// <summary>
    /// JavaScript 字符串取值 “strict”；属于 IDBTransactionDurability 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/IndexedDB/#dom-idbtransactiondurability-strict">Indexed Database API 3.0: 4.4 The IDBDatabase interface</see>
    /// </remarks>
    [Description("@#strict")]
    Strict = 1,

    /// <summary>
    /// JavaScript 字符串取值 “relaxed”；属于 IDBTransactionDurability 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/IndexedDB/#dom-idbtransactiondurability-relaxed">Indexed Database API 3.0: 4.4 The IDBDatabase interface</see>
    /// </remarks>
    [Description("@#relaxed")]
    Relaxed = 2
}

/// <summary>
/// WebIDL enum IDBTransactionMode。定义于 Indexed Database API 3.0。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/IndexedDB/#enumdef-idbtransactionmode">Indexed Database API 3.0: 4.10 The IDBTransaction interface</see>
/// </remarks>
[Description("@#IDBTransactionMode")]
[ECMAScript]
[String]
public enum IDBTransactionMode
{
    /// <summary>
    /// JavaScript 字符串取值 “readonly”；属于 IDBTransactionMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/IndexedDB/#dom-idbtransactionmode-readonly">Indexed Database API 3.0: 4.10 The IDBTransaction interface</see>
    /// </remarks>
    [Description("@#readonly")]
    Readonly = 0,

    /// <summary>
    /// JavaScript 字符串取值 “readwrite”；属于 IDBTransactionMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/IndexedDB/#dom-idbtransactionmode-readwrite">Indexed Database API 3.0: 4.10 The IDBTransaction interface</see>
    /// </remarks>
    [Description("@#readwrite")]
    Readwrite = 1,

    /// <summary>
    /// JavaScript 字符串取值 “versionchange”；属于 IDBTransactionMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/IndexedDB/#dom-idbtransactionmode-versionchange">Indexed Database API 3.0: 4.10 The IDBTransaction interface</see>
    /// </remarks>
    [Description("@#versionchange")]
    Versionchange = 2
}

/// <summary>
/// WebIDL enum ClientType。定义于 Service Workers Nightly。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/ServiceWorker/#enumdef-clienttype">Service Workers Nightly: 4.3 Clients</see>
/// </remarks>
[Description("@#ClientType")]
[ECMAScript]
[String]
public enum ClientType
{
    /// <summary>
    /// JavaScript 字符串取值 “window”；属于 ClientType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/ServiceWorker/#dom-clienttype-window">Service Workers Nightly: 4.3 Clients</see>
    /// </remarks>
    [Description("@#window")]
    Window = 0,

    /// <summary>
    /// JavaScript 字符串取值 “worker”；属于 ClientType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/ServiceWorker/#dom-clienttype-worker">Service Workers Nightly: 4.3 Clients</see>
    /// </remarks>
    [Description("@#worker")]
    Worker = 1,

    /// <summary>
    /// JavaScript 字符串取值 “sharedworker”；属于 ClientType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/ServiceWorker/#dom-clienttype-sharedworker">Service Workers Nightly: 4.3 Clients</see>
    /// </remarks>
    [Description("@#sharedworker")]
    Sharedworker = 2,

    /// <summary>
    /// JavaScript 字符串取值 “all”；属于 ClientType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/ServiceWorker/#dom-clienttype-all">Service Workers Nightly: 4.3 Clients</see>
    /// </remarks>
    [Description("@#all")]
    All = 3
}

/// <summary>
/// WebIDL enum FrameType。定义于 Service Workers Nightly。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/ServiceWorker/#enumdef-frametype">Service Workers Nightly: 4.2 Client</see>
/// </remarks>
[Description("@#FrameType")]
[ECMAScript]
[String]
public enum FrameType
{
    /// <summary>
    /// JavaScript 字符串取值 “auxiliary”；属于 FrameType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/ServiceWorker/#dom-frametype-auxiliary">Service Workers Nightly: 4.2 Client</see>
    /// </remarks>
    [Description("@#auxiliary")]
    Auxiliary = 0,

    /// <summary>
    /// JavaScript 字符串取值 “top-level”；属于 FrameType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/ServiceWorker/#dom-frametype-top-level">Service Workers Nightly: 4.2 Client</see>
    /// </remarks>
    [Description("@#top-level")]
    TopLevel = 1,

    /// <summary>
    /// JavaScript 字符串取值 “nested”；属于 FrameType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/ServiceWorker/#dom-frametype-nested">Service Workers Nightly: 4.2 Client</see>
    /// </remarks>
    [Description("@#nested")]
    Nested = 2,

    /// <summary>
    /// JavaScript 字符串取值 “none”；属于 FrameType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/ServiceWorker/#dom-frametype-none">Service Workers Nightly: 4.2 Client</see>
    /// </remarks>
    [Description("@#none")]
    None = 3
}

/// <summary>
/// WebIDL enum RouterSourceEnum。定义于 Service Workers Nightly。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/ServiceWorker/#enumdef-routersourceenum">Service Workers Nightly: 4.5 InstallEvent</see>
/// </remarks>
[Description("@#RouterSourceEnum")]
[ECMAScript]
[String]
public enum RouterSourceEnum
{
    /// <summary>
    /// JavaScript 字符串取值 “cache”；属于 RouterSourceEnum 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/ServiceWorker/#dom-routersourceenum-cache">Service Workers Nightly: 4.5 InstallEvent</see>
    /// </remarks>
    [Description("@#cache")]
    Cache = 0,

    /// <summary>
    /// JavaScript 字符串取值 “fetch-event”；属于 RouterSourceEnum 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/ServiceWorker/#dom-routersourceenum-fetch-event">Service Workers Nightly: 4.5 InstallEvent</see>
    /// </remarks>
    [Description("@#fetch-event")]
    FetchEvent = 1,

    /// <summary>
    /// JavaScript 字符串取值 “network”；属于 RouterSourceEnum 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/ServiceWorker/#dom-routersourceenum-network">Service Workers Nightly: 4.5 InstallEvent</see>
    /// </remarks>
    [Description("@#network")]
    Network = 2,

    /// <summary>
    /// JavaScript 字符串取值 “race-network-and-fetch-handler”；属于 RouterSourceEnum 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/ServiceWorker/#dom-routersourceenum-race-network-and-fetch-handler">Service Workers Nightly: 4.5 InstallEvent</see>
    /// </remarks>
    [Description("@#race-network-and-fetch-handler")]
    RaceNetworkAndFetchHandler = 3
}

/// <summary>
/// WebIDL enum RunningStatus。定义于 Service Workers Nightly。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/ServiceWorker/#enumdef-runningstatus">Service Workers Nightly: 4.5 InstallEvent</see>
/// </remarks>
[Description("@#RunningStatus")]
[ECMAScript]
[String]
public enum RunningStatus
{
    /// <summary>
    /// JavaScript 字符串取值 “running”；属于 RunningStatus 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/ServiceWorker/#dom-runningstatus-running">Service Workers Nightly: 4.5 InstallEvent</see>
    /// </remarks>
    [Description("@#running")]
    Running = 0,

    /// <summary>
    /// JavaScript 字符串取值 “not-running”；属于 RunningStatus 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/ServiceWorker/#dom-runningstatus-not-running">Service Workers Nightly: 4.5 InstallEvent</see>
    /// </remarks>
    [Description("@#not-running")]
    NotRunning = 1
}

/// <summary>
/// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The state read-only property of the ServiceWorker interface returns a string representing the current state of the service worker. It can be one of the following values: parsed, installing, installed, activating, activated, or redundant.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/ServiceWorker/state">MDN Web Docs: ServiceWorkerState</see>
/// </remarks>
[Description("@#ServiceWorkerState")]
[ECMAScript]
[String]
public enum ServiceWorkerState
{
    /// <summary>
    /// JavaScript 字符串取值 “parsed”；属于 ServiceWorkerState 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/ServiceWorker/#dom-serviceworkerstate-parsed">Service Workers Nightly: 3.1 ServiceWorker</see>
    /// </remarks>
    [Description("@#parsed")]
    Parsed = 0,

    /// <summary>
    /// JavaScript 字符串取值 “installing”；属于 ServiceWorkerState 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/ServiceWorker/#dom-serviceworkerstate-installing">Service Workers Nightly: 3.1 ServiceWorker</see>
    /// </remarks>
    [Description("@#installing")]
    Installing = 1,

    /// <summary>
    /// JavaScript 字符串取值 “installed”；属于 ServiceWorkerState 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/ServiceWorker/#dom-serviceworkerstate-installed">Service Workers Nightly: 3.1 ServiceWorker</see>
    /// </remarks>
    [Description("@#installed")]
    Installed = 2,

    /// <summary>
    /// JavaScript 字符串取值 “activating”；属于 ServiceWorkerState 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/ServiceWorker/#dom-serviceworkerstate-activating">Service Workers Nightly: 3.1 ServiceWorker</see>
    /// </remarks>
    [Description("@#activating")]
    Activating = 3,

    /// <summary>
    /// JavaScript 字符串取值 “activated”；属于 ServiceWorkerState 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/ServiceWorker/#dom-serviceworkerstate-activated">Service Workers Nightly: 3.1 ServiceWorker</see>
    /// </remarks>
    [Description("@#activated")]
    Activated = 4,

    /// <summary>
    /// JavaScript 字符串取值 “redundant”；属于 ServiceWorkerState 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/ServiceWorker/#dom-serviceworkerstate-redundant">Service Workers Nightly: 3.1 ServiceWorker</see>
    /// </remarks>
    [Description("@#redundant")]
    Redundant = 5
}

/// <summary>
/// WebIDL enum ServiceWorkerUpdateViaCache。定义于 Service Workers Nightly。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/ServiceWorker/#enumdef-serviceworkerupdateviacache">Service Workers Nightly: 3.2 ServiceWorkerRegistration</see>
/// </remarks>
[Description("@#ServiceWorkerUpdateViaCache")]
[ECMAScript]
[String]
public enum ServiceWorkerUpdateViaCache
{
    /// <summary>
    /// JavaScript 字符串取值 “imports”；属于 ServiceWorkerUpdateViaCache 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/ServiceWorker/#dom-serviceworkerupdateviacache-imports">Service Workers Nightly: 3.2 ServiceWorkerRegistration</see>
    /// </remarks>
    [Description("@#imports")]
    Imports = 0,

    /// <summary>
    /// JavaScript 字符串取值 “all”；属于 ServiceWorkerUpdateViaCache 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/ServiceWorker/#dom-serviceworkerupdateviacache-all">Service Workers Nightly: 3.2 ServiceWorkerRegistration</see>
    /// </remarks>
    [Description("@#all")]
    All = 1,

    /// <summary>
    /// JavaScript 字符串取值 “none”；属于 ServiceWorkerUpdateViaCache 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/ServiceWorker/#dom-serviceworkerupdateviacache-none">Service Workers Nightly: 3.2 ServiceWorkerRegistration</see>
    /// </remarks>
    [Description("@#none")]
    None = 2
}

/// <summary>
/// WebIDL enum AccelerometerLocalCoordinateSystem。定义于 Accelerometer。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/accelerometer/#enumdef-accelerometerlocalcoordinatesystem">Accelerometer: 7.1 The Accelerometer Interface</see>
/// </remarks>
[Description("@#AccelerometerLocalCoordinateSystem")]
[ECMAScript]
[String]
public enum AccelerometerLocalCoordinateSystem
{
    /// <summary>
    /// JavaScript 字符串取值 “device”；属于 AccelerometerLocalCoordinateSystem 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/accelerometer/#dom-accelerometerlocalcoordinatesystem-device">Accelerometer: 7.1 The Accelerometer Interface</see>
    /// </remarks>
    [Description("@#device")]
    Device = 0,

    /// <summary>
    /// JavaScript 字符串取值 “screen”；属于 AccelerometerLocalCoordinateSystem 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/accelerometer/#dom-accelerometerlocalcoordinatesystem-screen">Accelerometer: 7.1 The Accelerometer Interface</see>
    /// </remarks>
    [Description("@#screen")]
    Screen = 1
}

/// <summary>
/// WebIDL enum AriaNotifyPriority。定义于 Accessible Rich Internet Applications (WAI-ARIA) 1.3。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/aria/#dom-arianotifypriority">Accessible Rich Internet Applications (WAI-ARIA) 1.3: 10.2 Interface Mixin ARIANotifyMixin</see>
/// </remarks>
[Description("@#AriaNotifyPriority")]
[ECMAScript]
[String]
public enum AriaNotifyPriority
{
    /// <summary>
    /// JavaScript 字符串取值 “normal”；属于 AriaNotifyPriority 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/aria/#dom-arianotifypriority-normal">Accessible Rich Internet Applications (WAI-ARIA) 1.3: 10.2 Interface Mixin ARIANotifyMixin</see>
    /// </remarks>
    [Description("@#normal")]
    Normal = 0,

    /// <summary>
    /// JavaScript 字符串取值 “high”；属于 AriaNotifyPriority 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/aria/#dom-arianotifypriority-high">Accessible Rich Internet Applications (WAI-ARIA) 1.3: 10.2 Interface Mixin ARIANotifyMixin</see>
    /// </remarks>
    [Description("@#high")]
    High = 1
}

/// <summary>
/// WebIDL enum AttributionAggregationProtocol。定义于 Attribution Level 1。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/attribution/#enumdef-attributionaggregationprotocol">Attribution Level 1: 3.3 Finding a Supported Aggregation Service</see>
/// </remarks>
[Description("@#AttributionAggregationProtocol")]
[ECMAScript]
[String]
public enum AttributionAggregationProtocol
{
    /// <summary>
    /// The URL for &quot;dap-18-histogram&quot; is expected to identify the DAP Leader role. Implementations need to obtain HPKE configuration for both Aggregators statically; see #unconfigured.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/attribution/#dom-attributionaggregationprotocol-dap-18-histogram">Attribution Level 1: 3.3 Finding a Supported Aggregation Service</see>
    /// </remarks>
    [Description("@#dap-18-histogram")]
    Dap18Histogram = 0
}

/// <summary>
/// WebIDL enum AudioSessionState。定义于 Audio Session。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/audio-session/#enumdef-audiosessionstate">Audio Session: 3.2 Audio session states</see>
/// </remarks>
[Description("@#AudioSessionState")]
[ECMAScript]
[String]
public enum AudioSessionState
{
    /// <summary>
    /// JavaScript 字符串取值 “inactive”；属于 AudioSessionState 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/audio-session/#dom-audiosessionstate-inactive">Audio Session: 3.2 Audio session states</see>
    /// </remarks>
    [Description("@#inactive")]
    Inactive = 0,

    /// <summary>
    /// JavaScript 字符串取值 “active”；属于 AudioSessionState 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/audio-session/#dom-audiosessionstate-active">Audio Session: 3.2 Audio session states</see>
    /// </remarks>
    [Description("@#active")]
    Active = 1,

    /// <summary>
    /// JavaScript 字符串取值 “interrupted”；属于 AudioSessionState 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/audio-session/#dom-audiosessionstate-interrupted">Audio Session: 3.2 Audio session states</see>
    /// </remarks>
    [Description("@#interrupted")]
    Interrupted = 2
}

/// <summary>
/// WebIDL enum AudioSessionType。定义于 Audio Session。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/audio-session/#enumdef-audiosessiontype">Audio Session: 3.1 Audio session types</see>
/// </remarks>
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
    /// JavaScript 字符串取值 “playback”；属于 AudioSessionType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/audio-session/#dom-audiosessiontype-playback">Audio Session: 3.1 Audio session types</see>
    /// </remarks>
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
    /// JavaScript 字符串取值 “ambient”；属于 AudioSessionType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/audio-session/#dom-audiosessiontype-ambient">Audio Session: 3.1 Audio session types</see>
    /// </remarks>
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
/// WebIDL enum AutoplayPolicy。定义于 Autoplay Policy Detection。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/autoplay/#enumdef-autoplaypolicy">Autoplay Policy Detection: 2.1 Autoplay Policy Enum</see>
/// </remarks>
[Description("@#AutoplayPolicy")]
[ECMAScript]
[String]
public enum AutoplayPolicy
{
    /// <summary>
    /// JavaScript 字符串取值 “allowed”；属于 AutoplayPolicy 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/autoplay/#dom-autoplaypolicy-allowed">Autoplay Policy Detection: 2.1 Autoplay Policy Enum</see>
    /// </remarks>
    [Description("@#allowed")]
    Allowed = 0,

    /// <summary>
    /// JavaScript 字符串取值 “allowed-muted”；属于 AutoplayPolicy 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/autoplay/#dom-autoplaypolicy-allowed-muted">Autoplay Policy Detection: 2.1 Autoplay Policy Enum</see>
    /// </remarks>
    [Description("@#allowed-muted")]
    AllowedMuted = 1,

    /// <summary>
    /// JavaScript 字符串取值 “disallowed”；属于 AutoplayPolicy 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/autoplay/#dom-autoplaypolicy-disallowed">Autoplay Policy Detection: 2.1 Autoplay Policy Enum</see>
    /// </remarks>
    [Description("@#disallowed")]
    Disallowed = 2
}

/// <summary>
/// WebIDL enum AutoplayPolicyMediaType。定义于 Autoplay Policy Detection。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/autoplay/#enumdef-autoplaypolicymediatype">Autoplay Policy Detection: 2.2 The Autoplay Detection Methods</see>
/// </remarks>
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
/// WebIDL enum PresentationStyle。定义于 Clipboard API and events。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/clipboard-apis/#enumdef-presentationstyle">Clipboard API and events: 7.2 ClipboardItem Interface</see>
/// </remarks>
[Description("@#PresentationStyle")]
[ECMAScript]
[String]
public enum PresentationStyle
{
    /// <summary>
    /// JavaScript 字符串取值 “unspecified”；属于 PresentationStyle 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/clipboard-apis/#dom-presentationstyle-unspecified">Clipboard API and events: 7.2 ClipboardItem Interface</see>
    /// </remarks>
    [Description("@#unspecified")]
    Unspecified = 0,

    /// <summary>
    /// JavaScript 字符串取值 “inline”；属于 PresentationStyle 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/clipboard-apis/#dom-presentationstyle-inline">Clipboard API and events: 7.2 ClipboardItem Interface</see>
    /// </remarks>
    [Description("@#inline")]
    Inline = 1,

    /// <summary>
    /// JavaScript 字符串取值 “attachment”；属于 PresentationStyle 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/clipboard-apis/#dom-presentationstyle-attachment">Clipboard API and events: 7.2 ClipboardItem Interface</see>
    /// </remarks>
    [Description("@#attachment")]
    Attachment = 2
}

/// <summary>
/// WebIDL enum PressureSource。定义于 Compute Pressure Level 1。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/compute-pressure/#dom-pressuresource">Compute Pressure Level 1: 3.2 Pressure sources</see>
/// </remarks>
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
/// WebIDL enum PressureState。定义于 Compute Pressure Level 1。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/compute-pressure/#dom-pressurestate">Compute Pressure Level 1: 8 Pressure States</see>
/// </remarks>
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
/// WebIDL enum ContactProperty。定义于 Contact Picker API。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/contact-picker/#enumdef-contactproperty">Contact Picker API: 6.2 ContactProperty</see>
/// </remarks>
[Description("@#ContactProperty")]
[ECMAScript]
[String]
public enum ContactProperty
{
    /// <summary>
    /// JavaScript 字符串取值 “address”；属于 ContactProperty 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/contact-picker/#dom-contactproperty-address">Contact Picker API: Contact Picker API</see>
    /// </remarks>
    [Description("@#address")]
    Address = 0,

    /// <summary>
    /// JavaScript 字符串取值 “email”；属于 ContactProperty 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/contact-picker/#dom-contactproperty-email">Contact Picker API: Contact Picker API</see>
    /// </remarks>
    [Description("@#email")]
    Email = 1,

    /// <summary>
    /// JavaScript 字符串取值 “icon”；属于 ContactProperty 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/contact-picker/#dom-contactproperty-icon">Contact Picker API: Contact Picker API</see>
    /// </remarks>
    [Description("@#icon")]
    Icon = 2,

    /// <summary>
    /// JavaScript 字符串取值 “name”；属于 ContactProperty 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/contact-picker/#dom-contactproperty-name">Contact Picker API: Contact Picker API</see>
    /// </remarks>
    [Description("@#name")]
    Name = 3,

    /// <summary>
    /// JavaScript 字符串取值 “tel”；属于 ContactProperty 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/contact-picker/#dom-contactproperty-tel">Contact Picker API: Contact Picker API</see>
    /// </remarks>
    [Description("@#tel")]
    Tel = 4
}

/// <summary>
/// WebIDL enum MediaKeySessionClosedReason。定义于 Encrypted Media Extensions。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/encrypted-media/#dom-mediakeysessionclosedreason">Encrypted Media Extensions: 6 MediaKeySession Interface</see>
/// </remarks>
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
    /// JavaScript 字符串取值 “resource-evicted”；属于 MediaKeySessionClosedReason 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/encrypted-media/#dom-mediakeysessionclosedreason-resource-evicted">Encrypted Media Extensions: 6 MediaKeySession Interface</see>
    /// </remarks>
    [Description("@#resource-evicted")]
    ResourceEvicted = 4
}

/// <summary>
/// WebIDL enum MediaKeySessionType。定义于 Encrypted Media Extensions。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/encrypted-media/#dom-mediakeysessiontype">Encrypted Media Extensions: 5 MediaKeys Interface</see>
/// </remarks>
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
/// WebIDL enum MediaKeysRequirement。定义于 Encrypted Media Extensions。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/encrypted-media/#dom-mediakeysrequirement">Encrypted Media Extensions: 3.3 MediaKeySystemConfiguration dictionary</see>
/// </remarks>
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
/// WebIDL enum MediaKeyStatus。定义于 Encrypted Media Extensions。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/encrypted-media/#dom-mediakeystatus">Encrypted Media Extensions: 6.3 MediaKeyStatusMap Interface</see>
/// </remarks>
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
    /// JavaScript 字符串取值 “internal-error”；属于 MediaKeyStatus 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/encrypted-media/#dom-mediakeystatus-internal-error">Encrypted Media Extensions: 6.3 MediaKeyStatusMap Interface</see>
    /// </remarks>
    [Description("@#internal-error")]
    InternalError = 7
}

/// <summary>
/// WebIDL enum GamepadHapticEffectType。定义于 Gamepad。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/gamepad/#dom-gamepadhapticeffecttype">Gamepad: 10. GamepadHapticEffectType Enum</see>
/// </remarks>
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
/// WebIDL enum GamepadHapticsResult。定义于 Gamepad。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/gamepad/#dom-gamepadhapticsresult">Gamepad: 9 GamepadHapticsResult Enum</see>
/// </remarks>
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
/// WebIDL enum GyroscopeLocalCoordinateSystem。定义于 Gyroscope。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/gyroscope/#enumdef-gyroscopelocalcoordinatesystem">Gyroscope: 7.1 The Gyroscope Interface</see>
/// </remarks>
[Description("@#GyroscopeLocalCoordinateSystem")]
[ECMAScript]
[String]
public enum GyroscopeLocalCoordinateSystem
{
    /// <summary>
    /// JavaScript 字符串取值 “device”；属于 GyroscopeLocalCoordinateSystem 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/gyroscope/#dom-gyroscopelocalcoordinatesystem-device">Gyroscope: 7.1 The Gyroscope Interface</see>
    /// </remarks>
    [Description("@#device")]
    Device = 0,

    /// <summary>
    /// JavaScript 字符串取值 “screen”；属于 GyroscopeLocalCoordinateSystem 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/gyroscope/#dom-gyroscopelocalcoordinatesystem-screen">Gyroscope: 7.1 The Gyroscope Interface</see>
    /// </remarks>
    [Description("@#screen")]
    Screen = 1
}

/// <summary>
/// WebIDL enum ScriptInvokerType。定义于 Long Animation Frames API。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/long-animation-frames/#enumdef-scriptinvokertype">Long Animation Frames API: 2.2 PerformanceScriptTiming interface</see>
/// </remarks>
[Description("@#ScriptInvokerType")]
[ECMAScript]
[String]
public enum ScriptInvokerType
{
    /// <summary>
    /// JavaScript 字符串取值 “classic-script”；属于 ScriptInvokerType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/long-animation-frames/#dom-scriptinvokertype-classic-script">Long Animation Frames API: 2.2 PerformanceScriptTiming interface</see>
    /// </remarks>
    [Description("@#classic-script")]
    ClassicScript = 0,

    /// <summary>
    /// JavaScript 字符串取值 “module-script”；属于 ScriptInvokerType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/long-animation-frames/#dom-scriptinvokertype-module-script">Long Animation Frames API: 2.2 PerformanceScriptTiming interface</see>
    /// </remarks>
    [Description("@#module-script")]
    ModuleScript = 1,

    /// <summary>
    /// JavaScript 字符串取值 “event-listener”；属于 ScriptInvokerType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/long-animation-frames/#dom-scriptinvokertype-event-listener">Long Animation Frames API: 2.2 PerformanceScriptTiming interface</see>
    /// </remarks>
    [Description("@#event-listener")]
    EventListener = 2,

    /// <summary>
    /// JavaScript 字符串取值 “user-callback”；属于 ScriptInvokerType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/long-animation-frames/#dom-scriptinvokertype-user-callback">Long Animation Frames API: 2.2 PerformanceScriptTiming interface</see>
    /// </remarks>
    [Description("@#user-callback")]
    UserCallback = 3,

    /// <summary>
    /// JavaScript 字符串取值 “resolve-promise”；属于 ScriptInvokerType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/long-animation-frames/#dom-scriptinvokertype-resolve-promise">Long Animation Frames API: 2.2 PerformanceScriptTiming interface</see>
    /// </remarks>
    [Description("@#resolve-promise")]
    ResolvePromise = 4,

    /// <summary>
    /// JavaScript 字符串取值 “reject-promise”；属于 ScriptInvokerType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/long-animation-frames/#dom-scriptinvokertype-reject-promise">Long Animation Frames API: 2.2 PerformanceScriptTiming interface</see>
    /// </remarks>
    [Description("@#reject-promise")]
    RejectPromise = 5
}

/// <summary>
/// WebIDL enum ScriptWindowAttribution。定义于 Long Animation Frames API。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/long-animation-frames/#enumdef-scriptwindowattribution">Long Animation Frames API: 2.2 PerformanceScriptTiming interface</see>
/// </remarks>
[Description("@#ScriptWindowAttribution")]
[ECMAScript]
[String]
public enum ScriptWindowAttribution
{
    /// <summary>
    /// JavaScript 字符串取值 “self”；属于 ScriptWindowAttribution 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/long-animation-frames/#dom-scriptwindowattribution-self">Long Animation Frames API: 2.2 PerformanceScriptTiming interface</see>
    /// </remarks>
    [Description("@#self")]
    Self = 0,

    /// <summary>
    /// JavaScript 字符串取值 “descendant”；属于 ScriptWindowAttribution 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/long-animation-frames/#dom-scriptwindowattribution-descendant">Long Animation Frames API: 2.2 PerformanceScriptTiming interface</see>
    /// </remarks>
    [Description("@#descendant")]
    Descendant = 1,

    /// <summary>
    /// JavaScript 字符串取值 “ancestor”；属于 ScriptWindowAttribution 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/long-animation-frames/#dom-scriptwindowattribution-ancestor">Long Animation Frames API: 2.2 PerformanceScriptTiming interface</see>
    /// </remarks>
    [Description("@#ancestor")]
    Ancestor = 2,

    /// <summary>
    /// JavaScript 字符串取值 “same-page”；属于 ScriptWindowAttribution 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/long-animation-frames/#dom-scriptwindowattribution-same-page">Long Animation Frames API: 2.2 PerformanceScriptTiming interface</see>
    /// </remarks>
    [Description("@#same-page")]
    SamePage = 3,

    /// <summary>
    /// JavaScript 字符串取值 “other”；属于 ScriptWindowAttribution 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/long-animation-frames/#dom-scriptwindowattribution-other">Long Animation Frames API: 2.2 PerformanceScriptTiming interface</see>
    /// </remarks>
    [Description("@#other")]
    Other = 4
}

/// <summary>
/// WebIDL enum MagnetometerLocalCoordinateSystem。定义于 Magnetometer。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/magnetometer/#enumdef-magnetometerlocalcoordinatesystem">Magnetometer: 6.1 The Magnetometer Interface</see>
/// </remarks>
[Description("@#MagnetometerLocalCoordinateSystem")]
[ECMAScript]
[String]
public enum MagnetometerLocalCoordinateSystem
{
    /// <summary>
    /// JavaScript 字符串取值 “device”；属于 MagnetometerLocalCoordinateSystem 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/magnetometer/#dom-magnetometerlocalcoordinatesystem-device">Magnetometer: 6.1 The Magnetometer Interface</see>
    /// </remarks>
    [Description("@#device")]
    Device = 0,

    /// <summary>
    /// JavaScript 字符串取值 “screen”；属于 MagnetometerLocalCoordinateSystem 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/magnetometer/#dom-magnetometerlocalcoordinatesystem-screen">Magnetometer: 6.1 The Magnetometer Interface</see>
    /// </remarks>
    [Description("@#screen")]
    Screen = 1
}

/// <summary>
/// WebIDL enum ColorGamut。定义于 Media Capabilities。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/media-capabilities/#enumdef-colorgamut">Media Capabilities: 2.1.6 ColorGamut</see>
/// </remarks>
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
    /// JavaScript 字符串取值 “p3”；属于 ColorGamut 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/media-capabilities/#dom-colorgamut-p3">Media Capabilities: 2.1.6 ColorGamut</see>
    /// </remarks>
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
/// WebIDL enum MockCapturePromptResult。定义于 Media Capture Automation。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/mediacapture-automation/#dom-mockcapturepromptresult">Media Capture Automation: 3.1.1 Set capture prompt result</see>
/// </remarks>
[Description("@#MockCapturePromptResult")]
[ECMAScript]
[String]
public enum MockCapturePromptResult
{
    /// <summary>
    /// JavaScript 字符串取值 “granted”；属于 MockCapturePromptResult 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-automation/#dom-mockcapturepromptresult-granted">Media Capture Automation: 3.1.1 Set capture prompt result</see>
    /// </remarks>
    [Description("@#granted")]
    Granted = 0,

    /// <summary>
    /// JavaScript 字符串取值 “denied”；属于 MockCapturePromptResult 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-automation/#dom-mockcapturepromptresult-denied">Media Capture Automation: 3.1.1 Set capture prompt result</see>
    /// </remarks>
    [Description("@#denied")]
    Denied = 1
}

/// <summary>
/// WebIDL enum CaptureAction。定义于 The Capture-Handle Actions Mechanism。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/mediacapture-handle/actions/#dom-captureaction">The Capture-Handle Actions Mechanism: 3.1.1 Registering and responding to capture actions</see>
/// </remarks>
[Description("@#CaptureAction")]
[ECMAScript]
[String]
public enum CaptureAction
{
    /// <summary>
    /// JavaScript 字符串取值 “next”；属于 CaptureAction 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-handle/actions/#dom-captureaction-next">The Capture-Handle Actions Mechanism: 3.1.1 Registering and responding to capture actions</see>
    /// </remarks>
    [Description("@#next")]
    Next = 0,

    /// <summary>
    /// JavaScript 字符串取值 “previous”；属于 CaptureAction 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-handle/actions/#dom-captureaction-previous">The Capture-Handle Actions Mechanism: 3.1.1 Registering and responding to capture actions</see>
    /// </remarks>
    [Description("@#previous")]
    Previous = 1,

    /// <summary>
    /// JavaScript 字符串取值 “first”；属于 CaptureAction 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-handle/actions/#dom-captureaction-first">The Capture-Handle Actions Mechanism: 3.1.1 Registering and responding to capture actions</see>
    /// </remarks>
    [Description("@#first")]
    First = 2,

    /// <summary>
    /// JavaScript 字符串取值 “last”；属于 CaptureAction 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-handle/actions/#dom-captureaction-last">The Capture-Handle Actions Mechanism: 3.1.1 Registering and responding to capture actions</see>
    /// </remarks>
    [Description("@#last")]
    Last = 3
}

/// <summary>
/// WebIDL enum FillLightMode。定义于 MediaStream Image Capture。
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
    /// JavaScript 字符串取值 “auto”；属于 FillLightMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-image/#dom-filllightmode-auto">MediaStream Image Capture: 8.1 Values</see>
    /// </remarks>
    [Description("@#auto")]
    Auto = 0,

    /// <summary>
    /// JavaScript 字符串取值 “off”；属于 FillLightMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-image/#dom-filllightmode-off">MediaStream Image Capture: 8.1 Values</see>
    /// </remarks>
    [Description("@#off")]
    Off = 1,

    /// <summary>
    /// JavaScript 字符串取值 “flash”；属于 FillLightMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-image/#dom-filllightmode-flash">MediaStream Image Capture: 8.1 Values</see>
    /// </remarks>
    [Description("@#flash")]
    Flash = 2
}

/// <summary>
/// WebIDL enum MeteringMode。定义于 MediaStream Image Capture。
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
    /// JavaScript 字符串取值 “none”；属于 MeteringMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-image/#dom-meteringmode-none">MediaStream Image Capture: 11.1 Values</see>
    /// </remarks>
    [Description("@#none")]
    None = 0,

    /// <summary>
    /// JavaScript 字符串取值 “manual”；属于 MeteringMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-image/#dom-meteringmode-manual">MediaStream Image Capture: 11.1 Values</see>
    /// </remarks>
    [Description("@#manual")]
    Manual = 1,

    /// <summary>
    /// JavaScript 字符串取值 “single-shot”；属于 MeteringMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-image/#dom-meteringmode-single-shot">MediaStream Image Capture: 11.1 Values</see>
    /// </remarks>
    [Description("@#single-shot")]
    SingleShot = 2,

    /// <summary>
    /// JavaScript 字符串取值 “continuous”；属于 MeteringMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-image/#dom-meteringmode-continuous">MediaStream Image Capture: 11.1 Values</see>
    /// </remarks>
    [Description("@#continuous")]
    Continuous = 3
}

/// <summary>
/// WebIDL enum RedEyeReduction。定义于 MediaStream Image Capture。
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
    /// JavaScript 字符串取值 “never”；属于 RedEyeReduction 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-image/#dom-redeyereduction-never">MediaStream Image Capture: 7.1 Values</see>
    /// </remarks>
    [Description("@#never")]
    Never = 0,

    /// <summary>
    /// JavaScript 字符串取值 “always”；属于 RedEyeReduction 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-image/#dom-redeyereduction-always">MediaStream Image Capture: 7.1 Values</see>
    /// </remarks>
    [Description("@#always")]
    Always = 1,

    /// <summary>
    /// JavaScript 字符串取值 “controllable”；属于 RedEyeReduction 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-image/#dom-redeyereduction-controllable">MediaStream Image Capture: 7.1 Values</see>
    /// </remarks>
    [Description("@#controllable")]
    Controllable = 2
}

/// <summary>
/// WebIDL enum MediaDeviceKind。定义于 Media Capture and Streams。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/mediacapture-main/#dom-mediadevicekind">Media Capture and Streams: 9.3 Device Info</see>
/// </remarks>
[Description("@#MediaDeviceKind")]
[ECMAScript]
[String]
public enum MediaDeviceKind
{
    /// <summary>
    /// JavaScript 字符串取值 “audioinput”；属于 MediaDeviceKind 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-main/#idl-def-MediaDeviceKind.audioinput">Media Capture and Streams: 9.3 Device Info</see>
    /// </remarks>
    [Description("@#audioinput")]
    Audioinput = 0,

    /// <summary>
    /// JavaScript 字符串取值 “audiooutput”；属于 MediaDeviceKind 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-main/#idl-def-MediaDeviceKind.audiooutput">Media Capture and Streams: 9.3 Device Info</see>
    /// </remarks>
    [Description("@#audiooutput")]
    Audiooutput = 1,

    /// <summary>
    /// JavaScript 字符串取值 “videoinput”；属于 MediaDeviceKind 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-main/#idl-def-MediaDeviceKind.videoinput">Media Capture and Streams: 9.3 Device Info</see>
    /// </remarks>
    [Description("@#videoinput")]
    Videoinput = 2
}

/// <summary>
/// WebIDL enum VideoFacingModeEnum。定义于 Media Capture and Streams。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/mediacapture-main/#dom-videofacingmodeenum">Media Capture and Streams: 4.3.8 Constrainable Properties</see>
/// </remarks>
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
    /// JavaScript 字符串取值 “environment”；属于 VideoFacingModeEnum 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-main/#dom-videofacingmodeenum-environment">Media Capture and Streams: 4.3.8 Constrainable Properties</see>
    /// </remarks>
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
    /// JavaScript 字符串取值 “right”；属于 VideoFacingModeEnum 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-main/#dom-videofacingmodeenum-right">Media Capture and Streams: 4.3.8 Constrainable Properties</see>
    /// </remarks>
    [Description("@#right")]
    Right = 3
}

/// <summary>
/// WebIDL enum VideoResizeModeEnum。定义于 Media Capture and Streams。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/mediacapture-main/#dom-videoresizemodeenum">Media Capture and Streams: 4.3.8 Constrainable Properties</see>
/// </remarks>
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
/// WebIDL enum RecordingState。定义于 MediaStream Recording。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/mediacapture-record/#enumdef-recordingstate">MediaStream Recording: 2.7 RecordingState</see>
/// </remarks>
[Description("@#RecordingState")]
[ECMAScript]
[String]
public enum RecordingState
{
    /// <summary>
    /// JavaScript 字符串取值 “inactive”；属于 RecordingState 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-record/#dom-recordingstate-inactive">MediaStream Recording: 2.7.1 Values</see>
    /// </remarks>
    [Description("@#inactive")]
    Inactive = 0,

    /// <summary>
    /// JavaScript 字符串取值 “recording”；属于 RecordingState 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-record/#dom-recordingstate-recording">MediaStream Recording: 2.7.1 Values</see>
    /// </remarks>
    [Description("@#recording")]
    Recording = 1,

    /// <summary>
    /// JavaScript 字符串取值 “paused”；属于 RecordingState 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-record/#dom-recordingstate-paused">MediaStream Recording: 2.7.1 Values</see>
    /// </remarks>
    [Description("@#paused")]
    Paused = 2
}

/// <summary>
/// WebIDL enum CaptureStartFocusBehavior。定义于 Screen Capture。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/mediacapture-screen-share/#dom-capturestartfocusbehavior">Screen Capture: 5.4.2 CaptureStartFocusBehavior</see>
/// </remarks>
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
    /// JavaScript 字符串取值 “no-focus-change”；属于 CaptureStartFocusBehavior 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-screen-share/#idl-def-CaptureStartFocusBehavior.no-focus-change">Screen Capture: 5.4.2 CaptureStartFocusBehavior</see>
    /// </remarks>
    [Description("@#no-focus-change")]
    NoFocusChange = 2
}

/// <summary>
/// The setActionHandler() method of the MediaSession interface sets a handler for a media session action. These actions let a web app receive notifications when the user engages a device&apos;s built-in physical or onscreen media controls, such as play, stop, or seek buttons.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/MediaSession/setActionHandler">MDN Web Docs: MediaSessionAction</see>
/// </remarks>
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
    /// JavaScript 字符串取值 “stop”；属于 MediaSessionAction 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediasession/#dom-mediasessionaction-stop">Media Session: 4.4 Actions</see>
    /// </remarks>
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
/// WebIDL enum MediaSessionEnterPictureInPictureReason。定义于 Media Session。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/mediasession/#enumdef-mediasessionenterpictureinpicturereason">Media Session: 5 The MediaSession interface</see>
/// </remarks>
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
/// WebIDL enum MediaSessionPlaybackState。定义于 Media Session。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/mediasession/#enumdef-mediasessionplaybackstate">Media Session: 5 The MediaSession interface</see>
/// </remarks>
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
/// WebIDL enum NavigationTimingType。定义于 Navigation Timing Level 2。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/navigation-timing/#enumdef-navigationtimingtype">Navigation Timing Level 2: 3.3.1 The NavigationTimingType enum</see>
/// </remarks>
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
    /// JavaScript 字符串取值 “back_forward”；属于 NavigationTimingType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/navigation-timing/#dom-navigationtimingtype-back_forward">Navigation Timing Level 2: 3.3.1 The NavigationTimingType enum</see>
    /// </remarks>
    [Description("@#back_forward")]
    BackForward = 2
}

/// <summary>
/// WebIDL enum OrientationSensorLocalCoordinateSystem。定义于 Orientation Sensor。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/orientation-sensor/#enumdef-orientationsensorlocalcoordinatesystem">Orientation Sensor: 6.1 The OrientationSensor Interface</see>
/// </remarks>
[Description("@#OrientationSensorLocalCoordinateSystem")]
[ECMAScript]
[String]
public enum OrientationSensorLocalCoordinateSystem
{
    /// <summary>
    /// JavaScript 字符串取值 “device”；属于 OrientationSensorLocalCoordinateSystem 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/orientation-sensor/#dom-orientationsensorlocalcoordinatesystem-device">Orientation Sensor: 6.1 The OrientationSensor Interface</see>
    /// </remarks>
    [Description("@#device")]
    Device = 0,

    /// <summary>
    /// JavaScript 字符串取值 “screen”；属于 OrientationSensorLocalCoordinateSystem 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/orientation-sensor/#dom-orientationsensorlocalcoordinatesystem-screen">Orientation Sensor: 6.1 The OrientationSensor Interface</see>
    /// </remarks>
    [Description("@#screen")]
    Screen = 1
}

/// <summary>
/// WebIDL enum PaymentComplete。定义于 Payment Request API。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/payment-request/#dom-paymentcomplete">Payment Request API: 12. PaymentComplete enum</see>
/// </remarks>
[Description("@#PaymentComplete")]
[ECMAScript]
[String]
public enum PaymentComplete
{
    /// <summary>
    /// JavaScript 字符串取值 “fail”；属于 PaymentComplete 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/payment-request/#dom-paymentcomplete-fail">Payment Request API: 12. PaymentComplete enum</see>
    /// </remarks>
    [Description("@#fail")]
    Fail = 0,

    /// <summary>
    /// JavaScript 字符串取值 “success”；属于 PaymentComplete 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/payment-request/#dom-paymentcomplete-success">Payment Request API: 12. PaymentComplete enum</see>
    /// </remarks>
    [Description("@#success")]
    Success = 1,

    /// <summary>
    /// JavaScript 字符串取值 “unknown”；属于 PaymentComplete 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/payment-request/#dom-paymentcomplete-unknown">Payment Request API: 12. PaymentComplete enum</see>
    /// </remarks>
    [Description("@#unknown")]
    Unknown = 2
}

/// <summary>
/// WebIDL enum PresentationConnectionCloseReason。定义于 Presentation API。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/presentation-api/#dom-presentationconnectionclosereason">Presentation API: 6.5.4 Interface PresentationConnectionCloseEvent</see>
/// </remarks>
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
/// WebIDL enum PresentationConnectionState。定义于 Presentation API。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/presentation-api/#dom-presentationconnectionstate">Presentation API: 6.5 Interface PresentationConnection</see>
/// </remarks>
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
/// WebIDL enum SecurePaymentConfirmationAvailability。定义于 Secure Payment Confirmation。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/secure-payment-confirmation/#enumdef-securepaymentconfirmationavailability">Secure Payment Confirmation: 4.6 Checking if Secure Payment Confirmation is available</see>
/// </remarks>
[Description("@#SecurePaymentConfirmationAvailability")]
[ECMAScript]
[String]
public enum SecurePaymentConfirmationAvailability
{
    /// <summary>
    /// JavaScript 字符串取值 “available”；属于 SecurePaymentConfirmationAvailability 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/secure-payment-confirmation/#dom-securepaymentconfirmationavailability-available">Secure Payment Confirmation: 4.6 Checking if Secure Payment Confirmation is available</see>
    /// </remarks>
    [Description("@#available")]
    Available = 0,

    /// <summary>
    /// JavaScript 字符串取值 “unavailable-unknown-reason”；属于 SecurePaymentConfirmationAvailability 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/secure-payment-confirmation/#dom-securepaymentconfirmationavailability-unavailable-unknown-reason">Secure Payment Confirmation: 4.6 Checking if Secure Payment Confirmation is available</see>
    /// </remarks>
    [Description("@#unavailable-unknown-reason")]
    UnavailableUnknownReason = 1,

    /// <summary>
    /// JavaScript 字符串取值 “unavailable-feature-not-enabled”；属于 SecurePaymentConfirmationAvailability 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/secure-payment-confirmation/#dom-securepaymentconfirmationavailability-unavailable-feature-not-enabled">Secure Payment Confirmation: 4.6 Checking if Secure Payment Confirmation is available</see>
    /// </remarks>
    [Description("@#unavailable-feature-not-enabled")]
    UnavailableFeatureNotEnabled = 2,

    /// <summary>
    /// JavaScript 字符串取值 “unavailable-no-permission-policy”；属于 SecurePaymentConfirmationAvailability 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/secure-payment-confirmation/#dom-securepaymentconfirmationavailability-unavailable-no-permission-policy">Secure Payment Confirmation: 4.6 Checking if Secure Payment Confirmation is available</see>
    /// </remarks>
    [Description("@#unavailable-no-permission-policy")]
    UnavailableNoPermissionPolicy = 3,

    /// <summary>
    /// JavaScript 字符串取值 “unavailable-no-user-verifying-platform-authenticator”；属于 SecurePaymentConfirmationAvailability 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/secure-payment-confirmation/#dom-securepaymentconfirmationavailability-unavailable-no-user-verifying-platform-authenticator">Secure Payment Confirmation: 4.6 Checking if Secure Payment Confirmation is available</see>
    /// </remarks>
    [Description("@#unavailable-no-user-verifying-platform-authenticator")]
    UnavailableNoUserVerifyingPlatformAuthenticator = 4
}

/// <summary>
/// WebIDL enum SecurePaymentConfirmationCapability。定义于 Secure Payment Confirmation。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/secure-payment-confirmation/#enumdef-securepaymentconfirmationcapability">Secure Payment Confirmation: 4.7.1 SecurePaymentConfirmationCapability Enumeration</see>
/// </remarks>
[Description("@#SecurePaymentConfirmationCapability")]
[ECMAScript]
[String]
public enum SecurePaymentConfirmationCapability
{
    /// <summary>
    /// JavaScript 字符串取值 “browserBoundKeyHardware”；属于 SecurePaymentConfirmationCapability 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/secure-payment-confirmation/#dom-securepaymentconfirmationcapability-browserboundkeyhardware">Secure Payment Confirmation: 4.7.1 SecurePaymentConfirmationCapability Enumeration</see>
    /// </remarks>
    [Description("@#browserBoundKeyHardware")]
    BrowserBoundKeyHardware = 0
}

/// <summary>
/// WebIDL enum LockMode。定义于 Web Locks API。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/web-locks/#enumdef-lockmode">Web Locks API: 3.2 LockManager class</see>
/// </remarks>
[Description("@#LockMode")]
[ECMAScript]
[String]
public enum LockMode
{
    /// <summary>
    /// JavaScript 字符串取值 “shared”；属于 LockMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/web-locks/#dom-lockmode-shared">Web Locks API: 3.2 LockManager class</see>
    /// </remarks>
    [Description("@#shared")]
    Shared = 0,

    /// <summary>
    /// JavaScript 字符串取值 “exclusive”；属于 LockMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/web-locks/#dom-lockmode-exclusive">Web Locks API: 3.2 LockManager class</see>
    /// </remarks>
    [Description("@#exclusive")]
    Exclusive = 1
}

/// <summary>
/// WebIDL enum CredentialMediationRequirement。定义于 Credential Management Level 1。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webappsec-credential-management/#enumdef-credentialmediationrequirement">Credential Management Level 1: 2.3.2 Mediation Requirements</see>
/// </remarks>
[Description("@#CredentialMediationRequirement")]
[ECMAScript]
[String]
public enum CredentialMediationRequirement
{
    /// <summary>
    /// JavaScript 字符串取值 “silent”；属于 CredentialMediationRequirement 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webappsec-credential-management/#dom-credentialmediationrequirement-silent">Credential Management Level 1: 2.3.2 Mediation Requirements</see>
    /// </remarks>
    [Description("@#silent")]
    Silent = 0,

    /// <summary>
    /// JavaScript 字符串取值 “optional”；属于 CredentialMediationRequirement 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webappsec-credential-management/#dom-credentialmediationrequirement-optional">Credential Management Level 1: 2.3.2 Mediation Requirements</see>
    /// </remarks>
    [Description("@#optional")]
    Optional = 1,

    /// <summary>
    /// JavaScript 字符串取值 “conditional”；属于 CredentialMediationRequirement 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webappsec-credential-management/#dom-credentialmediationrequirement-conditional">Credential Management Level 1: 2.3.2 Mediation Requirements</see>
    /// </remarks>
    [Description("@#conditional")]
    Conditional = 2,

    /// <summary>
    /// JavaScript 字符串取值 “required”；属于 CredentialMediationRequirement 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webappsec-credential-management/#dom-credentialmediationrequirement-required">Credential Management Level 1: 2.3.2 Mediation Requirements</see>
    /// </remarks>
    [Description("@#required")]
    Required = 3
}

/// <summary>
/// WebIDL enum CredentialUiMode。定义于 Credential Management Level 1。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webappsec-credential-management/#enumdef-credentialuimode">Credential Management Level 1: 2.3.3 UI Mode</see>
/// </remarks>
[Description("@#CredentialUiMode")]
[ECMAScript]
[String]
public enum CredentialUiMode
{
    /// <summary>
    /// JavaScript 字符串取值 “immediate”；属于 CredentialUiMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webappsec-credential-management/#dom-credentialuimode-immediate">Credential Management Level 1: 2.3.3 UI Mode</see>
    /// </remarks>
    [Description("@#immediate")]
    Immediate = 0
}

/// <summary>
/// WebIDL enum SecurityPolicyViolationEventDisposition。定义于 Content Security Policy Level 3。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webappsec-csp/#enumdef-securitypolicyviolationeventdisposition">Content Security Policy Level 3: 5.1 Violation DOM Events</see>
/// </remarks>
[Description("@#SecurityPolicyViolationEventDisposition")]
[ECMAScript]
[String]
public enum SecurityPolicyViolationEventDisposition
{
    /// <summary>
    /// JavaScript 字符串取值 “enforce”；属于 SecurityPolicyViolationEventDisposition 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webappsec-csp/#dom-securitypolicyviolationeventdisposition-enforce">Content Security Policy Level 3: 5.1 Violation DOM Events</see>
    /// </remarks>
    [Description("@#enforce")]
    Enforce = 0,

    /// <summary>
    /// JavaScript 字符串取值 “report”；属于 SecurityPolicyViolationEventDisposition 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webappsec-csp/#dom-securitypolicyviolationeventdisposition-report">Content Security Policy Level 3: 5.1 Violation DOM Events</see>
    /// </remarks>
    [Description("@#report")]
    Report = 1
}

/// <summary>
/// WebIDL enum ReferrerPolicy。定义于 Referrer Policy。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webappsec-referrer-policy/#enumdef-referrerpolicy">Referrer Policy: 3 Referrer Policies</see>
/// </remarks>
[Description("@#ReferrerPolicy")]
[ECMAScript]
[String]
public enum ReferrerPolicy
{
    /// <summary>
    /// JavaScript 字符串取值 “”；属于 ReferrerPolicy 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webappsec-referrer-policy/">Referrer Policy: ReferrerPolicy.</see>
    /// </remarks>
[Description("@#")]
    Empty = 0,

    /// <summary>
    /// JavaScript 字符串取值 “no-referrer”；属于 ReferrerPolicy 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webappsec-referrer-policy/#dom-referrerpolicy-no-referrer">Referrer Policy: 3 Referrer Policies</see>
    /// </remarks>
    [Description("@#no-referrer")]
    NoReferrer = 1,

    /// <summary>
    /// JavaScript 字符串取值 “no-referrer-when-downgrade”；属于 ReferrerPolicy 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webappsec-referrer-policy/#dom-referrerpolicy-no-referrer-when-downgrade">Referrer Policy: 3 Referrer Policies</see>
    /// </remarks>
    [Description("@#no-referrer-when-downgrade")]
    NoReferrerWhenDowngrade = 2,

    /// <summary>
    /// JavaScript 字符串取值 “same-origin”；属于 ReferrerPolicy 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webappsec-referrer-policy/#dom-referrerpolicy-same-origin">Referrer Policy: 3 Referrer Policies</see>
    /// </remarks>
    [Description("@#same-origin")]
    SameOrigin = 3,

    /// <summary>
    /// JavaScript 字符串取值 “origin”；属于 ReferrerPolicy 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webappsec-referrer-policy/#dom-referrerpolicy-origin">Referrer Policy: 3 Referrer Policies</see>
    /// </remarks>
    [Description("@#origin")]
    Origin = 4,

    /// <summary>
    /// JavaScript 字符串取值 “strict-origin”；属于 ReferrerPolicy 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webappsec-referrer-policy/#dom-referrerpolicy-strict-origin">Referrer Policy: 3 Referrer Policies</see>
    /// </remarks>
    [Description("@#strict-origin")]
    StrictOrigin = 5,

    /// <summary>
    /// JavaScript 字符串取值 “origin-when-cross-origin”；属于 ReferrerPolicy 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webappsec-referrer-policy/#dom-referrerpolicy-origin-when-cross-origin">Referrer Policy: 3 Referrer Policies</see>
    /// </remarks>
    [Description("@#origin-when-cross-origin")]
    OriginWhenCrossOrigin = 6,

    /// <summary>
    /// JavaScript 字符串取值 “strict-origin-when-cross-origin”；属于 ReferrerPolicy 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webappsec-referrer-policy/#dom-referrerpolicy-strict-origin-when-cross-origin">Referrer Policy: 3 Referrer Policies</see>
    /// </remarks>
    [Description("@#strict-origin-when-cross-origin")]
    StrictOriginWhenCrossOrigin = 7,

    /// <summary>
    /// JavaScript 字符串取值 “unsafe-url”；属于 ReferrerPolicy 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webappsec-referrer-policy/#dom-referrerpolicy-unsafe-url">Referrer Policy: 3 Referrer Policies</see>
    /// </remarks>
    [Description("@#unsafe-url")]
    UnsafeUrl = 8
}

/// <summary>
/// WebIDL enum AttestationConveyancePreference。定义于 Web Authentication: An API for accessing Public Key Credentials - Level 3。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webauthn/#enumdef-attestationconveyancepreference">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.4.7 Attestation Conveyance Preference Enumeration (enum AttestationConveyancePreference)</see>
/// </remarks>
[Description("@#AttestationConveyancePreference")]
[ECMAScript]
[String]
public enum AttestationConveyancePreference
{
    /// <summary>
    /// JavaScript 字符串取值 “none”；属于 AttestationConveyancePreference 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webauthn/#dom-attestationconveyancepreference-none">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.4.7 Attestation Conveyance Preference Enumeration (enum AttestationConveyancePreference)</see>
    /// </remarks>
    [Description("@#none")]
    None = 0,

    /// <summary>
    /// JavaScript 字符串取值 “indirect”；属于 AttestationConveyancePreference 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webauthn/#dom-attestationconveyancepreference-indirect">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.4.7 Attestation Conveyance Preference Enumeration (enum AttestationConveyancePreference)</see>
    /// </remarks>
    [Description("@#indirect")]
    Indirect = 1,

    /// <summary>
    /// JavaScript 字符串取值 “direct”；属于 AttestationConveyancePreference 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webauthn/#dom-attestationconveyancepreference-direct">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.4.7 Attestation Conveyance Preference Enumeration (enum AttestationConveyancePreference)</see>
    /// </remarks>
    [Description("@#direct")]
    Direct = 2,

    /// <summary>
    /// JavaScript 字符串取值 “enterprise”；属于 AttestationConveyancePreference 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webauthn/#dom-attestationconveyancepreference-enterprise">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.4.7 Attestation Conveyance Preference Enumeration (enum AttestationConveyancePreference)</see>
    /// </remarks>
    [Description("@#enterprise")]
    Enterprise = 3
}

/// <summary>
/// WebIDL enum AuthenticatorAttachment。定义于 Web Authentication: An API for accessing Public Key Credentials - Level 3。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webauthn/#enumdef-authenticatorattachment">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.4.5 Authenticator Attachment Enumeration (enum AuthenticatorAttachment)</see>
/// </remarks>
[Description("@#AuthenticatorAttachment")]
[ECMAScript]
[String]
public enum AuthenticatorAttachment
{
    /// <summary>
    /// JavaScript 字符串取值 “platform”；属于 AuthenticatorAttachment 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webauthn/#dom-authenticatorattachment-platform">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.4.5 Authenticator Attachment Enumeration (enum AuthenticatorAttachment)</see>
    /// </remarks>
    [Description("@#platform")]
    Platform = 0,

    /// <summary>
    /// JavaScript 字符串取值 “cross-platform”；属于 AuthenticatorAttachment 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webauthn/#dom-authenticatorattachment-cross-platform">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.4.5 Authenticator Attachment Enumeration (enum AuthenticatorAttachment)</see>
    /// </remarks>
    [Description("@#cross-platform")]
    CrossPlatform = 1
}

/// <summary>
/// WebIDL enum AuthenticatorTransport。定义于 Web Authentication: An API for accessing Public Key Credentials - Level 3。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webauthn/#enumdef-authenticatortransport">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.8.4 Authenticator Transport Enumeration (enum AuthenticatorTransport)</see>
/// </remarks>
[Description("@#AuthenticatorTransport")]
[ECMAScript]
[String]
public enum AuthenticatorTransport
{
    /// <summary>
    /// JavaScript 字符串取值 “usb”；属于 AuthenticatorTransport 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webauthn/#dom-authenticatortransport-usb">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.8.4 Authenticator Transport Enumeration (enum AuthenticatorTransport)</see>
    /// </remarks>
    [Description("@#usb")]
    Usb = 0,

    /// <summary>
    /// JavaScript 字符串取值 “nfc”；属于 AuthenticatorTransport 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webauthn/#dom-authenticatortransport-nfc">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.8.4 Authenticator Transport Enumeration (enum AuthenticatorTransport)</see>
    /// </remarks>
    [Description("@#nfc")]
    Nfc = 1,

    /// <summary>
    /// JavaScript 字符串取值 “ble”；属于 AuthenticatorTransport 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webauthn/#dom-authenticatortransport-ble">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.8.4 Authenticator Transport Enumeration (enum AuthenticatorTransport)</see>
    /// </remarks>
    [Description("@#ble")]
    Ble = 2,

    /// <summary>
    /// JavaScript 字符串取值 “smart-card”；属于 AuthenticatorTransport 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webauthn/#dom-authenticatortransport-smart-card">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.8.4 Authenticator Transport Enumeration (enum AuthenticatorTransport)</see>
    /// </remarks>
    [Description("@#smart-card")]
    SmartCard = 3,

    /// <summary>
    /// JavaScript 字符串取值 “hybrid”；属于 AuthenticatorTransport 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webauthn/#dom-authenticatortransport-hybrid">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.8.4 Authenticator Transport Enumeration (enum AuthenticatorTransport)</see>
    /// </remarks>
    [Description("@#hybrid")]
    Hybrid = 4,

    /// <summary>
    /// JavaScript 字符串取值 “internal”；属于 AuthenticatorTransport 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webauthn/#dom-authenticatortransport-internal">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.8.4 Authenticator Transport Enumeration (enum AuthenticatorTransport)</see>
    /// </remarks>
    [Description("@#internal")]
    Internal = 5
}

/// <summary>
/// WebIDL enum ClientCapability。定义于 Web Authentication: An API for accessing Public Key Credentials - Level 3。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webauthn/#enumdef-clientcapability">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.8.7 Client Capability Enumeration (enum ClientCapability)</see>
/// </remarks>
[Description("@#ClientCapability")]
[ECMAScript]
[String]
public enum ClientCapability
{
    /// <summary>
    /// JavaScript 字符串取值 “conditionalCreate”；属于 ClientCapability 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webauthn/#dom-clientcapability-conditionalcreate">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.8.7 Client Capability Enumeration (enum ClientCapability)</see>
    /// </remarks>
    [Description("@#conditionalCreate")]
    ConditionalCreate = 0,

    /// <summary>
    /// JavaScript 字符串取值 “conditionalGet”；属于 ClientCapability 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webauthn/#dom-clientcapability-conditionalget">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.8.7 Client Capability Enumeration (enum ClientCapability)</see>
    /// </remarks>
    [Description("@#conditionalGet")]
    ConditionalGet = 1,

    /// <summary>
    /// JavaScript 字符串取值 “hybridTransport”；属于 ClientCapability 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webauthn/#dom-clientcapability-hybridtransport">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.8.7 Client Capability Enumeration (enum ClientCapability)</see>
    /// </remarks>
    [Description("@#hybridTransport")]
    HybridTransport = 2,

    /// <summary>
    /// JavaScript 字符串取值 “passkeyPlatformAuthenticator”；属于 ClientCapability 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webauthn/#dom-clientcapability-passkeyplatformauthenticator">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.8.7 Client Capability Enumeration (enum ClientCapability)</see>
    /// </remarks>
    [Description("@#passkeyPlatformAuthenticator")]
    PasskeyPlatformAuthenticator = 3,

    /// <summary>
    /// JavaScript 字符串取值 “userVerifyingPlatformAuthenticator”；属于 ClientCapability 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webauthn/#dom-clientcapability-userverifyingplatformauthenticator">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.8.7 Client Capability Enumeration (enum ClientCapability)</see>
    /// </remarks>
    [Description("@#userVerifyingPlatformAuthenticator")]
    UserVerifyingPlatformAuthenticator = 4,

    /// <summary>
    /// JavaScript 字符串取值 “relatedOrigins”；属于 ClientCapability 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webauthn/#dom-clientcapability-relatedorigins">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.8.7 Client Capability Enumeration (enum ClientCapability)</see>
    /// </remarks>
    [Description("@#relatedOrigins")]
    RelatedOrigins = 5,

    /// <summary>
    /// JavaScript 字符串取值 “signalAllAcceptedCredentials”；属于 ClientCapability 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webauthn/#dom-clientcapability-signalallacceptedcredentials">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.8.7 Client Capability Enumeration (enum ClientCapability)</see>
    /// </remarks>
    [Description("@#signalAllAcceptedCredentials")]
    SignalAllAcceptedCredentials = 6,

    /// <summary>
    /// JavaScript 字符串取值 “signalCurrentUserDetails”；属于 ClientCapability 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webauthn/#dom-clientcapability-signalcurrentuserdetails">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.8.7 Client Capability Enumeration (enum ClientCapability)</see>
    /// </remarks>
    [Description("@#signalCurrentUserDetails")]
    SignalCurrentUserDetails = 7,

    /// <summary>
    /// JavaScript 字符串取值 “signalUnknownCredential”；属于 ClientCapability 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webauthn/#dom-clientcapability-signalunknowncredential">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.8.7 Client Capability Enumeration (enum ClientCapability)</see>
    /// </remarks>
    [Description("@#signalUnknownCredential")]
    SignalUnknownCredential = 8
}

/// <summary>
/// WebIDL enum LargeBlobSupport。定义于 Web Authentication: An API for accessing Public Key Credentials - Level 3。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webauthn/#enumdef-largeblobsupport">Web Authentication: An API for accessing Public Key Credentials - Level 3: 10.1.5 Large blob storage extension (largeBlob)</see>
/// </remarks>
[Description("@#LargeBlobSupport")]
[ECMAScript]
[String]
public enum LargeBlobSupport
{
    /// <summary>
    /// JavaScript 字符串取值 “required”；属于 LargeBlobSupport 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webauthn/#dom-largeblobsupport-required">Web Authentication: An API for accessing Public Key Credentials - Level 3: 10.1.5 Large blob storage extension (largeBlob)</see>
    /// </remarks>
    [Description("@#required")]
    Required = 0,

    /// <summary>
    /// JavaScript 字符串取值 “preferred”；属于 LargeBlobSupport 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webauthn/#dom-largeblobsupport-preferred">Web Authentication: An API for accessing Public Key Credentials - Level 3: 10.1.5 Large blob storage extension (largeBlob)</see>
    /// </remarks>
    [Description("@#preferred")]
    Preferred = 1
}

/// <summary>
/// WebIDL enum PublicKeyCredentialHint。定义于 Web Authentication: An API for accessing Public Key Credentials - Level 3。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webauthn/#enumdef-publickeycredentialhint">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.8.8 User-agent Hints Enumeration (enum PublicKeyCredentialHint)</see>
/// </remarks>
[Description("@#PublicKeyCredentialHint")]
[ECMAScript]
[String]
public enum PublicKeyCredentialHint
{
    /// <summary>
    /// JavaScript 字符串取值 “security-key”；属于 PublicKeyCredentialHint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webauthn/#dom-publickeycredentialhint-security-key">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.8.8 User-agent Hints Enumeration (enum PublicKeyCredentialHint)</see>
    /// </remarks>
    [Description("@#security-key")]
    SecurityKey = 0,

    /// <summary>
    /// JavaScript 字符串取值 “client-device”；属于 PublicKeyCredentialHint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webauthn/#dom-publickeycredentialhint-client-device">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.8.8 User-agent Hints Enumeration (enum PublicKeyCredentialHint)</see>
    /// </remarks>
    [Description("@#client-device")]
    ClientDevice = 1,

    /// <summary>
    /// JavaScript 字符串取值 “hybrid”；属于 PublicKeyCredentialHint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webauthn/#dom-publickeycredentialhint-hybrid">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.8.8 User-agent Hints Enumeration (enum PublicKeyCredentialHint)</see>
    /// </remarks>
    [Description("@#hybrid")]
    Hybrid = 2
}

/// <summary>
/// WebIDL enum PublicKeyCredentialType。定义于 Web Authentication: An API for accessing Public Key Credentials - Level 3。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webauthn/#enumdef-publickeycredentialtype">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.8.2 Credential Type Enumeration (enum PublicKeyCredentialType)</see>
/// </remarks>
[Description("@#PublicKeyCredentialType")]
[ECMAScript]
[String]
public enum PublicKeyCredentialType
{
    /// <summary>
    /// JavaScript 字符串取值 “public-key”；属于 PublicKeyCredentialType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webauthn/#dom-publickeycredentialtype-public-key">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.8.2 Credential Type Enumeration (enum PublicKeyCredentialType)</see>
    /// </remarks>
    [Description("@#public-key")]
    PublicKey = 0
}

/// <summary>
/// WebIDL enum ResidentKeyRequirement。定义于 Web Authentication: An API for accessing Public Key Credentials - Level 3。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webauthn/#enumdef-residentkeyrequirement">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.4.6 Resident Key Requirement Enumeration (enum ResidentKeyRequirement)</see>
/// </remarks>
[Description("@#ResidentKeyRequirement")]
[ECMAScript]
[String]
public enum ResidentKeyRequirement
{
    /// <summary>
    /// JavaScript 字符串取值 “discouraged”；属于 ResidentKeyRequirement 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webauthn/#dom-residentkeyrequirement-discouraged">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.4.6 Resident Key Requirement Enumeration (enum ResidentKeyRequirement)</see>
    /// </remarks>
    [Description("@#discouraged")]
    Discouraged = 0,

    /// <summary>
    /// JavaScript 字符串取值 “preferred”；属于 ResidentKeyRequirement 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webauthn/#dom-residentkeyrequirement-preferred">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.4.6 Resident Key Requirement Enumeration (enum ResidentKeyRequirement)</see>
    /// </remarks>
    [Description("@#preferred")]
    Preferred = 1,

    /// <summary>
    /// JavaScript 字符串取值 “required”；属于 ResidentKeyRequirement 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webauthn/#dom-residentkeyrequirement-required">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.4.6 Resident Key Requirement Enumeration (enum ResidentKeyRequirement)</see>
    /// </remarks>
    [Description("@#required")]
    Required = 2
}

/// <summary>
/// WebIDL enum TokenBindingStatus。定义于 Web Authentication: An API for accessing Public Key Credentials - Level 3。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webauthn/#enumdef-tokenbindingstatus">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.8.1 Client Data Used in WebAuthn Signatures (dictionary CollectedClientData)</see>
/// </remarks>
[Description("@#TokenBindingStatus")]
[ECMAScript]
[String]
public enum TokenBindingStatus
{
    /// <summary>
    /// JavaScript 字符串取值 “present”；属于 TokenBindingStatus 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webauthn/#dom-tokenbindingstatus-present">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.8.1 Client Data Used in WebAuthn Signatures (dictionary CollectedClientData)</see>
    /// </remarks>
    [Description("@#present")]
    Present = 0,

    /// <summary>
    /// JavaScript 字符串取值 “supported”；属于 TokenBindingStatus 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webauthn/#dom-tokenbindingstatus-supported">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.8.1 Client Data Used in WebAuthn Signatures (dictionary CollectedClientData)</see>
    /// </remarks>
    [Description("@#supported")]
    Supported = 1
}

/// <summary>
/// WebIDL enum UserVerificationRequirement。定义于 Web Authentication: An API for accessing Public Key Credentials - Level 3。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webauthn/#enumdef-userverificationrequirement">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.8.6 User Verification Requirement Enumeration (enum UserVerificationRequirement)</see>
/// </remarks>
[Description("@#UserVerificationRequirement")]
[ECMAScript]
[String]
public enum UserVerificationRequirement
{
    /// <summary>
    /// JavaScript 字符串取值 “required”；属于 UserVerificationRequirement 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webauthn/#dom-userverificationrequirement-required">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.8.6 User Verification Requirement Enumeration (enum UserVerificationRequirement)</see>
    /// </remarks>
    [Description("@#required")]
    Required = 0,

    /// <summary>
    /// JavaScript 字符串取值 “preferred”；属于 UserVerificationRequirement 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webauthn/#dom-userverificationrequirement-preferred">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.8.6 User Verification Requirement Enumeration (enum UserVerificationRequirement)</see>
    /// </remarks>
    [Description("@#preferred")]
    Preferred = 1,

    /// <summary>
    /// JavaScript 字符串取值 “discouraged”；属于 UserVerificationRequirement 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webauthn/#dom-userverificationrequirement-discouraged">Web Authentication: An API for accessing Public Key Credentials - Level 3: 5.8.6 User Verification Requirement Enumeration (enum UserVerificationRequirement)</see>
    /// </remarks>
    [Description("@#discouraged")]
    Discouraged = 2
}

/// <summary>
/// WebIDL enum AlphaOption。定义于 WebCodecs。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webcodecs/#enumdef-alphaoption">WebCodecs: 7.10 Alpha Option</see>
/// </remarks>
[Description("@#AlphaOption")]
[ECMAScript]
[String]
public enum AlphaOption
{
    /// <summary>
    /// JavaScript 字符串取值 “keep”；属于 AlphaOption 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-alphaoption-keep">WebCodecs: 7.10 Alpha Option</see>
    /// </remarks>
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
/// WebIDL enum AudioSampleFormat。定义于 WebCodecs。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webcodecs/#enumdef-audiosampleformat">WebCodecs: 9.3 Audio Sample Format</see>
/// </remarks>
[Description("@#AudioSampleFormat")]
[ECMAScript]
[String]
public enum AudioSampleFormat
{
    /// <summary>
    /// JavaScript 字符串取值 “u8”；属于 AudioSampleFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-audiosampleformat-u8">WebCodecs: 9.3 Audio Sample Format</see>
    /// </remarks>
    [Description("@#u8")]
    U8 = 0,

    /// <summary>
    /// JavaScript 字符串取值 “s16”；属于 AudioSampleFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-audiosampleformat-s16">WebCodecs: 9.3 Audio Sample Format</see>
    /// </remarks>
    [Description("@#s16")]
    S16 = 1,

    /// <summary>
    /// JavaScript 字符串取值 “s32”；属于 AudioSampleFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-audiosampleformat-s32">WebCodecs: 9.3 Audio Sample Format</see>
    /// </remarks>
    [Description("@#s32")]
    S32 = 2,

    /// <summary>
    /// JavaScript 字符串取值 “f32”；属于 AudioSampleFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-audiosampleformat-f32">WebCodecs: 9.3 Audio Sample Format</see>
    /// </remarks>
    [Description("@#f32")]
    F32 = 3,

    /// <summary>
    /// JavaScript 字符串取值 “u8-planar”；属于 AudioSampleFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-audiosampleformat-u8-planar">WebCodecs: 9.3 Audio Sample Format</see>
    /// </remarks>
    [Description("@#u8-planar")]
    U8Planar = 4,

    /// <summary>
    /// JavaScript 字符串取值 “s16-planar”；属于 AudioSampleFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-audiosampleformat-s16-planar">WebCodecs: 9.3 Audio Sample Format</see>
    /// </remarks>
    [Description("@#s16-planar")]
    S16Planar = 5,

    /// <summary>
    /// JavaScript 字符串取值 “s32-planar”；属于 AudioSampleFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-audiosampleformat-s32-planar">WebCodecs: 9.3 Audio Sample Format</see>
    /// </remarks>
    [Description("@#s32-planar")]
    S32Planar = 6,

    /// <summary>
    /// JavaScript 字符串取值 “f32-planar”；属于 AudioSampleFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-audiosampleformat-f32-planar">WebCodecs: 9.3 Audio Sample Format</see>
    /// </remarks>
    [Description("@#f32-planar")]
    F32Planar = 7
}

/// <summary>
/// WebIDL enum CodecState。定义于 WebCodecs。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webcodecs/#enumdef-codecstate">WebCodecs: 7.15 CodecState</see>
/// </remarks>
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
    /// JavaScript 字符串取值 “configured”；属于 CodecState 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-codecstate-configured">WebCodecs: 7.15 CodecState</see>
    /// </remarks>
    [Description("@#configured")]
    Configured = 1,

    /// <summary>
    /// JavaScript 字符串取值 “closed”；属于 CodecState 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-codecstate-closed">WebCodecs: 7.15 CodecState</see>
    /// </remarks>
    [Description("@#closed")]
    Closed = 2
}

/// <summary>
/// WebIDL enum VideoColorPrimaries。定义于 WebCodecs。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webcodecs/#enumdef-videocolorprimaries">WebCodecs: 9.10 Video Color Primaries</see>
/// </remarks>
[Description("@#VideoColorPrimaries")]
[ECMAScript]
[String]
public enum VideoColorPrimaries
{
    /// <summary>
    /// JavaScript 字符串取值 “bt709”；属于 VideoColorPrimaries 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videocolorprimaries-bt709">WebCodecs: 9.10 Video Color Primaries</see>
    /// </remarks>
    [Description("@#bt709")]
    Bt709 = 0,

    /// <summary>
    /// JavaScript 字符串取值 “bt470bg”；属于 VideoColorPrimaries 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videocolorprimaries-bt470bg">WebCodecs: 9.10 Video Color Primaries</see>
    /// </remarks>
    [Description("@#bt470bg")]
    Bt470bg = 1,

    /// <summary>
    /// JavaScript 字符串取值 “smpte170m”；属于 VideoColorPrimaries 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videocolorprimaries-smpte170m">WebCodecs: 9.10 Video Color Primaries</see>
    /// </remarks>
    [Description("@#smpte170m")]
    Smpte170m = 2,

    /// <summary>
    /// JavaScript 字符串取值 “bt2020”；属于 VideoColorPrimaries 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videocolorprimaries-bt2020">WebCodecs: 9.10 Video Color Primaries</see>
    /// </remarks>
    [Description("@#bt2020")]
    Bt2020 = 3,

    /// <summary>
    /// JavaScript 字符串取值 “smpte432”；属于 VideoColorPrimaries 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videocolorprimaries-smpte432">WebCodecs: 9.10 Video Color Primaries</see>
    /// </remarks>
    [Description("@#smpte432")]
    Smpte432 = 4
}

/// <summary>
/// WebIDL enum VideoMatrixCoefficients。定义于 WebCodecs。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webcodecs/#enumdef-videomatrixcoefficients">WebCodecs: 9.12 Video Matrix Coefficients</see>
/// </remarks>
[Description("@#VideoMatrixCoefficients")]
[ECMAScript]
[String]
public enum VideoMatrixCoefficients
{
    /// <summary>
    /// JavaScript 字符串取值 “rgb”；属于 VideoMatrixCoefficients 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videomatrixcoefficients-rgb">WebCodecs: 9.12 Video Matrix Coefficients</see>
    /// </remarks>
    [Description("@#rgb")]
    Rgb = 0,

    /// <summary>
    /// JavaScript 字符串取值 “bt709”；属于 VideoMatrixCoefficients 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videomatrixcoefficients-bt709">WebCodecs: 9.12 Video Matrix Coefficients</see>
    /// </remarks>
    [Description("@#bt709")]
    Bt709 = 1,

    /// <summary>
    /// JavaScript 字符串取值 “bt470bg”；属于 VideoMatrixCoefficients 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videomatrixcoefficients-bt470bg">WebCodecs: 9.12 Video Matrix Coefficients</see>
    /// </remarks>
    [Description("@#bt470bg")]
    Bt470bg = 2,

    /// <summary>
    /// JavaScript 字符串取值 “smpte170m”；属于 VideoMatrixCoefficients 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videomatrixcoefficients-smpte170m">WebCodecs: 9.12 Video Matrix Coefficients</see>
    /// </remarks>
    [Description("@#smpte170m")]
    Smpte170m = 3,

    /// <summary>
    /// JavaScript 字符串取值 “bt2020-ncl”；属于 VideoMatrixCoefficients 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videomatrixcoefficients-bt2020-ncl">WebCodecs: 9.12 Video Matrix Coefficients</see>
    /// </remarks>
    [Description("@#bt2020-ncl")]
    Bt2020Ncl = 4
}

/// <summary>
/// WebIDL enum VideoPixelFormat。定义于 WebCodecs。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webcodecs/#enumdef-videopixelformat">WebCodecs: 9.8 Pixel Format</see>
/// </remarks>
[Description("@#VideoPixelFormat")]
[ECMAScript]
[String]
public enum VideoPixelFormat
{
    /// <summary>
    /// JavaScript 字符串取值 “I420”；属于 VideoPixelFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videopixelformat-i420">WebCodecs: 9.8 Pixel Format</see>
    /// </remarks>
    [Description("@#I420")]
    I420 = 0,

    /// <summary>
    /// JavaScript 字符串取值 “I420P10”；属于 VideoPixelFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videopixelformat-i420p10">WebCodecs: 9.8 Pixel Format</see>
    /// </remarks>
    [Description("@#I420P10")]
    I420P10 = 1,

    /// <summary>
    /// JavaScript 字符串取值 “I420P12”；属于 VideoPixelFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videopixelformat-i420p12">WebCodecs: 9.8 Pixel Format</see>
    /// </remarks>
    [Description("@#I420P12")]
    I420P12 = 2,

    /// <summary>
    /// JavaScript 字符串取值 “I420A”；属于 VideoPixelFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videopixelformat-i420a">WebCodecs: 9.8 Pixel Format</see>
    /// </remarks>
    [Description("@#I420A")]
    I420A = 3,

    /// <summary>
    /// JavaScript 字符串取值 “I420AP10”；属于 VideoPixelFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videopixelformat-i420ap10">WebCodecs: 9.8 Pixel Format</see>
    /// </remarks>
    [Description("@#I420AP10")]
    I420AP10 = 4,

    /// <summary>
    /// JavaScript 字符串取值 “I420AP12”；属于 VideoPixelFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videopixelformat-i420ap12">WebCodecs: 9.8 Pixel Format</see>
    /// </remarks>
    [Description("@#I420AP12")]
    I420AP12 = 5,

    /// <summary>
    /// JavaScript 字符串取值 “I422”；属于 VideoPixelFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videopixelformat-i422">WebCodecs: 9.8 Pixel Format</see>
    /// </remarks>
    [Description("@#I422")]
    I422 = 6,

    /// <summary>
    /// JavaScript 字符串取值 “I422P10”；属于 VideoPixelFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videopixelformat-i422p10">WebCodecs: 9.8 Pixel Format</see>
    /// </remarks>
    [Description("@#I422P10")]
    I422P10 = 7,

    /// <summary>
    /// JavaScript 字符串取值 “I422P12”；属于 VideoPixelFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videopixelformat-i422p12">WebCodecs: 9.8 Pixel Format</see>
    /// </remarks>
    [Description("@#I422P12")]
    I422P12 = 8,

    /// <summary>
    /// JavaScript 字符串取值 “I422A”；属于 VideoPixelFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videopixelformat-i422a">WebCodecs: 9.8 Pixel Format</see>
    /// </remarks>
    [Description("@#I422A")]
    I422A = 9,

    /// <summary>
    /// JavaScript 字符串取值 “I422AP10”；属于 VideoPixelFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videopixelformat-i422ap10">WebCodecs: 9.8 Pixel Format</see>
    /// </remarks>
    [Description("@#I422AP10")]
    I422AP10 = 10,

    /// <summary>
    /// JavaScript 字符串取值 “I422AP12”；属于 VideoPixelFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videopixelformat-i422ap12">WebCodecs: 9.8 Pixel Format</see>
    /// </remarks>
    [Description("@#I422AP12")]
    I422AP12 = 11,

    /// <summary>
    /// JavaScript 字符串取值 “I444”；属于 VideoPixelFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videopixelformat-i444">WebCodecs: 9.8 Pixel Format</see>
    /// </remarks>
    [Description("@#I444")]
    I444 = 12,

    /// <summary>
    /// JavaScript 字符串取值 “I444P10”；属于 VideoPixelFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videopixelformat-i444p10">WebCodecs: 9.8 Pixel Format</see>
    /// </remarks>
    [Description("@#I444P10")]
    I444P10 = 13,

    /// <summary>
    /// JavaScript 字符串取值 “I444P12”；属于 VideoPixelFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videopixelformat-i444p12">WebCodecs: 9.8 Pixel Format</see>
    /// </remarks>
    [Description("@#I444P12")]
    I444P12 = 14,

    /// <summary>
    /// JavaScript 字符串取值 “I444A”；属于 VideoPixelFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videopixelformat-i444a">WebCodecs: 9.8 Pixel Format</see>
    /// </remarks>
    [Description("@#I444A")]
    I444A = 15,

    /// <summary>
    /// JavaScript 字符串取值 “I444AP10”；属于 VideoPixelFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videopixelformat-i444ap10">WebCodecs: 9.8 Pixel Format</see>
    /// </remarks>
    [Description("@#I444AP10")]
    I444AP10 = 16,

    /// <summary>
    /// JavaScript 字符串取值 “I444AP12”；属于 VideoPixelFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videopixelformat-i444ap12">WebCodecs: 9.8 Pixel Format</see>
    /// </remarks>
    [Description("@#I444AP12")]
    I444AP12 = 17,

    /// <summary>
    /// JavaScript 字符串取值 “NV12”；属于 VideoPixelFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videopixelformat-nv12">WebCodecs: 9.8 Pixel Format</see>
    /// </remarks>
    [Description("@#NV12")]
    NV12 = 18,

    /// <summary>
    /// JavaScript 字符串取值 “RGBA”；属于 VideoPixelFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videopixelformat-rgba">WebCodecs: 9.8 Pixel Format</see>
    /// </remarks>
    [Description("@#RGBA")]
    RGBA = 19,

    /// <summary>
    /// JavaScript 字符串取值 “RGBX”；属于 VideoPixelFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videopixelformat-rgbx">WebCodecs: 9.8 Pixel Format</see>
    /// </remarks>
    [Description("@#RGBX")]
    RGBX = 20,

    /// <summary>
    /// JavaScript 字符串取值 “BGRA”；属于 VideoPixelFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videopixelformat-bgra">WebCodecs: 9.8 Pixel Format</see>
    /// </remarks>
    [Description("@#BGRA")]
    BGRA = 21,

    /// <summary>
    /// JavaScript 字符串取值 “BGRX”；属于 VideoPixelFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videopixelformat-bgrx">WebCodecs: 9.8 Pixel Format</see>
    /// </remarks>
    [Description("@#BGRX")]
    BGRX = 22
}

/// <summary>
/// WebIDL enum VideoTransferCharacteristics。定义于 WebCodecs。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webcodecs/#enumdef-videotransfercharacteristics">WebCodecs: 9.11 Video Transfer Characteristics</see>
/// </remarks>
[Description("@#VideoTransferCharacteristics")]
[ECMAScript]
[String]
public enum VideoTransferCharacteristics
{
    /// <summary>
    /// JavaScript 字符串取值 “bt709”；属于 VideoTransferCharacteristics 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videotransfercharacteristics-bt709">WebCodecs: 9.11 Video Transfer Characteristics</see>
    /// </remarks>
    [Description("@#bt709")]
    Bt709 = 0,

    /// <summary>
    /// JavaScript 字符串取值 “smpte170m”；属于 VideoTransferCharacteristics 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videotransfercharacteristics-smpte170m">WebCodecs: 9.11 Video Transfer Characteristics</see>
    /// </remarks>
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
    /// JavaScript 字符串取值 “linear”；属于 VideoTransferCharacteristics 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videotransfercharacteristics-linear">WebCodecs: 9.11 Video Transfer Characteristics</see>
    /// </remarks>
    [Description("@#linear")]
    Linear = 3,

    /// <summary>
    /// JavaScript 字符串取值 “pq”；属于 VideoTransferCharacteristics 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videotransfercharacteristics-pq">WebCodecs: 9.11 Video Transfer Characteristics</see>
    /// </remarks>
    [Description("@#pq")]
    Pq = 4,

    /// <summary>
    /// JavaScript 字符串取值 “hlg”；属于 VideoTransferCharacteristics 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videotransfercharacteristics-hlg">WebCodecs: 9.11 Video Transfer Characteristics</see>
    /// </remarks>
    [Description("@#hlg")]
    Hlg = 5
}

/// <summary>
/// WebIDL enum KeyType。定义于 Web Cryptography API Level 2。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webcrypto/#dom-keytype">Web Cryptography API Level 2: 13.2 Key interface data types</see>
/// </remarks>
[Description("@#KeyType")]
[ECMAScript]
[String]
public enum KeyType
{
    /// <summary>
    /// JavaScript 字符串取值 “public”；属于 KeyType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcrypto/#dom-keytype-public">Web Cryptography API Level 2: 13.2 Key interface data types</see>
    /// </remarks>
    [Description("@#public")]
    Public = 0,

    /// <summary>
    /// JavaScript 字符串取值 “private”；属于 KeyType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcrypto/#dom-keytype-private">Web Cryptography API Level 2: 13.2 Key interface data types</see>
    /// </remarks>
    [Description("@#private")]
    Private = 1,

    /// <summary>
    /// JavaScript 字符串取值 “secret”；属于 KeyType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcrypto/#dom-keytype-secret">Web Cryptography API Level 2: 13.2 Key interface data types</see>
    /// </remarks>
    [Description("@#secret")]
    Secret = 2
}

/// <summary>
/// WebIDL enum ExecutionWorld。定义于 Web Extensions。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webextensions/specification/#enumdef-executionworld">Web Extensions: 13.2.13 ExecutionWorld enum</see>
/// </remarks>
[Description("@#ExecutionWorld")]
[ECMAScript]
[String]
public enum ExecutionWorld
{
    /// <summary>
    /// JavaScript 字符串取值 “ISOLATED”；属于 ExecutionWorld 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webextensions/specification/#dom-executionworld-isolated">Web Extensions: 13.2.13 ExecutionWorld enum</see>
    /// </remarks>
    [Description("@#ISOLATED")]
    ISOLATED = 0,

    /// <summary>
    /// JavaScript 字符串取值 “MAIN”；属于 ExecutionWorld 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webextensions/specification/#dom-executionworld-main">Web Extensions: 13.2.13 ExecutionWorld enum</see>
    /// </remarks>
    [Description("@#MAIN")]
    MAIN = 1
}

/// <summary>
/// WebIDL enum RunAt。定义于 Web Extensions。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webextensions/specification/#enumdef-runat">Web Extensions: 13.2.12 RunAt enum</see>
/// </remarks>
[Description("@#RunAt")]
[ECMAScript]
[String]
public enum RunAt
{
    /// <summary>
    /// JavaScript 字符串取值 “document_start”；属于 RunAt 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webextensions/specification/#dom-runat-document_start">Web Extensions: 13.2.12 RunAt enum</see>
    /// </remarks>
    [Description("@#document_start")]
    DocumentStart = 0,

    /// <summary>
    /// JavaScript 字符串取值 “document_end”；属于 RunAt 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webextensions/specification/#dom-runat-document_end">Web Extensions: 13.2.12 RunAt enum</see>
    /// </remarks>
    [Description("@#document_end")]
    DocumentEnd = 1,

    /// <summary>
    /// JavaScript 字符串取值 “document_idle”；属于 RunAt 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webextensions/specification/#dom-runat-document_idle">Web Extensions: 13.2.12 RunAt enum</see>
    /// </remarks>
    [Description("@#document_idle")]
    DocumentIdle = 2
}

/// <summary>
/// WebIDL enum RTCRtpScriptTransformType。定义于 WebRTC Encoded Transform。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webrtc-encoded-transform/#enumdef-rtcrtpscripttransformtype">WebRTC Encoded Transform: 5 RTCRtpScriptTransform interface</see>
/// </remarks>
[Description("@#RTCRtpScriptTransformType")]
[ECMAScript]
[String]
public enum RTCRtpScriptTransformType
{
    /// <summary>
    /// JavaScript 字符串取值 “sframe”；属于 RTCRtpScriptTransformType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-encoded-transform/#dom-rtcrtpscripttransformtype-sframe">WebRTC Encoded Transform: 5 RTCRtpScriptTransform interface</see>
    /// </remarks>
    [Description("@#sframe")]
    Sframe = 0
}

/// <summary>
/// WebIDL enum SFrameCipherSuite。定义于 WebRTC Encoded Transform。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webrtc-encoded-transform/#enumdef-sframeciphersuite">WebRTC Encoded Transform: 3 SFrame transforms</see>
/// </remarks>
[Description("@#SFrameCipherSuite")]
[ECMAScript]
[String]
public enum SFrameCipherSuite
{
    /// <summary>
    /// JavaScript 字符串取值 “AES_128_CTR_HMAC_SHA256_80”；属于 SFrameCipherSuite 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-encoded-transform/#dom-sframeciphersuite-aes_128_ctr_hmac_sha256_80">WebRTC Encoded Transform: 3 SFrame transforms</see>
    /// </remarks>
    [Description("@#AES_128_CTR_HMAC_SHA256_80")]
    AES128CTRHMACSHA25680 = 0,

    /// <summary>
    /// JavaScript 字符串取值 “AES_128_CTR_HMAC_SHA256_64”；属于 SFrameCipherSuite 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-encoded-transform/#dom-sframeciphersuite-aes_128_ctr_hmac_sha256_64">WebRTC Encoded Transform: 3 SFrame transforms</see>
    /// </remarks>
    [Description("@#AES_128_CTR_HMAC_SHA256_64")]
    AES128CTRHMACSHA25664 = 1,

    /// <summary>
    /// JavaScript 字符串取值 “AES_128_CTR_HMAC_SHA256_32”；属于 SFrameCipherSuite 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-encoded-transform/#dom-sframeciphersuite-aes_128_ctr_hmac_sha256_32">WebRTC Encoded Transform: 3 SFrame transforms</see>
    /// </remarks>
    [Description("@#AES_128_CTR_HMAC_SHA256_32")]
    AES128CTRHMACSHA25632 = 2,

    /// <summary>
    /// JavaScript 字符串取值 “AES_128_GCM_SHA256_128”；属于 SFrameCipherSuite 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-encoded-transform/#dom-sframeciphersuite-aes_128_gcm_sha256_128">WebRTC Encoded Transform: 3 SFrame transforms</see>
    /// </remarks>
    [Description("@#AES_128_GCM_SHA256_128")]
    AES128GCMSHA256128 = 3,

    /// <summary>
    /// JavaScript 字符串取值 “AES_256_GCM_SHA512_128”；属于 SFrameCipherSuite 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-encoded-transform/#dom-sframeciphersuite-aes_256_gcm_sha512_128">WebRTC Encoded Transform: 3 SFrame transforms</see>
    /// </remarks>
    [Description("@#AES_256_GCM_SHA512_128")]
    AES256GCMSHA512128 = 4,

    /// <summary>
    /// JavaScript 字符串取值 “AES_256_CTR_HMAC_SHA512_80”；属于 SFrameCipherSuite 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-encoded-transform/#dom-sframeciphersuite-aes_256_ctr_hmac_sha512_80">WebRTC Encoded Transform: 3 SFrame transforms</see>
    /// </remarks>
    [Description("@#AES_256_CTR_HMAC_SHA512_80")]
    AES256CTRHMACSHA51280 = 5,

    /// <summary>
    /// JavaScript 字符串取值 “AES_256_CTR_HMAC_SHA512_64”；属于 SFrameCipherSuite 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-encoded-transform/#dom-sframeciphersuite-aes_256_ctr_hmac_sha512_64">WebRTC Encoded Transform: 3 SFrame transforms</see>
    /// </remarks>
    [Description("@#AES_256_CTR_HMAC_SHA512_64")]
    AES256CTRHMACSHA51264 = 6,

    /// <summary>
    /// JavaScript 字符串取值 “AES_256_CTR_HMAC_SHA512_32”；属于 SFrameCipherSuite 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-encoded-transform/#dom-sframeciphersuite-aes_256_ctr_hmac_sha512_32">WebRTC Encoded Transform: 3 SFrame transforms</see>
    /// </remarks>
    [Description("@#AES_256_CTR_HMAC_SHA512_32")]
    AES256CTRHMACSHA51232 = 7
}

/// <summary>
/// WebIDL enum SFrameTransformErrorEventType。定义于 WebRTC Encoded Transform。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webrtc-encoded-transform/#enumdef-sframetransformerroreventtype">WebRTC Encoded Transform: 3 SFrame transforms</see>
/// </remarks>
[Description("@#SFrameTransformErrorEventType")]
[ECMAScript]
[String]
public enum SFrameTransformErrorEventType
{
    /// <summary>
    /// JavaScript 字符串取值 “authentication”；属于 SFrameTransformErrorEventType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-encoded-transform/#dom-sframetransformerroreventtype-authentication">WebRTC Encoded Transform: 3 SFrame transforms</see>
    /// </remarks>
    [Description("@#authentication")]
    Authentication = 0,

    /// <summary>
    /// JavaScript 字符串取值 “keyID”；属于 SFrameTransformErrorEventType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-encoded-transform/#dom-sframetransformerroreventtype-keyid">WebRTC Encoded Transform: 3 SFrame transforms</see>
    /// </remarks>
    [Description("@#keyID")]
    KeyID = 1,

    /// <summary>
    /// JavaScript 字符串取值 “syntax”；属于 SFrameTransformErrorEventType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-encoded-transform/#dom-sframetransformerroreventtype-syntax">WebRTC Encoded Transform: 3 SFrame transforms</see>
    /// </remarks>
    [Description("@#syntax")]
    Syntax = 2
}

/// <summary>
/// WebIDL enum SFrameType。定义于 WebRTC Encoded Transform。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webrtc-encoded-transform/#enumdef-sframetype">WebRTC Encoded Transform: 3 SFrame transforms</see>
/// </remarks>
[Description("@#SFrameType")]
[ECMAScript]
[String]
public enum SFrameType
{
    /// <summary>
    /// JavaScript 字符串取值 “per-frame”；属于 SFrameType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-encoded-transform/#dom-sframetype-per-frame">WebRTC Encoded Transform: 3 SFrame transforms</see>
    /// </remarks>
    [Description("@#per-frame")]
    PerFrame = 0,

    /// <summary>
    /// JavaScript 字符串取值 “per-packet”；属于 SFrameType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-encoded-transform/#dom-sframetype-per-packet">WebRTC Encoded Transform: 3 SFrame transforms</see>
    /// </remarks>
    [Description("@#per-packet")]
    PerPacket = 1
}

/// <summary>
/// WebIDL enum RTCErrorDetailTypeIdp。定义于 Identity for WebRTC 1.0。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webrtc-identity/#dom-rtcerrordetailtypeidp">Identity for WebRTC 1.0: 10.3 RTCErrorDetailTypeIdp Enum</see>
/// </remarks>
[Description("@#RTCErrorDetailTypeIdp")]
[ECMAScript]
[String]
public enum RTCErrorDetailTypeIdp
{
    /// <summary>
    /// JavaScript 字符串取值 “idp-bad-script-failure”；属于 RTCErrorDetailTypeIdp 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-identity/#dom-rtcerrordetailtypeidp-idp-bad-script-failure">Identity for WebRTC 1.0: 10.3 RTCErrorDetailTypeIdp Enum</see>
    /// </remarks>
    [Description("@#idp-bad-script-failure")]
    IdpBadScriptFailure = 0,

    /// <summary>
    /// JavaScript 字符串取值 “idp-execution-failure”；属于 RTCErrorDetailTypeIdp 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-identity/#dom-rtcerrordetailtypeidp-idp-execution-failure">Identity for WebRTC 1.0: 10.3 RTCErrorDetailTypeIdp Enum</see>
    /// </remarks>
    [Description("@#idp-execution-failure")]
    IdpExecutionFailure = 1,

    /// <summary>
    /// JavaScript 字符串取值 “idp-load-failure”；属于 RTCErrorDetailTypeIdp 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-identity/#dom-rtcerrordetailtypeidp-idp-load-failure">Identity for WebRTC 1.0: 10.3 RTCErrorDetailTypeIdp Enum</see>
    /// </remarks>
    [Description("@#idp-load-failure")]
    IdpLoadFailure = 2,

    /// <summary>
    /// JavaScript 字符串取值 “idp-need-login”；属于 RTCErrorDetailTypeIdp 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-identity/#dom-rtcerrordetailtypeidp-idp-need-login">Identity for WebRTC 1.0: 10.3 RTCErrorDetailTypeIdp Enum</see>
    /// </remarks>
    [Description("@#idp-need-login")]
    IdpNeedLogin = 3,

    /// <summary>
    /// JavaScript 字符串取值 “idp-timeout”；属于 RTCErrorDetailTypeIdp 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-identity/#dom-rtcerrordetailtypeidp-idp-timeout">Identity for WebRTC 1.0: 10.3 RTCErrorDetailTypeIdp Enum</see>
    /// </remarks>
    [Description("@#idp-timeout")]
    IdpTimeout = 4,

    /// <summary>
    /// JavaScript 字符串取值 “idp-tls-failure”；属于 RTCErrorDetailTypeIdp 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-identity/#dom-rtcerrordetailtypeidp-idp-tls-failure">Identity for WebRTC 1.0: 10.3 RTCErrorDetailTypeIdp Enum</see>
    /// </remarks>
    [Description("@#idp-tls-failure")]
    IdpTlsFailure = 5,

    /// <summary>
    /// JavaScript 字符串取值 “idp-token-expired”；属于 RTCErrorDetailTypeIdp 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-identity/#dom-rtcerrordetailtypeidp-idp-token-expired">Identity for WebRTC 1.0: 10.3 RTCErrorDetailTypeIdp Enum</see>
    /// </remarks>
    [Description("@#idp-token-expired")]
    IdpTokenExpired = 6,

    /// <summary>
    /// JavaScript 字符串取值 “idp-token-invalid”；属于 RTCErrorDetailTypeIdp 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-identity/#dom-rtcerrordetailtypeidp-idp-token-invalid">Identity for WebRTC 1.0: 10.3 RTCErrorDetailTypeIdp Enum</see>
    /// </remarks>
    [Description("@#idp-token-invalid")]
    IdpTokenInvalid = 7
}

/// <summary>
/// WebIDL enum RTCPriorityType。定义于 WebRTC Priority Control API。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webrtc-priority/#enumdef-rtcprioritytype">WebRTC Priority Control API: 3.1 RTCPriorityType Enum</see>
/// </remarks>
[Description("@#RTCPriorityType")]
[ECMAScript]
[String]
public enum RTCPriorityType
{
    /// <summary>
    /// JavaScript 字符串取值 “very-low”；属于 RTCPriorityType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-priority/#dom-rtcprioritytype-very-low">WebRTC Priority Control API: 3.1 RTCPriorityType Enum</see>
    /// </remarks>
    [Description("@#very-low")]
    VeryLow = 0,

    /// <summary>
    /// JavaScript 字符串取值 “low”；属于 RTCPriorityType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-priority/#dom-rtcprioritytype-low">WebRTC Priority Control API: 3.1 RTCPriorityType Enum</see>
    /// </remarks>
    [Description("@#low")]
    Low = 1,

    /// <summary>
    /// JavaScript 字符串取值 “medium”；属于 RTCPriorityType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-priority/#dom-rtcprioritytype-medium">WebRTC Priority Control API: 3.1 RTCPriorityType Enum</see>
    /// </remarks>
    [Description("@#medium")]
    Medium = 2,

    /// <summary>
    /// JavaScript 字符串取值 “high”；属于 RTCPriorityType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-priority/#dom-rtcprioritytype-high">WebRTC Priority Control API: 3.1 RTCPriorityType Enum</see>
    /// </remarks>
    [Description("@#high")]
    High = 3
}

/// <summary>
/// WebIDL enum WebTransportCongestionControl。定义于 WebTransport。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webtransport/#enumdef-webtransportcongestioncontrol">WebTransport: 6.9 Configuration</see>
/// </remarks>
[Description("@#WebTransportCongestionControl")]
[ECMAScript]
[String]
public enum WebTransportCongestionControl
{
    /// <summary>
    /// JavaScript 字符串取值 “default”；属于 WebTransportCongestionControl 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webtransport/#dom-webtransportcongestioncontrol-default">WebTransport: 6.9 Configuration</see>
    /// </remarks>
    [Description("@#default")]
    Default = 0,

    /// <summary>
    /// JavaScript 字符串取值 “throughput”；属于 WebTransportCongestionControl 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webtransport/#dom-webtransportcongestioncontrol-throughput">WebTransport: 6.9 Configuration</see>
    /// </remarks>
    [Description("@#throughput")]
    Throughput = 1,

    /// <summary>
    /// JavaScript 字符串取值 “low-latency”；属于 WebTransportCongestionControl 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webtransport/#dom-webtransportcongestioncontrol-low-latency">WebTransport: 6.9 Configuration</see>
    /// </remarks>
    [Description("@#low-latency")]
    LowLatency = 2
}

/// <summary>
/// WebIDL enum WebTransportErrorSource。定义于 WebTransport。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webtransport/#enumdef-webtransporterrorsource">WebTransport: 12. WebTransportError Interface</see>
/// </remarks>
[Description("@#WebTransportErrorSource")]
[ECMAScript]
[String]
public enum WebTransportErrorSource
{
    /// <summary>
    /// JavaScript 字符串取值 “stream”；属于 WebTransportErrorSource 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webtransport/#dom-webtransporterrorsource-stream">WebTransport: 12. WebTransportError Interface</see>
    /// </remarks>
    [Description("@#stream")]
    Stream = 0,

    /// <summary>
    /// JavaScript 字符串取值 “session”；属于 WebTransportErrorSource 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webtransport/#dom-webtransporterrorsource-session">WebTransport: 12. WebTransportError Interface</see>
    /// </remarks>
    [Description("@#session")]
    Session = 1
}

/// <summary>
/// WebIDL enum WebTransportReliabilityMode。定义于 WebTransport。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webtransport/#enumdef-webtransportreliabilitymode">WebTransport: 6 WebTransport Interface</see>
/// </remarks>
[Description("@#WebTransportReliabilityMode")]
[ECMAScript]
[String]
public enum WebTransportReliabilityMode
{
    /// <summary>
    /// JavaScript 字符串取值 “pending”；属于 WebTransportReliabilityMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webtransport/#dom-webtransportreliabilitymode-pending">WebTransport: 6 WebTransport Interface</see>
    /// </remarks>
    [Description("@#pending")]
    Pending = 0,

    /// <summary>
    /// JavaScript 字符串取值 “reliable-only”；属于 WebTransportReliabilityMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webtransport/#dom-webtransportreliabilitymode-reliable-only">WebTransport: 6 WebTransport Interface</see>
    /// </remarks>
    [Description("@#reliable-only")]
    ReliableOnly = 1,

    /// <summary>
    /// JavaScript 字符串取值 “supports-unreliable”；属于 WebTransportReliabilityMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webtransport/#dom-webtransportreliabilitymode-supports-unreliable">WebTransport: 6 WebTransport Interface</see>
    /// </remarks>
    [Description("@#supports-unreliable")]
    SupportsUnreliable = 2
}

/// <summary>
/// WebIDL enum AlignSetting。定义于 WebVTT: The Web Video Text Tracks Format。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webvtt/#enumdef-alignsetting">WebVTT: The Web Video Text Tracks Format: 9.1 The VTTCue interface</see>
/// </remarks>
[Description("@#AlignSetting")]
[ECMAScript]
[String]
public enum AlignSetting
{
    /// <summary>
    /// JavaScript 字符串取值 “start”；属于 AlignSetting 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webvtt/#dom-alignsetting-start">WebVTT: The Web Video Text Tracks Format: 9.1 The VTTCue interface</see>
    /// </remarks>
    [Description("@#start")]
    Start = 0,

    /// <summary>
    /// JavaScript 字符串取值 “center”；属于 AlignSetting 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webvtt/#dom-alignsetting-center">WebVTT: The Web Video Text Tracks Format: 9.1 The VTTCue interface</see>
    /// </remarks>
    [Description("@#center")]
    Center = 1,

    /// <summary>
    /// JavaScript 字符串取值 “end”；属于 AlignSetting 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webvtt/#dom-alignsetting-end">WebVTT: The Web Video Text Tracks Format: 9.1 The VTTCue interface</see>
    /// </remarks>
    [Description("@#end")]
    End = 2,

    /// <summary>
    /// JavaScript 字符串取值 “left”；属于 AlignSetting 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webvtt/#dom-alignsetting-left">WebVTT: The Web Video Text Tracks Format: 9.1 The VTTCue interface</see>
    /// </remarks>
    [Description("@#left")]
    Left = 3,

    /// <summary>
    /// JavaScript 字符串取值 “right”；属于 AlignSetting 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webvtt/#dom-alignsetting-right">WebVTT: The Web Video Text Tracks Format: 9.1 The VTTCue interface</see>
    /// </remarks>
    [Description("@#right")]
    Right = 4
}

/// <summary>
/// WebIDL enum AutoKeyword。定义于 WebVTT: The Web Video Text Tracks Format。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webvtt/#enumdef-autokeyword">WebVTT: The Web Video Text Tracks Format: 9.1 The VTTCue interface</see>
/// </remarks>
[Description("@#AutoKeyword")]
[ECMAScript]
[String]
public enum AutoKeyword
{
    /// <summary>
    /// JavaScript 字符串取值 “auto”；属于 AutoKeyword 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webvtt/#dom-autokeyword-auto">WebVTT: The Web Video Text Tracks Format: 9.1 The VTTCue interface</see>
    /// </remarks>
    [Description("@#auto")]
    Auto = 0
}

/// <summary>
/// WebIDL enum DirectionSetting。定义于 WebVTT: The Web Video Text Tracks Format。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webvtt/#enumdef-directionsetting">WebVTT: The Web Video Text Tracks Format: 9.1 The VTTCue interface</see>
/// </remarks>
[Description("@#DirectionSetting")]
[ECMAScript]
[String]
public enum DirectionSetting
{
    /// <summary>
    /// JavaScript 字符串取值 “”；属于 DirectionSetting 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webvtt/">WebVTT: The Web Video Text Tracks Format: DirectionSetting.</see>
    /// </remarks>
[Description("@#")]
    Empty = 0,

    /// <summary>
    /// JavaScript 字符串取值 “rl”；属于 DirectionSetting 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webvtt/#dom-directionsetting-rl">WebVTT: The Web Video Text Tracks Format: 9.1 The VTTCue interface</see>
    /// </remarks>
    [Description("@#rl")]
    Rl = 1,

    /// <summary>
    /// JavaScript 字符串取值 “lr”；属于 DirectionSetting 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webvtt/#dom-directionsetting-lr">WebVTT: The Web Video Text Tracks Format: 9.1 The VTTCue interface</see>
    /// </remarks>
    [Description("@#lr")]
    Lr = 2
}

/// <summary>
/// WebIDL enum LineAlignSetting。定义于 WebVTT: The Web Video Text Tracks Format。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webvtt/#enumdef-linealignsetting">WebVTT: The Web Video Text Tracks Format: 9.1 The VTTCue interface</see>
/// </remarks>
[Description("@#LineAlignSetting")]
[ECMAScript]
[String]
public enum LineAlignSetting
{
    /// <summary>
    /// JavaScript 字符串取值 “start”；属于 LineAlignSetting 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webvtt/#dom-linealignsetting-start">WebVTT: The Web Video Text Tracks Format: 9.1 The VTTCue interface</see>
    /// </remarks>
    [Description("@#start")]
    Start = 0,

    /// <summary>
    /// JavaScript 字符串取值 “center”；属于 LineAlignSetting 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webvtt/#dom-linealignsetting-center">WebVTT: The Web Video Text Tracks Format: 9.1 The VTTCue interface</see>
    /// </remarks>
    [Description("@#center")]
    Center = 1,

    /// <summary>
    /// JavaScript 字符串取值 “end”；属于 LineAlignSetting 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webvtt/#dom-linealignsetting-end">WebVTT: The Web Video Text Tracks Format: 9.1 The VTTCue interface</see>
    /// </remarks>
    [Description("@#end")]
    End = 2
}

/// <summary>
/// WebIDL enum PositionAlignSetting。定义于 WebVTT: The Web Video Text Tracks Format。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webvtt/#enumdef-positionalignsetting">WebVTT: The Web Video Text Tracks Format: 9.1 The VTTCue interface</see>
/// </remarks>
[Description("@#PositionAlignSetting")]
[ECMAScript]
[String]
public enum PositionAlignSetting
{
    /// <summary>
    /// JavaScript 字符串取值 “line-left”；属于 PositionAlignSetting 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webvtt/#dom-positionalignsetting-line-left">WebVTT: The Web Video Text Tracks Format: 9.1 The VTTCue interface</see>
    /// </remarks>
    [Description("@#line-left")]
    LineLeft = 0,

    /// <summary>
    /// JavaScript 字符串取值 “center”；属于 PositionAlignSetting 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webvtt/#dom-positionalignsetting-center">WebVTT: The Web Video Text Tracks Format: 9.1 The VTTCue interface</see>
    /// </remarks>
    [Description("@#center")]
    Center = 1,

    /// <summary>
    /// JavaScript 字符串取值 “line-right”；属于 PositionAlignSetting 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webvtt/#dom-positionalignsetting-line-right">WebVTT: The Web Video Text Tracks Format: 9.1 The VTTCue interface</see>
    /// </remarks>
    [Description("@#line-right")]
    LineRight = 2,

    /// <summary>
    /// JavaScript 字符串取值 “auto”；属于 PositionAlignSetting 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webvtt/#dom-positionalignsetting-auto">WebVTT: The Web Video Text Tracks Format: 9.1 The VTTCue interface</see>
    /// </remarks>
    [Description("@#auto")]
    Auto = 3
}

/// <summary>
/// WebIDL enum ScrollSetting。定义于 WebVTT: The Web Video Text Tracks Format。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webvtt/#enumdef-scrollsetting">WebVTT: The Web Video Text Tracks Format: 9.2 The VTTRegion interface</see>
/// </remarks>
[Description("@#ScrollSetting")]
[ECMAScript]
[String]
public enum ScrollSetting
{
    /// <summary>
    /// JavaScript 字符串取值 “”；属于 ScrollSetting 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webvtt/">WebVTT: The Web Video Text Tracks Format: ScrollSetting.</see>
    /// </remarks>
[Description("@#")]
    Empty = 0,

    /// <summary>
    /// JavaScript 字符串取值 “up”；属于 ScrollSetting 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webvtt/#dom-scrollsetting-up">WebVTT: The Web Video Text Tracks Format: 9.2 The VTTRegion interface</see>
    /// </remarks>
    [Description("@#up")]
    Up = 1
}

/// <summary>
/// The AudioContext() constructor creates a new AudioContext object which represents an audio-processing graph, built from audio modules linked together, each represented by an AudioNode.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/AudioContext/AudioContext">MDN Web Docs: AudioContextLatencyCategory</see>
/// </remarks>
[Description("@#AudioContextLatencyCategory")]
[ECMAScript]
[String]
public enum AudioContextLatencyCategory
{
    /// <summary>
    /// JavaScript 字符串取值 “balanced”；属于 AudioContextLatencyCategory 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-audiocontextlatencycategory-balanced">Web Audio API 1.1: 1.2 The AudioContext Interface</see>
    /// </remarks>
    [Description("@#balanced")]
    Balanced = 0,

    /// <summary>
    /// JavaScript 字符串取值 “interactive”；属于 AudioContextLatencyCategory 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-audiocontextlatencycategory-interactive">Web Audio API 1.1: 1.2 The AudioContext Interface</see>
    /// </remarks>
    [Description("@#interactive")]
    Interactive = 1,

    /// <summary>
    /// JavaScript 字符串取值 “playback”；属于 AudioContextLatencyCategory 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-audiocontextlatencycategory-playback">Web Audio API 1.1: 1.2 The AudioContext Interface</see>
    /// </remarks>
    [Description("@#playback")]
    Playback = 2
}

/// <summary>
/// WebIDL enum AudioContextRenderSizeCategory。定义于 Web Audio API 1.1。
/// </summary>
/// <remarks>
/// <see href="https://webaudio.github.io/web-audio-api/#enumdef-audiocontextrendersizecategory">Web Audio API 1.1: 1.1 The BaseAudioContext Interface</see>
/// </remarks>
[Description("@#AudioContextRenderSizeCategory")]
[ECMAScript]
[String]
public enum AudioContextRenderSizeCategory
{
    /// <summary>
    /// JavaScript 字符串取值 “default”；属于 AudioContextRenderSizeCategory 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-audiocontextrendersizecategory-default">Web Audio API 1.1: 1.1 The BaseAudioContext Interface</see>
    /// </remarks>
    [Description("@#default")]
    Default = 0,

    /// <summary>
    /// JavaScript 字符串取值 “hardware”；属于 AudioContextRenderSizeCategory 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-audiocontextrendersizecategory-hardware">Web Audio API 1.1: 1.1 The BaseAudioContext Interface</see>
    /// </remarks>
    [Description("@#hardware")]
    Hardware = 1
}

/// <summary>
/// WebIDL enum AudioContextState。定义于 Web Audio API 1.1。
/// </summary>
/// <remarks>
/// <see href="https://webaudio.github.io/web-audio-api/#enumdef-audiocontextstate">Web Audio API 1.1: 1.1 The BaseAudioContext Interface</see>
/// </remarks>
[Description("@#AudioContextState")]
[ECMAScript]
[String]
public enum AudioContextState
{
    /// <summary>
    /// JavaScript 字符串取值 “suspended”；属于 AudioContextState 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-audiocontextstate-suspended">Web Audio API 1.1: 1.1 The BaseAudioContext Interface</see>
    /// </remarks>
    [Description("@#suspended")]
    Suspended = 0,

    /// <summary>
    /// JavaScript 字符串取值 “running”；属于 AudioContextState 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-audiocontextstate-running">Web Audio API 1.1: 1.1 The BaseAudioContext Interface</see>
    /// </remarks>
    [Description("@#running")]
    Running = 1,

    /// <summary>
    /// If the control thread state on the OfflineAudioContext is closed, reject promise with InvalidStateError and abort these steps.
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-audiocontextstate-closed">Web Audio API 1.1: 1.1 The BaseAudioContext Interface</see>
    /// </remarks>
    [Description("@#closed")]
    Closed = 2,

    /// <summary>
    /// JavaScript 字符串取值 “interrupted”；属于 AudioContextState 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-audiocontextstate-interrupted">Web Audio API 1.1: 1.1 The BaseAudioContext Interface</see>
    /// </remarks>
    [Description("@#interrupted")]
    Interrupted = 3
}

/// <summary>
/// WebIDL enum AudioSinkType。定义于 Web Audio API 1.1。
/// </summary>
/// <remarks>
/// <see href="https://webaudio.github.io/web-audio-api/#enumdef-audiosinktype">Web Audio API 1.1: 1.2 The AudioContext Interface</see>
/// </remarks>
[Description("@#AudioSinkType")]
[ECMAScript]
[String]
public enum AudioSinkType
{
    /// <summary>
    /// JavaScript 字符串取值 “none”；属于 AudioSinkType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-audiosinktype-none">Web Audio API 1.1: 1.2 The AudioContext Interface</see>
    /// </remarks>
    [Description("@#none")]
    None = 0
}

/// <summary>
/// WebIDL enum AutomationRate。定义于 Web Audio API 1.1。
/// </summary>
/// <remarks>
/// <see href="https://webaudio.github.io/web-audio-api/#enumdef-automationrate">Web Audio API 1.1: 1.6 The AudioParam Interface</see>
/// </remarks>
[Description("@#AutomationRate")]
[ECMAScript]
[String]
public enum AutomationRate
{
    /// <summary>
    /// JavaScript 字符串取值 “a-rate”；属于 AutomationRate 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-automationrate-a-rate">Web Audio API 1.1: 1.6 The AudioParam Interface</see>
    /// </remarks>
    [Description("@#a-rate")]
    ARate = 0,

    /// <summary>
    /// JavaScript 字符串取值 “k-rate”；属于 AutomationRate 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-automationrate-k-rate">Web Audio API 1.1: 1.6 The AudioParam Interface</see>
    /// </remarks>
    [Description("@#k-rate")]
    KRate = 1
}

/// <summary>
/// WebIDL enum BiquadFilterType。定义于 Web Audio API 1.1。
/// </summary>
/// <remarks>
/// <see href="https://webaudio.github.io/web-audio-api/#enumdef-biquadfiltertype">Web Audio API 1.1: 1.13 The BiquadFilterNode Interface</see>
/// </remarks>
[Description("@#BiquadFilterType")]
[ECMAScript]
[String]
public enum BiquadFilterType
{
    /// <summary>
    /// JavaScript 字符串取值 “lowpass”；属于 BiquadFilterType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-biquadfiltertype-lowpass">Web Audio API 1.1: 1.13 The BiquadFilterNode Interface</see>
    /// </remarks>
    [Description("@#lowpass")]
    Lowpass = 0,

    /// <summary>
    /// JavaScript 字符串取值 “highpass”；属于 BiquadFilterType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-biquadfiltertype-highpass">Web Audio API 1.1: 1.13 The BiquadFilterNode Interface</see>
    /// </remarks>
    [Description("@#highpass")]
    Highpass = 1,

    /// <summary>
    /// JavaScript 字符串取值 “bandpass”；属于 BiquadFilterType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-biquadfiltertype-bandpass">Web Audio API 1.1: 1.13 The BiquadFilterNode Interface</see>
    /// </remarks>
    [Description("@#bandpass")]
    Bandpass = 2,

    /// <summary>
    /// JavaScript 字符串取值 “lowshelf”；属于 BiquadFilterType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-biquadfiltertype-lowshelf">Web Audio API 1.1: 1.13 The BiquadFilterNode Interface</see>
    /// </remarks>
    [Description("@#lowshelf")]
    Lowshelf = 3,

    /// <summary>
    /// JavaScript 字符串取值 “highshelf”；属于 BiquadFilterType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-biquadfiltertype-highshelf">Web Audio API 1.1: 1.13 The BiquadFilterNode Interface</see>
    /// </remarks>
    [Description("@#highshelf")]
    Highshelf = 4,

    /// <summary>
    /// JavaScript 字符串取值 “peaking”；属于 BiquadFilterType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-biquadfiltertype-peaking">Web Audio API 1.1: 1.13 The BiquadFilterNode Interface</see>
    /// </remarks>
    [Description("@#peaking")]
    Peaking = 5,

    /// <summary>
    /// JavaScript 字符串取值 “notch”；属于 BiquadFilterType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-biquadfiltertype-notch">Web Audio API 1.1: 1.13 The BiquadFilterNode Interface</see>
    /// </remarks>
    [Description("@#notch")]
    Notch = 6,

    /// <summary>
    /// JavaScript 字符串取值 “allpass”；属于 BiquadFilterType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-biquadfiltertype-allpass">Web Audio API 1.1: 1.13 The BiquadFilterNode Interface</see>
    /// </remarks>
    [Description("@#allpass")]
    Allpass = 7
}

/// <summary>
/// WebIDL enum ChannelCountMode。定义于 Web Audio API 1.1。
/// </summary>
/// <remarks>
/// <see href="https://webaudio.github.io/web-audio-api/#enumdef-channelcountmode">Web Audio API 1.1: 1.5.1 AudioNode Creation</see>
/// </remarks>
[Description("@#ChannelCountMode")]
[ECMAScript]
[String]
public enum ChannelCountMode
{
    /// <summary>
    /// JavaScript 字符串取值 “max”；属于 ChannelCountMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-channelcountmode-max">Web Audio API 1.1: 1.5.1 AudioNode Creation</see>
    /// </remarks>
    [Description("@#max")]
    Max = 0,

    /// <summary>
    /// JavaScript 字符串取值 “clamped-max”；属于 ChannelCountMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-channelcountmode-clamped-max">Web Audio API 1.1: 1.5.1 AudioNode Creation</see>
    /// </remarks>
    [Description("@#clamped-max")]
    ClampedMax = 1,

    /// <summary>
    /// JavaScript 字符串取值 “explicit”；属于 ChannelCountMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-channelcountmode-explicit">Web Audio API 1.1: 1.5.1 AudioNode Creation</see>
    /// </remarks>
    [Description("@#explicit")]
    Explicit = 2
}

/// <summary>
/// WebIDL enum ChannelInterpretation。定义于 Web Audio API 1.1。
/// </summary>
/// <remarks>
/// <see href="https://webaudio.github.io/web-audio-api/#enumdef-channelinterpretation">Web Audio API 1.1: 1.5.1 AudioNode Creation</see>
/// </remarks>
[Description("@#ChannelInterpretation")]
[ECMAScript]
[String]
public enum ChannelInterpretation
{
    /// <summary>
    /// JavaScript 字符串取值 “speakers”；属于 ChannelInterpretation 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-channelinterpretation-speakers">Web Audio API 1.1: 1.5.1 AudioNode Creation</see>
    /// </remarks>
    [Description("@#speakers")]
    Speakers = 0,

    /// <summary>
    /// JavaScript 字符串取值 “discrete”；属于 ChannelInterpretation 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-channelinterpretation-discrete">Web Audio API 1.1: 1.5.1 AudioNode Creation</see>
    /// </remarks>
    [Description("@#discrete")]
    Discrete = 1
}

/// <summary>
/// WebIDL enum DistanceModelType。定义于 Web Audio API 1.1。
/// </summary>
/// <remarks>
/// <see href="https://webaudio.github.io/web-audio-api/#enumdef-distancemodeltype">Web Audio API 1.1: 1.27 The PannerNode Interface</see>
/// </remarks>
[Description("@#DistanceModelType")]
[ECMAScript]
[String]
public enum DistanceModelType
{
    /// <summary>
    /// JavaScript 字符串取值 “linear”；属于 DistanceModelType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-distancemodeltype-linear">Web Audio API 1.1: 1.27 The PannerNode Interface</see>
    /// </remarks>
    [Description("@#linear")]
    Linear = 0,

    /// <summary>
    /// JavaScript 字符串取值 “inverse”；属于 DistanceModelType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-distancemodeltype-inverse">Web Audio API 1.1: 1.27 The PannerNode Interface</see>
    /// </remarks>
    [Description("@#inverse")]
    Inverse = 1,

    /// <summary>
    /// JavaScript 字符串取值 “exponential”；属于 DistanceModelType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-distancemodeltype-exponential">Web Audio API 1.1: 1.27 The PannerNode Interface</see>
    /// </remarks>
    [Description("@#exponential")]
    Exponential = 2
}

/// <summary>
/// WebIDL enum OscillatorType。定义于 Web Audio API 1.1。
/// </summary>
/// <remarks>
/// <see href="https://webaudio.github.io/web-audio-api/#enumdef-oscillatortype">Web Audio API 1.1: 1.26 The OscillatorNode Interface</see>
/// </remarks>
[Description("@#OscillatorType")]
[ECMAScript]
[String]
public enum OscillatorType
{
    /// <summary>
    /// JavaScript 字符串取值 “sine”；属于 OscillatorType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-oscillatortype-sine">Web Audio API 1.1: 1.26 The OscillatorNode Interface</see>
    /// </remarks>
    [Description("@#sine")]
    Sine = 0,

    /// <summary>
    /// JavaScript 字符串取值 “square”；属于 OscillatorType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-oscillatortype-square">Web Audio API 1.1: 1.26 The OscillatorNode Interface</see>
    /// </remarks>
    [Description("@#square")]
    Square = 1,

    /// <summary>
    /// JavaScript 字符串取值 “sawtooth”；属于 OscillatorType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-oscillatortype-sawtooth">Web Audio API 1.1: 1.26 The OscillatorNode Interface</see>
    /// </remarks>
    [Description("@#sawtooth")]
    Sawtooth = 2,

    /// <summary>
    /// JavaScript 字符串取值 “triangle”；属于 OscillatorType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-oscillatortype-triangle">Web Audio API 1.1: 1.26 The OscillatorNode Interface</see>
    /// </remarks>
    [Description("@#triangle")]
    Triangle = 3,

    /// <summary>
    /// JavaScript 字符串取值 “custom”；属于 OscillatorType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-oscillatortype-custom">Web Audio API 1.1: 1.26 The OscillatorNode Interface</see>
    /// </remarks>
    [Description("@#custom")]
    Custom = 4
}

/// <summary>
/// WebIDL enum OverSampleType。定义于 Web Audio API 1.1。
/// </summary>
/// <remarks>
/// <see href="https://webaudio.github.io/web-audio-api/#enumdef-oversampletype">Web Audio API 1.1: 1.31 The WaveShaperNode Interface</see>
/// </remarks>
[Description("@#OverSampleType")]
[ECMAScript]
[String]
public enum OverSampleType
{
    /// <summary>
    /// JavaScript 字符串取值 “none”；属于 OverSampleType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-oversampletype-none">Web Audio API 1.1: 1.31 The WaveShaperNode Interface</see>
    /// </remarks>
    [Description("@#none")]
    None = 0,

    /// <summary>
    /// JavaScript 字符串取值 “2x”；属于 OverSampleType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-oversampletype-2x">Web Audio API 1.1: 1.31 The WaveShaperNode Interface</see>
    /// </remarks>
    [Description("@#2x")]
    _2x = 1,

    /// <summary>
    /// JavaScript 字符串取值 “4x”；属于 OverSampleType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-oversampletype-4x">Web Audio API 1.1: 1.31 The WaveShaperNode Interface</see>
    /// </remarks>
    [Description("@#4x")]
    _4x = 2
}

/// <summary>
/// WebIDL enum PanningModelType。定义于 Web Audio API 1.1。
/// </summary>
/// <remarks>
/// <see href="https://webaudio.github.io/web-audio-api/#enumdef-panningmodeltype">Web Audio API 1.1: 1.27 The PannerNode Interface</see>
/// </remarks>
[Description("@#PanningModelType")]
[ECMAScript]
[String]
public enum PanningModelType
{
    /// <summary>
    /// JavaScript 字符串取值 “equalpower”；属于 PanningModelType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-panningmodeltype-equalpower">Web Audio API 1.1: 1.27 The PannerNode Interface</see>
    /// </remarks>
    [Description("@#equalpower")]
    Equalpower = 0,

    /// <summary>
    /// JavaScript 字符串取值 “HRTF”；属于 PanningModelType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-audio-api/#dom-panningmodeltype-hrtf">Web Audio API 1.1: 1.27 The PannerNode Interface</see>
    /// </remarks>
    [Description("@#HRTF")]
    HRTF = 1
}

/// <summary>
/// WebIDL enum MIDIPortConnectionState。定义于 Web MIDI API。
/// </summary>
/// <remarks>
/// <see href="https://webaudio.github.io/web-midi-api/#dom-midiportconnectionstate">Web MIDI API: 5.4.5 MIDIPortConnectionState Enum</see>
/// </remarks>
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
/// WebIDL enum MIDIPortDeviceState。定义于 Web MIDI API。
/// </summary>
/// <remarks>
/// <see href="https://webaudio.github.io/web-midi-api/#dom-midiportdevicestate">Web MIDI API: 5.4.4 MIDIPortDeviceState Enum</see>
/// </remarks>
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
/// WebIDL enum MIDIPortType。定义于 Web MIDI API。
/// </summary>
/// <remarks>
/// <see href="https://webaudio.github.io/web-midi-api/#dom-midiporttype">Web MIDI API: 5.4.3 MIDIPortType Enum</see>
/// </remarks>
[Description("@#MIDIPortType")]
[ECMAScript]
[String]
public enum MIDIPortType
{
    /// <summary>
    /// JavaScript 字符串取值 “input”；属于 MIDIPortType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-midi-api/#dom-midiporttype-input">Web MIDI API: 5.4.3 MIDIPortType Enum</see>
    /// </remarks>
    [Description("@#input")]
    Input = 0,

    /// <summary>
    /// JavaScript 字符串取值 “output”；属于 MIDIPortType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-midi-api/#dom-midiporttype-output">Web MIDI API: 5.4.3 MIDIPortType Enum</see>
    /// </remarks>
    [Description("@#output")]
    Output = 1
}

/// <summary>
/// WebIDL enum AvailabilityStatus。定义于 Web Speech API。
/// </summary>
/// <remarks>
/// <see href="https://webaudio.github.io/web-speech-api/#enumdef-availabilitystatus">Web Speech API: 4.1 The SpeechRecognition Interface</see>
/// </remarks>
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
/// WebIDL enum SpeechRecognitionErrorCode。定义于 Web Speech API。
/// </summary>
/// <remarks>
/// <see href="https://webaudio.github.io/web-speech-api/#enumdef-speechrecognitionerrorcode">Web Speech API: 4.1 The SpeechRecognition Interface</see>
/// </remarks>
[Description("@#SpeechRecognitionErrorCode")]
[ECMAScript]
[String]
public enum SpeechRecognitionErrorCode
{
    /// <summary>
    /// JavaScript 字符串取值 “no-speech”；属于 SpeechRecognitionErrorCode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-speech-api/#dom-speechrecognitionerrorcode-no-speech">Web Speech API: 4.1.6 SpeechRecognitionErrorEvent</see>
    /// </remarks>
    [Description("@#no-speech")]
    NoSpeech = 0,

    /// <summary>
    /// JavaScript 字符串取值 “aborted”；属于 SpeechRecognitionErrorCode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-speech-api/#dom-speechrecognitionerrorcode-aborted">Web Speech API: 4.1.6 SpeechRecognitionErrorEvent</see>
    /// </remarks>
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
    /// JavaScript 字符串取值 “network”；属于 SpeechRecognitionErrorCode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-speech-api/#dom-speechrecognitionerrorcode-network">Web Speech API: 4.1.6 SpeechRecognitionErrorEvent</see>
    /// </remarks>
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
/// WebIDL enum SpeechRecognitionQuality。定义于 Web Speech API。
/// </summary>
/// <remarks>
/// <see href="https://webaudio.github.io/web-speech-api/#enumdef-speechrecognitionquality">Web Speech API: 4.1 The SpeechRecognition Interface</see>
/// </remarks>
[Description("@#SpeechRecognitionQuality")]
[ECMAScript]
[String]
public enum SpeechRecognitionQuality
{
    /// <summary>
    /// JavaScript 字符串取值 “command”；属于 SpeechRecognitionQuality 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-speech-api/#dom-speechrecognitionquality-command">Web Speech API: 4.1.4 SpeechRecognitionQuality Enum Values</see>
    /// </remarks>
    [Description("@#command")]
    Command = 0,

    /// <summary>
    /// JavaScript 字符串取值 “dictation”；属于 SpeechRecognitionQuality 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-speech-api/#dom-speechrecognitionquality-dictation">Web Speech API: 4.1.4 SpeechRecognitionQuality Enum Values</see>
    /// </remarks>
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
/// WebIDL enum SpeechSynthesisErrorCode。定义于 Web Speech API。
/// </summary>
/// <remarks>
/// <see href="https://webaudio.github.io/web-speech-api/#enumdef-speechsynthesiserrorcode">Web Speech API: 4.2 The SpeechSynthesis Interface</see>
/// </remarks>
[Description("@#SpeechSynthesisErrorCode")]
[ECMAScript]
[String]
public enum SpeechSynthesisErrorCode
{
    /// <summary>
    /// JavaScript 字符串取值 “canceled”；属于 SpeechSynthesisErrorCode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-speech-api/#dom-speechsynthesiserrorcode-canceled">Web Speech API: 4.2.7 SpeechSynthesisErrorEvent Attributes</see>
    /// </remarks>
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
    /// JavaScript 字符串取值 “network”；属于 SpeechSynthesisErrorCode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webaudio.github.io/web-speech-api/#dom-speechsynthesiserrorcode-network">Web Speech API: 4.2.7 SpeechSynthesisErrorEvent Attributes</see>
    /// </remarks>
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
/// WebIDL enum LanguageModelMessageRole。定义于 Prompt API。
/// </summary>
/// <remarks>
/// <see href="https://webmachinelearning.github.io/prompt-api/#enumdef-languagemodelmessagerole">Prompt API: 3 The API</see>
/// </remarks>
[Description("@#LanguageModelMessageRole")]
[ECMAScript]
[String]
public enum LanguageModelMessageRole
{
    /// <summary>
    /// JavaScript 字符串取值 “system”；属于 LanguageModelMessageRole 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/prompt-api/#dom-languagemodelmessagerole-system">Prompt API: 3 The API</see>
    /// </remarks>
    [Description("@#system")]
    System = 0,

    /// <summary>
    /// JavaScript 字符串取值 “user”；属于 LanguageModelMessageRole 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/prompt-api/#dom-languagemodelmessagerole-user">Prompt API: 3 The API</see>
    /// </remarks>
    [Description("@#user")]
    User = 1,

    /// <summary>
    /// JavaScript 字符串取值 “assistant”；属于 LanguageModelMessageRole 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/prompt-api/#dom-languagemodelmessagerole-assistant">Prompt API: 3 The API</see>
    /// </remarks>
    [Description("@#assistant")]
    Assistant = 2
}

/// <summary>
/// WebIDL enum LanguageModelMessageType。定义于 Prompt API。
/// </summary>
/// <remarks>
/// <see href="https://webmachinelearning.github.io/prompt-api/#enumdef-languagemodelmessagetype">Prompt API: 3 The API</see>
/// </remarks>
[Description("@#LanguageModelMessageType")]
[ECMAScript]
[String]
public enum LanguageModelMessageType
{
    /// <summary>
    /// JavaScript 字符串取值 “text”；属于 LanguageModelMessageType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/prompt-api/#dom-languagemodelmessagetype-text">Prompt API: 3 The API</see>
    /// </remarks>
    [Description("@#text")]
    Text = 0,

    /// <summary>
    /// JavaScript 字符串取值 “image”；属于 LanguageModelMessageType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/prompt-api/#dom-languagemodelmessagetype-image">Prompt API: 3 The API</see>
    /// </remarks>
    [Description("@#image")]
    Image = 1,

    /// <summary>
    /// JavaScript 字符串取值 “audio”；属于 LanguageModelMessageType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/prompt-api/#dom-languagemodelmessagetype-audio">Prompt API: 3 The API</see>
    /// </remarks>
    [Description("@#audio")]
    Audio = 2,

    /// <summary>
    /// JavaScript 字符串取值 “tool-call”；属于 LanguageModelMessageType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/prompt-api/#dom-languagemodelmessagetype-tool-call">Prompt API: 3 The API</see>
    /// </remarks>
    [Description("@#tool-call")]
    ToolCall = 3,

    /// <summary>
    /// JavaScript 字符串取值 “tool-response”；属于 LanguageModelMessageType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/prompt-api/#dom-languagemodelmessagetype-tool-response">Prompt API: 3 The API</see>
    /// </remarks>
    [Description("@#tool-response")]
    ToolResponse = 4
}

/// <summary>
/// WebIDL enum LanguageModelSamplingMode。定义于 Prompt API。
/// </summary>
/// <remarks>
/// <see href="https://webmachinelearning.github.io/prompt-api/#enumdef-languagemodelsamplingmode">Prompt API: 3 The API</see>
/// </remarks>
[Description("@#LanguageModelSamplingMode")]
[ECMAScript]
[String]
public enum LanguageModelSamplingMode
{
    /// <summary>
    /// JavaScript 字符串取值 “most-predictable”；属于 LanguageModelSamplingMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/prompt-api/#dom-languagemodelsamplingmode-most-predictable">Prompt API: 3 The API</see>
    /// </remarks>
    [Description("@#most-predictable")]
    MostPredictable = 0,

    /// <summary>
    /// JavaScript 字符串取值 “predictable”；属于 LanguageModelSamplingMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/prompt-api/#dom-languagemodelsamplingmode-predictable">Prompt API: 3 The API</see>
    /// </remarks>
    [Description("@#predictable")]
    Predictable = 1,

    /// <summary>
    /// JavaScript 字符串取值 “slightly-predictable”；属于 LanguageModelSamplingMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/prompt-api/#dom-languagemodelsamplingmode-slightly-predictable">Prompt API: 3 The API</see>
    /// </remarks>
    [Description("@#slightly-predictable")]
    SlightlyPredictable = 2,

    /// <summary>
    /// JavaScript 字符串取值 “balanced”；属于 LanguageModelSamplingMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/prompt-api/#dom-languagemodelsamplingmode-balanced">Prompt API: 3 The API</see>
    /// </remarks>
    [Description("@#balanced")]
    Balanced = 3,

    /// <summary>
    /// JavaScript 字符串取值 “slightly-creative”；属于 LanguageModelSamplingMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/prompt-api/#dom-languagemodelsamplingmode-slightly-creative">Prompt API: 3 The API</see>
    /// </remarks>
    [Description("@#slightly-creative")]
    SlightlyCreative = 4,

    /// <summary>
    /// JavaScript 字符串取值 “creative”；属于 LanguageModelSamplingMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/prompt-api/#dom-languagemodelsamplingmode-creative">Prompt API: 3 The API</see>
    /// </remarks>
    [Description("@#creative")]
    Creative = 5,

    /// <summary>
    /// JavaScript 字符串取值 “most-creative”；属于 LanguageModelSamplingMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/prompt-api/#dom-languagemodelsamplingmode-most-creative">Prompt API: 3 The API</see>
    /// </remarks>
    [Description("@#most-creative")]
    MostCreative = 6
}

/// <summary>
/// WebIDL enum MLConv2dFilterOperandLayout。定义于 Web Neural Network API。
/// </summary>
/// <remarks>
/// <see href="https://webmachinelearning.github.io/webnn/#enumdef-mlconv2dfilteroperandlayout">Web Neural Network API: 8.9.10 conv2d</see>
/// </remarks>
[Description("@#MLConv2dFilterOperandLayout")]
[ECMAScript]
[String]
public enum MLConv2dFilterOperandLayout
{
    /// <summary>
    /// JavaScript 字符串取值 “oihw”；属于 MLConv2dFilterOperandLayout 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mlconv2dfilteroperandlayout-oihw">Web Neural Network API: 8.9.10 conv2d</see>
    /// </remarks>
    [Description("@#oihw")]
    Oihw = 0,

    /// <summary>
    /// JavaScript 字符串取值 “hwio”；属于 MLConv2dFilterOperandLayout 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mlconv2dfilteroperandlayout-hwio">Web Neural Network API: 8.9.10 conv2d</see>
    /// </remarks>
    [Description("@#hwio")]
    Hwio = 1,

    /// <summary>
    /// JavaScript 字符串取值 “ohwi”；属于 MLConv2dFilterOperandLayout 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mlconv2dfilteroperandlayout-ohwi">Web Neural Network API: 8.9.10 conv2d</see>
    /// </remarks>
    [Description("@#ohwi")]
    Ohwi = 2,

    /// <summary>
    /// JavaScript 字符串取值 “ihwo”；属于 MLConv2dFilterOperandLayout 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mlconv2dfilteroperandlayout-ihwo">Web Neural Network API: 8.9.10 conv2d</see>
    /// </remarks>
    [Description("@#ihwo")]
    Ihwo = 3
}

/// <summary>
/// WebIDL enum MLConvTranspose2dFilterOperandLayout。定义于 Web Neural Network API。
/// </summary>
/// <remarks>
/// <see href="https://webmachinelearning.github.io/webnn/#enumdef-mlconvtranspose2dfilteroperandlayout">Web Neural Network API: 8.9.11 convTranspose2d</see>
/// </remarks>
[Description("@#MLConvTranspose2dFilterOperandLayout")]
[ECMAScript]
[String]
public enum MLConvTranspose2dFilterOperandLayout
{
    /// <summary>
    /// JavaScript 字符串取值 “iohw”；属于 MLConvTranspose2dFilterOperandLayout 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mlconvtranspose2dfilteroperandlayout-iohw">Web Neural Network API: 8.9.11 convTranspose2d</see>
    /// </remarks>
    [Description("@#iohw")]
    Iohw = 0,

    /// <summary>
    /// JavaScript 字符串取值 “hwoi”；属于 MLConvTranspose2dFilterOperandLayout 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mlconvtranspose2dfilteroperandlayout-hwoi">Web Neural Network API: 8.9.11 convTranspose2d</see>
    /// </remarks>
    [Description("@#hwoi")]
    Hwoi = 1,

    /// <summary>
    /// JavaScript 字符串取值 “ohwi”；属于 MLConvTranspose2dFilterOperandLayout 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mlconvtranspose2dfilteroperandlayout-ohwi">Web Neural Network API: 8.9.11 convTranspose2d</see>
    /// </remarks>
    [Description("@#ohwi")]
    Ohwi = 2
}

/// <summary>
/// WebIDL enum MLGruWeightLayout。定义于 Web Neural Network API。
/// </summary>
/// <remarks>
/// <see href="https://webmachinelearning.github.io/webnn/#enumdef-mlgruweightlayout">Web Neural Network API: 8.9.25 gru</see>
/// </remarks>
[Description("@#MLGruWeightLayout")]
[ECMAScript]
[String]
public enum MLGruWeightLayout
{
    /// <summary>
    /// JavaScript 字符串取值 “zrn”；属于 MLGruWeightLayout 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mlgruweightlayout-zrn">Web Neural Network API: 8.9.25 gru</see>
    /// </remarks>
    [Description("@#zrn")]
    Zrn = 0,

    /// <summary>
    /// JavaScript 字符串取值 “rzn”；属于 MLGruWeightLayout 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mlgruweightlayout-rzn">Web Neural Network API: 8.9.25 gru</see>
    /// </remarks>
    [Description("@#rzn")]
    Rzn = 1
}

/// <summary>
/// WebIDL enum MLInputOperandLayout。定义于 Web Neural Network API。
/// </summary>
/// <remarks>
/// <see href="https://webmachinelearning.github.io/webnn/#enumdef-mlinputoperandlayout">Web Neural Network API: 8.5 MLOperandDescriptor dictionary</see>
/// </remarks>
[Description("@#MLInputOperandLayout")]
[ECMAScript]
[String]
public enum MLInputOperandLayout
{
    /// <summary>
    /// JavaScript 字符串取值 “nchw”；属于 MLInputOperandLayout 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mlinputoperandlayout-nchw">Web Neural Network API: 8.5 MLOperandDescriptor dictionary</see>
    /// </remarks>
    [Description("@#nchw")]
    Nchw = 0,

    /// <summary>
    /// JavaScript 字符串取值 “nhwc”；属于 MLInputOperandLayout 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mlinputoperandlayout-nhwc">Web Neural Network API: 8.5 MLOperandDescriptor dictionary</see>
    /// </remarks>
    [Description("@#nhwc")]
    Nhwc = 1
}

/// <summary>
/// WebIDL enum MLInterpolationMode。定义于 Web Neural Network API。
/// </summary>
/// <remarks>
/// <see href="https://webmachinelearning.github.io/webnn/#enumdef-mlinterpolationmode">Web Neural Network API: 8.9.41 resample2d</see>
/// </remarks>
[Description("@#MLInterpolationMode")]
[ECMAScript]
[String]
public enum MLInterpolationMode
{
    /// <summary>
    /// JavaScript 字符串取值 “nearest-neighbor”；属于 MLInterpolationMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mlinterpolationmode-nearest-neighbor">Web Neural Network API: 8.9.41 resample2d</see>
    /// </remarks>
    [Description("@#nearest-neighbor")]
    NearestNeighbor = 0,

    /// <summary>
    /// JavaScript 字符串取值 “linear”；属于 MLInterpolationMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mlinterpolationmode-linear">Web Neural Network API: 8.9.41 resample2d</see>
    /// </remarks>
    [Description("@#linear")]
    Linear = 1
}

/// <summary>
/// WebIDL enum MLLstmWeightLayout。定义于 Web Neural Network API。
/// </summary>
/// <remarks>
/// <see href="https://webmachinelearning.github.io/webnn/#enumdef-mllstmweightlayout">Web Neural Network API: 8.9.33 lstm</see>
/// </remarks>
[Description("@#MLLstmWeightLayout")]
[ECMAScript]
[String]
public enum MLLstmWeightLayout
{
    /// <summary>
    /// JavaScript 字符串取值 “iofg”；属于 MLLstmWeightLayout 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mllstmweightlayout-iofg">Web Neural Network API: 8.9.33 lstm</see>
    /// </remarks>
    [Description("@#iofg")]
    Iofg = 0,

    /// <summary>
    /// JavaScript 字符串取值 “ifgo”；属于 MLLstmWeightLayout 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mllstmweightlayout-ifgo">Web Neural Network API: 8.9.33 lstm</see>
    /// </remarks>
    [Description("@#ifgo")]
    Ifgo = 1
}

/// <summary>
/// WebIDL enum MLOperandDataType。定义于 Web Neural Network API。
/// </summary>
/// <remarks>
/// <see href="https://webmachinelearning.github.io/webnn/#enumdef-mloperanddatatype">Web Neural Network API: 8.5 MLOperandDescriptor dictionary</see>
/// </remarks>
[Description("@#MLOperandDataType")]
[ECMAScript]
[String]
public enum MLOperandDataType
{
    /// <summary>
    /// JavaScript 字符串取值 “float32”；属于 MLOperandDataType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mloperanddatatype-float32">Web Neural Network API: 8.5 MLOperandDescriptor dictionary</see>
    /// </remarks>
    [Description("@#float32")]
    Float32 = 0,

    /// <summary>
    /// JavaScript 字符串取值 “float16”；属于 MLOperandDataType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mloperanddatatype-float16">Web Neural Network API: 8.5 MLOperandDescriptor dictionary</see>
    /// </remarks>
    [Description("@#float16")]
    Float16 = 1,

    /// <summary>
    /// JavaScript 字符串取值 “int32”；属于 MLOperandDataType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mloperanddatatype-int32">Web Neural Network API: 8.5 MLOperandDescriptor dictionary</see>
    /// </remarks>
    [Description("@#int32")]
    Int32 = 2,

    /// <summary>
    /// JavaScript 字符串取值 “uint32”；属于 MLOperandDataType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mloperanddatatype-uint32">Web Neural Network API: 8.5 MLOperandDescriptor dictionary</see>
    /// </remarks>
    [Description("@#uint32")]
    Uint32 = 3,

    /// <summary>
    /// JavaScript 字符串取值 “int64”；属于 MLOperandDataType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mloperanddatatype-int64">Web Neural Network API: 8.5 MLOperandDescriptor dictionary</see>
    /// </remarks>
    [Description("@#int64")]
    Int64 = 4,

    /// <summary>
    /// JavaScript 字符串取值 “uint64”；属于 MLOperandDataType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mloperanddatatype-uint64">Web Neural Network API: 8.5 MLOperandDescriptor dictionary</see>
    /// </remarks>
    [Description("@#uint64")]
    Uint64 = 5,

    /// <summary>
    /// JavaScript 字符串取值 “int8”；属于 MLOperandDataType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mloperanddatatype-int8">Web Neural Network API: 8.5 MLOperandDescriptor dictionary</see>
    /// </remarks>
    [Description("@#int8")]
    Int8 = 6,

    /// <summary>
    /// JavaScript 字符串取值 “uint8”；属于 MLOperandDataType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mloperanddatatype-uint8">Web Neural Network API: 8.5 MLOperandDescriptor dictionary</see>
    /// </remarks>
    [Description("@#uint8")]
    Uint8 = 7
}

/// <summary>
/// WebIDL enum MLPaddingMode。定义于 Web Neural Network API。
/// </summary>
/// <remarks>
/// <see href="https://webmachinelearning.github.io/webnn/#enumdef-mlpaddingmode">Web Neural Network API: 8.9.36 pad</see>
/// </remarks>
[Description("@#MLPaddingMode")]
[ECMAScript]
[String]
public enum MLPaddingMode
{
    /// <summary>
    /// JavaScript 字符串取值 “constant”；属于 MLPaddingMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mlpaddingmode-constant">Web Neural Network API: 8.9.36 pad</see>
    /// </remarks>
    [Description("@#constant")]
    Constant = 0,

    /// <summary>
    /// JavaScript 字符串取值 “edge”；属于 MLPaddingMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mlpaddingmode-edge">Web Neural Network API: 8.9.36 pad</see>
    /// </remarks>
    [Description("@#edge")]
    Edge = 1,

    /// <summary>
    /// JavaScript 字符串取值 “reflection”；属于 MLPaddingMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mlpaddingmode-reflection">Web Neural Network API: 8.9.36 pad</see>
    /// </remarks>
    [Description("@#reflection")]
    Reflection = 2
}

/// <summary>
/// WebIDL enum MLPowerPreference。定义于 Web Neural Network API。
/// </summary>
/// <remarks>
/// <see href="https://webmachinelearning.github.io/webnn/#enumdef-mlpowerpreference">Web Neural Network API: 8.2.1 MLContextOptions</see>
/// </remarks>
[Description("@#MLPowerPreference")]
[ECMAScript]
[String]
public enum MLPowerPreference
{
    /// <summary>
    /// JavaScript 字符串取值 “default”；属于 MLPowerPreference 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mlpowerpreference-default">Web Neural Network API: 8.2.1 MLContextOptions</see>
    /// </remarks>
    [Description("@#default")]
    Default = 0,

    /// <summary>
    /// JavaScript 字符串取值 “high-performance”；属于 MLPowerPreference 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mlpowerpreference-high-performance">Web Neural Network API: 8.2.1 MLContextOptions</see>
    /// </remarks>
    [Description("@#high-performance")]
    HighPerformance = 1,

    /// <summary>
    /// JavaScript 字符串取值 “low-power”；属于 MLPowerPreference 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mlpowerpreference-low-power">Web Neural Network API: 8.2.1 MLContextOptions</see>
    /// </remarks>
    [Description("@#low-power")]
    LowPower = 2
}

/// <summary>
/// WebIDL enum MLRecurrentNetworkDirection。定义于 Web Neural Network API。
/// </summary>
/// <remarks>
/// <see href="https://webmachinelearning.github.io/webnn/#enumdef-mlrecurrentnetworkdirection">Web Neural Network API: 8.9.25 gru</see>
/// </remarks>
[Description("@#MLRecurrentNetworkDirection")]
[ECMAScript]
[String]
public enum MLRecurrentNetworkDirection
{
    /// <summary>
    /// JavaScript 字符串取值 “forward”；属于 MLRecurrentNetworkDirection 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mlrecurrentnetworkdirection-forward">Web Neural Network API: 8.9.25 gru</see>
    /// </remarks>
    [Description("@#forward")]
    Forward = 0,

    /// <summary>
    /// JavaScript 字符串取值 “backward”；属于 MLRecurrentNetworkDirection 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mlrecurrentnetworkdirection-backward">Web Neural Network API: 8.9.25 gru</see>
    /// </remarks>
    [Description("@#backward")]
    Backward = 1,

    /// <summary>
    /// JavaScript 字符串取值 “both”；属于 MLRecurrentNetworkDirection 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mlrecurrentnetworkdirection-both">Web Neural Network API: 8.9.25 gru</see>
    /// </remarks>
    [Description("@#both")]
    Both = 2
}

/// <summary>
/// WebIDL enum PerformancePreference。定义于 Writing Assistance APIs。
/// </summary>
/// <remarks>
/// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#enumdef-performancepreference">Writing Assistance APIs: 2 The summarizer API</see>
/// </remarks>
[Description("@#PerformancePreference")]
[ECMAScript]
[String]
public enum PerformancePreference
{
    /// <summary>
    /// JavaScript 字符串取值 “auto”；属于 PerformancePreference 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#dom-performancepreference-auto">Writing Assistance APIs: 2.4.3 Options</see>
    /// </remarks>
    [Description("@#auto")]
    Auto = 0,

    /// <summary>
    /// JavaScript 字符串取值 “speed”；属于 PerformancePreference 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#dom-performancepreference-speed">Writing Assistance APIs: 2.4.3 Options</see>
    /// </remarks>
    [Description("@#speed")]
    Speed = 1,

    /// <summary>
    /// JavaScript 字符串取值 “capability”；属于 PerformancePreference 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#dom-performancepreference-capability">Writing Assistance APIs: 2.4.3 Options</see>
    /// </remarks>
    [Description("@#capability")]
    Capability = 2
}

/// <summary>
/// WebIDL enum RewriterFormat。定义于 Writing Assistance APIs。
/// </summary>
/// <remarks>
/// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#enumdef-rewriterformat">Writing Assistance APIs: 4 The rewriter API</see>
/// </remarks>
[Description("@#RewriterFormat")]
[ECMAScript]
[String]
public enum RewriterFormat
{
    /// <summary>
    /// JavaScript 字符串取值 “as-is”；属于 RewriterFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#dom-rewriterformat-as-is">Writing Assistance APIs: 4.4.3 Options</see>
    /// </remarks>
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
/// WebIDL enum RewriterLength。定义于 Writing Assistance APIs。
/// </summary>
/// <remarks>
/// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#enumdef-rewriterlength">Writing Assistance APIs: 4 The rewriter API</see>
/// </remarks>
[Description("@#RewriterLength")]
[ECMAScript]
[String]
public enum RewriterLength
{
    /// <summary>
    /// JavaScript 字符串取值 “as-is”；属于 RewriterLength 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#dom-rewriterlength-as-is">Writing Assistance APIs: 4.4.3 Options</see>
    /// </remarks>
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
/// WebIDL enum RewriterTone。定义于 Writing Assistance APIs。
/// </summary>
/// <remarks>
/// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#enumdef-rewritertone">Writing Assistance APIs: 4 The rewriter API</see>
/// </remarks>
[Description("@#RewriterTone")]
[ECMAScript]
[String]
public enum RewriterTone
{
    /// <summary>
    /// JavaScript 字符串取值 “as-is”；属于 RewriterTone 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#dom-rewritertone-as-is">Writing Assistance APIs: 4.4.3 Options</see>
    /// </remarks>
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
/// WebIDL enum SummarizerFormat。定义于 Writing Assistance APIs。
/// </summary>
/// <remarks>
/// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#enumdef-summarizerformat">Writing Assistance APIs: 2 The summarizer API</see>
/// </remarks>
[Description("@#SummarizerFormat")]
[ECMAScript]
[String]
public enum SummarizerFormat
{
    /// <summary>
    /// JavaScript 字符串取值 “plain-text”；属于 SummarizerFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#dom-summarizerformat-plain-text">Writing Assistance APIs: 2.4.3 Options</see>
    /// </remarks>
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
/// WebIDL enum SummarizerLength。定义于 Writing Assistance APIs。
/// </summary>
/// <remarks>
/// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#enumdef-summarizerlength">Writing Assistance APIs: 2 The summarizer API</see>
/// </remarks>
[Description("@#SummarizerLength")]
[ECMAScript]
[String]
public enum SummarizerLength
{
    /// <summary>
    /// JavaScript 字符串取值 “short”；属于 SummarizerLength 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#dom-summarizerlength-short">Writing Assistance APIs: 2.4.3 Options</see>
    /// </remarks>
    [Description("@#short")]
    Short = 0,

    /// <summary>
    /// JavaScript 字符串取值 “medium”；属于 SummarizerLength 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#dom-summarizerlength-medium">Writing Assistance APIs: 2.4.3 Options</see>
    /// </remarks>
    [Description("@#medium")]
    Medium = 1,

    /// <summary>
    /// JavaScript 字符串取值 “long”；属于 SummarizerLength 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#dom-summarizerlength-long">Writing Assistance APIs: 2.4.3 Options</see>
    /// </remarks>
    [Description("@#long")]
    Long = 2
}

/// <summary>
/// WebIDL enum SummarizerType。定义于 Writing Assistance APIs。
/// </summary>
/// <remarks>
/// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#enumdef-summarizertype">Writing Assistance APIs: 2 The summarizer API</see>
/// </remarks>
[Description("@#SummarizerType")]
[ECMAScript]
[String]
public enum SummarizerType
{
    /// <summary>
    /// JavaScript 字符串取值 “tldr”；属于 SummarizerType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#dom-summarizertype-tldr">Writing Assistance APIs: 2.4.3 Options</see>
    /// </remarks>
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
/// WebIDL enum WriterFormat。定义于 Writing Assistance APIs。
/// </summary>
/// <remarks>
/// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#enumdef-writerformat">Writing Assistance APIs: 3 The writer API</see>
/// </remarks>
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
/// WebIDL enum WriterLength。定义于 Writing Assistance APIs。
/// </summary>
/// <remarks>
/// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#enumdef-writerlength">Writing Assistance APIs: 3 The writer API</see>
/// </remarks>
[Description("@#WriterLength")]
[ECMAScript]
[String]
public enum WriterLength
{
    /// <summary>
    /// JavaScript 字符串取值 “short”；属于 WriterLength 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#dom-writerlength-short">Writing Assistance APIs: 3.4.3 Options</see>
    /// </remarks>
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
/// WebIDL enum WriterTone。定义于 Writing Assistance APIs。
/// </summary>
/// <remarks>
/// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#enumdef-writertone">Writing Assistance APIs: 3 The writer API</see>
/// </remarks>
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
/// WebIDL enum BinaryType。定义于 WebSockets Standard。
/// </summary>
/// <remarks>
/// <see href="https://websockets.spec.whatwg.org/#enumdef-binarytype">WebSockets Standard: 3.1 Interface definition</see>
/// </remarks>
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
/// WebIDL enum ActivationBlockersMixinBlockerReason。定义于 The HTML Geolocation Element。
/// </summary>
/// <remarks>
/// <see href="https://wicg.github.io/PEPC/geolocation-element.html#enumdef-activationblockersmixinblockerreason">The HTML Geolocation Element: 3.1.2 Action Blockers, Blocker Reasons, and Blocker Lifetimes</see>
/// </remarks>
[Description("@#ActivationBlockersMixinBlockerReason")]
[ECMAScript]
[String]
public enum ActivationBlockersMixinBlockerReason
{
    /// <summary>
    /// JavaScript 字符串取值 “”；属于 ActivationBlockersMixinBlockerReason 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/PEPC/geolocation-element.html">The HTML Geolocation Element: ActivationBlockersMixinBlockerReason.</see>
    /// </remarks>
[Description("@#")]
    Empty = 0,

    /// <summary>
    /// JavaScript 字符串取值 “illegal_subframe”；属于 ActivationBlockersMixinBlockerReason 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/PEPC/geolocation-element.html#dom-activationblockersmixinblockerreason-illegal_subframe">The HTML Geolocation Element: 3.1.2 Action Blockers, Blocker Reasons, and Blocker Lifetimes</see>
    /// </remarks>
    [Description("@#illegal_subframe")]
    IllegalSubframe = 1,

    /// <summary>
    /// JavaScript 字符串取值 “unsuccessful_registration”；属于 ActivationBlockersMixinBlockerReason 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/PEPC/geolocation-element.html#dom-activationblockersmixinblockerreason-unsuccessful_registration">The HTML Geolocation Element: 3.1.2 Action Blockers, Blocker Reasons, and Blocker Lifetimes</see>
    /// </remarks>
    [Description("@#unsuccessful_registration")]
    UnsuccessfulRegistration = 2,

    /// <summary>
    /// JavaScript 字符串取值 “recently_attached”；属于 ActivationBlockersMixinBlockerReason 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/PEPC/geolocation-element.html#dom-activationblockersmixinblockerreason-recently_attached">The HTML Geolocation Element: 3.1.2 Action Blockers, Blocker Reasons, and Blocker Lifetimes</see>
    /// </remarks>
    [Description("@#recently_attached")]
    RecentlyAttached = 3,

    /// <summary>
    /// JavaScript 字符串取值 “intersection_changed”；属于 ActivationBlockersMixinBlockerReason 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/PEPC/geolocation-element.html#dom-activationblockersmixinblockerreason-intersection_changed">The HTML Geolocation Element: 3.1.2 Action Blockers, Blocker Reasons, and Blocker Lifetimes</see>
    /// </remarks>
    [Description("@#intersection_changed")]
    IntersectionChanged = 4,

    /// <summary>
    /// JavaScript 字符串取值 “intersection_out_of_viewport_or_clipped”；属于 ActivationBlockersMixinBlockerReason 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/PEPC/geolocation-element.html#dom-activationblockersmixinblockerreason-intersection_out_of_viewport_or_clipped">The HTML Geolocation Element: 3.1.2 Action Blockers, Blocker Reasons, and Blocker Lifetimes</see>
    /// </remarks>
    [Description("@#intersection_out_of_viewport_or_clipped")]
    IntersectionOutOfViewportOrClipped = 5,

    /// <summary>
    /// JavaScript 字符串取值 “intersection_occluded_or_distorted”；属于 ActivationBlockersMixinBlockerReason 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/PEPC/geolocation-element.html#dom-activationblockersmixinblockerreason-intersection_occluded_or_distorted">The HTML Geolocation Element: 3.1.2 Action Blockers, Blocker Reasons, and Blocker Lifetimes</see>
    /// </remarks>
    [Description("@#intersection_occluded_or_distorted")]
    IntersectionOccludedOrDistorted = 6,

    /// <summary>
    /// JavaScript 字符串取值 “style_invalid”；属于 ActivationBlockersMixinBlockerReason 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/PEPC/geolocation-element.html#dom-activationblockersmixinblockerreason-style_invalid">The HTML Geolocation Element: 3.1.2 Action Blockers, Blocker Reasons, and Blocker Lifetimes</see>
    /// </remarks>
    [Description("@#style_invalid")]
    StyleInvalid = 7,

    /// <summary>
    /// JavaScript 字符串取值 “type_invalid”；属于 ActivationBlockersMixinBlockerReason 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/PEPC/geolocation-element.html#dom-activationblockersmixinblockerreason-type_invalid">The HTML Geolocation Element: 3.1.2 Action Blockers, Blocker Reasons, and Blocker Lifetimes</see>
    /// </remarks>
    [Description("@#type_invalid")]
    TypeInvalid = 8
}

/// <summary>
/// WebIDL enum BackgroundFetchFailureReason。定义于 Background Fetch。
/// </summary>
/// <remarks>
/// <see href="https://wicg.github.io/background-fetch/#enumdef-backgroundfetchfailurereason">Background Fetch: 6.4 BackgroundFetchRegistration</see>
/// </remarks>
[Description("@#BackgroundFetchFailureReason")]
[ECMAScript]
[String]
public enum BackgroundFetchFailureReason
{
    /// <summary>
    /// JavaScript 字符串取值 “”；属于 BackgroundFetchFailureReason 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/background-fetch/">Background Fetch: BackgroundFetchFailureReason.</see>
    /// </remarks>
[Description("@#")]
    Empty = 0,

    /// <summary>
    /// JavaScript 字符串取值 “aborted”；属于 BackgroundFetchFailureReason 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/background-fetch/#dom-backgroundfetchfailurereason-aborted">Background Fetch: 6.4 BackgroundFetchRegistration</see>
    /// </remarks>
    [Description("@#aborted")]
    Aborted = 1,

    /// <summary>
    /// JavaScript 字符串取值 “bad-status”；属于 BackgroundFetchFailureReason 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/background-fetch/#dom-backgroundfetchfailurereason-bad-status">Background Fetch: 6.4 BackgroundFetchRegistration</see>
    /// </remarks>
    [Description("@#bad-status")]
    BadStatus = 2,

    /// <summary>
    /// JavaScript 字符串取值 “fetch-error”；属于 BackgroundFetchFailureReason 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/background-fetch/#dom-backgroundfetchfailurereason-fetch-error">Background Fetch: 6.4 BackgroundFetchRegistration</see>
    /// </remarks>
    [Description("@#fetch-error")]
    FetchError = 3,

    /// <summary>
    /// JavaScript 字符串取值 “quota-exceeded”；属于 BackgroundFetchFailureReason 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/background-fetch/#dom-backgroundfetchfailurereason-quota-exceeded">Background Fetch: 6.4 BackgroundFetchRegistration</see>
    /// </remarks>
    [Description("@#quota-exceeded")]
    QuotaExceeded = 4,

    /// <summary>
    /// JavaScript 字符串取值 “download-total-exceeded”；属于 BackgroundFetchFailureReason 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/background-fetch/#dom-backgroundfetchfailurereason-download-total-exceeded">Background Fetch: 6.4 BackgroundFetchRegistration</see>
    /// </remarks>
    [Description("@#download-total-exceeded")]
    DownloadTotalExceeded = 5
}

/// <summary>
/// WebIDL enum BackgroundFetchResult。定义于 Background Fetch。
/// </summary>
/// <remarks>
/// <see href="https://wicg.github.io/background-fetch/#enumdef-backgroundfetchresult">Background Fetch: 6.4 BackgroundFetchRegistration</see>
/// </remarks>
[Description("@#BackgroundFetchResult")]
[ECMAScript]
[String]
public enum BackgroundFetchResult
{
    /// <summary>
    /// JavaScript 字符串取值 “”；属于 BackgroundFetchResult 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/background-fetch/">Background Fetch: BackgroundFetchResult.</see>
    /// </remarks>
[Description("@#")]
    Empty = 0,

    /// <summary>
    /// JavaScript 字符串取值 “success”；属于 BackgroundFetchResult 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/background-fetch/#dom-backgroundfetchresult-success">Background Fetch: 6.4 BackgroundFetchRegistration</see>
    /// </remarks>
    [Description("@#success")]
    Success = 1,

    /// <summary>
    /// JavaScript 字符串取值 “failure”；属于 BackgroundFetchResult 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/background-fetch/#dom-backgroundfetchresult-failure">Background Fetch: 6.4 BackgroundFetchRegistration</see>
    /// </remarks>
    [Description("@#failure")]
    Failure = 2
}

/// <summary>
/// WebIDL enum ConnectionAllowlistDisposition。定义于 Connection Allowlists。
/// </summary>
/// <remarks>
/// <see href="https://wicg.github.io/connection-allowlists/#enumdef-connectionallowlistdisposition">Connection Allowlists: 3.3 Reporting</see>
/// </remarks>
[Description("@#ConnectionAllowlistDisposition")]
[ECMAScript]
[String]
public enum ConnectionAllowlistDisposition
{
    /// <summary>
    /// JavaScript 字符串取值 “enforce”；属于 ConnectionAllowlistDisposition 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/connection-allowlists/#dom-connectionallowlistdisposition-enforce">Connection Allowlists: 3.3 Reporting</see>
    /// </remarks>
    [Description("@#enforce")]
    Enforce = 0,

    /// <summary>
    /// JavaScript 字符串取值 “report”；属于 ConnectionAllowlistDisposition 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/connection-allowlists/#dom-connectionallowlistdisposition-report">Connection Allowlists: 3.3 Reporting</see>
    /// </remarks>
    [Description("@#report")]
    Report = 1
}

/// <summary>
/// WebIDL enum ContentCategory。定义于 Content Index。
/// </summary>
/// <remarks>
/// <see href="https://wicg.github.io/content-index/spec/#enumdef-contentcategory">Content Index: 5.3 ContentIndex</see>
/// </remarks>
[Description("@#ContentCategory")]
[ECMAScript]
[String]
public enum ContentCategory
{
    /// <summary>
    /// JavaScript 字符串取值 “”；属于 ContentCategory 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/content-index/spec/">Content Index: ContentCategory.</see>
    /// </remarks>
[Description("@#")]
    Empty = 0,

    /// <summary>
    /// JavaScript 字符串取值 “homepage”；属于 ContentCategory 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/content-index/spec/#dom-contentcategory-homepage">Content Index: 5.3 ContentIndex</see>
    /// </remarks>
    [Description("@#homepage")]
    Homepage = 1,

    /// <summary>
    /// JavaScript 字符串取值 “article”；属于 ContentCategory 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/content-index/spec/#dom-contentcategory-article">Content Index: 5.3 ContentIndex</see>
    /// </remarks>
    [Description("@#article")]
    Article = 2,

    /// <summary>
    /// JavaScript 字符串取值 “video”；属于 ContentCategory 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/content-index/spec/#dom-contentcategory-video">Content Index: 5.3 ContentIndex</see>
    /// </remarks>
    [Description("@#video")]
    Video = 3,

    /// <summary>
    /// JavaScript 字符串取值 “audio”；属于 ContentCategory 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/content-index/spec/#dom-contentcategory-audio">Content Index: 5.3 ContentIndex</see>
    /// </remarks>
    [Description("@#audio")]
    Audio = 4
}

/// <summary>
/// WebIDL enum ScriptingPolicyViolationType。定义于 Scripting Policy。
/// </summary>
/// <remarks>
/// <see href="https://wicg.github.io/csp-next/scripting-policy.html#enumdef-scriptingpolicyviolationtype">Scripting Policy: 2.7 Reporting Violations</see>
/// </remarks>
[Description("@#ScriptingPolicyViolationType")]
[ECMAScript]
[String]
public enum ScriptingPolicyViolationType
{
    /// <summary>
    /// JavaScript 字符串取值 “externalScript”；属于 ScriptingPolicyViolationType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/csp-next/scripting-policy.html#dom-scriptingpolicyviolationtype-externalscript">Scripting Policy: 2.7 Reporting Violations</see>
    /// </remarks>
    [Description("@#externalScript")]
    ExternalScript = 0,

    /// <summary>
    /// JavaScript 字符串取值 “inlineScript”；属于 ScriptingPolicyViolationType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/csp-next/scripting-policy.html#dom-scriptingpolicyviolationtype-inlinescript">Scripting Policy: 2.7 Reporting Violations</see>
    /// </remarks>
    [Description("@#inlineScript")]
    InlineScript = 1,

    /// <summary>
    /// JavaScript 字符串取值 “inlineEventHandler”；属于 ScriptingPolicyViolationType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/csp-next/scripting-policy.html#dom-scriptingpolicyviolationtype-inlineeventhandler">Scripting Policy: 2.7 Reporting Violations</see>
    /// </remarks>
    [Description("@#inlineEventHandler")]
    InlineEventHandler = 2,

    /// <summary>
    /// JavaScript 字符串取值 “eval”；属于 ScriptingPolicyViolationType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/csp-next/scripting-policy.html#dom-scriptingpolicyviolationtype-eval">Scripting Policy: 2.7 Reporting Violations</see>
    /// </remarks>
    [Description("@#eval")]
    Eval = 3
}

/// <summary>
/// WebIDL enum ItemType。定义于 Digital Goods API。
/// </summary>
/// <remarks>
/// <see href="https://wicg.github.io/digital-goods/#enumdef-itemtype">Digital Goods API: 2.2 DigitalGoodsService interface</see>
/// </remarks>
[Description("@#ItemType")]
[ECMAScript]
[String]
public enum ItemType
{
    /// <summary>
    /// JavaScript 字符串取值 “product”；属于 ItemType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/digital-goods/#dom-itemtype-product">Digital Goods API: 2.2 DigitalGoodsService interface</see>
    /// </remarks>
    [Description("@#product")]
    Product = 0,

    /// <summary>
    /// JavaScript 字符串取值 “subscription”；属于 ItemType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/digital-goods/#dom-itemtype-subscription">Digital Goods API: 2.2 DigitalGoodsService interface</see>
    /// </remarks>
    [Description("@#subscription")]
    Subscription = 1
}

/// <summary>
/// WebIDL enum FenceReportingDestination。定义于 Fenced Frame。
/// </summary>
/// <remarks>
/// <see href="https://wicg.github.io/fenced-frame/#enumdef-fencereportingdestination">Fenced Frame: 2.4 The Fence interface</see>
/// </remarks>
[Description("@#FenceReportingDestination")]
[ECMAScript]
[String]
public enum FenceReportingDestination
{
    /// <summary>
    /// JavaScript 字符串取值 “buyer”；属于 FenceReportingDestination 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/fenced-frame/#dom-fencereportingdestination-buyer">Fenced Frame: 2.4 The Fence interface</see>
    /// </remarks>
    [Description("@#buyer")]
    Buyer = 0,

    /// <summary>
    /// JavaScript 字符串取值 “seller”；属于 FenceReportingDestination 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/fenced-frame/#dom-fencereportingdestination-seller">Fenced Frame: 2.4 The Fence interface</see>
    /// </remarks>
    [Description("@#seller")]
    Seller = 1,

    /// <summary>
    /// JavaScript 字符串取值 “component-seller”；属于 FenceReportingDestination 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/fenced-frame/#dom-fencereportingdestination-component-seller">Fenced Frame: 2.4 The Fence interface</see>
    /// </remarks>
    [Description("@#component-seller")]
    ComponentSeller = 2,

    /// <summary>
    /// JavaScript 字符串取值 “direct-seller”；属于 FenceReportingDestination 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/fenced-frame/#dom-fencereportingdestination-direct-seller">Fenced Frame: 2.4 The Fence interface</see>
    /// </remarks>
    [Description("@#direct-seller")]
    DirectSeller = 3,

    /// <summary>
    /// JavaScript 字符串取值 “shared-storage-select-url”；属于 FenceReportingDestination 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/fenced-frame/#dom-fencereportingdestination-shared-storage-select-url">Fenced Frame: 2.4 The Fence interface</see>
    /// </remarks>
    [Description("@#shared-storage-select-url")]
    SharedStorageSelectUrl = 4
}

/// <summary>
/// WebIDL enum OpaqueProperty。定义于 Fenced Frame。
/// </summary>
/// <remarks>
/// <see href="https://wicg.github.io/fenced-frame/#enumdef-opaqueproperty">Fenced Frame: 2.3.5 The FencedFrameConfig interface</see>
/// </remarks>
[Description("@#OpaqueProperty")]
[ECMAScript]
[String]
public enum OpaqueProperty
{
    /// <summary>
    /// JavaScript 字符串取值 “opaque”；属于 OpaqueProperty 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/fenced-frame/#dom-opaqueproperty-opaque">Fenced Frame: 2.3.5 The FencedFrameConfig interface</see>
    /// </remarks>
    [Description("@#opaque")]
    Opaque = 0
}

/// <summary>
/// WebIDL enum FileSystemPermissionMode。定义于 File System Access。
/// </summary>
/// <remarks>
/// <see href="https://wicg.github.io/file-system-access/#enumdef-filesystempermissionmode">File System Access: 2.2 Permissions</see>
/// </remarks>
[Description("@#FileSystemPermissionMode")]
[ECMAScript]
[String]
public enum FileSystemPermissionMode
{
    /// <summary>
    /// JavaScript 字符串取值 “read”；属于 FileSystemPermissionMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/file-system-access/#dom-filesystempermissionmode-read">File System Access: 2.2 Permissions</see>
    /// </remarks>
    /// <example>
    /// <code>status = await handle.queryPermission({ mode : &quot;read&quot; })</code>
    /// </example>
    [Description("@#read")]
    Read = 0,

    /// <summary>
    /// JavaScript 字符串取值 “readwrite”；属于 FileSystemPermissionMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/file-system-access/#dom-filesystempermissionmode-readwrite">File System Access: 2.2 Permissions</see>
    /// </remarks>
    /// <example>
    /// <code>status = await handle.queryPermission({ mode : &quot;readwrite&quot; })</code>
    /// </example>
    [Description("@#readwrite")]
    Readwrite = 1
}

/// <summary>
/// WebIDL enum WellKnownDirectory。定义于 File System Access。
/// </summary>
/// <remarks>
/// <see href="https://wicg.github.io/file-system-access/#enumdef-wellknowndirectory">File System Access: 3.2.2 Starting Directory</see>
/// </remarks>
[Description("@#WellKnownDirectory")]
[ECMAScript]
[String]
public enum WellKnownDirectory
{
    /// <summary>
    /// JavaScript 字符串取值 “desktop”；属于 WellKnownDirectory 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/file-system-access/#dom-wellknowndirectory-desktop">File System Access: 3.2.2 Starting Directory</see>
    /// </remarks>
    [Description("@#desktop")]
    Desktop = 0,

    /// <summary>
    /// JavaScript 字符串取值 “documents”；属于 WellKnownDirectory 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/file-system-access/#dom-wellknowndirectory-documents">File System Access: 3.2.2 Starting Directory</see>
    /// </remarks>
    [Description("@#documents")]
    Documents = 1,

    /// <summary>
    /// JavaScript 字符串取值 “downloads”；属于 WellKnownDirectory 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/file-system-access/#dom-wellknowndirectory-downloads">File System Access: 3.2.2 Starting Directory</see>
    /// </remarks>
    [Description("@#downloads")]
    Downloads = 2,

    /// <summary>
    /// JavaScript 字符串取值 “music”；属于 WellKnownDirectory 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/file-system-access/#dom-wellknowndirectory-music">File System Access: 3.2.2 Starting Directory</see>
    /// </remarks>
    [Description("@#music")]
    Music = 3,

    /// <summary>
    /// JavaScript 字符串取值 “pictures”；属于 WellKnownDirectory 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/file-system-access/#dom-wellknowndirectory-pictures">File System Access: 3.2.2 Starting Directory</see>
    /// </remarks>
    [Description("@#pictures")]
    Pictures = 4,

    /// <summary>
    /// JavaScript 字符串取值 “videos”；属于 WellKnownDirectory 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/file-system-access/#dom-wellknowndirectory-videos">File System Access: 3.2.2 Starting Directory</see>
    /// </remarks>
    [Description("@#videos")]
    Videos = 5
}

/// <summary>
/// WebIDL enum ScreenIdleState。定义于 Idle Detection API。
/// </summary>
/// <remarks>
/// <see href="https://wicg.github.io/idle-detection/#enumdef-screenidlestate">Idle Detection API: 2.1.2 The ScreenIdleState enum</see>
/// </remarks>
[Description("@#ScreenIdleState")]
[ECMAScript]
[String]
public enum ScreenIdleState
{
    /// <summary>
    /// JavaScript 字符串取值 “locked”；属于 ScreenIdleState 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/idle-detection/#dom-screenidlestate-locked">Idle Detection API: 2.1.2 The ScreenIdleState enum</see>
    /// </remarks>
    [Description("@#locked")]
    Locked = 0,

    /// <summary>
    /// JavaScript 字符串取值 “unlocked”；属于 ScreenIdleState 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/idle-detection/#dom-screenidlestate-unlocked">Idle Detection API: 2.1.2 The ScreenIdleState enum</see>
    /// </remarks>
    [Description("@#unlocked")]
    Unlocked = 1
}

/// <summary>
/// WebIDL enum UserIdleState。定义于 Idle Detection API。
/// </summary>
/// <remarks>
/// <see href="https://wicg.github.io/idle-detection/#enumdef-useridlestate">Idle Detection API: 2.1.1 The UserIdleState enum</see>
/// </remarks>
[Description("@#UserIdleState")]
[ECMAScript]
[String]
public enum UserIdleState
{
    /// <summary>
    /// JavaScript 字符串取值 “active”；属于 UserIdleState 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/idle-detection/#dom-useridlestate-active">Idle Detection API: 2.1.1 The UserIdleState enum</see>
    /// </remarks>
    [Description("@#active")]
    Active = 0,

    /// <summary>
    /// JavaScript 字符串取值 “idle”；属于 UserIdleState 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/idle-detection/#dom-useridlestate-idle">Idle Detection API: 2.1.1 The UserIdleState enum</see>
    /// </remarks>
    [Description("@#idle")]
    Idle = 1
}

/// <summary>
/// WebIDL enum IPAddressSpace。定义于 Local Network Access。
/// </summary>
/// <remarks>
/// <see href="https://wicg.github.io/local-network-access/#enumdef-ipaddressspace">Local Network Access: 2.1 IP Address Space</see>
/// </remarks>
[Description("@#IPAddressSpace")]
[ECMAScript]
[String]
public enum IPAddressSpace
{
    /// <summary>
    /// JavaScript 字符串取值 “public”；属于 IPAddressSpace 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/local-network-access/#dom-ipaddressspace-public">Local Network Access: 2.1 IP Address Space</see>
    /// </remarks>
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
    /// JavaScript 字符串取值 “loopback”；属于 IPAddressSpace 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/local-network-access/#dom-ipaddressspace-loopback">Local Network Access: 2.1 IP Address Space</see>
    /// </remarks>
    [Description("@#loopback")]
    Loopback = 2
}

/// <summary>
/// WebIDL enum ConnectionType。定义于 Network Information API。
/// </summary>
/// <remarks>
/// <see href="https://wicg.github.io/netinfo/#dom-connectiontype">Network Information API: 4.2 ConnectionType enum</see>
/// </remarks>
[Description("@#ConnectionType")]
[ECMAScript]
[String]
public enum ConnectionType
{
    /// <summary>
    /// JavaScript 字符串取值 “bluetooth”；属于 ConnectionType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/netinfo/#dom-connectiontype-bluetooth">Network Information API: 4.1 Underlying connection technology</see>
    /// </remarks>
    [Description("@#bluetooth")]
    Bluetooth = 0,

    /// <summary>
    /// JavaScript 字符串取值 “cellular”；属于 ConnectionType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/netinfo/#dom-connectiontype-cellular">Network Information API: 4.1 Underlying connection technology</see>
    /// </remarks>
    [Description("@#cellular")]
    Cellular = 1,

    /// <summary>
    /// JavaScript 字符串取值 “ethernet”；属于 ConnectionType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/netinfo/#dom-connectiontype-ethernet">Network Information API: 4.1 Underlying connection technology</see>
    /// </remarks>
    [Description("@#ethernet")]
    Ethernet = 2,

    /// <summary>
    /// JavaScript 字符串取值 “mixed”；属于 ConnectionType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/netinfo/#dom-connectiontype-mixed">Network Information API: 4.1 Underlying connection technology</see>
    /// </remarks>
    [Description("@#mixed")]
    Mixed = 3,

    /// <summary>
    /// JavaScript 字符串取值 “none”；属于 ConnectionType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/netinfo/#dom-connectiontype-none">Network Information API: 4.1 Underlying connection technology</see>
    /// </remarks>
    [Description("@#none")]
    None = 4,

    /// <summary>
    /// JavaScript 字符串取值 “other”；属于 ConnectionType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/netinfo/#dom-connectiontype-other">Network Information API: 4.1 Underlying connection technology</see>
    /// </remarks>
    [Description("@#other")]
    Other = 5,

    /// <summary>
    /// JavaScript 字符串取值 “unknown”；属于 ConnectionType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/netinfo/#dom-connectiontype-unknown">Network Information API: 4.1 Underlying connection technology</see>
    /// </remarks>
    [Description("@#unknown")]
    Unknown = 6,

    /// <summary>
    /// JavaScript 字符串取值 “wifi”；属于 ConnectionType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/netinfo/#dom-connectiontype-wifi">Network Information API: 4.1 Underlying connection technology</see>
    /// </remarks>
    [Description("@#wifi")]
    Wifi = 7,

    /// <summary>
    /// JavaScript 字符串取值 “wimax”；属于 ConnectionType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/netinfo/#dom-connectiontype-wimax">Network Information API: 4.1 Underlying connection technology</see>
    /// </remarks>
    [Description("@#wimax")]
    Wimax = 8
}

/// <summary>
/// WebIDL enum EffectiveConnectionType。定义于 Network Information API。
/// </summary>
/// <remarks>
/// <see href="https://wicg.github.io/netinfo/#dom-effectiveconnectiontype">Network Information API: 4.4 EffectiveConnectionType enum</see>
/// </remarks>
[Description("@#EffectiveConnectionType")]
[ECMAScript]
[String]
public enum EffectiveConnectionType
{
    /// <summary>
    /// JavaScript 字符串取值 “2g”；属于 EffectiveConnectionType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/netinfo/#dom-effectiveconnectiontype-2g">Network Information API: 4.3 Effective connection types</see>
    /// </remarks>
    [Description("@#2g")]
    _2g = 0,

    /// <summary>
    /// JavaScript 字符串取值 “3g”；属于 EffectiveConnectionType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/netinfo/#dom-effectiveconnectiontype-3g">Network Information API: 4.3 Effective connection types</see>
    /// </remarks>
    [Description("@#3g")]
    _3g = 1,

    /// <summary>
    /// JavaScript 字符串取值 “4g”；属于 EffectiveConnectionType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/netinfo/#dom-effectiveconnectiontype-4g">Network Information API: 4.3 Effective connection types</see>
    /// </remarks>
    [Description("@#4g")]
    _4g = 2,

    /// <summary>
    /// JavaScript 字符串取值 “slow-2g”；属于 EffectiveConnectionType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/netinfo/#dom-effectiveconnectiontype-slow-2g">Network Information API: 4.3 Effective connection types</see>
    /// </remarks>
    [Description("@#slow-2g")]
    Slow2g = 3
}

/// <summary>
/// WebIDL enum ClientLifecycleState。定义于 Page Lifecycle。
/// </summary>
/// <remarks>
/// <see href="https://wicg.github.io/page-lifecycle/#enumdef-clientlifecyclestate">Page Lifecycle: 5.3.1 Client</see>
/// </remarks>
[Description("@#ClientLifecycleState")]
[ECMAScript]
[String]
public enum ClientLifecycleState
{
    /// <summary>
    /// JavaScript 字符串取值 “active”；属于 ClientLifecycleState 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/page-lifecycle/#dom-clientlifecyclestate-active">Page Lifecycle: 5.3.1 Client</see>
    /// </remarks>
    [Description("@#active")]
    Active = 0,

    /// <summary>
    /// JavaScript 字符串取值 “frozen”；属于 ClientLifecycleState 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/page-lifecycle/#dom-clientlifecyclestate-frozen">Page Lifecycle: 5.3.1 Client</see>
    /// </remarks>
    [Description("@#frozen")]
    Frozen = 1
}

/// <summary>
/// WebIDL enum TaskPriority。定义于 Prioritized Task Scheduling。
/// </summary>
/// <remarks>
/// <see href="https://wicg.github.io/scheduling-apis/#enumdef-taskpriority">Prioritized Task Scheduling: 2.1 Task and Continuation Priorities</see>
/// </remarks>
[Description("@#TaskPriority")]
[ECMAScript]
[String]
public enum TaskPriority
{
    /// <summary>
    /// JavaScript 字符串取值 “user-blocking”；属于 TaskPriority 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/scheduling-apis/#dom-taskpriority-user-blocking">Prioritized Task Scheduling: 2.1 Task and Continuation Priorities</see>
    /// </remarks>
    [Description("@#user-blocking")]
    UserBlocking = 0,

    /// <summary>
    /// JavaScript 字符串取值 “user-visible”；属于 TaskPriority 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/scheduling-apis/#dom-taskpriority-user-visible">Prioritized Task Scheduling: 2.1 Task and Continuation Priorities</see>
    /// </remarks>
    [Description("@#user-visible")]
    UserVisible = 1,

    /// <summary>
    /// JavaScript 字符串取值 “background”；属于 TaskPriority 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/scheduling-apis/#dom-taskpriority-background">Prioritized Task Scheduling: 2.1 Task and Continuation Priorities</see>
    /// </remarks>
    [Description("@#background")]
    Background = 2
}

/// <summary>
/// WebIDL enum FlowControlType。定义于 Web Serial API。
/// </summary>
/// <remarks>
/// <see href="https://wicg.github.io/serial/#dom-flowcontroltype">Web Serial API: 4.4.1.2 FlowControlType enum</see>
/// </remarks>
[Description("@#FlowControlType")]
[ECMAScript]
[String]
public enum FlowControlType
{
    /// <summary>
    /// JavaScript 字符串取值 “none”；属于 FlowControlType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/serial/#dom-flowcontroltype-none">Web Serial API: 4.4.1.2 FlowControlType enum</see>
    /// </remarks>
    [Description("@#none")]
    None = 0,

    /// <summary>
    /// JavaScript 字符串取值 “hardware”；属于 FlowControlType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/serial/#dom-flowcontroltype-hardware">Web Serial API: 4.4.1.2 FlowControlType enum</see>
    /// </remarks>
    [Description("@#hardware")]
    Hardware = 1
}

/// <summary>
/// WebIDL enum ParityType。定义于 Web Serial API。
/// </summary>
/// <remarks>
/// <see href="https://wicg.github.io/serial/#dom-paritytype">Web Serial API: 4.4.1.1 ParityType enum</see>
/// </remarks>
[Description("@#ParityType")]
[ECMAScript]
[String]
public enum ParityType
{
    /// <summary>
    /// JavaScript 字符串取值 “none”；属于 ParityType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/serial/#dom-paritytype-none">Web Serial API: 4.4.1.1 ParityType enum</see>
    /// </remarks>
    [Description("@#none")]
    None = 0,

    /// <summary>
    /// JavaScript 字符串取值 “even”；属于 ParityType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/serial/#dom-paritytype-even">Web Serial API: 4.4.1.1 ParityType enum</see>
    /// </remarks>
    [Description("@#even")]
    Even = 1,

    /// <summary>
    /// JavaScript 字符串取值 “odd”；属于 ParityType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/serial/#dom-paritytype-odd">Web Serial API: 4.4.1.1 ParityType enum</see>
    /// </remarks>
    [Description("@#odd")]
    Odd = 2
}

/// <summary>
/// WebIDL enum BarcodeFormat。定义于 Accelerated Shape Detection in Images。
/// </summary>
/// <remarks>
/// <see href="https://wicg.github.io/shape-detection-api/#enumdef-barcodeformat">Accelerated Shape Detection in Images: 2.3.3 BarcodeFormat</see>
/// </remarks>
[Description("@#BarcodeFormat")]
[ECMAScript]
[String]
public enum BarcodeFormat
{
    /// <summary>
    /// JavaScript 字符串取值 “aztec”；属于 BarcodeFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/shape-detection-api/#dom-barcodeformat-aztec">Accelerated Shape Detection in Images: 2.3.3 BarcodeFormat</see>
    /// </remarks>
    [Description("@#aztec")]
    Aztec = 0,

    /// <summary>
    /// JavaScript 字符串取值 “code_128”；属于 BarcodeFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/shape-detection-api/#dom-barcodeformat-code_128">Accelerated Shape Detection in Images: 2.3.3 BarcodeFormat</see>
    /// </remarks>
    [Description("@#code_128")]
    Code128 = 1,

    /// <summary>
    /// JavaScript 字符串取值 “code_39”；属于 BarcodeFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/shape-detection-api/#dom-barcodeformat-code_39">Accelerated Shape Detection in Images: 2.3.3 BarcodeFormat</see>
    /// </remarks>
    [Description("@#code_39")]
    Code39 = 2,

    /// <summary>
    /// JavaScript 字符串取值 “code_93”；属于 BarcodeFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/shape-detection-api/#dom-barcodeformat-code_93">Accelerated Shape Detection in Images: 2.3.3 BarcodeFormat</see>
    /// </remarks>
    [Description("@#code_93")]
    Code93 = 3,

    /// <summary>
    /// JavaScript 字符串取值 “codabar”；属于 BarcodeFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/shape-detection-api/#dom-barcodeformat-codabar">Accelerated Shape Detection in Images: 2.3.3 BarcodeFormat</see>
    /// </remarks>
    [Description("@#codabar")]
    Codabar = 4,

    /// <summary>
    /// JavaScript 字符串取值 “data_matrix”；属于 BarcodeFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/shape-detection-api/#dom-barcodeformat-data_matrix">Accelerated Shape Detection in Images: 2.3.3 BarcodeFormat</see>
    /// </remarks>
    [Description("@#data_matrix")]
    DataMatrix = 5,

    /// <summary>
    /// JavaScript 字符串取值 “ean_13”；属于 BarcodeFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/shape-detection-api/#dom-barcodeformat-ean_13">Accelerated Shape Detection in Images: 2.3.3 BarcodeFormat</see>
    /// </remarks>
    [Description("@#ean_13")]
    Ean13 = 6,

    /// <summary>
    /// JavaScript 字符串取值 “ean_8”；属于 BarcodeFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/shape-detection-api/#dom-barcodeformat-ean_8">Accelerated Shape Detection in Images: 2.3.3 BarcodeFormat</see>
    /// </remarks>
    [Description("@#ean_8")]
    Ean8 = 7,

    /// <summary>
    /// JavaScript 字符串取值 “itf”；属于 BarcodeFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/shape-detection-api/#dom-barcodeformat-itf">Accelerated Shape Detection in Images: 2.3.3 BarcodeFormat</see>
    /// </remarks>
    [Description("@#itf")]
    Itf = 8,

    /// <summary>
    /// JavaScript 字符串取值 “pdf417”；属于 BarcodeFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/shape-detection-api/#dom-barcodeformat-pdf417">Accelerated Shape Detection in Images: 2.3.3 BarcodeFormat</see>
    /// </remarks>
    [Description("@#pdf417")]
    Pdf417 = 9,

    /// <summary>
    /// JavaScript 字符串取值 “qr_code”；属于 BarcodeFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/shape-detection-api/#dom-barcodeformat-qr_code">Accelerated Shape Detection in Images: 2.3.3 BarcodeFormat</see>
    /// </remarks>
    [Description("@#qr_code")]
    QrCode = 10,

    /// <summary>
    /// JavaScript 字符串取值 “unknown”；属于 BarcodeFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/shape-detection-api/#dom-barcodeformat-unknown">Accelerated Shape Detection in Images: 2.3.3 BarcodeFormat</see>
    /// </remarks>
    [Description("@#unknown")]
    Unknown = 11,

    /// <summary>
    /// JavaScript 字符串取值 “upc_a”；属于 BarcodeFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/shape-detection-api/#dom-barcodeformat-upc_a">Accelerated Shape Detection in Images: 2.3.3 BarcodeFormat</see>
    /// </remarks>
    [Description("@#upc_a")]
    UpcA = 12,

    /// <summary>
    /// JavaScript 字符串取值 “upc_e”；属于 BarcodeFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/shape-detection-api/#dom-barcodeformat-upc_e">Accelerated Shape Detection in Images: 2.3.3 BarcodeFormat</see>
    /// </remarks>
    [Description("@#upc_e")]
    UpcE = 13
}

/// <summary>
/// WebIDL enum LandmarkType。定义于 Accelerated Shape Detection in Images。
/// </summary>
/// <remarks>
/// <see href="https://wicg.github.io/shape-detection-api/#enumdef-landmarktype">Accelerated Shape Detection in Images: 2.2.2 DetectedFace</see>
/// </remarks>
[Description("@#LandmarkType")]
[ECMAScript]
[String]
public enum LandmarkType
{
    /// <summary>
    /// JavaScript 字符串取值 “mouth”；属于 LandmarkType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/shape-detection-api/#dom-landmarktype-mouth">Accelerated Shape Detection in Images: 2.2.2 DetectedFace</see>
    /// </remarks>
    [Description("@#mouth")]
    Mouth = 0,

    /// <summary>
    /// JavaScript 字符串取值 “eye”；属于 LandmarkType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/shape-detection-api/#dom-landmarktype-eye">Accelerated Shape Detection in Images: 2.2.2 DetectedFace</see>
    /// </remarks>
    [Description("@#eye")]
    Eye = 1,

    /// <summary>
    /// JavaScript 字符串取值 “nose”；属于 LandmarkType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/shape-detection-api/#dom-landmarktype-nose">Accelerated Shape Detection in Images: 2.2.2 DetectedFace</see>
    /// </remarks>
    [Description("@#nose")]
    Nose = 2
}

/// <summary>
/// WebIDL enum OperationType。定义于 Private State Token API。
/// </summary>
/// <remarks>
/// <see href="https://wicg.github.io/trust-token-api/#enumdef-operationtype">Private State Token API: Private State Token API</see>
/// </remarks>
[Description("@#OperationType")]
[ECMAScript]
[String]
public enum OperationType
{
    /// <summary>
    /// JavaScript 字符串取值 “token-request”；属于 OperationType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/trust-token-api/#dom-operationtype-token-request">Private State Token API: Private State Token API</see>
    /// </remarks>
    [Description("@#token-request")]
    TokenRequest = 0,

    /// <summary>
    /// JavaScript 字符串取值 “send-redemption-record”；属于 OperationType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/trust-token-api/#dom-operationtype-send-redemption-record">Private State Token API: Private State Token API</see>
    /// </remarks>
    [Description("@#send-redemption-record")]
    SendRedemptionRecord = 1,

    /// <summary>
    /// JavaScript 字符串取值 “token-redemption”；属于 OperationType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/trust-token-api/#dom-operationtype-token-redemption">Private State Token API: Private State Token API</see>
    /// </remarks>
    [Description("@#token-redemption")]
    TokenRedemption = 2
}

/// <summary>
/// WebIDL enum RefreshPolicy。定义于 Private State Token API。
/// </summary>
/// <remarks>
/// <see href="https://wicg.github.io/trust-token-api/#enumdef-refreshpolicy">Private State Token API: 6.1 Definitions</see>
/// </remarks>
[Description("@#RefreshPolicy")]
[ECMAScript]
[String]
public enum RefreshPolicy
{
    /// <summary>
    /// JavaScript 字符串取值 “none”；属于 RefreshPolicy 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/trust-token-api/#dom-refreshpolicy-none">Private State Token API: Private State Token API</see>
    /// </remarks>
    [Description("@#none")]
    None = 0,

    /// <summary>
    /// JavaScript 字符串取值 “refresh”；属于 RefreshPolicy 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/trust-token-api/#dom-refreshpolicy-refresh">Private State Token API: Private State Token API</see>
    /// </remarks>
    [Description("@#refresh")]
    Refresh = 1
}

/// <summary>
/// WebIDL enum TokenVersion。定义于 Private State Token API。
/// </summary>
/// <remarks>
/// <see href="https://wicg.github.io/trust-token-api/#enumdef-tokenversion">Private State Token API: Private State Token API</see>
/// </remarks>
[Description("@#TokenVersion")]
[ECMAScript]
[String]
public enum TokenVersion
{
    /// <summary>
    /// JavaScript 字符串取值 “1”；属于 TokenVersion 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/trust-token-api/#dom-tokenversion-1">Private State Token API: Private State Token API</see>
    /// </remarks>
    [Description("@#1")]
    _1 = 0
}

/// <summary>
/// WebIDL enum OTPCredentialTransportType。定义于 WebOTP API。
/// </summary>
/// <remarks>
/// <see href="https://wicg.github.io/web-otp/#enumdef-otpcredentialtransporttype">WebOTP API: 2.4 OTPCredentialTransportType</see>
/// </remarks>
[Description("@#OTPCredentialTransportType")]
[ECMAScript]
[String]
public enum OTPCredentialTransportType
{
    /// <summary>
    /// JavaScript 字符串取值 “sms”；属于 OTPCredentialTransportType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/web-otp/#dom-otpcredentialtransporttype-sms">WebOTP API: 2.4 OTPCredentialTransportType</see>
    /// </remarks>
    [Description("@#sms")]
    Sms = 0
}

/// <summary>
/// WebIDL enum KeyFormat。定义于 Modern Algorithms in the Web Cryptography API。
/// </summary>
/// <remarks>
/// <see href="https://wicg.github.io/webcrypto-modern-algos/#dom-keyformat">Modern Algorithms in the Web Cryptography API: 3.1.1 Key Formats</see>
/// </remarks>
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
    /// JavaScript 字符串取值 “raw-private”；属于 KeyFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/webcrypto-modern-algos/#dom-keyformat-raw-private">Modern Algorithms in the Web Cryptography API: 3.1.1 Key Formats</see>
    /// </remarks>
    [Description("@#raw-private")]
    RawPrivate = 1,

    /// <summary>
    /// JavaScript 字符串取值 “raw-seed”；属于 KeyFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/webcrypto-modern-algos/#dom-keyformat-raw-seed">Modern Algorithms in the Web Cryptography API: 3.1.1 Key Formats</see>
    /// </remarks>
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
    /// JavaScript 字符串取值 “spki”；属于 KeyFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/webcrypto-modern-algos/#dom-keyformat-spki">Modern Algorithms in the Web Cryptography API: 3.1.1 Key Formats</see>
    /// </remarks>
    [Description("@#spki")]
    Spki = 5,

    /// <summary>
    /// JavaScript 字符串取值 “pkcs8”；属于 KeyFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/webcrypto-modern-algos/#dom-keyformat-pkcs8">Modern Algorithms in the Web Cryptography API: 3.1.1 Key Formats</see>
    /// </remarks>
    [Description("@#pkcs8")]
    Pkcs8 = 6,

    /// <summary>
    /// JavaScript 字符串取值 “jwk”；属于 KeyFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/webcrypto-modern-algos/#dom-keyformat-jwk">Modern Algorithms in the Web Cryptography API: 3.1.1 Key Formats</see>
    /// </remarks>
    [Description("@#jwk")]
    Jwk = 7
}

/// <summary>
/// WebIDL enum KeyUsage。定义于 Modern Algorithms in the Web Cryptography API。
/// </summary>
/// <remarks>
/// <see href="https://wicg.github.io/webcrypto-modern-algos/#dom-keyusage">Modern Algorithms in the Web Cryptography API: 3.1.2 Key Usages</see>
/// </remarks>
[Description("@#KeyUsage")]
[ECMAScript]
[String]
public enum KeyUsage
{
    /// <summary>
    /// JavaScript 字符串取值 “encrypt”；属于 KeyUsage 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/webcrypto-modern-algos/#dom-keyusage-encrypt">Modern Algorithms in the Web Cryptography API: 3.1.2 Key Usages</see>
    /// </remarks>
    [Description("@#encrypt")]
    Encrypt = 0,

    /// <summary>
    /// JavaScript 字符串取值 “decrypt”；属于 KeyUsage 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/webcrypto-modern-algos/#dom-keyusage-decrypt">Modern Algorithms in the Web Cryptography API: 3.1.2 Key Usages</see>
    /// </remarks>
    [Description("@#decrypt")]
    Decrypt = 1,

    /// <summary>
    /// JavaScript 字符串取值 “sign”；属于 KeyUsage 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/webcrypto-modern-algos/#dom-keyusage-sign">Modern Algorithms in the Web Cryptography API: 3.1.2 Key Usages</see>
    /// </remarks>
    [Description("@#sign")]
    Sign = 2,

    /// <summary>
    /// JavaScript 字符串取值 “verify”；属于 KeyUsage 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/webcrypto-modern-algos/#dom-keyusage-verify">Modern Algorithms in the Web Cryptography API: 3.1.2 Key Usages</see>
    /// </remarks>
    [Description("@#verify")]
    Verify = 3,

    /// <summary>
    /// JavaScript 字符串取值 “deriveKey”；属于 KeyUsage 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/webcrypto-modern-algos/#dom-keyusage-derivekey">Modern Algorithms in the Web Cryptography API: 3.1.2 Key Usages</see>
    /// </remarks>
    [Description("@#deriveKey")]
    DeriveKey = 4,

    /// <summary>
    /// JavaScript 字符串取值 “deriveBits”；属于 KeyUsage 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/webcrypto-modern-algos/#dom-keyusage-derivebits">Modern Algorithms in the Web Cryptography API: 3.1.2 Key Usages</see>
    /// </remarks>
    [Description("@#deriveBits")]
    DeriveBits = 5,

    /// <summary>
    /// JavaScript 字符串取值 “wrapKey”；属于 KeyUsage 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/webcrypto-modern-algos/#dom-keyusage-wrapkey">Modern Algorithms in the Web Cryptography API: 3.1.2 Key Usages</see>
    /// </remarks>
    [Description("@#wrapKey")]
    WrapKey = 6,

    /// <summary>
    /// JavaScript 字符串取值 “unwrapKey”；属于 KeyUsage 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/webcrypto-modern-algos/#dom-keyusage-unwrapkey">Modern Algorithms in the Web Cryptography API: 3.1.2 Key Usages</see>
    /// </remarks>
    [Description("@#unwrapKey")]
    UnwrapKey = 7,

    /// <summary>
    /// JavaScript 字符串取值 “encapsulateKey”；属于 KeyUsage 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/webcrypto-modern-algos/#dom-keyusage-encapsulatekey">Modern Algorithms in the Web Cryptography API: 3.1.2 Key Usages</see>
    /// </remarks>
    [Description("@#encapsulateKey")]
    EncapsulateKey = 8,

    /// <summary>
    /// JavaScript 字符串取值 “encapsulateBits”；属于 KeyUsage 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/webcrypto-modern-algos/#dom-keyusage-encapsulatebits">Modern Algorithms in the Web Cryptography API: 3.1.2 Key Usages</see>
    /// </remarks>
    [Description("@#encapsulateBits")]
    EncapsulateBits = 9,

    /// <summary>
    /// JavaScript 字符串取值 “decapsulateKey”；属于 KeyUsage 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/webcrypto-modern-algos/#dom-keyusage-decapsulatekey">Modern Algorithms in the Web Cryptography API: 3.1.2 Key Usages</see>
    /// </remarks>
    [Description("@#decapsulateKey")]
    DecapsulateKey = 10,

    /// <summary>
    /// JavaScript 字符串取值 “decapsulateBits”；属于 KeyUsage 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/webcrypto-modern-algos/#dom-keyusage-decapsulatebits">Modern Algorithms in the Web Cryptography API: 3.1.2 Key Usages</see>
    /// </remarks>
    [Description("@#decapsulateBits")]
    DecapsulateBits = 11
}

/// <summary>
/// WebIDL enum USBDirection。定义于 WebUSB API。
/// </summary>
/// <remarks>
/// <see href="https://wicg.github.io/webusb/#enumdef-usbdirection">WebUSB API: 6.6 The USBEndpoint Interface</see>
/// </remarks>
[Description("@#USBDirection")]
[ECMAScript]
[String]
public enum USBDirection
{
    /// <summary>
    /// JavaScript 字符串取值 “in”；属于 USBDirection 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/webusb/#dom-usbdirection-in">WebUSB API: 6.6 The USBEndpoint Interface</see>
    /// </remarks>
    [Description("@#in")]
    In = 0,

    /// <summary>
    /// JavaScript 字符串取值 “out”；属于 USBDirection 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/webusb/#dom-usbdirection-out">WebUSB API: 6.6 The USBEndpoint Interface</see>
    /// </remarks>
    [Description("@#out")]
    Out = 1
}

/// <summary>
/// WebIDL enum USBEndpointType。定义于 WebUSB API。
/// </summary>
/// <remarks>
/// <see href="https://wicg.github.io/webusb/#enumdef-usbendpointtype">WebUSB API: 6.6 The USBEndpoint Interface</see>
/// </remarks>
[Description("@#USBEndpointType")]
[ECMAScript]
[String]
public enum USBEndpointType
{
    /// <summary>
    /// JavaScript 字符串取值 “bulk”；属于 USBEndpointType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/webusb/#dom-usbendpointtype-bulk">WebUSB API: 6.6 The USBEndpoint Interface</see>
    /// </remarks>
    [Description("@#bulk")]
    Bulk = 0,

    /// <summary>
    /// JavaScript 字符串取值 “interrupt”；属于 USBEndpointType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/webusb/#dom-usbendpointtype-interrupt">WebUSB API: 6.6 The USBEndpoint Interface</see>
    /// </remarks>
    [Description("@#interrupt")]
    Interrupt = 1,

    /// <summary>
    /// JavaScript 字符串取值 “isochronous”；属于 USBEndpointType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/webusb/#dom-usbendpointtype-isochronous">WebUSB API: 6.6 The USBEndpoint Interface</see>
    /// </remarks>
    [Description("@#isochronous")]
    Isochronous = 2
}

/// <summary>
/// WebIDL enum USBRecipient。定义于 WebUSB API。
/// </summary>
/// <remarks>
/// <see href="https://wicg.github.io/webusb/#enumdef-usbrecipient">WebUSB API: 6.2 The USBControlTransferParameters Dictionary</see>
/// </remarks>
[Description("@#USBRecipient")]
[ECMAScript]
[String]
public enum USBRecipient
{
    /// <summary>
    /// JavaScript 字符串取值 “device”；属于 USBRecipient 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/webusb/#dom-usbrecipient-device">WebUSB API: 6.2 The USBControlTransferParameters Dictionary</see>
    /// </remarks>
    [Description("@#device")]
    Device = 0,

    /// <summary>
    /// JavaScript 字符串取值 “interface”；属于 USBRecipient 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/webusb/#dom-usbrecipient-interface">WebUSB API: 6.2 The USBControlTransferParameters Dictionary</see>
    /// </remarks>
    [Description("@#interface")]
    Interface = 1,

    /// <summary>
    /// JavaScript 字符串取值 “endpoint”；属于 USBRecipient 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/webusb/#dom-usbrecipient-endpoint">WebUSB API: 6.2 The USBControlTransferParameters Dictionary</see>
    /// </remarks>
    [Description("@#endpoint")]
    Endpoint = 2,

    /// <summary>
    /// JavaScript 字符串取值 “other”；属于 USBRecipient 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/webusb/#dom-usbrecipient-other">WebUSB API: 6.2 The USBControlTransferParameters Dictionary</see>
    /// </remarks>
    [Description("@#other")]
    Other = 3
}

/// <summary>
/// WebIDL enum USBRequestType。定义于 WebUSB API。
/// </summary>
/// <remarks>
/// <see href="https://wicg.github.io/webusb/#enumdef-usbrequesttype">WebUSB API: 6.2 The USBControlTransferParameters Dictionary</see>
/// </remarks>
[Description("@#USBRequestType")]
[ECMAScript]
[String]
public enum USBRequestType
{
    /// <summary>
    /// JavaScript 字符串取值 “standard”；属于 USBRequestType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/webusb/#dom-usbrequesttype-standard">WebUSB API: 6.2 The USBControlTransferParameters Dictionary</see>
    /// </remarks>
    [Description("@#standard")]
    Standard = 0,

    /// <summary>
    /// JavaScript 字符串取值 “class”；属于 USBRequestType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/webusb/#dom-usbrequesttype-class">WebUSB API: 6.2 The USBControlTransferParameters Dictionary</see>
    /// </remarks>
    [Description("@#class")]
    Class = 1,

    /// <summary>
    /// JavaScript 字符串取值 “vendor”；属于 USBRequestType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/webusb/#dom-usbrequesttype-vendor">WebUSB API: 6.2 The USBControlTransferParameters Dictionary</see>
    /// </remarks>
    [Description("@#vendor")]
    Vendor = 2
}

/// <summary>
/// WebIDL enum USBTransferStatus。定义于 WebUSB API。
/// </summary>
/// <remarks>
/// <see href="https://wicg.github.io/webusb/#enumdef-usbtransferstatus">WebUSB API: 6.1 The USBDevice Interface</see>
/// </remarks>
[Description("@#USBTransferStatus")]
[ECMAScript]
[String]
public enum USBTransferStatus
{
    /// <summary>
    /// JavaScript 字符串取值 “ok”；属于 USBTransferStatus 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/webusb/#dom-usbtransferstatus-ok">WebUSB API: 6.1 The USBDevice Interface</see>
    /// </remarks>
    [Description("@#ok")]
    Ok = 0,

    /// <summary>
    /// JavaScript 字符串取值 “stall”；属于 USBTransferStatus 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/webusb/#dom-usbtransferstatus-stall">WebUSB API: 6.1 The USBDevice Interface</see>
    /// </remarks>
    [Description("@#stall")]
    Stall = 1,

    /// <summary>
    /// JavaScript 字符串取值 “babble”；属于 USBTransferStatus 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/webusb/#dom-usbtransferstatus-babble">WebUSB API: 6.1 The USBDevice Interface</see>
    /// </remarks>
    [Description("@#babble")]
    Babble = 2
}

/// <summary>
/// Note: This feature is available in Web Workers, except for Service Workers. The XMLHttpRequest property responseType is an enumerated string value specifying the type of data contained in the response. It also lets the author change the response type. If an empty string is set as the value of responseType, the default value of text is used.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/XMLHttpRequest/responseType">MDN Web Docs: XMLHttpRequestResponseType</see>
/// </remarks>
[Description("@#XMLHttpRequestResponseType")]
[ECMAScript]
[String]
public enum XMLHttpRequestResponseType
{
    /// <summary>
    /// JavaScript 字符串取值 “”；属于 XMLHttpRequestResponseType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://xhr.spec.whatwg.org/">XMLHttpRequest Standard: XMLHttpRequestResponseType.</see>
    /// </remarks>
[Description("@#")]
    Empty = 0,

    /// <summary>
    /// JavaScript 字符串取值 “arraybuffer”；属于 XMLHttpRequestResponseType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://xhr.spec.whatwg.org/#dom-xmlhttprequestresponsetype-arraybuffer">XMLHttpRequest Standard: 3 Interface XMLHttpRequest</see>
    /// </remarks>
    [Description("@#arraybuffer")]
    Arraybuffer = 1,

    /// <summary>
    /// JavaScript 字符串取值 “blob”；属于 XMLHttpRequestResponseType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://xhr.spec.whatwg.org/#dom-xmlhttprequestresponsetype-blob">XMLHttpRequest Standard: 3 Interface XMLHttpRequest</see>
    /// </remarks>
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
    /// JavaScript 字符串取值 “json”；属于 XMLHttpRequestResponseType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://xhr.spec.whatwg.org/#dom-xmlhttprequestresponsetype-json">XMLHttpRequest Standard: 3 Interface XMLHttpRequest</see>
    /// </remarks>
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
    /// JavaScript 字符串取值 “non-blocking”；属于 RenderBlockingStatusType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/resource-timing/#dom-renderblockingstatustype-non-blocking">Resource Timing: 3.3.1 RenderBlockingStatusType enum</see>
    /// </remarks>
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
    /// JavaScript 字符串取值 “mouse”；属于 HandwritingInputType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/handwriting-recognition/#dom-handwritinginputtype-mouse">Handwriting Recognition API: 3 Feature Query</see>
    /// </remarks>
    [Description("@#mouse")]
    Mouse = 0,

    /// <summary>
    /// JavaScript 字符串取值 “stylus”；属于 HandwritingInputType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/handwriting-recognition/#dom-handwritinginputtype-stylus">Handwriting Recognition API: 3 Feature Query</see>
    /// </remarks>
    [Description("@#stylus")]
    Stylus = 1,

    /// <summary>
    /// JavaScript 字符串取值 “touch”；属于 HandwritingInputType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/handwriting-recognition/#dom-handwritinginputtype-touch">Handwriting Recognition API: 3 Feature Query</see>
    /// </remarks>
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
    /// JavaScript 字符串取值 “per-character”；属于 HandwritingRecognitionType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/handwriting-recognition/#dom-handwritingrecognitiontype-per-character">Handwriting Recognition API: 3 Feature Query</see>
    /// </remarks>
    [Description("@#per-character")]
    PerCharacter = 1
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
    /// JavaScript 字符串取值 “direct”；属于 TouchType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/touch-events/#dom-touchtype-direct">Touch Events - Level 2: 3 Touch Interface</see>
    /// </remarks>
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
    /// <summary>
    /// JavaScript 字符串取值 “”；属于 GamepadHand 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/gamepad/extensions.html">Gamepad Extensions: GamepadHand.</see>
    /// </remarks>
[Description("@#")]
    Empty = 0,

    /// <summary>
    /// JavaScript 字符串取值 “left”；属于 GamepadHand 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/gamepad/extensions.html#dom-gamepadhand-left">Gamepad Extensions: 3 GamepadHand Enum</see>
    /// </remarks>
    [Description("@#left")]
    Left = 1,

    /// <summary>
    /// JavaScript 字符串取值 “right”；属于 GamepadHand 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/gamepad/extensions.html#dom-gamepadhand-right">Gamepad Extensions: 3 GamepadHand Enum</see>
    /// </remarks>
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
    /// JavaScript 字符串取值 “never”；属于 CursorCaptureConstraint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-screen-share/#idl-def-CursorCaptureConstraint.never">Screen Capture: 5.4.16 CursorCaptureConstraint</see>
    /// </remarks>
    [Description("@#never")]
    Never = 0,

    /// <summary>
    /// JavaScript 字符串取值 “always”；属于 CursorCaptureConstraint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-screen-share/#idl-def-CursorCaptureConstraint.always">Screen Capture: 5.4.16 CursorCaptureConstraint</see>
    /// </remarks>
    [Description("@#always")]
    Always = 1,

    /// <summary>
    /// JavaScript 字符串取值 “motion”；属于 CursorCaptureConstraint 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-screen-share/#idl-def-CursorCaptureConstraint.motion">Screen Capture: 5.4.16 CursorCaptureConstraint</see>
    /// </remarks>
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
    /// JavaScript 字符串取值 “constant”；属于 VideoEncoderBitrateMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videoencoderbitratemode-constant">WebCodecs: 7.14 VideoEncoderBitrateMode</see>
    /// </remarks>
    [Description("@#constant")]
    Constant = 0,

    /// <summary>
    /// JavaScript 字符串取值 “variable”；属于 VideoEncoderBitrateMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videoencoderbitratemode-variable">WebCodecs: 7.14 VideoEncoderBitrateMode</see>
    /// </remarks>
    [Description("@#variable")]
    Variable = 1,

    /// <summary>
    /// JavaScript 字符串取值 “quantizer”；属于 VideoEncoderBitrateMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-videoencoderbitratemode-quantizer">WebCodecs: 7.14 VideoEncoderBitrateMode</see>
    /// </remarks>
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
    /// JavaScript 字符串取值 “aac”；属于 AacBitstreamFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/aac_codec_registration.html#dom-aacbitstreamformat-aac">AAC WebCodecs Registration: 5.2 AacBitstreamFormat</see>
    /// </remarks>
    [Description("@#aac")]
    Aac = 0,

    /// <summary>
    /// JavaScript 字符串取值 “adts”；属于 AacBitstreamFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/aac_codec_registration.html#dom-aacbitstreamformat-adts">AAC WebCodecs Registration: 5.2 AacBitstreamFormat</see>
    /// </remarks>
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
    /// JavaScript 字符串取值 “opus”；属于 OpusBitstreamFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/opus_codec_registration.html#dom-opusbitstreamformat-opus">Opus WebCodecs Registration: 5.2 OpusBitstreamFormat</see>
    /// </remarks>
    [Description("@#opus")]
    Opus = 0,

    /// <summary>
    /// JavaScript 字符串取值 “ogg”；属于 OpusBitstreamFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/opus_codec_registration.html#dom-opusbitstreamformat-ogg">Opus WebCodecs Registration: 5.2 OpusBitstreamFormat</see>
    /// </remarks>
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
    /// JavaScript 字符串取值 “annexb”；属于 AvcBitstreamFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/avc_codec_registration.html#dom-avcbitstreamformat-annexb">AVC (H.264) WebCodecs Registration: 5.2 AvcBitstreamFormat</see>
    /// </remarks>
    [Description("@#annexb")]
    Annexb = 0,

    /// <summary>
    /// JavaScript 字符串取值 “avc”；属于 AvcBitstreamFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/avc_codec_registration.html#dom-avcbitstreamformat-avc">AVC (H.264) WebCodecs Registration: 5.2 AvcBitstreamFormat</see>
    /// </remarks>
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
    /// JavaScript 字符串取值 “annexb”；属于 HevcBitstreamFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/hevc_codec_registration.html#dom-hevcbitstreamformat-annexb">HEVC (H.265) WebCodecs Registration: 5.2 HevcBitstreamFormat</see>
    /// </remarks>
    [Description("@#annexb")]
    Annexb = 0,

    /// <summary>
    /// JavaScript 字符串取值 “hevc”；属于 HevcBitstreamFormat 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/hevc_codec_registration.html#dom-hevcbitstreamformat-hevc">HEVC (H.265) WebCodecs Registration: 5.2 HevcBitstreamFormat</see>
    /// </remarks>
    [Description("@#hevc")]
    Hevc = 1
}

/// <summary>
/// Each NavigateEvent has a focus reset behavior, a NavigationFocusReset-or-null, initially null.
/// </summary>
/// <remarks>
/// <see href="https://html.spec.whatwg.org/multipage/nav-history-apis.html#navigationfocusreset">HTML Standard: 7.2.6.10.1 The NavigateEvent interface</see>
/// </remarks>
[Description("@#NavigationFocusReset")]
[ECMAScript]
[String]
public enum NavigationFocusReset
{
    /// <summary>
    /// If it was left as null, then we treat that as &quot;after-transition&quot;, and continue onward.
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/nav-history-apis.html#dom-navigationfocusreset-after-transition">HTML Standard: 7.2.6.10.1 The NavigateEvent interface</see>
    /// </remarks>
    [Description("@#after-transition")]
    AfterTransition = 0,

    /// <summary>
    /// By default, using this method will cause focus to reset when any handlers&apos; returned promises settle. Focus will be reset to the first element with the autofocus attribute set, or the body element if the attribute isn&apos;t present. The focusReset option can be set to &quot;manual&quot; to avoid this behavior.
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/nav-history-apis.html#dom-navigationfocusreset-manual">HTML Standard: 7.2.6.10.1 The NavigateEvent interface</see>
    /// </remarks>
    [Description("@#manual")]
    Manual = 1
}

/// <summary>
/// Each NavigateEvent has a scroll behavior, a NavigationScrollBehavior-or-null, initially null.
/// </summary>
/// <remarks>
/// <see href="https://html.spec.whatwg.org/multipage/nav-history-apis.html#navigationscrollbehavior">HTML Standard: 7.2.6.10.1 The NavigateEvent interface</see>
/// </remarks>
[Description("@#NavigationScrollBehavior")]
[ECMAScript]
[String]
public enum NavigationScrollBehavior
{
    /// <summary>
    /// If called more than once, or called after automatic post-transition scroll processing has happened due to the scroll option being left as &quot;after-transition&quot;, or called before the navigation has committed, this method will throw an &quot;InvalidStateError&quot; DOMException.
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/nav-history-apis.html#dom-navigationscrollbehavior-after-transition">HTML Standard: 7.2.6.10.1 The NavigateEvent interface</see>
    /// </remarks>
    [Description("@#after-transition")]
    AfterTransition = 0,

    /// <summary>
    /// By default, using this method will delay the browser&apos;s scroll restoration logic for &quot;traverse&quot; or &quot;reload&quot; navigations, or its scroll-reset/scroll-to-a-fragment logic for &quot;push&quot; or &quot;replace&quot; navigations, until any handlers&apos; returned promises settle. The scroll option can be set to &quot;manual&quot; to turn off any browser-driven scroll behavior entirely for this navigation, or scroll() can be called before the promise settles to trigger this behavior early.
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/nav-history-apis.html#dom-navigationscrollbehavior-manual">HTML Standard: 7.2.6.10.1 The NavigateEvent interface</see>
    /// </remarks>
    [Description("@#manual")]
    Manual = 1
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
    /// JavaScript 字符串取值 “p256dh”；属于 PushEncryptionKeyName 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/push-api/#dom-pushencryptionkeyname-p256dh">Push API: 8.1 PushEncryptionKeyName enumeration</see>
    /// </remarks>
    [Description("@#p256dh")]
    P256dh = 0,

    /// <summary>
    /// JavaScript 字符串取值 “auth”；属于 PushEncryptionKeyName 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/push-api/#dom-pushencryptionkeyname-auth">Push API: 8.1 PushEncryptionKeyName enumeration</see>
    /// </remarks>
    [Description("@#auth")]
    Auth = 1
}

/// <summary>
/// Fire an event named error using the RTCErrorEvent interface with its errorDetail attribute set to either &quot;dtls-failure&quot; or &quot;fingerprint-failure&quot;, as appropriate, and other fields set as described under the RTCErrorDetailType enum description, at transport.
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcerrordetailtype">WebRTC: Real-Time Communication in Browsers: 11.2 RTCErrorDetailType Enum</see>
/// </remarks>
[Description("@#RTCErrorDetailType")]
[ECMAScript]
[String]
public enum RTCErrorDetailType
{
    /// <summary>
    /// For each channel in errorList, fire an event named error using the RTCErrorEvent interface with the errorDetail attribute set to &quot;data-channel-failure&quot; at channel.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcerrordetailtype-data-channel-failure">WebRTC: Real-Time Communication in Browsers: 11.2 RTCErrorDetailType Enum</see>
    /// </remarks>
    [Description("@#data-channel-failure")]
    DataChannelFailure = 0,

    /// <summary>
    /// Fire an event named error using the RTCErrorEvent interface with its errorDetail attribute set to either &quot;dtls-failure&quot; or &quot;fingerprint-failure&quot;, as appropriate, and other fields set as described under the RTCErrorDetailType enum description, at transport.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcerrordetailtype-dtls-failure">WebRTC: Real-Time Communication in Browsers: 11.2 RTCErrorDetailType Enum</see>
    /// </remarks>
    [Description("@#dtls-failure")]
    DtlsFailure = 1,

    /// <summary>
    /// Fire an event named error using the RTCErrorEvent interface with its errorDetail attribute set to either &quot;dtls-failure&quot; or &quot;fingerprint-failure&quot;, as appropriate, and other fields set as described under the RTCErrorDetailType enum description, at transport.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcerrordetailtype-fingerprint-failure">WebRTC: Real-Time Communication in Browsers: 11.2 RTCErrorDetailType Enum</see>
    /// </remarks>
    [Description("@#fingerprint-failure")]
    FingerprintFailure = 2,

    /// <summary>
    /// If the underlying data transport | transport was closed with an error, fire an event named error using the RTCErrorEvent interface with its errorDetail attribute set to &quot;sctp-failure&quot; at channel.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcerrordetailtype-sctp-failure">WebRTC: Real-Time Communication in Browsers: 11.2 RTCErrorDetailType Enum</see>
    /// </remarks>
    [Description("@#sctp-failure")]
    SctpFailure = 3,

    /// <summary>
    /// If the process to apply description failed to parse the SDP as defined in !RFC9429 Section 5.8. including session-level parsing (5.8.1.) and media-level parsing (5.8.2.), but excluding semantics verification (5.8.3.), then reject p with an RTCError (with errorDetail set to &quot;sdp-syntax-error&quot; and the sdpLineNumber attribute set to the line number in the SDP where the syntax error was detected) and abort these steps.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcerrordetailtype-sdp-syntax-error">WebRTC: Real-Time Communication in Browsers: 11.2 RTCErrorDetailType Enum</see>
    /// </remarks>
    [Description("@#sdp-syntax-error")]
    SdpSyntaxError = 4,

    /// <summary>
    /// If an error occurred due to hardware resources not being available, reject p with a newly created RTCError whose errorDetail is set to &quot;hardware-encoder-not-available&quot; and abort these steps.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcerrordetailtype-hardware-encoder-not-available">WebRTC: Real-Time Communication in Browsers: 11.2 RTCErrorDetailType Enum</see>
    /// </remarks>
    [Description("@#hardware-encoder-not-available")]
    HardwareEncoderNotAvailable = 5,

    /// <summary>
    /// If an error occurred due to a hardware encoder not supporting parameters, reject p with a newly created RTCError whose errorDetail is set to &quot;hardware-encoder-error&quot; and abort these steps.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcerrordetailtype-hardware-encoder-error">WebRTC: Real-Time Communication in Browsers: 11.2 RTCErrorDetailType Enum</see>
    /// </remarks>
    [Description("@#hardware-encoder-error")]
    HardwareEncoderError = 6
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
/// If options is provided, the ImageBitmap object&apos;s bitmap data is modified according to options. For example, if the premultiplyAlpha option is set to &quot;premultiply&quot;, the bitmap data&apos;s non-alpha color components are premultiplied by the alpha component.
/// </summary>
/// <remarks>
/// <see href="https://html.spec.whatwg.org/multipage/imagebitmap-and-animations.html#premultiplyalpha">HTML Standard: 8.11.2 The ImageBitmap interface</see>
/// </remarks>
[Description("@#PremultiplyAlpha")]
[ECMAScript]
[String]
public enum PremultiplyAlpha
{
    /// <summary>
    /// If val is &quot;none&quot;, the output that is not premultiplied by alpha must be left untouched and that is premultiplied by alpha must have its color components divided by alpha.
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/imagebitmap-and-animations.html#dom-premultiplyalpha-none">HTML Standard: 8.11.2 The ImageBitmap interface</see>
    /// </remarks>
    [Description("@#none")]
    None = 0,

    /// <summary>
    /// If options is provided, the ImageBitmap object&apos;s bitmap data is modified according to options. For example, if the premultiplyAlpha option is set to &quot;premultiply&quot;, the bitmap data&apos;s non-alpha color components are premultiplied by the alpha component.
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/imagebitmap-and-animations.html#dom-premultiplyalpha-premultiply">HTML Standard: 8.11.2 The ImageBitmap interface</see>
    /// </remarks>
    [Description("@#premultiply")]
    Premultiply = 1,

    /// <summary>
    /// If val is &quot;default&quot;, the alpha premultiplication behavior is implementation-specific, and should be chosen according to implementation deems optimal for drawing images onto the canvas.
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/imagebitmap-and-animations.html#dom-premultiplyalpha-default">HTML Standard: 8.11.2 The ImageBitmap interface</see>
    /// </remarks>
    [Description("@#default")]
    Default = 2
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
    /// JavaScript 字符串取值 “pq”；属于 TransferFunction 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/media-capabilities/#dom-transferfunction-pq">Media Capabilities: 2.1.7 TransferFunction</see>
    /// </remarks>
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
    /// JavaScript 字符串取值 “openid4vci-v1”；属于 DigitalCredentialIssuanceProtocol 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c-fedid.github.io/digital-credentials/#dom-digitalcredentialissuanceprotocol-openid4vci-v1">Digital Credentials: 5 Protocols</see>
    /// </remarks>
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
    /// JavaScript 字符串取值 “openid4vp-v1-unsigned”；属于 DigitalCredentialPresentationProtocol 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c-fedid.github.io/digital-credentials/#dom-digitalcredentialpresentationprotocol-openid4vp-v1-unsigned">Digital Credentials: 5 Protocols</see>
    /// </remarks>
    [Description("@#openid4vp-v1-unsigned")]
    Openid4vpV1Unsigned = 0,

    /// <summary>
    /// JavaScript 字符串取值 “openid4vp-v1-signed”；属于 DigitalCredentialPresentationProtocol 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c-fedid.github.io/digital-credentials/#dom-digitalcredentialpresentationprotocol-openid4vp-v1-signed">Digital Credentials: 5 Protocols</see>
    /// </remarks>
    [Description("@#openid4vp-v1-signed")]
    Openid4vpV1Signed = 1,

    /// <summary>
    /// JavaScript 字符串取值 “openid4vp-v1-multisigned”；属于 DigitalCredentialPresentationProtocol 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c-fedid.github.io/digital-credentials/#dom-digitalcredentialpresentationprotocol-openid4vp-v1-multisigned">Digital Credentials: 5 Protocols</see>
    /// </remarks>
    [Description("@#openid4vp-v1-multisigned")]
    Openid4vpV1Multisigned = 2,

    /// <summary>
    /// JavaScript 字符串取值 “org-iso-mdoc”；属于 DigitalCredentialPresentationProtocol 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c-fedid.github.io/digital-credentials/#dom-digitalcredentialpresentationprotocol-org-iso-mdoc">Digital Credentials: 5 Protocols</see>
    /// </remarks>
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
    /// JavaScript 字符串取值 “constant”；属于 BitrateMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-record/#dom-bitratemode-constant">MediaStream Recording: 2.6.1 Values</see>
    /// </remarks>
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
    /// JavaScript 字符串取值 “unavailable”；属于 Availability 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#dom-availability-unavailable">Writing Assistance APIs: 5.1 Common APIs</see>
    /// </remarks>
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
    /// JavaScript 字符串取值 “available”；属于 Availability 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#dom-availability-available">Writing Assistance APIs: 5.1 Common APIs</see>
    /// </remarks>
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
    /// JavaScript 字符串取值 “all”；属于 EchoCancellationModeEnum 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-main/#dom-echocancellationmodeenum-all">Media Capture and Streams: 4.3.8 Constrainable Properties</see>
    /// </remarks>
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
/// Let direction be an RTCRtpTransceiverDirection value representing the direction from the media description .
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcrtptransceiverdirection">WebRTC: Real-Time Communication in Browsers: 5.1 RTCPeerConnection Interface Extensions</see>
/// </remarks>
[Description("@#RTCRtpTransceiverDirection")]
[ECMAScript]
[String]
public enum RTCRtpTransceiverDirection
{
    /// <summary>
    /// If direction is &quot;sendrecv&quot; or &quot;recvonly&quot;, set transceiver.Receptive to true, otherwise set it to false.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcrtptransceiverdirection-sendrecv">WebRTC: Real-Time Communication in Browsers: 5.1 RTCPeerConnection Interface Extensions</see>
    /// </remarks>
    [Description("@#sendrecv")]
    Sendrecv = 0,

    /// <summary>
    /// If the direction is &quot;sendonly&quot; or &quot;inactive&quot;, the receiver is not prepared to receive anything, and the list will be empty.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcrtptransceiverdirection-sendonly">WebRTC: Real-Time Communication in Browsers: 5.1 RTCPeerConnection Interface Extensions</see>
    /// </remarks>
    [Description("@#sendonly")]
    Sendonly = 1,

    /// <summary>
    /// If direction is &quot;sendrecv&quot; or &quot;recvonly&quot;, set transceiver.Receptive to true, otherwise set it to false.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcrtptransceiverdirection-recvonly">WebRTC: Real-Time Communication in Browsers: 5.1 RTCPeerConnection Interface Extensions</see>
    /// </remarks>
    [Description("@#recvonly")]
    Recvonly = 2,

    /// <summary>
    /// If the direction is &quot;sendonly&quot; or &quot;inactive&quot;, the receiver is not prepared to receive anything, and the list will be empty.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcrtptransceiverdirection-inactive">WebRTC: Real-Time Communication in Browsers: 5.1 RTCPeerConnection Interface Extensions</see>
    /// </remarks>
    [Description("@#inactive")]
    Inactive = 3,

    /// <summary>
    /// For each non-stopped &quot;sendrecv&quot; transceiver of RTCRtpTransceiver/transceiver kind kind, set transceiver.Direction to &quot;sendonly&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcrtptransceiverdirection-stopped">WebRTC: Real-Time Communication in Browsers: 5.1 RTCPeerConnection Interface Extensions</see>
    /// </remarks>
    [Description("@#stopped")]
    Stopped = 4
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
    /// JavaScript 字符串取值 “license-renewal”；属于 MediaKeyMessageType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/encrypted-media/#dom-mediakeymessagetype-license-renewal">Encrypted Media Extensions: 6.4 MediaKeyMessageEvent</see>
    /// </remarks>
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
/// Let newState be the value of deriving a new state value as described by the RTCPeerConnectionState enum.
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcpeerconnectionstate">WebRTC: Real-Time Communication in Browsers: 4.3.3 RTCPeerConnectionState Enum</see>
/// </remarks>
[Description("@#RTCPeerConnectionState")]
[ECMAScript]
[String]
public enum RTCPeerConnectionState
{
    /// <summary>
    /// Set connection.ConnectionState to &quot;closed&quot;. This does not fire any event.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcpeerconnectionstate-closed">WebRTC: Real-Time Communication in Browsers: 4.3.3 RTCPeerConnectionState Enum</see>
    /// </remarks>
    [Description("@#closed")]
    Closed = 0,

    /// <summary>
    /// JavaScript 字符串取值 “failed”；属于 RTCPeerConnectionState 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcpeerconnectionstate-failed">WebRTC: Real-Time Communication in Browsers: 4.3.3 RTCPeerConnectionState Enum</see>
    /// </remarks>
    [Description("@#failed")]
    Failed = 1,

    /// <summary>
    /// JavaScript 字符串取值 “disconnected”；属于 RTCPeerConnectionState 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcpeerconnectionstate-disconnected">WebRTC: Real-Time Communication in Browsers: 4.3.3 RTCPeerConnectionState Enum</see>
    /// </remarks>
    [Description("@#disconnected")]
    Disconnected = 2,

    /// <summary>
    /// In the &quot;connecting&quot; state, one or more RTCIceTransports are in the &quot;new&quot; or &quot;checking&quot; state, or one or more RTCDtlsTransports are in the &quot;new&quot; or &quot;connecting&quot; state.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcpeerconnectionstate-new">WebRTC: Real-Time Communication in Browsers: 4.3.3 RTCPeerConnectionState Enum</see>
    /// </remarks>
    [Description("@#new")]
    New = 3,

    /// <summary>
    /// In the &quot;connecting&quot; state, one or more RTCIceTransports are in the &quot;new&quot; or &quot;checking&quot; state, or one or more RTCDtlsTransports are in the &quot;new&quot; or &quot;connecting&quot; state.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcpeerconnectionstate-connecting">WebRTC: Real-Time Communication in Browsers: 4.3.3 RTCPeerConnectionState Enum</see>
    /// </remarks>
    [Description("@#connecting")]
    Connecting = 4,

    /// <summary>
    /// If connection&apos;s RTCPeerConnectionState is not &quot;connected&quot; return false.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcpeerconnectionstate-connected">WebRTC: Real-Time Communication in Browsers: 4.3.3 RTCPeerConnectionState Enum</see>
    /// </remarks>
    [Description("@#connected")]
    Connected = 5
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
    /// JavaScript 字符串取值 “low”；属于 PerformanceTimingConfidenceValue 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/navigation-timing/#dom-performancetimingconfidencevalue-low">Navigation Timing Level 2: 3.3.3 The PerformanceTimingConfidenceValue enum</see>
    /// </remarks>
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
    /// JavaScript 字符串取值 “relu”；属于 MLRecurrentNetworkActivation 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mlrecurrentnetworkactivation-relu">Web Neural Network API: 8.9.25 gru</see>
    /// </remarks>
    [Description("@#relu")]
    Relu = 0,

    /// <summary>
    /// JavaScript 字符串取值 “sigmoid”；属于 MLRecurrentNetworkActivation 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mlrecurrentnetworkactivation-sigmoid">Web Neural Network API: 8.9.25 gru</see>
    /// </remarks>
    [Description("@#sigmoid")]
    Sigmoid = 1,

    /// <summary>
    /// JavaScript 字符串取值 “tanh”；属于 MLRecurrentNetworkActivation 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mlrecurrentnetworkactivation-tanh">Web Neural Network API: 8.9.25 gru</see>
    /// </remarks>
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
    /// JavaScript 字符串取值 “key”；属于 EncodedAudioChunkType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-encodedaudiochunktype-key">WebCodecs: 8.1 EncodedAudioChunk Interface</see>
    /// </remarks>
    [Description("@#key")]
    Key = 0,

    /// <summary>
    /// JavaScript 字符串取值 “delta”；属于 EncodedAudioChunkType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-encodedaudiochunktype-delta">WebCodecs: 8.1 EncodedAudioChunk Interface</see>
    /// </remarks>
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
    /// JavaScript 字符串取值 “key”；属于 EncodedVideoChunkType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-encodedvideochunktype-key">WebCodecs: 8.2 EncodedVideoChunk Interface</see>
    /// </remarks>
    [Description("@#key")]
    Key = 0,

    /// <summary>
    /// JavaScript 字符串取值 “delta”；属于 EncodedVideoChunkType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-encodedvideochunktype-delta">WebCodecs: 8.2 EncodedVideoChunk Interface</see>
    /// </remarks>
    [Description("@#delta")]
    Delta = 1
}

/// <summary>
/// Set connection.IceConnectionState to the value of deriving a new state value as described by the RTCIceConnectionState enum.
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webrtc-pc/#dom-rtciceconnectionstate">WebRTC: Real-Time Communication in Browsers: 4.3.4 RTCIceConnectionState Enum</see>
/// </remarks>
[Description("@#RTCIceConnectionState")]
[ECMAScript]
[String]
public enum RTCIceConnectionState
{
    /// <summary>
    /// Set connection.IceConnectionState to &quot;closed&quot;. This does not fire any event.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtciceconnectionstate-closed">WebRTC: Real-Time Communication in Browsers: 4.3.4 RTCIceConnectionState Enum</see>
    /// </remarks>
    [Description("@#closed")]
    Closed = 0,

    /// <summary>
    /// Performing an ICE restart is recommended when iceConnectionState transitions to &quot;failed&quot;. An application may additionally choose to listen for the iceConnectionState transition to &quot;disconnected&quot; and then use other sources of information (such as using getStats to measure if the number of bytes sent or received over the next couple of seconds increases) to determine whether an ICE restart is advisable.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtciceconnectionstate-failed">WebRTC: Real-Time Communication in Browsers: 4.3.4 RTCIceConnectionState Enum</see>
    /// </remarks>
    [Description("@#failed")]
    Failed = 1,

    /// <summary>
    /// Performing an ICE restart is recommended when iceConnectionState transitions to &quot;failed&quot;. An application may additionally choose to listen for the iceConnectionState transition to &quot;disconnected&quot; and then use other sources of information (such as using getStats to measure if the number of bytes sent or received over the next couple of seconds increases) to determine whether an ICE restart is advisable.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtciceconnectionstate-disconnected">WebRTC: Real-Time Communication in Browsers: 4.3.4 RTCIceConnectionState Enum</see>
    /// </remarks>
    [Description("@#disconnected")]
    Disconnected = 2,

    /// <summary>
    /// JavaScript 字符串取值 “new”；属于 RTCIceConnectionState 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtciceconnectionstate-new">WebRTC: Real-Time Communication in Browsers: 4.3.4 RTCIceConnectionState Enum</see>
    /// </remarks>
    [Description("@#new")]
    New = 3,

    /// <summary>
    /// JavaScript 字符串取值 “checking”；属于 RTCIceConnectionState 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtciceconnectionstate-checking">WebRTC: Real-Time Communication in Browsers: 4.3.4 RTCIceConnectionState Enum</see>
    /// </remarks>
    [Description("@#checking")]
    Checking = 4,

    /// <summary>
    /// JavaScript 字符串取值 “completed”；属于 RTCIceConnectionState 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtciceconnectionstate-completed">WebRTC: Real-Time Communication in Browsers: 4.3.4 RTCIceConnectionState Enum</see>
    /// </remarks>
    [Description("@#completed")]
    Completed = 5,

    /// <summary>
    /// JavaScript 字符串取值 “connected”；属于 RTCIceConnectionState 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtciceconnectionstate-connected">WebRTC: Real-Time Communication in Browsers: 4.3.4 RTCIceConnectionState Enum</see>
    /// </remarks>
    [Description("@#connected")]
    Connected = 6
}

/// <summary>
/// Set connection.IceGatheringState to the value of deriving a new state value as described by the RTCIceGatheringState enum.
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicegatheringstate">WebRTC: Real-Time Communication in Browsers: 4.3.2 RTCIceGatheringState Enum</see>
/// </remarks>
[Description("@#RTCIceGatheringState")]
[ECMAScript]
[String]
public enum RTCIceGatheringState
{
    /// <summary>
    /// JavaScript 字符串取值 “new”；属于 RTCIceGatheringState 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicegatheringstate-new">WebRTC: Real-Time Communication in Browsers: 4.3.2 RTCIceGatheringState Enum</see>
    /// </remarks>
    [Description("@#new")]
    New = 0,

    /// <summary>
    /// JavaScript 字符串取值 “gathering”；属于 RTCIceGatheringState 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicegatheringstate-gathering">WebRTC: Real-Time Communication in Browsers: 4.3.2 RTCIceGatheringState Enum</see>
    /// </remarks>
    [Description("@#gathering")]
    Gathering = 1,

    /// <summary>
    /// All RTCIceTransports have finished gathering candidates, and the RTCPeerConnection&apos;s RTCIceGatheringState has transitioned to &quot;complete&quot;. This is indicated by the candidate member of the event being set to null. This only exists for backwards compatibility, and this event does not need to be signaled to the remote peer. It&apos;s equivalent to an icegatheringstatechange event with the &quot;complete&quot; state.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicegatheringstate-complete">WebRTC: Real-Time Communication in Browsers: 4.3.2 RTCIceGatheringState Enum</see>
    /// </remarks>
    [Description("@#complete")]
    Complete = 2
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
    /// JavaScript 字符串取值 “floor”；属于 MLRoundingType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mlroundingtype-floor">Web Neural Network API: 8.9.37 Pooling operations</see>
    /// </remarks>
    [Description("@#floor")]
    Floor = 0,

    /// <summary>
    /// JavaScript 字符串取值 “ceil”；属于 MLRoundingType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/webnn/#dom-mlroundingtype-ceil">Web Neural Network API: 8.9.37 Pooling operations</see>
    /// </remarks>
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
    /// JavaScript 字符串取值 “voip”；属于 OpusApplication 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/opus_codec_registration.html#dom-opusapplication-voip">Opus WebCodecs Registration: 5.4 OpusApplication</see>
    /// </remarks>
    [Description("@#voip")]
    Voip = 0,

    /// <summary>
    /// JavaScript 字符串取值 “audio”；属于 OpusApplication 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/opus_codec_registration.html#dom-opusapplication-audio">Opus WebCodecs Registration: 5.4 OpusApplication</see>
    /// </remarks>
    [Description("@#audio")]
    Audio = 1,

    /// <summary>
    /// JavaScript 字符串取值 “lowdelay”；属于 OpusApplication 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/opus_codec_registration.html#dom-opusapplication-lowdelay">Opus WebCodecs Registration: 5.4 OpusApplication</see>
    /// </remarks>
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
    /// JavaScript 字符串取值 “auto”；属于 OpusSignal 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/opus_codec_registration.html#dom-opussignal-auto">Opus WebCodecs Registration: 5.3 OpusSignal</see>
    /// </remarks>
    [Description("@#auto")]
    Auto = 0,

    /// <summary>
    /// JavaScript 字符串取值 “music”；属于 OpusSignal 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/opus_codec_registration.html#dom-opussignal-music">Opus WebCodecs Registration: 5.3 OpusSignal</see>
    /// </remarks>
    [Description("@#music")]
    Music = 1,

    /// <summary>
    /// JavaScript 字符串取值 “voice”；属于 OpusSignal 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/opus_codec_registration.html#dom-opussignal-voice">Opus WebCodecs Registration: 5.3 OpusSignal</see>
    /// </remarks>
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
/// The CanvasColorType enumeration is used to specify the color type of the canvas&apos;s backing store.
/// </summary>
/// <remarks>
/// <see href="https://html.spec.whatwg.org/multipage/canvas.html#canvascolortype">HTML Standard: 4.12.5.1 The 2D rendering context</see>
/// </remarks>
[Description("@#CanvasColorType")]
[ECMAScript]
[String]
public enum CanvasColorType
{
    /// <summary>
    /// The &quot;unorm8&quot; value indicates that the type for all color components is 8-bit unsigned normalized.
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-canvascolortype-unorm8">HTML Standard: 4.12.5.1 The 2D rendering context</see>
    /// </remarks>
    [Description("@#unorm8")]
    Unorm8 = 0,

    /// <summary>
    /// The &quot;float16&quot; value indicates that the type for all color components is 16-bit floating point.
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-canvascolortype-float16">HTML Standard: 4.12.5.1 The 2D rendering context</see>
    /// </remarks>
    [Description("@#float16")]
    Float16 = 1
}

/// <summary>
/// The CanvasFillRule enumeration is used to select the fill rule algorithm by which to determine if a point is inside or outside a path.
/// </summary>
/// <remarks>
/// <see href="https://html.spec.whatwg.org/multipage/canvas.html#canvasfillrule">HTML Standard: 4.12.5.1 The 2D rendering context</see>
/// </remarks>
[Description("@#CanvasFillRule")]
[ECMAScript]
[String]
public enum CanvasFillRule
{
    /// <summary>
    /// The &quot;nonzero&quot; value indicates the nonzero winding rule, wherein
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-fillrule-nonzero">HTML Standard: 4.12.5.1 The 2D rendering context</see>
    /// </remarks>
    [Description("@#nonzero")]
    Nonzero = 0,

    /// <summary>
    /// The &quot;evenodd&quot; value indicates the even-odd rule, wherein
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-fillrule-evenodd">HTML Standard: 4.12.5.1 The 2D rendering context</see>
    /// </remarks>
    [Description("@#evenodd")]
    Evenodd = 1
}

/// <summary>
/// The ImageDataPixelFormat enumeration is used to specify type of the data attribute of an ImageData and the arrangement and numerical representation of the color components for each pixel.
/// </summary>
/// <remarks>
/// <see href="https://html.spec.whatwg.org/multipage/imagebitmap-and-animations.html#imagedatapixelformat">HTML Standard: 8.11.1 The ImageData interface</see>
/// </remarks>
[Description("@#ImageDataPixelFormat")]
[ECMAScript]
[String]
public enum ImageDataPixelFormat
{
    /// <summary>
    /// Let bytesPerPixel be 4 if settings[&quot;pixelFormat&quot;] is &quot;rgba-unorm8&quot;; otherwise 8.
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/imagebitmap-and-animations.html#dom-imagedatapixelformat-rgba-unorm8">HTML Standard: 8.11.1 The ImageData interface</see>
    /// </remarks>
    [Description("@#rgba-unorm8")]
    RgbaUnorm8 = 0,

    /// <summary>
    /// If settings[&quot;pixelFormat&quot;] is &quot;rgba-float16&quot; and source is not a Float16Array, then throw an &quot;InvalidStateError&quot; DOMException.
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/imagebitmap-and-animations.html#dom-imagedatapixelformat-rgba-float16">HTML Standard: 8.11.1 The ImageData interface</see>
    /// </remarks>
    [Description("@#rgba-float16")]
    RgbaFloat16 = 1
}

/// <summary>
/// The ImageSmoothingQuality enumeration is used to express a preference for the interpolation quality to use when smoothing images.
/// </summary>
/// <remarks>
/// <see href="https://html.spec.whatwg.org/multipage/canvas.html#imagesmoothingquality">HTML Standard: 4.12.5.1 The 2D rendering context</see>
/// </remarks>
[Description("@#ImageSmoothingQuality")]
[ECMAScript]
[String]
public enum ImageSmoothingQuality
{
    /// <summary>
    /// The &quot;low&quot; value indicates a preference for a low level of image interpolation quality. Low-quality image interpolation may be more computationally efficient than higher settings.
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-imagesmoothingquality-low">HTML Standard: 4.12.5.1 The 2D rendering context</see>
    /// </remarks>
    [Description("@#low")]
    Low = 0,

    /// <summary>
    /// The &quot;medium&quot; value indicates a preference for a medium level of image interpolation quality.
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-imagesmoothingquality-medium">HTML Standard: 4.12.5.1 The 2D rendering context</see>
    /// </remarks>
    [Description("@#medium")]
    Medium = 1,

    /// <summary>
    /// JavaScript 字符串取值 “high”；属于 ImageSmoothingQuality 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-context-2d-imagesmoothingquality-high">HTML Standard: 4.12.5.1 The 2D rendering context</see>
    /// </remarks>
    [Description("@#high")]
    High = 2
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
    /// JavaScript 字符串取值 “any”；属于 OrientationLockType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/screen-orientation/#dom-orientationlocktype-any">Screen Orientation: 6 OrientationLockType enum</see>
    /// </remarks>
    [Description("@#any")]
    Any = 0,

    /// <summary>
    /// JavaScript 字符串取值 “natural”；属于 OrientationLockType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/screen-orientation/#dom-orientationlocktype-natural">Screen Orientation: 6 OrientationLockType enum</see>
    /// </remarks>
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
    /// JavaScript 字符串取值 “portrait-primary”；属于 OrientationLockType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/screen-orientation/#dom-orientationlocktype-portrait-primary">Screen Orientation: 6 OrientationLockType enum</see>
    /// </remarks>
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
    /// JavaScript 字符串取值 “portrait-secondary”；属于 OrientationType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/screen-orientation/#dom-orientationtype-portrait-secondary">Screen Orientation: 7 OrientationType enum</see>
    /// </remarks>
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
    /// JavaScript 字符串取值 “landscape-secondary”；属于 OrientationType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/screen-orientation/#dom-orientationtype-landscape-secondary">Screen Orientation: 7 OrientationType enum</see>
    /// </remarks>
    [Description("@#landscape-secondary")]
    LandscapeSecondary = 3
}

/// <summary>
/// The PredefinedColorSpace enumeration is used to specify the color space of the canvas&apos;s backing store.
/// </summary>
/// <remarks>
/// <see href="https://html.spec.whatwg.org/multipage/canvas.html#predefinedcolorspace">HTML Standard: 4.12.5.4 Color spaces and color space conversion</see>
/// </remarks>
[Description("@#PredefinedColorSpace")]
[ECMAScript]
[String]
public enum PredefinedColorSpace
{
    /// <summary>
    /// The &quot;srgb&quot; value indicates the &apos;srgb&apos; color space.
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-predefinedcolorspace-srgb">HTML Standard: 4.12.5.4 Color spaces and color space conversion</see>
    /// </remarks>
    [Description("@#srgb")]
    Srgb = 0,

    /// <summary>
    /// The &quot;srgb-linear&quot; value indicates the &apos;srgb-linear&apos; color space.
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-predefinedcolorspace-srgb-linear">HTML Standard: 4.12.5.4 Color spaces and color space conversion</see>
    /// </remarks>
    [Description("@#srgb-linear")]
    SrgbLinear = 1,

    /// <summary>
    /// The &quot;display-p3&quot; value indicates the &apos;display-p3&apos; color space.
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-predefinedcolorspace-display-p3">HTML Standard: 4.12.5.4 Color spaces and color space conversion</see>
    /// </remarks>
    [Description("@#display-p3")]
    DisplayP3 = 2,

    /// <summary>
    /// The &quot;display-p3-linear&quot; value indicates the &apos;display-p3-linear&apos; color space.
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/canvas.html#dom-predefinedcolorspace-display-p3-linear">HTML Standard: 4.12.5.4 Color spaces and color space conversion</see>
    /// </remarks>
    [Description("@#display-p3-linear")]
    DisplayP3Linear = 3
}

/// <summary>
/// The RTCIceTransportState of an RTCIceTransport may change because a candidate pair with a usable connection was found and selected or it may change without the selected candidate pair changing. The selected pair and RTCIceTransportState are related and are handled in the same task.
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicetransportstate">WebRTC: Real-Time Communication in Browsers: 5.6.4 RTCIceTransportState Enum</see>
/// </remarks>
[Description("@#RTCIceTransportState")]
[ECMAScript]
[String]
public enum RTCIceTransportState
{
    /// <summary>
    /// Set the IceTransportState slot of each of connection&apos;s RTCIceTransports to &quot;closed&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicetransportstate-closed">WebRTC: Real-Time Communication in Browsers: 5.6.4 RTCIceTransportState Enum</see>
    /// </remarks>
    [Description("@#closed")]
    Closed = 0,

    /// <summary>
    /// The &quot;failed&quot; and &quot;completed&quot; states require an indication that there are no additional remote candidates. This can be indicated by calling addIceCandidate with a candidate value whose candidate property is set to an empty string or by canTrickleIceCandidates being set to false.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicetransportstate-failed">WebRTC: Real-Time Communication in Browsers: 5.6.4 RTCIceTransportState Enum</see>
    /// </remarks>
    [Description("@#failed")]
    Failed = 1,

    /// <summary>
    /// An ICE restart causes candidate gathering and connectivity checks to begin anew, causing a transition to &quot;connected&quot; if begun in the &quot;completed&quot; state. If begun in the transient &quot;disconnected&quot; state, it causes a transition to &quot;checking&quot;, effectively forgetting that connectivity was previously lost.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicetransportstate-disconnected">WebRTC: Real-Time Communication in Browsers: 5.6.4 RTCIceTransportState Enum</see>
    /// </remarks>
    [Description("@#disconnected")]
    Disconnected = 2,

    /// <summary>
    /// In the &quot;connecting&quot; state, one or more RTCIceTransports are in the &quot;new&quot; or &quot;checking&quot; state, or one or more RTCDtlsTransports are in the &quot;new&quot; or &quot;connecting&quot; state.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicetransportstate-new">WebRTC: Real-Time Communication in Browsers: 5.6.4 RTCIceTransportState Enum</see>
    /// </remarks>
    [Description("@#new")]
    New = 3,

    /// <summary>
    /// In the &quot;connecting&quot; state, one or more RTCIceTransports are in the &quot;new&quot; or &quot;checking&quot; state, or one or more RTCDtlsTransports are in the &quot;new&quot; or &quot;connecting&quot; state.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicetransportstate-checking">WebRTC: Real-Time Communication in Browsers: 5.6.4 RTCIceTransportState Enum</see>
    /// </remarks>
    [Description("@#checking")]
    Checking = 4,

    /// <summary>
    /// The most common transitions for a successful call will be new -&gt; checking -&gt; connected -&gt; completed, but under specific circumstances (only the last checked candidate succeeds, and gathering and the no-more candidates indication both occur prior to success), the state can transition directly from &quot;checking&quot; to &quot;completed&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicetransportstate-completed">WebRTC: Real-Time Communication in Browsers: 5.6.4 RTCIceTransportState Enum</see>
    /// </remarks>
    [Description("@#completed")]
    Completed = 5,

    /// <summary>
    /// The most common transitions for a successful call will be new -&gt; checking -&gt; connected -&gt; completed, but under specific circumstances (only the last checked candidate succeeds, and gathering and the no-more candidates indication both occur prior to success), the state can transition directly from &quot;checking&quot; to &quot;completed&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicetransportstate-connected">WebRTC: Real-Time Communication in Browsers: 5.6.4 RTCIceTransportState Enum</see>
    /// </remarks>
    [Description("@#connected")]
    Connected = 6
}

/// <summary>
/// The RTCSdpType enum describes the type of an RTCSessionDescriptionInit, RTCLocalSessionDescriptionInit, or RTCSessionDescription instance.
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcsdptype">WebRTC: Real-Time Communication in Browsers: 4.6.1 RTCSdpType</see>
/// </remarks>
[Description("@#RTCSdpType")]
[ECMAScript]
[String]
public enum RTCSdpType
{
    /// <summary>
    /// If remote is true and description is of type &quot;offer&quot;, then if any addTrack() methods on connection succeeded during the process to apply description, abort these steps and start the process over as if they had succeeded prior, to include the extra transceiver(s) in the process.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcsdptype-offer">WebRTC: Real-Time Communication in Browsers: 4.6.1 RTCSdpType</see>
    /// </remarks>
    [Description("@#offer")]
    Offer = 0,

    /// <summary>
    /// If description.type is &quot;rollback&quot; and connection.SignalingState is either &quot;stable&quot;, &quot;have-local-pranswer&quot;, or &quot;have-remote-pranswer&quot;, then reject p with a newly exception/created InvalidStateError and abort these steps.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcsdptype-pranswer">WebRTC: Real-Time Communication in Browsers: 4.6.1 RTCSdpType</see>
    /// </remarks>
    [Description("@#pranswer")]
    Pranswer = 1,

    /// <summary>
    /// The RTCAnswerOptions dictionary describe options specific to session description of type &quot;answer&quot; (none in this version of the specification).
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcsdptype-answer">WebRTC: Real-Time Communication in Browsers: 4.6.1 RTCSdpType</see>
    /// </remarks>
    [Description("@#answer")]
    Answer = 2,

    /// <summary>
    /// If description.type is &quot;rollback&quot; and connection.SignalingState is either &quot;stable&quot;, &quot;have-local-pranswer&quot;, or &quot;have-remote-pranswer&quot;, then reject p with a newly exception/created InvalidStateError and abort these steps.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcsdptype-rollback">WebRTC: Real-Time Communication in Browsers: 4.6.1 RTCSdpType</see>
    /// </remarks>
    [Description("@#rollback")]
    Rollback = 3
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
/// The ResizeQuality enumeration is used to express a preference for the interpolation quality to use when scaling images.
/// </summary>
/// <remarks>
/// <see href="https://html.spec.whatwg.org/multipage/imagebitmap-and-animations.html#resizequality">HTML Standard: 8.11.2 The ImageBitmap interface</see>
/// </remarks>
[Description("@#ResizeQuality")]
[ECMAScript]
[String]
public enum ResizeQuality
{
    /// <summary>
    /// The &quot;pixelated&quot; value indicates a preference for scaling the image to preserve the pixelation of the original as much as possible, with minor smoothing as necessary to avoid distorting the image when the target size is not a clean multiple of the original.
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/imagebitmap-and-animations.html#dom-resizequality-pixelated">HTML Standard: 8.11.2 The ImageBitmap interface</see>
    /// </remarks>
    [Description("@#pixelated")]
    Pixelated = 0,

    /// <summary>
    /// The &quot;low&quot; value indicates a preference for a low level of image interpolation quality. Low-quality image interpolation may be more computationally efficient than higher settings.
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/imagebitmap-and-animations.html#dom-resizequality-low">HTML Standard: 8.11.2 The ImageBitmap interface</see>
    /// </remarks>
    [Description("@#low")]
    Low = 1,

    /// <summary>
    /// The &quot;medium&quot; value indicates a preference for a medium level of image interpolation quality.
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/imagebitmap-and-animations.html#dom-resizequality-medium">HTML Standard: 8.11.2 The ImageBitmap interface</see>
    /// </remarks>
    [Description("@#medium")]
    Medium = 2,

    /// <summary>
    /// JavaScript 字符串取值 “high”；属于 ResizeQuality 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/imagebitmap-and-animations.html#dom-resizequality-high">HTML Standard: 8.11.2 The ImageBitmap interface</see>
    /// </remarks>
    [Description("@#high")]
    High = 3
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
    /// JavaScript 字符串取值 “quality”；属于 LatencyMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webcodecs/#dom-latencymode-quality">WebCodecs: 7.11 Latency Mode</see>
    /// </remarks>
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
    /// <summary>
    /// JavaScript 字符串取值 “”；属于 GamepadMappingType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/gamepad/">Gamepad: GamepadMappingType.</see>
    /// </remarks>
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
    /// JavaScript 字符串取值 “codec”；属于 RTCStatsType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-stats/#dom-rtcstatstype-codec">Identifiers for WebRTC&apos;s Statistics API: 7.1 RTCStatsType enum</see>
    /// </remarks>
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
    /// JavaScript 字符串取值 “media-playout”；属于 RTCStatsType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-stats/#dom-rtcstatstype-media-playout">Identifiers for WebRTC&apos;s Statistics API: 7.1 RTCStatsType enum</see>
    /// </remarks>
    [Description("@#media-playout")]
    MediaPlayout = 6,

    /// <summary>
    /// JavaScript 字符串取值 “peer-connection”；属于 RTCStatsType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-stats/#dom-rtcstatstype-peer-connection">Identifiers for WebRTC&apos;s Statistics API: 7.1 RTCStatsType enum</see>
    /// </remarks>
    [Description("@#peer-connection")]
    PeerConnection = 7,

    /// <summary>
    /// JavaScript 字符串取值 “data-channel”；属于 RTCStatsType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-stats/#dom-rtcstatstype-data-channel">Identifiers for WebRTC&apos;s Statistics API: 7.1 RTCStatsType enum</see>
    /// </remarks>
    [Description("@#data-channel")]
    DataChannel = 8,

    /// <summary>
    /// JavaScript 字符串取值 “transport”；属于 RTCStatsType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-stats/#dom-rtcstatstype-transport">Identifiers for WebRTC&apos;s Statistics API: 7.1 RTCStatsType enum</see>
    /// </remarks>
    [Description("@#transport")]
    Transport = 9,

    /// <summary>
    /// JavaScript 字符串取值 “candidate-pair”；属于 RTCStatsType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-stats/#dom-rtcstatstype-candidate-pair">Identifiers for WebRTC&apos;s Statistics API: 7.1 RTCStatsType enum</see>
    /// </remarks>
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
    /// JavaScript 字符串取值 “certificate”；属于 RTCStatsType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-stats/#dom-rtcstatstype-certificate">Identifiers for WebRTC&apos;s Statistics API: 7.1 RTCStatsType enum</see>
    /// </remarks>
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
    /// JavaScript 字符串取值 “spelling”；属于 CorrectionType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/proofreader-api/#dom-correctiontype-spelling">Proofreader API: 3 The proofreader API</see>
    /// </remarks>
    [Description("@#spelling")]
    Spelling = 0,

    /// <summary>
    /// JavaScript 字符串取值 “punctuation”；属于 CorrectionType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/proofreader-api/#dom-correctiontype-punctuation">Proofreader API: 3 The proofreader API</see>
    /// </remarks>
    [Description("@#punctuation")]
    Punctuation = 1,

    /// <summary>
    /// JavaScript 字符串取值 “capitalization”；属于 CorrectionType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/proofreader-api/#dom-correctiontype-capitalization">Proofreader API: 3 The proofreader API</see>
    /// </remarks>
    [Description("@#capitalization")]
    Capitalization = 2,

    /// <summary>
    /// JavaScript 字符串取值 “grammar”；属于 CorrectionType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webmachinelearning.github.io/proofreader-api/#dom-correctiontype-grammar">Proofreader API: 3 The proofreader API</see>
    /// </remarks>
    [Description("@#grammar")]
    Grammar = 3
}

/// <summary>
/// The sequence of supported face detection modes. Each string MUST be one of the members of HumanFaceDetectionModeEnum. See humanFaceDetectionMode for additional details.
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/mediacapture-extensions/#dom-humanfacedetectionmodeenum">Media Capture and Streams Extensions: 17.7 HumanFaceDetectionModeEnum</see>
/// </remarks>
[Description("@#HumanFaceDetectionModeEnum")]
[ECMAScript]
[String]
public enum HumanFaceDetectionModeEnum
{
    /// <summary>
    /// JavaScript 字符串取值 “none”；属于 HumanFaceDetectionModeEnum 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-extensions/#dom-humanfacedetectionmodeenum-none">Media Capture and Streams Extensions: HumanFaceDetectionModeEnum Enumeration Description</see>
    /// </remarks>
    [Description("@#none")]
    None = 0,

    /// <summary>
    /// With this setting, the source sets a superset of the metadata compared to the &quot;bounding-box&quot; setting. The source sets the same metadata and additionally metadata related to human face landmarks (all other SegmentTypes except &quot;human-face&quot;) including center point information in the member centerPoint of each Segment related to a detected landmark. As an input, this is interpreted as a command to enable the setting of human face and face landmark detection and to set bounding box related information to face segment metadata and to set the center point information of each detected face landmark.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-extensions/#dom-humanfacedetectionmodeenum-bounding-box">Media Capture and Streams Extensions: HumanFaceDetectionModeEnum Enumeration Description</see>
    /// </remarks>
    [Description("@#bounding-box")]
    BoundingBox = 1,

    /// <summary>
    /// JavaScript 字符串取值 “bounding-box-with-landmark-center-point”；属于 HumanFaceDetectionModeEnum 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-extensions/#dom-humanfacedetectionmodeenum-bounding-box-with-landmark-center-point">Media Capture and Streams Extensions: HumanFaceDetectionModeEnum Enumeration Description</see>
    /// </remarks>
    [Description("@#bounding-box-with-landmark-center-point")]
    BoundingBoxWithLandmarkCenterPoint = 2
}

/// <summary>
/// The type argument has to exactly match these values; we do not perform an ASCII case-insensitive match. This is different from how type content attribute values are treated, and how DOMTokenList&apos;s supports() method works, but it aligns with the WorkerType enumeration used in the Worker() constructor.
/// </summary>
/// <remarks>
/// <see href="https://html.spec.whatwg.org/multipage/workers.html#workertype">HTML Standard: 10.2.6.3 Dedicated workers and the Worker interface</see>
/// </remarks>
[Description("@#WorkerType")]
[ECMAScript]
[String]
public enum WorkerType
{
    /// <summary>
    /// JavaScript 字符串取值 “classic”；属于 WorkerType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/">HTML Standard: WorkerType.classic</see>
    /// </remarks>
[Description("@#classic")]
    Classic = 0,

    /// <summary>
    /// JavaScript 字符串取值 “module”；属于 WorkerType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://html.spec.whatwg.org/multipage/">HTML Standard: WorkerType.module</see>
    /// </remarks>
[Description("@#module")]
    Module = 1
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
    /// JavaScript 字符串取值 “monitor”；属于 DisplayCaptureSurfaceType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-screen-share/#idl-def-DisplayCaptureSurfaceType.monitor">Screen Capture: 5.4.15 DisplayCaptureSurfaceType</see>
    /// </remarks>
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
/// This MediaStreamTrack source does not set metadata in VideoFrameMetadata of VideoFrames related to human faces or human face landmarks, that is, to any Segment which has the type set to any of the alternatives listed in enumeration SegmentType. As an input, this is interpreted as a command to turn off the setting of human face and landmark detection.
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/mediacapture-extensions/#dom-segmenttype">Media Capture and Streams Extensions: 17.2 Segment</see>
/// </remarks>
[Description("@#SegmentType")]
[ECMAScript]
[String]
public enum SegmentType
{
    /// <summary>
    /// JavaScript 字符串取值 “human-face”；属于 SegmentType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-extensions/#dom-segmenttype-human-face">Media Capture and Streams Extensions: Dictionary Segment Members</see>
    /// </remarks>
    [Description("@#human-face")]
    HumanFace = 0,

    /// <summary>
    /// JavaScript 字符串取值 “left-eye”；属于 SegmentType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-extensions/#dom-segmenttype-left-eye">Media Capture and Streams Extensions: Dictionary Segment Members</see>
    /// </remarks>
    [Description("@#left-eye")]
    LeftEye = 1,

    /// <summary>
    /// JavaScript 字符串取值 “right-eye”；属于 SegmentType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-extensions/#dom-segmenttype-right-eye">Media Capture and Streams Extensions: Dictionary Segment Members</see>
    /// </remarks>
    [Description("@#right-eye")]
    RightEye = 2,

    /// <summary>
    /// JavaScript 字符串取值 “eye”；属于 SegmentType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-extensions/#dom-segmenttype-eye">Media Capture and Streams Extensions: Dictionary Segment Members</see>
    /// </remarks>
    [Description("@#eye")]
    Eye = 3,

    /// <summary>
    /// JavaScript 字符串取值 “mouth”；属于 SegmentType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-extensions/#dom-segmenttype-mouth">Media Capture and Streams Extensions: Dictionary Segment Members</see>
    /// </remarks>
    [Description("@#mouth")]
    Mouth = 4
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
    /// JavaScript 字符串取值 “shippingAddress”；属于 PaymentDelegation 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/web-based-payment-handler/#dom-paymentdelegation-shippingaddress">Web-based Payment Handler API: 4.3 PaymentDelegation enum</see>
    /// </remarks>
    [Description("@#shippingAddress")]
    ShippingAddress = 0,

    /// <summary>
    /// JavaScript 字符串取值 “payerName”；属于 PaymentDelegation 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/web-based-payment-handler/#dom-paymentdelegation-payername">Web-based Payment Handler API: 4.3 PaymentDelegation enum</see>
    /// </remarks>
    [Description("@#payerName")]
    PayerName = 1,

    /// <summary>
    /// JavaScript 字符串取值 “payerPhone”；属于 PaymentDelegation 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/web-based-payment-handler/#dom-paymentdelegation-payerphone">Web-based Payment Handler API: 4.3 PaymentDelegation enum</see>
    /// </remarks>
    [Description("@#payerPhone")]
    PayerPhone = 2,

    /// <summary>
    /// JavaScript 字符串取值 “payerEmail”；属于 PaymentDelegation 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/web-based-payment-handler/#dom-paymentdelegation-payeremail">Web-based Payment Handler API: 4.3 PaymentDelegation enum</see>
    /// </remarks>
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
    /// JavaScript 字符串取值 “preferred”；属于 AudioSelectionPreferenceEnum 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-screen-share/#idl-def-AudioSelectionPreferenceEnum.preferred">Screen Capture: 5.4.9 AudioSelectionPreferenceEnum</see>
    /// </remarks>
    [Description("@#preferred")]
    Preferred = 0
}

/// <summary>
/// bundlePolicy of type RTCBundlePolicy, defaulting to &quot;balanced&quot;.
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcbundlepolicy">WebRTC: Real-Time Communication in Browsers: 4.2.4 RTCBundlePolicy Enum</see>
/// </remarks>
[Description("@#RTCBundlePolicy")]
[ECMAScript]
[String]
public enum RTCBundlePolicy
{
    /// <summary>
    /// bundlePolicy of type RTCBundlePolicy, defaulting to &quot;balanced&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcbundlepolicy-balanced">WebRTC: Real-Time Communication in Browsers: 4.2.4 RTCBundlePolicy Enum</see>
    /// </remarks>
    [Description("@#balanced")]
    Balanced = 0,

    /// <summary>
    /// JavaScript 字符串取值 “max-compat”；属于 RTCBundlePolicy 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcbundlepolicy-max-compat">WebRTC: Real-Time Communication in Browsers: 4.2.4 RTCBundlePolicy Enum</see>
    /// </remarks>
    [Description("@#max-compat")]
    MaxCompat = 1,

    /// <summary>
    /// JavaScript 字符串取值 “max-bundle”；属于 RTCBundlePolicy 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcbundlepolicy-max-bundle">WebRTC: Real-Time Communication in Browsers: 4.2.4 RTCBundlePolicy Enum</see>
    /// </remarks>
    [Description("@#max-bundle")]
    MaxBundle = 2
}

/// <summary>
/// component of type RTCIceComponent, readonly, nullable
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicecomponent">WebRTC: Real-Time Communication in Browsers: 5.6.6 RTCIceComponent Enum</see>
/// </remarks>
[Description("@#RTCIceComponent")]
[ECMAScript]
[String]
public enum RTCIceComponent
{
    /// <summary>
    /// The assigned network component of the candidate (&quot;rtp&quot; or &quot;rtcp&quot;). This corresponds to the component-id field in candidate-attribute , decoded to the string representation as defined in RTCIceComponent.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicecomponent-rtp">WebRTC: Real-Time Communication in Browsers: 5.6.6 RTCIceComponent Enum</see>
    /// </remarks>
    [Description("@#rtp")]
    Rtp = 0,

    /// <summary>
    /// The assigned network component of the candidate (&quot;rtp&quot; or &quot;rtcp&quot;). This corresponds to the component-id field in candidate-attribute , decoded to the string representation as defined in RTCIceComponent.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicecomponent-rtcp">WebRTC: Real-Time Communication in Browsers: 5.6.6 RTCIceComponent Enum</see>
    /// </remarks>
    [Description("@#rtcp")]
    Rtcp = 1
}

/// <summary>
/// defaultSemantics of type GetUserMediaSemantics, readonly
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/mediacapture-extensions/#dom-getusermediasemantics">Media Capture and Streams Extensions: 4.5 GetUserMediaSemantics enum</see>
/// </remarks>
[Description("@#GetUserMediaSemantics")]
[ECMAScript]
[String]
public enum GetUserMediaSemantics
{
    /// <summary>
    /// JavaScript 字符串取值 “browser-chooses”；属于 GetUserMediaSemantics 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-extensions/#dom-getusermediasemantics-browser-chooses">Media Capture and Streams Extensions: 4.5 GetUserMediaSemantics enum</see>
    /// </remarks>
    [Description("@#browser-chooses")]
    BrowserChooses = 0,

    /// <summary>
    /// JavaScript 字符串取值 “user-chooses”；属于 GetUserMediaSemantics 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-extensions/#dom-getusermediasemantics-user-chooses">Media Capture and Streams Extensions: 4.5 GetUserMediaSemantics enum</see>
    /// </remarks>
    [Description("@#user-chooses")]
    UserChooses = 1
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
    /// JavaScript 字符串取值 “balanced”；属于 RTCDegradationPreference 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mst-content-hint/#dom-rtcdegradationpreference-balanced">MediaStreamTrack Content Hints: 4.2 Degradation preference when encoding</see>
    /// </remarks>
    [Description("@#balanced")]
    Balanced = 2,

    /// <summary>
    /// JavaScript 字符串取值 “maintain-framerate-and-resolution”；属于 RTCDegradationPreference 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mst-content-hint/#dom-rtcdegradationpreference-maintain-framerate-and-resolution">MediaStreamTrack Content Hints: 4.2 Degradation preference when encoding</see>
    /// </remarks>
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
/// gatheringState of type RTCIceGathererState, readonly
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicegathererstate">WebRTC: Real-Time Communication in Browsers: 5.6.3 RTCIceGathererState Enum</see>
/// </remarks>
[Description("@#RTCIceGathererState")]
[ECMAScript]
[String]
public enum RTCIceGathererState
{
    /// <summary>
    /// JavaScript 字符串取值 “new”；属于 RTCIceGathererState 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicegathererstate-new">WebRTC: Real-Time Communication in Browsers: 5.6.3 RTCIceGathererState Enum</see>
    /// </remarks>
    [Description("@#new")]
    New = 0,

    /// <summary>
    /// Set transport.IceGathererState to gathering.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicegathererstate-gathering">WebRTC: Real-Time Communication in Browsers: 5.6.3 RTCIceGathererState Enum</see>
    /// </remarks>
    [Description("@#gathering")]
    Gathering = 1,

    /// <summary>
    /// Set transport.IceGathererState to complete.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicegathererstate-complete">WebRTC: Real-Time Communication in Browsers: 5.6.3 RTCIceGathererState Enum</see>
    /// </remarks>
    [Description("@#complete")]
    Complete = 2
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
/// iceTransportPolicy of type RTCIceTransportPolicy, defaulting to &quot;all&quot;.
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicetransportpolicy">WebRTC: Real-Time Communication in Browsers: 4.2.3 RTCIceTransportPolicy Enum</see>
/// </remarks>
[Description("@#RTCIceTransportPolicy")]
[ECMAScript]
[String]
public enum RTCIceTransportPolicy
{
    /// <summary>
    /// If the iceTransportPolicy member of the RTCConfiguration is relay, candidates requiring external resolution, such as mDNS candidates and DNS candidates, MUST be prohibited.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicetransportpolicy-relay">WebRTC: Real-Time Communication in Browsers: 4.2.3 RTCIceTransportPolicy Enum</see>
    /// </remarks>
    [Description("@#relay")]
    Relay = 0,

    /// <summary>
    /// iceTransportPolicy of type RTCIceTransportPolicy, defaulting to &quot;all&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicetransportpolicy-all">WebRTC: Real-Time Communication in Browsers: 4.2.3 RTCIceTransportPolicy Enum</see>
    /// </remarks>
    [Description("@#all")]
    All = 1
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
    /// JavaScript 字符串取值 “include”；属于 MonitorTypeSurfacesEnum 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-screen-share/#idl-def-MonitorTypeSurfacesEnum.include">Screen Capture: 5.4.8 MonitorTypeSurfacesEnum</see>
    /// </remarks>
    [Description("@#include")]
    Include = 0,

    /// <summary>
    /// JavaScript 字符串取值 “exclude”；属于 MonitorTypeSurfacesEnum 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-screen-share/#idl-def-MonitorTypeSurfacesEnum.exclude">Screen Capture: 5.4.8 MonitorTypeSurfacesEnum</see>
    /// </remarks>
    [Description("@#exclude")]
    Exclude = 1
}

/// <summary>
/// protocol of type RTCIceProtocol, readonly, nullable
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webrtc-pc/#dom-rtciceprotocol">WebRTC: Real-Time Communication in Browsers: 4.8.1.2 RTCIceProtocol Enum</see>
/// </remarks>
[Description("@#RTCIceProtocol")]
[ECMAScript]
[String]
public enum RTCIceProtocol
{
    /// <summary>
    /// The protocol of the candidate (&quot;udp&quot;/&quot;tcp&quot;). This corresponds to the transport field in candidate-attribute .
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtciceprotocol-udp">WebRTC: Real-Time Communication in Browsers: 4.8.1.2 RTCIceProtocol Enum</see>
    /// </remarks>
    [Description("@#udp")]
    Udp = 0,

    /// <summary>
    /// The protocol of the candidate (&quot;udp&quot;/&quot;tcp&quot;). This corresponds to the transport field in candidate-attribute .
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtciceprotocol-tcp">WebRTC: Real-Time Communication in Browsers: 4.8.1.2 RTCIceProtocol Enum</see>
    /// </remarks>
    [Description("@#tcp")]
    Tcp = 1
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
    /// JavaScript 字符串取值 “cpu”；属于 RTCQualityLimitationReason 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-stats/#dom-rtcqualitylimitationreason-cpu">Identifiers for WebRTC&apos;s Statistics API: 8.9 RTCQualityLimitationReason enum</see>
    /// </remarks>
    [Description("@#cpu")]
    Cpu = 1,

    /// <summary>
    /// JavaScript 字符串取值 “bandwidth”；属于 RTCQualityLimitationReason 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-stats/#dom-rtcqualitylimitationreason-bandwidth">Identifiers for WebRTC&apos;s Statistics API: 8.9 RTCQualityLimitationReason enum</see>
    /// </remarks>
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
/// readyState of type RTCDataChannelState, readonly
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcdatachannelstate">WebRTC: Real-Time Communication in Browsers: 6.2 RTCDataChannel</see>
/// </remarks>
[Description("@#RTCDataChannelState")]
[ECMAScript]
[String]
public enum RTCDataChannelState
{
    /// <summary>
    /// An RTCDataChannel, created with createDataChannel or dispatched via an RTCDataChannelEvent, MUST initially be in the &quot;connecting&quot; state. When the RTCDataChannel object&apos;s underlying data transport is ready, the user agent MUST announce the RTCDataChannel as open .
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcdatachannelstate-connecting">WebRTC: Real-Time Communication in Browsers: 6.2 RTCDataChannel</see>
    /// </remarks>
    [Description("@#connecting")]
    Connecting = 0,

    /// <summary>
    /// An RTCDataChannel, created with createDataChannel or dispatched via an RTCDataChannelEvent, MUST initially be in the &quot;connecting&quot; state. When the RTCDataChannel object&apos;s underlying data transport is ready, the user agent MUST announce the RTCDataChannel as open .
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcdatachannelstate-open">WebRTC: Real-Time Communication in Browsers: 6.2 RTCDataChannel</see>
    /// </remarks>
    [Description("@#open")]
    Open = 1,

    /// <summary>
    /// If channel.ReadyState is &quot;closing&quot; or &quot;closed&quot;, abort these steps.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcdatachannelstate-closing">WebRTC: Real-Time Communication in Browsers: 6.2 RTCDataChannel</see>
    /// </remarks>
    [Description("@#closing")]
    Closing = 2,

    /// <summary>
    /// Give channel a new ID generated according to RFC8832. If no available ID could be generated, set channel.ReadyState to &quot;closed&quot;, and add channnel to errorList.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcdatachannelstate-closed">WebRTC: Real-Time Communication in Browsers: 6.2 RTCDataChannel</see>
    /// </remarks>
    [Description("@#closed")]
    Closed = 3
}

/// <summary>
/// relayProtocol of type RTCIceServerTransportProtocol, readonly, nullable
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webrtc-pc/#dom-rtciceservertransportprotocol">WebRTC: Real-Time Communication in Browsers: 4.8.1.5 RTCIceServerTransportProtocol Enum</see>
/// </remarks>
[Description("@#RTCIceServerTransportProtocol")]
[ECMAScript]
[String]
public enum RTCIceServerTransportProtocol
{
    /// <summary>
    /// JavaScript 字符串取值 “udp”；属于 RTCIceServerTransportProtocol 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtciceservertransportprotocol-udp">WebRTC: Real-Time Communication in Browsers: 4.8.1.5 RTCIceServerTransportProtocol Enum</see>
    /// </remarks>
    [Description("@#udp")]
    Udp = 0,

    /// <summary>
    /// JavaScript 字符串取值 “tcp”；属于 RTCIceServerTransportProtocol 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtciceservertransportprotocol-tcp">WebRTC: Real-Time Communication in Browsers: 4.8.1.5 RTCIceServerTransportProtocol Enum</see>
    /// </remarks>
    [Description("@#tcp")]
    Tcp = 1,

    /// <summary>
    /// JavaScript 字符串取值 “tls”；属于 RTCIceServerTransportProtocol 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtciceservertransportprotocol-tls">WebRTC: Real-Time Communication in Browsers: 4.8.1.5 RTCIceServerTransportProtocol Enum</see>
    /// </remarks>
    [Description("@#tls")]
    Tls = 2
}

/// <summary>
/// role of type RTCIceRole, readonly
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicerole">WebRTC: Real-Time Communication in Browsers: 5.6.5 RTCIceRole Enum</see>
/// </remarks>
[Description("@#RTCIceRole")]
[ECMAScript]
[String]
public enum RTCIceRole
{
    /// <summary>
    /// If IceRole is not unknown, do not modify IceRole.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicerole-unknown">WebRTC: Real-Time Communication in Browsers: 5.6.5 RTCIceRole Enum</see>
    /// </remarks>
    [Description("@#unknown")]
    Unknown = 0,

    /// <summary>
    /// If description is a local offer, set it to controlling.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicerole-controlling">WebRTC: Real-Time Communication in Browsers: 5.6.5 RTCIceRole Enum</see>
    /// </remarks>
    [Description("@#controlling")]
    Controlling = 1,

    /// <summary>
    /// If description is a remote offer, and does not contain a=ice-lite, set IceRole to controlled.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicerole-controlled">WebRTC: Real-Time Communication in Browsers: 5.6.5 RTCIceRole Enum</see>
    /// </remarks>
    [Description("@#controlled")]
    Controlled = 2
}

/// <summary>
/// rtcpMuxPolicy of type RTCRtcpMuxPolicy, defaulting to &quot;require&quot;.
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcrtcpmuxpolicy">WebRTC: Real-Time Communication in Browsers: 4.2.5 RTCRtcpMuxPolicy Enum</see>
/// </remarks>
[Description("@#RTCRtcpMuxPolicy")]
[ECMAScript]
[String]
public enum RTCRtcpMuxPolicy
{
    /// <summary>
    /// rtcpMuxPolicy of type RTCRtcpMuxPolicy, defaulting to &quot;require&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcrtcpmuxpolicy-require">WebRTC: Real-Time Communication in Browsers: 4.2.5 RTCRtcpMuxPolicy Enum</see>
    /// </remarks>
    [Description("@#require")]
    Require = 0
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
    /// JavaScript 字符串取值 “include”；属于 SelfCapturePreferenceEnum 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-screen-share/#idl-def-SelfCapturePreferenceEnum.include">Screen Capture: 5.4.4 SelfCapturePreferenceEnum</see>
    /// </remarks>
    [Description("@#include")]
    Include = 0,

    /// <summary>
    /// JavaScript 字符串取值 “exclude”；属于 SelfCapturePreferenceEnum 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-screen-share/#idl-def-SelfCapturePreferenceEnum.exclude">Screen Capture: 5.4.4 SelfCapturePreferenceEnum</see>
    /// </remarks>
    [Description("@#exclude")]
    Exclude = 1
}

/// <summary>
/// signalingState of type RTCSignalingState, readonly
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcsignalingstate">WebRTC: Real-Time Communication in Browsers: 4.3.1 RTCSignalingState Enum</see>
/// </remarks>
[Description("@#RTCSignalingState")]
[ECMAScript]
[String]
public enum RTCSignalingState
{
    /// <summary>
    /// JavaScript 字符串取值 “stable”；属于 RTCSignalingState 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcsignalingstate-stable">WebRTC: Real-Time Communication in Browsers: 4.3.1 RTCSignalingState Enum</see>
    /// </remarks>
    [Description("@#stable")]
    Stable = 0,

    /// <summary>
    /// If description is of type &quot;offer&quot;, set connection.PendingLocalDescription to a new RTCSessionDescription object constructed from description, set connection.SignalingState to &quot;have-local-offer&quot;, and release early candidates .
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcsignalingstate-have-local-offer">WebRTC: Real-Time Communication in Browsers: 4.3.1 RTCSignalingState Enum</see>
    /// </remarks>
    [Description("@#have-local-offer")]
    HaveLocalOffer = 1,

    /// <summary>
    /// If description is of type &quot;offer&quot;, set connection.PendingRemoteDescription attribute to a new RTCSessionDescription object constructed from description, and set connection.SignalingState to &quot;have-remote-offer&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcsignalingstate-have-remote-offer">WebRTC: Real-Time Communication in Browsers: 4.3.1 RTCSignalingState Enum</see>
    /// </remarks>
    [Description("@#have-remote-offer")]
    HaveRemoteOffer = 2,

    /// <summary>
    /// If description.type is &quot;rollback&quot; and connection.SignalingState is either &quot;stable&quot;, &quot;have-local-pranswer&quot;, or &quot;have-remote-pranswer&quot;, then reject p with a newly exception/created InvalidStateError and abort these steps.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcsignalingstate-have-local-pranswer">WebRTC: Real-Time Communication in Browsers: 4.3.1 RTCSignalingState Enum</see>
    /// </remarks>
    [Description("@#have-local-pranswer")]
    HaveLocalPranswer = 3,

    /// <summary>
    /// If description.type is &quot;rollback&quot; and connection.SignalingState is either &quot;stable&quot;, &quot;have-local-pranswer&quot;, or &quot;have-remote-pranswer&quot;, then reject p with a newly exception/created InvalidStateError and abort these steps.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcsignalingstate-have-remote-pranswer">WebRTC: Real-Time Communication in Browsers: 4.3.1 RTCSignalingState Enum</see>
    /// </remarks>
    [Description("@#have-remote-pranswer")]
    HaveRemotePranswer = 4,

    /// <summary>
    /// Set connection.SignalingState to &quot;closed&quot;. This does not fire any event.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcsignalingstate-closed">WebRTC: Real-Time Communication in Browsers: 4.3.1 RTCSignalingState Enum</see>
    /// </remarks>
    [Description("@#closed")]
    Closed = 5
}

/// <summary>
/// state of type RTCDtlsTransportState, readonly
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcdtlstransportstate">WebRTC: Real-Time Communication in Browsers: 5.5.1 RTCDtlsTransportState Enum</see>
/// </remarks>
[Description("@#RTCDtlsTransportState")]
[ECMAScript]
[String]
public enum RTCDtlsTransportState
{
    /// <summary>
    /// In the &quot;connecting&quot; state, one or more RTCIceTransports are in the &quot;new&quot; or &quot;checking&quot; state, or one or more RTCDtlsTransports are in the &quot;new&quot; or &quot;connecting&quot; state.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcdtlstransportstate-new">WebRTC: Real-Time Communication in Browsers: 5.5.1 RTCDtlsTransportState Enum</see>
    /// </remarks>
    [Description("@#new")]
    New = 0,

    /// <summary>
    /// In the &quot;connecting&quot; state, one or more RTCIceTransports are in the &quot;new&quot; or &quot;checking&quot; state, or one or more RTCDtlsTransports are in the &quot;new&quot; or &quot;connecting&quot; state.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcdtlstransportstate-connecting">WebRTC: Real-Time Communication in Browsers: 5.5.1 RTCDtlsTransportState Enum</see>
    /// </remarks>
    [Description("@#connecting")]
    Connecting = 1,

    /// <summary>
    /// If newState is connected then let newRemoteCertificates be the certificate chain in use by the remote side, with each certificate encoded in binary Distinguished Encoding Rules (DER) !X690, and set transport.RemoteCertificates to newRemoteCertificates.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcdtlstransportstate-connected">WebRTC: Real-Time Communication in Browsers: 5.5.1 RTCDtlsTransportState Enum</see>
    /// </remarks>
    [Description("@#connected")]
    Connected = 2,

    /// <summary>
    /// Set the DtlsTransportState slot of each of connection&apos;s RTCDtlsTransports to &quot;closed&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcdtlstransportstate-closed">WebRTC: Real-Time Communication in Browsers: 5.5.1 RTCDtlsTransportState Enum</see>
    /// </remarks>
    [Description("@#closed")]
    Closed = 3,

    /// <summary>
    /// If the state of transport is already &quot;failed&quot;, abort these steps.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcdtlstransportstate-failed">WebRTC: Real-Time Communication in Browsers: 5.5.1 RTCDtlsTransportState Enum</see>
    /// </remarks>
    [Description("@#failed")]
    Failed = 4
}

/// <summary>
/// state of type RTCSctpTransportState, readonly
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcsctptransportstate">WebRTC: Real-Time Communication in Browsers: 6.1.2 RTCSctpTransportState Enum</see>
/// </remarks>
[Description("@#RTCSctpTransportState")]
[ECMAScript]
[String]
public enum RTCSctpTransportState
{
    /// <summary>
    /// If description is of type &quot;offer&quot;, and it describes an SCTP association as defined in RFC8841 Section 10.2, and connection.SctpTransport is null, set connection.SctpTransport to the result of create an RTCSctpTransport|creating an RTCSctpTransport with an initial state of &quot;connecting&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#idl-def-RTCSctpTransportState.connecting">WebRTC: Real-Time Communication in Browsers: 6.1.2 RTCSctpTransportState Enum</see>
    /// </remarks>
    [Description("@#connecting")]
    Connecting = 0,

    /// <summary>
    /// If the DataChannelId slot is not null, transport is in the &quot;connected&quot; state and DataChannelId is greater or equal to transport.MaxChannels, exception/throw an OperationError.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#idl-def-RTCSctpTransportState.connected">WebRTC: Real-Time Communication in Browsers: 6.1.2 RTCSctpTransportState Enum</see>
    /// </remarks>
    [Description("@#connected")]
    Connected = 1,

    /// <summary>
    /// If connection.SctpTransport is not null, tear down the underlying SCTP association by sending an SCTP ABORT chunk and set the SctpTransportState to &quot;closed&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#idl-def-RTCSctpTransportState.closed">WebRTC: Real-Time Communication in Browsers: 6.1.2 RTCSctpTransportState Enum</see>
    /// </remarks>
    [Description("@#closed")]
    Closed = 2
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
    /// JavaScript 字符串取值 “frozen”；属于 RTCStatsIceCandidatePairState 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-stats/#dom-rtcstatsicecandidatepairstate-frozen">Identifiers for WebRTC&apos;s Statistics API: 8.19.1 RTCStatsIceCandidatePairState enum</see>
    /// </remarks>
    [Description("@#frozen")]
    Frozen = 0,

    /// <summary>
    /// JavaScript 字符串取值 “waiting”；属于 RTCStatsIceCandidatePairState 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-stats/#dom-rtcstatsicecandidatepairstate-waiting">Identifiers for WebRTC&apos;s Statistics API: 8.19.1 RTCStatsIceCandidatePairState enum</see>
    /// </remarks>
    [Description("@#waiting")]
    Waiting = 1,

    /// <summary>
    /// JavaScript 字符串取值 “in-progress”；属于 RTCStatsIceCandidatePairState 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-stats/#dom-rtcstatsicecandidatepairstate-in-progress">Identifiers for WebRTC&apos;s Statistics API: 8.19.1 RTCStatsIceCandidatePairState enum</see>
    /// </remarks>
    [Description("@#in-progress")]
    InProgress = 2,

    /// <summary>
    /// JavaScript 字符串取值 “failed”；属于 RTCStatsIceCandidatePairState 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-stats/#dom-rtcstatsicecandidatepairstate-failed">Identifiers for WebRTC&apos;s Statistics API: 8.19.1 RTCStatsIceCandidatePairState enum</see>
    /// </remarks>
    [Description("@#failed")]
    Failed = 3,

    /// <summary>
    /// JavaScript 字符串取值 “succeeded”；属于 RTCStatsIceCandidatePairState 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-stats/#dom-rtcstatsicecandidatepairstate-succeeded">Identifiers for WebRTC&apos;s Statistics API: 8.19.1 RTCStatsIceCandidatePairState enum</see>
    /// </remarks>
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
    /// JavaScript 字符串取值 “include”；属于 SurfaceSwitchingPreferenceEnum 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-screen-share/#idl-def-SurfaceSwitchingPreferenceEnum.include">Screen Capture: 5.4.7 SurfaceSwitchingPreferenceEnum</see>
    /// </remarks>
    [Description("@#include")]
    Include = 0,

    /// <summary>
    /// JavaScript 字符串取值 “exclude”；属于 SurfaceSwitchingPreferenceEnum 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-screen-share/#idl-def-SurfaceSwitchingPreferenceEnum.exclude">Screen Capture: 5.4.7 SurfaceSwitchingPreferenceEnum</see>
    /// </remarks>
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
    /// JavaScript 字符串取值 “include”；属于 SystemAudioPreferenceEnum 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-screen-share/#idl-def-SystemAudioPreferenceEnum.include">Screen Capture: 5.4.5 SystemAudioPreferenceEnum</see>
    /// </remarks>
    [Description("@#include")]
    Include = 0,

    /// <summary>
    /// JavaScript 字符串取值 “exclude”；属于 SystemAudioPreferenceEnum 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-screen-share/#idl-def-SystemAudioPreferenceEnum.exclude">Screen Capture: 5.4.5 SystemAudioPreferenceEnum</see>
    /// </remarks>
    [Description("@#exclude")]
    Exclude = 1
}

/// <summary>
/// tcpType of type RTCIceTcpCandidateType, readonly, nullable
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicetcpcandidatetype">WebRTC: Real-Time Communication in Browsers: 4.8.1.3 RTCIceTcpCandidateType Enum</see>
/// </remarks>
[Description("@#RTCIceTcpCandidateType")]
[ECMAScript]
[String]
public enum RTCIceTcpCandidateType
{
    /// <summary>
    /// The user agent will typically only gather active ICE TCP candidates.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicetcpcandidatetype-active">WebRTC: Real-Time Communication in Browsers: 4.8.1.3 RTCIceTcpCandidateType Enum</see>
    /// </remarks>
    [Description("@#active")]
    Active = 0,

    /// <summary>
    /// JavaScript 字符串取值 “passive”；属于 RTCIceTcpCandidateType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicetcpcandidatetype-passive">WebRTC: Real-Time Communication in Browsers: 4.8.1.3 RTCIceTcpCandidateType Enum</see>
    /// </remarks>
    [Description("@#passive")]
    Passive = 1,

    /// <summary>
    /// JavaScript 字符串取值 “so”；属于 RTCIceTcpCandidateType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicetcpcandidatetype-so">WebRTC: Real-Time Communication in Browsers: 4.8.1.3 RTCIceTcpCandidateType Enum</see>
    /// </remarks>
    [Description("@#so")]
    So = 2
}

/// <summary>
/// type of type RTCIceCandidateType, readonly, nullable
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicecandidatetype">WebRTC: Real-Time Communication in Browsers: 4.8.1.4 RTCIceCandidateType Enum</see>
/// </remarks>
[Description("@#RTCIceCandidateType")]
[ECMAScript]
[String]
public enum RTCIceCandidateType
{
    /// <summary>
    /// JavaScript 字符串取值 “host”；属于 RTCIceCandidateType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicecandidatetype-host">WebRTC: Real-Time Communication in Browsers: 4.8.1.4 RTCIceCandidateType Enum</see>
    /// </remarks>
    [Description("@#host")]
    Host = 0,

    /// <summary>
    /// JavaScript 字符串取值 “srflx”；属于 RTCIceCandidateType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicecandidatetype-srflx">WebRTC: Real-Time Communication in Browsers: 4.8.1.4 RTCIceCandidateType Enum</see>
    /// </remarks>
    [Description("@#srflx")]
    Srflx = 1,

    /// <summary>
    /// JavaScript 字符串取值 “prflx”；属于 RTCIceCandidateType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicecandidatetype-prflx">WebRTC: Real-Time Communication in Browsers: 4.8.1.4 RTCIceCandidateType Enum</see>
    /// </remarks>
    [Description("@#prflx")]
    Prflx = 2,

    /// <summary>
    /// JavaScript 字符串取值 “relay”；属于 RTCIceCandidateType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcicecandidatetype-relay">WebRTC: Real-Time Communication in Browsers: 4.8.1.4 RTCIceCandidateType Enum</see>
    /// </remarks>
    [Description("@#relay")]
    Relay = 3
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
    /// JavaScript 字符串取值 “none”；属于 UnderlineStyle 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/edit-context/#dom-underlinestyle-none">EditContext API: 4.2 TextFormatUpdateEvent</see>
    /// </remarks>
    [Description("@#none")]
    None = 0,

    /// <summary>
    /// JavaScript 字符串取值 “solid”；属于 UnderlineStyle 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/edit-context/#dom-underlinestyle-solid">EditContext API: 4.2 TextFormatUpdateEvent</see>
    /// </remarks>
    [Description("@#solid")]
    Solid = 1,

    /// <summary>
    /// JavaScript 字符串取值 “dotted”；属于 UnderlineStyle 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/edit-context/#dom-underlinestyle-dotted">EditContext API: 4.2 TextFormatUpdateEvent</see>
    /// </remarks>
    [Description("@#dotted")]
    Dotted = 2,

    /// <summary>
    /// JavaScript 字符串取值 “dashed”；属于 UnderlineStyle 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/edit-context/#dom-underlinestyle-dashed">EditContext API: 4.2 TextFormatUpdateEvent</see>
    /// </remarks>
    [Description("@#dashed")]
    Dashed = 3,

    /// <summary>
    /// JavaScript 字符串取值 “wavy”；属于 UnderlineStyle 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/edit-context/#dom-underlinestyle-wavy">EditContext API: 4.2 TextFormatUpdateEvent</see>
    /// </remarks>
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
    /// JavaScript 字符串取值 “none”；属于 UnderlineThickness 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/edit-context/#dom-underlinethickness-none">EditContext API: 4.2 TextFormatUpdateEvent</see>
    /// </remarks>
    [Description("@#none")]
    None = 0,

    /// <summary>
    /// JavaScript 字符串取值 “thin”；属于 UnderlineThickness 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/edit-context/#dom-underlinethickness-thin">EditContext API: 4.2 TextFormatUpdateEvent</see>
    /// </remarks>
    [Description("@#thin")]
    Thin = 1,

    /// <summary>
    /// JavaScript 字符串取值 “thick”；属于 UnderlineThickness 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/edit-context/#dom-underlinethickness-thick">EditContext API: 4.2 TextFormatUpdateEvent</see>
    /// </remarks>
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
    /// JavaScript 字符串取值 “system”；属于 WindowAudioPreferenceEnum 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-screen-share/#idl-def-WindowAudioPreferenceEnum.system">Screen Capture: 5.4.6 WindowAudioPreferenceEnum</see>
    /// </remarks>
    [Description("@#system")]
    System = 0,

    /// <summary>
    /// JavaScript 字符串取值 “window”；属于 WindowAudioPreferenceEnum 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-screen-share/#idl-def-WindowAudioPreferenceEnum.window">Screen Capture: 5.4.6 WindowAudioPreferenceEnum</see>
    /// </remarks>
    [Description("@#window")]
    Window = 1,

    /// <summary>
    /// JavaScript 字符串取值 “exclude”；属于 WindowAudioPreferenceEnum 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/mediacapture-screen-share/#idl-def-WindowAudioPreferenceEnum.exclude">Screen Capture: 5.4.6 WindowAudioPreferenceEnum</see>
    /// </remarks>
    [Description("@#exclude")]
    Exclude = 2
}

/// <summary>
/// WebIDL enum AnimationTriggerBehavior。定义于 Web Animations Module Level 2。
/// </summary>
/// <remarks>
/// <see href="https://drafts.csswg.org/web-animations-2/">Web Animations Module Level 2: AnimationTriggerBehavior</see>
/// </remarks>
[Description("@#AnimationTriggerBehavior")]
[ECMAScript]
[String]
public enum AnimationTriggerBehavior
{
    /// <summary>
    /// JavaScript 字符串取值 “once”；属于 AnimationTriggerBehavior 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/web-animations-2/">Web Animations Module Level 2: AnimationTriggerBehavior.once</see>
    /// </remarks>
[Description("@#once")]
    Once = 0,

    /// <summary>
    /// JavaScript 字符串取值 “repeat”；属于 AnimationTriggerBehavior 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/web-animations-2/">Web Animations Module Level 2: AnimationTriggerBehavior.repeat</see>
    /// </remarks>
[Description("@#repeat")]
    Repeat = 1,

    /// <summary>
    /// JavaScript 字符串取值 “alternate”；属于 AnimationTriggerBehavior 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/web-animations-2/">Web Animations Module Level 2: AnimationTriggerBehavior.alternate</see>
    /// </remarks>
[Description("@#alternate")]
    Alternate = 2,

    /// <summary>
    /// JavaScript 字符串取值 “state”；属于 AnimationTriggerBehavior 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/web-animations-2/">Web Animations Module Level 2: AnimationTriggerBehavior.state</see>
    /// </remarks>
[Description("@#state")]
    State = 3
}

/// <summary>
/// WebIDL enum WebGLPowerPreference。定义于 WebGL Specification。
/// </summary>
/// <remarks>
/// <see href="https://registry.khronos.org/webgl/specs/latest/1.0/">WebGL Specification: WebGLPowerPreference</see>
/// </remarks>
[Description("@#WebGLPowerPreference")]
[ECMAScript]
[String]
public enum WebGLPowerPreference
{
    /// <summary>
    /// JavaScript 字符串取值 “default”；属于 WebGLPowerPreference 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://registry.khronos.org/webgl/specs/latest/1.0/">WebGL Specification: WebGLPowerPreference.default</see>
    /// </remarks>
[Description("@#default")]
    Default = 0,

    /// <summary>
    /// JavaScript 字符串取值 “low-power”；属于 WebGLPowerPreference 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://registry.khronos.org/webgl/specs/latest/1.0/">WebGL Specification: WebGLPowerPreference.low-power</see>
    /// </remarks>
[Description("@#low-power")]
    LowPower = 1,

    /// <summary>
    /// JavaScript 字符串取值 “high-performance”；属于 WebGLPowerPreference 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://registry.khronos.org/webgl/specs/latest/1.0/">WebGL Specification: WebGLPowerPreference.high-performance</see>
    /// </remarks>
[Description("@#high-performance")]
    HighPerformance = 2
}