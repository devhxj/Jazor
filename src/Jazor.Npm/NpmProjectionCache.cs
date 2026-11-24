using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.Npm;

internal static class NpmProjectionCache
{
    // Include the emitting SDK and the host contracts used to validate public
    // source. Package versions alone do not identify a projection algorithm.
    private static readonly Guid[] ToolVersions =
    [
        typeof(NpmProjectionCache).Module.ModuleVersionId,
        typeof(NpmValueExport).Module.ModuleVersionId,
        typeof(ECMAScript.Date).Module.ModuleVersionId,
        typeof(ECMAScript.ECMAScriptAttribute).Module.ModuleVersionId,
        typeof(object).Module.ModuleVersionId,
        typeof(CSharpCompilation).Module.ModuleVersionId,
    ];

    internal static NpmValueProjection LoadComponent(string cache, IReadOnlyList<string> namespaces,
        IReadOnlyList<NpmValueExport> exports, CSharpParseOptions options, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var locations = new DefinitionIndex();
        var directory = EntryDirectory(cache, namespaces, exports, false, locations);
        cancellationToken.ThrowIfCancellationRequested();
        var code = Path.Combine(directory, "Projection.g.cs");
        var symbols = Path.Combine(directory, "symbols.json");
        if (File.Exists(code))
        {
            try
            {
                using var symbolStream = OpenSnapshot(symbols);
                var declarations = JsonSerializer.Deserialize<NpmValueProjection.Declaration[]>(symbolStream);
                if (declarations is not null)
                {
                    var cached = new NpmValueProjection(ReadSource(code), declarations, exports, locations.Definitions, options, cancellationToken);
                    cancellationToken.ThrowIfCancellationRequested();
                    return cached;
                }
            }
            catch (FileNotFoundException) { }
            catch (JsonException) { }
        }

        var projection = new NpmValueProjection(namespaces, exports, options);
        var declarationsBytes = JsonSerializer.SerializeToUtf8Bytes(projection.GetDeclarations(locations.Indices));
        var sourceBytes = System.Text.Encoding.UTF8.GetBytes(projection.Tree.GetText(cancellationToken).ToString());
        cancellationToken.ThrowIfCancellationRequested();
        Directory.CreateDirectory(directory);
        // Finish both deterministic writes once committed. The source file is the
        // publication marker for cold readers, so publish it after the index.
        WriteAtomic(symbols, declarationsBytes, CancellationToken.None);
        WriteAtomic(code, sourceBytes, CancellationToken.None);
        return projection;
    }

    internal static string LoadBindings(string cache, IReadOnlyList<string> namespaces, IReadOnlyList<NpmValueExport> exports, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var directory = EntryDirectory(cache, namespaces, exports, true, new DefinitionIndex());
        cancellationToken.ThrowIfCancellationRequested();
        var code = Path.Combine(directory, "Bindings.g.cs");
        if (File.Exists(code))
        {
            var cached = ReadSource(code);
            cancellationToken.ThrowIfCancellationRequested();
            return cached;
        }

        var source = NpmBindings.GenerateSource(namespaces, exports, cancellationToken);
        Directory.CreateDirectory(directory);
        WriteAtomic(code, System.Text.Encoding.UTF8.GetBytes(source), cancellationToken);
        return source;
    }

    private static string EntryDirectory(string cache, IReadOnlyList<string> namespaces,
        IReadOnlyList<NpmValueExport> exports, bool standalone, DefinitionIndex locations)
    {
        var inputs = new
        {
            ToolVersions,
            Standalone = standalone,
            Namespaces = namespaces.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal),
            Exports = exports.OrderBy(value => value.ModuleSpecifier, StringComparer.Ordinal)
                .ThenBy(value => value.ExportName, StringComparer.Ordinal)
                .Select(value => new { value.ModuleSpecifier, value.ExportName, value.PackageName, value.Version, value.Definition, value.Signatures, value.ConstantType }),
        };
        using var digest = SHA256.Create();
        using (var stream = new CryptoStream(Stream.Null, digest, CryptoStreamMode.Write))
            JsonSerializer.Serialize(stream, inputs, new JsonSerializerOptions { Converters = { locations } });
        return Path.Combine(cache, Convert.ToHexStringLower(digest.Hash!));
    }

    // Distinct declarations can have identical members. Preserve which references
    // denote the same declaration, while letting the current checker supply its span.
    private sealed class DefinitionIndex : System.Text.Json.Serialization.JsonConverter<NpmDefinition>
    {
        internal Dictionary<NpmDefinition, int> Indices { get; } = [];
        internal List<NpmDefinition> Definitions { get; } = [];

        public override void Write(Utf8JsonWriter writer, NpmDefinition value, JsonSerializerOptions options)
        {
            if (!Indices.TryGetValue(value, out var index))
            {
                index = Definitions.Count;
                Indices.Add(value, index);
                Definitions.Add(value);
            }
            writer.WriteStartArray();
            writer.WriteNumberValue(index);
            // Package-relative declaration paths participate in emitted host type names.
            writer.WriteStringValue(NpmValueProjection.PackageDeclarationPath(value));
            writer.WriteEndArray();
        }

        public override NpmDefinition Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            throw new NotSupportedException();
    }

    private static FileStream OpenSnapshot(string file) =>
        new(file, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete);

    private static string ReadSource(string file)
    {
        using var reader = new StreamReader(OpenSnapshot(file));
        return reader.ReadToEnd();
    }

    private static void WriteAtomic(string file, byte[] contents, CancellationToken cancellationToken)
    {
        var temporary = file + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            File.WriteAllBytes(temporary, contents);
            cancellationToken.ThrowIfCancellationRequested();
            // Let the first publisher win. On Windows, replacing an open
            // destination can fail even when its reader shares delete access.
            try
            {
                File.Move(temporary, file);
            }
            catch (IOException) when (File.Exists(file))
            {
                // Identical bytes indicate another publisher won. A damaged
                // cache file can still be repaired by the regenerated bytes.
                using (var published = new MemoryStream())
                {
                    using var snapshot = OpenSnapshot(file);
                    snapshot.CopyTo(published);
                    if (contents.AsSpan().SequenceEqual(published.GetBuffer().AsSpan(0, (int)published.Length)))
                        return;
                }
                File.Move(temporary, file, overwrite: true);
            }
        }
        finally
        {
            File.Delete(temporary);
        }
    }
}
