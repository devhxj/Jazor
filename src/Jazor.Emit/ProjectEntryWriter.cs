using System.Text;
using Jazor.Common;

namespace Jazor.Emit;

/// <summary>Writes the visible entries of the generated standard JavaScript project.</summary>
internal static class ProjectEntryWriter
{
    public const string BrowserEntryFileName = "entry.js";
    public const string SsrEntryFileName = "ssr-entry.js";
    public const string HydrationEntryFileName = "hydration.js";
    private const string SsrRuntimeResourceName = "Jazor.Emit.tooling.ssr-runner.js";

    public static IReadOnlyList<string> Write(
        string projectRoot,
        IReadOnlyList<string> browserEntries,
        bool enableSsr = false,
        IReadOnlyList<string>? hydrationRoots = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectRoot);
        ArgumentNullException.ThrowIfNull(browserEntries);

        var root = Path.GetFullPath(projectRoot);
        Directory.CreateDirectory(root);

        var written = new List<string>
        {
            WriteEntry(root, BrowserEntryFileName, browserEntries)
        };

        var ssrPath = Path.Combine(root, SsrEntryFileName);
        if (!enableSsr)
        {
            if (File.Exists(ssrPath))
                File.Delete(ssrPath);
            var hydrationPath = Path.Combine(root, HydrationEntryFileName);
            if (File.Exists(hydrationPath))
                File.Delete(hydrationPath);
        }
        else
        {
            written.Add(WriteSsrEntry(root));
            written.Add(WriteHydrationEntry(root, hydrationRoots ?? browserEntries));
        }

        return written;
    }

    private static string WriteEntry(string projectRoot, string fileName, IEnumerable<string> roots)
    {
        var content = CreateExports(fileName, roots);
        var destination = Path.Combine(projectRoot, fileName);
        ProjectFileWriter.Write(destination, content);
        return destination;
    }

    private static string WriteSsrEntry(string projectRoot)
    {
        var runtime = ReadResource(SsrRuntimeResourceName);
        // Browser roots may execute DOM/mount code. The SSR entry imports a component only
        // when a render request selects it; both entries still share the same package graph.
        var destination = Path.Combine(projectRoot, SsrEntryFileName);
        ProjectFileWriter.Write(destination, runtime);
        return destination;
    }

    private static string WriteHydrationEntry(string projectRoot, IEnumerable<string> roots)
    {
        // Literal dynamic imports expose the component graph to both development and production tools.
        var loaders = roots.Distinct(StringComparer.Ordinal).OrderBy(path => path, StringComparer.Ordinal)
            .Select(path => $"  {Quote(path)}: () => import({Quote(ECMAScriptModulePath.ResolveRelativeToImporter(HydrationEntryFileName, path))})");
        var content = "const componentLoaders = {\n" + string.Join(",\n", loaders) + "\n};\n" +
                      ReadResource("Jazor.Emit.tooling.hydration.js");
        var destination = Path.Combine(projectRoot, HydrationEntryFileName);
        ProjectFileWriter.Write(destination, content);
        return destination;
    }

    private static string ReadResource(string name)
    {
        using var stream = typeof(ProjectEntryWriter).Assembly.GetManifestResourceStream(name)
            ?? throw new InvalidOperationException($"Jazor project resource '{name}' was not embedded in Jazor.Emit.");
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        return reader.ReadToEnd().ReplaceLineEndings("\n");
    }

    private static string CreateExports(string fileName, IEnumerable<string> roots)
    {
        var imports = roots
            .Where(static path => !string.IsNullOrWhiteSpace(path))
            .Select(ECMAScriptModulePath.NormalizeRelativePath)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static path => path, StringComparer.Ordinal)
            .Select(path => ECMAScriptModulePath.ResolveRelativeToImporter(fileName, path))
            .ToArray();
        if (imports.Length == 0)
            throw new InvalidOperationException($"Jazor project entry '{fileName}' requires at least one application root.");

        var content = string.Concat(imports.Select(static path => $"export * from {Quote(path)};\n"));
        return content;
    }

    private static string Quote(string value)
        => "\"" + value.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal) + "\"";

}
