using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue.RazorSdk;

/// <summary>
/// Computes the deterministic current-component member closure rooted at
/// <c>BuildRenderTree</c>, supported lifecycle methods, and captured handlers.
/// The closure is intentionally limited to members declared on the current
/// component or its source-declared base types. External base types such as
/// ComponentBase remain host seams instead of being copied into the generated
/// artifact.
/// </summary>
/// <remarks>
/// closure 的结果必须确定且最小：只把当前渲染路径实际依赖的源声明成员带入输出。
/// 外部框架基类不属于当前模块的源码闭包，应通过 host seam 使用其约定能力。
/// </remarks>
internal sealed class CurrentComponentMemberClosure
{
    private readonly INamedTypeSymbol _componentType;
    private readonly HashSet<ISymbol> _includedSymbols;

    private CurrentComponentMemberClosure(INamedTypeSymbol componentType, HashSet<ISymbol> includedSymbols)
    {
        _componentType = componentType;
        _includedSymbols = includedSymbols;
        Members = includedSymbols
            .Where(IsUserDeclaredInventoryMember)
            .OrderBy(GetStableMemberKey, StringComparer.Ordinal)
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

    private static string GetStableMemberKey(ISymbol symbol)
    {
        foreach (var location in symbol.Locations)
        {
            if (!location.IsInSource)
                continue;

            var span = location.GetLineSpan();
            var path = (span.Path ?? string.Empty).Replace('\\', '/');
            var start = span.StartLinePosition;
            return path +
                   "|" +
                   start.Line.ToString("D10", System.Globalization.CultureInfo.InvariantCulture) +
                   "|" +
                   start.Character.ToString("D10", System.Globalization.CultureInfo.InvariantCulture) +
                   "|" +
                   symbol.Kind +
                   "|" +
                   symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        }

        return "~|" + symbol.Kind + "|" + symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }

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
            if (!CanInclude(canonical))
                return;

            if (canonical is not INamedTypeSymbol &&
                IsNestedRuntimeClassMember(canonical) &&
                canonical.ContainingType is not null)
            {
                AddMember(canonical.ContainingType);
            }

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
                        Visit(GetPropertyGetterOperation(property));
                        break;
                    case INamedTypeSymbol:
                        break;
                }
            }
        }

        private bool IsDeclaredOnCurrentComponent(ISymbol symbol)
            => symbol.ContainingType is not null &&
               ComponentSymbolPolicy.IsDeclaredOnComponentHierarchy(_componentType, symbol.ContainingType) &&
               symbol.ContainingType.DeclaringSyntaxReferences.Length > 0;

        private bool CanInclude(ISymbol symbol)
            => IsDeclaredOnCurrentComponent(symbol) ||
               symbol is INamedTypeSymbol type && IsRuntimeClassNestedInCurrentComponent(type) ||
               IsNestedRuntimeClassMember(symbol);

        private bool IsRuntimeClassNestedInCurrentComponent(ITypeSymbol? type)
        {
            if (type is not INamedTypeSymbol namedType ||
                namedType.TypeKind != TypeKind.Class ||
                namedType.IsRecord ||
                namedType.IsStatic ||
                namedType.ContainingType is null)
            {
                return false;
            }

            // Component helpers can use nested runtime classes for implementation details.
            // Follow only an all-runtime-class containment chain so records/interfaces never
            // become accidental artifact declarations.
            for (var current = namedType.ContainingType; current is not null; current = current.ContainingType)
            {
                if (ComponentSymbolPolicy.IsDeclaredOnComponentHierarchy(_componentType, current))
                    return true;

                if (current.TypeKind != TypeKind.Class || current.IsRecord || current.IsStatic)
                    return false;
            }

            return false;
        }

        private bool IsNestedRuntimeClassMember(ISymbol symbol)
            => symbol.ContainingType is not null &&
               IsRuntimeClassNestedInCurrentComponent(symbol.ContainingType);

        private IOperation? GetMethodOperation(IMethodSymbol method)
        {
            foreach (var reference in method.DeclaringSyntaxReferences)
            {
                _cancellationToken.ThrowIfCancellationRequested();
                var syntax = reference.GetSyntax(_cancellationToken);
                var model = GetSemanticModel(syntax.SyntaxTree);
                switch (syntax)
                {
                    case ConstructorDeclarationSyntax constructorDeclaration when constructorDeclaration.Body is not null:
                        return model.GetOperation(constructorDeclaration.Body, _cancellationToken);
                    case ConstructorDeclarationSyntax constructorDeclaration when constructorDeclaration.ExpressionBody is not null:
                        return model.GetOperation(constructorDeclaration.ExpressionBody.Expression, _cancellationToken);
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

        private IOperation? GetPropertyGetterOperation(IPropertySymbol property)
        {
            if (property.GetMethod is null)
                return null;

            foreach (var reference in property.DeclaringSyntaxReferences)
            {
                _cancellationToken.ThrowIfCancellationRequested();
                if (reference.GetSyntax(_cancellationToken) is not PropertyDeclarationSyntax propertyDeclaration)
                    continue;

                var model = GetSemanticModel(propertyDeclaration.SyntaxTree);
                if (propertyDeclaration.ExpressionBody is not null)
                    return model.GetOperation(propertyDeclaration.ExpressionBody.Expression, _cancellationToken);

                var getter = propertyDeclaration.AccessorList?.Accessors.FirstOrDefault(static accessor =>
                    accessor.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.GetAccessorDeclaration));
                if (getter?.Body is not null)
                    return model.GetOperation(getter.Body, _cancellationToken);
                if (getter?.ExpressionBody is not null)
                    return model.GetOperation(getter.ExpressionBody.Expression, _cancellationToken);
            }

            return GetMethodOperation(property.GetMethod);
        }

        private SemanticModel GetSemanticModel(SyntaxTree syntaxTree)
            => _compilation.GetSemanticModel(syntaxTree);

        private bool IsCurrentComponentMemberReference(ISymbol symbol, IOperation? instance)
        {
            if (!IsDeclaredOnCurrentComponent(symbol.OriginalDefinition))
                return false;

            if (symbol.IsStatic)
                return instance is null;

            return IsCurrentComponentReceiver(instance);
        }

        private static bool IsCurrentComponentReceiver(IOperation? operation)
            => operation switch
            {
                null => true,
                IConversionOperation conversion => IsCurrentComponentReceiver(conversion.Operand),
                IInstanceReferenceOperation
                {
                    ReferenceKind: InstanceReferenceKind.ContainingTypeInstance or
                        InstanceReferenceKind.ImplicitReceiver
                } => true,
                _ => false
            };

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
                else if (_builder.IsNestedRuntimeClassMember(operation.Field))
                    _builder.AddMember(operation.Field);

                base.VisitFieldReference(operation);
            }

            public override void VisitPropertyReference(IPropertyReferenceOperation operation)
            {
                if (_builder.IsCurrentComponentMemberReference(operation.Property, operation.Instance))
                    _builder.AddMember(operation.Property);
                else if (_builder.IsNestedRuntimeClassMember(operation.Property))
                    _builder.AddMember(operation.Property);

                base.VisitPropertyReference(operation);
            }

            public override void VisitInvocation(IInvocationOperation operation)
            {
                if (_builder.IsCurrentComponentMemberReference(operation.TargetMethod, operation.Instance))
                    _builder.AddMember(operation.TargetMethod);
                else if (_builder.IsNestedRuntimeClassMember(operation.TargetMethod))
                    _builder.AddMember(operation.TargetMethod);

                base.VisitInvocation(operation);
            }

            public override void VisitObjectCreation(IObjectCreationOperation operation)
            {
                if (_builder.IsRuntimeClassNestedInCurrentComponent(operation.Type))
                {
                    _builder.AddMember(operation.Type);
                    if (operation.Constructor is not null)
                        _builder.AddMember(operation.Constructor);
                }

                base.VisitObjectCreation(operation);
            }

            public override void VisitMethodReference(IMethodReferenceOperation operation)
            {
                if (_builder.IsCurrentComponentMemberReference(operation.Method, operation.Instance))
                    _builder.AddMember(operation.Method);
                else if (_builder.IsNestedRuntimeClassMember(operation.Method))
                    _builder.AddMember(operation.Method);

                base.VisitMethodReference(operation);
            }
        }
    }
}
