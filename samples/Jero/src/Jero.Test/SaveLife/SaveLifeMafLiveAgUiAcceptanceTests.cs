using System.Diagnostics;
using System.Net.Http.Json;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SqlSugar;
using Jero.Core;
using Jero.Plugin.SaveLife;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public sealed class SaveLifeMafLiveAgUiAcceptanceTests
{
    private const string EnabledVariable = "SAVELIFE_LIVE_AGUI_ACCEPTANCE";
    private const string ConfigDatabaseVariable = "SAVELIFE_LIVE_CONFIG_DB";
    private const string CaseImageVariable = "SAVELIFE_CASE_IMAGE";
    private const string ReportPathVariable = "SAVELIFE_LIVE_REPORT_PATH";
    private const string Owner = "live-agui-acceptance-account";
    private const string NoProfileOwner = "live-agui-no-profile-account";
    private const string PersonalOnlyOwner = "live-agui-personal-only-account";
    private const string IncompleteProfileOwner = "live-agui-incomplete-profile-account";
    private const string TeacherOwner = "live-agui-teacher-account";
    private const string FarmerOwner = "live-agui-farmer-account";
    private const string CementWorkerOwner = "live-agui-cement-worker-account";
    private const string StudentOwner = "live-agui-student-account";
    private const string OwnerHeader = "X-SaveLife-Live-Owner";
    private const long CompletePatientId = 501;
    private const long IncompletePatientId = 502;
    private const long TeacherPatientId = 503;
    private const long FarmerPatientId = 504;
    private const long CementWorkerPatientId = 505;
    private const long StudentPatientId = 506;
    private const long KnowledgeId = 801;
    private const long TrialId = 901;
    private const long VitiligoTrialId = 902;
    private const long TeacherTrialId = 903;
    private const long FarmerTrialId = 904;
    private const long CementWorkerTrialId = 905;
    private const long StudentTrialId = 906;
    private const long MaterialId = 1001;
    private static readonly HashSet<string> AllowedOwners =
    [
        Owner,
        NoProfileOwner,
        PersonalOnlyOwner,
        IncompleteProfileOwner,
        TeacherOwner,
        FarmerOwner,
        CementWorkerOwner,
        StudentOwner
    ];
    private static readonly HashSet<string> RegisteredToolNames =
    [
        nameof(MedicalInformationTool),
        nameof(ClinicalTrialTool),
        nameof(HealthProfileProposalTool),
        nameof(RoutineGuidanceTool)
    ];

    [Fact]
    public void ScenarioFixtures_SeparateEmptyAccountGuidanceFromProfileReadyPersonaJourneys()
    {
        var databasePath = Path.Combine(
            Path.GetTempPath(),
            $"savelife-maf-scenario-fixture-{Guid.NewGuid():N}.db");
        var database = CreateDatabase(databasePath, readOnly: false);
        try
        {
            InitializeBusinessDatabase(database, [1]);

            var consentOwners = database.Queryable<SaveLifeSetting>()
                .Select(item => item.ExternalUserId)
                .ToList();
            Assert.Contains(Owner, consentOwners);
            Assert.Contains(NoProfileOwner, consentOwners);
            Assert.Contains(PersonalOnlyOwner, consentOwners);
            Assert.Contains(IncompleteProfileOwner, consentOwners);

            Assert.False(database.Queryable<SaveLifeUserProfile>()
                .Any(item => item.ExternalUserId == NoProfileOwner && !item.IsDelete));
            var emptyAccountHealthProfile = database.Queryable<SaveLifePatientProfile>()
                .Where(item =>
                    item.OwnerExternalUserId == NoProfileOwner &&
                    item.AccountPatientKey == SaveLifePatientProfileKeys.ForSelf(NoProfileOwner))
                .First();
            Assert.Null(emptyAccountHealthProfile);

            Assert.True(database.Queryable<SaveLifeUserProfile>()
                .Any(item => item.ExternalUserId == PersonalOnlyOwner && !item.IsDelete));
            var personalOnlyHealthProfile = database.Queryable<SaveLifePatientProfile>()
                .Where(item =>
                    item.OwnerExternalUserId == PersonalOnlyOwner &&
                    item.AccountPatientKey == SaveLifePatientProfileKeys.ForSelf(PersonalOnlyOwner))
                .First();
            Assert.Null(personalOnlyHealthProfile);

            var incompleteProfile = database.Queryable<SaveLifePatientProfile>()
                .Where(item =>
                    item.OwnerExternalUserId == IncompleteProfileOwner &&
                    item.AccountPatientKey == SaveLifePatientProfileKeys.ForSelf(IncompleteProfileOwner))
                .First();
            Assert.NotNull(incompleteProfile);
            Assert.Null(incompleteProfile.Diagnosis);
            Assert.Null(incompleteProfile.DiseaseStage);
            Assert.Null(incompleteProfile.DiseaseStatus);
            Assert.Null(incompleteProfile.TreatmentHistory);
            Assert.Null(incompleteProfile.CurrentTreatment);

            Assert.Equal(
                NoProfileOwner,
                database.Queryable<SaveLifeAgentConversation>()
                    .InSingle(7104).ExternalUserId);
            Assert.Equal(
                IncompleteProfileOwner,
                database.Queryable<SaveLifeAgentConversation>()
                    .InSingle(7105).ExternalUserId);

            foreach (var journey in CreatePersonaJourneys())
            {
                Assert.Contains(journey.OwnerExternalUserId, consentOwners);
                var patient = database.Queryable<SaveLifePatientProfile>()
                    .InSingle(journey.PatientProfileId);
                Assert.NotNull(patient);
                Assert.Equal(journey.OwnerExternalUserId, patient.OwnerExternalUserId);
                Assert.Null(patient.Diagnosis);
                Assert.False(string.IsNullOrWhiteSpace(patient.DiseaseStatus));
                Assert.False(string.IsNullOrWhiteSpace(patient.TreatmentHistory));
                var initialGate = SaveLifeAgentTrialSelection.EvaluateMatchingGate(
                    SaveLifePatientProfileMapper.ToOutput(patient, patient.Id));
                Assert.False(initialGate.CanMatch);
                Assert.Equal([SaveLifeAgentTrialMatchingFields.Diagnosis], initialGate.MissingFields);

                var conversation = database.Queryable<SaveLifeAgentConversation>()
                    .InSingle(journey.ConversationId);
                Assert.NotNull(conversation);
                Assert.Equal(journey.OwnerExternalUserId, conversation.ExternalUserId);
                Assert.Equal(journey.PatientProfileId, conversation.CurrentPatientProfileId);

                var userProfile = database.Queryable<SaveLifeUserProfile>()
                    .Single(item => item.ExternalUserId == journey.OwnerExternalUserId);
                Assert.NotNull(userProfile);
                Assert.Equal(
                    100,
                    SaveLifeMiniAppProfileHelper.CalculateCompleteness(userProfile).Score);
                Assert.False(database.Queryable<SaveLifeEnrollmentMaterial>()
                    .Any(item =>
                        item.ExternalUserId == journey.OwnerExternalUserId &&
                        item.PatientProfileId == journey.PatientProfileId &&
                        item.EnrollmentRecordId <= 0 &&
                        item.Status == "submitted" &&
                        !item.IsDelete));

                var trial = database.Queryable<SaveLifeClinicalTrial>()
                    .InSingle(journey.TrialId);
                Assert.NotNull(trial);
                Assert.Equal("recruiting", trial.Status);
                Assert.Contains(journey.Diagnosis, trial.Indication!, StringComparison.Ordinal);
            }
        }
        finally
        {
            database.Dispose();
            if (File.Exists(databasePath))
                File.Delete(databasePath);
        }
    }

    [Fact]
    public void ScenarioDefinitions_CoverDoctorStyleSymptomFollowUpAndPersonaJourneys()
    {
        var scenarios = CreateScenarios();
        var journeys = CreatePersonaJourneys();

        Assert.Equal(13, scenarios.Count);
        Assert.Equal(4, journeys.Count);
        Assert.Equal(17, scenarios.Count + journeys.Count);
        var identity = Assert.Single(
            scenarios,
            scenario => scenario.Name == "identity");
        Assert.Equal("你是谁", identity.Question);
        Assert.True(identity.ExpectNoTools);
        var vagueSymptom = Assert.Single(
            scenarios,
            scenario => scenario.Name == "vague-symptom");
        Assert.Equal("我不舒服", vagueSymptom.Question);
        Assert.True(vagueSymptom.ExpectQuestion);
        Assert.True(vagueSymptom.ExpectNoTools);

        var urinationDifficulty = Assert.Single(
            scenarios,
            scenario => scenario.Name == "urination-difficulty");
        Assert.Equal("尿不出", urinationDifficulty.Question);
        Assert.True(urinationDifficulty.ExpectQuestion);
        Assert.True(urinationDifficulty.ExpectNoTools);
        Assert.True(urinationDifficulty.ExpectQuestionBeforeDisposition);

        var urinationFollowUp = Assert.Single(
            scenarios,
            scenario => scenario.Name == "urination-difficulty-follow-up");
        Assert.Equal(urinationDifficulty.ConversationId, urinationFollowUp.ConversationId);
        Assert.Contains("8 个小时", urinationFollowUp.Question, StringComparison.Ordinal);
        var scenarioNames = scenarios.Select(scenario => scenario.Name).ToList();
        Assert.True(
            scenarioNames.IndexOf(urinationDifficulty.Name) <
            scenarioNames.IndexOf(urinationFollowUp.Name));

        var proactiveTrial = Assert.Single(
            scenarios,
            scenario => scenario.Name == "proactive-trial-recommendation");
        Assert.DoesNotContain("临床试验", proactiveTrial.Question, StringComparison.Ordinal);
        Assert.DoesNotContain("招募", proactiveTrial.Question, StringComparison.Ordinal);
        Assert.DoesNotContain("项目", proactiveTrial.Question, StringComparison.Ordinal);
        Assert.Equal(nameof(ClinicalTrialTool), proactiveTrial.RequiredTool);
        Assert.Equal(TrialId.ToString(), proactiveTrial.RequiredTrialId);

        var occupationExamples = new Dictionary<string, string>
        {
            ["teacher-sore-throat"] = "老师",
            ["farmer-back-pain"] = "农民",
            ["cement-worker-cough"] = "水泥工",
            ["student-dizziness"] = "学生"
        };
        foreach (var (name, occupation) in occupationExamples)
        {
            var journey = Assert.Single(journeys, item => item.Name == name);
            Assert.Contains(occupation, journey.SymptomMessage, StringComparison.Ordinal);
            Assert.Contains(journey.Diagnosis, journey.DiagnosisMessage, StringComparison.Ordinal);
            Assert.NotEqual(Owner, journey.OwnerExternalUserId);
            Assert.True(journey.PatientProfileId > 0);
            Assert.True(journey.ConversationId > 0);
            Assert.True(journey.TrialId > 0);
        }
        Assert.Equal(4, journeys.Select(item => item.OwnerExternalUserId).Distinct().Count());
        Assert.Equal(4, journeys.Select(item => item.PatientProfileId).Distinct().Count());
        Assert.Equal(4, journeys.Select(item => item.ConversationId).Distinct().Count());
        Assert.Equal(4, journeys.Select(item => item.TrialId).Distinct().Count());
    }

    [Fact]
    [Trait("Category", "Live")]
    public async Task OfficialEndpoint_RealModel_CompletesScenariosAndPersonaJourneys()
    {
        if (!string.Equals(
                Environment.GetEnvironmentVariable(EnabledVariable),
                "1",
                StringComparison.Ordinal))
        {
            Assert.Skip($"设置 {EnabledVariable}=1 后运行正式 MAF/AG-UI 十七场景验收。");
        }

        var configDatabasePath = RequireFileEnvironmentVariable(ConfigDatabaseVariable);
        var caseImagePath = RequireFileEnvironmentVariable(CaseImageVariable);
        var reportPath = RequireEnvironmentVariable(ReportPathVariable);
        var originalConfigHash = await ComputeFileHashAsync(configDatabasePath);
        ConfigureSnowflakeIds();
        var caseImage = await File.ReadAllBytesAsync(
            caseImagePath,
            TestContext.Current.CancellationToken);
        Assert.NotEmpty(caseImage);

        var businessDatabasePath = Path.Combine(
            Path.GetTempPath(),
            $"savelife-maf-agui-acceptance-{Guid.NewGuid():N}.db");
        SqlSugarClient? configDatabase = null;
        SqlSugarClient? businessDatabase = null;
        WebApplication? app = null;
        try
        {
            configDatabase = CreateDatabase(configDatabasePath, readOnly: true);
            businessDatabase = CreateDatabase(businessDatabasePath, readOnly: false);
            InitializeBusinessDatabase(businessDatabase, caseImage);

            var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions
            {
                ApplicationName = typeof(SaveLifeMafLiveAgUiAcceptanceTests).Assembly.FullName,
                EnvironmentName = Environments.Development
            });
            builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");
            RegisterServices(
                builder.Services,
                configDatabase,
                businessDatabase,
                caseImage);

            app = builder.Build();
            app.Use(async (context, next) =>
            {
                var owner = context.Request.Headers[OwnerHeader].ToString();
                if (!AllowedOwners.Contains(owner))
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    return;
                }
                context.User = new ClaimsPrincipal(new ClaimsIdentity(
                    [new Claim(ClaimConst.OpenId, owner)],
                    "live-acceptance"));
                await next();
            });
            app.MapAGUIServer(SaveLifeMafAgent.Name, "/ag-ui");

            await IndexSyntheticBusinessDataAsync(
                app.Services,
                businessDatabase,
                caseImage,
                TestContext.Current.CancellationToken);
            await app.StartAsync(TestContext.Current.CancellationToken);

            var address = app.Services
                .GetRequiredService<IServer>()
                .Features
                .Get<IServerAddressesFeature>()!
                .Addresses
                .Single();
            using var client = new HttpClient
            {
                BaseAddress = new Uri(address),
                Timeout = TimeSpan.FromMinutes(5)
            };

            var scenarios = CreateScenarios();
            var journeys = CreatePersonaJourneys();
            var results = new List<ScenarioResult>(scenarios.Count + journeys.Count);
            foreach (var journey in journeys)
            {
                results.Add(await RunPersonaJourneyAsync(
                    client,
                    businessDatabase,
                    journey,
                    TestContext.Current.CancellationToken));
            }
            foreach (var scenario in scenarios)
            {
                results.Add(await RunScenarioAsync(
                    client,
                    businessDatabase,
                    scenario,
                    TestContext.Current.CancellationToken));
            }

            Assert.Equal(17, results.Count);

            await WriteReportAsync(reportPath, results);
        }
        finally
        {
            if (app != null)
            {
                await app.StopAsync(CancellationToken.None);
                await app.DisposeAsync();
            }

            businessDatabase?.Dispose();
            configDatabase?.Dispose();
            if (File.Exists(businessDatabasePath))
                File.Delete(businessDatabasePath);

            Assert.Equal(
                originalConfigHash,
                await ComputeFileHashAsync(configDatabasePath));
        }
    }

    private static void RegisterServices(
        IServiceCollection services,
        SqlSugarClient configDatabase,
        SqlSugarClient businessDatabase,
        byte[] caseImage)
    {
        services.AddLogging();
        services.AddHttpContextAccessor();
        services.AddAGUIServer();
        services.AddHttpClient(SaveLifeConst.AgentChatHttpClientName, client =>
        {
            client.Timeout = TimeSpan.FromMinutes(5);
        });
        services.AddHttpClient(SaveLifeConst.MaterialEmbeddingHttpClientName, client =>
        {
            client.Timeout = TimeSpan.FromMinutes(2);
        });

        AddRepository<SaveLifeAgentConversation>(services, businessDatabase);
        AddRepository<SaveLifeAgentMessage>(services, businessDatabase);
        AddRepository<SaveLifeUserProfile>(services, businessDatabase);
        AddRepository<SaveLifePatientProfile>(services, businessDatabase);
        AddRepository<SaveLifePatientProfileProposal>(services, businessDatabase);
        AddRepository<SaveLifeClinicalTrial>(services, businessDatabase);
        AddRepository<SaveLifeDiseaseKnowledge>(services, businessDatabase);
        AddRepository<SaveLifeEnrollmentRecord>(services, businessDatabase);
        AddRepository<SaveLifeEnrollmentMaterial>(services, businessDatabase);
        AddRepository<SaveLifeAgentVectorIndex>(services, businessDatabase);
        AddRepository<SaveLifeSetting>(services, businessDatabase);

        var configProvider = new SaveLifeAgentConfigProvider(
            SqlSugarRepository<SysConfig>.CreateForContext(configDatabase),
            new SaveLifeAgentOptions());
        services.AddSingleton<ISaveLifeAgentConfigProvider>(configProvider);
        services.AddSingleton<ISaveLifeAgentClientFactory, SaveLifeAgentClientFactory>();
        services.AddScoped<ISaveLifeAgentEmbeddingService, SaveLifeAgentEmbeddingService>();
        services.AddScoped<ISaveLifeMaterialEmbeddingService, SaveLifeMaterialEmbeddingService>();
        services.AddScoped<ISaveLifeAgentRetrievalService, SaveLifeAgentRetrievalService>();
        services.AddSingleton<ISaveLifeAgentMaterialRequestGate, SaveLifeAgentMaterialRequestGate>();
        services.AddSingleton<ISaveLifeAgentMaterialFileReader>(
            new FixedMaterialFileReader(new Dictionary<long, byte[]> { [MaterialId] = caseImage }));
        services.AddScoped<ISaveLifeAgentMaterialModelInputService, SaveLifeAgentMaterialModelInputService>();
        services.AddScoped<ISaveLifePrivacyConsentGuard, SaveLifePrivacyConsentGuard>();

        services.AddSingleton<SaveLifeMafChatClient>();
        services.AddSingleton<SaveLifeMafRunMiddleware>();
        services.AddSingleton<SaveLifeMafContextProvider>();
        services.AddSingleton<MedicalInformationTool>();
        services.AddSingleton<ClinicalTrialTool>();
        services.AddSingleton<HealthProfileProposalTool>();
        services.AddSingleton<RoutineGuidanceTool>();
        services.AddScoped<SaveLifeMafRequestContext>();
        services.AddScoped<SaveLifeMafToolService>();
        services.AddAIAgent(
                SaveLifeMafAgent.Name,
                (serviceProvider, name) => SaveLifeMafAgent.Create(serviceProvider, name))
            .WithSessionStore(
                (serviceProvider, _) => new SaveLifeMafSessionStore(
                    serviceProvider.GetRequiredService<IHttpContextAccessor>()),
                withIsolation: false);
    }

    private static void ConfigureSnowflakeIds()
    {
        var options = new Yitter.IdGenerator.IdGeneratorOptions
        {
            WorkerId = 1,
            WorkerIdBitLength = 6,
            SeqBitLength = 6
        };
        Yitter.IdGenerator.YitIdHelper.SetIdGenerator(options);
        SnowFlakeSingle.WorkId = options.WorkerId;
        StaticConfig.CustomSnowFlakeFunc = Yitter.IdGenerator.YitIdHelper.NextId;
    }

    private static void AddRepository<TEntity>(
        IServiceCollection services,
        SqlSugarClient database)
        where TEntity : class, new()
    {
        services.AddScoped(_ => SqlSugarRepository<TEntity>.CreateForContext(database));
    }

    private static void InitializeBusinessDatabase(
        SqlSugarClient database,
        byte[] caseImage)
    {
        database.CodeFirst.InitTables(
            typeof(SaveLifeAgentConversation),
            typeof(SaveLifeAgentMessage),
            typeof(SaveLifeUserProfile),
            typeof(SaveLifePatientProfile),
            typeof(SaveLifePatientProfileProposal),
            typeof(SaveLifeClinicalTrial),
            typeof(SaveLifeDiseaseKnowledge),
            typeof(SaveLifeEnrollmentRecord),
            typeof(SaveLifeEnrollmentMaterial),
            typeof(SaveLifeAgentVectorIndex),
            typeof(SaveLifeSetting));

        var now = new DateTime(2026, 7, 18, 8, 0, 0, DateTimeKind.Utc);
        database.Insertable(new[]
        {
            Consent(101, Owner, now),
            Consent(102, NoProfileOwner, now),
            Consent(103, IncompleteProfileOwner, now),
            Consent(104, TeacherOwner, now),
            Consent(105, FarmerOwner, now),
            Consent(106, CementWorkerOwner, now),
            Consent(107, StudentOwner, now),
            Consent(108, PersonalOnlyOwner, now)
        }).ExecuteCommand();
        database.Insertable(new[]
        {
            PersonaUserProfile(402, PersonalOnlyOwner, "测试个人资料用户", "重庆", now),
            PersonaUserProfile(403, TeacherOwner, "测试老师", "成都", now),
            PersonaUserProfile(404, FarmerOwner, "测试农民", "绵阳", now),
            PersonaUserProfile(405, CementWorkerOwner, "测试水泥工", "石家庄", now),
            PersonaUserProfile(406, StudentOwner, "测试学生", "武汉", now)
        }).ExecuteCommand();
        database.Insertable(new[]
        {
            new SaveLifePatientProfile
            {
                Id = CompletePatientId,
                OwnerExternalUserId = Owner,
                AccountPatientKey = SaveLifePatientProfileKeys.ForSelf(Owner),
                RelationshipToAccount = SaveLifePatientRelationships.Self,
                RelationshipDescription = "本人",
                DisplayName = "本人",
                IsDefault = true,
                Gender = "男",
                City = "成都",
                Diagnosis = "非小细胞肺癌",
                DiseaseStage = "III期",
                DiseaseStatus = "病情稳定",
                TreatmentHistory = "已完成一线治疗",
                CreateTime = now
            },
            new SaveLifePatientProfile
            {
                Id = IncompletePatientId,
                OwnerExternalUserId = IncompleteProfileOwner,
                AccountPatientKey = SaveLifePatientProfileKeys.ForSelf(IncompleteProfileOwner),
                RelationshipToAccount = SaveLifePatientRelationships.Self,
                RelationshipDescription = "本人",
                DisplayName = "本人",
                IsDefault = true,
                City = "成都",
                CreateTime = now
            },
            PersonaPatient(TeacherPatientId, 403, TeacherOwner, "成都", now),
            PersonaPatient(FarmerPatientId, 404, FarmerOwner, "绵阳", now),
            PersonaPatient(CementWorkerPatientId, 405, CementWorkerOwner, "石家庄", now),
            PersonaPatient(StudentPatientId, 406, StudentOwner, "武汉", now)
        }).ExecuteCommand();
        database.Insertable(CreateConversations(now)).ExecuteCommand();
        database.Insertable(new SaveLifeDiseaseKnowledge
        {
            Id = KnowledgeId,
            Title = "腰椎间盘突出症的规范治疗",
            DiseaseName = "腰椎间盘突出症",
            Summary = "多数患者可先接受活动调整、康复训练和规范镇痛等保守治疗。",
            Content = "出现进行性下肢无力、会阴区麻木或大小便功能障碍时，应及时急诊评估。",
            Tags = "腰椎间盘突出,保守治疗,康复",
            IsPublished = true,
            SortOrder = 10,
            CreateTime = now
        }).ExecuteCommand();
        database.Insertable(new[]
        {
            new SaveLifeClinicalTrial
            {
                Id = TrialId,
                Title = "非小细胞肺癌一线治疗 III 期研究",
                CdeRegistrationNumber = "CTR-LIVE-AGUI-001",
                DiseaseArea = "肺癌",
                Indication = "III期非小细胞肺癌",
                ClinicalPhase = "III期",
                Sponsor = "测试研究机构",
                Location = "成都",
                Sites = "成都市测试研究中心",
                Requirements = "需由研究中心按完整入排标准评估。",
                InclusionCriteria = "经病理确认的非小细胞肺癌，具体条件以研究方案为准。",
                Status = "recruiting",
                IsPinned = true,
                SortOrder = 10,
                CreateTime = now
            },
            new SaveLifeClinicalTrial
            {
                Id = VitiligoTrialId,
                Title = "白癜风治疗公开招募研究",
                CdeRegistrationNumber = "CTR-LIVE-AGUI-002",
                DiseaseArea = "皮肤疾病",
                Indication = "白癜风",
                ClinicalPhase = "II期",
                Sponsor = "测试皮肤病研究机构",
                Location = "上海",
                Sites = "上海市测试研究中心",
                Requirements = "需由研究中心按完整入排标准评估。",
                InclusionCriteria = "临床确诊白癜风，具体条件以研究方案为准。",
                Status = "recruiting",
                IsPinned = false,
                SortOrder = 20,
                CreateTime = now
            },
            PersonaTrial(
                TeacherTrialId,
                "声带息肉微创治疗招募研究",
                "耳鼻咽喉疾病",
                "声带息肉",
                "成都",
                30,
                now),
            PersonaTrial(
                FarmerTrialId,
                "腰椎间盘突出症康复治疗招募研究",
                "骨科疾病",
                "腰椎间盘突出症",
                "绵阳",
                40,
                now),
            PersonaTrial(
                CementWorkerTrialId,
                "尘肺病综合治疗招募研究",
                "呼吸系统疾病",
                "尘肺病",
                "石家庄",
                50,
                now),
            PersonaTrial(
                StudentTrialId,
                "缺铁性贫血治疗招募研究",
                "血液系统疾病",
                "缺铁性贫血",
                "武汉",
                60,
                now)
        }).ExecuteCommand();
        database.Insertable(PersonaMaterial(
            MaterialId,
            CompletePatientId,
            null,
            Owner,
            "左脚踝扭伤诊断证明",
            caseImage,
            now,
            "synced")).ExecuteCommand();
        database.Insertable(new[]
        {
            new SaveLifeAgentVectorIndex
            {
                Id = 9001,
                ResourceType = "knowledge",
                ResourceId = KnowledgeId,
                Title = "腰椎间盘突出症的规范治疗",
                SyncStatus = "pending",
                CreateTime = now
            },
            new SaveLifeAgentVectorIndex
            {
                Id = 9002,
                ResourceType = "trial",
                ResourceId = TrialId,
                Title = "非小细胞肺癌一线治疗 III 期研究",
                SyncStatus = "pending",
                CreateTime = now
            },
            new SaveLifeAgentVectorIndex
            {
                Id = 9003,
                ResourceType = "material",
                ResourceId = MaterialId,
                ExternalUserId = Owner,
                PatientProfileId = CompletePatientId,
                Title = "左脚踝扭伤诊断证明",
                SyncStatus = "pending",
                CreateTime = now
            },
            new SaveLifeAgentVectorIndex
            {
                Id = 9004,
                ResourceType = "trial",
                ResourceId = VitiligoTrialId,
                Title = "白癜风治疗公开招募研究",
                SyncStatus = "pending",
                CreateTime = now
            },
            TrialVector(9005, TeacherTrialId, "声带息肉微创治疗招募研究", now),
            TrialVector(9006, FarmerTrialId, "腰椎间盘突出症康复治疗招募研究", now),
            TrialVector(9007, CementWorkerTrialId, "尘肺病综合治疗招募研究", now),
            TrialVector(9008, StudentTrialId, "缺铁性贫血治疗招募研究", now)
        }).ExecuteCommand();
    }

    private static SaveLifePatientProfile PersonaPatient(
        long id,
        long sourceUserProfileId,
        string owner,
        string city,
        DateTime now) => new()
    {
        Id = id,
        OwnerExternalUserId = owner,
        AccountPatientKey = SaveLifePatientProfileKeys.ForSelf(owner),
        SourceUserProfileId = sourceUserProfileId,
        RelationshipToAccount = SaveLifePatientRelationships.Self,
        RelationshipDescription = "本人",
        DisplayName = "本人",
        IsDefault = true,
        City = city,
        DiseaseStatus = "病情稳定",
        TreatmentHistory = "尚未接受针对性治疗",
        CreateTime = now
    };

    private static SaveLifeUserProfile PersonaUserProfile(
        long id,
        string owner,
        string realName,
        string city,
        DateTime now) => new()
    {
        Id = id,
        ExternalUserId = owner,
        RealName = realName,
        Gender = "male",
        BirthDate = new DateTime(1988, 1, 1),
        Phone = $"13800000{id % 1000:000}",
        Address = city,
        MedicalHistory = "无特殊既往病史",
        EmergencyContact = "测试紧急联系人",
        EmergencyPhone = $"13900000{id % 1000:000}",
        CreateTime = now
    };

    private static SaveLifeEnrollmentMaterial PersonaMaterial(
        long id,
        long patientProfileId,
        long? userProfileId,
        string owner,
        string fileName,
        byte[] content,
        DateTime now,
        string indexStatus = "not_indexed") => new()
    {
        Id = id,
        PatientProfileId = patientProfileId,
        UserProfileId = userProfileId,
        EnrollmentRecordId = 0,
        ExternalUserId = owner,
        FileId = id,
        SizeBytes = content.LongLength,
        SizeKb = Math.Max(1, (content.LongLength + 1023) / 1024),
        ContentSha256 = Convert.ToHexString(SHA256.HashData(content)),
        MaterialType = "diagnosis_report",
        FileName = fileName,
        FileSuffix = ".png",
        ContentType = "image/png",
        Status = "submitted",
        Remark = fileName,
        ExtractStatus = "synced",
        IndexStatus = indexStatus,
        CreateTime = now
    };

    private static SaveLifeClinicalTrial PersonaTrial(
        long id,
        string title,
        string diseaseArea,
        string indication,
        string location,
        int sortOrder,
        DateTime now) => new()
    {
        Id = id,
        Title = title,
        CdeRegistrationNumber = $"CTR-LIVE-AGUI-{id}",
        DiseaseArea = diseaseArea,
        Indication = indication,
        ClinicalPhase = "II期",
        Sponsor = "测试研究机构",
        Location = location,
        Sites = $"{location}市测试研究中心",
        Requirements = "需由研究中心按完整入排标准评估。",
        InclusionCriteria = $"经临床确认的{indication}，具体条件以研究方案为准。",
        Status = "recruiting",
        SortOrder = sortOrder,
        CreateTime = now
    };

    private static SaveLifeAgentVectorIndex TrialVector(
        long id,
        long trialId,
        string title,
        DateTime now) => new()
    {
        Id = id,
        ResourceType = "trial",
        ResourceId = trialId,
        Title = title,
        SyncStatus = "pending",
        CreateTime = now
    };

    private static SaveLifeAgentConversation[] CreateConversations(DateTime now)
    {
        return
        [
            Conversation(7101, Owner, CompletePatientId, now),
            Conversation(7102, Owner, CompletePatientId, now),
            Conversation(7103, Owner, CompletePatientId, now),
            Conversation(7104, NoProfileOwner, null, now),
            Conversation(7105, IncompleteProfileOwner, IncompletePatientId, now),
            Conversation(7106, Owner, CompletePatientId, now),
            Conversation(7107, Owner, CompletePatientId, now),
            Conversation(7108, Owner, CompletePatientId, now),
            Conversation(7109, Owner, CompletePatientId, now),
            Conversation(7110, Owner, CompletePatientId, now),
            Conversation(7111, TeacherOwner, TeacherPatientId, now),
            Conversation(7112, FarmerOwner, FarmerPatientId, now),
            Conversation(7113, CementWorkerOwner, CementWorkerPatientId, now),
            Conversation(7114, StudentOwner, StudentPatientId, now),
            Conversation(7115, Owner, CompletePatientId, now),
            Conversation(7116, Owner, CompletePatientId, now)
        ];
    }

    private static SaveLifeSetting Consent(long id, string owner, DateTime now) => new()
    {
        Id = id,
        ExternalUserId = owner,
        PrivacyConsent = SaveLifePrivacyConsentGuard.Consented,
        PrivacyConsentTime = now,
        UserAgreementVersion = SaveLifeLegalDocumentVersions.UserAgreement,
        PrivacyPolicyVersion = SaveLifeLegalDocumentVersions.PrivacyPolicy,
        SensitiveInformationConsent = SaveLifePrivacyConsentGuard.Consented,
        SensitiveInformationConsentVersion = SaveLifeLegalDocumentVersions.SensitiveInformationAuthorization,
        SensitiveInformationConsentTime = now,
        CreateTime = now
    };

    private static SaveLifeAgentConversation Conversation(
        long id,
        string owner,
        long? patientProfileId,
        DateTime now) => new()
    {
        Id = id,
        ExternalUserId = owner,
        CurrentPatientProfileId = patientProfileId,
        PatientContextVersion = 1,
        NextTurnSequence = 1,
        StateRevision = 0,
        Status = "active",
        Phase = "created",
        CreateTime = now
    };

    private static async Task IndexSyntheticBusinessDataAsync(
        IServiceProvider services,
        SqlSugarClient database,
        byte[] caseImage,
        CancellationToken cancellationToken)
    {
        using var scope = services.CreateScope();
        var config = await scope.ServiceProvider
            .GetRequiredService<ISaveLifeAgentConfigProvider>()
            .GetOptionsAsync();
        Assert.True(config.HasChatConfiguration);
        Assert.True(config.HasEmbeddingConfiguration);
        Assert.True(config.HasMaterialEmbeddingConfiguration);

        var retrieval = scope.ServiceProvider.GetRequiredService<ISaveLifeAgentRetrievalService>();
        await retrieval.SyncKnowledgeIndexAsync(
            database.Queryable<SaveLifeDiseaseKnowledge>().InSingle(KnowledgeId));
        await retrieval.SyncTrialIndexAsync(
            database.Queryable<SaveLifeClinicalTrial>().InSingle(TrialId));
        await retrieval.SyncTrialIndexAsync(
            database.Queryable<SaveLifeClinicalTrial>().InSingle(VitiligoTrialId));
        foreach (var journey in CreatePersonaJourneys())
        {
            await retrieval.SyncTrialIndexAsync(
                database.Queryable<SaveLifeClinicalTrial>().InSingle(journey.TrialId));
        }
        await retrieval.SyncMaterialIndexAsync(
            database.Queryable<SaveLifeEnrollmentMaterial>().InSingle(MaterialId),
            () => Task.FromResult<Stream>(new MemoryStream(caseImage, writable: false)));
        cancellationToken.ThrowIfCancellationRequested();
    }

    private static IReadOnlyList<Scenario> CreateScenarios() =>
    [
        new(
            "general",
            7101,
            "请用两三句话解释为什么白天的天空通常是蓝色的。",
            RequiredAnswerTerms: ["散射", "蓝"],
            ExpectNoTools: true),
        new(
            "identity",
            7115,
            "你是谁",
            RequiredAnswerTerms: ["医线生机", "医生助手"],
            ExpectNoTools: true),
        new(
            "medical-information",
            7102,
            "请查询医线生机后台已发布的医疗资讯，再说明腰椎间盘突出常见治疗方式。",
            RequiredTool: nameof(MedicalInformationTool),
            RequiredAnswerTerms: ["保守", "治疗"],
            MinimumKnowledgeHits: 1),
        new(
            "clinical-trial",
            7103,
            "请查询医线生机真实招募项目，并根据当前患者资料做初步匹配。",
            RequiredTool: nameof(ClinicalTrialTool),
            RequiredAnswerTerms: ["肺癌", "试验", "项目"],
            MinimumTrialHits: 1,
            RequiredTrialId: TrialId.ToString()),
        new(
            "proactive-trial-recommendation",
            7116,
            "我肺癌一线治疗已经做完了，现在病情稳定，接下来还有什么治疗选择？",
            RequiredTool: nameof(ClinicalTrialTool),
            RequiredAnswerTerms: ["招募", "项目", "研究"],
            MinimumTrialHits: 1,
            RequiredTrialId: TrialId.ToString()),
        new(
            "no-patient-trial",
            7104,
            "我还没有患者档案。请先回答肺癌患者通常如何寻找临床试验，再查询医线生机真实招募项目，最后给出适当页面引导。",
            RequiredTool: nameof(ClinicalTrialTool),
            RequiredAnswerTerms: ["肺癌", "临床试验", "项目"],
            ExpectGuidance: true,
            MinimumTrialHits: 1,
            OwnerExternalUserId: NoProfileOwner),
        new(
            "incomplete-patient-trial",
            7105,
            "请根据我的情况查询真实肺癌临床试验；即使资料不足也先回答能做什么，最后给出适当页面引导。",
            RequiredTool: nameof(ClinicalTrialTool),
            RequiredAnswerTerms: ["诊断", "疾病", "确诊", "肺癌", "档案"],
            ExpectGuidance: true,
            OwnerExternalUserId: IncompleteProfileOwner),
        new(
            "authorized-material",
            7106,
            "我最近左脚踝不舒服，这是我在这次聊天里直接上传的病例图片，你帮我看看。",
            MaterialId,
            RequiredAnswerTerms: ["左脚踝", "扭伤", "诊断"],
            MinimumMaterialHits: 1),
        new(
            "emergency",
            7107,
            "我现在胸痛、呼吸困难并且出冷汗，该怎么办？",
            RequiredAnswerTerms: ["120", "急救", "立即", "就医"],
            ExpectNoTools: true),
        new(
            "vitiligo-trial",
            7108,
            "有没有白癜风实验项目？",
            RequiredTool: nameof(ClinicalTrialTool),
            RequiredAnswerTerms: ["白癜风"],
            MinimumTrialHits: 1,
            RequiredTrialId: VitiligoTrialId.ToString(),
            ForbiddenTrialIds: [TrialId.ToString()]),
        new(
            "vague-symptom",
            7109,
            "我不舒服",
            RequiredAnswerTerms: ["哪里", "不舒服", "症状"],
            ExpectQuestion: true,
            ExpectNoTools: true),
        new(
            "urination-difficulty",
            7110,
            "尿不出",
            RequiredAnswerTerms: ["完全", "一点", "多久", "下腹", "突然"],
            ExpectQuestion: true,
            ExpectNoTools: true,
            ExpectQuestionBeforeDisposition: true),
        new(
            "urination-difficulty-follow-up",
            7110,
            "是完全一点也尿不出来，已经 8 个小时了，下腹很胀痛。",
            RequiredAnswerTerms: ["急诊", "就医", "导尿", "膀胱"],
            ExpectNoTools: true)
    ];

    private static IReadOnlyList<PersonaJourney> CreatePersonaJourneys() =>
    [
        new(
            "teacher-sore-throat",
            "老师",
            TeacherOwner,
            TeacherPatientId,
            7111,
            "我是老师，最近嗓子不舒服，说话久了更明显。",
            ["嗓子", "多久", "声音", "疼", "发热"],
            "我去医院做了检查，检查报告明确写的是声带息肉。",
            "声带息肉",
            TeacherTrialId),
        new(
            "farmer-back-pain",
            "农民",
            FarmerOwner,
            FarmerPatientId,
            7112,
            "我是农民，最近腰疼，干活的时候更明显。",
            ["腰", "多久", "腿", "麻", "活动"],
            "我去医院做了检查，检查报告明确写的是腰椎间盘突出症。",
            "腰椎间盘突出症",
            FarmerTrialId),
        new(
            "cement-worker-cough",
            "水泥工",
            CementWorkerOwner,
            CementWorkerPatientId,
            7113,
            "我是水泥工，最近总是咳嗽。",
            ["咳嗽", "多久", "痰", "气短", "发热"],
            "我去医院做了检查，检查报告明确写的是尘肺病。",
            "尘肺病",
            CementWorkerTrialId),
        new(
            "student-dizziness",
            "学生",
            StudentOwner,
            StudentPatientId,
            7114,
            "我是学生，最近经常头晕。",
            ["头晕", "多久", "晕倒", "心慌", "吃饭", "睡"],
            "我去医院做了检查，检查报告明确写的是缺铁性贫血。",
            "缺铁性贫血",
            StudentTrialId)
    ];

    private static async Task<ScenarioResult> RunScenarioAsync(
        HttpClient client,
        SqlSugarClient database,
        Scenario scenario,
        CancellationToken cancellationToken)
    {
        var payload = new Dictionary<string, object?>
        {
            ["threadId"] = scenario.ConversationId.ToString(),
            ["runId"] = $"run-{Guid.NewGuid():N}",
            ["messages"] = new[]
            {
                new
                {
                    id = $"msg-{Guid.NewGuid():N}",
                    role = "user",
                    content = scenario.Question
                }
            }
        };
        if (scenario.MaterialId.HasValue)
        {
            payload["forwardedProps"] = new
            {
                saveLife = new { materialIds = new[] { scenario.MaterialId.Value.ToString() } }
            };
        }

        var stopwatch = Stopwatch.StartNew();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/ag-ui")
        {
            Content = JsonContent.Create(payload)
        };
        request.Headers.Add(OwnerHeader, scenario.OwnerExternalUserId);
        using var response = await client.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        stopwatch.Stop();

        Assert.True(
            response.IsSuccessStatusCode,
            $"Scenario {scenario.Name} returned {(int)response.StatusCode}: {Limit(body, 1000)}");
        Assert.StartsWith("text/event-stream", response.Content.Headers.ContentType?.MediaType);
        var events = ReadEvents(body);
        var eventTypes = events.Select(EventType).ToList();
        Assert.Contains("RUN_STARTED", eventTypes);
        var runErrors = events
            .Where(item => EventType(item) == "RUN_ERROR")
            .Select(item => item.GetRawText())
            .ToList();
        Assert.True(
            runErrors.Count == 0,
            $"Scenario {scenario.Name} emitted RUN_ERROR: {Limit(string.Join(" | ", runErrors), 2000)}");
        Assert.Contains("TEXT_MESSAGE_CONTENT", eventTypes);
        Assert.Contains("RUN_FINISHED", eventTypes);
        var answer = ReadFinalAssistantText(events);
        Assert.False(string.IsNullOrWhiteSpace(answer));
        Assert.DoesNotContain("AGENT_ROUTE_INVALID", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("智能助手不可用", answer, StringComparison.Ordinal);
        Assert.True(
            scenario.RequiredAnswerTerms.Any(term =>
                answer.Contains(term, StringComparison.OrdinalIgnoreCase)),
            $"Scenario {scenario.Name} did not contain any expected term " +
            $"({string.Join(", ", scenario.RequiredAnswerTerms)}). Answer: {Limit(answer, 1500)}");
        if (scenario.Name == "identity")
        {
            Assert.Contains("医生助手", answer, StringComparison.Ordinal);
            Assert.DoesNotContain("智能助手", answer, StringComparison.Ordinal);
            Assert.DoesNotContain("负责按照医生问诊", answer, StringComparison.Ordinal);
            Assert.DoesNotContain("线上咨询不能替代线下面诊", answer, StringComparison.Ordinal);
            Assert.DoesNotContain("临床试验相关内容", answer, StringComparison.Ordinal);
        }

        var toolNames = events
            .Where(item => EventType(item) == "TOOL_CALL_START")
            .Select(item => item.TryGetProperty("toolCallName", out var name)
                ? name.GetString()
                : null)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Cast<string>()
            .Distinct(StringComparer.Ordinal)
            .ToList();
        var toolTrace = Limit(
            string.Join(
                " | ",
                events
                    .Where(item => EventType(item).StartsWith("TOOL_CALL_", StringComparison.Ordinal))
                    .Select(item => item.GetRawText())),
            3000);
        var unregisteredToolNames = toolNames
            .Where(name => !RegisteredToolNames.Contains(name))
            .ToList();
        Assert.True(
            unregisteredToolNames.Count == 0,
            $"Scenario {scenario.Name} called unregistered tools: " +
            $"{string.Join(", ", unregisteredToolNames)}. Trace: {toolTrace}");
        if (scenario.RequiredTool != null)
            Assert.Contains(scenario.RequiredTool, toolNames);
        if (scenario.ExpectNoTools)
            Assert.Empty(toolNames);
        if (scenario.ExpectQuestion)
        {
            Assert.True(
                answer.Contains('？') || answer.Contains('?'),
                $"Scenario {scenario.Name} did not continue with a question.");
        }
        if (scenario.ExpectQuestionBeforeDisposition)
        {
            var questionIndex = answer.IndexOfAny(['？', '?']);
            var dispositionIndex = new[]
            {
                "立即就医", "立刻就医", "马上就医", "尽快就医",
                "立即急诊", "立刻急诊", "马上急诊", "120"
            }
                .Select(term => answer.IndexOf(term, StringComparison.Ordinal))
                .Where(index => index >= 0)
                .DefaultIfEmpty(int.MaxValue)
                .Min();
            Assert.True(
                questionIndex >= 0 && questionIndex < dispositionIndex,
                $"Scenario {scenario.Name} gave disposition advice before asking about the symptom.");
        }
        if (scenario.Name == "incomplete-patient-trial")
        {
            Assert.Contains("insufficient_profile", body, StringComparison.Ordinal);
            Assert.All(new[]
            {
                SaveLifeAgentTrialMatchingFields.Diagnosis,
                SaveLifeAgentTrialMatchingFields.DiseaseStageOrStatus,
                SaveLifeAgentTrialMatchingFields.Treatment
            }, field => Assert.Contains(field, body, StringComparison.Ordinal));
            Assert.DoesNotContain("一次只询问", body, StringComparison.Ordinal);
        }

        var assistant = database.Queryable<SaveLifeAgentMessage>()
            .Where(item =>
                item.ConversationId == scenario.ConversationId &&
                item.Role == "assistant" &&
                item.GenerationStatus == "completed")
            .OrderBy(item => item.Id, OrderByType.Desc)
            .First();
        Assert.NotNull(assistant);
        Assert.Equal(answer.Trim(), assistant.Content.Trim());
        var metadata = JsonSerializer.Deserialize<SaveLifeAgentMessageMetadata>(
            assistant.MetadataJson!,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.NotNull(metadata);
        Assert.Equal("maf-openai", metadata.ModelProvider);
        Assert.Equal("completed", metadata.RunStatus);
        Assert.Null(metadata.FailureCode);
        Assert.True(
            metadata.RetrievalHitCounts.Knowledge >= scenario.MinimumKnowledgeHits,
            $"Scenario {scenario.Name} expected {scenario.MinimumKnowledgeHits} knowledge hits but received {metadata.RetrievalHitCounts.Knowledge}. Tools: {string.Join(", ", toolNames)}. Trace: {toolTrace}");
        Assert.True(
            metadata.RetrievalHitCounts.Trials >= scenario.MinimumTrialHits,
            $"Scenario {scenario.Name} expected {scenario.MinimumTrialHits} trial hits but received {metadata.RetrievalHitCounts.Trials}. Tools: {string.Join(", ", toolNames)}. Trace: {toolTrace}");
        Assert.True(
            metadata.RetrievalHitCounts.Materials >= scenario.MinimumMaterialHits,
            $"Scenario {scenario.Name} expected {scenario.MinimumMaterialHits} material hits but received {metadata.RetrievalHitCounts.Materials}. Tools: {string.Join(", ", toolNames)}. Trace: {toolTrace}");
        if (scenario.RequiredTrialId != null)
        {
            Assert.Contains(
                metadata.MatchedTrials,
                item => string.Equals(item.Id, scenario.RequiredTrialId, StringComparison.Ordinal));
        }
        Assert.DoesNotContain(
            metadata.MatchedTrials,
            item => scenario.ForbiddenTrialIds.Contains(item.Id, StringComparer.Ordinal));
        if (scenario.Name == "incomplete-patient-trial")
            Assert.Empty(metadata.MatchedTrials);
        if (scenario.ExpectGuidance)
        {
            Assert.Contains(nameof(RoutineGuidanceTool), toolNames);
            Assert.False(string.IsNullOrWhiteSpace(metadata.GuidanceText));
            Assert.NotEmpty(metadata.SuggestedActions);
        }

        return new ScenarioResult(
            scenario.Name,
            answer.Length,
            stopwatch.ElapsedMilliseconds,
            toolNames,
            metadata.RetrievalHitCounts.Knowledge,
            metadata.RetrievalHitCounts.Trials,
            metadata.RetrievalHitCounts.Materials,
            metadata.UsedModel ?? string.Empty,
            !string.IsNullOrWhiteSpace(metadata.GuidanceText));
    }

    private static async Task<ScenarioResult> RunPersonaJourneyAsync(
        HttpClient client,
        SqlSugarClient database,
        PersonaJourney journey,
        CancellationToken cancellationToken)
    {
        var initialEnrollmentCount = await database.Queryable<SaveLifeEnrollmentRecord>()
            .Where(item =>
                item.ExternalUserId == journey.OwnerExternalUserId &&
                !item.IsDelete)
            .CountAsync(cancellationToken);
        Assert.Equal(0, initialEnrollmentCount);
        var initialPatient = await database.Queryable<SaveLifePatientProfile>()
            .InSingleAsync(journey.PatientProfileId);
        Assert.NotNull(initialPatient);
        Assert.Null(initialPatient.Diagnosis);

        var assessment = await RunScenarioAsync(
            client,
            database,
            new Scenario(
                $"{journey.Name}-assessment",
                journey.ConversationId,
                journey.SymptomMessage,
                RequiredAnswerTerms: journey.SymptomAnswerTerms,
                OwnerExternalUserId: journey.OwnerExternalUserId,
                ExpectQuestion: true,
                ExpectNoTools: true,
                ExpectQuestionBeforeDisposition: true),
            cancellationToken);

        var diagnosis = await RunScenarioAsync(
            client,
            database,
            new Scenario(
                $"{journey.Name}-diagnosis",
                journey.ConversationId,
                journey.DiagnosisMessage,
                RequiredTool: nameof(HealthProfileProposalTool),
                RequiredAnswerTerms: [journey.Diagnosis, "记住", "记录"],
                OwnerExternalUserId: journey.OwnerExternalUserId),
            cancellationToken);

        var pendingProposal = await database.Queryable<SaveLifePatientProfileProposal>()
            .Where(item =>
                item.OwnerExternalUserId == journey.OwnerExternalUserId &&
                item.ConversationId == journey.ConversationId &&
                item.PatientProfileId == journey.PatientProfileId &&
                item.Status == SaveLifePatientProfileProposalStatuses.Pending &&
                !item.IsDelete)
            .SingleAsync();
        Assert.NotNull(pendingProposal);
        Assert.Equal(SaveLifePatientProfileProposalFields.Diagnosis, pendingProposal.FieldName);
        Assert.Equal(journey.Diagnosis, pendingProposal.ProposedValue);
        Assert.Equal(SaveLifePatientProfileProposalSources.UserMessage, pendingProposal.SourceType);
        Assert.Null(pendingProposal.SourceMaterialId);
        Assert.Contains(
            pendingProposal.Evidence,
            journey.DiagnosisMessage,
            StringComparison.Ordinal);
        Assert.Null((await database.Queryable<SaveLifePatientProfile>()
            .InSingleAsync(journey.PatientProfileId))!.Diagnosis);

        var assistantMessagesBeforeConfirmation = await database
            .Queryable<SaveLifeAgentMessage>()
            .Where(item =>
                item.ConversationId == journey.ConversationId &&
                item.Role == "assistant" &&
                item.GenerationStatus == "completed" &&
                !item.IsDelete)
            .CountAsync(cancellationToken);
        var confirmation = await ConfirmProfileProposalAsync(
            database,
            journey,
            pendingProposal.Id);
        Assert.Equal(
            SaveLifePatientProfileProposalStatuses.Confirmed,
            confirmation.Proposal.Status);
        Assert.Equal(journey.Diagnosis, confirmation.Patient.Diagnosis);
        Assert.Contains(
            confirmation.SuggestedActions,
            item =>
                item.Type == SaveLifeAgentSuggestedActionTypes.StartScreening &&
                item.Label == "查看相关项目" &&
                item.TargetType == "trial" &&
                item.TargetId == null);
        Assert.Equal(
            assistantMessagesBeforeConfirmation,
            await database.Queryable<SaveLifeAgentMessage>()
                .Where(item =>
                    item.ConversationId == journey.ConversationId &&
                    item.Role == "assistant" &&
                    item.GenerationStatus == "completed" &&
                    !item.IsDelete)
                .CountAsync(cancellationToken));

        var confirmedPatient = await database.Queryable<SaveLifePatientProfile>()
            .InSingleAsync(journey.PatientProfileId);
        Assert.NotNull(confirmedPatient);
        Assert.Equal(journey.Diagnosis, confirmedPatient.Diagnosis);
        var matchingGate = SaveLifeAgentTrialSelection.EvaluateMatchingGate(
            SaveLifePatientProfileMapper.ToOutput(confirmedPatient, journey.PatientProfileId));
        Assert.True(matchingGate.CanMatch);

        var matching = await RunScenarioAsync(
            client,
            database,
            new Scenario(
                $"{journey.Name}-matching",
                journey.ConversationId,
                "请根据我已经确认的健康档案，查找真实、正在招募且适合我情况的项目。我想报名。",
                RequiredTool: nameof(ClinicalTrialTool),
                RequiredAnswerTerms: [journey.Diagnosis, "项目", "招募", "报名"],
                ExpectGuidance: true,
                MinimumTrialHits: 1,
                RequiredTrialId: journey.TrialId.ToString(),
                OwnerExternalUserId: journey.OwnerExternalUserId),
            cancellationToken);

        var matchingMetadata = await ReadLatestMetadataAsync(
            database,
            journey.ConversationId,
            cancellationToken);
        Assert.Contains(
            matchingMetadata.MatchedTrials,
            item =>
                item.Id == journey.TrialId.ToString() &&
                item.ResultLevel == SaveLifeAgentTrialResultLevels.InitialMatch);
        Assert.Contains(
            matchingMetadata.SuggestedActions,
            item =>
                (item.Type == SaveLifeAgentSuggestedActionTypes.StartScreening &&
                 item.TargetType == "trial") ||
                (item.Type == SaveLifeAgentSuggestedActionTypes.UploadMaterial &&
                 item.TargetType == "material"));
        Assert.Equal(
            initialEnrollmentCount,
            await database.Queryable<SaveLifeEnrollmentRecord>()
                .Where(item =>
                    item.ExternalUserId == journey.OwnerExternalUserId &&
                    !item.IsDelete)
                .CountAsync(cancellationToken));

        return new ScenarioResult(
            journey.Name,
            assessment.AnswerCharacters + diagnosis.AnswerCharacters + matching.AnswerCharacters,
            assessment.DurationMilliseconds + diagnosis.DurationMilliseconds +
                matching.DurationMilliseconds,
            assessment.ToolNames
                .Concat(diagnosis.ToolNames)
                .Concat(matching.ToolNames)
                .Distinct(StringComparer.Ordinal)
                .ToList(),
            assessment.KnowledgeHits + diagnosis.KnowledgeHits + matching.KnowledgeHits,
            assessment.TrialHits + diagnosis.TrialHits + matching.TrialHits,
            assessment.MaterialHits + diagnosis.MaterialHits + matching.MaterialHits,
            matching.Model,
            matching.HasGuidance);
    }

    private static async Task<SaveLifePatientProfileProposalConfirmationOutput>
        ConfirmProfileProposalAsync(
            SqlSugarClient database,
            PersonaJourney journey,
            long proposalId)
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimConst.OpenId, journey.OwnerExternalUserId)],
                "live-acceptance"))
        };
        var service = new SaveLifePatientProfileService(
            SqlSugarRepository<SaveLifePatientProfile>.CreateForContext(database),
            SqlSugarRepository<SaveLifeAgentConversation>.CreateForContext(database),
            SqlSugarRepository<SaveLifeUserProfile>.CreateForContext(database),
            new SaveLifePrivacyConsentGuard(
                SqlSugarRepository<SaveLifeSetting>.CreateForContext(database),
                SqlSugarRepository<SaveLifeAccountDeletionRecord>.CreateForContext(database)),
            new SaveLifePatientIdGenerator(),
            new HttpContextAccessor { HttpContext = httpContext });
        return await service.ResolveProfileProposal(
            proposalId,
            new DecideSaveLifePatientProfileProposalInput { Decision = "confirm" });
    }

    private static async Task<SaveLifeAgentMessageMetadata> ReadLatestMetadataAsync(
        SqlSugarClient database,
        long conversationId,
        CancellationToken cancellationToken)
    {
        var assistant = await database.Queryable<SaveLifeAgentMessage>()
            .Where(item =>
                item.ConversationId == conversationId &&
                item.Role == "assistant" &&
                item.GenerationStatus == "completed" &&
                !item.IsDelete)
            .OrderBy(item => item.Id, OrderByType.Desc)
            .FirstAsync(cancellationToken);
        Assert.NotNull(assistant);
        var metadata = JsonSerializer.Deserialize<SaveLifeAgentMessageMetadata>(
            assistant.MetadataJson!,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.NotNull(metadata);
        return metadata;
    }

    private static List<JsonElement> ReadEvents(string body)
    {
        var events = new List<JsonElement>();
        foreach (var line in body.Split('\n'))
        {
            if (!line.StartsWith("data: ", StringComparison.Ordinal))
                continue;

            using var document = JsonDocument.Parse(line[6..].TrimEnd('\r'));
            events.Add(document.RootElement.Clone());
        }
        return events;
    }

    private static string EventType(JsonElement item) =>
        item.TryGetProperty("type", out var type)
            ? type.GetString() ?? string.Empty
            : string.Empty;

    private static string ReadFinalAssistantText(IReadOnlyList<JsonElement> events)
    {
        var messageOrder = new List<string>();
        var messageText = new Dictionary<string, StringBuilder>(StringComparer.Ordinal);
        foreach (var item in events)
        {
            var eventType = EventType(item);
            if (!item.TryGetProperty("messageId", out var messageIdValue))
                continue;
            var messageId = messageIdValue.GetString();
            if (string.IsNullOrWhiteSpace(messageId))
                continue;

            if (eventType == "TEXT_MESSAGE_START")
            {
                if (!messageText.ContainsKey(messageId))
                {
                    messageOrder.Add(messageId);
                    messageText[messageId] = new StringBuilder();
                }
                continue;
            }

            if (eventType == "TEXT_MESSAGE_CONTENT" &&
                item.TryGetProperty("delta", out var delta))
            {
                if (!messageText.TryGetValue(messageId, out var text))
                {
                    messageOrder.Add(messageId);
                    text = new StringBuilder();
                    messageText[messageId] = text;
                }
                text.Append(delta.GetString());
            }
        }

        return messageOrder
            .Select(messageId => messageText[messageId].ToString())
            .LastOrDefault(text => !string.IsNullOrWhiteSpace(text)) ?? string.Empty;
    }

    private static async Task WriteReportAsync(
        string path,
        IReadOnlyList<ScenarioResult> results)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var report = new
        {
            GeneratedAtUtc = DateTime.UtcNow,
            Passed = results.Count == 17,
            ScenarioCount = results.Count,
            Scenarios = results
        };
        await File.WriteAllTextAsync(
            path,
            JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
    }

    private static SqlSugarClient CreateDatabase(string path, bool readOnly)
    {
        return new SqlSugarClient(new ConnectionConfig
        {
            ConfigureExternalServices = new ConfigureExternalServices
            {
                EntityService = (property, column) =>
                {
                    if (new NullabilityInfoContext().Create(property).WriteState is
                        NullabilityState.Nullable)
                    {
                        column.IsNullable = true;
                    }
                }
            },
            ConnectionString = readOnly
                ? $"Data Source={path};Mode=ReadOnly;Pooling=False"
                : $"Data Source={path};Pooling=False",
            DbType = DbType.Sqlite,
            InitKeyType = InitKeyType.Attribute,
            IsAutoCloseConnection = true
        });
    }

    private static string RequireFileEnvironmentVariable(string variable)
    {
        var path = RequireEnvironmentVariable(variable);
        Assert.True(File.Exists(path), $"环境变量 {variable} 指向的文件不存在。");
        return path;
    }

    private static string RequireEnvironmentVariable(string variable)
    {
        var value = Environment.GetEnvironmentVariable(variable)?.Trim();
        Assert.False(string.IsNullOrWhiteSpace(value), $"环境变量 {variable} 未设置。");
        return ResolveRepositoryPath(value!);
    }

    private static string ResolveRepositoryPath(string path)
    {
        if (Path.IsPathFullyQualified(path))
            return Path.GetFullPath(path);

        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "src", "Jero.slnx")))
                return Path.GetFullPath(Path.Combine(directory.FullName, path));
            directory = directory.Parent;
        }
        throw new DirectoryNotFoundException("无法定位仓库根目录。");
    }

    private static async Task<string> ComputeFileHashAsync(string path)
    {
        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete,
            bufferSize: 81920,
            useAsync: true);
        return Convert.ToHexString(await SHA256.HashDataAsync(stream));
    }

    private static string Limit(string value, int maximumLength) =>
        value.Length <= maximumLength ? value : value[..maximumLength];

    private sealed class FixedMaterialFileReader(
        IReadOnlyDictionary<long, byte[]> files) : ISaveLifeAgentMaterialFileReader
    {
        public Task<Stream> OpenReadAsync(
            long fileId,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return files.TryGetValue(fileId, out var content)
                ? Task.FromResult<Stream>(new MemoryStream(content, writable: false))
                : Task.FromException<Stream>(new FileNotFoundException("测试病例文件不存在。"));
        }
    }

    private sealed record Scenario(
        string Name,
        long ConversationId,
        string Question,
        long? MaterialId = null,
        string? RequiredTool = null,
        IReadOnlyList<string>? RequiredAnswerTerms = null,
        bool ExpectGuidance = false,
        int MinimumKnowledgeHits = 0,
        int MinimumTrialHits = 0,
        int MinimumMaterialHits = 0,
        string? RequiredTrialId = null,
        IReadOnlyList<string>? ForbiddenTrialIds = null,
        string OwnerExternalUserId = Owner,
        bool ExpectQuestion = false,
        bool ExpectNoTools = false,
        bool ExpectQuestionBeforeDisposition = false)
    {
        public IReadOnlyList<string> ForbiddenTrialIds { get; } = ForbiddenTrialIds ?? [];

        public IReadOnlyList<string> RequiredAnswerTerms { get; } =
            RequiredAnswerTerms ?? [];
    }

    private sealed record PersonaJourney(
        string Name,
        string Occupation,
        string OwnerExternalUserId,
        long PatientProfileId,
        long ConversationId,
        string SymptomMessage,
        IReadOnlyList<string> SymptomAnswerTerms,
        string DiagnosisMessage,
        string Diagnosis,
        long TrialId);

    private sealed record ScenarioResult(
        string Name,
        int AnswerCharacters,
        long DurationMilliseconds,
        IReadOnlyList<string> ToolNames,
        int KnowledgeHits,
        int TrialHits,
        int MaterialHits,
        string Model,
        bool HasGuidance);
}
