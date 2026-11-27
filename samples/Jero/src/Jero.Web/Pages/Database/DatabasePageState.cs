using Jero.Web.Application;
using Jero.Web.Infrastructure.Api.Database;
using Jero.Web.Pages.Components;
using Jero.Web.Pages.Shell;
using Jero.Web.Infrastructure.Http;
using static ECMAScript.Global;

namespace Jero.Web.Pages.Database;

[ECMAScriptModule("components/database-page-state.js")]
public static class DatabasePageModule
{
    public static DatabasePageState Create() => new();

    public sealed class DatabasePageState : global::Jero.Web.Composition.IPageState<DatabasePageProps>
    {
        private DatabaseOptionView[] _databaseItems = [];
        private DatabaseTableView[] _tableItems = [];
        private DatabaseColumnView[] _columnItems = [];
        private DatabaseColumnView[] _columnRows = [];
        private string[] _databaseTypeOptions = [];

        private string _configId = string.Empty;
        private string _selectedTableName = string.Empty;
        private bool _loadingDatabases;
        private bool _loadingTables;
        private bool _loadingColumns;
        private string _busyKey = string.Empty;
        private string? _error;

        private int _databaseRequestVersion;
        private int _tableRequestVersion;
        private int _tableRequestPending;
        private int _columnRequestVersion;

        private bool _addTableDialogVisible;
        private string _addTableName = string.Empty;
        private string _addTableDescription = string.Empty;
        private DatabaseColumnDraft[] _addTableColumns = [];
        private DatabaseColumnDraft[] _addTableColumnRows = [];
        private int _nextDraftKey;
        private int _editingDraftKey;
        private string _draftColumnName = string.Empty;
        private string _draftColumnDescription = string.Empty;
        private string _draftDataType = string.Empty;
        private int _draftLength;
        private int _draftDecimalDigits;
        private string _draftDefaultValue = string.Empty;
        private int _draftIsPrimaryKey;
        private int _draftIsIdentity;
        private int _draftIsNullable = 1;

        private bool _editTableDialogVisible;
        private string _editTableOldName = string.Empty;
        private string _editTableName = string.Empty;
        private string _editTableDescription = string.Empty;

        private bool _addColumnDialogVisible;
        private string _addColumnName = string.Empty;
        private string _addColumnDescription = string.Empty;
        private string _addColumnDataType = string.Empty;
        private int _addColumnLength;
        private int _addColumnDecimalDigits;
        private string _addColumnDefaultValue = string.Empty;
        private int _addColumnIsPrimaryKey;
        private int _addColumnIsIdentity;
        private int _addColumnIsNullable;

        private bool _editColumnDialogVisible;
        private string _editColumnOldName = string.Empty;
        private string _editColumnName = string.Empty;
        private string _editColumnDescription = string.Empty;
        private string _editColumnDefaultValue = string.Empty;



        private readonly JeroNavigation _navigation = new();

        private bool Busy => _busyKey.Length > 0;
        private bool HasDatabase => _configId.Trim().Length > 0;
        private bool HasTable => HasDatabase && _selectedTableName.Trim().Length > 0;
        private DatabaseTableView? SelectedTable => FindTable(_selectedTableName);

        public void Initialize()
        {
            LoadDatabases();
        }

        private void LoadDatabases()
        {
            var requestVersion = ++_databaseRequestVersion;
            _loadingDatabases = true;
            DatabaseClient.Databases().Then(outcome =>
            {
                if (requestVersion != _databaseRequestVersion)
                    return;

                _loadingDatabases = false;
                if (!outcome.Ok)
                {
                    _databaseItems = [];
                    _error = outcome.Error ?? "加载数据库列表失败。";
                    return;
                }

                _databaseItems = DatabaseClient.ReadDatabases(outcome.Data);
                _error = null;
                if (_configId.Length == 0 && _databaseItems.Length > 0)
                {
                    _configId = _databaseItems[0].ConfigId;
                    StartTableLoad(string.Empty);
                }
            }, _ =>
            {
                if (requestVersion != _databaseRequestVersion)
                    return;
                _loadingDatabases = false;
                _databaseItems = [];
                _error = "加载数据库列表失败，请检查网络后重试。";
            });
        }

        private void ChangeDatabase(string? value)
        {
            _configId = value ?? string.Empty;
            StartTableLoad(string.Empty);
        }

        private void StartTableLoad(string desiredSelection)
        {
            var currentConfigId = _configId.Trim();
            var requestVersion = ++_tableRequestVersion;
            ++_columnRequestVersion;

            _selectedTableName = desiredSelection;
            _columnItems = [];
            _columnRows = [];
            _loadingColumns = false;

            if (currentConfigId.Length == 0)
            {
                _tableItems = [];
                _databaseTypeOptions = [];
                _loadingTables = false;
                _tableRequestPending = 0;
                return;
            }

            _loadingTables = true;
            _tableRequestPending = 2;
            DatabaseClient.Tables(currentConfigId).Then(outcome =>
            {
                if (!IsCurrentTableRequest(requestVersion, currentConfigId))
                    return;

                if (!outcome.Ok)
                {
                    _tableItems = [];
                    _selectedTableName = string.Empty;
                    _error = outcome.Error ?? "加载数据表失败。";
                }
                else
                {
                    _tableItems = DatabaseClient.ReadTables(outcome.Data);
                    var desiredTable = FindTable(desiredSelection);
                    if (desiredTable is null)
                    {
                        _selectedTableName = string.Empty;
                    }
                    else
                    {
                        _selectedTableName = desiredTable.Name;
                        LoadColumns();
                    }
                }
                FinishTableRequest(requestVersion, currentConfigId);
            }, _ =>
            {
                if (!IsCurrentTableRequest(requestVersion, currentConfigId))
                    return;
                _tableItems = [];
                _selectedTableName = string.Empty;
                _error = "加载数据表失败，请检查网络后重试。";
                FinishTableRequest(requestVersion, currentConfigId);
            });

            DatabaseClient.DatabaseTypes(currentConfigId).Then(outcome =>
            {
                if (!IsCurrentTableRequest(requestVersion, currentConfigId))
                    return;
                if (!outcome.Ok)
                {
                    _databaseTypeOptions = [];
                    _error = outcome.Error ?? "加载数据库类型失败。";
                }
                else
                {
                    _databaseTypeOptions = DatabaseClient.ReadStrings(outcome.Data);
                }
                FinishTableRequest(requestVersion, currentConfigId);
            }, _ =>
            {
                if (!IsCurrentTableRequest(requestVersion, currentConfigId))
                    return;
                _databaseTypeOptions = [];
                _error = "加载数据库类型失败，请检查网络后重试。";
                FinishTableRequest(requestVersion, currentConfigId);
            });
        }

        private bool IsCurrentTableRequest(int requestVersion, string requestConfigId)
            => requestVersion == _tableRequestVersion && requestConfigId == _configId.Trim();

        private void FinishTableRequest(int requestVersion, string requestConfigId)
        {
            if (!IsCurrentTableRequest(requestVersion, requestConfigId))
                return;

            _tableRequestPending--;
            if (_tableRequestPending <= 0)
                _loadingTables = false;
        }

        private void ChangeTable(string? value)
        {
            _selectedTableName = value ?? string.Empty;
            LoadColumns();
        }

        private void LoadColumns()
        {
            var currentConfigId = _configId.Trim();
            var currentTableName = _selectedTableName.Trim();
            var requestVersion = ++_columnRequestVersion;
            _columnItems = [];
            _columnRows = [];

            if (currentConfigId.Length == 0 || currentTableName.Length == 0)
            {
                _loadingColumns = false;
                return;
            }

            _loadingColumns = true;
            DatabaseClient.Columns(currentTableName, currentConfigId).Then(outcome =>
            {
                if (!IsCurrentColumnRequest(requestVersion, currentConfigId, currentTableName))
                    return;

                _loadingColumns = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载数据列失败。";
                    return;
                }

                _columnItems = DatabaseClient.ReadColumns(outcome.Data);
                _columnRows = _columnItems;
                _error = null;
            }, _ =>
            {
                if (!IsCurrentColumnRequest(requestVersion, currentConfigId, currentTableName))
                    return;
                _loadingColumns = false;
                _error = "加载数据列失败，请检查网络后重试。";
            });
        }

        private bool IsCurrentColumnRequest(
            int requestVersion,
            string requestConfigId,
            string requestTableName)
            => requestVersion == _columnRequestVersion &&
               requestConfigId == _configId.Trim() &&
               requestTableName == _selectedTableName.Trim();

        private void Refresh()
        {
            if (_databaseItems.Length == 0)
            {
                LoadDatabases();
                return;
            }
            StartTableLoad(_selectedTableName);
        }

        private void OpenAddTable()
        {
            if (!EnsureDatabase("新增数据表"))
                return;

            _addTableName = string.Empty;
            _addTableDescription = string.Empty;
            _addTableColumns = [];
            _addTableColumnRows = [];
            _nextDraftKey = 0;
            ResetDraftEditor();
            _addTableDialogVisible = true;
        }

        private void ResetDraftEditor()
        {
            _editingDraftKey = 0;
            _draftColumnName = string.Empty;
            _draftColumnDescription = string.Empty;
            _draftDataType = string.Empty;
            _draftLength = 0;
            _draftDecimalDigits = 0;
            _draftDefaultValue = string.Empty;
            _draftIsPrimaryKey = 0;
            _draftIsIdentity = 0;
            _draftIsNullable = 1;
        }

        private void ChangeDraftDataType(string? value)
        {
            _draftDataType = value ?? string.Empty;
            NormalizeDraftType();
        }

        private void NormalizeDraftType()
        {
            if (IsStringType(_draftDataType))
            {
                if (_draftLength <= 0)
                    _draftLength = 32;
                _draftDecimalDigits = 0;
                return;
            }

            if (_draftDataType != "decimal")
            {
                _draftLength = 0;
                _draftDecimalDigits = 0;
            }
        }

        private void SaveDraftColumn()
        {
            var name = _draftColumnName.Trim();
            var description = _draftColumnDescription.Trim();
            var dataType = _draftDataType.Trim();
            if (name.Length == 0 || description.Length == 0 || dataType.Length == 0)
            {
                ElementPlus.ElMessageMembers.Warning("字段名、描述和数据类型不能为空");
                return;
            }

            NormalizeDraftType();
            var draft = new DatabaseColumnDraft
            {
                Key = _editingDraftKey == 0 ? ++_nextDraftKey : _editingDraftKey,
                DbColumnName = name,
                ColumnDescription = description,
                DataType = dataType,
                Length = _draftLength < 0 ? 0 : _draftLength,
                DecimalDigits = _draftDecimalDigits < 0 ? 0 : _draftDecimalDigits,
                DefaultValue = _draftDefaultValue.Trim(),
                IsPrimarykey = _draftIsPrimaryKey == 1 ? 1 : 0,
                IsIdentity = _draftIsIdentity == 1 ? 1 : 0,
                IsNullable = _draftIsNullable == 1 ? 1 : 0
            };

            if (_editingDraftKey == 0)
                AppendDraft(draft);
            else
                ReplaceDraft(draft);
            ResetDraftEditor();
        }

        private void AppendDraft(DatabaseColumnDraft draft)
        {
            var result = new DatabaseColumnDraft[_addTableColumns.Length + 1];
            for (var index = 0; index < _addTableColumns.Length; index++)
                result[index] = _addTableColumns[index];
            result[result.Length - 1] = draft;
            _addTableColumns = result;
            _addTableColumnRows = _addTableColumns;
        }

        private void ReplaceDraft(DatabaseColumnDraft draft)
        {
            var result = new DatabaseColumnDraft[_addTableColumns.Length];
            for (var index = 0; index < _addTableColumns.Length; index++)
                result[index] = _addTableColumns[index].Key == draft.Key ? draft : _addTableColumns[index];
            _addTableColumns = result;
            _addTableColumnRows = _addTableColumns;
        }

        private void EditDraft(int key)
        {
            var draft = FindDraft(key);
            if (draft is null)
                return;
            _editingDraftKey = draft.Key;
            _draftColumnName = draft.DbColumnName;
            _draftColumnDescription = draft.ColumnDescription;
            _draftDataType = draft.DataType;
            _draftLength = draft.Length;
            _draftDecimalDigits = draft.DecimalDigits;
            _draftDefaultValue = draft.DefaultValue;
            _draftIsPrimaryKey = draft.IsPrimarykey;
            _draftIsIdentity = draft.IsIdentity;
            _draftIsNullable = draft.IsNullable;
        }

        private void RemoveDraft(int key)
        {
            var count = 0;
            for (var index = 0; index < _addTableColumns.Length; index++)
                if (_addTableColumns[index].Key != key)
                    count++;

            var result = new DatabaseColumnDraft[count];
            var targetIndex = 0;
            for (var index = 0; index < _addTableColumns.Length; index++)
            {
                if (_addTableColumns[index].Key == key)
                    continue;
                result[targetIndex++] = _addTableColumns[index];
            }
            _addTableColumns = result;
            _addTableColumnRows = _addTableColumns;
            if (_editingDraftKey == key)
                ResetDraftEditor();
        }

        private void MoveDraft(int key, bool moveUp)
        {
            var index = FindDraftIndex(key);
            var targetIndex = moveUp ? index - 1 : index + 1;
            if (index < 0 || targetIndex < 0 || targetIndex >= _addTableColumns.Length)
                return;

            var current = _addTableColumns[index];
            _addTableColumns[index] = _addTableColumns[targetIndex];
            _addTableColumns[targetIndex] = current;
            _addTableColumnRows = _addTableColumns;
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
                Key = ++_nextDraftKey,
                DbColumnName = name,
                ColumnDescription = description,
                DataType = dataType,
                Length = length,
                DecimalDigits = 0,
                DefaultValue = string.Empty,
                IsNullable = nullable,
                IsPrimarykey = primaryKey,
                IsIdentity = identity
            };

        private void SaveAddTable()
        {
            if (Busy)
                return;

            var tableName = _addTableName.Trim();
            var description = _addTableDescription.Trim();
            if (tableName.Length == 0 || description.Length == 0)
            {
                _error = "表名称和描述不能为空。";
                return;
            }
            if (_addTableColumns.Length == 0)
            {
                _error = "请至少添加一个字段。";
                return;
            }

            for (var index = 0; index < _addTableColumns.Length; index++)
            {
                var column = _addTableColumns[index];
                if (column.DbColumnName.Trim().Length == 0 ||
                    column.ColumnDescription.Trim().Length == 0 ||
                    column.DataType.Trim().Length == 0)
                {
                    _error = "字段名、描述和数据类型不能为空。";
                    return;
                }
                for (var otherIndex = index + 1; otherIndex < _addTableColumns.Length; otherIndex++)
                {
                    if (column.DbColumnName.Trim().ToLowerInvariant() ==
                        _addTableColumns[otherIndex].DbColumnName.Trim().ToLowerInvariant())
                    {
                        _error = "字段【" + column.DbColumnName.Trim() + "】重复。";
                        return;
                    }
                }
            }

            var columns = new DatabaseAddColumnInput[_addTableColumns.Length];
            for (var index = 0; index < _addTableColumns.Length; index++)
                columns[index] = ToAddColumnInput(_addTableColumns[index], tableName);

            _busyKey = "add-table";
            DatabaseClient.AddTable(new DatabaseAddTableInput
            {
                ConfigId = _configId.Trim(),
                TableName = tableName,
                Description = description,
                DbColumnInfoList = columns
            }).Then(outcome =>
            {
                _busyKey = string.Empty;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "新增数据表失败。";
                    return;
                }
                _addTableDialogVisible = false;
                _error = null;
                ElementPlus.ElMessageMembers.Success("数据表已新增");
                StartTableLoad(tableName);
            }, _ =>
            {
                _busyKey = string.Empty;
                _error = "新增数据表失败，请检查网络后重试。";
            });
        }

        private void OpenEditTable()
        {
            if (!EnsureTable("编辑数据表"))
                return;
            var table = SelectedTable;
            _editTableOldName = _selectedTableName;
            _editTableName = _selectedTableName;
            _editTableDescription = table?.Description ?? string.Empty;
            _editTableDialogVisible = true;
        }

        private void SaveEditTable()
        {
            if (Busy)
                return;
            var tableName = _editTableName.Trim();
            var description = _editTableDescription.Trim();
            if (tableName.Length == 0 || description.Length == 0)
            {
                _error = "表名称和描述不能为空。";
                return;
            }

            _busyKey = "edit-table";
            DatabaseClient.UpdateTable(new DatabaseUpdateTableInput
            {
                ConfigId = _configId.Trim(),
                TableName = tableName,
                OldTableName = _editTableOldName,
                Description = description
            }).Then(outcome =>
            {
                _busyKey = string.Empty;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "编辑数据表失败。";
                    return;
                }
                _editTableDialogVisible = false;
                _error = null;
                ElementPlus.ElMessageMembers.Success("数据表已更新");
                StartTableLoad(tableName);
            }, _ =>
            {
                _busyKey = string.Empty;
                _error = "编辑数据表失败，请检查网络后重试。";
            });
        }

        private void DeleteTable()
        {
            if (!EnsureTable("删除数据表") || !BeginAction("delete-table"))
                return;
            DatabaseClient.DeleteTable(new DatabaseDeleteTableInput
            {
                ConfigId = _configId.Trim(),
                TableName = _selectedTableName.Trim()
            }).Then(outcome =>
            {
                _busyKey = string.Empty;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "删除数据表失败。";
                    return;
                }
                _error = null;
                ElementPlus.ElMessageMembers.Success("数据表已删除");
                StartTableLoad(string.Empty);
            }, _ =>
            {
                _busyKey = string.Empty;
                _error = "删除数据表失败，请检查网络后重试。";
            });
        }

        private void OpenAddColumn()
        {
            if (!EnsureTable("新增数据列"))
                return;
            _addColumnName = string.Empty;
            _addColumnDescription = string.Empty;
            _addColumnDataType = string.Empty;
            _addColumnLength = 0;
            _addColumnDecimalDigits = 0;
            _addColumnDefaultValue = string.Empty;
            _addColumnIsPrimaryKey = 0;
            _addColumnIsIdentity = 0;
            _addColumnIsNullable = 0;
            _addColumnDialogVisible = true;
        }

        private void ChangeAddColumnDataType(string? value)
        {
            _addColumnDataType = value ?? string.Empty;
            if (IsStringType(_addColumnDataType))
            {
                if (_addColumnLength <= 0)
                    _addColumnLength = 32;
                _addColumnDecimalDigits = 0;
            }
            else if (_addColumnDataType != "decimal")
            {
                _addColumnLength = 0;
                _addColumnDecimalDigits = 0;
            }
        }

        private void SaveAddColumn()
        {
            if (Busy)
                return;
            var name = _addColumnName.Trim();
            var description = _addColumnDescription.Trim();
            var dataType = _addColumnDataType.Trim();
            if (name.Length == 0 || description.Length == 0 || dataType.Length == 0)
            {
                _error = "列名称、描述和数据类型不能为空。";
                return;
            }

            _busyKey = "add-column";
            DatabaseClient.AddColumn(new DatabaseAddColumnInput
            {
                ConfigId = _configId.Trim(),
                TableName = _selectedTableName.Trim(),
                DbColumnName = name,
                ColumnDescription = description,
                DataType = dataType,
                Length = _addColumnLength < 0 ? 0 : _addColumnLength,
                DecimalDigits = _addColumnDecimalDigits < 0 ? 0 : _addColumnDecimalDigits,
                DefaultValue = _addColumnDefaultValue.Trim(),
                IsPrimarykey = _addColumnIsPrimaryKey == 1 ? 1 : 0,
                IsIdentity = _addColumnIsIdentity == 1 ? 1 : 0,
                IsNullable = _addColumnIsNullable == 1 ? 1 : 0
            }).Then(outcome =>
            {
                _busyKey = string.Empty;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "新增数据列失败。";
                    return;
                }
                _addColumnDialogVisible = false;
                _error = null;
                ElementPlus.ElMessageMembers.Success("数据列已新增");
                LoadColumns();
            }, _ =>
            {
                _busyKey = string.Empty;
                _error = "新增数据列失败，请检查网络后重试。";
            });
        }

        private void OpenEditColumn(string columnName)
        {
            var row = FindColumn(columnName);
            if (row is null)
                return;
            _editColumnOldName = row.DbColumnName;
            _editColumnName = row.DbColumnName;
            _editColumnDescription = row.ColumnDescription;
            _editColumnDefaultValue = row.DefaultValue;
            _editColumnDialogVisible = true;
        }

        private void SaveEditColumn()
        {
            if (Busy)
                return;
            var columnName = _editColumnName.Trim();
            var description = _editColumnDescription.Trim();
            if (columnName.Length == 0 || description.Length == 0)
            {
                _error = "列名称和描述不能为空。";
                return;
            }

            _busyKey = "edit-column";
            DatabaseClient.UpdateColumn(new DatabaseUpdateColumnInput
            {
                ConfigId = _configId.Trim(),
                TableName = _selectedTableName.Trim(),
                ColumnName = columnName,
                OldColumnName = _editColumnOldName,
                Description = description,
                DefaultValue = _editColumnDefaultValue.Trim()
            }).Then(outcome =>
            {
                _busyKey = string.Empty;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "编辑数据列失败。";
                    return;
                }
                _editColumnDialogVisible = false;
                _error = null;
                ElementPlus.ElMessageMembers.Success("数据列已更新");
                LoadColumns();
            }, _ =>
            {
                _busyKey = string.Empty;
                _error = "编辑数据列失败，请检查网络后重试。";
            });
        }

        private void DeleteColumn(string columnName)
        {
            if (columnName.Length == 0 || !BeginAction("delete-column:" + columnName))
                return;
            DatabaseClient.DeleteColumn(new DatabaseDeleteColumnInput
            {
                ConfigId = _configId.Trim(),
                TableName = _selectedTableName.Trim(),
                DbColumnName = columnName
            }).Then(outcome =>
            {
                _busyKey = string.Empty;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "删除数据列失败。";
                    return;
                }
                _error = null;
                ElementPlus.ElMessageMembers.Success("数据列已删除");
                LoadColumns();
            }, _ =>
            {
                _busyKey = string.Empty;
                _error = "删除数据列失败，请检查网络后重试。";
            });
        }

        private void MoveColumn(string rowName, bool moveUp)
        {
            var index = FindColumnIndex(rowName);
            var targetIndex = moveUp ? index - 1 : index + 1;
            if (index < 0 || targetIndex < 0 || targetIndex >= _columnItems.Length)
            {
                ElementPlus.ElMessageMembers.Warning(moveUp ? "已处于首位" : "已处于末位");
                return;
            }

            var columnName = moveUp ? _columnItems[targetIndex].DbColumnName : _columnItems[index].DbColumnName;
            var afterColumnName = moveUp ? _columnItems[index].DbColumnName : _columnItems[targetIndex].DbColumnName;
            if (!BeginAction("move-column:" + rowName))
                return;

            DatabaseClient.MoveColumn(new DatabaseMoveColumnInput
            {
                ConfigId = _configId.Trim(),
                TableName = _selectedTableName.Trim(),
                ColumnName = columnName,
                AfterColumnName = afterColumnName
            }).Then(outcome =>
            {
                _busyKey = string.Empty;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "移动数据列失败。";
                    return;
                }
                _error = null;
                ElementPlus.ElMessageMembers.Success("数据列位置已更新");
                LoadColumns();
            }, _ =>
            {
                _busyKey = string.Empty;
                _error = "移动数据列失败，请检查网络后重试。";
            });
        }

        private void OpenVisual()
        {
            if (!EnsureDatabase("查看可视化"))
                return;
            _navigation.NavigateTo(
                _navigation.BaseUri + "database/visual?configId=" + EncodeURIComponent(_configId.Trim()));
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
            _busyKey = key;
            _error = null;
            return true;
        }

        private DatabaseTableView? FindTable(string name)
        {
            for (var index = 0; index < _tableItems.Length; index++)
                if (_tableItems[index].Name == name)
                    return _tableItems[index];
            return null;
        }

        private DatabaseColumnView? FindColumn(string name)
        {
            for (var index = 0; index < _columnItems.Length; index++)
                if (_columnItems[index].DbColumnName == name)
                    return _columnItems[index];
            return null;
        }

        private int FindColumnIndex(string name)
        {
            for (var index = 0; index < _columnItems.Length; index++)
                if (_columnItems[index].DbColumnName == name)
                    return index;
            return -1;
        }

        private DatabaseColumnDraft? FindDraft(int key)
        {
            for (var index = 0; index < _addTableColumns.Length; index++)
                if (_addTableColumns[index].Key == key)
                    return _addTableColumns[index];
            return null;
        }

        private int FindDraftIndex(int key)
        {
            for (var index = 0; index < _addTableColumns.Length; index++)
                if (_addTableColumns[index].Key == key)
                    return index;
            return -1;
        }

        private DatabaseAddColumnInput ToAddColumnInput(DatabaseColumnDraft draft, string tableName)
            => new()
            {
                ConfigId = _configId.Trim(),
                TableName = tableName,
                DbColumnName = draft.DbColumnName.Trim(),
                DataType = draft.DataType.Trim(),
                Length = draft.Length,
                ColumnDescription = draft.ColumnDescription.Trim(),
                IsNullable = draft.IsNullable,
                IsIdentity = draft.IsIdentity,
                IsPrimarykey = draft.IsPrimarykey,
                DecimalDigits = draft.DecimalDigits,
                DefaultValue = draft.DefaultValue.Trim()
            };

        private static bool IsStringType(string dataType)
        {
            var normalized = dataType.Trim().ToLowerInvariant();
            return normalized == "char" || normalized == "nchar" ||
                   normalized == "nvarchar" || normalized == "varchar";
        }

        private ConfigurationOption[] DatabaseOptions()
        {
            var result = new ConfigurationOption[_databaseItems.Length];
            for (var index = 0; index < result.Length; index++)
                result[index] = new ConfigurationOption(_databaseItems[index].ConfigId, DatabaseLabel(_databaseItems[index]));
            return result;
        }

        private ConfigurationOption[] TableOptions()
        {
            var result = new ConfigurationOption[_tableItems.Length];
            for (var index = 0; index < result.Length; index++)
                result[index] = new ConfigurationOption(_tableItems[index].Name, TableLabel(_tableItems[index]));
            return result;
        }

        private ConfigurationOption[] DataTypeOptions() => ToOptions(_databaseTypeOptions);


        private static ConfigurationOption[] ToOptions(string[] values)
        {
            var result = new ConfigurationOption[values.Length];
            for (var index = 0; index < result.Length; index++)
                result[index] = new ConfigurationOption(values[index], values[index]);
            return result;
        }

        private static string DatabaseLabel(DatabaseOptionView item)
            => item.DbNickName.Length == 0 ? item.ConfigId : item.DbNickName;

        private static string TableLabel(DatabaseTableView item)
            => item.Description.Length == 0 ? item.Name : item.Name + "（" + item.Description + "）";

        public DatabasePageProps CreateProps() => new()
        {
            ColumnRows = _columnRows,
            ConfigId = _configId,
            SelectedTableName = _selectedTableName,
            LoadingDatabases = _loadingDatabases,
            LoadingTables = _loadingTables,
            LoadingColumns = _loadingColumns,
            BusyKey = _busyKey,
            Error = _error,
            AddTableDialogVisible = _addTableDialogVisible,
            AddTableName = _addTableName,
            AddTableDescription = _addTableDescription,
            AddTableColumnRows = _addTableColumnRows,
            EditingDraftKey = _editingDraftKey,
            DraftColumnName = _draftColumnName,
            DraftColumnDescription = _draftColumnDescription,
            DraftDataType = _draftDataType,
            DraftLength = _draftLength,
            DraftDecimalDigits = _draftDecimalDigits,
            DraftDefaultValue = _draftDefaultValue,
            DraftIsPrimaryKey = _draftIsPrimaryKey,
            DraftIsIdentity = _draftIsIdentity,
            DraftIsNullable = _draftIsNullable,
            EditTableDialogVisible = _editTableDialogVisible,
            EditTableName = _editTableName,
            EditTableDescription = _editTableDescription,
            AddColumnDialogVisible = _addColumnDialogVisible,
            AddColumnName = _addColumnName,
            AddColumnDescription = _addColumnDescription,
            AddColumnDataType = _addColumnDataType,
            AddColumnLength = _addColumnLength,
            AddColumnDecimalDigits = _addColumnDecimalDigits,
            AddColumnDefaultValue = _addColumnDefaultValue,
            AddColumnIsPrimaryKey = _addColumnIsPrimaryKey,
            AddColumnIsIdentity = _addColumnIsIdentity,
            AddColumnIsNullable = _addColumnIsNullable,
            EditColumnDialogVisible = _editColumnDialogVisible,
            EditColumnName = _editColumnName,
            EditColumnDescription = _editColumnDescription,
            EditColumnDefaultValue = _editColumnDefaultValue,
            Busy = Busy,
            HasTable = HasTable,
            HasDatabase = HasDatabase,
            DatabaseOptions = DatabaseOptions(),
            TableOptions = TableOptions(),
            DataTypeOptions = DataTypeOptions(),
            Refresh = Refresh,
            DismissError = () => _error = null,
            ChangeDatabase = ChangeDatabase,
            ChangeTable = ChangeTable,
            OpenAddTable = OpenAddTable,
            OpenEditTable = OpenEditTable,
            DeleteTable = DeleteTable,
            OpenVisual = OpenVisual,
            OpenAddColumn = OpenAddColumn,
            MoveColumnUp = value => MoveColumn(value, true),
            MoveColumnDown = value => MoveColumn(value, false),
            OpenEditColumn = OpenEditColumn,
            DeleteColumn = DeleteColumn,
            SetAddTableDialogVisible = value => _addTableDialogVisible = value,
            SetAddTableName = value => _addTableName = value ?? "",
            SetAddTableDescription = value => _addTableDescription = value ?? "",
            AddPrimaryPreset = AddPrimaryPreset,
            AddTenantPreset = AddTenantPreset,
            AddOrgPreset = AddOrgPreset,
            AddBasePresets = AddBasePresets,
            AddSoftDeletePreset = AddSoftDeletePreset,
            SetDraftColumnName = value => _draftColumnName = value ?? "",
            SetDraftColumnDescription = value => _draftColumnDescription = value ?? "",
            ChangeDraftDataType = ChangeDraftDataType,
            SetDraftDefaultValue = value => _draftDefaultValue = value ?? "",
            SetDraftLength = value => _draftLength = (int)(value ?? 0),
            SetDraftDecimalDigits = value => _draftDecimalDigits = (int)(value ?? 0),
            SetDraftIsPrimaryKey = value => _draftIsPrimaryKey = (int)value,
            SetDraftIsIdentity = value => _draftIsIdentity = (int)value,
            SetDraftIsNullable = value => _draftIsNullable = (int)value,
            ResetDraftEditor = ResetDraftEditor,
            SaveDraftColumn = SaveDraftColumn,
            MoveDraftUp = value => MoveDraft((int)value, true),
            MoveDraftDown = value => MoveDraft((int)value, false),
            EditDraft = value => EditDraft((int)value),
            RemoveDraft = value => RemoveDraft((int)value),
            SaveAddTable = SaveAddTable,
            SetEditTableDialogVisible = value => _editTableDialogVisible = value,
            SetEditTableName = value => _editTableName = value ?? "",
            SetEditTableDescription = value => _editTableDescription = value ?? "",
            SaveEditTable = SaveEditTable,
            SetAddColumnDialogVisible = value => _addColumnDialogVisible = value,
            SetAddColumnName = value => _addColumnName = value ?? "",
            SetAddColumnDescription = value => _addColumnDescription = value ?? "",
            ChangeAddColumnDataType = ChangeAddColumnDataType,
            SetAddColumnDefaultValue = value => _addColumnDefaultValue = value ?? "",
            SetAddColumnLength = value => _addColumnLength = (int)(value ?? 0),
            SetAddColumnDecimalDigits = value => _addColumnDecimalDigits = (int)(value ?? 0),
            SetAddColumnIsPrimaryKey = value => _addColumnIsPrimaryKey = (int)value,
            SetAddColumnIsIdentity = value => _addColumnIsIdentity = (int)value,
            SetAddColumnIsNullable = value => _addColumnIsNullable = (int)value,
            SaveAddColumn = SaveAddColumn,
            SetEditColumnDialogVisible = value => _editColumnDialogVisible = value,
            SetEditColumnName = value => _editColumnName = value ?? "",
            SetEditColumnDescription = value => _editColumnDescription = value ?? "",
            SetEditColumnDefaultValue = value => _editColumnDefaultValue = value ?? "",
            SaveEditColumn = SaveEditColumn,
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
    public required bool Busy { get; init; }
    public required bool HasTable { get; init; }
    public required bool HasDatabase { get; init; }
    public required ConfigurationOption[] DatabaseOptions { get; init; }
    public required ConfigurationOption[] TableOptions { get; init; }
    public required ConfigurationOption[] DataTypeOptions { get; init; }
    public required Action Refresh { get; init; }
    public required Action DismissError { get; init; }
    public required Action<string> ChangeDatabase { get; init; }
    public required Action<string> ChangeTable { get; init; }
    public required Action OpenAddTable { get; init; }
    public required Action OpenEditTable { get; init; }
    public required Action DeleteTable { get; init; }
    public required Action OpenVisual { get; init; }
    public required Action OpenAddColumn { get; init; }
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
}
