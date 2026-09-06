using System.Collections.Immutable;
using Jazor.Common;
using Jazor.Compiler;
using Jazor.RazorVue.Generation;
using Acornima.Ast;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue.RazorSdk;

/// <summary>
/// Computes the component members required by a render method before Vue module framing.
/// Keeping this closure explicit prevents the module builder from re-discovering compiler semantics.
/// 生命周期根也纳入同一闭包，使 setup 函数、state 与 dispose 逻辑来自确定的最小成员集合。
/// </summary>
internal static class MemberClosureBuilder
{
    private static readonly SymbolEqualityComparer Comparer = SymbolEqualityComparer.Default;
    private const string ComponentBaseMetadataName = "Microsoft.AspNetCore.Components.ComponentBase";
    private const string ParameterAttributeMetadataName = "Microsoft.AspNetCore.Components.ParameterAttribute";
    private const string RenderTreeBuilderMetadataName = "Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder";
    private const string IDisposableMetadataName = "System.IDisposable";
    private const string IAsyncDisposableMetadataName = "System.IAsyncDisposable";
    private const string ParameterViewMetadataName = "Microsoft.AspNetCore.Components.ParameterView";

    private static readonly ImmutableHashSet<string> SupportedLifecycleMethods =
        ImmutableHashSet.Create(
            StringComparer.Ordinal,
            "OnInitialized",
            "OnInitializedAsync",
            "OnParametersSet",
            "OnParametersSetAsync",
            "OnAfterRender",
            "OnAfterRenderAsync",
            "ShouldRender");

    public static bool TryBuild(
        GeneratedCSharpBinding binding,
        BoundComponent component,
        out MemberClosure? closure,
        out string? failure)
        => TryBuildCore(binding, component, out closure, out failure, out _);

    private static bool TryBuildCore(
        GeneratedCSharpBinding binding,
        BoundComponent component,
        out MemberClosure? closure,
        out string? failure,
        out ISymbol? failureSubject)
    {
        if (binding is null)
            throw new ArgumentNullException(nameof(binding));
        if (component is null)
            throw new ArgumentNullException(nameof(component));

        closure = null;
        failure = null;
        failureSubject = null;
        if (!binding.Components.Any(candidate => Comparer.Equals(candidate.ComponentSymbol, component.ComponentSymbol)))
        {
            failure = "The RazorVue component member closure input component was not present in the generated-C# binding.";
            return false;
        }

        if (!IsRenderTreeBuilderMethod(component.BuildRenderTreeMethod))
        {
            failure = "The RazorVue component member closure root was not BuildRenderTree(RenderTreeBuilder).";
            failureSubject = component.BuildRenderTreeMethod;
            return false;
        }

        if (!TryBuildComponentInitializationPlan(
                component.ComponentSymbol,
                out var initializationPlan,
                out var initializationFailure,
                out var initializationFailureSubject))
        {
            failure = initializationFailure;
            failureSubject = initializationFailureSubject;
            return false;
        }

        var setParametersAsyncRoot = FindEffectiveSetParametersAsyncOverride(
            binding.Compilation,
            component.ComponentSymbol);
        var injectProperties = LibraryComponentConventions
            .GetEffectiveInjectProperties(component.ComponentSymbol);
        foreach (var property in injectProperties)
        {
            if (!IsAutoProperty(property) || property.SetMethod is null)
            {
                failure = "RazorVue can activate [Inject] properties only when they are writable auto-properties. " +
                          "Use a normal settable property so the browser service adapter can assign the resolved service before component lifecycle callbacks.";
                failureSubject = property;
                return false;
            }
        }
        var cascadingProperties = LibraryComponentConventions
            .GetEffectiveCascadingParameterProperties(component.ComponentSymbol);
        foreach (var property in cascadingProperties)
        {
            if (!IsAutoProperty(property) || property.SetMethod is null)
            {
                failure = "RazorVue can activate [CascadingParameter] properties only when they are writable auto-properties. " +
                          "Use a normal settable property so the browser cascade adapter can assign the nearest value before component lifecycle callbacks.";
                failureSubject = property;
                return false;
            }
        }
        var lifecycleRoots = GetSupportedLifecycleRoots(binding.Compilation, component.ComponentSymbol);
        if (setParametersAsyncRoot is not null)
            lifecycleRoots = lifecycleRoots.Add(setParametersAsyncRoot);
        var roots = ImmutableArray.CreateBuilder<ISymbol>(
            1 + lifecycleRoots.Length + initializationPlan.Constructors.Length +
            (setParametersAsyncRoot is null ? 0 : 1));
        roots.Add(component.BuildRenderTreeMethod);
        roots.AddRange(lifecycleRoots);
        // Property injection is an activation root even when the authored component never reads
        // the property during its first render. Blazor still initializes the property, and a
        // later callback/render must observe the same value rather than a default null slot.
        // 即使首帧未读取，Blazor 也会完成注入；必须把属性纳入 closure 才能保留后续可观察行为。
        roots.AddRange(injectProperties);
        // Cascading parameters are activated by the browser provide/inject adapter. Keep them
        // in the closure even when the first render does not read the property, matching
        // Blazor's activation timing and allowing later lifecycle callbacks to observe updates.
        // 级联参数和 [Inject] 一样属于激活根，不能等到首帧读取后才加入 state。
        roots.AddRange(cascadingProperties);
        if (setParametersAsyncRoot is not null)
            roots.Add(setParametersAsyncRoot);
        roots.AddRange(initializationPlan.Constructors);
        if (initializationPlan.HasExplicitConstructors || setParametersAsyncRoot is not null)
        {
            // ParameterView overlays component storage rather than Vue's read-only props. Its
            // auto-property backing fields must therefore participate even when render code has
            // not read a parameter yet, otherwise a missing parameter loses its CLR initializer.
            // ParameterView 模式需要保留所有参数 storage，缺失参数才能维持默认/旧值。
            roots.AddRange(GetInstanceStorageMembers(
                component.ComponentSymbol,
                includeParameterProperties: setParametersAsyncRoot is not null));
        }

        var compilerClosure = CurrentComponentMemberClosure.Create(
            component.ComponentSymbol,
            binding.Compilation,
            roots.ToImmutable(),
            [component.BuildRenderTreeBody]);
        closure = new MemberClosure(
            component.ComponentSymbol,
            component.BuildRenderTreeMethod,
            compilerClosure,
            lifecycleRoots,
            initializationPlan,
            setParametersAsyncRoot,
            cascadingProperties);
        return true;
    }

    /// <summary>
    /// Final-pipeline variant that keeps the component/member source identity with a closure
    /// failure. The legacy string overload remains for focused closure tests only.
    /// </summary>
    internal static bool TryBuildWithDiagnostic(
        GeneratedCSharpBinding binding,
        BoundComponent component,
        out MemberClosure? closure,
        out RazorVueDiagnosticInfo? diagnostic)
    {
        var built = TryBuildCore(
            binding,
            component,
            out closure,
            out var failure,
            out var failureSubject);
        diagnostic = built
            ? null
            : RazorVueDiagnosticFactory.Create(
                RazorVueDiagnosticCategory.MemberClosure,
                failure!,
                RazorVueDiagnosticFactory.GetSymbolLocation(
                    failureSubject ?? component.BuildRenderTreeMethod),
                component.ComponentSymbol);
        return built;
    }

    private static bool IsRenderTreeBuilderMethod(IMethodSymbol method)
        => string.Equals(method.Name, "BuildRenderTree", StringComparison.Ordinal) &&
           method.Parameters.Length == 1 &&
           string.Equals(
               method.Parameters[0].Type.ToDisplayString(),
               RenderTreeBuilderMetadataName,
               StringComparison.Ordinal);

    private static IMethodSymbol? FindEffectiveSetParametersAsyncOverride(
        Compilation compilation,
        INamedTypeSymbol componentSymbol)
    {
        var componentBase = compilation.GetTypeByMetadataName(ComponentBaseMetadataName);
        foreach (var current in GetSourceComponentHierarchy(componentSymbol))
        {
            var entry = current.GetMembers()
                .OfType<IMethodSymbol>()
                .Where(method => IsSetParametersAsyncOverride(
                    method,
                    componentBase))
                .OrderBy(static method => GetStableMemberKey(method), StringComparer.Ordinal)
                .FirstOrDefault();
            if (entry is not null)
                return entry;
        }

        return null;
    }

    private static bool IsSetParametersAsyncOverride(
        IMethodSymbol method,
        INamedTypeSymbol? componentBase)
    {
        if (method.IsImplicitlyDeclared || method.IsStatic)
            return false;

        if (string.Equals(method.Name, "SetParametersAsync", StringComparison.Ordinal) &&
            method.IsOverride &&
            method.Parameters.Length == 1 &&
            string.Equals(
                method.Parameters[0].Type.OriginalDefinition.ToDisplayString(),
                ParameterViewMetadataName,
                StringComparison.Ordinal) &&
            OverridesComponentBase(method, componentBase))
        {
            return true;
        }

        return false;
    }

    private static bool TryBuildComponentInitializationPlan(
        INamedTypeSymbol componentSymbol,
        out ComponentInitializationPlan plan,
        out string? failure,
        out ISymbol? failureSubject)
    {
        var phases = ImmutableArray.CreateBuilder<ComponentInitializationPhase>();
        failure = null;
        failureSubject = null;
        var hierarchy = GetSourceComponentHierarchy(componentSymbol).ToImmutableArray();

        // Report an authored base(...) / this(...) protocol at the derived constructor
        // before inspecting inherited overload sets. This keeps the diagnostic attached to
        // the actual activation boundary instead of hiding it behind an unrelated selector
        // ambiguity on a base helper type.
        foreach (var sourceType in hierarchy)
        {
            foreach (var constructor in sourceType.InstanceConstructors.Where(IsExplicitSourceConstructor))
            {
                if (!TryValidateConstructorInitializer(constructor, out failure))
                {
                    failureSubject = constructor;
                    plan = ComponentInitializationPlan.Empty;
                    return false;
                }
            }
        }

        for (var index = hierarchy.Length - 1; index >= 0; index--)
        {
            var componentType = hierarchy[index];
            if (HasPrimaryConstructorParameters(componentType))
            {
                failure = "RazorVue cannot supply source component primary-constructor parameters. " +
                          "Use a parameterless component constructor and Vue props/VueInject for runtime inputs.";
                failureSubject = componentType;
                plan = ComponentInitializationPlan.Empty;
                return false;
            }

            var constructors = componentType.InstanceConstructors
                .Where(IsExplicitSourceConstructor)
                .OrderBy(static constructor => GetStableMemberKey(constructor), StringComparer.Ordinal)
                .ToImmutableArray();
            if (constructors.IsDefaultOrEmpty)
            {
                phases.Add(new ComponentInitializationPhase(componentType, null));
                continue;
            }

            if (constructors.Length > 1)
            {
                failure = "RazorVue requires one explicit source component constructor for typed activation. " +
                          "Multiple constructors require an unbound runtime selector.";
                failureSubject = constructors[0];
                plan = ComponentInitializationPlan.Empty;
                return false;
            }

            var constructor = constructors[0];
            if (!TryBuildConstructorParameters(constructor, out var parameters, out failure))
            {
                failureSubject = constructor;
                plan = ComponentInitializationPlan.Empty;
                return false;
            }

            phases.Add(new ComponentInitializationPhase(componentType, constructor, parameters));
        }

        plan = new ComponentInitializationPlan(phases.ToImmutable());
        return true;
    }

    private static bool IsExplicitSourceConstructor(IMethodSymbol constructor)
        => !constructor.IsImplicitlyDeclared &&
           constructor.DeclaringSyntaxReferences.Any(reference =>
               reference.GetSyntax() is ConstructorDeclarationSyntax);

    private static bool HasPrimaryConstructorParameters(INamedTypeSymbol componentType)
        => componentType.DeclaringSyntaxReferences.Any(reference =>
            reference.GetSyntax() is ClassDeclarationSyntax
            {
                ParameterList.Parameters.Count: > 0
            });

    private static bool TryValidateConstructorInitializer(
        IMethodSymbol constructor,
        out string? failure)
    {
        foreach (var reference in constructor.DeclaringSyntaxReferences)
        {
            if (reference.GetSyntax() is not ConstructorDeclarationSyntax declaration ||
                declaration.Initializer is null)
            {
                continue;
            }

            if (declaration.Initializer.IsKind(SyntaxKind.ThisConstructorInitializer))
            {
                failure = "RazorVue does not yet simulate this(...) component constructor chaining. " +
                          "Keep the parameterless constructor body as the component initialization entry point.";
                return false;
            }

            if (declaration.Initializer.IsKind(SyntaxKind.BaseConstructorInitializer) &&
                declaration.Initializer.ArgumentList.Arguments.Count > 0)
            {
                failure = "RazorVue can execute a source component constructor chain with base(), " +
                          "but base(...) arguments require a component activation protocol and are not supported.";
                return false;
            }
        }

        failure = null;
        return true;
    }

    private static bool TryBuildConstructorParameters(
        IMethodSymbol constructor,
        out ImmutableArray<ComponentInitializationParameter> parameters,
        out string? failure)
    {
        var builder = ImmutableArray.CreateBuilder<ComponentInitializationParameter>(constructor.Parameters.Length);
        foreach (var parameter in constructor.Parameters)
        {
            if (parameter.RefKind != RefKind.None || parameter.IsParams)
            {
                parameters = ImmutableArray<ComponentInitializationParameter>.Empty;
                failure = "RazorVue constructor activation supports only ordinary service parameters; ref/out/in/params parameters are not supported.";
                return false;
            }

            if (parameter.Type.IsValueType || parameter.Type is ITypeParameterSymbol ||
                parameter.Type.TypeKind is not (TypeKind.Class or TypeKind.Interface or TypeKind.Delegate))
            {
                parameters = ImmutableArray<ComponentInitializationParameter>.Empty;
                failure = "RazorVue constructor activation path is parameterless for this parameter shape; " +
                          "only reference-type service parameters resolved from Vue providers are supported.";
                return false;
            }

            builder.Add(new ComponentInitializationParameter(
                parameter.Name,
                LibraryComponentConventions.GetInjectServiceKeyForType(parameter.Type),
                parameter.Type.ToDisplayString(Format.NameFormat)));
        }

        parameters = builder.ToImmutable();
        failure = null;
        return true;
    }

    // Keep the original one-argument reflection/test seam stable while allowing the
    // ParameterView adapter to opt into parameter storage explicitly.
    // 保留旧的单参数契约，ParameterView 仅在调用处显式开启参数 storage。
    private static IEnumerable<ISymbol> GetInstanceStorageMembers(INamedTypeSymbol componentSymbol)
        => GetInstanceStorageMembers(componentSymbol, includeParameterProperties: false);

    private static IEnumerable<ISymbol> GetInstanceStorageMembers(
        INamedTypeSymbol componentSymbol,
        bool includeParameterProperties)
    {
        foreach (var current in GetSourceComponentHierarchy(componentSymbol))
        {
            foreach (var field in current.GetMembers().OfType<IFieldSymbol>())
            {
                if (!field.IsStatic && !field.IsConst && field.AssociatedSymbol is null)
                    yield return field;
            }

            foreach (var property in current.GetMembers().OfType<IPropertySymbol>())
            {
                if (!property.IsStatic &&
                    (includeParameterProperties || !IsParameterProperty(property)) &&
                    IsAutoProperty(property))
                    yield return property;
            }
        }
    }

    private static bool IsParameterProperty(IPropertySymbol property)
        => LibraryComponentConventions.IsParameterProperty(property);

    private static bool IsAutoProperty(IPropertySymbol property)
    {
        foreach (var reference in property.DeclaringSyntaxReferences)
        {
            if (reference.GetSyntax() is not PropertyDeclarationSyntax declaration)
                continue;

            if (declaration.ExpressionBody is not null || declaration.AccessorList is null)
                return false;

            if (declaration.AccessorList.Accessors.Any(accessor =>
                    accessor.Body is not null || accessor.ExpressionBody is not null))
            {
                return false;
            }

            return true;
        }

        return false;
    }

    private static IEnumerable<INamedTypeSymbol> GetSourceComponentHierarchy(INamedTypeSymbol componentSymbol)
    {
        for (var current = componentSymbol;
             current is not null && current.DeclaringSyntaxReferences.Length > 0;
             current = current.BaseType)
        {
            yield return current;
        }
    }

    private static ImmutableArray<IMethodSymbol> GetSupportedLifecycleRoots(
        Compilation compilation,
        INamedTypeSymbol componentSymbol)
    {
        var componentBase = compilation.GetTypeByMetadataName(ComponentBaseMetadataName);
        var disposable = compilation.GetTypeByMetadataName(IDisposableMetadataName);
        var asyncDisposable = compilation.GetTypeByMetadataName(IAsyncDisposableMetadataName);
        // Resolve the member that CLR virtual/interface dispatch would invoke for this concrete
        // component. A source base is part of the authored component surface, so selecting only
        // componentSymbol.GetMembers() would silently drop perfectly valid base hooks.
        // 生命周期识别必须沿真实 override/interface 链解析；同名普通 helper 绝不能成为 Vue hook。
        var roots = ImmutableArray.CreateBuilder<IMethodSymbol>();
        foreach (var lifecycleName in new[]
                 {
                     "OnInitialized",
                     "OnInitializedAsync",
                     "OnParametersSet",
                     "OnParametersSetAsync",
                     "OnAfterRender",
                     "OnAfterRenderAsync",
                     "ShouldRender"
                 })
        {
            if (FindEffectiveLifecycleOverride(componentSymbol, componentBase, lifecycleName) is { } lifecycle)
                roots.Add(lifecycle);
        }

        AddEffectiveDisposeRoot(disposable, "Dispose");
        AddEffectiveDisposeRoot(asyncDisposable, "DisposeAsync");
        return roots.ToImmutable();

        void AddEffectiveDisposeRoot(INamedTypeSymbol? interfaceType, string methodName)
        {
            var interfaceMethod = interfaceType?
                .GetMembers(methodName)
                .OfType<IMethodSymbol>()
                .SingleOrDefault(static method => method.Parameters.Length == 0);
            if (interfaceMethod is null)
                return;

            if (FindEffectiveInterfaceImplementation(componentSymbol, interfaceMethod) is not IMethodSymbol implementation ||
                !IsDeclaredOnSourceComponentHierarchy(componentSymbol, implementation.ContainingType))
            {
                return;
            }

            roots.Add(implementation);
        }
    }

    private static IMethodSymbol? FindEffectiveInterfaceImplementation(
        INamedTypeSymbol componentSymbol,
        IMethodSymbol interfaceMethod)
    {
        if (componentSymbol.FindImplementationForInterfaceMember(interfaceMethod) is not IMethodSymbol mapped)
            return null;

        // Roslyn may return the method that first establishes an interface slot (usually a
        // source base method) even when a more-derived source type overrides that virtual
        // method. Resolve the dispatch target exactly as CLR interface invocation would: walk
        // the source hierarchy from the concrete component toward the mapped declaration and
        // select the first method in the same virtual slot. Explicit interface implementations
        // are selected directly because they do not participate in the override chain.
        // 接口 dispose 根必须绑定最派生实现，否则 unmount 会漏掉派生清理逻辑。
        foreach (var current in GetSourceComponentHierarchy(componentSymbol))
        {
            var candidate = current
                .GetMembers()
                .OfType<IMethodSymbol>()
                .Where(method => IsEffectiveInterfaceImplementation(method, interfaceMethod, mapped))
                .OrderBy(static method => GetStableMemberKey(method), StringComparer.Ordinal)
                .FirstOrDefault();
            if (candidate is not null)
                return candidate;
        }

        return mapped;
    }

    private static bool IsEffectiveInterfaceImplementation(
        IMethodSymbol candidate,
        IMethodSymbol interfaceMethod,
        IMethodSymbol mapped)
    {
        if (candidate.IsStatic || candidate.Parameters.Length != interfaceMethod.Parameters.Length)
            return false;

        if (candidate.ExplicitInterfaceImplementations.Any(implementation =>
                Comparer.Equals(implementation.OriginalDefinition, interfaceMethod.OriginalDefinition)))
        {
            return true;
        }

        if (!string.Equals(candidate.Name, mapped.Name, StringComparison.Ordinal) ||
            candidate.MethodKind != MethodKind.Ordinary)
        {
            return false;
        }

        for (var current = candidate;
             current is not null;
             current = current.OverriddenMethod)
        {
            if (Comparer.Equals(current.OriginalDefinition, mapped.OriginalDefinition))
                return true;
        }

        return Comparer.Equals(candidate.OriginalDefinition, mapped.OriginalDefinition);
    }

    private static IMethodSymbol? FindEffectiveLifecycleOverride(
        INamedTypeSymbol componentSymbol,
        INamedTypeSymbol? componentBase,
        string lifecycleName)
    {
        if (componentBase is null)
            return null;

        foreach (var current in GetSourceComponentHierarchy(componentSymbol))
        {
            var lifecycle = current
                .GetMembers(lifecycleName)
                .OfType<IMethodSymbol>()
                .Where(method => IsEffectiveLifecycleOverride(method, componentBase))
                .OrderBy(static method => GetStableMemberKey(method), StringComparer.Ordinal)
                .FirstOrDefault();
            if (lifecycle is not null)
                return lifecycle;
        }

        return null;
    }

    private static bool IsEffectiveLifecycleOverride(
        IMethodSymbol method,
        INamedTypeSymbol? componentBase)
        => SupportedLifecycleMethods.Contains(method.Name) &&
           !method.IsStatic &&
           method.MethodKind == MethodKind.Ordinary &&
           method.IsOverride &&
           OverridesComponentBase(method, componentBase);

    private static bool IsSupportedLifecycleMethod(
        INamedTypeSymbol componentSymbol,
        IMethodSymbol method,
        INamedTypeSymbol? componentBase)
    {
        if (!SupportedLifecycleMethods.Contains(method.Name) ||
            method.IsStatic ||
            method.MethodKind != MethodKind.Ordinary ||
            !Comparer.Equals(method.ContainingType!.OriginalDefinition, componentSymbol.OriginalDefinition))
        {
            return false;
        }

        return method.IsOverride &&
               (componentBase is null ||
                (method.OverriddenMethod is not null &&
                 Comparer.Equals(method.OverriddenMethod.ContainingType.OriginalDefinition, componentBase)));
    }

    private static bool OverridesComponentBase(IMethodSymbol method, INamedTypeSymbol? componentBase)
    {
        if (componentBase is null)
            return false;

        for (var overridden = method.OverriddenMethod;
             overridden is not null;
             overridden = overridden.OverriddenMethod)
        {
            if (Comparer.Equals(overridden.ContainingType.OriginalDefinition, componentBase.OriginalDefinition))
                return true;
        }

        return false;
    }

    private static bool IsDisposeRoot(
        INamedTypeSymbol componentSymbol,
        IMethodSymbol method,
        INamedTypeSymbol? disposable,
        INamedTypeSymbol? asyncDisposable)
    {
        // Explicit IDisposable/IAsyncDisposable implementations have a distinct Roslyn
        // MethodKind but remain valid component unmount roots.
        if (method.IsStatic ||
            !Comparer.Equals(method.ContainingType?.OriginalDefinition, componentSymbol.OriginalDefinition))
        {
            return false;
        }

        return IsDisposeEntryPoint(method, disposable, asyncDisposable);
    }

    private static bool IsDeclaredOnSourceComponentHierarchy(
        INamedTypeSymbol componentSymbol,
        INamedTypeSymbol? declaringType)
        => declaringType is not null &&
           GetSourceComponentHierarchy(componentSymbol).Any(current =>
               Comparer.Equals(current.OriginalDefinition, declaringType.OriginalDefinition));

    private static bool IsDisposeEntryPoint(
        IMethodSymbol method,
        INamedTypeSymbol? disposable,
        INamedTypeSymbol? asyncDisposable)
    {
        if (method.IsStatic ||
            (method.MethodKind != MethodKind.Ordinary &&
             method.MethodKind != MethodKind.ExplicitInterfaceImplementation))
        {
            return false;
        }

        var declaringType = method.ContainingType;
        if (declaringType is null || method.Parameters.Length != 0)
            return false;

        if (string.Equals(method.Name, "Dispose", StringComparison.Ordinal) &&
            method.ReturnsVoid &&
            ImplementsInterface(declaringType, disposable))
        {
            return true;
        }

        if (string.Equals(method.Name, "DisposeAsync", StringComparison.Ordinal) &&
            ImplementsInterface(declaringType, asyncDisposable) &&
            IsAsyncDisposeReturnType(method.ReturnType))
        {
            return true;
        }

        return method.ExplicitInterfaceImplementations.Any(implementation =>
            (disposable is not null &&
             Comparer.Equals(implementation.ContainingType.OriginalDefinition, disposable.OriginalDefinition)) ||
            (asyncDisposable is not null &&
             Comparer.Equals(implementation.ContainingType.OriginalDefinition, asyncDisposable.OriginalDefinition)));
    }

    private static bool ImplementsInterface(INamedTypeSymbol componentSymbol, INamedTypeSymbol? interfaceType)
        => interfaceType is not null &&
           componentSymbol.AllInterfaces.Any(@interface =>
               Comparer.Equals(@interface.OriginalDefinition, interfaceType.OriginalDefinition));

    private static bool IsAsyncDisposeReturnType(ITypeSymbol returnType)
    {
        var display = returnType.OriginalDefinition.ToDisplayString();
        return string.Equals(display, "System.Threading.Tasks.ValueTask", StringComparison.Ordinal) ||
               string.Equals(display, "System.Threading.Tasks.Task", StringComparison.Ordinal) ||
               string.Equals(display, "System.Threading.Tasks.ValueTask<TResult>", StringComparison.Ordinal) ||
               string.Equals(display, "System.Threading.Tasks.Task<TResult>", StringComparison.Ordinal);
    }

    private static string GetStableMemberKey(ISymbol symbol)
    {
        foreach (var location in symbol.Locations)
        {
            if (!location.IsInSource)
                continue;

            var span = location.GetLineSpan();
            var path = span.Path.Replace('\\', '/');
            var start = span.StartLinePosition;
            return path +
                   "|" +
                   start.Line.ToString("D10", System.Globalization.CultureInfo.InvariantCulture) +
                   "|" +
                   start.Character.ToString("D10", System.Globalization.CultureInfo.InvariantCulture) +
                   "|" +
                   symbol.Kind +
                   "|" +
                   symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        }

        return "~|" + symbol.Kind + "|" + symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }
}

/// <summary>
/// Describes the source constructor chain that can be replayed inside one Vue setup instance.
/// The plan is intentionally structural: C# bodies still flow through SemanticWalker later.
/// 这里只保存 base-to-derived 执行顺序；不在 closure 阶段手工重写任何 C# statement。
/// </summary>
internal sealed record ComponentInitializationPlan(
    ImmutableArray<ComponentInitializationPhase> Phases)
{
    public static ComponentInitializationPlan Empty { get; } = new(
        ImmutableArray<ComponentInitializationPhase>.Empty);

    public bool HasExplicitConstructors
        => Phases.Any(static phase => phase.Constructor is not null);

    public ImmutableArray<IMethodSymbol> Constructors
        => Phases
            .Where(static phase => phase.Constructor is not null)
            .Select(static phase => phase.Constructor!)
            .ToImmutableArray();
}

/// <summary>One source type's instance-field initializer and optional constructor-body phase.</summary>
internal sealed record ComponentInitializationPhase(
    INamedTypeSymbol ComponentType,
    IMethodSymbol? Constructor,
    ImmutableArray<ComponentInitializationParameter> Parameters = default);

internal sealed record ComponentInitializationParameter(
    string Name,
    string ServiceKey,
    string ServiceTypeDisplay);

/// <summary>Declared component members required by one emitted Vue module. 它把 compiler closure 按 Vue 的 state/props/lifecycle 使用方式重新分类。</summary>
internal sealed record MemberClosure(
    INamedTypeSymbol ComponentSymbol,
    IMethodSymbol BuildRenderTreeMethod,
    CurrentComponentMemberClosure CompilerClosure,
    ImmutableArray<IMethodSymbol> LifecycleRoots,
    ComponentInitializationPlan InitializationPlan,
    IMethodSymbol? SetParametersAsyncRoot,
    ImmutableArray<IPropertySymbol> CascadingParameterProperties)
{
    private static readonly SymbolEqualityComparer Comparer = SymbolEqualityComparer.Default;
    private const string RenderTreeBuilderMetadataName = "Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder";

    public ImmutableArray<ISymbol> OrderedMembers
        => CompilerClosure.Members;

    public ImmutableArray<IFieldSymbol> StateFields
        => OrderedMembers
            .OfType<IFieldSymbol>()
            .Where(candidate =>
                IsComponentSurfaceMember(candidate) &&
                !candidate.IsStatic)
            .ToImmutableArray();

    /// <summary>
    /// Static fields have module lifetime and must not be copied into each Vue setup state's
    /// reactive object. They remain in the compiler module and are projected by the host as
    /// lexical module bindings.
    /// 静态字段属于 artifact/module 生命周期，不能误放进每个实例的 reactive state。
    /// </summary>
    public ImmutableArray<IFieldSymbol> StaticFields
        => OrderedMembers
            .OfType<IFieldSymbol>()
            .Where(candidate =>
                IsComponentSurfaceMember(candidate) &&
                candidate.IsStatic)
            .ToImmutableArray();

    public ImmutableArray<IPropertySymbol> ParameterProperties
    {
        get
        {
            var effectiveParameters = LibraryComponentConventions
                .GetEffectiveParameterProperties(ComponentSymbol)
                .Select(static property => property.OriginalDefinition)
                .ToImmutableHashSet(Comparer);
            // Normal components only need parameter storage already reached by the closure:
            // rendering reads Vue props directly. A SetParametersAsync override is different:
            // ComponentBase applies every supplied parameter to instance storage, including a
            // parameter not rendered by this component. Keep that complete surface so an omitted
            // value preserves its existing CLR default or prior assignment.
            // 自定义 SetParametersAsync 需要完整参数 surface，不能只看 render 可达成员。
            var candidates = UsesParameterViewState
                ? LibraryComponentConventions.GetEffectiveParameterProperties(ComponentSymbol)
                : OrderedMembers.OfType<IPropertySymbol>();
            return candidates
                .Where(property => effectiveParameters.Contains(property.OriginalDefinition))
                .ToImmutableArray();
        }
    }

    /// <summary>
    /// A custom ComponentBase.SetParametersAsync override changes the parameter carrier from
    /// Vue's immutable props proxy to component instance state. The runtime bridge owns the
    /// snapshot/overlay protocol; authored code retains the standard Blazor entry point.
    /// 自定义参数入口启用真实 ParameterView state adapter，而不是 props watch 伪装。
    /// </summary>
    public bool UsesParameterViewState
        => SetParametersAsyncRoot is not null;

    /// <summary>Effective standard Blazor property-injection contract for this component.</summary>
    public ImmutableArray<IPropertySymbol> InjectProperties
        => LibraryComponentConventions.GetEffectiveInjectProperties(ComponentSymbol)
            .Where(static property => property.SetMethod is not null)
            .ToImmutableArray();

    public ImmutableArray<IPropertySymbol> StateProperties
        => OrderedMembers
            .OfType<IPropertySymbol>()
            .Where(property =>
                IsComponentSurfaceMember(property) &&
                !property.IsStatic &&
                !IsParameterProperty(property) &&
                IsAutoProperty(property))
            .ToImmutableArray();

    public ImmutableArray<IPropertySymbol> StaticAutoProperties
        => OrderedMembers
            .OfType<IPropertySymbol>()
            .Where(property =>
                IsComponentSurfaceMember(property) &&
                property.IsStatic &&
                IsAutoProperty(property))
            .ToImmutableArray();

    public ImmutableArray<IPropertySymbol> ComputedProperties
        => OrderedMembers
            .OfType<IPropertySymbol>()
            .Where(property =>
                IsComponentSurfaceMember(property) &&
                !IsParameterProperty(property) &&
                !IsAutoProperty(property))
            .ToImmutableArray();

    public ImmutableArray<IMethodSymbol> ReachableMethods
        => OrderedMembers
            .OfType<IMethodSymbol>()
            .Where(method =>
                IsComponentSurfaceMember(method) &&
                method.MethodKind == MethodKind.Ordinary &&
                !Comparer.Equals(method.OriginalDefinition, BuildRenderTreeMethod.OriginalDefinition) &&
                !LifecycleRoots.Any(lifecycle => Comparer.Equals(lifecycle.OriginalDefinition, method.OriginalDefinition)) &&
                (SetParametersAsyncRoot is null ||
                 !Comparer.Equals(method.OriginalDefinition, SetParametersAsyncRoot.OriginalDefinition)))
            .ToImmutableArray();

    private bool IsComponentSurfaceMember(ISymbol member)
        => ComponentSymbolPolicy.IsDeclaredOnComponentHierarchy(
            ComponentSymbol,
            member.ContainingType);

    public Func<ISymbol, bool> CreateMemberFilter()
        => ShouldIncludeCompilerMember;

    public AstConverterOptions CreateAstConverterOptions(
        string stateIdentifier = "state",
        string propsIdentifier = "props",
        IReadOnlyDictionary<ISymbol, string>? declaredNames = null,
        Func<IPropertyReferenceOperation, SenseArgument, Expression?>? propertyReferenceRewriter = null,
        Compilation? compilation = null,
        VueInjectRegistry? injectRegistry = null,
        VueRenderRuntimeFeatures? ordinaryRenderFeatures = null)
    {
        SemanticWalkerHost? tableCellHost = compilation is not null &&
                                             injectRegistry is not null &&
                                             ordinaryRenderFeatures is not null
            ? new TDesignTableCellSemanticWalkerHost(
                compilation,
                ComponentSymbol,
                declaredNames,
                injectRegistry,
                ordinaryRenderFeatures)
            : null;
        return new AstConverterOptions(
            AstConverterProfile.Standard,
            MemberFilter: ShouldIncludeCompilerMember,
            DeclaredNames: declaredNames,
            Host: new VueSemanticWalkerHost(
                ComponentSymbol,
                stateIdentifier,
                propsIdentifier,
                BuildParameterRuntimeNameMap(ComponentSymbol),
                declaredNames,
                parameterPropertiesUseState: UsesParameterViewState,
                propertyReferenceRewriter: propertyReferenceRewriter,
                tableCellHost: tableCellHost),
            ModulePolicy: VueModulePolicy.Instance,
            RuntimeClassPrivateStorage: RuntimeClassPrivateStorage.ProxySafeMangledProperties);
    }

    private bool ShouldIncludeCompilerMember(ISymbol symbol)
    {
        // BuildRenderTree and template-only helpers are consumed by RenderEmitter before the
        // generic compiler module pass. Keeping them here would emit dead functions in the
        // final module because direct VNode lowering already owns their execution shape.
        // 仅跳过已被 direct emitter 完整接管的模板成员；事件/普通业务方法仍必须由 compiler 发射。
        if (symbol is IMethodSymbol method &&
            (Comparer.Equals(method.OriginalDefinition, BuildRenderTreeMethod.OriginalDefinition) ||
             IsDirectRenderTemplateHelper(method)))
        {
            return false;
        }

        // Storage properties are projected to props/state. Their compiler-generated
        // accessors would otherwise survive as dead functions that reference a backing
        // field removed later by module materialization. Computed accessors remain real
        // module members and continue through the compiler.
        if (symbol is IMethodSymbol { AssociatedSymbol: IPropertySymbol property } &&
            (IsParameterProperty(property) || IsAutoProperty(property)))
        {
            return false;
        }

        return CompilerClosure.ShouldInclude(symbol);
    }

    private static bool IsDirectRenderTemplateHelper(IMethodSymbol method)
        => method.ReturnsVoid &&
           method.Parameters.Length == 1 &&
           IsRenderTreeBuilderType(method.Parameters[0].Type);

    private static bool IsRenderTreeBuilderType(ITypeSymbol type)
        => string.Equals(
            type.OriginalDefinition.ToDisplayString(),
            RenderTreeBuilderMetadataName,
            StringComparison.Ordinal);

    private static bool IsParameterProperty(IPropertySymbol property)
        => LibraryComponentConventions.IsParameterProperty(property);

    private static bool IsAutoProperty(IPropertySymbol property)
    {
        foreach (var reference in property.DeclaringSyntaxReferences)
        {
            if (reference.GetSyntax() is not Microsoft.CodeAnalysis.CSharp.Syntax.PropertyDeclarationSyntax declaration)
                continue;

            if (declaration.ExpressionBody is not null ||
                declaration.AccessorList is null)
            {
                return false;
            }

            foreach (var accessor in declaration.AccessorList.Accessors)
            {
                if (accessor.Body is not null || accessor.ExpressionBody is not null)
                    return false;
            }

            return true;
        }

        return false;
    }

    private static IReadOnlyDictionary<string, string> BuildParameterRuntimeNameMap(INamedTypeSymbol componentSymbol)
        => LibraryComponentConventions.BuildParameterRuntimeNameMap(componentSymbol);

}
