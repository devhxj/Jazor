namespace Jero.Core;

/// <summary>
/// Excel文件ActionResult
/// </summary>
/// <typeparam name="T"></typeparam>
public class XlsxFileResult<T> : XlsxFileResultBase where T : class, new()
{
    public string FileDownloadName { get; }
    public ICollection<T> Data { get; }

    /// <summary>
    ///
    /// </summary>
    /// <param name="data"></param>
    /// <param name="fileDownloadName"></param>
    public XlsxFileResult(ICollection<T> data, string? fileDownloadName = null)
    {
        FileDownloadName = fileDownloadName ?? $"{Guid.NewGuid():N}.xlsx";
        Data = data;
    }

    public override async Task ExecuteResultAsync(ActionContext context)
    {
        var exporter = new ExcelExporter();
        var bytes = await exporter.ExportAsByteArray(Data);
        var fs = new MemoryStream(bytes);
        await DownloadExcelFileAsync(context, fs, FileDownloadName);
    }
}

/// <summary>
///
/// </summary>
public class XlsxFileResult : XlsxFileResultBase
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="fileDownloadName"></param>
    public XlsxFileResult(Stream stream, string? fileDownloadName = null)
    {
        Stream = stream;
        FileDownloadName = fileDownloadName ?? $"{Guid.NewGuid():N}.xlsx";
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="fileDownloadName"></param>

    public XlsxFileResult(byte[] bytes, string? fileDownloadName = null)
    {
        Stream = new MemoryStream(bytes);
        FileDownloadName = fileDownloadName ?? $"{Guid.NewGuid():N}.xlsx";
    }

    public Stream Stream { get; protected set; }
    public string FileDownloadName { get; protected set; }

    public override async Task ExecuteResultAsync(ActionContext context)
    {
        await DownloadExcelFileAsync(context, Stream, FileDownloadName);
    }
}

/// <summary>
/// 基类
/// </summary>
public class XlsxFileResultBase : ActionResult
{
    /// <summary>
    /// 下载Excel文件
    /// </summary>
    /// <param name="context"></param>
    /// <param name="stream"></param>
    /// <param name="downloadFileName"></param>
    /// <returns></returns>
    protected virtual async Task DownloadExcelFileAsync(ActionContext context, Stream stream, string downloadFileName)
    {
        var response = context.HttpContext.Response;
        response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        downloadFileName ??= Guid.NewGuid().ToString("N") + ".xlsx";

        if (string.IsNullOrEmpty(Path.GetExtension(downloadFileName))) downloadFileName += ".xlsx";

        context.HttpContext.Response.Headers.Append("Content-Disposition", new[] { "attachment; filename=" + HttpUtility.UrlEncode(downloadFileName) });
        await stream.CopyToAsync(context.HttpContext.Response.Body);
    }
}
