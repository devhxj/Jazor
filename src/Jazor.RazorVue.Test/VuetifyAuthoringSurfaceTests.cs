using System.Reflection;
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
    public void Vuetify_ComponentExports_MatchLocalVuetifyPackageEntrypoints()
    {
        var normalExports = ReadVuetifyNamedExports(
            Path.Combine("node_modules", "vuetify", "lib", "components", "index.d.ts"));
        var labsExports = ReadVuetifyNamedExports(
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

    private static SortedSet<string> ReadVuetifyNamedExports(string entryRelativePath)
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
