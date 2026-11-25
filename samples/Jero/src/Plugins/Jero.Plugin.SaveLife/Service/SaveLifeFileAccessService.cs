// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace Jero.Plugin.SaveLife.Service;

/// <summary>
/// 医线生机已授权业务数据的关联文件访问器。
/// </summary>
/// <remarks>
/// 调用方必须先完成报名记录、材料或用户资料的归属和权限校验。本服务只负责绕过平台文件表的数据范围，
/// 使 SaveLife 的全局业务数据能够读取其关联文件。
/// </remarks>
public interface ISaveLifeFileAccessService
{
    Task<IActionResult> DownloadAsync(long fileId);

    Task<Stream> OpenReadAsync(long fileId);
}

public sealed class SaveLifeFileAccessService(
    SqlSugarRepository<SysFile> fileRep,
    SysFileService fileService) : ISaveLifeFileAccessService, ITransient
{
    private readonly SqlSugarRepository<SysFile> _fileRep = fileRep;
    private readonly SysFileService _fileService = fileService;

    public async Task<IActionResult> DownloadAsync(long fileId)
    {
        var file = await GetFileAsync(fileId);
        return await _fileService.DownloadLoadedFile(file);
    }

    public async Task<Stream> OpenReadAsync(long fileId)
    {
        var file = await GetFileAsync(fileId);
        return await _fileService.GetFileStream(file);
    }

    private async Task<SysFile> GetFileAsync(long fileId)
    {
        if (fileId <= 0)
            throw Oops.Oh(ErrorCodeEnum.D8000);

        return await _fileRep.AsQueryable()
            .ClearFilter()
            .Where(file => file.Id == fileId)
            .FirstAsync()
            ?? throw Oops.Oh(ErrorCodeEnum.D8000);
    }
}
