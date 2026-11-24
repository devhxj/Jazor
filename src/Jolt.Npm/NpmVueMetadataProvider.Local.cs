using System.Text.Json;
using Jazor.Npm;
using static Jazor.Npm.NpmValueMetadataProvider;

namespace Jolt.Npm;

public sealed partial class NpmVueMetadataProvider
{
    private readonly System.Collections.Concurrent.ConcurrentDictionary<string, LocalSnapshot> _localCache = new();

    public async Task<IReadOnlyList<LocalVueContract>> ReadLocalComponentsAsync(string projectDirectory,
        IReadOnlyList<string> paths, IReadOnlyDictionary<string, string> overlays, CancellationToken cancellationToken,
        Func<IReadOnlyDictionary<string, string>> projectDeclarations)
        => (await ReadLocalComponentSnapshotAsync(projectDirectory, paths, overlays, cancellationToken, projectDeclarations)).Contracts;

    public async Task<LocalVueSnapshot> ReadLocalComponentSnapshotAsync(string projectDirectory,
        IReadOnlyList<string> paths, IReadOnlyDictionary<string, string> overlays, CancellationToken cancellationToken,
        Func<IReadOnlyDictionary<string, string>> projectDeclarations)
        => await ReadLocalComponentSnapshotAsync(projectDirectory, paths, overlays, cancellationToken, projectDeclarations, null);

    internal async Task<LocalVueSnapshot> ReadLocalComponentSnapshotAsync(string projectDirectory,
        IReadOnlyList<string> paths, IReadOnlyDictionary<string, string> overlays, CancellationToken cancellationToken,
        Func<IReadOnlyDictionary<string, string>> projectDeclarations, Validation? validation)
    {
        if (paths.Count == 0)
            return new([], false);

        var gate = ProjectGates.GetOrAdd(projectDirectory, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken);
        try
        {
            var overlayJson = JsonSerializer.Serialize(overlays.OrderBy(entry => entry.Key, StringComparer.Ordinal).ToDictionary());
            string? declarationsJson = null;
            string SerializeDeclarations() => declarationsJson ??= JsonSerializer.Serialize(projectDeclarations());
            if (_localCache.TryGetValue(projectDirectory, out var cached) && cached.Paths.SequenceEqual(paths) && cached.Overlays == overlayJson &&
                cached.Missing.All(path => !File.Exists(path)) && cached.MissingDirectories.All(path => !Directory.Exists(path)) &&
                (cached.Declarations is null || cached.Declarations == SerializeDeclarations()))
            {
                var changed = cached.Inputs.Select(input => (Path: input.Key, Previous: input.Value,
                        Current: validation is null ? Fingerprint(input.Key) : validation.ReadFingerprint(input.Key)))
                    .Where(input => input.Previous != input.Current).ToArray();
                if (changed.Length == 0)
                {
                    _values.ReportInputs(cached.Inputs, cached.TypeScript);
                    return new(cached.Contracts, cached.Declarations is not null);
                }
                if (changed.All(input => input.Current is not null && cached.Templates.ContainsKey(input.Path)))
                {
                    var sources = changed.Select(input => new { input.Path, Fingerprint = input.Current, cached.Templates[input.Path].DefinitionEnd });
                    var signatures = await RunMetadataAsync(projectDirectory, "vue-template-text.mjs",
                        [cached.TypeScript, JsonSerializer.Serialize(sources, JsonOptions)], cancellationToken, typeof(NpmVueMetadataProvider),
                        ["vue-template-text.mjs", "typescript-input.mjs"]);
                    var refreshed = JsonSerializer.Deserialize<string?[]>(signatures, JsonOptions)!;
                    if (changed.Select(input => cached.Templates[input.Path].Signature).SequenceEqual(refreshed))
                    {
                        foreach (var input in changed)
                            cached.Inputs[input.Path] = input.Current;
                        _values.ReportInputs(cached.Inputs, cached.TypeScript);
                        return new(cached.Contracts, cached.Declarations is not null);
                    }
                }
            }

            validation?.Clear();
            var store = _values.ResolveStore();
            var typescript = await _values.RestoreTypeScriptAsync(store, cancellationToken, typeof(NpmVueMetadataProvider));
            await _values.RestoreAsync(projectDirectory, store, cancellationToken);
            async Task<LocalQuery> ReadAsync(string declarations)
            {
                IReadOnlyList<string> arguments = [typescript, projectDirectory, JsonSerializer.Serialize(paths), overlayJson, declarations, WebIdlCarriers.Metadata];
                var result = await RunMetadataAsync(projectDirectory, "vue-source.mjs", arguments, cancellationToken,
                    typeof(NpmVueMetadataProvider), ["vue-source.mjs", "vue-contract.mjs", "vue-template-text.mjs", "typescript-input.mjs", "declaration-documentation.mjs", "web-idl-projection.mjs"]);
                return JsonSerializer.Deserialize<LocalQuery>(result, JsonOptions)!;
            }
            // Once an SFC imports Jolt, give the checker the current declarations on
            // its first pass. Its result determines whether the next snapshot needs them.
            if (cached?.Declarations is not null)
                SerializeDeclarations();
            var snapshot = await ReadAsync(declarationsJson ?? "{}");
            if (snapshot.UsesJolt && declarationsJson is null)
                snapshot = await ReadAsync(SerializeDeclarations());
            var inputs = snapshot.Inputs.Concat(PackageInputs(projectDirectory)).Distinct(StringComparer.Ordinal);
            var fingerprints = snapshot.Fingerprints;
            foreach (var path in inputs)
                if (!fingerprints.ContainsKey(path))
                    fingerprints[path] = Fingerprint(path);
            _values.ReportInputs(fingerprints, typescript);
            if (snapshot.Contracts.All(contract => contract.Error is null))
            {
                _localCache[projectDirectory] = new(paths.ToArray(), overlayJson, snapshot.UsesJolt ? declarationsJson : null,
                    snapshot.Contracts, fingerprints, snapshot.Missing, snapshot.MissingDirectories, typescript, snapshot.Templates);
            }
            return new(snapshot.Contracts, snapshot.UsesJolt);
        }
        finally
        {
            gate.Release();
        }
    }

    private sealed record TemplateSignature(int DefinitionEnd, string Signature);
    private sealed record LocalQuery(LocalVueContract[] Contracts, string[] Inputs, Dictionary<string, string?> Fingerprints, string[] Missing, string[] MissingDirectories,
        bool UsesJolt, Dictionary<string, TemplateSignature> Templates);
    private sealed record LocalSnapshot(string[] Paths, string Overlays, string? Declarations, LocalVueContract[] Contracts,
        Dictionary<string, string?> Inputs, string[] Missing, string[] MissingDirectories, string TypeScript,
        Dictionary<string, TemplateSignature> Templates);

}

public sealed record LocalVueContract(string Path, NpmVueComponent? Component, string? Error);
public sealed record LocalVueSnapshot(IReadOnlyList<LocalVueContract> Contracts, bool UsesJolt);
