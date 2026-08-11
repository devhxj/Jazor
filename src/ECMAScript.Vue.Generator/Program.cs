namespace ECMAScript.VueGenerator;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        try
        {
            switch (args)
            {
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
                default:
                    Console.Error.WriteLine("Usage: elementplus|vuetify [--check] | tdesign snapshot|bindings|components [--check|--report]");
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
