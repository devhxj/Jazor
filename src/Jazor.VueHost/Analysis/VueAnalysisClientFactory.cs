namespace Jazor.VueHost.Analysis;

public static class VueAnalysisClientFactory
{
    public static VueAnalysisClientOptions Parse(string[] args)
    {
        args ??= [];

        var mode = VueAnalysisClientMode.Null;
        string? command = null;
        string? arguments = null;

        foreach (var arg in args)
        {
            if (arg.StartsWith("--analysis-client=", StringComparison.OrdinalIgnoreCase))
            {
                var value = arg["--analysis-client=".Length..];
                mode = string.Equals(value, "transport", StringComparison.OrdinalIgnoreCase)
                    ? VueAnalysisClientMode.Transport
                    : VueAnalysisClientMode.Null;
            }
            else if (arg.StartsWith("--analysis-command=", StringComparison.OrdinalIgnoreCase))
            {
                command = arg["--analysis-command=".Length..];
            }
            else if (arg.StartsWith("--analysis-args=", StringComparison.OrdinalIgnoreCase))
            {
                arguments = arg["--analysis-args=".Length..];
            }
        }

        return new VueAnalysisClientOptions(mode, command, arguments);
    }

    public static IVueAnalysisClient CreateDefault()
        => new Services.NullVueAnalysisClient();

    public static IVueAnalysisClient CreateFromTransport(IAnalysisRpcTransport transport)
    {
        ArgumentNullException.ThrowIfNull(transport);
        return new RpcVueAnalysisClient(transport);
    }

    public static IVueAnalysisClient Create(string[] args)
        => Create(Parse(args));

    public static IVueAnalysisClient Create(VueAnalysisClientOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return options.Mode switch
        {
            VueAnalysisClientMode.Transport when !string.IsNullOrWhiteSpace(options.Command)
                => CreateFromTransport(new ProcessAnalysisRpcTransport(options.Command, options.Arguments)),
            _ => CreateDefault()
        };
    }

    public static IVueAnalysisClient Create(IAnalysisRpcTransport? transport = null)
        => transport is null
            ? CreateDefault()
            : new RpcVueAnalysisClient(transport);
}
