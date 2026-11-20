using System.Text.Json;
using Jolt.Compiler;

internal static class JoltOutput
{
    public static async Task WriteAsync(string project, string output, IReadOnlyList<JoltModule> modules, CancellationToken token,
        string? routerModule = null)
    {
        var files = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var (path, content) in JoltClrRuntime.ReadModules(modules.SelectMany(module => module.ClrImports)))
            files.Add(path, content);
        foreach (var module in modules)
        {
            if (module.ScopedCss is { } css)
                files.Add(Path.ChangeExtension(module.Path, ".jolt.css"), css);
            // Write the map before its module so Vite sees consistent debugger data.
            files.Add(module.Path + ".map", module.SourceMap);
            var source = Path.GetFullPath(Path.ChangeExtension(module.Path, ".jazor"), project);
            var directory = Path.GetDirectoryName(Path.GetFullPath(module.Path, output))!;
            var relativeSource = Path.GetRelativePath(directory, source).Replace(Path.DirectorySeparatorChar, '/');
            AddDeclaration(files, Path.ChangeExtension(module.Path, ".d.mts"), module, relativeSource);
            AddDeclaration(files, Path.ChangeExtension(module.Path, ".d.jazor.ts"), module, relativeSource);
            files.Add(module.Path, module.Content);
        }

        if (routerModule is not null)
        {
            files.Add(JoltCompileResult.RouterModulePath, routerModule);
            files.Add(Path.ChangeExtension(JoltCompileResult.RouterModulePath, ".d.mts"),
                "import type { RouteRecordRaw } from 'vue-router';\nexport declare const routes: RouteRecordRaw[];\n");
        }
        await WriteFilesAsync(output, "jolt.modules.json", files, null, token);
    }

    internal static void AddDeclaration(Dictionary<string, string> files, string path, JoltModule module, string? source)
    {
        var name = Path.GetFileName(path);
        var map = source is null ? null : module.CreateTypeScriptSourceMap(name, source);
        if (map is not null)
            files.Add(path + ".map", map);
        files.Add(path, map is null ? module.TypeScriptDeclaration :
            module.TypeScriptDeclaration + "\n//# sourceMappingURL=" + name + ".map\n");
    }

    internal static async Task WriteFilesAsync(string output, string manifestName,
        IReadOnlyDictionary<string, string> files, IReadOnlyDictionary<string, byte[]>? assets, CancellationToken token)
    {
        Directory.CreateDirectory(output);
        var manifest = Path.Combine(output, manifestName);
        await using var lease = new FileStream(Path.Combine(output, ".jolt-build.lock"), FileMode.OpenOrCreate,
            FileAccess.ReadWrite, FileShare.None);
        var earlier = File.Exists(manifest)
            ? JsonSerializer.Deserialize<string[]>(await File.ReadAllTextAsync(manifest, token)) ?? []
            : [];
        var owned = new SortedSet<string>(earlier, StringComparer.Ordinal);
        var current = new SortedSet<string>(files.Keys.Concat(assets?.Keys ?? []), StringComparer.Ordinal);
        if (assets is null)
        {
            // Older build manifests only tracked the .mjs, whose map was implicit.
            foreach (var module in earlier.Where(path => path.EndsWith(".mjs", StringComparison.Ordinal)))
                owned.Add(module + ".map");
            foreach (var path in owned)
                if (!path.EndsWith(".mjs", StringComparison.Ordinal) && !path.EndsWith(".mjs.map", StringComparison.Ordinal) &&
                    !path.EndsWith(".jolt.css", StringComparison.Ordinal) && !path.EndsWith(".d.mts", StringComparison.Ordinal) &&
                    !path.EndsWith(".d.mts.map", StringComparison.Ordinal) && !path.EndsWith(".d.jazor.ts", StringComparison.Ordinal) &&
                    !path.EndsWith(".d.jazor.ts.map", StringComparison.Ordinal) &&
                    !(path.StartsWith("clr/", StringComparison.Ordinal) && path.EndsWith(".js", StringComparison.Ordinal)))
                    throw new InvalidDataException($"Invalid module in {manifest}: {path}");
        }
        else
        {
            foreach (var path in current.Except(owned))
                if (File.Exists(Resolve(path)))
                    throw new IOException($"Jolt library output would replace an existing file at '{Resolve(path)}'.");
        }

        // Record ownership before replacing files so a retry can recover a partial write.
        var obsolete = owned.Except(current).ToArray();
        owned.UnionWith(current);
        if (assets is not null)
            await WriteChanged(manifestName, JsonSerializer.Serialize(owned));
        foreach (var (path, content) in files)
            await WriteChanged(path, content);
        if (assets is not null)
            foreach (var (path, content) in assets)
                await WriteBytes(path, content);
        foreach (var path in obsolete)
            File.Delete(Resolve(path));
        await WriteChanged(manifestName, JsonSerializer.Serialize(current));

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
            await ReplaceFile(file, temporary => File.WriteAllTextAsync(temporary, content, token));
        }

        async Task WriteBytes(string relative, byte[] content)
        {
            var file = Resolve(relative);
            if (File.Exists(file) && (await File.ReadAllBytesAsync(file, token)).AsSpan().SequenceEqual(content))
                return;
            await ReplaceFile(file, temporary => File.WriteAllBytesAsync(temporary, content, token));
        }

        async Task ReplaceFile(string file, Func<string, Task> write)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(file)!);
            string temporary = file + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                await write(temporary);
                File.Move(temporary, file, overwrite: true);
            }
            finally { File.Delete(temporary); }
        }
    }
}
