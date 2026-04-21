namespace Jolt.Rpc;

public sealed class StdioJoltRpcServer
{
    private readonly IJoltRpcProcessor _rpcProcessor;

    public StdioJoltRpcServer(IJoltRpcProcessor rpcProcessor)
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

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var requestLine = await input.ReadLineAsync(cancellationToken);
                if (requestLine is null)
                {
                    break;
                }

                if (string.IsNullOrWhiteSpace(requestLine))
                {
                    continue;
                }

                var responseLine = await _rpcProcessor.ProcessAsync(requestLine, cancellationToken);
                await output.WriteLineAsync(responseLine.AsMemory(), cancellationToken);
                await output.FlushAsync(cancellationToken);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
    }
}
