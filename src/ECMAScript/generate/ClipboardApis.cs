namespace ECMAScript;

/// <summary>
/// ClipboardEventInit
/// </summary>
[ECMAScript]
[Description("@#ClipboardEventInit")]
public record ClipboardEventInit(
    [property: Description("@#clipboardData")]DataTransfer? ClipboardData = null): EventInit;

/// <summary>
/// ClipboardItemOptions
/// </summary>
[ECMAScript]
[Description("@#ClipboardItemOptions")]
public record ClipboardItemOptions(
    [property: Description("@#presentationStyle")]PresentationStyle PresentationStyle = PresentationStyle.Unspecified);

/// <summary>
/// ClipboardUnsanitizedFormats
/// </summary>
[ECMAScript]
[Description("@#ClipboardUnsanitizedFormats")]
public record ClipboardUnsanitizedFormats(
    [property: Description("@#unsanitized")]string[]? Unsanitized = default);

/// <summary>
/// ClipboardPermissionDescriptor
/// </summary>
[ECMAScript]
[Description("@#ClipboardPermissionDescriptor")]
public record ClipboardPermissionDescriptor(
    [property: Description("@#allowWithoutGesture")]bool AllowWithoutGesture = false): PermissionDescriptor;

/// <summary>
/// ClipboardEvent
/// </summary>
[ECMAScript]
[Description("@#ClipboardEvent")]
public class ClipboardEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern ClipboardEvent(string type, ClipboardEventInit eventInitDict);

    /// <summary>
    /// clipboardData
    /// </summary>
    [Description("@#clipboardData")]
    public extern DataTransfer? ClipboardData { get; }
}

/// <summary>
/// ClipboardItem
/// </summary>
[ECMAScript]
[Description("@#ClipboardItem")]
public class ClipboardItem
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="items">items</param>
    /// <param name="options">options</param>
    public extern ClipboardItem(Dictionary<string, ClipboardItemData> items, ClipboardItemOptions options);

    /// <summary>
    /// presentationStyle
    /// </summary>
    [Description("@#presentationStyle")]
    public extern PresentationStyle PresentationStyle { get; }

    /// <summary>
    /// types
    /// </summary>
    [Description("@#types")]
    public extern FrozenSet<string> Types { get; }

    /// <summary>
    /// getType
    /// </summary>
    /// <param name="type">type</param>
    [Description("@#getType")]
    public extern PromiseResult<Blob> GetType(string type);

    /// <summary>
    /// supports
    /// </summary>
    /// <param name="type">type</param>
    [Description("@#supports")]
    public extern static bool Supports(string type);
}

/// <summary>
/// Clipboard
/// </summary>
[ECMAScript]
[Description("@#Clipboard")]
public class Clipboard : EventTarget
{
    /// <summary>
    /// read
    /// </summary>
    /// <param name="formats">formats</param>
    [Description("@#read")]
    public extern PromiseResult<ClipboardItems> Read(ClipboardUnsanitizedFormats? formats = default);

    /// <summary>
    /// readText
    /// </summary>
    [Description("@#readText")]
    public extern PromiseResult<string> ReadText();

    /// <summary>
    /// write
    /// </summary>
    /// <param name="data">data</param>
    [Description("@#write")]
    public extern PromiseResult<object> Write(ClipboardItems data);

    /// <summary>
    /// writeText
    /// </summary>
    /// <param name="data">data</param>
    [Description("@#writeText")]
    public extern PromiseResult<object> WriteText(string data);
}