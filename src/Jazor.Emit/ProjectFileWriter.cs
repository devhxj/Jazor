using System.Text;

namespace Jazor.Emit;

/// <summary>Publishes one project file without exposing partial content or touching unchanged files.</summary>
internal static class ProjectFileWriter
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(encoderShouldEmitUTF8Identifier: false);

    public static void Write(string path, string content)
    {
        if (File.Exists(path) && File.ReadAllText(path, Utf8WithoutBom) == content)
            return;

        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        var temporary = path + ".jazor-tmp-" + Guid.NewGuid().ToString("N");
        try
        {
            File.WriteAllText(temporary, content, Utf8WithoutBom);
            // A watcher or SSR worker must observe a complete file even during an incremental build.
            File.Move(temporary, path, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporary))
                File.Delete(temporary);
        }
    }
}
