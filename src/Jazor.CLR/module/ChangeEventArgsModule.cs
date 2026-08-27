using ECMAScript;
using static ECMAScript.Global;

namespace Jazor.CLR;

/// <summary>
/// Captures the browser value represented by Blazor <c>ChangeEventArgs</c> at listener time.
/// </summary>
/// <remarks>
/// A native DOM event does not expose the CLR <c>Value</c> payload. The listener therefore saves
/// the shaped value in a private weak map before invoking the user's callback. This preserves the
/// value observed by an async handler even when the DOM target changes before its continuation.
/// </remarks>
[ECMAScriptModule("Microsoft/AspNetCore/Components/ChangeEventArgsModule.js")]
[Jazor(Op.Alias, "Microsoft.AspNetCore.Components.ChangeEventArgs", "JazorEvent")]
public static class ChangeEventArgsModule
{
    private static readonly WeakMap<JazorEvent, object?> Values = new();

    /// <summary>Captures one change event and returns the same native event carrier.</summary>
    [Jazor(
        Op.Import,
        "Microsoft.AspNetCore.Components.ChangeEventArgs.captureChangeEvent",
        "captureChangeEvent")]
    public static JazorEvent CaptureChangeEvent(JazorEvent @event)
    {
        if (@event is null)
            throw new Error("ArgumentNullException: event is null.");

        var target = @event.Target;
        object? value;
        if (target is HTMLInputElement input)
        {
            if (string.Equals(input.Type, "file", StringComparison.OrdinalIgnoreCase))
                throw new Error("NotSupportedException: file input changes require InputFileChangeEventArgs.");

            value = string.Equals(input.Type, "checkbox", StringComparison.OrdinalIgnoreCase)
                ? input.Checked
                : input.Value;
        }
        else if (target is HTMLTextAreaElement textArea)
        {
            value = textArea.Value;
        }
        else if (target is HTMLSelectElement select)
        {
            if (!select.Multiple)
            {
                value = select.Value;
            }
            else
            {
                var selectedValues = new Array<string>();
                var selectedOptions = select.SelectedOptions;
                for (uint index = 0; index < selectedOptions.Length; index++)
                {
                    var option = selectedOptions.GetItem(index) as HTMLOptionElement;
                    if (option is null)
                        throw new Error("InvalidOperationException: selected option is not an HTMLOptionElement.");

                    selectedValues.Push(option.Value);
                }

                value = selectedValues;
            }
        }
        else
        {
            throw new Error("NotSupportedException: ChangeEventArgs requires an input, textarea, or select target.");
        }

        Values.Set(@event, value);
        return @event;
    }

    /// <summary>Reads the value captured for a previously wrapped change event.</summary>
    [Jazor(
        Op.Import,
        "Microsoft.AspNetCore.Components.ChangeEventArgs.Value.get",
        "getChangeEventValue")]
    public static object? GetChangeEventValue(JazorEvent @event)
    {
        if (@event is null)
            throw new Error("ArgumentNullException: event is null.");
        if (!Values.Has(@event))
            throw new Error("InvalidOperationException: ChangeEventArgs.Value was read before change capture.");

        return Values.Get(@event);
    }

    // Native DOM events are read-only carriers. Constructing or mutating a synthetic
    // ChangeEventArgs would claim a POCO contract that the browser path does not provide.
    [Jazor(Op.Discard, "Microsoft.AspNetCore.Components.ChangeEventArgs.Value.set")]
    public extern static void _b834c09ac3cad4f5(JazorEvent instance, object? value);

    [Jazor(Op.Discard, "Microsoft.AspNetCore.Components.ChangeEventArgs.ChangeEventArgs()")]
    public extern static JazorEvent _edaab150211bc8e2();
}
