namespace Jero.Core.Service;

/// <summary>
/// 系统存储过程服务 🧩
/// </summary>
[ApiDescriptionSettings(Order = 102)]
public class SysProcService : IDynamicApiController, ITransient
{
    private readonly ISqlSugarClient _db;

    public SysProcService(ISqlSugarClient db)
    {
        _db = db;
    }

    /// <summary>
    /// 导出存储过程数据-指定列，没有指定的字段会被隐藏 🔖
    /// </summary>
    /// <returns></returns>
    public async Task<IActionResult> PocExport2(ExportProcInput input)
    {
        var db = _db.AsTenant().GetConnectionScope(input.ConfigId);
        var dt = await db.Ado.UseStoredProcedure().GetDataTableAsync(input.ProcId, input.ProcParams);

        var headers = new Dictionary<string, Tuple<string, int>>();
        var index = 1;
        foreach (var val in input.EHeader)
        {
            headers.Add(val.Key.ToUpper(), new Tuple<string, int>(val.Value, index));
            index++;
        }
        var excelExporter = new ExcelExporter();
        var da = await excelExporter.ExportAsByteArray(dt, new ProcExporterHeaderFilter(headers));
        return new FileContentResult(da, "application/octet-stream") { FileDownloadName = input.ProcId + ".xlsx" };
    }

    /// <summary>
    /// 根据模板导出存储过程数据 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<IActionResult> PocExport(ExportProcByTMPInput input)
    {
        var db = _db.AsTenant().GetConnectionScope(input.ConfigId);
        var dt = await db.Ado.UseStoredProcedure().GetDataTableAsync(input.ProcId, input.ProcParams);

        var excelExporter = new ExcelExporter();
        string template = AppDomain.CurrentDomain.BaseDirectory + "/wwwroot/template/" + input.Template + ".xlsx";
        var bs = await excelExporter.ExportBytesByTemplate(dt, template);
        return new FileContentResult(bs, "application/octet-stream") { FileDownloadName = input.ProcId + ".xlsx" };
    }

    /// <summary>
    /// 获取存储过程返回表-Oracle、达梦参数顺序不能错 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<DataTable> ProcTable(BaseProcInput input)
    {
        var db = _db.AsTenant().GetConnectionScope(input.ConfigId);
        return await db.Ado.UseStoredProcedure().GetDataTableAsync(input.ProcId, input.ProcParams);
    }

    /// <summary>
    /// 获取存储过程返回数据集-Oracle、达梦参数顺序不能错
    /// Oracle 返回table、table1，其他返回table1、table2。适用于报表、复杂详细页面等 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<DataSet> CommonDataSet(BaseProcInput input)
    {
        var db = _db.AsTenant().GetConnectionScope(input.ConfigId);
        return await db.Ado.UseStoredProcedure().GetDataSetAllAsync(input.ProcId, input.ProcParams);
    }

    ///// <summary>
    ///// 根据配置表获取对映存储过程
    ///// </summary>
    ///// <param name="input"></param>
    ///// <returns></returns>
    //public async Task<DataTable> ProcEnitybyConfig(BaseProcInput input)
    //{
    //    var key = "ProcConfig";
    //    var ds = _sysCacheService.Get<Dictionary<string, string>>(key);
    //    if (ds == null || ds.Count == 0 || !ds.ContainsKey(input.ProcId))
    //    {
    //        var datas = await _db.Queryable<ProcConfig>().ToListAsync();
    //        ds = datas.ToDictionary(m => m.ProcId, m => m.ProcName);
    //        _sysCacheService.Set(key, ds);
    //    }
    //    var procName = ds[input.ProcId];
    //    return await _db.Ado.UseStoredProcedure().GetDataTableAsync(procName, input.ProcParams);
    //}
}