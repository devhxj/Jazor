using System.Text.Json;

namespace Jazor.AspNetCore.Dev;

/// <summary>
/// Compares provider-owned manifest snapshots and returns module updates only for verified
/// Vue template-only changes. Missing or unknown metadata remains a conservative full reload.
/// </summary>
internal sealed class HmrManifestTracker(
    IReadOnlyList<HmrArtifactRegistration> registrations)
{
    private const string VueHmrProviderId = "jazor.vue";

    private readonly IReadOnlyList<HmrArtifactRegistration> _registrations = registrations;
    private HmrManifestSnapshot _previous = HmrManifestSnapshot.Empty;

    /// <summary>Captures the initial manifest baseline before file notifications are processed.</summary>
    public void Initialize()
    {
        var snapshot = ReadSnapshot();
        if (snapshot.IsValid)
            _previous = snapshot;
    }

    /// <summary>Classifies one observed batch as no-op, template-only update, or full reload.</summary>
    public HmrDecision Evaluate(IReadOnlyList<string> changedPaths)
    {
        ArgumentNullException.ThrowIfNull(changedPaths);

        var current = ReadSnapshot();
        if (!current.IsValid)
        {
            return HmrDecision.FullReload(
                "hmr-manifest-invalid:" + (current.Error ?? "unknown"));
        }

        var decision = Classify(_previous, current, changedPaths);
        _previous = current;
        return decision;
    }

    private HmrManifestSnapshot ReadSnapshot()
    {
        var entries = new Dictionary<string, HmrManifestEntry>(StringComparer.OrdinalIgnoreCase);
        var loadedManifestPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            foreach (var registration in _registrations)
            {
                if (!File.Exists(registration.ManifestPath))
                    continue;

                loadedManifestPaths.Add(registration.ManifestPath);

                using var document = JsonDocument.Parse(File.ReadAllText(registration.ManifestPath));
                if (document.RootElement.ValueKind != JsonValueKind.Object ||
                    !TryGetProperty(document.RootElement, "modules", out var modules) ||
                    modules.ValueKind != JsonValueKind.Array)
                {
                    throw new InvalidOperationException("modules array is required");
                }

                foreach (var module in modules.EnumerateArray())
                {
                    if (module.ValueKind != JsonValueKind.Object)
                        throw new InvalidOperationException("module entry must be an object");

                    var relativePath = NormalizeRelativePath(ReadRequiredString(module, "path"));
                    var contentHash = ReadRequiredString(module, "contentHash");
                    var hmr = ReadHmrMetadata(module);
                    var artifactPath = GetArtifactPath(registration.ArtifactRootPath, relativePath);
                    var entry = new HmrManifestEntry(
                        artifactPath,
                        relativePath,
                        CombineRequestPath(registration.RequestPath, relativePath),
                        contentHash,
                        hmr);
                    if (!entries.TryAdd(artifactPath, entry))
                        throw new InvalidOperationException("duplicate artifact path '" + relativePath + "'");
                }
            }
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException or InvalidOperationException)
        {
            return HmrManifestSnapshot.Invalid(exception.Message);
        }

        return HmrManifestSnapshot.Valid(entries, loadedManifestPaths);
    }

    private HmrDecision Classify(
        HmrManifestSnapshot previous,
        HmrManifestSnapshot current,
        IReadOnlyList<string> changedPaths)
    {
        var changedModulePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var manifestChanged = false;
        foreach (var changedPath in changedPaths
                     .Where(static path => !string.IsNullOrWhiteSpace(path))
                     .Select(Path.GetFullPath)
                     .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var registration = FindRegistration(changedPath);
            if (registration is null)
                return HmrDecision.FullReload("hmr-unmapped-change");

            if (string.Equals(changedPath, registration.ManifestPath, StringComparison.OrdinalIgnoreCase))
            {
                manifestChanged = true;
                continue;
            }

            if (changedPath.EndsWith(".mjs.map", StringComparison.OrdinalIgnoreCase))
                continue;

            if (!changedPath.EndsWith(".mjs", StringComparison.OrdinalIgnoreCase))
                return HmrDecision.FullReload("hmr-non-module-change");

            changedModulePaths.Add(changedPath);
        }

        if (manifestChanged)
        {
            foreach (var changedPath in GetManifestChangedModulePaths(previous, current))
                changedModulePaths.Add(changedPath);
        }

        if (changedModulePaths.Count == 0)
            return HmrDecision.None();

        var updates = new List<HmrModuleUpdate>(changedModulePaths.Count);
        foreach (var changedModulePath in changedModulePaths.OrderBy(static path => path, StringComparer.OrdinalIgnoreCase))
        {
            var registration = FindRegistration(changedModulePath);
            if (registration is null)
                return HmrDecision.FullReload("hmr-unmapped-change");

            // A project can use ordinary reload before Emit has written a manifest. Do not turn
            // that normal startup state into an opaque HMR identity failure.
            if (!previous.HasLoadedManifest(registration.ManifestPath) &&
                !current.HasLoadedManifest(registration.ManifestPath))
            {
                return HmrDecision.FullReload("hmr-manifest-unavailable");
            }

            if (!previous.Entries.TryGetValue(changedModulePath, out var previousEntry) ||
                !current.Entries.TryGetValue(changedModulePath, out var currentEntry))
            {
                return HmrDecision.FullReload("hmr-module-identity-unavailable");
            }

            var moduleDecision = ClassifyModule(previousEntry, currentEntry);
            if (moduleDecision.FullReloadReason is not null)
                return HmrDecision.FullReload(moduleDecision.FullReloadReason);

            if (moduleDecision.Update is not null)
                updates.Add(moduleDecision.Update);
        }

        return updates.Count == 0
            ? HmrDecision.None()
            : HmrDecision.ModuleUpdate(updates);
    }

    private IEnumerable<string> GetManifestChangedModulePaths(
        HmrManifestSnapshot previous,
        HmrManifestSnapshot current)
    {
        var allPaths = previous.Entries.Keys
            .Concat(current.Entries.Keys)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static path => path, StringComparer.OrdinalIgnoreCase);
        foreach (var path in allPaths)
        {
            previous.Entries.TryGetValue(path, out var previousEntry);
            current.Entries.TryGetValue(path, out var currentEntry);
            if (Equals(previousEntry, currentEntry))
                continue;

            if (previousEntry is null || currentEntry is null)
            {
                yield return path;
                continue;
            }

            yield return path;
        }
    }

    private static HmrModuleClassification ClassifyModule(
        HmrManifestEntry previous,
        HmrManifestEntry current)
    {
        if (string.Equals(previous.ContentHash, current.ContentHash, StringComparison.Ordinal))
        {
            return Equals(previous.Hmr, current.Hmr)
                ? HmrModuleClassification.NoUpdate()
                : HmrModuleClassification.FullReload("hmr-content-hash-unchanged");
        }

        if (previous.Hmr is null || current.Hmr is null)
            return HmrModuleClassification.FullReload("hmr-metadata-missing");

        if (!string.Equals(previous.Hmr.ProviderId, current.Hmr.ProviderId, StringComparison.Ordinal) ||
            !string.Equals(previous.Hmr.ModuleId, current.Hmr.ModuleId, StringComparison.Ordinal))
        {
            return HmrModuleClassification.FullReload("hmr-module-identity-changed");
        }

        if (!string.Equals(current.Hmr.ProviderId, VueHmrProviderId, StringComparison.Ordinal))
            return HmrModuleClassification.FullReload("hmr-provider-unsupported");

        var previousVue = ReadVueHmrMetadata(previous.Hmr.Payload);
        var currentVue = ReadVueHmrMetadata(current.Hmr.Payload);
        if (!string.Equals(previousVue.ComponentId, currentVue.ComponentId, StringComparison.Ordinal))
            return HmrModuleClassification.FullReload("hmr-module-identity-changed");

        if (!string.Equals(previousVue.DescriptorHash, currentVue.DescriptorHash, StringComparison.Ordinal))
            return HmrModuleClassification.FullReload("hmr-descriptor-changed");

        if (!string.Equals(previousVue.LogicHash, currentVue.LogicHash, StringComparison.Ordinal))
            return HmrModuleClassification.FullReload("hmr-logic-changed");

        if (string.Equals(previousVue.TemplateHash, currentVue.TemplateHash, StringComparison.Ordinal))
            return HmrModuleClassification.FullReload("hmr-unclassified-content-change");

        if (!string.Equals(currentVue.BoundaryKind, "template-only", StringComparison.Ordinal))
            return HmrModuleClassification.FullReload("hmr-boundary-requires-reload");

        return HmrModuleClassification.ModuleUpdate(new HmrModuleUpdate(
            current.RelativePath,
            current.RequestUrl,
            currentVue.ComponentId,
            current.Hmr.ModuleId,
            currentVue.DescriptorHash,
            currentVue.TemplateHash,
            currentVue.LogicHash,
            currentVue.BoundaryKind));
    }

    private HmrArtifactRegistration? FindRegistration(string path)
        => _registrations
            .Where(registration => IsSamePathOrDescendant(path, registration.ArtifactRootPath))
            .OrderByDescending(static registration => registration.ArtifactRootPath.Length)
            .FirstOrDefault();

    private static HmrMetadata? ReadHmrMetadata(JsonElement module)
    {
        if (!TryGetProperty(module, "hmr", out var hmr) || hmr.ValueKind == JsonValueKind.Null)
            return null;
        if (hmr.ValueKind != JsonValueKind.Object)
            throw new InvalidOperationException("hmr metadata must be an object");

        if (!TryGetProperty(hmr, "data", out var payload) || payload.ValueKind != JsonValueKind.Object)
            throw new InvalidOperationException("hmr metadata data must be an object");

        var providerId = ReadRequiredString(hmr, "providerId");
        var moduleId = ReadRequiredString(hmr, "moduleId");
        if (string.Equals(providerId, VueHmrProviderId, StringComparison.Ordinal))
            _ = ReadVueHmrMetadata(payload);

        return new HmrMetadata(
            providerId,
            moduleId,
            payload.GetRawText());
    }

    private static VueHmrMetadata ReadVueHmrMetadata(string payload)
    {
        using var document = JsonDocument.Parse(payload);
        return ReadVueHmrMetadata(document.RootElement);
    }

    private static VueHmrMetadata ReadVueHmrMetadata(JsonElement payload)
    {
        var boundaryKind = ReadRequiredString(payload, "boundaryKind");
        if (boundaryKind is not ("unknown" or "template-only" or "logic-safe" or "full-reload-required"))
            throw new InvalidOperationException("unsupported hmr boundary kind '" + boundaryKind + "'");

        return new VueHmrMetadata(
            ReadRequiredString(payload, "componentId"),
            ReadRequiredString(payload, "descriptorHash"),
            ReadRequiredString(payload, "templateHash"),
            ReadRequiredString(payload, "logicHash"),
            boundaryKind);
    }

    private static string ReadRequiredString(JsonElement element, string propertyName)
    {
        if (!TryGetProperty(element, propertyName, out var property) ||
            property.ValueKind != JsonValueKind.String ||
            string.IsNullOrWhiteSpace(property.GetString()))
        {
            throw new InvalidOperationException("required manifest field '" + propertyName + "' is missing");
        }

        return property.GetString()!;
    }

    private static bool TryGetProperty(JsonElement element, string name, out JsonElement value)
    {
        foreach (var property in element.EnumerateObject())
        {
            if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    private static string NormalizeRelativePath(string relativePath)
    {
        var normalized = relativePath.Replace('\\', '/').TrimStart('/');
        if (string.IsNullOrWhiteSpace(normalized) || Path.IsPathRooted(normalized))
            throw new InvalidOperationException("manifest module path must be relative");

        var segments = normalized
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Where(static segment => segment != ".")
            .ToArray();
        if (segments.Length == 0 || segments.Any(static segment => segment == ".."))
            throw new InvalidOperationException("manifest module path cannot escape the artifact root");

        return string.Join("/", segments);
    }

    private static string GetArtifactPath(string rootPath, string relativePath)
    {
        var path = Path.GetFullPath(Path.Combine(rootPath, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        if (!IsSamePathOrDescendant(path, rootPath))
            throw new InvalidOperationException("manifest module path escapes the artifact root");

        return path;
    }

    private static string CombineRequestPath(string requestPath, string relativePath)
        => requestPath.TrimEnd('/') + "/" + relativePath;

    private static bool IsSamePathOrDescendant(string path, string ancestorPath)
    {
        var relativePath = Path.GetRelativePath(Path.GetFullPath(ancestorPath), Path.GetFullPath(path));
        return string.Equals(relativePath, ".", StringComparison.Ordinal) ||
               (!string.Equals(relativePath, "..", StringComparison.Ordinal) &&
                !relativePath.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal) &&
                !relativePath.StartsWith(".." + Path.AltDirectorySeparatorChar, StringComparison.Ordinal) &&
                !Path.IsPathRooted(relativePath));
    }
}

/// <summary>Connects one physical artifact root, its manifest, and the corresponding browser URL root.</summary>
internal sealed record HmrArtifactRegistration(
    string ArtifactRootPath,
    string ManifestPath,
    string RequestPath);

/// <summary>
/// Immutable HMR baseline. Loaded manifest paths distinguish a normal pre-Emit state from an
/// invalid manifest, which determines whether the caller can issue an ordinary full reload.
/// </summary>
internal sealed record HmrManifestSnapshot(
    bool IsValid,
    IReadOnlyDictionary<string, HmrManifestEntry> Entries,
    IReadOnlySet<string> LoadedManifestPaths,
    string? Error = null)
{
    public static HmrManifestSnapshot Empty { get; } = Valid(
        new Dictionary<string, HmrManifestEntry>(StringComparer.OrdinalIgnoreCase),
        new HashSet<string>(StringComparer.OrdinalIgnoreCase));

    public static HmrManifestSnapshot Valid(
        IReadOnlyDictionary<string, HmrManifestEntry> entries,
        IReadOnlySet<string> loadedManifestPaths)
        => new(true, entries, loadedManifestPaths);

    public static HmrManifestSnapshot Invalid(string error)
        => new(
            false,
            new Dictionary<string, HmrManifestEntry>(StringComparer.OrdinalIgnoreCase),
            new HashSet<string>(StringComparer.OrdinalIgnoreCase),
            error);

    public bool HasLoadedManifest(string manifestPath)
        => LoadedManifestPaths.Contains(manifestPath);
}

/// <summary>Canonical manifest entry keyed by its physical artifact path for change comparison.</summary>
internal sealed record HmrManifestEntry(
    string ArtifactPath,
    string RelativePath,
    string RequestUrl,
    string ContentHash,
    HmrMetadata? Hmr);

/// <summary>Keeps provider identity separate from the provider-owned JSON payload.</summary>
internal sealed record HmrMetadata(
    string ProviderId,
    string ModuleId,
    string Payload);

/// <summary>Vue-specific hashes used to prove that only a template boundary changed.</summary>
internal sealed record VueHmrMetadata(
    string ComponentId,
    string DescriptorHash,
    string TemplateHash,
    string LogicHash,
    string BoundaryKind);

/// <summary>Outcome selected from a manifest comparison before any browser message is created.</summary>
internal enum HmrDecisionKind
{
    None,
    ModuleUpdate,
    FullReload
}

/// <summary>Transport-neutral reload decision; the hub translates it to the stable browser protocol.</summary>
internal sealed record HmrDecision(
    HmrDecisionKind Kind,
    string Reason,
    IReadOnlyList<HmrModuleUpdate> Updates)
{
    public static HmrDecision None()
        => new(HmrDecisionKind.None, "hmr-no-effective-change", []);

    public static HmrDecision ModuleUpdate(
        IReadOnlyList<HmrModuleUpdate> updates)
        => new(HmrDecisionKind.ModuleUpdate, "hmr-template-only", updates);

    public static HmrDecision FullReload(string reason)
        => new(HmrDecisionKind.FullReload, reason, []);
}

/// <summary>Verified module replacement payload before it is projected into the browser JSON envelope.</summary>
internal sealed record HmrModuleUpdate(
    string Path,
    string Url,
    string ComponentId,
    string ModuleId,
    string DescriptorHash,
    string TemplateHash,
    string LogicHash,
    string BoundaryKind);

/// <summary>Per-module comparison result used while building one batch-level <see cref="HmrDecision"/>.</summary>
internal sealed record HmrModuleClassification(
    HmrModuleUpdate? Update,
    string? FullReloadReason)
{
    public static HmrModuleClassification NoUpdate()
        => new(null, null);

    public static HmrModuleClassification ModuleUpdate(HmrModuleUpdate update)
        => new(update, null);

    public static HmrModuleClassification FullReload(string reason)
        => new(null, reason);
}
