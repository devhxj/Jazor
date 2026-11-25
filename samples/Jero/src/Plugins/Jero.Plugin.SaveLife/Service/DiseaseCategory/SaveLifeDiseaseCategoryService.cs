namespace Jero.Plugin.SaveLife.Service;

/// <summary>
/// 疾病分类服务
/// </summary>
[ApiDescriptionSettings(SaveLifeConst.GroupName, Order = 120)]
public class SaveLifeDiseaseCategoryService : IDynamicApiController, ITransient
{
    private const int CategoryListDefaultPageSize = 100;
    private const int CategoryListMaxPageSize = 200;

    private readonly SqlSugarRepository<SaveLifeDiseaseCategory> _categoryRep;

    public SaveLifeDiseaseCategoryService(SqlSugarRepository<SaveLifeDiseaseCategory> categoryRep)
    {
        _categoryRep = categoryRep;
    }

    /// <summary>
    /// 分页查询疾病分类
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<SaveLifeDiseaseCategory>> Page(SaveLifeDiseaseCategoryInput input)
    {
        var keyword = input.Keyword?.Trim();
        return await _categoryRep.AsQueryable()
            .WhereIF(input.Status.HasValue, u => u.Status == input.Status)
            .WhereIF(!string.IsNullOrWhiteSpace(keyword), u =>
                (u.Name != null && u.Name.Contains(keyword!)) ||
                (u.Description != null && u.Description.Contains(keyword!)))
            .OrderBy(u => u.SortOrder)
            .OrderBy(u => u.Id)
            .ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 新增疾病分类
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Add")]
    public async Task<long> Add(AddSaveLifeDiseaseCategoryInput input)
    {
        var entity = input.Adapt<SaveLifeDiseaseCategory>();
        await _categoryRep.InsertAsync(entity);
        return entity.Id;
    }

    /// <summary>
    /// 更新疾病分类
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Update")]
    public async Task Update(UpdateSaveLifeDiseaseCategoryInput input)
    {
        await _categoryRep.AsUpdateable(input.Adapt<SaveLifeDiseaseCategory>()).IgnoreColumns(true).ExecuteCommandAsync();
    }

    /// <summary>
    /// 删除疾病分类
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Delete")]
    public async Task Delete(DeleteSaveLifeDiseaseCategoryInput input)
    {
        var entity = await _categoryRep.GetByIdAsync(input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await _categoryRep.FakeDeleteAsync(entity);
    }

    /// <summary>
    /// 获取疾病分类详情
    /// </summary>
    public async Task<SaveLifeDiseaseCategory> GetDetail([FromQuery] QueryByIdSaveLifeDiseaseCategoryInput input)
    {
        return await _categoryRep.GetByIdAsync(input.Id);
    }

    /// <summary>
    /// 获取疾病分类列表
    /// </summary>
    public async Task<List<SaveLifeDiseaseCategory>> GetList([FromQuery] SaveLifeDiseaseCategoryInput input)
    {
        var page = NormalizeCategoryListPage(input.Page);
        var pageSize = NormalizeCategoryListPageSize(input.PageSize);

        return await _categoryRep.AsQueryable()
            .WhereIF(input.Status.HasValue, u => u.Status == input.Status)
            .OrderBy(u => u.SortOrder)
            .OrderBy(u => u.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    /// <summary>
    /// 小程序获取启用疾病知识分类
    /// </summary>
    [AllowAnonymous]
    [HttpGet("/api/v1/save-life/knowledge/categories"), NonUnify]
    [ApiDescriptionSettings(Name = "MiniAppList")]
    public async Task<List<SaveLifeMiniAppKnowledgeCategoryOutput>> MiniAppList()
    {
        return await _categoryRep.AsQueryable()
            .Where(u => u.Status == StatusEnum.Enable)
            .OrderBy(u => u.SortOrder)
            .OrderBy(u => u.Id)
            .Select(u => new SaveLifeMiniAppKnowledgeCategoryOutput
            {
                Id = u.Id.ToString(),
                Name = u.Name,
                Description = u.Description,
                Icon = u.Icon
            })
            .ToListAsync();
    }

    private static int NormalizeCategoryListPage(int page)
    {
        return page <= 0 ? 1 : page;
    }

    private static int NormalizeCategoryListPageSize(int pageSize)
    {
        if (pageSize <= 0)
            return CategoryListDefaultPageSize;

        return Math.Min(pageSize, CategoryListMaxPageSize);
    }
}
