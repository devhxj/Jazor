using Acornima;
using Acornima.Ast;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Generic;
using System.Linq;

namespace Jazor.Compiler;

public partial class SemanticWalker
{
	/// <summary>
	/// 处理元组操作
	/// C# 示例：
	/// (1, "hello", true)          // 元组字面量
	/// var tuple = (x, y);         // 元组创建
	/// (double Sum, int Count) t2 = (4.5, 3);// 命名元组创建
	/// 转换结果：{ Item1: 1, Item2: "hello", Item3: true } 或 { Sum: 4.5, Count: 3 }。
	/// <para/>
	/// 这里仅负责“当前 tuple 视图”的对象字面量落地，不负责跨边界 remap。
	/// 当前编译器对 tuple 的 lowering 规则是：
	/// 1. tuple 只是一层语法糖，解构/比较/swap 等语义一律按“位置”处理；
	/// 2. 运行时不引入新的 tuple 类型，最终只落成普通对象；
	/// 3. 对外对象 shape 由“当前静态视图名字”决定，命名 tuple 在业务上视为稳定协议；
	/// 4. 只要 tuple 穿过边界后目标名字不同，就必须显式 remap，而不是直接透传。
	/// <para/>
	/// 换句话说：
	/// - 位置负责语义等价
	/// - 名字负责运行时协议
	/// - remap 负责把两者衔接起来
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitTuple(ITupleOperation operation, SenseArgument argument)
	{
		var nodes = new List<Node>();
		var tupleType = (INamedTypeSymbol)operation.NaturalType!;
		for (var index = 0; index < operation.Elements.Length; index++)
		{
			var fieldName = tupleType.TupleElements[index].Name;
			var element = operation.Elements[index];
			var key = new Identifier(fieldName);
			var value = Translate<Expression>(element, argument);
			nodes.Add(new ObjectProperty(
				PropertyKind.Init,
				key: key,
				value: value,
				computed: false,
				shorthand: false,
				method: false
			));
		}

		return WithOrigin(new ObjectExpression(NodeList.From(nodes)), operation);		
	}

	/// <summary>
	/// tuple 的运行时协议由“当前静态视图名字”决定。
	/// 只要目标视图和源视图名字不同，就需要在边界上显式重映射，
	/// 避免把 tuple 名字误当成可忽略的纯语法信息。
	/// <para/>
	/// 这里专门处理 Roslyn 已经显式给出 conversion 的场景。
	/// 如果 IOperation 树里根本没有 conversion，仍然要走 TranslateTupleForTarget(...)
	/// 主动在赋值/参数/return 等边界做 remap。
	/// </summary>
	private Expression? TryTranslateTupleConversion(IConversionOperation operation, SenseArgument argument)
	{
		if (operation.Type is not INamedTypeSymbol targetTupleType || !targetTupleType.IsTupleType ||
			operation.Operand.Type is not INamedTypeSymbol sourceTupleType || !sourceTupleType.IsTupleType)
			return null;

		if (HasSameTupleRuntimeShape(sourceTupleType, targetTupleType))
			return null;

		return BuildTupleProjection(operation.Operand, sourceTupleType, targetTupleType, argument);
	}

	/// <summary>
	/// tuple 在参数/赋值边界上按目标静态视图重映射。
	/// 这里不依赖 Roslyn 是否显式插入 Conversion，只要目标类型是另一套 tuple 名字，就主动 lower 成新对象。
	/// <para/>
	/// 这个 helper 是 tuple 边界规则的统一入口：
	/// - return / assignment / invocation / object creation / initializer
	/// 都应该复用它，而不是各自直接 Translate(value)。
	/// <para/>
	/// 这里故意不追求“C# tuple 名字不参与类型系统”的纯语义结论，而是遵循当前编译器约定：
	/// 位置负责语义，名字负责运行时协议，边界上由 remap 把两者接起来。
	/// </summary>
	private Expression TranslateTupleForTarget(IOperation source, ITypeSymbol? targetType, SenseArgument argument)
	{
		if (source.Type is INamedTypeSymbol sourceTupleType && sourceTupleType.IsTupleType &&
			targetType is INamedTypeSymbol targetTupleType && targetTupleType.IsTupleType &&
			!HasSameTupleRuntimeShape(sourceTupleType, targetTupleType))
			return BuildTupleProjection(source, sourceTupleType, targetTupleType, argument);

		return Translate<Expression>(source, argument);
	}

	/// <summary>
	/// 当 tuple 作为数组/集合元素时，目标视图来自元素类型而不是当前表达式本身。
	/// 这里统一抽出元素目标类型，避免数组创建、集合表达式、集合字面量各自猜一套规则。
	/// </summary>
	private static ITypeSymbol? GetCollectionElementTargetType(ITypeSymbol? targetType)
	{
		return targetType switch
		{
			IArrayTypeSymbol arrayType => arrayType.ElementType,
			INamedTypeSymbol namedType when namedType.TypeArguments.Length > 0 => namedType.TypeArguments[0],
			_ => null
		};
	}

	/// <summary>
	/// return 同样是 tuple 视图切换边界。
	/// 这里直接从 SemanticModel 取当前位置所在的 enclosing symbol 返回类型，
	/// 避免把 return source; 这类场景漏成直接透传。
	/// </summary>
	private static ITypeSymbol? GetTupleReturnTargetType(IReturnOperation operation)
	{
		return operation.SemanticModel?.GetEnclosingSymbol(operation.Syntax.SpanStart) is IMethodSymbol method
			? method.ReturnType
			: null;
	}

	/// <summary>
	/// 判断 tuple 的运行时对象 shape 是否一致。
	/// 只要任意一个槽位名字不同，就不能直接透传。
	/// </summary>
	private static bool HasSameTupleRuntimeShape(INamedTypeSymbol sourceType, INamedTypeSymbol targetType)
	{
		if (sourceType.TupleElements.Length != targetType.TupleElements.Length)
			return false;

		for (var index = 0; index < sourceType.TupleElements.Length; index++)
		{
			var sourceField = sourceType.TupleElements[index];
			var targetField = targetType.TupleElements[index];
			if (sourceField.Name != targetField.Name)
				return false;

			var sourceNested = sourceField.Type as INamedTypeSymbol;
			var targetNested = targetField.Type as INamedTypeSymbol;
			var isSourceNestedTuple = sourceNested?.IsTupleType == true;
			var isTargetNestedTuple = targetNested?.IsTupleType == true;
			if (isSourceNestedTuple != isTargetNestedTuple)
				return false;

			if (isSourceNestedTuple &&
				!HasSameTupleRuntimeShape(sourceNested!, targetNested!))
				return false;
		}

		return true;
	}

	/// <summary>
	/// 按目标 tuple 视图重新构造对象。
	/// 语义仍然按位置对应，但最终落地的 key 以目标视图名称为准。
	/// <para/>
	/// 例如：
	/// (name, age) -> (first, years)
	/// 会 lower 成 { first: source.name, years: source.age }，
	/// 而不是把原对象直接赋给目标变量。
	/// <para/>
	/// 实现上需要同时处理三类源值：
	/// 1. tuple 字面量：直接递归翻译各元素；
	/// 2. 简单引用：直接按字段取值；
	/// 3. 复杂表达式：先整体缓存，再按字段读取，避免重复求值。
	/// </summary>
	private Expression BuildTupleProjection(
		object source,
		INamedTypeSymbol sourceType,
		INamedTypeSymbol targetType,
		SenseArgument argument)
	{
		ITupleOperation? tupleLiteral = source as ITupleOperation;
		Expression? sourceExpression = null;
		Expression? cachedSourceInitialization = null;
		if (tupleLiteral is null)
		{
			if (source is IOperation sourceOperationForProjection)
			{
				if (ShouldCacheTupleSource(sourceOperationForProjection))
				{
					// projection 会按字段多次读取源值。
					// 对调用、属性、复杂表达式必须先整体缓存，
					// 否则会重复触发 getter/调用，改变原语义。
					var tempId = new Identifier(AllocateUniqueName(sourceOperationForProjection, argument, LoweringSite.TupleProjectionSource()));
					var init = Translate<Expression>(sourceOperationForProjection, argument);
					var declarator = new VariableDeclarator(tempId, null);
					argument.AddVarDeclarator(declarator, _recursionDepth);
					sourceExpression = tempId;
					cachedSourceInitialization = new AssignmentExpression(Operator.Assignment, tempId, init);
				}
				else
				{
					sourceExpression = Translate<Expression>(sourceOperationForProjection, argument);
				}
			}
			else if (source is Expression expression)
			{
				sourceExpression = expression;
			}
		}

		var properties = new List<Node>();
		for (var index = 0; index < targetType.TupleElements.Length; index++)
		{
			var sourceField = sourceType.TupleElements[index];
			var targetField = targetType.TupleElements[index];
			Expression value;
			if (sourceField.Type is INamedTypeSymbol nestedSourceType && nestedSourceType.IsTupleType &&
				targetField.Type is INamedTypeSymbol nestedTargetType && nestedTargetType.IsTupleType)
			{
				// 嵌套 tuple 仍然按“位置匹配、目标名字落地”的同一规则递归 remap。
				var nestedSource = tupleLiteral is not null
					? (object)tupleLiteral.Elements[index]
					: new MemberExpression(sourceExpression!, new Identifier(sourceField.Name), false, false);
				value = BuildTupleProjection(nestedSource, nestedSourceType, nestedTargetType, argument);
			}
			else if (tupleLiteral is not null)
			{
				value = Translate<Expression>(tupleLiteral.Elements[index], argument);
			}
			else
			{
				value = new MemberExpression(sourceExpression!, new Identifier(sourceField.Name), false, false);
			}

			properties.Add(new ObjectProperty(
				PropertyKind.Init,
				key: new Identifier(targetField.Name),
				value: value,
				computed: false,
				shorthand: false,
				method: false));
		}

		var projection = new ObjectExpression(NodeList.From(properties));
		Expression result = cachedSourceInitialization is null
			? projection
			: new SequenceExpression(NodeList.From<Expression>(cachedSourceInitialization, projection));

		return source is IOperation operationForOrigin
			? WithOriginIfMissing(result, operationForOrigin)
			: result;
	}

	/// <summary>
	/// tuple lowering 会多次读取同一个源值的不同槽位。
	/// 复杂表达式必须先缓存，否则会改变 getter/调用的求值次数。
	/// <para/>
	/// 例如：
	/// - ((int,int))GetTuple()
	/// - SomePropertyReturningTuple
	/// 都不能在 remap / compare / deconstruct 中被重复求值。
	/// <para/>
	/// 这里故意放宽“什么算复杂源值”的判断：宁可多缓存一次，也不能重复求值。
	/// </summary>
	private static bool ShouldCacheTupleSource(IOperation operation)
	{
		return operation switch
		{
			ITupleOperation => false,
			ILocalReferenceOperation => false,
			IParameterReferenceOperation => false,
			IFieldReferenceOperation => false,
			IInstanceReferenceOperation => false,
			IConversionOperation { Operand: ITupleOperation } => false,
			_ => true
		};
	}

	/// <summary>
	/// 处理解构赋值操作。
	/// <para/>
	/// 这里的核心目标不是模拟 CLR tuple 运行时，而是把 C# 解构语法糖展开成“结果等价”的 JS 赋值序列。
	/// 展开时需要同时满足三条约束：
	/// 1. 仍然按位置读取 tuple 槽位；
	/// 2. 复杂右值最多求值一次；
	/// 3. 自引用场景必须先缓存右侧元素，再回写左侧目标。
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	public override Node? VisitDeconstructionAssignment(IDeconstructionAssignmentOperation operation, SenseArgument argument)
	{
		// C# 示例：
		// var tuple = (aaa:1,2);
		// (int bbb, int ccc) = tuple;
		// int ddd,eee;
		// (ddd, eee) = tuple;
		// int kkk;
		// (kkk,int qqq) = tuple;
		// (int fff, (int ggg,int hhh)) = (2,tuple);
		// var func = (int x,int y)=>(mmm:x,y);
		// (int zzz,int yyy)= func(2,5);
		// 转换结果：
		// let tuple = {
		//     aaa: 1,
		//     Item2: 2
		// };
		// let bbb = tuple.aaa;
		// let ccc = tuple.Item2;
		// let ddd,eee;
		// ddd = tuple.aaa;
		// eee = tuple.Item2;
		// let kkk;
		// kkk = tuple.aaa;
		// let qqq = tuple.Item2;
		// let fff = 2;
		// let ggg = tuple.aaa;
		// let hhh = tuple.Item2;
		// let func = (x,y)=>{
		//     mmm: x,
		//     Item2: y
		// };
		// const temp = func(2,5);
		// let zzz = temp.aaa;
		// let yyy = temp.Item2;
		var expressions = new List<Expression>();
		Deconstruct(operation.Target, operation.Value.Type!, operation.Value, expressions);
		return WithOrigin(new SequenceExpression(NodeList.From(expressions)), operation);

		/// <summary>
		/// 从 value 获取某个 tuple 槽位的值表达式。
		/// 这里统一处理“源值是字面量 / 已缓存表达式 / 普通操作 / 已翻译表达式”四种情况，
		/// 避免解构主流程在每个分支里重复判断来源形态。
		/// </summary>
		/// <param name="value">值来源（IOperation 或 Expression）</param>
		/// <param name="fieldName">字段名</param>
		/// <param name="index">字段索引（用于 conversion 和 invocation 场景）</param>
		/// <param name="tempVar">临时变量（用于 invocation 场景）</param>
		/// <param name="argument">上下文参数</param>
		/// <returns>字段值表达式，失败返回 null</returns>
		Expression? GetTupleFieldValue(object value, string fieldName, int index, Identifier? tempVar, SenseArgument argument)
		{
			if (tempVar is not null)
				return new MemberExpression(tempVar, new Identifier(fieldName), false, false);
			if (value is ILocalReferenceOperation localRef)
			{
				var obj = new Identifier(localRef.Local.Name);
				return new MemberExpression(obj, new Identifier(fieldName), false, false);
			}
			if (value is ITupleOperation tupleOp)
				return Translate<Expression>(tupleOp.Elements[index], argument);
			if (value is IConversionOperation conversion && conversion.Operand is ITupleOperation conversionTuple)
			{
				return Translate<Expression>(conversionTuple.Elements[index], argument);
			}
			if (value is IOperation op)
			{
				var tupleExpr = Translate<Expression>(op, argument);
				return new MemberExpression(tupleExpr, new Identifier(fieldName), false, false);
			}
			if (value is Expression expr)
			{
				return new MemberExpression(expr, new Identifier(fieldName), false, false);
			}
			return null;
		}

		static void CollectTargetLocals(IOperation target, HashSet<ILocalSymbol> locals)
		{
			switch (target)
			{
				case ILocalReferenceOperation localRef:
					locals.Add(localRef.Local);
					break;
				case IDeclarationExpressionOperation declarationExpr:
					CollectTargetLocals(declarationExpr.Expression, locals);
					break;
				case ITupleOperation tupleTarget:
					foreach (var tupleElement in tupleTarget.Elements)
						CollectTargetLocals(tupleElement, locals);
					break;
			}
		}

		static bool ReferencesTargetLocal(IOperation operation, HashSet<ILocalSymbol> locals)
		{
			if (operation is ILocalReferenceOperation localRef &&
				locals.Contains(localRef.Local))
				return true;

			foreach (var child in operation.ChildOperations)
			{
				if (ReferencesTargetLocal(child, locals))
					return true;
			}

			return false;
		}

		static bool ShouldCacheTupleField(object value, int index, HashSet<ILocalSymbol> targetLocals)
		{
			if (targetLocals.Count == 0)
				return false;

			return value switch
			{
				// 只有右侧某个元素会读取左侧目标局部变量时，才需要把“槽位值”单独缓存。
				// 这样既能保证 swap / 自引用解构正确，又不会对所有元素都无差别引入临时变量。
				ITupleOperation tupleOp => ReferencesTargetLocal(tupleOp.Elements[index], targetLocals),
				IConversionOperation { Operand: ITupleOperation conversionTuple } => ReferencesTargetLocal(conversionTuple.Elements[index], targetLocals),
				_ => false
			};
		}

		static string ComposeTupleSlot(string parentSlot, int index)
		{
			var segment = index.ToString(System.Globalization.CultureInfo.InvariantCulture);
			return string.IsNullOrEmpty(parentSlot)
				? segment
				: parentSlot + "." + segment;
		}

		void Deconstruct(IOperation target, ITypeSymbol valueType, object value, List<Expression> exprs, bool declareTargets = false, string tupleSlot = "")
		{
			if (valueType.IsTupleType && target is ITupleOperation or IDeclarationExpressionOperation)
			{
				ITupleOperation tupleTarget;
				if (target is IDeclarationExpressionOperation declarationExpressionOp)
				{
					declareTargets = true;
					tupleTarget = (ITupleOperation)declarationExpressionOp.Expression;
				}
				else
					tupleTarget = (ITupleOperation)target;

				var targetLocals = new HashSet<ILocalSymbol>(SymbolEqualityComparer.Default);
				CollectTargetLocals(tupleTarget, targetLocals);

				Identifier? tempVar = null;
				if (value is IOperation valueOperation && ShouldCacheTupleSource(valueOperation))
				{
					// 复杂源值先整体缓存，再按字段读取。
					// 这样 deconstruct 仍按位置展开，但不会重复求值。
					tempVar = new Identifier(AllocateUniqueName(valueOperation, argument, LoweringSite.TupleDeconstructionSource()));
					var init = Translate<Expression>(valueOperation, argument);
					var declarator = new VariableDeclarator(tempVar, null);
					argument.AddVarDeclarator(declarator, _recursionDepth);
					exprs.Add(new AssignmentExpression(Operator.Assignment, tempVar, init));
				}

				var cachedValues = new Expression?[tupleTarget.Elements.Length];
				for (var index = 0; index < tupleTarget.Elements.Length; index++)
				{
					var element = tupleTarget.Elements[index];
					var field = ((INamedTypeSymbol)valueType).TupleElements[index];

					if (element is IDiscardOperation)
						continue;

					var fieldValue = GetTupleFieldValue(value, field.Name, index, tempVar, argument);
					if (fieldValue is null)
					{
						HandleTransformationFailure<Node>(target, $"The {target.Kind} operation is not supported in DeconstructionAssignment.");
						return;
					}

					if (ShouldCacheTupleField(value, index, targetLocals))
					{
						// 右侧这个槽位会引用左侧目标局部变量。
						// 必须先把槽位值缓存下来，再进行后续赋值，否则会被前面已执行的回写污染。
						var slotSite = ComposeTupleSlot(tupleSlot, index);
						var cacheId = new Identifier(AllocateUniqueName(operation, argument, LoweringSite.TupleFieldCache(slotSite)));
						var cacheDecl = new VariableDeclarator(cacheId, null);
						argument.AddVarDeclarator(cacheDecl, _recursionDepth);
						exprs.Add(new AssignmentExpression(Operator.Assignment, cacheId, fieldValue));
						cachedValues[index] = cacheId;
					}
					else
						cachedValues[index] = fieldValue;
				}

				// 遍历元组元素进行解构
				for (var index = 0; index < tupleTarget.Elements.Length; index++)
				{
					var element = tupleTarget.Elements[index];
					var field = ((INamedTypeSymbol)valueType).TupleElements[index];

					if (element is IDiscardOperation)
						continue;

					var right = cachedValues[index];
					if (right is null)
					{
						HandleTransformationFailure<Node>(target, $"The {target.Kind} operation is not supported in DeconstructionAssignment.");
						return;
					}

					if (field.Type.IsTupleType)
					{
						Deconstruct(element, field.Type, right, exprs, declareTargets, ComposeTupleSlot(tupleSlot, index));
					}
					else if (declareTargets || element is IDeclarationExpressionOperation)
					{
						var id = Translate<Node>(element, argument);
						var declarator = new VariableDeclarator(id, null);
						argument.AddVarDeclarator(declarator, _recursionDepth);
						exprs.Add(new AssignmentExpression(Operator.Assignment, id, right));
					}
					else if (element is ILocalReferenceOperation localRef)
					{
						var left = Translate<Node>(localRef, argument);
						exprs.Add(new AssignmentExpression(Operator.Assignment, left, right));
					}
					else
					{
						HandleTransformationFailure<Node>(element, $"The {element.Kind} operation is not supported in DeconstructionAssignment.");
						return;
					}
				}
			}
			else if (valueType.TypeKind == TypeKind.Class && value is IOperation expr)
			{
				// 自定义 Deconstruct 与 tuple 直接字段访问是两条路径：
				// 1. tuple 解构：按对象字段直接展开；
				// 2. 自定义 Deconstruct：先调用实例 Deconstruct(...)，
				//    再按当前编译器约定从返回数组中取出各 out 值。
				ITupleOperation tupleResult;
				bool isDeclarationExpressionTarget = false;
				if (target is IDeclarationExpressionOperation declarationExpr && declarationExpr.Expression is ITupleOperation t1)
				{
					tupleResult = t1;
					isDeclarationExpressionTarget = true;
				}
				else if (target is ITupleOperation t2)
					tupleResult = t2;
				else
				{
					HandleTransformationFailure<Node>(target, $"The {target.Kind} operation is not supported in DeconstructionAssignment.");
					return;
				}

				List<Expression> args = [];
				List<(int Index, Identifier Id)> nestedRefs = [];
				var tupleType = (INamedTypeSymbol)tupleResult.Type!;
				for (var index = 0; index < tupleResult.Elements.Length; index++)
				{
					var element = tupleResult.Elements[index];
					if (element is ILocalReferenceOperation localRef && isDeclarationExpressionTarget)
					{
						var name = localRef.Local.Name;
						var id = new Identifier(name);
						var declarator = new VariableDeclarator(id, null);

						args.Add(id);
						argument.AddVarDeclarator(declarator, _recursionDepth);
					}
					else if (element is ITupleOperation subTuple)
					{
						// 如果是一个元组，需要创建一个临时变量，被自定义Deconstruct方法调用后
						// 再解构出元组里面变量定义或引用
						var name = AllocateUniqueName(operation, argument, LoweringSite.TupleNestedArgument(ComposeTupleSlot(tupleSlot, index)));
						var id = new Identifier(name);
						var declarator = new VariableDeclarator(id, null);

						args.Add(id);
						nestedRefs.Add((index, id));
						argument.AddVarDeclarator(declarator, _recursionDepth);
					}
					else
					{
						var name = tupleType.TupleElements[index].Name;
						var id = new Identifier(name);
						// 处理声明表达式
						if (element is IDeclarationExpressionOperation)
						{
							var declarator = new VariableDeclarator(id, null);
							argument.AddVarDeclarator(declarator, _recursionDepth);
						}
						args.Add(id);
					}
				}

				// Deconstruct 方法在 C# 中通过 out 参数写回且无返回值，
				// 但 JS 没有 out/ref。
				// 当前编译器约定把它 lower 成：
				// - 普通参数调用
				// - 返回一个数组，数组元素就是原本 out 参数的输出值
				var obj = Translate<Expression>(expr, argument);
				var prop = new Identifier("Deconstruct");
				var func = new MemberExpression(obj, prop, false, false);
				var call = new CallExpression(func, NodeList.From(args), false);
				var deconstructName = AllocateUniqueName(operation, argument, LoweringSite.DeconstructResult());
				var deconstructId = new Identifier(deconstructName);
				var deconstructDecl = new VariableDeclarator(deconstructId, null);
				argument.AddVarDeclarator(deconstructDecl, _recursionDepth);
				exprs.Add(new AssignmentExpression(Operator.Assignment, deconstructId, call));

				// 从返回数组中取值，再回写到目标变量或临时嵌套 tuple 引用。
				for (var i = 0; i < args.Count; i++)
				{
					var indexer = new NumericLiteral(i, i.ToString());
					var member = new MemberExpression(deconstructId, indexer, computed: true, optional: false);
					var assignExpr = new AssignmentExpression(Operator.Assignment, args[i], member);
					exprs.Add(assignExpr);
				}

				IMethodSymbol method;
				// 如果 out 参数对应的是嵌套 tuple，这里继续递归展开。
				if (expr is IInvocationOperation invocation)
					method = invocation.TargetMethod;
				else
					method = (IMethodSymbol)valueType
						.GetMembers()
						.First(x => x.Kind == SymbolKind.Method && x.Name == "Deconstruct");

				foreach (var (index, id) in nestedRefs)
				{
					var parameter = method.Parameters[index];
					var element = tupleResult.Elements[index];
					Deconstruct(element, parameter.Type, id, expressions, tupleSlot: ComposeTupleSlot(tupleSlot, index));
				}
			}
			else
			{
				HandleTransformationFailure<Node>(target, $"The {target.Kind} operation is not supported in DeconstructionAssignment.");
			}
		}
	}

	/// <summary>
	/// 处理元组二元操作符操作（相等/不等比较）
	/// <para/>
	/// 支持的运算符：
	/// - == (相等): 所有元素都相等时返回 true
	/// - != (不等): 任意元素不等时返回 true
	/// <para/>
	/// C# 示例及转换结果：
	/// <code>
	/// // 简单元组比较
	/// (a, b) == (c, d)              → a===c&&b===d
	/// (x, y) != (1, 2)              → x!==1||y!==2
	///
	/// // 命名元组比较
	/// (name, age) == ("John", 25)   → name==="John"&&age===25
	///
	/// // 元组变量比较
	/// tuple1 == tuple2              → tuple1.Item1===tuple2.Item1&&tuple1.Item2===tuple2.Item2
	///
	/// // 嵌套元组比较
	/// ((a, b), c) == ((d, e), f)    → (a===d&&b===e)&&c===f
	///
	/// // 方法调用结果比较
	/// GetTuple() == (1, 2)          → const temp=GetTuple(); temp.Item1===1&&temp.Item2===2
	/// </code>
	/// <para/>
	/// 实现说明：
	/// - C# 编译器保证左右元组元素类型和个数必须相同
	/// - 递归处理嵌套元组
	/// - 使用严格相等 (===) 和严格不等 (!==) 运算符
	/// - 对于调用、属性等复杂源值，会创建临时变量缓存结果，避免重复求值
	/// </summary>
	/// <param name="operation">元组二元操作</param>
	/// <param name="argument">上下文参数，用于存放临时变量定义</param>
	/// <returns>带括号的 JavaScript 逻辑表达式，转换失败返回 null</returns>
	public override Node? VisitTupleBinaryOperator(ITupleBinaryOperation operation, SenseArgument argument)
	{
		// C#本身语法限定左右都必须是同样元素类型和个数的元组
		// 所以此处不用考虑类型和个数不匹配
		var isEq = operation.OperatorKind == BinaryOperatorKind.Equals;

		var result = BuildTupleBinaryExpression(
			(operation.LeftOperand, operation.LeftOperand.Type!),
			(operation.RightOperand, operation.RightOperand.Type!),
			isEq,
			argument);

		if (result is null)
			return HandleTransformationFailure<Node>(operation, "Tuple binary operation could not be translated to JavaScript.");

		return new ParenthesizedExpression(result);
	}

	/// <summary>
	/// 处理丢弃操作（下划线变量）
	/// C# 示例：
	/// _ = SomeMethod();        // 丢弃返回值
	/// (_, var y) = GetTuple(); // 丢弃元组的第一个元素
	/// 转换结果：undefined
	/// </summary>
	/// <param name="operation">当前访问的operation</param>
	/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
	/// <returns>Acornima的ESTree的Node</returns>
	/// <remarks>
	/// 注意：VisitSimpleAssignment 和 VisitDeconstructionAssignment 已经直接处理了 IDiscardOperation，
	/// 不会调用此方法。此方法主要作为兜底处理其他可能的场景（罕见）。
	/// </remarks>
	public override Node? VisitDiscardOperation(IDiscardOperation operation, SenseArgument argument)
	{
		// 返回 undefined 作为兜底
		// 注意：大多数情况下，此方法不会被调用，因为：
		// 1. _ = value 由 VisitSimpleAssignment 直接处理（检查 Target is IDiscardOperation）
		// 2. (_, y) = tuple 由 VisitDeconstructionAssignment 直接处理（检查 element is IDiscardOperation）
		return new Identifier("undefined");
	}

	/// <summary>
	/// <summary>
	/// 元组比较操作数的处理结果
	/// </summary>
	/// <param name="Expression">转换后的表达式（用于成员访问）</param>
	/// <param name="TupleOperation">如果是元组字面量，保留原始操作</param>
	private readonly record struct TupleOperandResult(
		Expression? Expression,
		ITupleOperation? TupleOperation,
		Expression? Initialization);

	/// <summary>
	/// 处理元组比较操作数。
	/// <para/>
	/// 比较场景和 remap / deconstruct 一样，也可能多次读取同一个 tuple 源值。
	/// 因此这里会把复杂操作数先缓存成一个表达式入口，后续统一按字段读取。
	/// </summary>
	private TupleOperandResult ProcessTupleOperand(object target, SenseArgument argument)
	{
		if (target is ITupleOperation tuple)
			return new TupleOperandResult(null, tuple, null);
		if (target is IOperation op)
		{
			if (ShouldCacheTupleSource(op))
			{
				var id = new Identifier(AllocateUniqueName(op, argument, LoweringSite.TupleBinaryOperandCache()));
				var init = Translate<Expression>(op, argument);
				var declarator = new VariableDeclarator(id, null);
				argument.AddVarDeclarator(declarator, _recursionDepth);
				var initialization = new AssignmentExpression(Operator.Assignment, id, init);
				return new TupleOperandResult(id, null, initialization);
			}

			return new TupleOperandResult(Translate<Expression>(op, argument), null, null);
		}
		if (target is Expression expr)
			return new TupleOperandResult(expr, null, null);

		return default;
	}

	/// <summary>
	/// 获取比较时某个 tuple 槽位的表达式。
	/// 如果原操作数本身就是 tuple 字面量，则直接使用对应元素表达式；
	/// 否则从缓存后的对象表达式按字段取值。
	/// </summary>
	private Expression? GetTupleElementExpression(
		in TupleOperandResult operand,
		IFieldSymbol field,
		int index,
		SenseArgument argument)
	{
		if (operand.TupleOperation is not null)
			return Translate<Expression>(operand.TupleOperation.Elements[index], argument);
		if (operand.Expression is not null)
			return new MemberExpression(operand.Expression, new Identifier(field.Name), false, false);
		return null;
	}

	private Expression? BuildTupleBinaryExpression(
		(object Target, ITypeSymbol Type) left,
		(object Target, ITypeSymbol Type) right,
		bool isEq,
		SenseArgument argument)
	{
		// 类型防御性检查：正常情况下 Roslyn 已保证左右都是同形 tuple，
		// 这里保留兜底只是为了让失败路径更明确。
		if (left.Type is not INamedTypeSymbol leftType || right.Type is not INamedTypeSymbol rightType)
			return null;

		// 先把左右操作数归一化成“tuple 字面量”或“可按字段读取的表达式入口”。
		var leftResult = ProcessTupleOperand(left.Target, argument);
		var rightResult = ProcessTupleOperand(right.Target, argument);

		Expression? result = null;

		// 遍历元组元素
		for (var index = 0; index < leftType.TupleElements.Length; index++)
		{
			var leftField = leftType.TupleElements[index];
			var rightField = rightType.TupleElements[index];

			// 处理嵌套元组
			if (leftField.Type.IsTupleType)
			{
				var subLeft = leftResult.TupleOperation is not null
					? (object)leftResult.TupleOperation.Elements[index]
					: GetTupleElementExpression(leftResult, leftField, index, argument)!;
				var subRight = rightResult.TupleOperation is not null
					? (object)rightResult.TupleOperation.Elements[index]
					: GetTupleElementExpression(rightResult, rightField, index, argument)!;

				var subResult = BuildTupleBinaryExpression(
					(subLeft, leftField.Type),
					(subRight, rightField.Type),
					isEq,
					argument);

				if (subResult is null)
					return null;

				result = result is null
					? subResult
					: new LogicalExpression(isEq ? Operator.LogicalAnd : Operator.LogicalOr, result, subResult);
			}
			else
			{
				var exprLeft = GetTupleElementExpression(leftResult, leftField, index, argument);
				var exprRight = GetTupleElementExpression(rightResult, rightField, index, argument);

				if (exprLeft is null || exprRight is null)
					return null;

				var expr = new NonLogicalBinaryExpression(
					isEq ? Operator.StrictEquality : Operator.StrictInequality,
					exprLeft,
					exprRight);

				result = result is null
					? expr
					: new LogicalExpression(isEq ? Operator.LogicalAnd : Operator.LogicalOr, result, expr);
			}
		}

		if (result is null)
			return null;

		if (leftResult.Initialization is null && rightResult.Initialization is null)
			return result;

		var expressions = new List<Expression>();
		if (leftResult.Initialization is not null)
			expressions.Add(leftResult.Initialization);
		if (rightResult.Initialization is not null)
			expressions.Add(rightResult.Initialization);
		expressions.Add(result);
		result = new SequenceExpression(NodeList.From(expressions));

		return result;
	}
}
