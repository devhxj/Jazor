using System.Runtime.InteropServices;

internal static class JoltDevInput
{
    internal static void StopOnClose(CancellationTokenSource stop)
    {
        if (!Console.IsInputRedirected)
            throw new ArgumentException("--stop-on-stdin-close requires redirected standard input.");

        // A pending synchronous Windows pipe read can stall LibNode during startup
        // when it initializes stdio. Probe a pipe before attempting to drain it.
        var pipe = OperatingSystem.IsWindows() ? GetStdHandle(-10) : 0;
        if (pipe != 0 && GetFileType(pipe) != 3)
            pipe = 0;
        new Thread(() =>
        {
            try
            {
                using var input = Console.OpenStandardInput();
                if (pipe == 0)
                    input.CopyTo(Stream.Null);
                else
                {
                    var buffer = new byte[4096];
                    while (!stop.IsCancellationRequested &&
                        PeekNamedPipe(pipe, 0, 0, 0, out var available, 0))
                    {
                        if (available > 0)
                        {
                            if (input.Read(buffer, 0, (int)Math.Min(available, (uint)buffer.Length)) == 0)
                                break;
                        }
                        else
                            Thread.Sleep(50);
                    }
                    if (stop.IsCancellationRequested)
                        return;
                }
            }
            catch (IOException)
            {
                // A broken input pipe also ends the owning session.
            }

            try { stop.Cancel(); }
            catch (ObjectDisposedException) { }
        }) { IsBackground = true }.Start();
    }

    [DllImport("kernel32.dll")]
    private static extern nint GetStdHandle(int handle);

    [DllImport("kernel32.dll")]
    private static extern uint GetFileType(nint file);

    [DllImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool PeekNamedPipe(nint pipe, nint buffer, uint bufferSize,
        nint bytesRead, out uint bytesAvailable, nint bytesLeft);
}
