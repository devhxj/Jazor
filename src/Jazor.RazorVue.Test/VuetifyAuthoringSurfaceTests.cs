using System.Reflection;
using System.Runtime.CompilerServices;
using ECMAScript.VueContract;
using ECMAScript.Vuetify;
using Microsoft.AspNetCore.Components;

namespace Jazor.RazorVue.Test;

[TestClass]
public sealed class VuetifyAuthoringSurfaceTests
{
    private static readonly Type AdditionalAttributesType = typeof(IReadOnlyDictionary<string, object?>);
    private static readonly string BannedChoiceName = "E" + "ither";
    private static readonly string BannedInterfaceChoiceName = "I" + BannedChoiceName;
    private static readonly string[] NormalVuetifyComponentExportNames =
        GetRuntimeComponentExportNames(typeof(VuetifyComponents));
    private static readonly string[] LabsVuetifyComponentExportNames =
        GetRuntimeComponentExportNames(typeof(VuetifyLabsComponents));

    [TestMethod]
    public void Vuetify_AuthoringComponents_ExposeOnlyAdditionalAttributesAsObjectSink()
    {
        var components = GetVuetifyAuthoringComponents();

        CollectionAssert.AreEquivalent(
            VuetifyTestMetadata.RuntimeComponentExportNames,
            components.Select(static type => type.Name).ToArray());

        foreach (var component in components)
        {
            var additionalAttributes = component.GetProperty(
                "AdditionalAttributes",
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            Assert.IsNotNull(additionalAttributes, component.FullName);
            Assert.AreEqual(AdditionalAttributesType, additionalAttributes!.PropertyType, component.FullName);

            var parameter = additionalAttributes.GetCustomAttribute<ParameterAttribute>(inherit: true);
            Assert.IsNotNull(parameter, component.FullName);
            Assert.IsTrue(parameter!.CaptureUnmatchedValues, component.FullName);

            foreach (var property in component.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy))
            {
                if (property.Name == additionalAttributes.Name && property.PropertyType == additionalAttributes.PropertyType)
                    continue;

                AssertNoWeakObjectContract(property.PropertyType, $"{component.FullName}.{property.Name}");
                AssertNoRuntimeDispatchContract(property, $"{component.FullName}.{property.Name}");
            }
        }
    }

    [TestMethod]
    public void Vuetify_AuthoringComponents_ExposeExplicitCssPropsAsCssClassAndCssStyle()
    {
        foreach (var component in GetVuetifyAuthoringComponents())
        {
            var parameterNames = component
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                .Where(static property => property.GetCustomAttribute<ParameterAttribute>(inherit: true) is not null)
                .Select(static property => property.Name)
                .ToArray();

            CollectionAssert.DoesNotContain(parameterNames, "Class", component.FullName);
            CollectionAssert.DoesNotContain(parameterNames, "Style", component.FullName);

            var hasCssClassMapping = false;
            var hasCssStyleMapping = false;
            foreach (var mapping in component.GetCustomAttributes<VuePropAttribute>(inherit: false))
            {
                if (mapping.Name == "class")
                {
                    hasCssClassMapping = true;
                    Assert.AreEqual("CssClass", mapping.PublicName, component.FullName);
                }
                else if (mapping.Name == "style")
                {
                    hasCssStyleMapping = true;
                    Assert.AreEqual("CssStyle", mapping.PublicName, component.FullName);
                }
            }

            AssertCssParameterMapping(
                component,
                "CssClass",
                "class",
                typeof(ECMAScript.Vue3.VueClassValue?),
                hasCssClassMapping);
            AssertCssParameterMapping(
                component,
                "CssStyle",
                "style",
                typeof(VuetifyStyleValue?),
                hasCssStyleMapping);
        }
    }

    [TestMethod]
    public void Vuetify_PublicContracts_DoNotUseBannedChoiceWrappersOrRuntimeDispatch()
    {
        var publicTypes = GetVuetifyPublicTypes();

        foreach (var type in publicTypes)
        {
            AssertNoBannedChoiceContract(type, type.FullName ?? type.Name);
            AssertNoRuntimeDispatchContract(type, type.FullName ?? type.Name);

            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                AssertNoBannedChoiceContract(property.PropertyType, $"{type.FullName}.{property.Name}");
                AssertNoRuntimeDispatchContract(property, $"{type.FullName}.{property.Name}");
            }

            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                AssertNoBannedChoiceContract(field.FieldType, $"{type.FullName}.{field.Name}");
                AssertNoRuntimeDispatchContract(field, $"{type.FullName}.{field.Name}");
            }

            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                if (method.IsSpecialName)
                    continue;

                AssertNoBannedChoiceContract(method.ReturnType, $"{type.FullName}.{method.Name} return");
                AssertNoRuntimeDispatchContract(method.ReturnParameter, $"{type.FullName}.{method.Name} return");

                foreach (var parameter in method.GetParameters())
                {
                    AssertNoBannedChoiceContract(parameter.ParameterType, $"{type.FullName}.{method.Name} parameter {parameter.Name}");
                    AssertNoRuntimeDispatchContract(parameter, $"{type.FullName}.{method.Name} parameter {parameter.Name}");
                }
            }
        }
    }

    [TestMethod]
    public void Vuetify_ImgSourceObject_MatchesVuetifySourceContract()
    {
        var aspect = typeof(VImgSourceObject).GetProperty(nameof(VImgSourceObject.Aspect));

        Assert.IsNotNull(aspect);
        Assert.AreEqual(typeof(ECMAScript.Number), aspect!.PropertyType);
        Assert.IsTrue(aspect.IsDefined(typeof(RequiredMemberAttribute), inherit: false));
    }

    [TestMethod]
    public void Vuetify_AvatarAndBadge_MatchVuetifySourceContracts()
    {
        Assert.AreEqual(typeof(VuetifyIconValue?), typeof(VAvatar).GetProperty(nameof(VAvatar.Icon))?.PropertyType);
        Assert.AreEqual(typeof(VuetifyBorderValue?), typeof(VAvatar).GetProperty(nameof(VAvatar.Border))?.PropertyType);
        Assert.AreEqual(typeof(ECMAScript.Vue3.VueClassValue?), typeof(VAvatar).GetProperty(nameof(VAvatar.CssClass))?.PropertyType);
        Assert.AreEqual(typeof(VuetifyStyleValue?), typeof(VAvatar).GetProperty(nameof(VAvatar.CssStyle))?.PropertyType);

        Assert.AreEqual(typeof(VuetifyTransitionValue?), typeof(VBadge).GetProperty(nameof(VBadge.Transition))?.PropertyType);
        Assert.AreEqual(typeof(VuetifyLocation?), typeof(VBadge).GetProperty(nameof(VBadge.Location))?.PropertyType);
        Assert.AreEqual(typeof(VuetifyIconValue?), typeof(VBadge).GetProperty(nameof(VBadge.Icon))?.PropertyType);
        Assert.AreEqual(typeof(RenderFragment), typeof(VBadge).GetProperty(nameof(VBadge.BadgeContent))?.PropertyType);
    }

    [TestMethod]
    public void Vuetify_Form_MatchesVuetifySourceContract()
    {
        Assert.AreEqual(typeof(ECMAScript.Vue3.VueClassValue?), typeof(VForm).GetProperty(nameof(VForm.CssClass))?.PropertyType);
        Assert.AreEqual(typeof(VuetifyStyleValue?), typeof(VForm).GetProperty(nameof(VForm.CssStyle))?.PropertyType);
        Assert.AreEqual(typeof(EventCallback<VFormSubmitEvent>), typeof(VForm).GetProperty(nameof(VForm.Submit))?.PropertyType);
        Assert.AreEqual(typeof(RenderFragment<VFormDefaultSlotContext>), typeof(VForm).GetProperty(nameof(VForm.ChildContent))?.PropertyType);
        Assert.IsTrue(typeof(ECMAScript.SubmitEvent).IsAssignableFrom(typeof(VFormSubmitEvent)));
        Assert.IsTrue(typeof(ECMAScript.IPromise<VuetifyFormValidationResult>).IsAssignableFrom(typeof(VFormSubmitEvent)));

        Assert.AreEqual(typeof(ECMAScript.Vue3.IVueRef<VuetifyFormFieldValidationResult[]>), typeof(VFormDefaultSlotContext).GetProperty(nameof(VFormDefaultSlotContext.Errors))?.PropertyType);
        Assert.AreEqual(typeof(ECMAScript.Vue3.VueComputedRef<bool>), typeof(VFormDefaultSlotContext).GetProperty(nameof(VFormDefaultSlotContext.IsDisabled))?.PropertyType);
        Assert.AreEqual(typeof(ECMAScript.Vue3.VueComputedRef<bool>), typeof(VFormDefaultSlotContext).GetProperty(nameof(VFormDefaultSlotContext.IsReadonly))?.PropertyType);
        Assert.AreEqual(typeof(ECMAScript.Vue3.VueShallowRef<bool>), typeof(VFormDefaultSlotContext).GetProperty(nameof(VFormDefaultSlotContext.IsValidating))?.PropertyType);
        Assert.AreEqual(typeof(ECMAScript.Vue3.IVueRef<bool?>), typeof(VFormDefaultSlotContext).GetProperty(nameof(VFormDefaultSlotContext.IsValid))?.PropertyType);
        Assert.AreEqual(typeof(ECMAScript.Vue3.IVueRef<VuetifyFormField[]>), typeof(VFormDefaultSlotContext).GetProperty(nameof(VFormDefaultSlotContext.Items))?.PropertyType);
        Assert.AreEqual(typeof(VuetifyFormValidateCallback), typeof(VFormDefaultSlotContext).GetProperty(nameof(VFormDefaultSlotContext.Validate))?.PropertyType);
        Assert.AreEqual(typeof(Action), typeof(VFormDefaultSlotContext).GetProperty(nameof(VFormDefaultSlotContext.Reset))?.PropertyType);
        Assert.AreEqual(typeof(Action), typeof(VFormDefaultSlotContext).GetProperty(nameof(VFormDefaultSlotContext.ResetValidation))?.PropertyType);

        Assert.AreEqual(typeof(ECMAScript.Vue3.VueStringNumberValue?), typeof(VuetifyFormField).GetProperty(nameof(VuetifyFormField.Id))?.PropertyType);
        Assert.AreEqual(typeof(VuetifyFormFieldValidateCallback), typeof(VuetifyFormField).GetProperty(nameof(VuetifyFormField.Validate))?.PropertyType);
        Assert.AreEqual(typeof(VuetifyFormFieldResetCallback), typeof(VuetifyFormField).GetProperty(nameof(VuetifyFormField.Reset))?.PropertyType);
        Assert.AreEqual(typeof(VuetifyFormFieldResetCallback), typeof(VuetifyFormField).GetProperty(nameof(VuetifyFormField.ResetValidation))?.PropertyType);
        Assert.AreEqual(typeof(bool?), typeof(VuetifyFormField).GetProperty(nameof(VuetifyFormField.IsValid))?.PropertyType);
        Assert.AreEqual(typeof(string[]), typeof(VuetifyFormField).GetProperty(nameof(VuetifyFormField.ErrorMessages))?.PropertyType);
    }

    [TestMethod]
    public void Vuetify_ProgressComponents_MatchVuetifySourceContracts()
    {
        Assert.AreEqual(typeof(ECMAScript.Vue3.VueClassValue?), typeof(VProgressCircular).GetProperty(nameof(VProgressCircular.CssClass))?.PropertyType);
        Assert.AreEqual(typeof(VuetifyStyleValue?), typeof(VProgressCircular).GetProperty(nameof(VProgressCircular.CssStyle))?.PropertyType);
        Assert.AreEqual(typeof(RenderFragment<VProgressCircularDefaultSlotContext>), typeof(VProgressCircular).GetProperty(nameof(VProgressCircular.ChildContent))?.PropertyType);
        Assert.AreEqual(typeof(ECMAScript.Number), typeof(VProgressCircularDefaultSlotContext).GetProperty(nameof(VProgressCircularDefaultSlotContext.Value))?.PropertyType);

        Assert.AreEqual(typeof(VuetifyRoundedValue?), typeof(VProgressLinear).GetProperty(nameof(VProgressLinear.Rounded))?.PropertyType);
        Assert.AreEqual(typeof(VuetifyLocation?), typeof(VProgressLinear).GetProperty(nameof(VProgressLinear.Location))?.PropertyType);
        Assert.AreEqual(typeof(EventCallback<ECMAScript.Number>), typeof(VProgressLinear).GetProperty(nameof(VProgressLinear.ModelValueChanged))?.PropertyType);
        Assert.AreEqual(typeof(RenderFragment<VProgressLinearDefaultSlotContext>), typeof(VProgressLinear).GetProperty(nameof(VProgressLinear.ChildContent))?.PropertyType);
        Assert.AreEqual(typeof(ECMAScript.Number), typeof(VProgressLinearDefaultSlotContext).GetProperty(nameof(VProgressLinearDefaultSlotContext.Value))?.PropertyType);
        Assert.AreEqual(typeof(ECMAScript.Number), typeof(VProgressLinearDefaultSlotContext).GetProperty(nameof(VProgressLinearDefaultSlotContext.Buffer))?.PropertyType);
    }

    [TestMethod]
    public void Vuetify_Sheet_MatchesVuetifySourceContract()
    {
        Assert.AreEqual(typeof(VuetifyRoundedValue?), typeof(VSheet).GetProperty(nameof(VSheet.Rounded))?.PropertyType);
        Assert.AreEqual(typeof(VuetifyPosition?), typeof(VSheet).GetProperty(nameof(VSheet.Position))?.PropertyType);
        Assert.AreEqual(typeof(VuetifyLocation?), typeof(VSheet).GetProperty(nameof(VSheet.Location))?.PropertyType);
        Assert.AreEqual(typeof(ECMAScript.Vue3.VueStringNumberValue?), typeof(VSheet).GetProperty(nameof(VSheet.Elevation))?.PropertyType);
        Assert.AreEqual(typeof(ECMAScript.Vue3.VueClassValue?), typeof(VSheet).GetProperty(nameof(VSheet.CssClass))?.PropertyType);
        Assert.AreEqual(typeof(VuetifyStyleValue?), typeof(VSheet).GetProperty(nameof(VSheet.CssStyle))?.PropertyType);
        Assert.AreEqual(typeof(VuetifyBorderValue?), typeof(VSheet).GetProperty(nameof(VSheet.Border))?.PropertyType);
        Assert.AreEqual(typeof(RenderFragment), typeof(VSheet).GetProperty(nameof(VSheet.ChildContent))?.PropertyType);
    }

    [TestMethod]
    public void Vuetify_Icon_MatchesVuetifySourceContract()
    {
        Assert.AreEqual(typeof(VuetifyIconValue?), typeof(VIcon).GetProperty(nameof(VIcon.Icon))?.PropertyType);
        Assert.AreEqual(typeof(ECMAScript.Vue3.VueStringNumberValue?), typeof(VIcon).GetProperty(nameof(VIcon.Size))?.PropertyType);
        Assert.AreEqual(typeof(ECMAScript.Vue3.VueStringNumberValue?), typeof(VIcon).GetProperty(nameof(VIcon.Opacity))?.PropertyType);
        Assert.AreEqual(typeof(ECMAScript.Vue3.VueClassValue?), typeof(VIcon).GetProperty(nameof(VIcon.CssClass))?.PropertyType);
        Assert.AreEqual(typeof(VuetifyStyleValue?), typeof(VIcon).GetProperty(nameof(VIcon.CssStyle))?.PropertyType);
        Assert.AreEqual(typeof(RenderFragment), typeof(VIcon).GetProperty(nameof(VIcon.ChildContent))?.PropertyType);
    }

    [TestMethod]
    public void Vuetify_Toolbar_MatchesVuetifySourceContract()
    {
        Assert.AreEqual(typeof(VuetifyRoundedValue?), typeof(VToolbar).GetProperty(nameof(VToolbar.Rounded))?.PropertyType);
        Assert.AreEqual(typeof(ECMAScript.Vue3.VueStringNumberValue?), typeof(VToolbar).GetProperty(nameof(VToolbar.Elevation))?.PropertyType);
        Assert.AreEqual(typeof(ECMAScript.Vue3.VueClassValue?), typeof(VToolbar).GetProperty(nameof(VToolbar.CssClass))?.PropertyType);
        Assert.AreEqual(typeof(VuetifyStyleValue?), typeof(VToolbar).GetProperty(nameof(VToolbar.CssStyle))?.PropertyType);
        Assert.AreEqual(typeof(VuetifyBorderValue?), typeof(VToolbar).GetProperty(nameof(VToolbar.Border))?.PropertyType);
        Assert.AreEqual(typeof(VuetifyToolbarDensityValue?), typeof(VToolbar).GetProperty(nameof(VToolbar.Density))?.PropertyType);
        Assert.AreEqual(typeof(ECMAScript.Vue3.VueStringNumberValue?), typeof(VToolbar).GetProperty(nameof(VToolbar.ExtensionHeight))?.PropertyType);
        Assert.AreEqual(typeof(ECMAScript.Vue3.VueStringNumberValue?), typeof(VToolbar).GetProperty(nameof(VToolbar.Height))?.PropertyType);
        Assert.AreEqual(typeof(RenderFragment), typeof(VToolbar).GetProperty(nameof(VToolbar.ImageContent))?.PropertyType);
        Assert.AreEqual(typeof(RenderFragment), typeof(VToolbar).GetProperty(nameof(VToolbar.Prepend))?.PropertyType);
        Assert.AreEqual(typeof(RenderFragment), typeof(VToolbar).GetProperty(nameof(VToolbar.Append))?.PropertyType);
        Assert.AreEqual(typeof(RenderFragment), typeof(VToolbar).GetProperty(nameof(VToolbar.TitleContent))?.PropertyType);
        Assert.AreEqual(typeof(RenderFragment), typeof(VToolbar).GetProperty(nameof(VToolbar.Extension))?.PropertyType);
    }

    [TestMethod]
    public void Vuetify_ToolbarFamily_MatchesVuetifySourceContracts()
    {
        Assert.AreEqual(typeof(ECMAScript.Vue3.VueClassValue?), typeof(VToolbarItems).GetProperty(nameof(VToolbarItems.CssClass))?.PropertyType);
        Assert.AreEqual(typeof(VuetifyStyleValue?), typeof(VToolbarItems).GetProperty(nameof(VToolbarItems.CssStyle))?.PropertyType);
        Assert.AreEqual(typeof(RenderFragment), typeof(VToolbarItems).GetProperty(nameof(VToolbarItems.ChildContent))?.PropertyType);

        Assert.AreEqual(typeof(ECMAScript.Vue3.VueClassValue?), typeof(VToolbarTitle).GetProperty(nameof(VToolbarTitle.CssClass))?.PropertyType);
        Assert.AreEqual(typeof(VuetifyStyleValue?), typeof(VToolbarTitle).GetProperty(nameof(VToolbarTitle.CssStyle))?.PropertyType);
        Assert.AreEqual(typeof(RenderFragment), typeof(VToolbarTitle).GetProperty(nameof(VToolbarTitle.ChildContent))?.PropertyType);
        Assert.AreEqual(typeof(RenderFragment), typeof(VToolbarTitle).GetProperty(nameof(VToolbarTitle.TextContent))?.PropertyType);
    }

    [TestMethod]
    public void Vuetify_GridFamily_MatchesVuetifySourceContracts()
    {
        Assert.AreEqual(typeof(ECMAScript.Vue3.VueStringNumberValue?), typeof(VContainer).GetProperty(nameof(VContainer.Height))?.PropertyType);
        Assert.AreEqual(typeof(ECMAScript.Vue3.VueClassValue?), typeof(VContainer).GetProperty(nameof(VContainer.CssClass))?.PropertyType);
        Assert.AreEqual(typeof(VuetifyStyleValue?), typeof(VContainer).GetProperty(nameof(VContainer.CssStyle))?.PropertyType);

        Assert.AreEqual(typeof(ECMAScript.Vue3.VueClassValue?), typeof(VRow).GetProperty(nameof(VRow.CssClass))?.PropertyType);
        Assert.AreEqual(typeof(VuetifyStyleValue?), typeof(VRow).GetProperty(nameof(VRow.CssStyle))?.PropertyType);
        Assert.AreEqual(typeof(string), typeof(VRow).GetProperty(nameof(VRow.Align))?.PropertyType);
        Assert.AreEqual(typeof(bool), typeof(VRow).GetProperty(nameof(VRow.NoGutters))?.PropertyType);

        Assert.AreEqual(typeof(VuetifyGridSpanValue?), typeof(VCol).GetProperty(nameof(VCol.Cols))?.PropertyType);
        Assert.AreEqual(typeof(VuetifyGridSpanValue?), typeof(VCol).GetProperty(nameof(VCol.Md))?.PropertyType);
        Assert.AreEqual(typeof(ECMAScript.Vue3.VueStringNumberValue?), typeof(VCol).GetProperty(nameof(VCol.Order))?.PropertyType);
        Assert.AreEqual(typeof(ECMAScript.Vue3.VueStringNumberValue?), typeof(VCol).GetProperty(nameof(VCol.OffsetMd))?.PropertyType);
        Assert.AreEqual(typeof(ECMAScript.Vue3.VueClassValue?), typeof(VCol).GetProperty(nameof(VCol.CssClass))?.PropertyType);
        Assert.AreEqual(typeof(VuetifyStyleValue?), typeof(VCol).GetProperty(nameof(VCol.CssStyle))?.PropertyType);

        Assert.AreEqual(typeof(ECMAScript.Vue3.VueClassValue?), typeof(VSpacer).GetProperty(nameof(VSpacer.CssClass))?.PropertyType);
        Assert.AreEqual(typeof(VuetifyStyleValue?), typeof(VSpacer).GetProperty(nameof(VSpacer.CssStyle))?.PropertyType);
        Assert.AreEqual(typeof(RenderFragment), typeof(VSpacer).GetProperty(nameof(VSpacer.ChildContent))?.PropertyType);
    }

    [TestMethod]
    public void Vuetify_ComponentExports_MatchLocalVuetifyPackageEntrypoints()
    {
        var normalExports = ReadVuetifyNamedExportsRecursively(
            Path.Combine("node_modules", "vuetify", "lib", "components", "index.d.ts"));
        var labsExports = ReadVuetifyEntrypointExportNames(
            Path.Combine("node_modules", "vuetify", "lib", "labs", "components.d.ts"));

        CollectionAssert.IsSubsetOf(NormalVuetifyComponentExportNames, normalExports.ToArray());
        CollectionAssert.IsSubsetOf(LabsVuetifyComponentExportNames, labsExports.ToArray());
        CollectionAssert.DoesNotContain(NormalVuetifyComponentExportNames, "VCalendar");
        CollectionAssert.DoesNotContain(NormalVuetifyComponentExportNames, "VTimePicker");
        CollectionAssert.DoesNotContain(NormalVuetifyComponentExportNames, "VTreeview");
        CollectionAssert.DoesNotContain(VuetifyTestMetadata.RuntimeComponentExportNames, "VHotkey");
    }

    [TestMethod]
    public void Vuetify_AuthoringComponents_UseMatchingPackageEntrypoints()
    {
        var components = GetVuetifyAuthoringComponents();
        var importSpecifiers = components.ToDictionary(
            static type => type.Name,
            static type => type.GetCustomAttribute<VueLibraryComponentAttribute>()?.ImportSpecifier,
            StringComparer.Ordinal);

        foreach (var componentName in NormalVuetifyComponentExportNames)
            Assert.AreEqual("vuetify/components", importSpecifiers[componentName], componentName);

        foreach (var componentName in LabsVuetifyComponentExportNames)
            Assert.AreEqual("vuetify/labs/components", importSpecifiers[componentName], componentName);
    }

    private static Type[] GetVuetifyAuthoringComponents()
        => typeof(VuetifyComponents).Assembly
            .GetExportedTypes()
            .Where(static type => !type.IsAbstract && typeof(ECMAScript.Vue3.IVueLibraryComponent).IsAssignableFrom(type))
            .OrderBy(static type => type.Name, StringComparer.Ordinal)
            .ToArray();

    private static Type[] GetVuetifyPublicTypes()
        => typeof(VuetifyComponents).Assembly
            .GetExportedTypes()
            .Where(static type => type.Namespace == "ECMAScript.Vuetify")
            .OrderBy(static type => type.FullName, StringComparer.Ordinal)
            .ToArray();

    private static string[] GetRuntimeComponentExportNames(Type exportHost)
        => exportHost
            .GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(static property => property.PropertyType == typeof(IVuetifyComponent))
            .Select(static property => property.Name)
            .OrderBy(static name => name, StringComparer.Ordinal)
            .ToArray();

    private static void AssertCssParameterMapping(
        Type component,
        string propertyName,
        string runtimeName,
        Type expectedPropertyType,
        bool hasMapping)
    {
        var property = component.GetProperty(
            propertyName,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        if (property is null && !hasMapping)
            return;

        Assert.IsNotNull(property, $"{component.FullName}.{propertyName}");
        Assert.AreEqual(expectedPropertyType, property.PropertyType, $"{component.FullName}.{propertyName}");
        Assert.IsNotNull(
            property.GetCustomAttribute<ParameterAttribute>(inherit: true),
            $"{component.FullName}.{propertyName}");

        var mapping = component
            .GetCustomAttributes<VuePropAttribute>(inherit: false)
            .SingleOrDefault(attribute => attribute.PublicName == propertyName);
        Assert.IsNotNull(mapping, $"{component.FullName}.{propertyName}");
        Assert.IsTrue(hasMapping, $"{component.FullName}.{propertyName}");
        Assert.AreEqual(runtimeName, mapping!.Name, $"{component.FullName}.{propertyName}");
    }

    private static SortedSet<string> ReadVuetifyEntrypointExportNames(string entryRelativePath)
    {
        var filePath = FindRepositoryFile(entryRelativePath);
        Assert.IsTrue(File.Exists(filePath), filePath);

        var names = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var line in File.ReadLines(filePath))
        {
            if (TryReadTopLevelReExportName(line, out var exportName))
                names.Add(exportName);
        }

        return names;
    }

    private static SortedSet<string> ReadVuetifyNamedExportsRecursively(string entryRelativePath)
    {
        var names = new SortedSet<string>(StringComparer.Ordinal);
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        Visit(FindRepositoryFile(entryRelativePath));
        return names;

        void Visit(string filePath)
        {
            if (!visited.Add(filePath))
                return;

            Assert.IsTrue(File.Exists(filePath), filePath);
            foreach (var line in File.ReadLines(filePath))
            {
                if (line.StartsWith("export * from './", StringComparison.Ordinal))
                {
                    var relativeSpecifier = line["export * from ".Length..^1].Trim('\'', '"');
                    var relativePath = relativeSpecifier.EndsWith(".js", StringComparison.Ordinal)
                        ? relativeSpecifier[..^3] + ".d.ts"
                        : relativeSpecifier + ".d.ts";
                    Visit(Path.GetFullPath(Path.Combine(Path.GetDirectoryName(filePath)!, relativePath)));
                    continue;
                }

                if (line.StartsWith("export declare const ", StringComparison.Ordinal))
                {
                    var exportName = line["export declare const ".Length..];
                    var delimiter = exportName.IndexOfAny([':', ' ', '<']);
                    names.Add(delimiter >= 0 ? exportName[..delimiter] : exportName);
                    continue;
                }

                if (!line.StartsWith("export { ", StringComparison.Ordinal))
                    continue;

                var end = line.IndexOf(" }", StringComparison.Ordinal);
                Assert.IsTrue(end > "export { ".Length, line);
                var exportList = line["export { ".Length..end]
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                foreach (var exportName in exportList)
                {
                    if (exportName.StartsWith("type ", StringComparison.Ordinal))
                        continue;

                    var aliasIndex = exportName.IndexOf(" as ", StringComparison.Ordinal);
                    names.Add(aliasIndex >= 0 ? exportName[(aliasIndex + " as ".Length)..] : exportName);
                }
            }
        }
    }

    private static bool TryReadTopLevelReExportName(string line, out string exportName)
    {
        const string prefix = "export * from './";
        exportName = string.Empty;
        if (!line.StartsWith(prefix, StringComparison.Ordinal))
            return false;

        var start = prefix.Length;
        var end = line.IndexOf('/', start);
        if (end <= start)
        {
            end = line.LastIndexOf('/', line.Length - ".js';".Length - 1);
            if (end <= start)
                end = line.IndexOf(".js", start, StringComparison.Ordinal);
        }

        if (end <= start)
            return false;

        exportName = line[start..end];
        return !string.IsNullOrWhiteSpace(exportName);
    }

    private static string FindRepositoryFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate))
                return candidate;

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not locate repository file: " + relativePath);
    }

    private static void AssertNoWeakObjectContract(Type type, string memberName)
    {
        if (UsesPlainObject(type))
            Assert.Fail($"{memberName} exposes '{type}' outside AdditionalAttributes.");
    }

    private static void AssertNoBannedChoiceContract(Type type, string memberName)
    {
        if (UsesBannedChoiceWrapper(type))
            Assert.Fail($"{memberName} exposes '{type}' with banned choice-wrapper naming.");
    }

    private static void AssertNoRuntimeDispatchContract(ICustomAttributeProvider provider, string memberName)
    {
        if (provider.IsDefined(typeof(System.Runtime.CompilerServices.DynamicAttribute), inherit: false))
            Assert.Fail($"{memberName} exposes a runtime-dispatch marker.");
    }

    private static bool UsesPlainObject(Type type)
    {
        if (type == typeof(object))
            return true;

        if (type.IsArray)
            return UsesPlainObject(type.GetElementType()!);

        if (type.IsGenericType)
            return type.GetGenericArguments().Any(UsesPlainObject);

        return type.HasElementType && UsesPlainObject(type.GetElementType()!);
    }

    private static bool UsesBannedChoiceWrapper(Type type)
    {
        if (type.Name.Contains(BannedChoiceName, StringComparison.Ordinal) ||
            type.Name.Contains(BannedInterfaceChoiceName, StringComparison.Ordinal) ||
            (type.FullName?.Contains(BannedChoiceName, StringComparison.Ordinal) ?? false) ||
            (type.FullName?.Contains(BannedInterfaceChoiceName, StringComparison.Ordinal) ?? false))
        {
            return true;
        }

        if (type.IsArray)
            return UsesBannedChoiceWrapper(type.GetElementType()!);

        if (type.IsGenericType)
            return type.GetGenericArguments().Any(UsesBannedChoiceWrapper);

        return type.HasElementType && UsesBannedChoiceWrapper(type.GetElementType()!);
    }
}
