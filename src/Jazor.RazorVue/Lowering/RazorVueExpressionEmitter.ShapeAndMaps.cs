using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Jazor.RazorVue.Descriptor;
using Jazor.RazorVue.RenderTree;
using Microsoft.CodeAnalysis;

namespace Jazor.RazorVue.Lowering;

internal sealed partial class RazorVueExpressionEmitter
{
    internal ImmutableArray<VueLogicFieldDescriptor> GetRequiredSetupFields()
        => _requiredSetupFields
            .SelectMany(field => _logicFieldsByName.TryGetValue(field.Name, out var candidate) &&
                                 RazorVueSymbolIdentity.SameMember(candidate.FieldSymbol, field)
                ? [candidate]
                : ImmutableArray<VueLogicFieldDescriptor>.Empty)
            .Distinct()
            .ToImmutableArray();

    internal ImmutableArray<VueLogicMethodDescriptor> GetRequiredSetupMethods()
        => _requiredSetupMethods
            .SelectMany(method => _logicMethodsByName.TryGetValue(method.Name, out var candidates)
                ? candidates.Where(candidate => RazorVueSymbolIdentity.SameMember(candidate.MethodSymbol, method))
                : ImmutableArray<VueLogicMethodDescriptor>.Empty)
            .Distinct()
            .ToImmutableArray();

    private void AppendFragmentShape(StringBuilder builder, RazorVueRenderFragment fragment)
    {
        builder.Append('[');
        if (!fragment.Children.IsDefaultOrEmpty)
        {
            for (var i = 0; i < fragment.Children.Length; i++)
            {
                if (i > 0)
                    builder.Append(',');

                AppendNodeShape(builder, fragment.Children[i]);
            }
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
                AppendSlotTemplatesShape(builder, component.SlotTemplates);
                AppendFragmentShape(builder, component.Children);
                break;
            case RazorVueTextNode text:
                builder.Append("text(").Append(text.Text).Append(')');
                break;
            case RazorVueExpressionNode expression:
                builder.Append("expr(").Append(expression.Expression.Syntax.ToString()).Append(')');
                break;
            case RazorVueUnsupportedTemplateNode unsupported:
                builder.Append("unsupported(").Append(unsupported.Message).Append(')');
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
            case RazorVueForNode loop:
                builder.Append("for(")
                    .Append(loop.VariableName)
                    .Append('=')
                    .Append(loop.InitialValue.Syntax.ToString())
                    .Append(';')
                    .Append(loop.ConditionKind)
                    .Append(':')
                    .Append(loop.LimitValue.Syntax.ToString())
                    .Append(';')
                    .Append(loop.StepKind);
                if (loop.StepValue is not null)
                    builder.Append(':').Append(loop.StepValue.Syntax.ToString());
                builder.Append(')');
                AppendFragmentShape(builder, loop.Body);
                break;
        }
    }

    private static void AppendAttributesShape(StringBuilder builder, ImmutableArray<RazorVueAttributeEntry> attributes)
    {
        builder.Append('{');
        for (var i = 0; i < attributes.Length; i++)
        {
            if (i > 0)
                builder.Append(',');

            switch (attributes[i])
            {
                case RazorVueAttributeNode attribute:
                    builder.Append(attribute.Name);
                    if (attribute.Value is not null)
                        builder.Append('=').Append(attribute.Value.Syntax.ToString());
                    break;
                case RazorVueAttributeSpreadNode spread:
                    builder.Append("...");
                    builder.Append(spread.Expression.Syntax.ToString());
                    break;
            }
        }

        builder.Append('}');
    }

    private void AppendSlotTemplatesShape(StringBuilder builder, ImmutableArray<RazorVueComponentSlotTemplateNode> slotTemplates)
    {
        builder.Append('<');
        for (var i = 0; i < slotTemplates.Length; i++)
        {
            if (i > 0)
                builder.Append(',');

            var slotTemplate = slotTemplates[i];
            builder.Append(slotTemplate.PublicName);
            if (!string.IsNullOrWhiteSpace(slotTemplate.ParameterName))
                builder.Append('(').Append(slotTemplate.ParameterName).Append(')');
            AppendFragmentShape(builder, slotTemplate.Children);
        }

        builder.Append('>');
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
                static slot => slot.PublicName,
                static slot => slot,
                StringComparer.Ordinal);
            builder[item.Key] = slots;
        }

        return builder.ToImmutable();
    }

    private static ImmutableDictionary<string, ImmutableDictionary<string, VueEmitDescriptor>> BuildComponentEmitDescriptorsByRazorAlias(
        ImmutableDictionary<string, VueComponentDescriptor>? resolvedComponents)
    {
        if (resolvedComponents is null || resolvedComponents.IsEmpty)
            return ImmutableDictionary<string, ImmutableDictionary<string, VueEmitDescriptor>>.Empty;

        var builder = ImmutableDictionary.CreateBuilder<string, ImmutableDictionary<string, VueEmitDescriptor>>(StringComparer.Ordinal);
        foreach (var item in resolvedComponents)
        {
            var emitsBuilder = ImmutableDictionary.CreateBuilder<string, VueEmitDescriptor>(StringComparer.Ordinal);
            foreach (var emit in item.Value.Emits)
            {
                if (!string.IsNullOrWhiteSpace(emit.RazorAlias))
                    emitsBuilder[emit.RazorAlias!] = emit;
            }

            builder[item.Key] = emitsBuilder.ToImmutable();
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

}
