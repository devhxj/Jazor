using Jazor.Vue.Analysis.Runtime;

var useStdio = Console.IsInputRedirected
    || args.Any(static arg => string.Equals(arg, "--stdio", StringComparison.OrdinalIgnoreCase));
var cancellationToken = CancellationToken.None;

var service = new JazorVueAnalysisService();
var processor = new VueAnalysisRpcProcessor(service);
var server = new StdioVueAnalysisRpcServer(processor);

if (useStdio)
{
    await server.RunAsync(Console.In, Console.Out, cancellationToken);
    return;
}

Console.WriteLine("Jazor.Vue.Analysis.Host ready.");
Console.WriteLine("Pass --stdio or pipe RPC requests to stdin.");
