using Jolt.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Jolt.Compiler;

internal enum NativeModel { TextChange, TextInput, Checkbox, Radio, Select, SelectMultiple, NumberChange, NumberInput }

internal sealed record ModelBinding(string Property, string Update, string Type, NativeModel? Native = null, LocalModel? Local = null);

internal sealed partial class Projection
{
    private readonly List<(TextSpan Generated, TextSpan Source, ModelBinding Binding)> _bindingChecks = [];
    private bool _insideBoundSelect;

    private void EmitBinding(JoltAttribute attribute, ComponentContract? component, JoltElement element, HashSet<string> seen)
    {
        if (attribute.Kind == JoltAttributeKind.BindEvent)
        {
            if (!seen.Add(attribute.Name))
                Report("JOLT1003", attribute.NameSpan, "The @bind:event modifier is specified more than once.");
            if (!element.Attributes.Any(candidate => candidate.Kind == JoltAttributeKind.Bind && candidate.Target is null &&
                candidate.Modifier is null or "get"))
                Report("JOLT1023", attribute.NameSpan, "The @bind:event modifier requires a text or number control binding.");
            return;
        }

        var group = element.Attributes.Where(candidate => candidate.Kind == JoltAttributeKind.Bind &&
            string.Equals(candidate.Target, attribute.Target, StringComparison.OrdinalIgnoreCase)).ToArray();
        var ordinary = group.FirstOrDefault(candidate => candidate.Modifier is null);
        var getter = group.FirstOrDefault(candidate => candidate.Modifier == "get");
        var setter = group.FirstOrDefault(candidate => candidate.Modifier == "set");
        var after = group.FirstOrDefault(candidate => candidate.Modifier == "after");
        if (group[0] == attribute)
        {
            foreach (var duplicates in group.GroupBy(candidate => candidate.Modifier))
            {
                foreach (var duplicate in duplicates.Skip(1))
                    Report("JOLT1003", duplicate.NameSpan, $"The '{duplicate.Name}' directive is specified more than once.");
            }
        }
        if (ordinary is not null && (getter is not null || setter is not null))
        {
            if (group[0] == attribute)
                Report("JOLT1023", (getter ?? setter)!.NameSpan, "Use a writable @bind target or a paired :get and :set.");
            return;
        }
        if ((getter is null) != (setter is null) || ordinary is null && getter is null)
        {
            if (group[0] == attribute)
                Report("JOLT1023", attribute.NameSpan, "The binding requires a writable target or both :get and :set.");
            return;
        }
        if (getter is not null && after is not null)
        {
            if (group[0] == attribute)
                Report("JOLT1023", after.NameSpan, "Use :after with a writable @bind target. A :set callback handles follow-up work for a split binding.");
            return;
        }

        var reader = (ordinary ?? getter)!;
        if (attribute != reader)
            return;
        var binding = attribute.Target is { } target
            ? component?.FindModel(target)
            : component is null ? FindNativeModel(element) : null;
        if (binding is null)
        {
            Report("JOLT1023", attribute.NameSpan, attribute.Target is null
                ? "@bind requires an input with a supported static type, a textarea, or a select with static multiplicity. Use @bind-Name for a declared component model."
                : $"'{element.Name}' has no supported model named '{attribute.Target}'. A matching prop and update callback or event are required.");
            return;
        }

        if (BindingExpression(reader, ordinary is null ? "Supply a C# expression for the binding value." :
            "Supply a writable C# expression for the binding target.") is not { } expression)
            return;
        var setterExpression = setter is null ? null : BindingExpression(setter, "Supply a C# setter callback.");
        var afterExpression = after is null ? null : BindingExpression(after, "Supply a C# callback to run after the update.");
        if (setter is not null && setterExpression is null || after is not null && afterExpression is null)
        {
            return;
        }

        var names = new List<string> { binding.Property, binding.Update };
        if (binding.Local is { } local && component is not null)
        {
            names.AddRange([local.Property, local.Callback]);
            foreach (var entry in group)
                MarkupBindings.Add(new(new(component.Path, JoltMarkupKind.Property, local.Property),
                    new TextSpan(entry.NameSpan.Start + "@bind-".Length, entry.Target!.Length)));
        }
        if (names.Distinct(StringComparer.Ordinal).Any(name => !seen.Add(name)))
        {
            Report("JOLT1003", attribute.NameSpan, $"The binding conflicts with another '{binding.Property}' or '{binding.Update}' attribute.");
            return;
        }
        if (binding.Native is not null)
        {
            foreach (var duplicate in element.Attributes.Where(candidate => candidate.Kind == JoltAttributeKind.Markup &&
                candidate.Name != binding.Property && string.Equals(candidate.Name, binding.Property, StringComparison.OrdinalIgnoreCase)))
            {
                Report("JOLT1003", duplicate.NameSpan, $"The '{binding.Property}' attribute is already supplied by @bind.");
            }
        }

        if (binding.Native is not null)
            Call(new(IntrinsicKind.Bind, expression.CodeSpan, binding.Property, Binding: binding), "object",
                $"{binding.Type} value, global::System.Action<{binding.Type}> update" +
                (setterExpression is null ? "" : $", global::System.Func<{binding.Type}> read"));
        else
            Call(new(IntrinsicKind.Prop, expression.CodeSpan, binding.Property), "object", $"{binding.Type} value");
        int start = _code.Length;
        Copy(expression.CodeSpan);
        if (ordinary is not null)
            _bindingChecks.Add((new TextSpan(start, expression.CodeSpan.Length), expression.CodeSpan, binding));

        if (binding.Native is null)
        {
            Write("), ");
            Call(new(IntrinsicKind.Prop, expression.CodeSpan, binding.Update), "object", $"global::System.Action<{binding.Type}> update");
        }
        else
            Write(", ");
        if (setterExpression is null && afterExpression is null)
            EmitBindingAssignment(expression);
        else
            EmitModelUpdate(binding, expression, setterExpression, afterExpression);
        if (binding.Native is not null && setterExpression is not null)
        {
            Write(", () => (");
            Copy(expression.CodeSpan);
            Write(")");
        }
        Write("), ");
    }

    private JoltExpression? BindingExpression(JoltAttribute attribute, string message)
    {
        if (attribute.Value is [JoltExpression { CodeSpan.Length: > 0 } expression])
            return expression;
        Report("JOLT1023", attribute.NameSpan, message);
        return null;
    }

    private void EmitBindingAssignment(JoltExpression expression)
    {
        // A normal C# assignment keeps receivers and indexers inside the event callback
        // and lets Roslyn diagnose nonassignable targets in the authored expression.
        string value;
        do
            value = "__joltModelValue" + _nextLocal++;
        while (Document.Source.Contains(value, StringComparison.Ordinal));
        int setterStart = _code.Length;
        Write($"{value} => {{ (");
        Copy(expression.CodeSpan);
        Write($") = {value}; }}");
        _anchors.Add((TextSpan.FromBounds(setterStart, _code.Length), expression.CodeSpan));
    }

    private void EmitModelUpdate(ModelBinding binding, JoltExpression getter, JoltExpression? setter, JoltExpression? after)
    {
        const string action = "global::System.Action";
        const string asyncAction = "global::System.Func<global::System.Threading.Tasks.Task>";
        string update = $"{action}<{binding.Type}>";
        string callback = setter is null ? $"{action} after" : $"{update} setter";
        string asyncCallback = setter is null ? $"{asyncAction} after" :
            $"global::System.Func<{binding.Type}, global::System.Threading.Tasks.Task> setter";
        string prefix = after is null ? "" : $"{update} assign, ";
        TextSpan span = (setter ?? after)!.CodeSpan;
        string method = IntrinsicPrefix + Intrinsics.Count;
        int start = _code.Length;
        Call(new(IntrinsicKind.ModelUpdate, span), update, prefix + callback);
        _methods.Add($"private static {update} {method}({prefix}{asyncCallback}) => throw new global::System.NotImplementedException();\n");
        if (after is not null)
        {
            EmitBindingAssignment(getter);
            Write(", ");
        }
        Copy(span);
        _anchors.Add((TextSpan.FromBounds(start, _code.Length), span));
        Write(")");
    }

    private ModelBinding? FindNativeModel(JoltElement element)
    {
        var mode = JoltFormBindings.Get(element) switch
        {
            JoltFormBinding.Text => NativeModel.TextChange,
            JoltFormBinding.Checkbox => NativeModel.Checkbox,
            JoltFormBinding.Radio => NativeModel.Radio,
            JoltFormBinding.Select => NativeModel.Select,
            JoltFormBinding.SelectMultiple => NativeModel.SelectMultiple,
            JoltFormBinding.Number => NativeModel.NumberChange,
            _ => (NativeModel?)null,
        };
        if (mode is null)
            return null;
        if (mode == NativeModel.Checkbox)
        {
            foreach (var attribute in element.Attributes.Where(attribute =>
                attribute.Name.Equals("true-value", StringComparison.OrdinalIgnoreCase) ||
                attribute.Name.Equals("false-value", StringComparison.OrdinalIgnoreCase)))
                Report("JOLT1023", attribute.NameSpan, "Checkbox @bind requires boolean values.");
        }
        var modifier = element.Attributes.FirstOrDefault(attribute => attribute.Kind == JoltAttributeKind.BindEvent);
        if (modifier is not null)
        {
            if (mode is not (NativeModel.TextChange or NativeModel.NumberChange))
                Report("JOLT1023", modifier.NameSpan, "Only text and number controls support @bind:event.");
            else if (modifier.Value is [JoltText { Text: "oninput" }])
                mode = mode == NativeModel.NumberChange ? NativeModel.NumberInput : NativeModel.TextInput;
            else if (modifier.Value is not [JoltText { Text: "onchange" }])
                Report("JOLT1023", modifier.Value.FirstOrDefault()?.Span ?? modifier.NameSpan,
                    "The @bind:event modifier expects 'oninput' or 'onchange'.");
        }
        return mode switch
        {
            NativeModel.Checkbox => new("checked", "onUpdate:modelValue", "bool", mode),
            NativeModel.Radio => new("modelValue", "onUpdate:modelValue", "string", mode),
            NativeModel.SelectMultiple => new("value", "onUpdate:modelValue", "string[]", mode),
            NativeModel.NumberChange or NativeModel.NumberInput => new("value", "onUpdate:modelValue", "double?", mode),
            _ => new("value", "onUpdate:modelValue", "string", mode),
        };
    }

    internal void CheckBindings(SemanticModel model, List<Diagnostic> validationDiagnostics)
    {
        var root = model.SyntaxTree.GetRoot();
        foreach (var (generated, source, binding) in _bindingChecks)
        {
            if (root.FindNode(generated, getInnermostNodeForTie: true) is not ExpressionSyntax syntax)
                continue;
            var info = model.GetTypeInfo(syntax);
            if (info.Type is { TypeKind: not TypeKind.Error } type && info.ConvertedType is { } expected &&
                !SymbolEqualityComparer.Default.Equals(type, expected))
            {
                ReportValidation(validationDiagnostics, "JOLT1023", source, $"The '{binding.Property}' binding target must have type '{binding.Type}'.");
            }
            while (syntax is ParenthesizedExpressionSyntax parenthesized)
                syntax = parenthesized.Expression;
            if (model.GetSymbolInfo(syntax).Symbol is IPropertySymbol property &&
                (property.ContainingType.Name.StartsWith("__JoltScope", StringComparison.Ordinal) ||
                property.GetAttributes().Any(attribute => attribute.AttributeClass?.Name == "ParameterAttribute")))
            {
                ReportValidation(validationDiagnostics, "JOLT1023", source, $"The '{binding.Property}' binding target is a read-only component input.");
            }
        }
    }
}
