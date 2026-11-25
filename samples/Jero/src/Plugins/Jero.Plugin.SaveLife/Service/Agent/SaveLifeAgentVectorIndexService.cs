namespace Jero.Plugin.SaveLife.Service;

/// <summary>
/// 助手向量索引运营服务
/// </summary>
[ApiDescriptionSettings(SaveLifeConst.GroupName, Order = 165)]
public class SaveLifeAgentVectorIndexService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<SaveLifeAgentVectorIndex> _indexRep;
    private readonly SqlSugarRepository<SaveLifeDiseaseKnowledge> _knowledgeRep;
    private readonly SqlSugarRepository<SaveLifeClinicalTrial> _trialRep;
    private readonly SqlSugarRepository<SaveLifeEnrollmentMaterial> _materialRep;
    private readonly ISaveLifePublicKnowledgeBaseIndexProcessor _knowledgeBaseIndexProcessor;
    private readonly ISaveLifeMaterialIndexProcessor _materialIndexProcessor;

    public SaveLifeAgentVectorIndexService(
        SqlSugarRepository<SaveLifeAgentVectorIndex> indexRep,
        SqlSugarRepository<SaveLifeDiseaseKnowledge> knowledgeRep,
        SqlSugarRepository<SaveLifeClinicalTrial> trialRep,
        SqlSugarRepository<SaveLifeEnrollmentMaterial> materialRep,
        ISaveLifePublicKnowledgeBaseIndexProcessor knowledgeBaseIndexProcessor,
        ISaveLifeMaterialIndexProcessor materialIndexProcessor)
    {
        _indexRep = indexRep;
        _knowledgeRep = knowledgeRep;
        _trialRep = trialRep;
        _materialRep = materialRep;
        _knowledgeBaseIndexProcessor = knowledgeBaseIndexProcessor;
        _materialIndexProcessor = materialIndexProcessor;
    }

    [HttpPost]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<SaveLifeAgentVectorIndex>> Page(SaveLifeAgentVectorIndexInput input)
    {
        var resourceType = NormalizeResourceTypeOrNull(input.ResourceType);
        var syncStatus = NormalizeSyncStatusOrNull(input.SyncStatus);
        var keyword = input.Keyword?.Trim();

        return await _indexRep.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(resourceType), u => u.ResourceType == resourceType)
            .WhereIF(!string.IsNullOrWhiteSpace(syncStatus), u => u.SyncStatus == syncStatus)
            .WhereIF(!string.IsNullOrWhiteSpace(keyword), u =>
                (u.Title != null && u.Title.Contains(keyword!)) ||
                (u.DiseaseName != null && u.DiseaseName.Contains(keyword!)) ||
                (u.Content != null && u.Content.Contains(keyword!)))
            .OrderBy(u => u.SyncStatus == "failed", OrderByType.Desc)
            .OrderBy(u => u.UpdateTime, OrderByType.Desc)
            .OrderBy(u => u.CreateTime, OrderByType.Desc)
            .ToPagedListAsync(input.Page, input.PageSize);
    }

    [HttpPost]
    [ApiDescriptionSettings(Name = "Retry")]
    public async Task Retry(RetrySaveLifeAgentVectorIndexInput input)
    {
        var index = await _indexRep.GetByIdAsync(input.Id)
            ?? throw Oops.Oh("向量索引不存在").StatusCode(404);

        if (index.ResourceType == "knowledge")
        {
            var knowledge = await _knowledgeRep.GetByIdAsync(index.ResourceId)
                ?? throw Oops.Oh("疾病知识不存在").StatusCode(404);
            await _knowledgeBaseIndexProcessor.QueueKnowledgeAsync(knowledge.Id);
            return;
        }

        if (index.ResourceType == "trial")
        {
            var trial = await _trialRep.GetByIdAsync(index.ResourceId)
                ?? throw Oops.Oh("招募项目不存在").StatusCode(404);
            await _knowledgeBaseIndexProcessor.QueueTrialAsync(trial.Id);
            return;
        }

        if (index.ResourceType == "material")
        {
            var material = await _materialRep.GetFirstAsync(u =>
                u.Id == index.ResourceId &&
                u.ExternalUserId == index.ExternalUserId)
                ?? throw Oops.Oh("报名材料不存在").StatusCode(404);
            await _materialIndexProcessor.QueueMaterialAsync(material.Id);
            return;
        }

        throw Oops.Oh("暂不支持该资源类型重试").StatusCode(400);
    }

    private static string? NormalizeResourceTypeOrNull(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var normalized = value.Trim().ToLowerInvariant();
        return normalized is "knowledge" or "trial" or "material"
            ? normalized
            : throw Oops.Oh("资源类型不正确").StatusCode(400);
    }

    private static string? NormalizeSyncStatusOrNull(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var normalized = value.Trim().ToLowerInvariant();
        return normalized is "pending" or "synced" or "failed"
            ? normalized
            : throw Oops.Oh("同步状态不正确").StatusCode(400);
    }
}
