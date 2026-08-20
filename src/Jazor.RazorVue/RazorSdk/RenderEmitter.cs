using System.Collections.Immutable;
using Acornima;
using Acornima.Ast;
using Jazor.Common;
using Jazor.Compiler;
using Jazor.RazorVue.Generation;
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
    private const string CascadingValueRuntimeModuleSpecifier = "@jazor/vue-runtime/cascading.mjs";
    private const string CascadingValueRuntimeExportName = "CascadingValue";
    private const string CascadingValueMetadataName = "Microsoft.AspNetCore.Components.CascadingValue`1";
    private const string CascadingValueTypePropName = "__jazorCascadeType";
    private const string BlazorRoutingRuntimeModuleSpecifier = "@jazor/vue-runtime/blazor-routing.mjs";
    private const string BlazorComponentsRuntimeModuleSpecifier = "@jazor/vue-runtime/blazor-components.mjs";
    private const string StandardInputValueTypePropName = "__jazorValueType";
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
        => TryEmit(
            compilation,
            componentSymbol,
            buildRenderTreeMethod,
            buildRenderTreeBody,
            declaredNames,
            reservedImportNames: null,
            injectRegistry,
            out result,
            out failure);

    public static bool TryEmit(
        Compilation compilation,
        INamedTypeSymbol componentSymbol,
        IMethodSymbol buildRenderTreeMethod,
        IBlockOperation buildRenderTreeBody,
        IReadOnlyDictionary<ISymbol, string>? declaredNames,
        IEnumerable<string>? reservedImportNames,
        VueInjectRegistry injectRegistry,
        out RenderResult result,
        out string? failure)
    {
        var emitted = TryEmitCore(
            compilation,
            componentSymbol,
            buildRenderTreeMethod,
            buildRenderTreeBody,
            declaredNames,
            reservedImportNames,
            injectRegistry,
            parameterPropertiesUseState: false,
            out result,
            out var exception);
        failure = exception?.Message;
        return emitted;
    }

    /// <summary>
    /// Lowers an inline <c>RenderFragment&lt;T&gt;</c> carried by a TDesign table-cell union into
    /// TDesign's <c>cell(h, params) =&gt; VNode</c> runtime contract. The surrounding computed
    /// member still belongs to <see cref="SemanticWalker"/>; this method only claims the
    /// RenderTreeBuilder protocol nested inside the known table-cell conversion.
    /// 普通成员中的表格 Cell 不是 BuildRenderTree 直通路径，但其内部仍是 Razor builder 协议；
    /// 这里复用同一 VNode lowering，不能把 builder 回调泄漏到最终 JS。
    /// </summary>
    internal static Expression? TryEmitTDesignTableCell(
        Compilation compilation,
        INamedTypeSymbol componentSymbol,
        IReadOnlyDictionary<ISymbol, string>? declaredNames,
        VueInjectRegistry injectRegistry,
        IConversionOperation operation,
        SenseArgument argument,
        VueRenderRuntimeFeatures runtimeFeatures)
    {
        if (!IsTDesignTableCellType(operation.Type))
            return null;

        var emitter = new Emitter(
            compilation,
            componentSymbol,
            declaredNames,
            reservedImportNames: null,
            injectRegistry,
            sharedArgument: argument);
        return emitter.TryEmitTDesignTableCell(operation, runtimeFeatures);
    }

    /// <summary>
    /// Production diagnostic seam for direct RenderTree lowering. It preserves the source-aware
    /// failure until the final compilation boundary reports it; the string overload remains a
    /// narrow compatibility seam for existing compiler-facing tests.
    /// 这里不能把异常先格式化成 string，否则 mapped Razor span 与 compiler/direct 类别都会丢失。
    /// </summary>
    internal static bool TryEmitWithDiagnostic(
        Compilation compilation,
        INamedTypeSymbol componentSymbol,
        IMethodSymbol buildRenderTreeMethod,
        IBlockOperation buildRenderTreeBody,
        IReadOnlyDictionary<ISymbol, string>? declaredNames,
        IEnumerable<string>? reservedImportNames,
        VueInjectRegistry injectRegistry,
        out RenderResult result,
        out RazorVueDiagnosticInfo? diagnostic,
        bool parameterPropertiesUseState = false)
    {
        var emitted = TryEmitCore(
            compilation,
            componentSymbol,
            buildRenderTreeMethod,
            buildRenderTreeBody,
            declaredNames,
            reservedImportNames,
            injectRegistry,
            parameterPropertiesUseState,
            out result,
            out var exception);
        if (emitted)
        {
            diagnostic = null;
            return true;
        }

        diagnostic = exception switch
        {
            RazorVueDiagnosticException typed => typed.Diagnostic.WithComponent(componentSymbol),
            OperationTransformationException operation => RazorVueDiagnosticFactory.Create(
                // Unwrapped operation failures originate from RenderEmitter's RenderTreeBuilder
                // protocol checks. SemanticWalker failures are wrapped above as CompilerBridge.
                // 这里保留 direct-render owner，不能把 builder 协议错误误标为 compiler failure。
                RazorVueDiagnosticCategory.DirectRender,
                operation.Message ?? "No direct render lowering detail was provided.",
                operation.SourceLocation,
                componentSymbol),
            SyntaxNodeTransformationException syntax => RazorVueDiagnosticFactory.FromException(
                syntax,
                RazorVueDiagnosticCategory.CompilerBridge,
                componentSymbol),
            SymbolTransformationException symbol => RazorVueDiagnosticFactory.FromException(
                symbol,
                RazorVueDiagnosticCategory.CompilerBridge,
                componentSymbol),
            _ => RazorVueDiagnosticFactory.Create(
                RazorVueDiagnosticCategory.DirectRender,
                exception?.Message ?? "No direct render lowering detail was provided.",
                RazorVueDiagnosticFactory.GetSymbolLocation(buildRenderTreeMethod),
                componentSymbol)
        };
        return false;
    }

    private static bool TryEmitCore(
        Compilation compilation,
        INamedTypeSymbol componentSymbol,
        IMethodSymbol buildRenderTreeMethod,
        IBlockOperation buildRenderTreeBody,
        IReadOnlyDictionary<ISymbol, string>? declaredNames,
        IEnumerable<string>? reservedImportNames,
        VueInjectRegistry injectRegistry,
        bool parameterPropertiesUseState,
        out RenderResult result,
        out Exception? failure)
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
                failure = new InvalidOperationException(
                    "RazorVue direct render operation lowering requires BuildRenderTree(RenderTreeBuilder).");
                return false;
            }

            var lowered = new Emitter(
                    compilation,
                    componentSymbol,
                    declaredNames,
                    reservedImportNames,
                    injectRegistry,
                    parameterPropertiesUseState)
                // A concrete component can inherit BuildRenderTree from a generic base. Roslyn
                // then supplies a constructed method symbol here, while the source body keeps
                // the base declaration's parameter symbol. Bind the operation body by the
                // original definition so its RenderTreeBuilder calls stay on the direct path.
                // 泛型基类上的构造方法与方法体参数符号不同，入口必须锚定原始定义。
                .EmitBlock(
                    buildRenderTreeBody,
                    BuilderBinding.ForSymbol(buildRenderTreeMethod.OriginalDefinition.Parameters[0]));
            result = new RenderResult(
                lowered.RenderExpression,
                lowered.PreludeStatements,
                lowered.ModuleHoists,
                UsesFragment: lowered.UsesFragment,
                UsesStaticVNode: lowered.UsesStaticVNode,
                UsesRawMarkupRuntime: lowered.UsesRawMarkupRuntime,
                UsesBlockTree: lowered.UsesBlockTree,
                UsesTextVNode: lowered.UsesTextVNode,
                UsesRenderList: lowered.UsesRenderList,
                UsesWithCtx: lowered.UsesWithCtx,
                UsesCreateSlots: lowered.UsesCreateSlots,
                UsesMergeProps: lowered.UsesMergeProps,
                UsesHandlerCache: lowered.UsesHandlerCache,
                UsesProps: AstReferenceAnalysis.ReferencesIdentifier(lowered.RenderExpression, "props") ||
                           lowered.PreludeStatements.Any(static statement => AstReferenceAnalysis.ReferencesIdentifier(statement, "props")),
                UsesSlots: lowered.UsesSlots,
                lowered.ImportDeclarations,
                lowered.ReferenceCaptureStateMembers);
            return true;
        }
        catch (RazorVueDiagnosticException exception)
        {
            failure = exception;
            return false;
        }
        catch (OperationTransformationException exception)
        {
            failure = exception;
            return false;
        }
        catch (SyntaxNodeTransformationException exception)
        {
            failure = exception;
            return false;
        }
        catch (SymbolTransformationException exception)
        {
            failure = exception;
            return false;
        }
        catch (InvalidOperationException exception)
        {
            failure = exception;
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
        private readonly Dictionary<Expression, DirectBinderValueKind> _directBinderHandlers =
            new(ReferenceExpressionComparer.Instance);
        private readonly Dictionary<ILocalSymbol, IOperation> _compileTimeFrameLocalValues = new(SymbolComparer);
        private readonly HashSet<ILocalSymbol> _erasedRenderObjectLocals = new(SymbolComparer);
        private readonly HashSet<ILocalSymbol> _mutableRenderLocals = new(SymbolComparer);
        private readonly HashSet<string> _renderLocalNames = new(StringComparer.Ordinal);
        private string? _componentAttributeNormalizerName;
        private bool _usesMergeProps;
        private bool _usesFragment;
        private bool _usesRawMarkupRuntime;
        private bool _usesSlots;
        private bool _usesBlockTree;
        private bool _usesTextVNode;
        private bool _usesRenderList;
        private bool _usesWithCtx;
        private bool _usesCreateSlots;
        private bool _usesHandlerCache;
        private int _nonHoistableRenderScopeDepth;
        private int _staticPropsHoistCount;
        private int _staticVNodeHoistCount;
        private int _handlerCacheCount;

        public Emitter(
            Compilation compilation,
            INamedTypeSymbol componentSymbol,
            IReadOnlyDictionary<ISymbol, string>? declaredNames,
            IEnumerable<string>? reservedImportNames,
            VueInjectRegistry injectRegistry,
            bool parameterPropertiesUseState = false,
            SenseArgument? sharedArgument = null)
        {
            _compilation = compilation;
            _componentSymbol = componentSymbol;
            // Direct RenderTree lowering also resolves authored runtime types (for example a
            // nested helper class constructed directly in BuildRenderTree). Give this walker the
            // same current-module root and declared-name map used by AstConverter; otherwise a
            // private nested type is mistaken for a flattened external module type and the final
            // artifact imports itself.
            // direct 路径同样必须共享当前模块类型/命名上下文，避免生成自模块 import。
            _walker = new SemanticWalker(
                componentSymbol,
                declaredNames ?? new Dictionary<ISymbol, string>(SymbolComparer))
            {
                Host = new VueSemanticWalkerHost(
                    componentSymbol,
                    parameterRuntimeNames: BuildComponentParameterNameMap(componentSymbol),
                    memberRuntimeNames: declaredNames,
                    parameterPropertiesUseState: parameterPropertiesUseState,
                    parameterReferenceRewriter: RewriteDirectParameterReference,
                    localReferenceRewriter: RewriteDirectLocalReference,
                    propertyReferenceRewriter: RewriteDirectRenderFragmentParameterReference,
                    directBinderHandlerObserver: (handler, valueKind) => _directBinderHandlers[handler] = valueKind)
            };
            var argument = sharedArgument?.WithNewScope() ?? new SenseArgument(Sense.Any, UseImportAliases: true);
            if (sharedArgument is null && reservedImportNames is not null)
            {
                // Direct render is lowered after ordinary component members but uses a separate
                // SemanticWalker. Seed it with compiler bindings so another module's same export
                // name receives the regular stable alias before AST references are created.
                // 不能在模块拼装阶段才改 import 名，否则 AST 已引用旧名字；别名必须在 lowering 时确定。
                argument = argument.WithImportContext(
                    new Dictionary<string, string>(StringComparer.Ordinal),
                    new Dictionary<string, string>(StringComparer.Ordinal),
                    new HashSet<string>(reservedImportNames, StringComparer.Ordinal),
                    currentModuleImportPath: null,
                    currentModuleBindings: new HashSet<string>(StringComparer.Ordinal));
            }
            _argument = argument;
            _componentSlotNames = BuildComponentSlotNameMap(componentSymbol);
            _injectRegistry = injectRegistry;
        }

        public Expression? TryEmitTDesignTableCell(
            IConversionOperation operation,
            VueRenderRuntimeFeatures runtimeFeatures)
        {
            if (!TryGetGenericRenderFragmentBody(operation.Operand, out var valueParameter, out var builder, out var body))
                return null;

            // TDesign invokes cell(h, params). `h` is intentionally ignored so the Vue framing
            // helper remains visible to nested direct component lowering; `context` is the C#
            // RenderFragment<T> value parameter and must be evaluated only at cell invocation.
            // TDesign 的首参不能命名为 h，否则会遮蔽片段内部 Vue h helper。
            const string renderFunctionParameterName = "__jazor$renderH";
            const string contextParameterName = "context";
            CollectMutableRenderLocals(body);
            var context = new EmitContext(
                BuilderBinding.ForSymbol(builder),
                ImmutableDictionary<IParameterSymbol, IOperation>.Empty.WithComparers(SymbolComparer),
                ImmutableDictionary<IParameterSymbol, string>.Empty
                    .WithComparers(SymbolComparer)
                    .SetItem(valueParameter, contextParameterName),
                ImmutableDictionary<ILocalSymbol, string>.Empty.WithComparers(SymbolComparer),
                ImmutableDictionary<ILocalSymbol, DirectRenderFragment>.Empty.WithComparers(SymbolComparer),
                ImmutableDictionary<ILocalSymbol, DirectRenderObject>.Empty.WithComparers(SymbolComparer),
                ImmutableDictionary<ILocalSymbol, INamedTypeSymbol>.Empty.WithComparers(SymbolComparer),
                ImmutableHashSet<ILocalSymbol>.Empty.WithComparer(SymbolComparer),
                new List<Statement>(),
                AllowPreludeDeclarations: true,
                Argument: _argument.WithNewScope());
            var lowered = EmitRenderFragmentBodyExpression(
                builder,
                body,
                context,
                operation,
                "TDesign table-cell RenderFragment<T> content");
            runtimeFeatures.Merge(
                usesMergeProps: _usesMergeProps,
                usesFragment: _usesFragment || lowered.UsesFragment,
                usesRawMarkupRuntime: _usesRawMarkupRuntime,
                usesSlots: _usesSlots,
                usesBlockTree: _usesBlockTree,
                usesTextVNode: _usesTextVNode,
                usesRenderList: _usesRenderList,
                usesWithCtx: _usesWithCtx,
                usesCreateSlots: _usesCreateSlots,
                usesHandlerCache: _usesHandlerCache,
                usesProps: AstReferenceAnalysis.ReferencesIdentifier(lowered.RenderExpression, "props"));
            return new ArrowFunctionExpression(
                NodeList.From<Node>(
                    new Identifier(renderFunctionParameterName),
                    new Identifier(contextParameterName)),
                lowered.RenderExpression,
                expression: true,
                async: false);
        }

        public LoweredRender EmitBlock(IBlockOperation block, BuilderBinding builder)
        {
            CollectMutableRenderLocals(block);
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
            // A previous branch can allocate a hoist and later be cleared/replaced. The module
            // import contract must reflect only retained output, otherwise Vue gets an unused
            // helper import and callers observe a false static-vnode capability.
            // 只从最终保留的 hoist 推导 helper；不能让已剪枝的分支污染 import/feature metadata。
            var usesStaticVNode = moduleHoists.Any(static hoist =>
                AstReferenceAnalysis.ReferencesIdentifier(hoist.Initializer, "createStaticVNode"));
            return new LoweredRender(
                renderExpression,
                _preludeStatements.ToImmutableArray(),
                moduleHoists,
                usesFragment,
                usesStaticVNode,
                _usesRawMarkupRuntime,
                _usesBlockTree,
                _usesTextVNode,
                _usesRenderList,
                _usesWithCtx,
                _usesCreateSlots,
                _usesMergeProps,
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

                case IForLoopOperation forLoop:
                    EmitForLoop(forLoop, context, state);
                    return context;

                case IWhileLoopOperation whileLoop:
                    EmitWhileLoop(whileLoop, context, state);
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
                        _mutableRenderLocals.Contains(declarator.Symbol)
                            ? VariableDeclarationKind.Let
                            : VariableDeclarationKind.Const,
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

            // Vue identifies sibling VNodes by type/key before it considers their static props.
            // Give each Razor branch a stable internal identity so an adjacent @if cannot reuse
            // a disappearing sibling's DOM/block with another branch's static shape.
            // Vue 会先按 type/key 判断 sibling 身份，再处理静态 props；为每个 Razor 分支生成
            // 稳定内部 key，避免相邻 @if 在前一分支消失、后一分支出现时错误复用 DOM/block。
            var whenTrue = EmitChildContentExpression(
                conditional.WhenTrue,
                context,
                CreateConditionalBranchKey(conditional, whenTrue: true));
            var whenFalse = conditional.WhenFalse is null
                ? new DirectRenderFragmentBody(Null(), UsesFragment: false)
                : EmitChildContentExpression(
                    conditional.WhenFalse,
                    context,
                    CreateConditionalBranchKey(conditional, whenTrue: false));
            state.AddChild(new ConditionalExpression(condition, whenTrue.RenderExpression, whenFalse.RenderExpression));
            state.UsesFragment = state.UsesFragment || whenTrue.UsesFragment || whenFalse.UsesFragment;

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

            if (frame is ComponentFrame componentFrame &&
                TryEmitConditionalComponentSlot(
                    componentFrame,
                    condition,
                    whenTrueInvocations,
                    whenFalseInvocations,
                    context,
                    state))
            {
                return true;
            }

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

        private bool TryEmitConditionalComponentSlot(
            ComponentFrame frame,
            Expression condition,
            ImmutableArray<IInvocationOperation> whenTrueInvocations,
            ImmutableArray<IInvocationOperation> whenFalseInvocations,
            EmitContext context,
            RenderState state)
        {
            // A conditional slot is representable only when each branch is one matching
            // component parameter (or intentionally absent). Mixed prop/slot branches retain
            // the existing attribute path so their source-order contract is not guessed.
            // 条件 slot 仅接收两支各一个同名参数（或显式缺席）；属性与 slot 混合时不猜测顺序。
            if (!TryResolveConditionalComponentSlotInvocation(
                    whenTrueInvocations,
                    context,
                    out var whenTrueName,
                    out var whenTrueFragment) ||
                !TryResolveConditionalComponentSlotInvocation(
                    whenFalseInvocations,
                    context,
                    out var whenFalseName,
                    out var whenFalseFragment) ||
                whenTrueName is null && whenFalseName is null ||
                whenTrueName is not null &&
                whenFalseName is not null &&
                !string.Equals(whenTrueName, whenFalseName, StringComparison.Ordinal) ||
                whenTrueFragment is { ParameterName: null } && whenFalseFragment is { ParameterName: not null } ||
                whenTrueFragment is { ParameterName: not null } && whenFalseFragment is { ParameterName: null })
            {
                return false;
            }

            var name = whenTrueName ?? whenFalseName!;
            var parameterName = whenTrueFragment?.ParameterName ?? whenFalseFragment?.ParameterName;
            var absent = new DirectRenderFragment(
                Null(),
                ParameterName: parameterName,
                AvailabilityCondition: new BooleanLiteral(false, "false"),
                RenderExpressionWhenAvailable: Null());
            var whenTrue = whenTrueFragment ?? absent;
            var whenFalse = whenFalseFragment ?? absent;
            var fragment = new DirectRenderFragment(
                new ConditionalExpression(condition, whenTrue.RenderExpression, whenFalse.RenderExpression),
                ParameterName: parameterName,
                UsesFragment: whenTrue.UsesFragment || whenFalse.UsesFragment,
                Selection: new ConditionalRenderFragmentSelection(condition, whenTrue, whenFalse));
            frame.Slots.Add(new DirectSlot(
                frame.TryGetDeclaredSlotName(name, out var declaredSlotName)
                    ? declaredSlotName
                    : frame.NormalizeSlotName(name),
                fragment));
            state.UsesFragment = state.UsesFragment || fragment.UsesFragment;
            return true;
        }

        private bool TryResolveConditionalComponentSlotInvocation(
            ImmutableArray<IInvocationOperation> invocations,
            EmitContext context,
            out string? name,
            out DirectRenderFragment? fragment)
        {
            name = null;
            fragment = null;
            if (invocations.Length == 0)
                return true;

            if (invocations.Length != 1)
                return false;

            var invocation = invocations[0];
            var methodName = invocation.TargetMethod.OriginalDefinition.Name;
            if (!string.Equals(methodName, "AddAttribute", StringComparison.Ordinal) &&
                !string.Equals(methodName, "AddComponentParameter", StringComparison.Ordinal) ||
                invocation.Arguments.Length != 3 ||
                !TryGetConstantString(invocation.Arguments[1].Value, out var parameterName) ||
                !TryResolveRenderFragmentContentExpression(invocation.Arguments[2].Value, context, out var renderFragment))
            {
                return false;
            }

            EnsureSignature(invocation, invocation.Arguments.Length == 3);
            RequireOmittableSequence(invocation.Arguments[0].Value);
            name = parameterName;
            fragment = renderFragment;
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
            // Vue render functions are synchronous. Do not let a branch-free await foreach
            // slip through the Array.from path merely because it has no loop control flow.
            // Vue render 函数是同步的；无 break/continue 的 await foreach 也不能误走 Array.from 路径。
            if (forEachLoop.IsAsynchronous)
            {
                throw Unsupported(
                    forEachLoop,
                    "Async foreach cannot execute inside Razor's synchronous BuildRenderTree contract.");
            }

            // Array.from maps lazily at render time and supplies each iteration its own alias
            // scope. Building children once outside the mapper would duplicate/collapse loop effects.
            // 循环体必须留在 mapper 内运行，不能在 lowering 时预先展开或共享局部变量。
            var collection = LowerForEachLoopCollection(forEachLoop, context);
            Node mapperParameter;
            EmitContext loopContext;
            var hasSimpleLoopVariable = TryResolveLoopControlVariable(forEachLoop.LoopControlVariable, out var loopVariable);
            if (hasSimpleLoopVariable)
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

            if (HasBranchTargetingLoop(forEachLoop.Body, forEachLoop))
            {
                try
                {
                    var resultName = CreateUniqueLocalName("__jazor$foreach");
                    var result = new Identifier(resultName);
                    var branchingBody = EmitBranchingLoopBody(forEachLoop, loopContext, result);
                    var left = new VariableDeclaration(
                        VariableDeclarationKind.Let,
                        NodeList.From(new VariableDeclarator(mapperParameter, null)));
                    var loop = new ForOfStatement(
                        left,
                        new LogicalExpression(Operator.NullishCoalescing, collection, CreateArray([])),
                        new NestedBlockStatement(NodeList.From(branchingBody.Statements)),
                        @await: false);
                    var children = BuildImperativeLoopChildren(result, loop);

                    _usesBlockTree = true;
                    _usesFragment = true;
                    state.UsesFragment = true;
                    state.AddChild(VNodePlan.Block(CreateForLoopFragmentExpression(
                        children,
                        branchingBody.HasExplicitRootKey
                            ? VuePatchFlags.KeyedFragment
                            : VuePatchFlags.UnkeyedFragment)));
                    return;
                }
                finally
                {
                    foreach (var name in loopContext.LocalAliases.Values.Except(context.LocalAliases.Values, StringComparer.Ordinal))
                        _renderLocalNames.Remove(name);
                }
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
            if (hasSimpleLoopVariable &&
                !body.UsesFragment &&
                body.DirectRoot is { } directRoot &&
                directRoot.CanUseRenderList)
            {
                // renderList must receive the original source: Vue supports arrays, iterables,
                // object records, and numeric ranges. `?? []` would silently change those
                // runtime contracts before Vue can apply its own list protocol.
                // 保留原 collection 给 Vue；不能用 Array.from 的 null fallback 改写对象/range 语义。
                _usesRenderList = true;
                _usesBlockTree = true;
                _usesFragment = true;
                state.UsesFragment = true;
                state.AddChild(VNodePlan.Block(CreateForEachFragmentExpression(
                    collection,
                    mapper,
                    directRoot.HasExplicitKey
                        ? VuePatchFlags.KeyedFragment
                        : VuePatchFlags.UnkeyedFragment)));
            }
            else
            {
                state.AddChild(Call(
                    new MemberExpression(new Identifier("Array"), new Identifier("from"), computed: false, optional: false),
                    new LogicalExpression(Operator.NullishCoalescing, collection, CreateArray([])),
                    mapper));
            }
            state.UsesFragment = state.UsesFragment || body.UsesFragment;
        }

        private void EmitForLoop(IForLoopOperation forLoop, EmitContext context, RenderState state)
        {
            // Razor SG keeps @for as IForLoopOperation instead of rewriting it to foreach.
            // Keep its control variable outside the JavaScript for initializer: C# closures
            // capture one loop variable, while `for (let i = ...)` creates one binding per turn.
            // Razor SG 会保留 @for 的 IForLoopOperation；控制变量放在 JS for initializer 外，
            // 才能维持 C# closure 捕获同一个循环变量的语义，而不是 JS let 的每轮新绑定。
            var loopArgument = context.Argument.WithNewScope();
            var loopAliases = context.LocalAliases;
            var loopLocalNames = new List<string>();
            foreach (var local in forLoop.Locals)
            {
                var localName = CreateUniqueLocalName(local.Name);
                loopAliases = loopAliases.SetItem(local, localName);
                loopLocalNames.Add(localName);
                _renderLocalNames.Add(localName);
            }

            var loopContext = context with
            {
                LocalAliases = loopAliases,
                Argument = loopArgument
            };
            try
            {
                var initializers = LowerForLoopInitializers(forLoop.Before, loopContext);
                var condition = forLoop.Condition is null
                    ? null
                    : LowerExpression(forLoop.Condition, loopContext);
                var update = LowerForLoopUpdates(forLoop.AtLoopBottom, loopContext);
                if (loopArgument.HasVarDeclarator)
                {
                    throw Unsupported(
                        forLoop,
                        "For direct render lowering does not support initializer, condition, or update expressions that require compiler temporary declarations.");
                }

                var resultName = CreateUniqueLocalName("__jazor$for");
                var result = new Identifier(resultName);
                Expression children;
                bool hasExplicitRootKey;
                if (HasBranchTargetingLoop(forLoop.Body, forLoop))
                {
                    var body = EmitBranchingLoopBody(forLoop, loopContext, result);
                    var loop = new ForStatement(
                        init: null,
                        test: condition,
                        update: update,
                        body: new NestedBlockStatement(NodeList.From(body.Statements)));
                    children = BuildImperativeLoopChildren(result, loop, initializers);
                    hasExplicitRootKey = body.HasExplicitRootKey;
                }
                else
                {
                    var body = EmitLoopIterationBody(forLoop.Body, loopContext);
                    var append = new NonSpecialExpressionStatement(Call(
                        new MemberExpression(result, new Identifier("push"), computed: false, optional: false),
                        body.RenderExpression));
                    var loop = new ForStatement(
                        init: null,
                        test: condition,
                        update: update,
                        body: new NestedBlockStatement(NodeList.From<Statement>(append)));
                    children = BuildImperativeLoopChildren(result, loop, initializers);
                    hasExplicitRootKey = body.HasExplicitRootKey;
                }

                _usesBlockTree = true;
                _usesFragment = true;
                state.UsesFragment = true;
                state.AddChild(VNodePlan.Block(CreateForLoopFragmentExpression(
                    children,
                    hasExplicitRootKey
                        ? VuePatchFlags.KeyedFragment
                        : VuePatchFlags.UnkeyedFragment)));
            }
            finally
            {
                foreach (var localName in loopLocalNames)
                    _renderLocalNames.Remove(localName);
            }
        }

        private ImmutableArray<Statement> LowerForLoopInitializers(
            ImmutableArray<IOperation> before,
            EmitContext context)
        {
            var statements = ImmutableArray.CreateBuilder<Statement>();
            foreach (var operation in before)
            {
                switch (operation)
                {
                    case IVariableDeclarationGroupOperation declarationGroup:
                    {
                        var declarations = new List<VariableDeclarator>();
                        foreach (var declaration in declarationGroup.Declarations)
                        {
                            foreach (var declarator in declaration.Declarators)
                            {
                                if (declarator.Initializer is null ||
                                    !context.LocalAliases.TryGetValue(declarator.Symbol, out var localName))
                                {
                                    throw Unsupported(
                                        declarator,
                                        "For direct render lowering requires initialized local control variables.");
                                }

                                declarations.Add(new VariableDeclarator(
                                    new Identifier(localName),
                                    LowerExpression(declarator.Initializer.Value, context)));
                            }
                        }

                        if (declarations.Count > 0)
                        {
                            statements.Add(new VariableDeclaration(
                                VariableDeclarationKind.Let,
                                NodeList.From(declarations)));
                        }

                        break;
                    }

                    case IExpressionStatementOperation expressionStatement:
                        statements.Add(new NonSpecialExpressionStatement(
                            LowerExpression(expressionStatement.Operation, context)));
                        break;

                    default:
                        throw Unsupported(
                            operation,
                            "For direct render lowering only supports local declarations or expressions in the initializer.");
                }
            }

            return statements.ToImmutable();
        }

        private Expression? LowerForLoopUpdates(
            ImmutableArray<IOperation> updates,
            EmitContext context)
        {
            if (updates.Length == 0)
                return null;

            var expressions = new List<Expression>(updates.Length);
            foreach (var update in updates)
            {
                var operation = update is IExpressionStatementOperation expressionStatement
                    ? expressionStatement.Operation
                    : update;
                expressions.Add(LowerExpression(operation, context));
            }

            return expressions.Count == 1
                ? expressions[0]
                : new SequenceExpression(NodeList.From(expressions));
        }

        private void EmitWhileLoop(IWhileLoopOperation whileLoop, EmitContext context, RenderState state)
        {
            var loopArgument = context.Argument.WithNewScope();
            var loopContext = context with { Argument = loopArgument };
            var condition = LowerExpression(whileLoop.Condition!, loopContext);
            if (loopArgument.HasVarDeclarator)
            {
                throw Unsupported(
                    whileLoop,
                    "While direct render lowering does not support conditions that require compiler temporary declarations.");
            }

            var resultName = CreateUniqueLocalName("__jazor$while");
            var result = new Identifier(resultName);
            bool hasExplicitRootKey;
            NestedBlockStatement loopBody;
            if (HasBranchTargetingLoop(whileLoop.Body, whileLoop))
            {
                var body = EmitBranchingLoopBody(whileLoop, loopContext, result);
                loopBody = new NestedBlockStatement(NodeList.From(body.Statements));
                hasExplicitRootKey = body.HasExplicitRootKey;
            }
            else
            {
                var body = EmitLoopIterationBody(whileLoop.Body, loopContext);
                var append = new NonSpecialExpressionStatement(Call(
                    new MemberExpression(result, new Identifier("push"), computed: false, optional: false),
                    body.RenderExpression));
                loopBody = new NestedBlockStatement(NodeList.From<Statement>(append));
                hasExplicitRootKey = body.HasExplicitRootKey;
            }
            Statement loop = whileLoop.ConditionIsTop
                ? new WhileStatement(condition, loopBody)
                : new DoWhileStatement(loopBody, condition);
            var children = BuildImperativeLoopChildren(result, loop);

            _usesBlockTree = true;
            _usesFragment = true;
            state.UsesFragment = true;
            state.AddChild(VNodePlan.Block(CreateForLoopFragmentExpression(
                children,
                hasExplicitRootKey
                    ? VuePatchFlags.KeyedFragment
                    : VuePatchFlags.UnkeyedFragment)));
        }

        private static Expression BuildImperativeLoopChildren(
            Identifier result,
            Statement loop,
            IEnumerable<Statement>? leadingStatements = null)
        {
            var statements = new List<Statement>();
            if (leadingStatements is not null)
                statements.AddRange(leadingStatements);
            statements.Add(new VariableDeclaration(
                VariableDeclarationKind.Const,
                NodeList.From(new VariableDeclarator(result, CreateArray([])))));
            statements.Add(loop);
            statements.Add(new ReturnStatement(result));
            return Call(
                new ArrowFunctionExpression(
                    NodeList.From<Node>(),
                    new FunctionBody(NodeList.From(statements), strict: true),
                    expression: false,
                    async: false),
                Array.Empty<Expression>());
        }

        private BranchingLoopBody EmitBranchingLoopBody(
            ILoopOperation loop,
            EmitContext context,
            Identifier result)
        {
            // A branch must be a child of the real JavaScript loop it controls. Materialize
            // completed builder segments into the result array between structured statements;
            // putting break/continue inside a vnode IIFE would target the wrong function scope.
            // 普通 branch 必须与真实 loop 同层，不能藏在 iteration expression/IIFE 内。
            CollectMutableRenderLocals(loop.Body);
            var statements = new List<Statement>();
            var argument = context.Argument.WithNewScope();
            var bodyContext = context with
            {
                PreludeStatements = statements,
                AllowPreludeDeclarations = true,
                Argument = argument,
                IsTerminated = false
            };
            var state = new RenderState();
            var facts = new BranchingLoopFacts();
            _ = EmitBranchingLoopOperation(
                loop.Body,
                loop,
                bodyContext,
                state,
                result,
                statements,
                facts);
            FlushBranchingLoopRenderState(loop.Body, state, result, statements, facts);

            if (argument.HasVarDeclarator)
            {
                statements.Insert(0, new VariableDeclaration(
                    VariableDeclarationKind.Let,
                    argument.FlushVarDeclarator()));
            }

            return new BranchingLoopBody(
                statements.ToImmutableArray(),
                facts.SawRenderedRoot && facts.AllRenderedRootsExplicitlyKeyed);
        }

        private EmitContext EmitBranchingLoopOperation(
            IOperation operation,
            ILoopOperation ownerLoop,
            EmitContext context,
            RenderState state,
            Identifier result,
            List<Statement> statements,
            BranchingLoopFacts facts)
        {
            if (context.IsTerminated)
                return context;

            if (operation is IBlockOperation block)
            {
                foreach (var child in block.Operations)
                {
                    context = EmitBranchingLoopOperation(
                        child,
                        ownerLoop,
                        context,
                        state,
                        result,
                        statements,
                        facts);
                    if (context.IsTerminated)
                        break;
                }

                return context;
            }

            if (operation is IBranchOperation branch && IsBranchTargetingLoop(branch, ownerLoop))
            {
                FlushBranchingLoopRenderState(operation, state, result, statements, facts);
                statements.Add(LowerLoopBranch(branch, context));
                return context with { IsTerminated = true };
            }

            if (operation is IConditionalOperation conditional &&
                HasBranchTargetingLoop(conditional, ownerLoop))
            {
                if (state.Stack.Count != 0)
                {
                    throw Unsupported(
                        conditional,
                        "Loop break/continue cannot leave an open RenderTreeBuilder frame. Close the element, component, or region before branching.");
                }

                FlushBranchingLoopRenderState(operation, state, result, statements, facts);
                var condition = LowerExpression(conditional.Condition, context);
                var whenTrue = EmitBranchingLoopBranch(
                    conditional.WhenTrue,
                    ownerLoop,
                    context,
                    result,
                    facts);
                NestedBlockStatement? whenFalse = null;
                if (conditional.WhenFalse is not null)
                {
                    whenFalse = EmitBranchingLoopBranch(
                        conditional.WhenFalse,
                        ownerLoop,
                        context,
                        result,
                        facts);
                }

                statements.Add(new IfStatement(condition, whenTrue, whenFalse));
                return context;
            }

            if (operation is IExpressionStatementOperation expressionStatement &&
                IsLoopSideEffectOperation(operation))
            {
                if (state.Stack.Count != 0)
                {
                    throw Unsupported(
                        operation,
                        "An ordinary loop side effect cannot be moved across an open RenderTreeBuilder frame. Complete the frame before the statement.");
                }

                FlushBranchingLoopRenderState(operation, state, result, statements, facts);
                statements.Add(new NonSpecialExpressionStatement(
                    LowerExpression(expressionStatement.Operation, context)));
                return context;
            }

            // Once a root is complete, evaluate it before the next ordinary statement. This lets
            // helper calls and assignments stay in source order without delaying their effects
            // until a later vnode expression happens to run.
            if (state.Stack.Count == 0 && state.Roots.Count > 0)
                FlushBranchingLoopRenderState(operation, state, result, statements, facts);

            return EmitOperation(operation, context, state);
        }

        private NestedBlockStatement EmitBranchingLoopBranch(
            IOperation operation,
            ILoopOperation ownerLoop,
            EmitContext context,
            Identifier result,
            BranchingLoopFacts facts)
        {
            var statements = new List<Statement>();
            var state = new RenderState();
            var branchContext = context with
            {
                PreludeStatements = statements,
                AllowPreludeDeclarations = true,
                IsTerminated = false
            };
            _ = EmitBranchingLoopOperation(
                operation,
                ownerLoop,
                branchContext,
                state,
                result,
                statements,
                facts);
            FlushBranchingLoopRenderState(operation, state, result, statements, facts);
            return new NestedBlockStatement(NodeList.From(statements));
        }

        private static void FlushBranchingLoopRenderState(
            IOperation operation,
            RenderState state,
            Identifier result,
            List<Statement> statements,
            BranchingLoopFacts facts)
        {
            if (state.Stack.Count != 0)
            {
                throw Unsupported(
                    operation,
                    "Loop control flow left an open " + state.Stack.Peek().Describe() + " frame.");
            }

            if (state.Roots.Count > 0)
            {
                facts.SawRenderedRoot = true;
                facts.AllRenderedRootsExplicitlyKeyed =
                    facts.AllRenderedRootsExplicitlyKeyed &&
                    state.Roots.Count == 1 &&
                    state.Roots[0].HasExplicitKey;
                var content = VueSlotAstFactory.NormalizeContent(state.ToRenderExpression());
                statements.Add(new NonSpecialExpressionStatement(Call(
                    new MemberExpression(result, new Identifier("push"), computed: false, optional: false),
                    new SpreadElement(content))));
            }
            else if (state.PendingPreludeStatements.Count > 0)
            {
                statements.AddRange(state.PendingPreludeStatements);
            }

            state.Clear();
        }

        private Statement LowerLoopBranch(IBranchOperation operation, EmitContext context)
        {
            var previousContext = _activeExpressionContext;
            _activeExpressionContext = context;
            try
            {
                var statement = _walker.Visit(operation, context.Argument);
                if (statement is BreakStatement or ContinueStatement)
                    return (Statement)statement;

                throw Unsupported(operation, "Only ordinary break/continue can target a direct-render loop.");
            }
            finally
            {
                _activeExpressionContext = previousContext;
            }
        }

        private static bool HasBranchTargetingLoop(IOperation operation, ILoopOperation loop)
        {
            if (operation is IBranchOperation branch && IsBranchTargetingLoop(branch, loop))
                return true;

            return operation.Descendants()
                .OfType<IBranchOperation>()
                .Any(branchOperation => IsBranchTargetingLoop(branchOperation, loop));
        }

        private static bool IsBranchTargetingLoop(IBranchOperation branch, ILoopOperation loop)
            => branch.BranchKind switch
            {
                BranchKind.Break => SymbolComparer.Equals(branch.Target, loop.ExitLabel),
                BranchKind.Continue => SymbolComparer.Equals(branch.Target, loop.ContinueLabel),
                _ => false
            };

        private DirectRenderFragmentBody EmitLoopIterationBody(IOperation operation, EmitContext context)
        {
            // Iteration content is non-hoistable by contract, so it can never introduce a
            // module-static VNode. Only its fragment/key facts need to return to the parent.
            // 循环体禁止 module hoist，因此只回传 Fragment/key，不传播 static-vnode 状态。
            CollectMutableRenderLocals(operation);
            if (operation is not IBlockOperation block)
                return EmitNonHoistableChildContentExpression(operation, context);

            var operations = block.Operations;
            var firstRender = -1;
            var lastRender = -1;
            for (var index = 0; index < operations.Length; index++)
            {
                if (IsLoopSideEffectOperation(operations[index]))
                    continue;

                firstRender = firstRender < 0 ? index : firstRender;
                lastRender = index;
            }

            if (firstRender < 0)
            {
                throw Unsupported(
                    operation,
                    "Loop direct render lowering requires RenderTreeBuilder content in the loop body.");
            }

            for (var index = firstRender; index <= lastRender; index++)
            {
                if (IsLoopSideEffectOperation(operations[index]))
                {
                    throw Unsupported(
                        operations[index],
                        "Loop direct render lowering only supports ordinary statements before or after a complete RenderTreeBuilder content segment.");
                }
            }

            if (firstRender == 0 && lastRender == operations.Length - 1)
                return EmitNonHoistableChildContentExpression(operation, context);

            var iterationArgument = context.Argument.WithNewScope();
            var iterationContext = context with { Argument = iterationArgument };
            var leading = LowerLoopSideEffectStatements(
                operations.Take(firstRender).ToImmutableArray(),
                iterationContext);
            var rendered = EmitNonHoistableRenderOperations(
                operations.Skip(firstRender).Take(lastRender - firstRender + 1).ToImmutableArray(),
                operation,
                iterationContext);
            var trailing = LowerLoopSideEffectStatements(
                operations.Skip(lastRender + 1).ToImmutableArray(),
                iterationContext);
            var vnodeName = CreateUniqueLocalName("__jazor$loopVNode");
            var statements = new List<Statement>(leading.Length + trailing.Length + 3);
            if (iterationArgument.HasVarDeclarator)
            {
                statements.Add(new VariableDeclaration(
                    VariableDeclarationKind.Let,
                    iterationArgument.FlushVarDeclarator()));
            }

            statements.AddRange(leading);
            statements.Add(new VariableDeclaration(
                VariableDeclarationKind.Const,
                NodeList.From(new VariableDeclarator(
                    new Identifier(vnodeName),
                    rendered.RenderExpression))));
            statements.AddRange(trailing);
            statements.Add(new ReturnStatement(new Identifier(vnodeName)));
            // The IIFE keeps iteration side effects ordered, but it does not change the VNode
            // returned to Vue. Preserve only the root-key fact for Fragment diff selection;
            // DirectRoot remains null so this wrapper never becomes eligible for renderList.
            // IIFE 只负责副作用顺序；保留 key 事实用于 Fragment diff，但不能借此误开 renderList。
            return new DirectRenderFragmentBody(
                Call(
                    new ArrowFunctionExpression(
                        NodeList.From<Node>(),
                        new FunctionBody(NodeList.From(statements), strict: true),
                        expression: false,
                        async: false),
                    Array.Empty<Expression>()),
                rendered.UsesFragment,
                DirectRoot: null,
                HasExplicitRootKey: rendered.HasExplicitRootKey);
        }

        private DirectRenderFragmentBody EmitNonHoistableRenderOperations(
            ImmutableArray<IOperation> operations,
            IOperation sourceOperation,
            EmitContext context)
        {
            _nonHoistableRenderScopeDepth++;
            try
            {
                return WithScopedLocalNames(() =>
                {
                    var state = new RenderState();
                    var preludeStatements = new List<Statement>();
                    var argument = context.Argument.WithNewScope();
                    _ = EmitOperations(operations, context with
                    {
                        PreludeStatements = preludeStatements,
                        AllowPreludeDeclarations = true,
                        Argument = argument
                    }, state);
                    if (state.Stack.Count != 0)
                    {
                        throw Unsupported(
                            sourceOperation,
                            "Loop render content left unclosed " + state.Stack.Peek().Describe() + " frames.");
                    }

                    var hasDirectRoot = preludeStatements.Count == 0 &&
                                        !argument.HasVarDeclarator &&
                                        state.Roots.Count == 1 &&
                                        state.Roots[0].IsDirectVNodeRoot;
                    // renderList needs the stricter direct-root proof, while Fragment diffing
                    // only needs to know whether the single produced VNode carries a user key.
                    // IIFE/opaque lowering may intentionally lose the former without losing the latter.
                    // renderList 与 Fragment keyed diff 的证据不同，不能让 IIFE 包装抹掉根 key 事实。
                    var hasSingleExplicitlyKeyedVNode = state.Roots.Count == 1 &&
                                                         state.Roots[0].HasExplicitKey;
                    return new DirectRenderFragmentBody(
                        WrapWithExpressionScope(argument, preludeStatements, state.ToRenderExpression()),
                        state.UsesFragment || state.Roots.Count > 1,
                        hasDirectRoot ? state.Roots[0] : null,
                        HasExplicitRootKey: hasSingleExplicitlyKeyedVNode);
                });
            }
            finally
            {
                _nonHoistableRenderScopeDepth--;
            }
        }

        private ImmutableArray<Statement> LowerLoopSideEffectStatements(
            ImmutableArray<IOperation> operations,
            EmitContext context)
        {
            var statements = ImmutableArray.CreateBuilder<Statement>(operations.Length);
            foreach (var operation in operations)
            {
                if (operation is not IExpressionStatementOperation expressionStatement ||
                    !IsLoopSideEffectOperation(operation))
                {
                    throw Unsupported(
                        operation,
                        "Loop direct render lowering expected an ordinary expression statement outside RenderTreeBuilder content.");
                }

                statements.Add(new NonSpecialExpressionStatement(
                    LowerExpression(expressionStatement.Operation, context)));
            }

            return statements.ToImmutable();
        }

        private static bool IsLoopSideEffectOperation(IOperation operation)
        {
            if (operation is not IExpressionStatementOperation expressionStatement)
                return false;

            var expression = expressionStatement.Operation;
            while (expression is IConversionOperation conversion)
                expression = conversion.Operand;
            return expression is not IInvocationOperation;
        }

        private void CollectMutableRenderLocals(IOperation operation)
        {
            TrackMutableRenderLocal(operation);
            foreach (var descendant in operation.Descendants())
                TrackMutableRenderLocal(descendant);
        }

        private void TrackMutableRenderLocal(IOperation operation)
        {
            var target = operation switch
            {
                ISimpleAssignmentOperation assignment => assignment.Target,
                ICompoundAssignmentOperation assignment => assignment.Target,
                ICoalesceAssignmentOperation assignment => assignment.Target,
                IIncrementOrDecrementOperation increment => increment.Target,
                _ => null
            };
            if (target is not null && TryGetAssignedLocal(target, out var local))
                _mutableRenderLocals.Add(local);
        }

        private static bool TryGetAssignedLocal(IOperation operation, out ILocalSymbol local)
        {
            while (operation is IConversionOperation conversion)
                operation = conversion.Operand;

            if (operation is ILocalReferenceOperation localReference)
            {
                local = localReference.Local;
                return true;
            }

            local = null!;
            return false;
        }

        private static Expression CreateForEachFragmentExpression(
            Expression collection,
            ArrowFunctionExpression mapper,
            int fragmentFlag)
        {
            // openBlock(true) isolates dynamic children collected by renderList from the parent
            // block. This is the Vue compiler's v-for protocol and prevents inner VNodes from
            // being accidentally collected by an enclosing element block.
            // disableTracking 必须为 true，确保 list children 只归 fragment 管理。
            return new SequenceExpression(NodeList.From<Expression>(
                Call("openBlock", new BooleanLiteral(true, "true")),
                Call(
                    "createElementBlock",
                    new Identifier("Fragment"),
                    Null(),
                    Call("renderList", collection, mapper),
                new NumericLiteral(
                        fragmentFlag,
                        fragmentFlag.ToString(System.Globalization.CultureInfo.InvariantCulture)))));
        }

        private static Expression CreateForLoopFragmentExpression(
            Expression children,
            int fragmentFlag)
        {
            // The loop builds one array per render. Frame it as Vue's dynamic fragment so its
            // children stay isolated from the parent block just like renderList output.
            // @for 每次 render 生成一个数组；同样使用 Vue dynamic Fragment 隔离其 children，
            // 避免被父级 block 错误收集。
            return new SequenceExpression(NodeList.From<Expression>(
                Call("openBlock", new BooleanLiteral(true, "true")),
                Call(
                    "createElementBlock",
                    new Identifier("Fragment"),
                    Null(),
                    children,
                    new NumericLiteral(
                        fragmentFlag,
                        fragmentFlag.ToString(System.Globalization.CultureInfo.InvariantCulture)))));
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

        private Expression LowerForEachLoopCollection(IForEachLoopOperation operation, EmitContext context)
        {
            var previousContext = _activeExpressionContext;
            _activeExpressionContext = context;
            try
            {
                return _walker.BuildForEachLoopCollection(operation, context.Argument);
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

        private DirectRenderFragmentBody EmitChildContentExpression(
            IOperation operation,
            EmitContext context,
            string? implicitRootKey = null)
        {
            return WithScopedLocalNames(() =>
            {
                var childState = new RenderState(implicitRootKey);
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

                var hasDirectRoot = preludeStatements.Count == 0 &&
                                    !childArgument.HasVarDeclarator &&
                                    childState.Roots.Count == 1 &&
                                    childState.Roots[0].IsDirectVNodeRoot;
                // A loop body can be an opaque h() result when local side effects prevent
                // block collection. Its explicit key still defines Fragment identity.
                // loop body 即使因局部副作用降级为 opaque h()，显式 key 仍决定 Fragment identity。
                var hasSingleExplicitlyKeyedVNode = childState.Roots.Count == 1 &&
                                                     childState.Roots[0].HasExplicitKey;
                return new DirectRenderFragmentBody(
                    WrapWithExpressionScope(childArgument, preludeStatements, childState.ToRenderExpression()),
                    childState.UsesFragment,
                    hasDirectRoot ? childState.Roots[0] : null,
                    HasExplicitRootKey: hasSingleExplicitlyKeyedVNode);
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
                        usesFragment);
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
                        UseBlockTree,
                        UseTextVNode));
                    return true;

                case "CloseElement":
                    EnsureSignature(invocation, method.Parameters.Length == 0);
                    state.Close<ElementFrame>(invocation);
                    return true;

                case "OpenComponent":
                    EnsureSignature(invocation, method.Name == "OpenComponent");
                    RequireOmittableSequence(invocation.Arguments[0].Value);
                    var componentType = ResolveOpenComponentType(invocation, context);
                    var isCascadingValue = IsCascadingValueComponent(componentType);
                    var hasStandardAdapter = TryGetStandardBlazorComponentAdapter(componentType, out var standardAdapter);
                    var runtimeComponentType = _injectRegistry.ResolveImplementation(componentType);
                    var componentExpression = isCascadingValue
                        ? _argument.BindImportSpecifier(
                            CascadingValueRuntimeModuleSpecifier,
                            CascadingValueRuntimeExportName)
                        : hasStandardAdapter
                            ? _argument.BindImportSpecifier(
                                standardAdapter.ModuleSpecifier,
                                standardAdapter.ExportName)
                            : BindComponentImport(componentType);
                    var parameterNameMap = isCascadingValue
                        ? BuildCascadingValueParameterNameMap()
                        : BuildComponentParameterNameMap(runtimeComponentType);
                    var slotNameMap = isCascadingValue
                        ? BuildCascadingValueSlotNameMap()
                        : BuildComponentSlotParameterNameMap(runtimeComponentType);
                    state.StartChildren();
                    var componentFrame = new ComponentFrame(
                        componentExpression,
                        parameterNameMap,
                        slotNameMap,
                        HoistStaticProps,
                        CanHoistStaticProps,
                        CacheStableEventHandler,
                        CanCacheStableEventHandler,
                        handler => IsStableEventHandler(handler, context),
                        UseBlockTree,
                        UseWithCtx,
                        UseCreateSlots,
                        slotsAreInStableScope: _nonHoistableRenderScopeDepth == 0,
                        isCascadingValue: isCascadingValue,
                        standardAdapterKind: hasStandardAdapter
                            ? standardAdapter.Kind
                            : StandardBlazorComponentAdapterKind.None,
                        standardValueType: hasStandardAdapter
                            ? GetStandardBlazorComponentValueType(componentType)
                            : null);
                    if (hasStandardAdapter &&
                        TryBuildStandardInputValueTypeDescriptor(
                            standardAdapter.Kind,
                            componentFrame.StandardValueType,
                            out var standardInputValueTypeDescriptor))
                    {
                        // The closed InputBase<T> type is available when Razor opens the
                        // component even if TypeInference later presents Value as an open T.
                        // Install this framework-only prop at frame creation so descriptor
                        // emission does not depend on the generated helper's local symbols.
                        componentFrame.SetStandardInputValueTypeDescriptor(standardInputValueTypeDescriptor);
                    }
                    state.Stack.Push(componentFrame);
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
                    if (TryCreateStaticMarkupVNode(
                            invocation.Arguments[1].Value,
                            allowRawStringLiteral: true,
                            out var staticVNode))
                    {
                        if (staticVNode is not NullLiteral)
                            state.AddStaticChild(staticVNode);
                    }
                    else
                    {
                        state.AddOptionalChild(CreateRawMarkupContent(
                            LowerExpression(invocation.Arguments[1].Value, context)));
                    }
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
            if (!SymbolComparer.Equals(method.ContainingType!.OriginalDefinition, _componentSymbol.OriginalDefinition) &&
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
            if (syntax is not MethodDeclarationSyntax methodDeclaration)
                return false;

            var model = _compilation.GetSemanticModel(methodDeclaration.SyntaxTree);
            var helperOperation = methodDeclaration.Body is not null
                ? model.GetOperation(methodDeclaration.Body)
                : methodDeclaration.ExpressionBody is null
                    ? null
                    : model.GetOperation(methodDeclaration.ExpressionBody.Expression);
            if (helperOperation is null)
                return false;

            var substitutions = context.Substitutions.ToBuilder();
            for (var index = 1; index < invocation.Arguments.Length && index < method.Parameters.Length; index++)
                AddParameterSubstitution(substitutions, method, index, invocation.Arguments[index].Value);

            var helperContext = new EmitContext(
                // Generic Razor TypeInference calls are bound to a constructed method, while
                // the method body operation references OriginalDefinition parameter symbols.
                // Bind the body to that stable symbol identity so nested builder calls remain
                // in the direct-render protocol instead of falling through to raw C# lowering.
                // 泛型构造调用与方法体参数符号不同，必须统一到 OriginalDefinition。
                BuilderBinding.ForSymbol(method.OriginalDefinition.Parameters[0]),
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
            if (helperOperation is IBlockOperation body)
                _ = EmitOperations(body.Operations, helperContext, state);
            else
                _ = EmitOperation(helperOperation, helperContext, state);
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

            if (frame is ComponentFrame standardComponentFrame &&
                invocation.Arguments.Length == 3 &&
                TryEmitStandardBlazorComponentParameter(
                    standardComponentFrame,
                    name,
                    invocation.Arguments[2].Value,
                    context))
            {
                return;
            }

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
            frame.AddAttribute(new DirectAttribute(
                frame.NormalizeAttributeName(name),
                value,
                DirectBinderValueKind: _directBinderHandlers.TryGetValue(value, out var directBinderValueKind)
                    ? directBinderValueKind
                    : DirectBinderValueKind.None));
        }

        private void EmitAddComponentParameter(IInvocationOperation invocation, EmitContext context, RenderState state)
        {
            EnsureSignature(invocation, invocation.Arguments.Length == 3);
            RequireOmittableSequence(invocation.Arguments[0].Value);
            if (state.Stack.Count == 0 || state.Stack.Peek() is not ComponentFrame frame || frame.ChildrenStarted)
                throw Unsupported(invocation, "Component parameters must be added before component children.");

            if (!TryGetConstantString(invocation.Arguments[1].Value, out var name))
                throw Unsupported(invocation.Arguments[1].Value, "Component parameter names must be compile-time strings for direct render lowering.");

            if (TryEmitStandardBlazorComponentParameter(
                    frame,
                    name,
                    invocation.Arguments[2].Value,
                    context))
            {
                return;
            }

            if (frame.IsCascadingValue && string.Equals(name, "Value", StringComparison.Ordinal))
            {
                // Razor SG's TypeInference helper keeps CascadingValue<TValue> open even when
                // the authored Value expression is concrete. The expression operation carries
                // the actual source type, which is the same type Blazor uses for cascade lookup.
                // TypeInference 的 TValue 是开放泛型，真实 Value 表达式类型才是级联 key 来源。
                var valueType = GetCascadingValueType(invocation.Arguments[2].Value, context);
                frame.SetCascadingValueTypeKey(LibraryComponentConventions.GetCascadingTypeKey(valueType));
            }

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

        private bool TryEmitStandardBlazorComponentParameter(
            ComponentFrame frame,
            string name,
            IOperation valueOperation,
            EmitContext context)
        {
            if (frame.StandardAdapterKind == StandardBlazorComponentAdapterKind.None)
                return false;

            // Router discovers its pages from the final generated route catalog. AppAssembly is
            // still required by the standard Razor API, but it is intentionally not materialized
            // as a browser reflection object.
            // Router 页面从最终 catalog 发现；AppAssembly 保留标准作者面但不进入浏览器反射路径。
            if (frame.StandardAdapterKind == StandardBlazorComponentAdapterKind.Router &&
                string.Equals(name, "AppAssembly", StringComparison.Ordinal))
            {
                return true;
            }

            if (frame.StandardAdapterKind == StandardBlazorComponentAdapterKind.DynamicComponent &&
                string.Equals(name, "Type", StringComparison.Ordinal))
            {
                if (!TryResolveTypeOfExpression(valueOperation, context.LocalComponentTypes, out var targetComponent))
                {
                    throw Unsupported(
                        valueOperation,
                        "DynamicComponent.Type must be a statically discoverable RazorVue component type. " +
                        "Use typeof(MyComponent) or a local initialized from typeof(MyComponent).");
                }

                frame.AddAttribute(new DirectAttribute(
                    "__jazorComponent",
                    BindComponentImport(targetComponent)));
                return true;
            }

            if ((frame.StandardAdapterKind == StandardBlazorComponentAdapterKind.RouteView &&
                 string.Equals(name, "DefaultLayout", StringComparison.Ordinal)) ||
                (frame.StandardAdapterKind == StandardBlazorComponentAdapterKind.LayoutView &&
                 string.Equals(name, "Layout", StringComparison.Ordinal)))
            {
                if (!TryResolveTypeOfExpression(valueOperation, context.LocalComponentTypes, out var layoutType))
                {
                    throw Unsupported(
                        valueOperation,
                        name + " must be a statically discoverable RazorVue layout component type.");
                }

                frame.AddAttribute(new DirectAttribute(
                    frame.StandardAdapterKind == StandardBlazorComponentAdapterKind.RouteView
                        ? "__jazorDefaultLayout"
                        : "__jazorLayout",
                    BindComponentImport(layoutType)));
                return true;
            }

            if (IsStandardInputAdapter(frame.StandardAdapterKind) &&
                string.Equals(name, "ValueExpression", StringComparison.Ordinal))
            {
                // The input adapter receives the strongly typed Value/ValueChanged pair. Its
                // browser parse path has no CLR Expression<TDelegate> reflection counterpart.
                // Input adapter 使用 Value/ValueChanged；ValueExpression 不应落入浏览器反射。
                return true;
            }

            if (IsStandardInputAdapter(frame.StandardAdapterKind) &&
                string.Equals(name, "Value", StringComparison.Ordinal) &&
                TryBuildStandardInputValueTypeDescriptor(
                    frame.StandardAdapterKind,
                    valueOperation.Type is { TypeKind: not TypeKind.TypeParameter }
                        ? valueOperation.Type
                        : frame.StandardValueType,
                    out var valueTypeDescriptor))
            {
                // InputBase<T> normally uses Expression<T> and reflection to recover the
                // parser. The browser adapter cannot materialize that server-side expression,
                // so carry only the closed, compiler-known value contract as a hidden prop.
                // 运行时只需要闭合后的值域描述，不把 Expression<T> 或反射对象带入浏览器。
                frame.SetStandardInputValueTypeDescriptor(valueTypeDescriptor);
            }

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

        private void UseTextVNode()
            => _usesTextVNode = true;

        private void UseWithCtx()
            => _usesWithCtx = true;

        private void UseCreateSlots()
            => _usesCreateSlots = true;

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

                // The guard above establishes the key union. Keep the projection explicit so an
                // impossible third arm cannot become a coverage-only fallback or hide a bad AST shape.
                var name = property.Key is Identifier identifier
                    ? identifier.Name
                    : ((Acornima.Ast.StringLiteral)property.Key).Value;
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

            // Empty raw HTML is zero Razor content, not a Static vnode with staticCount 0.
            // Vue cannot mount/unmount an empty Static vnode because it has neither el nor anchor.
            // 空 MarkupString 必须直接省略；不能生成 createStaticVNode("", 0) 作为伪子节点。
            if (markup.Length == 0)
            {
                vnode = Null();
                return true;
            }

            var analysis = VueRawMarkup.AnalyzeStatic(markup);
            if (analysis.NodeCount == 0)
            {
                vnode = Null();
                return true;
            }

            if (!analysis.CanHydrateAsStaticVNode)
            {
                // A comment-first static fragment is rare, but Vue Static hydration cannot
                // adopt it. Keep static input at module scope while delegating only the VNode
                // framing to the shared runtime; no C# expression is re-evaluated here.
                // leading comment 走共享 runtime，避免错误 staticCount 导致 sibling 脱位。
                var runtimeName = "__jazor$hoistedRawMarkup" +
                    _staticVNodeHoistCount.ToString(System.Globalization.CultureInfo.InvariantCulture);
                _staticVNodeHoistCount++;
                _usesRawMarkupRuntime = true;
                _moduleHoists.Add(new RenderModuleHoist(
                    runtimeName,
                    Call(VueRawMarkup.CreateRawMarkupName, StringLiteral(markup))));
                vnode = new Identifier(runtimeName);
                return true;
            }

            var name = "__jazor$hoistedStatic" +
                _staticVNodeHoistCount.ToString(System.Globalization.CultureInfo.InvariantCulture);
            _staticVNodeHoistCount++;
            _moduleHoists.Add(new RenderModuleHoist(
                name,
                Call(
                    "createStaticVNode",
                    StringLiteral(markup),
                    new NumericLiteral(
                        analysis.NodeCount,
                        analysis.NodeCount.ToString(System.Globalization.CultureInfo.InvariantCulture)))));
            vnode = new Identifier(name);
            return true;
        }

        private Expression CreateRawMarkupContent(Expression markup)
        {
            // The source expression is deliberately passed once. The helper owns only Vue's
            // DOM-cardinality/comment framing; normal C# conversion/member/call lowering has
            // already happened before this point.
            // 参数必须单次传入 helper，不能为 count 再求值一次 MarkupString getter/call。
            _usesRawMarkupRuntime = true;
            return Call(VueRawMarkup.CreateRawMarkupName, markup);
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

            frame.SetExplicitVNodeKey(LowerExpression(invocation.Arguments[0].Value, context));
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
                return;
            }

            if (TryResolveRenderFragmentContentExpression(invocation.Arguments[1].Value, context, out var slotExpression))
            {
                if (slotExpression.ReturnsVueSlotContent)
                    state.AddChildSequence(slotExpression.RenderExpression);
                else
                    state.AddChild(slotExpression.RenderExpression);
                state.UsesFragment = state.UsesFragment || slotExpression.UsesFragment;
                return;
            }

            if (IsRenderFragmentOperationValue(invocation.Arguments[1].Value) ||
                IsGenericRenderFragmentOperationValue(invocation.Arguments[1].Value))
            {
                throw Unsupported(invocation.Arguments[1].Value, "RenderFragment content requires a resolvable inline, local, helper, or component-slot source.");
            }

            if (IsMarkupStringOperationValue(invocation.Arguments[1].Value))
            {
                if (IsNullableMarkupStringOperationValue(invocation.Arguments[1].Value))
                {
                    _usesRawMarkupRuntime = true;
                    state.AddOptionalChild(BuildNullableMarkupContent(
                        LowerMarkupStringExpression(invocation.Arguments[1].Value, context)));
                    return;
                }

                if (TryCreateStaticMarkupVNode(
                        invocation.Arguments[1].Value,
                        allowRawStringLiteral: false,
                        out var staticVNode))
                {
                    if (staticVNode is not NullLiteral)
                        state.AddStaticChild(staticVNode);
                }
                else
                {
                    state.AddOptionalChild(CreateRawMarkupContent(
                        LowerMarkupStringExpression(invocation.Arguments[1].Value, context)));
                }
                return;
            }

            var textOperation = invocation.Arguments[1].Value;
            var textExpression = LowerExpression(textOperation, context);
            if (IsStaticTextContent(textOperation))
                state.AddStaticChild(textExpression);
            else if (IsGuaranteedStringTextContent(textOperation))
                state.AddDynamicTextChild(textExpression);
            else
                state.AddChild(textExpression);
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
                   SymbolComparer.Equals(member.ContainingType!.OriginalDefinition, _componentSymbol.OriginalDefinition);
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
                   containingType.ContainingNamespace!.ToDisplayString(),
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
                Node? node;
                try
                {
                    node = _walker.Visit(operation, context.Argument);
                }
                catch (OperationTransformationException exception)
                {
                    // RenderTree protocol errors are owned by this emitter, but expression
                    // failures from SemanticWalker retain the compiler bridge category and its
                    // exact Roslyn location. Do not reclassify them from message text upstream.
                    throw new RazorVueDiagnosticException(
                        RazorVueDiagnosticFactory.FromException(
                            exception,
                            RazorVueDiagnosticCategory.CompilerBridge,
                            _componentSymbol),
                        exception);
                }

                if (node is null)
                    throw CreateCompilerBridgeFailure(
                        operation,
                        "Expression did not produce a JavaScript node.");
                if (node is not Expression expression)
                    throw CreateCompilerBridgeFailure(
                        operation,
                        "Expression did not lower to a JavaScript expression.");

                return expression;
            }
            finally
            {
                _activeExpressionContext = previousContext;
            }
        }

        private RazorVueDiagnosticException CreateCompilerBridgeFailure(
            IOperation operation,
            string message)
            => new(
                RazorVueDiagnosticFactory.Create(
                    RazorVueDiagnosticCategory.CompilerBridge,
                    message,
                    operation.Syntax.GetLocation(),
                    _componentSymbol),
                new InvalidOperationException(message));

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
                    lowered.UsesFragment);
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
                    lowered.UsesFragment);
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
                    UsesFragment: lowered.UsesFragment);
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
                    property.ContainingType!.OriginalDefinition,
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
                if (!TryGetReturnedPropertyValue(property, out var returnedValue) &&
                    !TryGetLocalRenderFragmentPropertyValue(property, out returnedValue))
                {
                    return false;
                }

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
            var propertyExpressionBody = declaration.ExpressionBody;
            IOperation? operation = propertyExpressionBody is not null
                ? model.GetOperation(propertyExpressionBody.Expression)
                : declaration.AccessorList!.Accessors
                    .Where(static accessor => accessor.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.GetAccessorDeclaration))
                    .Select(accessor =>
                    {
                        var getterExpressionBody = accessor.ExpressionBody;
                        return getterExpressionBody is not null
                            ? model.GetOperation(getterExpressionBody.Expression)
                            : accessor.Body is not null
                                ? TryGetSingleReturnValue(model.GetOperation(accessor.Body))
                                : null;
                    })
                    .SingleOrDefault();
            if (operation is null)
                return false;

            returnedValue = operation;
            return true;
        }

        private bool TryGetLocalRenderFragmentPropertyValue(IPropertySymbol property, out IOperation returnedValue)
        {
            returnedValue = null!;
            if (property.DeclaringSyntaxReferences[0].GetSyntax() is not PropertyDeclarationSyntax
                {
                    AccessorList: { } accessorList
                } declaration)
            {
                return false;
            }

            var getter = accessorList.Accessors.SingleOrDefault(static accessor =>
                accessor.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.GetAccessorDeclaration) && accessor.Body is not null);
            if (getter?.Body is null)
                return false;

            var model = _compilation.GetSemanticModel(declaration.SyntaxTree);
            if (!TryGetRenderFragmentFactoryReturn(
                    model.GetOperation(getter.Body),
                    out var propertyReturn,
                    out var localRenderFragmentDeclarations) ||
                propertyReturn is null)
            {
                return false;
            }

            // Property getters share the same deliberately narrow grammar as fragment factory
            // methods: initialized RenderFragment locals followed by one return. This supports
            // ordinary @code composition without inventing arbitrary getter dataflow.
            // 属性 getter 与片段工厂共用受限语法：已初始化局部片段加单一 return，不推断任意数据流。
            return TryUnwrapLocalRenderFragmentFactoryReturn(
                propertyReturn,
                localRenderFragmentDeclarations,
                out returnedValue);
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
                    UsesFragment: helper.UsesFragment);
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
                        UsesFragment: fragmentState.UsesFragment || fragmentState.Roots.Count > 1);
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
                return new DirectRenderFunction(existingName, UsesFragment: false);

            var functionName = CreateRenderFragmentHelperFunctionName(method);
            _renderFragmentHelperFunctionNames.Add(originalMethod, functionName);

            if (!_emittingRenderFragmentHelperFunctions.Add(originalMethod))
                return new DirectRenderFunction(functionName, UsesFragment: false);

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
                        functionState.UsesFragment || functionState.Roots.Count > 1);
                });

                _usesFragment = _usesFragment || lowered.UsesFragment;
                var functionBody = new FunctionBody(
                    NodeList.From<Statement>(new ReturnStatement(lowered.RenderExpression)),
                    strict: true);
                _preludeStatements.Add(new FunctionDeclaration(
                    new Identifier(functionName),
                    NodeList.From<Node>(parameterNames.Select(static name => (Node)new Identifier(name))),
                    functionBody,
                    generator: false,
                    async: false));
                return new DirectRenderFunction(functionName, lowered.UsesFragment);
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
                case MethodDeclarationSyntax { ExpressionBody: { } expressionBody }:
                    returnedOperation = model.GetOperation(expressionBody.Expression);
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

            if (!TryUnwrapLocalRenderFragmentFactoryReturn(
                    returnedOperation!,
                    localRenderFragmentDeclarations,
                    out returnedOperation))
            {
                return false;
            }
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

        private static bool TryUnwrapLocalRenderFragmentFactoryReturn(
            IOperation returnedOperation,
            ImmutableArray<IVariableDeclarationGroupOperation> localRenderFragmentDeclarations,
            out IOperation resolvedOperation)
        {
            resolvedOperation = returnedOperation;
            var visited = new HashSet<ILocalSymbol>(SymbolComparer);
            while (true)
            {
                while (resolvedOperation is IConversionOperation conversion)
                    resolvedOperation = conversion.Operand;

                if (resolvedOperation is not ILocalReferenceOperation localReference)
                    return true;

                // The factory grammar has already limited these declarations to initialized
                // RenderFragment locals. Follow that straight-line provenance only; do not
                // infer arbitrary C# dataflow from a return expression.
                // 工厂语法已限制为已初始化的片段局部变量；这里只追踪该直线来源，不猜测任意数据流。
                if (!visited.Add(localReference.Local))
                    return false;

                var declaration = localRenderFragmentDeclarations
                    .SelectMany(static group => group.Declarations)
                    .SelectMany(static declaration => declaration.Declarators)
                    .FirstOrDefault(candidate => SymbolComparer.Equals(candidate.Symbol, localReference.Local));
                if (declaration?.Initializer is null)
                    return false;

                resolvedOperation = declaration.Initializer.Value;
            }
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

    private static bool IsTDesignTableCellType(ITypeSymbol? type)
        => type is INamedTypeSymbol named &&
           named.OriginalDefinition.TypeParameters.Length == 1 &&
           string.Equals(named.ContainingNamespace.ToDisplayString(), "ECMAScript.TDesign", StringComparison.Ordinal) &&
           (string.Equals(named.Name, "TPrimaryTableColCell", StringComparison.Ordinal) ||
            string.Equals(named.Name, "TBaseTableColCell", StringComparison.Ordinal));

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
                   named.ContainingNamespace!.ToDisplayString(),
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
               named.ContainingNamespace!.ToDisplayString(),
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

    private static string CreateConditionalBranchKey(IConditionalOperation conditional, bool whenTrue)
    {
        var mappedSpan = conditional.Syntax.GetLocation().GetMappedLineSpan();
        // Do not use an emitter counter here. Inserting an unrelated earlier conditional must not
        // renumber existing branch identities and force unrelated DOM replacement after HMR.
        // 不使用 emit counter；前面插入无关条件不能重编号既有分支，否则 HMR 会无故替换 DOM。
        var identity = string.Concat(
            mappedSpan.Path,
            "|",
            mappedSpan.StartLinePosition.Line.ToString(System.Globalization.CultureInfo.InvariantCulture),
            "|",
            mappedSpan.StartLinePosition.Character.ToString(System.Globalization.CultureInfo.InvariantCulture),
            "|",
            mappedSpan.EndLinePosition.Line.ToString(System.Globalization.CultureInfo.InvariantCulture),
            "|",
            mappedSpan.EndLinePosition.Character.ToString(System.Globalization.CultureInfo.InvariantCulture),
            "|",
            conditional.Syntax.SyntaxTree.FilePath,
            "|",
            conditional.Syntax.SpanStart.ToString(System.Globalization.CultureInfo.InvariantCulture),
            "|",
            conditional.Syntax.Span.Length.ToString(System.Globalization.CultureInfo.InvariantCulture));
        return "__jazor$if_" + ComputeStableKeyHash(identity) + (whenTrue ? "t" : "f");
    }

    private static string ComputeStableKeyHash(string value)
    {
        // FNV-1a is sufficient for an artifact-local identity and keeps mapped source paths out
        // of the browser bundle. This is identity metadata, not a security hash.
        // FNV-1a 足够生成 artifact 内 identity，并且不把 source path 暴露到 browser bundle；
        // 它只用于稳定标识，不承担安全哈希职责。
        const ulong OffsetBasis = 14695981039346656037UL;
        const ulong Prime = 1099511628211UL;
        var hash = OffsetBasis;
        foreach (var character in value)
        {
            hash ^= character;
            hash *= Prime;
        }

        return hash.ToString("x16", System.Globalization.CultureInfo.InvariantCulture);
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

        // The supported RenderTreeBuilder attribute APIs are instance void methods. After
        // unwrapping the expression statement, C# cannot legally place a conversion around
        // that invocation, so accepting one here would only describe an impossible IR shape.
        // 支持的 attribute builder 均为 instance void；这里不保留虚假的 conversion fallback。
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

    private static bool IsCascadingValueComponent(INamedTypeSymbol componentType)
        => string.Equals(componentType.OriginalDefinition.MetadataName, "CascadingValue`1", StringComparison.Ordinal) &&
           string.Equals(
               componentType.OriginalDefinition.ContainingNamespace?.ToDisplayString(),
               "Microsoft.AspNetCore.Components",
               StringComparison.Ordinal);

    /// <summary>
    /// Identifies standard Blazor components whose public authoring contract is projected to a
    /// browser runtime adapter. The enum is deliberately closed: an unregistered component must
    /// continue through the normal generated-component path or fail at the existing boundary.
    /// </summary>
    private enum StandardBlazorComponentAdapterKind
    {
        None,
        Router,
        RouteView,
        LayoutView,
        NavLink,
        DynamicComponent,
        ErrorBoundary,
        EditForm,
        InputText,
        InputTextArea,
        InputCheckbox,
        InputNumber,
        InputDate,
        InputSelect
    }

    private readonly record struct StandardBlazorComponentAdapter(
        StandardBlazorComponentAdapterKind Kind,
        string ModuleSpecifier,
        string ExportName);

    private static bool TryGetStandardBlazorComponentAdapter(
        INamedTypeSymbol componentType,
        out StandardBlazorComponentAdapter adapter)
    {
        var definition = componentType.OriginalDefinition;
        var name = definition.MetadataName;
        var namespaceName = definition.ContainingNamespace?.ToDisplayString();
        if (string.Equals(namespaceName, "Microsoft.AspNetCore.Components.Routing", StringComparison.Ordinal))
        {
            adapter = name switch
            {
                "Router" => new StandardBlazorComponentAdapter(
                    StandardBlazorComponentAdapterKind.Router,
                    BlazorRoutingRuntimeModuleSpecifier,
                    "Router"),
                "NavLink" => new StandardBlazorComponentAdapter(
                    StandardBlazorComponentAdapterKind.NavLink,
                    BlazorRoutingRuntimeModuleSpecifier,
                    "NavLink"),
                _ => default
            };
            return adapter.Kind != StandardBlazorComponentAdapterKind.None;
        }

        if (string.Equals(namespaceName, "Microsoft.AspNetCore.Components.Forms", StringComparison.Ordinal))
        {
            adapter = name switch
            {
                "EditForm" => new StandardBlazorComponentAdapter(
                    StandardBlazorComponentAdapterKind.EditForm,
                    BlazorComponentsRuntimeModuleSpecifier,
                    "EditForm"),
                "InputText" => new StandardBlazorComponentAdapter(
                    StandardBlazorComponentAdapterKind.InputText,
                    BlazorComponentsRuntimeModuleSpecifier,
                    "InputText"),
                "InputTextArea" => new StandardBlazorComponentAdapter(
                    StandardBlazorComponentAdapterKind.InputTextArea,
                    BlazorComponentsRuntimeModuleSpecifier,
                    "InputTextArea"),
                "InputCheckbox" => new StandardBlazorComponentAdapter(
                    StandardBlazorComponentAdapterKind.InputCheckbox,
                    BlazorComponentsRuntimeModuleSpecifier,
                    "InputCheckbox"),
                "InputNumber`1" => new StandardBlazorComponentAdapter(
                    StandardBlazorComponentAdapterKind.InputNumber,
                    BlazorComponentsRuntimeModuleSpecifier,
                    "InputNumber"),
                "InputDate`1" => new StandardBlazorComponentAdapter(
                    StandardBlazorComponentAdapterKind.InputDate,
                    BlazorComponentsRuntimeModuleSpecifier,
                    "InputDate"),
                "InputSelect`1" => new StandardBlazorComponentAdapter(
                    StandardBlazorComponentAdapterKind.InputSelect,
                    BlazorComponentsRuntimeModuleSpecifier,
                    "InputSelect"),
                _ => default
            };
            return adapter.Kind != StandardBlazorComponentAdapterKind.None;
        }

        if (string.Equals(namespaceName, "Microsoft.AspNetCore.Components", StringComparison.Ordinal))
        {
            adapter = name switch
            {
                "RouteView" => new StandardBlazorComponentAdapter(
                    StandardBlazorComponentAdapterKind.RouteView,
                    BlazorRoutingRuntimeModuleSpecifier,
                    "RouteView"),
                "LayoutView" => new StandardBlazorComponentAdapter(
                    StandardBlazorComponentAdapterKind.LayoutView,
                    BlazorRoutingRuntimeModuleSpecifier,
                    "LayoutView"),
                "DynamicComponent" => new StandardBlazorComponentAdapter(
                    StandardBlazorComponentAdapterKind.DynamicComponent,
                    BlazorComponentsRuntimeModuleSpecifier,
                    "DynamicComponent"),
                "ErrorBoundary" => new StandardBlazorComponentAdapter(
                    StandardBlazorComponentAdapterKind.ErrorBoundary,
                    BlazorComponentsRuntimeModuleSpecifier,
                    "ErrorBoundary"),
                _ => default
            };
            return adapter.Kind != StandardBlazorComponentAdapterKind.None;
        }

        if (string.Equals(namespaceName, "Microsoft.AspNetCore.Components.Web", StringComparison.Ordinal))
        {
            adapter = name switch
            {
                "ErrorBoundary" => new StandardBlazorComponentAdapter(
                    StandardBlazorComponentAdapterKind.ErrorBoundary,
                    BlazorComponentsRuntimeModuleSpecifier,
                    "ErrorBoundary"),
                _ => default
            };
            return adapter.Kind != StandardBlazorComponentAdapterKind.None;
        }

        adapter = default;
        return false;
    }

    private static bool IsStandardInputAdapter(StandardBlazorComponentAdapterKind kind)
        => kind is StandardBlazorComponentAdapterKind.InputText or
            StandardBlazorComponentAdapterKind.InputTextArea or
            StandardBlazorComponentAdapterKind.InputCheckbox or
            StandardBlazorComponentAdapterKind.InputNumber or
            StandardBlazorComponentAdapterKind.InputDate or
            StandardBlazorComponentAdapterKind.InputSelect;

    private static ITypeSymbol? GetStandardBlazorComponentValueType(INamedTypeSymbol componentType)
        => componentType.TypeArguments.Length == 1
            ? componentType.TypeArguments[0]
            : null;

    private static bool TryBuildStandardInputValueTypeDescriptor(
        StandardBlazorComponentAdapterKind adapterKind,
        ITypeSymbol? valueType,
        out Expression descriptor)
    {
        descriptor = null!;
        if (valueType is null)
            return false;

        var nullable = valueType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T;
        var effectiveType = nullable && valueType is INamedTypeSymbol nullableType
            ? nullableType.TypeArguments[0]
            : valueType;

        var properties = new List<Node>
        {
            CreateObjectProperty("nullable", new BooleanLiteral(nullable, nullable ? "true" : "false"))
        };

        if (adapterKind == StandardBlazorComponentAdapterKind.InputDate)
        {
            var displayName = effectiveType.OriginalDefinition.ToDisplayString();
            var kind = displayName switch
            {
                "System.DateOnly" => "dateonly",
                "System.DateTimeOffset" => "datetimeoffset",
                "System.DateTime" => "datetime",
                _ => null
            };
            if (kind is null)
                return false;

            properties.Insert(0, CreateObjectProperty("kind", StringLiteral(kind)));
            descriptor = new ObjectExpression(NodeList.From<Node>(properties));
            return true;
        }

        if (adapterKind == StandardBlazorComponentAdapterKind.InputNumber)
        {
            var specialType = effectiveType.SpecialType;
            var isBigInt = specialType is SpecialType.System_Int64 or SpecialType.System_UInt64 ||
                           string.Equals(effectiveType.OriginalDefinition.ToDisplayString(), "System.Int128", StringComparison.Ordinal) ||
                           string.Equals(effectiveType.OriginalDefinition.ToDisplayString(), "System.UInt128", StringComparison.Ordinal) ||
                           string.Equals(effectiveType.OriginalDefinition.ToDisplayString(), "System.Numerics.BigInteger", StringComparison.Ordinal);
            var isNumber = specialType is SpecialType.System_SByte or
                SpecialType.System_Byte or
                SpecialType.System_Int16 or
                SpecialType.System_UInt16 or
                SpecialType.System_Int32 or
                SpecialType.System_UInt32 or
                SpecialType.System_Single or
                SpecialType.System_Double or
                SpecialType.System_Decimal || isBigInt;
            if (!isNumber)
                return false;

            properties.Insert(0, CreateObjectProperty("kind", StringLiteral(isBigInt ? "bigint" : "number")));
            properties.Add(CreateObjectProperty(
                "integer",
                new BooleanLiteral(
                    isBigInt || specialType is SpecialType.System_SByte or
                        SpecialType.System_Byte or
                        SpecialType.System_Int16 or
                        SpecialType.System_UInt16 or
                        SpecialType.System_Int32 or
                        SpecialType.System_UInt32,
                    (isBigInt || specialType is SpecialType.System_SByte or
                        SpecialType.System_Byte or
                        SpecialType.System_Int16 or
                        SpecialType.System_UInt16 or
                        SpecialType.System_Int32 or
                        SpecialType.System_UInt32) ? "true" : "false")));
            descriptor = new ObjectExpression(NodeList.From<Node>(properties));
            return true;
        }

        if (adapterKind == StandardBlazorComponentAdapterKind.InputSelect)
        {
            if (effectiveType.TypeKind == TypeKind.Enum && effectiveType is INamedTypeSymbol enumType)
            {
                properties.Insert(0, CreateObjectProperty("kind", StringLiteral("enum")));
                var values = enumType.GetMembers()
                    .OfType<IFieldSymbol>()
                    .Where(static field => field.HasConstantValue)
                    .OrderBy(static field => field.Name, StringComparer.Ordinal)
                    .Select(field => (Node)CreateObjectProperty(
                        field.Name,
                        CreateInputScalarLiteral(enumType.EnumUnderlyingType!, field.ConstantValue)));
                properties.Add(CreateObjectProperty("values", new ObjectExpression(NodeList.From(values))));
                descriptor = new ObjectExpression(NodeList.From<Node>(properties));
                return true;
            }

            if (effectiveType.SpecialType == SpecialType.System_String)
            {
                properties.Insert(0, CreateObjectProperty("kind", StringLiteral("string")));
                descriptor = new ObjectExpression(NodeList.From<Node>(properties));
                return true;
            }

            if (effectiveType.SpecialType == SpecialType.System_Boolean)
            {
                properties.Insert(0, CreateObjectProperty("kind", StringLiteral("boolean")));
                descriptor = new ObjectExpression(NodeList.From<Node>(properties));
                return true;
            }

            if (effectiveType.SpecialType is SpecialType.System_SByte or
                SpecialType.System_Byte or
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
                properties.Insert(0, CreateObjectProperty("kind", StringLiteral("number")));
                properties.Add(CreateObjectProperty(
                    "integer",
                    new BooleanLiteral(
                        effectiveType.SpecialType is SpecialType.System_SByte or
                            SpecialType.System_Byte or
                            SpecialType.System_Int16 or
                            SpecialType.System_UInt16 or
                            SpecialType.System_Int32 or
                            SpecialType.System_UInt32 or
                            SpecialType.System_Int64 or
                            SpecialType.System_UInt64,
                        effectiveType.SpecialType is SpecialType.System_SByte or
                            SpecialType.System_Byte or
                            SpecialType.System_Int16 or
                            SpecialType.System_UInt16 or
                            SpecialType.System_Int32 or
                            SpecialType.System_UInt32 or
                            SpecialType.System_Int64 or
                            SpecialType.System_UInt64 ? "true" : "false")));
                descriptor = new ObjectExpression(NodeList.From<Node>(properties));
                return true;
            }
        }

        return false;
    }

    private static Expression CreateInputScalarLiteral(ITypeSymbol type, object? value)
    {
        if (value is null)
            return Null();
        if (value is bool boolean)
            return new BooleanLiteral(boolean, boolean ? "true" : "false");
        if (value is string text)
            return StringLiteral(text);

        var textValue = Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture) ?? "0";
        if (type.SpecialType is SpecialType.System_Int64 or SpecialType.System_UInt64)
        {
            var bigInteger = System.Numerics.BigInteger.Parse(textValue, System.Globalization.CultureInfo.InvariantCulture);
            return new BigIntLiteral(bigInteger, bigInteger.ToString(System.Globalization.CultureInfo.InvariantCulture) + "n");
        }

        return new NumericLiteral(Convert.ToDouble(value, System.Globalization.CultureInfo.InvariantCulture), textValue);
    }

    private static ITypeSymbol GetCascadingValueType(IOperation operation, EmitContext context)
    {
        while (operation is IConversionOperation conversion)
            operation = conversion.Operand;

        if (operation is IParameterReferenceOperation parameterReference &&
            TryGetParameterSubstitution(context.Substitutions, parameterReference.Parameter, out var substituted))
        {
            // Razor's generated TypeInference helpers describe Value as TValue. Follow the
            // existing direct-render parameter substitution instead of leaking that helper's
            // open generic type into the browser runtime key.
            // TypeInference 参数 TValue 只是一层生成 helper；沿用既有替换拿到作者实参类型。
            return GetCascadingValueType(substituted, context);
        }

        if (operation.Type is null || operation.Type.TypeKind == TypeKind.TypeParameter)
        {
            throw Unsupported(
                operation,
                "CascadingValue requires a statically known Value type for browser cascade lookup. " +
                "Pass a value with a concrete declared type rather than an unresolved generic type parameter.");
        }

        return operation.Type;
    }

    private static bool TryGetParameterSubstitution(
        IReadOnlyDictionary<IParameterSymbol, IOperation> substitutions,
        IParameterSymbol parameter,
        out IOperation value)
    {
        if (substitutions.TryGetValue(parameter, out value!))
            return true;

        return substitutions.TryGetValue(parameter.OriginalDefinition, out value!);
    }

    private static ImmutableDictionary<string, string> BuildCascadingValueParameterNameMap()
        => ImmutableDictionary.CreateRange(
            StringComparer.Ordinal,
            new[]
            {
                new KeyValuePair<string, string>("Value", "value"),
                new KeyValuePair<string, string>("Name", "name"),
                new KeyValuePair<string, string>("IsFixed", "isFixed")
            });

    private static ImmutableDictionary<string, string> BuildCascadingValueSlotNameMap()
        => ImmutableDictionary.CreateRange(
            StringComparer.Ordinal,
            new[] { new KeyValuePair<string, string>("ChildContent", "default") });

    private static bool TryResolveTypeOfExpression(
        IOperation operation,
        IReadOnlyDictionary<ILocalSymbol, INamedTypeSymbol> localComponentTypes,
        out INamedTypeSymbol componentType)
    {
        componentType = null!;
        while (operation is IConversionOperation conversion)
            operation = conversion.Operand;

        // Official Razor SG commonly wraps component type expressions in
        // RuntimeHelpers.TypeCheck<T>(typeof(T)). The helper is compile-time only and must not
        // force page authors to introduce a local or a RazorVue-specific token.
        // Razor SG 的 TypeCheck 只是类型绑定协议，DynamicComponent 仍可直接使用 typeof(T)。
        if (operation is IInvocationOperation typeCheck &&
            string.Equals(typeCheck.TargetMethod.Name, "TypeCheck", StringComparison.Ordinal) &&
            typeCheck.Arguments.Length == 1)
        {
            return TryResolveTypeOfExpression(typeCheck.Arguments[0].Value, localComponentTypes, out componentType);
        }

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
        // RenderTreeBuilder members recognized by this lowering are instance methods. An
        // extension-style static call has another containing type and is not this protocol.
        // RenderTreeBuilder 约定只接受 instance receiver，不能把首个参数误当 builder。
        IOperation? receiver = invocation.Instance;

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

        return builder.ToString();
    }

    private static ComponentImportDescriptor ResolveComponentImport(INamedTypeSymbol componentType)
    {
        var exportPath = GetECMAScriptModuleExportPath(componentType);
        if (!string.IsNullOrWhiteSpace(exportPath))
            return new ComponentImportDescriptor(NormalizeModuleImportPath(exportPath!), "default");

        foreach (var attribute in componentType.GetAttributes())
        {
            if (!string.Equals(
                    attribute.AttributeClass!.ToDisplayString(),
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
            if (string.Equals(attribute.AttributeClass!.ToDisplayString(), ECMAScriptModuleAttributeMetadataName, StringComparison.Ordinal) &&
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
        // AddContent(MarkupString?) must omit null and evaluate the payload once. Passing it as
        // one function argument preserves both properties without an extra IIFE in every render.
        return Call(VueRawMarkup.CreateRawMarkupName, markupExpression);
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

    private static Expression BuildFusedDirectDomBindHandler(
        ArrowFunctionExpression handlerExpression,
        string attributeName)
    {
        var eventParameter = new Identifier("event");
        var target = new MemberExpression(eventParameter, new Identifier("target"), computed: false, optional: false);
        var value = new MemberExpression(target, StringLiteral(attributeName), computed: true, optional: false);
        var sourceAssignment = (AssignmentExpression)handlerExpression.Body;
        var assignment = new AssignmentExpression(sourceAssignment.Operator, sourceAssignment.Left, value)
        {
            UserData = sourceAssignment.UserData
        };
        // Roslyn has already proved this binder is one direct parameter-to-target assignment.
        // Keep the compiler-lowered lambda as the semantic owner while removing the generic
        // value/event discriminator and rest forwarding from the hot DOM event path.
        // direct bind 只融合 target.value/checked 提取；赋值本身仍由 SemanticWalker 产物执行。
        return new ArrowFunctionExpression(
            NodeList.From<Node>(eventParameter),
            assignment,
            expression: true,
            async: false)
        {
            UserData = handlerExpression.UserData
        };
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

    /// <summary>
    /// Identifies text payloads that can be represented as immutable VNode children. This is
    /// intentionally narrower than C# constant folding: only literal/null scalar payloads have
    /// no render-time source evaluation or conversion semantics to preserve.
    /// 只接受无运行时求值的标量文本；复杂 conversion/format/call 仍作为 dynamic text。
    /// </summary>
    private static bool IsStaticTextContent(IOperation operation)
    {
        while (operation is IConversionOperation conversion)
            operation = conversion.Operand;
        return operation.ConstantValue.HasValue &&
               operation.ConstantValue.Value is null or string or bool or char or sbyte or byte or short or ushort or int or uint or long or ulong or float or double or decimal;
    }

    /// <summary>
    /// TEXT patching is enabled only for an authored string expression. Other AddContent values
    /// may need CLR formatting or value-specific normalization, so they retain the conservative
    /// child diff until that semantic slice has an explicit Vue contract.
    /// 目前只优化 string，避免 object/数值/自定义格式化被错误当作 Vue text fast path。
    /// </summary>
    private static bool IsGuaranteedStringTextContent(IOperation operation)
    {
        while (operation is IConversionOperation conversion)
            operation = conversion.Operand;

        return operation.Type?.SpecialType == SpecialType.System_String;
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
            method.ContainingType!.OriginalDefinition.ToDisplayString(Format.NameFormat),
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

    /// <summary>
    /// Maintains the active RenderTree frame stack and the RazorVue-only RenderPlan.
    /// C# expressions have already been lowered before entering this state; plans retain only
    /// VNode framing/update facts, so this layer never becomes a second expression compiler.
    /// 保留 Vue block 所需最小 metadata，不保存或重写 IOperation。
    /// </summary>
    private sealed class RenderState
    {
        private readonly string? _implicitRootKey;

        public RenderState(string? implicitRootKey = null)
        {
            _implicitRootKey = implicitRootKey;
        }

        public RenderPlan Plan { get; } = new();

        public List<VNodePlan> Roots => Plan.Roots;

        public Stack<Frame> Stack { get; } = new();

        public List<Statement> PendingPreludeStatements { get; } = new();

        private List<Expression> Guards { get; } = new();

        public bool UsesFragment { get; set; }

        public Expression ToRenderExpression()
            => Plan.ToRenderExpression();

        public void StartChildren()
        {
            if (Stack.Count > 0)
                Stack.Peek().ChildrenStarted = true;
        }

        public void AddChild(Expression expression)
            => AddChild(VNodePlan.Opaque(expression));

        public void AddStaticChild(Expression expression)
            => AddChild(VNodePlan.Static(expression));

        public void AddDynamicTextChild(Expression expression)
            => AddChild(VNodePlan.DynamicText(expression));

        public void AddChild(VNodePlan child)
        {
            if (Stack.Count == 0)
            {
                var expression = WrapWithStatements(PendingPreludeStatements, child.Expression);
                PendingPreludeStatements.Clear();
                var guarded = ApplyGuards(expression);
                // A root guard is runtime control flow. Its contained VNode facts remain useful
                // inside that branch, but the root itself cannot be treated as a static child.
                // 根 guard 改变 vnode existence，因此最外层降级为 opaque。
                Roots.Add(Guards.Count == 0
                    ? child with { Expression = guarded }
                    : VNodePlan.Opaque(guarded));
                return;
            }

            var frame = Stack.Peek();
            frame.ChildrenStarted = true;
            frame.Children.Add(child);
        }

        public void AddChildSequence(Expression expression)
        {
            if (Stack.Count == 0)
            {
                expression = WrapWithStatements(PendingPreludeStatements, expression);
                PendingPreludeStatements.Clear();
                expression = VueSlotAstFactory.NormalizeContent(ApplyGuards(expression));
                Roots.Add(VNodePlan.Sequence(expression));
                return;
            }

            var frame = Stack.Peek();
            frame.ChildrenStarted = true;
            frame.Children.Add(VNodePlan.Sequence(
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
            if (Stack.Count == 0 && TryGetImplicitRootKey(out var implicitRootKey))
                _ = frame.TrySetImplicitRootKey(implicitRootKey);
            if (frame is RegionFrame region && region.CreatesFragment)
                UsesFragment = true;
            AddChild(frame.ToVNodePlan());
        }

        private bool TryGetImplicitRootKey(out string key)
        {
            if (_implicitRootKey is null)
            {
                key = string.Empty;
                return false;
            }

            key = _implicitRootKey + "_" +
                Roots.Count.ToString(System.Globalization.CultureInfo.InvariantCulture);
            return true;
        }
    }

    /// <summary>
    /// Final RazorVue VNode plan for one render scope. It deliberately stores only an already
    /// lowered ESTree expression plus Vue update facts; it never retains Roslyn operations.
    /// 该 plan 是 render framing metadata，不是第二套 C# lowering pipeline。
    /// </summary>
    private sealed class RenderPlan
    {
        public List<VNodePlan> Roots { get; } = new();

        public Expression ToRenderExpression()
        {
            return Roots.Count switch
            {
                0 => Null(),
                1 when Roots[0].Kind == VNodePlanKind.Sequence => Roots[0].Expression,
                1 => Roots[0].Expression,
                _ => CreateFragment(Roots.Select(static child => child.ToNormalArrayItem()))
            };
        }
    }

    /// <summary>Classifies a finished VNode without making claims about its C# source expression.</summary>
    private enum VNodePlanKind
    {
        Static,
        DynamicText,
        Block,
        Opaque,
        Sequence
    }

    /// <summary>
    /// Small immutable child-plan carrier. `Sequence` represents slot/nullable protocol output
    /// and is intentionally excluded from block collection until fragment lowering owns it.
    /// sequence/conditional/slot 保持 opaque，避免错误标记为 Vue dynamic child。
    /// </summary>
    private readonly record struct VNodePlan(
        Expression Expression,
        VNodePlanKind Kind,
        bool HasExplicitKey = false,
        bool CanUseRenderList = false)
    {
        public bool CanParticipateInBlock
            => Kind is VNodePlanKind.Static or VNodePlanKind.DynamicText or VNodePlanKind.Block;

        public bool IsDynamicChild
            => Kind is VNodePlanKind.DynamicText or VNodePlanKind.Block;

        // Only completed element/component/block nodes are legal renderList mapper roots.
        // Text, static markup, sequence, and opaque protocol values retain Array.from because
        // their fragment identity and Vue normalization contract are not proven here.
        // 只把完整 VNode 交给 renderList，其他内容继续走保守路径。
        public bool IsDirectVNodeRoot
            => Kind == VNodePlanKind.Block ||
               Kind == VNodePlanKind.Static && Expression is CallExpression;

        public static VNodePlan Static(
            Expression expression,
            bool hasExplicitKey = false,
            bool canUseRenderList = false)
            => new(expression, VNodePlanKind.Static, hasExplicitKey, canUseRenderList);

        public static VNodePlan DynamicText(Expression expression)
            => new(expression, VNodePlanKind.DynamicText);

        public static VNodePlan Block(
            Expression expression,
            bool hasExplicitKey = false,
            bool canUseRenderList = false)
            => new(expression, VNodePlanKind.Block, hasExplicitKey, canUseRenderList);

        public static VNodePlan Opaque(Expression expression, bool hasExplicitKey = false)
            => new(expression, VNodePlanKind.Opaque, hasExplicitKey);

        public static VNodePlan Sequence(Expression expression)
            => new(expression, VNodePlanKind.Sequence);

        public Expression ToNormalArrayItem()
            => Kind == VNodePlanKind.Sequence ? new SpreadElement(Expression) : Expression;

        public Expression ToBlockArrayItem()
            => Kind == VNodePlanKind.DynamicText
                ? Call(
                    "createTextVNode",
                    Expression,
                    new NumericLiteral(VuePatchFlags.Text, VuePatchFlags.Text.ToString(System.Globalization.CultureInfo.InvariantCulture)))
                : Kind == VNodePlanKind.Static && Expression is Acornima.Ast.StringLiteral or Acornima.Ast.NumericLiteral or Acornima.Ast.BooleanLiteral
                    // Vue's hydration walks block children verbatim; a bare primitive is not a
                    // vnode and crashes hydrateNode. The Vue compiler always wraps block-array
                    // text in createTextVNode, and static text needs no TEXT patch flag because
                    // it never updates. Mount-time normalizeVNode used to mask this, hydration
                    // does not. block children 数组中的静态原始值必须包成 text vnode。
                    ? Call("createTextVNode", Expression)
                    : Expression;
    }

    /// <summary>Base stack frame for an open Razor render region. 子类决定关闭后的 Vue vnode 形状。</summary>
    private abstract class Frame
    {
        public bool ChildrenStarted { get; set; }

        public List<VNodePlan> Children { get; } = new();

        public abstract VNodePlan ToVNodePlan();

        // The enclosing Razor conditional calls this only after the frame becomes a branch root.
        // Returning false lets RenderState preserve non-prop content under a keyed Fragment.
        // 仅当 frame 成为 Razor 条件分支根时调用；不能承载 props 的 frame 返回 false，
        // RenderState 会用 keyed Fragment 保留原内容。
        public virtual bool TrySetImplicitRootKey(string key)
            => false;

        public Expression ToRenderExpression()
            => ToVNodePlan().Expression;

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
        private string? _implicitRootKey;

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

        protected bool HasExplicitVNodeKey { get; private set; }

        // Event callbacks, bind adapters, and refs are valid VNodes but are deliberately not
        // part of E2's list proof. Their closure/ref identity is a later optimization slice.
        // event/@bind/@ref 暂不进入 renderList 快路径，先保留现有 per-iteration 协议。
        protected virtual bool CanUseRenderList
            => _referenceCaptures.Count == 0 &&
               !_propSources
                   .OfType<AttributePropSource>()
                   .Any(static source =>
                       IsDirectEventAttributeName(source.Attribute.Name) ||
                       source.Attribute.Name.StartsWith("@", StringComparison.Ordinal) ||
                       source.Attribute.ValueExpression is ArrowFunctionExpression or FunctionExpression);

        public void AddAttribute(DirectAttribute attribute)
        {
            _propSources.Add(new AttributePropSource(attribute));
            _lastAttributeName = attribute.Name;
        }

        public void SetExplicitVNodeKey(Expression valueExpression)
        {
            AddAttribute(new DirectAttribute("key", valueExpression));
            HasExplicitVNodeKey = true;
        }

        public override bool TrySetImplicitRootKey(string key)
        {
            // Razor @key remains the author's explicit identity contract. The compiler only
            // synthesizes a branch key when the root has no authored key of its own.
            // Razor @key 是作者显式 identity 契约；只有根节点未声明 @key 时才补条件分支 key。
            if (!HasExplicitVNodeKey)
                _implicitRootKey = key;
            return true;
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

            }
            }

            if (_referenceCaptures.Count > 0)
                properties.Add(CreateObjectProperty("ref", FormatReferenceCaptureExpression()));
            if (_implicitRootKey is not null)
                properties.Add(CreateObjectProperty("key", StringLiteral(_implicitRootKey)));
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

                var attribute = ((AttributePropSource)source).Attribute;

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
        private readonly Action? _useTextVNode;
        private string? _updatesAttributeName;
        private string? _updatesEventName;

        public ElementFrame(Expression tagExpression, string tagName)
            : this(tagExpression, tagName, null, null, null, null, null, null, null)
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
            Action? useBlockTree,
            Action? useTextVNode = null)
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
            _useTextVNode = useTextVNode;
        }

        public string TagName { get; }

        public override string NormalizeAttributeName(string name)
            => NormalizeDirectElementAttributeName(name);

        public override VNodePlan ToVNodePlan()
        {
            var props = FormatPropsExpression();
            var isSingleDynamicTextChild = Children.Count == 1 && Children[0].Kind == VNodePlanKind.DynamicText;
            // A leaf with dynamic props was already a proven G2 block shape. For non-leaf
            // nodes, require a complete immediate-child plan before expanding that fast path.
            // 无 children 的动态 props 保留旧安全 block；有 children 才要求完整 child plan。
            var hasSafeBlockChildren = Children.Count == 0 ||
                                       isSingleDynamicTextChild ||
                                       Children.All(static child => child.CanParticipateInBlock);
            var hasDynamicChild = isSingleDynamicTextChild || Children.Any(static child => child.IsDynamicChild);
            var children = hasSafeBlockChildren
                ? FormatBlockChildrenExpression()
                : FormatChildrenExpression();
            var patch = BuildPatchMetadata(
                hasBlockChild: hasDynamicChild,
                additionalFlags: isSingleDynamicTextChild ? VuePatchFlags.Text : 0);
            // A Vue block is valid only after every direct child has either static identity,
            // its own block contract, or a TEXT vnode flag. Conditional/sequence/slot/raw
            // values stay opaque and keep h()'s complete children diff.
            // immediate child 不完整时绝不伪造 block，防止 Vue 跳过未知 child 更新。
            if (!hasSafeBlockChildren || !patch.RequiresBlock)
            {
                var expression = Call("h", _tagExpression, props, children);
                return !patch.RequiresBlock && Children.All(static child => child.Kind == VNodePlanKind.Static)
                    ? VNodePlan.Static(expression, HasExplicitVNodeKey, CanUseRenderList)
                    : VNodePlan.Opaque(expression, HasExplicitVNodeKey);
            }

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
            return VNodePlan.Block(
                new SequenceExpression(NodeList.From<Expression>(Call("openBlock"), Call("createElementBlock", arguments))),
                HasExplicitVNodeKey,
                CanUseRenderList);
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
                value = !_eventModifiers.ContainsKey(attribute.Name) &&
                        value is ArrowFunctionExpression directBinder &&
                        IsFusibleDirectDomBindAttribute(
                            _updatesAttributeName,
                            attribute.DirectBinderValueKind,
                            directBinder)
                    ? BuildFusedDirectDomBindHandler(directBinder, _updatesAttributeName)
                    : BuildDirectDomBindHandler(value, _updatesAttributeName);
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

        private static bool IsFusibleDirectDomBindAttribute(
            string attributeName,
            DirectBinderValueKind valueKind,
            ArrowFunctionExpression handler)
        {
            if (handler.Params.Count != 1 ||
                handler.Params[0] is not Identifier ||
                handler.Body is not AssignmentExpression { Operator: Operator.Assignment })
                return false;

            // DOM value is already a string and checked is already a boolean. Other binder
            // types can require BindConverter parse/culture semantics and stay on the generic path.
            // 仅 value:string 与 checked:boolean 可无转换直传；数值/日期等不得绕过解析语义。
            return string.Equals(attributeName, "value", StringComparison.Ordinal) &&
                       valueKind == DirectBinderValueKind.String ||
                   string.Equals(attributeName, "checked", StringComparison.Ordinal) &&
                       valueKind == DirectBinderValueKind.Boolean;
        }

        private Expression FormatChildrenExpression()
            => Children.Count == 0
                ? Null()
                : CreateArray(Children.Select(static child => child.ToNormalArrayItem()));

        private Expression FormatBlockChildrenExpression()
        {
            if (Children.Count == 0)
                return Null();

            if (Children.Count == 1 && Children[0].Kind == VNodePlanKind.DynamicText)
                return Children[0].Expression;

            if (Children.Any(static child => child.Kind == VNodePlanKind.DynamicText ||
                    // Bare static primitives in block arrays now emit createTextVNode too, so
                    // the import must be registered for exactly the same child set. The AST
                    // literal types are qualified because this frame owns a StringLiteral helper.
                    (child.Kind == VNodePlanKind.Static &&
                        child.Expression is Acornima.Ast.StringLiteral or Acornima.Ast.NumericLiteral or Acornima.Ast.BooleanLiteral)))
                _useTextVNode?.Invoke();

            return CreateArray(Children.Select(static child => child.ToBlockArrayItem()));
        }
    }

    /// <summary>Accumulates component props, event listeners, and named/default slots.</summary>
    private sealed class ComponentFrame : PropFrame
    {
        private readonly Expression _componentExpression;
        private readonly ImmutableDictionary<string, string> _parameterNameMap;
        private readonly ImmutableDictionary<string, string> _slotNameMap;
        private readonly Action? _useBlockTree;
        private readonly Action? _useWithCtx;
        private readonly Action? _useCreateSlots;
        private readonly bool _slotsAreInStableScope;
        private readonly bool _isCascadingValue;
        private readonly StandardBlazorComponentAdapterKind _standardAdapterKind;
        private bool _hasCascadingValueTypeKey;
        private bool _hasStandardInputValueTypeDescriptor;

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
            : this(componentExpression, parameterNameMap, slotNameMap, null, null, null, null, null, null, null, null, true)
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
            Action? useBlockTree,
            Action? useWithCtx,
            Action? useCreateSlots,
            bool slotsAreInStableScope,
            bool isCascadingValue = false,
            StandardBlazorComponentAdapterKind standardAdapterKind = StandardBlazorComponentAdapterKind.None,
            ITypeSymbol? standardValueType = null)
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
            _useWithCtx = useWithCtx;
            _useCreateSlots = useCreateSlots;
            _slotsAreInStableScope = slotsAreInStableScope;
            _isCascadingValue = isCascadingValue;
            _standardAdapterKind = standardAdapterKind;
            StandardValueType = standardValueType;
        }

        public List<DirectSlot> Slots { get; } = new();

        public bool IsCascadingValue => _isCascadingValue;

        public StandardBlazorComponentAdapterKind StandardAdapterKind => _standardAdapterKind;

        public ITypeSymbol? StandardValueType { get; }

        public void SetCascadingValueTypeKey(string typeKey)
        {
            if (!_isCascadingValue)
                throw new InvalidOperationException("Only the CascadingValue runtime adapter can receive a cascade type key.");
            if (_hasCascadingValueTypeKey)
                return;

            AddAttribute(new DirectAttribute(
                CascadingValueTypePropName,
                StringLiteral(typeKey)));
            _hasCascadingValueTypeKey = true;
        }

        public void SetStandardInputValueTypeDescriptor(Expression descriptor)
        {
            if (!IsStandardInputAdapter(_standardAdapterKind) || _hasStandardInputValueTypeDescriptor)
                return;

            AddAttribute(new DirectAttribute(StandardInputValueTypePropName, descriptor));
            _hasStandardInputValueTypeDescriptor = true;
        }

        protected override bool CanUseRenderList
            => base.CanUseRenderList && Slots.Count == 0;

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

        public override VNodePlan ToVNodePlan()
        {
            if (_isCascadingValue && !_hasCascadingValueTypeKey)
            {
                throw new InvalidOperationException(
                    "RazorVue CascadingValue lowering requires a concrete Value parameter so the browser adapter can build its typed cascade key.");
            }

            var props = FormatPropsExpression();
            Expression? children = null;
            var additionalFlags = 0;
            if (Slots.Count > 0)
            {
                var slotsAreStable = _slotsAreInStableScope &&
                                     Slots.All(static slot =>
                                         slot.Fragment.Selection is null &&
                                         slot.Fragment.AvailabilityCondition is null &&
                                         !slot.Fragment.ReturnsVueSlotContent);
                if (slotsAreStable)
                {
                    // Stable slots must still carry withCtx so Vue restores the rendering
                    // instance when a child invokes them later. `_: 1` enables the compiler
                    // fast path without hoisting a closure across component instances.
                    // 固定 slot 使用 withCtx 保持实例上下文，_: 1 只声明对象稳定性。
                    _useWithCtx?.Invoke();
                    var stableMembers = new List<Node>(Slots.Count + 1);
                    stableMembers.AddRange(Slots.Select(static slot => (Node)CreateSlotProperty(slot, slot.Fragment)));
                    stableMembers.Add(CreateObjectProperty("_", new NumericLiteral(1, "1")));
                    children = new ObjectExpression(NodeList.From(stableMembers));
                }
                else
                {
                    // Conditional/forwarded/non-stable-scope slots may change their presence
                    // or capture on each render. Vue's createSlots protocol owns that update
                    // boundary; never mark them stable merely because their key is constant.
                    // 条件、转发或非稳定 scope slot 必须走 createSlots + 1024。
                    _useWithCtx?.Invoke();
                    _useCreateSlots?.Invoke();
                    var baseMembers = new List<Node> { CreateObjectProperty("_", new NumericLiteral(2, "2")) };
                    var dynamicSlots = new List<Expression>(Slots.Count);
                    for (var index = 0; index < Slots.Count; index++)
                    {
                        var slot = Slots[index];
                        dynamicSlots.Add(CreateDynamicSlotExpression(
                            slot,
                            slot.Fragment,
                            index.ToString(System.Globalization.CultureInfo.InvariantCulture)));
                    }

                    children = Call(
                        "createSlots",
                        new ObjectExpression(NodeList.From(baseMembers)),
                        CreateArray(dynamicSlots));
                    additionalFlags |= VuePatchFlags.DynamicSlots;
                }
            }
            else if (Children.Count > 0)
            {
                children = Children.Count == 1
                    ? Children[0].Expression
                    : CreateArray(Children.Select(static child => child.ToNormalArrayItem()));
            }

            var patch = BuildPatchMetadata(
                // Vue invokes component slots outside the parent's direct child traversal.
                // Even a stable slot object must therefore create a component block, otherwise
                // the parent cannot retain Vue's component/slot update boundary.
                // slot 即便稳定也必须有 component block，不能退化成普通 h(...)。
                hasBlockChild: Slots.Count != 0,
                componentProps: true,
                additionalFlags: additionalFlags);
            // Non-slot component children are eagerly created while the block is open. Keep
            // them on h() until each direct child carries a proven patch contract.
            if (Children.Count != 0 || !patch.RequiresBlock)
            {
                var expression = children is null
                    ? Call("h", _componentExpression, props)
                    : Call("h", _componentExpression, props, children);
                return Children.Count == 0 && !patch.RequiresBlock
                    ? VNodePlan.Static(expression, HasExplicitVNodeKey, CanUseRenderList)
                    : VNodePlan.Opaque(expression, HasExplicitVNodeKey);
            }

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

            return VNodePlan.Block(
                new SequenceExpression(NodeList.From<Expression>(Call("openBlock"), Call("createBlock", arguments))),
                HasExplicitVNodeKey,
                CanUseRenderList);
        }

        private static Expression CreateDynamicSlotExpression(
            DirectSlot slot,
            DirectRenderFragment fragment,
            string branchKey)
        {
            if (fragment.Selection is { } selection)
            {
                return new ConditionalExpression(
                    selection.Condition,
                    CreateDynamicSlotExpression(slot, selection.WhenTrue, branchKey + "t"),
                    CreateDynamicSlotExpression(slot, selection.WhenFalse, branchKey + "f"));
            }

            if (fragment.AvailabilityCondition is BooleanLiteral availability)
            {
                return availability.Value
                    ? CreateDynamicSlotDescriptor(slot, fragment, branchKey)
                    : Null();
            }

            var descriptor = CreateDynamicSlotDescriptor(slot, fragment, branchKey);
            return fragment.AvailabilityCondition is null
                ? descriptor
                : new ConditionalExpression(
                    fragment.AvailabilityCondition,
                    descriptor,
                    Null());
        }

        private static ObjectExpression CreateDynamicSlotDescriptor(
            DirectSlot slot,
            DirectRenderFragment fragment,
            string branchKey)
            => new(NodeList.From<Node>(
                CreateObjectProperty("name", StringLiteral(slot.Name)),
                CreateObjectProperty("fn", CreateSlotFunction(slot, fragment)),
                CreateObjectProperty("key", StringLiteral(branchKey))));

        private static Property CreateSlotProperty(
            DirectSlot slot,
            DirectRenderFragment fragment)
            => CreateObjectProperty(slot.Name, CreateSlotFunction(slot, fragment));

        private static Expression CreateSlotFunction(
            DirectSlot slot,
            DirectRenderFragment fragment)
        {
            var slotFunction = new ArrowFunctionExpression(
                slot.Fragment.ParameterName is null
                    ? NodeList.From<Node>()
                    : NodeList.From<Node>(new Identifier(slot.Fragment.ParameterName)),
                NormalizeSlotRenderExpression(
                    BindSlotRenderExpression(slot.Fragment.ParameterName, fragment)),
                expression: true,
                async: false);
            // Every component slot can run after its parent render returns. Preserve the
            // originating component instance for both stable and dynamic slot protocols.
            // 所有 slot 都可能延后执行，统一 withCtx 才能恢复父组件渲染上下文。
            return Call("withCtx", slotFunction);
        }

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
        private string? _implicitRootKey;

        public bool CreatesFragment
            => Children.Count > 1 || _implicitRootKey is not null && Children.Count > 0;

        public override bool TrySetImplicitRootKey(string key)
        {
            if (Children.Count == 1)
            {
                // A region is a transparent RenderTreeBuilder transport frame. Wrapping its
                // sole completed child would change a RenderFragment's public vnode shape;
                // branch-key synthesis intentionally stops at this opaque fragment boundary.
                // Region 只是 RenderTreeBuilder 的透明传输 frame；包裹其唯一 child 会改变
                // RenderFragment 的 vnode 形状，因此 branch key 在这个 opaque 边界停止。
                return true;
            }

            _implicitRootKey = key;
            return true;
        }

        public override VNodePlan ToVNodePlan()
        {
            return Children.Count switch
            {
                0 => VNodePlan.Static(Null()),
                1 when _implicitRootKey is null => Children[0],
                _ when _implicitRootKey is not null => VNodePlan.Opaque(CreateFragment(
                    Children.Select(static child => child.ToNormalArrayItem()),
                    _implicitRootKey)),
                _ => Children.All(static child => child.Kind == VNodePlanKind.Static)
                    ? VNodePlan.Static(CreateFragment(Children.Select(static child => child.ToNormalArrayItem())))
                    : VNodePlan.Opaque(CreateFragment(Children.Select(static child => child.ToNormalArrayItem())))
            };
        }
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
        bool UsesRawMarkupRuntime,
        bool UsesBlockTree,
        bool UsesTextVNode,
        bool UsesRenderList,
        bool UsesWithCtx,
        bool UsesCreateSlots,
        bool UsesMergeProps,
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
        Expression ValueExpression,
        DirectBinderValueKind DirectBinderValueKind = DirectBinderValueKind.None);

    /// <summary>Tracks compiler-returned AST nodes by identity without occupying Node.UserData.</summary>
    private sealed class ReferenceExpressionComparer : IEqualityComparer<Expression>
    {
        public static ReferenceExpressionComparer Instance { get; } = new();

        public bool Equals(Expression? x, Expression? y)
            => ReferenceEquals(x, y);

        public int GetHashCode(Expression obj)
            => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
    }

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
        bool UsesFragment);

    /// <summary>Tracks a compile-time render-object local erased after direct lowering.</summary>
    private sealed record DirectRenderObject(
        ImmutableDictionary<IPropertySymbol, DirectRenderFragment> RenderFragments);

    private readonly record struct DirectRenderFragmentBody(
        Expression RenderExpression,
        bool UsesFragment,
        VNodePlan? DirectRoot = null,
        bool HasExplicitRootKey = false);

    private readonly record struct BranchingLoopBody(
        ImmutableArray<Statement> Statements,
        bool HasExplicitRootKey);

    private sealed class BranchingLoopFacts
    {
        public bool SawRenderedRoot { get; set; }

        public bool AllRenderedRootsExplicitlyKeyed { get; set; } = true;
    }

    private readonly record struct DirectEventModifier(
        Expression? PreventDefaultCondition,
        Expression? StopPropagationCondition);

    /// <summary>Vue runtime patch-flag values used by the conservative direct-render subset.</summary>
    private static class VuePatchFlags
    {
        public const int Text = 1 << 0;
        public const int Class = 1 << 1;
        public const int Style = 1 << 2;
        public const int Props = 1 << 3;
        public const int FullProps = 1 << 4;
        public const int NeedPatch = 1 << 9;
        public const int DynamicSlots = 1 << 10;
        public const int StableFragment = 1 << 6;
        public const int KeyedFragment = 1 << 7;
        public const int UnkeyedFragment = 1 << 8;
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

    private static Expression CreateFragment(IEnumerable<Expression> children, string? key = null)
        => Call(
            "h",
            new Identifier("Fragment"),
            key is null
                ? Null()
                : new ObjectExpression(NodeList.From<Node>(
                    [CreateObjectProperty("key", StringLiteral(key))])),
            CreateArray(children));

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
    bool UsesRawMarkupRuntime,
    bool UsesBlockTree,
    bool UsesTextVNode,
    bool UsesRenderList,
    bool UsesWithCtx,
    bool UsesCreateSlots,
    bool UsesMergeProps,
    bool UsesHandlerCache,
    bool UsesProps,
    bool UsesSlots,
    ImmutableArray<ImportDeclaration> ImportDeclarations,
    ImmutableArray<ISymbol> ReferenceCaptureStateMembers);

/// <summary>Represents an immutable expression allocated once at module scope.</summary>
internal sealed record RenderModuleHoist(
    string Name,
    Expression Initializer);

/// <summary>
/// Accumulates Vue runtime requirements discovered while lowering RenderTreeBuilder fragments
/// nested inside ordinary compiler members. The module builder owns the final imports because
/// these fragments share the component's Vue framing rather than producing standalone modules.
/// 普通成员片段与根 render 共用一个 Vue module，helper 导入必须在 framing 层统一落位。
/// </summary>
internal sealed class VueRenderRuntimeFeatures
{
    public bool UsesMergeProps { get; private set; }

    public bool UsesFragment { get; private set; }

    public bool UsesRawMarkupRuntime { get; private set; }

    public bool UsesSlots { get; private set; }

    public bool UsesBlockTree { get; private set; }

    public bool UsesTextVNode { get; private set; }

    public bool UsesRenderList { get; private set; }

    public bool UsesWithCtx { get; private set; }

    public bool UsesCreateSlots { get; private set; }

    public bool UsesHandlerCache { get; private set; }

    public bool UsesProps { get; private set; }

    public void Merge(
        bool usesMergeProps,
        bool usesFragment,
        bool usesRawMarkupRuntime,
        bool usesSlots,
        bool usesBlockTree,
        bool usesTextVNode,
        bool usesRenderList,
        bool usesWithCtx,
        bool usesCreateSlots,
        bool usesHandlerCache,
        bool usesProps)
    {
        UsesMergeProps |= usesMergeProps;
        UsesFragment |= usesFragment;
        UsesRawMarkupRuntime |= usesRawMarkupRuntime;
        UsesSlots |= usesSlots;
        UsesBlockTree |= usesBlockTree;
        UsesTextVNode |= usesTextVNode;
        UsesRenderList |= usesRenderList;
        UsesWithCtx |= usesWithCtx;
        UsesCreateSlots |= usesCreateSlots;
        UsesHandlerCache |= usesHandlerCache;
        UsesProps |= usesProps;
    }
}
