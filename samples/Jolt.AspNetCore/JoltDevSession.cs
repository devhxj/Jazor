using System.Diagnostics;
using System.Globalization;
using Jazor.AspNetCore.Dev;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

internal sealed class JoltDevSession(
    IHostEnvironment environment,
    IOptions<JazorFrontendOptions> frontendOptions) : IHostedService, IAsyncDisposable
{
    private Process? _owned;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var options = frontendOptions.Value;
        var origin = options.Vite.ServerOrigin;
        var project = Path.GetFullPath(options.ProjectRootPath, environment.ContentRootPath);
        var entry = new Uri(origin, $"{options.PathBase}{options.RequestPath}/{options.DevelopmentEntryRelativePath}");
        using var client = new HttpClient(new SocketsHttpHandler { AllowAutoRedirect = false })
        {
            Timeout = TimeSpan.FromSeconds(1)
        };
        if (await IsReadyAsync(cancellationToken))
            return;

        using var startup = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        startup.CancelAfter(options.Vite.StartupTimeout);
        // Resolve the local tool manifest from the ASP.NET project even when its
        // configured frontend root lives outside that directory.
        var start = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = environment.ContentRootPath,
            UseShellExecute = false,
            RedirectStandardInput = true
        };
        string[] arguments = ["tool", "run", "jolt", "dev", project, "--port", origin.Port.ToString(CultureInfo.InvariantCulture),
            "--stop-on-stdin-close"];
        foreach (var argument in arguments)
            start.ArgumentList.Add(argument);
        startup.Token.ThrowIfCancellationRequested();
        var process = Process.Start(start) ?? throw new InvalidOperationException("Could not launch the installed Jolt tool.");
        _owned = process;
        try
        {
            while (!await IsReadyAsync(startup.Token))
            {
                if (process.HasExited)
                    throw new InvalidOperationException($"Jolt dev exited with code {process.ExitCode}. Check the tool output and run 'dotnet tool restore' if it is missing.");
                await Task.Delay(100, startup.Token);
            }
        }
        catch
        {
            await DisposeAsync();
            throw;
        }

        async Task<bool> IsReadyAsync(CancellationToken stop)
        {
            try
            {
                using var response = await client.GetAsync(entry, HttpCompletionOption.ResponseHeadersRead, stop);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                return false;
            }
            catch (OperationCanceledException) when (!stop.IsCancellationRequested)
            {
                return false;
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => DisposeAsync().AsTask();

    public async ValueTask DisposeAsync()
    {
        using var process = Interlocked.Exchange(ref _owned, null);
        if (process is null || process.HasExited)
            return;

        // Closing the writer gives Jolt a chance to dispose Vite before dotnet tool run exits.
        process.StandardInput.Close();
        var shutdownTimeout = frontendOptions.Value.Vite.ShutdownTimeout;
        using (var graceful = new CancellationTokenSource(shutdownTimeout))
        {
            try
            {
                await process.WaitForExitAsync(graceful.Token);
                return;
            }
            catch (OperationCanceledException) when (graceful.IsCancellationRequested) { }
        }

        try
        {
            if (!process.HasExited)
                process.Kill(entireProcessTree: true);
        }
        catch (InvalidOperationException) when (process.HasExited)
        {
        }
        using var forced = new CancellationTokenSource(shutdownTimeout);
        await process.WaitForExitAsync(forced.Token);
    }
}
