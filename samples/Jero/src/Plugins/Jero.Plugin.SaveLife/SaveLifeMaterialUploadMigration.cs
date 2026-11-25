using SqlSugar;
using Jero.Plugin.SaveLife.Service;

namespace Jero.Plugin.SaveLife;

public static class SaveLifeMaterialUploadMigration
{
    public const string CommandScopeUniqueIndexName = "uq_sl_material_upload_command_scope";
    public const string ActiveSlotUniqueIndexName = "uq_sl_enrollment_material_active_slot";
    public const string OutboxAggregateUniqueIndexName = "uq_sl_material_work_outbox_aggregate";

    public static void Apply(ISqlSugarClient db)
    {
        ArgumentNullException.ThrowIfNull(db);
        if (db.CurrentConnectionConfig.DbType != DbType.Sqlite)
            throw new NotSupportedException($"当前数据库 {db.CurrentConnectionConfig.DbType} 尚未实现材料上传迁移");

        db.CodeFirst.InitTables(
            typeof(SaveLifeEnrollmentMaterial),
            typeof(SaveLifeMaterialUploadCommand),
            typeof(SaveLifeMaterialWorkOutbox));
        db.Ado.BeginTran();
        try
        {
            BackfillActiveSlots(db);
            BackfillProcessingRevisions(db);
            db.Ado.ExecuteCommand($"""
                CREATE UNIQUE INDEX IF NOT EXISTS "{CommandScopeUniqueIndexName}"
                ON "sl_material_upload_command" (
                    IFNULL("TenantId", 0),
                    "ExternalUserId",
                    "PatientProfileId",
                    "EnrollmentRecordId",
                    "ClientCommandId"
                )
                """);
            db.Ado.ExecuteCommand($"""
                CREATE UNIQUE INDEX IF NOT EXISTS "{ActiveSlotUniqueIndexName}"
                ON "sl_enrollment_material" (
                    IFNULL("TenantId", 0),
                    "ExternalUserId",
                    "PatientProfileId",
                    "EnrollmentRecordId",
                    "UploadSlot"
                )
                WHERE "IsDelete" = 0 AND "UploadSlot" IS NOT NULL
                """);
            db.Ado.ExecuteCommand($"""
                CREATE UNIQUE INDEX IF NOT EXISTS "{OutboxAggregateUniqueIndexName}"
                ON "sl_material_work_outbox" (
                    IFNULL("TenantId", 0),
                    "WorkType",
                    "AggregateId"
                )
                """);
            db.Ado.CommitTran();
        }
        catch
        {
            db.Ado.RollbackTran();
            throw;
        }
    }

    private static void BackfillProcessingRevisions(ISqlSugarClient db)
    {
        db.Updateable<SaveLifeEnrollmentMaterial>()
            .SetColumns(item => new SaveLifeEnrollmentMaterial { ProcessingRevision = 1 })
            .Where(item => item.ProcessingRevision <= 0)
            .ExecuteCommand();
    }

    private static void BackfillActiveSlots(ISqlSugarClient db)
    {
        var groups = db.Queryable<SaveLifeEnrollmentMaterial>()
            .ClearFilter()
            .Where(item => !item.IsDelete)
            .ToList()
            .GroupBy(item => new
            {
                TenantScope = item.TenantId ?? 0,
                item.ExternalUserId,
                item.PatientProfileId,
                item.EnrollmentRecordId
            });
        foreach (var group in groups)
        {
            var materials = group.OrderBy(item => item.CreateTime).ThenBy(item => item.Id).ToList();
            var occupied = materials
                .Where(item => item.UploadSlot is > 0)
                .Select(item => item.UploadSlot!.Value)
                .ToList();
            if (occupied.Count != occupied.Distinct().Count())
                throw new InvalidOperationException("材料上传迁移发现重复活动槽位，迁移已回滚");

            var assignedSlots = occupied.ToHashSet();
            foreach (var material in materials.Where(item => item.UploadSlot is null or <= 0))
            {
                var slot = SaveLifeEnrollmentMaterialHelper.ResolveNextUploadSlot(assignedSlots);
                assignedSlots.Add(slot);
                material.UploadSlot = slot;
                db.Updateable(material)
                    .UpdateColumns(item => item.UploadSlot)
                    .ExecuteCommand();
            }
        }
    }
}
