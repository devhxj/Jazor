namespace Jazor.Vite;

public sealed class JazorViteOptions
{
    public JazorViteOptions(
        JazorViteMode mode,
        string bunCommand,
        string? bunArguments,
        string? vueHostCommand,
        string? vueHostArguments,
        string? workingDirectory)
    {
        Mode = mode;
        BunCommand = string.IsNullOrWhiteSpace(bunCommand) ? "bun" : bunCommand;
        BunArguments = bunArguments;
        VueHostCommand = vueHostCommand;
        VueHostArguments = vueHostArguments;
        WorkingDirectory = workingDirectory;
    }

    public JazorViteMode Mode { get; }

    public string BunCommand { get; }

    public string? BunArguments { get; }

    public string? VueHostCommand { get; }

    public string? VueHostArguments { get; }

    public string? WorkingDirectory { get; }

    public bool HasVueHostProcess
        => !string.IsNullOrWhiteSpace(VueHostCommand);

    public static JazorViteOptions Parse(string[] args)
    {
        args ??= [];

        var mode = JazorViteMode.None;
        var bunCommand = "bun";
        string? bunArguments = "x vite";
        string? vueHostCommand = null;
        string? vueHostArguments = null;
        string? workingDirectory = null;

        foreach (var arg in args)
        {
            if (string.Equals(arg, "--probe-host", StringComparison.OrdinalIgnoreCase))
            {
                mode = JazorViteMode.ProbeHost;
            }
            else if (string.Equals(arg, "--run-dev", StringComparison.OrdinalIgnoreCase))
            {
                mode = JazorViteMode.RunDevServer;
            }
            else if (arg.StartsWith("--bun-command=", StringComparison.OrdinalIgnoreCase))
            {
                bunCommand = arg["--bun-command=".Length..];
            }
            else if (arg.StartsWith("--bun-args=", StringComparison.OrdinalIgnoreCase))
            {
                bunArguments = arg["--bun-args=".Length..];
            }
            else if (arg.StartsWith("--vuehost-command=", StringComparison.OrdinalIgnoreCase))
            {
                vueHostCommand = arg["--vuehost-command=".Length..];
            }
            else if (arg.StartsWith("--vuehost-args=", StringComparison.OrdinalIgnoreCase))
            {
                vueHostArguments = arg["--vuehost-args=".Length..];
            }
            else if (arg.StartsWith("--working-directory=", StringComparison.OrdinalIgnoreCase))
            {
                workingDirectory = arg["--working-directory=".Length..];
            }
        }

        return new JazorViteOptions(
            mode,
            bunCommand,
            bunArguments,
            vueHostCommand,
            vueHostArguments,
            workingDirectory);
    }
}
