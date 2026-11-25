using System.Text.Json;
using Furion.Shapeless;

namespace Jero.Core.Service;

/// <summary>
/// 系统消息模板服务 🧩
/// </summary>
[ApiDescriptionSettings(Order = 305)]
public class SysTemplateService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<SysTemplate> _sysTemplateRep;
    private readonly UserManager _userManager;
    private readonly IViewEngine _viewEngine;

    public SysTemplateService(
        SqlSugarRepository<SysTemplate> sysTemplateRep,
        IViewEngine viewEngine,
        UserManager userManager)
    {
        _sysTemplateRep = sysTemplateRep;
        _userManager = userManager;
        _viewEngine = viewEngine;
    }

    /// <summary>
    /// 获取模板列表 📑
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings]
    [DisplayName("获取模板列表")]
    public async Task<SqlSugarPagedList<SysTemplate>> Page(PageTemplateInput input)
    {
        return await _sysTemplateRep.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(input.GroupName), u => u.GroupName.Contains(input.GroupName))
            .WhereIF(_userManager.SuperAdmin && input.TenantId > 0, u => u.TenantId == input.TenantId)
            .WhereIF(!string.IsNullOrWhiteSpace(input.Name), u => u.Name.Contains(input.Name))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Code), u => u.Code.Contains(input.Code))
            .WhereIF(input.Type.HasValue, u => u.Type == input.Type)
            .OrderBy(u => new { u.OrderNo, u.Id })
            .ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 获取模板 📑
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    [DisplayName("获取模板")]
    [ApiDescriptionSettings]
    public async Task<SysTemplate> GetTemplate(string code)
    {
        return await _sysTemplateRep.GetFirstAsync(u => u.Name == code) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
    }

    /// <summary>
    /// 预览模板内容 📑
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("预览模板内容")]
    [ApiDescriptionSettings]
    public async Task<string> ProView(ProViewTemplateInput input)
    {
        var template = await _sysTemplateRep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        return await RenderAsync(template.Content, input.Data ?? throw Oops.Oh("模板渲染参数不能为空"));
    }

    /// <summary>
    /// 增加模板 📑
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("增加模板")]
    [ApiDescriptionSettings(Name = "Add"), HttpPost]
    public async Task AddTemplate(AddTemplateInput input)
    {
        var isExist = await _sysTemplateRep.IsAnyAsync(u => u.Name == input.Name);
        if (isExist) throw Oops.Oh(ErrorCodeEnum.T1000);

        isExist = await _sysTemplateRep.IsAnyAsync(u => u.Code == input.Code);
        if (isExist) throw Oops.Oh(ErrorCodeEnum.T1001);

        await _sysTemplateRep.InsertAsync(input.Adapt<SysTemplate>());
    }

    /// <summary>
    /// 更新模板 📑
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("更新模板")]
    [ApiDescriptionSettings(Name = "Update"), HttpPost]
    public async Task UpdateTemplate(UpdateTemplateInput input)
    {
        var isExist = await _sysTemplateRep.IsAnyAsync(u => u.Name == input.Name && u.Id != input.Id);
        if (isExist) throw Oops.Oh(ErrorCodeEnum.T1000);

        isExist = await _sysTemplateRep.IsAnyAsync(u => u.Code == input.Code && u.Id != input.Id);
        if (isExist) throw Oops.Oh(ErrorCodeEnum.T1001);

        await _sysTemplateRep.AsUpdateable(input.Adapt<SysTemplate>()).IgnoreColumns(true).ExecuteCommandAsync();
    }

    /// <summary>
    /// 删除模板 📑
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("删除模板")]
    [ApiDescriptionSettings(Name = "Delete"), HttpPost]
    public async Task DeleteTemplate(BaseIdInput input)
    {
        await _sysTemplateRep.DeleteAsync(u => u.Id == input.Id);
    }

    /// <summary>
    /// 获取分组列表 🔖
    /// </summary>
    /// <returns></returns>
    [ApiDescriptionSettings]
    [DisplayName("获取分组列表")]
    public async Task<List<string>> GetGroupList()
    {
        return await _sysTemplateRep.AsQueryable()
            .GroupBy(u => u.GroupName)
            .Select(u => u.GroupName).ToListAsync();
    }

    /// <summary>
    /// 渲染模板内容 📑
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("渲染模板内容")]
    [ApiDescriptionSettings, HttpPost]
    public async Task<string> Render(RenderTemplateInput input)
    {
        return await RenderAsync(input.Content, input.Data ?? throw Oops.Oh("模板渲染参数不能为空"));
    }

    /// <summary>
    /// 渲染模板内容 📑
    /// </summary>
    /// <param name="content"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    [NonAction]
    public async Task<string> RenderAsync(string content, object data)
    {
        // Razor reads Model.Name dynamically, while the HTTP formatter binds object values as JsonElement.
        if (data is JsonElement { ValueKind: JsonValueKind.Object or JsonValueKind.Array } element)
            data = Clay.Parse(element.GetRawText());
        return await _viewEngine.RunCompileFromCachedAsync(Regex.Replace(content, "@\\((.*?)\\)", "@(Model.$1)"), data, builderAction: builder =>
        {
            builder.AddAssemblyReferenceByName("System.Text.RegularExpressions");
            builder.AddAssemblyReferenceByName("System.Collections");
            builder.AddAssemblyReferenceByName("System.Linq");

            builder.AddUsing("System.Text.RegularExpressions");
            builder.AddUsing("System.Collections.Generic");
            builder.AddUsing("System.Linq");
        });
    }

    /// <summary>
    /// 根据编码渲染模板内容
    /// </summary>
    /// <param name="code"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    [NonAction]
    public async Task<string> RenderByCode(string code, Dictionary<string, object> data)
    {
        var template = await _sysTemplateRep.GetFirstAsync(u => u.Code == code) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        return await RenderAsync(template.Content, data);
    }
}
