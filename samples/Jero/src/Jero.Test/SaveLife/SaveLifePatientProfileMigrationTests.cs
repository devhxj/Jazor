using System.Reflection;
using SqlSugar;
using Jero.Plugin.SaveLife;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifePatientProfileMigrationTests
{
    [Fact]
    public void Apply_WithLegacyProposal_AddsSourceColumnsAndBackfillsUserMessage()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = CreateSqliteClient(dbPath);
            db.CodeFirst.InitTables(typeof(SaveLifePatientProfileProposal));
            db.Insertable(new SaveLifePatientProfileProposal
            {
                Id = 101,
                OwnerExternalUserId = "openid-1",
                ConversationId = 201,
                PatientProfileId = 301,
                SourceUserMessageId = 401,
                SourceAssistantMessageId = 402,
                FieldName = SaveLifePatientProfileProposalFields.Diagnosis,
                ProposedValue = "肺腺癌",
                Evidence = "肺腺癌",
                Status = SaveLifePatientProfileProposalStatuses.Pending,
                ExpiresTime = new DateTime(2026, 7, 26),
                CreateTime = new DateTime(2026, 7, 19)
            }).ExecuteCommand();
            DropColumn(db, "sl_patient_profile_proposal", nameof(SaveLifePatientProfileProposal.SourceMaterialId));
            DropColumn(db, "sl_patient_profile_proposal", nameof(SaveLifePatientProfileProposal.SourceType));

            SaveLifePatientProfileMigration.Apply(db);
            SaveLifePatientProfileMigration.Apply(db);

            Assert.True(HasColumn(
                db,
                "sl_patient_profile_proposal",
                nameof(SaveLifePatientProfileProposal.SourceType)));
            Assert.True(HasColumn(
                db,
                "sl_patient_profile_proposal",
                nameof(SaveLifePatientProfileProposal.SourceMaterialId)));
            var proposal = db.Queryable<SaveLifePatientProfileProposal>().ClearFilter().Single();
            Assert.Equal(SaveLifePatientProfileProposalSources.UserMessage, proposal.SourceType);
            Assert.Null(proposal.SourceMaterialId);
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    [Fact]
    public void Apply_WithBlankProposalSource_BackfillsUserMessage()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = CreateSqliteClient(dbPath);
            db.CodeFirst.InitTables(typeof(SaveLifePatientProfileProposal));
            db.Insertable(new SaveLifePatientProfileProposal
            {
                Id = 101,
                OwnerExternalUserId = "openid-1",
                ConversationId = 201,
                PatientProfileId = 301,
                SourceUserMessageId = 401,
                SourceAssistantMessageId = 402,
                SourceType = string.Empty,
                FieldName = SaveLifePatientProfileProposalFields.Diagnosis,
                ProposedValue = "肺腺癌",
                Evidence = "肺腺癌",
                Status = SaveLifePatientProfileProposalStatuses.Pending,
                ExpiresTime = new DateTime(2026, 7, 26),
                CreateTime = new DateTime(2026, 7, 19)
            }).ExecuteCommand();

            SaveLifePatientProfileMigration.Apply(db);

            var proposal = db.Queryable<SaveLifePatientProfileProposal>().ClearFilter().Single();
            Assert.Equal(SaveLifePatientProfileProposalSources.UserMessage, proposal.SourceType);
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    [Fact]
    public void Apply_WithEmptyDatabase_CreatesPatientTableWithoutRows()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = CreateSqliteClient(dbPath);

            SaveLifePatientProfileMigration.Apply(db);

            Assert.True(db.DbMaintenance.IsAnyTable("sl_patient_profile", false));
            Assert.True(db.DbMaintenance.IsAnyTable("sl_patient_profile_proposal", false));
            Assert.Empty(db.Queryable<SaveLifePatientProfile>().ClearFilter().ToList());
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    [Fact]
    public void Apply_WithHistoricalAccount_CreatesOneSelfPatientAndDoesNotChangeSource()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = CreateSqliteClient(dbPath);
            db.CodeFirst.InitTables(typeof(SaveLifeUserProfile));
            var source = new SaveLifeUserProfile
            {
                Id = 101,
                ExternalUserId = "openid-1",
                RealName = "张三",
                Gender = "male",
                BirthDate = new DateTime(1980, 3, 2),
                Address = "四川省成都市",
                Ethnicity = "汉族",
                MedicalHistory = "肺癌术后",
                Allergies = "青霉素",
                CreateTime = new DateTime(2026, 1, 1)
            };
            db.Insertable(source).ExecuteCommand();

            SaveLifePatientProfileMigration.Apply(db);
            SaveLifePatientProfileMigration.Apply(db);

            var patients = db.Queryable<SaveLifePatientProfile>().ClearFilter().ToList();
            var patient = Assert.Single(patients);
            Assert.Equal(source.Id, patient.Id);
            Assert.Equal("openid-1", patient.OwnerExternalUserId);
            Assert.Equal(source.Id, patient.SourceUserProfileId);
            Assert.Equal(SaveLifePatientProfileKeys.ForSelf("openid-1"), patient.AccountPatientKey);
            Assert.Equal(SaveLifePatientRelationships.Self, patient.RelationshipToAccount);
            Assert.Equal("本人", patient.RelationshipDescription);
            Assert.True(patient.IsDefault);
            Assert.Equal("张三", patient.DisplayName);
            Assert.Equal("male", patient.Gender);
            Assert.Equal(new DateTime(1980, 3, 2), patient.BirthDate);
            Assert.Equal("四川省成都市", patient.Residence);
            Assert.Equal("汉族", patient.Ethnicity);
            Assert.Equal("肺癌术后", patient.MedicalHistory);
            Assert.Equal("青霉素", patient.Allergies);

            var unchangedSource = db.Queryable<SaveLifeUserProfile>().ClearFilter().Single();
            Assert.Equal("肺癌术后", unchangedSource.MedicalHistory);
            Assert.Equal("青霉素", unchangedSource.Allergies);
            Assert.False(unchangedSource.IsDelete);
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    [Fact]
    public void Apply_WithPreviouslySelectedParent_PreservesSelectionAfterConversationIsCleared()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = CreateSqliteClient(dbPath);
            db.CodeFirst.InitTables(typeof(SaveLifeUserProfile));
            db.Insertable(CreateUserProfile(101, "openid-1")).ExecuteCommand();
            SaveLifePatientProfileMigration.Apply(db);

            var self = db.Queryable<SaveLifePatientProfile>().Single(item =>
                item.OwnerExternalUserId == "openid-1" &&
                item.RelationshipToAccount == SaveLifePatientRelationships.Self);
            var parent = CreatePatient(202, "openid-1", SaveLifePatientRelationships.Parent, "母亲");
            db.Insertable(parent).ExecuteCommand();
            db.Insertable(new SaveLifeAgentConversation
            {
                Id = 301,
                ExternalUserId = "openid-1",
                CurrentPatientProfileId = parent.Id,
                Status = "active",
                UpdateTime = new DateTime(2026, 7, 18),
                CreateTime = new DateTime(2026, 7, 17)
            }).ExecuteCommand();

            SaveLifePatientProfileMigration.Apply(db);

            Assert.False(db.Queryable<SaveLifePatientProfile>().Single(item => item.Id == self.Id).IsDefault);
            Assert.True(db.Queryable<SaveLifePatientProfile>().Single(item => item.Id == parent.Id).IsDefault);

            db.Updateable<SaveLifeAgentConversation>()
                .SetColumns(item => new SaveLifeAgentConversation { IsDelete = true })
                .Where(item => item.Id == 301)
                .ExecuteCommand();
            SaveLifePatientProfileMigration.Apply(db);

            Assert.False(db.Queryable<SaveLifePatientProfile>().Single(item => item.Id == self.Id).IsDefault);
            Assert.True(db.Queryable<SaveLifePatientProfile>().Single(item => item.Id == parent.Id).IsDefault);
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    [Fact]
    public void PatientIdentity_AllowsMultipleOtherPatientsButOnlyOneSelfPatientPerAccount()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = CreateSqliteClient(dbPath);
            SaveLifePatientProfileMigration.Apply(db);

            db.Insertable(new[]
            {
                CreatePatient(1, "openid-1", SaveLifePatientRelationships.Self, "本人"),
                CreatePatient(2, "openid-1", SaveLifePatientRelationships.Parent, "母亲"),
                CreatePatient(3, "openid-1", SaveLifePatientRelationships.Other, "亲属")
            }).ExecuteCommand();

            Assert.Equal(3, db.Queryable<SaveLifePatientProfile>().Count());
            Assert.ThrowsAny<Exception>(() => db.Insertable(
                CreatePatient(4, "openid-1", SaveLifePatientRelationships.Self, "另一个本人"))
                .ExecuteCommand());

            var selfWithIncorrectKey = CreatePatient(
                5,
                "openid-1",
                SaveLifePatientRelationships.Self,
                "错误身份键本人");
            selfWithIncorrectKey.AccountPatientKey = SaveLifePatientProfileKeys.ForOther(5);
            Assert.ThrowsAny<Exception>(() => db.Insertable(selfWithIncorrectKey).ExecuteCommand());
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    [Fact]
    public void Apply_WhenPatientInsertFails_RollsBackAllBackfilledPatients()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = CreateSqliteClient(dbPath);
            db.CodeFirst.InitTables(typeof(SaveLifeUserProfile), typeof(SaveLifePatientProfile));
            db.Insertable(new[]
            {
                CreateUserProfile(201, "openid-1"),
                CreateUserProfile(202, "openid-2")
            }).ExecuteCommand();
            db.Ado.ExecuteCommand("""
                CREATE TRIGGER fail_second_patient
                BEFORE INSERT ON sl_patient_profile
                WHEN NEW.OwnerExternalUserId = 'openid-2'
                BEGIN
                    SELECT RAISE(ABORT, 'forced patient migration failure');
                END;
                """);

            Assert.ThrowsAny<Exception>(() => SaveLifePatientProfileMigration.Apply(db));

            Assert.Empty(db.Queryable<SaveLifePatientProfile>().ClearFilter().ToList());
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    [Fact]
    public void Apply_WithExistingSelfPatient_PreservesConfirmedHealthFields()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = CreateSqliteClient(dbPath);
            db.CodeFirst.InitTables(typeof(SaveLifeUserProfile), typeof(SaveLifePatientProfile));
            db.Insertable(CreateUserProfile(301, "openid-1", "历史病史")).ExecuteCommand();
            var existing = CreatePatient(401, "openid-1", SaveLifePatientRelationships.Self, "本人");
            existing.Diagnosis = "已确认诊断";
            existing.MedicalHistory = "用户确认后的病史";
            db.Insertable(existing).ExecuteCommand();

            SaveLifePatientProfileMigration.Apply(db);

            var patient = db.Queryable<SaveLifePatientProfile>().ClearFilter().Single();
            Assert.Equal(401, patient.Id);
            Assert.Equal("已确认诊断", patient.Diagnosis);
            Assert.Equal("用户确认后的病史", patient.MedicalHistory);
            Assert.Equal(301, patient.SourceUserProfileId);
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    private static SaveLifePatientProfile CreatePatient(
        long id,
        string externalUserId,
        string relationship,
        string displayName)
    {
        return new SaveLifePatientProfile
        {
            Id = id,
            OwnerExternalUserId = externalUserId,
            AccountPatientKey = relationship == SaveLifePatientRelationships.Self
                ? SaveLifePatientProfileKeys.ForSelf(externalUserId)
                : SaveLifePatientProfileKeys.ForOther(id),
            RelationshipToAccount = relationship,
            RelationshipDescription = relationship == SaveLifePatientRelationships.Self
                ? "本人"
                : displayName,
            DisplayName = displayName,
            IsDefault = relationship == SaveLifePatientRelationships.Self,
            CreateTime = new DateTime(2026, 7, 14)
        };
    }

    private static void DropColumn(ISqlSugarClient db, string tableName, string columnName)
    {
        db.Ado.ExecuteCommand($"ALTER TABLE \"{tableName}\" DROP COLUMN \"{columnName}\"");
    }

    private static bool HasColumn(ISqlSugarClient db, string tableName, string columnName)
    {
        return db.DbMaintenance.GetColumnInfosByTableName(tableName, false)
            .Any(column => column.DbColumnName.Equals(columnName, StringComparison.OrdinalIgnoreCase));
    }

    private static SaveLifeUserProfile CreateUserProfile(
        long id,
        string externalUserId,
        string? medicalHistory = null)
    {
        return new SaveLifeUserProfile
        {
            Id = id,
            ExternalUserId = externalUserId,
            RealName = "本人",
            MedicalHistory = medicalHistory,
            CreateTime = new DateTime(2026, 7, 14)
        };
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
}
