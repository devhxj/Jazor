namespace Jero.Core.Service;

/// <summary>
/// 多OSS文件提供者
/// </summary>
public class MultiOSSFileProvider : ICustomFileProvider, ITransient
{
    private readonly SysFileProviderService _fileProviderService;
    private readonly IOSSServiceManager _ossServiceManager;
    private readonly OSSProviderOptions _ossProviderOptions;

    public MultiOSSFileProvider(SysFileProviderService fileProviderService,
        IOSSServiceManager ossServiceManager,
        IOptions<OSSProviderOptions> ossProviderOptions)
    {
        _fileProviderService = fileProviderService;
        _ossServiceManager = ossServiceManager;
        _ossProviderOptions = ossProviderOptions.Value;
    }

    /// <summary>
    /// 上传文件
    /// </summary>
    /// <param name="file">文件</param>
    /// <param name="sysFile">系统文件信息</param>
    /// <param name="path">文件存储位置</param>
    /// <param name="finalName">文件最终名称</param>
    /// <returns></returns>
    public async Task<SysFile> UploadFileAsync(IFormFile file, SysFile sysFile, string path, string finalName)
    {
        // 获取OSS配置（传入文件信息用于策略选择）
        var provider = await GetFileProvider(sysFile, file) ?? throw Oops.Oh("未找到可用的存储提供者配置");

        // 获取OSS服务
        var ossService = await _ossServiceManager.GetOSSServiceAsync(provider);

        // 设置文件信息
        sysFile.Provider = provider.Provider;
        sysFile.BucketName = provider.BucketName; // 保存原始存储桶名称

        var filePath = string.Concat(path, "/", finalName);

        // 上传文件
        await ossService.PutObjectAsync(provider.BucketName, filePath, file.OpenReadStream());

        // 生成外链地址
        sysFile.Url = GenerateFileUrl(provider, provider.BucketName, filePath);

        return sysFile;
    }

    /// <summary>
    /// 删除文件
    /// </summary>
    /// <param name="sysFile">系统文件信息</param>
    /// <returns></returns>
    public async Task DeleteFileAsync(SysFile sysFile)
    {
        // 获取OSS配置（统一方法）
        var provider = await GetFileProvider(sysFile) ?? throw Oops.Oh($"未找到存储提供者配置: {sysFile.Provider}-{sysFile.BucketName}");
        var ossService = await _ossServiceManager.GetOSSServiceAsync(provider);
        var filePath = string.Concat(sysFile.FilePath, "/", $"{sysFile.Id}{sysFile.Suffix}");

        await ossService.RemoveObjectAsync(provider.BucketName, filePath);
    }

    /// <summary>
    /// 获取文件流
    /// </summary>
    /// <param name="sysFile">系统文件信息</param>
    /// <param name="fileName">文件名</param>
    /// <returns></returns>
    public async Task<FileStreamResult> GetFileStreamResultAsync(SysFile sysFile, string fileName)
    {
        // 获取OSS配置（统一方法）
        var provider = await GetFileProvider(sysFile) ?? throw Oops.Oh($"未找到存储提供者配置: {sysFile.Provider}-{sysFile.BucketName}");
        var ossService = await _ossServiceManager.GetOSSServiceAsync(provider);
        var filePath = Path.Combine(sysFile.FilePath ?? "", sysFile.Id + sysFile.Suffix);

        var httpRemoteService = App.GetRequiredService<IHttpRemoteService>();
        var stream = await httpRemoteService.GetAsStreamAsync(await ossService.PresignedGetObjectAsync(provider.BucketName, filePath, 5));
        if (stream == null) throw Oops.Oh("获取OSS文件流失败");

        return new FileStreamResult(stream, "application/octet-stream") { FileDownloadName = fileName + sysFile.Suffix };
    }

    /// <summary>
    /// 下载文件Base64格式
    /// </summary>
    /// <param name="sysFile">系统文件信息</param>
    /// <returns></returns>
    public async Task<string> DownloadFileBase64Async(SysFile sysFile)
    {
        using var httpClient = new HttpClient();
        if (string.IsNullOrWhiteSpace(sysFile.Url)) throw Oops.Oh("文件URL为空");

        HttpResponseMessage response = await httpClient.GetAsync(sysFile.Url);
        if (response.IsSuccessStatusCode)
        {
            byte[] fileBytes = await response.Content.ReadAsByteArrayAsync();
            return Convert.ToBase64String(fileBytes);
        }
        throw Oops.Oh($"下载文件失败，状态码: {response.StatusCode}");
    }

    /// <summary>
    /// 获取文件提供者配置（统一方法，支持上传、删除、下载场景）
    /// </summary>
    /// <param name="sysFile">系统文件信息</param>
    /// <param name="file">上传的文件（可选，仅上传时传入）</param>
    /// <returns></returns>
    private async Task<SysFileProvider?> GetFileProvider(SysFile sysFile, IFormFile? file = null)
    {
        // 1. 如果已指定存储桶，直接使用
        if (!string.IsNullOrEmpty(sysFile.BucketName))
        {
            var provider = !string.IsNullOrWhiteSpace(sysFile.Provider)
                ? await _fileProviderService.GetFileProviderByBucket(sysFile.Provider, sysFile.BucketName)
                : await _fileProviderService.GetProviderByBucketName(sysFile.BucketName);
            if (provider != null) return provider;

            // 如果数据库中找不到，尝试配置文件兜底
            if (_ossProviderOptions.Enabled && _ossProviderOptions.Bucket == sysFile.BucketName)
            {
                return await CreateProviderFromConfiguration();
            }
        }

        // 2. 如果有上传文件信息，使用策略选择（仅上传场景）
        if (file != null)
        {
            var uploadInput = new UploadFileInput
            {
                File = file,
                BucketName = sysFile.BucketName,
                FileType = sysFile.FileType ?? string.Empty
            };

            return await SelectProviderAsync(file, uploadInput);
        }

        // 3. 最后兜底：使用默认存储提供者
        return await _fileProviderService.GetDefaultProvider();
    }

    /// <summary>
    /// 选择合适的OSS存储提供者（内联版本）
    /// </summary>
    /// <param name="file">上传的文件</param>
    /// <param name="input">上传输入参数</param>
    /// <returns></returns>
    private async Task<SysFileProvider?> SelectProviderAsync(IFormFile file, UploadFileInput input)
    {
        // 1. 优先使用指定的提供者ID
        if (input.ProviderId.HasValue)
        {
            var provider = await _fileProviderService.GetFileProviderById(input.ProviderId.Value);
            if (provider != null) return provider;
        }

        // 2. 其次使用指定的存储桶名称
        if (!string.IsNullOrEmpty(input.BucketName))
        {
            var providers = await _fileProviderService.GetCachedFileProviders();
            var provider = providers.FirstOrDefault(p => p.BucketName == input.BucketName);
            if (provider != null) return provider;
        }

        // 3. 使用默认提供者
        var defaultProvider = await _fileProviderService.GetDefaultProvider();
        if (defaultProvider != null) return defaultProvider;

        // 4. 兜底：如果数据库中没有配置，尝试从配置文件创建默认提供者
        return await CreateProviderFromConfiguration();
    }

    /// <summary>
    /// 生成文件URL（内联版本）
    /// </summary>
    /// <param name="provider">存储提供者配置</param>
    /// <param name="bucketName">存储桶名称</param>
    /// <param name="filePath">文件路径</param>
    /// <returns></returns>
    private static string GenerateFileUrl(SysFileProvider provider, string bucketName, string filePath)
    {
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentException.ThrowIfNullOrWhiteSpace(bucketName);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var protocol = provider.IsEnableHttps == true ? "https" : "http";

        // 如果有自定义域名，直接使用
        if (!string.IsNullOrWhiteSpace(provider.SinceDomain))
        {
            return $"{provider.SinceDomain.TrimEnd('/')}/{filePath.TrimStart('/')}";
        }

        // 根据不同提供者生成URL
        return provider.Provider.ToUpper() switch
        {
            "ALIYUN" => $"{protocol}://{bucketName}.{provider.Endpoint}/{filePath.TrimStart('/')}",
            "QCLOUD" => $"{protocol}://{bucketName}-{provider.Endpoint}.cos.{provider.Region}.myqcloud.com/{filePath.TrimStart('/')}",
            "MINIO" => $"{protocol}://{provider.Endpoint}/{bucketName}/{filePath.TrimStart('/')}",
            _ => throw Oops.Oh($"不支持的OSS提供者: {provider.Provider}")
        };
    }

    /// <summary>
    /// 从配置文件创建默认提供者（兜底机制）
    /// </summary>
    /// <returns></returns>
    private Task<SysFileProvider?> CreateProviderFromConfiguration()
    {
        try
        {
            // 检查是否启用了OSS配置
            if (!_ossProviderOptions.Enabled && !App.Configuration["MultiOSS:Enabled"].ToBoolean())
                return Task.FromResult<SysFileProvider?>(null);

            // 验证必要配置
            if (string.IsNullOrWhiteSpace(_ossProviderOptions.AccessKey) ||
                string.IsNullOrWhiteSpace(_ossProviderOptions.SecretKey) ||
                string.IsNullOrWhiteSpace(_ossProviderOptions.Bucket))
            {
                return Task.FromResult<SysFileProvider?>(null);
            }

            // 使用现有的OSSProviderOptions创建临时提供者配置（不保存到数据库）
            var provider = new SysFileProvider
            {
                Id = 0, // 临时ID
                Provider = Enum.GetName(_ossProviderOptions.Provider) ?? throw Oops.Oh("OSS提供者配置无效"),
                BucketName = _ossProviderOptions.Bucket,
                AccessKey = _ossProviderOptions.AccessKey,
                SecretKey = _ossProviderOptions.SecretKey,
                Endpoint = _ossProviderOptions.Endpoint,
                Region = _ossProviderOptions.Region,
                IsEnableHttps = _ossProviderOptions.IsEnableHttps,
                IsEnableCache = _ossProviderOptions.IsEnableCache,
                IsEnable = true,
                IsDefault = true,
                SinceDomain = _ossProviderOptions.CustomHost,
                CreateTime = DateTime.Now
            };

            return Task.FromResult<SysFileProvider?>(provider);
        }
        catch (Exception)
        {
            // 配置读取失败，返回null
            return Task.FromResult<SysFileProvider?>(null);
        }
    }
}
