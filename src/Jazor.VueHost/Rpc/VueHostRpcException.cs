namespace Jazor.VueHost.Rpc;

public sealed class VueHostRpcException : Exception
{
    public VueHostRpcException(string code, string message)
        : base(message)
    {
        Code = code ?? throw new ArgumentNullException(nameof(code));
    }

    public string Code { get; }
}
