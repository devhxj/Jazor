namespace Jero.Core.Service;

/// <summary>
/// 系统差异日志服务 🧩
/// </summary>
[ApiDescriptionSettings(Order = 330)]
public class SysLogDiffService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<SysLogDiff> _sysLogDiffRep;
    private readonly UserManager _userManager;

    public SysLogDiffService(UserManager userManager, SqlSugarRepository<SysLogDiff> sysLogDiffRep)
    {
        _sysLogDiffRep = sysLogDiffRep;
        _userManager = userManager;
    }

    /// <summary>
    /// 获取差异日志分页列表 🔖
    /// </summary>
    /// <returns></returns>
    [SuppressMonitor]
    [DisplayName("获取差异日志分页列表")]
    public async Task<SqlSugarPagedList<SysLogDiff>> Page(PageLogInput input)
    {
        return await _sysLogDiffRep.AsQueryable()
            .WhereIF(_userManager.SuperAdmin && input.TenantId > 0, u => u.TenantId == input.TenantId)
            .WhereIF(!string.IsNullOrWhiteSpace(input.StartTime.ToString()), u => u.CreateTime >= input.StartTime)
            .WhereIF(!string.IsNullOrWhiteSpace(input.EndTime.ToString()), u => u.CreateTime <= input.EndTime)
            .OrderBy(u => u.CreateTime, OrderByType.Desc)
            .ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 获取差异日志详情 🔖
    /// </summary>
    /// <returns></returns>
    [SuppressMonitor]
    [DisplayName("获取差异日志详情")]
    public async Task<SysLogDiff> GetDetail(long id)
    {
        return await _sysLogDiffRep.GetFirstAsync(u => u.Id == id);
    }
}