namespace Jero.Plugin.SaveLife.Service;

public static class SaveLifeEnrollmentMaterialHelper
{
    public const int MaxMaterialListCount = 20;
    public const long MaxMaterialFileBytes = 20 * 1024 * 1024;
    public const string AllowedSuffixes = ".jpg.png.pdf.doc.docx.mp4.mov";
    public const string MissingMaterialEmbeddingConfigurationMessage = "未配置病例资料多模态向量模型";
    public const string MissingTextEmbeddingConfigurationMessage = "未配置病例资料文本向量模型";

    private static readonly Dictionary<string, string> MaterialTypeTexts = new(StringComparer.OrdinalIgnoreCase)
    {
        ["medical_record"] = "病例资料",
        ["diagnosis_report"] = "诊断报告",
        ["lab_report"] = "检验检查",
        ["imaging"] = "影像资料",
        ["other"] = "其他资料"
    };

    private static readonly Dictionary<string, string> StatusTexts = new(StringComparer.OrdinalIgnoreCase)
    {
        ["submitted"] = "已提交",
        ["approved"] = "已通过",
        ["need_supplement"] = "需补充",
        ["rejected"] = "已退回"
    };

    private static readonly Dictionary<string, string> ExtractStatusTexts = new(StringComparer.OrdinalIgnoreCase)
    {
        ["pending"] = "等待后台解析",
        ["processing"] = "后台解析中",
        ["synced"] = "已完成解析",
        ["failed"] = "解析失败",
        ["skipped"] = "已跳过自动解析"
    };

    private static readonly Dictionary<string, string> ExtensionContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = "image/jpeg",
        [".png"] = "image/png",
        [".pdf"] = "application/pdf",
        [".doc"] = "application/msword",
        [".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        [".mp4"] = "video/mp4",
        [".mov"] = "video/quicktime"
    };

    public static string NormalizeMaterialType(string? value)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
            return "medical_record";

        return MaterialTypeTexts.ContainsKey(normalized)
            ? normalized
            : throw Oops.Oh("材料类型不正确").StatusCode(400);
    }

    public static string GetMaterialTypeText(string? value)
    {
        return !string.IsNullOrWhiteSpace(value) && MaterialTypeTexts.TryGetValue(value, out var text)
            ? text
            : MaterialTypeTexts["other"];
    }

    public static string NormalizeStatus(string? value)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
            return "submitted";

        return StatusTexts.ContainsKey(normalized)
            ? normalized
            : throw Oops.Oh("材料状态不正确").StatusCode(400);
    }

    public static string GetStatusText(string? value)
    {
        return !string.IsNullOrWhiteSpace(value) && StatusTexts.TryGetValue(value, out var text)
            ? text
            : StatusTexts["submitted"];
    }

    public static string NormalizeExtension(string? fileName)
    {
        var normalized = fileName?.Trim() ?? string.Empty;
        var extension = normalized.StartsWith(".", StringComparison.Ordinal) && normalized.IndexOf('.', 1) < 0
            ? normalized
            : Path.GetExtension(normalized);
        extension = extension.ToLowerInvariant();
        return extension is ".jpeg" or ".jpe" ? ".jpg" : extension;
    }

    public static bool IsAllowedExtension(string? extension)
    {
        return !string.IsNullOrWhiteSpace(extension) &&
            ExtensionContentTypes.ContainsKey(extension);
    }

    public static string ResolveContentType(string extension, string? contentType)
    {
        if (!string.IsNullOrWhiteSpace(contentType) && contentType != "application/octet-stream")
            return contentType;

        return ExtensionContentTypes.TryGetValue(extension, out var mapped)
            ? mapped
            : contentType ?? "application/octet-stream";
    }

    public static bool IsImageOrVideo(string? extension, string? contentType)
    {
        var normalizedExtension = NormalizeExtension(extension);
        if (normalizedExtension is ".jpg" or ".png" or ".mp4" or ".mov")
            return true;

        return contentType?.StartsWith("image/", StringComparison.OrdinalIgnoreCase) == true ||
            contentType?.StartsWith("video/", StringComparison.OrdinalIgnoreCase) == true;
    }

    public static int ResolveNextUploadSlot(IEnumerable<int> occupiedSlots)
    {
        ArgumentNullException.ThrowIfNull(occupiedSlots);
        var usedSlots = occupiedSlots.Where(slot => slot > 0).ToHashSet();
        for (var slot = 1; slot < int.MaxValue; slot++)
        {
            if (!usedSlots.Contains(slot))
                return slot;
        }

        throw new InvalidOperationException("材料上传槽位已耗尽");
    }

    public static string GetExtractStatusText(string? value)
    {
        return !string.IsNullOrWhiteSpace(value) && ExtractStatusTexts.TryGetValue(value, out var text)
            ? text
            : ExtractStatusTexts["pending"];
    }

    public static bool IsPersonalMaterial(SaveLifeEnrollmentMaterial? material)
    {
        return material != null && material.EnrollmentRecordId <= 0;
    }

    public static bool IsEffectiveForEnrollment(
        SaveLifeEnrollmentMaterial? material,
        string externalUserId,
        long enrollmentRecordId)
    {
        return material != null &&
            !material.IsDelete &&
            string.Equals(material.ExternalUserId, externalUserId, StringComparison.Ordinal) &&
            (material.EnrollmentRecordId <= 0 || material.EnrollmentRecordId == enrollmentRecordId);
    }

    public static bool CanDeletePersonalMaterial(SaveLifeEnrollmentMaterial? material)
    {
        return IsPersonalMaterial(material) &&
            material!.Status is "submitted" or "need_supplement" or "rejected";
    }

    public static string FormatSize(long sizeKb)
    {
        if (sizeKb >= 1024)
            return $"{sizeKb / 1024D:0.#}MB";

        return $"{Math.Max(1, sizeKb)}KB";
    }

    public static string? NormalizeText(string? value, int maxLength, string fieldName)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
            return null;

        if (normalized.Length > maxLength)
            throw Oops.Oh($"{fieldName}不能超过{maxLength}个字符").StatusCode(400);

        return normalized;
    }
}
