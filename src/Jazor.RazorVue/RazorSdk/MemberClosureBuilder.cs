using System.Collections.Immutable;
using Jazor.Compiler;
using Jazor.RazorVue.Generation;
using Acornima.Ast;
using Microsoft.CodeAnalysis;
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
    private const string RenderFragmentMetadataName = "Microsoft.AspNetCore.Components.RenderFragment";
    private const string IDisposableMetadataName = "System.IDisposable";
    private const string IAsyncDisposableMetadataName = "System.IAsyncDisposable";

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
    {
        if (binding is null)
            throw new ArgumentNullException(nameof(binding));
        if (component is null)
            throw new ArgumentNullException(nameof(component));

        closure = null;
        failure = null;
        if (!binding.Components.Any(candidate => Comparer.Equals(candidate.ComponentSymbol, component.ComponentSymbol)))
        {
            failure = "The RazorVue component member closure input component was not present in the generated-C# binding.";
            return false;
        }

        if (!IsRenderTreeBuilderMethod(component.BuildRenderTreeMethod))
        {
            failure = "The RazorVue component member closure root was not BuildRenderTree(RenderTreeBuilder).";
            return false;
        }

        var lifecycleRoots = GetSupportedLifecycleRoots(binding.Compilation, component.ComponentSymbol);
        var roots = ImmutableArray.CreateBuilder<IMethodSymbol>(1 + lifecycleRoots.Length);
        roots.Add(component.BuildRenderTreeMethod);
        roots.AddRange(lifecycleRoots);

        var compilerClosure = CurrentComponentMemberClosure.Create(
            component.ComponentSymbol,
            binding.Compilation,
            roots.ToImmutable(),
            [component.BuildRenderTreeBody]);
        closure = new MemberClosure(
            component.ComponentSymbol,
            component.BuildRenderTreeMethod,
            compilerClosure,
            lifecycleRoots);
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
        var built = TryBuild(binding, component, out closure, out var failure);
        diagnostic = built
            ? null
            : RazorVueDiagnosticFactory.Create(
                RazorVueDiagnosticCategory.MemberClosure,
                failure ?? "No component member closure detail was provided.",
                RazorVueDiagnosticFactory.GetSymbolLocation(component.BuildRenderTreeMethod),
                component.ComponentSymbol,
                isAuthorReachable: true);
        return built;
    }

    private static bool IsRenderTreeBuilderMethod(IMethodSymbol method)
        => string.Equals(method.Name, "BuildRenderTree", StringComparison.Ordinal) &&
           method.Parameters.Length == 1 &&
           string.Equals(
               method.Parameters[0].Type.ToDisplayString(),
               RenderTreeBuilderMetadataName,
               StringComparison.Ordinal);

    private static ImmutableArray<IMethodSymbol> GetSupportedLifecycleRoots(
        Compilation compilation,
        INamedTypeSymbol componentSymbol)
    {
        var componentBase = compilation.GetTypeByMetadataName(ComponentBaseMetadataName);
        var disposable = compilation.GetTypeByMetadataName(IDisposableMetadataName);
        var asyncDisposable = compilation.GetTypeByMetadataName(IAsyncDisposableMetadataName);
        // Only explicit ComponentBase overrides and disposal entry points become lifecycle roots.
        // Same-named helper methods must stay ordinary component members, never acquire Vue hooks.
        // 生命周期识别依赖 Roslyn override/interface 关系，不能只按方法名猜测。
        return componentSymbol
            .GetMembers()
            .OfType<IMethodSymbol>()
            .Where(method =>
                IsSupportedLifecycleMethod(componentSymbol, method, componentBase) ||
                IsDisposeRoot(componentSymbol, method, disposable, asyncDisposable))
            .OrderBy(static method => GetStableMemberKey(method), StringComparer.Ordinal)
            .ToImmutableArray();
    }

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

    private static bool IsDisposeRoot(
        INamedTypeSymbol componentSymbol,
        IMethodSymbol method,
        INamedTypeSymbol? disposable,
        INamedTypeSymbol? asyncDisposable)
    {
        // Explicit IDisposable/IAsyncDisposable implementations have a distinct Roslyn
        // MethodKind but remain valid component unmount roots.
        if (method.IsStatic ||
            (method.MethodKind != MethodKind.Ordinary &&
             method.MethodKind != MethodKind.ExplicitInterfaceImplementation) ||
            method.Parameters.Length != 0 ||
            !Comparer.Equals(method.ContainingType?.OriginalDefinition, componentSymbol.OriginalDefinition))
        {
            return false;
        }

        if (string.Equals(method.Name, "Dispose", StringComparison.Ordinal) &&
            method.ReturnsVoid &&
            ImplementsInterface(componentSymbol, disposable))
        {
            return true;
        }

        if (string.Equals(method.Name, "DisposeAsync", StringComparison.Ordinal) &&
            ImplementsInterface(componentSymbol, asyncDisposable) &&
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
            var path = (span.Path ?? string.Empty).Replace('\\', '/');
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

/// <summary>Declared component members required by one emitted Vue module. 它把 compiler closure 按 Vue 的 state/props/lifecycle 使用方式重新分类。</summary>
internal sealed record MemberClosure(
    INamedTypeSymbol ComponentSymbol,
    IMethodSymbol BuildRenderTreeMethod,
    CurrentComponentMemberClosure CompilerClosure,
    ImmutableArray<IMethodSymbol> LifecycleRoots)
{
    private static readonly SymbolEqualityComparer Comparer = SymbolEqualityComparer.Default;
    private const string RenderTreeBuilderMetadataName = "Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder";
    private const string RenderFragmentMetadataName = "Microsoft.AspNetCore.Components.RenderFragment";

    public ImmutableArray<ISymbol> OrderedMembers
        => CompilerClosure.Members;

    public ImmutableArray<IFieldSymbol> StateFields
        => OrderedMembers
            .OfType<IFieldSymbol>()
            .Where(IsComponentSurfaceMember)
            .ToImmutableArray();

    public ImmutableArray<IPropertySymbol> ParameterProperties
    {
        get
        {
            var effectiveParameters = LibraryComponentConventions
                .GetEffectiveParameterProperties(ComponentSymbol)
                .Select(static property => property.OriginalDefinition)
                .ToImmutableHashSet(Comparer);
            return OrderedMembers
                .OfType<IPropertySymbol>()
                .Where(property => effectiveParameters.Contains(property.OriginalDefinition))
                .ToImmutableArray();
        }
    }

    public ImmutableArray<IPropertySymbol> StateProperties
        => OrderedMembers
            .OfType<IPropertySymbol>()
            .Where(property =>
                IsComponentSurfaceMember(property) &&
                !IsParameterProperty(property) &&
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
                !LifecycleRoots.Any(lifecycle => Comparer.Equals(lifecycle.OriginalDefinition, method.OriginalDefinition)))
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
        Func<IPropertyReferenceOperation, SenseArgument, Expression?>? propertyReferenceRewriter = null)
        => new(
            AstConverterProfile.Standard,
            MemberFilter: ShouldIncludeCompilerMember,
            DeclaredNames: declaredNames,
            Host: new VueSemanticWalkerHost(
                ComponentSymbol,
                stateIdentifier,
                propsIdentifier,
                BuildParameterRuntimeNameMap(ComponentSymbol),
                declaredNames,
                propertyReferenceRewriter: propertyReferenceRewriter),
            ModulePolicy: VueModulePolicy.Instance,
            RuntimeClassPrivateStorage: RuntimeClassPrivateStorage.ProxySafeMangledProperties);

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
        => IsAnyRenderFragmentType(method.ReturnType) ||
           (method.ReturnsVoid &&
            method.Parameters.Length == 1 &&
            IsRenderTreeBuilderType(method.Parameters[0].Type));

    private static bool IsAnyRenderFragmentType(ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol named || !string.Equals(named.Name, "RenderFragment", StringComparison.Ordinal))
            return false;

        return string.Equals(
            named.OriginalDefinition.ToDisplayString(),
            RenderFragmentMetadataName,
            StringComparison.Ordinal) ||
            string.Equals(
                named.OriginalDefinition.ToDisplayString(),
                RenderFragmentMetadataName + "<TValue>",
                StringComparison.Ordinal);
    }

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
