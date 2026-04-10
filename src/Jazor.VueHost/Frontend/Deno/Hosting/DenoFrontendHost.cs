using Jazor.VueContracts.Protocol;
using Jazor.VueHost.Frontend.Deno.Protocol;
using Jazor.VueHost.Lsp;

namespace Jazor.VueHost.Frontend.Deno.Hosting;

internal sealed class DenoVolarHost : IDenoVolarHost
{
    private readonly DenoVolarHostOptions _options;
    private readonly IDenoWorkerProcess _workerProcess;
    private int _startupAttempted;

    public DenoVolarHost(DenoVolarHostOptions options)
        : this(options, workerProcess: null)
    {
    }

    internal DenoVolarHost(DenoVolarHostOptions options, IDenoWorkerProcess? workerProcess)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _workerProcess = workerProcess ?? new DenoWorkerProcess(_options);
    }

    public bool IsRunning => _workerProcess.IsRunning;

    public async ValueTask StartAsync(CancellationToken cancellationToken)
    {
        if (!_options.Enabled || Interlocked.Exchange(ref _startupAttempted, 1) != 0)
        {
            return;
        }

        try
        {
            await _workerProcess.StartAsync(cancellationToken);
        }
        catch when (_options.IgnoreStartupFailure)
        {
            // Keep the host process available even when the optional worker is unavailable.
        }
    }

    public async ValueTask StopAsync(CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
        {
            return;
        }

        await _workerProcess.StopAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync(CancellationToken.None);
    }

    public async ValueTask<IReadOnlyList<LspDiagnostic>> GetTemplateDiagnosticsAsync(
        DocumentSnapshot document,
        DenoVolarIntelliSenseContext? context,
        CancellationToken cancellationToken)
    {
        var request = new DenoTemplateDiagnosticRequest
        {
            DocumentPath = document.DocumentPath,
            Text = document.Text,
            FrontendContext = context?.SemanticContext,
            FrontendArtifacts = context?.Artifacts
        };
        var diagnostics = await SendAsync<LspDiagnostic[]>("template/diagnostics", request, cancellationToken);
        return diagnostics ?? Array.Empty<LspDiagnostic>();
    }

    public async ValueTask<IReadOnlyList<LspCompletionItem>> GetTemplateCompletionItemsAsync(
        DocumentSnapshot document,
        LspPosition position,
        DenoVolarIntelliSenseContext? context,
        CancellationToken cancellationToken)
    {
        var request = CreateRequest(document, position, context);
        var items = await SendAsync<LspCompletionItem[]>("template/completion", request, cancellationToken);
        return items ?? Array.Empty<LspCompletionItem>();
    }

    public async ValueTask<IReadOnlyList<LspDocumentSymbol>> GetTemplateDocumentSymbolsAsync(
        DocumentSnapshot document,
        DenoVolarIntelliSenseContext? context,
        CancellationToken cancellationToken)
    {
        var request = new DenoTemplateDocumentRequest
        {
            DocumentPath = document.DocumentPath,
            Text = document.Text,
            FrontendContext = context?.SemanticContext,
            FrontendArtifacts = context?.Artifacts
        };
        var symbols = await SendAsync<LspDocumentSymbol[]>("template/documentSymbols", request, cancellationToken);
        return symbols ?? Array.Empty<LspDocumentSymbol>();
    }

    public async ValueTask<IReadOnlyList<LspSemanticToken>> GetTemplateSemanticTokensAsync(
        DocumentSnapshot document,
        DenoVolarIntelliSenseContext? context,
        CancellationToken cancellationToken)
    {
        var request = new DenoTemplateSemanticTokensRequest
        {
            DocumentPath = document.DocumentPath,
            Text = document.Text,
            FrontendContext = context?.SemanticContext,
            FrontendArtifacts = context?.Artifacts
        };
        var tokens = await SendAsync<LspSemanticToken[]>("template/semanticTokens", request, cancellationToken);
        return tokens ?? Array.Empty<LspSemanticToken>();
    }

    public async ValueTask<LspHoverResult?> GetTemplateHoverAsync(
        DocumentSnapshot document,
        LspPosition position,
        DenoVolarIntelliSenseContext? context,
        CancellationToken cancellationToken)
    {
        var request = CreateRequest(document, position, context);
        return await SendAsync<LspHoverResult>("template/hover", request, cancellationToken);
    }

    public async ValueTask<IReadOnlyList<LspLocation>> GetTemplateDefinitionAsync(
        DocumentSnapshot document,
        LspPosition position,
        DenoVolarIntelliSenseContext? context,
        CancellationToken cancellationToken)
    {
        var request = CreateRequest(document, position, context);
        var locations = await SendAsync<LspLocation[]>("template/definition", request, cancellationToken);
        return locations ?? Array.Empty<LspLocation>();
    }

    public async ValueTask<IReadOnlyList<LspLocation>> GetTemplateReferencesAsync(
        DocumentSnapshot document,
        LspPosition position,
        bool includeDeclaration,
        DenoVolarIntelliSenseContext? context,
        CancellationToken cancellationToken)
    {
        var request = new DenoTemplateReferenceRequest
        {
            DocumentPath = document.DocumentPath,
            Text = document.Text,
            Position = position,
            IncludeDeclaration = includeDeclaration,
            FrontendContext = context?.SemanticContext,
            FrontendArtifacts = context?.Artifacts
        };
        var locations = await SendAsync<LspLocation[]>("template/references", request, cancellationToken);
        return locations ?? Array.Empty<LspLocation>();
    }

    public async ValueTask<LspWorkspaceEdit?> GetTemplateRenameAsync(
        DocumentSnapshot document,
        LspPosition position,
        string newName,
        DenoVolarIntelliSenseContext? context,
        CancellationToken cancellationToken)
    {
        var request = new DenoTemplateRenameRequest
        {
            DocumentPath = document.DocumentPath,
            Text = document.Text,
            Position = position,
            NewName = newName,
            FrontendContext = context?.SemanticContext,
            FrontendArtifacts = context?.Artifacts
        };
        return await SendAsync<LspWorkspaceEdit>("template/rename", request, cancellationToken);
    }

    private async ValueTask<TResult?> SendAsync<TResult>(
        string method,
        object payload,
        CancellationToken cancellationToken)
    {
        await EnsureStartedAsync(cancellationToken);
        if (!IsRunning)
        {
            return default;
        }

        try
        {
            return await _workerProcess.SendRequestAsync<TResult>(method, payload, cancellationToken);
        }
        catch when (_options.IgnoreStartupFailure)
        {
            return default;
        }
    }

    private async ValueTask EnsureStartedAsync(CancellationToken cancellationToken)
    {
        if (IsRunning || !_options.Enabled)
        {
            return;
        }

        await StartAsync(cancellationToken);
    }

    private static DenoTemplateRequest CreateRequest(
        DocumentSnapshot document,
        LspPosition position,
        DenoVolarIntelliSenseContext? context)
        => new()
        {
            DocumentPath = document.DocumentPath,
            Text = document.Text,
            Position = position,
            FrontendContext = context?.SemanticContext,
            FrontendArtifacts = context?.Artifacts
        };
}
