using Jazor.Vite;
using Jazor.Vite.Bun;
using Jazor.Vite.VueHost;
using Jazor.VueContracts.Protocol;

var options = JazorViteOptions.Parse(args);
var cancellationToken = CancellationToken.None;

if (options.Mode == JazorViteMode.None)
{
    Console.WriteLine("Jazor.Vite");
    Console.WriteLine("Use --probe-host or --run-dev.");
    Console.WriteLine("Optional: --vuehost-command=<cmd> --vuehost-args=<args> --bun-command=<cmd> --bun-args=<args> --working-directory=<dir>");
    return;
}

if (options.Mode == JazorViteMode.ProbeHost)
{
    if (!options.HasVueHostProcess)
        throw new InvalidOperationException("Host probing requires --vuehost-command.");

    var client = new ProcessVueHostRpcClient(options.VueHostCommand!, options.VueHostArguments);
    var hostInfo = await client.GetHostInfoAsync(cancellationToken);
    Console.WriteLine(ProtocolJsonSerializer.Serialize(hostInfo));
    return;
}

if (options.HasVueHostProcess)
{
    var client = new ProcessVueHostRpcClient(options.VueHostCommand!, options.VueHostArguments);
    var ping = await client.PingAsync(cancellationToken);
    Console.WriteLine($"VueHost {ping.Message} ({ping.ProtocolVersion})");
}

var launcher = new BunViteLauncher(options);
var exitCode = await launcher.RunAsync(cancellationToken);
Environment.ExitCode = exitCode;
