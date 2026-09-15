namespace ECMAScript.VueGenerator;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        try
        {
            switch (args)
            {
                case ["documentation", "snapshot", .. var snapshotArgs]:
                    global::BindingDocumentationSnapshot.Run(snapshotArgs);
                    return 0;
                case ["documentation", .. var commandArgs]:
                    global::BindingDocumentationGenerator.Run(commandArgs);
                    return 0;
                case ["elementplus", .. var commandArgs]:
                    global::ElementPlusGenerator.Run(commandArgs);
                    return 0;
                case ["vuetify", .. var commandArgs]:
                    VuetifyCatalogGenerator.Run(commandArgs);
                    return 0;
                case ["tdesign", "snapshot", .. var commandArgs]:
                    await global::TDesignSnapshotGenerator.RunAsync(commandArgs);
                    return 0;
                case ["tdesign", "bindings", .. var commandArgs]:
                    global::TDesignBindingGenerator.Run(commandArgs);
                    return 0;
                case ["tdesign", "components", .. var commandArgs]:
                    global::TDesignComponentGenerator.Run(commandArgs);
                    return 0;
                case ["tdesign", "documentation", .. var commandArgs]:
                    global::TDesignDocumentation.Run(commandArgs);
                    return 0;
                default:
                    Console.Error.WriteLine("Usage: elementplus|vuetify [--check] | tdesign snapshot|bindings|components [--check|--report] | tdesign documentation <upstream-source.tar.gz>");
                    return 1;
            }
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.Message);
            return 1;
        }
    }
}