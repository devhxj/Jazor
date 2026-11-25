using SqlSugar;
using System.Reflection;
using Jero.Plugin.SaveLife;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeDataIntegrityMigrationTests
{
    [Fact]
    public void AgentMessageIdentity_UniqueClientAndReplyKeys_RejectDuplicates()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = CreateSqliteClient(dbPath);
            db.CodeFirst.InitTables(typeof(SaveLifeAgentMessage));
            db.Insertable(new SaveLifeAgentMessage
            {
                Id = 1,
                ConversationId = 10,
                ExternalUserId = "openid-1",
                Role = "user",
                Content = "问题",
                ClientMessageId = "client-1",
                CreateTime = new DateTime(2026, 7, 11)
            }).ExecuteCommand();
            db.Insertable(new SaveLifeAgentMessage
            {
                Id = 2,
                ConversationId = 10,
                ExternalUserId = "openid-1",
                Role = "assistant",
                Content = "回答",
                ReplyToMessageId = 1,
                GenerationStatus = "completed",
                CreateTime = new DateTime(2026, 7, 11)
            }).ExecuteCommand();

            Assert.ThrowsAny<Exception>(() => db.Insertable(new SaveLifeAgentMessage
            {
                Id = 3,
                ConversationId = 10,
                ExternalUserId = "openid-1",
                Role = "user",
                Content = "重复问题",
                ClientMessageId = "client-1",
                CreateTime = new DateTime(2026, 7, 11)
            }).ExecuteCommand());
            Assert.ThrowsAny<Exception>(() => db.Insertable(new SaveLifeAgentMessage
            {
                Id = 4,
                ConversationId = 10,
                ExternalUserId = "openid-1",
                Role = "assistant",
                Content = "重复回答",
                ReplyToMessageId = 1,
                GenerationStatus = "completed",
                CreateTime = new DateTime(2026, 7, 11)
            }).ExecuteCommand());
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    [Fact]
    public async Task AtomicUpsert_WhenInsertLosesUniqueRace_ReloadsAndUpdatesWinner()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = CreateSqliteClient(dbPath);
            db.CodeFirst.InitTables(typeof(SaveLifeUserProfile));
            SaveLifeDataIntegrityMigration.Apply(db);
            var competingInsertAdded = false;

            var profile = await SaveLifeAtomicUpsert.ExecuteAsync(
                () => Task.FromResult<SaveLifeUserProfile?>(db.Queryable<SaveLifeUserProfile>()
                    .ClearFilter()
                    .First(u => u.ExternalUserId == "openid-race")),
                () => CreateProfile(2, "openid-race", "候选用户"),
                entity => entity.RealName = "最终用户",
                entity =>
                {
                    if (!competingInsertAdded)
                    {
                        db.Insertable(CreateProfile(1, "openid-race", "竞争用户")).ExecuteCommand();
                        competingInsertAdded = true;
                    }

                    db.Insertable(entity).ExecuteCommand();
                    return Task.CompletedTask;
                },
                entity =>
                {
                    db.Updateable(entity).ExecuteCommand();
                    return Task.CompletedTask;
                });

            Assert.Equal(1, profile.Id);
            Assert.Equal("最终用户", profile.RealName);
            Assert.Equal(1, db.Queryable<SaveLifeUserProfile>()
                .Count(u => u.ExternalUserId == "openid-race"));
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    [Fact]
    public void Apply_WithDuplicateProfilesAndEnrollments_MergesReferencesAndCreatesUniqueIndexes()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = CreateSqliteClient(dbPath);
            db.CodeFirst.InitTables(
                typeof(SaveLifeUserProfile),
                typeof(SaveLifeEnrollmentRecord),
                typeof(SaveLifeEnrollmentMaterial),
                typeof(SaveLifeAgentConversation),
                typeof(SaveLifeNotification));

            var older = new DateTime(2026, 1, 1);
            var newer = new DateTime(2026, 2, 1);
            db.Insertable(new[]
            {
                new SaveLifeUserProfile
                {
                    Id = 10,
                    ExternalUserId = "openid-1",
                    RealName = string.Empty,
                    Gender = string.Empty,
                    Phone = string.Empty,
                    Email = string.Empty,
                    Address = string.Empty,
                    Ethnicity = string.Empty,
                    EmergencyContact = string.Empty,
                    EmergencyPhone = string.Empty,
                    MedicalHistory = "既往病史 A",
                    Allergies = string.Empty,
                    CreateTime = older
                },
                new SaveLifeUserProfile
                {
                    Id = 11,
                    ExternalUserId = "openid-1",
                    RealName = "张三",
                    Gender = string.Empty,
                    Phone = string.Empty,
                    Email = string.Empty,
                    Address = string.Empty,
                    Ethnicity = string.Empty,
                    EmergencyContact = string.Empty,
                    EmergencyPhone = string.Empty,
                    MedicalHistory = string.Empty,
                    Allergies = string.Empty,
                    UpdateTime = newer,
                    CreateTime = newer
                }
            }).ExecuteCommand();
            db.Insertable(new[]
            {
                new SaveLifeEnrollmentRecord
                {
                    Id = 100,
                    UserProfileId = 10,
                    ExternalUserId = "openid-1",
                    ProjectId = 9001,
                    ProjectTitle = string.Empty,
                    ContactName = string.Empty,
                    ContactPhone = string.Empty,
                    Status = "cancelled",
                    AuditRemark = string.Empty,
                    DiseaseDescription = string.Empty,
                    MedicalHistory = "报名病史 A",
                    DiagnosisReport = string.Empty,
                    MedicationStatus = string.Empty,
                    OtherInfo = string.Empty,
                    Notes = string.Empty,
                    CreateTime = older
                },
                new SaveLifeEnrollmentRecord
                {
                    Id = 101,
                    UserProfileId = 11,
                    ExternalUserId = "openid-1",
                    ProjectId = 9001,
                    Status = "pending",
                    ContactName = "张三",
                    ProjectTitle = string.Empty,
                    ContactPhone = string.Empty,
                    AuditRemark = string.Empty,
                    DiseaseDescription = string.Empty,
                    MedicalHistory = string.Empty,
                    DiagnosisReport = string.Empty,
                    MedicationStatus = string.Empty,
                    OtherInfo = string.Empty,
                    Notes = string.Empty,
                    UpdateTime = newer,
                    CreateTime = newer
                }
            }).ExecuteCommand();
            db.Insertable(new SaveLifeEnrollmentMaterial
            {
                Id = 200,
                UserProfileId = 10,
                ExternalUserId = "openid-1",
                EnrollmentRecordId = 100,
                ProjectId = 9001,
                FileId = 1,
                FileName = "report.pdf",
                CreateTime = older
            }).ExecuteCommand();
            db.Insertable(new SaveLifeAgentConversation
            {
                Id = 300,
                ExternalUserId = "openid-1",
                EnrollmentRecordId = 100,
                ContextType = "enrollment",
                ContextId = 100,
                CreateTime = older
            }).ExecuteCommand();
            db.Insertable(new SaveLifeNotification
            {
                Id = 400,
                ExternalUserId = "openid-1",
                NotificationType = "enrollment",
                Title = "报名通知",
                BusinessType = "enrollment",
                BusinessId = 100,
                CreateTime = older
            }).ExecuteCommand();

            SaveLifeDataIntegrityMigration.Apply(db);
            SaveLifeDataIntegrityMigration.Apply(db);

            var activeProfile = db.Queryable<SaveLifeUserProfile>()
                .Where(u => u.ExternalUserId == "openid-1")
                .Single();
            Assert.Equal(11, activeProfile.Id);
            Assert.Equal("张三", activeProfile.RealName);
            Assert.Equal("既往病史 A", activeProfile.MedicalHistory);

            var activeEnrollment = db.Queryable<SaveLifeEnrollmentRecord>()
                .Where(u => u.ExternalUserId == "openid-1" && u.ProjectId == 9001)
                .Single();
            Assert.Equal(101, activeEnrollment.Id);
            Assert.Equal("pending", activeEnrollment.Status);
            Assert.Equal("报名病史 A", activeEnrollment.MedicalHistory);
            Assert.Equal(11, activeEnrollment.UserProfileId);

            var material = db.Queryable<SaveLifeEnrollmentMaterial>().Single(u => u.Id == 200);
            Assert.Equal(11, material.UserProfileId);
            Assert.Equal(101, material.EnrollmentRecordId);

            var conversation = db.Queryable<SaveLifeAgentConversation>().Single(u => u.Id == 300);
            Assert.Equal(101, conversation.EnrollmentRecordId);
            Assert.Equal(101, conversation.ContextId);

            var notification = db.Queryable<SaveLifeNotification>().Single(u => u.Id == 400);
            Assert.Equal(101, notification.BusinessId);

            var archivedProfiles = db.Queryable<SaveLifeUserProfile>()
                .Where(u => u.Id != activeProfile.Id)
                .ToList();
            Assert.Single(archivedProfiles);
            Assert.True(archivedProfiles[0].IsDelete);
            Assert.StartsWith("__merged__:", archivedProfiles[0].ExternalUserId);

            var archivedEnrollments = db.Queryable<SaveLifeEnrollmentRecord>()
                .Where(u => u.Id != activeEnrollment.Id)
                .ToList();
            Assert.Single(archivedEnrollments);
            Assert.True(archivedEnrollments[0].IsDelete);
            Assert.StartsWith("__merged__:", archivedEnrollments[0].ExternalUserId);

            Assert.ThrowsAny<Exception>(() => db.Insertable(new SaveLifeUserProfile
            {
                Id = 12,
                ExternalUserId = "openid-1",
                CreateTime = newer
            }).ExecuteCommand());
            Assert.ThrowsAny<Exception>(() => db.Insertable(new SaveLifeEnrollmentRecord
            {
                Id = 102,
                ExternalUserId = "openid-1",
                ProjectId = 9001,
                CreateTime = newer
            }).ExecuteCommand());
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    private static SqlSugarClient CreateSqliteClient(string dbPath)
    {
        return new SqlSugarClient(new ConnectionConfig
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
    }

    private static SaveLifeUserProfile CreateProfile(long id, string externalUserId, string realName)
    {
        return new SaveLifeUserProfile
        {
            Id = id,
            ExternalUserId = externalUserId,
            RealName = realName,
            Gender = string.Empty,
            Phone = string.Empty,
            Email = string.Empty,
            Address = string.Empty,
            Ethnicity = string.Empty,
            EmergencyContact = string.Empty,
            EmergencyPhone = string.Empty,
            MedicalHistory = string.Empty,
            Allergies = string.Empty,
            CreateTime = new DateTime(2026, 7, 11)
        };
    }
}
