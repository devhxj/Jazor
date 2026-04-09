namespace Jazor.VueHost.Razor.Toolset;

internal sealed class RazorSdkToolsetHost
{
    private readonly RazorSdkToolsetResolver _resolver;

    public RazorSdkToolsetHost(RazorSdkToolsetResolver? resolver = null)
    {
        _resolver = resolver ?? new RazorSdkToolsetResolver();
    }

    public RazorSdkToolset? ResolveToolset()
        => _resolver.Resolve();

    public string Describe()
    {
        var toolset = ResolveToolset();
        if (toolset is null)
        {
            return "Razor SDK toolset: unavailable";
        }

        return string.Join(
            Environment.NewLine,
            [
                "Razor SDK toolset: available",
                $"  root:                {toolset.RootPath}",
                $"  sdk version:         {toolset.SdkVersion}",
                $"  sdk root:            {toolset.SdkRootPath}",
                $"  razor sdk root:      {toolset.RazorSdkRootPath}",
                $"  source generator:    {toolset.RazorSourceGeneratorPath}",
                $"  tasks:               {toolset.RazorTasksPath}",
                $"  design-time targets: {toolset.RazorDesignTimeTargetsPath}",
                $"  component targets:   {toolset.RazorComponentTargetsPath}"
            ]);
    }
}
