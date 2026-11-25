namespace Jero.Plugin.SaveLife.Service;

/// <summary>
/// 小程序通知服务
/// </summary>
[ApiDescriptionSettings(SaveLifeConst.GroupName, Order = 170)]
public class SaveLifeNotificationService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<SaveLifeNotification> _notificationRep;
    private readonly ISaveLifePrivacyConsentGuard _privacyConsentGuard;

    public SaveLifeNotificationService(
        SqlSugarRepository<SaveLifeNotification> notificationRep,
        ISaveLifePrivacyConsentGuard privacyConsentGuard)
    {
        _notificationRep = notificationRep;
        _privacyConsentGuard = privacyConsentGuard;
    }

    /// <summary>
    /// 分页查询通知
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<SaveLifeNotification>> Page(SaveLifeNotificationInput input)
    {
        var externalUserId = input.ExternalUserId?.Trim();
        return await _notificationRep.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(externalUserId), u => u.ExternalUserId == externalUserId)
            .WhereIF(!string.IsNullOrWhiteSpace(input.NotificationType), u => u.NotificationType == input.NotificationType)
            .WhereIF(!string.IsNullOrWhiteSpace(input.BusinessType), u => u.BusinessType == input.BusinessType)
            .WhereIF(input.IsRead.HasValue, u => u.IsRead == input.IsRead)
            .OrderBy(u => u.CreateTime, OrderByType.Desc)
            .ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 新增通知
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Add")]
    public async Task<long> Add(AddSaveLifeNotificationInput input)
    {
        var notification = new SaveLifeNotification
        {
            ExternalUserId = NormalizeRequired(input.ExternalUserId, "外部用户Id不能为空"),
            NotificationType = NormalizeOptional(input.NotificationType, 32) ?? "system",
            Title = NormalizeRequired(input.Title, "标题不能为空", 128),
            Content = NormalizeOptional(input.Content, 512),
            BusinessType = NormalizeOptional(input.BusinessType, 32),
            BusinessId = input.BusinessId
        };

        await _notificationRep.InsertAsync(notification);
        return notification.Id;
    }

    /// <summary>
    /// 删除通知
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Delete")]
    public async Task Delete(DeleteSaveLifeNotificationInput input)
    {
        var notification = await _notificationRep.GetByIdAsync(input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await _notificationRep.FakeDeleteAsync(notification);
    }

    /// <summary>
    /// 获取通知详情
    /// </summary>
    public async Task<SaveLifeNotification> GetDetail([FromQuery] QueryByIdSaveLifeNotificationInput input)
    {
        return await _notificationRep.GetByIdAsync(input.Id);
    }

    /// <summary>
    /// 小程序当前用户通知列表
    /// </summary>
    [Authorize]
    [HttpGet("/api/v1/save-life/notifications"), NonUnify]
    [ApiDescriptionSettings(Name = "MiniAppList")]
    public async Task<SqlSugarPagedList<SaveLifeMiniAppNotificationOutput>> MiniAppList([FromQuery] SaveLifeNotificationInput input)
    {
        await _privacyConsentGuard.EnsureAccountActiveAsync();
        var externalUserId = GetCurrentMiniAppOpenId();
        input.PageSize = NormalizePageSize(input.PageSize);
        var page = await _notificationRep.AsQueryable()
            .Where(u => u.ExternalUserId == externalUserId)
            .WhereIF(!string.IsNullOrWhiteSpace(input.NotificationType), u => u.NotificationType == input.NotificationType)
            .WhereIF(!string.IsNullOrWhiteSpace(input.BusinessType), u => u.BusinessType == input.BusinessType)
            .WhereIF(input.IsRead.HasValue, u => u.IsRead == input.IsRead)
            .OrderBy(u => u.IsRead)
            .OrderBy(u => u.CreateTime, OrderByType.Desc)
            .ToPagedListAsync(input.Page, input.PageSize);

        return new SqlSugarPagedList<SaveLifeMiniAppNotificationOutput>
        {
            Page = page.Page,
            PageSize = page.PageSize,
            Total = page.Total,
            TotalPages = page.TotalPages,
            HasNextPage = page.HasNextPage,
            HasPrevPage = page.HasPrevPage,
            Items = page.Items.Select(ToMiniAppOutput).ToList()
        };
    }

    /// <summary>
    /// 小程序当前用户未读数
    /// </summary>
    [Authorize]
    [HttpGet("/api/v1/save-life/notifications/unread-count"), NonUnify]
    [ApiDescriptionSettings(Name = "MiniAppUnreadCount")]
    public async Task<int> MiniAppUnreadCount()
    {
        await _privacyConsentGuard.EnsureAccountActiveAsync();
        var externalUserId = GetCurrentMiniAppOpenId();
        return await _notificationRep.AsQueryable()
            .Where(u => u.ExternalUserId == externalUserId && !u.IsRead)
            .CountAsync();
    }

    /// <summary>
    /// 小程序标记单条通知已读
    /// </summary>
    [Authorize]
    [HttpPost("/api/v1/save-life/notifications/{id}/read"), NonUnify]
    [ApiDescriptionSettings(Name = "MiniAppRead")]
    public async Task<SaveLifeMiniAppNotificationOutput> MiniAppRead(long id)
    {
        await _privacyConsentGuard.EnsureAccountActiveAsync();
        var externalUserId = GetCurrentMiniAppOpenId();
        var notification = await _notificationRep.GetFirstAsync(u => u.Id == id && u.ExternalUserId == externalUserId)
            ?? throw Oops.Oh("通知不存在或不属于当前用户").StatusCode(404);

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadTime = DateTime.Now;
            await _notificationRep.AsUpdateable(notification).IgnoreColumns(true).ExecuteCommandAsync();
        }

        return ToMiniAppOutput(notification);
    }

    /// <summary>
    /// 小程序全部标记已读
    /// </summary>
    [Authorize]
    [HttpPost("/api/v1/save-life/notifications/read-all"), NonUnify]
    [ApiDescriptionSettings(Name = "MiniAppReadAll")]
    public async Task MiniAppReadAll()
    {
        await _privacyConsentGuard.EnsureAccountActiveAsync();
        var externalUserId = GetCurrentMiniAppOpenId();
        await _notificationRep.AsUpdateable()
            .SetColumns(u => new SaveLifeNotification
            {
                IsRead = true,
                ReadTime = DateTime.Now
            })
            .Where(u => u.ExternalUserId == externalUserId && !u.IsRead)
            .ExecuteCommandAsync();
    }

    private static SaveLifeMiniAppNotificationOutput ToMiniAppOutput(SaveLifeNotification notification)
    {
        return new SaveLifeMiniAppNotificationOutput
        {
            Id = notification.Id.ToString(),
            NotificationType = notification.NotificationType,
            Title = notification.Title,
            Content = notification.Content,
            BusinessType = notification.BusinessType,
            BusinessId = notification.BusinessId?.ToString(),
            IsRead = notification.IsRead,
            ReadTime = notification.ReadTime,
            CreateTime = notification.CreateTime
        };
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

    private static string NormalizeRequired(string? value, string errorMessage, int maxLength = 64)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
            throw Oops.Oh(errorMessage).StatusCode(400);

        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
            return null;

        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }
}
