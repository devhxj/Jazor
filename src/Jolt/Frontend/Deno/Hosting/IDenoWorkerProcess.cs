namespace Jolt.Frontend.Deno.Hosting;

internal interface IDenoWorkerProcess
{
    bool IsRunning { get; }

    ValueTask StartAsync(CancellationToken cancellationToken);

    ValueTask<TResult?> SendRequestAsync<TResult>(
        string method,
        object payload,
        CancellationToken cancellationToken);

    ValueTask StopAsync(CancellationToken cancellationToken);
}
