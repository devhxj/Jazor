using System.Collections.Immutable;

namespace Jazor.RazorVue.Descriptor;

internal static class VueIntrinsicComponentDescriptors
{
    public static ImmutableArray<VueComponentDescriptor> All { get; } =
    [
        Create("Teleport"),
        Create("Transition"),
        Create("KeepAlive"),
        Create("Suspense"),
        CreateCascadingValueProvider()
    ];

    private static VueComponentDescriptor Create(string name)
        => new(
            Name: name,
            FullName: $"ECMAScript.UI.Vue.{name}",
            SourceKind: VueComponentSourceKind.Intrinsic,
            ResolutionNamespace: "ECMAScript.UI.Vue",
            ImportSpecifier: "vue",
            ExportName: name,
            ContainerContractFullName: null,
            RouteTemplates: [],
            Props: [],
            Emits: [],
            Slots: [],
            StyleDependencies: [],
            PluginRequirements: [],
            Flags: VueComponentFlags.None,
            CascadingParameters: []);

    private static VueComponentDescriptor CreateCascadingValueProvider()
        => new(
            Name: VueCascadingValueProviderDescriptor.Name,
            FullName: VueCascadingValueProviderDescriptor.FullName,
            SourceKind: VueComponentSourceKind.Intrinsic,
            ResolutionNamespace: "Jazor.RazorVue.Intrinsics",
            ImportSpecifier: "vue",
            ExportName: VueCascadingValueProviderDescriptor.Name,
            ContainerContractFullName: null,
            RouteTemplates: [],
            Props:
            [
                new VuePropDescriptor(
                    Name: VueCascadingValueProviderDescriptor.ProvideKeyPropName,
                    PublicName: "ProvideKey",
                    TypeName: "string",
                    Required: true,
                    AcceptsBinding: false,
                    DefaultExpression: null,
                    DefaultSource: VuePropDefaultSource.None,
                    Kind: VuePropKind.Normal,
                    CaptureUnmatchedValues: false),
                new VuePropDescriptor(
                    Name: VueCascadingValueProviderDescriptor.ValuePropName,
                    PublicName: "Value",
                    TypeName: "object?",
                    Required: false,
                    AcceptsBinding: false,
                    DefaultExpression: null,
                    DefaultSource: VuePropDefaultSource.None,
                    Kind: VuePropKind.Normal,
                    CaptureUnmatchedValues: false),
                new VuePropDescriptor(
                    Name: VueCascadingValueProviderDescriptor.IsFixedPropName,
                    PublicName: "IsFixed",
                    TypeName: "bool",
                    Required: false,
                    AcceptsBinding: false,
                    DefaultExpression: "false",
                    DefaultSource: VuePropDefaultSource.None,
                    Kind: VuePropKind.Normal,
                    CaptureUnmatchedValues: false)
            ],
            Emits: [],
            Slots:
            [
                new VueSlotDescriptor(
                    Name: "default",
                    PublicName: "ChildContent",
                    NamePattern: null,
                    PatternOnly: false,
                    IsDefault: true,
                    Parameters: [],
                    Required: false)
            ],
            StyleDependencies: [],
            PluginRequirements: [],
            Flags: VueComponentFlags.None,
            CascadingParameters: []);
}
