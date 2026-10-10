using Acornima;
using Acornima.Ast;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.Compiler;

public partial class SemanticWalker
{
    private Expression BuildErasedUnionProjection(IPropertyReferenceOperation operation, Expression instance, SenseArgument argument)
    {
        if (operation.Property.Name == "Value")
            return instance;

        var target = UnwrapUnionNullable(operation.Property.Type);
        // Open generic projections remain compile-time annotations. A closed use of the
        // same property is checked below against its substituted Roslyn return type.
        if (target.TypeKind == TypeKind.TypeParameter)
            return instance;

        var value = StabilizePatternExpression(operation, instance, argument, "union-projection", out var initialization);
        var domain = GetErasedUnionLeaves(operation.Property.ContainingType).ToArray();
        var selected = GetErasedUnionLeaves(target).ToArray();
        var compilation = operation.SemanticModel!.Compilation;
        foreach (var branch in selected)
        {
            foreach (var sibling in domain)
            {
                if (selected.Any(candidate => SymbolEqualityComparer.Default.Equals(candidate, sibling)) ||
                    branch.IsValueType || sibling.IsValueType || branch is IArrayTypeSymbol || sibling is IArrayTypeSymbol)
                    continue;
                var forward = compilation.ClassifyCommonConversion(branch, sibling);
                var reverse = compilation.ClassifyCommonConversion(sibling, branch);
                // A user-defined conversion can create a different JS value (e.g. Date
                // from string). Only reference assignability loses an exact branch tag.
                if ((forward.IsReference && forward.IsImplicit) || (reverse.IsReference && reverse.IsImplicit))
                    return HandleTransformationFailure<Expression>(operation,
                        $"Union projection '{operation.Property}' needs an exact branch tag because '{branch}' and '{sibling}' are assignable. Erased JavaScript values cannot recover that tag. Use a non-overlapping typed model or declare an explicit host projection mapping.");
            }
        }
        Expression? match = null;
        foreach (var branch in selected)
        {
            var condition = BuildErasedUnionBranchMatch(operation, branch, domain, selected, value, argument);
            match = match is null ? condition : new LogicalExpression(Operator.LogicalOr, match, condition);
        }
        if (match is null)
            return HandleTransformationFailure<Expression>(operation, $"Union projection '{operation.Property}' has no runtime branch.");

        // Value and the type guards must share one read, including a getter/Proxy receiver.
        // 分支不匹配返回 null；不能把 AsX 一律擦除为 identity，或重复求值接收者。
        return PrependEvaluation(initialization, new ConditionalExpression(match, value, Null));
    }

    private Expression BuildErasedUnionBranchMatch(IOperation operation, ITypeSymbol branch,
        IReadOnlyList<ITypeSymbol> domain, IReadOnlyList<ITypeSymbol> selected, Expression value, SenseArgument argument)
    {
        if (branch is INamedTypeSymbol { TypeKind: TypeKind.Enum } enumType && Util.IsStringEnumType(enumType))
        {
            Expression? enumMatch = null;
            foreach (var member in enumType.GetMembers().OfType<IFieldSymbol>().Where(static member => member.HasConstantValue))
            {
                var condition = new NonLogicalBinaryExpression(Operator.StrictEquality, value, CreateStringLiteral(GetStringEnumLiteralText(member)));
                enumMatch = enumMatch is null ? condition : new LogicalExpression(Operator.LogicalOr, enumMatch, condition);
            }
            return enumMatch ?? new BooleanLiteral(false, "false");
        }

        if (!Util.IsObjectLiteralHostType(branch))
            return CreateTypeMatchExpr(operation, branch, value, argument);

        // A closed union can distinguish its sole object-literal branch by the other
        // runtime domains. Never infer it from properties or a library-specific type name.
        Expression match = new LogicalExpression(Operator.LogicalAnd,
            new NonLogicalBinaryExpression(Operator.StrictInequality, value, Null),
            new NonLogicalBinaryExpression(Operator.StrictEquality,
                new NonUpdateUnaryExpression(Operator.TypeOf, value), CreateStringLiteral("object")));
        match = new LogicalExpression(Operator.LogicalAnd, match,
            new NonUpdateUnaryExpression(Operator.LogicalNot,
                new CallExpression(IsArrayExpr, NodeList.From(value), optional: false)));
        foreach (var sibling in domain)
        {
            if (selected.Any(candidate => SymbolEqualityComparer.Default.Equals(candidate, sibling)))
                continue;
            var mapper = GetMapperType(sibling).Mapper;
            if (mapper == TypeMapper.Object || Util.IsObjectLiteralHostType(sibling))
                return HandleTransformationFailure<Expression>(operation,
                    $"Union projection to '{branch}' cannot distinguish the erased object branch '{sibling}'. Declare an explicit host projection mapping.");
            if (mapper is TypeMapper.Class or TypeMapper.Date or TypeMapper.Map or TypeMapper.Set)
                match = new LogicalExpression(Operator.LogicalAnd, match,
                    new NonUpdateUnaryExpression(Operator.LogicalNot, CreateTypeMatchExpr(operation, sibling, value, argument)));
        }
        return match;
    }

    private static ITypeSymbol UnwrapUnionNullable(ITypeSymbol type)
        => type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable
            ? nullable.TypeArguments[0] : type;

    private static IEnumerable<ITypeSymbol> GetErasedUnionLeaves(ITypeSymbol type)
    {
        type = UnwrapUnionNullable(type);
        if (type is not INamedTypeSymbol named || !Util.IsHostErasedUnionType(named))
        {
            yield return type;
            yield break;
        }
        // Native union constructors persist the authored branch symbols in metadata.
        // Do not maintain a second registry of union names or primitive combinations.
        foreach (var constructor in named.InstanceConstructors.Where(constructor => constructor.Parameters.Length == 1 &&
            !SymbolEqualityComparer.Default.Equals(constructor.Parameters[0].Type, named)))
            foreach (var branch in GetErasedUnionLeaves(constructor.Parameters[0].Type))
                yield return branch;
    }
}
