using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using Jero.Plugin.SaveLife;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifePatientDataMigrationTests
{
    private static readonly HashSet<string> RehearsalTableNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "sl_user_profile",
        "sl_agent_conversation",
        "sl_agent_message",
        "sl_enrollment_record",
        "sl_enrollment_material",
        "sl_agent_vector_index"
    };

    [Fact]
    public void EnsureSchema_WithLegacyTables_AddsPatientOwnershipColumnsBeforeDataMigration()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = CreateSqliteClient(dbPath);
            InitHistoricalTables(db);
            DropColumn(db, "sl_agent_conversation", "CurrentPatientProfileId");
            DropColumn(db, "sl_agent_conversation", "PatientContextVersion");
            DropColumn(db, "sl_agent_conversation", "FlowStateVersion");
            DropColumn(db, "sl_agent_conversation", "FlowStateJson");
            DropColumn(db, "sl_agent_message", "PatientProfileId");
            DropColumn(db, "sl_agent_message", "PatientContextVersion");
            DropColumn(db, "sl_agent_message", "ContextSourceMessageId");
            DropColumn(
                db,
                "sl_enrollment_record",
                "PatientProfileId",
                "index_sl_enrollment_record_PT");
            DropColumn(
                db,
                "sl_enrollment_material",
                "PatientProfileId",
                "index_sl_enrollment_material_P");
            DropColumn(
                db,
                "sl_agent_vector_index",
                "PatientProfileId",
                "index_sl_agent_vector_index_PT");

            SaveLifePatientProfileMigration.EnsureSchema(db);

            Assert.True(HasTable(db, "sl_patient_profile"));
            Assert.True(HasTable(db, "sl_patient_profile_proposal"));
            Assert.True(HasColumn(db, "sl_agent_conversation", "CurrentPatientProfileId"));
            Assert.True(HasColumn(db, "sl_agent_conversation", "PatientContextVersion"));
            Assert.True(HasColumn(db, "sl_agent_conversation", "FlowStateVersion"));
            Assert.True(HasColumn(db, "sl_agent_conversation", "FlowStateJson"));
            Assert.True(HasColumn(db, "sl_agent_message", "PatientProfileId"));
            Assert.True(HasColumn(db, "sl_agent_message", "PatientContextVersion"));
            Assert.True(HasColumn(db, "sl_agent_message", "ContextSourceMessageId"));
            Assert.True(HasColumn(db, "sl_enrollment_record", "PatientProfileId"));
            Assert.True(HasColumn(db, "sl_enrollment_material", "PatientProfileId"));
            Assert.True(HasColumn(db, "sl_agent_vector_index", "PatientProfileId"));
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    [Fact]
    public void EnsureSchema_WithLegacyVectorIndex_AddsVersionColumnsIdempotently()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = CreateSqliteClient(dbPath);
            InitHistoricalTables(db);
            DropColumn(db, "sl_agent_vector_index", nameof(SaveLifeAgentVectorIndex.EmbeddingConfiguredModel));
            DropColumn(db, "sl_agent_vector_index", nameof(SaveLifeAgentVectorIndex.EmbeddingDimensions));
            DropColumn(db, "sl_agent_vector_index", nameof(SaveLifeAgentVectorIndex.SourceContentSha256));

            SaveLifePatientProfileMigration.EnsureSchema(db);
            SaveLifePatientProfileMigration.EnsureSchema(db);

            Assert.True(HasColumn(db, "sl_agent_vector_index", nameof(SaveLifeAgentVectorIndex.EmbeddingConfiguredModel)));
            Assert.True(HasColumn(db, "sl_agent_vector_index", nameof(SaveLifeAgentVectorIndex.EmbeddingDimensions)));
            Assert.True(HasColumn(db, "sl_agent_vector_index", nameof(SaveLifeAgentVectorIndex.SourceContentSha256)));
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    [Fact]
    public void Apply_WithLegacyConversationRows_BackfillsScopeWithoutCopyingMessageContent()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = CreateSqliteClient(dbPath);
            InitHistoricalTables(db);
            db.Insertable(CreateUserProfile(11, "openid-1")).ExecuteCommand();
            db.Insertable(new SaveLifeAgentConversation
            {
                Id = 401,
                ExternalUserId = "openid-1",
                Status = "active",
                Phase = "created",
                CreateTime = new DateTime(2026, 7, 14)
            }).ExecuteCommand();
            db.Insertable(new SaveLifeAgentMessage
            {
                Id = 501,
                ConversationId = 401,
                ExternalUserId = "openid-1",
                Role = "user",
                Content = "仅用于验证旧消息正文保持不变",
                ClientMessageId = "legacy-message-1",
                CreateTime = new DateTime(2026, 7, 14)
            }).ExecuteCommand();

            DropColumn(db, "sl_agent_conversation", "CurrentPatientProfileId");
            DropColumn(db, "sl_agent_conversation", "PatientContextVersion");
            DropColumn(db, "sl_agent_conversation", "FlowStateVersion");
            DropColumn(db, "sl_agent_conversation", "FlowStateJson");
            DropColumn(db, "sl_agent_message", "PatientProfileId");
            DropColumn(db, "sl_agent_message", "PatientContextVersion");
            DropColumn(db, "sl_agent_message", "ContextSourceMessageId");

            SaveLifePatientProfileMigration.Apply(db);
            SaveLifePatientProfileMigration.Apply(db);

            var patient = db.Queryable<SaveLifePatientProfile>().ClearFilter().Single();
            var conversation = db.Queryable<SaveLifeAgentConversation>().ClearFilter().Single();
            var message = db.Queryable<SaveLifeAgentMessage>().ClearFilter().Single();
            Assert.Equal(patient.Id, conversation.CurrentPatientProfileId);
            Assert.Equal(1, conversation.PatientContextVersion);
            Assert.Equal(
                SaveLifeAgentConversationState.CurrentVersion,
                conversation.FlowStateVersion);
            Assert.Null(conversation.FlowStateJson);
            Assert.Equal(patient.Id, message.PatientProfileId);
            Assert.Equal(conversation.PatientContextVersion, message.PatientContextVersion);
            Assert.Null(message.ContextSourceMessageId);
            Assert.Equal("仅用于验证旧消息正文保持不变", message.Content);
            Assert.Equal("legacy-message-1", message.ClientMessageId);
            Assert.Single(db.Queryable<SaveLifePatientProfile>().ClearFilter().ToList());
            Assert.Single(db.Queryable<SaveLifeAgentMessage>().ClearFilter().ToList());
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    [Fact]
    public void Apply_WithHistoricalPatientData_BackfillsOwnershipAndReplacesEnrollmentUniqueness()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = CreateSqliteClient(dbPath);
            InitHistoricalTables(db);
            db.Insertable(CreateUserProfile(11, "openid-1")).ExecuteCommand();
            db.Insertable(CreateEnrollment(101, "openid-1", 9001)).ExecuteCommand();
            db.Insertable(CreateMaterial(201, 101, "openid-1", 9001)).ExecuteCommand();
            db.Insertable(CreatePrivateIndex(301, 201, "openid-1")).ExecuteCommand();

            var firstReport = SaveLifePatientProfileMigration.Apply(db);
            var secondReport = SaveLifePatientProfileMigration.Apply(db);

            var patient = db.Queryable<SaveLifePatientProfile>().ClearFilter().Single();
            Assert.Equal(patient.Id, db.Queryable<SaveLifeEnrollmentRecord>().Single().PatientProfileId);
            Assert.Equal(patient.Id, db.Queryable<SaveLifeEnrollmentMaterial>().Single().PatientProfileId);
            Assert.Equal(patient.Id, db.Queryable<SaveLifeAgentVectorIndex>().Single().PatientProfileId);
            Assert.Equal(1, firstReport.EnrollmentBackfilledCount);
            Assert.Equal(1, firstReport.MaterialBackfilledCount);
            Assert.Equal(1, firstReport.PrivateIndexBackfilledCount);
            Assert.Equal(0, secondReport.TotalBackfilledCount);
            Assert.Empty(firstReport.EnrollmentConflicts);

            var otherPatient = CreateOtherPatient(12, "openid-1", "母亲");
            db.Insertable(otherPatient).ExecuteCommand();
            var otherEnrollment = CreateEnrollment(102, "openid-1", 9001);
            otherEnrollment.PatientProfileId = otherPatient.Id;
            db.Insertable(otherEnrollment).ExecuteCommand();

            var duplicate = CreateEnrollment(103, "openid-1", 9001);
            duplicate.PatientProfileId = otherPatient.Id;
            Assert.ThrowsAny<Exception>(() => db.Insertable(duplicate).ExecuteCommand());
            Assert.Equal(2, db.Queryable<SaveLifeEnrollmentRecord>().Count());
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    [Fact]
    public void CurrentSchema_DuplicatePatientResumeAndEnrollmentWritesAreRejected()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = CreateSqliteClient(dbPath);
            InitHistoricalTables(db);
            SaveLifePatientProfileMigration.Apply(db);

            var accountPatientKey = SaveLifePatientOnboardingPolicy.BuildAccountPatientKey(
                "openid-1",
                "create-father-evaluation-1");
            var patient = CreateOtherPatient(501, "openid-1", "父亲");
            patient.AccountPatientKey = accountPatientKey;
            db.Insertable(patient).ExecuteCommand();
            var duplicatePatient = CreateOtherPatient(502, "openid-1", "父亲");
            duplicatePatient.AccountPatientKey = accountPatientKey;
            Assert.ThrowsAny<Exception>(() => db.Insertable(duplicatePatient).ExecuteCommand());

            db.Insertable(new SaveLifeAgentConversation
            {
                Id = 401,
                ExternalUserId = "openid-1",
                CurrentPatientProfileId = patient.Id,
                PatientContextVersion = 3,
                Status = "active",
                CreateTime = new DateTime(2026, 7, 15)
            }).ExecuteCommand();
            var resumedMessage = new SaveLifeAgentMessage
            {
                Id = 601,
                ConversationId = 401,
                ExternalUserId = "openid-1",
                PatientProfileId = patient.Id,
                PatientContextVersion = 3,
                ContextSourceMessageId = 501,
                Role = "user",
                Content = "恢复待继续咨询",
                ClientMessageId = "resume:7001:patient:501",
                CreateTime = new DateTime(2026, 7, 15)
            };
            db.Insertable(resumedMessage).ExecuteCommand();
            var duplicateResume = new SaveLifeAgentMessage
            {
                Id = 602,
                ConversationId = resumedMessage.ConversationId,
                ExternalUserId = resumedMessage.ExternalUserId,
                PatientProfileId = resumedMessage.PatientProfileId,
                PatientContextVersion = resumedMessage.PatientContextVersion,
                ContextSourceMessageId = resumedMessage.ContextSourceMessageId,
                Role = resumedMessage.Role,
                Content = resumedMessage.Content,
                ClientMessageId = resumedMessage.ClientMessageId,
                CreateTime = resumedMessage.CreateTime
            };
            Assert.ThrowsAny<Exception>(() => db.Insertable(duplicateResume).ExecuteCommand());

            var enrollment = CreateEnrollment(701, "openid-1", 9001);
            enrollment.PatientProfileId = patient.Id;
            db.Insertable(enrollment).ExecuteCommand();
            var duplicateEnrollment = CreateEnrollment(702, "openid-1", 9001);
            duplicateEnrollment.PatientProfileId = patient.Id;
            Assert.ThrowsAny<Exception>(() => db.Insertable(duplicateEnrollment).ExecuteCommand());

            Assert.Single(db.Queryable<SaveLifePatientProfile>().ClearFilter().ToList());
            Assert.Single(db.Queryable<SaveLifeAgentMessage>().ClearFilter().ToList());
            Assert.Single(db.Queryable<SaveLifeEnrollmentRecord>().ClearFilter().ToList());
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    [Fact]
    public void Apply_WithEnrollmentAlreadyBoundToOtherPatient_PropagatesPatientToMaterialAndIndex()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = CreateSqliteClient(dbPath);
            InitHistoricalTables(db);
            db.CodeFirst.InitTables(typeof(SaveLifePatientProfile));
            db.Insertable(CreateUserProfile(11, "openid-1")).ExecuteCommand();
            var self = SaveLifePatientProfileFactory.CreateSelf(
                CreateUserProfile(11, "openid-1"),
                "openid-1");
            var parent = CreateOtherPatient(12, "openid-1", "母亲");
            db.Insertable(new[] { self, parent }).ExecuteCommand();
            var enrollment = CreateEnrollment(101, "openid-1", 9001);
            enrollment.PatientProfileId = parent.Id;
            db.Insertable(enrollment).ExecuteCommand();
            db.Insertable(CreateMaterial(201, 101, "openid-1", 9001)).ExecuteCommand();
            db.Insertable(CreatePrivateIndex(301, 201, "openid-1")).ExecuteCommand();

            SaveLifePatientProfileMigration.Apply(db);

            Assert.Equal(parent.Id, db.Queryable<SaveLifeEnrollmentRecord>().Single().PatientProfileId);
            Assert.Equal(parent.Id, db.Queryable<SaveLifeEnrollmentMaterial>().Single().PatientProfileId);
            Assert.Equal(parent.Id, db.Queryable<SaveLifeAgentVectorIndex>().Single().PatientProfileId);
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    [Fact]
    public void Apply_WithProspectivePatientProjectConflict_ReturnsReportAndRollsBack()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = CreateSqliteClient(dbPath);
            db.CodeFirst.InitTables(typeof(SaveLifeUserProfile), typeof(SaveLifeEnrollmentRecord));
            db.Insertable(CreateUserProfile(11, "openid-1")).ExecuteCommand();
            db.Insertable(new[]
            {
                CreateEnrollment(101, "openid-1", 9001),
                CreateEnrollment(102, "openid-1", 9001)
            }).ExecuteCommand();

            var error = Assert.Throws<SaveLifePatientDataMigrationConflictException>(() =>
                SaveLifePatientProfileMigration.Apply(db));

            var conflict = Assert.Single(error.Report.EnrollmentConflicts);
            Assert.Equal(9001, conflict.ProjectId);
            Assert.Equal(new long[] { 101, 102 }, conflict.EnrollmentRecordIds.OrderBy(u => u));
            Assert.Empty(db.Queryable<SaveLifePatientProfile>().ClearFilter().ToList());
            Assert.All(
                db.Queryable<SaveLifeEnrollmentRecord>().ClearFilter().ToList(),
                enrollment => Assert.Null(enrollment.PatientProfileId));
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    [Fact]
    public void Apply_WhenPatientDataBackfillFails_RollsBackAllPatientOwnership()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = CreateSqliteClient(dbPath);
            InitHistoricalTables(db);
            db.Insertable(CreateUserProfile(11, "openid-1")).ExecuteCommand();
            db.Insertable(CreateEnrollment(101, "openid-1", 9001)).ExecuteCommand();
            db.Insertable(CreateMaterial(201, 101, "openid-1", 9001)).ExecuteCommand();
            db.Insertable(CreatePrivateIndex(301, 201, "openid-1")).ExecuteCommand();
            db.Ado.ExecuteCommand("""
                CREATE TRIGGER fail_material_patient_backfill
                BEFORE UPDATE OF PatientProfileId ON sl_enrollment_material
                WHEN NEW.PatientProfileId IS NOT NULL
                BEGIN
                    SELECT RAISE(ABORT, 'forced patient data migration failure');
                END;
                """);

            Assert.ThrowsAny<Exception>(() => SaveLifePatientProfileMigration.Apply(db));

            Assert.Empty(db.Queryable<SaveLifePatientProfile>().ClearFilter().ToList());
            Assert.Null(db.Queryable<SaveLifeEnrollmentRecord>().Single().PatientProfileId);
            Assert.Null(db.Queryable<SaveLifeEnrollmentMaterial>().Single().PatientProfileId);
            Assert.Null(db.Queryable<SaveLifeAgentVectorIndex>().Single().PatientProfileId);
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    [Fact]
    public void IntegrityMigration_WithSameAccountDifferentPatients_PreservesBothEnrollments()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = CreateSqliteClient(dbPath);
            db.CodeFirst.InitTables(typeof(SaveLifeEnrollmentRecord), typeof(SaveLifePatientProfile));
            var self = CreateOtherPatient(11, "openid-1", "本人");
            self.AccountPatientKey = SaveLifePatientProfileKeys.ForSelf("openid-1");
            self.RelationshipToAccount = SaveLifePatientRelationships.Self;
            self.IsDefault = true;
            var parent = CreateOtherPatient(12, "openid-1", "父亲");
            db.Insertable(new[] { self, parent }).ExecuteCommand();
            var selfEnrollment = CreateEnrollment(101, "openid-1", 9001);
            selfEnrollment.PatientProfileId = self.Id;
            var parentEnrollment = CreateEnrollment(102, "openid-1", 9001);
            parentEnrollment.PatientProfileId = parent.Id;
            db.Insertable(new[] { selfEnrollment, parentEnrollment }).ExecuteCommand();

            SaveLifeDataIntegrityMigration.Apply(db);
            SaveLifePatientProfileMigration.Apply(db);

            var enrollments = db.Queryable<SaveLifeEnrollmentRecord>().ClearFilter().ToList();
            Assert.Equal(2, enrollments.Count);
            Assert.All(enrollments, enrollment => Assert.False(enrollment.IsDelete));
            Assert.Equal(2, enrollments.Select(u => u.PatientProfileId).Distinct().Count());
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    [Fact]
    public void Apply_OnDesensitizedProductionStructureCopy_RehearsesReleaseAndFailureRecovery()
    {
        var sourcePath = ResolveProductionStructureDatabase();
        if (!File.Exists(sourcePath))
        {
            Assert.Skip(
                "未找到本地生产结构源；可通过 SAVELIFE_LEGACY_SCHEMA_DB 指向只读的迁移前 SQLite 副本。测试只读取 sqlite_master，不读取业务数据。");
        }
        if (!IsLegacyPatientSchema(sourcePath))
        {
            Assert.Skip(
                "结构源已经包含患者归属迁移；请通过 SAVELIFE_LEGACY_SCHEMA_DB 指向迁移前的只读备份后重新演练。");
        }

        var successPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        var failurePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            CloneRelevantSchema(sourcePath, successPath);
            using (var db = CreateSqliteClient(successPath))
            {
                Assert.False(HasTable(db, "sl_patient_profile"));
                SaveLifePatientProfileMigration.EnsureSchema(db);
                SeedSyntheticLegacyPatientData(db);
                var firstConversationReport = SaveLifeAgentConversationMigration.Apply(db);
                var secondConversationReport = SaveLifeAgentConversationMigration.Apply(db);
                SaveLifeAgentTurnMigration.Apply(db);
                SaveLifeAgentTurnMigration.Apply(db);
                SaveLifeDataIntegrityMigration.Apply(db);

                var firstReport = SaveLifePatientProfileMigration.Apply(db);
                var secondReport = SaveLifePatientProfileMigration.Apply(db);

                var patient = db.Queryable<SaveLifePatientProfile>().ClearFilter().Single();
                var conversation = db.Queryable<SaveLifeAgentConversation>().Single();
                var message = db.Queryable<SaveLifeAgentMessage>().Single();
                Assert.Equal(1, firstReport.EnrollmentBackfilledCount);
                Assert.Equal(1, firstReport.MaterialBackfilledCount);
                Assert.Equal(1, firstReport.PrivateIndexBackfilledCount);
                Assert.Equal(0, secondReport.TotalBackfilledCount);
                Assert.Equal(0, firstConversationReport.AccountConflictCount);
                Assert.Equal(0, secondConversationReport.AccountConflictCount);
                Assert.True(HasIndex(
                    db,
                    SaveLifeAgentConversationMigration.ActiveAccountUniqueIndexName));
                Assert.True(HasIndex(
                    db,
                    SaveLifeAgentTurnMigration.TurnSequenceUniqueIndexName));
                Assert.Equal(patient.Id, conversation.CurrentPatientProfileId);
                Assert.Equal(1, conversation.PatientContextVersion);
                Assert.Equal(
                    SaveLifeAgentConversationState.CurrentVersion,
                    conversation.FlowStateVersion);
                Assert.Null(conversation.FlowStateJson);
                Assert.Equal(patient.Id, message.PatientProfileId);
                Assert.Equal(conversation.PatientContextVersion, message.PatientContextVersion);
                Assert.Null(message.ContextSourceMessageId);
                Assert.Equal("合成迁移演练消息", message.Content);
                Assert.Equal(
                    patient.Id,
                    db.Queryable<SaveLifeEnrollmentRecord>().Single().PatientProfileId);
                Assert.Equal(
                    patient.Id,
                    db.Queryable<SaveLifeEnrollmentMaterial>().Single().PatientProfileId);
                Assert.Equal(
                    patient.Id,
                    db.Queryable<SaveLifeAgentVectorIndex>().Single().PatientProfileId);
                Assert.False(HasIndex(
                    db,
                    SaveLifeDataIntegrityMigration.EnrollmentUniqueIndexName));
                Assert.True(HasIndex(
                    db,
                    SaveLifePatientProfileMigration.EnrollmentPatientUniqueIndexName));
                Assert.True(HasColumn(db, "sl_user_profile", nameof(SaveLifeUserProfile.MedicalHistory)));
                Assert.True(HasColumn(db, "sl_user_profile", nameof(SaveLifeUserProfile.Allergies)));
            }

            CloneRelevantSchema(sourcePath, failurePath);
            using (var db = CreateSqliteClient(failurePath))
            {
                SaveLifePatientProfileMigration.EnsureSchema(db);
                SeedSyntheticLegacyPatientData(db);
                SaveLifeAgentConversationMigration.Apply(db);
                SaveLifeAgentTurnMigration.Apply(db);
                db.Ado.ExecuteCommand("""
                    CREATE TRIGGER fail_rehearsal_material_backfill
                    BEFORE UPDATE OF PatientProfileId ON sl_enrollment_material
                    WHEN NEW.PatientProfileId IS NOT NULL
                    BEGIN
                        SELECT RAISE(ABORT, 'forced release rehearsal failure');
                    END;
                    """);

                Assert.ThrowsAny<Exception>(() => SaveLifePatientProfileMigration.Apply(db));

                var conversation = db.Queryable<SaveLifeAgentConversation>().Single();
                var message = db.Queryable<SaveLifeAgentMessage>().Single();
                Assert.Empty(db.Queryable<SaveLifePatientProfile>().ClearFilter().ToList());
                Assert.Null(conversation.CurrentPatientProfileId);
                Assert.Null(conversation.FlowStateVersion);
                Assert.Null(conversation.FlowStateJson);
                Assert.Equal(1, conversation.PatientContextVersion);
                Assert.Null(message.PatientProfileId);
                Assert.Null(message.ContextSourceMessageId);
                Assert.Equal(conversation.PatientContextVersion, message.PatientContextVersion);
                Assert.Equal("合成迁移演练消息", message.Content);
                Assert.Null(db.Queryable<SaveLifeEnrollmentRecord>().Single().PatientProfileId);
                Assert.Null(db.Queryable<SaveLifeEnrollmentMaterial>().Single().PatientProfileId);
                Assert.Null(db.Queryable<SaveLifeAgentVectorIndex>().Single().PatientProfileId);
                Assert.True(HasIndex(
                    db,
                    SaveLifeDataIntegrityMigration.EnrollmentUniqueIndexName));
                Assert.False(HasIndex(
                    db,
                    SaveLifePatientProfileMigration.EnrollmentPatientUniqueIndexName));
                Assert.True(HasIndex(
                    db,
                    SaveLifeAgentConversationMigration.ActiveAccountUniqueIndexName));
            }
        }
        finally
        {
            if (File.Exists(successPath))
                File.Delete(successPath);
            if (File.Exists(failurePath))
                File.Delete(failurePath);
        }
    }

    [Fact]
    public void CompatibilitySurface_LegacyProfileFieldsAndMiniAppRoutesRemainAvailable()
    {
        Assert.NotNull(typeof(SaveLifeUserProfile).GetProperty(nameof(SaveLifeUserProfile.RealName)));
        Assert.NotNull(typeof(SaveLifeUserProfile).GetProperty(nameof(SaveLifeUserProfile.Gender)));
        Assert.NotNull(typeof(SaveLifeUserProfile).GetProperty(nameof(SaveLifeUserProfile.BirthDate)));
        Assert.NotNull(typeof(SaveLifeUserProfile).GetProperty(nameof(SaveLifeUserProfile.MedicalHistory)));
        Assert.NotNull(typeof(SaveLifeUserProfile).GetProperty(nameof(SaveLifeUserProfile.Allergies)));

        var getMethod = typeof(SaveLifeUserProfileService).GetMethod(
            nameof(SaveLifeUserProfileService.MiniAppProfile));
        var updateMethod = typeof(SaveLifeUserProfileService).GetMethod(
            nameof(SaveLifeUserProfileService.MiniAppUpdateProfile));
        var getRoute = getMethod?.GetCustomAttribute<HttpGetAttribute>();
        var updateRoute = updateMethod?.GetCustomAttribute<HttpPostAttribute>();

        Assert.Equal("/api/v1/save-life/users/miniapp/profile", getRoute?.Template);
        Assert.Equal("/api/v1/save-life/users/miniapp/profile", updateRoute?.Template);
    }

    private static string ResolveProductionStructureDatabase()
    {
        var configured = Environment.GetEnvironmentVariable("SAVELIFE_LEGACY_SCHEMA_DB");
        if (!string.IsNullOrWhiteSpace(configured))
            return Path.GetFullPath(configured.Trim());

        return Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "Jero",
            "Jero.db"));
    }

    private static void CloneRelevantSchema(string sourcePath, string destinationPath)
    {
        using var source = CreateSqliteClient(sourcePath, readOnly: true);
        var definitions = source.Ado.SqlQuery<SqliteSchemaDefinition>("""
            SELECT type AS Type, name AS Name, tbl_name AS TableName, sql AS Sql
            FROM sqlite_master
            WHERE type IN ('table', 'index') AND sql IS NOT NULL
            ORDER BY CASE type WHEN 'table' THEN 0 ELSE 1 END, name
            """)
            .Where(item =>
                RehearsalTableNames.Contains(item.Name) ||
                RehearsalTableNames.Contains(item.TableName))
            .ToList();
        Assert.Equal(RehearsalTableNames.Count, definitions.Count(item => item.Type == "table"));

        using var destination = CreateSqliteClient(destinationPath);
        foreach (var definition in definitions)
            destination.Ado.ExecuteCommand(definition.Sql);
    }

    private static bool IsLegacyPatientSchema(string sourcePath)
    {
        using var source = CreateSqliteClient(sourcePath, readOnly: true);
        return !HasTable(source, "sl_patient_profile") &&
            !HasColumn(source, "sl_enrollment_record", nameof(SaveLifeEnrollmentRecord.PatientProfileId));
    }

    private static void SeedSyntheticLegacyPatientData(ISqlSugarClient db)
    {
        db.Insertable(CreateUserProfile(11, "migration-rehearsal-openid")).ExecuteCommand();
        db.Insertable(new SaveLifeAgentConversation
        {
            Id = 401,
            ExternalUserId = "migration-rehearsal-openid",
            Status = "active",
            Phase = "created",
            CreateTime = new DateTime(2026, 7, 14)
        }).ExecuteCommand();
        db.Insertable(new SaveLifeAgentMessage
        {
            Id = 501,
            ConversationId = 401,
            ExternalUserId = "migration-rehearsal-openid",
            Role = "user",
            Content = "合成迁移演练消息",
            ClientMessageId = "migration-rehearsal-message",
            CreateTime = new DateTime(2026, 7, 14)
        }).ExecuteCommand();
        db.Insertable(CreateEnrollment(101, "migration-rehearsal-openid", 9001)).ExecuteCommand();
        db.Insertable(CreateMaterial(201, 101, "migration-rehearsal-openid", 9001)).ExecuteCommand();
        db.Insertable(CreatePrivateIndex(301, 201, "migration-rehearsal-openid")).ExecuteCommand();
    }

    private static void InitHistoricalTables(ISqlSugarClient db)
    {
        db.CodeFirst.InitTables(
            typeof(SaveLifeUserProfile),
            typeof(SaveLifeAgentConversation),
            typeof(SaveLifeAgentMessage),
            typeof(SaveLifeEnrollmentRecord),
            typeof(SaveLifeEnrollmentMaterial),
            typeof(SaveLifeAgentVectorIndex));
    }

    private static void DropColumn(
        ISqlSugarClient db,
        string tableName,
        string columnName,
        string? indexName = null)
    {
        if (!string.IsNullOrWhiteSpace(indexName))
            db.Ado.ExecuteCommand($"DROP INDEX IF EXISTS \"{indexName}\"");
        db.Ado.ExecuteCommand($"ALTER TABLE \"{tableName}\" DROP COLUMN \"{columnName}\"");
    }

    private static bool HasTable(ISqlSugarClient db, string tableName)
    {
        return db.DbMaintenance.IsAnyTable(tableName, false);
    }

    private static bool HasColumn(ISqlSugarClient db, string tableName, string columnName)
    {
        return db.DbMaintenance.GetColumnInfosByTableName(tableName, false)
            .Any(u => u.DbColumnName.Equals(columnName, StringComparison.OrdinalIgnoreCase));
    }

    private static bool HasIndex(ISqlSugarClient db, string indexName)
    {
        return db.Ado.SqlQuery<string>("SELECT name FROM sqlite_master WHERE type = 'index'")
            .Contains(indexName, StringComparer.OrdinalIgnoreCase);
    }

    private static SaveLifeUserProfile CreateUserProfile(long id, string externalUserId)
    {
        return new SaveLifeUserProfile
        {
            Id = id,
            ExternalUserId = externalUserId,
            RealName = "本人",
            CreateTime = new DateTime(2026, 7, 14)
        };
    }

    private static SaveLifePatientProfile CreateOtherPatient(long id, string externalUserId, string relationship)
    {
        return new SaveLifePatientProfile
        {
            Id = id,
            OwnerExternalUserId = externalUserId,
            AccountPatientKey = SaveLifePatientProfileKeys.ForOther(id),
            RelationshipToAccount = SaveLifePatientRelationships.Parent,
            RelationshipDescription = relationship,
            DisplayName = relationship,
            CreateTime = new DateTime(2026, 7, 14)
        };
    }

    private static SaveLifeEnrollmentRecord CreateEnrollment(long id, string externalUserId, long projectId)
    {
        return new SaveLifeEnrollmentRecord
        {
            Id = id,
            ExternalUserId = externalUserId,
            ProjectId = projectId,
            Status = "pending",
            CreateTime = new DateTime(2026, 7, 14)
        };
    }

    private static SaveLifeEnrollmentMaterial CreateMaterial(
        long id,
        long enrollmentRecordId,
        string externalUserId,
        long projectId)
    {
        return new SaveLifeEnrollmentMaterial
        {
            Id = id,
            EnrollmentRecordId = enrollmentRecordId,
            ExternalUserId = externalUserId,
            ProjectId = projectId,
            FileId = 1,
            FileName = "report.pdf",
            CreateTime = new DateTime(2026, 7, 14)
        };
    }

    private static SaveLifeAgentVectorIndex CreatePrivateIndex(
        long id,
        long materialId,
        string externalUserId)
    {
        return new SaveLifeAgentVectorIndex
        {
            Id = id,
            ResourceType = "material",
            ResourceId = materialId,
            ExternalUserId = externalUserId,
            Title = "检查报告",
            CreateTime = new DateTime(2026, 7, 14)
        };
    }

    private static SqlSugarClient CreateSqliteClient(string dbPath, bool readOnly = false)
    {
        return new SqlSugarClient(new ConnectionConfig
        {
            ConnectionString = readOnly
                ? $"Data Source={dbPath};Mode=ReadOnly;Pooling=False"
                : $"Data Source={dbPath};Pooling=False",
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
    }

    private sealed class SqliteSchemaDefinition
    {
        public string Name { get; init; } = string.Empty;

        public string Sql { get; init; } = string.Empty;

        public string TableName { get; init; } = string.Empty;

        public string Type { get; init; } = string.Empty;
    }
}
