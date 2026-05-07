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

internal sealed partial class RazorVueExpressionEmitter
{
    internal readonly record struct LifecyclePayloadEmission(string Expression, bool UsesFirstRender);
    // Structural omission must stay distinct from an explicit JS "null" value,
    // otherwise minimal-arity lowering would drop user-authored null expressions.
    private readonly record struct OptionalJsArgument(string Expression, bool HasValue)
    {
        public static OptionalJsArgument Missing => new(string.Empty, false);
    }

    internal const string LifecycleFirstRenderPlaceholder = "__jazorVueLifecycleFirstRender__";

    private readonly RazorVueSemanticSnapshot _snapshot;
    private readonly Dictionary<string, VuePropDescriptor> _propsByPublicName;
    private readonly Dictionary<string, VueSlotDescriptor> _slotsByPublicName;
    private readonly Dictionary<string, VueEmitDescriptor> _emitsByRazorAlias;
    private readonly ImmutableDictionary<string, VueComponentDescriptor> _resolvedComponents;
    private readonly ImmutableDictionary<string, ImmutableDictionary<string, VuePropDescriptor>> _componentPropsByPublicName;
    private readonly ImmutableDictionary<string, ImmutableDictionary<string, VueSlotDescriptor>> _componentSlotsByPublicName;
    private readonly ImmutableDictionary<string, ImmutableDictionary<string, VueEmitDescriptor>> _componentEmitDescriptorsByRazorAlias;
    private readonly ImmutableDictionary<string, string> _componentReferences;
    private readonly ImmutableDictionary<string, ImmutableDictionary<string, string>> _componentEmitsByRazorAlias;

    private readonly ImmutableDictionary<string, VueLogicFieldDescriptor> _logicFieldsByName;
    private readonly ImmutableDictionary<string, ImmutableArray<VueLogicMethodDescriptor>> _logicMethodsByName;
    private readonly HashSet<IFieldSymbol> _requiredSetupFields;
    private readonly HashSet<IMethodSymbol> _requiredSetupMethods;

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
            static slot => slot.PublicName,
            static slot => slot,
            StringComparer.Ordinal);
        _emitsByRazorAlias = snapshot.Descriptor.Emits
            .Where(static emit => !string.IsNullOrWhiteSpace(emit.RazorAlias))
            .ToDictionary(
                static emit => emit.RazorAlias!,
                static emit => emit,
                StringComparer.Ordinal);
        _resolvedComponents = resolvedComponents ?? ImmutableDictionary<string, VueComponentDescriptor>.Empty;
        _componentReferences = componentReferences ?? ImmutableDictionary<string, string>.Empty;
        _componentPropsByPublicName = BuildComponentPropsByPublicName(_resolvedComponents);
        _componentSlotsByPublicName = BuildComponentSlotsByPublicName(_resolvedComponents);
        _componentEmitDescriptorsByRazorAlias = BuildComponentEmitDescriptorsByRazorAlias(_resolvedComponents);
        _componentEmitsByRazorAlias = componentEmitsByRazorAlias ?? ImmutableDictionary<string, ImmutableDictionary<string, string>>.Empty;
        _logicFieldsByName = snapshot.Logic.Fields.ToImmutableDictionary(
            static field => field.Name,
            static field => field,
            StringComparer.Ordinal);
        _logicMethodsByName = snapshot.Logic.Methods
            .GroupBy(static method => method.Name, StringComparer.Ordinal)
            .ToImmutableDictionary(
                static group => group.Key,
                static group => group.ToImmutableArray(),
                StringComparer.Ordinal);
        _requiredSetupFields = new HashSet<IFieldSymbol>(SymbolEqualityComparer.Default);
        _requiredSetupMethods = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
    }

    internal static LifecyclePayloadEmission EmitLifecyclePayload(IMethodSymbol method, IOperation operation, bool allowFirstRenderPayload)
    {
        if (method is null)
            throw new ArgumentNullException(nameof(method));
        if (operation is null)
            throw new ArgumentNullException(nameof(operation));

        return EmitLifecyclePayloadCore(method, operation, allowFirstRenderPayload);
    }

    public string EmitFragment(RazorVueRenderFragment fragment)
    {
        var emission = EmitFragmentArgument(fragment);
        return emission.HasValue ? emission.Expression : "null";
    }

    internal string EmitTemplateExpression(IOperation operation)
        => EmitExpression(operation);

    private OptionalJsArgument EmitFragmentArgument(RazorVueRenderFragment fragment)
    {
        if (fragment.Children.IsDefaultOrEmpty)
        {
            return _snapshot.Descriptor.Slots.Any(static slot => slot.IsDefault)
                ? new OptionalJsArgument("slots.default ? slots.default() : null", true)
                : OptionalJsArgument.Missing;
        }

        if (fragment.Children.Length == 1)
            return new OptionalJsArgument(EmitNode(fragment.Children[0]), true);

        return new OptionalJsArgument("[" + string.Join(", ", fragment.Children.Select(EmitNode)) + "]", true);
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

                foreach (var slotTemplate in component.SlotTemplates)
                {
                    foreach (var origin in slotTemplate.Origins)
                        yield return origin;
                    foreach (var childOrigin in CollectOrigins(slotTemplate.Children))
                        yield return childOrigin;
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
            case RazorVueForNode loop:
                foreach (var childOrigin in CollectOrigins(loop.Body))
                    yield return childOrigin;
                break;
        }
    }
}
