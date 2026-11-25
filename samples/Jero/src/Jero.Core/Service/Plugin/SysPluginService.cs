namespace Jero.Core.Service;

/// <summary>
/// 系统动态插件服务 🧩
/// </summary>
[ApiDescriptionSettings(Order = 245)]
public class SysPluginService : IDynamicApiController, ITransient
{
    private readonly IDynamicApiRuntimeChangeProvider _provider;
    private readonly SqlSugarRepository<SysPlugin> _sysPluginRep;
    private readonly UserManager _userManager;

    public SysPluginService(IDynamicApiRuntimeChangeProvider provider,
        SqlSugarRepository<SysPlugin> sysPluginRep,
        UserManager userManager)
    {
        _provider = provider;
        _userManager = userManager;
        _sysPluginRep = sysPluginRep;
    }

    /// <summary>
    /// 获取动态插件列表 🧩
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取动态插件列表")]
    public async Task<SqlSugarPagedList<SysPlugin>> Page(PagePluginInput input)
    {
        return await _sysPluginRep.AsQueryable()
            .WhereIF(_userManager.SuperAdmin && input.TenantId > 0, u => u.TenantId == input.TenantId)
            .WhereIF(!string.IsNullOrWhiteSpace(input.Name), u => u.Name.Contains(input.Name))
            .OrderBy(u => new { u.OrderNo, u.Id })
            .ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 增加动态插件 🧩
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Add"), HttpPost]
    [DisplayName("增加动态插件")]
    public async Task AddPlugin(AddPluginInput input)
    {
        var isExist = await _sysPluginRep.IsAnyAsync(u => u.Name == input.Name || u.AssemblyName == input.AssemblyName);
        if (isExist) throw Oops.Oh(ErrorCodeEnum.D1900);

        // 添加动态程序集/接口
        input.AssemblyName = CompileAssembly(input.CsharpCode, input.AssemblyName);

        await _sysPluginRep.InsertAsync(input.Adapt<SysPlugin>());
    }

    /// <summary>
    /// 更新动态插件 🧩
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Update"), HttpPost]
    [DisplayName("更新动态插件")]
    public async Task UpdatePlugin(UpdatePluginInput input)
    {
        var plugin = await _sysPluginRep.GetByIdAsync(input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        var isExist = await _sysPluginRep.IsAnyAsync(u => (u.Name == input.Name || u.AssemblyName == input.AssemblyName) && u.Id != input.Id);
        if (isExist) throw Oops.Oh(ErrorCodeEnum.D1900);

        // 先移除当前已加载的程序集，再按新的代码/程序集名称重新编译，避免编辑时改名后卸载错目标
        if (!string.IsNullOrWhiteSpace(plugin.AssemblyName))
        {
            RemoveAssembly(plugin.AssemblyName);
        }

        input.AssemblyName = CompileAssembly(input.CsharpCode, input.AssemblyName);

        await _sysPluginRep.AsUpdateable(input.Adapt<SysPlugin>()).IgnoreColumns(true).ExecuteCommandAsync();
    }

    /// <summary>
    /// 删除动态插件 🧩
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Delete"), HttpPost]
    [DisplayName("删除动态插件")]
    public async Task DeletePlugin(DeletePluginInput input)
    {
        var plugin = await _sysPluginRep.GetByIdAsync(input.Id);
        if (plugin == null) return;

        // 移除动态程序集/接口
        if (!string.IsNullOrWhiteSpace(plugin.AssemblyName))
            RemoveAssembly(plugin.AssemblyName);

        await _sysPluginRep.DeleteAsync(u => u.Id == input.Id);
    }

    /// <summary>
    /// 添加动态程序集/接口 🧩
    /// </summary>
    /// <param name="csharpCode"></param>
    /// <param name="assemblyName">程序集名称</param>
    /// <returns></returns>
    [DisplayName("添加动态程序集/接口")]
    public string CompileAssembly([FromBody] string csharpCode, [FromQuery] string? assemblyName = default)
    {
        if (string.IsNullOrWhiteSpace(csharpCode))
            throw Oops.Oh("动态插件代码不能为空");

        // MVC 应用部件会继续使用这个程序集，注册期间必须保持其加载。
        var dynamicAssembly = App.CompileCSharpClassCode(csharpCode, assemblyName).Assembly;
        var compiledAssemblyName = dynamicAssembly.GetName().Name;
        if (string.IsNullOrWhiteSpace(compiledAssemblyName))
            throw Oops.Oh("动态插件程序集名称不能为空");

        // 将程序集添加进动态 WebAPI 应用部件
        _provider.AddAssembliesWithNotifyChanges(dynamicAssembly);

        // 返回动态程序集名称
        return compiledAssemblyName;
    }

    /// <summary>
    /// 移除动态程序集/接口 🧩
    /// </summary>
    [ApiDescriptionSettings(Name = "RemoveAssembly"), HttpPost]
    [DisplayName("移除动态程序集/接口")]
    public void RemoveAssembly(string assemblyName)
    {
        _provider.RemoveAssembliesWithNotifyChanges(assemblyName);
    }

    /// <summary>
    /// 加载所有动态程序集/接口
    /// </summary>
    /// <returns></returns>
    [NonAction]
    public async Task LoadAllAssembly()
    {
        var plugins = await _sysPluginRep.GetListAsync();
        foreach (var plugin in plugins)
        {
            CompileAssembly(plugin.CsharpCode, plugin.AssemblyName);
        }
    }
}
