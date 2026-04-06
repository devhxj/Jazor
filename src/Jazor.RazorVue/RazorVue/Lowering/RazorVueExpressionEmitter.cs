using System.Collections.Immutable;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Descriptor;
using Jazor.RazorVue.RenderTree;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue.Lowering;

internal sealed class RazorVueExpressionEmitter
{
    private readonly RazorVueSemanticSnapshot _snapshot;
    private readonly Dictionary<string, VuePropDescriptor> _propsByPublicName;
    private readonly Dictionary<string, VueSlotDescriptor> _slotsByPublicName;
    private readonly Dictionary<string, VueEmitDescriptor> _emitsByRazorAlias;
    private readonly ImmutableDictionary<string, ImmutableDictionary<string, VuePropDescriptor>> _componentPropsByPublicName;
    private readonly ImmutableDictionary<string, ImmutableDictionary<string, VueSlotDescriptor>> _componentSlotsByPublicName;
    private readonly ImmutableDictionary<string, string> _componentReferences;
    private readonly ImmutableDictionary<string, ImmutableDictionary<string, string>> _componentEmitsByRazorAlias;

    public RazorVueExpressionEmitter(
        RazorVueSemanticSnapshot snapshot,
        ImmutableDictionary<string, string>? componentReferences = null,
        ImmutableDictionary<string, VueComponentDescriptor>? resolvedComponents = null,
        ImmutableDictionary<string, ImmutableDictionary<string, string>>? componentEmitsByRazorAlias = null)
    {
        _snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
        _propsByPublicName = snapshot.Descriptor.Props.ToDictionary(
            static prop => prop.PublicName,
            static prop => prop,
            StringComparer.Ordinal);
        _slotsByPublicName = snapshot.Descriptor.Slots.ToDictionary(
            static slot => slot.IsDefault ? "ChildContent" : ToUpperCamelCase(slot.Name),
            static slot => slot,
            StringComparer.Ordinal);
        _emitsByRazorAlias = snapshot.Descriptor.Emits
            .Where(static emit => !string.IsNullOrWhiteSpace(emit.RazorAlias))
            .ToDictionary(
                static emit => emit.RazorAlias!,
                static emit => emit,
                StringComparer.Ordinal);
        _componentReferences = componentReferences ?? ImmutableDictionary<string, string>.Empty;
        _componentPropsByPublicName = BuildComponentPropsByPublicName(resolvedComponents);
        _componentSlotsByPublicName = BuildComponentSlotsByPublicName(resolvedComponents);
        _componentEmitsByRazorAlias = componentEmitsByRazorAlias ?? ImmutableDictionary<string, ImmutableDictionary<string, string>>.Empty;
    }

    public string EmitFragment(RazorVueRenderFragment fragment)
    {
        if (fragment.Children.IsDefaultOrEmpty)
        {
            return _snapshot.Descriptor.Slots.Any(static slot => slot.IsDefault)
                ? "slots.default ? slots.default() : null"
                : "null";
        }

        if (fragment.Children.Length == 1)
            return EmitNode(fragment.Children[0]);

        return "[" + string.Join(", ", fragment.Children.Select(EmitNode)) + "]";
    }

    public string DescribeFragment(RazorVueRenderFragment fragment)
    {
        var builder = new StringBuilder();
        AppendFragmentShape(builder, fragment);
        return builder.ToString();
    }

    public IEnumerable<RazorVueSourceOrigin> CollectOrigins(RazorVueRenderFragment fragment)
    {
        foreach (var child in fragment.Children)
        {
            foreach (var origin in CollectOrigins(child))
                yield return origin;
        }
    }

    private IEnumerable<RazorVueSourceOrigin> CollectOrigins(RazorVueRenderNode node)
    {
        foreach (var origin in node.Origins)
            yield return origin;

        switch (node)
        {
            case RazorVueElementNode element:
                foreach (var attribute in element.Attributes)
                {
                    foreach (var origin in attribute.Origins)
                        yield return origin;
                }

                foreach (var childOrigin in CollectOrigins(element.Children))
                    yield return childOrigin;
                break;
            case RazorVueComponentNode component:
                foreach (var attribute in component.Attributes)
                {
                    foreach (var origin in attribute.Origins)
                        yield return origin;
                }

                foreach (var childOrigin in CollectOrigins(component.Children))
                    yield return childOrigin;
                break;
            case RazorVueConditionalNode conditional:
                foreach (var childOrigin in CollectOrigins(conditional.WhenTrue))
                    yield return childOrigin;
                foreach (var childOrigin in CollectOrigins(conditional.WhenFalse))
                    yield return childOrigin;
                break;
            case RazorVueForEachNode loop:
                foreach (var childOrigin in CollectOrigins(loop.Body))
                    yield return childOrigin;
                break;
        }
    }

    private string EmitNode(RazorVueRenderNode node)
        => node switch
        {
            RazorVueElementNode element => EmitElementNode(element),
            RazorVueComponentNode component => EmitComponentNode(component),
            RazorVueTextNode text => ToJavaScriptString(text.Text),
            RazorVueExpressionNode expression => EmitExpression(expression.Expression),
            RazorVueSlotOutletNode slot => EmitSlotOutlet(slot),
            RazorVueConditionalNode conditional => "(" + EmitExpression(conditional.Condition) + " ? " +
                                                  EmitFragment(conditional.WhenTrue) + " : " +
                                                  EmitFragment(conditional.WhenFalse) + ")",
            RazorVueForEachNode loop => EmitLoop(loop),
            _ => throw new NotSupportedException($"Unsupported RazorVue render node: {node.GetType().Name}.")
        };

    private string EmitElementNode(RazorVueElementNode element)
        => "h(" + ToJavaScriptString(element.TagName) + ", " +
           EmitAttributes(element.Attributes) + ", " +
           EmitFragment(element.Children) + ")";

    private string EmitComponentNode(RazorVueComponentNode component)
    {
        var slotEntries = new List<string>();
        if (!component.Children.Children.IsDefaultOrEmpty)
            slotEntries.Add("default: () => " + EmitFragment(component.Children));

        var attributes = EmitAttributes(component.Attributes, component, slotEntries);
        var slots = slotEntries.Count == 0
            ? "null"
            : "{ " + string.Join(", ", slotEntries) + " }";

        return "h(" + ResolveComponentReference(component) + ", " + attributes + ", " + slots + ")";
    }

    private string EmitSlotOutlet(RazorVueSlotOutletNode slot)
    {
        if (slot.Argument is null)
            return "slots." + slot.SlotName + " ? slots." + slot.SlotName + "() : null";

        return "slots." + slot.SlotName + " ? slots." + slot.SlotName + "(" + EmitExpression(slot.Argument) + ") : null";
    }

    private string EmitLoop(RazorVueForEachNode loop)
        => EmitExpression(loop.Source) + ".map((" + loop.ItemName + ") => " + EmitFragment(loop.Body) + ")";

    private string EmitAttributes(ImmutableArray<RazorVueAttributeNode> attributes)
    {
        if (attributes.IsDefaultOrEmpty)
            return "null";

        var entries = attributes.Select(attribute =>
            ToJavaScriptString(attribute.Name) + ": " + (attribute.Value is null ? "true" : EmitExpression(attribute.Value!)));
        return "{ " + string.Join(", ", entries) + " }";
    }

    private string EmitAttributes(
        ImmutableArray<RazorVueAttributeNode> attributes,
        RazorVueComponentNode component,
        List<string> slotEntries)
    {
        if (attributes.IsDefaultOrEmpty)
            return "null";

        _componentEmitsByRazorAlias.TryGetValue(component.ComponentName, out var emitsByAlias);
        _componentPropsByPublicName.TryGetValue(component.ComponentName, out var propsByPublicName);
        _componentSlotsByPublicName.TryGetValue(component.ComponentName, out var slotsByPublicName);

        var entries = new List<string>();
        foreach (var attribute in attributes)
        {
            if (slotsByPublicName is not null &&
                slotsByPublicName.TryGetValue(attribute.Name, out var slotDescriptor) &&
                attribute.Value is not null)
            {
                var slotName = slotDescriptor.Name;
                var slotExpression = EmitExpression(attribute.Value!);
                if (slotDescriptor.Parameters.IsDefaultOrEmpty || !IsCallableSlotExpression(attribute.Value!))
                    slotEntries.Add(slotName + ": () => " + slotExpression);
                else
                    slotEntries.Add(slotName + ": (context) => " + slotExpression + "(context)");

                continue;
            }

            var name = attribute.Name;
            if (emitsByAlias is not null && emitsByAlias.TryGetValue(name, out var vueEventName))
                name = vueEventName;
            else if (propsByPublicName is not null && propsByPublicName.TryGetValue(name, out var propDescriptor))
                name = propDescriptor.Name;

            entries.Add(ToJavaScriptString(name) + ": " + (attribute.Value is null ? "true" : EmitExpression(attribute.Value!)));
        }

        return entries.Count == 0
            ? "null"
            : "{ " + string.Join(", ", entries) + " }";
    }

    private string EmitExpression(IOperation operation)
    {
        var current = Unwrap(operation);
        if (current is null)
            return "undefined";

        return current switch
        {
            ILiteralOperation literal => EmitLiteral(literal),
            ILocalReferenceOperation local => local.Local.Name,
            IParameterReferenceOperation parameter => parameter.Parameter.Name,
            IPropertyReferenceOperation property => EmitPropertyReference(property),
            IFieldReferenceOperation field => EmitFieldReference(field),
            IBinaryOperation binary => "(" + EmitExpression(binary.LeftOperand) + " " +
                                       GetBinaryOperator(binary.OperatorKind) + " " +
                                       EmitExpression(binary.RightOperand) + ")",
            IUnaryOperation unary => GetUnaryOperator(unary.OperatorKind) + EmitExpression(unary.Operand),
            IInvocationOperation invocation => EmitInvocation(invocation),
            IInterpolatedStringOperation interpolated => EmitInterpolatedString(interpolated),
            IConditionalOperation conditional when conditional.WhenTrue is not null && conditional.WhenFalse is not null =>
                "(" + EmitExpression(conditional.Condition) + " ? " +
                EmitExpression(conditional.WhenTrue) + " : " +
                EmitExpression(conditional.WhenFalse) + ")",
            IObjectCreationOperation creation when creation.Arguments.Length == 0 => "{}",
            IDefaultValueOperation => "null",
            _ => throw new NotSupportedException(
                $"RazorVue render currently does not support expression '{current.Kind}' in component '{_snapshot.Descriptor.FullName}'.")
        };
    }

    private string EmitPropertyReference(IPropertyReferenceOperation property)
    {
        if (IsCurrentComponentMember(property.Property, property.Instance))
        {
            if (_propsByPublicName.TryGetValue(property.Property.Name, out var prop))
                return "props." + prop.Name;

            if (_slotsByPublicName.TryGetValue(property.Property.Name, out var slot))
            {
                if (slot.IsDefault)
                    return "slots.default ? slots.default() : null";

                return "props." + ToLowerCamelCase(property.Property.Name);
            }

            if (_emitsByRazorAlias.TryGetValue(property.Property.Name, out _))
                return "props." + ToLowerCamelCase(property.Property.Name);

            throw new NotSupportedException(
                $"RazorVue render currently only supports parameter properties in template expressions. Unsupported member: '{property.Property.Name}'.");
        }

        return EmitMemberTarget(property.Instance) + "." + property.Property.Name;
    }

    private string EmitFieldReference(IFieldReferenceOperation field)
    {
        if (IsCurrentComponentMember(field.Field, field.Instance))
        {
            throw new NotSupportedException(
                $"RazorVue render currently does not support component field '{field.Field.Name}' in template expressions.");
        }

        return EmitMemberTarget(field.Instance) + "." + field.Field.Name;
    }

    private string EmitInvocation(IInvocationOperation invocation)
    {
        if (invocation.Instance is not null && invocation.TargetMethod.Name == "Invoke")
        {
            return EmitExpression(invocation.Instance) + "(" +
                   string.Join(", ", invocation.Arguments.Select(argument => EmitExpression(argument.Value))) + ")";
        }

        if (IsCurrentComponentMember(invocation.TargetMethod, invocation.Instance))
        {
            throw new NotSupportedException(
                $"RazorVue render currently does not support calling component method '{invocation.TargetMethod.Name}' from template expressions.");
        }

        var target = invocation.Instance is not null
            ? EmitExpression(invocation.Instance) + "." + invocation.TargetMethod.Name
            : invocation.TargetMethod.Name;

        return target + "(" + string.Join(", ", invocation.Arguments.Select(argument => EmitExpression(argument.Value))) + ")";
    }

    private string EmitInterpolatedString(IInterpolatedStringOperation interpolated)
    {
        var builder = new StringBuilder();
        builder.Append('`');
        foreach (var part in interpolated.Parts)
        {
            switch (part)
            {
                case IInterpolatedStringTextOperation text:
                    builder.Append(EscapeTemplateText(text.Text.ConstantValue.HasValue && text.Text.ConstantValue.Value is string value ? value : string.Empty));
                    break;
                case IInterpolationOperation interpolation:
                    builder.Append("${").Append(EmitExpression(interpolation.Expression)).Append('}');
                    break;
            }
        }

        builder.Append('`');
        return builder.ToString();
    }

    private void AppendFragmentShape(StringBuilder builder, RazorVueRenderFragment fragment)
    {
        builder.Append('[');
        for (var i = 0; i < fragment.Children.Length; i++)
        {
            if (i > 0)
                builder.Append(',');

            AppendNodeShape(builder, fragment.Children[i]);
        }

        builder.Append(']');
    }

    private void AppendNodeShape(StringBuilder builder, RazorVueRenderNode node)
    {
        switch (node)
        {
            case RazorVueElementNode element:
                builder.Append("element(").Append(element.TagName).Append(')');
                AppendAttributesShape(builder, element.Attributes);
                AppendFragmentShape(builder, element.Children);
                break;
            case RazorVueComponentNode component:
                builder.Append("component(").Append(component.ComponentName).Append(')');
                AppendAttributesShape(builder, component.Attributes);
                AppendFragmentShape(builder, component.Children);
                break;
            case RazorVueTextNode text:
                builder.Append("text(").Append(text.Text).Append(')');
                break;
            case RazorVueExpressionNode expression:
                builder.Append("expr(").Append(expression.Expression.Syntax.ToString()).Append(')');
                break;
            case RazorVueSlotOutletNode slot:
                builder.Append("slot(").Append(slot.SlotName).Append(')');
                break;
            case RazorVueConditionalNode conditional:
                builder.Append("if(").Append(conditional.Condition.Syntax.ToString()).Append(')');
                AppendFragmentShape(builder, conditional.WhenTrue);
                AppendFragmentShape(builder, conditional.WhenFalse);
                break;
            case RazorVueForEachNode loop:
                builder.Append("foreach(").Append(loop.ItemName).Append(':').Append(loop.Source.Syntax.ToString()).Append(')');
                AppendFragmentShape(builder, loop.Body);
                break;
        }
    }

    private static void AppendAttributesShape(StringBuilder builder, ImmutableArray<RazorVueAttributeNode> attributes)
    {
        builder.Append('{');
        for (var i = 0; i < attributes.Length; i++)
        {
            if (i > 0)
                builder.Append(',');

            var attribute = attributes[i];
            builder.Append(attribute.Name);
            if (attribute.Value is not null)
                builder.Append('=').Append(attribute.Value.Syntax.ToString());
        }

        builder.Append('}');
    }

    private static ImmutableDictionary<string, ImmutableDictionary<string, VuePropDescriptor>> BuildComponentPropsByPublicName(
        ImmutableDictionary<string, VueComponentDescriptor>? resolvedComponents)
    {
        if (resolvedComponents is null || resolvedComponents.IsEmpty)
            return ImmutableDictionary<string, ImmutableDictionary<string, VuePropDescriptor>>.Empty;

        var builder = ImmutableDictionary.CreateBuilder<string, ImmutableDictionary<string, VuePropDescriptor>>(StringComparer.Ordinal);
        foreach (var item in resolvedComponents)
        {
            var propsBuilder = ImmutableDictionary.CreateBuilder<string, VuePropDescriptor>(StringComparer.OrdinalIgnoreCase);
            foreach (var prop in item.Value.Props)
            {
                if (!string.IsNullOrEmpty(prop.PublicName))
                    propsBuilder[prop.PublicName] = prop;
                if (!string.IsNullOrEmpty(prop.Name))
                    propsBuilder[prop.Name] = prop;
            }

            builder[item.Key] = propsBuilder.ToImmutable();
        }

        return builder.ToImmutable();
    }

    private static ImmutableDictionary<string, ImmutableDictionary<string, VueSlotDescriptor>> BuildComponentSlotsByPublicName(
        ImmutableDictionary<string, VueComponentDescriptor>? resolvedComponents)
    {
        if (resolvedComponents is null || resolvedComponents.IsEmpty)
            return ImmutableDictionary<string, ImmutableDictionary<string, VueSlotDescriptor>>.Empty;

        var builder = ImmutableDictionary.CreateBuilder<string, ImmutableDictionary<string, VueSlotDescriptor>>(StringComparer.Ordinal);
        foreach (var item in resolvedComponents)
        {
            var slots = item.Value.Slots.ToImmutableDictionary(
                slot => slot.IsDefault ? "ChildContent" : ToUpperCamelCase(slot.Name),
                static slot => slot,
                StringComparer.Ordinal);
            builder[item.Key] = slots;
        }

        return builder.ToImmutable();
    }

    private static string ToUpperCamelCase(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        if (value.Length == 1)
            return char.ToUpperInvariant(value[0]).ToString();

        return char.ToUpperInvariant(value[0]) + value.Substring(1);
    }

    private static string ToLowerCamelCase(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        if (value.Length == 1)
            return char.ToLowerInvariant(value[0]).ToString();

        return char.ToLowerInvariant(value[0]) + value.Substring(1);
    }

    private bool IsCurrentComponentMember(ISymbol symbol, IOperation? instance)
    {
        for (var current = _snapshot.ComponentSymbol; current is not null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(symbol.ContainingType, current))
                return instance is null || Unwrap(instance) is IInstanceReferenceOperation;
        }

        return false;
    }

    private string EmitMemberTarget(IOperation? instance)
    {
        var current = Unwrap(instance);
        if (current is null)
            throw new NotSupportedException("RazorVue render member access is missing an instance target.");

        return EmitExpression(current);
    }

    private string ResolveComponentReference(RazorVueComponentNode component)
    {
        if (_componentReferences.TryGetValue(component.ComponentName, out var reference))
            return reference;

        throw new NotSupportedException(
            $"RazorVue render could not resolve component node '{component.ComponentName}' in component '{_snapshot.Descriptor.FullName}'.");
    }

    private static IOperation? Unwrap(IOperation? operation)
    {
        var current = operation;
        while (current is IConversionOperation conversion && conversion.IsImplicit)
            current = conversion.Operand;

        return current;
    }

    private static bool IsCallableSlotExpression(IOperation operation)
        => Unwrap(operation)?.Type?.TypeKind == TypeKind.Delegate;

    private static string EmitLiteral(ILiteralOperation literal)
    {
        if (!literal.ConstantValue.HasValue || literal.ConstantValue.Value is null)
            return "null";

        return literal.ConstantValue.Value switch
        {
            string text => ToJavaScriptString(text),
            char ch => ToJavaScriptString(ch.ToString()),
            bool value => value ? "true" : "false",
            float value => value.ToString("R", CultureInfo.InvariantCulture),
            double value => value.ToString("R", CultureInfo.InvariantCulture),
            decimal value => value.ToString(CultureInfo.InvariantCulture),
            sbyte value => value.ToString(CultureInfo.InvariantCulture),
            byte value => value.ToString(CultureInfo.InvariantCulture),
            short value => value.ToString(CultureInfo.InvariantCulture),
            ushort value => value.ToString(CultureInfo.InvariantCulture),
            int value => value.ToString(CultureInfo.InvariantCulture),
            uint value => value.ToString(CultureInfo.InvariantCulture),
            long value => value.ToString(CultureInfo.InvariantCulture),
            ulong value => value.ToString(CultureInfo.InvariantCulture),
            _ => Convert.ToString(literal.ConstantValue.Value, CultureInfo.InvariantCulture) ?? "null"
        };
    }

    private static string GetBinaryOperator(BinaryOperatorKind kind)
        => kind switch
        {
            BinaryOperatorKind.Add => "+",
            BinaryOperatorKind.Subtract => "-",
            BinaryOperatorKind.Multiply => "*",
            BinaryOperatorKind.Divide => "/",
            BinaryOperatorKind.Remainder => "%",
            BinaryOperatorKind.Equals => "===",
            BinaryOperatorKind.NotEquals => "!==",
            BinaryOperatorKind.LessThan => "<",
            BinaryOperatorKind.LessThanOrEqual => "<=",
            BinaryOperatorKind.GreaterThan => ">",
            BinaryOperatorKind.GreaterThanOrEqual => ">=",
            BinaryOperatorKind.ConditionalAnd => "&&",
            BinaryOperatorKind.ConditionalOr => "||",
            BinaryOperatorKind.And => "&",
            BinaryOperatorKind.Or => "|",
            BinaryOperatorKind.ExclusiveOr => "^",
            _ => throw new NotSupportedException($"Unsupported RazorVue binary operator: {kind}.")
        };

    private static string GetUnaryOperator(UnaryOperatorKind kind)
        => kind switch
        {
            UnaryOperatorKind.Not => "!",
            UnaryOperatorKind.Minus => "-",
            UnaryOperatorKind.Plus => "+",
            _ => throw new NotSupportedException($"Unsupported RazorVue unary operator: {kind}.")
        };

    private static string ToJavaScriptString(string value)
        => "\"" + (value ?? string.Empty)
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\r", "\\r")
            .Replace("\n", "\\n") + "\"";

    private static string EscapeTemplateText(string value)
        => (value ?? string.Empty)
            .Replace("\\", "\\\\")
            .Replace("`", "\\`")
            .Replace("${", "\\${");
}

