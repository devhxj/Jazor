namespace Jero.Plugin.SaveLife.Service;

public static class SaveLifeMaterialProcessingFailurePolicy
{
    private static readonly string[] PermanentFailureFragments =
    [
        "缺少用户归属",
        "PDF 已加密",
        "PDF 文件结构异常",
        "PDF 文件无法解析文本层",
        "PDF 页数超过",
        "PDF 字形数量超过",
        "旧版 Word 文档结构异常",
        "病例文档解析资源超限",
        "病例资料文件为空",
        "病例资料文件超过多模态解析限制",
        "病例文档实际内容超过 20MB",
        "DOCX 已加密",
        "DOCX 条目",
        "DOCX 包含",
        "DOCX XML",
        "DOCX 中央目录",
        "DOCX 不支持",
        "DOCX 缺少"
    ];

    public static bool IsPermanentFailure(string? error)
    {
        if (string.IsNullOrWhiteSpace(error))
            return false;

        if (SaveLifeKnowledgeBaseRetryPolicy.IsConfigurationError(error))
            return false;

        return PermanentFailureFragments.Any(fragment =>
            error.Contains(fragment, StringComparison.OrdinalIgnoreCase));
    }
}
