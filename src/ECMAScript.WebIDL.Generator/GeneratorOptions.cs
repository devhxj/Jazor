namespace ECMAScript.WebIDL.Generator;

internal sealed record GeneratorOptions(
    string RepositoryRoot,
    string WorkerPath,
    string OutputDirectory,
    string InventoryFileName)
{
    public static GeneratorOptions Parse(string[] args, RepositoryLayout layout)
    {
        var workerPath = layout.DefaultWorkerPath;
        var outputDirectory = layout.DefaultOutputDirectory;
        var inventoryFileName = "webidl.inventory.json";

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--worker":
                    workerPath = RequireValue(args, ++i, "--worker");
                    break;
                case "--out":
                    outputDirectory = RequireValue(args, ++i, "--out");
                    break;
                case "--inventory":
                    inventoryFileName = RequireRawValue(args, ++i, "--inventory");
                    break;
            }
        }

        return new GeneratorOptions(layout.RepositoryRoot, workerPath, outputDirectory, inventoryFileName);
    }

    private static string RequireValue(string[] args, int index, string optionName)
    {
        return Path.GetFullPath(RequireRawValue(args, index, optionName));
    }

    private static string RequireRawValue(string[] args, int index, string optionName)
    {
        if (index >= args.Length || string.IsNullOrWhiteSpace(args[index]))
        {
            throw new ArgumentException($"Missing value for '{optionName}'.", nameof(args));
        }

        return args[index];
    }
}
