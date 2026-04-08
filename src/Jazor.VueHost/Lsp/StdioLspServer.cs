namespace Jazor.VueHost.Lsp;

internal sealed class StdioLspServer
{
    private readonly LspSession _session;

    public StdioLspServer(LspSession session)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
    }

    public async ValueTask RunAsync(
        Stream input,
        Stream output,
        CancellationToken cancellationToken)
    {
        var reader = new LspMessageReader(input);
        var writer = new LspMessageWriter(output);

        while (true)
        {
            var messageJson = await reader.ReadMessageAsync(cancellationToken);
            if (messageJson is null)
            {
                return;
            }

            var request = LspJsonSerializer.Deserialize<LspRequestMessage>(messageJson);
            if (request is null || string.IsNullOrWhiteSpace(request.Method))
            {
                continue;
            }

            if (request.Id is not null)
            {
                var response = await _session.HandleRequestAsync(request, cancellationToken);
                if (response is not null)
                {
                    await writer.WriteMessageAsync(LspJsonSerializer.Serialize(response), cancellationToken);
                }

                continue;
            }

            var shouldContinue = await _session.HandleNotificationAsync(request, cancellationToken);
            if (!shouldContinue)
            {
                return;
            }
        }
    }
}
