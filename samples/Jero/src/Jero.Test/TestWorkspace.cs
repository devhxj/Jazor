using System.Runtime.CompilerServices;

namespace Jero.Test;

internal static class TestWorkspace
{
    // Test runners and file-based browser checks may start in different working directories.
    // Locate the checkout from the compiled source, including when running from a test copy.
    internal static string Create(string suite) => Directory.CreateDirectory(Path.Combine(RepositoryRoot(), "test", "jero", suite,
        Guid.NewGuid().ToString("N"))).FullName;

    internal static string RepositoryRoot([CallerFilePath] string source = "")
    {
        for (var root = new DirectoryInfo(Path.GetDirectoryName(source)!); root is not null; root = root.Parent)
        {
            if (File.Exists(Path.Combine(root.FullName, "Jazor.slnx")))
                return root.FullName;
        }

        throw new DirectoryNotFoundException($"Cannot locate Jazor.slnx above '{source}'.");
    }
}
