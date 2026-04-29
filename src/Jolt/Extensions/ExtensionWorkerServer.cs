using System.Text.Json;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Jolt.Lsp;
using ECMAScript.Contract.VueContracts.Protocol;

namespace Jolt.Extensions;

/// <summary>
/// Hosts process-isolated extensions and applies Jolt-mediated IO/network policy to provider requests.
/// These checks do not sandbox arbitrary extension code outside the request/response surfaces Jolt validates.
/// </summary>
internal sealed class ExtensionWorkerServer
{
    private const string InvokeTimeoutEnvironmentVariable = "JOLT_EXTENSION_INVOKE_TIMEOUT_MS";
    private static readonly TimeSpan DefaultInvokeTimeout = TimeSpan.FromSeconds(30);
    private static readonly string[] NetworkUriSchemes =
    [
        "http",
        "https",
        "ws",
        "wss"
    ];

    private IExtension? _extension;
    private CollectibleExtensionLoadContext? _loadContext;
    private IReadOnlyList<ExtensionWorkerProviderDescriptor> _providerDescriptors = Array.Empty<ExtensionWorkerProviderDescriptor>();
    private ExtensionSandboxProfile _sandboxProfile = ExtensionSandboxProfile.Unrestricted;
    private TimeSpan _invokeTimeout = DefaultInvokeTimeout;
    private bool _bootstrapped;

    public async ValueTask RunAsync(
        Stream input,
        Stream output,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(output);

        var reader = new LspMessageReader(input);
        var writer = new LspMessageWriter(output);

        try
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var requestJson = await reader.ReadMessageAsync(cancellationToken);
                if (requestJson is null)
                {
                    break;
                }

                var (response, shouldExit) = await HandleMessageSafelyAsync(requestJson, cancellationToken);
                await writer.WriteMessageAsync(LspJsonSerializer.Serialize(response), cancellationToken);

                if (shouldExit)
                {
                    break;
                }
            }
        }
        finally
        {
            await ShutdownCoreAsync(CancellationToken.None);
        }
    }

    private async ValueTask<(ExtensionWorkerResponseEnvelope Response, bool ShouldExit)> HandleMessageSafelyAsync(
        string requestJson,
        CancellationToken cancellationToken)
    {
        var requestId = 0;
        try
        {
            var request = LspJsonSerializer.Deserialize<ExtensionWorkerRequestEnvelope>(requestJson)
                ?? throw new ExtensionWorkerProtocolException(
                    ExtensionWorkerErrorCodes.InvalidRequest,
                    "request payload is invalid.");
            requestId = request.Id;
            return await HandleRequestAsync(request, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (ExtensionWorkerProtocolException protocolException)
        {
            return (
                new ExtensionWorkerResponseEnvelope(
                    Id: requestId,
                    Result: null,
                    Error: new ExtensionWorkerError(protocolException.Code, protocolException.Message)),
                false);
        }
        catch (Exception exception)
        {
            return (
                new ExtensionWorkerResponseEnvelope(
                    Id: requestId,
                    Result: null,
                    Error: new ExtensionWorkerError(
                        ExtensionWorkerErrorCodes.InternalError,
                        exception.Message)),
                false);
        }
    }

    private async ValueTask<(ExtensionWorkerResponseEnvelope Response, bool ShouldExit)> HandleRequestAsync(
        ExtensionWorkerRequestEnvelope request,
        CancellationToken cancellationToken)
    {
        if (request.Id <= 0)
        {
            throw new ExtensionWorkerProtocolException(
                ExtensionWorkerErrorCodes.InvalidRequest,
                "request id must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(request.Method))
        {
            throw new ExtensionWorkerProtocolException(
                ExtensionWorkerErrorCodes.InvalidRequest,
                "request method is required.");
        }

        switch (request.Method)
        {
            case ExtensionWorkerMethodNames.Bootstrap:
            {
                var bootstrapRequest = DeserializeRequired<ExtensionWorkerBootstrapRequest>(
                    request.Params,
                    "bootstrap params");
                var bootstrapResponse = await HandleBootstrapAsync(bootstrapRequest, cancellationToken);
                return (new ExtensionWorkerResponseEnvelope(request.Id, bootstrapResponse, null), false);
            }

            case ExtensionWorkerMethodNames.Invoke:
            {
                var invokeRequest = DeserializeRequired<ExtensionWorkerInvokeRequest>(
                    request.Params,
                    "invoke params");
                using var invokeTimeoutSource = CreateOperationTimeoutTokenSource(cancellationToken, _invokeTimeout);
                object? invokeResult;
                try
                {
                    invokeResult = await HandleInvokeAsync(invokeRequest, invokeTimeoutSource.Token);
                }
                catch (OperationCanceledException exception)
                    when (!cancellationToken.IsCancellationRequested && invokeTimeoutSource.IsCancellationRequested)
                {
                    throw new ExtensionWorkerProtocolException(
                        ExtensionWorkerErrorCodes.InternalError,
                        $"extension capability '{invokeRequest.Capability}' timed out after {_invokeTimeout.TotalSeconds:0.###} seconds.",
                        exception);
                }

                return (new ExtensionWorkerResponseEnvelope(request.Id, invokeResult, null), false);
            }

            case ExtensionWorkerMethodNames.Shutdown:
            {
                await ShutdownCoreAsync(cancellationToken);
                return (new ExtensionWorkerResponseEnvelope(request.Id, Result: null, Error: null), true);
            }

            default:
                throw new ExtensionWorkerProtocolException(
                    ExtensionWorkerErrorCodes.UnsupportedMethod,
                    $"unsupported worker method '{request.Method}'.");
        }
    }

    private async ValueTask<ExtensionWorkerBootstrapResponse> HandleBootstrapAsync(
        ExtensionWorkerBootstrapRequest request,
        CancellationToken cancellationToken)
    {
        if (_bootstrapped && _extension is not null)
        {
            return new ExtensionWorkerBootstrapResponse(
                Metadata: _extension.Metadata,
                Providers: _providerDescriptors);
        }

        if (string.IsNullOrWhiteSpace(request.RootDirectory))
        {
            throw new ExtensionWorkerProtocolException(
                ExtensionWorkerErrorCodes.InvalidParams,
                "bootstrap rootDirectory is required.");
        }

        if (string.IsNullOrWhiteSpace(request.ExtensionDirectory))
        {
            throw new ExtensionWorkerProtocolException(
                ExtensionWorkerErrorCodes.InvalidParams,
                "bootstrap extensionDirectory is required.");
        }

        if (string.IsNullOrWhiteSpace(request.AssemblyPath))
        {
            throw new ExtensionWorkerProtocolException(
                ExtensionWorkerErrorCodes.InvalidParams,
                "bootstrap assemblyPath is required.");
        }

        if (string.IsNullOrWhiteSpace(request.ExtensionTypeName))
        {
            throw new ExtensionWorkerProtocolException(
                ExtensionWorkerErrorCodes.InvalidParams,
                "bootstrap extensionTypeName is required.");
        }

        var normalizedRoot = Path.GetFullPath(request.RootDirectory);
        var normalizedExtensionDirectory = Path.GetFullPath(request.ExtensionDirectory);
        var normalizedAssemblyPath = Path.GetFullPath(request.AssemblyPath);
        if (!File.Exists(normalizedAssemblyPath))
        {
            throw new ExtensionWorkerProtocolException(
                ExtensionWorkerErrorCodes.InvalidParams,
                $"extension assembly '{normalizedAssemblyPath}' does not exist.");
        }

        var (extension, loadContext) = CreateExtension(normalizedAssemblyPath, request.ExtensionTypeName);
        var bootstrapped = false;
        try
        {
            var sandboxProfile = request.SandboxProfile ?? ExtensionSandboxProfile.Unrestricted;
            _invokeTimeout = ExtensionWorkerHostSettingResolver.ResolvePositiveDurationFromMilliseconds(
                request.Settings,
                ExtensionWorkerHostSettingNames.InvokeTimeoutMilliseconds,
                InvokeTimeoutEnvironmentVariable,
                DefaultInvokeTimeout);
            var context = new ExtensionContext(
                rootDirectory: normalizedRoot,
                extensionDirectory: normalizedExtensionDirectory,
                registry: NullExtensionRegistry.Instance,
                settings: request.Settings,
                sandboxProfile: sandboxProfile);
            await extension.InitializeAsync(context, cancellationToken);
            await extension.ActivateAsync(cancellationToken);

            _extension = extension;
            _loadContext = loadContext;
            _providerDescriptors = DescribeProviders(extension);
            _sandboxProfile = sandboxProfile;
            _bootstrapped = true;
            bootstrapped = true;

            return new ExtensionWorkerBootstrapResponse(
                Metadata: extension.Metadata,
                Providers: _providerDescriptors);
        }
        finally
        {
            if (!bootstrapped)
            {
                await TryDeactivateSilentlyAsync(extension);
                loadContext.Unload();
            }
        }
    }

    private async ValueTask<object?> HandleInvokeAsync(
        ExtensionWorkerInvokeRequest request,
        CancellationToken cancellationToken)
    {
        if (!_bootstrapped || _extension is null)
        {
            throw new ExtensionWorkerProtocolException(
                ExtensionWorkerErrorCodes.NotBootstrapped,
                "worker extension is not bootstrapped.");
        }

        if (string.IsNullOrWhiteSpace(request.Capability))
        {
            throw new ExtensionWorkerProtocolException(
                ExtensionWorkerErrorCodes.InvalidParams,
                "invoke capability is required.");
        }

        return request.Capability.Trim() switch
        {
            ExtensionCapabilityNames.Diagnostic => await InvokeDiagnosticAsync(_extension, request.Context, _sandboxProfile, cancellationToken),
            ExtensionCapabilityNames.CodeAction => await InvokeCodeActionAsync(_extension, request.Context, _sandboxProfile, cancellationToken),
            ExtensionCapabilityNames.Hover => await InvokeHoverAsync(_extension, request.Context, _sandboxProfile, cancellationToken),
            ExtensionCapabilityNames.Completion => await InvokeCompletionAsync(_extension, request.Context, _sandboxProfile, cancellationToken),
            ExtensionCapabilityNames.DocumentSymbol => await InvokeDocumentSymbolAsync(_extension, request.Context, _sandboxProfile, cancellationToken),
            ExtensionCapabilityNames.SignatureHelp => await InvokeSignatureHelpAsync(_extension, request.Context, _sandboxProfile, cancellationToken),
            ExtensionCapabilityNames.InlayHint => await InvokeInlayHintAsync(_extension, request.Context, _sandboxProfile, cancellationToken),
            ExtensionCapabilityNames.WorkspaceSymbol => await InvokeWorkspaceSymbolAsync(_extension, request.Context, _sandboxProfile, cancellationToken),
            ExtensionCapabilityNames.FoldingRange => await InvokeFoldingRangeAsync(_extension, request.Context, _sandboxProfile, cancellationToken),
            ExtensionCapabilityNames.References => await InvokeReferencesAsync(_extension, request.Context, _sandboxProfile, cancellationToken),
            ExtensionCapabilityNames.Rename => await InvokeRenameAsync(_extension, request.Context, _sandboxProfile, cancellationToken),
            _ => throw new ExtensionWorkerProtocolException(
                ExtensionWorkerErrorCodes.UnsupportedCapability,
                $"unsupported capability '{request.Capability}'.")
        };
    }

    private static async ValueTask<IReadOnlyList<LspDiagnostic>> InvokeDiagnosticAsync(
        IExtension extension,
        object? context,
        ExtensionSandboxProfile sandboxProfile,
        CancellationToken cancellationToken)
    {
        if (extension is not ILspDiagnosticProvider provider)
        {
            throw new ExtensionWorkerProtocolException(
                ExtensionWorkerErrorCodes.ProviderNotImplemented,
                $"extension does not implement provider capability '{ExtensionCapabilityNames.Diagnostic}'.");
        }

        var typedContext = DeserializeRequired<LspDiagnosticProviderContext>(context, "diagnostic context");
        EnsureReadPathAllowed(sandboxProfile, ExtensionCapabilityNames.Diagnostic, typedContext.Document);
        return await provider.ProvideDiagnosticsAsync(typedContext, cancellationToken);
    }

    private static async ValueTask<IReadOnlyList<LspCodeAction>> InvokeCodeActionAsync(
        IExtension extension,
        object? context,
        ExtensionSandboxProfile sandboxProfile,
        CancellationToken cancellationToken)
    {
        if (extension is not ILspCodeActionProvider provider)
        {
            throw new ExtensionWorkerProtocolException(
                ExtensionWorkerErrorCodes.ProviderNotImplemented,
                $"extension does not implement provider capability '{ExtensionCapabilityNames.CodeAction}'.");
        }

        var typedContext = DeserializeRequired<LspCodeActionProviderContext>(context, "codeAction context");
        EnsureReadPathAllowed(sandboxProfile, ExtensionCapabilityNames.CodeAction, typedContext.Document);
        EnsureNetworkUrisAllowedForCodeActions(
            sandboxProfile,
            ExtensionCapabilityNames.CodeAction,
            typedContext.ExistingActions,
            payloadKind: "context");

        var actions = await provider.ProvideCodeActionsAsync(typedContext, cancellationToken);
        EnsureWritePathsAllowedForCodeActions(
            sandboxProfile,
            ExtensionCapabilityNames.CodeAction,
            actions);
        EnsureNetworkUrisAllowedForCodeActions(
            sandboxProfile,
            ExtensionCapabilityNames.CodeAction,
            actions,
            payloadKind: "result");
        return actions;
    }

    private static async ValueTask<LspHoverResult?> InvokeHoverAsync(
        IExtension extension,
        object? context,
        ExtensionSandboxProfile sandboxProfile,
        CancellationToken cancellationToken)
    {
        if (extension is not ILspHoverProvider provider)
        {
            throw new ExtensionWorkerProtocolException(
                ExtensionWorkerErrorCodes.ProviderNotImplemented,
                $"extension does not implement provider capability '{ExtensionCapabilityNames.Hover}'.");
        }

        var typedContext = DeserializeRequired<LspHoverProviderContext>(context, "hover context");
        EnsureReadPathAllowed(sandboxProfile, ExtensionCapabilityNames.Hover, typedContext.Document);
        return await provider.ProvideHoverAsync(typedContext, cancellationToken);
    }

    private static async ValueTask<IReadOnlyList<LspCompletionItem>> InvokeCompletionAsync(
        IExtension extension,
        object? context,
        ExtensionSandboxProfile sandboxProfile,
        CancellationToken cancellationToken)
    {
        if (extension is not ILspCompletionProvider provider)
        {
            throw new ExtensionWorkerProtocolException(
                ExtensionWorkerErrorCodes.ProviderNotImplemented,
                $"extension does not implement provider capability '{ExtensionCapabilityNames.Completion}'.");
        }

        var typedContext = DeserializeRequired<LspCompletionProviderContext>(context, "completion context");
        EnsureReadPathAllowed(sandboxProfile, ExtensionCapabilityNames.Completion, typedContext.Document);
        return await provider.ProvideCompletionItemsAsync(typedContext, cancellationToken);
    }

    private static async ValueTask<IReadOnlyList<LspDocumentSymbol>> InvokeDocumentSymbolAsync(
        IExtension extension,
        object? context,
        ExtensionSandboxProfile sandboxProfile,
        CancellationToken cancellationToken)
    {
        if (extension is not ILspDocumentSymbolProvider provider)
        {
            throw new ExtensionWorkerProtocolException(
                ExtensionWorkerErrorCodes.ProviderNotImplemented,
                $"extension does not implement provider capability '{ExtensionCapabilityNames.DocumentSymbol}'.");
        }

        var typedContext = DeserializeRequired<LspDocumentSymbolProviderContext>(context, "documentSymbol context");
        EnsureReadPathAllowed(sandboxProfile, ExtensionCapabilityNames.DocumentSymbol, typedContext.Document);
        return await provider.ProvideDocumentSymbolsAsync(typedContext, cancellationToken);
    }

    private static async ValueTask<LspSignatureHelp?> InvokeSignatureHelpAsync(
        IExtension extension,
        object? context,
        ExtensionSandboxProfile sandboxProfile,
        CancellationToken cancellationToken)
    {
        if (extension is not ILspSignatureHelpProvider provider)
        {
            throw new ExtensionWorkerProtocolException(
                ExtensionWorkerErrorCodes.ProviderNotImplemented,
                $"extension does not implement provider capability '{ExtensionCapabilityNames.SignatureHelp}'.");
        }

        var typedContext = DeserializeRequired<LspSignatureHelpProviderContext>(context, "signatureHelp context");
        EnsureReadPathAllowed(sandboxProfile, ExtensionCapabilityNames.SignatureHelp, typedContext.Document);
        return await provider.ProvideSignatureHelpAsync(typedContext, cancellationToken);
    }

    private static async ValueTask<IReadOnlyList<LspInlayHint>> InvokeInlayHintAsync(
        IExtension extension,
        object? context,
        ExtensionSandboxProfile sandboxProfile,
        CancellationToken cancellationToken)
    {
        if (extension is not ILspInlayHintProvider provider)
        {
            throw new ExtensionWorkerProtocolException(
                ExtensionWorkerErrorCodes.ProviderNotImplemented,
                $"extension does not implement provider capability '{ExtensionCapabilityNames.InlayHint}'.");
        }

        var typedContext = DeserializeRequired<LspInlayHintProviderContext>(context, "inlayHint context");
        EnsureReadPathAllowed(sandboxProfile, ExtensionCapabilityNames.InlayHint, typedContext.Document);
        return await provider.ProvideInlayHintsAsync(typedContext, cancellationToken);
    }

    private static async ValueTask<IReadOnlyList<LspWorkspaceSymbol>> InvokeWorkspaceSymbolAsync(
        IExtension extension,
        object? context,
        ExtensionSandboxProfile sandboxProfile,
        CancellationToken cancellationToken)
    {
        if (extension is not ILspWorkspaceSymbolProvider provider)
        {
            throw new ExtensionWorkerProtocolException(
                ExtensionWorkerErrorCodes.ProviderNotImplemented,
                $"extension does not implement provider capability '{ExtensionCapabilityNames.WorkspaceSymbol}'.");
        }

        var typedContext = DeserializeRequired<LspWorkspaceSymbolProviderContext>(context, "workspaceSymbol context");
        EnsureReadPathAllowed(sandboxProfile, ExtensionCapabilityNames.WorkspaceSymbol, typedContext.OpenDocuments);
        EnsureNetworkUrisAllowedForWorkspaceSymbols(
            sandboxProfile,
            ExtensionCapabilityNames.WorkspaceSymbol,
            typedContext.ExistingSymbols,
            payloadKind: "context");

        var symbols = await provider.ProvideWorkspaceSymbolsAsync(typedContext, cancellationToken);
        EnsureNetworkUrisAllowedForWorkspaceSymbols(
            sandboxProfile,
            ExtensionCapabilityNames.WorkspaceSymbol,
            symbols,
            payloadKind: "result");
        return symbols;
    }

    private static async ValueTask<IReadOnlyList<LspFoldingRange>> InvokeFoldingRangeAsync(
        IExtension extension,
        object? context,
        ExtensionSandboxProfile sandboxProfile,
        CancellationToken cancellationToken)
    {
        if (extension is not ILspFoldingRangeProvider provider)
        {
            throw new ExtensionWorkerProtocolException(
                ExtensionWorkerErrorCodes.ProviderNotImplemented,
                $"extension does not implement provider capability '{ExtensionCapabilityNames.FoldingRange}'.");
        }

        var typedContext = DeserializeRequired<LspFoldingRangeProviderContext>(context, "foldingRange context");
        EnsureReadPathAllowed(sandboxProfile, ExtensionCapabilityNames.FoldingRange, typedContext.Document);
        return await provider.ProvideFoldingRangesAsync(typedContext, cancellationToken);
    }

    private static async ValueTask<IReadOnlyList<LspLocation>> InvokeReferencesAsync(
        IExtension extension,
        object? context,
        ExtensionSandboxProfile sandboxProfile,
        CancellationToken cancellationToken)
    {
        if (extension is not ILspReferenceProvider provider)
        {
            throw new ExtensionWorkerProtocolException(
                ExtensionWorkerErrorCodes.ProviderNotImplemented,
                $"extension does not implement provider capability '{ExtensionCapabilityNames.References}'.");
        }

        var typedContext = DeserializeRequired<LspReferenceProviderContext>(context, "references context");
        EnsureReadPathAllowed(sandboxProfile, ExtensionCapabilityNames.References, typedContext.Document);
        EnsureNetworkUrisAllowedForLocations(
            sandboxProfile,
            ExtensionCapabilityNames.References,
            typedContext.ExistingLocations,
            payloadKind: "context");

        var locations = await provider.ProvideReferencesAsync(typedContext, cancellationToken);
        EnsureNetworkUrisAllowedForLocations(
            sandboxProfile,
            ExtensionCapabilityNames.References,
            locations,
            payloadKind: "result");
        return locations;
    }

    private static async ValueTask<LspWorkspaceEdit?> InvokeRenameAsync(
        IExtension extension,
        object? context,
        ExtensionSandboxProfile sandboxProfile,
        CancellationToken cancellationToken)
    {
        if (extension is not ILspRenameProvider provider)
        {
            throw new ExtensionWorkerProtocolException(
                ExtensionWorkerErrorCodes.ProviderNotImplemented,
                $"extension does not implement provider capability '{ExtensionCapabilityNames.Rename}'.");
        }

        var typedContext = DeserializeRequired<LspRenameProviderContext>(context, "rename context");
        EnsureReadPathAllowed(sandboxProfile, ExtensionCapabilityNames.Rename, typedContext.Document);
        EnsureNetworkUrisAllowedForWorkspaceEdit(
            sandboxProfile,
            ExtensionCapabilityNames.Rename,
            typedContext.ExistingEdit,
            payloadKind: "context");

        var edit = await provider.ProvideRenameAsync(typedContext, cancellationToken);
        EnsureWritePathsAllowedForWorkspaceEdit(
            sandboxProfile,
            ExtensionCapabilityNames.Rename,
            edit);
        EnsureNetworkUrisAllowedForWorkspaceEdit(
            sandboxProfile,
            ExtensionCapabilityNames.Rename,
            edit,
            payloadKind: "result");
        return edit;
    }

    private static void EnsureReadPathAllowed(
        ExtensionSandboxProfile sandboxProfile,
        string capability,
        DocumentSnapshot document)
    {
        var documentPath = NormalizeDocumentPathForSandbox(document, capability);
        if (sandboxProfile.IsReadPathAllowed(documentPath))
        {
            return;
        }

        throw new ExtensionWorkerProtocolException(
            ExtensionWorkerErrorCodes.SandboxViolation,
            $"sandbox io read denied for capability '{capability}' path '{documentPath}'.");
    }

    private static void EnsureReadPathAllowed(
        ExtensionSandboxProfile sandboxProfile,
        string capability,
        IReadOnlyList<DocumentSnapshot> documents)
    {
        if (documents is null)
        {
            throw new ExtensionWorkerProtocolException(
                ExtensionWorkerErrorCodes.SandboxViolation,
                $"sandbox io read denied for capability '{capability}': open document set is missing.");
        }

        foreach (var document in documents)
        {
            EnsureReadPathAllowed(sandboxProfile, capability, document);
        }
    }

    private static void EnsureWritePathsAllowedForCodeActions(
        ExtensionSandboxProfile sandboxProfile,
        string capability,
        IReadOnlyList<LspCodeAction>? actions)
    {
        if (actions is null)
        {
            return;
        }

        foreach (var action in actions)
        {
            if (action?.Edit is null)
            {
                continue;
            }

            EnsureWritePathsAllowedForWorkspaceEdit(sandboxProfile, capability, action.Edit);
        }
    }

    private static void EnsureWritePathsAllowedForWorkspaceEdit(
        ExtensionSandboxProfile sandboxProfile,
        string capability,
        LspWorkspaceEdit? edit)
    {
        if (edit?.Changes is null)
        {
            return;
        }

        foreach (var change in edit.Changes)
        {
            var writePath = ResolveWorkspaceEditWritePath(change.Key, capability);
            if (sandboxProfile.IsWritePathAllowed(writePath))
            {
                continue;
            }

            throw new ExtensionWorkerProtocolException(
                ExtensionWorkerErrorCodes.SandboxViolation,
                $"sandbox io write denied for capability '{capability}' path '{writePath}'.");
        }
    }

    private static string ResolveWorkspaceEditWritePath(string targetUriOrPath, string capability)
    {
        if (string.IsNullOrWhiteSpace(targetUriOrPath))
        {
            throw new ExtensionWorkerProtocolException(
                ExtensionWorkerErrorCodes.SandboxViolation,
                $"sandbox io write denied for capability '{capability}': workspace edit target is missing.");
        }

        var trimmed = targetUriOrPath.Trim();
        if (Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
        {
            if (!uri.IsFile)
            {
                throw new ExtensionWorkerProtocolException(
                    ExtensionWorkerErrorCodes.SandboxViolation,
                    $"sandbox io write denied for capability '{capability}': unsupported workspace edit uri scheme '{uri.Scheme}'.");
            }

            try
            {
                return Path.GetFullPath(uri.LocalPath);
            }
            catch (ArgumentException)
            {
                throw new ExtensionWorkerProtocolException(
                    ExtensionWorkerErrorCodes.SandboxViolation,
                    $"sandbox io write denied for capability '{capability}': invalid workspace edit uri '{trimmed}'.");
            }
            catch (NotSupportedException)
            {
                throw new ExtensionWorkerProtocolException(
                    ExtensionWorkerErrorCodes.SandboxViolation,
                    $"sandbox io write denied for capability '{capability}': invalid workspace edit uri '{trimmed}'.");
            }
            catch (PathTooLongException)
            {
                throw new ExtensionWorkerProtocolException(
                    ExtensionWorkerErrorCodes.SandboxViolation,
                    $"sandbox io write denied for capability '{capability}': invalid workspace edit uri '{trimmed}'.");
            }
        }

        try
        {
            return Path.GetFullPath(trimmed);
        }
        catch (ArgumentException)
        {
            throw new ExtensionWorkerProtocolException(
                ExtensionWorkerErrorCodes.SandboxViolation,
                $"sandbox io write denied for capability '{capability}': invalid workspace edit path '{trimmed}'.");
        }
        catch (NotSupportedException)
        {
            throw new ExtensionWorkerProtocolException(
                ExtensionWorkerErrorCodes.SandboxViolation,
                $"sandbox io write denied for capability '{capability}': invalid workspace edit path '{trimmed}'.");
        }
        catch (PathTooLongException)
        {
            throw new ExtensionWorkerProtocolException(
                ExtensionWorkerErrorCodes.SandboxViolation,
                $"sandbox io write denied for capability '{capability}': invalid workspace edit path '{trimmed}'.");
        }
    }

    private static void EnsureNetworkUrisAllowedForCodeActions(
        ExtensionSandboxProfile sandboxProfile,
        string capability,
        IReadOnlyList<LspCodeAction>? actions,
        string payloadKind)
    {
        if (actions is null)
        {
            return;
        }

        foreach (var action in actions)
        {
            if (action?.Edit is null)
            {
                continue;
            }

            EnsureNetworkUrisAllowedForWorkspaceEdit(
                sandboxProfile,
                capability,
                action.Edit,
                payloadKind);
        }
    }

    private static void EnsureNetworkUrisAllowedForWorkspaceSymbols(
        ExtensionSandboxProfile sandboxProfile,
        string capability,
        IReadOnlyList<LspWorkspaceSymbol>? symbols,
        string payloadKind)
    {
        if (symbols is null)
        {
            return;
        }

        foreach (var symbol in symbols)
        {
            EnsureNetworkHostAllowedForUriValue(
                sandboxProfile,
                capability,
                symbol?.Location?.Uri,
                payloadKind);
        }
    }

    private static void EnsureNetworkUrisAllowedForLocations(
        ExtensionSandboxProfile sandboxProfile,
        string capability,
        IReadOnlyList<LspLocation>? locations,
        string payloadKind)
    {
        if (locations is null)
        {
            return;
        }

        foreach (var location in locations)
        {
            EnsureNetworkHostAllowedForUriValue(
                sandboxProfile,
                capability,
                location?.Uri,
                payloadKind);
        }
    }

    private static void EnsureNetworkUrisAllowedForWorkspaceEdit(
        ExtensionSandboxProfile sandboxProfile,
        string capability,
        LspWorkspaceEdit? edit,
        string payloadKind)
    {
        if (edit?.Changes is null)
        {
            return;
        }

        foreach (var change in edit.Changes)
        {
            EnsureNetworkHostAllowedForUriValue(
                sandboxProfile,
                capability,
                change.Key,
                payloadKind);
        }
    }

    private static void EnsureNetworkHostAllowedForUriValue(
        ExtensionSandboxProfile sandboxProfile,
        string capability,
        string? uriValue,
        string payloadKind)
    {
        if (!TryParseNetworkUri(uriValue, out var uri))
        {
            return;
        }

        if (sandboxProfile.IsNetworkHostAllowed(uri.Host))
        {
            return;
        }

        throw new ExtensionWorkerProtocolException(
            ExtensionWorkerErrorCodes.SandboxViolation,
            $"sandbox network denied for capability '{capability}' {payloadKind} uri '{uri.AbsoluteUri}'.");
    }

    private static bool TryParseNetworkUri(string? value, [NotNullWhen(true)] out Uri? uri)
    {
        uri = null;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (!Uri.TryCreate(value.Trim(), UriKind.Absolute, out var parsed))
        {
            return false;
        }

        if (!NetworkUriSchemes.Contains(parsed.Scheme, StringComparer.OrdinalIgnoreCase))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(parsed.Host))
        {
            return false;
        }

        uri = parsed;
        return true;
    }

    private static string NormalizeDocumentPathForSandbox(DocumentSnapshot document, string capability)
    {
        if (document is null || string.IsNullOrWhiteSpace(document.DocumentPath))
        {
            throw new ExtensionWorkerProtocolException(
                ExtensionWorkerErrorCodes.SandboxViolation,
                $"sandbox io read denied for capability '{capability}': document path is missing.");
        }

        try
        {
            return Path.GetFullPath(document.DocumentPath);
        }
        catch (ArgumentException)
        {
            throw new ExtensionWorkerProtocolException(
                ExtensionWorkerErrorCodes.SandboxViolation,
                $"sandbox io read denied for capability '{capability}': invalid document path '{document.DocumentPath}'.");
        }
        catch (NotSupportedException)
        {
            throw new ExtensionWorkerProtocolException(
                ExtensionWorkerErrorCodes.SandboxViolation,
                $"sandbox io read denied for capability '{capability}': invalid document path '{document.DocumentPath}'.");
        }
        catch (PathTooLongException)
        {
            throw new ExtensionWorkerProtocolException(
                ExtensionWorkerErrorCodes.SandboxViolation,
                $"sandbox io read denied for capability '{capability}': invalid document path '{document.DocumentPath}'.");
        }
    }

    private async ValueTask ShutdownCoreAsync(CancellationToken cancellationToken)
    {
        var extension = _extension;
        var loadContext = _loadContext;

        _extension = null;
        _loadContext = null;
        _providerDescriptors = Array.Empty<ExtensionWorkerProviderDescriptor>();
        _sandboxProfile = ExtensionSandboxProfile.Unrestricted;
        _bootstrapped = false;

        if (extension is not null)
        {
            try
            {
                await extension.DeactivateAsync(cancellationToken);
            }
            catch (Exception) {
                // Best-effort shutdown.
            }
        }

        if (loadContext is not null)
        {
            loadContext.Unload();
        }
    }

    private static (IExtension Extension, CollectibleExtensionLoadContext LoadContext) CreateExtension(
        string assemblyPath,
        string extensionTypeName)
    {
        if (string.IsNullOrWhiteSpace(assemblyPath) || string.IsNullOrWhiteSpace(extensionTypeName))
        {
            throw new ExtensionWorkerProtocolException(
                ExtensionWorkerErrorCodes.InvalidParams,
                "assembly path and extension type name are required.");
        }

        CollectibleExtensionLoadContext? candidateContext = null;
        try
        {
            candidateContext = new CollectibleExtensionLoadContext(assemblyPath);
            var assembly = candidateContext.LoadMainAssembly(assemblyPath);
            var extensionType = assembly.GetType(extensionTypeName, throwOnError: false, ignoreCase: false);
            if (extensionType is null)
            {
                candidateContext.Unload();
                throw new ExtensionWorkerProtocolException(
                    ExtensionWorkerErrorCodes.InvalidParams,
                    $"extension type '{extensionTypeName}' was not found.");
            }

            if (!typeof(IExtension).IsAssignableFrom(extensionType))
            {
                candidateContext.Unload();
                throw new ExtensionWorkerProtocolException(
                    ExtensionWorkerErrorCodes.InvalidParams,
                    $"extension type '{extensionTypeName}' does not implement IExtension.");
            }

            if (Activator.CreateInstance(extensionType) is not IExtension extension)
            {
                candidateContext.Unload();
                throw new ExtensionWorkerProtocolException(
                    ExtensionWorkerErrorCodes.InvalidParams,
                    $"extension type '{extensionTypeName}' cannot be instantiated.");
            }

            return (extension, candidateContext);
        }
        catch (ExtensionWorkerProtocolException)
        {
            throw;
        }
        catch (FileNotFoundException exception)
        {
            candidateContext?.Unload();
            throw new ExtensionWorkerProtocolException(
                ExtensionWorkerErrorCodes.InternalError,
                $"failed to load extension assembly: {exception.Message}");
        }
        catch (FileLoadException exception)
        {
            candidateContext?.Unload();
            throw new ExtensionWorkerProtocolException(
                ExtensionWorkerErrorCodes.InternalError,
                $"failed to load extension assembly: {exception.Message}");
        }
        catch (BadImageFormatException exception)
        {
            candidateContext?.Unload();
            throw new ExtensionWorkerProtocolException(
                ExtensionWorkerErrorCodes.InternalError,
                $"failed to load extension assembly: {exception.Message}");
        }
        catch (TypeLoadException exception)
        {
            candidateContext?.Unload();
            throw new ExtensionWorkerProtocolException(
                ExtensionWorkerErrorCodes.InternalError,
                $"failed to load extension assembly: {exception.Message}");
        }
        catch (ReflectionTypeLoadException exception)
        {
            candidateContext?.Unload();
            throw new ExtensionWorkerProtocolException(
                ExtensionWorkerErrorCodes.InternalError,
                $"failed to load extension assembly: {exception.Message}");
        }
        catch (MissingMethodException exception)
        {
            candidateContext?.Unload();
            throw new ExtensionWorkerProtocolException(
                ExtensionWorkerErrorCodes.InternalError,
                $"failed to load extension assembly: {exception.Message}");
        }
        catch (MemberAccessException exception)
        {
            candidateContext?.Unload();
            throw new ExtensionWorkerProtocolException(
                ExtensionWorkerErrorCodes.InternalError,
                $"failed to load extension assembly: {exception.Message}");
        }
        catch (TargetInvocationException exception)
        {
            candidateContext?.Unload();
            throw new ExtensionWorkerProtocolException(
                ExtensionWorkerErrorCodes.InternalError,
                $"failed to load extension assembly: {exception.Message}");
        }
        catch (InvalidOperationException exception)
        {
            candidateContext?.Unload();
            throw new ExtensionWorkerProtocolException(
                ExtensionWorkerErrorCodes.InternalError,
                $"failed to load extension assembly: {exception.Message}");
        }
    }

    private static async ValueTask TryDeactivateSilentlyAsync(IExtension extension)
    {
        try
        {
            await extension.DeactivateAsync(CancellationToken.None);
        }
        catch (Exception) {
            // Ignore deactivate failures when bootstrap does not complete.
        }
    }

    private static IReadOnlyList<ExtensionWorkerProviderDescriptor> DescribeProviders(IExtension extension)
    {
        var providers = new List<ExtensionWorkerProviderDescriptor>();

        if (extension is ILspDiagnosticProvider diagnosticProvider)
        {
            providers.Add(new ExtensionWorkerProviderDescriptor(
                Capability: ExtensionCapabilityNames.Diagnostic,
                Name: diagnosticProvider.Name,
                Priority: diagnosticProvider.Priority));
        }

        if (extension is ILspCodeActionProvider codeActionProvider)
        {
            providers.Add(new ExtensionWorkerProviderDescriptor(
                Capability: ExtensionCapabilityNames.CodeAction,
                Name: codeActionProvider.Name,
                Priority: codeActionProvider.Priority));
        }

        if (extension is ILspHoverProvider hoverProvider)
        {
            providers.Add(new ExtensionWorkerProviderDescriptor(
                Capability: ExtensionCapabilityNames.Hover,
                Name: hoverProvider.Name,
                Priority: hoverProvider.Priority));
        }

        if (extension is ILspCompletionProvider completionProvider)
        {
            providers.Add(new ExtensionWorkerProviderDescriptor(
                Capability: ExtensionCapabilityNames.Completion,
                Name: completionProvider.Name,
                Priority: completionProvider.Priority));
        }

        if (extension is ILspDocumentSymbolProvider documentSymbolProvider)
        {
            providers.Add(new ExtensionWorkerProviderDescriptor(
                Capability: ExtensionCapabilityNames.DocumentSymbol,
                Name: documentSymbolProvider.Name,
                Priority: documentSymbolProvider.Priority));
        }

        if (extension is ILspSignatureHelpProvider signatureHelpProvider)
        {
            providers.Add(new ExtensionWorkerProviderDescriptor(
                Capability: ExtensionCapabilityNames.SignatureHelp,
                Name: signatureHelpProvider.Name,
                Priority: signatureHelpProvider.Priority));
        }

        if (extension is ILspInlayHintProvider inlayHintProvider)
        {
            providers.Add(new ExtensionWorkerProviderDescriptor(
                Capability: ExtensionCapabilityNames.InlayHint,
                Name: inlayHintProvider.Name,
                Priority: inlayHintProvider.Priority));
        }

        if (extension is ILspWorkspaceSymbolProvider workspaceSymbolProvider)
        {
            providers.Add(new ExtensionWorkerProviderDescriptor(
                Capability: ExtensionCapabilityNames.WorkspaceSymbol,
                Name: workspaceSymbolProvider.Name,
                Priority: workspaceSymbolProvider.Priority));
        }

        if (extension is ILspFoldingRangeProvider foldingRangeProvider)
        {
            providers.Add(new ExtensionWorkerProviderDescriptor(
                Capability: ExtensionCapabilityNames.FoldingRange,
                Name: foldingRangeProvider.Name,
                Priority: foldingRangeProvider.Priority));
        }

        if (extension is ILspReferenceProvider referenceProvider)
        {
            providers.Add(new ExtensionWorkerProviderDescriptor(
                Capability: ExtensionCapabilityNames.References,
                Name: referenceProvider.Name,
                Priority: referenceProvider.Priority));
        }

        if (extension is ILspRenameProvider renameProvider)
        {
            providers.Add(new ExtensionWorkerProviderDescriptor(
                Capability: ExtensionCapabilityNames.Rename,
                Name: renameProvider.Name,
                Priority: renameProvider.Priority));
        }

        return providers;
    }

    private static TPayload DeserializeRequired<TPayload>(object? payload, string name)
    {
        if (payload is JsonElement element)
        {
            return LspJsonSerializer.Deserialize<TPayload>(element.GetRawText())
                ?? throw new ExtensionWorkerProtocolException(
                    ExtensionWorkerErrorCodes.InvalidParams,
                    $"{name} payload is invalid.");
        }

        if (payload is TPayload typed)
        {
            return typed;
        }

        if (payload is null)
        {
            throw new ExtensionWorkerProtocolException(
                ExtensionWorkerErrorCodes.InvalidParams,
                $"{name} payload is required.");
        }

        return LspJsonSerializer.Deserialize<TPayload>(LspJsonSerializer.Serialize(payload))
            ?? throw new ExtensionWorkerProtocolException(
                ExtensionWorkerErrorCodes.InvalidParams,
                $"{name} payload is invalid.");
    }

    private static CancellationTokenSource CreateOperationTimeoutTokenSource(
        CancellationToken cancellationToken,
        TimeSpan timeout)
    {
        var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        linkedSource.CancelAfter(timeout);
        return linkedSource;
    }

    private sealed class ExtensionWorkerProtocolException(
        string code,
        string message,
        Exception? innerException = null) : Exception(message, innerException)
    {
        public string Code { get; } = code;
    }
}
