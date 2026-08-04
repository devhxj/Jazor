using Acornima;
using Acornima.Ast;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.Compiler;

public partial class SemanticWalker
{
	/// <summary>
	/// Lowers the array-shaped LINQ slice with compiler-owned source-shape and ownership analysis.
	/// </summary>
	/// <remarks>
	/// Where/Select must normalize non-list IEnumerable carriers before calling Array methods, while
	/// ToArray/ToList may reuse a result that is already known to be a fresh Array. This information
	/// exists only at the bound usage site, so the formal Compile hook owns it instead of an Import.
	/// </remarks>
	public Expression? CompileEnumerableArrayLike(
		ISymbol symbol,
		SenseArgument context,
		Expression? handler,
		Expression?[] args,
		IOperation? originOperation)
	{
		var method = (IMethodSymbol)symbol;
		var arguments = new List<Expression>(args.Length);
		foreach (var argumentExpression in args)
			arguments.Add(argumentExpression!);

		var sourceType = originOperation is IInvocationOperation invocation
			? UnwrapImplicitConversions(invocation.Arguments[0].Value).Type
			: method.Parameters[0].Type;
		if (TryBuildEnumerableArrayLikeIntrinsic(method, arguments, sourceType, context, out var loweredExpression))
			return loweredExpression;

		return HandleTransformationFailure<Expression>(
			originOperation!,
			$"Enumerable member '{method.OriginalDefinition.ToDisplayString(Jazor.Common.Format.NameFormat)}' does not match the array-like Compile protocol.");
	}

	/// <summary>
	/// Lowers LINQ default-returning overloads through their mapped overload that accepts an
	/// explicit fallback value.
	/// </summary>
	/// <remarks>
	/// JavaScript erases generic arguments, while C# selects <c>default(TSource)</c> from the
	/// closed call-site type. Keeping this decision here preserves the normal default-value
	/// support boundary and lets runtime helpers remain ordinary enumerable operations.
	/// </remarks>
	public Expression? CompileEnumerableDefaultIfEmpty(
		ISymbol symbol,
		SenseArgument context,
		Expression? handler,
		Expression?[] args,
		IOperation? originOperation)
		=> CompileEnumerableDefaultValueOverload(symbol, context, args, originOperation);

	public Expression? CompileEnumerableFirstOrDefault(
		ISymbol symbol,
		SenseArgument context,
		Expression? handler,
		Expression?[] args,
		IOperation? originOperation)
		=> CompileEnumerableDefaultValueOverload(symbol, context, args, originOperation);

	public Expression? CompileEnumerableLastOrDefault(
		ISymbol symbol,
		SenseArgument context,
		Expression? handler,
		Expression?[] args,
		IOperation? originOperation)
		=> CompileEnumerableDefaultValueOverload(symbol, context, args, originOperation);

	public Expression? CompileEnumerableSingleOrDefault(
		ISymbol symbol,
		SenseArgument context,
		Expression? handler,
		Expression?[] args,
		IOperation? originOperation)
		=> CompileEnumerableDefaultValueOverload(symbol, context, args, originOperation);

	/// <summary>
	/// Lowers <c>Enumerable.ElementAtOrDefault</c> without inventing a CLR fallback overload.
	/// Unlike the other default-returning terminal operators, the BCL has no public
	/// explicit-default form, so this Compile mapping owns the closed default and the iteration
	/// protocol directly as ESTree.
	/// </summary>
	/// <remarks>
	/// The IIFE evaluates source then index exactly once before enumeration. Returning from a
	/// JavaScript <c>for...of</c> closes an iterator, which preserves the observable early-stop
	/// behavior of the int and from-start Index paths. A from-end Index observes the complete
	/// source and retains only the requested tail in a bounded ring buffer.
	/// </remarks>
	public Expression? CompileEnumerableElementAtOrDefault(
		ISymbol symbol,
		SenseArgument context,
		Expression? handler,
		Expression?[] args,
		IOperation? originOperation)
	{
		// Compile dispatch is keyed by the exact static BCL overload, and its only consumers are
		// bound invocations and method references. Revalidating that closed shape here would create
		// unreachable guard branches without strengthening the authored C# contract.
		var method = (IMethodSymbol)symbol;
		var defaultValue = BuildDefaultValueExpression(originOperation!, method.TypeArguments[0], context);
		return method.Parameters[1].Type.SpecialType == SpecialType.System_Int32
			? BuildEnumerableElementAtOrDefaultInt(args, defaultValue)
			: BuildEnumerableElementAtOrDefaultIndex(method, context, args, defaultValue, originOperation!);
	}

	/// <summary>
	/// Lowers the mapped <c>Enumerable.Zip</c> overloads through the JavaScript iterator protocol.
	/// </summary>
	/// <remarks>
	/// Explicit <c>IEnumerator&lt;T&gt;</c> calls deliberately remain outside the public CLR mapping
	/// surface. Zip is nevertheless expressible as a self-contained protocol: create iterators in
	/// source order, advance them in that same order, and close them in reverse construction order.
	/// Keeping that protocol in this compiler-owned ESTree hook prevents an Array-only shortcut
	/// from changing generator order, early termination, or disposal visibility.
	/// </remarks>
	public Expression? CompileEnumerableZip(
		ISymbol symbol,
		SenseArgument context,
		Expression? handler,
		Expression?[] args,
		IOperation? originOperation)
	{
		// Generated Compile dispatch is keyed by the two exact static BCL overloads. Invocation and
		// method-reference lowering both pass the already bound arguments, so revalidating those
		// closed shapes here would only add unreachable branches.
		var method = (IMethodSymbol)symbol;
		return BuildEnumerableZipInvocation(method, args);
	}

	/// <summary>
	/// Lowers <c>Enumerable.Cast&lt;TResult&gt;</c> through a compiler-owned type protocol.
	/// </summary>
	/// <remarks>
	/// The non-generic <c>IEnumerable</c> input has no element type left at runtime. A generic
	/// JavaScript <c>typeof</c> fallback would therefore confuse numeric widths, enums, char and
	/// string. Reuse the conservative pattern discriminator only when the target carrier is
	/// distinguishable; otherwise fail at the bound source location rather than alter CLR results.
	/// </remarks>
	public Expression? CompileEnumerableCast(
		ISymbol symbol,
		SenseArgument context,
		Expression? handler,
		Expression?[] args,
		IOperation? originOperation)
		=> CompileEnumerableTypeFilter(symbol, context, args, originOperation, ofType: false);

	/// <summary>
	/// Lowers <c>Enumerable.OfType&lt;TResult&gt;</c> through the same carrier discriminator as
	/// <see cref="CompileEnumerableCast"/> while retaining only matching non-null values.
	/// </summary>
	public Expression? CompileEnumerableOfType(
		ISymbol symbol,
		SenseArgument context,
		Expression? handler,
		Expression?[] args,
		IOperation? originOperation)
		=> CompileEnumerableTypeFilter(symbol, context, args, originOperation, ofType: true);

	private Expression CompileEnumerableTypeFilter(
		ISymbol symbol,
		SenseArgument context,
		Expression?[] args,
		IOperation? originOperation,
		bool ofType)
	{
		var method = (IMethodSymbol)symbol;
		if (originOperation is IInvocationOperation)
			return BuildEnumerableTypeFilterInvocation(method, context, args[0]!, originOperation, ofType);

		// VisitMethodReference has already created the source-bound delegate proxy. Wrapping it in
		// another lambda would make Enumerable.OfType<T> return a function instead of a sequence.
		return BuildEnumerableTypeFilterInvocation(method, context, args[0]!, originOperation!, ofType);
	}

	private Expression BuildEnumerableTypeFilterInvocation(
		IMethodSymbol method,
		SenseArgument context,
		Expression sourceArgument,
		IOperation originOperation,
		bool ofType)
	{
		var source = new Identifier("__enumerableTypeFilterSource");
		var item = new Identifier("__enumerableTypeFilterItem");
		var result = new Identifier("__enumerableTypeFilterResult");
		var targetType = method.TypeArguments[0];
		var match = BuildEnumerableElementTypeMatch(originOperation, targetType, item, context);
		var append = new NonSpecialExpressionStatement(
			new CallExpression(
				new MemberExpression(result, new Identifier("push"), computed: false, optional: false),
				NodeList.From<Expression>(item),
				optional: false));
		var isNull = new NonLogicalBinaryExpression(Operator.Equality, item, Null);
		var isNonNull = new NonLogicalBinaryExpression(Operator.Inequality, item, Null);
		var loopBody = new List<Statement>();

		if (ofType)
		{
			loopBody.Add(new IfStatement(
				new LogicalExpression(Operator.LogicalAnd, isNonNull, match),
				append,
				null));
		}
		else
		{
			var invalidCast = BuildEnumerableInvalidCastThrowStatement(targetType);
			if (!targetType.IsReferenceType && !IsNullableType(targetType))
				loopBody.Add(new IfStatement(isNull, invalidCast, null));

			loopBody.Add(new IfStatement(
				new LogicalExpression(
					Operator.LogicalAnd,
					isNonNull,
					new NonUpdateUnaryExpression(Operator.LogicalNot, match)),
				invalidCast,
				null));
			loopBody.Add(append);
		}

		var statements = new List<Statement>
		{
			new IfStatement(
				new NonLogicalBinaryExpression(Operator.Equality, source, Null),
				BuildArgumentNullThrowStatement("source"),
				null),
			new VariableDeclaration(
				VariableDeclarationKind.Const,
				NodeList.From(new VariableDeclarator(
					result,
					new ArrayExpression(NodeList.Empty<Expression?>())))),
			new ForOfStatement(
				new VariableDeclaration(VariableDeclarationKind.Let, NodeList.From(new VariableDeclarator(item, null))),
				source,
				new NestedBlockStatement(NodeList.From(loopBody)),
				@await: false),
			new ReturnStatement(result)
		};
		var iife = new ArrowFunctionExpression(
			NodeList.From<Node>(source),
			new FunctionBody(NodeList.From(statements), strict: true),
			expression: false,
			async: false);
		return new CallExpression(iife, NodeList.From(sourceArgument), optional: false);
	}

	private Expression BuildEnumerableElementTypeMatch(
		IOperation originOperation,
		ITypeSymbol targetType,
		Expression item,
		SenseArgument context)
	{
		if (targetType.SpecialType == SpecialType.System_Object)
			return new BooleanLiteral(true, "true");

		if (targetType.TypeKind == TypeKind.Interface || targetType is ITypeParameterSymbol)
		{
			return HandleTransformationFailure<Expression>(
				originOperation,
				$"Enumerable type filtering cannot discriminate erased target '{targetType.OriginalDefinition.ToDisplayString(Jazor.Common.Format.NameFormat)}'. Use a runtime-distinguishable concrete carrier or filter in authored C# before this boundary.");
		}

		if (targetType.SpecialType == SpecialType.System_Char ||
			targetType.SpecialType == SpecialType.System_String)
		{
			return HandleTransformationFailure<Expression>(
				originOperation,
				$"Enumerable type filtering cannot distinguish '{targetType.OriginalDefinition.ToDisplayString(Jazor.Common.Format.NameFormat)}' from the shared JavaScript string carrier.");
		}

		var mapper = GetMapperType(targetType).Mapper;
		if (mapper == TypeMapper.Number || targetType.TypeKind == TypeKind.Enum)
		{
			return HandleTransformationFailure<Expression>(
				originOperation,
				$"Enumerable type filtering cannot distinguish '{targetType.OriginalDefinition.ToDisplayString(Jazor.Common.Format.NameFormat)}' from other numeric or enum CLR values on the JavaScript Number carrier.");
		}

		if (mapper is not (
			TypeMapper.Boolean or
			TypeMapper.BigInt or
			TypeMapper.Date or
			TypeMapper.Map or
			TypeMapper.Set or
			TypeMapper.Array or
			TypeMapper.Class))
		{
			return HandleTransformationFailure<Expression>(
				originOperation,
				$"Enumerable type filtering does not have a stable runtime discriminator for '{targetType.OriginalDefinition.ToDisplayString(Jazor.Common.Format.NameFormat)}'.");
		}

		return CreateTypeMatchExpr(originOperation, targetType, item, context);
	}

	private static ThrowStatement BuildEnumerableInvalidCastThrowStatement(ITypeSymbol targetType)
		=> new(
			new NewExpression(
				new Identifier("Error"),
				NodeList.From<Expression>(CreateStringLiteral(
					$"InvalidCastException: element cannot be cast to {targetType.OriginalDefinition.ToDisplayString(Jazor.Common.Format.NameFormat)}."))));

	private static Expression BuildEnumerableZipInvocation(IMethodSymbol method, Expression?[] args)
	{
		var sources = new[]
		{
			new Identifier("__enumerableZipFirst"),
			new Identifier("__enumerableZipSecond"),
			new Identifier("__enumerableZipThird")
		};
		var sourceNames = new[] { "first", "second", "third" };
		var iterators = new[]
		{
			new Identifier("__enumerableZipFirstIterator"),
			new Identifier("__enumerableZipSecondIterator"),
			new Identifier("__enumerableZipThirdIterator")
		};
		var steps = new[]
		{
			new Identifier("__enumerableZipFirstStep"),
			new Identifier("__enumerableZipSecondStep"),
			new Identifier("__enumerableZipThirdStep")
		};
		var closeNames = new[]
		{
			"__enumerableZipFirstClose",
			"__enumerableZipSecondClose",
			"__enumerableZipThirdClose"
		};
		var hasSelector = method.Parameters.Length == 3 &&
			method.Parameters[2].Type.TypeKind == TypeKind.Delegate;
		var sourceCount = hasSelector ? method.Parameters.Length - 1 : method.Parameters.Length;
		var selector = hasSelector
			? new Identifier("__enumerableZipSelector")
			: null;
		var result = new Identifier("__enumerableZipResult");

		var statements = new List<Statement>();
		for (var index = 0; index < sourceCount; index++)
		{
			statements.Add(new IfStatement(
				new NonLogicalBinaryExpression(Operator.Equality, sources[index], Null),
				BuildArgumentNullThrowStatement(sourceNames[index]),
				null));
		}
		if (selector is not null)
		{
			statements.Add(new IfStatement(
				new NonLogicalBinaryExpression(Operator.Equality, selector, Null),
				BuildArgumentNullThrowStatement("resultSelector"),
				null));
		}

		statements.Add(new VariableDeclaration(
			VariableDeclarationKind.Const,
			NodeList.From(new VariableDeclarator(
				result,
				new ArrayExpression(NodeList.Empty<Expression?>())))));
		for (var index = 0; index < sourceCount; index++)
		{
			statements.Add(new VariableDeclaration(
				VariableDeclarationKind.Const,
				NodeList.From(new VariableDeclarator(iterators[index], BuildEnumerableIterator(sources[index])))));
		}

		var loopStatements = new List<Statement>();
		for (var index = 0; index < sourceCount; index++)
		{
			loopStatements.Add(new VariableDeclaration(
				VariableDeclarationKind.Const,
				NodeList.From(new VariableDeclarator(steps[index], BuildEnumerableIteratorNext(iterators[index])))));
			loopStatements.Add(new IfStatement(
				BuildEnumerableIteratorDone(steps[index]),
				new ReturnStatement(result),
				null));
		}
		loopStatements.Add(new NonSpecialExpressionStatement(
			BuildEnumerableZipResultAppend(method, result, steps.Take(sourceCount).ToArray(), selector)));
		var loop = new WhileStatement(
			new BooleanLiteral(true, "true"),
			new NestedBlockStatement(NodeList.From(loopStatements)));
		var finalizerStatements = new List<Statement>();
		for (var index = sourceCount - 1; index >= 0; index--)
			finalizerStatements.Add(BuildEnumerableIteratorClose(iterators[index], closeNames[index]));
		var finalizer = new NestedBlockStatement(NodeList.From(finalizerStatements));
		statements.Add(new TryStatement(
			new NestedBlockStatement(NodeList.From<Statement>(loop)),
			handler: null,
			finalizer));
		// Every normal loop exit returns inside try. Keep the function structurally total in case a
		// future iterator protocol extension adds a non-returning loop exit.
		statements.Add(new ReturnStatement(result));

		var parameters = sources.Take(sourceCount).Cast<Node>().ToList();
		if (selector is not null)
			parameters.Add(selector);
		var iife = new ArrowFunctionExpression(
			NodeList.From(parameters),
			new FunctionBody(NodeList.From(statements), strict: true),
			expression: false,
			async: false);
		var invocationArguments = args.Select(static argument => argument!).ToArray();
		return new CallExpression(iife, NodeList.From(invocationArguments), optional: false);
	}

	private static Expression BuildEnumerableIterator(Identifier enumerable)
	{
		var iterator = new MemberExpression(
			new Identifier("Symbol"),
			new Identifier("iterator"),
			computed: false,
			optional: false);
		return new CallExpression(
			new MemberExpression(enumerable, iterator, computed: true, optional: false),
			NodeList.Empty<Expression>(),
			optional: false);
	}

	private static Expression BuildEnumerableIteratorNext(Identifier iterator)
		=> new CallExpression(
			new MemberExpression(iterator, new Identifier("next"), computed: false, optional: false),
			NodeList.Empty<Expression>(),
			optional: false);

	private static Expression BuildEnumerableIteratorDone(Identifier step)
		=> new MemberExpression(step, new Identifier("done"), computed: false, optional: false);

	private static Statement BuildEnumerableIteratorClose(Identifier iterator, string closeName)
	{
		var close = new Identifier(closeName);
		var closeAccessor = new MemberExpression(iterator, new Identifier("return"), computed: false, optional: false);
		var closeCall = new CallExpression(
			new MemberExpression(close, new Identifier("call"), computed: false, optional: false),
			NodeList.From<Expression>(iterator),
			optional: false);
		return new NestedBlockStatement(NodeList.From<Statement>(
			new VariableDeclaration(
				VariableDeclarationKind.Const,
				NodeList.From(new VariableDeclarator(close, closeAccessor))),
			new IfStatement(
				new NonLogicalBinaryExpression(Operator.Inequality, close, Null),
				new NonSpecialExpressionStatement(closeCall),
				null)));
	}

	private static Expression BuildEnumerableZipResultAppend(
		IMethodSymbol method,
		Identifier result,
		Identifier[] steps,
		Identifier? selector)
	{
		var values = steps.Select(static step =>
			(Expression)new MemberExpression(step, new Identifier("value"), computed: false, optional: false)).ToArray();
		Expression item;
		if (selector is not null)
		{
			item = new CallExpression(selector, NodeList.From(values), optional: false);
		}
		else
		{
			var enumerableReturn = (INamedTypeSymbol)method.ReturnType;
			var tupleType = (INamedTypeSymbol)enumerableReturn.TypeArguments[0];
			var properties = new List<Node>();
			for (var index = 0; index < values.Length; index++)
			{
				properties.Add(new ObjectProperty(
					PropertyKind.Init,
					new Identifier(GetTupleRuntimeFieldName(tupleType.TupleElements[index])),
					values[index],
					computed: false,
					shorthand: false,
					method: false));
			}
			item = new ObjectExpression(NodeList.From(properties));
		}

		return new CallExpression(
			new MemberExpression(result, new Identifier("push"), computed: false, optional: false),
			NodeList.From<Expression>(item),
			optional: false);
	}

	private static Expression BuildEnumerableElementAtOrDefaultInt(
		Expression?[] args,
		Expression defaultValue)
	{
		var sourceParameter = new Identifier("__enumerableSource");
		var indexParameter = new Identifier("__enumerableIndex");
		var statements = new List<Statement>
		{
			new IfStatement(
				new NonLogicalBinaryExpression(Operator.Equality, sourceParameter, Null),
				BuildArgumentNullThrowStatement("source"),
				null),
			new IfStatement(
				new NonLogicalBinaryExpression(
					Operator.LessThan,
					indexParameter,
					new NumericLiteral(0, "0")),
				new ReturnStatement(defaultValue),
				null)
		};
		statements.AddRange(BuildEnumerableElementAtFromStartStatements(
			sourceParameter,
			indexParameter,
			defaultValue,
			"__enumerableCurrentIndex",
			"__enumerableItem"));

		return BuildEnumerableElementAtIife(sourceParameter, indexParameter, statements, args);
	}

	private Expression BuildEnumerableElementAtOrDefaultIndex(
		IMethodSymbol method,
		SenseArgument context,
		Expression?[] args,
		Expression defaultValue,
		IOperation originOperation)
	{
		var indexType = method.Parameters[1].Type;
		var isFromEndProperty = indexType.GetMembers("IsFromEnd")
			.OfType<IPropertySymbol>()
			.Single();
		var valueProperty = indexType.GetMembers("Value")
			.OfType<IPropertySymbol>()
			.Single();
		var sourceParameter = new Identifier("__enumerableSource");
		var indexParameter = new Identifier("__enumerableIndex");
		var fromEnd = new Identifier("__enumerableFromEnd");
		var indexValue = new Identifier("__enumerableIndexValue");
		var currentIndex = new Identifier("__enumerableTailIndex");
		var tail = new Identifier("__enumerableTail");
		var tailItem = new Identifier("__enumerableTailItem");
		var mappedIsFromEnd = GetWhiteListExpression(
			isFromEndProperty.GetMethod!,
			context,
			[],
			indexParameter,
			out _,
			originOperation)!;
		var mappedValue = GetWhiteListExpression(
			valueProperty.GetMethod!,
			context,
			[],
			indexParameter,
			out _,
			originOperation)!;

		var fromStartStatements = BuildEnumerableElementAtFromStartStatements(
			sourceParameter,
			indexValue,
			defaultValue,
			"__enumerableCurrentIndex",
			"__enumerableItem");
		var tailLength = new MemberExpression(
			tail,
			new Identifier("length"),
			computed: false,
			optional: false);
		var pushTailItem = new CallExpression(
			new MemberExpression(tail, new Identifier("push"), computed: false, optional: false),
			NodeList.From<Expression>(tailItem),
			optional: false);
		var replaceOldestItem = new AssignmentExpression(
			Operator.Assignment,
			new MemberExpression(tail, currentIndex, computed: true, optional: false),
			tailItem);
		var advanceTailIndex = new AssignmentExpression(
			Operator.Assignment,
			currentIndex,
			new NonLogicalBinaryExpression(
				Operator.Remainder,
				new NonLogicalBinaryExpression(
					Operator.Addition,
					currentIndex,
					new NumericLiteral(1, "1")),
				indexValue));
		var tailLoopBody = new NestedBlockStatement(NodeList.From<Statement>(
			new IfStatement(
				new NonLogicalBinaryExpression(Operator.LessThan, tailLength, indexValue),
				new NonSpecialExpressionStatement(pushTailItem),
				new NestedBlockStatement(NodeList.From<Statement>(
					new NonSpecialExpressionStatement(replaceOldestItem),
					new NonSpecialExpressionStatement(advanceTailIndex))))));
		var tailLoopBinding = new VariableDeclaration(
			VariableDeclarationKind.Let,
			NodeList.From(new VariableDeclarator(tailItem, null)));

		var statements = new List<Statement>
		{
			new IfStatement(
				new NonLogicalBinaryExpression(Operator.Equality, sourceParameter, Null),
				BuildArgumentNullThrowStatement("source"),
				null),
			new VariableDeclaration(
				VariableDeclarationKind.Const,
				NodeList.From(new VariableDeclarator(fromEnd, mappedIsFromEnd))),
			new VariableDeclaration(
				VariableDeclarationKind.Const,
				NodeList.From(new VariableDeclarator(indexValue, mappedValue))),
			new IfStatement(
				new NonUpdateUnaryExpression(Operator.LogicalNot, fromEnd),
				new NestedBlockStatement(NodeList.From(fromStartStatements)),
				null),
			// Index.End (^0) is never a valid element and does not enumerate an unknown source.
			new IfStatement(
				new NonLogicalBinaryExpression(
					Operator.StrictEquality,
					indexValue,
					new NumericLiteral(0, "0")),
				new ReturnStatement(defaultValue),
				null),
			new VariableDeclaration(
				VariableDeclarationKind.Let,
				NodeList.From(new VariableDeclarator(
					tail,
					new ArrayExpression(NodeList.Empty<Expression?>())))),
			new VariableDeclaration(
				VariableDeclarationKind.Let,
				NodeList.From(new VariableDeclarator(currentIndex, new NumericLiteral(0, "0")))),
			new ForOfStatement(tailLoopBinding, sourceParameter, tailLoopBody, @await: false),
			new IfStatement(
				new NonLogicalBinaryExpression(Operator.LessThan, tailLength, indexValue),
				new ReturnStatement(defaultValue),
				null),
			new ReturnStatement(new MemberExpression(tail, currentIndex, computed: true, optional: false))
		};

		return BuildEnumerableElementAtIife(sourceParameter, indexParameter, statements, args);
	}

	private static IReadOnlyList<Statement> BuildEnumerableElementAtFromStartStatements(
		Expression source,
		Expression index,
		Expression defaultValue,
		string currentIndexName,
		string itemName)
	{
		var currentIndex = new Identifier(currentIndexName);
		var item = new Identifier(itemName);
		var loopBody = new NestedBlockStatement(NodeList.From<Statement>(
			new IfStatement(
				new NonLogicalBinaryExpression(Operator.StrictEquality, currentIndex, index),
				new ReturnStatement(item),
				null),
			new NonSpecialExpressionStatement(
				new UpdateExpression(Operator.Increment, currentIndex, prefix: false))));
		var loopBinding = new VariableDeclaration(
			VariableDeclarationKind.Let,
			NodeList.From(new VariableDeclarator(item, null)));
		return
		[
			new VariableDeclaration(
				VariableDeclarationKind.Let,
				NodeList.From(new VariableDeclarator(currentIndex, new NumericLiteral(0, "0")))),
			new ForOfStatement(loopBinding, source, loopBody, @await: false),
			new ReturnStatement(defaultValue)
		];
	}

	private static Expression BuildEnumerableElementAtIife(
		Identifier sourceParameter,
		Identifier indexParameter,
		IReadOnlyList<Statement> statements,
		Expression?[] args)
	{
		var body = new FunctionBody(NodeList.From(statements), strict: true);
		var iife = new ArrowFunctionExpression(
			NodeList.From<Node>(sourceParameter, indexParameter),
			body,
			expression: false,
			async: false);
		return new CallExpression(iife, NodeList.From(args[0]!, args[1]!), optional: false);
	}

	private Expression? CompileEnumerableDefaultValueOverload(
		ISymbol symbol,
		SenseArgument context,
		Expression?[] args,
		IOperation? originOperation)
	{
		// Compile dispatch is keyed by this exact static generic BCL member. The two source forms
		// below are the only Roslyn operations that can consume that member in authored C#.
		var method = (IMethodSymbol)symbol;
		switch (originOperation)
		{
			case IInvocationOperation invocation:
			{
				var invocationArguments = new List<Expression>(args.Length);
				foreach (var argument in args)
					invocationArguments.Add(argument!);

				return BuildDefaultValueCall(method, invocation, context, invocationArguments);
			}

			case IMethodReferenceOperation methodReference:
			{
				var parameters = new List<Node>(method.Parameters.Length);
				var methodGroupArguments = new List<Expression>(method.Parameters.Length);
				for (var index = 0; index < method.Parameters.Length; index++)
				{
					var parameter = new Identifier($"__enumerableDefaultArg{index}");
					parameters.Add(parameter);
					methodGroupArguments.Add(parameter);
				}

				var call = BuildDefaultValueCall(method, methodReference, context, methodGroupArguments);
				var body = new FunctionBody(
					NodeList.From<Statement>(new ReturnStatement(call)),
					strict: true);
				return new ArrowFunctionExpression(
					NodeList.From(parameters),
					body,
					@async: false,
					expression: false);
			}

			default:
				throw new InvalidOperationException(
					"Enumerable.DefaultIfEmpty() requires a bound invocation or static method reference.");
		}
	}

	private Expression BuildDefaultValueCall(
		IMethodSymbol method,
		IOperation originOperation,
		SenseArgument context,
		IReadOnlyList<Expression> arguments)
	{
		if (method.TypeArguments.Length != 1)
		{
			throw new InvalidOperationException(
				$"Enumerable.{method.Name}() requires one bound TSource type argument.");
		}

		var fallbackOverload = method.ContainingType
			.GetMembers(method.Name)
			.OfType<IMethodSymbol>()
			.SingleOrDefault(candidate => IsDefaultValueFallbackOverload(candidate, method));
		if (fallbackOverload is null)
			throw new InvalidOperationException(
				$"Enumerable.{method.Name}(.., defaultValue) is required as the runtime fallback contract.");

		var defaultValue = BuildDefaultValueExpression(originOperation, method.TypeArguments[0], context);
		var fallbackArguments = new List<Expression>(arguments.Count + 1);
		fallbackArguments.AddRange(arguments);
		fallbackArguments.Add(defaultValue);
		var expression = GetWhiteListExpression(
			fallbackOverload,
			context,
			fallbackArguments,
			out _,
			originOperation,
			fallbackOverload.ContainingType);
		return expression ?? throw new InvalidOperationException(
			$"Enumerable.{method.Name}(.., defaultValue) must have an Import mapping.");
	}

	private static bool IsDefaultValueFallbackOverload(IMethodSymbol candidate, IMethodSymbol method)
	{
		var original = method.OriginalDefinition;
		if (!candidate.IsStatic ||
			candidate.TypeParameters.Length != original.TypeParameters.Length ||
			candidate.Parameters.Length != original.Parameters.Length + 1 ||
			!SymbolEqualityComparer.Default.Equals(candidate.Parameters[candidate.Parameters.Length - 1].Type, candidate.TypeParameters[0]))
		{
			return false;
		}

		for (var index = 0; index < original.Parameters.Length; index++)
		{
			var candidateType = candidate.Parameters[index].Type.ToDisplayString(Jazor.Common.Format.NameFormat);
			var sourceType = original.Parameters[index].Type.ToDisplayString(Jazor.Common.Format.NameFormat);
			if (!string.Equals(candidateType, sourceType, StringComparison.Ordinal))
				return false;
		}

		return true;
	}
}
