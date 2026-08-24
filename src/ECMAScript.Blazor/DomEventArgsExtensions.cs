using ECMAScript;
using ECMAScript.Contract;
using Microsoft.AspNetCore.Components.Web;

namespace ECMAScript.Blazor;

/// <summary>
/// Blazor DOM event adapters.
///
/// These are source-level mapping declarations, not a second runtime module. The
/// generated whitelist uses the original Blazor getter keys and lowers them to the
/// native event carrier that RazorVue already forwards to callbacks.
/// </summary>
[Jazor(Op.Alias, "Microsoft.AspNetCore.Components.Web.MouseEventArgs", "MouseEvent")]
internal static class MouseEventArgsExtensions
{
    [Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.MouseEventArgs.Detail.get", "__arg1.detail")]
    internal static long Detail(this MouseEventArgs instance) => instance.Detail;

    [Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.MouseEventArgs.ScreenX.get", "__arg1.screenX")]
    internal static double ScreenX(this MouseEventArgs instance) => instance.ScreenX;

    [Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.MouseEventArgs.ScreenY.get", "__arg1.screenY")]
    internal static double ScreenY(this MouseEventArgs instance) => instance.ScreenY;

    [Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.MouseEventArgs.ClientX.get", "__arg1.clientX")]
    internal static double ClientX(this MouseEventArgs instance) => instance.ClientX;

    [Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.MouseEventArgs.ClientY.get", "__arg1.clientY")]
    internal static double ClientY(this MouseEventArgs instance) => instance.ClientY;

    [Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.MouseEventArgs.OffsetX.get", "__arg1.offsetX")]
    internal static double OffsetX(this MouseEventArgs instance) => instance.OffsetX;

    [Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.MouseEventArgs.OffsetY.get", "__arg1.offsetY")]
    internal static double OffsetY(this MouseEventArgs instance) => instance.OffsetY;

    [Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.MouseEventArgs.PageX.get", "__arg1.pageX")]
    internal static double PageX(this MouseEventArgs instance) => instance.PageX;

    [Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.MouseEventArgs.PageY.get", "__arg1.pageY")]
    internal static double PageY(this MouseEventArgs instance) => instance.PageY;

    [Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.MouseEventArgs.MovementX.get", "__arg1.movementX")]
    internal static double MovementX(this MouseEventArgs instance) => instance.MovementX;

    [Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.MouseEventArgs.MovementY.get", "__arg1.movementY")]
    internal static double MovementY(this MouseEventArgs instance) => instance.MovementY;

    [Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.MouseEventArgs.Button.get", "__arg1.button")]
    internal static long Button(this MouseEventArgs instance) => instance.Button;

    [Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.MouseEventArgs.Buttons.get", "__arg1.buttons")]
    internal static long Buttons(this MouseEventArgs instance) => instance.Buttons;

    [Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.MouseEventArgs.CtrlKey.get", "__arg1.ctrlKey")]
    internal static bool CtrlKey(this MouseEventArgs instance) => instance.CtrlKey;

    [Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.MouseEventArgs.ShiftKey.get", "__arg1.shiftKey")]
    internal static bool ShiftKey(this MouseEventArgs instance) => instance.ShiftKey;

    [Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.MouseEventArgs.AltKey.get", "__arg1.altKey")]
    internal static bool AltKey(this MouseEventArgs instance) => instance.AltKey;

    [Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.MouseEventArgs.MetaKey.get", "__arg1.metaKey")]
    internal static bool MetaKey(this MouseEventArgs instance) => instance.MetaKey;

    [Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.MouseEventArgs.Type.get", "__arg1.type")]
    internal static string Type(this MouseEventArgs instance) => instance.Type;
}

/// <summary>Maps read-only keyboard event arguments to the native KeyboardEvent carrier.</summary>
[Jazor(Op.Alias, "Microsoft.AspNetCore.Components.Web.KeyboardEventArgs", "KeyboardEvent")]
internal static class KeyboardEventArgsExtensions
{
    [Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.KeyboardEventArgs.Key.get", "__arg1.key")]
    internal static string Key(this KeyboardEventArgs instance) => instance.Key;

    [Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.KeyboardEventArgs.Code.get", "__arg1.code")]
    internal static string Code(this KeyboardEventArgs instance) => instance.Code;

    [Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.KeyboardEventArgs.Location.get", "__arg1.location")]
    internal static float Location(this KeyboardEventArgs instance) => instance.Location;

    [Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.KeyboardEventArgs.Repeat.get", "__arg1.repeat")]
    internal static bool Repeat(this KeyboardEventArgs instance) => instance.Repeat;

    [Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.KeyboardEventArgs.CtrlKey.get", "__arg1.ctrlKey")]
    internal static bool CtrlKey(this KeyboardEventArgs instance) => instance.CtrlKey;

    [Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.KeyboardEventArgs.ShiftKey.get", "__arg1.shiftKey")]
    internal static bool ShiftKey(this KeyboardEventArgs instance) => instance.ShiftKey;

    [Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.KeyboardEventArgs.AltKey.get", "__arg1.altKey")]
    internal static bool AltKey(this KeyboardEventArgs instance) => instance.AltKey;

    [Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.KeyboardEventArgs.MetaKey.get", "__arg1.metaKey")]
    internal static bool MetaKey(this KeyboardEventArgs instance) => instance.MetaKey;

    [Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.KeyboardEventArgs.Type.get", "__arg1.type")]
    internal static string Type(this KeyboardEventArgs instance) => instance.Type;

    [Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.KeyboardEventArgs.IsComposing.get", "__arg1.isComposing")]
    internal static bool IsComposing(this KeyboardEventArgs instance) => instance.IsComposing;
}

/// <summary>Maps the focus event type to the native FocusEvent carrier.</summary>
[Jazor(Op.Alias, "Microsoft.AspNetCore.Components.Web.FocusEventArgs", "FocusEvent")]
internal static class FocusEventArgsExtensions
{
    [Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.FocusEventArgs.Type.get", "__arg1.type")]
    internal static string? Type(this FocusEventArgs instance) => instance.Type;
}
