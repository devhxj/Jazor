using Microsoft.CodeAnalysis;

namespace Jazor.RazorVue.Generation;

/// <summary>
/// Registers RazorVue generation against the completed compilation exposed by the analyzer hook.
/// Incremental generators cannot otherwise observe source produced by another generator in the same pass.
/// 该生成器只消费 Razor SG 已完成的 C# 编译结果，不重新解析 Razor 或建立平行前端。
/// </summary>
[Generator]
public sealed class RazorVueGenerator : IIncrementalGenerator
{
    public RazorVueGenerator()
    {
        // GeneratorDriver invokes Initialize from inside RunGeneratorsAndUpdateCompilation.
        // Revalidate here so a tiered-JIT replacement is repaired before that driver call.
        Bootstrap.Initialize();
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Must precede registration because the current generator pass needs the native hook.
        // 必须先确认 hook；否则本轮 Razor SG 产物不会进入 RazorVue 的最终编译输入。
        Bootstrap.Initialize();

        var razorSources = context.AdditionalTextsProvider
            .Select(static (source, cancellationToken) => RazorSourceTextRegistry.TryCreate(source, cancellationToken))
            .Where(static source => source is not null)
            .Select(static (source, _) => source!.Value)
            .Collect();
        context.RegisterImplementationSourceOutput(
            razorSources,
            static (output, sources) =>
            {
                if (sources.IsDefaultOrEmpty)
                    return;

                output.AddSource(
                    RazorSourceTextRegistry.CarrierHintName,
                    Microsoft.CodeAnalysis.Text.SourceText.From(
                        RazorSourceTextRegistry.BuildCarrierSource(sources),
                        System.Text.Encoding.UTF8));
            });

        var failure = InitializeHookInstaller.GetInstallFailure();
        if (string.IsNullOrEmpty(failure))
            return;

        context.RegisterSourceOutput(
            context.CompilationProvider,
            (output, _) => output.ReportDiagnostic(Diagnostic.Create(
                Diagnostics.TailOutputFailed,
                Location.None,
                failure)));
    }
}
