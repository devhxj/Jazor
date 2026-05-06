using Jolt.Analysis;
using Jolt.Volar.Deno.Hosting;
using Jolt.Volar.Deno.Protocol;
using Jolt.Lsp;
using Jazor.RazorVue.Protocol;
using Jolt.Services;
using Jolt.Workspace;

namespace Jolt.Test;

[TestClass]
public sealed class JoltServiceLifecycleTests
{
    [TestMethod]
    public async Task JoltService_ConcurrentStartAndStop_SerializesUnderlyingHostLifecycle()
    {
        var denoHost = new BlockingDenoHost();
        var service = new JoltService(
            new InMemoryWorkspaceStore(),
            new JazorVueAnalysisService(),
            denoHost);

        var startTask = service.StartAsync(CancellationToken.None).AsTask();
        await denoHost.StartEntered.Task;

        var stopTask = service.StopAsync(CancellationToken.None).AsTask();
        await Task.Delay(100);
        Assert.AreEqual(0, denoHost.StopCallCount, "Stop should wait until start finishes.");

        denoHost.AllowStart.SetResult(true);
        await Task.WhenAll(startTask, stopTask);

        Assert.AreEqual(1, denoHost.StartCallCount);
        Assert.AreEqual(1, denoHost.StopCallCount);
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.OpenDocumentAsync(
                new DocumentSnapshot("Counter.jazor", DocumentKind.Jazor, "<template />", "1"),
                CancellationToken.None));
    }

    [TestMethod]
    public async Task JoltService_RepeatedStart_DoesNotRestartUnderlyingHost()
    {
        var denoHost = new BlockingDenoHost();
        denoHost.AllowStart.SetResult(true);
        var service = new JoltService(
            new InMemoryWorkspaceStore(),
            new JazorVueAnalysisService(),
            denoHost);

        await service.StartAsync(CancellationToken.None);
        await service.StartAsync(CancellationToken.None);
        await service.StopAsync(CancellationToken.None);

        Assert.AreEqual(1, denoHost.StartCallCount);
        Assert.AreEqual(1, denoHost.StopCallCount);
    }

    private sealed class BlockingDenoHost : IDenoVolarHost
    {
        public TaskCompletionSource<bool> StartEntered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource<bool> AllowStart { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public int StartCallCount { get; private set; }

        public int StopCallCount { get; private set; }

        public bool IsEnabled => true;

        public bool IsRunning => StartCallCount > StopCallCount;

        public async ValueTask StartAsync(CancellationToken cancellationToken)
        {
            StartCallCount++;
            StartEntered.TrySetResult(true);
            await AllowStart.Task.WaitAsync(cancellationToken);
        }

        public ValueTask StopAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            StopCallCount++;
            return ValueTask.CompletedTask;
        }

        public ValueTask DisposeAsync()
            => ValueTask.CompletedTask;

        public ValueTask<DenoSfcCompileResult?> CompileSfcAsync(
            string documentPath,
            string sfcText,
            string filename,
            CancellationToken cancellationToken)
            => ValueTask.FromResult<DenoSfcCompileResult?>(default);

        public ValueTask<IReadOnlyList<LspDiagnostic>> GetTemplateDiagnosticsAsync(
            DocumentSnapshot document,
            DenoVolarIntelliSenseContext? context,
            CancellationToken cancellationToken)
            => ValueTask.FromResult<IReadOnlyList<LspDiagnostic>>(Array.Empty<LspDiagnostic>());

        public ValueTask<IReadOnlyList<LspCompletionItem>> GetTemplateCompletionItemsAsync(
            DocumentSnapshot document,
            LspPosition position,
            DenoVolarIntelliSenseContext? context,
            CancellationToken cancellationToken)
            => ValueTask.FromResult<IReadOnlyList<LspCompletionItem>>(Array.Empty<LspCompletionItem>());

        public ValueTask<IReadOnlyList<LspDocumentSymbol>> GetTemplateDocumentSymbolsAsync(
            DocumentSnapshot document,
            DenoVolarIntelliSenseContext? context,
            CancellationToken cancellationToken)
            => ValueTask.FromResult<IReadOnlyList<LspDocumentSymbol>>(Array.Empty<LspDocumentSymbol>());

        public ValueTask<IReadOnlyList<LspSemanticToken>> GetTemplateSemanticTokensAsync(
            DocumentSnapshot document,
            DenoVolarIntelliSenseContext? context,
            CancellationToken cancellationToken)
            => ValueTask.FromResult<IReadOnlyList<LspSemanticToken>>(Array.Empty<LspSemanticToken>());

        public ValueTask<LspHoverResult?> GetTemplateHoverAsync(
            DocumentSnapshot document,
            LspPosition position,
            DenoVolarIntelliSenseContext? context,
            CancellationToken cancellationToken)
            => ValueTask.FromResult<LspHoverResult?>(null);

        public ValueTask<IReadOnlyList<LspLocation>> GetTemplateDefinitionAsync(
            DocumentSnapshot document,
            LspPosition position,
            DenoVolarIntelliSenseContext? context,
            CancellationToken cancellationToken)
            => ValueTask.FromResult<IReadOnlyList<LspLocation>>(Array.Empty<LspLocation>());

        public ValueTask<IReadOnlyList<LspLocation>> GetTemplateReferencesAsync(
            DocumentSnapshot document,
            LspPosition position,
            bool includeDeclaration,
            DenoVolarIntelliSenseContext? context,
            CancellationToken cancellationToken)
            => ValueTask.FromResult<IReadOnlyList<LspLocation>>(Array.Empty<LspLocation>());

        public ValueTask<LspWorkspaceEdit?> GetTemplateRenameAsync(
            DocumentSnapshot document,
            LspPosition position,
            string newName,
            DenoVolarIntelliSenseContext? context,
            CancellationToken cancellationToken)
            => ValueTask.FromResult<LspWorkspaceEdit?>(null);
    }
}

