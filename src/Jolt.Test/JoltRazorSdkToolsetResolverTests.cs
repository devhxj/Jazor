using Jolt.Razor.Toolset;

namespace Jolt.Test;

[TestClass]
public sealed class JoltRazorSdkToolsetResolverTests
{
    [TestMethod]
    public void ResolveRazorTasksPath_WithMultipleTaskTargetFrameworks_SelectsHighestTargetFramework()
    {
        var tempDirectory = Directory.CreateTempSubdirectory("jolt-razor-sdk-toolset-");
        try
        {
            var razorSdkRoot = Path.Combine(tempDirectory.FullName, "Microsoft.NET.Sdk.Razor");
            var net10Tasks = WriteTaskAssemblyPlaceholder(razorSdkRoot, "net10.0");
            var net11Tasks = WriteTaskAssemblyPlaceholder(razorSdkRoot, "net11.0");

            var resolved = RazorSdkToolsetResolver.ResolveRazorTasksPath(razorSdkRoot);

            Assert.AreEqual(net11Tasks, resolved);
            Assert.AreNotEqual(net10Tasks, resolved);
        }
        finally
        {
            tempDirectory.Delete(recursive: true);
        }
    }

    [TestMethod]
    public void ResolveRazorTasksPath_WithOnlyNet10TaskTargetFramework_SelectsExistingTaskAssembly()
    {
        var tempDirectory = Directory.CreateTempSubdirectory("jolt-razor-sdk-toolset-");
        try
        {
            var razorSdkRoot = Path.Combine(tempDirectory.FullName, "Microsoft.NET.Sdk.Razor");
            var expected = WriteTaskAssemblyPlaceholder(razorSdkRoot, "net10.0");

            var resolved = RazorSdkToolsetResolver.ResolveRazorTasksPath(razorSdkRoot);

            Assert.AreEqual(expected, resolved);
        }
        finally
        {
            tempDirectory.Delete(recursive: true);
        }
    }

    private static string WriteTaskAssemblyPlaceholder(string razorSdkRoot, string targetFramework)
    {
        var taskPath = Path.Combine(razorSdkRoot, "tasks", targetFramework, "Microsoft.NET.Sdk.Razor.Tasks.dll");
        Directory.CreateDirectory(Path.GetDirectoryName(taskPath)!);
        File.WriteAllText(taskPath, targetFramework);
        return taskPath;
    }
}
