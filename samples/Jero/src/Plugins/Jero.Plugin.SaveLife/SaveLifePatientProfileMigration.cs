using SqlSugar;

namespace Jero.Plugin.SaveLife;

public static class SaveLifePatientProfileMigration
{
    public const string SelfUniqueIndexName = "uq_sl_patient_profile_owner_self";
    public const string EnrollmentPatientUniqueIndexName = "uq_sl_enrollment_record_patient_project";

    private const string ArchivedExternalUserPrefix = "__merged__:";

    public static SaveLifePatientDataMigrationReport Apply(ISqlSugarClient db)
    {
        ArgumentNullException.ThrowIfNull(db);

        EnsureSchema(db);
        EnsureSelfUniqueIndex(db);
        var report = new SaveLifePatientDataMigrationReport();
        var sourceProfiles = HasTable(db, "sl_user_profile")
            ? db.Queryable<SaveLifeUserProfile>()
                .ClearFilter()
                .Where(u => !u.IsDelete && u.ExternalUserId != null)
                .ToList()
                .Where(IsMigratableProfile)
                .OrderBy(u => u.Id)
                .ToList()
            : [];

        db.Ado.BeginTran();
        try
        {
            BackfillProposalSources(db);
            var patients = db.Queryable<SaveLifePatientProfile>()
                .ClearFilter()
                .ToList();
            foreach (var source in sourceProfiles)
            {
                var ownerExternalUserId = source.ExternalUserId!.Trim();
                var selfKey = SaveLifePatientProfileKeys.ForSelf(ownerExternalUserId);
                var patient = FindSelfPatient(patients, ownerExternalUserId, selfKey);
                var accountHasDefaultPatient = patients.Any(item =>
                    string.Equals(item.OwnerExternalUserId, ownerExternalUserId, StringComparison.Ordinal) &&
                    item.IsDefault &&
                    !item.IsDelete);

                if (patient == null)
                {
                    patient = SaveLifePatientProfileFactory.CreateSelf(source, ownerExternalUserId);
                    if (accountHasDefaultPatient)
                        patient.IsDefault = false;
                    db.Insertable(patient).ExecuteCommand();
                    patients.Add(patient);
                    continue;
                }

                if (FillMissingCompatibilityFields(
                        patient,
                        source,
                        ownerExternalUserId,
                        selfKey,
                        accountHasDefaultPatient))
                    db.Updateable(patient).ExecuteCommand();
            }

            BackfillConversationPatients(db, patients);
            BackfillAccountDefaultPatients(db, patients);
            BackfillEnrollmentPatients(db, patients, report);
            BackfillMaterialPatients(db, patients, report);
            BackfillPrivateIndexPatients(db, patients, report);
            ReplaceEnrollmentUniqueIndex(db);

            db.Ado.CommitTran();
            return report;
        }
        catch
        {
            db.Ado.RollbackTran();
            throw;
        }
    }

    private static void BackfillProposalSources(ISqlSugarClient db)
    {
        db.Updateable<SaveLifePatientProfileProposal>()
            .SetColumns(item => new SaveLifePatientProfileProposal
            {
                SourceType = SaveLifePatientProfileProposalSources.UserMessage
            })
            .Where(item => item.SourceType == null || item.SourceType == string.Empty)
            .ExecuteCommand();
    }

    public static void EnsureSchema(ISqlSugarClient db)
    {
        ArgumentNullException.ThrowIfNull(db);

        db.CodeFirst.InitTables(
            typeof(SaveLifePatientProfile),
            typeof(SaveLifePatientProfileProposal),
            typeof(SaveLifeAgentConversation),
            typeof(SaveLifeAgentMessage),
            typeof(SaveLifeEnrollmentRecord),
            typeof(SaveLifeEnrollmentMaterial),
            typeof(SaveLifeAgentVectorIndex));
    }

    private static void BackfillEnrollmentPatients(
        ISqlSugarClient db,
        IReadOnlyCollection<SaveLifePatientProfile> patients,
        SaveLifePatientDataMigrationReport report)
    {
        var enrollments = db.Queryable<SaveLifeEnrollmentRecord>()
            .ClearFilter()
            .ToList();
        var changed = new List<SaveLifeEnrollmentRecord>();
        foreach (var enrollment in enrollments.Where(u => !u.IsDelete && !u.PatientProfileId.HasValue))
        {
            var patient = ResolveHistoricalPatient(
                patients,
                enrollment.UserProfileId,
                enrollment.ExternalUserId);
            if (patient == null)
                continue;

            enrollment.PatientProfileId = patient.Id;
            changed.Add(enrollment);
        }

        var conflicts = enrollments
            .Where(u => !u.IsDelete && u.PatientProfileId is > 0)
            .GroupBy(u => new { PatientProfileId = u.PatientProfileId!.Value, u.ProjectId })
            .Where(group => group.Count() > 1)
            .Select(group => new SaveLifeEnrollmentPatientConflict
            {
                PatientProfileId = group.Key.PatientProfileId,
                ProjectId = group.Key.ProjectId,
                EnrollmentRecordIds = group.Select(u => u.Id).OrderBy(u => u).ToList()
            })
            .OrderBy(u => u.PatientProfileId)
            .ThenBy(u => u.ProjectId)
            .ToList();
        report.EnrollmentConflicts.AddRange(conflicts);
        if (conflicts.Count > 0)
            throw new SaveLifePatientDataMigrationConflictException(report);

        foreach (var enrollment in changed)
        {
            db.Updateable(enrollment)
                .UpdateColumns(u => u.PatientProfileId)
                .ExecuteCommand();
        }

        report.EnrollmentBackfilledCount = changed.Count;
    }

    private static SaveLifePatientProfile? ResolveHistoricalPatient(
        IEnumerable<SaveLifePatientProfile> patients,
        long? sourceUserProfileId,
        string? externalUserId)
    {
        var activePatients = patients.Where(u => u.Id > 0 && !u.IsDelete);
        if (sourceUserProfileId is > 0)
        {
            var sourcePatient = activePatients.FirstOrDefault(u =>
                u.SourceUserProfileId == sourceUserProfileId.Value);
            if (sourcePatient != null &&
                (string.IsNullOrWhiteSpace(externalUserId) ||
                 string.Equals(sourcePatient.OwnerExternalUserId, externalUserId.Trim(), StringComparison.Ordinal)))
            {
                return sourcePatient;
            }
        }

        if (string.IsNullOrWhiteSpace(externalUserId))
            return null;

        var ownerExternalUserId = externalUserId.Trim();
        return FindSelfPatient(
            activePatients,
            ownerExternalUserId,
            SaveLifePatientProfileKeys.ForSelf(ownerExternalUserId));
    }

    private static void BackfillMaterialPatients(
        ISqlSugarClient db,
        IReadOnlyCollection<SaveLifePatientProfile> patients,
        SaveLifePatientDataMigrationReport report)
    {
        var enrollments = db.Queryable<SaveLifeEnrollmentRecord>()
            .ClearFilter()
            .Where(u => u.PatientProfileId != null)
            .ToList()
            .ToDictionary(u => u.Id);
        var changed = new List<SaveLifeEnrollmentMaterial>();
        var materials = db.Queryable<SaveLifeEnrollmentMaterial>()
            .ClearFilter()
            .Where(u => u.PatientProfileId == null)
            .ToList();
        foreach (var material in materials)
        {
            SaveLifePatientProfile? patient = null;
            if (enrollments.TryGetValue(material.EnrollmentRecordId, out var enrollment))
            {
                patient = ResolveOwnedPatient(
                    patients,
                    enrollment.PatientProfileId,
                    material.ExternalUserId);
            }
            patient ??= ResolveHistoricalPatient(
                patients,
                material.UserProfileId,
                material.ExternalUserId);
            if (patient == null)
                continue;

            material.PatientProfileId = patient.Id;
            changed.Add(material);
        }

        foreach (var material in changed)
        {
            db.Updateable(material)
                .UpdateColumns(u => u.PatientProfileId)
                .ExecuteCommand();
        }

        report.MaterialBackfilledCount = changed.Count;
    }

    private static void BackfillPrivateIndexPatients(
        ISqlSugarClient db,
        IReadOnlyCollection<SaveLifePatientProfile> patients,
        SaveLifePatientDataMigrationReport report)
    {
        var materials = db.Queryable<SaveLifeEnrollmentMaterial>()
            .ClearFilter()
            .Where(u => u.PatientProfileId != null)
            .ToList()
            .ToDictionary(u => u.Id);
        var changed = new List<SaveLifeAgentVectorIndex>();
        var privateIndexes = db.Queryable<SaveLifeAgentVectorIndex>()
            .ClearFilter()
            .Where(u => u.ExternalUserId != null && u.PatientProfileId == null)
            .ToList();
        foreach (var index in privateIndexes.Where(u => !string.IsNullOrWhiteSpace(u.ExternalUserId)))
        {
            SaveLifePatientProfile? patient = null;
            if (index.ResourceType == "material" &&
                materials.TryGetValue(index.ResourceId, out var material))
            {
                patient = ResolveOwnedPatient(
                    patients,
                    material.PatientProfileId,
                    index.ExternalUserId);
            }
            patient ??= ResolveHistoricalPatient(patients, null, index.ExternalUserId);
            if (patient == null)
                continue;

            index.PatientProfileId = patient.Id;
            changed.Add(index);
        }

        foreach (var index in changed)
        {
            db.Updateable(index)
                .UpdateColumns(u => u.PatientProfileId)
                .ExecuteCommand();
        }

        report.PrivateIndexBackfilledCount = changed.Count;
    }

    private static SaveLifePatientProfile? ResolveOwnedPatient(
        IEnumerable<SaveLifePatientProfile> patients,
        long? patientProfileId,
        string? externalUserId)
    {
        if (patientProfileId is not > 0 || string.IsNullOrWhiteSpace(externalUserId))
            return null;

        var ownerExternalUserId = externalUserId.Trim();
        return patients.FirstOrDefault(u =>
            u.Id == patientProfileId.Value &&
            !u.IsDelete &&
            string.Equals(u.OwnerExternalUserId, ownerExternalUserId, StringComparison.Ordinal));
    }

    private static void BackfillConversationPatients(
        ISqlSugarClient db,
        IReadOnlyCollection<SaveLifePatientProfile> patients)
    {
        var unboundConversationIds = db.Queryable<SaveLifeAgentMessage>()
            .ClearFilter()
            .Where(u => u.PatientProfileId == null)
            .Select(u => u.ConversationId)
            .Distinct()
            .ToList();
        var unversionedConversationIds = db.Queryable<SaveLifeAgentMessage>()
            .ClearFilter()
            .Where(u => u.PatientContextVersion <= 0)
            .Select(u => u.ConversationId)
            .Distinct()
            .ToList();
        var conversationIds = unboundConversationIds
            .Concat(unversionedConversationIds)
            .Distinct()
            .ToList();
        var conversationQuery = db.Queryable<SaveLifeAgentConversation>().ClearFilter();
        var conversations = conversationIds.Count == 0
            ? conversationQuery
                .Where(u =>
                    u.FlowStateVersion == null ||
                    u.CurrentPatientProfileId == null ||
                    u.PatientContextVersion <= 0)
                .ToList()
            : conversationQuery
                .Where(u =>
                    u.FlowStateVersion == null ||
                    u.CurrentPatientProfileId == null ||
                    u.PatientContextVersion <= 0 ||
                    conversationIds.Contains(u.Id))
                .ToList();
        var unboundConversationIdSet = unboundConversationIds.ToHashSet();
        var unversionedConversationIdSet = unversionedConversationIds.ToHashSet();
        foreach (var conversation in conversations)
        {
            var changed = false;
            if (!conversation.FlowStateVersion.HasValue)
            {
                conversation.FlowStateVersion = SaveLifeAgentConversationState.CurrentVersion;
                changed = true;
            }

            if (conversation.PatientContextVersion <= 0)
            {
                conversation.PatientContextVersion = 1;
                changed = true;
            }

            if (!conversation.CurrentPatientProfileId.HasValue &&
                !string.IsNullOrWhiteSpace(conversation.ExternalUserId))
            {
                var ownerExternalUserId = conversation.ExternalUserId.Trim();
                var patient = FindSelfPatient(
                    patients,
                    ownerExternalUserId,
                    SaveLifePatientProfileKeys.ForSelf(ownerExternalUserId));
                if (patient is { Id: > 0, IsDelete: false })
                {
                    conversation.CurrentPatientProfileId = patient.Id;
                    changed = true;
                }
            }

            if (changed)
            {
                db.Updateable(conversation)
                    .UpdateColumns(u => new
                    {
                        u.CurrentPatientProfileId,
                        u.FlowStateVersion,
                        u.PatientContextVersion
                    })
                    .ExecuteCommand();
            }

            if (unversionedConversationIdSet.Contains(conversation.Id))
            {
                var patientContextVersion = conversation.PatientContextVersion;
                db.Updateable<SaveLifeAgentMessage>()
                    .SetColumns(u => new SaveLifeAgentMessage
                    {
                        PatientContextVersion = patientContextVersion
                    })
                    .Where(u =>
                        u.ConversationId == conversation.Id &&
                        u.ExternalUserId == conversation.ExternalUserId &&
                        u.PatientContextVersion <= 0)
                    .ExecuteCommand();
            }

            if (!conversation.CurrentPatientProfileId.HasValue ||
                !unboundConversationIdSet.Contains(conversation.Id))
            {
                continue;
            }

            var patientProfileId = conversation.CurrentPatientProfileId.Value;
            db.Updateable<SaveLifeAgentMessage>()
                .SetColumns(u => new SaveLifeAgentMessage { PatientProfileId = patientProfileId })
                .Where(u =>
                    u.ConversationId == conversation.Id &&
                    u.ExternalUserId == conversation.ExternalUserId &&
                    u.PatientProfileId == null)
                .ExecuteCommand();
        }
    }

    private static SaveLifePatientProfile? FindSelfPatient(
        IEnumerable<SaveLifePatientProfile> patients,
        string ownerExternalUserId,
        string selfKey)
    {
        return patients.FirstOrDefault(u =>
                string.Equals(u.OwnerExternalUserId, ownerExternalUserId, StringComparison.Ordinal) &&
                string.Equals(u.AccountPatientKey, selfKey, StringComparison.Ordinal))
            ?? patients.FirstOrDefault(u =>
                string.Equals(u.OwnerExternalUserId, ownerExternalUserId, StringComparison.Ordinal) &&
                (u.RelationshipToAccount == SaveLifePatientRelationships.Self || u.IsDefault));
    }

    private static bool FillMissingCompatibilityFields(
        SaveLifePatientProfile patient,
        SaveLifeUserProfile source,
        string ownerExternalUserId,
        string selfKey,
        bool accountHasDefaultPatient)
    {
        var changed = false;
        changed |= SetValue(patient.OwnerExternalUserId, ownerExternalUserId, value => patient.OwnerExternalUserId = value);
        changed |= SetValue(patient.AccountPatientKey, selfKey, value => patient.AccountPatientKey = value);
        changed |= SetValue(patient.RelationshipToAccount, SaveLifePatientRelationships.Self, value => patient.RelationshipToAccount = value);
        changed |= SetValue(patient.RelationshipDescription, "本人", value => patient.RelationshipDescription = value, onlyWhenMissing: true);
        changed |= SetValue(patient.DisplayName, FirstText(source.RealName) ?? "本人", value => patient.DisplayName = value, onlyWhenMissing: true);
        changed |= SetNullableValue(patient.SourceUserProfileId, source.Id, value => patient.SourceUserProfileId = value);
        changed |= SetNullableText(patient.Gender, source.Gender, value => patient.Gender = value);
        changed |= SetNullableValue(patient.BirthDate, source.BirthDate, value => patient.BirthDate = value);
        changed |= SetNullableText(patient.Ethnicity, source.Ethnicity, value => patient.Ethnicity = value);
        changed |= SetNullableText(patient.Residence, source.Address, value => patient.Residence = value);
        changed |= SetNullableText(patient.MedicalHistory, source.MedicalHistory, value => patient.MedicalHistory = value);
        changed |= SetNullableText(patient.Allergies, source.Allergies, value => patient.Allergies = value);

        if (!patient.IsDefault && !accountHasDefaultPatient)
        {
            patient.IsDefault = true;
            changed = true;
        }
        if (patient.IsDelete)
        {
            patient.IsDelete = false;
            patient.DeleteTime = null;
            changed = true;
        }

        if (changed)
            patient.UpdateTime = DateTime.Now;
        return changed;
    }

    private static void BackfillAccountDefaultPatients(
        ISqlSugarClient db,
        IReadOnlyCollection<SaveLifePatientProfile> patients)
    {
        var changedPatients = new List<SaveLifePatientProfile>();
        var latestSelections = db.Queryable<SaveLifeAgentConversation>()
            .ClearFilter()
            .Where(item => item.CurrentPatientProfileId != null)
            .ToList()
            .Where(item => !string.IsNullOrWhiteSpace(item.ExternalUserId))
            .GroupBy(item => item.ExternalUserId, StringComparer.Ordinal)
            .Select(group => group
                .OrderByDescending(item => item.UpdateTime ?? item.CreateTime)
                .ThenByDescending(item => item.Id)
                .First())
            .ToList();

        foreach (var selection in latestSelections)
        {
            var selectedPatientId = selection.CurrentPatientProfileId!.Value;
            var accountPatients = patients
                .Where(item =>
                    !item.IsDelete &&
                    string.Equals(
                        item.OwnerExternalUserId,
                        selection.ExternalUserId,
                        StringComparison.Ordinal))
                .ToList();
            if (accountPatients.All(item => item.Id != selectedPatientId))
                continue;

            var changed = accountPatients
                .Where(item => item.IsDefault != (item.Id == selectedPatientId))
                .ToList();
            foreach (var patient in changed)
                patient.IsDefault = patient.Id == selectedPatientId;
            changedPatients.AddRange(changed);
        }

        if (changedPatients.Count > 0)
            db.Updateable(changedPatients).UpdateColumns(item => item.IsDefault).ExecuteCommand();
    }

    private static bool SetValue(
        string current,
        string value,
        Action<string> setter,
        bool onlyWhenMissing = false)
    {
        if (onlyWhenMissing && !string.IsNullOrWhiteSpace(current))
            return false;
        if (string.Equals(current, value, StringComparison.Ordinal))
            return false;

        setter(value);
        return true;
    }

    private static bool SetNullableText(
        string? current,
        string? value,
        Action<string?> setter)
    {
        var normalized = FirstText(value);
        if (!string.IsNullOrWhiteSpace(current) || normalized == null)
            return false;

        setter(normalized);
        return true;
    }

    private static bool SetNullableValue<T>(
        T? current,
        T? value,
        Action<T?> setter)
        where T : struct
    {
        if (current.HasValue || !value.HasValue)
            return false;

        setter(value);
        return true;
    }

    private static string? FirstText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static bool IsMigratableProfile(SaveLifeUserProfile profile)
    {
        return profile.Id > 0 &&
            !string.IsNullOrWhiteSpace(profile.ExternalUserId) &&
            !profile.ExternalUserId.StartsWith(ArchivedExternalUserPrefix, StringComparison.Ordinal);
    }

    private static bool HasTable(ISqlSugarClient db, string tableName)
    {
        return (db.DbMaintenance.GetColumnInfosByTableName(tableName, false)?.Count ?? 0) > 0;
    }

    private static void EnsureSelfUniqueIndex(ISqlSugarClient db)
    {
        var dbType = db.CurrentConnectionConfig.DbType.ToString();
        if (dbType == "Sqlite")
        {
            db.Ado.ExecuteCommand($"""
                CREATE UNIQUE INDEX IF NOT EXISTS "{SelfUniqueIndexName}"
                ON "sl_patient_profile" ("OwnerExternalUserId")
                WHERE "RelationshipToAccount" = '{SaveLifePatientRelationships.Self}'
                  AND "IsDelete" = 0
                """);
            return;
        }

        if (dbType is "PostgreSQL" or "Kdbndp")
        {
            db.Ado.ExecuteCommand($"""
                CREATE UNIQUE INDEX IF NOT EXISTS "{SelfUniqueIndexName}"
                ON "sl_patient_profile" ("OwnerExternalUserId")
                WHERE "RelationshipToAccount" = '{SaveLifePatientRelationships.Self}'
                  AND "IsDelete" = FALSE
                """);
            return;
        }

        if (dbType == "SqlServer")
        {
            db.Ado.ExecuteCommand($"""
                IF NOT EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = N'{SelfUniqueIndexName}'
                      AND object_id = OBJECT_ID(N'sl_patient_profile')
                )
                CREATE UNIQUE INDEX [{SelfUniqueIndexName}]
                ON [sl_patient_profile] ([OwnerExternalUserId])
                WHERE [RelationshipToAccount] = '{SaveLifePatientRelationships.Self}'
                  AND [IsDelete] = 0
                """);
        }
    }

    private static void ReplaceEnrollmentUniqueIndex(ISqlSugarClient db)
    {
        var dbType = db.CurrentConnectionConfig.DbType.ToString();
        var legacyIndexName = SaveLifeDataIntegrityMigration.EnrollmentUniqueIndexName;
        if (dbType == "Sqlite")
        {
            db.Ado.ExecuteCommand($"DROP INDEX IF EXISTS \"{legacyIndexName}\"");
            db.Ado.ExecuteCommand($"""
                CREATE UNIQUE INDEX IF NOT EXISTS "{EnrollmentPatientUniqueIndexName}"
                ON "sl_enrollment_record" ("PatientProfileId", "ProjectId")
                WHERE "PatientProfileId" IS NOT NULL
                  AND "IsDelete" = 0
                """);
            return;
        }


        if (dbType is "PostgreSQL" or "Kdbndp")
        {
            db.Ado.ExecuteCommand($"DROP INDEX IF EXISTS \"{legacyIndexName}\"");
            db.Ado.ExecuteCommand($"""
                CREATE UNIQUE INDEX IF NOT EXISTS "{EnrollmentPatientUniqueIndexName}"
                ON "sl_enrollment_record" ("PatientProfileId", "ProjectId")
                WHERE "PatientProfileId" IS NOT NULL
                  AND "IsDelete" = FALSE
                """);
            return;
        }

        if (dbType == "SqlServer")
        {
            db.Ado.ExecuteCommand($"""
                IF EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = N'{legacyIndexName}'
                      AND object_id = OBJECT_ID(N'sl_enrollment_record')
                )
                DROP INDEX [{legacyIndexName}] ON [sl_enrollment_record]

                IF NOT EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = N'{EnrollmentPatientUniqueIndexName}'
                      AND object_id = OBJECT_ID(N'sl_enrollment_record')
                )
                CREATE UNIQUE INDEX [{EnrollmentPatientUniqueIndexName}]
                ON [sl_enrollment_record] ([PatientProfileId], [ProjectId])
                WHERE [PatientProfileId] IS NOT NULL
                  AND [IsDelete] = 0
                """);
            return;
        }

        if (dbType is "MySql" or "MySqlConnector")
        {
            DropMySqlIndexIfExists(db, legacyIndexName);
            if (!MySqlIndexExists(db, EnrollmentPatientUniqueIndexName))
            {
                db.Ado.ExecuteCommand($"""
                    CREATE UNIQUE INDEX `{EnrollmentPatientUniqueIndexName}`
                    ON `sl_enrollment_record` (`PatientProfileId`, `ProjectId`, `IsDelete`)
                    """);
            }
        }
    }

    private static void DropMySqlIndexIfExists(ISqlSugarClient db, string indexName)
    {
        if (!MySqlIndexExists(db, indexName))
            return;

        db.Ado.ExecuteCommand($"DROP INDEX `{indexName}` ON `sl_enrollment_record`");
    }

    private static bool MySqlIndexExists(ISqlSugarClient db, string indexName)
    {
        return db.Ado.SqlQuery<int>($"""
            SELECT COUNT(1)
            FROM information_schema.statistics
            WHERE table_schema = DATABASE()
              AND table_name = 'sl_enrollment_record'
              AND index_name = '{indexName}'
            """).FirstOrDefault() > 0;
    }
}

public sealed class SaveLifePatientDataMigrationReport
{
    public int EnrollmentBackfilledCount { get; set; }

    public int MaterialBackfilledCount { get; set; }

    public int PrivateIndexBackfilledCount { get; set; }

    public int TotalBackfilledCount =>
        EnrollmentBackfilledCount + MaterialBackfilledCount + PrivateIndexBackfilledCount;

    public List<SaveLifeEnrollmentPatientConflict> EnrollmentConflicts { get; } = [];
}

public sealed class SaveLifeEnrollmentPatientConflict
{
    public long PatientProfileId { get; set; }

    public long ProjectId { get; set; }

    public List<long> EnrollmentRecordIds { get; set; } = [];
}

public sealed class SaveLifePatientDataMigrationConflictException : InvalidOperationException
{
    public SaveLifePatientDataMigrationConflictException(SaveLifePatientDataMigrationReport report)
        : base($"患者报名数据存在 {report.EnrollmentConflicts.Count} 组重复，迁移已回滚")
    {
        Report = report;
    }

    public SaveLifePatientDataMigrationReport Report { get; }
}
