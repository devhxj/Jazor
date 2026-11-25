


using Aliyun.OSS.Util;
using Furion.AspNetCore;
using Microsoft.AspNetCore.Mvc.ModelBinding;
namespace Jero.Core.Service;

/// <summary>
/// 系统文件服务 🧩
/// </summary>
[ApiDescriptionSettings(Order = 410, Description = "系统文件")]
public class SysFileService : IDynamicApiController, ITransient
{
    private readonly UserManager _userManager;
    private readonly SqlSugarRepository<SysFile> _sysFileRep;
    private readonly OSSProviderOptions _OSSProviderOptions;
    private readonly UploadOptions _uploadOptions;
    private readonly IConfiguration _configuration;
    private readonly string _imageType = ".jpeg.jpg.png.bmp.gif.tif";
    private readonly INamedServiceProvider<ICustomFileProvider> _namedServiceProvider;
    private readonly ICustomFileProvider _customFileProvider;

    public SysFileService(UserManager userManager,
        SqlSugarRepository<SysFile> sysFileRep,
        IOptions<OSSProviderOptions> oSSProviderOptions,
        IOptions<UploadOptions> uploadOptions,
        INamedServiceProvider<ICustomFileProvider> namedServiceProvider,
        IConfiguration configuration)
    {
        _namedServiceProvider = namedServiceProvider;
        _userManager = userManager;
        _sysFileRep = sysFileRep;
        _OSSProviderOptions = oSSProviderOptions.Value;
        _uploadOptions = uploadOptions.Value;
        _configuration = configuration;

        // 简化提供者选择逻辑
        if (_OSSProviderOptions.Enabled || _configuration["MultiOSS:Enabled"].ToBoolean())
        {
            // 统一使用MultiOSSFileProvider处理所有OSS情况
            _customFileProvider = _namedServiceProvider.GetService<ITransient>(nameof(MultiOSSFileProvider));
        }
        else if (_configuration["SSHProvider:Enabled"].ToBoolean())
        {
            _customFileProvider = _namedServiceProvider.GetService<ITransient>(nameof(SSHFileProvider));
        }
        else
        {
            _customFileProvider = _namedServiceProvider.GetService<ITransient>(nameof(DefaultFileProvider));
        }
    }

    /// <summary>
    /// 获取文件分页列表 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取文件分页列表")]
    public async Task<SqlSugarPagedList<SysFile>> Page(PageFileInput input)
    {
        var fileName = input.FileName?.Trim() ?? string.Empty;
        var filePath = input.FilePath?.Trim() ?? string.Empty;
        var hasDateRange = input.StartTime.HasValue && input.EndTime.HasValue;
        var startTime = input.StartTime ?? DateTime.MinValue;
        var endTime = input.EndTime ?? DateTime.MaxValue;

        // 获取所有公开附件
        var publicList = _sysFileRep.AsQueryable().ClearFilter().Where(u => u.IsPublic == true);
        // 获取私有附件
        var privateList = _sysFileRep.AsQueryable().Where(u => u.IsPublic == false);
        // 合并公开和私有附件并分页
        return await _sysFileRep.Context.UnionAll(publicList, privateList)
            .WhereIF(!string.IsNullOrWhiteSpace(fileName), u => !string.IsNullOrWhiteSpace(u.FileName) && u.FileName.Contains(fileName))
            .WhereIF(!string.IsNullOrWhiteSpace(filePath), u => !string.IsNullOrWhiteSpace(u.FilePath) && u.FilePath.Contains(filePath))
            .WhereIF(hasDateRange, u => u.CreateTime >= startTime && u.CreateTime <= endTime)
            .OrderBy(u => u.CreateTime, OrderByType.Desc)
            .ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 上传文件Base64 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("上传文件Base64")]
    public async Task<SysFile> UploadFileFromBase64(UploadFileFromBase64Input input)
    {
        var pattern = @"data:(?<type>.+?);base64,(?<data>[^""]+)";
        var regex = new Regex(pattern, RegexOptions.Compiled);
        var match = regex.Match(input.FileDataBase64);
        if (!match.Success) throw Oops.Oh("Base64文件格式不正确");

        byte[] fileData;
        try
        {
            fileData = Convert.FromBase64String(match.Groups["data"].Value);
        }
        catch (FormatException)
        {
            throw Oops.Oh("Base64文件内容无效");
        }
        var contentType = match.Groups["type"].Value;
        if (string.IsNullOrEmpty(input.FileName))
            input.FileName = $"{YitIdHelper.NextId()}.{contentType.AsSpan(contentType.LastIndexOf('/') + 1)}";

        using var ms = new MemoryStream();
        ms.Write(fileData);
        ms.Seek(0, SeekOrigin.Begin);
        IFormFile formFile = new FormFile(ms, 0, fileData.Length, "file", input.FileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
        var uploadFileInput = input.Adapt<UploadFileInput>();
        uploadFileInput.File = formFile;
        return await UploadFile(uploadFileInput);
    }

    /// <summary>
    /// 上传多文件 🔖
    /// </summary>
    /// <param name="files"></param>
    /// <returns></returns>
    [DisplayName("上传多文件")]
    public async Task<List<SysFile>> UploadFiles([Required] List<IFormFile> files)
    {
        var fileList = new List<SysFile>();
        foreach (var file in files)
        {
            var uploadedFile = await UploadFile(new UploadFileInput { File = file });
            fileList.Add(uploadedFile);
        }
        return fileList;
    }

    /// <summary>
    /// 根据文件Id或Url下载 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("根据文件Id或Url下载")]
    public async Task<IActionResult> DownloadFile(SysFile input)
    {
        var file = input.Id > 0
            ? await GetFile(input.Id)
            : (await _sysFileRep.CopyNew().GetFirstAsync(u => u.Url == input.Url) ?? throw Oops.Oh("文件不存在"));
        var fileName = GetDownloadFileName(file);
        return await GetFileStreamResult(file, fileName);
    }

    /// <summary>
    /// 使用已经完成业务授权校验的文件记录下载文件
    /// </summary>
    /// <remarks>
    /// 调用方必须先验证业务归属和访问权限。此方法不会重新查询文件记录或应用数据范围筛选。
    /// </remarks>
    [NonAction]
    public Task<IActionResult> DownloadLoadedFile(SysFile file)
    {
        ArgumentNullException.ThrowIfNull(file);
        if (file.Id <= 0)
            throw Oops.Oh(ErrorCodeEnum.D8000);

        return GetFileStreamResult(file, GetDownloadFileName(file));
    }

    /// <summary>
    /// 文件预览 🔖
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [DisplayName("文件预览")]
    public async Task<IActionResult> GetPreview([FromRoute] long id)
    {
        var file = await GetFile(id);
        //var fileName = HttpUtility.UrlEncode(file.FileName, Encoding.GetEncoding("UTF-8"));
        return await GetFileStreamResult(file, file.Id + "");
    }

    /// <summary>
    /// 获取文件流
    /// </summary>
    /// <param name="file"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    private async Task<IActionResult> GetFileStreamResult(SysFile file, string fileName)
    {
        return await _customFileProvider.GetFileStreamResultAsync(file, fileName);
    }

    /// <summary>
    /// 获取文件流
    /// </summary>
    [NonAction]
    public async Task<Stream> GetFileStream(SysFile file)
    {
        var resolvedFile = ShouldReloadFileRecord(file) ? await GetFile(file.Id) : file;
        var fileName = GetDownloadFileName(resolvedFile);
        var result = await _customFileProvider.GetFileStreamResultAsync(resolvedFile, fileName);
        return result.FileStream;
    }

    private static bool ShouldReloadFileRecord(SysFile file)
    {
        return file.Id > 0 &&
            (string.IsNullOrWhiteSpace(file.FilePath) ||
                string.IsNullOrWhiteSpace(file.Suffix) ||
                string.IsNullOrWhiteSpace(file.BucketName));
    }

    /// <summary>
    /// 下载指定文件Base64格式 🔖
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    [DisplayName("下载指定文件Base64格式")]
    public async Task<string> DownloadFileBase64([FromBody] string url)
    {
        var sysFile = await _sysFileRep.CopyNew().GetFirstAsync(u => u.Url == url) ?? throw Oops.Oh($"文件不存在");
        return await _customFileProvider.DownloadFileBase64Async(sysFile);
    }

    /// <summary>
    /// 删除文件 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Delete"), HttpPost]
    [DisplayName("删除文件")]
    public async Task DeleteFile(BaseIdInput input)
    {
        if (!await TryDeleteFile(input.Id))
            throw Oops.Oh($"文件不存在");
    }

    [NonAction]
    public Task<bool> TryDeleteFile(long fileId)
    {
        return SysFileStorageConsistency.DeleteIfExistsAsync(
            async () => (SysFile?)await _sysFileRep.GetByIdAsync(fileId),
            _customFileProvider.DeleteFileAsync,
            item => _sysFileRep.DeleteAsync(item));
    }

    /// <summary>
    /// 更新文件 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Update"), HttpPost]
    [DisplayName("更新文件")]
    public async Task UpdateFile(SysFile input)
    {
        var isExist = await _sysFileRep.IsAnyAsync(u => u.Id == input.Id);
        if (!isExist) throw Oops.Oh(ErrorCodeEnum.D8000);

        await _sysFileRep.UpdateAsync(input);
    }

    /// <summary>
    /// 获取文件 🔖
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [DisplayName("获取文件")]
    public async Task<SysFile> GetFile([FromQuery] long id)
    {
        var file = await _sysFileRep.CopyNew().GetByIdAsync(id);
        return file ?? throw Oops.Oh(ErrorCodeEnum.D8000);
    }

    /// <summary>
    /// 根据文件Id集合获取文件 🔖
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    [DisplayName("根据文件Id集合获取文件")]
    public async Task<List<SysFile>> GetFileByIds([FromQuery][FlexibleArray<long>] List<long> ids)
    {
        return await _sysFileRep.AsQueryable().Where(u => ids.Contains(u.Id)).ToListAsync();
    }

    /// <summary>
    /// 获取文件路径 🔖
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取文件路径")]
    public async Task<List<TreeNode>> GetFolder()
    {
        // 优化：直接在数据库层面获取不重复的文件路径
        var folders = await _sysFileRep.AsQueryable()
            .Select(u => u.FilePath)
            .Distinct()
            .ToListAsync();

        var folderPaths = new List<string>();
        foreach (var folder in folders)
        {
            if (!string.IsNullOrWhiteSpace(folder)) folderPaths.Add(folder);
        }

        var pathTreeBuilder = new PathTreeBuilder();
        var tree = pathTreeBuilder.BuildTree(folderPaths);
        return tree.Children;
    }

    private static string GetDownloadFileName(SysFile file)
    {
        var rawFileName = string.IsNullOrWhiteSpace(file.FileName) ? file.Id.ToString() : file.FileName;
        return HttpUtility.UrlEncode(rawFileName, Encoding.UTF8) ?? rawFileName;
    }

    /// <summary>
    /// 上传文件 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <param name="targetPath">存储目标路径</param>
    /// <returns></returns>
    [DisplayName("上传文件")]
    public async Task<SysFile> UploadFile([FromForm] UploadFileInput input, [BindNever] string targetPath = "")
    {
        var file = input.File;
        if (file == null || file.Length <= 0) throw Oops.Oh(ErrorCodeEnum.D8000);

        if (file.FileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) throw Oops.Oh(ErrorCodeEnum.D8005);

        // 判断是否重复上传的文件
        var sizeKb = file.Length / 1024; // 大小KB
        var fileMd5 = string.Empty;
        if (_uploadOptions.EnableMd5)
        {
            await using (var fileStream = file.OpenReadStream())
            {
                fileMd5 = OssUtils.ComputeContentMd5(fileStream, fileStream.Length);
            }
            // Mysql8 中如果使用了 utf8mb4_general_ci 之外的编码会出错，尽量避免在条件里使用.ToString()
            // 因为 Squsugar 并不是把变量转换为字符串来构造SQL语句，而是构造了CAST(123 AS CHAR)这样的语句，这样这个返回值是utf8mb4_general_ci，所以容易出错。
            var sysFile = await _sysFileRep.GetFirstAsync(u => u.FileMd5 == fileMd5 && u.SizeKb == sizeKb);
            if (sysFile != null) return sysFile;
        }

        // 验证文件类型
        if (!_uploadOptions.ContentType.Contains(file.ContentType)) throw Oops.Oh($"{ErrorCodeEnum.D8001}:{file.ContentType}");

        // 验证文件大小
        if (sizeKb > _uploadOptions.MaxSize) throw Oops.Oh($"{ErrorCodeEnum.D8002}，允许最大：{_uploadOptions.MaxSize}KB");

        // 获取文件后缀
        var suffix = Path.GetExtension(file.FileName).ToLower(); // 后缀
        if (string.IsNullOrWhiteSpace(suffix))
            suffix = string.Concat(".", file.ContentType.AsSpan(file.ContentType.LastIndexOf('/') + 1));
        if (!string.IsNullOrWhiteSpace(suffix))
        {
            //var contentTypeProvider = FS.GetFileExtensionContentTypeProvider();
            //suffix = contentTypeProvider.Mappings.FirstOrDefault(u => u.Value == file.ContentType).Key;
            // 修改 image/jpeg 类型返回的 .jpeg、jpe 后缀
            if (suffix == ".jpeg" || suffix == ".jpe")
                suffix = ".jpg";
        }
        if (string.IsNullOrWhiteSpace(suffix)) throw Oops.Oh(ErrorCodeEnum.D8003);

        // 防止客户端伪造文件类型
        if (!string.IsNullOrWhiteSpace(input.AllowSuffix) && !input.AllowSuffix.Contains(suffix)) throw Oops.Oh(ErrorCodeEnum.D8003);
        //if (!VerifyFileExtensionName.IsSameType(file.OpenReadStream(), suffix)) throw Oops.Oh(ErrorCodeEnum.D8001);

        // 文件存储位置
        var path = string.IsNullOrWhiteSpace(targetPath) ? input.FilePath : targetPath;
        path = string.IsNullOrWhiteSpace(path) ? _uploadOptions.Path : path;
        path = path.ParseToDateTimeForRep();

        var newFile = input.Adapt<SysFile>();
        newFile.Id = YitIdHelper.NextId();

        // 优先使用用户指定的存储桶名称，如果没有指定则使用默认配置
        if (!string.IsNullOrEmpty(input.BucketName))
        {
            newFile.BucketName = input.BucketName;
        }
        else
        {
            // MultiOSSFileProvider会自动使用默认配置
            newFile.BucketName = _OSSProviderOptions.Enabled ? _OSSProviderOptions.Bucket : "Local";
        }

        newFile.FileName = Path.GetFileNameWithoutExtension(file.FileName);
        newFile.Suffix = suffix;
        newFile.SizeKb = sizeKb;
        newFile.FilePath = path;
        newFile.FileMd5 = fileMd5;
        newFile.DataId = input.DataId;

        var finalName = newFile.Id + suffix; // 文件最终名称

        return await SysFileStorageConsistency.UploadAsync(
            () => _customFileProvider.UploadFileAsync(file, newFile, path, finalName),
            item => _sysFileRep.AsInsertable(item).ExecuteCommandAsync(),
            _customFileProvider.DeleteFileAsync);
    }

    /// <summary>
    /// 上传头像 🔖
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    [DisplayName("上传头像")]
    public async Task<SysFile> UploadAvatar([Required] IFormFile file)
    {
        var sysFile = await UploadFile(new UploadFileInput { File = file, AllowSuffix = _imageType }, "upload/avatar");

        var sysUserRep = _sysFileRep.ChangeRepository<SqlSugarRepository<SysUser>>();
        var user = await sysUserRep.GetByIdAsync(_userManager.UserId);
        await sysUserRep.UpdateAsync(u => new SysUser() { Avatar = sysFile.Url }, u => u.Id == user.Id);
        // 删除已有头像文件
        if (!string.IsNullOrWhiteSpace(user.Avatar))
        {
            var fileId = Path.GetFileNameWithoutExtension(user.Avatar);
            if (long.TryParse(fileId, out var id))
            {
                try
                {
                    await DeleteFile(new BaseIdInput { Id = id });
                }
                catch
                {
                    // 忽略删除旧头像文件的错误，不影响新头像上传
                }
            }
        }

        return sysFile;
    }

    /// <summary>
    /// 上传电子签名 🔖
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    [DisplayName("上传电子签名")]
    public async Task<SysFile> UploadSignature([Required] IFormFile file)
    {
        var sysFile = await UploadFile(new UploadFileInput { File = file, AllowSuffix = _imageType }, "upload/signature");

        var sysUserRep = _sysFileRep.ChangeRepository<SqlSugarRepository<SysUser>>();
        var user = await sysUserRep.GetByIdAsync(_userManager.UserId);
        // 删除已有电子签名文件
        if (!string.IsNullOrWhiteSpace(user.Signature) && user.Signature.EndsWith(".png"))
        {
            var fileId = Path.GetFileNameWithoutExtension(user.Signature);
            if (long.TryParse(fileId, out var id))
            {
                try
                {
                    await DeleteFile(new BaseIdInput { Id = id });
                }
                catch
                {
                    // 忽略删除旧签名文件的错误，不影响新签名上传
                }
            }
        }
        await sysUserRep.UpdateAsync(u => new SysUser() { Signature = sysFile.Url }, u => u.Id == user.Id);
        return sysFile;
    }

    #region 统一实体与文件关联时，业务应用实体只需要定义一个SysFile集合导航属性，业务增加和更新、删除分别调用即可

    /// <summary>
    /// 更新文件的业务数据Id
    /// </summary>
    /// <param name="dataId"></param>
    /// <param name="sysFiles"></param>
    /// <returns></returns>
    [NonAction]
    public async Task UpdateFileByDataId(long dataId, List<SysFile> sysFiles)
    {
        var newFileIds = sysFiles.Select(u => u.Id).ToList();

        // 求文件Id差集并删除（无效文件）
        var tmpFiles = await _sysFileRep.GetListAsync(u => u.DataId == dataId);
        var tmpFileIds = tmpFiles.Select(u => u.Id).ToList();
        var deleteFileIds = tmpFileIds.Except(newFileIds);
        foreach (var fileId in deleteFileIds)
            await DeleteFile(new BaseIdInput() { Id = fileId });

        await _sysFileRep.UpdateAsync(u => new SysFile() { DataId = dataId }, u => newFileIds.Contains(u.Id));
    }

    /// <summary>
    /// 删除业务数据对应的文件
    /// </summary>
    /// <param name="dataId"></param>
    /// <returns></returns>
    [NonAction]
    public async Task DeleteFileByDataId(long dataId)
    {
        // 删除冗余无效的物理文件
        var tmpFiles = await _sysFileRep.GetListAsync(u => u.DataId == dataId);
        foreach (var file in tmpFiles)
            await _customFileProvider.DeleteFileAsync(file);
        await _sysFileRep.AsDeleteable().Where(u => u.DataId == dataId).ExecuteCommandAsync();
    }

    #endregion 统一实体与文件关联时，业务应用实体只需要定义一个SysFile集合导航属性，业务增加和更新、删除分别调用即可
}
