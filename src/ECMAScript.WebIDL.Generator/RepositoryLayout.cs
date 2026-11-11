namespace ECMAScript.WebIDL.Generator;

internal sealed record RepositoryLayout(
    string RepositoryRoot,
    string DefaultWorkerPath,
    string DefaultNodeProjectPath,
    string DefaultOutputDirectory)
{
    public static RepositoryLayout Discover(string baseDirectory)
    {
        var current = new DirectoryInfo(baseDirectory);
        while (current is not null && !IsRepositoryRoot(current.FullName))
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
            Path.Combine(repositoryRoot, "src", "ECMAScript.WebIDL.Generator", "node", "collect.ts"),
            Path.Combine(repositoryRoot, "src", "ECMAScript.WebIDL.Generator", "node", "package.json"),
            Path.Combine(repositoryRoot, "src", "ECMAScript","webidl"));
    }

    private static bool IsRepositoryRoot(string directory)
    {
        return File.Exists(Path.Combine(directory, "Jazor.slnx"))
            || File.Exists(Path.Combine(directory, "Jazor.sln"));
    }
}
