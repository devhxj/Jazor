namespace Jero.Plugin.SaveLife.Service;

/// <summary>
/// 用户收藏服务
/// </summary>
[ApiDescriptionSettings(SaveLifeConst.GroupName, Order = 180)]
public class SaveLifeFavoriteService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<SaveLifeFavorite> _favoriteRep;
    private readonly SqlSugarRepository<SaveLifeClinicalTrial> _trialRep;
    private readonly SqlSugarRepository<SaveLifeDiseaseKnowledge> _knowledgeRep;
    private readonly ISaveLifePrivacyConsentGuard _privacyConsentGuard;

    public SaveLifeFavoriteService(
        SqlSugarRepository<SaveLifeFavorite> favoriteRep,
        SqlSugarRepository<SaveLifeClinicalTrial> trialRep,
        SqlSugarRepository<SaveLifeDiseaseKnowledge> knowledgeRep,
        ISaveLifePrivacyConsentGuard privacyConsentGuard)
    {
        _favoriteRep = favoriteRep;
        _trialRep = trialRep;
        _knowledgeRep = knowledgeRep;
        _privacyConsentGuard = privacyConsentGuard;
    }

    /// <summary>
    /// 分页查询收藏
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<SaveLifeFavorite>> Page(SaveLifeFavoriteInput input)
    {
        var externalUserId = input.ExternalUserId?.Trim();
        var objectType = NormalizeObjectTypeOrNull(input.ObjectType);
        return await _favoriteRep.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(externalUserId), u => u.ExternalUserId == externalUserId)
            .WhereIF(!string.IsNullOrWhiteSpace(objectType), u => u.ObjectType == objectType)
            .OrderBy(u => u.CreateTime, OrderByType.Desc)
            .ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 小程序收藏列表
    /// </summary>
    [Authorize]
    [HttpGet("/api/v1/save-life/favorites"), NonUnify]
    [ApiDescriptionSettings(Name = "MiniAppList")]
    public async Task<SqlSugarPagedList<SaveLifeMiniAppFavoriteOutput>> MiniAppList([FromQuery] SaveLifeFavoriteInput input)
    {
        await _privacyConsentGuard.EnsureAccountActiveAsync();
        var externalUserId = GetCurrentMiniAppOpenId();
        input.PageSize = NormalizePageSize(input.PageSize);
        var objectType = NormalizeObjectTypeOrNull(input.ObjectType);
        var page = await _favoriteRep.AsQueryable()
            .Where(u => u.ExternalUserId == externalUserId)
            .WhereIF(!string.IsNullOrWhiteSpace(objectType), u => u.ObjectType == objectType)
            .OrderBy(u => u.CreateTime, OrderByType.Desc)
            .ToPagedListAsync(input.Page, input.PageSize);

        var outputs = await ToFavoriteOutputs(page.Items?.ToList() ?? new List<SaveLifeFavorite>());
        return new SqlSugarPagedList<SaveLifeMiniAppFavoriteOutput>
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
    /// 小程序收藏对象状态
    /// </summary>
    [Authorize]
    [HttpGet("/api/v1/save-life/favorites/state"), NonUnify]
    [ApiDescriptionSettings(Name = "MiniAppState")]
    public async Task<SaveLifeMiniAppFavoriteStateOutput> MiniAppState([FromQuery] ToggleSaveLifeMiniAppFavoriteInput input)
    {
        await _privacyConsentGuard.EnsureAccountActiveAsync();
        var externalUserId = GetCurrentMiniAppOpenId();
        var objectType = NormalizeObjectType(input.ObjectType);
        await EnsureObjectExists(objectType, input.ObjectId);
        var exists = await _favoriteRep.IsAnyAsync(u =>
            u.ExternalUserId == externalUserId &&
            u.ObjectType == objectType &&
            u.ObjectId == input.ObjectId);

        return new SaveLifeMiniAppFavoriteStateOutput
        {
            ObjectType = objectType,
            ObjectId = input.ObjectId.ToString(),
            IsFavorite = exists
        };
    }

    /// <summary>
    /// 小程序收藏对象，幂等
    /// </summary>
    [Authorize]
    [HttpPost("/api/v1/save-life/favorites"), NonUnify]
    [ApiDescriptionSettings(Name = "MiniAppAdd")]
    public async Task<SaveLifeMiniAppFavoriteStateOutput> MiniAppAdd(ToggleSaveLifeMiniAppFavoriteInput input)
    {
        await _privacyConsentGuard.EnsureAccountActiveAsync();
        var externalUserId = GetCurrentMiniAppOpenId();
        var objectType = NormalizeObjectType(input.ObjectType);
        await EnsureObjectExists(objectType, input.ObjectId);
        var existing = await _favoriteRep.GetFirstAsync(u =>
            u.ExternalUserId == externalUserId &&
            u.ObjectType == objectType &&
            u.ObjectId == input.ObjectId);

        if (existing == null)
        {
            await _favoriteRep.InsertAsync(new SaveLifeFavorite
            {
                ExternalUserId = externalUserId,
                ObjectType = objectType,
                ObjectId = input.ObjectId
            });
        }

        return new SaveLifeMiniAppFavoriteStateOutput
        {
            ObjectType = objectType,
            ObjectId = input.ObjectId.ToString(),
            IsFavorite = true
        };
    }

    /// <summary>
    /// 小程序取消收藏，幂等
    /// </summary>
    [Authorize]
    [HttpDelete("/api/v1/save-life/favorites"), NonUnify]
    [ApiDescriptionSettings(Name = "MiniAppDelete")]
    public async Task<SaveLifeMiniAppFavoriteStateOutput> MiniAppDelete([FromQuery] ToggleSaveLifeMiniAppFavoriteInput input)
    {
        await _privacyConsentGuard.EnsureAccountActiveAsync();
        var externalUserId = GetCurrentMiniAppOpenId();
        var objectType = NormalizeObjectType(input.ObjectType);
        var existing = await _favoriteRep.GetFirstAsync(u =>
            u.ExternalUserId == externalUserId &&
            u.ObjectType == objectType &&
            u.ObjectId == input.ObjectId);

        if (existing != null)
            await _favoriteRep.FakeDeleteAsync(existing);

        return new SaveLifeMiniAppFavoriteStateOutput
        {
            ObjectType = objectType,
            ObjectId = input.ObjectId.ToString(),
            IsFavorite = false
        };
    }

    private async Task<List<SaveLifeMiniAppFavoriteOutput>> ToFavoriteOutputs(List<SaveLifeFavorite> favorites)
    {
        var trialIds = favorites.Where(u => u.ObjectType == "trial").Select(u => u.ObjectId).ToList();
        var knowledgeIds = favorites.Where(u => u.ObjectType == "knowledge").Select(u => u.ObjectId).ToList();
        var trials = trialIds.Count == 0
            ? new List<SaveLifeClinicalTrial>()
            : await _trialRep.AsQueryable().Where(u => trialIds.Contains(u.Id)).ToListAsync();
        var knowledge = knowledgeIds.Count == 0
            ? new List<SaveLifeDiseaseKnowledge>()
            : await _knowledgeRep.AsQueryable().Where(u => knowledgeIds.Contains(u.Id)).ToListAsync();

        return favorites.Select(item =>
        {
            if (item.ObjectType == "trial")
            {
                var trial = trials.FirstOrDefault(u => u.Id == item.ObjectId);
                return new SaveLifeMiniAppFavoriteOutput
                {
                    Id = item.Id.ToString(),
                    ObjectType = item.ObjectType,
                    ObjectId = item.ObjectId.ToString(),
                    Title = trial?.Title ?? $"项目 {item.ObjectId}",
                    Summary = trial?.DiseaseArea ?? trial?.Requirements,
                    Status = trial?.Status,
                    CreateTime = item.CreateTime
                };
            }

            var article = knowledge.FirstOrDefault(u => u.Id == item.ObjectId);
            return new SaveLifeMiniAppFavoriteOutput
            {
                Id = item.Id.ToString(),
                ObjectType = item.ObjectType,
                ObjectId = item.ObjectId.ToString(),
                Title = article?.Title ?? article?.DiseaseName ?? $"知识 {item.ObjectId}",
                Summary = article?.Summary,
                Status = article?.IsPublished == true ? "published" : "offline",
                ImageUrl = article?.ImageUrl,
                CreateTime = item.CreateTime
            };
        }).ToList();
    }

    private async Task EnsureObjectExists(string objectType, long objectId)
    {
        if (objectId <= 0)
            throw Oops.Oh("收藏对象Id不正确").StatusCode(400);

        if (objectType == "trial")
        {
            var exists = await _trialRep.IsAnyAsync(u => u.Id == objectId && u.Status == "recruiting");
            if (!exists)
                throw Oops.Oh("招募项目不存在或已下架").StatusCode(404);
            return;
        }

        var knowledgeExists = await _knowledgeRep.IsAnyAsync(u => u.Id == objectId && u.IsPublished);
        if (!knowledgeExists)
            throw Oops.Oh("疾病知识不存在或已下架").StatusCode(404);
    }

    private static string NormalizeObjectType(string? value)
    {
        var normalized = value?.Trim().ToLowerInvariant();
        return normalized is "trial" or "knowledge"
            ? normalized
            : throw Oops.Oh("收藏对象类型不正确").StatusCode(400);
    }

    private static string? NormalizeObjectTypeOrNull(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return NormalizeObjectType(value);
    }

    private static string GetCurrentMiniAppOpenId()
    {
        var openId = App.User?.FindFirst(ClaimConst.OpenId)?.Value?.Trim();
        if (!string.IsNullOrWhiteSpace(openId))
            return openId;

        throw Oops.Oh("小程序登录态无效，请重新登录").StatusCode(400);
    }

    private static int NormalizePageSize(int pageSize)
    {
        if (pageSize <= 0)
            return 10;

        return Math.Min(pageSize, 50);
    }
}
