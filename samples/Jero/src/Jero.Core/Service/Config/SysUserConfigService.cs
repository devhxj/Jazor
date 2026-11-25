namespace Jero.Core.Service;

/// <summary>
/// 系统用户配置参数服务 🧩
/// </summary>
[ApiDescriptionSettings(Order = 440)]
public class SysUserConfigService : IDynamicApiController, ITransient
{
    private readonly SysCacheService _sysCacheService;
    private readonly SqlSugarRepository<SysUserConfig> _sysConfigRep;
    private readonly SqlSugarRepository<SysUserConfigData> _sysConfigDataRep;
    public readonly ISugarQueryable<SysConfig> VSysConfig;
    private readonly UserManager _userManager;

    public SysUserConfigService(SysCacheService sysCacheService,
        SqlSugarRepository<SysUserConfig> sysConfigRep,
        SqlSugarRepository<SysUserConfigData> sysConfigDataRep,
       UserManager userManager)
    {
        _userManager = userManager;
        _sysCacheService = sysCacheService;
        _sysConfigRep = sysConfigRep;
        _sysConfigDataRep = sysConfigDataRep;
        VSysConfig = _sysConfigRep.AsQueryable()
            .LeftJoin(
                _sysConfigDataRep.AsQueryable().WhereIF(_userManager.SuperAdmin, cv => cv.UserId == _userManager.UserId),
                (c, cv) => c.Id == cv.ConfigId
            )
            //.Select<SysConfig>().MergeTable();
            // 解决PostgreSQL下并启用驼峰转下划线时,报字段不存在,SqlSugar在Select后生成的sql, as后没转下划线导致.
            .SelectMergeTable((c, cv) => new SysConfig { Id = c.Id.SelectAll(), Value = cv.Value });
    }

    /// <summary>
    /// 获取配置参数分页列表 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取配置参数分页列表")]
    public async Task<SqlSugarPagedList<SysConfig>> Page(PageConfigInput input)
    {
        var name = input.Name?.Trim() ?? string.Empty;
        var code = input.Code?.Trim() ?? string.Empty;
        var groupCode = input.GroupCode?.Trim() ?? string.Empty;

        return await VSysConfig
            .WhereIF(!string.IsNullOrWhiteSpace(name), u => u.Name != null && u.Name.Contains(name))
            .WhereIF(!string.IsNullOrWhiteSpace(code), u => u.Code != null && u.Code.Contains(code))
            .WhereIF(!string.IsNullOrWhiteSpace(groupCode), u => u.GroupCode != null && u.GroupCode.Equals(groupCode))
            .OrderBuilder(input)
            .ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 获取配置参数列表 🔖
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取配置参数列表")]
    public async Task<List<SysConfig>> List(PageConfigInput input)
    {
        var groupCode = input.GroupCode?.Trim() ?? string.Empty;

        return await VSysConfig
            .WhereIF(!string.IsNullOrWhiteSpace(groupCode), u => u.GroupCode != null && u.GroupCode.Equals(groupCode))
            .ToListAsync();
    }

    /// <summary>
    /// 增加配置参数 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Add"), HttpPost]
    [DisplayName("增加配置参数")]
    public async Task AddConfig(AddConfigInput input)
    {
        var isExist = await _sysConfigRep.IsAnyAsync(u => u.Name == input.Name || u.Code == input.Code);
        if (isExist) throw Oops.Oh(ErrorCodeEnum.D9000);

        var configId = _sysConfigRep.InsertReturnSnowflakeId(input.Adapt<SysUserConfig>());
        await _sysConfigDataRep.InsertAsync(new SysUserConfigData()
        {
            UserId = _userManager.UserId,
            ConfigId = configId,
            Value = input.Value
        });
    }

    /// <summary>
    /// 更新配置参数 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Update"), HttpPost]
    [DisplayName("更新配置参数")]
    [UnitOfWork]
    public async Task UpdateConfig(UpdateConfigInput input)
    {
        var isExist = await _sysConfigRep.IsAnyAsync(u => (u.Name == input.Name || u.Code == input.Code) && u.Id != input.Id);
        if (isExist) throw Oops.Oh(ErrorCodeEnum.D9000);

        var config = input.Adapt<SysUserConfig>();
        await _sysConfigRep.AsUpdateable(config).IgnoreColumns(true).ExecuteCommandAsync();
        var configData = await _sysConfigDataRep.GetFirstAsync(cv => cv.ConfigId == input.Id);
        if (configData == null)
            await _sysConfigDataRep.AsInsertable(new SysUserConfigData() { UserId = _userManager.UserId, ConfigId = input.Id, Value = input.Value }).ExecuteCommandAsync();
        else
        {
            configData.Value = input.Value;
            await _sysConfigDataRep.AsUpdateable(configData).IgnoreColumns(true).ExecuteCommandAsync();
        }

        RemoveConfigCache(config);
    }

    /// <summary>
    /// 删除配置参数 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Delete"), HttpPost]
    [DisplayName("删除配置参数")]
    public async Task DeleteConfig(DeleteConfigInput input)
    {
        var config = await _sysConfigRep.GetByIdAsync(input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        // 禁止删除系统参数
        if (config.SysFlag == YesNoEnum.Y) throw Oops.Oh(ErrorCodeEnum.D9001);

        await _sysConfigRep.DeleteAsync(config);
        await _sysConfigDataRep.DeleteAsync(it => it.UserId == _userManager.UserId && it.ConfigId == config.Id);

        RemoveConfigCache(config);
    }

    /// <summary>
    /// 批量删除配置参数 🔖
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "BatchDelete"), HttpPost]
    [DisplayName("批量删除配置参数")]
    public async Task BatchDeleteConfig(List<long> ids)
    {
        foreach (var id in ids)
        {
            var config = await _sysConfigRep.GetByIdAsync(id);
            if (config == null) continue;
            // 禁止删除系统参数
            if (config.SysFlag == YesNoEnum.Y) continue;

            await _sysConfigRep.DeleteAsync(config);
            await _sysConfigDataRep.DeleteAsync(it => it.UserId == _userManager.UserId && it.ConfigId == config.Id);

            RemoveConfigCache(config);
        }
    }

    /// <summary>
    /// 获取配置参数详情 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取配置参数详情")]
    public async Task<SysConfig> GetDetail([FromQuery] ConfigInput input)
    {
        return await VSysConfig.FirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
    }

    /// <summary>
    /// 根据Code获取配置参数 🔖
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    [NonAction]
    public async Task<SysConfig> GetConfig(string code)
    {
        return await VSysConfig.FirstAsync(u => u.Code == code) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
    }

    /// <summary>
    /// 根据Code获取配置参数值 🔖
    /// </summary>
    /// <param name="code">编码</param>
    /// <returns></returns>
    [DisplayName("根据Code获取配置参数值")]
    public async Task<string?> GetConfigValueByCode(string code)
    {
        return await GetConfigValueByCode<string>(code);
    }

    /// <summary>
    /// 获取配置参数值
    /// </summary>
    /// <param name="code">编码</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    [NonAction]
    public async Task<string?> GetConfigValueByCode(string code, string? defaultValue = default)
    {
        return await GetConfigValueByCode<string>(code, defaultValue);
    }

    /// <summary>
    /// 获取配置参数值
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <param name="code">编码</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    [NonAction]
    public async Task<T?> GetConfigValueByCode<T>(string code, T? defaultValue = default)
    {
        return await GetConfigValueByCode<T>(code, _userManager.UserId, defaultValue);
    }

    /// <summary>
    /// 获取配置参数值
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <param name="code">编码</param>
    /// <param name="userId">用户Id</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    [NonAction]
    public async Task<T?> GetConfigValueByCode<T>(string code, long userId, T? defaultValue = default)
    {
        if (string.IsNullOrWhiteSpace(code)) return defaultValue;

        var value = _sysCacheService.Get<string>($"{CacheConst.KeyUserConfig}{userId}{code}");
        if (string.IsNullOrEmpty(value))
        {
            value = (await VSysConfig.FirstAsync(u => u.Code == code))?.Value;
            _sysCacheService.Set($"{CacheConst.KeyUserConfig}{userId}{code}", value);
        }
        if (string.IsNullOrWhiteSpace(value)) return defaultValue;
        return (T)Convert.ChangeType(value, typeof(T));
    }

    /// <summary>
    /// 更新配置参数值
    /// </summary>
    /// <param name="code"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    [NonAction]
    public async Task UpdateConfigValue(string code, string value)
    {
        var config = await _sysConfigRep.GetFirstAsync(u => u.Code == code);
        if (config == null) return;

        var configData = new SysUserConfigData() { UserId = _userManager.UserId, ConfigId = config.Id, Value = value };
        await _sysConfigDataRep.Context.Storageable(configData).WhereColumns(it => new[] { it.UserId, it.ConfigId }).ExecuteCommandAsync();

        RemoveConfigCache(config);
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
    /// 批量更新配置参数值 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "BatchUpdate"), HttpPost]
    [DisplayName("批量更新配置参数值")]
    public async Task BatchUpdateConfig(List<BatchConfigInput> input)
    {
        foreach (var config in input)
        {
            await UpdateConfigValue(config.Code, config.Value);
        }
    }

    /// <summary>
    /// 清除配置缓存
    /// </summary>
    /// <param name="config"></param>
    private void RemoveConfigCache(SysUserConfig config)
    {
        _sysCacheService.Remove($"{CacheConst.KeyUserConfig}{_userManager.UserId}{config.Code}");
    }
}
