namespace Jero.Plugin.SaveLife.Service;

/// <summary>
/// 疾病知识服务
/// </summary>
[ApiDescriptionSettings(SaveLifeConst.GroupName, Order = 130)]
public class SaveLifeDiseaseKnowledgeService : IDynamicApiController, ITransient
{
    private const string AllowedContentMediaSuffixes = ".jpg.jpeg.png.gif.mp4";

    private readonly SqlSugarRepository<SaveLifeDiseaseKnowledge> _knowledgeRep;
    private readonly SqlSugarRepository<SaveLifeDiseaseCategory> _categoryRep;
    private readonly SqlSugarRepository<SaveLifeKnowledgeReadHistory> _readHistoryRep;
    private readonly SqlSugarRepository<SaveLifeKnowledgeLike> _likeRep;
    private readonly SysFileService _fileService;
    private readonly ISaveLifePublicKnowledgeBaseIndexProcessor _knowledgeBaseIndexProcessor;

    public SaveLifeDiseaseKnowledgeService(
        SqlSugarRepository<SaveLifeDiseaseKnowledge> knowledgeRep,
        SqlSugarRepository<SaveLifeDiseaseCategory> categoryRep,
        SqlSugarRepository<SaveLifeKnowledgeReadHistory> readHistoryRep,
        SqlSugarRepository<SaveLifeKnowledgeLike> likeRep,
        SysFileService fileService,
        ISaveLifePublicKnowledgeBaseIndexProcessor knowledgeBaseIndexProcessor)
    {
        _knowledgeRep = knowledgeRep;
        _categoryRep = categoryRep;
        _readHistoryRep = readHistoryRep;
        _likeRep = likeRep;
        _fileService = fileService;
        _knowledgeBaseIndexProcessor = knowledgeBaseIndexProcessor;
    }

    /// <summary>
    /// 分页查询疾病知识
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<SaveLifeDiseaseKnowledge>> Page(SaveLifeDiseaseKnowledgeInput input)
    {
        var keyword = input.Keyword?.Trim();
        var contentCategory = NormalizeContentCategoryFilter(input.ContentCategory);
        var page = await _knowledgeRep.AsQueryable()
            .WhereIF(input.CategoryId > 0, u => u.CategoryId == input.CategoryId)
            .WhereIF(contentCategory == SaveLifeKnowledgeContentCategory.Science, u =>
                u.ContentCategory == null || u.ContentCategory == string.Empty ||
                u.ContentCategory == SaveLifeKnowledgeContentCategory.Science)
            .WhereIF(contentCategory != null && contentCategory != SaveLifeKnowledgeContentCategory.Science,
                u => u.ContentCategory == contentCategory)
            .WhereIF(input.IsPublished.HasValue, u => u.IsPublished == input.IsPublished)
            .WhereIF(!string.IsNullOrWhiteSpace(input.DiseaseName), u => u.DiseaseName != null && u.DiseaseName.Contains(input.DiseaseName!.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(keyword), u =>
                (u.Title != null && u.Title.Contains(keyword!)) ||
                (u.Author != null && u.Author.Contains(keyword!)) ||
                (u.Summary != null && u.Summary.Contains(keyword!)) ||
                (u.DiseaseName != null && u.DiseaseName.Contains(keyword!)))
            .OrderBy(u => u.SortOrder)
            .OrderBy(u => u.CreateTime, OrderByType.Desc)
            .ToPagedListAsync(input.Page, input.PageSize);

        foreach (var item in page.Items ?? [])
        {
            item.Content = SaveLifeKnowledgeContentSanitizer.Normalize(item.Content);
        }

        return page;
    }

    /// <summary>
    /// 新增疾病知识
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Add")]
    public async Task<long> Add(AddSaveLifeDiseaseKnowledgeInput input)
    {
        input.ContentCategory = NormalizeContentCategoryForWrite(input.ContentCategory);
        var entity = input.Adapt<SaveLifeDiseaseKnowledge>();
        entity.Content = SaveLifeKnowledgeContentSanitizer.Normalize(
            input.Content,
            GetPublicMediaOrigin());
        await _knowledgeRep.InsertAsync(entity);
        await _knowledgeBaseIndexProcessor.QueueKnowledgeAsync(entity.Id);
        return entity.Id;
    }

    /// <summary>
    /// 更新疾病知识
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Update")]
    public async Task Update(UpdateSaveLifeDiseaseKnowledgeInput input)
    {
        input.ContentCategory = NormalizeContentCategoryForWrite(input.ContentCategory);
        var entity = input.Adapt<SaveLifeDiseaseKnowledge>();
        await _knowledgeRep.AsUpdateable(entity)
            .IgnoreColumns(true)
            .IgnoreColumns(u => new { u.Content })
            .ExecuteCommandAsync();

        var latest = await _knowledgeRep.GetByIdAsync(input.Id);
        if (latest != null)
            await _knowledgeBaseIndexProcessor.QueueKnowledgeAsync(latest.Id);
    }

    /// <summary>
    /// 单独更新疾病知识正文
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "UpdateContent")]
    public async Task UpdateContent(UpdateSaveLifeDiseaseKnowledgeContentInput input)
    {
        var content = SaveLifeKnowledgeContentSanitizer.Normalize(
            input.Content,
            GetPublicMediaOrigin());
        var knowledge = await _knowledgeRep.GetByIdAsync(input.Id)
            ?? throw Oops.Oh("疾病知识不存在").StatusCode(404);

        knowledge.Content = content;
        knowledge.UpdateTime = DateTime.Now;
        await _knowledgeRep.AsUpdateable(knowledge)
            .IgnoreColumns(true)
            .UpdateColumns(u => new { u.Content, u.UpdateTime })
            .ExecuteCommandAsync();

        await _knowledgeBaseIndexProcessor.QueueKnowledgeAsync(knowledge.Id);
    }

    /// <summary>
    /// 上传疾病知识正文媒体
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "UploadContentMedia")]
    public async Task<SaveLifeKnowledgeMediaOutput> UploadContentMedia([FromForm] UploadSaveLifeKnowledgeMediaInput input)
    {
        var file = NormalizeMediaFile(input.File);
        var uploadedFile = await _fileService.UploadFile(new UploadFileInput
        {
            File = file,
            AllowSuffix = AllowedContentMediaSuffixes,
            FileType = "save-life-knowledge-content",
            IsPublic = true,
            DataId = input.KnowledgeId
        }, "upload/save-life/knowledge");

        var mediaUrl = SaveLifeKnowledgeContentSanitizer.NormalizeMediaSourceUrl(
            uploadedFile.Url,
            GetPublicMediaOrigin());
        if (string.IsNullOrWhiteSpace(mediaUrl))
            throw Oops.Oh("媒体上传成功，但未生成公开预览地址，请检查文件存储提供者配置").StatusCode(500);

        return new SaveLifeKnowledgeMediaOutput
        {
            Id = uploadedFile.Id.ToString(),
            Url = mediaUrl,
            FileName = BuildDisplayFileName(uploadedFile.FileName, uploadedFile.Suffix),
            FileSuffix = uploadedFile.Suffix,
            ContentType = file.ContentType,
            MediaType = ResolveMediaType(uploadedFile.Suffix, file.ContentType),
            SizeKb = uploadedFile.SizeKb,
            SizeInfo = uploadedFile.SizeInfo ?? SaveLifeEnrollmentMaterialHelper.FormatSize(uploadedFile.SizeKb)
        };
    }

    /// <summary>
    /// 删除疾病知识
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Delete")]
    public async Task Delete(DeleteSaveLifeDiseaseKnowledgeInput input)
    {
        var entity = await _knowledgeRep.GetByIdAsync(input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await _knowledgeBaseIndexProcessor.RemoveKnowledgeAsync(entity.Id);
        await _knowledgeRep.FakeDeleteAsync(entity);
    }

    /// <summary>
    /// 发布或下架疾病知识
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Publish")]
    public async Task Publish(PublishSaveLifeDiseaseKnowledgeInput input)
    {
        await _knowledgeRep.AsUpdateable()
            .SetColumns(u => new SaveLifeDiseaseKnowledge { IsPublished = input.IsPublished })
            .Where(u => u.Id == input.Id)
            .ExecuteCommandAsync();

        var latest = await _knowledgeRep.GetByIdAsync(input.Id);
        if (latest != null)
            await _knowledgeBaseIndexProcessor.QueueKnowledgeAsync(latest.Id);
    }

    /// <summary>
    /// 获取疾病知识详情
    /// </summary>
    public async Task<SaveLifeDiseaseKnowledge> GetDetail([FromQuery] QueryByIdSaveLifeDiseaseKnowledgeInput input)
    {
        var detail = await _knowledgeRep.GetByIdAsync(input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        detail.Content = SaveLifeKnowledgeContentSanitizer.Normalize(detail.Content);

        return detail;
    }

    /// <summary>
    /// 小程序分页查询疾病知识库
    /// </summary>
    [AllowAnonymous]
    [HttpGet("/api/v1/save-life/knowledge"), NonUnify]
    [ApiDescriptionSettings(Name = "MiniAppList")]
    public async Task<SqlSugarPagedList<SaveLifeMiniAppKnowledgeOutput>> MiniAppList([FromQuery] SaveLifeDiseaseKnowledgeInput input)
    {
        input.PageSize = NormalizePageSize(input.PageSize);
        var keyword = input.Keyword?.Trim();
        var contentCategory = NormalizeContentCategoryFilter(input.ContentCategory);

        var page = await _knowledgeRep.AsQueryable()
            .Where(u => u.IsPublished)
            .WhereIF(input.CategoryId > 0, u => u.CategoryId == input.CategoryId)
            .WhereIF(contentCategory == SaveLifeKnowledgeContentCategory.Science, u =>
                u.ContentCategory == null || u.ContentCategory == string.Empty ||
                u.ContentCategory == SaveLifeKnowledgeContentCategory.Science)
            .WhereIF(contentCategory != null && contentCategory != SaveLifeKnowledgeContentCategory.Science,
                u => u.ContentCategory == contentCategory)
            .WhereIF(!string.IsNullOrWhiteSpace(input.DiseaseName), u => u.DiseaseName != null && u.DiseaseName.Contains(input.DiseaseName!.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(keyword), u =>
                (u.Title != null && u.Title.Contains(keyword!)) ||
                (u.Summary != null && u.Summary.Contains(keyword!)) ||
                (u.DiseaseName != null && u.DiseaseName.Contains(keyword!)) ||
                (u.Tags != null && u.Tags.Contains(keyword!)))
            .OrderBy(u => u.SortOrder)
            .OrderBy(u => u.CreateTime, OrderByType.Desc)
            .ToPagedListAsync(input.Page, input.PageSize);

        var outputs = await ToMiniAppKnowledgeOutputs(page.Items?.ToList() ?? new List<SaveLifeDiseaseKnowledge>(), false);
        return new SqlSugarPagedList<SaveLifeMiniAppKnowledgeOutput>
        {
            Page = page.Page,
            PageSize = page.PageSize,
            Total = page.Total,
            TotalPages = page.TotalPages,
            HasNextPage = page.HasNextPage,
            HasPrevPage = page.HasPrevPage,
            Items = outputs
        };
    }

    /// <summary>
    /// 小程序获取疾病知识详情
    /// </summary>
    [AllowAnonymous]
    [HttpGet("/api/v1/save-life/knowledge/{id}"), NonUnify]
    [ApiDescriptionSettings(Name = "MiniAppDetail")]
    public async Task<SaveLifeMiniAppKnowledgeOutput> MiniAppDetail(long id)
    {
        var knowledge = await _knowledgeRep.GetFirstAsync(u => u.Id == id && u.IsPublished)
            ?? throw Oops.Oh("疾病知识不存在或已下架").StatusCode(404);

        await _knowledgeRep.AsUpdateable()
            .SetColumns(u => new SaveLifeDiseaseKnowledge { ViewCount = u.ViewCount + 1 })
            .Where(u => u.Id == id)
            .ExecuteCommandAsync();

        knowledge.ViewCount += 1;
        await RecordReadHistoryIfAuthenticated(id);
        var output = (await ToMiniAppKnowledgeOutputs(new List<SaveLifeDiseaseKnowledge> { knowledge }, true)).First();
        output.Related = await LoadRelatedKnowledge(knowledge);
        return output;
    }

    /// <summary>
    /// 小程序疾病知识点赞
    /// </summary>
    [AllowAnonymous]
    [HttpPost("/api/v1/save-life/knowledge/{id}/like"), NonUnify]
    [ApiDescriptionSettings(Name = "MiniAppLike")]
    public async Task<SaveLifeMiniAppKnowledgeOutput> MiniAppLike(long id, [FromBody] LikeSaveLifeKnowledgeInput? input = null)
    {
        var knowledge = await _knowledgeRep.GetFirstAsync(u => u.Id == id && u.IsPublished)
            ?? throw Oops.Oh("疾病知识不存在或已下架").StatusCode(404);

        var likeIdentity = BuildLikeIdentity(input?.ClientLikeKey);
        var isNewLike = await TryInsertKnowledgeLike(id, likeIdentity);
        if (isNewLike)
        {
            await _knowledgeRep.AsUpdateable()
                .SetColumns(u => new SaveLifeDiseaseKnowledge { LikeCount = u.LikeCount + 1 })
                .Where(u => u.Id == id)
                .ExecuteCommandAsync();

            knowledge.LikeCount += 1;
        }

        return (await ToMiniAppKnowledgeOutputs(
            new List<SaveLifeDiseaseKnowledge> { knowledge },
            false,
            likeIdentity.LikeKey)).First();
    }

    /// <summary>
    /// 小程序当前用户疾病知识阅读历史
    /// </summary>
    [Authorize]
    [HttpGet("/api/v1/save-life/knowledge/read-history"), NonUnify]
    [ApiDescriptionSettings(Name = "MiniAppReadHistory")]
    public async Task<SqlSugarPagedList<SaveLifeMiniAppKnowledgeReadHistoryOutput>> MiniAppReadHistory([FromQuery] SaveLifeKnowledgeReadHistoryInput input)
    {
        var externalUserId = GetCurrentMiniAppOpenId();
        input.PageSize = NormalizePageSize(input.PageSize);

        var page = await _readHistoryRep.AsQueryable()
            .Where(u => u.ExternalUserId == externalUserId)
            .OrderBy(u => u.LastReadTime, OrderByType.Desc)
            .ToPagedListAsync(input.Page, input.PageSize);

        var histories = page.Items?.ToList() ?? new List<SaveLifeKnowledgeReadHistory>();
        var knowledgeIds = histories.Select(u => u.KnowledgeId).Distinct().ToList();
        var knowledge = knowledgeIds.Count == 0
            ? new List<SaveLifeDiseaseKnowledge>()
            : await _knowledgeRep.AsQueryable()
                .Where(u => knowledgeIds.Contains(u.Id) && u.IsPublished)
                .ToListAsync();

        return new SqlSugarPagedList<SaveLifeMiniAppKnowledgeReadHistoryOutput>
        {
            Page = page.Page,
            PageSize = page.PageSize,
            Total = page.Total,
            TotalPages = page.TotalPages,
            HasNextPage = page.HasNextPage,
            HasPrevPage = page.HasPrevPage,
            Items = histories.Select(item =>
            {
                var article = knowledge.FirstOrDefault(u => u.Id == item.KnowledgeId);
                return new SaveLifeMiniAppKnowledgeReadHistoryOutput
                {
                    Id = item.Id.ToString(),
                    KnowledgeId = item.KnowledgeId.ToString(),
                    Title = article?.Title ?? $"知识 {item.KnowledgeId}",
                    DiseaseName = article?.DiseaseName,
                    Summary = article?.Summary,
                    ImageUrl = article?.ImageUrl,
                    ReadCount = item.ReadCount,
                    LastReadTime = item.LastReadTime
                };
            }).ToList()
        };
    }

    private async Task<List<SaveLifeMiniAppKnowledgeOutput>> ToMiniAppKnowledgeOutputs(
        List<SaveLifeDiseaseKnowledge> knowledge,
        bool includeContent,
        string? currentLikeKey = null)
    {
        if (knowledge.Count == 0)
            return new List<SaveLifeMiniAppKnowledgeOutput>();

        var likedIds = new HashSet<long>();
        if (!string.IsNullOrWhiteSpace(currentLikeKey))
        {
            var knowledgeIds = knowledge.Select(u => u.Id).ToList();
            likedIds = (await _likeRep.AsQueryable()
                    .Where(u => knowledgeIds.Contains(u.KnowledgeId) && u.LikeKey == currentLikeKey)
                    .Select(u => u.KnowledgeId)
                    .ToListAsync())
                .ToHashSet();
        }

        return knowledge.Select(item =>
        {
            return new SaveLifeMiniAppKnowledgeOutput
            {
                Id = item.Id.ToString(),
                CategoryId = item.CategoryId?.ToString(),
                CategoryName = SaveLifeKnowledgeContentCategory.GetLabel(item.ContentCategory),
                ContentCategory = SaveLifeKnowledgeContentCategory.NormalizeOrDefault(item.ContentCategory),
                Title = item.Title,
                Author = item.Author,
                DiseaseName = item.DiseaseName,
                Content = includeContent ? SaveLifeKnowledgeContentSanitizer.Normalize(item.Content) : null,
                Summary = item.Summary,
                ImageUrl = item.ImageUrl,
                Tags = item.Tags,
                ViewCount = item.ViewCount,
                LikeCount = item.LikeCount,
                IsLiked = likedIds.Contains(item.Id),
                CreateTime = item.CreateTime,
                UpdateTime = item.UpdateTime
            };
        }).ToList();
    }

    private async Task<bool> TryInsertKnowledgeLike(long knowledgeId, KnowledgeLikeIdentity identity)
    {
        var existing = await _likeRep.GetFirstAsync(u => u.KnowledgeId == knowledgeId && u.LikeKey == identity.LikeKey);
        if (existing != null)
            return false;

        try
        {
            await _likeRep.InsertAsync(new SaveLifeKnowledgeLike
            {
                KnowledgeId = knowledgeId,
                ExternalUserId = identity.ExternalUserId,
                LikeKey = identity.LikeKey,
                SourceType = identity.SourceType
            });
            return true;
        }
        catch
        {
            existing = await _likeRep.GetFirstAsync(u => u.KnowledgeId == knowledgeId && u.LikeKey == identity.LikeKey);
            if (existing != null)
                return false;

            throw;
        }
    }

    private static string? NormalizeContentCategoryFilter(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Equals("all", StringComparison.OrdinalIgnoreCase))
            return null;

        if (SaveLifeKnowledgeContentCategory.TryNormalize(value, out var normalized))
            return normalized;

        throw Oops.Oh("资讯分类只能为科普、罕见病或新药资讯").StatusCode(400);
    }

    private static string NormalizeContentCategoryForWrite(string? value)
    {
        if (SaveLifeKnowledgeContentCategory.TryNormalize(value, out var normalized))
            return normalized;

        throw Oops.Oh("资讯分类只能为科普、罕见病或新药资讯").StatusCode(400);
    }

    private static KnowledgeLikeIdentity BuildLikeIdentity(string? clientLikeKey)
    {
        var openId = App.User?.FindFirst(ClaimConst.OpenId)?.Value?.Trim();
        if (!string.IsNullOrWhiteSpace(openId))
        {
            return new KnowledgeLikeIdentity("user", openId, $"user:{SaveLifeUploadFileValidator.ComputeStableHash(openId)}");
        }

        var normalizedClientKey = NormalizeClientLikeKey(clientLikeKey);
        if (!string.IsNullOrWhiteSpace(normalizedClientKey))
        {
            return new KnowledgeLikeIdentity("client", null, $"client:{SaveLifeUploadFileValidator.ComputeStableHash(normalizedClientKey)}");
        }

        var remoteIp = App.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var bucket = DateTime.UtcNow.ToString("yyyyMMddHH", CultureInfo.InvariantCulture);
        var ipHash = SaveLifeUploadFileValidator.ComputeStableHash($"{remoteIp}:{bucket}");
        return new KnowledgeLikeIdentity("ip", null, $"ip:{ipHash}");
    }

    private static string? NormalizeClientLikeKey(string? value)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
            return null;

        if (normalized.Length is < 16 or > 128 || normalized.Any(char.IsControl))
            throw Oops.Oh("点赞标识不正确").StatusCode(400);

        return normalized;
    }

    private async Task<List<SaveLifeMiniAppKnowledgeOutput>> LoadRelatedKnowledge(SaveLifeDiseaseKnowledge knowledge)
    {
        var tags = SplitTags(knowledge.Tags);
        var related = await _knowledgeRep.AsQueryable()
            .Where(u => u.IsPublished && u.Id != knowledge.Id)
            .WhereIF(knowledge.CategoryId.HasValue, u => u.CategoryId == knowledge.CategoryId)
            .WhereIF(!knowledge.CategoryId.HasValue && !string.IsNullOrWhiteSpace(knowledge.DiseaseName), u =>
                u.DiseaseName != null && u.DiseaseName.Contains(knowledge.DiseaseName!))
            .OrderBy(u => u.SortOrder)
            .OrderBy(u => u.ViewCount, OrderByType.Desc)
            .OrderBy(u => u.CreateTime, OrderByType.Desc)
            .Take(6)
            .ToListAsync();

        if (tags.Count > 0 && related.Count < 6)
        {
            var tagCandidates = await _knowledgeRep.AsQueryable()
                .Where(u => u.IsPublished && u.Id != knowledge.Id)
                .Where(u => u.Tags != null)
                .OrderBy(u => u.SortOrder)
                .OrderBy(u => u.ViewCount, OrderByType.Desc)
                .OrderBy(u => u.CreateTime, OrderByType.Desc)
                .Take(30)
                .ToListAsync();
            var tagRelated = tagCandidates
                .Where(u => tags.Any(tag => u.Tags?.Contains(tag) == true))
                .Take(6)
                .ToList();

            related = tagRelated
                .Concat(related)
                .GroupBy(u => u.Id)
                .Select(u => u.First())
                .Take(6)
                .ToList();
        }

        return await ToMiniAppKnowledgeOutputs(related, false);
    }

    private async Task RecordReadHistoryIfAuthenticated(long knowledgeId)
    {
        var externalUserId = TryGetCurrentMiniAppOpenId();
        if (string.IsNullOrWhiteSpace(externalUserId))
            return;

        var existing = await _readHistoryRep.GetFirstAsync(u => u.ExternalUserId == externalUserId && u.KnowledgeId == knowledgeId);
        if (existing == null)
        {
            await _readHistoryRep.InsertAsync(new SaveLifeKnowledgeReadHistory
            {
                ExternalUserId = externalUserId,
                KnowledgeId = knowledgeId,
                ReadCount = 1,
                LastReadTime = DateTime.Now
            });
            return;
        }

        existing.ReadCount += 1;
        existing.LastReadTime = DateTime.Now;
        await _readHistoryRep.AsUpdateable(existing).IgnoreColumns(true).ExecuteCommandAsync();
    }

    private static string? TryGetCurrentMiniAppOpenId()
    {
        return App.User?.FindFirst(ClaimConst.OpenId)?.Value?.Trim();
    }

    private static string GetCurrentMiniAppOpenId()
    {
        var openId = TryGetCurrentMiniAppOpenId();
        if (!string.IsNullOrWhiteSpace(openId))
            return openId;

        throw Oops.Oh("小程序登录态无效，请重新登录").StatusCode(400);
    }

    private static string GetPublicMediaOrigin()
    {
        var request = App.HttpContext?.Request
            ?? throw Oops.Oh("当前请求缺少 HTTP 上下文").StatusCode(500);
        var virtualPath = string.IsNullOrWhiteSpace(App.Settings.VirtualPath)
            ? string.Empty
            : App.Settings.VirtualPath.TrimEnd('/');
        return $"{request.Scheme}://{request.Host}{virtualPath}";
    }

    private static List<string> SplitTags(string? tags)
    {
        if (string.IsNullOrWhiteSpace(tags))
            return new List<string>();

        return tags
            .Split(new[] { ',', '，', ';', '；', '|', '/', '、', '[', ']', '"', '\'' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(u => u.Trim())
            .Where(u => !string.IsNullOrWhiteSpace(u))
            .Distinct()
            .Take(8)
            .ToList();
    }

    private static int NormalizePageSize(int pageSize)
    {
        if (pageSize <= 0)
            return 10;

        return Math.Min(pageSize, 50);
    }

    private static IFormFile NormalizeMediaFile(IFormFile? file)
    {
        if (file == null || file.Length <= 0)
            throw Oops.Oh("媒体文件不能为空").StatusCode(400);

        if (file.Length > 50 * 1024 * 1024)
            throw Oops.Oh("媒体文件不能超过50MB").StatusCode(413);

        var fileName = Path.GetFileName(file.FileName?.Trim() ?? string.Empty);
        if (string.IsNullOrWhiteSpace(fileName))
            throw Oops.Oh("媒体文件名不能为空").StatusCode(400);

        if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            throw Oops.Oh("媒体文件名不正确").StatusCode(400);

        var extension = NormalizeMediaExtension(fileName);
        if (!IsAllowedMediaExtension(extension))
            throw Oops.Oh("仅支持 jpg、png、gif 图片或 mp4 视频").StatusCode(400);

        var contentType = SaveLifeUploadFileValidator.ValidateAndResolveContentType(file, extension);
        if (contentType == file.ContentType)
            return file;

        return new FormFile(file.OpenReadStream(), 0, file.Length, file.Name, fileName)
        {
            Headers = file.Headers ?? new HeaderDictionary(),
            ContentType = contentType
        };
    }

    private static string NormalizeMediaExtension(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension is ".jpeg" or ".jpe" ? ".jpg" : extension;
    }

    private static bool IsAllowedMediaExtension(string extension)
    {
        return extension is ".jpg" or ".png" or ".gif" or ".mp4";
    }

    private static string ResolveMediaType(string? extension, string? contentType)
    {
        var normalizedExtension = string.IsNullOrWhiteSpace(extension) ? string.Empty : extension.ToLowerInvariant();
        if (normalizedExtension == ".mp4" || contentType?.StartsWith("video/", StringComparison.OrdinalIgnoreCase) == true)
            return "video";

        return "image";
    }

    private static string BuildDisplayFileName(string? fileName, string? suffix)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return "正文媒体";

        return !string.IsNullOrWhiteSpace(suffix) && !fileName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)
            ? $"{fileName}{suffix}"
            : fileName;
    }

    private sealed record KnowledgeLikeIdentity(string SourceType, string? ExternalUserId, string LikeKey);
}
