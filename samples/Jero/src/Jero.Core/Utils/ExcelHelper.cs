


using OfficeOpenXml;
namespace Jero.Core;

public class ExcelHelper
{
    /// <summary>
    /// 数据导入
    /// </summary>
    /// <param name="file"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static IActionResult ImportData<IN, T>(IFormFile file, Action<List<IN>, Action<StorageableResult<T>, List<IN>, List<T>>> action) where IN : BaseImportInput, new() where T : EntityBaseId, new()
    {
        try
        {
            var result = CommonUtil.ImportExcelDataAsync<IN>(file).Result ?? throw Oops.Oh("有效数据为空");
            result.ForEach(u => u.Id = YitIdHelper.NextId());

            var tasks = new List<Task>();
            action.Invoke(result, (storageable, pageItems, rows) =>
            {
                // 标记校验信息
                tasks.Add(Task.Run(() =>
                {
                    if (!storageable.TotalList.Any()) return;

                    // 通过Id标记校验信息
                    var itemMap = pageItems.ToDictionary(u => u.Id, u => u);
                    foreach (var item in storageable.TotalList)
                    {
                        var temp = itemMap.GetValueOrDefault(item.Item.Id);
                        if (temp != null) temp.Error ??= item.StorageMessage;
                    }
                }));
            });

            // 等待所有标记验证信息任务完成
            Task.WhenAll(tasks).GetAwaiter().GetResult();

            // 仅导出错误记录
            var errorList = result.Where(u => !string.IsNullOrWhiteSpace(u.Error)).ToList();
            if (!errorList.Any())
                return new JsonResult(AdminResultProvider.Ok("导入成功"));
            return ExportData(errorList);
        }
        catch (Exception ex)
        {
            return new JsonResult(AdminResultProvider.Error(ex.Message));
        }
    }

    /// <summary>
    /// 导出Xlsx数据
    /// </summary>
    /// <param name="list"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static IActionResult ExportData(dynamic list, string fileName = "导入记录")
    {
        var exporter = new ExcelExporter();
        var fs = new MemoryStream(exporter.ExportAsByteArray(list).GetAwaiter().GetResult());
        return new XlsxFileResult(stream: fs, fileDownloadName: $"{fileName}-{DateTime.Now:yyyy-MM-dd_HHmmss}");
    }

    /// <summary>
    /// 根据类型导出Xlsx模板
    /// </summary>
    /// <param name="list"></param>
    /// <param name="filename"></param>
    /// <param name="addListValidationFun"></param>
    /// <returns></returns>
    public static IActionResult ExportTemplate<T>(IEnumerable<T> list, string filename = "导入模板", Func<ExcelWorksheet, PropertyInfo, IEnumerable<string>>? addListValidationFun = null)
    {
        using var package = new ExcelPackage((ExportData(list, filename) as XlsxFileResult)!.Stream);
        var worksheet = package.Workbook.Worksheets[0];

        // 创建一个隐藏的sheet，用于添加下拉列表
        var dropdownSheet = package.Workbook.Worksheets.Add("下拉数据");
        dropdownSheet.Hidden = eWorkSheetHidden.Hidden;

        var sysDictTypeService = App.GetService<SysDictTypeService>();
        foreach (var prop in typeof(T).GetProperties())
        {
            var propType = prop.PropertyType;

            var headerAttr = prop.GetCustomAttribute<ExporterHeaderAttribute>();
            if (Nullable.GetUnderlyingType(propType) is { IsEnum: true } enumType) propType = enumType;
            if (headerAttr == null) continue;

            // 获取列序号
            var columnIndex = 0;
            foreach (var item in worksheet.Cells[1, 1, 1, worksheet.Dimension.End.Column])
                if (++columnIndex > 0 && item.Text.Equals(headerAttr.DisplayName)) break;
            if (columnIndex <= 0) continue;

            // 优先从代理函数中获取下列列表，若为空且字段为枚举型，则填充枚举项为下列列表，若为字典字段，则填充字典值value列表为下列列表
            var dataList = addListValidationFun?.Invoke(worksheet, prop)?.ToList();
            if (dataList == null)
            {
                // 填充枚举项为下列列表
                if (propType.IsEnum())
                {
                    dataList = propType.EnumToList().Select(it => it.Describe).ToList();
                }
                else
                {
                    // 获取字段上的字典特性
                    var dict = prop.GetCustomAttribute<DictAttribute>();
                    if (dict != null)
                    {
                        // 填充字典值value为下列列表
                        dataList = sysDictTypeService.GetDataList(new GetDataDictTypeInput { Code = dict.DictTypeCode })
                            .Result?.Select(x => x.Label).ToList();
                    }
                }
            }

            if (dataList != null)
            {
                // 添加下拉列表
                AddListValidation(dropdownSheet, columnIndex, dataList);
                dropdownSheet.Cells[1, columnIndex, dataList.Count, columnIndex].LoadFromCollection(dataList);
            }
        }

        package.Save();
        package.Stream.Position = 0;
        return new XlsxFileResult(stream: package.Stream, fileDownloadName: $"{filename}-{DateTime.Now:yyyy-MM-dd_HHmmss}");

        void AddListValidation(ExcelWorksheet dropdownSheet, int columnIndex, List<string> dataList)
        {
            var validation = worksheet.DataValidations.AddListValidation(worksheet.Cells[2, columnIndex, ExcelPackage.MaxRows, columnIndex].Address);
            validation!.Formula.ExcelFormula = "=" + dropdownSheet.Cells[1, columnIndex, dataList.Count, columnIndex].FullAddressAbsolute;
            validation.ShowErrorMessage = true;
            validation.ErrorTitle = "无效输入";
            validation.Error = "请从列表中选择一个有效的选项";
        }
    }
}
