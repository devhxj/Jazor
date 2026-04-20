namespace Jolt.Analysis;

public static class VueAnalysisClientFactory
{
    public static IVueAnalysisClient CreateDefault()
        => new JazorVueAnalysisService();

    public static IVueAnalysisClient CreateFromTransport(IAnalysisRpcTransport transport)
    {
        ArgumentNullException.ThrowIfNull(transport);
        return new RpcVueAnalysisClient(transport);
    }

    public static IVueAnalysisClient Create(string[] args)
    {
        args ??= [];

        string? command = null;
        string? arguments = null;

        foreach (var arg in args)
        {
            if (arg.StartsWith("--analysis-command=", StringComparison.OrdinalIgnoreCase))
            {
                command = arg["--analysis-command=".Length..];
            }
            else if (arg.StartsWith("--analysis-args=", StringComparison.OrdinalIgnoreCase))
            {
                arguments = arg["--analysis-args=".Length..];
            }
        }

        return !string.IsNullOrWhiteSpace(command)
            ? CreateFromTransport(new ProcessAnalysisRpcTransport(command, arguments))
            : CreateDefault();
    }

    public static IVueAnalysisClient Create(IAnalysisRpcTransport? transport = null)
        => transport is null
            ? CreateDefault()
            : new RpcVueAnalysisClient(transport);
}
