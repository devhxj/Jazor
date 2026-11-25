using System.Linq.Expressions;

namespace Jero.Plugin.SaveLife.Service;

public interface ISaveLifeAgentRetrievalService
{
    Task<SaveLifeAgentRetrievalResult> BuildRetrievalAsync(
        string query,
        SaveLifeAgentPatientSignal signal,
        string? externalUserId = null,
        long? enrollmentRecordId = null,
        CancellationToken cancellationToken = default,
        SaveLifeAgentRetrievalScope? scope = null);

    Task SyncKnowledgeIndexAsync(SaveLifeDiseaseKnowledge knowledge);

    Task SyncTrialIndexAsync(SaveLifeClinicalTrial trial);

    Task SyncMaterialIndexAsync(SaveLifeEnrollmentMaterial material, Func<Task<Stream>>? openFileStream = null);

    Task DeleteMaterialIndexAsync(long materialId, string externalUserId);
}

public sealed class SaveLifeAgentRetrievalService : ISaveLifeAgentRetrievalService, ITransient
{
    private const string KeywordFallbackSyncStatus = "keyword_fallback";

    private readonly SqlSugarRepository<SaveLifeAgentVectorIndex> _indexRep;
    private readonly SqlSugarRepository<SaveLifeDiseaseKnowledge> _knowledgeRep;
    private readonly SqlSugarRepository<SaveLifeClinicalTrial> _trialRep;
    private readonly SqlSugarRepository<SaveLifeEnrollmentMaterial> _materialRep;
    private readonly ISaveLifeAgentClientFactory _clientFactory;
    private readonly ISaveLifeAgentEmbeddingService _embeddingService;
    private readonly ISaveLifeMaterialEmbeddingService _materialEmbeddingService;

    public SaveLifeAgentRetrievalService(
        SqlSugarRepository<SaveLifeAgentVectorIndex> indexRep,
        SqlSugarRepository<SaveLifeDiseaseKnowledge> knowledgeRep,
        SqlSugarRepository<SaveLifeClinicalTrial> trialRep,
        SqlSugarRepository<SaveLifeEnrollmentMaterial> materialRep,
        ISaveLifeAgentClientFactory clientFactory,
        ISaveLifeAgentEmbeddingService embeddingService,
        ISaveLifeMaterialEmbeddingService materialEmbeddingService)
    {
        _indexRep = indexRep;
        _knowledgeRep = knowledgeRep;
        _trialRep = trialRep;
        _materialRep = materialRep;
        _clientFactory = clientFactory;
        _embeddingService = embeddingService;
        _materialEmbeddingService = materialEmbeddingService;
    }

    public Task<SaveLifeAgentRetrievalResult> BuildRetrievalAsync(
        string query,
        SaveLifeAgentPatientSignal signal,
        string? externalUserId = null,
        long? enrollmentRecordId = null,
        CancellationToken cancellationToken = default,
        SaveLifeAgentRetrievalScope? scope = null)
    {
        return SaveLifeAgentRetrievalFailureBoundary.ExecuteAsync(() =>
            BuildRetrievalCoreAsync(
                query,
                signal,
                externalUserId,
                enrollmentRecordId,
                scope ?? SaveLifeAgentRetrievalScope.All,
                cancellationToken),
            cancellationToken);
    }

    private async Task<SaveLifeAgentRetrievalResult> BuildRetrievalCoreAsync(
        string query,
        SaveLifeAgentPatientSignal signal,
        string? externalUserId,
        long? enrollmentRecordId,
        SaveLifeAgentRetrievalScope scope,
        CancellationToken cancellationToken)
    {
        var retrievalQuery = SaveLifeAgentRetrievalQuery.Build(query, signal);
        var knowledgeRetrievalQuery = BuildSourceRetrievalQuery(
            query,
            signal,
            signal.KnowledgeSearchTerms);
        var discoveryTrialRetrievalQuery = BuildSourceRetrievalQuery(
            query,
            signal,
            signal.TrialSearchTerms);
        var materialRetrievalQuery = BuildSourceRetrievalQuery(
            query,
            signal,
            signal.MaterialSearchTerms);
        var requestedTrialIds = scope.RequestedTrialIds
            .Where(id => id > 0)
            .Distinct()
            .ToList();
        var configuredTrialRetrievalMode = scope.IncludeTrials
            ? scope.TrialRetrievalMode
            : SaveLifeAgentTrialRetrievalModes.NotRequested;
        if (!SaveLifeAgentTrialRetrievalModes.All.Contains(configuredTrialRetrievalMode))
            throw new ArgumentOutOfRangeException(nameof(scope), configuredTrialRetrievalMode, "未知项目检索模式");

        var configuredForProjectDiscovery =
            configuredTrialRetrievalMode == SaveLifeAgentTrialRetrievalModes.Discovery;
        var trialMatchingGate = scope.IncludeTrials && !configuredForProjectDiscovery
            ? SaveLifeAgentTrialSelection.EvaluateMatchingGate(signal.CurrentPatient)
            : new SaveLifeAgentTrialMatchingGate();
        var requestedTrialIsPublicDiscovery =
            requestedTrialIds.Count > 0 &&
            configuredTrialRetrievalMode == SaveLifeAgentTrialRetrievalModes.Matching &&
            !trialMatchingGate.CanMatch;
        var trialRetrievalMode = requestedTrialIsPublicDiscovery
            ? SaveLifeAgentTrialRetrievalModes.Discovery
            : configuredTrialRetrievalMode;
        var isProjectDiscovery = trialRetrievalMode == SaveLifeAgentTrialRetrievalModes.Discovery;
        var includeAllTrialStatuses = isProjectDiscovery;
        var canRetrieveTrials = scope.IncludeTrials &&
            (requestedTrialIds.Count > 0 ||
             (isProjectDiscovery
                 ? discoveryTrialRetrievalQuery.HasProjectDiscoveryCriteria
                 : trialMatchingGate.CanMatch));
        var trialRetrievalQuery = !isProjectDiscovery && trialMatchingGate.CanMatch
            ? BuildTrialRetrievalQuery(query, signal)
            : discoveryTrialRetrievalQuery;
        var materialRetrievalPlan = SaveLifeAgentMaterialRetrievalPolicy.Build(
            materialRetrievalQuery,
            externalUserId,
            enrollmentRecordId,
            ParsePatientProfileId(signal.CurrentPatient?.Id),
            scope.RequestedMaterialIds,
            scope.HasMaterialIntent);
        var diagnostics = new SaveLifeAgentRetrievalDiagnostics();

        if (!scope.HasSources)
        {
            return new SaveLifeAgentRetrievalResult
            {
                Query = query,
                DiseaseKeyword = retrievalQuery.DiseaseKeyword,
                City = retrievalQuery.City,
                Diagnostics = diagnostics,
                TrialMatchingGate = trialMatchingGate,
                TrialRetrievalMode = trialRetrievalMode
            };
        }

        var knowledgeCandidates = new List<SaveLifeAgentVectorIndex>();
        if (scope.IncludeKnowledge)
        {
            knowledgeCandidates = await SaveLifeAgentRetrievalFailureBoundary.ExecuteForSourcesAsync(
                () => LoadKnowledgeCandidates(knowledgeRetrievalQuery),
                cancellationToken,
                diagnostics,
                SaveLifeAgentRetrievalSources.Knowledge);
            diagnostics.KnowledgeCandidateCount = knowledgeCandidates.Count;
            diagnostics.Complete(SaveLifeAgentRetrievalSources.Knowledge, knowledgeCandidates.Count);
        }

        var trialCandidates = new List<SaveLifeAgentVectorIndex>();
        if (canRetrieveTrials)
        {
            trialCandidates = await SaveLifeAgentRetrievalFailureBoundary.ExecuteForSourcesAsync(
                () => LoadTrialCandidates(
                    trialRetrievalQuery,
                    requestedTrialIds,
                    includeAllTrialStatuses),
                cancellationToken,
                diagnostics,
                SaveLifeAgentRetrievalSources.Trial);
            diagnostics.TrialCandidateCount = trialCandidates.Count;
            diagnostics.Complete(SaveLifeAgentRetrievalSources.Trial, trialCandidates.Count);
        }

        var materialCandidates = new List<SaveLifeAgentVectorIndex>();
        if (scope.IncludeMaterials && materialRetrievalPlan.CanSearch)
        {
            materialCandidates = await SaveLifeAgentRetrievalFailureBoundary.ExecuteForSourcesAsync(
                () => LoadMaterialCandidates(materialRetrievalPlan),
                cancellationToken,
                diagnostics,
                SaveLifeAgentRetrievalSources.Material);
            diagnostics.MaterialCandidateCount = materialCandidates.Count;
            diagnostics.Complete(SaveLifeAgentRetrievalSources.Material, materialCandidates.Count);
        }

        var sourcesWithCandidates = new List<string>();
        if (knowledgeCandidates.Count > 0)
            sourcesWithCandidates.Add(SaveLifeAgentRetrievalSources.Knowledge);
        if (trialCandidates.Count > 0)
            sourcesWithCandidates.Add(SaveLifeAgentRetrievalSources.Trial);
        if (materialCandidates.Count > 0)
            sourcesWithCandidates.Add(SaveLifeAgentRetrievalSources.Material);

        SaveLifeAgentOptions? options = null;
        if (sourcesWithCandidates.Count > 0)
        {
            options = await SaveLifeAgentRetrievalFailureBoundary.ExecuteForSourcesAsync(
                _clientFactory.GetOptionsAsync,
                cancellationToken,
                diagnostics,
                sourcesWithCandidates.ToArray());
        }

        var embeddingText = string.Join("\n", new[]
        {
            knowledgeCandidates.Count > 0 ? knowledgeRetrievalQuery.BuildEmbeddingText() : null,
            trialCandidates.Count > 0 ? trialRetrievalQuery.BuildEmbeddingText() : null
        }
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase));
        var textEmbeddingSources = new List<string>();
        if (knowledgeCandidates.Count > 0)
            textEmbeddingSources.Add(SaveLifeAgentRetrievalSources.Knowledge);
        if (trialCandidates.Count > 0 && requestedTrialIds.Count == 0)
            textEmbeddingSources.Add(SaveLifeAgentRetrievalSources.Trial);
        var queryEmbeddingTask = textEmbeddingSources.Count == 0
            ? Task.FromResult(SaveLifeAgentEmbeddingAttempt.NotNeeded())
            : SaveLifeAgentRetrievalFailureBoundary.ExecuteForSourcesAsync(
                () => GenerateEmbeddingAsync(embeddingText),
                cancellationToken,
                diagnostics,
                textEmbeddingSources.ToArray());
        var materialQueryEmbeddingTask = materialCandidates.Count == 0
            ? Task.FromResult(SaveLifeAgentEmbeddingAttempt.NotNeeded())
            : SaveLifeAgentRetrievalFailureBoundary.ExecuteForSourcesAsync(
                () => GenerateMaterialQueryEmbeddingAsync(
                    string.IsNullOrWhiteSpace(materialRetrievalQuery.BuildEmbeddingText())
                        ? query
                        : materialRetrievalQuery.BuildEmbeddingText()),
                cancellationToken,
                diagnostics,
                SaveLifeAgentRetrievalSources.Material);
        await Task.WhenAll(queryEmbeddingTask, materialQueryEmbeddingTask);

        var queryEmbeddingAttempt = await queryEmbeddingTask;
        diagnostics.TextEmbeddingStatus = queryEmbeddingAttempt.Status;

        var materialQueryEmbeddingAttempt = await materialQueryEmbeddingTask;
        diagnostics.MaterialEmbeddingStatus = materialQueryEmbeddingAttempt.Status;

        var knowledge = knowledgeCandidates.Count == 0
            ? []
            : await SaveLifeAgentRetrievalFailureBoundary.ExecuteForSourcesAsync(
                () => ScoreAndShapeKnowledgeAsync(
                    knowledgeCandidates,
                    queryEmbeddingAttempt.Embedding,
                    options!,
                    knowledgeRetrievalQuery),
                cancellationToken,
                diagnostics,
                SaveLifeAgentRetrievalSources.Knowledge);
        diagnostics.KnowledgeHitCount = knowledge.Count;
        if (scope.IncludeKnowledge)
            diagnostics.Complete(SaveLifeAgentRetrievalSources.Knowledge, knowledge.Count);

        var trials = trialCandidates.Count == 0
            ? []
            : await SaveLifeAgentRetrievalFailureBoundary.ExecuteForSourcesAsync(
                () => ScoreAndShapeTrialsAsync(
                    trialCandidates,
                    queryEmbeddingAttempt.Embedding,
                    options!,
                    trialRetrievalQuery,
                    options!.MaxTrialReferences,
                    requestedTrialIds,
                    includeAllTrialStatuses),
                cancellationToken,
                diagnostics,
                SaveLifeAgentRetrievalSources.Trial);
        diagnostics.TrialHitCount = trials.Count;
        if (scope.IncludeTrials && (isProjectDiscovery || trialMatchingGate.CanMatch))
            diagnostics.Complete(SaveLifeAgentRetrievalSources.Trial, trials.Count);

        var materials = materialCandidates.Count == 0
            ? []
            : await SaveLifeAgentRetrievalFailureBoundary.ExecuteForSourcesAsync(
                () => ScoreAndShapeMaterialsAsync(
                    materialCandidates,
                    materialQueryEmbeddingAttempt.Embedding,
                    options!,
                    materialRetrievalPlan),
                cancellationToken,
                diagnostics,
                SaveLifeAgentRetrievalSources.Material);
        diagnostics.MaterialHitCount = materials.Count;
        if (scope.IncludeMaterials && materialRetrievalPlan.CanSearch)
            diagnostics.Complete(SaveLifeAgentRetrievalSources.Material, materials.Count);

        return new SaveLifeAgentRetrievalResult
        {
            Query = query,
            DiseaseKeyword = !isProjectDiscovery && trialMatchingGate.CanMatch
                ? trialRetrievalQuery.DiseaseKeyword
                : discoveryTrialRetrievalQuery.DiseaseKeyword ?? knowledgeRetrievalQuery.DiseaseKeyword,
            City = !isProjectDiscovery && trialMatchingGate.CanMatch
                ? trialRetrievalQuery.City
                : discoveryTrialRetrievalQuery.City ?? knowledgeRetrievalQuery.City,
            Knowledge = knowledge,
            Trials = trials,
            Materials = materials,
            MaterialAccessScope = materialRetrievalPlan.CanSearch
                ? new SaveLifeAgentMaterialAccessScope(
                    materialRetrievalPlan.ExternalUserId,
                    materialRetrievalPlan.PatientProfileId!.Value,
                    materialRetrievalPlan.EnrollmentRecordId)
                : null,
            Diagnostics = diagnostics,
            TrialMatchingGate = trialMatchingGate,
            TrialRetrievalMode = trialRetrievalMode
        };
    }

    private static SaveLifeAgentRetrievalQuery BuildTrialRetrievalQuery(
        string query,
        SaveLifeAgentPatientSignal signal)
    {
        var patient = signal.CurrentPatient!;
        var queryWithConfirmedProfile = string.Join("\n", new[]
        {
            query,
            patient.Diagnosis,
            patient.DiseaseStage,
            patient.DiseaseStatus,
            patient.TreatmentHistory,
            patient.CurrentTreatment,
            patient.City
        }
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase));

        return SaveLifeAgentRetrievalQuery.Build(queryWithConfirmedProfile, new SaveLifeAgentPatientSignal
        {
            CurrentPatient = patient,
            DiseaseKeyword = patient.Diagnosis,
            City = patient.City,
            Phone = signal.Phone,
            SearchTerms = signal.SearchTerms
        });
    }

    private static SaveLifeAgentRetrievalQuery BuildSourceRetrievalQuery(
        string query,
        SaveLifeAgentPatientSignal signal,
        IReadOnlyList<string> sourceSearchTerms)
    {
        return SaveLifeAgentRetrievalQuery.Build(query, new SaveLifeAgentPatientSignal
        {
            CurrentPatient = signal.CurrentPatient,
            DiseaseKeyword = signal.DiseaseKeyword,
            City = signal.City,
            Phone = signal.Phone,
            SearchTerms = signal.UseSourceSearchTerms
                ? sourceSearchTerms
                : signal.SearchTerms
        });
    }

    private static long? ParsePatientProfileId(string? value)
    {
        return long.TryParse(value, out var patientProfileId) && patientProfileId > 0
            ? patientProfileId
            : null;
    }

    public async Task SyncKnowledgeIndexAsync(SaveLifeDiseaseKnowledge knowledge)
    {
        if (!knowledge.IsPublished)
        {
            await DeleteIndexAsync("knowledge", knowledge.Id);
            await MarkKnowledgeBaseStateAsync(knowledge, false, null);
            return;
        }

        var text = BuildKnowledgeText(knowledge);
        var hash = _embeddingService.ComputeContentHash(text);
        var existing = await _indexRep.GetFirstAsync(u => u.ResourceType == "knowledge" && u.ResourceId == knowledge.Id && u.ExternalUserId == null);
        if (existing != null && existing.ContentHash == hash && existing.SyncStatus == "synced")
        {
            await MarkKnowledgeBaseStateAsync(knowledge, true, null);
            return;
        }

        try
        {
            var embedding = await _embeddingService.GenerateEmbeddingAsync(text);
            await UpsertIndexAsync(
                existing,
                "knowledge",
                knowledge.Id,
                null,
                knowledge.Title,
                knowledge.DiseaseName,
                text,
                hash,
                knowledge.SortOrder,
                false,
                embedding,
                (await _clientFactory.GetOptionsAsync()).EmbeddingModel);
            await MarkKnowledgeBaseStateAsync(knowledge, true, null);
        }
        catch (Exception ex)
        {
            var error = SaveLifeAgentDiagnostics.BuildErrorSummary(ex, 512) ?? "疾病知识向量入库失败";
            await MarkIndexSyncFailedAsync(existing, "knowledge", knowledge.Id, null, knowledge.Title, knowledge.DiseaseName, text, hash, knowledge.SortOrder, false, error);
            await MarkKnowledgeBaseStateAsync(knowledge, false, error);
            throw;
        }
    }

    public async Task SyncTrialIndexAsync(SaveLifeClinicalTrial trial)
    {
        var text = BuildTrialText(trial);
        var hash = _embeddingService.ComputeContentHash(text);
        var existing = await _indexRep.GetFirstAsync(u => u.ResourceType == "trial" && u.ResourceId == trial.Id && u.ExternalUserId == null);
        if (existing != null &&
            existing.ContentHash == hash &&
            (existing.SyncStatus == "synced" ||
                (existing.SyncStatus == "pending" && !string.IsNullOrWhiteSpace(existing.EmbeddingJson))))
        {
            if (existing.SyncStatus != "synced" ||
                existing.SyncError != null ||
                existing.IsPinned != trial.IsPinned ||
                existing.SortOrder != trial.SortOrder)
            {
                existing.IsPinned = trial.IsPinned;
                existing.SortOrder = trial.SortOrder;
                existing.SyncStatus = "synced";
                existing.SyncError = null;
                existing.IndexedTime = DateTime.Now;
                await _indexRep.AsUpdateable(existing)
                    .UpdateColumns(u => new { u.IsPinned, u.SortOrder, u.SyncStatus, u.SyncError, u.IndexedTime })
                    .ExecuteCommandAsync();
            }

            await MarkTrialKnowledgeBaseStateAsync(trial, true, null);
            return;
        }

        try
        {
            var embedding = await _embeddingService.GenerateEmbeddingAsync(text);
            await UpsertIndexAsync(
                existing,
                "trial",
                trial.Id,
                null,
                trial.Title,
                trial.DiseaseArea,
                text,
                hash,
                trial.SortOrder,
                trial.IsPinned,
                embedding,
                (await _clientFactory.GetOptionsAsync()).EmbeddingModel);
            await MarkTrialKnowledgeBaseStateAsync(trial, true, null);
        }
        catch (Exception ex)
        {
            var error = SaveLifeAgentDiagnostics.BuildErrorSummary(ex, 512) ?? "临床试验项目向量入库失败";
            await MarkIndexSyncFailedAsync(existing, "trial", trial.Id, null, trial.Title, trial.DiseaseArea, text, hash, trial.SortOrder, trial.IsPinned, error);
            await MarkTrialKnowledgeBaseStateAsync(trial, false, error);
            throw;
        }
    }

    public async Task SyncMaterialIndexAsync(SaveLifeEnrollmentMaterial material, Func<Task<Stream>>? openFileStream = null)
    {
        if (string.IsNullOrWhiteSpace(material.ExternalUserId))
            throw Oops.Oh("报名材料缺少用户归属，不能进入私有知识库").StatusCode(400);
        if (!material.PatientProfileId.HasValue)
            throw Oops.Oh("报名材料缺少患者归属，不能进入私有知识库").StatusCode(400);

        var existing = await _indexRep.GetFirstAsync(u =>
            u.ResourceType == "material" &&
            u.ResourceId == material.Id &&
            u.ExternalUserId == material.ExternalUserId);
        var processingLease = SaveLifeMaterialProcessingLease.TryFromMaterial(material);

        if (material.IsDelete)
        {
            if (existing != null)
                await _indexRep.DeleteAsync(existing);
            return;
        }

        var text = BuildMaterialText(material);
        var hash = _embeddingService.ComputeContentHash(text);

        try
        {
            await EnsureMaterialProcessingCurrentAsync(processingLease);
            var suffix = SaveLifeEnrollmentMaterialHelper.NormalizeExtension(material.FileSuffix ?? material.FileName);
            var contentType = SaveLifeEnrollmentMaterialHelper.ResolveContentType(suffix, material.ContentType);
            var isImageOrVideo = SaveLifeEnrollmentMaterialHelper.IsImageOrVideo(suffix, contentType);
            var materialEmbedding = await _materialEmbeddingService.GenerateMaterialEmbeddingAsync(
                material,
                openFileStream ?? MissingMaterialFileStream);
            await EnsureMaterialProcessingCurrentAsync(processingLease);
            if (materialEmbedding == null)
                throw Oops.Oh(SaveLifeEnrollmentMaterialHelper.MissingMaterialEmbeddingConfigurationMessage).StatusCode(503);

            var embedding = SaveLifeAgentRetrievalFailureBoundary.RequireEmbedding(
                materialEmbedding.Embedding,
                "material");
            var embeddingModel = materialEmbedding.Model;
            const string embeddingKind = "multimodal";
            if (existing != null &&
                existing.ContentHash == hash &&
                existing.SyncStatus == "synced" &&
                existing.EmbeddingKind == embeddingKind &&
                existing.EmbeddingModel == embeddingModel)
            {
                return;
            }
            if (isImageOrVideo)
                material.ExtractModel = materialEmbedding.Model;

            await PublishMaterialIndexAsync(
                processingLease,
                () => UpsertIndexAsync(
                    existing,
                    "material",
                    material.Id,
                    material.ExternalUserId,
                    BuildMaterialTitle(material),
                    null,
                    text,
                    hash,
                    80,
                    false,
                    embedding,
                    embeddingModel,
                    embeddingKind,
                    material.PatientProfileId,
                    materialEmbedding.ConfiguredModel,
                    materialEmbedding.Dimensions,
                    material.ContentSha256));
        }
        catch (SaveLifeMaterialProcessingSupersededException)
        {
            throw;
        }
        catch (Exception ex)
        {
            var error = SaveLifeAgentDiagnostics.BuildErrorSummary(ex, 512) ?? "报名材料向量入库失败";
            await PublishMaterialIndexAsync(
                processingLease,
                () => MarkIndexSyncFailedAsync(
                    existing,
                    "material",
                    material.Id,
                    material.ExternalUserId,
                    BuildMaterialTitle(material),
                    null,
                    text,
                    hash,
                    80,
                    false,
                    error,
                    material.PatientProfileId));
            throw;
        }
    }

    private async Task EnsureMaterialProcessingCurrentAsync(SaveLifeMaterialProcessingLease? lease)
    {
        if (lease == null)
            return;

        if (!await SaveLifeMaterialProcessingPersistence.IsCurrentAsync(
                _materialRep.Context,
                lease,
                DateTime.Now))
            throw new SaveLifeMaterialProcessingSupersededException();
    }

    private async Task PublishMaterialIndexAsync(
        SaveLifeMaterialProcessingLease? lease,
        Func<Task> publish)
    {
        if (lease == null)
        {
            await publish();
            return;
        }

        if (!await SaveLifeMaterialIndexPublicationPersistence.TryPublishAsync(
                _materialRep.Context,
                lease,
                publish))
            throw new SaveLifeMaterialProcessingSupersededException();
    }

    private static Task<Stream> MissingMaterialFileStream()
    {
        throw new InvalidOperationException("病例资料多模态向量化缺少原始文件流");
    }

    public async Task DeleteMaterialIndexAsync(long materialId, string externalUserId)
    {
        var existing = await _indexRep.GetFirstAsync(u =>
            u.ResourceType == "material" &&
            u.ResourceId == materialId &&
            u.ExternalUserId == externalUserId);
        if (existing != null)
            await _indexRep.DeleteAsync(existing);
    }

    private async Task<List<SaveLifeAgentVectorIndex>> LoadKnowledgeCandidates(SaveLifeAgentRetrievalQuery retrievalQuery)
    {
        var candidates = await _indexRep.AsQueryable()
            .Where(u => u.ResourceType == "knowledge" && u.ExternalUserId == null)
            .Where(u => u.SyncStatus == "synced")
            .WhereIF(retrievalQuery.SearchTerms.Count > 0, BuildIndexTextPredicate(retrievalQuery.SearchTerms))
            .OrderBy(u => u.SortOrder)
            .OrderBy(u => u.CreateTime, OrderByType.Desc)
            .ToListAsync();

        var keywordCandidates = await LoadKnowledgeEntityCandidates(
            retrievalQuery,
            candidates.Select(u => u.ResourceId).ToHashSet());
        candidates.AddRange(keywordCandidates);
        return candidates;
    }

    private async Task<List<SaveLifeAgentVectorIndex>> LoadTrialCandidates(
        SaveLifeAgentRetrievalQuery retrievalQuery,
        IReadOnlyList<long> requestedTrialIds,
        bool includeAllTrialStatuses)
    {
        if (requestedTrialIds.Count > 0)
        {
            var requestedTrialQuery = _trialRep.AsQueryable()
                .Where(trial => requestedTrialIds.Contains(trial.Id));
            if (!includeAllTrialStatuses)
            {
                requestedTrialQuery = requestedTrialQuery.Where(trial =>
                    trial.Status == SaveLifeAgentTrialSelection.RecruitingStatus);
            }

            var requestedTrials = await requestedTrialQuery
                .ToListAsync();
            return requestedTrials.Select(BuildTrialCandidate).ToList();
        }

        var candidates = await _indexRep.AsQueryable()
            .Where(u => u.ResourceType == "trial" && u.ExternalUserId == null)
            .Where(u => u.SyncStatus == "synced")
            .WhereIF(retrievalQuery.SearchTerms.Count > 0, BuildTrialIndexTopicPredicate(retrievalQuery.SearchTerms))
            .OrderBy(u => u.CreateTime, OrderByType.Desc)
            .ToListAsync();

        var keywordCandidates = await LoadTrialEntityCandidates(
            retrievalQuery,
            candidates.Select(u => u.ResourceId).ToHashSet(),
            includeAllTrialStatuses);
        candidates.AddRange(keywordCandidates);
        return candidates;
    }

    private static SaveLifeAgentVectorIndex BuildTrialCandidate(SaveLifeClinicalTrial trial)
    {
        return new SaveLifeAgentVectorIndex
        {
            ResourceType = "trial",
            ResourceId = trial.Id,
            Title = trial.Title,
            DiseaseName = trial.DiseaseArea,
            Content = BuildTrialText(trial),
            IsPinned = trial.IsPinned,
            SortOrder = trial.SortOrder,
            SyncStatus = KeywordFallbackSyncStatus
        };
    }

    private async Task<List<SaveLifeAgentVectorIndex>> LoadKnowledgeEntityCandidates(
        SaveLifeAgentRetrievalQuery retrievalQuery,
        HashSet<long> excludedResourceIds)
    {
        var knowledge = await _knowledgeRep.AsQueryable()
            .Where(u => u.IsPublished)
            .WhereIF(excludedResourceIds.Count > 0, u => !excludedResourceIds.Contains(u.Id))
            .WhereIF(retrievalQuery.SearchTerms.Count > 0, BuildKnowledgeTextPredicate(retrievalQuery.SearchTerms))
            .OrderBy(u => u.SortOrder)
            .OrderBy(u => u.CreateTime, OrderByType.Desc)
            .ToListAsync();

        return knowledge.Select(item => new SaveLifeAgentVectorIndex
        {
            ResourceType = "knowledge",
            ResourceId = item.Id,
            Title = item.Title,
            DiseaseName = item.DiseaseName,
            Content = BuildKnowledgeText(item),
            SortOrder = item.SortOrder,
            SyncStatus = "synced"
        }).ToList();
    }

    private async Task<List<SaveLifeAgentVectorIndex>> LoadTrialEntityCandidates(
        SaveLifeAgentRetrievalQuery retrievalQuery,
        HashSet<long> excludedResourceIds,
        bool includeAllTrialStatuses)
    {
        var trials = await _trialRep.AsQueryable()
            .WhereIF(
                !includeAllTrialStatuses,
                u => u.Status == SaveLifeAgentTrialSelection.RecruitingStatus)
            .WhereIF(excludedResourceIds.Count > 0, u => !excludedResourceIds.Contains(u.Id))
            .WhereIF(retrievalQuery.SearchTerms.Count > 0, BuildTrialTextPredicate(retrievalQuery.SearchTerms))
            .OrderBy(u => u.CreateTime, OrderByType.Desc)
            .ToListAsync();

        return trials.Select(BuildTrialCandidate).ToList();
    }

    private async Task<List<SaveLifeAgentVectorIndex>> LoadMaterialCandidates(SaveLifeAgentMaterialRetrievalPlan plan)
    {
        if (!plan.CanSearch)
            return new List<SaveLifeAgentVectorIndex>();

        var patientProfileId = plan.PatientProfileId
            ?? throw new InvalidOperationException("材料检索缺少患者范围");

        var materialQuery = _materialRep.AsQueryable()
            .Where(u =>
                u.ExternalUserId == plan.ExternalUserId &&
                u.PatientProfileId == patientProfileId &&
                !u.IsDelete)
            .Where(u => u.Status == "submitted" || u.Status == "approved" || u.Status == "need_supplement" || u.Status == "rejected");
        var requestedMaterialIds = plan.RequestedMaterialIds.ToList();
        if (requestedMaterialIds.Count > 0)
            materialQuery = materialQuery.Where(u => requestedMaterialIds.Contains(u.Id));
        if (plan.EnrollmentRecordId.HasValue)
        {
            var enrollmentRecordId = plan.EnrollmentRecordId.Value;
            materialQuery = materialQuery.Where(u =>
                u.EnrollmentRecordId <= 0 || u.EnrollmentRecordId == enrollmentRecordId);
        }
        else
        {
            materialQuery = materialQuery.Where(u => u.EnrollmentRecordId <= 0);
        }

        var scopedMaterials = await materialQuery
            .Select(material => new SaveLifeEnrollmentMaterial
            {
                Id = material.Id,
                ExtractStatus = material.ExtractStatus,
                IndexStatus = material.IndexStatus,
                ContentSha256 = material.ContentSha256
            })
            .ToListAsync();
        if (requestedMaterialIds.Count > 0 && scopedMaterials.Count != requestedMaterialIds.Count)
        {
            throw MaterialNotReady(
                SaveLifeAgentMaterialReadinessStatuses.Failed,
                SaveLifeAgentMaterialReadinessMessages.SelectedUnavailable,
                retryable: false);
        }
        if (plan.HasMaterialIntent)
        {
            if (scopedMaterials.Any(material =>
                    SaveLifeAgentMaterialRetrievalPolicy.GetReadinessStatus(material) ==
                    SaveLifeAgentMaterialReadinessStatuses.Failed))
            {
                throw MaterialNotReady(
                    SaveLifeAgentMaterialReadinessStatuses.Failed,
                    SaveLifeAgentMaterialReadinessMessages.Failed);
            }

            if (scopedMaterials.Any(material =>
                    SaveLifeAgentMaterialRetrievalPolicy.GetReadinessStatus(material) ==
                    SaveLifeAgentMaterialReadinessStatuses.Processing))
            {
                throw MaterialNotReady(
                    SaveLifeAgentMaterialReadinessStatuses.Processing,
                    SaveLifeAgentMaterialReadinessMessages.Processing);
            }
        }

        var materialIds = scopedMaterials
            .Where(material =>
                SaveLifeAgentMaterialRetrievalPolicy.GetReadinessStatus(material) ==
                SaveLifeAgentMaterialReadinessStatuses.Ready)
            .Select(material => material.Id)
            .ToList();

        if (materialIds.Count == 0)
            return new List<SaveLifeAgentVectorIndex>();

        var materialOptions = await _clientFactory.GetOptionsAsync();
        var syncedIndexQuery = _indexRep.AsQueryable()
            .Where(u =>
                u.ResourceType == "material" &&
                u.ExternalUserId == plan.ExternalUserId &&
                u.PatientProfileId == patientProfileId &&
                u.EmbeddingKind == "multimodal" &&
                u.EmbeddingConfiguredModel == materialOptions.MaterialEmbeddingModel &&
                u.EmbeddingDimensions == materialOptions.MaterialEmbeddingDimensions)
            .Where(u => u.SyncStatus == "synced")
            .Where(u => materialIds.Contains(u.ResourceId));
        var syncedIndexes = await syncedIndexQuery.ToListAsync();
        var materialHashes = scopedMaterials.ToDictionary(item => item.Id, item => item.ContentSha256);
        var currentIndexes = syncedIndexes
            .Where(index =>
                materialHashes.TryGetValue(index.ResourceId, out var contentSha256) &&
                !string.IsNullOrWhiteSpace(contentSha256) &&
                string.Equals(index.SourceContentSha256, contentSha256, StringComparison.OrdinalIgnoreCase))
            .ToList();
        if (plan.HasMaterialIntent)
        {
            var indexedMaterialIds = currentIndexes.Select(index => index.ResourceId).ToList();
            if (materialIds.Any(materialId => !indexedMaterialIds.Contains(materialId)))
            {
                throw MaterialNotReady(
                    SaveLifeAgentMaterialReadinessStatuses.Failed,
                    SaveLifeAgentMaterialReadinessMessages.Failed);
            }
        }

        if (requestedMaterialIds.Count == 0 && !plan.EnrollmentRecordId.HasValue && plan.SearchTerms.Count > 0)
            currentIndexes = currentIndexes
                .Where(index => plan.SearchTerms.Take(12).Any(term =>
                    (index.Title?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (index.Content?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (index.DiseaseName?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)))
                .ToList();

        return currentIndexes
            .OrderBy(u => u.SortOrder)
            .ThenByDescending(u => u.CreateTime)
            .Take(16)
            .ToList();
    }

    private async Task<List<SaveLifeAgentKnowledgeReference>> ScoreAndShapeKnowledgeAsync(
        List<SaveLifeAgentVectorIndex> candidates,
        float[]? queryEmbedding,
        SaveLifeAgentOptions options,
        SaveLifeAgentRetrievalQuery retrievalQuery)
    {
        var results = new List<SaveLifeAgentKnowledgeReference>();
        var resourceIds = candidates.Select(u => u.ResourceId).Distinct().ToList();
        var knowledgeById = resourceIds.Count == 0
            ? new Dictionary<long, SaveLifeDiseaseKnowledge>()
            : (await _knowledgeRep.AsQueryable()
                .Where(u => resourceIds.Contains(u.Id))
                .ToListAsync())
                .ToDictionary(u => u.Id);
        foreach (var candidate in candidates)
        {
            if (!knowledgeById.TryGetValue(candidate.ResourceId, out var knowledge) || !knowledge.IsPublished)
                continue;

            var score = GetScore(candidate, queryEmbedding, retrievalQuery);
            results.Add(new SaveLifeAgentKnowledgeReference
            {
                Id = candidate.ResourceId.ToString(),
                Title = knowledge.Title,
                DiseaseName = knowledge.DiseaseName,
                Summary = knowledge.Summary,
                Content = Truncate(candidate.Content, 600),
                Score = score,
                Tags = knowledge.Tags
            });
        }

        return results
            .Where(u => u.Score >= options.MinVectorScore || queryEmbedding == null)
            .OrderByDescending(u => u.Score)
            .ThenByDescending(u => u.Title)
            .Take(options.MaxKnowledgeReferences)
            .ToList();
    }

    private async Task<List<SaveLifeAgentTrialReference>> ScoreAndShapeTrialsAsync(
        List<SaveLifeAgentVectorIndex> candidates,
        float[]? queryEmbedding,
        SaveLifeAgentOptions options,
        SaveLifeAgentRetrievalQuery retrievalQuery,
        int maxReferences,
        IReadOnlyCollection<long>? exactTrialIds = null,
        bool includeAllTrialStatuses = false)
    {
        var results = new List<SaveLifeAgentTrialReference>();
        var resourceIds = candidates.Select(u => u.ResourceId).Distinct().ToList();
        var trialsById = resourceIds.Count == 0
            ? new Dictionary<long, SaveLifeClinicalTrial>()
            : (await _trialRep.AsQueryable()
                .Where(u => resourceIds.Contains(u.Id))
                .ToListAsync())
                .ToDictionary(u => u.Id);
        foreach (var candidate in candidates)
        {
            if (!trialsById.TryGetValue(candidate.ResourceId, out var trial) ||
                (!includeAllTrialStatuses &&
                 !string.Equals(
                     trial.Status,
                     SaveLifeAgentTrialSelection.RecruitingStatus,
                     StringComparison.OrdinalIgnoreCase)))
                continue;

            var score = exactTrialIds?.Contains(candidate.ResourceId) == true
                ? 1D
                : GetScore(
                    candidate,
                    queryEmbedding,
                    retrievalQuery,
                    trial.Title,
                    trial.ProtocolNumber,
                    trial.CdeRegistrationNumber,
                    trial.DiseaseArea,
                    trial.Indication,
                    trial.ClinicalPhase,
                    trial.Sponsor,
                    trial.Location,
                    trial.Sites,
                    trial.Description,
                    trial.Benefits);
            results.Add(new SaveLifeAgentTrialReference
            {
                Id = candidate.ResourceId.ToString(),
                Title = trial.Title,
                DiseaseArea = trial.DiseaseArea,
                Indication = trial.Indication,
                ClinicalPhase = trial.ClinicalPhase,
                Sponsor = trial.Sponsor,
                Location = trial.Location,
                Status = trial.Status,
                Requirements = Truncate(trial.Requirements, 600),
                Description = Truncate(trial.Description, 800),
                Benefits = Truncate(trial.Benefits, 800),
                InclusionCriteria = Truncate(trial.InclusionCriteria, 1200),
                ExclusionCriteria = Truncate(trial.ExclusionCriteria, 1200),
                Sites = Truncate(trial.Sites, 800),
                IsPinned = trial.IsPinned,
                SortOrder = trial.SortOrder,
                Score = score
            });
        }

        return results
            .Where(u => u.Score >= options.MinVectorScore || queryEmbedding == null)
            .OrderMatchesByRelevance()
            .Take(Math.Max(1, maxReferences))
            .ToList();
    }

    private async Task<List<SaveLifeAgentMaterialReference>> ScoreAndShapeMaterialsAsync(
        List<SaveLifeAgentVectorIndex> candidates,
        float[]? queryEmbedding,
        SaveLifeAgentOptions options,
        SaveLifeAgentMaterialRetrievalPlan plan)
    {
        var results = new List<SaveLifeAgentMaterialReference>();
        var validatedQuery = queryEmbedding == null
            ? null
            : SaveLifeAgentVectorCompatibilityPolicy.ValidateQueryVector(
                queryEmbedding,
                options.MaterialEmbeddingDimensions,
                options.MaterialEmbeddingModel);
        var resourceIds = candidates.Select(u => u.ResourceId).Distinct().ToList();
        var materialsById = resourceIds.Count == 0
            ? new Dictionary<long, SaveLifeEnrollmentMaterial>()
            : (await _materialRep.AsQueryable()
                .Where(u =>
                    resourceIds.Contains(u.Id) &&
                    u.ExternalUserId == plan.ExternalUserId &&
                    u.PatientProfileId == plan.PatientProfileId)
                .ToListAsync())
                .ToDictionary(u => u.Id);
        foreach (var candidate in candidates)
        {
            if (!materialsById.TryGetValue(candidate.ResourceId, out var material) ||
                !SaveLifeAgentMaterialRetrievalPolicy.IsEligibleForRetrieval(material, plan.ExternalUserId) ||
                !SaveLifeAgentMaterialRetrievalPolicy.IsInScope(material, plan))
                continue;

            var indexVector = SaveLifeAgentVectorCompatibilityPolicy.ParseIndexVector(
                candidate.EmbeddingJson,
                options.MaterialEmbeddingDimensions,
                options.MaterialEmbeddingModel);
            var score = SaveLifeAgentMaterialRetrievalPolicy.ScoreCandidate(
                candidate,
                material,
                plan,
                validatedQuery == null
                    ? 1
                    : SaveLifeAgentVectorCompatibilityPolicy.CosineSimilarity(
                        indexVector,
                        validatedQuery,
                        options.MaterialEmbeddingDimensions,
                        options.MaterialEmbeddingModel));
            results.Add(BuildMaterialReference(material, candidate, score));
        }

        return results
            .Where(u => u.Score >= options.MinVectorScore || queryEmbedding == null)
            .OrderByDescending(u => u.Score)
            .ThenByDescending(u => u.Title)
            .Take(4)
            .ToList();
    }

    private static SaveLifeAgentUnavailableException MaterialNotReady(
        string readinessStatus,
        string publicMessage,
        bool retryable = true)
    {
        return new SaveLifeAgentUnavailableException(
            SaveLifeAgentFailureCodes.MaterialNotReady,
            SaveLifeAgentFailureStages.Material,
            new InvalidOperationException($"Patient material is {readinessStatus}."),
            retryable: retryable,
            publicMessage: publicMessage);
    }

    private double GetScore(
        SaveLifeAgentVectorIndex index,
        float[]? queryEmbedding,
        SaveLifeAgentRetrievalQuery retrievalQuery,
        params string?[] topicValues)
    {
        var keywordScore = topicValues.Length == 0
            ? retrievalQuery.ScoreText(index.Title, index.DiseaseName, index.Content)
            : retrievalQuery.ScoreText(topicValues);
        if (string.Equals(index.SyncStatus, KeywordFallbackSyncStatus, StringComparison.Ordinal))
            return keywordScore;
        if (queryEmbedding == null)
            return keywordScore > 0 ? keywordScore : 1;

        var model = index.EmbeddingModel ?? "unknown";
        var validatedQuery = SaveLifeAgentVectorCompatibilityPolicy.ValidateQueryVector(
            queryEmbedding,
            queryEmbedding.Length,
            model);
        var vector = SaveLifeAgentVectorCompatibilityPolicy.ParseIndexVector(
            index.EmbeddingJson,
            validatedQuery.Length,
            model);
        var vectorScore = SaveLifeAgentVectorCompatibilityPolicy.CosineSimilarity(
            vector,
            validatedQuery,
            validatedQuery.Length,
            model);
        return Math.Max(vectorScore, keywordScore);
    }

    private async Task<SaveLifeAgentEmbeddingAttempt> GenerateEmbeddingAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return SaveLifeAgentEmbeddingAttempt.NotNeeded();

        var embedding = await _embeddingService.GenerateEmbeddingAsync(text);
        return SaveLifeAgentEmbeddingAttempt.Succeeded(
            SaveLifeAgentRetrievalFailureBoundary.RequireEmbedding(embedding, "text"));
    }

    private async Task<SaveLifeAgentEmbeddingAttempt> GenerateMaterialQueryEmbeddingAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return SaveLifeAgentEmbeddingAttempt.NotNeeded();

        var options = await _clientFactory.GetOptionsAsync();
        var embedding = (await _materialEmbeddingService.GenerateQueryEmbeddingAsync(text))?.Embedding;
        if (embedding != null)
        {
            embedding = SaveLifeAgentVectorCompatibilityPolicy.ValidateQueryVector(
                embedding,
                options.MaterialEmbeddingDimensions,
                options.MaterialEmbeddingModel);
        }
        return SaveLifeAgentEmbeddingAttempt.Succeeded(
            SaveLifeAgentRetrievalFailureBoundary.RequireEmbedding(embedding, "material"));
    }

    public static SaveLifeAgentMaterialReference BuildMaterialReference(
        SaveLifeEnrollmentMaterial material,
        SaveLifeAgentVectorIndex candidate,
        double score)
    {
        var isPersonalMaterial = SaveLifeEnrollmentMaterialHelper.IsPersonalMaterial(material);

        return new SaveLifeAgentMaterialReference
        {
            Id = material.Id.ToString(),
            MaterialId = material.Id.ToString(),
            EnrollmentRecordId = isPersonalMaterial ? null : material.EnrollmentRecordId.ToString(),
            IsPersonalMaterial = isPersonalMaterial,
            Title = candidate.Title,
            MaterialType = material.MaterialType,
            MaterialTypeText = SaveLifeEnrollmentMaterialHelper.GetMaterialTypeText(material.MaterialType),
            FileName = BuildMaterialDisplayFileName(material),
            Status = material.Status,
            StatusText = SaveLifeEnrollmentMaterialHelper.GetStatusText(material.Status),
            CanSatisfyRequiredMaterial = SaveLifeAgentMaterialRetrievalPolicy.CanSatisfyRequiredMaterial(material),
            RequiresSupplement = SaveLifeAgentMaterialRetrievalPolicy.RequiresSupplement(material),
            Summary = Truncate(candidate.Content, 600),
            Score = score
        };
    }

    private async Task UpsertIndexAsync(
        SaveLifeAgentVectorIndex? existing,
        string resourceType,
        long resourceId,
        string? externalUserId,
        string title,
        string? diseaseName,
        string content,
        string contentHash,
        int sortOrder,
        bool isPinned,
        float[]? embedding,
        string embeddingModel,
        string embeddingKind = "text",
        long? patientProfileId = null,
        string? embeddingConfiguredModel = null,
        int? embeddingDimensions = null,
        string? sourceContentSha256 = null)
    {
        var entity = existing ?? new SaveLifeAgentVectorIndex
        {
            ResourceType = resourceType,
            ResourceId = resourceId,
            ExternalUserId = externalUserId
        };

        entity.Title = title;
        entity.ExternalUserId = externalUserId;
        entity.PatientProfileId = patientProfileId;
        entity.DiseaseName = diseaseName;
        entity.Content = content;
        entity.ContentHash = contentHash;
        entity.SourceContentSha256 = sourceContentSha256;
        entity.EmbeddingModel = embeddingModel;
        entity.EmbeddingConfiguredModel = embeddingConfiguredModel ?? embeddingModel;
        entity.EmbeddingDimensions = embeddingDimensions ?? embedding?.Length;
        entity.EmbeddingKind = embeddingKind;
        entity.EmbeddingJson = embedding == null ? null : _embeddingService.SerializeVector(embedding);
        entity.IsPinned = isPinned;
        entity.SortOrder = sortOrder;
        entity.SyncStatus = "synced";
        entity.SyncError = null;
        entity.IndexedTime = DateTime.Now;

        if (existing == null)
        {
            await _indexRep.InsertAsync(entity);
            return;
        }

        await _indexRep.AsUpdateable(entity).IgnoreColumns(true).ExecuteCommandAsync();
    }

    private async Task MarkIndexSyncFailedAsync(
        SaveLifeAgentVectorIndex? existing,
        string resourceType,
        long resourceId,
        string? externalUserId,
        string title,
        string? diseaseName,
        string content,
        string contentHash,
        int sortOrder,
        bool isPinned,
        string error,
        long? patientProfileId = null)
    {
        var entity = existing ?? new SaveLifeAgentVectorIndex
        {
            ResourceType = resourceType,
            ResourceId = resourceId,
            ExternalUserId = externalUserId
        };

        entity.Title = title;
        entity.ExternalUserId = externalUserId;
        entity.PatientProfileId = patientProfileId;
        entity.DiseaseName = diseaseName;
        entity.Content = content;
        entity.ContentHash = contentHash;
        entity.IsPinned = isPinned;
        entity.SortOrder = sortOrder;
        entity.SyncStatus = "failed";
        entity.SyncError = Truncate(error, 512);
        entity.RetryCount += 1;
        entity.IndexedTime = DateTime.Now;

        if (existing == null)
        {
            await _indexRep.InsertAsync(entity);
            return;
        }

        await _indexRep.AsUpdateable(entity).IgnoreColumns(true).ExecuteCommandAsync();
    }

    private static string BuildKnowledgeText(SaveLifeDiseaseKnowledge knowledge)
    {
        return SaveLifeKnowledgeIndexTextBuilder.Build(knowledge);
    }

    private static Expression<Func<SaveLifeAgentVectorIndex, bool>> BuildIndexTextPredicate(IReadOnlyList<string> terms)
    {
        var expression = Expressionable.Create<SaveLifeAgentVectorIndex>();
        foreach (var term in terms.Take(12))
        {
            var value = term;
            expression = expression.Or(u =>
                (u.Title != null && u.Title.Contains(value)) ||
                (u.Content != null && u.Content.Contains(value)) ||
                (u.DiseaseName != null && u.DiseaseName.Contains(value)));
        }

        return expression.ToExpression();
    }

    private static Expression<Func<SaveLifeAgentVectorIndex, bool>> BuildTrialIndexTopicPredicate(
        IReadOnlyList<string> terms)
    {
        var expression = Expressionable.Create<SaveLifeAgentVectorIndex>();
        foreach (var term in terms.Take(12))
        {
            var value = term;
            expression = expression.Or(u =>
                (u.Title != null && u.Title.Contains(value)) ||
                (u.DiseaseName != null && u.DiseaseName.Contains(value)));
        }

        return expression.ToExpression();
    }

    private static Expression<Func<SaveLifeDiseaseKnowledge, bool>> BuildKnowledgeTextPredicate(IReadOnlyList<string> terms)
    {
        var expression = Expressionable.Create<SaveLifeDiseaseKnowledge>();
        foreach (var term in terms.Take(12))
        {
            var value = term;
            expression = expression.Or(u =>
                (u.Title != null && u.Title.Contains(value)) ||
                (u.Summary != null && u.Summary.Contains(value)) ||
                (u.Content != null && u.Content.Contains(value)) ||
                (u.DiseaseName != null && u.DiseaseName.Contains(value)) ||
                (u.Tags != null && u.Tags.Contains(value)));
        }

        return expression.ToExpression();
    }

    private static Expression<Func<SaveLifeClinicalTrial, bool>> BuildTrialTextPredicate(IReadOnlyList<string> terms)
    {
        var expression = Expressionable.Create<SaveLifeClinicalTrial>();
        foreach (var term in terms.Take(12))
        {
            var value = term;
            expression = expression.Or(u =>
                (u.Title != null && u.Title.Contains(value)) ||
                (u.ProtocolNumber != null && u.ProtocolNumber.Contains(value)) ||
                (u.CdeRegistrationNumber != null && u.CdeRegistrationNumber.Contains(value)) ||
                (u.DiseaseArea != null && u.DiseaseArea.Contains(value)) ||
                (u.Indication != null && u.Indication.Contains(value)) ||
                (u.Description != null && u.Description.Contains(value)) ||
                (u.Benefits != null && u.Benefits.Contains(value)) ||
                (u.Location != null && u.Location.Contains(value)) ||
                (u.Sites != null && u.Sites.Contains(value)) ||
                (u.Sponsor != null && u.Sponsor.Contains(value)));
        }

        return expression.ToExpression();
    }

    private static string BuildTrialText(SaveLifeClinicalTrial trial)
    {
        return string.Join("\n", new[]
        {
            trial.Title,
            BuildTrialContentField("项目状态", trial.Status),
            trial.DiseaseArea,
            trial.Indication,
            trial.ClinicalPhase,
            trial.Sponsor,
            trial.Location,
            trial.Sites,
            trial.Requirements,
            trial.InclusionCriteria,
            trial.ExclusionCriteria,
            BuildTrialContentField("项目描述", trial.Description),
            BuildTrialContentField("受试者权益", trial.Benefits)
        }.Where(u => !string.IsNullOrWhiteSpace(u)));
    }

    private static string? BuildTrialContentField(string label, string? value)
    {
        var normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : $"{label}：{normalized}";
    }

    private static string BuildMaterialText(SaveLifeEnrollmentMaterial material)
    {
        return string.Join("\n", new[]
        {
            BuildMaterialTitle(material),
            $"材料类型：{SaveLifeEnrollmentMaterialHelper.GetMaterialTypeText(material.MaterialType)}",
            $"文件名：{BuildMaterialDisplayFileName(material)}",
            $"材料状态：{SaveLifeEnrollmentMaterialHelper.GetStatusText(material.Status)}",
            $"用户备注：{material.Remark}",
            $"审核备注：{material.ReviewRemark}",
            $"补充要求：{material.SupplementRequest}",
            material.ExtractedText
        }.Where(u => !string.IsNullOrWhiteSpace(u)));
    }

    private static string BuildMaterialTitle(SaveLifeEnrollmentMaterial material)
    {
        return $"{SaveLifeEnrollmentMaterialHelper.GetMaterialTypeText(material.MaterialType)} - {BuildMaterialDisplayFileName(material)}";
    }

    private static string BuildMaterialDisplayFileName(SaveLifeEnrollmentMaterial material)
    {
        if (string.IsNullOrWhiteSpace(material.FileName))
            return "病例材料";

        return !string.IsNullOrWhiteSpace(material.FileSuffix) && !material.FileName.EndsWith(material.FileSuffix, StringComparison.OrdinalIgnoreCase)
            ? $"{material.FileName}{material.FileSuffix}"
            : material.FileName;
    }

    private async Task DeleteIndexAsync(string resourceType, long resourceId)
    {
        var existing = await _indexRep.GetFirstAsync(u => u.ResourceType == resourceType && u.ResourceId == resourceId && u.ExternalUserId == null);
        if (existing != null)
            await _indexRep.DeleteAsync(existing);
    }

    private async Task MarkKnowledgeBaseStateAsync(SaveLifeDiseaseKnowledge knowledge, bool isInKnowledgeBase, string? error)
    {
        knowledge.IsInKnowledgeBase = isInKnowledgeBase;
        knowledge.KnowledgeBaseError = error;
        ApplyKnowledgeBaseRetryState(knowledge, isInKnowledgeBase, error);
        await _knowledgeRep.AsUpdateable(knowledge)
            .UpdateColumns(u => new
            {
                u.IsInKnowledgeBase,
                u.KnowledgeBaseError,
                u.KnowledgeBaseRetryCount,
                u.KnowledgeBaseLastAttemptTime,
                u.KnowledgeBaseNextRetryTime
            })
            .ExecuteCommandAsync();
    }

    private async Task MarkTrialKnowledgeBaseStateAsync(SaveLifeClinicalTrial trial, bool isInKnowledgeBase, string? error)
    {
        trial.IsInKnowledgeBase = isInKnowledgeBase;
        trial.KnowledgeBaseError = error;
        ApplyKnowledgeBaseRetryState(trial, isInKnowledgeBase, error);
        await _trialRep.AsUpdateable(trial)
            .UpdateColumns(u => new
            {
                u.IsInKnowledgeBase,
                u.KnowledgeBaseError,
                u.KnowledgeBaseRetryCount,
                u.KnowledgeBaseLastAttemptTime,
                u.KnowledgeBaseNextRetryTime
            })
            .ExecuteCommandAsync();
    }

    private static void ApplyKnowledgeBaseRetryState(
        SaveLifeDiseaseKnowledge knowledge,
        bool isInKnowledgeBase,
        string? error)
    {
        if (isInKnowledgeBase || string.IsNullOrWhiteSpace(error))
        {
            knowledge.KnowledgeBaseRetryCount = 0;
            knowledge.KnowledgeBaseLastAttemptTime = null;
            knowledge.KnowledgeBaseNextRetryTime = null;
            return;
        }

        var now = DateTime.Now;
        knowledge.KnowledgeBaseRetryCount += 1;
        knowledge.KnowledgeBaseLastAttemptTime = now;
        knowledge.KnowledgeBaseNextRetryTime = SaveLifeKnowledgeBaseRetryPolicy.CalculateNextRetryTime(
            knowledge.KnowledgeBaseRetryCount,
            error,
            now);
    }

    private static void ApplyKnowledgeBaseRetryState(
        SaveLifeClinicalTrial trial,
        bool isInKnowledgeBase,
        string? error)
    {
        if (isInKnowledgeBase || string.IsNullOrWhiteSpace(error))
        {
            trial.KnowledgeBaseRetryCount = 0;
            trial.KnowledgeBaseLastAttemptTime = null;
            trial.KnowledgeBaseNextRetryTime = null;
            return;
        }

        var now = DateTime.Now;
        trial.KnowledgeBaseRetryCount += 1;
        trial.KnowledgeBaseLastAttemptTime = now;
        trial.KnowledgeBaseNextRetryTime = SaveLifeKnowledgeBaseRetryPolicy.CalculateNextRetryTime(
            trial.KnowledgeBaseRetryCount,
            error,
            now);
    }

    private static string? Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return value.Length <= maxLength ? value : value[..maxLength];
    }
}

public sealed class SaveLifeAgentRetrievalResult
{
    public string Query { get; set; } = string.Empty;

    public string? DiseaseKeyword { get; set; }

    public string? City { get; set; }

    public List<SaveLifeAgentKnowledgeReference> Knowledge { get; set; } = new();

    public List<SaveLifeAgentTrialReference> Trials { get; set; } = new();

    public List<SaveLifeAgentMaterialReference> Materials { get; set; } = new();

    internal SaveLifeAgentMaterialAccessScope? MaterialAccessScope { get; set; }

    public SaveLifeAgentRetrievalDiagnostics Diagnostics { get; set; } = new();

    public SaveLifeAgentTrialMatchingGate TrialMatchingGate { get; set; } = new();

    public string TrialRetrievalMode { get; set; } = SaveLifeAgentTrialRetrievalModes.NotRequested;

    public bool TrialRecommendationsSuppressed { get; set; }
}

public sealed record SaveLifeAgentRetrievalScope(
    bool IncludeKnowledge,
    bool IncludeTrials,
    bool IncludeMaterials)
{
    public static SaveLifeAgentRetrievalScope All { get; } = new(true, true, true)
    {
        TrialRetrievalMode = SaveLifeAgentTrialRetrievalModes.Matching
    };

    public static SaveLifeAgentRetrievalScope None { get; } = new(false, false, false)
    {
        TrialRetrievalMode = SaveLifeAgentTrialRetrievalModes.NotRequested
    };

    public static SaveLifeAgentRetrievalScope KnowledgeOnly { get; } = new(true, false, false)
    {
        TrialRetrievalMode = SaveLifeAgentTrialRetrievalModes.NotRequested
    };

    public static SaveLifeAgentRetrievalScope TrialsAndMaterials { get; } = new(false, true, true)
    {
        TrialRetrievalMode = SaveLifeAgentTrialRetrievalModes.Matching
    };

    public static SaveLifeAgentRetrievalScope ProjectDiscovery { get; } = new(false, true, false)
    {
        TrialRetrievalMode = SaveLifeAgentTrialRetrievalModes.Discovery
    };

    public static SaveLifeAgentRetrievalScope PublicIllnessHelp { get; } = new(true, true, false)
    {
        TrialRetrievalMode = SaveLifeAgentTrialRetrievalModes.Discovery
    };

    public static SaveLifeAgentRetrievalScope MaterialsOnly { get; } = new(false, false, true)
    {
        TrialRetrievalMode = SaveLifeAgentTrialRetrievalModes.NotRequested
    };

    public string TrialRetrievalMode { get; init; } = SaveLifeAgentTrialRetrievalModes.Matching;

    public IReadOnlyList<long> RequestedMaterialIds { get; init; } = [];

    public IReadOnlyList<long> RequestedTrialIds { get; init; } = [];

    public bool HasMaterialIntent { get; init; }

    public bool HasSources => IncludeKnowledge || IncludeTrials || IncludeMaterials;
}

public static class SaveLifeAgentTrialRetrievalModes
{
    public const string NotRequested = "not_requested";
    public const string Discovery = "discovery";
    public const string Matching = "matching";

    internal static readonly HashSet<string> All = [NotRequested, Discovery, Matching];
}

public sealed class SaveLifeAgentRetrievalDiagnostics
{
    public string KnowledgeStatus { get; set; } = SaveLifeAgentRetrievalSourceStatuses.NotRequested;

    public string TrialStatus { get; set; } = SaveLifeAgentRetrievalSourceStatuses.NotRequested;

    public string MaterialStatus { get; set; } = SaveLifeAgentRetrievalSourceStatuses.NotRequested;

    public string TextEmbeddingStatus { get; set; } = "not_needed";

    public string? TextEmbeddingError { get; set; }

    public string MaterialEmbeddingStatus { get; set; } = "not_needed";

    public string? MaterialEmbeddingError { get; set; }

    public int KnowledgeCandidateCount { get; set; }

    public int TrialCandidateCount { get; set; }

    public int MaterialCandidateCount { get; set; }

    public int KnowledgeHitCount { get; set; }

    public int TrialHitCount { get; set; }

    public int MaterialHitCount { get; set; }

    public string? VectorCompatibilityErrorCode { get; set; }

    public string? VectorCompatibilityModel { get; set; }

    public int? VectorCompatibilityExpectedDimensions { get; set; }

    public int? VectorCompatibilityActualDimensions { get; set; }

    internal void RecordVectorCompatibilityFailure(SaveLifeAgentVectorCompatibilityException error)
    {
        VectorCompatibilityErrorCode = error.ErrorCode;
        VectorCompatibilityModel = error.Model;
        VectorCompatibilityExpectedDimensions = error.ExpectedDimensions;
        VectorCompatibilityActualDimensions = error.ActualDimensions;
        MaterialEmbeddingStatus = SaveLifeAgentRetrievalSourceStatuses.Failed;
        MaterialEmbeddingError = error.ErrorCode;
    }

    internal void Complete(string source, int hitCount)
    {
        SetStatus(
            source,
            hitCount > 0
                ? SaveLifeAgentRetrievalSourceStatuses.Succeeded
                : SaveLifeAgentRetrievalSourceStatuses.NoHits);
    }

    internal void Fail(IEnumerable<string> sources)
    {
        foreach (var source in sources)
            SetStatus(source, SaveLifeAgentRetrievalSourceStatuses.Failed);
    }

    private void SetStatus(string source, string status)
    {
        switch (source)
        {
            case SaveLifeAgentRetrievalSources.Knowledge:
                KnowledgeStatus = status;
                break;
            case SaveLifeAgentRetrievalSources.Trial:
                TrialStatus = status;
                break;
            case SaveLifeAgentRetrievalSources.Material:
                MaterialStatus = status;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(source), source, "Unknown retrieval source.");
        }
    }
}

public static class SaveLifeAgentRetrievalSources
{
    public const string Knowledge = "knowledge";
    public const string Trial = "trial";
    public const string Material = "material";
}

public static class SaveLifeAgentRetrievalSourceStatuses
{
    public const string NotRequested = "not_requested";
    public const string Succeeded = "succeeded";
    public const string NoHits = "no_hits";
    public const string Failed = "failed";
}

internal sealed class SaveLifeAgentEmbeddingAttempt
{
    public float[]? Embedding { get; init; }

    public string Status { get; init; } = "not_needed";

    public static SaveLifeAgentEmbeddingAttempt NotNeeded()
    {
        return new SaveLifeAgentEmbeddingAttempt();
    }

    public static SaveLifeAgentEmbeddingAttempt Succeeded(float[] embedding)
    {
        return new SaveLifeAgentEmbeddingAttempt
        {
            Embedding = embedding,
            Status = "succeeded"
        };
    }
}

internal static class SaveLifeAgentRetrievalFailureBoundary
{
    public static async Task<T> ExecuteAsync<T>(
        Func<Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await operation();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (SaveLifeAgentUnavailableException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw CreateUnavailableException(ex);
        }
    }

    public static float[] RequireEmbedding(float[]? embedding, string source)
    {
        if (embedding is { Length: > 0 })
            return embedding;

        throw CreateUnavailableException(
            new InvalidOperationException($"Required {source} retrieval embedding was empty."));
    }

    public static async Task<T> ExecuteForSourcesAsync<T>(
        Func<Task<T>> operation,
        CancellationToken cancellationToken,
        SaveLifeAgentRetrievalDiagnostics diagnostics,
        params string[] sources)
    {
        try
        {
            return await operation();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (SaveLifeAgentUnavailableException ex)
        {
            diagnostics.Fail(sources);
            if (ex.RetrievalDiagnostics != null)
                throw;

            throw new SaveLifeAgentUnavailableException(
                ex.Code,
                ex.Stage,
                ex.InnerException ?? ex,
                ex.Retryable,
                diagnostics,
                ex.Message);
        }
        catch (Exception ex)
        {
            diagnostics.Fail(sources);
            throw CreateUnavailableException(ex, diagnostics);
        }
    }

    private static SaveLifeAgentUnavailableException CreateUnavailableException(
        Exception exception,
        SaveLifeAgentRetrievalDiagnostics? diagnostics = null)
    {
        if (exception is SaveLifeAgentVectorCompatibilityException vectorError)
        {
            diagnostics?.RecordVectorCompatibilityFailure(vectorError);
            return new SaveLifeAgentUnavailableException(
                SaveLifeAgentFailureCodes.VectorIncompatible,
                SaveLifeAgentFailureStages.Retrieval,
                vectorError,
                retrievalDiagnostics: diagnostics);
        }

        if (exception is SaveLifeMaterialEmbeddingDimensionException dimensionError)
        {
            var dimensionVectorError = new SaveLifeAgentVectorCompatibilityException(
                "dimension_mismatch",
                dimensionError.ExpectedDimensions,
                dimensionError.ActualDimensions,
                dimensionError.ConfiguredModel);
            diagnostics?.RecordVectorCompatibilityFailure(dimensionVectorError);
            return new SaveLifeAgentUnavailableException(
                SaveLifeAgentFailureCodes.VectorIncompatible,
                SaveLifeAgentFailureStages.Retrieval,
                dimensionError,
                retrievalDiagnostics: diagnostics);
        }

        return new SaveLifeAgentUnavailableException(
            SaveLifeAgentFailureCodes.RetrievalUnavailable,
            SaveLifeAgentFailureStages.Retrieval,
            exception,
            retrievalDiagnostics: diagnostics);
    }
}

public sealed class SaveLifeAgentKnowledgeReference
{
    public string Id { get; set; } = string.Empty;

    public string? Title { get; set; }

    public string? DiseaseName { get; set; }

    public string? Summary { get; set; }

    public string? Content { get; set; }

    public string? Tags { get; set; }

    public double Score { get; set; }
}

public sealed class SaveLifeAgentTrialReference
{
    public string Id { get; set; } = string.Empty;

    public string? Title { get; set; }

    public string? DiseaseArea { get; set; }

    public string? Indication { get; set; }

    public string? ClinicalPhase { get; set; }

    public string? Sponsor { get; set; }

    public string? Location { get; set; }

    public string? Status { get; set; }

    public string? Requirements { get; set; }

    public string? Description { get; set; }

    public string? Benefits { get; set; }

    public string? InclusionCriteria { get; set; }

    public string? ExclusionCriteria { get; set; }

    public string? Sites { get; set; }

    public bool IsPinned { get; set; }

    public int SortOrder { get; set; } = 100;

    public double Score { get; set; }
}

public sealed class SaveLifeAgentMaterialReference
{
    public string Id { get; set; } = string.Empty;

    public string MaterialId { get; set; } = string.Empty;

    public string? EnrollmentRecordId { get; set; }

    public bool IsPersonalMaterial { get; set; }

    public string? Title { get; set; }

    public string? MaterialType { get; set; }

    public string? MaterialTypeText { get; set; }

    public string? FileName { get; set; }

    public string? Status { get; set; }

    public string? StatusText { get; set; }

    public bool CanSatisfyRequiredMaterial { get; set; }

    public bool RequiresSupplement { get; set; }

    public string? Summary { get; set; }

    public double Score { get; set; }
}
