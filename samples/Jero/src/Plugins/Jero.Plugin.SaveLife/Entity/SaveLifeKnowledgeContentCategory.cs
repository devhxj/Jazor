namespace Jero.Plugin.SaveLife;

/// <summary>
/// 小程序医药资讯固定栏目。全部为聚合筛选，不写入文章。
/// </summary>
public static class SaveLifeKnowledgeContentCategory
{
    public const string Science = "science";
    // Keep the existing wire/storage key so previously classified articles remain visible.
    public const string RareDisease = "participant";
    public const string NewDrug = "new-drug";

    public static bool TryNormalize(string? value, out string normalized)
    {
        var compact = string.Concat((value ?? string.Empty).Where(u => !char.IsWhiteSpace(u)))
            .Trim()
            .ToLowerInvariant();
        normalized = compact switch
        {
            "" or Science or "科普" or "医学科普" or "健康科普" or "疾病知识" or "用药知识" => Science,
            RareDisease or "rare-disease" or "raredisease" or "罕见病" or "参与者故事" or "患者故事" or "受试者故事" => RareDisease,
            NewDrug or "newdrug" or "新药资讯" or "新药信息" => NewDrug,
            _ => string.Empty
        };
        return normalized.Length > 0;
    }

    public static string NormalizeOrDefault(string? value)
    {
        return TryNormalize(value, out var normalized) ? normalized : Science;
    }

    public static string GetLabel(string? value)
    {
        return NormalizeOrDefault(value) switch
        {
            RareDisease => "罕见病",
            NewDrug => "新药资讯",
            _ => "科普"
        };
    }
}
