namespace Jero.Core.Service;

/// <summary>
/// 系统访问日志服务 🧩
/// </summary>
[ApiDescriptionSettings(Order = 340)]
public class SysLogVisService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<SysLogVis> _sysLogVisRep;
    private readonly UserManager _userManager;

    public SysLogVisService(UserManager userManager, SqlSugarRepository<SysLogVis> sysLogVisRep)
    {
        _sysLogVisRep = sysLogVisRep;
        _userManager = userManager;
    }

    /// <summary>
    /// 获取访问日志分页列表 🔖
    /// </summary>
    /// <returns></returns>
    [SuppressMonitor]
    [DisplayName("获取访问日志分页列表")]
    public async Task<SqlSugarPagedList<SysLogVis>> Page(PageVisLogInput input)
    {
        return await _sysLogVisRep.AsQueryable()
            .WhereIF(_userManager.SuperAdmin && input.TenantId > 0, u => u.TenantId == input.TenantId)
            .WhereIF(!string.IsNullOrWhiteSpace(input.StartTime.ToString()), u => u.CreateTime >= input.StartTime)
            .WhereIF(!string.IsNullOrWhiteSpace(input.EndTime.ToString()), u => u.CreateTime <= input.EndTime)
            .WhereIF(!string.IsNullOrWhiteSpace(input.Account), u => u.Account == input.Account)
            .WhereIF(!string.IsNullOrWhiteSpace(input.ActionName), u => u.ActionName == input.ActionName)
            .WhereIF(!string.IsNullOrWhiteSpace(input.RemoteIp), u => u.RemoteIp == input.RemoteIp)
            .WhereIF(!string.IsNullOrWhiteSpace(input.Elapsed.ToString()), u => u.Elapsed >= input.Elapsed)
            .WhereIF(!string.IsNullOrWhiteSpace(input.Status) && input.Status == "200", u => u.Status == "200")
            .WhereIF(!string.IsNullOrWhiteSpace(input.Status) && input.Status != "200", u => u.Status != "200")
            .OrderBy(u => u.CreateTime, OrderByType.Desc)
            .ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 清空访问日志 🔖
    /// </summary>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Clear"), HttpPost]
    [DisplayName("清空访问日志")]
    public void Clear()
    {
        _sysLogVisRep.RunWithoutFilter(() =>
            _sysLogVisRep.AsSugarClient().Deleteable<SysLogVis>().ExecuteCommand());
    }

    /// <summary>
    /// 获取访问日志列表 🔖
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取访问日志列表")]
    public async Task<List<LogVisOutput>> GetList()
    {
        return await _sysLogVisRep.AsQueryable()
            .Where(u => u.Longitude > 0 && u.Longitude > 0)
            .Select(u => new LogVisOutput
            {
                Location = u.Location ?? string.Empty,
                Longitude = (double?)u.Longitude,
                Latitude = (double?)u.Latitude,
                RealName = u.RealName ?? string.Empty,
                LogDateTime = u.LogDateTime
            }).ToListAsync();
    }
}
