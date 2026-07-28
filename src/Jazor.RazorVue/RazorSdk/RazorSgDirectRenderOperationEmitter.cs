using System.Collections.Immutable;
using Acornima.Ast;
using Jazor.Common;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue.RazorSdk;

internal static class RazorSgDirectRenderOperationEmitter
{
    private const string RenderTreeBuilderMetadataName = "Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder";
    private const string ECMAScriptModuleAttributeMetadataName = "ECMAScript.ECMAScriptModuleAttribute";
    private const string VueLibraryComponentAttributeMetadataName = "ECMAScript.VueContract.VueLibraryComponentAttribute";
    private const string VuePropAttributeMetadataName = "ECMAScript.VueContract.VuePropAttribute";
    private const string VueLibraryEmitAttributeMetadataName = "ECMAScript.VueContract.VueLibraryEmitAttribute";
    private const string VueSlotAttributeMetadataName = "ECMAScript.VueContract.VueSlotAttribute";
    private static readonly SymbolEqualityComparer SymbolComparer = SymbolEqualityComparer.Default;

    public static bool TryEmit(
        Compilation compilation,
        INamedTypeSymbol componentSymbol,
        IMethodSymbol buildRenderTreeMethod,
        IBlockOperation buildRenderTreeBody,
        IReadOnlyDictionary<ISymbol, string>? declaredNames,
        out RazorSgDirectRenderOperationBuildResult result,
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

            var lowered = new Emitter(compilation, componentSymbol, declaredNames)
                .EmitBlock(buildRenderTreeBody, BuilderBinding.ForSymbol(buildRenderTreeMethod.Parameters[0]));
            result = new RazorSgDirectRenderOperationBuildResult(
                lowered.RenderExpression,
                lowered.PreludeLines,
                UsesFragment: lowered.UsesFragment,
                UsesStaticVNode: lowered.UsesStaticVNode,
                UsesProps: lowered.RenderExpression.Contains("props.", StringComparison.Ordinal) ||
                           lowered.PreludeLines.Any(static line => line.Contains("props.", StringComparison.Ordinal)),
                UsesSlots: lowered.UsesSlots,
                lowered.ImportLines);
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

    private sealed class Emitter
    {
        private readonly Compilation _compilation;
        private readonly INamedTypeSymbol _componentSymbol;
        private readonly SemanticWalker _walker;
        private readonly SenseArgument _argument;
        private readonly SortedDictionary<string, ImportBinding> _imports = new(StringComparer.Ordinal);
        private readonly List<string> _preludeLines = new();
        private readonly Dictionary<string, int> _localNameCounts = new(StringComparer.Ordinal);
        private readonly ImmutableDictionary<IPropertySymbol, string> _componentSlotNames;
        private readonly HashSet<IMethodSymbol> _activeRenderFragmentHelpers = new(SymbolComparer);
        private readonly HashSet<IMethodSymbol> _activeRenderObjectHelpers = new(SymbolComparer);
        private readonly Dictionary<IMethodSymbol, string> _renderFragmentHelperFunctionNames = new(SymbolComparer);
        private readonly HashSet<IMethodSymbol> _emittingRenderFragmentHelperFunctions = new(SymbolComparer);
        private bool _usesMergeProps;
        private bool _usesFragment;
        private bool _usesStaticVNode;
        private bool _usesSlots;

        public Emitter(
            Compilation compilation,
            INamedTypeSymbol componentSymbol,
            IReadOnlyDictionary<ISymbol, string>? declaredNames)
        {
            _compilation = compilation;
            _componentSymbol = componentSymbol;
            _walker = new SemanticWalker(test: false)
            {
                Host = new CurrentComponentSemanticWalkerHost(
                    componentSymbol,
                    parameterRuntimeNames: BuildComponentParameterNameMap(componentSymbol),
                    memberRuntimeNames: declaredNames)
            };
            _argument = new SenseArgument(Sense.Any, UseImportAliases: true);
            _componentSlotNames = BuildComponentSlotNameMap(componentSymbol);
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
                _preludeLines,
                AllowPreludeDeclarations: true);
            var state = new RenderState();
            _ = EmitOperations(block.Operations, context, state);
            if (state.Stack.Count != 0)
                throw Unsupported(block, "RazorVue direct render operation lowering found unclosed RenderTreeBuilder frames.");

            var renderExpression = state.ToRenderExpression();
            var usesFragment = _usesFragment || state.UsesFragment || state.Roots.Count > 1;
            var usesStaticVNode = _usesStaticVNode || state.UsesStaticVNode;
            return new LoweredRender(renderExpression, _preludeLines.ToImmutableArray(), usesFragment, usesStaticVNode, _usesSlots, BuildImportLines());
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
                    throw Unsupported(operation, "RazorVue direct render operation lowering only supports straight-line RenderTreeBuilder statements in this slice.");
            }
        }

        private void EmitExpressionStatement(IOperation expression, EmitContext context, RenderState state)
        {
            while (expression is IConversionOperation conversion)
                expression = conversion.Operand;

            if (expression is not IInvocationOperation invocation)
                throw Unsupported(expression, "RazorVue direct render operation lowering only supports invocation statements.");

            if (TryEmitHelperInvocation(invocation, context, state))
                return;

            if (TryEmitRenderFragmentInvoke(invocation, context, state))
                return;

            if (TryEmitRenderTreeBuilderInvocation(invocation, context, state))
                return;

            throw Unsupported(invocation, "RazorVue direct render operation lowering does not support invocation '" +
                                          invocation.TargetMethod.OriginalDefinition.ToDisplayString(Format.NameFormat) +
                                          "' in BuildRenderTree.");
        }

        private EmitContext EmitVariableDeclarationGroup(
            IVariableDeclarationGroupOperation declarationGroup,
            EmitContext context,
            RenderState state)
        {
            if (!context.AllowPreludeDeclarations || state.Stack.Count != 0 || state.Roots.Count != 0)
                throw Unsupported(declarationGroup, "Local declarations in direct render lowering are only supported before RenderTreeBuilder output begins.");

            var localAliases = context.LocalAliases;
            var localRenderFragments = context.LocalRenderFragments;
            var localRenderObjects = context.LocalRenderObjects;
            foreach (var declaration in declarationGroup.Declarations)
            {
                foreach (var declarator in declaration.Declarators)
                {
                    if (declarator.Initializer is null)
                        throw Unsupported(declarator, "Local declarations in direct render lowering must have an initializer.");

                    var localName = CreateUniqueLocalName(declarator.Symbol.Name);
                    var valueExpression = LowerExpression(declarator.Initializer.Value, context with
                    {
                        LocalAliases = localAliases
                    });
                    context.PreludeLines.Add("const " + localName + " = " + valueExpression + ";");
                    localAliases = localAliases.SetItem(declarator.Symbol, localName);
                    if (TryResolveRenderFragmentExpression(declarator.Initializer.Value, context, out var renderFragment))
                        localRenderFragments = localRenderFragments.SetItem(declarator.Symbol, renderFragment);
                    if (TryResolveRenderObjectExpression(declarator.Initializer.Value, context, out var renderObject))
                        localRenderObjects = localRenderObjects.SetItem(declarator.Symbol, renderObject);
                }
            }

            return context with
            {
                LocalAliases = localAliases,
                LocalRenderFragments = localRenderFragments,
                LocalRenderObjects = localRenderObjects
            };
        }

        private void EmitConditional(IConditionalOperation conditional, EmitContext context, RenderState state)
        {
            var condition = LowerExpression(conditional.Condition, context);
            if (TryEmitConditionalAttribute(conditional, condition, context, state))
                return;

            if (state.Stack.Count == 0 &&
                state.Roots.Count == 0 &&
                IsTerminatingWithoutOutput(conditional.WhenTrue) &&
                IsNoOutputOperation(conditional.WhenFalse))
            {
                state.AddGuard("!(" + condition + ")");
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
                ? new DirectRenderFragmentBody("null", UsesFragment: false, UsesStaticVNode: false)
                : EmitChildContentExpression(conditional.WhenFalse, context);
            state.AddChild(condition + " ? " + whenTrue.RenderExpression + " : " + whenFalse.RenderExpression);
            state.UsesFragment = state.UsesFragment || whenTrue.UsesFragment || whenFalse.UsesFragment;
            state.UsesStaticVNode = state.UsesStaticVNode || whenTrue.UsesStaticVNode || whenFalse.UsesStaticVNode;

            if (state.Stack.Count == 0 && IsTerminatingOperation(conditional.WhenTrue) && !IsTerminatingOperation(conditional.WhenFalse))
                state.AddGuard("!(" + condition + ")");
            else if (state.Stack.Count == 0 && !IsTerminatingOperation(conditional.WhenTrue) && IsTerminatingOperation(conditional.WhenFalse))
                state.AddGuard(condition);
        }

        private bool TryEmitConditionalAttribute(
            IConditionalOperation conditional,
            string condition,
            EmitContext context,
            RenderState state)
        {
            if (state.Stack.Count == 0 ||
                state.Stack.Peek() is not PropFrame frame ||
                frame.ChildrenStarted)
            {
                return false;
            }

            if (TryGetSingleAddAttributeInvocation(conditional.WhenTrue, out var whenTrueAttribute) &&
                IsNoOutputOperation(conditional.WhenFalse))
            {
                EmitConditionalAttribute(whenTrueAttribute, condition, conditionWhenPresent: true, context, frame);
                return true;
            }

            if (IsNoOutputOperation(conditional.WhenTrue) &&
                conditional.WhenFalse is not null &&
                TryGetSingleAddAttributeInvocation(conditional.WhenFalse, out var whenFalseAttribute))
            {
                EmitConditionalAttribute(whenFalseAttribute, condition, conditionWhenPresent: false, context, frame);
                return true;
            }

            return false;
        }

        private void EmitConditionalAttribute(
            IInvocationOperation invocation,
            string condition,
            bool conditionWhenPresent,
            EmitContext context,
            PropFrame frame)
        {
            EnsureSignature(invocation, invocation.Arguments.Length is 2 or 3);
            RequireOmittableSequence(invocation.Arguments[0].Value);
            if (!TryGetConstantString(invocation.Arguments[1].Value, out var name))
                throw Unsupported(invocation.Arguments[1].Value, "Attribute names must be compile-time strings for direct render lowering.");

            var value = invocation.Arguments.Length == 2
                ? "true"
                : LowerExpression(invocation.Arguments[2].Value, context);
            var guardedValue = conditionWhenPresent
                ? condition + " ? " + value + " : null"
                : condition + " ? null : " + value;
            frame.AddAttribute(new DirectAttribute(frame.NormalizeAttributeName(name), guardedValue));
        }

        private void EmitForEachLoop(IForEachLoopOperation forEachLoop, EmitContext context, RenderState state)
        {
            if (!TryResolveLoopControlVariable(forEachLoop.LoopControlVariable, out var loopVariable))
                throw Unsupported(forEachLoop, "Foreach direct render lowering requires a local loop variable.");

            var collection = LowerExpression(forEachLoop.Collection, context);
            var itemName = SanitizeJavaScriptIdentifierPart(loopVariable.Name, "item");
            var loopContext = context with
            {
                LocalAliases = context.LocalAliases.SetItem(loopVariable, itemName)
            };
            var body = EmitChildContentExpression(forEachLoop.Body, loopContext);
            state.AddChild("Array.from(" + collection + " ?? [], " + itemName + " => " + body.RenderExpression + ")");
            state.UsesFragment = state.UsesFragment || body.UsesFragment;
            state.UsesStaticVNode = state.UsesStaticVNode || body.UsesStaticVNode;
        }

        private DirectRenderFragmentBody EmitChildContentExpression(IOperation operation, EmitContext context)
        {
            return WithScopedLocalNames(() =>
            {
                var childState = new RenderState();
                var preludeLines = new List<string>();
                _ = EmitOperation(operation, context with
                {
                    PreludeLines = preludeLines,
                    AllowPreludeDeclarations = true
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
                    WrapWithPrelude(preludeLines, childState.ToRenderExpression()),
                    childState.UsesFragment,
                    childState.UsesStaticVNode);
            });
        }

        private DirectRenderFragmentBody EmitRenderFragmentBodyExpression(
            IParameterSymbol builder,
            IOperation body,
            EmitContext context,
            IOperation sourceOperation,
            string description)
        {
            return WithScopedLocalNames(() =>
            {
                var slotState = new RenderState();
                var preludeLines = new List<string>();
                _ = EmitOperation(
                    body,
                    new EmitContext(
                    BuilderBinding.ForSymbol(builder),
                    context.Substitutions,
                    context.ParameterAliases,
                    context.LocalAliases,
                    context.LocalRenderFragments,
                    context.LocalRenderObjects,
                        preludeLines,
                        AllowPreludeDeclarations: true),
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
                    WrapWithPrelude(preludeLines, slotState.ToRenderExpression()),
                    usesFragment,
                    slotState.UsesStaticVNode);
            });
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
                    state.Stack.Push(new ElementFrame(tagExpression, tagName));
                    return true;

                case "CloseElement":
                    EnsureSignature(invocation, method.Parameters.Length == 0);
                    state.Close<ElementFrame>(invocation);
                    return true;

                case "OpenComponent":
                    EnsureSignature(invocation, method.Name == "OpenComponent");
                    RequireOmittableSequence(invocation.Arguments[0].Value);
                    var componentType = ResolveOpenComponentType(invocation);
                    var componentExpression = BindComponentImport(componentType);
                    var parameterNameMap = BuildComponentParameterNameMap(componentType);
                    state.StartChildren();
                    state.Stack.Push(new ComponentFrame(componentExpression, parameterNameMap));
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
                    var markup = LowerExpression(invocation.Arguments[1].Value, context);
                    state.UsesStaticVNode = true;
                    state.AddChild("createStaticVNode(" + markup + ", 1)");
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
                substitutions[method.Parameters[index]] = invocation.Arguments[index].Value;

            var helperContext = new EmitContext(
                BuilderBinding.ForSymbol(method.Parameters[0]),
                substitutions.ToImmutable(),
                context.ParameterAliases,
                context.LocalAliases,
                context.LocalRenderFragments,
                context.LocalRenderObjects,
                context.PreludeLines,
                AllowPreludeDeclarations: false);
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

            state.AddChild(expression.RenderExpression);
            state.UsesFragment = state.UsesFragment || expression.UsesFragment;
            state.UsesStaticVNode = state.UsesStaticVNode || expression.UsesStaticVNode;
            return true;
        }

        private void EmitAddAttribute(IInvocationOperation invocation, EmitContext context, RenderState state)
        {
            EnsureSignature(invocation, invocation.Arguments.Length is 2 or 3);
            RequireOmittableSequence(invocation.Arguments[0].Value);
            if (state.Stack.Count == 0 || state.Stack.Peek() is not PropFrame frame || frame.ChildrenStarted)
                throw Unsupported(invocation, "Attributes must be added before children on an open element or component: " + invocation.Syntax);

            var nameOperation = invocation.Arguments[1].Value;
            if (!TryGetConstantString(nameOperation, out var name))
                throw Unsupported(nameOperation, "Attribute names must be compile-time strings for direct render lowering.");

            var value = invocation.Arguments.Length == 2
                ? "true"
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

            if (IsGenericRenderFragmentOperationValue(invocation.Arguments[2].Value))
                throw Unsupported(invocation.Arguments[2].Value, "Generic RenderFragment<T> scoped slots are not supported by direct render operation lowering yet.");

            if (TryGetRenderFragmentBody(invocation.Arguments[2].Value, out var slotBuilder, out var slotBody))
            {
                var slot = EmitRenderFragmentBodyExpression(slotBuilder, slotBody, context, invocation.Arguments[2].Value, "RenderFragment slot '" + name + "'");
                frame.Slots.Add(new DirectSlot(frame.NormalizeSlotName(name), null, slot.RenderExpression));
                state.UsesFragment = state.UsesFragment || slot.UsesFragment;
                state.UsesStaticVNode = state.UsesStaticVNode || slot.UsesStaticVNode;
                return;
            }

            if (TryResolveRenderFragmentContentExpression(invocation.Arguments[2].Value, context, out var forwardedSlotExpression))
            {
                frame.Slots.Add(new DirectSlot(frame.NormalizeSlotName(name), null, forwardedSlotExpression.RenderExpression));
                state.UsesFragment = state.UsesFragment || forwardedSlotExpression.UsesFragment;
                state.UsesStaticVNode = state.UsesStaticVNode || forwardedSlotExpression.UsesStaticVNode;
                return;
            }

            if (string.Equals(name, "ChildContent", StringComparison.Ordinal))
                throw Unsupported(invocation, "ChildContent component parameter must be a RenderFragment for direct render lowering.");

            if (IsRenderFragmentOperationValue(invocation.Arguments[2].Value))
                throw Unsupported(invocation.Arguments[2].Value, "RenderFragment component parameters must be inline lambdas for direct render lowering.");

            frame.AddAttribute(new DirectAttribute(
                frame.NormalizeAttributeName(name),
                LowerExpression(invocation.Arguments[2].Value, context)));
        }

        private void EmitAddMultipleAttributes(IInvocationOperation invocation, EmitContext context, RenderState state)
        {
            EnsureSignature(invocation, invocation.Arguments.Length == 2);
            RequireOmittableSequence(invocation.Arguments[0].Value);
            if (state.Stack.Count == 0 || state.Stack.Peek() is not PropFrame frame || frame.ChildrenStarted)
                throw Unsupported(invocation, "Multiple attributes must be added before children on an open element or component.");

            if (TryEmitKnownMultipleAttributes(invocation.Arguments[1].Value, context, frame))
                return;

            frame.AddMultipleAttributes(LowerExpression(invocation.Arguments[1].Value, context));
            _usesMergeProps = true;
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
            if (!string.Equals(value, "false", StringComparison.Ordinal))
                frame.SetEventModifier(eventName, preventDefault, stopPropagation);
        }

        private static void EmitAddNamedEvent(IInvocationOperation invocation, RenderState state)
        {
            var offset = GetRenderTreeBuilderReceiverArgumentOffset(invocation);
            EnsureSignature(invocation, invocation.Arguments.Length - offset == 2);
            if (state.Stack.Count == 0 || state.Stack.Peek() is not ElementFrame frame || frame.ChildrenStarted)
                throw Unsupported(invocation, "Named event metadata must target an open element before children.");

            if (!TryGetConstantString(invocation.Arguments[offset].Value, out var eventName) ||
                !TryGetConstantString(invocation.Arguments[offset + 1].Value, out var assignedEventName) ||
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
            if (IsGenericRenderFragmentOperationValue(invocation.Arguments[1].Value))
                throw Unsupported(invocation.Arguments[1].Value, "Generic RenderFragment<T> content is not supported by direct render operation lowering yet.");

            if (TryGetRenderFragmentBody(invocation.Arguments[1].Value, out var slotBuilder, out var slotBody))
            {
                var slot = EmitRenderFragmentBodyExpression(slotBuilder, slotBody, context, invocation.Arguments[1].Value, "RenderFragment content");
                state.AddChild(slot.RenderExpression);
                state.UsesFragment = state.UsesFragment || slot.UsesFragment;
                state.UsesStaticVNode = state.UsesStaticVNode || slot.UsesStaticVNode;
                return;
            }

            if (TryResolveRenderFragmentContentExpression(invocation.Arguments[1].Value, context, out var slotExpression))
            {
                state.AddChild(slotExpression.RenderExpression);
                state.UsesFragment = state.UsesFragment || slotExpression.UsesFragment;
                state.UsesStaticVNode = state.UsesStaticVNode || slotExpression.UsesStaticVNode;
                return;
            }

            if (IsRenderFragmentOperationValue(invocation.Arguments[1].Value))
                throw Unsupported(invocation.Arguments[1].Value, "RenderFragment content must be an inline lambda for direct render lowering.");

            state.AddChild(LowerExpression(invocation.Arguments[1].Value, context));
        }

        private bool TryEmitKnownMultipleAttributes(IOperation operation, EmitContext context, PropFrame frame)
        {
            while (operation is IConversionOperation conversion)
                operation = conversion.Operand;

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

        private string LowerExpression(IOperation operation, EmitContext context)
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
                return parameterAlias;
            }

            if (operation is ILocalReferenceOperation localReference &&
                context.LocalAliases.TryGetValue(localReference.Local, out var localAlias))
            {
                return localAlias;
            }

            var node = _walker.Visit(operation, _argument)
                ?? throw Unsupported(operation, "Expression did not produce a JavaScript node.");
            if (node is not Expression)
                throw Unsupported(operation, "Expression did not lower to a JavaScript expression.");

            return node.ToKnRECMAScript().Trim();
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
            while (operation is IConversionOperation conversion)
                operation = conversion.Operand;

            if (operation is IParameterReferenceOperation parameterReference &&
                context.Substitutions.TryGetValue(parameterReference.Parameter, out var substituted))
            {
                return TryResolveRenderFragmentExpression(substituted, context, out renderFragment);
            }

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
                renderFragment = new DirectRenderFragment(
                    condition + " ? " + whenTrue.RenderExpression + " : " + whenFalse.RenderExpression,
                    whenTrue.UsesFragment || whenFalse.UsesFragment,
                    whenTrue.UsesStaticVNode || whenFalse.UsesStaticVNode);
                return true;
            }

            if (operation.ConstantValue.HasValue && operation.ConstantValue.Value is null)
            {
                renderFragment = new DirectRenderFragment("null");
                return true;
            }

            if (operation is IInvocationOperation invocation &&
                TryResolveRenderFragmentHelperInvocation(invocation, context, out renderFragment))
            {
                return true;
            }

            if (operation is IPropertyReferenceOperation propertyReference)
            {
                var property = propertyReference.Property.OriginalDefinition;
                if (_componentSlotNames.TryGetValue(property, out var propertySlotName))
                {
                    _usesSlots = true;
                    renderFragment = new DirectRenderFragment(BuildSlotInvocationExpression(propertySlotName));
                    return true;
                }

                if (propertyReference.Instance is not null &&
                    TryResolveRenderObjectExpression(propertyReference.Instance, context, out var renderObject) &&
                    renderObject.RenderFragments.TryGetValue(property, out var objectRenderFragment))
                {
                    renderFragment = objectRenderFragment;
                    return true;
                }
            }

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
                !IsCurrentComponentMethod(method))
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
                    substitutions[method.Parameters[index]] = invocation.Arguments[index].Value;

                var helperContext = new EmitContext(
                    context.Builder,
                    substitutions.ToImmutable(),
                    context.ParameterAliases,
                    context.LocalAliases,
                    context.LocalRenderFragments,
                    context.LocalRenderObjects,
                    context.PreludeLines,
                    AllowPreludeDeclarations: false);

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
                        IsRenderFragmentType(propertyReference.Property.Type) &&
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
                    !IsRenderFragmentType(propertyReference.Property.Type))
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
            if (!IsRenderFragmentType(method.ReturnType) ||
                method.DeclaringSyntaxReferences.Length != 1 ||
                !IsCurrentComponentMethod(method))
            {
                return false;
            }

            if (!TryGetReturnedRenderFragmentBody(method, out var builder, out var body))
                return false;

            if (_activeRenderFragmentHelpers.Contains(method.OriginalDefinition) ||
                ContainsMethodInvocation(body, method))
            {
                var helper = EnsureRenderFragmentHelperFunction(method, builder, body, context);
                var arguments = invocation.Arguments
                    .Select(argument => LowerExpression(argument.Value, context))
                    .ToArray();
                renderFragment = new DirectRenderFragment(
                    helper.FunctionName + "(" + string.Join(", ", arguments) + ")",
                    helper.UsesFragment,
                    helper.UsesStaticVNode);
                return true;
            }

            if (!_activeRenderFragmentHelpers.Add(method.OriginalDefinition))
                throw Unsupported(invocation, "Recursive RenderFragment helper '" + method.Name + "' is not supported by direct render operation lowering yet.");

            var substitutions = context.Substitutions.ToBuilder();
            for (var index = 0; index < invocation.Arguments.Length && index < method.Parameters.Length; index++)
                substitutions[method.Parameters[index]] = invocation.Arguments[index].Value;

            try
            {
                renderFragment = WithScopedLocalNames(() =>
                {
                    var fragmentState = new RenderState();
                    var preludeLines = new List<string>();
                    _ = EmitOperation(
                        body,
                        new EmitContext(
                            BuilderBinding.ForSymbol(builder),
                            substitutions.ToImmutable(),
                            context.ParameterAliases,
                            context.LocalAliases,
                            context.LocalRenderFragments,
                            context.LocalRenderObjects,
                            preludeLines,
                            AllowPreludeDeclarations: true),
                        fragmentState);
                    if (fragmentState.Stack.Count != 0)
                        throw Unsupported(invocation, "RenderFragment helper '" + method.Name + "' left unclosed " + fragmentState.Stack.Peek().Describe() + " frames.");

                    return new DirectRenderFragment(
                        WrapWithPrelude(preludeLines, fragmentState.ToRenderExpression()),
                        fragmentState.UsesFragment || fragmentState.Roots.Count > 1,
                        fragmentState.UsesStaticVNode);
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
            IParameterSymbol builder,
            IOperation body,
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

                var lowered = WithScopedLocalNames(() =>
                {
                    var functionState = new RenderState();
                    var preludeLines = new List<string>();
                    _ = EmitOperation(
                        body,
                        new EmitContext(
                            BuilderBinding.ForSymbol(builder),
                            context.Substitutions,
                            parameterAliases.ToImmutable(),
                            ImmutableDictionary<ILocalSymbol, string>.Empty.WithComparers(SymbolComparer),
                            context.LocalRenderFragments,
                            context.LocalRenderObjects,
                            preludeLines,
                            AllowPreludeDeclarations: true),
                        functionState);
                    if (functionState.Stack.Count != 0)
                        throw Unsupported(body, "RenderFragment helper '" + method.Name + "' left unclosed " + functionState.Stack.Peek().Describe() + " frames.");

                    return new DirectRenderFragmentBody(
                        WrapWithPrelude(preludeLines, functionState.ToRenderExpression()),
                        functionState.UsesFragment || functionState.Roots.Count > 1,
                        functionState.UsesStaticVNode);
                });

                _usesFragment = _usesFragment || lowered.UsesFragment;
                _usesStaticVNode = _usesStaticVNode || lowered.UsesStaticVNode;
                context.PreludeLines.Add(
                    "function " +
                    functionName +
                    "(" +
                    string.Join(", ", parameterNames) +
                    ") { return " +
                    lowered.RenderExpression +
                    "; }");
                return new DirectRenderFunction(functionName, lowered.UsesFragment, lowered.UsesStaticVNode);
            }
            finally
            {
                _emittingRenderFragmentHelperFunctions.Remove(originalMethod);
            }
        }

        private bool IsCurrentComponentMethod(IMethodSymbol method)
            => SymbolComparer.Equals(method.ContainingType?.OriginalDefinition, _componentSymbol.OriginalDefinition);

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
            out IParameterSymbol builder,
            out IOperation body)
        {
            builder = null!;
            body = null!;
            var syntax = method.DeclaringSyntaxReferences[0].GetSyntax();
            var model = _compilation.GetSemanticModel(syntax.SyntaxTree);
            IOperation? returnedOperation = syntax switch
            {
                MethodDeclarationSyntax { ExpressionBody.Expression: { } expression } => model.GetOperation(expression),
                MethodDeclarationSyntax { Body: { } methodBody } => TryGetSingleReturnValue(model.GetOperation(methodBody)),
                _ => null
            };

            return returnedOperation is not null &&
                   TryGetRenderFragmentBody(returnedOperation, out builder, out body);
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

        private string BindComponentImport(INamedTypeSymbol componentType)
        {
            var descriptor = ResolveComponentImport(componentType);
            var key = descriptor.ImportSpecifier + "\0" + descriptor.ExportName;
            if (_imports.TryGetValue(key, out var existing))
                return existing.LocalName;

            var localName = "i$" + Format.HashName(key).TrimStart('_');
            _imports.Add(key, new ImportBinding(descriptor.ImportSpecifier, descriptor.ExportName, localName));
            return localName;
        }

        private ImmutableArray<string> BuildImportLines()
        {
            var lines = ImmutableArray.CreateBuilder<string>();
            if (_usesMergeProps)
                lines.Add("import { mergeProps } from \"vue\";");

            foreach (var pair in _argument.FlushImportSpecifiers())
            {
                var specifiers = string.Join(", ", pair.Value.Select(static specifier => specifier.ToKnRECMAScript().Trim()));
                lines.Add("import { " + specifiers + " } from \"" + pair.Key + "\";");
            }

            foreach (var binding in _imports.Values)
            {
                var specifier = string.Equals(binding.ExportName, "default", StringComparison.Ordinal)
                    ? "default as " + binding.LocalName
                    : binding.ExportName + " as " + binding.LocalName;
                lines.Add("import { " + specifier + " } from \"" + binding.ImportSpecifier + "\";");
            }

            return lines.ToImmutable();
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
            try
            {
                return action();
            }
            finally
            {
                _localNameCounts.Clear();
                foreach (var pair in snapshot)
                    _localNameCounts.Add(pair.Key, pair.Value);
            }
        }

        private static string WrapWithPrelude(IReadOnlyList<string> preludeLines, string expression)
        {
            if (preludeLines.Count == 0)
                return expression;

            return "(() => { " + string.Join(" ", preludeLines) + " return " + expression + "; })()";
        }
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

    private static bool TryGetSingleAddAttributeInvocation(
        IOperation operation,
        out IInvocationOperation invocation)
    {
        invocation = null!;
        if (operation is IBlockOperation block)
        {
            if (block.Operations.Length != 1)
                return false;

            operation = block.Operations[0];
        }

        if (operation is IExpressionStatementOperation statement)
            operation = statement.Operation;

        while (operation is IConversionOperation conversion)
            operation = conversion.Operand;

        if (operation is not IInvocationOperation candidate ||
            !IsRenderTreeBuilderMethod(candidate.TargetMethod) ||
            !string.Equals(candidate.TargetMethod.OriginalDefinition.Name, "AddAttribute", StringComparison.Ordinal))
        {
            return false;
        }

        invocation = candidate;
        return true;
    }

    private static INamedTypeSymbol ResolveOpenComponentType(IInvocationOperation invocation)
    {
        if (invocation.TargetMethod.TypeArguments.Length == 1 &&
            invocation.TargetMethod.TypeArguments[0] is INamedTypeSymbol genericComponentType)
        {
            return genericComponentType;
        }

        if (invocation.Arguments.Length == 2 &&
            TryResolveTypeOfExpression(invocation.Arguments[1].Value, out var componentType))
        {
            return componentType;
        }

        throw Unsupported(invocation, "OpenComponent must use a generic component type or typeof(T) for direct render lowering.");
    }

    private static bool TryResolveTypeOfExpression(IOperation operation, out INamedTypeSymbol componentType)
    {
        componentType = null!;
        while (operation is IConversionOperation conversion)
            operation = conversion.Operand;

        if (operation is ITypeOfOperation { TypeOperand: INamedTypeSymbol namedType })
        {
            componentType = namedType;
            return true;
        }

        return false;
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
                return new ComponentImportDescriptor(importSpecifier.Trim(), exportName.Trim());
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
    {
        var names = ImmutableDictionary.CreateBuilder<string, string>(StringComparer.Ordinal);
        foreach (var attribute in componentType.GetAttributes())
        {
            var attributeName = attribute.AttributeClass?.ToDisplayString();
            if (string.Equals(attributeName, VuePropAttributeMetadataName, StringComparison.Ordinal))
            {
                AddDescriptorName(attribute, names, listener: false);
                continue;
            }

            if (string.Equals(attributeName, VueLibraryEmitAttributeMetadataName, StringComparison.Ordinal))
            {
                AddDescriptorName(attribute, names, listener: true);
                continue;
            }

            if (string.Equals(attributeName, VueSlotAttributeMetadataName, StringComparison.Ordinal))
                AddSlotDescriptorName(attribute, names);
        }

        return names.ToImmutable();
    }

    private static ImmutableDictionary<IPropertySymbol, string> BuildComponentSlotNameMap(INamedTypeSymbol componentType)
    {
        var names = ImmutableDictionary.CreateBuilder<IPropertySymbol, string>(SymbolComparer);
        var descriptorNames = BuildComponentParameterNameMap(componentType);
        for (var current = componentType; current is not null; current = current.BaseType)
        {
            foreach (var member in current.GetMembers())
            {
                if (member is not IPropertySymbol property ||
                    !IsRenderFragmentType(property.Type))
                {
                    continue;
                }

                var publicName = property.Name;
                var slotName = descriptorNames.TryGetValue(publicName, out var descriptorName)
                    ? descriptorName
                    : string.Equals(publicName, "ChildContent", StringComparison.Ordinal)
                        ? "default"
                        : NormalizeDirectComponentParameterName(publicName);
                if (!string.IsNullOrWhiteSpace(slotName))
                    names[property.OriginalDefinition] = slotName;
            }
        }

        return names.ToImmutable();
    }

    private static bool IsRenderFragmentType(ITypeSymbol type)
        => type is INamedTypeSymbol named &&
           !named.IsGenericType &&
           string.Equals(named.OriginalDefinition.ToDisplayString(), "Microsoft.AspNetCore.Components.RenderFragment", StringComparison.Ordinal);

    private static void AddDescriptorName(
        AttributeData attribute,
        ImmutableDictionary<string, string>.Builder names,
        bool listener)
    {
        if (attribute.ConstructorArguments.Length == 0 ||
            attribute.ConstructorArguments[0].Value is not string publicName ||
            string.IsNullOrWhiteSpace(publicName))
        {
            return;
        }

        var name = GetNamedString(attribute, "Name");
        if (string.IsNullOrWhiteSpace(name))
            return;

        names[publicName] = listener ? ToVueListenerPropName(name!) : name!;
    }

    private static void AddSlotDescriptorName(
        AttributeData attribute,
        ImmutableDictionary<string, string>.Builder names)
    {
        if (attribute.ConstructorArguments.Length == 0 ||
            attribute.ConstructorArguments[0].Value is not string publicName ||
            string.IsNullOrWhiteSpace(publicName))
        {
            return;
        }

        if (GetNamedBoolean(attribute, "PatternOnly") == true)
            return;

        var name = GetNamedBoolean(attribute, "IsDefault") == true
            ? "default"
            : GetNamedString(attribute, "Name");
        if (!string.IsNullOrWhiteSpace(name))
            names[publicName] = name!;
    }

    private static string? GetNamedString(AttributeData attribute, string name)
        => attribute.NamedArguments
            .FirstOrDefault(argument => string.Equals(argument.Key, name, StringComparison.Ordinal))
            .Value.Value as string;

    private static bool? GetNamedBoolean(AttributeData attribute, string name)
    {
        foreach (var argument in attribute.NamedArguments)
        {
            if (string.Equals(argument.Key, name, StringComparison.Ordinal) &&
                argument.Value.Value is bool value)
            {
                return value;
            }
        }

        return null;
    }

    private static string ToVueListenerPropName(string eventName)
        => eventName.StartsWith("on", StringComparison.Ordinal) &&
           eventName.Length > 2 &&
           char.IsUpper(eventName[2])
            ? eventName
            : "on" + char.ToUpperInvariant(eventName[0]) + eventName.Substring(1);

    private static string BuildSlotInvocationExpression(string slotName)
    {
        var slotAccess = FormatSlotAccessExpression(slotName);
        return "(typeof " + slotAccess + " === \"function\" ? " + slotAccess + "() : null)";
    }

    private static string FormatSlotAccessExpression(string slotName)
        => IsIdentifierName(slotName)
            ? "slots." + slotName
            : "slots[" + "\"" + EscapeJavaScriptString(slotName) + "\"" + "]";

    private static string BuildDirectDomBindHandler(string handlerExpression, string attributeName)
    {
        var escapedAttributeName = "\"" + EscapeJavaScriptString(attributeName) + "\"";
        return "(eventOrValue, ...args) => { const value = eventOrValue !== null && eventOrValue !== undefined && typeof eventOrValue === \"object\" && eventOrValue.target !== null && eventOrValue.target !== undefined && " +
               escapedAttributeName +
               " in eventOrValue.target ? eventOrValue.target[" +
               escapedAttributeName +
               "] : eventOrValue; return (" +
               handlerExpression +
               ")(value, ...args); }";
    }

    private static string BuildDirectEventModifierHandler(string handlerExpression, DirectEventModifier modifier)
    {
        var statements = new List<string>();
        if (modifier.PreventDefault)
            statements.Add("event?.preventDefault?.();");
        if (modifier.StopPropagation)
            statements.Add("event?.stopPropagation?.();");
        statements.Add("return (" + handlerExpression + ")(event, ...args);");
        return "(event, ...args) => { " + string.Join(" ", statements) + " }";
    }

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

    private sealed class RenderState
    {
        public List<string> Roots { get; } = new();

        public Stack<Frame> Stack { get; } = new();

        private List<string> Guards { get; } = new();

        public bool UsesFragment { get; set; }

        public bool UsesStaticVNode { get; set; }

        public string ToRenderExpression()
        {
            return Roots.Count switch
            {
                0 => "null",
                1 => Roots[0],
                _ => "h(Fragment, null, [" + string.Join(", ", Roots) + "])"
            };
        }

        public void StartChildren()
        {
            if (Stack.Count > 0)
                Stack.Peek().ChildrenStarted = true;
        }

        public void AddChild(string expression)
        {
            if (Stack.Count == 0)
            {
                Roots.Add(ApplyGuards(expression));
                return;
            }

            Stack.Peek().Children.Add(expression);
        }

        public void AddGuard(string expression)
        {
            Guards.Add(expression);
        }

        private string ApplyGuards(string expression)
        {
            if (Guards.Count == 0)
                return expression;

            var guard = Guards.Count == 1
                ? Guards[0]
                : string.Join(" && ", Guards.Select(static item => "(" + item + ")"));
            return guard + " ? " + expression + " : null";
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

    private abstract class Frame
    {
        public bool ChildrenStarted { get; set; }

        public List<string> Children { get; } = new();

        public abstract string ToRenderExpression();

        public virtual string Describe()
            => GetType().Name;
    }

    private abstract class PropFrame : Frame
    {
        private readonly List<DirectAttribute> _attributes = new();
        private readonly List<string> _multipleAttributes = new();
        private string? _lastAttributeName;

        protected IReadOnlyList<DirectAttribute> Attributes
            => _attributes;

        public void AddAttribute(DirectAttribute attribute)
        {
            for (var index = _attributes.Count - 1; index >= 0; index--)
            {
                if (string.Equals(_attributes[index].Name, attribute.Name, StringComparison.Ordinal))
                {
                    _attributes[index] = attribute;
                    _lastAttributeName = attribute.Name;
                    return;
                }
            }

            _attributes.Add(attribute);
            _lastAttributeName = attribute.Name;
        }

        public void AddMultipleAttributes(string attributesExpression)
        {
            if (string.Equals(attributesExpression, "null", StringComparison.Ordinal) ||
                string.Equals(attributesExpression, "undefined", StringComparison.Ordinal))
            {
                return;
            }

            _multipleAttributes.Add(attributesExpression);
            _lastAttributeName = null;
        }

        public bool TrySetLastAttributeValue(string valueExpression)
        {
            if (_lastAttributeName is null)
                return false;

            for (var index = _attributes.Count - 1; index >= 0; index--)
            {
                if (string.Equals(_attributes[index].Name, _lastAttributeName, StringComparison.Ordinal))
                {
                    _attributes[index] = _attributes[index] with { ValueExpression = valueExpression };
                    return true;
                }
            }

            return false;
        }

        public abstract string NormalizeAttributeName(string name);

        protected virtual string FormatAttributeValueExpression(DirectAttribute attribute)
            => attribute.ValueExpression;

        protected string FormatPropsExpression()
        {
            var explicitProps = _attributes.Count == 0
                ? "null"
                : "{ " + string.Join(", ", _attributes.Select(attribute =>
                    FormatJavaScriptPropertyName(attribute.Name) + ": " + FormatAttributeValueExpression(attribute))) + " }";
            if (_multipleAttributes.Count == 0)
                return explicitProps;

            var arguments = new List<string>(_multipleAttributes.Count + 1)
            {
                explicitProps
            };
            arguments.AddRange(_multipleAttributes);
            return "mergeProps(" + string.Join(", ", arguments) + ")";
        }
    }

    private sealed class ElementFrame : PropFrame
    {
        private readonly string _tagExpression;
        private readonly Dictionary<string, DirectEventModifier> _eventModifiers = new(StringComparer.Ordinal);
        private string? _updatesAttributeName;
        private string? _updatesEventName;

        public ElementFrame(string tagExpression, string tagName)
        {
            _tagExpression = tagExpression;
            TagName = tagName;
        }

        public string TagName { get; }

        public override string NormalizeAttributeName(string name)
            => NormalizeDirectElementAttributeName(name);

        public override string ToRenderExpression()
            => "h(" + _tagExpression + ", " + FormatPropsExpression() + ", " + FormatChildrenExpression() + ")";

        public override string Describe()
            => "ElementFrame('" + TagName + "')";

        public void SetUpdatesAttributeName(string name)
        {
            _updatesAttributeName = name;
            _updatesEventName = Attributes
                .LastOrDefault(static attribute => IsDirectEventAttributeName(attribute.Name))
                ?.Name;
        }

        public void SetEventModifier(string eventName, bool preventDefault, bool stopPropagation)
        {
            var runtimeName = NormalizeDirectElementAttributeName(eventName);
            _eventModifiers.TryGetValue(runtimeName, out var existing);
            _eventModifiers[runtimeName] = new DirectEventModifier(
                existing.PreventDefault || preventDefault,
                existing.StopPropagation || stopPropagation);
        }

        protected override string FormatAttributeValueExpression(DirectAttribute attribute)
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

        private string FormatChildrenExpression()
            => Children.Count == 0 ? "null" : "[" + string.Join(", ", Children) + "]";
    }

    private sealed class ComponentFrame : PropFrame
    {
        private readonly string _componentExpression;
        private readonly ImmutableDictionary<string, string> _parameterNameMap;

        public ComponentFrame(
            string componentExpression,
            ImmutableDictionary<string, string> parameterNameMap)
        {
            _componentExpression = componentExpression;
            _parameterNameMap = parameterNameMap;
        }

        public List<DirectSlot> Slots { get; } = new();

        public override string NormalizeAttributeName(string name)
            => _parameterNameMap.TryGetValue(name, out var mapped)
                ? mapped
                : NormalizeDirectComponentParameterName(name);

        public string NormalizeSlotName(string name)
        {
            if (_parameterNameMap.TryGetValue(name, out var mapped))
                return mapped;

            return string.Equals(name, "ChildContent", StringComparison.Ordinal)
                ? "default"
                : NormalizeDirectComponentParameterName(name);
        }

        public override string ToRenderExpression()
        {
            var props = FormatPropsExpression();
            if (Slots.Count > 0)
            {
                var slots = "{ " + string.Join(", ", Slots.Select(slot =>
                    FormatJavaScriptPropertyName(slot.Name) + ": () => " + slot.RenderExpression)) + " }";
                return "h(" + _componentExpression + ", " + props + ", " + slots + ")";
            }

            if (Children.Count == 0)
                return "h(" + _componentExpression + ", " + props + ")";

            var children = Children.Count == 1
                ? Children[0]
                : "[" + string.Join(", ", Children) + "]";
            return "h(" + _componentExpression + ", " + props + ", " + children + ")";
        }
    }

    private sealed class RegionFrame : Frame
    {
        public override string ToRenderExpression()
            => Children.Count switch
            {
                0 => "null",
                1 => Children[0],
                _ => "h(Fragment, null, [" + string.Join(", ", Children) + "])"
            };
    }

    private sealed record EmitContext(
        BuilderBinding Builder,
        ImmutableDictionary<IParameterSymbol, IOperation> Substitutions,
        ImmutableDictionary<IParameterSymbol, string> ParameterAliases,
        ImmutableDictionary<ILocalSymbol, string> LocalAliases,
        ImmutableDictionary<ILocalSymbol, DirectRenderFragment> LocalRenderFragments,
        ImmutableDictionary<ILocalSymbol, DirectRenderObject> LocalRenderObjects,
        List<string> PreludeLines,
        bool AllowPreludeDeclarations,
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

    private sealed record LoweredRender(
        string RenderExpression,
        ImmutableArray<string> PreludeLines,
        bool UsesFragment,
        bool UsesStaticVNode,
        bool UsesSlots,
        ImmutableArray<string> ImportLines);

    private sealed record ImportBinding(
        string ImportSpecifier,
        string ExportName,
        string LocalName);

    private readonly record struct ComponentImportDescriptor(
        string ImportSpecifier,
        string ExportName);

    private sealed record DirectAttribute(
        string Name,
        string ValueExpression);

    private sealed record DirectSlot(
        string Name,
        string? ParameterName,
        string RenderExpression);

    private readonly record struct DirectRenderFragment(
        string RenderExpression,
        bool UsesFragment = false,
        bool UsesStaticVNode = false);

    private readonly record struct DirectRenderFunction(
        string FunctionName,
        bool UsesFragment,
        bool UsesStaticVNode);

    private sealed record DirectRenderObject(
        ImmutableDictionary<IPropertySymbol, DirectRenderFragment> RenderFragments);

    private readonly record struct DirectRenderFragmentBody(
        string RenderExpression,
        bool UsesFragment,
        bool UsesStaticVNode);

    private readonly record struct DirectEventModifier(
        bool PreventDefault,
        bool StopPropagation);

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

    private static string NormalizeDirectComponentParameterName(string name)
        => string.IsNullOrEmpty(name)
            ? name
            : char.ToLowerInvariant(name[0]) + name.Substring(1);

    private static string FormatJavaScriptPropertyName(string name)
        => IsIdentifierName(name) ? name : "\"" + EscapeJavaScriptString(name) + "\"";

    private static bool IsIdentifierName(string value)
    {
        if (string.IsNullOrEmpty(value) ||
            !(char.IsLetter(value[0]) || value[0] == '_' || value[0] == '$'))
        {
            return false;
        }

        for (var index = 1; index < value.Length; index++)
        {
            var ch = value[index];
            if (!(char.IsLetterOrDigit(ch) || ch == '_' || ch == '$'))
                return false;
        }

        return true;
    }

    private static string EscapeJavaScriptString(string value)
        => value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\r", "\\r")
            .Replace("\n", "\\n");
}

internal sealed record RazorSgDirectRenderOperationBuildResult(
    string RenderExpression,
    ImmutableArray<string> PreludeLines,
    bool UsesFragment,
    bool UsesStaticVNode,
    bool UsesProps,
    bool UsesSlots,
    ImmutableArray<string> ImportLines);
