namespace Jazor.Vue;

public sealed class VueAnalysisRpcException : Exception
{
    public VueAnalysisRpcException(string code, string message)
        : base(message)
    {
        Code = code ?? throw new ArgumentNullException(nameof(code));
    }

    public string Code { get; }
}

