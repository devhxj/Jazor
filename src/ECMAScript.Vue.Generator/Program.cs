namespace ECMAScript.VueGenerator;

internal static class Program
{
    public static int Main(string[] args)
    {
        try
        {
            switch (args)
            {
                case ["authoring", .. var commandArgs]:
                    global::BindingAuthoringDocumentation.Run(commandArgs);
                    return 0;
                case ["documentation", "snapshot", .. var snapshotArgs]:
                    global::BindingDocumentationSnapshot.Run(snapshotArgs);
                    return 0;
                case ["documentation", .. var commandArgs]:
                    global::BindingDocumentationGenerator.Run(commandArgs);
                    return 0;
                default:
                    Console.Error.WriteLine("Usage: authoring [--check] | documentation [--check] | documentation snapshot <vue-3.5.42.tar.gz>");
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
