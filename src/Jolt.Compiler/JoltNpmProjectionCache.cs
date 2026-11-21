using System.Security.Cryptography;
using System.Text.Json;
using Jolt.Npm;
using Microsoft.CodeAnalysis.CSharp;

namespace Jolt.Compiler;

internal static class JoltNpmProjectionCache
{
    // Include the emitting SDK and the host contracts used to validate public
    // source. Package versions alone do not identify a projection algorithm.
    private static readonly Guid[] ToolVersions =
    [
        typeof(JoltNpmProjectionCache).Module.ModuleVersionId,
        typeof(NpmValueExport).Module.ModuleVersionId,
        typeof(ECMAScript.Date).Module.ModuleVersionId,
        typeof(ECMAScript.Vue).Module.ModuleVersionId,
        typeof(ECMAScript.ECMAScriptAttribute).Module.ModuleVersionId,
        typeof(object).Module.ModuleVersionId,
        typeof(CSharpCompilation).Module.ModuleVersionId,
    ];

    internal static JoltNpmValueProjection LoadComponent(string cache, IReadOnlyList<string> namespaces,
        IReadOnlyList<NpmValueExport> exports, CSharpParseOptions options, CancellationToken cancellationToken)
    {
        var locations = new DefinitionIndex();
        var directory = EntryDirectory(cache, namespaces, exports, false, locations);
        var code = Path.Combine(directory, "Projection.g.cs");
        var symbols = Path.Combine(directory, "symbols.json");
        if (File.Exists(code))
        {
            try
            {
                using var symbolStream = OpenSnapshot(symbols);
                var declarations = JsonSerializer.Deserialize<JoltNpmValueProjection.Declaration[]>(symbolStream);
                if (declarations is not null)
                    return new(ReadSource(code), declarations, exports, locations.Definitions, options, cancellationToken);
            }
            catch (FileNotFoundException) { }
            catch (JsonException) { }
        }

        var projection = new JoltNpmValueProjection(namespaces, exports, options);
        Directory.CreateDirectory(directory);
        WriteAtomic(symbols, JsonSerializer.SerializeToUtf8Bytes(projection.GetDeclarations(locations.Indices)), cancellationToken);
        WriteAtomic(code, System.Text.Encoding.UTF8.GetBytes(projection.Tree.GetText(cancellationToken).ToString()), cancellationToken);
        return projection;
    }

    internal static string LoadBindings(string cache, IReadOnlyList<NpmValueExport> exports, CancellationToken cancellationToken)
    {
        var directory = EntryDirectory(cache, [], exports, true, new DefinitionIndex());
        var code = Path.Combine(directory, "Bindings.g.cs");
        if (File.Exists(code))
            return ReadSource(code);

        var source = JoltNpmBindings.GenerateSource(exports, cancellationToken);
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
                .Select(value => new { value.ModuleSpecifier, value.ExportName, value.Definition, value.Signatures, value.ConstantType }),
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
            writer.WriteStringValue(JoltNpmValueProjection.PackageDeclarationPath(value));
            writer.WriteEndArray();
        }

        public override NpmDefinition Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            throw new NotSupportedException();
    }

    // Windows permits atomic replacement during a read only if the reader
    // shares delete access. Concurrent CLI and editor processes can both publish.
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
            // Concurrent builders of this content-addressed entry publish
            // identical bytes. A missing sidecar can also be regenerated.
            File.Move(temporary, file, overwrite: true);
        }
        finally
        {
            File.Delete(temporary);
        }
    }
}
