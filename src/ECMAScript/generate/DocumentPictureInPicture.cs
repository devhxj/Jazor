namespace ECMAScript;

/// <summary>
/// DocumentPictureInPictureOptions
/// </summary>
[ECMAScript]
[Description("@#DocumentPictureInPictureOptions")]
public record DocumentPictureInPictureOptions(
    [property: Description("@#width")]ulong Width = 0,
	[property: Description("@#height")]ulong Height = 0,
	[property: Description("@#disallowReturnToOpener")]bool DisallowReturnToOpener = false,
	[property: Description("@#preferInitialWindowPlacement")]bool PreferInitialWindowPlacement = false);

/// <summary>
/// DocumentPictureInPictureEventInit
/// </summary>
[ECMAScript]
[Description("@#DocumentPictureInPictureEventInit")]
public record DocumentPictureInPictureEventInit(
    [property: Description("@#window")]Window? Window = default): EventInit;

/// <summary>
/// DocumentPictureInPicture
/// </summary>
[ECMAScript]
[Description("@#DocumentPictureInPicture")]
public class DocumentPictureInPicture : EventTarget
{
    /// <summary>
    /// requestWindow
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#requestWindow")]
    public extern PromiseResult<Window> RequestWindow(DocumentPictureInPictureOptions? options = default);

    /// <summary>
    /// window
    /// </summary>
    [Description("@#window")]
    public extern Window Window { get; }

    /// <summary>
    /// onenter
    /// </summary>
    [Description("@#onenter")]
    public extern EventHandler Onenter { get; set; }
}

/// <summary>
/// DocumentPictureInPictureEvent
/// </summary>
[ECMAScript]
[Description("@#DocumentPictureInPictureEvent")]
public class DocumentPictureInPictureEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern DocumentPictureInPictureEvent(string type, DocumentPictureInPictureEventInit eventInitDict);

    /// <summary>
    /// window
    /// </summary>
    [Description("@#window")]
    public extern Window Window { get; }
}