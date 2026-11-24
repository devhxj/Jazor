#!/usr/bin/env dotnet run
#:property AssemblyName=Jazor.NodeApiVerification
#:property CETCompat=false
#:property PublishAot=false
#:project ../../src/Jazor.Npm/Jazor.Npm.csproj

using System.Text.Json;
using Jazor.Npm;

var project = Directory.CreateTempSubdirectory("jazor-runtime-entry-");
try
{
    var package = project.CreateSubdirectory("node_modules/example");
    await File.WriteAllTextAsync(Path.Combine(package.FullName, "entry.mjs"), "export const value = 1;");
    await File.WriteAllTextAsync(Path.Combine(package.FullName, "index.cjs"), "module.exports = { value: 1 };");
    await File.WriteAllTextAsync(Path.Combine(package.FullName, "feature.mjs"), "export const value = 2;");
    var manifest = Path.Combine(package.FullName, "package.json");
    await File.WriteAllTextAsync(manifest, """{"name":"example","main":"index.cjs","module":"entry.mjs"}""");
    await ExpectAsync("example", "entry.mjs");
    await ExpectAsync("example/feature.mjs", "feature.mjs");
    await File.WriteAllTextAsync(manifest, """{"name":"example","main":"index.cjs","module":"entry.mjs","exports":{".":"./index.cjs","./feature":"./feature.mjs"}}""");
    await ExpectAsync("example", "index.cjs");
    await ExpectAsync("example/feature", "feature.mjs");
    await ExpectAsync("example/entry.mjs", null);
    await File.WriteAllTextAsync(manifest, """{"name":"example","main":"index.cjs","module":"entry.mjs","exports":{"./feature":"./feature.mjs"}}""");
    await ExpectAsync("example", null);

    var fixture = project.CreateSubdirectory("fixture");
    await File.WriteAllTextAsync(Path.Combine(project.FullName, "package.json"),
        """{"private":true,"dependencies":{"module-fixture":"file:./fixture"}}""");
    const string legacyManifest = """{"name":"module-fixture","version":"1.0.0","main":"index.cjs","module":"entry.mjs","types":"index.d.ts"}""";
    await File.WriteAllTextAsync(Path.Combine(fixture.FullName, "package.json"), legacyManifest);
    await File.WriteAllTextAsync(Path.Combine(fixture.FullName, "entry.mjs"), """
        export const value = 1;
        export const notify = Object.assign(text => text, { success(text) { return text; } });
        export const notifyMembers = 2;
        export const readValue = value => value;
        """);
    await File.WriteAllTextAsync(Path.Combine(fixture.FullName, "index.cjs"), "module.exports = { value: 1 };");
    await File.WriteAllTextAsync(Path.Combine(fixture.FullName, "index.d.ts"), """
        /**
         * @summary A numeric export.
         * @description
         * A numeric export.
         *
         * Additional detail.
         */
        export declare const value: number;
        /** Notify subscribers. */
        export declare const notify: { (text: string): string; success(text: string): string };
        export declare const notifyMembers: number;
        /**
         * Read the value.
         * @param value - Input value.
         * @typeParam TValue - The input type.
         * @template TResult - The result type.
         * @returns The value.
         * @remarks See {@linkcode Number} and {@link https://example.com}.
         * @example readValue(1)
         * @example readValue(2)
         * @deprecated
         */
        export declare function readValue<TValue, TResult>(value: number): number;
        """);
    var store = Environment.GetEnvironmentVariable("JazorPnpmStoreDirectory") ?? Path.GetFullPath(".tmp/nodeapi-store");
    var provider = new NpmValueMetadataProvider(store);
    var document = Path.Combine(project.FullName, "Probe.cs");
    var docs = (await provider.ReadValueAsync(document, "module-fixture", "value")).Definition?.Documentation;
    if (docs is not { Summary: "A numeric export.", Remarks: "Additional detail." })
        throw new InvalidOperationException("Expected date-fns-style summary and description on the npm definition.");
    var tagged = (await provider.ReadValueAsync(document, "module-fixture", "readValue")).Definition?.Documentation;
    if (tagged is not { Summary: "Read the value.", Returns: "The value.", Remarks: "See Number and https://example.com.",
            Examples: ["readValue(1)", "readValue(2)"], Deprecated: "" } ||
        tagged.Parameters?["value"] != "Input value." ||
        tagged.TypeParameters?["TValue"] != "The input type." ||
        tagged.TypeParameters?["TResult"] != "The result type.")
        throw new InvalidOperationException("Expected structured tags on the npm definition.");
    var index = Path.Combine(project.FullName, "completion.txt");
    var request = string.Join('\n', Path.Combine(project.FullName, "Probe.csproj"), "Npm.ModuleFixture", store);
    if (await NpmValueMetadataProvider.WriteEditorExportsAsync(request, index, Console.Error, cancellationToken: default) != 0 ||
        (await File.ReadAllLinesAsync(index))[0] != "Notify\tNotifyMembers\tNotifyMembers_2\tReadValue\tValue")
        throw new InvalidOperationException("Cold completion must distinguish the callable's companion from the real export.");
    var projected = await provider.ReadValueProjectionsAsync(document, "module-fixture", ["value"]);
    if (projected.Values is not [{ ModuleSpecifier: "module-fixture", ConstantType.Kind: "number" }])
        throw new InvalidOperationException("Expected the legacy module to project with a bare import specifier.");

    var installedManifest = Path.Combine(project.FullName, "node_modules", "module-fixture", "package.json");
    await ChangeRuntimeEntryAsync("index.cjs");
    try
    {
        await provider.ReadValueProjectionsAsync(document, "module-fixture", ["value"]);
        throw new InvalidOperationException("Expected the warm query to reject the new CommonJS entry.");
    }
    catch (Exception ex) when (ex.Message.Contains("does not resolve to a public ESM file", StringComparison.Ordinal))
    {
    }

    Console.WriteLine("Verified legacy module entry, explicit exports authority, subpath resolution, and warm value projection.");

    async Task ChangeRuntimeEntryAsync(string entry)
    {
        var contents = legacyManifest.Replace("entry.mjs", entry, StringComparison.Ordinal);
        await File.WriteAllTextAsync(Path.Combine(fixture.FullName, "package.json"), contents);
        await File.WriteAllTextAsync(installedManifest, contents);
    }

    async Task ExpectAsync(string specifier, string? entry)
    {
        var output = await NpmValueMetadataProvider.RunMetadataAsync(project.FullName, "resolve-module.mjs",
            [specifier, "{}"], CancellationToken.None);
        var resolved = JsonSerializer.Deserialize<string>(output);
        if (Path.GetFileName(resolved) != entry)
            throw new InvalidOperationException($"Expected '{specifier}' to resolve to '{entry}', got '{resolved}'.");
    }
}
finally
{
    project.Delete(recursive: true);
}
