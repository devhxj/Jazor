


using MiniExcelLibs;
namespace Jero.Core;

public static class MiniExcelUtil
{
    private const string SheetName = "ImportTemplate";
    private const string DirectoryName = "export";

    /// <summary>
    /// 导出模板Excel
    /// </summary>
    /// <returns></returns>
    public static async Task<IActionResult> ExportExcelTemplate<T>(string? fileName = null) where T : class, new()
    {
        var values = Array.Empty<T>();
        // 在内存中当开辟空间
        var memoryStream = new MemoryStream();
        // 将数据写到内存当中
        await memoryStream.SaveAsAsync(values, sheetName: SheetName);
        // 从0的位置开始写入
        memoryStream.Seek(0, SeekOrigin.Begin);
        return new FileStreamResult(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
        {
            FileDownloadName = $"{(string.IsNullOrEmpty(fileName) ? typeof(T).Name : fileName)}.xlsx"
        };
    }

    /// <summary>
    /// 获取导入数据Excel
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public static async Task<IEnumerable<T>> GetImportExcelData<T>([Required] IFormFile file) where T : class, new()
    {
        using MemoryStream stream = new MemoryStream();
        await file.CopyToAsync(stream);
        stream.Position = 0;
        return await stream.QueryAsync<T>(sheetName: SheetName).ToArrayAsync();
    }

    /// <summary>
    /// 获取导出数据excel地址
    /// </summary>
    /// <returns></returns>
    public static async Task<string> GetExportDataExcelUrl<T>(IEnumerable<T> exportData) where T : class, new()
    {
        var fileName = string.Format("{0}.xlsx", YitIdHelper.NextId());
        try
        {
            var path = Path.Combine(App.WebHostEnvironment.WebRootPath, DirectoryName);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            var filePath = Path.Combine(path, fileName);
            await MiniExcel.SaveAsAsync(filePath, exportData, overwriteFile: true);
        }
        catch (Exception error)
        {
            throw Oops.Oh("出现错误：" + error);
        }
        var host = CommonUtil.GetLocalhost();
        return $"{host}/{DirectoryName}/{fileName}";
    }
}
