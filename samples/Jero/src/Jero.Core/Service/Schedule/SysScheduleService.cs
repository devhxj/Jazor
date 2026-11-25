namespace Jero.Core.Service;

/// <summary>
/// 系统日程服务
/// </summary>
[ApiDescriptionSettings(Order = 295)]
public class SysScheduleService : IDynamicApiController, ITransient
{
    private readonly UserManager _userManager;
    private readonly SqlSugarRepository<SysSchedule> _sysSchedule;

    public SysScheduleService(UserManager userManager,
        SqlSugarRepository<SysSchedule> sysSchedule)
    {
        _userManager = userManager;
        _sysSchedule = sysSchedule;
    }

    /// <summary>
    /// 获取日程列表
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取日程列表")]
    public async Task<List<SysSchedule>> Page(ListScheduleInput input)
    {
        return await _sysSchedule.AsQueryable()
            .Where(u => u.UserId == _userManager.UserId)
            .WhereIF(_userManager.SuperAdmin && input.TenantId > 0, u => u.TenantId == input.TenantId)
            .WhereIF(!string.IsNullOrWhiteSpace(input.StartTime.ToString()), u => u.ScheduleTime >= input.StartTime)
            .WhereIF(!string.IsNullOrWhiteSpace(input.EndTime.ToString()), u => u.ScheduleTime <= input.EndTime)
            .OrderBy(u => u.StartTime, OrderByType.Asc)
            .ToListAsync();
    }

    /// <summary>
    /// 获取日程详情
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [DisplayName("获取日程详情")]
    public async Task<SysSchedule> GetDetail(long id)
    {
        return await _sysSchedule.GetFirstAsync(u => u.Id == id);
    }

    /// <summary>
    /// 增加日程
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Add"), HttpPost]
    [DisplayName("增加日程")]
    public async Task AddUserSchedule(AddScheduleInput input)
    {
        input.UserId = _userManager.UserId;
        await _sysSchedule.InsertAsync(input.Adapt<SysSchedule>());
    }

    /// <summary>
    /// 更新日程
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Update"), HttpPost]
    [DisplayName("更新日程")]
    public async Task UpdateUserSchedule(UpdateScheduleInput input)
    {
        await _sysSchedule.AsUpdateable(input.Adapt<SysSchedule>()).IgnoreColumns(true).ExecuteCommandAsync();
    }

    /// <summary>
    /// 删除日程
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Delete"), HttpPost]
    [DisplayName("删除日程")]
    public async Task DeleteUserSchedule(DeleteScheduleInput input)
    {
        await _sysSchedule.DeleteAsync(u => u.Id == input.Id);
    }

    /// <summary>
    /// 设置日程状态
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("设置日程状态")]
    public async Task<int> SetStatus(ScheduleInput input)
    {
        if (!Enum.IsDefined(typeof(FinishStatusEnum), input.Status)) throw Oops.Oh(ErrorCodeEnum.D3005);

        return await _sysSchedule.AsUpdateable()
            .SetColumns(u => u.Status == input.Status)
            .Where(u => u.Id == input.Id)
            .ExecuteCommandAsync();
    }
}