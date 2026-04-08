using Jazor.VueContracts.Protocol;
using Jazor.VueHost.Analysis;
using Jazor.VueHost.Hosting;
using Jazor.VueHost.Rpc;
using Jazor.VueHost.Workspace;
using Jazor.VueHost.Services;
using SharedVueHostRpcMethodNames = Jazor.VueContracts.Protocol.VueHostRpcMethodNames;

var useStdio = Console.IsInputRedirected
    || args.Any(static arg => string.Equals(arg, "--stdio", StringComparison.OrdinalIgnoreCase));
var cancellationToken = CancellationToken.None;
var analysisClient = VueAnalysisClientFactory.Create(args);
var hostService = new VueHostService(
    new InMemoryWorkspaceStore(),
    analysisClient);
var entry = new VueHostServiceEntry(hostService);
var rpcDispatcher = new VueHostRpcDispatcher(hostService);
var rpcProcessor = new VueHostRpcProcessor(rpcDispatcher);

await entry.RunAsync(cancellationToken);
try
{
    if (useStdio)
    {
        var stdioServer = new StdioVueHostRpcServer(rpcProcessor);
        await stdioServer.RunAsync(Console.In, Console.Out, cancellationToken);
        return;
    }

    var responseJson = await rpcProcessor.ProcessAsync(
        VueHostRpcSerializer.Serialize(new RpcRequestEnvelope(
            id: "startup",
            method: SharedVueHostRpcMethodNames.GetHostInfo,
            payloadJson: null)),
        cancellationToken);

    Console.WriteLine("Jazor.VueHost skeleton ready.");
    Console.WriteLine(responseJson);
}
finally
{
    await hostService.StopAsync(cancellationToken);
}
