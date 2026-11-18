using System.Runtime.CompilerServices;
using System.Text.Json;
using Jazor.ComponentProbe.Npm;

if (args is ["--verify"])
    await VerifyAsync();
else
{
    if (args.Length is not (0 or 3))
        throw new ArgumentException("Usage: dotnet run --project samples/JazorComponentProbe/Npm -- [--verify | fixture-directory package-name export-name]");
    Console.WriteLine(args.Length == 0
        ? await NpmComponentProbe.ReadAsync(FixtureDirectory("current"), "reka-ui", "CollapsibleRoot")
        : await NpmComponentProbe.ReadAsync(args[0], args[1], args[2]));
}

static string FixtureDirectory(string name, [CallerFilePath] string source = "") =>
    Path.Combine(Path.GetDirectoryName(source)!, name);

static async Task VerifyAsync()
{
    using var component = await ReadFixtureAsync("current", "CollapsibleRoot");
    var metadata = component.RootElement;
    Require(metadata.GetProperty("nodeVersion").GetString()!.StartsWith("v26.", StringComparison.Ordinal), "LibNode 26");
    Require(metadata.GetProperty("module").GetString()!.EndsWith("reka-ui/dist/index.js", StringComparison.Ordinal), "resolved ESM export");
    Require(metadata.GetProperty("declaration").GetString()!.EndsWith("reka-ui/dist/index.d.ts", StringComparison.Ordinal), "resolved TypeScript entry");
    var open = metadata.GetProperty("props").GetProperty("open");
    Require(open.GetProperty("type").GetString() == "boolean" && !open.GetProperty("required").GetBoolean(), "optional open prop");
    var emit = metadata.GetProperty("events").GetProperty("update:open")[0][0];
    Require(emit.GetProperty("type").GetString() == "[value: boolean]", "update:open payload");
    var slot = metadata.GetProperty("slots").GetProperty("default").GetProperty("signatures")[0][0];
    Require(slot.GetProperty("type").GetString() == "{ open: boolean; }", "default slot scope");

    using var previous = await ReadFixtureAsync("previous", "NumberFieldRoot");
    using var current = await ReadFixtureAsync("current", "NumberFieldRoot");
    Require(previous.RootElement.GetProperty("version").GetString() == "2.10.1", "previous package version");
    Require(current.RootElement.GetProperty("version").GetString() == "2.11.0", "current package version");
    Require(!previous.RootElement.GetProperty("props").TryGetProperty("allowInvalid", out _), "previous declaration lacks allowInvalid");
    var allowInvalid = current.RootElement.GetProperty("props").GetProperty("allowInvalid");
    Require(allowInvalid.GetProperty("type").GetString() == "boolean" && !allowInvalid.GetProperty("required").GetBoolean(), "current optional allowInvalid prop");
    Console.WriteLine("Verified Reka UI runtime resolution, CollapsibleRoot props/events/slots, and NumberFieldRoot declaration change from 2.10.1 to 2.11.0.");
}

static async Task<JsonDocument> ReadFixtureAsync(string fixture, string exportName) =>
    JsonDocument.Parse(await NpmComponentProbe.ReadAsync(FixtureDirectory(fixture), "reka-ui", exportName));

static void Require(bool result, string description)
{
    if (!result)
        throw new InvalidOperationException($"Could not verify {description}.");
}
