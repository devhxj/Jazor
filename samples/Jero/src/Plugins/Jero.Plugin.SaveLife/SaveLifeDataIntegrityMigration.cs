using SqlSugar;

namespace Jero.Plugin.SaveLife;

public static class SaveLifeDataIntegrityMigration
{
    public const string ProfileUniqueIndexName = "uq_sl_user_profile_external_user";
    public const string EnrollmentUniqueIndexName = "uq_sl_enrollment_record_user_project";

    private const string ArchivedExternalUserPrefix = "__merged__:";

    public static void Apply(ISqlSugarClient db)
    {
        var hasProfiles = HasTable(db, "sl_user_profile");
        var hasEnrollments = HasTable(db, "sl_enrollment_record");
        var hasPatientEnrollmentColumn = hasEnrollments &&
            HasColumn(db, "sl_enrollment_record", nameof(SaveLifeEnrollmentRecord.PatientProfileId));
        if (!hasProfiles && !hasEnrollments)
            return;

        db.Ado.BeginTran();
        try
        {
            if (hasProfiles)
                MergeDuplicateProfiles(db);
            if (hasEnrollments)
                MergeDuplicateEnrollments(db, hasPatientEnrollmentColumn);

            db.Ado.CommitTran();
        }
        catch
        {
            db.Ado.RollbackTran();
            throw;
        }

        if (hasProfiles)
        {
            EnsureUniqueIndex(
                db,
                "sl_user_profile",
                ProfileUniqueIndexName,
                ["ExternalUserId"]);
        }

        var hasPatientScopedEnrollments = hasPatientEnrollmentColumn &&
            db.Queryable<SaveLifeEnrollmentRecord>()
                .ClearFilter()
                .Count(u => !u.IsDelete && u.PatientProfileId != null) > 0;
        if (hasEnrollments && !hasPatientScopedEnrollments)
        {
            EnsureUniqueIndex(
                db,
                "sl_enrollment_record",
                EnrollmentUniqueIndexName,
                ["ExternalUserId", "ProjectId"]);
        }
    }

    private static void MergeDuplicateProfiles(ISqlSugarClient db)
    {
        var profiles = db.Queryable<SaveLifeUserProfile>()
            .ClearFilter()
            .ToList();
        NormalizeEmptyExternalUserIds(db, profiles);

        var groups = profiles
            .Where(u => IsBusinessExternalUserId(u.ExternalUserId))
            .GroupBy(u => u.ExternalUserId!.Trim(), StringComparer.Ordinal)
            .Where(u => u.Count() > 1)
            .ToList();

        foreach (var group in groups)
        {
            var ordered = group
                .OrderBy(u => u.IsDelete)
                .ThenByDescending(GetLastChangedTime)
                .ThenByDescending(GetProfileCompleteness)
                .ThenByDescending(u => u.Id)
                .ToList();
            var canonical = ordered[0];
            var duplicates = ordered.Skip(1).ToList();

            MergeProfileFields(canonical, ordered);
            canonical.ExternalUserId = group.Key;
            if (ordered.Any(u => !u.IsDelete))
            {
                canonical.IsDelete = false;
                canonical.DeleteTime = null;
            }

            foreach (var duplicate in duplicates)
            {
                RebindProfileReferences(db, duplicate.Id, canonical.Id);
                ArchiveDuplicate(duplicate, canonical.Id);
                db.Updateable(duplicate).ExecuteCommand();
            }

            db.Updateable(canonical).ExecuteCommand();
        }
    }

    private static void MergeDuplicateEnrollments(
        ISqlSugarClient db,
        bool usePatientScope)
    {
        var enrollments = db.Queryable<SaveLifeEnrollmentRecord>()
            .ClearFilter()
            .ToList();
        NormalizeEmptyExternalUserIds(db, enrollments);

        var groups = enrollments
            .Where(u => u.PatientProfileId is > 0 || IsBusinessExternalUserId(u.ExternalUserId))
            .GroupBy(u => BuildEnrollmentScopeKey(u, usePatientScope), StringComparer.Ordinal)
            .Where(u => u.Count() > 1)
            .ToList();

        foreach (var group in groups)
        {
            var ordered = group
                .OrderBy(u => u.IsDelete)
                .ThenByDescending(u => GetEnrollmentStatusPriority(u.Status))
                .ThenByDescending(GetLastChangedTime)
                .ThenByDescending(u => u.Id)
                .ToList();
            var canonical = ordered[0];
            var duplicates = ordered.Skip(1).ToList();

            MergeEnrollmentFields(canonical, ordered);
            canonical.ExternalUserId = canonical.ExternalUserId?.Trim();
            if (ordered.Any(u => !u.IsDelete))
            {
                canonical.IsDelete = false;
                canonical.DeleteTime = null;
            }

            foreach (var duplicate in duplicates)
            {
                RebindEnrollmentReferences(db, duplicate.Id, canonical);
                ArchiveDuplicate(duplicate, canonical.Id);
                db.Updateable(duplicate).ExecuteCommand();
            }

            db.Updateable(canonical).ExecuteCommand();
        }
    }

    private static void RebindProfileReferences(ISqlSugarClient db, long duplicateId, long canonicalId)
    {
        if (HasTable(db, "sl_enrollment_record"))
        {
            db.Updateable<SaveLifeEnrollmentRecord>()
                .SetColumns(u => new SaveLifeEnrollmentRecord { UserProfileId = canonicalId })
                .Where(u => u.UserProfileId == duplicateId)
                .ExecuteCommand();
        }

        if (HasTable(db, "sl_enrollment_material"))
        {
            db.Updateable<SaveLifeEnrollmentMaterial>()
                .SetColumns(u => new SaveLifeEnrollmentMaterial { UserProfileId = canonicalId })
                .Where(u => u.UserProfileId == duplicateId)
                .ExecuteCommand();
        }
    }

    private static void RebindEnrollmentReferences(
        ISqlSugarClient db,
        long duplicateId,
        SaveLifeEnrollmentRecord canonical)
    {
        if (HasTable(db, "sl_enrollment_material"))
        {
            db.Updateable<SaveLifeEnrollmentMaterial>()
                .SetColumns(u => new SaveLifeEnrollmentMaterial
                {
                    EnrollmentRecordId = canonical.Id,
                    ProjectId = canonical.ProjectId,
                    UserProfileId = canonical.UserProfileId,
                    PatientProfileId = canonical.PatientProfileId
                })
                .Where(u => u.EnrollmentRecordId == duplicateId)
                .ExecuteCommand();
        }

        if (HasTable(db, "sl_agent_conversation"))
        {
            db.Updateable<SaveLifeAgentConversation>()
                .SetColumns(u => new SaveLifeAgentConversation { EnrollmentRecordId = canonical.Id })
                .Where(u => u.EnrollmentRecordId == duplicateId)
                .ExecuteCommand();
            db.Updateable<SaveLifeAgentConversation>()
                .SetColumns(u => new SaveLifeAgentConversation { ContextId = canonical.Id })
                .Where(u => u.ContextType == "enrollment" && u.ContextId == duplicateId)
                .ExecuteCommand();
        }

        if (HasTable(db, "sl_notification"))
        {
            db.Updateable<SaveLifeNotification>()
                .SetColumns(u => new SaveLifeNotification { BusinessId = canonical.Id })
                .Where(u => u.BusinessType == "enrollment" && u.BusinessId == duplicateId)
                .ExecuteCommand();
        }

        if (HasTable(db, "sl_announcement"))
        {
            db.Updateable<SaveLifeAnnouncement>()
                .SetColumns(u => new SaveLifeAnnouncement { BusinessId = canonical.Id })
                .Where(u => u.BusinessType == "enrollment" && u.BusinessId == duplicateId)
                .ExecuteCommand();
        }
    }

    private static void MergeProfileFields(
        SaveLifeUserProfile canonical,
        IReadOnlyList<SaveLifeUserProfile> candidates)
    {
        canonical.RealName = FirstText(canonical.RealName, candidates.Select(u => u.RealName));
        canonical.Gender = FirstText(canonical.Gender, candidates.Select(u => u.Gender));
        canonical.BirthDate ??= candidates.Select(u => u.BirthDate).FirstOrDefault(u => u.HasValue);
        canonical.Phone = FirstText(canonical.Phone, candidates.Select(u => u.Phone));
        canonical.Email = FirstText(canonical.Email, candidates.Select(u => u.Email));
        canonical.Address = FirstText(canonical.Address, candidates.Select(u => u.Address));
        canonical.Ethnicity = FirstText(canonical.Ethnicity, candidates.Select(u => u.Ethnicity));
        canonical.EmergencyContact = FirstText(
            canonical.EmergencyContact,
            candidates.Select(u => u.EmergencyContact));
        canonical.EmergencyPhone = FirstText(
            canonical.EmergencyPhone,
            candidates.Select(u => u.EmergencyPhone));
        canonical.MedicalHistory = MergeDistinctText(candidates.Select(u => u.MedicalHistory));
        canonical.Allergies = MergeDistinctText(candidates.Select(u => u.Allergies));
        ApplyMergedTimes(canonical, candidates);
    }

    private static void MergeEnrollmentFields(
        SaveLifeEnrollmentRecord canonical,
        IReadOnlyList<SaveLifeEnrollmentRecord> candidates)
    {
        canonical.PatientProfileId ??= candidates.Select(u => u.PatientProfileId).FirstOrDefault(u => u.HasValue);
        canonical.UserProfileId ??= candidates.Select(u => u.UserProfileId).FirstOrDefault(u => u.HasValue);
        canonical.ProjectTitle = FirstText(canonical.ProjectTitle, candidates.Select(u => u.ProjectTitle));
        canonical.ContactName = FirstText(canonical.ContactName, candidates.Select(u => u.ContactName));
        canonical.ContactPhone = FirstText(canonical.ContactPhone, candidates.Select(u => u.ContactPhone));
        canonical.SubmittedTime ??= candidates.Select(u => u.SubmittedTime).FirstOrDefault(u => u.HasValue);
        canonical.AuditTime ??= candidates.Select(u => u.AuditTime).FirstOrDefault(u => u.HasValue);
        canonical.AuditRemark = FirstText(canonical.AuditRemark, candidates.Select(u => u.AuditRemark));
        canonical.DiseaseDescription = MergeDistinctText(candidates.Select(u => u.DiseaseDescription));
        canonical.MedicalHistory = MergeDistinctText(candidates.Select(u => u.MedicalHistory));
        canonical.DiagnosisReport = MergeDistinctText(candidates.Select(u => u.DiagnosisReport));
        canonical.MedicationStatus = MergeDistinctText(candidates.Select(u => u.MedicationStatus));
        canonical.OtherInfo = MergeDistinctText(candidates.Select(u => u.OtherInfo));
        canonical.Notes = MergeDistinctText(candidates.Select(u => u.Notes));
        ApplyMergedTimes(canonical, candidates);
    }

    private static void NormalizeEmptyExternalUserIds(
        ISqlSugarClient db,
        IEnumerable<SaveLifeUserProfile> profiles)
    {
        foreach (var profile in profiles.Where(u => string.IsNullOrWhiteSpace(u.ExternalUserId) && u.ExternalUserId != null))
        {
            profile.ExternalUserId = null;
            db.Updateable(profile).ExecuteCommand();
        }
    }

    private static void NormalizeEmptyExternalUserIds(
        ISqlSugarClient db,
        IEnumerable<SaveLifeEnrollmentRecord> enrollments)
    {
        foreach (var enrollment in enrollments.Where(u => string.IsNullOrWhiteSpace(u.ExternalUserId) && u.ExternalUserId != null))
        {
            enrollment.ExternalUserId = null;
            db.Updateable(enrollment).ExecuteCommand();
        }
    }

    private static void ArchiveDuplicate(SaveLifeUserProfile duplicate, long canonicalId)
    {
        duplicate.ExternalUserId = BuildArchivedExternalUserId(canonicalId, duplicate.Id);
        duplicate.IsDelete = true;
        duplicate.DeleteTime ??= DateTime.Now;
        duplicate.UpdateTime = DateTime.Now;
    }

    private static void ArchiveDuplicate(SaveLifeEnrollmentRecord duplicate, long canonicalId)
    {
        duplicate.ExternalUserId = BuildArchivedExternalUserId(canonicalId, duplicate.Id);
        duplicate.PatientProfileId = null;
        duplicate.IsDelete = true;
        duplicate.DeleteTime ??= DateTime.Now;
        duplicate.UpdateTime = DateTime.Now;
    }

    private static string BuildArchivedExternalUserId(long canonicalId, long duplicateId)
    {
        return $"{ArchivedExternalUserPrefix}{canonicalId}:{duplicateId}";
    }

    private static string? FirstText(string? preferred, IEnumerable<string?> values)
    {
        if (!string.IsNullOrWhiteSpace(preferred))
            return preferred.Trim();

        return values.FirstOrDefault(u => !string.IsNullOrWhiteSpace(u))?.Trim();
    }

    private static string? MergeDistinctText(IEnumerable<string?> values)
    {
        var distinct = values
            .Where(u => !string.IsNullOrWhiteSpace(u))
            .Select(u => u!.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToList();
        return distinct.Count == 0 ? null : string.Join("\n\n", distinct);
    }

    private static void ApplyMergedTimes<T>(T canonical, IReadOnlyList<T> candidates)
        where T : EntityBase
    {
        var createTimes = candidates
            .Select(u => u.CreateTime)
            .Where(u => u != default)
            .ToList();
        if (createTimes.Count > 0)
            canonical.CreateTime = createTimes.Min();

        canonical.UpdateTime = candidates
            .Select(GetLastChangedTime)
            .Max();
    }

    private static DateTime GetLastChangedTime(EntityBase entity)
    {
        return entity.UpdateTime ?? entity.CreateTime;
    }

    private static int GetProfileCompleteness(SaveLifeUserProfile profile)
    {
        return new string?[]
        {
            profile.RealName,
            profile.Gender,
            profile.Phone,
            profile.Email,
            profile.Address,
            profile.Ethnicity,
            profile.EmergencyContact,
            profile.EmergencyPhone,
            profile.MedicalHistory,
            profile.Allergies
        }.Count(u => !string.IsNullOrWhiteSpace(u)) + (profile.BirthDate.HasValue ? 1 : 0);
    }

    private static int GetEnrollmentStatusPriority(string? status)
    {
        return status?.Trim().ToLowerInvariant() switch
        {
            "contacted" => 5,
            "approved" => 4,
            "need_supplement" => 4,
            "pending" => 4,
            "draft" => 3,
            "cancelled" => 2,
            "rejected" => 2,
            "closed" => 1,
            _ => 0
        };
    }

    private static bool IsBusinessExternalUserId(string? externalUserId)
    {
        return !string.IsNullOrWhiteSpace(externalUserId) &&
            !externalUserId.StartsWith(ArchivedExternalUserPrefix, StringComparison.Ordinal);
    }

    private static bool HasTable(ISqlSugarClient db, string tableName)
    {
        return (db.DbMaintenance.GetColumnInfosByTableName(tableName, false)?.Count ?? 0) > 0;
    }

    private static bool HasColumn(ISqlSugarClient db, string tableName, string columnName)
    {
        return db.DbMaintenance.GetColumnInfosByTableName(tableName, false)?
            .Any(u => u.DbColumnName.Equals(columnName, StringComparison.OrdinalIgnoreCase)) == true;
    }

    private static string BuildEnrollmentScopeKey(
        SaveLifeEnrollmentRecord enrollment,
        bool usePatientScope)
    {
        if (usePatientScope && enrollment.PatientProfileId is > 0)
            return $"patient:{enrollment.PatientProfileId.Value}\u001f{enrollment.ProjectId}";

        return $"account:{enrollment.ExternalUserId!.Trim()}\u001f{enrollment.ProjectId}";
    }

    private static void EnsureUniqueIndex(
        ISqlSugarClient db,
        string tableName,
        string indexName,
        IReadOnlyList<string> columns)
    {
        var dbType = db.CurrentConnectionConfig.DbType.ToString();
        var quotedColumns = string.Join(", ", columns.Select(u => QuoteIdentifier(dbType, u)));
        var quotedTable = QuoteIdentifier(dbType, tableName);
        var quotedIndex = QuoteIdentifier(dbType, indexName);

        if (dbType is "Sqlite" or "PostgreSQL" or "Kdbndp")
        {
            db.Ado.ExecuteCommand(
                $"CREATE UNIQUE INDEX IF NOT EXISTS {quotedIndex} ON {quotedTable} ({quotedColumns})");
            return;
        }

        if (dbType == "SqlServer")
        {
            db.Ado.ExecuteCommand($"""
                IF NOT EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = N'{EscapeSqlLiteral(indexName)}'
                      AND object_id = OBJECT_ID(N'{EscapeSqlLiteral(tableName)}')
                )
                CREATE UNIQUE INDEX {quotedIndex} ON {quotedTable} ({quotedColumns})
                """);
            return;
        }

        if (dbType is "MySql" or "MySqlConnector")
        {
            var count = db.Ado.SqlQuery<int>($"""
                SELECT COUNT(1)
                FROM information_schema.statistics
                WHERE table_schema = DATABASE()
                  AND table_name = '{EscapeSqlLiteral(tableName)}'
                  AND index_name = '{EscapeSqlLiteral(indexName)}'
                """).FirstOrDefault();
            if (count == 0)
            {
                db.Ado.ExecuteCommand(
                    $"CREATE UNIQUE INDEX {quotedIndex} ON {quotedTable} ({quotedColumns})");
            }
        }
    }

    private static string QuoteIdentifier(string dbType, string identifier)
    {
        return dbType switch
        {
            "SqlServer" => $"[{identifier.Replace("]", "]]", StringComparison.Ordinal)}]",
            "MySql" or "MySqlConnector" => $"`{identifier.Replace("`", "``", StringComparison.Ordinal)}`",
            _ => $"\"{identifier.Replace("\"", "\"\"", StringComparison.Ordinal)}\""
        };
    }

    private static string EscapeSqlLiteral(string value)
    {
        return value.Replace("'", "''", StringComparison.Ordinal);
    }
}
