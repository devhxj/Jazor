using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.Compiler;

/// <summary>
/// Computes the deterministic current-component member closure rooted at
/// <c>BuildRenderTree</c>, supported lifecycle methods, and captured handlers.
/// The closure is intentionally limited to members declared directly on the
/// current component type; external base types such as ComponentBase remain host
/// seams instead of being copied into the generated artifact.
/// </summary>
public sealed class CurrentComponentMemberClosure
{
    private readonly INamedTypeSymbol _componentType;
    private readonly HashSet<ISymbol> _includedSymbols;

    private CurrentComponentMemberClosure(INamedTypeSymbol componentType, HashSet<ISymbol> includedSymbols)
    {
        _componentType = componentType;
        _includedSymbols = includedSymbols;
        Members = componentType
            .GetMembers()
            .Where(symbol => ShouldInclude(symbol) && IsUserDeclaredInventoryMember(symbol))
            .ToImmutableArray();
    }

    public ImmutableArray<ISymbol> Members { get; }

    public static CurrentComponentMemberClosure Build(
        INamedTypeSymbol componentType,
        SemanticModel semanticModel,
        IEnumerable<IMethodSymbol> roots,
        CancellationToken cancellationToken = default)
    {
        if (semanticModel is null)
            throw new ArgumentNullException(nameof(semanticModel));

        return Create(
            componentType,
            semanticModel.Compilation,
            roots,
            rootOperations: Array.Empty<IOperation>(),
            cancellationToken);
    }

    public static CurrentComponentMemberClosure Create(
        INamedTypeSymbol componentType,
        Compilation compilation,
        IEnumerable<IMethodSymbol> roots,
        IEnumerable<IOperation> rootOperations,
        CancellationToken cancellationToken = default)
    {
        if (componentType is null)
            throw new ArgumentNullException(nameof(componentType));
        if (compilation is null)
            throw new ArgumentNullException(nameof(compilation));
        if (roots is null)
            throw new ArgumentNullException(nameof(roots));
        if (rootOperations is null)
            throw new ArgumentNullException(nameof(rootOperations));

        var builder = new Builder(componentType, compilation, cancellationToken);
        foreach (var root in roots)
        {
            builder.AddMember(root);
        }

        foreach (var operation in rootOperations)
        {
            builder.Visit(operation);
        }

        return new CurrentComponentMemberClosure(componentType, builder.IncludedSymbols);
    }

    public bool Contains(ISymbol symbol)
    {
        if (symbol is null)
            throw new ArgumentNullException(nameof(symbol));

        return ContainsCore(symbol);
    }

    public bool ShouldInclude(ISymbol symbol)
    {
        if (symbol is null)
            return false;

        if (ContainsCore(symbol))
            return true;

        return symbol switch
        {
            IMethodSymbol method when method.AssociatedSymbol is not null
                => ContainsCore(method.AssociatedSymbol),
            IFieldSymbol field when field.AssociatedSymbol is not null
                => ContainsCore(field.AssociatedSymbol),
            _ => false
        };
    }

    private bool ContainsCore(ISymbol symbol)
        => _includedSymbols.Contains(Canonicalize(symbol));

    private static ISymbol Canonicalize(ISymbol symbol)
        => symbol.OriginalDefinition;

    private static bool IsUserDeclaredInventoryMember(ISymbol symbol)
        => symbol switch
        {
            IMethodSymbol { AssociatedSymbol: not null } => false,
            IFieldSymbol { AssociatedSymbol: not null } => false,
            _ => true
        };

    private sealed class Builder
    {
        private readonly INamedTypeSymbol _componentType;
        private readonly Compilation _compilation;
        private readonly CancellationToken _cancellationToken;
        private readonly Queue<ISymbol> _pendingSymbols = new();
        private readonly HashSet<ISymbol> _processedSymbols = new(SymbolEqualityComparer.Default);
        private readonly CurrentComponentOperationWalker _walker;

        public Builder(INamedTypeSymbol componentType, Compilation compilation, CancellationToken cancellationToken)
        {
            _componentType = componentType;
            _compilation = compilation;
            _cancellationToken = cancellationToken;
            _walker = new CurrentComponentOperationWalker(this);
        }

        public HashSet<ISymbol> IncludedSymbols { get; } = new(SymbolEqualityComparer.Default);

        public void AddMember(ISymbol? symbol)
        {
            _cancellationToken.ThrowIfCancellationRequested();
            if (symbol is null)
                return;

            var canonical = Canonicalize(symbol);
            if (!IsDeclaredOnCurrentComponent(canonical))
                return;

            if (!IncludedSymbols.Add(canonical))
                return;

            _pendingSymbols.Enqueue(canonical);
            if (canonical is IPropertySymbol property)
            {
                AddMember(property.GetMethod);
                AddMember(property.SetMethod);
            }

            DrainPendingSymbols();
        }

        public void Visit(IOperation? operation)
        {
            _cancellationToken.ThrowIfCancellationRequested();
            if (operation is null)
                return;

            _walker.Visit(operation);
            DrainPendingSymbols();
        }

        private void DrainPendingSymbols()
        {
            while (_pendingSymbols.Count > 0)
            {
                _cancellationToken.ThrowIfCancellationRequested();
                var symbol = _pendingSymbols.Dequeue();
                if (!_processedSymbols.Add(symbol))
                    continue;

                switch (symbol)
                {
                    case IMethodSymbol method:
                        Visit(GetMethodOperation(method));
                        break;
                    case IFieldSymbol field:
                        Visit(GetFieldInitializerOperation(field));
                        break;
                    case IPropertySymbol property:
                        Visit(GetPropertyInitializerOperation(property));
                        break;
                }
            }
        }

        private bool IsDeclaredOnCurrentComponent(ISymbol symbol)
            => SymbolEqualityComparer.Default.Equals(
                symbol.ContainingType?.OriginalDefinition,
                _componentType.OriginalDefinition);

        private IOperation? GetMethodOperation(IMethodSymbol method)
        {
            foreach (var reference in method.DeclaringSyntaxReferences)
            {
                _cancellationToken.ThrowIfCancellationRequested();
                var syntax = reference.GetSyntax(_cancellationToken);
                var model = GetSemanticModel(syntax.SyntaxTree);
                switch (syntax)
                {
                    case MethodDeclarationSyntax methodDeclaration when methodDeclaration.Body is not null:
                        return model.GetOperation(methodDeclaration.Body, _cancellationToken);
                    case MethodDeclarationSyntax methodDeclaration when methodDeclaration.ExpressionBody is not null:
                        return model.GetOperation(methodDeclaration.ExpressionBody.Expression, _cancellationToken);
                    case AccessorDeclarationSyntax accessorDeclaration when accessorDeclaration.Body is not null:
                        return model.GetOperation(accessorDeclaration.Body, _cancellationToken);
                    case AccessorDeclarationSyntax accessorDeclaration when accessorDeclaration.ExpressionBody is not null:
                        return model.GetOperation(accessorDeclaration.ExpressionBody.Expression, _cancellationToken);
                    case ArrowExpressionClauseSyntax arrowExpression:
                        return model.GetOperation(arrowExpression.Expression, _cancellationToken);
                }
            }

            return null;
        }

        private IOperation? GetFieldInitializerOperation(IFieldSymbol field)
        {
            foreach (var reference in field.DeclaringSyntaxReferences)
            {
                _cancellationToken.ThrowIfCancellationRequested();
                if (reference.GetSyntax(_cancellationToken) is not VariableDeclaratorSyntax variable ||
                    variable.Initializer is null)
                {
                    continue;
                }

                return GetSemanticModel(variable.SyntaxTree)
                    .GetOperation(variable.Initializer.Value, _cancellationToken);
            }

            return null;
        }

        private IOperation? GetPropertyInitializerOperation(IPropertySymbol property)
        {
            foreach (var reference in property.DeclaringSyntaxReferences)
            {
                _cancellationToken.ThrowIfCancellationRequested();
                if (reference.GetSyntax(_cancellationToken) is not PropertyDeclarationSyntax propertyDeclaration ||
                    propertyDeclaration.Initializer is null)
                {
                    continue;
                }

                return GetSemanticModel(propertyDeclaration.SyntaxTree)
                    .GetOperation(propertyDeclaration.Initializer.Value, _cancellationToken);
            }

            return null;
        }

        private SemanticModel GetSemanticModel(SyntaxTree syntaxTree)
            => _compilation.GetSemanticModel(syntaxTree);

        private bool IsCurrentComponentMemberReference(ISymbol symbol, IOperation? instance)
        {
            if (!IsDeclaredOnCurrentComponent(symbol.OriginalDefinition))
                return false;

            if (symbol.IsStatic)
                return instance is null;

            return instance is null ||
                   instance is IInstanceReferenceOperation
                   {
                       ReferenceKind: InstanceReferenceKind.ContainingTypeInstance or
                           InstanceReferenceKind.ImplicitReceiver
                   };
        }

        private sealed class CurrentComponentOperationWalker : OperationWalker
        {
            private readonly Builder _builder;

            public CurrentComponentOperationWalker(Builder builder)
            {
                _builder = builder;
            }

            public override void VisitFieldReference(IFieldReferenceOperation operation)
            {
                if (_builder.IsCurrentComponentMemberReference(operation.Field, operation.Instance))
                    _builder.AddMember(operation.Field);

                base.VisitFieldReference(operation);
            }

            public override void VisitPropertyReference(IPropertyReferenceOperation operation)
            {
                if (_builder.IsCurrentComponentMemberReference(operation.Property, operation.Instance))
                    _builder.AddMember(operation.Property);

                base.VisitPropertyReference(operation);
            }

            public override void VisitInvocation(IInvocationOperation operation)
            {
                if (_builder.IsCurrentComponentMemberReference(operation.TargetMethod, operation.Instance))
                    _builder.AddMember(operation.TargetMethod);

                base.VisitInvocation(operation);
            }

            public override void VisitMethodReference(IMethodReferenceOperation operation)
            {
                if (_builder.IsCurrentComponentMemberReference(operation.Method, operation.Instance))
                    _builder.AddMember(operation.Method);

                base.VisitMethodReference(operation);
            }
        }
    }
}
