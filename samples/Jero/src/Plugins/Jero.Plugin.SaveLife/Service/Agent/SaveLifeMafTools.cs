using System.ComponentModel;
using System.Text.Json;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Jero.Plugin.SaveLife.Service;

public sealed class MedicalInformationTool(IHttpContextAccessor httpContextAccessor)
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public AIFunction CreateFunction(JsonSerializerOptions serializerOptions)
    {
        return AIFunctionFactory.Create(
            SearchAsync,
            name: nameof(MedicalInformationTool),
            serializerOptions: serializerOptions);
    }

    [Description("查询医线生机后台已发布的医疗资讯。需要平台资料或真实引用时使用；普通医学常识可以直接回答。")]
    public Task<MedicalInformationToolResult> SearchAsync(
        [Description("要查询的完整医疗问题。")]
        string query,
        [Description("从当前问题提取 0 到 5 个核心检索词；没有明确条件时传空数组。用户要求结合本人情况时，只可加入已确认健康档案中的诊断。")]
        string[] searchTerms,
        CancellationToken cancellationToken = default)
    {
        return GetService().SearchMedicalInformationAsync(
            query,
            searchTerms,
            cancellationToken);
    }

    private SaveLifeMafToolService GetService()
    {
        return _httpContextAccessor.HttpContext?.RequestServices
            .GetRequiredService<SaveLifeMafToolService>()
            ?? throw new InvalidOperationException("MedicalInformationTool requires an active HTTP request.");
    }
}

public sealed class ClinicalTrialTool(IHttpContextAccessor httpContextAccessor)
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public AIFunction CreateFunction(JsonSerializerOptions serializerOptions)
    {
        return AIFunctionFactory.Create(
            SearchAsync,
            new AIFunctionFactoryOptions
            {
                Name = nameof(ClinicalTrialTool),
                SerializerOptions = serializerOptions,
                ConfigureParameterBinding = parameter => parameter.Name == "searchTerms"
                    ? new AIFunctionFactoryOptions.ParameterBindingOptions
                    {
                        BindParameter = (_, arguments) => BindSearchTerms(
                            arguments,
                            serializerOptions)
                    }
                    : default
            });
    }

    [Description("查询真实临床试验项目资料。公开查询可返回各项目状态；个体化初步匹配只返回正在招募的项目。只查询，不报名，也不判断入组资格。")]
    public Task<ClinicalTrialToolResult> SearchAsync(
        [Description("查询模式只能是 discovery 或 personalized。公开查项目用 discovery；用户明确询问适合本人或报名，以及问诊后判断适合主动推荐时用 personalized，服务端只按已确认健康档案初步匹配。")]
        string mode,
        [Description("discovery 时填写完整问题；personalized 时可省略。")]
        string? query = null,
        [Description("discovery 时填写 0 到 5 个核心检索词；personalized 时可省略。")]
        string[]? searchTerms = null,
        CancellationToken cancellationToken = default)
    {
        var personalize = mode?.Trim().ToLowerInvariant() switch
        {
            "discovery" => false,
            "personalized" => true,
            _ => throw new ArgumentOutOfRangeException(
                nameof(mode),
                mode,
                "Clinical trial mode must be discovery or personalized.")
        };
        return GetService().SearchClinicalTrialsAsync(
            query ?? string.Empty,
            searchTerms ?? [],
            personalize,
            cancellationToken);
    }

    private SaveLifeMafToolService GetService()
    {
        return _httpContextAccessor.HttpContext?.RequestServices
            .GetRequiredService<SaveLifeMafToolService>()
            ?? throw new InvalidOperationException("ClinicalTrialTool requires an active HTTP request.");
    }

    private static string[]? BindSearchTerms(
        AIFunctionArguments arguments,
        JsonSerializerOptions serializerOptions)
    {
        if (!arguments.TryGetValue("searchTerms", out var value) || value == null)
            return null;
        if (value is string[] terms)
            return terms;
        if (value is string term)
            return string.IsNullOrWhiteSpace(term) ? [] : [term];
        if (value is JsonElement element)
        {
            if (element.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
                return null;
            if (element.ValueKind == JsonValueKind.String)
            {
                var elementValue = element.GetString();
                return string.IsNullOrWhiteSpace(elementValue) ? [] : [elementValue];
            }
            if (element.ValueKind == JsonValueKind.Array)
                return element.Deserialize<string[]>(serializerOptions) ?? [];
        }
        throw new JsonException("searchTerms must be a string or an array of strings.");
    }
}

public sealed class RoutineGuidanceTool(IHttpContextAccessor httpContextAccessor)
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public AIFunction CreateFunction(JsonSerializerOptions serializerOptions)
    {
        return AIFunctionFactory.Create(
            BuildAsync,
            name: nameof(RoutineGuidanceTool),
            serializerOptions: serializerOptions);
    }

    [Description("用户主动索要小程序入口，或对话已推进到适合提供下一步入口时，返回一个导航操作，用于完善资料、上传病例、查看招募或继续报名。客户端会显示结果，正文不要重复；不需要导航时不要调用此 Tool。")]
    public Task<RoutineGuidanceToolResult> BuildAsync(
        [Description("目标：next_step（报名下一步）、personal_profile（个人资料）、health_profile（健康档案）、material（病例资料）、trial_list（项目信息）、enrollment_list（报名记录）、enrollment_detail（当前报名详情）。")]
        string destination,
        CancellationToken cancellationToken = default)
    {
        return GetService().BuildRoutineGuidanceAsync(
            destination,
            cancellationToken);
    }

    private SaveLifeMafToolService GetService()
    {
        return _httpContextAccessor.HttpContext?.RequestServices
            .GetRequiredService<SaveLifeMafToolService>()
            ?? throw new InvalidOperationException("RoutineGuidanceTool requires an active HTTP request.");
    }
}

public sealed class HealthProfileProposalTool(IHttpContextAccessor httpContextAccessor)
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public AIFunction CreateFunction(JsonSerializerOptions serializerOptions)
    {
        return AIFunctionFactory.Create(
            ProposeAsync,
            name: nameof(HealthProfileProposalTool),
            serializerOptions: serializerOptions);
    }

    [Description("把当前消息或本轮授权报告中一条明确、适合长期记录的本人健康事实生成待确认信息。每轮最多一次；不直接写健康档案。")]
    public Task<HealthProfileProposalToolResult> ProposeAsync(
        [Description("允许记录的健康档案字段，只能是 city、diagnosis、diseaseStage、diseaseStatus、treatmentHistory、currentTreatment、pathology、biomarkers、comorbidities、medicalHistory 或 allergies。")]
        string field,
        [Description("待确认的简洁事实值，不得包含推测。")]
        string proposedValue,
        [Description("来源：user_message 表示当前消息；material 表示本轮授权报告。")]
        string sourceType,
        [Description("逐字来自当前消息或报告提取文本的短证据；图片报告填写识别到的原文事实。")]
        string evidence,
        [Description("material 来源填写本轮材料 Id；user_message 来源留空。")]
        string? sourceMaterialId = null,
        CancellationToken cancellationToken = default)
    {
        return GetService().ProposeHealthProfileAsync(
            field,
            proposedValue,
            sourceType,
            evidence,
            sourceMaterialId,
            cancellationToken);
    }

    private SaveLifeMafToolService GetService()
    {
        return _httpContextAccessor.HttpContext?.RequestServices
            .GetRequiredService<SaveLifeMafToolService>()
            ?? throw new InvalidOperationException("HealthProfileProposalTool requires an active HTTP request.");
    }
}

public sealed class SaveLifeMafToolService(
    SaveLifeMafRequestContext requestContext,
    ISaveLifeAgentRetrievalService retrievalService,
    SqlSugarRepository<SaveLifeEnrollmentMaterial> materialRep,
    SqlSugarRepository<SaveLifeEnrollmentRecord> enrollmentRep,
    ILogger<SaveLifeMafToolService> logger)
{
    private readonly SaveLifeMafRequestContext _requestContext = requestContext;
    private readonly ISaveLifeAgentRetrievalService _retrievalService = retrievalService;
    private readonly SqlSugarRepository<SaveLifeEnrollmentMaterial> _materialRep = materialRep;
    private readonly SqlSugarRepository<SaveLifeEnrollmentRecord> _enrollmentRep = enrollmentRep;
    private readonly ILogger<SaveLifeMafToolService> _logger = logger;

    public Task<HealthProfileProposalToolResult> ProposeHealthProfileAsync(
        string field,
        string proposedValue,
        string sourceType,
        string evidence,
        string? sourceMaterialId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            long? materialId = null;
            if (!string.IsNullOrWhiteSpace(sourceMaterialId))
            {
                if (!long.TryParse(sourceMaterialId, out var parsedMaterialId) || parsedMaterialId <= 0)
                    throw new SaveLifeAgentProfileProposalValidationException("档案提案材料Id格式无效");
                materialId = parsedMaterialId;
            }

            return Task.FromResult(_requestContext.StageHealthProfileProposal(
                field,
                proposedValue,
                sourceType,
                evidence,
                materialId));
        }
        catch (SaveLifeAgentProfileProposalValidationException exception)
        {
            return Task.FromResult(new HealthProfileProposalToolResult
            {
                Status = "invalid",
                Message = exception.Message
            });
        }
    }

    public async Task<MedicalInformationToolResult> SearchMedicalInformationAsync(
        string query,
        IReadOnlyList<string> searchTerms,
        CancellationToken cancellationToken)
    {
        query = NormalizeQuery(query);
        var normalizedSearchTerms = NormalizeSearchTerms(searchTerms);
        try
        {
            var retrieval = await _retrievalService.BuildRetrievalAsync(
                query,
                new SaveLifeAgentPatientSignal
                {
                    KnowledgeSearchTerms = normalizedSearchTerms,
                    UseSourceSearchTerms = true
                },
                cancellationToken: cancellationToken,
                scope: SaveLifeAgentRetrievalScope.KnowledgeOnly);
            _requestContext.MedicalRetrieval = retrieval;
            var result = new MedicalInformationToolResult
            {
                Status = retrieval.Knowledge.Count == 0 ? "no_results" : "ok",
                Message = retrieval.Knowledge.Count == 0
                    ? "后台医疗资讯暂未找到相关内容。"
                    : "以下内容来自医线生机后台已发布医疗资讯。",
                Items = retrieval.Knowledge.Select(item => new SaveLifeAgentKnowledgeOutput
                {
                    Id = item.Id,
                    Title = item.Title,
                    DiseaseName = item.DiseaseName,
                    Summary = item.Summary,
                    Content = item.Content,
                    Tags = item.Tags,
                    Score = item.Score
                }).ToList()
            };
            _requestContext.MedicalInformationResult = result;
            return result;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "MedicalInformationTool failed.");
            var result = new MedicalInformationToolResult
            {
                Status = "unavailable",
                Message = "后台医疗资讯暂时不可用。"
            };
            _requestContext.MedicalInformationResult = result;
            return result;
        }
    }

    public async Task<ClinicalTrialToolResult> SearchClinicalTrialsAsync(
        string query,
        IReadOnlyList<string> searchTerms,
        bool personalize,
        CancellationToken cancellationToken)
    {
        var retrievalQuery = personalize ? string.Empty : NormalizeQuery(query);
        IReadOnlyList<string> normalizedSearchTerms = personalize
            ? []
            : NormalizeSearchTerms(searchTerms);
        var resultMode = personalize
            ? SaveLifeAgentTrialResultLevels.InitialMatch
            : SaveLifeAgentTrialResultLevels.Discovery;
        var matchingGate = personalize
            ? SaveLifeAgentTrialSelection.EvaluateMatchingGate(_requestContext.HealthProfile)
            : new SaveLifeAgentTrialMatchingGate();
        if (personalize && !matchingGate.CanMatch)
        {
            _requestContext.ClinicalTrialRetrieval = new SaveLifeAgentRetrievalResult
            {
                TrialMatchingGate = matchingGate,
                TrialRetrievalMode = SaveLifeAgentTrialRetrievalModes.Matching
            };
            var insufficient = new ClinicalTrialToolResult
            {
                Status = "insufficient_profile",
                Mode = resultMode,
                Message = BuildTrialMatchingGateMessage(matchingGate.MissingFields),
                MissingFields = matchingGate.MissingFields.ToList()
            };
            _requestContext.ClinicalTrialResult = insufficient;
            return insufficient;
        }

        try
        {
            var scope = personalize
                ? new SaveLifeAgentRetrievalScope(false, true, false)
                {
                    TrialRetrievalMode = SaveLifeAgentTrialRetrievalModes.Matching
                }
                : SaveLifeAgentRetrievalScope.ProjectDiscovery;
            var retrieval = await _retrievalService.BuildRetrievalAsync(
                retrievalQuery,
                new SaveLifeAgentPatientSignal
                {
                    CurrentPatient = personalize ? _requestContext.HealthProfile : null,
                    SearchTerms = [],
                    TrialSearchTerms = normalizedSearchTerms,
                    UseSourceSearchTerms = true
                },
                _requestContext.Conversation.ExternalUserId,
                _requestContext.Conversation.EnrollmentRecordId,
                cancellationToken,
                scope);
            retrieval.TrialRetrievalMode = personalize
                ? SaveLifeAgentTrialRetrievalModes.Matching
                : SaveLifeAgentTrialRetrievalModes.Discovery;
            if (personalize)
            {
                retrieval.TrialMatchingGate = matchingGate;
                retrieval.Trials = retrieval.Trials.Take(3).ToList();
            }
            _requestContext.ClinicalTrialRetrieval = retrieval;

            var result = new ClinicalTrialToolResult
            {
                Status = retrieval.Trials.Count == 0 ? "no_results" : "ok",
                Mode = resultMode,
                Message = BuildTrialMessage(retrieval.Trials.Count, personalize),
                Items = retrieval.Trials.Select(item => new SaveLifeAgentMatchedTrialOutput
                {
                    Id = item.Id,
                    Title = item.Title,
                    DiseaseArea = item.DiseaseArea,
                    Indication = item.Indication,
                    ClinicalPhase = item.ClinicalPhase,
                    Sponsor = item.Sponsor,
                    Location = item.Location,
                    Status = item.Status,
                    Requirements = item.Requirements,
                    Description = item.Description,
                    Benefits = item.Benefits,
                    ResultLevel = resultMode,
                    IsPinned = item.IsPinned,
                    Score = item.Score
                }).ToList()
            };
            _requestContext.ClinicalTrialResult = result;
            return result;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "ClinicalTrialTool failed.");
            var result = new ClinicalTrialToolResult
            {
                Status = "unavailable",
                Mode = resultMode,
                Message = "临床试验数据暂时不可用。"
            };
            _requestContext.ClinicalTrialResult = result;
            return result;
        }
    }

    public async Task<RoutineGuidanceToolResult> BuildRoutineGuidanceAsync(
        string destination,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var normalizedDestination = destination?.Trim().ToLowerInvariant() ?? string.Empty;
        RoutineGuidanceToolResult result;
        if (normalizedDestination == "next_step")
        {
            try
            {
                result = await BuildNextStepGuidanceAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "RoutineGuidanceTool state lookup failed.");
                result = RoutineGuidanceToolResult.Empty();
            }
        }
        else
        {
            result = BuildNavigationGuidance(
                    normalizedDestination,
                    _requestContext.BusinessContext)
                ?? RoutineGuidanceToolResult.Empty();
        }

        _requestContext.RoutineGuidanceResult = result;
        return result;
    }

    private async Task<RoutineGuidanceToolResult> BuildNextStepGuidanceAsync(
        CancellationToken cancellationToken)
    {
        if (_requestContext.BusinessContext is
            {
                ContextType: "enrollment",
                ContextId: { Length: > 0 }
            })
        {
            return BuildNavigationGuidance(
                    "enrollment_detail",
                    _requestContext.BusinessContext)
                ?? RoutineGuidanceToolResult.Empty();
        }

        var externalUserId = _requestContext.Conversation.ExternalUserId;
        var profile = _requestContext.CurrentUserProfile;
        if (profile == null || SaveLifeMiniAppProfileHelper.CalculateCompleteness(profile).Score < 100)
            return BuildNavigationGuidance("personal_profile", null)!;

        var patientProfileId = long.TryParse(_requestContext.HealthProfile?.Id, out var parsedPatientId)
            ? parsedPatientId
            : 0;
        var hasMaterial = patientProfileId > 0 && await _materialRep.AsQueryable()
            .Where(item =>
                item.ExternalUserId == externalUserId &&
                item.PatientProfileId == patientProfileId &&
                item.EnrollmentRecordId <= 0 &&
                (item.Status == "submitted" || item.Status == "approved") &&
                !item.IsDelete)
            .AnyAsync(cancellationToken);
        if (!hasMaterial)
            return BuildNavigationGuidance("material", null)!;

        var hasEnrollment = await _enrollmentRep.AsQueryable()
            .Where(item =>
                item.ExternalUserId == externalUserId &&
                !item.IsDelete)
            .AnyAsync(cancellationToken);
        return BuildNavigationGuidance(hasEnrollment ? "enrollment_list" : "trial_list", null)!;
    }

    internal static RoutineGuidanceToolResult? BuildNavigationGuidance(
        string destination,
        SaveLifeAgentBusinessContextOutput? businessContext)
    {
        switch (destination?.Trim().ToLowerInvariant())
        {
            case "personal_profile":
                return Navigation(
                    "可以前往个人资料页完善信息，聊天中不会代为修改。",
                    SaveLifeAgentSuggestedActionTypes.CompleteAccountProfile,
                    "完善个人信息",
                    "personal_profile");
            case "health_profile":
                return Navigation(
                    "可以前往健康档案页完善信息，聊天中不会代为修改。",
                    SaveLifeAgentSuggestedActionTypes.CompleteHealthProfile,
                    "完善健康档案",
                    "health_profile");
            case "material":
                return Navigation(
                    "可以前往病例资料页查看或上传，资料仅在当前登录账号范围内使用。",
                    SaveLifeAgentSuggestedActionTypes.UploadMaterial,
                    "上传病例资料",
                    "material");
            case "enrollment_list":
                return Navigation(
                    "可以前往报名记录页查看当前账号的报名进度。",
                    SaveLifeAgentSuggestedActionTypes.ViewEnrollments,
                    "查看报名记录",
                    "enrollment");
            case "trial_list":
                return Navigation(
                    "可以前往项目信息页查看当前公开项目信息，并从项目详情提交报名信息。",
                    SaveLifeAgentSuggestedActionTypes.StartScreening,
                    "查看项目信息",
                    "trial");
            case "enrollment_detail" when businessContext is
                {
                    ContextType: "enrollment",
                    ContextId: { Length: > 0 }
                } enrollment:
                return Navigation(
                    "可以前往报名详情页查看当前进度。",
                    SaveLifeAgentSuggestedActionTypes.ViewEnrollment,
                    "查看报名详情",
                    "enrollment",
                    enrollment.ContextId);
            default:
                return null;
        }
    }

    private static string BuildTrialMessage(int count, bool personalize)
    {
        if (personalize)
        {
            if (count > 0)
                return $"根据当前已确认资料找到 {count} 个初步相关的招募项目，不代表符合入组条件。";
            return "根据当前已确认资料暂未找到初步相关的招募项目。";
        }
        if (count > 0)
            return $"找到 {count} 个项目资料。每项均包含当前状态，非招募中项目仅供资料了解，不能报名。";
        return "当前项目库未找到符合查询条件的项目。";
    }

    private static string BuildTrialMatchingGateMessage(IReadOnlyList<string> missingFields)
    {
        var descriptions = missingFields
            .Select(field => field switch
            {
                SaveLifeAgentTrialMatchingFields.PatientProfile => "当前账号本人的健康档案",
                SaveLifeAgentTrialMatchingFields.Diagnosis => "已经确认的疾病或诊断",
                SaveLifeAgentTrialMatchingFields.DiseaseStageOrStatus => "疾病阶段或当前病情状态",
                SaveLifeAgentTrialMatchingFields.Treatment => "既往治疗或当前治疗",
                _ => "用于初步匹配的健康资料"
            })
            .Distinct(StringComparer.Ordinal)
            .ToList();
        var missingSummary = descriptions.Count == 0
            ? string.Empty
            : $"缺少：{string.Join("、", descriptions)}。";
        return $"当前已确认健康档案信息不足，暂不能进行个体化初步匹配。{missingSummary}";
    }

    private static RoutineGuidanceToolResult Navigation(
        string guidanceText,
        string actionType,
        string actionLabel,
        string targetType,
        string? targetId = null)
    {
        return new RoutineGuidanceToolResult
        {
            Status = "guidance",
            GuidanceText = guidanceText,
            Actions =
            [
                new SaveLifeAgentSuggestedActionOutput
                {
                    Type = actionType,
                    Label = actionLabel,
                    TargetType = targetType,
                    TargetId = targetId
                }
            ]
        };
    }

    private string NormalizeQuery(string? query)
    {
        var normalized = query?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
            normalized = _requestContext.Request.LatestUserMessage.Content;
        return normalized.Length <= 4000 ? normalized : normalized[..4000];
    }

    private static IReadOnlyList<string> NormalizeSearchTerms(
        IEnumerable<string>? searchTerms)
    {
        return (searchTerms ?? [])
            .Select(value => value?.Trim())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!.Length <= 80 ? value : value[..80])
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(5)
            .ToList();
    }
}

public sealed class MedicalInformationToolResult
{
    public string Status { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public List<SaveLifeAgentKnowledgeOutput> Items { get; set; } = [];
}

public sealed class ClinicalTrialToolResult
{
    public string Status { get; set; } = string.Empty;

    public string Mode { get; set; } = SaveLifeAgentTrialResultLevels.Discovery;

    public string Message { get; set; } = string.Empty;

    public List<string> MissingFields { get; set; } = [];

    public List<SaveLifeAgentMatchedTrialOutput> Items { get; set; } = [];
}

public sealed class RoutineGuidanceToolResult
{
    public string Status { get; set; } = "empty";

    public string? GuidanceText { get; set; }

    public List<SaveLifeAgentSuggestedActionOutput> Actions { get; set; } = [];

    public static RoutineGuidanceToolResult Empty() => new();
}

public sealed class HealthProfileProposalToolResult
{
    public string Status { get; set; } = string.Empty;

    public string? Message { get; set; }

    public SaveLifePatientProfileProposalOutput? Proposal { get; set; }
}
