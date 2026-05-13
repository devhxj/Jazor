using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Descriptor;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue.RenderTree;

internal static class RazorVueForLoopAnalyzer
{
    internal readonly record struct AnalyzedForLoop(
        string VariableName,
        IOperation InitialValue,
        RazorVueForConditionKind ConditionKind,
        IOperation LimitValue,
        RazorVueForStepKind StepKind,
        IOperation? StepValue);

    public static AnalyzedForLoop AnalyzeRequired(
        IForLoopOperation operation,
        Func<IOperation?, IOperation?> unwrap,
        string ownerComponentFullName)
    {
        if (!TryAnalyze(operation, unwrap, out var analyzedLoop))
        {
            throw CreateUnsupportedForLoopException(operation, ownerComponentFullName);
        }

        ValidateStaticLoopProgressIfProvable(operation, analyzedLoop, unwrap, ownerComponentFullName);
        return analyzedLoop;
    }

    public static bool TryAnalyze(
        IForLoopOperation operation,
        Func<IOperation?, IOperation?> unwrap,
        out AnalyzedForLoop analyzedLoop)
    {
        analyzedLoop = default;
        if (operation.Before.Length != 1 ||
            operation.Before[0] is not IVariableDeclarationGroupOperation declarationGroup ||
            declarationGroup.Declarations.Length != 1)
        {
            return false;
        }

        var declaration = declarationGroup.Declarations[0];
        if (declaration.Declarators.Length != 1)
            return false;

        var declarator = declaration.Declarators[0];
        if (declarator.Symbol is null ||
            declarator.Initializer?.Value is null)
        {
            return false;
        }

        var condition = unwrap(operation.Condition);
        if (condition is not IBinaryOperation binaryCondition ||
            !TryMapForConditionKind(binaryCondition.OperatorKind, out var conditionKind))
        {
            return false;
        }

        if (!TryMatchLoopVariable(binaryCondition.LeftOperand, declarator.Symbol, unwrap) &&
            !TryMatchLoopVariable(binaryCondition.RightOperand, declarator.Symbol, unwrap))
        {
            return false;
        }

        var limitOperand = binaryCondition.RightOperand;
        if (TryMatchLoopVariable(binaryCondition.RightOperand, declarator.Symbol, unwrap))
        {
            conditionKind = InvertConditionKind(conditionKind);
            limitOperand = binaryCondition.LeftOperand;
        }

        if (operation.AtLoopBottom.Length != 1 ||
            !TryAnalyzeForStep(unwrap(operation.AtLoopBottom[0]), declarator.Symbol, unwrap, out var stepKind, out var stepValue))
        {
            return false;
        }

        analyzedLoop = new AnalyzedForLoop(
            declarator.Symbol.Name,
            unwrap(declarator.Initializer.Value) ?? declarator.Initializer.Value,
            conditionKind,
            unwrap(limitOperand) ?? limitOperand,
            stepKind,
            stepValue);
        return true;
    }

    public static RazorVueCompilationIssueException CreateUnsupportedForLoopException(
        IForLoopOperation loop,
        string ownerComponentFullName)
        => CreateForLoopIssueException(
            loop,
            ownerComponentFullName,
            $"RazorVue render currently only supports count-style for-loops with a single declared loop variable, direct comparison condition, and ++/--/+=/-= iterator in component '{ownerComponentFullName}'.");

    private static bool TryAnalyzeForStep(
        IOperation? operation,
        ILocalSymbol loopVariable,
        Func<IOperation?, IOperation?> unwrap,
        out RazorVueForStepKind stepKind,
        out IOperation? stepValue)
    {
        stepKind = default;
        stepValue = null;
        if (operation is null)
            return false;

        if (operation is IExpressionStatementOperation expressionStatement)
            operation = unwrap(expressionStatement.Operation);

        switch (operation)
        {
            case IIncrementOrDecrementOperation incrementOrDecrement
                when TryMatchLoopVariable(incrementOrDecrement.Target, loopVariable, unwrap):
                stepKind = incrementOrDecrement.Kind == OperationKind.Increment
                    ? RazorVueForStepKind.Increment
                    : incrementOrDecrement.Kind == OperationKind.Decrement
                        ? RazorVueForStepKind.Decrement
                        : default;
                return incrementOrDecrement.Kind is OperationKind.Increment or OperationKind.Decrement;

            case ICompoundAssignmentOperation compoundAssignment
                when TryMatchLoopVariable(compoundAssignment.Target, loopVariable, unwrap):
                if (compoundAssignment.OperatorKind == BinaryOperatorKind.Add)
                {
                    stepKind = RazorVueForStepKind.AddAssign;
                    stepValue = unwrap(compoundAssignment.Value) ?? compoundAssignment.Value;
                    return true;
                }

                if (compoundAssignment.OperatorKind == BinaryOperatorKind.Subtract)
                {
                    stepKind = RazorVueForStepKind.SubtractAssign;
                    stepValue = unwrap(compoundAssignment.Value) ?? compoundAssignment.Value;
                    return true;
                }

                return false;
        }

        return false;
    }

    private static bool TryMapForConditionKind(
        BinaryOperatorKind operatorKind,
        out RazorVueForConditionKind conditionKind)
    {
        conditionKind = operatorKind switch
        {
            BinaryOperatorKind.LessThan => RazorVueForConditionKind.LessThan,
            BinaryOperatorKind.LessThanOrEqual => RazorVueForConditionKind.LessThanOrEqual,
            BinaryOperatorKind.GreaterThan => RazorVueForConditionKind.GreaterThan,
            BinaryOperatorKind.GreaterThanOrEqual => RazorVueForConditionKind.GreaterThanOrEqual,
            _ => default
        };

        return operatorKind is BinaryOperatorKind.LessThan or
            BinaryOperatorKind.LessThanOrEqual or
            BinaryOperatorKind.GreaterThan or
            BinaryOperatorKind.GreaterThanOrEqual;
    }

    private static RazorVueForConditionKind InvertConditionKind(RazorVueForConditionKind conditionKind)
        => conditionKind switch
        {
            RazorVueForConditionKind.LessThan => RazorVueForConditionKind.GreaterThan,
            RazorVueForConditionKind.LessThanOrEqual => RazorVueForConditionKind.GreaterThanOrEqual,
            RazorVueForConditionKind.GreaterThan => RazorVueForConditionKind.LessThan,
            RazorVueForConditionKind.GreaterThanOrEqual => RazorVueForConditionKind.LessThanOrEqual,
            _ => throw new NotSupportedException($"Unsupported RazorVue for condition kind '{conditionKind}'.")
        };

    private static bool TryMatchLoopVariable(
        IOperation? operation,
        ILocalSymbol loopVariable,
        Func<IOperation?, IOperation?> unwrap)
    {
        var current = unwrap(operation);
        return current is ILocalReferenceOperation localReference &&
               SymbolEqualityComparer.Default.Equals(localReference.Local, loopVariable);
    }

    private static void ValidateStaticLoopProgressIfProvable(
        IForLoopOperation loop,
        AnalyzedForLoop analyzedLoop,
        Func<IOperation?, IOperation?> unwrap,
        string ownerComponentFullName)
    {
        if (!TryGetNumericConstant(analyzedLoop.InitialValue, unwrap, out var initialValue) ||
            !TryGetNumericConstant(analyzedLoop.LimitValue, unwrap, out var limitValue) ||
            !TryGetConstantSignedStep(analyzedLoop, unwrap, out var signedStep))
        {
            return;
        }

        if (!EvaluateCondition(initialValue, limitValue, analyzedLoop.ConditionKind))
            return;

        var stepDirection = CompareToZero(signedStep);
        if (stepDirection == 0)
        {
            throw CreateForLoopIssueException(
                loop,
                ownerComponentFullName,
                $"RazorVue render does not support count-style for-loops whose statically known iterator step becomes zero after the loop has entered in component '{ownerComponentFullName}'.");
        }

        var expectsPositiveProgress = analyzedLoop.ConditionKind is RazorVueForConditionKind.LessThan or RazorVueForConditionKind.LessThanOrEqual;
        if ((expectsPositiveProgress && stepDirection < 0) ||
            (!expectsPositiveProgress && stepDirection > 0))
        {
            throw CreateForLoopIssueException(
                loop,
                ownerComponentFullName,
                $"RazorVue render does not support count-style for-loops whose statically known iterator step moves away from the loop limit after the loop has entered in component '{ownerComponentFullName}'.");
        }
    }

    private static bool TryGetConstantSignedStep(
        AnalyzedForLoop analyzedLoop,
        Func<IOperation?, IOperation?> unwrap,
        out NumericConstant signedStep)
    {
        switch (analyzedLoop.StepKind)
        {
            case RazorVueForStepKind.Increment:
                signedStep = NumericConstant.CreateDecimal(1m);
                return true;
            case RazorVueForStepKind.Decrement:
                signedStep = NumericConstant.CreateDecimal(-1m);
                return true;
            case RazorVueForStepKind.AddAssign:
                return TryGetNumericConstant(analyzedLoop.StepValue, unwrap, out signedStep);
            case RazorVueForStepKind.SubtractAssign:
                if (TryGetNumericConstant(analyzedLoop.StepValue, unwrap, out var rawStep))
                {
                    signedStep = Negate(rawStep);
                    return true;
                }

                break;
        }

        signedStep = default;
        return false;
    }

    private static bool TryGetNumericConstant(
        IOperation? operation,
        Func<IOperation?, IOperation?> unwrap,
        out NumericConstant value)
    {
        value = default;
        var current = unwrap(operation);
        if (current?.ConstantValue.HasValue != true ||
            current.ConstantValue.Value is null)
        {
            return false;
        }

        switch (current.ConstantValue.Value)
        {
            case sbyte signedByte:
                value = NumericConstant.CreateDecimal(signedByte);
                return true;
            case byte unsignedByte:
                value = NumericConstant.CreateDecimal(unsignedByte);
                return true;
            case short signedShort:
                value = NumericConstant.CreateDecimal(signedShort);
                return true;
            case ushort unsignedShort:
                value = NumericConstant.CreateDecimal(unsignedShort);
                return true;
            case int signedInt:
                value = NumericConstant.CreateDecimal(signedInt);
                return true;
            case uint unsignedInt:
                value = NumericConstant.CreateDecimal(unsignedInt);
                return true;
            case long signedLong:
                value = NumericConstant.CreateDecimal(signedLong);
                return true;
            case ulong unsignedLong:
                value = NumericConstant.CreateDecimal(unsignedLong);
                return true;
            case char character:
                value = NumericConstant.CreateDecimal(character);
                return true;
            case nint nativeInt:
                value = NumericConstant.CreateDecimal(nativeInt);
                return true;
            case nuint nativeUInt:
                value = NumericConstant.CreateDecimal(nativeUInt);
                return true;
            case decimal decimalValue:
                value = NumericConstant.CreateDecimal(decimalValue);
                return true;
            case float singleValue when !float.IsNaN(singleValue) && !float.IsInfinity(singleValue):
                value = NumericConstant.CreateDouble(singleValue);
                return true;
            case double doubleValue when !double.IsNaN(doubleValue) && !double.IsInfinity(doubleValue):
                value = NumericConstant.CreateDouble(doubleValue);
                return true;
            default:
                return false;
        }
    }

    private static NumericConstant Negate(NumericConstant value)
        => value.Kind == NumericConstantKind.Double
            ? NumericConstant.CreateDouble(-value.DoubleValue)
            : NumericConstant.CreateDecimal(-value.DecimalValue);

    private static bool EvaluateCondition(
        NumericConstant initialValue,
        NumericConstant limitValue,
        RazorVueForConditionKind conditionKind)
    {
        var comparison = Compare(initialValue, limitValue);
        return conditionKind switch
        {
            RazorVueForConditionKind.LessThan => comparison < 0,
            RazorVueForConditionKind.LessThanOrEqual => comparison <= 0,
            RazorVueForConditionKind.GreaterThan => comparison > 0,
            RazorVueForConditionKind.GreaterThanOrEqual => comparison >= 0,
            _ => throw new NotSupportedException($"Unsupported RazorVue for condition kind '{conditionKind}'.")
        };
    }

    private static int CompareToZero(NumericConstant value)
        => value.Kind == NumericConstantKind.Double
            ? value.DoubleValue.CompareTo(0d)
            : value.DecimalValue.CompareTo(0m);

    private static int Compare(NumericConstant left, NumericConstant right)
    {
        if (left.Kind == NumericConstantKind.Decimal &&
            right.Kind == NumericConstantKind.Decimal)
        {
            return left.DecimalValue.CompareTo(right.DecimalValue);
        }

        return ConvertToDouble(left).CompareTo(ConvertToDouble(right));
    }

    private static double ConvertToDouble(NumericConstant value)
        => value.Kind == NumericConstantKind.Double
            ? value.DoubleValue
            : (double)value.DecimalValue;

    private static RazorVueCompilationIssueException CreateForLoopIssueException(
        IForLoopOperation loop,
        string ownerComponentFullName,
        string message)
    {
        var issue = new RazorVueCompilationIssue(
            RazorVueIssueCode.CanonicalizationFailed,
            RazorVueIssueSeverity.Error,
            message,
			[]);
        return new RazorVueCompilationIssueException(
            issue,
            ownerComponentFullName,
            loop.Syntax is null ? null : RazorVueSourceOrigin.FromLocation(loop.Syntax.GetLocation(), RazorVueOriginKind.Template));
    }

    private readonly record struct NumericConstant(
        NumericConstantKind Kind,
        decimal DecimalValue,
        double DoubleValue)
    {
        public static NumericConstant CreateDecimal(decimal value)
            => new(NumericConstantKind.Decimal, value, default);

        public static NumericConstant CreateDouble(double value)
            => new(NumericConstantKind.Double, default, value);
    }

    private enum NumericConstantKind
    {
        Decimal,
        Double
    }
}
