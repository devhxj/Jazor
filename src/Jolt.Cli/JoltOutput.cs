using System.Text.Json;
using Jolt.Compiler;

internal static class JoltOutput
{
    public static async Task WriteAsync(string output, IReadOnlyList<JoltModule> modules, CancellationToken token)
    {
        Directory.CreateDirectory(output);
        var manifest = Path.Combine(output, "jolt.modules.json");
        await using var lease = new FileStream(Path.Combine(output, ".jolt-build.lock"), FileMode.OpenOrCreate,
            FileAccess.ReadWrite, FileShare.None);
        var earlier = File.Exists(manifest)
            ? JsonSerializer.Deserialize<string[]>(await File.ReadAllTextAsync(manifest, token)) ?? []
            : [];
        var current = new SortedSet<string>(StringComparer.Ordinal);
        var runtimeModules = JoltClrRuntime.ReadModules(modules.SelectMany(module => module.ClrImports));
        foreach (var (path, content) in runtimeModules)
        {
            current.Add(path);
            await WriteChanged(path, content);
        }
        foreach (var module in modules)
        {
            current.Add(module.Path);
            if (module.ScopedCss is { } css)
            {
                var cssPath = Path.ChangeExtension(module.Path, ".jolt.css");
                current.Add(cssPath);
                await WriteChanged(cssPath, css);
            }
            // Write the map before its module so Vite sees consistent debugger data.
            await WriteChanged(module.Path + ".map", module.SourceMap);
            await WriteChanged(module.Path, module.Content);
        }
        foreach (var obsolete in earlier.Except(current))
        {
            if (obsolete.EndsWith(".mjs", StringComparison.Ordinal))
            {
                File.Delete(Resolve(obsolete));
                File.Delete(Resolve(obsolete + ".map"));
            }
            else if (obsolete.EndsWith(".jolt.css", StringComparison.Ordinal) ||
                obsolete.StartsWith("clr/", StringComparison.Ordinal) && obsolete.EndsWith(".js", StringComparison.Ordinal))
            {
                File.Delete(Resolve(obsolete));
            }
            else
            {
                throw new InvalidDataException($"Invalid module in {manifest}: {obsolete}");
            }
        }
        await WriteChanged("jolt.modules.json", JsonSerializer.Serialize(current));

        string Resolve(string relative)
        {
            var file = Path.GetFullPath(relative, output);
            var path = Path.GetRelativePath(output, file);
            if (Path.IsPathRooted(path) || path.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal) || path == "..")
                throw new InvalidDataException($"Invalid output path {relative}.");
            return file;
        }

        async Task WriteChanged(string relative, string content)
        {
            var file = Resolve(relative);
            if (File.Exists(file) && await File.ReadAllTextAsync(file, token) == content)
                return;
            Directory.CreateDirectory(Path.GetDirectoryName(file)!);
            string temporary = file + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                await File.WriteAllTextAsync(temporary, content, token);
                File.Move(temporary, file, overwrite: true);
            }
            finally { File.Delete(temporary); }
        }
    }
}
