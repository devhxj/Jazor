namespace Jolt.Razor.Toolset;

internal sealed record RazorSdkToolset(
    string RootPath,
    string SdkVersion,
    string SdkRootPath,
    string RazorSdkRootPath,
    string RazorSourceGeneratorPath,
    string RazorTasksPath,
    string RazorDesignTimeTargetsPath,
    string RazorComponentTargetsPath);
