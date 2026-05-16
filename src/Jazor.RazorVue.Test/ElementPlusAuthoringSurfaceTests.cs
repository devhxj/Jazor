using System.Reflection;
using System.Runtime.CompilerServices;
using ECMAScript;
using ECMAScript.ElementPlus;
using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;
using static ECMAScript.Vue3;

namespace Jazor.RazorVue.Test;

[TestClass]
public sealed class ElementPlusAuthoringSurfaceTests
{
    private static readonly Type AdditionalAttributesType = typeof(IReadOnlyDictionary<string, object?>);
    private static readonly string BannedChoiceName = "E" + "ither";
    private static readonly string BannedInterfaceChoiceName = "I" + BannedChoiceName;

    [TestMethod]
    public void ElementPlus_AuthoringComponents_MatchOfficialComponentExports()
    {
        var components = GetElementPlusAuthoringComponents();

        CollectionAssert.AreEquivalent(
            ElementPlusTestMetadata.StrongAuthoringComponentNames,
            components.Select(static type => type.Name).ToArray());

        CollectionAssert.AreEquivalent(
            ElementPlusTestMetadata.OfficialComponentExportNames,
            ElementPlusTestMetadata.RuntimeComponentExportNames);
    }

    [TestMethod]
    public void ElementPlus_AuthoringComponents_ExposeOnlyAdditionalAttributesAsObjectSink()
    {
        foreach (var component in GetElementPlusAuthoringComponents())
        {
            var additionalAttributes = component.GetProperty(
                nameof(ElementPlusComponentBase.AdditionalAttributes),
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
    public void ElementPlus_AuthoringComponents_ExposeExplicitCssPropsAsCssClassAndCssStyle()
    {
        foreach (var component in GetElementPlusAuthoringComponents())
        {
            var parameterNames = component
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                .Where(static property => property.GetCustomAttribute<ParameterAttribute>(inherit: true) is not null)
                .Select(static property => property.Name)
                .ToArray();

            CollectionAssert.DoesNotContain(parameterNames, "Class", component.FullName);
            CollectionAssert.DoesNotContain(parameterNames, "Style", component.FullName);

            AssertCssParameterMapping(
                component,
                nameof(ElementPlusComponentBase.CssClass),
                "class",
                typeof(VueClassValue?));
            AssertCssParameterMapping(
                component,
                nameof(ElementPlusComponentBase.CssStyle),
                "style",
                typeof(VueStyleValue?));
        }
    }

    [TestMethod]
    public void ElementPlus_PublicContracts_DoNotUseBannedChoiceWrappersOrRuntimeDispatch()
    {
        var publicTypes = typeof(ElementPlus).Assembly
            .GetExportedTypes()
            .Where(static type => type.Namespace == "ECMAScript.ElementPlus")
            .OrderBy(static type => type.FullName, StringComparer.Ordinal)
            .ToArray();

        foreach (var type in publicTypes)
        {
            AssertNoBannedChoiceContract(type, type.FullName ?? type.Name);
            AssertNoRuntimeDispatchContract(type, type.FullName ?? type.Name);

            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                AssertNoBannedChoiceContract(property.PropertyType, $"{type.FullName}.{property.Name}");
                AssertNoRuntimeDispatchContract(property, $"{type.FullName}.{property.Name}");
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
    public void ElementPlus_AuthoringComponents_UseExpectedPackageEntrypointsAndPluginContracts()
    {
        foreach (var component in GetElementPlusAuthoringComponents())
        {
            var libraryComponent = component.GetCustomAttribute<VueLibraryComponentAttribute>(inherit: false);
            Assert.IsNotNull(libraryComponent, component.FullName);
            Assert.AreEqual("element-plus", libraryComponent!.ImportSpecifier, component.FullName);

            var styleAttribute = component.GetCustomAttribute<VueLibraryStyleAttribute>(inherit: false);
            Assert.IsNotNull(styleAttribute, component.FullName);
            Assert.AreEqual("element-plus/dist/index.css", styleAttribute!.StyleSpecifier, component.FullName);

            var pluginRequirement = component.GetCustomAttribute<VueLibraryPluginRequirementAttribute>(inherit: false);
            Assert.IsNotNull(pluginRequirement, component.FullName);
            Assert.AreEqual("element-plus", pluginRequirement!.RequirementId, component.FullName);
        }
    }

    [TestMethod]
    public void ElementPlus_ModelBindingContracts_UseExplicitCanonicalNames()
    {
        AssertModelBindingContract(typeof(ElDialog), nameof(ElDialog.ModelValue), typeof(bool?));
        AssertModelBindingContract(typeof(ElCheckboxGroup), nameof(ElCheckboxGroup.ModelValue), typeof(VueStringNumberValue[]));
        AssertModelBindingContract(typeof(ElInput), nameof(ElInput.ModelValue), typeof(VueStringNumberValue?));
        AssertModelBindingContract(typeof(ElInputNumber), nameof(ElInputNumber.ModelValue), typeof(Number?));
        AssertModelBindingContract(typeof(ElInputOtp), nameof(ElInputOtp.ModelValue), typeof(VueStringNumberValue?));
        AssertModelBindingContract(typeof(ElSlider), nameof(ElSlider.ModelValue), typeof(VueNumberOrNumbersValue?));
        AssertModelBindingContract(typeof(ElSwitch), nameof(ElSwitch.ModelValue), typeof(VueBooleanStringNumberValue?));
    }

    [TestMethod]
    public void ElementPlus_AuthoringAliases_CanDifferFromRuntimePackageExports()
    {
        var componentType = typeof(ElVirtualizedSelect);
        var libraryComponent = componentType.GetCustomAttribute<VueLibraryComponentAttribute>(inherit: false);
        Assert.IsNotNull(libraryComponent, componentType.FullName);
        Assert.AreEqual("ElSelectV2", libraryComponent!.ExportName, componentType.FullName);

        var exportProperty = typeof(ElementPlusComponents).GetProperty(
            nameof(ElementPlusComponents.ElVirtualizedSelect),
            BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
        Assert.IsNotNull(exportProperty);

        var ecmaScriptName = exportProperty!
            .GetCustomAttributesData()
            .SingleOrDefault(static attribute => attribute.AttributeType == typeof(ECMAScriptNameAttribute));
        Assert.IsNotNull(ecmaScriptName);
        Assert.AreEqual("ElSelectV2", ecmaScriptName!.ConstructorArguments[0].Value as string);
    }

    [TestMethod]
    public void ElementPlus_InstallableBaselineComponents_ArePresentOnAuthoringSurface()
    {
        foreach (var componentName in new[] { "ElAutoResizer", "ElCollapseTransition", "ElPopper", "ElTreeSelect" })
        {
            var componentType = typeof(ElementPlus).Assembly
                .GetExportedTypes()
                .SingleOrDefault(type => type.Name == componentName);
            Assert.IsNotNull(componentType, componentName);
            Assert.IsTrue(typeof(IVueLibraryComponent).IsAssignableFrom(componentType!), componentName);

            var exportProperty = typeof(ElementPlusComponents).GetProperty(
                componentName,
                BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
            Assert.IsNotNull(exportProperty, componentName);
        }
    }

    [TestMethod]
    public void ElementPlus_DocumentSectionBracketMetadata_DoesNotLeakIntoAuthoringSurface()
    {
        Assert.IsNull(typeof(ElMention).GetProperty("InputSlots"), typeof(ElMention).FullName);
        Assert.IsNull(typeof(ElMention).GetProperty("OnInputEvents"), typeof(ElMention).FullName);
        Assert.IsNull(typeof(ElImage).GetProperty("ImageViewerSlots"), typeof(ElImage).FullName);
    }

    [TestMethod]
    public void ElementPlus_SlotContracts_RenameCollisionsPredictably()
    {
        AssertSlotContract(typeof(ElAlert), nameof(ElAlert.TitleSlot), "title");
        AssertSlotContract(typeof(ElButton), nameof(ElButton.LoadingSlot), "loading");
        AssertSlotContract(typeof(ElButton), nameof(ElButton.IconSlot), "icon");
        AssertSlotContract(typeof(ElCard), nameof(ElCard.HeaderSlot), "header");
        AssertSlotContract(typeof(ElCard), nameof(ElCard.FooterSlot), "footer");
        AssertSlotContract(typeof(ElDialog), nameof(ElDialog.TitleSlot), "title");
        AssertSlotContract(typeof(ElDatePicker), nameof(ElDatePicker.RangeSeparatorSlot), "range-separator");
        AssertSlotContract(typeof(ElDescriptions), nameof(ElDescriptions.TitleSlot), "title");
        AssertSlotContract(typeof(ElDescriptionsItem), nameof(ElDescriptionsItem.LabelSlot), "label");
    }

    [TestMethod]
    public void ElementPlus_DirectiveExports_MatchOfficialDirectiveSymbols()
    {
        var exportNames = typeof(ElementPlusDirectives)
            .GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Select(GetDirectiveExportName)
            .OrderBy(static name => name, StringComparer.Ordinal)
            .ToArray();

        CollectionAssert.AreEquivalent(
            ElementPlusTestMetadata.OfficialDirectiveExportNames,
            exportNames);
    }

    private static Type[] GetElementPlusAuthoringComponents()
        => typeof(ElementPlusComponents).Assembly
            .GetExportedTypes()
            .Where(static type => !type.IsAbstract && typeof(IVueLibraryComponent).IsAssignableFrom(type))
            .OrderBy(static type => type.Name, StringComparer.Ordinal)
            .ToArray();

    private static string GetDirectiveExportName(PropertyInfo property)
    {
        foreach (var attribute in property.CustomAttributes)
        {
            if (attribute.AttributeType.FullName != "ECMAScript.ECMAScriptNameAttribute")
                continue;

            if (attribute.ConstructorArguments.Count == 1 &&
                attribute.ConstructorArguments[0].ArgumentType == typeof(string) &&
                attribute.ConstructorArguments[0].Value is string explicitName &&
                !string.IsNullOrWhiteSpace(explicitName))
            {
                return explicitName;
            }
        }

        return property.Name;
    }

    private static void AssertCssParameterMapping(
        Type component,
        string propertyName,
        string runtimeName,
        Type expectedPropertyType)
    {
        var property = component.GetProperty(
            propertyName,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        Assert.IsNotNull(property, $"{component.FullName}.{propertyName}");
        Assert.AreEqual(expectedPropertyType, property!.PropertyType, $"{component.FullName}.{propertyName}");
        Assert.IsNotNull(property.GetCustomAttribute<ParameterAttribute>(inherit: true), $"{component.FullName}.{propertyName}");

        var mapping = component
            .GetCustomAttributes<VuePropAttribute>(inherit: false)
            .SingleOrDefault(attribute => attribute.PublicName == propertyName);
        Assert.IsNotNull(mapping, $"{component.FullName}.{propertyName}");
        Assert.AreEqual(runtimeName, mapping!.Name, $"{component.FullName}.{propertyName}");
    }

    private static void AssertModelBindingContract(Type component, string propertyName, Type expectedPropertyType)
    {
        var property = component.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        Assert.IsNotNull(property, $"{component.FullName}.{propertyName}");
        Assert.AreEqual(expectedPropertyType, property!.PropertyType, $"{component.FullName}.{propertyName}");

        var propMapping = component
            .GetCustomAttributes<VuePropAttribute>(inherit: false)
            .SingleOrDefault(attribute => attribute.PublicName == propertyName);
        Assert.IsNotNull(propMapping, $"{component.FullName}.{propertyName}");
        Assert.AreEqual(VuePropKind.Model, propMapping!.Kind, $"{component.FullName}.{propertyName}");
        Assert.IsTrue(propMapping.AcceptsBinding, $"{component.FullName}.{propertyName}");
        Assert.AreEqual(ToLowerCamelCase(propertyName), propMapping.Name, $"{component.FullName}.{propertyName}");

        var changedPropertyName = propertyName + "Changed";
        var changedProperty = component.GetProperty(changedPropertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        Assert.IsNotNull(changedProperty, $"{component.FullName}.{changedPropertyName}");

        var emitMapping = component
            .GetCustomAttributes<VueLibraryEmitAttribute>(inherit: false)
            .SingleOrDefault(attribute => attribute.RazorAlias == changedPropertyName);
        Assert.IsNotNull(emitMapping, $"{component.FullName}.{changedPropertyName}");
        Assert.AreEqual(VueEmitKind.ModelUpdate, emitMapping!.Kind, $"{component.FullName}.{changedPropertyName}");
        Assert.AreEqual("update:" + ToLowerCamelCase(propertyName), emitMapping.Name, $"{component.FullName}.{changedPropertyName}");
        Assert.AreEqual(expectedPropertyType.FullName, emitMapping.PayloadTypeName, $"{component.FullName}.{changedPropertyName}");
    }

    private static void AssertSlotContract(Type component, string propertyName, string runtimeName)
    {
        var property = component.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        Assert.IsNotNull(property, $"{component.FullName}.{propertyName}");
        Assert.AreEqual(typeof(RenderFragment), property!.PropertyType, $"{component.FullName}.{propertyName}");

        var slot = component
            .GetCustomAttributes<VueSlotAttribute>(inherit: false)
            .SingleOrDefault(attribute => attribute.PublicName == propertyName);
        Assert.IsNotNull(slot, $"{component.FullName}.{propertyName}");
        Assert.AreEqual(runtimeName, slot!.Name, $"{component.FullName}.{propertyName}");
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
        if (provider.IsDefined(typeof(DynamicAttribute), inherit: false))
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

    private static string ToLowerCamelCase(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        if (value.Length == 1)
            return char.ToLowerInvariant(value[0]).ToString();

        if (char.IsUpper(value[0]) && char.IsUpper(value[1]))
            return value;

        return char.ToLowerInvariant(value[0]) + value[1..];
    }
}
