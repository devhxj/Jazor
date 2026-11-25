using System.Reflection;
using System.Runtime.CompilerServices;
using SqlSugar;
using Jero.Core;
using Jero.Plugin.SaveLife;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifePatientEnrollmentTests
{
    [Fact]
    public void EnrollmentSubmissionEndpoints_DependOnSharedPatientEnrollmentService()
    {
        Assert.Contains(
            typeof(SaveLifeAgentService).GetConstructors().Single().GetParameters(),
            parameter => parameter.ParameterType == typeof(ISaveLifePatientEnrollmentService));
        Assert.Contains(
            typeof(SaveLifeEnrollmentRecordService).GetConstructors().Single().GetParameters(),
            parameter => parameter.ParameterType == typeof(ISaveLifePatientEnrollmentService));
    }

    [Fact]
    public void EnrollmentSubmissionEndpoints_ExposeSeparateTransactionalBoundary()
    {
        var sharedSubmission = typeof(ISaveLifePatientEnrollmentService)
            .GetMethod(nameof(ISaveLifePatientEnrollmentService.SubmitConfirmedAsync))!;
        var transactionalSubmission = typeof(ISaveLifePatientEnrollmentService)
            .GetMethod(nameof(ISaveLifePatientEnrollmentService.SubmitConfirmedInTransactionAsync))!;
        var agentSubmission = typeof(SaveLifeAgentService)
            .GetMethod(nameof(SaveLifeAgentService.SubmitIntention))!;
        var miniAppSubmission = typeof(SaveLifeEnrollmentRecordService)
            .GetMethod(nameof(SaveLifeEnrollmentRecordService.MiniAppSubmit))!;

        Assert.NotNull(transactionalSubmission);
        Assert.False(AsyncMethodCalls(agentSubmission, sharedSubmission));
        Assert.True(AsyncMethodCalls(miniAppSubmission, sharedSubmission));
    }

    [Fact]
    public void ConfirmedIntentionPath_DoesNotDependOnRetrieval()
    {
        var intentionSubmission = typeof(SaveLifeAgentService)
            .GetMethod(nameof(SaveLifeAgentService.SubmitIntention))!;
        var retrieval = typeof(ISaveLifeAgentRetrievalService)
            .GetMethod(nameof(ISaveLifeAgentRetrievalService.BuildRetrievalAsync))!;

        Assert.False(AsyncMethodCalls(intentionSubmission, retrieval));
    }

    [Fact]
    public void ResolveConfirmedTrialId_WithoutExplicitTrial_DoesNotUseRetrievedCandidate()
    {
        var matchedTrials = new List<SaveLifeAgentMatchedTrialOutput>
        {
            new() { Id = "901", Title = "检索到但未确认的项目" }
        };

        var retrievedCandidateId = SaveLifeAgentTrialSelection.ResolveCandidateTrialId(null, matchedTrials);
        var confirmedTrialId = SaveLifeAgentTrialSelection.ResolveConfirmedTrialId(inputTrialId: null);

        Assert.Equal(901, retrievedCandidateId);
        Assert.Null(confirmedTrialId);

    }

    [Fact]
    public void ResolveConversationPatientProfileId_WithChangedPatient_RejectsStaleConfirmation()
    {
        Assert.Equal(
            501,
            SaveLifePatientEnrollmentPolicy.ResolveConversationPatientProfileId(501, requestedPatientProfileId: null));
        Assert.ThrowsAny<Exception>(() =>
            SaveLifePatientEnrollmentPolicy.ResolveConversationPatientProfileId(502, requestedPatientProfileId: 501));
        Assert.ThrowsAny<Exception>(() =>
            SaveLifePatientEnrollmentPolicy.ResolveConversationPatientProfileId(null, requestedPatientProfileId: null));
    }

    [Theory]
    [InlineData("trial_required", "intention")]
    [InlineData("considering", "hesitate_intention")]
    [InlineData("draft", "draft_intention")]
    [InlineData("pending", "pending_intention")]
    public void ResolveConversationPhase_KeepsExistingClientPhaseContract(string status, string expectedPhase)
    {
        Assert.Equal(expectedPhase, SaveLifePatientEnrollmentPolicy.ResolveConversationPhase(status));
    }

    [Fact]
    public void EnrollmentContracts_ExposePatientOwnership()
    {
        Assert.NotNull(typeof(SubmitSaveLifeAgentIntentionInput).GetProperty("PatientProfileId"));
        Assert.NotNull(typeof(SubmitSaveLifeMiniAppEnrollmentInput).GetProperty("PatientProfileId"));
        Assert.NotNull(typeof(SaveLifeMiniAppEnrollmentOutput).GetProperty("PatientProfileId"));
    }

    [Fact]
    public async Task SubmitConfirmedAsync_WithoutSpecificProject_DoesNotCreateEnrollment()
    {
        using var database = new EnrollmentDatabase();
        database.AddAccount("openid-1");
        var patient = database.AddPatient(501, "openid-1", complete: true);

        await Assert.ThrowsAnyAsync<Exception>(() => database.Service.SubmitConfirmedAsync(
            CreateSubmission("openid-1", patient.Id, projectId: 0)));

        Assert.Empty(database.Enrollments());
    }

    [Theory]
    [InlineData("paused")]
    [InlineData("completed")]
    [InlineData("draft")]
    public async Task SubmitConfirmedAsync_WithNonRecruitingProject_DoesNotCreateEnrollment(string status)
    {
        using var database = new EnrollmentDatabase();
        database.AddAccount("openid-1");
        var patient = database.AddPatient(501, "openid-1", complete: true);
        database.AddTrial(901, status);

        await Assert.ThrowsAnyAsync<Exception>(() => database.Service.SubmitConfirmedAsync(
            CreateSubmission("openid-1", patient.Id, 901)));

        Assert.Empty(database.Enrollments());
    }

    [Fact]
    public async Task SubmitConfirmedAsync_WithOtherAccountPatient_DoesNotCreateEnrollment()
    {
        using var database = new EnrollmentDatabase();
        database.AddAccount("openid-1");
        var otherPatient = database.AddPatient(601, "openid-2", complete: true);
        database.AddTrial(901);

        await Assert.ThrowsAnyAsync<Exception>(() => database.Service.SubmitConfirmedAsync(
            CreateSubmission("openid-1", otherPatient.Id, 901)));

        Assert.Empty(database.Enrollments());
    }

    [Fact]
    public async Task SubmitConfirmedAsync_WithMissingPatient_DoesNotCreateEnrollment()
    {
        using var database = new EnrollmentDatabase();
        database.AddAccount("openid-1");
        database.AddTrial(901);

        await Assert.ThrowsAnyAsync<Exception>(() => database.Service.SubmitConfirmedAsync(
            CreateSubmission("openid-1", patientProfileId: null, 901)));

        Assert.Empty(database.Enrollments());
    }

    [Fact]
    public async Task ResolvePatientProfileIdAsync_WithoutRequestedPatient_UsesOwnedSelfPatientOnly()
    {
        using var database = new EnrollmentDatabase();
        database.AddAccount("openid-1");
        var self = database.AddPatient(501, "openid-1", complete: true);
        database.AddPatient(502, "openid-1", complete: true, "母亲");

        var resolved = await database.Service.ResolvePatientProfileIdAsync(
            "openid-1",
            requestedPatientProfileId: null,
            allowSelfFallback: true);

        Assert.Equal(self.Id, resolved);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ResolvePatientProfileIdAsync_WithInvalidRequestedPatient_DoesNotFallbackToSelf(
        long invalidPatientProfileId)
    {
        using var database = new EnrollmentDatabase();
        database.AddAccount("openid-1");
        database.AddPatient(501, "openid-1", complete: true);

        await Assert.ThrowsAnyAsync<Exception>(() => database.Service.ResolvePatientProfileIdAsync(
            "openid-1",
            invalidPatientProfileId,
            allowSelfFallback: true));
    }

    [Fact]
    public async Task SubmitConfirmedAsync_WithIncompletePatient_CreatesPatientDraft()
    {
        using var database = new EnrollmentDatabase();
        var account = database.AddAccount("openid-1");
        var patient = database.AddPatient(501, "openid-1", complete: false);
        database.AddTrial(901);

        var result = await database.Service.SubmitConfirmedAsync(
            CreateSubmission("openid-1", patient.Id, 901));

        Assert.Equal("draft", result.Enrollment.Status);
        Assert.Equal(patient.Id, result.Enrollment.PatientProfileId);
        Assert.Equal(account.Id, result.Enrollment.UserProfileId);
        Assert.False(result.Validation.CanSubmit);
        Assert.Single(database.Enrollments());
    }

    [Fact]
    public async Task SubmitConfirmedAsync_WithCompletePatient_CreatesPendingEnrollment()
    {
        using var database = new EnrollmentDatabase();
        var account = database.AddAccount("openid-1");
        var patient = database.AddPatient(501, "openid-1", complete: true);
        database.AddTrial(901);

        var result = await database.Service.SubmitConfirmedAsync(
            CreateSubmission("openid-1", patient.Id, 901));

        Assert.Equal("pending", result.Enrollment.Status);
        Assert.Equal(patient.Id, result.Enrollment.PatientProfileId);
        Assert.Equal(account.Id, result.Enrollment.UserProfileId);
        Assert.Equal("测试联系人", result.Enrollment.ContactName);
        Assert.Equal("13800138000", result.Enrollment.ContactPhone);
        Assert.Equal(patient.Diagnosis, result.Enrollment.DiseaseDescription);
        Assert.Equal(patient.TreatmentHistory, result.Enrollment.MedicalHistory);
        Assert.True(result.Validation.CanSubmit);
        Assert.Single(database.Enrollments());
    }

    [Fact]
    public async Task SubmitConfirmedAsync_ForSamePatientAndProject_ReusesSingleEnrollment()
    {
        using var database = new EnrollmentDatabase();
        database.AddAccount("openid-1");
        var patient = database.AddPatient(501, "openid-1", complete: false);
        database.AddTrial(901);

        var first = await database.Service.SubmitConfirmedAsync(
            CreateSubmission("openid-1", patient.Id, 901));
        patient.Gender = "female";
        patient.BirthDate = new DateTime(1960, 1, 1);
        patient.City = "成都";
        patient.Diagnosis = "肺癌";
        patient.TreatmentHistory = "已完成一线治疗";
        database.UpdatePatient(patient);

        var second = await database.Service.SubmitConfirmedAsync(
            CreateSubmission("openid-1", patient.Id, 901));

        Assert.Equal(first.Enrollment.Id, second.Enrollment.Id);
        Assert.Equal("pending", second.Enrollment.Status);
        Assert.Single(database.Enrollments());
    }

    [Fact]
    public async Task SubmitConfirmedAsync_ForDifferentPatients_AllowsSameProject()
    {
        using var database = new EnrollmentDatabase();
        database.AddAccount("openid-1");
        var self = database.AddPatient(501, "openid-1", complete: true);
        var mother = database.AddPatient(502, "openid-1", complete: true, "母亲");
        database.AddTrial(901);

        var selfEnrollment = await database.Service.SubmitConfirmedAsync(
            CreateSubmission("openid-1", self.Id, 901));
        var motherEnrollment = await database.Service.SubmitConfirmedAsync(
            CreateSubmission("openid-1", mother.Id, 901));

        Assert.NotEqual(selfEnrollment.Enrollment.Id, motherEnrollment.Enrollment.Id);
        Assert.Equal(
            new long?[] { self.Id, mother.Id },
            database.Enrollments().OrderBy(item => item.PatientProfileId).Select(item => item.PatientProfileId));
    }

    [Fact]
    public void ValidateForSubmission_UsesPatientHealthDataAndAccountContact()
    {
        var account = CreateAccount("openid-1");
        var patient = CreatePatient(501, "openid-1", complete: true);

        var complete = SaveLifeEnrollmentValidation.ValidatePatientForSubmission(
            account,
            patient,
            new SaveLifeEnrollmentSubmissionFields());
        patient.TreatmentHistory = null;
        patient.MedicalHistory = null;
        var incomplete = SaveLifeEnrollmentValidation.ValidatePatientForSubmission(
            account,
            patient,
            new SaveLifeEnrollmentSubmissionFields());

        Assert.True(complete.CanSubmit);
        Assert.False(incomplete.CanSubmit);
        Assert.Contains("患者治疗经过或既往病史", incomplete.MissingFields);
    }

    private static SaveLifePatientEnrollmentSubmissionInput CreateSubmission(
        string externalUserId,
        long? patientProfileId,
        long projectId)
    {
        return new SaveLifePatientEnrollmentSubmissionInput
        {
            ExternalUserId = externalUserId,
            PatientProfileId = patientProfileId,
            ProjectId = projectId,
            Fields = new SaveLifeEnrollmentSubmissionFields()
        };
    }

    private static bool AsyncMethodCalls(MethodInfo asyncMethod, MethodInfo calledMethod)
    {
        var stateMachine = asyncMethod.GetCustomAttribute<AsyncStateMachineAttribute>()?.StateMachineType;
        var moveNext = stateMachine?.GetMethod("MoveNext", BindingFlags.Instance | BindingFlags.NonPublic);
        var il = moveNext?.GetMethodBody()?.GetILAsByteArray();
        return il != null && il.AsSpan().IndexOf(BitConverter.GetBytes(calledMethod.MetadataToken)) >= 0;
    }

    private static SaveLifeUserProfile CreateAccount(string externalUserId)
    {
        return new SaveLifeUserProfile
        {
            Id = 101,
            ExternalUserId = externalUserId,
            RealName = "测试联系人",
            Phone = "13800138000",
            CreateTime = new DateTime(2026, 7, 14)
        };
    }

    private static SaveLifePatientProfile CreatePatient(
        long id,
        string ownerExternalUserId,
        bool complete,
        string relationshipDescription = "本人")
    {
        var isSelf = relationshipDescription == "本人";
        return new SaveLifePatientProfile
        {
            Id = id,
            OwnerExternalUserId = ownerExternalUserId,
            AccountPatientKey = isSelf
                ? SaveLifePatientProfileKeys.ForSelf(ownerExternalUserId)
                : SaveLifePatientProfileKeys.ForOther(id),
            RelationshipToAccount = isSelf
                ? SaveLifePatientRelationships.Self
                : SaveLifePatientRelationships.Parent,
            RelationshipDescription = relationshipDescription,
            DisplayName = relationshipDescription,
            IsDefault = isSelf,
            Gender = complete ? "female" : null,
            BirthDate = complete ? new DateTime(1960, 1, 1) : null,
            City = complete ? "成都" : null,
            Diagnosis = complete ? "肺癌" : null,
            TreatmentHistory = complete ? "已完成一线治疗" : null,
            CreateTime = new DateTime(2026, 7, 14)
        };
    }

    private sealed class EnrollmentDatabase : IDisposable
    {
        private readonly string _dbPath;
        private readonly SqlSugarClient _db;

        public EnrollmentDatabase()
        {
            _dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
            _db = new SqlSugarClient(new ConnectionConfig
            {
                ConnectionString = $"Data Source={_dbPath};Pooling=False",
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
            long nextId = 10_000;
            _db.Aop.DataExecuting = (_, entityInfo) =>
            {
                if (entityInfo.OperationType != DataFilterType.InsertByObject ||
                    !entityInfo.EntityColumnInfo.IsPrimarykey ||
                    entityInfo.EntityColumnInfo.PropertyInfo.PropertyType != typeof(long))
                {
                    return;
                }

                var id = (long?)entityInfo.EntityColumnInfo.PropertyInfo.GetValue(entityInfo.EntityValue);
                if (id.GetValueOrDefault() == 0)
                    entityInfo.SetValue(Interlocked.Increment(ref nextId));
            };
            _db.CodeFirst.InitTables(
                typeof(SaveLifeEnrollmentRecord),
                typeof(SaveLifeClinicalTrial),
                typeof(SaveLifeUserProfile),
                typeof(SaveLifePatientProfile));
            Service = new SaveLifePatientEnrollmentService(
                CreateRepository<SaveLifeEnrollmentRecord>(_db),
                CreateRepository<SaveLifeClinicalTrial>(_db),
                CreateRepository<SaveLifeUserProfile>(_db),
                CreateRepository<SaveLifePatientProfile>(_db));
        }

        public SaveLifePatientEnrollmentService Service { get; }

        public SaveLifeUserProfile AddAccount(string externalUserId)
        {
            var account = CreateAccount(externalUserId);
            _db.Insertable(account).ExecuteCommand();
            return account;
        }

        public SaveLifePatientProfile AddPatient(
            long id,
            string ownerExternalUserId,
            bool complete,
            string relationshipDescription = "本人")
        {
            var patient = CreatePatient(id, ownerExternalUserId, complete, relationshipDescription);
            _db.Insertable(patient).ExecuteCommand();
            return patient;
        }

        public void UpdatePatient(SaveLifePatientProfile patient)
        {
            _db.Updateable(patient).ExecuteCommand();
        }

        public void AddTrial(long id, string status = SaveLifeAgentTrialSelection.RecruitingStatus)
        {
            _db.Insertable(new SaveLifeClinicalTrial
            {
                Id = id,
                Title = $"测试项目 {id}",
                CdeRegistrationNumber = $"CTR{id}",
                Status = status,
                CreateTime = new DateTime(2026, 7, 14)
            }).ExecuteCommand();
        }

        public List<SaveLifeEnrollmentRecord> Enrollments()
        {
            return _db.Queryable<SaveLifeEnrollmentRecord>().ClearFilter().ToList();
        }

        public void Dispose()
        {
            _db.Dispose();
            if (File.Exists(_dbPath))
                File.Delete(_dbPath);
        }

        private static SqlSugarRepository<T> CreateRepository<T>(ISqlSugarClient db)
            where T : class, new()
        {
            return SqlSugarRepository<T>.CreateForContext(db);
        }
    }
}
