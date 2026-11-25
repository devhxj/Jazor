namespace Jero.Core.Service;

/// <summary>
/// 系统机构服务 🧩
/// </summary>
[ApiDescriptionSettings(Order = 470)]
public class SysOrgService : IDynamicApiController, ITransient
{
    private readonly UserManager _userManager;
    private readonly SysCacheService _sysCacheService;
    private readonly SysUserExtOrgService _sysUserExtOrgService;
    private readonly SysUserRoleService _sysUserRoleService;
    private readonly SysRoleOrgService _sysRoleOrgService;
    private readonly SqlSugarRepository<SysOrg> _sysOrgRep;

    public SysOrgService(UserManager userManager,
        SysCacheService sysCacheService,
        SysUserExtOrgService sysUserExtOrgService,
        SysUserRoleService sysUserRoleService,
        SysRoleOrgService sysRoleOrgService,
        SqlSugarRepository<SysOrg> sysOrgRep)
    {
        _userManager = userManager;
        _sysCacheService = sysCacheService;
        _sysUserExtOrgService = sysUserExtOrgService;
        _sysUserRoleService = sysUserRoleService;
        _sysRoleOrgService = sysRoleOrgService;
        _sysOrgRep = sysOrgRep;
    }

    /// <summary>
    /// 获取机构列表 🔖
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取机构列表")]
    public async Task<List<SysOrg>> GetList([FromQuery] OrgInput input)
    {
        // 获取拥有的机构Id集合
        var userOrgIdList = await GetUserOrgIdList();

        var queryable = _sysOrgRep.AsQueryable().WhereIF(input.TenantId > 0, u => u.TenantId == input.TenantId).OrderBy(u => new { u.OrderNo, u.Id });
        // 带条件筛选时返回列表数据
        if (!string.IsNullOrWhiteSpace(input.Name) || !string.IsNullOrWhiteSpace(input.Code) || !string.IsNullOrWhiteSpace(input.Type))
        {
            return await queryable.WhereIF(userOrgIdList.Count > 0, u => userOrgIdList.Contains(u.Id))
                .WhereIF(!string.IsNullOrWhiteSpace(input.Name), u => u.Name.Contains(input.Name))
                .WhereIF(!string.IsNullOrWhiteSpace(input.Code), u => u.Code == input.Code)
                .WhereIF(!string.IsNullOrWhiteSpace(input.Type), u => u.Type == input.Type)
                .ToListAsync();
        }

        List<SysOrg> orgTree;
        if (_userManager.SuperAdmin)
        {
            orgTree = await queryable.ToTreeAsync(u => u.Children, u => u.Pid, input.Id);
        }
        else
        {
            orgTree = await queryable.ToTreeAsync(u => u.Children, u => u.Pid, input.Id, userOrgIdList.Select(d => (object)d).ToArray());
            // 递归禁用没权限的机构（防止用户修改或创建无权的机构和用户）
            HandlerOrgTree(orgTree, userOrgIdList);
        }

        var sysOrg = await _sysOrgRep.GetSingleAsync(u => u.Id == input.Id);
        if (sysOrg == null) return orgTree;

        sysOrg.Children = orgTree;
        orgTree = new List<SysOrg> { sysOrg };
        return orgTree;
    }

    /// <summary>
    /// 递归禁用没权限的机构
    /// </summary>
    /// <param name="orgTree"></param>
    /// <param name="userOrgIdList"></param>
    private static void HandlerOrgTree(List<SysOrg> orgTree, List<long> userOrgIdList)
    {
        foreach (var org in orgTree)
        {
            org.Disabled = !userOrgIdList.Contains(org.Id); // 设置禁用/不可选择
            if (org.Children != null)
                HandlerOrgTree(org.Children, userOrgIdList);
        }
    }

    /// <summary>
    /// 获取机构树 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取机构树")]
    public async Task<List<OrgTreeOutput>> GetTree([FromQuery] OrgInput input)
    {
        // 获取拥有的机构Id集合
        var userOrgIdList = await GetUserOrgIdList();

        var queryable = _sysOrgRep.AsQueryable().WhereIF(input.TenantId > 0, u => u.TenantId == input.TenantId).OrderBy(u => new { u.OrderNo, u.Id });
        List<OrgTreeOutput> orgTree;
        if (_userManager.SuperAdmin)
        {
            orgTree = await queryable.Select<OrgTreeOutput>().ToTreeAsync(u => u.Children, u => u.Pid, input.Id);
        }
        else
        {
            orgTree = await queryable.Select<OrgTreeOutput>().ToTreeAsync(u => u.Children, u => u.Pid, input.Id, userOrgIdList.Select(d => (object)d).ToArray());
            // 递归禁用没权限的机构（防止用户修改或创建无权的机构和用户）
            HandlerOrgTree(orgTree, userOrgIdList);
        }

        var sysOrg = await _sysOrgRep.AsQueryable().Select<OrgTreeOutput>().FirstAsync(u => u.Id == input.Id);
        if (sysOrg == null) return orgTree;

        sysOrg.Children = orgTree;
        orgTree = new List<OrgTreeOutput> { sysOrg };
        return orgTree;
    }

    /// <summary>
    /// 递归禁用没权限的机构
    /// </summary>
    /// <param name="orgTree"></param>
    /// <param name="userOrgIdList"></param>
    private static void HandlerOrgTree(List<OrgTreeOutput> orgTree, List<long> userOrgIdList)
    {
        foreach (var org in orgTree)
        {
            org.Disabled = !userOrgIdList.Contains(org.Id); // 设置禁用/不可选择
            if (org.Children != null)
                HandlerOrgTree(org.Children, userOrgIdList);
        }
    }

    /// <summary>
    /// 增加机构 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Add"), HttpPost]
    [DisplayName("增加机构")]
    public async Task<long> AddOrg(AddOrgInput input)
    {
        if (!_userManager.SuperAdmin && input.Pid == 0)
            throw Oops.Oh(ErrorCodeEnum.D2009);

        if (await _sysOrgRep.IsAnyAsync(u => u.Name == input.Name && u.Code == input.Code))
            throw Oops.Oh(ErrorCodeEnum.D2002);

        if (!_userManager.SuperAdmin && input.Pid != 0)
        {
            // 新增机构父Id不是0，则进行权限校验
            var orgIdList = await GetUserOrgIdList();
            // 新增机构的父机构不在自己的数据范围内
            if (orgIdList.Count < 1 || !orgIdList.Contains(input.Pid))
                throw Oops.Oh(ErrorCodeEnum.D2003);
        }

        // 删除与此父机构有关的用户机构缓存
        if (input.Pid == 0)
        {
            DeleteAllUserOrgCache(0, 0);
        }
        else
        {
            var pOrg = await _sysOrgRep.GetFirstAsync(u => u.Id == input.Pid);
            if (pOrg != null)
                DeleteAllUserOrgCache(pOrg.Id, pOrg.Pid);
        }

        var newOrg = await _sysOrgRep.AsInsertable(input.Adapt<SysOrg>()).ExecuteReturnEntityAsync();
        return newOrg.Id;
    }

    /// <summary>
    /// 批量增加机构
    /// </summary>
    /// <param name="orgs"></param>
    /// <returns></returns>
    [NonAction]
    public async Task BatchAddOrgs(List<SysOrg> orgs)
    {
        DeleteAllUserOrgCache(0, 0);
        await _sysOrgRep.AsDeleteable().ExecuteCommandAsync();
        await _sysOrgRep.AsInsertable(orgs).ExecuteCommandAsync();
    }

    /// <summary>
    /// 更新机构 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [UnitOfWork]
    [ApiDescriptionSettings(Name = "Update"), HttpPost]
    [DisplayName("更新机构")]
    public async Task UpdateOrg(UpdateOrgInput input)
    {
        if (!_userManager.SuperAdmin && input.Pid == 0)
            throw Oops.Oh(ErrorCodeEnum.D2012);

        if (input.Pid != 0)
        {
            //var pOrg = await _sysOrgRep.GetFirstAsync(u => u.Id == input.Pid);
            //_ = pOrg ?? throw Oops.Oh(ErrorCodeEnum.D2000);

            // 若父机构发生变化则清空用户机构缓存
            var sysOrg = await _sysOrgRep.GetFirstAsync(u => u.Id == input.Id);
            if (sysOrg != null && sysOrg.Pid != input.Pid)
            {
                // 删除与此机构、新父机构有关的用户机构缓存
                DeleteAllUserOrgCache(sysOrg.Id, input.Pid);
            }
        }
        if (input.Id == input.Pid)
            throw Oops.Oh(ErrorCodeEnum.D2001);

        if (await _sysOrgRep.IsAnyAsync(u => u.Name == input.Name && u.Code == input.Code && u.Id != input.Id))
            throw Oops.Oh(ErrorCodeEnum.D2002);

        // 父Id不能为自己的子节点
        var childIdList = await GetChildIdListWithSelfById(input.Id);
        if (childIdList.Contains(input.Pid))
            throw Oops.Oh(ErrorCodeEnum.D2001);

        // 是否有权限操作此机构
        if (!_userManager.SuperAdmin)
        {
            var orgIdList = await GetUserOrgIdList();
            if (orgIdList.Count < 1 || !orgIdList.Contains(input.Id))
                throw Oops.Oh(ErrorCodeEnum.D2003);
        }

        await _sysOrgRep.AsUpdateable(input.Adapt<SysOrg>()).IgnoreColumns(true).ExecuteCommandAsync();
    }

    /// <summary>
    /// 删除机构 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [UnitOfWork]
    [ApiDescriptionSettings(Name = "Delete"), HttpPost]
    [DisplayName("删除机构")]
    public async Task DeleteOrg(DeleteOrgInput input)
    {
        var sysOrg = await _sysOrgRep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);

        // 是否有权限操作此机构
        if (!_userManager.SuperAdmin)
        {
            var orgIdList = await GetUserOrgIdList();
            if (orgIdList.Count < 1 || !orgIdList.Contains(sysOrg.Id))
                throw Oops.Oh(ErrorCodeEnum.D2003);
        }

        // 若机构为租户默认机构禁止删除
        var isTenantOrg = await _sysOrgRep.ChangeRepository<SqlSugarRepository<SysTenant>>()
            .IsAnyAsync(u => u.OrgId == input.Id);
        if (isTenantOrg)
            throw Oops.Oh(ErrorCodeEnum.D2008);

        // 若机构有用户则禁止删除
        var orgHasEmp = await _sysOrgRep.ChangeRepository<SqlSugarRepository<SysUser>>()
            .IsAnyAsync(u => u.OrgId == input.Id);
        if (orgHasEmp)
            throw Oops.Oh(ErrorCodeEnum.D2004);

        // 若扩展机构有用户则禁止删除
        var hasExtOrgEmp = await _sysUserExtOrgService.HasUserOrg(sysOrg.Id);
        if (hasExtOrgEmp)
            throw Oops.Oh(ErrorCodeEnum.D2005);

        // 若子机构有用户则禁止删除
        var childOrgTreeList = await _sysOrgRep.AsQueryable().ToChildListAsync(u => u.Pid, input.Id, true);
        var childOrgIdList = childOrgTreeList.Select(u => u.Id).ToList();

        // 若子机构有用户则禁止删除
        var cOrgHasEmp = await _sysOrgRep.ChangeRepository<SqlSugarRepository<SysUser>>()
            .IsAnyAsync(u => childOrgIdList.Contains(u.OrgId));
        if (cOrgHasEmp) throw Oops.Oh(ErrorCodeEnum.D2007);

        // 若有绑定注册方案则禁止删除
        var hasUserRegWay = await _sysOrgRep.Context.Queryable<SysUserRegWay>().AnyAsync(u => u.OrgId == input.Id);
        if (hasUserRegWay) throw Oops.Oh(ErrorCodeEnum.D2010);

        // 删除与此机构、父机构有关的用户机构缓存
        DeleteAllUserOrgCache(sysOrg.Id, sysOrg.Pid);

        // 级联删除机构子节点
        await _sysOrgRep.DeleteAsync(u => childOrgIdList.Contains(u.Id));

        // 级联删除角色机构数据
        await _sysRoleOrgService.DeleteRoleOrgByOrgIdList(childOrgIdList);

        // 级联删除用户机构数据
        await _sysUserExtOrgService.DeleteUserExtOrgByOrgIdList(childOrgIdList);
    }

    /// <summary>
    /// 删除与此机构、父机构有关的用户机构缓存
    /// </summary>
    /// <param name="orgId"></param>
    /// <param name="orgPid"></param>
    private void DeleteAllUserOrgCache(long orgId, long orgPid)
    {
        var userOrgKeyList = _sysCacheService.GetKeysByPrefixKey(CacheConst.KeyUserOrg);
        if (userOrgKeyList is not { Count: > 0 }) return;

        foreach (var userOrgKey in userOrgKeyList)
        {
            var userOrgList = _sysCacheService.Get<List<long>>(userOrgKey);
            var userId = long.Parse(userOrgKey.Substring(CacheConst.KeyUserOrg));
            var dbConfigId = _sysOrgRep.Context.CurrentConnectionConfig.ConfigId?.ToString() ?? SqlSugarConst.MainConfigId;
            if (userOrgList != null && (userOrgList.Contains(orgId) || userOrgList.Contains(orgPid)))
                SqlSugarFilter.DeleteUserOrgCache(userId, dbConfigId);

            if (orgPid != 0) continue;

            var dataScope = _sysCacheService.Get<int>($"{CacheConst.KeyRoleMaxDataScope}{userId}");
            if (dataScope == (int)DataScopeEnum.All)
                SqlSugarFilter.DeleteUserOrgCache(userId, dbConfigId);
        }
    }

    /// <summary>
    /// 获取当前用户机构Id集合
    /// </summary>
    /// <returns></returns>
    [NonAction]
    public async Task<List<long>> GetUserOrgIdList()
    {
        if (_userManager.SuperAdmin) return new();
        return await GetUserOrgIdList(_userManager.UserId, _userManager.OrgId);
    }

    /// <summary>
    /// 根据指定用户Id获取机构Id集合
    /// </summary>
    /// <returns></returns>
    [NonAction]
    public async Task<List<long>> GetUserOrgIdList(long userId, long userOrgId)
    {
        var orgIdList = _sysCacheService.Get<List<long>>($"{CacheConst.KeyUserOrg}{userId}"); // 取缓存
        if (orgIdList is { Count: >= 1 }) return orgIdList;

        // 本人创建机构集合
        var orgList0 = await _sysOrgRep.AsQueryable().Where(u => u.CreateUserId == userId).Select(u => u.Id).ToListAsync();

        // 扩展机构集合
        var orgList1 = await _sysUserExtOrgService.GetUserExtOrgList(userId);

        // 角色机构集合
        var orgList2 = await GetUserRoleOrgIdList(userId, userOrgId);

        // 机构并集
        orgIdList = orgList1.Select(u => u.OrgId).Union(orgList2).Union(orgList0).ToList();

        // 当前所属机构
        if (!orgIdList.Contains(userOrgId)) orgIdList.Add(userOrgId);

        _sysCacheService.Set($"{CacheConst.KeyUserOrg}{userId}", orgIdList, TimeSpan.FromDays(7)); // 存缓存
        return orgIdList;
    }

    /// <summary>
    /// 获取用户角色机构Id集合
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="userOrgId">用户的机构Id</param>
    /// <returns></returns>
    private async Task<List<long>> GetUserRoleOrgIdList(long userId, long userOrgId)
    {
        var roleList = await _sysUserRoleService.GetUserRoleList(userId);

        if (roleList.Count < 1) return new(); // 空机构Id集合

        return await GetUserOrgIdList(roleList, userId, userOrgId);
    }

    /// <summary>
    /// 判定用户是否有某角色权限
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="role">角色代码</param>
    /// <returns></returns>
    [NonAction]
    public async Task<bool> GetUserHasRole(long userId, SysRole role)
    {
        if (_userManager.SuperAdmin)
            return true;
        var userOrgId = _userManager.OrgId;
        var roleList = await _sysUserRoleService.GetUserRoleList(userId);
        if (roleList != null && roleList.Exists(r => r.Code == role.Code) == true)
            return true;
        roleList = new List<SysRole> { role };
        var orgIds = await GetUserOrgIdList(roleList, userId, userOrgId);
        return orgIds.Contains(userOrgId);
    }

    /// <summary>
    /// 根据角色Id集合获取机构Id集合
    /// </summary>
    /// <param name="roleList"></param>
    /// <param name="userId"></param>
    /// <param name="userOrgId">用户的机构Id</param>
    /// <returns></returns>
    private async Task<List<long>> GetUserOrgIdList(List<SysRole> roleList, long userId, long userOrgId)
    {
        // 按最大范围策略设定(若同时拥有ALL和SELF权限，则结果ALL)
        int strongerDataScopeType = (int)DataScopeEnum.Self;

        // 自定义数据范围的角色集合
        var customDataScopeRoleIdList = new List<long>();

        // 数据范围的机构集合
        var dataScopeOrgIdList = new List<long>();

        if (roleList is { Count: > 0 })
        {
            roleList.ForEach(u =>
            {
                if (u.DataScope == DataScopeEnum.Define)
                {
                    customDataScopeRoleIdList.Add(u.Id);
                    strongerDataScopeType = (int)u.DataScope; // 自定义数据权限时也要更新最大范围
                }
                else if ((int)u.DataScope <= strongerDataScopeType)
                {
                    strongerDataScopeType = (int)u.DataScope;
                    // 根据数据范围获取机构集合
                    var orgIds = GetOrgIdListByDataScope(userOrgId, strongerDataScopeType).GetAwaiter().GetResult();
                    dataScopeOrgIdList = dataScopeOrgIdList.Union(orgIds).ToList();
                }
            });
        }

        // 缓存当前用户最大角色数据范围
        _sysCacheService.Set(CacheConst.KeyRoleMaxDataScope + userId, strongerDataScopeType, TimeSpan.FromDays(7));

        // 根据角色集合获取机构集合
        var roleOrgIdList = await _sysRoleOrgService.GetRoleOrgIdList(customDataScopeRoleIdList);

        // 并集机构集合
        return roleOrgIdList.Union(dataScopeOrgIdList).ToList();
    }

    /// <summary>
    /// 根据数据范围获取机构Id集合
    /// </summary>
    /// <param name="userOrgId">用户的机构Id</param>
    /// <param name="dataScope"></param>
    /// <returns></returns>
    private async Task<List<long>> GetOrgIdListByDataScope(long userOrgId, int dataScope)
    {
        var orgId = userOrgId;//var orgId = _userManager.OrgId;
        var orgIdList = new List<long>();
        switch (dataScope)
        {
            // 若数据范围是全部，则获取所有机构Id集合
            case (int)DataScopeEnum.All:
                orgIdList = await _sysOrgRep.AsQueryable().Select(u => u.Id).ToListAsync();
                break;
            // 若数据范围是本部门及以下，则获取本节点和子节点集合
            case (int)DataScopeEnum.DeptChild:
                orgIdList = await GetChildIdListWithSelfById(orgId);
                break;
            // 若数据范围是本部门不含子节点，则直接返回本部门
            case (int)DataScopeEnum.Dept:
                orgIdList.Add(orgId);
                break;
        }
        return orgIdList;
    }

    /// <summary>
    /// 根据节点Id获取子节点Id集合(包含自己)
    /// </summary>
    /// <param name="pid"></param>
    /// <returns></returns>
    [NonAction]
    public async Task<List<long>> GetChildIdListWithSelfById(long pid)
    {
        var orgTreeList = await _sysOrgRep.AsQueryable().ToChildListAsync(u => u.Pid, pid, true);
        return orgTreeList.Select(u => u.Id).ToList();
    }
}
