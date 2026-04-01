namespace ECMAScript.WebIDL.Generator;

internal sealed record RepositoryLayout(
    string RepositoryRoot,
    string DefaultWorkerPath,
    string DefaultOutputDirectory)
{
    public static RepositoryLayout Discover(string baseDirectory)
    {
        var current = new DirectoryInfo(baseDirectory);
        while (current is not null && !File.Exists(Path.Combine(current.FullName, "Jazor.sln")))
        {
            current = current.Parent;
        }

        if (current is null)
        {
            throw new DirectoryNotFoundException("Could not locate the repository root from the current application base directory.");
        }

        var repositoryRoot = current.FullName;
        return new RepositoryLayout(
            repositoryRoot,
            Path.Combine(repositoryRoot, "src", "ECMAScript.WebIDL", "deno", "collect.ts"),
            Path.Combine(repositoryRoot, "src", "ECMAScript", "generate", ".webidl"));
    }
}
