using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace Jazor.AspNetCore.Dev;

internal sealed record JoltDevelopmentTool(string? AppHost, string? Package);

internal sealed class JoltDevelopmentSession(Process process, TimeSpan shutdownTimeout) : IAsyncDisposable
{
    private readonly TaskCompletionSource<Uri> _origin = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly StringBuilder _startupErrors = new();
    private Task _exit = Task.CompletedTask;

    internal static JoltDevelopmentSession Start(JoltDevelopmentTool tool, string contentRoot, string projectRoot,
        JazorSPAOptions options, int port, CancellationToken cancellationToken)
    {
        var start = new ProcessStartInfo(tool.AppHost ?? "dotnet")
        {
            WorkingDirectory = contentRoot,
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        if (tool.AppHost is null)
        {
            start.ArgumentList.Add("tool");
            start.ArgumentList.Add("exec");
            start.ArgumentList.Add(tool.Package!);
            start.ArgumentList.Add("--");
        }

        foreach (var argument in new[] { "dev", projectRoot, "--port", port.ToString(CultureInfo.InvariantCulture), "--stop-on-stdin-close" })
            start.ArgumentList.Add(argument);
        // Both launch paths use the same private protocol; older tools use their Vite config.
        start.Environment["JOLT_VITE_BASE"] = options.GetPublicBasePath();
        start.Environment["JAZOR_GENERATED_ROOT"] = options.ResolveGeneratedRoot(projectRoot);

        if (options.JoltDevelopmentConfiguration is { } configuration)
            start.Environment["JOLT_DEVELOPMENT_CONFIGURATION"] = configuration;
        cancellationToken.ThrowIfCancellationRequested();
        var child = Process.Start(start) ?? throw new InvalidOperationException("Could not launch Jolt dev.");
        var session = new JoltDevelopmentSession(child, options.Vite.ShutdownTimeout);
        session.ReadOutput();
        return session;
    }

    internal Task<Uri> WaitForOriginAsync(CancellationToken cancellationToken) => _origin.Task.WaitAsync(cancellationToken);

    private void ReadOutput()
    {
        // The CLI prints its bound URL after Vite starts. Continue draining stdout for HMR diagnostics.
        process.OutputDataReceived += (_, args) =>
        {
            if (args.Data is null)
                return;
            else
            {
                if (Uri.TryCreate(args.Data, UriKind.Absolute, out var url) && url.Scheme == Uri.UriSchemeHttp && url.IsLoopback && url.Port > 0)
                {
                    lock (_startupErrors)
                    {
                        _origin.TrySetResult(new Uri(url.GetLeftPart(UriPartial.Authority)));
                        _startupErrors.Clear();
                    }
                }
                Console.WriteLine(args.Data);
            }
        };
        process.ErrorDataReceived += (_, args) =>
        {
            if (args.Data is not { } line)
                return;
            Console.Error.WriteLine(line);
            lock (_startupErrors)
            {
                if (_origin.Task.IsCompleted)
                    return;
                const int limit = 8192;
                _startupErrors.AppendLine(line.Length > limit ? line[^limit..] : line);
                if (_startupErrors.Length > limit)
                    _startupErrors.Remove(0, _startupErrors.Length - limit);
            }
        };
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        _exit = ObserveExitAsync();
    }

    private async Task ObserveExitAsync()
    {
        // WaitForExitAsync drains both redirected readers before reporting the startup failure.
        await process.WaitForExitAsync().ConfigureAwait(false);
        lock (_startupErrors)
        {
            var details = _startupErrors.ToString().Trim();
            _origin.TrySetException(new InvalidOperationException(
                $"Jolt dev exited with code {process.ExitCode} before announcing its Vite URL."
                + (details.Length == 0 ? " See the tool output for details." : Environment.NewLine + details)));
            _startupErrors.Clear();
        }
    }

    internal void RequireRunning()
    {
        if (process.HasExited)
            throw new InvalidOperationException($"Jolt dev exited with code {process.ExitCode}. See the tool output for details.");
    }

    public async ValueTask DisposeAsync()
    {
        using (process)
        {
            if (process.HasExited)
            {
                await _exit.WaitAsync(shutdownTimeout).ConfigureAwait(false);
                return;
            }

            // EOF is also delivered when the ASP.NET host itself is forcibly stopped.
            process.StandardInput.Close();
            using (var graceful = new CancellationTokenSource(shutdownTimeout))
            {
                try
                {
                    await _exit.WaitAsync(graceful.Token).ConfigureAwait(false);
                    return;
                }
                catch (OperationCanceledException) when (graceful.IsCancellationRequested) { }
            }

            try
            {
                process.Kill(entireProcessTree: true);
            }
            catch (InvalidOperationException) when (process.HasExited) { }

            using var forced = new CancellationTokenSource(shutdownTimeout);
            await _exit.WaitAsync(forced.Token).ConfigureAwait(false);
        }
    }
}
