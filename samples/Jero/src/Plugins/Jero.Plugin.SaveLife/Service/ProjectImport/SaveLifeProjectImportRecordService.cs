


using Furion.DatabaseAccessor;
using OfficeOpenXml;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
namespace Jero.Plugin.SaveLife.Service;

/// <summary>
/// 项目导入记录服务
/// </summary>
[ApiDescriptionSettings(SaveLifeConst.GroupName, Order = 150)]
public class SaveLifeProjectImportRecordService : IDynamicApiController, ITransient
{
    private const int MaxBlockCount = 100;
    private const int MaxProjectFileRowCount = 10000;
    private const long MaxProjectImportFileBytes = 30 * 1024 * 1024;
    private const long MaxKnowledgeImportFileBytes = 10 * 1024 * 1024;
    private const int MaxSourceTextLength = 200000;
    private const string ProjectImportTemplateFileName = "项目导入模板";
    private const string KnowledgeImportTemplateFileName = "疾病知识导入模板";
    private static readonly JsonSerializerOptions ProjectImportJsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };
    private static readonly string[] ProjectImportTemplateHeaders =
    [
        "CDE登记号",
        "方案编号",
        "试验题目",
        "药物名称",
        "申办方",
        "试验阶段",
        "疾病领域",
        "状态",
        "目标例数",
        "参与者类型",
        "适应症",
        "入选标准",
        "排除标准",
        "参与者权益",
        "伦理委员会",
        "机构名称",
        "主要研究者",
        "省",
        "城市",
        "国家/地区",
    ];

    private static readonly string[] ProjectImportOptionalTemplateHeaders =
    [
        "项目描述",
        "已招募人数",
        "开始日期",
        "结束日期",
        "试验周期",
        "各参加单位信息",
    ];

    private static readonly string[] ProjectImportLegacyCompatibilityHeaders =
    [
        "目标人数",
        "参与者类别",
        "受试者权益",
        "伦理委员会信息",
    ];

    private static readonly string[] ProjectImportDerivedHeaders =
    [
        "地点",
        "招募要求",
    ];

    private static readonly string[] ProjectImportKnownHeaders = ProjectImportTemplateHeaders
        .Concat(ProjectImportOptionalTemplateHeaders)
        .Concat(ProjectImportLegacyCompatibilityHeaders)
        .Concat(ProjectImportDerivedHeaders)
        .ToArray();

    private static readonly string[] ProjectImportBlockHeaders = ProjectImportTemplateHeaders
        .Concat(ProjectImportOptionalTemplateHeaders)
        .Concat(ProjectImportLegacyCompatibilityHeaders)
        .Concat(ProjectImportDerivedHeaders)
        .ToArray();

    private static readonly string[] KnowledgeImportTemplateHeaders =
    [
        "标题",
        "分类名称",
        "疾病名称",
        "作者",
        "标签（逗号分隔）",
        "封面图URL",
        "摘要",
        "正文内容",
    ];

    private static readonly string[] KnowledgeImportOptionalHeaders =
    [
        "发布状态",
        "浏览量",
        "点赞数",
        "排序",
    ];

    private static readonly Dictionary<string, string> ProjectImportHeaderAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["标题"] = "试验题目",
        ["项目标题"] = "试验题目",
        ["项目名称"] = "试验题目",
        ["研究题目"] = "试验题目",
        ["方案号"] = "方案编号",
        ["登记号"] = "CDE登记号",
        ["注册号"] = "CDE登记号",
        ["临床试验登记号"] = "CDE登记号",
        ["描述"] = "项目描述",
        ["简介"] = "项目描述",
        ["摘要"] = "项目描述",
        ["申办单位"] = "申办方",
        ["主办方"] = "申办方",
        ["阶段"] = "试验阶段",
        ["临床阶段"] = "试验阶段",
        ["领域"] = "疾病领域",
        ["疾病类型"] = "疾病领域",
        ["项目状态"] = "状态",
        ["招募状态"] = "状态",
        ["试验状态"] = "状态",
        ["目标招募人数"] = "目标人数",
        ["目标病例数"] = "目标例数",
        ["目标受试者例数"] = "目标例数",
        ["计划人数"] = "目标人数",
        ["招募人数"] = "目标人数",
        ["目标例数"] = "目标人数",
        ["入组人数"] = "已招募人数",
        ["已入组人数"] = "已招募人数",
        ["开始时间"] = "开始日期",
        ["结束时间"] = "结束日期",
        ["受试者类型"] = "参与者类别",
        ["参与者类型"] = "参与者类别",
        ["人群"] = "参与者类别",
        ["纳入标准"] = "入选标准",
        ["周期"] = "试验周期",
        ["权益"] = "受试者权益",
        ["补偿"] = "受试者权益",
        ["参与者权益"] = "受试者权益",
        ["伦理委员会"] = "伦理委员会信息",
        ["研究中心"] = "各参加单位信息",
        ["中心"] = "各参加单位信息",
        ["医院"] = "各参加单位信息",
        ["参加单位"] = "各参加单位信息",
        ["参加机构"] = "各参加单位信息",
        ["研究机构"] = "机构名称",
        ["临床试验机构"] = "机构名称",
        ["研究者"] = "主要研究者",
        ["PI"] = "主要研究者",
        ["省份"] = "省",
        ["国家地区"] = "国家/地区",
    };

    private static readonly Dictionary<string, string> KnowledgeImportHeaderAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["文章标题"] = "标题",
        ["知识标题"] = "标题",
        ["分类"] = "分类名称",
        ["栏目"] = "分类名称",
        ["疾病"] = "疾病名称",
        ["病种"] = "疾病名称",
        ["撰写人"] = "作者",
        ["标签"] = "标签（逗号分隔）",
        ["关键词"] = "标签（逗号分隔）",
        ["标签逗号分隔"] = "标签（逗号分隔）",
        ["封面图"] = "封面图URL",
        ["封面"] = "封面图URL",
        ["图片"] = "封面图URL",
        ["封面图片"] = "封面图URL",
        ["封面图地址"] = "封面图URL",
        ["封面图片地址"] = "封面图URL",
        ["封面图URL"] = "封面图URL",
        ["简介"] = "摘要",
        ["正文"] = "正文内容",
        ["内容"] = "正文内容",
        ["文章内容"] = "正文内容",
        ["发布"] = "发布状态",
        ["状态"] = "发布状态",
        ["是否发布"] = "发布状态",
        ["点赞"] = "点赞数",
    };

    private readonly SqlSugarRepository<SaveLifeProjectImportRecord> _recordRep;
    private readonly SqlSugarRepository<SaveLifeClinicalTrial> _trialRep;
    private readonly SqlSugarRepository<SaveLifeDiseaseKnowledge> _knowledgeRep;
    private readonly SqlSugarRepository<SaveLifeDiseaseCategory> _categoryRep;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ISaveLifePublicKnowledgeBaseIndexProcessor _knowledgeBaseIndexProcessor;

    public SaveLifeProjectImportRecordService(
        SqlSugarRepository<SaveLifeProjectImportRecord> recordRep,
        SqlSugarRepository<SaveLifeClinicalTrial> trialRep,
        SqlSugarRepository<SaveLifeDiseaseKnowledge> knowledgeRep,
        SqlSugarRepository<SaveLifeDiseaseCategory> categoryRep,
        IHttpClientFactory httpClientFactory,
        ISaveLifePublicKnowledgeBaseIndexProcessor knowledgeBaseIndexProcessor)
    {
        _recordRep = recordRep;
        _trialRep = trialRep;
        _knowledgeRep = knowledgeRep;
        _categoryRep = categoryRep;
        _httpClientFactory = httpClientFactory;
        _knowledgeBaseIndexProcessor = knowledgeBaseIndexProcessor;
    }

    /// <summary>
    /// 分页查询项目导入记录
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<SaveLifeProjectImportRecord>> Page(SaveLifeProjectImportRecordInput input)
    {
        return await _recordRep.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(input.ImportType), u => u.ImportType == input.ImportType)
            .WhereIF(!string.IsNullOrWhiteSpace(input.InputMode), u => u.InputMode == input.InputMode)
            .WhereIF(!string.IsNullOrWhiteSpace(input.Status), u => u.Status == input.Status)
            .WhereIF(input.ProjectId > 0, u => u.ProjectId == input.ProjectId)
            .OrderBy(u => u.CreateTime, OrderByType.Desc)
            .ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 新增项目导入记录
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Add")]
    public async Task<long> Add(AddSaveLifeProjectImportRecordInput input)
    {
        var entity = input.Adapt<SaveLifeProjectImportRecord>();
        await _recordRep.InsertAsync(entity);
        return entity.Id;
    }

    /// <summary>
    /// 更新项目导入记录
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Update")]
    public async Task Update(UpdateSaveLifeProjectImportRecordInput input)
    {
        await _recordRep.AsUpdateable(input.Adapt<SaveLifeProjectImportRecord>()).IgnoreColumns(true).ExecuteCommandAsync();
    }

    /// <summary>
    /// 删除项目导入记录
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Delete")]
    public async Task Delete(DeleteSaveLifeProjectImportRecordInput input)
    {
        var entity = await _recordRep.GetByIdAsync(input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await _recordRep.FakeDeleteAsync(entity);
    }

    /// <summary>
    /// 获取项目导入记录详情
    /// </summary>
    public async Task<SaveLifeProjectImportRecord> GetDetail([FromQuery] QueryByIdSaveLifeProjectImportRecordInput input)
    {
        return await _recordRep.GetByIdAsync(input.Id);
    }

    /// <summary>
    /// 下载临床试验项目导入模板
    /// </summary>
    [HttpGet]
    [NonUnify]
    [ApiDescriptionSettings(Name = "DownloadProjectTemplate")]
    public IActionResult DownloadProjectTemplate()
    {
        return ExcelHelper.ExportTemplate(
            [BuildProjectImportTemplateSample()],
            ProjectImportTemplateFileName);
    }

    /// <summary>
    /// 下载疾病知识文章导入模板
    /// </summary>
    [HttpGet]
    [NonUnify]
    [ApiDescriptionSettings(Name = "DownloadKnowledgeTemplate")]
    public IActionResult DownloadKnowledgeTemplate()
    {
        return ExcelHelper.ExportTemplate(
            [BuildKnowledgeImportTemplateSample()],
            KnowledgeImportTemplateFileName);
    }

    /// <summary>
    /// 导入粘贴的临床试验项目文本
    /// </summary>
    [HttpPost]
    [UnitOfWork]
    [ApiDescriptionSettings(Name = "ImportProjectText")]
    public async Task<SaveLifeTextImportResult> ImportProjectText(ImportSaveLifeProjectTextInput input)
    {
        var sourceText = NormalizeSourceText(input.SourceText);
        var blocks = SplitImportBlocks(sourceText);
        var failures = new List<string>();
        var imported = new List<SaveLifeClinicalTrial>();
        var skippedCount = 0;
        var projectNumberIndex = await LoadExistingProjectNumberIndex();

        for (var i = 0; i < blocks.Count; i++)
        {
            var block = ParseTextBlock(blocks[i]);
            var entity = BuildProject(block);
            if (!TryPrepareRequiredCdeRegistrationNumber(entity))
            {
                skippedCount++;
                failures.Add(BuildMissingCdeSkipMessage(i + 1, "条"));
                continue;
            }

            if (string.IsNullOrWhiteSpace(entity.Title))
            {
                failures.Add($"第 {i + 1} 条缺少试验题目");
                continue;
            }

            var candidateKeys = BuildProjectNumberKeys(entity);
            if (FindProjectIdsByNumberKeys(projectNumberIndex.ProjectIdsByNumberKey, candidateKeys).Count > 0)
            {
                skippedCount++;
                failures.Add(BuildDuplicateCdeSkipMessage(i + 1, "条", entity.CdeRegistrationNumber));
                continue;
            }

            try
            {
                await _trialRep.InsertAsync(entity);
                ReplaceProjectNumberKeys(
                    projectNumberIndex.ProjectIdsByNumberKey,
                    projectNumberIndex.NumberKeysByProjectId,
                    entity.Id,
                    candidateKeys);
                imported.Add(entity);
            }
            catch (Exception ex)
            {
                failures.Add($"第 {i + 1} 条导入失败：{ex.Message}");
            }
        }

        return await SaveImportRecord(
            "project",
            input.InputMode,
            sourceText,
            blocks.Count,
            imported.Count,
            failures,
            imported.FirstOrDefault(),
            skippedCount: skippedCount,
            useProjectActionCounts: true);
    }

    /// <summary>
    /// 导入临床试验项目文件
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "ImportProjectFile")]
    public async Task<SaveLifeTextImportResult> ImportProjectFile([FromForm] ImportSaveLifeProjectFileInput input)
    {
        var (file, extension) = ValidateProjectImportFile(input.File);
        var rows = extension switch
        {
            ".csv" => await ReadProjectRowsFromCsv(file),
            ".xlsx" => await ReadProjectRowsFromXlsx(file),
            _ => throw Oops.Oh("仅支持 .xlsx 和 .csv 文件"),
        };

        if (rows.Count == 0) throw Oops.Oh("导入文件中没有可导入的数据");
        if (rows.Count > MaxProjectFileRowCount) throw Oops.Oh($"项目文件一次最多导入 {MaxProjectFileRowCount} 条记录");

        var failures = new List<string>();
        var affectedProjects = new List<SaveLifeClinicalTrial>();
        var insertedCount = 0;
        var overwrittenCount = 0;
        var skippedCount = 0;
        var candidates = rows
            .Select(row => new ProjectImportCandidate(
                row,
                BuildProjectFromImportRow(row)))
            .ToList();
        var projectNumberIndex = await LoadExistingProjectNumberIndex();

        foreach (var candidate in candidates)
        {
            var row = candidate.Row;
            var entity = candidate.Entity;
            if (!TryPrepareRequiredCdeRegistrationNumber(entity))
            {
                skippedCount++;
                failures.Add(BuildMissingCdeSkipMessage(row.RowNumber, "行"));
                continue;
            }

            if (string.IsNullOrWhiteSpace(entity.Title))
            {
                failures.Add($"第 {row.RowNumber} 行缺少试验题目");
                continue;
            }

            var candidateKeys = BuildProjectNumberKeys(entity);
            var matchKeys = BuildProjectMatchKeys(entity);
            var ambiguousKey = matchKeys.FirstOrDefault(key =>
                projectNumberIndex.ProjectIdsByNumberKey.TryGetValue(key, out var projectIds) &&
                projectIds.Count > 1);
            if (ambiguousKey != null)
            {
                failures.Add(
                    $"第 {row.RowNumber} 行编号冲突：系统中相同{GetProjectNumberLabel(ambiguousKey)}对应多个项目，请先处理重复编号。");
                continue;
            }

            var matchedProjectIds = FindProjectIdsByNumberKeys(
                projectNumberIndex.ProjectIdsByNumberKey,
                matchKeys);
            if (matchedProjectIds.Count > 1)
            {
                failures.Add(
                    $"第 {row.RowNumber} 行编号冲突：同一编号对应多个项目，未执行导入。");
                continue;
            }

            if (matchedProjectIds.Count == 1)
            {
                var existingProjectId = matchedProjectIds[0];
                if (!input.OverwriteExisting)
                {
                    skippedCount++;
                    failures.Add(BuildDuplicateCdeSkipMessage(
                        row.RowNumber,
                        "行",
                        entity.CdeRegistrationNumber));
                    AssociateProjectNumberKeys(
                        projectNumberIndex.ProjectIdsByNumberKey,
                        projectNumberIndex.NumberKeysByProjectId,
                        existingProjectId,
                        candidateKeys);
                    continue;
                }

                try
                {
                    PrepareProjectOverwrite(entity, existingProjectId);
                    await OverwriteProjectFromImport(entity);
                    await _knowledgeBaseIndexProcessor.QueueTrialAsync(existingProjectId);
                    ReplaceProjectNumberKeys(
                        projectNumberIndex.ProjectIdsByNumberKey,
                        projectNumberIndex.NumberKeysByProjectId,
                        existingProjectId,
                        candidateKeys);
                    overwrittenCount++;
                    affectedProjects.Add(entity);
                }
                catch (Exception ex)
                {
                    failures.Add($"第 {row.RowNumber} 行覆盖失败：{ex.Message}");
                }

                continue;
            }

            try
            {
                await _trialRep.InsertAsync(entity);
                ReplaceProjectNumberKeys(
                    projectNumberIndex.ProjectIdsByNumberKey,
                    projectNumberIndex.NumberKeysByProjectId,
                    entity.Id,
                    candidateKeys);
                insertedCount++;
                affectedProjects.Add(entity);
            }
            catch (Exception ex)
            {
                failures.Add($"第 {row.RowNumber} 行导入失败：{ex.Message}");
            }
        }

        var sourceText = BuildProjectFileSourceText(file.FileName, rows);
        return await SaveImportRecord(
            "project",
            extension.TrimStart('.'),
            sourceText,
            rows.Count,
            insertedCount + overwrittenCount,
            failures,
            affectedProjects.FirstOrDefault(),
            skippedCount,
            overwrittenCount,
            useProjectActionCounts: true);
    }

    /// <summary>
    /// 导入粘贴的疾病知识文本
    /// </summary>
    [HttpPost]
    [UnitOfWork]
    [ApiDescriptionSettings(Name = "ImportKnowledgeText")]
    public async Task<SaveLifeTextImportResult> ImportKnowledgeText(ImportSaveLifeKnowledgeTextInput input)
    {
        var sourceText = NormalizeSourceText(input.SourceText);
        var blocks = SplitImportBlocks(sourceText);
        var failures = new List<string>();
        var imported = new List<SaveLifeDiseaseKnowledge>();
        var categories = await _categoryRep.AsQueryable().ToListAsync();

        for (var i = 0; i < blocks.Count; i++)
        {
            var block = ParseTextBlock(blocks[i]);
            var entity = BuildKnowledge(block, categories);
            if (string.IsNullOrWhiteSpace(entity.Title))
            {
                failures.Add($"第 {i + 1} 条缺少文章标题");
                continue;
            }

            try
            {
                await _knowledgeRep.InsertAsync(entity);
                imported.Add(entity);
            }
            catch (Exception ex)
            {
                failures.Add($"第 {i + 1} 条导入失败：{ex.Message}");
            }
        }

        return await SaveImportRecord("knowledge", input.InputMode, sourceText, blocks.Count, imported.Count, failures);
    }

    /// <summary>
    /// 导入疾病知识文章文件
    /// </summary>
    [HttpPost]
    [UnitOfWork]
    [ApiDescriptionSettings(Name = "ImportKnowledgeFile")]
    public async Task<SaveLifeTextImportResult> ImportKnowledgeFile([FromForm] ImportSaveLifeKnowledgeFileInput input)
    {
        var (file, extension) = ValidateKnowledgeImportFile(input.File);
        var rows = extension switch
        {
            ".csv" => await ReadKnowledgeRowsFromCsv(file),
            ".xlsx" => await ReadKnowledgeRowsFromXlsx(file),
            _ => throw Oops.Oh("仅支持 .xlsx 和 .csv 文件"),
        };

        if (rows.Count == 0) throw Oops.Oh("导入文件中没有可导入的数据");
        if (rows.Count > MaxBlockCount) throw Oops.Oh($"一次最多导入 {MaxBlockCount} 条记录");

        var failures = new List<string>();
        var imported = new List<SaveLifeDiseaseKnowledge>();
        var categories = await _categoryRep.AsQueryable().ToListAsync();

        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            var block = ParseTextBlock(BuildKnowledgeImportBlock(row));
            var entity = BuildKnowledge(block, categories);
            if (string.IsNullOrWhiteSpace(entity.Title))
            {
                failures.Add($"第 {row.RowNumber} 行缺少标题");
                continue;
            }

            try
            {
                var categoryName = row.Get("分类名称");
                if (!string.IsNullOrWhiteSpace(categoryName))
                {
                    entity.CategoryId = await ResolveKnowledgeCategoryId(categoryName, categories);
                }

                await _knowledgeRep.InsertAsync(entity);
                imported.Add(entity);
            }
            catch (Exception ex)
            {
                failures.Add($"第 {row.RowNumber} 行导入失败：{ex.Message}");
            }
        }

        var sourceText = BuildKnowledgeFileSourceText(file.FileName, rows);
        return await SaveImportRecord("knowledge", extension.TrimStart('.'), sourceText, rows.Count, imported.Count, failures);
    }

    /// <summary>
    /// 从药物临床试验登记平台自动导入项目
    /// </summary>
    [HttpPost]
    [UnitOfWork]
    [ApiDescriptionSettings(Name = "AutoImportProjects")]
    public async Task<SaveLifeTextImportResult> AutoImportProjects(AutoImportSaveLifeProjectInput input)
    {
        var client = _httpClientFactory.CreateClient(SaveLifeConst.ClinicalTrialHttpClientName);
        var requestPath = BuildClinicalTrialSearchPath(input);
        var sourceUrl = new Uri(client.BaseAddress!, requestPath).ToString();
        var failures = new List<string>();
        var imported = new List<SaveLifeClinicalTrial>();
        var skippedCount = 0;

        string html;
        try
        {
            html = await client.GetStringAsync(requestPath);
        }
        catch (Exception ex)
        {
            failures.Add($"自动抓取失败：{ex.Message}");
            return await SaveImportRecord("project-auto", "api", sourceUrl, 0, 0, failures);
        }

        if (LooksLikeShieldPage(html))
        {
            failures.Add("目标站点返回防护页面或非列表内容，自动导入未获取到有效项目。");
            return await SaveImportRecord("project-auto", "api", sourceUrl, 0, 0, failures);
        }

        var candidates = ParseClinicalTrialSearchHtml(html)
            .Take(Math.Min(input.ImportLimit, MaxBlockCount))
            .ToList();

        if (candidates.Count == 0)
        {
            failures.Add("未从目标站点解析到临床试验项目。");
            return await SaveImportRecord("project-auto", "api", sourceUrl, 0, 0, failures);
        }

        for (var i = 0; i < candidates.Count; i++)
        {
            var entity = candidates[i];
            if (!TryPrepareRequiredCdeRegistrationNumber(entity))
            {
                skippedCount++;
                failures.Add(BuildMissingCdeSkipMessage(i + 1, "条"));
                continue;
            }

            if (string.IsNullOrWhiteSpace(entity.Title))
            {
                failures.Add($"第 {i + 1} 条缺少试验题目");
                continue;
            }

            if (await _trialRep.AsQueryable().AnyAsync(u => u.CdeRegistrationNumber == entity.CdeRegistrationNumber))
            {
                skippedCount++;
                failures.Add(BuildDuplicateCdeSkipMessage(i + 1, "条", entity.CdeRegistrationNumber));
                continue;
            }

            try
            {
                await _trialRep.InsertAsync(entity);
                imported.Add(entity);
            }
            catch (Exception ex)
            {
                failures.Add($"第 {i + 1} 条导入失败：{ex.Message}");
            }
        }

        return await SaveImportRecord(
            "project-auto",
            "api",
            sourceUrl,
            candidates.Count,
            imported.Count,
            failures,
            imported.FirstOrDefault(),
            skippedCount: skippedCount,
            useProjectActionCounts: true);
    }

    private async Task<SaveLifeTextImportResult> SaveImportRecord(
        string importType,
        string? inputMode,
        string sourceText,
        int totalCount,
        int successCount,
        List<string> failures,
        SaveLifeClinicalTrial? firstProject = null,
        int skippedCount = 0,
        int overwrittenCount = 0,
        bool useProjectActionCounts = false)
    {
        var failedCount = CalculateImportFailedCount(totalCount, successCount, skippedCount);
        var insertedCount = Math.Max(0, successCount - overwrittenCount);
        var resultMessage = useProjectActionCounts
            ? BuildProjectActionResultMessage(
                totalCount,
                insertedCount,
                overwrittenCount,
                skippedCount,
                failedCount,
                failures)
            : BuildResultMessage(totalCount, successCount, failedCount, failures);
        var record = new SaveLifeProjectImportRecord
        {
            ImportType = importType,
            InputMode = Limit(inputMode, 64) ?? "text",
            Status = successCount > 0 || skippedCount > 0 ? "success" : "failed",
            ProjectId = firstProject?.Id,
            ProjectTitle = firstProject?.Title,
            SourceText = sourceText,
            ResultMessage = resultMessage,
            TotalCount = totalCount,
            SuccessCount = successCount,
            FailedCount = failedCount,
        };

        await _recordRep.InsertAsync(record);
        return new SaveLifeTextImportResult
        {
            RecordId = record.Id,
            TotalCount = totalCount,
            SuccessCount = successCount,
            FailedCount = failedCount,
            SkippedCount = skippedCount,
            ResultMessage = resultMessage,
        };
    }

    private static SaveLifeProjectImportTemplateRow BuildProjectImportTemplateSample()
    {
        return new SaveLifeProjectImportTemplateRow
        {
            CdeRegistrationNumber = "CTR20260001",
            ProtocolNumber = "SL-PD1-III-001",
            Title = "示例PD-1抑制剂治疗晚期肺癌III期临床试验",
            DrugName = "示例PD-1抑制剂",
            Sponsor = "示例生物医药有限公司",
            ClinicalPhase = "III期",
            DiseaseArea = "肿瘤",
            Status = "招募中",
            TargetCaseCount = "200",
            ParticipantTypeText = "晚期肺癌患者",
            Indication = "晚期非小细胞肺癌",
            InclusionCriteria = "18-75岁；经病理确诊；签署知情同意书",
            ExclusionCriteria = "妊娠或哺乳期；严重心肝肾功能异常",
            ParticipantBenefits = "交通补贴及必要检查费用",
            EthicsCommittee = "示例医院伦理委员会",
            InstitutionNames = "示例医院; 示例肿瘤医院",
            PrincipalInvestigators = "张三; 李四",
            Provinces = "浙江省; 上海市",
            Cities = "杭州市; 上海市",
            Countries = "中国; 中国",
            Description = "多中心、随机、双盲临床研究",
            EnrolledCount = "35",
            StartDate = "2026-01-01",
            EndDate = "2027-12-31",
            TrialDuration = "24个月",
            Sites = "示例医院 / 张三 / 中国 / 浙江省 / 杭州",
        };
    }

    private static SaveLifeKnowledgeImportTemplateRow BuildKnowledgeImportTemplateSample()
    {
        return new SaveLifeKnowledgeImportTemplateRow
        {
            Title = "示例：高血压的预防与治疗",
            CategoryName = "心血管疾病",
            DiseaseName = "高血压",
            Author = "张医生",
            Tags = "高血压,预防,治疗",
            ImageUrl = "https://example.com/image.jpg",
            Summary = "本文介绍高血压的预防和治疗方法",
            Content = "高血压是一种常见的慢性疾病，建议定期监测血压并遵医嘱治疗。",
        };
    }

    private static (IFormFile File, string Extension) ValidateProjectImportFile(IFormFile? file)
    {
        if (file == null || file.Length <= 0) throw Oops.Oh("导入文件不能为空");
        if (file.Length > MaxProjectImportFileBytes) throw Oops.Oh("导入文件不能超过 30MB");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        return extension switch
        {
            ".xlsx" or ".csv" => (file, extension),
            ".xls" => throw Oops.Oh("暂不支持旧版 .xls，请另存为 .xlsx 后导入"),
            _ => throw Oops.Oh("仅支持 .xlsx 和 .csv 文件"),
        };
    }

    private static (IFormFile File, string Extension) ValidateKnowledgeImportFile(IFormFile? file)
    {
        if (file == null || file.Length <= 0) throw Oops.Oh("导入文件不能为空");
        if (file.Length > MaxKnowledgeImportFileBytes) throw Oops.Oh("导入文件不能超过 10MB");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        return extension switch
        {
            ".xlsx" or ".csv" => (file, extension),
            ".xls" => throw Oops.Oh("暂不支持旧版 .xls，请另存为 .xlsx 后导入"),
            _ => throw Oops.Oh("仅支持 .xlsx 和 .csv 文件"),
        };
    }

    private async Task<ProjectNumberIndex> LoadExistingProjectNumberIndex()
    {
        var existingProjects = await _trialRep.AsQueryable()
            .Select(u => new SaveLifeClinicalTrial
            {
                Id = u.Id,
                CdeRegistrationNumber = u.CdeRegistrationNumber,
            })
            .ToListAsync();
        var projectNumberIndex = new ProjectNumberIndex();

        foreach (var project in existingProjects)
        {
            ReplaceProjectNumberKeys(
                projectNumberIndex.ProjectIdsByNumberKey,
                projectNumberIndex.NumberKeysByProjectId,
                project.Id,
                BuildProjectNumberKeys(project));
        }

        return projectNumberIndex;
    }

    private async Task OverwriteProjectFromImport(SaveLifeClinicalTrial entity)
    {
        var affectedRows = await _trialRep.AsUpdateable(entity)
            .WhereColumns(u => new { u.Id })
            .UpdateColumns(u => new
            {
                u.Title,
                u.ProtocolNumber,
                u.CdeRegistrationNumber,
                u.Description,
                u.Sponsor,
                u.ClinicalPhase,
                u.DiseaseArea,
                u.Indication,
                u.Status,
                u.TargetCount,
                u.EnrolledCount,
                u.StartDate,
                u.EndDate,
                u.ParticipantType,
                u.InclusionCriteria,
                u.ExclusionCriteria,
                u.TrialDuration,
                u.Benefits,
                u.EthicsCommitteeInfo,
                u.Sites,
                u.Location,
                u.Requirements,
                u.IsInKnowledgeBase,
                u.KnowledgeBaseError,
                u.KnowledgeBaseRetryCount,
                u.KnowledgeBaseLastAttemptTime,
                u.KnowledgeBaseNextRetryTime,
                u.UpdateTime,
            })
            .ExecuteCommandAsync();

        if (affectedRows != 1)
            throw new InvalidOperationException("匹配的项目不存在或已被删除");
    }

    private static List<string> BuildProjectNumberKeys(SaveLifeClinicalTrial trial)
    {
        var keys = new List<string>();
        var cdeRegistrationNumber = NormalizeBusinessKeyPart(trial.CdeRegistrationNumber);

        if (!string.IsNullOrWhiteSpace(cdeRegistrationNumber))
            keys.Add($"cde:{cdeRegistrationNumber}");

        return keys;
    }

    private static List<string> BuildProjectMatchKeys(SaveLifeClinicalTrial trial)
    {
        return BuildProjectNumberKeys(trial);
    }

    private static bool TryPrepareRequiredCdeRegistrationNumber(SaveLifeClinicalTrial trial)
    {
        var normalized = SaveLifeCdeRegistrationNumber.Normalize(trial.CdeRegistrationNumber);
        if (string.IsNullOrWhiteSpace(normalized))
            return false;

        trial.CdeRegistrationNumber = normalized;
        return true;
    }

    private static string BuildMissingCdeSkipMessage(int position, string unit)
    {
        return $"第 {position} {unit}缺少 CDE 登记号，已跳过。";
    }

    private static string BuildDuplicateCdeSkipMessage(
        int position,
        string unit,
        string cdeRegistrationNumber)
    {
        return $"第 {position} {unit}CDE 登记号“{cdeRegistrationNumber}”已存在，已跳过。";
    }

    private static int CalculateImportFailedCount(int totalCount, int successCount, int skippedCount)
    {
        return Math.Max(0, totalCount - successCount - skippedCount);
    }

    private static List<long> FindProjectIdsByNumberKeys(
        Dictionary<string, HashSet<long>> numberIndex,
        IEnumerable<string> numberKeys)
    {
        return numberKeys
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Where(numberIndex.ContainsKey)
            .SelectMany(key => numberIndex[key])
            .Distinct()
            .OrderBy(projectId => projectId)
            .ToList();
    }

    private static void ReplaceProjectNumberKeys(
        Dictionary<string, HashSet<long>> numberIndex,
        Dictionary<long, HashSet<string>> keysByProjectId,
        long projectId,
        IEnumerable<string> numberKeys)
    {
        if (keysByProjectId.TryGetValue(projectId, out var existingKeys))
        {
            foreach (var key in existingKeys)
            {
                if (!numberIndex.TryGetValue(key, out var projectIds))
                    continue;

                projectIds.Remove(projectId);
                if (projectIds.Count == 0)
                    numberIndex.Remove(key);
            }

            keysByProjectId.Remove(projectId);
        }

        AssociateProjectNumberKeys(numberIndex, keysByProjectId, projectId, numberKeys);
    }

    private static void AssociateProjectNumberKeys(
        Dictionary<string, HashSet<long>> numberIndex,
        Dictionary<long, HashSet<string>> keysByProjectId,
        long projectId,
        IEnumerable<string> numberKeys)
    {
        var keys = numberKeys
            .Where(key => !string.IsNullOrWhiteSpace(key))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (keys.Count == 0)
            return;

        if (!keysByProjectId.TryGetValue(projectId, out var projectKeys))
        {
            projectKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            keysByProjectId.Add(projectId, projectKeys);
        }

        foreach (var key in keys)
        {
            projectKeys.Add(key);
            if (!numberIndex.TryGetValue(key, out var projectIds))
            {
                projectIds = [];
                numberIndex.Add(key, projectIds);
            }

            projectIds.Add(projectId);
        }
    }

    private static string GetProjectNumberLabel(string numberKey)
    {
        return numberKey.StartsWith("cde:", StringComparison.OrdinalIgnoreCase)
            ? "CDE登记号"
            : "方案编号";
    }

    private static void PrepareProjectOverwrite(SaveLifeClinicalTrial trial, long existingId)
    {
        trial.Id = existingId;
        trial.IsInKnowledgeBase = false;
        trial.KnowledgeBaseError = null;
        trial.KnowledgeBaseRetryCount = 0;
        trial.KnowledgeBaseLastAttemptTime = null;
        trial.KnowledgeBaseNextRetryTime = null;
        trial.UpdateTime = DateTime.Now;
    }

    private static string? NormalizeBusinessKeyPart(string? value)
    {
        var normalized = value?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(normalized))
            return null;

        normalized = Regex.Replace(normalized, @"[\s\p{P}\p{S}]+", string.Empty);
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static Task<List<ProjectImportRow>> ReadProjectRowsFromXlsx(IFormFile file)
    {
        try
        {
            using var stream = file.OpenReadStream();
            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets.FirstOrDefault(u => u.Dimension != null)
                ?? throw Oops.Oh("XLSX 文件中没有可读取的工作表");
            var dimension = worksheet.Dimension ?? throw Oops.Oh("XLSX 文件中没有可读取的数据");
            var headerRow = FindProjectHeaderRow(worksheet);
            if (headerRow <= 0) throw Oops.Oh("XLSX 模板至少需要“试验题目”和“CDE登记号”列");

            var headers = ReadXlsxRow(worksheet, headerRow, dimension.End.Column)
                .Select(NormalizeProjectImportHeader)
                .ToList();

            var rows = Enumerable.Range(headerRow + 1, dimension.End.Row - headerRow)
                .Select(rowNumber => ProjectImportRow.FromCells(
                    rowNumber,
                    headers,
                    ReadXlsxRow(worksheet, rowNumber, dimension.End.Column)))
                .Where(row => !row.IsEmpty)
                .ToList();

            return Task.FromResult(rows);
        }
        catch (Exception ex)
        {
            throw Oops.Oh($"XLSX 文件解析失败：{ex.Message}");
        }
    }

    private static async Task<List<KnowledgeImportRow>> ReadKnowledgeRowsFromXlsx(IFormFile file)
    {
        try
        {
            var importedRows = await CommonUtil.ImportExcelData<ImportSaveLifeKnowledgeRowInput>(file);
            return importedRows
                .Select((row, index) => KnowledgeImportRow.FromImportInput(index + 2, row))
                .Where(row => !row.IsEmpty)
                .ToList();
        }
        catch (Exception ex)
        {
            throw Oops.Oh($"XLSX 文件解析失败：{ex.Message}");
        }
    }

    private static int FindProjectHeaderRow(ExcelWorksheet worksheet)
    {
        var dimension = worksheet.Dimension;
        if (dimension == null) return 0;

        var maxScanRow = Math.Min(dimension.End.Row, dimension.Start.Row + 9);
        for (var rowNumber = dimension.Start.Row; rowNumber <= maxScanRow; rowNumber++)
        {
            var headers = ReadXlsxRow(worksheet, rowNumber, dimension.End.Column)
                .Select(NormalizeProjectImportHeader)
                .ToList();
            if (IsProjectImportHeaderRow(headers))
            {
                return rowNumber;
            }
        }

        return 0;
    }

    private static bool IsProjectImportHeaderRow(IReadOnlyCollection<string> headers)
    {
        return headers.Contains("试验题目")
            && headers.Contains("CDE登记号");
    }

    private static List<string> ReadXlsxRow(ExcelWorksheet worksheet, int rowNumber, int maxColumn)
    {
        var cells = new List<string>(maxColumn);
        for (var columnNumber = 1; columnNumber <= maxColumn; columnNumber++)
        {
            cells.Add(worksheet.Cells[rowNumber, columnNumber].Text?.Trim() ?? string.Empty);
        }

        return cells;
    }

    private static async Task<List<ProjectImportRow>> ReadProjectRowsFromCsv(IFormFile file)
    {
        await using var stream = file.OpenReadStream();
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        var csvText = DecodeCsv(memoryStream.ToArray());
        var csvRows = ParseCsvRows(csvText);

        if (csvRows.Count == 0) return [];

        var headers = csvRows[0]
            .Select(NormalizeProjectImportHeader)
            .ToList();

        if (!IsProjectImportHeaderRow(headers))
        {
            throw Oops.Oh("CSV 模板至少需要“试验题目”和“CDE登记号”列");
        }

        return csvRows
            .Skip(1)
            .Select((cells, index) => ProjectImportRow.FromCells(index + 2, headers, cells))
            .Where(row => !row.IsEmpty)
            .ToList();
    }

    private static async Task<List<KnowledgeImportRow>> ReadKnowledgeRowsFromCsv(IFormFile file)
    {
        await using var stream = file.OpenReadStream();
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        var csvText = DecodeCsv(memoryStream.ToArray());
        var csvRows = ParseCsvRows(csvText);

        if (csvRows.Count == 0) return [];

        var headers = csvRows[0]
            .Select(NormalizeKnowledgeImportHeader)
            .ToList();

        if (!headers.Contains("标题"))
        {
            throw Oops.Oh("CSV 模板缺少“标题”列");
        }

        return csvRows
            .Skip(1)
            .Select((cells, index) => KnowledgeImportRow.FromCells(index + 2, headers, cells))
            .Where(row => !row.IsEmpty)
            .ToList();
    }

    private static string DecodeCsv(byte[] bytes)
    {
        if (bytes.Length == 0) return string.Empty;

        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
        {
            return Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
        }

        try
        {
            return new UTF8Encoding(false, true).GetString(bytes);
        }
        catch (DecoderFallbackException)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            return Encoding.GetEncoding("GB18030").GetString(bytes);
        }
    }

    private static List<List<string>> ParseCsvRows(string content)
    {
        var rows = new List<List<string>>();
        var cell = new StringBuilder();
        var row = new List<string>();
        var inQuotes = false;

        var normalizedContent = content.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
        for (var index = 0; index < normalizedContent.Length; index++)
        {
            var current = normalizedContent[index];
            var next = index + 1 < normalizedContent.Length ? normalizedContent[index + 1] : '\0';

            if (current == '"')
            {
                if (inQuotes && next == '"')
                {
                    cell.Append('"');
                    index++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
                continue;
            }

            if (current == ',' && !inQuotes)
            {
                row.Add(cell.ToString().Trim());
                cell.Clear();
                continue;
            }

            if (current == '\n' && !inQuotes)
            {
                row.Add(cell.ToString().Trim());
                if (row.Any(u => !string.IsNullOrWhiteSpace(u))) rows.Add(row);
                row = [];
                cell.Clear();
                continue;
            }

            cell.Append(current);
        }

        row.Add(cell.ToString().Trim());
        if (row.Any(u => !string.IsNullOrWhiteSpace(u))) rows.Add(row);
        if (inQuotes) throw Oops.Oh("CSV 文件存在未闭合的引号");
        return rows;
    }

    private static string NormalizeProjectImportHeader(string header)
    {
        var normalized = NormalizeKey(header.TrimStart('\uFEFF').Trim().TrimStart('*'));
        if (ProjectImportKnownHeaders.Any(u => NormalizeKey(u).Equals(normalized, StringComparison.OrdinalIgnoreCase)))
        {
            return ProjectImportKnownHeaders.First(u => NormalizeKey(u).Equals(normalized, StringComparison.OrdinalIgnoreCase));
        }

        return ProjectImportHeaderAliases.TryGetValue(normalized, out var alias) ? alias : header.Trim();
    }

    private static string? NormalizeProjectImportCellValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        var normalized = value
            .Replace("_x000D_", "\r", StringComparison.OrdinalIgnoreCase)
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n');
        normalized = new string(normalized
            .Select(character => char.IsControl(character) && character != '\n' ? ' ' : character)
            .ToArray());
        var lines = normalized
            .Split('\n')
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();

        return lines.Count == 0 ? null : string.Join("\n", lines);
    }

    private static string NormalizeKnowledgeImportHeader(string header)
    {
        var normalized = NormalizeKey(header.TrimStart('\uFEFF').Trim().TrimStart('*'));
        var allHeaders = KnowledgeImportTemplateHeaders.Concat(KnowledgeImportOptionalHeaders).ToList();
        if (allHeaders.Any(u => NormalizeKey(u).Equals(normalized, StringComparison.OrdinalIgnoreCase)))
        {
            return allHeaders.First(u => NormalizeKey(u).Equals(normalized, StringComparison.OrdinalIgnoreCase));
        }

        return KnowledgeImportHeaderAliases.TryGetValue(normalized, out var alias) ? alias : header.Trim();
    }

    private static string BuildProjectImportBlock(ProjectImportRow row)
    {
        return string.Join("\n", ProjectImportBlockHeaders
            .Select(header =>
            {
                var value = row.Get(header);
                if (string.IsNullOrWhiteSpace(value)) return null;
                var targetKey = header == "参与者类别" ? "受试者类型" : header == "各参加单位信息" ? "研究中心" : header;
                return $"{targetKey}：{value}";
            })
            .Where(u => !string.IsNullOrWhiteSpace(u)));
    }

    private static string BuildKnowledgeImportBlock(KnowledgeImportRow row)
    {
        return string.Join("\n", KnowledgeImportTemplateHeaders
            .Concat(KnowledgeImportOptionalHeaders)
            .Select(header =>
            {
                var value = row.Get(header);
                if (string.IsNullOrWhiteSpace(value)) return null;
                return header switch
                {
                    "分类名称" => $"分类：{value}",
                    "标签（逗号分隔）" => $"标签：{value}",
                    "封面图URL" => $"封面图：{value}",
                    "正文内容" => $"正文：{value}",
                    "发布状态" => $"状态：{value}",
                    _ => $"{header}：{value}",
                };
            })
            .Where(u => !string.IsNullOrWhiteSpace(u)));
    }

    private static string BuildProjectFileSourceText(string fileName, List<ProjectImportRow> rows)
    {
        var builder = new StringBuilder();
        foreach (var row in rows)
        {
            var block = BuildProjectImportBlock(row);
            if (string.IsNullOrWhiteSpace(block)) continue;

            if (!AppendLimitedSourceBlock(builder, $"文件：{fileName}\n行号：{row.RowNumber}\n{block}", MaxSourceTextLength))
                break;
        }

        return builder.Length > 0 ? builder.ToString() : $"文件：{fileName}";
    }

    private static string BuildKnowledgeFileSourceText(string fileName, List<KnowledgeImportRow> rows)
    {
        var builder = new StringBuilder();
        foreach (var row in rows)
        {
            var block = BuildKnowledgeImportBlock(row);
            if (string.IsNullOrWhiteSpace(block)) continue;

            if (!AppendLimitedSourceBlock(builder, $"文件：{fileName}\n行号：{row.RowNumber}\n{block}", MaxSourceTextLength))
                break;
        }

        return builder.Length > 0 ? builder.ToString() : $"文件：{fileName}";
    }

    private static bool AppendLimitedSourceBlock(StringBuilder builder, string block, int maxLength)
    {
        const string separator = "\n\n---\n\n";
        const string truncationMarker = "\n\n...（内容过长，已截断）";
        var prefix = builder.Length == 0 ? string.Empty : separator;
        var requiredLength = prefix.Length + block.Length;
        if (builder.Length + requiredLength <= maxLength)
        {
            builder.Append(prefix);
            builder.Append(block);
            return true;
        }

        var availableLength = maxLength - builder.Length - prefix.Length - truncationMarker.Length;
        if (availableLength > 0)
        {
            builder.Append(prefix);
            builder.Append(block[..Math.Min(block.Length, availableLength)]);
        }

        if (builder.Length + truncationMarker.Length <= maxLength)
            builder.Append(truncationMarker);

        return false;
    }

    private static SaveLifeClinicalTrial BuildProjectFromImportRow(ProjectImportRow row)
    {
        var title = row.Get("试验题目");
        var indication = Limit(row.Get("适应症"), 512);
        var diseaseResolution = SaveLifeDiseaseAreaResolver.Resolve(
            row.Get("疾病领域"),
            title,
            indication);

        return new SaveLifeClinicalTrial
        {
            Title = Limit(title, 256) ?? string.Empty,
            ProtocolNumber = Limit(row.Get("方案编号"), 64),
            CdeRegistrationNumber = Limit(row.Get("CDE登记号"), 64) ?? string.Empty,
            Description = row.Get("项目描述"),
            Sponsor = Limit(row.Get("申办方"), 128),
            ClinicalPhase = Limit(row.Get("试验阶段"), 32),
            DiseaseArea = Limit(diseaseResolution.DiseaseArea, 64),
            Indication = indication,
            Status = NormalizeProjectStatus(row.Get("状态")),
            TargetCount = ParseInt(row.Get("目标例数") ?? row.Get("目标人数")),
            EnrolledCount = ParseInt(row.Get("已招募人数")),
            StartDate = ParseDate(row.Get("开始日期")),
            EndDate = ParseDate(row.Get("结束日期")),
            ParticipantType = Limit(row.Get("参与者类别") ?? row.Get("参与者类型"), 64),
            InclusionCriteria = row.Get("入选标准"),
            ExclusionCriteria = row.Get("排除标准"),
            TrialDuration = Limit(row.Get("试验周期"), 128),
            Benefits = row.Get("受试者权益") ?? row.Get("参与者权益"),
            EthicsCommitteeInfo = row.Get("伦理委员会信息") ?? row.Get("伦理委员会"),
            Sites = row.Get("各参加单位信息"),
            Location = Limit(row.Get("地点") ?? row.Get("城市"), 128),
            Requirements = row.Get("招募要求"),
        };
    }

    private static SaveLifeClinicalTrial BuildProject(ParsedTextBlock block)
    {
        var title = GetField(block, "标题", "试验题目", "项目标题", "项目名称", "研究题目")
            ?? block.FirstFreeLine;

        var description = GetField(block, "描述", "项目描述", "简介", "摘要")
            ?? JoinFreeLines(block, title);
        var indication = Limit(GetField(block, "适应症"), 512);
        var diseaseResolution = SaveLifeDiseaseAreaResolver.Resolve(
            GetField(block, "疾病领域", "领域", "疾病类型"),
            title,
            indication);

        return new SaveLifeClinicalTrial
        {
            Title = Limit(title, 256) ?? string.Empty,
            ProtocolNumber = Limit(GetField(block, "方案编号", "方案号"), 64),
            CdeRegistrationNumber = Limit(GetField(block, "CDE登记号", "登记号", "注册号", "临床试验登记号"), 64) ?? string.Empty,
            Description = description,
            Sponsor = Limit(GetField(block, "申办方", "申办单位", "主办方"), 128),
            ClinicalPhase = Limit(GetField(block, "阶段", "临床阶段", "试验阶段"), 32),
            DiseaseArea = Limit(diseaseResolution.DiseaseArea, 64),
            Indication = indication,
            Status = NormalizeProjectStatus(GetField(block, "状态", "项目状态", "招募状态")),
            TargetCount = ParseInt(GetField(block, "目标例数", "目标人数", "目标招募人数", "计划人数", "招募人数")),
            EnrolledCount = ParseInt(GetField(block, "已招募人数", "入组人数", "已入组人数")),
            StartDate = ParseDate(GetField(block, "开始日期", "开始时间")),
            EndDate = ParseDate(GetField(block, "结束日期", "结束时间")),
            ParticipantType = Limit(GetField(block, "受试者类型", "参与者类别", "参与者类型", "人群"), 64),
            InclusionCriteria = GetField(block, "入选标准", "纳入标准"),
            ExclusionCriteria = GetField(block, "排除标准"),
            TrialDuration = Limit(GetField(block, "试验周期", "周期"), 128),
            Benefits = GetField(block, "受试者权益", "参与者权益", "权益", "补偿"),
            EthicsCommitteeInfo = GetField(block, "伦理委员会", "伦理委员会信息"),
            Sites = GetField(block, "研究中心", "各参加单位信息", "中心", "医院"),
            Location = Limit(GetField(block, "地点", "地区", "城市"), 128),
            Requirements = GetField(block, "招募要求", "要求", "条件"),
        };
    }

    private static string BuildClinicalTrialSearchPath(AutoImportSaveLifeProjectInput input)
    {
        var page = Math.Clamp(input.Page, 1, 10);
        var pageSize = Math.Clamp(input.PageSize, 1, 50);
        var status = string.IsNullOrWhiteSpace(input.Status) ? "进行中" : input.Status.Trim();
        var keyword = Limit(input.Keyword, 64) ?? string.Empty;
        var query = new Dictionary<string, string?>
        {
            ["keywords"] = keyword,
            ["state"] = status,
            ["rule"] = "CTR",
            ["sort"] = "desc",
            ["currentpage"] = page.ToString(),
            ["pagesize"] = pageSize.ToString(),
        };

        return "clinicaltrials.searchlist.dhtml?" + string.Join("&",
            query.Select(u => $"{WebUtility.UrlEncode(u.Key)}={WebUtility.UrlEncode(u.Value)}"));
    }

    private static bool LooksLikeShieldPage(string html)
    {
        if (string.IsNullOrWhiteSpace(html)) return true;
        return html.Contains("9DhefwqGPrzGxEp9hPaoag", StringComparison.Ordinal)
            || html.Contains("请开启JavaScript", StringComparison.OrdinalIgnoreCase)
            || html.Contains("正在加载", StringComparison.Ordinal)
            || (!html.Contains("<table", StringComparison.OrdinalIgnoreCase)
                && !html.Contains("CTR", StringComparison.OrdinalIgnoreCase));
    }

    private static List<SaveLifeClinicalTrial> ParseClinicalTrialSearchHtml(string html)
    {
        var rows = Regex.Matches(html, @"<tr\b[^>]*>(?<row>.*?)</tr>", RegexOptions.IgnoreCase | RegexOptions.Singleline)
            .Select(u => u.Groups["row"].Value)
            .ToList();
        var result = new List<SaveLifeClinicalTrial>();

        foreach (var row in rows)
        {
            if (!Regex.IsMatch(row, @"<td\b", RegexOptions.IgnoreCase)) continue;

            var cells = Regex.Matches(row, @"<t[dh]\b[^>]*>(?<cell>.*?)</t[dh]>", RegexOptions.IgnoreCase | RegexOptions.Singleline)
                .Select(u => CleanHtmlText(u.Groups["cell"].Value))
                .Where(u => !string.IsNullOrWhiteSpace(u))
                .ToList();

            if (cells.Count == 0) continue;

            var text = string.Join("\n", cells);
            var cdeNumber = Regex.Match(text, @"CTR\d{8,}", RegexOptions.IgnoreCase).Value;

            var title = cells.FirstOrDefault(u => !u.Contains("CTR", StringComparison.OrdinalIgnoreCase)
                && !u.Contains("进行中", StringComparison.Ordinal)
                && !u.Contains("已完成", StringComparison.Ordinal)
                && !u.Contains("主动暂停", StringComparison.Ordinal)
                && u.Length > 6);

            result.Add(new SaveLifeClinicalTrial
            {
                Title = Limit(title, 256) ?? Limit(text.Split('\n').FirstOrDefault(), 256) ?? string.Empty,
                CdeRegistrationNumber = Limit(cdeNumber, 64) ?? string.Empty,
                Sponsor = Limit(FindLikelySponsor(cells), 128),
                Status = NormalizeProjectStatus(text),
                Description = text,
                SortOrder = 100,
            });
        }

        return result
            .Where(u => !string.IsNullOrWhiteSpace(u.Title))
            .GroupBy(u => string.IsNullOrWhiteSpace(u.CdeRegistrationNumber)
                ? $"missing:{u.Title}"
                : u.CdeRegistrationNumber,
                StringComparer.OrdinalIgnoreCase)
            .Select(u => u.First())
            .ToList();
    }

    private static string? FindLikelySponsor(List<string> cells)
    {
        return cells.FirstOrDefault(u => u.Contains("公司", StringComparison.Ordinal)
            || u.Contains("医院", StringComparison.Ordinal)
            || u.Contains("研究院", StringComparison.Ordinal)
            || u.Contains("药业", StringComparison.Ordinal));
    }

    private static string CleanHtmlText(string html)
    {
        var withoutTags = Regex.Replace(html, "<[^>]+>", " ", RegexOptions.Singleline);
        var decoded = WebUtility.HtmlDecode(withoutTags);
        return Regex.Replace(decoded, @"\s+", " ").Trim();
    }

    private static SaveLifeDiseaseKnowledge BuildKnowledge(
        ParsedTextBlock block,
        List<SaveLifeDiseaseCategory> categories)
    {
        var title = GetField(block, "标题", "文章标题", "知识标题") ?? block.FirstFreeLine;
        var content = GetField(block, "正文", "内容", "文章内容") ?? JoinFreeLines(block, title);
        var categoryName = GetField(block, "分类", "栏目");
        var categoryId = string.IsNullOrWhiteSpace(categoryName)
            ? null
            : categories.FirstOrDefault(u => u.Name == categoryName.Trim())?.Id;

        return new SaveLifeDiseaseKnowledge
        {
            Title = Limit(title, 256) ?? string.Empty,
            CategoryId = categoryId,
            DiseaseName = Limit(GetField(block, "疾病", "疾病名称", "病种"), 128),
            Author = Limit(GetField(block, "作者", "撰写人"), 64),
            Summary = Limit(GetField(block, "摘要", "简介"), 512),
            Content = SaveLifeKnowledgeContentSanitizer.Normalize(content),
            ImageUrl = Limit(GetField(block, "封面图", "封面", "图片"), 512),
            Tags = Limit(GetField(block, "标签", "关键词"), 512),
            ViewCount = ParseInt(GetField(block, "浏览量", "浏览")) ?? 0,
            LikeCount = ParseInt(GetField(block, "点赞", "点赞数")) ?? 0,
            IsPublished = ParsePublished(GetField(block, "状态", "发布", "发布状态")),
            SortOrder = ParseInt(GetField(block, "排序", "排序值")) ?? 100,
        };
    }

    private async Task<long?> ResolveKnowledgeCategoryId(string categoryName, List<SaveLifeDiseaseCategory> categories)
    {
        var normalizedName = Limit(categoryName, 64);
        if (string.IsNullOrWhiteSpace(normalizedName)) return null;

        var existing = categories.FirstOrDefault(u => string.Equals(u.Name, normalizedName, StringComparison.OrdinalIgnoreCase));
        if (existing != null) return existing.Id;

        var category = new SaveLifeDiseaseCategory
        {
            Name = normalizedName,
            Description = $"{normalizedName} 分类由疾病知识导入自动创建",
            Icon = "📋",
            SortOrder = 100,
            Status = StatusEnum.Enable,
        };

        await _categoryRep.InsertAsync(category);
        categories.Add(category);
        return category.Id;
    }

    private static string NormalizeSourceText(string? sourceText)
    {
        var normalized = (sourceText ?? string.Empty)
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Trim();

        if (string.IsNullOrWhiteSpace(normalized)) throw Oops.Oh("导入内容不能为空");
        if (normalized.Length > MaxSourceTextLength) throw Oops.Oh($"导入内容不能超过 {MaxSourceTextLength} 个字符");
        return normalized;
    }

    private static List<string> SplitImportBlocks(string sourceText)
    {
        var blocks = Regex.Split(sourceText, @"(?:\n\s*){2,}|^\s*[-=]{3,}\s*$", RegexOptions.Multiline)
            .Select(u => u.Trim())
            .Where(u => !string.IsNullOrWhiteSpace(u))
            .ToList();

        if (blocks.Count == 0) throw Oops.Oh("导入内容不能为空");
        if (blocks.Count > MaxBlockCount) throw Oops.Oh($"一次最多导入 {MaxBlockCount} 条记录");
        return blocks;
    }

    private static ParsedTextBlock ParseTextBlock(string text)
    {
        var block = new ParsedTextBlock();
        string? currentKey = null;

        foreach (var rawLine in text.Split('\n'))
        {
            var line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;

            if (TrySplitKeyValue(line, out var key, out var value))
            {
                currentKey = key;
                AppendField(block.Fields, key, value);
                continue;
            }

            if (currentKey != null && IsMultilineKey(currentKey))
            {
                AppendField(block.Fields, currentKey, line);
                continue;
            }

            block.FreeLines.Add(line);
        }

        return block;
    }

    private static bool TrySplitKeyValue(string line, out string key, out string value)
    {
        var match = Regex.Match(line, @"^\s*(?<key>[\p{L}\p{N}_（）()\/\-\s]{1,30})\s*[:：]\s*(?<value>.*)$");
        key = NormalizeKey(match.Groups["key"].Value);
        value = match.Groups["value"].Value.Trim();
        return match.Success && !string.IsNullOrWhiteSpace(key);
    }

    private static void AppendField(Dictionary<string, string> fields, string key, string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;
        fields[key] = fields.TryGetValue(key, out var existing)
            ? $"{existing}\n{value.Trim()}"
            : value.Trim();
    }

    private static string? GetField(ParsedTextBlock block, params string[] aliases)
    {
        foreach (var alias in aliases)
        {
            if (block.Fields.TryGetValue(NormalizeKey(alias), out var value) && !string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }

        return null;
    }

    private static string NormalizeKey(string key)
    {
        return key.Trim()
            .Replace(" ", string.Empty, StringComparison.Ordinal)
            .Replace("　", string.Empty, StringComparison.Ordinal);
    }

    private static bool IsMultilineKey(string key)
    {
        return key.Contains("描述", StringComparison.Ordinal)
            || key.Contains("简介", StringComparison.Ordinal)
            || key.Contains("摘要", StringComparison.Ordinal)
            || key.Contains("正文", StringComparison.Ordinal)
            || key.Contains("内容", StringComparison.Ordinal)
            || key.Contains("标准", StringComparison.Ordinal)
            || key.Contains("要求", StringComparison.Ordinal)
            || key.Contains("权益", StringComparison.Ordinal)
            || key.Contains("委员会", StringComparison.Ordinal)
            || key.Contains("中心", StringComparison.Ordinal)
            || key.Contains("医院", StringComparison.Ordinal);
    }

    private static string? JoinFreeLines(ParsedTextBlock block, string? title)
    {
        var lines = block.FreeLines
            .Where(u => !string.Equals(u, title, StringComparison.Ordinal))
            .ToList();
        return lines.Count == 0 ? null : string.Join("\n", lines);
    }

    private static string? Limit(string? value, int maxLength)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrWhiteSpace(trimmed)) return null;
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }

    private static int? ParseInt(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        var match = Regex.Match(value, @"\d+");
        if (!match.Success || !int.TryParse(match.Value, out var number)) return null;
        return Math.Max(0, number);
    }

    private static DateTime? ParseDate(string? value)
    {
        return DateTime.TryParse(value, out var date) ? date : null;
    }

    private static string NormalizeProjectStatus(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "recruiting";

        var status = value.Trim();
        if (status.Contains("暂停", StringComparison.Ordinal)) return "paused";
        if (status.Contains("结束", StringComparison.Ordinal)
            || status.Contains("完成", StringComparison.Ordinal)
            || status.Contains("终止", StringComparison.Ordinal)) return "ended";
        if (status.Contains("草稿", StringComparison.Ordinal)) return "draft";
        if (status.Contains("招募", StringComparison.Ordinal) || status.Contains("进行中", StringComparison.Ordinal)) return "recruiting";
        return Limit(status, 32) ?? "recruiting";
    }

    private static bool ParsePublished(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return true;

        var normalized = value.Trim();
        if (normalized.Contains("下架", StringComparison.Ordinal)
            || normalized.Contains("未发布", StringComparison.Ordinal)
            || normalized.Contains("草稿", StringComparison.Ordinal)
            || normalized.Equals("false", StringComparison.OrdinalIgnoreCase)
            || normalized == "0"
            || normalized == "否")
        {
            return false;
        }

        return true;
    }

    private static string BuildResultMessage(int totalCount, int successCount, int failedCount, List<string> failures)
    {
        var message = $"导入完成：共 {totalCount} 条，成功 {successCount} 条，失败 {failedCount} 条。";
        return failures.Count == 0 ? message : $"{message}\n{string.Join("\n", failures)}";
    }

    private static string BuildProjectActionResultMessage(
        int totalCount,
        int insertedCount,
        int overwrittenCount,
        int skippedCount,
        int failedCount,
        List<string> failures)
    {
        var message =
            $"导入完成：共 {totalCount} 条，新增 {insertedCount} 条，覆盖 {overwrittenCount} 条，跳过 {skippedCount} 条，失败 {failedCount} 条。";
        return failures.Count == 0 ? message : $"{message}\n{string.Join("\n", failures)}";
    }

    private sealed class ParsedTextBlock
    {
        public Dictionary<string, string> Fields { get; } = new(StringComparer.OrdinalIgnoreCase);

        public List<string> FreeLines { get; } = new();

        public string? FirstFreeLine => FreeLines.FirstOrDefault();
    }

    private sealed class ProjectImportCandidate
    {
        public ProjectImportCandidate(ProjectImportRow row, SaveLifeClinicalTrial entity)
        {
            Row = row;
            Entity = entity;
        }

        public ProjectImportRow Row { get; }

        public SaveLifeClinicalTrial Entity { get; }
    }

    private sealed class ProjectNumberIndex
    {
        public Dictionary<string, HashSet<long>> ProjectIdsByNumberKey { get; } =
            new(StringComparer.OrdinalIgnoreCase);

        public Dictionary<long, HashSet<string>> NumberKeysByProjectId { get; } = [];
    }

    private sealed class ProjectImportRow
    {
        private readonly Dictionary<string, string> _values = new(StringComparer.OrdinalIgnoreCase);

        public int RowNumber { get; private init; }

        public bool IsEmpty => !_values.Values.Any(u => !string.IsNullOrWhiteSpace(u));

        public string? Get(string header)
        {
            return _values.TryGetValue(header, out var value) && !string.IsNullOrWhiteSpace(value)
                ? value.Trim()
                : null;
        }

        public static ProjectImportRow FromImportInput(int rowNumber, ImportSaveLifeProjectRowInput input)
        {
            var row = new ProjectImportRow { RowNumber = rowNumber };
            row.Set("试验题目", input.Title);
            row.Set("方案编号", input.ProtocolNumber);
            row.Set("CDE登记号", input.CdeRegistrationNumber);
            row.Set("项目描述", input.Description);
            row.Set("药物名称", input.DrugName);
            row.Set("申办方", input.Sponsor);
            row.Set("试验阶段", input.ClinicalPhase);
            row.Set("疾病领域", input.DiseaseArea);
            row.Set("状态", input.Status);
            row.Set("目标例数", input.TargetCaseCount);
            row.Set("目标人数", input.TargetCount);
            row.Set("已招募人数", input.EnrolledCount);
            row.Set("开始日期", input.StartDate);
            row.Set("结束日期", input.EndDate);
            row.Set("参与者类型", input.ParticipantTypeText);
            row.Set("参与者类别", input.ParticipantType);
            row.Set("适应症", input.Indication);
            row.Set("入选标准", input.InclusionCriteria);
            row.Set("排除标准", input.ExclusionCriteria);
            row.Set("试验周期", input.TrialDuration);
            row.Set("参与者权益", input.ParticipantBenefits);
            row.Set("受试者权益", input.Benefits);
            row.Set("伦理委员会", input.EthicsCommittee);
            row.Set("伦理委员会信息", input.EthicsCommitteeInfo);
            row.Set("各参加单位信息", input.Sites);
            row.Set("机构名称", input.InstitutionNames);
            row.Set("主要研究者", input.PrincipalInvestigators);
            row.Set("省", input.Provinces);
            row.Set("城市", input.Cities);
            row.Set("国家/地区", input.Countries);
            row.ApplyV17DerivedFields();
            return row;
        }

        public static ProjectImportRow FromCells(int rowNumber, List<string> headers, List<string> cells)
        {
            var row = new ProjectImportRow { RowNumber = rowNumber };
            for (var index = 0; index < headers.Count; index++)
            {
                if (index >= cells.Count) break;
                var header = headers[index];
                if (string.IsNullOrWhiteSpace(header)) continue;
                row.Set(header, cells[index]);
            }
            row.ApplyV17DerivedFields();
            return row;
        }

        private void Set(string header, string? value)
        {
            var normalizedHeader = NormalizeProjectImportHeader(header);
            var normalizedValue = NormalizeProjectImportCellValue(value);
            if (normalizedValue == null) return;
            _values[normalizedHeader] = normalizedValue;
        }

        private void SetIfEmpty(string header, string? value)
        {
            if (Get(header) != null) return;
            Set(header, value);
        }

        private void ApplyV17DerivedFields()
        {
            SetIfEmpty("项目描述", BuildProjectDescription(Get("药物名称"), Get("适应症")));

            var rawParticipantType = FirstNotEmpty(Get("参与者类型"), Get("参与者类别"));
            SetIfEmpty("招募要求", BuildProjectRequirements(rawParticipantType, Get("适应症")));
            var normalizedParticipantType = NormalizeParticipantType(rawParticipantType);
            if (!string.IsNullOrWhiteSpace(normalizedParticipantType))
            {
                Set("参与者类别", normalizedParticipantType);
            }

            SetIfEmpty("各参加单位信息", BuildProjectSitesJson(
                Get("机构名称"),
                Get("主要研究者"),
                Get("省"),
                Get("城市"),
                Get("国家/地区")));
            SetIfEmpty("地点", BuildProjectLocation(Get("省"), Get("城市"), Get("国家/地区")));
        }

        private static string? FirstNotEmpty(params string?[] values)
        {
            return values.FirstOrDefault(u => !string.IsNullOrWhiteSpace(u));
        }

        private static string? BuildProjectDescription(string? drugName, string? indication)
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(drugName)) parts.Add($"药物名称：{drugName.Trim()}");
            if (!string.IsNullOrWhiteSpace(indication)) parts.Add($"适应症：{indication.Trim()}");
            return parts.Count == 0 ? null : string.Join("；", parts);
        }

        private static string? BuildProjectRequirements(string? participantType, string? indication)
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(participantType)) parts.Add($"参与者类型：{participantType.Trim()}");
            if (!string.IsNullOrWhiteSpace(indication)) parts.Add($"适应症：{indication.Trim()}");
            return parts.Count == 0 ? null : string.Join("；", parts);
        }

        private static string? NormalizeParticipantType(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;

            var normalized = value.Trim();
            if (normalized.Contains("健康", StringComparison.Ordinal)) return "健康志愿者";
            if (normalized.Contains("志愿者", StringComparison.Ordinal)) return "健康志愿者";
            return "患者";
        }

        private static string? BuildProjectSitesJson(
            string? institutionNames,
            string? principalInvestigators,
            string? provinces,
            string? cities,
            string? countries)
        {
            var institutions = SplitMultiValue(institutionNames);
            if (institutions.Count == 0) return null;

            var investigators = SplitMultiValue(principalInvestigators);
            var provinceList = SplitMultiValue(provinces);
            var cityList = SplitMultiValue(cities);
            var countryList = SplitMultiValue(countries);
            var sites = new List<Dictionary<string, object?>>();

            for (var index = 0; index < institutions.Count; index++)
            {
                var institution = institutions[index];
                if (string.IsNullOrWhiteSpace(institution)) continue;

                sites.Add(new Dictionary<string, object?>
                {
                    ["index"] = index + 1,
                    ["institution_name"] = institution,
                    ["principal_investigator"] = GetIndexedValue(investigators, index),
                    ["country"] = GetIndexedValue(countryList, index) ?? "中国",
                    ["province"] = GetIndexedValue(provinceList, index),
                    ["city"] = GetIndexedValue(cityList, index),
                });
            }

            return sites.Count == 0 ? null : JsonSerializer.Serialize(sites, ProjectImportJsonOptions);
        }

        private static string? BuildProjectLocation(string? provinces, string? cities, string? countries)
        {
            var locations = SplitMultiValue(cities);
            if (locations.Count == 0) locations = SplitMultiValue(provinces);
            if (locations.Count == 0) locations = SplitMultiValue(countries);

            var distinctLocations = locations
                .Where(u => !string.IsNullOrWhiteSpace(u))
                .Distinct()
                .ToList();
            if (distinctLocations.Count == 0) return null;
            if (distinctLocations.Count <= 3) return string.Join("、", distinctLocations);
            return $"{string.Join("、", distinctLocations.Take(3))}等";
        }

        private static List<string> SplitMultiValue(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return [];

            return value
                .Replace("\r\n", "\n", StringComparison.Ordinal)
                .Replace('\r', '\n')
                .Split([';', '；', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(u => !string.IsNullOrWhiteSpace(u))
                .ToList();
        }

        private static string? GetIndexedValue(List<string> values, int index)
        {
            return index < values.Count && !string.IsNullOrWhiteSpace(values[index])
                ? values[index]
                : null;
        }
    }

    private sealed class KnowledgeImportRow
    {
        private readonly Dictionary<string, string> _values = new(StringComparer.OrdinalIgnoreCase);

        public int RowNumber { get; private init; }

        public bool IsEmpty => !_values.Values.Any(u => !string.IsNullOrWhiteSpace(u));

        public string? Get(string header)
        {
            return _values.TryGetValue(header, out var value) && !string.IsNullOrWhiteSpace(value)
                ? value.Trim()
                : null;
        }

        public static KnowledgeImportRow FromImportInput(int rowNumber, ImportSaveLifeKnowledgeRowInput input)
        {
            var row = new KnowledgeImportRow { RowNumber = rowNumber };
            row.Set("标题", input.Title);
            row.Set("分类名称", input.CategoryName);
            row.Set("疾病名称", input.DiseaseName);
            row.Set("作者", input.Author);
            row.Set("标签（逗号分隔）", input.Tags);
            row.Set("封面图URL", input.ImageUrl);
            row.Set("摘要", input.Summary);
            row.Set("正文内容", input.Content);
            return row;
        }

        public static KnowledgeImportRow FromCells(int rowNumber, List<string> headers, List<string> cells)
        {
            var row = new KnowledgeImportRow { RowNumber = rowNumber };
            for (var index = 0; index < headers.Count; index++)
            {
                if (index >= cells.Count) break;
                var header = headers[index];
                if (string.IsNullOrWhiteSpace(header)) continue;
                row.Set(header, cells[index]);
            }
            return row;
        }

        private void Set(string header, string? value)
        {
            var normalizedHeader = NormalizeKnowledgeImportHeader(header);
            if (string.IsNullOrWhiteSpace(value)) return;
            _values[normalizedHeader] = value.Trim();
        }
    }
}
