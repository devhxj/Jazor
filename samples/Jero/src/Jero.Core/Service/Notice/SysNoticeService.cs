namespace Jero.Core.Service;

/// <summary>
/// 系统通知公告服务 🧩
/// </summary>
[ApiDescriptionSettings(Order = 380, Description = "通知公告")]
public class SysNoticeService : IDynamicApiController, ITransient
{
    private readonly UserManager _userManager;
    private readonly SqlSugarRepository<SysUser> _sysUserRep;
    private readonly SqlSugarRepository<SysNotice> _sysNoticeRep;
    private readonly SqlSugarRepository<SysNoticeUser> _sysNoticeUserRep;
    private readonly SysOnlineUserService _sysOnlineUserService;

    public SysNoticeService(
        UserManager userManager,
        SqlSugarRepository<SysUser> sysUserRep,
        SqlSugarRepository<SysNotice> sysNoticeRep,
        SqlSugarRepository<SysNoticeUser> sysNoticeUserRep,
        SysOnlineUserService sysOnlineUserService)
    {
        _userManager = userManager;
        _sysUserRep = sysUserRep;
        _sysNoticeRep = sysNoticeRep;
        _sysNoticeUserRep = sysNoticeUserRep;
        _sysOnlineUserService = sysOnlineUserService;
    }

    /// <summary>
    /// 获取通知公告分页列表 📢
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取通知公告分页列表")]
    public async Task<SqlSugarPagedList<SysNotice>> Page(PageNoticeInput input)
    {
        var title = input.Title?.Trim() ?? string.Empty;

        return await _sysNoticeRep.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(title), u => !string.IsNullOrWhiteSpace(u.Title) && u.Title.Contains(title))
            .WhereIF(input.Type > 0, u => u.Type == input.Type)
            .WhereIF(!_userManager.SuperAdmin, u => u.CreateUserId == _userManager.UserId)
            .OrderBy(u => u.CreateTime, OrderByType.Desc)
            .ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 增加通知公告 📢
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Add"), HttpPost]
    [DisplayName("增加通知公告")]
    public async Task AddNotice(AddNoticeInput input)
    {
        var notice = input.Adapt<SysNotice>();
        InitNoticeInfo(notice);
        await _sysNoticeRep.InsertAsync(notice);
    }

    /// <summary>
    /// 更新通知公告 📢
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [UnitOfWork]
    [ApiDescriptionSettings(Name = "Update"), HttpPost]
    [DisplayName("更新通知公告")]
    public async Task UpdateNotice(UpdateNoticeInput input)
    {
        if (input.CreateUserId != _userManager.UserId)
            throw Oops.Oh(ErrorCodeEnum.D7003);

        var notice = input.Adapt<SysNotice>();
        InitNoticeInfo(notice);
        await _sysNoticeRep.UpdateAsync(notice);
    }

    /// <summary>
    /// 删除通知公告 📢
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [UnitOfWork]
    [ApiDescriptionSettings(Name = "Delete"), HttpPost]
    [DisplayName("删除通知公告")]
    public async Task DeleteNotice(DeleteNoticeInput input)
    {
        var sysNotice = await _sysNoticeRep.GetByIdAsync(input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);

        if (sysNotice.CreateUserId != _userManager.UserId) throw Oops.Oh(ErrorCodeEnum.D7003);

        if (sysNotice.Status == NoticeStatusEnum.PUBLIC) throw Oops.Oh(ErrorCodeEnum.D7001);

        await _sysNoticeRep.DeleteAsync(u => u.Id == input.Id);

        await _sysNoticeUserRep.DeleteAsync(u => u.NoticeId == input.Id);
    }

    /// <summary>
    /// 发布通知公告 📢
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("发布通知公告")]
    public async Task Public(NoticeInput input)
    {
        if (!(await _sysNoticeRep.IsAnyAsync(u => u.Id == input.Id && u.CreateUserId == _userManager.UserId)))
            throw Oops.Oh(ErrorCodeEnum.D7003);

        // 更新发布状态和时间
        await _sysNoticeRep.UpdateAsync(u => new SysNotice() { Status = NoticeStatusEnum.PUBLIC, PublicTime = DateTime.Now }, u => u.Id == input.Id);

        var notice = await _sysNoticeRep.GetByIdAsync(input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);

        // 通知到的人(所有账号)
        var userIdList = await _sysUserRep.AsQueryable().Select(u => u.Id).ToListAsync();

        await _sysNoticeUserRep.DeleteAsync(u => u.NoticeId == notice.Id);
        var noticeUserList = userIdList.Select(u => new SysNoticeUser
        {
            NoticeId = notice.Id,
            UserId = u,
        }).ToList();
        await _sysNoticeUserRep.InsertRangeAsync(noticeUserList);

        // 广播所有在线账号
        await _sysOnlineUserService.PublicNotice(notice, userIdList);
    }

    /// <summary>
    /// 设置通知公告已读状态 📢
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("设置通知公告已读状态")]
    public async Task SetRead(NoticeInput input)
    {
        await _sysNoticeUserRep.UpdateAsync(u => new SysNoticeUser
        {
            ReadStatus = NoticeUserStatusEnum.READ,
            ReadTime = DateTime.Now
        }, u => u.NoticeId == input.Id && u.UserId == _userManager.UserId);
    }

    /// <summary>
    /// 获取接收的通知公告
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取接收的通知公告")]
    public async Task<SqlSugarPagedList<SysNoticeUser>> PageReceived(PageNoticeInput input)
    {
        var title = input.Title?.Trim() ?? string.Empty;

        var pageList = await _sysNoticeUserRep.AsQueryable()
            .InnerJoin<SysNotice>((noticeUser, notice) => noticeUser.NoticeId == notice.Id)
            .Where((noticeUser, notice) => noticeUser.UserId == _userManager.UserId)
            .WhereIF(!string.IsNullOrWhiteSpace(title), (noticeUser, notice) => !string.IsNullOrWhiteSpace(notice.Title) && notice.Title.Contains(title))
            .WhereIF(input.Type is > 0, (noticeUser, notice) => notice.Type == input.Type)
            .OrderBy((noticeUser, notice) => notice.CreateTime, OrderByType.Desc)
            .Select((noticeUser, notice) => new SysNoticeUser
            {
                Id = noticeUser.Id,
                NoticeId = noticeUser.NoticeId,
                UserId = noticeUser.UserId,
                ReadTime = noticeUser.ReadTime,
                ReadStatus = noticeUser.ReadStatus
            })
            .ToPagedListAsync(input.Page, input.PageSize);

        var noticeUserList = pageList.Items.ToList();
        if (noticeUserList.Count == 0)
        {
            pageList.Items = noticeUserList;
            return pageList;
        }

        var noticeIds = noticeUserList.Select(u => u.NoticeId).Distinct().ToList();
        var noticeMap = (await _sysNoticeRep.AsQueryable()
                .Where(u => noticeIds.Contains(u.Id))
                .ToListAsync())
            .ToDictionary(u => u.Id);

        foreach (var noticeUser in noticeUserList)
        {
            if (noticeMap.TryGetValue(noticeUser.NoticeId, out var notice))
                noticeUser.SysNotice = notice;
        }

        pageList.Items = noticeUserList;
        return pageList;
    }

    /// <summary>
    /// 获取未读的通知公告 📢
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取未读的通知公告")]
    public async Task<List<SysNotice>> GetUnReadList()
    {
        return await _sysNoticeUserRep.AsQueryable()
            .InnerJoin<SysNotice>((noticeUser, notice) => noticeUser.NoticeId == notice.Id)
            .Where((noticeUser, notice) => noticeUser.UserId == _userManager.UserId && noticeUser.ReadStatus == NoticeUserStatusEnum.UNREAD)
            .OrderBy((noticeUser, notice) => notice.CreateTime, OrderByType.Desc)
            .Select((noticeUser, notice) => notice)
            .ToListAsync();
    }

    /// <summary>
    /// 初始化通知公告信息
    /// </summary>
    /// <param name="notice"></param>
    [NonAction]
    private void InitNoticeInfo(SysNotice notice)
    {
        notice.PublicUserId = _userManager.UserId;
        notice.PublicUserName = _userManager.RealName;
        notice.PublicOrgId = _userManager.OrgId;
    }
}
