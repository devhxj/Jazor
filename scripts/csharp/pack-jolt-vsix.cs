#!/usr/bin/env dotnet run
#:property AssemblyName=Jazor.NodeApiVerification
#:property CETCompat=false
#:property PublishAot=false
#:project ../../src/Jazor.NodeHost/Jazor.NodeHost.csproj

using System.IO.Compression;
using System.Text.Json.Nodes;
using System.Xml.Linq;
using Jazor.NodeHost;

if (args is not [var version, var destination])
    throw new ArgumentException("Usage: pack-jolt-vsix.cs <version> <output.vsix>");
var root = Directory.GetCurrentDirectory();
var client = Path.Combine(root, "editors", "jolt");
var store = Environment.GetEnvironmentVariable("JazorPnpmStoreDirectory") ?? Path.Combine(root, ".tmp", "nodeapi-store");
await NodeToolchain.RestoreAsync(client, store, CancellationToken.None, Console.Error);
var manifest = JsonNode.Parse(await File.ReadAllTextAsync(Path.Combine(client, "package.json")))!.AsObject();
manifest["version"] = version;
Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(destination))!);
using var file = File.Create(destination);
using var zip = new ZipArchive(file, ZipArchiveMode.Create);

XNamespace schema = "http://schemas.microsoft.com/developer/vsx-schema/2011";
var identity = new XElement(schema + "Identity", new XAttribute("Language", "en-US"),
    new XAttribute("Id", (string)manifest["name"]!), new XAttribute("Version", version), new XAttribute("Publisher", (string)manifest["publisher"]!));
var metadata = new XElement(schema + "Metadata", identity,
    new XElement(schema + "DisplayName", (string)manifest["displayName"]!), new XElement(schema + "Description", (string)manifest["description"]!),
    new XElement(schema + "Properties", new XElement(schema + "Property", new XAttribute("Id", "Microsoft.VisualStudio.Code.Engine"),
        new XAttribute("Value", (string)manifest["engines"]!["vscode"]!))));
Put("extension.vsixmanifest", new XElement(schema + "PackageManifest", new XAttribute("Version", "2.0.0"), metadata,
    new XElement(schema + "Installation", new XElement(schema + "InstallationTarget", new XAttribute("Id", "Microsoft.VisualStudio.Code"))),
    new XElement(schema + "Dependencies"), new XElement(schema + "Assets", new XElement(schema + "Asset",
        new XAttribute("Type", "Microsoft.VisualStudio.Code.Manifest"), new XAttribute("Path", "extension/package.json"),
        new XAttribute("Addressable", "true")))).ToString());
Put("extension/package.json", manifest.ToJsonString());
var files = new List<string> { "extension.cjs", "jazor.tmLanguage.json", "README.md" };
// NodeHost installs a hoisted tree. Include transitive client dependencies and their licenses.
var modules = Path.Combine(client, "node_modules");
foreach (var directory in Directory.EnumerateDirectories(modules).Where(d => !Path.GetFileName(d).StartsWith('.')))
    files.AddRange(Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories).Select(p => Path.GetRelativePath(client, p)));
foreach (var path in files.Order(StringComparer.Ordinal))
    zip.CreateEntryFromFile(Path.Combine(client, path), "extension/" + path.Replace('\\', '/'));
zip.CreateEntryFromFile(Path.Combine(root, "LICENSE.txt"), "extension/LICENSE.txt");
XNamespace types = "http://schemas.openxmlformats.org/package/2006/content-types";
var extensions = files.Select(Path.GetExtension).OfType<string>().Where(s => s.Length > 1).Select(s => s[1..].ToLowerInvariant())
    .Append("json").Append("txt").Append("vsixmanifest").Distinct().Order();
Put("[Content_Types].xml", new XElement(types + "Types", extensions.Select(e => new XElement(types + "Default",
    new XAttribute("Extension", e), new XAttribute("ContentType", e == "vsixmanifest" ? "text/xml" : "application/octet-stream")))).ToString());

void Put(string path, string contents)
{
    using var writer = new StreamWriter(zip.CreateEntry(path).Open());
    writer.Write(contents);
}
