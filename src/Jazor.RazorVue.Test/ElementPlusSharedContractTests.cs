using System.Reflection;
using ECMAScript;
using ECMAScript.ElementPlus;
using Microsoft.AspNetCore.Components;
using static ECMAScript.Vue3;

namespace Jazor.RazorVue.Test;

[TestClass]
public sealed class ElementPlusSharedContractTests
{
    [TestMethod]
    public void ElementPlus_BaseAuthoringSurface_UsesSharedVueCssContracts()
    {
        Assert.AreEqual(typeof(VueClassValue?), typeof(ElementPlusComponentBase).GetProperty(nameof(ElementPlusComponentBase.CssClass))?.PropertyType);
        Assert.AreEqual(typeof(VueStyleValue?), typeof(ElementPlusComponentBase).GetProperty(nameof(ElementPlusComponentBase.CssStyle))?.PropertyType);

        var additionalAttributes = typeof(ElementPlusComponentBase).GetProperty(nameof(ElementPlusComponentBase.AdditionalAttributes));
        Assert.IsNotNull(additionalAttributes);
        Assert.AreEqual(typeof(IReadOnlyDictionary<string, object?>), additionalAttributes!.PropertyType);

        var parameter = additionalAttributes.GetCustomAttribute<ParameterAttribute>(inherit: true);
        Assert.IsNotNull(parameter);
        Assert.IsTrue(parameter!.CaptureUnmatchedValues);
    }

    [TestMethod]
    public void ElementPlus_CommonOptionShapes_ReuseSharedVueUnions()
    {
        Assert.AreEqual(typeof(VueTeleportTarget?), typeof(ElementPlusLoadingOptions).GetProperty(nameof(ElementPlusLoadingOptions.Target))?.PropertyType);
        Assert.AreEqual(typeof(VueBooleanStringValue?), typeof(ElementPlusLinkConfig).GetProperty(nameof(ElementPlusLinkConfig.Underline))?.PropertyType);
        Assert.AreEqual(typeof(VueTransitionValue?), typeof(ElementPlusDialogConfig).GetProperty(nameof(ElementPlusDialogConfig.Transition))?.PropertyType);
    }

    [TestMethod]
    public void ElementPlus_ComponentContracts_ReuseSharedVueUnions()
    {
        Assert.AreEqual(typeof(VueStringNumberValue?), typeof(ElDialog).GetProperty(nameof(ElDialog.Width))?.PropertyType);
        Assert.AreEqual(typeof(VueBooleanStringNumberValue?), typeof(ElSwitch).GetProperty(nameof(ElSwitch.ModelValue))?.PropertyType);
        Assert.AreEqual(typeof(VueBooleanStringNumberValue?), typeof(ElSwitch).GetProperty(nameof(ElSwitch.ActiveValue))?.PropertyType);
        Assert.AreEqual(typeof(VueBooleanStringNumberValue?), typeof(ElSwitch).GetProperty(nameof(ElSwitch.InactiveValue))?.PropertyType);
        Assert.AreEqual(typeof(VueStringNumberValue?), typeof(ElInput).GetProperty(nameof(ElInput.ModelValue))?.PropertyType);
        Assert.AreEqual(typeof(VueStringNumberValue?), typeof(ElInput).GetProperty(nameof(ElInput.Minlength))?.PropertyType);
        Assert.AreEqual(typeof(VueStringNumberValue?), typeof(ElInputOtp).GetProperty(nameof(ElInputOtp.ModelValue))?.PropertyType);
        Assert.AreEqual(typeof(string[]), typeof(ElInputTag).GetProperty(nameof(ElInputTag.ModelValue))?.PropertyType);
        Assert.AreEqual(typeof(VueTransitionValue?), typeof(ElDialog).GetProperty(nameof(ElDialog.Transition))?.PropertyType);
        Assert.AreEqual(typeof(VueBooleanNumberValue?), typeof(ElCascader).GetProperty(nameof(ElCascader.FitInputWidth))?.PropertyType);
        Assert.AreEqual(typeof(VueBooleanNumberValue?), typeof(ElVirtualizedSelect).GetProperty(nameof(ElVirtualizedSelect.FitInputWidth))?.PropertyType);
        Assert.AreEqual(typeof(string[]), typeof(ElTooltip).GetProperty(nameof(ElTooltip.TriggerKeys))?.PropertyType);
        Assert.AreEqual(typeof(string[]), typeof(ElPopover).GetProperty(nameof(ElPopover.TriggerKeys))?.PropertyType);
        Assert.AreEqual(typeof(ElementPlusTooltipTriggerValue?), typeof(ElTooltip).GetProperty(nameof(ElTooltip.Trigger))?.PropertyType);
        Assert.AreEqual(typeof(ElementPlusTooltipTriggerValue?), typeof(ElPopover).GetProperty(nameof(ElPopover.Trigger))?.PropertyType);
        Assert.AreEqual(typeof(string), typeof(ElCarousel).GetProperty(nameof(ElCarousel.Trigger))?.PropertyType);
        Assert.AreEqual(typeof(string), typeof(ElMenu).GetProperty(nameof(ElMenu.MenuTrigger))?.PropertyType);
        Assert.AreEqual(typeof(VueStringNumberValue?), typeof(ElInput).GetProperty(nameof(ElInput.Max))?.PropertyType);
        Assert.AreEqual(typeof(VueStringNumberValue?), typeof(ElInput).GetProperty(nameof(ElInput.Min))?.PropertyType);
        Assert.AreEqual(typeof(VueStringNumberValue?), typeof(ElInput).GetProperty(nameof(ElInput.Step))?.PropertyType);
        Assert.AreEqual(typeof(VueStyleValue?), typeof(ElInput).GetProperty(nameof(ElInput.InputStyle))?.PropertyType);
        Assert.AreEqual(typeof(ElementPlusInputAutoSize?), typeof(ElInput).GetProperty(nameof(ElInput.Autosize))?.PropertyType);
        Assert.AreEqual(typeof(ElementPlusTagTooltipProps), typeof(ElVirtualizedSelect).GetProperty("TagTooltip")?.PropertyType);
        Assert.AreEqual(typeof(ElementPlusTagTooltipProps), typeof(ElTreeSelect).GetProperty("TagTooltip")?.PropertyType);
        Assert.AreEqual(typeof(ElementPlusTagTooltipProps), typeof(ElSelect).GetProperty("TagTooltip")?.PropertyType);
        Assert.AreEqual(typeof(VueStringNumberValue[]), typeof(ElTransfer).GetProperty(nameof(ElTransfer.ModelValue))?.PropertyType);
        Assert.AreEqual(typeof(VueStringNumberValue[]), typeof(ElTransfer).GetProperty(nameof(ElTransfer.LeftDefaultChecked))?.PropertyType);
        Assert.AreEqual(typeof(VueStringNumberValue[]), typeof(ElTransfer).GetProperty(nameof(ElTransfer.RightDefaultChecked))?.PropertyType);
    }

    [TestMethod]
    public void ElementPlus_DropdownAndFormContracts_UseOfficialNamedTypes()
    {
        Assert.AreEqual(typeof(ElementPlusDropdownTriggerValue?), typeof(ElDropdown).GetProperty(nameof(ElDropdown.Trigger))?.PropertyType);
        Assert.AreEqual(typeof(ElementPlusButtonProps), typeof(ElDropdown).GetProperty(nameof(ElDropdown.ButtonProps))?.PropertyType);
        Assert.AreEqual(typeof(ScrollIntoViewArg?), typeof(ElForm).GetProperty(nameof(ElForm.ScrollIntoViewOptions))?.PropertyType);
    }

    [TestMethod]
    public void ElementPlus_BracketedOfficialProps_UseNamedContracts()
    {
        Assert.AreEqual(typeof(ElementPlusTransferDataItem[]), typeof(ElTransfer).GetProperty(nameof(ElTransfer.Data))?.PropertyType);
        Assert.AreEqual(typeof(ElementPlusTransferTargetOrder?), typeof(ElTransfer).GetProperty(nameof(ElTransfer.TargetOrder))?.PropertyType);
        Assert.AreEqual(typeof(ElementPlusTransferTextPair?), typeof(ElTransfer).GetProperty(nameof(ElTransfer.Titles))?.PropertyType);
        Assert.AreEqual(typeof(ElementPlusTransferTextPair?), typeof(ElTransfer).GetProperty(nameof(ElTransfer.ButtonTexts))?.PropertyType);
        Assert.AreEqual(typeof(ElementPlusTransferFormat), typeof(ElTransfer).GetProperty(nameof(ElTransfer.Format))?.PropertyType);
        Assert.AreEqual(typeof(ElementPlusTransferPropsAlias), typeof(ElTransfer).GetProperty("Props")?.PropertyType);

        Assert.AreEqual(typeof(ElementPlusSelectPropsAlias), typeof(ElSelect).GetProperty("Props")?.PropertyType);
        Assert.AreEqual(typeof(ElementPlusSelectPropsAlias), typeof(ElVirtualizedSelect).GetProperty("Props")?.PropertyType);
        Assert.AreEqual(typeof(ElementPlusCheckboxOptionPropsAlias), typeof(ElCheckboxGroup).GetProperty("Props")?.PropertyType);
        Assert.AreEqual(typeof(ElementPlusMentionOptionPropsAlias), typeof(ElMention).GetProperty("Props")?.PropertyType);
        Assert.AreEqual(typeof(ElementPlusRadioOptionPropsAlias), typeof(ElRadioGroup).GetProperty("Props")?.PropertyType);
        Assert.AreEqual(typeof(ElementPlusSegmentedPropsAlias), typeof(ElSegmented).GetProperty("Props")?.PropertyType);
        Assert.AreEqual(typeof(ElementPlusTreeOptionProps), typeof(ElTree).GetProperty("Props")?.PropertyType);
        Assert.AreEqual(typeof(ElementPlusTreeOptionProps), typeof(ElTreeV2).GetProperty("Props")?.PropertyType);
        Assert.AreEqual(typeof(ElementPlusCascaderProps), typeof(ElCascader).GetProperty("Props")?.PropertyType);
        Assert.AreEqual(typeof(ElementPlusCascaderProps), typeof(ElCascaderPanel).GetProperty("Props")?.PropertyType);
    }

    [TestMethod]
    public void ElementPlus_ConfigProviderContracts_UseConsistentNamedValueTypes()
    {
        Assert.AreEqual(typeof(ElementPlusLanguage), typeof(ElementPlusInstallOptions).GetProperty(nameof(ElementPlusInstallOptions.Locale))?.PropertyType);
        Assert.AreEqual(typeof(ElementPlusLanguage), typeof(ElConfigProvider).GetProperty(nameof(ElConfigProvider.Locale))?.PropertyType);

        Assert.AreEqual(typeof(ElementPlusButtonConfig), typeof(ElementPlusInstallOptions).GetProperty(nameof(ElementPlusInstallOptions.Button))?.PropertyType);
        Assert.AreEqual(typeof(ElementPlusButtonConfig), typeof(ElConfigProvider).GetProperty(nameof(ElConfigProvider.Button))?.PropertyType);

        Assert.AreEqual(typeof(ElementPlusCardConfig), typeof(ElementPlusInstallOptions).GetProperty(nameof(ElementPlusInstallOptions.Card))?.PropertyType);
        Assert.AreEqual(typeof(ElementPlusCardConfig), typeof(ElConfigProvider).GetProperty(nameof(ElConfigProvider.Card))?.PropertyType);

        Assert.AreEqual(typeof(ElementPlusLinkConfig), typeof(ElementPlusInstallOptions).GetProperty(nameof(ElementPlusInstallOptions.Link))?.PropertyType);
        Assert.AreEqual(typeof(ElementPlusLinkConfig), typeof(ElConfigProvider).GetProperty(nameof(ElConfigProvider.Link))?.PropertyType);

        Assert.AreEqual(typeof(ElementPlusDialogConfig), typeof(ElementPlusInstallOptions).GetProperty(nameof(ElementPlusInstallOptions.Dialog))?.PropertyType);
        Assert.AreEqual(typeof(ElementPlusDialogConfig), typeof(ElConfigProvider).GetProperty(nameof(ElConfigProvider.Dialog))?.PropertyType);

        Assert.AreEqual(typeof(ElementPlusMessageConfig), typeof(ElementPlusInstallOptions).GetProperty(nameof(ElementPlusInstallOptions.Message))?.PropertyType);
        Assert.AreEqual(typeof(ElementPlusMessageConfig), typeof(ElConfigProvider).GetProperty(nameof(ElConfigProvider.Message))?.PropertyType);

        Assert.AreEqual(typeof(ElementPlusTableConfig), typeof(ElementPlusInstallOptions).GetProperty(nameof(ElementPlusInstallOptions.Table))?.PropertyType);
        Assert.AreEqual(typeof(ElementPlusTableConfig), typeof(ElConfigProvider).GetProperty(nameof(ElConfigProvider.Table))?.PropertyType);

        Assert.AreEqual(typeof(VueProps), typeof(ElementPlusInstallOptions).GetProperty(nameof(ElementPlusInstallOptions.ExperimentalFeatures))?.PropertyType);
        Assert.AreEqual(typeof(VueProps), typeof(ElConfigProvider).GetProperty(nameof(ElConfigProvider.ExperimentalFeatures))?.PropertyType);

        Assert.AreEqual(typeof(VueValue[]), typeof(ElementPlusInstallOptions).GetProperty(nameof(ElementPlusInstallOptions.EmptyValues))?.PropertyType);
        Assert.AreEqual(typeof(VueValue[]), typeof(ElConfigProvider).GetProperty(nameof(ElConfigProvider.EmptyValues))?.PropertyType);

        Assert.AreEqual(typeof(ElementPlusValueOnClearValue?), typeof(ElementPlusInstallOptions).GetProperty(nameof(ElementPlusInstallOptions.ValueOnClear))?.PropertyType);
        Assert.AreEqual(typeof(ElementPlusValueOnClearValue?), typeof(ElConfigProvider).GetProperty(nameof(ElConfigProvider.ValueOnClear))?.PropertyType);
    }

    [TestMethod]
    public void ElementPlus_StructuredOptionShapes_UseNamedElementPlusContracts()
    {
        Assert.AreEqual(typeof(ElementPlusTranslatePair), typeof(ElementPlusLanguage).GetProperty(nameof(ElementPlusLanguage.El))?.PropertyType);

        Assert.AreEqual(typeof(ElementPlusTableOverflowTooltipValue?), typeof(ElementPlusTableConfig).GetProperty(nameof(ElementPlusTableConfig.ShowOverflowTooltip))?.PropertyType);
        Assert.AreEqual(typeof(ElementPlusTableOverflowTooltipOptions), typeof(ElementPlusTableConfig).GetProperty(nameof(ElementPlusTableConfig.TooltipOptions))?.PropertyType);

        Assert.AreEqual(typeof(ElementPlusTableOverflowTooltipValue?), typeof(ElTable).GetProperty(nameof(ElTable.ShowOverflowTooltip))?.PropertyType);
        Assert.AreEqual(typeof(ElementPlusTableOverflowTooltipOptions), typeof(ElTable).GetProperty(nameof(ElTable.TooltipOptions))?.PropertyType);
        Assert.AreEqual(typeof(ElementPlusTableOverflowTooltipValue?), typeof(ElTableColumn).GetProperty(nameof(ElTableColumn.ShowOverflowTooltip))?.PropertyType);
    }

    [TestMethod]
    public void ElementPlus_UseEmptyValuesProps_UseConsistentNamedContracts()
    {
        var componentsWithEmptyValues = new[]
        {
            typeof(ElCascader),
            typeof(ElColorPicker),
            typeof(ElDatePicker),
            typeof(ElSelect),
            typeof(ElTimePicker),
            typeof(ElTimeSelect),
            typeof(ElTreeSelect),
            typeof(ElVirtualizedSelect)
        };

        foreach (var component in componentsWithEmptyValues)
        {
            Assert.AreEqual(typeof(VueValue[]), component.GetProperty("EmptyValues")?.PropertyType, component.FullName);
            Assert.AreEqual(typeof(ElementPlusValueOnClearValue?), component.GetProperty("ValueOnClear")?.PropertyType, component.FullName);
        }

        Assert.IsNull(typeof(ElAutocomplete).GetProperty("EmptyValues"), typeof(ElAutocomplete).FullName);
        Assert.IsNull(typeof(ElAutocomplete).GetProperty("ValueOnClear"), typeof(ElAutocomplete).FullName);
        Assert.IsNull(typeof(ElInputTag).GetProperty("ValueOnClear"), typeof(ElInputTag).FullName);

        Assert.AreEqual(typeof(ElementPlusValueOnClearValue?), typeof(ElInputNumber).GetProperty("ValueOnClear")?.PropertyType);
    }

    [TestMethod]
    public void ElementPlus_PublicNamespace_DoesNotReintroduceDuplicatedCommonVueUnions()
    {
        var publicTypeNames = typeof(ElementPlus).Assembly
            .GetExportedTypes()
            .Where(static type => type.Namespace == "ECMAScript.ElementPlus")
            .Select(static type => type.Name)
            .OrderBy(static name => name, StringComparer.Ordinal)
            .ToArray();

        CollectionAssert.DoesNotContain(publicTypeNames, "ElementPlusBooleanStringValue");
        CollectionAssert.DoesNotContain(publicTypeNames, "ElementPlusStringOrComponent");
        CollectionAssert.DoesNotContain(publicTypeNames, "ElementPlusSelectorOrElement");
        CollectionAssert.DoesNotContain(publicTypeNames, "ElementPlusCssStyleValue");
        CollectionAssert.DoesNotContain(publicTypeNames, "ElementPlusCssStyleValues");
        CollectionAssert.DoesNotContain(publicTypeNames, "ElementPlusStringNumberValue");
        CollectionAssert.DoesNotContain(publicTypeNames, "ElementPlusStringArray");
    }

    [TestMethod]
    public void ElementPlus_ImportHosts_UseExpectedEntrypoints()
    {
        Assert.AreEqual("npm:element-plus", GetEcmaScriptImport(typeof(ElementPlus)));
        Assert.AreEqual("element-plus", GetEcmaScriptImport(typeof(ElementPlusComponents)));
        Assert.AreEqual("element-plus", GetEcmaScriptImport(typeof(ElementPlusDirectives)));
    }

    private static string? GetEcmaScriptImport(Type type)
        => type.GetCustomAttributesData()
            .SingleOrDefault(static attribute => attribute.AttributeType == typeof(ECMAScriptAttribute))
            ?.ConstructorArguments
            .FirstOrDefault()
            .Value as string;
}
