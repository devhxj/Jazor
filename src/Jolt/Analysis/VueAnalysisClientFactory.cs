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
                command = TryReadOptionValue(arg, "--analysis-command");
            }
            else if (arg.StartsWith("--analysis-args=", StringComparison.OrdinalIgnoreCase))
            {
                arguments = TryReadOptionValue(arg, "--analysis-args");
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

    private static string? TryReadOptionValue(string argument, string optionName)
    {
        var prefix = optionName + "=";
        if (!argument.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            || argument.Length <= prefix.Length)
        {
            return null;
        }

        return argument[prefix.Length..];
    }
}
