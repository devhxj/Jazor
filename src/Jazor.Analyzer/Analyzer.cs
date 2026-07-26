using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Immutable;
using System.Linq;
using ECMAScript.Contract;
using Jazor.Compiler;
using Jazor.Common;

namespace Jazor.Analyzer;

/// <summary>
/// 表示一个诊断分析器，该分析器对标记有[ECMAScript]、[ECMAScriptModule]特性的类型和成员强制执行特定的规则。
/// </summary>
/// <remarks>
/// 约定“ES特性”包括 <b>[ECMAScript]</b>、<b>[ECMAScriptModule]</b>，同时约定只诊断被“ES特性”标记的类，不考虑“ES特性”的来源
/// <para>1、支持类型：默认支持数组、Lambda、委托、枚举、接口、record、匿名类型、抽象类、特性、类型参数、类型白名单和其他被“ES特性”标注的类型</para>
/// <para>2、分析器对泛型实参、数组元素类型、局部推断类型、集合表达式等擦除位置做严格入口诊断；若出现闭合的外部具体类型，要求该类型本身受支持</para>
/// <para>3、分析器不追踪类型参数 T 的真实来源；类型参数本身允许通过，等到具体运行时敏感的类型或成员用法再诊断</para>
/// <para>4、支持成员：被ES特性标注的类型的成员都可以使用，其余需要匹配白名单中的构造函数、字段、属性、方法、索引器</para>
/// <para>4、“ES特性”只能标记最外层的类、接口、枚举、委托等</para>
/// <para>5、“ES特性”标记的类中不能使用事件</para>
/// <para>6、“ES特性”标记的类中不能使用析构函数</para>
/// <para>7、“ES特性”标记的类中默认支持Lambda、委托、枚举、接口、匿名类型、抽象类、特性、类型参数</para>
/// <para>8、“ES特性”标记的类可支持其他特性，但不需要对特性的类型参数进行检查</para>
/// <para>9、“ES特性”还可以标记枚举类型，不支持结构体和接口，但都不进行诊断</para>
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public partial class Analyzer : DiagnosticAnalyzer
{
	private const string Attribute = "Attribute";
	private const string DiagnosticId = "JAZOR001";
	private const string AmbiguousRuntimeTypeFilterDiagnosticId = "JAZOR002";
	private const string InvalidSpreadUsageDiagnosticId = "JAZOR003";
	private const string ConflictingSpreadPropertyNameDiagnosticId = "JAZOR004";
	private const string Title = "Jazor";
	private const string MessageFormat = "[{0}] is not support in ECMAScript";
	private const string AmbiguousRuntimeTypeFilterMessageFormat = "[{0}] cannot be used for {1} because runtime alias '{2}' is shared with incompatible supported types: {3}";
	private const string InvalidSpreadUsageMessageFormat = "[Spread] is only valid on instance record properties that participate in structural object lowering";
	private const string ConflictingSpreadPropertyNameMessageFormat = "[Spread] cannot be combined with explicit JavaScript property-name attributes on '{0}'";
	private const string Category = "Security";

	/// <summary>
	/// 表示用于定义特定分析器诊断特征的诊断规则。
	/// </summary>
	/// <remarks>
	/// 此规则包含诊断 ID、标题、消息格式、类别、严重性以及是否默认启用等信息。用于配置和描述分析器报告的诊断行为。
	/// </remarks>
	private static readonly DiagnosticDescriptor Rule = new(
		DiagnosticId,
		Title,
		MessageFormat,
		Category,
		DiagnosticSeverity.Error,
		isEnabledByDefault: true);

	private static readonly DiagnosticDescriptor AmbiguousRuntimeTypeFilterRule = new(
		AmbiguousRuntimeTypeFilterDiagnosticId,
		Title,
		AmbiguousRuntimeTypeFilterMessageFormat,
		Category,
		DiagnosticSeverity.Error,
		isEnabledByDefault: true);

	private static readonly DiagnosticDescriptor InvalidSpreadUsageRule = new(
		InvalidSpreadUsageDiagnosticId,
		Title,
		InvalidSpreadUsageMessageFormat,
		Category,
		DiagnosticSeverity.Error,
		isEnabledByDefault: true);

	private static readonly DiagnosticDescriptor ConflictingSpreadPropertyNameRule = new(
		ConflictingSpreadPropertyNameDiagnosticId,
		Title,
		ConflictingSpreadPropertyNameMessageFormat,
		Category,
		DiagnosticSeverity.Error,
		isEnabledByDefault: true);

	/// <summary>
	/// <inheritdoc/>
	/// </summary>
	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule, AmbiguousRuntimeTypeFilterRule, InvalidSpreadUsageRule, ConflictingSpreadPropertyNameRule];

	internal static readonly ImmutableArray<OperationKind> AnalysisOperationKinds =
	[
		OperationKind.FieldInitializer,
		OperationKind.PropertyInitializer,
		OperationKind.ParameterInitializer,
		OperationKind.VariableDeclarationGroup,
		OperationKind.ObjectCreation,
		OperationKind.ArrayCreation,
		OperationKind.CollectionExpression,
		OperationKind.Invocation,
		OperationKind.BinaryOperator,
		OperationKind.FieldReference,
		OperationKind.PropertyReference,
		OperationKind.MethodReference,
		OperationKind.IsType,
		OperationKind.IsPattern,
		OperationKind.Switch,
		OperationKind.SwitchExpression,
		OperationKind.CatchClause,
		OperationKind.TypeOf,
		OperationKind.Conversion,
		OperationKind.ConditionalAccess,
		OperationKind.DefaultValue,
		OperationKind.Await,
		OperationKind.Using,
		OperationKind.EventReference,
		OperationKind.EventAssignment,
		OperationKind.AnonymousFunction,
		OperationKind.LocalFunction
	];

	private static readonly OperationKind[] AnalysisOperationKindArray = AnalysisOperationKinds.ToArray();

	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();
		context.RegisterSymbolAction(AnalyzeSpreadPropertyUsage, SymbolKind.Property);
        context.RegisterSymbolStartAction(startContext =>
		{
			var symbol = (INamedTypeSymbol)startContext.Symbol;
			if (symbol.TypeKind != TypeKind.Class)
				return;

			var hasAttribute = HasECMAScriptAttribute(symbol);
			// 特性只能标记最外层的类、接口、枚举、委托等，如果被包含，则直接报错
			if (hasAttribute && symbol.ContainingType is not null)
			{
				startContext.RegisterSymbolEndAction(static endContext =>
					endContext.ReportDiagnostic(Diagnostic.Create(Rule,
						endContext.Symbol.Locations.FirstOrDefault(x => x.IsInSource) ?? Location.None, "Error Nested Class")));

				// 不诊断嵌套类（性能优化）
				return;
			}

			// 跳过未被特性标注
			if (!hasAttribute && !InECMAScriptAttribute(symbol))
				return;

			// 处理字段、属性初始值
			startContext.RegisterOperationAction(AnalysisOperationAction, AnalysisOperationKindArray);

			// 检查类成员定义中使用的类型
			startContext.RegisterSymbolEndAction(AnalysisSymbolEndAction);
		}, SymbolKind.NamedType);
	}

	private static bool InECMAScriptAttribute(ITypeSymbol typeSymbol)
	{
		ISymbol current;
		if (typeSymbol.ContainingType is null)
			current = typeSymbol;
		else
		{
			current = typeSymbol.ContainingType;
			while (current.ContainingType is not null)
			{
				if (current.ContainingType.TypeKind == TypeKind.Class)
					current = current.ContainingType;
				else
					break;
			}
		}

		return current
			.GetAttributes()
			.Any(a => Util.IsECMAScriptSupportMarkerAttribute(a.AttributeClass));
	}

	private static bool HasECMAScriptAttribute(ITypeSymbol typeSymbol)
		=> typeSymbol.GetAttributes()
			.Any(a => Util.IsECMAScriptSupportMarkerAttribute(a.AttributeClass));

	private static bool IsWhiteListedType(ITypeSymbol typeSymbol)
		=> WhiteListLookup.TryGetValue(
			WhiteList.Types,
			typeSymbol.OriginalDefinition.ToDisplayString(Format.NameFormat),
			out _,
			out _);

	private static bool IsWhiteListedMember(ISymbol symbol)
		=> WhiteListLookup.TryGetValue(WhiteList.Members, symbol, out _, out _);

	private static bool IsWhiteListedProperty(IPropertySymbol property)
	{
		if (IsWhiteListedMember(property))
			return true;

		if (property.GetMethod is not null && IsWhiteListedMember(property.GetMethod))
			return true;

		if (property.SetMethod is not null && IsWhiteListedMember(property.SetMethod))
			return true;

		return false;
	}

	private static bool TryGetClassLikeRuntimeAlias(ITypeSymbol typeSymbol, out string runtimeAlias)
	{
		if (typeSymbol is null ||
			!WhiteListLookup.TryGetValue(
				WhiteList.Types,
				typeSymbol.OriginalDefinition.ToDisplayString(Format.NameFormat),
				out _,
				out var entry) ||
			entry.Op != Op.Alias ||
			string.IsNullOrWhiteSpace(entry.Value))
		{
			runtimeAlias = string.Empty;
			return false;
		}

		runtimeAlias = entry.Value!;
		return runtimeAlias is not ("String" or "Object" or "Array" or "Number" or "Date" or "BigInt" or "Map" or "Set" or "Boolean");
	}

	private static void CheckAmbiguousRuntimeTypeFilter(
		Action<Diagnostic> report,
		Compilation compilation,
		ITypeSymbol? typeSymbol,
		Location location,
		string usage)
	{
		if (typeSymbol is null ||
			!TryGetClassLikeRuntimeAlias(typeSymbol, out var runtimeAlias))
			return;

		var targetDisplayName = typeSymbol.OriginalDefinition.ToDisplayString(Format.NameFormat);
		var targetErasedDisplayName = EraseGenericDisplayArguments(targetDisplayName);
		var conflicts = WhiteList.Types
			.Where(static pair => pair.Value.Op == Op.Alias)
			.Where(pair => string.Equals(pair.Value.Value, runtimeAlias, StringComparison.Ordinal))
			.Where(pair => !string.Equals(pair.Key, targetDisplayName, StringComparison.Ordinal))
			.Where(pair => !string.Equals(EraseGenericDisplayArguments(pair.Key), targetErasedDisplayName, StringComparison.Ordinal))
			.Where(pair =>
			{
				var candidateType = TryResolveWhiteListAliasType(compilation, pair.Key);
				return candidateType is null || !IsRuntimeAliasAssignableToTarget(candidateType, typeSymbol);
			})
			.Select(static pair => pair.Key)
			.OrderBy(static name => name, StringComparer.Ordinal)
			.ToArray();

		if (conflicts.Length == 0)
			return;

		report(Diagnostic.Create(
			AmbiguousRuntimeTypeFilterRule,
			location,
			targetDisplayName,
			usage,
			runtimeAlias,
			string.Join(", ", conflicts)));
	}

	private static void CheckAmbiguousRuntimePattern(
		Action<Diagnostic> report,
		Compilation compilation,
		IPatternOperation? pattern,
		Location location)
	{
		if (pattern is null)
			return;

		switch (pattern)
		{
			case IDeclarationPatternOperation declarationPattern:
				CheckAmbiguousRuntimeTypeFilter(report, compilation, declarationPattern.MatchedType, location, "type checks");
				break;
			case ITypePatternOperation typePattern:
				CheckAmbiguousRuntimeTypeFilter(report, compilation, typePattern.MatchedType, location, "type checks");
				break;
			case IRecursivePatternOperation recursivePattern:
				if (recursivePattern.MatchedType is ITypeSymbol matchedType &&
					!matchedType.IsAnonymousType &&
					!matchedType.IsTupleType &&
					matchedType.SpecialType != SpecialType.System_Object)
				{
					CheckAmbiguousRuntimeTypeFilter(report, compilation, matchedType, location, "type checks");
				}

				foreach (var subpattern in recursivePattern.DeconstructionSubpatterns)
					CheckAmbiguousRuntimePattern(report, compilation, subpattern, location);

				foreach (var subpattern in recursivePattern.PropertySubpatterns)
					CheckAmbiguousRuntimePattern(report, compilation, subpattern.Pattern, location);
				break;
			case IBinaryPatternOperation binaryPattern:
				CheckAmbiguousRuntimePattern(report, compilation, binaryPattern.LeftPattern, location);
				CheckAmbiguousRuntimePattern(report, compilation, binaryPattern.RightPattern, location);
				break;
			case INegatedPatternOperation negatedPattern:
				CheckAmbiguousRuntimePattern(report, compilation, negatedPattern.Pattern, location);
				break;
			case IListPatternOperation listPattern:
				foreach (var subpattern in listPattern.Patterns)
					CheckAmbiguousRuntimePattern(report, compilation, subpattern, location);
				break;
			case ISlicePatternOperation slicePattern:
				CheckAmbiguousRuntimePattern(report, compilation, slicePattern.Pattern, location);
				break;
		}
	}

	private static ITypeSymbol? TryResolveWhiteListAliasType(Compilation compilation, string displayName)
	{
		return displayName switch
		{
			"bool" => compilation.GetSpecialType(SpecialType.System_Boolean),
			"byte" => compilation.GetSpecialType(SpecialType.System_Byte),
			"char" => compilation.GetSpecialType(SpecialType.System_Char),
			"decimal" => compilation.GetSpecialType(SpecialType.System_Decimal),
			"double" => compilation.GetSpecialType(SpecialType.System_Double),
			"float" => compilation.GetSpecialType(SpecialType.System_Single),
			"int" => compilation.GetSpecialType(SpecialType.System_Int32),
			"long" => compilation.GetSpecialType(SpecialType.System_Int64),
			"object" => compilation.GetSpecialType(SpecialType.System_Object),
			"sbyte" => compilation.GetSpecialType(SpecialType.System_SByte),
			"short" => compilation.GetSpecialType(SpecialType.System_Int16),
			"string" => compilation.GetSpecialType(SpecialType.System_String),
			"uint" => compilation.GetSpecialType(SpecialType.System_UInt32),
			"ulong" => compilation.GetSpecialType(SpecialType.System_UInt64),
			"ushort" => compilation.GetSpecialType(SpecialType.System_UInt16),
			_ => compilation.GetTypeByMetadataName(displayName)
		};
	}

	private static bool IsRuntimeAliasAssignableToTarget(ITypeSymbol candidateType, ITypeSymbol targetType)
	{
		if (SymbolEqualityComparer.Default.Equals(candidateType.OriginalDefinition, targetType.OriginalDefinition))
			return true;

		if (candidateType is not INamedTypeSymbol namedCandidate)
			return false;

		for (var current = namedCandidate.BaseType; current is not null; current = current.BaseType)
		{
			if (SymbolEqualityComparer.Default.Equals(current.OriginalDefinition, targetType.OriginalDefinition))
				return true;
		}

		foreach (var @interface in namedCandidate.AllInterfaces)
		{
			if (SymbolEqualityComparer.Default.Equals(@interface.OriginalDefinition, targetType.OriginalDefinition))
				return true;
		}

		return false;
	}

	private static string EraseGenericDisplayArguments(string displayName)
	{
		if (displayName.IndexOf('<') < 0)
			return displayName;

		var builder = new System.Text.StringBuilder(displayName.Length);
		var depth = 0;
		foreach (var ch in displayName)
		{
			if (ch == '<')
			{
				depth++;
				continue;
			}

			if (ch == '>')
			{
				if (depth > 0)
					depth--;
				continue;
			}

			if (depth == 0)
				builder.Append(ch);
		}

		return builder.ToString();
	}

	private static void CheckDirectType(Action<Diagnostic> report, ITypeSymbol typeSymbol, Location location)
	{
		var fullName = typeSymbol.OriginalDefinition.ToDisplayString(Format.NameFormat);
		if (IsWhiteListedType(typeSymbol))
			return;

		if (StructuralRecordSupport.IsStructuralRecordType(typeSymbol))
			return;

		if (!InECMAScriptAttribute(typeSymbol.OriginalDefinition))
			report(Diagnostic.Create(Rule, location, fullName));
	}

	private static void CheckMethodSignature(Action<Diagnostic> report, IMethodSymbol method, Location location)
	{
		if (method.MethodKind != MethodKind.Constructor)
			CheckType(report, method.ReturnType, location);

		foreach (var param in method.Parameters)
			CheckType(report, param.Type, GetLocation(param.Locations));

		foreach (var typeParam in method.TypeParameters)
			foreach (var constraint in typeParam.ConstraintTypes)
				CheckType(report, constraint, GetLocation(typeParam.Locations));
	}

	private static void CheckTypeArguments(Action<Diagnostic> report, ImmutableArray<ITypeSymbol> typeArguments, Location location)
	{
		foreach (var typeArgument in typeArguments)
			CheckType(report, typeArgument, location);
	}

	private static void CheckType(Action<Diagnostic> report, ITypeSymbol? typeSymbol, Location location)
	{
		// 允许枚举、接口、委托、匿名类型、抽象类、特性、类型参数
		if (typeSymbol is null ||
			typeSymbol.TypeKind == TypeKind.Enum ||
			typeSymbol.TypeKind == TypeKind.Interface ||
			typeSymbol.TypeKind == TypeKind.Delegate ||
			StructuralRecordSupport.IsStructuralRecordType(typeSymbol) ||
			typeSymbol.IsAnonymousType ||
			typeSymbol.IsAbstract ||
			IsAttribute(typeSymbol) ||
			typeSymbol is ITypeParameterSymbol)
			return;

		if (typeSymbol is IArrayTypeSymbol arrayType)
		{
			// 默认支持数组类型，不检查数组类型，只递归检查其元素类型
			CheckType(report, arrayType.ElementType, location);
			return;
		}

		if (typeSymbol is INamedTypeSymbol namedType)
		{
			if (namedType.IsTupleType)
			{
				// 递归检查元组的每个元素类型
				foreach (var field in namedType.TupleElements)
					CheckType(report, field.Type, location);

				return;
			}

			if (namedType.IsGenericType)
			{
				CheckDirectType(report, namedType.OriginalDefinition, location);

				foreach (var typeArg in namedType.TypeArguments)
					CheckType(report, typeArg, location);

				return;
			}
		}

		CheckDirectType(report, typeSymbol.OriginalDefinition, location);
	}

	private static void AnalysisOperationAction(OperationAnalysisContext ctx)
	{
		switch (ctx.Operation.Kind)
		{
			case OperationKind.FieldInitializer:
				{
					var initializer = (IFieldInitializerOperation)ctx.Operation;
					CheckType(ctx.ReportDiagnostic, initializer.Value.Type, initializer.Syntax.GetLocation());
					foreach (var field in initializer.InitializedFields)
						CheckType(ctx.ReportDiagnostic, field.Type, GetLocation(field.Locations));
				}
				break;
			case OperationKind.PropertyInitializer:
				{
					var initializer = (IPropertyInitializerOperation)ctx.Operation;
					CheckType(ctx.ReportDiagnostic, initializer.Value.Type, initializer.Syntax.GetLocation());
					foreach (var property in initializer.InitializedProperties)
						CheckType(ctx.ReportDiagnostic, property.Type, GetLocation(property.Locations));
				}
				break;
			case OperationKind.ParameterInitializer:
				{
					var initializer = (IParameterInitializerOperation)ctx.Operation;
					var property = initializer.Parameter;
					CheckType(ctx.ReportDiagnostic, property.Type, GetLocation(property.Locations));
					CheckType(ctx.ReportDiagnostic, initializer.Value.Type, initializer.Syntax.GetLocation());
				}
				break;
			case OperationKind.VariableDeclarationGroup:
				{
					var group = (IVariableDeclarationGroupOperation)ctx.Operation;
					foreach (var declaration in group.Declarations)
					{
						foreach (var declarator in declaration.Declarators)
						{
							if (declarator.Symbol is not null)
								CheckType(ctx.ReportDiagnostic, declarator.Symbol.Type, GetLocation(declarator.Symbol.Locations));
						}
					}
				}
				break;
			case OperationKind.ObjectCreation:
				{
					var creation = (IObjectCreationOperation)ctx.Operation;
					var type = creation.Type!;
					var location = creation.Syntax.GetLocation();
					CheckType(ctx.ReportDiagnostic, type, location);
					// 特性和[ECMAScript]标注类型不需要检查
					if (!IsAttribute(type) && !StructuralRecordSupport.IsStructuralRecordType(type) && !InECMAScriptAttribute(type))
					{
						// 添加构造函数检查
						var ctorKey = creation.Constructor!.OriginalDefinition.ToDisplayString(Format.NameFormat);
						if (!IsWhiteListedMember(creation.Constructor))
							ctx.ReportDiagnostic(Diagnostic.Create(Rule,
								location,
								ctorKey));
					}
				}
				break;
			case OperationKind.ArrayCreation:
				{
					var creation = (IArrayCreationOperation)ctx.Operation;
					if (creation.Type is IArrayTypeSymbol arrayType)
						CheckType(ctx.ReportDiagnostic, arrayType.ElementType, creation.Syntax.GetLocation());
				}
				break;
			case OperationKind.CollectionExpression:
				{
					var collection = (ICollectionExpressionOperation)ctx.Operation;
					CheckType(ctx.ReportDiagnostic, collection.Type, collection.Syntax.GetLocation());
				}
				break;
			case OperationKind.Invocation:
				{
					var invocation = (IInvocationOperation)ctx.Operation;
					// 检查 Instance 是否是委托类型（适用于 myDelegate() 或 event?.Invoke()）
					if (invocation.Instance?.Type?.TypeKind == TypeKind.Delegate)
						return;

					// 检查是否是委托的 Invoke 方法（适用于 myDelegate.Invoke()）
					if (invocation.TargetMethod.Name == "Invoke" &&
						invocation.TargetMethod.ContainingType.TypeKind == TypeKind.Delegate)
						return;

					CheckTypeArguments(ctx.ReportDiagnostic, invocation.TargetMethod.TypeArguments, invocation.Syntax.GetLocation());

					var key = invocation.TargetMethod.OriginalDefinition.ToDisplayString(Format.NameFormat);
					if (StructuralRecordSupport.IsStructuralRecordRuntimeSemanticInvocation(invocation))
					{
						ctx.ReportDiagnostic(Diagnostic.Create(Rule,
							invocation.Syntax.GetLocation(),
							key));
						return;
					}

					if (IsSupportedObjectLiteralAddInvocation(invocation))
						return;

					if (!Util.IsECMAScriptRecordProxyMember(
							invocation.TargetMethod,
							invocation.Instance?.Type ?? invocation.TargetMethod.ContainingType) &&
						StructuralRecordSupport.IsNonStructuralRecordRuntimeMember(
						invocation.TargetMethod,
						invocation.Instance?.Type ?? invocation.TargetMethod.ContainingType))
					{
						ctx.ReportDiagnostic(Diagnostic.Create(Rule,
							invocation.Syntax.GetLocation(),
							key));
						return;
					}

					if (IsWhiteListedMember(invocation.TargetMethod))
						return;

					if (InECMAScriptAttribute(invocation.TargetMethod.ContainingType))
						return;

					ctx.ReportDiagnostic(Diagnostic.Create(Rule,
						invocation.Syntax.GetLocation(),
						key));
				}
				break;
			case OperationKind.BinaryOperator:
				{
					var operation = (IBinaryOperation)ctx.Operation;
					if (operation.OperatorMethod is not null &&
						StructuralRecordSupport.IsNonStructuralRecordRuntimeMember(operation.OperatorMethod))
					{
						ctx.ReportDiagnostic(Diagnostic.Create(Rule,
							operation.Syntax.GetLocation(),
							operation.OperatorMethod.OriginalDefinition.ToDisplayString(Format.NameFormat)));
					}
				}
				break;
			case OperationKind.FieldReference:
				{
					var operation = (IFieldReferenceOperation)ctx.Operation;
					if (!Util.IsECMAScriptRecordProxyMember(
							operation.Field,
							operation.Instance?.Type ?? operation.Field.ContainingType) &&
						StructuralRecordSupport.IsNonStructuralRecordRuntimeMember(
						operation.Field,
						operation.Instance?.Type ?? operation.Field.ContainingType))
					{
						ctx.ReportDiagnostic(Diagnostic.Create(Rule,
							operation.Syntax.GetLocation(),
							operation.Field.OriginalDefinition.ToDisplayString(Format.NameFormat)));
						return;
					}

					// 枚举字段、特性、白名单内不检查
					if (operation.Field.ContainingType.TypeKind == TypeKind.Enum ||
						StructuralRecordSupport.IsStructuralRecordMember(operation.Field) ||
						InECMAScriptAttribute(operation.Field.ContainingType) ||
						IsWhiteListedMember(operation.Field))
						return;

					ctx.ReportDiagnostic(Diagnostic.Create(Rule,
						operation.Syntax.GetLocation(),
						operation.Field.OriginalDefinition.ToDisplayString(Format.NameFormat)));
				}
				break;

			case OperationKind.PropertyReference:
				{
					var operation = (IPropertyReferenceOperation)ctx.Operation;
					var hostType = operation.Instance?.Type ?? operation.Property.ContainingType;
					if (IsSupportedObjectLiteralIndexerReference(operation, hostType) ||
						Util.IsECMAScriptRecordProxyMember(operation.Property, hostType))
					{
						return;
					}

					if (StructuralRecordSupport.IsNonStructuralRecordRuntimeMember(
						operation.Property,
						hostType))
					{
						ctx.ReportDiagnostic(Diagnostic.Create(Rule,
							operation.Syntax.GetLocation(),
							operation.Property.OriginalDefinition.ToDisplayString(Format.NameFormat)));
						return;
					}

					// 匿名类型、特性、白名单内不检查
					if (operation.Property.ContainingType.IsAnonymousType ||
						StructuralRecordSupport.IsStructuralRecordMember(operation.Property) ||
						InECMAScriptAttribute(operation.Property.ContainingType) ||
						IsWhiteListedProperty(operation.Property))
						return;

					ctx.ReportDiagnostic(Diagnostic.Create(Rule,
						operation.Syntax.GetLocation(),
						operation.Property.OriginalDefinition.ToDisplayString(Format.NameFormat)));
				}
				break;
			case OperationKind.MethodReference:
				{
					var operation = (IMethodReferenceOperation)ctx.Operation;
					var key = operation.Method.OriginalDefinition.ToDisplayString(Format.NameFormat);
					if (!Util.IsECMAScriptRecordProxyMember(
							operation.Method,
							operation.Instance?.Type ?? operation.Method.ContainingType) &&
						StructuralRecordSupport.IsNonStructuralRecordRuntimeMember(
						operation.Method,
						operation.Instance?.Type ?? operation.Method.ContainingType))
					{
						ctx.ReportDiagnostic(Diagnostic.Create(Rule,
							operation.Syntax.GetLocation(),
							key));
						return;
					}

					if (IsWhiteListedMember(operation.Method))
						return;

					if (InECMAScriptAttribute(operation.Method.ContainingType))
						return;

					ctx.ReportDiagnostic(Diagnostic.Create(Rule,
						operation.Syntax.GetLocation(),
						key));
				}
				break;
			case OperationKind.IsType:
				{
					var operation = (IIsTypeOperation)ctx.Operation;
					CheckAmbiguousRuntimeTypeFilter(
						ctx.ReportDiagnostic,
						ctx.Compilation,
						operation.TypeOperand,
						operation.Syntax.GetLocation(),
						"type checks");
				}
				break;
			case OperationKind.IsPattern:
				{
					var operation = (IIsPatternOperation)ctx.Operation;
					CheckAmbiguousRuntimePattern(
						ctx.ReportDiagnostic,
						ctx.Compilation,
						operation.Pattern,
						operation.Syntax.GetLocation());
				}
				break;
			case OperationKind.Switch:
				{
					var operation = (ISwitchOperation)ctx.Operation;
					foreach (var @case in operation.Cases)
					{
						foreach (var clause in @case.Clauses.OfType<IPatternCaseClauseOperation>())
						{
							CheckAmbiguousRuntimePattern(
								ctx.ReportDiagnostic,
								ctx.Compilation,
								clause.Pattern,
								clause.Syntax.GetLocation());
						}
					}
				}
				break;
			case OperationKind.SwitchExpression:
				{
					var operation = (ISwitchExpressionOperation)ctx.Operation;
					foreach (var arm in operation.Arms)
					{
						CheckAmbiguousRuntimePattern(
							ctx.ReportDiagnostic,
							ctx.Compilation,
							arm.Pattern,
							arm.Syntax.GetLocation());
					}
				}
				break;
			case OperationKind.CatchClause:
				{
					var operation = (ICatchClauseOperation)ctx.Operation;
					if (operation.ExceptionType is null)
						return;

					CheckAmbiguousRuntimeTypeFilter(
						ctx.ReportDiagnostic,
						ctx.Compilation,
						operation.ExceptionType,
						operation.Syntax.GetLocation(),
						"catch type filtering");
				}
				break;
			case OperationKind.TypeOf:
				{
					var operation = (ITypeOfOperation)ctx.Operation;
					CheckType(ctx.ReportDiagnostic, operation.TypeOperand, operation.Syntax.GetLocation());
				}
				break;
			case OperationKind.DefaultValue:
				CheckType(ctx.ReportDiagnostic, ctx.Operation.Type, ctx.Operation.Syntax.GetLocation());
				break;
			case OperationKind.Conversion:
				CheckType(ctx.ReportDiagnostic, ctx.Operation.Type, ctx.Operation.Syntax.GetLocation());
				break;
			case OperationKind.ConditionalAccess:
				CheckType(ctx.ReportDiagnostic, ctx.Operation.Type, ctx.Operation.Syntax.GetLocation());
				break;
			case OperationKind.Await:
				CheckType(ctx.ReportDiagnostic, ctx.Operation.Type, ctx.Operation.Syntax.GetLocation());
				break;
			case OperationKind.Using:
				{
					var operation = (IUsingOperation)ctx.Operation;
					CheckType(ctx.ReportDiagnostic, operation.Resources.Type, operation.Syntax.GetLocation());
				}
				break;
			case OperationKind.EventReference:
				{
					var operation = (IEventReferenceOperation)ctx.Operation;
					ctx.ReportDiagnostic(Diagnostic.Create(Rule,
						operation.Syntax.GetLocation(),
						$"Event '{operation.Event.Name}'"));
				}
				break;
			case OperationKind.EventAssignment:
				ctx.ReportDiagnostic(Diagnostic.Create(Rule, ctx.Operation.Syntax.GetLocation(), $"Event"));
				break;
			case OperationKind.AnonymousFunction:
				{
					var operation = (IAnonymousFunctionOperation)ctx.Operation;
					foreach (var param in operation.Symbol.Parameters)
						CheckType(ctx.ReportDiagnostic, param.Type, GetLocation(param.Locations));
				}
				break;
			case OperationKind.LocalFunction:
				{
					var operation = (ILocalFunctionOperation)ctx.Operation;
					CheckMethodSignature(ctx.ReportDiagnostic, operation.Symbol, operation.Syntax.GetLocation());
				}
				break;
		}
	}

	private static void AnalysisSymbolEndAction(SymbolAnalysisContext ctx)
	{
		var symbol = (INamedTypeSymbol)ctx.Symbol;

		// 检查基类
		if (symbol.BaseType is not null && symbol.BaseType.SpecialType != SpecialType.System_Object)
			CheckType(ctx.ReportDiagnostic, symbol.BaseType, GetLocation(symbol.Locations));

		// 检查接口
		foreach (var iface in symbol.Interfaces)
			CheckType(ctx.ReportDiagnostic, iface, GetLocation(symbol.Locations));

		// 检查类型参数约束
		foreach (var typeParam in symbol.TypeParameters)
			foreach (var constraint in typeParam.ConstraintTypes)
				CheckType(ctx.ReportDiagnostic, constraint, GetLocation(typeParam.Locations));

		foreach (var member in symbol.GetMembers())
		{
			if (member is IFieldSymbol field)
				CheckType(ctx.ReportDiagnostic, field.Type, GetLocation(field.Locations));

			else if (member is IPropertySymbol property)
				CheckType(ctx.ReportDiagnostic, property.Type, GetLocation(property.Locations));

			else if (member is IEventSymbol evt)
				ctx.ReportDiagnostic(Diagnostic.Create(Rule, GetLocation(evt.Locations), $"Event '{evt.Name}'"));

			else if (member is IMethodSymbol method)
			{
				// 不支持析构函数
				if (method.MethodKind == MethodKind.Destructor)
					ctx.ReportDiagnostic(Diagnostic.Create(Rule, GetLocation(method.Locations), "Destructor"));
				else
					CheckMethodSignature(ctx.ReportDiagnostic, method, GetLocation(method.Locations));
			}

			//else if (member is INamedTypeSymbol nestedType)
			//{
			//	// 内部不支持嵌套成员
			//	ctx.ReportDiagnostic(Diagnostic.Create(Rule, GetLocation(nestedType.Locations), $"Nested '{nestedType.TypeKind}'"));
			//}
		}
	}

	private static Location GetLocation(ImmutableArray<Location> locations)
		=> locations.FirstOrDefault(x => x.IsInSource) ?? Location.None;

	private static bool IsSupportedObjectLiteralIndexerReference(
		IPropertyReferenceOperation operation,
		ITypeSymbol? hostType)
	{
		if (!IsSingleParameterIndexer(operation.Property) ||
			!Util.IsObjectLiteralHostType(hostType))
		{
			return false;
		}

		return operation.Parent is ISimpleAssignmentOperation assignment &&
			   ReferenceEquals(assignment.Target, operation) &&
			   IsInsideObjectOrCollectionInitializer(assignment);
	}

	private static bool IsSingleParameterIndexer(IPropertySymbol property)
		=> property.IsIndexer && property.Parameters.Length == 1;

	private static bool IsSupportedObjectLiteralAddInvocation(IInvocationOperation invocation)
	{
		var method = invocation.TargetMethod;
		if (method is not
			{
				MethodKind: MethodKind.Ordinary,
				IsStatic: false,
				Name: "Add"
			} ||
			method.Parameters.Length != 2 ||
			invocation.Arguments.Length != 2 ||
			method.Parameters[0].RefKind != RefKind.None ||
			method.Parameters[1].RefKind != RefKind.None ||
			!IsSupportedObjectLiteralKeyType(method.Parameters[0].Type) ||
			!Util.IsObjectLiteralHostType(invocation.Instance?.Type))
		{
			return false;
		}

		return invocation.Parent is IObjectOrCollectionInitializerOperation initializer &&
			   initializer.Initializers.Contains(invocation);
	}

	private static bool IsSupportedObjectLiteralKeyType(ITypeSymbol keyType)
	{
		if (keyType.SpecialType == SpecialType.System_String)
			return true;

		if (keyType.SpecialType is
			SpecialType.System_Byte or
			SpecialType.System_SByte or
			SpecialType.System_Int16 or
			SpecialType.System_UInt16 or
			SpecialType.System_Int32 or
			SpecialType.System_UInt32 or
			SpecialType.System_Int64 or
			SpecialType.System_UInt64 or
			SpecialType.System_Single or
			SpecialType.System_Double or
			SpecialType.System_Decimal)
		{
			return true;
		}

		return keyType.OriginalDefinition.ToDisplayString(Format.NameFormat) == "ECMAScript.Symbol";
	}

	private static bool IsInsideObjectOrCollectionInitializer(IOperation operation)
	{
		for (var current = operation.Parent; current is not null; current = current.Parent)
		{
			if (current is IObjectOrCollectionInitializerOperation)
				return true;

			if (current is IObjectCreationOperation or IAnonymousObjectCreationOperation)
				return false;
		}

		return false;
	}

	private static bool IsAttribute(ITypeSymbol typeSymbol)
	{
		if (typeSymbol.TypeKind != TypeKind.Class)
			return false;

		if (!typeSymbol.Name.EndsWith(Attribute, StringComparison.Ordinal))
			return false;

		if (typeSymbol.IsAbstract)
			return true;

		var current = typeSymbol;
		while (current is not null)
		{
			if (current.Name == Attribute)
				return true;

			current = current.BaseType;
		}

		return false;
	}

	private static void AnalyzeSpreadPropertyUsage(SymbolAnalysisContext context)
	{
		if (context.Symbol is not IPropertySymbol property)
			return;

		if (!HasSpreadAttribute(property))
			return;

		var location = GetLocation(property.Locations);
		if (property.IsStatic ||
			property.IsIndexer ||
			property.ContainingType is not INamedTypeSymbol { IsRecord: true })
		{
			context.ReportDiagnostic(Diagnostic.Create(InvalidSpreadUsageRule, location));
			return;
		}

		if (HasExplicitJavaScriptPropertyName(property))
		{
			context.ReportDiagnostic(Diagnostic.Create(
				ConflictingSpreadPropertyNameRule,
				location,
				property.ToDisplayString(Format.NameFormat)));
		}
	}

	private static bool HasSpreadAttribute(IPropertySymbol property)
		=> property.GetAttributes().Any(static attribute => attribute.AttributeClass?.ToDisplayString() == "ECMAScript.SpreadAttribute");

	private static bool HasExplicitJavaScriptPropertyName(IPropertySymbol property)
	{
		foreach (var attribute in property.GetAttributes())
		{
			var attributeName = attribute.AttributeClass?.Name;
			if (attributeName == "ECMAScriptNameAttribute")
				return true;

			if (attributeName == "DescriptionAttribute" &&
				attribute.ConstructorArguments.Length > 0 &&
				attribute.ConstructorArguments[0].Value is string description &&
				description.Trim().StartsWith("@#", StringComparison.Ordinal))
			{
				return true;
			}
		}

		return false;
	}
}
