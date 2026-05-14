using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Jazor.RazorVue.Descriptor;

internal sealed record VueComponentDescriptor(
    string Name,
    string FullName,
    VueComponentSourceKind SourceKind,
    string ResolutionNamespace,
    string ImportSpecifier,
    string ExportName,
    string? ContainerContractFullName,
    ImmutableArray<string> RouteTemplates,
    ImmutableArray<VuePropDescriptor> Props,
    ImmutableArray<VueEmitDescriptor> Emits,
    ImmutableArray<VueSlotDescriptor> Slots,
    ImmutableArray<string> StyleDependencies,
    ImmutableArray<string> PluginRequirements,
    VueComponentFlags Flags);

internal static class VueComponentDescriptorRouteTemplateResolver
{
    public static ImmutableArray<string> Resolve(
        ImmutableArray<string> descriptorRouteTemplates,
        string? razorDocumentText)
    {
        if (!descriptorRouteTemplates.IsDefaultOrEmpty)
            return descriptorRouteTemplates;

        if (string.IsNullOrWhiteSpace(razorDocumentText))
            return ImmutableArray<string>.Empty;

        return ParseRazorPageDirectives(razorDocumentText!);
    }

    private static ImmutableArray<string> ParseRazorPageDirectives(string razorDocumentText)
    {
        var builder = ImmutableArray.CreateBuilder<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var rawLine in razorDocumentText.Split(["\r\n", "\n", "\r"], StringSplitOptions.None))
        {
            var line = rawLine.Trim();
            if (!line.StartsWith("@page", StringComparison.Ordinal))
                continue;

            var remainder = line.Substring("@page".Length).TrimStart();
            if (remainder.Length == 0)
                continue;

            var routeTemplate = TryReadQuotedDirectiveValue(remainder);
            if (string.IsNullOrWhiteSpace(routeTemplate))
                continue;

            var normalized = routeTemplate!.Trim();
            if (seen.Add(normalized))
                builder.Add(normalized);
        }

        return builder.ToImmutable();
    }

    private static string? TryReadQuotedDirectiveValue(string text)
    {
        if (text.Length < 2)
            return null;

        var quote = text[0];
        if (quote is not ('"' or '\''))
            return null;

        var closingIndex = text.IndexOf(quote, 1);
        if (closingIndex <= 0)
            return null;

        return text.Substring(1, closingIndex - 1);
    }
}

internal sealed record VueLifecycleDescriptor(
    bool HasOnInitialized,
    bool HasOnInitializedAsync,
    bool HasOnParametersSet,
    bool HasOnParametersSetAsync,
    bool HasOnAfterRender,
    bool HasOnAfterRenderAsync,
    bool HasShouldRender,
    bool HasSetParametersAsync,
    bool HasDispose,
    bool HasDisposeAsync)
{
    public bool HasAnyHook
        => HasOnInitialized || HasOnInitializedAsync ||
           HasOnParametersSet || HasOnParametersSetAsync ||
           HasOnAfterRender || HasOnAfterRenderAsync ||
           HasShouldRender || HasSetParametersAsync ||
           HasDispose || HasDisposeAsync;
}

internal sealed record VueLogicMethodDescriptor(
    string Name,
    int Arity,
    bool IsAsync,
    IMethodSymbol MethodSymbol);

internal sealed record VueLogicFieldDescriptor(
    string Name,
    bool IsReadOnly,
    IFieldSymbol FieldSymbol);

internal sealed record VueLogicDescriptor(
    ImmutableArray<VueLogicFieldDescriptor> Fields,
    ImmutableArray<VueLogicMethodDescriptor> Methods)
{
    public static VueLogicDescriptor Empty { get; } = new(
        ImmutableArray<VueLogicFieldDescriptor>.Empty,
        ImmutableArray<VueLogicMethodDescriptor>.Empty);
}

internal enum VueComponentSourceKind
{
    UserComponent,
    Intrinsic,
    LibraryComponent
}
