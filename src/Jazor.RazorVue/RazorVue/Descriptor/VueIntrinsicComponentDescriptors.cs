using System.Collections.Immutable;

namespace Jazor.RazorVue.Descriptor;

internal static class VueIntrinsicComponentDescriptors
{
    public static ImmutableArray<VueComponentDescriptor> All { get; } =
    [
        Create("Teleport"),
        Create("Transition"),
        Create("KeepAlive"),
        Create("Suspense")
    ];

    private static VueComponentDescriptor Create(string name)
        => new(
            Name: name,
            FullName: $"ECMAScript.UI.Vue.{name}",
            SourceKind: VueComponentSourceKind.Intrinsic,
            ResolutionNamespace: "ECMAScript.UI.Vue",
            ImportSpecifier: "vue",
            ExportName: name,
            Props: [],
            Emits: [],
            Slots: [],
            StyleDependencies: [],
            PluginRequirements: [],
            Flags: VueComponentFlags.None);
}

