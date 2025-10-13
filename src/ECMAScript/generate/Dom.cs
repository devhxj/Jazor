namespace ECMAScript;

/// <summary>
/// EventInit
/// </summary>
[ECMAScript]
[Description("@#EventInit")]
public record EventInit(
    [property: Description("@#bubbles")]bool Bubbles = false,
	[property: Description("@#cancelable")]bool Cancelable = false,
	[property: Description("@#composed")]bool Composed = false);

/// <summary>
/// CustomEventInit
/// </summary>
[ECMAScript]
[Description("@#CustomEventInit")]
public record CustomEventInit(
    [property: Description("@#detail")]object? Detail = default): EventInit;

/// <summary>
/// EventListenerOptions
/// </summary>
[ECMAScript]
[Description("@#EventListenerOptions")]
public record EventListenerOptions(
    [property: Description("@#capture")]bool Capture = false);

/// <summary>
/// AddEventListenerOptions
/// </summary>
[ECMAScript]
[Description("@#AddEventListenerOptions")]
public record AddEventListenerOptions(
    [property: Description("@#passive")]bool Passive = default,
	[property: Description("@#once")]bool Once = false,
	[property: Description("@#signal")]AbortSignal? Signal = default): EventListenerOptions;

/// <summary>
/// MutationObserverInit
/// </summary>
[ECMAScript]
[Description("@#MutationObserverInit")]
public record MutationObserverInit(
    [property: Description("@#childList")]bool ChildList = false,
	[property: Description("@#attributes")]bool Attributes = default,
	[property: Description("@#characterData")]bool CharacterData = default,
	[property: Description("@#subtree")]bool Subtree = false,
	[property: Description("@#attributeOldValue")]bool AttributeOldValue = default,
	[property: Description("@#characterDataOldValue")]bool CharacterDataOldValue = default,
	[property: Description("@#attributeFilter")]string[]? AttributeFilter = default);

/// <summary>
/// GetRootNodeOptions
/// </summary>
[ECMAScript]
[Description("@#GetRootNodeOptions")]
public record GetRootNodeOptions(
    [property: Description("@#composed")]bool Composed = false);

/// <summary>
/// ElementCreationOptions
/// </summary>
[ECMAScript]
[Description("@#ElementCreationOptions")]
public record ElementCreationOptions(
    [property: Description("@#customElementRegistry")]CustomElementRegistry? CustomElementRegistry = default,
	[property: Description("@#is")]string? Is = default);

/// <summary>
/// ImportNodeOptions
/// </summary>
[ECMAScript]
[Description("@#ImportNodeOptions")]
public record ImportNodeOptions(
    [property: Description("@#customElementRegistry")]CustomElementRegistry? CustomElementRegistry = default,
	[property: Description("@#selfOnly")]bool SelfOnly = false);

/// <summary>
/// ShadowRootInit
/// </summary>
[ECMAScript]
[Description("@#ShadowRootInit")]
public record ShadowRootInit(
    [property: Description("@#mode")]ShadowRootMode? Mode = default,
	[property: Description("@#delegatesFocus")]bool DelegatesFocus = false,
	[property: Description("@#slotAssignment")]SlotAssignmentMode SlotAssignment = SlotAssignmentMode.Named,
	[property: Description("@#clonable")]bool Clonable = false,
	[property: Description("@#serializable")]bool Serializable = false,
	[property: Description("@#customElementRegistry")]CustomElementRegistry? CustomElementRegistry = default);

/// <summary>
/// StaticRangeInit
/// </summary>
[ECMAScript]
[Description("@#StaticRangeInit")]
public record StaticRangeInit(
    [property: Description("@#startContainer")]Node? StartContainer = default,
	[property: Description("@#startOffset")]uint StartOffset = default,
	[property: Description("@#endContainer")]Node? EndContainer = default,
	[property: Description("@#endOffset")]uint EndOffset = default);

/// <summary>
/// Event
/// </summary>
[ECMAScript]
[Description("@#Event")]
public class Event
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern Event(string type, EventInit eventInitDict);

    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern string Type { get; }

    /// <summary>
    /// target
    /// </summary>
    [Description("@#target")]
    public extern EventTarget? Target { get; }

    /// <summary>
    /// srcElement
    /// </summary>
    [Description("@#srcElement")]
    public extern EventTarget? SrcElement { get; }

    /// <summary>
    /// currentTarget
    /// </summary>
    [Description("@#currentTarget")]
    public extern EventTarget? CurrentTarget { get; }

    /// <summary>
    /// composedPath
    /// </summary>
    [Description("@#composedPath")]
    public extern EventTarget[] ComposedPath();

    /// <summary>
    /// NONE
    /// </summary>
    [Description("@#NONE")]
    public const ushort NONE = 0;

    /// <summary>
    /// CAPTURING_PHASE
    /// </summary>
    [Description("@#CAPTURING_PHASE")]
    public const ushort CAPTURING_PHASE = 1;

    /// <summary>
    /// AT_TARGET
    /// </summary>
    [Description("@#AT_TARGET")]
    public const ushort AT_TARGET = 2;

    /// <summary>
    /// BUBBLING_PHASE
    /// </summary>
    [Description("@#BUBBLING_PHASE")]
    public const ushort BUBBLING_PHASE = 3;

    /// <summary>
    /// eventPhase
    /// </summary>
    [Description("@#eventPhase")]
    public extern ushort EventPhase { get; }

    /// <summary>
    /// stopPropagation
    /// </summary>
    [Description("@#stopPropagation")]
    public extern void StopPropagation();

    /// <summary>
    /// cancelBubble
    /// </summary>
    [Description("@#cancelBubble")]
    public extern bool CancelBubble { get; set; }

    /// <summary>
    /// stopImmediatePropagation
    /// </summary>
    [Description("@#stopImmediatePropagation")]
    public extern void StopImmediatePropagation();

    /// <summary>
    /// bubbles
    /// </summary>
    [Description("@#bubbles")]
    public extern bool Bubbles { get; }

    /// <summary>
    /// cancelable
    /// </summary>
    [Description("@#cancelable")]
    public extern bool Cancelable { get; }

    /// <summary>
    /// returnValue
    /// </summary>
    [Description("@#returnValue")]
    public extern bool ReturnValue { get; set; }

    /// <summary>
    /// preventDefault
    /// </summary>
    [Description("@#preventDefault")]
    public extern void PreventDefault();

    /// <summary>
    /// defaultPrevented
    /// </summary>
    [Description("@#defaultPrevented")]
    public extern bool DefaultPrevented { get; }

    /// <summary>
    /// composed
    /// </summary>
    [Description("@#composed")]
    public extern bool Composed { get; }

    /// <summary>
    /// isTrusted
    /// </summary>
    [Description("@#isTrusted")]
    public extern bool IsTrusted { get; }

    /// <summary>
    /// timeStamp
    /// </summary>
    [Description("@#timeStamp")]
    public extern double TimeStamp { get; }

    /// <summary>
    /// initEvent
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="bubbles">bubbles</param>
    /// <param name="cancelable">cancelable</param>
    [Description("@#initEvent")]
    public extern void InitEvent(string type, bool bubbles = false, bool cancelable = false);
}

/// <summary>
/// CustomEvent
/// </summary>
[ECMAScript]
[Description("@#CustomEvent")]
public class CustomEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern CustomEvent(string type, CustomEventInit eventInitDict);

    /// <summary>
    /// detail
    /// </summary>
    [Description("@#detail")]
    public extern object Detail { get; }

    /// <summary>
    /// initCustomEvent
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="bubbles">bubbles</param>
    /// <param name="cancelable">cancelable</param>
    /// <param name="detail">detail</param>
    [Description("@#initCustomEvent")]
    public extern void InitCustomEvent(string type, bool bubbles = false, bool cancelable = false, object? detail = default);
}

/// <summary>
/// EventTarget
/// </summary>
[ECMAScript]
[Description("@#EventTarget")]
public partial class EventTarget
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern EventTarget();

    /// <summary>
    /// addEventListener
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="callback">callback</param>
    /// <param name="options">options</param>
    [Description("@#addEventListener")]
    public extern void AddEventListener(string type, EventListener? callback, Either<AddEventListenerOptions, bool> options);

    /// <summary>
    /// addEventListener
    /// </summary>
    /// <param name="type">type para</param>
    /// <param name="callback">callback para</param>
    /// <param name="options">options</param>
    [Description("@#addEventListener")]
    public extern void AddEventListener(string type, EventListener? callback, AddEventListenerOptions? options = default);

    /// <summary>
    /// addEventListener
    /// </summary>
    /// <param name="type">type para</param>
    /// <param name="callback">callback para</param>
    /// <param name="options">options</param>
    [Description("@#addEventListener")]
    public extern void AddEventListener(string type, EventListener? callback, bool options);

    /// <summary>
    /// removeEventListener
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="callback">callback</param>
    /// <param name="options">options</param>
    [Description("@#removeEventListener")]
    public extern void RemoveEventListener(string type, EventListener? callback, Either<EventListenerOptions, bool> options);

    /// <summary>
    /// removeEventListener
    /// </summary>
    /// <param name="type">type para</param>
    /// <param name="callback">callback para</param>
    /// <param name="options">options</param>
    [Description("@#removeEventListener")]
    public extern void RemoveEventListener(string type, EventListener? callback, EventListenerOptions? options = default);

    /// <summary>
    /// removeEventListener
    /// </summary>
    /// <param name="type">type para</param>
    /// <param name="callback">callback para</param>
    /// <param name="options">options</param>
    [Description("@#removeEventListener")]
    public extern void RemoveEventListener(string type, EventListener? callback, bool options);

    /// <summary>
    /// dispatchEvent
    /// </summary>
    /// <param name="event">event</param>
    [Description("@#dispatchEvent")]
    public extern bool DispatchEvent(Event @event);

    /// <summary>
    /// when
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="options">options</param>
    [Description("@#when")]
    public extern Observable When(string type, ObservableEventListenerOptions? options = default);
}

/// <summary>
/// AbortController
/// </summary>
[ECMAScript]
[Description("@#AbortController")]
public class AbortController
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern AbortController();

    /// <summary>
    /// signal
    /// </summary>
    [Description("@#signal")]
    public extern AbortSignal Signal { get; }

    /// <summary>
    /// abort
    /// </summary>
    /// <param name="reason">reason</param>
    [Description("@#abort")]
    public extern void Abort(object reason);
}

/// <summary>
/// AbortSignal
/// </summary>
[ECMAScript]
[Description("@#AbortSignal")]
public class AbortSignal : EventTarget
{
    /// <summary>
    /// abort
    /// </summary>
    /// <param name="reason">reason</param>
    [Description("@#abort")]
    public extern static AbortSignal Abort(object reason);

    /// <summary>
    /// timeout
    /// </summary>
    /// <param name="milliseconds">milliseconds</param>
    [Description("@#timeout")]
    public extern static AbortSignal Timeout(ulong milliseconds);

    /// <summary>
    /// any
    /// </summary>
    /// <param name="signals">signals</param>
    [Description("@#any")]
    public extern static AbortSignal Any(AbortSignal[] signals);

    /// <summary>
    /// aborted
    /// </summary>
    [Description("@#aborted")]
    public extern bool Aborted { get; }

    /// <summary>
    /// reason
    /// </summary>
    [Description("@#reason")]
    public extern object Reason { get; }

    /// <summary>
    /// throwIfAborted
    /// </summary>
    [Description("@#throwIfAborted")]
    public extern void ThrowIfAborted();

    /// <summary>
    /// onabort
    /// </summary>
    [Description("@#onabort")]
    public extern EventHandler Onabort { get; set; }
}

/// <summary>
/// NodeList
/// </summary>
[ECMAScript]
[Description("@#NodeList")]
public class NodeList : IEnumerable<Node>
{
    /// <summary>
    /// item
    /// </summary>
    /// <param name="index">index</param>
    [Description("@#item")]
    public extern Node? GetItem(uint index);

    /// <summary>
    /// length
    /// </summary>
    [Description("@#length")]
    public extern uint Length { get; }

    extern IEnumerator<Node> IEnumerable<Node>.GetEnumerator();
    extern IEnumerator IEnumerable.GetEnumerator();
}

/// <summary>
/// HTMLCollection
/// </summary>
[ECMAScript]
[Description("@#HTMLCollection")]
public class HTMLCollection
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
    public extern Element? GetItem(uint index);

    /// <summary>
    /// namedItem
    /// </summary>
    /// <param name="name">name</param>
    [Description("@#namedItem")]
    public extern Element? NamedItem(string name);
}

/// <summary>
/// MutationObserver
/// </summary>
[ECMAScript]
[Description("@#MutationObserver")]
public class MutationObserver
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="callback">callback</param>
    public extern MutationObserver(MutationCallback callback);

    /// <summary>
    /// observe
    /// </summary>
    /// <param name="target">target</param>
    /// <param name="options">options</param>
    [Description("@#observe")]
    public extern void Observe(Node target, MutationObserverInit? options = default);

    /// <summary>
    /// disconnect
    /// </summary>
    [Description("@#disconnect")]
    public extern void Disconnect();

    /// <summary>
    /// takeRecords
    /// </summary>
    [Description("@#takeRecords")]
    public extern MutationRecord[] TakeRecords();
}

/// <summary>
/// MutationRecord
/// </summary>
[ECMAScript]
[Description("@#MutationRecord")]
public class MutationRecord
{
    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern string Type { get; }

    /// <summary>
    /// target
    /// </summary>
    [Description("@#target")]
    public extern Node Target { get; }

    /// <summary>
    /// addedNodes
    /// </summary>
    [Description("@#addedNodes")]
    public extern NodeList AddedNodes { get; }

    /// <summary>
    /// removedNodes
    /// </summary>
    [Description("@#removedNodes")]
    public extern NodeList RemovedNodes { get; }

    /// <summary>
    /// previousSibling
    /// </summary>
    [Description("@#previousSibling")]
    public extern Node? PreviousSibling { get; }

    /// <summary>
    /// nextSibling
    /// </summary>
    [Description("@#nextSibling")]
    public extern Node? NextSibling { get; }

    /// <summary>
    /// attributeName
    /// </summary>
    [Description("@#attributeName")]
    public extern string? AttributeName { get; }

    /// <summary>
    /// attributeNamespace
    /// </summary>
    [Description("@#attributeNamespace")]
    public extern string? AttributeNamespace { get; }

    /// <summary>
    /// oldValue
    /// </summary>
    [Description("@#oldValue")]
    public extern string? OldValue { get; }
}

/// <summary>
/// Node
/// </summary>
[ECMAScript]
[Description("@#Node")]
public class Node : EventTarget
{
    /// <summary>
    /// ELEMENT_NODE
    /// </summary>
    [Description("@#ELEMENT_NODE")]
    public const ushort ELEMENT_NODE = 1;

    /// <summary>
    /// ATTRIBUTE_NODE
    /// </summary>
    [Description("@#ATTRIBUTE_NODE")]
    public const ushort ATTRIBUTE_NODE = 2;

    /// <summary>
    /// TEXT_NODE
    /// </summary>
    [Description("@#TEXT_NODE")]
    public const ushort TEXT_NODE = 3;

    /// <summary>
    /// CDATA_SECTION_NODE
    /// </summary>
    [Description("@#CDATA_SECTION_NODE")]
    public const ushort CDATA_SECTION_NODE = 4;

    /// <summary>
    /// ENTITY_REFERENCE_NODE
    /// </summary>
    [Description("@#ENTITY_REFERENCE_NODE")]
    public const ushort ENTITY_REFERENCE_NODE = 5;

    /// <summary>
    /// ENTITY_NODE
    /// </summary>
    [Description("@#ENTITY_NODE")]
    public const ushort ENTITY_NODE = 6;

    /// <summary>
    /// PROCESSING_INSTRUCTION_NODE
    /// </summary>
    [Description("@#PROCESSING_INSTRUCTION_NODE")]
    public const ushort PROCESSING_INSTRUCTION_NODE = 7;

    /// <summary>
    /// COMMENT_NODE
    /// </summary>
    [Description("@#COMMENT_NODE")]
    public const ushort COMMENT_NODE = 8;

    /// <summary>
    /// DOCUMENT_NODE
    /// </summary>
    [Description("@#DOCUMENT_NODE")]
    public const ushort DOCUMENT_NODE = 9;

    /// <summary>
    /// DOCUMENT_TYPE_NODE
    /// </summary>
    [Description("@#DOCUMENT_TYPE_NODE")]
    public const ushort DOCUMENT_TYPE_NODE = 10;

    /// <summary>
    /// DOCUMENT_FRAGMENT_NODE
    /// </summary>
    [Description("@#DOCUMENT_FRAGMENT_NODE")]
    public const ushort DOCUMENT_FRAGMENT_NODE = 11;

    /// <summary>
    /// NOTATION_NODE
    /// </summary>
    [Description("@#NOTATION_NODE")]
    public const ushort NOTATION_NODE = 12;

    /// <summary>
    /// nodeType
    /// </summary>
    [Description("@#nodeType")]
    public extern ushort NodeType { get; }

    /// <summary>
    /// nodeName
    /// </summary>
    [Description("@#nodeName")]
    public extern string NodeName { get; }

    /// <summary>
    /// baseURI
    /// </summary>
    [Description("@#baseURI")]
    public extern string BaseURI { get; }

    /// <summary>
    /// isConnected
    /// </summary>
    [Description("@#isConnected")]
    public extern bool IsConnected { get; }

    /// <summary>
    /// ownerDocument
    /// </summary>
    [Description("@#ownerDocument")]
    public extern Document? OwnerDocument { get; }

    /// <summary>
    /// getRootNode
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#getRootNode")]
    public extern Node GetRootNode(GetRootNodeOptions? options = default);

    /// <summary>
    /// parentNode
    /// </summary>
    [Description("@#parentNode")]
    public extern Node? ParentNode { get; }

    /// <summary>
    /// parentElement
    /// </summary>
    [Description("@#parentElement")]
    public extern Element? ParentElement { get; }

    /// <summary>
    /// hasChildNodes
    /// </summary>
    [Description("@#hasChildNodes")]
    public extern bool HasChildNodes();

    /// <summary>
    /// childNodes
    /// </summary>
    [Description("@#childNodes")]
    public extern NodeList ChildNodes { get; }

    /// <summary>
    /// firstChild
    /// </summary>
    [Description("@#firstChild")]
    public extern Node? FirstChild { get; }

    /// <summary>
    /// lastChild
    /// </summary>
    [Description("@#lastChild")]
    public extern Node? LastChild { get; }

    /// <summary>
    /// previousSibling
    /// </summary>
    [Description("@#previousSibling")]
    public extern Node? PreviousSibling { get; }

    /// <summary>
    /// nextSibling
    /// </summary>
    [Description("@#nextSibling")]
    public extern Node? NextSibling { get; }

    /// <summary>
    /// nodeValue
    /// </summary>
    [Description("@#nodeValue")]
    public extern string? NodeValue { get; set; }

    /// <summary>
    /// textContent
    /// </summary>
    [Description("@#textContent")]
    public extern string? TextContent { get; set; }

    /// <summary>
    /// normalize
    /// </summary>
    [Description("@#normalize")]
    public extern void Normalize();

    /// <summary>
    /// cloneNode
    /// </summary>
    /// <param name="subtree">subtree</param>
    [Description("@#cloneNode")]
    public extern Node CloneNode(bool subtree = false);

    /// <summary>
    /// isEqualNode
    /// </summary>
    /// <param name="otherNode">otherNode</param>
    [Description("@#isEqualNode")]
    public extern bool IsEqualNode(Node? otherNode);

    /// <summary>
    /// isSameNode
    /// </summary>
    /// <param name="otherNode">otherNode</param>
    [Description("@#isSameNode")]
    public extern bool IsSameNode(Node? otherNode);

    /// <summary>
    /// DOCUMENT_POSITION_DISCONNECTED
    /// </summary>
    [Description("@#DOCUMENT_POSITION_DISCONNECTED")]
    public const ushort DOCUMENT_POSITION_DISCONNECTED = 0x01;

    /// <summary>
    /// DOCUMENT_POSITION_PRECEDING
    /// </summary>
    [Description("@#DOCUMENT_POSITION_PRECEDING")]
    public const ushort DOCUMENT_POSITION_PRECEDING = 0x02;

    /// <summary>
    /// DOCUMENT_POSITION_FOLLOWING
    /// </summary>
    [Description("@#DOCUMENT_POSITION_FOLLOWING")]
    public const ushort DOCUMENT_POSITION_FOLLOWING = 0x04;

    /// <summary>
    /// DOCUMENT_POSITION_CONTAINS
    /// </summary>
    [Description("@#DOCUMENT_POSITION_CONTAINS")]
    public const ushort DOCUMENT_POSITION_CONTAINS = 0x08;

    /// <summary>
    /// DOCUMENT_POSITION_CONTAINED_BY
    /// </summary>
    [Description("@#DOCUMENT_POSITION_CONTAINED_BY")]
    public const ushort DOCUMENT_POSITION_CONTAINED_BY = 0x10;

    /// <summary>
    /// DOCUMENT_POSITION_IMPLEMENTATION_SPECIFIC
    /// </summary>
    [Description("@#DOCUMENT_POSITION_IMPLEMENTATION_SPECIFIC")]
    public const ushort DOCUMENT_POSITION_IMPLEMENTATION_SPECIFIC = 0x20;

    /// <summary>
    /// compareDocumentPosition
    /// </summary>
    /// <param name="other">other</param>
    [Description("@#compareDocumentPosition")]
    public extern ushort CompareDocumentPosition(Node other);

    /// <summary>
    /// contains
    /// </summary>
    /// <param name="other">other</param>
    [Description("@#contains")]
    public extern bool Contains(Node? other);

    /// <summary>
    /// lookupPrefix
    /// </summary>
    /// <param name="namespace">namespace</param>
    [Description("@#lookupPrefix")]
    public extern string? LookupPrefix(string? @namespace);

    /// <summary>
    /// lookupNamespaceURI
    /// </summary>
    /// <param name="prefix">prefix</param>
    [Description("@#lookupNamespaceURI")]
    public extern string? LookupNamespaceURI(string? prefix);

    /// <summary>
    /// isDefaultNamespace
    /// </summary>
    /// <param name="namespace">namespace</param>
    [Description("@#isDefaultNamespace")]
    public extern bool IsDefaultNamespace(string? @namespace);

    /// <summary>
    /// insertBefore
    /// </summary>
    /// <param name="node">node</param>
    /// <param name="child">child</param>
    [Description("@#insertBefore")]
    public extern Node InsertBefore(Node node, Node? child);

    /// <summary>
    /// appendChild
    /// </summary>
    /// <param name="node">node</param>
    [Description("@#appendChild")]
    public extern Node AppendChild(Node node);

    /// <summary>
    /// replaceChild
    /// </summary>
    /// <param name="node">node</param>
    /// <param name="child">child</param>
    [Description("@#replaceChild")]
    public extern Node ReplaceChild(Node node, Node child);

    /// <summary>
    /// removeChild
    /// </summary>
    /// <param name="child">child</param>
    [Description("@#removeChild")]
    public extern Node RemoveChild(Node child);
}

/// <summary>
/// XMLDocument
/// </summary>
[ECMAScript]
[Description("@#XMLDocument")]
public class XMLDocument : Document
{

}

/// <summary>
/// DOMImplementation
/// </summary>
[ECMAScript]
[Description("@#DOMImplementation")]
public class DOMImplementation
{
    /// <summary>
    /// createDocumentType
    /// </summary>
    /// <param name="name">name</param>
    /// <param name="publicId">publicId</param>
    /// <param name="systemId">systemId</param>
    [Description("@#createDocumentType")]
    public extern DocumentType CreateDocumentType(string name, string publicId, string systemId);

    /// <summary>
    /// createDocument
    /// </summary>
    /// <param name="namespace">namespace</param>
    /// <param name="qualifiedName">qualifiedName</param>
    /// <param name="doctype">doctype</param>
    [Description("@#createDocument")]
    public extern XMLDocument CreateDocument(string? @namespace, string qualifiedName, DocumentType? doctype = null);

    /// <summary>
    /// createHTMLDocument
    /// </summary>
    /// <param name="title">title</param>
    [Description("@#createHTMLDocument")]
    public extern Document CreateHTMLDocument(string title);

    /// <summary>
    /// hasFeature
    /// </summary>
    [Description("@#hasFeature")]
    public extern bool HasFeature();
}

/// <summary>
/// DocumentType
/// </summary>
[ECMAScript]
[Description("@#DocumentType")]
public class DocumentType : Node
{
    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; }

    /// <summary>
    /// publicId
    /// </summary>
    [Description("@#publicId")]
    public extern string PublicId { get; }

    /// <summary>
    /// systemId
    /// </summary>
    [Description("@#systemId")]
    public extern string SystemId { get; }

    #region mixin ChildNode
    /// <summary>
    /// before
    /// </summary>
    /// <param name="nodes">nodes</param>
    [Description("@#before")]
    public extern void Before(Either<Node, string> nodes);

    /// <summary>
    /// before
    /// </summary>
    /// <param name="nodes">nodes</param>
    [Description("@#before")]
    public extern void Before(Node nodes);

    /// <summary>
    /// before
    /// </summary>
    /// <param name="nodes">nodes</param>
    [Description("@#before")]
    public extern void Before(string nodes);

    /// <summary>
    /// after
    /// </summary>
    /// <param name="nodes">nodes</param>
    [Description("@#after")]
    public extern void After(Either<Node, string> nodes);

    /// <summary>
    /// after
    /// </summary>
    /// <param name="nodes">nodes</param>
    [Description("@#after")]
    public extern void After(Node nodes);

    /// <summary>
    /// after
    /// </summary>
    /// <param name="nodes">nodes</param>
    [Description("@#after")]
    public extern void After(string nodes);

    /// <summary>
    /// replaceWith
    /// </summary>
    /// <param name="nodes">nodes</param>
    [Description("@#replaceWith")]
    public extern void ReplaceWith(Either<Node, string> nodes);

    /// <summary>
    /// replaceWith
    /// </summary>
    /// <param name="nodes">nodes</param>
    [Description("@#replaceWith")]
    public extern void ReplaceWith(Node nodes);

    /// <summary>
    /// replaceWith
    /// </summary>
    /// <param name="nodes">nodes</param>
    [Description("@#replaceWith")]
    public extern void ReplaceWith(string nodes);

    /// <summary>
    /// remove
    /// </summary>
    [Description("@#remove")]
    public extern void Remove();
    #endregion
}

/// <summary>
/// DocumentFragment
/// </summary>
[ECMAScript]
[Description("@#DocumentFragment")]
public class DocumentFragment : Node
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern DocumentFragment();

    #region mixin NonElementParentNode
    /// <summary>
    /// getElementById
    /// </summary>
    /// <param name="elementId">elementId</param>
    [Description("@#getElementById")]
    public extern Element? GetElementById(string elementId);
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
}

/// <summary>
/// ShadowRoot
/// </summary>
[ECMAScript]
[Description("@#ShadowRoot")]
public partial class ShadowRoot : DocumentFragment
{
    /// <summary>
    /// mode
    /// </summary>
    [Description("@#mode")]
    public extern ShadowRootMode Mode { get; }

    /// <summary>
    /// delegatesFocus
    /// </summary>
    [Description("@#delegatesFocus")]
    public extern bool DelegatesFocus { get; }

    /// <summary>
    /// slotAssignment
    /// </summary>
    [Description("@#slotAssignment")]
    public extern SlotAssignmentMode SlotAssignment { get; }

    /// <summary>
    /// clonable
    /// </summary>
    [Description("@#clonable")]
    public extern bool Clonable { get; }

    /// <summary>
    /// serializable
    /// </summary>
    [Description("@#serializable")]
    public extern bool Serializable { get; }

    /// <summary>
    /// host
    /// </summary>
    [Description("@#host")]
    public extern Element Host { get; }

    /// <summary>
    /// onslotchange
    /// </summary>
    [Description("@#onslotchange")]
    public extern EventHandler Onslotchange { get; set; }

    /// <summary>
    /// setHTMLUnsafe
    /// </summary>
    /// <param name="html">html</param>
    [Description("@#setHTMLUnsafe")]
    public extern void SetHTMLUnsafe(Either<TrustedHTML, string> html);

    /// <summary>
    /// setHTMLUnsafe
    /// </summary>
    /// <param name="html">html</param>
    [Description("@#setHTMLUnsafe")]
    public extern void SetHTMLUnsafe(TrustedHTML html);

    /// <summary>
    /// setHTMLUnsafe
    /// </summary>
    /// <param name="html">html</param>
    [Description("@#setHTMLUnsafe")]
    public extern void SetHTMLUnsafe(string html);

    /// <summary>
    /// getHTML
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#getHTML")]
    public extern string GetHTML(GetHTMLOptions? options = default);

    /// <summary>
    /// innerHTML
    /// </summary>
    [Description("@#innerHTML")]
    public extern Either<TrustedHTML, string> InnerHTML { get; set; }

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
}

/// <summary>
/// NamedNodeMap
/// </summary>
[ECMAScript]
[Description("@#NamedNodeMap")]
public class NamedNodeMap
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
    public extern Attr? GetItem(uint index);

    /// <summary>
    /// getNamedItem
    /// </summary>
    /// <param name="qualifiedName">qualifiedName</param>
    [Description("@#getNamedItem")]
    public extern Attr? GetNamedItem(string qualifiedName);

    /// <summary>
    /// getNamedItemNS
    /// </summary>
    /// <param name="namespace">namespace</param>
    /// <param name="localName">localName</param>
    [Description("@#getNamedItemNS")]
    public extern Attr? GetNamedItemNS(string? @namespace, string localName);

    /// <summary>
    /// setNamedItem
    /// </summary>
    /// <param name="attr">attr</param>
    [Description("@#setNamedItem")]
    public extern Attr? SetNamedItem(Attr attr);

    /// <summary>
    /// setNamedItemNS
    /// </summary>
    /// <param name="attr">attr</param>
    [Description("@#setNamedItemNS")]
    public extern Attr? SetNamedItemNS(Attr attr);

    /// <summary>
    /// removeNamedItem
    /// </summary>
    /// <param name="qualifiedName">qualifiedName</param>
    [Description("@#removeNamedItem")]
    public extern Attr RemoveNamedItem(string qualifiedName);

    /// <summary>
    /// removeNamedItemNS
    /// </summary>
    /// <param name="namespace">namespace</param>
    /// <param name="localName">localName</param>
    [Description("@#removeNamedItemNS")]
    public extern Attr RemoveNamedItemNS(string? @namespace, string localName);
}

/// <summary>
/// Attr
/// </summary>
[ECMAScript]
[Description("@#Attr")]
public class Attr : Node
{
    /// <summary>
    /// namespaceURI
    /// </summary>
    [Description("@#namespaceURI")]
    public extern string? NamespaceURI { get; }

    /// <summary>
    /// prefix
    /// </summary>
    [Description("@#prefix")]
    public extern string? Prefix { get; }

    /// <summary>
    /// localName
    /// </summary>
    [Description("@#localName")]
    public extern string LocalName { get; }

    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; }

    /// <summary>
    /// value
    /// </summary>
    [Description("@#value")]
    public extern string Value { get; set; }

    /// <summary>
    /// ownerElement
    /// </summary>
    [Description("@#ownerElement")]
    public extern Element? OwnerElement { get; }

    /// <summary>
    /// specified
    /// </summary>
    [Description("@#specified")]
    public extern bool Specified { get; }
}

/// <summary>
/// CharacterData
/// </summary>
[ECMAScript]
[Description("@#CharacterData")]
public class CharacterData : Node
{
    /// <summary>
    /// data
    /// </summary>
    [Description("@#data")]
    public extern string Data { get; set; }

    /// <summary>
    /// length
    /// </summary>
    [Description("@#length")]
    public extern uint Length { get; }

    /// <summary>
    /// substringData
    /// </summary>
    /// <param name="offset">offset</param>
    /// <param name="count">count</param>
    [Description("@#substringData")]
    public extern string SubstringData(uint offset, uint count);

    /// <summary>
    /// appendData
    /// </summary>
    /// <param name="data">data</param>
    [Description("@#appendData")]
    public extern void AppendData(string data);

    /// <summary>
    /// insertData
    /// </summary>
    /// <param name="offset">offset</param>
    /// <param name="data">data</param>
    [Description("@#insertData")]
    public extern void InsertData(uint offset, string data);

    /// <summary>
    /// deleteData
    /// </summary>
    /// <param name="offset">offset</param>
    /// <param name="count">count</param>
    [Description("@#deleteData")]
    public extern void DeleteData(uint offset, uint count);

    /// <summary>
    /// replaceData
    /// </summary>
    /// <param name="offset">offset</param>
    /// <param name="count">count</param>
    /// <param name="data">data</param>
    [Description("@#replaceData")]
    public extern void ReplaceData(uint offset, uint count, string data);

    #region mixin NonDocumentTypeChildNode
    /// <summary>
    /// previousElementSibling
    /// </summary>
    [Description("@#previousElementSibling")]
    public extern Element? PreviousElementSibling { get; }

    /// <summary>
    /// nextElementSibling
    /// </summary>
    [Description("@#nextElementSibling")]
    public extern Element? NextElementSibling { get; }
    #endregion

    #region mixin ChildNode
    /// <summary>
    /// before
    /// </summary>
    /// <param name="nodes">nodes</param>
    [Description("@#before")]
    public extern void Before(Either<Node, string> nodes);

    /// <summary>
    /// before
    /// </summary>
    /// <param name="nodes">nodes</param>
    [Description("@#before")]
    public extern void Before(Node nodes);

    /// <summary>
    /// before
    /// </summary>
    /// <param name="nodes">nodes</param>
    [Description("@#before")]
    public extern void Before(string nodes);

    /// <summary>
    /// after
    /// </summary>
    /// <param name="nodes">nodes</param>
    [Description("@#after")]
    public extern void After(Either<Node, string> nodes);

    /// <summary>
    /// after
    /// </summary>
    /// <param name="nodes">nodes</param>
    [Description("@#after")]
    public extern void After(Node nodes);

    /// <summary>
    /// after
    /// </summary>
    /// <param name="nodes">nodes</param>
    [Description("@#after")]
    public extern void After(string nodes);

    /// <summary>
    /// replaceWith
    /// </summary>
    /// <param name="nodes">nodes</param>
    [Description("@#replaceWith")]
    public extern void ReplaceWith(Either<Node, string> nodes);

    /// <summary>
    /// replaceWith
    /// </summary>
    /// <param name="nodes">nodes</param>
    [Description("@#replaceWith")]
    public extern void ReplaceWith(Node nodes);

    /// <summary>
    /// replaceWith
    /// </summary>
    /// <param name="nodes">nodes</param>
    [Description("@#replaceWith")]
    public extern void ReplaceWith(string nodes);

    /// <summary>
    /// remove
    /// </summary>
    [Description("@#remove")]
    public extern void Remove();
    #endregion
}

/// <summary>
/// Text
/// </summary>
[ECMAScript]
[Description("@#Text")]
public class Text : CharacterData
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="data">data</param>
    public extern Text(string data);

    /// <summary>
    /// splitText
    /// </summary>
    /// <param name="offset">offset</param>
    [Description("@#splitText")]
    public extern Text SplitText(uint offset);

    /// <summary>
    /// wholeText
    /// </summary>
    [Description("@#wholeText")]
    public extern string WholeText { get; }

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

    #region mixin Slottable
    /// <summary>
    /// assignedSlot
    /// </summary>
    [Description("@#assignedSlot")]
    public extern HTMLSlotElement? AssignedSlot { get; }
    #endregion
}

/// <summary>
/// CDATASection
/// </summary>
[ECMAScript]
[Description("@#CDATASection")]
public class CDATASection(string data) : Text(data)
{

}

/// <summary>
/// ProcessingInstruction
/// </summary>
[ECMAScript]
[Description("@#ProcessingInstruction")]
public class ProcessingInstruction : CharacterData
{
    /// <summary>
    /// target
    /// </summary>
    [Description("@#target")]
    public extern string Target { get; }

    #region mixin LinkStyle
    /// <summary>
    /// sheet
    /// </summary>
    [Description("@#sheet")]
    public extern CSSStyleSheet? Sheet { get; }
    #endregion
}

/// <summary>
/// Comment
/// </summary>
[ECMAScript]
[Description("@#Comment")]
public class Comment : CharacterData
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="data">data</param>
    public extern Comment(string data);
}

/// <summary>
/// AbstractRange
/// </summary>
[ECMAScript]
[Description("@#AbstractRange")]
public class AbstractRange
{
    /// <summary>
    /// startContainer
    /// </summary>
    [Description("@#startContainer")]
    public extern Node StartContainer { get; }

    /// <summary>
    /// startOffset
    /// </summary>
    [Description("@#startOffset")]
    public extern uint StartOffset { get; }

    /// <summary>
    /// endContainer
    /// </summary>
    [Description("@#endContainer")]
    public extern Node EndContainer { get; }

    /// <summary>
    /// endOffset
    /// </summary>
    [Description("@#endOffset")]
    public extern uint EndOffset { get; }

    /// <summary>
    /// collapsed
    /// </summary>
    [Description("@#collapsed")]
    public extern bool Collapsed { get; }
}

/// <summary>
/// StaticRange
/// </summary>
[ECMAScript]
[Description("@#StaticRange")]
public class StaticRange : AbstractRange
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="init">init</param>
    public extern StaticRange(StaticRangeInit init);
}

/// <summary>
/// NodeIterator
/// </summary>
[ECMAScript]
[Description("@#NodeIterator")]
public class NodeIterator
{
    /// <summary>
    /// root
    /// </summary>
    [Description("@#root")]
    public extern Node Root { get; }

    /// <summary>
    /// referenceNode
    /// </summary>
    [Description("@#referenceNode")]
    public extern Node ReferenceNode { get; }

    /// <summary>
    /// pointerBeforeReferenceNode
    /// </summary>
    [Description("@#pointerBeforeReferenceNode")]
    public extern bool PointerBeforeReferenceNode { get; }

    /// <summary>
    /// whatToShow
    /// </summary>
    [Description("@#whatToShow")]
    public extern uint WhatToShow { get; }

    /// <summary>
    /// filter
    /// </summary>
    [Description("@#filter")]
    public extern NodeFilter? Filter { get; }

    /// <summary>
    /// nextNode
    /// </summary>
    [Description("@#nextNode")]
    public extern Node? NextNode();

    /// <summary>
    /// previousNode
    /// </summary>
    [Description("@#previousNode")]
    public extern Node? PreviousNode();

    /// <summary>
    /// detach
    /// </summary>
    [Description("@#detach")]
    public extern void Detach();
}

/// <summary>
/// TreeWalker
/// </summary>
[ECMAScript]
[Description("@#TreeWalker")]
public class TreeWalker
{
    /// <summary>
    /// root
    /// </summary>
    [Description("@#root")]
    public extern Node Root { get; }

    /// <summary>
    /// whatToShow
    /// </summary>
    [Description("@#whatToShow")]
    public extern uint WhatToShow { get; }

    /// <summary>
    /// filter
    /// </summary>
    [Description("@#filter")]
    public extern NodeFilter? Filter { get; }

    /// <summary>
    /// currentNode
    /// </summary>
    [Description("@#currentNode")]
    public extern Node CurrentNode { get; set; }

    /// <summary>
    /// parentNode
    /// </summary>
    [Description("@#parentNode")]
    public extern Node? ParentNode();

    /// <summary>
    /// firstChild
    /// </summary>
    [Description("@#firstChild")]
    public extern Node? FirstChild();

    /// <summary>
    /// lastChild
    /// </summary>
    [Description("@#lastChild")]
    public extern Node? LastChild();

    /// <summary>
    /// previousSibling
    /// </summary>
    [Description("@#previousSibling")]
    public extern Node? PreviousSibling();

    /// <summary>
    /// nextSibling
    /// </summary>
    [Description("@#nextSibling")]
    public extern Node? NextSibling();

    /// <summary>
    /// previousNode
    /// </summary>
    [Description("@#previousNode")]
    public extern Node? PreviousNode();

    /// <summary>
    /// nextNode
    /// </summary>
    [Description("@#nextNode")]
    public extern Node? NextNode();
}

/// <summary>
/// DOMTokenList
/// </summary>
[ECMAScript]
[Description("@#DOMTokenList")]
public class DOMTokenList : IEnumerable<string>
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
    /// <param name="token">token</param>
    [Description("@#contains")]
    public extern bool Contains(string token);

    /// <summary>
    /// add
    /// </summary>
    /// <param name="tokens">tokens</param>
    [Description("@#add")]
    public extern void Add(string tokens);

    /// <summary>
    /// remove
    /// </summary>
    /// <param name="tokens">tokens</param>
    [Description("@#remove")]
    public extern void Remove(string tokens);

    /// <summary>
    /// toggle
    /// </summary>
    /// <param name="token">token</param>
    /// <param name="force">force</param>
    [Description("@#toggle")]
    public extern bool Toggle(string token, bool force);

    /// <summary>
    /// replace
    /// </summary>
    /// <param name="token">token</param>
    /// <param name="newToken">newToken</param>
    [Description("@#replace")]
    public extern bool Replace(string token, string newToken);

    /// <summary>
    /// supports
    /// </summary>
    /// <param name="token">token</param>
    [Description("@#supports")]
    public extern bool Supports(string token);

    /// <summary>
    /// value
    /// </summary>
    [Description("@#value")]
    public extern string Value { get; set; }

    extern IEnumerator<string> IEnumerable<string>.GetEnumerator();
    extern IEnumerator IEnumerable.GetEnumerator();
}

/// <summary>
/// XPathResult
/// </summary>
[ECMAScript]
[Description("@#XPathResult")]
public class XPathResult
{
    /// <summary>
    /// ANY_TYPE
    /// </summary>
    [Description("@#ANY_TYPE")]
    public const ushort ANY_TYPE = 0;

    /// <summary>
    /// NUMBER_TYPE
    /// </summary>
    [Description("@#NUMBER_TYPE")]
    public const ushort NUMBER_TYPE = 1;

    /// <summary>
    /// STRING_TYPE
    /// </summary>
    [Description("@#STRING_TYPE")]
    public const ushort STRING_TYPE = 2;

    /// <summary>
    /// BOOLEAN_TYPE
    /// </summary>
    [Description("@#BOOLEAN_TYPE")]
    public const ushort BOOLEAN_TYPE = 3;

    /// <summary>
    /// UNORDERED_NODE_ITERATOR_TYPE
    /// </summary>
    [Description("@#UNORDERED_NODE_ITERATOR_TYPE")]
    public const ushort UNORDERED_NODE_ITERATOR_TYPE = 4;

    /// <summary>
    /// ORDERED_NODE_ITERATOR_TYPE
    /// </summary>
    [Description("@#ORDERED_NODE_ITERATOR_TYPE")]
    public const ushort ORDERED_NODE_ITERATOR_TYPE = 5;

    /// <summary>
    /// UNORDERED_NODE_SNAPSHOT_TYPE
    /// </summary>
    [Description("@#UNORDERED_NODE_SNAPSHOT_TYPE")]
    public const ushort UNORDERED_NODE_SNAPSHOT_TYPE = 6;

    /// <summary>
    /// ORDERED_NODE_SNAPSHOT_TYPE
    /// </summary>
    [Description("@#ORDERED_NODE_SNAPSHOT_TYPE")]
    public const ushort ORDERED_NODE_SNAPSHOT_TYPE = 7;

    /// <summary>
    /// ANY_UNORDERED_NODE_TYPE
    /// </summary>
    [Description("@#ANY_UNORDERED_NODE_TYPE")]
    public const ushort ANY_UNORDERED_NODE_TYPE = 8;

    /// <summary>
    /// FIRST_ORDERED_NODE_TYPE
    /// </summary>
    [Description("@#FIRST_ORDERED_NODE_TYPE")]
    public const ushort FIRST_ORDERED_NODE_TYPE = 9;

    /// <summary>
    /// resultType
    /// </summary>
    [Description("@#resultType")]
    public extern ushort ResultType { get; }

    /// <summary>
    /// numberValue
    /// </summary>
    [Description("@#numberValue")]
    public extern double NumberValue { get; }

    /// <summary>
    /// stringValue
    /// </summary>
    [Description("@#stringValue")]
    public extern string StringValue { get; }

    /// <summary>
    /// booleanValue
    /// </summary>
    [Description("@#booleanValue")]
    public extern bool BooleanValue { get; }

    /// <summary>
    /// singleNodeValue
    /// </summary>
    [Description("@#singleNodeValue")]
    public extern Node? SingleNodeValue { get; }

    /// <summary>
    /// invalidIteratorState
    /// </summary>
    [Description("@#invalidIteratorState")]
    public extern bool InvalidIteratorState { get; }

    /// <summary>
    /// snapshotLength
    /// </summary>
    [Description("@#snapshotLength")]
    public extern uint SnapshotLength { get; }

    /// <summary>
    /// iterateNext
    /// </summary>
    [Description("@#iterateNext")]
    public extern Node? IterateNext();

    /// <summary>
    /// snapshotItem
    /// </summary>
    /// <param name="index">index</param>
    [Description("@#snapshotItem")]
    public extern Node? SnapshotItem(uint index);
}

/// <summary>
/// XPathExpression
/// </summary>
[ECMAScript]
[Description("@#XPathExpression")]
public class XPathExpression
{
    /// <summary>
    /// evaluate
    /// </summary>
    /// <param name="contextNode">contextNode</param>
    /// <param name="type">type</param>
    /// <param name="result">result</param>
    [Description("@#evaluate")]
    public extern XPathResult Evaluate(Node contextNode, ushort type = 0, XPathResult? result = null);
}

/// <summary>
/// XPathEvaluator
/// </summary>
[ECMAScript]
[Description("@#XPathEvaluator")]
public class XPathEvaluator
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern XPathEvaluator();

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
}

/// <summary>
/// XSLTProcessor
/// </summary>
[ECMAScript]
[Description("@#XSLTProcessor")]
public class XSLTProcessor
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern XSLTProcessor();

    /// <summary>
    /// importStylesheet
    /// </summary>
    /// <param name="style">style</param>
    [Description("@#importStylesheet")]
    public extern void ImportStylesheet(Node style);

    /// <summary>
    /// transformToFragment
    /// </summary>
    /// <param name="source">source</param>
    /// <param name="output">output</param>
    [Description("@#transformToFragment")]
    public extern DocumentFragment TransformToFragment(Node source, Document output);

    /// <summary>
    /// transformToDocument
    /// </summary>
    /// <param name="source">source</param>
    [Description("@#transformToDocument")]
    public extern Document TransformToDocument(Node source);

    /// <summary>
    /// setParameter
    /// </summary>
    /// <param name="namespaceUri">namespaceURI</param>
    /// <param name="localName">localName</param>
    /// <param name="value">value</param>
    [Description("@#setParameter")]
    public extern void SetParameter(string namespaceUri, string localName, object value);

    /// <summary>
    /// getParameter
    /// </summary>
    /// <param name="namespaceUri">namespaceURI</param>
    /// <param name="localName">localName</param>
    [Description("@#getParameter")]
    public extern object GetParameter(string namespaceUri, string localName);

    /// <summary>
    /// removeParameter
    /// </summary>
    /// <param name="namespaceUri">namespaceURI</param>
    /// <param name="localName">localName</param>
    [Description("@#removeParameter")]
    public extern void RemoveParameter(string namespaceUri, string localName);

    /// <summary>
    /// clearParameters
    /// </summary>
    [Description("@#clearParameters")]
    public extern void ClearParameters();

    /// <summary>
    /// reset
    /// </summary>
    [Description("@#reset")]
    public extern void Reset();
}