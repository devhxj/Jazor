namespace Jero.Core.Service;

/// <summary>
/// 系统操作日志服务 🧩
/// </summary>
[ApiDescriptionSettings(Order = 360)]
public class SysLogOpService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<SysLogOp> _sysLogOpRep;
    private readonly UserManager _userManager;

    public SysLogOpService(UserManager userManager, SqlSugarRepository<SysLogOp> sysLogOpRep)
    {
        _sysLogOpRep = sysLogOpRep;
        _userManager = userManager;
    }

    /// <summary>
    /// 获取操作日志分页列表 🔖
    /// </summary>
    /// <returns></returns>
    [SuppressMonitor]
    [DisplayName("获取操作日志分页列表")]
    public async Task<SqlSugarPagedList<SysLogOp>> Page(PageOpLogInput input)
    {
        return await _sysLogOpRep.AsQueryable()
            .WhereIF(_userManager.SuperAdmin && input.TenantId > 0, u => u.TenantId == input.TenantId)
            .WhereIF(!string.IsNullOrWhiteSpace(input.StartTime.ToString()), u => u.CreateTime >= input.StartTime)
            .WhereIF(!string.IsNullOrWhiteSpace(input.EndTime.ToString()), u => u.CreateTime <= input.EndTime)
            .WhereIF(!string.IsNullOrWhiteSpace(input.Account), u => u.Account == input.Account)
            .WhereIF(!string.IsNullOrWhiteSpace(input.RemoteIp), u => u.RemoteIp == input.RemoteIp)
            .WhereIF(!string.IsNullOrWhiteSpace(input.ControllerName), u => u.ControllerName == input.ControllerName)
            .WhereIF(!string.IsNullOrWhiteSpace(input.ActionName), u => u.ActionName == input.ActionName)
            .WhereIF(!string.IsNullOrWhiteSpace(input.Elapsed.ToString()), u => u.Elapsed >= input.Elapsed)
            //.OrderBy(u => u.CreateTime, OrderByType.Desc)
            .IgnoreColumns(u => new { u.RequestParam, u.ReturnResult, u.Message })
            .OrderBuilder(input)
            .ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 获取操作日志详情 🔖
    /// </summary>
    /// <returns></returns>
    [SuppressMonitor]
    [DisplayName("获取操作日志详情")]
    public async Task<SysLogOp> GetDetail(long id)
    {
        return await _sysLogOpRep.GetFirstAsync(u => u.Id == id);
    }

    /// <summary>
    /// 清空操作日志 🔖
    /// </summary>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Clear"), HttpPost]
    [DisplayName("清空操作日志")]
    public void Clear()
    {
        _sysLogOpRep.RunWithoutFilter(() =>
            _sysLogOpRep.AsSugarClient().Deleteable<SysLogOp>().ExecuteCommand());
    }

    /// <summary>
    /// 导出操作日志 🔖
    /// </summary>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Export"), NonUnify]
    [DisplayName("导出操作日志")]
    public async Task<IActionResult> ExportLogOp(LogInput input)
    {
        var logOpList = await _sysLogOpRep.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(input.StartTime.ToString()) && !string.IsNullOrWhiteSpace(input.EndTime.ToString()),
                    u => u.CreateTime >= input.StartTime && u.CreateTime <= input.EndTime)
            .OrderBy(u => u.CreateTime, OrderByType.Desc)
            .Select<ExportLogDto>().ToListAsync();

        IExcelExporter excelExporter = new ExcelExporter();
        var res = await excelExporter.ExportAsByteArray(logOpList);
        return new FileStreamResult(new MemoryStream(res), "application/octet-stream") { FileDownloadName = DateTime.Now.ToString("yyyyMMddHHmm") + "操作日志.xlsx" };
    }
}
