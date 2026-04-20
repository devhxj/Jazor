using System.Diagnostics;

namespace Jolt.Hosting;

internal static class ChildProcessUtilities
{
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
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch (ObjectDisposedException)
        {
            return;
        }
        catch (InvalidOperationException)
        {
            return;
        }
        catch (PlatformNotSupportedException)
        {
            return;
        }
        catch (NotSupportedException)
        {
            return;
        }
        catch (System.ComponentModel.Win32Exception)
        {
            return;
        }

        try
        {
            await process.WaitForExitAsync(CancellationToken.None);
        }
        catch (ObjectDisposedException)
        {
        }
        catch (InvalidOperationException)
        {
        }
    }
}
