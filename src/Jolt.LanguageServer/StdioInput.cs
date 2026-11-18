using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Jolt.LanguageServer;

internal static class StdioInput
{
    internal static Stream Open()
    {
        var input = Console.OpenStandardInput();
        if (!OperatingSystem.IsWindows())
            return input;
        var handle = GetStdHandle(-10);
        const int fileTypePipe = 3;
        return GetFileType(handle) == fileTypePipe ? new BufferedStream(new PipeInput(input, handle)) : input;
    }

    // A blocking ReadFile on Windows' synchronous stdio pipe can stall embedded
    // pnpm during a cold install. Read only bytes already queued in the pipe.
    private sealed class PipeInput(Stream input, IntPtr handle) : Stream
    {
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (buffer.IsEmpty)
                return 0;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!PeekNamedPipe(handle, IntPtr.Zero, 0, IntPtr.Zero, out var available, IntPtr.Zero))
                {
                    int error = Marshal.GetLastPInvokeError();
                    if (error is 109 or 233) // Broken or disconnected pipe.
                        return 0;
                    throw new Win32Exception(error);
                }
                if (available > 0)
                    return input.Read(buffer.Span[..(int)Math.Min((uint)buffer.Length, available)]);
                await Task.Delay(15, cancellationToken);
            }
        }

        public override int Read(byte[] buffer, int offset, int count) =>
            ReadAsync(buffer.AsMemory(offset, count)).AsTask().GetAwaiter().GetResult();
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                input.Dispose();
            base.Dispose(disposing);
        }
    }

    [DllImport("kernel32.dll", ExactSpelling = true)]
    private static extern IntPtr GetStdHandle(int handle);

    [DllImport("kernel32.dll", ExactSpelling = true)]
    private static extern int GetFileType(IntPtr handle);

    [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool PeekNamedPipe(IntPtr handle, IntPtr buffer, uint size, IntPtr read,
        out uint available, IntPtr bytesLeft);
}
