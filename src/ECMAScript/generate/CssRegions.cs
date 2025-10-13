namespace ECMAScript;

/// <summary>
/// Document
/// </summary>
[ECMAScript]
[Description("@#Document")]
public partial class Document : Node
{
    /// <summary>
    /// namedFlows
    /// </summary>
    [Description("@#namedFlows")]
    public extern NamedFlowMap NamedFlows { get; }

    /// <summary>
    /// startViewTransition
    /// </summary>
    /// <param name="callbackOptions">callbackOptions</param>
    [Description("@#startViewTransition")]
    public extern ViewTransition StartViewTransition(Either<ViewTransitionUpdateCallback, StartViewTransitionOptions> callbackOptions);

    /// <summary>
    /// startViewTransition
    /// </summary>
    /// <param name="callbackOptions">callbackOptions</param>
    [Description("@#startViewTransition")]
    public extern ViewTransition StartViewTransition(ViewTransitionUpdateCallback callbackOptions);

    /// <summary>
    /// startViewTransition
    /// </summary>
    /// <param name="callbackOptions">callbackOptions</param>
    [Description("@#startViewTransition")]
    public extern ViewTransition StartViewTransition(StartViewTransitionOptions? callbackOptions = default);

    /// <summary>
    /// elementFromPoint
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    [Description("@#elementFromPoint")]
    public extern Element? ElementFromPoint(double x, double y);

    /// <summary>
    /// elementsFromPoint
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    [Description("@#elementsFromPoint")]
    public extern Element[] ElementsFromPoint(double x, double y);

    /// <summary>
    /// caretPositionFromPoint
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="options">options</param>
    [Description("@#caretPositionFromPoint")]
    public extern CaretPosition? CaretPositionFromPoint(double x, double y, CaretPositionFromPointOptions? options = default);

    /// <summary>
    /// scrollingElement
    /// </summary>
    [Description("@#scrollingElement")]
    public extern Element? ScrollingElement { get; }

    /// <summary>
    /// Constructor 
    /// </summary>
    public extern Document();

    /// <summary>
    /// implementation
    /// </summary>
    [Description("@#implementation")]
    public extern DOMImplementation Implementation { get; }

    /// <summary>
    /// URL
    /// </summary>
    [Description("@#URL")]
    public extern string URL { get; }

    /// <summary>
    /// documentURI
    /// </summary>
    [Description("@#documentURI")]
    public extern string DocumentURI { get; }

    /// <summary>
    /// compatMode
    /// </summary>
    [Description("@#compatMode")]
    public extern string CompatMode { get; }

    /// <summary>
    /// characterSet
    /// </summary>
    [Description("@#characterSet")]
    public extern string CharacterSet { get; }

    /// <summary>
    /// charset
    /// </summary>
    [Description("@#charset")]
    public extern string Charset { get; }

    /// <summary>
    /// inputEncoding
    /// </summary>
    [Description("@#inputEncoding")]
    public extern string InputEncoding { get; }

    /// <summary>
    /// contentType
    /// </summary>
    [Description("@#contentType")]
    public extern string ContentType { get; }

    /// <summary>
    /// doctype
    /// </summary>
    [Description("@#doctype")]
    public extern DocumentType? Doctype { get; }

    /// <summary>
    /// documentElement
    /// </summary>
    [Description("@#documentElement")]
    public extern Element? DocumentElement { get; }

    /// <summary>
    /// getElementsByTagName
    /// </summary>
    /// <param name="qualifiedName">qualifiedName</param>
    [Description("@#getElementsByTagName")]
    public extern HTMLCollection GetElementsByTagName(string qualifiedName);

    /// <summary>
    /// getElementsByTagNameNS
    /// </summary>
    /// <param name="namespace">namespace</param>
    /// <param name="localName">localName</param>
    [Description("@#getElementsByTagNameNS")]
    public extern HTMLCollection GetElementsByTagNameNS(string? @namespace, string localName);

    /// <summary>
    /// getElementsByClassName
    /// </summary>
    /// <param name="classNames">classNames</param>
    [Description("@#getElementsByClassName")]
    public extern HTMLCollection GetElementsByClassName(string classNames);

    /// <summary>
    /// createElement
    /// </summary>
    /// <param name="localName">localName</param>
    /// <param name="options">options</param>
    [Description("@#createElement")]
    public extern Element CreateElement(string localName, Either<string, ElementCreationOptions> options);

    /// <summary>
    /// createElement
    /// </summary>
    /// <param name="localName">localName para</param>
    /// <param name="options">options</param>
    [Description("@#createElement")]
    public extern Element CreateElement(string localName, string options);

    /// <summary>
    /// createElement
    /// </summary>
    /// <param name="localName">localName para</param>
    /// <param name="options">options</param>
    [Description("@#createElement")]
    public extern Element CreateElement(string localName, ElementCreationOptions? options = default);

    /// <summary>
    /// createElementNS
    /// </summary>
    /// <param name="namespace">namespace</param>
    /// <param name="qualifiedName">qualifiedName</param>
    /// <param name="options">options</param>
    [Description("@#createElementNS")]
    public extern Element CreateElementNS(string? @namespace, string qualifiedName, Either<string, ElementCreationOptions> options);

    /// <summary>
    /// createElementNS
    /// </summary>
    /// <param name="namespace">namespace para</param>
    /// <param name="qualifiedName">qualifiedName para</param>
    /// <param name="options">options</param>
    [Description("@#createElementNS")]
    public extern Element CreateElementNS(string? @namespace, string qualifiedName, string options);

    /// <summary>
    /// createElementNS
    /// </summary>
    /// <param name="namespace">namespace para</param>
    /// <param name="qualifiedName">qualifiedName para</param>
    /// <param name="options">options</param>
    [Description("@#createElementNS")]
    public extern Element CreateElementNS(string? @namespace, string qualifiedName, ElementCreationOptions? options = default);

    /// <summary>
    /// createDocumentFragment
    /// </summary>
    [Description("@#createDocumentFragment")]
    public extern DocumentFragment CreateDocumentFragment();

    /// <summary>
    /// createTextNode
    /// </summary>
    /// <param name="data">data</param>
    [Description("@#createTextNode")]
    public extern Text CreateTextNode(string data);

    /// <summary>
    /// createCDATASection
    /// </summary>
    /// <param name="data">data</param>
    [Description("@#createCDATASection")]
    public extern CDATASection CreateCDATASection(string data);

    /// <summary>
    /// createComment
    /// </summary>
    /// <param name="data">data</param>
    [Description("@#createComment")]
    public extern Comment CreateComment(string data);

    /// <summary>
    /// createProcessingInstruction
    /// </summary>
    /// <param name="target">target</param>
    /// <param name="data">data</param>
    [Description("@#createProcessingInstruction")]
    public extern ProcessingInstruction CreateProcessingInstruction(string target, string data);

    /// <summary>
    /// importNode
    /// </summary>
    /// <param name="node">node</param>
    /// <param name="options">options</param>
    [Description("@#importNode")]
    public extern Node ImportNode(Node node, Either<bool, ImportNodeOptions> options);

    /// <summary>
    /// importNode
    /// </summary>
    /// <param name="node">node para</param>
    /// <param name="options">options</param>
    [Description("@#importNode")]
    public extern Node ImportNode(Node node, bool options);

    /// <summary>
    /// importNode
    /// </summary>
    /// <param name="node">node para</param>
    /// <param name="options">options</param>
    [Description("@#importNode")]
    public extern Node ImportNode(Node node, ImportNodeOptions? options = default);

    /// <summary>
    /// adoptNode
    /// </summary>
    /// <param name="node">node</param>
    [Description("@#adoptNode")]
    public extern Node AdoptNode(Node node);

    /// <summary>
    /// createAttribute
    /// </summary>
    /// <param name="localName">localName</param>
    [Description("@#createAttribute")]
    public extern Attr CreateAttribute(string localName);

    /// <summary>
    /// createAttributeNS
    /// </summary>
    /// <param name="namespace">namespace</param>
    /// <param name="qualifiedName">qualifiedName</param>
    [Description("@#createAttributeNS")]
    public extern Attr CreateAttributeNS(string? @namespace, string qualifiedName);

    /// <summary>
    /// createEvent
    /// </summary>
    /// <param name="interface">interface</param>
    [Description("@#createEvent")]
    public extern Event CreateEvent(string @interface);

    /// <summary>
    /// createRange
    /// </summary>
    [Description("@#createRange")]
    public extern Range CreateRange();

    /// <summary>
    /// createNodeIterator
    /// </summary>
    /// <param name="root">root</param>
    /// <param name="whatToShow">whatToShow</param>
    /// <param name="filter">filter</param>
    [Description("@#createNodeIterator")]
    public extern NodeIterator CreateNodeIterator(Node root, uint whatToShow = 0xFFFFFFFF, NodeFilter? filter = null);

    /// <summary>
    /// createTreeWalker
    /// </summary>
    /// <param name="root">root</param>
    /// <param name="whatToShow">whatToShow</param>
    /// <param name="filter">filter</param>
    [Description("@#createTreeWalker")]
    public extern TreeWalker CreateTreeWalker(Node root, uint whatToShow = 0xFFFFFFFF, NodeFilter? filter = null);

    /// <summary>
    /// measureElement
    /// </summary>
    /// <param name="element">element</param>
    [Description("@#measureElement")]
    public extern FontMetrics MeasureElement(Element element);

    /// <summary>
    /// measureText
    /// </summary>
    /// <param name="text">text</param>
    /// <param name="styleMap">styleMap</param>
    [Description("@#measureText")]
    public extern FontMetrics MeasureText(string text, StylePropertyMapReadOnly styleMap);

    /// <summary>
    /// fullscreenEnabled
    /// </summary>
    [Description("@#fullscreenEnabled")]
    public extern bool FullscreenEnabled { get; }

    /// <summary>
    /// fullscreen
    /// </summary>
    [Description("@#fullscreen")]
    public extern bool Fullscreen { get; }

    /// <summary>
    /// exitFullscreen
    /// </summary>
    [Description("@#exitFullscreen")]
    public extern PromiseResult<object> ExitFullscreen();

    /// <summary>
    /// onfullscreenchange
    /// </summary>
    [Description("@#onfullscreenchange")]
    public extern EventHandler Onfullscreenchange { get; set; }

    /// <summary>
    /// onfullscreenerror
    /// </summary>
    [Description("@#onfullscreenerror")]
    public extern EventHandler Onfullscreenerror { get; set; }

    /// <summary>
    /// parseHTMLUnsafe
    /// </summary>
    /// <param name="html">html</param>
    [Description("@#parseHTMLUnsafe")]
    public extern static Document ParseHTMLUnsafe(Either<TrustedHTML, string> html);

    /// <summary>
    /// parseHTMLUnsafe
    /// </summary>
    /// <param name="html">html</param>
    [Description("@#parseHTMLUnsafe")]
    public extern static Document ParseHTMLUnsafe(TrustedHTML html);

    /// <summary>
    /// parseHTMLUnsafe
    /// </summary>
    /// <param name="html">html</param>
    [Description("@#parseHTMLUnsafe")]
    public extern static Document ParseHTMLUnsafe(string html);

    /// <summary>
    /// location
    /// </summary>
    [Description("@#location")]
    public extern Location? Location { get; }

    /// <summary>
    /// domain
    /// </summary>
    [Description("@#domain")]
    public extern string Domain { get; set; }

    /// <summary>
    /// referrer
    /// </summary>
    [Description("@#referrer")]
    public extern string Referrer { get; }

    /// <summary>
    /// cookie
    /// </summary>
    [Description("@#cookie")]
    public extern string Cookie { get; set; }

    /// <summary>
    /// lastModified
    /// </summary>
    [Description("@#lastModified")]
    public extern string LastModified { get; }

    /// <summary>
    /// readyState
    /// </summary>
    [Description("@#readyState")]
    public extern DocumentReadyState ReadyState { get; }

    [Description("@#")]
    public extern object this[string name] { get; }

    /// <summary>
    /// title
    /// </summary>
    [Description("@#title")]
    public extern string Title { get; set; }

    /// <summary>
    /// dir
    /// </summary>
    [Description("@#dir")]
    public extern string Dir { get; set; }

    /// <summary>
    /// body
    /// </summary>
    [Description("@#body")]
    public extern HTMLElement? Body { get; set; }

    /// <summary>
    /// head
    /// </summary>
    [Description("@#head")]
    public extern HTMLHeadElement? Head { get; }

    /// <summary>
    /// images
    /// </summary>
    [Description("@#images")]
    public extern HTMLCollection Images { get; }

    /// <summary>
    /// embeds
    /// </summary>
    [Description("@#embeds")]
    public extern HTMLCollection Embeds { get; }

    /// <summary>
    /// plugins
    /// </summary>
    [Description("@#plugins")]
    public extern HTMLCollection Plugins { get; }

    /// <summary>
    /// links
    /// </summary>
    [Description("@#links")]
    public extern HTMLCollection Links { get; }

    /// <summary>
    /// forms
    /// </summary>
    [Description("@#forms")]
    public extern HTMLCollection Forms { get; }

    /// <summary>
    /// scripts
    /// </summary>
    [Description("@#scripts")]
    public extern HTMLCollection Scripts { get; }

    /// <summary>
    /// getElementsByName
    /// </summary>
    /// <param name="elementName">elementName</param>
    [Description("@#getElementsByName")]
    public extern NodeList GetElementsByName(string elementName);

    /// <summary>
    /// currentScript
    /// </summary>
    [Description("@#currentScript")]
    public extern HTMLOrSVGScriptElement? CurrentScript { get; }

    /// <summary>
    /// open
    /// </summary>
    /// <param name="unused1">unused1</param>
    /// <param name="unused2">unused2</param>
    [Description("@#open")]
    public extern Document Open(string unused1, string unused2);

    /// <summary>
    /// open
    /// </summary>
    /// <param name="url">url</param>
    /// <param name="name">name</param>
    /// <param name="features">features</param>
    [Description("@#open")]
    public extern WindowProxy? Open(string url, string name, string features);

    /// <summary>
    /// close
    /// </summary>
    [Description("@#close")]
    public extern void Close();

    /// <summary>
    /// write
    /// </summary>
    /// <param name="text">text</param>
    [Description("@#write")]
    public extern void Write(Either<TrustedHTML, string> text);

    /// <summary>
    /// write
    /// </summary>
    /// <param name="text">text</param>
    [Description("@#write")]
    public extern void Write(TrustedHTML text);

    /// <summary>
    /// write
    /// </summary>
    /// <param name="text">text</param>
    [Description("@#write")]
    public extern void Write(string text);

    /// <summary>
    /// writeln
    /// </summary>
    /// <param name="text">text</param>
    [Description("@#writeln")]
    public extern void Writeln(Either<TrustedHTML, string> text);

    /// <summary>
    /// writeln
    /// </summary>
    /// <param name="text">text</param>
    [Description("@#writeln")]
    public extern void Writeln(TrustedHTML text);

    /// <summary>
    /// writeln
    /// </summary>
    /// <param name="text">text</param>
    [Description("@#writeln")]
    public extern void Writeln(string text);

    /// <summary>
    /// defaultView
    /// </summary>
    [Description("@#defaultView")]
    public extern WindowProxy? DefaultView { get; }

    /// <summary>
    /// hasFocus
    /// </summary>
    [Description("@#hasFocus")]
    public extern bool HasFocus();

    /// <summary>
    /// designMode
    /// </summary>
    [Description("@#designMode")]
    public extern string DesignMode { get; set; }

    /// <summary>
    /// execCommand
    /// </summary>
    /// <param name="commandId">commandId</param>
    /// <param name="showUi">showUI</param>
    /// <param name="value">value</param>
    [Description("@#execCommand")]
    public extern bool ExecCommand(string commandId, bool showUi = false, string? value = default);

    /// <summary>
    /// queryCommandEnabled
    /// </summary>
    /// <param name="commandId">commandId</param>
    [Description("@#queryCommandEnabled")]
    public extern bool QueryCommandEnabled(string commandId);

    /// <summary>
    /// queryCommandIndeterm
    /// </summary>
    /// <param name="commandId">commandId</param>
    [Description("@#queryCommandIndeterm")]
    public extern bool QueryCommandIndeterm(string commandId);

    /// <summary>
    /// queryCommandState
    /// </summary>
    /// <param name="commandId">commandId</param>
    [Description("@#queryCommandState")]
    public extern bool QueryCommandState(string commandId);

    /// <summary>
    /// queryCommandSupported
    /// </summary>
    /// <param name="commandId">commandId</param>
    [Description("@#queryCommandSupported")]
    public extern bool QueryCommandSupported(string commandId);

    /// <summary>
    /// queryCommandValue
    /// </summary>
    /// <param name="commandId">commandId</param>
    [Description("@#queryCommandValue")]
    public extern string QueryCommandValue(string commandId);

    /// <summary>
    /// hidden
    /// </summary>
    [Description("@#hidden")]
    public extern bool Hidden { get; }

    /// <summary>
    /// visibilityState
    /// </summary>
    [Description("@#visibilityState")]
    public extern DocumentVisibilityState VisibilityState { get; }

    /// <summary>
    /// onreadystatechange
    /// </summary>
    [Description("@#onreadystatechange")]
    public extern EventHandler Onreadystatechange { get; set; }

    /// <summary>
    /// onvisibilitychange
    /// </summary>
    [Description("@#onvisibilitychange")]
    public extern EventHandler Onvisibilitychange { get; set; }

    /// <summary>
    /// fgColor
    /// </summary>
    [Description("@#fgColor")]
    public extern string FgColor { get; set; }

    /// <summary>
    /// linkColor
    /// </summary>
    [Description("@#linkColor")]
    public extern string LinkColor { get; set; }

    /// <summary>
    /// vlinkColor
    /// </summary>
    [Description("@#vlinkColor")]
    public extern string VlinkColor { get; set; }

    /// <summary>
    /// alinkColor
    /// </summary>
    [Description("@#alinkColor")]
    public extern string AlinkColor { get; set; }

    /// <summary>
    /// bgColor
    /// </summary>
    [Description("@#bgColor")]
    public extern string BgColor { get; set; }

    /// <summary>
    /// anchors
    /// </summary>
    [Description("@#anchors")]
    public extern HTMLCollection Anchors { get; }

    /// <summary>
    /// applets
    /// </summary>
    [Description("@#applets")]
    public extern HTMLCollection Applets { get; }

    /// <summary>
    /// clear
    /// </summary>
    [Description("@#clear")]
    public extern void Clear();

    /// <summary>
    /// captureEvents
    /// </summary>
    [Description("@#captureEvents")]
    public extern void CaptureEvents();

    /// <summary>
    /// releaseEvents
    /// </summary>
    [Description("@#releaseEvents")]
    public extern void ReleaseEvents();

    /// <summary>
    /// all
    /// </summary>
    [Description("@#all")]
    public extern HTMLAllCollection All { get; }

    /// <summary>
    /// onfreeze
    /// </summary>
    [Description("@#onfreeze")]
    public extern EventHandler Onfreeze { get; set; }

    /// <summary>
    /// onresume
    /// </summary>
    [Description("@#onresume")]
    public extern EventHandler Onresume { get; set; }

    /// <summary>
    /// wasDiscarded
    /// </summary>
    [Description("@#wasDiscarded")]
    public extern bool WasDiscarded { get; }

    /// <summary>
    /// permissionsPolicy
    /// </summary>
    [Description("@#permissionsPolicy")]
    public extern PermissionsPolicy PermissionsPolicy { get; }

    /// <summary>
    /// pictureInPictureEnabled
    /// </summary>
    [Description("@#pictureInPictureEnabled")]
    public extern bool PictureInPictureEnabled { get; }

    /// <summary>
    /// exitPictureInPicture
    /// </summary>
    [Description("@#exitPictureInPicture")]
    public extern PromiseResult<object> ExitPictureInPicture();

    /// <summary>
    /// onpointerlockchange
    /// </summary>
    [Description("@#onpointerlockchange")]
    public extern EventHandler Onpointerlockchange { get; set; }

    /// <summary>
    /// onpointerlockerror
    /// </summary>
    [Description("@#onpointerlockerror")]
    public extern EventHandler Onpointerlockerror { get; set; }

    /// <summary>
    /// exitPointerLock
    /// </summary>
    [Description("@#exitPointerLock")]
    public extern void ExitPointerLock();

    /// <summary>
    /// prerendering
    /// </summary>
    [Description("@#prerendering")]
    public extern bool Prerendering { get; }

    /// <summary>
    /// onprerenderingchange
    /// </summary>
    [Description("@#onprerenderingchange")]
    public extern EventHandler Onprerenderingchange { get; set; }

    /// <summary>
    /// requestStorageAccessFor
    /// </summary>
    /// <param name="requestedOrigin">requestedOrigin</param>
    [Description("@#requestStorageAccessFor")]
    public extern PromiseResult<object> RequestStorageAccessFor(string requestedOrigin);

    /// <summary>
    /// hasUnpartitionedCookieAccess
    /// </summary>
    [Description("@#hasUnpartitionedCookieAccess")]
    public extern PromiseResult<bool> HasUnpartitionedCookieAccess();

    /// <summary>
    /// fragmentDirective
    /// </summary>
    [Description("@#fragmentDirective")]
    public extern FragmentDirective FragmentDirective { get; }

    /// <summary>
    /// getSelection
    /// </summary>
    [Description("@#getSelection")]
    public extern Selection? GetSelection();

    /// <summary>
    /// hasStorageAccess
    /// </summary>
    [Description("@#hasStorageAccess")]
    public extern PromiseResult<bool> HasStorageAccess();

    /// <summary>
    /// requestStorageAccess
    /// </summary>
    [Description("@#requestStorageAccess")]
    public extern PromiseResult<object> RequestStorageAccess();

    /// <summary>
    /// rootElement
    /// </summary>
    [Description("@#rootElement")]
    public extern SVGSVGElement? RootElement { get; }

    /// <summary>
    /// hasPrivateToken
    /// </summary>
    /// <param name="issuer">issuer</param>
    [Description("@#hasPrivateToken")]
    public extern PromiseResult<bool> HasPrivateToken(string issuer);

    /// <summary>
    /// hasRedemptionRecord
    /// </summary>
    /// <param name="issuer">issuer</param>
    [Description("@#hasRedemptionRecord")]
    public extern PromiseResult<bool> HasRedemptionRecord(string issuer);

    /// <summary>
    /// timeline
    /// </summary>
    [Description("@#timeline")]
    public extern DocumentTimeline Timeline { get; }

    #region mixin FontFaceSource
    /// <summary>
    /// fonts
    /// </summary>
    [Description("@#fonts")]
    public extern FontFaceSet Fonts { get; }
    #endregion

    #region mixin GeometryUtils
    /// <summary>
    /// getBoxQuads
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#getBoxQuads")]
    public extern DOMQuad[] GetBoxQuads(BoxQuadOptions? options = default);

    /// <summary>
    /// convertQuadFromNode
    /// </summary>
    /// <param name="quad">quad</param>
    /// <param name="from">from</param>
    /// <param name="options">options</param>
    [Description("@#convertQuadFromNode")]
    public extern DOMQuad ConvertQuadFromNode(DOMQuadInit quad, GeometryNode from, ConvertCoordinateOptions? options = default);

    /// <summary>
    /// convertRectFromNode
    /// </summary>
    /// <param name="rect">rect</param>
    /// <param name="from">from</param>
    /// <param name="options">options</param>
    [Description("@#convertRectFromNode")]
    public extern DOMQuad ConvertRectFromNode(DOMRectReadOnly rect, GeometryNode from, ConvertCoordinateOptions? options = default);

    /// <summary>
    /// convertPointFromNode
    /// </summary>
    /// <param name="point">point</param>
    /// <param name="from">from</param>
    /// <param name="options">options</param>
    [Description("@#convertPointFromNode")]
    public extern DOMPoint ConvertPointFromNode(DOMPointInit point, GeometryNode from, ConvertCoordinateOptions? options = default);
    #endregion

    #region mixin NonElementParentNode
    /// <summary>
    /// getElementById
    /// </summary>
    /// <param name="elementId">elementId</param>
    [Description("@#getElementById")]
    public extern Element? GetElementById(string elementId);
    #endregion

    #region mixin DocumentOrShadowRoot
    /// <summary>
    /// styleSheets
    /// </summary>
    [Description("@#styleSheets")]
    public extern StyleSheetList StyleSheets { get; }

    /// <summary>
    /// adoptedStyleSheets
    /// </summary>
    [Description("@#adoptedStyleSheets")]
    public extern ObservableCollection<CSSStyleSheet> AdoptedStyleSheets { get; set; }

    /// <summary>
    /// customElementRegistry
    /// </summary>
    [Description("@#customElementRegistry")]
    public extern CustomElementRegistry? CustomElementRegistry { get; }

    /// <summary>
    /// fullscreenElement
    /// </summary>
    [Description("@#fullscreenElement")]
    public extern Element? FullscreenElement { get; }

    /// <summary>
    /// activeElement
    /// </summary>
    [Description("@#activeElement")]
    public extern Element? ActiveElement { get; }

    /// <summary>
    /// pictureInPictureElement
    /// </summary>
    [Description("@#pictureInPictureElement")]
    public extern Element? PictureInPictureElement { get; }

    /// <summary>
    /// pointerLockElement
    /// </summary>
    [Description("@#pointerLockElement")]
    public extern Element? PointerLockElement { get; }

    /// <summary>
    /// getAnimations
    /// </summary>
    [Description("@#getAnimations")]
    public extern Animation[] GetAnimations();
    #endregion

    #region mixin ParentNode
    /// <summary>
    /// children
    /// </summary>
    [Description("@#children")]
    public extern HTMLCollection Children { get; }

    /// <summary>
    /// firstElementChild
    /// </summary>
    [Description("@#firstElementChild")]
    public extern Element? FirstElementChild { get; }

    /// <summary>
    /// lastElementChild
    /// </summary>
    [Description("@#lastElementChild")]
    public extern Element? LastElementChild { get; }

    /// <summary>
    /// childElementCount
    /// </summary>
    [Description("@#childElementCount")]
    public extern uint ChildElementCount { get; }

    /// <summary>
    /// prepend
    /// </summary>
    /// <param name="nodes">nodes</param>
    [Description("@#prepend")]
    public extern void Prepend(Either<Node, string> nodes);

    /// <summary>
    /// prepend
    /// </summary>
    /// <param name="nodes">nodes</param>
    [Description("@#prepend")]
    public extern void Prepend(Node nodes);

    /// <summary>
    /// prepend
    /// </summary>
    /// <param name="nodes">nodes</param>
    [Description("@#prepend")]
    public extern void Prepend(string nodes);

    /// <summary>
    /// append
    /// </summary>
    /// <param name="nodes">nodes</param>
    [Description("@#append")]
    public extern void Append(Either<Node, string> nodes);

    /// <summary>
    /// append
    /// </summary>
    /// <param name="nodes">nodes</param>
    [Description("@#append")]
    public extern void Append(Node nodes);

    /// <summary>
    /// append
    /// </summary>
    /// <param name="nodes">nodes</param>
    [Description("@#append")]
    public extern void Append(string nodes);

    /// <summary>
    /// replaceChildren
    /// </summary>
    /// <param name="nodes">nodes</param>
    [Description("@#replaceChildren")]
    public extern void ReplaceChildren(Either<Node, string> nodes);

    /// <summary>
    /// replaceChildren
    /// </summary>
    /// <param name="nodes">nodes</param>
    [Description("@#replaceChildren")]
    public extern void ReplaceChildren(Node nodes);

    /// <summary>
    /// replaceChildren
    /// </summary>
    /// <param name="nodes">nodes</param>
    [Description("@#replaceChildren")]
    public extern void ReplaceChildren(string nodes);

    /// <summary>
    /// moveBefore
    /// </summary>
    /// <param name="node">node</param>
    /// <param name="child">child</param>
    [Description("@#moveBefore")]
    public extern void MoveBefore(Node node, Node? child);

    /// <summary>
    /// querySelector
    /// </summary>
    /// <param name="selectors">selectors</param>
    [Description("@#querySelector")]
    public extern Element? QuerySelector(string selectors);

    /// <summary>
    /// querySelectorAll
    /// </summary>
    /// <param name="selectors">selectors</param>
    [Description("@#querySelectorAll")]
    public extern NodeList QuerySelectorAll(string selectors);
    #endregion

    #region mixin XPathEvaluatorBase
    /// <summary>
    /// createExpression
    /// </summary>
    /// <param name="expression">expression</param>
    /// <param name="resolver">resolver</param>
    [Description("@#createExpression")]
    public extern XPathExpression CreateExpression(string expression, XPathNSResolver? resolver = null);

    /// <summary>
    /// createNSResolver
    /// </summary>
    /// <param name="nodeResolver">nodeResolver</param>
    [Description("@#createNSResolver")]
    public extern Node CreateNSResolver(Node nodeResolver);

    /// <summary>
    /// evaluate
    /// </summary>
    /// <param name="expression">expression</param>
    /// <param name="contextNode">contextNode</param>
    /// <param name="resolver">resolver</param>
    /// <param name="type">type</param>
    /// <param name="result">result</param>
    [Description("@#evaluate")]
    public extern XPathResult Evaluate(string expression, Node contextNode, XPathNSResolver? resolver = null, ushort type = 0, XPathResult? result = null);
    #endregion

    #region mixin GlobalEventHandlers
    /// <summary>
    /// onanimationstart
    /// </summary>
    [Description("@#onanimationstart")]
    public extern EventHandler Onanimationstart { get; set; }

    /// <summary>
    /// onanimationiteration
    /// </summary>
    [Description("@#onanimationiteration")]
    public extern EventHandler Onanimationiteration { get; set; }

    /// <summary>
    /// onanimationend
    /// </summary>
    [Description("@#onanimationend")]
    public extern EventHandler Onanimationend { get; set; }

    /// <summary>
    /// onanimationcancel
    /// </summary>
    [Description("@#onanimationcancel")]
    public extern EventHandler Onanimationcancel { get; set; }

    /// <summary>
    /// onsnapchanged
    /// </summary>
    [Description("@#onsnapchanged")]
    public extern EventHandler Onsnapchanged { get; set; }

    /// <summary>
    /// onsnapchanging
    /// </summary>
    [Description("@#onsnapchanging")]
    public extern EventHandler Onsnapchanging { get; set; }

    /// <summary>
    /// ontransitionrun
    /// </summary>
    [Description("@#ontransitionrun")]
    public extern EventHandler Ontransitionrun { get; set; }

    /// <summary>
    /// ontransitionstart
    /// </summary>
    [Description("@#ontransitionstart")]
    public extern EventHandler Ontransitionstart { get; set; }

    /// <summary>
    /// ontransitionend
    /// </summary>
    [Description("@#ontransitionend")]
    public extern EventHandler Ontransitionend { get; set; }

    /// <summary>
    /// ontransitioncancel
    /// </summary>
    [Description("@#ontransitioncancel")]
    public extern EventHandler Ontransitioncancel { get; set; }

    /// <summary>
    /// onfencedtreeclick
    /// </summary>
    [Description("@#onfencedtreeclick")]
    public extern EventHandler Onfencedtreeclick { get; set; }

    /// <summary>
    /// onabort
    /// </summary>
    [Description("@#onabort")]
    public extern EventHandler Onabort { get; set; }

    /// <summary>
    /// onauxclick
    /// </summary>
    [Description("@#onauxclick")]
    public extern EventHandler Onauxclick { get; set; }

    /// <summary>
    /// onbeforeinput
    /// </summary>
    [Description("@#onbeforeinput")]
    public extern EventHandler Onbeforeinput { get; set; }

    /// <summary>
    /// onbeforematch
    /// </summary>
    [Description("@#onbeforematch")]
    public extern EventHandler Onbeforematch { get; set; }

    /// <summary>
    /// onbeforetoggle
    /// </summary>
    [Description("@#onbeforetoggle")]
    public extern EventHandler Onbeforetoggle { get; set; }

    /// <summary>
    /// onblur
    /// </summary>
    [Description("@#onblur")]
    public extern EventHandler Onblur { get; set; }

    /// <summary>
    /// oncancel
    /// </summary>
    [Description("@#oncancel")]
    public extern EventHandler Oncancel { get; set; }

    /// <summary>
    /// oncanplay
    /// </summary>
    [Description("@#oncanplay")]
    public extern EventHandler Oncanplay { get; set; }

    /// <summary>
    /// oncanplaythrough
    /// </summary>
    [Description("@#oncanplaythrough")]
    public extern EventHandler Oncanplaythrough { get; set; }

    /// <summary>
    /// onchange
    /// </summary>
    [Description("@#onchange")]
    public extern EventHandler Onchange { get; set; }

    /// <summary>
    /// onclick
    /// </summary>
    [Description("@#onclick")]
    public extern EventHandler Onclick { get; set; }

    /// <summary>
    /// onclose
    /// </summary>
    [Description("@#onclose")]
    public extern EventHandler Onclose { get; set; }

    /// <summary>
    /// oncommand
    /// </summary>
    [Description("@#oncommand")]
    public extern EventHandler Oncommand { get; set; }

    /// <summary>
    /// oncontextlost
    /// </summary>
    [Description("@#oncontextlost")]
    public extern EventHandler Oncontextlost { get; set; }

    /// <summary>
    /// oncontextmenu
    /// </summary>
    [Description("@#oncontextmenu")]
    public extern EventHandler Oncontextmenu { get; set; }

    /// <summary>
    /// oncontextrestored
    /// </summary>
    [Description("@#oncontextrestored")]
    public extern EventHandler Oncontextrestored { get; set; }

    /// <summary>
    /// oncopy
    /// </summary>
    [Description("@#oncopy")]
    public extern EventHandler Oncopy { get; set; }

    /// <summary>
    /// oncuechange
    /// </summary>
    [Description("@#oncuechange")]
    public extern EventHandler Oncuechange { get; set; }

    /// <summary>
    /// oncut
    /// </summary>
    [Description("@#oncut")]
    public extern EventHandler Oncut { get; set; }

    /// <summary>
    /// ondblclick
    /// </summary>
    [Description("@#ondblclick")]
    public extern EventHandler Ondblclick { get; set; }

    /// <summary>
    /// ondrag
    /// </summary>
    [Description("@#ondrag")]
    public extern EventHandler Ondrag { get; set; }

    /// <summary>
    /// ondragend
    /// </summary>
    [Description("@#ondragend")]
    public extern EventHandler Ondragend { get; set; }

    /// <summary>
    /// ondragenter
    /// </summary>
    [Description("@#ondragenter")]
    public extern EventHandler Ondragenter { get; set; }

    /// <summary>
    /// ondragleave
    /// </summary>
    [Description("@#ondragleave")]
    public extern EventHandler Ondragleave { get; set; }

    /// <summary>
    /// ondragover
    /// </summary>
    [Description("@#ondragover")]
    public extern EventHandler Ondragover { get; set; }

    /// <summary>
    /// ondragstart
    /// </summary>
    [Description("@#ondragstart")]
    public extern EventHandler Ondragstart { get; set; }

    /// <summary>
    /// ondrop
    /// </summary>
    [Description("@#ondrop")]
    public extern EventHandler Ondrop { get; set; }

    /// <summary>
    /// ondurationchange
    /// </summary>
    [Description("@#ondurationchange")]
    public extern EventHandler Ondurationchange { get; set; }

    /// <summary>
    /// onemptied
    /// </summary>
    [Description("@#onemptied")]
    public extern EventHandler Onemptied { get; set; }

    /// <summary>
    /// onended
    /// </summary>
    [Description("@#onended")]
    public extern EventHandler Onended { get; set; }

    /// <summary>
    /// onerror
    /// </summary>
    [Description("@#onerror")]
    public extern OnErrorEventHandler Onerror { get; set; }

    /// <summary>
    /// onfocus
    /// </summary>
    [Description("@#onfocus")]
    public extern EventHandler Onfocus { get; set; }

    /// <summary>
    /// onformdata
    /// </summary>
    [Description("@#onformdata")]
    public extern EventHandler Onformdata { get; set; }

    /// <summary>
    /// oninput
    /// </summary>
    [Description("@#oninput")]
    public extern EventHandler Oninput { get; set; }

    /// <summary>
    /// oninvalid
    /// </summary>
    [Description("@#oninvalid")]
    public extern EventHandler Oninvalid { get; set; }

    /// <summary>
    /// onkeydown
    /// </summary>
    [Description("@#onkeydown")]
    public extern EventHandler Onkeydown { get; set; }

    /// <summary>
    /// onkeypress
    /// </summary>
    [Description("@#onkeypress")]
    public extern EventHandler Onkeypress { get; set; }

    /// <summary>
    /// onkeyup
    /// </summary>
    [Description("@#onkeyup")]
    public extern EventHandler Onkeyup { get; set; }

    /// <summary>
    /// onload
    /// </summary>
    [Description("@#onload")]
    public extern EventHandler Onload { get; set; }

    /// <summary>
    /// onloadeddata
    /// </summary>
    [Description("@#onloadeddata")]
    public extern EventHandler Onloadeddata { get; set; }

    /// <summary>
    /// onloadedmetadata
    /// </summary>
    [Description("@#onloadedmetadata")]
    public extern EventHandler Onloadedmetadata { get; set; }

    /// <summary>
    /// onloadstart
    /// </summary>
    [Description("@#onloadstart")]
    public extern EventHandler Onloadstart { get; set; }

    /// <summary>
    /// onmousedown
    /// </summary>
    [Description("@#onmousedown")]
    public extern EventHandler Onmousedown { get; set; }

    /// <summary>
    /// onmouseenter
    /// </summary>
    [Description("@#onmouseenter")]
    public extern EventHandler Onmouseenter { get; set; }

    /// <summary>
    /// onmouseleave
    /// </summary>
    [Description("@#onmouseleave")]
    public extern EventHandler Onmouseleave { get; set; }

    /// <summary>
    /// onmousemove
    /// </summary>
    [Description("@#onmousemove")]
    public extern EventHandler Onmousemove { get; set; }

    /// <summary>
    /// onmouseout
    /// </summary>
    [Description("@#onmouseout")]
    public extern EventHandler Onmouseout { get; set; }

    /// <summary>
    /// onmouseover
    /// </summary>
    [Description("@#onmouseover")]
    public extern EventHandler Onmouseover { get; set; }

    /// <summary>
    /// onmouseup
    /// </summary>
    [Description("@#onmouseup")]
    public extern EventHandler Onmouseup { get; set; }

    /// <summary>
    /// onpaste
    /// </summary>
    [Description("@#onpaste")]
    public extern EventHandler Onpaste { get; set; }

    /// <summary>
    /// onpause
    /// </summary>
    [Description("@#onpause")]
    public extern EventHandler Onpause { get; set; }

    /// <summary>
    /// onplay
    /// </summary>
    [Description("@#onplay")]
    public extern EventHandler Onplay { get; set; }

    /// <summary>
    /// onplaying
    /// </summary>
    [Description("@#onplaying")]
    public extern EventHandler Onplaying { get; set; }

    /// <summary>
    /// onprogress
    /// </summary>
    [Description("@#onprogress")]
    public extern EventHandler Onprogress { get; set; }

    /// <summary>
    /// onratechange
    /// </summary>
    [Description("@#onratechange")]
    public extern EventHandler Onratechange { get; set; }

    /// <summary>
    /// onreset
    /// </summary>
    [Description("@#onreset")]
    public extern EventHandler Onreset { get; set; }

    /// <summary>
    /// onresize
    /// </summary>
    [Description("@#onresize")]
    public extern EventHandler Onresize { get; set; }

    /// <summary>
    /// onscroll
    /// </summary>
    [Description("@#onscroll")]
    public extern EventHandler Onscroll { get; set; }

    /// <summary>
    /// onscrollend
    /// </summary>
    [Description("@#onscrollend")]
    public extern EventHandler Onscrollend { get; set; }

    /// <summary>
    /// onsecuritypolicyviolation
    /// </summary>
    [Description("@#onsecuritypolicyviolation")]
    public extern EventHandler Onsecuritypolicyviolation { get; set; }

    /// <summary>
    /// onseeked
    /// </summary>
    [Description("@#onseeked")]
    public extern EventHandler Onseeked { get; set; }

    /// <summary>
    /// onseeking
    /// </summary>
    [Description("@#onseeking")]
    public extern EventHandler Onseeking { get; set; }

    /// <summary>
    /// onselect
    /// </summary>
    [Description("@#onselect")]
    public extern EventHandler Onselect { get; set; }

    /// <summary>
    /// onslotchange
    /// </summary>
    [Description("@#onslotchange")]
    public extern EventHandler Onslotchange { get; set; }

    /// <summary>
    /// onstalled
    /// </summary>
    [Description("@#onstalled")]
    public extern EventHandler Onstalled { get; set; }

    /// <summary>
    /// onsubmit
    /// </summary>
    [Description("@#onsubmit")]
    public extern EventHandler Onsubmit { get; set; }

    /// <summary>
    /// onsuspend
    /// </summary>
    [Description("@#onsuspend")]
    public extern EventHandler Onsuspend { get; set; }

    /// <summary>
    /// ontimeupdate
    /// </summary>
    [Description("@#ontimeupdate")]
    public extern EventHandler Ontimeupdate { get; set; }

    /// <summary>
    /// ontoggle
    /// </summary>
    [Description("@#ontoggle")]
    public extern EventHandler Ontoggle { get; set; }

    /// <summary>
    /// onvolumechange
    /// </summary>
    [Description("@#onvolumechange")]
    public extern EventHandler Onvolumechange { get; set; }

    /// <summary>
    /// onwaiting
    /// </summary>
    [Description("@#onwaiting")]
    public extern EventHandler Onwaiting { get; set; }

    /// <summary>
    /// onwebkitanimationend
    /// </summary>
    [Description("@#onwebkitanimationend")]
    public extern EventHandler Onwebkitanimationend { get; set; }

    /// <summary>
    /// onwebkitanimationiteration
    /// </summary>
    [Description("@#onwebkitanimationiteration")]
    public extern EventHandler Onwebkitanimationiteration { get; set; }

    /// <summary>
    /// onwebkitanimationstart
    /// </summary>
    [Description("@#onwebkitanimationstart")]
    public extern EventHandler Onwebkitanimationstart { get; set; }

    /// <summary>
    /// onwebkittransitionend
    /// </summary>
    [Description("@#onwebkittransitionend")]
    public extern EventHandler Onwebkittransitionend { get; set; }

    /// <summary>
    /// onwheel
    /// </summary>
    [Description("@#onwheel")]
    public extern EventHandler Onwheel { get; set; }

    /// <summary>
    /// onpointerover
    /// </summary>
    [Description("@#onpointerover")]
    public extern EventHandler Onpointerover { get; set; }

    /// <summary>
    /// onpointerenter
    /// </summary>
    [Description("@#onpointerenter")]
    public extern EventHandler Onpointerenter { get; set; }

    /// <summary>
    /// onpointerdown
    /// </summary>
    [Description("@#onpointerdown")]
    public extern EventHandler Onpointerdown { get; set; }

    /// <summary>
    /// onpointermove
    /// </summary>
    [Description("@#onpointermove")]
    public extern EventHandler Onpointermove { get; set; }

    /// <summary>
    /// onpointerrawupdate
    /// </summary>
    [Description("@#onpointerrawupdate")]
    public extern EventHandler Onpointerrawupdate { get; set; }

    /// <summary>
    /// onpointerup
    /// </summary>
    [Description("@#onpointerup")]
    public extern EventHandler Onpointerup { get; set; }

    /// <summary>
    /// onpointercancel
    /// </summary>
    [Description("@#onpointercancel")]
    public extern EventHandler Onpointercancel { get; set; }

    /// <summary>
    /// onpointerout
    /// </summary>
    [Description("@#onpointerout")]
    public extern EventHandler Onpointerout { get; set; }

    /// <summary>
    /// onpointerleave
    /// </summary>
    [Description("@#onpointerleave")]
    public extern EventHandler Onpointerleave { get; set; }

    /// <summary>
    /// ongotpointercapture
    /// </summary>
    [Description("@#ongotpointercapture")]
    public extern EventHandler Ongotpointercapture { get; set; }

    /// <summary>
    /// onlostpointercapture
    /// </summary>
    [Description("@#onlostpointercapture")]
    public extern EventHandler Onlostpointercapture { get; set; }

    /// <summary>
    /// onselectstart
    /// </summary>
    [Description("@#onselectstart")]
    public extern EventHandler Onselectstart { get; set; }

    /// <summary>
    /// onselectionchange
    /// </summary>
    [Description("@#onselectionchange")]
    public extern EventHandler Onselectionchange { get; set; }

    /// <summary>
    /// ontouchstart
    /// </summary>
    [Description("@#ontouchstart")]
    public extern EventHandler Ontouchstart { get; set; }

    /// <summary>
    /// ontouchend
    /// </summary>
    [Description("@#ontouchend")]
    public extern EventHandler Ontouchend { get; set; }

    /// <summary>
    /// ontouchmove
    /// </summary>
    [Description("@#ontouchmove")]
    public extern EventHandler Ontouchmove { get; set; }

    /// <summary>
    /// ontouchcancel
    /// </summary>
    [Description("@#ontouchcancel")]
    public extern EventHandler Ontouchcancel { get; set; }

    /// <summary>
    /// onbeforexrselect
    /// </summary>
    [Description("@#onbeforexrselect")]
    public extern EventHandler Onbeforexrselect { get; set; }
    #endregion
}

/// <summary>
/// NamedFlowMap
/// </summary>
[ECMAScript]
[Description("@#NamedFlowMap")]
public class NamedFlowMap : IDictionary<string, NamedFlow>
{
    #region Dictionary
    extern NamedFlow IDictionary<string, NamedFlow>.this[string key] { get; set; }
    extern ICollection<string> IDictionary<string, NamedFlow>.Keys { get; }
    extern ICollection<NamedFlow> IDictionary<string, NamedFlow>.Values { get; }
    extern int ICollection<KeyValuePair<string, NamedFlow>>.Count { get; }
    extern bool ICollection<KeyValuePair<string, NamedFlow>>.IsReadOnly { get; }
    extern void IDictionary<string, NamedFlow>.Add(string key, NamedFlow value);
    extern void ICollection<KeyValuePair<string, NamedFlow>>.Add(KeyValuePair<string, NamedFlow> item);
    extern void ICollection<KeyValuePair<string, NamedFlow>>.Clear();
    extern bool ICollection<KeyValuePair<string, NamedFlow>>.Contains(KeyValuePair<string, NamedFlow> item);
    extern bool IDictionary<string, NamedFlow>.ContainsKey(string key);
    extern void ICollection<KeyValuePair<string, NamedFlow>>.CopyTo(KeyValuePair<string, NamedFlow>[] array, int arrayIndex);
    extern IEnumerator<KeyValuePair<string, NamedFlow>> IEnumerable<KeyValuePair<string, NamedFlow>>.GetEnumerator();
    extern bool IDictionary<string, NamedFlow>.Remove(string key);
    extern bool ICollection<KeyValuePair<string, NamedFlow>>.Remove(KeyValuePair<string, NamedFlow> item);
    extern bool IDictionary<string, NamedFlow>.TryGetValue(string key, [MaybeNullWhen(false)] out NamedFlow value);
    extern IEnumerator IEnumerable.GetEnumerator();
    #endregion
}

/// <summary>
/// NamedFlow
/// </summary>
[ECMAScript]
[Description("@#NamedFlow")]
public class NamedFlow : EventTarget
{
    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; }

    /// <summary>
    /// overset
    /// </summary>
    [Description("@#overset")]
    public extern bool Overset { get; }

    /// <summary>
    /// getRegions
    /// </summary>
    [Description("@#getRegions")]
    public extern Element[] GetRegions();

    /// <summary>
    /// firstEmptyRegionIndex
    /// </summary>
    [Description("@#firstEmptyRegionIndex")]
    public extern short FirstEmptyRegionIndex { get; }

    /// <summary>
    /// getContent
    /// </summary>
    [Description("@#getContent")]
    public extern Node[] GetContent();

    /// <summary>
    /// getRegionsByContent
    /// </summary>
    /// <param name="node">node</param>
    [Description("@#getRegionsByContent")]
    public extern Element[] GetRegionsByContent(Node node);
}