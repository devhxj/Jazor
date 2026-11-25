using Furion.DatabaseAccessor;

namespace Jero.Plugin.SaveLife.Service;

/// <summary>
/// 报名审核角色与项目疾病领域的独立配置服务。
/// </summary>
[Authorize]
[ApiDescriptionSettings(SaveLifeConst.GroupName, Order = 111)]
public class SaveLifeEnrollmentAuditScopeService :
    IDynamicApiController,
    ITransient,
    ISaveLifeEnrollmentAuditScopeAccessService
{
    public const string EnrollmentAuditPermission = "saveLifeEnrollmentRecord:audit";
    public const string ManageAuditScopePermission = "saveLifeEnrollmentRecord:manageAuditScope";

    private readonly SqlSugarRepository<SaveLifeEnrollmentAuditScope> _scopeRep;
    private readonly SqlSugarRepository<SysUserRole> _userRoleRep;
    private readonly SqlSugarRepository<SysRole> _roleRep;
    private readonly SqlSugarRepository<SysRoleMenu> _roleMenuRep;
    private readonly SqlSugarRepository<SysMenu> _menuRep;
    private readonly SqlSugarRepository<SaveLifeClinicalTrial> _trialRep;
    private readonly UserManager _userManager;
    private readonly SysMenuService _sysMenuService;

    public SaveLifeEnrollmentAuditScopeService(
        SqlSugarRepository<SaveLifeEnrollmentAuditScope> scopeRep,
        SqlSugarRepository<SysUserRole> userRoleRep,
        SqlSugarRepository<SysRole> roleRep,
        SqlSugarRepository<SysRoleMenu> roleMenuRep,
        SqlSugarRepository<SysMenu> menuRep,
        SqlSugarRepository<SaveLifeClinicalTrial> trialRep,
        UserManager userManager,
        SysMenuService sysMenuService)
    {
        _scopeRep = scopeRep;
        _userRoleRep = userRoleRep;
        _roleRep = roleRep;
        _roleMenuRep = roleMenuRep;
        _menuRep = menuRep;
        _trialRep = trialRep;
        _userManager = userManager;
        _sysMenuService = sysMenuService;
    }

    /// <summary>
    /// 获取可配置的审核角色及其领域范围。
    /// </summary>
    [HttpGet]
    [ApiDescriptionSettings(Name = "Config")]
    public async Task<SaveLifeEnrollmentAuditScopeConfigOutput> Config()
    {
        await EnsureCanManageAuditScopes();

        var roles = await GetEnabledAuditRolesAsync();
        var roleIds = roles.Select(item => item.RoleId).ToList();
        var scopes = roleIds.Count == 0
            ? new List<SaveLifeEnrollmentAuditScope>()
            : await _scopeRep.AsQueryable()
                .Where(item => roleIds.Contains(item.RoleId))
                .ToListAsync();
        var projectDiseaseAreas = await _trialRep.AsQueryable()
            .Where(item => item.DiseaseArea != null)
            .Select(item => item.DiseaseArea!)
            .ToListAsync();

        foreach (var role in roles)
        {
            role.DiseaseAreas = NormalizeDiseaseAreas(
                scopes
                    .Where(item => item.RoleId == role.RoleId)
                    .Select(item => item.DiseaseArea));
        }

        return new SaveLifeEnrollmentAuditScopeConfigOutput
        {
            DiseaseAreas = NormalizeDiseaseAreas(projectDiseaseAreas.Concat(scopes.Select(item => item.DiseaseArea))),
            Roles = roles
        };
    }

    /// <summary>
    /// 保存一个审核角色负责的项目疾病领域。
    /// </summary>
    [HttpPost]
    [UnitOfWork]
    [ApiDescriptionSettings(Name = "SaveRoleScope")]
    public async Task SaveRoleScope(SaveSaveLifeEnrollmentAuditRoleScopeInput input)
    {
        await EnsureCanManageAuditScopes();

        var isAuditRole = (await GetEnabledAuditRoleIdsAsync()).Contains(input.RoleId);
        if (!isAuditRole)
            throw Oops.Oh("仅可配置拥有报名审核权限的角色").StatusCode(400);

        var diseaseAreas = NormalizeDiseaseAreas(input.DiseaseAreas);
        await _scopeRep.DeleteAsync(item => item.RoleId == input.RoleId);
        if (diseaseAreas.Count == 0)
            return;

        await _scopeRep.InsertRangeAsync(diseaseAreas.Select(diseaseArea => new SaveLifeEnrollmentAuditScope
        {
            RoleId = input.RoleId,
            DiseaseArea = diseaseArea
        }).ToList());
    }

    [NonAction]
    public async Task<SaveLifeEnrollmentAuditAccessScope> GetCurrentAccessScopeAsync()
    {
        if (_userManager.SuperAdmin || _userManager.SysAdmin)
            return SaveLifeEnrollmentAuditAccessScope.Unrestricted;

        if (_userManager.UserId <= 0)
            return SaveLifeEnrollmentAuditAccessScope.Empty;

        var userRoleIds = await _userRoleRep.AsQueryable()
            .Where(item => item.UserId == _userManager.UserId)
            .Select(item => item.RoleId)
            .ToListAsync();
        if (userRoleIds.Count == 0)
            return SaveLifeEnrollmentAuditAccessScope.Empty;

        var auditRoleIds = await GetEnabledAuditRoleIdsAsync(userRoleIds);
        if (auditRoleIds.Count == 0)
            return SaveLifeEnrollmentAuditAccessScope.Empty;

        var diseaseAreas = await _scopeRep.AsQueryable()
            .Where(item => auditRoleIds.Contains(item.RoleId))
            .Select(item => item.DiseaseArea)
            .ToListAsync();
        var normalizedDiseaseAreas = NormalizeDiseaseAreas(diseaseAreas);
        return normalizedDiseaseAreas.Count == 0
            ? SaveLifeEnrollmentAuditAccessScope.Empty
            : new SaveLifeEnrollmentAuditAccessScope(false, normalizedDiseaseAreas);
    }

    private async Task EnsureCanManageAuditScopes()
    {
        if (_userManager.SuperAdmin || _userManager.SysAdmin)
            return;

        var permissions = await _sysMenuService.GetOwnBtnPermList();
        if (permissions.Any(permission =>
                permission.Equals(ManageAuditScopePermission, StringComparison.OrdinalIgnoreCase)))
            return;

        throw Oops.Oh("无权配置审核角色领域").StatusCode(403);
    }

    private async Task<List<long>> GetEnabledAuditRoleIdsAsync(IEnumerable<long>? candidateRoleIds = null)
    {
        var auditMenuIds = await _menuRep.AsQueryable()
            .Where(item => item.Permission == EnrollmentAuditPermission)
            .Select(item => item.Id)
            .ToListAsync();
        if (auditMenuIds.Count == 0)
            return new List<long>();

        var roleIds = await _roleMenuRep.AsQueryable()
            .Where(item => auditMenuIds.Contains(item.MenuId))
            .Select(item => item.RoleId)
            .Distinct()
            .ToListAsync();
        if (candidateRoleIds != null)
        {
            var candidateSet = candidateRoleIds.ToHashSet();
            roleIds = roleIds.Where(candidateSet.Contains).ToList();
        }
        if (roleIds.Count == 0)
            return new List<long>();

        return await _roleRep.AsQueryable()
            .Where(item => roleIds.Contains(item.Id) && item.Status != StatusEnum.Disable)
            .Select(item => item.Id)
            .ToListAsync();
    }

    private async Task<List<SaveLifeEnrollmentAuditRoleScopeOutput>> GetEnabledAuditRolesAsync()
    {
        var roleIds = await GetEnabledAuditRoleIdsAsync();
        if (roleIds.Count == 0)
            return new List<SaveLifeEnrollmentAuditRoleScopeOutput>();

        return await _roleRep.AsQueryable()
            .Where(item => roleIds.Contains(item.Id))
            .OrderBy(item => item.OrderNo)
            .OrderBy(item => item.Id)
            .Select(item => new SaveLifeEnrollmentAuditRoleScopeOutput
            {
                RoleId = item.Id,
                RoleName = item.Name,
                RoleCode = item.Code
            })
            .ToListAsync();
    }

    private static List<string> NormalizeDiseaseAreas(IEnumerable<string>? diseaseAreas)
    {
        var result = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var value in diseaseAreas ?? [])
        {
            var diseaseArea = value?.Trim();
            if (string.IsNullOrWhiteSpace(diseaseArea))
                continue;
            if (diseaseArea.Length > 64)
                throw Oops.Oh("疾病领域不能超过64个字符").StatusCode(400);
            if (seen.Add(diseaseArea))
                result.Add(diseaseArea);
        }

        return result.OrderBy(item => item, StringComparer.Ordinal).ToList();
    }
}
