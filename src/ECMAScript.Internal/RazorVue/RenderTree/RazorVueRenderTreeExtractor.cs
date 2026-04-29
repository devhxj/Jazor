using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Jazor.RazorVue.Artifacts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue.RenderTree;

/// <summary>
/// Extracts a framework-agnostic RazorVue render tree from BuildRenderTree operations.
/// </summary>
internal sealed class RazorVueRenderTreeExtractor
{
    /// <summary>
    /// Converts BuildRenderTree syntax/operations into a <see cref="RazorVueRenderFragment"/>.
    /// </summary>
    public RazorVueRenderFragment Extract(RazorVueCompilationContext context, RazorVueSemanticSnapshot snapshot)
    {
        if (context is null)
            throw new ArgumentNullException(nameof(context));
        if (snapshot is null)
            throw new ArgumentNullException(nameof(snapshot));

        var method = snapshot.BuildRenderTreeMethod;
        if (method is null)
            return RazorVueRenderFragment.Empty;

        var builderParameters = method.Parameters
            .Where(static parameter => string.Equals(parameter.Name, "builder", StringComparison.Ordinal) ||
                                       string.Equals(parameter.Type.Name, "RenderTreeBuilder", StringComparison.Ordinal))
            .ToImmutableHashSet<IParameterSymbol>(SymbolEqualityComparer.Default);

        foreach (var reference in method.DeclaringSyntaxReferences)
        {
            if (reference.GetSyntax() is not MethodDeclarationSyntax methodSyntax)
                continue;

            var model = context.Compilation.GetSemanticModel(methodSyntax.SyntaxTree);
            var operation = methodSyntax.Body is not null
                ? model.GetOperation(methodSyntax.Body)
                : methodSyntax.ExpressionBody is not null
                    ? model.GetOperation(methodSyntax.ExpressionBody.Expression)
                    : null;

            if (operation is IBlockOperation block)
                return new Parser(snapshot, context.Symbols, builderParameters).Parse(block.Operations);

            if (operation is not null)
                return new Parser(snapshot, context.Symbols, builderParameters).Parse([operation]);
        }

        return RazorVueRenderFragment.Empty;
    }

    private sealed class Parser
    {
        private readonly RazorVueSemanticSnapshot _snapshot;
        private readonly RazorVueCompilationSymbols _symbols;
        private readonly ImmutableHashSet<IParameterSymbol> _builderParameters;
        private readonly List<RazorVueRenderNode> _rootChildren = [];
        private readonly Stack<OpenNodeBuilder> _openNodes = new();

        public Parser(
            RazorVueSemanticSnapshot snapshot,
            RazorVueCompilationSymbols symbols,
            ImmutableHashSet<IParameterSymbol> builderParameters)
        {
            _snapshot = snapshot;
            _symbols = symbols;
            _builderParameters = builderParameters;
        }

        public RazorVueRenderFragment Parse(IEnumerable<IOperation> operations)
        {
            foreach (var operation in operations)
                ParseOperation(operation);

            while (_openNodes.Count > 0)
                AddNode(_openNodes.Pop().Build());

            return new RazorVueRenderFragment(_rootChildren.ToImmutableArray());
        }

        private void ParseOperation(IOperation? operation)
        {
            var current = Unwrap(operation);
            if (current is null)
                return;

            switch (current)
            {
                case IExpressionStatementOperation expressionStatement:
                    ParseExpressionStatement(expressionStatement);
                    break;
                case IConditionalOperation conditional:
                    AddNode(new RazorVueConditionalNode(
                        conditional.Condition,
                        ParseNestedBranch(conditional.WhenTrue),
                        ParseNestedBranch(conditional.WhenFalse),
                        CreateOrigins(current, RazorVueOriginKind.Template)));
                    break;
                case IForEachLoopOperation foreachLoop:
                    AddNode(new RazorVueForEachNode(
                        foreachLoop.Locals.Length > 0 ? foreachLoop.Locals[0].Name : "item",
                        foreachLoop.Collection,
                        ParseNestedBranch(foreachLoop.Body),
                        CreateOrigins(current, RazorVueOriginKind.Template)));
                    break;
                case IBlockOperation block:
                    foreach (var child in block.Operations)
                        ParseOperation(child);
                    break;
                case IVariableDeclarationGroupOperation:
                    break;
                default:
                    break;
            }
        }

        private void ParseExpressionStatement(IExpressionStatementOperation expressionStatement)
        {
            if (Unwrap(expressionStatement.Operation) is not IInvocationOperation invocation)
                return;

            if (!IsRenderTreeBuilderInvocation(invocation))
                return;

            switch (invocation.TargetMethod.Name)
            {
                case "OpenElement":
                    OpenElement(invocation);
                    break;
                case "CloseElement":
                    CloseCurrentNode(expectedComponent: false);
                    break;
                case "OpenComponent":
                    OpenComponent(invocation);
                    break;
                case "CloseComponent":
                    CloseCurrentNode(expectedComponent: true);
                    break;
                case "AddAttribute":
                    AddAttribute(invocation);
                    break;
                case "AddContent":
                    AddContent(invocation);
                    break;
                case "AddMarkupContent":
                    AddMarkupContent(invocation);
                    break;
            }
        }

        private void OpenElement(IInvocationOperation invocation)
        {
            var tagName = GetConstantStringArgument(invocation, 1);
            if (!string.IsNullOrWhiteSpace(tagName))
                _openNodes.Push(new ElementBuilder(tagName!, CreateOrigins(invocation, RazorVueOriginKind.Template)));
        }

        private void OpenComponent(IInvocationOperation invocation)
        {
            if (invocation.TargetMethod.TypeArguments.Length != 1)
                return;

            var componentType = invocation.TargetMethod.TypeArguments[0];
            var resolutionName = GetComponentResolutionName(invocation, componentType.ToDisplayString());
            _openNodes.Push(new ComponentBuilder(
                componentType.Name,
                componentType.ToDisplayString(),
                resolutionName,
                CreateOrigins(invocation, RazorVueOriginKind.Template)));
        }

        private static string GetComponentResolutionName(IInvocationOperation invocation, string fallback)
        {
            if (invocation.Syntax is not InvocationExpressionSyntax invocationSyntax)
                return fallback;

            if (invocationSyntax.Expression is not MemberAccessExpressionSyntax { Name: GenericNameSyntax genericName })
                return fallback;

            if (genericName.TypeArgumentList.Arguments.Count != 1)
                return fallback;

            return genericName.TypeArgumentList.Arguments[0].ToString();
        }

        private void CloseCurrentNode(bool expectedComponent)
        {
            if (_openNodes.Count == 0)
                return;

            var current = _openNodes.Pop();
            if (current is ComponentBuilder != expectedComponent)
                return;

            AddNode(current.Build());
        }

        private void AddAttribute(IInvocationOperation invocation)
        {
            if (_openNodes.Count == 0)
                return;

            var name = GetConstantStringArgument(invocation, 1);
            if (string.IsNullOrWhiteSpace(name))
                return;

            _openNodes.Peek().AddAttribute(new RazorVueAttributeNode(
                name!,
                GetInvocationArgument(invocation, 2),
                CreateOrigins(invocation, RazorVueOriginKind.Template)));
        }

        private void AddContent(IInvocationOperation invocation)
        {
            var value = GetInvocationArgument(invocation, 1);
            if (value is null)
                return;

            var origins = CreateOrigins(invocation, RazorVueOriginKind.Template);
            if (TryGetConstantString(value) is string text)
            {
                AddNode(new RazorVueTextNode(text, origins));
                return;
            }

            if (TryResolveSlotOutlet(value) is string slotName)
            {
                AddNode(new RazorVueSlotOutletNode(slotName, null, origins));
                return;
            }

            AddNode(new RazorVueExpressionNode(value, origins));
        }

        private void AddMarkupContent(IInvocationOperation invocation)
        {
            if (TryGetConstantString(GetInvocationArgument(invocation, 1)) is not string markup ||
                string.IsNullOrEmpty(markup))
            {
                return;
            }

            AddNode(new RazorVueTextNode(markup, CreateOrigins(invocation, RazorVueOriginKind.Template)));
        }

        private RazorVueRenderFragment ParseNestedBranch(IOperation? operation)
        {
            var current = Unwrap(operation);
            if (current is null)
                return RazorVueRenderFragment.Empty;

            return current switch
            {
                IBlockOperation block => new Parser(_snapshot, _symbols, _builderParameters).Parse(block.Operations),
                _ => new Parser(_snapshot, _symbols, _builderParameters).Parse([current])
            };
        }

        private void AddNode(RazorVueRenderNode node)
        {
            if (_openNodes.Count > 0)
                _openNodes.Peek().AddChild(node);
            else
                _rootChildren.Add(node);
        }

        private bool IsRenderTreeBuilderInvocation(IInvocationOperation invocation)
        {
            if (_builderParameters.Count == 0)
                return false;

            return invocation.Instance is IParameterReferenceOperation parameterReference &&
                   _builderParameters.Contains(parameterReference.Parameter);
        }

        private string? TryResolveSlotOutlet(IOperation operation)
        {
            if (Unwrap(operation) is not IPropertyReferenceOperation propertyReference)
                return null;

            if (!IsCurrentComponentMember(propertyReference.Property, propertyReference.Instance))
                return null;

            if (!IsRenderFragment(propertyReference.Property.Type))
                return null;

            return string.Equals(propertyReference.Property.Name, "ChildContent", StringComparison.Ordinal)
                ? "default"
                : ToLowerCamelCase(propertyReference.Property.Name);
        }

        private bool IsRenderFragment(ITypeSymbol typeSymbol)
            => typeSymbol is INamedTypeSymbol namedType &&
               ((_symbols.RenderFragment is not null && SymbolEqualityComparer.Default.Equals(namedType.OriginalDefinition, _symbols.RenderFragment)) ||
                (_symbols.RenderFragmentOfT is not null && SymbolEqualityComparer.Default.Equals(namedType.OriginalDefinition, _symbols.RenderFragmentOfT)));

        private bool IsCurrentComponentMember(ISymbol symbol, IOperation? instance)
        {
            if (!SymbolEqualityComparer.Default.Equals(symbol.ContainingType, _snapshot.ComponentSymbol))
                return false;

            return instance is null || Unwrap(instance) is IInstanceReferenceOperation;
        }

        private static IOperation? GetInvocationArgument(IInvocationOperation invocation, int index)
        {
            if (invocation.Arguments.Length <= index)
                return null;

            return Unwrap(invocation.Arguments[index].Value);
        }

        private static string? GetConstantStringArgument(IInvocationOperation invocation, int index)
            => TryGetConstantString(GetInvocationArgument(invocation, index));

        private static string? TryGetConstantString(IOperation? operation)
        {
            var current = Unwrap(operation);
            if (current?.ConstantValue.HasValue == true &&
                current.ConstantValue.Value is string text)
                return text;

            return null;
        }

        private static IOperation? Unwrap(IOperation? operation)
        {
            var current = operation;
            while (current is IConversionOperation conversion && conversion.IsImplicit)
                current = conversion.Operand;

            return current;
        }

        private static ImmutableArray<RazorVueSourceOrigin> CreateOrigins(IOperation operation, RazorVueOriginKind originKind)
            => operation.Syntax is null
                ? ImmutableArray<RazorVueSourceOrigin>.Empty
                : ImmutableArray.Create(RazorVueSourceOrigin.FromLocation(operation.Syntax.GetLocation(), originKind));

        private static string ToLowerCamelCase(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            if (value.Length == 1)
                return char.ToLowerInvariant(value[0]).ToString();

            if (char.IsUpper(value[0]) && char.IsUpper(value[1]))
                return value;

            return char.ToLowerInvariant(value[0]) + value.Substring(1);
        }
    }

    private abstract class OpenNodeBuilder
    {
        private readonly List<RazorVueAttributeNode> _attributes = [];
        private readonly List<RazorVueRenderNode> _children = [];

        protected OpenNodeBuilder(ImmutableArray<RazorVueSourceOrigin> origins)
        {
            Origins = origins;
        }

        protected ImmutableArray<RazorVueSourceOrigin> Origins { get; }

        public void AddAttribute(RazorVueAttributeNode attribute)
            => _attributes.Add(attribute);

        public void AddChild(RazorVueRenderNode child)
            => _children.Add(child);

        protected ImmutableArray<RazorVueAttributeNode> BuildAttributes()
            => _attributes.ToImmutableArray();

        protected RazorVueRenderFragment BuildChildren()
            => new(_children.ToImmutableArray());

        public abstract RazorVueRenderNode Build();
    }

    private sealed class ElementBuilder(string tagName, ImmutableArray<RazorVueSourceOrigin> origins)
        : OpenNodeBuilder(origins)
    {
        public override RazorVueRenderNode Build()
            => new RazorVueElementNode(tagName, BuildAttributes(), BuildChildren(), Origins);
    }

    private sealed class ComponentBuilder(string componentName, string componentFullName, string resolutionName, ImmutableArray<RazorVueSourceOrigin> origins)
        : OpenNodeBuilder(origins)
    {
        public override RazorVueRenderNode Build()
            => new RazorVueComponentNode(componentName, componentFullName, resolutionName, BuildAttributes(), BuildChildren(), Origins);
    }
}

