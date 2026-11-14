using Acornima.Ast;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.Compiler;

public partial class SemanticWalker
{
    // Validate missing runtime protocols at the operation that needs them. Erased type
    // annotations alone are deliberately not grounds for rejecting an ordinary signature.
    private void ValidateRuntimeProtocol(IOperation operation)
    {
        var dynamicBinding = operation switch
        {
            IConversionOperation conversion => conversion.Operand.Type?.TypeKind == TypeKind.Dynamic &&
                conversion.Type!.SpecialType != SpecialType.System_Object && conversion.Type.TypeKind != TypeKind.Dynamic,
            IBinaryOperation binary => binary.LeftOperand.Type?.TypeKind == TypeKind.Dynamic || binary.RightOperand.Type?.TypeKind == TypeKind.Dynamic,
            IUnaryOperation unary => unary.Operand.Type!.TypeKind == TypeKind.Dynamic,
            IIncrementOrDecrementOperation update => update.Target.Type!.TypeKind == TypeKind.Dynamic,
            ICompoundAssignmentOperation assignment => assignment.Target.Type!.TypeKind == TypeKind.Dynamic || assignment.Value.Type?.TypeKind == TypeKind.Dynamic,
            _ => false
        };
        if (dynamicBinding)
            HandleTransformationFailure<Node>(operation, "Dynamic operator or conversion requires the C# runtime binder. Use a statically typed operand; JavaScript coercion does not preserve this operation.");

        if (operation.Syntax is FixedStatementSyntax)
            HandleTransformationFailure<Node>(operation, "Fixed statements require CLR pinning and stable memory addresses, which the JavaScript target does not provide.");
        if (operation.Kind == OperationKind.None &&
            (operation.Syntax.IsKind(SyntaxKind.PointerIndirectionExpression) ||
             operation.Syntax is ElementAccessExpressionSyntax && operation.ChildOperations.Any(child => child.Type is IPointerTypeSymbol)))
            HandleTransformationFailure<Node>(operation, "Pointer dereference and indexing require an unmanaged memory location; JavaScript array access is not a pointer protocol.");

        if (operation is IVariableDeclaratorOperation { Symbol.RefKind: not RefKind.None } ||
            operation is ISimpleAssignmentOperation { IsRef: true } ||
            operation is IConditionalOperation { IsRef: true })
            HandleTransformationFailure<Node>(operation, "Reference aliases (ref locals, ref reassignment and conditional ref expressions) require a persistent storage-location protocol. Use ordinary values or supported ref/out calls.");

        var callable = operation switch
        {
            ILocalFunctionOperation localFunction => localFunction.Symbol,
            IAnonymousFunctionOperation anonymousFunction => anonymousFunction.Symbol,
            _ => null
        };
        if (callable is { ReturnsByRef: true } or { ReturnsByRefReadonly: true })
            HandleTransformationFailure<Node>(operation, "Source ref returns require a storage-location protocol; by-reference returns cannot be lowered to JavaScript values.");

        if (operation is IBinaryOperation { Type.TypeKind: TypeKind.Delegate, OperatorKind: BinaryOperatorKind.Add or BinaryOperatorKind.Subtract } ||
            operation is ICompoundAssignmentOperation { Target.Type.TypeKind: TypeKind.Delegate })
        {
            if (operation is IBinaryOperation eventBinary && GetUnsupportedEventBinaryOperationReason(eventBinary) is { } eventFailure)
                HandleTransformationFailure<Node>(operation, eventFailure);
            HandleTransformationFailure<Node>(operation, "Delegate combination/removal requires an invocation-list protocol. Subscribe individual handlers instead of using JavaScript arithmetic.");
        }

        var member = operation switch
        {
            IMemberReferenceOperation reference => reference.Member,
            IInvocationOperation invocation => invocation.TargetMethod,
            _ => null
        };
        if (member is { IsStatic: true, IsAbstract: true, ContainingType.TypeKind: TypeKind.Interface })
            HandleTransformationFailure<Node>(operation, "Static abstract interface dispatch requires a concrete generic runtime implementation. Erased type parameters cannot supply that implementation.");
        if (member is { IsStatic: false, ContainingType.TypeKind: TypeKind.Interface } &&
            !member.IsAbstract && member.Locations.Any(location => location.IsInSource))
            HandleTransformationFailure<Node>(operation, "Default interface implementations have no emitted runtime slot. Provide a concrete class implementation before calling this member.");
        if (member is { IsStatic: true, ContainingType.IsGenericType: true } &&
            IsGenericStaticStorage(member) && member.Locations.Any(location => location.IsInSource) &&
            !Util.HasECMAScriptSupportMarker(member.ContainingType) && !Util.IsECMAScriptModuleType(member.ContainingType))
            HandleTransformationFailure<Node>(operation, "Static storage on generic source classes requires separate state for each closed type. The current erased class cannot provide that state; use an explicit state owner.");

        if (operation.Type is INamedTypeSymbol { IsRecord: true, IsValueType: true, IsReadOnly: false })
            HandleTransformationFailure<Node>(operation, "Mutable record structs require value copies at storage boundaries. Use a readonly record struct or a class until mutable value-copy semantics are supported.");

        var callArguments = operation switch
        {
            IInvocationOperation invocation => invocation.Arguments,
            IObjectCreationOperation creation => creation.Arguments,
            _ => default
        };
        if (!callArguments.IsDefault)
        {
            var writable = callArguments.Where(arg => arg.Parameter!.RefKind is RefKind.Ref or RefKind.Out && arg.Value is not IDiscardOperation).ToArray();
            for (var i = 0; i < writable.Length; i++)
                for (var j = i + 1; j < writable.Length; j++)
                {
                    var firstValue = writable[i].Value is IDeclarationExpressionOperation firstDeclaration ? firstDeclaration.Expression : writable[i].Value;
                    var secondValue = writable[j].Value is IDeclarationExpressionOperation secondDeclaration ? secondDeclaration.Expression : writable[j].Value;
                    // Distinct storage categories and distinct field symbols cannot alias.
                    // The existing target lowering still evaluates receivers/indexes once.
                    if (!AreDistinctReferenceLocations(firstValue, secondValue))
                        HandleTransformationFailure<Node>(operation,
                            "Potentially aliased ref/out arguments require shared storage during the call; use provably distinct locations until shared-location ref/out calls are supported.");
                }
        }

        if (operation is IObjectCreationOperation { Type.IsRecord: true, Constructor: { } constructor } &&
            !Util.IsHostErasedUnionType((INamedTypeSymbol)operation.Type!) &&
            constructor.DeclaringSyntaxReferences.Any(reference => reference.GetSyntax() is ConstructorDeclarationSyntax declaration &&
                (declaration.Body?.Statements.Count > 0 || declaration.ExpressionBody is not null || declaration.Initializer is not null)))
            HandleTransformationFailure<Node>(operation, "Structural record construction cannot discard an executable constructor body. Use positional data initialization or a class for constructor behavior.");
    }

    private static bool IsGenericStaticStorage(ISymbol member)
        => member is IFieldSymbol { IsConst: false } || member is IPropertySymbol property &&
            property.DeclaringSyntaxReferences.Any(reference => ((PropertyDeclarationSyntax)reference.GetSyntax())
                .AccessorList?.Accessors.Any(accessor => accessor.Body is null && accessor.ExpressionBody is null) == true);

    private static bool AreDistinctReferenceLocations(IOperation first, IOperation second)
    {
        if (first is ILocalReferenceOperation a && second is ILocalReferenceOperation b)
            return !SymbolEqualityComparer.Default.Equals(a.Local, b.Local);
        if (first is ILocalReferenceOperation local)
            return second is not IParameterReferenceOperation { Parameter.RefKind: not RefKind.None } parameter ||
                SymbolEqualityComparer.Default.Equals(local.Local.ContainingSymbol, parameter.Parameter.ContainingSymbol);
        if (second is ILocalReferenceOperation)
            return AreDistinctReferenceLocations(second, first);
        if (first is IParameterReferenceOperation { Parameter.RefKind: RefKind.None } valueParameter)
            return second is not IParameterReferenceOperation other || !SymbolEqualityComparer.Default.Equals(valueParameter.Parameter, other.Parameter);
        if (second is IParameterReferenceOperation { Parameter.RefKind: RefKind.None })
            return AreDistinctReferenceLocations(second, first);
        if (first is IFieldReferenceOperation fieldA && second is IFieldReferenceOperation fieldB)
            return !SymbolEqualityComparer.Default.Equals(fieldA.Field, fieldB.Field);
        if (first is IArrayElementReferenceOperation arrayA && second is IArrayElementReferenceOperation arrayB)
            return arrayA.Indices.Length != arrayB.Indices.Length || arrayA.Indices.Zip(arrayB.Indices, (a, b) =>
                a.ConstantValue.HasValue && b.ConstantValue.HasValue && !Equals(a.ConstantValue.Value, b.ConstantValue.Value)).Any(different => different);
        return first is IArrayElementReferenceOperation && second is IFieldReferenceOperation ||
            second is IArrayElementReferenceOperation && first is IFieldReferenceOperation;
    }
}
