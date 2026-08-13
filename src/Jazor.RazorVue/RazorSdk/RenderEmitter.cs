using System.Collections.Immutable;
using Acornima;
using Acornima.Ast;
using Jazor.Common;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue.RazorSdk;

/// <summary>
/// Lowers bound BuildRenderTree operations to the final Vue render AST shape.
/// C# member and expression semantics remain delegated to the compiler's SemanticWalker hooks.
/// 此 direct path 只解释 RenderTreeBuilder 协议，再将值表达式交回 compiler，避免双重实现 C# 语义。
/// </summary>
internal static class RenderEmitter
{
    private const string RenderTreeBuilderMetadataName = "Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder";
    private const string MarkupStringMetadataName = "Microsoft.AspNetCore.Components.MarkupString";
    private const string ECMAScriptModuleAttributeMetadataName = "ECMAScript.ECMAScriptModuleAttribute";
    private const string VueLibraryComponentAttributeMetadataName = "ECMAScript.VueContract.VueLibraryComponentAttribute";
    private static readonly SymbolEqualityComparer SymbolComparer = SymbolEqualityComparer.Default;

    public static bool TryEmit(
        Compilation compilation,
        INamedTypeSymbol componentSymbol,
        IMethodSymbol buildRenderTreeMethod,
        IBlockOperation buildRenderTreeBody,
        IReadOnlyDictionary<ISymbol, string>? declaredNames,
        VueInjectRegistry injectRegistry,
        out RenderResult result,
        out string? failure)
    {
        if (compilation is null)
            throw new ArgumentNullException(nameof(compilation));
        if (componentSymbol is null)
            throw new ArgumentNullException(nameof(componentSymbol));
        if (buildRenderTreeMethod is null)
            throw new ArgumentNullException(nameof(buildRenderTreeMethod));
        if (buildRenderTreeBody is null)
            throw new ArgumentNullException(nameof(buildRenderTreeBody));
        if (injectRegistry is null)
            throw new ArgumentNullException(nameof(injectRegistry));

        result = default!;
        failure = null;
        try
        {
            if (buildRenderTreeMethod.Parameters.Length != 1 ||
                !IsRenderTreeBuilder(buildRenderTreeMethod.Parameters[0].Type))
            {
                failure = "RazorVue direct render operation lowering requires BuildRenderTree(RenderTreeBuilder).";
                return false;
            }

            var lowered = new Emitter(compilation, componentSymbol, declaredNames, injectRegistry)
                .EmitBlock(buildRenderTreeBody, BuilderBinding.ForSymbol(buildRenderTreeMethod.Parameters[0]));
            result = new RenderResult(
                lowered.RenderExpression,
                lowered.PreludeStatements,
                lowered.ModuleHoists,
                UsesFragment: lowered.UsesFragment,
                UsesStaticVNode: lowered.UsesStaticVNode,
                UsesBlockTree: lowered.UsesBlockTree,
                UsesHandlerCache: lowered.UsesHandlerCache,
                UsesProps: AstReferenceAnalysis.ReferencesIdentifier(lowered.RenderExpression, "props") ||
                           lowered.PreludeStatements.Any(static statement => AstReferenceAnalysis.ReferencesIdentifier(statement, "props")),
                UsesSlots: lowered.UsesSlots,
                lowered.ImportDeclarations,
                lowered.ReferenceCaptureStateMembers);
            return true;
        }
        catch (OperationTransformationException exception)
        {
            failure = exception.Message;
            return false;
        }
        catch (InvalidOperationException exception)
        {
            failure = exception.Message;
            return false;
        }
    }

    /// <summary>
    /// Stateful one-pass RenderTree operation emitter.
    /// 栈帧和临时变量都归一次 emit 所有，确保嵌套 builder 区域按 Razor 原始顺序关闭。
    /// </summary>
    private sealed class Emitter
    {
        private readonly Compilation _compilation;
        private readonly INamedTypeSymbol _componentSymbol;
        private readonly SemanticWalker _walker;
        private readonly SenseArgument _argument;
        private readonly List<Statement> _preludeStatements = new();
        private readonly List<RenderModuleHoist> _moduleHoists = new();
        private readonly List<VariableDeclaration> _renderFragmentPreludeDeclarations = new();
        private readonly Dictionary<string, int> _localNameCounts = new(StringComparer.Ordinal);
        private readonly ImmutableDictionary<IPropertySymbol, string> _componentSlotNames;
        private readonly VueInjectRegistry _injectRegistry;
        private EmitContext? _activeExpressionContext;
        private readonly HashSet<IMethodSymbol> _activeRenderFragmentHelpers = new(SymbolComparer);
        private readonly HashSet<IPropertySymbol> _activeRenderFragmentProperties = new(SymbolComparer);
        private readonly HashSet<IMethodSymbol> _activeRenderObjectHelpers = new(SymbolComparer);
        private readonly Dictionary<IMethodSymbol, string> _renderFragmentHelperFunctionNames = new(SymbolComparer);
        private readonly HashSet<IMethodSymbol> _emittingRenderFragmentHelperFunctions = new(SymbolComparer);
        private readonly HashSet<ISymbol> _referenceCaptureStateMembers = new(SymbolComparer);
        private readonly Dictionary<ILocalSymbol, IOperation> _compileTimeFrameLocalValues = new(SymbolComparer);
        private readonly HashSet<ILocalSymbol> _erasedRenderObjectLocals = new(SymbolComparer);
        private readonly HashSet<string> _renderLocalNames = new(StringComparer.Ordinal);
        private string? _componentAttributeNormalizerName;
        private bool _usesMergeProps;
        private bool _usesFragment;
        private bool _usesStaticVNode;
        private bool _usesSlots;
        private bool _usesBlockTree;
        private bool _usesHandlerCache;
        private int _nonHoistableRenderScopeDepth;
        private int _staticPropsHoistCount;
        private int _staticVNodeHoistCount;
        private int _handlerCacheCount;

        public Emitter(
            Compilation compilation,
            INamedTypeSymbol componentSymbol,
            IReadOnlyDictionary<ISymbol, string>? declaredNames,
            VueInjectRegistry injectRegistry)
        {
            _compilation = compilation;
            _componentSymbol = componentSymbol;
            _walker = new SemanticWalker(test: false)
            {
                Host = new VueSemanticWalkerHost(
                    componentSymbol,
                    parameterRuntimeNames: BuildComponentParameterNameMap(componentSymbol),
                    memberRuntimeNames: declaredNames,
                    parameterReferenceRewriter: RewriteDirectParameterReference,
                    localReferenceRewriter: RewriteDirectLocalReference,
                    propertyReferenceRewriter: RewriteDirectRenderFragmentParameterReference)
            };
            _argument = new SenseArgument(Sense.Any, UseImportAliases: true);
            _componentSlotNames = BuildComponentSlotNameMap(componentSymbol);
            _injectRegistry = injectRegistry;
        }

        public LoweredRender EmitBlock(IBlockOperation block, BuilderBinding builder)
        {
            var context = new EmitContext(
                builder,
                ImmutableDictionary<IParameterSymbol, IOperation>.Empty.WithComparers(SymbolComparer),
                ImmutableDictionary<IParameterSymbol, string>.Empty.WithComparers(SymbolComparer),
                ImmutableDictionary<ILocalSymbol, string>.Empty.WithComparers(SymbolComparer),
                ImmutableDictionary<ILocalSymbol, DirectRenderFragment>.Empty.WithComparers(SymbolComparer),
                ImmutableDictionary<ILocalSymbol, DirectRenderObject>.Empty.WithComparers(SymbolComparer),
                ImmutableDictionary<ILocalSymbol, INamedTypeSymbol>.Empty.WithComparers(SymbolComparer),
                ImmutableHashSet<ILocalSymbol>.Empty.WithComparer(SymbolComparer),
                _preludeStatements,
                AllowPreludeDeclarations: true,
                Argument: _argument.WithNewScope());
            var state = new RenderState();
            _ = EmitOperations(block.Operations, context, state);
            if (state.Stack.Count != 0)
                throw Unsupported(block, "RazorVue direct render operation lowering found unclosed RenderTreeBuilder frames.");

            var renderExpression = WrapWithExpressionScope(context.Argument, [], state.ToRenderExpression());
            // The final vnode may be assembled from several compiler-lowered children. Anchor
            // that synthetic composition to the BuildRenderTree body so VueModuleBuilder can
            // retain Razor source-map coverage even when a direct frame replaces builder calls.
            // 最终 vnode 组合没有单一子表达式来源，锚定到 BuildRenderTree body 保持 Razor map 可追踪。
            renderExpression.UserData = CreateDirectRenderSourceOrigin(block);
            PruneUnreferencedRenderFragmentDeclarations(_preludeStatements, renderExpression);
            var moduleHoists = PruneUnreferencedModuleHoists(renderExpression, _preludeStatements);
            var usesFragment = _usesFragment || state.UsesFragment || state.Roots.Count > 1;
            var usesStaticVNode = _usesStaticVNode || state.UsesStaticVNode ||
                                  moduleHoists.Any(static hoist =>
                                      AstReferenceAnalysis.ReferencesIdentifier(hoist.Initializer, "createStaticVNode"));
            return new LoweredRender(
                renderExpression,
                _preludeStatements.ToImmutableArray(),
                moduleHoists,
                usesFragment,
                usesStaticVNode,
                _usesBlockTree,
                _usesHandlerCache,
                _usesSlots,
                BuildImportDeclarations(),
                _referenceCaptureStateMembers
                    .OrderBy(static member => member.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), StringComparer.Ordinal)
                    .ToImmutableArray());
        }

        private EmitContext EmitOperations(
            ImmutableArray<IOperation> operations,
            EmitContext context,
            RenderState state)
        {
            foreach (var operation in operations)
            {
                context = EmitOperation(operation, context, state);
                if (context.IsTerminated)
                    break;
            }

            return context;
        }

        private EmitContext EmitOperation(IOperation operation, EmitContext context, RenderState state)
        {
            switch (operation)
            {
                case IExpressionStatementOperation statement:
                    EmitExpressionStatement(statement.Operation, context, state);
                    return context;

                // Expression-bodied RenderFragment helpers expose their builder call directly.
                // Route it through the same lowering path as a block-body expression statement.
                case IInvocationOperation invocation:
                    EmitExpressionStatement(invocation, context, state);
                    return context;

                case IBlockOperation block:
                    return EmitOperations(block.Operations, context, state);

                case IVariableDeclarationGroupOperation declarationGroup:
                    return EmitVariableDeclarationGroup(declarationGroup, context, state);

                case IConditionalOperation conditional:
                    EmitConditional(conditional, context, state);
                    return context;

                case IForEachLoopOperation forEachLoop:
                    EmitForEachLoop(forEachLoop, context, state);
                    return context;

                case IReturnOperation:
                    return context with { IsTerminated = true };

                default:
                    throw Unsupported(
                        operation,
                        "RazorVue direct render operation lowering only supports straight-line RenderTreeBuilder statements in this slice. Operation: '" +
                        operation.Kind + "'. Syntax: '" + operation.Syntax + "'.");
            }
        }

        private void EmitExpressionStatement(IOperation expression, EmitContext context, RenderState state)
        {
            while (expression is IConversionOperation conversion)
                expression = conversion.Operand;

            if (expression is ISimpleAssignmentOperation { Target: IDiscardOperation } discardAssignment)
            {
                expression = discardAssignment.Value;
                while (expression is IConversionOperation discardedConversion)
                    expression = discardedConversion.Operand;
            }

            // Razor SDK emits this only to retain binding metadata in generated C#:
            // var (_, _) = (nameof(Component.Value), 0). It has no render-time role.
            // Erase it only after proving that every target is a discard and the RHS
            // is compile-time-only, so user-authored or observable assignments stay visible.
            if (expression is IDeconstructionAssignmentOperation deconstructionAssignment &&
                IsPureDiscardDeconstructionAssignment(deconstructionAssignment))
            {
                return;
            }

            // RenderTreeBuilder's imperative protocol is represented by invocation statements.
            // Other C# statements need an explicit direct-render model instead of being guessed.
            // direct render 不把任意表达式当 vnode 副作用，未建模的语句必须明确失败。
            if (expression is not IInvocationOperation invocation)
                throw Unsupported(
                    expression,
                    "RazorVue direct render operation lowering only supports invocation statements. Operation: '" +
                    expression.Kind + "'.");

            if (IsSecondaryBuilderInvocation(invocation, context))
                return;

            if (TryEmitHelperInvocation(invocation, context, state))
                return;

            if (TryEmitRenderFragmentInvoke(invocation, context, state))
                return;

            if (TryEmitRenderTreeBuilderInvocation(invocation, context, state))
                return;

            if (context.AllowPreludeDeclarations && state.Stack.Count == 0 && state.Roots.Count == 0)
            {
                context.PreludeStatements.Add(new NonSpecialExpressionStatement(LowerExpression(invocation, context)));
                return;
            }

            throw Unsupported(invocation, "RazorVue direct render operation lowering does not support invocation '" +
                                          invocation.TargetMethod.OriginalDefinition.ToDisplayString(Format.NameFormat) +
                                          "' in BuildRenderTree.");
        }

        private static bool IsPureDiscardDeconstructionAssignment(
            IDeconstructionAssignmentOperation operation)
            => IsDiscardDeconstructionTarget(operation.Target) &&
               IsCompileTimeOnlyDeconstructionValue(operation.Value);

        private static bool IsDiscardDeconstructionTarget(IOperation operation)
            => operation switch
            {
                IDiscardOperation => true,
                IDeclarationExpressionOperation declaration
                    => IsDiscardDeconstructionTarget(declaration.Expression),
                ITupleOperation tuple
                    => tuple.Elements.All(IsDiscardDeconstructionTarget),
                IConversionOperation conversion
                    => IsDiscardDeconstructionTarget(conversion.Operand),
                _ => false
            };

        private static bool IsCompileTimeOnlyDeconstructionValue(IOperation operation)
            => operation switch
            {
                INameOfOperation => true,
                ILiteralOperation => true,
                ITupleOperation tuple
                    => tuple.Elements.All(IsCompileTimeOnlyDeconstructionValue),
                IConversionOperation conversion
                    => IsCompileTimeOnlyDeconstructionValue(conversion.Operand),
                _ => false
            };

        private EmitContext EmitVariableDeclarationGroup(
            IVariableDeclarationGroupOperation declarationGroup,
            EmitContext context,
            RenderState state)
        {
            var localAliases = context.LocalAliases;
            var localRenderFragments = context.LocalRenderFragments;
            var localRenderObjects = context.LocalRenderObjects;
            var localComponentTypes = context.LocalComponentTypes;
            var secondaryBuilders = context.SecondaryBuilders;
            var preludeStatements = state.Roots.Count == 0
                ? context.PreludeStatements
                : state.PendingPreludeStatements;
            foreach (var declaration in declarationGroup.Declarations)
            {
                foreach (var declarator in declaration.Declarators)
                {
                    if (declarator.Initializer is null)
                        throw Unsupported(declarator, "Local declarations in direct render lowering must have an initializer.");

                    if (TryResolveTypeOfExpression(declarator.Initializer.Value, localComponentTypes, out var componentType))
                    {
                        localComponentTypes = localComponentTypes.SetItem(declarator.Symbol, componentType);
                        continue;
                    }

                    if (IsRenderTreeBuilder(declarator.Symbol.Type))
                    {
                        secondaryBuilders = secondaryBuilders.Add(declarator.Symbol);
                        continue;
                    }

                    if (state.Stack.Count != 0 && TryTrackCompileTimeFrameLocal(declarator))
                        continue;

                    if (!context.AllowPreludeDeclarations || state.Stack.Count != 0)
                    {
                        var location = declarator.Syntax.GetLocation().GetMappedLineSpan();
                        var source = location.IsValid
                            ? location.Path + "(" + (location.StartLinePosition.Line + 1) + "," + (location.StartLinePosition.Character + 1) + ")"
                            : "<unknown source>";
                        throw Unsupported(
                            declarator,
                            "Runtime local declarations in direct render lowering are only supported outside open RenderTreeBuilder frames. Declaration: '" +
                            declarator.Syntax.ToString().Replace('\r', ' ').Replace('\n', ' ') +
                            "'. Source: " + source + ".");
                    }

                    var localName = CreateUniqueLocalName(declarator.Symbol.Name);
                    var declarationContext = context with
                    {
                        LocalAliases = localAliases,
                        LocalRenderFragments = localRenderFragments,
                        LocalRenderObjects = localRenderObjects,
                        LocalComponentTypes = localComponentTypes,
                        SecondaryBuilders = secondaryBuilders
                    };

                    if (TryResolveRenderObjectExpression(declarator.Initializer.Value, declarationContext, out var renderObject) &&
                        IsRenderFragmentDescriptorType(declarator.Symbol.Type))
                    {
                        // A render-only descriptor has no Vue runtime representation. Keeping
                        // its C# helper would preserve an unbound RenderTreeBuilder callback in
                        // the emitted module. Descriptors with any other instance state remain
                        // runtime values because later expressions may observe that state.
                        localRenderObjects = localRenderObjects.SetItem(declarator.Symbol, renderObject);
                        _erasedRenderObjectLocals.Add(declarator.Symbol);
                        continue;
                    }

                    // MarkupString is a Razor transport marker, not a CLR value retained by
                    // the Vue artifact. Route local initializers through the same payload
                    // projection used by AddContent so helper locals never fall back to a
                    // synthetic `new MarkupString(...)` runtime shape.
                    // MarkupString 局部变量只保留 raw HTML payload；不能让已退休的 builder
                    // 协议或普通 object creation 再承担它的擦除职责。
                    var valueExpression = IsMarkupString(declarator.Symbol.Type) ||
                                          IsNullableMarkupString(declarator.Symbol.Type)
                        ? LowerMarkupStringExpression(declarator.Initializer.Value, declarationContext)
                        : LowerExpression(declarator.Initializer.Value, declarationContext);
                    var runtimeDeclaration = new VariableDeclaration(
                        VariableDeclarationKind.Const,
                        NodeList.From(new VariableDeclarator(new Identifier(localName), valueExpression)));
                    preludeStatements.Add(runtimeDeclaration);
                    localAliases = localAliases.SetItem(declarator.Symbol, localName);
                    // A render local is recreated whenever this expression scope runs. An
                    // inline event closure that captures it cannot use the setup cache, or the
                    // first render's local value would leak into later branches/renders.
                    // render-local 每次求值都会重建；捕获它的 handler 绝不能复用首轮缓存。
                    _renderLocalNames.Add(localName);
                    if (TryResolveRenderFragmentExpression(declarator.Initializer.Value, declarationContext, out var renderFragment))
                    {
                        localRenderFragments = localRenderFragments.SetItem(declarator.Symbol, renderFragment);
                        _renderFragmentPreludeDeclarations.Add(runtimeDeclaration);
                    }
                    if (TryResolveRenderObjectExpression(declarator.Initializer.Value, declarationContext, out renderObject))
                        localRenderObjects = localRenderObjects.SetItem(declarator.Symbol, renderObject);
                }
            }

            return context with
            {
                LocalAliases = localAliases,
                LocalRenderFragments = localRenderFragments,
                LocalRenderObjects = localRenderObjects,
                LocalComponentTypes = localComponentTypes,
                SecondaryBuilders = secondaryBuilders
            };
        }

        private bool TryTrackCompileTimeFrameLocal(IVariableDeclaratorOperation declarator)
        {
            var value = UnwrapTransparentRazorSgOperation(declarator.Initializer!.Value);
            if (!value.ConstantValue.HasValue)
                return false;

            // A compile-time value has no evaluation side effect, so inlining it keeps
            // source order intact even when Razor SG declares it inside an open frame.
            _compileTimeFrameLocalValues[declarator.Symbol] = value;
            return true;
        }

        private bool TryGetKnownConstantString(IOperation operation, out string value)
        {
            operation = UnwrapTransparentRazorSgOperation(operation);
            if (operation is ILocalReferenceOperation localReference &&
                _compileTimeFrameLocalValues.TryGetValue(localReference.Local, out var localValue))
            {
                operation = localValue;
            }

            return TryGetConstantString(operation, out value);
        }

        private void EmitConditional(IConditionalOperation conditional, EmitContext context, RenderState state)
        {
            // A conditional can either select vnode content or contribute a prop branch. Probe the
            // latter before lowering child bodies because props are only legal before children.
            // 条件属性必须在 children 开始前合并；否则同一 conditional 要作为 vnode 内容分支处理。
            var condition = LowerExpression(conditional.Condition, context);
            if (TryEmitConditionalAttribute(conditional, condition, context, state))
                return;

            if (state.Stack.Count == 0 &&
                state.Roots.Count == 0 &&
                IsTerminatingWithoutOutput(conditional.WhenTrue) &&
                IsNoOutputOperation(conditional.WhenFalse))
            {
                state.AddGuard(new NonUpdateUnaryExpression(Operator.LogicalNot, condition));
                return;
            }

            if (state.Stack.Count == 0 &&
                state.Roots.Count == 0 &&
                IsNoOutputOperation(conditional.WhenTrue) &&
                conditional.WhenFalse is not null &&
                IsTerminatingWithoutOutput(conditional.WhenFalse))
            {
                state.AddGuard(condition);
                return;
            }

            var whenTrue = EmitChildContentExpression(conditional.WhenTrue, context);
            var whenFalse = conditional.WhenFalse is null
                ? new DirectRenderFragmentBody(Null(), UsesFragment: false, UsesStaticVNode: false)
                : EmitChildContentExpression(conditional.WhenFalse, context);
            state.AddChild(new ConditionalExpression(condition, whenTrue.RenderExpression, whenFalse.RenderExpression));
            state.UsesFragment = state.UsesFragment || whenTrue.UsesFragment || whenFalse.UsesFragment;
            state.UsesStaticVNode = state.UsesStaticVNode || whenTrue.UsesStaticVNode || whenFalse.UsesStaticVNode;

            if (state.Stack.Count == 0 && IsTerminatingOperation(conditional.WhenTrue) && !IsTerminatingOperation(conditional.WhenFalse))
                state.AddGuard(new NonUpdateUnaryExpression(Operator.LogicalNot, condition));
            else if (state.Stack.Count == 0 && !IsTerminatingOperation(conditional.WhenTrue) && IsTerminatingOperation(conditional.WhenFalse))
                state.AddGuard(condition);
        }

        private bool TryEmitConditionalAttribute(
            IConditionalOperation conditional,
            Expression condition,
            EmitContext context,
            RenderState state)
        {
            if (state.Stack.Count == 0 ||
                state.Stack.Peek() is not PropFrame frame ||
                frame.ChildrenStarted)
            {
                return false;
            }

            if (!TryGetAttributeInvocations(conditional.WhenTrue, out var whenTrueInvocations) ||
                !TryGetAttributeInvocations(conditional.WhenFalse, out var whenFalseInvocations) ||
                whenTrueInvocations.Any(invocation => !TryGetRenderTreeBuilderReceiver(invocation, context, out _)) ||
                whenFalseInvocations.Any(invocation => !TryGetRenderTreeBuilderReceiver(invocation, context, out _)) ||
                whenTrueInvocations.Length == 0 && whenFalseInvocations.Length == 0)
                return false;

            // Keep both branches as prop sources instead of executing either during emission.
            // mergeProps will evaluate the selected runtime branch in the original render pass.
            // 条件分支不能在生成期拍平，必须由运行时选择以保留属性值的单次求值。
            frame.AddConditionalAttributes(
                condition,
                BuildConditionalAttributes(whenTrueInvocations, context, frame),
                BuildConditionalAttributes(whenFalseInvocations, context, frame));
            _usesMergeProps = true;
            return true;
        }

        private ImmutableArray<DirectAttribute> BuildConditionalAttributes(
            ImmutableArray<IInvocationOperation> invocations,
            EmitContext context,
            PropFrame frame)
        {
            var attributes = ImmutableArray.CreateBuilder<DirectAttribute>(invocations.Length);
            foreach (var invocation in invocations)
            {
                EnsureSignature(invocation, invocation.Arguments.Length is 2 or 3);
                RequireOmittableSequence(invocation.Arguments[0].Value);
                if (!TryGetConstantString(invocation.Arguments[1].Value, out var name))
                    throw Unsupported(invocation.Arguments[1].Value, "Attribute names must be compile-time strings for direct render lowering.");

                var methodName = invocation.TargetMethod.OriginalDefinition.Name;
                if (string.Equals(methodName, "AddComponentParameter", StringComparison.Ordinal) &&
                    frame is not ComponentFrame)
                {
                    throw Unsupported(invocation, "AddComponentParameter requires an open component.");
                }

                if (invocation.Arguments.Length == 3 &&
                    (IsRenderFragmentOperationValue(invocation.Arguments[2].Value) ||
                     IsGenericRenderFragmentOperationValue(invocation.Arguments[2].Value)))
                {
                    throw Unsupported(
                        invocation.Arguments[2].Value,
                        "Conditional RenderFragment component parameters are not supported by direct render lowering.");
                }

                var value = invocation.Arguments.Length == 2
                    ? new BooleanLiteral(true, "true")
                    : LowerExpression(invocation.Arguments[2].Value, context);
                attributes.Add(new DirectAttribute(frame.NormalizeAttributeName(name), value));
            }

            return attributes.ToImmutable();
        }

        private void EmitForEachLoop(IForEachLoopOperation forEachLoop, EmitContext context, RenderState state)
        {
            // Array.from maps lazily at render time and supplies each iteration its own alias
            // scope. Building children once outside the mapper would duplicate/collapse loop effects.
            // 循环体必须留在 mapper 内运行，不能在 lowering 时预先展开或共享局部变量。
            var collection = LowerExpression(forEachLoop.Collection, context);
            Node mapperParameter;
            EmitContext loopContext;
            if (TryResolveLoopControlVariable(forEachLoop.LoopControlVariable, out var loopVariable))
            {
                var itemName = SanitizeJavaScriptIdentifierPart(loopVariable.Name, "item");
                mapperParameter = new Identifier(itemName);
                _renderLocalNames.Add(itemName);
                loopContext = context with
                {
                    LocalAliases = context.LocalAliases.SetItem(loopVariable, itemName)
                };
            }
            else
            {
                loopContext = CreateForEachDeconstructionContext(forEachLoop, context);
                mapperParameter = LowerForEachLoopBinding(forEachLoop, loopContext);
            }

            DirectRenderFragmentBody body;
            try
            {
                body = EmitNonHoistableChildContentExpression(forEachLoop.Body, loopContext);
            }
            finally
            {
                foreach (var name in loopContext.LocalAliases.Values.Except(context.LocalAliases.Values, StringComparer.Ordinal))
                    _renderLocalNames.Remove(name);
            }
            var mapper = new ArrowFunctionExpression(
                NodeList.From<Node>(mapperParameter),
                body.RenderExpression,
                expression: true,
                async: false);
            state.AddChild(Call(
                new MemberExpression(new Identifier("Array"), new Identifier("from"), computed: false, optional: false),
                new LogicalExpression(Operator.NullishCoalescing, collection, CreateArray([])),
                mapper));
            state.UsesFragment = state.UsesFragment || body.UsesFragment;
            state.UsesStaticVNode = state.UsesStaticVNode || body.UsesStaticVNode;
        }

        private EmitContext CreateForEachDeconstructionContext(IForEachLoopOperation operation, EmitContext context)
        {
            var localAliases = context.LocalAliases.ToBuilder();
            var locals = GetLoopControlLocals(operation.LoopControlVariable).ToArray();
            if (locals.Length == 0)
            {
                throw Unsupported(
                    operation,
                    "Foreach direct render lowering requires a local loop variable or a local deconstruction target.");
            }

            foreach (var local in locals)
                localAliases[local] = CreateUniqueLocalName(local.Name);

            return context with { LocalAliases = localAliases.ToImmutable() };
        }

        private Node LowerForEachLoopBinding(IForEachLoopOperation operation, EmitContext context)
        {
            var previousContext = _activeExpressionContext;
            _activeExpressionContext = context;
            try
            {
                var binding = _walker.BuildForEachLoopBinding(operation, context.Argument);
                if (binding is VariableDeclaration { Declarations.Count: 1 } declaration)
                    return declaration.Declarations[0].Id;

                throw Unsupported(operation, "Compiler foreach binding did not produce one mapper parameter.");
            }
            finally
            {
                _activeExpressionContext = previousContext;
            }
        }

        private static ImmutableArray<ILocalSymbol> GetLoopControlLocals(IOperation operation)
        {
            var seen = new HashSet<ILocalSymbol>(SymbolComparer);
            var locals = ImmutableArray.CreateBuilder<ILocalSymbol>();

            void Add(ILocalSymbol local)
            {
                if (seen.Add(local))
                    locals.Add(local);
            }

            if (operation is ILocalReferenceOperation localReference)
                Add(localReference.Local);

            foreach (var descendant in operation.Descendants())
            {
                if (descendant is ILocalReferenceOperation descendantReference)
                    Add(descendantReference.Local);
            }

            return locals.ToImmutable();
        }

        private DirectRenderFragmentBody EmitChildContentExpression(IOperation operation, EmitContext context)
        {
            return WithScopedLocalNames(() =>
            {
                var childState = new RenderState();
                var preludeStatements = new List<Statement>();
                var childArgument = context.Argument.WithNewScope();
                _ = EmitOperation(operation, context with
                {
                    PreludeStatements = preludeStatements,
                    AllowPreludeDeclarations = true,
                    Argument = childArgument
                }, childState);
                if (childState.Stack.Count != 0)
                {
                    throw Unsupported(
                        operation,
                        "Structured direct render lowering left unclosed " +
                        childState.Stack.Peek().Describe() +
                        " frames.");
                }

                if (childState.Roots.Count > 1)
                    childState.UsesFragment = true;

                return new DirectRenderFragmentBody(
                    WrapWithExpressionScope(childArgument, preludeStatements, childState.ToRenderExpression()),
                    childState.UsesFragment,
                    childState.UsesStaticVNode);
            });
        }

        private DirectRenderFragmentBody EmitNonHoistableChildContentExpression(
            IOperation operation,
            EmitContext context)
        {
            _nonHoistableRenderScopeDepth++;
            try
            {
                return EmitChildContentExpression(operation, context);
            }
            finally
            {
                _nonHoistableRenderScopeDepth--;
            }
        }

        private DirectRenderFragmentBody EmitRenderFragmentBodyExpression(
            IParameterSymbol builder,
            IOperation body,
            EmitContext context,
            IOperation sourceOperation,
            string description)
        {
            _nonHoistableRenderScopeDepth++;
            try
            {
                return WithScopedLocalNames(() =>
                {
                    var slotState = new RenderState();
                    var preludeStatements = new List<Statement>();
                    var slotArgument = context.Argument.WithNewScope();
                    _ = EmitOperation(
                        body,
                        new EmitContext(
                            BuilderBinding.ForSymbol(builder),
                            context.Substitutions,
                            context.ParameterAliases,
                            context.LocalAliases,
                            context.LocalRenderFragments,
                            context.LocalRenderObjects,
                            context.LocalComponentTypes,
                            context.SecondaryBuilders,
                            preludeStatements,
                            AllowPreludeDeclarations: true,
                            Argument: slotArgument),
                        slotState);
                    if (slotState.Stack.Count != 0)
                    {
                        throw Unsupported(
                            sourceOperation,
                            description +
                            " left unclosed " +
                            slotState.Stack.Peek().Describe() +
                            " frames.");
                    }

                    var usesFragment = slotState.UsesFragment || slotState.Roots.Count > 1;
                    return new DirectRenderFragmentBody(
                        WrapWithExpressionScope(slotArgument, preludeStatements, slotState.ToRenderExpression()),
                        usesFragment,
                        slotState.UsesStaticVNode);
                });
            }
            finally
            {
                _nonHoistableRenderScopeDepth--;
            }
        }

        private bool TryEmitRenderTreeBuilderInvocation(
            IInvocationOperation invocation,
            EmitContext context,
            RenderState state)
        {
            if (!TryGetRenderTreeBuilderReceiver(invocation, context, out _))
            {
                return false;
            }

            // Match SDK names only after receiver identity is proven. A user method sharing an
            // OpenElement name must remain ordinary C# lowering and never mutate this frame stack.
            // 先确认 builder receiver，避免同名用户方法误操作 RenderTree 栈。
            var method = invocation.TargetMethod.OriginalDefinition;
            switch (method.Name)
            {
                case "OpenElement":
                    EnsureSignature(invocation, method.Parameters.Length == 2);
                    RequireOmittableSequence(invocation.Arguments[0].Value);
                    var tagExpression = LowerExpression(invocation.Arguments[1].Value, context);
                    if (!TryGetConstantString(invocation.Arguments[1].Value, out var tagName))
                        throw Unsupported(invocation.Arguments[1].Value, "OpenElement tag names must be compile-time strings for direct render lowering.");
                    state.StartChildren();
                    state.Stack.Push(new ElementFrame(
                        tagExpression,
                        tagName,
                        HoistStaticProps,
                        CanHoistStaticProps,
                        CacheStableEventHandler,
                        CanCacheStableEventHandler,
                        handler => IsStableEventHandler(handler, context),
                        UseBlockTree));
                    return true;

                case "CloseElement":
                    EnsureSignature(invocation, method.Parameters.Length == 0);
                    state.Close<ElementFrame>(invocation);
                    return true;

                case "OpenComponent":
                    EnsureSignature(invocation, method.Name == "OpenComponent");
                    RequireOmittableSequence(invocation.Arguments[0].Value);
                    var componentType = ResolveOpenComponentType(invocation, context);
                    var runtimeComponentType = _injectRegistry.ResolveImplementation(componentType);
                    var componentExpression = BindComponentImport(componentType);
                    var parameterNameMap = BuildComponentParameterNameMap(runtimeComponentType);
                    var slotNameMap = BuildComponentSlotParameterNameMap(runtimeComponentType);
                    state.StartChildren();
                    state.Stack.Push(new ComponentFrame(
                        componentExpression,
                        parameterNameMap,
                        slotNameMap,
                        HoistStaticProps,
                        CanHoistStaticProps,
                        CacheStableEventHandler,
                        CanCacheStableEventHandler,
                        handler => IsStableEventHandler(handler, context),
                        UseBlockTree));
                    return true;

                case "CloseComponent":
                    EnsureSignature(invocation, method.Parameters.Length == 0);
                    state.Close<ComponentFrame>(invocation);
                    return true;

                case "OpenRegion":
                    EnsureSignature(invocation, method.Parameters.Length == 1);
                    RequireOmittableSequence(invocation.Arguments[0].Value);
                    state.StartChildren();
                    state.Stack.Push(new RegionFrame());
                    return true;

                case "CloseRegion":
                    EnsureSignature(invocation, method.Parameters.Length == 0);
                    state.Close<RegionFrame>(invocation);
                    return true;

                case "AddAttribute":
                    EmitAddAttribute(invocation, context, state);
                    return true;

                case "AddComponentParameter":
                    EmitAddComponentParameter(invocation, context, state);
                    return true;

                case "AddMultipleAttributes":
                    EmitAddMultipleAttributes(invocation, context, state);
                    return true;

                case "AddContent":
                    EmitAddContent(invocation, context, state);
                    return true;

                case "AddMarkupContent":
                    EnsureSignature(invocation, invocation.Arguments.Length == 2);
                    RequireOmittableSequence(invocation.Arguments[0].Value);
                    state.UsesStaticVNode = true;
                    state.AddChild(TryCreateStaticMarkupVNode(
                        invocation.Arguments[1].Value,
                        allowRawStringLiteral: true,
                        out var staticVNode)
                        ? staticVNode
                        : Call(
                            "createStaticVNode",
                            LowerExpression(invocation.Arguments[1].Value, context),
                            new NumericLiteral(1, "1")));
                    return true;

                case "AddElementReferenceCapture":
                    EmitReferenceCapture(invocation, context, state, component: false);
                    return true;

                case "AddComponentReferenceCapture":
                    EmitReferenceCapture(invocation, context, state, component: true);
                    return true;

                case "AddComponentRenderMode":
                    EmitComponentRenderMode(invocation, context, state);
                    return true;

                case "Clear":
                    EnsureSignature(invocation, invocation.Arguments.Length == 0);
                    state.Clear();
                    return true;

                case "Dispose":
                    EnsureSignature(invocation, invocation.Arguments.Length == 0);
                    return true;

                case "SetAttributeValue":
                    EmitSetAttributeValue(invocation, context, state);
                    return true;

                case "SetKey":
                    EmitSetKey(invocation, context, state);
                    return true;

                case "SetUpdatesAttributeName":
                    EmitSetUpdatesAttributeName(invocation, context, state);
                    return true;

                case "AddEventPreventDefaultAttribute":
                    EmitAddEventModifier(invocation, context, state, preventDefault: true, stopPropagation: false);
                    return true;

                case "AddEventStopPropagationAttribute":
                    EmitAddEventModifier(invocation, context, state, preventDefault: false, stopPropagation: true);
                    return true;

                case "AddNamedEvent":
                    EmitAddNamedEvent(invocation, state);
                    return true;

                default:
                    throw Unsupported(invocation, "RenderTreeBuilder." + method.Name + " is not supported by direct render operation lowering yet.");
            }
        }

        private static bool TryGetRenderTreeBuilderReceiver(
            IInvocationOperation invocation,
            EmitContext context,
            out IOperation receiver)
        {
            receiver = null!;
            if (invocation.Instance is null)
            {
                if (IsRenderTreeBuilderMethod(invocation.TargetMethod) &&
                    IsRenderTreeBuilderMetadataMethodName(invocation.TargetMethod.Name))
                {
                    receiver = invocation;
                    return true;
                }

                if (invocation.Arguments.Length > 0 &&
                    IsRenderTreeBuilder(invocation.Arguments[0].Value.Type!) &&
                    context.Builder.Matches(invocation.Arguments[0].Value, context.Substitutions))
                {
                    receiver = invocation.Arguments[0].Value;
                    return true;
                }

                return false;
            }

            if (IsRenderTreeBuilderMethod(invocation.TargetMethod) ||
                IsRenderTreeBuilder(invocation.Instance.Type!))
            {
                if (!context.Builder.Matches(invocation.Instance, context.Substitutions))
                    return false;

                receiver = invocation.Instance;
                return true;
            }

            return false;
        }

        private static bool IsRenderTreeBuilderMetadataMethodName(string name)
            => string.Equals(name, "AddEventPreventDefaultAttribute", StringComparison.Ordinal) ||
               string.Equals(name, "AddEventStopPropagationAttribute", StringComparison.Ordinal) ||
               string.Equals(name, "AddNamedEvent", StringComparison.Ordinal);

        private bool TryEmitHelperInvocation(IInvocationOperation invocation, EmitContext context, RenderState state)
        {
            if (!invocation.TargetMethod.ReturnsVoid ||
                invocation.TargetMethod.DeclaringSyntaxReferences.Length != 1 ||
                invocation.Arguments.Length == 0)
            {
                return false;
            }

            var method = invocation.TargetMethod;
            if (!SymbolComparer.Equals(method.ContainingType?.OriginalDefinition, _componentSymbol.OriginalDefinition) &&
                !method.IsStatic)
            {
                return false;
            }

            if (!IsRenderTreeBuilder(method.Parameters[0].Type) ||
                !context.Builder.Matches(invocation.Arguments[0].Value, context.Substitutions))
            {
                return false;
            }

            var syntax = method.DeclaringSyntaxReferences[0].GetSyntax();
            if (syntax is not MethodDeclarationSyntax { Body: not null } methodDeclaration)
                return false;

            var model = _compilation.GetSemanticModel(methodDeclaration.SyntaxTree);
            if (model.GetOperation(methodDeclaration.Body) is not IBlockOperation body)
                return false;

            var substitutions = context.Substitutions.ToBuilder();
            for (var index = 1; index < invocation.Arguments.Length && index < method.Parameters.Length; index++)
                AddParameterSubstitution(substitutions, method, index, invocation.Arguments[index].Value);

            var helperContext = new EmitContext(
                BuilderBinding.ForSymbol(method.Parameters[0]),
                substitutions.ToImmutable(),
                context.ParameterAliases,
                context.LocalAliases,
                context.LocalRenderFragments,
                context.LocalRenderObjects,
                context.LocalComponentTypes,
                context.SecondaryBuilders,
                context.PreludeStatements,
                AllowPreludeDeclarations: context.AllowPreludeDeclarations,
                Argument: context.Argument);
            _ = EmitOperations(body.Operations, helperContext, state);
            return true;
        }

        private bool TryEmitRenderFragmentInvoke(IInvocationOperation invocation, EmitContext context, RenderState state)
        {
            if (!string.Equals(invocation.TargetMethod.Name, "Invoke", StringComparison.Ordinal) ||
                invocation.Arguments.Length != 1 ||
                !IsRenderTreeBuilder(invocation.Arguments[0].Value.Type!) ||
                !context.Builder.Matches(invocation.Arguments[0].Value, context.Substitutions) ||
                invocation.Instance is null ||
                !IsRenderFragmentOperationValue(invocation.Instance))
            {
                return false;
            }

            if (!TryResolveRenderFragmentContentExpression(invocation.Instance, context, out var expression))
                throw Unsupported(invocation, "RenderFragment.Invoke direct lowering requires a known inline, slot, or component-local RenderFragment source.");

            if (expression.ReturnsVueSlotContent)
                state.AddChildSequence(expression.RenderExpression);
            else
                state.AddChild(expression.RenderExpression);
            state.UsesFragment = state.UsesFragment || expression.UsesFragment;
            state.UsesStaticVNode = state.UsesStaticVNode || expression.UsesStaticVNode;
            return true;
        }

        private void EmitAddAttribute(IInvocationOperation invocation, EmitContext context, RenderState state)
        {
            // Razor's frame protocol commits attributes before child content. Enforcing it here
            // keeps duplicate/splat precedence deterministic when the frame later forms a vnode.
            // 属性顺序是 RenderTree ABI 的一部分，children 后追加属性不能被随意重排。
            EnsureSignature(invocation, invocation.Arguments.Length is 2 or 3);
            RequireOmittableSequence(invocation.Arguments[0].Value);
            if (state.Stack.Count == 0 || state.Stack.Peek() is not PropFrame frame || frame.ChildrenStarted)
                throw Unsupported(invocation, "Attributes must be added before children on an open element or component: " + invocation.Syntax);

            var nameOperation = invocation.Arguments[1].Value;
            if (!TryGetConstantString(nameOperation, out var name))
                throw Unsupported(nameOperation, "Attribute names must be compile-time strings for direct render lowering.");

            if (frame is ComponentFrame componentFrame &&
                invocation.Arguments.Length == 3 &&
                TryEmitComponentParameterValue(
                    componentFrame,
                    name,
                    invocation.Arguments[2].Value,
                    context,
                    state))
            {
                return;
            }

            var value = invocation.Arguments.Length == 2
                ? new BooleanLiteral(true, "true")
                : LowerExpression(invocation.Arguments[2].Value, context);
            frame.AddAttribute(new DirectAttribute(frame.NormalizeAttributeName(name), value));
        }

        private void EmitAddComponentParameter(IInvocationOperation invocation, EmitContext context, RenderState state)
        {
            EnsureSignature(invocation, invocation.Arguments.Length == 3);
            RequireOmittableSequence(invocation.Arguments[0].Value);
            if (state.Stack.Count == 0 || state.Stack.Peek() is not ComponentFrame frame || frame.ChildrenStarted)
                throw Unsupported(invocation, "Component parameters must be added before component children.");

            if (!TryGetConstantString(invocation.Arguments[1].Value, out var name))
                throw Unsupported(invocation.Arguments[1].Value, "Component parameter names must be compile-time strings for direct render lowering.");

            if (TryEmitComponentParameterValue(
                    frame,
                    name,
                    invocation.Arguments[2].Value,
                    context,
                    state))
            {
                return;
            }

            frame.AddAttribute(new DirectAttribute(
                frame.NormalizeAttributeName(name),
                LowerExpression(invocation.Arguments[2].Value, context)));
        }

        private bool TryEmitComponentParameterValue(
            ComponentFrame frame,
            string name,
            IOperation valueOperation,
            EmitContext context,
            RenderState state)
        {
            var isDeclaredSlot = frame.TryGetDeclaredSlotName(name, out var slotName);
            if (TryResolveRenderFragmentContentExpression(valueOperation, context, out var forwardedSlotExpression))
            {
                frame.Slots.Add(new DirectSlot(
                    isDeclaredSlot ? slotName : frame.NormalizeSlotName(name),
                    forwardedSlotExpression));
                state.UsesFragment = state.UsesFragment || forwardedSlotExpression.UsesFragment;
                state.UsesStaticVNode = state.UsesStaticVNode || forwardedSlotExpression.UsesStaticVNode;
                return true;
            }

            // Slot identity comes from the target component's declared parameter metadata.
            // Do not infer the protocol from a conventional C# name such as ChildContent.
            if (isDeclaredSlot &&
                !IsRenderFragmentOperationValue(valueOperation) &&
                !IsGenericRenderFragmentOperationValue(valueOperation))
            {
                throw Unsupported(valueOperation, name + " component parameter must be a RenderFragment for direct render lowering.");
            }

            if (IsRenderFragmentOperationValue(valueOperation) || IsGenericRenderFragmentOperationValue(valueOperation))
                throw Unsupported(valueOperation, "RenderFragment component parameters require a resolvable inline, local, helper, or component-slot source.");

            return false;
        }

        private void EmitAddMultipleAttributes(IInvocationOperation invocation, EmitContext context, RenderState state)
        {
            EnsureSignature(invocation, invocation.Arguments.Length == 2);
            RequireOmittableSequence(invocation.Arguments[0].Value);
            if (state.Stack.Count == 0 || state.Stack.Peek() is not PropFrame frame || frame.ChildrenStarted)
                throw Unsupported(invocation, "Multiple attributes must be added before children on an open element or component.");

            if (TryEmitKnownMultipleAttributes(invocation.Arguments[1].Value, context, frame))
                return;

            var attributes = LowerExpression(invocation.Arguments[1].Value, context);
            if (frame is ComponentFrame component)
                // Runtime splats have no C# operation per entry. Normalize them in the
                // generated setup scope so descriptor-owned Vue names still win.
                // 动态 splat 无法逐项绑定；必须先规范化再进入 mergeProps 的覆盖顺序。
                attributes = NormalizeDynamicComponentAttributes(component, attributes);

            if (frame.AddMultipleAttributes(attributes))
                _usesMergeProps = true;
        }

        private Expression NormalizeDynamicComponentAttributes(
            ComponentFrame component,
            Expression attributes)
        {
            var helperName = _componentAttributeNormalizerName;
            if (helperName is null)
            {
                helperName = CreateUniqueLocalName("__normalizeComponentAttributes");
                _componentAttributeNormalizerName = helperName;
                _preludeStatements.Add(BuildComponentAttributeNormalizer(helperName));
            }

            return Call(
                new Identifier(helperName),
                attributes,
                component.CreateParameterNameMapExpression());
        }

        private Expression HoistStaticProps(ObjectExpression props)
        {
            var name = "__jazor$hoistedProps" +
                _staticPropsHoistCount.ToString(System.Globalization.CultureInfo.InvariantCulture);
            _staticPropsHoistCount++;
            _moduleHoists.Add(new RenderModuleHoist(name, props));
            return new Identifier(name);
        }

        private Expression CacheStableEventHandler(Expression handler)
        {
            var index = _handlerCacheCount++;
            _usesHandlerCache = true;
            var cache = new MemberExpression(
                new Identifier("__jazor$handlerCache"),
                new NumericLiteral(index, index.ToString(System.Globalization.CultureInfo.InvariantCulture)),
                computed: true,
                optional: false);
            // The cache belongs to one setup instance. `||` has the same lazily-created
            // handler behavior as Vue's SFC cache while avoiding a render-time allocation.
            // 缓存位于 setup 实例而非模块；每个组件实例保留自己的闭包与事件身份。
            return new LogicalExpression(
                Operator.LogicalOr,
                cache,
                new AssignmentExpression(Operator.Assignment, cache, handler));
        }

        private bool CanCacheStableEventHandler(Expression handler)
        {
            if (_nonHoistableRenderScopeDepth != 0 || !IsInlineFunction(handler))
                return false;

            // A loop/slot local would be captured by the first render and become stale. The
            // direct emitter records aliases while entering those lexical scopes, so checking
            // references here is conservative and independent of AST traversal order.
            // foreach/slot 局部一旦被缓存会固定首轮值，因此只缓存不捕获 render-local 的闭包。
            return !_renderLocalNames.Any(name => AstReferenceAnalysis.ReferencesIdentifier(handler, name));
        }

        private bool IsStableEventHandler(Expression handler, EmitContext context)
        {
            if (handler is Identifier identifier)
            {
                // A named component member is emitted once in setup, whereas a direct-render
                // local or template parameter is recreated by the render function. Only the
                // former can be omitted from Vue's dynamic-prop list.
                // 同名 Identifier 不能一概当稳定：render local/template 参数每轮会变化，必须参与 patch。
                return !_renderLocalNames.Contains(identifier.Name) &&
                       !context.LocalAliases.Values.Contains(identifier.Name, StringComparer.Ordinal) &&
                       !context.ParameterAliases.Values.Contains(identifier.Name, StringComparer.Ordinal);
            }

            return CanCacheStableEventHandler(handler);
        }

        private void UseBlockTree()
            => _usesBlockTree = true;

        private static bool IsInlineFunction(Expression expression)
            => expression is ArrowFunctionExpression or FunctionExpression;

        private bool CanHoistStaticProps(ObjectExpression props)
        {
            if (_nonHoistableRenderScopeDepth != 0 || props.Properties.Count == 0)
                return false;

            foreach (var member in props.Properties)
            {
                if (member is not ObjectProperty
                    {
                        Computed: false,
                        Key: Identifier or Acornima.Ast.StringLiteral,
                        Value: Expression value
                    } property ||
                    !IsStaticPropValue(value))
                {
                    return false;
                }

                var name = property.Key switch
                {
                    Identifier identifier => identifier.Name,
                    Acornima.Ast.StringLiteral literal => literal.Value,
                    _ => string.Empty
                };
                if (string.Equals(name, "key", StringComparison.Ordinal) ||
                    string.Equals(name, "ref", StringComparison.Ordinal) ||
                    IsDirectEventAttributeName(name))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsStaticPropValue(Expression expression)
            => expression is NullLiteral or Acornima.Ast.StringLiteral or BooleanLiteral or NumericLiteral or BigIntLiteral;

        private bool TryCreateStaticMarkupVNode(
            IOperation operation,
            bool allowRawStringLiteral,
            out Expression vnode)
        {
            if (_nonHoistableRenderScopeDepth != 0 ||
                !TryGetStaticMarkupText(operation, allowRawStringLiteral, out var markup))
            {
                vnode = null!;
                return false;
            }

            var name = "__jazor$hoistedStatic" +
                _staticVNodeHoistCount.ToString(System.Globalization.CultureInfo.InvariantCulture);
            _staticVNodeHoistCount++;
            _moduleHoists.Add(new RenderModuleHoist(
                name,
                Call(
                    "createStaticVNode",
                    StringLiteral(markup),
                    new NumericLiteral(1, "1"))));
            vnode = new Identifier(name);
            return true;
        }

        private Expression LowerMarkupStringExpression(IOperation operation, EmitContext context)
        {
            operation = UnwrapTransparentRazorSgOperation(operation);
            if (operation is IObjectCreationOperation creation &&
                IsMarkupString(creation.Type) &&
                creation.Arguments.Length == 1)
            {
                // MarkupString is a Razor render marker, not a CLR object in the emitted Vue
                // module. Keep the payload expression's normal compiler lowering and lifetime.
                // MarkupString 只携带 raw HTML payload，不能让 retired host 再负责它的擦除。
                return LowerExpression(creation.Arguments[0].Value, context);
            }

            return LowerExpression(operation, context);
        }

        private void EmitSetAttributeValue(IInvocationOperation invocation, EmitContext context, RenderState state)
        {
            EnsureSignature(invocation, invocation.Arguments.Length == 2);
            RequireOmittableSequence(invocation.Arguments[0].Value);
            if (state.Stack.Count == 0 || state.Stack.Peek() is not PropFrame frame || frame.ChildrenStarted)
                throw Unsupported(invocation, "SetAttributeValue must target the most recent open element or component before children.");

            var value = LowerExpression(invocation.Arguments[1].Value, context);
            if (!frame.TrySetLastAttributeValue(value))
                throw Unsupported(invocation, "SetAttributeValue requires a known preceding attribute in direct render lowering.");
        }

        private void EmitSetKey(IInvocationOperation invocation, EmitContext context, RenderState state)
        {
            EnsureSignature(invocation, invocation.Arguments.Length == 1);
            if (state.Stack.Count == 0 || state.Stack.Peek() is not PropFrame frame || frame.ChildrenStarted)
                throw Unsupported(invocation, "SetKey must target an open element or component before children.");

            frame.AddAttribute(new DirectAttribute("key", LowerExpression(invocation.Arguments[0].Value, context)));
        }

        private void EmitSetUpdatesAttributeName(IInvocationOperation invocation, EmitContext context, RenderState state)
        {
            EnsureSignature(invocation, invocation.Arguments.Length == 1);
            if (state.Stack.Count == 0 || state.Stack.Peek() is not ElementFrame frame || frame.ChildrenStarted)
                throw Unsupported(invocation, "SetUpdatesAttributeName must target an open element before children.");

            if (!TryGetConstantString(invocation.Arguments[0].Value, out var name))
                throw Unsupported(invocation.Arguments[0].Value, "SetUpdatesAttributeName requires a compile-time attribute name for direct render lowering.");

            frame.SetUpdatesAttributeName(name);
        }

        private void EmitAddEventModifier(
            IInvocationOperation invocation,
            EmitContext context,
            RenderState state,
            bool preventDefault,
            bool stopPropagation)
        {
            var offset = GetRenderTreeBuilderReceiverArgumentOffset(invocation);
            var payloadLength = invocation.Arguments.Length - offset;
            EnsureSignature(invocation, payloadLength is 2 or 3);
            if (payloadLength == 3)
                RequireOmittableSequence(invocation.Arguments[offset].Value);
            if (state.Stack.Count == 0 || state.Stack.Peek() is not ElementFrame frame || frame.ChildrenStarted)
                throw Unsupported(invocation, "Event modifier attributes must target an open element before children.");

            var eventNameArgumentIndex = offset + (payloadLength == 3 ? 1 : 0);
            var valueArgumentIndex = offset + (payloadLength == 3 ? 2 : 1);
            if (!TryGetConstantString(invocation.Arguments[eventNameArgumentIndex].Value, out var eventName))
                throw Unsupported(invocation.Arguments[eventNameArgumentIndex].Value, "Event modifier names must be compile-time strings for direct render lowering.");

            var value = LowerExpression(invocation.Arguments[valueArgumentIndex].Value, context);
            if (value is not BooleanLiteral { Value: false })
                frame.SetEventModifier(eventName, value, preventDefault, stopPropagation);
        }

        private void EmitAddNamedEvent(IInvocationOperation invocation, RenderState state)
        {
            var offset = GetRenderTreeBuilderReceiverArgumentOffset(invocation);
            EnsureSignature(invocation, invocation.Arguments.Length - offset == 2);
            if (state.Stack.Count == 0 || state.Stack.Peek() is not ElementFrame frame || frame.ChildrenStarted)
                throw Unsupported(invocation, "Named event metadata must target an open element before children.");

            if (!TryGetKnownConstantString(invocation.Arguments[offset].Value, out var eventName) ||
                !TryGetKnownConstantString(invocation.Arguments[offset + 1].Value, out var assignedEventName) ||
                string.IsNullOrWhiteSpace(eventName) ||
                string.IsNullOrWhiteSpace(assignedEventName))
            {
                throw Unsupported(invocation, "Named event metadata requires compile-time event names for direct render lowering.");
            }
        }

        private void EmitAddContent(IInvocationOperation invocation, EmitContext context, RenderState state)
        {
            EnsureSignature(invocation, invocation.Arguments.Length >= 2);
            RequireOmittableSequence(invocation.Arguments[0].Value);

            if (invocation.Arguments.Length == 3)
            {
                if (!IsGenericRenderFragmentOperationValue(invocation.Arguments[1].Value))
                {
                    throw Unsupported(invocation, "AddContent<TValue> requires a resolvable RenderFragment<TValue> source.");
                }

                var valueExpression = LowerExpression(invocation.Arguments[2].Value, context);
                if (TryResolveComponentSlot(invocation.Arguments[1].Value, out var componentSlotName, out var genericSlot) &&
                    genericSlot)
                {
                    _usesSlots = true;
                    state.AddChildSequence(BuildSlotInvocationExpression(componentSlotName, valueExpression));
                    return;
                }

                if (!TryResolveRenderFragmentContentExpression(invocation.Arguments[1].Value, context, out var scopedFragment) ||
                    scopedFragment.ParameterName is null)
                {
                    throw Unsupported(invocation, "AddContent<TValue> requires a resolvable RenderFragment<TValue> source.");
                }

                var scopedContent = InvokeRenderFragment(scopedFragment, valueExpression);
                if (scopedFragment.ReturnsVueSlotContent)
                    state.AddChildSequence(scopedContent);
                else
                    state.AddChild(scopedContent);
                state.UsesFragment = state.UsesFragment || scopedFragment.UsesFragment;
                state.UsesStaticVNode = state.UsesStaticVNode || scopedFragment.UsesStaticVNode;
                return;
            }

            if (TryResolveRenderFragmentContentExpression(invocation.Arguments[1].Value, context, out var slotExpression))
            {
                if (slotExpression.ReturnsVueSlotContent)
                    state.AddChildSequence(slotExpression.RenderExpression);
                else
                    state.AddChild(slotExpression.RenderExpression);
                state.UsesFragment = state.UsesFragment || slotExpression.UsesFragment;
                state.UsesStaticVNode = state.UsesStaticVNode || slotExpression.UsesStaticVNode;
                return;
            }

            if (IsRenderFragmentOperationValue(invocation.Arguments[1].Value) ||
                IsGenericRenderFragmentOperationValue(invocation.Arguments[1].Value))
            {
                throw Unsupported(invocation.Arguments[1].Value, "RenderFragment content requires a resolvable inline, local, helper, or component-slot source.");
            }

            if (IsMarkupStringOperationValue(invocation.Arguments[1].Value))
            {
                state.UsesStaticVNode = true;
                if (IsNullableMarkupStringOperationValue(invocation.Arguments[1].Value))
                {
                    state.AddOptionalChild(BuildNullableMarkupContent(
                        LowerMarkupStringExpression(invocation.Arguments[1].Value, context)));
                    return;
                }

                state.AddChild(TryCreateStaticMarkupVNode(
                    invocation.Arguments[1].Value,
                    allowRawStringLiteral: false,
                    out var staticVNode)
                    ? staticVNode
                    : Call(
                        "createStaticVNode",
                        LowerMarkupStringExpression(invocation.Arguments[1].Value, context),
                        new NumericLiteral(1, "1")));
                return;
            }

            state.AddChild(LowerExpression(invocation.Arguments[1].Value, context));
        }

        private void EmitReferenceCapture(
            IInvocationOperation invocation,
            EmitContext context,
            RenderState state,
            bool component)
        {
            EnsureSignature(invocation, invocation.Arguments.Length == 2);
            RequireOmittableSequence(invocation.Arguments[0].Value);
            if (state.Stack.Count == 0 ||
                state.Stack.Peek() is not PropFrame frame ||
                frame.ChildrenStarted ||
                component != (frame is ComponentFrame))
            {
                throw Unsupported(
                    invocation,
                    component
                        ? "Component reference captures require the current open component before children."
                        : "Element reference captures require the current open element before children.");
            }

            var capture = invocation.Arguments[1].Value;
            CollectReferenceCaptureStateMembers(capture);
            frame.AddReferenceCapture(LowerExpression(capture, context));
        }

        private void CollectReferenceCaptureStateMembers(IOperation capture)
        {
            capture = UnwrapReferenceCaptureOperation(capture);
            if (capture is not IAnonymousFunctionOperation lambda || lambda.Symbol.Parameters.Length != 1)
                return;

            CollectCaptureAssignments(lambda.Body, lambda.Symbol.Parameters[0]);
        }

        private void CollectCaptureAssignments(IOperation operation, IParameterSymbol captureParameter)
        {
            if (operation is ISimpleAssignmentOperation assignment &&
                IsCaptureParameterValue(assignment.Value, captureParameter) &&
                TryGetCurrentComponentStorageMember(assignment.Target, out var member))
            {
                _referenceCaptureStateMembers.Add(member);
            }

            foreach (var child in operation.ChildOperations)
                CollectCaptureAssignments(child, captureParameter);
        }

        private static IOperation UnwrapReferenceCaptureOperation(IOperation operation)
        {
            while (true)
            {
                switch (operation)
                {
                    case IConversionOperation conversion:
                        operation = conversion.Operand;
                        continue;

                    case IDelegateCreationOperation delegateCreation:
                        operation = delegateCreation.Target;
                        continue;

                    default:
                        return operation;
                }
            }
        }

        private static bool IsCaptureParameterValue(IOperation operation, IParameterSymbol captureParameter)
        {
            operation = UnwrapReferenceCaptureOperation(operation);
            return operation is IParameterReferenceOperation parameterReference &&
                   SymbolComparer.Equals(parameterReference.Parameter, captureParameter);
        }

        private bool TryGetCurrentComponentStorageMember(IOperation operation, out ISymbol member)
        {
            operation = UnwrapReferenceCaptureOperation(operation);
            member = operation switch
            {
                IFieldReferenceOperation fieldReference => fieldReference.Field,
                IPropertyReferenceOperation propertyReference => propertyReference.Property,
                _ => null!
            };

            return member is not null &&
                   !member.IsStatic &&
                   SymbolComparer.Equals(member.ContainingType?.OriginalDefinition, _componentSymbol.OriginalDefinition);
        }

        private void EmitComponentRenderMode(
            IInvocationOperation invocation,
            EmitContext context,
            RenderState state)
        {
            EnsureSignature(invocation, invocation.Arguments.Length == 1);
            if (state.Stack.Count == 0 ||
                state.Stack.Peek() is not ComponentFrame frame ||
                frame.ChildrenStarted)
            {
                throw Unsupported(invocation, "Component render mode metadata requires the current open component before children.");
            }

            // Vue has no server render-mode equivalent. Lowering the argument still
            // validates its C# semantics while the metadata itself is intentionally erased.
            _ = LowerExpression(invocation.Arguments[0].Value, context);
        }

        private bool TryEmitKnownMultipleAttributes(IOperation operation, EmitContext context, PropFrame frame)
        {
            operation = UnwrapTransparentRazorSgOperation(operation);

            if (operation is not IObjectCreationOperation { Initializer: not null } creation)
                return false;

            foreach (var initializer in creation.Initializer.Initializers)
            {
                if (!TryGetAttributeInitializer(initializer, out var keyOperation, out var valueOperation))
                    return false;

                if (!TryGetConstantString(keyOperation, out var name))
                    throw Unsupported(keyOperation, "Bulk attribute names must be compile-time strings for direct render lowering.");

                frame.AddAttribute(new DirectAttribute(
                    frame.NormalizeAttributeName(name),
                    LowerExpression(valueOperation, context)));
            }

            return true;
        }

        private static IOperation UnwrapTransparentRazorSgOperation(IOperation operation)
        {
            while (true)
            {
                switch (operation)
                {
                    case IConversionOperation conversion:
                        operation = conversion.Operand;
                        continue;

                    // Razor SG uses TypeCheck<T> only for generated-C# binding. It has no
                    // runtime behavior, so direct lowering must inspect its real operand.
                    case IInvocationOperation invocation when
                        IsRazorRuntimeHelpersTypeCheck(invocation.TargetMethod) &&
                        invocation.Arguments.Length == 1:
                        operation = invocation.Arguments[0].Value;
                        continue;

                    default:
                        return operation;
                }
            }
        }

        private static bool IsRazorRuntimeHelpersTypeCheck(IMethodSymbol method)
            => method.IsStatic &&
               string.Equals(method.Name, "TypeCheck", StringComparison.Ordinal) &&
               method.ContainingType is { Name: "RuntimeHelpers" } containingType &&
               string.Equals(
                   containingType.ContainingNamespace?.ToDisplayString(),
                   "Microsoft.AspNetCore.Components.CompilerServices",
                   StringComparison.Ordinal);

        private static bool TryGetAttributeInitializer(
            IOperation operation,
            out IOperation keyOperation,
            out IOperation valueOperation)
        {
            keyOperation = null!;
            valueOperation = null!;
            while (operation is IConversionOperation conversion)
                operation = conversion.Operand;

            if (operation is ISimpleAssignmentOperation assignment &&
                TryGetIndexerKey(assignment.Target, out keyOperation))
            {
                valueOperation = assignment.Value;
                return true;
            }

            if (operation is IInvocationOperation invocation &&
                string.Equals(invocation.TargetMethod.Name, "Add", StringComparison.Ordinal) &&
                invocation.Arguments.Length >= 2)
            {
                keyOperation = invocation.Arguments[0].Value;
                valueOperation = invocation.Arguments[1].Value;
                return true;
            }

            return false;
        }

        private static bool TryGetIndexerKey(IOperation operation, out IOperation keyOperation)
        {
            keyOperation = null!;
            while (operation is IConversionOperation conversion)
                operation = conversion.Operand;

            if (operation is IPropertyReferenceOperation propertyReference &&
                propertyReference.Arguments.Length == 1)
            {
                keyOperation = propertyReference.Arguments[0].Value;
                return true;
            }

            return false;
        }

        private static int GetRenderTreeBuilderReceiverArgumentOffset(IInvocationOperation invocation)
            => invocation.Arguments.Length > 0 &&
               invocation.Arguments[0].Value.Type is { } type &&
               IsRenderTreeBuilder(type)
                ? 1
                : 0;

        private Expression LowerExpression(IOperation operation, EmitContext context)
        {
            while (operation is IConversionOperation conversion)
                operation = conversion.Operand;

            if (operation is IParameterReferenceOperation parameterReference &&
                context.Substitutions.TryGetValue(parameterReference.Parameter, out var substituted))
            {
                return LowerExpression(substituted, context);
            }

            if (operation is IParameterReferenceOperation aliasedParameterReference &&
                context.ParameterAliases.TryGetValue(aliasedParameterReference.Parameter, out var parameterAlias))
            {
                return new Identifier(parameterAlias);
            }

            if (operation is ILocalReferenceOperation localReference &&
                _compileTimeFrameLocalValues.TryGetValue(localReference.Local, out var compileTimeValue))
            {
                return LowerExpression(compileTimeValue, context);
            }

            if (operation is ILocalReferenceOperation aliasedLocalReference &&
                context.LocalAliases.TryGetValue(aliasedLocalReference.Local, out var localAlias))
            {
                return new Identifier(localAlias);
            }

            var previousContext = _activeExpressionContext;
            _activeExpressionContext = context;
            try
            {
                var node = _walker.Visit(operation, context.Argument)
                    ?? throw Unsupported(operation, "Expression did not produce a JavaScript node.");
                if (node is not Expression expression)
                    throw Unsupported(operation, "Expression did not lower to a JavaScript expression.");

                return expression;
            }
            finally
            {
                _activeExpressionContext = previousContext;
            }
        }

        private Expression? RewriteDirectParameterReference(
            IParameterReferenceOperation operation,
            SenseArgument argument)
        {
            var context = _activeExpressionContext;
            if (context is null)
                return null;

            if (context.Substitutions.TryGetValue(operation.Parameter, out var substituted))
                return _walker.Visit(substituted, argument) as Expression;

            return context.ParameterAliases.TryGetValue(operation.Parameter, out var alias)
                ? new Identifier(alias)
                : null;
        }

        private Expression? RewriteDirectLocalReference(
            ILocalReferenceOperation operation,
            SenseArgument argument)
        {
            var context = _activeExpressionContext;
            if (context is null)
                return null;

            if (context.LocalAliases.TryGetValue(operation.Local, out var alias))
                return new Identifier(alias);

            if (_erasedRenderObjectLocals.Contains(operation.Local))
            {
                throw Unsupported(
                    operation,
                    "RenderFragment descriptor local '" + operation.Local.Name +
                    "' can only be consumed through a resolved RenderFragment member in direct render lowering.");
            }

            return null;
        }

        private Expression? RewriteDirectRenderFragmentParameterReference(
            IPropertyReferenceOperation operation,
            SenseArgument argument)
        {
            if (!_componentSlotNames.TryGetValue(operation.Property.OriginalDefinition, out var slotName))
                return null;

            // Vue owns RenderFragment parameters as slots. Project their C# value surface as
            // a function-or-null value so helper logic such as `content is not null` observes
            // the same presence semantics without recreating a RenderTreeBuilder callback.
            _usesSlots = true;
            return BuildSlotValueExpression(slotName!);
        }

        private bool TryResolveRenderFragmentContentExpression(
            IOperation operation,
            EmitContext context,
            out DirectRenderFragment expression)
        {
            expression = default;
            if (!TryResolveRenderFragmentExpression(operation, context, out var renderFragment))
                return false;

            expression = renderFragment;
            return true;
        }

        private bool TryResolveRenderFragmentExpression(
            IOperation operation,
            EmitContext context,
            out DirectRenderFragment renderFragment)
        {
            renderFragment = default;
            // Razor SG may add transparent conversions around template values. Resolve the
            // underlying slot/property expression before looking up local provenance.
            operation = UnwrapTransparentRazorSgOperation(operation);

            if (operation is IParameterReferenceOperation parameterReference &&
                context.Substitutions.TryGetValue(parameterReference.Parameter, out var substituted))
            {
                return TryResolveRenderFragmentExpression(substituted, context, out renderFragment);
            }

            if (TryGetRenderFragmentBody(operation, out var builder, out var body))
            {
                var lowered = EmitRenderFragmentBodyExpression(
                    builder,
                    body,
                    context,
                    operation,
                    "RenderFragment content");
                renderFragment = new DirectRenderFragment(
                    lowered.RenderExpression,
                    null,
                    lowered.UsesFragment,
                    lowered.UsesStaticVNode);
                return true;
            }

            if (TryGetGenericRenderFragmentBody(operation, out var valueParameter, out builder, out body))
            {
                var parameterName = SanitizeJavaScriptIdentifierPart(valueParameter.Name, "value");
                var lowered = EmitRenderFragmentBodyExpression(
                    builder,
                    body,
                    context with
                    {
                        ParameterAliases = context.ParameterAliases.SetItem(valueParameter, parameterName)
                    },
                    operation,
                    "RenderFragment<T> content");
                renderFragment = new DirectRenderFragment(
                    lowered.RenderExpression,
                    parameterName,
                    lowered.UsesFragment,
                    lowered.UsesStaticVNode);
                return true;
            }

            if (TryResolveRenderFragmentMethodReference(operation, context, out renderFragment))
                return true;

            if (operation is ILocalReferenceOperation localReference)
            {
                if (context.LocalRenderFragments.TryGetValue(localReference.Local, out var localRenderFragment))
                {
                    renderFragment = localRenderFragment;
                    return true;
                }

                return false;
            }

            if (operation is IConditionalOperation conditional &&
                TryResolveRenderFragmentExpression(conditional.WhenTrue, context, out var whenTrue) &&
                conditional.WhenFalse is not null &&
                TryResolveRenderFragmentExpression(conditional.WhenFalse, context, out var whenFalse))
            {
                var condition = LowerExpression(conditional.Condition, context);
                var parameterName = whenTrue.ParameterName ?? whenFalse.ParameterName;
                var whenTrueExpression = parameterName is null
                    ? whenTrue.RenderExpression
                    : InvokeRenderFragment(whenTrue, new Identifier(parameterName));
                var whenFalseExpression = parameterName is null
                    ? whenFalse.RenderExpression
                    : InvokeRenderFragment(whenFalse, new Identifier(parameterName));
                renderFragment = new DirectRenderFragment(
                    new ConditionalExpression(condition, whenTrueExpression, whenFalseExpression),
                    parameterName,
                    whenTrue.UsesFragment || whenFalse.UsesFragment,
                    whenTrue.UsesStaticVNode || whenFalse.UsesStaticVNode,
                    Selection: new ConditionalRenderFragmentSelection(
                        condition,
                        whenTrue,
                        whenFalse),
                    ReturnsVueSlotContent: whenTrue.ReturnsVueSlotContent || whenFalse.ReturnsVueSlotContent);
                return true;
            }

            if (operation.ConstantValue.HasValue && operation.ConstantValue.Value is null)
            {
                renderFragment = new DirectRenderFragment(
                    Null(),
                    AvailabilityCondition: new BooleanLiteral(false, "false"),
                    RenderExpressionWhenAvailable: Null());
                return true;
            }

            if (operation is IInvocationOperation invocation &&
                TryResolveLocalGenericRenderFragmentInvocation(invocation, context, out renderFragment))
            {
                return true;
            }

            if (operation is IInvocationOperation helperInvocation &&
                TryResolveRenderFragmentHelperInvocation(helperInvocation, context, out renderFragment))
            {
                return true;
            }

            if (operation is IInvocationOperation slotInvocation &&
                TryResolveComponentScopedSlotInvocation(slotInvocation, context, out renderFragment))
            {
                return true;
            }

            if (operation is IPropertyReferenceOperation currentComponentProperty &&
                TryResolveCurrentComponentRenderFragmentProperty(
                    currentComponentProperty,
                    context,
                    out renderFragment))
            {
                return true;
            }

            if (TryResolveComponentSlot(operation, out var propertySlotName, out var genericPropertySlot))
            {
                _usesSlots = true;
                if (genericPropertySlot)
                {
                    const string parameterName = "value";
                    renderFragment = new DirectRenderFragment(
                        BuildSlotInvocationExpression(propertySlotName, new Identifier(parameterName)),
                        parameterName,
                        AvailabilityCondition: BuildSlotAvailabilityCondition(propertySlotName),
                        RenderExpressionWhenAvailable: BuildSlotCallExpression(
                            propertySlotName,
                            new Identifier(parameterName)),
                        ReturnsVueSlotContent: true);
                }
                else
                {
                    renderFragment = new DirectRenderFragment(
                        BuildSlotInvocationExpression(propertySlotName),
                        AvailabilityCondition: BuildSlotAvailabilityCondition(propertySlotName),
                        RenderExpressionWhenAvailable: BuildSlotCallExpression(propertySlotName),
                        ReturnsVueSlotContent: true);
                }
                return true;
            }

            if (operation is IPropertyReferenceOperation propertyReference)
            {
                if (propertyReference.Instance is not null &&
                    TryResolveRenderObjectExpression(propertyReference.Instance, context, out var renderObject) &&
                    renderObject.RenderFragments.TryGetValue(
                        propertyReference.Property.OriginalDefinition,
                        out var objectRenderFragment))
                {
                    renderFragment = objectRenderFragment;
                    return true;
                }
            }

            return false;
        }

        private bool TryResolveLocalGenericRenderFragmentInvocation(
            IInvocationOperation invocation,
            EmitContext context,
            out DirectRenderFragment renderFragment)
        {
            renderFragment = default;
            if (invocation.TargetMethod.MethodKind != MethodKind.DelegateInvoke ||
                invocation.Arguments.Length != 1 ||
                invocation.Instance is null)
            {
                return false;
            }

            var instance = invocation.Instance;
            while (instance is IConversionOperation conversion)
                instance = conversion.Operand;

            if (instance is not ILocalReferenceOperation localReference ||
                !IsGenericRenderFragmentType(localReference.Local.Type) ||
                !context.LocalRenderFragments.TryGetValue(localReference.Local, out var localRenderFragment) ||
                localRenderFragment.ParameterName is null ||
                localRenderFragment.AvailabilityCondition is not null &&
                !localRenderFragment.ReturnsVueSlotContent)
            {
                return false;
            }

            // A local RenderFragment<T> is already lowered as a value-parameterized Vue
            // expression. An optional scoped slot has already been projected to a local
            // function-or-null value, so invoke that alias instead of rereading slots.*.
            // This preserves the C# local assignment's evaluation and presence semantics.
            var value = LowerExpression(invocation.Arguments[0].Value, context);
            Expression expression;
            if (localRenderFragment.AvailabilityCondition is not null &&
                localRenderFragment.ReturnsVueSlotContent)
            {
                var localName = context.LocalAliases[localReference.Local];
                var localValue = new Identifier(localName);
                expression = new ConditionalExpression(
                    new NonLogicalBinaryExpression(Operator.StrictInequality, localValue, Null()),
                    Call(localValue, value),
                    Null());
            }
            else
            {
                expression = localRenderFragment.Selection is { } selection
                    ? new ConditionalExpression(
                        selection.Condition,
                        InvokeRenderFragment(selection.WhenTrue, value),
                        InvokeRenderFragment(selection.WhenFalse, value))
                    : InvokeRenderFragment(localRenderFragment, value);
            }
            renderFragment = new DirectRenderFragment(
                expression,
                UsesFragment: localRenderFragment.UsesFragment,
                UsesStaticVNode: localRenderFragment.UsesStaticVNode,
                ReturnsVueSlotContent: localRenderFragment.ReturnsVueSlotContent);
            return true;
        }

        private bool TryResolveComponentScopedSlotInvocation(
            IInvocationOperation invocation,
            EmitContext context,
            out DirectRenderFragment renderFragment)
        {
            renderFragment = default;
            if (invocation.TargetMethod.MethodKind != MethodKind.DelegateInvoke ||
                invocation.Arguments.Length != 1 ||
                invocation.Instance is null ||
                !TryResolveComponentSlot(invocation.Instance, out var slotName, out var genericSlot) ||
                !genericSlot)
            {
                return false;
            }

            // Razor SG represents @Template(value) as a delegate invocation followed by
            // AddContent. A RenderFragment<T> parameter is a Vue scoped slot, so lower the
            // completed invocation directly instead of trying to recreate its builder callback.
            _usesSlots = true;
            renderFragment = new DirectRenderFragment(
                BuildSlotInvocationExpression(slotName, LowerExpression(invocation.Arguments[0].Value, context)),
                ReturnsVueSlotContent: true);
            return true;
        }

        private bool TryResolveRenderFragmentMethodReference(
            IOperation operation,
            EmitContext context,
            out DirectRenderFragment renderFragment)
        {
            renderFragment = default;
            while (operation is IConversionOperation conversion)
                operation = conversion.Operand;
            if (operation is IDelegateCreationOperation delegateCreation)
                operation = delegateCreation.Target;
            if (operation is not IMethodReferenceOperation methodReference)
                return false;

            var method = methodReference.Method;
            if (!method.ReturnsVoid ||
                method.Parameters.Length != 1 ||
                !IsRenderTreeBuilder(method.Parameters[0].Type) ||
                method.DeclaringSyntaxReferences.Length != 1 ||
                !ComponentSymbolPolicy.IsDeclaredOnComponentHierarchy(_componentSymbol, method.ContainingType) ||
                !method.IsStatic && methodReference.Instance is not IInstanceReferenceOperation)
            {
                return false;
            }

            if (!_activeRenderFragmentHelpers.Add(method.OriginalDefinition))
            {
                throw Unsupported(
                    methodReference,
                    "Recursive RenderFragment method group '" + method.Name +
                    "' is not supported by direct render operation lowering.");
            }

            try
            {
                if (method.DeclaringSyntaxReferences[0].GetSyntax() is not MethodDeclarationSyntax declaration)
                    return false;

                var model = _compilation.GetSemanticModel(declaration.SyntaxTree);
                IOperation? methodBody = declaration.Body is not null
                    ? model.GetOperation(declaration.Body)
                    : declaration.ExpressionBody is { Expression: { } expression }
                        ? model.GetOperation(expression)
                        : null;
                if (methodBody is null)
                    return false;
                if (ContainsMethodInvocation(methodBody, method))
                {
                    throw Unsupported(
                        methodReference,
                        "Recursive RenderFragment method group '" + method.Name +
                        "' is not supported by direct render operation lowering.");
                }

                var lowered = EmitRenderFragmentBodyExpression(
                    method.Parameters[0],
                    methodBody,
                    context,
                    methodReference,
                    "RenderFragment method group");
                renderFragment = new DirectRenderFragment(
                    lowered.RenderExpression,
                    UsesFragment: lowered.UsesFragment,
                    UsesStaticVNode: lowered.UsesStaticVNode);
                return true;
            }
            finally
            {
                _activeRenderFragmentHelpers.Remove(method.OriginalDefinition);
            }
        }

        private bool TryResolveCurrentComponentRenderFragmentProperty(
            IPropertyReferenceOperation propertyReference,
            EmitContext context,
            out DirectRenderFragment renderFragment)
        {
            renderFragment = default;
            var property = propertyReference.Property;
            if (!IsAnyRenderFragmentType(property.Type) ||
                property.DeclaringSyntaxReferences.Length != 1 ||
                !SymbolComparer.Equals(
                    property.ContainingType?.OriginalDefinition,
                    _componentSymbol.OriginalDefinition) ||
                !property.IsStatic && propertyReference.Instance is not IInstanceReferenceOperation)
            {
                return false;
            }

            if (!_activeRenderFragmentProperties.Add(property.OriginalDefinition))
            {
                throw Unsupported(
                    propertyReference,
                    "Recursive RenderFragment property '" + property.Name +
                    "' is not supported by direct render operation lowering.");
            }

            try
            {
                if (!TryGetReturnedPropertyValue(property, out var returnedValue))
                    return false;

                return TryResolveRenderFragmentExpression(returnedValue, context, out renderFragment);
            }
            finally
            {
                _activeRenderFragmentProperties.Remove(property.OriginalDefinition);
            }
        }

        private bool TryGetReturnedPropertyValue(IPropertySymbol property, out IOperation returnedValue)
        {
            returnedValue = null!;
            if (property.DeclaringSyntaxReferences[0].GetSyntax() is not PropertyDeclarationSyntax declaration)
                return false;

            var model = _compilation.GetSemanticModel(declaration.SyntaxTree);
            IOperation? operation = declaration.ExpressionBody is { Expression: { } propertyExpression }
                ? model.GetOperation(propertyExpression)
                : declaration.AccessorList?.Accessors
                    .Where(static accessor => accessor.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.GetAccessorDeclaration))
                    .Select(accessor => accessor.ExpressionBody is { Expression: { } getterExpression }
                        ? model.GetOperation(getterExpression)
                        : accessor.Body is not null
                            ? TryGetSingleReturnValue(model.GetOperation(accessor.Body))
                            : null)
                    .SingleOrDefault();
            if (operation is null)
                return false;

            returnedValue = operation;
            return true;
        }

        private bool TryResolveComponentSlot(
            IOperation operation,
            out string slotName,
            out bool generic)
        {
            while (operation is IConversionOperation conversion)
                operation = conversion.Operand;
            if (operation is IDelegateCreationOperation delegateCreation)
                operation = delegateCreation.Target;

            if (operation is IPropertyReferenceOperation propertyReference &&
                _componentSlotNames.TryGetValue(propertyReference.Property.OriginalDefinition, out var resolvedSlotName))
            {
                slotName = resolvedSlotName!;
                generic = IsGenericRenderFragmentType(propertyReference.Property.Type);
                return true;
            }

            slotName = string.Empty;
            generic = false;
            return false;
        }

        private bool TryResolveRenderObjectExpression(
            IOperation operation,
            EmitContext context,
            out DirectRenderObject renderObject)
        {
            renderObject = default!;
            while (operation is IConversionOperation conversion)
                operation = conversion.Operand;

            if (operation is IParameterReferenceOperation parameterReference &&
                context.Substitutions.TryGetValue(parameterReference.Parameter, out var substituted))
            {
                return TryResolveRenderObjectExpression(substituted, context, out renderObject);
            }

            if (operation is ILocalReferenceOperation localReference)
            {
                if (context.LocalRenderObjects.TryGetValue(localReference.Local, out var localRenderObject))
                {
                    renderObject = localRenderObject;
                    return true;
                }

                return false;
            }

            if (operation is IInvocationOperation invocation &&
                TryResolveRenderObjectHelperInvocation(invocation, context, out renderObject))
            {
                return true;
            }

            if (operation is IObjectCreationOperation creation)
                return TryResolveObjectCreationRenderFragments(creation, context, out renderObject);

            return false;
        }

        private bool TryResolveRenderObjectHelperInvocation(
            IInvocationOperation invocation,
            EmitContext context,
            out DirectRenderObject renderObject)
        {
            renderObject = default!;
            var method = invocation.TargetMethod;
            if (method.ReturnsVoid ||
                method.DeclaringSyntaxReferences.Length != 1 ||
                !ComponentSymbolPolicy.IsDeclaredOnComponentHierarchy(_componentSymbol, method.ContainingType))
            {
                return false;
            }

            if (!_activeRenderObjectHelpers.Add(method.OriginalDefinition))
                throw Unsupported(invocation, "Recursive render-state helper '" + method.Name + "' is not supported by direct render operation lowering yet.");

            try
            {
                var syntax = method.DeclaringSyntaxReferences[0].GetSyntax();
                if (syntax is not MethodDeclarationSyntax methodDeclaration)
                    return false;

                var model = _compilation.GetSemanticModel(methodDeclaration.SyntaxTree);
                IOperation? bodyOperation = methodDeclaration.ExpressionBody is { Expression: { } expression }
                    ? model.GetOperation(expression)
                    : methodDeclaration.Body is not null
                        ? model.GetOperation(methodDeclaration.Body)
                        : null;

                if (bodyOperation is null)
                    return false;

                var substitutions = context.Substitutions.ToBuilder();
                for (var index = 0; index < invocation.Arguments.Length && index < method.Parameters.Length; index++)
                    AddParameterSubstitution(substitutions, method, index, invocation.Arguments[index].Value);

                var helperContext = new EmitContext(
                    context.Builder,
                    substitutions.ToImmutable(),
                    context.ParameterAliases,
                    context.LocalAliases,
                    context.LocalRenderFragments,
                    context.LocalRenderObjects,
                    context.LocalComponentTypes,
                    context.SecondaryBuilders,
                    context.PreludeStatements,
                    AllowPreludeDeclarations: false,
                    Argument: context.Argument);

                return TryResolveReturnedRenderObject(bodyOperation, helperContext, out renderObject);
            }
            finally
            {
                _activeRenderObjectHelpers.Remove(method.OriginalDefinition);
            }
        }

        private bool TryResolveReturnedRenderObject(
            IOperation operation,
            EmitContext context,
            out DirectRenderObject renderObject)
        {
            renderObject = default!;
            while (operation is IConversionOperation conversion)
                operation = conversion.Operand;

            if (operation is IBlockOperation block)
            {
                var helperContext = context;
                foreach (var child in block.Operations)
                {
                    if (child is IVariableDeclarationGroupOperation declarationGroup)
                    {
                        helperContext = TrackRenderProvenanceDeclarationGroup(declarationGroup, helperContext);
                        continue;
                    }

                    if (child is IReturnOperation returnOperation &&
                        returnOperation.ReturnedValue is not null)
                    {
                        return TryResolveRenderObjectExpression(returnOperation.ReturnedValue, helperContext, out renderObject);
                    }

                    if (IsNoOutputOperation(child))
                        continue;

                    return false;
                }

                return false;
            }

            return TryResolveRenderObjectExpression(operation, context, out renderObject);
        }

        private EmitContext TrackRenderProvenanceDeclarationGroup(
            IVariableDeclarationGroupOperation declarationGroup,
            EmitContext context)
        {
            var localRenderFragments = context.LocalRenderFragments;
            var localRenderObjects = context.LocalRenderObjects;
            foreach (var declaration in declarationGroup.Declarations)
            {
                foreach (var declarator in declaration.Declarators)
                {
                    if (declarator.Initializer is null)
                        continue;

                    if (TryResolveRenderFragmentExpression(declarator.Initializer.Value, context, out var renderFragment))
                        localRenderFragments = localRenderFragments.SetItem(declarator.Symbol, renderFragment);
                    if (TryResolveRenderObjectExpression(declarator.Initializer.Value, context, out var renderObject))
                        localRenderObjects = localRenderObjects.SetItem(declarator.Symbol, renderObject);
                }
            }

            return context with
            {
                LocalRenderFragments = localRenderFragments,
                LocalRenderObjects = localRenderObjects
            };
        }

        private bool TryResolveObjectCreationRenderFragments(
            IObjectCreationOperation creation,
            EmitContext context,
            out DirectRenderObject renderObject)
        {
            renderObject = default!;
            var builder = ImmutableDictionary.CreateBuilder<IPropertySymbol, DirectRenderFragment>(SymbolComparer);

            if (creation.Constructor is not null &&
                TryBuildConstructorRenderFragmentPropertyMap(creation.Constructor, out var constructorMap))
            {
                foreach (var argument in creation.Arguments)
                {
                    if (argument.Parameter is null ||
                        !constructorMap.TryGetValue(argument.Parameter.OriginalDefinition, out var property) ||
                        !TryResolveRenderFragmentExpression(argument.Value, context, out var renderFragment))
                    {
                        continue;
                    }

                    builder[property] = renderFragment;
                }
            }

            if (creation.Initializer is not null)
            {
                foreach (var initializer in creation.Initializer.Initializers)
                {
                    if (initializer is ISimpleAssignmentOperation assignment &&
                        assignment.Target is IPropertyReferenceOperation propertyReference &&
                        IsAnyRenderFragmentType(propertyReference.Property.Type) &&
                        TryResolveRenderFragmentExpression(assignment.Value, context, out var renderFragment))
                    {
                        builder[propertyReference.Property.OriginalDefinition] = renderFragment;
                    }
                }
            }

            if (builder.Count == 0)
                return false;

            renderObject = new DirectRenderObject(builder.ToImmutable());
            return true;
        }

        private bool TryBuildConstructorRenderFragmentPropertyMap(
            IMethodSymbol constructor,
            out ImmutableDictionary<IParameterSymbol, IPropertySymbol> map)
        {
            map = ImmutableDictionary<IParameterSymbol, IPropertySymbol>.Empty.WithComparers(SymbolComparer);
            if (constructor.DeclaringSyntaxReferences.Length != 1)
                return false;

            var syntax = constructor.DeclaringSyntaxReferences[0].GetSyntax();
            if (syntax is not ConstructorDeclarationSyntax { Body: not null } constructorDeclaration)
                return false;

            var model = _compilation.GetSemanticModel(constructorDeclaration.SyntaxTree);
            if (model.GetOperation(constructorDeclaration.Body) is not IBlockOperation body)
                return false;

            var builder = ImmutableDictionary.CreateBuilder<IParameterSymbol, IPropertySymbol>(SymbolComparer);
            foreach (var operation in body.Operations)
            {
                if (operation is not IExpressionStatementOperation
                    {
                        Operation: ISimpleAssignmentOperation
                        {
                            Target: IPropertyReferenceOperation propertyReference,
                            Value: IParameterReferenceOperation parameterReference
                        }
                    } ||
                    propertyReference.Instance is not IInstanceReferenceOperation ||
                    !IsAnyRenderFragmentType(propertyReference.Property.Type))
                {
                    continue;
                }

                builder[parameterReference.Parameter.OriginalDefinition] = propertyReference.Property.OriginalDefinition;
            }

            if (builder.Count == 0)
                return false;

            map = builder.ToImmutable();
            return true;
        }

        private bool TryResolveRenderFragmentHelperInvocation(
            IInvocationOperation invocation,
            EmitContext context,
            out DirectRenderFragment renderFragment)
        {
            renderFragment = default;
            var method = invocation.TargetMethod;
            if (!IsAnyRenderFragmentType(method.ReturnType) ||
                method.DeclaringSyntaxReferences.Length != 1 ||
                !ComponentSymbolPolicy.IsDeclaredOnComponentHierarchy(_componentSymbol, method.ContainingType))
            {
                return false;
            }

            if (!TryGetReturnedRenderFragmentBody(method, out var helperBody))
                return false;

            if (_activeRenderFragmentHelpers.Contains(method.OriginalDefinition) ||
                ContainsMethodInvocation(helperBody.Body, method))
            {
                var helper = EnsureRenderFragmentHelperFunction(method, helperBody, context);
                var arguments = invocation.Arguments
                    .Select(argument => LowerExpression(argument.Value, context))
                    .ToList();
                string? parameterName = null;
                if (helperBody.ValueParameter is not null)
                {
                    parameterName = SanitizeJavaScriptIdentifierPart(helperBody.ValueParameter.Name, "value");
                    arguments.Add(new Identifier(parameterName));
                }
                renderFragment = new DirectRenderFragment(
                    Call(new Identifier(helper.FunctionName), arguments.ToArray()),
                    ParameterName: parameterName,
                    UsesFragment: helper.UsesFragment,
                    UsesStaticVNode: helper.UsesStaticVNode);
                return true;
            }

            // Membership was checked immediately above; recursive re-entry is handled by
            // the hoisted-helper path before reaching this registration.
            _activeRenderFragmentHelpers.Add(method.OriginalDefinition);

            var substitutions = context.Substitutions.ToBuilder();
            for (var index = 0; index < invocation.Arguments.Length && index < method.Parameters.Length; index++)
                AddParameterSubstitution(substitutions, method, index, invocation.Arguments[index].Value);

            try
            {
                renderFragment = WithScopedLocalNames(() =>
                {
                    var factoryContext = context with { Substitutions = substitutions.ToImmutable() };
                    foreach (var declarationGroup in helperBody.LocalRenderFragmentDeclarations)
                        factoryContext = TrackRenderProvenanceDeclarationGroup(declarationGroup, factoryContext);

                    var parameterAliases = factoryContext.ParameterAliases;
                    string? parameterName = null;
                    if (helperBody.ValueParameter is not null)
                    {
                        parameterName = SanitizeJavaScriptIdentifierPart(helperBody.ValueParameter.Name, "value");
                        parameterAliases = parameterAliases.SetItem(helperBody.ValueParameter, parameterName);
                    }

                    var fragmentState = new RenderState();
                    var preludeStatements = new List<Statement>();
                    var fragmentArgument = context.Argument.WithNewScope();
                    _ = EmitOperation(
                        helperBody.Body,
                        new EmitContext(
                            BuilderBinding.ForSymbol(helperBody.Builder),
                            factoryContext.Substitutions,
                            parameterAliases,
                            factoryContext.LocalAliases,
                            factoryContext.LocalRenderFragments,
                            factoryContext.LocalRenderObjects,
                            factoryContext.LocalComponentTypes,
                            factoryContext.SecondaryBuilders,
                            preludeStatements,
                            AllowPreludeDeclarations: true,
                            Argument: fragmentArgument),
                        fragmentState);
                    if (fragmentState.Stack.Count != 0)
                        throw Unsupported(invocation, "RenderFragment helper '" + method.Name + "' left unclosed " + fragmentState.Stack.Peek().Describe() + " frames.");

                    return new DirectRenderFragment(
                        WrapWithExpressionScope(fragmentArgument, preludeStatements, fragmentState.ToRenderExpression()),
                        ParameterName: parameterName,
                        UsesFragment: fragmentState.UsesFragment || fragmentState.Roots.Count > 1,
                        UsesStaticVNode: fragmentState.UsesStaticVNode);
                });
                return true;
            }
            finally
            {
                _activeRenderFragmentHelpers.Remove(method.OriginalDefinition);
            }
        }

        private DirectRenderFunction EnsureRenderFragmentHelperFunction(
            IMethodSymbol method,
            RenderFragmentHelperBody helperBody,
            EmitContext context)
        {
            var originalMethod = method.OriginalDefinition;
            if (_renderFragmentHelperFunctionNames.TryGetValue(originalMethod, out var existingName))
                return new DirectRenderFunction(existingName, UsesFragment: false, UsesStaticVNode: false);

            var functionName = CreateRenderFragmentHelperFunctionName(method);
            _renderFragmentHelperFunctionNames.Add(originalMethod, functionName);

            if (!_emittingRenderFragmentHelperFunctions.Add(originalMethod))
                return new DirectRenderFunction(functionName, UsesFragment: false, UsesStaticVNode: false);

            try
            {
                var parameterAliases = context.ParameterAliases.ToBuilder();
                var parameterNames = new List<string>(method.Parameters.Length);
                foreach (var parameter in method.Parameters)
                {
                    var parameterName = CreateUniqueLocalName(parameter.Name);
                    parameterAliases[parameter] = parameterName;
                    parameterNames.Add(parameterName);
                }
                if (helperBody.ValueParameter is not null)
                {
                    var valueParameterName = CreateUniqueLocalName(helperBody.ValueParameter.Name);
                    parameterAliases[helperBody.ValueParameter] = valueParameterName;
                    parameterNames.Add(valueParameterName);
                }

                var lowered = WithScopedLocalNames(() =>
                {
                    var functionState = new RenderState();
                    var preludeStatements = new List<Statement>();
                    var functionArgument = context.Argument.WithNewScope();
                    // A hoisted helper only sees its own formal parameters and component
                    // members. Call-site substitutions are tied to a different lexical
                    // scope and would make recursive calls capture their initial values.
                    var factoryContext = context with
                    {
                        Substitutions = ImmutableDictionary<IParameterSymbol, IOperation>.Empty.WithComparers(SymbolComparer),
                        ParameterAliases = parameterAliases.ToImmutable(),
                        LocalAliases = ImmutableDictionary<ILocalSymbol, string>.Empty.WithComparers(SymbolComparer)
                    };
                    foreach (var declarationGroup in helperBody.LocalRenderFragmentDeclarations)
                        factoryContext = TrackRenderProvenanceDeclarationGroup(declarationGroup, factoryContext);
                    _ = EmitOperation(
                        helperBody.Body,
                        new EmitContext(
                            BuilderBinding.ForSymbol(helperBody.Builder),
                            factoryContext.Substitutions,
                            factoryContext.ParameterAliases,
                            factoryContext.LocalAliases,
                            factoryContext.LocalRenderFragments,
                            factoryContext.LocalRenderObjects,
                            factoryContext.LocalComponentTypes,
                            factoryContext.SecondaryBuilders,
                            preludeStatements,
                            AllowPreludeDeclarations: true,
                            Argument: functionArgument),
                        functionState);
                    if (functionState.Stack.Count != 0)
                        throw Unsupported(helperBody.Body, "RenderFragment helper '" + method.Name + "' left unclosed " + functionState.Stack.Peek().Describe() + " frames.");

                    return new DirectRenderFragmentBody(
                        WrapWithExpressionScope(functionArgument, preludeStatements, functionState.ToRenderExpression()),
                        functionState.UsesFragment || functionState.Roots.Count > 1,
                        functionState.UsesStaticVNode);
                });

                _usesFragment = _usesFragment || lowered.UsesFragment;
                _usesStaticVNode = _usesStaticVNode || lowered.UsesStaticVNode;
                var functionBody = new FunctionBody(
                    NodeList.From<Statement>(new ReturnStatement(lowered.RenderExpression)),
                    strict: true);
                _preludeStatements.Add(new FunctionDeclaration(
                    new Identifier(functionName),
                    NodeList.From<Node>(parameterNames.Select(static name => (Node)new Identifier(name))),
                    functionBody,
                    generator: false,
                    async: false));
                return new DirectRenderFunction(functionName, lowered.UsesFragment, lowered.UsesStaticVNode);
            }
            finally
            {
                _emittingRenderFragmentHelperFunctions.Remove(originalMethod);
            }
        }

        private static void AddParameterSubstitution(
            ImmutableDictionary<IParameterSymbol, IOperation>.Builder substitutions,
            IMethodSymbol method,
            int index,
            IOperation value)
        {
            substitutions[method.Parameters[index]] = value;
            substitutions[method.OriginalDefinition.Parameters[index]] = value;
        }

        private static bool ContainsMethodInvocation(IOperation operation, IMethodSymbol method)
        {
            if (operation is IInvocationOperation invocation &&
                SymbolComparer.Equals(invocation.TargetMethod.OriginalDefinition, method.OriginalDefinition))
            {
                return true;
            }

            foreach (var child in operation.ChildOperations)
            {
                if (ContainsMethodInvocation(child, method))
                    return true;
            }

            return false;
        }

        private bool TryGetReturnedRenderFragmentBody(
            IMethodSymbol method,
            out RenderFragmentHelperBody helperBody)
        {
            helperBody = default;
            var syntax = method.DeclaringSyntaxReferences[0].GetSyntax();
            var model = _compilation.GetSemanticModel(syntax.SyntaxTree);
            IOperation? returnedOperation;
            var localRenderFragmentDeclarations = ImmutableArray<IVariableDeclarationGroupOperation>.Empty;
            switch (syntax)
            {
                case MethodDeclarationSyntax { ExpressionBody.Expression: { } expression }:
                    returnedOperation = model.GetOperation(expression);
                    break;
                case MethodDeclarationSyntax { Body: { } methodBody }:
                    if (!TryGetRenderFragmentFactoryReturn(
                            model.GetOperation(methodBody),
                            out returnedOperation,
                            out localRenderFragmentDeclarations))
                    {
                        return false;
                    }

                    break;
                default:
                    return false;
            }

            if (returnedOperation is null)
                return false;
            if (TryGetRenderFragmentBody(returnedOperation, out var builder, out var body))
            {
                helperBody = new RenderFragmentHelperBody(
                    ValueParameter: null,
                    builder,
                    body,
                    localRenderFragmentDeclarations);
                return true;
            }

            if (!TryGetGenericRenderFragmentBody(returnedOperation, out var valueParameter, out builder, out body))
                return false;

            helperBody = new RenderFragmentHelperBody(
                valueParameter,
                builder,
                body,
                localRenderFragmentDeclarations);
            return true;
        }

        private static bool TryGetRenderFragmentFactoryReturn(
            IOperation? operation,
            out IOperation? returnedOperation,
            out ImmutableArray<IVariableDeclarationGroupOperation> localRenderFragmentDeclarations)
        {
            returnedOperation = null;
            localRenderFragmentDeclarations = ImmutableArray<IVariableDeclarationGroupOperation>.Empty;
            if (operation is not IBlockOperation block)
                return false;

            var declarations = ImmutableArray.CreateBuilder<IVariableDeclarationGroupOperation>();
            foreach (var child in block.Operations)
            {
                if (child is IVariableDeclarationGroupOperation declarationGroup)
                {
                    foreach (var declarator in declarationGroup.Declarations.SelectMany(static declaration => declaration.Declarators))
                    {
                        if (declarator.Initializer is null ||
                            !IsAnyRenderFragmentType(declarator.Symbol.Type))
                        {
                            return false;
                        }
                    }

                    declarations.Add(declarationGroup);
                    continue;
                }

                if (child is IReturnOperation { ReturnedValue: not null } returnOperation &&
                    returnedOperation is null)
                {
                    returnedOperation = returnOperation.ReturnedValue;
                    continue;
                }

                return false;
            }

            if (returnedOperation is null)
                return false;

            localRenderFragmentDeclarations = declarations.ToImmutable();
            return true;
        }

        private static IOperation? TryGetSingleReturnValue(IOperation? operation)
        {
            if (operation is not IBlockOperation block)
                return null;

            IOperation? returnedValue = null;
            foreach (var child in block.Operations)
            {
                if (child is not IReturnOperation returnOperation)
                    return null;
                if (returnOperation.ReturnedValue is null || returnedValue is not null)
                    return null;

                returnedValue = returnOperation.ReturnedValue;
            }

            return returnedValue;
        }

        private Expression BindComponentImport(INamedTypeSymbol componentType)
        {
            var runtimeComponentType = _injectRegistry.ResolveImplementation(componentType);
            var descriptor = ResolveComponentImport(runtimeComponentType);
            return _argument
                .BindImportSpecifier(descriptor.ImportSpecifier, descriptor.ExportName);
        }

        private ImmutableArray<ImportDeclaration> BuildImportDeclarations()
        {
            var groupedSpecifiers = new Dictionary<string, List<ImportDeclarationSpecifier>>(StringComparer.Ordinal);
            if (_usesMergeProps)
                groupedSpecifiers["vue"] = [new ImportSpecifier(new Identifier("mergeProps"))];

            foreach (var pair in _argument.FlushImportSpecifiers())
            {
                if (!groupedSpecifiers.TryGetValue(pair.Key, out var specifiers))
                {
                    specifiers = new List<ImportDeclarationSpecifier>();
                    groupedSpecifiers.Add(pair.Key, specifiers);
                }
                specifiers.AddRange(pair.Value);
            }

            var declarations = ImmutableArray.CreateBuilder<ImportDeclaration>();
            foreach (var pair in groupedSpecifiers.OrderBy(static pair => pair.Key, StringComparer.Ordinal))
                declarations.AddRange(ImportDeclarationFactory.Create(pair.Key, pair.Value));
            return declarations.ToImmutable();
        }

        private string CreateUniqueLocalName(string name)
        {
            var baseName = SanitizeJavaScriptIdentifierPart(name, "local");
            if (!_localNameCounts.TryGetValue(baseName, out var count))
            {
                _localNameCounts.Add(baseName, 1);
                return baseName;
            }

            _localNameCounts[baseName] = count + 1;
            return baseName + "$" + count.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        private string CreateRenderFragmentHelperFunctionName(IMethodSymbol method)
        {
            var baseName = "render" + SanitizeJavaScriptIdentifierPart(method.Name, "Fragment");
            if (!_localNameCounts.TryGetValue(baseName, out var count))
            {
                _localNameCounts.Add(baseName, 1);
                return baseName;
            }

            _localNameCounts[baseName] = count + 1;
            return baseName + "$" + count.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        private T WithScopedLocalNames<T>(Func<T> action)
        {
            var snapshot = new Dictionary<string, int>(_localNameCounts, StringComparer.Ordinal);
            var renderLocalSnapshot = new HashSet<string>(_renderLocalNames, StringComparer.Ordinal);
            try
            {
                return action();
            }
            finally
            {
                _localNameCounts.Clear();
                foreach (var pair in snapshot)
                    _localNameCounts.Add(pair.Key, pair.Value);

                _renderLocalNames.Clear();
                foreach (var name in renderLocalSnapshot)
                    _renderLocalNames.Add(name);
            }
        }

        private Expression WrapWithExpressionScope(
            SenseArgument argument,
            List<Statement> preludeStatements,
            Expression expression)
        {
            PruneUnreferencedRenderFragmentDeclarations(preludeStatements, expression);
            if (argument.HasVarDeclarator)
            {
                preludeStatements.Insert(0, new VariableDeclaration(
                        VariableDeclarationKind.Let,
                        argument.FlushVarDeclarator()));
            }

            return WrapWithStatements(preludeStatements, expression);
        }

        private void PruneUnreferencedRenderFragmentDeclarations(
            List<Statement> preludeStatements,
            Expression expression)
        {
            var candidates = preludeStatements
                .OfType<VariableDeclaration>()
                .Where(IsTrackedRenderFragmentDeclaration)
                .Select(static declaration =>
                {
                    if (declaration.Declarations.Count != 1 ||
                        declaration.Declarations[0].Id is not Identifier identifier)
                    {
                        throw new InvalidOperationException(
                            "Tracked RenderFragment declarations must contain one identifier binding.");
                    }

                    return new RenderFragmentDeclarationCandidate(
                        declaration,
                        identifier.Name,
                        declaration.Declarations[0].Init);
                })
                .ToArray();
            if (candidates.Length == 0)
                return;

            var roots = new List<Node> { expression };
            roots.AddRange(preludeStatements.Where(statement =>
                !candidates.Any(candidate => ReferenceEquals(candidate.Declaration, statement))));
            var referencedNames = AstReferenceAnalysis.CollectIdentifiers(roots);
            var liveDeclarations = new HashSet<VariableDeclaration>();
            var changed = true;
            while (changed)
            {
                changed = false;
                foreach (var candidate in candidates)
                {
                    if (!referencedNames.Contains(candidate.Name) ||
                        !liveDeclarations.Add(candidate.Declaration))
                    {
                        continue;
                    }

                    changed = true;
                    if (candidate.Initializer is not null)
                    {
                        referencedNames.UnionWith(
                            AstReferenceAnalysis.CollectIdentifiers([candidate.Initializer]));
                    }
                }
            }

            preludeStatements.RemoveAll(statement =>
                candidates.Any(candidate =>
                    ReferenceEquals(candidate.Declaration, statement) &&
                    !liveDeclarations.Contains(candidate.Declaration)));
        }

        private bool IsTrackedRenderFragmentDeclaration(VariableDeclaration declaration)
            => _renderFragmentPreludeDeclarations.Any(candidate => ReferenceEquals(candidate, declaration));

        private ImmutableArray<RenderModuleHoist> PruneUnreferencedModuleHoists(
            Expression renderExpression,
            IReadOnlyList<Statement> preludeStatements)
        {
            if (_moduleHoists.Count == 0)
                return [];

            // `Clear()` and conditional template paths can discard a previously created static
            // node. Module hoists have no observable initializer side effects, so retain only
            // entries reachable from the final render/prelude roots, then follow hoist-to-hoist
            // references until stable.
            // 静态 hoist 只有最终 render 可达时才输出，避免 Clear 后仍携带死常量。
            var referencedNames = AstReferenceAnalysis.CollectIdentifiers(
                [renderExpression, .. preludeStatements]);
            var retained = new List<RenderModuleHoist>(_moduleHoists.Count);
            var retainedNames = new HashSet<string>(StringComparer.Ordinal);
            var changed = true;
            while (changed)
            {
                changed = false;
                foreach (var hoist in _moduleHoists)
                {
                    if (!referencedNames.Contains(hoist.Name) || !retainedNames.Add(hoist.Name))
                        continue;

                    retained.Add(hoist);
                    referencedNames.UnionWith(AstReferenceAnalysis.CollectIdentifiers([hoist.Initializer]));
                    changed = true;
                }
            }

            return retained.ToImmutableArray();
        }

        /// <summary>Captures a local RenderFragment declaration until liveness determines whether it is emitted.</summary>
        private sealed record RenderFragmentDeclarationCandidate(
            VariableDeclaration Declaration,
            string Name,
            Expression? Initializer);
    }

    private static Expression WrapWithStatements(IReadOnlyList<Statement> statements, Expression expression)
    {
        if (statements.Count == 0)
            return expression;

        var bodyStatements = new List<Statement>(statements.Count + 1);
        bodyStatements.AddRange(statements);
        bodyStatements.Add(new ReturnStatement(expression));
        var body = new FunctionBody(NodeList.From(bodyStatements), strict: true);
        var arrow = new ArrowFunctionExpression(
            NodeList.From<Node>(),
            body,
            expression: false,
            async: false);
        return new CallExpression(arrow, NodeList.From<Expression>(), optional: false);
    }

    private static bool TryGetRenderFragmentBody(
        IOperation operation,
        out IParameterSymbol builder,
        out IOperation body)
    {
        builder = null!;
        body = null!;
        while (operation is IConversionOperation conversion)
            operation = conversion.Operand;

        if (operation is IDelegateCreationOperation delegateCreation)
            operation = delegateCreation.Target;

        if (operation is not IAnonymousFunctionOperation lambda ||
            lambda.Symbol.Parameters.Length != 1 ||
            !IsRenderTreeBuilder(lambda.Symbol.Parameters[0].Type))
        {
            return false;
        }

        builder = lambda.Symbol.Parameters[0];
        body = lambda.Body;
        return true;
    }

    private static bool TryGetGenericRenderFragmentBody(
        IOperation operation,
        out IParameterSymbol valueParameter,
        out IParameterSymbol builder,
        out IOperation body)
    {
        valueParameter = null!;
        builder = null!;
        body = null!;
        while (operation is IConversionOperation conversion)
            operation = conversion.Operand;

        if (operation is IDelegateCreationOperation delegateCreation)
            operation = delegateCreation.Target;

        if (operation is not IAnonymousFunctionOperation lambda ||
            lambda.Symbol.Parameters.Length != 1 ||
            IsRenderTreeBuilder(lambda.Symbol.Parameters[0].Type))
        {
            return false;
        }

        var returnedOperation = TryGetSingleReturnedValue(lambda.Body);
        if (returnedOperation is null ||
            !TryGetRenderFragmentBody(returnedOperation, out builder, out body))
        {
            return false;
        }

        valueParameter = lambda.Symbol.Parameters[0];
        return true;
    }

    private static IOperation? TryGetSingleReturnedValue(IOperation operation)
        => operation is IBlockOperation
           {
               Operations.Length: 1
           } block &&
           block.Operations[0] is IReturnOperation { ReturnedValue: not null } returnOperation
            ? returnOperation.ReturnedValue
            : null;

    private static bool IsGenericRenderFragmentOperationValue(IOperation operation)
    {
        while (operation is IConversionOperation conversion)
            operation = conversion.Operand;

        if (operation is IDelegateCreationOperation delegateCreation)
            operation = delegateCreation.Target;

        return operation.Type is INamedTypeSymbol named &&
               named.OriginalDefinition.TypeParameters.Length == 1 &&
               string.Equals(named.Name, "RenderFragment", StringComparison.Ordinal) &&
               string.Equals(
                   named.ContainingNamespace?.ToDisplayString(),
                   "Microsoft.AspNetCore.Components",
                   StringComparison.Ordinal);
    }

    private static bool IsRenderFragmentOperationValue(IOperation operation)
    {
        while (operation is IConversionOperation conversion)
            operation = conversion.Operand;

        if (operation is IDelegateCreationOperation delegateCreation)
            operation = delegateCreation.Target;

        return operation.Type is INamedTypeSymbol named &&
               !named.IsGenericType &&
               string.Equals(named.OriginalDefinition.ToDisplayString(), "Microsoft.AspNetCore.Components.RenderFragment", StringComparison.Ordinal);
    }

    private static bool IsGenericRenderFragmentType(ITypeSymbol? type)
        => type is INamedTypeSymbol named &&
           named.OriginalDefinition.TypeParameters.Length == 1 &&
           string.Equals(named.Name, "RenderFragment", StringComparison.Ordinal) &&
           string.Equals(
               named.ContainingNamespace?.ToDisplayString(),
               "Microsoft.AspNetCore.Components",
               StringComparison.Ordinal);

        private static bool IsMarkupStringOperationValue(IOperation operation)
        {
            operation = UnwrapMarkupStringOperation(operation);

            var type = operation.Type;
            return IsMarkupString(type) || IsNullableMarkupString(type);
        }

        private static bool IsMarkupString(ITypeSymbol? type)
            => type is not null &&
               string.Equals(
                   type.OriginalDefinition.ToDisplayString(Format.NameFormat),
                   MarkupStringMetadataName,
                   StringComparison.Ordinal);

        private static bool IsNullableMarkupString(ITypeSymbol? type)
            => type is INamedTypeSymbol nullable &&
               nullable.IsGenericType &&
               nullable.TypeArguments.Length == 1 &&
               nullable.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T &&
               IsMarkupString(nullable.TypeArguments[0]);

        private static bool IsNullableMarkupStringOperationValue(IOperation operation)
        {
            operation = UnwrapMarkupStringOperation(operation);
            return IsNullableMarkupString(operation.Type);
        }

        private static bool TryGetStaticMarkupText(
            IOperation operation,
            bool allowRawStringLiteral,
            out string markup)
        {
            operation = UnwrapMarkupStringOperation(operation);
            if (allowRawStringLiteral && TryGetConstantString(operation, out markup))
                return true;

            if (operation is IObjectCreationOperation creation &&
                IsMarkupString(creation.Type) &&
                creation.Arguments.Length == 1 &&
                TryGetConstantString(creation.Arguments[0].Value, out markup))
            {
                return true;
            }

            if (TryGetConstantString(operation, out markup) && IsMarkupString(operation.Type))
                return true;

            markup = string.Empty;
            return false;
        }

        private static IOperation UnwrapMarkupStringOperation(IOperation operation)
        {
            while (operation is IConversionOperation conversion)
                operation = conversion.Operand;

            return operation;
        }

        private static SourceOrigin CreateDirectRenderSourceOrigin(IOperation operation)
        {
            var lineSpan = operation.Syntax.GetLocation().GetMappedLineSpan();
            var sourcePath = !string.IsNullOrWhiteSpace(lineSpan.Path)
                ? lineSpan.Path
                : operation.Syntax.SyntaxTree.FilePath;
            return new SourceOrigin(
                sourcePath,
                lineSpan.StartLinePosition.Line,
                lineSpan.StartLinePosition.Character,
                lineSpan.EndLinePosition.Line,
                lineSpan.EndLinePosition.Character);
        }

    private static bool IsTerminatingWithoutOutput(IOperation? operation)
    {
        if (operation is null)
            return false;

        if (operation is IReturnOperation returnOperation)
            return returnOperation.ReturnedValue is null;

        return operation is IBlockOperation block &&
               block.Operations.Length == 1 &&
               IsTerminatingWithoutOutput(block.Operations[0]);
    }

    private static bool IsTerminatingOperation(IOperation? operation)
    {
        if (operation is null)
            return false;

        if (operation is IReturnOperation returnOperation)
            return returnOperation.ReturnedValue is null;

        if (operation is IBlockOperation block)
            return block.Operations.Length > 0 &&
                   IsTerminatingOperation(block.Operations[block.Operations.Length - 1]);

        return operation is IConditionalOperation conditional &&
               IsTerminatingOperation(conditional.WhenTrue) &&
               IsTerminatingOperation(conditional.WhenFalse);
    }

    private static bool IsNoOutputOperation(IOperation? operation)
    {
        if (operation is null)
            return true;

        return operation is IBlockOperation block && block.Operations.Length == 0;
    }

    private static bool TryGetAttributeInvocations(
        IOperation? operation,
        out ImmutableArray<IInvocationOperation> invocations)
    {
        if (operation is null)
        {
            invocations = ImmutableArray<IInvocationOperation>.Empty;
            return true;
        }

        var builder = ImmutableArray.CreateBuilder<IInvocationOperation>();
        if (operation is IBlockOperation block)
        {
            foreach (var child in block.Operations)
            {
                if (!TryGetAttributeInvocation(child, out var invocation))
                {
                    invocations = default;
                    return false;
                }

                builder.Add(invocation);
            }

            invocations = builder.ToImmutable();
            return true;
        }

        if (!TryGetAttributeInvocation(operation, out var singleInvocation))
        {
            invocations = default;
            return false;
        }

        builder.Add(singleInvocation);
        invocations = builder.ToImmutable();
        return true;
    }

    private static bool TryGetAttributeInvocation(
        IOperation operation,
        out IInvocationOperation invocation)
    {
        invocation = null!;
        if (operation is IExpressionStatementOperation statement)
            operation = statement.Operation;

        while (operation is IConversionOperation conversion)
            operation = conversion.Operand;

        if (operation is not IInvocationOperation candidate || !IsRenderTreeBuilderMethod(candidate.TargetMethod))
            return false;

        var methodName = candidate.TargetMethod.OriginalDefinition.Name;
        if (!string.Equals(methodName, "AddAttribute", StringComparison.Ordinal) &&
            !string.Equals(methodName, "AddComponentParameter", StringComparison.Ordinal))
            return false;

        invocation = candidate;
        return true;
    }

    private static INamedTypeSymbol ResolveOpenComponentType(
        IInvocationOperation invocation,
        EmitContext context)
    {
        if (invocation.TargetMethod.TypeArguments.Length == 1 &&
            invocation.TargetMethod.TypeArguments[0] is INamedTypeSymbol genericComponentType)
        {
            return genericComponentType;
        }

        if (invocation.Arguments.Length == 2 &&
            TryResolveTypeOfExpression(
                invocation.Arguments[1].Value,
                context.LocalComponentTypes,
                out var componentType))
        {
            return componentType;
        }

        throw Unsupported(invocation, "OpenComponent must use a generic component type or typeof(T) for direct render lowering.");
    }

    private static bool TryResolveTypeOfExpression(
        IOperation operation,
        IReadOnlyDictionary<ILocalSymbol, INamedTypeSymbol> localComponentTypes,
        out INamedTypeSymbol componentType)
    {
        componentType = null!;
        while (operation is IConversionOperation conversion)
            operation = conversion.Operand;

        if (operation is ITypeOfOperation { TypeOperand: INamedTypeSymbol namedType })
        {
            componentType = namedType;
            return true;
        }

        if (operation is ILocalReferenceOperation localReference &&
            localComponentTypes.TryGetValue(localReference.Local, out componentType))
        {
            return true;
        }

        return false;
    }

    private static bool IsSecondaryBuilderInvocation(
        IInvocationOperation invocation,
        EmitContext context)
    {
        IOperation? receiver = invocation.Instance;
        if (receiver is null && invocation.Arguments.Length > 0 && IsRenderTreeBuilderMethod(invocation.TargetMethod))
            receiver = invocation.Arguments[0].Value;

        while (receiver is IConversionOperation conversion)
            receiver = conversion.Operand;

        return receiver is ILocalReferenceOperation localReference &&
               context.SecondaryBuilders.Contains(localReference.Local);
    }

    private static bool TryResolveLoopControlVariable(IOperation operation, out ILocalSymbol local)
    {
        local = null!;
        while (operation is IConversionOperation conversion)
            operation = conversion.Operand;

        switch (operation)
        {
            case ILocalReferenceOperation localReference:
                local = localReference.Local;
                return true;

            case IVariableDeclaratorOperation declarator:
                local = declarator.Symbol;
                return true;

            case IVariableDeclarationOperation declaration
                when declaration.Declarators.Length == 1:
                local = declaration.Declarators[0].Symbol;
                return true;

            case IVariableDeclarationGroupOperation group
                when group.Declarations.Length == 1 &&
                     group.Declarations[0].Declarators.Length == 1:
                local = group.Declarations[0].Declarators[0].Symbol;
                return true;

            default:
                return false;
        }
    }

    private static string SanitizeJavaScriptIdentifierPart(string value, string fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
            return fallback;

        var builder = new System.Text.StringBuilder(value.Length);
        for (var index = 0; index < value.Length; index++)
        {
            var ch = value[index];
            var valid = index == 0
                ? char.IsLetter(ch) || ch == '_' || ch == '$'
                : char.IsLetterOrDigit(ch) || ch == '_' || ch == '$';
            builder.Append(valid ? ch : '_');
        }

        return builder.Length == 0 ? fallback : builder.ToString();
    }

    private static ComponentImportDescriptor ResolveComponentImport(INamedTypeSymbol componentType)
    {
        var exportPath = GetECMAScriptModuleExportPath(componentType);
        if (!string.IsNullOrWhiteSpace(exportPath))
            return new ComponentImportDescriptor(NormalizeModuleImportPath(exportPath!), "default");

        foreach (var attribute in componentType.GetAttributes())
        {
            if (!string.Equals(
                    attribute.AttributeClass?.ToDisplayString(),
                    VueLibraryComponentAttributeMetadataName,
                    StringComparison.Ordinal))
            {
                continue;
            }

            if (attribute.ConstructorArguments.Length == 2 &&
                attribute.ConstructorArguments[0].Value is string importSpecifier &&
                attribute.ConstructorArguments[1].Value is string exportName &&
                !string.IsNullOrWhiteSpace(importSpecifier) &&
                !string.IsNullOrWhiteSpace(exportName))
            {
                return new ComponentImportDescriptor(
                    importSpecifier.Trim(),
                    exportName.Trim());
            }
        }

        throw new InvalidOperationException(
            "RazorVue component '" +
            componentType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) +
            "' must declare [ECMAScriptModule(\"./path\")] or [VueLibraryComponent(\"package\", \"Export\")] for direct render lowering.");
    }

    private static string? GetECMAScriptModuleExportPath(INamedTypeSymbol componentType)
    {
        foreach (var attribute in componentType.GetAttributes())
        {
            if (string.Equals(attribute.AttributeClass?.ToDisplayString(), ECMAScriptModuleAttributeMetadataName, StringComparison.Ordinal) &&
                attribute.ConstructorArguments.Length == 1 &&
                attribute.ConstructorArguments[0].Value is string exportPath &&
                !string.IsNullOrWhiteSpace(exportPath))
            {
                return exportPath;
            }
        }

        return null;
    }

    private static ImmutableDictionary<string, string> BuildComponentParameterNameMap(INamedTypeSymbol componentType)
        => LibraryComponentConventions.BuildParameterRuntimeNameMap(componentType);

    private static ImmutableDictionary<string, string> BuildComponentSlotParameterNameMap(INamedTypeSymbol componentType)
    {
        var names = ImmutableDictionary.CreateBuilder<string, string>(StringComparer.Ordinal);
        foreach (var property in LibraryComponentConventions.GetEffectiveParameterProperties(componentType))
        {
            if (IsAnyRenderFragmentType(property.Type))
                names.Add(property.Name, LibraryComponentConventions.GetSlotRuntimeName(componentType, property));
        }

        return names.ToImmutable();
    }

    private static ImmutableDictionary<IPropertySymbol, string> BuildComponentSlotNameMap(INamedTypeSymbol componentType)
    {
        var names = ImmutableDictionary.CreateBuilder<IPropertySymbol, string>(SymbolComparer);
        foreach (var property in LibraryComponentConventions
                     .GetEffectiveParameterProperties(componentType))
        {
            if (!IsAnyRenderFragmentType(property.Type))
                continue;

            var slotName = LibraryComponentConventions.GetSlotRuntimeName(componentType, property);
            if (!string.IsNullOrWhiteSpace(slotName))
                names[property.OriginalDefinition] = slotName;
        }

        return names.ToImmutable();
    }

    private static bool IsRenderFragmentType(ITypeSymbol type)
        => type is INamedTypeSymbol named &&
           !named.IsGenericType &&
           string.Equals(named.OriginalDefinition.ToDisplayString(), "Microsoft.AspNetCore.Components.RenderFragment", StringComparison.Ordinal);

    private static bool IsAnyRenderFragmentType(ITypeSymbol type)
        => IsRenderFragmentType(type) || IsGenericRenderFragmentType(type);

    private static bool IsRenderFragmentDescriptorType(ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol namedType)
            return false;

        var hasRenderFragmentProperty = false;
        for (INamedTypeSymbol? current = namedType;
             current is not null && current.SpecialType != SpecialType.System_Object;
             current = current.BaseType)
        {
            foreach (var property in current.GetMembers().OfType<IPropertySymbol>())
            {
                if (property.IsStatic)
                    continue;
                if (property.IsIndexer || !IsAnyRenderFragmentType(property.Type))
                    return false;

                hasRenderFragmentProperty = true;
            }

            if (current.GetMembers().OfType<IFieldSymbol>()
                .Any(static field => !field.IsStatic && !field.IsImplicitlyDeclared))
            {
                return false;
            }
        }

        return hasRenderFragmentProperty;
    }

    private static FunctionDeclaration BuildComponentAttributeNormalizer(string helperName)
    {
        var attributes = new Identifier("attributes");
        var parameterNames = new Identifier("parameterNames");
        var entries = new Identifier("entries");
        var result = new Identifier("result");
        var entry = new Identifier("entry");
        var name = new Identifier("name");
        var value = new Identifier("value");

        var entryName = new MemberExpression(
            entry,
            new NumericLiteral(0, "0"),
            computed: true,
            optional: false);
        var entryValue = new MemberExpression(
            entry,
            new NumericLiteral(1, "1"),
            computed: true,
            optional: false);
        var isEntryArray = Call(
            new MemberExpression(
                new Identifier("Array"),
                new Identifier("isArray"),
                computed: false,
                optional: false),
            entry);
        var pairName = new ConditionalExpression(
            isEntryArray,
            entryName,
            new LogicalExpression(
                Operator.NullishCoalescing,
                new MemberExpression(entry, new Identifier("key"), computed: false, optional: false),
                new MemberExpression(entry, new Identifier("Key"), computed: false, optional: false)));
        var pairValue = new ConditionalExpression(
            isEntryArray,
            entryValue,
            new LogicalExpression(
                Operator.NullishCoalescing,
                new MemberExpression(entry, new Identifier("value"), computed: false, optional: false),
                new MemberExpression(entry, new Identifier("Value"), computed: false, optional: false)));

        var runtimeName = new LogicalExpression(
            Operator.NullishCoalescing,
            new MemberExpression(parameterNames, name, computed: true, optional: false),
            name);

        var hasNoAttributes = new LogicalExpression(
            Operator.LogicalOr,
            new NonLogicalBinaryExpression(Operator.StrictEquality, attributes, Null()),
            new NonLogicalBinaryExpression(
                Operator.StrictEquality,
                attributes,
                new Identifier("undefined")));
        var iterableEntries = new LogicalExpression(
            Operator.LogicalOr,
            new NonLogicalBinaryExpression(Operator.InstanceOf, attributes, new Identifier("Map")),
            Call(
                new MemberExpression(
                    new Identifier("Array"),
                    new Identifier("isArray"),
                    computed: false,
                    optional: false),
                attributes));
        var objectEntries = Call(
            new MemberExpression(
                new Identifier("Object"),
                new Identifier("entries"),
                computed: false,
                optional: false),
            attributes);
        var entriesExpression = new ConditionalExpression(iterableEntries, attributes, objectEntries);
        var loopBody = new NestedBlockStatement(NodeList.From<Statement>(
            new VariableDeclaration(
                VariableDeclarationKind.Const,
                NodeList.From(new VariableDeclarator(name, pairName))),
            new VariableDeclaration(
                VariableDeclarationKind.Const,
                NodeList.From(new VariableDeclarator(value, pairValue))),
            new NonSpecialExpressionStatement(new AssignmentExpression(
                Operator.Assignment,
                new MemberExpression(result, runtimeName, computed: true, optional: false),
                value))));

        var body = new FunctionBody(
            NodeList.From<Statement>(
                new IfStatement(
                    hasNoAttributes,
                    new ReturnStatement(new ObjectExpression(NodeList.Empty<Node>())),
                    null),
                new VariableDeclaration(
                    VariableDeclarationKind.Const,
                    NodeList.From(new VariableDeclarator(entries, entriesExpression))),
                new VariableDeclaration(
                    VariableDeclarationKind.Const,
                    NodeList.From(new VariableDeclarator(result, new ObjectExpression(NodeList.Empty<Node>())))),
                new ForOfStatement(
                    new VariableDeclaration(
                        VariableDeclarationKind.Const,
                        NodeList.From(new VariableDeclarator(entry, null))),
                    entries,
                    loopBody,
                    @await: false),
                new ReturnStatement(result)),
            strict: true);
        return new FunctionDeclaration(
            new Identifier(helperName),
            NodeList.From<Node>(attributes, parameterNames),
            body,
            generator: false,
            async: false);
    }

    private static Expression BuildSlotInvocationExpression(
        string slotName,
        params Expression[] arguments)
    {
        return new ConditionalExpression(
            BuildSlotAvailabilityCondition(slotName),
            BuildSlotCallExpression(slotName, arguments),
            Null());
    }

    private static Expression BuildSlotValueExpression(string slotName)
        => new ConditionalExpression(
            BuildSlotAvailabilityCondition(slotName),
            FormatSlotAccessExpression(slotName),
            Null());

    private static Expression BuildSlotAvailabilityCondition(string slotName)
        => new NonLogicalBinaryExpression(
            Operator.StrictEquality,
            new NonUpdateUnaryExpression(Operator.TypeOf, FormatSlotAccessExpression(slotName)),
            StringLiteral("function"));

    private static Expression BuildSlotCallExpression(
        string slotName,
        params Expression[] arguments)
        => Call(FormatSlotAccessExpression(slotName), arguments);

    private static Expression InvokeRenderFragment(
        DirectRenderFragment fragment,
        Expression argument)
    {
        if (fragment.ParameterName is null)
            return fragment.RenderExpression;

        var function = new ArrowFunctionExpression(
            NodeList.From<Node>(new Identifier(fragment.ParameterName)),
            fragment.RenderExpression,
            expression: true,
            async: false);
        return Call(function, argument);
    }

    private static Expression BuildNullableMarkupContent(Expression markupExpression)
    {
        // AddContent(MarkupString?) omits null rather than materializing a static vnode.
        // Keep the source expression single-evaluated because a component property getter can
        // be observable, then let parent frames expand null to zero children.
        var markup = new Identifier("__markup");
        var isAbsent = new LogicalExpression(
            Operator.LogicalOr,
            new NonLogicalBinaryExpression(Operator.StrictEquality, markup, Null()),
            new NonLogicalBinaryExpression(Operator.StrictEquality, markup, new Identifier("undefined")));
        var content = new ConditionalExpression(
            isAbsent,
            Null(),
            Call("createStaticVNode", markup, new NumericLiteral(1, "1")));
        var declaration = new VariableDeclaration(
            VariableDeclarationKind.Const,
            NodeList.From(new VariableDeclarator(markup, markupExpression)));
        return Call(new ArrowFunctionExpression(
            NodeList.Empty<Node>(),
            new FunctionBody(NodeList.From<Statement>(declaration, new ReturnStatement(content)), strict: true),
            expression: false,
            async: false));
    }

    private static Expression FormatSlotAccessExpression(string slotName)
        => JavaScriptAstFactory.IsJavaScriptIdentifierName(slotName)
            ? new MemberExpression(new Identifier("slots"), new Identifier(slotName), computed: false, optional: false)
            : new MemberExpression(new Identifier("slots"), StringLiteral(slotName), computed: true, optional: false);

    private static Expression BuildDirectDomBindHandler(Expression handlerExpression, string attributeName)
    {
        var eventOrValue = new Identifier("eventOrValue");
        var args = new Identifier("args");
        var target = new MemberExpression(eventOrValue, new Identifier("target"), computed: false, optional: false);
        var condition = LogicalAnd(
            new NonLogicalBinaryExpression(Operator.StrictInequality, eventOrValue, Null()),
            new NonLogicalBinaryExpression(Operator.StrictInequality, eventOrValue, new Identifier("undefined")),
            new NonLogicalBinaryExpression(
                Operator.StrictEquality,
                new NonUpdateUnaryExpression(Operator.TypeOf, eventOrValue),
                StringLiteral("object")),
            new NonLogicalBinaryExpression(Operator.StrictInequality, target, Null()),
            new NonLogicalBinaryExpression(Operator.StrictInequality, target, new Identifier("undefined")),
            new NonLogicalBinaryExpression(Operator.In, StringLiteral(attributeName), target));
        var value = new Identifier("value");
        var valueExpression = new ConditionalExpression(
            condition,
            new MemberExpression(target, StringLiteral(attributeName), computed: true, optional: false),
            eventOrValue);
        var declaration = new VariableDeclaration(
            VariableDeclarationKind.Const,
            NodeList.From(new VariableDeclarator(value, valueExpression)));
        var invocation = new CallExpression(
            handlerExpression,
            NodeList.From<Expression>(value, new SpreadElement(args)),
            optional: false);
        var body = new FunctionBody(
            NodeList.From<Statement>(declaration, new ReturnStatement(invocation)),
            strict: true);
        return new ArrowFunctionExpression(
            NodeList.From<Node>(eventOrValue, new RestElement(args)),
            body,
            expression: false,
            async: false);
    }

    private static Expression BuildDirectEventModifierHandler(Expression handlerExpression, DirectEventModifier modifier)
    {
        var eventParameter = new Identifier("event");
        var args = new Identifier("args");
        var statements = new List<Statement>();
        AddDirectEventModifierStatement(statements, eventParameter, modifier.PreventDefaultCondition, "preventDefault");
        AddDirectEventModifierStatement(statements, eventParameter, modifier.StopPropagationCondition, "stopPropagation");
        statements.Add(new ReturnStatement(new CallExpression(
            handlerExpression,
            NodeList.From<Expression>(eventParameter, new SpreadElement(args)),
            optional: false)));
        return new ArrowFunctionExpression(
            NodeList.From<Node>(eventParameter, new RestElement(args)),
            new FunctionBody(NodeList.From(statements), strict: true),
            expression: false,
            async: false);
    }

    private static void AddDirectEventModifierStatement(
        List<Statement> statements,
        Expression eventParameter,
        Expression? condition,
        string methodName)
    {
        if (condition is null || condition is BooleanLiteral { Value: false })
            return;

        var invocation = new NonSpecialExpressionStatement(OptionalMethodCall(eventParameter, methodName));
        statements.Add(condition is BooleanLiteral { Value: true }
            ? invocation
            : new IfStatement(condition, invocation, null));
    }

    private static CallExpression OptionalMethodCall(Expression receiver, string methodName)
        => new(
            new MemberExpression(receiver, new Identifier(methodName), computed: false, optional: true),
            NodeList.From<Expression>(),
            optional: true);

    private static bool IsDirectEventAttributeName(string name)
        => name.StartsWith("on", StringComparison.Ordinal) &&
           name.Length > 2 &&
           char.IsUpper(name[2]);

    private static string NormalizeModuleImportPath(string path)
        => ECMAScriptModulePath.NormalizeRootRelativeImportSpecifier(path);

    private static bool TryGetConstantString(IOperation operation, out string value)
    {
        while (operation is IConversionOperation conversion)
            operation = conversion.Operand;

        if (operation.ConstantValue.HasValue && operation.ConstantValue.Value is string constant)
        {
            value = constant;
            return true;
        }

        value = string.Empty;
        return false;
    }

    private static void RequireOmittableSequence(IOperation operation)
    {
        if (!CanOmit(operation))
            throw Unsupported(operation, "RenderTreeBuilder sequence arguments must be side-effect-free for direct render lowering.");
    }

    private static bool CanOmit(IOperation operation)
    {
        while (operation is IConversionOperation conversion)
            operation = conversion.Operand;

        return operation.ConstantValue.HasValue ||
               operation is IParameterReferenceOperation ||
               operation is ILocalReferenceOperation ||
               operation is IFieldReferenceOperation ||
               operation is IPropertyReferenceOperation ||
               operation is IBinaryOperation binary && CanOmit(binary.LeftOperand) && CanOmit(binary.RightOperand);
    }

    private static bool IsRenderTreeBuilderMethod(IMethodSymbol method)
        => string.Equals(
            method.ContainingType?.OriginalDefinition.ToDisplayString(Format.NameFormat),
            RenderTreeBuilderMetadataName,
            StringComparison.Ordinal);

    private static bool IsRenderTreeBuilder(ITypeSymbol type)
        => string.Equals(
            type.OriginalDefinition.ToDisplayString(Format.NameFormat),
            RenderTreeBuilderMetadataName,
            StringComparison.Ordinal);

    private static void EnsureSignature(IOperation operation, bool condition)
    {
        if (!condition)
            throw Unsupported(operation, "Unsupported RenderTreeBuilder overload for direct render operation lowering: " + operation.Syntax);
    }

    private static OperationTransformationException Unsupported(IOperation operation, string message)
        => new(operation, message);

    /// <summary>Maintains the active RenderTree frame stack and produces the final vnode expression.</summary>
    private sealed class RenderState
    {
        public List<Expression> Roots { get; } = new();

        public Stack<Frame> Stack { get; } = new();

        public List<Statement> PendingPreludeStatements { get; } = new();

        private List<Expression> Guards { get; } = new();

        public bool UsesFragment { get; set; }

        public bool UsesStaticVNode { get; set; }

        public Expression ToRenderExpression()
        {
            return Roots.Count switch
            {
                0 => Null(),
                1 when Roots[0] is SpreadElement spread => spread.Argument,
                1 => Roots[0],
                _ => CreateFragment(Roots)
            };
        }

        public void StartChildren()
        {
            if (Stack.Count > 0)
                Stack.Peek().ChildrenStarted = true;
        }

        public void AddChild(Expression expression)
        {
            if (Stack.Count == 0)
            {
                expression = WrapWithStatements(PendingPreludeStatements, expression);
                PendingPreludeStatements.Clear();
                Roots.Add(ApplyGuards(expression));
                return;
            }

            var frame = Stack.Peek();
            frame.ChildrenStarted = true;
            frame.Children.Add(expression);
        }

        public void AddChildSequence(Expression expression)
        {
            if (Stack.Count == 0)
            {
                expression = WrapWithStatements(PendingPreludeStatements, expression);
                PendingPreludeStatements.Clear();
                expression = VueSlotAstFactory.NormalizeContent(ApplyGuards(expression));
                Roots.Add(new SpreadElement(expression));
                return;
            }

            var frame = Stack.Peek();
            frame.ChildrenStarted = true;
            frame.Children.Add(new SpreadElement(
                VueSlotAstFactory.NormalizeContent(expression)));
        }

        public void AddOptionalChild(Expression expression)
        {
            if (Stack.Count == 0)
            {
                AddChild(expression);
                return;
            }

            AddChildSequence(expression);
        }

        public void AddGuard(Expression expression)
        {
            Guards.Add(expression);
        }

        public void Clear()
        {
            Roots.Clear();
            Stack.Clear();
            PendingPreludeStatements.Clear();
            Guards.Clear();
            UsesFragment = false;
            UsesStaticVNode = false;
        }

        private Expression ApplyGuards(Expression expression)
        {
            if (Guards.Count == 0)
                return expression;

            var guard = Guards[0];
            for (var index = 1; index < Guards.Count; index++)
                guard = new LogicalExpression(Operator.LogicalAnd, guard, Guards[index]);
            return new ConditionalExpression(guard, expression, Null());
        }

        public void Close<TFrame>(IOperation operation)
            where TFrame : Frame
        {
            if (Stack.Count == 0 || Stack.Peek() is not TFrame)
                throw Unsupported(operation, "RenderTreeBuilder frame close order is invalid for direct render lowering.");

            var frame = Stack.Pop();
            if (frame is RegionFrame region && region.Children.Count > 1)
                UsesFragment = true;
            AddChild(frame.ToRenderExpression());
        }
    }

    /// <summary>Base stack frame for an open Razor render region. 子类决定关闭后的 Vue vnode 形状。</summary>
    private abstract class Frame
    {
        public bool ChildrenStarted { get; set; }

        public List<Expression> Children { get; } = new();

        public abstract Expression ToRenderExpression();

        public virtual string Describe()
            => GetType().Name;
    }

    /// <summary>Common frame for elements/components that collect props before their children close.</summary>
    private abstract class PropFrame : Frame
    {
        private readonly List<PropSource> _propSources = new();
        private readonly List<Expression> _referenceCaptures = new();
        private readonly Func<ObjectExpression, Expression>? _hoistStaticProps;
        private readonly Func<ObjectExpression, bool>? _canHoistStaticProps;
        private readonly Func<Expression, Expression>? _cacheStableEventHandler;
        private readonly Func<Expression, bool>? _canCacheStableEventHandler;
        private readonly Func<Expression, bool>? _isStableEventHandler;
        private string? _lastAttributeName;

        protected PropFrame(
            Func<ObjectExpression, Expression>? hoistStaticProps = null,
            Func<ObjectExpression, bool>? canHoistStaticProps = null,
            Func<Expression, Expression>? cacheStableEventHandler = null,
            Func<Expression, bool>? canCacheStableEventHandler = null,
            Func<Expression, bool>? isStableEventHandler = null)
        {
            _hoistStaticProps = hoistStaticProps;
            _canHoistStaticProps = canHoistStaticProps;
            _cacheStableEventHandler = cacheStableEventHandler;
            _canCacheStableEventHandler = canCacheStableEventHandler;
            _isStableEventHandler = isStableEventHandler;
        }

        protected IEnumerable<DirectAttribute> Attributes
            => _propSources
                .OfType<AttributePropSource>()
                .Select(static source => source.Attribute);

        public void AddAttribute(DirectAttribute attribute)
        {
            _propSources.Add(new AttributePropSource(attribute));
            _lastAttributeName = attribute.Name;
        }

        public bool AddMultipleAttributes(Expression attributesExpression)
        {
            if (attributesExpression is NullLiteral ||
                attributesExpression is Identifier { Name: "undefined" })
            {
                return false;
            }

            _propSources.Add(new MultipleAttributesPropSource(attributesExpression));
            _lastAttributeName = null;
            return true;
        }

        public void AddConditionalAttributes(
            Expression condition,
            ImmutableArray<DirectAttribute> whenTrue,
            ImmutableArray<DirectAttribute> whenFalse)
        {
            if (whenTrue.Length == 0 && whenFalse.Length == 0)
                return;

            _propSources.Add(new ConditionalAttributesPropSource(condition, whenTrue, whenFalse));
            _lastAttributeName = null;
        }

        public void AddReferenceCapture(Expression capture)
        {
            if (capture is NullLiteral || capture is Identifier { Name: "undefined" })
                return;

            _referenceCaptures.Add(capture);
        }

        public bool TrySetLastAttributeValue(Expression valueExpression)
        {
            if (_lastAttributeName is null)
                return false;

            for (var index = _propSources.Count - 1; index >= 0; index--)
            {
                if (_propSources[index] is AttributePropSource source &&
                    string.Equals(source.Attribute.Name, _lastAttributeName, StringComparison.Ordinal))
                {
                    _propSources[index] = source with
                    {
                        Attribute = source.Attribute with { ValueExpression = valueExpression }
                    };
                    return true;
                }
            }

            return false;
        }

        public abstract string NormalizeAttributeName(string name);

        protected virtual Expression FormatAttributeValueExpression(DirectAttribute attribute)
            => attribute.ValueExpression;

        protected virtual bool ShouldCacheEventHandler(
            DirectAttribute attribute,
            Expression formattedValue)
            => IsDirectEventAttributeName(attribute.Name) &&
               CanCacheStableEventHandler(formattedValue);

        protected bool CanCacheStableEventHandler(Expression handler)
            => _canCacheStableEventHandler?.Invoke(handler) == true;

        protected bool IsStableEventHandler(Expression handler)
            => _isStableEventHandler?.Invoke(handler) == true;

        protected virtual bool IsStableEventAttribute(DirectAttribute attribute)
            => IsDirectEventAttributeName(attribute.Name) &&
               IsStableEventHandler(attribute.ValueExpression);

        protected Expression FormatPropsExpression()
        {
            var arguments = new List<Expression>();
            var properties = new List<Node>();

            foreach (var source in _propSources)
            {
                switch (source)
                {
                    case AttributePropSource attribute:
                        properties.Add(CreateAttributeProperty(attribute.Attribute));
                        break;

                    case MultipleAttributesPropSource multipleAttributes:
                        FlushProperties(arguments, properties);
                        arguments.Add(multipleAttributes.Expression);
                        break;

                    case ConditionalAttributesPropSource conditionalAttributes:
                        FlushProperties(arguments, properties);
                        arguments.Add(new ConditionalExpression(
                            conditionalAttributes.Condition,
                            CreateAttributeObject(conditionalAttributes.WhenTrue),
                            CreateAttributeObject(conditionalAttributes.WhenFalse)));
                        break;

                    default:
                        throw new NotSupportedException("Unsupported direct render prop source: " + source.GetType().Name);
                }
            }

            if (_referenceCaptures.Count > 0)
                properties.Add(CreateObjectProperty("ref", FormatReferenceCaptureExpression()));
            FlushProperties(arguments, properties);

            var result = arguments.Count switch
            {
                0 => Null(),
                1 => arguments[0],
                _ => Call("mergeProps", arguments)
            };

            // Hoist only one plain object literal. This excludes mergeProps, conditional
            // sources, splats, ref captures, and any dynamic value by construction.
            if (arguments.Count == 1 &&
                arguments[0] is ObjectExpression objectExpression &&
                _hoistStaticProps is not null &&
                _canHoistStaticProps?.Invoke(objectExpression) == true)
            {
                result = _hoistStaticProps(objectExpression);
            }

            return result;
        }

        private ObjectProperty CreateAttributeProperty(DirectAttribute attribute)
        {
            var value = FormatAttributeValueExpression(attribute);
            if (ShouldCacheEventHandler(attribute, value) && _cacheStableEventHandler is not null)
                value = _cacheStableEventHandler(value);
            return CreateObjectProperty(attribute.Name, value);
        }

        private ObjectExpression CreateAttributeObject(IEnumerable<DirectAttribute> attributes)
            => new(NodeList.From<Node>(attributes.Select(CreateAttributeProperty)));

        private static void FlushProperties(List<Expression> arguments, List<Node> properties)
        {
            if (properties.Count == 0)
                return;

            arguments.Add(new ObjectExpression(NodeList.From(properties)));
            properties.Clear();
        }

        protected DirectPatchMetadata BuildPatchMetadata(
            bool hasBlockChild,
            bool componentProps = false,
            int additionalFlags = 0)
        {
            var flags = additionalFlags;
            var fullProps = false;
            var dynamicProps = new List<string>();
            var seenDynamicProps = new HashSet<string>(StringComparer.Ordinal);

            foreach (var source in _propSources)
            {
                if (source is MultipleAttributesPropSource or ConditionalAttributesPropSource)
                {
                    fullProps = true;
                    continue;
                }

                if (source is not AttributePropSource { Attribute: var attribute })
                    continue;

                if (string.Equals(attribute.Name, "ref", StringComparison.Ordinal))
                {
                    flags |= VuePatchFlags.NeedPatch;
                    continue;
                }

                if (IsStaticPropValueCore(attribute.ValueExpression))
                    continue;

                if (string.Equals(attribute.Name, "key", StringComparison.Ordinal))
                {
                    fullProps = true;
                    continue;
                }

                if (IsStableEventAttribute(attribute))
                    continue;

                if (string.Equals(attribute.Name, "class", StringComparison.Ordinal))
                {
                    if (componentProps)
                    {
                        // CLASS/STYLE are DOM-element fast paths in Vue. A component must
                        // expose these as ordinary dynamic props, or shouldUpdateComponent()
                        // can skip the child update entirely. 组件 class/style 不能复用元素 flag。
                        flags |= VuePatchFlags.Props;
                        if (seenDynamicProps.Add(attribute.Name))
                            dynamicProps.Add(attribute.Name);
                    }
                    else
                    {
                        flags |= VuePatchFlags.Class;
                    }
                    continue;
                }

                if (string.Equals(attribute.Name, "style", StringComparison.Ordinal))
                {
                    if (componentProps)
                    {
                        // Keep the runtime name in dynamicProps: custom component ABI may
                        // deliberately map a C# property to the exact lowercase Vue key.
                        // dynamicProps 保留实际 Vue 名，不能回写 Razor 参数名。
                        flags |= VuePatchFlags.Props;
                        if (seenDynamicProps.Add(attribute.Name))
                            dynamicProps.Add(attribute.Name);
                    }
                    else
                    {
                        flags |= VuePatchFlags.Style;
                    }
                    continue;
                }

                flags |= VuePatchFlags.Props;
                if (seenDynamicProps.Add(attribute.Name))
                    dynamicProps.Add(attribute.Name);
            }

            if (_referenceCaptures.Count > 0)
                flags |= VuePatchFlags.NeedPatch;

            if (fullProps)
            {
                flags &= ~VuePatchFlags.Props;
                flags |= VuePatchFlags.FullProps;
                dynamicProps.Clear();
            }

            return new DirectPatchMetadata(
                RequiresBlock: hasBlockChild || flags != 0,
                Flag: flags,
                DynamicProps: dynamicProps.Count == 0 ? null : dynamicProps.ToImmutableArray());
        }

        private static bool IsStaticPropValueCore(Expression expression)
            => expression is NullLiteral or Acornima.Ast.StringLiteral or BooleanLiteral or NumericLiteral or BigIntLiteral;

        private Expression FormatReferenceCaptureExpression()
        {
            if (_referenceCaptures.Count == 1)
                return _referenceCaptures[0];

            var value = new Identifier("value");
            var statements = _referenceCaptures
                .Select(capture => (Statement)new NonSpecialExpressionStatement(Call(capture, value)))
                .ToArray();
            return new ArrowFunctionExpression(
                NodeList.From<Node>(value),
                new FunctionBody(NodeList.From(statements), strict: true),
                expression: false,
                async: false);
        }
    }

    /// <summary>Accumulates one HTML element's attributes and children.</summary>
    private sealed class ElementFrame : PropFrame
    {
        private readonly Expression _tagExpression;
        private readonly Dictionary<string, DirectEventModifier> _eventModifiers = new(StringComparer.Ordinal);
        private readonly Action? _useBlockTree;
        private string? _updatesAttributeName;
        private string? _updatesEventName;

        public ElementFrame(Expression tagExpression, string tagName)
            : this(tagExpression, tagName, null, null, null, null, null, null)
        {
        }

        public ElementFrame(
            Expression tagExpression,
            string tagName,
            Func<ObjectExpression, Expression>? hoistStaticProps,
            Func<ObjectExpression, bool>? canHoistStaticProps,
            Func<Expression, Expression>? cacheStableEventHandler,
            Func<Expression, bool>? canCacheStableEventHandler,
            Func<Expression, bool>? isStableEventHandler,
            Action? useBlockTree)
            : base(
                hoistStaticProps,
                canHoistStaticProps,
                cacheStableEventHandler,
                canCacheStableEventHandler,
                isStableEventHandler)
        {
            _tagExpression = tagExpression;
            TagName = tagName;
            _useBlockTree = useBlockTree;
        }

        public string TagName { get; }

        public override string NormalizeAttributeName(string name)
            => NormalizeDirectElementAttributeName(name);

        public override Expression ToRenderExpression()
        {
            var props = FormatPropsExpression();
            var children = FormatChildrenExpression();
            var patch = BuildPatchMetadata(
                hasBlockChild: false);
            // `createElementBlock` records an empty dynamicChildren list. It is correct only
            // when this frame has no immediate children; otherwise Vue would skip ordinary
            // child diffing for dynamic text/conditional/foreach content that this lowering
            // has not yet converted into individually flagged VNodes.
            // 带即时 children 时暂保留 h() 的完整 children diff，避免错误空 block 快路径。
            if (Children.Count != 0 || !patch.RequiresBlock)
                return Call("h", _tagExpression, props, children);

            _useBlockTree?.Invoke();
            var arguments = new List<Expression> { _tagExpression, props, children };
            if (patch.Flag != 0 || patch.DynamicProps is not null)
            {
                arguments.Add(new NumericLiteral(
                    patch.Flag,
                    patch.Flag.ToString(System.Globalization.CultureInfo.InvariantCulture)));
                if (patch.DynamicProps is not null)
                    arguments.Add(CreateArray(patch.DynamicProps.Value.Select(static name => StringLiteral(name))));
            }

            // Comma expression preserves a single render invocation with no wrapper closure.
            // openBlock/createElementBlock mirrors Vue compiler's block collection protocol.
            return new SequenceExpression(NodeList.From<Expression>(Call("openBlock"), Call("createElementBlock", arguments)));
        }

        public override string Describe()
            => "ElementFrame('" + TagName + "')";

        public void SetUpdatesAttributeName(string name)
        {
            _updatesAttributeName = name;
            _updatesEventName = Attributes
                .LastOrDefault(static attribute => IsDirectEventAttributeName(attribute.Name))
                ?.Name;
        }

        public void SetEventModifier(
            string eventName,
            Expression condition,
            bool preventDefault,
            bool stopPropagation)
        {
            var runtimeName = NormalizeDirectElementAttributeName(eventName);
            _eventModifiers.TryGetValue(runtimeName, out var existing);
            _eventModifiers[runtimeName] = new DirectEventModifier(
                preventDefault
                    ? MergeDirectEventModifierCondition(existing.PreventDefaultCondition, condition)
                    : existing.PreventDefaultCondition,
                stopPropagation
                    ? MergeDirectEventModifierCondition(existing.StopPropagationCondition, condition)
                    : existing.StopPropagationCondition);
        }

        private static Expression MergeDirectEventModifierCondition(Expression? existing, Expression condition)
        {
            if (existing is null)
                return condition;
            if (existing is BooleanLiteral { Value: true } || condition is BooleanLiteral { Value: true })
                return new BooleanLiteral(true, "true");

            return new LogicalExpression(Operator.LogicalOr, existing, condition);
        }

        protected override Expression FormatAttributeValueExpression(DirectAttribute attribute)
        {
            var value = attribute.ValueExpression;
            if (_updatesAttributeName is not null &&
                string.Equals(attribute.Name, _updatesEventName, StringComparison.Ordinal))
            {
                value = BuildDirectDomBindHandler(value, _updatesAttributeName);
            }

            if (_eventModifiers.TryGetValue(attribute.Name, out var modifier))
                value = BuildDirectEventModifierHandler(value, modifier);

            return value;
        }

        protected override bool ShouldCacheEventHandler(
            DirectAttribute attribute,
            Expression formattedValue)
        {
            if (_eventModifiers.ContainsKey(attribute.Name))
                return false;

            if (IsUpdatesEvent(attribute))
            {
                // The bind adapter itself is a fresh arrow on every render. Its captured
                // Razor binder must still be an inline stable closure, otherwise caching would
                // retain a stale user-supplied callback. Official @bind satisfies this shape.
                // 无 modifier 的 @bind wrapper 可缓存；但底层 binder 必须是稳定 inline
                // closure，不能把动态回调错误固定在首轮 render。
                return CanCacheStableEventHandler(attribute.ValueExpression);
            }

            return base.ShouldCacheEventHandler(attribute, formattedValue);
        }

        protected override bool IsStableEventAttribute(DirectAttribute attribute)
            => !_eventModifiers.ContainsKey(attribute.Name) &&
               (IsUpdatesEvent(attribute)
                   ? CanCacheStableEventHandler(attribute.ValueExpression)
                   : base.IsStableEventAttribute(attribute));

        private bool IsUpdatesEvent(DirectAttribute attribute)
            => _updatesAttributeName is not null &&
               string.Equals(attribute.Name, _updatesEventName, StringComparison.Ordinal);

        private Expression FormatChildrenExpression()
            => Children.Count == 0 ? Null() : CreateArray(Children);
    }

    /// <summary>Accumulates component props, event listeners, and named/default slots.</summary>
    private sealed class ComponentFrame : PropFrame
    {
        private readonly Expression _componentExpression;
        private readonly ImmutableDictionary<string, string> _parameterNameMap;
        private readonly ImmutableDictionary<string, string> _slotNameMap;
        private readonly Action? _useBlockTree;

        public ComponentFrame(
            Expression componentExpression,
            ImmutableDictionary<string, string> parameterNameMap)
            : this(componentExpression, parameterNameMap, ImmutableDictionary<string, string>.Empty)
        {
        }

        public ComponentFrame(
            Expression componentExpression,
            ImmutableDictionary<string, string> parameterNameMap,
            ImmutableDictionary<string, string> slotNameMap)
            : this(componentExpression, parameterNameMap, slotNameMap, null, null, null, null, null, null)
        {
        }

        public ComponentFrame(
            Expression componentExpression,
            ImmutableDictionary<string, string> parameterNameMap,
            ImmutableDictionary<string, string> slotNameMap,
            Func<ObjectExpression, Expression>? hoistStaticProps,
            Func<ObjectExpression, bool>? canHoistStaticProps,
            Func<Expression, Expression>? cacheStableEventHandler,
            Func<Expression, bool>? canCacheStableEventHandler,
            Func<Expression, bool>? isStableEventHandler,
            Action? useBlockTree)
            : base(
                hoistStaticProps,
                canHoistStaticProps,
                cacheStableEventHandler,
                canCacheStableEventHandler,
                isStableEventHandler)
        {
            _componentExpression = componentExpression;
            _parameterNameMap = parameterNameMap;
            _slotNameMap = slotNameMap;
            _useBlockTree = useBlockTree;
        }

        public List<DirectSlot> Slots { get; } = new();

        public override string NormalizeAttributeName(string name)
            => _parameterNameMap.TryGetValue(name, out var mapped)
                ? mapped
                : name;

        public Expression CreateParameterNameMapExpression()
            => new ObjectExpression(NodeList.From<Node>(_parameterNameMap
                .OrderBy(static pair => pair.Key, StringComparer.Ordinal)
                .Select(static pair => (Node)CreateObjectProperty(pair.Key, StringLiteral(pair.Value)))));

        public string NormalizeSlotName(string name)
            => _parameterNameMap.TryGetValue(name, out var mapped)
                ? mapped
                : name;

        public bool TryGetDeclaredSlotName(string parameterName, out string runtimeName)
            => _slotNameMap.TryGetValue(parameterName, out runtimeName!);

        public override Expression ToRenderExpression()
        {
            var props = FormatPropsExpression();
            Expression? children = null;
            var additionalFlags = 0;
            if (Slots.Count > 0)
            {
                var slotMembers = new List<Node>(Slots.Count);
                foreach (var slot in Slots)
                {
                    if (slot.Fragment.Selection is null &&
                        slot.Fragment.AvailabilityCondition is null)
                    {
                        slotMembers.Add(CreateSlotProperty(slot, slot.Fragment));
                        continue;
                    }

                    slotMembers.Add(new SpreadElement(
                        CreateSlotProjectionExpression(slot, slot.Fragment)));
                }

                children = new ObjectExpression(NodeList.From(slotMembers));
                additionalFlags |= VuePatchFlags.DynamicSlots;
            }
            else if (Children.Count > 0)
            {
                children = Children.Count == 1
                    ? Children[0] is SpreadElement spread
                        ? spread.Argument
                        : Children[0]
                    : CreateArray(Children);
            }

            var patch = BuildPatchMetadata(
                hasBlockChild: false,
                componentProps: true,
                additionalFlags: additionalFlags);
            // Non-slot component children are eagerly created while the block is open. Keep
            // them on h() until each direct child carries a proven patch contract.
            if (Children.Count != 0 || !patch.RequiresBlock)
                return children is null
                    ? Call("h", _componentExpression, props)
                    : Call("h", _componentExpression, props, children);

            _useBlockTree?.Invoke();
            var arguments = new List<Expression> { _componentExpression, props, children ?? Null() };
            if (patch.Flag != 0 || patch.DynamicProps is not null)
            {
                arguments.Add(new NumericLiteral(
                    patch.Flag,
                    patch.Flag.ToString(System.Globalization.CultureInfo.InvariantCulture)));
                if (patch.DynamicProps is not null)
                    arguments.Add(CreateArray(patch.DynamicProps.Value.Select(static name => StringLiteral(name))));
            }

            return new SequenceExpression(NodeList.From<Expression>(Call("openBlock"), Call("createBlock", arguments)));
        }

        private static Expression CreateSlotProjectionExpression(
            DirectSlot slot,
            DirectRenderFragment fragment)
        {
            if (fragment.Selection is { } selection)
            {
                return new ConditionalExpression(
                    selection.Condition,
                    CreateSlotProjectionExpression(slot, selection.WhenTrue),
                    CreateSlotProjectionExpression(slot, selection.WhenFalse));
            }

            if (fragment.AvailabilityCondition is BooleanLiteral availability)
            {
                return availability.Value
                    ? CreateSlotPropertyObject(slot, fragment)
                    : new ObjectExpression(NodeList.Empty<Node>());
            }

            var propertyObject = CreateSlotPropertyObject(slot, fragment);
            return fragment.AvailabilityCondition is null
                ? propertyObject
                : new ConditionalExpression(
                    fragment.AvailabilityCondition,
                    propertyObject,
                    new ObjectExpression(NodeList.Empty<Node>()));
        }

        private static ObjectExpression CreateSlotPropertyObject(
            DirectSlot slot,
            DirectRenderFragment fragment)
        {
            var propertyObject = new ObjectExpression(NodeList.From<Node>(
                CreateSlotProperty(slot, fragment)));
            return propertyObject;
        }

        private static Property CreateSlotProperty(
            DirectSlot slot,
            DirectRenderFragment fragment)
            => CreateObjectProperty(
                slot.Name,
                new ArrowFunctionExpression(
                    slot.Fragment.ParameterName is null
                        ? NodeList.From<Node>()
                        : NodeList.From<Node>(new Identifier(slot.Fragment.ParameterName)),
                    NormalizeSlotRenderExpression(
                        BindSlotRenderExpression(slot.Fragment.ParameterName, fragment)),
                    expression: true,
                    async: false));

        private static Expression BindSlotRenderExpression(
            string? slotParameterName,
            DirectRenderFragment fragment)
        {
            var renderExpression = fragment.RenderExpressionWhenAvailable ?? fragment.RenderExpression;
            if (slotParameterName is null ||
                fragment.ParameterName is null ||
                string.Equals(slotParameterName, fragment.ParameterName, StringComparison.Ordinal))
            {
                return renderExpression;
            }

            var function = new ArrowFunctionExpression(
                NodeList.From<Node>(new Identifier(fragment.ParameterName)),
                renderExpression,
                expression: true,
                async: false);
            return Call(function, new Identifier(slotParameterName));
        }
    }

    /// <summary>Groups a RenderTree region without creating an extra DOM element.</summary>
    private sealed class RegionFrame : Frame
    {
        public override Expression ToRenderExpression()
            => Children.Count switch
            {
                0 => Null(),
                1 => Children[0],
                _ => CreateFragment(Children)
            };
    }

    /// <summary>Describes lexical context needed while lowering one expression into the active frame.</summary>
    private sealed record EmitContext(
        BuilderBinding Builder,
        ImmutableDictionary<IParameterSymbol, IOperation> Substitutions,
        ImmutableDictionary<IParameterSymbol, string> ParameterAliases,
        ImmutableDictionary<ILocalSymbol, string> LocalAliases,
        ImmutableDictionary<ILocalSymbol, DirectRenderFragment> LocalRenderFragments,
        ImmutableDictionary<ILocalSymbol, DirectRenderObject> LocalRenderObjects,
        ImmutableDictionary<ILocalSymbol, INamedTypeSymbol> LocalComponentTypes,
        ImmutableHashSet<ILocalSymbol> SecondaryBuilders,
        List<Statement> PreludeStatements,
        bool AllowPreludeDeclarations,
        SenseArgument Argument,
        bool IsTerminated = false);

    private readonly record struct BuilderBinding(ISymbol Symbol)
    {
        public static BuilderBinding ForSymbol(ISymbol symbol)
            => new(symbol);

        public bool Matches(IOperation operation, ImmutableDictionary<IParameterSymbol, IOperation> substitutions)
        {
            while (operation is IConversionOperation conversion)
                operation = conversion.Operand;

            if (operation is IParameterReferenceOperation parameterReference &&
                substitutions.TryGetValue(parameterReference.Parameter, out var substituted))
            {
                return Matches(substituted, substitutions);
            }

            return operation switch
            {
                IParameterReferenceOperation parameter => SymbolComparer.Equals(parameter.Parameter, Symbol),
                ILocalReferenceOperation localReference => SymbolComparer.Equals(localReference.Local, Symbol),
                _ => false
            };
        }
    }

    /// <summary>Represents a render expression and statements needed to evaluate it once.</summary>
    private sealed record LoweredRender(
        Expression RenderExpression,
        ImmutableArray<Statement> PreludeStatements,
        ImmutableArray<RenderModuleHoist> ModuleHoists,
        bool UsesFragment,
        bool UsesStaticVNode,
        bool UsesBlockTree,
        bool UsesHandlerCache,
        bool UsesSlots,
        ImmutableArray<ImportDeclaration> ImportDeclarations,
        ImmutableArray<ISymbol> ReferenceCaptureStateMembers);

    private readonly record struct ComponentImportDescriptor(
        string ImportSpecifier,
        string ExportName);

    /// <summary>Preserves an attribute pair until frame completion determines its Vue prop form.</summary>
    private sealed record DirectAttribute(
        string Name,
        Expression ValueExpression);

    /// <summary>Discriminated source of props merged into an element or component.</summary>
    private abstract record PropSource;

    /// <summary>Prop source backed by one ordinary Razor attribute.</summary>
    private sealed record AttributePropSource(DirectAttribute Attribute) : PropSource;

    /// <summary>Prop source backed by Razor's splatted-attributes expression.</summary>
    private sealed record MultipleAttributesPropSource(Expression Expression) : PropSource;

    /// <summary>Conditional prop source retained so the selected branch keeps source evaluation order.</summary>
    private sealed record ConditionalAttributesPropSource(
        Expression Condition,
        ImmutableArray<DirectAttribute> WhenTrue,
        ImmutableArray<DirectAttribute> WhenFalse) : PropSource;

    /// <summary>Represents a slot callback and source order before attachment to a component vnode.</summary>
    private sealed record DirectSlot(
        string Name,
        DirectRenderFragment Fragment);

    /// <summary>Models a RenderFragment value selected by conditional Razor control flow.</summary>
    private sealed record ConditionalRenderFragmentSelection(
        Expression Condition,
        DirectRenderFragment WhenTrue,
        DirectRenderFragment WhenFalse);

    private readonly record struct DirectRenderFragment(
        Expression RenderExpression,
        string? ParameterName = null,
        bool UsesFragment = false,
        bool UsesStaticVNode = false,
        Expression? AvailabilityCondition = null,
        Expression? RenderExpressionWhenAvailable = null,
        ConditionalRenderFragmentSelection? Selection = null,
        bool ReturnsVueSlotContent = false);

    private readonly record struct RenderFragmentHelperBody(
        IParameterSymbol? ValueParameter,
        IParameterSymbol Builder,
        IOperation Body,
        ImmutableArray<IVariableDeclarationGroupOperation> LocalRenderFragmentDeclarations);

    private readonly record struct DirectRenderFunction(
        string FunctionName,
        bool UsesFragment,
        bool UsesStaticVNode);

    /// <summary>Tracks a compile-time render-object local erased after direct lowering.</summary>
    private sealed record DirectRenderObject(
        ImmutableDictionary<IPropertySymbol, DirectRenderFragment> RenderFragments);

    private readonly record struct DirectRenderFragmentBody(
        Expression RenderExpression,
        bool UsesFragment,
        bool UsesStaticVNode);

    private readonly record struct DirectEventModifier(
        Expression? PreventDefaultCondition,
        Expression? StopPropagationCondition);

    /// <summary>Vue runtime patch-flag values used by the conservative direct-render subset.</summary>
    private static class VuePatchFlags
    {
        public const int Class = 1 << 1;
        public const int Style = 1 << 2;
        public const int Props = 1 << 3;
        public const int FullProps = 1 << 4;
        public const int NeedPatch = 1 << 9;
        public const int DynamicSlots = 1 << 10;
    }

    /// <summary>Captures one vnode's runtime update surface without widening C# semantics.</summary>
    private readonly record struct DirectPatchMetadata(
        bool RequiresBlock,
        int Flag,
        ImmutableArray<string>? DynamicProps);

    private static NullLiteral Null()
        => new("null");

    private static StringLiteral StringLiteral(string value)
        => JavaScriptAstFactory.CreateStringLiteral(value);

    private static CallExpression Call(string name, params Expression[] arguments)
        => Call(name, (IEnumerable<Expression>)arguments);

    private static CallExpression Call(string name, IEnumerable<Expression> arguments)
        => new(new Identifier(name), NodeList.From(arguments), optional: false);

    private static CallExpression Call(Expression callee, params Expression[] arguments)
        => new(callee, NodeList.From(arguments), optional: false);

    private static Expression LogicalAnd(params Expression[] expressions)
    {
        if (expressions.Length == 0)
            return new BooleanLiteral(true, "true");

        var result = expressions[0];
        for (var index = 1; index < expressions.Length; index++)
            result = new LogicalExpression(Operator.LogicalAnd, result, expressions[index]);
        return result;
    }

    private static ArrayExpression CreateArray(IEnumerable<Expression> expressions)
        => new(NodeList.From<Expression?>(expressions.Select(static expression => (Expression?)expression)));

    private static Expression NormalizeSlotRenderExpression(Expression expression)
        => VueSlotAstFactory.NormalizeContent(expression);

    private static Expression CreateFragment(IEnumerable<Expression> children)
        => Call("h", new Identifier("Fragment"), Null(), CreateArray(children));

    private static ObjectProperty CreateObjectProperty(string name, Expression value)
        => new(
            PropertyKind.Init,
            CreateObjectPropertyKey(name),
            value,
            computed: false,
            shorthand: false,
            method: false);

    private static Expression CreateObjectPropertyKey(string name)
        => JavaScriptAstFactory.IsJavaScriptIdentifierName(name)
            ? new Identifier(name)
            : StringLiteral(name);

    private static string NormalizeDirectElementAttributeName(string name)
    {
        if (string.Equals(name, "class", StringComparison.Ordinal))
            return "class";
        if (name.StartsWith("on", StringComparison.Ordinal) &&
            name.Length > 2 &&
            char.IsLower(name[2]))
        {
            return "on" + char.ToUpperInvariant(name[2]) + name.Substring(3);
        }
        if (name.StartsWith("on", StringComparison.Ordinal) &&
            name.Length > 2 &&
            char.IsUpper(name[2]))
        {
            return name;
        }

        return name;
    }

}

/// <summary>Vue render lowering output together with imports and source-origin metadata. Module builder 据此完成 setup/render framing。</summary>
/// <remarks>Hoist metadata is file-level so the module builder can materialize it before setup.</remarks>
internal sealed record RenderResult(
    Expression RenderExpression,
    ImmutableArray<Statement> PreludeStatements,
    ImmutableArray<RenderModuleHoist> ModuleHoists,
    bool UsesFragment,
    bool UsesStaticVNode,
    bool UsesBlockTree,
    bool UsesHandlerCache,
    bool UsesProps,
    bool UsesSlots,
    ImmutableArray<ImportDeclaration> ImportDeclarations,
    ImmutableArray<ISymbol> ReferenceCaptureStateMembers);

/// <summary>Represents an immutable expression allocated once at module scope.</summary>
internal sealed record RenderModuleHoist(
    string Name,
    Expression Initializer);
