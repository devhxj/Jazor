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
        out ImmutableArray<RazorVueDiagnosticInfo> diagnostics)
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
            if (diagnostics.IsDefaultOrEmpty)
            {
                diagnostics = ImmutableArray.Create(RazorVueDiagnosticFactory.Create(
                    RazorVueDiagnosticCategory.ComponentBinding,
                    "No component binding detail was provided.",
                    GetFirstComponentLocation(components),
                    components.FirstOrDefault()));
            }
            return false;
        }

        if (!TryBuildVueRenderArtifacts(cancellationToken, binding!, out var artifacts, out diagnostics))
        {
            return false;
        }

        catalogSource = BuildArtifactCatalogSource(artifacts);
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
                diagnosticBuilder.Add(closureDiagnostic ?? RazorVueDiagnosticFactory.Create(
                    RazorVueDiagnosticCategory.MemberClosure,
                    "No component member closure detail was provided.",
                    RazorVueDiagnosticFactory.GetSymbolLocation(component.BuildRenderTreeMethod),
                    component.ComponentSymbol));
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

    private static string BuildArtifactCatalogSource(ImmutableArray<VueModuleArtifact> artifacts)
    {
        // Emit neutral data carriers only. Jazor.Emit owns materialization of .mjs/.map/assets;
        // this generator must not write files or embed an alternate emission protocol.
        // catalog 仅声明 artifact 数据，文件落盘仍属于 Emit 边界。
        var builder = new System.Text.StringBuilder();
        builder.AppendLine("// <auto-generated/>");
        builder.AppendLine("#nullable enable");
        builder.AppendLine("namespace Jazor.Generated");
        builder.AppendLine("{");
        builder.AppendLine("    [global::System.Runtime.CompilerServices.CompilerGenerated]");
        builder.AppendLine("    internal static partial class ArtifactCatalog");
        builder.AppendLine("    {");
        builder.AppendLine("        internal const int SchemaVersion = 1;");
        builder.AppendLine("        internal const string ProducerId = \"jazor.vue\";");
        builder.AppendLine();
        builder.AppendLine("        internal static global::System.Collections.IEnumerable GetModules()");
        builder.AppendLine("        {");
        builder.AppendLine("            return _modules;");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        internal static global::System.Collections.IEnumerable GetAssets()");
        builder.AppendLine("        {");
        builder.AppendLine("            return _assets;");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        [global::System.Runtime.CompilerServices.CompilerGenerated]");
        builder.AppendLine("        private sealed class GeneratedArtifactModule");
        builder.AppendLine("        {");
        builder.AppendLine("            public GeneratedArtifactModule(string id, string typeName, string relativePath, string content, string hash, string sourceMapRelativePath, string sourceMapContent, string mapHash, string hmrProviderId, string hmrModuleId, string hmrPayload, string[] packageImports)");
        builder.AppendLine("            {");
        builder.AppendLine("                Id = id;");
        builder.AppendLine("                TypeName = typeName;");
        builder.AppendLine("                RelativePath = relativePath;");
        builder.AppendLine("                Content = content;");
        builder.AppendLine("                Hash = hash;");
        builder.AppendLine("                SourceMapRelativePath = sourceMapRelativePath;");
        builder.AppendLine("                SourceMapContent = sourceMapContent;");
        builder.AppendLine("                MapHash = mapHash;");
        builder.AppendLine("                HmrProviderId = hmrProviderId;");
        builder.AppendLine("                HmrModuleId = hmrModuleId;");
        builder.AppendLine("                HmrPayload = hmrPayload;");
        builder.AppendLine("                PackageImports = packageImports;");
        builder.AppendLine("            }");
        builder.AppendLine();
        builder.AppendLine("            public string Id { get; }");
        builder.AppendLine("            public string TypeName { get; }");
        builder.AppendLine("            public string RelativePath { get; }");
        builder.AppendLine("            public string Content { get; }");
        builder.AppendLine("            public string Hash { get; }");
        builder.AppendLine("            public string SourceMapRelativePath { get; }");
        builder.AppendLine("            public string SourceMapContent { get; }");
        builder.AppendLine("            public string MapHash { get; }");
        builder.AppendLine("            public string HmrProviderId { get; }");
        builder.AppendLine("            public string HmrModuleId { get; }");
        builder.AppendLine("            public string HmrPayload { get; }");
        builder.AppendLine("            public string[] PackageImports { get; }");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        [global::System.Runtime.CompilerServices.CompilerGenerated]");
        builder.AppendLine("        private sealed class GeneratedArtifactAsset");
        builder.AppendLine("        {");
        builder.AppendLine("            public GeneratedArtifactAsset(string sourcePath, string artifactPath, string kind, string importPath, string contentHash)");
        builder.AppendLine("            {");
        builder.AppendLine("                SourcePath = sourcePath;");
        builder.AppendLine("                ArtifactPath = artifactPath;");
        builder.AppendLine("                Kind = kind;");
        builder.AppendLine("                ImportPath = importPath;");
        builder.AppendLine("                ContentHash = contentHash;");
        builder.AppendLine("            }");
        builder.AppendLine();
        builder.AppendLine("            public string SourcePath { get; }");
        builder.AppendLine("            public string ArtifactPath { get; }");
        builder.AppendLine("            public string Kind { get; }");
        builder.AppendLine("            public string ImportPath { get; }");
        builder.AppendLine("            public string ContentHash { get; }");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        private static readonly GeneratedArtifactModule[] _modules = new GeneratedArtifactModule[]");
        builder.AppendLine("        {");

        foreach (var artifact in artifacts)
        {
            builder.AppendLine("            new GeneratedArtifactModule(");
            builder.Append("                id: ").Append(EscapeCSharpString(artifact.ComponentId)).AppendLine(",");
            builder.Append("                typeName: ").Append(EscapeCSharpString(artifact.ComponentId)).AppendLine(",");
            builder.Append("                relativePath: ").Append(EscapeCSharpString(artifact.RelativePath)).AppendLine(",");
            builder.Append("                content: ").Append(EscapeCSharpString(artifact.ModuleText)).AppendLine(",");
            builder.Append("                hash: ").Append(EscapeCSharpString(artifact.ContentHash)).AppendLine(",");
            builder.Append("                sourceMapRelativePath: ").Append(EscapeCSharpString(artifact.SourceMapRelativePath)).AppendLine(",");
            builder.Append("                sourceMapContent: ").Append(EscapeCSharpString(artifact.SourceMapContent)).AppendLine(",");
            builder.Append("                mapHash: ").Append(EscapeCSharpString(artifact.MapHash)).AppendLine(",");
            builder.Append("                hmrProviderId: \"jazor.vue\",").AppendLine();
            builder.Append("                hmrModuleId: ").Append(EscapeCSharpString(artifact.Hmr.ModuleId)).AppendLine(",");
            builder.Append("                hmrPayload: ").Append(EscapeCSharpString(BuildVueHmrPayload(artifact.ComponentId, artifact.Hmr))).AppendLine(",");
            builder.Append("                packageImports: new string[] { ");
            builder.Append(string.Join(", ", artifact.PackageImports.Select(EscapeCSharpString)));
            builder.AppendLine(" }),");
        }

        builder.AppendLine("        };");
        builder.AppendLine();
        builder.AppendLine("        private static readonly GeneratedArtifactAsset[] _assets = new GeneratedArtifactAsset[]");
        builder.AppendLine("        {");

        foreach (var asset in artifacts
                     .SelectMany(static artifact => artifact.Assets)
                     .GroupBy(static asset => asset.ArtifactPath, StringComparer.OrdinalIgnoreCase)
                     .Select(static group => group.First())
                     .OrderBy(static asset => asset.ArtifactPath, StringComparer.OrdinalIgnoreCase)
                     .ThenBy(static asset => asset.SourcePath, StringComparer.Ordinal))
        {
            builder.AppendLine("            new GeneratedArtifactAsset(");
            builder.Append("                sourcePath: ").Append(EscapeCSharpString(asset.SourcePath)).AppendLine(",");
            builder.Append("                artifactPath: ").Append(EscapeCSharpString(asset.ArtifactPath)).AppendLine(",");
            builder.Append("                kind: ").Append(EscapeCSharpString(asset.Kind)).AppendLine(",");
            builder.Append("                importPath: ").Append(EscapeCSharpString(asset.ImportPath)).AppendLine(",");
            builder.Append("                contentHash: ").Append(EscapeCSharpString(asset.ContentHash)).AppendLine("),");
        }

        builder.AppendLine("        };");
        builder.AppendLine("    }");
        builder.AppendLine("}");
        return builder.ToString();
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
