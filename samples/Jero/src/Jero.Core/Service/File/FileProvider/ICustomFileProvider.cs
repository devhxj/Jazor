namespace Jero.Core.Service;

/// <summary>
/// 自定义文件提供器接口
/// </summary>
public interface ICustomFileProvider
{
    /// <summary>
    /// 获取文件流
    /// </summary>
    /// <param name="sysFile"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public Task<FileStreamResult> GetFileStreamResultAsync(SysFile sysFile, string fileName);

    /// <summary>
    /// 下载指定文件Base64格式
    /// </summary>
    /// <param name="sysFile"></param>
    /// <returns></returns>
    public Task<string> DownloadFileBase64Async(SysFile sysFile);

    /// <summary>
    /// 删除文件
    /// </summary>
    /// <param name="sysFile"></param>
    /// <returns></returns>
    public Task DeleteFileAsync(SysFile sysFile);

    /// <summary>
    /// 上传文件
    /// </summary>
    /// <param name="file">文件</param>
    /// <param name="sysFile"></param>
    /// <param name="path">文件存储位置</param>
    /// <param name="finalName">文件最终名称</param>
    /// <returns></returns>
    public Task<SysFile> UploadFileAsync(IFormFile file, SysFile sysFile, string path, string finalName);
}