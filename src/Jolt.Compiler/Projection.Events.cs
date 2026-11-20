using Jolt.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Jolt.Compiler;

internal sealed partial class Projection
{
    private void EmitDomEvent(JoltAttribute attribute, string name, JoltElement element)
        => EmitDomEventProperty(name, attribute, attribute, DomEventModifiers(element, name));

    private void EmitStandaloneEventModifier(JoltAttribute attribute, JoltElement element, HashSet<string> seen)
    {
        string name = ComponentContract.EventProperty(attribute.Target!);
        if (element.Attributes.Any(candidate => candidate.Kind == JoltAttributeKind.Event &&
            ComponentContract.EventProperty(candidate.Target!) == name))
            return;

        var modifiers = DomEventModifiers(element, name);
        if (modifiers[0] != attribute)
            return;
        if (!seen.Add(name))
            Report("JOLT1003", attribute.NameSpan, $"Event '{attribute.Target}' is specified more than once.");
        EmitDomEventProperty(name, attribute, null, modifiers);
    }

    private static JoltAttribute[] DomEventModifiers(JoltElement element, string name) => element.Attributes
        .Where(attribute => attribute.Kind == JoltAttributeKind.EventModifier &&
            ComponentContract.EventProperty(attribute.Target!) == name)
        .ToArray();

    private void EmitDomEventProperty(string name, JoltAttribute attribute, JoltAttribute? handler, JoltAttribute[] modifiers)
    {
        string target = attribute.Target!;
        if (target.Contains(':'))
        {
            Report("JOLT1024", attribute.NameSpan, "Use preventDefault or stopPropagation after a DOM event name.");
            return;
        }

        TextSpan span = handler?.Value.FirstOrDefault()?.Span ?? attribute.NameSpan;
        var handlerSpan = handler?.Value switch
        {
            [JoltExpression expression] => expression.CodeSpan,
            [JoltText text] => text.Span,
            _ => (TextSpan?)null,
        };
        if (handler is not null && (handlerSpan is not { Length: > 0 } codeSpan ||
            string.IsNullOrWhiteSpace(Source.ToString(codeSpan))))
        {
            Report("JOLT1024", handler.NameSpan, $"Event '{handler.Target}' requires a C# handler.");
            handlerSpan = null;
        }

        string type = "global::ECMAScript." + DomEventType(target);
        const string action = "global::System.Action";
        const string func = "global::System.Func";
        const string task = "global::System.Threading.Tasks.Task";
        string modifierParameter = modifiers.Length == 0 ? "" : $", {action}<global::ECMAScript.EventRef> modifiers";
        const string result = "object";
        string intrinsicName = IntrinsicPrefix + Intrinsics.Count;

        // Overload resolution gives event lambdas and method groups their DOM type and
        // distinguishes synchronous handlers from handlers whose Task Vue must observe.
        int generatedStart = _code.Length;
        Call(new(IntrinsicKind.DomEvent, handlerSpan ?? span, name), result, $"{action} handler{modifierParameter}");
        foreach (string signature in new[] { $"{action}<{type}>", $"{func}<{task}>", $"{func}<{type}, {task}>" })
            _methods.Add($"private static {result} {intrinsicName}({signature} handler{modifierParameter}) => throw new global::System.NotImplementedException();\n");
        if (handlerSpan is { } code)
            Copy(code);
        else
            Write("() => { }");
        if (modifiers.Length > 0)
            EmitDomEventModifiers(modifiers);
        _anchors.Add((TextSpan.FromBounds(generatedStart, _code.Length), handlerSpan ?? span));
        Write("), ");
    }

    private void EmitDomEventModifiers(JoltAttribute[] modifiers)
    {
        string eventLocal = "__joltDomEvent" + _nextLocal++;
        Write($", {eventLocal} => {{ ");
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var modifier in modifiers)
        {
            if (!seen.Add(modifier.Modifier!))
            {
                Report("JOLT1024", modifier.NameSpan, $"Event modifier '{modifier.Modifier}' is specified more than once.");
                continue;
            }
            string method = modifier.Modifier switch
            {
                "preventDefault" => nameof(ECMAScript.EventRef.PreventDefault),
                "stopPropagation" => nameof(ECMAScript.EventRef.StopPropagation),
                _ => throw new InvalidOperationException($"Unknown event modifier '{modifier.Modifier}'."),
            };
            if (modifier.Value is [JoltExpression condition])
            {
                // The bool expression stays inside the listener so reactive reads happen
                // for every dispatch and precede the author handler.
                Write("if (");
                Copy(condition.CodeSpan);
                Write(") ");
            }
            else if (modifier.Value.Length > 0)
            {
                Report("JOLT1024", modifier.NameSpan, $"Event modifier '{modifier.Modifier}' requires a C# boolean expression.");
                continue;
            }
            Write($"{eventLocal}.{method}(); ");
        }
        Write("}");
    }

    private static string DomEventType(string name) => name.ToLowerInvariant() switch
    {
        "keydown" or "keypress" or "keyup" => nameof(ECMAScript.KeyboardEvent),
        "click" or "auxclick" or "dblclick" or "contextmenu" or
        "mousedown" or "mouseup" or "mouseenter" or "mouseleave" or "mousemove" or
        "mouseout" or "mouseover" => nameof(ECMAScript.MouseEvent),
        "submit" => nameof(ECMAScript.SubmitEvent),
        "focus" or "blur" or "focusin" or "focusout" => nameof(ECMAScript.FocusEvent),
        "beforeinput" => nameof(ECMAScript.InputEvent),
        "compositionstart" or "compositionupdate" or "compositionend" => nameof(ECMAScript.CompositionEvent),
        "wheel" => nameof(ECMAScript.WheelEvent),
        _ => nameof(ECMAScript.EventRef),
    };
}
