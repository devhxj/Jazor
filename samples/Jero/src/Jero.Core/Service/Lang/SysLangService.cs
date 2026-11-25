namespace Jero.Core.Service;

/// <summary>
/// 语言服务 🧩
/// </summary>
[ApiDescriptionSettings(Order = 100, Description = "语言服务")]
public partial class SysLangService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<SysLang> _sysLangRep;

    public SysLangService(SqlSugarRepository<SysLang> sysLangRep)
    {
        _sysLangRep = sysLangRep;
    }

    /// <summary>
    /// 分页查询语言 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("分页查询语言")]
    [ApiDescriptionSettings(Name = "Page"), HttpPost]
    public async Task<SqlSugarPagedList<SysLangOutput>> Page(PageSysLangInput input)
    {
        var keyword = input.Keyword?.Trim() ?? string.Empty;
        var name = input.Name?.Trim() ?? string.Empty;
        var code = input.Code?.Trim() ?? string.Empty;
        var isoCode = input.IsoCode?.Trim() ?? string.Empty;
        var urlCode = input.UrlCode?.Trim() ?? string.Empty;

        var query = _sysLangRep.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(keyword), u => (!string.IsNullOrWhiteSpace(u.Name) && u.Name.Contains(keyword)) || (!string.IsNullOrWhiteSpace(u.Code) && u.Code.Contains(keyword)) || (!string.IsNullOrWhiteSpace(u.IsoCode) && u.IsoCode.Contains(keyword)) || (!string.IsNullOrWhiteSpace(u.UrlCode) && u.UrlCode.Contains(keyword)))
            .WhereIF(!string.IsNullOrWhiteSpace(name), u => !string.IsNullOrWhiteSpace(u.Name) && u.Name.Contains(name))
            .WhereIF(!string.IsNullOrWhiteSpace(code), u => !string.IsNullOrWhiteSpace(u.Code) && u.Code.Contains(code))
            .WhereIF(!string.IsNullOrWhiteSpace(isoCode), u => !string.IsNullOrWhiteSpace(u.IsoCode) && u.IsoCode.Contains(isoCode))
            .WhereIF(!string.IsNullOrWhiteSpace(urlCode), u => !string.IsNullOrWhiteSpace(u.UrlCode) && u.UrlCode.Contains(urlCode))
            .Select<SysLangOutput>();
        return await query.OrderBuilder(input).ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 获取语言详情 ℹ️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取语言详情")]
    [ApiDescriptionSettings(Name = "Detail"), HttpGet]
    public async Task<SysLang> Detail([FromQuery] QueryByIdSysLangInput input)
    {
        return await _sysLangRep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
    }

    /// <summary>
    /// 增加语言 ➕
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("增加语言")]
    [ApiDescriptionSettings(Name = "Add"), HttpPost]
    public async Task<long> Add(AddSysLangInput input)
    {
        var entity = input.Adapt<SysLang>();
        return await _sysLangRep.InsertAsync(entity) ? entity.Id : 0;
    }

    /// <summary>
    /// 更新语言 ✏️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("更新语言")]
    [ApiDescriptionSettings(Name = "Update"), HttpPost]
    public async Task Update(UpdateSysLangInput input)
    {
        var entity = input.Adapt<SysLang>();
        await _sysLangRep.AsUpdateable(entity)
        .ExecuteCommandAsync();
    }

    /// <summary>
    /// 删除语言 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("删除语言")]
    [ApiDescriptionSettings(Name = "Delete"), HttpPost]
    public async Task Delete(DeleteSysLangInput input)
    {
        var entity = await _sysLangRep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await _sysLangRep.DeleteAsync(entity);   //真删除
    }

    /// <summary>
    /// 获取下拉列表数据 🔖
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    [DisplayName("获取下拉列表数据")]
    [ApiDescriptionSettings(Name = "DropdownData"), HttpPost]
    public async Task<dynamic> DropdownData()
    {
        var data = await _sysLangRep.Context.Queryable<SysLang>()
            .Where(m => m.Active == true)
            .Select(u => new
            {
                Code = u.Code,
                Value = u.UrlCode,
                Label = $"{u.Name}"
            }).ToListAsync();
        return data;
    }
}
