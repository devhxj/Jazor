namespace Jero.Plugin.SaveLife.Service;

/// <summary>
/// 临床试验项目服务
/// </summary>
[ApiDescriptionSettings(SaveLifeConst.GroupName, Order = 100)]
public class SaveLifeClinicalTrialService : IDynamicApiController, ITransient
{
    private const int ProjectListDefaultPageSize = 100;
    private const int ProjectListMaxPageSize = 200;

    private readonly SqlSugarRepository<SaveLifeClinicalTrial> _trialRep;
    private readonly SqlSugarRepository<SaveLifeDiseaseKnowledge> _knowledgeRep;
    private readonly SqlSugarRepository<SaveLifeDiseaseCategory> _categoryRep;
    private readonly SqlSugarRepository<SaveLifeAnnouncement> _announcementRep;
    private readonly ISaveLifePublicKnowledgeBaseIndexProcessor _knowledgeBaseIndexProcessor;
    private readonly ISaveLifeAgentRetrievalService _agentRetrievalService;

    public SaveLifeClinicalTrialService(
        SqlSugarRepository<SaveLifeClinicalTrial> trialRep,
        SqlSugarRepository<SaveLifeDiseaseKnowledge> knowledgeRep,
        SqlSugarRepository<SaveLifeDiseaseCategory> categoryRep,
        SqlSugarRepository<SaveLifeAnnouncement> announcementRep,
        ISaveLifePublicKnowledgeBaseIndexProcessor knowledgeBaseIndexProcessor,
        ISaveLifeAgentRetrievalService agentRetrievalService)
    {
        _trialRep = trialRep;
        _knowledgeRep = knowledgeRep;
        _categoryRep = categoryRep;
        _announcementRep = announcementRep;
        _knowledgeBaseIndexProcessor = knowledgeBaseIndexProcessor;
        _agentRetrievalService = agentRetrievalService;
    }

    /// <summary>
    /// 分页查询临床试验项目
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<SaveLifeClinicalTrial>> Page(SaveLifeClinicalTrialInput input)
    {
        var keyword = input.Keyword?.Trim();
        var cdeKeyword = SaveLifeCdeRegistrationNumber.Normalize(keyword);
        var hasCdeKeyword = !string.IsNullOrWhiteSpace(cdeKeyword);
        return await _trialRep.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(keyword), u =>
                (u.Title != null && u.Title.Contains(keyword!)) ||
                (u.Sponsor != null && u.Sponsor.Contains(keyword!)) ||
                (hasCdeKeyword && u.CdeRegistrationNumber.Contains(cdeKeyword)) ||
                (u.Location != null && u.Location.Contains(keyword!)))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Status), u => u.Status == input.Status)
            .WhereIF(!string.IsNullOrWhiteSpace(input.DiseaseArea), u => u.DiseaseArea == input.DiseaseArea)
            .WhereIF(!string.IsNullOrWhiteSpace(input.ClinicalPhase), u => u.ClinicalPhase == input.ClinicalPhase)
            .WhereIF(!string.IsNullOrWhiteSpace(input.Sponsor), u => u.Sponsor != null && u.Sponsor.Contains(input.Sponsor!.Trim()))
            .OrderByPinnedFirst()
            .ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 小程序试验筛选选项
    /// </summary>
    [AllowAnonymous]
    [HttpGet("/api/v1/save-life/trials/filter-options"), NonUnify]
    [ApiDescriptionSettings(Name = "MiniAppFilterOptions")]
    public async Task<SaveLifeMiniAppTrialFilterOptionsOutput> MiniAppFilterOptions()
    {
        var diseaseAreas = await _trialRep.AsQueryable()
            .Where(u => u.Status == "recruiting" && u.DiseaseArea != null)
            .Select(u => u.DiseaseArea!)
            .ToListAsync();
        var clinicalPhases = await _trialRep.AsQueryable()
            .Where(u => u.Status == "recruiting" && u.ClinicalPhase != null)
            .Select(u => u.ClinicalPhase!)
            .ToListAsync();
        var statuses = await _trialRep.AsQueryable()
            .Where(u => u.Status != null)
            .Select(u => u.Status!)
            .ToListAsync();

        return new SaveLifeMiniAppTrialFilterOptionsOutput
        {
            DiseaseAreas = diseaseAreas
                .Where(u => !string.IsNullOrWhiteSpace(u))
                .Distinct()
                .OrderBy(u => u)
                .Select(u => new SaveLifeMiniAppTrialFilterOptionItem { Value = u, Label = u })
                .ToList(),
            ClinicalPhases = clinicalPhases
                .Where(u => !string.IsNullOrWhiteSpace(u))
                .Distinct()
                .OrderBy(u => u)
                .Select(u => new SaveLifeMiniAppTrialFilterOptionItem { Value = u, Label = u })
                .ToList(),
            Statuses = statuses
                .Where(u => !string.IsNullOrWhiteSpace(u))
                .Distinct()
                .OrderBy(u => u)
                .Select(u => new SaveLifeMiniAppTrialFilterOptionItem
                {
                    Value = u,
                    Label = u switch
                    {
                        "recruiting" => "招募中",
                        "ongoing" => "进行中",
                        "completed" => "已完成",
                        "suspended" => "已暂停",
                        "upcoming" => "即将开始",
                        _ => u
                    }
                })
                .ToList()
        };
    }

    /// <summary>
    /// 小程序首页推荐数据
    /// </summary>
    [AllowAnonymous]
    [HttpGet("/api/v1/save-life/home"), NonUnify]
    [ApiDescriptionSettings(Name = "MiniAppHome")]
    public async Task<SaveLifeMiniAppHomeOutput> MiniAppHome()
    {
        var recommendedTrials = await _trialRep.AsQueryable()
            .Where(u => u.Status == "recruiting")
            .OrderByPinnedFirst()
            .Take(4)
            .ToListAsync();

        var recommendedKnowledge = await _knowledgeRep.AsQueryable()
            .Where(u => u.IsPublished)
            .OrderBy(u => u.SortOrder)
            .OrderBy(u => u.CreateTime, OrderByType.Desc)
            .Take(4)
            .ToListAsync();

        var announcements = await _announcementRep.AsQueryable()
            .Where(u => u.IsPublished)
            .OrderBy(u => u.SortOrder)
            .OrderBy(u => u.CreateTime, OrderByType.Desc)
            .Take(3)
            .ToListAsync();

        return new SaveLifeMiniAppHomeOutput
        {
            RecommendedTrials = recommendedTrials.Select(ToMiniAppTrialOutput).ToList(),
            RecommendedKnowledge = ToMiniAppKnowledgeOutputs(recommendedKnowledge),
            Announcements = announcements.Select(ToMiniAppAnnouncementOutput).ToList(),
            Stats = new SaveLifeMiniAppHomeStatsOutput
            {
                TrialCount = await _trialRep.AsQueryable().Where(u => u.Status == "recruiting").CountAsync(),
                KnowledgeCount = await _knowledgeRep.AsQueryable().Where(u => u.IsPublished).CountAsync(),
                AnnouncementCount = await _announcementRep.AsQueryable().Where(u => u.IsPublished).CountAsync(),
                CategoryCount = await _categoryRep.AsQueryable().Where(u => u.Status == StatusEnum.Enable).CountAsync()
            }
        };
    }

    /// <summary>
    /// 新增临床试验项目
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Add")]
    public async Task<long> Add(AddSaveLifeClinicalTrialInput input)
    {
        input.CdeRegistrationNumber = RequireCdeRegistrationNumber(input.CdeRegistrationNumber);
        await EnsureCdeRegistrationNumberAvailable(input.CdeRegistrationNumber);
        var entity = input.Adapt<SaveLifeClinicalTrial>();
        await _trialRep.InsertAsync(entity);
        await _knowledgeBaseIndexProcessor.QueueTrialAsync(entity.Id);
        return entity.Id;
    }

    /// <summary>
    /// 更新临床试验项目
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Update")]
    public async Task Update(UpdateSaveLifeClinicalTrialInput input)
    {
        input.CdeRegistrationNumber = RequireCdeRegistrationNumber(input.CdeRegistrationNumber);
        await EnsureCdeRegistrationNumberAvailable(input.CdeRegistrationNumber, input.Id);
        var entity = input.Adapt<SaveLifeClinicalTrial>();
        await _trialRep.AsUpdateable(entity)
            .IgnoreColumns(true)
            .IgnoreColumns(u => new { u.IsPinned })
            .ExecuteCommandAsync();

        var latest = await _trialRep.GetByIdAsync(input.Id);
        if (latest != null)
            await _knowledgeBaseIndexProcessor.QueueTrialAsync(latest.Id);
    }

    /// <summary>
    /// 置顶或取消置顶临床试验项目
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Pin")]
    public async Task Pin(PinSaveLifeClinicalTrialInput input)
    {
        var entity = await _trialRep.GetByIdAsync(input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        if (entity.IsPinned == input.IsPinned)
            return;

        entity.IsPinned = input.IsPinned;
        entity.UpdateTime = DateTime.Now;
        await _trialRep.AsUpdateable(entity)
            .UpdateColumns(u => new { u.IsPinned, u.UpdateTime })
            .ExecuteCommandAsync();

        await _knowledgeBaseIndexProcessor.QueueTrialAsync(entity.Id);
    }

    /// <summary>
    /// 全部项目重新入库
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "ReindexAll")]
    public async Task<int> ReindexAll()
    {
        return await _trialRep.AsUpdateable()
            .SetColumns(u => new SaveLifeClinicalTrial
            {
                IsInKnowledgeBase = false,
                KnowledgeBaseError = null,
                KnowledgeBaseLastAttemptTime = null,
                KnowledgeBaseNextRetryTime = null
            })
            .Where(u => !u.IsDelete)
            .ExecuteCommandAsync();
    }

    /// <summary>
    /// 测试项目公开知识库检索
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "TestKnowledgeBase")]
    public async Task<SaveLifeClinicalTrialKnowledgeBaseTestOutput> TestKnowledgeBase(
        TestSaveLifeClinicalTrialKnowledgeBaseInput input,
        CancellationToken cancellationToken)
    {
        var keyword = input.Keyword?.Trim();
        if (string.IsNullOrWhiteSpace(keyword))
            throw Oops.Oh("测试关键词不能为空").StatusCode(422);

        try
        {
            var retrieval = await _agentRetrievalService.BuildRetrievalAsync(
                keyword,
                new SaveLifeAgentPatientSignal
                {
                    SearchTerms = [keyword],
                    TrialSearchTerms = [keyword],
                    UseSourceSearchTerms = true
                },
                cancellationToken: cancellationToken,
                scope: SaveLifeAgentRetrievalScope.ProjectDiscovery);
            var projectIds = retrieval.Trials
                .Select(item => long.TryParse(item.Id, out var id) ? id : 0)
                .Where(id => id > 0)
                .Distinct()
                .ToList();
            var projects = projectIds.Count == 0
                ? new List<SaveLifeClinicalTrial>()
                : await _trialRep.AsQueryable()
                    .Where(item => projectIds.Contains(item.Id))
                    .Select(item => new SaveLifeClinicalTrial
                    {
                        Id = item.Id,
                        CdeRegistrationNumber = item.CdeRegistrationNumber,
                        IsInKnowledgeBase = item.IsInKnowledgeBase
                    })
                    .ToListAsync(cancellationToken);
            var projectsById = projects.ToDictionary(item => item.Id);

            return new SaveLifeClinicalTrialKnowledgeBaseTestOutput
            {
                Keyword = keyword,
                CandidateCount = retrieval.Diagnostics.TrialCandidateCount,
                HitCount = retrieval.Trials.Count,
                RetrievalStatus = retrieval.Diagnostics.TrialStatus,
                TextEmbeddingStatus = retrieval.Diagnostics.TextEmbeddingStatus,
                Projects = retrieval.Trials.Select(item =>
                {
                    var projectId = long.TryParse(item.Id, out var id) ? id : 0;
                    projectsById.TryGetValue(projectId, out var project);
                    return new SaveLifeClinicalTrialKnowledgeBaseTestProjectOutput
                    {
                        Id = projectId,
                        CdeRegistrationNumber = project?.CdeRegistrationNumber,
                        Title = item.Title,
                        DiseaseArea = item.DiseaseArea,
                        ClinicalPhase = item.ClinicalPhase,
                        Status = item.Status,
                        IsInKnowledgeBase = project?.IsInKnowledgeBase ?? false,
                        Score = item.Score,
                        Description = item.Description,
                        Benefits = item.Benefits
                    };
                }).ToList()
            };
        }
        catch (SaveLifeAgentUnavailableException exception)
        {
            var diagnostics = exception.RetrievalDiagnostics;
            return new SaveLifeClinicalTrialKnowledgeBaseTestOutput
            {
                Keyword = keyword,
                CandidateCount = diagnostics?.TrialCandidateCount ?? 0,
                HitCount = diagnostics?.TrialHitCount ?? 0,
                RetrievalStatus = diagnostics?.TrialStatus,
                TextEmbeddingStatus = diagnostics?.TextEmbeddingStatus,
                ErrorSummary = SaveLifeAgentDiagnostics.BuildErrorSummary(exception) ?? exception.Message
            };
        }
    }

    /// <summary>
    /// 删除临床试验项目
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Delete")]
    public async Task Delete(DeleteSaveLifeClinicalTrialInput input)
    {
        var entity = await _trialRep.GetByIdAsync(input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await _knowledgeBaseIndexProcessor.RemoveTrialAsync(entity.Id);
        await _trialRep.FakeDeleteAsync(entity);
    }

    /// <summary>
    /// 获取临床试验项目详情
    /// </summary>
    public async Task<SaveLifeClinicalTrial> GetDetail([FromQuery] QueryByIdSaveLifeClinicalTrialInput input)
    {
        return await _trialRep.GetByIdAsync(input.Id);
    }

    /// <summary>
    /// 获取临床试验项目列表
    /// </summary>
    public async Task<List<SaveLifeClinicalTrialListOutput>> GetList([FromQuery] SaveLifeClinicalTrialInput input)
    {
        var page = NormalizeProjectListPage(input.Page);
        var pageSize = NormalizeProjectListPageSize(input.PageSize);

        return await _trialRep.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(input.Status), u => u.Status == input.Status)
            .WhereIF(!string.IsNullOrWhiteSpace(input.DiseaseArea), u => u.DiseaseArea == input.DiseaseArea)
            .OrderByPinnedFirst()
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new SaveLifeClinicalTrialListOutput
            {
                Id = u.Id,
                Title = u.Title,
                Sponsor = u.Sponsor,
                ClinicalPhase = u.ClinicalPhase,
                DiseaseArea = u.DiseaseArea,
                Indication = u.Indication,
                Status = u.Status,
                IsPinned = u.IsPinned,
                SortOrder = u.SortOrder
            })
            .ToListAsync();
    }

    /// <summary>
    /// 小程序分页查询招募项目
    /// </summary>
    [AllowAnonymous]
    [HttpGet("/api/v1/save-life/trials"), NonUnify]
    [ApiDescriptionSettings(Name = "MiniAppList")]
    public async Task<SqlSugarPagedList<SaveLifeMiniAppClinicalTrialOutput>> MiniAppList([FromQuery] SaveLifeClinicalTrialInput input)
    {
        input.PageSize = NormalizePageSize(input.PageSize);
        var keyword = input.Keyword?.Trim();
        var cdeKeyword = SaveLifeCdeRegistrationNumber.Normalize(keyword);
        var hasCdeKeyword = !string.IsNullOrWhiteSpace(cdeKeyword);

        return await _trialRep.AsQueryable()
            .Where(u => u.Status == "recruiting")
            .WhereIF(!string.IsNullOrWhiteSpace(keyword), u =>
                (u.Title != null && u.Title.Contains(keyword!)) ||
                (u.Sponsor != null && u.Sponsor.Contains(keyword!)) ||
                (hasCdeKeyword && u.CdeRegistrationNumber.Contains(cdeKeyword)) ||
                (u.DiseaseArea != null && u.DiseaseArea.Contains(keyword!)) ||
                (u.Indication != null && u.Indication.Contains(keyword!)) ||
                (u.Location != null && u.Location.Contains(keyword!)) ||
                (u.Requirements != null && u.Requirements.Contains(keyword!)))
            .WhereIF(!string.IsNullOrWhiteSpace(input.DiseaseArea), u => u.DiseaseArea == input.DiseaseArea)
            .WhereIF(!string.IsNullOrWhiteSpace(input.ClinicalPhase), u => u.ClinicalPhase == input.ClinicalPhase)
            .OrderByPinnedFirst()
            .ToPagedListAsync(input.Page, input.PageSize, u => new SaveLifeMiniAppClinicalTrialOutput
            {
                Id = u.Id.ToString(),
                Title = u.Title,
                ProtocolNumber = u.ProtocolNumber,
                CdeRegistrationNumber = u.CdeRegistrationNumber,
                Description = u.Description,
                Sponsor = u.Sponsor,
                ClinicalPhase = u.ClinicalPhase,
                TrialPhase = u.ClinicalPhase,
                DiseaseArea = u.DiseaseArea,
                Indication = u.Indication,
                Status = u.Status,
                TargetCount = u.TargetCount,
                EnrolledCount = u.EnrolledCount,
                StartDate = u.StartDate,
                EndDate = u.EndDate,
                ParticipantType = u.ParticipantType,
                InclusionCriteria = u.InclusionCriteria,
                ExclusionCriteria = u.ExclusionCriteria,
                TrialDuration = u.TrialDuration,
                Benefits = u.Benefits,
                EthicsCommitteeInfo = u.EthicsCommitteeInfo,
                Sites = u.Sites,
                Location = u.Location,
                Requirements = u.Requirements,
                CreateTime = u.CreateTime,
                UpdateTime = u.UpdateTime
            });
    }

    /// <summary>
    /// 小程序获取招募项目详情
    /// </summary>
    [AllowAnonymous]
    [HttpGet("/api/v1/save-life/trials/{id}"), NonUnify]
    [ApiDescriptionSettings(Name = "MiniAppDetail")]
    public async Task<SaveLifeMiniAppClinicalTrialOutput> MiniAppDetail(long id)
    {
        var trial = await _trialRep.GetFirstAsync(u => u.Id == id && u.Status == "recruiting")
            ?? throw Oops.Oh("招募项目不存在或已下架").StatusCode(404);

        return ToMiniAppTrialOutput(trial);
    }

    private static SaveLifeMiniAppClinicalTrialOutput ToMiniAppTrialOutput(SaveLifeClinicalTrial trial)
    {
        return new SaveLifeMiniAppClinicalTrialOutput
        {
            Id = trial.Id.ToString(),
            Title = trial.Title,
            ProtocolNumber = trial.ProtocolNumber,
            CdeRegistrationNumber = trial.CdeRegistrationNumber,
            Description = trial.Description,
            Sponsor = trial.Sponsor,
            ClinicalPhase = trial.ClinicalPhase,
            TrialPhase = trial.ClinicalPhase,
            DiseaseArea = trial.DiseaseArea,
            Indication = trial.Indication,
            Status = trial.Status,
            TargetCount = trial.TargetCount,
            EnrolledCount = trial.EnrolledCount,
            StartDate = trial.StartDate,
            EndDate = trial.EndDate,
            ParticipantType = trial.ParticipantType,
            InclusionCriteria = trial.InclusionCriteria,
            ExclusionCriteria = trial.ExclusionCriteria,
            TrialDuration = trial.TrialDuration,
            Benefits = trial.Benefits,
            EthicsCommitteeInfo = trial.EthicsCommitteeInfo,
            Sites = trial.Sites,
            Location = trial.Location,
            Requirements = trial.Requirements,
            CreateTime = trial.CreateTime,
            UpdateTime = trial.UpdateTime
        };
    }

    private static SaveLifeMiniAppAnnouncementOutput ToMiniAppAnnouncementOutput(SaveLifeAnnouncement announcement)
    {
        return new SaveLifeMiniAppAnnouncementOutput
        {
            Id = announcement.Id.ToString(),
            Title = announcement.Title,
            Content = announcement.Content,
            AnnouncementType = announcement.AnnouncementType,
            BusinessType = announcement.BusinessType,
            BusinessId = announcement.BusinessId?.ToString(),
            CreateTime = announcement.CreateTime
        };
    }

    private static List<SaveLifeMiniAppKnowledgeOutput> ToMiniAppKnowledgeOutputs(List<SaveLifeDiseaseKnowledge> knowledge)
    {
        return knowledge.Select(item =>
        {
            return new SaveLifeMiniAppKnowledgeOutput
            {
                Id = item.Id.ToString(),
                CategoryId = item.CategoryId?.ToString(),
                CategoryName = SaveLifeKnowledgeContentCategory.GetLabel(item.ContentCategory),
                ContentCategory = SaveLifeKnowledgeContentCategory.NormalizeOrDefault(item.ContentCategory),
                Title = item.Title,
                Author = item.Author,
                DiseaseName = item.DiseaseName,
                Content = null,
                Summary = item.Summary,
                ImageUrl = item.ImageUrl,
                Tags = item.Tags,
                ViewCount = item.ViewCount,
                LikeCount = item.LikeCount,
                CreateTime = item.CreateTime,
                UpdateTime = item.UpdateTime
            };
        }).ToList();
    }

    private async Task EnsureCdeRegistrationNumberAvailable(
        string cdeRegistrationNumber,
        long? excludedId = null)
    {
        var exists = await _trialRep.AsQueryable()
            .Where(u => u.CdeRegistrationNumber == cdeRegistrationNumber)
            .WhereIF(excludedId.HasValue, u => u.Id != excludedId!.Value)
            .AnyAsync();
        if (exists)
            throw Oops.Oh($"CDE 登记号“{cdeRegistrationNumber}”已存在").StatusCode(409);
    }

    private static string RequireCdeRegistrationNumber(string? value)
    {
        var normalized = SaveLifeCdeRegistrationNumber.Normalize(value);
        return !string.IsNullOrWhiteSpace(normalized)
            ? normalized
            : throw Oops.Oh("CDE 登记号不能为空").StatusCode(422);
    }

    private static int NormalizePageSize(int pageSize)
    {
        if (pageSize <= 0)
            return 10;

        return Math.Min(pageSize, 50);
    }

    private static int NormalizeProjectListPage(int page)
    {
        return page <= 0 ? 1 : page;
    }

    private static int NormalizeProjectListPageSize(int pageSize)
    {
        if (pageSize <= 0)
            return ProjectListDefaultPageSize;

        return Math.Min(pageSize, ProjectListMaxPageSize);
    }
}
