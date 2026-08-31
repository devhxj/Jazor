using System.Collections.Immutable;
using System.Threading.Tasks;
using Jazor.RazorVue.RazorSdk;
using Microsoft.CodeAnalysis;

namespace Jazor.RazorVue.Generation;

/// <summary>
/// Builds the Vue render catalog from the final Razor source-generator compilation.
/// It consumes bound operations and does not recreate Razor documents or IR.
/// 这是生成期末端协调器：选择组件、绑定操作、构建闭包，并将模块产物写入 generated catalog。
/// </summary>
internal static class RazorTailOutput
{
    // Roslyn lowering is CPU-bound, but unconstrained Task.Run fan-out makes generator hosts
    // compete with their own compilation work. Keep this deliberately small and deterministic.
    // 有界 worker 避免 source generator 与宿主编译争抢线程，不能按组件数无限并发。
    private const int MaximumArtifactBuildWorkers = 4;

    internal static bool TryBuildFinalCompilationCatalog(
        Compilation compilation,
        CancellationToken cancellationToken,
        out string? catalogSource,
        out ImmutableArray<RazorVueDiagnosticInfo> diagnostics,
        bool appendToExistingModuleCatalog = false)
    {
        catalogSource = null;
        diagnostics = ImmutableArray<RazorVueDiagnosticInfo>.Empty;

        // Only components with a bindable render body produce artifacts. Module-marked
        // container contracts and library implementations participate through imports, while
        // handwritten BuildRenderTree components remain valid tail output roots.
        ImmutableArray<INamedTypeSymbol> components;
        try
        {
            components = ComponentSelector.DiscoverTailOutputComponents(compilation);
        }
        catch (Exception exception)
        {
            diagnostics = ImmutableArray.Create(RazorVueDiagnosticFactory.FromException(
                exception,
                RazorVueDiagnosticCategory.Internal));
            return false;
        }

        if (components.IsDefaultOrEmpty)
            return true;

        if (!GeneratedCSharpBinder.TryBindFinalCompilationWithDiagnostics(
                compilation,
                components,
                out var binding,
                out diagnostics))
        {
            // The binder emits one typed diagnostic for every failed component. Keeping that
            // contract direct avoids a generic fallback that can never identify the failed root.
            return false;
        }

        if (!TryBuildVueRenderArtifacts(cancellationToken, binding!, out var artifacts, out diagnostics))
        {
            return false;
        }

        catalogSource = BuildModuleCatalogSource(
            artifacts,
            appendToExistingModuleCatalog,
            compilation.AssemblyName ?? "Jazor.RazorVue");
        return true;
    }

    private static bool TryBuildVueRenderArtifacts(
        CancellationToken cancellationToken,
        GeneratedCSharpBinding binding,
        out ImmutableArray<VueModuleArtifact> artifacts,
        out ImmutableArray<RazorVueDiagnosticInfo> diagnostics)
    {
        artifacts = ImmutableArray<VueModuleArtifact>.Empty;
        diagnostics = ImmutableArray<RazorVueDiagnosticInfo>.Empty;

        // Build closures on one stable thread first. Besides preserving the first closure
        // diagnostic, this separates graph discovery from independent module emission.
        // closure 仍串行建立，确保失败顺序不受 worker 调度影响；只有模块发射可并行。
        var inputs = ImmutableArray.CreateBuilder<ArtifactBuildInput>();
        var diagnosticBuilder = ImmutableArray.CreateBuilder<RazorVueDiagnosticInfo>();
        foreach (var component in binding.Components
                     .OrderBy(
                         static component => component.ComponentSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                         StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!MemberClosureBuilder.TryBuildWithDiagnostic(
                    binding,
                    component,
                    out var closure,
                    out var closureDiagnostic))
            {
                // TryBuildWithDiagnostic always carries the failure subject and category when it
                // returns false; do not erase that information behind a generic fallback.
                diagnosticBuilder.Add(closureDiagnostic!);
                continue;
            }

            inputs.Add(new ArtifactBuildInput(component, closure!));
        }

        if (diagnosticBuilder.Count > 0)
        {
            diagnostics = OrderDiagnostics(diagnosticBuilder);
            return false;
        }

        try
        {
            // VueInject is compilation-wide metadata. Validate it once before worker fan-out so
            // one invalid assembly declaration produces one precise error instead of an error per
            // component that happened to reach module framing first.
            // 注入注册表是 compilation 级契约，必须在并行 artifact 前单次验证。
            _ = VueInjectRegistry.ForCompilation(binding.Compilation);
        }
        catch (Exception exception)
        {
            diagnostics = ImmutableArray.Create(RazorVueDiagnosticFactory.FromException(
                exception,
                RazorVueDiagnosticCategory.VueInject));
            return false;
        }

        var results = BuildArtifacts(
            cancellationToken,
            binding,
            inputs.ToImmutable());
        var builder = ImmutableArray.CreateBuilder<VueModuleArtifact>(results.Length);
        // Workers can finish in any order. Consume the stable input order so diagnostics remain
        // deterministic while every independent component still contributes its first failure.
        // worker 完成顺序不参与诊断；按稳定索引回收并聚合独立组件失败。
        foreach (var result in results)
        {
            if (result.Diagnostic is not null)
            {
                diagnosticBuilder.Add(result.Diagnostic);
                continue;
            }

            builder.Add(result.Artifact!);
        }

        if (diagnosticBuilder.Count > 0)
        {
            diagnostics = OrderDiagnostics(diagnosticBuilder);
            return false;
        }

        try
        {
            // The route catalog is an ordinary generated artifact, not a second router
            // protocol. It keeps route discovery tied to the same final SG symbols and module
            // paths as pages/layouts, while Emit remains responsible for materialization.
            // 路由表与页面 artifact 使用同一最终符号/路径，不能另起页面侧注册协议。
            builder.Add(RazorVueRouteCatalogBuilder.Build(binding, builder.ToImmutable()));
        }
        catch (Exception exception)
        {
            diagnostics = ImmutableArray.Create(RazorVueDiagnosticFactory.FromException(
                exception,
                RazorVueDiagnosticCategory.VueModule,
                binding.Components[0].ComponentSymbol));
            return false;
        }

        artifacts = builder
            .OrderBy(static artifact => artifact.RelativePath, StringComparer.OrdinalIgnoreCase)
            .ThenBy(static artifact => artifact.ComponentId, StringComparer.Ordinal)
            .ToImmutableArray();
        return true;
    }

    private static ArtifactBuildResult[] BuildArtifacts(
        CancellationToken cancellationToken,
        GeneratedCSharpBinding binding,
        ImmutableArray<ArtifactBuildInput> inputs)
    {
        var results = new ArtifactBuildResult[inputs.Length];
        var workerCount = GetArtifactBuildWorkerCount(inputs.Length);
        if (workerCount <= 1)
        {
            for (var index = 0; index < inputs.Length; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                results[index] = BuildArtifactAsync(cancellationToken, binding, inputs[index])
                    .GetAwaiter()
                    .GetResult();
            }

            return results;
        }

        var nextIndex = -1;
        var workers = new Task[workerCount];
        for (var workerIndex = 0; workerIndex < workers.Length; workerIndex++)
        {
            workers[workerIndex] = Task.Run(
                async () =>
                {
                    while (true)
                    {
                        var index = Interlocked.Increment(ref nextIndex);
                        if (index >= inputs.Length)
                            return;

                        cancellationToken.ThrowIfCancellationRequested();
                        results[index] = await BuildArtifactAsync(
                                cancellationToken,
                                binding,
                                inputs[index])
                            .ConfigureAwait(false);
                    }
                },
                cancellationToken);
        }

        Task.WhenAll(workers).GetAwaiter().GetResult();
        return results;
    }

    private static async Task<ArtifactBuildResult> BuildArtifactAsync(
        CancellationToken cancellationToken,
        GeneratedCSharpBinding binding,
        ArtifactBuildInput input)
    {
        try
        {
            var artifact = await VueModuleBuilder.BuildAsync(
                    binding,
                    input.Component,
                    input.Closure,
                    cancellationToken)
                .ConfigureAwait(false);
            return new ArtifactBuildResult(artifact, null);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // External cancellation is control flow, not a component conversion diagnostic.
            throw;
        }
        catch (Exception ex)
        {
            return new ArtifactBuildResult(
                null,
                RazorVueDiagnosticFactory.FromException(
                    ex,
                    RazorVueDiagnosticCategory.VueModule,
                    input.Component.ComponentSymbol,
                    input.Component.BuildRenderTreeMethod));
        }
    }

    private static int GetArtifactBuildWorkerCount(int componentCount)
    {
        if (componentCount <= 0)
            return 0;

        return Math.Min(componentCount, MaximumArtifactBuildWorkers);
    }

    private sealed record ArtifactBuildInput(
        BoundComponent Component,
        MemberClosure Closure);

    private sealed record ArtifactBuildResult(
        VueModuleArtifact? Artifact,
        RazorVueDiagnosticInfo? Diagnostic);

    private static Location GetFirstComponentLocation(ImmutableArray<INamedTypeSymbol> components)
        => components
            .Select(RazorVueDiagnosticFactory.GetSymbolLocation)
            .FirstOrDefault(static location => location != Location.None) ?? Location.None;

    private static ImmutableArray<RazorVueDiagnosticInfo> OrderDiagnostics(
        ImmutableArray<RazorVueDiagnosticInfo>.Builder diagnostics)
        => diagnostics
            .OrderBy(static diagnostic => diagnostic.ComponentId ?? string.Empty, StringComparer.Ordinal)
            .ThenBy(static diagnostic => diagnostic.PrimaryLocation.GetLineSpan().Path ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .ThenBy(static diagnostic => diagnostic.PrimaryLocation.GetLineSpan().StartLinePosition.Line)
            .ThenBy(static diagnostic => diagnostic.PrimaryLocation.GetLineSpan().StartLinePosition.Character)
            .ThenBy(static diagnostic => Diagnostics.GetDescriptor(diagnostic.Category).Id, StringComparer.Ordinal)
            .ToImmutableArray();

    private static string BuildModuleCatalogSource(
        ImmutableArray<VueModuleArtifact> artifacts,
        bool appendToExistingModuleCatalog,
        string assemblyName)
    {
        // Both ESGenerator and RazorVue contribute to this one carrier. The base declaration owns
        // the public reflection shape; a Razor contribution only implements the two partial hooks.
        // 两个生成器共享一个 ModuleCatalog，避免 source map/HMR/asset 再分裂成并列 carrier。
        var builder = new System.Text.StringBuilder();
        builder.AppendLine("// <auto-generated/>");
        builder.AppendLine("#nullable enable");
        builder.AppendLine("namespace Jazor.Generated");
        builder.AppendLine("{");
        // The compiler generator owns the class declaration when it is present. A second
        // CompilerGenerated attribute on another partial declaration is rejected by Roslyn.
        // 编译器已有基础声明时这里只输出 partial contribution，不能重复类级特性。
        if (!appendToExistingModuleCatalog)
            builder.AppendLine("    [global::System.Runtime.CompilerServices.CompilerGenerated]");
        builder.AppendLine("    internal static partial class ModuleCatalog");
        builder.AppendLine("    {");

        if (!appendToExistingModuleCatalog)
        {
            builder.AppendLine("        internal const int SchemaVersion = 2;");
            builder.Append("        internal static string AssemblyName { get; } = ")
                .Append(EscapeCSharpString(assemblyName))
                .AppendLine(";");
            builder.AppendLine();
            builder.AppendLine("        internal static global::System.Collections.IEnumerable GetModules()");
            builder.AppendLine("        {");
            builder.AppendLine("            var modules = new global::System.Collections.Generic.List<object>(_modules.Length);");
            builder.AppendLine("            modules.AddRange(_modules);");
            builder.AppendLine("            AppendModules(modules);");
            builder.AppendLine("            return modules;");
            builder.AppendLine("        }");
            builder.AppendLine();
            builder.AppendLine("        internal static global::System.Collections.IEnumerable GetAssets()");
            builder.AppendLine("        {");
            builder.AppendLine("            var assets = new global::System.Collections.Generic.List<object>(_assets.Length);");
            builder.AppendLine("            assets.AddRange(_assets);");
            builder.AppendLine("            AppendAssets(assets);");
            builder.AppendLine("            return assets;");
            builder.AppendLine("        }");
            builder.AppendLine();
            builder.AppendLine("        static partial void AppendModules(global::System.Collections.Generic.List<object> modules);");
            builder.AppendLine("        static partial void AppendAssets(global::System.Collections.Generic.List<object> assets);");
            builder.AppendLine();
        }
        else
        {
            builder.AppendLine("        static partial void AppendModules(global::System.Collections.Generic.List<object> modules)");
            builder.AppendLine("        {");
            foreach (var artifact in artifacts)
                AppendModule(builder, artifact, "GeneratedVueModule", "                modules.Add(new GeneratedVueModule(");
            builder.AppendLine("        }");
            builder.AppendLine();
            builder.AppendLine("        static partial void AppendAssets(global::System.Collections.Generic.List<object> assets)");
            builder.AppendLine("        {");
            foreach (var asset in GetDistinctAssets(artifacts))
                AppendAsset(builder, asset, "GeneratedVueAsset", "                assets.Add(new GeneratedVueAsset(");
            builder.AppendLine("        }");
            builder.AppendLine();
        }

        if (!appendToExistingModuleCatalog)
        {
            AppendModuleType(builder, "GeneratedModule");
            AppendAssetType(builder, "GeneratedAsset");
            builder.AppendLine("        private static readonly GeneratedModule[] _modules = new GeneratedModule[]");
            builder.AppendLine("        {");
            foreach (var artifact in artifacts)
                AppendModule(builder, artifact, "GeneratedModule", "            new GeneratedModule(");
            builder.AppendLine("        };");
            builder.AppendLine();
            builder.AppendLine("        private static readonly GeneratedAsset[] _assets = new GeneratedAsset[]");
            builder.AppendLine("        {");
            foreach (var asset in GetDistinctAssets(artifacts))
                AppendAsset(builder, asset, "GeneratedAsset", "            new GeneratedAsset(");
            builder.AppendLine("        };");
        }
        else
        {
            AppendModuleType(builder, "GeneratedVueModule");
            AppendAssetType(builder, "GeneratedVueAsset");
        }

        builder.AppendLine("    }");
        builder.AppendLine("}");
        return builder.ToString();

        static IEnumerable<(string OwnerModulePath, VueAsset Asset)> GetDistinctAssets(ImmutableArray<VueModuleArtifact> values)
            => values.SelectMany(static artifact => artifact.Assets.Select(asset => (OwnerModulePath: artifact.RelativePath, Asset: asset)))
                .GroupBy(static asset => asset.OwnerModulePath + "\n" + asset.Asset.ArtifactPath, StringComparer.OrdinalIgnoreCase)
                .Select(static group => group.OrderBy(static asset => asset.Asset.SourcePath, StringComparer.Ordinal).First())
                .OrderBy(static asset => asset.OwnerModulePath, StringComparer.OrdinalIgnoreCase)
                .ThenBy(static asset => asset.Asset.ArtifactPath, StringComparer.OrdinalIgnoreCase)
                .ThenBy(static asset => asset.Asset.SourcePath, StringComparer.Ordinal);

        static void AppendModuleType(System.Text.StringBuilder target, string typeName)
        {
            target.AppendLine("        [global::System.Runtime.CompilerServices.CompilerGenerated]");
            target.Append("        private sealed class ").AppendLine(typeName);
            target.AppendLine("        {");
            target.AppendLine("            public "+typeName+"(string id, string typeName, string relativePath, string content, string hash, string sourceMapRelativePath, string sourceMapContent, string mapHash, string hmrProviderId, string hmrModuleId, string hmrPayload, string[] packageImports, string[] dependencies)");
            target.AppendLine("            {");
            target.AppendLine("                Id = id; TypeName = typeName; RelativePath = relativePath; Content = content; Hash = hash;");
            target.AppendLine("                SourceMapRelativePath = sourceMapRelativePath; SourceMapContent = sourceMapContent; MapHash = mapHash;");
            target.AppendLine("                HmrProviderId = hmrProviderId; HmrModuleId = hmrModuleId; HmrPayload = hmrPayload;");
            target.AppendLine("                PackageImports = packageImports; Dependencies = dependencies;");
            target.AppendLine("            }");
            target.AppendLine("            public string Id { get; } public string TypeName { get; } public string RelativePath { get; }");
            target.AppendLine("            public string Content { get; } public string Hash { get; }");
            target.AppendLine("            public string SourceMapRelativePath { get; } public string SourceMapContent { get; } public string MapHash { get; }");
            target.AppendLine("            public string HmrProviderId { get; } public string HmrModuleId { get; } public string HmrPayload { get; }");
            target.AppendLine("            public string[] PackageImports { get; } public string[] Dependencies { get; }");
            target.AppendLine("        }");
            target.AppendLine();
        }

        static void AppendAssetType(System.Text.StringBuilder target, string typeName)
        {
            target.AppendLine("        [global::System.Runtime.CompilerServices.CompilerGenerated]");
            target.Append("        private sealed class ").AppendLine(typeName);
            target.AppendLine("        {");
            target.Append("            public ").Append(typeName).AppendLine("(string ownerModulePath, string sourcePath, string artifactPath, string kind, string importPath, string contentHash)");
            target.AppendLine("            { OwnerModulePath = ownerModulePath; SourcePath = sourcePath; ArtifactPath = artifactPath; Kind = kind; ImportPath = importPath; ContentHash = contentHash; }");
            target.AppendLine("            public string OwnerModulePath { get; } public string SourcePath { get; } public string ArtifactPath { get; } public string Kind { get; }");
            target.AppendLine("            public string ImportPath { get; } public string ContentHash { get; }");
            target.AppendLine("        }");
            target.AppendLine();
        }

        static void AppendModule(System.Text.StringBuilder target, VueModuleArtifact artifact, string typeName, string prefix)
        {
            target.AppendLine(prefix);
            target.Append("                id: ").Append(EscapeCSharpString(artifact.ComponentId)).AppendLine(",");
            target.Append("                typeName: ").Append(EscapeCSharpString(artifact.ComponentId)).AppendLine(",");
            target.Append("                relativePath: ").Append(EscapeCSharpString(artifact.RelativePath)).AppendLine(",");
            target.Append("                content: ").Append(EscapeCSharpString(artifact.ModuleText)).AppendLine(",");
            target.Append("                hash: ").Append(EscapeCSharpString(artifact.ContentHash)).AppendLine(",");
            target.Append("                sourceMapRelativePath: ").Append(EscapeCSharpString(artifact.SourceMapRelativePath)).AppendLine(",");
            target.Append("                sourceMapContent: ").Append(EscapeCSharpString(artifact.SourceMapContent)).AppendLine(",");
            target.Append("                mapHash: ").Append(EscapeCSharpString(artifact.MapHash)).AppendLine(",");
            target.AppendLine("                hmrProviderId: \"jazor.vue\",");
            target.Append("                hmrModuleId: ").Append(EscapeCSharpString(artifact.Hmr.ModuleId)).AppendLine(",");
            target.Append("                hmrPayload: ").Append(EscapeCSharpString(BuildVueHmrPayload(artifact.ComponentId, artifact.Hmr))).AppendLine(",");
            target.Append("                packageImports: new string[] { ").Append(string.Join(", ", artifact.PackageImports.Select(EscapeCSharpString))).AppendLine(" },");
            target.Append("                dependencies: new string[] { ").Append(string.Join(", ", (artifact.Dependencies.IsDefault ? ImmutableArray<string>.Empty : artifact.Dependencies).Select(EscapeCSharpString)));
            target.AppendLine(prefix.Contains("modules.Add", StringComparison.Ordinal) ? " }));" : " }),");
        }

        static void AppendAsset(System.Text.StringBuilder target, (string OwnerModulePath, VueAsset Asset) asset, string typeName, string prefix)
        {
            target.AppendLine(prefix);
            target.Append("                ownerModulePath: ").Append(EscapeCSharpString(asset.OwnerModulePath)).AppendLine(",");
            target.Append("                sourcePath: ").Append(EscapeCSharpString(asset.Asset.SourcePath)).AppendLine(",");
            target.Append("                artifactPath: ").Append(EscapeCSharpString(asset.Asset.ArtifactPath)).AppendLine(",");
            target.Append("                kind: ").Append(EscapeCSharpString(asset.Asset.Kind)).AppendLine(",");
            target.Append("                importPath: ").Append(EscapeCSharpString(asset.Asset.ImportPath)).AppendLine(",");
            target.Append("                contentHash: ").Append(EscapeCSharpString(asset.Asset.ContentHash));
            target.AppendLine(prefix.Contains("assets.Add", StringComparison.Ordinal) ? "));" : "),");
        }
    }

    private static string BuildVueHmrPayload(string componentId, VueHmrMetadata hmr)
        => "{\"componentId\":" + EscapeJsonString(componentId) +
           ",\"descriptorHash\":" + EscapeJsonString(hmr.DescriptorHash) +
           ",\"templateHash\":" + EscapeJsonString(hmr.TemplateHash) +
           ",\"logicHash\":" + EscapeJsonString(hmr.LogicHash) +
           ",\"boundaryKind\":" + EscapeJsonString(hmr.BoundaryKind.ToWireValue()) + "}";

    private static string EscapeJsonString(string value)
    {
        var builder = new System.Text.StringBuilder(value.Length + 2);
        builder.Append('"');
        foreach (var ch in value)
        {
            builder.Append(ch switch
            {
                '"' => "\\\"",
                '\\' => "\\\\",
                '\b' => "\\b",
                '\f' => "\\f",
                '\n' => "\\n",
                '\r' => "\\r",
                '\t' => "\\t",
                _ when ch < ' ' => "\\u" + ((int)ch).ToString("x4", System.Globalization.CultureInfo.InvariantCulture),
                _ => ch.ToString()
            });
        }

        builder.Append('"');
        return builder.ToString();
    }

    private static string EscapeCSharpString(string value)
    {
        var builder = new System.Text.StringBuilder(value.Length + 2);
        builder.Append('"');
        foreach (var ch in value)
        {
            builder.Append(ch switch
            {
                '\\' => "\\\\",
                '"' => "\\\"",
                '\0' => "\\0",
                '\a' => "\\a",
                '\b' => "\\b",
                '\f' => "\\f",
                '\n' => "\\n",
                '\r' => "\\r",
                '\t' => "\\t",
                '\v' => "\\v",
                _ => ch.ToString()
            });
        }

        builder.Append('"');
        return builder.ToString();
    }
}
