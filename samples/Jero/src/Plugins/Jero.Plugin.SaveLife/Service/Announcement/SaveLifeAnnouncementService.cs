namespace Jero.Plugin.SaveLife.Service;

/// <summary>
/// 小程序公告服务
/// </summary>
[ApiDescriptionSettings(SaveLifeConst.GroupName, Order = 190)]
public class SaveLifeAnnouncementService(SqlSugarRepository<SaveLifeAnnouncement> announcementRep) : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<SaveLifeAnnouncement> _announcementRep = announcementRep;

	/// <summary>
	/// 分页查询公告
	/// </summary>
	[HttpPost]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<SaveLifeAnnouncement>> Page(SaveLifeAnnouncementInput input)
    {
        var keyword = input.Keyword?.Trim();
        return await _announcementRep.AsQueryable()
            .WhereIF(input.IsPublished.HasValue, u => u.IsPublished == input.IsPublished)
            .WhereIF(!string.IsNullOrWhiteSpace(input.AnnouncementType), u => u.AnnouncementType == input.AnnouncementType)
            .WhereIF(!string.IsNullOrWhiteSpace(keyword), u =>
                (u.Title != null && u.Title.Contains(keyword!)) ||
                (u.Content != null && u.Content.Contains(keyword!)))
            .OrderBy(u => u.SortOrder)
            .OrderBy(u => u.CreateTime, OrderByType.Desc)
            .ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 新增公告
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Add")]
    public async Task<long> Add(AddSaveLifeAnnouncementInput input)
    {
        var entity = input.Adapt<SaveLifeAnnouncement>();
        Normalize(entity);
        await _announcementRep.InsertAsync(entity);
        return entity.Id;
    }

    /// <summary>
    /// 更新公告
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Update")]
    public async Task Update(UpdateSaveLifeAnnouncementInput input)
    {
        var entity = input.Adapt<SaveLifeAnnouncement>();
        Normalize(entity);
        await _announcementRep.AsUpdateable(entity).IgnoreColumns(true).ExecuteCommandAsync();
    }

    /// <summary>
    /// 删除公告
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Delete")]
    public async Task Delete(DeleteSaveLifeAnnouncementInput input)
    {
        var entity = await _announcementRep.GetByIdAsync(input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await _announcementRep.FakeDeleteAsync(entity);
    }

    /// <summary>
    /// 获取公告详情
    /// </summary>
    public async Task<SaveLifeAnnouncement> GetDetail([FromQuery] QueryByIdSaveLifeAnnouncementInput input)
    {
        return await _announcementRep.GetByIdAsync(input.Id);
    }

    /// <summary>
    /// 小程序公告列表
    /// </summary>
    [AllowAnonymous]
    [HttpGet("/api/v1/save-life/announcements"), NonUnify]
    [ApiDescriptionSettings(Name = "MiniAppList")]
    public async Task<List<SaveLifeMiniAppAnnouncementOutput>> MiniAppList()
    {
        var list = await _announcementRep.AsQueryable()
            .Where(u => u.IsPublished)
            .OrderBy(u => u.SortOrder)
            .OrderBy(u => u.CreateTime, OrderByType.Desc)
            .Take(5)
            .ToListAsync();

        return [.. list.Select(ToMiniAppOutput)];
    }

    private static SaveLifeMiniAppAnnouncementOutput ToMiniAppOutput(SaveLifeAnnouncement announcement)
    {
        return new SaveLifeMiniAppAnnouncementOutput
        {
            Id = announcement.Id.ToString(),
            Title = announcement.Title,
            Content = announcement.Content,
            AnnouncementType = announcement.AnnouncementType,
            BusinessType = announcement.BusinessType,
            BusinessId = announcement.BusinessId?.ToString(),
            CreateTime = announcement.CreateTime
        };
    }

    private static void Normalize(SaveLifeAnnouncement entity)
    {
        entity.Title = LimitRequired(entity.Title, 128, "公告标题不能为空");
        entity.Content = LimitOptional(entity.Content, 512);
        entity.AnnouncementType = LimitOptional(entity.AnnouncementType, 32) ?? "system";
        entity.BusinessType = LimitOptional(entity.BusinessType, 32);
    }

    private static string LimitRequired(string? value, int maxLength, string errorMessage)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
            throw Oops.Oh(errorMessage).StatusCode(400);

        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }

    private static string? LimitOptional(string? value, int maxLength)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
            return null;

        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }
}
