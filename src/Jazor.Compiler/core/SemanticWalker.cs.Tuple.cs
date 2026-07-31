using Acornima;
using Acornima.Ast;
using Jazor.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Generic;
using System.Linq;

namespace Jazor.Compiler;

/// <summary>
/// 实现 tuple 和 structural value 的使用点擦除型 lowering。
/// </summary>
/// <remarks>
/// 这里保留投影、解构、比较和属性形状，不模拟 <c>System.ValueTuple</c> 的运行时身份。
/// 对可能有副作用的 tuple 源表达式必须先缓存，再重复使用其字段；直接重复翻译会改变行为。
/// </remarks>
public partial class SemanticWalker
{
	private readonly struct TupleValueSource
	{
		public TupleValueSource(IOperation operation)
		{
			Operation = operation;
			AstExpression = null;
		}

		public TupleValueSource(Expression astExpression)
		{
			Operation = null;
			AstExpression = astExpression;
		}

		public IOperation? Operation { get; }

		public Expression? AstExpression { get; }
	}

	private static string GetTupleRuntimeFieldName(IFieldSymbol field)
		=> Util.GetConfigOrSymbolName(field);

	private bool ShouldLowerStructurally(INamedTypeSymbol? namedType)
		=> IsStructuralType(namedType);

	private bool TryGetStructuralRuntimePropertyName(INamedTypeSymbol structuralType, int index, out string propertyName)
		=> TryGetStructuralRuntimeProperty(structuralType, index, out propertyName, out _);

	private bool TryGetStructuralRuntimeProperty(
		INamedTypeSymbol structuralType,
		int index,
		out string propertyName,
		out ITypeSymbol propertyType)
	{
		propertyName = null!;
		propertyType = null!;

		var constructor = structuralType.Constructors
			.FirstOrDefault(ctor => !ctor.IsStatic && ctor.Parameters.Length > index);
		if (constructor is null)
			return false;

		var parameter = constructor.Parameters[index];
		var property = EnumerateNamedTypeHierarchyBaseFirst(structuralType)
			.SelectMany(static current => current.GetMembers().OfType<IPropertySymbol>())
			.FirstOrDefault(member =>
				!member.IsStatic &&
				string.Equals(member.Name, parameter.Name, System.StringComparison.OrdinalIgnoreCase));

		propertyName = property is null
			? parameter.Name
			: Util.GetConfigOrSymbolName(property);
		propertyType = property?.Type ?? parameter.Type;
		return true;
	}

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
			var fieldName = GetTupleRuntimeFieldName(tupleType.TupleElements[index]);
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

		return BuildTupleProjection(new TupleValueSource(operation.Operand), sourceTupleType, targetTupleType, argument);
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
			return BuildTupleProjection(new TupleValueSource(source), sourceTupleType, targetTupleType, argument);

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
			if (!string.Equals(
				GetTupleRuntimeFieldName(sourceField),
				GetTupleRuntimeFieldName(targetField),
				System.StringComparison.Ordinal))
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
		TupleValueSource source,
		INamedTypeSymbol sourceType,
		INamedTypeSymbol targetType,
		SenseArgument argument)
	{
		var sourceOperation = source.Operation;
		ITupleOperation? tupleLiteral = sourceOperation as ITupleOperation;
		Expression? sourceExpression = null;
		Expression? cachedSourceInitialization = null;
		if (tupleLiteral is null)
		{
			if (sourceOperation is { } sourceOperationForProjection)
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
			else if (source.AstExpression is { } expression)
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
				TupleValueSource nestedSource = tupleLiteral is not null
					? new TupleValueSource(tupleLiteral.Elements[index])
					: new TupleValueSource(new MemberExpression(sourceExpression!, new Identifier(GetTupleRuntimeFieldName(sourceField)), false, false));
				value = BuildTupleProjection(nestedSource, nestedSourceType, nestedTargetType, argument);
			}
			else if (tupleLiteral is not null)
			{
				value = Translate<Expression>(tupleLiteral.Elements[index], argument);
			}
			else
			{
				value = new MemberExpression(sourceExpression!, new Identifier(GetTupleRuntimeFieldName(sourceField)), false, false);
			}

			properties.Add(new ObjectProperty(
				PropertyKind.Init,
				key: new Identifier(GetTupleRuntimeFieldName(targetField)),
				value: value,
				computed: false,
				shorthand: false,
				method: false));
		}

		var projection = new ObjectExpression(NodeList.From(properties));
		Expression result = cachedSourceInitialization is null
			? projection
			: new SequenceExpression(NodeList.From<Expression>(cachedSourceInitialization, projection));

		return sourceOperation is { } operationForOrigin
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
		DeconstructionInfo? deconstructionInfo =
			operation.SemanticModel is { } semanticModel &&
			operation.Syntax is AssignmentExpressionSyntax assignmentSyntax
				? semanticModel.GetDeconstructionInfo(assignmentSyntax)
				: null;
		var preparations = new List<Expression>();
		var writes = new List<Expression>();
		Deconstruct(
			operation.Target,
			operation.Value.Type!,
			new TupleValueSource(operation.Value),
			deconstructionInfo,
			preparations,
			writes);
		preparations.AddRange(writes);
		return WithOrigin(new SequenceExpression(NodeList.From(preparations)), operation);

		static DeconstructionInfo? GetNestedDeconstructionInfo(DeconstructionInfo? current, int index)
		{
			if (current is not { } info ||
				info.Nested.IsDefaultOrEmpty ||
				index >= info.Nested.Length)
			{
				return null;
			}

			return info.Nested[index];
		}

		static bool HasDeconstructionBinding(DeconstructionInfo? current)
			=> current is { } info &&
				(info.Method is not null || !info.Nested.IsDefaultOrEmpty);

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
		/// <returns>字段值表达式</returns>
		Expression GetTupleFieldValue(TupleValueSource value, IFieldSymbol field, int index, Identifier? tempVar, SenseArgument argument)
		{
			var fieldName = GetTupleRuntimeFieldName(field);
			if (tempVar is not null)
				return new MemberExpression(tempVar, new Identifier(fieldName), false, false);

			var valueOperation = value.Operation;
			if (valueOperation is ILocalReferenceOperation localRef)
			{
				var obj = new Identifier(localRef.Local.Name);
				return new MemberExpression(obj, new Identifier(fieldName), false, false);
			}
			if (valueOperation is ITupleOperation tupleOp)
			{
				// Nested tuple literals must use the enclosing slot's static view.
				// Roslyn can infer different element names for the literal itself;
				// direct translation would then construct one shape and read another.
				return TranslateTupleForTarget(tupleOp.Elements[index], field.Type, argument);
			}
			if (valueOperation is IConversionOperation conversion && conversion.Operand is ITupleOperation conversionTuple)
			{
				return TranslateTupleForTarget(conversionTuple.Elements[index], field.Type, argument);
			}
			if (valueOperation is { } operationValue)
			{
				var tupleExpr = Translate<Expression>(operationValue, argument);
				return new MemberExpression(tupleExpr, new Identifier(fieldName), false, false);
			}
			// TupleValueSource 只能由 operation 或 AST expression 构造；前者已在上方穷尽。
			return new MemberExpression(value.AstExpression!, new Identifier(fieldName), false, false);
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

		static bool ShouldCacheTupleField(TupleValueSource value, int index, HashSet<ILocalSymbol> targetLocals)
		{
			if (targetLocals.Count == 0)
				return false;

			return value.Operation switch
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

		Identifier CreateDeconstructSlotPlaceholder(string slot)
		{
			var id = new Identifier(AllocateUniqueName(operation, argument, LoweringSite.TupleNestedArgument(slot)));
			argument.AddVarDeclarator(new VariableDeclarator(id, null), _recursionDepth);
			return id;
		}

		Expression BuildDeconstructionFieldWriteTarget(IFieldReferenceOperation fieldReference)
		{
			if (IsImportedModuleStaticFieldMutation(fieldReference, argument))
			{
				return HandleTransformationFailure<Expression>(
					fieldReference,
					$"Cross-module static field mutation '{fieldReference.Field.OriginalDefinition.ToDisplayString(Jazor.Common.Format.NameFormat)}' is not supported because ECMAScript imported bindings are read-only. Expose a property setter or helper method on the module host instead.");
			}

			var instance = Translate<Expression>(fieldReference.Instance, argument, null);
			var fieldName = ResolveInitializerAssignmentMemberName(
				fieldReference,
				fieldReference.Field,
				"deconstruction assignment",
				fieldReference.Instance?.Type ?? fieldReference.Field.ContainingType);
			var property = new Identifier(fieldName);
			if (instance is not null)
				return new MemberExpression(instance, property, computed: false, optional: false);

			if (fieldReference.Field.IsStatic && fieldReference.Field.ContainingType is not null)
			{
				var containing = BuildFullTypeName(fieldReference.Field.ContainingType, argument);
				if (containing is not null)
					return new MemberExpression(containing, property, computed: false, optional: false);
			}

			return property;
		}

		void AppendDeconstructionWrite(IOperation target, Expression right, List<Expression> exprs, bool declareTarget = false)
		{
			switch (target)
			{
				case IDiscardOperation:
					return;

				case IDeclarationExpressionOperation declarationExpression:
				{
					var left = Translate<Expression>(declarationExpression.Expression, argument);
					if (declareTarget)
						argument.AddVarDeclarator(new VariableDeclarator(left, null), _recursionDepth);

					exprs.Add(new AssignmentExpression(Operator.Assignment, left, right));
					return;
				}

				case ILocalReferenceOperation localReference:
				{
					var left = Translate<Expression>(localReference, argument);
					if (declareTarget)
						argument.AddVarDeclarator(new VariableDeclarator(left, null), _recursionDepth);

					exprs.Add(new AssignmentExpression(Operator.Assignment, left, right));
					return;
				}

				case IFieldReferenceOperation fieldReference:
				{
					var left = BuildDeconstructionFieldWriteTarget(fieldReference);
					exprs.Add(new AssignmentExpression(Operator.Assignment, left, right));
					return;
				}

				case IPropertyReferenceOperation propertyReference:
				{
					var instance = Translate<Expression>(propertyReference.Instance, argument, null);
					var propertyArguments = new List<Expression>(propertyReference.Arguments.Length);
					foreach (var propertyArgument in propertyReference.Arguments)
					{
						var argContext = propertyArgument.Parameter?.RefKind is RefKind.Out
							? argument.With(Sense.OutParameter)
							: argument;
						propertyArguments.Add(Translate<Expression>(propertyArgument.Value, argContext));
					}

					exprs.Add(BuildPropertySetterAssignment(propertyReference, argument, instance, propertyArguments, right));
					return;
				}

				default:
					HandleTransformationFailure<Node>(target, $"The {target.Kind} operation is not supported in DeconstructionAssignment.");
					return;
			}
		}

		void Deconstruct(
			IOperation target,
			ITypeSymbol valueType,
			TupleValueSource value,
			DeconstructionInfo? bindingInfo,
			List<Expression> preparations,
			List<Expression> writes,
			bool declareTargets = false,
			string tupleSlot = "")
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
				if (value.Operation is { } valueOperation && ShouldCacheTupleSource(valueOperation))
				{
					// 复杂源值先整体缓存，再按字段读取。
					// 这样 deconstruct 仍按位置展开，但不会重复求值。
					tempVar = new Identifier(AllocateUniqueName(valueOperation, argument, LoweringSite.TupleDeconstructionSource()));
					var init = Translate<Expression>(valueOperation, argument);
					var declarator = new VariableDeclarator(tempVar, null);
					argument.AddVarDeclarator(declarator, _recursionDepth);
					preparations.Add(new AssignmentExpression(Operator.Assignment, tempVar, init));
				}

				var cachedValues = new Expression?[tupleTarget.Elements.Length];
				for (var index = 0; index < tupleTarget.Elements.Length; index++)
				{
					var element = tupleTarget.Elements[index];
					var field = ((INamedTypeSymbol)valueType).TupleElements[index];

					if (element is IDiscardOperation)
						continue;

					var fieldValue = GetTupleFieldValue(value, field, index, tempVar, argument);

					if (ShouldCacheTupleField(value, index, targetLocals))
					{
						// 右侧这个槽位会引用左侧目标局部变量。
						// 必须先把槽位值缓存下来，再进行后续赋值，否则会被前面已执行的回写污染。
						var slotSite = ComposeTupleSlot(tupleSlot, index);
						var cacheId = new Identifier(AllocateUniqueName(operation, argument, LoweringSite.TupleFieldCache(slotSite)));
						var cacheDecl = new VariableDeclarator(cacheId, null);
						argument.AddVarDeclarator(cacheDecl, _recursionDepth);
						preparations.Add(new AssignmentExpression(Operator.Assignment, cacheId, fieldValue));
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

					// 第一轮已为每个非 discard 槽位建立字段表达式或缓存。
					var right = cachedValues[index]!;

					var nestedBinding = GetNestedDeconstructionInfo(bindingInfo, index);
					if (field.Type.IsTupleType || HasDeconstructionBinding(nestedBinding))
					{
						Deconstruct(
							element,
							field.Type,
							new TupleValueSource(right),
							nestedBinding,
							preparations,
							writes,
							declareTargets,
							ComposeTupleSlot(tupleSlot, index));
					}
					else
						AppendDeconstructionWrite(element, right, writes, declareTargets || element is IDeclarationExpressionOperation);
				}
			}
			else if (valueType is INamedTypeSymbol recordType &&
					 ShouldLowerStructurally(recordType) &&
					 value.Operation is { } recordExpr)
			{
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

				Identifier? tempVar = null;
				if (ShouldCacheTupleSource(recordExpr))
				{
					tempVar = new Identifier(AllocateUniqueName(recordExpr, argument, LoweringSite.TupleDeconstructionSource()));
					var init = Translate<Expression>(recordExpr, argument);
					var declarator = new VariableDeclarator(tempVar, null);
					argument.AddVarDeclarator(declarator, _recursionDepth);
					preparations.Add(new AssignmentExpression(Operator.Assignment, tempVar, init));
				}

				var recordExprValue = tempVar is null
					? Translate<Expression>(recordExpr, argument)
					: (Expression)tempVar;

				for (var index = 0; index < tupleResult.Elements.Length; index++)
				{
					var element = tupleResult.Elements[index];
					if (element is IDiscardOperation)
						continue;

					if (!TryGetStructuralRuntimeProperty(
						recordType,
						index,
						out var propertyName,
						out var sourceMemberType))
					{
						HandleTransformationFailure<Node>(target, $"Structural type '{recordType.ToDisplayString(Jazor.Common.Format.NameFormat)}' could not resolve positional member {index} for deconstruction.");
						return;
					}

					var right = new MemberExpression(recordExprValue, new Identifier(propertyName), false, false);
					var nestedBinding = GetNestedDeconstructionInfo(bindingInfo, index);

					if (sourceMemberType.IsTupleType || HasDeconstructionBinding(nestedBinding))
						Deconstruct(
							element,
							sourceMemberType,
							new TupleValueSource(right),
							nestedBinding,
							preparations,
							writes,
							isDeclarationExpressionTarget || element is IDeclarationExpressionOperation,
							ComposeTupleSlot(tupleSlot, index));
					else
						AppendDeconstructionWrite(element, right, writes, isDeclarationExpressionTarget || element is IDeclarationExpressionOperation);
				}
			}
			else if (bindingInfo?.Method is IMethodSymbol method &&
				(value.Operation is not null || value.AstExpression is not null))
			{
				if (method.IsExtensionMethod)
				{
					HandleTransformationFailure<Node>(
						target,
						$"Extension Deconstruct method '{method.ToDisplayString(Jazor.Common.Format.NameFormat)}' is not supported because source extension methods do not have a receiver-member runtime slot.");
					return;
				}

				if (valueType.TypeKind == TypeKind.Struct)
				{
					HandleTransformationFailure<Node>(
						target,
						$"Custom Deconstruct on struct type '{valueType.ToDisplayString(Jazor.Common.Format.NameFormat)}' is not supported because member struct runtime declarations are not emitted.");
					return;
				}

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
				var assignmentTargets = new IOperation?[tupleResult.Elements.Length];
				var nestedAssignmentIds = new Identifier?[tupleResult.Elements.Length];
				var skipAssignments = new bool[tupleResult.Elements.Length];
				for (var index = 0; index < tupleResult.Elements.Length; index++)
				{
					var element = tupleResult.Elements[index];
					if (element is IDiscardOperation)
					{
						args.Add(CreateDeconstructSlotPlaceholder(ComposeTupleSlot(tupleSlot, index)));
						skipAssignments[index] = true;
					}
					else if (element is ILocalReferenceOperation localRef && isDeclarationExpressionTarget)
					{
						var name = localRef.Local.Name;
						var id = new Identifier(name);
						var declarator = new VariableDeclarator(id, null);

						args.Add(id);
						assignmentTargets[index] = localRef;
						argument.AddVarDeclarator(declarator, _recursionDepth);
					}
					else if (element is IDeclarationExpressionOperation declarationExpression)
					{
						var id = Translate<Expression>(declarationExpression.Expression, argument);
						var declarator = new VariableDeclarator(id, null);

						args.Add(id);
						assignmentTargets[index] = declarationExpression;
						argument.AddVarDeclarator(declarator, _recursionDepth);
					}
					else if (element is ITupleOperation subTuple)
					{
						// 如果是一个元组，需要创建一个临时变量，被自定义Deconstruct方法调用后
						// 再解构出元组里面变量定义或引用
						var id = CreateDeconstructSlotPlaceholder(ComposeTupleSlot(tupleSlot, index));

						args.Add(id);
						nestedAssignmentIds[index] = id;
					}
					else if (element is ILocalReferenceOperation localReference)
					{
						var id = new Identifier(localReference.Local.Name);
						args.Add(id);
						assignmentTargets[index] = localReference;
					}
					else
					{
						var id = CreateDeconstructSlotPlaceholder(ComposeTupleSlot(tupleSlot, index));
						args.Add(id);
						assignmentTargets[index] = element;
					}
				}

				// Deconstruct 方法在 C# 中通过 out 参数写回且无返回值，
				// 但 JS 没有 out/ref。
				// 当前编译器约定把它 lower 成：
				// - 普通参数调用
				// - 返回一个数组，数组元素就是原本 out 参数的输出值
				var obj = value.Operation is { } valueOperation
					? Translate<Expression>(valueOperation, argument)
					: value.AstExpression!;
				var methodName = GetCurrentModuleDeclaredOrConfigName(method);
				var callee = new MemberExpression(
					obj,
					new Identifier(methodName),
					computed: false,
					optional: false);
				var call = new CallExpression(callee, NodeList.From(args), optional: false);
				var deconstructName = AllocateUniqueName(operation, argument, LoweringSite.DeconstructResult());
				var deconstructId = new Identifier(deconstructName);
				var deconstructDecl = new VariableDeclarator(deconstructId, null);
				argument.AddVarDeclarator(deconstructDecl, _recursionDepth);
				preparations.Add(new AssignmentExpression(Operator.Assignment, deconstructId, call));

				// 从返回数组中取值，再回写到目标变量或临时嵌套 tuple 引用。
				for (var i = 0; i < args.Count; i++)
				{
					if (skipAssignments[i])
						continue;

					var indexer = new NumericLiteral(i, i.ToString());
					var member = new MemberExpression(deconstructId, indexer, computed: true, optional: false);
					if (nestedAssignmentIds[i] is { } nestedAssignmentId)
					{
						preparations.Add(new AssignmentExpression(Operator.Assignment, nestedAssignmentId, member));
						Deconstruct(
							tupleResult.Elements[i],
							method.Parameters[i].Type,
							new TupleValueSource(nestedAssignmentId),
							GetNestedDeconstructionInfo(bindingInfo, i),
							preparations,
							writes,
							tupleSlot: ComposeTupleSlot(tupleSlot, i));
						continue;
					}

					if (assignmentTargets[i] is { } assignmentTarget)
						AppendDeconstructionWrite(assignmentTarget, member, writes);
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
			(new TupleValueSource(operation.LeftOperand), operation.LeftOperand.Type!),
			(new TupleValueSource(operation.RightOperand), operation.RightOperand.Type!),
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
	private TupleOperandResult ProcessTupleOperand(TupleValueSource target, SenseArgument argument)
	{
		var operation = target.Operation;
		if (operation is ITupleOperation tuple)
			return new TupleOperandResult(null, tuple, null);
		if (operation is { } operationValue)
		{
			if (ShouldCacheTupleSource(operationValue))
			{
				var id = new Identifier(AllocateUniqueName(operationValue, argument, LoweringSite.TupleBinaryOperandCache()));
				var init = Translate<Expression>(operationValue, argument);
				var declarator = new VariableDeclarator(id, null);
				argument.AddVarDeclarator(declarator, _recursionDepth);
				var initialization = new AssignmentExpression(Operator.Assignment, id, init);
				return new TupleOperandResult(id, null, initialization);
			}

			return new TupleOperandResult(Translate<Expression>(operationValue, argument), null, null);
		}
		return new TupleOperandResult(target.AstExpression!, null, null);
	}

	/// <summary>
	/// 获取比较时某个 tuple 槽位的表达式。
	/// 如果原操作数本身就是 tuple 字面量，则直接使用对应元素表达式；
	/// 否则从缓存后的对象表达式按字段取值。
	/// </summary>
	private Expression GetTupleElementExpression(
		in TupleOperandResult operand,
		IFieldSymbol field,
		int index,
		SenseArgument argument)
	{
		if (operand.TupleOperation is not null)
			return Translate<Expression>(operand.TupleOperation.Elements[index], argument);

		// ProcessTupleOperand always supplies either TupleOperation or Expression for a
		// bound tuple operand. An empty result cannot originate from valid Roslyn tuple syntax.
		return new MemberExpression(operand.Expression!, new Identifier(GetTupleRuntimeFieldName(field)), false, false);
	}

	private Expression BuildTupleBinaryExpression(
		(TupleValueSource Target, ITypeSymbol Type) left,
		(TupleValueSource Target, ITypeSymbol Type) right,
		bool isEq,
		SenseArgument argument)
	{
		// ITupleBinaryOperation guarantees two non-empty, same-shape named tuple types.
		// Keep that Roslyn contract explicit so unreachable malformed-operation branches
		// cannot drift into the normal lowering path.
		var leftType = (INamedTypeSymbol)left.Type;
		var rightType = (INamedTypeSymbol)right.Type;

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
					? new TupleValueSource(leftResult.TupleOperation.Elements[index])
					: new TupleValueSource(GetTupleElementExpression(leftResult, leftField, index, argument));
				var subRight = rightResult.TupleOperation is not null
					? new TupleValueSource(rightResult.TupleOperation.Elements[index])
					: new TupleValueSource(GetTupleElementExpression(rightResult, rightField, index, argument));

				var subResult = BuildTupleBinaryExpression(
					(subLeft, leftField.Type),
					(subRight, rightField.Type),
					isEq,
					argument);

				result = result is null
					? subResult
					: new LogicalExpression(isEq ? Operator.LogicalAnd : Operator.LogicalOr, result, subResult);
			}
			else
			{
				var exprLeft = GetTupleElementExpression(leftResult, leftField, index, argument);
				var exprRight = GetTupleElementExpression(rightResult, rightField, index, argument);

				var expr = new NonLogicalBinaryExpression(
					isEq ? Operator.StrictEquality : Operator.StrictInequality,
					exprLeft,
					exprRight);

				result = result is null
					? expr
					: new LogicalExpression(isEq ? Operator.LogicalAnd : Operator.LogicalOr, result, expr);
			}
		}

		if (leftResult.Initialization is null && rightResult.Initialization is null)
			return result!;

		var expressions = new List<Expression>();
		if (leftResult.Initialization is not null)
			expressions.Add(leftResult.Initialization);
		if (rightResult.Initialization is not null)
			expressions.Add(rightResult.Initialization);
		expressions.Add(result!);
		result = new SequenceExpression(NodeList.From(expressions));

		return result;
	}
}
