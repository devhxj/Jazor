using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;

namespace Jazor.RazorVue;

internal static class RazorVueSourceStableLocalInitializerHelper
{
    private readonly record struct SourceStableLocalInitializer(
        IOperation Initializer,
        SyntaxNode? AllowedAssignmentSyntax);

    public static bool IsSupportedLifecycleFirstRenderCarrierType(ITypeSymbol? type)
    {
        if (type is null)
            return false;

        if (type.SpecialType == SpecialType.System_Boolean)
            return true;

        return type is INamedTypeSymbol
        {
            OriginalDefinition.SpecialType: SpecialType.System_Nullable_T,
            TypeArguments.Length: 1
        } namedType &&
               namedType.TypeArguments[0].SpecialType == SpecialType.System_Boolean;
    }

    public static bool CanParticipateInLifecycleCompilerFallback(ITypeSymbol? type)
        => type is not null && type.TypeKind != TypeKind.Error;

    public static Dictionary<ILocalSymbol, IOperation> CollectSourceStableLocalInitializers(
        Compilation compilation,
        IReadOnlyList<IOperation> operations,
        Func<ITypeSymbol?, bool> isSupportedCarrierType)
        => CollectSourceStableLocalInitializersCore(
            compilation,
            operations,
            isSupportedCarrierType,
            includeDeconstructionLocals: true);

    private static Dictionary<ILocalSymbol, IOperation> CollectSourceStableLocalInitializersCore(
        Compilation compilation,
        IReadOnlyList<IOperation> operations,
        Func<ITypeSymbol?, bool> isSupportedCarrierType,
        bool includeDeconstructionLocals)
    {
        var collected = CollectSourceStableLocalInitializerStates(operations, isSupportedCarrierType);
        var result = new Dictionary<ILocalSymbol, IOperation>(SymbolEqualityComparer.Default);
        foreach (var pair in collected)
        {
            if (TryGetSourceStableLocalInitializer(compilation, pair.Key, isSupportedCarrierType, out var initializer) &&
                initializer is not null)
            {
                result[pair.Key] = initializer;
            }
        }

        if (includeDeconstructionLocals)
        {
            foreach (var local in CollectDeconstructionDeclaredLocals(operations, isSupportedCarrierType))
            {
                if (result.ContainsKey(local))
                    continue;

                if (TryGetSourceStableLocalInitializer(compilation, local, isSupportedCarrierType, out var initializer) &&
                    initializer is not null)
                {
                    result[local] = initializer;
                }
            }
        }

        return result;
    }

    public static bool TryGetSourceStableLocalInitializer(
        Compilation compilation,
        ILocalSymbol local,
        Func<ITypeSymbol?, bool> isSupportedCarrierType,
        out IOperation? initializer)
    {
        initializer = null;
        if (!isSupportedCarrierType(local.Type))
            return false;

        foreach (var syntaxReference in local.DeclaringSyntaxReferences)
        {
            if (syntaxReference.GetSyntax() is not VariableDeclaratorSyntax declarator)
                continue;

            var semanticModel = compilation.GetSemanticModel(declarator.SyntaxTree);
            if (!TryGetSourceStableLocalInitializer(
                    local,
                    declarator,
                    semanticModel,
                    isSupportedCarrierType,
                    out initializer))
            {
                continue;
            }

            return initializer is not null;
        }

        if (TryGetSourceStableDeconstructionLocalInitializer(
                compilation,
                local,
                isSupportedCarrierType,
                out initializer))
        {
            return initializer is not null;
        }

        return false;
    }

    public static bool IsSourceStableLocalInitializerInvalidatedByLaterWrites(
        Compilation compilation,
        ILocalSymbol local,
        Func<ITypeSymbol?, bool> isSupportedCarrierType)
    {
        if (!isSupportedCarrierType(local.Type))
            return false;

        foreach (var syntaxReference in local.DeclaringSyntaxReferences)
        {
            if (syntaxReference.GetSyntax() is not VariableDeclaratorSyntax declarator)
                continue;

            var semanticModel = compilation.GetSemanticModel(declarator.SyntaxTree);
            if (!TryIsSourceStableLocalInitializerInvalidatedByLaterWrites(
                    local,
                    declarator,
                    semanticModel,
                    isSupportedCarrierType,
                    out var invalidated))
            {
                continue;
            }

            return invalidated;
        }

        foreach (var syntaxReference in local.DeclaringSyntaxReferences)
        {
            if (syntaxReference.GetSyntax() is not SingleVariableDesignationSyntax designation)
                continue;

            var semanticModel = compilation.GetSemanticModel(designation.SyntaxTree);
            return HasMutableLocalWrites(local, designation, semanticModel);
        }

        return false;
    }

    private static bool TryGetSourceStableLocalInitializer(
        ILocalSymbol local,
        VariableDeclaratorSyntax declarator,
        SemanticModel semanticModel,
        Func<ITypeSymbol?, bool> isSupportedCarrierType,
        out IOperation? initializer)
    {
        initializer = null;
        if (!isSupportedCarrierType(local.Type))
            return false;

        if (declarator.Initializer?.Value is not null)
        {
            if (HasMutableLocalWrites(local, declarator, semanticModel))
                return false;

            if (!RazorVuePropertyInitializerHelper.TryGetNormalizedOperation(
                    semanticModel,
                    declarator.Initializer.Value,
                    out var initializerOperation))
            {
                return false;
            }

            initializer = initializerOperation;
            return true;
        }

        if (declarator.Parent?.Parent?.Parent is not BlockSyntax rootBlock ||
            semanticModel.GetOperation(rootBlock) is not IBlockOperation rootOperation)
        {
            return false;
        }

        var collected = CollectSourceStableLocalInitializerStates(rootOperation.Operations, isSupportedCarrierType);
        if (!collected.TryGetValue(local, out var resolved))
            return false;

        if (HasMutableLocalWrites(local, declarator, semanticModel, resolved.AllowedAssignmentSyntax))
            return false;

        initializer = resolved.Initializer;
        return true;
    }

    private static bool TryIsSourceStableLocalInitializerInvalidatedByLaterWrites(
        ILocalSymbol local,
        VariableDeclaratorSyntax declarator,
        SemanticModel semanticModel,
        Func<ITypeSymbol?, bool> isSupportedCarrierType,
        out bool invalidated)
    {
        invalidated = false;
        if (!isSupportedCarrierType(local.Type))
            return false;

        if (declarator.Initializer?.Value is not null)
        {
            invalidated = HasMutableLocalWrites(local, declarator, semanticModel);
            return true;
        }

        if (declarator.Parent?.Parent?.Parent is not BlockSyntax rootBlock ||
            semanticModel.GetOperation(rootBlock) is not IBlockOperation rootOperation)
        {
            return false;
        }

        var collected = CollectSourceStableLocalInitializerStates(rootOperation.Operations, isSupportedCarrierType);
        if (!collected.TryGetValue(local, out var resolved))
        {
            invalidated = HasMutableLocalWrites(local, declarator, semanticModel);
            return true;
        }

        invalidated = HasMutableLocalWrites(local, declarator, semanticModel, resolved.AllowedAssignmentSyntax);
        return true;
    }

    private static Dictionary<ILocalSymbol, SourceStableLocalInitializer> CollectSourceStableLocalInitializerStates(
        IReadOnlyList<IOperation> operations,
        Func<ITypeSymbol?, bool> isSupportedCarrierType)
    {
        var result = new Dictionary<ILocalSymbol, SourceStableLocalInitializer>(SymbolEqualityComparer.Default);
        var pendingAssignments = new HashSet<ILocalSymbol>(SymbolEqualityComparer.Default);

        foreach (var operation in operations)
        {
            var current = RazorVueOperationNormalizer.Unwrap(operation);
            if (current is null or IEmptyOperation)
                continue;

            if (pendingAssignments.Count > 0)
            {
                if (TryExtractImmediateAssignedLocal(
                        current,
                        pendingAssignments,
                        out var local,
                        out var initializer,
                        out var assignmentSyntax))
                {
                    result[local] = new SourceStableLocalInitializer(
                        ResolveInitializerAlias(initializer, result),
                        assignmentSyntax);
                    continue;
                }

                if (TryRegisterSourceStableContinuationDeclarators(current, result, pendingAssignments, isSupportedCarrierType))
                    continue;

                pendingAssignments.Clear();
            }

            switch (current)
            {
                case IVariableDeclarationGroupOperation declarationGroup:
                    RegisterSourceStableDeclarators(
                        declarationGroup.Declarations,
                        result,
                        pendingAssignments,
                        isSupportedCarrierType);
                    break;
                case IVariableDeclarationOperation declarationOperation:
                    RegisterSourceStableDeclarators(
                        [declarationOperation],
                        result,
                        pendingAssignments,
                        isSupportedCarrierType);
                    break;
            }
        }

        return result;
    }

    private static bool TryRegisterSourceStableContinuationDeclarators(
        IOperation current,
        Dictionary<ILocalSymbol, SourceStableLocalInitializer> result,
        HashSet<ILocalSymbol> pendingAssignments,
        Func<ITypeSymbol?, bool> isSupportedCarrierType)
    {
        switch (current)
        {
            case IVariableDeclarationGroupOperation declarationGroup:
                RegisterSourceStableDeclarators(
                    declarationGroup.Declarations,
                    result,
                    pendingAssignments,
                    isSupportedCarrierType);
                return true;
            case IVariableDeclarationOperation declarationOperation:
                RegisterSourceStableDeclarators(
                    [declarationOperation],
                    result,
                    pendingAssignments,
                    isSupportedCarrierType);
                return true;
            default:
                return false;
        }
    }

    private static void RegisterSourceStableDeclarators(
        IEnumerable<IVariableDeclarationOperation> declarations,
        Dictionary<ILocalSymbol, SourceStableLocalInitializer> result,
        HashSet<ILocalSymbol> pendingAssignments,
        Func<ITypeSymbol?, bool> isSupportedCarrierType)
    {
        foreach (var declaration in declarations)
        {
            foreach (var declarator in declaration.Declarators)
            {
                if (!isSupportedCarrierType(declarator.Symbol.Type))
                    continue;

                if (declarator.Initializer?.Value is not { } initializer)
                {
                    pendingAssignments.Add(declarator.Symbol);
                    continue;
                }

                result[declarator.Symbol] = new SourceStableLocalInitializer(
                    RazorVueOperationNormalizer.Unwrap(initializer) ?? initializer,
                    null);
            }
        }
    }

    private static bool TryExtractImmediateAssignedLocal(
        IOperation? operation,
        HashSet<ILocalSymbol> pendingAssignments,
        out ILocalSymbol localSymbol,
        out IOperation initializer,
        out SyntaxNode? assignmentSyntax)
    {
        localSymbol = default!;
        initializer = default!;
        assignmentSyntax = null;
        switch (operation)
        {
            case ISimpleAssignmentOperation assignment
                when assignment.Target is ILocalReferenceOperation localReference &&
                     pendingAssignments.Remove(localReference.Local):
                localSymbol = localReference.Local;
                initializer = RazorVueOperationNormalizer.Unwrap(assignment.Value) ?? assignment.Value;
                assignmentSyntax = assignment.Syntax;
                return true;
            case IExpressionStatementOperation expressionStatement:
                return TryExtractImmediateAssignedLocal(
                    expressionStatement.Operation,
                    pendingAssignments,
                    out localSymbol,
                    out initializer,
                    out assignmentSyntax);
            case IBlockOperation block when block.Operations.Length == 1:
                return TryExtractImmediateAssignedLocal(
                    block.Operations[0],
                    pendingAssignments,
                    out localSymbol,
                    out initializer,
                    out assignmentSyntax);
            default:
                return false;
        }
    }

    private static IOperation ResolveInitializerAlias(
        IOperation initializer,
        IReadOnlyDictionary<ILocalSymbol, SourceStableLocalInitializer> result)
    {
        var current = RazorVueOperationNormalizer.Unwrap(initializer) ?? initializer;
        var visitedLocals = new HashSet<ILocalSymbol>(SymbolEqualityComparer.Default);
        while (current is ILocalReferenceOperation localReference &&
               visitedLocals.Add(localReference.Local) &&
               result.TryGetValue(localReference.Local, out var resolved))
        {
            current = resolved.Initializer;
        }

        return current;
    }

    private static bool HasMutableLocalWrites(
        ILocalSymbol local,
        VariableDeclaratorSyntax declarator,
        SemanticModel semanticModel,
        SyntaxNode? allowedImmediateAssignmentSyntax = null)
    {
        return HasMutableLocalWrites(local, (SyntaxNode)declarator, semanticModel, allowedImmediateAssignmentSyntax);
    }

    private static bool HasMutableLocalWrites(
        ILocalSymbol local,
        SyntaxNode declarationSyntax,
        SemanticModel semanticModel,
        SyntaxNode? allowedImmediateAssignmentSyntax = null)
    {
        var rootBlock = declarationSyntax.Ancestors().OfType<BlockSyntax>().FirstOrDefault();
        if (rootBlock is null)
            return true;

        if (semanticModel.GetOperation(rootBlock) is not IOperation rootOperation)
            return true;

        foreach (var operation in EnumerateOperations(rootOperation))
        {
            switch (operation)
            {
                case IAssignmentOperation assignment
                    when IsAllowedInitializerAssignment(assignment, declarationSyntax, allowedImmediateAssignmentSyntax):
                    continue;
                case IAssignmentOperation assignment
                    when ReferencesLocalSymbol(assignment.Target, local):
                    return true;
                case IIncrementOrDecrementOperation incrementOrDecrement
                    when ReferencesLocalSymbol(incrementOrDecrement.Target, local):
                    return true;
                case IArgumentOperation argument
                    when argument.Parameter?.RefKind is RefKind.Ref or RefKind.Out &&
                         ReferencesLocalSymbol(argument.Value, local):
                    return true;
            }
        }

        return false;
    }

    private static IEnumerable<IOperation> EnumerateOperations(IOperation root)
    {
        yield return root;
        foreach (var child in root.ChildOperations)
        {
            if (child is null)
                continue;

            foreach (var nested in EnumerateOperations(child))
                yield return nested;
        }
    }

    private static bool IsAllowedInitializerAssignment(
        IAssignmentOperation assignment,
        SyntaxNode declarationSyntax,
        SyntaxNode? allowedImmediateAssignmentSyntax)
        => IsDeclaratorInitializerAssignment(assignment, declarationSyntax) ||
           (allowedImmediateAssignmentSyntax is not null &&
            assignment.Syntax is not null &&
            ReferenceEquals(assignment.Syntax.SyntaxTree, allowedImmediateAssignmentSyntax.SyntaxTree) &&
            assignment.Syntax.Span.Equals(allowedImmediateAssignmentSyntax.Span));

    private static bool IsDeclaratorInitializerAssignment(
        IAssignmentOperation assignment,
        SyntaxNode declarationSyntax)
        => declarationSyntax is VariableDeclaratorSyntax declarator &&
           assignment.Syntax is not null &&
           assignment.Syntax.Span == declarator.Span;

    private static bool ReferencesLocalSymbol(IOperation? operation, ILocalSymbol local)
    {
        var current = RazorVueOperationNormalizer.Unwrap(operation);
        if (current is null)
            return false;

        if (current is ILocalReferenceOperation localReference &&
            SymbolEqualityComparer.Default.Equals(localReference.Local, local))
        {
            return true;
        }

        foreach (var child in current.ChildOperations)
        {
            if (child is not null && ReferencesLocalSymbol(child, local))
                return true;
        }

        return false;
    }

    private static IEnumerable<ILocalSymbol> CollectDeconstructionDeclaredLocals(
        IReadOnlyList<IOperation> operations,
        Func<ITypeSymbol?, bool> isSupportedCarrierType)
    {
        var seen = new HashSet<ILocalSymbol>(SymbolEqualityComparer.Default);
        foreach (var operation in operations)
        {
            foreach (var current in EnumerateOperations(operation))
            {
                if (current is not IDeclarationExpressionOperation declarationExpression)
                    continue;

                foreach (var local in CollectDeconstructionDeclaredLocalsFromTarget(
                             declarationExpression.Expression,
                             isSupportedCarrierType,
                             seen))
                {
                    yield return local;
                }
            }
        }
    }

    private static IEnumerable<ILocalSymbol> CollectDeconstructionDeclaredLocalsFromTarget(
        IOperation? operation,
        Func<ITypeSymbol?, bool> isSupportedCarrierType,
        HashSet<ILocalSymbol> seen)
    {
        var current = RazorVueOperationNormalizer.Unwrap(operation);
        if (current is null)
            yield break;

        switch (current)
        {
            case ILocalReferenceOperation localReference
                when isSupportedCarrierType(localReference.Local.Type) &&
                     seen.Add(localReference.Local):
                yield return localReference.Local;
                yield break;
            case IDeclarationExpressionOperation declarationExpression:
                foreach (var local in CollectDeconstructionDeclaredLocalsFromTarget(
                             declarationExpression.Expression,
                             isSupportedCarrierType,
                             seen))
                {
                    yield return local;
                }

                yield break;
            case ITupleOperation tupleOperation:
                foreach (var element in tupleOperation.Elements)
                {
                    foreach (var local in CollectDeconstructionDeclaredLocalsFromTarget(
                                 element,
                                 isSupportedCarrierType,
                                 seen))
                    {
                        yield return local;
                    }
                }

                yield break;
            default:
                yield break;
        }
    }

    private static bool TryGetSourceStableDeconstructionLocalInitializer(
        Compilation compilation,
        ILocalSymbol local,
        Func<ITypeSymbol?, bool> isSupportedCarrierType,
        out IOperation? initializer)
    {
        initializer = null;
        if (!isSupportedCarrierType(local.Type))
            return false;

        foreach (var syntaxReference in local.DeclaringSyntaxReferences)
        {
            var syntax = syntaxReference.GetSyntax();
            if (syntax is not SingleVariableDesignationSyntax singleDesignation)
                continue;

            if (!TryGetSourceStableDeconstructionLocalInitializer(
                    compilation,
                    local,
                    singleDesignation,
                    isSupportedCarrierType,
                    out initializer))
            {
                continue;
            }

            return initializer is not null;
        }

        return false;
    }

    private static bool TryGetSourceStableDeconstructionLocalInitializer(
        Compilation compilation,
        ILocalSymbol local,
        SingleVariableDesignationSyntax designation,
        Func<ITypeSymbol?, bool> isSupportedCarrierType,
        out IOperation? initializer)
    {
        initializer = null;
        if (designation.Parent is not ParenthesizedVariableDesignationSyntax parenthesizedDesignation)
            return false;

        var semanticModel = compilation.GetSemanticModel(designation.SyntaxTree);
        var designationOperation = semanticModel.GetOperation(parenthesizedDesignation);
        if (designationOperation is null &&
            designation.Parent?.Parent is not null)
        {
            designationOperation = semanticModel.GetOperation(designation.Parent.Parent);
        }

        if (designationOperation is null)
            return false;

        var deconstructionTarget = TryUnwrapDeconstructionTarget(designationOperation);
        if (deconstructionTarget is null)
            return false;

        if (!TryFindDeconstructionSlotForLocal(deconstructionTarget, local, out var slotPath))
            return false;

        if (!TryGetOwningDeconstructionOperation(parenthesizedDesignation, semanticModel, out var deconstruction))
            return false;

        if (HasMutableLocalWrites(local, designation, semanticModel, deconstruction.Syntax))
            return false;

        var sourceStableLocals = CollectSourceStableLocalInitializersCore(
            compilation,
            GetContainingBlockOperations(deconstruction),
            isSupportedCarrierType,
            includeDeconstructionLocals: false);

        var resolvedInitializers = new Dictionary<ILocalSymbol, SourceStableLocalInitializer>(SymbolEqualityComparer.Default);
        foreach (var pair in sourceStableLocals)
            resolvedInitializers[pair.Key] = new SourceStableLocalInitializer(pair.Value, null);

        var resolvedValue = ResolveInitializerAlias(
            RazorVueOperationNormalizer.Unwrap(deconstruction.Value) ?? deconstruction.Value,
            resolvedInitializers);

        if (!TryProjectDeconstructionSlotValue(
                resolvedValue,
                slotPath,
                out initializer))
        {
            return false;
        }

        return initializer is not null;
    }

    private static IOperation? TryUnwrapDeconstructionTarget(IOperation operation)
    {
        var current = RazorVueOperationNormalizer.Unwrap(operation) ?? operation;
        return current switch
        {
            IDeclarationExpressionOperation declarationExpression => RazorVueOperationNormalizer.Unwrap(declarationExpression.Expression) ?? declarationExpression.Expression,
            ITupleOperation tuple => tuple,
            _ => null
        };
    }

    private static bool TryFindDeconstructionSlotForLocal(
        IOperation target,
        ILocalSymbol local,
        out ImmutableArray<int> slotPath)
    {
        var builder = ImmutableArray.CreateBuilder<int>();
        if (TryFindDeconstructionSlotForLocalCore(target, local, builder))
        {
            slotPath = builder.ToImmutable();
            return true;
        }

        slotPath = default;
        return false;
    }

    private static bool TryFindDeconstructionSlotForLocalCore(
        IOperation operation,
        ILocalSymbol local,
        ImmutableArray<int>.Builder path)
    {
        if (RazorVueOperationNormalizer.Unwrap(operation) is not ITupleOperation tupleOperation)
            return false;

        for (var index = 0; index < tupleOperation.Elements.Length; index++)
        {
            var element = RazorVueOperationNormalizer.Unwrap(tupleOperation.Elements[index]) ?? tupleOperation.Elements[index];
            switch (element)
            {
                case IDeclarationExpressionOperation declarationExpression:
                    if (TryIsLocalDeclarationExpression(declarationExpression, local))
                    {
                        path.Add(index);
                        return true;
                    }

                    if (TryUnwrapDeconstructionTarget(declarationExpression) is { } nestedDeclarationTarget &&
                        TryFindNestedDeconstructionSlot(index, nestedDeclarationTarget, local, path))
                    {
                        return true;
                    }

                    break;
                case ILocalReferenceOperation localReference
                    when SymbolEqualityComparer.Default.Equals(localReference.Local, local):
                    path.Add(index);
                    return true;
                case ITupleOperation nestedTuple
                    when TryFindNestedDeconstructionSlot(index, nestedTuple, local, path):
                    return true;
            }
        }

        return false;
    }

    private static bool TryFindNestedDeconstructionSlot(
        int index,
        IOperation nestedTarget,
        ILocalSymbol local,
        ImmutableArray<int>.Builder path)
    {
        var originalCount = path.Count;
        path.Add(index);
        if (TryFindDeconstructionSlotForLocalCore(nestedTarget, local, path))
            return true;

        path.RemoveAt(path.Count - 1);
        while (path.Count > originalCount)
            path.RemoveAt(path.Count - 1);
        return false;
    }

    private static bool TryIsLocalDeclarationExpression(IDeclarationExpressionOperation declarationExpression, ILocalSymbol local)
        => RazorVueOperationNormalizer.Unwrap(declarationExpression.Expression) is ILocalReferenceOperation localReference &&
           SymbolEqualityComparer.Default.Equals(localReference.Local, local);

    private static bool TryGetOwningDeconstructionOperation(
        ParenthesizedVariableDesignationSyntax designation,
        SemanticModel semanticModel,
        out IDeconstructionAssignmentOperation deconstruction)
    {
        foreach (var ancestor in designation.Ancestors())
        {
            if (semanticModel.GetOperation(ancestor) is IDeconstructionAssignmentOperation deconstructionOperation)
            {
                deconstruction = deconstructionOperation;
                return true;
            }
        }

        deconstruction = default!;
        return false;
    }

    private static IReadOnlyList<IOperation> GetContainingBlockOperations(IDeconstructionAssignmentOperation deconstruction)
    {
        foreach (var ancestor in deconstruction.Syntax.Ancestors())
        {
            if (ancestor is not BlockSyntax blockSyntax)
                continue;

            if (deconstruction.SemanticModel?.GetOperation(blockSyntax) is IBlockOperation blockOperation)
                return blockOperation.Operations;
        }

        return [];
    }

    private static bool TryProjectDeconstructionSlotValue(
        IOperation source,
        ImmutableArray<int> slotPath,
        out IOperation? initializer)
    {
        initializer = null;
        if (slotPath.IsDefaultOrEmpty)
            return false;

        var currentOperation = RazorVueOperationNormalizer.Unwrap(source) ?? source;
        foreach (var index in slotPath)
        {
            if (!TryProjectSingleDeconstructionSlot(
                    currentOperation,
                    index,
                    out var nextOperation))
            {
                initializer = null;
                return false;
            }

            currentOperation = nextOperation;
        }

        initializer = currentOperation;
        return true;
    }

    private static bool TryProjectSingleDeconstructionSlot(
        IOperation source,
        int slotIndex,
        out IOperation projected)
    {
        projected = default!;

        source = RazorVueOperationNormalizer.Unwrap(source) ?? source;
        if (source is IConversionOperation conversion &&
            (RazorVueOperationNormalizer.Unwrap(conversion.Operand) ?? conversion.Operand) is ITupleOperation conversionTuple)
        {
            source = conversionTuple;
        }

        if (source is ITupleOperation tupleOperation)
        {
            if (slotIndex < 0 || slotIndex >= tupleOperation.Elements.Length)
                return false;

            projected = RazorVueOperationNormalizer.Unwrap(tupleOperation.Elements[slotIndex]) ?? tupleOperation.Elements[slotIndex];
            return true;
        }

        return false;
    }
}
