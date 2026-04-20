namespace Jolt.Rpc;

public sealed class JoltRpcException : Exception
{
    public JoltRpcException(string code, string message)
        : base(message)
    {
        Code = code ?? throw new ArgumentNullException(nameof(code));
    }

    public string Code { get; }
}
