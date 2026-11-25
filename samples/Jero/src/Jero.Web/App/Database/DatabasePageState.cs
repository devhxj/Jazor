using Jero.Web.App.Components;
using Jero.Web.App.Shell;
using Jero.Web.App.Http;
using static ECMAScript.Global;

namespace Jero.Web.App.Database;

[ECMAScriptModule("components/database-page-state.js")]
public static class DatabasePageModule
{
    public static DatabasePageState Create() => new();

    public sealed class DatabasePageState : global::Jero.Web.App.IPageState<DatabasePageProps>
    {
        private DatabaseOptionView[] databaseItems = [];
        private DatabaseTableView[] tableItems = [];
        private DatabaseColumnView[] columnItems = [];
        private DatabaseColumnView[] columnRows = [];
        private string[] databaseTypeOptions = [];
        private string[] applicationNamespaces = [];
        private DatabaseNamedOption[] entityBaseClassOptions = [];

        private string configId = string.Empty;
        private string selectedTableName = string.Empty;
        private bool loadingDatabases;
        private bool loadingTables;
        private bool loadingColumns;
        private bool loadingNamespaces;
        private bool loadingEntityBaseClasses;
        private string busyKey = string.Empty;
        private string? error;

        private int databaseRequestVersion;
        private int tableRequestVersion;
        private int tableRequestPending;
        private int columnRequestVersion;

        private bool addTableDialogVisible;
        private string addTableName = string.Empty;
        private string addTableDescription = string.Empty;
        private DatabaseColumnDraft[] addTableColumns = [];
        private DatabaseColumnDraft[] addTableColumnRows = [];
        private int nextDraftKey;
        private int editingDraftKey;
        private string draftColumnName = string.Empty;
        private string draftColumnDescription = string.Empty;
        private string draftDataType = string.Empty;
        private int draftLength;
        private int draftDecimalDigits;
        private string draftDefaultValue = string.Empty;
        private int draftIsPrimaryKey;
        private int draftIsIdentity;
        private int draftIsNullable = 1;

        private bool editTableDialogVisible;
        private string editTableOldName = string.Empty;
        private string editTableName = string.Empty;
        private string editTableDescription = string.Empty;

        private bool addColumnDialogVisible;
        private string addColumnName = string.Empty;
        private string addColumnDescription = string.Empty;
        private string addColumnDataType = string.Empty;
        private int addColumnLength;
        private int addColumnDecimalDigits;
        private string addColumnDefaultValue = string.Empty;
        private int addColumnIsPrimaryKey;
        private int addColumnIsIdentity;
        private int addColumnIsNullable;

        private bool editColumnDialogVisible;
        private string editColumnOldName = string.Empty;
        private string editColumnName = string.Empty;
        private string editColumnDescription = string.Empty;
        private string editColumnDefaultValue = string.Empty;

        private bool entityDialogVisible;
        private string entityName = string.Empty;
        private string entityBaseClassName = "EntityBase";
        private string entityPosition = string.Empty;

        private bool seedDialogVisible;
        private string seedPosition = string.Empty;
        private string seedSuffix = string.Empty;
        private bool seedFilterExistingData;

        private readonly JeroNavigation Navigation = new();

        private bool Busy => busyKey.Length > 0;
        private bool HasDatabase => configId.Trim().Length > 0;
        private bool HasTable => HasDatabase && selectedTableName.Trim().Length > 0;
        private DatabaseTableView? SelectedTable => FindTable(selectedTableName);

        public void Initialize()
        {
            LoadDatabases();
            LoadApplicationNamespaces();
            LoadEntityBaseClasses();
        }

        private void LoadDatabases()
        {
            var requestVersion = ++databaseRequestVersion;
            loadingDatabases = true;
            DatabaseClient.Databases().Then(outcome =>
            {
                if (requestVersion != databaseRequestVersion)
                    return;

                loadingDatabases = false;
                if (!outcome.Ok)
                {
                    databaseItems = [];
                    error = outcome.Error ?? "加载数据库列表失败。";
                    return;
                }

                databaseItems = DatabaseClient.ReadDatabases(outcome.Data);
                error = null;
                if (configId.Length == 0 && databaseItems.Length > 0)
                {
                    configId = databaseItems[0].configId;
                    StartTableLoad(string.Empty);
                }
            }, _ =>
            {
                if (requestVersion != databaseRequestVersion)
                    return;
                loadingDatabases = false;
                databaseItems = [];
                error = "加载数据库列表失败，请检查网络后重试。";
            });
        }

        private void LoadApplicationNamespaces()
        {
            if (loadingNamespaces)
                return;

            loadingNamespaces = true;
            DatabaseClient.ApplicationNamespaces().Then(outcome =>
            {
                loadingNamespaces = false;
                if (!outcome.Ok)
                {
                    applicationNamespaces = [];
                    error = outcome.Error ?? "加载应用命名空间失败。";
                    return;
                }

                applicationNamespaces = DatabaseClient.ReadStrings(outcome.Data);
            }, _ =>
            {
                loadingNamespaces = false;
                applicationNamespaces = [];
                error = "加载应用命名空间失败，请检查网络后重试。";
            });
        }

        private void LoadEntityBaseClasses()
        {
            if (loadingEntityBaseClasses)
                return;

            loadingEntityBaseClasses = true;
            DatabaseClient.EntityBaseClasses().Then(outcome =>
            {
                loadingEntityBaseClasses = false;
                if (!outcome.Ok)
                {
                    entityBaseClassOptions = [];
                    error = outcome.Error ?? "加载实体基类字典失败。";
                    return;
                }

                entityBaseClassOptions = DatabaseClient.ReadNamedOptions(outcome.Data);
                if (entityDialogVisible && !ContainsEntityBaseClass(entityBaseClassName))
                    entityBaseClassName = DefaultEntityBaseClass();
            }, _ =>
            {
                loadingEntityBaseClasses = false;
                entityBaseClassOptions = [];
                error = "加载实体基类字典失败，请检查网络后重试。";
            });
        }

        private void ChangeDatabase(string? value)
        {
            configId = value ?? string.Empty;
            StartTableLoad(string.Empty);
        }

        private void StartTableLoad(string desiredSelection)
        {
            var currentConfigId = configId.Trim();
            var requestVersion = ++tableRequestVersion;
            ++columnRequestVersion;

            selectedTableName = desiredSelection;
            columnItems = [];
            columnRows = [];
            loadingColumns = false;

            if (currentConfigId.Length == 0)
            {
                tableItems = [];
                databaseTypeOptions = [];
                loadingTables = false;
                tableRequestPending = 0;
                return;
            }

            loadingTables = true;
            tableRequestPending = 2;
            DatabaseClient.Tables(currentConfigId).Then(outcome =>
            {
                if (!IsCurrentTableRequest(requestVersion, currentConfigId))
                    return;

                if (!outcome.Ok)
                {
                    tableItems = [];
                    selectedTableName = string.Empty;
                    error = outcome.Error ?? "加载数据表失败。";
                }
                else
                {
                    tableItems = DatabaseClient.ReadTables(outcome.Data);
                    var desiredTable = FindTable(desiredSelection);
                    if (desiredTable is null)
                    {
                        selectedTableName = string.Empty;
                    }
                    else
                    {
                        selectedTableName = desiredTable.name;
                        LoadColumns();
                    }
                }
                FinishTableRequest(requestVersion, currentConfigId);
            }, _ =>
            {
                if (!IsCurrentTableRequest(requestVersion, currentConfigId))
                    return;
                tableItems = [];
                selectedTableName = string.Empty;
                error = "加载数据表失败，请检查网络后重试。";
                FinishTableRequest(requestVersion, currentConfigId);
            });

            DatabaseClient.DatabaseTypes(currentConfigId).Then(outcome =>
            {
                if (!IsCurrentTableRequest(requestVersion, currentConfigId))
                    return;
                if (!outcome.Ok)
                {
                    databaseTypeOptions = [];
                    error = outcome.Error ?? "加载数据库类型失败。";
                }
                else
                {
                    databaseTypeOptions = DatabaseClient.ReadStrings(outcome.Data);
                }
                FinishTableRequest(requestVersion, currentConfigId);
            }, _ =>
            {
                if (!IsCurrentTableRequest(requestVersion, currentConfigId))
                    return;
                databaseTypeOptions = [];
                error = "加载数据库类型失败，请检查网络后重试。";
                FinishTableRequest(requestVersion, currentConfigId);
            });
        }

        private bool IsCurrentTableRequest(int requestVersion, string requestConfigId)
            => requestVersion == tableRequestVersion && requestConfigId == configId.Trim();

        private void FinishTableRequest(int requestVersion, string requestConfigId)
        {
            if (!IsCurrentTableRequest(requestVersion, requestConfigId))
                return;

            tableRequestPending--;
            if (tableRequestPending <= 0)
                loadingTables = false;
        }

        private void ChangeTable(string? value)
        {
            selectedTableName = value ?? string.Empty;
            LoadColumns();
        }

        private void LoadColumns()
        {
            var currentConfigId = configId.Trim();
            var currentTableName = selectedTableName.Trim();
            var requestVersion = ++columnRequestVersion;
            columnItems = [];
            columnRows = [];

            if (currentConfigId.Length == 0 || currentTableName.Length == 0)
            {
                loadingColumns = false;
                return;
            }

            loadingColumns = true;
            DatabaseClient.Columns(currentTableName, currentConfigId).Then(outcome =>
            {
                if (!IsCurrentColumnRequest(requestVersion, currentConfigId, currentTableName))
                    return;

                loadingColumns = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载数据列失败。";
                    return;
                }

                columnItems = DatabaseClient.ReadColumns(outcome.Data);
                columnRows = columnItems;
                error = null;
            }, _ =>
            {
                if (!IsCurrentColumnRequest(requestVersion, currentConfigId, currentTableName))
                    return;
                loadingColumns = false;
                error = "加载数据列失败，请检查网络后重试。";
            });
        }

        private bool IsCurrentColumnRequest(
            int requestVersion,
            string requestConfigId,
            string requestTableName)
            => requestVersion == columnRequestVersion &&
               requestConfigId == configId.Trim() &&
               requestTableName == selectedTableName.Trim();

        private void Refresh()
        {
            if (databaseItems.Length == 0)
            {
                LoadDatabases();
                return;
            }
            StartTableLoad(selectedTableName);
        }

        private void OpenAddTable()
        {
            if (!EnsureDatabase("新增数据表"))
                return;

            addTableName = string.Empty;
            addTableDescription = string.Empty;
            addTableColumns = [];
            addTableColumnRows = [];
            nextDraftKey = 0;
            ResetDraftEditor();
            addTableDialogVisible = true;
        }

        private void ResetDraftEditor()
        {
            editingDraftKey = 0;
            draftColumnName = string.Empty;
            draftColumnDescription = string.Empty;
            draftDataType = string.Empty;
            draftLength = 0;
            draftDecimalDigits = 0;
            draftDefaultValue = string.Empty;
            draftIsPrimaryKey = 0;
            draftIsIdentity = 0;
            draftIsNullable = 1;
        }

        private void ChangeDraftDataType(string? value)
        {
            draftDataType = value ?? string.Empty;
            NormalizeDraftType();
        }

        private void NormalizeDraftType()
        {
            if (IsStringType(draftDataType))
            {
                if (draftLength <= 0)
                    draftLength = 32;
                draftDecimalDigits = 0;
                return;
            }

            if (draftDataType != "decimal")
            {
                draftLength = 0;
                draftDecimalDigits = 0;
            }
        }

        private void SaveDraftColumn()
        {
            var name = draftColumnName.Trim();
            var description = draftColumnDescription.Trim();
            var dataType = draftDataType.Trim();
            if (name.Length == 0 || description.Length == 0 || dataType.Length == 0)
            {
                ElementPlus.ElMessageMembers.Warning("字段名、描述和数据类型不能为空");
                return;
            }

            NormalizeDraftType();
            var draft = new DatabaseColumnDraft
            {
                key = editingDraftKey == 0 ? ++nextDraftKey : editingDraftKey,
                dbColumnName = name,
                columnDescription = description,
                dataType = dataType,
                length = draftLength < 0 ? 0 : draftLength,
                decimalDigits = draftDecimalDigits < 0 ? 0 : draftDecimalDigits,
                defaultValue = draftDefaultValue.Trim(),
                isPrimarykey = draftIsPrimaryKey == 1 ? 1 : 0,
                isIdentity = draftIsIdentity == 1 ? 1 : 0,
                isNullable = draftIsNullable == 1 ? 1 : 0
            };

            if (editingDraftKey == 0)
                AppendDraft(draft);
            else
                ReplaceDraft(draft);
            ResetDraftEditor();
        }

        private void AppendDraft(DatabaseColumnDraft draft)
        {
            var result = new DatabaseColumnDraft[addTableColumns.Length + 1];
            for (var index = 0; index < addTableColumns.Length; index++)
                result[index] = addTableColumns[index];
            result[result.Length - 1] = draft;
            addTableColumns = result;
            addTableColumnRows = addTableColumns;
        }

        private void ReplaceDraft(DatabaseColumnDraft draft)
        {
            var result = new DatabaseColumnDraft[addTableColumns.Length];
            for (var index = 0; index < addTableColumns.Length; index++)
                result[index] = addTableColumns[index].key == draft.key ? draft : addTableColumns[index];
            addTableColumns = result;
            addTableColumnRows = addTableColumns;
        }

        private void EditDraft(int key)
        {
            var draft = FindDraft(key);
            if (draft is null)
                return;
            editingDraftKey = draft.key;
            draftColumnName = draft.dbColumnName;
            draftColumnDescription = draft.columnDescription;
            draftDataType = draft.dataType;
            draftLength = draft.length;
            draftDecimalDigits = draft.decimalDigits;
            draftDefaultValue = draft.defaultValue;
            draftIsPrimaryKey = draft.isPrimarykey;
            draftIsIdentity = draft.isIdentity;
            draftIsNullable = draft.isNullable;
        }

        private void RemoveDraft(int key)
        {
            var count = 0;
            for (var index = 0; index < addTableColumns.Length; index++)
                if (addTableColumns[index].key != key)
                    count++;

            var result = new DatabaseColumnDraft[count];
            var targetIndex = 0;
            for (var index = 0; index < addTableColumns.Length; index++)
            {
                if (addTableColumns[index].key == key)
                    continue;
                result[targetIndex++] = addTableColumns[index];
            }
            addTableColumns = result;
            addTableColumnRows = addTableColumns;
            if (editingDraftKey == key)
                ResetDraftEditor();
        }

        private void MoveDraft(int key, bool moveUp)
        {
            var index = FindDraftIndex(key);
            var targetIndex = moveUp ? index - 1 : index + 1;
            if (index < 0 || targetIndex < 0 || targetIndex >= addTableColumns.Length)
                return;

            var current = addTableColumns[index];
            addTableColumns[index] = addTableColumns[targetIndex];
            addTableColumns[targetIndex] = current;
            addTableColumnRows = addTableColumns;
        }

        private void AddPrimaryPreset()
            => AppendDraft(CreatePreset("Id", "主键Id", "bigint", 0, 0, 1, 0));

        private void AddTenantPreset()
            => AppendDraft(CreatePreset("TenantId", "租户Id", "bigint", 0, 1, 0, 0));

        private void AddOrgPreset()
            => AppendDraft(CreatePreset("OrgId", "机构Id", "bigint", 0, 1, 0, 0));

        private void AddBasePresets()
        {
            AppendDraft(CreatePreset("CreateTime", "创建时间", "datetime", 0, 1, 0, 0));
            AppendDraft(CreatePreset("UpdateTime", "更新时间", "datetime", 0, 1, 0, 0));
            AppendDraft(CreatePreset("CreateUserId", "创建者Id", "bigint", 0, 1, 0, 0));
            AppendDraft(CreatePreset("CreateUserName", "创建者姓名", "varchar", 64, 1, 0, 0));
            AppendDraft(CreatePreset("UpdateUserId", "修改者Id", "bigint", 0, 1, 0, 0));
            AppendDraft(CreatePreset("UpdateUserName", "修改者姓名", "varchar", 64, 1, 0, 0));
        }

        private void AddSoftDeletePreset()
            => AppendDraft(CreatePreset("IsDelete", "软删除", "bit", 0, 0, 0, 0));

        private DatabaseColumnDraft CreatePreset(
            string name,
            string description,
            string dataType,
            int length,
            int nullable,
            int primaryKey,
            int identity)
            => new()
            {
                key = ++nextDraftKey,
                dbColumnName = name,
                columnDescription = description,
                dataType = dataType,
                length = length,
                decimalDigits = 0,
                defaultValue = string.Empty,
                isNullable = nullable,
                isPrimarykey = primaryKey,
                isIdentity = identity
            };

        private void SaveAddTable()
        {
            if (Busy)
                return;

            var tableName = addTableName.Trim();
            var description = addTableDescription.Trim();
            if (tableName.Length == 0 || description.Length == 0)
            {
                error = "表名称和描述不能为空。";
                return;
            }
            if (addTableColumns.Length == 0)
            {
                error = "请至少添加一个字段。";
                return;
            }

            for (var index = 0; index < addTableColumns.Length; index++)
            {
                var column = addTableColumns[index];
                if (column.dbColumnName.Trim().Length == 0 ||
                    column.columnDescription.Trim().Length == 0 ||
                    column.dataType.Trim().Length == 0)
                {
                    error = "字段名、描述和数据类型不能为空。";
                    return;
                }
                for (var otherIndex = index + 1; otherIndex < addTableColumns.Length; otherIndex++)
                {
                    if (column.dbColumnName.Trim().ToLowerInvariant() ==
                        addTableColumns[otherIndex].dbColumnName.Trim().ToLowerInvariant())
                    {
                        error = "字段【" + column.dbColumnName.Trim() + "】重复。";
                        return;
                    }
                }
            }

            var columns = new DatabaseAddColumnInput[addTableColumns.Length];
            for (var index = 0; index < addTableColumns.Length; index++)
                columns[index] = ToAddColumnInput(addTableColumns[index], tableName);

            busyKey = "add-table";
            DatabaseClient.AddTable(new DatabaseAddTableInput
            {
                configId = configId.Trim(),
                tableName = tableName,
                description = description,
                dbColumnInfoList = columns
            }).Then(outcome =>
            {
                busyKey = string.Empty;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "新增数据表失败。";
                    return;
                }
                addTableDialogVisible = false;
                error = null;
                ElementPlus.ElMessageMembers.Success("数据表已新增");
                StartTableLoad(tableName);
            }, _ =>
            {
                busyKey = string.Empty;
                error = "新增数据表失败，请检查网络后重试。";
            });
        }

        private void OpenEditTable()
        {
            if (!EnsureTable("编辑数据表"))
                return;
            var table = SelectedTable;
            editTableOldName = selectedTableName;
            editTableName = selectedTableName;
            editTableDescription = table?.description ?? string.Empty;
            editTableDialogVisible = true;
        }

        private void SaveEditTable()
        {
            if (Busy)
                return;
            var tableName = editTableName.Trim();
            var description = editTableDescription.Trim();
            if (tableName.Length == 0 || description.Length == 0)
            {
                error = "表名称和描述不能为空。";
                return;
            }

            busyKey = "edit-table";
            DatabaseClient.UpdateTable(new DatabaseUpdateTableInput
            {
                configId = configId.Trim(),
                tableName = tableName,
                oldTableName = editTableOldName,
                description = description
            }).Then(outcome =>
            {
                busyKey = string.Empty;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "编辑数据表失败。";
                    return;
                }
                editTableDialogVisible = false;
                error = null;
                ElementPlus.ElMessageMembers.Success("数据表已更新");
                StartTableLoad(tableName);
            }, _ =>
            {
                busyKey = string.Empty;
                error = "编辑数据表失败，请检查网络后重试。";
            });
        }

        private void DeleteTable()
        {
            if (!EnsureTable("删除数据表") || !BeginAction("delete-table"))
                return;
            DatabaseClient.DeleteTable(new DatabaseDeleteTableInput
            {
                configId = configId.Trim(),
                tableName = selectedTableName.Trim()
            }).Then(outcome =>
            {
                busyKey = string.Empty;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "删除数据表失败。";
                    return;
                }
                error = null;
                ElementPlus.ElMessageMembers.Success("数据表已删除");
                StartTableLoad(string.Empty);
            }, _ =>
            {
                busyKey = string.Empty;
                error = "删除数据表失败，请检查网络后重试。";
            });
        }

        private void OpenAddColumn()
        {
            if (!EnsureTable("新增数据列"))
                return;
            addColumnName = string.Empty;
            addColumnDescription = string.Empty;
            addColumnDataType = string.Empty;
            addColumnLength = 0;
            addColumnDecimalDigits = 0;
            addColumnDefaultValue = string.Empty;
            addColumnIsPrimaryKey = 0;
            addColumnIsIdentity = 0;
            addColumnIsNullable = 0;
            addColumnDialogVisible = true;
        }

        private void ChangeAddColumnDataType(string? value)
        {
            addColumnDataType = value ?? string.Empty;
            if (IsStringType(addColumnDataType))
            {
                if (addColumnLength <= 0)
                    addColumnLength = 32;
                addColumnDecimalDigits = 0;
            }
            else if (addColumnDataType != "decimal")
            {
                addColumnLength = 0;
                addColumnDecimalDigits = 0;
            }
        }

        private void SaveAddColumn()
        {
            if (Busy)
                return;
            var name = addColumnName.Trim();
            var description = addColumnDescription.Trim();
            var dataType = addColumnDataType.Trim();
            if (name.Length == 0 || description.Length == 0 || dataType.Length == 0)
            {
                error = "列名称、描述和数据类型不能为空。";
                return;
            }

            busyKey = "add-column";
            DatabaseClient.AddColumn(new DatabaseAddColumnInput
            {
                configId = configId.Trim(),
                tableName = selectedTableName.Trim(),
                dbColumnName = name,
                columnDescription = description,
                dataType = dataType,
                length = addColumnLength < 0 ? 0 : addColumnLength,
                decimalDigits = addColumnDecimalDigits < 0 ? 0 : addColumnDecimalDigits,
                defaultValue = addColumnDefaultValue.Trim(),
                isPrimarykey = addColumnIsPrimaryKey == 1 ? 1 : 0,
                isIdentity = addColumnIsIdentity == 1 ? 1 : 0,
                isNullable = addColumnIsNullable == 1 ? 1 : 0
            }).Then(outcome =>
            {
                busyKey = string.Empty;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "新增数据列失败。";
                    return;
                }
                addColumnDialogVisible = false;
                error = null;
                ElementPlus.ElMessageMembers.Success("数据列已新增");
                LoadColumns();
            }, _ =>
            {
                busyKey = string.Empty;
                error = "新增数据列失败，请检查网络后重试。";
            });
        }

        private void OpenEditColumn(string columnName)
        {
            var row = FindColumn(columnName);
            if (row is null)
                return;
            editColumnOldName = row.dbColumnName;
            editColumnName = row.dbColumnName;
            editColumnDescription = row.columnDescription;
            editColumnDefaultValue = row.defaultValue;
            editColumnDialogVisible = true;
        }

        private void SaveEditColumn()
        {
            if (Busy)
                return;
            var columnName = editColumnName.Trim();
            var description = editColumnDescription.Trim();
            if (columnName.Length == 0 || description.Length == 0)
            {
                error = "列名称和描述不能为空。";
                return;
            }

            busyKey = "edit-column";
            DatabaseClient.UpdateColumn(new DatabaseUpdateColumnInput
            {
                configId = configId.Trim(),
                tableName = selectedTableName.Trim(),
                columnName = columnName,
                oldColumnName = editColumnOldName,
                description = description,
                defaultValue = editColumnDefaultValue.Trim()
            }).Then(outcome =>
            {
                busyKey = string.Empty;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "编辑数据列失败。";
                    return;
                }
                editColumnDialogVisible = false;
                error = null;
                ElementPlus.ElMessageMembers.Success("数据列已更新");
                LoadColumns();
            }, _ =>
            {
                busyKey = string.Empty;
                error = "编辑数据列失败，请检查网络后重试。";
            });
        }

        private void DeleteColumn(string columnName)
        {
            if (columnName.Length == 0 || !BeginAction("delete-column:" + columnName))
                return;
            DatabaseClient.DeleteColumn(new DatabaseDeleteColumnInput
            {
                configId = configId.Trim(),
                tableName = selectedTableName.Trim(),
                dbColumnName = columnName
            }).Then(outcome =>
            {
                busyKey = string.Empty;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "删除数据列失败。";
                    return;
                }
                error = null;
                ElementPlus.ElMessageMembers.Success("数据列已删除");
                LoadColumns();
            }, _ =>
            {
                busyKey = string.Empty;
                error = "删除数据列失败，请检查网络后重试。";
            });
        }

        private void MoveColumn(string rowName, bool moveUp)
        {
            var index = FindColumnIndex(rowName);
            var targetIndex = moveUp ? index - 1 : index + 1;
            if (index < 0 || targetIndex < 0 || targetIndex >= columnItems.Length)
            {
                ElementPlus.ElMessageMembers.Warning(moveUp ? "已处于首位" : "已处于末位");
                return;
            }

            var columnName = moveUp ? columnItems[targetIndex].dbColumnName : columnItems[index].dbColumnName;
            var afterColumnName = moveUp ? columnItems[index].dbColumnName : columnItems[targetIndex].dbColumnName;
            if (!BeginAction("move-column:" + rowName))
                return;

            DatabaseClient.MoveColumn(new DatabaseMoveColumnInput
            {
                configId = configId.Trim(),
                tableName = selectedTableName.Trim(),
                columnName = columnName,
                afterColumnName = afterColumnName
            }).Then(outcome =>
            {
                busyKey = string.Empty;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "移动数据列失败。";
                    return;
                }
                error = null;
                ElementPlus.ElMessageMembers.Success("数据列位置已更新");
                LoadColumns();
            }, _ =>
            {
                busyKey = string.Empty;
                error = "移动数据列失败，请检查网络后重试。";
            });
        }

        private void OpenCreateEntity()
        {
            if (!EnsureTable("生成实体"))
                return;
            if (applicationNamespaces.Length == 0 && !loadingNamespaces)
                LoadApplicationNamespaces();
            if (entityBaseClassOptions.Length == 0 && !loadingEntityBaseClasses)
                LoadEntityBaseClasses();
            entityName = ToPascalCase(selectedTableName);
            entityBaseClassName = DefaultEntityBaseClass();
            entityPosition = applicationNamespaces.Length == 0 ? string.Empty : applicationNamespaces[0];
            entityDialogVisible = true;
        }

        private void SaveEntity()
        {
            if (Busy)
                return;
            var position = entityPosition.Trim();
            if (position.Length == 0)
            {
                error = "请选择实体存放位置。";
                return;
            }
            if (!ContainsEntityBaseClass(entityBaseClassName))
            {
                error = "请选择实体基类字典中的现有项。";
                return;
            }

            busyKey = "create-entity";
            DatabaseClient.CreateEntity(new DatabaseCreateEntityInput
            {
                configId = configId.Trim(),
                tableName = selectedTableName.Trim(),
                entityName = entityName.Trim(),
                baseClassName = entityBaseClassName.Trim(),
                position = position
            }).Then(outcome =>
            {
                busyKey = string.Empty;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "生成实体失败。";
                    return;
                }
                entityDialogVisible = false;
                error = null;
                ElementPlus.ElMessageMembers.Success("实体已生成");
            }, _ =>
            {
                busyKey = string.Empty;
                error = "生成实体失败，请检查网络后重试。";
            });
        }

        private void OpenCreateSeedData()
        {
            if (!EnsureTable("生成种子数据"))
                return;
            if (applicationNamespaces.Length == 0 && !loadingNamespaces)
                LoadApplicationNamespaces();
            seedPosition = applicationNamespaces.Length == 0 ? string.Empty : applicationNamespaces[0];
            seedSuffix = string.Empty;
            seedFilterExistingData = false;
            seedDialogVisible = true;
        }

        private void SaveSeedData()
        {
            if (Busy)
                return;
            var position = seedPosition.Trim();
            if (position.Length == 0)
            {
                error = "请选择种子数据存放位置。";
                return;
            }

            busyKey = "create-seed";
            DatabaseClient.CreateSeedData(new DatabaseCreateSeedDataInput
            {
                configId = configId.Trim(),
                tableName = selectedTableName.Trim(),
                position = position,
                suffix = seedSuffix.Trim(),
                filterExistingData = seedFilterExistingData
            }).Then(outcome =>
            {
                busyKey = string.Empty;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "生成种子数据失败。";
                    return;
                }
                seedDialogVisible = false;
                error = null;
                ElementPlus.ElMessageMembers.Success("种子数据已生成");
            }, _ =>
            {
                busyKey = string.Empty;
                error = "生成种子数据失败，请检查网络后重试。";
            });
        }

        private void OpenVisual()
        {
            if (!EnsureDatabase("查看可视化"))
                return;
            Navigation.NavigateTo(
                Navigation.BaseUri + "database/visual?configId=" + EncodeURIComponent(configId.Trim()));
        }

        private bool EnsureDatabase(string action)
        {
            if (HasDatabase)
                return true;
            ElementPlus.ElMessageMembers.Warning("请先选择库名后再" + action);
            return false;
        }

        private bool EnsureTable(string action)
        {
            if (HasTable)
                return true;
            ElementPlus.ElMessageMembers.Warning("请先选择库名和表名后再" + action);
            return false;
        }

        private bool BeginAction(string key)
        {
            if (Busy)
                return false;
            busyKey = key;
            error = null;
            return true;
        }

        private DatabaseTableView? FindTable(string name)
        {
            for (var index = 0; index < tableItems.Length; index++)
                if (tableItems[index].name == name)
                    return tableItems[index];
            return null;
        }

        private DatabaseColumnView? FindColumn(string name)
        {
            for (var index = 0; index < columnItems.Length; index++)
                if (columnItems[index].dbColumnName == name)
                    return columnItems[index];
            return null;
        }

        private int FindColumnIndex(string name)
        {
            for (var index = 0; index < columnItems.Length; index++)
                if (columnItems[index].dbColumnName == name)
                    return index;
            return -1;
        }

        private DatabaseColumnDraft? FindDraft(int key)
        {
            for (var index = 0; index < addTableColumns.Length; index++)
                if (addTableColumns[index].key == key)
                    return addTableColumns[index];
            return null;
        }

        private int FindDraftIndex(int key)
        {
            for (var index = 0; index < addTableColumns.Length; index++)
                if (addTableColumns[index].key == key)
                    return index;
            return -1;
        }

        private DatabaseAddColumnInput ToAddColumnInput(DatabaseColumnDraft draft, string tableName)
            => new()
            {
                configId = configId.Trim(),
                tableName = tableName,
                dbColumnName = draft.dbColumnName.Trim(),
                dataType = draft.dataType.Trim(),
                length = draft.length,
                columnDescription = draft.columnDescription.Trim(),
                isNullable = draft.isNullable,
                isIdentity = draft.isIdentity,
                isPrimarykey = draft.isPrimarykey,
                decimalDigits = draft.decimalDigits,
                defaultValue = draft.defaultValue.Trim()
            };

        private static bool IsStringType(string dataType)
        {
            var normalized = dataType.Trim().ToLowerInvariant();
            return normalized == "char" || normalized == "nchar" ||
                   normalized == "nvarchar" || normalized == "varchar";
        }

        private static string ToPascalCase(string value)
        {
            var result = string.Empty;
            var capitalize = true;
            for (var index = 0; index < value.Length; index++)
            {
                var character = value[index];
                if (character == '_' || character == '-' || char.IsWhiteSpace(character))
                {
                    capitalize = true;
                    continue;
                }
                var text = character.ToString();
                result += capitalize ? text.ToUpperInvariant() : text;
                capitalize = false;
            }
            return result;
        }

        private string DefaultEntityBaseClass()
        {
            if (ContainsEntityBaseClass("EntityBase"))
                return "EntityBase";
            return entityBaseClassOptions.Length == 0 ? string.Empty : entityBaseClassOptions[0].value;
        }

        private bool ContainsEntityBaseClass(string value)
        {
            for (var index = 0; index < entityBaseClassOptions.Length; index++)
                if (entityBaseClassOptions[index].value == value)
                    return true;
            return false;
        }

        private ConfigurationOption[] DatabaseOptions()
        {
            var result = new ConfigurationOption[databaseItems.Length];
            for (var index = 0; index < result.Length; index++)
                result[index] = new ConfigurationOption(databaseItems[index].configId, DatabaseLabel(databaseItems[index]));
            return result;
        }

        private ConfigurationOption[] TableOptions()
        {
            var result = new ConfigurationOption[tableItems.Length];
            for (var index = 0; index < result.Length; index++)
                result[index] = new ConfigurationOption(tableItems[index].name, TableLabel(tableItems[index]));
            return result;
        }

        private ConfigurationOption[] EntityBaseOptions()
        {
            var result = new ConfigurationOption[entityBaseClassOptions.Length];
            for (var index = 0; index < result.Length; index++)
                result[index] = new ConfigurationOption(entityBaseClassOptions[index].value, entityBaseClassOptions[index].label);
            return result;
        }

        private ConfigurationOption[] DataTypeOptions() => ToOptions(databaseTypeOptions);

        private ConfigurationOption[] NamespaceOptions() => ToOptions(applicationNamespaces);

        private static ConfigurationOption[] ToOptions(string[] values)
        {
            var result = new ConfigurationOption[values.Length];
            for (var index = 0; index < result.Length; index++)
                result[index] = new ConfigurationOption(values[index], values[index]);
            return result;
        }

        private static string DatabaseLabel(DatabaseOptionView item)
            => item.dbNickName.Length == 0 ? item.configId : item.dbNickName;

        private static string TableLabel(DatabaseTableView item)
            => item.description.Length == 0 ? item.name : item.name + "（" + item.description + "）";

        public DatabasePageProps CreateProps() => new()
        {
            ColumnRows = columnRows,
            ConfigId = configId,
            SelectedTableName = selectedTableName,
            LoadingDatabases = loadingDatabases,
            LoadingTables = loadingTables,
            LoadingColumns = loadingColumns,
            LoadingNamespaces = loadingNamespaces,
            LoadingEntityBaseClasses = loadingEntityBaseClasses,
            BusyKey = busyKey,
            Error = error,
            AddTableDialogVisible = addTableDialogVisible,
            AddTableName = addTableName,
            AddTableDescription = addTableDescription,
            AddTableColumnRows = addTableColumnRows,
            EditingDraftKey = editingDraftKey,
            DraftColumnName = draftColumnName,
            DraftColumnDescription = draftColumnDescription,
            DraftDataType = draftDataType,
            DraftLength = draftLength,
            DraftDecimalDigits = draftDecimalDigits,
            DraftDefaultValue = draftDefaultValue,
            DraftIsPrimaryKey = draftIsPrimaryKey,
            DraftIsIdentity = draftIsIdentity,
            DraftIsNullable = draftIsNullable,
            EditTableDialogVisible = editTableDialogVisible,
            EditTableName = editTableName,
            EditTableDescription = editTableDescription,
            AddColumnDialogVisible = addColumnDialogVisible,
            AddColumnName = addColumnName,
            AddColumnDescription = addColumnDescription,
            AddColumnDataType = addColumnDataType,
            AddColumnLength = addColumnLength,
            AddColumnDecimalDigits = addColumnDecimalDigits,
            AddColumnDefaultValue = addColumnDefaultValue,
            AddColumnIsPrimaryKey = addColumnIsPrimaryKey,
            AddColumnIsIdentity = addColumnIsIdentity,
            AddColumnIsNullable = addColumnIsNullable,
            EditColumnDialogVisible = editColumnDialogVisible,
            EditColumnName = editColumnName,
            EditColumnDescription = editColumnDescription,
            EditColumnDefaultValue = editColumnDefaultValue,
            EntityDialogVisible = entityDialogVisible,
            EntityName = entityName,
            EntityBaseClassName = entityBaseClassName,
            EntityPosition = entityPosition,
            SeedDialogVisible = seedDialogVisible,
            SeedPosition = seedPosition,
            SeedSuffix = seedSuffix,
            SeedFilterExistingData = seedFilterExistingData,
            Busy = Busy,
            HasTable = HasTable,
            HasDatabase = HasDatabase,
            DatabaseOptions = DatabaseOptions(),
            TableOptions = TableOptions(),
            DataTypeOptions = DataTypeOptions(),
            NamespaceOptions = NamespaceOptions(),
            EntityBaseOptions = EntityBaseOptions(),
            Refresh = Refresh,
            DismissError = () => error = null,
            ChangeDatabase = ChangeDatabase,
            ChangeTable = ChangeTable,
            OpenAddTable = OpenAddTable,
            OpenEditTable = OpenEditTable,
            DeleteTable = DeleteTable,
            OpenVisual = OpenVisual,
            OpenAddColumn = OpenAddColumn,
            OpenCreateEntity = OpenCreateEntity,
            OpenCreateSeedData = OpenCreateSeedData,
            MoveColumnUp = value => MoveColumn(value, true),
            MoveColumnDown = value => MoveColumn(value, false),
            OpenEditColumn = OpenEditColumn,
            DeleteColumn = DeleteColumn,
            SetAddTableDialogVisible = value => addTableDialogVisible = value,
            SetAddTableName = value => addTableName = value ?? "",
            SetAddTableDescription = value => addTableDescription = value ?? "",
            AddPrimaryPreset = AddPrimaryPreset,
            AddTenantPreset = AddTenantPreset,
            AddOrgPreset = AddOrgPreset,
            AddBasePresets = AddBasePresets,
            AddSoftDeletePreset = AddSoftDeletePreset,
            SetDraftColumnName = value => draftColumnName = value ?? "",
            SetDraftColumnDescription = value => draftColumnDescription = value ?? "",
            ChangeDraftDataType = ChangeDraftDataType,
            SetDraftDefaultValue = value => draftDefaultValue = value ?? "",
            SetDraftLength = value => draftLength = (int)(value ?? 0),
            SetDraftDecimalDigits = value => draftDecimalDigits = (int)(value ?? 0),
            SetDraftIsPrimaryKey = value => draftIsPrimaryKey = (int)value,
            SetDraftIsIdentity = value => draftIsIdentity = (int)value,
            SetDraftIsNullable = value => draftIsNullable = (int)value,
            ResetDraftEditor = ResetDraftEditor,
            SaveDraftColumn = SaveDraftColumn,
            MoveDraftUp = value => MoveDraft((int)value, true),
            MoveDraftDown = value => MoveDraft((int)value, false),
            EditDraft = value => EditDraft((int)value),
            RemoveDraft = value => RemoveDraft((int)value),
            SaveAddTable = SaveAddTable,
            SetEditTableDialogVisible = value => editTableDialogVisible = value,
            SetEditTableName = value => editTableName = value ?? "",
            SetEditTableDescription = value => editTableDescription = value ?? "",
            SaveEditTable = SaveEditTable,
            SetAddColumnDialogVisible = value => addColumnDialogVisible = value,
            SetAddColumnName = value => addColumnName = value ?? "",
            SetAddColumnDescription = value => addColumnDescription = value ?? "",
            ChangeAddColumnDataType = ChangeAddColumnDataType,
            SetAddColumnDefaultValue = value => addColumnDefaultValue = value ?? "",
            SetAddColumnLength = value => addColumnLength = (int)(value ?? 0),
            SetAddColumnDecimalDigits = value => addColumnDecimalDigits = (int)(value ?? 0),
            SetAddColumnIsPrimaryKey = value => addColumnIsPrimaryKey = (int)value,
            SetAddColumnIsIdentity = value => addColumnIsIdentity = (int)value,
            SetAddColumnIsNullable = value => addColumnIsNullable = (int)value,
            SaveAddColumn = SaveAddColumn,
            SetEditColumnDialogVisible = value => editColumnDialogVisible = value,
            SetEditColumnName = value => editColumnName = value ?? "",
            SetEditColumnDescription = value => editColumnDescription = value ?? "",
            SetEditColumnDefaultValue = value => editColumnDefaultValue = value ?? "",
            SaveEditColumn = SaveEditColumn,
            SetEntityDialogVisible = value => entityDialogVisible = value,
            SetEntityName = value => entityName = value ?? "",
            SetEntityBaseClassName = value => entityBaseClassName = value ?? "",
            SetEntityPosition = value => entityPosition = value ?? "",
            SaveEntity = SaveEntity,
            SetSeedDialogVisible = value => seedDialogVisible = value,
            SetSeedSuffix = value => seedSuffix = value ?? "",
            SetSeedPosition = value => seedPosition = value ?? "",
            SetSeedFilterExistingData = value => seedFilterExistingData = value == 1,
            SaveSeedData = SaveSeedData,
        };
    }
}

public sealed record DatabasePageProps : VueProps
{
    public required DatabaseColumnView[] ColumnRows { get; init; }
    public required string ConfigId { get; init; }
    public required string SelectedTableName { get; init; }
    public required bool LoadingDatabases { get; init; }
    public required bool LoadingTables { get; init; }
    public required bool LoadingColumns { get; init; }
    public required bool LoadingNamespaces { get; init; }
    public required bool LoadingEntityBaseClasses { get; init; }
    public required string BusyKey { get; init; }
    public required string? Error { get; init; }
    public required bool AddTableDialogVisible { get; init; }
    public required string AddTableName { get; init; }
    public required string AddTableDescription { get; init; }
    public required DatabaseColumnDraft[] AddTableColumnRows { get; init; }
    public required int EditingDraftKey { get; init; }
    public required string DraftColumnName { get; init; }
    public required string DraftColumnDescription { get; init; }
    public required string DraftDataType { get; init; }
    public required int DraftLength { get; init; }
    public required int DraftDecimalDigits { get; init; }
    public required string DraftDefaultValue { get; init; }
    public required int DraftIsPrimaryKey { get; init; }
    public required int DraftIsIdentity { get; init; }
    public required int DraftIsNullable { get; init; }
    public required bool EditTableDialogVisible { get; init; }
    public required string EditTableName { get; init; }
    public required string EditTableDescription { get; init; }
    public required bool AddColumnDialogVisible { get; init; }
    public required string AddColumnName { get; init; }
    public required string AddColumnDescription { get; init; }
    public required string AddColumnDataType { get; init; }
    public required int AddColumnLength { get; init; }
    public required int AddColumnDecimalDigits { get; init; }
    public required string AddColumnDefaultValue { get; init; }
    public required int AddColumnIsPrimaryKey { get; init; }
    public required int AddColumnIsIdentity { get; init; }
    public required int AddColumnIsNullable { get; init; }
    public required bool EditColumnDialogVisible { get; init; }
    public required string EditColumnName { get; init; }
    public required string EditColumnDescription { get; init; }
    public required string EditColumnDefaultValue { get; init; }
    public required bool EntityDialogVisible { get; init; }
    public required string EntityName { get; init; }
    public required string EntityBaseClassName { get; init; }
    public required string EntityPosition { get; init; }
    public required bool SeedDialogVisible { get; init; }
    public required string SeedPosition { get; init; }
    public required string SeedSuffix { get; init; }
    public required bool SeedFilterExistingData { get; init; }
    public required bool Busy { get; init; }
    public required bool HasTable { get; init; }
    public required bool HasDatabase { get; init; }
    public required ConfigurationOption[] DatabaseOptions { get; init; }
    public required ConfigurationOption[] TableOptions { get; init; }
    public required ConfigurationOption[] DataTypeOptions { get; init; }
    public required ConfigurationOption[] NamespaceOptions { get; init; }
    public required ConfigurationOption[] EntityBaseOptions { get; init; }
    public required Action Refresh { get; init; }
    public required Action DismissError { get; init; }
    public required Action<string> ChangeDatabase { get; init; }
    public required Action<string> ChangeTable { get; init; }
    public required Action OpenAddTable { get; init; }
    public required Action OpenEditTable { get; init; }
    public required Action DeleteTable { get; init; }
    public required Action OpenVisual { get; init; }
    public required Action OpenAddColumn { get; init; }
    public required Action OpenCreateEntity { get; init; }
    public required Action OpenCreateSeedData { get; init; }
    public required Action<string> MoveColumnUp { get; init; }
    public required Action<string> MoveColumnDown { get; init; }
    public required Action<string> OpenEditColumn { get; init; }
    public required Action<string> DeleteColumn { get; init; }
    public required Action<bool> SetAddTableDialogVisible { get; init; }
    public required Action<string?> SetAddTableName { get; init; }
    public required Action<string?> SetAddTableDescription { get; init; }
    public required Action AddPrimaryPreset { get; init; }
    public required Action AddTenantPreset { get; init; }
    public required Action AddOrgPreset { get; init; }
    public required Action AddBasePresets { get; init; }
    public required Action AddSoftDeletePreset { get; init; }
    public required Action<string?> SetDraftColumnName { get; init; }
    public required Action<string?> SetDraftColumnDescription { get; init; }
    public required Action<string> ChangeDraftDataType { get; init; }
    public required Action<string?> SetDraftDefaultValue { get; init; }
    public required Action<double?> SetDraftLength { get; init; }
    public required Action<double?> SetDraftDecimalDigits { get; init; }
    public required Action<double> SetDraftIsPrimaryKey { get; init; }
    public required Action<double> SetDraftIsIdentity { get; init; }
    public required Action<double> SetDraftIsNullable { get; init; }
    public required Action ResetDraftEditor { get; init; }
    public required Action SaveDraftColumn { get; init; }
    public required Action<double> MoveDraftUp { get; init; }
    public required Action<double> MoveDraftDown { get; init; }
    public required Action<double> EditDraft { get; init; }
    public required Action<double> RemoveDraft { get; init; }
    public required Action SaveAddTable { get; init; }
    public required Action<bool> SetEditTableDialogVisible { get; init; }
    public required Action<string?> SetEditTableName { get; init; }
    public required Action<string?> SetEditTableDescription { get; init; }
    public required Action SaveEditTable { get; init; }
    public required Action<bool> SetAddColumnDialogVisible { get; init; }
    public required Action<string?> SetAddColumnName { get; init; }
    public required Action<string?> SetAddColumnDescription { get; init; }
    public required Action<string> ChangeAddColumnDataType { get; init; }
    public required Action<string?> SetAddColumnDefaultValue { get; init; }
    public required Action<double?> SetAddColumnLength { get; init; }
    public required Action<double?> SetAddColumnDecimalDigits { get; init; }
    public required Action<double> SetAddColumnIsPrimaryKey { get; init; }
    public required Action<double> SetAddColumnIsIdentity { get; init; }
    public required Action<double> SetAddColumnIsNullable { get; init; }
    public required Action SaveAddColumn { get; init; }
    public required Action<bool> SetEditColumnDialogVisible { get; init; }
    public required Action<string?> SetEditColumnName { get; init; }
    public required Action<string?> SetEditColumnDescription { get; init; }
    public required Action<string?> SetEditColumnDefaultValue { get; init; }
    public required Action SaveEditColumn { get; init; }
    public required Action<bool> SetEntityDialogVisible { get; init; }
    public required Action<string?> SetEntityName { get; init; }
    public required Action<string?> SetEntityBaseClassName { get; init; }
    public required Action<string?> SetEntityPosition { get; init; }
    public required Action SaveEntity { get; init; }
    public required Action<bool> SetSeedDialogVisible { get; init; }
    public required Action<string?> SetSeedSuffix { get; init; }
    public required Action<string?> SetSeedPosition { get; init; }
    public required Action<double> SetSeedFilterExistingData { get; init; }
    public required Action SaveSeedData { get; init; }
}
