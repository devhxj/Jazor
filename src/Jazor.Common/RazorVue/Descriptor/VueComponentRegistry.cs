using System.Collections.Immutable;
using System.Linq;
using Jazor.RazorVue.Artifacts;

namespace Jazor.RazorVue.Descriptor;

internal sealed class VueComponentRegistry
{
    private static readonly ImmutableArray<VueComponentDescriptor> IntrinsicComponents = VueIntrinsicComponentDescriptors.All;

    private VueComponentRegistry(
        ImmutableArray<VueComponentDescriptor> components,
        ImmutableDictionary<string, ImmutableArray<VueComponentDescriptor>> componentsByName,
        ImmutableDictionary<string, VueComponentDescriptor> componentsByFullName,
        ImmutableDictionary<string, ImmutableArray<VueComponentDescriptor>> componentsByResolutionNamespace)
    {
        Components = components;
        ComponentsByName = componentsByName;
        ComponentsByFullName = componentsByFullName;
        ComponentsByResolutionNamespace = componentsByResolutionNamespace;
    }

    public ImmutableArray<VueComponentDescriptor> Components { get; }

    public ImmutableDictionary<string, ImmutableArray<VueComponentDescriptor>> ComponentsByName { get; }

    public ImmutableDictionary<string, VueComponentDescriptor> ComponentsByFullName { get; }

    public ImmutableDictionary<string, ImmutableArray<VueComponentDescriptor>> ComponentsByResolutionNamespace { get; }

    public static VueComponentRegistry Create(
        ImmutableArray<RazorVueSemanticSnapshot> userSnapshots,
        ImmutableArray<VueComponentDescriptor> libraryComponents = default(ImmutableArray<VueComponentDescriptor>))
    {
        var userComponents = userSnapshots.IsDefault
            ? ImmutableArray<VueComponentDescriptor>.Empty
            : userSnapshots.Select(static snapshot => snapshot.Descriptor).ToImmutableArray();

        return Create(userComponents, libraryComponents);
    }

    public static VueComponentRegistry Create(
        ImmutableArray<VueComponentDescriptor> userComponents,
        ImmutableArray<VueComponentDescriptor> libraryComponents = default(ImmutableArray<VueComponentDescriptor>))
    {
        var allComponents = ImmutableArray.CreateBuilder<VueComponentDescriptor>();
        AddRange(allComponents, IntrinsicComponents);
        AddRange(allComponents, userComponents);
        AddRange(allComponents, libraryComponents);

        var byName = allComponents
            .GroupBy(static descriptor => descriptor.Name, StringComparer.Ordinal)
            .ToImmutableDictionary(
                static group => group.Key,
                static group => group.ToImmutableArray(),
                StringComparer.Ordinal);

        var byFullName = ImmutableDictionary.CreateBuilder<string, VueComponentDescriptor>(StringComparer.Ordinal);
        foreach (var component in allComponents)
            byFullName[component.FullName] = component;

        var byResolutionNamespace = allComponents
            .GroupBy(static descriptor => descriptor.ResolutionNamespace ?? string.Empty, StringComparer.Ordinal)
            .ToImmutableDictionary(
                static group => group.Key,
                static group => group.ToImmutableArray(),
                StringComparer.Ordinal);

        return new VueComponentRegistry(
            allComponents.ToImmutable(),
            byName,
            byFullName.ToImmutable(),
            byResolutionNamespace);
    }

    public VueComponentResolutionResult Resolve(string componentName, VueComponentResolutionContext context)
    {
        if (componentName is null)
            throw new ArgumentNullException(nameof(componentName));
        if (context is null)
            throw new ArgumentNullException(nameof(context));

        if (componentName.IndexOf('.') >= 0)
        {
            return ComponentsByFullName.TryGetValue(componentName, out var exactDescriptor)
                ? VueComponentResolutionResult.Resolved(componentName, exactDescriptor)
                : VueComponentResolutionResult.NotFound(componentName);
        }

        if (!ComponentsByName.TryGetValue(componentName, out var candidates))
            return VueComponentResolutionResult.NotFound(componentName);

        var intrinsicMatches = candidates
            .Where(static descriptor => descriptor.SourceKind == VueComponentSourceKind.Intrinsic)
            .ToImmutableArray();
        var visibleMatches = candidates
            .Where(descriptor => descriptor.SourceKind != VueComponentSourceKind.Intrinsic && IsVisible(descriptor, context))
            .ToImmutableArray();

        if (intrinsicMatches.Length > 0)
        {
            // Intrinsic names are reserved. If a visible user/library descriptor
            // collides with an intrinsic short name, resolution must surface that
            // conflict instead of silently shadowing the intrinsic.
            return visibleMatches.Length > 0
                ? VueComponentResolutionResult.ReservedIntrinsicName(componentName, intrinsicMatches.AddRange(visibleMatches))
                : VueComponentResolutionResult.Resolved(componentName, intrinsicMatches[0]);
        }

        if (visibleMatches.Length == 0)
            return VueComponentResolutionResult.NotFound(componentName);

        if (visibleMatches.Length == 1)
            return VueComponentResolutionResult.Resolved(componentName, visibleMatches[0]);

        return VueComponentResolutionResult.Ambiguous(componentName, visibleMatches);
    }

    private static bool IsVisible(VueComponentDescriptor descriptor, VueComponentResolutionContext context)
    {
        if (string.Equals(descriptor.ResolutionNamespace, context.CurrentNamespace, StringComparison.Ordinal))
            return true;

        return context.Imports.Contains(descriptor.ResolutionNamespace, StringComparer.Ordinal);
    }

    private static void AddRange(
        ImmutableArray<VueComponentDescriptor>.Builder target,
        ImmutableArray<VueComponentDescriptor> source)
    {
        if (source.IsDefaultOrEmpty)
            return;

        foreach (var item in source)
            target.Add(item);
    }
}
