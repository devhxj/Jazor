namespace Jazor.VueHost.Extensions;

internal sealed class ExtensionRegistry : IExtensionRegistry
{
    private readonly object _gate = new();
    private readonly Dictionary<string, IExtension> _extensions = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<ILspDiagnosticProvider> _lspDiagnosticProviders = [];
    private readonly List<ILspCodeActionProvider> _lspCodeActionProviders = [];
    private readonly List<ILspHoverProvider> _lspHoverProviders = [];
    private readonly List<ILspCompletionProvider> _lspCompletionProviders = [];
    private readonly List<ILspDocumentSymbolProvider> _lspDocumentSymbolProviders = [];
    private readonly List<ILspSignatureHelpProvider> _lspSignatureHelpProviders = [];
    private readonly List<ILspInlayHintProvider> _lspInlayHintProviders = [];
    private readonly List<ILspWorkspaceSymbolProvider> _lspWorkspaceSymbolProviders = [];
    private readonly List<ILspFoldingRangeProvider> _lspFoldingRangeProviders = [];
    private readonly List<ILspReferenceProvider> _lspReferenceProviders = [];
    private readonly List<ILspRenameProvider> _lspRenameProviders = [];
    private readonly Dictionary<string, ExtensionLoadHealth> _extensionLoadHealthByKey = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, ExtensionProviderHealth> _providerHealthByKey = new(StringComparer.OrdinalIgnoreCase);

    public void RegisterExtension(IExtension extension)
    {
        ArgumentNullException.ThrowIfNull(extension);

        var id = extension.Metadata.Id;
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new InvalidOperationException("Extension metadata id cannot be empty.");
        }

        lock (_gate)
        {
            if (_extensions.ContainsKey(id))
            {
                throw new InvalidOperationException($"Extension '{id}' is already registered.");
            }

            _extensions[id] = extension;
        }

        if (extension is ILspDiagnosticProvider diagnosticProvider)
        {
            RegisterLspDiagnosticProvider(diagnosticProvider);
        }

        if (extension is ILspCodeActionProvider codeActionProvider)
        {
            RegisterLspCodeActionProvider(codeActionProvider);
        }

        if (extension is ILspHoverProvider hoverProvider)
        {
            RegisterLspHoverProvider(hoverProvider);
        }

        if (extension is ILspCompletionProvider completionProvider)
        {
            RegisterLspCompletionProvider(completionProvider);
        }

        if (extension is ILspDocumentSymbolProvider documentSymbolProvider)
        {
            RegisterLspDocumentSymbolProvider(documentSymbolProvider);
        }

        if (extension is ILspSignatureHelpProvider signatureHelpProvider)
        {
            RegisterLspSignatureHelpProvider(signatureHelpProvider);
        }

        if (extension is ILspInlayHintProvider inlayHintProvider)
        {
            RegisterLspInlayHintProvider(inlayHintProvider);
        }

        if (extension is ILspWorkspaceSymbolProvider workspaceSymbolProvider)
        {
            RegisterLspWorkspaceSymbolProvider(workspaceSymbolProvider);
        }

        if (extension is ILspFoldingRangeProvider foldingRangeProvider)
        {
            RegisterLspFoldingRangeProvider(foldingRangeProvider);
        }

        if (extension is ILspReferenceProvider referenceProvider)
        {
            RegisterLspReferenceProvider(referenceProvider);
        }

        if (extension is ILspRenameProvider renameProvider)
        {
            RegisterLspRenameProvider(renameProvider);
        }
    }

    public void UnregisterExtension(IExtension extension)
    {
        ArgumentNullException.ThrowIfNull(extension);

        var id = extension.Metadata.Id;
        lock (_gate)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                _extensions.Remove(id);
            }

            if (extension is ILspDiagnosticProvider diagnosticProvider)
            {
                _lspDiagnosticProviders.RemoveAll(existing => ReferenceEquals(existing, diagnosticProvider));
            }

            if (extension is ILspCodeActionProvider codeActionProvider)
            {
                _lspCodeActionProviders.RemoveAll(existing => ReferenceEquals(existing, codeActionProvider));
            }

            if (extension is ILspHoverProvider hoverProvider)
            {
                _lspHoverProviders.RemoveAll(existing => ReferenceEquals(existing, hoverProvider));
            }

            if (extension is ILspCompletionProvider completionProvider)
            {
                _lspCompletionProviders.RemoveAll(existing => ReferenceEquals(existing, completionProvider));
            }

            if (extension is ILspDocumentSymbolProvider documentSymbolProvider)
            {
                _lspDocumentSymbolProviders.RemoveAll(existing => ReferenceEquals(existing, documentSymbolProvider));
            }

            if (extension is ILspSignatureHelpProvider signatureHelpProvider)
            {
                _lspSignatureHelpProviders.RemoveAll(existing => ReferenceEquals(existing, signatureHelpProvider));
            }

            if (extension is ILspInlayHintProvider inlayHintProvider)
            {
                _lspInlayHintProviders.RemoveAll(existing => ReferenceEquals(existing, inlayHintProvider));
            }

            if (extension is ILspWorkspaceSymbolProvider workspaceSymbolProvider)
            {
                _lspWorkspaceSymbolProviders.RemoveAll(existing => ReferenceEquals(existing, workspaceSymbolProvider));
            }

            if (extension is ILspFoldingRangeProvider foldingRangeProvider)
            {
                _lspFoldingRangeProviders.RemoveAll(existing => ReferenceEquals(existing, foldingRangeProvider));
            }

            if (extension is ILspReferenceProvider referenceProvider)
            {
                _lspReferenceProviders.RemoveAll(existing => ReferenceEquals(existing, referenceProvider));
            }

            if (extension is ILspRenameProvider renameProvider)
            {
                _lspRenameProviders.RemoveAll(existing => ReferenceEquals(existing, renameProvider));
            }
        }
    }

    public void RegisterLspDiagnosticProvider(ILspDiagnosticProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        if (string.IsNullOrWhiteSpace(provider.Name))
        {
            throw new InvalidOperationException("Diagnostic provider name cannot be empty.");
        }

        lock (_gate)
        {
            _lspDiagnosticProviders.RemoveAll(existing =>
                string.Equals(existing.Name, provider.Name, StringComparison.OrdinalIgnoreCase));
            _lspDiagnosticProviders.Add(provider);
            _lspDiagnosticProviders.Sort(static (left, right) =>
            {
                var priority = right.Priority.CompareTo(left.Priority);
                return priority != 0
                    ? priority
                    : string.Compare(left.Name, right.Name, StringComparison.OrdinalIgnoreCase);
            });
        }
    }

    public void RegisterLspCodeActionProvider(ILspCodeActionProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        if (string.IsNullOrWhiteSpace(provider.Name))
        {
            throw new InvalidOperationException("Code action provider name cannot be empty.");
        }

        lock (_gate)
        {
            _lspCodeActionProviders.RemoveAll(existing =>
                string.Equals(existing.Name, provider.Name, StringComparison.OrdinalIgnoreCase));
            _lspCodeActionProviders.Add(provider);
            _lspCodeActionProviders.Sort(static (left, right) =>
            {
                var priority = right.Priority.CompareTo(left.Priority);
                return priority != 0
                    ? priority
                    : string.Compare(left.Name, right.Name, StringComparison.OrdinalIgnoreCase);
            });
        }
    }

    public void RegisterLspHoverProvider(ILspHoverProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        if (string.IsNullOrWhiteSpace(provider.Name))
        {
            throw new InvalidOperationException("Hover provider name cannot be empty.");
        }

        lock (_gate)
        {
            _lspHoverProviders.RemoveAll(existing =>
                string.Equals(existing.Name, provider.Name, StringComparison.OrdinalIgnoreCase));
            _lspHoverProviders.Add(provider);
            _lspHoverProviders.Sort(static (left, right) =>
            {
                var priority = right.Priority.CompareTo(left.Priority);
                return priority != 0
                    ? priority
                    : string.Compare(left.Name, right.Name, StringComparison.OrdinalIgnoreCase);
            });
        }
    }

    public void RegisterLspCompletionProvider(ILspCompletionProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        if (string.IsNullOrWhiteSpace(provider.Name))
        {
            throw new InvalidOperationException("Completion provider name cannot be empty.");
        }

        lock (_gate)
        {
            _lspCompletionProviders.RemoveAll(existing =>
                string.Equals(existing.Name, provider.Name, StringComparison.OrdinalIgnoreCase));
            _lspCompletionProviders.Add(provider);
            _lspCompletionProviders.Sort(static (left, right) =>
            {
                var priority = right.Priority.CompareTo(left.Priority);
                return priority != 0
                    ? priority
                    : string.Compare(left.Name, right.Name, StringComparison.OrdinalIgnoreCase);
            });
        }
    }

    public void RegisterLspDocumentSymbolProvider(ILspDocumentSymbolProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        if (string.IsNullOrWhiteSpace(provider.Name))
        {
            throw new InvalidOperationException("Document symbol provider name cannot be empty.");
        }

        lock (_gate)
        {
            _lspDocumentSymbolProviders.RemoveAll(existing =>
                string.Equals(existing.Name, provider.Name, StringComparison.OrdinalIgnoreCase));
            _lspDocumentSymbolProviders.Add(provider);
            _lspDocumentSymbolProviders.Sort(static (left, right) =>
            {
                var priority = right.Priority.CompareTo(left.Priority);
                return priority != 0
                    ? priority
                    : string.Compare(left.Name, right.Name, StringComparison.OrdinalIgnoreCase);
            });
        }
    }

    public void RegisterLspSignatureHelpProvider(ILspSignatureHelpProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        if (string.IsNullOrWhiteSpace(provider.Name))
        {
            throw new InvalidOperationException("Signature help provider name cannot be empty.");
        }

        lock (_gate)
        {
            _lspSignatureHelpProviders.RemoveAll(existing =>
                string.Equals(existing.Name, provider.Name, StringComparison.OrdinalIgnoreCase));
            _lspSignatureHelpProviders.Add(provider);
            _lspSignatureHelpProviders.Sort(static (left, right) =>
            {
                var priority = right.Priority.CompareTo(left.Priority);
                return priority != 0
                    ? priority
                    : string.Compare(left.Name, right.Name, StringComparison.OrdinalIgnoreCase);
            });
        }
    }

    public void RegisterLspInlayHintProvider(ILspInlayHintProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        if (string.IsNullOrWhiteSpace(provider.Name))
        {
            throw new InvalidOperationException("Inlay hint provider name cannot be empty.");
        }

        lock (_gate)
        {
            _lspInlayHintProviders.RemoveAll(existing =>
                string.Equals(existing.Name, provider.Name, StringComparison.OrdinalIgnoreCase));
            _lspInlayHintProviders.Add(provider);
            _lspInlayHintProviders.Sort(static (left, right) =>
            {
                var priority = right.Priority.CompareTo(left.Priority);
                return priority != 0
                    ? priority
                    : string.Compare(left.Name, right.Name, StringComparison.OrdinalIgnoreCase);
            });
        }
    }

    public void RegisterLspWorkspaceSymbolProvider(ILspWorkspaceSymbolProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        if (string.IsNullOrWhiteSpace(provider.Name))
        {
            throw new InvalidOperationException("Workspace symbol provider name cannot be empty.");
        }

        lock (_gate)
        {
            _lspWorkspaceSymbolProviders.RemoveAll(existing =>
                string.Equals(existing.Name, provider.Name, StringComparison.OrdinalIgnoreCase));
            _lspWorkspaceSymbolProviders.Add(provider);
            _lspWorkspaceSymbolProviders.Sort(static (left, right) =>
            {
                var priority = right.Priority.CompareTo(left.Priority);
                return priority != 0
                    ? priority
                    : string.Compare(left.Name, right.Name, StringComparison.OrdinalIgnoreCase);
            });
        }
    }

    public void RegisterLspFoldingRangeProvider(ILspFoldingRangeProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        if (string.IsNullOrWhiteSpace(provider.Name))
        {
            throw new InvalidOperationException("Folding range provider name cannot be empty.");
        }

        lock (_gate)
        {
            _lspFoldingRangeProviders.RemoveAll(existing =>
                string.Equals(existing.Name, provider.Name, StringComparison.OrdinalIgnoreCase));
            _lspFoldingRangeProviders.Add(provider);
            _lspFoldingRangeProviders.Sort(static (left, right) =>
            {
                var priority = right.Priority.CompareTo(left.Priority);
                return priority != 0
                    ? priority
                    : string.Compare(left.Name, right.Name, StringComparison.OrdinalIgnoreCase);
            });
        }
    }

    public void RegisterLspReferenceProvider(ILspReferenceProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        if (string.IsNullOrWhiteSpace(provider.Name))
        {
            throw new InvalidOperationException("Reference provider name cannot be empty.");
        }

        lock (_gate)
        {
            _lspReferenceProviders.RemoveAll(existing =>
                string.Equals(existing.Name, provider.Name, StringComparison.OrdinalIgnoreCase));
            _lspReferenceProviders.Add(provider);
            _lspReferenceProviders.Sort(static (left, right) =>
            {
                var priority = right.Priority.CompareTo(left.Priority);
                return priority != 0
                    ? priority
                    : string.Compare(left.Name, right.Name, StringComparison.OrdinalIgnoreCase);
            });
        }
    }

    public void RegisterLspRenameProvider(ILspRenameProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        if (string.IsNullOrWhiteSpace(provider.Name))
        {
            throw new InvalidOperationException("Rename provider name cannot be empty.");
        }

        lock (_gate)
        {
            _lspRenameProviders.RemoveAll(existing =>
                string.Equals(existing.Name, provider.Name, StringComparison.OrdinalIgnoreCase));
            _lspRenameProviders.Add(provider);
            _lspRenameProviders.Sort(static (left, right) =>
            {
                var priority = right.Priority.CompareTo(left.Priority);
                return priority != 0
                    ? priority
                    : string.Compare(left.Name, right.Name, StringComparison.OrdinalIgnoreCase);
            });
        }
    }

    public IReadOnlyDictionary<string, IExtension> GetExtensions()
    {
        lock (_gate)
        {
            return new Dictionary<string, IExtension>(_extensions, StringComparer.OrdinalIgnoreCase);
        }
    }

    public IReadOnlyList<ILspDiagnosticProvider> GetLspDiagnosticProviders()
    {
        lock (_gate)
        {
            return _lspDiagnosticProviders.ToArray();
        }
    }

    public IReadOnlyList<ILspCodeActionProvider> GetLspCodeActionProviders()
    {
        lock (_gate)
        {
            return _lspCodeActionProviders.ToArray();
        }
    }

    public IReadOnlyList<ILspHoverProvider> GetLspHoverProviders()
    {
        lock (_gate)
        {
            return _lspHoverProviders.ToArray();
        }
    }

    public IReadOnlyList<ILspCompletionProvider> GetLspCompletionProviders()
    {
        lock (_gate)
        {
            return _lspCompletionProviders.ToArray();
        }
    }

    public IReadOnlyList<ILspDocumentSymbolProvider> GetLspDocumentSymbolProviders()
    {
        lock (_gate)
        {
            return _lspDocumentSymbolProviders.ToArray();
        }
    }

    public IReadOnlyList<ILspSignatureHelpProvider> GetLspSignatureHelpProviders()
    {
        lock (_gate)
        {
            return _lspSignatureHelpProviders.ToArray();
        }
    }

    public IReadOnlyList<ILspInlayHintProvider> GetLspInlayHintProviders()
    {
        lock (_gate)
        {
            return _lspInlayHintProviders.ToArray();
        }
    }

    public IReadOnlyList<ILspWorkspaceSymbolProvider> GetLspWorkspaceSymbolProviders()
    {
        lock (_gate)
        {
            return _lspWorkspaceSymbolProviders.ToArray();
        }
    }

    public IReadOnlyList<ILspFoldingRangeProvider> GetLspFoldingRangeProviders()
    {
        lock (_gate)
        {
            return _lspFoldingRangeProviders.ToArray();
        }
    }

    public IReadOnlyList<ILspReferenceProvider> GetLspReferenceProviders()
    {
        lock (_gate)
        {
            return _lspReferenceProviders.ToArray();
        }
    }

    public IReadOnlyList<ILspRenameProvider> GetLspRenameProviders()
    {
        lock (_gate)
        {
            return _lspRenameProviders.ToArray();
        }
    }

    public void ReportExtensionLoad(ExtensionLoadInvocation invocation)
    {
        ArgumentNullException.ThrowIfNull(invocation);

        if (string.IsNullOrWhiteSpace(invocation.ExtensionId)
            || string.IsNullOrWhiteSpace(invocation.Source))
        {
            return;
        }

        var status = invocation.Status?.Trim();
        if (string.IsNullOrWhiteSpace(status))
        {
            return;
        }

        var key = CreateHealthKey(invocation.Source, invocation.ExtensionId);
        lock (_gate)
        {
            _extensionLoadHealthByKey.TryGetValue(key, out var current);
            current ??= new ExtensionLoadHealth(
                ExtensionId: invocation.ExtensionId,
                Source: invocation.Source,
                LoadedCount: 0,
                RejectedCount: 0,
                FailedCount: 0,
                AttemptCount: 0,
                LastAttemptAt: null,
                LastLoadedAt: null,
                LastRejectedAt: null,
                LastFailedAt: null,
                LastReason: null,
                LastManifestPath: null,
                LastAssemblyPath: null,
                LastExtensionDirectory: null);

            var loadedCount = current.LoadedCount;
            var rejectedCount = current.RejectedCount;
            var failedCount = current.FailedCount;
            var lastLoadedAt = current.LastLoadedAt;
            var lastRejectedAt = current.LastRejectedAt;
            var lastFailedAt = current.LastFailedAt;
            if (string.Equals(status, ExtensionLoadStatus.Loaded, StringComparison.OrdinalIgnoreCase))
            {
                loadedCount++;
                lastLoadedAt = invocation.Timestamp;
            }
            else if (string.Equals(status, ExtensionLoadStatus.Rejected, StringComparison.OrdinalIgnoreCase))
            {
                rejectedCount++;
                lastRejectedAt = invocation.Timestamp;
            }
            else if (string.Equals(status, ExtensionLoadStatus.Failed, StringComparison.OrdinalIgnoreCase))
            {
                failedCount++;
                lastFailedAt = invocation.Timestamp;
            }

            _extensionLoadHealthByKey[key] = current with
            {
                LoadedCount = loadedCount,
                RejectedCount = rejectedCount,
                FailedCount = failedCount,
                AttemptCount = current.AttemptCount + 1,
                LastAttemptAt = invocation.Timestamp,
                LastLoadedAt = lastLoadedAt,
                LastRejectedAt = lastRejectedAt,
                LastFailedAt = lastFailedAt,
                LastReason = invocation.Reason,
                LastManifestPath = invocation.ManifestPath,
                LastAssemblyPath = invocation.AssemblyPath,
                LastExtensionDirectory = invocation.ExtensionDirectory
            };
        }
    }

    public IReadOnlyList<ExtensionLoadHealth> GetExtensionLoadHealth()
    {
        lock (_gate)
        {
            return _extensionLoadHealthByKey.Values
                .OrderBy(static item => item.Source, StringComparer.OrdinalIgnoreCase)
                .ThenBy(static item => item.ExtensionId, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
    }

    public void ReportProviderInvocation(ExtensionProviderInvocation invocation)
    {
        ArgumentNullException.ThrowIfNull(invocation);

        if (string.IsNullOrWhiteSpace(invocation.ProviderName) || string.IsNullOrWhiteSpace(invocation.Capability))
        {
            return;
        }

        lock (_gate)
        {
            var key = CreateHealthKey(invocation.Capability, invocation.ProviderName);
            _providerHealthByKey.TryGetValue(key, out var current);
            current ??= new ExtensionProviderHealth(
                ProviderName: invocation.ProviderName,
                Capability: invocation.Capability,
                SuccessCount: 0,
                FailureCount: 0,
                TimeoutCount: 0,
                SkippedCount: 0,
                LastDuration: TimeSpan.Zero,
                LastSuccessAt: null,
                LastFailureAt: null,
                LastErrorMessage: null);

            var now = DateTimeOffset.UtcNow;
            ExtensionProviderHealth next;
            if (invocation.Skipped)
            {
                next = current with
                {
                    SkippedCount = current.SkippedCount + 1,
                    LastDuration = invocation.Duration,
                    LastErrorMessage = invocation.ErrorMessage
                };
            }
            else if (invocation.Succeeded)
            {
                next = current with
                {
                    SuccessCount = current.SuccessCount + 1,
                    LastDuration = invocation.Duration,
                    LastSuccessAt = now
                };
            }
            else
            {
                next = current with
                {
                    FailureCount = current.FailureCount + 1,
                    TimeoutCount = invocation.TimedOut
                        ? current.TimeoutCount + 1
                        : current.TimeoutCount,
                    LastDuration = invocation.Duration,
                    LastFailureAt = now,
                    LastErrorMessage = invocation.ErrorMessage
                };
            }

            _providerHealthByKey[key] = next;
        }
    }

    public IReadOnlyList<ExtensionProviderHealth> GetProviderHealth()
    {
        lock (_gate)
        {
            return _providerHealthByKey.Values
                .OrderBy(static item => item.Capability, StringComparer.OrdinalIgnoreCase)
                .ThenBy(static item => item.ProviderName, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
    }

    private static string CreateHealthKey(string capability, string providerName)
        => capability.Trim() + "|" + providerName.Trim();
}
