using System.Numerics;
using Acornima;
using Acornima.Ast;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.Compiler;

public partial class SemanticWalker
{
    private static (int Bits, bool Signed) IntegralWidth(ITypeSymbol? type)
    {
        if (type is INamedTypeSymbol { TypeKind: TypeKind.Enum } enumeration)
            type = enumeration.EnumUnderlyingType;
        return type?.SpecialType switch
        {
            SpecialType.System_SByte => (8, true), SpecialType.System_Byte => (8, false),
            SpecialType.System_Int16 => (16, true), SpecialType.System_UInt16 => (16, false),
            SpecialType.System_Int32 => (32, true), SpecialType.System_UInt32 => (32, false),
            SpecialType.System_Int64 => (64, true), SpecialType.System_UInt64 => (64, false),
            _ => default
        };
    }

    private static Expression NumericCall(string owner, string name, params Expression[] arguments)
        => new CallExpression(new MemberExpression(new Identifier(owner), new Identifier(name), false, false), NodeList.From(arguments), false);

    private static Expression NormalizeIntegral(Expression value, ITypeSymbol type, bool isChecked)
    {
        var (bits, signed) = IntegralWidth(type);
        if (isChecked)
        {
            // Evaluate once before checking. Overflow throws before the caller writes back.
            var parameter = new Identifier("$integer");
            var minimum = signed ? -(BigInteger.One << (bits - 1)) : BigInteger.Zero;
            var maximum = (BigInteger.One << (signed ? bits - 1 : bits)) - 1;
            Expression Bound(BigInteger bound) => bits == 64
                ? new BigIntLiteral(bound, bound.ToString(System.Globalization.CultureInfo.InvariantCulture) + "n")
                : new NumericLiteral((double)bound, bound.ToString(System.Globalization.CultureInfo.InvariantCulture));
            var outside = new LogicalExpression(Operator.LogicalOr,
                new NonLogicalBinaryExpression(Operator.LessThan, parameter, Bound(minimum)),
                new NonLogicalBinaryExpression(Operator.GreaterThan, parameter, Bound(maximum)));
            return InvokeIntegralFunction(value, parameter, new IfStatement(outside, NumericThrow("OverflowException"), null), parameter);
        }
        if (bits == 64)
            return NumericCall("BigInt", signed ? "asIntN" : "asUintN", new NumericLiteral(64, "64"), value);
        if (bits == 32)
            return new NonLogicalBinaryExpression(signed ? Operator.BitwiseOr : Operator.UnsignedRightShift, value, new NumericLiteral(0, "0"));
        if (!signed)
            return new NonLogicalBinaryExpression(Operator.BitwiseAnd, value, new NumericLiteral((1 << bits) - 1, ((1 << bits) - 1).ToString()));
        return new NonLogicalBinaryExpression(Operator.RightShift,
            new NonLogicalBinaryExpression(Operator.LeftShift, value, new NumericLiteral(32 - bits, (32 - bits).ToString())),
            new NumericLiteral(32 - bits, (32 - bits).ToString()));
    }

    private static ThrowStatement NumericThrow(string message)
        => new(new NewExpression(new Identifier("Error"), NodeList.From<Expression>(JavaScriptAstFactory.CreateStringLiteral(message))));

    private static Expression InvokeIntegralFunction(Expression value, Identifier parameter, Statement check, Expression result)
        => new CallExpression(new ArrowFunctionExpression(NodeList.From<Node>(parameter),
            new FunctionBody(NodeList.From<Statement>(check, new ReturnStatement(result)), true), false, false), NodeList.From(value), false);

    private static Expression IntegralBinary(BinaryOperatorKind kind, Expression left, Expression right, ITypeSymbol type, bool isChecked)
    {
        var width = IntegralWidth(type);
        if (kind is BinaryOperatorKind.Divide or BinaryOperatorKind.Remainder)
        {
            return JavaScriptAstFactory.CreateSingleEvaluationArrowInvocation([("$dividend", left), ("$divisor", right)], values =>
            {
                Expression zero = width.Bits == 64 ? new BigIntLiteral(BigInteger.Zero, "0n") : new NumericLiteral(0, "0");
                var quotient = new NonLogicalBinaryExpression(kind == BinaryOperatorKind.Divide ? Operator.Division : Operator.Remainder, values[0], values[1]);
                Expression result = kind == BinaryOperatorKind.Divide && width.Bits != 64 ? NumericCall("Math", "trunc", quotient) : quotient;
                // CLR integers have no negative zero, including a negative exact remainder.
                if (width.Bits != 64)
                    result = new NonLogicalBinaryExpression(Operator.Addition, result, new NumericLiteral(0, "0"));
                // CLR signed division/remainder both trap min / -1, including unchecked.
                if (kind == BinaryOperatorKind.Divide)
                    result = NormalizeIntegral(result, type, width.Bits >= 32 || isChecked);
                var parameter = new Identifier("$nonzero");
                var checks = new List<Statement>
                {
                    new IfStatement(new NonLogicalBinaryExpression(Operator.StrictEquality, parameter, zero), NumericThrow("DivideByZeroException"), null)
                };
                // Small compound targets compute in int first; their min / -1 is valid
                // arithmetic and only the subsequent checked narrowing can overflow.
                if (width.Signed && width.Bits >= 32)
                {
                    var minimum = -(BigInteger.One << (width.Bits - 1));
                    Expression min = width.Bits == 64 ? new BigIntLiteral(minimum, minimum + "n") : new NumericLiteral((double)minimum, minimum.ToString());
                    Expression minusOne = width.Bits == 64 ? new BigIntLiteral(BigInteger.MinusOne, "-1n") : new NumericLiteral(-1, "-1");
                    checks.Add(new IfStatement(new LogicalExpression(Operator.LogicalAnd,
                        new NonLogicalBinaryExpression(Operator.StrictEquality, values[0], min),
                        new NonLogicalBinaryExpression(Operator.StrictEquality, parameter, minusOne)), NumericThrow("OverflowException"), null));
                }
                return InvokeIntegralFunction(values[1], parameter,
                    new NestedBlockStatement(NodeList.From(checks)), result);
            });
        }
        Expression value = kind == BinaryOperatorKind.Multiply && !isChecked && width.Bits <= 32
            ? NumericCall("Math", "imul", left, right)
            : new NonLogicalBinaryExpression(CSharpBinaryOperators[kind], left, right);
        return NormalizeIntegral(value, type, isChecked);
    }

    private Expression? TryIntegralConversion(IConversionOperation operation, Expression value)
    {
        var target = operation.Type!;
        var width = IntegralWidth(target);
        if (width.Bits == 0 || operation.OperatorMethod is not null || operation.IsImplicit ||
            operation.Operand.Type?.SpecialType == target.SpecialType)
            return null;
        var source = operation.Operand.Type!;
        var sourceWidth = IntegralWidth(source);
        // Roslyn already proved literal conversions; avoid adding runtime normalization to
        // scalar constants and retain their original source-map/text representation.
        if (operation.ConstantValue.HasValue)
            return BuildValueLiteral(target, operation.ConstantValue.Value);
        if (sourceWidth.Bits == 0 && source.SpecialType is not (SpecialType.System_Single or SpecialType.System_Double))
            return null;
        if (source.SpecialType is SpecialType.System_Single or SpecialType.System_Double)
        {
            value = NumericCall("Math", "trunc", value);
            // Non-finite conversion is platform-dependent in unchecked CLR execution.
            // Refuse to turn NaN/infinity into an accidental JS bitwise zero.
            var finite = new Identifier("$finite");
            value = InvokeIntegralFunction(value, finite,
                new IfStatement(new NonUpdateUnaryExpression(Operator.LogicalNot, NumericCall("Number", "isFinite", finite)), NumericThrow("OverflowException"), null), finite);
        }
        if (width.Bits == 64 && sourceWidth.Bits != 64)
            value = new CallExpression(new Identifier("BigInt"), NodeList.From(value), false);
        else if (width.Bits != 64 && sourceWidth.Bits == 64)
        {
            // Check before converting to Number so a BigInt never loses precision first.
            if (operation.IsChecked)
            {
                var checkedBigInt = NormalizeBigIntNarrowing(value, target);
                return new CallExpression(new Identifier("Number"), NodeList.From(checkedBigInt), false);
            }
            value = NumericCall("BigInt", width.Signed ? "asIntN" : "asUintN", new NumericLiteral(width.Bits, width.Bits.ToString()), value);
            return new CallExpression(new Identifier("Number"), NodeList.From(value), false);
        }
        return NormalizeIntegral(value, target, operation.IsChecked);
    }

    private static Expression NormalizeBigIntNarrowing(Expression value, ITypeSymbol target)
    {
        var (bits, signed) = IntegralWidth(target);
        var parameter = new Identifier("$narrow");
        var minimum = signed ? -(BigInteger.One << (bits - 1)) : BigInteger.Zero;
        var maximum = (BigInteger.One << (signed ? bits - 1 : bits)) - 1;
        var outside = new LogicalExpression(Operator.LogicalOr,
            new NonLogicalBinaryExpression(Operator.LessThan, parameter, new BigIntLiteral(minimum, minimum + "n")),
            new NonLogicalBinaryExpression(Operator.GreaterThan, parameter, new BigIntLiteral(maximum, maximum + "n")));
        return InvokeIntegralFunction(value, parameter, new IfStatement(outside, NumericThrow("OverflowException"), null), parameter);
    }

}
