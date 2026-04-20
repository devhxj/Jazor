namespace Jolt.Analysis;

public sealed class StdioVueAnalysisRpcServer
{
    private readonly IVueAnalysisRpcProcessor _processor;

    public StdioVueAnalysisRpcServer(IVueAnalysisRpcProcessor processor)
    {
        _processor = processor ?? throw new ArgumentNullException(nameof(processor));
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
            cancellationToken.ThrowIfCancellationRequested();
            var requestLine = await input.ReadLineAsync();
            if (requestLine is null)
                break;

            if (string.IsNullOrWhiteSpace(requestLine))
                continue;

            var responseLine = await _processor.ProcessAsync(requestLine, cancellationToken);
            await output.WriteLineAsync(responseLine);
            await output.FlushAsync();
        }
    }
}
