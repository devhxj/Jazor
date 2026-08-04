using Acornima;
using Acornima.Ast;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Generic;
using System.Linq;

namespace Jazor.Compiler;

/// <summary>
/// 处理 using、await using 资源绑定和释放协议。
/// </summary>
/// <remarks>
/// using 的关键语义是无论 body 如何退出都执行释放，并保持同步/异步释放的区别。
/// 资源表达式通常需要缓存一次，避免在 finally 路径中重新求值；释放方法必须由当前白名单和
/// 接口投影确认，不能凭约定调用一个可能不存在的 <c>dispose</c> 属性。
/// </remarks>
public partial class SemanticWalker
{
	private enum UsingDisposalKind
	{
		Synchronous,
		Asynchronous
	}

	private sealed record UsingResourceBinding(
		Expression ResourceExpression,
		IMethodSymbol DisposeMethod,
		ITypeSymbol HostType,
		UsingDisposalKind DisposalKind,
		IOperation OriginOperation,
		Identifier? VariableIdentifier = null,
		VariableDeclarator? VariableDeclarator = null,
		List<Statement>? PrefixStatements = null);

	private List<Statement> TranslateOperationsToStatements(IEnumerable<IOperation> operations, SenseArgument context)
		=> TranslateOperationsToStatementsCore(operations, context);

	internal List<Statement> TranslateStatementSequence(IEnumerable<IOperation> operations, SenseArgument context)
	{
		var operationList = operations.ToList();
		if (operationList.Count == 0)
			return [];

		var bodyContext = EnsureScopeContext(operationList[0], context, ScopeSite.FunctionBody()).With(Sense.FunctionBody);
		return MaterializeScopedStatements(bodyContext, TranslateOperationsToStatementsCore(operationList, bodyContext));
	}

	private List<Statement> TranslateOperationsToStatementsCore(IEnumerable<IOperation> operations, SenseArgument context)
	{
		var operationList = operations.ToList();
		return TranslateOperationsRangeToStatements(operationList, 0, context);
	}

	private List<Statement> TranslateOperationsRangeToStatements(IReadOnlyList<IOperation> operations, int startIndex, SenseArgument context)
	{
		var pendingStatements = new List<Statement>();
		for (var index = startIndex; index < operations.Count; index++)
		{
			var operation = operations[index];
			if (operation is IUsingDeclarationOperation usingDeclaration)
			{
				pendingStatements.AddRange(LowerUsingDeclarationToStatements(usingDeclaration, operations, index + 1, context));
				break;
			}

			var node = Visit(operation, context);

			if (node is Statement statement)
				pendingStatements.Add(statement);
			else if (node is SequenceExpression { Expressions.Count: 0 })
				// Empty sequences are compiler-owned "no runtime statement" markers.
				continue;
			else if (node is Expression expression)
				// Pattern-switch statements intentionally lower to an IIFE call expression.
				pendingStatements.Add(new NonSpecialExpressionStatement(expression));
			else if (IsHostSkippedVariableDeclaration(operation, context))
				continue;
			else
				HandleTransformationFailure<Node>(operation, $"{operation.Kind} could not be translated to JavaScript.");
		}

		return pendingStatements;
	}

	private bool IsHostSkippedVariableDeclaration(IOperation operation, SenseArgument context)
		=> operation switch
		{
			IVariableDeclarationGroupOperation group => group.Declarations
				.SelectMany(static declaration => declaration.Declarators)
				.All(declarator => Host?.ShouldSkipVariableDeclarator(declarator, context) == true),
			IVariableDeclarationOperation declaration => declaration.Declarators
				.All(declarator => Host?.ShouldSkipVariableDeclarator(declarator, context) == true),
			_ => false
		};

	public override Node? VisitUsing(IUsingOperation operation, SenseArgument argument)
	{
		var scopedArgument = EnsureScopeContext(operation, argument);
		var disposalKind = GetUsingDisposalKind(operation.IsAsynchronous);
		var resources = BindUsingResources(operation.Resources, scopedArgument, allowVariableDeclarators: true, disposalKind);
		var usingBody = TranslateUsingBody(operation.Body, scopedArgument);
		var statements = BuildUsingTryFinallyStatements(resources, usingBody, scopedArgument);
		var node = statements.Count == 1
			? statements[0]
			: new NestedBlockStatement(NodeList.From(statements));
		return WithOriginIfMissing(node, operation);
	}

	public override Node? VisitUsingDeclaration(IUsingDeclarationOperation operation, SenseArgument argument)
	{
		// using declaration is lowered by sequential-operation materialization because
		// its disposal scope extends to the end of the enclosing block-like body.
		return HandleTransformationFailure<Node>(operation, "Using declaration operations must be lowered by the enclosing sequential statement translator.");
	}

	private List<Statement> LowerUsingDeclarationToStatements(
		IUsingDeclarationOperation operation,
		IReadOnlyList<IOperation> siblingOperations,
		int nextIndex,
		SenseArgument context)
	{
		var disposalKind = GetUsingDisposalKind(operation.IsAsynchronous);
		var resources = BindUsingDeclarationResources(operation, context, disposalKind);
		var bodyStatements = TranslateOperationsRangeToStatements(siblingOperations, nextIndex, context);
		return BuildUsingTryFinallyStatements(resources, bodyStatements, context);
	}

	private List<UsingResourceBinding> BindUsingDeclarationResources(
		IUsingDeclarationOperation operation,
		SenseArgument context,
		UsingDisposalKind disposalKind)
	{
		var resources = new List<UsingResourceBinding>();
		foreach (var declaration in operation.DeclarationGroup!.Declarations)
		{
			foreach (var declaratorOperation in declaration.Declarators)
			{
				var binding = BindUsingDeclarator(declaratorOperation, context, disposalKind);
				resources.Add(binding);
			}
		}

		return resources;
	}

	private List<UsingResourceBinding> BindUsingResources(
		IOperation resourcesOperation,
		SenseArgument context,
		bool allowVariableDeclarators,
		UsingDisposalKind disposalKind)
	{
		switch (resourcesOperation)
		{
			case IVariableDeclarationGroupOperation declarationGroup when allowVariableDeclarators:
			{
				var resources = new List<UsingResourceBinding>();
				foreach (var declaration in declarationGroup.Declarations)
				{
					foreach (var declaratorOperation in declaration.Declarators)
						resources.Add(BindUsingDeclarator(declaratorOperation, context, disposalKind));
				}

				return resources;
			}

			default:
			{
				var prefixStatements = new List<Statement>();
				var resourceExpression = MaterializeUsingResourceExpression(resourcesOperation, context, prefixStatements);
				var disposeMethod = ResolveUsingDisposeMethod(resourcesOperation, resourcesOperation.Type!, requireInterfaceFallback: true, disposalKind);
				var hostType = resourcesOperation.Type!;
				return [new UsingResourceBinding(resourceExpression, disposeMethod, hostType, disposalKind, resourcesOperation, PrefixStatements: prefixStatements)];
			}
		}
	}

	private UsingResourceBinding BindUsingDeclarator(
		IVariableDeclaratorOperation declaratorOperation,
		SenseArgument context,
		UsingDisposalKind disposalKind)
	{
		var variableDeclarator = Translate<VariableDeclarator>(declaratorOperation, context);
		var identifier = (Identifier)variableDeclarator.Id;
		var disposeMethod = ResolveUsingDisposeMethod(declaratorOperation, declaratorOperation.Symbol.Type, requireInterfaceFallback: false, disposalKind);
		var hostType = declaratorOperation.Symbol.Type;
		return new UsingResourceBinding(
			identifier,
			disposeMethod,
			hostType,
			disposalKind,
			declaratorOperation,
			identifier,
			variableDeclarator);
	}

	private List<Statement> TranslateUsingBody(IOperation bodyOperation, SenseArgument context)
	{
		var bodyContext = context.EnterScope(bodyOperation, ScopeSite.NestedBlock());
		var pendingStatements = bodyOperation is IBlockOperation blockOperation
			? TranslateOperationsToStatementsCore(blockOperation.Operations, bodyContext)
			: TranslateOperationsToStatements([bodyOperation], bodyContext);
		return MaterializeScopedStatements(bodyContext, pendingStatements);
	}

	private List<Statement> BuildUsingTryFinallyStatements(
		IReadOnlyList<UsingResourceBinding> resources,
		List<Statement> bodyStatements,
		SenseArgument context)
	{
		List<Statement> currentStatements = bodyStatements;

		for (var index = resources.Count - 1; index >= 0; index--)
		{
			var resource = resources[index];
			var statements = new List<Statement>();
			if (resource.PrefixStatements is { Count: > 0 })
				statements.AddRange(resource.PrefixStatements);

			if (resource.VariableDeclarator is not null)
				statements.Add(new VariableDeclaration(VariableDeclarationKind.Let, NodeList.From(resource.VariableDeclarator)));

			statements.Add(new TryStatement(
				new NestedBlockStatement(NodeList.From(currentStatements)),
				handler: null,
				finalizer: new NestedBlockStatement(NodeList.From(BuildUsingFinalizerStatements(resource, context)))));
			currentStatements = statements;
		}

		return currentStatements;
	}

	private List<Statement> BuildUsingFinalizerStatements(UsingResourceBinding resource, SenseArgument context)
	{
		var statements = new List<Statement>();
		var resourceReference = resource.VariableIdentifier ?? resource.ResourceExpression;
		var nullCheck = new NonLogicalBinaryExpression(Operator.StrictInequality, resourceReference, Null);
		var disposeCall = BuildUsingDisposeStatement(resource, resourceReference, context);
		statements.Add(new IfStatement(
			nullCheck,
			disposeCall,
			null));
		return statements;
	}

	private Statement BuildUsingDisposeStatement(UsingResourceBinding resource, Expression resourceReference, SenseArgument context)
	{
		var callExpression = BuildMethodCallExpression(
			resource.OriginOperation,
			resource.DisposeMethod,
			resource.OriginOperation.Syntax,
			resource.OriginOperation.SemanticModel,
			resourceReference,
			[],
			context,
			resource.HostType,
			allowIntrinsic: true,
			invocationOperation: null);

		var finalizerExpression = resource.DisposalKind == UsingDisposalKind.Asynchronous
			? new AwaitExpression(callExpression)
			: callExpression;
		return new NonSpecialExpressionStatement(finalizerExpression);
	}

	private static bool TryResolveUsingDisposeMethod(ITypeSymbol resourceType, string methodName, out IMethodSymbol disposeMethod)
	{
		if (resourceType is INamedTypeSymbol namedType)
		{
			foreach (var member in namedType.GetMembers(methodName).OfType<IMethodSymbol>())
			{
				if (!member.IsStatic &&
					member.Parameters.Length == 0)
				{
					disposeMethod = member;
					return true;
				}
			}
		}

		disposeMethod = null!;
		return false;
	}

	private static bool TryResolveUsingDisposeMethodByInterface(
		ITypeSymbol resourceType,
		string interfaceDisplayName,
		string methodName,
		out IMethodSymbol disposeMethod)
	{
		if (resourceType is INamedTypeSymbol namedType)
		{
			var interfaceType = namedType.AllInterfaces.FirstOrDefault(@interface =>
				@interface.OriginalDefinition.ToDisplayString(Jazor.Common.Format.NameFormat) == interfaceDisplayName);
			if (interfaceType is not null)
			{
				disposeMethod = interfaceType.GetMembers(methodName)
					.OfType<IMethodSymbol>()
					.Single();
				return true;
			}
		}

		disposeMethod = null!;
		return false;
	}

	private Expression MaterializeUsingResourceExpression(
		IOperation resourceOperation,
		SenseArgument context,
		List<Statement> prefixStatements)
	{
		var resourceExpression = Translate<Expression>(resourceOperation, context);
		if (CanReuseUsingResourceExpression(resourceExpression))
			return resourceExpression;

		var tempIdentifier = new Identifier(
			AllocateUniqueName(
				resourceOperation,
				context,
				LoweringSite.UsingResourceTemp("resource")));
		prefixStatements.Add(new VariableDeclaration(
			VariableDeclarationKind.Let,
			NodeList.From(new VariableDeclarator(tempIdentifier, resourceExpression))));
		return tempIdentifier;
	}

	private static bool CanReuseUsingResourceExpression(Expression expression)
		=> expression is Identifier or ThisExpression or Super;

	private IMethodSymbol ResolveUsingDisposeMethod(
		IOperation originOperation,
		ITypeSymbol resourceType,
		bool requireInterfaceFallback,
		UsingDisposalKind disposalKind)
	{
		var methodName = GetUsingDisposeMethodName(disposalKind);
		var interfaceDisplayName = GetUsingDisposeInterfaceDisplayName(disposalKind);

		if (TryResolveUsingDisposeMethod(resourceType, methodName, out var disposeMethod))
			return disposeMethod;

		if (resourceType is ITypeParameterSymbol typeParameter &&
			TryResolveUsingTypeParameterDisposeMethod(
				typeParameter,
				interfaceDisplayName,
				methodName,
				out disposeMethod))
		{
			if (disposeMethod.MethodKind == MethodKind.ExplicitInterfaceImplementation)
			{
				return HandleTransformationFailure<IMethodSymbol>(
					originOperation,
					$"Using resource type '{resourceType.OriginalDefinition.ToDisplayString(Jazor.Common.Format.NameFormat)}' " +
					$"resolves {methodName}() to an explicit interface implementation, which does not have a supported JavaScript runtime slot.");
			}

			return disposeMethod;
		}

		if (TryResolveUsingDisposeInterfaceImplementation(resourceType, interfaceDisplayName, methodName, out disposeMethod))
		{
			if (disposeMethod.MethodKind == MethodKind.ExplicitInterfaceImplementation)
			{
				return HandleTransformationFailure<IMethodSymbol>(
					originOperation,
					$"Using resource type '{resourceType.OriginalDefinition.ToDisplayString(Jazor.Common.Format.NameFormat)}' " +
					$"resolves {methodName}() to an explicit interface implementation, which does not have a supported JavaScript runtime slot.");
			}

			return disposeMethod;
		}

		if (requireInterfaceFallback &&
			TryResolveUsingDisposeMethodByInterface(resourceType, interfaceDisplayName, methodName, out disposeMethod))
			return disposeMethod;

		return HandleTransformationFailure<IMethodSymbol>(
			originOperation,
			$"Using resource type '{resourceType.OriginalDefinition.ToDisplayString(Jazor.Common.Format.NameFormat)}' does not expose a supported {methodName}() member.");
	}

	private static bool TryResolveUsingTypeParameterDisposeMethod(
		ITypeParameterSymbol typeParameter,
		string interfaceDisplayName,
		string methodName,
		out IMethodSymbol disposeMethod)
	{
		foreach (var constraintType in typeParameter.ConstraintTypes)
		{
			if (constraintType is ITypeParameterSymbol nestedTypeParameter &&
				TryResolveUsingTypeParameterDisposeMethod(
					nestedTypeParameter,
					interfaceDisplayName,
					methodName,
					out disposeMethod))
			{
				return true;
			}

			if (TryResolveUsingDisposeMethod(constraintType, methodName, out disposeMethod) ||
				TryResolveUsingDisposeInterfaceImplementation(
					constraintType,
					interfaceDisplayName,
					methodName,
					out disposeMethod) ||
				TryResolveUsingDisposeMethodByInterface(
					constraintType,
					interfaceDisplayName,
					methodName,
					out disposeMethod))
			{
				return true;
			}
		}

		disposeMethod = null!;
		return false;
	}

	private static UsingDisposalKind GetUsingDisposalKind(bool isAsynchronous)
		=> isAsynchronous ? UsingDisposalKind.Asynchronous : UsingDisposalKind.Synchronous;

	private static string GetUsingDisposeMethodName(UsingDisposalKind disposalKind)
		=> disposalKind == UsingDisposalKind.Asynchronous ? "DisposeAsync" : "Dispose";

	private static string GetUsingDisposeInterfaceDisplayName(UsingDisposalKind disposalKind)
		=> disposalKind == UsingDisposalKind.Asynchronous ? "System.IAsyncDisposable" : "System.IDisposable";

	private static bool TryResolveUsingDisposeInterfaceImplementation(
		ITypeSymbol resourceType,
		string interfaceDisplayName,
		string methodName,
		out IMethodSymbol disposeMethod)
	{
		disposeMethod = null!;

		if (resourceType is not INamedTypeSymbol namedType)
			return false;

		if (!TryResolveUsingDisposeMethodByInterface(resourceType, interfaceDisplayName, methodName, out var interfaceMethod))
			return false;

		if (namedType.FindImplementationForInterfaceMember(interfaceMethod) is IMethodSymbol implementation &&
			!implementation.IsStatic &&
			implementation.Parameters.Length == 0)
		{
			disposeMethod = implementation;
			return true;
		}

		return false;
	}
}
