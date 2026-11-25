namespace Jero.Core.Service;

public class SSHFileProvider : ICustomFileProvider, ITransient
{
    /// <summary>
    /// 创建SSH连接助手
    /// </summary>
    /// <returns></returns>
    private SSHHelper CreateSSHHelper()
    {
        var host = App.Configuration["SSHProvider:Host"];
        var username = App.Configuration["SSHProvider:Username"];
        var password = App.Configuration["SSHProvider:Password"];
        if (string.IsNullOrWhiteSpace(host)) throw Oops.Oh("SSH存储Host未配置");
        if (string.IsNullOrWhiteSpace(username)) throw Oops.Oh("SSH存储Username未配置");
        if (string.IsNullOrWhiteSpace(password)) throw Oops.Oh("SSH存储Password未配置");

        return new SSHHelper(
            host,
            App.Configuration["SSHProvider:Port"].ToInt(),
            username,
            password);
    }

    /// <summary>
    /// 构建文件完整路径
    /// </summary>
    /// <param name="sysFile"></param>
    /// <returns></returns>
    private string BuildFilePath(SysFile sysFile)
    {
        if (string.IsNullOrWhiteSpace(sysFile.FilePath)) throw Oops.Oh("SSH文件路径为空");
        return string.Concat(sysFile.FilePath, "/", sysFile.Id + sysFile.Suffix);
    }

    public Task DeleteFileAsync(SysFile sysFile)
    {
        var fullPath = BuildFilePath(sysFile);
        using var helper = CreateSSHHelper();
        helper.DeleteFile(fullPath);
        return Task.CompletedTask;
    }

    public Task<string> DownloadFileBase64Async(SysFile sysFile)
    {
        var filePath = BuildFilePath(sysFile);
        using var helper = CreateSSHHelper();
        return Task.FromResult(Convert.ToBase64String(helper.ReadAllBytes(filePath)));
    }

    public Task<FileStreamResult> GetFileStreamResultAsync(SysFile sysFile, string fileName)
    {
        var filePath = BuildFilePath(sysFile);
        using var helper = CreateSSHHelper();
        return Task.FromResult(new FileStreamResult(helper.OpenRead(filePath), "application/octet-stream")
        {
            FileDownloadName = fileName + sysFile.Suffix
        });
    }

    public Task<SysFile> UploadFileAsync(IFormFile file, SysFile sysFile, string path, string finalName)
    {
        var fullPath = string.Concat(path.StartsWith('/') ? path : "/" + path, "/", finalName);
        using var helper = CreateSSHHelper();
        helper.UploadFile(file.OpenReadStream(), fullPath);
        return Task.FromResult(sysFile);
    }
}
