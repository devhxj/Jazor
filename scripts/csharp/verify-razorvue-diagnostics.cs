#!/usr/bin/env dotnet run

using System.Diagnostics;
using System.Text.Json;

var repoRoot = RequireRepositoryRoot();
var outputDirectory = GetOption("--output") ?? Path.Combine(repoRoot, "artifacts", "quality", "razorvue-diagnostics");
var root = Path.GetFullPath(outputDirectory);
Directory.CreateDirectory(root);
var fixtureRoot = Path.Combine(Path.GetTempPath(), "jazor-razorvue-diagnostics-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(fixtureRoot);

try
{
    var source = Path.Combine(fixtureRoot, "Page.razor");
    var generated = Path.Combine(fixtureRoot, "Page.razor.g.cs");
    var artifact = Path.Combine(fixtureRoot, "Page.mjs");
    var map = Path.Combine(fixtureRoot, "Page.mjs.map");
    await File.WriteAllTextAsync(source, "<h1>Hello</h1>\n");
    await File.WriteAllTextAsync(generated, "void BuildRenderTree() { }\n");
    await File.WriteAllTextAsync(artifact, "export default {};\n//# sourceMappingURL=Page.mjs.map\n");
    await File.WriteAllTextAsync(map, "{\"version\":3,\"sources\":[\"Page.razor\"],\"sourcesContent\":[\"<h1>Hello</h1>\"],\"mappings\":\"\"}");

    var success = await RunInspectorAsync("success", source, generated, artifact, map, root);
    Assert(success.ExitCode == 0, "success fixture must exit with code 0");
    Assert(success.Report.GetProperty("schemaVersion").GetString() == "1.0", "success report schema must be 1.0");
    Assert(success.Report.GetProperty("status").GetString() == "succeeded", "success report status must be succeeded");
    Assert(success.Report.GetProperty("diagnostics").GetArrayLength() == 0, "success report must have no diagnostics");

    var mapFailure = await RunInspectorAsync("map-failure", source, generated, artifact, Path.Combine(fixtureRoot, "missing.map"), root);
    Assert(mapFailure.ExitCode != 0, "missing map fixture must fail");
    AssertDiagnostic(mapFailure.Report, "JAZORVGA026", "failed", "module");
    AssertSarif(mapFailure.Sarif, "JAZORVGA026");

    var generatedFailure = await RunInspectorAsync("generated-failure", source, Path.Combine(fixtureRoot, "missing.g.cs"), artifact, null, root);
    Assert(generatedFailure.ExitCode != 0, "missing generated fixture must fail");
    AssertDiagnostic(generatedFailure.Report, "JAZORVGA020", "failed", "generation");
    AssertSarif(generatedFailure.Sarif, "JAZORVGA020");

    var summary = Path.Combine(root, "summary.md");
    await File.WriteAllTextAsync(summary,
        "# RazorVue diagnostics verification\n\n" +
        "- successful chain report (`schemaVersion=1.0`): passed\n" +
        "- source map failure (`JAZORVGA026`): passed\n" +
        "- generated C# failure (`JAZORVGA020`): passed\n");
    Console.WriteLine($"RazorVue diagnostics verification passed. Reports: {root}");
}
finally
{
    try { Directory.Delete(fixtureRoot, recursive: true); } catch { }
}

async Task<InspectionResult> RunInspectorAsync(string name, string source, string generated, string artifact, string? map, string output)
{
    var reportPath = Path.Combine(output, name + ".report.json");
    var sarifPath = Path.Combine(output, name + ".sarif");
    var arguments = new List<string>
    {
        "run", "--file", Path.Combine(repoRoot, "scripts", "csharp", "inspect-razorvue-chain.cs"), "--",
        "--source", source, "--generated", generated, "--artifact", artifact,
        "--report", reportPath, "--sarif", sarifPath
    };
    if (map is not null) { arguments.Add("--map"); arguments.Add(map); }
    using var process = new Process
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = "dotnet", WorkingDirectory = repoRoot, UseShellExecute = false,
            RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true
        }
    };
    foreach (var argument in arguments) process.StartInfo.ArgumentList.Add(argument);
    process.Start();
    var stdout = process.StandardOutput.ReadToEndAsync();
    var stderr = process.StandardError.ReadToEndAsync();
    await process.WaitForExitAsync();
    if (!File.Exists(reportPath) || !File.Exists(sarifPath))
        throw new InvalidOperationException($"Inspector did not write both reports for {name}.\n{await stderr}");
    using var reportDocument = JsonDocument.Parse(await File.ReadAllTextAsync(reportPath));
    using var sarifDocument = JsonDocument.Parse(await File.ReadAllTextAsync(sarifPath));
    return new InspectionResult(process.ExitCode, reportDocument.RootElement.Clone(), sarifDocument.RootElement.Clone());
}

static void AssertDiagnostic(JsonElement report, string id, string status, string category)
{
    Assert(report.GetProperty("status").GetString() == status, $"report status must be {status}");
    var diagnostic = report.GetProperty("diagnostics")[0];
    Assert(diagnostic.GetProperty("id").GetString() == id, $"diagnostic ID must be {id}");
    Assert(diagnostic.GetProperty("category").GetString() == category, $"diagnostic category must be {category}");
    Assert(diagnostic.GetProperty("helpUri").GetString()!.Contains("razorvue-diagnostic-matrix.md#", StringComparison.Ordinal), "diagnostic HelpLink must include documentation anchor");
    Assert(report.GetProperty("remediations").GetArrayLength() == 1, "failed report must contain one remediation");
}

static void AssertSarif(JsonElement sarif, string id)
{
    var result = sarif.GetProperty("runs")[0].GetProperty("results")[0];
    Assert(result.GetProperty("ruleId").GetString() == id, $"SARIF rule ID must be {id}");
    Assert(result.GetProperty("helpUri").GetString()!.Contains("razorvue-diagnostic-matrix.md#", StringComparison.Ordinal), "SARIF HelpLink must include documentation anchor");
}

static void Assert(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}

static string RequireRepositoryRoot()
{
    for (var directory = new DirectoryInfo(Directory.GetCurrentDirectory()); directory is not null; directory = directory.Parent)
        if (File.Exists(Path.Combine(directory.FullName, "Jazor.slnx"))) return directory.FullName;
    throw new InvalidOperationException("Unable to locate Jazor.slnx.");
}

string? GetOption(string name)
{
    var index = Array.IndexOf(args, name);
    return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
}

sealed record InspectionResult(int ExitCode, JsonElement Report, JsonElement Sarif);
