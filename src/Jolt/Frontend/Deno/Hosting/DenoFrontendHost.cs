using Jazor.VueContracts.Protocol;
using Jolt.Frontend.Deno.Protocol;
using Jolt.Lsp;

namespace Jolt.Frontend.Deno.Hosting;

internal sealed class DenoVolarHost : IDenoVolarHost
{
    private readonly DenoVolarHostOptions _options;
    private readonly IDenoWorkerProcess _workerProcess;
    private readonly SemaphoreSlim _lifecycleGate = new(1, 1);
    private static readonly TimeSpan RetryBackoff = TimeSpan.FromMilliseconds(100);

    public DenoVolarHost(DenoVolarHostOptions options)
        : this(options, workerProcess: null)
    {
    }

    internal DenoVolarHost(DenoVolarHostOptions options, IDenoWorkerProcess? workerProcess)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _workerProcess = workerProcess ?? new DenoWorkerProcess(_options);
    }

    public bool IsEnabled => _options.Enabled;

    public bool IsRunning => _workerProcess.IsRunning;

    public async ValueTask StartAsync(CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
        {
            return;
        }

        await _lifecycleGate.WaitAsync(cancellationToken);
        try
        {
            if (IsRunning)
            {
                return;
            }

            try
            {
                await _workerProcess.StartAsync(cancellationToken);
            }
            catch (System.ComponentModel.Win32Exception) when (_options.IgnoreStartupFailure)
            {
                await ResetWorkerStateCoreAsync();
            }
            catch (UnauthorizedAccessException) when (_options.IgnoreStartupFailure)
            {
                await ResetWorkerStateCoreAsync();
            }
            catch (IOException) when (_options.IgnoreStartupFailure)
            {
                await ResetWorkerStateCoreAsync();
            }
            catch (InvalidOperationException) when (_options.IgnoreStartupFailure)
            {
                await ResetWorkerStateCoreAsync();
            }
            catch (ArgumentException) when (_options.IgnoreStartupFailure)
            {
                await ResetWorkerStateCoreAsync();
            }
            catch (NotSupportedException) when (_options.IgnoreStartupFailure)
            {
                await ResetWorkerStateCoreAsync();
            }
        }
        finally
        {
            _lifecycleGate.Release();
        }
    }

    public async ValueTask StopAsync(CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
        {
            return;
        }

        await _lifecycleGate.WaitAsync(cancellationToken);
        try
        {
            await _workerProcess.StopAsync(cancellationToken);
        }
        finally
        {
            _lifecycleGate.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await StopAsync(CancellationToken.None);
        }
        finally
        {
            _lifecycleGate.Dispose();
        }
    }

    public async ValueTask<DenoSfcCompileResult?> CompileSfcAsync(
        string documentPath,
        string sfcText,
        string filename,
        CancellationToken cancellationToken)
        => await CompileSfcAsync(
            documentPath,
            sfcText,
            filename,
            isProduction: false,
            cancellationToken);

    public async ValueTask<DenoSfcCompileResult?> CompileSfcAsync(
        string documentPath,
        string sfcText,
        string filename,
        bool isProduction,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(documentPath);
        ArgumentNullException.ThrowIfNull(sfcText);
        ArgumentException.ThrowIfNullOrWhiteSpace(filename);

        var request = new DenoSfcCompileRequest
        {
            DocumentPath = documentPath,
            SfcText = sfcText,
            Filename = filename,
            IsProduction = isProduction
        };

        return await SendAsync<DenoSfcCompileResult>("compile/sfc", request, cancellationToken);
    }

    public async ValueTask<DenoTypeScriptCompileResult?> CompileTypeScriptAsync(
        string documentPath,
        string text,
        string filename,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(documentPath);
        ArgumentNullException.ThrowIfNull(text);
        ArgumentException.ThrowIfNullOrWhiteSpace(filename);

        var request = new DenoTypeScriptCompileRequest
        {
            DocumentPath = documentPath,
            Text = text,
            Filename = filename
        };

        return await SendAsync<DenoTypeScriptCompileResult>("compile/ts", request, cancellationToken);
    }

    public async ValueTask<DenoCssModuleCompileResult?> CompileCssModuleAsync(
        string documentPath,
        string text,
        string filename,
        bool isProduction,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(documentPath);
        ArgumentNullException.ThrowIfNull(text);
        ArgumentException.ThrowIfNullOrWhiteSpace(filename);

        var request = new DenoCssModuleCompileRequest
        {
            DocumentPath = documentPath,
            Text = text,
            Filename = filename,
            IsProduction = isProduction
        };

        return await SendAsync<DenoCssModuleCompileResult>("compile/css-module", request, cancellationToken);
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

    public async ValueTask<IReadOnlyList<LspDocumentLink>> GetTemplateDocumentLinksAsync(
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
        var links = await SendAsync<LspDocumentLink[]>("template/documentLinks", request, cancellationToken);
        return links ?? Array.Empty<LspDocumentLink>();
    }

    public async ValueTask<IReadOnlyList<LspInlayHint>> GetTemplateInlayHintsAsync(
        DocumentSnapshot document,
        LspRange range,
        DenoVolarIntelliSenseContext? context,
        CancellationToken cancellationToken)
    {
        var request = new DenoTemplateRangeRequest
        {
            DocumentPath = document.DocumentPath,
            Text = document.Text,
            Range = range,
            FrontendContext = context?.SemanticContext,
            FrontendArtifacts = context?.Artifacts
        };
        var hints = await SendAsync<LspInlayHint[]>("template/inlayHints", request, cancellationToken);
        return hints ?? Array.Empty<LspInlayHint>();
    }

    public async ValueTask<IReadOnlyList<LspFoldingRange>> GetTemplateFoldingRangesAsync(
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
        var ranges = await SendAsync<LspFoldingRange[]>("template/foldingRanges", request, cancellationToken);
        return ranges ?? Array.Empty<LspFoldingRange>();
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

    public async ValueTask<IReadOnlyList<LspLocation>> GetTemplateImplementationAsync(
        DocumentSnapshot document,
        LspPosition position,
        DenoVolarIntelliSenseContext? context,
        CancellationToken cancellationToken)
    {
        var request = CreateRequest(document, position, context);
        var locations = await SendAsync<LspLocation[]>("template/implementation", request, cancellationToken);
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
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (ObjectDisposedException)
        {
            await TryResetWorkerStateAsync();
        }
        catch (IOException)
        {
            await TryResetWorkerStateAsync();
        }
        catch (InvalidOperationException)
        {
            await TryResetWorkerStateAsync();
        }
        catch (System.Text.Json.JsonException)
        {
            await TryResetWorkerStateAsync();
        }
        catch (NotSupportedException)
        {
            await TryResetWorkerStateAsync();
        }

        await Task.Delay(RetryBackoff, cancellationToken);
        await EnsureStartedAsync(cancellationToken);
        if (!IsRunning)
        {
            return default;
        }

        try
        {
            return await _workerProcess.SendRequestAsync<TResult>(method, payload, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (ObjectDisposedException)
        {
            await TryResetWorkerStateAsync();
            throw;
        }
        catch (IOException)
        {
            await TryResetWorkerStateAsync();
            throw;
        }
        catch (InvalidOperationException)
        {
            await TryResetWorkerStateAsync();
            throw;
        }
        catch (System.Text.Json.JsonException)
        {
            await TryResetWorkerStateAsync();
            throw;
        }
        catch (NotSupportedException)
        {
            await TryResetWorkerStateAsync();
            throw;
        }
    }

    private async ValueTask TryResetWorkerStateAsync()
    {
        await _lifecycleGate.WaitAsync(CancellationToken.None);
        try
        {
            await ResetWorkerStateCoreAsync();
        }
        finally
        {
            _lifecycleGate.Release();
        }
    }

    private async ValueTask ResetWorkerStateCoreAsync()
    {
        try
        {
            await _workerProcess.StopAsync(CancellationToken.None);
        }
        catch (ObjectDisposedException)
        {
            // Ignore worker teardown failures so the next request can retry startup.
        }
        catch (IOException)
        {
            // Ignore worker teardown failures so the next request can retry startup.
        }
        catch (System.ComponentModel.Win32Exception)
        {
            // Ignore worker teardown failures so the next request can retry startup.
        }
        catch (InvalidOperationException)
        {
            // Ignore worker teardown failures so the next request can retry startup.
        }
        catch (PlatformNotSupportedException)
        {
            // Ignore worker teardown failures so the next request can retry startup.
        }
        catch (NotSupportedException)
        {
            // Ignore worker teardown failures so the next request can retry startup.
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
