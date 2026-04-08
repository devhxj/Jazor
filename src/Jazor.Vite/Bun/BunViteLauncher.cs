using System.Diagnostics;
using System.Text.Json;

namespace Jazor.Vite.Bun;

public sealed class BunViteLauncher
{
    private readonly JazorViteOptions _options;

    public BunViteLauncher(JazorViteOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<int> RunAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _options.BunCommand,
                Arguments = _options.BunArguments ?? string.Empty,
                UseShellExecute = false,
                RedirectStandardInput = false,
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                CreateNoWindow = false
            }
        };

        if (!string.IsNullOrWhiteSpace(_options.WorkingDirectory))
            process.StartInfo.WorkingDirectory = _options.WorkingDirectory!;

        if (_options.HasVueHostProcess)
        {
            process.StartInfo.Environment["JAZOR_VUEHOST_COMMAND"] = _options.VueHostCommand!;
            process.StartInfo.Environment["JAZOR_VUEHOST_ARGS"] = _options.VueHostArguments ?? string.Empty;
            process.StartInfo.Environment["JAZOR_VUEHOST_ARGS_JSON"] = JsonSerializer.Serialize(
                CommandLineArgumentSplitter.Split(_options.VueHostArguments));
            process.StartInfo.Environment["JAZOR_VUEHOST_RPC_MODE"] = "process-stdio";
        }

        if (!process.Start())
            throw new InvalidOperationException($"Failed to start Bun process '{_options.BunCommand}'.");

        await process.WaitForExitAsync(cancellationToken);
        return process.ExitCode;
    }
}
