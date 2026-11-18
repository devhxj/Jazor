using Jolt.LanguageServer;

using var shutdown = new CancellationTokenSource();
Console.CancelKeyPress += (_, args) =>
{
    args.Cancel = true;
    shutdown.Cancel();
};
await new Server(Console.OpenStandardInput(), Console.OpenStandardOutput()).RunAsync(shutdown.Token);
