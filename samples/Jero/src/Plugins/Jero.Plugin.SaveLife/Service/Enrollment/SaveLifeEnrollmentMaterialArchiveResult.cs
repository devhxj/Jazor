namespace Jero.Plugin.SaveLife.Service;

/// <summary>
/// 将报名资料写入 ZIP 响应，避免为私有文件创建公开下载地址。
/// </summary>
internal sealed class SaveLifeEnrollmentMaterialArchiveResult : ActionResult
{
    private readonly IReadOnlyList<SaveLifeEnrollmentMaterial> _materials;
    private readonly ISaveLifeFileAccessService _fileAccessService;
    private readonly string _fileDownloadName;

    public SaveLifeEnrollmentMaterialArchiveResult(
        IReadOnlyList<SaveLifeEnrollmentMaterial> materials,
        ISaveLifeFileAccessService fileAccessService,
        string fileDownloadName)
    {
        _materials = materials;
        _fileAccessService = fileAccessService;
        _fileDownloadName = fileDownloadName;
    }

    public override async Task ExecuteResultAsync(ActionContext context)
    {
        var response = context.HttpContext.Response;
        response.ContentType = "application/zip";
        response.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
        {
            FileNameStar = _fileDownloadName
        }.ToString();

        using var archive = new ZipArchive(response.Body, ZipArchiveMode.Create, leaveOpen: true);
        var usedEntryNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var material in _materials)
        {
            var entry = archive.CreateEntry(
                GetUniqueEntryName(material, usedEntryNames),
                CompressionLevel.Fastest);
            await using var entryStream = entry.Open();
            await using var fileStream = await _fileAccessService.OpenReadAsync(material.FileId);
            await fileStream.CopyToAsync(entryStream, context.HttpContext.RequestAborted);
        }
    }

    private static string GetUniqueEntryName(
        SaveLifeEnrollmentMaterial material,
        ISet<string> usedEntryNames)
    {
        var fileName = BuildDisplayFileName(material);
        var extension = Path.GetExtension(fileName);
        var stem = Path.GetFileNameWithoutExtension(fileName);
        var candidate = fileName;
        var duplicateNumber = 2;
        while (!usedEntryNames.Add(candidate))
        {
            candidate = $"{stem} ({duplicateNumber}){extension}";
            duplicateNumber++;
        }

        return candidate;
    }

    private static string BuildDisplayFileName(SaveLifeEnrollmentMaterial material)
    {
        var fileName = material.FileName?.Trim();
        if (string.IsNullOrWhiteSpace(fileName))
            fileName = $"病例材料_{material.Id}";

        fileName = fileName.Replace('/', '_').Replace('\\', '_');
        fileName = Path.GetFileName(fileName);
        if (string.IsNullOrWhiteSpace(fileName) || fileName is "." or "..")
            fileName = $"病例材料_{material.Id}";

        var suffix = SaveLifeEnrollmentMaterialHelper.NormalizeExtension(material.FileSuffix);
        if (!string.IsNullOrWhiteSpace(suffix) &&
            !fileName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
        {
            fileName += suffix;
        }

        return string.Concat(fileName.Select(character =>
            Path.GetInvalidFileNameChars().Contains(character) ? '_' : character));
    }
}
