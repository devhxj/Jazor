using System.Diagnostics;

namespace Jazor.VueHost.LanguageServers;

internal sealed class ExternalLspProcess : IAsyncDisposable
{
    private readonly ExternalProcessOptions _options;
    private Process? _process;

    public ExternalLspProcess(ExternalProcessOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public Stream? StandardInput => _process?.StandardInput.BaseStream;

    public Stream? StandardOutput => _process?.StandardOutput.BaseStream;

    public bool IsRunning => _process is { HasExited: false };

    public ValueTask StartAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (IsRunning)
        {
            return ValueTask.CompletedTask;
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = _options.FileName,
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        foreach (var argument in _options.Arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        if (!string.IsNullOrWhiteSpace(_options.WorkingDirectory))
        {
            startInfo.WorkingDirectory = _options.WorkingDirectory;
        }

        foreach (var pair in _options.EnvironmentVariables)
        {
            startInfo.Environment[pair.Key] = pair.Value;
        }

        _process = new Process
        {
            StartInfo = startInfo
        };

        if (!_process.Start())
        {
            throw new InvalidOperationException($"Failed to start external language server '{_options.Name}'.");
        }

        return ValueTask.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        if (_process is null)
        {
            return;
        }

        try
        {
            if (!_process.HasExited)
            {
                _process.Kill(entireProcessTree: true);
                await _process.WaitForExitAsync();
            }
        }
        finally
        {
            _process.Dispose();
            _process = null;
        }
    }
}
