namespace Jazor.VueHost.Extensions;

internal sealed class ExtensionRegistry : IExtensionRegistry
{
    private readonly object _gate = new();
    private readonly Dictionary<string, IExtension> _extensions = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<ILspDiagnosticProvider> _lspDiagnosticProviders = [];
    private readonly List<ILspCodeActionProvider> _lspCodeActionProviders = [];

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
}
