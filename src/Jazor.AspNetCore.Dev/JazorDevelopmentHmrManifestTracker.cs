using System.Text.Json;

namespace Jazor.AspNetCore.Dev;

/// <summary>
/// Reads compiler-owned metadata from the emitted manifest and compares snapshots.
/// It deliberately has no knowledge of Razor syntax or Vue runtime internals.
/// </summary>
internal sealed class JazorDevelopmentHmrManifestTracker(
    IReadOnlyList<JazorDevelopmentHmrArtifactRegistration> registrations)
{
    private readonly IReadOnlyList<JazorDevelopmentHmrArtifactRegistration> _registrations = registrations;
    private JazorDevelopmentHmrManifestSnapshot _previous = JazorDevelopmentHmrManifestSnapshot.Empty;

    public void Initialize()
    {
        var snapshot = ReadSnapshot();
        if (snapshot.IsValid)
            _previous = snapshot;
    }

    public JazorDevelopmentHmrDecision Evaluate(IReadOnlyList<string> changedPaths)
    {
        ArgumentNullException.ThrowIfNull(changedPaths);

        var current = ReadSnapshot();
        if (!current.IsValid)
        {
            return JazorDevelopmentHmrDecision.FullReload(
                "hmr-manifest-invalid:" + (current.Error ?? "unknown"));
        }

        var decision = Classify(_previous, current, changedPaths);
        _previous = current;
        return decision;
    }

    private JazorDevelopmentHmrManifestSnapshot ReadSnapshot()
    {
        var entries = new Dictionary<string, JazorDevelopmentHmrManifestEntry>(StringComparer.OrdinalIgnoreCase);
        try
        {
            foreach (var registration in _registrations)
            {
                if (!File.Exists(registration.ManifestPath))
                    continue;

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
                    var entry = new JazorDevelopmentHmrManifestEntry(
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
            return JazorDevelopmentHmrManifestSnapshot.Invalid(exception.Message);
        }

        return JazorDevelopmentHmrManifestSnapshot.Valid(entries);
    }

    private JazorDevelopmentHmrDecision Classify(
        JazorDevelopmentHmrManifestSnapshot previous,
        JazorDevelopmentHmrManifestSnapshot current,
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
                return JazorDevelopmentHmrDecision.FullReload("hmr-unmapped-change");

            if (string.Equals(changedPath, registration.ManifestPath, StringComparison.OrdinalIgnoreCase))
            {
                manifestChanged = true;
                continue;
            }

            if (changedPath.EndsWith(".mjs.map", StringComparison.OrdinalIgnoreCase))
                continue;

            if (!changedPath.EndsWith(".mjs", StringComparison.OrdinalIgnoreCase))
                return JazorDevelopmentHmrDecision.FullReload("hmr-non-module-change");

            changedModulePaths.Add(changedPath);
        }

        if (manifestChanged)
        {
            foreach (var changedPath in GetManifestChangedModulePaths(previous, current))
                changedModulePaths.Add(changedPath);
        }

        if (changedModulePaths.Count == 0)
            return JazorDevelopmentHmrDecision.None();

        var updates = new List<JazorDevelopmentHmrModuleUpdate>(changedModulePaths.Count);
        foreach (var changedModulePath in changedModulePaths.OrderBy(static path => path, StringComparer.OrdinalIgnoreCase))
        {
            if (!previous.Entries.TryGetValue(changedModulePath, out var previousEntry) ||
                !current.Entries.TryGetValue(changedModulePath, out var currentEntry))
            {
                return JazorDevelopmentHmrDecision.FullReload("hmr-module-identity-unavailable");
            }

            var moduleDecision = ClassifyModule(previousEntry, currentEntry);
            if (moduleDecision.FullReloadReason is not null)
                return JazorDevelopmentHmrDecision.FullReload(moduleDecision.FullReloadReason);

            if (moduleDecision.Update is not null)
                updates.Add(moduleDecision.Update);
        }

        return updates.Count == 0
            ? JazorDevelopmentHmrDecision.None()
            : JazorDevelopmentHmrDecision.ModuleUpdate(updates);
    }

    private IEnumerable<string> GetManifestChangedModulePaths(
        JazorDevelopmentHmrManifestSnapshot previous,
        JazorDevelopmentHmrManifestSnapshot current)
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

    private static JazorDevelopmentHmrModuleClassification ClassifyModule(
        JazorDevelopmentHmrManifestEntry previous,
        JazorDevelopmentHmrManifestEntry current)
    {
        if (string.Equals(previous.ContentHash, current.ContentHash, StringComparison.Ordinal))
        {
            return Equals(previous.Hmr, current.Hmr)
                ? JazorDevelopmentHmrModuleClassification.NoUpdate()
                : JazorDevelopmentHmrModuleClassification.FullReload("hmr-content-hash-unchanged");
        }

        if (previous.Hmr is null || current.Hmr is null)
            return JazorDevelopmentHmrModuleClassification.FullReload("hmr-metadata-missing");

        if (!string.Equals(previous.Hmr.ComponentId, current.Hmr.ComponentId, StringComparison.Ordinal) ||
            !string.Equals(previous.Hmr.ModuleId, current.Hmr.ModuleId, StringComparison.Ordinal))
        {
            return JazorDevelopmentHmrModuleClassification.FullReload("hmr-module-identity-changed");
        }

        if (!string.Equals(previous.Hmr.DescriptorHash, current.Hmr.DescriptorHash, StringComparison.Ordinal))
            return JazorDevelopmentHmrModuleClassification.FullReload("hmr-descriptor-changed");

        if (!string.Equals(previous.Hmr.LogicHash, current.Hmr.LogicHash, StringComparison.Ordinal))
            return JazorDevelopmentHmrModuleClassification.FullReload("hmr-logic-changed");

        if (string.Equals(previous.Hmr.TemplateHash, current.Hmr.TemplateHash, StringComparison.Ordinal))
            return JazorDevelopmentHmrModuleClassification.FullReload("hmr-unclassified-content-change");

        if (!string.Equals(current.Hmr.BoundaryKind, "template-only", StringComparison.Ordinal))
            return JazorDevelopmentHmrModuleClassification.FullReload("hmr-boundary-requires-reload");

        return JazorDevelopmentHmrModuleClassification.ModuleUpdate(new JazorDevelopmentHmrModuleUpdate(
            current.RelativePath,
            current.RequestUrl,
            current.Hmr.ComponentId,
            current.Hmr.ModuleId,
            current.Hmr.DescriptorHash,
            current.Hmr.TemplateHash,
            current.Hmr.LogicHash,
            current.Hmr.BoundaryKind));
    }

    private JazorDevelopmentHmrArtifactRegistration? FindRegistration(string path)
        => _registrations
            .Where(registration => IsSamePathOrDescendant(path, registration.ArtifactRootPath))
            .OrderByDescending(static registration => registration.ArtifactRootPath.Length)
            .FirstOrDefault();

    private static JazorDevelopmentHmrMetadata? ReadHmrMetadata(JsonElement module)
    {
        if (!TryGetProperty(module, "hmr", out var hmr) || hmr.ValueKind == JsonValueKind.Null)
            return null;
        if (hmr.ValueKind != JsonValueKind.Object)
            throw new InvalidOperationException("hmr metadata must be an object");

        var boundaryKind = ReadRequiredString(hmr, "boundaryKind");
        if (boundaryKind is not ("unknown" or "template-only" or "logic-safe" or "full-reload-required"))
            throw new InvalidOperationException("unsupported hmr boundary kind '" + boundaryKind + "'");

        return new JazorDevelopmentHmrMetadata(
            ReadRequiredString(hmr, "componentId"),
            ReadRequiredString(hmr, "moduleId"),
            ReadRequiredString(hmr, "descriptorHash"),
            ReadRequiredString(hmr, "templateHash"),
            ReadRequiredString(hmr, "logicHash"),
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

internal sealed record JazorDevelopmentHmrArtifactRegistration(
    string ArtifactRootPath,
    string ManifestPath,
    string RequestPath);

internal sealed record JazorDevelopmentHmrManifestSnapshot(
    bool IsValid,
    IReadOnlyDictionary<string, JazorDevelopmentHmrManifestEntry> Entries,
    string? Error = null)
{
    public static JazorDevelopmentHmrManifestSnapshot Empty { get; } = Valid(
        new Dictionary<string, JazorDevelopmentHmrManifestEntry>(StringComparer.OrdinalIgnoreCase));

    public static JazorDevelopmentHmrManifestSnapshot Valid(
        IReadOnlyDictionary<string, JazorDevelopmentHmrManifestEntry> entries)
        => new(true, entries);

    public static JazorDevelopmentHmrManifestSnapshot Invalid(string error)
        => new(false, new Dictionary<string, JazorDevelopmentHmrManifestEntry>(StringComparer.OrdinalIgnoreCase), error);
}

internal sealed record JazorDevelopmentHmrManifestEntry(
    string ArtifactPath,
    string RelativePath,
    string RequestUrl,
    string ContentHash,
    JazorDevelopmentHmrMetadata? Hmr);

internal sealed record JazorDevelopmentHmrMetadata(
    string ComponentId,
    string ModuleId,
    string DescriptorHash,
    string TemplateHash,
    string LogicHash,
    string BoundaryKind);

internal enum JazorDevelopmentHmrDecisionKind
{
    None,
    ModuleUpdate,
    FullReload
}

internal sealed record JazorDevelopmentHmrDecision(
    JazorDevelopmentHmrDecisionKind Kind,
    string Reason,
    IReadOnlyList<JazorDevelopmentHmrModuleUpdate> Updates)
{
    public static JazorDevelopmentHmrDecision None()
        => new(JazorDevelopmentHmrDecisionKind.None, "hmr-no-effective-change", []);

    public static JazorDevelopmentHmrDecision ModuleUpdate(
        IReadOnlyList<JazorDevelopmentHmrModuleUpdate> updates)
        => new(JazorDevelopmentHmrDecisionKind.ModuleUpdate, "hmr-template-only", updates);

    public static JazorDevelopmentHmrDecision FullReload(string reason)
        => new(JazorDevelopmentHmrDecisionKind.FullReload, reason, []);
}

internal sealed record JazorDevelopmentHmrModuleUpdate(
    string Path,
    string Url,
    string ComponentId,
    string ModuleId,
    string DescriptorHash,
    string TemplateHash,
    string LogicHash,
    string BoundaryKind);

internal sealed record JazorDevelopmentHmrModuleClassification(
    JazorDevelopmentHmrModuleUpdate? Update,
    string? FullReloadReason)
{
    public static JazorDevelopmentHmrModuleClassification NoUpdate()
        => new(null, null);

    public static JazorDevelopmentHmrModuleClassification ModuleUpdate(JazorDevelopmentHmrModuleUpdate update)
        => new(update, null);

    public static JazorDevelopmentHmrModuleClassification FullReload(string reason)
        => new(null, reason);
}
