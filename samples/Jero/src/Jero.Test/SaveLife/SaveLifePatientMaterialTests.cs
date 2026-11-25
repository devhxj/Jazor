using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using OpenAI.Chat;
using OpenAI.Embeddings;
using SqlSugar;
using Jero.Core;
using Jero.Plugin.SaveLife;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifePatientMaterialTests
{
    [Theory]
    [InlineData(nameof(SaveLifeEnrollmentMaterialService.MiniAppRetry), "/api/v1/save-life/enrollments/{enrollmentId}/materials/{materialId}/retry")]
    [InlineData(nameof(SaveLifeEnrollmentMaterialService.MiniAppPersonalRetry), "/api/v1/save-life/materials/{materialId}/retry")]
    public void MaterialRetryEndpoints_ExposePatientScopedMiniAppRoutes(string methodName, string expectedRoute)
    {
        var method = typeof(SaveLifeEnrollmentMaterialService).GetMethod(methodName);
        var route = method?.GetCustomAttribute<HttpPostAttribute>();

        Assert.NotNull(method);
        Assert.Equal(expectedRoute, route?.Template);
    }

    [Fact]
    public void BuildRetrievalPlan_RequiresConfirmedPatientScope()
    {
        var query = SaveLifeAgentRetrievalQuery.Build(
            "我上传的报告处理好了吗",
            new SaveLifeAgentPatientSignal());

        var withoutPatient = SaveLifeAgentMaterialRetrievalPolicy.Build(
            query,
            "openid-1",
            enrollmentRecordId: null,
            patientProfileId: null);
        var withPatient = SaveLifeAgentMaterialRetrievalPolicy.Build(
            query,
            "openid-1",
            enrollmentRecordId: null,
            patientProfileId: 501,
            hasMaterialIntent: true);

        Assert.False(withoutPatient.CanSearch);
        Assert.True(withPatient.CanSearch);
        Assert.Equal(501, withPatient.PatientProfileId);
    }

    [Fact]
    public void MaterialApiContracts_ExposeOptionalPatientScopeAndOutputOwnership()
    {
        Assert.NotNull(typeof(UploadSaveLifeMiniAppEnrollmentMaterialInput)
            .GetProperty(nameof(UploadSaveLifeMiniAppEnrollmentMaterialInput.PatientProfileId)));
        Assert.NotNull(typeof(SaveLifeEnrollmentMaterialOutput)
            .GetProperty(nameof(SaveLifeEnrollmentMaterialOutput.PatientProfileId)));

        var output = SaveLifeEnrollmentMaterialService.ToOutput(
            Material(1001, "openid-1", 501, 0),
            canDelete: true);
        Assert.Equal("501", output.PatientProfileId);
    }

    [Fact]
    public void PersonalMaterialEndpoints_DoNotAcceptPatientScope()
    {
        foreach (var methodName in new[]
        {
            nameof(SaveLifeEnrollmentMaterialService.MiniAppPersonalList),
            nameof(SaveLifeEnrollmentMaterialService.MiniAppPersonalUpload),
            nameof(SaveLifeEnrollmentMaterialService.MiniAppPersonalRetry),
            nameof(SaveLifeEnrollmentMaterialService.MiniAppPersonalDelete),
            nameof(SaveLifeEnrollmentMaterialService.MiniAppPersonalPreview)
        })
        {
            var method = typeof(SaveLifeEnrollmentMaterialService).GetMethod(methodName);
            Assert.NotNull(method);
            Assert.DoesNotContain(method!.GetParameters(), parameter =>
                string.Equals(parameter.Name, "patientProfileId", StringComparison.OrdinalIgnoreCase));
        }
    }

    [Fact]
    public void ResolvePersonalPatient_WithMissingRequest_UsesOwnedSelfOnly()
    {
        var self = Patient(501, "openid-1", SaveLifePatientRelationships.Self, "本人");
        var mother = Patient(502, "openid-1", SaveLifePatientRelationships.Parent, "母亲");

        var resolved = SaveLifePatientMaterialScopePolicy.ResolvePersonalPatient(
            [self, mother],
            "openid-1",
            requestedPatientProfileId: null);

        Assert.Same(self, resolved);
    }

    [Fact]
    public void ResolveCurrentAccountPatient_IgnoresLegacyRequestedPatient()
    {
        var self = Patient(501, "openid-1", SaveLifePatientRelationships.Self, "本人");
        var mother = Patient(502, "openid-1", SaveLifePatientRelationships.Parent, "母亲");

        var resolved = SaveLifePatientMaterialScopePolicy.ResolveCurrentAccountPatient(
            [self, mother],
            "openid-1");

        Assert.Same(self, resolved);
    }

    [Fact]
    public void ResolvePersonalPatient_WithExplicitOwnedPatient_AllowsOtherPatientOnlyById()
    {
        var self = Patient(501, "openid-1", SaveLifePatientRelationships.Self, "本人");
        var mother = Patient(502, "openid-1", SaveLifePatientRelationships.Parent, "母亲");
        var foreign = Patient(601, "openid-2", SaveLifePatientRelationships.Parent, "母亲");

        var resolved = SaveLifePatientMaterialScopePolicy.ResolvePersonalPatient(
            [self, mother, foreign],
            "openid-1",
            mother.Id.ToString());

        Assert.Same(mother, resolved);
        Assert.ThrowsAny<Exception>(() => SaveLifePatientMaterialScopePolicy.ResolvePersonalPatient(
            [self, mother, foreign],
            "openid-1",
            foreign.Id.ToString()));
    }

    [Fact]
    public void ResolveEnrollmentPatient_UsesRecordPatientAndFailsClosedForLegacyOtherPatient()
    {
        var self = Patient(501, "openid-1", SaveLifePatientRelationships.Self, "本人");
        var mother = Patient(502, "openid-1", SaveLifePatientRelationships.Parent, "母亲");

        Assert.Same(mother, SaveLifePatientMaterialScopePolicy.ResolveEnrollmentPatient(
            [self, mother],
            "openid-1",
            enrollmentPatientProfileId: mother.Id,
            requestedPatientProfileId: null));
        Assert.ThrowsAny<Exception>(() => SaveLifePatientMaterialScopePolicy.ResolveEnrollmentPatient(
            [self, mother],
            "openid-1",
            enrollmentPatientProfileId: mother.Id,
            requestedPatientProfileId: self.Id.ToString()));
        Assert.ThrowsAny<Exception>(() => SaveLifePatientMaterialScopePolicy.ResolveEnrollmentPatient(
            [self, mother],
            "openid-1",
            enrollmentPatientProfileId: null,
            requestedPatientProfileId: mother.Id.ToString()));
    }

    [Fact]
    public void IsInScope_RequiresMatchingAccountPatientAndEnrollment()
    {
        var plan = new SaveLifeAgentMaterialRetrievalPlan
        {
            CanSearch = true,
            ExternalUserId = "openid-1",
            PatientProfileId = 501,
            EnrollmentRecordId = 3001
        };

        Assert.True(SaveLifeAgentMaterialRetrievalPolicy.IsInScope(Material(1001, "openid-1", 501, 0), plan));
        Assert.True(SaveLifeAgentMaterialRetrievalPolicy.IsInScope(Material(1002, "openid-1", 501, 3001), plan));
        Assert.False(SaveLifeAgentMaterialRetrievalPolicy.IsInScope(Material(1003, "openid-1", 502, 3001), plan));
        Assert.False(SaveLifeAgentMaterialRetrievalPolicy.IsInScope(Material(1004, "openid-2", 501, 3001), plan));
        Assert.False(SaveLifeAgentMaterialRetrievalPolicy.IsInScope(Material(1005, "openid-1", 501, 3002), plan));
    }

    [Theory]
    [InlineData("synced", "synced", SaveLifeAgentMaterialReadinessStatuses.Ready)]
    [InlineData("pending", "pending", SaveLifeAgentMaterialReadinessStatuses.Processing)]
    [InlineData("processing", "pending", SaveLifeAgentMaterialReadinessStatuses.Processing)]
    [InlineData("failed", "failed", SaveLifeAgentMaterialReadinessStatuses.Failed)]
    [InlineData("skipped", "skipped", SaveLifeAgentMaterialReadinessStatuses.Failed)]
    public void GetReadinessStatus_UsesExtractionAndIndexState(
        string extractStatus,
        string indexStatus,
        string expected)
    {
        var material = Material(1001, "openid-1", 501, 0);
        material.ExtractStatus = extractStatus;
        material.IndexStatus = indexStatus;

        var status = SaveLifeAgentMaterialRetrievalPolicy.GetReadinessStatus(material);

        Assert.Equal(expected, status);
    }

    [Theory]
    [InlineData("synced", true)]
    [InlineData("skipped", false)]
    [InlineData("failed", false)]
    [InlineData("pending", false)]
    public void CanIndexExtraction_RequiresSuccessfullyParsedContent(
        string extractionStatus,
        bool expected)
    {
        var extraction = new SaveLifeMaterialTextExtractionResult
        {
            Status = extractionStatus
        };

        Assert.Equal(expected, SaveLifeMaterialIndexProcessor.CanIndexExtraction(extraction));
    }

    [Fact]
    public void HasPrivatePatientScope_RequiresAccountAndPatientOwnership()
    {
        Assert.True(SaveLifeMaterialIndexProcessor.HasPrivatePatientScope(
            Material(1001, "openid-1", 501, 0)));

        var missingPatient = Material(1002, "openid-1", 501, 0);
        missingPatient.PatientProfileId = null;
        Assert.False(SaveLifeMaterialIndexProcessor.HasPrivatePatientScope(missingPatient));

        var missingOwner = Material(1003, "openid-1", 501, 0);
        missingOwner.ExternalUserId = string.Empty;
        Assert.False(SaveLifeMaterialIndexProcessor.HasPrivatePatientScope(missingOwner));
    }

    [Fact]
    public async Task SyncMaterialIndexAsync_PersistsPatientScopeOnPrivateIndex()
    {
        await WithDatabase(async db =>
        {
            var textEmbeddingService = new FixedEmbeddingService();
            var service = CreateRetrievalService(db, textEmbeddingService);
            var material = Material(1001, "openid-1", 501, 0);
            material.ExtractedText = "肺癌病理检查结果";
            material.ContentSha256 = new string('A', 64);

            await service.SyncMaterialIndexAsync(material);

            var index = db.Queryable<SaveLifeAgentVectorIndex>().Single();
            Assert.Equal(material.Id, index.ResourceId);
            Assert.Equal(material.ExternalUserId, index.ExternalUserId);
            Assert.Equal(material.PatientProfileId, index.PatientProfileId);
            Assert.Equal("synced", index.SyncStatus);
            Assert.Equal("multimodal", index.EmbeddingKind);
            Assert.Equal("test-material-embedding", index.EmbeddingModel);
            Assert.Equal("configured-material-embedding", index.EmbeddingConfiguredModel);
            Assert.Equal(2, index.EmbeddingDimensions);
            Assert.Equal(material.ContentSha256, index.SourceContentSha256);
            Assert.Equal(0, textEmbeddingService.GenerateEmbeddingCallCount);
        });
    }

    [Fact]
    public async Task SyncMaterialIndexAsync_WhenMaterialIsDeletedDuringEmbedding_DiscardsLateVector()
    {
        await WithDatabase(async db =>
        {
            var embedding = new BlockingMaterialEmbeddingService();
            var service = CreateRetrievalService(db, materialEmbeddingService: embedding);
            var material = Material(1001, "openid-1", 501, 0);
            material.FileId = 701;
            material.ContentSha256 = new string('A', 64);
            material.ProcessingRevision = 1;
            material.ProcessingLeaseOwner = "lease-a";
            material.ProcessingLeaseExpiresTime = DateTime.Now.AddMinutes(5);
            db.Insertable(material).ExecuteCommand();

            var processing = service.SyncMaterialIndexAsync(material);
            await embedding.Started.Task.WaitAsync(TestContext.Current.CancellationToken);
            await db.Updateable<SaveLifeEnrollmentMaterial>()
                .SetColumns(item => new SaveLifeEnrollmentMaterial
                {
                    IsDelete = true,
                    DeleteTime = DateTime.Now,
                    ProcessingRevision = item.ProcessingRevision + 1,
                    ProcessingLeaseOwner = null,
                    ProcessingLeaseExpiresTime = null
                })
                .Where(item => item.Id == material.Id)
                .ExecuteCommandAsync(TestContext.Current.CancellationToken);
            embedding.Release.TrySetResult();

            await Assert.ThrowsAsync<SaveLifeMaterialProcessingSupersededException>(() => processing);
            Assert.Empty(db.Queryable<SaveLifeAgentVectorIndex>().ClearFilter().ToList());
        });
    }

    [Fact]
    public async Task SyncMaterialIndexAsync_WithCurrentProcessingLease_PublishesIndexTransactionally()
    {
        await WithDatabase(async db =>
        {
            var service = CreateRetrievalService(db);
            var material = Material(1001, "openid-1", 501, 0);
            material.FileId = 701;
            material.ContentSha256 = new string('A', 64);
            material.ProcessingRevision = 1;
            material.ProcessingLeaseOwner = "lease-a";
            material.ProcessingLeaseExpiresTime = DateTime.Now.AddMinutes(5);
            db.Insertable(material).ExecuteCommand();

            await service.SyncMaterialIndexAsync(material);

            var index = db.Queryable<SaveLifeAgentVectorIndex>().Single();
            Assert.Equal(material.Id, index.ResourceId);
            Assert.Equal("synced", index.SyncStatus);
        });
    }

    [Fact]
    public async Task BuildRetrievalAsync_WithSameAccountPatients_LoadsOnlyCurrentPatientMaterial()
    {
        await WithDatabase(async db =>
        {
            var current = Material(1001, "openid-1", 501, 0);
            current.ExtractedText = "当前患者肺癌病理报告";
            var other = Material(1002, "openid-1", 502, 0);
            other.ExtractedText = "另一位患者乳腺癌病理报告";
            db.Insertable(new[] { current, other }).ExecuteCommand();
            db.Insertable(new[]
            {
                Index(2001, current, "当前患者肺癌病理报告"),
                Index(2002, other, "另一位患者乳腺癌病理报告")
            }).ExecuteCommand();

            var result = await CreateRetrievalService(db).BuildRetrievalAsync(
                "请看我上传的病理报告",
                PatientSignal(501),
                "openid-1",
                cancellationToken: TestContext.Current.CancellationToken,
                scope: new SaveLifeAgentRetrievalScope(false, false, true) { HasMaterialIntent = true });

            Assert.Equal(1, result.Diagnostics.MaterialCandidateCount);
            Assert.Equal("1001", Assert.Single(result.Materials).Id);
        });
    }

    [Fact]
    public async Task BuildRetrievalAsync_WithLegacyTextMaterialIndex_FailsClosed()
    {
        await WithDatabase(async db =>
        {
            var material = Material(1001, "openid-1", 501, 0);
            material.ExtractedText = "肺癌病理检查结果";
            db.Insertable(material).ExecuteCommand();
            var legacyIndex = Index(2001, material, material.ExtractedText);
            legacyIndex.EmbeddingKind = "text";
            legacyIndex.EmbeddingModel = "BAAI/bge-m3";
            db.Insertable(legacyIndex).ExecuteCommand();

            var error = await Assert.ThrowsAsync<SaveLifeAgentUnavailableException>(() =>
                CreateRetrievalService(db).BuildRetrievalAsync(
                    "请看我上传的病理报告",
                    PatientSignal(501),
                    "openid-1",
                    cancellationToken: TestContext.Current.CancellationToken,
                    scope: new SaveLifeAgentRetrievalScope(false, false, true) { HasMaterialIntent = true }));

            Assert.Equal(SaveLifeAgentFailureCodes.MaterialNotReady, error.Code);
            Assert.Equal(SaveLifeAgentFailureStages.Material, error.Stage);
        });
    }

    [Fact]
    public async Task BuildRetrievalAsync_WithWrongMaterialDimensions_FailsClosed()
    {
        await WithDatabase(async db =>
        {
            var material = Material(1001, "openid-1", 501, 0);
            material.ExtractedText = "肺癌病理检查结果";
            db.Insertable(material).ExecuteCommand();
            var wrongDimensionIndex = Index(2001, material, material.ExtractedText);
            wrongDimensionIndex.EmbeddingDimensions = 3;
            db.Insertable(wrongDimensionIndex).ExecuteCommand();

            var error = await Assert.ThrowsAsync<SaveLifeAgentUnavailableException>(() =>
                CreateRetrievalService(db).BuildRetrievalAsync(
                    "请看我上传的病理报告",
                    PatientSignal(501),
                    "openid-1",
                    cancellationToken: TestContext.Current.CancellationToken,
                    scope: new SaveLifeAgentRetrievalScope(false, false, true) { HasMaterialIntent = true }));

            Assert.Equal(SaveLifeAgentFailureCodes.MaterialNotReady, error.Code);
            Assert.Equal(SaveLifeAgentFailureStages.Material, error.Stage);
        });
    }

    [Fact]
    public async Task BuildRetrievalAsync_WithCorruptMaterialVector_ReturnsTypedCompatibilityFailure()
    {
        await WithDatabase(async db =>
        {
            var material = Material(1001, "openid-1", 501, 0);
            material.ExtractedText = "肺癌病理检查结果";
            db.Insertable(material).ExecuteCommand();
            var corruptIndex = Index(2001, material, material.ExtractedText);
            corruptIndex.EmbeddingJson = "not-json";
            db.Insertable(corruptIndex).ExecuteCommand();

            var error = await Assert.ThrowsAsync<SaveLifeAgentUnavailableException>(() =>
                CreateRetrievalService(db).BuildRetrievalAsync(
                    "请看我上传的病理报告",
                    PatientSignal(501),
                    "openid-1",
                    cancellationToken: TestContext.Current.CancellationToken,
                    scope: new SaveLifeAgentRetrievalScope(false, false, true) { HasMaterialIntent = true }));

            Assert.Equal(SaveLifeAgentFailureCodes.VectorIncompatible, error.Code);
            Assert.Equal(SaveLifeAgentFailureStages.Retrieval, error.Stage);
            Assert.NotNull(error.RetrievalDiagnostics);
            Assert.Equal("invalid_json", error.RetrievalDiagnostics!.VectorCompatibilityErrorCode);
            Assert.Equal("doubao-embedding-vision", error.RetrievalDiagnostics.VectorCompatibilityModel);
            Assert.Equal(2, error.RetrievalDiagnostics.VectorCompatibilityExpectedDimensions);
            Assert.Equal(1, error.RetrievalDiagnostics.MaterialCandidateCount);
            Assert.DoesNotContain("not-json", error.RetrievalDiagnostics.MaterialEmbeddingError ?? string.Empty);
        });
    }

    [Fact]
    public async Task BuildRetrievalAsync_WithCurrentPatientMaterialProcessing_ReturnsProcessingState()
    {
        await WithDatabase(async db =>
        {
            var material = Material(1001, "openid-1", 501, 0);
            material.ExtractStatus = "processing";
            material.IndexStatus = "pending";
            db.Insertable(material).ExecuteCommand();

            var error = await Assert.ThrowsAsync<SaveLifeAgentUnavailableException>(() =>
                CreateRetrievalService(db).BuildRetrievalAsync(
                    "请看我刚上传的报告",
                    PatientSignal(501),
                    "openid-1",
                    cancellationToken: TestContext.Current.CancellationToken,
                    scope: new SaveLifeAgentRetrievalScope(false, false, true) { HasMaterialIntent = true }));

            Assert.Equal(SaveLifeAgentFailureCodes.MaterialNotReady, error.Code);
            Assert.Equal(SaveLifeAgentFailureStages.Material, error.Stage);
            Assert.True(error.Retryable);
            Assert.Contains("处理中", error.Message);
        });
    }

    [Fact]
    public async Task BuildRetrievalAsync_WithRequestedMaterial_IgnoresUnrelatedFailedMaterial()
    {
        await WithDatabase(async db =>
        {
            var requested = Material(1001, "openid-1", 501, 0);
            requested.ExtractedText = "本轮选择的病例资料";
            var unrelated = Material(1002, "openid-1", 501, 0);
            unrelated.ExtractStatus = "failed";
            unrelated.IndexStatus = "failed";
            db.Insertable(new[] { requested, unrelated }).ExecuteCommand();
            db.Insertable(Index(2001, requested, requested.ExtractedText)).ExecuteCommand();

            var result = await CreateRetrievalService(db).BuildRetrievalAsync(
                "请解读我刚上传的资料",
                PatientSignal(501),
                "openid-1",
                cancellationToken: TestContext.Current.CancellationToken,
                scope: new SaveLifeAgentRetrievalScope(false, false, true)
                {
                    HasMaterialIntent = true,
                    RequestedMaterialIds = [requested.Id]
                });

            Assert.Equal("1001", Assert.Single(result.Materials).Id);
        });
    }

    [Fact]
    public async Task BuildRetrievalAsync_WithCurrentPatientMaterialFailed_ReturnsRetryableFailure()
    {
        await WithDatabase(async db =>
        {
            var material = Material(1001, "openid-1", 501, 0);
            material.ExtractStatus = "failed";
            material.IndexStatus = "failed";
            db.Insertable(material).ExecuteCommand();

            var error = await Assert.ThrowsAsync<SaveLifeAgentUnavailableException>(() =>
                CreateRetrievalService(db).BuildRetrievalAsync(
                    "请看我上传失败的报告",
                    PatientSignal(501),
                    "openid-1",
                    cancellationToken: TestContext.Current.CancellationToken,
                    scope: new SaveLifeAgentRetrievalScope(false, false, true) { HasMaterialIntent = true }));

            Assert.Equal(SaveLifeAgentFailureCodes.MaterialNotReady, error.Code);
            Assert.Equal(SaveLifeAgentFailureStages.Material, error.Stage);
            Assert.True(error.Retryable);
            Assert.Contains("失败", error.Message);
        });
    }

    [Fact]
    public async Task BuildRetrievalAsync_WithMissingSyncedIndex_ReturnsRetryableFailure()
    {
        await WithDatabase(async db =>
        {
            var material = Material(1001, "openid-1", 501, 0);
            material.ExtractedText = "肺癌病理报告";
            db.Insertable(material).ExecuteCommand();

            var error = await Assert.ThrowsAsync<SaveLifeAgentUnavailableException>(() =>
                CreateRetrievalService(db).BuildRetrievalAsync(
                    "请看我上传的报告",
                    PatientSignal(501),
                    "openid-1",
                    cancellationToken: TestContext.Current.CancellationToken,
                    scope: new SaveLifeAgentRetrievalScope(false, false, true) { HasMaterialIntent = true }));

            Assert.Equal(SaveLifeAgentFailureCodes.MaterialNotReady, error.Code);
            Assert.Equal(SaveLifeAgentFailureStages.Material, error.Stage);
            Assert.Contains("失败", error.Message);
        });
    }

    private static SaveLifeEnrollmentMaterial Material(
        long id,
        string owner,
        long patientProfileId,
        long enrollmentRecordId)
    {
        return new SaveLifeEnrollmentMaterial
        {
            Id = id,
            ExternalUserId = owner,
            PatientProfileId = patientProfileId,
            EnrollmentRecordId = enrollmentRecordId,
            MaterialType = "diagnosis_report",
            FileName = $"报告-{id}",
            FileSuffix = ".pdf",
            ContentType = "application/pdf",
            Status = "submitted",
            ExtractStatus = "synced",
            IndexStatus = "synced",
            ContentSha256 = new string('A', 64),
            CreateTime = new DateTime(2026, 7, 14)
        };
    }

    private static SaveLifePatientProfile Patient(
        long id,
        string owner,
        string relationship,
        string relationshipDescription)
    {
        return new SaveLifePatientProfile
        {
            Id = id,
            OwnerExternalUserId = owner,
            AccountPatientKey = relationship == SaveLifePatientRelationships.Self
                ? SaveLifePatientProfileKeys.ForSelf(owner)
                : SaveLifePatientProfileKeys.ForOther(id),
            RelationshipToAccount = relationship,
            RelationshipDescription = relationshipDescription,
            DisplayName = relationshipDescription,
            IsDefault = relationship == SaveLifePatientRelationships.Self,
            CreateTime = new DateTime(2026, 7, 14)
        };
    }

    private static SaveLifeAgentVectorIndex Index(
        long id,
        SaveLifeEnrollmentMaterial material,
        string content)
    {
        return new SaveLifeAgentVectorIndex
        {
            Id = id,
            PatientProfileId = material.PatientProfileId,
            ResourceType = "material",
            ResourceId = material.Id,
            ExternalUserId = material.ExternalUserId,
            Title = material.FileName,
            Content = content,
            EmbeddingJson = "[1,0]",
            EmbeddingModel = "test-material-embedding",
            EmbeddingKind = "multimodal",
            EmbeddingConfiguredModel = "doubao-embedding-vision",
            EmbeddingDimensions = 2,
            SourceContentSha256 = material.ContentSha256,
            SyncStatus = "synced",
            CreateTime = new DateTime(2026, 7, 14)
        };
    }

    private static SaveLifeAgentPatientSignal PatientSignal(long patientProfileId)
    {
        return new SaveLifeAgentPatientSignal
        {
            CurrentPatient = new SaveLifePatientProfileOutput
            {
                Id = patientProfileId.ToString()
            }
        };
    }

    private static SaveLifeAgentRetrievalService CreateRetrievalService(
        ISqlSugarClient db,
        ISaveLifeAgentEmbeddingService? embeddingService = null,
        ISaveLifeMaterialEmbeddingService? materialEmbeddingService = null)
    {
        return new SaveLifeAgentRetrievalService(
            SqlSugarRepository<SaveLifeAgentVectorIndex>.CreateForContext(db),
            SqlSugarRepository<SaveLifeDiseaseKnowledge>.CreateForContext(db),
            SqlSugarRepository<SaveLifeClinicalTrial>.CreateForContext(db),
            SqlSugarRepository<SaveLifeEnrollmentMaterial>.CreateForContext(db),
            new FixedClientFactory(),
            embeddingService ?? new FixedEmbeddingService(),
            materialEmbeddingService ?? new FixedMaterialEmbeddingService());
    }

    private static async Task WithDatabase(Func<ISqlSugarClient, Task> test)
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = new SqlSugarClient(new ConnectionConfig
            {
                ConnectionString = $"Data Source={dbPath};Pooling=False",
                DbType = DbType.Sqlite,
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute,
                ConfigureExternalServices = new ConfigureExternalServices
                {
                    EntityService = (property, column) =>
                    {
                        if (new NullabilityInfoContext().Create(property).WriteState is NullabilityState.Nullable)
                            column.IsNullable = true;
                    }
                }
            });
            db.CodeFirst.InitTables(
                typeof(SaveLifeAgentVectorIndex),
                typeof(SaveLifeEnrollmentMaterial));
            await test(db);
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    private sealed class FixedClientFactory : ISaveLifeAgentClientFactory
    {
        public Task<SaveLifeAgentOptions> GetOptionsAsync() => Task.FromResult(new SaveLifeAgentOptions
        {
            MaterialEmbeddingDimensions = 2
        });

        public Task<bool> CanUseChatAsync() => Task.FromResult(false);

        public Task<bool> CanUseEmbeddingsAsync() => Task.FromResult(true);

        public Task<ChatClient> CreateChatClientAsync() => throw new NotSupportedException();

        public Task<EmbeddingClient> CreateEmbeddingClientAsync() => throw new NotSupportedException();
    }

    private sealed class FixedEmbeddingService : ISaveLifeAgentEmbeddingService
    {
        public int GenerateEmbeddingCallCount { get; private set; }

        public Task<float[]?> GenerateEmbeddingAsync(string text)
        {
            GenerateEmbeddingCallCount++;
            return Task.FromResult<float[]?>([1F, 0F]);
        }

        public string ComputeContentHash(string text) => "hash";

        public double CosineSimilarity(IReadOnlyList<float> left, IReadOnlyList<float> right) => 1D;

        public string SerializeVector(IReadOnlyList<float> vector) => "[1,0]";

        public float[] DeserializeVector(string? json) => [1F, 0F];
    }

    private sealed class FixedMaterialEmbeddingService : ISaveLifeMaterialEmbeddingService
    {
        public Task<SaveLifeMaterialEmbeddingResult?> GenerateQueryEmbeddingAsync(string text) =>
            Task.FromResult<SaveLifeMaterialEmbeddingResult?>(new SaveLifeMaterialEmbeddingResult
            {
                Embedding = [1F, 0F],
                Model = "test-material-embedding",
                ConfiguredModel = "configured-material-embedding",
                Dimensions = 2
            });

        public Task<SaveLifeMaterialEmbeddingResult?> GenerateMaterialEmbeddingAsync(
            SaveLifeEnrollmentMaterial material,
            Func<Task<Stream>> openFileStream) =>
            Task.FromResult<SaveLifeMaterialEmbeddingResult?>(new SaveLifeMaterialEmbeddingResult
            {
                Embedding = [1F, 0F],
                Model = "test-material-embedding",
                ConfiguredModel = "configured-material-embedding",
                Dimensions = 2
            });
    }

    private sealed class BlockingMaterialEmbeddingService : ISaveLifeMaterialEmbeddingService
    {
        public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task<SaveLifeMaterialEmbeddingResult?> GenerateQueryEmbeddingAsync(string text) =>
            Task.FromResult<SaveLifeMaterialEmbeddingResult?>(new SaveLifeMaterialEmbeddingResult
            {
                Embedding = [1F, 0F],
                Model = "test-material-embedding"
            });

        public async Task<SaveLifeMaterialEmbeddingResult?> GenerateMaterialEmbeddingAsync(
            SaveLifeEnrollmentMaterial material,
            Func<Task<Stream>> openFileStream)
        {
            Started.TrySetResult();
            await Release.Task;
            return new SaveLifeMaterialEmbeddingResult
            {
                Embedding = [1F, 0F],
                Model = "test-material-embedding",
                ConfiguredModel = "configured-material-embedding",
                Dimensions = 2
            };
        }
    }
}
