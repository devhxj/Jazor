using Acornima;
using Acornima.Ast;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Generic;

namespace Jazor.Compiler;

/// <summary>
/// 处理字符串字面量、插值字符串及字符串拼接相关 operation。
/// </summary>
/// <remarks>
/// 字符串 lowering 需要区分编译期可折叠值和运行时表达式；只有前者可以安全合并，
/// 运行时片段必须保持原始求值顺序和转换行为。
/// </remarks>
public partial class SemanticWalker
{
	/// <summary>
	/// 处理插值字符串文本操作
	/// C# 示例：
	/// $"Hello {name}, welcome!" 中的 "Hello " 和 ", welcome!" 部分
	/// 转换结果：字符串字面量 "Hello " / ", welcome!"
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitInterpolatedStringText(IInterpolatedStringTextOperation operation, SenseArgument argument)
	{
		// 插值字符串中的文本部分转换为字符串字面量
		var text = operation.Text.ConstantValue.Value?.ToString() ?? "";
		return CreateStringLiteral(text);
	}

	/// <summary>
	/// 处理插值表达式操作
	/// C# 示例：
	/// $"Hello {name}!" 中的 {name} 部分
	/// $"Value: {x + y:F2}" 中的 {x + y:F2} 部分
	/// 转换结果：返回插值表达式 name / (x + y)，并处理格式化
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitInterpolation(IInterpolationOperation operation, SenseArgument argument)
	{
		var value = Translate<Expression>(operation.Expression, argument);
		var valueType = GetInterpolationValueType(operation);
		if (valueType is null)
			return CreateStringLiteral("");

		// Nullable annotations are compile-time diagnostics, not a runtime non-null guarantee. Direct-value
		// formatting can use `?? ""` with one evaluation; only a conversion call needs a cached receiver.
		var requiresNullGuard = IsNullableType(valueType) || valueType.IsReferenceType;
		var requiresAlignment = operation.Alignment is not null;
		var runtimeType = UnwrapNullableInterpolationType(valueType);
		var useNullishTextFallback = requiresNullGuard &&
			UsesDirectInterpolationValue(operation, runtimeType);
		var expressions = new List<Expression>();
		Expression interpolationValue = value;
		if (requiresNullGuard && !useNullishTextFallback)
		{
			var valueIdentifier = new Identifier(AllocateUniqueName(operation, argument, LoweringSite.InterpolationValue()));
			argument.AddVarDeclarator(new VariableDeclarator(valueIdentifier, null), _recursionDepth);
			expressions.Add(new AssignmentExpression(Operator.Assignment, valueIdentifier, value));
			interpolationValue = valueIdentifier;
		}

		var formattedValue = BuildFormattedInterpolationValue(
			operation,
			runtimeType,
			interpolationValue,
			argument);
		if (useNullishTextFallback)
			formattedValue = BuildDirectInterpolationNullFallback(value);
		else if (requiresNullGuard)
		{
			formattedValue = new ConditionalExpression(
				new NonLogicalBinaryExpression(Operator.Equality, interpolationValue, Null),
				CreateStringLiteral(""),
				formattedValue);
		}

		if (!requiresAlignment)
		{
			if (expressions.Count == 0)
				return formattedValue;

			expressions.Add(formattedValue);
			return new SequenceExpression(NodeList.From(expressions));
		}

		// C# requires alignment to be an int constant. Roslyn has already validated the source, so
		// lowering can choose one padding contract at compile time without a runtime sign branch.
		expressions.Add(BuildInterpolationAlignment(operation, formattedValue, argument));
		return new SequenceExpression(NodeList.From(expressions));
	}

	private ITypeSymbol? GetInterpolationValueType(IInterpolationOperation operation)
	{
		if (operation.Expression.ConstantValue is { HasValue: true, Value: null })
			return null;

		return operation.Expression.Type ?? HandleTransformationFailure<ITypeSymbol>(
			operation,
			"Interpolated value is missing a bound static type.");
	}

	private Expression BuildFormattedInterpolationValue(
		IInterpolationOperation operation,
		ITypeSymbol valueType,
		Expression value,
		SenseArgument argument)
	{
		if (UsesDirectInterpolationValue(operation, valueType))
			return value;

		if (valueType.TypeKind == TypeKind.Enum)
			return HandleTransformationFailure<Expression>(
				operation,
				$"Interpolated enum value '{valueType.ToDisplayString(Jazor.Common.Format.NameFormat)}' requires an enum text runtime contract.");

		RejectUnsupportedInterpolationRuntimeType(operation, valueType);
		if (operation.FormatString is not null)
		{
			if (TryBuildNumericHexInterpolation(operation, valueType, value, argument, out var numericHex))
				return numericHex;

			var formattableMethod = FindFormattableToStringMethod(valueType);
			if (formattableMethod is not null)
			{
				EnsureInterpolationFormatContract(operation, formattableMethod);
				var format = Translate<Expression>(operation.FormatString, argument);
				return BuildMethodCallExpression(
					operation,
					formattableMethod,
					operation.Syntax,
					semanticModel: null,
					value,
					[format, Null],
					argument,
					hostType: valueType);
			}
		}

		var toStringMethod = FindParameterlessToStringMethod(valueType)
			?? HandleTransformationFailure<IMethodSymbol>(
				operation,
				$"Interpolated value type '{valueType.ToDisplayString(Jazor.Common.Format.NameFormat)}' does not expose a callable ToString() contract.");
		EnsureInterpolationToStringContract(operation, valueType, toStringMethod);
		return BuildMethodCallExpression(
			operation,
			toStringMethod,
			operation.Syntax,
			semanticModel: null,
			value,
			[],
			argument,
			hostType: valueType);
	}

	private bool TryBuildNumericHexInterpolation(
		IInterpolationOperation operation,
		ITypeSymbol valueType,
		Expression value,
		SenseArgument argument,
		out Expression expression)
	{
		expression = null!;
		if (valueType.SpecialType is not (SpecialType.System_Int32 or SpecialType.System_UInt32))
		{
			return false;
		}

		// The caller enters this helper only for a bound format specifier. C# interpolation format
		// text is syntax-owned and Roslyn exposes it as a non-null string constant.
		var format = (string)operation.FormatString!.ConstantValue.Value!;
		if (format is not ("X" or "x"))
			return false;

		// Integer X/x formatting is already compiler-owned by the one-argument ToString
		// intrinsic. Interpolation binds the IFormattable overload, so route the same format
		// through that intrinsic instead of requiring a duplicate CLR helper.
		var method = ((INamedTypeSymbol)valueType).GetMembers(nameof(ToString))
			.OfType<IMethodSymbol>()
			.Single(static candidate =>
				candidate.Parameters.Length == 1 &&
				candidate.Parameters[0].Type.SpecialType == SpecialType.System_String);
		var formatExpression = Translate<Expression>(operation.FormatString, argument);
		return TryBuildIntegerHexToStringIntrinsic(
			method,
			value,
			[formatExpression],
			out expression);
	}

	private static bool UsesDirectInterpolationValue(IInterpolationOperation operation, ITypeSymbol type)
	{
		if (type.SpecialType is SpecialType.System_String or SpecialType.System_Char)
			return true;

		if (type.TypeKind == TypeKind.Enum)
			return Util.IsStringEnumType(type) && operation.FormatString is null;

		return operation.FormatString is null && UsesTemplateLiteralScalarSemantics(type);
	}

	private static Expression BuildDirectInterpolationNullFallback(Expression value)
	{
		// A string literal on the right of `??` proves the C# coalesce expression already has text.
		if (value is LogicalExpression { Operator: Operator.NullishCoalescing, Right: StringLiteral })
			return value;

		return new LogicalExpression(
			Operator.NullishCoalescing,
			value,
			CreateStringLiteral(""));
	}

	private static bool UsesTemplateLiteralScalarSemantics(ITypeSymbol type)
	{
		if (IsSystemHalfType(type))
			return true;

		return type.SpecialType is
			SpecialType.System_SByte or
			SpecialType.System_Byte or
			SpecialType.System_Int16 or
			SpecialType.System_UInt16 or
			SpecialType.System_Int32 or
			SpecialType.System_UInt32 or
			SpecialType.System_Int64 or
			SpecialType.System_UInt64 or
			SpecialType.System_Single or
			SpecialType.System_Double or
			SpecialType.System_Decimal;
	}

	private Expression BuildInterpolationAlignment(
		IInterpolationOperation operation,
		Expression formattedValue,
		SenseArgument argument)
	{
		// A valid IInterpolationOperation is syntax-bound, and C# requires alignment to be an int
		// constant. Recovery operations are rejected by Roslyn before they enter this compiler.
		var stringType = operation.SemanticModel!.Compilation.GetSpecialType(SpecialType.System_String);
		var alignment = (int)operation.Alignment!.ConstantValue.Value!;

		if (alignment == 0)
			return formattedValue;

		var method = FindStringPaddingMethod(stringType, alignment < 0 ? "PadRight" : "PadLeft");
		var width = Math.Abs((long)alignment);
		return BuildMethodCallExpression(
			operation,
			method,
			operation.Syntax,
			semanticModel: null,
			formattedValue,
			[new NumericLiteral(width, width.ToString(System.Globalization.CultureInfo.InvariantCulture))],
			argument,
			hostType: stringType);
	}

	private static ITypeSymbol UnwrapNullableInterpolationType(ITypeSymbol type)
		=> type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable
			? nullable.TypeArguments[0]
			: type;

	private static IMethodSymbol? FindParameterlessToStringMethod(ITypeSymbol type)
	{
		for (var current = type as INamedTypeSymbol; current is not null; current = current.BaseType)
		{
			var method = current.GetMembers(nameof(ToString))
				.OfType<IMethodSymbol>()
				.SingleOrDefault(static candidate => !candidate.IsStatic && candidate.Parameters.Length == 0);
			if (method is not null)
				return method;
		}

		return null;
	}

	private static IMethodSymbol? FindFormattableToStringMethod(ITypeSymbol type)
	{
		if (type is not INamedTypeSymbol namedType)
			return null;

		var formattable = namedType.AllInterfaces.FirstOrDefault(static candidate =>
			IsSystemInterface(candidate, "IFormattable"));
		if (formattable is null)
			return null;

		var interfaceMethod = formattable.GetMembers(nameof(ToString))
			.OfType<IMethodSymbol>()
			.Single();
		return namedType.FindImplementationForInterfaceMember(interfaceMethod) as IMethodSymbol;
	}

	private void EnsureInterpolationFormatContract(IInterpolationOperation operation, IMethodSymbol method)
	{
		if (method.Locations.Any(static location => location.IsInSource))
			return;

		if (TryGetWhiteListValue(WhiteList.Members, method, out _, out var entry) && entry.Op != ECMAScript.Contract.Op.Discard)
			return;

		HandleTransformationFailure<Node>(
			operation,
			$"Interpolated format '{operation.FormatString?.Syntax}' requires a supported CLR mapping for '{method.OriginalDefinition.ToDisplayString(Jazor.Common.Format.NameFormat)}'.");
	}

	private void EnsureInterpolationToStringContract(
		IInterpolationOperation operation,
		ITypeSymbol valueType,
		IMethodSymbol method)
	{
		// A source class inherits JavaScript Object.prototype.toString(), whose result is not C#'s
		// default CLR type text. The contract must come from source (including a source base class).
		if (!valueType.Locations.Any(static location => location.IsInSource) ||
			method.Locations.Any(static location => location.IsInSource))
		{
			return;
		}

		HandleTransformationFailure<Node>(
			operation,
			$"Interpolated source value type '{valueType.ToDisplayString(Jazor.Common.Format.NameFormat)}' does not expose a stable string conversion contract. Override ToString() or use an explicit mapped text projection.");
	}

	private static bool IsSystemInterface(ITypeSymbol type, string name)
		=> type.TypeKind == TypeKind.Interface &&
			string.Equals(type.Name, name, StringComparison.Ordinal) &&
			string.Equals(type.ContainingNamespace?.ToDisplayString(), "System", StringComparison.Ordinal);

	private void RejectUnsupportedInterpolationRuntimeType(IInterpolationOperation operation, ITypeSymbol type)
	{
		if (type is ITypeParameterSymbol ||
			type.SpecialType == SpecialType.System_Object ||
			type.TypeKind is TypeKind.Interface or TypeKind.Delegate ||
			type.IsTupleType ||
			type.IsAnonymousType ||
			type is IArrayTypeSymbol)
		{
			HandleTransformationFailure<Node>(
				operation,
				$"Interpolated value type '{type.ToDisplayString(Jazor.Common.Format.NameFormat)}' does not expose a stable string conversion contract.");
		}
	}

	private static IMethodSymbol FindStringPaddingMethod(ITypeSymbol stringType, string name)
		=> stringType.GetMembers(name)
			.OfType<IMethodSymbol>()
			.Single(static candidate => candidate.Parameters.Length == 1);

	/// <summary>
	/// 处理插值字符串操作
	/// C# 示例：
	/// $"Hello, {name}!"           // 插值字符串
	/// $"Value: {x + y}"           // 包含表达式的插值字符串
	/// 转换结果：`Hello${name}!` / `Value: ${(x + y)}`
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitInterpolatedString(IInterpolatedStringOperation operation, SenseArgument argument)
	{
		var quasis = new List<TemplateElement>();
		var expressions = new List<Expression>();

		foreach (var part in operation.Parts)
		{
			switch (part)
			{
				case IInterpolatedStringTextOperation textOp:
					// 遇到文本，直接添加为 quasi
					var literal = textOp.Text as ILiteralOperation;
					var cooked = literal?.ConstantValue.Value as string ?? "";
					quasis.Add(new TemplateElement(
						TemplateValue.From(cooked, cooked),
						tail: false // tail 将在最后统一设置
					));
					break;

				case IInterpolationOperation interpOp:
					// 核心逻辑：在处理表达式前，确保它前面有一个 quasi。
					// 如果当前 quasi 数量不比 expression 多一个，说明前面是表达式或这是开头，需要补一个空的 quasi。
					if (quasis.Count == expressions.Count)
					{
						quasis.Add(new TemplateElement(
							TemplateValue.From("", ""),
							tail: false
						));
					}

					// 转换并添加表达式
					var expr = Visit(interpOp, argument) as Expression;
					if (expr is not null)
					{
						expressions.Add(expr);
					}
					break;
			}
		}

		// 循环结束后，处理尾部 quasi
		if (quasis.Count == expressions.Count)
		{
			// 如果数量相等，说明字符串以表达式结尾，需要补一个空的尾部 quasi。
			quasis.Add(new TemplateElement(TemplateValue.From("", ""), tail: true));
		}
		else if (quasis.Count > 0)
		{
			// 否则，字符串以文本结尾，将最后一个 quasi 标记为 tail。
			var lastQuasi = quasis[quasis.Count - 1];
			quasis[quasis.Count - 1] = new TemplateElement(lastQuasi.Value, tail: true);
		}

		// 优化：如果没有任何表达式，只有一个文本部分，返回更简单的 StringLiteral。
		if (expressions.Count == 0 && quasis.Count == 1)
		{
			var cookedValue = quasis[0].Value.Cooked ?? "";
			// 对于测试兼容性，确保返回带引号的字符串字面量
			return CreateStringLiteral(cookedValue);
		}

		// 返回结构完整的 TemplateLiteral
		return new TemplateLiteral(NodeList.From(quasis), NodeList.From(expressions));
	}	
}
