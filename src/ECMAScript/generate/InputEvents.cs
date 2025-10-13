namespace ECMAScript;

/// <summary>
/// InputEventInit
/// </summary>
[ECMAScript]
[Description("@#InputEventInit")]
public record InputEventInit(
    [property: Description("@#dataTransfer")]DataTransfer? DataTransfer = null,
	[property: Description("@#targetRanges")]StaticRange[]? TargetRanges = default,
	[property: Description("@#data")]string? Data = null,
	[property: Description("@#isComposing")]bool IsComposing = false,
	[property: Description("@#inputType")]string? InputType = default): UIEventInit
{
    [Category("optional")]
    public extern static InputEventInit OptionalDataTransferTargetRanges(
        [Description("@#dataTransfer")]DataTransfer? dataTransfer = null,
        [Description("@#targetRanges")]StaticRange[]? targetRanges = default);

    [Category("optional")]
    public extern static InputEventInit OptionalDataIsComposingInputType(
        [Description("@#data")]string? data = null,
        [Description("@#isComposing")]bool isComposing = false,
        [Description("@#inputType")]string? inputType = default);
}

/// <summary>
/// InputEvent
/// </summary>
[ECMAScript]
[Description("@#InputEvent")]
public partial class InputEvent(string type, UIEventInit eventInitDict) : UIEvent(type, eventInitDict)
{
    /// <summary>
    /// dataTransfer
    /// </summary>
    [Description("@#dataTransfer")]
    public extern DataTransfer? DataTransfer { get; }

    /// <summary>
    /// getTargetRanges
    /// </summary>
    [Description("@#getTargetRanges")]
    public extern StaticRange[] GetTargetRanges();

    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern InputEvent(string type, InputEventInit eventInitDict);

    /// <summary>
    /// data
    /// </summary>
    [Description("@#data")]
    public extern string? Data { get; }

    /// <summary>
    /// isComposing
    /// </summary>
    [Description("@#isComposing")]
    public extern bool IsComposing { get; }

    /// <summary>
    /// inputType
    /// </summary>
    [Description("@#inputType")]
    public extern string InputType { get; }
}