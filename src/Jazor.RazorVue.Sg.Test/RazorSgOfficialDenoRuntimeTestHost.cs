using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using Acornima;
using Acornima.Ast;

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
    private static readonly IReadOnlyDictionary<string, string> EcmascriptResourceModules = ReadEcmascriptResourceModules();
    private static readonly IReadOnlyDictionary<string, string> VueRuntimeResourceModules = ReadVueRuntimeResourceModules();

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
            MaterializeCatalogDependencies(root, moduleRelativePath, moduleText, supportingModules);
            MaterializeRazorVueRuntimeModules(root, moduleText, supportingModules);
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
                const serverPrefetch = [];
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

                export function onServerPrefetch(callback) {
                    serverPrefetch.push(callback);
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

                export async function __runServerPrefetch() {
                    for (const callback of serverPrefetch) {
                        await callback();
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
            // carrier 物理布局由 ManifestModel 的 development 路径决定（含 clr/ 前缀）；
            // 这里同时暴露逻辑前缀与项目前缀，覆盖两种引用形态。
            ["System/"] = "./clr/System/",
            ["Microsoft/"] = "./clr/Microsoft/",
            ["clr/"] = "./clr/",
            ["@jazor/vue-runtime/"] = "./@jazor/vue-runtime/",
            ["vue"] = "./node_modules/vue/index.mjs",
        };

        if (supportingModules is not null)
        {
            foreach (var supportingModule in supportingModules)
            {
                // fixture 里的每个模块文件都按其裸 specifier 暴露：上游包（如
                // tdesign-vue-next）不发布 exports 字段，深层 specifier 直接按文件布局解析，
                // 因此映射必须逐文件建立，不能只映射包根。
                if (!IsJavaScriptModulePath(supportingModule.Key))
                    continue;

                var normalized = supportingModule.Key.Replace('\\', '/');
                if (!normalized.StartsWith(NodeModulesPrefix, StringComparison.Ordinal))
                    continue;

                var specifier = normalized.Substring(NodeModulesPrefix.Length);
                imports[specifier] = "./" + normalized;
            }
        }

        return JsonSerializer.Serialize(new { imports });
    }

    private const string NodeModulesPrefix = "node_modules/";

    private static void MaterializeCatalogDependencies(
        string root,
        string modulePath,
        string moduleText,
        IReadOnlyDictionary<string, string>? supportingModules)
    {
        // Collect compiler-owned carrier imports from the JavaScript module AST. 引用可能是
        // 项目相对路径（载体之间）或 clr/、System/、Microsoft/ 前缀形态；三者都要归一到
        // 清单键，并按各自的声明路径落盘，保持相对 import 可解析。
        var pendingPaths = new Queue<(string ManifestKey, string ProjectPath)>(
            GetCarrierManifestKeys(moduleText, modulePath).Select(key => (key, key)));
        if (supportingModules is not null)
        {
            foreach (var supportingModule in supportingModules)
            {
                if (!IsJavaScriptModulePath(supportingModule.Key))
                    continue;

                foreach (var path in GetCarrierManifestKeys(supportingModule.Value, supportingModule.Key))
                    pendingPaths.Enqueue((path, path));
            }
        }

        var materializedPaths = new HashSet<string>(StringComparer.Ordinal);
        while (pendingPaths.TryDequeue(out var pending))
        {
            if (!materializedPaths.Add(pending.ManifestKey))
                continue;

            if (!EcmascriptResourceModules.TryGetValue(pending.ManifestKey, out var content))
            {
                throw new InvalidOperationException(
                    $"ECMAScript resource package does not contain imported module '{pending.ManifestKey}'.");
            }

            WriteFile(Path.Combine(root, pending.ManifestKey.Replace('/', Path.DirectorySeparatorChar)), content);
            // 载体内部的 import 也是相对的，解析基点必须用该载体自己的项目路径，
            // 不能按文件名在清单里猜测。
            foreach (var dependency in GetCarrierManifestKeys(content, pending.ManifestKey))
                pendingPaths.Enqueue((dependency, dependency));
        }
    }

    /// <summary>
    /// 把 carrier 引用归一为清单逻辑键。
    ///
    /// 载体文件的清单键不含 clr/ 前缀（那是项目源码树前缀），因此相对 specifier
    /// 需要先解析成项目路径、再与清单里的 development 路径比对。
    /// </summary>
    private static IEnumerable<string> GetCarrierManifestKeys(string moduleText, string? importerPath)
    {
        foreach (var specifier in GetCatalogImportPaths(moduleText, importerPath))
        {
            if (TryResolveCarrierKey(specifier.Replace('\\', '/'), importerPath, out var manifestKey))
            {
                yield return manifestKey;
                continue;
            }

            throw new InvalidOperationException(
                $"ECMAScript resource package does not contain imported module '{specifier}'.");
        }
    }

    /// <summary>
    /// Writes Jazor.Vue-owned ESM helpers needed by the generated artifact under the same prefix
    /// Emit materializes in production. The test host reads the JS-resource package itself rather
    /// than restoring the retired RazorVue embedded-resource carrier.
    /// </summary>
    private static void MaterializeRazorVueRuntimeModules(
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
            if (!VueRuntimeResourceModules.TryGetValue(importPath, out var content))
            {
                throw new InvalidOperationException(
                    $"Jazor.Vue JS-resource package does not contain runtime import '{importPath}'.");
            }

            WriteFile(
                Path.Combine(root, importPath.Replace('/', Path.DirectorySeparatorChar)),
                content);
            // Runtime helpers can import CLR-owned modules directly (for example the
            // NavigationManager adapter). Materialize those catalog dependencies from the
            // helper content as well as from generated component modules.
            MaterializeCatalogDependencies(root, importPath, content, supportingModules);
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

    private static bool IsJavaScriptModulePath(string path)
        => path.EndsWith(".js", StringComparison.OrdinalIgnoreCase) ||
           path.EndsWith(".mjs", StringComparison.OrdinalIgnoreCase);

    private static IEnumerable<string> GetCatalogImportPaths(string moduleText, string? importerPath)
        => new Parser().ParseModule(moduleText).Body
            .OfType<ImportDeclaration>()
            .Select(static import => import.Source.Value)
            .Where(path => IsCarrierModuleSpecifier(path, importerPath));

    /// <summary>
    /// 判断一个 specifier 是否指向 carrier 模块。
    ///
    /// 判据是"解析后的目标是否存在于 carrier 清单"：载体之间用相对 specifier 互引，
    /// 组件模块用 clr/** 前缀引用载体，测试 fixture 也会用相对 specifier 引用自己的 helper。
    /// 只有清单成员才算载体，因此必须先解析再判定，不能按前缀猜测。
    /// </summary>
    private static bool IsCarrierModuleSpecifier(string path, string? importerPath)
        => TryResolveCarrierKey(path.Replace('\\', '/'), importerPath, out _);

    /// <summary>
    /// 把 carrier 引用解析为清单键。
    ///
    /// carrier 的清单键就是项目相对路径（含 clr/ 前缀），因此：
    /// - clr/** 形态可直接命中；
    /// - 相对 specifier 先按 importer 的最终位置解析成项目路径，再命中；
    /// - 解析结果不在清单里的引用属于 fixture 自己的模块，不是 carrier。
    /// </summary>
    private static bool TryResolveCarrierKey(string specifier, string? importerPath, out string manifestKey)
    {
        manifestKey = string.Empty;
        if (EcmascriptResourceModules.ContainsKey(specifier))
        {
            manifestKey = specifier;
            return true;
        }

        // 非相对形态：Jazor.Vue 的 dist 载体仍按旧的无前缀写法引用 CLR 载体
        // （Microsoft/**、System/**），import map 也按同样规则映射到 clr/。
        if (!specifier.StartsWith("./", StringComparison.Ordinal) &&
            !specifier.StartsWith("../", StringComparison.Ordinal))
        {
            var prefixed = "clr/" + specifier;
            if (EcmascriptResourceModules.ContainsKey(prefixed))
            {
                manifestKey = prefixed;
                return true;
            }

            return false;
        }

        if (importerPath is null)
            return false;

        // 相对 specifier 只有落到清单声明的路径上才算 carrier。
        var resolved = ResolveVirtualPath(importerPath.Replace('\\', '/'), specifier);
        if (resolved is not null && EcmascriptResourceModules.ContainsKey(resolved))
        {
            manifestKey = resolved;
            return true;
        }

        return false;
    }

    /// <summary>
    /// 按 POSIX 语义把相对 specifier 解析为项目路径，越出项目根的引用返回 null。
    /// </summary>
    private static string? ResolveVirtualPath(string importerPath, string specifier)
    {
        var separator = importerPath.LastIndexOf('/');
        var segments = new List<string>(
            separator < 0 ? [] : importerPath[..separator].Split('/'));
        foreach (var segment in specifier.Split('/'))
        {
            switch (segment)
            {
                case "" or ".":
                    continue;
                case "..":
                    if (segments.Count == 0)
                        return null;
                    segments.RemoveAt(segments.Count - 1);
                    break;
                default:
                    segments.Add(segment);
                    break;
            }
        }

        return string.Join('/', segments);
    }

    private static IReadOnlyDictionary<string, string> ReadEcmascriptResourceModules()
    {
        var repositoryRoot = FindRepositoryRoot();
        var packageRoot = Path.Combine(repositoryRoot, "src", "ECMAScript");
        var manifestPath = Path.Combine(packageRoot, "manifest.json");
        if (!File.Exists(manifestPath))
            throw new FileNotFoundException("The ECMAScript JS-resource manifest was not found.", manifestPath);

        using var document = JsonDocument.Parse(File.ReadAllText(manifestPath));
        var resourceModules = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var entry in document.RootElement.GetProperty("imports").EnumerateObject())
        {
            // 清单键就是 carrier 的项目相对路径（含 clr/ 前缀），与开发/生产字段一致。
            var production = entry.Value.GetProperty("production").GetString();
            if (string.IsNullOrWhiteSpace(production) ||
                !production.StartsWith("clr/", StringComparison.Ordinal))
                continue;

            var sourcePath = Path.Combine(packageRoot, production.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(sourcePath))
                throw new FileNotFoundException($"ECMAScript resource module '{production}' was not found.", sourcePath);
            var content = File.ReadAllText(sourcePath);

            if (!resourceModules.TryAdd(production, content))
                throw new InvalidOperationException($"ECMAScript resource package contains duplicate path '{production}'.");
        }

        return resourceModules;
    }

    private static IReadOnlyDictionary<string, string> ReadVueRuntimeResourceModules()
    {
        var repositoryRoot = FindRepositoryRoot();
        var packageRoot = Path.Combine(repositoryRoot, "src", "Jazor.Vue");
        var manifestPath = Path.Combine(packageRoot, "manifest.json");
        if (!File.Exists(manifestPath))
            throw new FileNotFoundException("The Jazor.Vue JS-resource manifest was not found.", manifestPath);

        using var document = JsonDocument.Parse(File.ReadAllText(manifestPath));
        var root = document.RootElement;
        if (root.GetProperty("schemaVersion").GetInt32() != 2 ||
            !string.Equals(root.GetProperty("libraryId").GetString(), "jazor-vue-runtime", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Jazor.Vue runtime manifest '{manifestPath}' has an unexpected identity.");
        }

        var resourceModules = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var entry in root.GetProperty("imports").EnumerateObject())
        {
            if (!entry.Name.StartsWith("@jazor/vue-runtime/", StringComparison.Ordinal))
                continue;

            var value = entry.Value;
            if (!string.Equals(value.GetProperty("type").GetString(), "module", StringComparison.Ordinal))
                throw new InvalidOperationException($"Jazor.Vue runtime import '{entry.Name}' is not a module.");

            var production = value.GetProperty("production").GetString();
            var hash = value.GetProperty("productionHash").GetString();
            if (string.IsNullOrWhiteSpace(production) ||
                !production.StartsWith("dist/", StringComparison.Ordinal) ||
                string.IsNullOrWhiteSpace(hash))
            {
                throw new InvalidOperationException($"Jazor.Vue runtime import '{entry.Name}' has an invalid production entry.");
            }

            var sourcePath = Path.Combine(packageRoot, production.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(sourcePath))
                throw new FileNotFoundException($"Jazor.Vue runtime module '{production}' was not found.", sourcePath);

            var actualHash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(sourcePath))).ToLowerInvariant();
            if (!string.Equals(actualHash, hash, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Jazor.Vue runtime module '{production}' hash does not match its manifest.");
            }

            if (!resourceModules.TryAdd(entry.Name, File.ReadAllText(sourcePath)))
                throw new InvalidOperationException($"Jazor.Vue runtime manifest contains duplicate import '{entry.Name}'.");
        }

        return resourceModules;
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Jazor.slnx")))
                return directory.FullName;
        }

        throw new DirectoryNotFoundException("Could not locate the Jazor repository root.");
    }

    private static async Task RunDenoTestAsync(string testFile, string workingDirectory)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = RazorSgDenoRuntime.ResolveExecutable(),
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

}
