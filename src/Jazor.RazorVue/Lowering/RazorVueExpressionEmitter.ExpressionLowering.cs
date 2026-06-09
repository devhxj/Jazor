using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text;
using Acornima;
using Acornima.Ast;
using Jazor.Compiler;
using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Descriptor;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue.Lowering;

internal sealed partial class RazorVueExpressionEmitter
{
    internal const string ShouldRenderSwitchFallthroughAlias = "__jazorShouldRenderSwitchFallthrough";
    internal const string ShouldRenderSwitchResultAlias = "__jazorShouldRenderSwitchResult";

    private string EmitExpression(IOperation operation, SenseArgument? compilerArgument = null)
    {
        var current = Unwrap(operation);
        if (current is null)
            return "undefined";

        return EmitCompilerLoweredExpression(current, compilerArgument);
    }

    internal string EmitSetupExpression(IOperation operation, SenseArgument? compilerArgument = null)
    {
        var current = Unwrap(operation);
        if (current is null)
            return "undefined";

        ThrowIfReadOnlyByRefParameterEscapes(current);
        return WithSetupRewriteScope(() => EmitCompilerLoweredExpression(current, compilerArgument));
    }

    internal string EmitSetupStatementSequence(IEnumerable<IOperation> operations, SenseArgument? compilerArgument = null)
    {
        var operationList = operations?.ToArray() ?? throw new ArgumentNullException(nameof(operations));
        if (operationList.Length == 0)
            return string.Empty;

        foreach (var operation in operationList)
            ThrowIfReadOnlyByRefParameterEscapes(operation);

        var argument = (compilerArgument ?? _compilerArgument).With(Sense.FunctionBody);
        return WithSetupRewriteScope(() =>
        {
            var statements = _semanticWalker.TranslateStatementSequence(operationList, argument);
            var functionBody = new FunctionBody(NodeList.From(statements), strict: true);
            return NormalizeImperativeFunctionText(functionBody.ToKnRECMAScript());
        });
    }

    internal string EmitSetupShouldRenderStatementSequence(IEnumerable<IOperation> operations, SenseArgument? compilerArgument = null)
    {
        var operationList = operations?.ToArray() ?? throw new ArgumentNullException(nameof(operations));
        if (operationList.Length == 0)
            return string.Empty;

        foreach (var operation in operationList)
            ThrowIfReadOnlyByRefParameterEscapes(operation);

        var argument = (compilerArgument ?? _compilerArgument).With(Sense.FunctionBody);
        return WithShouldRenderStatementRewriteScope(() =>
            WithSetupRewriteScope(() =>
            {
                var statements = _semanticWalker.TranslateStatementSequence(operationList, argument);
                RewriteShouldRenderPatternSwitchIifes(operationList, statements);
                var functionBody = new FunctionBody(NodeList.From(statements), strict: true);
                return NormalizeImperativeFunctionText(functionBody.ToKnRECMAScript());
            }));
    }

    internal string EmitSetupLifecycleStatement(IOperation operation, SenseArgument? compilerArgument = null)
    {
        if (operation is null)
            throw new ArgumentNullException(nameof(operation));

        ThrowIfReadOnlyByRefParameterEscapes(operation);

        var argument = (compilerArgument ?? _compilerArgument).With(Sense.FunctionBody);
        return WithSetupRewriteScope(() =>
        {
            var node = _semanticWalker.Visit(operation, argument);
            if (node is not Statement statement)
            {
                throw new NotSupportedException(
                    $"RazorVue lifecycle statement lowering expected a statement but received '{node?.Type.ToString() ?? "<null>"}'.");
            }

            return statement.ToKnRECMAScript();
        });
    }

    private static void RewriteShouldRenderPatternSwitchIifes(
        IReadOnlyList<IOperation> operations,
        IList<Statement> statements)
    {
        if (operations.Count == 0 || statements.Count == 0)
            return;

        var patternSwitchPlans = CreateShouldRenderPatternSwitchPlans(operations);
        if (patternSwitchPlans.Count == 0)
            return;

        var needsFallthroughSentinel = false;
        var bridgeIndex = 0;
        var planIndex = 0;
        for (var statementIndex = 0; statementIndex < statements.Count; statementIndex++)
        {
            var statement = statements[statementIndex];
            if (!TryRewriteShouldRenderPatternSwitchStatement(
                    statement,
                    patternSwitchPlans,
                    ref planIndex,
                    ref bridgeIndex,
                    topLevel: true,
                    out var replacement,
                    out var statementNeedsFallthroughSentinel))
            {
                continue;
            }

            statements[statementIndex] = replacement;
            needsFallthroughSentinel |= statementNeedsFallthroughSentinel;
        }

        if (planIndex != patternSwitchPlans.Count)
            throw new NotSupportedException("RazorVue ShouldRender pattern-switch lowering could not locate every compiler-owned switch IIFE statement.");

        if (needsFallthroughSentinel)
        {
            statements.Insert(
                0,
                new VariableDeclaration(
                    VariableDeclarationKind.Const,
                    NodeList.From(new VariableDeclarator(
                        new Identifier(ShouldRenderSwitchFallthroughAlias),
                        new ObjectExpression(NodeList.Empty<Node>())))));
        }
    }

    private static IReadOnlyList<ShouldRenderPatternSwitchPlan> CreateShouldRenderPatternSwitchPlans(
        IReadOnlyList<IOperation> operations)
    {
        var plans = new List<ShouldRenderPatternSwitchPlan>();
        for (var index = 0; index < operations.Count; index++)
            AddShouldRenderPatternSwitchPlans(operations[index], isLastTopLevelOperation: index == operations.Count - 1, plans);

        return plans;
    }

    private static void AddShouldRenderPatternSwitchPlans(
        IOperation operation,
        bool isLastTopLevelOperation,
        List<ShouldRenderPatternSwitchPlan> plans)
    {
        var current = Unwrap(operation);
        switch (current)
        {
            case null:
                return;

            case ISwitchOperation switchOperation when IsPatternSwitchOperation(switchOperation):
                plans.Add(new ShouldRenderPatternSwitchPlan(BridgeFallthrough: !isLastTopLevelOperation));
                return;

            case IBlockOperation block:
                foreach (var child in block.Operations)
                    AddShouldRenderPatternSwitchPlans(child, isLastTopLevelOperation: false, plans);
                return;

            case IConditionalOperation conditional when conditional.Syntax is IfStatementSyntax:
                AddShouldRenderPatternSwitchPlans(conditional.WhenTrue, isLastTopLevelOperation: false, plans);
                if (conditional.WhenFalse is not null)
                    AddShouldRenderPatternSwitchPlans(conditional.WhenFalse, isLastTopLevelOperation: false, plans);
                return;

            case IWhileLoopOperation whileLoop:
                AddShouldRenderPatternSwitchPlans(whileLoop.Body, isLastTopLevelOperation: false, plans);
                return;

            case IForLoopOperation forLoop:
                AddShouldRenderPatternSwitchPlans(forLoop.Body, isLastTopLevelOperation: false, plans);
                return;

            case IForEachLoopOperation forEachLoop:
                AddShouldRenderPatternSwitchPlans(forEachLoop.Body, isLastTopLevelOperation: false, plans);
                return;

            case ITryOperation tryOperation:
                AddShouldRenderPatternSwitchPlans(tryOperation.Body, isLastTopLevelOperation: false, plans);
                foreach (var catchClause in tryOperation.Catches)
                    AddShouldRenderPatternSwitchPlans(catchClause.Handler, isLastTopLevelOperation: false, plans);
                if (tryOperation.Finally is not null)
                    AddShouldRenderPatternSwitchPlans(tryOperation.Finally, isLastTopLevelOperation: false, plans);
                return;
        }
    }

    private static bool TryRewriteShouldRenderPatternSwitchStatement(
        Statement statement,
        IReadOnlyList<ShouldRenderPatternSwitchPlan> plans,
        ref int planIndex,
        ref int bridgeIndex,
        bool topLevel,
        out Statement replacement,
        out bool needsFallthroughSentinel)
    {
        needsFallthroughSentinel = false;
        if (TryRewriteShouldRenderPatternSwitchIifeStatement(
                statement,
                plans,
                ref planIndex,
                ref bridgeIndex,
                topLevel,
                out replacement,
                out needsFallthroughSentinel))
        {
            return true;
        }

        switch (statement)
        {
            case BlockStatement block:
            {
                var rewrittenBody = RewriteShouldRenderPatternSwitchStatementList(
                    block.Body,
                    plans,
                    ref planIndex,
                    ref bridgeIndex,
                    out needsFallthroughSentinel);
                if (rewrittenBody is null)
                {
                    replacement = statement;
                    return false;
                }

                replacement = CreateShouldRenderPatternSwitchBlockReplacement(block, rewrittenBody);
                return true;
            }

            case IfStatement ifStatement:
            {
                var consequentChanged = TryRewriteShouldRenderPatternSwitchStatement(
                    ifStatement.Consequent,
                    plans,
                    ref planIndex,
                    ref bridgeIndex,
                    topLevel: false,
                    out var consequent,
                    out var consequentNeedsFallthroughSentinel);
                var alternate = ifStatement.Alternate;
                var alternateNeedsFallthroughSentinel = false;
                var alternateChanged = false;
                if (ifStatement.Alternate is not null)
                {
                    alternateChanged = TryRewriteShouldRenderPatternSwitchStatement(
                        ifStatement.Alternate,
                        plans,
                        ref planIndex,
                        ref bridgeIndex,
                        topLevel: false,
                        out var rewrittenAlternate,
                        out var rewrittenAlternateNeedsFallthroughSentinel);
                    if (alternateChanged)
                    {
                        alternate = rewrittenAlternate;
                        alternateNeedsFallthroughSentinel = rewrittenAlternateNeedsFallthroughSentinel;
                    }
                }

                if (!consequentChanged && !alternateChanged)
                {
                    replacement = statement;
                    return false;
                }

                replacement = new IfStatement(
                    ifStatement.Test,
                    consequentChanged ? consequent : ifStatement.Consequent,
                    alternateChanged ? alternate : ifStatement.Alternate);
                needsFallthroughSentinel = consequentNeedsFallthroughSentinel || (alternateChanged && alternateNeedsFallthroughSentinel);
                return true;
            }

            case WhileStatement whileStatement:
                return TryRewriteShouldRenderPatternSwitchLoopBody(
                    statement,
                    whileStatement.Body,
                    rewrittenBody => new WhileStatement(whileStatement.Test, rewrittenBody),
                    plans,
                    ref planIndex,
                    ref bridgeIndex,
                    out replacement,
                    out needsFallthroughSentinel);

            case DoWhileStatement doWhileStatement:
                return TryRewriteShouldRenderPatternSwitchLoopBody(
                    statement,
                    doWhileStatement.Body,
                    rewrittenBody => new DoWhileStatement(rewrittenBody, doWhileStatement.Test),
                    plans,
                    ref planIndex,
                    ref bridgeIndex,
                    out replacement,
                    out needsFallthroughSentinel);

            case ForStatement forStatement:
                return TryRewriteShouldRenderPatternSwitchLoopBody(
                    statement,
                    forStatement.Body,
                    rewrittenBody => new ForStatement(forStatement.Init, forStatement.Test, forStatement.Update, rewrittenBody),
                    plans,
                    ref planIndex,
                    ref bridgeIndex,
                    out replacement,
                    out needsFallthroughSentinel);

            case ForOfStatement forOfStatement:
                return TryRewriteShouldRenderPatternSwitchLoopBody(
                    statement,
                    forOfStatement.Body,
                    rewrittenBody => new ForOfStatement(forOfStatement.Left, forOfStatement.Right, rewrittenBody, forOfStatement.Await),
                    plans,
                    ref planIndex,
                    ref bridgeIndex,
                    out replacement,
                    out needsFallthroughSentinel);

            case TryStatement tryStatement:
            {
                var blockChanged = TryRewriteShouldRenderPatternSwitchNestedBlock(
                    tryStatement.Block,
                    plans,
                    ref planIndex,
                    ref bridgeIndex,
                    out var block,
                    out var blockNeedsFallthroughSentinel);
                var handlerBody = tryStatement.Handler?.Body;
                var handlerNeedsFallthroughSentinel = false;
                var handlerChanged = false;
                if (tryStatement.Handler is not null)
                {
                    handlerChanged = TryRewriteShouldRenderPatternSwitchNestedBlock(
                        tryStatement.Handler.Body,
                        plans,
                        ref planIndex,
                        ref bridgeIndex,
                        out var rewrittenHandlerBody,
                        out var rewrittenHandlerNeedsFallthroughSentinel);
                    if (handlerChanged)
                    {
                        handlerBody = rewrittenHandlerBody;
                        handlerNeedsFallthroughSentinel = rewrittenHandlerNeedsFallthroughSentinel;
                    }
                }

                var finalizer = tryStatement.Finalizer;
                var finalizerNeedsFallthroughSentinel = false;
                var finalizerChanged = false;
                if (tryStatement.Finalizer is not null)
                {
                    finalizerChanged = TryRewriteShouldRenderPatternSwitchNestedBlock(
                        tryStatement.Finalizer,
                        plans,
                        ref planIndex,
                        ref bridgeIndex,
                        out var rewrittenFinalizer,
                        out var rewrittenFinalizerNeedsFallthroughSentinel);
                    if (finalizerChanged)
                    {
                        finalizer = rewrittenFinalizer;
                        finalizerNeedsFallthroughSentinel = rewrittenFinalizerNeedsFallthroughSentinel;
                    }
                }

                if (!blockChanged && !handlerChanged && !finalizerChanged)
                {
                    replacement = statement;
                    return false;
                }

                replacement = new TryStatement(
                    blockChanged ? block : tryStatement.Block,
                    handlerChanged ? new CatchClause(tryStatement.Handler!.Param, handlerBody!) : tryStatement.Handler,
                    finalizerChanged ? finalizer : tryStatement.Finalizer);
                needsFallthroughSentinel = blockNeedsFallthroughSentinel ||
                    (handlerChanged && handlerNeedsFallthroughSentinel) ||
                    (finalizerChanged && finalizerNeedsFallthroughSentinel);
                return true;
            }

            default:
                replacement = statement;
                return false;
        }
    }

    private static bool TryRewriteShouldRenderPatternSwitchIifeStatement(
        Statement statement,
        IReadOnlyList<ShouldRenderPatternSwitchPlan> plans,
        ref int planIndex,
        ref int bridgeIndex,
        bool topLevel,
        out Statement replacement,
        out bool needsFallthroughSentinel)
    {
        needsFallthroughSentinel = false;
        replacement = statement;

        var expression = statement switch
        {
            NonSpecialExpressionStatement nonSpecialExpressionStatement => nonSpecialExpressionStatement.Expression,
            ExpressionStatement expressionStatement => expressionStatement.Expression,
            _ => null
        };

        if (expression is not CallExpression
            {
                Callee: ArrowFunctionExpression { Body: FunctionBody }
            } callExpression)
        {
            return false;
        }

        if (planIndex >= plans.Count)
            throw new NotSupportedException("RazorVue ShouldRender pattern-switch lowering found more compiler-owned switch IIFEs than expected.");

        var plan = plans[planIndex++];
        if (!plan.BridgeFallthrough && topLevel)
        {
            replacement = new ReturnStatement(callExpression);
            return true;
        }

        if (!plan.BridgeFallthrough)
        {
            replacement = new ReturnStatement(callExpression);
            return true;
        }

        var resultAlias = bridgeIndex == 0
            ? ShouldRenderSwitchResultAlias
            : ShouldRenderSwitchResultAlias + "$" + bridgeIndex.ToString(CultureInfo.InvariantCulture);
        var fallthroughCall = RewritePatternSwitchIifeFallthrough(callExpression);
        replacement = new NestedBlockStatement(NodeList.From(new Statement[]
        {
            new VariableDeclaration(
                VariableDeclarationKind.Const,
                NodeList.From(new VariableDeclarator(
                    new Identifier(resultAlias),
                    fallthroughCall))),
            new IfStatement(
                new NonLogicalBinaryExpression(
                    Operator.StrictInequality,
                    new Identifier(resultAlias),
                    new Identifier(ShouldRenderSwitchFallthroughAlias)),
                new ReturnStatement(new Identifier(resultAlias)),
                null)
        }));

        needsFallthroughSentinel = true;
        bridgeIndex++;
        return true;
    }

    private static Statement CreateShouldRenderPatternSwitchBlockReplacement(
        BlockStatement original,
        IReadOnlyList<Statement> body)
        => original is FunctionBody functionBody
            ? new FunctionBody(NodeList.From(body), functionBody.Strict)
            : new NestedBlockStatement(NodeList.From(body));

    private static List<Statement>? RewriteShouldRenderPatternSwitchStatementList(
        IEnumerable<Statement> statements,
        IReadOnlyList<ShouldRenderPatternSwitchPlan> plans,
        ref int planIndex,
        ref int bridgeIndex,
        out bool needsFallthroughSentinel)
    {
        needsFallthroughSentinel = false;
        List<Statement>? rewritten = null;
        var index = 0;
        foreach (var statement in statements)
        {
            if (TryRewriteShouldRenderPatternSwitchStatement(
                    statement,
                    plans,
                    ref planIndex,
                    ref bridgeIndex,
                    topLevel: false,
                    out var replacement,
                    out var statementNeedsFallthroughSentinel))
            {
                rewritten ??= statements.ToList();
                rewritten[index] = replacement;
                needsFallthroughSentinel |= statementNeedsFallthroughSentinel;
            }

            index++;
        }

        return rewritten;
    }

    private static bool TryRewriteShouldRenderPatternSwitchNestedBlock(
        NestedBlockStatement block,
        IReadOnlyList<ShouldRenderPatternSwitchPlan> plans,
        ref int planIndex,
        ref int bridgeIndex,
        out NestedBlockStatement replacement,
        out bool needsFallthroughSentinel)
    {
        var rewrittenBody = RewriteShouldRenderPatternSwitchStatementList(
            block.Body,
            plans,
            ref planIndex,
            ref bridgeIndex,
            out needsFallthroughSentinel);
        if (rewrittenBody is null)
        {
            replacement = block;
            return false;
        }

        replacement = new NestedBlockStatement(NodeList.From(rewrittenBody));
        return true;
    }

    private static bool TryRewriteShouldRenderPatternSwitchLoopBody(
        Statement original,
        Statement body,
        Func<Statement, Statement> createReplacement,
        IReadOnlyList<ShouldRenderPatternSwitchPlan> plans,
        ref int planIndex,
        ref int bridgeIndex,
        out Statement replacement,
        out bool needsFallthroughSentinel)
    {
        if (!TryRewriteShouldRenderPatternSwitchStatement(
                body,
                plans,
                ref planIndex,
                ref bridgeIndex,
                topLevel: false,
                out var rewrittenBody,
                out needsFallthroughSentinel))
        {
            replacement = original;
            return false;
        }

        replacement = createReplacement(rewrittenBody);
        return true;
    }

    private static CallExpression RewritePatternSwitchIifeFallthrough(CallExpression callExpression)
    {
        if (callExpression.Callee is not ArrowFunctionExpression { Body: FunctionBody functionBody } arrowFunction)
        {
            throw new NotSupportedException("RazorVue ShouldRender pattern-switch lowering expected a compiler-owned arrow IIFE.");
        }

        var statements = functionBody.Body
            .Select(static statement => RewritePatternSwitchFallthroughReturn(statement))
            .ToList();
        statements.Add(new ReturnStatement(new Identifier(ShouldRenderSwitchFallthroughAlias)));

        var rewrittenBody = new FunctionBody(NodeList.From(statements), functionBody.Strict);
        var rewrittenArrow = new ArrowFunctionExpression(
            NodeList.Empty<Node>(),
            rewrittenBody,
            expression: false,
            async: arrowFunction.Async);
        return new CallExpression(rewrittenArrow, NodeList.Empty<Expression>(), callExpression.Optional);
    }

    private static Statement RewritePatternSwitchFallthroughReturn(Statement statement)
    {
        switch (statement)
        {
            case ReturnStatement { Argument: null }:
                return new ReturnStatement(new Identifier(ShouldRenderSwitchFallthroughAlias));

            case IfStatement ifStatement:
                return new IfStatement(
                    ifStatement.Test,
                    RewritePatternSwitchFallthroughReturn(ifStatement.Consequent),
                    ifStatement.Alternate is null
                        ? null
                        : RewritePatternSwitchFallthroughReturn(ifStatement.Alternate));

            case NestedBlockStatement block:
                return new NestedBlockStatement(NodeList.From(block.Body.Select(static child => RewritePatternSwitchFallthroughReturn(child))));

            default:
                return statement;
        }
    }

    private static bool IsPatternSwitchOperation(IOperation operation)
    {
        var current = Unwrap(operation);
        return current is ISwitchOperation switchOperation &&
            switchOperation.Cases.Any(static switchCase =>
                switchCase.Clauses.Any(static clause => clause.CaseKind == CaseKind.Pattern));
    }

    private readonly record struct ShouldRenderPatternSwitchPlan(bool BridgeFallthrough);

    private LifecyclePayloadEmission EmitLifecyclePayloadCore(
        IMethodSymbol method,
        IOperation? operation,
        bool allowFirstRenderPayload,
        bool allowPureCompilerOwnedFallback = false)
    {
        var current = Unwrap(operation);
        if (current is null)
            throw new NotSupportedException($"RazorVue lifecycle payload is missing an operation in component '{method.ContainingType.ToDisplayString()}'.");

        try
        {
            if (TryEmitCompilerOwnedLifecyclePayload(
                    method,
                    current,
                    allowFirstRenderPayload,
                    out var dependencyBackedPayload,
                    requireLocalDependency: true))
            {
                return dependencyBackedPayload;
            }

            return current switch
            {
                ILiteralOperation literal => new LifecyclePayloadEmission(EmitLiteral(literal), false),
                IDefaultValueOperation defaultValue when IsNullDefaultValue(defaultValue) => new LifecyclePayloadEmission("null", false),
                IParameterReferenceOperation parameter when IsFirstRenderPayloadParameter(method, parameter, allowFirstRenderPayload) =>
                    new LifecyclePayloadEmission(LifecycleFirstRenderPlaceholder, true),
                IPropertyReferenceOperation property => EmitLifecyclePayloadPropertyReference(method, property),
                IFieldReferenceOperation field => EmitLifecyclePayloadFieldReference(method, field),
                IInvocationOperation invocation => EmitLifecyclePayloadInvocation(method, invocation, allowFirstRenderPayload),
                IUnaryOperation unary => EmitLifecyclePayloadUnary(method, unary, allowFirstRenderPayload),
                IBinaryOperation binary => EmitLifecyclePayloadBinary(method, binary, allowFirstRenderPayload),
                IConditionalOperation conditional when conditional.WhenTrue is not null && conditional.WhenFalse is not null =>
                    EmitLifecyclePayloadConditional(method, conditional, allowFirstRenderPayload),
                IInterpolatedStringOperation interpolated => EmitLifecyclePayloadInterpolatedString(method, interpolated, allowFirstRenderPayload),
                _ => TryEmitCompilerOwnedLifecyclePayload(method, current, allowFirstRenderPayload, out var compilerOwnedPayload)
                    ? compilerOwnedPayload
                    : throw new NotSupportedException(
                        $"RazorVue lifecycle payload does not support expression '{current.Kind}' in component '{method.ContainingType.ToDisplayString()}'.")
            };
        }
        catch (RazorVueCompilationIssueException)
        {
            throw;
        }
        catch (NotSupportedException)
        {
            if (TryEmitCompilerOwnedLifecyclePayload(
                    method,
                    current,
                    allowFirstRenderPayload,
                    out var compilerOwnedPayload,
                    allowPureCompilerOwnedFallback: allowPureCompilerOwnedFallback))
            {
                return compilerOwnedPayload;
            }

            throw;
        }
    }

    private bool TryEmitCompilerOwnedLifecyclePayload(
        IMethodSymbol lifecycleMethod,
        IOperation operation,
        bool allowFirstRenderPayload,
        out LifecyclePayloadEmission emission,
        bool requireLocalDependency = false,
        bool allowPureCompilerOwnedFallback = false)
    {
        emission = default;
        var firstRenderParameter = allowFirstRenderPayload
            ? lifecycleMethod.Parameters.FirstOrDefault(static parameter => parameter.Name == "firstRender")
            : null;

        var sourceStableLocals = RazorVueSourceStableLocalInitializerHelper.CollectSourceStableLocalInitializers(
            _snapshot.Compilation,
            GetLifecycleMethodOperations(lifecycleMethod),
            RazorVueSourceStableLocalInitializerHelper.CanParticipateInLifecycleCompilerFallback);

        if (!ValidateLifecycleCompilerPayloadShape(operation, sourceStableLocals))
            return false;

        try
        {
            var dependencies = CollectLifecycleCompilerFallbackDependencies(operation, sourceStableLocals);
            var usesFirstRender = firstRenderParameter is not null &&
                MayContainLifecycleFirstRenderReference(lifecycleMethod, operation, sourceStableLocals, dependencies);
            var requiredLocals = dependencies.SourceStableLocals;
            var orderedLocals = OrderLifecycleSourceStableLocals(requiredLocals);
            var firstRenderAliasLocals = firstRenderParameter is null
                ? ImmutableHashSet<ILocalSymbol>.Empty
                : CollectLifecycleFirstRenderAliasLocals(requiredLocals, firstRenderParameter);
            var preludeLocals = firstRenderAliasLocals.IsEmpty
                ? orderedLocals
                : orderedLocals
                    .Where(pair => !firstRenderAliasLocals.Contains(pair.Key))
                    .ToImmutableArray();
            var localAliases = CreateLifecycleSourceStableLocalAliases(orderedLocals, firstRenderAliasLocals);
            var localFunctions = dependencies.LocalFunctions;
            var localFunctionAliases = CreateLifecycleLocalFunctionAliases(localFunctions);
            var callableLocals = dependencies.CallableLocals;
            var callableLocalAliases = CreateLifecycleCallableLocalAliases(callableLocals);
            var hasLocalDependency =
                requiredLocals.Count != 0 ||
                !localFunctions.IsDefaultOrEmpty ||
                !callableLocals.IsDefaultOrEmpty;
            if (requireLocalDependency && !hasLocalDependency)
                return false;

            if (!usesFirstRender && !hasLocalDependency && !allowPureCompilerOwnedFallback)
            {
                return false;
            }

            var preludeBindings = EmitLifecyclePreludeBindings(
                preludeLocals,
                localAliases,
                localFunctions,
                localFunctionAliases,
                callableLocals,
                callableLocalAliases,
                firstRenderParameter);
            var parameterAliases = CreateLifecycleParameterAliases(firstRenderParameter);
            var expression = WithSourceStableLocalInitializers(
                requiredLocals,
                () => WithScopedLifecycleCallableAliases(
                    callableLocalAliases,
                    localFunctionAliases,
                    () => WithScopedLocalAliases(
                        localAliases,
                        () => WithScopedParameterAliases(parameterAliases, () => EmitSetupExpression(operation)))));
            emission = new LifecyclePayloadEmission(expression, usesFirstRender, preludeBindings);
            return true;
        }
        catch (RazorVueCompilationIssueException)
        {
            throw;
        }
        catch (Exception ex) when (ex is NotSupportedException or OperationTransformationException)
        {
            return false;
        }
    }

    private static Dictionary<ILocalSymbol, IOperation> CollectLifecycleReferencedSourceStableLocals(
        IOperation operation,
        IReadOnlyDictionary<ILocalSymbol, IOperation> sourceStableLocals)
    {
        var result = new Dictionary<ILocalSymbol, IOperation>(SymbolEqualityComparer.Default);
        if (sourceStableLocals.Count == 0)
            return result;

        var visiting = new HashSet<ILocalSymbol>(SymbolEqualityComparer.Default);

        collect(operation);
        return result;

        void collect(IOperation currentOperation)
        {
            foreach (var descendant in currentOperation.DescendantsAndSelf())
            {
                if (descendant is not ILocalReferenceOperation localReference ||
                    !sourceStableLocals.TryGetValue(localReference.Local, out var initializer))
                {
                    continue;
                }

                result[localReference.Local] = initializer;
                if (!visiting.Add(localReference.Local))
                    continue;

                try
                {
                    collect(initializer);
                }
                finally
                {
                    visiting.Remove(localReference.Local);
                }
            }
        }
    }

    private LifecycleCompilerFallbackDependencies CollectLifecycleCompilerFallbackDependencies(
        IOperation operation,
        IReadOnlyDictionary<ILocalSymbol, IOperation> sourceStableLocals)
    {
        var requiredLocals = new Dictionary<ILocalSymbol, IOperation>(SymbolEqualityComparer.Default);
        var localFunctions = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
        var callableLocals = new HashSet<ILocalSymbol>(SymbolEqualityComparer.Default);
        var visitingLocals = new HashSet<ILocalSymbol>(SymbolEqualityComparer.Default);
        var visitingLocalFunctions = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
        var visitingCallableLocals = new HashSet<ILocalSymbol>(SymbolEqualityComparer.Default);

        collect(operation);

        return new LifecycleCompilerFallbackDependencies(
            requiredLocals,
            localFunctions
                .OrderBy(static method => method.Locations.FirstOrDefault(static location => location.IsInSource)?.SourceTree?.FilePath ?? string.Empty, StringComparer.Ordinal)
                .ThenBy(static method => method.Locations.FirstOrDefault(static location => location.IsInSource)?.SourceSpan.Start ?? int.MaxValue)
                .ThenBy(static method => method.ToDisplayString(), StringComparer.Ordinal)
                .ToImmutableArray(),
            callableLocals
                .OrderBy(static local => local.Locations.FirstOrDefault(static location => location.IsInSource)?.SourceTree?.FilePath ?? string.Empty, StringComparer.Ordinal)
                .ThenBy(static local => local.Locations.FirstOrDefault(static location => location.IsInSource)?.SourceSpan.Start ?? int.MaxValue)
                .ThenBy(static local => local.Name, StringComparer.Ordinal)
                .ToImmutableArray());

        void collect(IOperation currentOperation)
        {
            foreach (var descendant in currentOperation.DescendantsAndSelf())
            {
                switch (descendant)
                {
                    case ILocalReferenceOperation localReference:
                        if (sourceStableLocals.TryGetValue(localReference.Local, out var initializer))
                        {
                            requiredLocals[localReference.Local] = initializer;
                            if (!visitingLocals.Add(localReference.Local))
                                continue;

                            try
                            {
                                collect(initializer);
                            }
                            finally
                            {
                                visitingLocals.Remove(localReference.Local);
                            }

                            continue;
                        }

                        if (localReference.Local.Type?.TypeKind == TypeKind.Delegate &&
                            callableLocals.Add(localReference.Local) &&
                            visitingCallableLocals.Add(localReference.Local))
                        {
                            try
                            {
                                collect(GetLifecycleCallableInitializer(localReference.Local));
                            }
                            finally
                            {
                                visitingCallableLocals.Remove(localReference.Local);
                            }
                        }

                        continue;

                    case IInvocationOperation invocation when invocation.TargetMethod.MethodKind == MethodKind.LocalFunction:
                        if (!localFunctions.Add(invocation.TargetMethod) ||
                            !visitingLocalFunctions.Add(invocation.TargetMethod))
                        {
                            continue;
                        }

                        try
                        {
                            collect(GetLifecycleLocalFunctionOperation(invocation.TargetMethod));
                        }
                        finally
                        {
                            visitingLocalFunctions.Remove(invocation.TargetMethod);
                        }

                        continue;

                    case IMethodReferenceOperation methodReference when methodReference.Method.MethodKind == MethodKind.LocalFunction:
                        if (!localFunctions.Add(methodReference.Method) ||
                            !visitingLocalFunctions.Add(methodReference.Method))
                        {
                            continue;
                        }

                        try
                        {
                            collect(GetLifecycleLocalFunctionOperation(methodReference.Method));
                        }
                        finally
                        {
                            visitingLocalFunctions.Remove(methodReference.Method);
                        }

                        continue;
                }
            }
        }
    }

    private static ImmutableArray<KeyValuePair<ILocalSymbol, IOperation>> OrderLifecycleSourceStableLocals(
        IReadOnlyDictionary<ILocalSymbol, IOperation> sourceStableLocals)
    {
        if (sourceStableLocals.Count == 0)
            return ImmutableArray<KeyValuePair<ILocalSymbol, IOperation>>.Empty;

        var ordered = sourceStableLocals
            .OrderBy(static pair => pair.Key.Locations.FirstOrDefault(static location => location.IsInSource)?.SourceTree?.FilePath ?? string.Empty, StringComparer.Ordinal)
            .ThenBy(static pair => pair.Key.Locations.FirstOrDefault(static location => location.IsInSource)?.SourceSpan.Start ?? int.MaxValue)
            .ThenBy(static pair => pair.Key.Name, StringComparer.Ordinal)
            .ToImmutableArray();

        return ordered;
    }

    private static IReadOnlyDictionary<ILocalSymbol, string> CreateLifecycleSourceStableLocalAliases(
        ImmutableArray<KeyValuePair<ILocalSymbol, IOperation>> orderedLocals,
        ImmutableHashSet<ILocalSymbol> firstRenderAliasLocals)
    {
        var aliases = new Dictionary<ILocalSymbol, string>(SymbolEqualityComparer.Default);
        foreach (var pair in orderedLocals)
        {
            aliases[pair.Key] = firstRenderAliasLocals.Contains(pair.Key)
                ? "currentFirstRender"
                : "__jazorLifecycleLocal" + Jazor.Common.Format.HashName(pair.Key.ToDisplayString()).TrimStart('_');
        }

        return aliases;
    }

    private static ImmutableHashSet<ILocalSymbol> CollectLifecycleFirstRenderAliasLocals(
        IReadOnlyDictionary<ILocalSymbol, IOperation> requiredLocals,
        IParameterSymbol firstRenderParameter)
    {
        if (requiredLocals.Count == 0)
            return ImmutableHashSet<ILocalSymbol>.Empty;

        var builder = ImmutableHashSet.CreateBuilder<ILocalSymbol>(SymbolEqualityComparer.Default);
        foreach (var local in requiredLocals.Keys)
        {
            if (IsLifecycleFirstRenderAliasLocal(local, requiredLocals, firstRenderParameter, new HashSet<ILocalSymbol>(SymbolEqualityComparer.Default)))
                builder.Add(local);
        }

        return builder.ToImmutable();
    }

    private static bool IsLifecycleFirstRenderAliasLocal(
        ILocalSymbol local,
        IReadOnlyDictionary<ILocalSymbol, IOperation> requiredLocals,
        IParameterSymbol firstRenderParameter,
        HashSet<ILocalSymbol> visiting)
    {
        if (!visiting.Add(local))
            return false;

        try
        {
            if (!requiredLocals.TryGetValue(local, out var initializer))
                return false;

            var current = RazorVueOperationNormalizer.Unwrap(initializer) ?? initializer;
            return current switch
            {
                IParameterReferenceOperation parameterReference
                    when SymbolEqualityComparer.Default.Equals(parameterReference.Parameter, firstRenderParameter) => true,
                ILocalReferenceOperation localReference => IsLifecycleFirstRenderAliasLocal(
                    localReference.Local,
                    requiredLocals,
                    firstRenderParameter,
                    visiting),
                _ => false
            };
        }
        finally
        {
            visiting.Remove(local);
        }
    }

    private static IReadOnlyDictionary<IMethodSymbol, string> CreateLifecycleLocalFunctionAliases(
        ImmutableArray<IMethodSymbol> orderedLocalFunctions)
    {
        var aliases = new Dictionary<IMethodSymbol, string>(SymbolEqualityComparer.Default);
        foreach (var method in orderedLocalFunctions)
            aliases[method] = "__jazorLifecycleLocalFunction" + Jazor.Common.Format.HashName(method.ToDisplayString()).TrimStart('_');

        return aliases;
    }

    private static IReadOnlyDictionary<ILocalSymbol, string> CreateLifecycleCallableLocalAliases(
        ImmutableArray<ILocalSymbol> orderedCallableLocals)
    {
        var aliases = new Dictionary<ILocalSymbol, string>(SymbolEqualityComparer.Default);
        foreach (var local in orderedCallableLocals)
            aliases[local] = "__jazorLifecycleCallable" + Jazor.Common.Format.HashName(local.ToDisplayString()).TrimStart('_');

        return aliases;
    }

    private ImmutableArray<LifecyclePayloadPreludeBinding> EmitLifecyclePreludeBindings(
        ImmutableArray<KeyValuePair<ILocalSymbol, IOperation>> orderedLocals,
        IReadOnlyDictionary<ILocalSymbol, string> localAliases,
        ImmutableArray<IMethodSymbol> orderedLocalFunctions,
        IReadOnlyDictionary<IMethodSymbol, string> localFunctionAliases,
        ImmutableArray<ILocalSymbol> orderedCallableLocals,
        IReadOnlyDictionary<ILocalSymbol, string> callableLocalAliases,
        IParameterSymbol? firstRenderParameter)
    {
        if (orderedLocals.IsDefaultOrEmpty &&
            orderedLocalFunctions.IsDefaultOrEmpty &&
            orderedCallableLocals.IsDefaultOrEmpty)
        {
            return ImmutableArray<LifecyclePayloadPreludeBinding>.Empty;
        }

        var builder = ImmutableArray.CreateBuilder<LifecyclePayloadPreludeBinding>();
        foreach (var pair in orderedLocals)
        {
            var alias = localAliases[pair.Key];
            var expression = EmitLifecyclePreludeScopedExpression(
                pair.Value,
                orderedLocals,
                localAliases,
                localFunctionAliases,
                callableLocalAliases,
                firstRenderParameter,
                excludedLocal: pair.Key,
                excludedLocalFunction: null,
                excludedCallableLocal: null);
            builder.Add(LifecyclePayloadPreludeBinding.Const(alias, expression));
        }

        foreach (var localFunction in orderedLocalFunctions)
        {
            var alias = localFunctionAliases[localFunction];
            var functionCode = EmitLifecycleLocalFunctionPrelude(
                localFunction,
                alias,
                orderedLocals,
                localAliases,
                localFunctionAliases,
                callableLocalAliases,
                firstRenderParameter);
            builder.Add(LifecyclePayloadPreludeBinding.Statement(functionCode));
        }

        foreach (var callableLocal in orderedCallableLocals)
        {
            var alias = callableLocalAliases[callableLocal];
            var expression = EmitLifecyclePreludeScopedExpression(
                GetLifecycleCallableInitializer(callableLocal),
                orderedLocals,
                localAliases,
                localFunctionAliases,
                callableLocalAliases,
                firstRenderParameter,
                excludedLocal: null,
                excludedLocalFunction: null,
                excludedCallableLocal: callableLocal);
            builder.Add(LifecyclePayloadPreludeBinding.Const(alias, expression));
        }

        return builder.ToImmutable();
    }

    private string EmitLifecyclePreludeScopedExpression(
        IOperation operation,
        ImmutableArray<KeyValuePair<ILocalSymbol, IOperation>> orderedLocals,
        IReadOnlyDictionary<ILocalSymbol, string> localAliases,
        IReadOnlyDictionary<IMethodSymbol, string> localFunctionAliases,
        IReadOnlyDictionary<ILocalSymbol, string> callableLocalAliases,
        IParameterSymbol? firstRenderParameter,
        ILocalSymbol? excludedLocal,
        IMethodSymbol? excludedLocalFunction,
        ILocalSymbol? excludedCallableLocal)
    {
        var remainingInitializers = new Dictionary<ILocalSymbol, IOperation>(SymbolEqualityComparer.Default);
        foreach (var candidate in orderedLocals)
        {
            if (excludedLocal is not null && SymbolEqualityComparer.Default.Equals(candidate.Key, excludedLocal))
                continue;

            remainingInitializers[candidate.Key] = candidate.Value;
        }

        var remainingAliases = new Dictionary<ILocalSymbol, string>(SymbolEqualityComparer.Default);
        foreach (var candidate in localAliases)
        {
            if (excludedLocal is not null && SymbolEqualityComparer.Default.Equals(candidate.Key, excludedLocal))
                continue;

            remainingAliases[candidate.Key] = candidate.Value;
        }

        var remainingFunctionAliases = new Dictionary<IMethodSymbol, string>(SymbolEqualityComparer.Default);
        foreach (var candidate in localFunctionAliases)
        {
            if (excludedLocalFunction is not null && SymbolEqualityComparer.Default.Equals(candidate.Key, excludedLocalFunction))
                continue;

            remainingFunctionAliases[candidate.Key] = candidate.Value;
        }

        var remainingCallableAliases = new Dictionary<ILocalSymbol, string>(SymbolEqualityComparer.Default);
        foreach (var candidate in callableLocalAliases)
        {
            if (excludedCallableLocal is not null && SymbolEqualityComparer.Default.Equals(candidate.Key, excludedCallableLocal))
                continue;

            remainingCallableAliases[candidate.Key] = candidate.Value;
        }

        return WithSourceStableLocalInitializers(
            remainingInitializers,
            () => WithScopedLifecycleCallableAliases(
                remainingCallableAliases,
                remainingFunctionAliases,
                () => WithScopedLocalAliases(
                    remainingAliases,
                    () => WithScopedParameterAliases(
                        CreateLifecycleParameterAliases(firstRenderParameter),
                        () => EmitSetupExpression(operation)))));
    }

    private string EmitLifecycleLocalFunctionPrelude(
        IMethodSymbol localFunction,
        string alias,
        ImmutableArray<KeyValuePair<ILocalSymbol, IOperation>> orderedLocals,
        IReadOnlyDictionary<ILocalSymbol, string> localAliases,
        IReadOnlyDictionary<IMethodSymbol, string> localFunctionAliases,
        IReadOnlyDictionary<ILocalSymbol, string> callableLocalAliases,
        IParameterSymbol? firstRenderParameter)
    {
        var syntaxReference = localFunction.DeclaringSyntaxReferences.FirstOrDefault();
        if (syntaxReference?.GetSyntax() is not LocalFunctionStatementSyntax localFunctionSyntax)
        {
            throw new NotSupportedException(
                $"RazorVue lifecycle payload does not support local function '{localFunction.Name}' without source syntax.");
        }

        var semanticModel = _snapshot.Compilation.GetSemanticModel(localFunctionSyntax.SyntaxTree);
        if (semanticModel.GetOperation(localFunctionSyntax) is not ILocalFunctionOperation localFunctionOperation)
        {
            throw new NotSupportedException(
                $"RazorVue lifecycle payload could not resolve local function operation for '{localFunction.Name}'.");
        }

        ThrowIfReadOnlyByRefParameterEscapes(localFunctionOperation);

        var parameterAliases = ImmutableDictionary.CreateBuilder<IParameterSymbol, string>(SymbolEqualityComparer.Default);
        foreach (var parameter in localFunction.Parameters)
            parameterAliases[parameter] = parameter.Name;
        if (firstRenderParameter is not null)
            parameterAliases[firstRenderParameter] = "currentFirstRender";

        var functionNode = WithSourceStableLocalInitializers(
            BuildOrderedLocalInitializerDictionary(orderedLocals),
            () => WithScopedLifecycleCallableAliases(
                callableLocalAliases,
                localFunctionAliases,
                () => WithScopedLocalAliases(
                    localAliases,
                    () => WithScopedParameterAliases(
                        parameterAliases.ToImmutable(),
                        () => _semanticWalker.Visit(localFunctionOperation, _compilerArgument)))));

        if (functionNode is not FunctionDeclaration functionDeclaration)
        {
            throw new NotSupportedException(
                $"RazorVue lifecycle payload expected local function '{localFunction.Name}' to lower as a function declaration.");
        }

        var aliasDeclaration = new FunctionDeclaration(
            new Identifier(alias),
            functionDeclaration.Params,
            functionDeclaration.Body,
            functionDeclaration.Generator,
            functionDeclaration.Async);
        return MaterializeCompilerStatement(aliasDeclaration, _compilerArgument);
    }

    private static ImmutableDictionary<IParameterSymbol, string> CreateLifecycleParameterAliases(
        IParameterSymbol? firstRenderParameter)
        => firstRenderParameter is null
            ? ImmutableDictionary<IParameterSymbol, string>.Empty.WithComparers(SymbolEqualityComparer.Default)
            : ImmutableDictionary.Create<IParameterSymbol, string>(SymbolEqualityComparer.Default)
                .Add(firstRenderParameter, "currentFirstRender");

    private IOperation GetLifecycleCallableInitializer(ILocalSymbol local)
    {
        if (!RazorVueSourceStableLocalInitializerHelper.TryGetSourceStableLocalInitializer(
                _snapshot.Compilation,
                local,
                static type => type is not null && type.TypeKind != TypeKind.Error,
                out var initializer) ||
            initializer is null)
        {
            throw new NotSupportedException(
                $"RazorVue lifecycle payload could not resolve callable local initializer for '{local.Name}'.");
        }

        return initializer;
    }

    private IOperation GetLifecycleLocalFunctionOperation(IMethodSymbol localFunction)
    {
        var syntaxReference = localFunction.DeclaringSyntaxReferences.FirstOrDefault();
        if (syntaxReference?.GetSyntax() is not LocalFunctionStatementSyntax localFunctionSyntax)
        {
            throw new NotSupportedException(
                $"RazorVue lifecycle payload does not support local function '{localFunction.Name}' without source syntax.");
        }

        var semanticModel = _snapshot.Compilation.GetSemanticModel(localFunctionSyntax.SyntaxTree);
        if (semanticModel.GetOperation(localFunctionSyntax) is not ILocalFunctionOperation localFunctionOperation)
        {
            throw new NotSupportedException(
                $"RazorVue lifecycle payload could not resolve local function operation for '{localFunction.Name}'.");
        }

        return localFunctionOperation;
    }

    private static IReadOnlyDictionary<ILocalSymbol, IOperation> BuildOrderedLocalInitializerDictionary(
        ImmutableArray<KeyValuePair<ILocalSymbol, IOperation>> orderedLocals)
    {
        var dictionary = new Dictionary<ILocalSymbol, IOperation>(SymbolEqualityComparer.Default);
        foreach (var pair in orderedLocals)
            dictionary[pair.Key] = pair.Value;

        return dictionary;
    }

    private readonly record struct LifecycleCompilerFallbackDependencies(
        IReadOnlyDictionary<ILocalSymbol, IOperation> SourceStableLocals,
        ImmutableArray<IMethodSymbol> LocalFunctions,
        ImmutableArray<ILocalSymbol> CallableLocals);

    private bool MayContainLifecycleFirstRenderReference(
        IMethodSymbol lifecycleMethod,
        IOperation operation,
        IReadOnlyDictionary<ILocalSymbol, IOperation> sourceStableLocals,
        LifecycleCompilerFallbackDependencies dependencies)
    {
        var firstRenderParameter = lifecycleMethod.Parameters.FirstOrDefault(static parameter => parameter.Name == "firstRender");
        if (firstRenderParameter is null)
            return false;

        var visitedLocals = new HashSet<ILocalSymbol>(SymbolEqualityComparer.Default);
        var visitedLocalFunctions = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
        var visitedCallableLocals = new HashSet<ILocalSymbol>(SymbolEqualityComparer.Default);
        var localFunctions = new HashSet<IMethodSymbol>(dependencies.LocalFunctions, SymbolEqualityComparer.Default);
        var callableLocals = new HashSet<ILocalSymbol>(dependencies.CallableLocals, SymbolEqualityComparer.Default);

        return contains(operation);

        bool contains(IOperation currentOperation)
        {
            foreach (var descendant in currentOperation.DescendantsAndSelf())
            {
                switch (descendant)
                {
                    case IParameterReferenceOperation parameterReference
                        when SymbolEqualityComparer.Default.Equals(parameterReference.Parameter, firstRenderParameter):
                        return true;

                    case ILocalReferenceOperation localReference when sourceStableLocals.TryGetValue(localReference.Local, out var initializer):
                        if (!visitedLocals.Add(localReference.Local))
                            continue;

                        try
                        {
                            if (contains(initializer))
                                return true;
                        }
                        finally
                        {
                            visitedLocals.Remove(localReference.Local);
                        }

                        continue;

                    case ILocalReferenceOperation localReference when callableLocals.Contains(localReference.Local):
                        if (!visitedCallableLocals.Add(localReference.Local))
                            continue;

                        try
                        {
                            if (contains(GetLifecycleCallableInitializer(localReference.Local)))
                                return true;
                        }
                        finally
                        {
                            visitedCallableLocals.Remove(localReference.Local);
                        }

                        continue;

                    case IInvocationOperation invocation when localFunctions.Contains(invocation.TargetMethod):
                        if (!visitedLocalFunctions.Add(invocation.TargetMethod))
                            continue;

                        try
                        {
                            if (contains(GetLifecycleLocalFunctionOperation(invocation.TargetMethod)))
                                return true;
                        }
                        finally
                        {
                            visitedLocalFunctions.Remove(invocation.TargetMethod);
                        }

                        continue;

                    case IMethodReferenceOperation methodReference when localFunctions.Contains(methodReference.Method):
                        if (!visitedLocalFunctions.Add(methodReference.Method))
                            continue;

                        try
                        {
                            if (contains(GetLifecycleLocalFunctionOperation(methodReference.Method)))
                                return true;
                        }
                        finally
                        {
                            visitedLocalFunctions.Remove(methodReference.Method);
                        }

                        continue;
                }
            }

            return false;
        }
    }

    private static bool ValidateLifecycleCompilerPayloadShape(
        IOperation operation,
        IReadOnlyDictionary<ILocalSymbol, IOperation> sourceStableLocals)
    {
        var visitedLocals = new HashSet<ILocalSymbol>(SymbolEqualityComparer.Default);
        var patternDeclaredLocals = CollectPatternDeclaredLocals(operation);
        foreach (var descendant in operation.DescendantsAndSelf())
        {
            switch (descendant)
            {
                case ILocalReferenceOperation localReference:
                    if (sourceStableLocals.TryGetValue(localReference.Local, out var initializer))
                    {
                        if (localReference.Local.Type?.TypeKind == TypeKind.Delegate)
                            continue;

                        if (!visitedLocals.Add(localReference.Local))
                            return false;

                        try
                        {
                            if (!ValidateLifecycleCompilerPayloadShape(initializer, sourceStableLocals))
                                return false;
                        }
                        finally
                        {
                            visitedLocals.Remove(localReference.Local);
                        }

                        continue;
                    }
                    if (localReference.Local.Type?.TypeKind == TypeKind.Delegate)
                        continue;
                    if (patternDeclaredLocals.Contains(localReference.Local))
                        continue;

                    return false;
                case IAnonymousFunctionOperation:
                case IDeclarationExpressionOperation:
                case IVariableDeclaratorOperation:
                    return false;
            }

            if (descendant.Kind is OperationKind.FlowAnonymousFunction or OperationKind.FlowCapture)
                return false;
        }

        return true;
    }

    private static HashSet<ILocalSymbol> CollectPatternDeclaredLocals(IOperation operation)
    {
        var result = new HashSet<ILocalSymbol>(SymbolEqualityComparer.Default);
        foreach (var descendant in operation.DescendantsAndSelf())
        {
            switch (descendant)
            {
                case IDeclarationPatternOperation declarationPattern when declarationPattern.DeclaredSymbol is ILocalSymbol declarationLocal:
                    result.Add(declarationLocal);
                    break;
                case IRecursivePatternOperation recursivePattern when recursivePattern.DeclaredSymbol is ILocalSymbol recursiveLocal:
                    result.Add(recursiveLocal);
                    break;
                case IListPatternOperation listPattern when listPattern.DeclaredSymbol is ILocalSymbol listLocal:
                    result.Add(listLocal);
                    break;
            }
        }

        return result;
    }


    private IReadOnlyList<IOperation> GetLifecycleMethodOperations(IMethodSymbol method)
    {
        foreach (var syntaxReference in method.DeclaringSyntaxReferences)
        {
            if (syntaxReference.GetSyntax() is not MethodDeclarationSyntax methodSyntax)
                continue;

            var semanticModel = _snapshot.Compilation.GetSemanticModel(methodSyntax.SyntaxTree);
            if (methodSyntax.Body is not null &&
                semanticModel.GetOperation(methodSyntax.Body) is IBlockOperation blockOperation)
            {
                return blockOperation.Operations;
            }

            if (methodSyntax.ExpressionBody?.Expression is { } expressionSyntax &&
                semanticModel.GetOperation(expressionSyntax) is { } expressionOperation)
            {
                return [expressionOperation];
            }
        }

        return [];
    }

    private LifecyclePayloadEmission EmitLifecyclePayloadPropertyReference(
        IMethodSymbol method,
        IPropertyReferenceOperation property)
    {
        if (IsCurrentComponentMember(method.ContainingType, property.Property, property.Instance) &&
            IsComponentParameterProperty(property.Property))
        {
            return new LifecyclePayloadEmission("props." + ToLifecyclePropName(property.Property.Name), false);
        }

        if (IsCurrentComponentMember(method.ContainingType, property.Property, property.Instance) &&
            TryEmitLifecycleCurrentComponentPropertyReference(property.Property, out var lifecyclePropertyExpression))
        {
            return new LifecyclePayloadEmission(lifecyclePropertyExpression, false);
        }

        throw new NotSupportedException(
            $"RazorVue lifecycle payload only supports component [Parameter] properties or source-stable current-component value members. Unsupported member: '{property.Property.Name}'.");
    }

    private LifecyclePayloadEmission EmitLifecyclePayloadFieldReference(
        IMethodSymbol method,
        IFieldReferenceOperation field)
    {
        if (IsCurrentComponentMember(method.ContainingType, field.Field, field.Instance) &&
            TryEmitLifecycleCurrentComponentFieldReference(field.Field, out var lifecycleFieldExpression))
        {
            return new LifecyclePayloadEmission(lifecycleFieldExpression, false);
        }

        throw new NotSupportedException(
            $"RazorVue lifecycle payload only supports source-stable current-component fields. Unsupported member: '{field.Field.Name}'.");
    }

    private LifecyclePayloadEmission EmitLifecyclePayloadUnary(
        IMethodSymbol method,
        IUnaryOperation unary,
        bool allowFirstRenderPayload)
    {
        var operand = EmitLifecyclePayloadCore(method, unary.Operand, allowFirstRenderPayload);
        return new LifecyclePayloadEmission(GetUnaryOperator(unary.OperatorKind) + operand.Expression, operand.UsesFirstRender);
    }

    private LifecyclePayloadEmission EmitLifecyclePayloadBinary(
        IMethodSymbol method,
        IBinaryOperation binary,
        bool allowFirstRenderPayload)
    {
        var left = EmitLifecyclePayloadCore(method, binary.LeftOperand, allowFirstRenderPayload);
        var right = EmitLifecyclePayloadCore(method, binary.RightOperand, allowFirstRenderPayload);
        return new LifecyclePayloadEmission(
            "(" + left.Expression + " " + GetBinaryOperator(binary.OperatorKind) + " " + right.Expression + ")",
            left.UsesFirstRender || right.UsesFirstRender);
    }

    private LifecyclePayloadEmission EmitLifecyclePayloadConditional(
        IMethodSymbol method,
        IConditionalOperation conditional,
        bool allowFirstRenderPayload)
    {
        var condition = EmitLifecyclePayloadCore(method, conditional.Condition, allowFirstRenderPayload);
        var whenTrue = EmitLifecyclePayloadCore(method, conditional.WhenTrue, allowFirstRenderPayload);
        var whenFalse = EmitLifecyclePayloadCore(method, conditional.WhenFalse, allowFirstRenderPayload);
        return new LifecyclePayloadEmission(
            "(" + condition.Expression + " ? " + whenTrue.Expression + " : " + whenFalse.Expression + ")",
            condition.UsesFirstRender || whenTrue.UsesFirstRender || whenFalse.UsesFirstRender);
    }

    private LifecyclePayloadEmission EmitLifecyclePayloadInterpolatedString(
        IMethodSymbol method,
        IInterpolatedStringOperation interpolated,
        bool allowFirstRenderPayload)
    {
        var builder = new StringBuilder();
        var usesFirstRender = false;
        builder.Append('`');
        foreach (var part in interpolated.Parts)
        {
            switch (part)
            {
                case IInterpolatedStringTextOperation text:
                    builder.Append(EscapeTemplateText(text.Text.ConstantValue.HasValue && text.Text.ConstantValue.Value is string value ? value : string.Empty));
                    break;
                case IInterpolationOperation interpolation:
                    var expression = EmitLifecyclePayloadCore(method, interpolation.Expression, allowFirstRenderPayload);
                    builder.Append("${").Append(expression.Expression).Append('}');
                    usesFirstRender |= expression.UsesFirstRender;
                    break;
            }
        }

        builder.Append('`');
        return new LifecyclePayloadEmission(builder.ToString(), usesFirstRender);
    }

    private LifecyclePayloadEmission EmitLifecyclePayloadInvocation(
        IMethodSymbol lifecycleMethod,
        IInvocationOperation invocation,
        bool allowFirstRenderPayload)
    {
        var normalizedCallbackFactory = TryNormalizeRazorGeneratedCallbackFactory(invocation);
        if (normalizedCallbackFactory.Length != 0)
            return new LifecyclePayloadEmission(normalizedCallbackFactory, false);

        if (TryNormalizeRazorInferredEventCallback(invocation, out var normalizedInferredCallback))
            return new LifecyclePayloadEmission(normalizedInferredCallback, false);

        if (TryNormalizeCurrentComponentCallbackInvokeAsync(invocation, useSetupEmitter: true, compilerArgument: null, out var normalizedCallbackInvoke))
            return new LifecyclePayloadEmission(normalizedCallbackInvoke, false);

        if (invocation.Instance is null &&
            invocation.TargetMethod.MethodKind == MethodKind.LocalFunction &&
            _scopedLifecycleLocalFunctionAliases is not null &&
            _scopedLifecycleLocalFunctionAliases.TryGetValue(invocation.TargetMethod, out var localFunctionAlias))
        {
            return EmitLifecyclePayloadBoundInvocation(
                invocation.TargetMethod,
                invocation.Arguments,
                operation => EmitLifecyclePayloadCore(
                    lifecycleMethod,
                    operation,
                    allowFirstRenderPayload,
                    allowPureCompilerOwnedFallback: true),
                _ => localFunctionAlias,
                () => new NotSupportedException(
                    $"RazorVue lifecycle payload does not support local function invocation '{invocation.TargetMethod.Name}' in component '{lifecycleMethod.ContainingType.ToDisplayString()}'."));
        }

        if (invocation.Instance is not null && invocation.TargetMethod.Name == "Invoke")
        {
            var targetExpression = EmitLifecyclePayloadInvocationTarget(lifecycleMethod, invocation.Instance, allowFirstRenderPayload);
            return EmitLifecyclePayloadBoundInvocation(
                invocation.TargetMethod,
                invocation.Arguments,
                operation => EmitLifecyclePayloadCore(
                    lifecycleMethod,
                    operation,
                    allowFirstRenderPayload,
                    allowPureCompilerOwnedFallback: true),
                _ => targetExpression,
                () => new NotSupportedException(
                    $"RazorVue lifecycle payload does not support delegate invocation '{invocation.TargetMethod.Name}' in component '{lifecycleMethod.ContainingType.ToDisplayString()}'."));
        }

        if (IsCurrentComponentMember(invocation.TargetMethod, invocation.Instance))
        {
            return EmitLifecyclePayloadCurrentComponentHelperInvocation(lifecycleMethod, invocation, allowFirstRenderPayload);
        }

        throw new NotSupportedException(
            $"RazorVue lifecycle payload does not support invocation '{invocation.TargetMethod.ToDisplayString()}' in component '{lifecycleMethod.ContainingType.ToDisplayString()}'.");
    }

    private LifecyclePayloadEmission EmitLifecyclePayloadCurrentComponentHelperInvocation(
        IMethodSymbol lifecycleMethod,
        IInvocationOperation invocation,
        bool allowFirstRenderPayload)
    {
        if (IsUnsupportedSetupHelperMethod(invocation.TargetMethod))
        {
            throw new NotSupportedException(
                $"RazorVue lifecycle payload does not support async/task current-component helper '{invocation.TargetMethod.Name}'.");
        }

        RecordRequiredSetupMethod(invocation.TargetMethod);
        return EmitLifecyclePayloadBoundInvocation(
            invocation.TargetMethod,
            invocation.Arguments,
            operation => EmitLifecyclePayloadCore(
                lifecycleMethod,
                operation,
                allowFirstRenderPayload,
                allowPureCompilerOwnedFallback: true),
            target => ToLowerCamelCase(target.Name),
            () => new NotSupportedException(
                $"RazorVue lifecycle payload does not support current-component helper '{invocation.TargetMethod.Name}' in component '{lifecycleMethod.ContainingType.ToDisplayString()}'."));
    }

    private string EmitLifecyclePayloadInvocationTarget(
        IMethodSymbol lifecycleMethod,
        IOperation target,
        bool allowFirstRenderPayload)
    {
        if (TryEmitLifecyclePayloadTargetExpression(lifecycleMethod, target, allowFirstRenderPayload, out var expression))
            return expression;

        throw new NotSupportedException(
            $"RazorVue lifecycle payload does not support invocation target '{target.Kind}' in component '{lifecycleMethod.ContainingType.ToDisplayString()}'.");
    }

    private bool TryEmitLifecyclePayloadTargetExpression(
        IMethodSymbol lifecycleMethod,
        IOperation operation,
        bool allowFirstRenderPayload,
        out string expression)
    {
        var current = Unwrap(operation);
        expression = string.Empty;
        if (current is null)
            return false;

        switch (current)
        {
            case IParameterReferenceOperation parameter when IsFirstRenderPayloadParameter(lifecycleMethod, parameter, allowFirstRenderPayload):
                expression = LifecycleFirstRenderPlaceholder;
                return true;
            case ILocalReferenceOperation localReference when _scopedLifecycleCallableAliases is not null &&
                                                             _scopedLifecycleCallableAliases.TryGetValue(localReference.Local, out expression):
                return true;
            case ILocalReferenceOperation localReference when _scopedLocalAliases is not null &&
                                                             _scopedLocalAliases.TryGetValue(localReference.Local, out expression):
                return true;
            case IPropertyReferenceOperation property:
                if (!TryEmitLifecyclePayloadTargetPropertyReference(lifecycleMethod, property, allowFirstRenderPayload, out expression))
                    return false;

                return true;
            case IFieldReferenceOperation field:
                if (!TryEmitLifecyclePayloadTargetFieldReference(lifecycleMethod, field, allowFirstRenderPayload, out expression))
                    return false;

                return true;
            case IMethodReferenceOperation methodReference when methodReference.Instance is null &&
                                                               _scopedLifecycleLocalFunctionAliases is not null &&
                                                               _scopedLifecycleLocalFunctionAliases.TryGetValue(methodReference.Method, out expression):
                return true;
            case IInvocationOperation invocation:
                var invocationEmission = EmitLifecyclePayloadInvocation(lifecycleMethod, invocation, allowFirstRenderPayload);
                expression = invocationEmission.Expression;
                return true;
            case IUnaryOperation unary:
                var unaryEmission = EmitLifecyclePayloadUnary(lifecycleMethod, unary, allowFirstRenderPayload);
                expression = unaryEmission.Expression;
                return true;
            case IBinaryOperation binary:
                var binaryEmission = EmitLifecyclePayloadBinary(lifecycleMethod, binary, allowFirstRenderPayload);
                expression = binaryEmission.Expression;
                return true;
            case IConditionalOperation conditional when conditional.WhenTrue is not null && conditional.WhenFalse is not null:
                var conditionalEmission = EmitLifecyclePayloadConditional(lifecycleMethod, conditional, allowFirstRenderPayload);
                expression = conditionalEmission.Expression;
                return true;
            case IInterpolatedStringOperation interpolated:
                var interpolatedEmission = EmitLifecyclePayloadInterpolatedString(lifecycleMethod, interpolated, allowFirstRenderPayload);
                expression = interpolatedEmission.Expression;
                return true;
            case ILiteralOperation literal:
                expression = EmitLiteral(literal);
                return true;
            case IDefaultValueOperation defaultValue when IsNullDefaultValue(defaultValue):
                expression = "null";
                return true;
            default:
                return false;
        }
    }

    private bool TryEmitLifecyclePayloadTargetPropertyReference(
        IMethodSymbol lifecycleMethod,
        IPropertyReferenceOperation property,
        bool allowFirstRenderPayload,
        out string expression)
    {
        expression = string.Empty;
        if (TryEmitKnownAliasedProperty(property, useSetupEmitter: true, compilerArgument: null, out expression))
            return true;

        if (IsCurrentComponentMember(lifecycleMethod.ContainingType, property.Property, property.Instance))
        {
            if (IsComponentParameterProperty(property.Property))
            {
                expression = "props." + ToLifecyclePropName(property.Property.Name);
                return true;
            }

            return TryEmitLifecycleCurrentComponentPropertyReference(property.Property, out expression);
        }

        if (property.Instance is null || property.Arguments.Length != 0 || property.Property.IsIndexer)
            return false;

        if (!TryEmitLifecyclePayloadTargetExpression(lifecycleMethod, property.Instance, allowFirstRenderPayload, out var instanceExpression))
            return false;

        expression = instanceExpression + "." + ResolveMemberName(property.Property);
        return true;
    }

    private bool TryEmitLifecyclePayloadTargetFieldReference(
        IMethodSymbol lifecycleMethod,
        IFieldReferenceOperation field,
        bool allowFirstRenderPayload,
        out string expression)
    {
        expression = string.Empty;
        if (IsCurrentComponentMember(lifecycleMethod.ContainingType, field.Field, field.Instance))
            return TryEmitLifecycleCurrentComponentFieldReference(field.Field, out expression);

        if (field.Instance is null)
            return false;

        if (!TryEmitLifecyclePayloadTargetExpression(lifecycleMethod, field.Instance, allowFirstRenderPayload, out var instanceExpression))
            return false;

        expression = instanceExpression + "." + ResolveMemberName(field.Field);
        return true;
    }

    private string EmitPropertyReference(IPropertyReferenceOperation property)
    {
        if (TryEmitKnownAliasedProperty(property, useSetupEmitter: false, compilerArgument: null, out var alias))
            return alias;

        if (IsCurrentComponentMember(property.Property, property.Instance))
        {
            if (_propsByPublicName.TryGetValue(property.Property.Name, out var prop))
                return "props." + prop.Name;

            if (_slotsByPublicName.TryGetValue(property.Property.Name, out var slot))
                return EmitCurrentComponentSlotReference(slot);

            if (_emitsByRazorAlias.ContainsKey(property.Property.Name))
                return EmitCurrentComponentCallbackReference(property.Property);

            if (_logicPropertiesByName.TryGetValue(property.Property.Name, out var logicProperty) &&
                RazorVueSymbolIdentity.SameMember(logicProperty.PropertySymbol, property.Property))
            {
                EnsureSupportedRenderPropertyReference(property);

                RecordRequiredSetupProperty(property.Property);
                return logicProperty.LoweringKind == VueLogicPropertyLoweringKind.ValueBinding
                    ? ToLowerCamelCase(property.Property.Name)
                    : ToLowerCamelCase(property.Property.Name) + "()";
            }

            throw new NotSupportedException(
                $"RazorVue render currently only supports parameter properties in template expressions. Unsupported member: '{property.Property.Name}'.");
        }

        return EmitMemberTarget(property.Instance) + "." + ResolveMemberName(property.Property);
    }

    internal bool TryRewritePropertyReference(
        IPropertyReferenceOperation property,
        SenseArgument argument,
        bool useSetupEmitter,
        out string expression)
    {
        if (_imperativeBuilderAlias is not null &&
            RazorVueComponentTypeCarrierHelper.TryGetInvalidatedSourceStableComponentTypeMember(
                _snapshot.Compilation,
                _snapshot.ComponentSymbol,
                property,
                out var invalidatedComponentTypeMemberCarrier))
        {
            throw CreateImperativeComponentTypeMemberCarrierInvalidatedException(property, invalidatedComponentTypeMemberCarrier);
        }

        if (_imperativeBuilderAlias is not null &&
            RazorVueComponentTypeCarrierHelper.TryResolveSourceStableVueComponentTypeMember(
                _snapshot.Compilation,
                _snapshot.ComponentSymbol,
                property,
                out var componentTypeMemberCarrier,
                out _))
        {
            throw CreateImperativeComponentTypeMemberCarrierRuntimeValueException(property, componentTypeMemberCarrier);
        }

        if (useSetupEmitter)
        {
            if (TryEmitKnownAliasedProperty(property, useSetupEmitter: true, argument, out expression))
                return true;

            if (IsCurrentComponentMember(property.Property, property.Instance))
            {
                expression = EmitSetupPropertyReference(property, argument);
                return true;
            }

            expression = string.Empty;
            return false;
        }

        if (TryEmitKnownAliasedProperty(property, useSetupEmitter: false, argument, out expression))
            return true;

        if (IsCurrentComponentMember(property.Property, property.Instance))
        {
            if (_propsByPublicName.TryGetValue(property.Property.Name, out var prop))
            {
                expression = "props." + prop.Name;
                return true;
            }

            if (_slotsByPublicName.TryGetValue(property.Property.Name, out var slot))
            {
                expression = EmitCurrentComponentSlotReference(slot);
                return true;
            }

            if (_emitsByRazorAlias.ContainsKey(property.Property.Name))
            {
                expression = EmitCurrentComponentCallbackReference(property.Property);
                return true;
            }

            if (_logicPropertiesByName.TryGetValue(property.Property.Name, out var logicProperty) &&
                RazorVueSymbolIdentity.SameMember(logicProperty.PropertySymbol, property.Property))
            {
                EnsureSupportedRenderPropertyReference(property);

                RecordRequiredSetupProperty(property.Property);
                expression = logicProperty.LoweringKind == VueLogicPropertyLoweringKind.ValueBinding
                    ? ToLowerCamelCase(property.Property.Name)
                    : ToLowerCamelCase(property.Property.Name) + "()";
                return true;
            }

            throw new NotSupportedException(
                $"RazorVue render currently only supports parameter properties in template expressions. Unsupported member: '{property.Property.Name}'.");
        }

        if (TryEmitTemplateDataPropertyProjection(property, argument, out expression))
            return true;

        expression = string.Empty;
        return false;
    }

    private bool TryEmitTemplateDataPropertyProjection(
        IPropertyReferenceOperation property,
        SenseArgument argument,
        out string expression)
    {
        expression = string.Empty;
        var instance = Unwrap(property.Instance);
        if (instance is null)
            return false;

        if (property.Arguments.Length != 0 || property.Property.IsStatic || property.Property.IsIndexer)
            return false;

        if (!IsTemplateDataProjectionCarrier(instance.Type))
            return false;

        expression = EmitExpression(instance, argument) + "." + ResolveMemberName(property.Property);
        return true;
    }

    private static bool IsTemplateDataProjectionCarrier(ITypeSymbol? type)
    {
        if (type is null)
            return false;

        var original = type.OriginalDefinition;
        if (original is INamedTypeSymbol namedOriginal && Util.IsHostErasedUnionType(namedOriginal))
            return false;

        if (original.TypeKind is TypeKind.Array or TypeKind.Delegate or TypeKind.TypeParameter ||
            type.IsTupleType ||
            original.IsAnonymousType ||
            original.SpecialType != SpecialType.None)
        {
            return false;
        }

        return original.TypeKind is TypeKind.Class or TypeKind.Struct;
    }

    private string EmitSetupPropertyReference(IPropertyReferenceOperation property, SenseArgument? compilerArgument = null)
    {
        if (TryEmitKnownAliasedProperty(property, useSetupEmitter: true, compilerArgument, out var alias))
            return alias;

        if (IsCurrentComponentMember(property.Property, property.Instance))
        {
            if (_propsByPublicName.TryGetValue(property.Property.Name, out var prop))
                return "props." + prop.Name;

            if (_slotsByPublicName.TryGetValue(property.Property.Name, out var slot))
                return EmitCurrentComponentSlotReference(slot);

            if (_emitsByRazorAlias.ContainsKey(property.Property.Name))
                return EmitCurrentComponentCallbackReference(property.Property);

            if (_logicPropertiesByName.TryGetValue(property.Property.Name, out var logicProperty) &&
                RazorVueSymbolIdentity.SameMember(logicProperty.PropertySymbol, property.Property))
            {
                EnsureSupportedRenderPropertyReference(property);

                RecordRequiredSetupProperty(property.Property);
                return logicProperty.LoweringKind == VueLogicPropertyLoweringKind.ValueBinding
                    ? ToLowerCamelCase(property.Property.Name)
                    : ToLowerCamelCase(property.Property.Name) + "()";
            }

            throw CreateUnsupportedSetupLogicException(
                property.Property,
                $"RazorVue setup-side logic only supports component [Parameter] properties. Unsupported member: '{property.Property.Name}'.");
        }

        return EmitMemberTarget(property.Instance, compilerArgument) + "." + ResolveMemberName(property.Property);
    }

    private bool TryEmitKnownAliasedProperty(
        IPropertyReferenceOperation property,
        bool useSetupEmitter,
        SenseArgument? compilerArgument,
        out string expression)
    {
        expression = string.Empty;
        if (property.Instance is null)
            return false;

        if (!string.Equals(property.Property.Name, "Count", StringComparison.Ordinal))
            return false;

        if (!IsArrayLikeCountCarrier(property.Property.ContainingType))
            return false;

        expression = (useSetupEmitter
            ? EmitSetupExpression(property.Instance, compilerArgument)
            : EmitExpression(property.Instance, compilerArgument)) + ".length";
        return true;
    }

    private static bool IsArrayLikeCountCarrier(ITypeSymbol? type)
    {
        if (type is null)
            return false;

        var displayName = type.OriginalDefinition.ToDisplayString();
        return string.Equals(displayName, "System.Collections.Generic.List<T>", StringComparison.Ordinal) ||
               string.Equals(displayName, "System.Collections.Generic.IList<T>", StringComparison.Ordinal) ||
               string.Equals(displayName, "System.Collections.Generic.ICollection<T>", StringComparison.Ordinal) ||
               string.Equals(displayName, "System.Collections.Generic.IReadOnlyList<T>", StringComparison.Ordinal) ||
               string.Equals(displayName, "System.Collections.Generic.IReadOnlyCollection<T>", StringComparison.Ordinal) ||
               string.Equals(displayName, "System.Collections.ICollection", StringComparison.Ordinal) ||
               string.Equals(displayName, "System.Collections.ObjectModel.ReadOnlyCollection<T>", StringComparison.Ordinal);
    }

    private string EmitFieldReference(IFieldReferenceOperation field)
    {
        if (IsCurrentComponentMember(field.Field, field.Instance))
        {
            if (_logicFieldsByName.ContainsKey(field.Field.Name))
            {
                EnsureSupportedRenderFieldReference(field);

                RecordRequiredSetupField(field.Field);
                return ToLowerCamelCase(field.Field.Name);
            }

            throw new NotSupportedException(
                $"RazorVue render currently does not support component field '{field.Field.Name}' in component '{_snapshot.Descriptor.FullName}'.");
        }

        return EmitMemberTarget(field.Instance) + "." + ResolveMemberName(field.Field);
    }

    internal bool TryRewriteFieldReference(
        IFieldReferenceOperation field,
        SenseArgument argument,
        bool useSetupEmitter,
        out string expression)
    {
        if (_imperativeBuilderAlias is not null &&
            RazorVueComponentTypeCarrierHelper.TryGetInvalidatedSourceStableComponentTypeMember(
                _snapshot.Compilation,
                _snapshot.ComponentSymbol,
                field,
                out var invalidatedComponentTypeMemberCarrier))
        {
            throw CreateImperativeComponentTypeMemberCarrierInvalidatedException(field, invalidatedComponentTypeMemberCarrier);
        }

        if (_imperativeBuilderAlias is not null &&
            RazorVueComponentTypeCarrierHelper.TryResolveSourceStableVueComponentTypeMember(
                _snapshot.Compilation,
                _snapshot.ComponentSymbol,
                field,
                out var componentTypeMemberCarrier,
                out _))
        {
            throw CreateImperativeComponentTypeMemberCarrierRuntimeValueException(field, componentTypeMemberCarrier);
        }

        if (useSetupEmitter)
        {
            if (IsCurrentComponentMember(field.Field, field.Instance))
            {
                expression = EmitSetupFieldReference(field, argument);
                return true;
            }

            expression = string.Empty;
            return false;
        }

        if (IsCurrentComponentMember(field.Field, field.Instance))
        {
            if (_logicFieldsByName.ContainsKey(field.Field.Name))
            {
                EnsureSupportedRenderFieldReference(field);

                RecordRequiredSetupField(field.Field);
                expression = ToLowerCamelCase(field.Field.Name);
                return true;
            }

            throw new NotSupportedException(
                $"RazorVue render currently does not support component field '{field.Field.Name}' in component '{_snapshot.Descriptor.FullName}'.");
        }

        expression = string.Empty;
        return false;
    }

    private string EmitSetupFieldReference(IFieldReferenceOperation field, SenseArgument? compilerArgument = null)
    {
        if (IsCurrentComponentMember(field.Field, field.Instance))
        {
            if (_logicFieldsByName.ContainsKey(field.Field.Name))
            {
                EnsureSupportedRenderFieldReference(field);

                RecordRequiredSetupField(field.Field);
                return ToLowerCamelCase(field.Field.Name);
            }

            throw CreateUnsupportedSetupLogicException(
                field.Field,
                $"RazorVue setup-side logic does not support component field '{field.Field.Name}'.");
        }

        return EmitMemberTarget(field.Instance, compilerArgument) + "." + ResolveMemberName(field.Field);
    }

    private static bool IsNullDefaultValue(IDefaultValueOperation defaultValue)
    {
        var type = defaultValue.Type;
        if (type is null)
            return false;

        if (type.IsReferenceType)
            return true;

        return type is INamedTypeSymbol namedType &&
               namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T;
    }

    private static bool IsFirstRenderPayloadParameter(
        IMethodSymbol method,
        IParameterReferenceOperation parameter,
        bool allowFirstRenderPayload)
    {
        if (!allowFirstRenderPayload)
            return false;

        return method.Parameters.Any(candidate =>
            candidate.Name == "firstRender" &&
            SymbolEqualityComparer.Default.Equals(candidate, parameter.Parameter));
    }

    private static bool IsCurrentComponentMember(
        INamedTypeSymbol componentSymbol,
        ISymbol symbol,
        IOperation? instance)
        => RazorVueSymbolIdentity.IsCurrentComponentMember(
            componentSymbol,
            symbol,
            instance,
            Unwrap);

    private static bool IsComponentParameterProperty(IPropertySymbol property)
        => property.GetAttributes().Any(static attribute =>
            string.Equals(
                attribute.AttributeClass?.ToDisplayString(),
                "Microsoft.AspNetCore.Components.ParameterAttribute",
                StringComparison.Ordinal));

    private bool TryEmitLifecycleCurrentComponentPropertyReference(
        IPropertySymbol property,
        out string expression)
    {
        expression = string.Empty;
        if (!_logicPropertiesByName.TryGetValue(property.Name, out var logicProperty) ||
            !RazorVueSymbolIdentity.SameMember(logicProperty.PropertySymbol, property))
        {
            return false;
        }

        if (logicProperty.LoweringKind == VueLogicPropertyLoweringKind.ValueBinding &&
            !IsSupportedRenderPropertyReference(property))
        {
            return false;
        }

        if (logicProperty.LoweringKind is not (VueLogicPropertyLoweringKind.ValueBinding or VueLogicPropertyLoweringKind.GetterFunction))
            return false;

        RecordRequiredSetupProperty(property);
        expression = logicProperty.LoweringKind == VueLogicPropertyLoweringKind.ValueBinding
            ? ToLowerCamelCase(property.Name)
            : ToLowerCamelCase(property.Name) + "()";
        return true;
    }

    private bool TryEmitLifecycleCurrentComponentFieldReference(
        IFieldSymbol field,
        out string expression)
    {
        expression = string.Empty;
        if (!_logicFieldsByName.ContainsKey(field.Name))
            return false;

        if (!IsSupportedRenderFieldReference(field))
            return false;

        RecordRequiredSetupField(field);
        expression = ToLowerCamelCase(field.Name);
        return true;
    }

    private void EnsureSupportedRenderPropertyReference(IPropertyReferenceOperation propertyReference)
    {
        var property = propertyReference.Property;
        if (IsSupportedRenderPropertyReference(propertyReference))
            return;

        var reason = property.SetMethod is not null &&
                     RazorVueCurrentComponentValueMemberHelper.TryGetUnsupportedSetupPropertyReason(
                         GetOperationCompilation(propertyReference),
                         property,
                         out var mutableReason)
            ? mutableReason
            : "property initializer could not be lowered as a setup carrier";
        throw CreateUnsupportedSetupLogicException(
            property,
            $"RazorVue render does not support component property '{property.Name}': {reason}.");
    }

    private void EnsureSupportedRenderPropertyReference(IPropertySymbol property)
    {
        if (IsSupportedRenderPropertyReference(property))
            return;

        var reason = property.SetMethod is not null &&
                     RazorVueCurrentComponentValueMemberHelper.TryGetUnsupportedSetupPropertyReason(
                         _snapshot.Compilation,
                         property,
                         out var mutableReason)
            ? mutableReason
            : "property initializer could not be lowered as a setup carrier";
        throw CreateUnsupportedSetupLogicException(
            property,
            $"RazorVue render does not support component property '{property.Name}': {reason}.");
    }

    private bool IsSupportedRenderPropertyReference(IPropertyReferenceOperation propertyReference)
        => IsSupportedRenderPropertyReference(propertyReference.Property, GetOperationCompilation(propertyReference));

    private bool IsSupportedRenderPropertyReference(IPropertySymbol property)
        => IsSupportedRenderPropertyReference(property, _snapshot.Compilation);

    private bool IsSupportedRenderPropertyReference(IPropertySymbol property, Compilation compilation)
    {
        if (_logicPropertiesByName.TryGetValue(property.Name, out var logicProperty) &&
            RazorVueSymbolIdentity.SameMember(logicProperty.PropertySymbol, property) &&
            logicProperty.LoweringKind == VueLogicPropertyLoweringKind.GetterFunction)
        {
            return true;
        }

        if (RazorVueCurrentComponentValueMemberHelper.TryGetValueMemberInitializer(
                compilation,
                property,
                out _))
        {
            return true;
        }

        return RazorVueCurrentComponentValueMemberHelper.CanUseMutableSetupCarrierMember(property);
    }

    private void EnsureSupportedRenderFieldReference(IFieldReferenceOperation fieldReference)
    {
        var field = fieldReference.Field;
        if (IsSupportedRenderFieldReference(fieldReference))
            return;

        var reason = !field.IsReadOnly &&
                     RazorVueCurrentComponentValueMemberHelper.TryGetUnsupportedMutableSetupCarrierMemberReason(field, out var mutableReason)
            ? mutableReason
            : "field initializer could not be lowered as a setup carrier";
        throw CreateUnsupportedSetupLogicException(
            field,
            $"RazorVue render does not support component field '{field.Name}': {reason}.");
    }

    private void EnsureSupportedRenderFieldReference(IFieldSymbol field)
    {
        if (IsSupportedRenderFieldReference(field))
            return;

        var reason = !field.IsReadOnly &&
                     RazorVueCurrentComponentValueMemberHelper.TryGetUnsupportedMutableSetupCarrierMemberReason(field, out var mutableReason)
            ? mutableReason
            : "field initializer could not be lowered as a setup carrier";
        throw CreateUnsupportedSetupLogicException(
            field,
            $"RazorVue render does not support component field '{field.Name}': {reason}.");
    }

    private bool IsSupportedRenderFieldReference(IFieldReferenceOperation fieldReference)
        => IsSupportedRenderFieldReference(fieldReference.Field, GetOperationCompilation(fieldReference));

    private bool IsSupportedRenderFieldReference(IFieldSymbol field)
        => IsSupportedRenderFieldReference(field, _snapshot.Compilation);

    private bool IsSupportedRenderFieldReference(IFieldSymbol field, Compilation compilation)
    {
        if (RazorVueCurrentComponentValueMemberHelper.TryGetValueMemberInitializer(
                compilation,
                field,
                out _))
        {
            return true;
        }

        return RazorVueCurrentComponentValueMemberHelper.CanUseMutableSetupCarrierMember(field);
    }

    private Compilation GetOperationCompilation(IOperation operation)
    {
        var semanticCompilation = operation.SemanticModel?.Compilation;
        return semanticCompilation is not null && semanticCompilation.ContainsSyntaxTree(operation.Syntax.SyntaxTree)
            ? semanticCompilation
            : _snapshot.Compilation;
    }

    private static string ResolveMemberName(ISymbol symbol)
        => Util.GetConfigOrSymbolName(symbol);

    private static string ToLifecyclePropName(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        if (value.Length == 1)
            return char.ToLowerInvariant(value[0]).ToString();

        if (char.IsUpper(value[0]) && char.IsUpper(value[1]))
            return value;

        return char.ToLowerInvariant(value[0]) + value.Substring(1);
    }

    private string EmitInvocation(IInvocationOperation invocation)
    {
        var normalizedCallbackFactory = TryNormalizeRazorGeneratedCallbackFactory(invocation);
        if (normalizedCallbackFactory.Length != 0)
            return normalizedCallbackFactory;

        if (TryNormalizeRazorInferredEventCallback(invocation, out var normalizedInferredCallback))
            return normalizedInferredCallback;

        if (TryNormalizeCurrentComponentCallbackInvokeAsync(invocation, useSetupEmitter: false, compilerArgument: null, out var normalizedCallbackInvoke))
            return normalizedCallbackInvoke;

        if (invocation.Instance is not null && invocation.TargetMethod.Name == "Invoke")
        {
            return EmitBoundInvocation(
                invocation.TargetMethod,
                invocation.Arguments,
                operation => EmitExpression(operation),
                EmitExpression(invocation.Instance));
        }

        if (IsCurrentComponentMember(invocation.TargetMethod, invocation.Instance))
        {
            if (IsUnsupportedSetupHelperMethod(invocation.TargetMethod))
                throw CreateUnsupportedSetupLogicException(invocation.TargetMethod);

            RecordRequiredSetupMethod(invocation.TargetMethod);
            return EmitBoundInvocation(
                invocation.TargetMethod,
                invocation.Arguments,
                operation => EmitExpression(operation),
                target => ToLowerCamelCase(target.Name) + "(",
                allowTargetEvaluationCapture: false);
        }

        var targetMethodName = GetEmittedMethodName(invocation.TargetMethod);
        var target = invocation.Instance is not null
            ? EmitMemberInvocationTarget(invocation.Instance, targetMethodName, compilerArgument: null)
            : targetMethodName;

        return EmitBoundInvocation(
            invocation.TargetMethod,
            invocation.Arguments,
            operation => EmitExpression(operation),
            _ => target + "(",
            allowTargetEvaluationCapture: false);
    }

    internal bool TryRewriteInvocation(
        IInvocationOperation invocation,
        SenseArgument argument,
        bool useSetupEmitter,
        out string expression)
    {
        if (TryRewriteLifecycleScopedCallableInvocation(invocation, argument, useSetupEmitter, out expression))
            return true;

        if (useSetupEmitter)
            return TryRewriteSetupInvocation(invocation, argument, out expression);

        if (TryRewriteImperativeBuilderInvocation(invocation, argument, out expression))
            return true;

        if (TryRewriteImperativeRenderHelperInvocation(invocation, argument, out expression))
            return true;

        var normalizedCallbackFactory = TryNormalizeRazorGeneratedCallbackFactory(invocation);
        if (normalizedCallbackFactory.Length != 0)
        {
            expression = normalizedCallbackFactory;
            return true;
        }

        if (TryNormalizeRazorInferredEventCallback(invocation, out var normalizedInferredCallback))
        {
            expression = normalizedInferredCallback;
            return true;
        }

        if (TryNormalizeCurrentComponentCallbackInvokeAsync(invocation, useSetupEmitter: false, argument, out var normalizedCallbackInvoke))
        {
            expression = normalizedCallbackInvoke;
            return true;
        }

        if (invocation.Instance is not null && invocation.TargetMethod.Name == "Invoke")
        {
            expression = EmitBoundInvocation(
                invocation.TargetMethod,
                invocation.Arguments,
                operation => EmitExpression(operation, argument),
                EmitExpression(invocation.Instance, argument));
            return true;
        }

        if (IsCurrentComponentMember(invocation.TargetMethod, invocation.Instance))
        {
            if (IsUnsupportedSetupHelperMethod(invocation.TargetMethod))
                throw CreateUnsupportedSetupLogicException(invocation.TargetMethod);

            RecordRequiredSetupMethod(invocation.TargetMethod);
            expression = EmitBoundInvocation(
                invocation.TargetMethod,
                invocation.Arguments,
                operation => EmitExpression(operation, argument),
                target => ToLowerCamelCase(target.Name) + "(",
                allowTargetEvaluationCapture: false);
            return true;
        }

        expression = string.Empty;
        return false;
    }

    private bool TryRewriteSetupInvocation(
        IInvocationOperation invocation,
        SenseArgument? compilerArgument,
        out string expression)
    {
        expression = string.Empty;
        var normalizedCallbackFactory = TryNormalizeRazorGeneratedCallbackFactory(invocation);
        if (normalizedCallbackFactory.Length != 0)
        {
            expression = normalizedCallbackFactory;
            return true;
        }

        if (TryNormalizeRazorInferredEventCallback(invocation, out var normalizedInferredCallback))
        {
            expression = normalizedInferredCallback;
            return true;
        }

        if (TryNormalizeCurrentComponentCallbackInvokeAsync(invocation, useSetupEmitter: true, compilerArgument, out var normalizedCallbackInvoke))
        {
            expression = normalizedCallbackInvoke;
            return true;
        }

        if (invocation.Instance is not null && invocation.TargetMethod.Name == "Invoke")
        {
            expression = EmitBoundInvocation(
                invocation.TargetMethod,
                invocation.Arguments,
                operation => EmitSetupExpression(operation, compilerArgument),
                EmitSetupExpression(invocation.Instance, compilerArgument));
            return true;
        }

        if (IsCurrentComponentMember(invocation.TargetMethod, invocation.Instance))
        {
            if (IsUnsupportedSetupHelperMethod(invocation.TargetMethod))
                throw CreateUnsupportedSetupLogicException(invocation.TargetMethod);

            RecordRequiredSetupMethod(invocation.TargetMethod);
            expression = EmitBoundInvocation(
                invocation.TargetMethod,
                invocation.Arguments,
                operation => EmitSetupExpression(operation, compilerArgument),
                target => ToLowerCamelCase(target.Name) + "(",
                allowTargetEvaluationCapture: false);
            return true;
        }

        return false;
    }

    internal bool TryRewriteMethodReference(
        IMethodReferenceOperation operation,
        SenseArgument argument,
        bool useSetupEmitter,
        out string expression)
    {
        if (operation.Instance is null &&
            _scopedLifecycleLocalFunctionAliases is not null &&
            _scopedLifecycleLocalFunctionAliases.TryGetValue(operation.Method, out expression))
        {
            return true;
        }

        if (IsCurrentComponentMember(operation.Method, operation.Instance))
        {
            RecordRequiredSetupMethod(operation.Method);
            expression = ToLowerCamelCase(operation.Method.Name);
            return true;
        }

        expression = string.Empty;
        return false;
    }

    internal bool TryRewriteParameterReference(IParameterReferenceOperation operation, SenseArgument argument, out string expression)
    {
        if (_scopedParameterAliases is not null &&
            _scopedParameterAliases.TryGetValue(operation.Parameter, out expression))
        {
            return true;
        }

        expression = string.Empty;
        return false;
    }

    internal bool TryRewriteLocalReference(ILocalReferenceOperation operation, SenseArgument argument, out string expression)
    {
        if (_imperativeBuilderAlias is not null &&
            RazorVueComponentTypeCarrierHelper.TryGetInvalidatedSourceStableComponentTypeMember(
                _snapshot.Compilation,
                _snapshot.ComponentSymbol,
                operation,
                out var invalidatedComponentTypeMemberCarrier))
        {
            throw CreateImperativeComponentTypeMemberCarrierInvalidatedException(operation, invalidatedComponentTypeMemberCarrier);
        }

        if (_imperativeBuilderAlias is not null &&
            RazorVueComponentTypeCarrierHelper.TryResolveSourceStableVueComponentTypeLocal(
                _snapshot.Compilation,
                _snapshot.ComponentSymbol,
                operation.Local,
                out _))
        {
            throw CreateImperativeComponentTypeCarrierRuntimeValueException(operation, operation.Local);
        }

        if (_scopedLifecycleCallableAliases is not null &&
            _scopedLifecycleCallableAliases.TryGetValue(operation.Local, out expression))
        {
            return true;
        }

        if (_scopedLocalAliases is not null &&
            _scopedLocalAliases.TryGetValue(operation.Local, out expression))
        {
            return true;
        }

        if (_sourceStableLocalInitializers is not null &&
            _sourceStableLocalInitializers.TryGetValue(operation.Local, out var initializer))
        {
            expression = EmitSetupExpression(initializer, argument);
            return true;
        }

        expression = string.Empty;
        return false;
    }

    internal bool TryRewriteLocalDeclarationIdentifier(ILocalSymbol local, out Identifier identifier)
    {
        if (_scopedLocalAliases is not null &&
            _scopedLocalAliases.TryGetValue(local, out var alias))
        {
            identifier = new Identifier(alias);
            return true;
        }

        identifier = default!;
        return false;
    }

    private bool TryRewriteLifecycleScopedCallableInvocation(
        IInvocationOperation invocation,
        SenseArgument argument,
        bool useSetupEmitter,
        out string expression)
    {
        expression = string.Empty;
        if (invocation.Instance is not null)
            return false;

        if (invocation.TargetMethod.MethodKind == MethodKind.LocalFunction &&
            _scopedLifecycleLocalFunctionAliases is not null &&
            _scopedLifecycleLocalFunctionAliases.TryGetValue(invocation.TargetMethod, out var localFunctionAlias))
        {
            expression = EmitBoundInvocation(
                invocation.TargetMethod,
                invocation.Arguments,
                operation => useSetupEmitter
                    ? EmitSetupExpression(operation, argument)
                    : EmitExpression(operation, argument),
                _ => localFunctionAlias + "(",
                allowTargetEvaluationCapture: false);
            return true;
        }

        return false;
    }

    private string EmitBoundInvocation(
        IMethodSymbol targetMethod,
        ImmutableArray<IArgumentOperation> arguments,
        Func<IOperation, string> emitArgument,
        Func<IMethodSymbol, string> emitTargetPrefix,
        bool allowTargetEvaluationCapture)
    {
        var binding = BindInvocationArguments(targetMethod, arguments, emitArgument, CreateUnsupportedSetupLogicException(targetMethod));
        var targetPrefix = emitTargetPrefix(targetMethod);
        return allowTargetEvaluationCapture
            ? ComposeEvaluationOrderedInvocation(targetPrefix, binding)
            : ComposeParameterBindingInvocation(targetPrefix, binding);
    }

    private string EmitBoundInvocation(
        IMethodSymbol targetMethod,
        ImmutableArray<IArgumentOperation> arguments,
        Func<IOperation, string> emitArgument,
        string targetExpression)
    {
        var binding = BindInvocationArguments(targetMethod, arguments, emitArgument, CreateUnsupportedSetupLogicException(targetMethod));
        if (!RequiresTargetCapture(binding))
            return ComposeParameterBindingInvocation(targetExpression + "(", binding);

        return ComposeTargetAndParameterBindingInvocation(targetExpression, binding);
    }

    private LifecyclePayloadEmission EmitLifecyclePayloadBoundInvocation(
        IMethodSymbol targetMethod,
        ImmutableArray<IArgumentOperation> arguments,
        Func<IOperation, LifecyclePayloadEmission> emitArgument,
        Func<IMethodSymbol, string> emitTarget,
        Func<Exception> createFailure)
    {
        var binding = BindInvocationArguments(targetMethod, arguments, emitArgument, createFailure());
        var expression = ComposeParameterBindingInvocation(emitTarget(targetMethod) + "(", binding);
        return new LifecyclePayloadEmission(
            expression,
            binding.UsesFirstRender,
            binding.PreludeBindings);
    }

    private BoundInvocationArguments<string> BindInvocationArguments(
        IMethodSymbol targetMethod,
        ImmutableArray<IArgumentOperation> arguments,
        Func<IOperation, string> emitArgument,
        Exception failure)
    {
        var canonicalMethod = RazorVueMethodSymbolNormalizer.GetCanonicalMethod(targetMethod);
        var emittedByParameterOrdinal = new string[canonicalMethod.Parameters.Length];
        var emittedInSourceOrder = new List<BoundInvocationArgument<string>>(arguments.Length);
        var boundParameters = new HashSet<IParameterSymbol>(SymbolEqualityComparer.Default);

        foreach (var argument in arguments)
        {
            if (argument.Parameter is not { } rawParameter)
                throw failure;

            var parameter = RazorVueMethodSymbolNormalizer.NormalizeParameter(targetMethod, rawParameter);
            if (!CanBindSetupHelperArgument(parameter) ||
                parameter.Ordinal < 0 ||
                parameter.Ordinal >= emittedByParameterOrdinal.Length)
            {
                throw failure;
            }

            if (!boundParameters.Add(parameter))
                throw failure;

            var emitted = emitArgument(argument.Value);
            emittedByParameterOrdinal[parameter.Ordinal] = emitted;
            emittedInSourceOrder.Add(new BoundInvocationArgument<string>(parameter, emitted));
        }

        if (boundParameters.Count != emittedByParameterOrdinal.Length ||
            emittedByParameterOrdinal.Any(static item => item is null))
        {
            throw failure;
        }

        return new BoundInvocationArguments<string>(
            canonicalMethod.Parameters.ToArray(),
            emittedByParameterOrdinal!,
            emittedInSourceOrder.ToImmutableArray());
    }

    private BoundInvocationArguments<LifecyclePayloadEmission> BindInvocationArguments(
        IMethodSymbol targetMethod,
        ImmutableArray<IArgumentOperation> arguments,
        Func<IOperation, LifecyclePayloadEmission> emitArgument,
        Exception failure)
    {
        var canonicalMethod = RazorVueMethodSymbolNormalizer.GetCanonicalMethod(targetMethod);
        var emittedByParameterOrdinal = new LifecyclePayloadEmission[canonicalMethod.Parameters.Length];
        var emittedInSourceOrder = new List<BoundInvocationArgument<LifecyclePayloadEmission>>(arguments.Length);
        var boundParameters = new HashSet<IParameterSymbol>(SymbolEqualityComparer.Default);
        var usesFirstRender = false;
        var preludeBuilder = ImmutableArray.CreateBuilder<LifecyclePayloadPreludeBinding>();

        foreach (var argument in arguments)
        {
            if (argument.Parameter is not { } rawParameter)
                throw failure;

            var parameter = RazorVueMethodSymbolNormalizer.NormalizeParameter(targetMethod, rawParameter);
            if (!CanBindSetupHelperArgument(parameter) ||
                parameter.Ordinal < 0 ||
                parameter.Ordinal >= emittedByParameterOrdinal.Length)
            {
                throw failure;
            }

            if (!boundParameters.Add(parameter))
                throw failure;

            var emitted = emitArgument(argument.Value);
            emittedByParameterOrdinal[parameter.Ordinal] = emitted;
            emittedInSourceOrder.Add(new BoundInvocationArgument<LifecyclePayloadEmission>(parameter, emitted));
            usesFirstRender |= emitted.UsesFirstRender;
            if (!emitted.PreludeBindings.IsDefaultOrEmpty)
                preludeBuilder.AddRange(emitted.PreludeBindings);
        }

        if (boundParameters.Count != emittedByParameterOrdinal.Length ||
            emittedByParameterOrdinal.Any(static item => item.Expression is null))
        {
            throw failure;
        }

        return new BoundInvocationArguments<LifecyclePayloadEmission>(
            canonicalMethod.Parameters.ToArray(),
            emittedByParameterOrdinal!,
            emittedInSourceOrder.ToImmutableArray(),
            usesFirstRender,
            preludeBuilder.ToImmutable());
    }

    private readonly record struct BoundInvocationArgument<T>(IParameterSymbol Parameter, T Emitted);

    private readonly record struct BoundInvocationArguments<T>(
        IParameterSymbol[] ParametersByOrdinal,
        T[] ArgumentsByParameterOrdinal,
        ImmutableArray<BoundInvocationArgument<T>> ArgumentsInSourceOrder,
        bool UsesFirstRender = false,
        ImmutableArray<LifecyclePayloadPreludeBinding> PreludeBindings = default);

    private static string ComposeParameterBindingInvocation(
        string targetPrefix,
        BoundInvocationArguments<string> binding)
    {
        var declarationOrderArguments = string.Join(", ", binding.ArgumentsByParameterOrdinal);
        if (ArgumentsAreAlreadyInDeclarationOrder(binding))
            return targetPrefix + declarationOrderArguments + ")";

        return ComposeEvaluationOrderedInvocation(
            targetPrefix + string.Join(", ", binding.ParametersByOrdinal.Select(static parameter => parameter.Name)) + ")",
            binding);
    }

    private static string ComposeParameterBindingInvocation(
        string targetPrefix,
        BoundInvocationArguments<LifecyclePayloadEmission> binding)
    {
        var declarationOrderArguments = string.Join(", ", binding.ArgumentsByParameterOrdinal.Select(static item => item.Expression));
        if (ArgumentsAreAlreadyInDeclarationOrder(binding))
            return targetPrefix + declarationOrderArguments + ")";

        var call = targetPrefix + string.Join(", ", binding.ParametersByOrdinal.Select(static parameter => parameter.Name)) + ")";
        for (var index = binding.ArgumentsInSourceOrder.Length - 1; index >= 0; index--)
        {
            var argument = binding.ArgumentsInSourceOrder[index];
            call = "((" + argument.Parameter.Name + ") => " + call + ")(" + argument.Emitted.Expression + ")";
        }

        return call;
    }

    private static string ComposeEvaluationOrderedInvocation(
        string targetPrefix,
        BoundInvocationArguments<string> binding)
    {
        var declarationOrderArguments = string.Join(", ", binding.ArgumentsByParameterOrdinal);
        var call = targetPrefix + declarationOrderArguments + ")";
        for (var index = binding.ArgumentsInSourceOrder.Length - 1; index >= 0; index--)
        {
            var argument = binding.ArgumentsInSourceOrder[index];
            call = "((" + argument.Parameter.Name + ") => " + call + ")(" + argument.Emitted + ")";
        }

        return call;
    }

    private static string ComposeTargetAndParameterBindingInvocation(
        string targetExpression,
        BoundInvocationArguments<string> binding)
    {
        var call = "__jazorTarget(" + string.Join(", ", binding.ParametersByOrdinal.Select(static parameter => parameter.Name)) + ")";
        for (var index = binding.ArgumentsInSourceOrder.Length - 1; index >= 0; index--)
        {
            var argument = binding.ArgumentsInSourceOrder[index];
            call = "((" + argument.Parameter.Name + ") => " + call + ")(" + argument.Emitted + ")";
        }

        return "((__jazorTarget) => " + call + ")(" + targetExpression + ")";
    }

    private static bool ArgumentsAreAlreadyInDeclarationOrder(BoundInvocationArguments<string> binding)
    {
        for (var index = 0; index < binding.ArgumentsInSourceOrder.Length; index++)
        {
            if (binding.ArgumentsInSourceOrder[index].Parameter.Ordinal != index)
                return false;
        }

        return true;
    }

    private static bool ArgumentsAreAlreadyInDeclarationOrder(BoundInvocationArguments<LifecyclePayloadEmission> binding)
    {
        for (var index = 0; index < binding.ArgumentsInSourceOrder.Length; index++)
        {
            if (binding.ArgumentsInSourceOrder[index].Parameter.Ordinal != index)
                return false;
        }

        return true;
    }

    private static bool RequiresTargetCapture(BoundInvocationArguments<string> binding)
        => !ArgumentsAreAlreadyInDeclarationOrder(binding);

    private static bool CanBindSetupHelperArgument(IParameterSymbol parameter)
        => parameter.RefKind is RefKind.None or RefKind.In;

    private void ThrowIfReadOnlyByRefParameterEscapes(IOperation operation)
    {
        foreach (var current in operation.DescendantsAndSelf())
        {
            if (Unwrap(current) is not IArgumentOperation argument ||
                argument.Parameter?.RefKind is not (RefKind.Ref or RefKind.Out or RefKind.In))
            {
                continue;
            }

            if (!TryGetReadOnlyByRefParameterReference(argument.Value, out var escapedParameter))
                continue;

            throw CreateUnsupportedSetupLogicException(
                escapedParameter,
                $"RazorVue setup lowering supports reading read-only 'in' value parameter '{escapedParameter.Name}' as a value, but cannot forward it through a by-reference invocation.");
        }
    }

    private static bool TryGetReadOnlyByRefParameterReference(
        IOperation? operation,
        out IParameterSymbol parameter)
    {
        parameter = default!;
        if (operation is null)
            return false;

        foreach (var current in operation.DescendantsAndSelf())
        {
            if (Unwrap(current) is IParameterReferenceOperation parameterReference &&
                parameterReference.Parameter.RefKind == RefKind.In)
            {
                parameter = parameterReference.Parameter;
                return true;
            }
        }

        return false;
    }

    private string EmitMemberInvocationTarget(
        IOperation instance,
        string targetMethodName,
        SenseArgument? compilerArgument)
    {
        var target = compilerArgument is null
            ? EmitExpression(instance)
            : EmitSetupExpression(instance, compilerArgument);

        if (targetMethodName == "toString" && RequiresParenthesizedMemberTarget(instance))
            target = "(" + target + ")";

        return target + "." + targetMethodName;
    }

    private static bool RequiresParenthesizedMemberTarget(IOperation instance)
    {
        var current = instance;
        while (current is IParenthesizedOperation parenthesized)
            current = parenthesized.Operand;

        return Unwrap(current) is IBinaryOperation or IConditionalOperation;
    }

    private static string GetEmittedMethodName(IMethodSymbol method)
        => method.Name == "ToString" && method.Parameters.Length == 0 && method.MethodKind == MethodKind.Ordinary
            ? "toString"
            : method.Name;

    private static bool IsUnsupportedSetupHelperMethod(IMethodSymbol method)
    {
        if (method.IsAsync)
            return true;

        var returnType = method.ReturnType;
        if (returnType is not INamedTypeSymbol namedType)
            return false;

        var displayName = namedType.OriginalDefinition.ToDisplayString();
        return string.Equals(displayName, "System.Threading.Tasks.Task", StringComparison.Ordinal) ||
               string.Equals(displayName, "System.Threading.Tasks.Task<TResult>", StringComparison.Ordinal) ||
               string.Equals(displayName, "System.Threading.Tasks.ValueTask", StringComparison.Ordinal) ||
               string.Equals(displayName, "System.Threading.Tasks.ValueTask<TResult>", StringComparison.Ordinal);
    }

    private bool TryGetSimplePropertyBodyOperation(IPropertySymbol property, out IOperation operation)
    {
        operation = default!;
        if (property.DeclaringSyntaxReferences.Length == 0)
            return false;

        foreach (var syntaxReference in property.DeclaringSyntaxReferences)
        {
            if (syntaxReference.GetSyntax() is not PropertyDeclarationSyntax declaration)
                continue;

            var semanticModel = _snapshot.Compilation.GetSemanticModel(declaration.SyntaxTree);
            if (RazorVuePropertyInitializerHelper.TryGetPropertyValueOperation(semanticModel, declaration, out var propertyOperation) &&
                RazorVueOperationNormalizer.Unwrap(propertyOperation) is { } initializer)
            {
                operation = initializer;
                return true;
            }
        }

        return false;
    }

    private RazorVueCompilationIssueException CreateUnsupportedSetupLogicException(IMethodSymbol method)
        => CreateUnsupportedSetupLogicException(
            method,
            $"RazorVue setup lowering does not support method '{method.Name}' in component '{method.ContainingType.ToDisplayString()}'.");

    private RazorVueCompilationIssueException CreateUnsupportedSetupLogicException(IParameterSymbol parameter, string message)
    {
        var originLocation = parameter.Locations.FirstOrDefault(static location => location.IsInSource);
        var origin = originLocation is null
            ? null
            : RazorVueSourceOrigin.FromLocation(originLocation, RazorVueOriginKind.Logic);
        var ownerComponent = parameter.ContainingType?.ToDisplayString() ??
                             parameter.ContainingSymbol?.ContainingType?.ToDisplayString() ??
                             _snapshot.Descriptor.FullName;
        var issue = new RazorVueCompilationIssue(
            RazorVueIssueCode.UnsupportedSetupLogicLowering,
            RazorVueIssueSeverity.Error,
            message,
            ImmutableArray<string>.Empty);
        return new RazorVueCompilationIssueException(issue, ownerComponent, origin);
    }

    private RazorVueCompilationIssueException CreateUnsupportedSetupLogicException(ISymbol symbol, string message)
    {
        var originLocation = symbol.Locations.FirstOrDefault(static location => location.IsInSource);
        var origin = originLocation is null
            ? null
            : RazorVueSourceOrigin.FromLocation(originLocation, RazorVueOriginKind.Logic);
        var ownerComponent = symbol.ContainingType?.ToDisplayString() ?? _snapshot.Descriptor.FullName;
        var issue = new RazorVueCompilationIssue(
            RazorVueIssueCode.UnsupportedSetupLogicLowering,
            RazorVueIssueSeverity.Error,
            message,
            ImmutableArray<string>.Empty);
        return new RazorVueCompilationIssueException(issue, ownerComponent, origin);
    }

    private string EmitInterpolatedString(IInterpolatedStringOperation interpolated)
    {
        var builder = new StringBuilder();
        builder.Append('`');
        foreach (var part in interpolated.Parts)
        {
            switch (part)
            {
                case IInterpolatedStringTextOperation text:
                    builder.Append(EscapeTemplateText(text.Text.ConstantValue.HasValue && text.Text.ConstantValue.Value is string value ? value : string.Empty));
                    break;
                case IInterpolationOperation interpolation:
                    if (interpolation.FormatString is not null)
                        throw new NotSupportedException(
                            "Interpolation format specifiers are not supported in RazorVue template expressions.");
                    builder.Append("${").Append(EmitExpression(interpolation.Expression)).Append('}');
                    break;
            }
        }

        builder.Append('`');
        return builder.ToString();
    }

    private string EmitSetupInterpolatedString(IInterpolatedStringOperation interpolated, SenseArgument? compilerArgument = null)
    {
        var builder = new StringBuilder();
        builder.Append('`');
        foreach (var part in interpolated.Parts)
        {
            switch (part)
            {
                case IInterpolatedStringTextOperation text:
                    builder.Append(EscapeTemplateText(text.Text.ConstantValue.HasValue && text.Text.ConstantValue.Value is string value ? value : string.Empty));
                    break;
                case IInterpolationOperation interpolation:
                    builder.Append("${").Append(EmitSetupExpression(interpolation.Expression, compilerArgument)).Append('}');
                    break;
            }
        }

        builder.Append('`');
        return builder.ToString();
    }

    private bool IsCurrentComponentMember(ISymbol symbol, IOperation? instance)
    {
        if (symbol is IMethodSymbol { MethodKind: MethodKind.LocalFunction })
            return false;

        return RazorVueSymbolIdentity.IsCurrentComponentMember(
            _snapshot.ComponentSymbol,
            symbol,
            instance,
            Unwrap);
    }

    private string EmitMemberTarget(IOperation? instance)
    {
        var current = Unwrap(instance);
        if (current is null)
            throw new NotSupportedException("RazorVue render member access is missing an instance target.");

        return EmitExpression(current);
    }

    private string EmitMemberTarget(IOperation? instance, SenseArgument? compilerArgument)
    {
        var current = Unwrap(instance);
        if (current is null)
            throw new NotSupportedException("RazorVue render member access is missing an instance target.");

        return compilerArgument is null
            ? EmitExpression(current)
            : EmitSetupExpression(current, compilerArgument);
    }

    private static IOperation? Unwrap(IOperation? operation)
        => RazorVueOperationNormalizer.Unwrap(operation);

    private string TryNormalizeRazorGeneratedCallbackFactory(IInvocationOperation invocation)
    {
        if (!IsEventCallbackFactoryCreate(invocation) || invocation.Arguments.Length < 2)
            return string.Empty;

        var receiver = Unwrap(invocation.Arguments[0].Value);
        if (receiver is not IInstanceReferenceOperation)
            return string.Empty;

        var callbackTarget = UnwrapDelegateCarrier(invocation.Arguments[1].Value) ?? Unwrap(invocation.Arguments[1].Value);
        if (callbackTarget is null)
            return string.Empty;

        if (TryNormalizeRazorInferredEventCallback(callbackTarget, out var inferredCallbackFactory))
            return inferredCallbackFactory;

        return callbackTarget switch
        {
            IPropertyReferenceOperation property when IsCurrentComponentMember(property.Property, property.Instance) =>
                EmitCurrentComponentCallbackReference(property.Property),
            IFieldReferenceOperation field when IsCurrentComponentMember(field.Field, field.Instance) =>
                EmitCurrentComponentCallbackReference(field.Field),
            IMethodReferenceOperation method when IsCurrentComponentMember(method.Method, method.Instance) =>
                TryNormalizeCurrentComponentCallbackMethod(method),
            IAnonymousFunctionOperation anonymousFunction =>
                EmitSetupExpression(anonymousFunction),
            _ => string.Empty
        };
    }

    private string TryNormalizeCurrentComponentCallbackMethod(IMethodReferenceOperation methodReference)
    {
        if (!TryGetSimpleMethodBodyOperation(methodReference.Method, out var bodyOperation))
        {
            if (IsUnsupportedSetupHelperMethod(methodReference.Method))
                return string.Empty;

            RecordRequiredSetupMethod(methodReference.Method);
            return ToLowerCamelCase(methodReference.Method.Name);
        }

        var parameterNames = methodReference.Method.Parameters
            .Select(static parameter => parameter.Name)
            .ToArray();

        var bodyExpression = WithScopedParameterAliases(
            methodReference.Method.Parameters,
            parameterNames,
            () => EmitSetupExpression(bodyOperation));

        return parameterNames.Length == 0
            ? "() => " + NormalizeArrowFunctionExpressionBody(bodyExpression)
            : "(" + string.Join(", ", parameterNames) + ") => " + NormalizeArrowFunctionExpressionBody(bodyExpression);
    }

    private bool TryNormalizeRazorInferredEventCallback(IOperation callbackTarget, out string expression)
    {
        expression = string.Empty;
        if (callbackTarget is not IInvocationOperation invocation ||
            !IsInferredEventCallback(invocation) ||
            invocation.Arguments.Length < 2)
        {
            return false;
        }

        if (!TryGetAssignedLambdaTarget(invocation.Arguments[1].Value, out var assignedTarget))
            return false;

        switch (assignedTarget)
        {
            case IPropertyReferenceOperation property when IsCurrentComponentMember(property.Property, property.Instance):
                var changedAlias = GetBindChangedSymbol(property.Property);
                if (!_emitsByRazorAlias.ContainsKey(changedAlias))
                    throw CreateInvalidBindTargetException(property.Property);

                expression = EmitCurrentComponentCallbackReference(changedAlias);
                return true;
            case IFieldReferenceOperation field when IsCurrentComponentMember(field.Field, field.Instance):
                RecordRequiredSetupField(field.Field);
                expression = "(__value) => (" + ToLowerCamelCase(field.Field.Name) + " = __value)";
                return true;
            case ILocalReferenceOperation local:
                expression = "(__value) => (" + local.Local.Name + " = __value)";
                return true;
            case IParameterReferenceOperation:
                throw CreateUnsupportedSetupLogicException(
                    _snapshot.ComponentSymbol,
                    $"RazorVue setup-side logic does not support assigning to method parameters from generated two-way binding in component '{_snapshot.Descriptor.FullName}'.");
            default:
                return false;
        }
    }

    private bool TryNormalizeCurrentComponentCallbackInvokeAsync(
        IInvocationOperation invocation,
        bool useSetupEmitter,
        SenseArgument? compilerArgument,
        out string expression)
    {
        expression = string.Empty;
        if (!string.Equals(invocation.TargetMethod.Name, "InvokeAsync", StringComparison.Ordinal))
            return false;

        var callbackInstance = Unwrap(invocation.Instance);
        if (callbackInstance is null)
            return false;

        string emitArgument(IOperation operation)
            => useSetupEmitter
                ? EmitSetupExpression(operation, compilerArgument)
                : EmitExpression(operation, compilerArgument);

        switch (callbackInstance)
        {
            case IPropertyReferenceOperation property when IsCurrentComponentMember(property.Property, property.Instance):
                expression = EmitCurrentComponentCallbackInvocation(property.Property.Name, invocation.Arguments, emitArgument);
                return true;
            case IFieldReferenceOperation field when IsCurrentComponentMember(field.Field, field.Instance):
                RecordRequiredSetupField(field.Field);
                expression = EmitCallbackInvocationExpression(ToLowerCamelCase(field.Field.Name), invocation.Arguments, emitArgument, optional: false);
                return true;
            default:
                return false;
        }
    }

    private string EmitCurrentComponentCallbackInvocation(
        string razorAlias,
        ImmutableArray<IArgumentOperation> arguments,
        Func<IOperation, string> emitArgument)
    {
        if (_emitsByRazorAlias.TryGetValue(razorAlias, out var emitDescriptor))
        {
            if (arguments.Length == 0)
                return "emit(" + ToJavaScriptString(emitDescriptor.Name) + ")";

            if (arguments.Length == 1)
                return "emit(" + ToJavaScriptString(emitDescriptor.Name) + ", " + emitArgument(arguments[0].Value) + ")";

            throw new NotSupportedException(
                $"RazorVue render currently does not support EventCallback.InvokeAsync with {arguments.Length} arguments for '{razorAlias}' in component '{_snapshot.Descriptor.FullName}'.");
        }

        if (_propsByPublicName.TryGetValue(razorAlias, out var propDescriptor))
            return EmitCallbackInvocationExpression("props." + propDescriptor.Name, arguments, emitArgument, optional: true);

        throw new NotSupportedException(
            $"RazorVue render currently does not support callback member '{razorAlias}' in component '{_snapshot.Descriptor.FullName}'.");
    }

    private static string EmitCallbackInvocationExpression(
        string callbackExpression,
        ImmutableArray<IArgumentOperation> arguments,
        Func<IOperation, string> emitArgument,
        bool optional)
    {
        var argumentList = string.Join(", ", arguments.Select(argument => emitArgument(argument.Value)));
        return optional
            ? callbackExpression + "?.(" + argumentList + ")"
            : callbackExpression + "(" + argumentList + ")";
    }

    private bool TryGetSimpleMethodBodyOperation(IMethodSymbol method, out IOperation operation)
    {
        operation = default!;
        if (method.DeclaringSyntaxReferences.Length == 0)
            return false;

        if (method.DeclaringSyntaxReferences[0].GetSyntax() is not MethodDeclarationSyntax methodSyntax)
            return false;

        SyntaxNode? bodyExpressionSyntax = methodSyntax.ExpressionBody?.Expression;
        if (bodyExpressionSyntax is null && methodSyntax.Body is not null)
        {
            bodyExpressionSyntax = methodSyntax.Body.Statements.Count switch
            {
                1 when methodSyntax.Body.Statements[0] is ReturnStatementSyntax { Expression: not null } returnStatement => returnStatement.Expression,
                1 when methodSyntax.Body.Statements[0] is ExpressionStatementSyntax expressionStatement => expressionStatement.Expression,
                _ => null
            };
        }

        if (bodyExpressionSyntax is null)
            return false;

        var semanticModel = _snapshot.Compilation.GetSemanticModel(bodyExpressionSyntax.SyntaxTree);
        operation = Unwrap(semanticModel.GetOperation(bodyExpressionSyntax))!;
        return operation is not null;
    }

    private T WithScopedParameterAliases<T>(
        ImmutableArray<IParameterSymbol> parameters,
        IReadOnlyList<string> aliases,
        Func<T> action)
    {
        var previous = _scopedParameterAliases;
        var current = previous is null
            ? new Dictionary<IParameterSymbol, string>(SymbolEqualityComparer.Default)
            : new Dictionary<IParameterSymbol, string>(previous, SymbolEqualityComparer.Default);

        for (var index = 0; index < parameters.Length && index < aliases.Count; index++)
            current[parameters[index]] = aliases[index];

        _scopedParameterAliases = current;
        try
        {
            return action();
        }
        finally
        {
            _scopedParameterAliases = previous;
        }
    }

    private T WithScopedParameterAliases<T>(
        IReadOnlyDictionary<IParameterSymbol, string> aliases,
        Func<T> action)
    {
        var previous = _scopedParameterAliases;
        var current = previous is null
            ? new Dictionary<IParameterSymbol, string>(SymbolEqualityComparer.Default)
            : new Dictionary<IParameterSymbol, string>(previous, SymbolEqualityComparer.Default);

        foreach (var pair in aliases)
            current[pair.Key] = pair.Value;

        _scopedParameterAliases = current;
        try
        {
            return action();
        }
        finally
        {
            _scopedParameterAliases = previous;
        }
    }

    private static string NormalizeArrowFunctionExpressionBody(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            return "undefined";

        if (expression[0] == '{')
            return "(" + expression + ")";

        return expression;
    }

    private bool TryGetAssignedLambdaTarget(IOperation operation, out IOperation target)
    {
        target = default!;
        if (!TryGetAnonymousFunction(operation, out var anonymousFunction))
            return false;

        var body = UnwrapLambdaBody(anonymousFunction.Body);

        if (body is not ISimpleAssignmentOperation assignment)
            return false;

        target = Unwrap(assignment.Target)!;
        return target is not null;
    }

    private static IOperation? UnwrapLambdaBody(IOperation? operation)
    {
        var current = Unwrap(operation);
        while (true)
        {
            switch (current)
            {
                case IBlockOperation block:
                    if (TryGetSingleEffectiveLambdaOperation(block, out var effectiveOperation))
                    {
                        current = Unwrap(effectiveOperation);
                        continue;
                    }

                    return current;
                case IExpressionStatementOperation statement:
                    current = Unwrap(statement.Operation);
                    continue;
                case IReturnOperation returnOperation when returnOperation.ReturnedValue is not null:
                    current = Unwrap(returnOperation.ReturnedValue);
                    continue;
                default:
                    return current;
            }
        }
    }

    private static bool TryGetSingleEffectiveLambdaOperation(
        IBlockOperation block,
        out IOperation effectiveOperation)
    {
        effectiveOperation = default!;
        if (block.Operations.Length == 0)
            return false;

        var effectiveOperations = block.Operations
            .Where(static operation => operation is not IReturnOperation { ReturnedValue: null })
            .ToArray();
        if (effectiveOperations.Length != 1)
            return false;

        effectiveOperation = effectiveOperations[0];
        return true;
    }

    private static bool TryGetAnonymousFunction(IOperation? operation, out IAnonymousFunctionOperation anonymousFunction)
    {
        anonymousFunction = default!;
        var current = UnwrapDelegateCarrier(operation);
        switch (current)
        {
            case IAnonymousFunctionOperation directAnonymousFunction:
                anonymousFunction = directAnonymousFunction;
                return true;
            case IDelegateCreationOperation delegateCreation when UnwrapDelegateCarrier(delegateCreation.Target) is IAnonymousFunctionOperation targetAnonymousFunction:
                anonymousFunction = targetAnonymousFunction;
                return true;
            default:
                return false;
        }
    }

    private static IOperation? UnwrapDelegateCarrier(IOperation? operation)
    {
        var current = Unwrap(operation);
        while (true)
        {
            switch (current)
            {
                case IConversionOperation conversion:
                    current = Unwrap(conversion.Operand);
                    continue;
                case IDelegateCreationOperation delegateCreation:
                    current = Unwrap(delegateCreation.Target);
                    continue;
                default:
                    return current;
            }
        }
    }

    private static bool IsInferredEventCallback(IInvocationOperation invocation)
        => invocation.TargetMethod.Name == "CreateInferredEventCallback" &&
           string.Equals(
               invocation.TargetMethod.ContainingType?.ToDisplayString(),
               "Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers",
               StringComparison.Ordinal);

    private static string GetBindChangedSymbol(IPropertySymbol property)
        => property.Name + "Changed";

    private string EmitCurrentComponentCallbackReference(ISymbol symbol)
    {
        return EmitCurrentComponentCallbackReference(symbol.Name);
    }

    private string EmitCurrentComponentCallbackReference(string razorAlias)
    {
        if (_emitsByRazorAlias.TryGetValue(razorAlias, out var emitDescriptor))
        {
            var payloadParameterName = GetVueEmitPayloadParameterName(emitDescriptor);
            return payloadParameterName.Length == 0
                ? "() => emit(" + ToJavaScriptString(emitDescriptor.Name) + ")"
                : "(" + payloadParameterName + ") => emit(" + ToJavaScriptString(emitDescriptor.Name) + ", " + payloadParameterName + ")";
        }

        if (_propsByPublicName.TryGetValue(razorAlias, out var propDescriptor))
            return "props." + propDescriptor.Name;

        throw new NotSupportedException(
            $"RazorVue render currently does not support callback member '{razorAlias}' in component '{_snapshot.Descriptor.FullName}'.");
    }

    private static string GetVueEmitPayloadParameterName(VueEmitDescriptor emitDescriptor)
        => string.Equals(emitDescriptor.PayloadTypeName, "void", StringComparison.Ordinal)
            ? string.Empty
            : "__value";

    private RazorVueCompilationIssueException CreateInvalidBindTargetException(IPropertySymbol property)
    {
        var originLocation = property.Locations.FirstOrDefault(static location => location.IsInSource);
        var origin = originLocation is null
            ? null
            : RazorVueSourceOrigin.FromLocation(originLocation, RazorVueOriginKind.Logic);
        var issue = new RazorVueCompilationIssue(
            RazorVueIssueCode.InvalidBindTarget,
            RazorVueIssueSeverity.Error,
            $"Component '{_snapshot.Descriptor.Name}' does not support two-way binding for parameter '{property.Name}'.",
            ImmutableArray<string>.Empty);
        return new RazorVueCompilationIssueException(issue, _snapshot.Descriptor.FullName, origin);
    }

    private static bool IsEventCallbackFactoryCreate(IInvocationOperation invocation)
        => invocation.TargetMethod.Name == "Create" &&
           string.Equals(
               invocation.TargetMethod.ContainingType?.ToDisplayString(),
               "Microsoft.AspNetCore.Components.EventCallbackFactory",
               StringComparison.Ordinal);

    private static bool IsCallableSlotExpression(IOperation operation)
        => Unwrap(operation)?.Type?.TypeKind == TypeKind.Delegate;

    private static string EmitLiteral(ILiteralOperation literal)
    {
        if (!literal.ConstantValue.HasValue || literal.ConstantValue.Value is null)
            return "null";

        return literal.ConstantValue.Value switch
        {
            string text => ToJavaScriptString(text),
            char ch => ToJavaScriptString(ch.ToString()),
            bool value => value ? "true" : "false",
            float value => value.ToString("R", CultureInfo.InvariantCulture),
            double value => value.ToString("R", CultureInfo.InvariantCulture),
            decimal value => value.ToString(CultureInfo.InvariantCulture),
            sbyte value => value.ToString(CultureInfo.InvariantCulture),
            byte value => value.ToString(CultureInfo.InvariantCulture),
            short value => value.ToString(CultureInfo.InvariantCulture),
            ushort value => value.ToString(CultureInfo.InvariantCulture),
            int value => value.ToString(CultureInfo.InvariantCulture),
            uint value => value.ToString(CultureInfo.InvariantCulture),
            long value => value.ToString(CultureInfo.InvariantCulture),
            ulong value => value.ToString(CultureInfo.InvariantCulture),
            _ => Convert.ToString(literal.ConstantValue.Value, CultureInfo.InvariantCulture) ?? "null"
        };
    }

    private static string GetBinaryOperator(BinaryOperatorKind kind)
        => kind switch
        {
            BinaryOperatorKind.Add => "+",
            BinaryOperatorKind.Subtract => "-",
            BinaryOperatorKind.Multiply => "*",
            BinaryOperatorKind.Divide => "/",
            BinaryOperatorKind.Remainder => "%",
            BinaryOperatorKind.Equals => "===",
            BinaryOperatorKind.NotEquals => "!==",
            BinaryOperatorKind.LessThan => "<",
            BinaryOperatorKind.LessThanOrEqual => "<=",
            BinaryOperatorKind.GreaterThan => ">",
            BinaryOperatorKind.GreaterThanOrEqual => ">=",
            BinaryOperatorKind.ConditionalAnd => "&&",
            BinaryOperatorKind.ConditionalOr => "||",
            BinaryOperatorKind.And => "&",
            BinaryOperatorKind.Or => "|",
            BinaryOperatorKind.ExclusiveOr => "^",
            BinaryOperatorKind.LeftShift => "<<",
            BinaryOperatorKind.RightShift => ">>",
            _ => throw new NotSupportedException($"Unsupported RazorVue binary operator: {kind}.")
        };

    private static string GetUnaryOperator(UnaryOperatorKind kind)
        => kind switch
        {
            UnaryOperatorKind.Not => "!",
            UnaryOperatorKind.BitwiseNegation => "~",
            UnaryOperatorKind.Minus => "-",
            UnaryOperatorKind.Plus => "+",
            _ => throw new NotSupportedException($"Unsupported RazorVue unary operator: {kind}.")
        };

    private static string ToJavaScriptString(string value)
        => "\"" + (value ?? string.Empty)
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\r", "\\r")
            .Replace("\n", "\\n") + "\"";

    private static string EscapeTemplateText(string value)
        => (value ?? string.Empty)
            .Replace("\\", "\\\\")
            .Replace("`", "\\`")
            .Replace("${", "\\${");
}
