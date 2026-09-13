#!/usr/bin/env dotnet run

using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

var repoRoot = RequireRepositoryRoot();
var reportPath = GetOption("--report");
var baselinePath = GetOption("--baseline");
var generatorProject = Path.Combine(repoRoot, "src", "ECMAScript.Vue.Generator", "ECMAScript.Vue.Generator.csproj");
var checks = new[]
{
    new Check("elementplus", ["elementplus", "--check"]),
    new Check("vuetify", ["vuetify", "--check"]),
    new Check("tdesign snapshot", ["tdesign", "snapshot", "--check"]),
    new Check("tdesign bindings", ["tdesign", "bindings", "--check"]),
    new Check("tdesign components", ["tdesign", "components", "--check"])
};

var checkResults = new List<BindingCheckResult>();
foreach (var check in checks)
{
    Console.WriteLine($"[binding-contract] {check.Name}");
    try
    {
        await RunDotNetAsync(generatorProject, check.Arguments, repoRoot);
        checkResults.Add(new BindingCheckResult(check.Name, true, null));
    }
    catch (Exception exception)
    {
        checkResults.Add(new BindingCheckResult(check.Name, false, exception.Message));
    }
}

var targets = new[]
{
    new BindingTarget("element-plus", "2.14.5", Path.Combine(repoRoot, "src", "ECMAScript.ElementPlus"), Path.Combine(repoRoot, "src", "ECMAScript.Vue.Generator", "upstream", "element-plus", "2.14.5"), "Element Plus"),
    new BindingTarget("vuetify", "4.2.1", Path.Combine(repoRoot, "src", "ECMAScript.Vuetify"), Path.Combine(repoRoot, "src", "ECMAScript.Vue.Generator", "upstream", "vuetify", "4.2.1"), "Vuetify"),
    new BindingTarget("tdesign-vue-next", "1.20.7", Path.Combine(repoRoot, "src", "ECMAScript.TDesign"), Path.Combine(repoRoot, "src", "ECMAScript.Vue.Generator", "upstream", "tdesign-vue-next", "1.20.7"), "TDesign")
};

var targetResults = new List<BindingTargetResult>();
foreach (var target in targets)
{
    try
    {
        VerifyTarget(target);
        targetResults.Add(new BindingTargetResult(target.DisplayName, target.LibraryId, target.Version, true, null, ReadInventory(target)));
    }
    catch (Exception exception)
    {
        targetResults.Add(new BindingTargetResult(target.DisplayName, target.LibraryId, target.Version, false, exception.Message, null));
        Console.Error.WriteLine(exception.Message);
    }
}

if (reportPath is not null)
    WriteReport(reportPath, checkResults, targetResults, baselinePath);

if (checkResults.Any(static result => !result.Passed) || targetResults.Any(static result => !result.Passed))
    Environment.ExitCode = 1;
else
    Console.WriteLine("Vue binding contract gate passed.");

static void VerifyTarget(BindingTarget target)
{
    var manifestPath = Path.Combine(target.ProjectDirectory, "manifest.json");
    var readmePath = Path.Combine(target.ProjectDirectory, "README.md");
    if (!File.Exists(manifestPath) || !File.Exists(readmePath) || !Directory.Exists(target.UpstreamDirectory))
        throw new InvalidOperationException($"{target.DisplayName}: project, README, manifest, or upstream snapshot is missing.");

    using var manifest = JsonDocument.Parse(File.ReadAllText(manifestPath));
    var root = manifest.RootElement;
    var libraryId = root.GetProperty("libraryId").GetString();
    var version = root.GetProperty("version").GetString();
    if (!string.Equals(libraryId, target.LibraryId, StringComparison.Ordinal) || !string.Equals(version, target.Version, StringComparison.Ordinal))
        throw new InvalidOperationException($"{target.DisplayName}: manifest identity is {libraryId}@{version}, expected {target.LibraryId}@{target.Version}.");

    var upstreamMetadata = Path.Combine(target.UpstreamDirectory, "package.json");
    if (!File.Exists(upstreamMetadata))
        throw new InvalidOperationException($"{target.DisplayName}: upstream package.json is missing.");

    using var package = JsonDocument.Parse(File.ReadAllText(upstreamMetadata));
    var upstreamVersion = package.RootElement.TryGetProperty("version", out var versionProperty) ? versionProperty.GetString() : null;
    if (!string.Equals(upstreamVersion, target.Version, StringComparison.Ordinal))
        throw new InvalidOperationException($"{target.DisplayName}: upstream snapshot version is {upstreamVersion}, expected {target.Version}.");

    var readme = File.ReadAllText(readmePath);
    if (!readme.Contains("注释", StringComparison.Ordinal) || !readme.Contains("manifest.json", StringComparison.Ordinal))
        throw new InvalidOperationException($"{target.DisplayName}: README must document original comments and manifest resource ownership.");

    Console.WriteLine($"  {target.DisplayName}: {target.LibraryId}@{target.Version}; source docs and manifest present");
}

static BindingContractInventory ReadInventory(BindingTarget target)
{
    var metadataPath = target.LibraryId switch
    {
        "element-plus" => Path.Combine(target.UpstreamDirectory, "web-types.json"),
        "vuetify" => Path.Combine(target.UpstreamDirectory, "contracts.json"),
        "tdesign-vue-next" => Path.Combine(target.UpstreamDirectory, "components.json"),
        _ => throw new InvalidOperationException($"Unsupported binding inventory source: {target.LibraryId}")
    };
    using var document = JsonDocument.Parse(File.ReadAllText(metadataPath));
    var components = document.RootElement.TryGetProperty("components", out var directComponents)
        ? directComponents
        : document.RootElement.GetProperty("contributions").GetProperty("html").GetProperty("vue-components");
    var exports = new SortedSet<string>(StringComparer.Ordinal);
    var props = new SortedSet<string>(StringComparer.Ordinal);
    var events = new SortedSet<string>(StringComparer.Ordinal);
    var slots = new SortedSet<string>(StringComparer.Ordinal);
    var members = new SortedSet<string>(StringComparer.Ordinal);
    var componentCount = 0;
    foreach (var component in components.EnumerateArray())
    {
        componentCount++;
        AddString(component, "export", exports);
        AddString(component, "runtimeExport", exports);
        AddString(component, "sourceExport", exports);
        if (target.LibraryId == "element-plus" && component.TryGetProperty("source", out var source) &&
            source.TryGetProperty("symbol", out var symbol) && symbol.ValueKind == JsonValueKind.String)
            exports.Add(symbol.GetString()!);
        if (target.LibraryId == "element-plus" && component.TryGetProperty("js", out var js) &&
            js.TryGetProperty("events", out var jsEvents))
            AddMembersValue(jsEvents, events);
        AddMembers(component, "props", props);
        AddMembers(component, "events", events);
        AddMembers(component, "slots", slots);
        if (component.TryGetProperty("members", out var declaredMembers) && declaredMembers.ValueKind == JsonValueKind.Array)
        {
            foreach (var member in declaredMembers.EnumerateArray())
            {
                var kind = member.TryGetProperty("kind", out var kindValue) ? kindValue.GetString() : null;
                var name = member.TryGetProperty("runtimeName", out var runtimeName) ? runtimeName.GetString() : null;
                if (kind is not null && name is not null)
                {
                    var value = kind + ":" + name;
                    members.Add(value);
                    if (kind == "prop") props.Add(name);
                    else if (kind == "event") events.Add(name);
                    else if (kind == "slot") slots.Add(name);
                }
            }
        }
    }

    var fingerprintInput = string.Join("\n", exports.Select(static value => "export:" + value)
        .Concat(props.Select(static value => "prop:" + value))
        .Concat(events.Select(static value => "event:" + value))
        .Concat(slots.Select(static value => "slot:" + value)));
    fingerprintInput = string.Join("\n", fingerprintInput.Split('\n').Concat(members.Select(static value => "member:" + value)));
    var fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(fingerprintInput))).ToLowerInvariant();
    return new BindingContractInventory(componentCount, exports, props, events, slots, members, fingerprint);
}

static void AddMembers(JsonElement component, string propertyName, ISet<string> values)
{
    if (!component.TryGetProperty(propertyName, out var members) || members.ValueKind != JsonValueKind.Array)
        return;
    foreach (var member in members.EnumerateArray())
    {
        if (member.ValueKind == JsonValueKind.String)
        {
            values.Add(member.GetString()!);
            continue;
        }
        if (member.TryGetProperty("name", out var name) && name.ValueKind == JsonValueKind.String)
            values.Add(name.GetString()!);
    }
}

static void AddMembersValue(JsonElement members, ISet<string> values)
{
    if (members.ValueKind != JsonValueKind.Array)
        return;
    foreach (var member in members.EnumerateArray())
    {
        if (member.ValueKind == JsonValueKind.String)
            values.Add(member.GetString()!);
        else if (member.TryGetProperty("name", out var name) && name.ValueKind == JsonValueKind.String)
            values.Add(name.GetString()!);
    }
}

static void AddString(JsonElement element, string propertyName, ISet<string> values)
{
    if (element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String)
        values.Add(value.GetString()!);
}

static async Task RunDotNetAsync(string projectPath, IReadOnlyList<string> commandArguments, string workdir)
{
    using var process = new Process
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = "dotnet", WorkingDirectory = workdir, UseShellExecute = false,
            RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true
        }
    };
    process.StartInfo.ArgumentList.Add("run");
    process.StartInfo.ArgumentList.Add("--project");
    process.StartInfo.ArgumentList.Add(projectPath);
    process.StartInfo.ArgumentList.Add("--");
    foreach (var argument in commandArguments)
        process.StartInfo.ArgumentList.Add(argument);

    process.Start();
    var stdout = process.StandardOutput.ReadToEndAsync();
    var stderr = process.StandardError.ReadToEndAsync();
    await process.WaitForExitAsync();
    var output = await stdout;
    var error = await stderr;
    if (process.ExitCode == 0)
    {
        Console.Write(output);
        return;
    }

    throw new InvalidOperationException($"Binding generator check failed ({string.Join(' ', commandArguments)}).{Environment.NewLine}{output}{Environment.NewLine}{error}");
}

static void WriteReport(string path, IReadOnlyList<BindingCheckResult> checks, IReadOnlyList<BindingTargetResult> targets, string? baselinePath)
{
    var fullPath = Path.GetFullPath(path);
    Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
    var baseline = baselinePath is null ? null : ReadBaseline(baselinePath);
    var diffs = targets.Select(target => CreateDiff(target, baseline)).ToArray();
    var report = new BindingContractReport(
        "1.0",
        checks.All(static result => result.Passed) && targets.All(static result => result.Passed) ? "passed" : "failed",
        checks,
        targets,
        targets.Select(static target => target.Inventory).OfType<BindingContractInventory>().ToArray(), diffs);
    File.WriteAllText(fullPath, JsonSerializer.Serialize(report, new JsonSerializerOptions
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    }) + Environment.NewLine);
    var summaryPath = Path.ChangeExtension(fullPath, ".md");
    var lines = new List<string>
    {
        "# Vue binding contract verification",
        "",
        $"- Schema: `1.0`",
        $"- Status: `{report.Status}`",
        "",
        "| Check | Result |",
        "| --- | --- |"
    };
    foreach (var result in checks)
        lines.Add($"| `{result.Name}` | {(result.Passed ? "passed" : "failed")} |");
    foreach (var result in targets)
    {
        var inventory = result.Inventory;
        var inventoryText = inventory is null
            ? ""
            : $"; {inventory.Components} components, {inventory.Exports} exports, {inventory.Props} props, {inventory.Events} events, {inventory.Slots} slots; fingerprint `{inventory.Fingerprint[..12]}`";
        lines.Add($"| `{result.Name}` `{result.LibraryId}@{result.Version}` | {(result.Passed ? "passed" : "failed")}{inventoryText} |");
    }
    File.WriteAllText(summaryPath, string.Join(Environment.NewLine, lines) + Environment.NewLine);
}

static BindingContractDiff CreateDiff(BindingTargetResult target, BindingBaseline? baseline)
{
    var previous = baseline?.Targets.FirstOrDefault(item => string.Equals(item.LibraryId, target.LibraryId, StringComparison.Ordinal));
    var current = target.Inventory;
    if (previous is null || current is null)
        return new BindingContractDiff(target.LibraryId, "baseline-unavailable", Array.Empty<string>(), Array.Empty<string>(), 0, 0, 0, 0);
    var changed = current.Fingerprint == previous.Inventory?.Fingerprint ? Array.Empty<string>() : new[] { "inventory" };
    var previousInventory = previous.Inventory;
    var deltas = new List<string>();
    AddDelta(deltas, "components", current.Components, previousInventory?.Components);
    AddDelta(deltas, "props", current.Props, previousInventory?.Props);
    AddDelta(deltas, "events", current.Events, previousInventory?.Events);
    AddDelta(deltas, "slots", current.Slots, previousInventory?.Slots);
    AddDelta(deltas, "members", current.Members, previousInventory?.Members);
    return new BindingContractDiff(target.LibraryId, changed.Length == 0 ? "unchanged" : "changed", changed.Concat(deltas).ToArray(), Array.Empty<string>(), current.Components - (previousInventory?.Components ?? 0), current.Props - (previousInventory?.Props ?? 0), current.Events - (previousInventory?.Events ?? 0), current.Slots - (previousInventory?.Slots ?? 0));
}

static void AddDelta(ICollection<string> deltas, string name, int current, int? previous)
{
    if (previous is not null && current != previous.Value)
        deltas.Add($"{name}:{previous.Value}->{current}");
}

static BindingBaseline? ReadBaseline(string path)
{
    if (!File.Exists(path))
        throw new InvalidOperationException($"Binding baseline report does not exist: {path}");
    return JsonSerializer.Deserialize<BindingBaseline>(File.ReadAllText(path), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
}

string? GetOption(string name)
{
    var index = Array.IndexOf(args, name);
    return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
}

static string RequireRepositoryRoot()
{
    for (var directory = new DirectoryInfo(Directory.GetCurrentDirectory()); directory is not null; directory = directory.Parent)
        if (File.Exists(Path.Combine(directory.FullName, "Jazor.slnx")))
            return directory.FullName;
    throw new InvalidOperationException("Unable to locate Jazor.slnx.");
}

sealed record Check(string Name, IReadOnlyList<string> Arguments);
sealed record BindingTarget(string LibraryId, string Version, string ProjectDirectory, string UpstreamDirectory, string DisplayName);
sealed record BindingCheckResult(string Name, bool Passed, string? Error);
sealed record BindingTargetResult(string Name, string LibraryId, string Version, bool Passed, string? Error, BindingContractInventory? Inventory);
sealed record BindingContractInventory(int Components, IReadOnlyCollection<string> ExportNames, IReadOnlyCollection<string> PropNames, IReadOnlyCollection<string> EventNames, IReadOnlyCollection<string> SlotNames, IReadOnlyCollection<string> MemberNames, string Fingerprint)
{
    public int Exports => ExportNames.Count;
    public int Props => PropNames.Count;
    public int Events => EventNames.Count;
    public int Slots => SlotNames.Count;
    public int Members => MemberNames.Count;
}
sealed record BindingContractReport(string SchemaVersion, string Status, IReadOnlyList<BindingCheckResult> Checks, IReadOnlyList<BindingTargetResult> Targets, IReadOnlyList<BindingContractInventory> Inventories, IReadOnlyList<BindingContractDiff> Diffs);
sealed record BindingContractDiff(string LibraryId, string Status, IReadOnlyList<string> Changed, IReadOnlyList<string> Removed, int ComponentDelta, int PropDelta, int EventDelta, int SlotDelta);
sealed record BindingBaseline(IReadOnlyList<BindingTargetResult> Targets);
