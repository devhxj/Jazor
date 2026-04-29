using System.Collections.Immutable;

namespace Jazor.RazorVue.Descriptor;

/// <summary>
/// Carries namespace and using-import scope for library component resolution.
/// </summary>
public sealed class VueComponentResolutionContext
{
    /// <summary>
    /// Creates a resolution context for the current component declaration scope.
    /// </summary>
    public VueComponentResolutionContext(string currentNamespace, ImmutableArray<string> imports)
    {
        CurrentNamespace = currentNamespace ?? string.Empty;
        Imports = imports.IsDefault ? ImmutableArray<string>.Empty : imports;
    }

    /// <summary>
    /// Gets the namespace of the component being lowered.
    /// </summary>
    public string CurrentNamespace { get; }

    /// <summary>
    /// Gets imported namespaces visible to component lookup.
    /// </summary>
    public ImmutableArray<string> Imports { get; }

    /// <summary>
    /// Creates a context from namespace plus string-based imports.
    /// </summary>
    public static VueComponentResolutionContext Create(string currentNamespace, params string[] imports)
        => new(
            currentNamespace,
            imports is null ? ImmutableArray<string>.Empty : ImmutableArray.Create(imports));
}

