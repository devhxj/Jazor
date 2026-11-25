


using NewLife.Reflection;
namespace Jero.Core.Service;

/// <summary>
/// 平台参数配置服务 🧩
/// </summary>
[ApiDescriptionSettings(Order = 440)]
public class SysConfigService : IDynamicApiController, ITransient
{
    private static readonly SysTenantService SysTenantService = App.GetService<SysTenantService>();
    private readonly SqlSugarRepository<SysConfig> _sysConfigRep;
    private readonly SqlSugarRepository<SysTenant> _sysTenantRep;
    private readonly SysCacheService _sysCacheService;
    private readonly UserManager _userManager;

    public SysConfigService(
        SqlSugarRepository<SysTenant> sysTenantRep,
        SqlSugarRepository<SysConfig> sysConfigRep,
        SysCacheService sysCacheService,
        UserManager userManager)
    {
        _sysCacheService = sysCacheService;
        _sysConfigRep = sysConfigRep;
        _sysTenantRep = sysTenantRep;
        _userManager = userManager;
    }

    /// <summary>
    /// 获取参数配置分页列表 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取参数配置分页列表")]
    public async Task<SqlSugarPagedList<SysConfig>> Page(PageConfigInput input)
    {
        var name = input.Name?.Trim() ?? string.Empty;
        var code = input.Code?.Trim() ?? string.Empty;
        var groupCode = input.GroupCode?.Trim() ?? string.Empty;

        return await _sysConfigRep.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(name), u => u.Name != null && u.Name.Contains(name))
            .WhereIF(!string.IsNullOrWhiteSpace(code), u => u.Code != null && u.Code.Contains(code))
            .WhereIF(!string.IsNullOrWhiteSpace(groupCode), u => u.GroupCode != null && u.GroupCode.Equals(groupCode))
            .OrderBuilder(input)
            .ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 获取参数配置列表 🔖
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取参数配置列表")]
    public async Task<List<SysConfig>> List(PageConfigInput input)
    {
        var groupCode = input.GroupCode?.Trim() ?? string.Empty;

        return await _sysConfigRep.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(groupCode), u => u.GroupCode != null && u.GroupCode.Equals(groupCode))
            .ToListAsync();
    }

    /// <summary>
    /// 增加参数配置 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Add"), HttpPost]
    [DisplayName("增加参数配置")]
    public async Task AddConfig(AddConfigInput input)
    {
        if (input.SysFlag == YesNoEnum.Y && !_userManager.SuperAdmin) throw Oops.Oh(ErrorCodeEnum.D3010);

        var isExist = await _sysConfigRep.IsAnyAsync(u => u.Name == input.Name || u.Code == input.Code);
        if (isExist) throw Oops.Oh(ErrorCodeEnum.D9000);

        await _sysConfigRep.InsertAsync(input.Adapt<SysConfig>());
    }

    /// <summary>
    /// 更新参数配置 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Update"), HttpPost]
    [DisplayName("更新参数配置")]
    public async Task UpdateConfig(UpdateConfigInput input)
    {
        if (input.SysFlag == YesNoEnum.Y && !_userManager.SuperAdmin) throw Oops.Oh(ErrorCodeEnum.D3010);

        var isExist = await _sysConfigRep.IsAnyAsync(u => (u.Name == input.Name || u.Code == input.Code) && u.Id != input.Id);
        if (isExist) throw Oops.Oh(ErrorCodeEnum.D9000);

        var config = input.Adapt<SysConfig>();
        await _sysConfigRep.AsUpdateable(config).IgnoreColumns(true).ExecuteCommandAsync();

        Remove(config);
    }

    /// <summary>
    /// 删除参数配置 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Delete"), HttpPost]
    [DisplayName("删除参数配置")]
    public async Task DeleteConfig(DeleteConfigInput input)
    {
        var config = await _sysConfigRep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);

        // 禁止删除系统参数
        if (config.SysFlag == YesNoEnum.Y)
        { throw Oops.Oh(ErrorCodeEnum.D9001); }
        else
        { await _sysConfigRep.DeleteAsync(config); }

        Remove(config);
    }

    /// <summary>
    /// 批量删除参数配置 🔖
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "BatchDelete"), HttpPost]
    [DisplayName("批量删除参数配置")]
    public async Task BatchDeleteConfig(List<long> ids)
    {
        foreach (var id in ids)
        {
            var config = await _sysConfigRep.GetFirstAsync(u => u.Id == id);
            if (config == null) continue;

            // 禁止删除系统参数
            if (config.SysFlag == YesNoEnum.Y) continue;

            await _sysConfigRep.DeleteAsync(config);

            Remove(config);
        }
    }

    /// <summary>
    /// 获取参数配置详情 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取参数配置详情")]
    public async Task<SysConfig> GetDetail([FromQuery] ConfigInput input)
    {
        return await _sysConfigRep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
    }

    /// <summary>
    /// 获取参数配置值
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    [NonAction]
    public async Task<T> GetConfigValue<T>(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) throw Oops.Oh("参数配置编码不能为空");

        var value = _sysCacheService.Get<string>($"{CacheConst.KeyConfig}{code}");
        if (string.IsNullOrEmpty(value))
        {
            value = (await _sysConfigRep.AsQueryable().FirstAsync(u => u.Code == code))?.Value;
            _sysCacheService.Set($"{CacheConst.KeyConfig}{code}", value);
        }
        if (string.IsNullOrWhiteSpace(value)) throw Oops.Oh($"参数配置 {code} 未配置或值为空");

        var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
        return (T)Convert.ChangeType(value, targetType);
    }

    /// <summary>
    /// 更新参数配置值
    /// </summary>
    /// <param name="code"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    [NonAction]
    public async Task UpdateConfigValue(string code, string value)
    {
        var config = await _sysConfigRep.AsQueryable().FirstAsync(u => u.Code == code);
        if (config == null) return;

        config.Value = value;
        await _sysConfigRep.AsUpdateable(config).ExecuteCommandAsync();

        Remove(config);
    }

    /// <summary>
    /// 获取分组列表 🔖
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取分组列表")]
    public async Task<List<string>> GetGroupList()
    {
        var groupCodes = await _sysConfigRep.AsQueryable()
            .GroupBy(u => u.GroupCode)
            .Select(u => u.GroupCode).ToListAsync();
        return groupCodes.Where(u => !string.IsNullOrWhiteSpace(u)).Select(u => u!).ToList();
    }

    /// <summary>
    /// 获取 Token 过期时间
    /// </summary>
    /// <returns></returns>
    [NonAction]
    public async Task<int> GetTokenExpire()
    {
        var tokenExpireStr = await GetConfigValue<string>(ConfigConst.SysTokenExpire);
        _ = int.TryParse(tokenExpireStr, out var tokenExpire);
        return tokenExpire == 0 ? 20 : tokenExpire;
    }

    /// <summary>
    /// 获取 RefreshToken 过期时间
    /// </summary>
    /// <returns></returns>
    [NonAction]
    public async Task<int> GetRefreshTokenExpire()
    {
        var refreshTokenExpireStr = await GetConfigValue<string>(ConfigConst.SysRefreshTokenExpire);
        _ = int.TryParse(refreshTokenExpireStr, out var refreshTokenExpire);
        return refreshTokenExpire == 0 ? 40 : refreshTokenExpire;
    }

    /// <summary>
    /// 获取后台默认首页地址
    /// </summary>
    /// <returns></returns>
    [NonAction]
    public async Task<string> GetDefaultHomePath()
    {
        var cacheKey = $"{CacheConst.KeyConfig}{ConfigConst.SysDefaultHomePath}";
        var value = _sysCacheService.Get<string>(cacheKey);
        if (string.IsNullOrWhiteSpace(value))
        {
            value = (await _sysConfigRep.AsQueryable().FirstAsync(u => u.Code == ConfigConst.SysDefaultHomePath))?.Value;
        }

        var normalized = DefaultHomePathUtil.NormalizeOrFallback(value);
        _sysCacheService.Set(cacheKey, normalized);
        return normalized;
    }

    /// <summary>
    /// 批量更新参数配置值
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "BatchUpdate"), HttpPost]
    [DisplayName("批量更新参数配置值")]
    public async Task BatchUpdateConfig(List<BatchConfigInput> input)
    {
        foreach (var config in input)
        {
            var info = await _sysConfigRep.AsQueryable().FirstAsync(c => c.Code == config.Code);
            if (info == null || info.SysFlag == YesNoEnum.Y) continue;

            await _sysConfigRep.AsUpdateable().SetColumns(u => u.Value == config.Value).Where(u => u.Code == config.Code).ExecuteCommandAsync();
            Remove(info);
        }
    }

    /// <summary>
    /// 获取系统信息 🔖
    /// </summary>
    /// <returns></returns>
    [SuppressMonitor]
    [AllowAnonymous]
    [DisplayName("获取系统信息")]
    public async Task<dynamic> GetSysInfo()
    {
        var tenant = await SysTenantService.GetCurrentTenantSysInfo();
        var wayList = await _sysConfigRep.Context.Queryable<SysUserRegWay>()
            .Where(u => u.TenantId == tenant.Id)
            .Select(u => new { Label = u.Name, Value = u.Id })
            .ToListAsync();

        var captcha = await GetConfigValue<bool>(ConfigConst.SysCaptcha);
        var secondVer = await GetConfigValue<bool>(ConfigConst.SysSecondVer);
        var hideTenantForLogin = await GetConfigValue<bool>(ConfigConst.SysHideTenantLogin);
        var defaultHomePath = await GetDefaultHomePath();
        return new
        {
            tenant.Logo,
            tenant.Title,
            tenant.ViceTitle,
            tenant.ViceDesc,
            tenant.Watermark,
            tenant.Copyright,
            tenant.Icp,
            tenant.IcpUrl,
            tenant.RegWayId,
            tenant.EnableReg,
            SecondVer = secondVer ? YesNoEnum.Y : YesNoEnum.N,
            Captcha = captcha ? YesNoEnum.Y : YesNoEnum.N,
            HideTenantForLogin = hideTenantForLogin,
            DefaultHomePath = defaultHomePath,
            WayList = wayList
        };
    }

    /// <summary>
    /// 保存系统信息 🔖
    /// </summary>
    /// <returns></returns>
    [UnitOfWork]
    [DisplayName("保存系统信息")]
    public async Task SaveSysInfo(InfoSaveInput input)
    {
        var tenant = await _sysTenantRep.GetFirstAsync(u => u.Id == _userManager.TenantId) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        if (!string.IsNullOrEmpty(input.LogoBase64)) SysTenantService.SetLogoUrl(tenant, input.LogoBase64, input.LogoFileName);
        // await UpdateConfigValue(ConfigConst.SysCaptcha, (input.Captcha == YesNoEnum.Y).ToString());
        // await UpdateConfigValue(ConfigConst.SysSecondVer, (input.SecondVer == YesNoEnum.Y).ToString());
        if (input.DefaultHomePath != null)
        {
            await SaveDefaultHomePath(input.DefaultHomePath);
        }

        tenant.Copy(input);
        tenant.RegWayId = input.EnableReg == YesNoEnum.Y ? input.RegWayId : null;
        await _sysTenantRep.Context.Updateable(tenant).ExecuteCommandAsync();
    }

    private async Task SaveDefaultHomePath(string? value)
    {
        string normalized;
        if (string.IsNullOrWhiteSpace(value))
        {
            normalized = DefaultHomePathUtil.FallbackHomePath;
        }
        else if (!DefaultHomePathUtil.TryNormalize(value, out normalized))
        {
            throw Oops.Oh("后台默认首页地址必须是以 / 开头的内部路由，且不能包含空白字符、反斜杠或外部协议");
        }

        var config = await _sysConfigRep.AsQueryable().FirstAsync(u => u.Code == ConfigConst.SysDefaultHomePath);
        if (config == null)
        {
            config = new SysConfig
            {
                Id = YitIdHelper.NextId(),
                Name = "后台默认首页地址",
                Code = ConfigConst.SysDefaultHomePath,
                Value = normalized,
                SysFlag = YesNoEnum.Y,
                Remark = "用户未配置个性化首页时使用的后台默认首页地址",
                OrderNo = 160,
                GroupCode = ConfigConst.SysDefaultGroup,
            };
            await _sysConfigRep.InsertAsync(config);
        }
        else if (!string.Equals(config.Value, normalized, StringComparison.Ordinal))
        {
            config.Value = normalized;
            await _sysConfigRep.AsUpdateable(config).UpdateColumns(u => new { u.Value }).ExecuteCommandAsync();
        }

        Remove(config);
    }

    private void Remove(SysConfig config)
    {
        _sysCacheService.Remove($"{CacheConst.KeyConfig}Value:{config.Code}");
        _sysCacheService.Remove($"{CacheConst.KeyConfig}Remark:{config.Code}");
        _sysCacheService.Remove($"{CacheConst.KeyConfig}{config.GroupCode}:GroupWithCache");
        _sysCacheService.Remove($"{CacheConst.KeyConfig}{config.Code}");
    }
}
