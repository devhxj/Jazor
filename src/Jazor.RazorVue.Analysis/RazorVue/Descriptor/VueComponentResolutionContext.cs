using System.Collections.Immutable;

namespace Jazor.RazorVue.Analysis.Descriptor;

public sealed class VueComponentResolutionContext
{
    public VueComponentResolutionContext(string currentNamespace, ImmutableArray<string> imports)
    {
        CurrentNamespace = currentNamespace ?? string.Empty;
        Imports = imports.IsDefault ? ImmutableArray<string>.Empty : imports;
    }

    public string CurrentNamespace { get; }

    public ImmutableArray<string> Imports { get; }

    public static VueComponentResolutionContext Create(string currentNamespace, params string[] imports)
        => new(
            currentNamespace,
            imports is null ? ImmutableArray<string>.Empty : ImmutableArray.Create(imports));
}

