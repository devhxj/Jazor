namespace Jero.Core.Service;

public class DefaultFileProvider : ICustomFileProvider, ITransient
{
    /// <summary>
    /// 构建文件的完整物理路径
    /// </summary>
    /// <param name="sysFile"></param>
    /// <returns></returns>
    private string BuildFullFilePath(SysFile sysFile)
    {
        return Path.Combine(App.WebHostEnvironment.WebRootPath, sysFile.FilePath ?? "", $"{sysFile.Id}{sysFile.Suffix}");
    }

    /// <summary>
    /// 构建目录的完整物理路径
    /// </summary>
    /// <param name="relativePath"></param>
    /// <returns></returns>
    private string BuildFullDirectoryPath(string relativePath)
    {
        return Path.Combine(App.WebHostEnvironment.WebRootPath, relativePath);
    }

    /// <summary>
    /// 确保目录存在
    /// </summary>
    /// <param name="directoryPath"></param>
    private void EnsureDirectoryExists(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
            Directory.CreateDirectory(directoryPath);
    }

    public Task DeleteFileAsync(SysFile sysFile)
    {
        var filePath = BuildFullFilePath(sysFile);
        if (File.Exists(filePath))
            File.Delete(filePath);
        return Task.CompletedTask;
    }

    public async Task<string> DownloadFileBase64Async(SysFile sysFile)
    {
        var realFile = BuildFullFilePath(sysFile);
        if (!File.Exists(realFile))
        {
            Log.Error($"DownloadFileBase64:文件[{realFile}]不存在");
            throw Oops.Oh($"文件[{sysFile.FilePath}]不存在");
        }

        byte[] fileBytes = await File.ReadAllBytesAsync(realFile);
        return Convert.ToBase64String(fileBytes);
    }

    public Task<FileStreamResult> GetFileStreamResultAsync(SysFile sysFile, string fileName)
    {
        var fullPath = BuildFullFilePath(sysFile);
        return Task.FromResult(new FileStreamResult(new FileStream(fullPath, FileMode.Open), "application/octet-stream")
        {
            FileDownloadName = fileName + sysFile.Suffix
        });
    }

    public async Task<SysFile> UploadFileAsync(IFormFile file, SysFile newFile, string path, string finalName)
    {
        newFile.Provider = ""; // 本地存储 Provider 显示为空

        var directoryPath = BuildFullDirectoryPath(path);
        EnsureDirectoryExists(directoryPath);

        var realFile = Path.Combine(directoryPath, finalName);
        await using var stream = File.Create(realFile);
        await file.CopyToAsync(stream);

        newFile.Url = $"{newFile.FilePath}/{newFile.Id + newFile.Suffix}";
        return newFile;
    }
}