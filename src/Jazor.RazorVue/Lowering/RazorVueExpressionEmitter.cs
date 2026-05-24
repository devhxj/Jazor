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
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jazor.RazorVue.Lowering;

internal sealed partial class RazorVueExpressionEmitter
{
    internal const string ImperativeRenderContextAlias = "__jazorRenderContext";

    internal readonly record struct LifecyclePayloadPreludeBinding(string Code)
    {
        public static LifecyclePayloadPreludeBinding Const(string alias, string expression)
            => new("const " + alias + " = " + expression + ";");

        public static LifecyclePayloadPreludeBinding Statement(string code)
            => new(code);
    }

    internal readonly record struct LifecyclePayloadEmission(
        string Expression,
        bool UsesFirstRender,
        ImmutableArray<LifecyclePayloadPreludeBinding> PreludeBindings = default);
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
    private readonly ImmutableDictionary<string, ImmutableDictionary<string, VueSlotDescriptor>> _componentSlotsByPublicName;
    private readonly ImmutableDictionary<string, ImmutableDictionary<string, VueEmitDescriptor>> _componentEmitDescriptorsByRazorAlias;
    private readonly ImmutableDictionary<string, string> _componentReferences;
    private readonly ImmutableDictionary<string, ImmutableDictionary<string, string>> _componentEmitsByRazorAlias;

    private readonly ImmutableDictionary<string, VueLogicPropertyDescriptor> _logicPropertiesByName;
    private readonly ImmutableDictionary<string, VueLogicFieldDescriptor> _logicFieldsByName;
    private readonly ImmutableDictionary<string, ImmutableArray<VueLogicMethodDescriptor>> _logicMethodsByName;
    private readonly HashSet<IPropertySymbol> _requiredSetupProperties;
    private readonly HashSet<IFieldSymbol> _requiredSetupFields;
    private readonly HashSet<IMethodSymbol> _requiredSetupMethods;
    private bool _isSetupRewriteScopeActive;
    private HashSet<IPropertySymbol>? _capturedSetupPropertyDependencies;
    private HashSet<IFieldSymbol>? _capturedSetupFieldDependencies;
    private HashSet<IMethodSymbol>? _capturedSetupMethodDependencies;
    private Dictionary<ILocalSymbol, IOperation>? _sourceStableLocalInitializers;
    private Dictionary<ILocalSymbol, string>? _scopedLifecycleCallableAliases;
    private Dictionary<IMethodSymbol, string>? _scopedLifecycleLocalFunctionAliases;
    private Dictionary<ILocalSymbol, string>? _scopedLocalAliases;
    private Dictionary<IParameterSymbol, string>? _scopedParameterAliases;
    private readonly RazorVueCompilerModuleContext _compilerModuleContext;
    private readonly SenseArgument _compilerArgument;
    private readonly SemanticWalker _semanticWalker;
    private readonly List<RazorVueCompilerImportBinding> _compilerImports;

    public RazorVueExpressionEmitter(
        RazorVueSemanticSnapshot snapshot,
        ImmutableDictionary<string, string>? componentReferences = null,
        ImmutableDictionary<string, VueComponentDescriptor>? resolvedComponents = null,
        ImmutableDictionary<string, ImmutableDictionary<string, string>>? componentEmitsByRazorAlias = null,
        RazorVueCompilerModuleContext? compilerModuleContext = null)
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
        _componentSlotsByPublicName = BuildComponentSlotsByPublicName(_resolvedComponents);
        _componentEmitDescriptorsByRazorAlias = BuildComponentEmitDescriptorsByRazorAlias(_resolvedComponents);
        _componentEmitsByRazorAlias = componentEmitsByRazorAlias ?? ImmutableDictionary<string, ImmutableDictionary<string, string>>.Empty;
        _logicPropertiesByName = snapshot.Logic.Properties.ToImmutableDictionary(
            static property => property.Name,
            static property => property,
            StringComparer.Ordinal);
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
        _requiredSetupProperties = new HashSet<IPropertySymbol>(SymbolEqualityComparer.Default);
        _requiredSetupFields = new HashSet<IFieldSymbol>(SymbolEqualityComparer.Default);
        _requiredSetupMethods = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
        _compilerModuleContext = compilerModuleContext ?? RazorVueCompilerModuleContext.Create(snapshot);
        _compilerArgument = new SenseArgument(Sense.Any, UseImportAliases: true);
        _semanticWalker = new SemanticWalker(snapshot.ComponentSymbol, _compilerModuleContext.DeclaredNames)
        {
            Host = new RazorVueCompilerHost(this),
            AllowStructuralSourceDataCarrierLowering = true
        };
        _compilerImports = new List<RazorVueCompilerImportBinding>();
    }

    internal static LifecyclePayloadEmission EmitLifecyclePayload(
        RazorVueSemanticSnapshot snapshot,
        IMethodSymbol method,
        IOperation operation,
        bool allowFirstRenderPayload)
    {
        if (snapshot is null)
            throw new ArgumentNullException(nameof(snapshot));

        return new RazorVueExpressionEmitter(snapshot).EmitLifecyclePayload(method, operation, allowFirstRenderPayload);
    }

    internal LifecyclePayloadEmission EmitLifecyclePayload(IMethodSymbol method, IOperation operation, bool allowFirstRenderPayload)
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

    internal bool ContainsImperativeRenderBody(RazorVueRenderFragment fragment)
        => ContainsImperativeRenderBodyCore(fragment);

    internal string EmitImperativeRenderBody(RazorVueRenderFragment fragment)
    {
        var statements = EmitImperativeFragmentStatements(
            fragment,
            ImperativeRenderContextAlias,
            EmptyLocalScope,
            EmptyParameterScope);

        if (string.IsNullOrWhiteSpace(statements))
            return "return " + ImperativeRenderContextAlias + ".finish();";

        return statements + "\nreturn " + ImperativeRenderContextAlias + ".finish();";
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

        if (ContainsTemplateLocalDeclaration(fragment))
        {
            return new OptionalJsArgument(
                EmitFragmentWithTemplateLocals(fragment, allowedLocalSymbols, allowedParameterSymbols),
                true);
        }

        if (fragment.Children.Length == 1)
            return new OptionalJsArgument(EmitNode(fragment.Children[0], allowedLocalSymbols, allowedParameterSymbols), true);

        return new OptionalJsArgument(
            "[" + string.Join(", ", fragment.Children.Select(child => EmitNode(child, allowedLocalSymbols, allowedParameterSymbols))) + "]",
            true);
    }

    private static bool ContainsImperativeRenderBodyCore(RazorVueRenderFragment fragment)
    {
        if (fragment.Children.IsDefaultOrEmpty)
            return false;

        foreach (var child in fragment.Children)
        {
            switch (child)
            {
                case RazorVueImperativeBlockNode:
                    return true;
                case RazorVueElementNode element:
                    if (RazorVueOpenNodeReplayHelper.RequiresImperativeScopedReplay(element.ReplayOperations) ||
                        ContainsImperativeRenderBodyCore(element.Children))
                    {
                        return true;
                    }

                    break;
                case RazorVueComponentNode component:
                    if (RazorVueOpenNodeReplayHelper.RequiresImperativeScopedReplay(component.ReplayOperations) ||
                        ContainsImperativeRenderBodyCore(component.Children) ||
                        ContainsImperativeRenderBodyCore(component.AmbientDefaultSlotChildren))
                    {
                        return true;
                    }

                    foreach (var slotTemplate in component.SlotTemplates)
                    {
                        if (ContainsImperativeRenderBodyCore(slotTemplate.Children))
                            return true;
                    }

                    foreach (var assignment in component.ImplicitDefaultSlotAssignments)
                    {
                        if (ContainsImperativeRenderBodyCore(assignment.Children))
                            return true;
                    }

                    break;
                case RazorVueConditionalNode conditional:
                    if (ContainsImperativeRenderBodyCore(conditional.WhenTrue) ||
                        ContainsImperativeRenderBodyCore(conditional.WhenFalse))
                    {
                        return true;
                    }

                    break;
                case RazorVueTemplateScopeNode templateScope when ContainsImperativeRenderBodyCore(templateScope.Children):
                    return true;
                case RazorVueForEachNode loop when ContainsImperativeRenderBodyCore(loop.Body):
                    return true;
                case RazorVueForNode loop when ContainsImperativeRenderBodyCore(loop.Body):
                    return true;
            }
        }

        return false;
    }

    private string EmitFragment(
        RazorVueRenderFragment fragment,
        ImmutableHashSet<ILocalSymbol> allowedLocalSymbols,
        ImmutableHashSet<IParameterSymbol> allowedParameterSymbols)
    {
        var emission = EmitFragmentArgument(fragment, allowedLocalSymbols, allowedParameterSymbols);
        return emission.HasValue ? emission.Expression : "null";
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

        return _compilerImports
            .Concat(_compilerModuleContext.CompilerImports)
            .Distinct()
            .ToImmutableArray();
    }

    internal void AppendRequiredHelperTypeDeclarations(StringBuilder builder, string indent)
        => _compilerModuleContext.AppendRequiredHelperTypeDeclarations(builder, indent);

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

    internal T WithSetupRewriteScope<T>(Func<T> action)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));

        var previous = _isSetupRewriteScopeActive;
        _isSetupRewriteScopeActive = true;
        try
        {
            return action();
        }
        finally
        {
            _isSetupRewriteScopeActive = previous;
        }
    }

    internal SetupDependencyCapture CaptureSetupDependencies(Func<string> emitExpression)
    {
        if (emitExpression is null)
            throw new ArgumentNullException(nameof(emitExpression));

        var previousProperties = _capturedSetupPropertyDependencies;
        var previousFields = _capturedSetupFieldDependencies;
        var previousMethods = _capturedSetupMethodDependencies;
        var propertyDependencies = new HashSet<IPropertySymbol>(SymbolEqualityComparer.Default);
        var fieldDependencies = new HashSet<IFieldSymbol>(SymbolEqualityComparer.Default);
        var methodDependencies = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
        _capturedSetupPropertyDependencies = propertyDependencies;
        _capturedSetupFieldDependencies = fieldDependencies;
        _capturedSetupMethodDependencies = methodDependencies;
        try
        {
            var expression = emitExpression();
            return new SetupDependencyCapture(
                expression,
                propertyDependencies.ToImmutableArray(),
                fieldDependencies.ToImmutableArray(),
                methodDependencies.ToImmutableArray());
        }
        finally
        {
            _capturedSetupPropertyDependencies = previousProperties;
            _capturedSetupFieldDependencies = previousFields;
            _capturedSetupMethodDependencies = previousMethods;
        }
    }

    internal SetupDependencyCapture CaptureSetupDependenciesWithParameterAliases(
        ImmutableArray<IParameterSymbol> parameters,
        IReadOnlyList<string> aliases,
        Func<string> emitExpression)
    {
        if (emitExpression is null)
            throw new ArgumentNullException(nameof(emitExpression));

        return WithScopedParameterAliases(parameters, aliases, () => CaptureSetupDependencies(emitExpression));
    }

    private void RecordRequiredSetupProperty(IPropertySymbol property)
    {
        _requiredSetupProperties.Add(property);
        _capturedSetupPropertyDependencies?.Add(property);
    }

    private void RecordRequiredSetupField(IFieldSymbol field)
    {
        _requiredSetupFields.Add(field);
        _capturedSetupFieldDependencies?.Add(field);
    }

    private void RecordRequiredSetupMethod(IMethodSymbol method)
    {
        _requiredSetupMethods.Add(method);
        _capturedSetupMethodDependencies?.Add(method);
    }

    internal T WithSourceStableLocalInitializers<T>(
        IReadOnlyDictionary<ILocalSymbol, IOperation> initializers,
        Func<T> action)
    {
        var previous = _sourceStableLocalInitializers;
        var current = previous is null
            ? new Dictionary<ILocalSymbol, IOperation>(SymbolEqualityComparer.Default)
            : new Dictionary<ILocalSymbol, IOperation>(previous, SymbolEqualityComparer.Default);

        foreach (var pair in initializers)
            current[pair.Key] = pair.Value;

        _sourceStableLocalInitializers = current;
        try
        {
            return action();
        }
        finally
        {
            _sourceStableLocalInitializers = previous;
        }
    }

    internal T WithScopedLifecycleCallableAliases<T>(
        IReadOnlyDictionary<ILocalSymbol, string> localAliases,
        IReadOnlyDictionary<IMethodSymbol, string> localFunctionAliases,
        Func<T> action)
    {
        var previousLocalAliases = _scopedLifecycleCallableAliases;
        var previousFunctionAliases = _scopedLifecycleLocalFunctionAliases;

        var currentLocalAliases = previousLocalAliases is null
            ? new Dictionary<ILocalSymbol, string>(SymbolEqualityComparer.Default)
            : new Dictionary<ILocalSymbol, string>(previousLocalAliases, SymbolEqualityComparer.Default);
        foreach (var pair in localAliases)
            currentLocalAliases[pair.Key] = pair.Value;

        var currentFunctionAliases = previousFunctionAliases is null
            ? new Dictionary<IMethodSymbol, string>(SymbolEqualityComparer.Default)
            : new Dictionary<IMethodSymbol, string>(previousFunctionAliases, SymbolEqualityComparer.Default);
        foreach (var pair in localFunctionAliases)
            currentFunctionAliases[pair.Key] = pair.Value;

        _scopedLifecycleCallableAliases = currentLocalAliases;
        _scopedLifecycleLocalFunctionAliases = currentFunctionAliases;
        try
        {
            return action();
        }
        finally
        {
            _scopedLifecycleCallableAliases = previousLocalAliases;
            _scopedLifecycleLocalFunctionAliases = previousFunctionAliases;
        }
    }

    internal readonly record struct SetupDependencyCapture(
        string Expression,
        ImmutableArray<IPropertySymbol> PropertyDependencies,
        ImmutableArray<IFieldSymbol> FieldDependencies,
        ImmutableArray<IMethodSymbol> MethodDependencies);

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

    private string MaterializeCompilerStatement(Node node, SenseArgument argument)
    {
        if (node is not Statement statement)
        {
            throw new NotSupportedException(
                $"RazorVue lifecycle callable lowering expected a statement but received '{node.Type}'.");
        }

        if (!argument.HasVarDeclarator)
            return statement.ToKnRECMAScript();

        var statements = new List<Statement>();
        var declarators = argument.FlushVarDeclarator();
        if (declarators.Count > 0)
            statements.Add(new VariableDeclaration(VariableDeclarationKind.Let, declarators));

        var builder = new StringBuilder();
        foreach (var item in statements)
            builder.AppendLine(item.ToKnRECMAScript());

        builder.Append(statement.ToKnRECMAScript());
        return builder.ToString().Trim();
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
            TryRewriteInvocation(invocationOperation, argument, useSetupEmitter: false, out expression))
        {
            return true;
        }

        if (current is IPropertyReferenceOperation property &&
            TryRewritePropertyReference(property, argument, useSetupEmitter: false, out expression))
        {
            return true;
        }

        if (current is IFieldReferenceOperation field &&
            TryRewriteFieldReference(field, argument, useSetupEmitter: false, out expression))
        {
            return true;
        }

        if (current is IMethodReferenceOperation methodReferenceOperation &&
            TryRewriteMethodReference(methodReferenceOperation, argument, useSetupEmitter: false, out expression))
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
                if (element.Key is not null)
                {
                    foreach (var origin in element.Key.Origins)
                        yield return origin;
                }
                foreach (var attribute in element.Attributes)
                {
                    foreach (var origin in attribute.Origins)
                        yield return origin;
                }

                foreach (var childOrigin in CollectOrigins(element.Children))
                    yield return childOrigin;
                break;
            case RazorVueComponentNode component:
                if (component.Key is not null)
                {
                    foreach (var origin in component.Key.Origins)
                        yield return origin;
                }
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

                foreach (var implicitDefaultSlotAssignment in component.ImplicitDefaultSlotAssignments)
                {
                    foreach (var origin in implicitDefaultSlotAssignment.Origins)
                        yield return origin;
                    foreach (var childOrigin in CollectOrigins(implicitDefaultSlotAssignment.Children))
                        yield return childOrigin;
                }

                foreach (var childOrigin in CollectOrigins(component.Children))
                    yield return childOrigin;
                break;
            case RazorVueLocalDeclarationNode localDeclaration:
                foreach (var origin in localDeclaration.Origins)
                    yield return origin;
                break;
            case RazorVueTemplateScopeNode templateScope:
                foreach (var origin in templateScope.Origins)
                    yield return origin;
                foreach (var childOrigin in CollectOrigins(templateScope.Children))
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
            case RazorVueImperativeBlockNode:
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
            => _emitter.TryRewriteInvocation(operation, argument, useSetupEmitter: _emitter._isSetupRewriteScopeActive, out var expression)
                ? ParseJavaScriptExpression(expression)
                : null;

        public override Expression? RewriteConversionPreorder(IConversionOperation operation, SenseArgument argument)
            => _emitter.TryRewriteStaticMarkupStringConversion(operation, out var expression)
                ? ParseJavaScriptExpression(expression)
                : null;

        public override Expression? RewriteObjectCreationPreorder(IObjectCreationOperation operation, SenseArgument argument)
        {
            _emitter._compilerModuleContext.RecordObjectCreation(operation);
            return _emitter.TryRewriteStaticMarkupStringObjectCreation(operation, out var expression)
                ? ParseJavaScriptExpression(expression)
                : null;
        }

        public override VariableDeclarator? RewriteVariableDeclaratorPreorder(IVariableDeclaratorOperation operation, SenseArgument argument)
            => _emitter.TryRewriteVariableDeclarator(operation, argument, out var declaratorExpression)
                ? new VariableDeclarator(new Identifier(operation.Symbol.Name), ParseJavaScriptExpression(declaratorExpression))
                : null;

        public override Expression? RewriteSimpleAssignmentPreorder(ISimpleAssignmentOperation operation, SenseArgument argument)
            => _emitter.TryRewriteSimpleAssignment(operation, argument, out var expression)
                ? ParseJavaScriptExpression(expression)
                : null;

        public override Expression? RewriteFieldReference(
            IFieldReferenceOperation operation,
            SenseArgument argument,
            Expression? instance)
            => _emitter.TryRewriteFieldReference(operation, argument, useSetupEmitter: _emitter._isSetupRewriteScopeActive, out var expression)
                ? ParseJavaScriptExpression(expression)
                : null;

        public override Expression? RewritePropertyReference(
            IPropertyReferenceOperation operation,
            SenseArgument argument,
            Expression? instance,
            IReadOnlyList<Expression> arguments)
            => _emitter.TryRewritePropertyReference(operation, argument, useSetupEmitter: _emitter._isSetupRewriteScopeActive, out var expression)
                ? ParseJavaScriptExpression(expression)
                : null;

        public override Expression? RewriteMethodReference(
            IMethodReferenceOperation operation,
            SenseArgument argument,
            Expression? instance)
            => _emitter.TryRewriteMethodReference(operation, argument, useSetupEmitter: _emitter._isSetupRewriteScopeActive, out var expression)
                ? ParseJavaScriptExpression(expression)
                : null;

        public override Expression? RewriteParameterReference(IParameterReferenceOperation operation, SenseArgument argument)
            => _emitter.TryRewriteParameterReference(operation, argument, out var expression)
                ? ParseJavaScriptExpression(expression)
                : null;

        public override Expression? RewriteLocalReference(ILocalReferenceOperation operation, SenseArgument argument)
            => _emitter.TryRewriteLocalReference(operation, argument, out var expression)
                ? ParseJavaScriptExpression(expression)
                : null;

        public override Expression? RewriteInvocation(
            IInvocationOperation operation,
            SenseArgument argument,
            Expression? instance,
            IReadOnlyList<Expression> arguments)
            => _emitter.TryRewriteInvocation(operation, argument, useSetupEmitter: _emitter._isSetupRewriteScopeActive, out var expression)
                ? ParseJavaScriptExpression(expression)
                : null;

        public override Expression? RewriteInstanceReference(IInstanceReferenceOperation operation, SenseArgument argument)
            => _emitter.TryRewriteInstanceReference(operation, argument, out var expression)
                ? ParseJavaScriptExpression(expression)
                : null;
    }
}
