using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using Acornima;
using Acornima.Ast;
using ECMAScriptGlobal = ECMAScript.Global;

namespace Jazor.RazorVue.Sg.Test;

/// <summary>
/// Runs a generated official RazorVue artifact with the DenoHost runtime packaged by Jazor.
/// </summary>
/// <remarks>
/// Official Razor authoring tests normally assert the generated module text. This host is
/// intentionally narrow: use it only when the observed behavior depends on JS evaluation,
/// such as an event callback seeing state assigned by a binding handler.
/// </remarks>
internal static class RazorSgOfficialDenoRuntimeTestHost
{
    private static readonly IReadOnlyDictionary<string, string> CatalogModules = ReadCatalogModules();

    public static async Task RunModuleTestAsync(
        string moduleRelativePath,
        string moduleText,
        string testFileName,
        string testSource,
        IReadOnlyDictionary<string, string>? supportingModules = null,
        string? vueRuntimeSource = null)
    {
        var root = RazorSgTestHost.CreateTestArtifactDirectory("deno-runtime");
        try
        {
            WriteFile(Path.Combine(root, moduleRelativePath), moduleText);
            if (supportingModules is not null)
            {
                foreach (var module in supportingModules)
                    WriteFile(Path.Combine(root, module.Key), module.Value);
            }
            MaterializeCatalogDependencies(root, moduleText, supportingModules);
            MaterializeRazorVueRuntimeDependencies(root, moduleText, supportingModules);
            WriteFile(
                Path.Combine(root, "package.json"),
                """{"type":"module"}""");
            WriteFile(
                Path.Combine(root, "deno.json"),
                BuildDenoImportMap(supportingModules));
            WriteFile(
                Path.Combine(root, "node_modules", "vue", "package.json"),
                """{"type":"module","exports":"./index.mjs"}""");
            WriteFile(
                Path.Combine(root, "node_modules", "vue", "index.mjs"),
                vueRuntimeSource ?? """
                export function defineComponent(options) {
                    return options;
                }

                export const Fragment = Symbol("Fragment");

                export function reactive(value) {
                    return value;
                }

                const provides = new Map();

                export function provide(key, value) {
                    provides.set(key, value);
                }

                export function inject(key, fallback) {
                    return provides.has(key) ? provides.get(key) : fallback;
                }

                export function ref(value) {
                    return { value };
                }

                export function onErrorCaptured(callback) {
                    // The lightweight host does not run a component scheduler; retaining the
                    // callback keeps adapter setup/import behavior executable in Deno fixtures.
                    return () => {};
                }

                const mounted = [];
                const updated = [];
                const unmounted = [];
                const watchers = [];

                export function watch(source, callback) {
                    watchers.push({ source, callback, value: source() });
                    return () => {};
                }

                export function onMounted(callback) {
                    mounted.push(callback);
                }

                export function onUpdated(callback) {
                    updated.push(callback);
                }

                export function onUnmounted(callback) {
                    unmounted.push(callback);
                }

                export function __runMounted() {
                    for (const callback of mounted) {
                        callback();
                    }
                }

                export function __runUpdated() {
                    for (const callback of updated) {
                        callback();
                    }
                }

                export function __runUnmounted() {
                    for (const callback of unmounted) {
                        callback();
                    }
                }

                export function __runWatchers() {
                    for (const watcher of watchers) {
                        const next = watcher.source();
                        const changed = Array.isArray(next) && Array.isArray(watcher.value)
                            ? next.length !== watcher.value.length || next.some((value, index) => !Object.is(value, watcher.value[index]))
                            : !Object.is(next, watcher.value);
                        if (changed) {
                            const previous = watcher.value;
                            watcher.value = next;
                            watcher.callback(next, previous);
                        }
                    }
                }

                export function createStaticVNode(html, count) {
                    return { name: "__static", props: { html, count }, children: html };
                }

                export function createCommentVNode(text) {
                    return { name: "__comment", children: text };
                }

                export function openBlock() {
                    return null;
                }

                export function createElementBlock(name, props, children, patchFlag, dynamicProps) {
                    return { name, props, children, patchFlag, dynamicProps, block: "element" };
                }

                export function createBlock(name, props, children, patchFlag, dynamicProps) {
                    return { name, props, children, patchFlag, dynamicProps, block: "component" };
                }

                // Preserve the slot function identity for assertions while matching Vue's
                // createSlots merge contract. Dynamic entries can be absent on a render pass.
                // 测试 stub 不模拟实例上下文，只保留 slot descriptor 的合并语义。
                export function withCtx(slot) {
                    return slot;
                }

                export function createSlots(slots, dynamicSlots) {
                    for (const slot of dynamicSlots) {
                        if (Array.isArray(slot)) {
                            for (const entry of slot) {
                                if (entry) slots[entry.name] = entry.fn;
                            }
                        } else if (slot) {
                            slots[slot.name] = slot.fn;
                        }
                    }
                    return slots;
                }

                // Match the Vue helper shape closely enough for generated artifact assertions.
                // block collection only relies on the returned VNode identity and patch flag.
                // 测试 stub 保留 text vnode 的 patchFlag，验证 E1 不会遗漏动态文本协议。
                export function createTextVNode(children, patchFlag) {
                    return { name: "__text", children, patchFlag };
                }

                // Keep the test contract aligned with Vue's array/iterable/object list domains.
                // 这里不模拟 diff，只验证 mapper、fragment flag 与 iteration identity。
                export function renderList(source, renderItem) {
                    if (Array.isArray(source) || typeof source === "string") {
                        return Array.from(source, (item, index) => renderItem(item, index));
                    }
                    if (typeof source === "number") {
                        return Array.from({ length: source }, (_, index) => renderItem(index + 1, index));
                    }
                    if (source?.[Symbol.iterator]) {
                        return Array.from(source, (item, index) => renderItem(item, index));
                    }
                    if (source && typeof source === "object") {
                        return Object.keys(source).map((key, index) => renderItem(source[key], key, index));
                    }
                    return [];
                }

                export function mergeProps(...sources) {
                    return Object.assign({}, ...sources.filter(source => source != null));
                }

                export function h(name, props, children) {
                    return { name, props, children };
                }
                """);

            var testFile = Path.Combine(root, testFileName);
            WriteFile(testFile, testSource);
            await RunDenoTestAsync(testFile, root);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    private static void WriteFile(string path, string content)
    {
        var directory = Path.GetDirectoryName(path)
            ?? throw new InvalidOperationException($"Could not resolve parent directory for '{path}'.");
        Directory.CreateDirectory(directory);
        File.WriteAllText(path, content);
    }

    private static string BuildDenoImportMap(IReadOnlyDictionary<string, string>? supportingModules)
    {
        var imports = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["System/"] = "./System/",
            ["Microsoft/"] = "./Microsoft/",
            ["@jazor/vue-runtime/"] = "./@jazor/vue-runtime/",
            ["vue"] = "./node_modules/vue/index.mjs",
        };

        if (supportingModules is not null)
        {
            foreach (var supportingModule in supportingModules)
            {
                var packageName = GetPackageName(supportingModule.Key);
                if (packageName is null)
                {
                    continue;
                }

                using var manifest = JsonDocument.Parse(supportingModule.Value);
                var exportPath = manifest.RootElement.GetProperty("exports").GetString()
                    ?? throw new InvalidOperationException($"Package fixture '{packageName}' must expose a string exports path.");
                if (!exportPath.StartsWith("./", StringComparison.Ordinal))
                {
                    throw new InvalidOperationException($"Package fixture '{packageName}' must expose a relative exports path.");
                }

                imports.Add(packageName, $"./node_modules/{packageName}/{exportPath[2..]}");
            }
        }

        return JsonSerializer.Serialize(new { imports });
    }

    private static string? GetPackageName(string supportingModulePath)
    {
        var normalizedPath = supportingModulePath.Replace('\\', '/');
        const string prefix = "node_modules/";
        const string suffix = "/package.json";
        if (!normalizedPath.StartsWith(prefix, StringComparison.Ordinal)
            || !normalizedPath.EndsWith(suffix, StringComparison.Ordinal))
        {
            return null;
        }

        return normalizedPath[prefix.Length..^suffix.Length];
    }

    private static void MaterializeCatalogDependencies(
        string root,
        string moduleText,
        IReadOnlyDictionary<string, string>? supportingModules)
    {
        // Collect compiler-owned CLR imports from JavaScript module AST; package.json等
        // supporting files are not JavaScript inputs.
        var pendingPaths = new Queue<string>(GetCatalogImportPaths(moduleText));
        if (supportingModules is not null)
        {
            foreach (var supportingModule in supportingModules)
            {
                if (!IsJavaScriptModulePath(supportingModule.Key))
                    continue;

                foreach (var path in GetCatalogImportPaths(supportingModule.Value))
                    pendingPaths.Enqueue(path);
            }
        }

        var materializedPaths = new HashSet<string>(StringComparer.Ordinal);
        while (pendingPaths.TryDequeue(out var relativePath))
        {
            if (!materializedPaths.Add(relativePath))
                continue;

            if (!CatalogModules.TryGetValue(relativePath, out var content))
            {
                throw new InvalidOperationException(
                    $"CLR runtime catalog does not contain imported module '{relativePath}'.");
            }

            WriteFile(Path.Combine(root, relativePath), content);
            foreach (var dependency in GetCatalogImportPaths(content))
                pendingPaths.Enqueue(dependency);
        }
    }

    /// <summary>
    /// Writes RazorVue-owned ESM helpers needed by the generated artifact under the same prefix
    /// Emit materializes in production. 测试 host 不模拟 helper 文本，直接读取 embedded resource。
    /// </summary>
    private static void MaterializeRazorVueRuntimeDependencies(
        string root,
        string moduleText,
        IReadOnlyDictionary<string, string>? supportingModules)
    {
        var imports = GetJavaScriptModules(moduleText, supportingModules)
            .SelectMany(static text => new Parser().ParseModule(text).Body.OfType<ImportDeclaration>())
            .Select(static declaration => declaration.Source.Value)
            .Where(static path => path.StartsWith("@jazor/vue-runtime/", StringComparison.Ordinal))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static path => path, StringComparer.Ordinal);

        foreach (var importPath in imports)
        {
            var resourceName = importPath switch
            {
                "@jazor/vue-runtime/raw-markup.mjs" => "Jazor.RazorVue.Runtime.raw-markup.mjs",
                "@jazor/vue-runtime/cascading.mjs" => "Jazor.RazorVue.Runtime.cascading.mjs",
                "@jazor/vue-runtime/blazor-routing.mjs" => "Jazor.RazorVue.Runtime.blazor-routing.mjs",
                "@jazor/vue-runtime/blazor-components.mjs" => "Jazor.RazorVue.Runtime.blazor-components.mjs",
                _ => throw new InvalidOperationException(
                    $"Official RazorVue Deno test host does not know runtime import '{importPath}'.")
            };

            var content = ReadRazorVueRuntimeResource(resourceName);
            WriteFile(
                Path.Combine(root, importPath.Replace('/', Path.DirectorySeparatorChar)),
                content);
            // Runtime helpers can import CLR-owned modules directly (for example the
            // NavigationManager adapter). Materialize those catalog dependencies from the
            // helper content as well as from generated component modules.
            MaterializeCatalogDependencies(root, content, supportingModules);
        }
    }

    private static IEnumerable<string> GetJavaScriptModules(
        string moduleText,
        IReadOnlyDictionary<string, string>? supportingModules)
    {
        yield return moduleText;
        if (supportingModules is null)
            yield break;

        foreach (var module in supportingModules)
        {
            if (IsJavaScriptModulePath(module.Key))
                yield return module.Value;
        }
    }

    private static string ReadRazorVueRuntimeResource(string resourceName)
    {
        using var stream = typeof(Jazor.RazorVue.RazorSdk.RenderEmitter).Assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"RazorVue runtime resource '{resourceName}' was not found.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }


    private static bool IsJavaScriptModulePath(string path)
        => path.EndsWith(".js", StringComparison.OrdinalIgnoreCase) ||
           path.EndsWith(".mjs", StringComparison.OrdinalIgnoreCase);

    private static IEnumerable<string> GetCatalogImportPaths(string moduleText)
        => new Parser().ParseModule(moduleText).Body
            .OfType<ImportDeclaration>()
            .Select(static import => import.Source.Value)
            .Where(static path =>
                path.StartsWith("System/", StringComparison.Ordinal) ||
                path.StartsWith("Microsoft/", StringComparison.Ordinal));

    private static IReadOnlyDictionary<string, string> ReadCatalogModules()
    {
        var catalogType = typeof(ECMAScriptGlobal).Assembly.GetType("ECMAScript.Catalog", throwOnError: true)!;
        var getModules = catalogType.GetMethod(
            "GetModules",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("ECMAScript.Catalog.GetModules() was not found.");
        var modules = getModules.Invoke(null, null) as IEnumerable
            ?? throw new InvalidOperationException("ECMAScript.Catalog.GetModules() returned no module collection.");

        var catalogModules = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var module in modules)
        {
            if (module is null)
                continue;

            var moduleType = module.GetType();
            var relativePath = ReadCatalogProperty(moduleType, module, "RelativePath");
            var content = ReadCatalogProperty(moduleType, module, "Content");
            if (!relativePath.StartsWith("System/", StringComparison.Ordinal) &&
                !relativePath.StartsWith("Microsoft/", StringComparison.Ordinal))
                continue;

            if (!catalogModules.TryAdd(relativePath, content))
                throw new InvalidOperationException($"CLR runtime catalog contains duplicate path '{relativePath}'.");
        }

        return catalogModules;
    }

    private static string ReadCatalogProperty(Type moduleType, object module, string propertyName)
        => moduleType.GetProperty(
            propertyName,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(module) as string
            ?? throw new InvalidOperationException(
                $"Catalog module property '{propertyName}' was not found on '{moduleType.FullName}'.");

    private static async Task RunDenoTestAsync(string testFile, string workingDirectory)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = ResolveBundledDenoExecutable(),
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            WorkingDirectory = workingDirectory
        };
        startInfo.ArgumentList.Add("test");
        startInfo.ArgumentList.Add("--quiet");
        startInfo.ArgumentList.Add("--allow-all");
        startInfo.ArgumentList.Add("--config");
        startInfo.ArgumentList.Add(Path.Combine(workingDirectory, "deno.json"));
        startInfo.ArgumentList.Add(testFile);
        startInfo.Environment["DENO_DIR"] = Path.Combine(workingDirectory, ".deno-cache");

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start the bundled DenoHost runtime.");
        var standardOutput = process.StandardOutput.ReadToEndAsync();
        var standardError = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode == 0)
            return;

        Assert.Fail(
            "Bundled DenoHost runtime test failed." + Environment.NewLine +
            await standardOutput + Environment.NewLine +
            await standardError);
    }

    private static string ResolveBundledDenoExecutable()
    {
        var root = FindRepositoryRoot();
        var executableName = OperatingSystem.IsWindows() ? "deno.exe" : "deno";
        var emitBuildRoot = Path.Combine(root, "src", "Jazor.Emit", "bin");
        if (!Directory.Exists(emitBuildRoot))
        {
            throw new FileNotFoundException(
                "Jazor.Emit build output is required for official RazorVue DenoHost tests. Build src/Jazor.Emit first.",
                emitBuildRoot);
        }

        var candidate = Directory.EnumerateFiles(emitBuildRoot, executableName, SearchOption.AllDirectories)
            .Where(path => path.Contains(
                Path.DirectorySeparatorChar + "runtimes" + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(path => path.Contains(
                Path.DirectorySeparatorChar + "net11.0" + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase))
            .ThenByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault();

        return candidate ?? throw new FileNotFoundException(
            "Bundled DenoHost runtime was not found. Build src/Jazor.Emit so its runtime assets are restored.",
            emitBuildRoot);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Jazor.slnx")))
                return directory.FullName;

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the Jazor repository root from the test output directory.");
    }
}
