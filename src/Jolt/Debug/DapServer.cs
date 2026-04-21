using System.Text.Json;
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
            string? messageJson;
            try
            {
                messageJson = await reader.ReadMessageAsync(cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (InvalidDataException)
            {
                continue;
            }

            if (messageJson is null)
            {
                break;
            }

            DapRequest? request;
            try
            {
                request = DapProtocolSerializer.Deserialize<DapRequest>(messageJson);
            }
            catch (JsonException)
            {
                continue;
            }
            catch (NotSupportedException)
            {
                continue;
            }

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
