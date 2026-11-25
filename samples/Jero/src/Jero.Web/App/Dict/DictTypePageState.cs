using Jero.Web.App.Components;
using Jero.Web.App.Shell;
using static ECMAScript.Global;

namespace Jero.Web.App.Dict;

[ECMAScriptModule("components/dict-type-page-state.js")]
public static class DictTypePageModule
{
    public static DictTypePageState Create() => new();

    public sealed class DictTypePageState : global::Jero.Web.App.IPageState<DictTypePageProps>
    {
        private readonly double[] JoltPageSizes = [10, 20, 50, 100];

        private DictTypeView[] typeRows = [];
        private DictTypeView[] typeItems = [];
        private int typePage = 1;
        private int typePageSize = 50;
        private int typeTotal;
        private string typeName = string.Empty;
        private string typeCode = string.Empty;
        private string typeField = "orderNo";
        private string typeOrder = "ascending";
        private bool typeLoading;

        private DictDataView[] dataRows = [];
        private DictDataView[] dataItems = [];
        private int dataPage = 1;
        private int dataPageSize = 50;
        private int dataTotal;
        private string dataLabel = string.Empty;
        private string dataCode = string.Empty;
        private string dataField = "orderNo";
        private string dataOrder = "ascending";
        private bool dataLoading;

        private string selectedTypeId = string.Empty;
        private string? error;

        private bool typeDialogVisible;
        private string typeDialogTitle = "新增字典";
        private bool typeSaving;
        private string editTypeId = string.Empty;
        private string editTypeName = string.Empty;
        private string editTypeCode = string.Empty;
        private int editTypeOrderNo = 100;
        private string editTypeRemark = string.Empty;
        private int editTypeStatus = 1;
        private int editTypeSysFlag = 2;
        private int editTypeIsTenant = 2;

        private bool dataDialogVisible;
        private string dataDialogTitle = "新增字典值";
        private bool dataSaving;
        private string editDataId = string.Empty;
        private string editDataLabel = string.Empty;
        private string editDataValue = string.Empty;
        private string editDataCode = string.Empty;
        private string editDataName = string.Empty;
        private string editDataTagType = "primary";
        private string editDataStyle = string.Empty;
        private string editDataClass = string.Empty;
        private int editDataOrderNo = 100;
        private string editDataRemark = string.Empty;
        private string editDataExtData = string.Empty;
        private int editDataStatus = 1;
        private bool editDataSystemLocked;

        private bool IsSuperAdmin => ApplicationState.User?.AccountType == 999;
        private bool CanAddType => ApplicationState.HasPermission("sysDictType:add");
        private bool CanEditType => ApplicationState.HasPermission("sysDictType:update");
        private bool CanDeleteType => ApplicationState.HasPermission("sysDictType:delete");
        private bool CanAddData => ApplicationState.HasPermission("sysDictData:add");
        private bool CanEditData => ApplicationState.HasPermission("sysDictData:update");
        private bool CanDeleteData => ApplicationState.HasPermission("sysDictData:delete");
        private DictTypeView? SelectedType => FindType(selectedTypeId);
        private bool CanOperateSelectedType => HasTypePermission(SelectedType);

        public void Initialize() => LoadTypePage(true);

        private void SearchTypes()
        {
            typePage = 1;
            LoadTypePage(true);
        }

        private void ResetTypeSearch()
        {
            typeName = string.Empty;
            typeCode = string.Empty;
            typePage = 1;
            LoadTypePage(true);
        }

        private void SearchData()
        {
            dataPage = 1;
            LoadDataPage();
        }

        private void ResetDataSearch()
        {
            dataLabel = string.Empty;
            dataCode = string.Empty;
            dataPage = 1;
            LoadDataPage();
        }

        private void ChangeTypePage(Number value)
        {
            typePage = (int)value;
            LoadTypePage(true);
        }

        private void ChangeTypePageSize(Number value)
        {
            typePageSize = (int)value;
            typePage = 1;
            LoadTypePage(true);
        }

        private void ChangeDataPage(Number value)
        {
            dataPage = (int)value;
            LoadDataPage();
        }

        private void ChangeDataPageSize(Number value)
        {
            dataPageSize = (int)value;
            dataPage = 1;
            LoadDataPage();
        }

        private void SortTypes(string prop, string order)
        {
            typeField = string.IsNullOrWhiteSpace(prop) ? "orderNo" : prop;
            typeOrder = order == "descending" ? "descending" : "ascending";
            LoadTypePage(false);
        }

        private void SortData(string prop, string order)
        {
            dataField = string.IsNullOrWhiteSpace(prop) ? "orderNo" : prop;
            dataOrder = order == "descending" ? "descending" : "ascending";
            LoadDataPage();
        }

        private void LoadTypePage(bool forceDataReload)
        {
            typeLoading = true;
            DictTypeClient.Page(new PageDictTypeInput
            {
                page = typePage,
                pageSize = typePageSize,
                field = typeField,
                order = typeOrder,
                name = typeName.Trim(),
                code = typeCode.Trim()
            }).Then(outcome =>
            {
                typeLoading = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载字典类型失败。";
                    typeItems = [];
                    typeRows = [];
                    typeTotal = 0;
                    ClearSelection();
                    return;
                }

                typeItems = DictTypeClient.ReadTypeList(outcome.Data);
                typeRows = typeItems;
                typeTotal = DictTypeClient.ReadTotal(outcome.Data);
                error = null;
                SyncSelectedType(forceDataReload);
            }, _ =>
            {
                typeLoading = false;
                error = "加载字典类型失败，请检查网络后重试。";
            });
        }

        private void LoadDataPage()
        {
            var selected = SelectedType;
            if (selected is null)
            {
                dataItems = [];
                dataRows = [];
                dataTotal = 0;
                return;
            }

            dataLoading = true;
            DictTypeClient.PageData(new PageDictDataInput
            {
                page = dataPage,
                pageSize = dataPageSize,
                field = dataField,
                order = dataOrder,
                dictTypeId = selected.id,
                label = dataLabel.Trim(),
                code = dataCode.Trim()
            }).Then(outcome =>
            {
                dataLoading = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载字典值失败。";
                    dataItems = [];
                    dataRows = [];
                    dataTotal = 0;
                    return;
                }

                dataItems = DictTypeClient.ReadDataList(outcome.Data);
                dataRows = dataItems;
                dataTotal = DictTypeClient.ReadTotal(outcome.Data);
                error = null;
            }, _ =>
            {
                dataLoading = false;
                error = "加载字典值失败，请检查网络后重试。";
            });
        }

        private void SyncSelectedType(bool forceDataReload)
        {
            var previousId = selectedTypeId;
            var selected = FindType(previousId);
            if (selected is null && typeItems.Length > 0)
                selected = typeItems[0];

            if (selected is null)
            {
                ClearSelection();
                return;
            }

            selectedTypeId = selected.id;
            if (forceDataReload || selected.id != previousId)
            {
                dataPage = 1;
                dataLabel = string.Empty;
                dataCode = string.Empty;
                LoadDataPage();
            }
        }

        private void SelectTypeRow(VueProps? row)
        {
            var id = RowString(row, "id");
            if (id.Length == 0 || id == selectedTypeId)
                return;

            selectedTypeId = id;
            dataLabel = string.Empty;
            dataCode = string.Empty;
            dataPage = 1;
            LoadDataPage();
        }

        private void ClearSelection()
        {
            selectedTypeId = string.Empty;
            dataItems = [];
            dataRows = [];
            dataTotal = 0;
        }

        private void OpenCreateType()
        {
            typeDialogTitle = "新增字典";
            editTypeId = string.Empty;
            editTypeName = string.Empty;
            editTypeCode = string.Empty;
            editTypeOrderNo = 100;
            editTypeRemark = string.Empty;
            editTypeStatus = 1;
            editTypeSysFlag = 2;
            editTypeIsTenant = 2;
            typeDialogVisible = true;
        }

        private void OpenEditType(VueProps? value)
        {
            var row = FindType(RowString(value, "id"));
            if (row is null || !HasTypePermission(row))
                return;

            typeDialogTitle = "编辑字典";
            editTypeId = row.id;
            editTypeName = row.name;
            editTypeCode = row.code;
            editTypeOrderNo = row.orderNo;
            editTypeRemark = row.remark ?? string.Empty;
            editTypeStatus = row.status;
            editTypeSysFlag = row.sysFlag;
            editTypeIsTenant = row.isTenant;
            typeDialogVisible = true;
        }

        private void SaveType()
        {
            if (typeSaving)
                return;

            var name = editTypeName.Trim();
            var code = editTypeCode.Trim();
            if (name.Length == 0 || code.Length == 0)
            {
                error = "字典名称和编码不能为空。";
                return;
            }

            typeSaving = true;
            var input = new DictTypeSaveInput
            {
                id = editTypeId,
                name = name,
                code = code,
                orderNo = editTypeOrderNo,
                remark = editTypeRemark.Trim(),
                status = editTypeStatus,
                sysFlag = IsSuperAdmin ? editTypeSysFlag : 2,
                isTenant = IsSuperAdmin ? editTypeIsTenant : 2
            };

            var request = editTypeId.Length == 0 ? DictTypeClient.Add(input) : DictTypeClient.Update(input);
            request.Then(outcome =>
            {
                typeSaving = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "保存字典失败。";
                    return;
                }

                typeDialogVisible = false;
                error = null;
                ElementPlus.ElMessageMembers.Success(editTypeId.Length == 0 ? "字典已新增" : "字典已更新");
                LoadTypePage(true);
            }, _ =>
            {
                typeSaving = false;
                error = "保存字典失败，请检查网络后重试。";
            });
        }

        private void DeleteType(VueProps? value)
        {
            var row = FindType(RowString(value, "id"));
            if (row is null || row.sysFlag == 1 || !HasTypePermission(row))
                return;

            DictTypeClient.Delete(row.id).Then(outcome =>
            {
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "删除字典失败。";
                    return;
                }

                if (selectedTypeId == row.id)
                    selectedTypeId = string.Empty;
                ElementPlus.ElMessageMembers.Success("字典已删除");
                LoadTypePage(true);
            }, _ => error = "删除字典失败，请检查网络后重试。");
        }

        private void OpenCreateData()
        {
            var selected = SelectedType;
            if (selected is null || !HasTypePermission(selected))
            {
                ElementPlus.ElMessageMembers.Warning("请先选择可维护的字典");
                return;
            }

            dataDialogTitle = "新增字典值";
            editDataId = string.Empty;
            editDataLabel = string.Empty;
            editDataValue = string.Empty;
            editDataCode = string.Empty;
            editDataName = string.Empty;
            editDataTagType = "primary";
            editDataStyle = string.Empty;
            editDataClass = string.Empty;
            editDataOrderNo = 100;
            editDataRemark = string.Empty;
            editDataExtData = string.Empty;
            editDataStatus = 1;
            editDataSystemLocked = false;
            dataDialogVisible = true;
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
            dataDialogTitle = copy ? "复制字典值" : "编辑字典值";
            editDataId = copy ? string.Empty : row.id;
            editDataLabel = row.label;
            editDataValue = row.value;
            editDataCode = row.code ?? string.Empty;
            editDataName = row.name ?? string.Empty;
            editDataTagType = NormalizeTagValue(row.tagType);
            editDataStyle = row.styleSetting ?? string.Empty;
            editDataClass = row.classSetting ?? string.Empty;
            editDataOrderNo = row.orderNo;
            editDataRemark = row.remark ?? string.Empty;
            editDataExtData = row.extData ?? string.Empty;
            editDataStatus = row.status;
            editDataSystemLocked = !copy && SelectedType?.sysFlag == 1;
            dataDialogVisible = true;
        }

        private void SaveData()
        {
            if (dataSaving)
                return;

            var selected = SelectedType;
            if (selected is null || !HasTypePermission(selected))
            {
                error = "当前字典不可维护。";
                return;
            }

            var label = editDataLabel.Trim();
            var value = editDataValue.Trim();
            if (label.Length == 0 || value.Length == 0)
            {
                error = "显示文本和字典值不能为空。";
                return;
            }

            dataSaving = true;
            var input = new DictDataSaveInput
            {
                id = editDataId,
                dictTypeId = selected.id,
                label = label,
                value = value,
                code = editDataCode.Trim(),
                name = editDataName.Trim(),
                tagType = NormalizeTagValue(editDataTagType),
                styleSetting = editDataStyle.Trim(),
                classSetting = editDataClass.Trim(),
                orderNo = editDataOrderNo,
                remark = editDataRemark.Trim(),
                extData = editDataExtData.Trim(),
                status = editDataStatus
            };

            var request = editDataId.Length == 0 ? DictTypeClient.AddData(input) : DictTypeClient.UpdateData(input);
            request.Then(outcome =>
            {
                dataSaving = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "保存字典值失败。";
                    return;
                }

                dataDialogVisible = false;
                error = null;
                ElementPlus.ElMessageMembers.Success(editDataId.Length == 0 ? "字典值已新增" : "字典值已更新");
                LoadDataPage();
            }, _ =>
            {
                dataSaving = false;
                error = "保存字典值失败，请检查网络后重试。";
            });
        }

        private void DeleteData(VueProps? value)
        {
            var row = FindData(RowString(value, "id"));
            var selected = SelectedType;
            if (row is null || selected is null || !HasTypePermission(selected))
                return;

            DictTypeClient.DeleteData(row.id).Then(outcome =>
            {
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "删除字典值失败。";
                    return;
                }

                ElementPlus.ElMessageMembers.Success("字典值已删除");
                LoadDataPage();
            }, _ => error = "删除字典值失败，请检查网络后重试。");
        }

        private bool HasTypePermission(DictTypeView? row)
            => row is not null && (row.sysFlag == 2 || IsSuperAdmin);

        private bool CanEditTypeRow(VueProps? row)
            => CanEditType && HasTypePermission(FindType(RowString(row, "id")));

        private bool CanDeleteTypeRow(VueProps? row)
        {
            var item = FindType(RowString(row, "id"));
            return CanDeleteType && item is not null && item.sysFlag == 2 && HasTypePermission(item);
        }

        private DictTypeView? FindType(string id)
        {
            for (var index = 0; index < typeItems.Length; index++)
                if (typeItems[index].id == id)
                    return typeItems[index];
            return null;
        }

        private DictDataView? FindData(string id)
        {
            for (var index = 0; index < dataItems.Length; index++)
                if (dataItems[index].id == id)
                    return dataItems[index];
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
            SetTypeName = v => typeName = v ?? "",
            SetTypeCode = v => typeCode = v ?? "",
            SetDataLabel = v => dataLabel = v ?? "",
            SetDataCode = v => dataCode = v ?? "",
            SetTypeDialogVisible = v => typeDialogVisible = v == true,
            SetEditTypeName = v => editTypeName = v ?? "",
            SetEditTypeCode = v => editTypeCode = v ?? "",
            SetEditTypeSysFlag = v => editTypeSysFlag = (int)v,
            SetEditTypeIsTenant = v => editTypeIsTenant = (int)v,
            SetEditTypeStatus = v => editTypeStatus = (int)v,
            SetEditTypeOrderNo = v => editTypeOrderNo = (int)(v ?? 100),
            SetEditTypeRemark = v => editTypeRemark = v ?? "",
            SetDataDialogVisible = v => dataDialogVisible = v == true,
            SetEditDataLabel = v => editDataLabel = v ?? "",
            SetEditDataValue = v => editDataValue = v ?? "",
            SetEditDataCode = v => editDataCode = v ?? "",
            SetEditDataName = v => editDataName = v ?? "",
            SetEditDataTagType = v => editDataTagType = v ?? "primary",
            SetEditDataStyle = v => editDataStyle = v ?? "",
            SetEditDataClass = v => editDataClass = v ?? "",
            SetEditDataStatus = v => editDataStatus = (int)v,
            SetEditDataOrderNo = v => editDataOrderNo = (int)(v ?? 100),
            SetEditDataRemark = v => editDataRemark = v ?? "",
            SetEditDataExtData = v => editDataExtData = v ?? "",
            IsSuperAdmin = IsSuperAdmin,
            CanEditType = CanEditType,
            CanDeleteType = CanDeleteType,
            SelectTypeRow = id => SelectTypeRow(typeRows.FirstOrDefault(row => row.id == id)),
            OpenEditType = id => OpenEditType(typeRows.FirstOrDefault(row => row.id == id)),
            DeleteType = id => DeleteType(typeRows.FirstOrDefault(row => row.id == id)),
            SortTypes = (prop, order) => SortTypes(prop, order),
            CanAddData = CanAddData,
            CanEditData = CanEditData,
            CanDeleteData = CanDeleteData,
            CanOperateSelectedType = CanOperateSelectedType,
            OpenEditData = id => OpenEditData(dataRows.FirstOrDefault(row => row.id == id)),
            OpenCopyData = id => OpenCopyData(dataRows.FirstOrDefault(row => row.id == id)),
            DeleteData = id => DeleteData(dataRows.FirstOrDefault(row => row.id == id)),
            SortData = (prop, order) => SortData(prop, order),
            DismissError = () => error = null,
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
            CancelType = () => typeDialogVisible = false,
            SaveType = SaveType,
            CancelData = () => dataDialogVisible = false,
            SaveData = SaveData,
            PageSizes = JoltPageSizes,
            TypeRows = typeRows,
            TypePage = typePage,
            TypePageSize = typePageSize,
            TypeTotal = typeTotal,
            TypeName = typeName,
            TypeCode = typeCode,
            TypeLoading = typeLoading,
            DataRows = dataRows,
            DataPage = dataPage,
            DataPageSize = dataPageSize,
            DataTotal = dataTotal,
            DataLabel = dataLabel,
            DataCode = dataCode,
            DataLoading = dataLoading,
            Error = error,
            TypeDialogVisible = typeDialogVisible,
            TypeDialogTitle = typeDialogTitle,
            TypeSaving = typeSaving,
            EditTypeId = editTypeId,
            EditTypeName = editTypeName,
            EditTypeCode = editTypeCode,
            EditTypeOrderNo = editTypeOrderNo,
            EditTypeRemark = editTypeRemark,
            EditTypeStatus = editTypeStatus,
            EditTypeSysFlag = editTypeSysFlag,
            EditTypeIsTenant = editTypeIsTenant,
            DataDialogVisible = dataDialogVisible,
            DataDialogTitle = dataDialogTitle,
            DataSaving = dataSaving,
            EditDataLabel = editDataLabel,
            EditDataValue = editDataValue,
            EditDataCode = editDataCode,
            EditDataName = editDataName,
            EditDataTagType = editDataTagType,
            EditDataStyle = editDataStyle,
            EditDataClass = editDataClass,
            EditDataOrderNo = editDataOrderNo,
            EditDataRemark = editDataRemark,
            EditDataExtData = editDataExtData,
            EditDataStatus = editDataStatus,
            EditDataSystemLocked = editDataSystemLocked,
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
