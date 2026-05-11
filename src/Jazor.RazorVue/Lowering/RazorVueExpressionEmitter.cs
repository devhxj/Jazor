using System.Collections.Immutable;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Acornima;
using Acornima.Ast;
using Jazor.Compiler;
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
    private readonly SenseArgument _compilerArgument;
    private readonly SemanticWalker _semanticWalker;
    private readonly List<RazorVueCompilerImportBinding> _compilerImports;

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
        _slotsByPublicName = snapshot.Descriptor.Slots
            .GroupBy(static slot => slot.PublicName, StringComparer.Ordinal)
            .ToDictionary(
                static group => group.Key,
                static group => group.First(),
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
        _compilerArgument = new SenseArgument(Sense.Any, UseImportAliases: true);
        _semanticWalker = new SemanticWalker(snapshot.ComponentSymbol, moduleDeclaredNames: new Dictionary<ISymbol, string>(SymbolEqualityComparer.Default))
        {
            Host = new RazorVueCompilerHost(this)
        };
        _compilerImports = new List<RazorVueCompilerImportBinding>();
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

    private string EmitFragment(
        RazorVueRenderFragment fragment,
        ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
        ImmutableHashSet<IParameterSymbol> allowedParameterSymbols)
    {
        var emission = EmitFragmentArgument(fragment, allowedLocalSymbols, allowedParameterSymbols);
        return emission.HasValue ? emission.Expression : "null";
    }

    internal string EmitTemplateExpression(IOperation operation)
        => EmitExpression(operation);

    private OptionalJsArgument EmitFragmentArgument(RazorVueRenderFragment fragment)
    {
        return EmitFragmentArgument(fragment, EmptyLocalScope, EmptyParameterScope);
    }

    private OptionalJsArgument EmitFragmentArgument(
        RazorVueRenderFragment fragment,
        ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
        ImmutableHashSet<IParameterSymbol> allowedParameterSymbols)
    {
        if (fragment.Children.IsDefaultOrEmpty)
        {
            return _snapshot.Descriptor.Slots.Any(static slot => slot.IsDefault)
                ? new OptionalJsArgument("slots.default ? slots.default() : null", true)
                : OptionalJsArgument.Missing;
        }

        if (fragment.Children.Length == 1)
            return new OptionalJsArgument(EmitNode(fragment.Children[0], allowedLocalSymbols, allowedParameterSymbols), true);

        return new OptionalJsArgument(
            "[" + string.Join(", ", fragment.Children.Select(child => EmitNode(child, allowedLocalSymbols, allowedParameterSymbols))) + "]",
            true);
    }

    public string DescribeFragment(RazorVueRenderFragment fragment)
    {
        var builder = new StringBuilder();
        AppendFragmentShape(builder, fragment);
        return builder.ToString();
    }

    internal ImmutableArray<RazorVueCompilerImportBinding> FlushCompilerImports()
    {
        foreach (var pair in _compilerArgument.FlushImportSpecifiers())
        {
            foreach (var specifier in pair.Value)
            {
                switch (specifier)
                {
                    case ImportDefaultSpecifier defaultSpecifier:
                        _compilerImports.Add(new RazorVueCompilerImportBinding(
                            pair.Key,
                            RazorVueCompilerImportKind.Default,
                            defaultSpecifier.Local.Name,
                            ImportedName: null));
                        break;
                    case ImportNamespaceSpecifier namespaceSpecifier:
                        _compilerImports.Add(new RazorVueCompilerImportBinding(
                            pair.Key,
                            RazorVueCompilerImportKind.Namespace,
                            namespaceSpecifier.Local.Name,
                            ImportedName: null));
                        break;
                    case ImportSpecifier namedSpecifier:
                        var importedName = namedSpecifier.Imported.ToECMAScript();
                        var localName = namedSpecifier.Local.Name;
                        _compilerImports.Add(new RazorVueCompilerImportBinding(
                            pair.Key,
                            RazorVueCompilerImportKind.Named,
                            localName,
                            ImportedName: importedName));
                        break;
                }
            }
        }

        return _compilerImports.Distinct().ToImmutableArray();
    }

    private string EmitCompilerLoweredExpression(IOperation operation, SenseArgument? compilerArgument = null)
    {
        var argument = compilerArgument ?? _compilerArgument;

        if (TryEmitCompilerOwnedExpression(operation, argument, out var directExpression))
            return NormalizeTopLevelExpressionText(operation, directExpression);

        var node = _semanticWalker.Visit(operation, argument);
        if (node is not Expression expression)
        {
            throw new NotSupportedException(
                $"RazorVue render currently does not support expression '{operation.Kind}' in component '{_snapshot.Descriptor.FullName}'.");
        }

        return NormalizeTopLevelExpressionText(operation, MaterializeCompilerExpression(expression, argument));
    }

    private string MaterializeCompilerExpression(Expression expression, SenseArgument argument)
    {
        if (!argument.HasVarDeclarator)
            return expression.ToKnRECMAScript();

        var statements = new List<Statement>();
        var declarators = argument.FlushVarDeclarator();
        if (declarators.Count > 0)
            statements.Add(new VariableDeclaration(VariableDeclarationKind.Let, declarators));
        statements.Add(new ReturnStatement(expression));

        var functionBody = new FunctionBody(NodeList.From(statements), strict: true);
        var arrowFunction = new ArrowFunctionExpression(
            NodeList.From<Node>(),
            functionBody,
            expression: false,
            async: false);
        return new CallExpression(arrowFunction, NodeList.Empty<Expression>(), optional: false).ToKnRECMAScript();
    }

    private bool TryEmitCompilerOwnedExpression(IOperation operation, SenseArgument argument, out string expression)
    {
        expression = string.Empty;
        var current = Unwrap(operation);
        if (current is null)
            return false;

        if (current is IInvocationOperation invocation && IsCurrentComponentMember(invocation.TargetMethod, invocation.Instance))
            return false;

        if (current is IMethodReferenceOperation methodReference && IsCurrentComponentMember(methodReference.Method, methodReference.Instance))
            return false;

        if (current is IFieldReferenceOperation fieldReference && IsCurrentComponentMember(fieldReference.Field, fieldReference.Instance))
            return false;

        if (current is IPropertyReferenceOperation propertyReference &&
            IsCurrentComponentMember(propertyReference.Property, propertyReference.Instance) &&
            !_propsByPublicName.ContainsKey(propertyReference.Property.Name) &&
            !_slotsByPublicName.ContainsKey(propertyReference.Property.Name) &&
            !_emitsByRazorAlias.ContainsKey(propertyReference.Property.Name))
        {
            return false;
        }

        if (current is IInvocationOperation invocationOperation &&
            TryRewriteInvocation(invocationOperation, argument, out expression))
        {
            return true;
        }

        if (current is IPropertyReferenceOperation property &&
            TryRewritePropertyReference(property, argument, out expression))
        {
            return true;
        }

        if (current is IFieldReferenceOperation field &&
            TryRewriteFieldReference(field, argument, out expression))
        {
            return true;
        }

        if (current is IMethodReferenceOperation methodReferenceOperation &&
            TryRewriteMethodReference(methodReferenceOperation, argument, out expression))
        {
            return true;
        }

        return false;
    }

    private static string NormalizeTopLevelExpressionText(IOperation operation, string expressionText)
    {
        var current = Unwrap(operation);
        if (current is IBinaryOperation or IConditionalOperation)
            return "(" + expressionText + ")";

        return expressionText;
    }

    private static Expression ParseJavaScriptExpression(string expressionText)
    {
        var module = new Parser().ParseModule("const __j = " + expressionText + ";");
        var declaration = (VariableDeclaration)module.Body[0];
        return (Expression)declaration.Declarations[0].Init!;
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

    private sealed class RazorVueCompilerHost : SemanticWalkerHost
    {
        private readonly RazorVueExpressionEmitter _emitter;

        public RazorVueCompilerHost(RazorVueExpressionEmitter emitter)
        {
            _emitter = emitter ?? throw new ArgumentNullException(nameof(emitter));
        }

        public override Expression? RewriteInvocationPreorder(IInvocationOperation operation, SenseArgument argument)
            => _emitter.TryRewriteInvocation(operation, argument, out var expression)
                ? ParseJavaScriptExpression(expression)
                : null;

        public override Expression? RewriteFieldReference(
            IFieldReferenceOperation operation,
            SenseArgument argument,
            Expression? instance)
            => _emitter.TryRewriteFieldReference(operation, argument, out var expression)
                ? ParseJavaScriptExpression(expression)
                : null;

        public override Expression? RewritePropertyReference(
            IPropertyReferenceOperation operation,
            SenseArgument argument,
            Expression? instance,
            IReadOnlyList<Expression> arguments)
            => _emitter.TryRewritePropertyReference(operation, argument, out var expression)
                ? ParseJavaScriptExpression(expression)
                : null;

        public override Expression? RewriteMethodReference(
            IMethodReferenceOperation operation,
            SenseArgument argument,
            Expression? instance)
            => _emitter.TryRewriteMethodReference(operation, argument, out var expression)
                ? ParseJavaScriptExpression(expression)
                : null;

        public override Expression? RewriteInvocation(
            IInvocationOperation operation,
            SenseArgument argument,
            Expression? instance,
            IReadOnlyList<Expression> arguments)
            => _emitter.TryRewriteInvocation(operation, argument, out var expression)
                ? ParseJavaScriptExpression(expression)
                : null;
    }
}
