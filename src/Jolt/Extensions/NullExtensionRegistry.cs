using Jolt.Hosting;
using System.Collections.Concurrent;

namespace Jolt.Extensions;

internal sealed class NullExtensionRegistry : IExtensionRegistry
{
    private static readonly ConcurrentDictionary<string, byte> ReportedOperations = new(StringComparer.Ordinal);
    public static NullExtensionRegistry Instance { get; } = new();

    private NullExtensionRegistry()
    {
        FallbackTelemetry.ReportActivation(
            component: "extensionRegistry",
            mode: "null",
            reason: "extension-registry-not-configured");
    }

    public void RegisterExtension(IExtension extension)
    {
        WriteNoopDebug(nameof(RegisterExtension));
    }

    public void UnregisterExtension(IExtension extension)
    {
        WriteNoopDebug(nameof(UnregisterExtension));
    }

    public void RegisterLspDiagnosticProvider(ILspDiagnosticProvider provider)
    {
        WriteNoopDebug(nameof(RegisterLspDiagnosticProvider));
    }

    public void RegisterLspCodeActionProvider(ILspCodeActionProvider provider)
    {
        WriteNoopDebug(nameof(RegisterLspCodeActionProvider));
    }

    public void RegisterLspHoverProvider(ILspHoverProvider provider)
    {
        WriteNoopDebug(nameof(RegisterLspHoverProvider));
    }

    public void RegisterLspCompletionProvider(ILspCompletionProvider provider)
    {
        WriteNoopDebug(nameof(RegisterLspCompletionProvider));
    }

    public void RegisterLspDocumentSymbolProvider(ILspDocumentSymbolProvider provider)
    {
        WriteNoopDebug(nameof(RegisterLspDocumentSymbolProvider));
    }

    public void RegisterLspSignatureHelpProvider(ILspSignatureHelpProvider provider)
    {
        WriteNoopDebug(nameof(RegisterLspSignatureHelpProvider));
    }

    public void RegisterLspInlayHintProvider(ILspInlayHintProvider provider)
    {
        WriteNoopDebug(nameof(RegisterLspInlayHintProvider));
    }

    public void RegisterLspWorkspaceSymbolProvider(ILspWorkspaceSymbolProvider provider)
    {
        WriteNoopDebug(nameof(RegisterLspWorkspaceSymbolProvider));
    }

    public void RegisterLspFoldingRangeProvider(ILspFoldingRangeProvider provider)
    {
        WriteNoopDebug(nameof(RegisterLspFoldingRangeProvider));
    }

    public void RegisterLspReferenceProvider(ILspReferenceProvider provider)
    {
        WriteNoopDebug(nameof(RegisterLspReferenceProvider));
    }

    public void RegisterLspRenameProvider(ILspRenameProvider provider)
    {
        WriteNoopDebug(nameof(RegisterLspRenameProvider));
    }

    public IReadOnlyDictionary<string, IExtension> GetExtensions()
        => new Dictionary<string, IExtension>(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<ILspDiagnosticProvider> GetLspDiagnosticProviders()
        => Array.Empty<ILspDiagnosticProvider>();

    public IReadOnlyList<ILspCodeActionProvider> GetLspCodeActionProviders()
        => Array.Empty<ILspCodeActionProvider>();

    public IReadOnlyList<ILspHoverProvider> GetLspHoverProviders()
        => Array.Empty<ILspHoverProvider>();

    public IReadOnlyList<ILspCompletionProvider> GetLspCompletionProviders()
        => Array.Empty<ILspCompletionProvider>();

    public IReadOnlyList<ILspDocumentSymbolProvider> GetLspDocumentSymbolProviders()
        => Array.Empty<ILspDocumentSymbolProvider>();

    public IReadOnlyList<ILspSignatureHelpProvider> GetLspSignatureHelpProviders()
        => Array.Empty<ILspSignatureHelpProvider>();

    public IReadOnlyList<ILspInlayHintProvider> GetLspInlayHintProviders()
        => Array.Empty<ILspInlayHintProvider>();

    public IReadOnlyList<ILspWorkspaceSymbolProvider> GetLspWorkspaceSymbolProviders()
        => Array.Empty<ILspWorkspaceSymbolProvider>();

    public IReadOnlyList<ILspFoldingRangeProvider> GetLspFoldingRangeProviders()
        => Array.Empty<ILspFoldingRangeProvider>();

    public IReadOnlyList<ILspReferenceProvider> GetLspReferenceProviders()
        => Array.Empty<ILspReferenceProvider>();

    public IReadOnlyList<ILspRenameProvider> GetLspRenameProviders()
        => Array.Empty<ILspRenameProvider>();

    public void ReportExtensionLoad(ExtensionLoadInvocation invocation)
    {
        WriteNoopDebug(nameof(ReportExtensionLoad));
    }

    public IReadOnlyList<ExtensionLoadHealth> GetExtensionLoadHealth()
        => Array.Empty<ExtensionLoadHealth>();

    public IReadOnlyList<ExtensionLoadInvocation> GetRecentExtensionLoadInvocations(int maxCount = 100)
        => Array.Empty<ExtensionLoadInvocation>();

    public void ReportProviderInvocation(ExtensionProviderInvocation invocation, bool isReplay = false)
    {
        WriteNoopDebug(nameof(ReportProviderInvocation));
    }

    public IReadOnlyList<ExtensionProviderHealth> GetProviderHealth()
        => Array.Empty<ExtensionProviderHealth>();

    public IReadOnlyList<ExtensionProviderInvocationSnapshot> GetRecentProviderInvocations(int maxCount = 200)
        => Array.Empty<ExtensionProviderInvocationSnapshot>();

    private static void WriteNoopDebug(string operation)
    {
        if (!ReportedOperations.TryAdd(operation, 0))
        {
            return;
        }

        try
        {
            Console.Error.WriteLine($"[jolt][extensions][debug] NullExtensionRegistry ignored '{operation}' because extension registry is not configured.");
        }
        catch
        {
        }
    }
}
