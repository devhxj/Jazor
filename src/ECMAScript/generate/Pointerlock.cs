namespace ECMAScript;

/// <summary>
/// PointerLockOptions
/// </summary>
[ECMAScript]
[Description("@#PointerLockOptions")]
public record PointerLockOptions(
    [property: Description("@#unadjustedMovement")]bool UnadjustedMovement = false);

/// <summary>
/// MouseEventInit
/// </summary>
[ECMAScript]
[Description("@#MouseEventInit")]
public record MouseEventInit(
    [property: Description("@#movementX")]double MovementX = 0d,
	[property: Description("@#movementY")]double MovementY = 0d,
	[property: Description("@#screenX")]int ScreenX = 0,
	[property: Description("@#screenY")]int ScreenY = 0,
	[property: Description("@#clientX")]int ClientX = 0,
	[property: Description("@#clientY")]int ClientY = 0,
	[property: Description("@#button")]short Button = 0,
	[property: Description("@#buttons")]ushort Buttons = 0,
	[property: Description("@#relatedTarget")]EventTarget? RelatedTarget = null): EventModifierInit
{
    [Category("optional")]
    public extern static MouseEventInit OptionalMovementXMovementY(
        [Description("@#movementX")]double movementX = 0d,
        [Description("@#movementY")]double movementY = 0d);

    [Category("optional")]
    public extern static MouseEventInit OptionalScreenXScreenYClientX7(
        [Description("@#screenX")]int screenX = 0,
        [Description("@#screenY")]int screenY = 0,
        [Description("@#clientX")]int clientX = 0,
        [Description("@#clientY")]int clientY = 0,
        [Description("@#button")]short button = 0,
        [Description("@#buttons")]ushort buttons = 0,
        [Description("@#relatedTarget")]EventTarget? relatedTarget = null);
}