#:property CETCompat=false
#:property PublishAot=false
#:project ../../src/Jolt.Npm/Jolt.Npm.csproj

using Jolt.Npm;
using System.Text.Json;

var store = Environment.GetEnvironmentVariable("JazorPnpmStoreDirectory") ?? Path.GetFullPath(".tmp/nodeapi-store");
var project = Directory.CreateDirectory(Path.Combine(".tmp", $"npm-component-cache-{Guid.NewGuid():N}")).FullName;
try
{
    var package = Directory.CreateDirectory(Path.Combine(project, "widget")).FullName;
    File.WriteAllText(Path.Combine(project, "package.json"), """{"private":true,"dependencies":{"vue":"3.5.42","widget":"link:./widget"}}""");
    File.WriteAllText(Path.Combine(package, "package.json"), """{"name":"widget","version":"1.0.0","type":"module","exports":{"types":"./index.d.ts","import":"./index.js"}}""");
    File.WriteAllText(Path.Combine(package, "index.js"), "export const Widget = {}, Alternate = {};");
    var declaration = Path.Combine(package, "shape.d.ts");
    File.WriteAllText(Path.Combine(package, "index.d.ts"), "export * from './shape.js';");
    File.WriteAllText(declaration, "export declare const Widget: import('vue').DefineComponent<{ title: string }>;");
    var document = Path.Combine(project, "Probe.jazor");

    await Check("Widget", "title");
    var cache = Path.Combine(project, ".jazor", "cache", "npm-metadata");
    var files = Directory.GetFiles(cache, "*.json");
    if (files.Length != 2)
        throw new InvalidOperationException("Expected independent caches for the list and component projection.");
    var prior = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    foreach (var file in files)
        File.SetLastWriteTimeUtc(file, prior);
    await Check("Widget", "title");
    if (files.Any(file => File.GetLastWriteTimeUtc(file) != prior))
        throw new InvalidOperationException("A fresh provider must reuse both persisted snapshots.");

    File.WriteAllText(declaration, "export declare const Widget: import('vue').DefineComponent<{ label: number }>; export { Widget as Alternate };");
    await Check("Widget", "label", "Alternate");
    if (files.Any(file => File.GetLastWriteTimeUtc(file) == prior))
        throw new InvalidOperationException("Both snapshots must be refreshed when their linked declaration changes.");
    Console.WriteLine("Verified persistent npm component metadata reuse and linked declaration invalidation.");

    async Task Check(string export, string prop, string? additional = null)
    {
        var provider = new NpmVueMetadataProvider(store);
        var exports = await provider.ListExportsAsync(document, "widget");
        var expected = additional is null ? new[] { export } : new[] { additional, export };
        var component = await provider.ReadAsync(document, "widget", export);
        if (!exports.SequenceEqual(expected) || !component.Props.ContainsKey(prop))
            throw new InvalidOperationException($"Unexpected component metadata: {JsonSerializer.Serialize(new { exports, component.Props })}");
    }
}
finally
{
    Directory.Delete(project, recursive: true);
}
