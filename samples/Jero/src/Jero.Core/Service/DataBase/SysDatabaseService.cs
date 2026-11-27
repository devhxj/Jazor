


using Npgsql;
namespace Jero.Core.Service;

/// <summary>
/// 系统数据库管理服务 🧩
/// </summary>
[ApiDescriptionSettings(Order = 250)]
public class SysDatabaseService : IDynamicApiController, ITransient
{
    private readonly ISqlSugarClient _db;

    public SysDatabaseService(ISqlSugarClient db)
    {
        _db = db;
    }

    /// <summary>
    /// 获取库列表 🔖
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取库列表")]
    public List<VisualDb> GetList()
    {
        return App.GetOptions<DbConnectionOptions>().ConnectionConfigs
            .Select(u => new VisualDb
            {
                ConfigId = u.ConfigId?.ToString() ?? string.Empty,
                DbNickName = u.DbNickName ?? string.Empty
            })
            .ToList();
    }

    /// <summary>
    /// 获取可视化库表结构 🔖
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取可视化库表结构")]
    public VisualDbTable GetVisualDbTable(string configId = SqlSugarConst.MainConfigId)
    {
        var visualTableList = new List<VisualTable>();
        var visualColumnList = new List<VisualColumn>();
        var columnRelationList = new List<ColumnRelation>();
        var dbOptions = App.GetOptions<DbConnectionOptions>().ConnectionConfigs
            .FirstOrDefault(u => u.ConfigId.ToString() == configId) ?? throw Oops.Oh(ErrorCodeEnum.D1401);

        // 遍历所有实体获取所有库表结构
        var random = new Random();
        foreach (var entityType in TableEntities(configId))
        {
            var entityInfo = _db.EntityMaintenance.GetEntityInfoNoCache(entityType);

            var visualTable = new VisualTable
            {
                TableName = entityInfo.DbTableName,
                TableComents = entityInfo.TableDescription + entityInfo.DbTableName,
                X = random.Next(5000),
                Y = random.Next(5000)
            };
            visualTableList.Add(visualTable);

            foreach (EntityColumnInfo columnInfo in entityInfo.Columns)
            {
                var visualColumn = new VisualColumn
                {
                    TableName = columnInfo.DbTableName,
                    ColumnName = dbOptions.DbSettings.EnableUnderLine ? columnInfo.DbColumnName.ToUnderLine() : columnInfo.DbColumnName,
                    DataType = columnInfo.PropertyInfo.PropertyType.Name,
                    DataLength = columnInfo.Length.ToString(),
                    ColumnDescription = columnInfo.ColumnDescription,
                };
                visualColumnList.Add(visualColumn);

                // 根据导航配置获取表之间关联关系
                if (columnInfo.Navigat != null)
                {
                    var name1 = columnInfo.Navigat.GetName();
                    var name2 = columnInfo.Navigat.GetName2();
                    var targetColumnName = string.IsNullOrEmpty(name2) ? "Id" : name2;
                    var relation = new ColumnRelation
                    {
                        SourceTableName = columnInfo.DbTableName,
                        SourceColumnName = dbOptions.DbSettings.EnableUnderLine ? name1.ToUnderLine() : name1,
                        Type = columnInfo.Navigat.GetNavigateType() == NavigateType.OneToOne ? "ONE_TO_ONE" : "ONE_TO_MANY",
                        TargetTableName = dbOptions.DbSettings.EnableUnderLine ? columnInfo.DbColumnName.ToUnderLine() : columnInfo.DbColumnName,
                        TargetColumnName = dbOptions.DbSettings.EnableUnderLine ? targetColumnName.ToUnderLine() : targetColumnName
                    };
                    columnRelationList.Add(relation);
                }
            }
        }

        return new VisualDbTable { VisualTableList = visualTableList, VisualColumnList = visualColumnList, ColumnRelationList = columnRelationList };
    }

    /// <summary>
    /// 获取字段列表 🔖
    /// </summary>
    /// <param name="tableName">表名</param>
    /// <param name="configId">ConfigId</param>
    /// <returns></returns>
    [DisplayName("获取字段列表")]
    public List<DbColumnOutput> GetColumnList(string tableName, string configId = SqlSugarConst.MainConfigId)
    {
        var db = _db.AsTenant().GetConnectionScope(configId);
        return string.IsNullOrWhiteSpace(tableName) ? new List<DbColumnOutput>() : db.DbMaintenance.GetColumnInfosByTableName(tableName, false).Adapt<List<DbColumnOutput>>();
    }

    /// <summary>
    /// 获取数据库数据类型列表 🔖
    /// </summary>
    /// <param name="configId"></param>
    /// <returns></returns>
    [DisplayName("获取数据库数据类型列表")]
    public List<string> GetDbTypeList(string configId = SqlSugarConst.MainConfigId)
    {
        var db = _db.AsTenant().GetConnectionScope(configId);
        return db.DbMaintenance.GetDbTypes().OrderBy(u => u).ToList();
    }

    /// <summary>
    /// 增加列 🔖
    /// </summary>
    /// <param name="input"></param>
    [ApiDescriptionSettings(Name = "AddColumn"), HttpPost]
    [DisplayName("增加列")]
    public void AddColumn(DbColumnInput input)
    {
        var column = new DbColumnInfo
        {
            ColumnDescription = input.ColumnDescription,
            DbColumnName = input.DbColumnName,
            IsIdentity = input.IsIdentity == 1,
            IsNullable = input.IsNullable == 1,
            IsPrimarykey = input.IsPrimarykey == 1,
            Length = input.Length,
            DecimalDigits = input.DecimalDigits,
            DataType = input.DataType
        };
        var db = _db.AsTenant().GetConnectionScope(input.ConfigId);
        db.DbMaintenance.AddColumn(input.TableName, column);
        // 默认值直接添加报错
        if (!string.IsNullOrWhiteSpace(input.DefaultValue))
        {
            db.DbMaintenance.AddDefaultValue(input.TableName, column.DbColumnName, input.DefaultValue);
        }
        db.DbMaintenance.AddColumnRemark(input.DbColumnName, input.TableName, input.ColumnDescription);
        if (column.IsPrimarykey) db.DbMaintenance.AddPrimaryKey(input.TableName, input.DbColumnName);
    }

    /// <summary>
    /// 删除列 🔖
    /// </summary>
    /// <param name="input"></param>
    [ApiDescriptionSettings(Name = "DeleteColumn"), HttpPost]
    [DisplayName("删除列")]
    public void DeleteColumn(DeleteDbColumnInput input)
    {
        var db = _db.AsTenant().GetConnectionScope(input.ConfigId);
        db.DbMaintenance.DropColumn(input.TableName, input.DbColumnName);
    }

    /// <summary>
    /// 编辑列 🔖
    /// </summary>
    /// <param name="input"></param>
    [ApiDescriptionSettings(Name = "UpdateColumn"), HttpPost]
    [DisplayName("编辑列")]
    public void UpdateColumn(UpdateDbColumnInput input)
    {
        var db = _db.AsTenant().GetConnectionScope(input.ConfigId);

        // 前端未修改列名时，不进行重命名操作，避免报错
        if (input.OldColumnName != input.ColumnName)
        {
            db.DbMaintenance.RenameColumn(input.TableName, input.OldColumnName, input.ColumnName);
        }

        if (!string.IsNullOrWhiteSpace(input.DefaultValue))
        {
            db.DbMaintenance.AddDefaultValue(input.TableName, input.ColumnName, input.DefaultValue);
        }
        //if (db.DbMaintenance.IsAnyColumnRemark(input.ColumnName, input.TableName))
        //{
        //    db.DbMaintenance.DeleteColumnRemark(input.ColumnName, input.TableName);
        //}

        db.DbMaintenance.AddColumnRemark(input.ColumnName, input.TableName, string.IsNullOrWhiteSpace(input.Description) ? input.ColumnName : input.Description);
    }

    /// <summary>
    /// 移动列位置 🔖
    /// </summary>
    /// <param name="input"></param>
    [ApiDescriptionSettings(Name = "MoveColumn"), HttpPost]
    [DisplayName("移动列")]
    public void MoveColumn(MoveDbColumnInput input)
    {
        var db = _db.AsTenant().GetConnectionScope(input.ConfigId);
        var dbMaintenance = db.DbMaintenance;

        var dbType = db.CurrentConnectionConfig.DbType;

        var columns = dbMaintenance.GetColumnInfosByTableName(input.TableName, false);
        var targetColumn = columns.FirstOrDefault(c =>
            c.DbColumnName.Equals(input.ColumnName, StringComparison.OrdinalIgnoreCase));

        if (targetColumn == null)
            throw new Exception($"列 {input.ColumnName} 在表 {input.TableName} 中不存在");

        switch (dbType)
        {
            case SqlSugar.DbType.MySql:
                MoveColumnInMySQL(db, input.TableName, input.ColumnName, input.AfterColumnName);
                break;

            default:
                throw new NotSupportedException($"暂不支持 {dbType} 数据库的列移动操作");
        }
    }

    /// <summary>
    /// 获取列定义
    /// </summary>
    /// <param name="db"></param>
    /// <param name="tableName"></param>
    /// <param name="columnName"></param>
    /// <param name="noDefault"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private string GetColumnDefinitionInMySQL(ISqlSugarClient db, string tableName, string columnName, bool noDefault = false)
    {
        var columnDef = db.Ado.SqlQuery<dynamic>(
            $"SHOW FULL COLUMNS FROM `{tableName}` WHERE Field = '{columnName}'"
        ).FirstOrDefault();

        if (columnDef == null)
            throw new Exception($"Column {columnName} not found");

        var definition = new StringBuilder();
        definition.Append($"`{columnName}` ");  // 列名
        definition.Append($"{columnDef.Type} "); // 数据类型

        // 处理约束条件
        definition.Append(columnDef.Null == "YES" ? "NULL " : "NOT NULL ");
        if (columnDef.Default != null && !noDefault)
            definition.Append($"DEFAULT '{columnDef.Default}' ");
        if (!string.IsNullOrEmpty(columnDef.Extra))
            definition.Append($"{columnDef.Extra} ");
        if (!string.IsNullOrEmpty(columnDef.Comment))
            definition.Append($"COMMENT '{columnDef.Comment.Replace("'", "''")}'");

        return definition.ToString();
    }

    /// <summary>
    /// MySQL 列移动实现
    /// </summary>
    /// <param name="db"></param>
    /// <param name="tableName"></param>
    /// <param name="columnName"></param>
    /// <param name="afterColumnName"></param>
    private void MoveColumnInMySQL(ISqlSugarClient db, string tableName, string columnName, string afterColumnName)
    {
        var definition = GetColumnDefinitionInMySQL(db, tableName, columnName);
        var sql = new StringBuilder();
        sql.Append($"ALTER TABLE `{tableName}` MODIFY COLUMN {definition}");

        if (string.IsNullOrEmpty(afterColumnName))
            sql.Append(" FIRST");
        else
            sql.Append($" AFTER `{afterColumnName}`");

        db.Ado.ExecuteCommand(sql.ToString());
    }

    /// <summary>
    /// 获取表列表 🔖
    /// </summary>
    /// <param name="configId">ConfigId</param>
    /// <returns></returns>
    [DisplayName("获取表列表")]
    public List<DbTableInfo> GetTableList(string configId = SqlSugarConst.MainConfigId)
    {
        var db = _db.AsTenant().GetConnectionScope(configId);
        return db.DbMaintenance.GetTableInfoList(false);
    }

    /// <summary>获取已映射实体的数据库表。</summary>
    [DisplayName("获取已映射实体的数据库表")]
    public List<DatabaseEntityTable> GetEntityTables(string configId = SqlSugarConst.MainConfigId)
    {
        var db = _db.AsTenant().GetConnectionScope(configId);
        var tables = db.DbMaintenance.GetTableInfoList(false);
        var result = new List<DatabaseEntityTable>();
        foreach (var entityType in TableEntities(configId))
        {
            var entity = db.EntityMaintenance.GetEntityInfoNoCache(entityType);
            // Some databases allow case-distinct identifiers. Prefer the ORM's exact mapped spelling.
            var table = tables.FirstOrDefault(table => table.Name == entity.DbTableName) ??
                tables.FirstOrDefault(table => string.Equals(table.Name, entity.DbTableName, StringComparison.OrdinalIgnoreCase));
            if (table is not null)
                result.Add(new(table.Name, entity.TableDescription ?? table.Description, entityType.Name));
        }
        return result;
    }

    private static IEnumerable<Type> TableEntities(string configId) =>
        App.EffectiveTypes
            .Where(type => type.IsClass && !type.IsAbstract && type.IsDefined(typeof(SugarTable), false))
            .Where(type =>
            {
                var entityConfigId = type.IsDefined(typeof(LogTableAttribute)) ? SqlSugarConst.LogConfigId :
                    type.GetCustomAttribute<TenantAttribute>()?.configId.ToString() ?? SqlSugarConst.MainConfigId;
                return entityConfigId == configId;
            });

    /// <summary>
    /// 增加表 🔖
    /// </summary>
    /// <param name="input"></param>
    [ApiDescriptionSettings(Name = "AddTable"), HttpPost]
    [DisplayName("增加表")]
    public void AddTable(DbTableInput input)
    {
        if (input.DbColumnInfoList == null || !input.DbColumnInfoList.Any())
            throw Oops.Oh(ErrorCodeEnum.db1000);

        if (input.DbColumnInfoList.GroupBy(u => u.DbColumnName).Any(u => u.Count() > 1))
            throw Oops.Oh(ErrorCodeEnum.db1002);

        var config = App.GetOptions<DbConnectionOptions>().ConnectionConfigs.FirstOrDefault(u => u.ConfigId.ToString() == input.ConfigId)
            ?? throw Oops.Oh(ErrorCodeEnum.D1401);
        var db = _db.AsTenant().GetConnectionScope(input.ConfigId);
        var typeBuilder = db.DynamicBuilder().CreateClass(input.TableName, new SugarTable() { TableName = input.TableName, TableDescription = input.Description });
        input.DbColumnInfoList.ForEach(u =>
        {
            var dbColumnName = config!.DbSettings.EnableUnderLine ? u.DbColumnName.Trim().ToUnderLine() : u.DbColumnName.Trim();
            // 虚拟类都默认string类型，具体以列数据类型为准
            typeBuilder.CreateProperty(dbColumnName, typeof(string), new SugarColumn()
            {
                IsPrimaryKey = u.IsPrimarykey == 1,
                IsIdentity = u.IsIdentity == 1,
                ColumnDataType = u.DataType,
                Length = u.Length,
                IsNullable = u.IsNullable == 1,
                DecimalDigits = u.DecimalDigits,
                ColumnDescription = u.ColumnDescription,
                DefaultValue = u.DefaultValue,
            });
        });
        db.CodeFirst.InitTables(typeBuilder.BuilderType());
    }

    /// <summary>
    /// 删除表 🔖
    /// </summary>
    /// <param name="input"></param>
    [ApiDescriptionSettings(Name = "DeleteTable"), HttpPost]
    [DisplayName("删除表")]
    public void DeleteTable(DeleteDbTableInput input)
    {
        var db = _db.AsTenant().GetConnectionScope(input.ConfigId);
        db.DbMaintenance.DropTable(input.TableName);
    }

    /// <summary>
    /// 编辑表 🔖
    /// </summary>
    /// <param name="input"></param>
    [ApiDescriptionSettings(Name = "UpdateTable"), HttpPost]
    [DisplayName("编辑表")]
    public void UpdateTable(UpdateDbTableInput input)
    {
        var db = _db.AsTenant().GetConnectionScope(input.ConfigId);
        db.DbMaintenance.RenameTable(input.OldTableName, input.TableName);
        try
        {
            if (db.DbMaintenance.IsAnyTableRemark(input.TableName))
                db.DbMaintenance.DeleteTableRemark(input.TableName);

            if (!string.IsNullOrWhiteSpace(input.Description))
                db.DbMaintenance.AddTableRemark(input.TableName, input.Description);
        }
        catch (NotSupportedException ex)
        {
            throw Oops.Oh(ex.ToString());
        }
    }

    /// <summary>
    /// 备份数据库（PostgreSQL）🔖
    /// </summary>
    /// <returns></returns>
    [HttpPost, NonUnify]
    [DisplayName("备份数据库（PostgreSQL）")]
    public async Task<IActionResult> BackupDatabase()
    {
        if (_db.CurrentConnectionConfig.DbType != SqlSugar.DbType.PostgreSQL)
            throw Oops.Oh("只支持 PostgreSQL 数据库 😁");

        var npgsqlConn = new NpgsqlConnectionStringBuilder(_db.CurrentConnectionConfig.ConnectionString);
        if (npgsqlConn == null || string.IsNullOrWhiteSpace(npgsqlConn.Host) || string.IsNullOrWhiteSpace(npgsqlConn.Username) || string.IsNullOrWhiteSpace(npgsqlConn.Password) || string.IsNullOrWhiteSpace(npgsqlConn.Database))
            throw Oops.Oh("PostgreSQL 数据库配置错误");

        // 确保备份目录存在
        var backupDirectory = Path.Combine(Directory.GetCurrentDirectory(), "backups");
        Directory.CreateDirectory(backupDirectory);

        // 构建备份文件名
        string backupFileName = $"backup_{DateTime.Now:yyyyMMddHHmmss}.sql";
        string backupFilePath = Path.Combine(backupDirectory, backupFileName);

        // 启动pg_dump进程进行备份
        // 设置密码：export PGPASSWORD='xxxxxx'
        var bash = $"-U {npgsqlConn.Username} -h {npgsqlConn.Host} -p {npgsqlConn.Port} -E UTF8 -F c -b -v -f {backupFilePath} {npgsqlConn.Database}";
        var startInfo = new ProcessStartInfo
        {
            FileName = "pg_dump",
            Arguments = bash,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            EnvironmentVariables =
            {
                ["PGPASSWORD"] = npgsqlConn.Password
            }
        };

        //_logger.LogInformation("备份数据库：pg_dump " + bash);

        //try
        //{
        using (var backupProcess = Process.Start(startInfo))
        {
            if (backupProcess == null)
                throw Oops.Oh("启动 pg_dump 进程失败");

            await backupProcess.WaitForExitAsync();

            //var output = await backupProcess.StandardOutput.ReadToEndAsync();
            //var error = await backupProcess.StandardError.ReadToEndAsync();

            // 检查备份是否成功
            if (backupProcess.ExitCode != 0)
            {
                throw Oops.Oh($"备份失败：ExitCode({backupProcess.ExitCode})");
            }
        }

        //    _logger.LogInformation($"备份成功：{backupFilePath}");
        //}
        //catch (Exception ex)
        //{
        //    _logger.LogError(ex, $"备份失败：");
        //    throw;
        //}

        // 若备份成功则提供下载链接
        return new FileStreamResult(new FileStream(backupFilePath, FileMode.Open), "application/octet-stream")
        {
            FileDownloadName = backupFileName
        };
    }
}
