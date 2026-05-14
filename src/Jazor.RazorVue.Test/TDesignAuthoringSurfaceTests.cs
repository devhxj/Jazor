using System.Reflection;
using System.Runtime.CompilerServices;
using ECMAScript;
using ECMAScript.TDesign;
using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;
using static ECMAScript.Vue3;

namespace Jazor.RazorVue.Test;

[TestClass]
public sealed class TDesignAuthoringSurfaceTests
{
    private static readonly Type AdditionalAttributesType = typeof(IReadOnlyDictionary<string, object?>);
    private static readonly string BannedChoiceName = "E" + "ither";
    private static readonly string BannedInterfaceChoiceName = "I" + BannedChoiceName;

    [TestMethod]
    public void TDesign_AuthoringComponents_ExposeOnlyAdditionalAttributesAsObjectSink()
    {
        var components = GetTDesignAuthoringComponents();

        CollectionAssert.AreEquivalent(
            TDesignTestMetadata.RuntimeComponentExportNames,
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
    public void TDesign_AuthoringComponents_ExposeExplicitCssPropsAsCssClassAndCssStyle()
    {
        foreach (var component in GetTDesignAuthoringComponents())
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
                "CssClass",
                "class",
                typeof(ECMAScript.Vue3.VueClassValue?));
            AssertCssParameterMapping(
                component,
                "CssStyle",
                "style",
                typeof(VueStyleValue?));
        }
    }

    [TestMethod]
    public void TDesign_PublicContracts_DoNotUseBannedChoiceWrappersOrRuntimeDispatch()
    {
        var publicTypes = typeof(TDesignComponents).Assembly
            .GetExportedTypes()
            .Where(static type => type.Namespace == "ECMAScript.TDesign")
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
    public void TDesign_ComponentExports_MatchLocalPackageEntrypoints()
    {
        var exportNames = ReadTDesignEntrypointExportNames(
            Path.Combine(".tmp", "tdesign-inspect", "tdesign-vue-next", "package", "lib", "components.js"));

        CollectionAssert.IsSubsetOf(
            new[]
            {
                "Button", "Breadcrumb", "BreadcrumbItem", "Layout", "Aside", "Header", "Content", "Footer",
                "Menu", "HeadMenu", "Submenu", "MenuItem", "MenuGroup", "Card", "Link", "Tabs", "TabPanel",
                "Avatar", "AvatarGroup", "Badge", "Space", "Divider", "ConfigProvider"
            },
            exportNames.ToArray());
    }

    [TestMethod]
    public void TDesign_AuthoringComponents_UseMatchingPackageEntrypoints()
    {
        var components = GetTDesignAuthoringComponents();
        foreach (var component in components)
        {
            var attribute = component.GetCustomAttribute<VueLibraryComponentAttribute>(inherit: false);
            Assert.IsNotNull(attribute, component.FullName);
            Assert.AreEqual("tdesign-vue-next", attribute!.ImportSpecifier, component.FullName);
        }
    }

    [TestMethod]
    public void TDesign_ButtonMenuAndSecondBatchContracts_MatchVerifiedSourceShapes()
    {
        Assert.AreEqual(typeof(TDesignButtonShape?), typeof(TButton).GetProperty(nameof(TButton.Shape))?.PropertyType);
        Assert.AreEqual(typeof(TDesignButtonTheme?), typeof(TButton).GetProperty(nameof(TButton.Theme))?.PropertyType);
        Assert.AreEqual(typeof(TDesignButtonVariant?), typeof(TButton).GetProperty(nameof(TButton.Variant))?.PropertyType);
        Assert.AreEqual(typeof(EventCallback<MouseEvent>), typeof(TButton).GetProperty(nameof(TButton.OnClick))?.PropertyType);
        Assert.AreEqual(typeof(TDesignMenuWidthValue?), typeof(TMenu).GetProperty(nameof(TMenu.Width))?.PropertyType);
        Assert.AreEqual(typeof(TDesignMenuValue?), typeof(TMenu).GetProperty(nameof(TMenu.Value))?.PropertyType);
        Assert.AreEqual(typeof(EventCallback<TDesignMenuValue>), typeof(TMenu).GetProperty(nameof(TMenu.OnChange))?.PropertyType);
        Assert.AreEqual(typeof(EventCallback<TDesignMenuValue[]>), typeof(TMenu).GetProperty(nameof(TMenu.OnExpand))?.PropertyType);
        Assert.AreEqual(typeof(TDesignMenuRouteTarget?), typeof(TMenuItem).GetProperty(nameof(TMenuItem.To))?.PropertyType);
        Assert.AreEqual(typeof(EventCallback<TDesignMenuItemClickContext>), typeof(TMenuItem).GetProperty(nameof(TMenuItem.OnClick))?.PropertyType);
        Assert.AreEqual(typeof(TDesignBreadcrumbTheme?), typeof(TBreadcrumb).GetProperty(nameof(TBreadcrumb.Theme))?.PropertyType);
        Assert.AreEqual(typeof(EventCallback<MouseEvent>), typeof(TBreadcrumbItem).GetProperty(nameof(TBreadcrumbItem.OnClick))?.PropertyType);
        Assert.AreEqual(typeof(TDesignLinkDownloadValue?), typeof(TLink).GetProperty(nameof(TLink.Download))?.PropertyType);
        Assert.AreEqual(typeof(TDesignLinkTheme?), typeof(TLink).GetProperty(nameof(TLink.Theme))?.PropertyType);
        Assert.AreEqual(typeof(EventCallback<MouseEvent>), typeof(TLink).GetProperty(nameof(TLink.OnClick))?.PropertyType);
        Assert.AreEqual(typeof(TDesignTabValue?), typeof(TTabs).GetProperty(nameof(TTabs.Value))?.PropertyType);
        Assert.AreEqual(typeof(EventCallback<TDesignTabValue>), typeof(TTabs).GetProperty(nameof(TTabs.ValueChanged))?.PropertyType);
        Assert.AreEqual(typeof(EventCallback<TDesignTabAddContext>), typeof(TTabs).GetProperty(nameof(TTabs.OnAdd))?.PropertyType);
        Assert.AreEqual(typeof(EventCallback<TDesignTabsDragSortContext>), typeof(TTabs).GetProperty(nameof(TTabs.OnDragSort))?.PropertyType);
        Assert.AreEqual(typeof(EventCallback<TDesignTabRemoveContext>), typeof(TTabs).GetProperty(nameof(TTabs.OnRemove))?.PropertyType);
        Assert.AreEqual(typeof(TDesignTabValue?), typeof(TTabPanel).GetProperty(nameof(TTabPanel.Value))?.PropertyType);
        Assert.AreEqual(typeof(EventCallback<TDesignTabPanelRemoveContext>), typeof(TTabPanel).GetProperty(nameof(TTabPanel.OnRemove))?.PropertyType);
        Assert.AreEqual(typeof(TDesignAvatarShape?), typeof(TAvatar).GetProperty(nameof(TAvatar.Shape))?.PropertyType);
        Assert.AreEqual(typeof(EventCallback<TDesignAvatarErrorContext>), typeof(TAvatar).GetProperty(nameof(TAvatar.OnError))?.PropertyType);
        Assert.AreEqual(typeof(TDesignAvatarGroupCascading?), typeof(TAvatarGroup).GetProperty(nameof(TAvatarGroup.Cascading))?.PropertyType);
        Assert.AreEqual(typeof(TDesignBadgeCountValue?), typeof(TBadge).GetProperty(nameof(TBadge.CountValue))?.PropertyType);
        Assert.AreEqual(typeof(TDesignBadgeOffset?), typeof(TBadge).GetProperty(nameof(TBadge.Offset))?.PropertyType);
        Assert.AreEqual(typeof(TDesignBadgeShape?), typeof(TBadge).GetProperty(nameof(TBadge.Shape))?.PropertyType);
    }

    private static Type[] GetTDesignAuthoringComponents()
        => typeof(TDesignComponents).Assembly
            .GetExportedTypes()
            .Where(static type => !type.IsAbstract && typeof(ECMAScript.Vue3.IVueLibraryComponent).IsAssignableFrom(type))
            .OrderBy(static type => type.Name, StringComparer.Ordinal)
            .ToArray();

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

    private static SortedSet<string> ReadTDesignEntrypointExportNames(string entryRelativePath)
    {
        var filePath = FindRepositoryFile(entryRelativePath);
        Assert.IsTrue(System.IO.File.Exists(filePath), filePath);

        var names = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var line in System.IO.File.ReadLines(filePath))
        {
            const string prefix = "export { ";
            if (!line.StartsWith(prefix, StringComparison.Ordinal))
                continue;

            var end = line.IndexOf(" }", StringComparison.Ordinal);
            Assert.IsTrue(end > prefix.Length, line);
            var exportList = line[prefix.Length..end]
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var exportName in exportList)
            {
                if (exportName.StartsWith("default as ", StringComparison.Ordinal))
                {
                    names.Add(exportName["default as ".Length..]);
                    continue;
                }

                var aliasIndex = exportName.IndexOf(" as ", StringComparison.Ordinal);
                names.Add(aliasIndex >= 0 ? exportName[..aliasIndex] : exportName);
            }
        }

        return names;
    }

    private static string FindRepositoryFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);
            if (System.IO.File.Exists(candidate))
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
}
