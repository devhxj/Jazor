using Jolt.LanguageServer;

using var shutdown = new CancellationTokenSource();
Console.CancelKeyPress += (_, args) =>
{
    args.Cancel = true;
    shutdown.Cancel();
};
await Server.RunStdioAsync(shutdown.Token);
