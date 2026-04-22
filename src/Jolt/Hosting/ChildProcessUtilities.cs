using System.Diagnostics;

namespace Jolt.Hosting;

internal static class ChildProcessUtilities
{
    private static readonly TimeSpan TerminateWaitTimeout = TimeSpan.FromSeconds(5);

    public static async Task WaitForExitOrTerminateOnCancellationAsync(
        Process process,
        CancellationToken cancellationToken)
    {
        try
        {
            await process.WaitForExitAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            await TerminateProcessAsync(process);
            throw;
        }
    }

    public static async Task TerminateProcessAsync(Process? process)
    {
        if (process is null)
        {
            return;
        }

        try
        {
            if (process.HasExited)
            {
                return;
            }
        }
        catch (ObjectDisposedException ex)
        {
            WriteTerminationDebug(process, ex);
            return;
        }
        catch (InvalidOperationException ex)
        {
            WriteTerminationDebug(process, ex);
            return;
        }

        var processId = TryGetProcessIdValue(process);
        if (OperatingSystem.IsWindows() && processId is not null)
        {
            await TryTerminateWindowsProcessTreeAsync(processId.Value);
        }

        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch (ObjectDisposedException ex)
        {
            WriteTerminationDebug(process, ex);
            return;
        }
        catch (InvalidOperationException ex)
        {
            WriteTerminationDebug(process, ex);
            return;
        }
        catch (PlatformNotSupportedException ex)
        {
            WriteTerminationDebug(process, ex);
            return;
        }
        catch (NotSupportedException ex)
        {
            WriteTerminationDebug(process, ex);
            return;
        }
        catch (System.ComponentModel.Win32Exception ex)
        {
            WriteTerminationDebug(process, ex);
            return;
        }

        try
        {
            using var waitTimeout = new CancellationTokenSource(TerminateWaitTimeout);
            await process.WaitForExitAsync(waitTimeout.Token);
        }
        catch (ObjectDisposedException ex)
        {
            WriteTerminationDebug(process, ex);
        }
        catch (InvalidOperationException ex)
        {
            WriteTerminationDebug(process, ex);
        }
        catch (OperationCanceledException ex)
        {
            WriteTerminationDebug(process, ex);
        }
    }

    private static void WriteTerminationDebug(Process process, Exception exception)
    {
        try
        {
            Console.Error.WriteLine(
                $"[jolt][process][debug] Process termination cleanup ignored {exception.GetType().Name} for process id {TryGetProcessId(process)}: {exception.Message}");
        }
        catch
        {
        }
    }

    private static string TryGetProcessId(Process process)
        => TryGetProcessIdValue(process)?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "<unknown>";

    private static int? TryGetProcessIdValue(Process process)
    {
        try
        {
            return process.Id;
        }
        catch
        {
            return null;
        }
    }

    private static async Task TryTerminateWindowsProcessTreeAsync(int processId)
    {
        try
        {
            using var taskKill = Process.Start(new ProcessStartInfo
            {
                FileName = "taskkill.exe",
                ArgumentList =
                {
                    "/PID",
                    processId.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    "/T",
                    "/F"
                },
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            });
            if (taskKill is null)
            {
                return;
            }

            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await taskKill.WaitForExitAsync(timeout.Token);
        }
        catch (Exception ex) when (ex is InvalidOperationException
            or ObjectDisposedException
            or System.ComponentModel.Win32Exception
            or OperationCanceledException)
        {
            try
            {
                Console.Error.WriteLine(
                    $"[jolt][process][debug] taskkill process-tree cleanup ignored {ex.GetType().Name} for process id {processId}: {ex.Message}");
            }
            catch
            {
            }
        }
    }
}
