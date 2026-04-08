namespace Jazor.VueHost.Analysis;

public sealed class VueAnalysisClientOptions
{
    public VueAnalysisClientOptions(
        VueAnalysisClientMode mode,
        string? command,
        string? arguments)
    {
        Mode = mode;
        Command = command;
        Arguments = arguments;
    }

    public VueAnalysisClientMode Mode { get; }

    public string? Command { get; }

    public string? Arguments { get; }
}
