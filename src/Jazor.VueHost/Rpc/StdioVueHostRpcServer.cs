namespace Jazor.VueHost.Rpc;

public sealed class StdioVueHostRpcServer
{
    private readonly IVueHostRpcProcessor _rpcProcessor;

    public StdioVueHostRpcServer(IVueHostRpcProcessor rpcProcessor)
    {
        _rpcProcessor = rpcProcessor ?? throw new ArgumentNullException(nameof(rpcProcessor));
    }

    public async Task RunAsync(
        TextReader input,
        TextWriter output,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(output);

        while (!cancellationToken.IsCancellationRequested)
        {
            var requestLine = await input.ReadLineAsync(cancellationToken);
            if (requestLine is null)
                break;

            if (string.IsNullOrWhiteSpace(requestLine))
                continue;

            var responseLine = await _rpcProcessor.ProcessAsync(requestLine, cancellationToken);
            await output.WriteLineAsync(responseLine);
            await output.FlushAsync(cancellationToken);
        }
    }
}
