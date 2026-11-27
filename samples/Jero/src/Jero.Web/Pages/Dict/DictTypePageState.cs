using Jero.Web.Application;
using Jero.Web.Infrastructure.Api.Dict;
using Jero.Web.Pages.Components;
using Jero.Web.Pages.Shell;
using static ECMAScript.Global;

namespace Jero.Web.Pages.Dict;

[ECMAScriptModule("components/dict-type-page-state.js")]
public static class DictTypePageModule
{
    public static DictTypePageState Create() => new();

    public sealed class DictTypePageState : global::Jero.Web.Composition.IPageState<DictTypePageProps>
    {
        private readonly double[] _joltPageSizes = [10, 20, 50, 100];

        private DictTypeView[] _typeRows = [];
        private DictTypeView[] _typeItems = [];
        private int _typePage = 1;
        private int _typePageSize = 50;
        private int _typeTotal;
        private string _typeName = string.Empty;
        private string _typeCode = string.Empty;
        private string _typeField = "orderNo";
        private string _typeOrder = "ascending";
        private bool _typeLoading;

        private DictDataView[] _dataRows = [];
        private DictDataView[] _dataItems = [];
        private int _dataPage = 1;
        private int _dataPageSize = 50;
        private int _dataTotal;
        private string _dataLabel = string.Empty;
        private string _dataCode = string.Empty;
        private string _dataField = "orderNo";
        private string _dataOrder = "ascending";
        private bool _dataLoading;

        private string _selectedTypeId = string.Empty;
        private string? _error;

        private bool _typeDialogVisible;
        private string _typeDialogTitle = "新增字典";
        private bool _typeSaving;
        private string _editTypeId = string.Empty;
        private string _editTypeName = string.Empty;
        private string _editTypeCode = string.Empty;
        private int _editTypeOrderNo = 100;
        private string _editTypeRemark = string.Empty;
        private int _editTypeStatus = 1;
        private int _editTypeSysFlag = 2;
        private int _editTypeIsTenant = 2;

        private bool _dataDialogVisible;
        private string _dataDialogTitle = "新增字典值";
        private bool _dataSaving;
        private string _editDataId = string.Empty;
        private string _editDataLabel = string.Empty;
        private string _editDataValue = string.Empty;
        private string _editDataCode = string.Empty;
        private string _editDataName = string.Empty;
        private string _editDataTagType = "primary";
        private string _editDataStyle = string.Empty;
        private string _editDataClass = string.Empty;
        private int _editDataOrderNo = 100;
        private string _editDataRemark = string.Empty;
        private string _editDataExtData = string.Empty;
        private int _editDataStatus = 1;
        private bool _editDataSystemLocked;

        private bool IsSuperAdmin => ApplicationState.User?.AccountType == 999;
        private bool CanAddType => ApplicationState.HasPermission("sysDictType:add");
        private bool CanEditType => ApplicationState.HasPermission("sysDictType:update");
        private bool CanDeleteType => ApplicationState.HasPermission("sysDictType:delete");
        private bool CanAddData => ApplicationState.HasPermission("sysDictData:add");
        private bool CanEditData => ApplicationState.HasPermission("sysDictData:update");
        private bool CanDeleteData => ApplicationState.HasPermission("sysDictData:delete");
        private DictTypeView? SelectedType => FindType(_selectedTypeId);
        private bool CanOperateSelectedType => HasTypePermission(SelectedType);

        public void Initialize() => LoadTypePage(true);

        private void SearchTypes()
        {
            _typePage = 1;
            LoadTypePage(true);
        }

        private void ResetTypeSearch()
        {
            _typeName = string.Empty;
            _typeCode = string.Empty;
            _typePage = 1;
            LoadTypePage(true);
        }

        private void SearchData()
        {
            _dataPage = 1;
            LoadDataPage();
        }

        private void ResetDataSearch()
        {
            _dataLabel = string.Empty;
            _dataCode = string.Empty;
            _dataPage = 1;
            LoadDataPage();
        }

        private void ChangeTypePage(Number value)
        {
            _typePage = (int)value;
            LoadTypePage(true);
        }

        private void ChangeTypePageSize(Number value)
        {
            _typePageSize = (int)value;
            _typePage = 1;
            LoadTypePage(true);
        }

        private void ChangeDataPage(Number value)
        {
            _dataPage = (int)value;
            LoadDataPage();
        }

        private void ChangeDataPageSize(Number value)
        {
            _dataPageSize = (int)value;
            _dataPage = 1;
            LoadDataPage();
        }

        private void SortTypes(string prop, string order)
        {
            _typeField = string.IsNullOrWhiteSpace(prop) ? "orderNo" : prop;
            _typeOrder = order == "descending" ? "descending" : "ascending";
            LoadTypePage(false);
        }

        private void SortData(string prop, string order)
        {
            _dataField = string.IsNullOrWhiteSpace(prop) ? "orderNo" : prop;
            _dataOrder = order == "descending" ? "descending" : "ascending";
            LoadDataPage();
        }

        private void LoadTypePage(bool forceDataReload)
        {
            _typeLoading = true;
            DictTypeClient.Page(new PageDictTypeInput
            {
                Page = _typePage,
                PageSize = _typePageSize,
                Field = _typeField,
                Order = _typeOrder,
                Name = _typeName.Trim(),
                Code = _typeCode.Trim()
            }).Then(outcome =>
            {
                _typeLoading = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载字典类型失败。";
                    _typeItems = [];
                    _typeRows = [];
                    _typeTotal = 0;
                    ClearSelection();
                    return;
                }

                _typeItems = DictTypeClient.ReadTypeList(outcome.Data);
                _typeRows = _typeItems;
                _typeTotal = DictTypeClient.ReadTotal(outcome.Data);
                _error = null;
                SyncSelectedType(forceDataReload);
            }, _ =>
            {
                _typeLoading = false;
                _error = "加载字典类型失败，请检查网络后重试。";
            });
        }

        private void LoadDataPage()
        {
            var selected = SelectedType;
            if (selected is null)
            {
                _dataItems = [];
                _dataRows = [];
                _dataTotal = 0;
                return;
            }

            _dataLoading = true;
            DictTypeClient.PageData(new PageDictDataInput
            {
                Page = _dataPage,
                PageSize = _dataPageSize,
                Field = _dataField,
                Order = _dataOrder,
                DictTypeId = selected.Id,
                Label = _dataLabel.Trim(),
                Code = _dataCode.Trim()
            }).Then(outcome =>
            {
                _dataLoading = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载字典值失败。";
                    _dataItems = [];
                    _dataRows = [];
                    _dataTotal = 0;
                    return;
                }

                _dataItems = DictTypeClient.ReadDataList(outcome.Data);
                _dataRows = _dataItems;
                _dataTotal = DictTypeClient.ReadTotal(outcome.Data);
                _error = null;
            }, _ =>
            {
                _dataLoading = false;
                _error = "加载字典值失败，请检查网络后重试。";
            });
        }

        private void SyncSelectedType(bool forceDataReload)
        {
            var previousId = _selectedTypeId;
            var selected = FindType(previousId);
            if (selected is null && _typeItems.Length > 0)
                selected = _typeItems[0];

            if (selected is null)
            {
                ClearSelection();
                return;
            }

            _selectedTypeId = selected.Id;
            if (forceDataReload || selected.Id != previousId)
            {
                _dataPage = 1;
                _dataLabel = string.Empty;
                _dataCode = string.Empty;
                LoadDataPage();
            }
        }

        private void SelectTypeRow(VueProps? row)
        {
            var id = RowString(row, "id");
            if (id.Length == 0 || id == _selectedTypeId)
                return;

            _selectedTypeId = id;
            _dataLabel = string.Empty;
            _dataCode = string.Empty;
            _dataPage = 1;
            LoadDataPage();
        }

        private void ClearSelection()
        {
            _selectedTypeId = string.Empty;
            _dataItems = [];
            _dataRows = [];
            _dataTotal = 0;
        }

        private void OpenCreateType()
        {
            _typeDialogTitle = "新增字典";
            _editTypeId = string.Empty;
            _editTypeName = string.Empty;
            _editTypeCode = string.Empty;
            _editTypeOrderNo = 100;
            _editTypeRemark = string.Empty;
            _editTypeStatus = 1;
            _editTypeSysFlag = 2;
            _editTypeIsTenant = 2;
            _typeDialogVisible = true;
        }

        private void OpenEditType(VueProps? value)
        {
            var row = FindType(RowString(value, "id"));
            if (row is null || !HasTypePermission(row))
                return;

            _typeDialogTitle = "编辑字典";
            _editTypeId = row.Id;
            _editTypeName = row.Name;
            _editTypeCode = row.Code;
            _editTypeOrderNo = row.OrderNo;
            _editTypeRemark = row.Remark ?? string.Empty;
            _editTypeStatus = row.Status;
            _editTypeSysFlag = row.SysFlag;
            _editTypeIsTenant = row.IsTenant;
            _typeDialogVisible = true;
        }

        private void SaveType()
        {
            if (_typeSaving)
                return;

            var name = _editTypeName.Trim();
            var code = _editTypeCode.Trim();
            if (name.Length == 0 || code.Length == 0)
            {
                _error = "字典名称和编码不能为空。";
                return;
            }

            _typeSaving = true;
            var input = new DictTypeSaveInput
            {
                Id = _editTypeId,
                Name = name,
                Code = code,
                OrderNo = _editTypeOrderNo,
                Remark = _editTypeRemark.Trim(),
                Status = _editTypeStatus,
                SysFlag = IsSuperAdmin ? _editTypeSysFlag : 2,
                IsTenant = IsSuperAdmin ? _editTypeIsTenant : 2
            };

            var request = _editTypeId.Length == 0 ? DictTypeClient.Add(input) : DictTypeClient.Update(input);
            request.Then(outcome =>
            {
                _typeSaving = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "保存字典失败。";
                    return;
                }

                _typeDialogVisible = false;
                _error = null;
                ElementPlus.ElMessageMembers.Success(_editTypeId.Length == 0 ? "字典已新增" : "字典已更新");
                LoadTypePage(true);
            }, _ =>
            {
                _typeSaving = false;
                _error = "保存字典失败，请检查网络后重试。";
            });
        }

        private void DeleteType(VueProps? value)
        {
            var row = FindType(RowString(value, "id"));
            if (row is null || row.SysFlag == 1 || !HasTypePermission(row))
                return;

            DictTypeClient.Delete(row.Id).Then(outcome =>
            {
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "删除字典失败。";
                    return;
                }

                if (_selectedTypeId == row.Id)
                    _selectedTypeId = string.Empty;
                ElementPlus.ElMessageMembers.Success("字典已删除");
                LoadTypePage(true);
            }, _ => _error = "删除字典失败，请检查网络后重试。");
        }

        private void OpenCreateData()
        {
            var selected = SelectedType;
            if (selected is null || !HasTypePermission(selected))
            {
                ElementPlus.ElMessageMembers.Warning("请先选择可维护的字典");
                return;
            }

            _dataDialogTitle = "新增字典值";
            _editDataId = string.Empty;
            _editDataLabel = string.Empty;
            _editDataValue = string.Empty;
            _editDataCode = string.Empty;
            _editDataName = string.Empty;
            _editDataTagType = "primary";
            _editDataStyle = string.Empty;
            _editDataClass = string.Empty;
            _editDataOrderNo = 100;
            _editDataRemark = string.Empty;
            _editDataExtData = string.Empty;
            _editDataStatus = 1;
            _editDataSystemLocked = false;
            _dataDialogVisible = true;
        }

        private void OpenEditData(VueProps? value)
        {
            var row = FindData(RowString(value, "id"));
            var selected = SelectedType;
            if (row is null || selected is null || !HasTypePermission(selected))
                return;

            PopulateDataForm(row, false);
        }

        private void OpenCopyData(VueProps? value)
        {
            var row = FindData(RowString(value, "id"));
            var selected = SelectedType;
            if (row is null || selected is null || !HasTypePermission(selected))
                return;

            PopulateDataForm(row, true);
        }

        private void PopulateDataForm(DictDataView row, bool copy)
        {
            _dataDialogTitle = copy ? "复制字典值" : "编辑字典值";
            _editDataId = copy ? string.Empty : row.Id;
            _editDataLabel = row.Label;
            _editDataValue = row.Value;
            _editDataCode = row.Code ?? string.Empty;
            _editDataName = row.Name ?? string.Empty;
            _editDataTagType = NormalizeTagValue(row.TagType);
            _editDataStyle = row.StyleSetting ?? string.Empty;
            _editDataClass = row.ClassSetting ?? string.Empty;
            _editDataOrderNo = row.OrderNo;
            _editDataRemark = row.Remark ?? string.Empty;
            _editDataExtData = row.ExtData ?? string.Empty;
            _editDataStatus = row.Status;
            _editDataSystemLocked = !copy && SelectedType?.SysFlag == 1;
            _dataDialogVisible = true;
        }

        private void SaveData()
        {
            if (_dataSaving)
                return;

            var selected = SelectedType;
            if (selected is null || !HasTypePermission(selected))
            {
                _error = "当前字典不可维护。";
                return;
            }

            var label = _editDataLabel.Trim();
            var value = _editDataValue.Trim();
            if (label.Length == 0 || value.Length == 0)
            {
                _error = "显示文本和字典值不能为空。";
                return;
            }

            _dataSaving = true;
            var input = new DictDataSaveInput
            {
                Id = _editDataId,
                DictTypeId = selected.Id,
                Label = label,
                Value = value,
                Code = _editDataCode.Trim(),
                Name = _editDataName.Trim(),
                TagType = NormalizeTagValue(_editDataTagType),
                StyleSetting = _editDataStyle.Trim(),
                ClassSetting = _editDataClass.Trim(),
                OrderNo = _editDataOrderNo,
                Remark = _editDataRemark.Trim(),
                ExtData = _editDataExtData.Trim(),
                Status = _editDataStatus
            };

            var request = _editDataId.Length == 0 ? DictTypeClient.AddData(input) : DictTypeClient.UpdateData(input);
            request.Then(outcome =>
            {
                _dataSaving = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "保存字典值失败。";
                    return;
                }

                _dataDialogVisible = false;
                _error = null;
                ElementPlus.ElMessageMembers.Success(_editDataId.Length == 0 ? "字典值已新增" : "字典值已更新");
                LoadDataPage();
            }, _ =>
            {
                _dataSaving = false;
                _error = "保存字典值失败，请检查网络后重试。";
            });
        }

        private void DeleteData(VueProps? value)
        {
            var row = FindData(RowString(value, "id"));
            var selected = SelectedType;
            if (row is null || selected is null || !HasTypePermission(selected))
                return;

            DictTypeClient.DeleteData(row.Id).Then(outcome =>
            {
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "删除字典值失败。";
                    return;
                }

                ElementPlus.ElMessageMembers.Success("字典值已删除");
                LoadDataPage();
            }, _ => _error = "删除字典值失败，请检查网络后重试。");
        }

        private bool HasTypePermission(DictTypeView? row)
            => row is not null && (row.SysFlag == 2 || IsSuperAdmin);

        private bool CanEditTypeRow(VueProps? row)
            => CanEditType && HasTypePermission(FindType(RowString(row, "id")));

        private bool CanDeleteTypeRow(VueProps? row)
        {
            var item = FindType(RowString(row, "id"));
            return CanDeleteType && item is not null && item.SysFlag == 2 && HasTypePermission(item);
        }

        private DictTypeView? FindType(string id)
        {
            for (var index = 0; index < _typeItems.Length; index++)
                if (_typeItems[index].Id == id)
                    return _typeItems[index];
            return null;
        }

        private DictDataView? FindData(string id)
        {
            for (var index = 0; index < _dataItems.Length; index++)
                if (_dataItems[index].Id == id)
                    return _dataItems[index];
            return null;
        }

        private static string RowString(VueProps? row, string key)
        {
            if (row is null)
                return string.Empty;

            var value = Reflect.Get(row, key);
            return value is null ? string.Empty : StringValue(value);
        }

        private static int RowInt(VueProps? row, string key, int fallback)
        {
            if (row is null)
                return fallback;

            var value = Reflect.Get(row, key);
            return value is null ? fallback : (int)NumberValue(value);
        }

        private static string StatusText(VueProps? row)
            => RowInt(row, "status", 1) == 1 ? "启用" : "停用";

        private static string YesNoText(VueProps? row, string key)
            => RowInt(row, key, 2) == 1 ? "是" : "否";

        private static string StatusTagType(VueProps? row)
            => RowInt(row, "status", 1) == 1 ? "success" : "info";

        private static string YesNoTagType(VueProps? row, string key)
            => RowInt(row, key, 2) == 1 ? "success" : "info";

        private static string DataTagType(VueProps? row)
        {
            var value = NormalizeTagValue(RowString(row, "tagType"));
            if (value == "success") return "success";
            if (value == "info") return "info";
            if (value == "warning") return "warning";
            if (value == "danger") return "danger";
            return "primary";
        }

        private static string NormalizeTagValue(string? value)
        {
            if (value == "success" || value == "info" || value == "warning" || value == "danger")
                return value;
            return "primary";
        }

        public DictTypePageProps CreateProps() => new()
        {
            SetTypeName = v => _typeName = v ?? "",
            SetTypeCode = v => _typeCode = v ?? "",
            SetDataLabel = v => _dataLabel = v ?? "",
            SetDataCode = v => _dataCode = v ?? "",
            SetTypeDialogVisible = v => _typeDialogVisible = v == true,
            SetEditTypeName = v => _editTypeName = v ?? "",
            SetEditTypeCode = v => _editTypeCode = v ?? "",
            SetEditTypeSysFlag = v => _editTypeSysFlag = (int)v,
            SetEditTypeIsTenant = v => _editTypeIsTenant = (int)v,
            SetEditTypeStatus = v => _editTypeStatus = (int)v,
            SetEditTypeOrderNo = v => _editTypeOrderNo = (int)(v ?? 100),
            SetEditTypeRemark = v => _editTypeRemark = v ?? "",
            SetDataDialogVisible = v => _dataDialogVisible = v == true,
            SetEditDataLabel = v => _editDataLabel = v ?? "",
            SetEditDataValue = v => _editDataValue = v ?? "",
            SetEditDataCode = v => _editDataCode = v ?? "",
            SetEditDataName = v => _editDataName = v ?? "",
            SetEditDataTagType = v => _editDataTagType = v ?? "primary",
            SetEditDataStyle = v => _editDataStyle = v ?? "",
            SetEditDataClass = v => _editDataClass = v ?? "",
            SetEditDataStatus = v => _editDataStatus = (int)v,
            SetEditDataOrderNo = v => _editDataOrderNo = (int)(v ?? 100),
            SetEditDataRemark = v => _editDataRemark = v ?? "",
            SetEditDataExtData = v => _editDataExtData = v ?? "",
            IsSuperAdmin = IsSuperAdmin,
            CanEditType = CanEditType,
            CanDeleteType = CanDeleteType,
            SelectTypeRow = id => SelectTypeRow(_typeRows.FirstOrDefault(row => row.Id == id)),
            OpenEditType = id => OpenEditType(_typeRows.FirstOrDefault(row => row.Id == id)),
            DeleteType = id => DeleteType(_typeRows.FirstOrDefault(row => row.Id == id)),
            SortTypes = (prop, order) => SortTypes(prop, order),
            CanAddData = CanAddData,
            CanEditData = CanEditData,
            CanDeleteData = CanDeleteData,
            CanOperateSelectedType = CanOperateSelectedType,
            OpenEditData = id => OpenEditData(_dataRows.FirstOrDefault(row => row.Id == id)),
            OpenCopyData = id => OpenCopyData(_dataRows.FirstOrDefault(row => row.Id == id)),
            DeleteData = id => DeleteData(_dataRows.FirstOrDefault(row => row.Id == id)),
            SortData = (prop, order) => SortData(prop, order),
            DismissError = () => _error = null,
            SearchTypes = SearchTypes,
            ResetTypeSearch = ResetTypeSearch,
            OpenCreateType = OpenCreateType,
            ChangeTypePage = value => ChangeTypePage(value),
            ChangeTypePageSize = value => ChangeTypePageSize(value),
            SearchData = SearchData,
            ResetDataSearch = ResetDataSearch,
            OpenCreateData = OpenCreateData,
            ChangeDataPage = value => ChangeDataPage(value),
            ChangeDataPageSize = value => ChangeDataPageSize(value),
            CancelType = () => _typeDialogVisible = false,
            SaveType = SaveType,
            CancelData = () => _dataDialogVisible = false,
            SaveData = SaveData,
            PageSizes = _joltPageSizes,
            TypeRows = _typeRows,
            TypePage = _typePage,
            TypePageSize = _typePageSize,
            TypeTotal = _typeTotal,
            TypeName = _typeName,
            TypeCode = _typeCode,
            TypeLoading = _typeLoading,
            DataRows = _dataRows,
            DataPage = _dataPage,
            DataPageSize = _dataPageSize,
            DataTotal = _dataTotal,
            DataLabel = _dataLabel,
            DataCode = _dataCode,
            DataLoading = _dataLoading,
            Error = _error,
            TypeDialogVisible = _typeDialogVisible,
            TypeDialogTitle = _typeDialogTitle,
            TypeSaving = _typeSaving,
            EditTypeId = _editTypeId,
            EditTypeName = _editTypeName,
            EditTypeCode = _editTypeCode,
            EditTypeOrderNo = _editTypeOrderNo,
            EditTypeRemark = _editTypeRemark,
            EditTypeStatus = _editTypeStatus,
            EditTypeSysFlag = _editTypeSysFlag,
            EditTypeIsTenant = _editTypeIsTenant,
            DataDialogVisible = _dataDialogVisible,
            DataDialogTitle = _dataDialogTitle,
            DataSaving = _dataSaving,
            EditDataLabel = _editDataLabel,
            EditDataValue = _editDataValue,
            EditDataCode = _editDataCode,
            EditDataName = _editDataName,
            EditDataTagType = _editDataTagType,
            EditDataStyle = _editDataStyle,
            EditDataClass = _editDataClass,
            EditDataOrderNo = _editDataOrderNo,
            EditDataRemark = _editDataRemark,
            EditDataExtData = _editDataExtData,
            EditDataStatus = _editDataStatus,
            EditDataSystemLocked = _editDataSystemLocked,
            CanAddType = CanAddType,
            SelectedType = SelectedType,
        };
    }
}

public sealed record DictTypePageProps : VueProps
{
    public required Action<string?> SetTypeName { get; init; }
    public required Action<string?> SetTypeCode { get; init; }
    public required Action<string?> SetDataLabel { get; init; }
    public required Action<string?> SetDataCode { get; init; }
    public required Action<bool> SetTypeDialogVisible { get; init; }
    public required Action<string?> SetEditTypeName { get; init; }
    public required Action<string?> SetEditTypeCode { get; init; }
    public required Action<double> SetEditTypeSysFlag { get; init; }
    public required Action<double> SetEditTypeIsTenant { get; init; }
    public required Action<double> SetEditTypeStatus { get; init; }
    public required Action<double?> SetEditTypeOrderNo { get; init; }
    public required Action<string?> SetEditTypeRemark { get; init; }
    public required Action<bool> SetDataDialogVisible { get; init; }
    public required Action<string?> SetEditDataLabel { get; init; }
    public required Action<string?> SetEditDataValue { get; init; }
    public required Action<string?> SetEditDataCode { get; init; }
    public required Action<string?> SetEditDataName { get; init; }
    public required Action<string> SetEditDataTagType { get; init; }
    public required Action<string?> SetEditDataStyle { get; init; }
    public required Action<string?> SetEditDataClass { get; init; }
    public required Action<double> SetEditDataStatus { get; init; }
    public required Action<double?> SetEditDataOrderNo { get; init; }
    public required Action<string?> SetEditDataRemark { get; init; }
    public required Action<string?> SetEditDataExtData { get; init; }
    public required bool IsSuperAdmin { get; init; }
    public required bool CanEditType { get; init; }
    public required bool CanDeleteType { get; init; }
    public required Action<string> SelectTypeRow { get; init; }
    public required Action<string> OpenEditType { get; init; }
    public required Action<string> DeleteType { get; init; }
    public required Action<string, string> SortTypes { get; init; }
    public required bool CanAddData { get; init; }
    public required bool CanEditData { get; init; }
    public required bool CanDeleteData { get; init; }
    public required bool CanOperateSelectedType { get; init; }
    public required Action<string> OpenEditData { get; init; }
    public required Action<string> OpenCopyData { get; init; }
    public required Action<string> DeleteData { get; init; }
    public required Action<string, string> SortData { get; init; }
    public required Action DismissError { get; init; }
    public required Action SearchTypes { get; init; }
    public required Action ResetTypeSearch { get; init; }
    public required Action OpenCreateType { get; init; }
    public required Action<double> ChangeTypePage { get; init; }
    public required Action<double> ChangeTypePageSize { get; init; }
    public required Action SearchData { get; init; }
    public required Action ResetDataSearch { get; init; }
    public required Action OpenCreateData { get; init; }
    public required Action<double> ChangeDataPage { get; init; }
    public required Action<double> ChangeDataPageSize { get; init; }
    public required Action CancelType { get; init; }
    public required Action SaveType { get; init; }
    public required Action CancelData { get; init; }
    public required Action SaveData { get; init; }
    public required double[] PageSizes { get; init; }
    public required DictTypeView[] TypeRows { get; init; }
    public required int TypePage { get; init; }
    public required int TypePageSize { get; init; }
    public required int TypeTotal { get; init; }
    public required string TypeName { get; init; }
    public required string TypeCode { get; init; }
    public required bool TypeLoading { get; init; }
    public required DictDataView[] DataRows { get; init; }
    public required int DataPage { get; init; }
    public required int DataPageSize { get; init; }
    public required int DataTotal { get; init; }
    public required string DataLabel { get; init; }
    public required string DataCode { get; init; }
    public required bool DataLoading { get; init; }
    public required string? Error { get; init; }
    public required bool TypeDialogVisible { get; init; }
    public required string TypeDialogTitle { get; init; }
    public required bool TypeSaving { get; init; }
    public required string EditTypeId { get; init; }
    public required string EditTypeName { get; init; }
    public required string EditTypeCode { get; init; }
    public required int EditTypeOrderNo { get; init; }
    public required string EditTypeRemark { get; init; }
    public required int EditTypeStatus { get; init; }
    public required int EditTypeSysFlag { get; init; }
    public required int EditTypeIsTenant { get; init; }
    public required bool DataDialogVisible { get; init; }
    public required string DataDialogTitle { get; init; }
    public required bool DataSaving { get; init; }
    public required string EditDataLabel { get; init; }
    public required string EditDataValue { get; init; }
    public required string EditDataCode { get; init; }
    public required string EditDataName { get; init; }
    public required string EditDataTagType { get; init; }
    public required string EditDataStyle { get; init; }
    public required string EditDataClass { get; init; }
    public required int EditDataOrderNo { get; init; }
    public required string EditDataRemark { get; init; }
    public required string EditDataExtData { get; init; }
    public required int EditDataStatus { get; init; }
    public required bool EditDataSystemLocked { get; init; }
    public required bool CanAddType { get; init; }
    public required DictTypeView? SelectedType { get; init; }
}
