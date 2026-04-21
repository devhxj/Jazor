using Jolt.Lsp;

namespace Jolt.Extensions;

internal sealed class OutOfProcessExtensionProxy :
    IExtension,
    IExtensionCapabilityDescriptor,
    ILspDiagnosticProvider,
    ILspCodeActionProvider,
    ILspHoverProvider,
    ILspCompletionProvider,
    ILspDocumentSymbolProvider,
    ILspSignatureHelpProvider,
    ILspInlayHintProvider,
    ILspWorkspaceSymbolProvider,
    ILspFoldingRangeProvider,
    ILspReferenceProvider,
    ILspRenameProvider
{
    private const string BootstrapTimeoutEnvironmentVariable = "JOLT_EXTENSION_BOOTSTRAP_TIMEOUT_MS";
    private static readonly TimeSpan DefaultBootstrapTimeout = TimeSpan.FromSeconds(30);
    private readonly ExtensionWorkerBootstrapRequest _bootstrapRequest;
    private readonly IReadOnlyDictionary<string, ExtensionWorkerProviderDescriptor> _providerByCapability;
    private readonly IReadOnlySet<string> _providedCapabilities;
    private readonly SemaphoreSlim _workerRestartGate = new(1, 1);
    private readonly Lock _workerGate = new();

    private ExtensionWorkerClient? _workerClient;
    private int _deactivateInvoked;

    private OutOfProcessExtensionProxy(
        ExtensionWorkerClient workerClient,
        ExtensionMetadata metadata,
        ExtensionWorkerBootstrapRequest bootstrapRequest,
        IReadOnlyList<ExtensionWorkerProviderDescriptor> providers)
    {
        _workerClient = workerClient ?? throw new ArgumentNullException(nameof(workerClient));
        Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        _bootstrapRequest = bootstrapRequest ?? throw new ArgumentNullException(nameof(bootstrapRequest));

        var providerByCapability = new Dictionary<string, ExtensionWorkerProviderDescriptor>(StringComparer.OrdinalIgnoreCase);
        foreach (var provider in providers)
        {
            if (provider is null
                || string.IsNullOrWhiteSpace(provider.Capability)
                || string.IsNullOrWhiteSpace(provider.Name))
            {
                continue;
            }

            providerByCapability[provider.Capability.Trim()] = provider;
        }

        _providerByCapability = providerByCapability;
        _providedCapabilities = providerByCapability.Keys
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    public ExtensionMetadata Metadata { get; }

    public IReadOnlySet<string> ProvidedCapabilities => _providedCapabilities;

    string ILspDiagnosticProvider.Name => GetProviderName(
        ExtensionCapabilityNames.Diagnostic,
        fallback: $"{Metadata.Id}.diagnostic");

    int ILspDiagnosticProvider.Priority => GetPriority(ExtensionCapabilityNames.Diagnostic);

    string ILspCodeActionProvider.Name => GetProviderName(
        ExtensionCapabilityNames.CodeAction,
        fallback: $"{Metadata.Id}.codeAction");

    int ILspCodeActionProvider.Priority => GetPriority(ExtensionCapabilityNames.CodeAction);

    string ILspHoverProvider.Name => GetProviderName(
        ExtensionCapabilityNames.Hover,
        fallback: $"{Metadata.Id}.hover");

    int ILspHoverProvider.Priority => GetPriority(ExtensionCapabilityNames.Hover);

    string ILspCompletionProvider.Name => GetProviderName(
        ExtensionCapabilityNames.Completion,
        fallback: $"{Metadata.Id}.completion");

    int ILspCompletionProvider.Priority => GetPriority(ExtensionCapabilityNames.Completion);

    string ILspDocumentSymbolProvider.Name => GetProviderName(
        ExtensionCapabilityNames.DocumentSymbol,
        fallback: $"{Metadata.Id}.documentSymbol");

    int ILspDocumentSymbolProvider.Priority => GetPriority(ExtensionCapabilityNames.DocumentSymbol);

    string ILspSignatureHelpProvider.Name => GetProviderName(
        ExtensionCapabilityNames.SignatureHelp,
        fallback: $"{Metadata.Id}.signatureHelp");

    int ILspSignatureHelpProvider.Priority => GetPriority(ExtensionCapabilityNames.SignatureHelp);

    string ILspInlayHintProvider.Name => GetProviderName(
        ExtensionCapabilityNames.InlayHint,
        fallback: $"{Metadata.Id}.inlayHint");

    int ILspInlayHintProvider.Priority => GetPriority(ExtensionCapabilityNames.InlayHint);

    string ILspWorkspaceSymbolProvider.Name => GetProviderName(
        ExtensionCapabilityNames.WorkspaceSymbol,
        fallback: $"{Metadata.Id}.workspaceSymbol");

    int ILspWorkspaceSymbolProvider.Priority => GetPriority(ExtensionCapabilityNames.WorkspaceSymbol);

    string ILspFoldingRangeProvider.Name => GetProviderName(
        ExtensionCapabilityNames.FoldingRange,
        fallback: $"{Metadata.Id}.foldingRange");

    int ILspFoldingRangeProvider.Priority => GetPriority(ExtensionCapabilityNames.FoldingRange);

    string ILspReferenceProvider.Name => GetProviderName(
        ExtensionCapabilityNames.References,
        fallback: $"{Metadata.Id}.references");

    int ILspReferenceProvider.Priority => GetPriority(ExtensionCapabilityNames.References);

    string ILspRenameProvider.Name => GetProviderName(
        ExtensionCapabilityNames.Rename,
        fallback: $"{Metadata.Id}.rename");

    int ILspRenameProvider.Priority => GetPriority(ExtensionCapabilityNames.Rename);

    public static async ValueTask<OutOfProcessExtensionProxy> CreateAsync(
        string rootDirectory,
        string extensionDirectory,
        string assemblyPath,
        string extensionTypeName,
        ExtensionSandboxProfile sandboxProfile,
        IReadOnlyDictionary<string, string>? settings,
        CancellationToken cancellationToken)
    {
        var bootstrapRequest = new ExtensionWorkerBootstrapRequest(
            RootDirectory: rootDirectory,
            ExtensionDirectory: extensionDirectory,
            AssemblyPath: assemblyPath,
            ExtensionTypeName: extensionTypeName,
            Settings: settings,
            SandboxProfile: sandboxProfile);
        var (workerClient, bootstrap) = await CreateBootstrappedWorkerAsync(
            bootstrapRequest,
            cancellationToken);
        return new OutOfProcessExtensionProxy(
            workerClient,
            bootstrap.Metadata,
            bootstrapRequest,
            bootstrap.Providers);
    }

    ValueTask IExtension.InitializeAsync(ExtensionContext context, CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    ValueTask IExtension.ActivateAsync(CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    async ValueTask IExtension.DeactivateAsync(CancellationToken cancellationToken)
    {
        if (Interlocked.Exchange(ref _deactivateInvoked, 1) != 0)
        {
            return;
        }

        ExtensionWorkerClient? workerClient;
        lock (_workerGate)
        {
            workerClient = _workerClient;
            _workerClient = null;
        }

        if (workerClient is null)
        {
            return;
        }

        try
        {
            await workerClient.ShutdownAsync();
        }
        finally
        {
            await workerClient.DisposeAsync();
        }
    }

    async ValueTask<IReadOnlyList<LspDiagnostic>> ILspDiagnosticProvider.ProvideDiagnosticsAsync(
        LspDiagnosticProviderContext context,
        CancellationToken cancellationToken)
        => await InvokeOrDefaultAsync(
            ExtensionCapabilityNames.Diagnostic,
            context,
            defaultValue: Array.Empty<LspDiagnostic>(),
            cancellationToken);

    async ValueTask<IReadOnlyList<LspCodeAction>> ILspCodeActionProvider.ProvideCodeActionsAsync(
        LspCodeActionProviderContext context,
        CancellationToken cancellationToken)
        => await InvokeOrDefaultAsync(
            ExtensionCapabilityNames.CodeAction,
            context,
            defaultValue: Array.Empty<LspCodeAction>(),
            cancellationToken);

    async ValueTask<LspHoverResult?> ILspHoverProvider.ProvideHoverAsync(
        LspHoverProviderContext context,
        CancellationToken cancellationToken)
        => await InvokeOrDefaultAsync(
            ExtensionCapabilityNames.Hover,
            context,
            defaultValue: context.ExistingHover,
            cancellationToken);

    async ValueTask<IReadOnlyList<LspCompletionItem>> ILspCompletionProvider.ProvideCompletionItemsAsync(
        LspCompletionProviderContext context,
        CancellationToken cancellationToken)
        => await InvokeOrDefaultAsync(
            ExtensionCapabilityNames.Completion,
            context,
            defaultValue: Array.Empty<LspCompletionItem>(),
            cancellationToken);

    async ValueTask<IReadOnlyList<LspDocumentSymbol>> ILspDocumentSymbolProvider.ProvideDocumentSymbolsAsync(
        LspDocumentSymbolProviderContext context,
        CancellationToken cancellationToken)
        => await InvokeOrDefaultAsync(
            ExtensionCapabilityNames.DocumentSymbol,
            context,
            defaultValue: Array.Empty<LspDocumentSymbol>(),
            cancellationToken);

    async ValueTask<LspSignatureHelp?> ILspSignatureHelpProvider.ProvideSignatureHelpAsync(
        LspSignatureHelpProviderContext context,
        CancellationToken cancellationToken)
        => await InvokeOrDefaultAsync(
            ExtensionCapabilityNames.SignatureHelp,
            context,
            defaultValue: context.ExistingSignatureHelp,
            cancellationToken);

    async ValueTask<IReadOnlyList<LspInlayHint>> ILspInlayHintProvider.ProvideInlayHintsAsync(
        LspInlayHintProviderContext context,
        CancellationToken cancellationToken)
        => await InvokeOrDefaultAsync(
            ExtensionCapabilityNames.InlayHint,
            context,
            defaultValue: Array.Empty<LspInlayHint>(),
            cancellationToken);

    async ValueTask<IReadOnlyList<LspWorkspaceSymbol>> ILspWorkspaceSymbolProvider.ProvideWorkspaceSymbolsAsync(
        LspWorkspaceSymbolProviderContext context,
        CancellationToken cancellationToken)
        => await InvokeOrDefaultAsync(
            ExtensionCapabilityNames.WorkspaceSymbol,
            context,
            defaultValue: Array.Empty<LspWorkspaceSymbol>(),
            cancellationToken);

    async ValueTask<IReadOnlyList<LspFoldingRange>> ILspFoldingRangeProvider.ProvideFoldingRangesAsync(
        LspFoldingRangeProviderContext context,
        CancellationToken cancellationToken)
        => await InvokeOrDefaultAsync(
            ExtensionCapabilityNames.FoldingRange,
            context,
            defaultValue: Array.Empty<LspFoldingRange>(),
            cancellationToken);

    async ValueTask<IReadOnlyList<LspLocation>> ILspReferenceProvider.ProvideReferencesAsync(
        LspReferenceProviderContext context,
        CancellationToken cancellationToken)
        => await InvokeOrDefaultAsync(
            ExtensionCapabilityNames.References,
            context,
            defaultValue: Array.Empty<LspLocation>(),
            cancellationToken);

    async ValueTask<LspWorkspaceEdit?> ILspRenameProvider.ProvideRenameAsync(
        LspRenameProviderContext context,
        CancellationToken cancellationToken)
        => await InvokeOrDefaultAsync(
            ExtensionCapabilityNames.Rename,
            context,
            defaultValue: context.ExistingEdit,
            cancellationToken);

    private async ValueTask<TResult> InvokeOrDefaultAsync<TResult>(
        string capability,
        object context,
        TResult defaultValue,
        CancellationToken cancellationToken)
    {
        if (!_providedCapabilities.Contains(capability))
        {
            return defaultValue;
        }

        var invoked = await InvokeWithRecoveryAsync<TResult>(
            capability,
            context,
            cancellationToken);
        return invoked is null
            ? defaultValue
            : invoked;
    }

    private async ValueTask<TResult?> InvokeWithRecoveryAsync<TResult>(
        string capability,
        object context,
        CancellationToken cancellationToken)
    {
        var workerClient = GetWorkerClient();
        try
        {
            return await workerClient.InvokeAsync<TResult>(
                capability,
                context,
                cancellationToken);
        }
        catch (ExtensionWorkerConnectionException)
            when (Volatile.Read(ref _deactivateInvoked) == 0)
        {
            var replacement = await RestartWorkerAsync(workerClient, cancellationToken);
            return await replacement.InvokeAsync<TResult>(
                capability,
                context,
                cancellationToken);
        }
    }

    private async ValueTask<ExtensionWorkerClient> RestartWorkerAsync(
        ExtensionWorkerClient failedWorker,
        CancellationToken cancellationToken)
    {
        await _workerRestartGate.WaitAsync(cancellationToken);
        try
        {
            if (Volatile.Read(ref _deactivateInvoked) != 0)
            {
                throw new ObjectDisposedException(nameof(OutOfProcessExtensionProxy));
            }

            var currentWorker = GetWorkerClient();
            if (!ReferenceEquals(currentWorker, failedWorker))
            {
                return currentWorker;
            }

            var (replacementWorker, _) = await CreateBootstrappedWorkerAsync(
                _bootstrapRequest,
                cancellationToken);
            var replacementAdopted = false;
            lock (_workerGate)
            {
                if (_workerClient is not null
                    && ReferenceEquals(_workerClient, failedWorker)
                    && Volatile.Read(ref _deactivateInvoked) == 0)
                {
                    _workerClient = replacementWorker;
                    replacementAdopted = true;
                }
            }

            if (replacementAdopted)
            {
                await DisposeWorkerSilentlyAsync(failedWorker);
                return replacementWorker;
            }

            await DisposeWorkerSilentlyAsync(replacementWorker);
            return GetWorkerClient();
        }
        finally
        {
            _workerRestartGate.Release();
        }
    }

    private ExtensionWorkerClient GetWorkerClient()
    {
        lock (_workerGate)
        {
            return _workerClient
                ?? throw new ObjectDisposedException(nameof(OutOfProcessExtensionProxy));
        }
    }

    private static async ValueTask<(ExtensionWorkerClient WorkerClient, ExtensionWorkerBootstrapResponse Bootstrap)> CreateBootstrappedWorkerAsync(
        ExtensionWorkerBootstrapRequest bootstrapRequest,
        CancellationToken cancellationToken)
    {
        var workerClient = await ExtensionWorkerClient.StartAsync(cancellationToken);
        var bootstrapSucceeded = false;
        try
        {
            var timeout = ResolveBootstrapTimeout(
                BootstrapTimeoutEnvironmentVariable,
                DefaultBootstrapTimeout);
            using var bootstrapTimeout = CreateOperationTimeoutTokenSource(
                cancellationToken,
                timeout);
            ExtensionWorkerBootstrapResponse bootstrap;
            try
            {
                bootstrap = await workerClient.BootstrapAsync(bootstrapRequest, bootstrapTimeout.Token);
            }
            catch (OperationCanceledException exception)
                when (!cancellationToken.IsCancellationRequested && bootstrapTimeout.IsCancellationRequested)
            {
                throw new TimeoutException(
                    $"extension worker bootstrap timed out after {timeout.TotalSeconds:0.###} seconds.",
                    exception);
            }

            bootstrapSucceeded = true;
            return (workerClient, bootstrap);
        }
        finally
        {
            if (!bootstrapSucceeded)
            {
                await DisposeWorkerSilentlyAsync(workerClient);
            }
        }
    }

    private static async ValueTask DisposeWorkerSilentlyAsync(ExtensionWorkerClient workerClient)
    {
        try
        {
            await workerClient.DisposeAsync();
        }
        catch (ObjectDisposedException)
        {
            // Ignore disposal failures while recovering from worker crashes.
        }
        catch (IOException)
        {
            // Ignore disposal failures while recovering from worker crashes.
        }
        catch (InvalidOperationException)
        {
            // Ignore disposal failures while recovering from worker crashes.
        }
    }

    private static CancellationTokenSource CreateOperationTimeoutTokenSource(
        CancellationToken cancellationToken,
        TimeSpan timeout)
    {
        var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        linkedSource.CancelAfter(timeout);
        return linkedSource;
    }

    private static TimeSpan ResolveBootstrapTimeout(string environmentVariableName, TimeSpan defaultTimeout)
    {
        var configuredValue = Environment.GetEnvironmentVariable(environmentVariableName);
        return int.TryParse(configuredValue, out var milliseconds) && milliseconds > 0
            ? TimeSpan.FromMilliseconds(milliseconds)
            : defaultTimeout;
    }

    private string GetProviderName(string capability, string fallback)
    {
        return _providerByCapability.TryGetValue(capability, out var descriptor)
            ? descriptor.Name
            : fallback;
    }

    private int GetPriority(string capability)
    {
        return _providerByCapability.TryGetValue(capability, out var descriptor)
            ? descriptor.Priority
            : 0;
    }
}
