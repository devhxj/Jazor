namespace Jazor.Vue;

public sealed class StdioVueAnalysisRpcServer
{
    private static readonly System.Reflection.MethodInfo? CancellableReadLineAsyncMethod =
        typeof(TextReader).GetMethod(nameof(TextReader.ReadLineAsync), new[] { typeof(CancellationToken) });

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
        if (input is null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        if (output is null)
        {
            throw new ArgumentNullException(nameof(output));
        }

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var requestLine = await ReadLineAsync(input, cancellationToken);
                if (requestLine is null)
                {
                    break;
                }

                if (string.IsNullOrWhiteSpace(requestLine))
                {
                    continue;
                }

                var responseLine = await _processor.ProcessAsync(requestLine, cancellationToken);
                await output.WriteLineAsync(responseLine);
                cancellationToken.ThrowIfCancellationRequested();
                await output.FlushAsync();
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
    }

    private static async Task<string?> ReadLineAsync(TextReader input, CancellationToken cancellationToken)
    {
        if (CancellableReadLineAsyncMethod is not null)
        {
            var cancellableResult = CancellableReadLineAsyncMethod.Invoke(input, new object[] { cancellationToken });
            switch (cancellableResult)
            {
                case ValueTask<string?> valueTask:
                    return await valueTask.ConfigureAwait(false);
                case Task<string?> task:
                    return await task.ConfigureAwait(false);
            }
        }

        var readTask = input.ReadLineAsync();
        if (readTask.IsCompleted)
        {
            return await readTask.ConfigureAwait(false);
        }

        var cancellationTask = CreateCancellationTask(cancellationToken);
        var completedTask = await Task.WhenAny(readTask, cancellationTask).ConfigureAwait(false);
        if (completedTask != readTask)
        {
            throw new OperationCanceledException(cancellationToken);
        }

        return await readTask.ConfigureAwait(false);
    }

    private static Task CreateCancellationTask(CancellationToken cancellationToken)
    {
        if (!cancellationToken.CanBeCanceled)
        {
            return Task.Delay(Timeout.Infinite);
        }

        return Task.Delay(Timeout.Infinite, cancellationToken);
    }
}
