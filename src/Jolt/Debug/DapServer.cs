using Jolt.Lsp;

namespace Jolt.Debug;

internal sealed class DapServer(DapRequestHandler requestHandler)
{
    private readonly DapRequestHandler _requestHandler = requestHandler ?? throw new ArgumentNullException(nameof(requestHandler));

    public Task RunStdioAsync(CancellationToken cancellationToken)
        => RunAsync(
            Console.OpenStandardInput(),
            Console.OpenStandardOutput(),
            cancellationToken);

    internal async Task RunAsync(
        Stream input,
        Stream output,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(output);

        var reader = new LspMessageReader(input);
        var writer = new LspMessageWriter(output);

        while (!cancellationToken.IsCancellationRequested)
        {
            var messageJson = await reader.ReadMessageAsync(cancellationToken);
            if (messageJson is null)
            {
                break;
            }

            var request = DapProtocolSerializer.Deserialize<DapRequest>(messageJson);
            if (request is null || !string.Equals(request.Type, "request", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var result = await _requestHandler.HandleAsync(request, cancellationToken);
            await writer.WriteMessageAsync(
                DapProtocolSerializer.Serialize(result.Response),
                cancellationToken);

            foreach (var @event in result.Events)
            {
                await writer.WriteMessageAsync(
                    DapProtocolSerializer.Serialize(@event),
                    cancellationToken);
            }

            if (result.ShouldTerminate)
            {
                break;
            }
        }
    }
}
