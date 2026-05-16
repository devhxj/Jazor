using System.Reflection;
using ECMAScript;
using ECMAScript.ElementPlus;
using Microsoft.AspNetCore.Components;
using static ECMAScript.Vue3;

namespace Jazor.RazorVue.Test;

[TestClass]
public sealed class ElementPlusCallbackContractTests
{
    [TestMethod]
    public void ElementPlus_ComponentParameterSurface_DoesNotFallbackToSystemDelegate()
    {
        var delegateProps = typeof(ElementPlus).Assembly
            .GetExportedTypes()
            .Where(static type =>
                type.Namespace == "ECMAScript.ElementPlus" &&
                typeof(ElementPlusComponentBase).IsAssignableFrom(type) &&
                type != typeof(ElementPlusComponentBase) &&
                !type.IsAbstract)
            .SelectMany(static type => type
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(static property => property.GetCustomAttribute<ParameterAttribute>(inherit: true) is not null &&
                                          property.PropertyType == typeof(Delegate))
                .Select(property => $"{type.Name}.{property.Name}"))
            .OrderBy(static item => item, StringComparer.Ordinal)
            .ToArray();

        Assert.AreEqual(
            0,
            delegateProps.Length,
            "System.Delegate fallback leaked into generated ElementPlus component props: " + string.Join(", ", delegateProps));
    }

    [TestMethod]
    public void ElementPlus_ComponentFunctionProps_UseExpectedNamedContracts()
    {
        var expectations = new (Type ComponentType, string PropertyName, Type ExpectedType)[]
        {
            (typeof(ElAutoResizer), nameof(ElAutoResizer.OnResize), typeof(ElementPlusAutoResizerResizeCallback)),
            (typeof(ElAutocomplete), nameof(ElAutocomplete.FetchSuggestions), typeof(ElementPlusAutocompleteFetchSuggestionsValue?)),
            (typeof(ElCalendar), nameof(ElCalendar.Formatter), typeof(ElementPlusCalendarFormatterCallback)),
            (typeof(ElCascader), nameof(ElCascader.FilterMethod), typeof(ElementPlusCascaderFilterMethod)),
            (typeof(ElCascader), nameof(ElCascader.BeforeFilter), typeof(ElementPlusCascaderBeforeFilterCallback)),
            (typeof(ElCollapse), nameof(ElCollapse.BeforeCollapse), typeof(ElementPlusCollapseBeforeCollapseCallback)),
            (typeof(ElDatePicker), nameof(ElDatePicker.DisabledDate), typeof(ElementPlusDateLikeDisabledDate)),
            (typeof(ElDatePicker), nameof(ElDatePicker.CellClassName), typeof(ElementPlusDateLikeCellClassName)),
            (typeof(ElDatePickerPanel), nameof(ElDatePickerPanel.DisabledDate), typeof(ElementPlusDateLikeDisabledDate)),
            (typeof(ElDatePickerPanel), nameof(ElDatePickerPanel.CellClassName), typeof(ElementPlusDateLikeCellClassName)),
            (typeof(ElDialog), nameof(ElDialog.BeforeClose), typeof(ElementPlusDialogBeforeCloseCallback)),
            (typeof(ElDrawer), nameof(ElDrawer.BeforeClose), typeof(ElementPlusDialogBeforeCloseCallback)),
            (typeof(ElInput), nameof(ElInput.Formatter), typeof(ElementPlusInputFormatter)),
            (typeof(ElInput), nameof(ElInput.Parser), typeof(ElementPlusInputParser)),
            (typeof(ElInput), nameof(ElInput.CountGraphemes), typeof(ElementPlusInputCountGraphemes)),
            (typeof(ElInputNumber), nameof(ElInputNumber.Formatter), typeof(ElementPlusInputFormatter)),
            (typeof(ElInputNumber), nameof(ElInputNumber.Parser), typeof(ElementPlusInputParser)),
            (typeof(ElInputOtp), nameof(ElInputOtp.Validator), typeof(ElementPlusInputOtpValidator)),
            (typeof(ElInputOtp), nameof(ElInputOtp.Separator), typeof(ElementPlusInputOtpSeparatorValue?)),
            (typeof(ElMention), nameof(ElMention.FilterOption), typeof(ElementPlusMentionFilterOptionValue?)),
            (typeof(ElMention), nameof(ElMention.CheckIsWhole), typeof(ElementPlusMentionCheckIsWhole)),
            (typeof(ElProgress), nameof(ElProgress.Color), typeof(ElementPlusProgressColorValue?)),
            (typeof(ElProgress), nameof(ElProgress.Format), typeof(ElementPlusProgressFormatCallback)),
            (typeof(ElSelect), nameof(ElSelect.FilterMethod), typeof(ElementPlusSelectQueryCallback)),
            (typeof(ElSelect), nameof(ElSelect.RemoteMethod), typeof(ElementPlusSelectQueryCallback)),
            (typeof(ElVirtualizedSelect), nameof(ElVirtualizedSelect.FilterMethod), typeof(ElementPlusSelectQueryCallback)),
            (typeof(ElVirtualizedSelect), nameof(ElVirtualizedSelect.RemoteMethod), typeof(ElementPlusSelectQueryCallback)),
            (typeof(ElSlider), nameof(ElSlider.FormatTooltip), typeof(ElementPlusSliderFormatTooltipCallback)),
            (typeof(ElSlider), nameof(ElSlider.FormatValueText), typeof(ElementPlusSliderFormatValueTextCallback)),
            (typeof(ElSwitch), nameof(ElSwitch.BeforeChange), typeof(ElementPlusSwitchBeforeChangeCallback)),
            (typeof(ElTable), nameof(ElTable.RowClassName), typeof(ElementPlusTableRowClassNameValue?)),
            (typeof(ElTable), nameof(ElTable.RowStyle), typeof(ElementPlusTableRowStyleValue?)),
            (typeof(ElTable), nameof(ElTable.CellClassName), typeof(ElementPlusTableCellClassNameValue?)),
            (typeof(ElTable), nameof(ElTable.CellStyle), typeof(ElementPlusTableCellStyleValue?)),
            (typeof(ElTable), nameof(ElTable.HeaderRowClassName), typeof(ElementPlusTableRowClassNameValue?)),
            (typeof(ElTable), nameof(ElTable.HeaderRowStyle), typeof(ElementPlusTableRowStyleValue?)),
            (typeof(ElTable), nameof(ElTable.HeaderCellClassName), typeof(ElementPlusTableCellClassNameValue?)),
            (typeof(ElTable), nameof(ElTable.HeaderCellStyle), typeof(ElementPlusTableCellStyleValue?)),
            (typeof(ElTable), nameof(ElTable.RowKey), typeof(ElementPlusTableRowKeyValue?)),
            (typeof(ElTable), nameof(ElTable.SummaryMethod), typeof(ElementPlusTableSummaryMethodCallback)),
            (typeof(ElTable), nameof(ElTable.SpanMethod), typeof(ElementPlusTableSpanMethodCallback)),
            (typeof(ElTable), nameof(ElTable.Load), typeof(ElementPlusTableLoadCallback)),
            (typeof(ElTable), nameof(ElTable.TooltipFormatter), typeof(ElementPlusTableTooltipFormatter)),
            (typeof(ElTable), nameof(ElTable.RowExpandable), typeof(ElementPlusTableRowExpandableCallback)),
            (typeof(ElTableColumn), nameof(ElTableColumn.Index), typeof(ElementPlusTableColumnIndexValue?)),
            (typeof(ElTableColumn), nameof(ElTableColumn.RenderHeader), typeof(ElementPlusTableColumnRenderHeaderCallback)),
            (typeof(ElTableColumn), nameof(ElTableColumn.SortMethod), typeof(ElementPlusTableColumnSortMethodCallback)),
            (typeof(ElTableColumn), nameof(ElTableColumn.SortBy), typeof(ElementPlusTableColumnSortByValue?)),
            (typeof(ElTableColumn), nameof(ElTableColumn.Formatter), typeof(ElementPlusTableColumnFormatterCallback)),
            (typeof(ElTableColumn), nameof(ElTableColumn.Selectable), typeof(ElementPlusTableColumnSelectableCallback)),
            (typeof(ElTableColumn), nameof(ElTableColumn.FilterMethod), typeof(ElementPlusTableColumnFilterMethodCallback)),
            (typeof(ElTableColumn), nameof(ElTableColumn.TooltipFormatter), typeof(ElementPlusTableTooltipFormatter)),
            (typeof(ElTabs), nameof(ElTabs.BeforeLeave), typeof(ElementPlusTabsBeforeLeaveCallback)),
            (typeof(ElTimePicker), nameof(ElTimePicker.DisabledHours), typeof(ElementPlusTimePickerDisabledHoursCallback)),
            (typeof(ElTimePicker), nameof(ElTimePicker.DisabledMinutes), typeof(ElementPlusTimePickerDisabledMinutesCallback)),
            (typeof(ElTimePicker), nameof(ElTimePicker.DisabledSeconds), typeof(ElementPlusTimePickerDisabledSecondsCallback)),
            (typeof(ElTransfer), nameof(ElTransfer.FilterMethod), typeof(ElementPlusTransferFilterMethod)),
            (typeof(ElTree), nameof(ElTree.Load), typeof(ElementPlusTreeLoadCallback)),
            (typeof(ElTree), nameof(ElTree.RenderContent), typeof(ElementPlusTreeRenderContentCallback)),
            (typeof(ElTree), nameof(ElTree.FilterNodeMethod), typeof(ElementPlusTreeFilterNodeMethod)),
            (typeof(ElTree), nameof(ElTree.AllowDrag), typeof(ElementPlusTreeAllowDragCallback)),
            (typeof(ElTree), nameof(ElTree.AllowDrop), typeof(ElementPlusTreeAllowDropCallback)),
            (typeof(ElTreeSelect), nameof(ElTreeSelect.FilterMethod), typeof(ElementPlusSelectQueryCallback)),
            (typeof(ElTreeSelect), nameof(ElTreeSelect.RemoteMethod), typeof(ElementPlusSelectQueryCallback)),
            (typeof(ElTreeSelect), nameof(ElTreeSelect.Load), typeof(ElementPlusTreeLoadCallback)),
            (typeof(ElTreeSelect), nameof(ElTreeSelect.RenderContent), typeof(ElementPlusTreeRenderContentCallback)),
            (typeof(ElTreeSelect), nameof(ElTreeSelect.FilterNodeMethod), typeof(ElementPlusTreeFilterNodeMethod)),
            (typeof(ElTreeSelect), nameof(ElTreeSelect.AllowDrag), typeof(ElementPlusTreeAllowDragCallback)),
            (typeof(ElTreeSelect), nameof(ElTreeSelect.AllowDrop), typeof(ElementPlusTreeAllowDropCallback)),
            (typeof(ElTreeV2), nameof(ElTreeV2.FilterMethod), typeof(ElementPlusTreeV2FilterMethod)),
            (typeof(ElUpload), nameof(ElUpload.Data), typeof(ElementPlusUploadDataValue?)),
            (typeof(ElUpload), nameof(ElUpload.OnPreview), typeof(ElementPlusUploadPreviewCallback)),
            (typeof(ElUpload), nameof(ElUpload.OnRemove), typeof(ElementPlusUploadFileListCallback)),
            (typeof(ElUpload), nameof(ElUpload.OnSuccess), typeof(ElementPlusUploadSuccessCallback)),
            (typeof(ElUpload), nameof(ElUpload.OnError), typeof(ElementPlusUploadErrorCallback)),
            (typeof(ElUpload), nameof(ElUpload.OnProgress), typeof(ElementPlusUploadProgressCallback)),
            (typeof(ElUpload), nameof(ElUpload.OnChange), typeof(ElementPlusUploadFileListCallback)),
            (typeof(ElUpload), nameof(ElUpload.OnExceed), typeof(ElementPlusUploadExceedCallback)),
            (typeof(ElUpload), nameof(ElUpload.BeforeUpload), typeof(ElementPlusUploadBeforeUploadCallback)),
            (typeof(ElUpload), nameof(ElUpload.BeforeRemove), typeof(ElementPlusUploadBeforeRemoveCallback)),
            (typeof(ElUpload), nameof(ElUpload.HttpRequest), typeof(ElementPlusUploadRequestCallback))
        };

        foreach (var (componentType, propertyName, expectedType) in expectations)
        {
            AssertPropertyType(componentType, propertyName, expectedType);
        }
    }

    [TestMethod]
    public void ElementPlus_ComplexCallbackContracts_UseOfficialParametersAndReturnShapes()
    {
        AssertDelegateReturnType<ElementPlusAutocompleteFetchSuggestionsAsyncCallback>(typeof(IPromise<ElementPlusAutocompleteSuggestionItem[]?>));
        AssertDelegateReturnType<ElementPlusCascaderBeforeFilterCallback>(typeof(ElementPlusCascaderBeforeFilterResult));
        AssertDelegateReturnType<ElementPlusTabsBeforeLeaveSyncCallback>(typeof(bool?));
        AssertDelegateReturnType<ElementPlusTabsBeforeLeaveAsyncCallback>(typeof(IPromise<bool?>));
        AssertDelegateReturnType<ElementPlusTabsBeforeLeaveCallback>(typeof(ElementPlusTabsBeforeLeaveResult?));
        AssertDelegateReturnType<ElementPlusUploadDataPromiseFactory>(typeof(IPromise<ElementPlusUploadData>));
        AssertDelegateReturnType<ElementPlusUploadBeforeUploadCallback>(typeof(ElementPlusUploadBeforeUploadResult?));
        AssertDelegateReturnType<ElementPlusUploadBeforeRemoveCallback>(typeof(ElementPlusUploadBeforeRemoveResult));

        AssertDelegateParameterTypes<ElementPlusUploadBeforeRemoveCallback>(typeof(ElementPlusUploadFile), typeof(ElementPlusUploadFile[]));
        AssertDelegateParameterTypes<ElementPlusUploadPreviewCallback>(typeof(ElementPlusUploadFile));
        AssertDelegateParameterTypes<ElementPlusUploadFileListCallback>(typeof(ElementPlusUploadFile), typeof(ElementPlusUploadFile[]));
        AssertDelegateParameterTypes<ElementPlusUploadSuccessCallback>(typeof(VueValue), typeof(ElementPlusUploadFile), typeof(ElementPlusUploadFile[]));
        AssertDelegateParameterTypes<ElementPlusUploadProgressCallback>(typeof(ElementPlusUploadProgressEvent), typeof(ElementPlusUploadFile), typeof(ElementPlusUploadFile[]));
        AssertDelegateParameterTypes<ElementPlusUploadErrorCallback>(typeof(Error), typeof(ElementPlusUploadFile), typeof(ElementPlusUploadFile[]));
        AssertDelegateParameterTypes<ElementPlusUploadExceedCallback>(typeof(ECMAScript.File[]), typeof(ElementPlusUploadUserFile[]));

        AssertPropertyType(typeof(ElementPlusUploadFile), nameof(ElementPlusUploadFile.Status), typeof(ElementPlusUploadStatus));
        AssertPropertyType(typeof(ElementPlusUploadFile), nameof(ElementPlusUploadFile.Uid), typeof(Number));
        AssertPropertyType(typeof(ElementPlusUploadUserFile), nameof(ElementPlusUploadUserFile.Status), typeof(ElementPlusUploadStatus?));
        AssertPropertyType(typeof(ElementPlusUploadUserFile), nameof(ElementPlusUploadUserFile.Uid), typeof(Number?));
    }

    private static void AssertPropertyType(Type declaringType, string propertyName, Type expectedType)
    {
        var property = declaringType.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
        Assert.IsNotNull(property, $"{declaringType.FullName}.{propertyName}");
        Assert.AreEqual(expectedType, property!.PropertyType, $"{declaringType.FullName}.{propertyName}");
    }

    private static void AssertDelegateReturnType<TDelegate>(Type expectedType)
        where TDelegate : Delegate
    {
        var invoke = typeof(TDelegate).GetMethod("Invoke", BindingFlags.Instance | BindingFlags.Public);
        Assert.IsNotNull(invoke, typeof(TDelegate).FullName);
        Assert.AreEqual(expectedType, invoke!.ReturnType, typeof(TDelegate).FullName);
    }

    private static void AssertDelegateParameterTypes<TDelegate>(params Type[] expectedTypes)
        where TDelegate : Delegate
    {
        var invoke = typeof(TDelegate).GetMethod("Invoke", BindingFlags.Instance | BindingFlags.Public);
        Assert.IsNotNull(invoke, typeof(TDelegate).FullName);

        var actualTypes = invoke!.GetParameters()
            .Select(static parameter => parameter.ParameterType)
            .ToArray();

        CollectionAssert.AreEqual(expectedTypes, actualTypes, typeof(TDelegate).FullName);
    }
}
