namespace ECMAScript.WebIDL.Generator;

internal sealed record GeneratorOptions(
    string RepositoryRoot,
    string WorkerPath,
    string NodeProjectPath,
    string OutputDirectory,
    string InventoryFileName,
    string? InputInventoryPath = null,
    bool SkipSpecificationProse = false,
    bool TestCollector = false)
{
    public static GeneratorOptions Parse(string[] args, RepositoryLayout layout)
    {
        var workerPath = layout.DefaultWorkerPath;
        var nodeProjectPath = layout.DefaultNodeProjectPath;
        var outputDirectory = layout.DefaultOutputDirectory;
        var inventoryFileName = "webidl.inventory.json";
        string? inputInventoryPath = null;
        var skipSpecificationProse = Environment.GetEnvironmentVariable("JAZOR_SKIP_SPEC_PROSE") == "1";
        var testCollector = false;

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--worker":
                    workerPath = RequireValue(args, ++i, "--worker");
                    break;
                case "--node-project":
                    nodeProjectPath = RequireValue(args, ++i, "--node-project");
                    break;
                case "--skip-spec-prose":
                    skipSpecificationProse = true;
                    break;
                case "--test-collector":
                    testCollector = true;
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
                case "--deno-config":
                    throw new ArgumentException("--deno-config was replaced by --node-project <package.json>.", nameof(args));
                default:
                    throw new ArgumentException($"Unknown option '{args[i]}'.", nameof(args));
            }
        }

        return new GeneratorOptions(layout.RepositoryRoot, workerPath, nodeProjectPath, outputDirectory, inventoryFileName, inputInventoryPath, skipSpecificationProse, testCollector);
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
