using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Jazor.Compiler;

/// <summary>Registers immutable local import inputs for a Roslyn compilation.</summary>
public static class LibraryImportContext
{
    private sealed class State(Compilation compilation, ImmutableArray<LibraryImplementationReference> inputs,
        ImmutableArray<INamedTypeSymbol> additionalRoots = default,
        RuntimeClassPrivateStorage privateStorage = RuntimeClassPrivateStorage.JavaScriptPrivateFields)
    {
        internal ImmutableArray<LibraryImplementationReference> Inputs { get; } = inputs;
        internal Lazy<OrdinaryLibraryImportPlan> Plan { get; } = new(() =>
            OrdinaryLibraryImportPlan.CreateCore(compilation, inputs,
                FindRoots(compilation.Assembly.GlobalNamespace).Concat(additionalRoots.IsDefault ? [] : additionalRoots),
                privateStorage, CancellationToken.None));
    }

    private static readonly ConditionalWeakTable<Compilation, State> States = new();

    /// <summary>Register the locally resolved assembly manifest for one compilation.</summary>
    public static void Configure(Compilation compilation, IEnumerable<LibraryImplementationReference> inputs)
    {
        var references = inputs.ToImmutableArray();
        lock (States)
        {
            if (States.TryGetValue(compilation, out var existing) && existing.Inputs.SequenceEqual(references)) return;
            States.Remove(compilation);
            if (references.Length > 0) States.Add(compilation, new State(compilation, references));
        }
    }

    /// <summary>Carry local import inputs to a derived compilation.</summary>
    public static void Transfer(Compilation source, Compilation destination)
    {
        if (States.TryGetValue(source, out var state)) Configure(destination, state.Inputs);
    }

    /// <summary>Let an integration declare its bound compilation roots and private storage protocol.</summary>
    public static void AddRoots(Compilation compilation, IEnumerable<INamedTypeSymbol> roots,
        RuntimeClassPrivateStorage privateStorage = RuntimeClassPrivateStorage.JavaScriptPrivateFields)
    {
        lock (States)
        {
            if (!States.TryGetValue(compilation, out var state)) return;
            States.Remove(compilation);
            States.Add(compilation, new State(compilation, state.Inputs, roots.ToImmutableArray(), privateStorage));
        }
    }

    /// <summary>Obtain the compilation's complete source-root import plan, if explicitly configured.</summary>
    public static OrdinaryLibraryImportPlan? ForCompilation(Compilation compilation)
        => States.TryGetValue(compilation, out var state) ? state.Plan.Value : null;

    internal static ImmutableArray<LibraryImplementationReference> ReadInputs(IEnumerable<AdditionalText> files, CancellationToken token)
    {
        var builder = ImmutableArray.CreateBuilder<LibraryImplementationReference>();
        foreach (var file in files.Where(static file => file.Path.EndsWith(".jazorimports", StringComparison.OrdinalIgnoreCase)))
        {
            var assets = new List<LibraryImplementationReference>();
            foreach (var line in file.GetText(token)?.Lines ?? Enumerable.Empty<Microsoft.CodeAnalysis.Text.TextLine>())
            {
                var value = line.ToString().Trim();
                if (value.Length == 0) continue;
                var fields = value.Split('|');
                if (fields.Length < 2 || fields[0].Length == 0 || fields[1].Length == 0)
                    throw new NotSupportedException("Invalid ordinary library implementation manifest: " + file.Path);
                if (fields[0] == "asset" && fields.Length >= 3 && fields[2].Length > 0)
                    assets.Add(new LibraryImplementationReference(fields[1], fields[2])
                        { ContentHash = fields.Length > 3 ? fields[3] : null });
                else throw new NotSupportedException("Invalid ordinary library implementation record: " + value);
            }
            foreach (var group in assets.GroupBy(static asset => asset.AssemblyName).OrderBy(static group => group.Key, StringComparer.Ordinal))
            {
                var matches = group.Distinct().ToArray();
                if (matches.Length != 1)
                    throw new NotSupportedException($"Ordinary library '{group.Key}' requires exactly one resolved implementation asset for the selected TFM/RID; found {matches.Length}.");
                builder.Add(matches[0]);
            }
        }
        return builder.ToImmutable();
    }

    private static IEnumerable<INamedTypeSymbol> FindRoots(INamespaceOrTypeSymbol owner)
    {
        foreach (var member in owner.GetMembers())
        {
            if (member is INamedTypeSymbol type && type.DeclaringSyntaxReferences.Length > 0 &&
                Util.IsECMAScriptModuleType(type)) yield return type;
            if (member is INamespaceOrTypeSymbol container)
                foreach (var nested in FindRoots(container)) yield return nested;
        }
    }

}
