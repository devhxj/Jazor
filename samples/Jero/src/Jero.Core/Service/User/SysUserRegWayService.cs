namespace Jero.Core.Service;

/// <summary>
/// 系统用户注册方案服务 🧩
/// </summary>
[ApiDescriptionSettings(Order = 490)]
public class SysUserRegWayService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<SysUserRegWay> _sysUserRegWayRep;
    private readonly UserManager _userManager;

    public SysUserRegWayService(SqlSugarRepository<SysUserRegWay> sysUserRegWayRep, UserManager userManager)
    {
        _sysUserRegWayRep = sysUserRegWayRep;
        _userManager = userManager;
    }

    /// <summary>
    /// 查询注册方案列表 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("查询注册方案列表")]
    [ApiDescriptionSettings(Name = "List"), HttpPost]
    public async Task<List<UserRegWayOutput>> List(PageUserRegWayInput input)
    {
        var keyword = input.Keyword?.Trim() ?? string.Empty;
        var name = input.Name?.Trim() ?? string.Empty;
        var query = _sysUserRegWayRep.AsQueryable()
            .WhereIF(_userManager.SuperAdmin && input.TenantId > 0, u => u.TenantId == input.TenantId)
            .WhereIF(!string.IsNullOrWhiteSpace(keyword), u => !string.IsNullOrWhiteSpace(u.Name) && u.Name.Contains(keyword))
            .WhereIF(!string.IsNullOrWhiteSpace(name), u => !string.IsNullOrWhiteSpace(u.Name) && u.Name.Contains(name))
            .LeftJoin<SysRole>((u, a) => u.RoleId == a.Id)
            .LeftJoin<SysOrg>((u, a, b) => u.OrgId == b.Id)
            .LeftJoin<SysPos>((u, a, b, c) => u.PosId == c.Id)
            .Select((u, a, b, c) => new UserRegWayOutput
            {
                RoleName = a.Name ?? string.Empty,
                OrgName = b.Name ?? string.Empty,
                PosName = c.Name ?? string.Empty,
            }, true);
        return await query.OrderBuilder(input).ToListAsync();
    }

    /// <summary>
    /// 增加注册方案 ➕
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("增加注册方案")]
    [ApiDescriptionSettings(Name = "Add"), HttpPost]
    public async Task<long> Add(AddUserRegWayInput input)
    {
        var entity = input.Adapt<SysUserRegWay>();
        if (await _sysUserRegWayRep.IsAnyAsync(u => u.Name == input.Name)) throw Oops.Oh(ErrorCodeEnum.D2101);

        await CheckData(input);
        return await _sysUserRegWayRep.InsertAsync(entity) ? entity.Id : 0;
    }

    /// <summary>
    /// 更新注册方案 ✏️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("更新注册方案")]
    [ApiDescriptionSettings(Name = "Update"), HttpPost]
    public async Task Update(UpdateUserRegWayInput input)
    {
        if (await _sysUserRegWayRep.IsAnyAsync(u => u.Id != input.Id && u.Name == input.Name)) throw Oops.Oh(ErrorCodeEnum.D2101);

        await CheckData(input);
        await _sysUserRegWayRep.AsUpdateable(input).ExecuteCommandAsync();
    }

    /// <summary>
    /// 检查数据
    /// </summary>
    /// <param name="input"></param>
    [NonAction]
    public async Task CheckData(AddUserRegWayInput input)
    {
        // 检查外键数据是否存在
        if (!await _sysUserRegWayRep.Context.Queryable<SysRole>().AnyAsync(u => u.Id == input.RoleId)) throw Oops.Oh(ErrorCodeEnum.D1036);
        if (!await _sysUserRegWayRep.Context.Queryable<SysOrg>().AnyAsync(u => u.Id == input.OrgId)) throw Oops.Oh(ErrorCodeEnum.D2011);
        if (!await _sysUserRegWayRep.Context.Queryable<SysPos>().AnyAsync(u => u.Id == input.PosId)) throw Oops.Oh(ErrorCodeEnum.D6003);

        // 禁止注册超级管理员和系统管理员
        if (input.AccountType is AccountTypeEnum.SysAdmin or AccountTypeEnum.SuperAdmin) throw Oops.Oh(ErrorCodeEnum.D1037);
    }

    /// <summary>
    /// 删除注册方案 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [UnitOfWork]
    [DisplayName("删除注册方案")]
    [ApiDescriptionSettings(Name = "Delete"), HttpPost]
    public async Task Delete(BaseIdInput input)
    {
        var entity = await _sysUserRegWayRep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);

        // 关闭相关租户注册功能
        await _sysUserRegWayRep.Context.Updateable(new SysTenant { EnableReg = YesNoEnum.N, RegWayId = null })
            .UpdateColumns(u => new { u.EnableReg, u.RegWayId })
            .Where(u => u.RegWayId == input.Id)
            .ExecuteCommandAsync();

        // 删除方案
        await _sysUserRegWayRep.DeleteAsync(entity);
    }
}
