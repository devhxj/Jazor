namespace Jero.Core.Service;

/// <summary>
/// 系统常量服务 🧩
/// </summary>
[ApiDescriptionSettings(Order = 280)]
public class SysConstService : IDynamicApiController, ITransient
{
    private readonly SysCacheService _sysCacheService;

    public SysConstService(SysCacheService sysCacheService)
    {
        _sysCacheService = sysCacheService;
    }

    /// <summary>
    /// 获取所有常量列表 🔖
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取所有常量列表")]
    public async Task<List<ConstOutput>> GetList()
    {
        var key = $"{CacheConst.KeyConst}list";
        var constList = _sysCacheService.Get<List<ConstOutput>>(key);
        if (constList == null)
        {
            var typeList = GetConstAttributeList();
            constList = typeList.Select(u => new ConstOutput
            {
                Name = u.CustomAttributes.ToList().FirstOrDefault()?.ConstructorArguments.ToList().FirstOrDefault().Value?.ToString() ?? u.Name,
                Code = u.Name,
                Data = GetData(Convert.ToString(u.Name))
            }).ToList();
            _sysCacheService.Set(key, constList);
        }
        return await Task.FromResult(constList ?? []);
    }

    /// <summary>
    /// 根据类名获取常量数据 🔖
    /// </summary>
    /// <param name="typeName"></param>
    /// <returns></returns>
    [DisplayName("根据类名获取常量数据")]
    public async Task<List<ConstOutput>> GetData([Required] string typeName)
    {
        var key = $"{CacheConst.KeyConst}{typeName.ToUpper()}";
        var constList = _sysCacheService.Get<List<ConstOutput>>(key);
        if (constList == null)
        {
            var typeList = GetConstAttributeList();
            var type = typeList.FirstOrDefault(u => u.Name == typeName);
            if (type != null)
            {
                var isEnum = type.BaseType!.Name == "Enum";
                constList = type.GetFields()?
                    .Where(isEnum, u => u.FieldType.Name == typeName)
                    .Select(u => new ConstOutput
                    {
                        Name = u.Name,
                        Code = isEnum ? (int)u.GetValue(BindingFlags.Instance)! : u.GetValue(BindingFlags.Instance)
                    }).ToList();
                _sysCacheService.Set(key, constList);
            }
        }
        return await Task.FromResult(constList ?? []);
    }

    /// <summary>
    /// 获取常量特性类型列表
    /// </summary>
    /// <returns></returns>
    private List<Type> GetConstAttributeList()
    {
        return App.EffectiveTypes.Where(u => u.CustomAttributes.Any(c => c.AttributeType == typeof(ConstAttribute))).ToList();
    }
}
