using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection.Metadata;

namespace ECMAScript.Analyzer;

/// <summary>
/// 表示一个诊断分析器，该分析器对标记有[ECMAScript]、[ECMAScriptModule]特性的类型和成员强制执行特定的规则。
/// </summary>
/// <remarks>
/// 约定“ES特性”包括 <b>[ECMAScript]</b>、<b>[ECMAScriptModule]</b>，同时约定只诊断被“ES特性”标记的类，不考虑“ES特性”的来源
/// <para>1、支持类型：默认支持数组、Lambda、委托、枚举、接口、匿名类型、抽象类、特性、类型参数、类型白名单和其他被“ES特性”标注的类型，白名单只是检查名称，忽略泛型</para>
/// <para>2、支持成员：被ES特性标注的类型的成员都可以使用，其余需要匹配白名单中的构造函数、字段、属性、方法、索引器</para>
/// <para>3、成员白名单中的属性匹配，只是检查属性名，get和set不检查，不限制，方法名称检查忽略泛型，只检查纯名称</para>
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
	private const string ECMAScriptAttribute = "ECMAScriptAttribute";
	private const string ECMAScriptModuleAttribute = "ECMAScriptModuleAttribute";
	private const string DiagnosticId = "JAZOR001";
	private const string Title = "Jazor";
	private const string MessageFormat = "[{0}] is not support in ECMAScript";
	private const string Category = "Security";
	/// <summary>
	/// 不显示global::前缀，保留完整的命名空间路径，不显示泛型参数。
	/// </summary>
	private readonly static SymbolDisplayFormat NameFormat = new(
		globalNamespaceStyle:
			// 不包含 
			SymbolDisplayGlobalNamespaceStyle.Omitted,
		typeQualificationStyle:
			//保留完整的命名空间路径
			SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
		genericsOptions:
			// 不显示泛型参数
			SymbolDisplayGenericsOptions.IncludeTypeParameters,
		memberOptions:
			//SymbolDisplayMemberOptions.IncludeType |
			SymbolDisplayMemberOptions.IncludeModifiers |
			//SymbolDisplayMemberOptions.IncludeAccessibility |
			SymbolDisplayMemberOptions.IncludeExplicitInterface |
			SymbolDisplayMemberOptions.IncludeParameters |
			SymbolDisplayMemberOptions.IncludeContainingType |
			SymbolDisplayMemberOptions.IncludeConstantValue |
			SymbolDisplayMemberOptions.IncludeRef,
		delegateStyle:
			SymbolDisplayDelegateStyle.NameAndParameters,
		extensionMethodStyle:
			 SymbolDisplayExtensionMethodStyle.InstanceMethod,
		parameterOptions:
			SymbolDisplayParameterOptions.IncludeType |
			SymbolDisplayParameterOptions.IncludeModifiers |
			SymbolDisplayParameterOptions.IncludeParamsRefOut,
		propertyStyle:
			SymbolDisplayPropertyStyle.ShowReadWriteDescriptor,
		localOptions:
			SymbolDisplayLocalOptions.IncludeType |
			SymbolDisplayLocalOptions.IncludeModifiers |
			SymbolDisplayLocalOptions.IncludeConstantValue,
		kindOptions:
			SymbolDisplayKindOptions.None,
		miscellaneousOptions:
			SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
			SymbolDisplayMiscellaneousOptions.UseSpecialTypes
	);

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

	/// <summary>
	/// <inheritdoc/>
	/// </summary>
	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();
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
			startContext.RegisterOperationAction(AnalysisOperationAction,
				OperationKind.FieldInitializer,//obj.myField = 1
				OperationKind.PropertyInitializer, //obj.MyProperty = 1
				OperationKind.ParameterInitializer,//obj.MyMethod(myParam = 1)
				OperationKind.ObjectCreation,//new MyClass()
				OperationKind.ArrayCreation,//new MyType[10]
				OperationKind.DelegateCreation,//new Action(MyMethod)
				OperationKind.Invocation,//obj.MyMethod()
				OperationKind.FieldReference,//obj.myField
				OperationKind.PropertyReference,//obj.MyProperty
				OperationKind.MethodReference,//Action myAction = obj.MyMethod;
				OperationKind.TypeOf,//typeof(MyClass)
				OperationKind.Conversion,//(MyType)obj
				OperationKind.ConditionalAccess,//obj?.MyMethod()
				OperationKind.Await,//await task
				OperationKind.Using, //using var obj = new MyDisposable()
				OperationKind.EventReference,// obj.MyEvent += MyHandler
				OperationKind.EventAssignment,// obj.MyEvent = MyHandler
				OperationKind.AnonymousFunction);// 注册 Lambda

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
			.Any(a => a.AttributeClass?.Name == ECMAScriptAttribute || a.AttributeClass?.Name == ECMAScriptModuleAttribute);
	}

	private static bool HasECMAScriptAttribute(ITypeSymbol typeSymbol)
		=> typeSymbol.GetAttributes()
			.Any(a => a.AttributeClass?.Name == ECMAScriptAttribute || a.AttributeClass?.Name == ECMAScriptModuleAttribute);
	
	private static void CheckType(Action<Diagnostic> report, ITypeSymbol? typeSymbol,Location location)
	{
		// 允许枚举、接口、委托、匿名类型、抽象类、特性、类型参数
		if (typeSymbol is null ||
			typeSymbol.TypeKind == TypeKind.Enum ||
			typeSymbol.TypeKind == TypeKind.Interface ||
			typeSymbol.TypeKind == TypeKind.Delegate ||
			typeSymbol.IsAnonymousType ||
			typeSymbol.IsAbstract ||
			typeSymbol.Name == ECMAScriptAttribute ||
			typeSymbol.Name == ECMAScriptModuleAttribute ||
			IsAttribute(typeSymbol) ||
			typeSymbol is ITypeParameterSymbol)
			return;

		if (typeSymbol is IArrayTypeSymbol arrayType)
		{
			// 默认支持数组类型，不检查数组类型，只递归检查其元素类型
			// 只检查元素类型
			CheckType(report, arrayType.ElementType, location);
			return;
		}
		else if (typeSymbol is INamedTypeSymbol namedType)
		{
			if (namedType.IsTupleType)
			{
				// 递归检查元组的每个元素类型
				foreach (var field in namedType.TupleElements)
					CheckType(report, field.Type, location);

				return;
			}
			else if (namedType.IsGenericType)
			{
				// 泛型需要检查所有参数的类型，如果类型也是泛型，会在CheckType递归中检查
				// 如果是泛型类型，检查所有泛型参数类型，此处不需要检查嵌套的外层类型
				foreach (var typeArg in namedType.TypeArguments)
					CheckType(report, typeArg, location);

				return;
			}
		}

		// 允许白名单中的类型
		var fullName = typeSymbol.OriginalDefinition.ToDisplayString(NameFormat);
		if (WhiteList.Types.Contains(fullName))
			return;

		// 只要是被[ECMAScript]标记的类型就允许使用
		// 不用再判断自身类型，因为肯定是被标记的类型
		if (!InECMAScriptAttribute(typeSymbol.OriginalDefinition))
			report(Diagnostic.Create(Rule, location, fullName));
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
			case OperationKind.ObjectCreation:
				{
					var creation = (IObjectCreationOperation)ctx.Operation;
					var type = creation.Type!;
					// 特性和[ECMAScript]标注类型不需要检查
					if (!IsAttribute(type) && !InECMAScriptAttribute(type))
					{
						var location = creation.Syntax.GetLocation();
						CheckType(ctx.ReportDiagnostic, type, location);
						// 添加构造函数检查
						var ctorKey = creation.Constructor!.OriginalDefinition.ToDisplayString(NameFormat);
						if (!WhiteList.Members.Contains(ctorKey))
							ctx.ReportDiagnostic(Diagnostic.Create(Rule,
								location,
								ctorKey));
								//creation.Constructor!.ToDisplayString()));
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
			//case OperationKind.DelegateCreation:
			//	CheckType(ctx.ReportDiagnostic, ctx.Operation.Type, ctx.Operation.Syntax.GetLocation());
			//	break;
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

					if (InECMAScriptAttribute(invocation.TargetMethod.ContainingType))
						return;

					var key = invocation.TargetMethod.OriginalDefinition.ToDisplayString(NameFormat);
					if (WhiteList.Members.Contains(key))
						return;

					ctx.ReportDiagnostic(Diagnostic.Create(Rule,
						invocation.Syntax.GetLocation(),
						key));
				}
				break;
			case OperationKind.FieldReference:
				{
					var operation = (IFieldReferenceOperation)ctx.Operation;
					// 枚举字段、特性、白名单内不检查
					if (operation.Field.ContainingType.TypeKind == TypeKind.Enum ||
						InECMAScriptAttribute(operation.Field.ContainingType) ||
						WhiteList.Members.Contains(operation.Field.OriginalDefinition.ToDisplayString(NameFormat)))
						return;

					ctx.ReportDiagnostic(Diagnostic.Create(Rule,
						operation.Syntax.GetLocation(),
						operation.Field.OriginalDefinition.ToDisplayString(NameFormat)));
				}
				break;

			case OperationKind.PropertyReference:
				{
					var operation = (IPropertyReferenceOperation)ctx.Operation;

					// 匿名类型、特性、白名单内不检查
					if (operation.Property.ContainingType.IsAnonymousType ||
						InECMAScriptAttribute(operation.Property.ContainingType) ||
						WhiteList.Members.Contains(operation.Property.OriginalDefinition.ToDisplayString(NameFormat)))
						return;

					ctx.ReportDiagnostic(Diagnostic.Create(Rule,
						operation.Syntax.GetLocation(),
						operation.Property.OriginalDefinition.ToDisplayString(NameFormat)));
				}
				break;
			case OperationKind.MethodReference:
				{
					var operation = (IMethodReferenceOperation)ctx.Operation;
					if (InECMAScriptAttribute(operation.Method.ContainingType))
						return;

					var key = operation.Method.OriginalDefinition.ToDisplayString(NameFormat);
					if (WhiteList.Members.Contains(key))
						return;

					ctx.ReportDiagnostic(Diagnostic.Create(Rule,
						operation.Syntax.GetLocation(),
						key));
				}
				break;
			case OperationKind.TypeOf:
				{
					var operation = (ITypeOfOperation)ctx.Operation;
					CheckType(ctx.ReportDiagnostic, operation.TypeOperand, operation.Syntax.GetLocation());
				}
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
				{
					// 检查返回类型，跳过构造函数
					if (method.MethodKind != MethodKind.Constructor)
						CheckType(ctx.ReportDiagnostic, method.ReturnType, GetLocation(method.Locations));

					// 检查参数类型
					foreach (var param in method.Parameters)
						CheckType(ctx.ReportDiagnostic, param.Type, GetLocation(param.Locations));
				}
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

	private static bool IsAttribute(ITypeSymbol typeSymbol)
	{
		if (typeSymbol.TypeKind != TypeKind.Class)
			return false;

		return typeSymbol.Name.EndsWith(Attribute) && 
			(typeSymbol.IsAbstract || typeSymbol.ContainingType.Name == Attribute);
	}
}

