namespace Jero.Core;

/// <summary>
/// 文件帮助类
/// </summary>
public static class FileHelper
{
    private static readonly char[] WildcardChars = ['*', '?'];

    /// <summary>
    /// 尝试删除文件/目录
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static bool TryDelete(string path)
    {
        try
        {
            if (string.IsNullOrEmpty(path)) return false;
            if (Directory.Exists(path)) Directory.Delete(path, recursive: true);
            else File.Delete(path);
            return true;
        }
        catch (Exception)
        {
            // ignored
            return false;
        }
    }

    /// <summary>
    /// 复制目录
    /// </summary>
    /// <param name="sourceDir"></param>
    /// <param name="destinationDir"></param>
    /// <param name="overwrite"></param>
    public static void CopyDirectory(string sourceDir, string destinationDir, bool overwrite = false)
    {
        // 检查源目录是否存在
        if (!Directory.Exists(sourceDir)) throw new DirectoryNotFoundException("Source directory not found: " + sourceDir);

        // 如果目标目录不存在，则创建它
        if (!Directory.Exists(destinationDir)) Directory.CreateDirectory(destinationDir!);

        // 获取源目录下的所有文件并复制它们
        foreach (string file in Directory.GetFiles(sourceDir))
        {
            string name = Path.GetFileName(file);
            string dest = Path.Combine(destinationDir, name);
            File.Copy(file, dest, overwrite);
        }

        // 递归复制所有子目录
        foreach (string directory in Directory.GetDirectories(sourceDir))
        {
            string name = Path.GetFileName(directory);
            string dest = Path.Combine(destinationDir, name);
            CopyDirectory(directory, dest, overwrite);
        }
    }

    /// <summary>
    /// 根据相对路径或通配符查找目录下的文件/目录。
    /// </summary>
    /// <param name="rootDir">根目录。</param>
    /// <param name="patterns">相对路径模式，支持 Configuration\*.json、wwwroot\upload、wwwroot\upload\**。</param>
    /// <returns>匹配到的绝对路径。</returns>
    public static IReadOnlyList<string> FindPathsByPatterns(string rootDir, IEnumerable<string>? patterns)
    {
        if (string.IsNullOrWhiteSpace(rootDir) || patterns == null || !Directory.Exists(rootDir)) return [];

        var rootPath = Path.GetFullPath(rootDir);
        var matches = new SortedSet<string>(OperatingSystem.IsWindows()
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal);

        foreach (var pattern in patterns.Select(NormalizePattern).Where(u => !string.IsNullOrWhiteSpace(u)))
        {
            if (pattern.EndsWith($"{Path.DirectorySeparatorChar}**", StringComparison.Ordinal))
            {
                AddExistingPath(matches, Path.Combine(rootPath, pattern[..^3]));
                continue;
            }

            if (pattern.IndexOfAny(WildcardChars) >= 0)
            {
                AddWildcardMatches(rootPath, pattern, matches);
                continue;
            }

            AddExistingPath(matches, Path.Combine(rootPath, pattern));
        }

        return matches.ToList();
    }

    /// <summary>
    /// 根据相对路径或通配符删除目录下的文件/目录。
    /// </summary>
    /// <param name="rootDir">根目录。</param>
    /// <param name="patterns">相对路径模式。</param>
    /// <param name="onDelete">删除前回调。</param>
    public static void DeleteByPatterns(string rootDir, IEnumerable<string>? patterns, Action<string>? onDelete = null)
    {
        foreach (var path in FindPathsByPatterns(rootDir, patterns).OrderByDescending(u => u.Length))
        {
            onDelete?.Invoke(path);
            TryDelete(path);
        }
    }

    private static string NormalizePattern(string? pattern)
    {
        return (pattern ?? string.Empty)
            .Trim()
            .Replace('\\', Path.DirectorySeparatorChar)
            .Replace('/', Path.DirectorySeparatorChar)
            .TrimStart(Path.DirectorySeparatorChar);
    }

    private static void AddWildcardMatches(string rootPath, string pattern, ISet<string> matches)
    {
        var relativeDir = Path.GetDirectoryName(pattern);
        var searchPattern = Path.GetFileName(pattern);
        if (string.IsNullOrWhiteSpace(searchPattern)) return;

        var searchRoot = string.IsNullOrWhiteSpace(relativeDir) ? rootPath : Path.Combine(rootPath, relativeDir);
        if (!Directory.Exists(searchRoot)) return;

        foreach (var filePath in Directory.GetFiles(searchRoot, searchPattern, SearchOption.TopDirectoryOnly))
        {
            AddExistingPath(matches, filePath);
        }

        foreach (var directoryPath in Directory.GetDirectories(searchRoot, searchPattern, SearchOption.TopDirectoryOnly))
        {
            AddExistingPath(matches, directoryPath);
        }
    }

    private static void AddExistingPath(ISet<string> matches, string path)
    {
        var fullPath = Path.GetFullPath(path);
        if (File.Exists(fullPath) || Directory.Exists(fullPath)) matches.Add(fullPath);
    }

    /// <summary>
    /// 在文件倒数第lastIndex个identifier前插入内容（备份原文件）
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <param name="insertContent">要插入的内容</param>
    /// <param name="identifier">标识符号</param>
    /// <param name="lastIndex">倒数第几个标识符</param>
    /// <param name="createBackup">是否创建备份文件</param>
    public static async Task InsertsStringAtSpecifiedLocationInFile(string filePath, string insertContent, char identifier, int lastIndex, bool createBackup = false)
    {
        // 参数校验
        if (lastIndex < 1) throw new ArgumentOutOfRangeException(nameof(lastIndex));
        if (identifier == 0) throw new ArgumentException("标识符不能为空字符");

        if (!File.Exists(filePath))
            throw new FileNotFoundException("目标文件不存在", filePath);

        // 创建备份文件
        if (createBackup)
        {
            string backupPath = $"{filePath}.bak_{DateTime.Now:yyyyMMddHHmmss}";
            File.Copy(filePath, backupPath, true);
        }

        using var reader = new StreamReader(filePath, Encoding.UTF8);
        var content = await reader.ReadToEndAsync();
        reader.Close();
        // 逆向查找算法
        int index = content.LastIndexOf(identifier);
        if (index == -1)
        {
            throw new ArgumentException($"文件中未包含{identifier}");
        }

        int resIndex = content.LastIndexOf(identifier, index - lastIndex);
        if (resIndex == -1)
        {
            throw new ArgumentException($"文件中{identifier}不足{lastIndex}个");
        }

        StringBuilder sb = new StringBuilder(content);
        sb = sb.Insert(resIndex, insertContent);
        await WriteToFileAsync(filePath, sb);
    }

    /// <summary>
    /// 写入文件内容
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="sb"></param>
    public static async Task WriteToFileAsync(string filePath, StringBuilder sb)
    {
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        await using var writer = new StreamWriter(filePath, false, new UTF8Encoding(false)); // 无BOM
        await writer.WriteAsync(sb.ToString());
        writer.Close();
        Console.WriteLine($"文件【{filePath}】写入完成");
        Console.ResetColor();
    }
}
