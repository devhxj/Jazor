namespace ECMAScript;

/// <summary>
/// EditContextInit
/// </summary>
[ECMAScript]
[Description("@#EditContextInit")]
public record EditContextInit(
    [property: Description("@#text")]string? Text = default,
	[property: Description("@#selectionStart")]uint SelectionStart = default,
	[property: Description("@#selectionEnd")]uint SelectionEnd = default);

/// <summary>
/// TextUpdateEventInit
/// </summary>
[ECMAScript]
[Description("@#TextUpdateEventInit")]
public record TextUpdateEventInit(
    [property: Description("@#updateRangeStart")]uint UpdateRangeStart = default,
	[property: Description("@#updateRangeEnd")]uint UpdateRangeEnd = default,
	[property: Description("@#text")]string? Text = default,
	[property: Description("@#selectionStart")]uint SelectionStart = default,
	[property: Description("@#selectionEnd")]uint SelectionEnd = default,
	[property: Description("@#compositionStart")]uint CompositionStart = default,
	[property: Description("@#compositionEnd")]uint CompositionEnd = default): EventInit;

/// <summary>
/// TextFormatInit
/// </summary>
[ECMAScript]
[Description("@#TextFormatInit")]
public record TextFormatInit(
    [property: Description("@#rangeStart")]uint RangeStart = default,
	[property: Description("@#rangeEnd")]uint RangeEnd = default,
	[property: Description("@#underlineStyle")]UnderlineStyle? UnderlineStyle = default,
	[property: Description("@#underlineThickness")]UnderlineThickness? UnderlineThickness = default);

/// <summary>
/// TextFormatUpdateEventInit
/// </summary>
[ECMAScript]
[Description("@#TextFormatUpdateEventInit")]
public record TextFormatUpdateEventInit(
    [property: Description("@#textFormats")]TextFormat[]? TextFormats = default): EventInit;

/// <summary>
/// CharacterBoundsUpdateEventInit
/// </summary>
[ECMAScript]
[Description("@#CharacterBoundsUpdateEventInit")]
public record CharacterBoundsUpdateEventInit(
    [property: Description("@#rangeStart")]uint RangeStart = default,
	[property: Description("@#rangeEnd")]uint RangeEnd = default): EventInit;

/// <summary>
/// EditContext
/// </summary>
[ECMAScript]
[Description("@#EditContext")]
public class EditContext : EventTarget
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="options">options</param>
    public extern EditContext(EditContextInit options);

    /// <summary>
    /// updateText
    /// </summary>
    /// <param name="rangeStart">rangeStart</param>
    /// <param name="rangeEnd">rangeEnd</param>
    /// <param name="text">text</param>
    [Description("@#updateText")]
    public extern void UpdateText(uint rangeStart, uint rangeEnd, string text);

    /// <summary>
    /// updateSelection
    /// </summary>
    /// <param name="start">start</param>
    /// <param name="end">end</param>
    [Description("@#updateSelection")]
    public extern void UpdateSelection(uint start, uint end);

    /// <summary>
    /// updateControlBounds
    /// </summary>
    /// <param name="controlBounds">controlBounds</param>
    [Description("@#updateControlBounds")]
    public extern void UpdateControlBounds(DOMRect controlBounds);

    /// <summary>
    /// updateSelectionBounds
    /// </summary>
    /// <param name="selectionBounds">selectionBounds</param>
    [Description("@#updateSelectionBounds")]
    public extern void UpdateSelectionBounds(DOMRect selectionBounds);

    /// <summary>
    /// updateCharacterBounds
    /// </summary>
    /// <param name="rangeStart">rangeStart</param>
    /// <param name="characterBounds">characterBounds</param>
    [Description("@#updateCharacterBounds")]
    public extern void UpdateCharacterBounds(uint rangeStart, DOMRect[] characterBounds);

    /// <summary>
    /// attachedElements
    /// </summary>
    [Description("@#attachedElements")]
    public extern HTMLElement[] AttachedElements();

    /// <summary>
    /// text
    /// </summary>
    [Description("@#text")]
    public extern string Text { get; }

    /// <summary>
    /// selectionStart
    /// </summary>
    [Description("@#selectionStart")]
    public extern uint SelectionStart { get; }

    /// <summary>
    /// selectionEnd
    /// </summary>
    [Description("@#selectionEnd")]
    public extern uint SelectionEnd { get; }

    /// <summary>
    /// characterBoundsRangeStart
    /// </summary>
    [Description("@#characterBoundsRangeStart")]
    public extern uint CharacterBoundsRangeStart { get; }

    /// <summary>
    /// characterBounds
    /// </summary>
    [Description("@#characterBounds")]
    public extern DOMRect[] CharacterBounds();

    /// <summary>
    /// ontextupdate
    /// </summary>
    [Description("@#ontextupdate")]
    public extern EventHandler Ontextupdate { get; set; }

    /// <summary>
    /// ontextformatupdate
    /// </summary>
    [Description("@#ontextformatupdate")]
    public extern EventHandler Ontextformatupdate { get; set; }

    /// <summary>
    /// oncharacterboundsupdate
    /// </summary>
    [Description("@#oncharacterboundsupdate")]
    public extern EventHandler Oncharacterboundsupdate { get; set; }

    /// <summary>
    /// oncompositionstart
    /// </summary>
    [Description("@#oncompositionstart")]
    public extern EventHandler Oncompositionstart { get; set; }

    /// <summary>
    /// oncompositionend
    /// </summary>
    [Description("@#oncompositionend")]
    public extern EventHandler Oncompositionend { get; set; }
}

/// <summary>
/// TextUpdateEvent
/// </summary>
[ECMAScript]
[Description("@#TextUpdateEvent")]
public class TextUpdateEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="options">options</param>
    public extern TextUpdateEvent(string type, TextUpdateEventInit options);

    /// <summary>
    /// updateRangeStart
    /// </summary>
    [Description("@#updateRangeStart")]
    public extern uint UpdateRangeStart { get; }

    /// <summary>
    /// updateRangeEnd
    /// </summary>
    [Description("@#updateRangeEnd")]
    public extern uint UpdateRangeEnd { get; }

    /// <summary>
    /// text
    /// </summary>
    [Description("@#text")]
    public extern string Text { get; }

    /// <summary>
    /// selectionStart
    /// </summary>
    [Description("@#selectionStart")]
    public extern uint SelectionStart { get; }

    /// <summary>
    /// selectionEnd
    /// </summary>
    [Description("@#selectionEnd")]
    public extern uint SelectionEnd { get; }
}

/// <summary>
/// TextFormat
/// </summary>
[ECMAScript]
[Description("@#TextFormat")]
public class TextFormat
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="options">options</param>
    public extern TextFormat(TextFormatInit options);

    /// <summary>
    /// rangeStart
    /// </summary>
    [Description("@#rangeStart")]
    public extern uint RangeStart { get; }

    /// <summary>
    /// rangeEnd
    /// </summary>
    [Description("@#rangeEnd")]
    public extern uint RangeEnd { get; }

    /// <summary>
    /// underlineStyle
    /// </summary>
    [Description("@#underlineStyle")]
    public extern UnderlineStyle UnderlineStyle { get; }

    /// <summary>
    /// underlineThickness
    /// </summary>
    [Description("@#underlineThickness")]
    public extern UnderlineThickness UnderlineThickness { get; }
}

/// <summary>
/// TextFormatUpdateEvent
/// </summary>
[ECMAScript]
[Description("@#TextFormatUpdateEvent")]
public class TextFormatUpdateEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="options">options</param>
    public extern TextFormatUpdateEvent(string type, TextFormatUpdateEventInit options);

    /// <summary>
    /// getTextFormats
    /// </summary>
    [Description("@#getTextFormats")]
    public extern TextFormat[] GetTextFormats();
}

/// <summary>
/// CharacterBoundsUpdateEvent
/// </summary>
[ECMAScript]
[Description("@#CharacterBoundsUpdateEvent")]
public class CharacterBoundsUpdateEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="options">options</param>
    public extern CharacterBoundsUpdateEvent(string type, CharacterBoundsUpdateEventInit options);

    /// <summary>
    /// rangeStart
    /// </summary>
    [Description("@#rangeStart")]
    public extern uint RangeStart { get; }

    /// <summary>
    /// rangeEnd
    /// </summary>
    [Description("@#rangeEnd")]
    public extern uint RangeEnd { get; }
}