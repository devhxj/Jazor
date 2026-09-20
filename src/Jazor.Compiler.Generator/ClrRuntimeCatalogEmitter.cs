using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Jazor.Common;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Lowers Jazor.CLR runtime modules and publishes them as the ECMAScript JS-resource package.
/// </summary>
/// <remarks>
/// Roslyn source discovery and <see cref="AstConverter"/> remain the source of runtime semantics.
/// This emitter owns the embedded ECMAScript package carrier declared by the package manifest.
/// It must never generate a second managed catalog carrier for the ECMAScript project.
/// </remarks>
internal static class ClrRuntimeCatalogEmitter
{
    private const int ManifestSchemaVersion = 2;
    private const string LibraryId = "ecmascript";
    private static readonly UTF8Encoding Utf8WithoutBom = new(encoderShouldEmitUTF8Identifier: false);

    public static void Generate(
        string repoRoot,
        IEnumerable<MetadataReference> references,
        string? requestedPackageVersion = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repoRoot);
        ArgumentNullException.ThrowIfNull(references);

        var clrSourceFiles = SharedGeneration.GetClrCompilationSourceFiles(repoRoot)
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToArray();
        if (clrSourceFiles.Length == 0)
            return;

        var syntaxTrees = clrSourceFiles
            .Select(SharedGeneration.CreateSyntaxTree)
            .ToArray();
        var compilation = CSharpCompilation.Create(
            "Jazor.Compiler.Generator",
            syntaxTrees,
            [.. references],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var candidates = DiscoverClrRuntimeModules(compilation, syntaxTrees);
        Console.WriteLine($"clr-runtime candidates={candidates.Count}");
        if (candidates.Count == 0)
            return;

        var generated = new List<GeneratedClrRuntimeModule>(candidates.Count);
        var failures = new List<string>();
        foreach (var candidate in candidates.OrderBy(static x => x.RelativePath, StringComparer.OrdinalIgnoreCase))
        {
            try
            {
                var options = new AstConverterOptions(
                    AstConverterProfile.ClrRuntime,
                    symbol => ClrRuntimeSelection.ShouldInclude(candidate.RootType, symbol),
                    CurrentModuleOutputPath: NormalizeRelativePath(candidate.RelativePath),
                    ModuleCatalogOutputPrefix: string.Empty);
                var module = new AstConverter(candidate.RootType, candidate.SemanticModel, options)
                    .Convert()
                    .GetAwaiter()
                    .GetResult();
                if (module is null)
                    throw new InvalidOperationException("AstConverter returned no module.");

                var content = module.ToKnRECMAScript().ReplaceLineEndings("\n");
                var imports = module.Body
                    .OfType<Acornima.Ast.ImportDeclaration>()
                    .Select(static declaration => declaration.Source.Value)
                    .Where(static source => !string.IsNullOrWhiteSpace(source))
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(static source => source, StringComparer.Ordinal)
                    .ToArray();
                generated.Add(new GeneratedClrRuntimeModule(
                    candidate.RootType.ToDisplayString(Format.NameFormat),
                    NormalizeRelativePath(candidate.RelativePath),
                    content,
                    ComputeSha256Hex(content),
                    imports));
            }
            catch (Exception ex)
            {
                var failure = $"{candidate.RootType.ToDisplayString(Format.NameFormat)} -> {candidate.RelativePath} :: {ex.GetType().Name}: {ex.Message}";
                failures.Add(failure);
                Console.WriteLine($"clr-runtime emit fail: {failure}");
            }
        }

        // A partial runtime graph is unusable. Fail before touching dist or manifest so a prior
        // complete package remains available to the next consumer.
        if (failures.Count > 0)
            throw new InvalidOperationException(
                "CLR runtime resource generation failed; no partial ECMAScript package was written.\n" +
                string.Join("\n", failures.OrderBy(static value => value, StringComparer.Ordinal)));
        if (generated.Count == 0)
            throw new InvalidOperationException("CLR runtime discovery produced no modules.");

        var modulePaths = generated
            .Select(static module => module.RelativePath)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        generated = generated
            .Select(module =>
            {
                // 载体内部的 import 已写成相对 specifier（项目内模块必须如此），因此这里先把
                // 每个 specifier 解析回逻辑路径，再按 modulePaths 分类。否则所有相对 specifier
                // 都会被当成"非本图导入"，模块依赖会被误记为缺失。
                var resolvedImports = module.Imports
                    .Select(import => ResolveLogicalImport(module.RelativePath, import, modulePaths))
                    .Where(static value => value is not null)
                    .Select(static value => value!)
                    .ToArray();
                return module with
                {
                    // 依赖边保持**逻辑路径**：它是 carrier 内部的稳定身份，
                    // LibraryMaterializer 按 manifest.imports 的逻辑键遍历闭包。
                    // 写出的 specifier 才是相对的（见 AstConverter），两者刻意不同。
                    ModuleDependencies = resolvedImports
                        .Where(modulePaths.Contains)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .OrderBy(static value => value, StringComparer.OrdinalIgnoreCase)
                        .ToArray(),
                    PackageDependencies = resolvedImports
                        .Where(import => !modulePaths.Contains(import))
                        .Where(ECMAScriptModulePath.IsPackageSpecifier)
                        .Select(static import => import.Trim())
                        .Distinct(StringComparer.Ordinal)
                        .OrderBy(static value => value, StringComparer.Ordinal)
                        .ToArray()
                };
            })
            .OrderBy(static module => module.RelativePath, StringComparer.OrdinalIgnoreCase)
            .ThenBy(static module => module.TypeName, StringComparer.Ordinal)
            .ToList();

        var packageRoot = Path.Combine(repoRoot, "src", "ECMAScript");
        var manifestPath = Path.Combine(packageRoot, "manifest.json");
        var manifest = BuildManifest(ResolveVersion(repoRoot, requestedPackageVersion), generated);
        CommitPackage(packageRoot, manifestPath, generated, manifest);
        Console.WriteLine($"clr-runtime generated resources={generated.Count} manifest={manifestPath}");
    }

    private static List<ClrRuntimeModuleCandidate> DiscoverClrRuntimeModules(
        Compilation compilation,
        IEnumerable<SyntaxTree> syntaxTrees)
    {
        var results = new List<ClrRuntimeModuleCandidate>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var syntaxTree in syntaxTrees)
        {
            var semanticModel = compilation.GetSemanticModel(syntaxTree, ignoreAccessibility: true);
            var root = syntaxTree.GetRoot();
            foreach (var declaration in root.DescendantNodes().OfType<TypeDeclarationSyntax>())
            {
                if (semanticModel.GetDeclaredSymbol(declaration) is not INamedTypeSymbol typeSymbol ||
                    !IsClrRuntimeModuleRoot(typeSymbol, declaration, semanticModel))
                    continue;

                var relativePath = SharedGeneration.ReadModulePath(declaration.AttributeLists, semanticModel);
                if (string.IsNullOrWhiteSpace(relativePath))
                    continue;
                var key = typeSymbol.ToDisplayString(Format.NameFormat);
                if (seen.Add(key))
                    results.Add(new ClrRuntimeModuleCandidate(typeSymbol, semanticModel, relativePath!));
            }
        }

        return results;
    }

    private static bool IsClrRuntimeModuleRoot(
        INamedTypeSymbol typeSymbol,
        TypeDeclarationSyntax declaration,
        SemanticModel semanticModel)
    {
        if (!typeSymbol.IsStatic || typeSymbol.ContainingType is not null ||
            !string.Equals(typeSymbol.ContainingNamespace?.ToDisplayString(), "Jazor.CLR", StringComparison.Ordinal))
            return false;

        var modulePath = SharedGeneration.ReadModulePath(declaration.AttributeLists, semanticModel);
        var jazorAttribute = SharedGeneration.FindAttribute(declaration.AttributeLists, "Jazor");
        var isRuntimeCarrier = string.Equals(typeSymbol.Name, "RuntimeModule", StringComparison.Ordinal);
        var isInternalHelper = jazorAttribute is null && !string.IsNullOrWhiteSpace(modulePath);
        return !string.IsNullOrWhiteSpace(modulePath) &&
               (isRuntimeCarrier || isInternalHelper || jazorAttribute is not null) &&
               ClrRuntimeSelection.HasRuntimeContent(declaration);
    }

    private static object BuildManifest(
        string version,
        IReadOnlyList<GeneratedClrRuntimeModule> modules)
    {
        var imports = new SortedDictionary<string, object>(StringComparer.Ordinal);
        foreach (var module in modules)
        {
            var path = module.RelativePath;
            imports[module.RelativePath] = new
            {
                type = "module",
                development = path,
                production = path,
                developmentHash = module.Hash,
                productionHash = module.Hash,
                developmentDependencies = module.PackageDependencies,
                productionDependencies = module.PackageDependencies,
                developmentModuleDependencies = module.ModuleDependencies,
                productionModuleDependencies = module.ModuleDependencies,
                files = Array.Empty<object>()
            };
        }

        return new
        {
            schemaVersion = ManifestSchemaVersion,
            libraryId = LibraryId,
            version,
            source = "embedded-mjs",
            imports,
            requires = new SortedDictionary<string, string>(StringComparer.Ordinal),
            styles = Array.Empty<object>(),
            files = Array.Empty<object>()
        };
    }

    private static void CommitPackage(
        string packageRoot,
        string manifestPath,
        IReadOnlyList<GeneratedClrRuntimeModule> modules,
        object manifest)
    {
        var parent = Directory.GetParent(packageRoot)?.FullName
            ?? throw new InvalidOperationException($"Could not determine package parent for '{packageRoot}'.");
        Directory.CreateDirectory(parent);
        var staging = Path.Combine(parent, ".ecmascript-resource-" + Guid.NewGuid().ToString("N"));
        // Module declarations carry their complete project path (for example clr/System/...).
        // Stage at the package root so the declared path is written exactly once.
        var stagedClr = staging;
        var stagedManifest = Path.Combine(staging, "manifest.json");
        var clrRoot = Path.Combine(packageRoot, "clr");
        var backupClr = Path.Combine(parent, ".ecmascript-resource-backup-" + Guid.NewGuid().ToString("N"));
        var backupManifest = Path.Combine(parent, ".ecmascript-manifest-backup-" + Guid.NewGuid().ToString("N"));
        var clrMoved = false;
        var manifestMoved = false;

        try
        {
            Directory.CreateDirectory(stagedClr);
            foreach (var module in modules)
            {
                var stagedPath = GetSafePath(stagedClr, module.RelativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(stagedPath)!);
                File.WriteAllText(stagedPath, module.Content, Utf8WithoutBom);
                if (!string.Equals(ComputeSha256Hex(File.ReadAllBytes(stagedPath)), module.Hash, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException($"Generated module '{module.RelativePath}' failed hash verification.");
            }

            File.WriteAllText(
                stagedManifest,
                JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true }),
                Utf8WithoutBom);
            using (JsonDocument.Parse(File.ReadAllText(stagedManifest)))
            {
            }

            if (Directory.Exists(clrRoot))
                Directory.Move(clrRoot, backupClr);
            clrMoved = true;
            Directory.Move(Path.Combine(staging, "clr"), clrRoot);

            if (File.Exists(manifestPath))
                File.Move(manifestPath, backupManifest);
            manifestMoved = true;
            File.Move(stagedManifest, manifestPath);

            DeleteDirectory(backupClr);
            DeleteFile(backupManifest);
        }
        catch
        {
            if (File.Exists(manifestPath) && manifestMoved)
                DeleteFile(manifestPath);
            if (File.Exists(backupManifest) && !File.Exists(manifestPath))
                File.Move(backupManifest, manifestPath);
            if (Directory.Exists(clrRoot) && clrMoved)
                DeleteDirectory(clrRoot);
            if (Directory.Exists(backupClr) && !Directory.Exists(clrRoot))
                Directory.Move(backupClr, clrRoot);
            throw;
        }
        finally
        {
            DeleteDirectory(staging);
            DeleteDirectory(backupClr);
            DeleteFile(backupManifest);
        }
    }

    private static string ResolveVersion(string repoRoot, string? requestedPackageVersion)
    {
        var candidates = new List<(string Name, string Value)>();
        if (!string.IsNullOrWhiteSpace(requestedPackageVersion))
            candidates.Add(("--version", requestedPackageVersion));

        foreach (var name in new[] { "JazorPackageVersion", "MinVerVersionOverride" })
        {
            var value = Environment.GetEnvironmentVariable(name);
            if (!string.IsNullOrWhiteSpace(value))
                candidates.Add((name, value));
        }

        if (candidates.Count > 0)
        {
            var versions = candidates
                .Select(candidate => (candidate.Name, Value: NormalizePackageVersion(candidate.Value, candidate.Name)))
                .ToArray();
            var distinct = versions
                .Select(static candidate => candidate.Value)
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            if (distinct.Length > 1)
            {
                throw new InvalidOperationException(
                    "Conflicting package versions were supplied: " +
                    string.Join(", ", versions.Select(static candidate => candidate.Name + "=" + candidate.Value)) + ".");
            }

            return distinct[0];
        }

        // A nearest tag is not a release source: on a dirty feature branch it silently points at
        // an older package. Only a clean checkout whose HEAD is exactly a semantic-version tag
        // is safe to infer without an explicit version.
        try
        {
            using var status = Process.Start(new ProcessStartInfo
            {
                FileName = "git",
                Arguments = "status --porcelain --untracked-files=all",
                WorkingDirectory = repoRoot,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });
            if (status is not null)
            {
                var statusText = status.StandardOutput.ReadToEnd();
                status.WaitForExit();
                if (status.ExitCode == 0 && string.IsNullOrWhiteSpace(statusText))
                {
                    using var tagProcess = Process.Start(new ProcessStartInfo
                    {
                        FileName = "git",
                        Arguments = "describe --exact-match --tags --abbrev=0 HEAD",
                        WorkingDirectory = repoRoot,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    });
                    if (tagProcess is not null)
                    {
                        var tag = tagProcess.StandardOutput.ReadToEnd().Trim();
                        tagProcess.WaitForExit();
                        if (tag.StartsWith("v", StringComparison.Ordinal) &&
                            tagProcess.ExitCode == 0)
                        {
                            return NormalizePackageVersion(tag[1..], "git tag");
                        }
                    }
                }
            }
        }
        catch
        {
        }

        throw new InvalidOperationException(
            "ECMAScript resource generation requires an explicit package version. " +
            "Pass '--version MAJOR.MINOR.PATCH[-prerelease][+metadata]' (or set JazorPackageVersion) when the checkout " +
            "is not a clean exact release tag.");
    }

    private static string NormalizePackageVersion(string value, string source)
    {
        var original = value.Trim();
        var text = original;
        var metadataIndex = text.IndexOf('+', StringComparison.Ordinal);
        var metadata = metadataIndex >= 0 ? text[(metadataIndex + 1)..] : null;
        if (metadataIndex >= 0)
            text = text[..metadataIndex];

        var separatorIndex = text.IndexOf('-', StringComparison.Ordinal);
        var numeric = separatorIndex >= 0 ? text[..separatorIndex] : text;
        var prerelease = separatorIndex >= 0 ? text[(separatorIndex + 1)..] : null;
        var numericParts = numeric.Split('.');
        var validPrerelease = prerelease is null ||
            (prerelease.Length > 0 && prerelease.All(static character =>
                char.IsLetterOrDigit(character) || character is '.' or '-'));
        var validMetadata = metadata is null ||
            (metadata.Length > 0 && metadata.All(static character =>
                char.IsLetterOrDigit(character) || character is '.' or '-'));
        if (numericParts.Length != 3 ||
            numericParts.Any(static part =>
                part.Length == 0 || part.Any(static character => !char.IsDigit(character)) || !int.TryParse(part, out _)) ||
            !validPrerelease ||
            !validMetadata)
        {
            throw new InvalidOperationException(
                $"Package version from {source} must be MAJOR.MINOR.PATCH with optional prerelease/build metadata: '{value}'.");
        }

        return original;
    }

    private static string NormalizeRelativePath(string path)
    {
        var normalized = path.Replace('\\', '/').Trim();
        if (string.IsNullOrWhiteSpace(normalized) || normalized.StartsWith('/', StringComparison.Ordinal) || Path.IsPathRooted(normalized))
            throw new InvalidOperationException($"Runtime module path must be relative: '{path}'.");
        var segments = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Where(static segment => segment != ".")
            .ToArray();
        if (segments.Length == 0 || segments.Any(static segment => segment == ".."))
            throw new InvalidOperationException($"Runtime module path cannot escape package root: '{path}'.");
        return string.Join('/', segments);
    }

    private static string GetSafePath(string root, string relativePath)
    {
        var normalizedRoot = Path.GetFullPath(root);
        var rootWithSeparator = normalizedRoot.EndsWith(Path.DirectorySeparatorChar)
            ? normalizedRoot
            : normalizedRoot + Path.DirectorySeparatorChar;
        var candidate = Path.GetFullPath(Path.Combine(normalizedRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        if (!candidate.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Runtime module path escapes staging root: '{relativePath}'.");
        return candidate;
    }

    private static string ComputeSha256Hex(string value)
        => ComputeSha256Hex(Utf8WithoutBom.GetBytes(value));

    private static string ComputeSha256Hex(byte[] bytes)
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static void DeleteFile(string path)
    {
        if (File.Exists(path))
            File.Delete(path);
    }

    private static void DeleteDirectory(string path)
    {
        if (Directory.Exists(path))
            Directory.Delete(path, recursive: true);
    }

    private sealed record ClrRuntimeModuleCandidate(
        INamedTypeSymbol RootType,
        SemanticModel SemanticModel,
        string RelativePath);

    /// <summary>
    /// 把载体模块写出的 import specifier 解析回逻辑 carrier 路径。
    ///
    /// 相对 specifier 以调用方的项目输出位置为起点；解析出的项目路径去掉 carrier 前缀就是
    /// 逻辑路径。裸 specifier 原样返回（真实包引用，如 "vue"）。
    /// </summary>
    private static string? ResolveLogicalImport(
        string importerRelativePath,
        string importSpecifier,
        IReadOnlySet<string> knownModulePaths)
    {
        if (string.IsNullOrWhiteSpace(importSpecifier))
            return null;

        if (!importSpecifier.StartsWith(".", StringComparison.Ordinal))
            return knownModulePaths.Contains(importSpecifier) ? importSpecifier : importSpecifier;

        // 相对 specifier 需要用 ResolveRelativePath 按 importer 展开（而不是 ResolveRelativeToImporter，
        // 后者是反方向：由两个项目路径反算相对 specifier）。
        var importerProjectPath = NormalizeRelativePath(importerRelativePath);
        var targetProjectPath = ECMAScriptModulePath.ResolveRelativePath(importerProjectPath, importSpecifier);
        var normalized = NormalizeRelativePath(targetProjectPath);
        return normalized;
    }

    private sealed record GeneratedClrRuntimeModule(
        string TypeName,
        string RelativePath,
        string Content,
        string Hash,
        IReadOnlyList<string> Imports)
    {
        public IReadOnlyList<string> ModuleDependencies { get; init; } = [];
        public IReadOnlyList<string> PackageDependencies { get; init; } = [];
    }
}
