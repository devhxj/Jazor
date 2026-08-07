namespace ECMAScript.WebIDL.Generator;

internal sealed record GeneratorOptions(
    string RepositoryRoot,
    string WorkerPath,
    string DenoConfigPath,
    string OutputDirectory,
    string InventoryFileName,
    string? InputInventoryPath = null)
{
    public static GeneratorOptions Parse(string[] args, RepositoryLayout layout)
    {
        var workerPath = layout.DefaultWorkerPath;
        var denoConfigPath = layout.DefaultDenoConfigPath;
        var outputDirectory = layout.DefaultOutputDirectory;
        var inventoryFileName = "webidl.inventory.json";
        string? inputInventoryPath = null;

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--worker":
                    workerPath = RequireValue(args, ++i, "--worker");
                    break;
                case "--deno-config":
                    denoConfigPath = RequireValue(args, ++i, "--deno-config");
                    break;
                case "--out":
                    outputDirectory = RequireValue(args, ++i, "--out");
                    break;
                case "--inventory":
                    inventoryFileName = RequireRawValue(args, ++i, "--inventory");
                    break;
                case "--from-inventory":
                    inputInventoryPath = RequireValue(args, ++i, "--from-inventory");
                    break;
            }
        }

        return new GeneratorOptions(layout.RepositoryRoot, workerPath, denoConfigPath, outputDirectory, inventoryFileName, inputInventoryPath);
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
