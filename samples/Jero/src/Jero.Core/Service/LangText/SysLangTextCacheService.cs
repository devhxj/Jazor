namespace Jero.Core.Service;

public class LangFieldMap<TEntity>
{
    /// <summary>实体名，如 Product</summary>
    public string EntityName { get; set; } = string.Empty;
    /// <summary>字段名，如 Name/Description</summary>
    public string FieldName { get; set; } = string.Empty;
    /// <summary>如何取主键ID</summary>
    public Func<TEntity, long> IdSelector { get; set; } = _ => throw new InvalidOperationException($"{nameof(IdSelector)} 未配置");

    /// <summary>如何写回翻译值</summary>
    public Action<TEntity, string> SetTranslatedValue { get; set; } = (_, _) => throw new InvalidOperationException($"{nameof(SetTranslatedValue)} 未配置");
}

/// <summary>
/// 翻译缓存服务 🧩
/// </summary>
[ApiDescriptionSettings(Order = 100, Description = "翻译缓存服务")]
public class SysLangTextCacheService : IDynamicApiController, ITransient
{
    private readonly SysCacheService _sysCacheService;
    private readonly SqlSugarRepository<SysLangText> _sysLangTextRep;
    private TimeSpan expireSeconds = TimeSpan.FromHours(1);

    public SysLangTextCacheService(
        SysCacheService sysCacheService,
        SqlSugarRepository<SysLangText> sysLangTextRep)
    {
        _sysCacheService = sysCacheService;
        _sysLangTextRep = sysLangTextRep;
    }

    private string BuildKey(string entityName, string fieldName, long entityId, string langCode)
    {
        return $"LangCache_{entityName}_{fieldName}_{entityId}_{langCode}";
    }

    /// <summary>
    /// 【单条翻译获取】
    /// 根据实体类型、字段、主键ID 和语言编码获取翻译内容。<br/>
    /// 适用于：小表（如菜单、字典），可设置较长缓存时间。<br/>
    /// <br/>
    /// 【示例】<br/>
    /// var content = await _sysLangTextCacheService.GetTranslation("Product", "Name", 123, "en-US");
    /// </summary>
    /// <param name="entityName">实体名称，如 "Product"</param>
    /// <param name="fieldName">字段名称，如 "Name"</param>
    /// <param name="entityId">实体主键ID</param>
    /// <param name="langCode">语言编码，如 "zh-CN"</param>
    /// <returns>翻译后的内容（若无则返回 null 或空）</returns>
    [NonAction]
    public async Task<string> GetTranslation(string entityName, string fieldName, long entityId, string langCode)
    {
        var key = BuildKey(entityName, fieldName, entityId, langCode);
        var value = _sysCacheService.Get<string>(key);
        if (!string.IsNullOrEmpty(value)) return value;

        value = await _sysLangTextRep.AsQueryable()
            .Where(u => u.EntityName == entityName && u.FieldName == fieldName && u.EntityId == entityId && u.LangCode == langCode)
            .Select(u => u.Content)
            .FirstAsync();

        if (!string.IsNullOrEmpty(value))
        {
            _sysCacheService.Set(key, value, expireSeconds); // 设置过期
        }

        return value ?? string.Empty;
    }

    /// <summary>
    /// 根据实体类型、字段、主键ID 和语言编码获取翻译实体
    /// </summary>
    /// <param name="entityName">实体名称</param>
    /// <param name="fieldName">字段名称</param>
    /// <param name="entityId">实体主键ID</param>
    /// <param name="langCode">语言编码</param>
    /// <returns></returns>
    [NonAction]
    public async Task<SysLangText?> GetTranslationEntity(string entityName, string fieldName, long entityId, string langCode)
    {
        var key = BuildKey(entityName, fieldName, entityId, langCode) + "_entity";
        var value = _sysCacheService.Get<SysLangText>(key);
        if (!value.IsNullOrEmpty()) return value;

        value = await _sysLangTextRep.AsQueryable()
            .Where(u => u.EntityName == entityName && u.FieldName == fieldName && u.EntityId == entityId && u.LangCode == langCode)
            .FirstAsync();

        if (!value.IsNullOrEmpty())
        {
            _sysCacheService.Set(key, value, expireSeconds); // 设置过期
        }

        return value;
    }

    /// <summary>
    /// 【批量翻译获取】<br/>
    /// 根据实体、字段和一批主键ID获取对应翻译内容，自动从缓存或数据库获取。<br/>
    /// 适用于：SKU、多商品、批量字典等需要高效批量获取的场景。<br/>
    ///
    /// 【示例】<br/>
    /// var dict = await _sysLangTextCacheService.GetTranslations("SKU", "Name", skuIds, "en_US");
    /// </summary>
    /// <param name="entityName">实体名称</param>
    /// <param name="fieldName">字段名称</param>
    /// <param name="entityIds">主键ID集合</param>
    /// <param name="langCode">语言编码</param>
    /// <returns>主键ID到翻译内容的字典</returns>
    [NonAction]
    public async Task<Dictionary<long, string>> GetTranslations(string entityName, string fieldName, List<long> entityIds, string langCode)
    {
        var result = new Dictionary<long, string>();
        var missingIds = new HashSet<long>(); // 用 HashSet 提高后面 Contains 的性能

        foreach (var id in entityIds.Distinct()) // 先去重，防止重复缓存 Key
        {
            var key = BuildKey(entityName, fieldName, id, langCode);
            var value = _sysCacheService.Get<string>(key);
            if (!string.IsNullOrWhiteSpace(value))
            {
                result[id] = value;
            }
            else
            {
                missingIds.Add(id);
            }
        }

        if (missingIds.Any())
        {
            var list = await _sysLangTextRep.AsQueryable()
                .Where(u => u.EntityName == entityName &&
                            u.FieldName == fieldName &&
                            missingIds.Contains(u.EntityId) &&
                            u.LangCode == langCode)
                .ToListAsync();

            foreach (var item in list)
            {
                if (string.IsNullOrWhiteSpace(item.Content)) continue; // 跳过脏数据

                var key = BuildKey(item.EntityName, item.FieldName, item.EntityId, item.LangCode);
                _sysCacheService.Set(key, item.Content, expireSeconds);

                // 用 TryAdd 防止异常
                result[item.EntityId] = item.Content;
            }
        }

        return result;
    }

    /// <summary>
    /// 【列表翻译】<br/>
    /// 按配置把同一字段的翻译写回到实体列表中。内部会调用批量翻译接口。<br/>
    /// <br/>
    /// 【示例】<br/>
    /// await _sysLangTextCacheService.TranslateList(products, "Product", "Name", p =&gt; p.Id, (p, val) =&gt; p.Name = val, "zh-CN");
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="list">待翻译的实体列表</param>
    /// <param name="entityName">实体名称</param>
    /// <param name="fieldName">字段名称</param>
    /// <param name="idSelector">用于取出主键ID的表达式</param>
    /// <param name="setTranslatedValue">写回翻译值的委托</param>
    /// <param name="langCode">语言编码</param>
    /// <returns>翻译后的实体列表（引用传递）</returns>
    [NonAction]
    public async Task<List<TEntity>> TranslateList<TEntity>(List<TEntity> list, string entityName, string fieldName, Func<TEntity, long> idSelector, Action<TEntity, string> setTranslatedValue, string langCode)
    {
        var ids = list.Select(idSelector).Distinct().ToList();
        var dict = await GetTranslations(entityName, fieldName, ids, langCode);

        foreach (var item in list)
        {
            var id = idSelector(item);
            if (dict.TryGetValue(id, out var value))
            {
                setTranslatedValue(item, value);
            }
        }

        return list;
    }

    /// <summary>
    /// 【多字段批量翻译】
    /// 对列表中的实体对象，按配置的字段映射进行多字段翻译处理。<br/>
    /// 常用于：菜单多语言、商品多语言、SKU多语言等需要多字段翻译的场景。<br/><br/>
    /// ✅ 特点：<br/>
    /// 1️⃣ 可同时翻译同一实体的多个字段（如 Name、Description、Title 等）<br/>
    /// 2️⃣ 内部先尝试从缓存读取，如缓存未命中则批量查询数据库，并自动写回缓存<br/>
    /// 3️⃣ 引用传递，直接对原实体对象赋值，无需额外返回<br/><br/>
    /// 【使用示例】：<br/>
    /// <code>
    /// var fields = new List&lt;LangFieldMap&lt;Product&gt;&gt;
    /// {
    ///     new LangFieldMap&lt;Product&gt; {
    ///         EntityName = "Product",
    ///         FieldName = "Name",
    ///         IdSelector = p =&gt; p.Id,
    ///         SetTranslatedValue = (p, val) =&gt; p.Name = val
    ///     },
    ///     new LangFieldMap&lt;Product&gt; {
    ///         EntityName = "Product",
    ///         FieldName = "Description",
    ///         IdSelector = p =&gt; p.Id,
    ///         SetTranslatedValue = (p, val) =&gt; p.Description = val
    ///     }
    /// };
    /// await _sysLangTextCacheService.TranslateMultiFields(products, fields, "zh-CN");
    /// </code>
    /// </summary>
    /// <typeparam name="TEntity">要翻译的实体类型，如 Product/Menu/SKU 等</typeparam>
    /// <param name="list">需要翻译的实体对象列表</param>
    /// <param name="fields">需要翻译的字段映射集合，支持多个字段</param>
    /// <param name="langCode">语言编码，如 "zh-CN"、"en-US"、"it-IT" 等</param>
    /// <returns>翻译后的实体列表（引用传递，原对象已直接赋值）</returns>
    [NonAction]
    public async Task<List<TEntity>> TranslateMultiFields<TEntity>(
    List<TEntity> list,
    List<LangFieldMap<TEntity>> fields,
    string langCode)
    {
        var keyToField = new Dictionary<string, (TEntity Entity, LangFieldMap<TEntity> FieldMap)>();
        var missingKeys = new List<string>();

        // 先尝试从缓存读取
        foreach (var item in list)
        {
            foreach (var field in fields)
            {
                var id = field.IdSelector(item);
                var key = BuildKey(field.EntityName, field.FieldName, id, langCode);
                var cached = _sysCacheService.Get<string>(key);
                if (!string.IsNullOrEmpty(cached))
                {
                    // 命中缓存，直接赋值
                    field.SetTranslatedValue(item, cached);
                }
                else
                {
                    // 缓存未命中，加入待查表
                    keyToField[key] = (item, field);
                    missingKeys.Add(key);
                }
            }
        }

        if (missingKeys.Any())
        {
            // 把缺失的 keys 拆解成组合实体
            var missingTuples = missingKeys
                .Select(key =>
                {
                    var parts = key.Split('_');
                    return new
                    {
                        EntityName = parts[1],
                        FieldName = parts[2],
                        EntityId = long.Parse(parts[3])
                    };
                })
                .ToList();

            // 按 EntityName + FieldName 分组
            var grouped = missingTuples
                .GroupBy(x => new { x.EntityName, x.FieldName })
                .ToList();

            var result = new List<SysLangText>();

            // 分批查询，每组单独查询
            const int chunkSize = 500;
            foreach (var g in grouped)
            {
                var allIds = g.Select(x => x.EntityId).Distinct().ToList();
                for (int i = 0; i < allIds.Count; i += chunkSize)
                {
                    var chunk = allIds.Skip(i).Take(chunkSize).ToList();
                    var temp = await _sysLangTextRep.AsQueryable()
                        .Where(u => u.LangCode == langCode
                                    && u.EntityName == g.Key.EntityName
                                    && u.FieldName == g.Key.FieldName
                                    && chunk.Contains(u.EntityId))
                        .ToListAsync();
                    result.AddRange(temp);
                }
            }

            // 遍历查询结果，写回实体和缓存
            foreach (var item in result)
            {
                var key = BuildKey(item.EntityName, item.FieldName, item.EntityId, item.LangCode);
                if (keyToField.TryGetValue(key, out var tuple))
                {
                    tuple.FieldMap.SetTranslatedValue(tuple.Entity, item.Content);
                    _sysCacheService.Set(key, item.Content, expireSeconds);
                }
            }
        }

        return list;
    }

    /// <summary>
    /// 删除缓存
    /// </summary>
    /// <param name="entityName"></param>
    /// <param name="fieldName"></param>
    /// <param name="entityId"></param>
    /// <param name="langCode"></param>
    public void DeleteCache(string entityName, string fieldName, long entityId, string langCode)
    {
        var key = BuildKey(entityName, fieldName, entityId, langCode);
        _sysCacheService.Remove(key);
    }

    /// <summary>
    /// 更新缓存
    /// </summary>
    /// <param name="entityName"></param>
    /// <param name="fieldName"></param>
    /// <param name="entityId"></param>
    /// <param name="langCode"></param>
    /// <param name="newValue"></param>
    public void UpdateCache(string entityName, string fieldName, long entityId, string langCode, string newValue)
    {
        var key = BuildKey(entityName, fieldName, entityId, langCode);
        _sysCacheService.Set(key, newValue, expireSeconds);
    }
}
