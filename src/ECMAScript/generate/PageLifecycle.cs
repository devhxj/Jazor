namespace ECMAScript;

/// <summary>
/// Client
/// </summary>
[ECMAScript]
[Description("@#Client")]
public partial class Client
{
    /// <summary>
    /// lifecycleState
    /// </summary>
    [Description("@#lifecycleState")]
    public extern ClientLifecycleState LifecycleState { get; }

    /// <summary>
    /// url
    /// </summary>
    [Description("@#url")]
    public extern string Url { get; }

    /// <summary>
    /// frameType
    /// </summary>
    [Description("@#frameType")]
    public extern FrameType FrameType { get; }

    /// <summary>
    /// id
    /// </summary>
    [Description("@#id")]
    public extern string Id { get; }

    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern ClientType Type { get; }

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
}