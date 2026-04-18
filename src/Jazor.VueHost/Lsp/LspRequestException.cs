namespace Jazor.VueHost.Lsp;

internal sealed class LspRequestException : InvalidOperationException
{
    public LspRequestException(int errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public LspRequestException(int errorCode, string message, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }

    public int ErrorCode { get; }
}
